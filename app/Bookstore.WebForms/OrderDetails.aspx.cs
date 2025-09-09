using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using Bookstore.Domain.Orders;
using Bookstore.Domain;

namespace Bookstore.WebForms
{
    public partial class OrderDetails : BasePage
    {
        public IOrderService OrderService { get; set; }

        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                await LoadOrderDetailsAsync();
            }
        }

        private async Task LoadOrderDetailsAsync()
        {
            try
            {
                var orderIdParam = Request.QueryString["id"];
                if (string.IsNullOrEmpty(orderIdParam) || !int.TryParse(orderIdParam, out int orderId))
                {
                    Response.Redirect("~/Orders.aspx");
                    return;
                }

                var order = await OrderService.GetOrderAsync(orderId);
                if (order == null)
                {
                    Response.Redirect("~/Orders.aspx");
                    return;
                }

                // Populate order details
                OrderIdLiteral.Text = order.Id.ToString();
                OrderStatusLiteral.Text = order.OrderStatus.GetDescription();
                
                if (order.DeliveryDate == DateTime.MinValue)
                {
                    DeliveryDateLiteral.Text = "Unknown";
                }
                else
                {
                    DeliveryDateLiteral.Text = order.DeliveryDate.ToString("d");
                }
                
                TotalCostLiteral.Text = order.Total.ToString("C");

                // Bind order items
                var orderItems = order.OrderItems.Select(x => new
                {
                    BookId = x.BookId,
                    BookName = x.Book.Name,
                    ImageUrl = x.Book.CoverImageUrl,
                    Price = x.Book.Price
                }).ToList();

                OrderItemsGridView.DataSource = orderItems;
                OrderItemsGridView.DataBind();
            }
            catch (Exception ex)
            {
                ShowError("An error occurred while loading the order details. Please try again.");
                Response.Redirect("~/Orders.aspx");
            }
        }
    }
}