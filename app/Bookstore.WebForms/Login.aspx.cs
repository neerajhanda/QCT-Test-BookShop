using System;
using System.Web;
using System.Web.UI;
using BobsBookstoreClassic.Data;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using Bookstore.WebForms.StateManagement;

namespace Bookstore.WebForms
{
    public partial class Login : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                HandlePageLoad();
            }
        }

        private void HandlePageLoad()
        {
            var action = Request.QueryString["action"];
            
            if (action == "logout")
            {
                HandleLogout();
                return;
            }

            // Simple auto-login for testing - just set a cookie and redirect
            HandleSimpleLogin();
        }

        protected void LoginButton_Click(object sender, EventArgs e)
        {
            try
            {
                var authType = BookstoreConfiguration.GetSetting("Services/Authentication");
                
                if (authType == "aws")
                {
                    InitiateCognitoLogin();
                }
                else
                {
                    InitiateLocalLogin();
                }
            }
            catch (Exception ex)
            {
                ShowError(string.Format("Login failed: {0}", ex.Message));
            }
        }

        private void InitiateCognitoLogin()
        {
            var returnUrl = Request.QueryString["ReturnUrl"] ?? "~/Default.aspx";
            
            // Store return URL in session for later use
            SessionManager.ReturnUrl = returnUrl;
            
            // Trigger OWIN OpenID Connect authentication
            var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
            authenticationManager.Challenge(new AuthenticationProperties
            {
                RedirectUri = ResolveUrl(returnUrl)
            }, OpenIdConnectAuthenticationDefaults.AuthenticationType);
        }

        private void InitiateLocalLogin()
        {
            var returnUrl = Request.QueryString["ReturnUrl"] ?? "~/Default.aspx";
            
            // For local authentication, redirect to the middleware endpoint
            var loginUrl = string.Format("~/Login.aspx?ReturnUrl={0}", Server.UrlEncode(returnUrl));
            Response.Redirect(loginUrl);
        }

        private void HandleLogout()
        {
            try
            {
                var authType = BookstoreConfiguration.GetSetting("Services/Authentication");
                
                if (authType == "aws")
                {
                    HandleCognitoLogout();
                }
                else
                {
                    HandleLocalLogout();
                }
            }
            catch (Exception ex)
            {
                ShowError(string.Format("Logout failed: {0}", ex.Message));
            }
        }

        private void HandleCognitoLogout()
        {
            // Clear OWIN authentication
            var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
            authenticationManager.SignOut();

            // Clear ASP.NET authentication cookie
            if (Request.Cookies[".AspNet.Cookies"] != null)
            {
                var cookie = new HttpCookie(".AspNet.Cookies")
                {
                    Expires = DateTime.Now.AddDays(-1)
                };
                Response.Cookies.Add(cookie);
            }

            // Redirect to Cognito logout
            var domain = BookstoreConfiguration.GetSetting("Authentication/Cognito/CognitoDomain");
            var clientId = BookstoreConfiguration.GetSetting("Authentication/Cognito/LocalClientId");
            var logoutUri = $"{Request.Url.Scheme}://{Request.Url.Host}:{Request.Url.Port}/Default.aspx";

            var cognitoLogoutUrl = string.Format("{0}/logout?client_id={1}&logout_uri={2}", domain, clientId, logoutUri);
            Response.Redirect(cognitoLogoutUrl);
        }

        private void HandleSimpleLogin()
        {
            // Simple authentication - just set a cookie and redirect
            var authCookie = new HttpCookie("SimpleAuth", "authenticated")
            {
                Expires = DateTime.Now.AddDays(1),
                HttpOnly = true
            };
            Response.Cookies.Add(authCookie);
            
            var returnUrl = Request.QueryString["ReturnUrl"];
            var redirectUrl = !string.IsNullOrEmpty(returnUrl) ? returnUrl : "~/Default.aspx";
            
            // Prevent redirect loops
            if (redirectUrl.ToLower().Contains("login.aspx"))
            {
                redirectUrl = "~/Default.aspx";
            }
            
            Response.Redirect(redirectUrl);
        }

        private void HandleLocalLogout()
        {
            // Clear authentication cookie
            if (Request.Cookies["SimpleAuth"] != null)
            {
                var logoutCookie = new HttpCookie("SimpleAuth")
                {
                    Expires = DateTime.Now.AddDays(-1),
                    HttpOnly = true
                };
                Response.Cookies.Add(logoutCookie);
            }
            
            // Clear session
            Session.Clear();
            Session.Abandon();
            
            Response.Redirect("~/Default.aspx");
        }

        private void HandleAuthenticationReturn()
        {
            var error = Request.QueryString["error"];
            if (!string.IsNullOrEmpty(error))
            {
                var errorDescription = Request.QueryString["error_description"];
                ShowError(string.Format("Authentication failed: {0}. {1}", error, errorDescription));
                return;
            }

            // If we have a code, the OWIN middleware should handle it
            var code = Request.QueryString["code"];
            if (!string.IsNullOrEmpty(code))
            {
                ShowInfo("Authentication successful. Redirecting...");
                
                // Get return URL from session or default
                var returnUrl = SessionManager.ReturnUrl ?? "~/Default.aspx";
                SessionManager.ClearReturnUrl();
                
                // Use a client-side redirect to allow OWIN to process the authentication
                ClientScript.RegisterStartupScript(this.GetType(), "redirect", 
                    string.Format("setTimeout(function() {{ window.location.href = '{0}'; }}, 1000);", ResolveUrl(returnUrl)), true);
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorPanel.Visible = true;
            InfoPanel.Visible = false;
        }

        private void ShowInfo(string message)
        {
            InfoMessage.Text = message;
            InfoPanel.Visible = true;
            ErrorPanel.Visible = false;
        }
    }
}