using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using Bookstore.Domain.Orders;
using Bookstore.WebForms.Models;
using NLog;

namespace Bookstore.WebForms
{
    public partial class CheckoutFinished : BasePage
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // Dependency injection properties
        public IOrderService OrderService { get; set; }

        // Page-level properties for data binding
        protected List<CheckoutFinishedItemViewModel> _orderItems;

        protected async void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // Require authentication for order confirmation
                RequireAuthentication();

                if (!IsPostBack)
                {
                    await LoadOrderDetailsAsync();
                    BindControls();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading checkout finished page for user {UserId}", CurrentUserId);
                ShowError("An error occurred while loading your order confirmation. Please contact support if this issue persists.");
            }
        }

        protected async Task LoadOrderDetailsAsync()
        {
            try
            {
                // Get order ID from query string
                if (!int.TryParse(Request.QueryString["orderId"], out int orderId) || orderId <= 0)
                {
                    Response.Redirect("~/Default.aspx");
                    return;
                }

                // Load order details
                var order = await OrderService.GetOrderAsync(orderId);
                if (order == null)
                {
                    ShowError("Order not found.");
                    return;
                }

                // Verify that the order belongs to the current user
                if (order.Customer.Sub != CurrentUserId)
                {
                    Response.Redirect("~/Unauthorized.aspx");
                    return;
                }

                // Convert to view models
                _orderItems = order.OrderItems.Select(x => new CheckoutFinishedItemViewModel
                {
                    BookId = x.Book.Id,
                    Bookname = x.Book.Name,
                    Price = x.Book.Price,
                    Quantity = x.Quantity,
                    Url = x.Book.CoverImageUrl,
                    Total = x.Book.Price * x.Quantity
                }).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading order details for user {UserId}", CurrentUserId);
                throw;
            }
        }

        protected void BindControls()
        {
            try
            {
                if (_orderItems != null && _orderItems.Any())
                {
                    // Bind order items
                    OrderItemsRepeater.DataSource = _orderItems;
                    OrderItemsRepeater.DataBind();
                    
                    OrderDetailsPanel.Visible = true;
                }
                else
                {
                    OrderDetailsPanel.Visible = false;
                    ShowError("No order items found.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error binding controls for user {UserId}", CurrentUserId);
                throw;
            }
        }

        private void ShowError(string message)
        {
            ErrorPanel.Visible = true;
            ErrorLabel.Text = message;
        }
    }
}