using System;
using System.Web;
using System.Web.SessionState;
using NLog;

namespace Bookstore.WebForms.StateManagement
{
    /// <summary>
    /// Centralized session state management for the WebForms application
    /// </summary>
    public static class SessionManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // Session key constants
        private const string SHOPPING_CART_CORRELATION_ID = "ShoppingCartCorrelationId";
        private const string USER_PREFERENCES = "UserPreferences";
        private const string SEARCH_CRITERIA = "SearchCriteria";
        private const string ERROR_MESSAGE = "ErrorMessage";
        private const string SUCCESS_MESSAGE = "SuccessMessage";
        private const string RETURN_URL = "ReturnUrl";
        private const string LAST_ERROR_MESSAGE = "LastErrorMessage";
        private const string LAST_ERROR_DETAILS = "LastErrorDetails";
        private const string ERROR_RETURN_URL = "ErrorReturnUrl";
        private const string NOTIFICATION = "Notification";

        /// <summary>
        /// Gets or sets the shopping cart correlation ID for the current session
        /// </summary>
        public static string ShoppingCartCorrelationId
        {
            get
            {
                var correlationId = GetSessionValue<string>(SHOPPING_CART_CORRELATION_ID);
                if (string.IsNullOrEmpty(correlationId))
                {
                    correlationId = HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity != null && HttpContext.Current.User.Identity.IsAuthenticated
                        ? HttpContext.Current.User.Identity.Name
                        : Guid.NewGuid().ToString();
                    SetSessionValue(SHOPPING_CART_CORRELATION_ID, correlationId);
                }
                return correlationId;
            }
            set { SetSessionValue(SHOPPING_CART_CORRELATION_ID, value); }
        }

        /// <summary>
        /// Gets or sets user preferences for the current session
        /// </summary>
        public static UserPreferences UserPreferences
        {
            get { return GetSessionValue<UserPreferences>(USER_PREFERENCES) ?? new UserPreferences(); }
            set { SetSessionValue(USER_PREFERENCES, value); }
        }

        /// <summary>
        /// Gets or sets search criteria for the current session
        /// </summary>
        public static SearchCriteria SearchCriteria
        {
            get { return GetSessionValue<SearchCriteria>(SEARCH_CRITERIA); }
            set { SetSessionValue(SEARCH_CRITERIA, value); }
        }

        /// <summary>
        /// Gets or sets error message for display
        /// </summary>
        public static string ErrorMessage
        {
            get { return GetSessionValue<string>(ERROR_MESSAGE); }
            set { SetSessionValue(ERROR_MESSAGE, value); }
        }

        /// <summary>
        /// Gets or sets success message for display
        /// </summary>
        public static string SuccessMessage
        {
            get { return GetSessionValue<string>(SUCCESS_MESSAGE); }
            set { SetSessionValue(SUCCESS_MESSAGE, value); }
        }

        /// <summary>
        /// Gets or sets return URL for authentication redirects
        /// </summary>
        public static string ReturnUrl
        {
            get { return GetSessionValue<string>(RETURN_URL); }
            set { SetSessionValue(RETURN_URL, value); }
        }

        /// <summary>
        /// Gets or sets last error message for error pages
        /// </summary>
        public static string LastErrorMessage
        {
            get { return GetSessionValue<string>(LAST_ERROR_MESSAGE); }
            set { SetSessionValue(LAST_ERROR_MESSAGE, value); }
        }

        /// <summary>
        /// Gets or sets last error details for error pages
        /// </summary>
        public static string LastErrorDetails
        {
            get { return GetSessionValue<string>(LAST_ERROR_DETAILS); }
            set { SetSessionValue(LAST_ERROR_DETAILS, value); }
        }

        /// <summary>
        /// Gets or sets error return URL for error pages
        /// </summary>
        public static string ErrorReturnUrl
        {
            get { return GetSessionValue<string>(ERROR_RETURN_URL); }
            set { SetSessionValue(ERROR_RETURN_URL, value); }
        }

        /// <summary>
        /// Gets or sets notification message
        /// </summary>
        public static string Notification
        {
            get { return GetSessionValue<string>(NOTIFICATION); }
            set { SetSessionValue(NOTIFICATION, value); }
        }

        /// <summary>
        /// Clears error message from session
        /// </summary>
        public static void ClearErrorMessage()
        {
            RemoveSessionValue(ERROR_MESSAGE);
        }

