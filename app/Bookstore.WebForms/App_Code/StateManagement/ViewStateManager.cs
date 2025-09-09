using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using NLog;

namespace Bookstore.WebForms.StateManagement
{
    /// <summary>
    /// ViewState optimization and management utilities
    /// </summary>
    public static class ViewStateManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Optimizes ViewState for a page by disabling it for controls that don't need it
        /// </summary>
        /// <param name="page">The page to optimize</param>
        public static void OptimizePageViewState(Page page)
        {
            try
            {
                if (page == null) return;

                // Recursively optimize controls
                OptimizeControlViewState(page);
                
                Logger.Debug("ViewState optimization completed for page: {PageType}", page.GetType().Name);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error optimizing ViewState for page: {PageType}", page != null ? page.GetType().Name : "Unknown");
            }
        }

        /// <summary>
        /// Recursively optimizes ViewState for controls
        /// </summary>
        /// <param name="control">The control to optimize</param>
        private static void OptimizeControlViewState(Control control)
        {
            if (control == null) return;

            try
            {
                // Disable ViewState for controls that typically don't need it
                if (ShouldDisableViewState(control))
                {
                    control.EnableViewState = false;
                }

                // Recursively process child controls
                foreach (Control childControl in control.Controls)
                {
                    OptimizeControlViewState(childControl);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error optimizing ViewState for control: {ControlType}", control != null ? control.GetType().Name : "Unknown");
            }
        }

        /// <summary>
        /// Determines if ViewState should be disabled for a control
        /// </summary>
        /// <param name="control">The control to check</param>
        /// <returns>True if ViewState should be disabled</returns>
        private static bool ShouldDisableViewState(Control control)
        {
            // Controls that typically don't need ViewState
            var controlsToDisable = new HashSet<Type>
            {
                typeof(Label),
                typeof(Literal),
                typeof(Image),
                typeof(HyperLink),
                typeof(Panel), // Only if it doesn't contain interactive controls
                typeof(PlaceHolder)
            };

            var controlType = control.GetType();
            
            // Check if it's a control type we want to disable
            if (controlsToDisable.Contains(controlType))
            {
                return true;
            }

            // Special case for Panel - disable only if it doesn't contain interactive controls
            if (control is Panel)
            {
                return !ContainsInteractiveControls((Panel)control);
            }

            return false;
        }

        /// <summary>
        /// Checks if a container control contains interactive controls
        /// </summary>
        /// <param name="container">The container to check</param>
        /// <returns>True if it contains interactive controls</returns>
        private static bool ContainsInteractiveControls(Control container)
        {
            var interactiveControlTypes = new HashSet<Type>
            {
                typeof(Button),
                typeof(LinkButton),
                typeof(ImageButton),
                typeof(TextBox),
                typeof(DropDownList),
                typeof(ListBox),
                typeof(CheckBox),
                typeof(CheckBoxList),
                typeof(RadioButton),
                typeof(RadioButtonList),
                typeof(GridView),
                typeof(Repeater),
                typeof(DataList),
                typeof(ListView)
            };

            foreach (Control control in container.Controls)
            {
                if (interactiveControlTypes.Contains(control.GetType()))
                {
                    return true;
                }

                // Recursively check child controls
                if (ContainsInteractiveControls(control))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Configures ViewState settings for data-bound controls
        /// </summary>
        /// <param name="control">The data-bound control</param>
        /// <param name="enableViewState">Whether to enable ViewState</param>
        /// <param name="enablePaging">Whether paging is enabled</param>
        public static void ConfigureDataBoundControlViewState(Control control, bool enableViewState = true, bool enablePaging = false)
        {
            try
            {
                if (control == null) return;

                // Set EnableViewState if it's a WebControl
                if (control is WebControl)
                {
                    ((WebControl)control).EnableViewState = enableViewState;
                }

                // Special handling for different control types
                if (control is GridView)
                {
                    ConfigureGridViewViewState((GridView)control, enableViewState, enablePaging);
                }
                else if (control is DataList)
                {
                    ConfigureDataListViewState((DataList)control, enableViewState);
                }
                else if (control is ListView)
                {
                    ConfigureListViewViewState((ListView)control, enableViewState);
                }
                
                // Handle Repeater separately since it doesn't inherit from WebControl
                if (control is System.Web.UI.WebControls.Repeater)
                {
                    ConfigureRepeaterViewState((System.Web.UI.WebControls.Repeater)control, enableViewState);
                }

                Logger.Debug("ViewState configured for control: {ControlType}, EnableViewState: {EnableViewState}", 
                    control.GetType().Name, enableViewState);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error configuring ViewState for control: {ControlType}", control != null ? control.GetType().Name : "Unknown");
            }
        }

        /// <summary>
        /// Configures ViewState for GridView controls
        /// </summary>
        private static void ConfigureGridViewViewState(GridView gridView, bool enableViewState, bool enablePaging)
        {
            gridView.EnableViewState = enableViewState;
            
            // If paging is disabled and ViewState is disabled, we need to handle data binding differently
            if (!enableViewState && !enablePaging)
            {
                // Consider using data keys instead of ViewState for row identification
                gridView.DataKeyNames = new[] { "Id" }; // Assuming most entities have an Id field
            }
        }

        /// <summary>
        /// Configures ViewState for Repeater controls
        /// </summary>
        private static void ConfigureRepeaterViewState(Repeater repeater, bool enableViewState)
        {
            repeater.EnableViewState = enableViewState;
            
            // Repeaters typically don't need ViewState unless they have interactive controls
            // in their item templates that need to maintain state across postbacks
        }

        /// <summary>
        /// Configures ViewState for DataList controls
        /// </summary>
        private static void ConfigureDataListViewState(DataList dataList, bool enableViewState)
        {
            dataList.EnableViewState = enableViewState;
        }

        /// <summary>
        /// Configures ViewState for ListView controls
        /// </summary>
        private static void ConfigureListViewViewState(ListView listView, bool enableViewState)
        {
            listView.EnableViewState = enableViewState;
        }

        /// <summary>
        /// Gets the estimated ViewState size for a page
        /// </summary>
        /// <param name="page">The page to analyze</param>
        /// <returns>Estimated ViewState size in bytes</returns>
        public static int GetEstimatedViewStateSize(Page page)
        {
            try
            {
                if (page == null) return 0;

                // This is an approximation - ViewState is not directly accessible
                // Return 0 as we cannot access the protected ViewState property
                return 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error estimating ViewState size for page: {PageType}", page != null ? page.GetType().Name : "Unknown");
                return 0;
            }
        }

        /// <summary>
        /// Logs ViewState information for debugging
        /// </summary>
        /// <param name="page">The page to analyze</param>
        public static void LogViewStateInfo(Page page)
        {
            try
            {
                if (page == null) return;

                var viewStateSize = GetEstimatedViewStateSize(page);
                var enabledControls = CountViewStateEnabledControls(page);
                var totalControls = CountTotalControls(page);

                Logger.Debug("ViewState Info - Page: {PageType}, Size: {ViewStateSize} bytes, " +
                           "ViewState Enabled Controls: {EnabledControls}/{TotalControls}",
                    page.GetType().Name, viewStateSize, enabledControls, totalControls);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error logging ViewState info for page: {PageType}", page != null ? page.GetType().Name : "Unknown");
            }
        }

        /// <summary>
        /// Counts controls with ViewState enabled
        /// </summary>
        private static int CountViewStateEnabledControls(Control control)
        {
            int count = 0;
            
            if (control.EnableViewState)
            {
                count++;
            }

            foreach (Control childControl in control.Controls)
            {
                count += CountViewStateEnabledControls(childControl);
            }

            return count;
        }

        /// <summary>
        /// Counts total controls on a page
        /// </summary>
        private static int CountTotalControls(Control control)
        {
            int count = 1; // Count the current control

            foreach (Control childControl in control.Controls)
            {
                count += CountTotalControls(childControl);
            }

            return count;
        }
    }
}