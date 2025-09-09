using System;

namespace Bookstore.WebForms
{
    public partial class SimpleTest : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // This page doesn't inherit from BasePage to avoid any authentication issues
            // It's a pure test page to verify basic functionality
        }
    }
}