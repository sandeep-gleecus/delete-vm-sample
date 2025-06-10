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
    /// Represents a single data mapping entry between an item in the system and the same item in another, external system
    /// </summary>
    public class RemoteDataMapping
    {
        /// <summary>
        /// The project that the data mapping entry relates to (null if it's not project specific)
        /// </summary>
        public Nullable<int> ProjectId = null;

        /// <summary>
        /// The id of the item inside Spira
        /// </summary>
        public int InternalId = -1;

        /// <summary>
        /// The id of the item in the external system
        /// </summary>
        public string ExternalKey = "";

        /// <summary>
        /// Whether this is the primary external mapped value in the case of 1..n mapping
        /// </summary>
        public bool Primary = true;
    }
}
