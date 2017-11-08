using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITInventory.Controllers
{
    public class ProductSelection
    {
        public string KitName { get; set; }
        public string[] productList { get; set; }
        public int[] orderQty { get; set; }
        public int[] productCost { get; set; }
        public int[] OfficeIds { get; set; }
        public string[] vendorEmails { get; set; }
        public string[] vendorMessages { get; set; }
    }
}