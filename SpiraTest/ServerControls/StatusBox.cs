using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// Displays an AJAX enabled status box that contains a label, hyperlink and background color that is set by CSS class
    /// </summary>
    /// <remarks>Currently used by the Execution status indicator on the test case and test run details pages</remarks>
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:StatusBox runat=server></{0}:StatusBox>")]
    public class StatusBox : WebControl, IScriptControl
    {
        #region Properties

        /// <summary>
        /// The text to display
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Text
        {
            get
            {
                String s = (String)ViewState["Text"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["Text"] = value;
            }
        }

        /// <summary>
        /// The css class to use for displaying the status
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string DataCssClass
        {
            get
            {
                String s = (String)ViewState["DataCssClass"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["DataCssClass"] = value;
            }
        }

        /// <summary>
        /// The URL to use if we have a hyperlink
        /// </summary>
        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("")]
        [Localizable(true)]
        public string NavigateUrl
        {
            get
            {
                String s = (String)ViewState["NavigateUrl"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["NavigateUrl"] = value;
            }
        }

        #endregion

        #region IScriptControl Interface

        /// <summary>
        /// Sets the various properties on the client component
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            ScriptControlDescriptor desc = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.StatusBox", ClientID);
            desc.AddProperty("text", this.Text);
            desc.AddProperty("navigateUrl", this.NavigateUrl);
            desc.AddProperty("cssClass", this.CssClass);
            desc.AddProperty("dataCssClass", this.DataCssClass);
            yield return desc;
        }

        /// <summary>
        /// Returns the web service references
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.StatusBox.js"));
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
        /// We need to render this control as a DIV
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
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

        #endregion
    }
}
