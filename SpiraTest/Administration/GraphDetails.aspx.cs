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
using Microsoft.Security.Application;

namespace Inflectra.SpiraTest.Web.Administration
{
    /// <summary>
    /// Displays the admin page for managing custom graphs in the system
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_Graphs", "System-Reporting/#edit-graphs", "Admin_Graphs"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ReportAdmin | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class GraphDetails : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.GraphDetails::";

        protected int graphId;

        /// <summary>
        /// Called when the page is first loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Reset the error messages
            this.lblMessage.Text = "";

            //Get the report id unless blank
            if (String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_GRAPH_ID]))
            {
                //Redirect back to the list page
                Response.Redirect("Graphs.aspx", true);
            }
            else
            {
                if (!Int32.TryParse(Request.QueryString[GlobalFunctions.PARAMETER_GRAPH_ID], out this.graphId))
                {
                    //Redirect back to the list page
                    Response.Redirect("Graphs.aspx", true);
                }
            }

            //Add the event handlers
            this.btnCancel.Click += btnCancel_Click;
            this.btnUpdate.Click += btnUpdate_Click;

            //Load and bind data
            if (!IsPostBack)
            {
                LoadAndBindData();
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Saves the changes to the graph and returns to the list page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnUpdate_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure form validated OK
            if (!IsValid)
            {
                return;
            }

            try
            {
                //Get the report id unless blank
                if (String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_GRAPH_ID]))
                {
                    //Redirect back to the list page
                    Response.Redirect("Graphs.aspx", true);
                }
                else
                {
                    if (!Int32.TryParse(Request.QueryString[GlobalFunctions.PARAMETER_GRAPH_ID], out this.graphId))
                    {
                        //Redirect back to the list page
                        Response.Redirect("Graphs.aspx", true);
                    }
                }

                //Next we need to load the graph from the database
                GraphManager graphManager = new GraphManager();
                GraphCustom graph = graphManager.GraphCustom_RetrieveById(this.graphId);
                if (graph == null)
                {
                    //Redirect back to the list page
                    Response.Redirect("Graphs.aspx", true);
                }

                //Track the changes
                graph.StartTracking();
                graph.Name = this.txtName.Text.Trim();
                if (String.IsNullOrEmpty(this.txtDescription.Text))
                {
                    graph.Description = null;
                }
                else
                {
                    graph.Description = this.txtDescription.Text.Trim();
                }
                graph.Query = this.txtQuery.Text;
                graph.IsActive = this.chkActive.Checked;

                //Update the position if numeric
                if (!String.IsNullOrEmpty(this.txtPosition.Text))
                {
                    int newPosition;
                    if (Int32.TryParse(this.txtPosition.Text, out newPosition) && newPosition > 0)
                    {
                        graph.Position = newPosition;
                    }
                }

                //Save the graph
                graphManager.GraphCustom_Update(graph);

                //Redirect back to the list page
                Response.Redirect("Graphs.aspx", true);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw;
            }
        }

        /// <summary>
        /// Redirects back to the graph list page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("Graphs.aspx", true);
        }

        /// <summary>
        /// Loads the data on the page
        /// </summary>
        protected void LoadAndBindData()
        {
            const string METHOD_NAME = "LoadAndBindData";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Load in the specific graph (include any inactive ones)
                GraphManager graphManager = new GraphManager();

                if (this.graphId > 0)
                {
                    GraphCustom graph = graphManager.GraphCustom_RetrieveById(this.graphId);
                    if (graph == null)
                    {
                        //Return to the graph list page
                        Response.Redirect("Graphs.aspx", true);
                    }

                    //Set the page title
                    ((MasterPages.Administration)(this.Master)).PageTitle = Resources.Main.Admin_Graphs + " | " + Encoder.HtmlAttributeEncode(graph.Name);

                    //Set the page form
                    this.lblGraphName.Text = Encoder.HtmlEncode(graph.Name);
                    this.txtName.Text = graph.Name;
                    this.txtDescription.Text = graph.Description;
                    this.chkActive.Checked = graph.IsActive;
                    this.txtPosition.Text = graph.Position.ToString();
                    this.txtQuery.Text = graph.Query;

                    //Display the list of reportable entities
                    Dictionary<string, string> reportableEntities = ReportManager.GetReportableEntities();
                    this.ddlQuerySelection.DataSource = reportableEntities;
                    this.ddlQuerySelection.DataBind();
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
    }
}
