using System;
using System.Linq;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Collections.Generic;

using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Services.v3_0.DataObjects
{
    /// <summary>
    /// Represents a custom list in the system
    /// </summary>
    public class RemoteCustomList
    {
       /// <summary>
        /// Default constructor
        /// </summary>
        public RemoteCustomList()
        {
        }

        /// <summary>
        /// Creates a new remote custom list object from the corresponding data-row
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="customList">The custom list</param>
        /// <remarks>Does not populate the values</remarks>
        internal RemoteCustomList(int projectid, CustomPropertyList customList)
        {
            //Populate the fields
            this.CustomPropertyListId = customList.CustomPropertyListId;
            this.ProjectId = projectid;
            this.Name = customList.Name;
            this.Active = customList.IsActive;
        }

        /// <summary>
        /// The id of the custom list
        /// </summary>
        public Nullable<int> CustomPropertyListId;

        /// <summary>
        /// The id of the project the custom list belongs to
        /// </summary>
        public int ProjectId;

        /// <summary>
        /// The name of the custom list
        /// </summary>
        public string Name;

        /// <summary>
        /// Whether the list is active or not
        /// </summary>
        public bool Active;

        /// <summary>
        /// The collection of values in the custom list
        /// </summary>
        public List<RemoteCustomListValue> Values;
    }
}
