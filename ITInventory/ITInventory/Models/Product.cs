//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ITInventory.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Product
    {
        public Product()
        {
            this.KitProducts = new HashSet<KitProduct>();
            this.Lots = new HashSet<Lot>();
            this.OrderProducts = new HashSet<OrderProduct>();
        }
    
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string SKU { get; set; }
        public int Weight { get; set; }
        public string WeightUnit { get; set; }
        public int PackageWeight { get; set; }
        public int PackageHeight { get; set; }
        public int PackageLength { get; set; }
        public string PackageUnits { get; set; }
        public int QtyPerPackage { get; set; }
        public int DefaultOrderSize { get; set; }
        public Nullable<int> ManufacturerId { get; set; }
        public int VendorId { get; set; }
        public string ImagePath { get; set; }
        public int OnOrder { get; set; }
        public string Catalog { get; set; }
        public Nullable<int> NewStockLevel { get; set; }
        public int StockLevel { get; set; }
        public Nullable<int> OriginalQty { get; set; }
        public int ReorderThreshold { get; set; }
        public Nullable<bool> IsKit { get; set; }
        public Nullable<int> KitId { get; set; }
        public bool Active { get; set; }
        public System.DateTime LastOrderedDate { get; set; }
        public System.DateTime LastEditDateTime { get; set; }
        public string LastEditUser { get; set; }
        public int CategoryId { get; set; }
    
        public virtual ICollection<KitProduct> KitProducts { get; set; }
        public virtual ICollection<Lot> Lots { get; set; }
        public virtual ICollection<OrderProduct> OrderProducts { get; set; }
        public virtual Category Category { get; set; }
        public virtual Vendor Vendor { get; set; }
    }
}