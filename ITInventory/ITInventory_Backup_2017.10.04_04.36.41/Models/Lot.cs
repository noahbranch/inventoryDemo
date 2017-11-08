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
    
    public partial class Lot
    {
        public Lot()
        {
            this.AllocationLots = new HashSet<AllocationLot>();
        }
    
        public int Id { get; set; }
        public int ProductId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string LastEditUser { get; set; }
        public System.DateTime LastEditDateTime { get; set; }
    
        public virtual Product Product { get; set; }
        public virtual ICollection<AllocationLot> AllocationLots { get; set; }
    }
}
