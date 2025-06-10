using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using System.Web.UI.WebControls.WebParts;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome
{
    /// <summary>
    /// Displays the task graphs web part
    /// </summary>
    public partial class TaskGraphs : WebPartBase, IWebPartReloadable
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.TaskGraphs::";

        #region Properties

        /// <summary>
        /// Stores the currently selected graph
        /// </summary>
        [
        Personalizable,
        LocalizedWebDisplayName("Global_SelectedGraph"),
        LocalizedWebDescription(""),
        DefaultValue(Graph.GraphEnum.None)
        ]
        public Graph.GraphEnum SelectedGraph
        {
            get
            {
                return this.selectedGraph;
            }
            set
            {
                this.selectedGraph = value;
                //Force the data to reload
                LoadAndBindData();
            }
        }
        protected Graph.GraphEnum selectedGraph = Graph.GraphEnum.None;

        #endregion

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
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
                this.jqSnapshotGraph.ErrorMessageClientID = this.MessageBoxClientID;
                this.jqSnapshotGraph.ProjectId = this.ProjectId;
                this.jqSnapshotGraph.WebPartUniqueId = this.WebPartUniqueId;

                //Register any client-side event handlers
                this.ddlGraphFilter.ClientScriptMethod = ddlGraphFilter.ClientID + "_selectedItemChanged";

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
        /// Returns a handle to the interface
        /// </summary>
        /// <returns>IWebPartReloadable</returns>
        [ConnectionProvider("ReloadableProvider", "ReloadableProvider")]
        public IWebPartReloadable GetReloadable()
        {
            return this;
        }

        /// <summary>
        /// Loads the data in the control
        /// </summary>
        public void LoadAndBindData()
        {
            //Display the list of graphs
            GraphManager graphManager = new GraphManager();
            List<Graph> graphs = graphManager.RetrieveByType(Graph.GraphTypeEnum.SnapshotGraphs, DataModel.Artifact.ArtifactTypeEnum.Task);
            this.ddlGraphFilter.DataSource = graphs;
            this.ddlGraphFilter.DataBind();

            //Now set the release as a filter
            int releaseId = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_PROJECT_HOME_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_SELECTED_RELEASE_ID, -1);
            Dictionary<string, object> filters = new Dictionary<string, object>();
            if (releaseId != -1)
            {
                filters["ReleaseId"] = releaseId;
            }
            jqSnapshotGraph.SetFilters(filters);
   
            //The date-range and graph type are stored in the WebPart settings for this widget
            if (SelectedGraph != Graph.GraphEnum.None)
            {
                try
                {
                    this.ddlGraphFilter.SelectedValue = ((int)SelectedGraph).ToString();
                }
                catch (ArgumentOutOfRangeException)
                {
                    //In case the graph and artifact type id don't match
                }
            }
            Graph currentRow = graphs.FirstOrDefault(g => g.GraphId == (int)SelectedGraph);
            if (currentRow == null)
            {
                //If no item selected, just use the first graph in the list
                Graph firstGraph = graphs.First();
                this.jqSnapshotGraph.SelectedGraph = (Graph.GraphEnum)firstGraph.GraphId;
            }
            else
            {
                this.jqSnapshotGraph.SelectedGraph = SelectedGraph;
            }
        }
    }
}