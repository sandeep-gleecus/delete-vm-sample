using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;

using Inflectra.SpiraTest.Web.ServerControls.Authorization;
using Inflectra.SpiraTest.Business;
using System.Web;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.ServerControls
{
	/// <summary>
	/// This class extends the default image-button to include an additional
	/// property that can be used to display an alternate image when hovered-over
	/// </summary>
    /// <remarks>If theming is enabled on the page, automatically looks in the theme folder for the images</remarks>
	[ToolboxData("<{0}:ImageButtonEx runat=server></{0}:ImageButtonEx>")]
	public class ImageButtonEx : System.Web.UI.WebControls.ImageButton, IAuthorizedControl
	{
		protected string hoverImageUrl;
		protected string selectedImageUrl;
		protected bool selected;
		protected string confirmationMessage;
		protected AuthorizedControlBase authorizedControlBase;


		/// <summary>
		/// Constructor - delegates to base class
		/// </summary>
		public ImageButtonEx() : base()
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
                return (bool)ViewState["Confirmation"];
			}
			set
			{
                ViewState["Confirmation"] = value;
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

            //If theming enabled, automatically look in the relative folder (saves having to SkinID each control)
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
                //Need to handle some cases where the server and client path handling differes
                string imageUrl = this.ImageUrl;
                string hoverImageUrl = this.HoverImageUrl;
                //Handle the ~ case
                if (imageUrl.Contains("~"))
                {
                    imageUrl = this.ResolveUrl(imageUrl);
                }
                if (hoverImageUrl.Contains("~"))
                {
                    hoverImageUrl = this.ResolveUrl(hoverImageUrl);
                }

				//Need to remove any relative path modifiers (used when inside a user-control)
                imageUrl = imageUrl.Replace("../","");
                hoverImageUrl = hoverImageUrl.Replace("../","");
                writer.AddAttribute("onmouseover", "this.src='" + hoverImageUrl + "'", false);
                writer.AddAttribute("onmouseout", "this.src='" + imageUrl + "'", false);
			}

			//Now add the confirmation message box if the flag is set
			if (this.Confirmation)
			{
				//Add the confirmation message, escaping any single or double quotes
				string script = this.confirmationMessage;
				script = script.Replace ("'", @"\'");
				script = script.Replace ("\"", @"\""");
				writer.AddAttribute(HtmlTextWriterAttribute.Onclick, "return confirm ('" + script + "');");
			}

			//If the control is disabled, grey-it out and change the cursor to normal
			if (!this.Enabled)
			{
				//Need to handle the different browsers separately
				if (this.Context.Request.Browser.Browser.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "ie")
				{
					//Internet Explorer
					writer.AddStyleAttribute("cursor", "default");
					writer.AddStyleAttribute("filter", "alpha(opacity=50)");
				}
				else
				{
					//Mozilla and Opera
					writer.AddAttribute ("style", "cursor:default;opacity:0.5");
				}
			}
		}
	}
}
