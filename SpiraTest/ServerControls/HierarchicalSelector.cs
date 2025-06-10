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

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// Displays an AJAX-enabled control that allows users to selected hierarchical artifacts in dialog boxes
    /// </summary>
    [
    ToolboxData("<{0}:HierarchicalSelector runat=server></{0}:HierarchicalSelector>"),
    ]
    public class HierarchicalSelector : WebControl, IScriptControl
    {
        protected Dictionary<string, string> handlers;

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

        [
        Category("Behavior"),
        DefaultValue(true),
        Description("Should the control allow multi-selecting of items")
        ]
        public bool MultipleSelect
        {
            get
            {
                object obj = ViewState["MultipleSelect"];

                return (obj == null) ? true : (bool)obj;
            }
            set
            {
                ViewState["MultipleSelect"] = value;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(true),
        Description("Should the control allow the selecting of summary items")
        ]
        public bool SummarySelect
        {
            get
            {
                object obj = ViewState["SummarySelect"];

                return (obj == null) ? true : (bool)obj;
            }
            set
            {
                ViewState["SummarySelect"] = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        [Editor(typeof(ImageUrlEditor), typeof(UITypeEditor))]
        [Description("Used to display a non-summary item")]
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

        [Category("Data")]
        [DefaultValue("Name")]
        [Description("The field used to display the artifact name")]
        public string NameField
        {
            get
            {
                object obj = ViewState["NameField"];

                return (obj == null) ? "Name" : (string)obj;
            }
            set
            {
                ViewState["NameField"] = value;
            }
        }

        [Category("Data")]
        [DefaultValue("Name")]
        [Description("The legend used to for the artifact name")]
        public string NameLegend
        {
            get
            {
                object obj = ViewState["NameLegend"];

                return (obj == null) ? Resources.Fields.Name : (string)obj;
            }
            set
            {
                ViewState["NameLegend"] = value;
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

        [Category("Appearance")]
        [DefaultValue("")]
        [Editor(typeof(ImageUrlEditor), typeof(UITypeEditor))]
        [Description("Used to display a summary item in its expanded state")]
        public string ExpandedItemImage
        {
            get
            {
                object obj = ViewState["expandedItemImage"];

                return (obj == null) ? string.Empty : (string)obj;
            }
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
            ScriptControlDescriptor descriptor = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.HierarchicalSelector", this.ClientID);

            if (!string.IsNullOrEmpty(WebServiceClass))
            {
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

            if (!string.IsNullOrEmpty(ItemImage))
            {
                descriptor.AddProperty("itemImage", ItemImage);
            }
            if (!string.IsNullOrEmpty(SummaryItemImage))
            {
                descriptor.AddProperty("summaryItemImage",SummaryItemImage);
            }
            if (!string.IsNullOrEmpty(ExpandedItemImage))
            {
                descriptor.AddProperty("expandedItemImage", ExpandedItemImage);
            }
            if (!string.IsNullOrEmpty(AlternateItemImage))
            {
                descriptor.AddProperty("alternateItemImage", AlternateItemImage);
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
            descriptor.AddProperty("nameField", this.NameField);
            descriptor.AddProperty("nameLegend", this.NameLegend);
            descriptor.AddProperty("autoLoad", this.AutoLoad);
            descriptor.AddProperty("multipleSelect", this.MultipleSelect);
            descriptor.AddProperty("summarySelect", this.SummarySelect);

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
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.HierarchicalSelector.js"));
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
                return HtmlTextWriterTag.Table;
            }
        }

        #endregion
    }
}
