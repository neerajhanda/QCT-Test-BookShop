using Bookstore.Domain.Orders;
using Bookstore.WebForms.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.WebForms.Tests.Integration
{
    [TestClass]
    public class AdminOrderManagementIntegrationTests
    {
        private Mock<IOrderService> _mockOrderService;

        [TestInitialize]
        public void Setup()
        {
            _mockOrderService = new Mock<IOrderService>();
        }

        [TestMethod]
        public async Task AdminOrderWorkflow_GetOrdersWithFilters_ShouldReturnFilteredResults()
        {
            // Arrange
            var filters = new OrderFilters
            {
                OrderStatusFilter = OrderStatus.Processing,
                OrderDateFromFilter = DateTime.Now.AddDays(-30),
                OrderDateToFilter = DateTime.Now
            };

            var orders = CreateMockOrders();
            var paginatedOrders = CreateMockPaginatedList(orders);
            
            _mockOrderService.Setup(s => s.GetOrdersAsync(It.IsAny<OrderFilters>(), It.IsAny<int>(), It.IsAny<int>()))
                           .ReturnsAsync(paginatedOrders);

            // Act
            var result = await _mockOrderService.Object.GetOrdersAsync(filters, 1, 10);
            var viewModel = new AdminOrderIndexViewModel(result, filters);

            // Assert
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(2, viewModel.Items.Count);
            Assert.IsNotNull(viewModel.Filters);
            Assert.AreEqual(OrderStatus.Processing, viewModel.Filters.OrderStatusFilter);
        }

        [TestMethod]
        public async Task AdminOrderWorkflow_GetOrderDetails_ShouldReturnCompleteOrderInfo()
        {
            // Arrange
            var orderId = 1;
            var order = CreateMockOrder(orderId);
            
            _mockOrderService.Setup(s => s.GetOrderAsync(orderId))
                           .ReturnsAsync(order);

            // Act
            var result = await _mockOrderService.Object.GetOrderAsync(orderId);
            var viewModel = new AdminOrderDetailsViewModel(result);

            // Assert
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(orderId, viewModel.OrderId);
            Assert.AreEqual("John Doe", viewModel.CustomerName);
            Assert.IsTrue(viewModel.Items.Count > 0);
            Assert.IsTrue(viewModel.Total > 0);
        }

        [TestMethod]
        public async Task AdminOrderWorkflow_UpdateOrderStatus_ShouldUpdateSuccessfully()
        {
            // Arrange
            var orderId = 1;
            var originalOrder = CreateMockOrder(orderId);
            var updatedOrder = CreateMockOrder(orderId);
            updatedOrder.OrderStatus = OrderStatus.Shipped;

            _mockOrderService.Setup(s => s.GetOrderAsync(orderId))
                           .ReturnsAsync(originalOrder);
            
            _mockOrderService.Setup(s => s.UpdateOrderStatusAsync(It.IsAny<UpdateOrderStatusDto>()))
                           .Returns(Task.CompletedTask);

            _mockOrderService.Setup(s => s.GetOrderAsync(orderId))
                           .ReturnsAsync(updatedOrder);

            // Act
            var originalViewModel = new AdminOrderDetailsViewModel(originalOrder);
            var dto = new UpdateOrderStatusDto(orderId, OrderStatus.Shipped);
            await _mockOrderService.Object.UpdateOrderStatusAsync(dto);
            var updatedViewModel = new AdminOrderDetailsViewModel(updatedOrder);

            // Assert
            Assert.AreEqual(OrderStatus.Processing, originalViewModel.SelectedOrderStatus);
            Assert.AreEqual(OrderStatus.Shipped, updatedViewModel.SelectedOrderStatus);
            _mockOrderService.Verify(s => s.UpdateOrderStatusAsync(It.IsAny<UpdateOrderStatusDto>()), Times.Once);
        }

        [TestMethod]
        public async Task AdminOrderWorkflow_PaginationHandling_ShouldWorkCorrectly()
        {
            // Arrange
            var orders = CreateMockOrders();
            var paginatedOrders = CreateMockPaginatedList(orders, pageIndex: 2, totalPages: 5);
            
            _mockOrderService.Setup(s => s.GetOrdersAsync(It.IsAny<OrderFilters>(), 2, 10))
                           .ReturnsAsync(paginatedOrders);

            // Act
            var result = await _mockOrderService.Object.GetOrdersAsync(new OrderFilters(), 2, 10);
            var viewModel = new AdminOrderIndexViewModel(result, new OrderFilters());

            // Assert
            Assert.AreEqual(2, viewModel.PageIndex);
            Assert.AreEqual(5, viewModel.PageCount);
            Assert.IsTrue(viewModel.HasPreviousPage);
            Assert.IsTrue(viewModel.HasNextPage);
        }

        [TestMethod]
        public void AdminOrderViewModels_DataBinding_ShouldHandleNullValues()
        {
            // Arrange
            var order = CreateMockOrder(1);
            order.Address.AddressLine2 = null; // Test null handling

            // Act
            var viewModel = new AdminOrderDetailsViewModel(order);

            // Assert
            Assert.IsNull(viewModel.AddressLine2);
            Assert.IsNotNull(viewModel.AddressLine1);
            Assert.IsNotNull(viewModel.CustomerName);
        }

        [TestMethod]
        public void AdminOrderViewModels_CurrencyFormatting_ShouldFormatCorrectly()
        {
            // Arrange
            var order = CreateMockOrder(1);
            order.SubTotal = 123.45m;
            order.Tax = 9.88m;

            // Act
            var viewModel = new AdminOrderDetailsViewModel(order);

            // Assert
            Assert.AreEqual(123.45m, viewModel.Subtotal);
            Assert.AreEqual(9.88m, viewModel.Tax);
            Assert.AreEqual(133.33m, viewModel.Total);
        }

        private List<Order> CreateMockOrders()
        {
            return new List<Order>
            {
                CreateMockOrder(1),
                CreateMockOrder(2)
            };
        }

        private Order CreateMockOrder(int orderId)
        {
            var customer = new Customer { FullName = "John Doe" };
            var address = new Address 
            { 
                AddressLine1 = "123 Main St", 
                AddressLine2 = "Apt 4B",
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

            var orderItem = new OrderItem { Book = book };

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

        private IPaginatedList<Order> CreateMockPaginatedList(List<Order> orders, int pageIndex = 1, int totalPages = 1)
        {
            var mock = new Mock<IPaginatedList<Order>>();
            mock.Setup(p => p.Count).Returns(orders.Count);
            mock.Setup(p => p.PageIndex).Returns(pageIndex);
            mock.Setup(p => p.TotalPages).Returns(totalPages);
            mock.Setup(p => p.HasNextPage).Returns(pageIndex < totalPages);
            mock.Setup(p => p.HasPreviousPage).Returns(pageIndex > 1);
            mock.Setup(p => p.GetPageList(It.IsAny<int>())).Returns(new List<int> { pageIndex });
            mock.Setup(p => p.GetEnumerator()).Returns(orders.GetEnumerator());
            
            return mock.Object;
        }
    }
}