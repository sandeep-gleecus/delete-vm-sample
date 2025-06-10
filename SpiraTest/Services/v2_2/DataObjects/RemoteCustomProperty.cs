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
    /// Represents a single project custom property configuration entry
    /// </summary>
    public class RemoteCustomProperty
    {
        public int CustomPropertyId;
        public int ProjectId;
        public int ArtifactTypeId;
        public string Alias;
        public Nullable<int> CustomPropertyListId;
        public string CustomPropertyName;
        public Nullable<int> CustomPropertyTypeId;
        public string CustomPropertyTypeName;
    }
}
