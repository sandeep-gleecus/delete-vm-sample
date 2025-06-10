using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Web;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.ServerControls.Authorization;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.ServerControls
{
	#region DropMenu class

	/// <summary>
	/// This class displays a pulldown menu that contains text and icon entries
	/// </summary>
    /// <remarks>Automatically makes the images in the dropmenu relative to the current theme folder</remarks>
	[ToolboxData("<{0}:DropMenu runat=server></{0}:DropMenu>")]
	public class DropMenu : WebControl, IAuthorizedControl, IPostBackEventHandler, IScriptControl
	{
		protected AuthorizedControlBase authorizedControlBase;
        protected string confirmationMessage;

		/// <summary>
		/// Constructor - delegates to base class
		/// </summary>
		public DropMenu() : base()
		{
			//Instantiate the authorized control default implementation
			authorizedControlBase = new AuthorizedControlBase(this.ViewState);

			//Instantiate the collection of menu entry items
			this.dropMenuItems = new DropMenuItemCollection();
		}

        #region IScriptControl Members

        /// <summary>
        /// Return the various attributes to set on the client component
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            //Write the $create command that actually instantiates the control
            ScriptControlDescriptor desc = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.DropMenu", this.ClientID);

            //Set the various attributes on the drop menu
            desc.AddProperty("enabled", this.Enabled);
            desc.AddProperty("authorized", this.Authorized);
            desc.AddProperty("authorizedDropMenuItems", this.Authorized_DropMenuItems);
			desc.AddProperty("buttonTextSelectsItem", this.ButtonTextSelectsItem);
            if (!this.MenuWidth.IsEmpty)
            {
                desc.AddProperty("menuWidth", this.MenuWidth);
            }
            if (!String.IsNullOrEmpty(this.CssClass))
            {
                desc.AddProperty("cssClass", this.CssClass);
            }
            if (!String.IsNullOrEmpty(this.MenuCssClass))
            {
                desc.AddProperty("menuCssClass", this.MenuCssClass);
            }
            if (!String.IsNullOrEmpty(this.ImageUrl))
            {
                desc.AddProperty("imageUrl", this.ImageUrl);
            }
            if (!String.IsNullOrEmpty(this.GlyphIconCssClass))
            {
                desc.AddProperty("glyphIconCssClass", this.GlyphIconCssClass);
            }
			if (!String.IsNullOrEmpty(this.Text))
            {
                desc.AddProperty("text", this.Text);
            }
            if (!String.IsNullOrEmpty(this.ClientScriptMethod))
            {
                desc.AddProperty("clientScriptMethod", this.ClientScriptMethod);
            }
            if (!String.IsNullOrEmpty(this.ImageAltText))
            {
                desc.AddProperty("imageAltText", this.ImageAltText);
            }
            if (Confirmation)
            {
                desc.AddProperty("confirmationMessage", this.ConfirmationMessage);
            }
            if (!String.IsNullOrEmpty(this.ClientScriptServerControlId))
            {
                //First we need to get the server control
                Control clientScriptControl = this.Parent.FindControl(this.ClientScriptServerControlId);
                if (clientScriptControl != null)
                {
                    string clientId = clientScriptControl.ClientID;
                    desc.AddProperty("clientScriptControlClientId", clientId);
                }
            }

            //If theming is enabled, need to pass the theme folder so that images resolve correctly
            if (Page.EnableTheming && Page.Theme != "")
            {
                if (HttpContext.Current.Request.ApplicationPath == "/")
                {
                    desc.AddProperty("themeFolder", "/App_Themes/" + Page.Theme + "/");
                }
                else
                {
                    desc.AddProperty("themeFolder", HttpContext.Current.Request.ApplicationPath + "/App_Themes/" + Page.Theme + "/");
                }
            }

            //Now we need to add the menu items
            if (this.dropMenuItems != null && this.dropMenuItems.Count > 0)
            {
                desc.AddScriptProperty("items", this.dropMenuItems.ToJSConstructor(this.Parent));
            }

            yield return desc;
        }

        /// <summary>
        /// Return the references to the client script resource files
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(DropMenu), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DropMenu.js"));
        }

        #endregion

        /// <summary>
        /// Writes out the HTML tag
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
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
                if (ViewState["Confirmation"] == null)
                {
                    return false;
                }
                else
                {
                    return (bool)ViewState["Confirmation"];
                }
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
		/// Collection of DropMenuItem entries
		/// </summary>
		[
		Bindable(true),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Visible),
		PersistenceMode(PersistenceMode.InnerProperty),
		Category("Data"),
		Description("Collection of DropMenuItem entries")
		]
		public DropMenuItemCollection DropMenuItems 
		{
			get
			{
				return this.dropMenuItems;
			}
			set
			{
				this.dropMenuItems = value;
			}
		}
		DropMenuItemCollection dropMenuItems;

		/// <summary>
		/// The width of the popup-menu
		/// </summary>
		[
		Browsable(true),
		Description("The width of the popup-menu"),
		Category("Appearance"),
		TypeConverter(typeof(UnitConverter)),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public Unit MenuWidth
		{
			get
			{
				return  this.menuWidth;
			}
			set
			{
				this.menuWidth = value;
			}
		}
		protected Unit menuWidth = Unit.Empty;

		/// <summary>
		/// The CSS class that is used to render the drop-down menu itself
		/// </summary>
		[
		Browsable(true),
		Description("The CSS class that is used to render the drop-down menu itself"),
		Category("Appearance"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public string MenuCssClass
		{
			get
			{
				return  this.menuCssClass;
			}
			set
			{
				this.menuCssClass = value;
			}
		}
		protected string menuCssClass = "";

        /// <summary>
        /// Contains meta-data about the field that can be used in dynamic controls (e.g. datagrid)
        /// </summary>
        [
        Bindable(true),
        Category("Misc"),
        Description("Contains meta-data about the field that can be used in dynamic controls"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public string MetaData
        {
            get
            {
                if (ViewState["MetaData"] == null)
                {
                    return "";
                }
                else
                {
                    return ((string)ViewState["MetaData"]);
                }
            }
            set
            {
                ViewState["MetaData"] = value;
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
        /// The specified permissions required to access the dropdown menu at all
        /// </summary>
        [
        Bindable(true),
        Category("Security"),
        Description("Does the user have sufficient permissions to access the dropdown menu at all"),
        DefaultValue(Project.PermissionEnum.None),
        PersistenceMode(PersistenceMode.Attribute),
        ]
        public Project.PermissionEnum Authorized_Permission_DropMenuItems
        {
            get
            {
                return authorizedControlBase.Authorized_Permission_SubControl;
            }
            set
            {
                authorizedControlBase.Authorized_Permission_SubControl = value;
            }
        }

        /// <summary>
        /// Is the user authorized to access the dropdown menu
        /// </summary>
        [
        Bindable(true),
        Category("Data"),
        Description("Should the dropdown menu render"),
        DefaultValue(true),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        protected bool Authorized_DropMenuItems
        {
            get
            {
                if (ViewState["Authorized_DropMenuItems"] == null)
                {
                    return true;
                }
                else
                {
                    return (bool)ViewState["Authorized_DropMenuItems"];
                }
            }
            set
            {
                ViewState["Authorized_DropMenuItems"] = value;
            }
        }


        /// <summary>
        /// Does clicking on the menu force validation to occur
        /// </summary>
        [
        Bindable(true),
        Category("Data"),
        Description("Does clicking on the menu force validation to occur"),
        DefaultValue(false),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public bool CausesValidation
        {
            get
            {
                if (ViewState["CausesValidation"] == null)
                {
                    return false;
                }
                else
                {
                    return (bool)ViewState["CausesValidation"];
                }
            }
            set
            {
                ViewState["CausesValidation"] = value;
            }
        }

        /// <summary>
        /// Is the user authorized to use this control
        /// </summary>
        [
        Bindable(true),
        Category("Data"),
        Description("Does clicking on the menu force validation to occur"),
        DefaultValue(true),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        protected bool Authorized
        {
            get
            {
                if (ViewState["Authorized"] == null)
                {
                    return true;
                }
                else
                {
                    return (bool)ViewState["Authorized"];
                }
            }
            set
            {
                ViewState["Authorized"] = value;
            }
        }

       
        /// <summary>
        /// Does clicking on the icon cause a postback event to occur
        /// </summary>
        [
        Bindable(true),
        Category("Data"),
        Description("Does clicking on the icon cause a postback event to occur"),
        DefaultValue(true),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public bool PostBackOnClick
        {
            get
            {
                if (ViewState["PostBackOnClick"] == null)
                {
                    return true;
                }
                else
                {
                    return (bool)ViewState["PostBackOnClick"];
                }
            }
            set
            {
                ViewState["PostBackOnClick"] = value;
            }
        }        

        /// <summary>
        /// The text to be displayed next to the icon
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        Description("The name of the validation group"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public string Text
        {
            get
            {
                if (ViewState["Text"] == null)
                {
                    return "";
                }
                else
                {
                    return (string)ViewState["Text"];
                }
            }
            set
            {
                ViewState["Text"] = value;
            }
        }

        /// <summary>
        /// The name of the validation group
        /// </summary>
        [
        Bindable(true),
        Category("Data"),
        Description("The name of the validation group"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public string ValidationGroup
        {
            get
            {
                if (ViewState["ValidationGroup"] == null)
                {
                    return "";
                }
                else
                {
                    return (string)ViewState["ValidationGroup"];
                }
            }
            set
            {
                ViewState["CausesValidation"] = value;
            }
        }

		/// <summary>
		/// Contains the URL for an image to display to the left of the menu text
		/// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        Description("Contains the URL for an image to display to the left of the menu text"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public string ImageUrl
        {
            get;
            set;
        }

        /// <summary>
        /// Contains the CSS Class for a glyph icon to display to the left of the menu text
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        Description("Contains the CSS Class for a glyph icon to display to the left of the menu text"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public string GlyphIconCssClass
        {
            get;
            set;
        }

		/// <summary>
		/// Is the user authorized to access the dropdown menu
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("Contains the CSS Class for a glyph icon to display to the left of the menu text"),
		DefaultValue(false),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public bool ButtonTextSelectsItem
		{
			get
			{
				return (this.buttonTextSelectsItem);
			}
			set
			{
				this.buttonTextSelectsItem = value;
			}
		}
        public bool buttonTextSelectsItem = false;

		[
        NotifyParentProperty(true),
        Browsable(true),
        DefaultValue(""),
        Description("The ALT text that should be displayed next to the image"),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public string ImageAltText
        {
            get
            {
                object obj = ViewState["ImageAltText"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["ImageAltText"] = value;
            }
        }

		// Redefines the Click event.
		public event DropMenuEventHandler Click;
      
		//Invoke delegates registered with the Click event.
		protected virtual void OnClick(DropMenuEventArgs e) 
		{         
			if (Click != null) 
			{
				Click(this, e);
			}   
		}

 		/// <summary>
		/// Make sure that we have the right authorization or configuration to display/enable the control
		/// </summary>
		/// <param name="e">The parameters passed to handler</param>
		protected override void OnPreRender(EventArgs e)
		{
            ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);
            if (scriptManager == null)
            {
                throw new InvalidOperationException("ScriptManager required on the page.");
            }

            //Register the client component
            scriptManager.RegisterScriptControl(this);
            ClientScriptManager clientScriptManager = Page.ClientScript;

            //Finally, execute the base class
            base.OnPreRender(e);

            //If theming enabled, automatically look in the relative folder (saves having to SkinID each control)
			if (Page.EnableTheming && Page.Theme != "")
			{
				if (!String.IsNullOrEmpty(this.ImageUrl) && this.ImageUrl.IndexOf("App_Themes") == -1)
				{
                    this.ImageUrl = UrlRewriterModule.ResolveImages(this.ImageUrl, Page);
				}
			}

            //Check to see if we are authorized, if not then set the Not Authorized property
            if (Context != null && this.Enabled)
            {
                int currentRoleId = authorizedControlBase.ProjectRoleId;
                bool isSystemAdmin = authorizedControlBase.UserIsAdmin;
                bool isGroupAdmin = authorizedControlBase.UserIsGroupAdmin;
                if (authorizedControlBase.UserIsArtifactCreatorOrOwner)
                {
                    //Limited OK
                    this.Authorized = (authorizedControlBase.IsAuthorized(currentRoleId, isSystemAdmin, isGroupAdmin) != Project.AuthorizationState.Prohibited);
                }
                else
                {
                    //Limited not Sufficient
                    this.Authorized = (authorizedControlBase.IsAuthorized(currentRoleId, isSystemAdmin, isGroupAdmin) == Project.AuthorizationState.Authorized);
                }
                this.Enabled = this.Authorized;
            }

            //Check to see if we are authorized for the dropdown menu items, if not then set the Not Authorized property
            if (Context != null && this.Enabled)
            {
                int currentRoleId = authorizedControlBase.ProjectRoleId;
                authorizedControlBase.Authorized_Permission_SubControl = this.Authorized_Permission_DropMenuItems;
                bool isSystemAdmin = authorizedControlBase.UserIsAdmin;
                bool isGroupAdmin = authorizedControlBase.UserIsGroupAdmin;
                if (authorizedControlBase.UserIsArtifactCreatorOrOwner)
                {
                    //Limited OK
                    this.Authorized_DropMenuItems = (authorizedControlBase.IsAuthorizedSubControl(currentRoleId, isSystemAdmin, isGroupAdmin) != Project.AuthorizationState.Prohibited);
                }
                else
                {
                    //Limited not Sufficient
                    this.Authorized_DropMenuItems = (authorizedControlBase.IsAuthorizedSubControl(currentRoleId, isSystemAdmin, isGroupAdmin) == Project.AuthorizationState.Authorized);
                }
            }

            //Server Side event handling
            //Create the postback url, handling the special case of when validation needs to be triggered
            if (string.IsNullOrEmpty(ClientScriptMethod) && this.PostBackOnClick)
            {
                string postbackHref = "";
                if (CausesValidation)
                {
                    PostBackOptions postBackOptions = new PostBackOptions(this);
                    postBackOptions.RequiresJavaScriptProtocol = true;
                    postBackOptions.PerformValidation = true;
                    postBackOptions.Argument = "";
                    postbackHref = this.Page.ClientScript.GetPostBackEventReference(postBackOptions, true);
                }
                else
                {
                    postbackHref = this.Page.ClientScript.GetPostBackClientHyperlink(this, "");
                }
                ClientScriptMethod = postbackHref;
            }


            //See if any of the drop menu items are meant to call server-side code
            foreach (DropMenuItem dropMenuItem in dropMenuItems)
            {
			    //Check to see if we are authorized, if not then disable the button
			    if (Context != null && this.Enabled)
			    {
                    int currentRoleId = authorizedControlBase.ProjectRoleId;
                    bool isSystemAdmin = authorizedControlBase.UserIsAdmin;
                    bool isGroupAdmin = authorizedControlBase.UserIsGroupAdmin;
                    dropMenuItem.Visible = (dropMenuItem.IsAuthorized(currentRoleId, isSystemAdmin, isGroupAdmin));
                }

                if (string.IsNullOrEmpty(dropMenuItem.ClientScriptMethod))
                {
                    //Server Side
                    //Create the postback url, handling the special case of when validation needs to be triggered
                    string postbackHref = "";
                    if (dropMenuItem.CausesValidation)
                    {
                        PostBackOptions postBackOptions = new PostBackOptions(this);
                        postBackOptions.RequiresJavaScriptProtocol = true;
                        postBackOptions.PerformValidation = true;
                        postBackOptions.Argument = dropMenuItem.Name;
                        postbackHref = this.Page.ClientScript.GetPostBackEventReference(postBackOptions, true);
                    }
                    else
                    {
                        postbackHref = this.Page.ClientScript.GetPostBackClientHyperlink(this, dropMenuItem.Name);
                    }
                    dropMenuItem.ClientScriptMethod = postbackHref;
                }
            }
		}

		// Re-implement the IPostBackEventHandler's RaisePostBackEvent method.
		// Note: C# allows this, where VB.NET does not.
		public void RaisePostBackEvent(System.String eventArgument)
		{
            //See if we need to handle Page.Validate events
            if (String.IsNullOrEmpty(eventArgument))
            {
                if (this.CausesValidation)
                {
                    this.Page.Validate(this.ValidationGroup);
                }
            }
            else
            {
                foreach (DropMenuItem dmi in this.dropMenuItems)
                {
                    if (dmi.Name == eventArgument)
                    {
                        if (dmi.CausesValidation)
                        {
                            this.Page.Validate(this.ValidationGroup);
                        }
                        break;
                    }
                }
            }

            // Raise the Click event of the custom Button web control.
            OnClick(new DropMenuEventArgs(eventArgument));
		}

		/// <summary>
		/// Creates the menu item array from the provided drop-menu items
		/// </summary>
		protected override void Render(HtmlTextWriter writer)
		{
            base.Render(writer);

            //Create the client component
            ScriptManager.GetCurrent(this.Page).RegisterScriptDescriptors(this);
		}

		/// <summary>
		/// Adds the onmouseover and onmouseout client-side events to the existing control
		/// </summary>
		/// <param name="writer">The HTML writer output stream</param>
		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			//Next add the standard attributes to the control
			base.AddAttributesToRender (writer);

            //ARIA attributes
            writer.AddAttribute("role", "menubar");
            writer.AddAttribute("aria-owns", this.ClientID + "_popup");

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
	#endregion

	#region DropMenuItem class

	/// <summary>
	/// This contains the image and textual information for a single menu entry
	/// </summary>
	[
	ToolboxData("<{0}:DropMenuItem runat=server></{0}:DropMenuItem>")
	]
    public class DropMenuItem : Control, IAuthorizedControl
	{
        protected AuthorizedControlBase authorizedControlBase;

        public DropMenuItem() : base()
        {
            //Instantiate the authorized control default implementation
            authorizedControlBase = new AuthorizedControlBase(this.ViewState);
        }

        /// <summary>
        /// Generates the JSON serialized version of the menu item for use by the client control
        /// </summary>
        /// <returns></returns>
        protected internal string ToJSConstructor(Control parentControl)
        {
            StringBuilder output = new StringBuilder();

            //Output the command to generate a new instance of the client component
            output.AppendFormat("new {0}", this.GetType().FullName);

            //Pass the data in the constructor
            output.Append("(");
            output.AppendFormat("'{0}', ", ServerControlCommon.JSEncode(Name, false));
            output.AppendFormat("'{0}', ", ServerControlCommon.JSEncode(Value, false));
            if (String.IsNullOrEmpty(ImageUrl))
            {
                output.AppendFormat("{0}, ", "null");
            }
            else
            {
                output.AppendFormat("'{0}', ", ServerControlCommon.JSEncode(UrlRewriterModule.ResolveImages(ImageUrl, parentControl.Page), false));
            }
            if (String.IsNullOrEmpty(GlyphIconCssClass))
            {
                output.AppendFormat("{0}, ", "null");
            }
            else
            {
                output.AppendFormat("'{0}', ", ServerControlCommon.JSEncode(GlyphIconCssClass, false));
            }
			if (String.IsNullOrEmpty(ItemCssClass))
			{
				output.AppendFormat("{0}, ", "null");
			}
			else
			{
				output.AppendFormat("'{0}', ", ServerControlCommon.JSEncode(ItemCssClass, false));
			}
			output.AppendFormat("'{0}', ", ServerControlCommon.JSEncode(ClientScriptMethod, false));
            output.AppendFormat("'{0}', ", ServerControlCommon.JSEncode(AlternateText, false));
            output.AppendFormat("{0}, ", Visible.ToString().ToLowerInvariant());
            if (String.IsNullOrEmpty(NavigateUrl))
            {
                output.Append("'', ");
            }
            else
            {
                output.AppendFormat("'{0}', ", ServerControlCommon.JSEncode(UrlRewriterModule.ResolveUrl(NavigateUrl), false));
            }
            output.AppendFormat("{0},", ServerControlCommon.JSEncode(Divider.ToString().ToLowerInvariant(), false));
            output.AppendFormat("{0},", ServerControlCommon.JSEncode(Confirmation.ToString().ToLowerInvariant(), false));
            if (String.IsNullOrEmpty(ConfirmationMessage))
            {
                output.Append("''");
            }
            else
            {
                output.AppendFormat("'{0}'", ServerControlCommon.JSEncode(ConfirmationMessage, false));
            }
            output.Append(")");

            return output.ToString();
        }

        /// <summary>
        /// Returns true if the user is authorized to use the item in the menu
        /// </summary>
        /// <param name="currentRoleId">Current role</param>
        /// <param name="isSystemAdmin">Is user a system admin</param>
        /// <param name="isGroupAdmin">Is the user a group admin</param>
        /// <returns>true if authorized</returns>
        public bool IsAuthorized(int currentRoleId, bool isSystemAdmin, bool isGroupAdmin)
        {
            bool enabled;
            if (authorizedControlBase.UserIsArtifactCreatorOrOwner)
            {
                //Limited OK
                enabled = (authorizedControlBase.IsAuthorized(currentRoleId, isSystemAdmin, isGroupAdmin) != Project.AuthorizationState.Prohibited);
            }
            else
            {
                //Limited not Sufficient
                enabled = (authorizedControlBase.IsAuthorized(currentRoleId, isSystemAdmin, isGroupAdmin) == Project.AuthorizationState.Authorized);
            }
            return enabled;
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

		[
		NotifyParentProperty(true),
		Browsable(true),
		Description("The URL of the image that should be displayed in the menu entry"),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public string ImageUrl 
		{
			get
			{
				return this.imageUrl;
			}
			set
			{
				this.imageUrl = value;
			}
		}
		protected string imageUrl;

        /// <summary>
        /// Contains the CSS Class for a glyph icon to display to the left of the menu text
        /// </summary>
        [
        NotifyParentProperty(true),
        Category("Appearance"),
        Description("Contains the CSS Class for a glyph icon to display to the left of the menu text"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public string GlyphIconCssClass
        {
            get;
            set;
        }

		/// <summary>
		/// Contains the CSS Class for a item to display
		/// </summary>
		[
		NotifyParentProperty(true),
		Category("Appearance"),
		Description("Contains the CSS Class for a item to display"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public string ItemCssClass
		{
			get;
			set;
		}

		[
        NotifyParentProperty(true),
        Browsable(true),
        Description("The URL to navigate to - used if we need a real hyperlink rather than a javascript/postback command"),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public string NavigateUrl
        {
            get
            {
                return this.navigateUrl;
            }
            set
            {
                this.navigateUrl = value;
            }
        }
        protected string navigateUrl;

        [
        NotifyParentProperty(true),
        Browsable(true),
        Description("Is this entry just a divider"),
        PersistenceMode(PersistenceMode.Attribute),
        DefaultValue(false)
        ]
        public bool Divider
        {
            get
            {
                return this.divider;
            }
            set
            {
                this.divider = value;
            }
        }
        protected bool divider = false;

		[
		NotifyParentProperty(true),
		Browsable(true),
		Description("The menu entry item text"),
		PersistenceMode(PersistenceMode.Attribute),
		DefaultValue("")
		]
		public string Value 
		{
			get
			{
				return this.value;
			}
			set
			{
				this.value = value;
			}
		}
		protected string value = "";

        [
        NotifyParentProperty(true),
        Browsable(true),
        Description("The menu entry item tooltip"),
        PersistenceMode(PersistenceMode.Attribute),
        DefaultValue("")
        ]
        public string AlternateText
        {
            get
            {
                return this.alternateText;
            }
            set
            {
                this.alternateText = value;
            }
        }
        protected string alternateText = "";

        [
         NotifyParentProperty(true),
         Browsable(true),
         Description("Should the button cause data validation to occur"),
         PersistenceMode(PersistenceMode.Attribute),
         DefaultValue(false)
         ]
        public bool CausesValidation
        {
            get
            {
                return this.causesValidation;
            }
            set
            {
                this.causesValidation = value;
            }
        }
        protected bool causesValidation = false;

        /// <summary>
        /// Does the option require a confirmation
        /// </summary>
        [
        NotifyParentProperty(true),
        Browsable(true),
        Description("Does the option require a confirmation"),
        PersistenceMode(PersistenceMode.Attribute),
        DefaultValue(false)
        ]
        public bool Confirmation
        {
            get;
            set;
        }

        /// <summary>
        /// The confirmation message (if confirmation enabled)
        /// </summary>
        [
        NotifyParentProperty(true),
        Browsable(true),
        Description("The confirmation message (if confirmation enabled)"),
        PersistenceMode(PersistenceMode.Attribute),
        DefaultValue("")
        ]
        public string ConfirmationMessage
        {
            get;
            set;
        }

		[
		NotifyParentProperty(true),
		Browsable(true),
		Description("The name associated with the menu entry - returned as the command argument"),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public string Name 
		{
			get
			{
				return this.name;
			}
			set
			{
				this.name = value;
			}
		}
		protected string name;

	}

	#endregion

	#region DropMenuItemCollection class

	/// <summary>
	/// Holds a collection of DropMenuItem objects
	/// </summary>
	public class DropMenuItemCollection : CollectionBase 
	{
		/// <summary>
		/// Constructor for class
		/// </summary>
		public DropMenuItemCollection()
		{
		}

		/// <summary>
		/// Indexer property for the collection that returns and sets an item
		/// </summary>
		public DropMenuItem this[int index]
		{
			get
			{
				return (DropMenuItem) this.List[index];
			}
			set
			{
				this.List[index] = value;
			}
		}

		/// <summary>
		/// Adds a new drop-menu to the collection
		/// </summary>
		/// <param name="item">The dropmenu item to add</param>
		public void Add(DropMenuItem item) 
		{
			this.List.Add(item);
		}

		/// <summary>
		/// Inserts a drop-menu item at a specific point in the collection
		/// </summary>
		/// <param name="index">The index of the location to insert at</param>
		/// <param name="item">The dropmenu item to insert</param>
		public void Insert(int index, DropMenuItem item) 
		{
			this.List.Insert(index, item);
		}
		
		/// <summary>
		/// Removes a drop-menu item from the collection
		/// </summary>
		/// <param name="item">The dropmenu item to remove</param>
		public void Remove(DropMenuItem item) 
		{
			List.Remove(item);
		}

		/// <summary>
		/// Determines if a specific drop-menu item is already in the collection
		/// </summary>
		/// <param name="item">The dropmenu item to search for</param>
		/// <returns>True if the item is already in the collection</returns>
		public bool Contains(DropMenuItem item) 
		{
			return this.List.Contains(item);
		}

		/// <summary>
		/// Collection IndexOf method 
		/// </summary>
		/// <param name="item">The dropmenu item to get the index of</param>
		/// <returns>The index that the item resides at in the collection</returns>
		public int IndexOf(DropMenuItem item) 
		{ 
			return List.IndexOf(item); 
		} 

		/// <summary>
		/// Copues an array of dropmenu items into the collection at the specified location
		/// </summary>
		/// <param name="array">The array of drop-menu items to copy</param>
		/// <param name="index">The location in the collection to copy them to</param>
		public void CopyTo(DropMenuItem[] array, int index) 
		{ 
			List.CopyTo(array, index); 
		}

        /// <summary>
        /// Generates the JSON version of this collection (JS array)
        /// </summary>
        /// <returns></returns>
        protected internal string ToJSConstructor(Control parentControl)
        {
            StringBuilder output = new StringBuilder();

            output.Append("[");

            //Iterate through the pages
            for (int i = 0; i < InnerList.Count; i++)
            {
                DropMenuItem dropMenuItem = (DropMenuItem)InnerList[i];
                if (i > 0)
                {
                    output.Append(", ");
                }
                output.Append(dropMenuItem.ToJSConstructor(parentControl));
            }

            output.Append("]");

            return output.ToString();
        }
	}

	#endregion

	#region Event Args
	public delegate void DropMenuEventHandler(object sender, DropMenuEventArgs e);
	public class DropMenuEventArgs : EventArgs
	{
		protected string selectedName;
		public DropMenuEventArgs (string selectedName)
		{
			this.selectedName = selectedName;
		}
		public string SelectedName
		{
			get
			{
				return this.selectedName;
			}
			set
			{
				this.selectedName = value;
			}
		}
	}
	#endregion
}