        /// <summary>
        /// Clears success message from session
        /// </summary>
        public static void ClearSuccessMessage()
        {
            RemoveSessionValue(SUCCESS_MESSAGE);
        }

        /// <summary>
        /// Clears notification from session
        /// </summary>
        public static void ClearNotification()
        {
            RemoveSessionValue(NOTIFICATION);
        }

        /// <summary>
        /// Clears return URL from session
        /// </summary>
        public static void ClearReturnUrl()
        {
            RemoveSessionValue(RETURN_URL);
        }

        /// <summary>
        /// Clears error information from session
        /// </summary>
        public static void ClearErrorInfo()
        {
            RemoveSessionValue(LAST_ERROR_MESSAGE);
            RemoveSessionValue(LAST_ERROR_DETAILS);
            RemoveSessionValue(ERROR_RETURN_URL);
        }

        /// <summary>
        /// Clears all session data
        /// </summary>
        public static void ClearAll()
        {
            try
            {
                if (HttpContext.Current != null && HttpContext.Current.Session != null)
                    HttpContext.Current.Session.Clear();
                Logger.Debug("Session cleared successfully");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error clearing session");
            }
        }

        /// <summary>
        /// Abandons the current session
        /// </summary>
        public static void AbandonSession()
        {
            try
            {
                if (HttpContext.Current != null && HttpContext.Current.Session != null)
                    HttpContext.Current.Session.Abandon();
                Logger.Debug("Session abandoned successfully");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error abandoning session");
            }
        }

        /// <summary>
        /// Gets the current session ID
        /// </summary>
        public static string SessionId
        {
            get
            {
                try
                {
                    return HttpContext.Current != null && HttpContext.Current.Session != null ? HttpContext.Current.Session.SessionID : null;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error getting session ID");
                    return null;
                }
            }
        }

        /// <summary>
        /// Checks if session is available
        /// </summary>
        public static bool IsSessionAvailable
        {
            get
            {
                try
                {
                    return HttpContext.Current != null && HttpContext.Current.Session != null;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets session timeout in minutes
        /// </summary>
        public static int SessionTimeout
        {
            get
            {
                try
                {
                    return HttpContext.Current != null && HttpContext.Current.Session != null ? HttpContext.Current.Session.Timeout : 0;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error getting session timeout");
                    return 0;
                }
            }
        }

        /// <summary>
        /// Generic method to get a value from session
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="key">Session key</param>
        /// <returns>Value from session or default(T)</returns>
        private static T GetSessionValue<T>(string key)
        {
            try
            {
                var session = HttpContext.Current != null ? HttpContext.Current.Session : null;
                if (session == null) return default(T);

                var value = session[key];
                if (value is T)
                {
                    return (T)value;
                }
                return default(T);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error getting session value for key: {Key}", key);
                return default(T);
            }
        }

        /// <summary>
        /// Generic method to set a value in session
        /// </summary>
        /// <param name="key">Session key</param>
        /// <param name="value">Value to store</param>
        private static void SetSessionValue(string key, object value)
        {
            try
            {
                var session = HttpContext.Current != null ? HttpContext.Current.Session : null;
                if (session != null)
                {
                    session[key] = value;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error setting session value for key: {Key}", key);
            }
        }

        /// <summary>
        /// Removes a value from session
        /// </summary>
        /// <param name="key">Session key</param>
        private static void RemoveSessionValue(string key)
        {
            try
            {
                var session = HttpContext.Current != null ? HttpContext.Current.Session : null;
                if (session != null)
                {
                    session.Remove(key);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error removing session value for key: {Key}", key);
            }
        }
    }

    /// <summary>
    /// User preferences stored in session
    /// </summary>
    [Serializable]
    public class UserPreferences
    {
        public int PageSize { get; set; }
        public string SortBy { get; set; }
        public string SortDirection { get; set; }
        public string Theme { get; set; }
        public bool ShowOutOfStock { get; set; }
        public string Currency { get; set; }
        
        public UserPreferences()
        {
            PageSize = 10;
            SortBy = "Name";
            SortDirection = "ASC";
            Theme = "Default";
            ShowOutOfStock = true;
            Currency = "USD";
        }
    }

    /// <summary>
    /// Search criteria stored in session
    /// </summary>
    [Serializable]
    public class SearchCriteria
    {
        public string Query { get; set; }
        public string Category { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public bool InStockOnly { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        
        public SearchCriteria()
        {
            PageNumber = 1;
            PageSize = 10;
        }
    }
}