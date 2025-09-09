<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AdminNavigationControl.ascx.cs" Inherits="Bookstore.WebForms.Controls.AdminNavigationControl" %>

<div class="admin-navigation">
    <!-- Admin Breadcrumb Navigation -->
    <nav aria-label="Admin breadcrumb">
        <ol class="breadcrumb">
            <li class="breadcrumb-item">
                <a href="~/Admin/Dashboard.aspx" runat="server">
                    <i class="bi bi-house-door"></i> Admin Home
                </a>
            </li>
            <asp:PlaceHolder ID="BreadcrumbPlaceHolder" runat="server">
            </asp:PlaceHolder>
        </ol>
    </nav>

    <!-- Admin Quick Actions -->
    <div class="admin-quick-actions mb-3">
        <div class="btn-group" role="group" aria-label="Admin quick actions">
            <asp:LinkButton ID="DashboardButton" runat="server" 
                CssClass="btn btn-outline-primary btn-sm" 
                OnClick="NavigateToPage" 
                CommandArgument="~/Admin/Dashboard.aspx">
                <i class="bi bi-speedometer2"></i> Dashboard
            </asp:LinkButton>
            
            <asp:LinkButton ID="OrdersButton" runat="server" 
                CssClass="btn btn-outline-primary btn-sm" 
                OnClick="NavigateToPage" 
                CommandArgument="~/Admin/Orders.aspx">
                <i class="bi bi-cart-check"></i> Orders
            </asp:LinkButton>
            
            <asp:LinkButton ID="InventoryButton" runat="server" 
                CssClass="btn btn-outline-primary btn-sm" 
                OnClick="NavigateToPage" 
                CommandArgument="~/Admin/Inventory.aspx">
                <i class="bi bi-box-seam"></i> Inventory
            </asp:LinkButton>
            
            <asp:LinkButton ID="OffersButton" runat="server" 
                CssClass="btn btn-outline-primary btn-sm" 
                OnClick="NavigateToPage" 
                CommandArgument="~/Admin/Offers.aspx">
                <i class="bi bi-tag"></i> Offers
            </asp:LinkButton>
            
            <asp:LinkButton ID="ReferenceDataButton" runat="server" 
                CssClass="btn btn-outline-primary btn-sm" 
                OnClick="NavigateToPage" 
                CommandArgument="~/Admin/ReferenceData.aspx">
                <i class="bi bi-gear"></i> Reference Data
            </asp:LinkButton>
        </div>
    </div>

    <!-- Admin Status Bar -->
    <div class="admin-status-bar alert alert-info py-2">
        <div class="row align-items-center">
            <div class="col-md-6">
                <small>
                    <i class="bi bi-person-badge"></i>
                    Logged in as: <strong><asp:Literal ID="AdminUserLiteral" runat="server" /></strong>
                </small>
            </div>
            <div class="col-md-6 text-end">
                <small>
                    <i class="bi bi-clock"></i>
                    Last activity: <asp:Literal ID="LastActivityLiteral" runat="server" />
                </small>
            </div>
        </div>
    </div>
</div>

<style>
    .admin-navigation {
        margin-bottom: 1rem;
    }
    
    .admin-quick-actions .btn {
        margin-right: 0.25rem;
    }
    
    .admin-status-bar {
        border-left: 4px solid #0d6efd;
    }
    
    .breadcrumb-item a {
        text-decoration: none;
    }
    
    .breadcrumb-item a:hover {
        text-decoration: underline;
    }
</style>