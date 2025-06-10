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

namespace Inflectra.SpiraTest.Web.Services.v4_0.DataObjects
{
    /// <summary>
    /// Represents a custom list value (property value) in the system
    /// </summary>
    public class RemoteCustomListValue
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public RemoteCustomListValue()
        {
        }

        /// <summary>
        /// Creates a new remote custom list value object from the corresponding data-row
        /// </summary>
        /// <param name="customPropertyValue">The custom property value</param>
        internal RemoteCustomListValue(CustomPropertyValue customPropertyValue)
        {
            //Populate the fields
            this.CustomPropertyValueId = customPropertyValue.CustomPropertyValueId;
            this.CustomPropertyListId = customPropertyValue.CustomPropertyListId;
            this.Name = customPropertyValue.Name;
       }


        /// <summary>
        /// The id of the custom list value
        /// </summary>
        public Nullable<int> CustomPropertyValueId;

        /// <summary>
        /// The id of the custom list the value belongs to
        /// </summary>
        public int CustomPropertyListId;

        /// <summary>
        /// The name of the custom list value
        /// </summary>
        public string Name;
    }
}
