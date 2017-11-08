using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITInventory.Models
{
    public class AddtoCart
    {
        public int ProductId { get; set; }
        public int OrderQty { get; set; }
    }
}