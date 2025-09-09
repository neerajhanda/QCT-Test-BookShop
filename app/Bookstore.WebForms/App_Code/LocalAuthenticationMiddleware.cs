using System;
using Microsoft.Owin;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Bookstore.Domain.Customers;

namespace Bookstore.WebForms
{
    public class LocalAuthenticationMiddleware : OwinMiddleware
    {
        private const string UserId = "FB6135C7-1464-4A72-B74E-4B63D343DD09";

        private readonly ICustomerService _customerService;

        public LocalAuthenticationMiddleware(OwinMiddleware next, ICustomerService customerService) : base(next)
        {
            _customerService = customerService;
        }

        public override async Task Invoke(IOwinContext context)
        {
            var path = context.Request.Path.Value != null ? context.Request.Path.Value.ToLower() : null;
            
            // Handle logout requests first
            if (path == "/login.aspx" && context.Request.Query["action"] == "logout")
            {
                HandleLogout(context);
                return;
            }
            
            // Check for existing authentication before handling login
            if (HttpContext.Current != null && HttpContext.Current.Request.Cookies["LocalAuthentication"] != null)
            {
                CreateClaimsPrincipal(context);
                await SaveCustomerDetailsAsync();
            }
            
            // Handle login requests - but only if not already authenticated
            if (path == "/login.aspx" && context.Request.Query["action"] != "logout")
            {
                // If already authenticated, redirect to return URL or default
                if (HttpContext.Current != null && HttpContext.Current.User != null && 
                    HttpContext.Current.User.Identity != null && HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    var returnUrl = context.Request.Query["ReturnUrl"];
                    var redirectUrl = !string.IsNullOrEmpty(returnUrl) ? returnUrl : "/Default.aspx";
                    context.Response.Redirect(redirectUrl);
                    return;
                }
                else
                {
                    await HandleLogin(context);
                    return;
                }
            }

            await Next.Invoke(context);
        }

        private async Task HandleLogin(IOwinContext context)
        {
            try
            {
                CreateClaimsPrincipal(context);
                await SaveCustomerDetailsAsync();

                var userCookie = new HttpCookie("LocalAuthentication", "authenticated")
                {
                    Expires = DateTime.Now.AddDays(1),
                    HttpOnly = true,
                    Secure = context.Request.IsSecure
                };

                HttpContext.Current.Response.Cookies.Add(userCookie);

                // Get return URL from query string
                var returnUrl = context.Request.Query["ReturnUrl"];
                var redirectUrl = !string.IsNullOrEmpty(returnUrl) ? returnUrl : "/Default.aspx";
                
                // Ensure we don't redirect to login page to prevent loops
                if (redirectUrl.ToLower().Contains("/login.aspx"))
                {
                    redirectUrl = "/Default.aspx";
                }
                
                context.Response.Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error in HandleLogin: {0}", ex.Message));
                context.Response.Redirect("/Default.aspx");
            }
        }

        private void HandleLogout(IOwinContext context)
        {
            // Clear authentication cookie
            if (HttpContext.Current != null && HttpContext.Current.Request.Cookies["LocalAuthentication"] != null)
            {
                var logoutCookie = new HttpCookie("LocalAuthentication")
                {
                    Expires = DateTime.Now.AddDays(-1),
                    HttpOnly = true,
                    Secure = context.Request.IsSecure
                };
                HttpContext.Current.Response.Cookies.Add(logoutCookie);
            }

            // Clear user context
            context.Request.User = null;
            HttpContext.Current.User = null;

            context.Response.Redirect("/Default.aspx");
        }

        private void CreateClaimsPrincipal(IOwinContext context)
        {
            var identity = new ClaimsIdentity("LocalAuthentication");

            identity.AddClaim(new Claim(ClaimTypes.Name, "bookstoreuser"));
            identity.AddClaim(new Claim("sub", UserId));
            identity.AddClaim(new Claim("nameidentifier", UserId));
            identity.AddClaim(new Claim("given_name", "Bookstore"));
            identity.AddClaim(new Claim("family_name", "User"));
            identity.AddClaim(new Claim(ClaimTypes.Role, "Administrators"));

            var principal = new ClaimsPrincipal(identity);
            context.Request.User = principal;
            HttpContext.Current.User = principal;
        }

        private async Task SaveCustomerDetailsAsync()
        {
            try
            {
                var identity = (ClaimsIdentity)HttpContext.Current.User.Identity;

                var dto = new CreateOrUpdateCustomerDto(
                    GetClaimValue(identity, "nameidentifier") ?? GetClaimValue(identity, "sub"),
                    identity.Name,
                    GetClaimValue(identity, "given_name"),
                    GetClaimValue(identity, "family_name"));

                await _customerService.CreateOrUpdateCustomerAsync(dto);
            }
            catch (Exception ex)
            {
                // Log error but don't break authentication flow
                System.Diagnostics.Debug.WriteLine(string.Format("Error saving customer details: {0}", ex.Message));
            }
        }

        private string GetClaimValue(ClaimsIdentity identity, string claimType)
        {
            var claim = identity.FindFirst(claimType);
            if (claim != null)
                return claim.Value;
            
            var fallbackClaim = identity.FindFirst(c => c.Type.Contains(claimType));
            return fallbackClaim != null ? fallbackClaim.Value : null;
        }
    }
}