using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Web.UI.WebControls.WebParts;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// Extends the built-in WebPartManager to handle the case where the web parts are enclosed in an AJAX update panel
    /// and to support non-IE browsers
    /// </summary>
    [ToolboxData("<{0}:WebPartManagerEx runat=server></{0}:WebPartManagerEx>")]
    public class WebPartManagerEx : WebPartManager, IScriptControl
    {
        private static readonly object WebPartDrillDownEvent = new object();

        #region Properties

        /// <summary>
        /// Stores the logical name of the dashboard that the web part manager needs to store the settings for (e.g. MyPage)
        /// </summary>
        /// <remarks>Unlike the Microsoft built-in manager we don't want to store settings against physical URL paths</remarks>
        [
        Bindable(true),
        Category("Data"),
        Description("Stores the logical name of the dashboard that the web part manager needs to store the settings for"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public string DashboardName
        {
            get
            {
                if (ViewState["dashboardName"] == null)
                {
                    return "";
                }
                else
                {
                    return (string)ViewState["dashboardName"];
                }
            }
            set
            {
                ViewState["dashboardName"] = value;
            }
        }

        /// <summary>
        /// Stores the instance identifier of the dashboard if the dashboard can exist for different entities (projects, project groups, etc.)
        /// </summary>
        /// <remarks>We don't bother with Viewstate as this needs to be set at the Init stage anyway</remarks>
        [
        Bindable(true),
        Category("Data"),
        Description("Stores the logical name of the dashboard that the web part manager needs to store the settings for"),
        DefaultValue(-1),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public int DashboardInstanceId
        {
            get
            {
                return this.dashboardInstanceId;
            }
            set
            {
                this.dashboardInstanceId = value;
            }
        }
        protected int dashboardInstanceId = -1;

        #endregion

        protected override void RegisterClientScript()
        {
            //Do nothing as the client script is already rendered by the other methods
        }

        /// <summary>
        /// Renders the web part manager control
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.RenderEndTag();
            ScriptManager current = ScriptManager.GetCurrent(this.Page);
            if (current != null)
            {
                current.RegisterScriptDescriptors(this);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);

            if (scriptManager == null)
            {
                throw new InvalidOperationException("ScriptManager required on the page.");
            }

            scriptManager.RegisterScriptControl(this);
        }

        /// <summary>
        /// Overrides the default implementation since we now support all browsers
        /// </summary>
        /// <returns></returns>
        protected override bool CheckRenderClientScript()
        {
            return this.EnableClientScript;
        }

        /// <summary>
        /// Creates the ajax control and sets any properties/events on it
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            ScriptControlDescriptor desc = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.WebParts.WebPartManager", ClientID);
            desc.AddProperty("allowPageDesign", this.DisplayMode.AllowPageDesign);
            yield return desc;
        }

        /// <summary>
        /// Returns the list of script files that need to be included
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DragDrop.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.WebParts.js"));
        }

        /// <summary>
        /// Called to raise the drilldown event
        /// </summary>
        /// <param name="webPart">The web part</param>
        public void DrillDownWebPart(WebPart webPart)
        {
            this.OnWebPartDrillDown(new WebPartEventArgs(webPart));
        }

        /// <summary>
        /// The event delegate
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected virtual void OnWebPartDrillDown(WebPartEventArgs e)
        {
            WebPartEventHandler handler = (WebPartEventHandler)base.Events[WebPartDrillDownEvent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// The event that's called when you click on the drilldown link for a webpart
        /// </summary>
        public event WebPartEventHandler WebPartDrillDown
        {
            add
            {
                base.Events.AddHandler(WebPartDrillDownEvent, value);
            }
            remove
            {
                base.Events.RemoveHandler(WebPartDrillDownEvent, value);
            }
        }

        /// <summary>
        /// Gets the special 'path' for the dashboard used by SpiraTest instead of the physical ASP.NET path
        /// </summary>
        /// <returns></returns>
        public string GetPath()
        {
            if (this.DashboardInstanceId == -1)
            {
                return this.DashboardName;
            }
            else
            {
                return this.DashboardName + "/" + this.DashboardInstanceId.ToString();
            }
        }
    }
}
