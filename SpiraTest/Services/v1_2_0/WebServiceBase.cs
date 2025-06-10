using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;
using System;
using System.Collections.Generic;
using System.Web.Security;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;

namespace Inflectra.SpiraTest.Web.Services.v1_2_0
{
	/// <summary>
	/// The base class for all SpiraTest web services
	/// </summary>
	public class WebServiceBase : System.Web.Services.WebService
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.WebServiceBase::";

		protected const string Session_Authenticated = "Authenticated";
		protected const string Session_Authorized = "Authorized";
		protected const string Session_ProjectRole = "ProjectRole";

		/// <summary>
		/// Class Constructor
		/// </summary>
		public WebServiceBase()
		{
			//Do Nothing
		}

		/// <summary>
		/// The list of permissions for the current user's role on the current project
		/// </summary>
		protected ProjectRole ProjectRolePermissions
		{
			get
			{
				return (ProjectRole)Session["projectRolePermissions"];
			}
			set
			{
				Session["projectRolePermissions"] = value;
			}
		}

		/// <summary>
		/// The project role id of the authorized user
		/// </summary>
		protected int ProjectRoleId
		{
			get
			{
				if (Session[Session_ProjectRole] == null)
				{
					return -1;
				}
				else
				{
					return (int)Session[Session_ProjectRole];
				}
			}
			set
			{
				Session[Session_ProjectRole] = value;
			}
		}

		/// <summary>
		/// Is the current user the project owner
		/// </summary>
		protected bool IsProjectOwner
		{
			get
			{
				if (Session["projectOwner"] == null)
				{
					return false;
				}
				else
				{
					return (bool)Session["projectOwner"];
				}
			}
			set
			{
				Session["projectOwner"] = value;
			}
		}

