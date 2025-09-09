using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web;
using NLog;

namespace Bookstore.WebForms.StateManagement
{
    /// <summary>
    /// Performance monitoring for state management operations
    /// </summary>
    public static class StateManagementPerformanceMonitor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Dictionary<string, PerformanceCounter> _performanceCounters = new Dictionary<string, PerformanceCounter>();
        private static readonly object _lockObject = new object();

        // Performance thresholds (in milliseconds)
        private const int SESSION_OPERATION_THRESHOLD = 50;
        private const int CACHE_OPERATION_THRESHOLD = 10;
        private const int VIEWSTATE_OPTIMIZATION_THRESHOLD = 100;

        /// <summary>
        /// Monitors session operation performance
        /// </summary>
        /// <param name="operation">The operation being performed</param>
        /// <param name="action">The action to monitor</param>
        /// <returns>The result of the action</returns>
        public static T MonitorSessionOperation<T>(string operation, Func<T> action)
        {
            var stopwatch = Stopwatch.StartNew();
            var sessionId = GetCurrentSessionId();
            
            try
            {
                var result = action();
                stopwatch.Stop();
                
                LogPerformanceMetric("Session", operation, stopwatch.ElapsedMilliseconds, SESSION_OPERATION_THRESHOLD, sessionId);
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Logger.Error(ex, "Error during session operation: {Operation}, Duration: {Duration}ms, SessionId: {SessionId}", 
                    operation, stopwatch.ElapsedMilliseconds, sessionId);
                throw;
            }
        }

        /// <summary>
        /// Monitors session operation performance (void operations)
        /// </summary>
        /// <param name="operation">The operation being performed</param>
        /// <param name="action">The action to monitor</param>
        public static void MonitorSessionOperation(string operation, Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            var sessionId = GetCurrentSessionId();
            
            try
            {
                action();
                stopwatch.Stop();
                
                LogPerformanceMetric("Session", operation, stopwatch.ElapsedMilliseconds, SESSION_OPERATION_THRESHOLD, sessionId);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Logger.Error(ex, "Error during session operation: {Operation}, Duration: {Duration}ms, SessionId: {SessionId}", 
                    operation, stopwatch.ElapsedMilliseconds, sessionId);
                throw;
            }
        }

        /// <summary>
        /// Monitors cache operation performance
        /// </summary>
        /// <param name="operation">The operation being performed</param>
        /// <param name="cacheKey">The cache key being accessed</param>
        /// <param name="action">The action to monitor</param>
        /// <returns>The result of the action</returns>
        public static T MonitorCacheOperation<T>(string operation, string cacheKey, Func<T> action)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var result = action();
                stopwatch.Stop();
                
