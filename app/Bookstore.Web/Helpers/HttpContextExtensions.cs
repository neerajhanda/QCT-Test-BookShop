using System;
using System.Drawing;
using System.Net;
using Microsoft.AspNetCore.Owin;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Bookstore.Web.Helpers
{
    public static class HttpContextExtensions
    {
        public static string GetShoppingCartCorrelationId(this HttpContext context)
        {
            var CookieKey = "ShoppingCartId";

            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddYears(1),
                Path = "/"
            };

            string cookieValue = context.Request.Cookies[CookieKey];
            string shoppingCartClientId = cookieValue;

            //var shoppingCartClientId = context.Request.Cookies[CookieKey];

            if (string.IsNullOrWhiteSpace(shoppingCartClientId))
            {
                shoppingCartClientId = context.User.Identity.IsAuthenticated ? context.User.FindFirstValue(ClaimTypes.NameIdentifier) : Guid.NewGuid().ToString();
            }

            context.Response.Cookies.Append(CookieKey, shoppingCartClientId, cookieOptions);

            return shoppingCartClientId;
        }
    }
}