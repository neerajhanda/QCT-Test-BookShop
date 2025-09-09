<%@ Page Title="Book Details" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="BookDetails.aspx.cs" Inherits="Bookstore.WebForms.BookDetails" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="panel-body">
        <div id="notificationPanel" runat="server" visible="false" class="alert alert-info">
            <asp:Literal ID="NotificationMessage" runat="server"></asp:Literal>
        </div>

        <asp:Panel ID="BookDetailsPanel" runat="server" Visible="false">
            <h1><asp:Literal ID="BookTitleLiteral" runat="server"></asp:Literal></h1>
            <br>
            
            <div class="container">
                <div class="row">
                    <div class="col">
                        <div class="media">
                            <asp:Image ID="BookImage" runat="server" 
                                      CssClass="mr-3" 
                                      AlternateText="Book Cover"
                                      onerror="this.onerror=null;this.src='/Content/images/default_c.jpg';" 
                                      style="width: 135px;height: 200px;" />
                            
                            <div class="media-body">
                                <table class="table">
                                    <tbody>
                                        <tr>
                                            <th scope="col">Title</th>
                                            <td><asp:Literal ID="TitleLiteral" runat="server"></asp:Literal></td>
                                        </tr>
                                        <tr>
                                            <th scope="col">Author</th>
                                            <td><asp:Literal ID="AuthorLiteral" runat="server"></asp:Literal></td>
                                        </tr>
                                        <tr>
                                            <th scope="col">Publisher</th>
                                            <td><asp:Literal ID="PublisherLiteral" runat="server"></asp:Literal></td>
                                        </tr>
                                        <tr>
                                            <th scope="col">ISBN</th>
                                            <td><asp:Literal ID="ISBNLiteral" runat="server"></asp:Literal></td>
                                        </tr>
                                        <tr>
                                            <th scope="col">Genre</th>
                                            <td><asp:Literal ID="GenreLiteral" runat="server"></asp:Literal></td>
                                        </tr>
                                        <tr>
                                            <th scope="col">Type</th>
                                            <td><asp:Literal ID="TypeLiteral" runat="server"></asp:Literal></td>
                                        </tr>
                                        <tr>
                                            <th scope="col">Condition</th>
                                            <td><asp:Literal ID="ConditionLiteral" runat="server"></asp:Literal></td>
                                        </tr>
                                        <tr>
                                            <th scope="col">Price</th>
                                            <td><asp:Literal ID="PriceLiteral" runat="server"></asp:Literal></td>
                                        </tr>
                                        <tr>
                                            <th scope="col">Availability</th>
                                            <td>
                                                <asp:Panel ID="InStockPanel" runat="server" Visible="false">
                                                    <span class="text-success">In Stock (<asp:Literal ID="QuantityLiteral" runat="server"></asp:Literal> available)</span>
                                                </asp:Panel>
                                                <asp:Panel ID="OutOfStockPanel" runat="server" Visible="false">
                                                    <span class="text-danger">Out of Stock</span>
                                                </asp:Panel>
                                            </td>
                                        </tr>
                                        <asp:Panel ID="DescriptionRow" runat="server" Visible="false">
                                            <tr>
                                                <th scope="col">Description</th>
                                                <td><asp:Literal ID="DescriptionLiteral" runat="server"></asp:Literal></td>
                                            </tr>
                                        </asp:Panel>
                                        <asp:Panel ID="NoDescriptionRow" runat="server" Visible="false">
                                            <tr>
                                                <th scope="col">Description</th>
                                                <td>No description found.</td>
                                            </tr>
                                        </asp:Panel>
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="row">
                    <div class="col">
                        <asp:Panel ID="ActionButtonsPanel" runat="server" Visible="false">
                            <asp:Button ID="AddToCartButton" runat="server" 
                                       CssClass="btn btn-primary me-2" 
                                       Text="Add to Cart" 
                                       OnClick="AddToCartButton_Click" />
                            <asp:Button ID="AddToWishlistButton" runat="server" 
                                       CssClass="btn btn-outline-secondary" 
                                       Text="Add to Wishlist" 
                                       OnClick="AddToWishlistButton_Click" />
                        </asp:Panel>
                        
                        <div class="mt-3">
                            <asp:HyperLink ID="BackToSearchLink" runat="server" 
                                          NavigateUrl="~/Search.aspx" 
                                          CssClass="btn btn-link">
                                ← Back to Search Results
                            </asp:HyperLink>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="BookNotFoundPanel" runat="server" Visible="false">
            <h3>Book not found.</h3>
            <p>The requested book could not be found. It may have been removed or the link may be incorrect.</p>
            <asp:HyperLink ID="SearchLink" runat="server" 
                          NavigateUrl="~/Search.aspx" 
                          CssClass="btn btn-primary">
                Browse Books
            </asp:HyperLink>
        </asp:Panel>
    </div>
</asp:Content>