                LogPerformanceMetric("Cache", operation, stopwatch.ElapsedMilliseconds, CACHE_OPERATION_THRESHOLD, cacheKey);
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Logger.Error(ex, "Error during cache operation: {Operation}, Duration: {Duration}ms, CacheKey: {CacheKey}", 
                    operation, stopwatch.ElapsedMilliseconds, cacheKey);
                throw;
            }
        }

        /// <summary>
        /// Monitors cache operation performance (void operations)
        /// </summary>
        /// <param name="operation">The operation being performed</param>
        /// <param name="cacheKey">The cache key being accessed</param>
        /// <param name="action">The action to monitor</param>
        public static void MonitorCacheOperation(string operation, string cacheKey, Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                action();
                stopwatch.Stop();
                
                LogPerformanceMetric("Cache", operation, stopwatch.ElapsedMilliseconds, CACHE_OPERATION_THRESHOLD, cacheKey);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Logger.Error(ex, "Error during cache operation: {Operation}, Duration: {Duration}ms, CacheKey: {CacheKey}", 
                    operation, stopwatch.ElapsedMilliseconds, cacheKey);
                throw;
            }
        }

        /// <summary>
        /// Monitors ViewState optimization performance
        /// </summary>
        /// <param name="pageType">The type of page being optimized</param>
        /// <param name="action">The optimization action</param>
        public static void MonitorViewStateOptimization(string pageType, Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                action();
                stopwatch.Stop();
                
                LogPerformanceMetric("ViewState", "Optimization", stopwatch.ElapsedMilliseconds, VIEWSTATE_OPTIMIZATION_THRESHOLD, pageType);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Logger.Error(ex, "Error during ViewState optimization: PageType: {PageType}, Duration: {Duration}ms", 
                    pageType, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        /// <summary>
        /// Gets performance statistics for state management operations
        /// </summary>
        /// <returns>Performance statistics</returns>
        public static StateManagementPerformanceStats GetPerformanceStats()
        {
            try
            {
                var stats = new StateManagementPerformanceStats();
                
                // Get session statistics
                var sessionStats = GetSessionPerformanceStats();
                stats.SessionOperations = sessionStats;
                
                // Get cache statistics
                var cacheStats = GetCachePerformanceStats();
                stats.CacheOperations = cacheStats;
                
                // Get ViewState statistics
                var viewStateStats = GetViewStatePerformanceStats();
                stats.ViewStateOperations = viewStateStats;
                
                return stats;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error getting performance statistics");
                return new StateManagementPerformanceStats();
            }
        }

        /// <summary>
        /// Logs performance metrics
        /// </summary>
        private static void LogPerformanceMetric(string category, string operation, long durationMs, int threshold, string context)
        {
            try
            {
                if (durationMs > threshold)
                {
                    Logger.Warn("Slow {Category} operation: {Operation}, Duration: {Duration}ms, Context: {Context}, Threshold: {Threshold}ms", 
                        category, operation, durationMs, context, threshold);
                }
                else
                {
                    Logger.Debug("{Category} operation: {Operation}, Duration: {Duration}ms, Context: {Context}", 
                        category, operation, durationMs, context);
                }

                // Update performance counters
                UpdatePerformanceCounter(string.Format("{0}_{1}", category, operation), durationMs);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error logging performance metric");
            }
        }

        /// <summary>
        /// Updates performance counters
        /// </summary>
        private static void UpdatePerformanceCounter(string counterName, long value)
        {
            try
            {
                lock (_lockObject)
                {
                    if (!_performanceCounters.ContainsKey(counterName))
                    {
                        // In a real implementation, you might create actual Windows performance counters
                        // For now, we'll just track the concept
                        _performanceCounters[counterName] = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error updating performance counter: {CounterName}", counterName);
            }
        }

        /// <summary>
        /// Gets current session ID
        /// </summary>
        private static string GetCurrentSessionId()
        {
            try
            {
                return HttpContext.Current != null && HttpContext.Current.Session != null ? HttpContext.Current.Session.SessionID : "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Gets session performance statistics
        /// </summary>
        private static OperationPerformanceStats GetSessionPerformanceStats()
        {
            return new OperationPerformanceStats
            {
                TotalOperations = GetCounterValue("Session_Total"),
                AverageResponseTime = GetCounterValue("Session_Average"),
                SlowOperations = GetCounterValue("Session_Slow"),
                ErrorCount = GetCounterValue("Session_Errors")
            };
        }

        /// <summary>
        /// Gets cache performance statistics
        /// </summary>
        private static OperationPerformanceStats GetCachePerformanceStats()
        {
            return new OperationPerformanceStats
            {
                TotalOperations = GetCounterValue("Cache_Total"),
                AverageResponseTime = GetCounterValue("Cache_Average"),
                SlowOperations = GetCounterValue("Cache_Slow"),
                ErrorCount = GetCounterValue("Cache_Errors")
            };
        }

        /// <summary>
        /// Gets ViewState performance statistics
        /// </summary>
        private static OperationPerformanceStats GetViewStatePerformanceStats()
        {
            return new OperationPerformanceStats
            {
                TotalOperations = GetCounterValue("ViewState_Total"),
                AverageResponseTime = GetCounterValue("ViewState_Average"),
                SlowOperations = GetCounterValue("ViewState_Slow"),
                ErrorCount = GetCounterValue("ViewState_Errors")
            };
        }

        /// <summary>
        /// Gets counter value (placeholder implementation)
        /// </summary>
        private static long GetCounterValue(string counterName)
        {
            try
            {
                lock (_lockObject)
                {
                    // In a real implementation, this would return actual counter values
                    return _performanceCounters.ContainsKey(counterName) ? 0 : 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Resets all performance counters
        /// </summary>
        public static void ResetCounters()
        {
            try
            {
                lock (_lockObject)
                {
                    _performanceCounters.Clear();
                    Logger.Info("Performance counters reset");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error resetting performance counters");
            }
        }
    }

    /// <summary>
    /// State management performance statistics
    /// </summary>
    public class StateManagementPerformanceStats
    {
        public OperationPerformanceStats SessionOperations { get; set; }
        public OperationPerformanceStats CacheOperations { get; set; }
        public OperationPerformanceStats ViewStateOperations { get; set; }
        public DateTime CollectedAt { get; set; }
        
        public StateManagementPerformanceStats()
        {
            SessionOperations = new OperationPerformanceStats();
            CacheOperations = new OperationPerformanceStats();
            ViewStateOperations = new OperationPerformanceStats();
            CollectedAt = DateTime.Now;
        }
    }

    /// <summary>
    /// Performance statistics for a specific operation type
    /// </summary>
    public class OperationPerformanceStats
    {
        public long TotalOperations { get; set; }
        public long AverageResponseTime { get; set; }
        public long SlowOperations { get; set; }
        public long ErrorCount { get; set; }
        public double SuccessRate 
        {
            get { return TotalOperations > 0 ? ((double)(TotalOperations - ErrorCount) / TotalOperations) * 100 : 0; }
        }
    }
}