		/// <summary>
		/// Connects to a project, needed before calling other artifact import methods
		/// </summary>
		/// <param name="projectId">The project to connect to</param>
		/// <returns>Whether the user is authorized to import into this project</returns>
		/// <remarks>User needs to be a Project Owner or Manager to import data</remarks>
		[
		WebMethod
			(
			Description = "Connects to a project, needed before calling other artifact import methods",
			EnableSession = true
			)
		]
		public bool ConnectToProject(int projectId)
		{
			const string METHOD_NAME = "ConnectToProject";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Default to failure
			bool success = false;
			Session[Session_Authorized] = null;

			//Make sure we have an authenticated user
			if (Session[Session_Authenticated] == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Session Not Authenticated",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			else
			{
				int userId = (int)Session[Session_Authenticated];

				//See if we're authorized for this project
				//We only check to see that they have at least one role as the specific methods
				//need to check that the user can perform that specific function
				//we put the project role in session
				Business.ProjectManager projectManager = new Business.ProjectManager();
				ProjectUserView projectMembership = projectManager.RetrieveUserMembershipById(projectId, userId);
				if (projectMembership != null)
				{
					//Set the authorization session variable to the project id
					//and put the project role in session for use by the methods
					Session[Session_Authorized] = projectId;
					this.ProjectRoleId = projectMembership.ProjectRoleId;

					//Now get the list of permissions for this role
					ProjectRole projectRole = projectManager.RetrieveRolePermissions(ProjectRoleId);
					IsProjectOwner = projectRole.IsAdmin;
					ProjectRolePermissions = projectRole;
					success = true;
				}
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

			return success;
		}

		/// <summary>
		/// Truncates a string to the specified maximum length
		/// </summary>
		/// <param name="input">The string to be truncated</param>
		/// <param name="maxLength">The maximum length for the string</param>
		/// <returns>The truncated string</returns>
		protected string Truncate(string input, int maxLength)
		{
			string output;

			if (input.Length > maxLength)
			{
				output = input.Substring(0, maxLength);
			}
			else
			{
				output = input;
			}
			return output;
		}

		/// <summary>
		/// Retrieves the current date-time on the server in local time
		/// </summary>
		/// <returns>The current date-time in local-time</returns>
		[
		WebMethod
			(
			Description = "Retrieves the current date-time on the server",
			EnableSession = false
			)
		]
		public DateTime RetrieveServerDateTime()
		{
			//Returns the current date-time on the server
			//Need to start with UTC and convert based on global/user settings
			return GlobalFunctions.LocalizeDate(DateTime.UtcNow);
		}

		/// <summary>
		/// Authenticates against the server. Need to call before using other methods
		/// </summary>
		/// <param name="userName">The username of the user</param>
		/// <param name="password">The unhashed password of the user</param>
		/// <returns>Whether authentication was successful or not</returns>
		[
		WebMethod
			(
			Description = "Authenticates against the server. Need to call before using other methods",
			EnableSession = true
			)
		]
		public bool Authenticate(string userName, string password)
		{
			const string METHOD_NAME = "Authenticate";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Default to failure
			bool success = false;
			Session[Session_Authenticated] = null;
			Session[Session_Authorized] = null;
			Session[Session_ProjectRole] = null;
			string lowerUser = userName.ToLowerInvariant();

			//Authenticate the user based on passed-in credentials
			bool validUser = Membership.ValidateUser(lowerUser, password);

			//If the passwrod check does not return, lets see if the password is a GUID and test it.
			Guid passGu;
			if (!validUser && Guid.TryParse(password, out passGu))
			{
				SpiraMembershipProvider prov = (SpiraMembershipProvider)Membership.Provider;
				if (prov != null)
				{
					validUser = prov.ValidateUserByRssToken(lowerUser, password, true, true);
				}
			}

			//Get the session if we have a valid user/password combination
			if (validUser)
			{
				//Set the authenticated session variable to the user id
				MembershipUser membershipUser = Membership.GetUser(lowerUser);

				//Now need to make sure that we have enough concurrent user licenses
				Global.RegisterSession(Session.SessionID, (int)membershipUser.ProviderUserKey, "v1.2 API");

				//Make sure we've not exceeded our count of allowed licenses
				if (License.LicenseType == LicenseTypeEnum.ConcurrentUsers || License.LicenseType == LicenseTypeEnum.Demonstration)
				{
					int concurrentUserCount = Global.ConcurrentUsersCount();
					if (concurrentUserCount > License.Number)
					{
						//Log an error and throw an exception
						LicenseViolationException exception = new LicenseViolationException("You have insufficient licenses available to access this API with this user (" + concurrentUserCount + ")");
						Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
						throw ConvertExceptions(exception);
					}
				}

				Session[Session_Authenticated] = (int)membershipUser.ProviderUserKey;
				success = true;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

			return success;
		}

		/// <summary>
		/// Converts a normal exception into one that SOAP clients can handle
		/// </summary>
		/// <param name="exception">The normal exception</param>
		/// <returns>The Soap Exception</returns>
		protected SoapException ConvertExceptions(Exception exception)
		{
			//-- Build the detail element of the SOAP fault.
			System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
			System.Xml.XmlNode node = doc.CreateNode(
				XmlNodeType.Element,
				SoapException.DetailElementName.Name,
				SoapException.DetailElementName.Namespace
				);

			//-- append our error detail string to the SOAP detail element
			System.Xml.XmlNode details = doc.CreateNode(
				XmlNodeType.Element,
				exception.GetType().Name,
				SoapException.DetailElementName.Namespace
				);
			details.InnerText = exception.Message;
			node.AppendChild(details);
			return new SoapException(exception.Message, SoapException.ServerFaultCode, Context.Request.Url.ToString(), node);
		}

		/// <summary>
		/// Disconnects the currenly authenticated / authorized user
		/// </summary>
		[
		WebMethod
			(
			Description = "Disconnects the currenly authenticated / authorized user",
			EnableSession = true
			)
		]
		public void Disconnect()
		{
			const string METHOD_NAME = "Disconnect";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Reset the authentication and authorization session variables
			Session[Session_Authenticated] = null;
			Session[Session_Authorized] = null;
			Session[Session_ProjectRole] = null;
			Session.Abandon();

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Retrieves a list of projects that the authenticated user has access to
		/// </summary>
		/// <returns>The list of active projects</returns>
		[
		WebMethod
			(
			Description = "Retrieves a list of projects that the passed in user has access to",
			EnableSession = true
			)
		]
		public ProjectData RetrieveProjectList()
		{
			const string METHOD_NAME = "RetrieveProjectList";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have an authenticated user
			if (Session[Session_Authenticated] == null)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new SoapException("Session Not Authenticated",
					SoapException.ClientFaultCode,
					Context.Request.Url.AbsoluteUri);
			}
			else
			{
				int userId = (int)Session[Session_Authenticated];
				Business.ProjectManager projectManager = new Business.ProjectManager();
				List<ProjectForUserView> projects = projectManager.RetrieveForUser(userId);
				ProjectData projectData = new ProjectData();
				foreach (ProjectForUserView project in projects)
				{
					ProjectRow projectRow = new ProjectRow();
					projectRow.ProjectId = project.ProjectId;
					projectRow.Name = project.Name;
					projectRow.Description = project.Description;
					projectRow.CreationDate = project.CreationDate;
					projectRow.ActiveYn = (project.IsActive) ? "Y" : "N";
					projectRow.ProjectGroupId = project.ProjectGroupId;
					projectRow.ProjectGroupName = project.ProjectGroupName;
					projectRow.Website = project.Website;
					projectData.Project.Add(projectRow);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
				return projectData;
			}
		}
	}
}
