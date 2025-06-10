using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Services.Utils;
using System;
using System.ComponentModel;
using System.Web.Security;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;

namespace Inflectra.SpiraTest.Web.Services.v2_2
{
	/// <summary>
	/// This is the base class for all v2.2 web services
	/// </summary>
	[
	WebService(Namespace = "http://www.inflectra.com/SpiraTest/Services/v2.2/"),
	WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1),
	ToolboxItem(false)
	]
	public class WebServiceBase : System.Web.Services.WebService
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.v2_2.WebServiceBase::";

		protected const string Session_ProjectRole = "ProjectRole";
		protected const string Session_ProjectPermissions = "ProjectPermissions";

		#region Properties

		/// <summary>
		/// Is the current user authenticated
		/// </summary>
		protected bool IsAuthenticated
		{
			get
			{
				if (Session["authenticated"] == null)
				{
					return false;
				}
				else
				{
					return (bool)Session["authenticated"];
				}
			}
			set
			{
				//If we are setting to not authenticated - clear the user
				if (value == false)
				{
					Session["authenticatedUserId"] = -1;
				}
				Session["authenticated"] = value;
			}
		}

		/// <summary>
		/// The user id of the authenticated user
		/// </summary>
		protected int AuthenticatedUserId
		{
			get
			{
				if (Session["authenticatedUserId"] == null)
				{
					return -1;
				}
				else
				{
					return (int)Session["authenticatedUserId"];
				}
			}
			set
			{
				Session["authenticatedUserId"] = value;
			}
		}

		/// <summary>
		/// The user name of the authenticated user
		/// </summary>
		protected string AuthenticatedUserName
		{
			get
			{
				if (Session["authenticatedUserName"] == null)
				{
					return "";
				}
				else
				{
					return (string)Session["authenticatedUserName"];
				}
			}
			set
			{
				Session["authenticatedUserName"] = value;
			}
		}

		/// <summary>
		/// Is the current user authorized for the project
		/// </summary>
		protected bool IsAuthorized
		{
			get
			{
				if (Session["authorized"] == null)
				{
					return false;
				}
				else
				{
					return (bool)Session["authorized"];
				}
			}
			set
			{
				//If we are setting to not authorized - clear the project and role
				if (value == false)
				{
					Session["projectRoleId"] = -1;
					Session["authorizedProjectId"] = -1;
					Session["projectOwner"] = false;
				}
				Session["authorized"] = value;
			}
		}

		/// <summary>
		/// Is the current user a system admin
		/// </summary>
		protected bool IsSystemAdmin
		{
			get
			{
				if (String.IsNullOrEmpty(AuthenticatedUserName))
				{
					return false;
				}
				ProfileEx profile = new ProfileEx(AuthenticatedUserName);
				if (profile == null)
				{
					return false;
				}
				return profile.IsAdmin;
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
		/// The project id of the authorized project
		/// </summary>
		protected int AuthorizedProject
		{
			get
			{
				if (Session["authorizedProjectId"] == null)
				{
					return -1;
				}
				else
				{
					return (int)Session["authorizedProjectId"];
				}
			}
			set
			{
				Session["authorizedProjectId"] = value;
			}
		}

		/// <summary>
		/// The project role id of the authorized user
		/// </summary>
		protected int ProjectRoleId
		{
			get
			{
				if (Session["projectRoleId"] == null)
				{
					return -1;
				}
				else
				{
					return (int)Session["projectRoleId"];
				}
			}
			set
			{
				Session["projectRoleId"] = value;
			}
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

		#endregion

		/// <summary>
		/// Class Constructor
		/// </summary>
		public WebServiceBase()
		{
			//Do Nothing
		}

		/// <summary>
		/// Retrieves the current date-time on the server in local time
		/// </summary>
		/// <returns>The current date-time in local time</returns>
		[
		WebMethod
			(
			Description = "Retrieves the current date-time on the server",
			EnableSession = false
			)
		]
		public DateTime System_GetServerDateTime()
		{
			//We need to get the date/time in utc and then convert to the timezone in global settings
			//(since we're not authenticated as a user, we ignore the current profile)
			return GlobalFunctions.LocalizeDate(DateTime.UtcNow);
		}

		/// <summary>
		/// Gets the base URL of the website that the system is running under. Used for notifications and exported artifacts
		/// </summary>
		/// <returns>The URL</returns>
		[
		WebMethod
			(
			Description = "Gets the base URL of the website that the system is running under. Used for notifications and exported artifacts",
			EnableSession = true
			)
		]
		public string System_GetWebServerUrl()
		{
			return ConfigurationSettings.Default.General_WebServerUrl;
		}

		/// <summary>
		/// Gets the brand name of the product installed
		/// </summary>
		/// <returns>The product name</returns>
		[
		WebMethod
			(
			Description = "Gets the brand name of the product installed",
			EnableSession = true
			)
		]
		public string System_GetProductName()
		{
			return ConfigurationSettings.Default.License_ProductType;
		}

		/// <summary>
		/// Authenticates against the server. Need to call before using other methods
		/// </summary>
		/// <param name="userName">The username of the user</param>
		/// <param name="password">The unhashed password of the user</param>
		/// <returns>Whether authentication was successful or not</returns>
		/// <remarks>Also checks to make sure they have enough connection licenses</remarks>
		[
		WebMethod
			(
			Description = "Authenticates against the server. Need to call before using other methods",
			EnableSession = true
			)
		]
		public bool Connection_Authenticate(string userName, string password)
		{
			const string METHOD_NAME = "Connection_Authenticate";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Default to failure
			bool success = false;
			IsAuthenticated = false;
			IsAuthorized = false;
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
				Global.RegisterSession(Session.SessionID, (int)membershipUser.ProviderUserKey, "v2.2 API");

				//Make sure we've not exceeded our count of allowed licenses
				if (Common.License.LicenseType == LicenseTypeEnum.ConcurrentUsers || Common.License.LicenseType == LicenseTypeEnum.Demonstration)
				{
					int concurrentUserCount = Global.ConcurrentUsersCount();
					if (concurrentUserCount > Common.License.Number)
					{
						//Log an error and throw an exception
						LicenseViolationException exception = new LicenseViolationException("You have insufficient licenses available to access this API with this user (" + concurrentUserCount + ")");
						Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
						throw ConvertExceptions(exception);
					}
				}

				IsAuthenticated = true;
				AuthenticatedUserId = (int)membershipUser.ProviderUserKey;
				AuthenticatedUserName = membershipUser.UserName;
				success = true;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

			return success;
		}

		/// <summary>
		/// Authenticates against the server. Need to call before using other methods.
		/// This overload allows you to specify the name of the plug-in calling the API
		/// </summary>
		/// <param name="userName">The username of the user</param>
		/// <param name="password">The unhashed password of the user</param>
		/// <param name="plugInName">The name of the plug-in</param>
		/// <returns>Whether authentication was successful or not</returns>
		/// <remarks>Also checks to make sure they have enough connection licenses</remarks>
		[
		WebMethod
			(
			Description = "Authenticates against the server. Need to call before using other methods",
			EnableSession = true
			)
		]
		public bool Connection_Authenticate2(string userName, string password, string plugInName)
		{
			const string METHOD_NAME = "Connection_Authenticate2";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Default to failure
			bool success = false;
			IsAuthenticated = false;
			IsAuthorized = false;
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
				Global.RegisterSession(Session.SessionID, (int)membershipUser.ProviderUserKey, plugInName);

				//Make sure we've not exceeded our count of allowed licenses
				if (Common.License.LicenseType == LicenseTypeEnum.ConcurrentUsers || Common.License.LicenseType == LicenseTypeEnum.Demonstration)
				{
					int concurrentUserCount = Global.ConcurrentUsersCount();
					if (concurrentUserCount > Common.License.Number)
					{
						//Log an error and throw an exception
						Exception exception = new Exception("You have insufficient licenses available to access this API with this user (" + concurrentUserCount + ")");
						Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
						throw ConvertExceptions(exception);
					}
				}

				IsAuthenticated = true;
				AuthenticatedUserId = (int)membershipUser.ProviderUserKey;
				AuthenticatedUserName = membershipUser.UserName;
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
				StandardExceptions.GetExceptionType(exception),
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
		public void Connection_Disconnect()
		{
			const string METHOD_NAME = "Disconnect";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Reset the authentication and authorization session variables
			IsAuthenticated = false;
			IsAuthorized = false;
			Session.Abandon();

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Connects to a project, needed before calling other artifact import methods
		/// </summary>
		/// <param name="projectId">The project to connect to</param>
		/// <returns>Whether the user is authorized to access this project</returns>
		/// <remarks>The user's role is put into session where other methods can check it for permissions</remarks>
		[
		WebMethod
			(
			Description = "Connects to a project, needed before calling any project-specific methods",
			EnableSession = true
			)
		]
		public bool Connection_ConnectToProject(int projectId)
		{
			const string METHOD_NAME = "ConnectToProject";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Default to failure
			bool success = false;
			IsAuthorized = false;

			//Make sure we have an authenticated user
			if (!IsAuthenticated)
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
				int userId = AuthenticatedUserId;

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
					IsAuthorized = true;
					AuthorizedProject = projectId;
					ProjectRoleId = projectMembership.ProjectRoleId;

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
	}
}
