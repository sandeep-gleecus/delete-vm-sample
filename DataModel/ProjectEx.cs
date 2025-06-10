using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds custom extensions to the Project entity
    /// </summary>
    public partial class Project : Artifact
    {
        public const string ARTIFACT_PREFIX = "PR";

		#region Enumerations

		//The different artifact permissions
		//<remarks>Make sure changes here are reflected in ClientScripts\GlobalFunctions.js permissionEnum object
		public enum PermissionEnum
        {
            None = -1,
            ProjectAdmin = -2,
            SystemAdmin = -3,
            ProjectGroupAdmin = -4,
			/// <summary>Commented out. When enabled, the code in 
			/// AuthorizedControlBase.IsAuthorized will need to be 
			/// addressed to check for permissions.</summary>
            //ProjectTemplateAdmin = -5,
			ReportAdmin = -6,
			//ResourceAdmin = -7,
			//PortfolioAdmin = -8,
            Create = 1,
            Modify = 2,
            Delete = 3,
            View = 4,
            LimitedModify = 5,
            BulkEdit = 6
        }

        /// <summary>The different possible authorization states</summary>
        public enum AuthorizationState
        {
            Prohibited = 1, // Not authorized
            Authorized = 2, // Authorized for all instances of an artifact type
            Limited = 3     // Authorized for just the items created/assigned to the user
        }

        //The different artifact notification types
        public enum ProjectArtifactNotifyTypeEnum
        {
            Creator = 1,
            Owner = 2
        }

        #endregion

        /// <summary>
        /// Returns the artifact prefix
        /// </summary>
        public override string ArtifactPrefix
        {
            get
            {
                return ARTIFACT_PREFIX;
            }
        }

        /// <summary>
        /// Returns the artifact type enumeration
        /// </summary>
        public override Artifact.ArtifactTypeEnum ArtifactType
        {
            get
            {
                return ArtifactTypeEnum.Project;
            }
        }

		/// <summary>The Artifact's token.</summary>
		public override string ArtifactToken
		{
			get
			{
				return this.ArtifactPrefix + ":" + this.ProjectId;
			}
		}

        /// <summary>The Artifact's ID</summary>
        public override int ArtifactId
        {
            get
            {
                return this.ProjectId;
            }
        }
    }
}
