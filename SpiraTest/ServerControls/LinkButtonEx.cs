using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using System.ComponentModel;

using Inflectra.SpiraTest.Web.ServerControls.Authorization;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.ServerControls
{
	/// <summary>
	/// This class extends the default link-button to include two additional
	/// properties that can be used to display a confirmation message
	/// </summary>
	[ToolboxData("<{0}:LinkButtonEx runat=server></{0}:LinkButtonEx>")]
	public class LinkButtonEx : System.Web.UI.WebControls.LinkButton, IAuthorizedControl
	{
		protected string confirmationMessage;
		protected AuthorizedControlBase authorizedControlBase;

		protected const string ViewStateKey_Confirmation = "Confirmation";

		/// <summary>
		/// Constructor - delegates to base class
		/// </summary>
		public LinkButtonEx() : base()
		{
			//Set the default confirmation
			this.Confirmation = false;

			//Instantiate the authorized control default implementation
			authorizedControlBase = new AuthorizedControlBase(this.ViewState);
		}

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

		/// <summary>
		/// Whether we want to display a confirmation message or not before executing any server-side events
		/// </summary>
		[
		Bindable(true),
		Category("Behavior"),
		Description("Whether we want to display a confirmation message or not before executing any server-side events"),
		DefaultValue(false),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public bool Confirmation
		{
			get
			{
				return (bool) ViewState[ViewStateKey_Confirmation];
			}
			set
			{
				ViewState[ViewStateKey_Confirmation] = value;
			}
		}

		/// <summary>
		/// The confirmation message to use if the Confirm flag is set
		/// </summary>
		[
		Bindable(true),
		Category("Behavior"),
		Description("The confirmation message to use if the Confirm flag is set"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public string ConfirmationMessage
		{
			get
			{
				return this.confirmationMessage;
			}
			set
			{
				this.confirmationMessage = value;
			}
		}

		/// <summary>
		/// Make sure that we have the right authorization or configuration to display/enable the control
		/// </summary>
		/// <param name="e">The parameters passed to handler</param>
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender (e);

			//Display the code to handle persistent tooltips
            this.Page.ClientScript.RegisterClientScriptResource(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.Tooltip.js");

			//Check to see if we are authorized, if not then disable the button
            if (Context != null && this.Enabled)
            {
                int currentRoleId = authorizedControlBase.ProjectRoleId;
                bool isSystemAdmin = authorizedControlBase.UserIsAdmin;
                bool isGroupAdmin = authorizedControlBase.UserIsGroupAdmin;
                if (authorizedControlBase.UserIsArtifactCreatorOrOwner)
                {
                    //Limited OK
                    this.Enabled = (authorizedControlBase.IsAuthorized(currentRoleId, isSystemAdmin, isGroupAdmin) != Project.AuthorizationState.Prohibited);
                }
                else
                {
                    //Limited not Sufficient
                    this.Enabled = (authorizedControlBase.IsAuthorized(currentRoleId, isSystemAdmin, isGroupAdmin) == Project.AuthorizationState.Authorized);
                }
            }
		}

		/// <summary>
		/// Adds the onclick confirmation event to the existing control
		/// </summary>
		/// <param name="writer">The HTML writer output stream</param>
		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			//Get the tooltip then remove from control
			string tooltip = this.ToolTip;
			this.ToolTip = "";

			//First add the standard attributes to the control
			base.AddAttributesToRender (writer);

			//Now add the confirmation message box if the flag is set
			if (this.Confirmation && this.Enabled)
			{
				//Add the confirmation message, escaping any single or double quotes
				string script = this.confirmationMessage;
				script = script.Replace ("'", @"\'");
				script = script.Replace ("\"", @"\""");
				writer.AddAttribute(HtmlTextWriterAttribute.Onclick, "return confirm ('" + script + "');");
			}

            //If disabled need to make it look disabled (not needed in IE, but may as well add to be consistent)
            if (!this.IsEnabled)
            {
                writer.AddStyleAttribute("color", "gray");
                writer.AddStyleAttribute("text-decoration", "none");
            }

			//Add the code to display a tooltip (used for items that get cut-off)
			if (tooltip != "")
			{
				writer.AddAttribute ("onMouseOver", "ddrivetip('" + ServerControlCommon.JSEncode(tooltip, false) + "');");
				writer.AddAttribute ("onMouseOut", "hideddrivetip();");
			}
		}
	}
}
