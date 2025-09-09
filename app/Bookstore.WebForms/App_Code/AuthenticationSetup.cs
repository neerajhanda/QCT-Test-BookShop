using System.Security.Claims;
using System.Threading.Tasks;
using Autofac;
using Autofac.Integration.Owin;
using Bookstore.Data;
using Bookstore.Domain.Customers;

using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System.Web;

namespace Bookstore.WebForms
{
    public static class AuthenticationSetup
    {
        public static void ConfigureAuthentication(IAppBuilder app)
        {
            if (BookstoreConfiguration.GetSetting("Services/Authentication") == "aws")
            {
                ConfigureCognitoAuthentication(app);
            }
            else
            {
                ConfigureLocalAuthentication(app);
            }
        }

        private static void ConfigureLocalAuthentication(IAppBuilder app)
        {
            // Temporarily disable middleware to test basic functionality
            // app.UseMiddlewareFromContainer<LocalAuthenticationMiddleware>();
        }

        private static void ConfigureCognitoAuthentication(IAppBuilder app)
        {
            // OpenIdConnect authentication temporarily disabled due to assembly version conflicts
            // Fall back to local authentication for now
            ConfigureLocalAuthentication(app);
            
            /*
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
                LoginPath = new Microsoft.Owin.PathString("/Login.aspx"),
                LogoutPath = new Microsoft.Owin.PathString("/Login.aspx?action=logout"),
                ExpireTimeSpan = System.TimeSpan.FromHours(24),
                SlidingExpiration = true
            });
            */
        }

        private static string GetClaimValue(ClaimsIdentity identity, string claimType)
        {
            var claim = identity.FindFirst(claimType) ?? 
                       identity.FindFirst(c => c.Type.Contains(claimType));
            return claim != null ? claim.Value : string.Empty;
        }
    }
}