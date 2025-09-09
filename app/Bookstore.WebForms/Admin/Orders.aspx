<%@ Page Title="Orders" Language="C#" MasterPageFile="~/Admin/AdminMaster.Master" AutoEventWireup="true" CodeBehind="Orders.aspx.cs" Inherits="Bookstore.WebForms.Admin.Orders" Async="true" %>
<%@ Register Src="~/Controls/PaginationControl.ascx" TagPrefix="uc" TagName="Pagination" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex m-3">
        <h5 class="me-auto">Orders</h5>
    </div>

    <div class="card mx-3">
        <div class="card-body">
            <div class="row row-cols-lg-auto g-3 align-items-center">
                <div class="col-12">
                    <label class="visually-hidden" for="<%= ddlOrderStatus.ClientID %>">Order Status</label>
                    <asp:DropDownList ID="ddlOrderStatus" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged">
                        <asp:ListItem Text="All Order Statuses" Value="" />
                    </asp:DropDownList>
                </div>
                <div class="col-12">
                    <label class="visually-hidden" for="<%= txtOrderDateFrom.ClientID %>">Order date from</label>
                    <asp:TextBox ID="txtOrderDateFrom" runat="server" CssClass="form-control" TextMode="Date" AutoPostBack="true" OnTextChanged="FilterChanged" />
                </div>
                <div class="col-12">
                    <label class="visually-hidden" for="<%= txtOrderDateTo.ClientID %>">Order date to</label>
                    <asp:TextBox ID="txtOrderDateTo" runat="server" CssClass="form-control" TextMode="Date" AutoPostBack="true" OnTextChanged="FilterChanged" />
                </div>
                <div class="col-12">
                    <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-primary" OnClick="btnFilter_Click" />
                </div>
                <div class="col-12">
                    <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-secondary" OnClick="btnClear_Click" />
                </div>
            </div>
        </div>
    </div>

    <div class="card m-3">
        <div class="card-header">
            <div class="d-flex justify-content-end">
                <uc:Pagination ID="ucPaginationTop" runat="server" />
            </div>
        </div>

        <div class="card-body">
            <asp:GridView ID="gvOrders" runat="server" CssClass="table table-striped table-hover" 
                AutoGenerateColumns="false" GridLines="None" OnRowCommand="gvOrders_RowCommand">
                <Columns>
                    <asp:BoundField DataField="CustomerName" HeaderText="Customer Name" />
                    <asp:BoundField DataField="OrderStatus" HeaderText="Status" />
                    <asp:BoundField DataField="OrderDate" HeaderText="Order Date" DataFormatString="{0:MM/dd/yyyy}" />
                    <asp:BoundField DataField="DeliveryDate" HeaderText="Delivery Date" DataFormatString="{0:MM/dd/yyyy}" />
                    <asp:BoundField DataField="Total" HeaderText="Order Total" DataFormatString="{0:C}" />
                    <asp:TemplateField HeaderText="">
                        <ItemTemplate>
                            <asp:LinkButton ID="lnkView" runat="server" Text="View" CssClass="card-link"
                                CommandName="ViewDetails" CommandArgument='<%# Eval("Id") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <div class="text-center p-3">
                        <p>No orders found matching the current filters.</p>
                    </div>
                </EmptyDataTemplate>
            </asp:GridView>
        </div>

        <div class="card-footer">
            <div class="d-flex justify-content-end">
                <uc:Pagination ID="ucPaginationBottom" runat="server" />
            </div>
        </div>
    </div>
</asp:Content>