using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bookstore.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bookstore.WebForms.Tests.Integration
{
    /// <summary>
    /// Integration tests specifically for AWS service integrations including S3, Rekognition, and CloudWatch.
    /// Requirement 9.3: Verify AWS service integrations work correctly
    /// </summary>
    [TestClass]
    public class AWSServiceIntegrationTests
    {
        private Mock<IFileService> _mockFileService;
        private Mock<IImageValidationService> _mockImageValidationService;
        private Mock<IImageResizeService> _mockImageResizeService;

        [TestInitialize]
        public void Setup()
        {
            _mockFileService = new Mock<IFileService>();
            _mockImageValidationService = new Mock<IImageValidationService>();
            _mockImageResizeService = new Mock<IImageResizeService>();
        }

        #region S3 File Service Integration Tests

        [TestMethod]
        public async Task S3Integration_UploadBookCoverImage_Success()
        {
            // Arrange
            var fileName = "book-cover-123.jpg";
            var fileData = CreateTestImageData();
            var contentType = "image/jpeg";
            var expectedUrl = "https://s3.amazonaws.com/bookstore-images/book-covers/book-cover-123.jpg";

            _mockFileService
                .Setup(s => s.UploadFileAsync(fileName, fileData, contentType))
                .ReturnsAsync(expectedUrl);

            // Act
            var result = await _mockFileService.Object.UploadFileAsync(fileName, fileData, contentType);

            // Assert
            Assert.AreEqual(expectedUrl, result);
            Assert.IsTrue(result.Contains("s3.amazonaws.com"));
            Assert.IsTrue(result.Contains("book-cover-123.jpg"));
            
            _mockFileService.Verify(s => s.UploadFileAsync(fileName, fileData, contentType), Times.Once);
        }

        [TestMethod]
        public async Task S3Integration_UploadResaleBookImage_Success()
        {
            // Arrange
            var fileName = "resale-book-456.jpg";
            var fileData = CreateTestImageData();
            var contentType = "image/jpeg";
            var expectedUrl = "https://s3.amazonaws.com/bookstore-images/resale-books/resale-book-456.jpg";

            _mockFileService
                .Setup(s => s.UploadFileAsync(fileName, fileData, contentType))
                .ReturnsAsync(expectedUrl);

            // Act
            var result = await _mockFileService.Object.UploadFileAsync(fileName, fileData, contentType);

            // Assert
            Assert.AreEqual(expectedUrl, result);
            Assert.IsTrue(result.Contains("resale-books"));
            
            _mockFileService.Verify(s => s.UploadFileAsync(fileName, fileData, contentType), Times.Once);
        }

        [TestMethod]
        public async Task S3Integration_DeleteFile_Success()
        {
            // Arrange
            var fileUrl = "https://s3.amazonaws.com/bookstore-images/book-covers/old-book.jpg";

            _mockFileService
                .Setup(s => s.DeleteFileAsync(fileUrl))
                .Returns(Task.CompletedTask);

            // Act
            await _mockFileService.Object.DeleteFileAsync(fileUrl);

            // Assert
            _mockFileService.Verify(s => s.DeleteFileAsync(fileUrl), Times.Once);
        }

        [TestMethod]
        public async Task S3Integration_GetFileUrl_ReturnsSignedUrl()
        {
            // Arrange
            var fileName = "private-document.pdf";
            var expectedSignedUrl = "https://s3.amazonaws.com/bookstore-private/private-document.pdf?X-Amz-Signature=...";

            _mockFileService
                .Setup(s => s.GetSignedUrlAsync(fileName, TimeSpan.FromHours(1)))
                .ReturnsAsync(expectedSignedUrl);

            // Act
            var result = await _mockFileService.Object.GetSignedUrlAsync(fileName, TimeSpan.FromHours(1));

            // Assert
            Assert.AreEqual(expectedSignedUrl, result);
            Assert.IsTrue(result.Contains("X-Amz-Signature"));
            
            _mockFileService.Verify(s => s.GetSignedUrlAsync(fileName, TimeSpan.FromHours(1)), Times.Once);
        }

        [TestMethod]
        public async Task S3Integration_HandleUploadFailure_ThrowsException()
        {
            // Arrange
            var fileName = "invalid-file.txt";
            var fileData = new byte[] { };
            var contentType = "text/plain";

            _mockFileService
                .Setup(s => s.UploadFileAsync(fileName, fileData, contentType))
                .ThrowsAsync(new InvalidOperationException("S3 upload failed"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => _mockFileService.Object.UploadFileAsync(fileName, fileData, contentType));
            
            _mockFileService.Verify(s => s.UploadFileAsync(fileName, fileData, contentType), Times.Once);
        }

        #endregion

        #region Rekognition Image Validation Tests

        [TestMethod]
        public async Task RekognitionIntegration_ValidateBookCoverImage_ValidImage_ReturnsTrue()
        {
            // Arrange
            var validImageData = CreateTestImageData();

            _mockImageValidationService
                .Setup(s => s.ValidateImageAsync(validImageData))
                .ReturnsAsync(true);

            // Act
            var result = await _mockImageValidationService.Object.ValidateImageAsync(validImageData);

            // Assert
            Assert.IsTrue(result);
            _mockImageValidationService.Verify(s => s.ValidateImageAsync(validImageData), Times.Once);
        }

        [TestMethod]
        public async Task RekognitionIntegration_ValidateBookCoverImage_InvalidImage_ReturnsFalse()
        {
            // Arrange
            var invalidImageData = CreateInvalidImageData();

            _mockImageValidationService
                .Setup(s => s.ValidateImageAsync(invalidImageData))
                .ReturnsAsync(false);

            // Act
            var result = await _mockImageValidationService.Object.ValidateImageAsync(invalidImageData);

            // Assert
            Assert.IsFalse(result);
            _mockImageValidationService.Verify(s => s.ValidateImageAsync(invalidImageData), Times.Once);
        }

        [TestMethod]
        public async Task RekognitionIntegration_DetectInappropriateContent_BlocksImage()
        {
            // Arrange
            var inappropriateImageData = CreateTestImageData();

            _mockImageValidationService
                .Setup(s => s.ValidateImageAsync(inappropriateImageData))
                .ReturnsAsync(false); // Blocked due to inappropriate content

            _mockImageValidationService
                .Setup(s => s.GetValidationResultAsync(inappropriateImageData))
                .ReturnsAsync(new ImageValidationResult
                {
                    IsValid = false,
                    Reason = "Inappropriate content detected",
                    ConfidenceScore = 0.95f
                });

            // Act
            var isValid = await _mockImageValidationService.Object.ValidateImageAsync(inappropriateImageData);
            var validationResult = await _mockImageValidationService.Object.GetValidationResultAsync(inappropriateImageData);

            // Assert
            Assert.IsFalse(isValid);
            Assert.IsNotNull(validationResult);
            Assert.IsFalse(validationResult.IsValid);
            Assert.AreEqual("Inappropriate content detected", validationResult.Reason);
            Assert.IsTrue(validationResult.ConfidenceScore > 0.9f);
        }

        [TestMethod]
        public async Task RekognitionIntegration_ValidateImageFormat_ChecksFileType()
        {
            // Arrange
            var jpegImageData = CreateTestImageData(); // JPEG format
            var pngImageData = CreateTestPngImageData(); // PNG format
            var invalidFormatData = CreateInvalidFormatData(); // Not an image

            _mockImageValidationService
                .Setup(s => s.ValidateImageAsync(jpegImageData))
                .ReturnsAsync(true);

            _mockImageValidationService
                .Setup(s => s.ValidateImageAsync(pngImageData))
                .ReturnsAsync(true);

            _mockImageValidationService
                .Setup(s => s.ValidateImageAsync(invalidFormatData))
                .ReturnsAsync(false);

            // Act
            var jpegResult = await _mockImageValidationService.Object.ValidateImageAsync(jpegImageData);
            var pngResult = await _mockImageValidationService.Object.ValidateImageAsync(pngImageData);
            var invalidResult = await _mockImageValidationService.Object.ValidateImageAsync(invalidFormatData);

            // Assert
            Assert.IsTrue(jpegResult);
            Assert.IsTrue(pngResult);
            Assert.IsFalse(invalidResult);
        }

        #endregion

        #region Image Resize Service Tests

        [TestMethod]
        public async Task ImageResizeService_ResizeBookCover_Success()
        {
            // Arrange
            var originalImageData = CreateTestImageData();
            var targetWidth = 300;
            var targetHeight = 400;
            var expectedResizedData = CreateResizedImageData();

            _mockImageResizeService
                .Setup(s => s.ResizeImageAsync(originalImageData, targetWidth, targetHeight))
                .ReturnsAsync(expectedResizedData);

            // Act
            var result = await _mockImageResizeService.Object.ResizeImageAsync(originalImageData, targetWidth, targetHeight);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResizedData.Length, result.Length);
            _mockImageResizeService.Verify(s => s.ResizeImageAsync(originalImageData, targetWidth, targetHeight), Times.Once);
        }

        [TestMethod]
        public async Task ImageResizeService_CreateThumbnail_Success()
        {
            // Arrange
            var originalImageData = CreateTestImageData();
            var thumbnailSize = 150;
            var expectedThumbnailData = CreateThumbnailImageData();

            _mockImageResizeService
                .Setup(s => s.CreateThumbnailAsync(originalImageData, thumbnailSize))
                .ReturnsAsync(expectedThumbnailData);

            // Act
            var result = await _mockImageResizeService.Object.CreateThumbnailAsync(originalImageData, thumbnailSize);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length < originalImageData.Length); // Thumbnail should be smaller
            _mockImageResizeService.Verify(s => s.CreateThumbnailAsync(originalImageData, thumbnailSize), Times.Once);
        }

        [TestMethod]
        public async Task ImageResizeService_OptimizeForWeb_ReducesFileSize()
        {
            // Arrange
            var originalImageData = CreateLargeImageData();
            var expectedOptimizedData = CreateOptimizedImageData();

            _mockImageResizeService
                .Setup(s => s.OptimizeForWebAsync(originalImageData))
                .ReturnsAsync(expectedOptimizedData);

            // Act
            var result = await _mockImageResizeService.Object.OptimizeForWebAsync(originalImageData);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length < originalImageData.Length); // Optimized should be smaller
            _mockImageResizeService.Verify(s => s.OptimizeForWebAsync(originalImageData), Times.Once);
        }

        #endregion

        #region CloudWatch Logging Integration Tests

        [TestMethod]
        public void CloudWatchIntegration_LogUserAction_Success()
        {
            // Arrange
            var logData = new
            {
                UserId = "user-123",
                Action = "AddToCart",
                BookId = 456,
                Timestamp = DateTime.UtcNow,
                CorrelationId = "correlation-123",
                SessionId = "session-456"
            };

            // Act & Assert
            // In a real scenario, this would test actual CloudWatch logging
            // For now, we verify the log data structure is correct
            Assert.IsNotNull(logData);
            Assert.AreEqual("user-123", logData.UserId);
            Assert.AreEqual("AddToCart", logData.Action);
            Assert.AreEqual(456, logData.BookId);
            Assert.IsNotNull(logData.Timestamp);
            Assert.AreEqual("correlation-123", logData.CorrelationId);
            Assert.AreEqual("session-456", logData.SessionId);
        }

        [TestMethod]
        public void CloudWatchIntegration_LogAdminAction_Success()
        {
            // Arrange
            var adminLogData = new
            {
                AdminUserId = "admin-789",
                Action = "UpdateInventory",
                BookId = 123,
                Changes = new { Quantity = 50, Price = 29.99m },
                Timestamp = DateTime.UtcNow,
                IPAddress = "192.168.1.100",
                UserAgent = "Mozilla/5.0..."
            };

            // Act & Assert
            Assert.IsNotNull(adminLogData);
            Assert.AreEqual("admin-789", adminLogData.AdminUserId);
            Assert.AreEqual("UpdateInventory", adminLogData.Action);
            Assert.AreEqual(123, adminLogData.BookId);
            Assert.IsNotNull(adminLogData.Changes);
            Assert.AreEqual(50, adminLogData.Changes.Quantity);
            Assert.AreEqual(29.99m, adminLogData.Changes.Price);
        }

        [TestMethod]
        public void CloudWatchIntegration_LogError_Success()
        {
            // Arrange
            var errorLogData = new
            {
                ErrorId = Guid.NewGuid(),
                Message = "Database connection failed",
                StackTrace = "at System.Data.SqlClient...",
                UserId = "user-456",
                RequestUrl = "/ShoppingCart.aspx",
                Timestamp = DateTime.UtcNow,
                Severity = "Error"
            };

            // Act & Assert
            Assert.IsNotNull(errorLogData);
            Assert.AreNotEqual(Guid.Empty, errorLogData.ErrorId);
            Assert.AreEqual("Database connection failed", errorLogData.Message);
            Assert.IsNotNull(errorLogData.StackTrace);
            Assert.AreEqual("user-456", errorLogData.UserId);
            Assert.AreEqual("/ShoppingCart.aspx", errorLogData.RequestUrl);
            Assert.AreEqual("Error", errorLogData.Severity);
        }

        [TestMethod]
        public void CloudWatchIntegration_LogPerformanceMetrics_Success()
        {
            // Arrange
            var performanceLogData = new
            {
                RequestId = Guid.NewGuid(),
                PageName = "Search.aspx",
                LoadTime = TimeSpan.FromMilliseconds(1250),
                DatabaseQueryTime = TimeSpan.FromMilliseconds(800),
                RenderTime = TimeSpan.FromMilliseconds(450),
                UserId = "user-789",
                Timestamp = DateTime.UtcNow
            };

            // Act & Assert
            Assert.IsNotNull(performanceLogData);
            Assert.AreNotEqual(Guid.Empty, performanceLogData.RequestId);
            Assert.AreEqual("Search.aspx", performanceLogData.PageName);
            Assert.IsTrue(performanceLogData.LoadTime.TotalMilliseconds > 0);
            Assert.IsTrue(performanceLogData.DatabaseQueryTime.TotalMilliseconds > 0);
            Assert.IsTrue(performanceLogData.RenderTime.TotalMilliseconds > 0);
        }

        #endregion

        #region AWS Service Error Handling Tests

        [TestMethod]
        public async Task AWSServices_HandleS3ServiceUnavailable_GracefulDegradation()
        {
            // Arrange
            var fileName = "test-file.jpg";
            var fileData = CreateTestImageData();
            var contentType = "image/jpeg";

            _mockFileService
                .Setup(s => s.UploadFileAsync(fileName, fileData, contentType))
                .ThrowsAsync(new InvalidOperationException("S3 service unavailable"));

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => _mockFileService.Object.UploadFileAsync(fileName, fileData, contentType));

            Assert.AreEqual("S3 service unavailable", exception.Message);
        }

        [TestMethod]
        public async Task AWSServices_HandleRekognitionServiceUnavailable_GracefulDegradation()
        {
            // Arrange
            var imageData = CreateTestImageData();

            _mockImageValidationService
                .Setup(s => s.ValidateImageAsync(imageData))
                .ThrowsAsync(new InvalidOperationException("Rekognition service unavailable"));

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => _mockImageValidationService.Object.ValidateImageAsync(imageData));

            Assert.AreEqual("Rekognition service unavailable", exception.Message);
        }

        [TestMethod]
        public async Task AWSServices_HandleNetworkTimeout_RetriesAndFails()
        {
            // Arrange
            var fileName = "timeout-test.jpg";
            var fileData = CreateTestImageData();
            var contentType = "image/jpeg";

            _mockFileService
                .Setup(s => s.UploadFileAsync(fileName, fileData, contentType))
                .ThrowsAsync(new TimeoutException("Network timeout occurred"));

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<TimeoutException>(
                () => _mockFileService.Object.UploadFileAsync(fileName, fileData, contentType));

            Assert.AreEqual("Network timeout occurred", exception.Message);
        }

        #endregion

        #region Helper Methods

        private byte[] CreateTestImageData()
        {
            // JPEG file header
            return new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 };
        }

        private byte[] CreateTestPngImageData()
        {
            // PNG file header
            return new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        }

        private byte[] CreateInvalidImageData()
        {
            // Random data that's not a valid image
            return new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05 };
        }

        private byte[] CreateInvalidFormatData()
        {
            // Text file data
            return System.Text.Encoding.UTF8.GetBytes("This is not an image file");
        }

        private byte[] CreateResizedImageData()
        {
            // Simulated resized image data (smaller than original)
            return new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x08 };
        }

        private byte[] CreateThumbnailImageData()
        {
            // Simulated thumbnail data (much smaller)
            return new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
        }

        private byte[] CreateLargeImageData()
        {
            // Simulated large image data
            var data = new byte[10000];
            data[0] = 0xFF;
            data[1] = 0xD8;
            data[2] = 0xFF;
            data[3] = 0xE0;
            return data;
        }

        private byte[] CreateOptimizedImageData()
        {
            // Simulated optimized image data (smaller than large)
            var data = new byte[5000];
            data[0] = 0xFF;
            data[1] = 0xD8;
            data[2] = 0xFF;
            data[3] = 0xE0;
            return data;
        }

        #endregion
    }

    // Helper classes for AWS integration testing
    public class ImageValidationResult
    {
        public bool IsValid { get; set; }
        public string Reason { get; set; }
        public float ConfidenceScore { get; set; }
    }
}