﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Impatient.Tests.Northwind
{
    [Table("Orders", Schema = "dbo")]
    public class Order
    {
        public int OrderID { get; set; }

        public string CustomerID { get; set; }

        public TestEnum? EmployeeID { get; set; }

        public DateTime? OrderDate { get; set; }

        public DateTime? RequiredDate { get; set; }

        public DateTime? ShippedDate { get; set; }

        public int? ShipVia { get; set; }

        public decimal? Freight { get; set; }

        public string ShipName { get; set; }

        public string ShipAddress { get; set; }

        public string ShipCity { get; set; }

        public string ShipRegion { get; set; }

        public string ShipPostalCode { get; set; }

        public string ShipCountry { get; set; }

        public Customer Customer { get; set; }

        public IEnumerable<OrderDetail> OrderDetails { get; set; }
    }

    public enum TestEnum
    {
        Blah = 1,
    }
}
