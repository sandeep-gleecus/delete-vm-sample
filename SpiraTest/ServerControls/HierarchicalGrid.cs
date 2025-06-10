using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Web.ServerControls.Authorization;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// Displays a hierarchical AJAX-enabled grid control for use in any list screen that displays hierarchically
    /// organized grid data. Allows expanding, collapsing and inline multi-row editing
    /// </summary>
    [
    ToolboxData("<{0}:HierarchicalGrid runat=server></{0}:HierarchicalGrid>"),
    PersistChildren(false),
    ParseChildren(true)
    ]
    public class HierarchicalGrid : Control, IScriptControl, IAuthorizedControl
    {
        private const string DefaultEmptyDataText = "No records found.";
        protected AuthorizedControlBase authorizedControlBase;
        protected List<int> selectedItems = new List<int>();
        protected Dictionary<string, string> handlers;
        protected Dictionary<string, object> filters;
        protected List<ContextMenuItem> contextMenuItems = new List<ContextMenuItem>();
        protected Dictionary<string, string> customCssClasses;

        /// <summary>
        /// Returns a list of selected items (have their checkboxes selected)
        /// </summary>
        /// <remarks>Needs to be called after postback</remarks>
        public List<int> SelectedItems
        {
            get
            {
                return this.selectedItems;
            }
        }

        [
         Category("Appearance"),
         DefaultValue(true),
         Description("Should the grid allow the rows of data to be edited")
         ]
        public bool AllowEditing
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["allowEditing"];

                return (obj == null) ? true : (bool)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["allowEditing"] = value;
            }
        }

        /// <summary>
        /// Should the grid allow users to reposition its columns
        /// </summary>
        /// <remarks>The web service tied to the grid needs to also be able to handle this</remarks>
        [
        Category("Behavior"),
        DefaultValue(false),
        Description("Should the grid allow users to reposition its columns")
        ]
        public bool AllowColumnPositioning
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["AllowColumnPositioning"];

                return (obj == null) ? false : (bool)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["AllowColumnPositioning"] = value;
            }
        }

        /// <summary>
        /// Any menu items to be displayed on a right-click
        /// </summary>
        [
        Bindable(true),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Visible),
        PersistenceMode(PersistenceMode.InnerProperty),
        Category("Data"),
        Description("Collection of ContextMenuItem entries")
        ]
        public List<ContextMenuItem> ContextMenuItems
        {
            get
            {
                return this.contextMenuItems;
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

        [Category("Context")]
        [DefaultValue(-1)]
        public int ProjectId
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["projectId"];

                return (obj == null) ? -1 : (int)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["projectId"] = value;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(""),
        Description("The ID of the server control that we want to use to display error messages (div, span, etc.)")
        ]
        public string ErrorMessageControlId
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["errorMessageControlId"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["errorMessageControlId"] = value;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(""),
        Description("The ID of the server control that we want to use to display the total count of artifacts")
        ]
        public string TotalCountControlId
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["totalCountControlId"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["totalCountControlId"] = value;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(""),
        Description("The ID of the server control that we want to use to display the visible count of artifacts")
        ]
        public string VisibleCountControlId
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["visibleCountControlId"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["visibleCountControlId"] = value;
            }
        }

        [Category("Styles")]
        [DefaultValue("")]
        public string CssClass
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["cssClass"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["cssClass"] = value;
            }
        }

        [
         Category("Data"),
         DefaultValue(false),
         Description("Should the grid automatically handle optimistic concurrency")
         ]
        public bool ConcurrencyEnabled
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["concurrencyEnabled"];

                return (obj == null) ? false : (bool)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["concurrencyEnabled"] = value;
            }
        }

        [
        Category("Appearance"),
        DefaultValue(""),
        Description("The artifact prefix that use to display in the ID filter box")
        ]
        public string ArtifactPrefix
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["artifactPrefix"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["artifactPrefix"] = value;
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

        [
        Category("Data"),
        DefaultValue(""),
        Description("Contains the fully qualified namespace and class of the web service that will be providing the data to this Ajax server control")
        ]
        public string WebServiceClass
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["webServiceClass"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["webServiceClass"] = value;
            }
        }

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
        [Description("Used to display a summary item in its expanded state")]
        public string ExpandedItemImage
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["expandedItemImage"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["expandedItemImage"] = value;
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

        [Category("Styles")]
        [DefaultValue("")]
        public string HeaderCssClass
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["headerCssClass"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["headerCssClass"] = value;
            }
        }

        [Category("Styles")]
        [DefaultValue("")]
        public string SubHeaderCssClass
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["subHeaderCssClass"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["subHeaderCssClass"] = value;
            }
        }

        [Category("Styles")]
        [DefaultValue("")]
        public string RowCssClass
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["rowCssClass"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["rowCssClass"] = value;
            }
        }

        [Category("Styles")]
        [DefaultValue("")]
        public string SelectedRowCssClass
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["selectedRowCssClass"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["selectedRowCssClass"] = value;
            }
        }

        [Category("Styles")]
        [DefaultValue("")]
        public string EditRowCssClass
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["editRowCssClass"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["editRowCssClass"] = value;
            }
        }

        /// <summary>
        /// Should the grid automatically load when page first loaded
        /// </summary>
        [
        Category("Appearance"),
        DefaultValue(true),
        Description("Should the grid automatically load when page first loaded")
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

        /// <summary>
        /// Should the grid ignore the provided indent level for display purposes and just use a simple
        /// summary/detail two-level view (used for the requirements/tasks combined grids)
        /// </summary>
        [
        Category("Appearance"),
        DefaultValue(false),
        Description("Should the grid ignore the provided indent level for display purposes and just use a simple summary/detail two-level view (used for the requirements/tasks combined grids)")
        ]
        public bool ForceTwoLevelIndent
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["ForceTwoLevelIndent"];

                return (obj == null) ? false : (bool)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["ForceTwoLevelIndent"] = value;
            }
        }

        [
         Category("Behavior"),
         DefaultValue(""),
         Description("The ID of the server control that we want to use to display any filter information")
         ]
        public string FilterInfoControlId
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["FilterInfoControlId"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["FilterInfoControlId"] = value;
            }

        }

        [Category("Appearance")]
        [DefaultValue(DefaultEmptyDataText)]
        [Localizable(true)]
        public string EmptyDataText
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["emptyDataText"];

                return (obj == null) ? DefaultEmptyDataText : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                if (DefaultEmptyDataText != value)
                {
                    ViewState["emptyDataText"] = value;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public HierarchicalGrid()
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

        /// <summary>
        /// Allows the passing in of a collection of custom css classes per field
        /// </summary>
        /// <param name="handlers">The collection of css classes (key=field, value=class)</param>
        public void SetCustomCssClasses(Dictionary<string, string> customCssClasses)
        {
            this.customCssClasses = customCssClasses;
        }

        /// <summary>
        /// Allows the passing in of a collection of filters that override those stored in the user's profile
        /// </summary>
        /// <param name="filters">The collection of filters</param>
        /// <remarks>Useful if we want to provide specific views of the data</remarks>
        public void SetFilters(Dictionary<string, object> filters)
        {
            this.filters = filters;
        }

        /// <summary>
        /// Gets the list of selected checkboxes from the grid and sets the list of primary keys
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            //Clear the list of items
            this.selectedItems.Clear();

            string prefix = this.ClientID + "_chkItem";
            foreach (string name in Page.Request.Form)
            {
                if (name != null && name.Length > prefix.Length && name.Substring(0, prefix.Length) == prefix)
                {
                    string primaryKeyString = name.Substring(prefix.Length, name.Length - prefix.Length);
                    int primaryKey = -1;
                    if (Int32.TryParse(primaryKeyString, out primaryKey))
                    {
                        this.selectedItems.Add(primaryKey);
                    }
                }
            }

            //Now load the base control
            base.OnLoad(e);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);

            if (scriptManager == null)
            {
                throw new InvalidOperationException("ScriptManager required on the page.");
            }

            scriptManager.RegisterScriptControl(this);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);

            if (!string.IsNullOrEmpty(CssClass))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClass);
            }
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");

            //--ARIA Attributes --
            //role=treegrid
            writer.AddAttribute("role", "treegrid");

            writer.RenderBeginTag(HtmlTextWriterTag.Table);
            writer.RenderEndTag(); 

            if (!DesignMode)
            {
                ScriptManager.GetCurrent(this.Page).RegisterScriptDescriptors(this);
            }
        }

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            ScriptControlDescriptor desc = new ScriptControlDescriptor(this.GetType().FullName, ClientID);

            if (!string.IsNullOrEmpty(CssClass))
            {
                desc.AddProperty("cssClass", CssClass);
            }

            if (!string.IsNullOrEmpty(HeaderCssClass))
            {
                desc.AddProperty("headerCssClass", HeaderCssClass);
            }

            if (!string.IsNullOrEmpty(SubHeaderCssClass))
            {
                desc.AddProperty("subHeaderCssClass", SubHeaderCssClass);
            }

            if (!string.IsNullOrEmpty(RowCssClass))
            {
                desc.AddProperty("rowCssClass", RowCssClass);
            }

            if (!string.IsNullOrEmpty(SelectedRowCssClass))
            {
                desc.AddProperty("selectedRowCssClass", SelectedRowCssClass);
            }

            if (!string.IsNullOrEmpty(EditRowCssClass))
            {
                desc.AddProperty("editRowCssClass", EditRowCssClass);
            }

            if (EmptyDataText != DefaultEmptyDataText)
            {
                desc.AddProperty("emptyDataText", EmptyDataText);
            }

            if (!string.IsNullOrEmpty(ItemImage))
            {
                desc.AddProperty("itemImage", ItemImage);
            }

            if (!string.IsNullOrEmpty(SummaryItemImage))
            {
                desc.AddProperty("summaryItemImage", SummaryItemImage);
            }

            if (!string.IsNullOrEmpty(ExpandedItemImage))
            {
                desc.AddProperty("expandedItemImage", ExpandedItemImage);
            }

            if (!string.IsNullOrEmpty(AlternateItemImage))
            {
                desc.AddProperty("alternateItemImage", AlternateItemImage);
            }

            if (ProjectId > -1)
            {
                desc.AddProperty("projectId", ProjectId);
            }

            if (!string.IsNullOrEmpty(WebServiceClass))
            {
                desc.AddScriptProperty("webServiceClass", WebServiceClass);
            }

            if (!string.IsNullOrEmpty(BaseUrl))
            {
                desc.AddProperty("baseUrl", this.ResolveUrl(BaseUrl));
            }

            if (!string.IsNullOrEmpty(ArtifactPrefix))
            {
                desc.AddProperty("artifactPrefix", ArtifactPrefix);
            }
            desc.AddProperty("concurrencyEnabled", this.ConcurrencyEnabled);
            desc.AddProperty("appPath", HttpContext.Current.Request.ApplicationPath);
            desc.AddProperty("autoLoad", this.AutoLoad);
            desc.AddProperty("forceTwoLevelIndent", this.ForceTwoLevelIndent);
            desc.AddProperty("allowColumnPositioning", this.AllowColumnPositioning);

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

            //Check to see if we are authorized, and pass value through
            bool allowEdit = this.AllowEditing;
            if (Context != null && allowEdit)
            {
                int currentRoleId = authorizedControlBase.ProjectRoleId;
                bool isSystemAdmin = authorizedControlBase.UserIsAdmin;
                bool isGroupAdmin = authorizedControlBase.UserIsGroupAdmin;
                allowEdit = (authorizedControlBase.IsAuthorized(currentRoleId, isSystemAdmin, isGroupAdmin) == Project.AuthorizationState.Authorized);

                //Iterate through the context menu items and pass to client control
                //These need to be authorized individually
                string contextMenuItemDescriptors = "";
                foreach (ContextMenuItem contextMenuItem in this.contextMenuItems)
                {
                    //Only add authorized items (i.e. only actions the current user is allowed to do)
                    if (contextMenuItem.IsAuthorized(currentRoleId, isSystemAdmin, isGroupAdmin))
                    {
                        if (contextMenuItemDescriptors != "")
                        {
                            contextMenuItemDescriptors += ",";
                        }
                        contextMenuItemDescriptors += contextMenuItem.ToJsonString();
                    }
                }
                //Get each item as a Dictionary of Strings
                desc.AddScriptProperty("contextMenuItems", "[" + contextMenuItemDescriptors + "]");
            }
            desc.AddProperty("allowEdit", allowEdit);

            if (!string.IsNullOrEmpty(ErrorMessageControlId))
            {
                //First we need to get the server control
                Control errorMessageControl = this.Parent.FindControl(this.ErrorMessageControlId);
                if (errorMessageControl != null)
                {
                    string clientId = errorMessageControl.ClientID;
                    desc.AddProperty("errorMessageControlId", clientId);
                }
            }

            if (!string.IsNullOrEmpty(TotalCountControlId))
            {
                //First we need to get the server control
                Control totalCountControl = this.Parent.FindControl(this.TotalCountControlId);
                if (totalCountControl != null)
                {
                    string clientId = totalCountControl.ClientID;
                    desc.AddProperty("totalCountControlId", clientId);
                }
            }

            if (!string.IsNullOrEmpty(VisibleCountControlId))
            {
                //First we need to get the server control
                Control visibleCountControl = this.Parent.FindControl(this.VisibleCountControlId);
                if (visibleCountControl != null)
                {
                    string clientId = visibleCountControl.ClientID;
                    desc.AddProperty("visibleCountControlId", clientId);
                }
            }

            if (!string.IsNullOrEmpty(FilterInfoControlId))
            {
                //First we need to get the server control
                Control filterInfoControl = this.Parent.FindControl(this.FilterInfoControlId);
                if (filterInfoControl != null)
                {
                    string clientId = filterInfoControl.ClientID;
                    desc.AddProperty("filterInfoControlId", clientId);
                }
            }

            //Add any custom css classes (for specific fields)
            if (this.customCssClasses != null && this.customCssClasses.Count > 0)
            {
                JsonDictionaryOfStrings jsonDic = new JsonDictionaryOfStrings(this.customCssClasses);
                desc.AddScriptProperty("customCssClasses", JsonDictionaryConvertor.Serialize(jsonDic));
            }

            //Finally we need to specify the server date format that we're using (for date controls)
            string dateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower(System.Globalization.CultureInfo.InvariantCulture);

            //Need to convert to the format expected by the control:
            //	m Month number (01 - January, etc.) 
            //	M = Month name 
            //	d = Date 
            //	y = Last two digits of the year 
            //	Y = All four digits of the year 
            //	W = Name of the day of the week (Monday, etc.)
            dateFormat = dateFormat.Replace("mm", "m");
            dateFormat = dateFormat.Replace("dd", "d");
            dateFormat = dateFormat.Replace("yyyy", "Y");
            dateFormat = dateFormat.Replace("yy", "y");
            desc.AddProperty("dateFormat", dateFormat);

            //Add any custom client-side handlers
            if (this.handlers != null)
            {
                foreach (KeyValuePair<string, string> handler in this.handlers)
                {
                    desc.AddEvent(handler.Key, handler.Value);
                }
            }

            //Add any custom filters - need to serialize the values into strings
            if (this.filters != null)
            {
                desc.AddScriptProperty("standardFilters", JsonDictionaryConvertor.Serialize(GlobalFunctions.SerializeCollection(this.filters)));
            }

            yield return desc;
        }

        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DragDrop.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DropDownList.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DropDownHierarchy.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DatePicker.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DateRangeFilter.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.ContextMenu.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.Tooltip.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.NumberRangeFilter.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.HierarchicalGrid.js"));
        }
    }
}
