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
    /// Represents a single sort that can be applied to a Retrieve query
    /// </summary>
    public class RemoteSort
    {
        public string PropertyName = "";
        public bool SortAscending = true;
    }
}
