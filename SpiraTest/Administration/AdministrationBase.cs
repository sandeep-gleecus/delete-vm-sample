using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Attributes;
using System;

#pragma warning disable 1591

namespace Inflectra.SpiraTest.Web.Administration
{
	/// <summary>
	/// Base class for all administration pages
	/// </summary>
	public class AdministrationBase : PageLayout
	{
		private const string CLASS_NAME = "Web.Administration.AdministrationBase::";

		/// <summary>
		/// Redirects back to the default administration page if the user does not have the appropriate permissions
		/// </summary>
		/// <param name="e"></param>
		/// <remarks>
		/// You cannot set an AdministrationLevel attribute on the default page otherwise you'll get into an endless loop
		/// </remarks>
		protected override void OnLoad(EventArgs e)
		{
			//First call the base class (which will ensure that the session variables are set first)
			base.OnLoad(e);

			//Now get the administration level attribute associate with this page
			AdministrationLevelAttribute adminLevel = (AdministrationLevelAttribute)Attribute.GetCustomAttribute(Page.GetType(), typeof(AdministrationLevelAttribute));

			//Now determine if they're valid.
			if (adminLevel != null)
			{
				//Get the flags of the *PAGE*, for easier checking below..
				//bool secAdmin = (adminLvl.AdministrationLevel & AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator) == AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator;
				bool allowsProjectAdmin = (adminLevel.AdministrationLevel & AdministrationLevelAttribute.AdministrationLevels.ProjectOwner) == AdministrationLevelAttribute.AdministrationLevels.ProjectOwner;
				bool allowsGroupAdmin = (adminLevel.AdministrationLevel & AdministrationLevelAttribute.AdministrationLevels.GroupOwner) == AdministrationLevelAttribute.AdministrationLevels.GroupOwner;
				bool allowsTemplateAdmin = (adminLevel.AdministrationLevel & AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin) == AdministrationLevelAttribute.AdministrationLevels.ProjectTemplateAdmin;
				bool allowsReportAdmin = (adminLevel.AdministrationLevel & AdministrationLevelAttribute.AdministrationLevels.ReportAdmin) == AdministrationLevelAttribute.AdministrationLevels.ReportAdmin;

				//Get project owner..
				bool isProjectOwner = false;
				if (ProjectRoleId > 0)
				{
					isProjectOwner = new ProjectManager().IsAuthorized(ProjectRoleId, DataModel.Artifact.ArtifactTypeEnum.None, DataModel.Project.PermissionEnum.ProjectAdmin) == DataModel.Project.AuthorizationState.Authorized;
				}

				//Set to base Admin status. Admin-true can see everything.
				bool isAuthorized = UserIsAdmin;

				//See if they're not an Admin, if they should still have access..
				if (allowsProjectAdmin && isProjectOwner) isAuthorized = true;
				if (allowsGroupAdmin && UserIsGroupAdmin) isAuthorized = true;
				if (allowsTemplateAdmin && UserIsTemplateAdmin) isAuthorized = true;
				if (allowsReportAdmin && UserIsReportAdmin) isAuthorized = true;

				if (!isAuthorized)
				{
					//Redirect to the administration home page, with an appropriate message displayed
					Response.Redirect("~/Default.aspx?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=You are not authorized to view this information!", true);
				}
			}
		}
	}
}
