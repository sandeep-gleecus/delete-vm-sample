using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Classes;
using System.Web.Security;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.ServerControls
{

	#region TabControl

	/// <summary>
	/// Summary description for wwWebTabControl.
	/// </summary>
    [
    ToolboxData("<{0}:TabControl runat=server></{0}:TabControl>"),
    ParseChildren(true),
    PersistChildren(false),
    Themeable(true)
    ]
    public class TabControl : Control, IScriptControl
    {
        protected Dictionary<string, string> handlers;

        #region Properties

        /// <summary>
        /// Collection of Tabpages.
        /// </summary>
        //[Bindable(true)]
        //[NotifyParentProperty(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]  // Content generates code for each page
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public TabPageCollection TabPages
        {
            get
            {
                return Tabs;
            }
        }
        private TabPageCollection Tabs = new TabPageCollection();

        /// <summary>
        /// Finds a specific tab page by its id
        /// </summary>
        /// <param name="id">The id being searched for</param>
        /// <returns>The matching TabPage (or null if none match)</returns>
        public TabPage GetTabById(string id)
        {
            //Loop through the tab-page collection to find the match
            foreach (TabPage tabPage in this.TabPages)
            {
                if (tabPage.ID == id)
                {
                    return tabPage;
                }
            }
            return null;
        }

        private new bool DesignMode = (HttpContext.Current == null);

        /// <summary>
        /// The Selected Tab. Set this to the TabPageClientId of the tab that you want to have selected
        /// </summary>
        [Browsable(true), Description("The TabPageClientId of the selected tab. This TabPageClientId must map to TabPageClientId assigned to a tab. Should also match an ID tag in the doc that is shown or hidden when tab is activated.")]
        public string SelectedTab
        {
            get
            {
                return this.cSelectedTab;
            }
            set
            {
                this.cSelectedTab = value;
            }
        }
        string cSelectedTab = "";

        /// <summary>
        /// The id of the page, used to store the expand/collapse state uniquely in settings
        /// </summary>
        [DefaultValue("")]
        public string PageId
        {
            get
            {
                if (ViewState["PageId"] == null)
                {
                    return "";
                }
                else
                {
                    return (string)ViewState["PageId"];
                }
            }
            set 
            {
                ViewState["PageId"] = value;
            }
        }

        /// <summary>
        /// The CSS Class of the container holding all the various tabs
        /// </summary>
        [Browsable(true), Description("The CSS Class of the container holding all the various tabs.")]
        public string CssClass
        {
            get { return this.cCssClass; }
            set { this.cCssClass = value; }
        }
        string cCssClass = "";

        /// <summary>
        /// The Width of the overall tab container
        /// </summary>
        [
        Browsable(true), Description("The Width of the overall tab container"),
        DefaultValue("100%")
        ]
        public string Width
        {
            get { return this.width; }
            set { this.width = value; }
        }
        protected string width = "100%";

        /// <summary>
        /// The CSS Class of the divider bar between the tabs and the page content
        /// </summary>
        [Browsable(true), Description("The CSS Class of the divider bar between the tabs and the page content.")]
        public string DividerCssClass
        {
            get { return this.cDividerCssClass; }
            set { this.cDividerCssClass = value; }
        }
        string cDividerCssClass = "";

        /// <summary>
        /// The width for each of the tabs. Each tab will be this width.
        /// </summary>
        [Browsable(true), Description("The width of all the individual tabs in pixels"), DefaultValue(110)]
        public int TabWidth
        {
            get { return this._TabWidth; }
            set { this._TabWidth = value; }
        }
        int _TabWidth = 110;

        /// <summary>
        /// The height of each of the tabs.
        /// </summary>
        [Browsable(true), Description("The Height of all the individual tabs in pixels"), DefaultValue(25)]
        public int TabHeight
        {
            get { return this._TabHeight; }
            set { this._TabHeight = value; }
        }
        int _TabHeight = 25;

        /// <summary>
        /// The CSS class that is used to render a selected button. Defaults to selectedtabbutton.
        /// </summary>
        [Browsable(true), Description("The CSS style used for the selected tab"), DefaultValue("selectedtabbutton")]
        public string SelectedTabCssClass
        {
            get { return this.cSelectedTabCssClass; }
            set { this.cSelectedTabCssClass = value; }
        }
        string cSelectedTabCssClass = "selectedtabbutton";

        /// <summary>
        /// The CSS class that is used to render a disabled tab. Defaults to disabledtabbutton.
        /// </summary>
        [Browsable(true), Description("The CSS style used for the disabled tab"), DefaultValue("disabledtabbutton")]
        public string DisabledTabCssClass
        {
            get { return this.cDisabledTabCssClass; }
            set { this.cDisabledTabCssClass = value; }
        }
        string cDisabledTabCssClass = "disabledtabbutton";

        /// <summary>
        /// The CSS class that is used to render nonselected tabs.
        /// </summary>
        [Browsable(true), Description("The CSS style used for non selected tabs"), DefaultValue("tabbutton")]
        public string TabCssClass
        {
            get { return this.cTabCssClass; }
            set { this.cTabCssClass = value; }
        }
        string cTabCssClass = "tabbutton";

        #endregion

        #region IScriptControl Members

        /// <summary>
        /// Return the various attributes to set on the client component
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            //Write the $create command that actually instantiates the control
            ScriptControlDescriptor desc = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.TabControl", this.ClientID);

            //Set the various attributes on the tab control
            desc.AddProperty("pageId", this.PageId);
            desc.AddProperty("tabWidth", this.TabWidth);
            desc.AddProperty("tabHeight", this.TabHeight);
            if (!String.IsNullOrEmpty(this.DividerCssClass))
            {
                desc.AddProperty("dividerCssClass", this.DividerCssClass);
            }
            if (!String.IsNullOrEmpty(this.TabCssClass))
            {
                desc.AddProperty("tabCssClass", this.TabCssClass);
            }
            if (!String.IsNullOrEmpty(this.SelectedTabCssClass))
            {
                desc.AddProperty("selectedTabCssClass", this.SelectedTabCssClass);
            }
            if (!String.IsNullOrEmpty(this.DisabledTabCssClass))
            {
                desc.AddProperty("disabledTabCssClass", this.DisabledTabCssClass);
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

            //Add any custom client-side handlers
            if (this.handlers != null)
            {
                foreach (KeyValuePair<string, string> handler in this.handlers)
                {
                    desc.AddEvent(handler.Key, handler.Value);
                }
            }

            //Now we need to add the tabs
            if (this.TabPages != null && this.TabPages.Count > 0)
            {
                desc.AddScriptProperty("tabPages", this.TabPages.ToJSConstructor(this.Parent));
                desc.AddProperty("selectedTabClientId", this.SelectedTab);
            }

            yield return desc;
        }

        /// <summary>
        /// Return the references to the client script resource files
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(TabControl), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.TabControl.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DropDownList.js"));
        }

        #endregion

        #region Overriden Events

        /// <summary>
        /// Make sure that we have the scriptmanager on the page and register the client component
        /// </summary>
        /// <param name="e">The Event Arguments</param>
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
        }

        /// <summary>
        /// Handle the postback of the currently selected tab
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnLoad(EventArgs e)
        {
            //Store the page id
            this.PageId = this.Page.GetType().FullName;
			
			MembershipUser user = Membership.GetUser();

			//See if we have a stored selected tab setting for the page/control
			//Only set from user settings if the selected tab was not already set (ie via the url)
			if (string.IsNullOrEmpty(this.SelectedTab))
			{
				if (user != null)
				{
					int userId = (int)user.ProviderUserKey;
					if (userId > 0)
					{
						UserSettingsCollection userSettingsCollection = new UserSettingsCollection(userId, GlobalFunctions.USER_SETTINGS_TAB_CONTROL_STATE);
						userSettingsCollection.Restore();
						string keyName = this.PageId + "-" + this.ClientID;
						if (userSettingsCollection[keyName] != null && userSettingsCollection[keyName] is String)
						{
							string selectedTab = (string)userSettingsCollection[keyName];
							this.SelectedTab = selectedTab;
						}
					}
				}
			}

            // *** Handle the Selected Tab Postback operation
            if (this.Page.IsPostBack)
            {
                string tabSelection = this.Page.Request.Form[this.ClientID + "_selectedTab"];
                if (tabSelection != "")
                {
                    this.SelectedTab = tabSelection;
                }
            }

            //Get the role to see if authorized or not (also checks license supports it)
            Business.ProjectManager projectManager = new Business.ProjectManager();
            int currentRoleId = -1;
            if (SpiraContext.Current != null && SpiraContext.Current.ProjectRoleId.HasValue)
            {
                currentRoleId = SpiraContext.Current.ProjectRoleId.Value;
            }
            //First we need to check permissions and make sure the selected tab exists
            foreach (TabPage tabPage in this.TabPages)
            {
                if (tabPage.CheckPermissions)
                {
                    UserManager userManager = new UserManager();
                    User fullUser = userManager.GetUserById(Convert.ToInt32(user.ProviderUserKey));

                    //First check if a user is an admin (and product member). If so give them a role of 1 - ie "Product Owner"
                    int projectRoleIdToCheck = currentRoleId;
                    if (fullUser.Profile.IsAdmin && currentRoleId > 0)
                    {
                        projectRoleIdToCheck = ProjectManager.ProjectRoleProjectOwner;
                    }

                    if (projectManager.IsAuthorized(projectRoleIdToCheck, tabPage.AuthorizedArtifactType, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized)
                    {
                        tabPage.Visible = false;
                    }
                }

                //If this page is the selected one and is hidden, unset the selected tab
                if (!tabPage.Visible && !String.IsNullOrEmpty(this.SelectedTab))
                {
                    string controlId = tabPage.TabPageControlId;
                    if (this.Parent != null && this.Parent.FindControl(controlId) != null)
                    {
                        if (this.Parent.FindControl(controlId).ClientID == SelectedTab)
                        {
                            SelectedTab = "";
                        }
                    }
                }
            }

            //Default to first visible tab if no tabs selected
            if (this.SelectedTab == "")
            {
                foreach (TabPage tabPage in this.TabPages)
                {
                    //Set selected to first visible
                    if (tabPage.Visible)
                    {
                        string controlId = tabPage.TabPageControlId;
                        if (this.Parent != null && this.Parent.FindControl(controlId) != null)
                        {
                            this.SelectedTab = this.Parent.FindControl(controlId).ClientID;
                            break;
                        }
                    }
                }
            }

            //If the selected tab has an AJAX container, check to see if we need to mark it as auto-loading
            foreach (TabPage tabPage in TabPages)
            {
                //See if we have an AJAX container that we need to set auto-load for
                if (String.IsNullOrEmpty(tabPage.AjaxControlContainer))
                {
                    //See if we are the selected tab
                    string controlId = tabPage.TabPageControlId;
                    if (!String.IsNullOrEmpty(tabPage.AjaxServerControlId) && this.Parent != null && this.Parent.FindControl(controlId) != null)
                    {
                        if (SelectedTab == this.Parent.FindControl(controlId).ClientID)
                        {
                            //See if we have a control that supports 'AutoLoad'
                            Control control = this.Parent.FindControlRecursive(tabPage.AjaxServerControlId);
                            if (control is SortedGrid)
                            {
                                ((SortedGrid)control).AutoLoad = true;
                            }
                            if (control is HierarchicalGrid)
                            {
                                ((HierarchicalGrid)control).AutoLoad = true;
                            }
                            if (control is OrderedGrid)
                            {
                                ((OrderedGrid)control).AutoLoad = true;
                            }
                        }
                    }
                }
                else
                {
                    //See if we are the selected tab
                    string controlId = tabPage.TabPageControlId;
                    if (this.Parent != null && this.Parent.FindControl(controlId) != null)
                    {
                        if (SelectedTab == this.Parent.FindControl(controlId).ClientID)
                        {
                            //Set the container (usually a user control panel) to auto-load
                            Control container = this.Parent.FindControl(tabPage.AjaxControlContainer);
                            if (container != null && container is UserControls.ArtifactUserControlBase)
                            {
                                UserControls.ArtifactUserControlBase userControl = (UserControls.ArtifactUserControlBase)container;
                                userControl.AutoLoad = true;
                            }
                        }
                    }
                }
            }

            base.OnLoad(e);
        }

        /// <summary>
        /// Renders the control
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            //Add the element's css attributes
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);
            writer.AddAttribute("role", "tablist");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, this.Width);

            //Render the DIV tag
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            //Now render the end tab
            writer.RenderEndTag();

            ScriptManager.GetCurrent(this.Page).RegisterScriptDescriptors(this);
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

        /// <summary>
        /// Adds a new item to the Tab collection.
        /// </summary>
        /// <param name="caption">The caption of the tab</param>
        /// <param name="actionLink">The HTTP or JavaScript link that is fired when the tab is activated. Can optionally be Default which activates the tab and activates the page ID.</param>
        /// <param name="tabPageControlId">The ID of the server control that this tab acts on</param>
        /// <param name="hasData">Does the tab contain data</param>
        public void AddTab(string caption, string actionLink, string tabPageControlId, bool hasData)
        {
            TabPage Tab = new TabPage();
            Tab.Caption = caption;
            //Tab.ActionLink = actionLink;
            Tab.TabPageControlId = tabPageControlId;
            Tab.HasData = hasData;
            this.AddTab(Tab);
        }

        /// <summary>
        /// Adds a new item to the Tab collection.
        /// </summary>
        /// <param name="caption">The caption of the tab</param>
        /// <param name="actionLink">The HTTP or JavaScript link that is fired when the tab is activated. Can optionally be Default which activates the tab and activates the page ID.</param>
        public void AddTab(string caption, string actionLink)
        {
            this.AddTab(caption, actionLink, "", false);
        }

        public void AddTab(TabPage Tab)
        {
            this.Tabs.Add(Tab);
        }

        /// <summary>
        /// Required to be able to properly deal with the Collection object
        /// </summary>
        /// <param name="obj"></param>
        protected override void AddParsedSubObject(object obj)
        {
            if (obj is TabPage)
            {
                this.TabPages.Add((TabPage)obj);
                return;
            }
        }
    }

	#endregion

	#region TabPage Class

	/// <summary>
	/// The individual TabPage class that holds the intermediary Tab page values
	/// </summary>
	[ToolboxData("<{0}:TabPage runat=server></{0}:TabPage>")] 
	public class TabPage : Control
	{
        /// <summary>
        /// Generates the JSON serialized version of the tab page for use by the client control
        /// </summary>
        /// <returns></returns>
        protected internal string ToJSConstructor(Control parentControl)
        {
            StringBuilder output = new StringBuilder();

            //Output the command to generate a new instance of the client component
            output.AppendFormat("new {0}", this.GetType().FullName);

            //Need to get the client id for the tab page control id (if there is one)
            string tabPageClientId = "";
            if (!String.IsNullOrEmpty(TabPageControlId))
            {
                //Find the control on the page
                if (parentControl != null)
                {
                    if (parentControl.FindControl(TabPageControlId) != null)
                    {
                        //We need to get the client id of this control
                        tabPageClientId = parentControl.FindControl(TabPageControlId).ClientID;
                    }
                }
            }

            //If we have an AJAX control that needs to get loaded also get the client id of that
            string ajaxControlClientId = "";
            if (!String.IsNullOrEmpty(AjaxServerControlId))
            {
                //Find the control on the page
                if (parentControl != null)
                {
                    //See if we have a naming container to use
                    if (String.IsNullOrEmpty(AjaxControlContainer))
                    {
                        if (parentControl.FindControl(AjaxServerControlId) != null)
                        {
                            //We need to get the client id of this control
                            ajaxControlClientId = parentControl.FindControl(AjaxServerControlId).ClientID;
                        }
                    }
                    else
                    {
                        if (parentControl.FindControl(AjaxControlContainer) != null)
                        {
                            Control container = parentControl.FindControl(AjaxControlContainer);
                            if (container.FindControl(AjaxServerControlId) != null)
                            {
                                //We need to get the client id of this control
                                ajaxControlClientId = container.FindControl(AjaxServerControlId).ClientID;
                            }
                        }
                    }
                }
            }

            //Pass the data in the constructor
            output.Append("(");
            output.AppendFormat("'{0}', ", ServerControlCommon.JSEncode(ID, false));
            output.AppendFormat("'{0}', ", ServerControlCommon.JSEncode(Caption, false));
            output.AppendFormat("{0}, ", HasData.ToString().ToLowerInvariant());
            output.AppendFormat("{0}, ", Enabled.ToString().ToLowerInvariant());
            output.AppendFormat("{0}, ", Visible.ToString().ToLowerInvariant());
            output.AppendFormat("'{0}', ", tabPageClientId);
            output.AppendFormat("'{0}', ", ajaxControlClientId);
            output.AppendFormat("'{0}', ", TabPageIcon);
            output.AppendFormat("'{0}', ", TabPageImageUrl);
			output.AppendFormat("'{0}'", TabName);

			output.Append(")");

            return output.ToString();
        }

		[
        NotifyParentProperty(true),
		Browsable(true),
        Description("The display caption for the Tab.")
        ]
		public string Caption 
		{
			get { return cCaption; }
			set { cCaption = value; }
		}
		string cCaption = "";

		[
		NotifyParentProperty(true),
		Browsable(true),
		Description("Does this tab have any data inside it")
		]
		public bool HasData
		{
			get
			{
				return this.cHasData;
			}
			set
			{
				this.cHasData = value;
			}
		}
		bool cHasData = false;

        [
        NotifyParentProperty(true),
        Browsable(true),
        Description("Do we need to check artifact view permissions for this tab"),
        DefaultValue(false)
        ]
        public bool CheckPermissions
        {
            get
            {
                return this.checkPermissions;
            }
            set
            {
                this.checkPermissions = value;
            }
        }
        bool checkPermissions = false;

        [
        NotifyParentProperty(true),
        Browsable(true),
        Description("Specifies the artifact view permissions needed for this tab"),
        DefaultValue(DataModel.Artifact.ArtifactTypeEnum.None)
        ]
        public DataModel.Artifact.ArtifactTypeEnum AuthorizedArtifactType
        {
            get
            {
                return this.authorizedArtifactType;
            }
            set
            {
                this.authorizedArtifactType = value;
            }
        }
        protected DataModel.Artifact.ArtifactTypeEnum authorizedArtifactType = DataModel.Artifact.ArtifactTypeEnum.None;

        [
        NotifyParentProperty(true),
        Browsable(true),
        Description("Is this tab enabled"),
        DefaultValue(true)
        ]
        public bool Enabled
        {
            get
            {
                return this.cEnabled;
            }
            set
            {
                this.cEnabled = value;
            }
        }
        protected bool cEnabled = true;

        [
        NotifyParentProperty(true),
        Browsable(true),
        Description("The server id of an AJAX control that needs to get fired when the tab first loads"),
        PersistenceMode(PersistenceMode.Attribute),
        DefaultValue("")
        ]
        public string AjaxServerControlId
        {
            get;
            set;
        }

        [
        NotifyParentProperty(true),
        Browsable(true),
        Description("The ASP.NET server control naming container that the AJAX control lives in."),
        PersistenceMode(PersistenceMode.Attribute),
        DefaultValue("")
        ]
        public string AjaxControlContainer
        {
            get;
            set;
        }


        [
        NotifyParentProperty(true),
        Browsable(true),
        Description("The TabPageControlId for this item. If you create a TabPageControlId you must create a matching ID tag in your server control that is to be enabled and disabled automatically."),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public string TabPageControlId
        {
            get
            {
                return this.cTabPageControlId;
            }
            set
            {
                this.cTabPageControlId = value;
            }
        }
        string cTabPageControlId = "";

        [
        Category("Appearance"),
        DefaultValue(""),
        Description("The icon classes used to represent the tab caption."),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public string TabPageIcon
        {
            get { return tabPageIcon; }
            set { tabPageIcon = value; }
        }
        string tabPageIcon = "";

        [
        Category("Appearance"),
        DefaultValue(""),
        Description("The url to the image used to represent the tab caption."),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public string TabPageImageUrl
        {
            get
            {

                return tabPageImageUrl;
            }
            set { tabPageImageUrl = value; }
        }
        string tabPageImageUrl = "";

		[
		Category("Appearance"),
		DefaultValue(""),
		Description("The name of the tab - equivalent to a non localized version of the caption used for example to set urls on changing tabs."),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public string TabName
		{
			get
			{

				return tabName;
			}
			set { tabName = value; }
		}
		string tabName = "";
	}

	#endregion


	#region TabPageCollection Class


	/// <summary>
	/// Holds a collection of Tab Pages
	/// </summary>
	public class TabPageCollection : CollectionBase 
	{
        /// <summary>
        /// Constructor
        /// </summary>
		public TabPageCollection()
		{
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
                TabPage tabPage = (TabPage)InnerList[i];
                if (i > 0)
                {
                    output.Append(", ");
                }
                output.Append(tabPage.ToJSConstructor(parentControl));
            }

            output.Append("]");

            return output.ToString();
        }

		/// <summary>
		/// Indexer property for the collection that returns and sets an item
		/// </summary>
		public TabPage this[int index]
		{
			get
			{
				return (TabPage) this.List[index];
			}
			set
			{
				this.List[index] = value;
			}
		}

        /// <summary>
        /// Indexer property for the collection that returns and sets an item
        /// </summary>
        /// <param name="controlId">The TabPageControlId of the tab page to be found</param>
        public TabPage this[string controlId]
        {
            get
            {
                foreach (TabPage tabPage in this.List)
                {
                    if (tabPage.TabPageControlId == controlId)
                    {
                        return tabPage;
                    }
                }
                return null;
            }
        }

		/// <summary>
		/// Adds a new error to the collection
		/// </summary>
		public void Add(TabPage Tab) 
		{
			this.List.Add(Tab);
		}

		public void Insert(int index, TabPage item) 
		{
			this.List.Insert(index,item);
		}
		
		public void Remove(TabPage Tab) 
		{
			List.Remove(Tab);
		}

		public bool Contains(TabPage Tab) 
		{
			return this.List.Contains(Tab);
		}

		//Collection IndexOf method 
		public int IndexOf(TabPage item) 
		{ 
			return List.IndexOf(item); 
		} 

		public void CopyTo(TabPage[] array, int index) 
		{ 
			List.CopyTo(array, index); 
		} 

	}
	#endregion
}
