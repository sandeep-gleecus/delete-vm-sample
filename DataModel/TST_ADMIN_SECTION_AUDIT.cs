//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Inflectra.SpiraTest.DataModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class TST_ADMIN_SECTION_AUDIT
    {
        public TST_ADMIN_SECTION_AUDIT()
        {
            this.TST_ADMIN_HISTORY_CHANGESET_AUDIT = new HashSet<TST_ADMIN_HISTORY_CHANGESET_AUDIT>();
            this.TST_USER_HISTORY_CHANGESET_AUDIT = new HashSet<TST_USER_HISTORY_CHANGESET_AUDIT>();
        }
    
        public int ADMIN_SECTION_ID { get; set; }
        public string NAME { get; set; }
        public string DESCRIPTION { get; set; }
        public Nullable<int> PARENT_ID { get; set; }
        public bool IS_ACTIVE { get; set; }
    
        public virtual ICollection<TST_ADMIN_HISTORY_CHANGESET_AUDIT> TST_ADMIN_HISTORY_CHANGESET_AUDIT { get; set; }
        public virtual ICollection<TST_USER_HISTORY_CHANGESET_AUDIT> TST_USER_HISTORY_CHANGESET_AUDIT { get; set; }
    }
}
