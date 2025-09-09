using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.UI.WebControls;
using Bookstore.Domain;
using Bookstore.Domain.ReferenceData;

namespace Bookstore.WebForms.Models
{
    public class AdminReferenceDataIndexViewModel : PaginatedViewModel<AdminReferenceDataIndexListItemViewModel>
    {
        public ReferenceDataFilters Filters { get; set; } = new ReferenceDataFilters();
        public List<ListItem> ReferenceDataTypes { get; set; } = new List<ListItem>();
        
        // Pagination properties
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public List<int> PaginationButtons { get; set; } = new List<int>();

        public AdminReferenceDataIndexViewModel() 
        {
            // Create reference data type dropdown items
            ReferenceDataTypes = Enum.GetValues(typeof(ReferenceDataType))
                .Cast<ReferenceDataType>()
                .Select(type => new ListItem(type.ToString(), ((int)type).ToString()))
                .ToList();
        }

        public AdminReferenceDataIndexViewModel(IPaginatedList<ReferenceDataItem> referenceDataItems, ReferenceDataFilters filters) : this()
        {
            foreach (var item in referenceDataItems.OrderBy(x => x.DataType.ToString()))
            {
                Items.Add(new AdminReferenceDataIndexListItemViewModel
                {
                    Id = item.Id,
                    ReferenceDataType = item.DataType.ToString(),
                    Text = item.Text,
                    DataType = item.DataType
                });
            }

            Filters = filters;

            PageIndex = referenceDataItems.PageIndex;
            PageSize = referenceDataItems.Count;
            PageCount = referenceDataItems.TotalPages;
            HasNextPage = referenceDataItems.HasNextPage;
            HasPreviousPage = referenceDataItems.HasPreviousPage;
            PaginationButtons = referenceDataItems.GetPageList(5).ToList();
        }
    }

    public class AdminReferenceDataIndexListItemViewModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string ReferenceDataType { get; set; }
        public ReferenceDataType DataType { get; set; }
    }

    public class AdminReferenceDataCreateUpdateViewModel
    {
        public AdminReferenceDataCreateUpdateViewModel() 
        {
            // Create reference data type dropdown items
            DataTypes = Enum.GetValues(typeof(ReferenceDataType))
                .Cast<ReferenceDataType>()
                .Select(type => new ListItem(type.ToString(), ((int)type).ToString()))
                .ToList();
        }

        public AdminReferenceDataCreateUpdateViewModel(ReferenceDataItem referenceDataItem) : this()
        {
            Id = referenceDataItem.Id;
            SelectedReferenceDataType = referenceDataItem.DataType;
            Text = referenceDataItem.Text;
        }

        public int Id { get; set; }

        [Required]
        [DisplayName("Reference Data Type")]
        public ReferenceDataType SelectedReferenceDataType { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Text cannot exceed 100 characters")]
        public string Text { get; set; }

        public List<ListItem> DataTypes { get; set; } = new List<ListItem>();

        public bool IsEditMode => Id > 0;
        public string PageTitle => IsEditMode ? "Update Reference Data" : "Create Reference Data";
        public string SubmitButtonText => IsEditMode ? "Update" : "Create";
    }
}