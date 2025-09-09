using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Bookstore.WebForms;

namespace Bookstore.WebForms.Tests.DependencyInjection
{
    [TestClass]
    public class DependencyResolverTests
    {
        private Mock<IWebFormsDependencyResolver> _mockResolver;

        [TestInitialize]
        public void Setup()
        {
            _mockResolver = new Mock<IWebFormsDependencyResolver>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Reset the static resolver to avoid test interference
            try
            {
                DependencyResolver.SetResolver(null);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        [TestMethod]
        public void SetResolver_WithValidResolver_SetsCurrentResolver()
        {
            // Arrange & Act
            DependencyResolver.SetResolver(_mockResolver.Object);

            // Assert
            Assert.AreEqual(_mockResolver.Object, DependencyResolver.Current);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetResolver_WithNullResolver_ThrowsArgumentNullException()
        {
            // Act
            DependencyResolver.SetResolver(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Current_WhenNotInitialized_ThrowsInvalidOperationException()
        {
            // Act
            var resolver = DependencyResolver.Current;
        }

        [TestMethod]
        public void Resolve_Generic_CallsCurrentResolver()
        {
            // Arrange
            var expectedService = new TestService();
            _mockResolver.Setup(r => r.Resolve<ITestService>()).Returns(expectedService);
            DependencyResolver.SetResolver(_mockResolver.Object);

            // Act
            var result = DependencyResolver.Resolve<ITestService>();

            // Assert
            Assert.AreEqual(expectedService, result);
            _mockResolver.Verify(r => r.Resolve<ITestService>(), Times.Once);
        }

        [TestMethod]
        public void Resolve_ByType_CallsCurrentResolver()
        {
            // Arrange
            var expectedService = new TestService();
            _mockResolver.Setup(r => r.Resolve(typeof(ITestService))).Returns(expectedService);
            DependencyResolver.SetResolver(_mockResolver.Object);

            // Act
            var result = DependencyResolver.Resolve(typeof(ITestService));

            // Assert
            Assert.AreEqual(expectedService, result);
            _mockResolver.Verify(r => r.Resolve(typeof(ITestService)), Times.Once);
        }

        [TestMethod]
        public void TryResolve_Generic_CallsCurrentResolver()
        {
            // Arrange
            var expectedService = new TestService();
            _mockResolver.Setup(r => r.TryResolve<ITestService>(out expectedService)).Returns(true);
            DependencyResolver.SetResolver(_mockResolver.Object);

            // Act
            ITestService result;
            var success = DependencyResolver.TryResolve<ITestService>(out result);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual(expectedService, result);
        }

        [TestMethod]
        public void TryResolve_ByType_CallsCurrentResolver()
        {
            // Arrange
            var expectedService = new TestService();
            object outService = expectedService;
            _mockResolver.Setup(r => r.TryResolve(typeof(ITestService), out outService)).Returns(true);
            DependencyResolver.SetResolver(_mockResolver.Object);

            // Act
            object result;
            var success = DependencyResolver.TryResolve(typeof(ITestService), out result);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual(expectedService, result);
        }

        [TestMethod]
        public void InjectProperties_CallsCurrentResolver()
        {
            // Arrange
            var instance = new TestService();
            DependencyResolver.SetResolver(_mockResolver.Object);

            // Act
            DependencyResolver.InjectProperties(instance);

            // Assert
            _mockResolver.Verify(r => r.InjectProperties(instance), Times.Once);
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
    }
}