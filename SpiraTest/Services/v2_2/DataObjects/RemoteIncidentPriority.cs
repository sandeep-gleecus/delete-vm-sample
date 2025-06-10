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
    /// Represents an incident priority in the project
    /// </summary>
    public class RemoteIncidentPriority
    {
        public Nullable<int> PriorityId;
        public string Name;
        public bool Active;
        public string Color;
    }
}
