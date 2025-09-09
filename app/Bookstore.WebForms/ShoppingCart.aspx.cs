using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bookstore.Domain.Carts;
using Bookstore.Domain.Customers;
using NLog;


namespace Bookstore.WebForms
{
    public partial class ShoppingCart : BasePage
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        public IShoppingCartService ShoppingCartService { get; set; }
        public ICustomerService CustomerService { get; set; }

        private List<ShoppingCartItemViewModel> _shoppingCartItems;

        protected async void Page_Load(object sender, EventArgs e)
        {
            try
            {
                Logger.Info("ShoppingCart Page_Load started");
                
                if (!IsPostBack)
                {
                    Logger.Info("ShoppingCart Page_Load - not postback, loading cart");
                    await LoadShoppingCartAsync();
                    Logger.Info("ShoppingCart Page_Load - cart loaded successfully");
                }
                else
                {
                    Logger.Info("ShoppingCart Page_Load - is postback, skipping load");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in ShoppingCart Page_Load");
                ShowEmptyCart();
                ShowNotificationWithCss("An error occurred while loading your shopping cart.", "alert-danger");
            }
        }

        private async Task LoadShoppingCartAsync()
        {
            try
            {
                Logger.Info("LoadShoppingCartAsync started");
                
                // Check if ShoppingCartService is available
                if (ShoppingCartService == null)
                {
                    Logger.Warn("ShoppingCartService is null - showing empty cart");
                    ShowEmptyCart();
                    return;
                }

                // Add a timeout wrapper to prevent hanging
                using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(10)))
                {
                    try
                    {

                Logger.Info("Getting shopping cart correlation ID");
                var correlationId = GetShoppingCartCorrelationId();
                Logger.Info($"Shopping cart correlation ID: {correlationId}");
                
                        Logger.Info("Calling ShoppingCartService.GetShoppingCartAsync");
                        var shoppingCart = await ShoppingCartService.GetShoppingCartAsync(correlationId);
                        Logger.Info("ShoppingCartService.GetShoppingCartAsync completed");

                        if (shoppingCart == null)
                        {
                            Logger.Info("Shopping cart is null - showing empty cart");
                            ShowEmptyCart();
                            return;
                        }

                        var shoppingCartItems = shoppingCart
                            .GetShoppingCartItems(ShoppingCartItemFilter.IncludeOutOfStockItems)
                            .Select(c => new ShoppingCartItemViewModel
                            {
                                BookId = c.Book.Id,
                                ImageUrl = c.Book.CoverImageUrl,
                                Price = c.Book.Price,
                                BookName = c.Book.Name,
                                ShoppingCartItemId = c.Id,
                                StockLevel = c.Book.Quantity,
                                HasLowStockLevels = c.Book.Quantity <= 5,
                                IsOutOfStock = c.Book.Quantity <= 0
                            }).ToList();

                        if (!shoppingCartItems.Any())
                        {
                            Logger.Info("No shopping cart items found - showing empty cart");
                            ShowEmptyCart();
                            return;
                        }

                        _shoppingCartItems = shoppingCartItems;
                        BindShoppingCart();
                        Logger.Info("Shopping cart loaded and bound successfully");
                    }
                    catch (System.Threading.Tasks.TaskCanceledException)
                    {
                        Logger.Warn("Shopping cart loading timed out - showing empty cart");
                        ShowEmptyCart();
                        ShowNotificationWithCss("Loading took too long. Please try again.", "alert-warning");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in LoadShoppingCartAsync");
                // Log error and show user-friendly message
                ShowEmptyCart();
                ShowNotificationWithCss("An error occurred while loading your shopping cart. Please try again.", "alert-danger");
            }
        }

        private void BindShoppingCart()
        {
            ShoppingCartRepeater.DataSource = _shoppingCartItems;
            ShoppingCartRepeater.DataBind();

            var totalPrice = _shoppingCartItems.Sum(x => x.Price);
            TotalPriceLabel.Text = totalPrice.ToString("C");

            // Show appropriate checkout/login panel
            if (User.Identity.IsAuthenticated)
            {
                CheckoutPanel.Visible = true;
                LoginPromptPanel.Visible = false;
            }
            else
            {
                CheckoutPanel.Visible = false;
                LoginPromptPanel.Visible = true;
            }

            EmptyCartPanel.Visible = false;
        }

        private void ShowEmptyCart()
        {
            EmptyCartPanel.Visible = true;
            ShoppingCartRepeater.Visible = false;
            TotalPriceLabel.Text = "$0.00";
            CheckoutPanel.Visible = false;
            LoginPromptPanel.Visible = false;
        }

        protected async void ShoppingCartRepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Remove")
            {
                try
                {
                    var shoppingCartItemId = Convert.ToInt32(e.CommandArgument);
                    var correlationId = GetShoppingCartCorrelationId();
                    
                    var dto = new DeleteShoppingCartItemDto(correlationId, shoppingCartItemId);
                    await ShoppingCartService.DeleteShoppingCartItemAsync(dto);

                    ShowNotificationWithCss("Item removed from shopping cart.", "alert-success");
                    
                    // Reload the shopping cart
                    await LoadShoppingCartAsync();
                }
                catch (Exception ex)
                {
                    ShowNotificationWithCss("An error occurred while removing the item. Please try again.", "alert-danger");
                }
            }
        }

        private new string GetShoppingCartCorrelationId()
        {
            // Use the centralized session manager for cart correlation ID
            return base.GetShoppingCartCorrelationId();
        }

        private void ShowNotificationWithCss(string message, string cssClass = "alert-info")
        {
            NotificationLabel.Text = message;
            NotificationPanel.CssClass = $"alert {cssClass}";
            NotificationPanel.Visible = true;
        }
    }

    public class ShoppingCartItemViewModel
    {
        public int ShoppingCartItemId { get; set; }
        public long BookId { get; set; }
        public string BookName { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int StockLevel { get; set; }
        public bool HasLowStockLevels { get; set; }
        public bool IsOutOfStock { get; set; }
    }
}