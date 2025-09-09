using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bookstore.Domain.Orders;
using Bookstore.Domain;

namespace Bookstore.WebForms
{
    public partial class Orders : BasePage
    {
        public IOrderService OrderService { get; set; }

        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                await LoadOrdersAsync();
            }
        }

        private async Task LoadOrdersAsync()
        {
            try
            {
                var userSub = GetUserSub();
                if (string.IsNullOrEmpty(userSub))
                {
                    Response.Redirect("~/Login.aspx?returnUrl=" + Server.UrlEncode(Request.Url.ToString()));
                    return;
                }

                var orders = await OrderService.GetOrdersAsync(userSub);
                var orderList = orders.ToList();

                if (!orderList.Any())
                {
                    NoOrdersPanel.Visible = true;
                    OrdersPanel.Visible = false;
                }
                else
                {
                    NoOrdersPanel.Visible = false;
                    OrdersPanel.Visible = true;
                    
                    var orderViewModels = orderList.Select(x => new
                    {
                        Id = x.Id,
                        SubTotal = x.SubTotal,
                        DeliveryDate = x.DeliveryDate,
                        OrderStatus = x.OrderStatus.GetDescription()
                    }).ToList();

                    OrdersGridView.DataSource = orderViewModels;
                    OrdersGridView.DataBind();
                }
            }
            catch (Exception ex)
            {
                // Log error and show user-friendly message
                ShowError("An error occurred while loading your orders. Please try again.");
            }
        }        
        protected async void OrdersGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewDetails")
            {
                int orderId = Convert.ToInt32(e.CommandArgument);
                Response.Redirect($"~/OrderDetails.aspx?id={orderId}");
            }
            else if (e.CommandName == "CancelOrder")
            {
                await CancelOrderAsync(Convert.ToInt32(e.CommandArgument));
            }
        }

        private async Task CancelOrderAsync(int orderId)
        {
            try
            {
                var userSub = GetUserSub();
                if (string.IsNullOrEmpty(userSub))
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }

                var dto = new CancelOrderDto(userSub, orderId);
                await OrderService.CancelOrderAsync(dto);
                
                ShowSuccess("Order cancelled successfully.");
                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                ShowError("An error occurred while cancelling the order. Please try again.");
            }
        }

        private string GetUserSub()
        {
            return CurrentUserId;
        }
    }
}