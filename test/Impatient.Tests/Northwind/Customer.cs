using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Impatient.Tests.Northwind
{
    [Table("Customers", Schema = "dbo")]
    public class Customer
    {
        [Required]
        public string CustomerID { get; set; }

        [Required]
        public string CompanyName { get; set; }

        public string ContactName { get; set; }

        public string ContactTitle { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string Region { get; set; }

        public string PostalCode { get; set; }

        public string Country { get; set; }

        public string Phone { get; set; }

        public string Fax { get; set; }

        public IEnumerable<Order> Orders { get; set; }
    }
}
