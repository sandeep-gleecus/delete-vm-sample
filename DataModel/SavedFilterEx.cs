using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds some lookup properties to the Saved Filter entity
    /// </summary>
    public partial class SavedFilter : Entity
    {
        #region Lookup Properties

        /// <summary>
        /// Returns the creator's full name
        /// </summary>
        public string UserName
        {
            get
            {
                if (User != null && User.Profile != null)
                {
                    return User.Profile.FullName;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Returns the Project name
        /// </summary>
        public string ProjectName
        {
            get
            {
                if (Project != null)
                {
                    return Project.Name;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Returns the artifact type name
        /// </summary>
        public string ArtifactTypeName
        {
            get
            {
                if (ArtifactType != null)
                {
                    return ArtifactType.Name;
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion
    }
}
