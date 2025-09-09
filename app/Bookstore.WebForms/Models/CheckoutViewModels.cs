using System.ComponentModel.DataAnnotations;

namespace Bookstore.WebForms.Models
{
    public class CheckoutAddressViewModel
    {
        public int Id { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Country { get; set; }

        public string ZipCode { get; set; }

        public bool IsPrimary { get; set; }
    }

    public class CheckoutItemViewModel
    {
        public string BookName { get; set; }

        public decimal Price { get; set; }

        public string ImageUrl { get; set; }

        public bool OutOfStock { get; set; }
    }

    public class CheckoutFinishedItemViewModel
    {
        public string Bookname { get; set; }

        public long BookId { get; set; }

        public int Quantity { get; set; }

        public string Url { get; set; }

        public decimal Price { get; set; }

        public decimal Total { get; set; }
    }
}