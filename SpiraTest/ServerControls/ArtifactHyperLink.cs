using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;
using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;

using Inflectra.SpiraTest.Web.ServerControls.Authorization;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using System.Web;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// This class displays a hyperlink containing the name of the artifact, the artifact type image,
    /// and optionally a button to change the artifact. Often used on details pages to change an associated
    /// artifact where there are too many to display in a normal dropdown list (e.g. test sets, requirements)
    /// </summary>
    [ToolboxData("<{0}:ArtifactHyperLink runat=server></{0}:ArtifactHyperLink>")]
    public class ArtifactHyperLink : WebControl, IAuthorizedControl, IScriptControl
    {
		protected AuthorizedControlBase authorizedControlBase;
        protected Dictionary<string, string> handlers;

        /// <summary>
        /// Constructor - delegates to base class
        /// </summary>
        public ArtifactHyperLink()
            : base()
        {
            //Instantiate the authorized control default implementation
            authorizedControlBase = new AuthorizedControlBase(this.ViewState);
        }

        /// <summary>
        /// Allows the passing in of a collection of client-side event handlers
        /// </summary>
        /// <param name="handlers">The collection of handlers</param>
        public void SetClientEventHandlers(Dictionary<string, string> handlers)
        {
            this.handlers = handlers;
        }

        #region IAuthorizedControl Members

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

        #region Properties

        [Category("Appearance")]
        [DefaultValue("")]
        [Editor(typeof(ImageUrlEditor), typeof(UITypeEditor))]
        [Description("Used to display a non-summary item")]
        public string ItemImage
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["itemImage"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["itemImage"] = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        [Editor(typeof(ImageUrlEditor), typeof(UITypeEditor))]
        [Description("Used to display a summary item in its normal (non-expanded) state")]
        public string SummaryItemImage
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["summaryItemImage"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["summaryItemImage"] = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        [Editor(typeof(ImageUrlEditor), typeof(UITypeEditor))]
        [Description("Used to display an alterate item image (e.g. test case with steps or iteration vs. release)")]
        public string AlternateItemImage
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["alternateItemImage"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["alternateItemImage"] = value;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(false),
        Description("Should we display the option to change the selected artifact")
        ]
        public bool DisplayChangeLink
        {
            get
            {
                object obj = ViewState["DisplayChangeLink"];

                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["DisplayChangeLink"] = value;
            }
        }

        /// <summary>
        /// The alternate text for the artifact image
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(""),
        Description("alternate text for the artifact image")
        ]
        public string AlternateText
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["AlternateText"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["AlternateText"] = value;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(""),
        Description("The base URL that should be used when displaying links to the details page for the artifact in question")
        ]
        public string BaseUrl
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["baseUrl"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["baseUrl"] = value;
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Registers the client component
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            //Check to see if we are authorized, if not then disable the hyperlink
            if (Context != null && this.Enabled)
            {
                int currentRoleId = authorizedControlBase.ProjectRoleId;
                bool isSystemAdmin = authorizedControlBase.UserIsAdmin;
                bool isGroupAdmin = authorizedControlBase.UserIsGroupAdmin;
                this.Enabled = (authorizedControlBase.IsAuthorized(currentRoleId, isSystemAdmin, isGroupAdmin) == Project.AuthorizationState.Authorized);
            }

            ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);
            if (scriptManager == null)
            {
                throw new InvalidOperationException("ScriptManager required on the page.");
            }

            //Register the client component
            if (this.Visible && !String.IsNullOrWhiteSpace(this.ClientID))
            {
                scriptManager.RegisterScriptControl(this);
            }

            base.OnPreRender(e);
        }

        /// <summary>
        /// Adds the mouse-over tooltip events to the existing control
        /// </summary>
        /// <param name="writer">The HTML writer output stream</param>
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            //Get the tooltip then remove from control
            string tooltip = this.ToolTip;
            this.ToolTip = "";

            //First add the standard attributes to the control
            base.AddAttributesToRender(writer);

            //Put the tooltip back
            this.ToolTip = tooltip;
        }

        /// <summary>
        /// Render the various client component descriptors
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            //Ad the code to create a client component from this DOM element
            if (this.Visible && !String.IsNullOrWhiteSpace(this.ClientID))
            {
                ScriptManager manager = ScriptManager.GetCurrent(this.Page);
                manager.RegisterScriptDescriptors(this);
            }
        }

        #endregion

        #region IScriptControl Members

        /// <summary>
        /// Gets the ajax control descriptors
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            ScriptControlDescriptor descriptor = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.ArtifactHyperLink", this.ClientID);

            //Add the ajax properties
            descriptor.AddProperty("name", this.UniqueID);
            descriptor.AddProperty("enabled", this.Enabled);
            descriptor.AddProperty("tooltip", this.ToolTip);
            descriptor.AddProperty("alternateText", this.AlternateText);
            descriptor.AddProperty("displayChangeLink", this.DisplayChangeLink);

            //If theming is enabled, need to pass the theme folder so that images resolve correctly
            if (Page.EnableTheming && Page.Theme != "")
            {
                if (HttpContext.Current.Request.ApplicationPath == "/")
                {
                    descriptor.AddProperty("themeFolder", "/App_Themes/" + Page.Theme + "/");
                }
                else
                {
                    descriptor.AddProperty("themeFolder", HttpContext.Current.Request.ApplicationPath + "/App_Themes/" + Page.Theme + "/");
                }
            }

            //The various icon images
            if (!string.IsNullOrEmpty(ItemImage))
            {
                descriptor.AddProperty("itemImage", ItemImage);
            }
            if (!string.IsNullOrEmpty(SummaryItemImage))
            {
                descriptor.AddProperty("summaryItemImage", SummaryItemImage);
            }
            if (!string.IsNullOrEmpty(AlternateItemImage))
            {
                descriptor.AddProperty("alternateItemImage", AlternateItemImage);
            }
            if (!string.IsNullOrEmpty(BaseUrl))
            {
                descriptor.AddProperty("baseUrl", this.ResolveUrl(BaseUrl));
            }

            //Add any custom client-side handlers
            if (this.handlers != null)
            {
                foreach (KeyValuePair<string, string> handler in this.handlers)
                {
                    descriptor.AddEvent(handler.Key, handler.Value);
                }
            }

            yield return descriptor;
        }

        /// <summary>
        /// Generate the script references
        /// </summary>
        /// <returns>List of references</returns>
        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.ArtifactHyperLink.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.Tooltip.js"));
        }

        #endregion
    }
}