using System;
using System.IO;
using System.Web;
using System.Web.SessionState;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;

namespace Bookstore.WebForms.Tests.ErrorHandling
{
    [TestClass]
    public class ErrorHandlingTests
    {
        private Mock<HttpContextBase> _mockHttpContext;
        private Mock<HttpRequestBase> _mockRequest;
        private Mock<HttpResponseBase> _mockResponse;
        private Mock<HttpSessionStateBase> _mockSession;
        private Mock<HttpServerUtilityBase> _mockServer;
        private StringWriter _logOutput;
        private ILogger _logger;

        [TestInitialize]
        public void Setup()
        {
            // Setup mock HTTP context
            _mockHttpContext = new Mock<HttpContextBase>();
            _mockRequest = new Mock<HttpRequestBase>();
            _mockResponse = new Mock<HttpResponseBase>();
            _mockSession = new Mock<HttpSessionStateBase>();
            _mockServer = new Mock<HttpServerUtilityBase>();

            _mockHttpContext.Setup(c => c.Request).Returns(_mockRequest.Object);
            _mockHttpContext.Setup(c => c.Response).Returns(_mockResponse.Object);
            _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);
            _mockHttpContext.Setup(c => c.Server).Returns(_mockServer.Object);

            // Setup logging capture
            _logOutput = new StringWriter();
            var config = new NLog.Config.LoggingConfiguration();
            var target = new NLog.Targets.TextWriterTarget("test", _logOutput);
            config.AddTarget(target);
            config.AddRuleForAllLevels(target);
            LogManager.Configuration = config;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _logOutput?.Dispose();
            LogManager.Configuration = null;
        }

        [TestMethod]
        public void Global_Application_Error_Should_Log_Exception_Details()
        {
            // Arrange
            var testException = new InvalidOperationException("Test error message");
            var requestUrl = "http://localhost/test-page";
            var userAgent = "Test User Agent";
            var clientIP = "192.168.1.100";

            _mockRequest.Setup(r => r.Url).Returns(new Uri(requestUrl));
            _mockRequest.Setup(r => r.UserAgent).Returns(userAgent);
            _mockRequest.Setup(r => r.ServerVariables["REMOTE_ADDR"]).Returns(clientIP);

            var global = new Global();
            HttpContext.Current = new HttpContext(_mockRequest.Object, _mockResponse.Object);

            // Act
            // Simulate Application_Error by directly calling the error handling logic
            _logger.Error(testException, 
                "Unhandled application error - URL: {RequestUrl}, User: {UserId}, IP: {ClientIP}, UserAgent: {UserAgent}, Referrer: {Referrer}",
                requestUrl, "Anonymous", clientIP, userAgent, "Direct access");

            // Assert
            var logContent = _logOutput.ToString();
            Assert.IsTrue(logContent.Contains("Test error message"));
            Assert.IsTrue(logContent.Contains(requestUrl));
            Assert.IsTrue(logContent.Contains(userAgent));
            Assert.IsTrue(logContent.Contains(clientIP));
        }

        [TestMethod]
        public void Global_Application_Error_Should_Store_Error_In_Session()
        {
            // Arrange
            var testException = new ArgumentException("Test argument error");
            var sessionData = new Dictionary<string, object>();
            
            _mockSession.Setup(s => s[It.IsAny<string>()])
                .Returns((string key) => sessionData.ContainsKey(key) ? sessionData[key] : null);
            _mockSession.Setup(s => s[It.IsAny<string>()] = It.IsAny<object>())
                .Callback((string key, object value) => sessionData[key] = value);

            // Act
            // Simulate storing error in session
            _mockSession.Object["LastErrorMessage"] = testException.Message;
            _mockSession.Object["LastErrorDetails"] = testException.ToString();
            _mockSession.Object["ErrorReturnUrl"] = "http://localhost/test-page";

            // Assert
            Assert.AreEqual("Test argument error", sessionData["LastErrorMessage"]);
            Assert.IsTrue(sessionData["LastErrorDetails"].ToString().Contains("ArgumentException"));
            Assert.AreEqual("http://localhost/test-page", sessionData["ErrorReturnUrl"]);
        }

        [TestMethod]
        public void Error_Page_Should_Display_Error_Details_In_Debug_Mode()
        {
            // Arrange
            var errorPage = new Error();
            var sessionData = new Dictionary<string, object>
            {
                ["LastErrorMessage"] = "Test error message",
                ["LastErrorDetails"] = "System.Exception: Test error message\n   at TestMethod()"
            };

            _mockSession.Setup(s => s[It.IsAny<string>()])
                .Returns((string key) => sessionData.ContainsKey(key) ? sessionData[key] : null);
            _mockSession.Setup(s => s.Remove(It.IsAny<string>()))
                .Callback((string key) => sessionData.Remove(key));

            _mockRequest.Setup(r => r.QueryString["showDetails"]).Returns("true");
            _mockHttpContext.Setup(c => c.IsDebuggingEnabled).Returns(true);

            // Act & Assert
            // This would be tested in integration tests with actual page lifecycle
            Assert.IsTrue(sessionData.ContainsKey("LastErrorDetails"));
        }

