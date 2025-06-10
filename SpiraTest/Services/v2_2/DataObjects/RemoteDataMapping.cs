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
    /// Represents a single data mapping entry between an item in the system and the same item in another, external system
    /// </summary>
    public class RemoteDataMapping
    {
        public Nullable<int> ProjectId = null;
        public int InternalId = -1;
        public string ExternalKey = "";
        public bool Primary = true;
    }
}
