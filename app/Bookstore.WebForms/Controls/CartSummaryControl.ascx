<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CartSummaryControl.ascx.cs" Inherits="Bookstore.WebForms.Controls.CartSummaryControl" %>

<div class="cart-summary-container">
    <asp:Panel ID="CartSummaryPanel" runat="server" CssClass="cart-summary">
        <div class="cart-header d-flex justify-content-between align-items-center">
            <h5 class="mb-0">
                <i class="bi bi-cart"></i> Shopping Cart
            </h5>
            <asp:HyperLink ID="ViewCartLink" runat="server" 
                          NavigateUrl="~/ShoppingCart.aspx" 
                          CssClass="btn btn-sm btn-outline-primary">
                View Cart
            </asp:HyperLink>
        </div>
        
        <div class="cart-content mt-2">
            <asp:Panel ID="EmptyCartPanel" runat="server" Visible="true" CssClass="empty-cart">
                <p class="text-muted mb-0">Your cart is empty</p>
            </asp:Panel>
            
            <asp:Panel ID="CartItemsPanel" runat="server" Visible="false">
                <div class="cart-items">
                    <asp:Repeater ID="CartItemsRepeater" runat="server" OnItemCommand="CartItemsRepeater_ItemCommand">
                        <ItemTemplate>
                            <div class="cart-item d-flex align-items-center mb-2 p-2 border-bottom">
                                <div class="item-image me-2">
                                    <img src='<%# Eval("ImageUrl") %>' 
                                         alt="Book Cover" 
                                         class="cart-item-image" 
                                         style="width: 40px; height: 50px; object-fit: cover;"
                                         onerror="this.onerror=null;this.src='/Content/images/default_c.jpg';" />
                                </div>
                                <div class="item-details flex-grow-1">
                                    <div class="item-name">
                                        <small><strong><%# Eval("BookName") %></strong></small>
                                    </div>
                                    <div class="item-price">
                                        <small class="text-success">$<%# Eval("Price", "{0:F2}") %></small>
                                    </div>
                                    <asp:Panel ID="StockWarningPanel" runat="server" 
                                              Visible='<%# (int)Eval("StockLevel") <= 5 && (int)Eval("StockLevel") > 0 %>'>
                                        <small class="text-warning">Only <%# Eval("StockLevel") %> left!</small>
                                    </asp:Panel>
                                    <asp:Panel ID="OutOfStockPanel" runat="server" 
                                              Visible='<%# (int)Eval("StockLevel") <= 0 %>'>
                                        <small class="text-danger">Out of stock</small>
                                    </asp:Panel>
                                </div>
                                <div class="item-actions">
                                    <asp:LinkButton ID="RemoveButton" runat="server" 
                                                   CssClass="btn btn-sm btn-outline-danger" 
                                                   CommandName="RemoveItem" 
                                                   CommandArgument='<%# Eval("ShoppingCartItemId") %>'
                                                   OnClientClick="return confirm('Remove this item from cart?');">
                                        <i class="bi bi-trash"></i>
                                    </asp:LinkButton>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                
                <div class="cart-total mt-3 p-2 bg-light">
                    <div class="d-flex justify-content-between align-items-center">
                        <strong>Total: $<asp:Literal ID="TotalPriceLiteral" runat="server"></asp:Literal></strong>
                        <div class="cart-actions">
                            <asp:HyperLink ID="CheckoutLink" runat="server" 
                                          NavigateUrl="~/Checkout.aspx" 
                                          CssClass="btn btn-sm btn-success me-2">
                                Checkout
                            </asp:HyperLink>
                        </div>
                    </div>
                    <div class="mt-1">
                        <small class="text-muted">
                            <asp:Literal ID="ItemCountLiteral" runat="server"></asp:Literal> item(s) in cart
                        </small>
                    </div>
                </div>
            </asp:Panel>
        </div>
    </asp:Panel>
    
    <asp:Panel ID="LoginPromptPanel" runat="server" Visible="false" CssClass="login-prompt">
        <div class="alert alert-info">
            <p class="mb-2">Please log in to view your cart</p>
            <asp:HyperLink ID="LoginLink" runat="server" 
                          NavigateUrl="~/Login.aspx" 
                          CssClass="btn btn-sm btn-primary">
                Log In
            </asp:HyperLink>
        </div>
    </asp:Panel>
</div>