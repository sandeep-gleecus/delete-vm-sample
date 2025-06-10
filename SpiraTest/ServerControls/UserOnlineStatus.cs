using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// Server control that displays whether a user is online or not
    /// </summary>
    [ToolboxData("<{0}:UserOnlineStatus runat=server></{0}:UserOnlineStatus>")]
    public class UserOnlineStatus : WebControl, IScriptControl
    {
        #region Properties

        [
         Category("Behavior"),
         DefaultValue(null),
         Description("The id of the user that we're checking for")
         ]
        public int? UserId
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["UserId"];

                return (obj == null) ? null : (int?)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["UserId"] = value;
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
            ScriptControlDescriptor desc = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.UserOnlineStatus", ClientID);
            desc.AddProperty("userId", this.UserId);

            yield return desc;
        }

        /// <summary>
        /// Returns the web service references
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.UserOnlineStatus.js"));
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
        /// Render this as a DIV
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
