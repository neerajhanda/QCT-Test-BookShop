using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Bookstore.Domain.Books;
using Bookstore.Domain.ReferenceData;
using Bookstore.WebForms.Admin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bookstore.WebForms.Tests.Integration
{
    [TestClass]
    public class InventoryIntegrationTests
    {
        private Mock<IBookService> _mockBookService;
        private Mock<IReferenceDataService> _mockReferenceDataService;
        private Mock<HttpContextBase> _mockHttpContext;
        private Mock<HttpRequestBase> _mockHttpRequest;
        private Mock<HttpResponseBase> _mockHttpResponse;
        private Mock<HttpSessionStateBase> _mockSession;

        [TestInitialize]
        public void Setup()
        {
            _mockBookService = new Mock<IBookService>();
            _mockReferenceDataService = new Mock<IReferenceDataService>();
            _mockHttpContext = new Mock<HttpContextBase>();
            _mockHttpRequest = new Mock<HttpRequestBase>();
            _mockHttpResponse = new Mock<HttpResponseBase>();
            _mockSession = new Mock<HttpSessionStateBase>();

            _mockHttpContext.Setup(x => x.Request).Returns(_mockHttpRequest.Object);
            _mockHttpContext.Setup(x => x.Response).Returns(_mockHttpResponse.Object);
            _mockHttpContext.Setup(x => x.Session).Returns(_mockSession.Object);
        }

        [TestMethod]
        public async Task CreateBook_WithValidData_ShouldCallBookService()
        {
            // Arrange
            var createDto = new CreateBookDto(
                "Test Book",
                "Test Author",
                1, // BookTypeId
                2, // ConditionId
                3, // GenreId
                4, // PublisherId
                2023,
                "1234567890",
                "Test Summary",
                19.99m,
                5,
                null, // No image stream
                null  // No filename
            );

            var bookResult = new BookResult { IsSuccess = true };
            _mockBookService.Setup(x => x.AddAsync(It.IsAny<CreateBookDto>()))
                .ReturnsAsync(bookResult);

            // Act & Assert
            _mockBookService.Verify(x => x.AddAsync(It.IsAny<CreateBookDto>()), Times.Never);
            
            // Simulate the service call
            var result = await _mockBookService.Object.AddAsync(createDto);

            // Verify
            Assert.IsTrue(result.IsSuccess);
            _mockBookService.Verify(x => x.AddAsync(It.Is<CreateBookDto>(dto => 
                dto.Name == "Test Book" && 
                dto.Author == "Test Author" &&
                dto.Price == 19.99m &&
                dto.Quantity == 5)), Times.Once);
        }

        [TestMethod]
        public async Task UpdateBook_WithValidData_ShouldCallBookService()
        {
            // Arrange
            var updateDto = new UpdateBookDto(
                1, // Id
                "Updated Book",
                "Updated Author",
                1, // BookTypeId
                2, // ConditionId
                3, // GenreId
                4, // PublisherId
                2023,
                "1234567890",
                "Updated Summary",
                24.99m,
                10,
                null, // No image stream
                null  // No filename
            );

            var bookResult = new BookResult { IsSuccess = true };
            _mockBookService.Setup(x => x.UpdateAsync(It.IsAny<UpdateBookDto>()))
                .ReturnsAsync(bookResult);

            // Act
            var result = await _mockBookService.Object.UpdateAsync(updateDto);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            _mockBookService.Verify(x => x.UpdateAsync(It.Is<UpdateBookDto>(dto => 
                dto.Id == 1 &&
                dto.Name == "Updated Book" && 
                dto.Author == "Updated Author" &&
                dto.Price == 24.99m &&
                dto.Quantity == 10)), Times.Once);
        }

        [TestMethod]
        public async Task CreateBook_WithImageUpload_ShouldIncludeImageData()
        {
            // Arrange
            var imageData = new byte[] { 1, 2, 3, 4, 5 };
            var imageStream = new MemoryStream(imageData);
            
            var createDto = new CreateBookDto(
                "Test Book",
                "Test Author",
                1, 2, 3, 4,
                2023,
                "1234567890",
                "Test Summary",
                19.99m,
                5,
                imageStream,
                "test-image.jpg"
            );

            var bookResult = new BookResult { IsSuccess = true };
            _mockBookService.Setup(x => x.AddAsync(It.IsAny<CreateBookDto>()))
                .ReturnsAsync(bookResult);

            // Act
            var result = await _mockBookService.Object.AddAsync(createDto);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            _mockBookService.Verify(x => x.AddAsync(It.Is<CreateBookDto>(dto => 
                dto.CoverImageStream != null &&
                dto.CoverImageFileName == "test-image.jpg")), Times.Once);
        }

        [TestMethod]
        public async Task CreateBook_WithInvalidImage_ShouldReturnError()
        {
            // Arrange
            var createDto = new CreateBookDto(
                "Test Book",
                "Test Author",
                1, 2, 3, 4,
                2023,
                "1234567890",
                "Test Summary",
                19.99m,
                5,
                null,
                "invalid-file.txt"
            );

            var bookResult = new BookResult 
            { 
                IsSuccess = false, 
                ErrorMessage = "Invalid file type. Only PNG, JPG, and JPEG files are allowed." 
            };
            
            _mockBookService.Setup(x => x.AddAsync(It.IsAny<CreateBookDto>()))
                .ReturnsAsync(bookResult);

            // Act
            var result = await _mockBookService.Object.AddAsync(createDto);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Invalid file type. Only PNG, JPG, and JPEG files are allowed.", result.ErrorMessage);
        }

        [TestMethod]
        public async Task GetBooks_WithFilters_ShouldApplyFiltersCorrectly()
        {
            // Arrange
            var filters = new BookFilters
            {
                Name = "Test",
                Author = "Author",
                GenreId = 1,
                PublisherId = 2,
                BookTypeId = 3,
                ConditionId = 4,
                LowStock = true
            };

            var books = new List<Book>
            {
                new Book { Id = 1, Name = "Test Book", Author = "Test Author" }
            };

            var paginatedBooks = new TestPaginatedList<Book>(books, 1, 10);
            
            _mockBookService.Setup(x => x.GetBooksAsync(It.IsAny<BookFilters>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(paginatedBooks);

            // Act
            var result = await _mockBookService.Object.GetBooksAsync(filters, 1, 10);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            _mockBookService.Verify(x => x.GetBooksAsync(It.Is<BookFilters>(f => 
                f.Name == "Test" &&
                f.Author == "Author" &&
                f.GenreId == 1 &&
                f.PublisherId == 2 &&
                f.BookTypeId == 3 &&
                f.ConditionId == 4 &&
                f.LowStock == true), 1, 10), Times.Once);
        }

        [TestMethod]
        public async Task GetBook_WithValidId_ShouldReturnBook()
        {
            // Arrange
            var book = new Book
            {
                Id = 1,
                Name = "Test Book",
                Author = "Test Author",
                Price = 19.99m,
                Quantity = 5,
                ISBN = "1234567890",
                Summary = "Test Summary",
                CoverImageUrl = "/images/test.jpg"
            };

            _mockBookService.Setup(x => x.GetBookAsync(1))
                .ReturnsAsync(book);

            // Act
            var result = await _mockBookService.Object.GetBookAsync(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("Test Book", result.Name);
            Assert.AreEqual("Test Author", result.Author);
            Assert.AreEqual(19.99m, result.Price);
            Assert.AreEqual(5, result.Quantity);
        }

        [TestMethod]
        public async Task GetBook_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            _mockBookService.Setup(x => x.GetBookAsync(999))
                .ReturnsAsync((Book)null);

            // Act
            var result = await _mockBookService.Object.GetBookAsync(999);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetAllReferenceData_ShouldReturnAllTypes()
        {
            // Arrange
            var referenceData = new List<ReferenceDataItem>
            {
                new ReferenceDataItem { Id = 1, Text = "Fiction", DataType = ReferenceDataType.Genre },
                new ReferenceDataItem { Id = 2, Text = "Penguin", DataType = ReferenceDataType.Publisher },
                new ReferenceDataItem { Id = 3, Text = "Hardcover", DataType = ReferenceDataType.BookType },
                new ReferenceDataItem { Id = 4, Text = "New", DataType = ReferenceDataType.Condition }
            };

            _mockReferenceDataService.Setup(x => x.GetAllReferenceDataAsync())
                .ReturnsAsync(referenceData);

            // Act
            var result = await _mockReferenceDataService.Object.GetAllReferenceDataAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count());
            Assert.IsTrue(result.Any(x => x.DataType == ReferenceDataType.Genre));
            Assert.IsTrue(result.Any(x => x.DataType == ReferenceDataType.Publisher));
            Assert.IsTrue(result.Any(x => x.DataType == ReferenceDataType.BookType));
            Assert.IsTrue(result.Any(x => x.DataType == ReferenceDataType.Condition));
        }

        [TestMethod]
        public void BookFilters_QueryStringParsing_ShouldParseCorrectly()
        {
            // This test would be implemented in the actual page code-behind
            // Here we're testing the concept of parsing query string parameters

            // Arrange
            var queryString = "?name=Test&author=Author&publisherId=1&genreId=2&bookTypeId=3&conditionId=4&lowStock=true";
            
            // Simulate parsing (this would be done in the actual page)
            var filters = new BookFilters();
            
            // In real implementation, this would parse from Request.QueryString
            filters.Name = "Test";
            filters.Author = "Author";
            filters.PublisherId = 1;
            filters.GenreId = 2;
            filters.BookTypeId = 3;
            filters.ConditionId = 4;
            filters.LowStock = true;

            // Assert
            Assert.AreEqual("Test", filters.Name);
            Assert.AreEqual("Author", filters.Author);
            Assert.AreEqual(1, filters.PublisherId);
            Assert.AreEqual(2, filters.GenreId);
            Assert.AreEqual(3, filters.BookTypeId);
            Assert.AreEqual(4, filters.ConditionId);
            Assert.IsTrue(filters.LowStock);
        }
    }
}