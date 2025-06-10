using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.UserControls
{
	/// <summary>
	/// Base class for all user controls that display based on an artifact id
	/// and artifact type (used for common elements in artifact details pages)
	/// </summary>
	public class ArtifactUserControlBase : UserControlBase
	{
		//Viewstate keys
		protected const string ViewStateKey_ArtifactId = "ArtifactId";
		protected const string ViewStateKey_ArtifactTypeEnum = "ArtifactTypeEnum";
        protected const string ViewStateKey_DisplayTypeEnum = "DisplayTypeEnum";
		protected const string ViewStateKey_HasData = "HasData";
        protected const string ViewStateKey_MessageLabelHandle = "MessageLabelHandle";

        #region Properties

        /// <summary>
        /// Should we auto-load any contained ajax control
        /// </summary>
        [DefaultValue(false)]
        public bool AutoLoad
        {
            get
            {
                if (ViewState["AutoLoad"] == null)
                {
                    return false;
                }
                else
                {
                    return (bool)ViewState["AutoLoad"];
                }
            }
            set
            {
                ViewState["AutoLoad"] = value;
            }
        }

        /// <summary>
		/// Does the artifact in question have any data. Used in providing visual cues on tab-controls
		/// </summary>
		public bool HasData
		{
			get
			{
				if (ViewState[ViewStateKey_HasData] == null)
				{
					return false;
				}
				else
				{
					return (bool) ViewState[ViewStateKey_HasData];
				}
			}
			set
			{
				ViewState[ViewStateKey_HasData] = value;
			}
		}

		/// <summary>
		/// The ID of artifact we're attaching the documents to
		/// </summary>
		public int ArtifactId
		{
			get
			{
                if (ViewState[ViewStateKey_ArtifactId] == null)
                {
                    return -1;
                }
                else
                {
                    return (int)ViewState[ViewStateKey_ArtifactId];
                }
			}
			set
			{
				ViewState[ViewStateKey_ArtifactId] = value;
			}
		}

		/// <summary>
		/// The type of artifact we're attaching the documents to
		/// </summary>
        public DataModel.Artifact.ArtifactTypeEnum ArtifactTypeEnum
		{
			get
			{
                if (ViewState[ViewStateKey_ArtifactTypeEnum] == null)
                {
                    return DataModel.Artifact.ArtifactTypeEnum.None;
                }
                else
                {
                    return (DataModel.Artifact.ArtifactTypeEnum)ViewState[ViewStateKey_ArtifactTypeEnum];
                }
			}
			set
			{
				ViewState[ViewStateKey_ArtifactTypeEnum] = value;
			}
		}

        /// <summary>
        /// The type of display for the artifact links
        /// </summary>
        public DataModel.Artifact.DisplayTypeEnum DisplayTypeEnum
        {
            get
            {
                if (ViewState[ViewStateKey_DisplayTypeEnum] == null)
                {
                    return DataModel.Artifact.DisplayTypeEnum.None;
                }
                else
                {
                    return (DataModel.Artifact.DisplayTypeEnum)ViewState[ViewStateKey_DisplayTypeEnum];
                }
            }
            set
            {
                ViewState[ViewStateKey_DisplayTypeEnum] = value;
            }
        }

		#endregion
	}
}
