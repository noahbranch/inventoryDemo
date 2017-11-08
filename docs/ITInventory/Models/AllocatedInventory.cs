using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITInventory.Models
{
    public class AllocatedInventory
    {
        public int Id { get; set; }
        public int LotId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int[] RSCDB { get; set; }
        public string OfficeName { get; set; }
        public int ProductCount { get; set; }
        public decimal OrderCost { get; set; }
        public bool IsSplit { get; set; }
        public DateTime LastEditDateTime { get; set; }
        public Nullable<int> SplitId { get; set; }
    }
}