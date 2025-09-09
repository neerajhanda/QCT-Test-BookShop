<%@ Page Title="Access Denied" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Unauthorized.aspx.cs" Inherits="Bookstore.WebForms.Unauthorized" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="row">
            <div class="col-md-8 col-md-offset-2">
                <div class="unauthorized-page">
                    <h1 class="unauthorized-title">Access Denied</h1>
                    <h2 class="unauthorized-subtitle">403 - Forbidden</h2>
                    
                    <div class="unauthorized-message">
                        <p>You don't have permission to access this resource.</p>
                        <asp:Literal ID="UnauthorizedDetailsLiteral" runat="server" />
                    </div>
                    
                    <asp:Panel ID="LoginPanel" runat="server" Visible="false" CssClass="login-suggestion">
                        <div class="alert alert-info">
                            <h4><i class="fa fa-info-circle"></i> Not logged in?</h4>
                            <p>You may need to log in to access this page.</p>
                            <asp:HyperLink ID="LoginLink" runat="server" NavigateUrl="~/Login.aspx" CssClass="btn btn-info">
                                <i class="fa fa-sign-in"></i> Log In
                            </asp:HyperLink>
                        </div>
                    </asp:Panel>
                    
                    <asp:Panel ID="InsufficientRolePanel" runat="server" Visible="false" CssClass="role-suggestion">
                        <div class="alert alert-warning">
                            <h4><i class="fa fa-exclamation-triangle"></i> Insufficient Privileges</h4>
                            <p>Your current account doesn't have the required permissions to access this resource.</p>
                            <p>If you believe this is an error, please contact an administrator.</p>
                        </div>
                    </asp:Panel>
                    
                    <div class="unauthorized-actions">
                        <asp:HyperLink ID="HomeLink" runat="server" NavigateUrl="~/" CssClass="btn btn-primary">
                            <i class="fa fa-home"></i> Return to Home
                        </asp:HyperLink>
                        
                        <asp:Button ID="BackButton" runat="server" Text="Go Back" CssClass="btn btn-default" OnClientClick="history.back(); return false;" />
                        
                        <asp:Panel ID="LogoutPanel" runat="server" Visible="false" style="display: inline;">
                            <asp:Button ID="LogoutButton" runat="server" Text="Log Out" CssClass="btn btn-warning" OnClick="LogoutButton_Click" />
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>
    </div>
    
    <style>
        .unauthorized-page {
            text-align: center;
            padding: 50px 0;
        }
        
        .unauthorized-title {
            font-size: 3em;
            color: #f0ad4e;
            margin-bottom: 10px;
        }
        
        .unauthorized-subtitle {
            font-size: 1.5em;
            color: #666;
            margin-bottom: 30px;
        }
        
        .unauthorized-message {
            font-size: 1.2em;
            margin-bottom: 30px;
        }
        
        .login-suggestion, .role-suggestion {
            text-align: left;
            margin: 20px 0;
        }
        
        .unauthorized-actions {
            margin-top: 30px;
        }
        
        .unauthorized-actions .btn {
            margin: 0 10px;
        }
        
        .alert {
            text-align: left;
        }
        
        .requested-resource {
            font-family: monospace;
            background-color: #f5f5f5;
            padding: 10px;
            border-radius: 4px;
            margin: 10px 0;
            word-break: break-all;
        }
    </style>
</asp:Content>