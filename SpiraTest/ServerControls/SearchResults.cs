using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Web.ServerControls.Authorization;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// Displays a scrollable list of search results
    /// </summary>
    [ToolboxData("<{0}:SearchResults runat=server></{0}:SearchResults>")]
    public class SearchResults : WebControl, IScriptControl
    {
        protected Dictionary<string, string> handlers;

        #region Properties

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

        #endregion

        #region IScriptControl Members

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            ScriptControlDescriptor descriptor = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.SearchResults", this.ClientID);

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
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.SearchResults.js"));
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

        /// <summary>
        /// Allows the passing in of a collection of client-side event handlers
        /// </summary>
        /// <param name="handlers">The collection of handlers</param>
        public void SetClientEventHandlers(Dictionary<string, string> handlers)
        {
            this.handlers = handlers;
        }
    }
}
