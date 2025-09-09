using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bookstore.Domain.Addresses;

namespace Bookstore.WebForms
{
    public partial class Address : BasePage
    {
        public IAddressService AddressService { get; set; }

        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                await LoadAddressesAsync();
                SetCreateAddressReturnUrl();
            }
        }

        private async Task LoadAddressesAsync()
        {
            try
            {
                var userSub = GetUserSub();
                if (string.IsNullOrEmpty(userSub))
                {
                    Response.Redirect("~/Login.aspx?returnUrl=" + Server.UrlEncode(Request.Url.ToString()));
                    return;
                }

                var addresses = await AddressService.GetAddressesAsync(userSub);
                var addressList = addresses.ToList();

                var addressViewModels = addressList.Select(x => new
                {
                    Id = x.Id,
                    AddressLine1 = x.AddressLine1,
                    AddressLine2 = x.AddressLine2,
                    City = x.City,
                    State = x.State,
                    Country = x.Country,
                    ZipCode = x.ZipCode
                }).ToList();

                AddressGridView.DataSource = addressViewModels;
                AddressGridView.DataBind();
            }
            catch (Exception ex)
            {
                ShowError("An error occurred while loading your addresses. Please try again.");
            }
        }

        private void SetCreateAddressReturnUrl()
        {
            var returnUrl = Server.UrlEncode(Request.Url.ToString());
            CreateAddressLink.NavigateUrl = $"~/AddressCreateUpdate.aspx?returnUrl={returnUrl}";
        }        
        protected async void AddressGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteAddress")
            {
                await DeleteAddressAsync(Convert.ToInt32(e.CommandArgument));
            }
        }

        private async Task DeleteAddressAsync(int addressId)
        {
            try
            {
                var userSub = GetUserSub();
                if (string.IsNullOrEmpty(userSub))
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }

                var dto = new DeleteAddressDto(addressId, userSub);
                await AddressService.DeleteAddressAsync(dto);
                
                ShowSuccess("Address deleted");
                await LoadAddressesAsync();
            }
            catch (Exception ex)
            {
                ShowError("An error occurred while deleting the address. Please try again.");
            }
        }

        private string GetUserSub()
        {
            return CurrentUserId;
        }
    }
}