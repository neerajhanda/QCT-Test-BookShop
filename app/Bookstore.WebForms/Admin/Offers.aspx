<%@ Page Title="Offers Management" Language="C#" MasterPageFile="~/Admin/AdminMaster.Master" AutoEventWireup="true" CodeBehind="Offers.aspx.cs" Inherits="Bookstore.WebForms.Admin.Offers" %>
<%@ Register Src="~/Controls/PaginationControl.ascx" TagPrefix="uc" TagName="Pagination" %>
<%@ Import Namespace="Bookstore.Domain.Offers" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex m-3">
        <h5 class="me-auto">Offers Management</h5>
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
                    <asp:Label ID="BookNameFilterLabel" runat="server" Text="Book Name" CssClass="visually-hidden" AssociatedControlID="BookNameFilterTextBox" />
                    <asp:TextBox ID="BookNameFilterTextBox" runat="server" CssClass="form-control" placeholder="Book Name" />
                </div>

                <div class="col-12">
                    <asp:Label ID="AuthorFilterLabel" runat="server" Text="Author" CssClass="visually-hidden" AssociatedControlID="AuthorFilterTextBox" />
                    <asp:TextBox ID="AuthorFilterTextBox" runat="server" CssClass="form-control" placeholder="Author" />
                </div>

                <div class="col-12">
                    <asp:Label ID="GenreFilterLabel" runat="server" Text="Genre" CssClass="visually-hidden" AssociatedControlID="GenreFilterDropDown" />
                    <asp:DropDownList ID="GenreFilterDropDown" runat="server" CssClass="form-select" />
                </div>

                <div class="col-12">
                    <asp:Label ID="ConditionFilterLabel" runat="server" Text="Condition" CssClass="visually-hidden" AssociatedControlID="ConditionFilterDropDown" />
                    <asp:DropDownList ID="ConditionFilterDropDown" runat="server" CssClass="form-select" />
                </div>

                <div class="col-12">
                    <asp:Label ID="OfferStatusFilterLabel" runat="server" Text="Status" CssClass="visually-hidden" AssociatedControlID="OfferStatusFilterDropDown" />
                    <asp:DropDownList ID="OfferStatusFilterDropDown" runat="server" CssClass="form-select" />
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

    <!-- Offers Grid -->
    <div class="card m-3">
        <div class="card-header">
            <div class="d-flex justify-content-end">
                <uc:Pagination ID="TopPagination" runat="server" />
            </div>
        </div>

        <div class="card-body">
            <asp:GridView ID="OffersGridView" runat="server" 
                CssClass="table table-striped table-hover" 
                AutoGenerateColumns="false"
                EmptyDataText="No offers found."
                OnRowCommand="OffersGridView_RowCommand">
                <Columns>
                    <asp:BoundField DataField="BookName" HeaderText="Book Name" />
                    <asp:BoundField DataField="Author" HeaderText="Author" />
                    <asp:BoundField DataField="CustomerName" HeaderText="Customer" />
                    <asp:BoundField DataField="Genre" HeaderText="Genre" />
                    <asp:BoundField DataField="Condition" HeaderText="Condition" />
                    <asp:BoundField DataField="FormattedOfferPrice" HeaderText="Offer Price" />
                    <asp:BoundField DataField="OfferStatusText" HeaderText="Status" />
                    <asp:BoundField DataField="FormattedOfferDate" HeaderText="Offer Date" />
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <div class="hstack gap-2">
                                <asp:LinkButton ID="ApproveButton" runat="server" 
                                    Text="Approve" 
                                    CssClass="btn btn-sm btn-success" 
                                    CommandName="Approve" 
                                    CommandArgument='<%# Eval("OfferId") %>'
                                    Visible='<%# (OfferStatus)Eval("OfferStatus") == OfferStatus.PendingApproval %>'
                                    OnClientClick="return confirm('Are you sure you want to approve this offer?');" />
                                
                                <asp:LinkButton ID="RejectButton" runat="server" 
                                    Text="Reject" 
                                    CssClass="btn btn-sm btn-danger" 
                                    CommandName="Reject" 
                                    CommandArgument='<%# Eval("OfferId") %>'
                                    Visible='<%# (OfferStatus)Eval("OfferStatus") == OfferStatus.PendingApproval %>'
                                    OnClientClick="return confirm('Are you sure you want to reject this offer?');" />
                                
                                <asp:LinkButton ID="ReceivedButton" runat="server" 
                                    Text="Mark Received" 
                                    CssClass="btn btn-sm btn-info" 
                                    CommandName="Received" 
                                    CommandArgument='<%# Eval("OfferId") %>'
                                    Visible='<%# (OfferStatus)Eval("OfferStatus") == OfferStatus.Approved %>'
                                    OnClientClick="return confirm('Confirm that the book has been received?');" />
                                
                                <asp:LinkButton ID="PaidButton" runat="server" 
                                    Text="Mark Paid" 
                                    CssClass="btn btn-sm btn-warning" 
                                    CommandName="Paid" 
                                    CommandArgument='<%# Eval("OfferId") %>'
                                    Visible='<%# (OfferStatus)Eval("OfferStatus") == OfferStatus.Received %>'
                                    OnClientClick="return confirm('Confirm that the customer has been paid?');" />
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