<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SearchFilterControl.ascx.cs" Inherits="Bookstore.WebForms.Controls.SearchFilterControl" %>

<div class="search-filter-container">
    <div class="row">
        <div class="col-7">
            <asp:TextBox ID="SearchTextBox" runat="server" 
                        CssClass="form-control" 
                        placeholder="Search for books..." 
                        Text="" />
        </div>
        
        <div class="col-4">
            <asp:DropDownList ID="SortByDropDown" runat="server" CssClass="form-select">
                <asp:ListItem Value="Name" Text="Name" />
                <asp:ListItem Value="PriceAsc" Text="Price (ascending)" />
                <asp:ListItem Value="PriceDesc" Text="Price (descending)" />
                <asp:ListItem Value="Author" Text="Author" />
                <asp:ListItem Value="Genre" Text="Genre" />
            </asp:DropDownList>
        </div>
        
        <div class="col-1">
            <asp:Button ID="SearchButton" runat="server" 
                       CssClass="btn btn-primary" 
                       Text="Search" 
                       OnClick="SearchButton_Click" />
        </div>
    </div>
    
    <asp:Panel ID="AdvancedFiltersPanel" runat="server" Visible="false" CssClass="advanced-filters mt-3">
        <div class="row">
            <div class="col-md-3">
                <label for="<%= GenreDropDown.ClientID %>" class="form-label">Genre:</label>
                <asp:DropDownList ID="GenreDropDown" runat="server" CssClass="form-select">
                    <asp:ListItem Value="" Text="All Genres" />
                </asp:DropDownList>
            </div>
            
            <div class="col-md-3">
                <label for="<%= AuthorTextBox.ClientID %>" class="form-label">Author:</label>
                <asp:TextBox ID="AuthorTextBox" runat="server" CssClass="form-control" placeholder="Author name" />
            </div>
            
            <div class="col-md-3">
                <label for="<%= MinPriceTextBox.ClientID %>" class="form-label">Min Price:</label>
                <asp:TextBox ID="MinPriceTextBox" runat="server" CssClass="form-control" TextMode="Number" step="0.01" />
            </div>
            
            <div class="col-md-3">
                <label for="<%= MaxPriceTextBox.ClientID %>" class="form-label">Max Price:</label>
                <asp:TextBox ID="MaxPriceTextBox" runat="server" CssClass="form-control" TextMode="Number" step="0.01" />
            </div>
        </div>
        
        <div class="row mt-2">
            <div class="col-md-3">
                <asp:CheckBox ID="InStockOnlyCheckBox" runat="server" Text="In Stock Only" CssClass="form-check-input" />
            </div>
            
            <div class="col-md-3">
                <asp:Button ID="ClearFiltersButton" runat="server" 
                           CssClass="btn btn-secondary" 
                           Text="Clear Filters" 
                           OnClick="ClearFiltersButton_Click" />
            </div>
        </div>
    </asp:Panel>
    
    <div class="mt-2">
        <asp:LinkButton ID="ToggleAdvancedButton" runat="server" 
                       CssClass="btn btn-link btn-sm" 
                       Text="Show Advanced Filters" 
                       OnClick="ToggleAdvancedButton_Click" />
    </div>
</div>