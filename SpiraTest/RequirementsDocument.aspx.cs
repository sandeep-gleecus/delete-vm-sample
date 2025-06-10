using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.MasterPages;
using Inflectra.SpiraTest.DataModel;
using System.Linq;
using License = Inflectra.SpiraTest.Common.License;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// Requirements Matrix and handling all raised events
	/// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Requirements, "SiteMap_RequirementsDocument", "Requirements-Management/#requirement-document-view")]
	public partial class RequirementsDocument : PageLayout
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.RequirementsDocument::";

		public string ShowOutlineCode = "false";
		public int? ParentRequirementId = null;
		
		#region Event Handlers

		/// <summary>
		/// This sets up the page upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent (CLASS_NAME + METHOD_NAME);
            
            //We need to make sure that the system is licensed for planning boards (SpiraTeam or SpiraPlan only)
            if (License.LicenseProductName == LicenseProductNameEnum.SpiraTest)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.RequirementsFeature_InvalidLicense, true);
            }

            //Redirect if the user has limited view permissions for this artifact type. Grant access to sys admins if they are a member of the project in any way
            //First check if a user is an admin (and product member). If so give them a role of 1 - ie "Product Owner"
            int projectRoleIdToCheck = ProjectRoleId;
            if (UserIsAdmin && ProjectRoleId > 0)
            {
                projectRoleIdToCheck = ProjectManager.ProjectRoleProjectOwner;
            }
            if (new Business.ProjectManager().IsAuthorized(projectRoleIdToCheck, Artifact.ArtifactTypeEnum.Requirement, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized && !(UserIsAdmin && ProjectRoleId > 0))
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.GlobalNavigation_NotHavePermissionToView, true);
            }

            //Update the setting that we're using the document view if it's not already set
            string listPage = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE, "");
            if (listPage != "Document")
            {
                SaveProjectSetting(GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_GENERAL_SETTINGS, GlobalFunctions.PROJECT_SETTINGS_KEY_HOME_PAGE, "Document");
            }

            //Specify the context for the quick-filters sidebar
            this.pnlNavigation.ProjectId = ProjectId;
            this.pnlNavigation.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.pnlNavigation.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 300));

            //Load the list of packages/epics
            RequirementManager requirementManager = new RequirementManager();
            List<RequirementView> packages = requirementManager.Requirement_RetrieveSummaryBacklog(ProjectId);

			this.lstPackages.DataSource = packages.Take(5000);
            this.lstPackages.DataBind();

            //Only load the data once
            if (!IsPostBack) 
			{
                //Databind the list/board selector
                this.plcListBoardSelector.DataBind();

				//Get the project collection
				ProjectSettingsCollection documentSettings = GetProjectSettings(GlobalFunctions.PROJECT_SETTINGS_REQUIREMENT_DOCUMENT_VIEW);

				//Get the list of fields in the show/hide columns list
				//Note: this method also sets the variable on the page for handling display of outline codes for requirements (eg 1.1.2.1) - this allows React to access this value
				this.ddlShowHideColumns.DataSource = CreateShowHideFieldList(documentSettings);
				this.ddlShowHideColumns.DataBind();

				//If we have a parent requirement id to show children for, set the public string here for page to access
				bool hasParentRequirementId = documentSettings.ContainsKey(GlobalFunctions.PROJECT_SETTINGS_KEY_PARENT_REQUIREMENT_ID);
				if (hasParentRequirementId)
				{
					ParentRequirementId = (int)documentSettings[GlobalFunctions.PROJECT_SETTINGS_KEY_PARENT_REQUIREMENT_ID];
				}

				//This is used to prevent ENTER firing the server events, since we want the grid to handle these client-side
				this.Form.DefaultButton = this.btnEnterCatch.UniqueID;
                this.btnEnterCatch.Attributes.Add("onclick", "return false;");
                this.btnEnterCatch.Style.Add(HtmlTextWriterStyle.Display, "none");
			}

            //Reset the error message
            this.lblMessage.Text = "";

            Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		#endregion

		#region Internal Methods
		/// <summary>
		/// Gets the list of values to display in the show/hide columns dropdown lists
		/// <param name="documentSettings">The project settings collection for the requirements document view</param>
		/// </summary>
		/// <returns></returns>
		private Dictionary<string, string> CreateShowHideFieldList(ProjectSettingsCollection documentSettings)
		{
			Dictionary<string, string> showHideList = new Dictionary<string, string>();

			//Need to get the non-localized list from the database
			ArtifactManager artifactManager = new ArtifactManager();
			List<ArtifactListFieldDisplay> artifactFields = artifactManager.ArtifactField_RetrieveForLists(ProjectId, UserId, Artifact.ArtifactTypeEnum.Requirement);


			//If the collection is empty add the entries for the default fields because we need it later
			if (documentSettings.Count == 0)
			{
				//Add the defaults fields to show
				documentSettings.Add("Description", 1);
				documentSettings.Add("ImportanceId", 1);
				documentSettings.Add("RequirementStatusId", 1);
				documentSettings.Add("RequirementTypeId", 1);
				documentSettings.Add("OwnerId", 1);
				documentSettings.Save();
			}

			//This is the selected list of standard fields avaiable on this page
			List<string> standardFields = new List<string>()
			{
				//Default visible standard fields
				"Description",
				"ImportanceId",
				"RequirementStatusId",
				"RequirementTypeId",
				"OwnerId",

				//Optional standard fields
				"AuthorId",
				"ComponentId",
				"ReleaseId",
				"CoverageId",
				"ProgressId",
				"EstimatePoints",
				"LastUpdateDate",
				"CreationDate"
			};

			//Filter artifact fields to the standard fields we specifically allow users to display on this page
			List<ArtifactListFieldDisplay> fieldsToDisplay = artifactFields.FindAll(field => standardFields.Contains(field.Name));
			
			//Loop through each standard field to localize it
			foreach (ArtifactListFieldDisplay field in fieldsToDisplay)
			{
				//See if we can localize the field name or not
				string localizedName = Resources.Fields.ResourceManager.GetString(field.Name);
				if (!String.IsNullOrEmpty(localizedName))
				{
					field.Caption = localizedName;
				}
			}

			//CUSTOM FIELDS
			//Get the template associated with the project
			int projectTemplateId = new TemplateManager().RetrieveForProject(ProjectId).ProjectTemplateId;
			//Pull the custom property definitions.. and find those that are in our list field display (of type text)
			List<CustomProperty> customProperties = new CustomPropertyManager().CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, false);
			List<ArtifactListFieldDisplay> customTextFields = new List<ArtifactListFieldDisplay>();
			//Loop through each custom property of type text
			foreach (CustomProperty customProperty in customProperties)
			{
				//get only those where the rich text option is set to false
				CustomPropertyOptionValue richTextOption = customProperty.Options.Where(cpo => cpo.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.RichText).FirstOrDefault();

				bool isRichText = richTextOption != null && richTextOption.Value == "Y";

				//Add the rich text custom property
				if (isRichText)
				{
					ArtifactListFieldDisplay customTextField = new ArtifactListFieldDisplay()
					{
						Caption = customProperty.Name,
						Name = customProperty.CustomPropertyFieldName,
						IsVisible = true,
						ArtifactFieldTypeId = (int)DataModel.Artifact.ArtifactFieldTypeEnum.Html
					};
					customTextFields.Add(customTextField);
				}
			}

			//Add the outline code as it is not a standard field but we want to show it in the same dropdown
			ArtifactListFieldDisplay outlineCode = new ArtifactListFieldDisplay()
			{
				Name = "OutlineCode",
				Caption = Resources.ClientScript.HierarchicalDocument_OutlineCode
			};

			//COMBINE ALL FIELDS
			//Combine the standard and custom field list and order it A-Z
			fieldsToDisplay.AddRange(customTextFields);
			fieldsToDisplay.Add(outlineCode);
			fieldsToDisplay = fieldsToDisplay.OrderBy(field => field.Caption).ToList();

			//Loop through all the fields to prep for the dropdown
			foreach (ArtifactListFieldDisplay field in fieldsToDisplay)
			{
				bool showField = documentSettings.ContainsKey(field.Name);
				string legend = (showField ? Resources.Dialogs.Global_Hide : Resources.Dialogs.Global_Show) + " " + field.Caption;
				showHideList.Add(field.Name, legend);			
			}

			//If should show the outline codes, set the public string here for page to access
			//We can set this elsewhere as this but doing here because we deal with the outline code dropdown item in the code above
			bool outlineCodeShow = documentSettings.ContainsKey("OutlineCode");
			if (outlineCodeShow)
			{
				ShowOutlineCode = "true";
			}

			return showHideList;
		}
		#endregion
	}
}
