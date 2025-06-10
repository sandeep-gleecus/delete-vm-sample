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
    /// Represents a single incident resolution/comment entry in the system
    /// </summary>
    public class RemoteIncidentResolution
    {
        public Nullable<int> IncidentResolutionId;
        public int IncidentId;
        public int CreatorId;
        public String Resolution;
        public DateTime CreationDate;
        public String CreatorName;
    }
}
