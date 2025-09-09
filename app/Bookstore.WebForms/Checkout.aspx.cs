using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bookstore.Domain.Addresses;
using Bookstore.Domain.Carts;
using Bookstore.Domain.Orders;
using Bookstore.WebForms.Models;
using NLog;

namespace Bookstore.WebForms
{
    public partial class Checkout : BasePage
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // Dependency injection properties
        public IAddressService AddressService { get; set; }
        public IShoppingCartService ShoppingCartService { get; set; }
        public IOrderService OrderService { get; set; }

        // Page-level properties for data binding
        protected int SelectedAddressId { get; set; }
        protected List<CheckoutAddressViewModel> _addresses;
        protected List<CheckoutItemViewModel> _shoppingCartItems;
        protected decimal _total;

        protected async void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // Require authentication for checkout
                RequireAuthentication();

                if (!IsPostBack)
                {
                    await LoadCheckoutDataAsync();
                    BindControls();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading checkout page for user {UserId}", CurrentUserId);
                ShowError("An error occurred while loading the checkout page. Please try again.");
            }
        }

        protected async Task LoadCheckoutDataAsync()
        {
            try
            {
                // Get shopping cart correlation ID from session or cookie
                var correlationId = GetShoppingCartCorrelationId();
                
                // Load shopping cart
                var shoppingCart = await ShoppingCartService.GetShoppingCartAsync(correlationId);
                if (shoppingCart == null)
                {
                    Response.Redirect("~/ShoppingCart.aspx");
                    return;
                }

                // Load user addresses
                var addresses = await AddressService.GetAddressesAsync(CurrentUserId);

                // Convert to view models
                _addresses = addresses.Select(x => new CheckoutAddressViewModel
                {
                    Id = x.Id,
                    AddressLine1 = x.AddressLine1,
                    AddressLine2 = x.AddressLine2,
                    City = x.City,
                    Country = x.Country,
                    State = x.State,
                    ZipCode = x.ZipCode,
                    IsPrimary = x.IsPrimary
                }).ToList();

                _shoppingCartItems = shoppingCart.GetShoppingCartItems(ShoppingCartItemFilter.IncludeOutOfStockItems)
                    .Select(x => new CheckoutItemViewModel
                    {
                        BookName = x.Book.Name,
                        ImageUrl = x.Book.CoverImageUrl,
                        Price = x.Book.Price,
                        OutOfStock = x.Book.Quantity <= 0
                    }).ToList();

                _total = shoppingCart.GetSubTotal(ShoppingCartItemFilter.ExcludeOutOfStockItems);

                // Set default selected address
                SelectedAddressId = _addresses.Count > 0 ? _addresses.First().Id : 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading checkout data for user {UserId}", CurrentUserId);
                throw;
            }
        }

        private void BindControls()
        {
            try
            {
                // Bind total price
                TotalPriceLabel.Text = _total.ToString("F2");

                // Bind addresses
                AddressRepeater.DataSource = _addresses;
                AddressRepeater.DataBind();

                // Set default selected address
                if (_addresses.Count > 0)
                {
                    foreach (RepeaterItem item in AddressRepeater.Items)
                    {
                        var radioButton = item.FindControl("AddressRadioButton") as RadioButton;
                        if (radioButton != null)
                        {
                            var addressId = int.Parse(radioButton.Attributes["data-address-id"] ?? "0");
                            if (addressId == SelectedAddressId)
                            {
                                radioButton.Checked = true;
                                break;
                            }
                        }
                    }
                }

                // Bind shopping cart items
                ShoppingCartItemsRepeater.DataSource = _shoppingCartItems;
                ShoppingCartItemsRepeater.DataBind();

                // Show/hide panels based on address availability
                if (_addresses.Count == 0)
                {
                    NoAddressPanel.Visible = true;
                    HasAddressPanel.Visible = false;
                }
                else
                {
                    NoAddressPanel.Visible = false;
                    HasAddressPanel.Visible = true;
                }

                // Update add address link with return URL
                AddAddressLink.NavigateUrl = $"~/Address.aspx?action=create&returnUrl={Server.UrlEncode(Request.RawUrl)}";
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error binding controls for user {UserId}", CurrentUserId);
                throw;
            }
        }

        protected async void FinishOrderButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate address selection
                var selectedAddressId = GetSelectedAddressId();
                if (selectedAddressId == 0)
                {
                    ShowError("Please select an address to proceed with checkout.");
                    return;
                }

                // Create order
                var correlationId = GetShoppingCartCorrelationId();
                var createOrderDto = new CreateOrderDto(CurrentUserId, correlationId, selectedAddressId);
                
                var orderId = await OrderService.CreateOrderAsync(createOrderDto);

                // Redirect to order confirmation page
                Response.Redirect($"~/CheckoutFinished.aspx?orderId={orderId}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error processing order for user {UserId}", CurrentUserId);
                ShowError("An error occurred while processing your order. Please try again.");
            }
        }

        private int GetSelectedAddressId()
        {
            // Find the selected radio button in the address repeater
            foreach (RepeaterItem item in AddressRepeater.Items)
            {
                var radioButton = item.FindControl("AddressRadioButton") as RadioButton;
                if (radioButton != null && radioButton.Checked)
                {
                    if (int.TryParse(radioButton.Attributes["data-address-id"] ?? "0", out int addressId))
                    {
                        return addressId;
                    }
                }
            }
            return 0;
        }

        private string GetShoppingCartCorrelationId()
        {
            // Try to get from session first
            var correlationId = Session["ShoppingCartCorrelationId"] as string;
            
            if (string.IsNullOrEmpty(correlationId))
            {
                // Try to get from cookie
                var cookie = Request.Cookies["ShoppingCartCorrelationId"];
                if (cookie != null)
                {
                    correlationId = cookie.Value;
                }
            }

            if (string.IsNullOrEmpty(correlationId))
            {
                // Generate new correlation ID
                correlationId = Guid.NewGuid().ToString();
                Session["ShoppingCartCorrelationId"] = correlationId;
                
                // Also set cookie for persistence
                var newCookie = new System.Web.HttpCookie("ShoppingCartCorrelationId", correlationId)
                {
                    Expires = DateTime.Now.AddDays(30)
                };
                Response.Cookies.Add(newCookie);
            }

            return correlationId;
        }

        private void ShowError(string message)
        {
            ErrorPanel.Visible = true;
            ErrorLabel.Text = message;
            NotificationPanel.Visible = false;
        }

        private void ShowNotification(string message)
        {
            NotificationPanel.Visible = true;
            NotificationLabel.Text = message;
            ErrorPanel.Visible = false;
        }
    }
}