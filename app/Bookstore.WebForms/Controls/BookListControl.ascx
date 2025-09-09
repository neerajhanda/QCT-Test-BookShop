<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BookListControl.ascx.cs" Inherits="Bookstore.WebForms.Controls.BookListControl" %>

<div class="book-list-container">
    <asp:Repeater ID="BooksRepeater" runat="server" OnItemDataBound="BooksRepeater_ItemDataBound">
        <HeaderTemplate>
            <div class="row justify-content-center">
        </HeaderTemplate>
        <ItemTemplate>
            <div class="col-lg-3 col-md-6">
                <div class="item text-center">
                    <asp:HyperLink ID="BookLink" runat="server" NavigateUrl='<%# GetBookDetailsUrl(Eval("BookId")) %>'>
                        <asp:Image ID="BookImage" runat="server" 
                                   ImageUrl='<%# Eval("CoverImageUrl") %>' 
                                   AlternateText="Book Cover" 
                                   CssClass="book_image" 
                                   onerror="this.onerror=null;this.src='/Content/images/default_c.jpg';" />
                    </asp:HyperLink>
                    
                    <p class="book-title">
                        <asp:Literal ID="BookName" runat="server" Text='<%# Eval("BookName") %>'></asp:Literal>
                    </p>
                    
                    <div class="book-price">
                        <asp:Panel ID="PricePanel" runat="server" Visible='<%# (bool)Eval("IsInStock") %>'>
                            <h6><span class="price">$<asp:Literal ID="BookPrice" runat="server" Text='<%# Eval("BookPrice", "{0:F2}") %>'></asp:Literal></span></h6>
                        </asp:Panel>
                        <asp:Panel ID="OutOfStockPanel" runat="server" Visible='<%# !(bool)Eval("IsInStock") %>'>
                            <h6 class="text-danger">Out Of Stock</h6>
                        </asp:Panel>
                        <asp:Panel ID="LowStockPanel" runat="server" Visible='<%# (bool)Eval("HasLowStockLevels") && (bool)Eval("IsInStock") %>'>
                            <p class="text-warning"><small>Low Stock</small></p>
                        </asp:Panel>
                    </div>
                    
                    <div class="hover">
                        <asp:HyperLink ID="ViewDetailsLink" runat="server" NavigateUrl='<%# GetBookDetailsUrl(Eval("BookId")) %>'>
                            <span><i class="bi bi-arrow-right"></i></span>
                        </asp:HyperLink>
                    </div>
                </div>
            </div>
        </ItemTemplate>
        <FooterTemplate>
            </div>
        </FooterTemplate>
    </asp:Repeater>
    
    <asp:Panel ID="NoResultsPanel" runat="server" Visible="false" CssClass="no-results">
        <h3>No books found.</h3>
    </asp:Panel>
</div>