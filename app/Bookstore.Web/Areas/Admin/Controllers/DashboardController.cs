using Bookstore.Domain.Books;
using Bookstore.Domain.Orders;
using Bookstore.Web.Areas.Admin.Models.Dashboard;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;


namespace Bookstore.Web.Areas.Admin.Controllers
{
    public class DashboardController : AdminAreaControllerBase
    {
        private readonly IOrderService orderService;
        private readonly IBookService bookService;

        public DashboardController(IOrderService orderService,IBookService bookService)
        {
            this.orderService = orderService;
            this.bookService = bookService;
        }

        public async Task<ActionResult> Index()
        {
            var orderStats = await orderService.GetStatisticsAsync();
            var inventoryStats = await bookService.GetStatisticsAsync();

            var model = new DashboardIndexViewModel
            {
                PastDueOrders = orderStats.PastDueOrders,
                PendingOrders = orderStats.PendingOrders,
                OrdersThisMonth = orderStats.OrdersThisMonth,
                OrdersTotal = orderStats.OrdersTotal,

                LowStock = inventoryStats.LowStock,
                OutOfStock = inventoryStats.OutOfStock,
                StockTotal = inventoryStats.StockTotal
            };

            return View(model);
        }
    }
}
