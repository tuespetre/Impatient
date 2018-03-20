using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Impatient.Tests.Northwind
{
    [Table("Products")]
    public class Product
    {
        public int ProductID { get; set; }

        public string ProductName { get; set; }

        public int? SupplierID { get; set; }

        public int? CategoryID { get; set; }

        public ProductStats ProductStats { get; set; }

        public bool Discontinued { get; set; }
    }

    public class DiscontinuedProduct : Product
    {
    }

    public class ProductStats
    {
        public string QuantityPerUnit { get; set; }

        public decimal? UnitPrice { get; set; }

        public short? UnitsInStock { get; set; }

        public short? UnitsOnOrder { get; set; }

        public short? ReorderLevel { get; set; }
    }
}
