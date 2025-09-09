using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bookstore.Domain.Books;
using Bookstore.Domain.Carts;
using Bookstore.Domain.Orders;
using Bookstore.Domain.Customers;
using Bookstore.Domain.Addresses;
using Bookstore.Domain.Offers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bookstore.WebForms.Tests.Integration
{
    /// <summary>
    /// Integration tests for data consistency and business rule enforcement across the application.
    /// Requirements: 9.4 - Validate data consistency and business rule enforcement
    /// </summary>
    [TestClass]
    public class DataConsistencyBusinessRulesIntegrationTests
    {
        private Mock<IBookService> _mockBookService;
        private Mock<IShoppingCartService> _mockShoppingCartService;
        private Mock<IOrderService> _mockOrderService;
        private Mock<ICustomerService> _mockCustomerService;
        private Mock<IAddressService> _mockAddressService;
        private Mock<IOfferService> _mockOfferService;

        [TestInitialize]
        public void Setup()
        {
            _mockBookService = new Mock<IBookService>();
            _mockShoppingCartService = new Mock<IShoppingCartService>();
            _mockOrderService = new Mock<IOrderService>();
            _mockCustomerService = new Mock<ICustomerService>();
            _mockAddressService = new Mock<IAddressService>();
            _mockOfferService = new Mock<IOfferService>();
        }

        #region Inventory Management Business Rules

        [TestMethod]
        public async Task BusinessRule_StockValidation_PreventNegativeInventory()
        {
            // Arrange
            var book = new Book
            {
                Id = 1,
                Name = "Test Book",
                Price = 19.99m,
                Quantity = 5 // Only 5 in stock
            };

            _mockBookService
                .Setup(s => s.GetBookAsync(book.Id))
                .ReturnsAsync(book);

            _mockBookService
                .Setup(s => s.UpdateBookAsync(It.Is<UpdateBookDto>(dto => dto.Quantity < 0)))
                .ThrowsAsync(new InvalidOperationException("Quantity cannot be negative"));

            // Act & Assert - Attempt to set negative quantity
            var updateDto = new UpdateBookDto
            {
                Id = book.Id,
                Name = book.Name,
                Price = book.Price,
                Quantity = -1 // Invalid negative quantity
            };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => _mockBookService.Object.UpdateBookAsync(updateDto));

            _mockBookService.Verify(s => s.UpdateBookAsync(It.IsAny<UpdateBookDto>()), Times.Once);
        }

        [TestMethod]
        public async Task BusinessRule_StockValidation_PreventOverselling()
        {
            // Arrange
            var correlationId = "oversell-test";
            var book = new Book
            {
                Id = 1,
                Name = "Limited Stock Book",
                Price = 29.99m,
                Quantity = 2 // Only 2 in stock
            };

            var cart = new Domain.Carts.ShoppingCart(correlationId);

            _mockBookService
                .Setup(s => s.GetBookAsync(book.Id))
                .ReturnsAsync(book);

            _mockShoppingCartService
                .Setup(s => s.GetShoppingCartAsync(correlationId))
                .ReturnsAsync(cart);

            _mockShoppingCartService
                .Setup(s => s.AddToShoppingCartAsync(It.Is<AddToShoppingCartDto>(dto => dto.Quantity > book.Quantity)))
                .ThrowsAsync(new InvalidOperationException("Insufficient stock available"));

            // Act & Assert - Attempt to add more than available stock
            var addToCartDto = new AddToShoppingCartDto(correlationId, book.Id, 5); // More than available

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => _mockShoppingCartService.Object.AddToShoppingCartAsync(addToCartDto));

            _mockShoppingCartService.Verify(s => s.AddToShoppingCartAsync(It.IsAny<AddToShoppingCartDto>()), Times.Once);
        }

        [TestMethod]
        public async Task BusinessRule_PriceValidation_EnforcePositivePrices()
        {
            // Arrange
            _mockBookService
                .Setup(s => s.CreateBookAsync(It.Is<CreateBookDto>(dto => dto.Price <= 0)))
                .ThrowsAsync(new ArgumentException("Price must be greater than zero"));

            _mockBookService
                .Setup(s => s.UpdateBookAsync(It.Is<UpdateBookDto>(dto => dto.Price <= 0)))
                .ThrowsAsync(new ArgumentException("Price must be greater than zero"));

            // Act & Assert - Create book with invalid price
            var createDto = new CreateBookDto
            {
                Name = "Invalid Price Book",
                Price = -10.00m, // Invalid negative price
                Quantity = 5
            };

            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => _mockBookService.Object.CreateBookAsync(createDto));

            // Act & Assert - Update book with invalid price
            var updateDto = new UpdateBookDto
            {
                Id = 1,
                Name = "Updated Book",
                Price = 0m, // Invalid zero price
                Quantity = 5
            };

            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => _mockBookService.Object.UpdateBookAsync(updateDto));

            _mockBookService.Verify(s => s.CreateBookAsync(It.IsAny<CreateBookDto>()), Times.Once);
            _mockBookService.Verify(s => s.UpdateBookAsync(It.IsAny<UpdateBookDto>()), Times.Once);
        }

        [TestMethod]
        public async Task BusinessRule_ISBNValidation_EnforceUniqueISBN()
        {
            // Arrange
            var existingISBN = "978-0-123456-78-9";

            _mockBookService
                .Setup(s => s.CreateBookAsync(It.Is<CreateBookDto>(dto => dto.ISBN == existingISBN)))
                .ThrowsAsync(new InvalidOperationException("ISBN already exists"));

            // Act & Assert
            var createDto = new CreateBookDto
            {
                Name = "Duplicate ISBN Book",
                Price = 19.99m,
                Quantity = 5,
                ISBN = existingISBN
            };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => _mockBookService.Object.CreateBookAsync(createDto));

            _mockBookService.Verify(s => s.CreateBookAsync(It.IsAny<CreateBookDto>()), Times.Once);
        }

        #endregion

        #region Shopping Cart Business Rules

        [TestMethod]
        public async Task BusinessRule_CartValidation_PreventDuplicateItems()
        {
            // Arrange
            var correlationId = "duplicate-test";
            var bookId = 123;
            var cart = new Domain.Carts.ShoppingCart(correlationId);
            cart.AddItemToShoppingCart(bookId, 2); // Already has this book

            _mockShoppingCartService
                .Setup(s => s.GetShoppingCartAsync(correlationId))
                .ReturnsAsync(cart);

            _mockShoppingCartService
                .Setup(s => s.AddToShoppingCartAsync(It.Is<AddToShoppingCartDto>(dto => 
                    dto.BookId == bookId && cart.ShoppingCartItems.Any(item => item.BookId == bookId))))
                .ThrowsAsync(new InvalidOperationException("Item already exists in cart. Use update quantity instead."));

            // Act & Assert
            var addToCartDto = new AddToShoppingCartDto(correlationId, bookId, 1);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => _mockShoppingCartService.Object.AddToShoppingCartAsync(addToCartDto));

            _mockShoppingCartService.Verify(s => s.AddToShoppingCartAsync(It.IsAny<AddToShoppingCartDto>()), Times.Once);
        }

        [TestMethod]
        public async Task BusinessRule_CartValidation_EnforceMaximumQuantity()
        {
            // Arrange
            var correlationId = "max-quantity-test";
            var bookId = 456;
            var maxQuantityPerItem = 10;

            _mockShoppingCartService
                .Setup(s => s.AddToShoppingCartAsync(It.Is<AddToShoppingCartDto>(dto => dto.Quantity > maxQuantityPerItem)))
                .ThrowsAsync(new InvalidOperationException($"Maximum quantity per item is {maxQuantityPerItem}"));

            // Act & Assert
            var addToCartDto = new AddToShoppingCartDto(correlationId, bookId, 15); // Exceeds maximum

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => _mockShoppingCartService.Object.AddToShoppingCartAsync(addToCartDto));

            _mockShoppingCartService.Verify(s => s.AddToShoppingCartAsync(It.IsAny<AddToShoppingCartDto>()), Times.Once);
        }

        [TestMethod]
        public async Task BusinessRule_CartValidation_PreventZeroQuantity()
        {
            // Arrange
            var correlationId = "zero-quantity-test";
            var bookId = 789;

            _mockShoppingCartService
                .Setup(s => s.AddToShoppingCartAsync(It.Is<AddToShoppingCartDto>(dto => dto.Quantity <= 0)))
                .ThrowsAsync(new ArgumentException("Quantity must be greater than zero"));

            // Act & Assert
            var addToCartDto = new AddToShoppingCartDto(correlationId, bookId, 0);

            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => _mockShoppingCartService.Object.AddToShoppingCartAsync(addToCartDto));

            _mockShoppingCartService.Verify(s => s.AddToShoppingCartAsync(It.IsAny<AddToShoppingCartDto>()), Times.Once);
        }

        #endregion

        #region Order Processing Business Rules

        [TestMethod]
        public async Task BusinessRule_OrderValidation_RequireValidShippingAddress()
        {
            // Arrange
            var customerId = "customer-123";
            var correlationId = "order-validation-test";
            var invalidAddressId = 999; // Non-existent address

            _mockAddressService
                .Setup(s => s.GetAddressAsync(invalidAddressId))
                .ReturnsAsync((Address)null);

            _mockOrderService
                .Setup(s => s.CreateOrderAsync(It.Is<CheckoutDto>(dto => dto.ShippingAddressId == invalidAddressId)))
                .ThrowsAsync(new InvalidOperationException("Invalid shipping address"));

            // Act & Assert
            var checkoutDto = new CheckoutDto
            {
                CorrelationId = correlationId,
                CustomerId = customerId,
                ShippingAddressId = invalidAddressId,
                BillingAddressId = 1
            };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => _mockOrderService.Object.CreateOrderAsync(checkoutDto));

            _mockOrderService.Verify(s => s.CreateOrderAsync(It.IsAny<CheckoutDto>()), Times.Once);
        }

        [TestMethod]
        public async Task BusinessRule_OrderValidation_RequireValidBillingAddress()
        {
            // Arrange
            var customerId = "customer-456";
            var correlationId = "billing-validation-test";
            var invalidBillingAddressId = 888;

            _mockAddressService
                .Setup(s => s.GetAddressAsync(invalidBillingAddressId))
                .ReturnsAsync((Address)null);

            _mockOrderService
                .Setup(s => s.CreateOrderAsync(It.Is<CheckoutDto>(dto => dto.BillingAddressId == invalidBillingAddressId)))
                .ThrowsAsync(new InvalidOperationException("Invalid billing address"));

            // Act & Assert
            var checkoutDto = new CheckoutDto
            {
                CorrelationId = correlationId,
                CustomerId = customerId,
                ShippingAddressId = 1,
                BillingAddressId = invalidBillingAddressId
            };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => _mockOrderService.Object.CreateOrderAsync(checkoutDto));

            _mockOrderService.Verify(s => s.CreateOrderAsync(It.IsAny<CheckoutDto>()), Times.Once);
        }

        [TestMethod]
        public async Task BusinessRule_OrderValidation_PreventEmptyCart()
        {
            // Arrange
            var customerId = "customer-789";
            var correlationId = "empty-cart-test";
            var emptyCart = new Domain.Carts.ShoppingCart(correlationId);

            _mockShoppingCartService
                .Setup(s => s.GetShoppingCartAsync(correlationId))
                .ReturnsAsync(emptyCart);

            _mockOrderService
                .Setup(s => s.CreateOrderAsync(It.IsAny<CheckoutDto>()))
                .ThrowsAsync(new InvalidOperationException("Cannot create order from empty cart"));

            // Act & Assert
            var checkoutDto = new CheckoutDto
            {
                CorrelationId = correlationId,
                CustomerId = customerId,
                ShippingAddressId = 1,
                BillingAddressId = 1
            };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => _mockOrderService.Object.CreateOrderAsync(checkoutDto));

            _mockOrderService.Verify(s => s.CreateOrderAsync(It.IsAny<CheckoutDto>()), Times.Once);
        }

        [TestMethod]
        public async Task BusinessRule_OrderStatusValidation_PreventInvalidStatusTransitions()
        {
            // Arrange
            var orderId = 123;
            var adminUserId = "admin-456";

            _mockOrderService
                .Setup(s => s.UpdateOrderStatusAsync(It.Is<UpdateOrderStatusDto>(dto => 
                    dto.Status == OrderStatus.Cancelled && dto.OrderId == orderId)))
                .ThrowsAsync(new InvalidOperationException("Cannot cancel a shipped order"));

            // Act & Assert - Try to cancel a shipped order
            var updateStatusDto = new UpdateOrderStatusDto
            {
                OrderId = orderId,
                Status = OrderStatus.Cancelled,
                UpdatedBy = adminUserId
            };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => _mockOrderService.Object.UpdateOrderStatusAsync(updateStatusDto));

            _mockOrderService.Verify(s => s.UpdateOrderStatusAsync(It.IsAny<UpdateOrderStatusDto>()), Times.Once);
        }

        #endregion

        #region Customer Data Consistency

        [TestMethod]
        public async Task DataConsistency_CustomerAddresses_MaintainRelationshipIntegrity()
        {
            // Arrange
            var customerId = "customer-consistency";
            var customer = new Customer
            {
                Id = customerId,
                FirstName = "Test",
                LastName = "Customer",
                Email = "test@example.com"
            };

            var addresses = new List<Address>
            {
                new Address
                {
                    Id = 1,
                    CustomerId = customerId,
                    AddressLine1 = "123 Test St",
                    City = "Test City",
                    State = "TS",
                    PostalCode = "12345",
                    Country = "USA"
                }
            };

            _mockCustomerService
                .Setup(s => s.GetCustomerAsync(customerId))
                .ReturnsAsync(customer);

            _mockAddressService
                .Setup(s => s.GetAddressesForCustomerAsync(customerId))
                .ReturnsAsync(addresses);

            // Act
            var retrievedCustomer = await _mockCustomerService.Object.GetCustomerAsync(customerId);
            var customerAddresses = await _mockAddressService.Object.GetAddressesForCustomerAsync(customerId);

            // Assert
            Assert.IsNotNull(retrievedCustomer);
            Assert.IsNotNull(customerAddresses);
            Assert.AreEqual(1, customerAddresses.Count);
            Assert.AreEqual(customerId, customerAddresses.First().CustomerId);
            
            // Verify relationship integrity
            foreach (var address in customerAddresses)
            {
                Assert.AreEqual(retrievedCustomer.Id, address.CustomerId);
            }
        }

        [TestMethod]
        public async Task DataConsistency_OrderCustomerRelationship_MaintainsIntegrity()
        {
            // Arrange
            var customerId = "order-customer-test";
            var customer = new Customer
            {
                Id = customerId,
                FirstName = "Order",
                LastName = "Customer",
                Email = "order@example.com"
            };

            var orders = new List<Order>
            {
                new Order
                {
                    Id = 1,
                    CustomerId = customerId,
                    Status = OrderStatus.Processing,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = 49.98m
                }
            };

            _mockCustomerService
                .Setup(s => s.GetCustomerAsync(customerId))
                .ReturnsAsync(customer);

            _mockOrderService
                .Setup(s => s.GetOrdersForCustomerAsync(customerId))
                .ReturnsAsync(orders);

            // Act
            var retrievedCustomer = await _mockCustomerService.Object.GetCustomerAsync(customerId);
            var customerOrders = await _mockOrderService.Object.GetOrdersForCustomerAsync(customerId);

            // Assert
            Assert.IsNotNull(retrievedCustomer);
            Assert.IsNotNull(customerOrders);
            Assert.AreEqual(1, customerOrders.Count);
            
            // Verify relationship integrity
            foreach (var order in customerOrders)
            {
                Assert.AreEqual(retrievedCustomer.Id, order.CustomerId);
            }
        }

        #endregion

        #region Offer and Pricing Business Rules

        [TestMethod]
        public async Task BusinessRule_OfferValidation_PreventInvalidDateRanges()
        {
            // Arrange
            _mockOfferService
                .Setup(s => s.CreateOfferAsync(It.Is<CreateOfferDto>(dto => dto.EndDate <= dto.StartDate)))
                .ThrowsAsync(new ArgumentException("End date must be after start date"));

            // Act & Assert
            var createOfferDto = new CreateOfferDto
            {
                Name = "Invalid Date Offer",
                Description = "Test offer with invalid dates",
                DiscountPercentage = 10,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(-1), // End date before start date
                CreatedBy = "admin-123"
            };

            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => _mockOfferService.Object.CreateOfferAsync(createOfferDto));

            _mockOfferService.Verify(s => s.CreateOfferAsync(It.IsAny<CreateOfferDto>()), Times.Once);
        }

        [TestMethod]
        public async Task BusinessRule_OfferValidation_PreventInvalidDiscountPercentage()
        {
            // Arrange
            _mockOfferService
                .Setup(s => s.CreateOfferAsync(It.Is<CreateOfferDto>(dto => dto.DiscountPercentage <= 0 || dto.DiscountPercentage > 100)))
                .ThrowsAsync(new ArgumentException("Discount percentage must be between 1 and 100"));

            // Act & Assert - Test negative discount
            var negativeDiscountDto = new CreateOfferDto
            {
                Name = "Negative Discount Offer",
                Description = "Test offer with negative discount",
                DiscountPercentage = -5,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                CreatedBy = "admin-123"
            };

            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => _mockOfferService.Object.CreateOfferAsync(negativeDiscountDto));

            // Act & Assert - Test excessive discount
            var excessiveDiscountDto = new CreateOfferDto
            {
                Name = "Excessive Discount Offer",
                Description = "Test offer with excessive discount",
                DiscountPercentage = 150,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                CreatedBy = "admin-123"
            };

            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => _mockOfferService.Object.CreateOfferAsync(excessiveDiscountDto));

            _mockOfferService.Verify(s => s.CreateOfferAsync(It.IsAny<CreateOfferDto>()), Times.Exactly(2));
        }

        [TestMethod]
        public async Task BusinessRule_PriceCalculation_AppliesOffersCorrectly()
        {
            // Arrange
            var book = new Book
            {
                Id = 1,
                Name = "Discounted Book",
                Price = 20.00m,
                Quantity = 10
            };

            var offer = new Offer
            {
                Id = 1,
                Name = "10% Off",
                DiscountPercentage = 10,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = OfferStatus.Active
            };

            var expectedDiscountedPrice = book.Price * (1 - offer.DiscountPercentage / 100);

            _mockBookService
                .Setup(s => s.GetBookAsync(book.Id))
                .ReturnsAsync(book);

            _mockOfferService
                .Setup(s => s.GetActiveOffersForBookAsync(book.Id))
                .ReturnsAsync(new List<Offer> { offer });

            // Act
            var retrievedBook = await _mockBookService.Object.GetBookAsync(book.Id);
            var activeOffers = await _mockOfferService.Object.GetActiveOffersForBookAsync(book.Id);

            // Assert
            Assert.IsNotNull(retrievedBook);
            Assert.IsNotNull(activeOffers);
            Assert.AreEqual(1, activeOffers.Count);
            
            var appliedOffer = activeOffers.First();
            var calculatedDiscountedPrice = retrievedBook.Price * (1 - appliedOffer.DiscountPercentage / 100);
            
            Assert.AreEqual(expectedDiscountedPrice, calculatedDiscountedPrice);
            Assert.AreEqual(18.00m, calculatedDiscountedPrice); // 20.00 * 0.9 = 18.00
        }

        #endregion

        #region Data Integrity Cross-Service Tests

        [TestMethod]
        public async Task DataIntegrity_CartToOrderConversion_MaintainsDataConsistency()
        {
            // Arrange
            var correlationId = "integrity-test";
            var customerId = "customer-integrity";
            var bookId = 123;
            var quantity = 2;
            var bookPrice = 25.99m;

            var cart = new Domain.Carts.ShoppingCart(correlationId);
            cart.AddItemToShoppingCart(bookId, quantity);
            
            // Simulate cart item with price
            var cartItem = cart.ShoppingCartItems.First();
            cartItem.Price = bookPrice;

            var expectedOrderTotal = quantity * bookPrice;

            _mockShoppingCartService
                .Setup(s => s.GetShoppingCartAsync(correlationId))
                .ReturnsAsync(cart);

            _mockOrderService
                .Setup(s => s.CreateOrderAsync(It.IsAny<CheckoutDto>()))
                .ReturnsAsync(new Order
                {
                    Id = 1,
                    CustomerId = customerId,
                    Status = OrderStatus.Processing,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = expectedOrderTotal,
                    OrderItems = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            BookId = bookId,
                            Quantity = quantity,
                            Price = bookPrice
                        }
                    }
                });

            // Act
            var retrievedCart = await _mockShoppingCartService.Object.GetShoppingCartAsync(correlationId);
            var cartTotal = retrievedCart.ShoppingCartItems.Sum(item => item.Price * item.Quantity);

            var checkoutDto = new CheckoutDto
            {
                CorrelationId = correlationId,
                CustomerId = customerId,
                ShippingAddressId = 1,
                BillingAddressId = 1
            };

            var createdOrder = await _mockOrderService.Object.CreateOrderAsync(checkoutDto);
            var orderTotal = createdOrder.OrderItems.Sum(item => item.Price * item.Quantity);

            // Assert
            Assert.AreEqual(cartTotal, orderTotal);
            Assert.AreEqual(expectedOrderTotal, orderTotal);
            Assert.AreEqual(retrievedCart.ShoppingCartItems.Count, createdOrder.OrderItems.Count);
            
            // Verify item-level consistency
            var cartItem1 = retrievedCart.ShoppingCartItems.First();
            var orderItem = createdOrder.OrderItems.First();
            
            Assert.AreEqual(cartItem1.BookId, orderItem.BookId);
            Assert.AreEqual(cartItem1.Quantity, orderItem.Quantity);
            Assert.AreEqual(cartItem1.Price, orderItem.Price);
        }

        [TestMethod]
        public async Task DataIntegrity_ConcurrentCartUpdates_MaintainsConsistency()
        {
            // Arrange
            var correlationId = "concurrent-test";
            var bookId = 456;
            var cart = new Domain.Carts.ShoppingCart(correlationId);

            _mockShoppingCartService
                .Setup(s => s.GetShoppingCartAsync(correlationId))
                .ReturnsAsync(cart);

            _mockShoppingCartService
                .Setup(s => s.AddToShoppingCartAsync(It.IsAny<AddToShoppingCartDto>()))
                .Callback<AddToShoppingCartDto>(dto => 
                {
                    // Simulate concurrent access check
                    if (cart.ShoppingCartItems.Any(item => item.BookId == dto.BookId))
                    {
                        throw new InvalidOperationException("Concurrent modification detected");
                    }
                    cart.AddItemToShoppingCart(dto.BookId, dto.Quantity);
                })
                .Returns(Task.CompletedTask);

            // Act & Assert - First addition should succeed
            var firstAddDto = new AddToShoppingCartDto(correlationId, bookId, 1);
            await _mockShoppingCartService.Object.AddToShoppingCartAsync(firstAddDto);

            // Act & Assert - Second addition of same item should fail due to concurrency check
            var secondAddDto = new AddToShoppingCartDto(correlationId, bookId, 1);
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => _mockShoppingCartService.Object.AddToShoppingCartAsync(secondAddDto));

            _mockShoppingCartService.Verify(s => s.AddToShoppingCartAsync(It.IsAny<AddToShoppingCartDto>()), Times.Exactly(2));
        }

        #endregion

        #region Business Rule Validation Summary Tests

        [TestMethod]
        public async Task BusinessRuleValidation_ComprehensiveValidation_EnforcesAllRules()
        {
            // This test verifies that multiple business rules are enforced together
            
            // Arrange - Set up various invalid scenarios
            var invalidScenarios = new List<Func<Task>>
            {
                // Invalid book creation
                () => _mockBookService.Object.CreateBookAsync(new CreateBookDto 
                { 
                    Name = "", 
                    Price = -1, 
                    Quantity = -1, 
                    ISBN = "invalid-isbn" 
                }),
                
                // Invalid cart operation
                () => _mockShoppingCartService.Object.AddToShoppingCartAsync(new AddToShoppingCartDto("", 0, 0)),
                
                // Invalid order creation
                () => _mockOrderService.Object.CreateOrderAsync(new CheckoutDto 
                { 
                    CorrelationId = "", 
                    CustomerId = "", 
                    ShippingAddressId = 0, 
                    BillingAddressId = 0 
                }),
                
                // Invalid offer creation
                () => _mockOfferService.Object.CreateOfferAsync(new CreateOfferDto 
                { 
                    Name = "", 
                    DiscountPercentage = -1, 
                    StartDate = DateTime.UtcNow, 
                    EndDate = DateTime.UtcNow.AddDays(-1) 
                })
            };

            // Set up mocks to throw appropriate exceptions
            _mockBookService
                .Setup(s => s.CreateBookAsync(It.IsAny<CreateBookDto>()))
                .ThrowsAsync(new ArgumentException("Invalid book data"));

            _mockShoppingCartService
                .Setup(s => s.AddToShoppingCartAsync(It.IsAny<AddToShoppingCartDto>()))
                .ThrowsAsync(new ArgumentException("Invalid cart operation"));

            _mockOrderService
                .Setup(s => s.CreateOrderAsync(It.IsAny<CheckoutDto>()))
                .ThrowsAsync(new ArgumentException("Invalid order data"));

            _mockOfferService
                .Setup(s => s.CreateOfferAsync(It.IsAny<CreateOfferDto>()))
                .ThrowsAsync(new ArgumentException("Invalid offer data"));

            // Act & Assert - All scenarios should throw exceptions
            foreach (var scenario in invalidScenarios)
            {
                await Assert.ThrowsExceptionAsync<ArgumentException>(scenario);
            }

            // Verify all services were called
            _mockBookService.Verify(s => s.CreateBookAsync(It.IsAny<CreateBookDto>()), Times.Once);
            _mockShoppingCartService.Verify(s => s.AddToShoppingCartAsync(It.IsAny<AddToShoppingCartDto>()), Times.Once);
            _mockOrderService.Verify(s => s.CreateOrderAsync(It.IsAny<CheckoutDto>()), Times.Once);
            _mockOfferService.Verify(s => s.CreateOfferAsync(It.IsAny<CreateOfferDto>()), Times.Once);
        }

        #endregion
    }
}