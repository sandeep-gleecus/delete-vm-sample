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

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.Reports
{
    /// <summary>
    /// Displays the snapshot graphs web part
    /// </summary>
    public partial class SnapshotGraphs : WebPartBase, IWebPartReloadable
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.Reports.SnapshotGraphs::";

        #region Properties

        /// <summary>
        /// The Artifact type the report contains data for
        /// </summary>
        [Personalizable]
        public DataModel.Artifact.ArtifactTypeEnum ArtifactTypeId
        {
            get;
            set;
        }

        /// <summary>
        /// The incident type the report contains data for (if artifact type = incidents)
        /// </summary>
        [Personalizable]
        public int? IncidentTypeId
        {
            get;
            set;
        }

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

                //Need to see if the user is authorized to view the specified artifact id
                //if not, we mustn't display the web part
                ProjectManager projectManager = new ProjectManager();
                if (projectManager.IsAuthorized(ProjectRoleId, ArtifactTypeId, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized)
                {
                    //We can't actually hide it, so just minimize
                    ChromeState = PartChromeState.Minimized;
                    return;
                }

                //Pass the message box and other context items to the AJAX control
                this.jqSnapshotGraph.ErrorMessageClientID = this.MessageBoxClientID;
                this.jqSnapshotGraph.ProjectId = this.ProjectId;
                this.jqSnapshotGraph.WebPartUniqueId = this.WebPartUniqueId;

                //Register any client-side event handlers
                this.ddlGraphFilter.ClientScriptMethod = ddlGraphFilter.ClientID + "_selectedItemChanged";
                this.ddlIncidentType.ClientScriptMethod = ddlIncidentType.ClientID + "_selectedItemChanged";

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
            List<Graph> graphs = graphManager.RetrieveByType(Graph.GraphTypeEnum.SnapshotGraphs, ArtifactTypeId);
            this.ddlGraphFilter.DataSource = graphs;
            this.ddlGraphFilter.DataBind();

            //Depending on the artifact type, display the appropriate filters
            if (ArtifactTypeId == DataModel.Artifact.ArtifactTypeEnum.Incident)
            {
                this.plcIncidentTypeFilter.Visible = true;
                List<IncidentType> incidentTypes = new IncidentManager().RetrieveIncidentTypes(ProjectTemplateId, true);
                this.ddlIncidentType.DataSource = incidentTypes;
                this.ddlIncidentType.DataBind();

                //The incident type is stored in the WebPart Settings
                if (IncidentTypeId.HasValue)
                {
                    try
                    {
                        this.ddlIncidentType.SelectedValue = IncidentTypeId.Value.ToString();
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        //In case the graph and artifact type id don't match
                    }

                    //Now set the type as a filter
                    Dictionary<string, object> filters = new Dictionary<string, object>();
                    filters["IncidentTypeId"] = IncidentTypeId.Value;
                    jqSnapshotGraph.SetFilters(filters);
                }
            }
            else
            {
                this.plcIncidentTypeFilter.Visible = false;
            }

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
                this.jqSnapshotGraph.Title = firstGraph.Name;
                this.jqSnapshotGraph.SelectedGraph = (Graph.GraphEnum)firstGraph.GraphId;
            }
            else
            {
                this.jqSnapshotGraph.Title = currentRow.Name;
                this.jqSnapshotGraph.SelectedGraph = SelectedGraph;
            }
        }
    }
}