using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Runtime.Serialization;

namespace Inflectra.SpiraTest.Web.Services.v4_0.DataObjects
{
    /// <summary>
    /// Represents a single sort that can be applied to a Retrieve query
    /// </summary>
    [DataContract]
    public class RemoteSort
    {
        /// <summary>
        /// The name of the field that we want to sort on
        /// </summary>
        [DataMember]
        public string PropertyName = "";

        /// <summary>
        /// Set true to sort ascending and false to sort descending
        /// </summary>
        [DataMember]
        public bool SortAscending = true;
    }
}
