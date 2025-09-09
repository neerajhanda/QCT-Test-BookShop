<%@ Page Title="Orders" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Orders.aspx.cs" Inherits="Bookstore.WebForms.Orders" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="panel-body">
        <h1>Your Orders</h1>

        <asp:Panel ID="NoOrdersPanel" runat="server" Visible="false">
            <h2>No Orders Found.</h2>
        </asp:Panel>

        <asp:Panel ID="OrdersPanel" runat="server" Visible="false">
            <asp:GridView ID="OrdersGridView" runat="server" 
                CssClass="table table-hover mt-5" 
                AutoGenerateColumns="false"
                OnRowCommand="OrdersGridView_RowCommand"
                GridLines="None"
                ShowHeader="true">
                <HeaderStyle CssClass="thead-light" />
                <Columns>
                    <asp:BoundField DataField="SubTotal" HeaderText="Total Cost" DataFormatString="{0:C}" />
                    <asp:BoundField DataField="DeliveryDate" HeaderText="Delivery Date" DataFormatString="{0:d}" />
                    <asp:BoundField DataField="OrderStatus" HeaderText="Status" />
                    <asp:TemplateField HeaderText="">
                        <ItemTemplate>
                            <div class="hstack gap-2">
                                <asp:LinkButton ID="DetailsButton" runat="server" 
                                    CssClass="btn" 
                                    Text="Details" 
                                    CommandName="ViewDetails" 
                                    CommandArgument='<%# Eval("Id") %>' />
                                <asp:LinkButton ID="CancelButton" runat="server" 
                                    CssClass="btn" 
                                    Text="Cancel" 
                                    CommandName="CancelOrder" 
                                    CommandArgument='<%# Eval("Id") %>'
                                    OnClientClick="return confirm('Are you sure you want to cancel this order?');" />
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </asp:Panel>
    </div>
</asp:Content>