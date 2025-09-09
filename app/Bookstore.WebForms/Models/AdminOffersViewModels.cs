using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Bookstore.Domain;
using Bookstore.Domain.Offers;
using Bookstore.Domain.ReferenceData;

namespace Bookstore.WebForms.Models
{
    public class AdminOffersIndexViewModel : PaginatedViewModel<AdminOffersIndexListItemViewModel>
    {
        public OfferFilters Filters { get; set; } = new OfferFilters();
        public List<ListItem> Genres { get; set; } = new List<ListItem>();
        public List<ListItem> BookConditions { get; set; } = new List<ListItem>();
        public List<ListItem> OfferStatuses { get; set; } = new List<ListItem>();

        public AdminOffersIndexViewModel() { }

        public AdminOffersIndexViewModel(IPaginatedList<Offer> offers, IEnumerable<ReferenceDataItem> referenceDataItems)
        {
            var items = new List<AdminOffersIndexListItemViewModel>();
            foreach (var offer in offers)
            {
                items.Add(new AdminOffersIndexListItemViewModel
                {
                    OfferId = offer.Id,
                    BookName = offer.BookName,
                    Author = offer.Author,
                    Genre = offer.Genre?.Text ?? "N/A",
                    CustomerName = offer.Customer?.FullName ?? "N/A",
                    OfferStatus = offer.OfferStatus,
                    OfferDate = offer.CreatedOn,
                    OfferPrice = offer.BookPrice,
                    Condition = offer.Condition?.Text ?? "N/A",
                    ISBN = offer.ISBN
                });
            }

            // Set the items using the base class constructor
            Items = items;
            CurrentPage = offers.PageIndex;
            ItemsPerPage = offers.Count;
            TotalPages = offers.TotalPages;
            TotalItems = offers.TotalCount;

            // Convert reference data to ListItems for WebForms dropdowns
            Genres = referenceDataItems
                .Where(x => x.DataType == ReferenceDataType.Genre)
                .Select(x => new ListItem(x.Text, x.Id.ToString()))
                .ToList();
            
            BookConditions = referenceDataItems
                .Where(x => x.DataType == ReferenceDataType.Condition)
                .Select(x => new ListItem(x.Text, x.Id.ToString()))
                .ToList();

            // Create offer status dropdown items
            OfferStatuses = Enum.GetValues(typeof(OfferStatus))
                .Cast<OfferStatus>()
                .Select(status => new ListItem(status.ToString(), ((int)status).ToString()))
                .ToList();
        }
    }

    public class AdminOffersIndexListItemViewModel
    {
        public int OfferId { get; set; }
        public string BookName { get; set; }
        public string CustomerName { get; set; }
        public string Author { get; set; }
        public string Genre { get; set; }
        public OfferStatus OfferStatus { get; set; }
        public DateTime OfferDate { get; set; }
        public decimal OfferPrice { get; set; }
        public string Condition { get; set; }
        public string ISBN { get; set; }
        
        public string OfferStatusText => OfferStatus.ToString();
        public string FormattedOfferPrice => OfferPrice.ToString("C");
        public string FormattedOfferDate => OfferDate.ToString("d");
    }
}