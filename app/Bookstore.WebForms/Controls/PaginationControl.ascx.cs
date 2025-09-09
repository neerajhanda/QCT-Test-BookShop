using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Bookstore.WebForms.Controls
{
    public partial class PaginationControl : UserControl
    {
        public event EventHandler<PageChangedEventArgs> PageChanged;

        private PaginationData _paginationData;

        public PaginationData PaginationData
        {
            get { return _paginationData; }
            set 
            { 
                _paginationData = value;
                BindPagination();
            }
        }

        public int MaxVisiblePages { get; set; } = 5;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindPagination();
            }
        }

        private void BindPagination()
        {
            if (_paginationData == null || _paginationData.TotalPages <= 1)
            {
                PaginationPanel.Visible = false;
                return;
            }

            PaginationPanel.Visible = true;

            // Configure Previous button
            PreviousButton.Enabled = _paginationData.HasPreviousPage;
            if (!_paginationData.HasPreviousPage)
            {
                PreviousButton.CssClass += " disabled";
            }

            // Configure Next button
            NextButton.Enabled = _paginationData.HasNextPage;
            if (!_paginationData.HasNextPage)
            {
                NextButton.CssClass += " disabled";
            }

            // Generate page buttons
            var pageButtons = GeneratePageButtons();
            PageButtonsRepeater.DataSource = pageButtons;
            PageButtonsRepeater.DataBind();

            // Update info labels
            CurrentPageLiteral.Text = _paginationData.CurrentPage.ToString();
            TotalPagesLiteral.Text = _paginationData.TotalPages.ToString();
            TotalItemsLiteral.Text = _paginationData.TotalItems.ToString();
        }

        private List<PageButtonData> GeneratePageButtons()
        {
            var buttons = new List<PageButtonData>();
            
            int startPage = Math.Max(1, _paginationData.CurrentPage - MaxVisiblePages / 2);
            int endPage = Math.Min(_paginationData.TotalPages, startPage + MaxVisiblePages - 1);
            
            // Adjust start page if we're near the end
            if (endPage - startPage + 1 < MaxVisiblePages)
            {
                startPage = Math.Max(1, endPage - MaxVisiblePages + 1);
            }

            for (int i = startPage; i <= endPage; i++)
            {
                buttons.Add(new PageButtonData
                {
                    PageNumber = i,
                    IsCurrentPage = i == _paginationData.CurrentPage
                });
            }

            return buttons;
        }

        protected void PreviousButton_Click(object sender, EventArgs e)
        {
            if (_paginationData.HasPreviousPage)
            {
                OnPageChanged(new PageChangedEventArgs(_paginationData.CurrentPage - 1));
            }
        }

        protected void NextButton_Click(object sender, EventArgs e)
        {
            if (_paginationData.HasNextPage)
            {
                OnPageChanged(new PageChangedEventArgs(_paginationData.CurrentPage + 1));
            }
        }

        protected void PageButtonsRepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "GoToPage")
            {
                int pageNumber = Convert.ToInt32(e.CommandArgument);
                OnPageChanged(new PageChangedEventArgs(pageNumber));
            }
        }

        public virtual void OnPageChanged(PageChangedEventArgs e)
        {
            PageChanged?.Invoke(this, e);
        }

        public void SetPaginationData(int currentPage, int totalPages, bool hasPreviousPage, bool hasNextPage)
        {
            PaginationData = new PaginationData
            {
                CurrentPage = currentPage,
                TotalPages = totalPages,
                HasPreviousPage = hasPreviousPage,
                HasNextPage = hasNextPage,
                TotalItems = 0, // Will be calculated if needed
                PageSize = 10 // Default page size
            };
        }
    }

    public class PaginationData
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    public class PageButtonData
    {
        public int PageNumber { get; set; }
        public bool IsCurrentPage { get; set; }
    }

    public class PageChangedEventArgs : EventArgs
    {
        public int NewPageNumber { get; }
        public int NewPageIndex { get; }

        public PageChangedEventArgs(int newPageNumber)
        {
            NewPageNumber = newPageNumber;
            NewPageIndex = newPageNumber; // For compatibility
        }
    }
}