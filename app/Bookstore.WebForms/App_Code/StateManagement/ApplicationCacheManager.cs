using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using NLog;

namespace Bookstore.WebForms.StateManagement
{
    /// <summary>
    /// Application-level caching manager for frequently accessed data
    /// </summary>
    public static class ApplicationCacheManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // Cache key constants
        private const string CATEGORIES_CACHE_KEY = "BookCategories";
        private const string PUBLISHERS_CACHE_KEY = "Publishers";
        private const string AUTHORS_CACHE_KEY = "Authors";
        private const string REFERENCE_DATA_CACHE_KEY = "ReferenceData";
        private const string SYSTEM_SETTINGS_CACHE_KEY = "SystemSettings";
        private const string FEATURED_BOOKS_CACHE_KEY = "FeaturedBooks";
        private const string BESTSELLERS_CACHE_KEY = "Bestsellers";

        // Cache duration constants (in minutes)
        private const int SHORT_CACHE_DURATION = 5;
        private const int MEDIUM_CACHE_DURATION = 30;
        private const int LONG_CACHE_DURATION = 60;
        private const int REFERENCE_DATA_CACHE_DURATION = 120; // 2 hours

        /// <summary>
        /// Gets the application cache instance
        /// </summary>
        private static Cache ApplicationCache 
        {
            get { return HttpContext.Current != null ? HttpContext.Current.Cache : HttpRuntime.Cache; }
        }

        /// <summary>
        /// Gets or sets cached book categories
        /// </summary>
        public static List<CategoryItem> BookCategories
        {
            get { return GetCachedItem<List<CategoryItem>>(CATEGORIES_CACHE_KEY); }
            set { SetCachedItem(CATEGORIES_CACHE_KEY, value, REFERENCE_DATA_CACHE_DURATION); }
        }

        /// <summary>
        /// Gets or sets cached publishers
        /// </summary>
        public static List<PublisherItem> Publishers
        {
            get { return GetCachedItem<List<PublisherItem>>(PUBLISHERS_CACHE_KEY); }
            set { SetCachedItem(PUBLISHERS_CACHE_KEY, value, REFERENCE_DATA_CACHE_DURATION); }
        }

        /// <summary>
        /// Gets or sets cached authors
        /// </summary>
        public static List<AuthorItem> Authors
        {
            get { return GetCachedItem<List<AuthorItem>>(AUTHORS_CACHE_KEY); }
            set { SetCachedItem(AUTHORS_CACHE_KEY, value, REFERENCE_DATA_CACHE_DURATION); }
        }

        /// <summary>
        /// Gets or sets cached reference data
        /// </summary>
        public static Dictionary<string, object> ReferenceData
        {
            get { return GetCachedItem<Dictionary<string, object>>(REFERENCE_DATA_CACHE_KEY); }
            set { SetCachedItem(REFERENCE_DATA_CACHE_KEY, value, REFERENCE_DATA_CACHE_DURATION); }
        }

        /// <summary>
        /// Gets or sets cached system settings
        /// </summary>
        public static Dictionary<string, string> SystemSettings
        {
            get { return GetCachedItem<Dictionary<string, string>>(SYSTEM_SETTINGS_CACHE_KEY); }
            set { SetCachedItem(SYSTEM_SETTINGS_CACHE_KEY, value, LONG_CACHE_DURATION); }
        }

        /// <summary>
        /// Gets or sets cached featured books
        /// </summary>
        public static List<FeaturedBookItem> FeaturedBooks
        {
            get { return GetCachedItem<List<FeaturedBookItem>>(FEATURED_BOOKS_CACHE_KEY); }
            set { SetCachedItem(FEATURED_BOOKS_CACHE_KEY, value, MEDIUM_CACHE_DURATION); }
        }

        /// <summary>
        /// Gets or sets cached bestsellers
        /// </summary>
        public static List<BestsellerItem> Bestsellers
        {
            get { return GetCachedItem<List<BestsellerItem>>(BESTSELLERS_CACHE_KEY); }
            set { SetCachedItem(BESTSELLERS_CACHE_KEY, value, SHORT_CACHE_DURATION); }
        }

        /// <summary>
        /// Gets a cached item by key
        /// </summary>
        /// <typeparam name="T">Type of the cached item</typeparam>
        /// <param name="key">Cache key</param>
        /// <returns>Cached item or default(T) if not found</returns>
        public static T GetCachedItem<T>(string key)
        {
            try
            {
                var cache = ApplicationCache;
                if (cache == null) return default(T);

                var cachedItem = cache[key];
                if (cachedItem is T)
                {
                    Logger.Debug("Cache hit for key: {CacheKey}", key);
                    return (T)cachedItem;
                }

                Logger.Debug("Cache miss for key: {CacheKey}", key);
                return default(T);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error getting cached item for key: {CacheKey}", key);
                return default(T);
            }
        }

        /// <summary>
        /// Sets a cached item with specified duration
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="value">Value to cache</param>
        /// <param name="durationMinutes">Cache duration in minutes</param>
        /// <param name="priority">Cache priority</param>
        public static void SetCachedItem(string key, object value, int durationMinutes, 
            CacheItemPriority priority = CacheItemPriority.Normal)
        {
            try
            {
                var cache = ApplicationCache;
                if (cache == null) return;

                var expiration = DateTime.Now.AddMinutes(durationMinutes);
                
                cache.Insert(key, value, null, expiration, Cache.NoSlidingExpiration, 
                    priority, OnCacheItemRemoved);

                Logger.Debug("Cached item with key: {CacheKey}, Duration: {Duration} minutes", key, durationMinutes);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error setting cached item for key: {CacheKey}", key);
            }
        }

