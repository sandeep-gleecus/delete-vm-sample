using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel.ReportableEntities
{
    /// <summary>
    /// Used to describe the display name and entity set for a reportable entity
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ReportableEntityAttribute: Attribute
    {
        #region Properties

        /// <summary>
        /// The display name of the reportable entity
        /// </summary>
        public string DisplayName
        {
            get;
            set;
        }

        /// <summary>
        /// The 
        /// </summary>
        public string EntitySet
        {
            get;
            set;
        }

        #endregion

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="administrationLevels">The ORed list of administration levels that can access this page</param>
        public ReportableEntityAttribute(string displayName, string entitySet)
        {
            this.DisplayName = displayName;
            this.EntitySet = entitySet;
        }
    }
}
