<%@ Page Title="Sell Your Books" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Resale.aspx.cs" Inherits="Bookstore.WebForms.Resale" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="panel-body">
        <h1>Sell Your Books</h1>

        <div class="col-12 text-right">
            <asp:Button ID="CreateOfferButton" runat="server" Text="Resell Book" CssClass="btn" OnClick="CreateOfferButton_Click" />
        </div>

        <asp:Panel ID="NoOffersPanel" runat="server" Visible="false">
            <div class="alert alert-info">
                <h4>No offers found</h4>
                <p>You haven't submitted any books for resale yet. Click "Resell Book" to get started!</p>
            </div>
        </asp:Panel>

        <asp:Panel ID="OffersPanel" runat="server" Visible="false">
            <asp:GridView ID="OffersGridView" runat="server" CssClass="table" AutoGenerateColumns="false" 
                          EmptyDataText="No offers found">
                <Columns>
                    <asp:BoundField DataField="BookName" HeaderText="Book Name" />
                    <asp:BoundField DataField="Author" HeaderText="Author" />
                    <asp:BoundField DataField="Genre" HeaderText="Genre" />
                    <asp:BoundField DataField="Publisher" HeaderText="Publisher" />
                    <asp:BoundField DataField="BookType" HeaderText="Book Type" />
                    <asp:BoundField DataField="ISBN" HeaderText="ISBN" />
                    <asp:BoundField DataField="Condition" HeaderText="Condition" />
                    <asp:BoundField DataField="Price" HeaderText="Book Price" DataFormatString="{0:C}" />
                    <asp:BoundField DataField="OfferStatus" HeaderText="Status" />
                </Columns>
            </asp:GridView>
        </asp:Panel>

        <!-- Create Offer Panel -->
        <asp:Panel ID="CreateOfferPanel" runat="server" Visible="false">
            <hr />
            <h2>Book Details</h2>
            <br />

            <div class="form-group">
                <label class="main-text form-check-label">Book Name</label>
                <asp:TextBox ID="BookNameTextBox" runat="server" CssClass="main-text form-control bg-light text-dark" />
                <asp:RequiredFieldValidator ID="BookNameRequired" runat="server" ControlToValidate="BookNameTextBox" 
                                          ErrorMessage="Book name is required" CssClass="text-danger" Display="Dynamic" />
            </div>

            <div class="form-group">
                <label class="main-text form-check-label">Author</label>
                <asp:TextBox ID="AuthorTextBox" runat="server" CssClass="main-text form-control bg-light text-dark" />
                <asp:RequiredFieldValidator ID="AuthorRequired" runat="server" ControlToValidate="AuthorTextBox" 
                                          ErrorMessage="Author is required" CssClass="text-danger" Display="Dynamic" />
            </div>

            <div class="form-row">
                <div class="form-group col-md-6">
                    <label class="main-text form-check-label">ISBN</label>
                    <asp:TextBox ID="ISBNTextBox" runat="server" CssClass="main-text form-control bg-light text-dark" 
                               placeholder="10 or 13 digit ISBN" />
                    <asp:RequiredFieldValidator ID="ISBNRequired" runat="server" ControlToValidate="ISBNTextBox" 
                                              ErrorMessage="ISBN is required" CssClass="text-danger" Display="Dynamic" />
                </div>

                <div class="form-group col-md-6">
                    <label class="main-text form-check-label">Publisher Name</label>
                    <asp:DropDownList ID="PublisherDropDownList" runat="server" CssClass="form-control mb-3">
                        <asp:ListItem Value="" Text="Select the publisher" />
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator ID="PublisherRequired" runat="server" ControlToValidate="PublisherDropDownList" 
                                              InitialValue="" ErrorMessage="Publisher is required" CssClass="text-danger" Display="Dynamic" />
                </div>
            </div>

            <div class="form-row">
                <div class="form-group col-md-6">
                    <label class="main-text form-check-label">Book Price</label>
                    <asp:TextBox ID="BookPriceTextBox" runat="server" CssClass="main-text form-control bg-light text-dark" 
                               placeholder="$" TextMode="Number" step="0.01" />
                    <asp:RequiredFieldValidator ID="BookPriceRequired" runat="server" ControlToValidate="BookPriceTextBox" 
                                              ErrorMessage="Book price is required" CssClass="text-danger" Display="Dynamic" />
                    <asp:RangeValidator ID="BookPriceRange" runat="server" ControlToValidate="BookPriceTextBox" 
                                      MinimumValue="0.01" MaximumValue="9999.99" Type="Currency"
                                      ErrorMessage="Book price must be between $0.01 and $9999.99" CssClass="text-danger" Display="Dynamic" />
                </div>
                <div class="form-group col-md-6">
                    <label class="main-text form-check-label">Book Condition</label>
                    <asp:DropDownList ID="ConditionDropDownList" runat="server" CssClass="form-control mb-3">
                        <asp:ListItem Value="" Text="Select the book condition" />
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator ID="ConditionRequired" runat="server" ControlToValidate="ConditionDropDownList" 
                                              InitialValue="" ErrorMessage="Condition is required" CssClass="text-danger" Display="Dynamic" />
                </div>
            </div>

            <div class="form-row">
                <div class="form-group col-md-6">
                    <label class="main-text form-check-label">Book Type</label>
                    <asp:DropDownList ID="BookTypeDropDownList" runat="server" CssClass="form-control mb-3">
                        <asp:ListItem Value="" Text="Select the book type" />
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator ID="BookTypeRequired" runat="server" ControlToValidate="BookTypeDropDownList" 
                                              InitialValue="" ErrorMessage="Book type is required" CssClass="text-danger" Display="Dynamic" />
                </div>

                <div class="form-group col-md-6">
                    <label class="main-text form-check-label">Book Genre</label>
                    <asp:DropDownList ID="GenreDropDownList" runat="server" CssClass="form-control mb-3">
                        <asp:ListItem Value="" Text="Select the genre" />
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator ID="GenreRequired" runat="server" ControlToValidate="GenreDropDownList" 
                                              InitialValue="" ErrorMessage="Genre is required" CssClass="text-danger" Display="Dynamic" />
                </div>
            </div>

            <!-- File Upload Section -->
            <div class="form-group">
                <label class="main-text form-check-label">Book Cover Image (Optional)</label>
                <asp:FileUpload ID="BookCoverFileUpload" runat="server" CssClass="form-control-file" 
                              accept="image/*" />
                <small class="form-text text-muted">Upload a cover image for your book (JPG, PNG, GIF - Max 5MB)</small>
                <asp:RegularExpressionValidator ID="FileUploadValidator" runat="server" 
                                              ControlToValidate="BookCoverFileUpload" 
                                              ValidationExpression="^.*\.(jpg|jpeg|png|gif|JPG|JPEG|PNG|GIF)$"
                                              ErrorMessage="Please select a valid image file (JPG, PNG, GIF)" 
                                              CssClass="text-danger" Display="Dynamic" />
            </div>

            <hr class="mt-2 mb-5">

            <div class="form-group text-center">
                <asp:Button ID="SubmitOfferButton" runat="server" Text="SUBMIT" CssClass="btn btn-lg" 
                          OnClick="SubmitOfferButton_Click" />
                <asp:Button ID="CancelButton" runat="server" Text="CANCEL" CssClass="btn btn-secondary btn-lg ml-2" 
                          OnClick="CancelButton_Click" CausesValidation="false" />
            </div>
        </asp:Panel>

        <!-- Success/Error Messages -->
        <asp:Panel ID="MessagePanel" runat="server" Visible="false">
            <asp:Label ID="MessageLabel" runat="server" />
        </asp:Panel>
    </div>
</asp:Content>