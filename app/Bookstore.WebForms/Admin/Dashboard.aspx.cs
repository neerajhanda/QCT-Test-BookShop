using System;
using System.Threading.Tasks;
using System.Web.UI;
using Bookstore.Domain;
using Bookstore.Domain.Books;
using Bookstore.Domain.Offers;
using Bookstore.Domain.Orders;
using NLog;

namespace Bookstore.WebForms.Admin
{
    /// <summary>
    /// Admin dashboard page that displays key metrics and statistics
    /// </summary>
    public partial class Dashboard : AdminBasePage
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // Injected services
        public IOrderService OrderService { get; set; }
        public IOfferService OfferService { get; set; }
        public IBookService BookService { get; set; }

        // Dashboard data properties
        public int PendingOrders { get; private set; }
        public int PastDueOrders { get; private set; }
        public int OrdersThisMonth { get; private set; }
        public int OrdersTotal { get; private set; }
        public int PendingOffers { get; private set; }
        public int OffersThisMonth { get; private set; }
        public int OffersTotal { get; private set; }
        public int OutOfStock { get; private set; }
        public int LowStock { get; private set; }
        public int StockTotal { get; private set; }

        /// <summary>
        /// Page load event handler
        /// </summary>
        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                await LoadDashboardDataAsync();
            }
        }

        /// <summary>
        /// Initialize admin page - override from AdminBasePage
        /// </summary>
        protected override void InitializeAdminPage()
        {
            base.InitializeAdminPage();
            LogAdminAction("Dashboard Accessed");
        }

        /// <summary>
        /// Load dashboard statistics from services
        /// </summary>
        private async Task LoadDashboardDataAsync()
        {
            try
            {
                ShowLoadingPanel(true);
                
                Logger.Debug("Loading dashboard statistics for admin user {UserId}", CurrentUserId);

                // Load statistics from services
                var orderStatsTask = OrderService.GetStatisticsAsync();
                var offerStatsTask = OfferService.GetStatisticsAsync();
                var inventoryStatsTask = BookService.GetStatisticsAsync();

                // Wait for all tasks to complete
                await Task.WhenAll(orderStatsTask, offerStatsTask, inventoryStatsTask);

                var orderStats = await orderStatsTask;
                var offerStats = await offerStatsTask;
                var inventoryStats = await inventoryStatsTask;

                // Set properties
                PendingOrders = orderStats.PendingOrders;
                PastDueOrders = orderStats.PastDueOrders;
                OrdersThisMonth = orderStats.OrdersThisMonth;
                OrdersTotal = orderStats.OrdersTotal;

                PendingOffers = offerStats.PendingOffers;
                OffersThisMonth = offerStats.OffersThisMonth;
                OffersTotal = offerStats.OffersTotal;

                OutOfStock = inventoryStats.OutOfStock;
                LowStock = inventoryStats.LowStock;
                StockTotal = inventoryStats.StockTotal;

                // Bind data to controls
                BindDashboardData();

                ShowLoadingPanel(false);
                
                Logger.Info("Dashboard statistics loaded successfully for admin user {UserId}", CurrentUserId);
                LogAdminAction("Dashboard Statistics Loaded", string.Format("Orders: {0}, Offers: {1}, Inventory: {2}", OrdersTotal, OffersTotal, StockTotal));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading dashboard statistics for admin user {UserId}", CurrentUserId);
                ShowError("Unable to load dashboard statistics. Please try again.");
                ShowLoadingPanel(false);
            }
        }

        /// <summary>
        /// Bind dashboard data to UI controls
        /// </summary>
        private void BindDashboardData()
        {
            try
            {
                // Bind order statistics
                lblPendingOrders.Text = PendingOrders == 1 
                    ? "There is 1 pending order" 
                    : string.Format("There are {0} pending orders", PendingOrders);
                lnkPendingOrders.NavigateUrl = string.Format("~/Admin/Orders.aspx?status={0}", (int)OrderStatus.Pending);

                lblPastDueOrders.Text = PastDueOrders == 1 
                    ? "There is 1 past-due order" 
                    : string.Format("There are {0} past-due orders", PastDueOrders);
                lnkPastDueOrders.NavigateUrl = "~/Admin/Orders.aspx";

                lblOrdersThisMonth.Text = OrdersThisMonth == 1 
                    ? "There has been 1 order placed this month" 
                    : string.Format("There have been {0} orders placed this month", OrdersThisMonth);
                lnkOrdersThisMonth.NavigateUrl = string.Format("~/Admin/Orders.aspx?dateFrom={0}", DateTime.UtcNow.StartOfMonth().ToString("yyyy-MM-dd"));

                lblOrdersTotal.Text = OrdersTotal == 1 
                    ? "There has been 1 order placed in total" 
                    : string.Format("There have been {0} orders placed in total", OrdersTotal);
                lnkOrdersTotal.NavigateUrl = "~/Admin/Orders.aspx";

                // Bind offer statistics
                lblPendingOffers.Text = PendingOffers == 1 
                    ? "There is 1 pending offer" 
                    : string.Format("There are {0} pending offers", PendingOffers);
                lnkPendingOffers.NavigateUrl = string.Format("~/Admin/Offers.aspx?status={0}", (int)OfferStatus.PendingApproval);

                lblOffersThisMonth.Text = OffersThisMonth == 1 
                    ? "There has been 1 offer made this month" 
                    : string.Format("There have been {0} offers made this month", OffersThisMonth);
                lnkOffersThisMonth.NavigateUrl = string.Format("~/Admin/Offers.aspx?dateFrom={0}", DateTime.UtcNow.StartOfMonth().ToString("yyyy-MM-dd"));

                lblOffersTotal.Text = OffersTotal == 1 
                    ? "There has been 1 offer made in total" 
                    : string.Format("There have been {0} offers made in total", OffersTotal);
                lnkOffersTotal.NavigateUrl = "~/Admin/Offers.aspx";

                // Bind inventory statistics
                lblOutOfStock.Text = OutOfStock == 1 
                    ? "1 book is out of stock" 
                    : string.Format("{0} books are out of stock", OutOfStock);
                lnkOutOfStock.NavigateUrl = "~/Admin/Inventory.aspx?lowStock=true";

                lblLowStock.Text = LowStock == 1 
                    ? "1 book is low in stock" 
                    : string.Format("{0} books are low in stock", LowStock);
                lnkLowStock.NavigateUrl = "~/Admin/Inventory.aspx?lowStock=true";

                lblStockTotal.Text = StockTotal == 1 
                    ? "There is a total of 1 book in inventory" 
                    : string.Format("There is a total of {0} books in inventory", StockTotal);
                lnkStockTotal.NavigateUrl = "~/Admin/Inventory.aspx";

                Logger.Debug("Dashboard data bound successfully for admin user {UserId}", CurrentUserId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error binding dashboard data for admin user {UserId}", CurrentUserId);
                ShowError("Error displaying dashboard data.");
            }
        }

        /// <summary>
        /// Show or hide the loading panel
        /// </summary>
        private void ShowLoadingPanel(bool show)
        {
            pnlLoading.Visible = show;
        }

        /// <summary>
        /// Override ShowError to use the error panel
        /// </summary>
        protected new void ShowError(string message)
        {
            lblError.Text = message;
            pnlError.Visible = true;
            base.ShowError(message);
        }
    }
}