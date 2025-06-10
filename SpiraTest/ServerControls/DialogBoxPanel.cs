using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// Displays a client-side pop-up dialog box based on a standard ASP.NET Panel that gets its content
    /// from the server controls contained within the Panel.
    /// </summary>
    [
    ToolboxData("<{0}:DialogBoxPanel runat=server></{0}:DialogBoxPanel>")
    ]
    public class DialogBoxPanel : Panel, IScriptControl
    {
        protected Dictionary<string, string> handlers;

        #region Properties

        [
        Browsable(true),
        Description("The server id of an AJAX control that needs to get fired when the panel is displayed"),
        PersistenceMode(PersistenceMode.Attribute),
        DefaultValue("")
        ]
        public string AjaxServerControlId
        {
            get
            {
                object obj = ViewState["AjaxServerControlId"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["AjaxServerControlId"] = value;
            }
        }

        /// <summary>
        /// The title of the dialog box
        /// </summary>
        [
        Browsable(true),
        Description("The title of the dialog box"),
        PersistenceMode(PersistenceMode.Attribute),
        DefaultValue("")
        ]
        public string Title
        {
            get
            {
                object obj = ViewState["Title"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["Title"] = value;
            }
        }

        /// <summary>
        /// The height of the body of the navigation bar
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        Description("The left position of the dialog (leave null for auto)"),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public Unit Left
        {
            get
            {
                if (ViewState["Left"] == null)
                {
                    return Unit.Empty;
                }
                else
                {
                    Unit u = (Unit)ViewState["Left"];
                    return u;
                }
            }

            set
            {
                ViewState["Left"] = value;
            }
        }

        /// <summary>
        /// The height of the body of the navigation bar
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        Description("The left position of the dialog (leave null for auto)"),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public Unit Top
        {
            get
            {
                if (ViewState["Top"] == null)
                {
                    return Unit.Empty;
                }
                else
                {
                    Unit u = (Unit)ViewState["Top"];
                    return u;
                }
            }

            set
            {
                ViewState["Top"] = value;
            }
        }

        [Browsable(true)]
        [Description("Whether the dialog will close once the mouse is away from it.")]
        [PersistenceMode(PersistenceMode.Attribute)]
        [DefaultValue(true)]
        public bool Persistent
        {
            get
            {
                object obj = ViewState["Persistent"];

                return (obj == null) ? true : (bool)obj;
            }
            set
            {
                ViewState["Persistent"] = value;
            }
        }

        [Browsable(true)]
        [Description("Whether the dialog will display modally")]
        [PersistenceMode(PersistenceMode.Attribute)]
        [DefaultValue(false)]
        public bool Modal
        {
            get
            {
                object obj = ViewState["Modal"];
                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["Modal"] = value;
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
        Description("The ClientID of the HTML DOM element or ASP.NET server control that we want to use to display error messages (div, span, etc.)")
        ]
        public string ErrorMessageControlClientId
        {
            get
            {
                object obj = ViewState["ErrorMessageControlClientId"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["ErrorMessageControlClientId"] = value;
            }
        }
        /// <summary>
        /// Determine useragent of the device for use in setting popups as modal or not below
        /// </summary>
        protected bool IsUserAgentMobile()
        {
            Page page = HttpContext.Current.Handler as Page;
            string userAgent = page.Request.ServerVariables["HTTP_USER_AGENT"];
            bool isMobile = false;
            if (!String.IsNullOrEmpty(userAgent))
            {
                string userLeft = "";
                if (userAgent.Length > 4)
                {
                    userLeft = userAgent.Substring(0, 4);
                }
                else
                {
                    userLeft = userAgent;
                }
                if (GlobalFunctions.BrowserRegex.IsMatch(userAgent) || GlobalFunctions.VersionRegex.IsMatch(userLeft))
                {
                    isMobile = true;
                }
            }
            return isMobile;
        }

        #endregion

        #region IScriptControl Members

        /// <summary>
        /// Generate the properties, methods and events sent to the client component
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            ScriptControlDescriptor descriptor = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.DialogBoxPanel", this.ClientID);

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
            else if (!String.IsNullOrEmpty(ErrorMessageControlClientId))
            {
                //Sometimes we've given the actual error control client id
                descriptor.AddProperty("errorMessageControlId", ErrorMessageControlClientId);
            }

            if (!string.IsNullOrEmpty(AjaxServerControlId))
            {
                //First we need to get the server control
                Control ajaxServerControlId = this.Parent.FindControl(this.AjaxServerControlId);
                if (ajaxServerControlId != null)
                {
                    string clientId = ajaxServerControlId.ClientID;
                    descriptor.AddProperty("ajaxClientId", clientId);
                }
            }

            //Add the dialog box title (if we have any specified)
            if (!string.IsNullOrEmpty(Title))
            {
                descriptor.AddProperty("title", Title);
            }

            //Add any custom client-side handlers
            if (this.handlers != null)
            {
                foreach (KeyValuePair<string, string> handler in this.handlers)
                {
                    descriptor.AddEvent(handler.Key, handler.Value);
                }
            }

            if (!Left.IsEmpty && Left.Type == UnitType.Pixel)
            {
                descriptor.AddProperty("left", (int)this.Left.Value);
            }
            if (!Top.IsEmpty && Top.Type == UnitType.Pixel)
            {
                descriptor.AddProperty("top", (int)this.Top.Value);
            }

            descriptor.AddProperty("persistent", this.Persistent);

            bool isModal = this.Modal;
            if (!isModal && IsUserAgentMobile())
            {
                //Always make modal if on mobile device
                isModal = true;
            }
            descriptor.AddProperty("modal", isModal);
            yield return descriptor;
        }
        
        /// <summary>
        /// Generate the script reference
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DialogBoxPanel.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DragDrop.js"));
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Register the client script
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(System.EventArgs e)
        {
            base.OnPreRender(e);

            ScriptManager manager = ScriptManager.GetCurrent(this.Page);
            manager.RegisterScriptControl(this);

            //Now remove the height style attribute and replace with min-height so that it can stretch
            if (!this.Height.IsEmpty)
            {
                Unit height = this.Height;
                this.Height = Unit.Empty;
                this.Style.Add("min-height", height.ToString());
            }
        }

        /// <summary>
        /// Make the dialog box initially hidden
        /// </summary>
        /// <param name="writer"></param>
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            base.AddAttributesToRender(writer);
			writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none"); // required to make sure panels do not render visibly before being hidden (if display none added in css or js)
			//Need to specify this explicitly (i.e. not just relying on CSS)
			//because some of the child ajax controls will need to know this
			//when calculating offsets, etc.
			writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "absolute");
        }

        /// <summary>
        /// Add the component descriptors to the render
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            ScriptManager manager = ScriptManager.GetCurrent(this.Page);
            manager.RegisterScriptDescriptors(this);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Allows the passing in of a collection of client-side event handlers
        /// </summary>
        /// <param name="handlers">The collection of handlers</param>
        public void SetClientEventHandlers(Dictionary<string, string> handlers)
        {
            this.handlers = handlers;
        }

        #endregion
    }
}
