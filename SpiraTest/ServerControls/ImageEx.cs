using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Web.ServerControls.Authorization;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// This class extends the default image control to include an additional
    /// property that can be used to display an alternate image when hovered-over
    /// and also the ability to fire client-side (Ajax) events if the user is authorized
    /// </summary>
    /// <remarks>Automatically makes images relative to the theme root if theming enabled</remarks>
    [ToolboxData("<{0}:ImageEx runat=server></{0}:ImageEx>")]
    public class ImageEx : System.Web.UI.WebControls.Image, IAuthorizedControl
    {
		protected string hoverImageUrl;
		protected string selectedImageUrl;
		protected bool selected;
		protected string confirmationMessage;
		protected AuthorizedControlBase authorizedControlBase;

		protected const string ViewStateKey_Confirmation = "Confirmation";

		/// <summary>
		/// Constructor - delegates to base class
		/// </summary>
        public ImageEx()
            : base()
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
		/// Contains the URL for an alternate image to display when the mouse hovers-over the button
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("Contains the URL for an alternate image to display when the mouse hovers-over the button"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public string HoverImageUrl
		{
			get
			{
				return (this.hoverImageUrl);
			}
			set
			{
				this.hoverImageUrl = value;
			}
		}

        [
        Category("Behavior"),
        DefaultValue(""),
        Description("The ID of the server control that we want to execute the client script method of (leave blank for a global function)")
        ]
        public string ClientScriptServerControlId
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["clientScriptServerControlId"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
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
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["clientScriptMethod"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["clientScriptMethod"] = value;
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
		/// Contains the URL for an alternate image to display when the button has been 'selected'
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("Contains the URL for an alternate image to display when the button has been selected"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public string SelectedImageUrl
		{
			get
			{
				return (this.selectedImageUrl);
			}
			set
			{
				this.selectedImageUrl = value;
			}
		}

		/// <summary>
		/// Specifies whether the button is in selected mode or not
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("Specifies whether the button is in selected mode or not"),
		DefaultValue(false),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public bool Selected
		{
			get
			{
				return (this.selected);
			}
			set
			{
				this.selected = value;
			}
		}

		/// <summary>
		/// If we have a button marked as selected, display that alternate image instead
		/// Also, make sure that we have the right authorization or configuration to display/enable the control
		/// </summary>
		/// <param name="e">The parameters passed to handler</param>
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender (e);
            //If themeing enabled, automatically look in the relative folder (saves having to SkinID each control)
            if (Page.EnableTheming && Page.Theme != "")
            {
                if (!String.IsNullOrEmpty(this.ImageUrl) && this.ImageUrl.IndexOf("App_Themes") == -1)
                {
                    this.ImageUrl = UrlRewriterModule.ResolveImages(this.ImageUrl, Page);
                }
                if (!String.IsNullOrEmpty(this.HoverImageUrl) && this.HoverImageUrl.IndexOf("App_Themes") == -1)
                {
                    this.HoverImageUrl = UrlRewriterModule.ResolveImages(this.HoverImageUrl, Page);
                }
                if (!String.IsNullOrEmpty(this.SelectedImageUrl) && this.SelectedImageUrl.IndexOf("App_Themes") == -1)
                {
                    this.SelectedImageUrl = UrlRewriterModule.ResolveImages(this.SelectedImageUrl, Page);
                }
            }

			if (this.selected && this.selectedImageUrl != "" && this.selectedImageUrl != null)
			{
				this.ImageUrl = this.SelectedImageUrl;
			}

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

                //Also write out a custom authorized attribute so that we can distinguish between unauthorized and other disabled conditions
                if (!this.Enabled)
                {
                    this.Attributes["tst:unauthorized"] = "unauthorized";
                }
            }
		}

		/// <summary>
		/// Adds the onmouseover and onmouseout client-side events to the existing control
		/// </summary>
		/// <param name="writer">The HTML writer output stream</param>
		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			//Next add the standard attributes to the control
			base.AddAttributesToRender (writer);

			//Now add the mouse-over attributes if we have an alternate URL
			if (this.HoverImageUrl != "" && this.HoverImageUrl != null && this.selected == false)
			{
				//Need to remove any path modifiers (used when inside a user-control)
				writer.AddAttribute("onmouseover", "this.src='" + this.HoverImageUrl.Replace("../","") + "'", false);
				writer.AddAttribute("onmouseout", "this.src='" + this.ImageUrl.Replace("../","") + "'", false);
			}

            //If we have client script associated, need to make it look like a button (cursor) and attach client handler
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
                writer.AddStyleAttribute("cursor", "pointer");
            }
		}
    }
}
