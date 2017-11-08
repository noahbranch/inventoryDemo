using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITInventory.Models
{
    public class OrderHistoryConfirm
    {
        public int QtyConfirmed { get; set; }
        public bool CheckedIn { get; set; }
        public int ProductId { get; set; }
        public int CheckId { get; set; }
        public int LotId { get; set; }
    }
}