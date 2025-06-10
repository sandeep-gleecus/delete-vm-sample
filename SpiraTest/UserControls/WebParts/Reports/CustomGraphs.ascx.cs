using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Data;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.Reports
{
    public partial class CustomGraphs : WebPartBase, IWebPartReloadable
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.Reports.CustomGraphs::";

        #region Properties

        /// <summary>
        /// The id of the custom graph
        /// </summary>
        [Personalizable]
        public int? GraphId
        {
            get;
            set;
        }

        /// <summary>
        /// The type of custom graph (bar, line, donut)
        /// </summary>
        [Personalizable]
        public ServerControls.JqPlot.CustomGraphTypeEnum GraphType
        {
            get;
            set;
        }

        /// <summary>
        /// The name of the current graph
        /// </summary>
        public string GraphName
        {
            get;
            set;
        }

        /// <summary>
        /// The description of the current graph
        /// </summary>
        public string GraphDescription
        {
            get;
            set;
        }

        #endregion

        /// <summary>
        /// Returns a handle to the interface
        /// </summary>
        /// <returns>IWebPartReloadable</returns>
        [ConnectionProvider("ReloadableProvider", "ReloadableProvider")]
        public IWebPartReloadable GetReloadable()
        {
            return this;
        }

        /// <summary>
        /// Called when the control is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Needs to set here because the value won't be set if the webpart is added from the catalog
                this.MessageBoxId = "lblMessage";

                //Pass the message box and other context items to the AJAX control
                this.jqCustomGraph.ErrorMessageClientID = this.msgCustomGraph.ClientID;
                this.jqCustomGraph.ProjectId = this.ProjectId;
                this.jqCustomGraph.SelectedGraph = Graph.GraphEnum.None;
                this.jqCustomGraph.CustomGraphId = this.GraphId;
                this.jqCustomGraph.CustomGraphType = this.GraphType;
                this.jqCustomGraph.WebPartUniqueId = this.WebPartUniqueId;
                this.jqCustomGraph.Title = this.Title;

                //Register any client-side event handlers
                this.ddlGraphSelection.ClientScriptMethod = ddlGraphSelection.ClientID + "_selectedItemChanged";
                this.radBarChart.ClientScriptMethod = radBarChart.ClientID + "_click(event)";
                this.radDonutChart.ClientScriptMethod = radDonutChart.ClientID + "_click(event)";
                this.radLineChart.ClientScriptMethod = radLineChart.ClientID + "_click(event)";

                //Now load the content
                if (WebPartVisible)
                {
                    LoadAndBindData();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                //Don't rethrow as this is loaded by an update panel and can't redirect to error page
                if (this.Message != null)
                {
                    this.Message.Text = Resources.Messages.Global_UnableToLoad + " '" + this.Title + "'";
                    this.Message.Type = MessageBox.MessageType.Error;
                }
            }
        }

        /// <summary>
        /// Loads the data in the control
        /// </summary>
        public void LoadAndBindData()
        {
            //Retrieve the list of active custom graphs available
            GraphManager graphManager = new GraphManager();
            List<GraphCustom> graphs = graphManager.GraphCustom_Retrieve();
            this.ddlGraphSelection.DataSource = graphs;
            this.ddlGraphSelection.DataBind();

            //Select the chosen graph
            if (this.GraphId.HasValue)
            {
                GraphCustom currentGraph = graphs.FirstOrDefault(g => g.GraphId == GraphId.Value);
                if (currentGraph == null)
                {
                    //The graph has been deactivated or deleted
                    this.GraphId = null;
                }
                else
                {
                    this.ddlGraphSelection.SelectedValue = this.GraphId.Value.ToString();
                    this.GraphName = currentGraph.Name;
                    this.GraphDescription = currentGraph.Description;
                }
            }

            //Select the type of graph
            switch (GraphType)
            {
                case ServerControls.JqPlot.CustomGraphTypeEnum.Bar:
                    this.radBarChart.Checked = true;
                    this.lblBarChart.CssClass = "btn btn-default active";
                    break;

                case ServerControls.JqPlot.CustomGraphTypeEnum.Line:
                    this.radLineChart.Checked = true;
                    this.lblLineChart.CssClass = "btn btn-default active";
                    break;

                case ServerControls.JqPlot.CustomGraphTypeEnum.Donut:
                    this.radDonutChart.Checked = true;
                    this.lblDonutChart.CssClass = "btn btn-default active";
                    break;
            }
        }
    }
}