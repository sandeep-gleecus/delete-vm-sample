using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Web.Services.v3_0.DataObjects
{
    /// <summary>
    /// Represents a single project custom property configuration entry
    /// </summary>
    public class RemoteCustomProperty
    {
        /// <summary>
        /// The id of the custom property
        /// </summary>
        public int CustomPropertyId;

        /// <summary>
        /// The project the custom property belongs to
        /// </summary>
        public int ProjectId;

        /// <summary>
        /// The artifact type that the custom property is for
        /// </summary>
        public int ArtifactTypeId;

        /// <summary>
        /// The display name for the custom property
        /// </summary>
        public string Alias;

        /// <summary>
        /// The associated custom list if this is a list custom property
        /// </summary>
        /// <remarks>
        /// Leave as null if this is for a text custom property
        /// </remarks>
        public RemoteCustomList CustomList;

        /// <summary>
        /// The internal system name of the custom property (e.g. TEXT_01)
        /// </summary>
        public string CustomPropertyName;

        /// <summary>
        /// The type of custom property (Text = 1, List = 2)
        /// </summary>
        public Nullable<int> CustomPropertyTypeId;

        /// <summary>
        /// The display name of the type of custom property
        /// </summary>
        public string CustomPropertyTypeName;
    }
}
