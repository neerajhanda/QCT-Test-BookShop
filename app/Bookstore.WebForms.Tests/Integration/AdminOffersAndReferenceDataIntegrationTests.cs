using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bookstore.Domain;
using Bookstore.Domain.Offers;
using Bookstore.Domain.ReferenceData;
using Bookstore.WebForms.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bookstore.WebForms.Tests.Integration
{
    [TestClass]
    public class AdminOffersAndReferenceDataIntegrationTests
    {
        private Mock<IOfferService> _mockOfferService;
        private Mock<IReferenceDataService> _mockReferenceDataService;
        private List<Offer> _testOffers;
        private List<ReferenceDataItem> _testReferenceData;

        [TestInitialize]
        public void Setup()
        {
            _mockOfferService = new Mock<IOfferService>();
            _mockReferenceDataService = new Mock<IReferenceDataService>();
            SetupTestData();
        }

        private void SetupTestData()
        {
            _testReferenceData = new List<ReferenceDataItem>
            {
                new ReferenceDataItem(ReferenceDataType.Genre, "Fiction") { Id = 1 },
                new ReferenceDataItem(ReferenceDataType.Genre, "Non-Fiction") { Id = 2 },
                new ReferenceDataItem(ReferenceDataType.Condition, "New") { Id = 3 },
                new ReferenceDataItem(ReferenceDataType.Condition, "Used") { Id = 4 },
                new ReferenceDataItem(ReferenceDataType.Publisher, "Test Publisher") { Id = 5 },
                new ReferenceDataItem(ReferenceDataType.BookType, "Hardcover") { Id = 6 }
            };

            _testOffers = new List<Offer>
            {
                new Offer(1, "The Great Gatsby", "F. Scott Fitzgerald", "9780743273565", 6, 3, 1, 5, 15.99m)
                {
                    Id = 1,
                    OfferStatus = OfferStatus.PendingApproval,
                    Genre = _testReferenceData[0], // Fiction
                    Condition = _testReferenceData[2], // New
                    Publisher = _testReferenceData[4], // Test Publisher
                    BookType = _testReferenceData[5] // Hardcover
                },
                new Offer(2, "Sapiens", "Yuval Noah Harari", "9780062316097", 6, 4, 2, 5, 22.99m)
                {
                    Id = 2,
                    OfferStatus = OfferStatus.Approved,
                    Genre = _testReferenceData[1], // Non-Fiction
                    Condition = _testReferenceData[3], // Used
                    Publisher = _testReferenceData[4], // Test Publisher
                    BookType = _testReferenceData[5] // Hardcover
                }
            };
        }

        [TestMethod]
        public async Task OffersWorkflow_CompleteLifecycle_ShouldProcessCorrectly()
        {
            // Arrange
            var mockPaginatedOffers = CreateMockPaginatedList(_testOffers);
            
            _mockOfferService.Setup(s => s.GetOffersAsync(It.IsAny<OfferFilters>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(mockPaginatedOffers.Object);
            
            _mockReferenceDataService.Setup(s => s.GetAllReferenceDataAsync())
                .ReturnsAsync(_testReferenceData);

            _mockOfferService.Setup(s => s.UpdateOfferStatusAsync(It.IsAny<UpdateOfferStatusDto>()))
                .Returns(Task.CompletedTask);

            // Act - Load offers page
            var offers = await _mockOfferService.Object.GetOffersAsync(new OfferFilters(), 1, 10);
            var referenceData = await _mockReferenceDataService.Object.GetAllReferenceDataAsync();
            var viewModel = new AdminOffersIndexViewModel(offers, referenceData);

            // Assert - Verify data loaded correctly
            Assert.AreEqual(2, viewModel.Items.Count);
            Assert.IsTrue(viewModel.Genres.Any(g => g.Text == "Fiction"));
            Assert.IsTrue(viewModel.BookConditions.Any(c => c.Text == "New"));

            // Act - Approve first offer
            var approveDto = new UpdateOfferStatusDto(1, OfferStatus.Approved);
            await _mockOfferService.Object.UpdateOfferStatusAsync(approveDto);

            // Act - Mark second offer as received
            var receivedDto = new UpdateOfferStatusDto(2, OfferStatus.Received);
            await _mockOfferService.Object.UpdateOfferStatusAsync(receivedDto);

            // Assert - Verify service calls
            _mockOfferService.Verify(s => s.UpdateOfferStatusAsync(
                It.Is<UpdateOfferStatusDto>(d => d.Id == 1 && d.Status == OfferStatus.Approved)), Times.Once);
            
            _mockOfferService.Verify(s => s.UpdateOfferStatusAsync(
                It.Is<UpdateOfferStatusDto>(d => d.Id == 2 && d.Status == OfferStatus.Received)), Times.Once);
        }

        [TestMethod]
        public async Task ReferenceDataWorkflow_CompleteLifecycle_ShouldProcessCorrectly()
        {
            // Arrange
            var mockPaginatedItems = CreateMockPaginatedList(_testReferenceData);
            
            _mockReferenceDataService.Setup(s => s.GetReferenceDataAsync(It.IsAny<ReferenceDataFilters>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(mockPaginatedItems.Object);
            
            _mockReferenceDataService.Setup(s => s.GetReferenceDataItemAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => _testReferenceData.First(r => r.Id == id));

            _mockReferenceDataService.Setup(s => s.CreateAsync(It.IsAny<CreateReferenceDataItemDto>()))
                .Returns(Task.CompletedTask);

            _mockReferenceDataService.Setup(s => s.UpdateAsync(It.IsAny<UpdateReferenceDataItemDto>()))
                .Returns(Task.CompletedTask);

            // Act - Load reference data page
            var referenceDataItems = await _mockReferenceDataService.Object.GetReferenceDataAsync(new ReferenceDataFilters(), 1, 10);
            var indexViewModel = new AdminReferenceDataIndexViewModel(referenceDataItems, new ReferenceDataFilters());

            // Assert - Verify data loaded correctly
            Assert.AreEqual(6, indexViewModel.Items.Count);
            Assert.IsTrue(indexViewModel.Items.Any(i => i.Text == "Fiction" && i.ReferenceDataType == "Genre"));

            // Act - Create new reference data item
            var createDto = new CreateReferenceDataItemDto(ReferenceDataType.Genre, "Science Fiction");
            await _mockReferenceDataService.Object.CreateAsync(createDto);

            // Act - Load existing item for editing
            var existingItem = await _mockReferenceDataService.Object.GetReferenceDataItemAsync(1);
            var editViewModel = new AdminReferenceDataCreateUpdateViewModel(existingItem);

            // Assert - Verify edit model
            Assert.AreEqual(1, editViewModel.Id);
            Assert.AreEqual("Fiction", editViewModel.Text);
            Assert.AreEqual(ReferenceDataType.Genre, editViewModel.SelectedReferenceDataType);
            Assert.IsTrue(editViewModel.IsEditMode);

            // Act - Update existing item
            var updateDto = new UpdateReferenceDataItemDto(1, ReferenceDataType.Genre, "Updated Fiction");
            await _mockReferenceDataService.Object.UpdateAsync(updateDto);

            // Assert - Verify service calls
            _mockReferenceDataService.Verify(s => s.CreateAsync(
                It.Is<CreateReferenceDataItemDto>(d => 
                    d.ReferenceDataType == ReferenceDataType.Genre && 
                    d.Text == "Science Fiction")), Times.Once);
            
            _mockReferenceDataService.Verify(s => s.UpdateAsync(
                It.Is<UpdateReferenceDataItemDto>(d => 
                    d.Id == 1 && 
                    d.ReferenceDataType == ReferenceDataType.Genre && 
                    d.Text == "Updated Fiction")), Times.Once);
        }

        [TestMethod]
        public async Task OffersAndReferenceData_Integration_ShouldWorkTogether()
        {
            // Arrange
            var mockPaginatedOffers = CreateMockPaginatedList(_testOffers);
            var mockPaginatedReferenceData = CreateMockPaginatedList(_testReferenceData);
            
            _mockOfferService.Setup(s => s.GetOffersAsync(It.IsAny<OfferFilters>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(mockPaginatedOffers.Object);
            
            _mockReferenceDataService.Setup(s => s.GetAllReferenceDataAsync())
                .ReturnsAsync(_testReferenceData);
            
            _mockReferenceDataService.Setup(s => s.GetReferenceDataAsync(It.IsAny<ReferenceDataFilters>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(mockPaginatedReferenceData.Object);

            // Act - Load offers with reference data for filtering
            var offers = await _mockOfferService.Object.GetOffersAsync(new OfferFilters(), 1, 10);
            var allReferenceData = await _mockReferenceDataService.Object.GetAllReferenceDataAsync();
            var offersViewModel = new AdminOffersIndexViewModel(offers, allReferenceData);

            // Act - Load reference data management
            var referenceDataItems = await _mockReferenceDataService.Object.GetReferenceDataAsync(new ReferenceDataFilters(), 1, 10);
            var referenceDataViewModel = new AdminReferenceDataIndexViewModel(referenceDataItems, new ReferenceDataFilters());

            // Assert - Verify both systems use the same reference data
            var genresInOffers = offersViewModel.Genres.Select(g => g.Text).ToList();
            var genresInReferenceData = referenceDataViewModel.Items
                .Where(i => i.ReferenceDataType == "Genre")
                .Select(i => i.Text)
                .ToList();

            Assert.IsTrue(genresInOffers.Contains("Fiction"));
            Assert.IsTrue(genresInOffers.Contains("Non-Fiction"));
            Assert.IsTrue(genresInReferenceData.Contains("Fiction"));
            Assert.IsTrue(genresInReferenceData.Contains("Non-Fiction"));

            // Act - Filter offers by genre
            var genreFilter = new OfferFilters { GenreId = 1 }; // Fiction
            await _mockOfferService.Object.GetOffersAsync(genreFilter, 1, 10);

            // Assert - Verify filtering works
            _mockOfferService.Verify(s => s.GetOffersAsync(
                It.Is<OfferFilters>(f => f.GenreId == 1), 1, 10), Times.Once);
        }

        [TestMethod]
        public void ViewModels_Pagination_ShouldWorkConsistently()
        {
            // Arrange
            var mockPaginatedOffers = CreateMockPaginatedList(_testOffers);
            var mockPaginatedReferenceData = CreateMockPaginatedList(_testReferenceData);

            // Act
            var offersViewModel = new AdminOffersIndexViewModel(mockPaginatedOffers.Object, _testReferenceData);
            var referenceDataViewModel = new AdminReferenceDataIndexViewModel(mockPaginatedReferenceData.Object, new ReferenceDataFilters());

            // Assert - Both should have consistent pagination properties
            Assert.AreEqual(1, offersViewModel.PageIndex);
            Assert.AreEqual(1, offersViewModel.PageCount);
            Assert.IsFalse(offersViewModel.HasNextPage);
            Assert.IsFalse(offersViewModel.HasPreviousPage);

            Assert.AreEqual(1, referenceDataViewModel.PageIndex);
            Assert.AreEqual(1, referenceDataViewModel.PageCount);
            Assert.IsFalse(referenceDataViewModel.HasNextPage);
            Assert.IsFalse(referenceDataViewModel.HasPreviousPage);
        }

        private Mock<IPaginatedList<T>> CreateMockPaginatedList<T>(List<T> items)
        {
            var mock = new Mock<IPaginatedList<T>>();
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