using System;
using System.Collections.Generic;
using System.Linq;

namespace Bookstore.WebForms.Models
{
    /// <summary>
    /// Base class for paginated view models
    /// </summary>
    /// <typeparam name="T">The type of items in the paginated list</typeparam>
    public class PaginatedViewModel<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalItems { get; set; } = 0;
        public int ItemsPerPage { get; set; } = 10;
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int StartItem => (CurrentPage - 1) * ItemsPerPage + 1;
        public int EndItem => Math.Min(CurrentPage * ItemsPerPage, TotalItems);

        public PaginatedViewModel()
        {
        }

        public PaginatedViewModel(IEnumerable<T> items, int currentPage, int totalItems, int itemsPerPage)
        {
            Items = items?.ToList() ?? new List<T>();
            CurrentPage = currentPage;
            TotalItems = totalItems;
            ItemsPerPage = itemsPerPage;
            TotalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
        }
    }
}