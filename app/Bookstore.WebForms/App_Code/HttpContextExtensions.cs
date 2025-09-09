using System;
using System.Web;

namespace Bookstore.WebForms
{
    public static class HttpContextExtensions
    {
        private const string SHOPPING_CART_CORRELATION_ID_KEY = "ShoppingCartCorrelationId";

        /// <summary>
        /// Gets or creates a shopping cart correlation ID for the current session
        /// </summary>
        public static string GetShoppingCartCorrelationId(this HttpContext context)
        {
            if (context == null || context.Session == null)
                return Guid.NewGuid().ToString();

            var correlationId = context.Session[SHOPPING_CART_CORRELATION_ID_KEY] as string;
            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
                context.Session[SHOPPING_CART_CORRELATION_ID_KEY] = correlationId;
            }

            return correlationId;
        }

        /// <summary>
        /// Sets the shopping cart correlation ID for the current session
        /// </summary>
        public static void SetShoppingCartCorrelationId(this HttpContext context, string correlationId)
        {
            if (context != null && context.Session != null)
            {
                context.Session[SHOPPING_CART_CORRELATION_ID_KEY] = correlationId;
            }
        }

        /// <summary>
        /// Clears the shopping cart correlation ID from the current session
        /// </summary>
        public static void ClearShoppingCartCorrelationId(this HttpContext context)
        {
            if (context != null && context.Session != null)
            {
                context.Session.Remove(SHOPPING_CART_CORRELATION_ID_KEY);
            }
        }
    }
}