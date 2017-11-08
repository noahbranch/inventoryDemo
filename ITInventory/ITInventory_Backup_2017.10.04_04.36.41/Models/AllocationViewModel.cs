using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITInventory.Models
{
    public class AllocationViewModel
    {
        public int Id { get; set; }
        public int LotId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal SellPrice { get; set; }
        public int StockLevel { get; set; }
        public int OnOrder { get; set; }

        public virtual ICollection<AllocationLot> Lots { get; set; }
    }
}