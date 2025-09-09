<%@ Page Title="Order Details" Language="C#" MasterPageFile="~/Admin/AdminMaster.Master" AutoEventWireup="true" CodeBehind="OrderDetails.aspx.cs" Inherits="Bookstore.WebForms.Admin.OrderDetails" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-success mx-3" role="alert">
        <asp:Literal ID="litMessage" runat="server" />
    </asp:Panel>

    <div class="d-flex m-3">
        <h5>Order Details</h5>
    </div>

    <div class="row justify-content-center mb-3">
        <div class="col-4">
            <div class="card h-100">
                <div class="card-body">
                    <address>
                        <strong><asp:Literal ID="litCustomerName" runat="server" /></strong>
                        <br />
                        <asp:Literal ID="litAddressLine1" runat="server" />
                        <br />
                        <asp:Panel ID="pnlAddressLine2" runat="server" Visible="false">
                            <asp:Literal ID="litAddressLine2" runat="server" />
                            <br />
                        </asp:Panel>
                        <asp:Literal ID="litCity" runat="server" /> <asp:Literal ID="litState" runat="server" /> <asp:Literal ID="litZipCode" runat="server" />
                        <br />
                        <asp:Literal ID="litCountry" runat="server" />
                    </address>
                </div>
            </div>
        </div>

        <div class="col-4">
            <div class="card h-100">
                <div class="card-body">
                    <dl class="row">
                        <dt class="col-3">Order status</dt>
                        <dd class="col-9">
                            <div class="d-flex">
                                <asp:DropDownList ID="ddlOrderStatus" runat="server" CssClass="form-select">
                                </asp:DropDownList>
                                <asp:Button ID="btnUpdateStatus" runat="server" Text="Update" CssClass="btn btn-primary ms-2" OnClick="btnUpdateStatus_Click" />
                            </div>
                        </dd>

                        <dt class="col-3">Ordered on</dt>
                        <dd class="col-9"><asp:Literal ID="litOrderDate" runat="server" /></dd>

                        <dt class="col-3">Deliver by</dt>
                        <dd class="col-9"><asp:Literal ID="litDeliveryDate" runat="server" /></dd>

                        <dt class="col-3">Subtotal</dt>
                        <dd class="col-9"><asp:Literal ID="litSubtotal" runat="server" /></dd>

                        <dt class="col-3">Tax</dt>
                        <dd class="col-9"><asp:Literal ID="litTax" runat="server" /></dd>

                        <dt class="col-3">Total</dt>
                        <dd class="col-9"><strong><asp:Literal ID="litTotal" runat="server" /></strong></dd>
                    </dl>
                </div>
            </div>
        </div>
    </div>

    <div class="row mb-3">
        <div class="col">
            <div class="card">
                <div class="card-body">
                    <asp:GridView ID="gvOrderItems" runat="server" CssClass="table table-striped table-hover" 
                        AutoGenerateColumns="false" GridLines="None">
                        <Columns>
                            <asp:BoundField DataField="Name" HeaderText="Name" />
                            <asp:BoundField DataField="Author" HeaderText="Author" />
                            <asp:BoundField DataField="Publisher" HeaderText="Publisher" />
                            <asp:BoundField DataField="Genre" HeaderText="Genre" />
                            <asp:BoundField DataField="BookType" HeaderText="Type" />
                            <asp:BoundField DataField="Condition" HeaderText="Condition" />
                            <asp:BoundField DataField="Price" HeaderText="Price" DataFormatString="{0:C}" />
                        </Columns>
                        <EmptyDataTemplate>
                            <div class="text-center p-3">
                                <p>No items found for this order.</p>
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>

    <div class="row">
        <div class="col-8">
            <asp:HyperLink ID="lnkBack" runat="server" NavigateUrl="Orders.aspx" CssClass="btn btn-primary" Text="Back" />
        </div>
    </div>
</asp:Content>