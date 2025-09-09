using Bookstore.Domain;
using Bookstore.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bookstore.WebForms.Models
{
    public class AdminOrderIndexViewModel : PaginatedViewModel<AdminOrderIndexListItemViewModel>
    {
        public List<AdminOrderIndexListItemViewModel> Items { get; set; } = new List<AdminOrderIndexListItemViewModel>();

        public OrderFilters Filters { get; set; } = new OrderFilters();

        public AdminOrderIndexViewModel() { }

        public AdminOrderIndexViewModel(IPaginatedList<Order> orderDtos, OrderFilters filters)
        {
            foreach (var order in orderDtos)
            {
                Items.Add(new AdminOrderIndexListItemViewModel
                {
                    Id = order.Id,
                    CustomerName = order.Customer.FullName,
                    OrderStatus = order.OrderStatus,
                    OrderDate = order.CreatedOn,
                    DeliveryDate = order.DeliveryDate,
                    Total = order.Total
                });
            }

            Filters = filters ?? new OrderFilters();

            CurrentPage = orderDtos.PageIndex;
            ItemsPerPage = orderDtos.Count;
            TotalPages = orderDtos.TotalPages;
            TotalItems = orderDtos.TotalCount;
        }
    }

    public class AdminOrderIndexListItemViewModel
    {
        public int Id { get; set; }

        public string CustomerName { get; set; }

        public OrderStatus OrderStatus { get; set; }

        public DateTime DeliveryDate { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal Total { get; set; }
    }

    public class AdminOrderDetailsViewModel
    {
        public int OrderId { get; set; }

        public OrderStatus SelectedOrderStatus { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime DeliveryDate { get; set; }

        public string CustomerName { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string ZipCode { get; set; }

        public string Country { get; set; }

        public decimal Subtotal { get; set; }

        public decimal Tax { get; set; }

        public decimal Total => Subtotal + Tax;

        public List<AdminOrderDetailsItemViewModel> Items { get; set; } = new List<AdminOrderDetailsItemViewModel>();

        public AdminOrderDetailsViewModel() { }

        public AdminOrderDetailsViewModel(Order order)
        {
            OrderId = order.Id;
            CustomerName = order.Customer.FullName;
            SelectedOrderStatus = order.OrderStatus;
            AddressLine1 = order.Address.AddressLine1;
            AddressLine2 = order.Address.AddressLine2;
            City = order.Address.City;
            State = order.Address.State;
            ZipCode = order.Address.ZipCode;
            Country = order.Address.Country;
            Subtotal = order.SubTotal;
            Tax = order.Tax;
            OrderDate = order.CreatedOn;
            DeliveryDate = order.DeliveryDate;

            foreach (var orderItem in order.OrderItems)
            {
                Items.Add(new AdminOrderDetailsItemViewModel
                {
                    Author = orderItem.Book.Author,
                    BookType = orderItem.Book.BookType.Text,
                    Condition = orderItem.Book.Condition.Text,
                    Genre = orderItem.Book.Genre.Text,
                    Name = orderItem.Book.Name,
                    Price = orderItem.Book.Price,
                    Publisher = orderItem.Book.Publisher.Text
                });
            }
        }
    }

    public class AdminOrderDetailsItemViewModel
    {
        public string Name { get; set; }

        public string Author { get; set; }

        public string Publisher { get; set; }

        public string Genre { get; set; }

        public string BookType { get; set; }

        public string Condition { get; set; }

        public decimal Price { get; set; }
    }
}