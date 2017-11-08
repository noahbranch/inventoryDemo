using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITInventory.Models
{
    public class Allocate
    {
        public int[] RSCDB { get; set; }
        public int[] ProductId { get; set; }
        public int[] ProductCount { get; set; }
        public bool IsSplit { get; set; }
    }
}