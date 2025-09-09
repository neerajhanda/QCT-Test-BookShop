<%@ Page Title="Addresses" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Address.aspx.cs" Inherits="Bookstore.WebForms.Address" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="panel-body">
        <h1>Your Addresses</h1>
        <div class="col-md-12 text-right">
            <asp:HyperLink ID="CreateAddressLink" runat="server" 
                NavigateUrl="~/AddressCreateUpdate.aspx" 
                CssClass="btn" 
                Text="Create New Address" />
        </div>

        <div class="container">
            <div class="row justify-content-center">
                <div class="col-xl-10 col-lg-12 col-md-9">
                    <div class="card o-hidden border-0 my-5">
                        <div class="card-body p-0">
                            <asp:GridView ID="AddressGridView" runat="server" 
                                CssClass="table" 
                                AutoGenerateColumns="false"
                                OnRowCommand="AddressGridView_RowCommand"
                                GridLines="None"
                                ShowHeader="true">
                                <HeaderStyle CssClass="thead-light" />
                                <Columns>
                                    <asp:BoundField DataField="AddressLine1" HeaderText="Address Line 1" />
                                    <asp:BoundField DataField="AddressLine2" HeaderText="Address Line 2" />
                                    <asp:BoundField DataField="City" HeaderText="City" />
                                    <asp:BoundField DataField="State" HeaderText="State" />
                                    <asp:BoundField DataField="Country" HeaderText="Country" />
                                    <asp:BoundField DataField="ZipCode" HeaderText="Zipcode" />
                                    <asp:TemplateField HeaderText="Action">
                                        <ItemTemplate>
                                            <asp:HyperLink ID="EditLink" runat="server" 
                                                NavigateUrl='<%# "~/AddressCreateUpdate.aspx?id=" + Eval("Id") + "&returnUrl=" + Server.UrlEncode(Request.Url.ToString()) %>'
                                                Text="Edit" /> |
                                            <asp:LinkButton ID="DeleteButton" runat="server" 
                                                Text="Delete" 
                                                CommandName="DeleteAddress" 
                                                CommandArgument='<%# Eval("Id") %>'
                                                OnClientClick="return confirm('Are you sure you want to delete this address?');" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>