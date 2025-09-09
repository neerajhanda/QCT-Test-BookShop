using Bookstore.Domain.Orders;
using Bookstore.WebForms.Models;
using System;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Bookstore.WebForms.Admin
{
    public partial class Orders : AdminBasePage
    {
        public IOrderService OrderService { get; set; }

        private AdminOrderIndexViewModel _viewModel;
        private const int DefaultPageSize = 10;

        protected async void Page_Load(object sender, EventArgs e)
        {
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
                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error initializing admin orders page");
                ShowErrorMessage("An error occurred while loading the orders. Please try again.");
            }
        }

        private void PopulateOrderStatusDropDown()
        {
            ddlOrderStatus.Items.Clear();
            ddlOrderStatus.Items.Add(new ListItem("All Order Statuses", ""));
            
            foreach (OrderStatus status in Enum.GetValues(typeof(OrderStatus)))
            {
                ddlOrderStatus.Items.Add(new ListItem(status.ToString(), ((int)status).ToString()));
            }
        }

        private async Task LoadOrdersAsync()
        {
            try
            {
                var filters = GetCurrentFilters();
                var pageIndex = GetCurrentPageIndex();
                
                var orders = await OrderService.GetOrdersAsync(filters, pageIndex, DefaultPageSize);
                _viewModel = new AdminOrderIndexViewModel(orders, filters);
                
                BindOrdersGrid();
                BindPaginationControls();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading orders data");
                ShowErrorMessage("An error occurred while loading orders data. Please try again.");
            }
        }

        private OrderFilters GetCurrentFilters()
        {
            var filters = new OrderFilters();

            // Order Status Filter
            if (!string.IsNullOrEmpty(ddlOrderStatus.SelectedValue))
            {
                if (Enum.TryParse<OrderStatus>(ddlOrderStatus.SelectedValue, out var status))
                {
                    filters.OrderStatusFilter = status;
                }
            }

            // Date From Filter
            if (!string.IsNullOrEmpty(txtOrderDateFrom.Text))
            {
                if (DateTime.TryParse(txtOrderDateFrom.Text, out var dateFrom))
                {
                    filters.OrderDateFromFilter = dateFrom;
                }
            }

            // Date To Filter
            if (!string.IsNullOrEmpty(txtOrderDateTo.Text))
            {
                if (DateTime.TryParse(txtOrderDateTo.Text, out var dateTo))
                {
                    filters.OrderDateToFilter = dateTo;
                }
            }

            return filters;
        }

        private int GetCurrentPageIndex()
        {
            if (int.TryParse(Request.QueryString["page"], out var page) && page > 0)
            {
                return page;
            }
            return 1;
        }

        private void BindOrdersGrid()
        {
            if (_viewModel?.Items != null)
            {
                gvOrders.DataSource = _viewModel.Items;
                gvOrders.DataBind();
            }
        }

        private void BindPaginationControls()
        {
            if (_viewModel != null)
            {
                ucPaginationTop.SetPaginationData(_viewModel.CurrentPage, _viewModel.TotalPages, 
                    _viewModel.HasPreviousPage, _viewModel.HasNextPage);
                ucPaginationBottom.SetPaginationData(_viewModel.CurrentPage, _viewModel.TotalPages, 
                    _viewModel.HasPreviousPage, _viewModel.HasNextPage);
            }
        }

        private string GetCurrentUrl()
        {
            var filters = GetCurrentFilters();
            var url = "Orders.aspx?";
            
            if (filters.OrderStatusFilter.HasValue)
            {
                url += $"status={filters.OrderStatusFilter.Value}&";
            }
            
            if (filters.OrderDateFromFilter.HasValue)
            {
                url += $"dateFrom={filters.OrderDateFromFilter.Value:yyyy-MM-dd}&";
            }
            
            if (filters.OrderDateToFilter.HasValue)
            {
                url += $"dateTo={filters.OrderDateToFilter.Value:yyyy-MM-dd}&";
            }
            
            return url.TrimEnd('&', '?');
        }

        protected async void FilterChanged(object sender, EventArgs e)
        {
            await LoadOrdersAsync();
        }

        protected async void btnFilter_Click(object sender, EventArgs e)
        {
            await LoadOrdersAsync();
        }

        protected async void btnClear_Click(object sender, EventArgs e)
        {
            ddlOrderStatus.SelectedIndex = 0;
            txtOrderDateFrom.Text = string.Empty;
            txtOrderDateTo.Text = string.Empty;
            
            await LoadOrdersAsync();
        }

        protected void gvOrders_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewDetails")
            {
                var orderId = e.CommandArgument.ToString();
                Response.Redirect($"OrderDetails.aspx?id={orderId}");
            }
        }

        private void ShowErrorMessage(string message)
        {
            // This would typically show an error message to the user
            // For now, we'll use a simple approach
            ClientScript.RegisterStartupScript(this.GetType(), "error", 
                $"alert('{message.Replace("'", "\\'")}');", true);
        }
    }
}