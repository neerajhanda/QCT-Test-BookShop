<%@ Page Title="Create / Update Book" Language="C#" MasterPageFile="~/Admin/AdminMaster.Master" AutoEventWireup="true" CodeBehind="InventoryCreateUpdate.aspx.cs" Inherits="Bookstore.WebForms.Admin.InventoryCreateUpdate" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex m-3">
        <h5 class="me-auto">Create / Update Book</h5>
    </div>

    <!-- Validation Summary -->
    <asp:ValidationSummary ID="ValidationSummary" runat="server" CssClass="alert alert-danger mx-3" DisplayMode="BulletList" />

    <!-- Error Message Panel -->
    <asp:Panel ID="ErrorPanel" runat="server" CssClass="alert alert-danger mx-3" Visible="false">
        <asp:Label ID="ErrorLabel" runat="server" />
    </asp:Panel>

    <form id="BookForm" runat="server" enctype="multipart/form-data" novalidate>
        <asp:HiddenField ID="BookIdHidden" runat="server" />

        <div class="row justify-content-center">
            <div class="col-4">
                <div class="mb-3">
                    <asp:Label ID="NameLabel" runat="server" Text="Name" CssClass="form-label" AssociatedControlID="NameTextBox" />
                    <asp:TextBox ID="NameTextBox" runat="server" CssClass="form-control" placeholder="Book name" />
                    <asp:RequiredFieldValidator ID="NameRequired" runat="server" 
                        ControlToValidate="NameTextBox" 
                        ErrorMessage="Name is required" 
                        Display="Dynamic" 
                        CssClass="text-danger" />
                </div>

                <div class="mb-3">
                    <asp:Label ID="AuthorLabel" runat="server" Text="Author" CssClass="form-label" AssociatedControlID="AuthorTextBox" />
                    <asp:TextBox ID="AuthorTextBox" runat="server" CssClass="form-control" placeholder="Author" />
                    <asp:RequiredFieldValidator ID="AuthorRequired" runat="server" 
                        ControlToValidate="AuthorTextBox" 
                        ErrorMessage="Author is required" 
                        Display="Dynamic" 
                        CssClass="text-danger" />
                </div>

                <div class="mb-3">
                    <asp:Label ID="ISBNLabel" runat="server" Text="ISBN" CssClass="form-label" AssociatedControlID="ISBNTextBox" />
                    <asp:TextBox ID="ISBNTextBox" runat="server" CssClass="form-control" placeholder="ISBN" />
                    <asp:RequiredFieldValidator ID="ISBNRequired" runat="server" 
                        ControlToValidate="ISBNTextBox" 
                        ErrorMessage="ISBN is required" 
                        Display="Dynamic" 
                        CssClass="text-danger" />
                </div>

                <div class="row">
                    <div class="col">
                        <asp:Label ID="PriceLabel" runat="server" Text="Price" CssClass="form-label" AssociatedControlID="PriceTextBox" />
                        <asp:TextBox ID="PriceTextBox" runat="server" CssClass="form-control" placeholder="Price" TextMode="Number" step="0.01" />
                        <asp:RequiredFieldValidator ID="PriceRequired" runat="server" 
                            ControlToValidate="PriceTextBox" 
                            ErrorMessage="Price is required" 
                            Display="Dynamic" 
                            CssClass="text-danger" />
                        <asp:RangeValidator ID="PriceRange" runat="server" 
                            ControlToValidate="PriceTextBox" 
                            Type="Currency" 
                            MinimumValue="0.01" 
                            MaximumValue="9999.99" 
                            ErrorMessage="Price must be between $0.01 and $9999.99" 
                            Display="Dynamic" 
                            CssClass="text-danger" />
                    </div>

                    <div class="col">
                        <asp:Label ID="QuantityLabel" runat="server" Text="Quantity" CssClass="form-label" AssociatedControlID="QuantityTextBox" />
                        <asp:TextBox ID="QuantityTextBox" runat="server" CssClass="form-control" placeholder="Quantity" TextMode="Number" />
                        <asp:RequiredFieldValidator ID="QuantityRequired" runat="server" 
                            ControlToValidate="QuantityTextBox" 
                            ErrorMessage="Quantity is required" 
                            Display="Dynamic" 
                            CssClass="text-danger" />
                        <asp:RangeValidator ID="QuantityRange" runat="server" 
                            ControlToValidate="QuantityTextBox" 
                            Type="Integer" 
                            MinimumValue="0" 
                            MaximumValue="9999" 
                            ErrorMessage="Quantity must be between 0 and 9999" 
                            Display="Dynamic" 
                            CssClass="text-danger" />
                    </div>
                </div>

                <div class="mb-3">
                    <asp:Label ID="YearLabel" runat="server" Text="Year" CssClass="form-label" AssociatedControlID="YearTextBox" />
                    <asp:TextBox ID="YearTextBox" runat="server" CssClass="form-control" placeholder="Publication Year" TextMode="Number" />
                    <asp:RangeValidator ID="YearRange" runat="server" 
                        ControlToValidate="YearTextBox" 
                        Type="Integer" 
                        MinimumValue="1000" 
                        MaximumValue="2030" 
                        ErrorMessage="Year must be between 1000 and 2030" 
                        Display="Dynamic" 
                        CssClass="text-danger" />
                </div>

                <div class="mb-3">
                    <asp:Label ID="SummaryLabel" runat="server" Text="Summary" CssClass="form-label" AssociatedControlID="SummaryTextBox" />
                    <asp:TextBox ID="SummaryTextBox" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" placeholder="Summary" />
                </div>
            </div>

            <div class="col-4">
                <div class="mb-3">
                    <asp:Label ID="PublisherLabel" runat="server" Text="Publisher" CssClass="form-label" AssociatedControlID="PublisherDropDown" />
                    <div class="input-group">
                        <asp:DropDownList ID="PublisherDropDown" runat="server" CssClass="form-select" />
                        <asp:HyperLink ID="AddPublisherLink" runat="server" NavigateUrl="~/Admin/ReferenceData.aspx?type=Publisher" CssClass="btn btn-outline-primary" Text="Add" />
                    </div>
                    <asp:RequiredFieldValidator ID="PublisherRequired" runat="server" 
                        ControlToValidate="PublisherDropDown" 
                        InitialValue="" 
                        ErrorMessage="Publisher is required" 
                        Display="Dynamic" 
                        CssClass="text-danger" />
                </div>

                <div class="mb-3">
                    <asp:Label ID="GenreLabel" runat="server" Text="Genre" CssClass="form-label" AssociatedControlID="GenreDropDown" />
                    <div class="input-group">
                        <asp:DropDownList ID="GenreDropDown" runat="server" CssClass="form-select" />
                        <asp:HyperLink ID="AddGenreLink" runat="server" NavigateUrl="~/Admin/ReferenceData.aspx?type=Genre" CssClass="btn btn-outline-primary" Text="Add" />
                    </div>
                    <asp:RequiredFieldValidator ID="GenreRequired" runat="server" 
                        ControlToValidate="GenreDropDown" 
                        InitialValue="" 
                        ErrorMessage="Genre is required" 
                        Display="Dynamic" 
                        CssClass="text-danger" />
                </div>

                <div class="mb-3">
                    <asp:Label ID="BookTypeLabel" runat="server" Text="Book Type" CssClass="form-label" AssociatedControlID="BookTypeDropDown" />
                    <div class="input-group">
                        <asp:DropDownList ID="BookTypeDropDown" runat="server" CssClass="form-select" />
                        <asp:HyperLink ID="AddBookTypeLink" runat="server" NavigateUrl="~/Admin/ReferenceData.aspx?type=BookType" CssClass="btn btn-outline-primary" Text="Add" />
                    </div>
                    <asp:RequiredFieldValidator ID="BookTypeRequired" runat="server" 
                        ControlToValidate="BookTypeDropDown" 
                        InitialValue="" 
                        ErrorMessage="Book Type is required" 
                        Display="Dynamic" 
                        CssClass="text-danger" />
                </div>

                <div class="mb-3">
                    <asp:Label ID="ConditionLabel" runat="server" Text="Condition" CssClass="form-label" AssociatedControlID="ConditionDropDown" />
                    <div class="input-group">
                        <asp:DropDownList ID="ConditionDropDown" runat="server" CssClass="form-select" />
                        <asp:HyperLink ID="AddConditionLink" runat="server" NavigateUrl="~/Admin/ReferenceData.aspx?type=Condition" CssClass="btn btn-outline-primary" Text="Add" />
                    </div>
                    <asp:RequiredFieldValidator ID="ConditionRequired" runat="server" 
                        ControlToValidate="ConditionDropDown" 
                        InitialValue="" 
                        ErrorMessage="Condition is required" 
                        Display="Dynamic" 
                        CssClass="text-danger" />
                </div>
            </div>
        </div>

        <div class="row justify-content-center">
            <div class="col-8">
                <div class="mb-3">
                    <asp:Label ID="CoverImageLabel" runat="server" Text="Cover Image" CssClass="form-label" AssociatedControlID="CoverImageFileUpload" />
                    <asp:FileUpload ID="CoverImageFileUpload" runat="server" CssClass="form-control mb-3" accept=".png,.jpg,.jpeg" />
                    <asp:Label ID="CoverImageError" runat="server" CssClass="text-danger" Visible="false" />
                    <asp:Image ID="CoverImagePreview" runat="server" CssClass="img-thumbnail" Visible="false" />
                </div>
            </div>
        </div>

        <div class="row justify-content-center">
            <div class="col-8 d-grid gap-2 d-md-block">
                <asp:Button ID="SaveButton" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="SaveButton_Click" />
                <asp:HyperLink ID="BackLink" runat="server" NavigateUrl="~/Admin/Inventory.aspx" CssClass="btn btn-secondary" Text="Back" />
            </div>
        </div>
    </form>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script type="text/javascript">
        $(function () {
            $("#<%= CoverImageFileUpload.ClientID %>").change(function () {
                readUrl(this);
            });
        });

        function readUrl(input) {
            if (input.files && input.files[0]) {
                var reader = new FileReader();

                reader.onload = function (e) {
                    $("#<%= CoverImagePreview.ClientID %>")
                        .attr("src", e.target.result)
                        .show();
                }

                reader.readAsDataURL(input.files[0]);
            }
        }
    </script>
</asp:Content>