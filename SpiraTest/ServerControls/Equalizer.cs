using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// Displays the 'equalizer' type graph used to display test status
    /// </summary>
    [ToolboxData("<{0}:Equalizer runat=server></{0}:Equalizer>")]
    public class Equalizer : WebControl, IScriptControl
    {
        #region Properties

        [
        Category("Data"),
        DefaultValue(""),
        Description("The % green to display")
        ]
        public int PercentGreen
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["percentGreen"];

                return (obj == null) ? 0 : (int)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                if (value >= 0)
                {
                    ViewState["percentGreen"] = value;
                }
            }
        }

        [
        Category("Data"),
        DefaultValue(""),
        Description("The % red to display")
        ]
        public int PercentRed
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["percentRed"];

                return (obj == null) ? 0 : (int)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                if (value >= 0)
                {
                    ViewState["percentRed"] = value;
                }
            }
        }

        [
        Category("Data"),
        DefaultValue(""),
        Description("The % orange to display")
        ]
        public int PercentOrange
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["percentOrange"];

                return (obj == null) ? 0 : (int)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                if (value >= 0)
                {
                    ViewState["percentOrange"] = value;
                }
            }
        }

        [
        Category("Data"),
        DefaultValue(""),
        Description("The % yellow to display")
        ]
        public int PercentYellow
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["percentYellow"];

                return (obj == null) ? 0 : (int)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                if (value >= 0)
                {
                    ViewState["percentYellow"] = value;
                }
            }
        }

        [
        Category("Data"),
        DefaultValue(""),
        Description("The % gray to display")
        ]
        public int PercentGray
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["percentGray"];

                return (obj == null) ? 0 : (int)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                if (value > 0)
                {
                    ViewState["percentGray"] = value;
                }
            }
        }

        [
        Category("Data"),
        DefaultValue(""),
        Description("The % dark gray to display")
        ]
        public int PercentDarkGray
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["percentDarkGray"];

                return (obj == null) ? 0 : (int)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                if (value > 0)
                {
                    ViewState["percentDarkGray"] = value;
                }
            }
        }

        [
        Category("Data"),
        DefaultValue(""),
        Description("The % light blue to display")
        ]
        public int PercentBlue
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["percentBlue"];

                return (obj == null) ? 0 : (int)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                if (value > 0)
                {
                    ViewState["percentBlue"] = value;
                }
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
            ScriptControlDescriptor desc = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.Equalizer", ClientID);
            desc.AddProperty("percentBlue", this.PercentBlue);
            desc.AddProperty("percentGray", this.PercentGray);
            desc.AddProperty("percentGreen", this.PercentGreen);
            desc.AddProperty("percentOrange", this.PercentOrange);
            desc.AddProperty("percentRed", this.PercentRed);
            desc.AddProperty("percentYellow", this.PercentYellow);
            desc.AddProperty("percentDarkGray", this.PercentDarkGray);

            //Get the tooltip then remove from control
            if (!String.IsNullOrEmpty(this.ToolTip))
            {
                desc.AddProperty("toolTip", this.ToolTip);
                this.ToolTip = "";
            }

            //Need to add additional tags in the case of Firefox which doesn't support inline SPAN widths
            bool isFirefox = false;
            if (Page.Request.Browser.Browser.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "mozilla" || Page.Request.Browser.Browser.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "firefox" || Page.Request.Browser.Browser.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "netscape")
            {
                isFirefox = true;
            }
            desc.AddProperty("isFirefox", isFirefox);

            yield return desc;
        }

        /// <summary>
        /// Returns the web service references
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.Equalizer.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.Tooltip.js"));
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

        #endregion
    }
}
