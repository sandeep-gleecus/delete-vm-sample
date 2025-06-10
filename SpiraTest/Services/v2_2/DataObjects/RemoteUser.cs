using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Web.Services.v2_2.DataObjects
{
    /// <summary>
    /// Represents a single User in the system
    /// </summary>
    public class RemoteUser
    {
        public Nullable<int> UserId;
        public String FirstName;
        public String LastName;
        public String MiddleInitial;
        public String UserName;
        public String Password;
        public String LdapDn;
        public String EmailAddress;
        public bool Admin;
        public bool Active;
    }
}
