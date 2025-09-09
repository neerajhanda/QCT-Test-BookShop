<%@ Page Title="Order Confirmation" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CheckoutFinished.aspx.cs" Inherits="Bookstore.WebForms.CheckoutFinished" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="panel-body">
        
        <asp:Panel ID="ErrorPanel" runat="server" Visible="false" CssClass="alert alert-danger">
            <asp:Label ID="ErrorLabel" runat="server" />
        </asp:Panel>

        <table width="100%" height="100%" cellpadding="0" cellspacing="0">
            <tr>
                <td valign="middle" align="center">
                    <h2>Order Placed Successfully!</h2>
                </td>
            </tr>
        </table>

        <br />
        <br />
        <div class="container" style="text-align: center">
            <ul class="time-horizontal">
                <li style="color:red"><b></b>Cart</li>
                <li style="color:red"><b></b>Choose Address</li>
                <li style="color:red"><b></b>Finish</li>
            </ul>
        </div>
        <br />
        <br />

        <asp:Panel ID="OrderDetailsPanel" runat="server">
            <table class="table" width="700">
                <thead>
                <tr>
                    <th>Cover</th>
                    <th>Name</th>
                    <th>Price</th>
                    <th>Quantity</th>
                    <th>Total</th>
                </tr>
                </thead>
                <tbody>
                    <asp:Repeater ID="OrderItemsRepeater" runat="server">
                        <ItemTemplate>
                            <tr>
                                <td>
                                    <img src='<%# Eval("Url") %>' height="80" width="50" onerror="this.onerror=null;this.src='/images/default_c.jpg';" />
                                </td>
                                <td>
                                    <%# Eval("Bookname") %>
                                </td>
                                <td>
                                    $<%# Eval("Price") %>
                                </td>
                                <td>
                                    <%# Eval("Quantity") %>
                                </td>
                                <td>
                                    $<%# Eval("Total") %>
                                </td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
        </asp:Panel>

        <asp:HyperLink ID="BackToHomeLink" runat="server" NavigateUrl="~/Default.aspx" Text="Back to home" />
    </div>
</asp:Content>