        /// <summary>
        /// Sets a cached item with dependency
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <param name="value">Value to cache</param>
        /// <param name="dependency">Cache dependency</param>
        /// <param name="durationMinutes">Cache duration in minutes</param>
        /// <param name="priority">Cache priority</param>
        public static void SetCachedItemWithDependency(string key, object value, CacheDependency dependency,
            int durationMinutes, CacheItemPriority priority = CacheItemPriority.Normal)
        {
            try
            {
                var cache = ApplicationCache;
                if (cache == null) return;

                var expiration = DateTime.Now.AddMinutes(durationMinutes);
                
                cache.Insert(key, value, dependency, expiration, Cache.NoSlidingExpiration, 
                    priority, OnCacheItemRemoved);

                Logger.Debug("Cached item with dependency for key: {CacheKey}, Duration: {Duration} minutes", 
                    key, durationMinutes);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error setting cached item with dependency for key: {CacheKey}", key);
            }
        }

        /// <summary>
        /// Removes a cached item
        /// </summary>
        /// <param name="key">Cache key</param>
        public static void RemoveCachedItem(string key)
        {
            try
            {
                var cache = ApplicationCache;
                if (cache == null) return;

                cache.Remove(key);
                Logger.Debug("Removed cached item with key: {CacheKey}", key);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error removing cached item for key: {CacheKey}", key);
            }
        }

        /// <summary>
        /// Clears all cached reference data
        /// </summary>
        public static void ClearReferenceDataCache()
        {
            try
            {
                RemoveCachedItem(CATEGORIES_CACHE_KEY);
                RemoveCachedItem(PUBLISHERS_CACHE_KEY);
                RemoveCachedItem(AUTHORS_CACHE_KEY);
                RemoveCachedItem(REFERENCE_DATA_CACHE_KEY);
                
                Logger.Info("Reference data cache cleared");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error clearing reference data cache");
            }
        }

        /// <summary>
        /// Clears all cached content data
        /// </summary>
        public static void ClearContentCache()
        {
            try
            {
                RemoveCachedItem(FEATURED_BOOKS_CACHE_KEY);
                RemoveCachedItem(BESTSELLERS_CACHE_KEY);
                
                Logger.Info("Content cache cleared");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error clearing content cache");
            }
        }

        /// <summary>
        /// Clears all application cache
        /// </summary>
        public static void ClearAllCache()
        {
            try
            {
                var cache = ApplicationCache;
                if (cache == null) return;

                var keysToRemove = new List<string>();
                
                // Collect all cache keys (this is a simplified approach)
                var enumerator = cache.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    keysToRemove.Add(enumerator.Key.ToString());
                }

                // Remove all items
                foreach (var key in keysToRemove)
                {
                    cache.Remove(key);
                }

                Logger.Info("All application cache cleared, {Count} items removed", keysToRemove.Count);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error clearing all application cache");
            }
        }

        /// <summary>
        /// Gets cache statistics
        /// </summary>
        /// <returns>Cache statistics</returns>
        public static CacheStatistics GetCacheStatistics()
        {
            try
            {
                var cache = ApplicationCache;
                if (cache == null) return new CacheStatistics();

                var stats = new CacheStatistics();
                var enumerator = cache.GetEnumerator();
                
                while (enumerator.MoveNext())
                {
                    stats.TotalItems++;
                    
                    var key = enumerator.Key.ToString();
                    if (key.Contains("Categories") || key.Contains("Publishers") || 
                        key.Contains("Authors") || key.Contains("ReferenceData"))
                    {
                        stats.ReferenceDataItems++;
                    }
                    else if (key.Contains("Featured") || key.Contains("Bestsellers"))
                    {
                        stats.ContentItems++;
                    }
                    else
                    {
                        stats.OtherItems++;
                    }
                }

                return stats;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error getting cache statistics");
                return new CacheStatistics();
            }
        }

        /// <summary>
        /// Cache item removal callback
        /// </summary>
        private static void OnCacheItemRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            try
            {
                Logger.Debug("Cache item removed - Key: {CacheKey}, Reason: {Reason}", key, reason);
                
                // Handle specific removal reasons if needed
                switch (reason)
                {
                    case CacheItemRemovedReason.Expired:
                        Logger.Debug("Cache item expired: {CacheKey}", key);
                        break;
                    case CacheItemRemovedReason.Removed:
                        Logger.Debug("Cache item manually removed: {CacheKey}", key);
                        break;
                    case CacheItemRemovedReason.Underused:
                        Logger.Debug("Cache item removed due to memory pressure: {CacheKey}", key);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in cache item removal callback for key: {CacheKey}", key);
            }
        }
    }

    /// <summary>
    /// Cache statistics
    /// </summary>
    public class CacheStatistics
    {
        public int TotalItems { get; set; }
        public int ReferenceDataItems { get; set; }
        public int ContentItems { get; set; }
        public int OtherItems { get; set; }
    }

    /// <summary>
    /// Category item for caching
    /// </summary>
    [Serializable]
    public class CategoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int BookCount { get; set; }
    }

    /// <summary>
    /// Publisher item for caching
    /// </summary>
    [Serializable]
    public class PublisherItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int BookCount { get; set; }
    }

    /// <summary>
    /// Author item for caching
    /// </summary>
    [Serializable]
    public class AuthorItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int BookCount { get; set; }
    }

    /// <summary>
    /// Featured book item for caching
    /// </summary>
    [Serializable]
    public class FeaturedBookItem
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public bool IsInStock { get; set; }
    }

    /// <summary>
    /// Bestseller item for caching
    /// </summary>
    [Serializable]
    public class BestsellerItem
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int SalesCount { get; set; }
        public bool IsInStock { get; set; }
    }
}