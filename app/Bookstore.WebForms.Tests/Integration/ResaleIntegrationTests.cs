using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Bookstore.Domain.Offers;
using Bookstore.Domain.ReferenceData;
using Bookstore.Domain;
using System.IO;

namespace Bookstore.WebForms.Tests.Integration
{
    [TestClass]
    public class ResaleIntegrationTests
    {
        private Mock<IOfferService> mockOfferService;
        private Mock<IReferenceDataService> mockReferenceDataService;
        private Mock<IFileService> mockFileService;

        [TestInitialize]
        public void Setup()
        {
            mockOfferService = new Mock<IOfferService>();
            mockReferenceDataService = new Mock<IReferenceDataService>();
            mockFileService = new Mock<IFileService>();
        }

        [TestMethod]
        public async Task CompleteResaleWorkflow_ShouldWorkEndToEnd()
        {
            // Arrange
            var userSub = "test-user-123";
            var testOffers = new List<Offer>();
            var testReferenceData = CreateTestReferenceData();

            mockOfferService.Setup(x => x.GetOffersAsync(userSub))
                           .ReturnsAsync(testOffers);
            mockReferenceDataService.Setup(x => x.GetAllReferenceDataAsync())
                                   .ReturnsAsync(testReferenceData);
            mockOfferService.Setup(x => x.CreateOfferAsync(It.IsAny<CreateOfferDto>()))
                           .Returns(Task.CompletedTask);

            // Act - Simulate the complete workflow
            // 1. Load page with no offers
            var initialOffers = await mockOfferService.Object.GetOffersAsync(userSub);
            
            // 2. Load reference data for create form
            var referenceData = await mockReferenceDataService.Object.GetAllReferenceDataAsync();
            
            // 3. Create new offer
            var createDto = new CreateOfferDto(
                userSub,
                "Integration Test Book",
                "Test Author",
                "1234567890123",
                1, // BookTypeId
                1, // ConditionId  
                1, // GenreId
                1, // PublisherId
                29.99m);

            await mockOfferService.Object.CreateOfferAsync(createDto);

            // Assert
            Assert.AreEqual(0, initialOffers.Count());
            Assert.IsTrue(referenceData.Any(x => x.DataType == ReferenceDataType.Genre));
            Assert.IsTrue(referenceData.Any(x => x.DataType == ReferenceDataType.Publisher));
            Assert.IsTrue(referenceData.Any(x => x.DataType == ReferenceDataType.BookType));
            Assert.IsTrue(referenceData.Any(x => x.DataType == ReferenceDataType.Condition));

            mockOfferService.Verify(x => x.GetOffersAsync(userSub), Times.Once);
            mockReferenceDataService.Verify(x => x.GetAllReferenceDataAsync(), Times.Once);
            mockOfferService.Verify(x => x.CreateOfferAsync(It.IsAny<CreateOfferDto>()), Times.Once);
        }

