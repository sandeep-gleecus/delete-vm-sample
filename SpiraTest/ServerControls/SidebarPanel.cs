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
    /// Displays an AJAX-enabled sidebar that can be stretched in width.
    /// </summary>
    /// <remarks>
    /// Used to display other controls such as a treeview or standard datalist
    /// </remarks>
    [
    ToolboxData("<{0}:SidebarPanel runat=server></{0}:SidebarPanel>"),
    ]
    public class SidebarPanel : Panel, IScriptControl
    {
        protected Dictionary<string, string> handlers;
        protected ITemplate headerTemplate = null;

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
        /// Should the sidebar be minimized
        /// </summary>
        [
        Category("Appearance"),
        DefaultValue(false),
        Description("Should the sidebar be minimized")
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

        /// <summary>
        /// The height of the body of the sidebar
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        Description("The height of the body of the sidebar"),
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
        /// The height of the body of the sidebar
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        Description("The width of the body of the sidebar"),
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

        /// <summary>
        /// The minimum of the body of the sidebar
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        Description("The minimum width of the sidebar"),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public Unit MinWidth
        {
            get
            {
                if (ViewState["MinWidth"] == null)
                {
                    return Unit.Pixel(200);
                }
                else
                {
                    Unit u = (Unit)ViewState["MinWidth"];
                    return u;
                }
            }

            set
            {
                ViewState["MinWidth"] = value;
            }
        }

        /// <summary>
        /// The minimum of the body of the sidebar
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        Description("The maximum width of the sidebar"),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public Unit MaxWidth
        {
            get
            {
                if (ViewState["MaxWidth"] == null)
                {
                    return Unit.Pixel(1000);
                }
                else
                {
                    Unit u = (Unit)ViewState["MaxWidth"];
                    return u;
                }
            }

            set
            {
                ViewState["MaxWidth"] = value;
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
        /// The caption to display for the header
        /// </summary>
        [Category("Appearance")]
        [DefaultValue("")]
        [Description("The caption to display for the header")]
        public string HeaderCaption
        {
            get
            {
                object obj = ViewState["HeaderCaption"];

                return (obj == null) ? "" : (string)obj;
            }
            set
            {
                ViewState["HeaderCaption"] = value;
            }
        }

        /// <summary>
        /// The URL to use for the header
        /// </summary>
        [Category("Appearance")]
        [DefaultValue("")]
        [Description("The URL to use for the header")]
        public string HeaderUrl
        {
            get
            {
                object obj = ViewState["HeaderUrl"];

                return (obj == null) ? "" : (string)obj;
            }
            set
            {
                ViewState["HeaderUrl"] = value;
            }
        }

        /// <summary>
        /// Whether or not to display a refresh icon next to the caption for the header
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(false)]
        [Description("The caption to display for the header")]
        public bool DisplayRefresh
        {
            get
            {
                object obj = ViewState["DisplayRefresh"];

                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["DisplayRefresh"] = value;
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
            ScriptControlDescriptor descriptor = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.SidebarPanel", this.ClientID);

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
            descriptor.AddProperty("minimized", Minimized);

            if (this.BodyHeight != Unit.Empty)
            {
                descriptor.AddProperty("bodyHeight", this.BodyHeight.ToString());
            }
            if (this.BodyWidth != Unit.Empty)
            {
                descriptor.AddProperty("bodyWidth", this.BodyWidth.ToString());
            }
            descriptor.AddProperty("minWidth", (int)this.MinWidth.Value);
            descriptor.AddProperty("maxWidth", (int)this.MaxWidth.Value);
            descriptor.AddProperty("displayRefresh", this.DisplayRefresh);

            if (!string.IsNullOrEmpty(this.ClientScriptServerControlId))
            {
                //First we need to get the server control
                Control clientScriptControl = this.FindControlRecursive(this.ClientScriptServerControlId);
                if (clientScriptControl != null)
                {
                    string clientId = clientScriptControl.ClientID;
                    descriptor.AddProperty("clientScriptControlId", clientId);
                }
            }
            if (!string.IsNullOrEmpty(this.ClientScriptMethod))
            {
                descriptor.AddProperty("clientScriptMethod", ClientScriptMethod); 
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
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.SidebarPanel.js"));
        }

        #endregion

        #region Overrides

        protected override void OnPreRender(System.EventArgs e)
        {
            base.OnPreRender(e);

            ScriptManager manager = ScriptManager.GetCurrent(this.Page);
            manager.RegisterScriptControl(this);
        }

        /// <summary>
        /// Renders the table and cells that make up the sidebar
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            //We need to put the necessary tags manually to ensure that the appropriate DIVs are built

            //<DIV>
            this.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            //<DIV> - Header
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "panel-heading");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            if (String.IsNullOrEmpty(HeaderUrl))
            {
                writer.Write(HeaderCaption);
            }
            else
            {
                //Add a hyperlink

                //<A>
                writer.AddAttribute(HtmlTextWriterAttribute.Href, ResolveClientUrl(HeaderUrl));
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.Write(HeaderCaption);
                //</A>
                writer.RenderEndTag();
            }
            
            //</DIV>
            writer.RenderEndTag();

            //<DIV> - Body
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "panel-body");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            
            //Now render the contents
            this.RenderContents(writer);

            //</DIV></DIV>
            writer.RenderEndTag();
            writer.RenderEndTag();

            ScriptManager manager = ScriptManager.GetCurrent(this.Page);
            manager.RegisterScriptDescriptors(this);
        }

        #endregion
    }
}
