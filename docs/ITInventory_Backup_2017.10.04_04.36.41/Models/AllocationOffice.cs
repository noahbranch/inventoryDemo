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
    
    public partial class AllocationOffice
    {
        public int Id { get; set; }
        public int AllocationId { get; set; }
        public int OfficeId { get; set; }
        public string LastEditUser { get; set; }
        public System.DateTime LastEditDateTime { get; set; }
    
        public virtual Allocation Allocation { get; set; }
    }
}