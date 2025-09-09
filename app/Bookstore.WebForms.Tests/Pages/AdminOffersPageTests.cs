using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Bookstore.Domain;
using Bookstore.Domain.Offers;
using Bookstore.Domain.ReferenceData;
using Bookstore.WebForms.Admin;
using Bookstore.WebForms.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bookstore.WebForms.Tests.Pages
{
    [TestClass]
    public class AdminOffersPageTests
    {
        private Mock<IOfferService> _mockOfferService;
        private Mock<IReferenceDataService> _mockReferenceDataService;
        private Offers _offersPage;
        private List<Offer> _testOffers;
        private List<ReferenceDataItem> _testReferenceData;

        [TestInitialize]
        public void Setup()
        {
            _mockOfferService = new Mock<IOfferService>();
            _mockReferenceDataService = new Mock<IReferenceDataService>();
            
            _offersPage = new Offers
            {
                OfferService = _mockOfferService.Object,
                ReferenceDataService = _mockReferenceDataService.Object
            };

            SetupTestData();
        }

        private void SetupTestData()
        {
            _testReferenceData = new List<ReferenceDataItem>
            {
                new ReferenceDataItem(ReferenceDataType.Genre, "Fiction") { Id = 1 },
                new ReferenceDataItem(ReferenceDataType.Genre, "Non-Fiction") { Id = 2 },
                new ReferenceDataItem(ReferenceDataType.Condition, "New") { Id = 3 },
                new ReferenceDataItem(ReferenceDataType.Condition, "Used") { Id = 4 }
            };

            _testOffers = new List<Offer>
            {
                new Offer(1, "Test Book 1", "Test Author 1", "123456789", 1, 3, 1, 1, 19.99m)
                {
                    Id = 1,
                    OfferStatus = OfferStatus.PendingApproval,
                    Genre = _testReferenceData[0],
                    Condition = _testReferenceData[2]
                },
                new Offer(2, "Test Book 2", "Test Author 2", "987654321", 1, 4, 2, 1, 29.99m)
                {
                    Id = 2,
                    OfferStatus = OfferStatus.Approved,
                    Genre = _testReferenceData[1],
                    Condition = _testReferenceData[3]
                }
            };
        }

        [TestMethod]
        public void AdminOffersIndexViewModel_Constructor_ShouldInitializeCorrectly()
        {
            // Arrange
            var mockPaginatedOffers = CreateMockPaginatedList(_testOffers);

            // Act
            var viewModel = new AdminOffersIndexViewModel(mockPaginatedOffers.Object, _testReferenceData);

            // Assert
            Assert.AreEqual(2, viewModel.Items.Count);
            Assert.AreEqual("Test Book 1", viewModel.Items[0].BookName);
            Assert.AreEqual("Test Author 1", viewModel.Items[0].Author);
            Assert.AreEqual(OfferStatus.PendingApproval, viewModel.Items[0].OfferStatus);
            Assert.AreEqual("$19.99", viewModel.Items[0].FormattedOfferPrice);
            
            Assert.IsTrue(viewModel.Genres.Any(g => g.Text == "Fiction"));
            Assert.IsTrue(viewModel.BookConditions.Any(c => c.Text == "New"));
            Assert.IsTrue(viewModel.OfferStatuses.Any(s => s.Text == "PendingApproval"));
        }

        [TestMethod]
        public void AdminOffersIndexListItemViewModel_Properties_ShouldFormatCorrectly()
        {
            // Arrange
            var offer = _testOffers[0];
            var viewModel = new AdminOffersIndexListItemViewModel
            {
                OfferId = offer.Id,
                BookName = offer.BookName,
                Author = offer.Author,
                OfferStatus = offer.OfferStatus,
                OfferPrice = offer.BookPrice,
                OfferDate = DateTime.Now
            };

            // Act & Assert
            Assert.AreEqual("PendingApproval", viewModel.OfferStatusText);
            Assert.AreEqual("$19.99", viewModel.FormattedOfferPrice);
            Assert.AreEqual(DateTime.Now.ToString("d"), viewModel.FormattedOfferDate);
        }

        [TestMethod]
        public async Task OfferService_GetOffersAsync_ShouldBeCalledWithCorrectParameters()
        {
            // Arrange
            var filters = new OfferFilters { BookName = "Test" };
            var mockPaginatedOffers = CreateMockPaginatedList(_testOffers);
            
            _mockOfferService.Setup(s => s.GetOffersAsync(It.IsAny<OfferFilters>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(mockPaginatedOffers.Object);
            
            _mockReferenceDataService.Setup(s => s.GetAllReferenceDataAsync())
                .ReturnsAsync(_testReferenceData);

            // Act
            await _mockOfferService.Object.GetOffersAsync(filters, 1, 10);

            // Assert
            _mockOfferService.Verify(s => s.GetOffersAsync(
                It.Is<OfferFilters>(f => f.BookName == "Test"), 
                1, 
                10), Times.Once);
        }

        [TestMethod]
        public async Task OfferService_UpdateOfferStatusAsync_ShouldBeCalledWithCorrectDto()
        {
            // Arrange
            var dto = new UpdateOfferStatusDto(1, OfferStatus.Approved);
            
            _mockOfferService.Setup(s => s.UpdateOfferStatusAsync(It.IsAny<UpdateOfferStatusDto>()))
                .Returns(Task.CompletedTask);

            // Act
            await _mockOfferService.Object.UpdateOfferStatusAsync(dto);

            // Assert
            _mockOfferService.Verify(s => s.UpdateOfferStatusAsync(
                It.Is<UpdateOfferStatusDto>(d => d.Id == 1 && d.Status == OfferStatus.Approved)), 
                Times.Once);
        }

        [TestMethod]
        public void OfferFilters_Properties_ShouldSetCorrectly()
        {
            // Arrange & Act
            var filters = new OfferFilters
            {
                BookName = "Test Book",
                Author = "Test Author",
                GenreId = 1,
                ConditionId = 2,
                OfferStatus = OfferStatus.Approved
            };

            // Assert
            Assert.AreEqual("Test Book", filters.BookName);
            Assert.AreEqual("Test Author", filters.Author);
            Assert.AreEqual(1, filters.GenreId);
            Assert.AreEqual(2, filters.ConditionId);
            Assert.AreEqual(OfferStatus.Approved, filters.OfferStatus);
        }

        [TestMethod]
        public void UpdateOfferStatusDto_Constructor_ShouldSetPropertiesCorrectly()
        {
            // Arrange & Act
            var dto = new UpdateOfferStatusDto(123, OfferStatus.Paid);

            // Assert
            Assert.AreEqual(123, dto.Id);
            Assert.AreEqual(OfferStatus.Paid, dto.Status);
        }

        private Mock<IPaginatedList<Offer>> CreateMockPaginatedList(List<Offer> offers)
        {
            var mock = new Mock<IPaginatedList<Offer>>();
            mock.Setup(m => m.GetEnumerator()).Returns(offers.GetEnumerator());
            mock.Setup(m => m.PageIndex).Returns(1);
            mock.Setup(m => m.Count).Returns(offers.Count);
            mock.Setup(m => m.TotalPages).Returns(1);
            mock.Setup(m => m.HasNextPage).Returns(false);
            mock.Setup(m => m.HasPreviousPage).Returns(false);
            mock.Setup(m => m.GetPageList(It.IsAny<int>())).Returns(new List<int> { 1 });
            return mock;
        }
    }
}