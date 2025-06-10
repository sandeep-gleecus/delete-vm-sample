using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Microsoft.Security.Application;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.Administration.Project
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// project to project associations screens
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_ProjectAssociations", "Product-General-Settings/#product-associations", "Admin_ProjectAssociations"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.ProjectOwner | AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class ProjectAssociations : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.ProjectAssociations::";

        /// <summary>
        /// Loads the page content
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Redirect if there's no project selected.
            if (ProjectId < 1)
                Response.Redirect("Default.aspx?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.Admin_SelectProject, true);

            //Register event handlers
            this.btnRemove.Click += btnRemove_Click;
            this.grdLinkedProjects.RowDataBound += grdLinkedProjects_RowDataBound;
            this.btnSaveAssociation.Click += btnSaveAssociation_Click;
            this.btnAddAssociation.Click += btnAddAssociation_Click;

            if (!IsPostBack)
            {
                //Clear any messages
                this.lblMessage.Text = "";

                //Set the project name and url
                this.lblProjectName.Text = Microsoft.Security.Application.Encoder.HtmlEncode(ProjectName);
                this.lnkAdminHome.NavigateUrl = UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "Default");

                LoadAndBindData();
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Adds a new project association
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnAddAssociation_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnAddAssociation_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
            try
            {
                //Loop through the rows and find which ones were checked
                ProjectManager projectManager = new ProjectManager();
                int sourceProjectId = ProjectId; //The current project is the source

                //Get the project id and artifact type ids
                if (String.IsNullOrEmpty(this.ddlNewProject.SelectedValue))
                {
                    this.lblMessage.Text = Resources.Messages.Admin_ProjectAssociations_ProjectRequired;
                    this.lblMessage.Type = MessageBox.MessageType.Error;
                    return;
                }
                List<uint> selectedValues = this.ddlArtifactTypesNew.SelectedValues(true);
                if (selectedValues.Count == 0)
                {
                    this.lblMessage.Text = Resources.Messages.Admin_ProjectAssociations_ArtifactTypesRequired;
                    this.lblMessage.Type = MessageBox.MessageType.Error;
                    return;
                }
                int destProjectId = Int32.Parse(this.ddlNewProject.SelectedValue);
                List<int> artifactTypeIds = selectedValues.Select(a => (int)a).ToList();
                projectManager.ProjectAssociation_Add(sourceProjectId, destProjectId, artifactTypeIds);

                //Reload the grid
                LoadAndBindData();

                //Display the success message
                this.lblMessage.Text = Resources.Messages.Admin_ProjectAssociations_AddSuccess;
                this.lblMessage.Type = MessageBox.MessageType.Success;

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                this.lblMessage.Text = exception.Message;
                this.lblMessage.Type = MessageBox.MessageType.Error;
            }
        }

        /// <summary>
        /// Updates an existing project association
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnSaveAssociation_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnSaveAssociation_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
            try
            {
                //Loop through the rows and find which ones were checked
                ProjectManager projectManager = new ProjectManager();
                int sourceProjectId = ProjectId; //The current project is the source

                //Get the project id and artifact type ids
                int destProjectId = Int32.Parse(this.hdnDestProjectId.Value);
                List<uint> selectedValues = this.ddlArtifactTypesEdit.SelectedValues(true);
                if (selectedValues.Count == 0)
                {
                    this.lblMessage.Text = Resources.Messages.Admin_ProjectAssociations_ArtifactTypesRequired;
                    this.lblMessage.Type = MessageBox.MessageType.Error;
                    return;
                }
                List<int> artifactTypeIds = selectedValues.Select(a => (int)a).ToList();
                projectManager.ProjectAssociation_Update(sourceProjectId, destProjectId, artifactTypeIds);

                //Reload the grid
                LoadAndBindData();

                //Display the success message
                this.lblMessage.Text = Resources.Messages.Admin_ProjectAssociations_SaveSuccess;
                this.lblMessage.Type = MessageBox.MessageType.Success;

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                this.lblMessage.Text = exception.Message;
                this.lblMessage.Type = MessageBox.MessageType.Error;
            }
        }

        /// <summary>
        /// Adds the data-* attributes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdLinkedProjects_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //Add the data-* attributes to the button
            //data-project-id="<%#((IGrouping<Project, ProjectArtifactSharing>)Container.DataItem).Key.ProjectId %>"]
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //We populate the project id, name and the list of artifact ids
                IGrouping<DataModel.Project, ProjectArtifactSharing> dataRow = (IGrouping<DataModel.Project, ProjectArtifactSharing>)e.Row.DataItem;
                HyperLinkEx lnkEdit = (HyperLinkEx)e.Row.FindControl("lnkEdit");
                lnkEdit.Attributes["data-project-id"] = dataRow.Key.ProjectId.ToString();
                lnkEdit.Attributes["data-project-name"] = Encoder.HtmlAttributeEncode(((IGrouping<DataModel.Project, ProjectArtifactSharing>)e.Row.DataItem).Key.Name);

                string listOfArtifactIds = dataRow.Select(d => d.ArtifactTypeId).ToList().ToDatabaseSerialization();
                lnkEdit.Attributes["data-artifact-ids"] = listOfArtifactIds;
            }
        }

        /// <summary>
        /// Removes the association from the project
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnRemove_Click(object sender, DropMenuEventArgs e)
        {
            const string METHOD_NAME = "btnRemove_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
            try
            {
                //Loop through the rows and find which ones were checked
                ProjectManager projectManager = new ProjectManager();
                int sourceProjectId = ProjectId; //The current project is the source
                foreach (GridViewRow gvr in this.grdLinkedProjects.Rows)
                {
                    if (gvr.RowType == DataControlRowType.DataRow)
                    {
                        //Find the checkbox and make sure it checked
                        CheckBoxEx chkDeleteAssociation = (CheckBoxEx)gvr.FindControl("chkDeleteAssociation");
                        if (chkDeleteAssociation != null && chkDeleteAssociation.Checked && chkDeleteAssociation.MetaData != null)
                        {
                            int destProjectId;
                            if (Int32.TryParse(chkDeleteAssociation.MetaData, out destProjectId))
                            {
                                //Delete this project association
                                projectManager.ProjectAssociation_Delete(sourceProjectId, destProjectId);
                            }
                        }
                    }
                }

                //Reload the grid
                LoadAndBindData();
                
                //Display the success message
                this.lblMessage.Text = Resources.Messages.Admin_ProjectAssociations_RemoveSuccess;
                this.lblMessage.Type = MessageBox.MessageType.Success;

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                this.lblMessage.Text = exception.Message;
                this.lblMessage.Type = MessageBox.MessageType.Error;
            }
        }

        /// <summary>
        /// Loads the page content
        /// </summary>
        protected void LoadAndBindData()
        {
            const string METHOD_NAME = "LoadAndBindData";

            try
            { 
                //Get the list of currently shared projects/artifacts
                ProjectManager projectManager = new ProjectManager();
                List<ProjectArtifactSharing> projectArtifactsSharing = projectManager.ProjectAssociation_RetrieveForProject(ProjectId);

                //Group by project
                List<IGrouping<DataModel.Project, ProjectArtifactSharing>> destinationProjects = projectArtifactsSharing.GroupBy(p => p.DestProject).ToList();
                this.grdLinkedProjects.DataSource = destinationProjects;
                this.grdLinkedProjects.DataBind();

                //Load the list of new projects, excluding the current one and any currently associated
                List<ProjectView> projects = projectManager.Retrieve().Where(p => p.ProjectId != ProjectId && !projectArtifactsSharing.Any(x => x.DestProjectId == p.ProjectId)).ToList();
                this.ddlNewProject.DataSource = projects;
                this.ddlNewProject.DataBind();

                //Load the list of artifact types, exclude placeholders and other artifacts based on what has been designed for as of 2019-01 (see KB329)
                //The list should be: requirements, incidents, and test cases. It seems to work for tasks too, so these have been left in here
                List<ArtifactType> artifactTypes = new ArtifactManager().ArtifactType_RetrieveAll(false, false, false, false, false).Where (
                    a => a.ArtifactTypeId != (int)Artifact.ArtifactTypeEnum.Placeholder &&
                    a.ArtifactTypeId != (int)Artifact.ArtifactTypeEnum.TestRun &&
                    a.ArtifactTypeId != (int)Artifact.ArtifactTypeEnum.Release &&
                    a.ArtifactTypeId != (int)Artifact.ArtifactTypeEnum.Document &&
                    a.ArtifactTypeId != (int)Artifact.ArtifactTypeEnum.TestSet &&
                    a.ArtifactTypeId != (int)Artifact.ArtifactTypeEnum.AutomationHost &&
                    a.ArtifactTypeId != (int)Artifact.ArtifactTypeEnum.RiskMitigation &&
                    a.ArtifactTypeId != (int)Artifact.ArtifactTypeEnum.RequirementStep &&
                    a.ArtifactTypeId != (int)Artifact.ArtifactTypeEnum.TestStep
                ).ToList();
                this.ddlArtifactTypesEdit.DataSource = artifactTypes;
                this.ddlArtifactTypesEdit.DataBind();
                this.ddlArtifactTypesNew.DataSource = artifactTypes;
                this.ddlArtifactTypesNew.DataBind();
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                this.lblMessage.Text = exception.Message;
                this.lblMessage.Type = MessageBox.MessageType.Error;
            }
        }
    }
}