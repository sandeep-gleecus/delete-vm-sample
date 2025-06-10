using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.Design;
using System.Web.UI.HtmlControls;
using System.Text;
using System.Diagnostics;
using Inflectra.SpiraTest.Web.ServerControls.Authorization;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.ServerControls
{

    /// <summary>
    /// Displays a client-side AJAX treeview
    /// </summary>
    [
    ToolboxData("<{0}:TreeView runat=server></{0}:TreeView>"),
    ]
    public class TreeView : WebControl, IScriptControl, IAuthorizedControl
    {
        protected Dictionary<string, string> handlers;
        protected AuthorizedControlBase authorizedControlBase;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public TreeView()
        {
            //Instantiate the authorized control default implementation
            authorizedControlBase = new AuthorizedControlBase(this.ViewState);
        }

        #region Properties

        /// <summary>
        /// Contains the id of the object that the tree belongs to (e.g. a project id)
        /// </summary>
        [
        Category("Context"),
        DefaultValue(-1)
        ]
        public int ContainerId
        {
            get
            {
                object obj = ViewState["containerId"];

                return (obj == null) ? -1 : (int)obj;
            }
            set
            {
                ViewState["containerId"] = value;
            }
        }

        /// <summary>
        /// Contains the id of the node we want to initially select
        /// </summary>
        [
        Category("Context"),
        DefaultValue(-1)
        ]
        public string SelectedNodeId
        {
            get
            {
                object obj = ViewState["selectedNodeId"];

                return (obj == null) ? "" : (string)obj;
            }
            set
            {
                ViewState["selectedNodeId"] = value;
            }
        }

        [
        Category("Data"),
        DefaultValue(""),
        Description("Contains the fully qualified namespace and class of the web service that will be providing the data to this Ajax server control")
        ]
        public string WebServiceClass
        {
            get
            {
                object obj = ViewState["webServiceClass"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["webServiceClass"] = value;
            }
        }

        [
        Category("Appearance"),
        DefaultValue(""),
        Description("Contains the filaname of the image to be displayed when data is loading")
        ]
        public string LoadingImageUrl
        {
            get
            {
                object obj = ViewState["loadingImageUrl"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["loadingImageUrl"] = value;
            }
        }

        [
        Category("Appearance"),
        DefaultValue(true),
        Description("Should the control automatically load when page first loaded")
        ]
        public bool AutoLoad
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["autoLoad"];

                return (obj == null) ? true : (bool)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["autoLoad"] = value;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(false),
        Description("Should the treeview allow items to be dragged onto it")
        ]
        public bool AllowDragging
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["allowDragging"];

                return (obj == null) ? false : (bool)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["allowDragging"] = value;
            }
        }

        [
         Category("Behavior"),
         DefaultValue(""),
         Description("The ID of the server control that we want to use to display error messages (div, span, etc.)")
         ]
        public string ErrorMessageControlId
        {
            get
            {
                object obj = ViewState["errorMessageControlId"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["errorMessageControlId"] = value;
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
        Description("The ID of the server control that we want to display the currently selected node")
        ]
        public string NodeLegendControlId
        {
            get
            {
                object obj = ViewState["nodeLegendControlId"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["nodeLegendControlId"] = value;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(""),
        Description("The format string for displaying the currently selected node. Use {0} to represent the node name")
        ]
        public string NodeLegendFormat
        {
            get
            {
                object obj = ViewState["NodeLegendFormat"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["NodeLegendFormat"] = value;
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

        [
        Category("Behavior"),
        DefaultValue(""),
        Description("The name of the items displayed in the treeview, used when displaying the Add/Edit dialog boxes")
        ]
        public string ItemName
        {
            get
            {
                object obj = ViewState["ItemName"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["ItemName"] = value;
            }
        }

        [
        Category("Appearance"),
        DefaultValue(false),
        Description("Should the treeview allow the nodes to be added to / edited / moved")
        ]
        public bool AllowEditing
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["allowEditing"];

                return (obj == null) ? false : (bool)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["allowEditing"] = value;
            }
        }

        [
        Category("Appearance"),
        DefaultValue(false),
        Description("Should the treeview allow the editing of the node descriptions")
        ]
        public bool EditDescriptions
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["EditDescriptions"];

                return (obj == null) ? false : (bool)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["EditDescriptions"] = value;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(""),
        Description("The URL template that is used if the treeview is able to rewrite the url when the node is clicked")
        ]
        public string PageUrlTemplate
        {
            get
            {
                object obj = ViewState["PageUrlTemplate"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["PageUrlTemplate"] = value;
            }
        }

        #endregion

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
        /// This is the type of artifact that the user's role needs to have permissions for to modify the treeview items
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
        /// This is the type of action that the user's role needs to have permissions for to modify the treeview items
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

        #region IScriptControl Members

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            ScriptControlDescriptor descriptor = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.TreeView", this.ClientID);

            if (!string.IsNullOrEmpty(WebServiceClass))
            {
                descriptor.AddScriptProperty("webServiceClass", WebServiceClass);
            }
            if (!string.IsNullOrEmpty(LoadingImageUrl))
            {
                descriptor.AddProperty("loadingImageUrl", LoadingImageUrl);
            }
            if (!string.IsNullOrEmpty(CssClass))
            {
                descriptor.AddProperty("cssClass", CssClass);
            }
            if (!string.IsNullOrEmpty(ItemName))
            {
                descriptor.AddProperty("itemName", ItemName);
            }

            if (ContainerId > -1)
            {
                descriptor.AddProperty("containerId", ContainerId);
            }
            descriptor.AddProperty("allowDragging", this.AllowDragging);
            descriptor.AddProperty("autoLoad", this.AutoLoad);
            descriptor.AddProperty("applicationPath", HttpContext.Current.Request.ApplicationPath);

            //Check to see if we are authorized, and pass value through
            bool allowEdit = this.AllowEditing;
            if (Context != null && allowEdit)
            {
                int currentRoleId = authorizedControlBase.ProjectRoleId;
                bool isSystemAdmin = authorizedControlBase.UserIsAdmin;
                bool isGroupAdmin = authorizedControlBase.UserIsGroupAdmin;
                allowEdit = (authorizedControlBase.IsAuthorized(currentRoleId, isSystemAdmin, isGroupAdmin) == Project.AuthorizationState.Authorized);
            }
            descriptor.AddProperty("allowEdit", allowEdit);
            if (allowEdit)
            {
                descriptor.AddProperty("editDescriptions", EditDescriptions);
            }

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

            if (!string.IsNullOrEmpty(ErrorMessageControlId))
            {
                //First we need to get the server control
                Control errorMessageControl = this.Parent.FindControl(this.ErrorMessageControlId);
                if (errorMessageControl != null)
                {
                    string clientId = errorMessageControl.ClientID;
                    descriptor.AddProperty("errorMessageControlId", clientId);
                }
            }

            if (!string.IsNullOrEmpty(NodeLegendControlId))
            {
                //First we need to get the server control
                Control nodeLegendControlId = this.Page.FindControlRecursive(this.NodeLegendControlId);
                if (nodeLegendControlId != null)
                {
                    string clientId = nodeLegendControlId.ClientID;
                    descriptor.AddProperty("nodeLegendControlId", clientId);
                    if (String.IsNullOrWhiteSpace(NodeLegendFormat))
                    {
                        descriptor.AddProperty("nodeLegendFormat", "{0}");
                    }
                    else
                    {
                        descriptor.AddProperty("nodeLegendFormat", NodeLegendFormat);
                    }
                }
            }

            //Add the event handler if we have a client method specified
            if (!string.IsNullOrEmpty(this.ClientScriptMethod))
            {
                //we need to use the event to fire the custom client method
                descriptor.AddEvent("nodeSelected", this.ClientID + "_nodeSelected");
            }
            //Pass the currently selected node if set
            if (!String.IsNullOrEmpty(this.SelectedNodeId))
            {
                descriptor.AddProperty("selectedNode", this.SelectedNodeId);
            }

            if (!String.IsNullOrEmpty(this.PageUrlTemplate))
            {
                descriptor.AddProperty("pageUrlTemplate", UrlRewriterModule.ResolveUrl(PageUrlTemplate));
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

        // Generate the script reference
        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DragDrop.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.Tooltip.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DialogBoxPanel.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DropDownHierarchy.js"));
            if (EditDescriptions)
            {
                //ckEditor
                yield return new ScriptReference("~/ckEditor/ckeditor-4.5.11.js");
                yield return new ScriptReference("~/ckEditor/adapters/jquery.js");
            }
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.TreeView.js"));
        }

        #endregion

        #region Overrides

        protected override void OnPreRender(System.EventArgs e)
        {
            base.OnPreRender(e);

            ScriptManager manager = ScriptManager.GetCurrent(this.Page);
            manager.RegisterScriptControl(this);

            if (!string.IsNullOrEmpty(this.ClientScriptMethod))
            {
                //If we don't have a server control then we can just execute the method
                //Otherwise we need to access the actual class
                string codeToExecute = "";
                if (string.IsNullOrEmpty(this.ClientScriptServerControlId))
                {
                    codeToExecute = this.ClientScriptMethod + "(args)";
                }
                else
                {
                    //First we need to get the server control
                    Control clientScriptControl = this.Page.FindControlRecursive(this.ClientScriptServerControlId);
                    if (clientScriptControl != null)
                    {
                        string clientId = clientScriptControl.ClientID;
                        codeToExecute = "$find('" + clientId + "')." + this.ClientScriptMethod + "(args)";
                    }
                }
                string script = "function " + this.ClientID + "_nodeSelected" + "(args) { " + codeToExecute + " }";
                ClientScriptManager clientScriptManager = Page.ClientScript;
                if (!clientScriptManager.IsClientScriptBlockRegistered(this.GetType(), this.UniqueID + "_nodeSelected"))
                {
                    clientScriptManager.RegisterClientScriptBlock(this.GetType(), this.UniqueID + "_nodeSelected", script, true);
                }
            }   
         
            if (EditDescriptions)
            {
                //Add the relative script location for ckeditor
                string jsScriptLocation = "var CKEDITOR_BASEPATH = '" + UrlRewriterModule.ResolveUrl("~/ckEditor/") + "';\n";
                ScriptManager.RegisterClientScriptBlock(this, typeof(ServerControlCommon), "CKEDITOR_BASEPATH", jsScriptLocation, true);
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            ScriptManager manager = ScriptManager.GetCurrent(this.Page);
            manager.RegisterScriptDescriptors(this);
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            base.AddAttributesToRender(writer);

            //--ARIA Attributes --
            //role=tree
            writer.AddAttribute("role", "tree");
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        #endregion
    }
}