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
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    [
    ToolboxData("<{0}:SortedGrid runat=server></{0}:SortedGrid>"),
    PersistChildren(false),
    ParseChildren(true)
    ]
    public class SortedGrid : Control, IScriptControl, IAuthorizedControl
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

        [Category("Context")]
        [DefaultValue(null)]
        public int? DisplayTypeId
        {
            [DebuggerStepThrough()]
            get
            {
                return (int?)ViewState["displayTypeId"];
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["displayTypeId"] = value;
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
        /// Should the grid disable the checkboxes of items that have negative primary key values
        /// </summary>
        [
         Category("Appearance"),
         DefaultValue(true),
         Description("Should the grid disable the checkboxes of items that have negative primary key values")
        ]
        public bool NegativePrimaryKeysDisabled
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["NegativePrimaryKeysDisabled"];

                return (obj == null) ? true : (bool)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["NegativePrimaryKeysDisabled"] = value;
            }

        }

        [
        Category("Behavior"),
        DefaultValue(""),
        Description("The URL template that is used if the grid is able to rewrite the url when a folder entry is clicked")
        ]
        public string FolderUrlTemplate
        {
            get
            {
                object obj = ViewState["FolderUrlTemplate"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["FolderUrlTemplate"] = value;
            }
        }

        [
         Category("Appearance"),
         DefaultValue(true),
         Description("Should the grid display a tooltip on the name/desc hyperlink")
        ]
        public bool DisplayTooltip
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["DisplayTooltip"];

                return (obj == null) ? true : (bool)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["DisplayTooltip"] = value;
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
        Category("Behavior"),
        DefaultValue(false),
        Description("Should the grid allow drag operations on its artifacts")
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

        [
        Category("Appearance"),
        DefaultValue(true),
        Description("Should the grid display an attachments flag column")
        ]
        public bool DisplayAttachments
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["displayAttachments"];

                return (obj == null) ? true : (bool)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["displayAttachments"] = value;
            }
        }

        [
        Category("Appearance"),
        DefaultValue(true),
        Description("Should the grid display a checkbox column")
        ]
        public bool DisplayCheckboxes
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["DisplayCheckboxes"];

                return (obj == null) ? true : (bool)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["DisplayCheckboxes"] = value;
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
         Category("Behavior"),
         DefaultValue(""),
         Description("The target for the base URL, default is empty string (self)")
         ]
        public string BaseUrlTarget
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["baseUrlTarget"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["baseUrlTarget"] = value;
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
        [Description("Used to display the item's icon")]
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

        [Category("Appearance")]
        [DefaultValue("")]
        [Editor(typeof(ImageUrlEditor), typeof(UITypeEditor))]
        [Description("Used to display the image for folder items")]
        public string FolderItemImage
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["folderItemImage"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["folderItemImage"] = value;
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
        public SortedGrid()
        {
            //Instantiate the authorized control default implementation
            authorizedControlBase = new AuthorizedControlBase(this.ViewState);
        }

        /// <summary>
        /// Gets the list of selected checkboxes from the grid and sets the list of primary keys
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            //Clear the list of items
            this.selectedItems.Clear();

            foreach (string name in Page.Request.Form)
            {
                if (name != null && name.Length > 7 && name.Substring(0, 7) == "chkItem")
                {
                    string primaryKeyString = name.Substring(7, name.Length - 7);
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
            //role=grid
            writer.AddAttribute("role", "grid");

            writer.RenderBeginTag(HtmlTextWriterTag.Table);
            writer.RenderEndTag(); 

            if (!DesignMode)
            {
                ScriptManager.GetCurrent(this.Page).RegisterScriptDescriptors(this);
            }
        }

        /// <summary>
        /// Allows the passing in of a collection of client-side event handlers
        /// </summary>
        /// <param name="handlers">The collection of handlers</param>
        public void SetClientEventHandlers(Dictionary<string,string> handlers)
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
            if (!string.IsNullOrEmpty(AlternateItemImage))
            {
                desc.AddProperty("alternateItemImage", AlternateItemImage);
            }
            if (!string.IsNullOrEmpty(FolderItemImage))
            {
                desc.AddProperty("folderItemImage", FolderItemImage);
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

            if (!string.IsNullOrEmpty(BaseUrlTarget))
            {
                desc.AddProperty("baseUrlTarget", BaseUrlTarget);
            }

            if (!string.IsNullOrEmpty(ArtifactPrefix))
            {
                desc.AddProperty("artifactPrefix", ArtifactPrefix);
            }

            if (DisplayTypeId.HasValue)
            {
                desc.AddProperty("displayTypeId", DisplayTypeId.Value);
            }

            if (!String.IsNullOrEmpty(this.FolderUrlTemplate))
            {
                desc.AddProperty("folderUrlTemplate", UrlRewriterModule.ResolveUrl(FolderUrlTemplate));
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
            desc.AddProperty("autoLoad", this.AutoLoad);
            desc.AddProperty("displayTooltip", this.DisplayTooltip);
            desc.AddProperty("negativePrimaryKeysDisabled", this.NegativePrimaryKeysDisabled);
            desc.AddProperty("allowDragging", (this.AllowDragging && allowEdit));   //You have to be able to edit to also drag
            desc.AddProperty("allowColumnPositioning", this.AllowColumnPositioning);
            desc.AddProperty("displayAttachments", this.DisplayAttachments);
            desc.AddProperty("displayCheckboxes", this.DisplayCheckboxes);
            desc.AddProperty("concurrencyEnabled", this.ConcurrencyEnabled);
            desc.AddProperty("appPath", HttpContext.Current.Request.ApplicationPath);

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

            //Finally we need to specify the server date format that we're using (for date controls)
            string dateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower(System.Globalization.CultureInfo.InvariantCulture);

            //Add any custom client-side handlers
            if (this.handlers != null)
            {
                foreach (KeyValuePair<string, string> handler in this.handlers)
                {
                    desc.AddEvent(handler.Key, handler.Value);
                }
            }

            //Add any custom css classes (for specific fields)
            if (this.customCssClasses != null && this.customCssClasses.Count > 0)
            {
                JsonDictionaryOfStrings jsonDic = new JsonDictionaryOfStrings(this.customCssClasses);
                desc.AddScriptProperty("customCssClasses", JsonDictionaryConvertor.Serialize(jsonDic));
            }

            //Add any custom filters - need to serialize the values into strings
            if (this.filters != null)
            {
                desc.AddScriptProperty("standardFilters", JsonDictionaryConvertor.Serialize(GlobalFunctions.SerializeCollection(this.filters)));
            }

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

            yield return desc;
        }

        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DragDrop.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DropDownList.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DropDownHierarchy.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DateRangeFilter.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DatePicker.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.ContextMenu.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.Tooltip.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.NumberRangeFilter.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.SortedGrid.js"));
        }
    }
}
