using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITInventory.Models
{
    public partial class ProductViewModel
    {

        public string ProductName { get; set; }
        public int ProductId { get; set; }
        public decimal Cost { get; set; }
        public int StockLevel { get; set; }
        public int OnOrder { get; set; }
        public bool Active { get; set; }
        public int DefaultOrderSize { get; set; }

        //public int ProductId { get; set; }
        //public bool? IsKit { get; set; }


        //public int Id { get; set; }
        //public string ProductName { get; set; }
        //public string SKU { get; set; }
        //public decimal? SellPrice { get; set; }
        //public int? Weight { get; set; }
        //public string WeightUnit { get; set; }
        //public int? PackageWeight { get; set; }
        //public int? PackageHeight { get; set; }
        //public int? PackageLength { get; set; }
        //public string PackageUnits { get; set; }
        //public int? QtyPerPackage { get; set; }
        //public int? DefaultOrderSize { get; set; }
        //public Nullable<int> ManufacturerId { get; set; }
        //public int? VendorId { get; set; }
        //public string ImagePath { get; set; }
        //public int? OnOrder { get; set; }
        //public string Catalog { get; set; }
        //public int? StockLevel { get; set; }
        //public int? ReorderThreshold { get; set; }
        //public Nullable<int> KitId { get; set; }
        //public bool? Active { get; set; }
        //public System.DateTime LastOrderedDate { get; set; }
        //public System.DateTime LastEditDateTime { get; set; }
        //public string LastEditUser { get; set; }
        //public Nullable<decimal> ListPrice { get; set; }
        //public Nullable<decimal> Cost { get; set; }
        //public bool? Ordered { get; set; }
        //public string VendorName { get; set; }
        //public string KitName { get; set; }
    }
}