using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Inflectra.SpiraTest.DataModel;
using System.Collections.Generic;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
    /// <summary>
    /// Represents a single project custom property configuration entry
    /// </summary>
    public class RemoteCustomProperty
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public RemoteCustomProperty()
        {
        }

        /// <summary>
        /// Creates a new remote custom property object from the corresponding entity
        /// </summary>
        /// <param name="customProperty">The custom property entity</param>
        /// <remarks>Does not list information</remarks>
        internal RemoteCustomProperty(CustomProperty customProperty)
        {
            //Populate the fields
            this.CustomPropertyId = customProperty.CustomPropertyId;
            this.CustomPropertyTypeId = customProperty.CustomPropertyTypeId;
            this.CustomPropertyTypeName = customProperty.CustomPropertyTypeName;
            this.ProjectTemplateId = customProperty.ProjectTemplateId;
            this.ArtifactTypeId = customProperty.ArtifactTypeId;
            this.PropertyNumber = customProperty.PropertyNumber;
            this.CustomPropertyFieldName = customProperty.CustomPropertyFieldName;
            this.Name = customProperty.Name;
            this.IsDeleted = customProperty.IsDeleted;
            this.SystemDataType = customProperty.Type.SystemType;

            //Now populate any options
            if (customProperty.Options != null && customProperty.Options.Count > 0)
            {
                this.Options = new List<RemoteCustomPropertyOption>();
                foreach (CustomPropertyOptionValue optionValue in customProperty.Options)
                {
                    this.Options.Add(new RemoteCustomPropertyOption(optionValue));
                }
            }
            else
            {
                this.Options = null;
            }
        }

        /// <summary>
        /// The id of the custom property (leave null for newly created ones)
        /// </summary>
        public int? CustomPropertyId;

        /// <summary>
        /// The project template the custom property belongs to
        /// </summary>
        public int ProjectTemplateId;

        /// <summary>
        /// The artifact type that the custom property is for
        /// </summary>
        public int ArtifactTypeId;

        /// <summary>
        /// The display name for the custom property
        /// </summary>
        public string Name;

        /// <summary>
        /// The associated custom list if this is a list custom property
        /// </summary>
        /// <remarks>
        /// This will be null if this is not a list custom property
        /// </remarks>
        public RemoteCustomList CustomList;

        /// <summary>
        /// The internal field name of the custom property (e.g. Custom_01)
        /// </summary>
        public string CustomPropertyFieldName;

        /// <summary>
        /// The type of custom property. It can have the following values:
        ///     Text = 1,
        ///     Integer = 2,
        ///     Decimal = 3,
        ///     Boolean = 4,
        ///     Date = 5,
        ///     List = 6,
        ///     MultiList = 7,
        ///     User = 8
        /// </summary>
        public int CustomPropertyTypeId;

        /// <summary>
        /// The display name of the type of custom property
        /// </summary>
        public string CustomPropertyTypeName;

        /// <summary>
        /// Has this custom property been deleted
        /// </summary>
        public bool IsDeleted;

        /// <summary>
        /// The position number of this custom property (1-30). Each artifact type can have 30 custom properties per project
        /// </summary>
        public int PropertyNumber;

        /// <summary>
        /// The physical data type that this custom property is stored as (Int32, String, DataTime, etc.)
        /// </summary>
        public string SystemDataType;

        /// <summary>
        /// The collection of custom property options
        /// </summary>
        public List<RemoteCustomPropertyOption> Options;
    }
}
