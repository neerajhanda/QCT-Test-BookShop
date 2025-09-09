<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Admin/AdminMaster.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="Bookstore.WebForms.Admin.Dashboard" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex m-3">
        <h5 class="me-auto">Dashboard</h5>
    </div>

    <div class="row row-cols-3">
        <!-- Orders Card -->
        <div class="col">
            <div class="card mx-3">
                <div class="card-header">Orders</div>
                <ul class="list-group list-group-flush">
                    <li class="list-group-item d-flex justify-content-between">
                        <asp:Label ID="lblPendingOrders" runat="server" />
                        <asp:HyperLink ID="lnkPendingOrders" runat="server" Text="View" />
                    </li>
                    <li class="list-group-item d-flex justify-content-between">
                        <asp:Label ID="lblPastDueOrders" runat="server" />
                        <asp:HyperLink ID="lnkPastDueOrders" runat="server" Text="View" />
                    </li>
                    <li class="list-group-item d-flex justify-content-between">
                        <asp:Label ID="lblOrdersThisMonth" runat="server" />
                        <asp:HyperLink ID="lnkOrdersThisMonth" runat="server" Text="View" />
                    </li>
                    <li class="list-group-item d-flex justify-content-between">
                        <asp:Label ID="lblOrdersTotal" runat="server" />
                        <asp:HyperLink ID="lnkOrdersTotal" runat="server" Text="View" />
                    </li>
                </ul>
            </div>
        </div>

        <!-- Offers Card -->
        <div class="col">
            <div class="card mx-3">
                <div class="card-header">Offers</div>
                <ul class="list-group list-group-flush">
                    <li class="list-group-item d-flex justify-content-between">
                        <asp:Label ID="lblPendingOffers" runat="server" />
                        <asp:HyperLink ID="lnkPendingOffers" runat="server" Text="View" />
                    </li>
                    <li class="list-group-item d-flex justify-content-between">
                        <asp:Label ID="lblOffersThisMonth" runat="server" />
                        <asp:HyperLink ID="lnkOffersThisMonth" runat="server" Text="View" />
                    </li>
                    <li class="list-group-item d-flex justify-content-between">
                        <asp:Label ID="lblOffersTotal" runat="server" />
                        <asp:HyperLink ID="lnkOffersTotal" runat="server" Text="View" />
                    </li>
                </ul>
            </div>
        </div>

        <!-- Inventory Card -->
        <div class="col">
            <div class="card mx-3">
                <div class="card-header">Inventory</div>
                <ul class="list-group list-group-flush">
                    <li class="list-group-item d-flex justify-content-between">
                        <asp:Label ID="lblOutOfStock" runat="server" />
                        <asp:HyperLink ID="lnkOutOfStock" runat="server" Text="View" />
                    </li>
                    <li class="list-group-item d-flex justify-content-between">
                        <asp:Label ID="lblLowStock" runat="server" />
                        <asp:HyperLink ID="lnkLowStock" runat="server" Text="View" />
                    </li>
                    <li class="list-group-item d-flex justify-content-between">
                        <asp:Label ID="lblStockTotal" runat="server" />
                        <asp:HyperLink ID="lnkStockTotal" runat="server" Text="View" />
                    </li>
                </ul>
            </div>
        </div>
    </div>

    <!-- Loading Panel -->
    <asp:Panel ID="pnlLoading" runat="server" CssClass="text-center mt-4" Visible="false">
        <div class="spinner-border" role="status">
            <span class="visually-hidden">Loading...</span>
        </div>
        <p class="mt-2">Loading dashboard data...</p>
    </asp:Panel>

    <!-- Error Panel -->
    <asp:Panel ID="pnlError" runat="server" CssClass="alert alert-danger mt-4" Visible="false">
        <asp:Label ID="lblError" runat="server" />
    </asp:Panel>
</asp:Content>