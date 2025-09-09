<%@ Page Title="Wishlist" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Wishlist.aspx.cs" Inherits="Bookstore.WebForms.Wishlist" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="panel-body">
        <h1>Wishlist</h1>
        <br />

        <asp:Panel ID="EmptyWishlistPanel" runat="server" Visible="false">
            <h2>Your wishlist is empty.</h2>
        </asp:Panel>

        <asp:Panel ID="WishlistPanel" runat="server" Visible="false">
            <asp:GridView ID="WishlistGridView" runat="server" 
                CssClass="table" 
                AutoGenerateColumns="false"
                OnRowCommand="WishlistGridView_RowCommand"
                GridLines="None"
                ShowHeader="true">
                <HeaderStyle CssClass="thead-light" />
                <Columns>
                    <asp:TemplateField HeaderText="Cover">
                        <ItemTemplate>
                            <img src='<%# Eval("ImageUrl") %>' 
                                 style="width: 135px;height: 200px;" 
                                 onerror="this.onerror=null;this.src='/Content/Images/default_c.jpg';" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="BookName" HeaderText="Book name" />
                    <asp:BoundField DataField="Price" HeaderText="Price" DataFormatString="{0:C}" />
                    <asp:TemplateField HeaderText="">
                        <ItemTemplate>
                            <div class="hstack gap-2">
                                <asp:LinkButton ID="MoveToCartButton" runat="server" 
                                    CssClass="btn btn-link" 
                                    Text="Move to shopping cart" 
                                    CommandName="MoveToCart" 
                                    CommandArgument='<%# Eval("ShoppingCartItemId") %>' />
                                <div class="vr"></div>
                                <asp:LinkButton ID="RemoveButton" runat="server" 
                                    CssClass="btn btn-link" 
                                    Text="Remove" 
                                    CommandName="RemoveItem" 
                                    CommandArgument='<%# Eval("ShoppingCartItemId") %>'
                                    OnClientClick="return confirm('Are you sure you want to remove this item from your wishlist?');" />
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>

            <asp:Button ID="MoveAllButton" runat="server" 
                Text="Move all items to shopping cart" 
                CssClass="btn" 
                OnClick="MoveAllButton_Click" />
        </asp:Panel>
    </div>
</asp:Content>