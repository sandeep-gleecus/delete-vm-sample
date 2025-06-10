using System;
using System.Drawing.Design;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Web.ServerControls.Authorization;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// Displays an AJAX-enabled navigation bar that displays a list of artifacts in list
    /// </summary>
    /// <remarks>
    /// Can handle SortedDataItems and HierarchicalDataItems appropriately as long as flag set correctly
    /// </remarks>
    [
    ToolboxData("<{0}:NavigationBar runat=server></{0}:NavigationBar>"),
    ]
    public class NavigationBar : WebControl, IScriptControl
    {
        protected Dictionary<string, object> filters;
        protected Dictionary<string, string> handlers;

        public enum DisplayModes
        {
            FilteredList = 1,
            AllItems = 2,
            Assigned = 3,
            Detected = 4,
            ForTestCase = 5,
            ForTestSet = 6,
            ForRelease = 7,
            ForRequirement = 8,
            ActiveOnly = 9
        }

        #region Properties

        /// <summary>
        /// Contains the project that the data is a part of
        /// </summary>
        [
        Category("Context"),
        DefaultValue(-1)
        ]
        public int ProjectId
        {
            get
            {
                object obj = ViewState["projectId"];

                return (obj == null) ? -1 : (int)obj;
            }
            set
            {
                ViewState["projectId"] = value;
            }
        }

        /// <summary>
        /// The ID of the currently selected artifact in the navigation bar
        /// </summary>
        [
        Category("Context"),
        DefaultValue(null)
        ]
        public Nullable<int> SelectedItemId
        {
            get
            {
                Nullable<int> obj = (Nullable<int>)ViewState["SelectedItemId"];

                return obj;
            }
            set
            {
                ViewState["SelectedItemId"] = value;
            }
        }

        /// <summary>
        /// The ID of any parent artifact that needs to be passed (optional). For example test steps need the parent test case
        /// </summary>
        [
        Category("Context"),
        DefaultValue(null)
        ]
        public Nullable<int> ContainerId
        {
            get
            {
                Nullable<int> obj = (Nullable<int>)ViewState["ContainerId"];

                return obj;
            }
            set
            {
                ViewState["ContainerId"] = value;
            }
        }

        /// <summary>
        /// The initial display mode for the navigation bar
        /// </summary>
        [
        Category("Appearance"),
        DefaultValue(DisplayModes.FilteredList),
        Description("The initial display mode for the navigation bar")
        ]
        public DisplayModes DisplayMode
        {
            get
            {
                object obj = ViewState["DisplayMode"];

                return (obj == null) ? DisplayModes.FilteredList : (DisplayModes)obj;
            }
            set
            {
                ViewState["DisplayMode"] = value;
            }
        }

        /// <summary>
        /// Should the navigation bar be minimized
        /// </summary>
        [
        Category("Appearance"),
        DefaultValue(false),
        Description("Should the navigation bar be minimized")
        ]
        public bool Minimized
        {
            get
            {
                object obj = ViewState["Minimized"];

                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["Minimized"] = value;
            }
        }

        /// <summary>
        /// Should we enable 'live-loading' where we can call the FormManager to load a different artifact (of the same type)
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(true),
        Description("Should we include an 'Assigned' option")
        ]
        public bool EnableLiveLoading
        {
            get
            {
                object obj = ViewState["EnableLiveLoading"];

                return (obj == null) ? true : (bool)obj;
            }
            set
            {
                ViewState["EnableLiveLoading"] = value;
            }
        }


        /// <summary>
        /// The ID of the related AjaxFormManager server control
        /// </summary>
        /// <remarks>
        /// Only used when LiveLoading is enabled
        /// </remarks>
        [
         Category("Behavior"),
         DefaultValue(""),
         Description("The ID of the related AjaxFormManager server control")
         ]
        public string FormManagerControlId
        {
            get
            {
                object obj = ViewState["FormManagerControlId"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["FormManagerControlId"] = value;
            }
        }


        /// <summary>
        /// Should we include an 'Assigned' option
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(true),
        Description("Should we include an 'Assigned' option")
        ]
        public bool IncludeAssigned
        {
            get
            {
                object obj = ViewState["IncludeAssigned"];

                return (obj == null) ? true : (bool)obj;
            }
            set
            {
                ViewState["IncludeAssigned"] = value;
            }
        }

        /// <summary>
        /// Should we include a 'Detected' option
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(false),
        Description("Should we include a 'Detected' option")
        ]
        public bool IncludeDetected
        {
            get
            {
                object obj = ViewState["IncludeDetected"];

                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["IncludeDetected"] = value;
            }
        }

        /// <summary>
        /// Should we include a 'For Test Case' option
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(false),
        Description("Should we include a 'For Test Case' option")
        ]
        public bool IncludeTestCase
        {
            get
            {
                object obj = ViewState["IncludeTestCase"];

                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["IncludeTestCase"] = value;
            }
        }

        /// <summary>
        /// Should we include a 'For Test Set' option
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(false),
        Description("Should we include a 'For Test Set' option")
        ]
        public bool IncludeTestSet
        {
            get
            {
                object obj = ViewState["IncludeTestSet"];

                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["IncludeTestSet"] = value;
            }
        }

        /// <summary>
        /// Should we include a 'For Release' option
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(false),
        Description("Should we include a 'For Release' option")
        ]
        public bool IncludeRelease
        {
            get
            {
                object obj = ViewState["IncludeRelease"];

                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["IncludeRelease"] = value;
            }
        }

        /// <summary>
        /// Should we include a 'For Requirement' option
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(false),
        Description("Should we include a 'For Requirement' option")
        ]
        public bool IncludeRequirement
        {
            get
            {
                object obj = ViewState["IncludeRequirement"];

                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["IncludeRequirement"] = value;
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
        DefaultValue(true),
        Description("Should the control automatically load when page first loaded")
        ]
        public bool AutoLoad
        {
            get
            {
                object obj = ViewState["autoLoad"];

                return (obj == null) ? true : (bool)obj;
            }
            set
            {
                ViewState["autoLoad"] = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        [Editor(typeof(ImageUrlEditor), typeof(UITypeEditor))]
        [Description("Used to display a normal item")]
        public string ItemImage
        {
            get
            {
                object obj = ViewState["itemImage"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["itemImage"] = value;
            }
        }

        /// <summary>
        /// The caption to display for the back to list page link
        /// </summary>
        [Category("Appearance")]
        [DefaultValue("")]
        [Description("The caption to display for the back to list page link")]
        public string ListScreenCaption
        {
            get
            {
                object obj = ViewState["ListScreenCaption"];

                return (obj == null) ? "" : (string)obj;
            }
            set
            {
                ViewState["ListScreenCaption"] = value;
            }
        }

        /// <summary>
        /// The URL to use for the back to list page link
        /// </summary>
        [Category("Appearance")]
        [DefaultValue("")]
        [Description("The URL to use for the back to list page link")]
        public string ListScreenUrl
        {
            get
            {
                object obj = ViewState["ListScreenUrl"];

                return (obj == null) ? "" : (string)obj;
            }
            set
            {
                ViewState["ListScreenUrl"] = value;
            }
        }

        /// <summary>
        /// The Base URL to use for items in the navigation bar
        /// </summary>
        [Category("Appearance")]
        [DefaultValue("")]
        [Description("The Base URL to use for items in the navigation bar")]
        public string ItemBaseUrl
        {
            get
            {
                object obj = ViewState["ItemBaseUrl"];

                return (obj == null) ? "" : (string)obj;
            }
            set
            {
                ViewState["ItemBaseUrl"] = value;
            }
        }

        /// <summary>
        /// The height of the body of the navigation bar
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        Description("The height of the body of the navigation bar"),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public Unit BodyHeight
        {
            get
            {
                if (ViewState["BodyHeight"] == null)
                {
                    return Unit.Empty;
                }
                else
                {
                    Unit u = (Unit)ViewState["BodyHeight"];
                    return u;
                }
            }

            set
            {
                ViewState["BodyHeight"] = value;
            }
        }

        /// <summary>
        /// The height of the body of the navigation bar
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        Description("The width of the body of the navigation bar"),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public Unit BodyWidth
        {
            get
            {
                if (ViewState["BodyWidth"] == null)
                {
                    return Unit.Empty;
                }
                else
                {
                    Unit u = (Unit)ViewState["BodyWidth"];
                    return u;
                }
            }

            set
            {
                ViewState["BodyWidth"] = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        [Editor(typeof(ImageUrlEditor), typeof(UITypeEditor))]
        [Description("Used to display an alterate item image (e.g. test case with steps or iteration vs. release)")]
        public string AlternateItemImage
        {
            get
            {
                object obj = ViewState["alternateItemImage"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["alternateItemImage"] = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        [Editor(typeof(ImageUrlEditor), typeof(UITypeEditor))]
        [Description("Used to display a summary item in its normal (non-expanded) state")]
        public string SummaryItemImage
        {
            get
            {
                object obj = ViewState["summaryItemImage"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["summaryItemImage"] = value;
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
        /// Allows the passing in of a collection of client-side event handlers
        /// </summary>
        /// <param name="handlers">The collection of handlers</param>
        public void SetClientEventHandlers(Dictionary<string, string> handlers)
        {
            this.handlers = handlers;
        }

        #endregion

        #region IScriptControl Members

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            ScriptControlDescriptor descriptor = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.NavigationBar", this.ClientID);

            if (!string.IsNullOrEmpty(WebServiceClass))
            {
                //We pass it as a script property because this control expects it as a javascript object not a string
                //unlike some of the other (older) controls
                descriptor.AddScriptProperty("webServiceClass", WebServiceClass);
            }
            if (!string.IsNullOrEmpty(CssClass))
            {
                descriptor.AddProperty("cssClass", CssClass);
            }
            if (ProjectId > -1)
            {
                descriptor.AddProperty("projectId", ProjectId);
            }
            descriptor.AddProperty("displayMode", (int)DisplayMode);
            descriptor.AddProperty("minimized", Minimized);
            descriptor.AddProperty("includeAssigned", IncludeAssigned);
            descriptor.AddProperty("includeDetected", IncludeDetected);
            descriptor.AddProperty("includeTestCase", IncludeTestCase);
            descriptor.AddProperty("includeTestSet", IncludeTestSet);
            descriptor.AddProperty("includeRelease", IncludeRelease);
            descriptor.AddProperty("includeRequirement", IncludeRequirement);
            if (EnableLiveLoading)
            {
                descriptor.AddProperty("enableLiveLoading", EnableLiveLoading);
                if (!String.IsNullOrEmpty(FormManagerControlId))
                {
                    //First we need to get the server control
                    if (this.Parent.FindControl(this.FormManagerControlId) != null && this.Parent.FindControl(this.FormManagerControlId) is AjaxFormManager)
                    {
                        AjaxFormManager formManager = (AjaxFormManager)this.Parent.FindControl(this.FormManagerControlId);
                        string clientId = formManager.ClientID;
                        descriptor.AddProperty("formManagerId", clientId);
                    }
                }
            }

            if (SelectedItemId.HasValue)
            {
                descriptor.AddProperty("selectedItemId", SelectedItemId.Value);
            }
            if (ContainerId.HasValue)
            {
                descriptor.AddProperty("containerId", ContainerId.Value);
            }

            if (!string.IsNullOrEmpty(ItemImage))
            {
                descriptor.AddProperty("itemImage", ItemImage);
            }
            if (!string.IsNullOrEmpty(AlternateItemImage))
            {
                descriptor.AddProperty("alternateItemImage", AlternateItemImage);
            }
            if (!string.IsNullOrEmpty(SummaryItemImage))
            {
                descriptor.AddProperty("summaryItemImage", SummaryItemImage);
            }
            if (!string.IsNullOrEmpty(ListScreenCaption))
            {
                descriptor.AddProperty("listScreenCaption", ListScreenCaption);
            }
            if (!string.IsNullOrEmpty(ListScreenUrl))
            {
                descriptor.AddProperty("listScreenUrl", UrlRewriterModule.ResolveUrl(ListScreenUrl));
            }
            if (!string.IsNullOrEmpty(ItemBaseUrl))
            {
                descriptor.AddProperty("itemBaseUrl", UrlRewriterModule.ResolveUrl(ItemBaseUrl));
            }

            if (this.BodyHeight != Unit.Empty)
            {
                descriptor.AddProperty("bodyHeight", this.BodyHeight.ToString());
            }
            if (this.BodyWidth != Unit.Empty)
            {
                descriptor.AddProperty("bodyWidth", this.BodyWidth.ToString());
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
            descriptor.AddProperty("autoLoad", this.AutoLoad);

            //Add any custom filters - need to serialize the values into strings
            if (this.filters != null)
            {
                descriptor.AddScriptProperty("standardFilters", JsonDictionaryConvertor.Serialize(GlobalFunctions.SerializeCollection(this.filters)));
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
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.NavigationBar.js"));
        }

        #endregion

        #region Overrides

        protected override void OnPreRender(System.EventArgs e)
        {
            base.OnPreRender(e);

            ScriptManager manager = ScriptManager.GetCurrent(this.Page);
            manager.RegisterScriptControl(this);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            ScriptManager manager = ScriptManager.GetCurrent(this.Page);
            manager.RegisterScriptDescriptors(this);
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
