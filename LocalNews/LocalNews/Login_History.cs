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
    
    public partial class Login_History
    {
        public long login_history_id { get; set; }
        public long user_id { get; set; }
        public long status_id { get; set; }
        public System.DateTime date_time { get; set; }
    
        public virtual User_Status User_Status { get; set; }
        public virtual User User { get; set; }
    }
}