using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// AJAX control that captures screenshots using the HTML Clipboard API
    /// </summary>
    [ToolboxData("<{0}:ScreenshotCapture runat=server></{0}:ScreenshotCapture>")]
    public class ScreenshotCapture : WebControl, IScriptControl
    {
        protected Dictionary<string, string> handlers;

        /// <summary>
        /// Allows the passing in of a collection of client-side event handlers
        /// </summary>
        /// <param name="handlers">The collection of handlers</param>
        public void SetClientEventHandlers(Dictionary<string, string> handlers)
        {
            this.handlers = handlers;
        }

        #region Properties

        //[
        //Category("Data"),
        //DefaultValue(""),
        //Description("The % green to display")
        //]
        //public int PercentGreen
        //{
        //    [DebuggerStepThrough()]
        //    get
        //    {
        //        object obj = ViewState["percentGreen"];

        //        return (obj == null) ? 0 : (int)obj;
        //    }
        //    [DebuggerStepThrough()]
        //    set
        //    {
        //        if (value >= 0)
        //        {
        //            ViewState["percentGreen"] = value;
        //        }
        //    }
        //}

        #endregion

        #region IScriptControl Interface

        /// <summary>
        /// Sets the various properties on the client component
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            ScriptControlDescriptor desc = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.ScreenshotCapture", ClientID);
            desc.AddProperty("height", this.Height.ToString());
            desc.AddProperty("width", this.Width.ToString());

            //Add any custom client-side handlers
            if (this.handlers != null)
            {
                foreach (KeyValuePair<string, string> handler in this.handlers)
                {
                    desc.AddEvent(handler.Key, handler.Value);
                }
            }

            yield return desc;
        }

        /// <summary>
        /// Returns the web service references
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.ScreenshotCapture.js"));
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Add the persistent tooltip javascript
        /// </summary>
        /// <param name="e">The Event Arguments</param>
        protected override void OnPreRender(EventArgs e)
        {
            //First execute the base class
            base.OnPreRender(e);

            //We need to register a client component to go with the server control
            ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);

            if (scriptManager == null)
            {
                throw new InvalidOperationException("ScriptManager required on the page.");
            }

            scriptManager.RegisterScriptControl(this);
        }

        /// <summary>
        /// Renders out the script descriptors
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
            if (!DesignMode)
            {
                ScriptManager.GetCurrent(this.Page).RegisterScriptDescriptors(this);
            }
        }

        /// <summary>
        /// We use a DIV as the ajax element to extend
        /// </summary>
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