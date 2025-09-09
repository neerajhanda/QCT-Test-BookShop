<%@ Page Title="Shopping Cart" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ShoppingCart.aspx.cs" Inherits="Bookstore.WebForms.ShoppingCart" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="panel-body">
        
        <asp:Panel ID="NotificationPanel" runat="server" Visible="false" CssClass="alert alert-info">
            <asp:Label ID="NotificationLabel" runat="server" />
        </asp:Panel>

        <p style="text-align: center">
            <h1>Cart</h1>
        </p>

        <div class="container">
            <ul class="time-horizontal">
                <li style="color:red"><b></b>Cart</li>
                <li>
                    <p></p>Choose Address
                </li>
                <li>
                    <p></p>Finish
                </li>
            </ul>
        </div>

        <div>
            <br />
            <br />
            <br />

            <h4>Total price：<asp:Label ID="TotalPriceLabel" runat="server" /></h4>
            <br />

            <asp:Panel ID="LoginPromptPanel" runat="server" Visible="false">
                <p>Please log in to complete your order.</p>
                <asp:HyperLink ID="LoginLink" runat="server" NavigateUrl="~/Login.aspx" CssClass="btn">Log In</asp:HyperLink>
            </asp:Panel>

            <asp:Panel ID="CheckoutPanel" runat="server" Visible="false">
                <asp:HyperLink ID="CheckoutLink" runat="server" NavigateUrl="~/Checkout.aspx" CssClass="btn">Check Out</asp:HyperLink>
            </asp:Panel>

            <asp:Repeater ID="ShoppingCartRepeater" runat="server" OnItemCommand="ShoppingCartRepeater_ItemCommand">
                <HeaderTemplate>
                    <table class="table">
                        <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td>
                            <img src='<%# Eval("ImageUrl") %>' onerror="this.onerror=null;this.src='/images/default_c.jpg';" style="max-width: 75px;max-height: 75px;" />
                        </td>
                        <td>
                            <span class='<%# (int)Eval("StockLevel") <= 0 ? "text-danger" : "" %>'><%# Eval("BookName") %></span>

                            <%# (int)Eval("StockLevel") <= 5 ? 
                                string.Format("<p class=\"text-danger\"><small>{0}</small></p>", 
                                    (int)Eval("StockLevel") <= 0 ? "Out of stock" : string.Format("Hurry, only {0} left!", Eval("StockLevel"))) : "" %>
                        </td>
                        <td>
                            <%# ((decimal)Eval("Price")).ToString("C") %>
                        </td>
                        <td>
                            <asp:Button ID="RemoveButton" runat="server" 
                                Text="Remove" 
                                CssClass="btn btn-link" 
                                CommandName="Remove" 
                                CommandArgument='<%# Eval("ShoppingCartItemId") %>'
                                OnClientClick="return confirm('Are you sure you want to remove this item from your shopping cart?');" />
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                        </tbody>
                    </table>
                </FooterTemplate>
            </asp:Repeater>

            <asp:Panel ID="EmptyCartPanel" runat="server" Visible="false">
                <p>Your shopping cart is empty.</p>
                <asp:HyperLink ID="ContinueShoppingLink" runat="server" NavigateUrl="~/Default.aspx" CssClass="btn">Continue Shopping</asp:HyperLink>
            </asp:Panel>
        </div>
        <br />
    </div>
</asp:Content>