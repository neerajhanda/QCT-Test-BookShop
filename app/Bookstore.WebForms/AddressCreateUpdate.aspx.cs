using System;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bookstore.Domain.Addresses;

namespace Bookstore.WebForms
{
    public partial class AddressCreateUpdate : BasePage
    {
        public IAddressService AddressService { get; set; }

        private int? AddressId
        {
            get
            {
                var idParam = Request.QueryString["id"];
                return int.TryParse(idParam, out int id) ? id : (int?)null;
            }
        }

        private string ReturnUrl => Request.QueryString["returnUrl"] ?? "~/Address.aspx";

        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PopulateStatesDropDown();
                CancelLink.NavigateUrl = ReturnUrl;
                
                if (AddressId.HasValue)
                {
                    await LoadAddressAsync();
                }
            }
        }

        private void PopulateStatesDropDown()
        {
            StateDropDownList.Items.Clear();
            StateDropDownList.Items.Add(new ListItem("", ""));
            StateDropDownList.Items.Add(new ListItem("Alabama", "AL"));
            StateDropDownList.Items.Add(new ListItem("Alaska", "AK"));
            StateDropDownList.Items.Add(new ListItem("Arizona", "AZ"));
            StateDropDownList.Items.Add(new ListItem("Arkansas", "AR"));
            StateDropDownList.Items.Add(new ListItem("California", "CA"));
            StateDropDownList.Items.Add(new ListItem("Colorado", "CO"));
            StateDropDownList.Items.Add(new ListItem("Connecticut", "CT"));
            StateDropDownList.Items.Add(new ListItem("Delaware", "DE"));
            StateDropDownList.Items.Add(new ListItem("District Of Columbia", "DC"));
            StateDropDownList.Items.Add(new ListItem("Florida", "FL"));
            StateDropDownList.Items.Add(new ListItem("Georgia", "GA"));
            StateDropDownList.Items.Add(new ListItem("Hawaii", "HI"));
            StateDropDownList.Items.Add(new ListItem("Idaho", "ID"));
            StateDropDownList.Items.Add(new ListItem("Illinois", "IL"));
            StateDropDownList.Items.Add(new ListItem("Indiana", "IN"));
            StateDropDownList.Items.Add(new ListItem("Iowa", "IA"));
            StateDropDownList.Items.Add(new ListItem("Kansas", "KS"));
            StateDropDownList.Items.Add(new ListItem("Kentucky", "KY"));
            StateDropDownList.Items.Add(new ListItem("Louisiana", "LA"));
            StateDropDownList.Items.Add(new ListItem("Maine", "ME"));
            StateDropDownList.Items.Add(new ListItem("Maryland", "MD"));
            StateDropDownList.Items.Add(new ListItem("Massachusetts", "MA"));
            StateDropDownList.Items.Add(new ListItem("Michigan", "MI"));
            StateDropDownList.Items.Add(new ListItem("Minnesota", "MN"));
            StateDropDownList.Items.Add(new ListItem("Mississippi", "MS"));
            StateDropDownList.Items.Add(new ListItem("Missouri", "MO"));
            StateDropDownList.Items.Add(new ListItem("Montana", "MT"));
            StateDropDownList.Items.Add(new ListItem("Nebraska", "NE"));
            StateDropDownList.Items.Add(new ListItem("Nevada", "NV"));
            StateDropDownList.Items.Add(new ListItem("New Hampshire", "NH"));
            StateDropDownList.Items.Add(new ListItem("New Jersey", "NJ"));
            StateDropDownList.Items.Add(new ListItem("New Mexico", "NM"));
            StateDropDownList.Items.Add(new ListItem("New York", "NY"));
            StateDropDownList.Items.Add(new ListItem("North Carolina", "NC"));
            StateDropDownList.Items.Add(new ListItem("North Dakota", "ND"));
            StateDropDownList.Items.Add(new ListItem("Ohio", "OH"));
            StateDropDownList.Items.Add(new ListItem("Oklahoma", "OK"));
            StateDropDownList.Items.Add(new ListItem("Oregon", "OR"));
            StateDropDownList.Items.Add(new ListItem("Pennsylvania", "PA"));
            StateDropDownList.Items.Add(new ListItem("Rhode Island", "RI"));
            StateDropDownList.Items.Add(new ListItem("South Carolina", "SC"));
            StateDropDownList.Items.Add(new ListItem("South Dakota", "SD"));
            StateDropDownList.Items.Add(new ListItem("Tennessee", "TN"));
            StateDropDownList.Items.Add(new ListItem("Texas", "TX"));
            StateDropDownList.Items.Add(new ListItem("Utah", "UT"));
            StateDropDownList.Items.Add(new ListItem("Vermont", "VT"));
            StateDropDownList.Items.Add(new ListItem("Virginia", "VA"));
            StateDropDownList.Items.Add(new ListItem("Washington", "WA"));
            StateDropDownList.Items.Add(new ListItem("West Virginia", "WV"));
            StateDropDownList.Items.Add(new ListItem("Wisconsin", "WI"));
            StateDropDownList.Items.Add(new ListItem("Wyoming", "WY"));
        }

        private async Task LoadAddressAsync()
        {
            try
            {
                var userSub = GetUserSub();
                if (string.IsNullOrEmpty(userSub))
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }

                var address = await AddressService.GetAddressAsync(userSub, AddressId.Value);
                if (address != null)
                {
                    AddressLine1TextBox.Text = address.AddressLine1;
                    AddressLine2TextBox.Text = address.AddressLine2;
                    CityTextBox.Text = address.City;
                    StateDropDownList.SelectedValue = address.State;
                    CountryTextBox.Text = address.Country;
                    ZipCodeTextBox.Text = address.ZipCode;
                }
            }
            catch (Exception ex)
            {
                ShowError("An error occurred while loading the address. Please try again.");
                Response.Redirect(ReturnUrl);
            }
        }

        protected async void SaveButton_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                await SaveAddressAsync();
            }
        }

        private async Task SaveAddressAsync()
        {
            try
            {
                var userSub = GetUserSub();
                if (string.IsNullOrEmpty(userSub))
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }

                if (AddressId.HasValue)
                {
                    // Update existing address
                    var dto = new UpdateAddressDto(
                        AddressId.Value,
                        AddressLine1TextBox.Text,
                        AddressLine2TextBox.Text,
                        CityTextBox.Text,
                        StateDropDownList.SelectedValue,
                        CountryTextBox.Text,
                        ZipCodeTextBox.Text,
                        userSub);

                    await AddressService.UpdateAddressAsync(dto);
                }
                else
                {
                    // Create new address
                    var dto = new CreateAddressDto(
                        AddressLine1TextBox.Text,
                        AddressLine2TextBox.Text,
                        CityTextBox.Text,
                        StateDropDownList.SelectedValue,
                        CountryTextBox.Text,
                        ZipCodeTextBox.Text,
                        userSub);

                    await AddressService.CreateAddressAsync(dto);
                }

                Response.Redirect(ReturnUrl);
            }
            catch (Exception ex)
            {
                ShowError("An error occurred while saving the address. Please try again.");
            }
        }

        private string GetUserSub()
        {
            return CurrentUserId;
        }
    }
}