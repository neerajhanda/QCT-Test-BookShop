using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Bookstore.WebForms.Controls
{
    public partial class SearchFilterControl : UserControl
    {
        public event EventHandler<SearchRequestedEventArgs> SearchRequested;
        public event EventHandler FiltersCleared;

        public SearchCriteria SearchCriteria
        {
            get
            {
                return new SearchCriteria
                {
                    SearchString = SearchTextBox.Text.Trim(),
                    SortBy = SortByDropDown.SelectedValue,
                    Genre = GenreDropDown.SelectedValue,
                    Author = AuthorTextBox.Text.Trim(),
                    MinPrice = ParseDecimal(MinPriceTextBox.Text),
                    MaxPrice = ParseDecimal(MaxPriceTextBox.Text),
                    InStockOnly = InStockOnlyCheckBox.Checked
                };
            }
            set
            {
                if (value != null)
                {
                    SearchTextBox.Text = value.SearchString ?? string.Empty;
                    SortByDropDown.SelectedValue = value.SortBy ?? "Name";
                    GenreDropDown.SelectedValue = value.Genre ?? string.Empty;
                    AuthorTextBox.Text = value.Author ?? string.Empty;
                    MinPriceTextBox.Text = value.MinPrice?.ToString("F2") ?? string.Empty;
                    MaxPriceTextBox.Text = value.MaxPrice?.ToString("F2") ?? string.Empty;
                    InStockOnlyCheckBox.Checked = value.InStockOnly;
                }
            }
        }

        public bool ShowAdvancedFilters
        {
            get { return AdvancedFiltersPanel.Visible; }
            set 
            { 
                AdvancedFiltersPanel.Visible = value;
                ToggleAdvancedButton.Text = value ? "Hide Advanced Filters" : "Show Advanced Filters";
            }
        }

        public IEnumerable<GenreItem> AvailableGenres
        {
            set
            {
                GenreDropDown.Items.Clear();
                GenreDropDown.Items.Add(new ListItem("All Genres", ""));
                
                if (value != null)
                {
                    foreach (var genre in value)
                    {
                        GenreDropDown.Items.Add(new ListItem(genre.Name, genre.Id.ToString()));
                    }
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                InitializeControl();
            }
        }

        private void InitializeControl()
        {
            // Set default sort option
            if (SortByDropDown.Items.Count > 0)
            {
                SortByDropDown.SelectedIndex = 0;
            }
        }

        protected void SearchButton_Click(object sender, EventArgs e)
        {
            OnSearchRequested(new SearchRequestedEventArgs(SearchCriteria.SearchString, SearchCriteria.SortBy));
        }

        protected void ClearFiltersButton_Click(object sender, EventArgs e)
        {
            ClearAllFilters();
            OnFiltersCleared(EventArgs.Empty);
        }

        protected void ToggleAdvancedButton_Click(object sender, EventArgs e)
        {
            ShowAdvancedFilters = !ShowAdvancedFilters;
        }

        private void ClearAllFilters()
        {
            SearchTextBox.Text = string.Empty;
            SortByDropDown.SelectedIndex = 0;
            GenreDropDown.SelectedIndex = 0;
            AuthorTextBox.Text = string.Empty;
            MinPriceTextBox.Text = string.Empty;
            MaxPriceTextBox.Text = string.Empty;
            InStockOnlyCheckBox.Checked = false;
        }

        private decimal? ParseDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
                
            if (decimal.TryParse(value, out decimal result))
                return result;
                
            return null;
        }

        public virtual void OnSearchRequested(SearchRequestedEventArgs e)
        {
            SearchRequested?.Invoke(this, e);
        }

        public virtual void OnFiltersCleared(EventArgs e)
        {
            FiltersCleared?.Invoke(this, e);
        }

        public void SetFocus()
        {
            SearchTextBox.Focus();
        }

        public void SetSearchCriteria(string searchString, string sortBy)
        {
            SearchTextBox.Text = searchString ?? string.Empty;
            if (!string.IsNullOrEmpty(sortBy) && SortByDropDown.Items.FindByValue(sortBy) != null)
            {
                SortByDropDown.SelectedValue = sortBy;
            }
        }
    }

    public class SearchCriteria
    {
        public string SearchString { get; set; }
        public string SortBy { get; set; }
        public string Genre { get; set; }
        public string Author { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool InStockOnly { get; set; }

        public bool HasFilters()
        {
            return !string.IsNullOrWhiteSpace(SearchString) ||
                   !string.IsNullOrWhiteSpace(Genre) ||
                   !string.IsNullOrWhiteSpace(Author) ||
                   MinPrice.HasValue ||
                   MaxPrice.HasValue ||
                   InStockOnly;
        }
    }

    public class GenreItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class SearchEventArgs : EventArgs
    {
        public SearchCriteria Criteria { get; }

        public SearchEventArgs(SearchCriteria criteria)
        {
            Criteria = criteria;
        }
    }

    public class SearchRequestedEventArgs : EventArgs
    {
        public string SearchString { get; set; }
        public string SortBy { get; set; }

        public SearchRequestedEventArgs()
        {
        }

        public SearchRequestedEventArgs(string searchString, string sortBy)
        {
            SearchString = searchString;
            SortBy = sortBy;
        }
    }
}