<%@ Page Title="Login" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Bookstore.WebForms.Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="row justify-content-center">
            <div class="col-md-6">
                <div class="card">
                    <div class="card-header">
                        <h3 class="text-center">Login to Bob's Used Books</h3>
                    </div>
                    <div class="card-body">
                        <asp:Panel ID="ErrorPanel" runat="server" CssClass="alert alert-danger" Visible="false">
                            <asp:Label ID="ErrorMessage" runat="server" />
                        </asp:Panel>
                        
                        <asp:Panel ID="InfoPanel" runat="server" CssClass="alert alert-info" Visible="false">
                            <asp:Label ID="InfoMessage" runat="server" />
                        </asp:Panel>

                        <div class="text-center">
                            <asp:Button ID="LoginButton" runat="server" 
                                Text="Login" 
                                CssClass="btn btn-primary btn-lg" 
                                OnClick="LoginButton_Click" />
                        </div>
                        
                        <div class="mt-3 text-center">
                            <small class="text-muted">
                                Click Login to authenticate with the configured authentication provider
                            </small>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>