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
	/// This class extends the default hyperlink to provide role-based security
	/// </summary>
	[ToolboxData("<{0}:HyperLinkEx runat=server></{0}:HyperLinkEx>")]
	public class HyperLinkEx : System.Web.UI.WebControls.HyperLink, IAuthorizedControl
	{
		protected AuthorizedControlBase authorizedControlBase;
        protected string confirmationMessage;

		/// <summary>
		/// Constructor - delegates to base class
		/// </summary>
		public HyperLinkEx() : base()
		{
			//Instantiate the authorized control default implementation
			authorizedControlBase = new AuthorizedControlBase(this.ViewState);
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
                if (ViewState["confirmation"] == null)
                {
                    return false;
                }
                else
                {
                    return (bool)ViewState["confirmation"];
                }
            }
            set
            {
                ViewState["confirmation"] = value;
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

        [
        Category("Behavior"),
        DefaultValue(""),
        Description("The ID of the server control that we want to execute the client script method of (leave blank for a global function)")
        ]
        public string ClientScriptServerControlId
        {
            get
            {
                object obj = ViewState["clientScriptServerControlId"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["clientScriptServerControlId"] = value;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(""),
        Description("The client side script method that we want to execute")
        ]
        public string ClientScriptMethod
        {
            get
            {
                object obj = ViewState["clientScriptMethod"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["clientScriptMethod"] = value;
            }
        }

		/// <summary>
		/// Adds additional attributes to the render, specifically for handling persistent tooltips
		/// </summary>
		/// <param name="writer">The HTML output stream</param>
		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			//Get the tooltip then remove from control
			string tooltip = this.ToolTip;
			this.ToolTip = "";

			//First add the standard attributes by calling base class
			base.AddAttributesToRender (writer);

			//Add the code to display a tooltip (used for items that get cut-off)
			if (tooltip != "")
			{
				writer.AddAttribute ("onMouseOver", "ddrivetip('" + ServerControlCommon.JSEncode(tooltip, false) + "');");
				writer.AddAttribute ("onMouseOut", "hideddrivetip();");
			}

            //If disabled need to make it look disabled (not needed in IE, but may as well add to be consistent)
            if (!this.IsEnabled)
            {
                writer.AddStyleAttribute("color", "gray");
                writer.AddStyleAttribute("text-decoration", "none");
                writer.AddAttribute("aria-disabled", "true");
            }

            //If we have client script associated, need attach client handler
            if (!string.IsNullOrEmpty(this.ClientScriptMethod) && this.Enabled)
            {
                //If we don't have a server control then we can just execute the method
                //Otherwise we need to access the actual class
                if (string.IsNullOrEmpty(this.ClientScriptServerControlId))
                {
                    //Now add the confirmation message box if the flag is set
                    if (this.Confirmation)
                    {
                        string script = this.confirmationMessage;
                        script = script.Replace("'", @"\'");
                        script = script.Replace("\"", @"\""");

                        writer.AddAttribute("onclick", "if (confirm ('" + script + "')) " + this.ClientScriptMethod + ";");
                    }
                    else
                    {
                        writer.AddAttribute("onclick", this.ClientScriptMethod);
                    }
                }
                else
                {
                    //First we need to get the server control
                    Control clientScriptControl = this.Parent.FindControl(this.ClientScriptServerControlId);
                    if (clientScriptControl != null)
                    {
                        string clientId = clientScriptControl.ClientID;
                        //Now add the confirmation message box if the flag is set
                        if (this.Confirmation)
                        {
                            string script = this.confirmationMessage;
                            script = script.Replace("'", @"\'");
                            script = script.Replace("\"", @"\""");

                            writer.AddAttribute("onclick", "if (confirm ('" + script + "')) $find('" + clientId + "')." + this.ClientScriptMethod + ";");
                        }
                        else
                        {
                            writer.AddAttribute("onclick", "$find('" + clientId + "')." + this.ClientScriptMethod);
                        }
                    }
                }
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
	}
}
