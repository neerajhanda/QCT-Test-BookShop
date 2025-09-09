using Bookstore.Domain.Orders;
using Bookstore.WebForms.Admin;
using Bookstore.WebForms.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.WebForms.Tests.Pages
{
    [TestClass]
    public class AdminOrderDetailsPageTests
    {
        private Mock<IOrderService> _mockOrderService;
        private OrderDetails _orderDetailsPage;

        [TestInitialize]
        public void Setup()
        {
            _mockOrderService = new Mock<IOrderService>();
            _orderDetailsPage = new OrderDetails();
            _orderDetailsPage.OrderService = _mockOrderService.Object;
        }

        [TestMethod]
        public async Task GetOrderAsync_WithValidId_ShouldReturnOrder()
        {
            // Arrange
            var orderId = 1;
            var order = CreateMockOrder(orderId);
            
            _mockOrderService.Setup(s => s.GetOrderAsync(orderId))
                           .ReturnsAsync(order);

            // Act
            var result = await _mockOrderService.Object.GetOrderAsync(orderId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(orderId, result.Id);
            Assert.AreEqual("John Doe", result.Customer.FullName);
        }

        [TestMethod]
        public async Task UpdateOrderStatusAsync_WithValidData_ShouldCallService()
        {
            // Arrange
            var orderId = 1;
            var newStatus = OrderStatus.Shipped;
            var dto = new UpdateOrderStatusDto(orderId, newStatus);
            
            _mockOrderService.Setup(s => s.UpdateOrderStatusAsync(It.IsAny<UpdateOrderStatusDto>()))
                           .Returns(Task.CompletedTask);

            // Act
            await _mockOrderService.Object.UpdateOrderStatusAsync(dto);

            // Assert
            _mockOrderService.Verify(s => s.UpdateOrderStatusAsync(It.Is<UpdateOrderStatusDto>(
                d => d.OrderId == orderId && d.OrderStatus == newStatus)), Times.Once);
        }

        [TestMethod]
        public void AdminOrderDetailsViewModel_WithValidOrder_ShouldMapCorrectly()
        {
            // Arrange
            var order = CreateMockOrder(1);

            // Act
            var viewModel = new AdminOrderDetailsViewModel(order);

            // Assert
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(1, viewModel.OrderId);
            Assert.AreEqual("John Doe", viewModel.CustomerName);
            Assert.AreEqual(OrderStatus.Processing, viewModel.SelectedOrderStatus);
            Assert.AreEqual("123 Main St", viewModel.AddressLine1);
            Assert.AreEqual("Anytown", viewModel.City);
            Assert.AreEqual("ST", viewModel.State);
            Assert.AreEqual("12345", viewModel.ZipCode);
            Assert.AreEqual("USA", viewModel.Country);
            Assert.AreEqual(25.99m, viewModel.Subtotal);
            Assert.AreEqual(2.08m, viewModel.Tax);
            Assert.AreEqual(28.07m, viewModel.Total);
            Assert.AreEqual(1, viewModel.Items.Count);
        }

        [TestMethod]
        public void AdminOrderDetailsViewModel_WithOrderItems_ShouldMapItemsCorrectly()
        {
            // Arrange
            var order = CreateMockOrder(1);

            // Act
            var viewModel = new AdminOrderDetailsViewModel(order);

            // Assert
            Assert.AreEqual(1, viewModel.Items.Count);
            var item = viewModel.Items[0];
            Assert.AreEqual("Test Book", item.Name);
            Assert.AreEqual("Test Author", item.Author);
            Assert.AreEqual("Test Publisher", item.Publisher);
            Assert.AreEqual("Fiction", item.Genre);
            Assert.AreEqual("Paperback", item.BookType);
            Assert.AreEqual("Good", item.Condition);
            Assert.AreEqual(25.99m, item.Price);
        }

        [TestMethod]
        public void AdminOrderDetailsViewModel_EmptyConstructor_ShouldInitializeCollections()
        {
            // Act
            var viewModel = new AdminOrderDetailsViewModel();

            // Assert
            Assert.IsNotNull(viewModel.Items);
            Assert.AreEqual(0, viewModel.Items.Count);
        }

        private Order CreateMockOrder(int orderId)
        {
            var customer = new Customer { FullName = "John Doe" };
            var address = new Address 
            { 
                AddressLine1 = "123 Main St", 
                City = "Anytown", 
                State = "ST", 
                ZipCode = "12345", 
                Country = "USA" 
            };

            var publisher = new Publisher { Text = "Test Publisher" };
            var genre = new Genre { Text = "Fiction" };
            var bookType = new BookType { Text = "Paperback" };
            var condition = new Condition { Text = "Good" };

            var book = new Book
            {
                Name = "Test Book",
                Author = "Test Author",
                Publisher = publisher,
                Genre = genre,
                BookType = bookType,
                Condition = condition,
                Price = 25.99m
            };

            var orderItem = new OrderItem
            {
                Book = book
            };

            return new Order
            {
                Id = orderId,
                Customer = customer,
                Address = address,
                OrderStatus = OrderStatus.Processing,
                CreatedOn = DateTime.Now.AddDays(-5),
                DeliveryDate = DateTime.Now.AddDays(2),
                SubTotal = 25.99m,
                Tax = 2.08m,
                OrderItems = new List<OrderItem> { orderItem }
            };
        }
    }
}