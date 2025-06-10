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
    /// Represents a custom list value (property value) in the system
    /// </summary>
    public class RemoteCustomListValue
    {
        public Nullable<int> CustomPropertyValueId;
        public int CustomPropertyListId;
        public string Name;
        public bool Active;
    }
}
