<%@ Page Title="Error" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="Bookstore.WebForms.Error" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="row">
            <div class="col-md-8 col-md-offset-2">
                <div class="error-page">
                    <h1 class="error-title">Oops! Something went wrong</h1>
                    <div class="error-message">
                        <p>We're sorry, but an unexpected error has occurred. Our team has been notified and is working to resolve the issue.</p>
                        
                        <asp:Panel ID="ErrorDetailsPanel" runat="server" Visible="false" CssClass="alert alert-danger">
                            <h4>Error Details:</h4>
                            <asp:Literal ID="ErrorDetailsLiteral" runat="server" />
                        </asp:Panel>
                        
                        <div class="error-actions">
                            <asp:HyperLink ID="HomeLink" runat="server" NavigateUrl="~/" CssClass="btn btn-primary">
                                <i class="fa fa-home"></i> Return to Home
                            </asp:HyperLink>
                            
                            <asp:Button ID="RetryButton" runat="server" Text="Try Again" CssClass="btn btn-default" OnClick="RetryButton_Click" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    
    <style>
        .error-page {
            text-align: center;
            padding: 50px 0;
        }
        
        .error-title {
            font-size: 2.5em;
            color: #d9534f;
            margin-bottom: 30px;
        }
        
        .error-message {
            font-size: 1.2em;
            margin-bottom: 30px;
        }
        
        .error-actions {
            margin-top: 30px;
        }
        
        .error-actions .btn {
            margin: 0 10px;
        }
        
        .alert {
            text-align: left;
            margin: 20px 0;
        }
    </style>
</asp:Content>