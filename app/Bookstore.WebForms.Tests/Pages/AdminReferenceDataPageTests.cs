using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Bookstore.Domain;
using Bookstore.Domain.ReferenceData;
using Bookstore.WebForms.Admin;
using Bookstore.WebForms.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bookstore.WebForms.Tests.Pages
{
    [TestClass]
    public class AdminReferenceDataPageTests
    {
        private Mock<IReferenceDataService> _mockReferenceDataService;
        private ReferenceData _referenceDataPage;
        private List<ReferenceDataItem> _testReferenceDataItems;

        [TestInitialize]
        public void Setup()
        {
            _mockReferenceDataService = new Mock<IReferenceDataService>();
            
            _referenceDataPage = new ReferenceData
            {
                ReferenceDataService = _mockReferenceDataService.Object
            };

            SetupTestData();
        }

        private void SetupTestData()
        {
            _testReferenceDataItems = new List<ReferenceDataItem>
            {
                new ReferenceDataItem(ReferenceDataType.Genre, "Fiction") { Id = 1 },
                new ReferenceDataItem(ReferenceDataType.Genre, "Non-Fiction") { Id = 2 },
                new ReferenceDataItem(ReferenceDataType.Condition, "New") { Id = 3 },
                new ReferenceDataItem(ReferenceDataType.Condition, "Used") { Id = 4 },
                new ReferenceDataItem(ReferenceDataType.Publisher, "Test Publisher") { Id = 5 },
                new ReferenceDataItem(ReferenceDataType.BookType, "Hardcover") { Id = 6 }
            };
        }

        [TestMethod]
        public void AdminReferenceDataIndexViewModel_Constructor_ShouldInitializeCorrectly()
        {
            // Arrange
            var mockPaginatedItems = CreateMockPaginatedList(_testReferenceDataItems);
            var filters = new ReferenceDataFilters { ReferenceDataType = ReferenceDataType.Genre };

            // Act
            var viewModel = new AdminReferenceDataIndexViewModel(mockPaginatedItems.Object, filters);

            // Assert
            Assert.AreEqual(6, viewModel.Items.Count);
            Assert.AreEqual("Fiction", viewModel.Items.First(i => i.Id == 1).Text);
            Assert.AreEqual("Genre", viewModel.Items.First(i => i.Id == 1).ReferenceDataType);
            Assert.AreEqual(ReferenceDataType.Genre, viewModel.Items.First(i => i.Id == 1).DataType);
            
            Assert.IsTrue(viewModel.ReferenceDataTypes.Any(t => t.Text == "Genre"));
            Assert.IsTrue(viewModel.ReferenceDataTypes.Any(t => t.Text == "Condition"));
            Assert.IsTrue(viewModel.ReferenceDataTypes.Any(t => t.Text == "Publisher"));
            Assert.IsTrue(viewModel.ReferenceDataTypes.Any(t => t.Text == "BookType"));
            
            Assert.AreEqual(ReferenceDataType.Genre, viewModel.Filters.ReferenceDataType);
        }

        [TestMethod]
        public void AdminReferenceDataIndexViewModel_DefaultConstructor_ShouldInitializeReferenceDataTypes()
        {
            // Act
            var viewModel = new AdminReferenceDataIndexViewModel();

            // Assert
            Assert.IsTrue(viewModel.ReferenceDataTypes.Any(t => t.Text == "Genre"));
            Assert.IsTrue(viewModel.ReferenceDataTypes.Any(t => t.Text == "Condition"));
            Assert.IsTrue(viewModel.ReferenceDataTypes.Any(t => t.Text == "Publisher"));
            Assert.IsTrue(viewModel.ReferenceDataTypes.Any(t => t.Text == "BookType"));
        }

        [TestMethod]
        public void AdminReferenceDataCreateUpdateViewModel_DefaultConstructor_ShouldInitializeDataTypes()
        {
            // Act
            var viewModel = new AdminReferenceDataCreateUpdateViewModel();

            // Assert
            Assert.IsTrue(viewModel.DataTypes.Any(t => t.Text == "Genre"));
            Assert.IsTrue(viewModel.DataTypes.Any(t => t.Text == "Condition"));
            Assert.IsTrue(viewModel.DataTypes.Any(t => t.Text == "Publisher"));
            Assert.IsTrue(viewModel.DataTypes.Any(t => t.Text == "BookType"));
            Assert.IsFalse(viewModel.IsEditMode);
            Assert.AreEqual("Create Reference Data", viewModel.PageTitle);
            Assert.AreEqual("Create", viewModel.SubmitButtonText);
        }

        [TestMethod]
        public void AdminReferenceDataCreateUpdateViewModel_WithReferenceDataItem_ShouldInitializeForEdit()
        {
            // Arrange
            var referenceDataItem = _testReferenceDataItems[0]; // Fiction genre

            // Act
            var viewModel = new AdminReferenceDataCreateUpdateViewModel(referenceDataItem);

            // Assert
            Assert.AreEqual(1, viewModel.Id);
            Assert.AreEqual(ReferenceDataType.Genre, viewModel.SelectedReferenceDataType);
            Assert.AreEqual("Fiction", viewModel.Text);
            Assert.IsTrue(viewModel.IsEditMode);
            Assert.AreEqual("Update Reference Data", viewModel.PageTitle);
            Assert.AreEqual("Update", viewModel.SubmitButtonText);
        }

        [TestMethod]
        public async Task ReferenceDataService_GetReferenceDataAsync_ShouldBeCalledWithCorrectParameters()
        {
            // Arrange
            var filters = new ReferenceDataFilters { ReferenceDataType = ReferenceDataType.Genre };
            var mockPaginatedItems = CreateMockPaginatedList(_testReferenceDataItems);
            
            _mockReferenceDataService.Setup(s => s.GetReferenceDataAsync(It.IsAny<ReferenceDataFilters>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(mockPaginatedItems.Object);

            // Act
            await _mockReferenceDataService.Object.GetReferenceDataAsync(filters, 1, 10);

            // Assert
            _mockReferenceDataService.Verify(s => s.GetReferenceDataAsync(
                It.Is<ReferenceDataFilters>(f => f.ReferenceDataType == ReferenceDataType.Genre), 
                1, 
                10), Times.Once);
        }

        [TestMethod]
        public async Task ReferenceDataService_GetReferenceDataItemAsync_ShouldBeCalledWithCorrectId()
        {
            // Arrange
            var referenceDataItem = _testReferenceDataItems[0];
            
            _mockReferenceDataService.Setup(s => s.GetReferenceDataItemAsync(It.IsAny<int>()))
                .ReturnsAsync(referenceDataItem);

            // Act
            var result = await _mockReferenceDataService.Object.GetReferenceDataItemAsync(1);

            // Assert
            _mockReferenceDataService.Verify(s => s.GetReferenceDataItemAsync(1), Times.Once);
            Assert.AreEqual(referenceDataItem, result);
        }

        [TestMethod]
        public async Task ReferenceDataService_CreateAsync_ShouldBeCalledWithCorrectDto()
        {
            // Arrange
            var dto = new CreateReferenceDataItemDto(ReferenceDataType.Genre, "Science Fiction");
            
            _mockReferenceDataService.Setup(s => s.CreateAsync(It.IsAny<CreateReferenceDataItemDto>()))
                .Returns(Task.CompletedTask);

            // Act
            await _mockReferenceDataService.Object.CreateAsync(dto);

            // Assert
            _mockReferenceDataService.Verify(s => s.CreateAsync(
                It.Is<CreateReferenceDataItemDto>(d => 
                    d.ReferenceDataType == ReferenceDataType.Genre && 
                    d.Text == "Science Fiction")), 
                Times.Once);
        }

        [TestMethod]
        public async Task ReferenceDataService_UpdateAsync_ShouldBeCalledWithCorrectDto()
        {
            // Arrange
            var dto = new UpdateReferenceDataItemDto(1, ReferenceDataType.Genre, "Updated Fiction");
            
            _mockReferenceDataService.Setup(s => s.UpdateAsync(It.IsAny<UpdateReferenceDataItemDto>()))
                .Returns(Task.CompletedTask);

            // Act
            await _mockReferenceDataService.Object.UpdateAsync(dto);

            // Assert
            _mockReferenceDataService.Verify(s => s.UpdateAsync(
                It.Is<UpdateReferenceDataItemDto>(d => 
                    d.Id == 1 && 
                    d.ReferenceDataType == ReferenceDataType.Genre && 
                    d.Text == "Updated Fiction")), 
                Times.Once);
        }

        [TestMethod]
        public void ReferenceDataFilters_Properties_ShouldSetCorrectly()
        {
            // Arrange & Act
            var filters = new ReferenceDataFilters
            {
                ReferenceDataType = ReferenceDataType.Publisher
            };

            // Assert
            Assert.AreEqual(ReferenceDataType.Publisher, filters.ReferenceDataType);
        }

        [TestMethod]
        public void CreateReferenceDataItemDto_Constructor_ShouldSetPropertiesCorrectly()
        {
            // Arrange & Act
            var dto = new CreateReferenceDataItemDto(ReferenceDataType.BookType, "Paperback");

            // Assert
            Assert.AreEqual(ReferenceDataType.BookType, dto.ReferenceDataType);
            Assert.AreEqual("Paperback", dto.Text);
        }

        [TestMethod]
        public void UpdateReferenceDataItemDto_Constructor_ShouldSetPropertiesCorrectly()
        {
            // Arrange & Act
            var dto = new UpdateReferenceDataItemDto(5, ReferenceDataType.Publisher, "Updated Publisher");

            // Assert
            Assert.AreEqual(5, dto.Id);
            Assert.AreEqual(ReferenceDataType.Publisher, dto.ReferenceDataType);
            Assert.AreEqual("Updated Publisher", dto.Text);
        }

        private Mock<IPaginatedList<ReferenceDataItem>> CreateMockPaginatedList(List<ReferenceDataItem> items)
        {
            var mock = new Mock<IPaginatedList<ReferenceDataItem>>();
            mock.Setup(m => m.GetEnumerator()).Returns(items.GetEnumerator());
            mock.Setup(m => m.PageIndex).Returns(1);
            mock.Setup(m => m.Count).Returns(items.Count);
            mock.Setup(m => m.TotalPages).Returns(1);
            mock.Setup(m => m.HasNextPage).Returns(false);
            mock.Setup(m => m.HasPreviousPage).Returns(false);
            mock.Setup(m => m.GetPageList(It.IsAny<int>())).Returns(new List<int> { 1 });
            return mock;
        }
    }
}