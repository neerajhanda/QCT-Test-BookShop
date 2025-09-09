<%@ Page Title="Inventory" Language="C#" MasterPageFile="~/Admin/AdminMaster.Master" AutoEventWireup="true" CodeBehind="Inventory.aspx.cs" Inherits="Bookstore.WebForms.Admin.Inventory" %>
<%@ Register Src="~/Controls/PaginationControl.ascx" TagPrefix="uc" TagName="Pagination" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex m-3">
        <h5 class="me-auto">Inventory</h5>
        <asp:HyperLink ID="NewBookLink" runat="server" NavigateUrl="~/Admin/InventoryCreateUpdate.aspx" CssClass="btn btn-primary" Text="New Book" />
    </div>

    <!-- Message Banner -->
    <asp:Panel ID="MessagePanel" runat="server" CssClass="alert alert-success mx-3" Visible="false">
        <asp:Label ID="MessageLabel" runat="server" />
    </asp:Panel>

    <!-- Filter Panel -->
    <div class="card mx-3">
        <div class="card-body">
            <div class="row row-cols-lg-auto g-3 align-items-center">
                <div class="col-12">
                    <asp:Label ID="NameFilterLabel" runat="server" Text="Name" CssClass="visually-hidden" AssociatedControlID="NameFilterTextBox" />
                    <asp:TextBox ID="NameFilterTextBox" runat="server" CssClass="form-control" placeholder="Name" />
                </div>

                <div class="col-12">
                    <asp:Label ID="AuthorFilterLabel" runat="server" Text="Author" CssClass="visually-hidden" AssociatedControlID="AuthorFilterTextBox" />
                    <asp:TextBox ID="AuthorFilterTextBox" runat="server" CssClass="form-control" placeholder="Author" />
                </div>

                <div class="col-12">
                    <asp:Label ID="PublisherFilterLabel" runat="server" Text="Publisher" CssClass="visually-hidden" AssociatedControlID="PublisherFilterDropDown" />
                    <asp:DropDownList ID="PublisherFilterDropDown" runat="server" CssClass="form-select" />
                </div>

                <div class="col-12">
                    <asp:Label ID="GenreFilterLabel" runat="server" Text="Genre" CssClass="visually-hidden" AssociatedControlID="GenreFilterDropDown" />
                    <asp:DropDownList ID="GenreFilterDropDown" runat="server" CssClass="form-select" />
                </div>

                <div class="col-12">
                    <asp:Label ID="BookTypeFilterLabel" runat="server" Text="Book Type" CssClass="visually-hidden" AssociatedControlID="BookTypeFilterDropDown" />
                    <asp:DropDownList ID="BookTypeFilterDropDown" runat="server" CssClass="form-select" />
                </div>

                <div class="col-12">
                    <asp:Label ID="ConditionFilterLabel" runat="server" Text="Condition" CssClass="visually-hidden" AssociatedControlID="ConditionFilterDropDown" />
                    <asp:DropDownList ID="ConditionFilterDropDown" runat="server" CssClass="form-select" />
                </div>

                <div class="col-12">
                    <div class="form-check">
                        <asp:CheckBox ID="LowStockCheckBox" runat="server" CssClass="form-check-input" />
                        <asp:Label ID="LowStockLabel" runat="server" Text="Low stock" CssClass="form-check-label" AssociatedControlID="LowStockCheckBox" />
                    </div>
                </div>

                <div class="col-12">
                    <asp:Button ID="FilterButton" runat="server" Text="Filter" CssClass="btn btn-primary" OnClick="FilterButton_Click" />
                </div>

                <div class="col-12">
                    <asp:Button ID="ClearButton" runat="server" Text="Clear" CssClass="btn btn-secondary" OnClick="ClearButton_Click" />
                </div>
            </div>
        </div>
    </div>

    <!-- Inventory Grid -->
    <div class="card m-3">
        <div class="card-header">
            <div class="d-flex justify-content-end">
                <uc:Pagination ID="TopPagination" runat="server" />
            </div>
        </div>

        <div class="card-body">
            <asp:GridView ID="InventoryGridView" runat="server" 
                CssClass="table table-striped table-hover" 
                AutoGenerateColumns="false"
                EmptyDataText="No books found."
                OnRowCommand="InventoryGridView_RowCommand">
                <Columns>
                    <asp:BoundField DataField="Name" HeaderText="Name" />
                    <asp:BoundField DataField="Author" HeaderText="Author" />
                    <asp:BoundField DataField="Publisher" HeaderText="Publisher" />
                    <asp:BoundField DataField="Genre" HeaderText="Genre" />
                    <asp:BoundField DataField="BookType" HeaderText="Type" />
                    <asp:BoundField DataField="Condition" HeaderText="Condition" />
                    <asp:BoundField DataField="Price" HeaderText="Price" DataFormatString="{0:C}" />
                    <asp:BoundField DataField="Quantity" HeaderText="Stock Level" />
                    <asp:BoundField DataField="UpdatedOn" HeaderText="Updated" DataFormatString="{0:d}" />
                    <asp:TemplateField HeaderText="">
                        <ItemTemplate>
                            <div class="hstack gap-2">
                                <asp:LinkButton ID="ViewButton" runat="server" 
                                    Text="View" 
                                    CssClass="card-link" 
                                    CommandName="ViewDetails" 
                                    CommandArgument='<%# Eval("Id") %>' />
                                <div class="vr"></div>
                                <asp:LinkButton ID="UpdateButton" runat="server" 
                                    Text="Update" 
                                    CssClass="card-link" 
                                    CommandName="UpdateBook" 
                                    CommandArgument='<%# Eval("Id") %>' />
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <div class="card-footer">
            <div class="d-flex justify-content-end">
                <uc:Pagination ID="BottomPagination" runat="server" />
            </div>
        </div>
    </div>
</asp:Content>