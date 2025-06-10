using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Services.Utils;
using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Security;

namespace Inflectra.SpiraTest.Web.Services.v3_0
{
	/// <summary>
	/// This is the base class for all v3.0 SOAP WCF services
	/// </summary>
	/// <remarks>
	/// We use ASP.NET compatibility mode because the services are hosted in IIS and we need access to the session object
	/// </remarks>
	[
	ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, Namespace = "http://www.inflectra.com/SpiraTest/Services/v3.0/"),
	AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required),
	]
	public class ServiceBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.v3_0.ServiceBase::";

		#region Properties

		/// <summary>
		/// Is the current user authenticated
		/// </summary>
		protected bool IsAuthenticated
		{
			get
			{
				bool retValue = false;
				if (HttpContext.Current.Session["authenticated"] != null)
				{
					retValue = (bool)HttpContext.Current.Session["authenticated"];
				}

				return retValue;
			}
			set
			{
				//If we are setting to not authenticated - clear the user
				if (value == false)
				{
					HttpContext.Current.Session["authenticatedUserId"] = -1;
				}
				HttpContext.Current.Session["authenticated"] = value;
			}
		}

		/// <summary>
		/// The user id of the authenticated user
		/// </summary>
		protected int AuthenticatedUserId
		{
			get
			{
				if (HttpContext.Current.Session["authenticatedUserId"] == null)
				{
					return -1;
				}
				else
				{
					return (int)HttpContext.Current.Session["authenticatedUserId"];
				}
			}
			set
			{
				HttpContext.Current.Session["authenticatedUserId"] = value;
			}
		}

		/// <summary>
		/// The user name of the authenticated user
		/// </summary>
		protected string AuthenticatedUserName
		{
			get
			{
				if (HttpContext.Current.Session["authenticatedUserName"] == null)
				{
					return "";
				}
				else
				{
					return (string)HttpContext.Current.Session["authenticatedUserName"];
				}
			}
			set
			{
				HttpContext.Current.Session["authenticatedUserName"] = value;
			}
		}

		/// <summary>
		/// Is the current user authorized for the project
		/// </summary>
		protected bool IsAuthorized
		{
			get
			{
				if (HttpContext.Current.Session["authorized"] == null)
				{
					return false;
				}
				else
				{
					return (bool)HttpContext.Current.Session["authorized"];
				}
			}
			set
			{
				//If we are setting to not authorized - clear the project and role
				if (value == false)
				{
					HttpContext.Current.Session["projectRoleId"] = -1;
					HttpContext.Current.Session["authorizedProjectId"] = -1;
					HttpContext.Current.Session["projectOwner"] = false;
				}
				HttpContext.Current.Session["authorized"] = value;
			}
		}

		/// <summary>
		/// Is the current user the project owner
		/// </summary>
		protected bool IsProjectOwner
		{
			get
			{
				if (HttpContext.Current.Session["projectOwner"] == null)
				{
					return false;
				}
				else
				{
					return (bool)HttpContext.Current.Session["projectOwner"];
				}
			}
			set
			{
				HttpContext.Current.Session["projectOwner"] = value;
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
		/// The project id of the authorized project
		/// </summary>
		protected int AuthorizedProject
		{
			get
			{
				if (HttpContext.Current.Session["authorizedProjectId"] == null)
				{
					return -1;
				}
				else
				{
					return (int)HttpContext.Current.Session["authorizedProjectId"];
				}
			}
			set
			{
				HttpContext.Current.Session["authorizedProjectId"] = value;
			}
		}

		/// <summary>
		/// The project role id of the authorized user
		/// </summary>
		protected int ProjectRoleId
		{
			get
			{
				if (HttpContext.Current.Session["projectRoleId"] == null)
				{
					return -1;
				}
				else
				{
					return (int)HttpContext.Current.Session["projectRoleId"];
				}
			}
			set
			{
				HttpContext.Current.Session["projectRoleId"] = value;
			}
		}

		/// <summary>
		/// The list of permissions for the current user's role on the current project
		/// </summary>
		protected ProjectRole ProjectRolePermissions
		{
			get
			{
				return (ProjectRole)HttpContext.Current.Session["projectRolePermissions"];
			}
			set
			{
				HttpContext.Current.Session["projectRolePermissions"] = value;
			}
		}

		#endregion

		/// <summary>
		/// Class Constructor
		/// </summary>
		public ServiceBase()
		{
			//Do Nothing
		}

		/// <summary>
		/// Retrieves the current date-time on the server in local time
		/// </summary>
		/// <returns>The current date-time in local time</returns>
		public DateTime System_GetServerDateTime()
		{
			//Returns the current date-time on the server
			//We need to get the date/time in utc and then convert to the timezone in global settings
			//(since we're not authenticated as a user, we ignore the current profile)
			return GlobalFunctions.LocalizeDate(DateTime.UtcNow);
		}

		/// <summary>
		/// Gets the base URL of the website that the system is running under. Used for notifications and exported artifacts
		/// </summary>
		/// <returns>The URL</returns>
		public string System_GetWebServerUrl()
		{
			return ConfigurationSettings.Default.General_WebServerUrl;
		}

		/// <summary>
		/// Gets the brand name of the product installed
		/// </summary>
		/// <returns>The product name</returns>
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
		public bool Connection_Authenticate(string userName, string password)
		{
			const string METHOD_NAME = "Authenticate";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Default to failure
			bool success = false;
			IsAuthenticated = false;
			IsAuthorized = false;
			string lowerUser = userName.ToLower(System.Globalization.CultureInfo.InvariantCulture);

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
				Global.RegisterSession(HttpContext.Current.Session.SessionID, (int)membershipUser.ProviderUserKey, "v3.0 API");

				//Make sure we've not exceeded our count of allowed licenses
				if (License.LicenseType == LicenseTypeEnum.ConcurrentUsers || License.LicenseType == LicenseTypeEnum.Demonstration)
				{
					int concurrentUserCount = Global.ConcurrentUsersCount();
					if (concurrentUserCount > License.Number)
					{
						//Log an error and throw an exception
						LicenseViolationException exception = new LicenseViolationException(Resources.Messages.Services_LicenseViolationException + " (" + concurrentUserCount + ")");
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
				Global.RegisterSession(HttpContext.Current.Session.SessionID, (int)membershipUser.ProviderUserKey, plugInName);

				//Make sure we've not exceeded our count of allowed licenses
				if (License.LicenseType == LicenseTypeEnum.ConcurrentUsers || License.LicenseType == LicenseTypeEnum.Demonstration)
				{
					int concurrentUserCount = Global.ConcurrentUsersCount();
					if (concurrentUserCount > License.Number)
					{
						//Log an error and throw an exception
						LicenseViolationException exception = new LicenseViolationException(Resources.Messages.Services_LicenseViolationException + " (" + concurrentUserCount + ")");
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
		/// Converts a normal exception into one that WCF clients can handle
		/// </summary>
		/// <param name="exception">The normal exception</param>
		/// <returns>The Fault Exception</returns>
		protected FaultException ConvertExceptions(Exception exception)
		{
			//If it's a FaultException already, then we have nothing to do
			if (exception is FaultException)
			{
				return (FaultException)exception;
			}

			//-- Build the detail element of the Fault Message
			ServiceFaultMessage serviceFaultMessage = new ServiceFaultMessage();
			serviceFaultMessage.Message = exception.Message;
			serviceFaultMessage.StackTrace = exception.StackTrace;
			serviceFaultMessage.Type = StandardExceptions.GetExceptionType(exception);

			//Build the Fault Reason
			FaultReason faultReason = new FaultReason(exception.Message);

			//Now return the FaultException that contains the details of the exception
			return new FaultException<ServiceFaultMessage>(serviceFaultMessage, faultReason);
		}

		/// <summary>
		/// Creates a new WCF fault message based on the passed-in information
		/// </summary>
		/// <param name="message">The error message</param>
		/// <param name="type">The error type</param>
		/// <returns>The Fault Exception</returns>
		protected FaultException CreateFault(string type, string message)
		{
			//-- Build the detail element of the Fault Message
			ServiceFaultMessage serviceFaultMessage = new ServiceFaultMessage();
			serviceFaultMessage.Message = message;
			serviceFaultMessage.StackTrace = String.Empty;
			serviceFaultMessage.Type = type;

			//Build the Fault Reason
			FaultReason faultReason = new FaultReason(message);

			//Now return the FaultException that contains the details of the exception
			return new FaultException<ServiceFaultMessage>(serviceFaultMessage, faultReason);
		}

		/// <summary>
		/// Disconnects the currenly authenticated / authorized user
		/// </summary>
		public void Connection_Disconnect()
		{
			const string METHOD_NAME = "Disconnect";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Reset the authentication and authorization session variables
			IsAuthenticated = false;
			IsAuthorized = false;
			HttpContext.Current.Session.Abandon();

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Connects to a project, needed before calling other artifact import methods
		/// </summary>
		/// <param name="projectId">The project to connect to</param>
		/// <returns>Whether the user is authorized to access this project</returns>
		/// <remarks>The user's role is put into session where other methods can check it for permissions</remarks>
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

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
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