<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PaginationControl.ascx.cs" Inherits="Bookstore.WebForms.Controls.PaginationControl" %>

<asp:Panel ID="PaginationPanel" runat="server" Visible="false">
    <nav aria-label="Page navigation">
        <ul class="pagination mb-0">
            <li class="page-item">
                <asp:LinkButton ID="PreviousButton" runat="server" 
                               CssClass="page-link" 
                               Text="Previous" 
                               OnClick="PreviousButton_Click" 
                               Enabled="false" />
            </li>
            
            <asp:Repeater ID="PageButtonsRepeater" runat="server" OnItemCommand="PageButtonsRepeater_ItemCommand">
                <ItemTemplate>
                    <li class="page-item">
                        <asp:LinkButton ID="PageButton" runat="server" 
                                       CssClass="page-link" 
                                       Text='<%# Eval("PageNumber") %>' 
                                       CommandName="GoToPage" 
                                       CommandArgument='<%# Eval("PageNumber") %>' 
                                       Enabled='<%# !(bool)Eval("IsCurrentPage") %>' />
                    </li>
                </ItemTemplate>
            </asp:Repeater>
            
            <li class="page-item">
                <asp:LinkButton ID="NextButton" runat="server" 
                               CssClass="page-link" 
                               Text="Next" 
                               OnClick="NextButton_Click" 
                               Enabled="false" />
            </li>
        </ul>
    </nav>
    
    <div class="pagination-info mt-2">
        <small class="text-muted">
            Showing page <asp:Literal ID="CurrentPageLiteral" runat="server"></asp:Literal> 
            of <asp:Literal ID="TotalPagesLiteral" runat="server"></asp:Literal>
            (<asp:Literal ID="TotalItemsLiteral" runat="server"></asp:Literal> total items)
        </small>
    </div>
</asp:Panel>