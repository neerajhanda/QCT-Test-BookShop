<%@ Page Title="Search for Books" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="Bookstore.WebForms.Search" Async="true" %>
<%@ Register Src="~/Controls/SearchFilterControl.ascx" TagPrefix="uc" TagName="SearchFilter" %>
<%@ Register Src="~/Controls/BookListControl.ascx" TagPrefix="uc" TagName="BookList" %>
<%@ Register Src="~/Controls/PaginationControl.ascx" TagPrefix="uc" TagName="Pagination" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="panel-body">
        <div id="notificationPanel" runat="server" visible="false" class="alert alert-info">
            <asp:Literal ID="NotificationMessage" runat="server"></asp:Literal>
        </div>

        <h2 class="mb-3">Search for Books</h2>

        <uc:SearchFilter ID="SearchFilterControl" runat="server" OnSearchRequested="SearchFilterControl_SearchRequested" />

        <asp:Panel ID="ResultsPanel" runat="server" Visible="false">
            <section class="static about-sec">
                <div class="container">
                    <h2>Search Results</h2>
                    
                    <asp:Panel ID="ResultsCountPanel" runat="server" CssClass="mb-3">
                        <p class="text-muted">
                            Showing <asp:Literal ID="ResultsCountLiteral" runat="server"></asp:Literal> results
                            <asp:Panel ID="SearchTermPanel" runat="server" Visible="false">
                                for "<asp:Literal ID="SearchTermLiteral" runat="server"></asp:Literal>"
                            </asp:Panel>
                        </p>
                    </asp:Panel>

                    <div class="recent-book-sec">
                        <uc:BookList ID="BookListControl" runat="server" />
                    </div>

                    <div class="pagination-container">
                        <uc:Pagination ID="PaginationControl" runat="server" OnPageChanged="PaginationControl_PageChanged" />
                    </div>
                </div>
            </section>
        </asp:Panel>

        <asp:Panel ID="NoResultsPanel" runat="server" Visible="false">
            <h3>No results found.</h3>
            <p>Try adjusting your search criteria or browse our catalog.</p>
        </asp:Panel>
    </div>
</asp:Content>