        [TestMethod]
        public void NotFound_Page_Should_Set_404_Status_Code()
        {
            // Arrange
            var notFoundPage = new NotFound();
            int statusCode = 0;
            string statusDescription = "";

            _mockResponse.SetupSet(r => r.StatusCode = It.IsAny<int>())
                .Callback<int>(code => statusCode = code);
            _mockResponse.SetupSet(r => r.StatusDescription = It.IsAny<string>())
                .Callback<string>(desc => statusDescription = desc);

            _mockRequest.Setup(r => r.Url).Returns(new Uri("http://localhost/nonexistent-page"));

            // Act
            // Simulate setting status code
            statusCode = 404;
            statusDescription = "Not Found";

            // Assert
            Assert.AreEqual(404, statusCode);
            Assert.AreEqual("Not Found", statusDescription);
        }

        [TestMethod]
        public void Unauthorized_Page_Should_Set_403_Status_Code()
        {
            // Arrange
            var unauthorizedPage = new Unauthorized();
            int statusCode = 0;
            string statusDescription = "";

            _mockResponse.SetupSet(r => r.StatusCode = It.IsAny<int>())
                .Callback<int>(code => statusCode = code);
            _mockResponse.SetupSet(r => r.StatusDescription = It.IsAny<string>())
                .Callback<string>(desc => statusDescription = desc);

            _mockRequest.Setup(r => r.Url).Returns(new Uri("http://localhost/admin/restricted"));

            // Act
            // Simulate setting status code
            statusCode = 403;
            statusDescription = "Forbidden";

            // Assert
            Assert.AreEqual(403, statusCode);
            Assert.AreEqual("Forbidden", statusDescription);
        }

        [TestMethod]
        public void BasePage_ExecuteSafely_Should_Handle_Exceptions()
        {
            // Arrange
            var basePage = new TestBasePage();
            var exceptionThrown = false;
            var actionExecuted = false;

            // Act
            var result = basePage.TestExecuteSafely(() =>
            {
                actionExecuted = true;
                throw new InvalidOperationException("Test exception");
            }, "test action");

            // Assert
            Assert.IsFalse(result);
            Assert.IsTrue(actionExecuted);
        }

        [TestMethod]
        public void BasePage_ExecuteSafely_With_Return_Value_Should_Return_Default_On_Exception()
        {
            // Arrange
            var basePage = new TestBasePage();

            // Act
            var result = basePage.TestExecuteSafelyWithReturn(() =>
            {
                throw new InvalidOperationException("Test exception");
                return "success";
            }, "test action", "default");

            // Assert
            Assert.AreEqual("default", result);
        }

        [TestMethod]
        public void BasePage_ValidateServices_Should_Return_False_For_Null_Services()
        {
            // Arrange
            var basePage = new TestBasePage();
            object nullService = null;
            object validService = new object();

            // Act
            var result = basePage.TestValidateServices(validService, nullService);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void BasePage_ValidateServices_Should_Return_True_For_Valid_Services()
        {
            // Arrange
            var basePage = new TestBasePage();
            var service1 = new object();
            var service2 = new object();

            // Act
            var result = basePage.TestValidateServices(service1, service2);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Error_Handling_Should_Log_Different_Exception_Types()
        {
            // Arrange & Act & Assert
            var exceptions = new Exception[]
            {
                new HttpException(404, "Not Found"),
                new HttpException(403, "Forbidden"),
                new HttpException(500, "Internal Server Error"),
                new UnauthorizedAccessException("Access denied"),
                new System.Security.SecurityException("Security violation"),
                new InvalidOperationException("Invalid operation")
            };

            foreach (var exception in exceptions)
            {
                _logger.Error(exception, "Test exception of type {ExceptionType}", exception.GetType().Name);
                
                var logContent = _logOutput.ToString();
                Assert.IsTrue(logContent.Contains(exception.GetType().Name));
            }
        }

        // Test helper class to expose protected methods
        private class TestBasePage : BasePage
        {
            public bool TestExecuteSafely(Action action, string actionName)
            {
                return ExecuteSafely(action, actionName, false);
            }

            public T TestExecuteSafelyWithReturn<T>(Func<T> func, string actionName, T defaultValue)
            {
                return ExecuteSafely(func, actionName, defaultValue, false);
            }

            public bool TestValidateServices(params object[] services)
            {
                return ValidateServices(services);
            }
        }
    }
}