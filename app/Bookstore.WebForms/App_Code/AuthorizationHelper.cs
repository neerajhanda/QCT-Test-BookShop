using System;
using System.Security.Claims;
using System.Web;

namespace Bookstore.WebForms
{
    public static class AuthorizationHelper
    {
        /// <summary>
        /// Checks if the current user is authenticated
        /// </summary>
        public static bool IsAuthenticated()
        {
            // Simple cookie-based authentication check
            if (HttpContext.Current != null && HttpContext.Current.Request.Cookies["SimpleAuth"] != null)
            {
                var authCookie = HttpContext.Current.Request.Cookies["SimpleAuth"];
                return authCookie.Value == "authenticated";
            }
            
            // Fallback to standard authentication
            return HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity != null && HttpContext.Current.User.Identity.IsAuthenticated;
        }

        /// <summary>
        /// Checks if the current user has the specified role
        /// </summary>
        public static bool IsInRole(string role)
        {
            if (!IsAuthenticated())
                return false;

            return HttpContext.Current.User.IsInRole(role);
        }

        /// <summary>
        /// Checks if the current user is an administrator
        /// </summary>
        public static bool IsAdministrator()
        {
            return IsInRole("Administrators") || IsInRole("Admin");
        }

        /// <summary>
        /// Gets the current user's ID from claims
        /// </summary>
        public static string GetUserId()
        {
            if (!IsAuthenticated())
                return null;

            var identity = HttpContext.Current.User.Identity as ClaimsIdentity;
            if (identity == null)
                return null;

            // Try different claim types for user ID
            var userIdClaim = identity.FindFirst("sub") ?? 
                             identity.FindFirst("nameidentifier") ?? 
                             identity.FindFirst(ClaimTypes.NameIdentifier);

            return userIdClaim != null ? userIdClaim.Value : null;
        }

        /// <summary>
        /// Gets the current user's display name
        /// </summary>
        public static string GetUserDisplayName()
        {
            if (!IsAuthenticated())
                return null;

            var identity = HttpContext.Current.User.Identity as ClaimsIdentity;
            if (identity == null)
                return HttpContext.Current.User.Identity.Name;

            var givenNameClaim = identity.FindFirst("given_name") ?? identity.FindFirst(ClaimTypes.GivenName);
            var givenName = givenNameClaim != null ? givenNameClaim.Value : null;
            var familyNameClaim = identity.FindFirst("family_name") ?? identity.FindFirst(ClaimTypes.Surname);
            var familyName = familyNameClaim != null ? familyNameClaim.Value : null;

            if (!string.IsNullOrEmpty(givenName) && !string.IsNullOrEmpty(familyName))
            {
                return string.Format("{0} {1}", givenName, familyName);
            }

            return identity.Name ?? "User";
        }

        /// <summary>
        /// Redirects to login page if user is not authenticated
        /// </summary>
        public static void RequireAuthentication(string returnUrl = null)
        {
            if (!IsAuthenticated())
            {
                var currentUrl = returnUrl ?? HttpContext.Current.Request.RawUrl;
                var loginUrl = string.Format("~/Login.aspx?ReturnUrl={0}", HttpUtility.UrlEncode(currentUrl));
                HttpContext.Current.Response.Redirect(loginUrl);
            }
        }

        /// <summary>
        /// Redirects to login page if user is not in the specified role
        /// </summary>
        public static void RequireRole(string role, string returnUrl = null)
        {
            RequireAuthentication(returnUrl);

            if (!IsInRole(role))
            {
                // Redirect to unauthorized page or login
                HttpContext.Current.Response.Redirect("~/Unauthorized.aspx");
            }
        }

        /// <summary>
        /// Redirects to login page if user is not an administrator
        /// </summary>
        public static void RequireAdministrator(string returnUrl = null)
        {
            RequireAuthentication(returnUrl);

            if (!IsAdministrator())
            {
                HttpContext.Current.Response.Redirect("~/Unauthorized.aspx");
            }
        }

        /// <summary>
        /// Gets the logout URL based on the authentication type
        /// </summary>
        public static string GetLogoutUrl()
        {
            return "~/Login.aspx?action=logout";
        }

        /// <summary>
        /// Gets the login URL with optional return URL
        /// </summary>
        public static string GetLoginUrl(string returnUrl = null)
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                return "~/Login.aspx";
            }

            return string.Format("~/Login.aspx?ReturnUrl={0}", HttpUtility.UrlEncode(returnUrl));
        }

        /// <summary>
        /// Signs out the current user
        /// </summary>
        public static void SignOut()
        {
            try
            {
                // Clear session
                StateManagement.SessionManager.ClearAll();
                
                // Try to sign out using OWIN if available
                var context = HttpContext.Current;
                if (context != null)
                {
                    try
                    {
                        var owinContext = context.GetOwinContext();
                        if (owinContext != null && owinContext.Authentication != null)
                        {
                            owinContext.Authentication.SignOut();
                        }
                    }
                    catch
                    {
                        // OWIN context not available, continue with basic signout
                    }
                    
                    // Abandon session
                    if (context.Session != null)
                    {
                        context.Session.Abandon();
                    }
                    
                    // Redirect to login page
                    context.Response.Redirect(GetLoginUrl());
                }
            }
            catch (Exception)
            {
                // If all else fails, just redirect to login
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.Response.Redirect("~/Login.aspx");
                }
            }
        }
    }
}