using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Bookstore.WebForms.Controls
{
    public partial class CartSummaryControl : UserControl
    {
        public event EventHandler<CartItemRemovedEventArgs> ItemRemoved;
        public event EventHandler CartUpdated;

        private CartSummaryData _cartData;

        public CartSummaryData CartData
        {
            get { return _cartData; }
            set 
            { 
                _cartData = value;
                BindCartData();
            }
        }

        public bool ShowLoginPrompt { get; set; } = false;

        public bool ShowItemActions { get; set; } = true;

        public int MaxDisplayItems { get; set; } = 5;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindCartData();
            }
        }

        private void BindCartData()
        {
            if (ShowLoginPrompt)
            {
                CartSummaryPanel.Visible = false;
                LoginPromptPanel.Visible = true;
                return;
            }

            LoginPromptPanel.Visible = false;
            CartSummaryPanel.Visible = true;

            if (_cartData == null || !_cartData.Items.Any())
            {
                EmptyCartPanel.Visible = true;
                CartItemsPanel.Visible = false;
                return;
            }

            EmptyCartPanel.Visible = false;
            CartItemsPanel.Visible = true;

            // Bind cart items (limit to MaxDisplayItems for summary view)
            var itemsToShow = _cartData.Items.Take(MaxDisplayItems).ToList();
            CartItemsRepeater.DataSource = itemsToShow;
            CartItemsRepeater.DataBind();

            // Update totals
            TotalPriceLiteral.Text = _cartData.TotalPrice.ToString("F2");
            
            var itemCount = _cartData.Items.Count();
            var displayText = itemCount == 1 ? "1 item" : $"{itemCount} items";
            if (itemCount > MaxDisplayItems)
            {
                displayText += $" (showing {MaxDisplayItems})";
            }
            ItemCountLiteral.Text = displayText;

            // Configure action buttons visibility
            if (!ShowItemActions)
            {
                // Hide remove buttons in repeater items
                foreach (RepeaterItem item in CartItemsRepeater.Items)
                {
                    var removeButton = item.FindControl("RemoveButton") as LinkButton;
                    if (removeButton != null)
                    {
                        removeButton.Visible = false;
                    }
                }
            }
        }

        protected void CartItemsRepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "RemoveItem")
            {
                int shoppingCartItemId = Convert.ToInt32(e.CommandArgument);
                OnItemRemoved(new CartItemRemovedEventArgs(shoppingCartItemId));
            }
        }

        public virtual void OnItemRemoved(CartItemRemovedEventArgs e)
        {
            ItemRemoved?.Invoke(this, e);
        }

        public virtual void OnCartUpdated(EventArgs e)
        {
            CartUpdated?.Invoke(this, e);
        }

        public void RefreshCart()
        {
            BindCartData();
        }

        public void ShowAsLoginRequired()
        {
            ShowLoginPrompt = true;
            BindCartData();
        }
    }

    public class CartSummaryData
    {
        public decimal TotalPrice => Items?.Sum(x => x.Price) ?? 0;
        public List<CartSummaryItem> Items { get; set; } = new List<CartSummaryItem>();
    }

    public class CartSummaryItem
    {
        public int ShoppingCartItemId { get; set; }
        public long BookId { get; set; }
        public string BookName { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int StockLevel { get; set; }
        public bool HasLowStockLevels => StockLevel <= 5 && StockLevel > 0;
        public bool IsOutOfStock => StockLevel <= 0;
    }

    public class CartItemRemovedEventArgs : EventArgs
    {
        public int ShoppingCartItemId { get; }

        public CartItemRemovedEventArgs(int shoppingCartItemId)
        {
            ShoppingCartItemId = shoppingCartItemId;
        }
    }
}