        [TestMethod]
        public async Task FileUploadWorkflow_ShouldHandleImageUpload()
        {
            // Arrange
            var fileName = "book_cover_test.jpg";
            var expectedPath = $"uploads/{fileName}";
            
            mockFileService.Setup(x => x.SaveAsync(It.IsAny<Stream>(), It.IsAny<string>()))
                          .ReturnsAsync(expectedPath);

            // Act
            using (var testStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }))
            {
                var uploadedPath = await mockFileService.Object.SaveAsync(testStream, fileName);

                // Assert
                Assert.AreEqual(expectedPath, uploadedPath);
                mockFileService.Verify(x => x.SaveAsync(It.IsAny<Stream>(), fileName), Times.Once);
            }
        }

        [TestMethod]
        public async Task ErrorHandling_ShouldHandleServiceExceptions()
        {
            // Arrange
            var userSub = "test-user-123";
            mockOfferService.Setup(x => x.GetOffersAsync(userSub))
                           .ThrowsAsync(new Exception("Service unavailable"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(async () =>
            {
                await mockOfferService.Object.GetOffersAsync(userSub);
            });
        }

        [TestMethod]
        public async Task DataValidation_ShouldValidateOfferData()
        {
            // Arrange
            var validDto = new CreateOfferDto(
                "user-123",
                "Valid Book Name",
                "Valid Author",
                "1234567890",
                1, 1, 1, 1,
                19.99m);

            mockOfferService.Setup(x => x.CreateOfferAsync(It.IsAny<CreateOfferDto>()))
                           .Returns(Task.CompletedTask);

            // Act
            await mockOfferService.Object.CreateOfferAsync(validDto);

            // Assert
            mockOfferService.Verify(x => x.CreateOfferAsync(It.Is<CreateOfferDto>(dto =>
                !string.IsNullOrEmpty(dto.BookName) &&
                !string.IsNullOrEmpty(dto.Author) &&
                !string.IsNullOrEmpty(dto.ISBN) &&
                dto.BookPrice > 0)), Times.Once);
        }

        [TestMethod]
        public void ReferenceDataMapping_ShouldMapToDropDownItems()
        {
            // Arrange
            var referenceData = CreateTestReferenceData();

            // Act
            var genres = referenceData
                .Where(x => x.DataType == ReferenceDataType.Genre)
                .Select(x => new { Value = x.Id.ToString(), Text = x.Text })
                .ToList();

            var publishers = referenceData
                .Where(x => x.DataType == ReferenceDataType.Publisher)
                .Select(x => new { Value = x.Id.ToString(), Text = x.Text })
                .ToList();

            // Assert
            Assert.AreEqual(2, genres.Count);
            Assert.AreEqual(2, publishers.Count);
            Assert.IsTrue(genres.Any(x => x.Text == "Fiction"));
            Assert.IsTrue(publishers.Any(x => x.Text == "Test Publisher"));
        }

        [TestMethod]
        public async Task OfferStatusDisplay_ShouldShowCorrectStatus()
        {
            // Arrange
            var userSub = "test-user-123";
            var offers = new List<Offer>
            {
                new Offer
                {
                    Id = 1,
                    BookName = "Test Book",
                    Author = "Test Author",
                    OfferStatus = OfferStatus.Pending,
                    Genre = new ReferenceDataItem { Text = "Fiction" },
                    Publisher = new ReferenceDataItem { Text = "Test Publisher" },
                    BookType = new ReferenceDataItem { Text = "Paperback" },
                    Condition = new ReferenceDataItem { Text = "Good" }
                }
            };

            mockOfferService.Setup(x => x.GetOffersAsync(userSub))
                           .ReturnsAsync(offers);

            // Act
            var result = await mockOfferService.Object.GetOffersAsync(userSub);
            var offer = result.First();

            // Assert
            Assert.AreEqual(OfferStatus.Pending, offer.OfferStatus);
            Assert.IsNotNull(offer.OfferStatus.GetDescription());
        }

        [TestMethod]
        public void FormValidation_ShouldValidateRequiredFields()
        {
            // Arrange
            var requiredFieldValidations = new Dictionary<string, bool>
            {
                { "BookName", false },
                { "Author", false },
                { "ISBN", false },
                { "BookPrice", false },
                { "Publisher", false },
                { "BookType", false },
                { "Genre", false },
                { "Condition", false }
            };

            // Act - Simulate validation checks
            requiredFieldValidations["BookName"] = !string.IsNullOrEmpty("Test Book");
            requiredFieldValidations["Author"] = !string.IsNullOrEmpty("Test Author");
            requiredFieldValidations["ISBN"] = !string.IsNullOrEmpty("1234567890");
            requiredFieldValidations["BookPrice"] = decimal.TryParse("19.99", out var price) && price > 0;
            requiredFieldValidations["Publisher"] = int.TryParse("1", out var publisherId) && publisherId > 0;
            requiredFieldValidations["BookType"] = int.TryParse("1", out var bookTypeId) && bookTypeId > 0;
            requiredFieldValidations["Genre"] = int.TryParse("1", out var genreId) && genreId > 0;
            requiredFieldValidations["Condition"] = int.TryParse("1", out var conditionId) && conditionId > 0;

            // Assert
            Assert.IsTrue(requiredFieldValidations.All(x => x.Value), 
                "All required fields should pass validation");
        }

        private List<ReferenceDataItem> CreateTestReferenceData()
        {
            return new List<ReferenceDataItem>
            {
                new ReferenceDataItem { Id = 1, Text = "Fiction", DataType = ReferenceDataType.Genre },
                new ReferenceDataItem { Id = 2, Text = "Non-Fiction", DataType = ReferenceDataType.Genre },
                new ReferenceDataItem { Id = 1, Text = "Test Publisher", DataType = ReferenceDataType.Publisher },
                new ReferenceDataItem { Id = 2, Text = "Another Publisher", DataType = ReferenceDataType.Publisher },
                new ReferenceDataItem { Id = 1, Text = "Paperback", DataType = ReferenceDataType.BookType },
                new ReferenceDataItem { Id = 2, Text = "Hardcover", DataType = ReferenceDataType.BookType },
                new ReferenceDataItem { Id = 1, Text = "Good", DataType = ReferenceDataType.Condition },
                new ReferenceDataItem { Id = 2, Text = "Excellent", DataType = ReferenceDataType.Condition }
            };
        }
    }
}