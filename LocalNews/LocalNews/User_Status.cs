//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LocalNews
{
    using System;
    using System.Collections.Generic;
    
    public partial class User_Status
    {
        public User_Status()
        {
            this.Login_History = new HashSet<Login_History>();
        }
    
        public long status_id { get; set; }
        public string status_name { get; set; }
    
        public virtual ICollection<Login_History> Login_History { get; set; }
    }
}
