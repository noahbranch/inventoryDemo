using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITInventory.Models
{
    public class VendorEmail
    {
        public string Subject { get; set; }
        public string ToAddress { get; set; }
        public string FromAddress { get; set; }
        public string messageBody { get; set; }
    }
}