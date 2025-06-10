using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Web.ServerControls.Authorization;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// Adds permission checking to a placeholder control
    /// </summary>
    [ToolboxData("<{0}:PlaceHolderEx runat=server></{0}:PlaceHolderEx>")]
    public class PlaceHolderEx : PlaceHolder, IAuthorizedControl
    {
        protected AuthorizedControlBase authorizedControlBase;

		/// <summary>
		/// Constructor - delegates to base class
		/// </summary>
        public PlaceHolderEx()
            : base()
		{
			//Instantiate the authorized control default implementation
			authorizedControlBase = new AuthorizedControlBase(this.ViewState);
		}

        #region Properties

        /// <summary>
        /// This is the type of artifact that the user's role needs to have permissions for
        /// </summary>
        [
        Bindable(true),
        Category("Security"),
        Description("This is the type of artifact that the user's role needs to have permissions for"),
        DefaultValue(DataModel.Artifact.ArtifactTypeEnum.None),
        PersistenceMode(PersistenceMode.Attribute),
        ]
        public DataModel.Artifact.ArtifactTypeEnum Authorized_ArtifactType
        {
            get
            {
                return authorizedControlBase.Authorized_ArtifactType;
            }
            set
            {
                authorizedControlBase.Authorized_ArtifactType = value;
            }
        }

        /// <summary>
        /// This is the type of action that the user's role needs to have permissions for
        /// </summary>
        [
        Bindable(true),
        Category("Security"),
        Description("This is the type of action that the user's role needs to have permissions for"),
        DefaultValue(Project.PermissionEnum.None),
        PersistenceMode(PersistenceMode.Attribute),
        ]
        public Project.PermissionEnum Authorized_Permission
        {
            get
            {
                return authorizedControlBase.Authorized_Permission;
            }
            set
            {
                authorizedControlBase.Authorized_Permission = value;
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Does the permission checking
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            //Check to see if we are authorized, if not then hide the content
            if (Context != null && this.Visible)
            {
                int currentRoleId = authorizedControlBase.ProjectRoleId;
                bool isSystemAdmin = authorizedControlBase.UserIsAdmin;
                bool isGroupAdmin = authorizedControlBase.UserIsGroupAdmin;
                this.Visible = (authorizedControlBase.IsAuthorized(currentRoleId, isSystemAdmin, isGroupAdmin) == Project.AuthorizationState.Authorized);
            }

            base.OnPreRender(e);
        }

        #endregion
    }
}