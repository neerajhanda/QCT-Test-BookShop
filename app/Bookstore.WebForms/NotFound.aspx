<%@ Page Title="Page Not Found" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="NotFound.aspx.cs" Inherits="Bookstore.WebForms.NotFound" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="row">
            <div class="col-md-8 col-md-offset-2">
                <div class="not-found-page">
                    <h1 class="not-found-title">404</h1>
                    <h2 class="not-found-subtitle">Page Not Found</h2>
                    
                    <div class="not-found-message">
                        <p>Sorry, the page you are looking for could not be found.</p>
                        <asp:Literal ID="RequestedUrlLiteral" runat="server" />
                    </div>
                    
                    <div class="not-found-suggestions">
                        <h4>Here are some suggestions:</h4>
                        <ul>
                            <li>Check the URL for typos</li>
                            <li>Use the search function to find what you're looking for</li>
                            <li>Browse our book categories</li>
                            <li>Return to the home page</li>
                        </ul>
                    </div>
                    
                    <div class="not-found-actions">
                        <asp:HyperLink ID="HomeLink" runat="server" NavigateUrl="~/" CssClass="btn btn-primary">
                            <i class="fa fa-home"></i> Go to Home Page
                        </asp:HyperLink>
                        
                        <asp:HyperLink ID="SearchLink" runat="server" NavigateUrl="~/Search.aspx" CssClass="btn btn-default">
                            <i class="fa fa-search"></i> Search Books
                        </asp:HyperLink>
                        
                        <asp:Button ID="BackButton" runat="server" Text="Go Back" CssClass="btn btn-default" OnClientClick="history.back(); return false;" />
                    </div>
                </div>
            </div>
        </div>
    </div>
    
    <style>
        .not-found-page {
            text-align: center;
            padding: 50px 0;
        }
        
        .not-found-title {
            font-size: 6em;
            color: #d9534f;
            margin-bottom: 0;
            font-weight: bold;
        }
        
        .not-found-subtitle {
            font-size: 2em;
            color: #666;
            margin-bottom: 30px;
        }
        
        .not-found-message {
            font-size: 1.2em;
            margin-bottom: 30px;
        }
        
        .not-found-suggestions {
            text-align: left;
            max-width: 400px;
            margin: 0 auto 30px auto;
        }
        
        .not-found-suggestions ul {
            list-style-type: disc;
            padding-left: 20px;
        }
        
        .not-found-actions {
            margin-top: 30px;
        }
        
        .not-found-actions .btn {
            margin: 0 10px;
        }
        
        .requested-url {
            font-family: monospace;
            background-color: #f5f5f5;
            padding: 10px;
            border-radius: 4px;
            margin: 10px 0;
            word-break: break-all;
        }
    </style>
</asp:Content>