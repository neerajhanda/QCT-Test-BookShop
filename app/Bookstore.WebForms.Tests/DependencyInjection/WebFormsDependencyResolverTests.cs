using System;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Autofac;
using Autofac.Integration.Owin;
using Microsoft.Owin;
using Bookstore.WebForms;

namespace Bookstore.WebForms.Tests.DependencyInjection
{
    [TestClass]
    public class WebFormsDependencyResolverTests
    {
        private WebFormsDependencyResolver _resolver;
        private Mock<ILifetimeScope> _mockLifetimeScope;
        private Mock<HttpContextBase> _mockHttpContext;
        private Mock<IOwinContext> _mockOwinContext;

        [TestInitialize]
        public void Setup()
        {
            _resolver = new WebFormsDependencyResolver();
            _mockLifetimeScope = new Mock<ILifetimeScope>();
            _mockHttpContext = new Mock<HttpContextBase>();
            _mockOwinContext = new Mock<IOwinContext>();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Resolve_Generic_WhenHttpContextIsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            HttpContext.Current = null;

            // Act
            _resolver.Resolve<ITestService>();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Resolve_ByType_WhenHttpContextIsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            HttpContext.Current = null;

            // Act
            _resolver.Resolve(typeof(ITestService));
        }

        [TestMethod]
        public void TryResolve_Generic_WhenHttpContextIsNull_ReturnsFalse()
        {
            // Arrange
            HttpContext.Current = null;

            // Act
            ITestService result;
            var success = _resolver.TryResolve<ITestService>(out result);

            // Assert
            Assert.IsFalse(success);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void TryResolve_ByType_WhenHttpContextIsNull_ReturnsFalse()
        {
            // Arrange
            HttpContext.Current = null;

            // Act
            object result;
            var success = _resolver.TryResolve(typeof(ITestService), out result);

            // Assert
            Assert.IsFalse(success);
            Assert.IsNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InjectProperties_WithNullInstance_ThrowsArgumentNullException()
        {
            // Act
            _resolver.InjectProperties(null);
        }

        // Test interfaces and classes
        public interface ITestService
        {
            string Name { get; }
        }

        public class TestService : ITestService
        {
            public string Name => "Test Service";
        }

        public class TestServiceWithDependency
        {
            public ITestService TestService { get; set; }
        }
    }
}