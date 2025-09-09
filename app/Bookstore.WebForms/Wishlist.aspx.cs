using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bookstore.Domain.Carts;

namespace Bookstore.WebForms
{
    public partial class Wishlist : BasePage
    {
        public IShoppingCartService ShoppingCartService { get; set; }

        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                await LoadWishlistAsync();
            }
        }

        private async Task LoadWishlistAsync()
        {
            try
            {
                // Check if ShoppingCartService is available
                if (ShoppingCartService == null)
                {
                    EmptyWishlistPanel.Visible = true;
                    WishlistPanel.Visible = false;
                    return;
                }

                var correlationId = GetShoppingCartCorrelationId();
                var shoppingCart = await ShoppingCartService.GetShoppingCartAsync(correlationId);
                
                if (shoppingCart == null)
                {
                    EmptyWishlistPanel.Visible = true;
                    WishlistPanel.Visible = false;
                    return;
                }

                var wishlistItems = shoppingCart.GetWishListItems().ToList();
                
                if (!wishlistItems.Any())
                {
                    EmptyWishlistPanel.Visible = true;
                    WishlistPanel.Visible = false;
                }
                else
                {
                    EmptyWishlistPanel.Visible = false;
                    WishlistPanel.Visible = true;
                    
                    var wishlistViewModels = wishlistItems.Select(x => new
                    {
                        ShoppingCartItemId = x.Id,
                        BookName = x.Book.Name,
                        ImageUrl = x.Book.CoverImageUrl,
                        Price = x.Book.Price
                    }).ToList();

                    WishlistGridView.DataSource = wishlistViewModels;
                    WishlistGridView.DataBind();
                }
            }
            catch (Exception ex)
            {
                ShowError("An error occurred while loading your wishlist. Please try again.");
            }
        }     
   protected async void WishlistGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int shoppingCartItemId = Convert.ToInt32(e.CommandArgument);
            
            if (e.CommandName == "MoveToCart")
            {
                await MoveToShoppingCartAsync(shoppingCartItemId);
            }
            else if (e.CommandName == "RemoveItem")
            {
                await RemoveFromWishlistAsync(shoppingCartItemId);
            }
        }

        protected async void MoveAllButton_Click(object sender, EventArgs e)
        {
            await MoveAllItemsToShoppingCartAsync();
        }

        private async Task MoveToShoppingCartAsync(int shoppingCartItemId)
        {
            try
            {
                var correlationId = GetShoppingCartCorrelationId();
                var dto = new MoveWishlistItemToShoppingCartDto(correlationId, shoppingCartItemId);
                
                await ShoppingCartService.MoveWishlistItemToShoppingCartAsync(dto);
                
                ShowSuccess("Item moved to shopping cart");
                await LoadWishlistAsync();
            }
            catch (Exception ex)
            {
                ShowError("An error occurred while moving the item to shopping cart. Please try again.");
            }
        }

        private async Task MoveAllItemsToShoppingCartAsync()
        {
            try
            {
                var correlationId = GetShoppingCartCorrelationId();
                var dto = new MoveAllWishlistItemsToShoppingCartDto(correlationId);
                
                await ShoppingCartService.MoveAllWishlistItemsToShoppingCartAsync(dto);
                
                ShowSuccess("All items moved to shopping cart");
                await LoadWishlistAsync();
            }
            catch (Exception ex)
            {
                ShowError("An error occurred while moving items to shopping cart. Please try again.");
            }
        }

        private async Task RemoveFromWishlistAsync(int shoppingCartItemId)
        {
            try
            {
                var correlationId = GetShoppingCartCorrelationId();
                var dto = new DeleteShoppingCartItemDto(correlationId, shoppingCartItemId);
                
                await ShoppingCartService.DeleteShoppingCartItemAsync(dto);
                
                ShowSuccess("Item removed from wishlist");
                await LoadWishlistAsync();
            }
            catch (Exception ex)
            {
                ShowError("An error occurred while removing the item from wishlist. Please try again.");
            }
        }

        private string GetShoppingCartCorrelationId()
        {
            // Use the centralized session manager for cart correlation ID
            return base.GetShoppingCartCorrelationId();
        }


    }
}