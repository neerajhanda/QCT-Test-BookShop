<%@ Page Title="Order Details" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="OrderDetails.aspx.cs" Inherits="Bookstore.WebForms.OrderDetails" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="panel-body">
        <h1>Order: #<asp:Literal ID="OrderIdLiteral" runat="server" /></h1>

        <div class="container mt-1">
            <h5>Status: <asp:Literal ID="OrderStatusLiteral" runat="server" /></h5>
            <h5>Delivery Date: <asp:Literal ID="DeliveryDateLiteral" runat="server" /></h5>
            <h5>Total cost: <asp:Literal ID="TotalCostLiteral" runat="server" /></h5>
        </div>

        <div class="container mt-5">
            <h2>Your Order:</h2>

            <asp:GridView ID="OrderItemsGridView" runat="server" 
                CssClass="table table-hover mt-5" 
                AutoGenerateColumns="false"
                GridLines="None"
                ShowHeader="true">
                <HeaderStyle CssClass="thead-light" />
                <Columns>
                    <asp:TemplateField HeaderText="Cover">
                        <ItemTemplate>
                            <img src='<%# Eval("ImageUrl") %>' 
                                 class="img-thumbnail" 
                                 onerror="this.onerror=null;this.src='/Content/Images/default_c.jpg';" 
                                 style="max-width: 75px;max-height: 75px;" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Title">
                        <ItemTemplate>
                            <asp:HyperLink ID="BookLink" runat="server" 
                                NavigateUrl='<%# "~/BookDetails.aspx?id=" + Eval("BookId") %>'
                                Text='<%# Eval("BookName") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Price" HeaderText="Price" DataFormatString="${0}" />
                </Columns>
            </asp:GridView>
            
            <asp:HyperLink ID="BackLink" runat="server" 
                NavigateUrl="~/Orders.aspx" 
                CssClass="btn" 
                Text="Back" />
        </div>
    </div>
</asp:Content>