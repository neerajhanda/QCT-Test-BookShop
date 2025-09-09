<%@ Page Title="Book Details" Language="C#" MasterPageFile="~/Admin/AdminMaster.Master" AutoEventWireup="true" CodeBehind="InventoryDetails.aspx.cs" Inherits="Bookstore.WebForms.Admin.InventoryDetails" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex m-3">
        <h5 class="me-auto">Book Details</h5>
    </div>

    <div class="row justify-content-center">
        <div class="col-4 mb-3">
            <asp:Image ID="CoverImage" runat="server" CssClass="img-fluid img-thumbnail" AlternateText="Book Cover" />
        </div>

        <div class="col-4 mb-3">
            <table class="table">
                <tbody>
                    <tr>
                        <th scope="row">Name</th>
                        <td><asp:Label ID="NameLabel" runat="server" /></td>
                    </tr>
                    <tr>
                        <th scope="row">Author</th>
                        <td><asp:Label ID="AuthorLabel" runat="server" /></td>
                    </tr>
                    <tr>
                        <th scope="row">Genre</th>
                        <td><asp:Label ID="GenreLabel" runat="server" /></td>
                    </tr>
                    <tr>
                        <th scope="row">Price</th>
                        <td><asp:Label ID="PriceLabel" runat="server" /></td>
                    </tr>
                    <tr>
                        <th scope="row">Publisher</th>
                        <td><asp:Label ID="PublisherLabel" runat="server" /></td>
                    </tr>
                    <tr>
                        <th scope="row">ISBN</th>
                        <td><asp:Label ID="ISBNLabel" runat="server" /></td>
                    </tr>
                    <tr>
                        <th scope="row">Book Type</th>
                        <td><asp:Label ID="BookTypeLabel" runat="server" /></td>
                    </tr>
                    <tr>
                        <th scope="row">Condition</th>
                        <td><asp:Label ID="ConditionLabel" runat="server" /></td>
                    </tr>
                    <tr>
                        <th scope="row">Quantity</th>
                        <td><asp:Label ID="QuantityLabel" runat="server" /></td>
                    </tr>
                    <tr>
                        <th scope="row">Description</th>
                        <td><asp:Label ID="SummaryLabel" runat="server" /></td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>

    <div class="row justify-content-center">
        <div class="col-8">
            <asp:HyperLink ID="BackLink" runat="server" NavigateUrl="~/Admin/Inventory.aspx" CssClass="btn btn-primary" Text="Back" />
            <asp:HyperLink ID="EditLink" runat="server" CssClass="btn btn-secondary" Text="Edit" />
        </div>
    </div>
</asp:Content>