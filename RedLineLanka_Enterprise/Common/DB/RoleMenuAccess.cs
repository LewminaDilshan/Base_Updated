//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RedLineLanka_Enterprise.Common.DB
{
    using System;
    using System.Collections.Generic;
    
    public partial class RoleMenuAccess
    {
        public int RoleID { get; set; }
        public int MenuID { get; set; }
    
        public virtual Menu Menu { get; set; }
        public virtual Role Role { get; set; }
    }
}
