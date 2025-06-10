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
using System.Data;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.Reports
{
    /// <summary>
    /// Displays the summary graph web part
    /// </summary>
    public partial class SummaryGraphs : WebPartBase, IWebPartReloadable
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.Reports.SummaryGraphs::";

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
        /// The id of the type of graph
        /// </summary>
        [Personalizable]
        public Graph.GraphEnum GraphId
        {
            get;
            set;
        }

        /// <summary>
        /// Stores the field we're using for the x-axis
        /// </summary>
        [
        Personalizable,
        LocalizedWebDisplayName("Reports_XAxis"),
        LocalizedWebDescription(""),
        DefaultValue("")
        ]
        public string XAxisField
        {
            get
            {
                return this.xAxisField;
            }
            set
            {
                this.xAxisField = value;
                //Force the data to reload
                LoadAndBindData();
            }
        }
        protected string xAxisField = "";

        /// <summary>
        /// Stores the field we're grouping the data by
        /// </summary>
        [
        Personalizable,
        LocalizedWebDisplayName("Reports_GroupedBy"),
        LocalizedWebDescription(""),
        DefaultValue("")
        ]
        public string GroupByField
        {
            get
            {
                return this.groupByField;
            }
            set
            {
                this.groupByField = value;
                //Force the data to reload
                LoadAndBindData();
            }
        }
        protected string groupByField = "";

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
                this.jqSummaryGraph.ErrorMessageClientID = this.MessageBoxClientID;
                this.jqSummaryGraph.ProjectId = this.ProjectId;
                this.jqSummaryGraph.SelectedGraph = GraphId;
                this.jqSummaryGraph.WebPartUniqueId = this.WebPartUniqueId;
                this.jqSummaryGraph.ArtifactType = ArtifactTypeId;
                this.jqSummaryGraph.Title = this.Title;

                //Register any client-side event handlers
                this.ddlXAxis.ClientScriptMethod = ddlXAxis.ClientID + "_selectedItemChanged";
                this.ddlGroupedBy.ClientScriptMethod = ddlGroupedBy.ClientID + "_selectedItemChanged";
                this.btnSwitchValues.ClientScriptMethod = btnSwitchValues.ClientID + "_click(event)";

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
            //Now get the list of artifact fields that you can use for the x-axis or group-by selectors
            GraphManager graphManager = new GraphManager();
            DataSet artifactFieldDataSet = graphManager.RetrieveSummaryChartFields(ProjectId, ProjectTemplateId, ArtifactTypeId);
            this.ddlXAxis.DataSource = artifactFieldDataSet;
            this.ddlXAxis.DataBind();
            this.ddlGroupedBy.DataSource = artifactFieldDataSet;
            this.ddlGroupedBy.DataBind();

            //The x-axis and group-by fields are stored in WebPart settings
            if (!String.IsNullOrEmpty(XAxisField))
            {
                try
                {
                    this.ddlXAxis.SelectedValue = XAxisField;
                    this.jqSummaryGraph.XAxisField = XAxisField;
                }
                catch (ArgumentOutOfRangeException)
                {
                    //In case the graph and artifact type id don't match
                }
            }
            if (!String.IsNullOrEmpty(GroupByField))
            {
                try
                {
                    this.ddlGroupedBy.SelectedValue = GroupByField;
                    this.jqSummaryGraph.GroupByField = GroupByField;
                }
                catch (ArgumentOutOfRangeException)
                {
                    //In case the graph and artifact type id don't match
                }
            }
        }
    }
}