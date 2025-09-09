<%@ Page Title="Reference Data Management" Language="C#" MasterPageFile="~/Admin/AdminMaster.Master" AutoEventWireup="true" CodeBehind="ReferenceData.aspx.cs" Inherits="Bookstore.WebForms.Admin.ReferenceData" %>
<%@ Register Src="~/Controls/PaginationControl.ascx" TagPrefix="uc" TagName="Pagination" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex m-3">
        <h5 class="me-auto">Reference Data Management</h5>
        <asp:HyperLink ID="NewReferenceDataLink" runat="server" NavigateUrl="~/Admin/ReferenceDataCreateUpdate.aspx" CssClass="btn btn-primary" Text="New Reference Data" />
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
                    <asp:Label ID="ReferenceDataTypeFilterLabel" runat="server" Text="Reference Data Type" CssClass="visually-hidden" AssociatedControlID="ReferenceDataTypeFilterDropDown" />
                    <asp:DropDownList ID="ReferenceDataTypeFilterDropDown" runat="server" CssClass="form-select" />
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

    <!-- Reference Data Grid -->
    <div class="card m-3">
        <div class="card-header">
            <div class="d-flex justify-content-end">
                <uc:Pagination ID="TopPagination" runat="server" />
            </div>
        </div>

        <div class="card-body">
            <asp:GridView ID="ReferenceDataGridView" runat="server" 
                CssClass="table table-striped table-hover" 
                AutoGenerateColumns="false"
                EmptyDataText="No reference data found."
                OnRowCommand="ReferenceDataGridView_RowCommand">
                <Columns>
                    <asp:BoundField DataField="ReferenceDataType" HeaderText="Type" />
                    <asp:BoundField DataField="Text" HeaderText="Text" />
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <div class="hstack gap-2">
                                <asp:LinkButton ID="UpdateButton" runat="server" 
                                    Text="Update" 
                                    CssClass="card-link" 
                                    CommandName="UpdateItem" 
                                    CommandArgument='<%# Eval("Id") %>' />
                                <div class="vr"></div>
                                <asp:LinkButton ID="DeleteButton" runat="server" 
                                    Text="Delete" 
                                    CssClass="card-link text-danger" 
                                    CommandName="DeleteItem" 
                                    CommandArgument='<%# Eval("Id") %>'
                                    OnClientClick="return confirm('Are you sure you want to delete this reference data item?');" />
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