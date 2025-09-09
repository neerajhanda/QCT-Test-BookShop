<%@ Page Title="Checkout" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Checkout.aspx.cs" Inherits="Bookstore.WebForms.Checkout" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="panel-body">
        
        <asp:Panel ID="NotificationPanel" runat="server" Visible="false" CssClass="alert alert-info">
            <asp:Label ID="NotificationLabel" runat="server" />
        </asp:Panel>

        <asp:Panel ID="ErrorPanel" runat="server" Visible="false" CssClass="alert alert-danger">
            <asp:Label ID="ErrorLabel" runat="server" />
        </asp:Panel>

        <h1>Finish your order</h1>
        <br/>
        <br/>
        <div class="container" style="text-align: center">
            <ul class="time-horizontal">
                <li style="color:red"><b></b>Cart</li>
                <li style="color:red"><b></b>Choose Address</li>
                <li>
                    <p></p>Finish
                </li>
            </ul>
        </div>
        <br/>
        <br/>
        <br/>

        <div>
            <h2>Total price: $<asp:Label ID="TotalPriceLabel" runat="server" /></h2>
        </div>

        <asp:Panel ID="CheckoutFormPanel" runat="server">
            <asp:Panel ID="AddressSelectionPanel" runat="server">
                <table class="table">
                    <thead>
                    <tr>
                        <th></th>
                        <th>Address Line 1</th>
                        <th>Address Line 2</th>
                        <th>City</th>
                        <th>State</th>
                        <th>Country</th>
                        <th>ZipCode</th>
                        <th></th>
                    </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="AddressRepeater" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td>
                                        <asp:RadioButton ID="AddressRadioButton" runat="server" 
                                            GroupName="SelectedAddress" 
                                            CommandArgument='<%# Eval("Id") %>' />
                                    </td>
                                    <td><%# Eval("AddressLine1") %></td>
                                    <td><%# Eval("AddressLine2") %></td>
                                    <td><%# Eval("City") %></td>
                                    <td><%# Eval("State") %></td>
                                    <td><%# Eval("Country") %></td>
                                    <td><%# Eval("ZipCode") %></td>
                                    <td>
                                        <asp:HyperLink ID="EditAddressLink" runat="server" 
                                            NavigateUrl='<%# "~/Address.aspx?action=edit&id=" + Eval("Id") + "&returnUrl=" + Server.UrlEncode(Request.RawUrl) %>' 
                                            Text="Edit" />
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>

                <asp:Panel ID="NoAddressPanel" runat="server" Visible="false">
                    <asp:Button ID="FinishOrderButtonDisabled" runat="server" Text="Finish your order" CssClass="btn" Enabled="false" />
                    <p style="color:red">Add an Address to proceed to checkout</p>
                </asp:Panel>

                <asp:Panel ID="HasAddressPanel" runat="server" Visible="false">
                    <asp:Button ID="FinishOrderButton" runat="server" Text="Finish your order" CssClass="btn" OnClick="FinishOrderButton_Click" />
                </asp:Panel>

                <!-- Address validation is handled in code-behind -->
            </asp:Panel>

            <div>
                <p style="text-align: center">
                    <asp:HyperLink ID="AddAddressLink" runat="server" 
                        NavigateUrl='<%# "~/Address.aspx?action=create&returnUrl=" + Server.UrlEncode(Request.RawUrl) %>' 
                        Text="Add address" />
                </p>
            </div>

            <table class="table">
                <thead>
                <tr>
                    <th>Cover</th>
                    <th>Name</th>
                    <th>Price</th>
                </tr>
                </thead>
                <tbody>
                    <asp:Repeater ID="ShoppingCartItemsRepeater" runat="server">
                        <ItemTemplate>
                            <tr>
                                <td>
                                    <img src='<%# Eval("ImageUrl") %>' height="80" width="50" onerror="this.onerror=null;this.src='/images/default_c.jpg';"/>
                                </td>
                                <td>
                                    <span class='<%# (bool)Eval("OutOfStock") ? "text-danger" : "" %>'><%# Eval("BookName") %></span>
                                    <asp:Panel ID="OutOfStockPanel" runat="server" Visible='<%# (bool)Eval("OutOfStock") %>'>
                                        <p class="text-danger"><small>Out of stock</small></p>
                                    </asp:Panel>
                                </td>
                                <td>
                                    <span style='<%# (bool)Eval("OutOfStock") ? "text-decoration: line-through;" : "" %>'>$<%# Eval("Price") %></span>
                                </td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
        </asp:Panel>
    </div>
</asp:Content>