using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;

namespace Inflectra.SpiraTest.Web.Administration
{
    /// <summary>
    /// Displays the admin page for managing custom graphs in the system
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_Graphs", "System-Reporting/#edit-graphs", "Admin_Graphs"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ReportAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class Graphs : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.Graphs::";

        /// <summary>
        /// Loads the custom graph list page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Reset the error messages
            this.lblMessage.Text = "";

            //Add the event handlers
            this.grdGraphs.RowCommand += new GridViewCommandEventHandler(grdGraphs_RowCommand);

            //Load and bind data
            if (!IsPostBack)
            {
                LoadAndBindData();
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Called when a button in the grid is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdGraphs_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            const string METHOD_NAME = "grdGraphs_RowCommand";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //See which command was clicked
                if (e.CommandName == "CloneGraph")
                {
                    int graphId = Int32.Parse((string)e.CommandArgument);
                    new GraphManager().GraphCustom_Clone(graphId);
                    LoadAndBindData();
                    this.lblMessage.Text = Resources.Messages.Admin_Graphs_GraphCloned;
                    this.lblMessage.Type = MessageBox.MessageType.Information;
                }
                if (e.CommandName == "DeleteGraph")
                {
                    int graphId = Int32.Parse((string)e.CommandArgument);
                    new GraphManager().GraphCustom_Delete(graphId);
                    LoadAndBindData();
                    this.lblMessage.Text = Resources.Messages.Admin_Graphs_GraphDeleted;
                    this.lblMessage.Type = MessageBox.MessageType.Information;
                }
                if (e.CommandName == "AddGraph")
                {
                    //Redirect to the new graph
                    int graphId = new GraphManager().GraphCustom_Create(Resources.Main.Graphs_NewGraph, "");
                    Response.Redirect("GraphDetails.aspx?" + GlobalFunctions.PARAMETER_GRAPH_ID + "=" + graphId);
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Loads the report list
        /// </summary>
        protected void LoadAndBindData()
        {
            const string METHOD_NAME = "LoadAndBindData";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Load in all the custom graphs
                GraphManager graphManager = new GraphManager();
                List<GraphCustom> customGraphs = graphManager.GraphCustom_Retrieve(false);

                this.grdGraphs.DataSource = customGraphs;
                this.grdGraphs.DataBind();

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        protected void Unnamed_PreRender(object sender, EventArgs e)
        {

        }
    }
}
