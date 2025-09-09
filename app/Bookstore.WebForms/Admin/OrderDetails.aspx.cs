using Bookstore.Domain.Orders;
using Bookstore.WebForms.Models;
using System;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace Bookstore.WebForms.Admin
{
    public partial class OrderDetails : AdminBasePage
    {
        public IOrderService OrderService { get; set; }

        private AdminOrderDetailsViewModel _viewModel;
        private int _orderId;

        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!int.TryParse(Request.QueryString["id"], out _orderId) || _orderId <= 0)
            {
                Response.Redirect("Orders.aspx");
                return;
            }

            if (!IsPostBack)
            {
                await InitializePageAsync();
            }
        }

        private async Task InitializePageAsync()
        {
            try
            {
                PopulateOrderStatusDropDown();
                await LoadOrderDetailsAsync();
                DisplayMessage();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error initializing order details page for order {OrderId}", _orderId);
                ShowErrorMessage("An error occurred while loading the order details. Please try again.");
            }
        }

        private void PopulateOrderStatusDropDown()
        {
            ddlOrderStatus.Items.Clear();
            
            foreach (OrderStatus status in Enum.GetValues(typeof(OrderStatus)))
            {
                ddlOrderStatus.Items.Add(new ListItem(status.ToString(), ((int)status).ToString()));
            }
        }

        private async Task LoadOrderDetailsAsync()
        {
            try
            {
                var order = await OrderService.GetOrderAsync(_orderId);
                if (order == null)
                {
                    Response.Redirect("Orders.aspx");
                    return;
                }

                _viewModel = new AdminOrderDetailsViewModel(order);
                BindOrderDetails();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading order details for order {OrderId}", _orderId);
                ShowErrorMessage("An error occurred while loading order details. Please try again.");
            }
        }

        private void BindOrderDetails()
        {
            if (_viewModel == null) return;

            // Bind customer and address information
            litCustomerName.Text = Server.HtmlEncode(_viewModel.CustomerName);
            litAddressLine1.Text = Server.HtmlEncode(_viewModel.AddressLine1);
            
            if (!string.IsNullOrWhiteSpace(_viewModel.AddressLine2))
            {
                pnlAddressLine2.Visible = true;
                litAddressLine2.Text = Server.HtmlEncode(_viewModel.AddressLine2);
            }
            
            litCity.Text = Server.HtmlEncode(_viewModel.City);
            litState.Text = Server.HtmlEncode(_viewModel.State);
            litZipCode.Text = Server.HtmlEncode(_viewModel.ZipCode);
            litCountry.Text = Server.HtmlEncode(_viewModel.Country);

            // Bind order information
            ddlOrderStatus.SelectedValue = ((int)_viewModel.SelectedOrderStatus).ToString();
            litOrderDate.Text = _viewModel.OrderDate.ToString("MM/dd/yyyy");
            litDeliveryDate.Text = _viewModel.DeliveryDate.ToString("MM/dd/yyyy");
            litSubtotal.Text = _viewModel.Subtotal.ToString("C");
            litTax.Text = _viewModel.Tax.ToString("C");
            litTotal.Text = _viewModel.Total.ToString("C");

            // Bind order items
            gvOrderItems.DataSource = _viewModel.Items;
            gvOrderItems.DataBind();
        }

        protected async void btnUpdateStatus_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Enum.TryParse<OrderStatus>(ddlOrderStatus.SelectedValue, out var newStatus))
                {
                    ShowErrorMessage("Invalid order status selected.");
                    return;
                }

                var dto = new UpdateOrderStatusDto(_orderId, newStatus);
                await OrderService.UpdateOrderStatusAsync(dto);

                // Show success message
                ShowSuccessMessage("Order status has been updated");
                
                // Reload the order details to reflect the change
                await LoadOrderDetailsAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error updating order status for order {OrderId}", _orderId);
                ShowErrorMessage("An error occurred while updating the order status. Please try again.");
            }
        }

        private void DisplayMessage()
        {
            var message = Session["Message"] as string;
            if (!string.IsNullOrEmpty(message))
            {
                ShowSuccessMessage(message);
                Session.Remove("Message");
            }
        }

        private void ShowSuccessMessage(string message)
        {
            pnlMessage.CssClass = "alert alert-success mx-3";
            litMessage.Text = Server.HtmlEncode(message);
            pnlMessage.Visible = true;
        }

        private void ShowErrorMessage(string message)
        {
            pnlMessage.CssClass = "alert alert-danger mx-3";
            litMessage.Text = Server.HtmlEncode(message);
            pnlMessage.Visible = true;
        }
    }
}