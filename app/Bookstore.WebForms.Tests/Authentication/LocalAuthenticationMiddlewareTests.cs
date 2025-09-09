using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Bookstore.Domain.Customers;
using Bookstore.WebForms;
using Microsoft.Owin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Bookstore.WebForms.Tests.Authentication
{
    [TestClass]
    public class LocalAuthenticationMiddlewareTests
    {
        private Mock<ICustomerService> _mockCustomerService;
        private Mock<OwinMiddleware> _mockNext;
        private LocalAuthenticationMiddleware _middleware;

        [TestInitialize]
        public void Setup()
        {
            _mockCustomerService = new Mock<ICustomerService>();
            _mockNext = new Mock<OwinMiddleware>(Mock.Of<OwinMiddleware>());
            _middleware = new LocalAuthenticationMiddleware(_mockNext.Object, _mockCustomerService.Object);
        }

        [TestMethod]
        public async Task Invoke_WhenPathIsLogin_HandlesLogin()
        {
            // Arrange
            var mockContext = CreateMockOwinContext("/login.aspx", new Dictionary<string, string[]>());
            var mockHttpContext = new Mock<HttpContextBase>();
            var mockResponse = new Mock<HttpResponseBase>();
            
            mockHttpContext.Setup(c => c.Response).Returns(mockResponse.Object);
            HttpContext.Current = mockHttpContext.Object.ApplicationInstance.Context;

            _mockCustomerService.Setup(s => s.CreateOrUpdateCustomerAsync(It.IsAny<CreateOrUpdateCustomerDto>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.Invoke(mockContext.Object);

            // Assert
            mockResponse.Verify(r => r.Redirect(It.IsAny<string>()), Times.Once);
            _mockCustomerService.Verify(s => s.CreateOrUpdateCustomerAsync(It.IsAny<CreateOrUpdateCustomerDto>()), Times.Once);
        }

        [TestMethod]
        public async Task Invoke_WhenPathIsLogout_HandlesLogout()
        {
            // Arrange
            var queryParams = new Dictionary<string, string[]>
            {
                { "action", new[] { "logout" } }
            };
            var mockContext = CreateMockOwinContext("/login.aspx", queryParams);
            var mockHttpContext = new Mock<HttpContextBase>();
            var mockResponse = new Mock<HttpResponseBase>();
            var mockRequest = new Mock<HttpRequestBase>();
            
            mockHttpContext.Setup(c => c.Response).Returns(mockResponse.Object);
            mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
            HttpContext.Current = mockHttpContext.Object.ApplicationInstance.Context;

            // Act
            await _middleware.Invoke(mockContext.Object);

            // Assert
            mockResponse.Verify(r => r.Redirect("/Default.aspx"), Times.Once);
        }

        [TestMethod]
        public async Task Invoke_WhenUserHasAuthCookie_CreatesPrincipal()
        {
            // Arrange
            var mockContext = CreateMockOwinContext("/some-page.aspx", new Dictionary<string, string[]>());
            var mockHttpContext = new Mock<HttpContextBase>();
            var mockRequest = new Mock<HttpRequestBase>();
            var mockCookies = new HttpCookieCollection();
            mockCookies.Add(new HttpCookie("LocalAuthentication", "authenticated"));
            
            mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
            mockRequest.Setup(r => r.Cookies).Returns(mockCookies);
            HttpContext.Current = mockHttpContext.Object.ApplicationInstance.Context;

            _mockCustomerService.Setup(s => s.CreateOrUpdateCustomerAsync(It.IsAny<CreateOrUpdateCustomerDto>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.Invoke(mockContext.Object);

            // Assert
            _mockNext.Verify(n => n.Invoke(mockContext.Object), Times.Once);
            _mockCustomerService.Verify(s => s.CreateOrUpdateCustomerAsync(It.IsAny<CreateOrUpdateCustomerDto>()), Times.Once);
        }

        [TestMethod]
        public async Task Invoke_WhenNoAuthCookie_CallsNext()
        {
            // Arrange
            var mockContext = CreateMockOwinContext("/some-page.aspx", new Dictionary<string, string[]>());
            var mockHttpContext = new Mock<HttpContextBase>();
            var mockRequest = new Mock<HttpRequestBase>();
            var mockCookies = new HttpCookieCollection();
            
            mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
            mockRequest.Setup(r => r.Cookies).Returns(mockCookies);
            HttpContext.Current = mockHttpContext.Object.ApplicationInstance.Context;

            // Act
            await _middleware.Invoke(mockContext.Object);

            // Assert
            _mockNext.Verify(n => n.Invoke(mockContext.Object), Times.Once);
            _mockCustomerService.Verify(s => s.CreateOrUpdateCustomerAsync(It.IsAny<CreateOrUpdateCustomerDto>()), Times.Never);
        }

        private Mock<IOwinContext> CreateMockOwinContext(string path, Dictionary<string, string[]> queryParams)
        {
            var mockContext = new Mock<IOwinContext>();
            var mockRequest = new Mock<IOwinRequest>();
            var mockResponse = new Mock<IOwinResponse>();

            mockRequest.Setup(r => r.Path).Returns(new PathString(path));
            mockRequest.Setup(r => r.Query).Returns(new ReadableStringCollection(queryParams));
            mockRequest.Setup(r => r.IsSecure).Returns(false);

            mockContext.Setup(c => c.Request).Returns(mockRequest.Object);
            mockContext.Setup(c => c.Response).Returns(mockResponse.Object);

            return mockContext;
        }
    }

    // Helper class to mock IReadableStringCollection
    public class ReadableStringCollection : IReadableStringCollection
    {
        private readonly Dictionary<string, string[]> _values;

        public ReadableStringCollection(Dictionary<string, string[]> values)
        {
            _values = values ?? new Dictionary<string, string[]>();
        }

        public string this[string key] => _values.ContainsKey(key) ? _values[key][0] : null;

        public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string[] GetValues(string key)
        {
            return _values.ContainsKey(key) ? _values[key] : null;
        }

        public bool ContainsKey(string key)
        {
            return _values.ContainsKey(key);
        }

        public ICollection<string> Keys => _values.Keys;
    }
}