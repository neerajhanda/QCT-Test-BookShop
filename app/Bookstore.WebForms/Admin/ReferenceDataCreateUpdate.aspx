<%@ Page Title="Reference Data" Language="C#" MasterPageFile="~/Admin/AdminMaster.Master" AutoEventWireup="true" CodeBehind="ReferenceDataCreateUpdate.aspx.cs" Inherits="Bookstore.WebForms.Admin.ReferenceDataCreateUpdate" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex m-3">
        <h5 class="me-auto">
            <asp:Label ID="PageTitleLabel" runat="server" Text="Create Reference Data" />
        </h5>
        <asp:HyperLink ID="BackToListLink" runat="server" NavigateUrl="~/Admin/ReferenceData.aspx" CssClass="btn btn-secondary" Text="Back to List" />
    </div>

    <!-- Message Banner -->
    <asp:Panel ID="MessagePanel" runat="server" CssClass="alert alert-danger mx-3" Visible="false">
        <asp:Label ID="MessageLabel" runat="server" />
    </asp:Panel>

    <!-- Reference Data Form -->
    <div class="card mx-3">
        <div class="card-body">
            <div class="row">
                <div class="col-md-6">
                    <div class="mb-3">
                        <asp:Label ID="ReferenceDataTypeLabel" runat="server" Text="Reference Data Type" CssClass="form-label" AssociatedControlID="ReferenceDataTypeDropDown" />
                        <asp:DropDownList ID="ReferenceDataTypeDropDown" runat="server" CssClass="form-select" />
                        <asp:RequiredFieldValidator ID="ReferenceDataTypeRequiredValidator" runat="server" 
                            ControlToValidate="ReferenceDataTypeDropDown" 
                            InitialValue=""
                            ErrorMessage="Reference Data Type is required" 
                            CssClass="text-danger" 
                            Display="Dynamic" />
                    </div>

                    <div class="mb-3">
                        <asp:Label ID="TextLabel" runat="server" Text="Text" CssClass="form-label" AssociatedControlID="TextTextBox" />
                        <asp:TextBox ID="TextTextBox" runat="server" CssClass="form-control" MaxLength="100" />
                        <asp:RequiredFieldValidator ID="TextRequiredValidator" runat="server" 
                            ControlToValidate="TextTextBox" 
                            ErrorMessage="Text is required" 
                            CssClass="text-danger" 
                            Display="Dynamic" />
                        <asp:RegularExpressionValidator ID="TextLengthValidator" runat="server" 
                            ControlToValidate="TextTextBox" 
                            ValidationExpression="^.{1,100}$"
                            ErrorMessage="Text cannot exceed 100 characters" 
                            CssClass="text-danger" 
                            Display="Dynamic" />
                    </div>

                    <div class="mb-3">
                        <asp:Button ID="SubmitButton" runat="server" Text="Create" CssClass="btn btn-primary me-2" OnClick="SubmitButton_Click" />
                        <asp:HyperLink ID="CancelLink" runat="server" NavigateUrl="~/Admin/ReferenceData.aspx" CssClass="btn btn-secondary" Text="Cancel" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Validation Summary -->
    <div class="mx-3">
        <asp:ValidationSummary ID="ValidationSummary" runat="server" 
            CssClass="alert alert-danger" 
            HeaderText="Please correct the following errors:" 
            DisplayMode="BulletList" />
    </div>
</asp:Content>