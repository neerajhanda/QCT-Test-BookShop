<%@ Page Title="Create / Update Address" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AddressCreateUpdate.aspx.cs" Inherits="Bookstore.WebForms.AddressCreateUpdate" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="panel-body">
        <h1>Create / Update Address</h1>
        <br />

        <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="text-danger" />

        <div class="form-group">
            <asp:Label ID="AddressLine1Label" runat="server" Text="Address Line 1" AssociatedControlID="AddressLine1TextBox" />
            <asp:TextBox ID="AddressLine1TextBox" runat="server" CssClass="form-control" />
            <asp:RequiredFieldValidator ID="AddressLine1Required" runat="server" 
                ControlToValidate="AddressLine1TextBox" 
                ErrorMessage="Address Line 1 is required" 
                CssClass="text-danger" 
                Display="Dynamic" />
        </div>

        <div class="form-group">
            <asp:Label ID="AddressLine2Label" runat="server" Text="Address Line 2" AssociatedControlID="AddressLine2TextBox" />
            <asp:TextBox ID="AddressLine2TextBox" runat="server" CssClass="form-control" />
        </div>

        <div class="form-group">
            <asp:Label ID="CityLabel" runat="server" Text="City" AssociatedControlID="CityTextBox" />
            <asp:TextBox ID="CityTextBox" runat="server" CssClass="form-control" />
            <asp:RequiredFieldValidator ID="CityRequired" runat="server" 
                ControlToValidate="CityTextBox" 
                ErrorMessage="City is required" 
                CssClass="text-danger" 
                Display="Dynamic" />
        </div>

        <div class="form-group">
            <asp:Label ID="StateLabel" runat="server" Text="State" AssociatedControlID="StateDropDownList" />
            <asp:DropDownList ID="StateDropDownList" runat="server" CssClass="form-control" />
            <asp:RequiredFieldValidator ID="StateRequired" runat="server" 
                ControlToValidate="StateDropDownList" 
                ErrorMessage="State is required" 
                CssClass="text-danger" 
                Display="Dynamic" 
                InitialValue="" />
        </div>

        <div class="form-group">
            <asp:Label ID="CountryLabel" runat="server" Text="Country" AssociatedControlID="CountryTextBox" />
            <asp:TextBox ID="CountryTextBox" runat="server" CssClass="form-control" />
            <asp:RequiredFieldValidator ID="CountryRequired" runat="server" 
                ControlToValidate="CountryTextBox" 
                ErrorMessage="Country is required" 
                CssClass="text-danger" 
                Display="Dynamic" />
        </div>

        <div class="form-group">
            <asp:Label ID="ZipCodeLabel" runat="server" Text="Zipcode" AssociatedControlID="ZipCodeTextBox" />
            <asp:TextBox ID="ZipCodeTextBox" runat="server" CssClass="form-control" />
            <asp:RequiredFieldValidator ID="ZipCodeRequired" runat="server" 
                ControlToValidate="ZipCodeTextBox" 
                ErrorMessage="Zipcode is required" 
                CssClass="text-danger" 
                Display="Dynamic" />
        </div>

        <center>
            <div class="form-group">
                <asp:Button ID="SaveButton" runat="server" Text="Save" CssClass="btn" OnClick="SaveButton_Click" />
                <asp:HyperLink ID="CancelLink" runat="server" CssClass="btn" Text="Cancel" />
            </div>
        </center>
    </div>
</asp:Content>