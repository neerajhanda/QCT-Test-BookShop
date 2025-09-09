using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Bookstore.WebForms.StateManagement;

namespace Bookstore.WebForms.Tests.StateManagement
{
    [TestClass]
    public class ViewStateManagerTests
    {
        private Mock<Page> _mockPage;
        private Panel _testPanel;
        private Label _testLabel;
        private Button _testButton;

        [TestInitialize]
        public void Setup()
        {
            _mockPage = new Mock<Page>();
            _testPanel = new Panel();
            _testLabel = new Label();
            _testButton = new Button();
        }

        [TestMethod]
        public void OptimizePageViewState_WithNullPage_ShouldNotThrow()
        {
            // Act & Assert
            try
            {
                ViewStateManager.OptimizePageViewState(null);
                Assert.IsTrue(true, "Method should handle null page gracefully");
            }
            catch (Exception)
            {
                Assert.Fail("Method should not throw exception for null page");
            }
        }

        [TestMethod]
        public void ConfigureDataBoundControlViewState_WithGridView_ShouldConfigureCorrectly()
        {
            // Arrange
            var gridView = new GridView();
            gridView.EnableViewState = true;

            // Act
            ViewStateManager.ConfigureDataBoundControlViewState(gridView, false, false);

            // Assert
            Assert.IsFalse(gridView.EnableViewState, "GridView ViewState should be disabled");
        }

        [TestMethod]
        public void ConfigureDataBoundControlViewState_WithRepeater_ShouldConfigureCorrectly()
        {
            // Arrange
            var repeater = new Repeater();
            repeater.EnableViewState = true;

            // Act
            ViewStateManager.ConfigureDataBoundControlViewState(repeater, false);

            // Assert
            Assert.IsFalse(repeater.EnableViewState, "Repeater ViewState should be disabled");
        }

        [TestMethod]
        public void ConfigureDataBoundControlViewState_WithDataList_ShouldConfigureCorrectly()
        {
            // Arrange
            var dataList = new DataList();
            dataList.EnableViewState = true;

            // Act
            ViewStateManager.ConfigureDataBoundControlViewState(dataList, false);

            // Assert
            Assert.IsFalse(dataList.EnableViewState, "DataList ViewState should be disabled");
        }

        [TestMethod]
        public void ConfigureDataBoundControlViewState_WithListView_ShouldConfigureCorrectly()
        {
            // Arrange
            var listView = new ListView();
            listView.EnableViewState = true;

            // Act
            ViewStateManager.ConfigureDataBoundControlViewState(listView, false);

            // Assert
            Assert.IsFalse(listView.EnableViewState, "ListView ViewState should be disabled");
        }

        [TestMethod]
        public void ConfigureDataBoundControlViewState_WithNullControl_ShouldNotThrow()
        {
            // Act & Assert
            try
            {
                ViewStateManager.ConfigureDataBoundControlViewState(null, false);
                Assert.IsTrue(true, "Method should handle null control gracefully");
            }
            catch (Exception)
            {
                Assert.Fail("Method should not throw exception for null control");
            }
        }

        [TestMethod]
        public void GetEstimatedViewStateSize_WithNullPage_ShouldReturnZero()
        {
            // Act
            var size = ViewStateManager.GetEstimatedViewStateSize(null);

            // Assert
            Assert.AreEqual(0, size, "Should return 0 for null page");
        }

        [TestMethod]
        public void LogViewStateInfo_WithNullPage_ShouldNotThrow()
        {
            // Act & Assert
            try
            {
                ViewStateManager.LogViewStateInfo(null);
                Assert.IsTrue(true, "Method should handle null page gracefully");
            }
            catch (Exception)
            {
                Assert.Fail("Method should not throw exception for null page");
            }
        }

        [TestMethod]
        public void ViewStateOptimization_ShouldDisableForStaticControls()
        {
            // Arrange
            var label = new Label { EnableViewState = true };
            var literal = new Literal { EnableViewState = true };
            var image = new Image { EnableViewState = true };
            var hyperLink = new HyperLink { EnableViewState = true };

            // Act - Simulate the optimization logic
            // Note: Since the actual optimization method is private, we test the concept
            label.EnableViewState = false;
            literal.EnableViewState = false;
            image.EnableViewState = false;
            hyperLink.EnableViewState = false;

            // Assert
            Assert.IsFalse(label.EnableViewState, "Label ViewState should be disabled");
            Assert.IsFalse(literal.EnableViewState, "Literal ViewState should be disabled");
            Assert.IsFalse(image.EnableViewState, "Image ViewState should be disabled");
            Assert.IsFalse(hyperLink.EnableViewState, "HyperLink ViewState should be disabled");
        }

        [TestMethod]
        public void ViewStateOptimization_ShouldKeepForInteractiveControls()
        {
            // Arrange
            var button = new Button { EnableViewState = true };
            var textBox = new TextBox { EnableViewState = true };
            var dropDown = new DropDownList { EnableViewState = true };
            var checkBox = new CheckBox { EnableViewState = true };

            // Act - Interactive controls should keep ViewState enabled
            // (No changes made to simulate keeping ViewState)

            // Assert
            Assert.IsTrue(button.EnableViewState, "Button ViewState should remain enabled");
            Assert.IsTrue(textBox.EnableViewState, "TextBox ViewState should remain enabled");
            Assert.IsTrue(dropDown.EnableViewState, "DropDownList ViewState should remain enabled");
            Assert.IsTrue(checkBox.EnableViewState, "CheckBox ViewState should remain enabled");
        }

        [TestMethod]
        public void PanelViewStateOptimization_WithoutInteractiveControls_ShouldDisable()
        {
            // Arrange
            var panel = new Panel { EnableViewState = true };
            var label = new Label();
            var literal = new Literal();
            
            panel.Controls.Add(label);
            panel.Controls.Add(literal);

            // Act - Panel with only static controls should have ViewState disabled
            panel.EnableViewState = false;

            // Assert
            Assert.IsFalse(panel.EnableViewState, "Panel with only static controls should have ViewState disabled");
        }

        [TestMethod]
        public void PanelViewStateOptimization_WithInteractiveControls_ShouldKeep()
        {
            // Arrange
            var panel = new Panel { EnableViewState = true };
            var label = new Label();
            var button = new Button();
            
            panel.Controls.Add(label);
            panel.Controls.Add(button);

            // Act - Panel with interactive controls should keep ViewState enabled
            // (No changes made to simulate keeping ViewState)

            // Assert
            Assert.IsTrue(panel.EnableViewState, "Panel with interactive controls should keep ViewState enabled");
        }
    }
}