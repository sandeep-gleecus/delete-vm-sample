using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// Represents a single User in the system
    /// </summary>
    public class RemoteUser
    {
        /// <summary>
        /// The id of the user
        /// </summary>
        public Nullable<int> UserId;

        /// <summary>
        /// The first (given) name of the user
        /// </summary>
        public String FirstName;

        /// <summary>
        /// The last name (surname) of the user
        /// </summary>
        public String LastName;

        /// <summary>
        /// The middle initials of the user
        /// </summary>
        public String MiddleInitial;

        /// <summary>
        /// The login used by the user
        /// </summary>
        public String UserName;

        /// <summary>
        /// The LDAP Distinguished Name for the user (null for non-LDAP users)
        /// </summary>
        public String LdapDn;

        /// <summary>
        /// The email address of the user
        /// </summary>
        public String EmailAddress;

        /// <summary>
        /// Whether the user is a system administrator
        /// </summary>
        public bool Admin;

        /// <summary>
        /// Whether the user is active in the system
        /// </summary>
        public bool Active;

        /// <summary>
        /// The department of the user
        /// </summary>
        public string Department;

        /// <summary>
        /// Is this user approved by the system administrator
        /// </summary>
        public bool Approved;

        /// <summary>
        /// Is this user locked-out of their account
        /// </summary>
        public bool Locked;
        
        /// <summary>
        /// This is the RSS token for this user
        /// </summary>
        /// <remarks>
        /// For security reasons, you have to access the API as a system administrator to retrieve this field
        /// </remarks>
        public string RssToken;

        /// <summary>
        /// The full name of the user concatenated (First + Middle + Last)
        /// </summary>
        public string FullName;
    }
}
