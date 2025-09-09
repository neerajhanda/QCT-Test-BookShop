using System.Configuration;

namespace Bookstore.WebForms
{
    /// <summary>
    /// Configuration helper for the Bookstore application
    /// </summary>
    public static class BookstoreConfiguration
    {
        /// <summary>
        /// Gets a configuration setting value
        /// </summary>
        /// <param name="key">The configuration key</param>
        /// <returns>The configuration value</returns>
        public static string GetSetting(string key)
        {
            return ConfigurationManager.AppSettings[key] ?? string.Empty;
        }

        /// <summary>
        /// Gets a connection string
        /// </summary>
        /// <param name="name">The connection string name</param>
        /// <returns>The connection string</returns>
        public static string GetConnectionString(string name)
        {
            var connectionString = ConfigurationManager.ConnectionStrings[name];
            return connectionString != null ? connectionString.ConnectionString : string.Empty;
        }
    }
}