using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;

using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Business;
using System.Xml;
using System.ServiceModel.Channels;
using System.ServiceModel.Activation;
using System.Web.Security;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Services.Utils;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Services.v6_0.DataObjects;

namespace Inflectra.SpiraTest.Web.Services.v6_0
{
    /// <summary>
    /// This is the base class for all v6.0 SOAP WCF services
    /// </summary>
    /// <remarks>
    /// We use ASP.NET compatibility mode because the services are hosted in IIS and we need access to the session object
    /// </remarks>
    [
    ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, Namespace = "http://www.inflectra.com/SpiraTest/Services/v6.0/"),
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required),
    ]
    public class SoapServiceBase
    {
       private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.v6_0.ServiceBase::";

		private const string RAPISE_PLUGIN_NAME = "Rapise";	//There are some special behaviors we perform for Rapise calling

        /// <summary>
        /// Class Constructor
        /// </summary>
        public SoapServiceBase()
        {
            //Do Nothing
        }

        /// <summary>
        /// Authenticates against the server. Used by all the public methods
        /// </summary>
        /// <param name="userName">The username of the user</param>
        /// <param name="password">The unhashed password of the user</param>
        /// <param name="plugInName">The name of the plug-in</param>
        /// <returns>True if successful</returns>
        /// <remarks>Also checks to make sure they have enough connection licenses</remarks>
        protected bool IsAuthenticated(RemoteCredentials credentials)
        {
            const string METHOD_NAME = "IsAuthenticated";

            if (credentials == null)
            {
                throw new ArgumentNullException(Resources.Messages.Services_EmptyCredentialsObject);
            }

            //Authenticate the user based on passed-in credentials, see if we have an API Key or Password
            bool validUser;
            string userName = credentials.UserName;
            if (String.IsNullOrEmpty(credentials.ApiKey))
            {
                validUser = Membership.ValidateUser(userName.ToLower(System.Globalization.CultureInfo.InvariantCulture), credentials.Password);
            }
            else if (string.IsNullOrEmpty(credentials.Password))
            {
                SpiraMembershipProvider provider = (SpiraMembershipProvider)Membership.Provider;
                validUser = provider.ValidateUserByRssToken(userName.ToLower(System.Globalization.CultureInfo.InvariantCulture), credentials.ApiKey);
            }
            else
            {
                throw new ArgumentNullException(Resources.Messages.Services_YouNeedToPassApiKeyOrPassword);
            }

            //Get the session if we have a valid user/password combination
            if (validUser)
            {
                //Set the authenticated session variable to the user id
                MembershipUser membershipUser = Membership.GetUser(userName.ToLowerInvariant());

				//Now need to make sure that we have enough concurrent user licenses, unless it was Rapise
				if (credentials.PlugInName != RAPISE_PLUGIN_NAME)
				{
					Web.Global.RegisterSession(HttpContext.Current.Session.SessionID, (int)membershipUser.ProviderUserKey, credentials.PlugInName);
				}

                //Make sure we've not exceeded our count of allowed licenses
                if (Common.License.LicenseType == LicenseTypeEnum.ConcurrentUsers || Common.License.LicenseType == LicenseTypeEnum.Demonstration)
                {
                    int concurrentUserCount = Web.Global.ConcurrentUsersCount();
                    if (concurrentUserCount > Common.License.Number)
                    {
                        //Log an error and throw an exception
                        LicenseViolationException exception = new LicenseViolationException(Resources.Messages.Services_LicenseViolationException + " (" + concurrentUserCount + ")");
                        Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                        throw ConvertExceptions(exception);
                    }
                }

                //Populate the User ID and update user name to correct case
                credentials.UserId = (int)membershipUser.ProviderUserKey;
                credentials.UserName = (string)membershipUser.UserName;

                //Finally see if this user is a system admin
                ProfileEx profile = new ProfileEx(credentials.UserName);
                if (profile == null)
                {
                    credentials.IsSystemAdmin = false;
                }
                credentials.IsSystemAdmin = profile.IsAdmin;
            }
            return validUser;
        }

        /// <summary>
        /// Checks to see if the user is a member of the project
        /// </summary>
        /// <param name="credentials">The current user's credentials</param>
        /// <param name="projectId">The id of the project</param>
        /// <returns>True if authorized</returns>
        protected bool IsAuthorized(RemoteCredentials credentials, int projectId)
        {
            Business.ProjectManager projectManager = new Business.ProjectManager();
            List<ProjectForUserView> authProjects = projectManager.RetrieveForUser(credentials.UserId);
            if (authProjects.Any(p => p.ProjectId == projectId))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks to see if the specified user is authorized to view a specific program
        /// </summary>
        /// <param name="credentials">The current user's credentials</param>
        /// <param name="projectGroupId">The id of the project group</param>
        /// <returns>True - if authorized</returns>
        protected bool IsAuthorizedForGroup(RemoteCredentials credentials, int projectGroupId)
        {
            ProjectGroupManager projectGroupManager = new ProjectGroupManager();
            bool isAuthorized = projectGroupManager.IsAuthorized(credentials.UserId, projectGroupId);
            return isAuthorized;
        }

        /// <summary>
        /// Checks to see if the current user is authorized to perform a specific action on an artifact type
        /// </summary>
        /// <param name="credentials">The current user's credentials</param>
        /// <param name="projectId">The id of the project</param>
        /// <param name="artifactType">The artifact type</param>
        /// <param name="permission">The operation being performed</param>
        /// <returns>True if authorized</returns>
        protected bool IsAuthorized(RemoteCredentials credentials, int projectId, DataModel.Artifact.ArtifactTypeEnum artifactType, Project.PermissionEnum permission)
        {
            bool isAuthorized = false;
            //See if we're authorized for this project
            //We only check to see that they have at least one role as the specific methods
            //need to check that the user can perform that specific function
            //we put the project role in session
            Business.ProjectManager projectManager = new Business.ProjectManager();
            ProjectUserView projectUser = projectManager.RetrieveUserMembershipById(projectId, credentials.UserId);
            if (projectUser != null)
            {
                //If we're the project owner, then we're authorized
                if (projectUser.IsAdmin)
                {
                    isAuthorized = true;
                }
                else
                {
                    //Now get the list of permissions for this role
                    int projectRoleId = projectUser.ProjectRoleId;
                    ProjectRole projectRole = projectManager.RetrieveRolePermissions(projectRoleId);
                    if (projectRole.FindByProjectRoleIdArtifactTypeIdPermissionId(projectRoleId, (int)artifactType, (int)permission) != null)
                    {
                        isAuthorized = true;
                    }
                }
            }

            return isAuthorized;
        }

		/// <summary>
		/// Checks to see if the current user is a report administrator
		/// </summary>
		/// <returns>True if a report administrator</returns>
		protected bool IsReportAdmin(RemoteCredentials credentials)
		{
			User user = new UserManager().GetUserById(credentials.UserId);
			if (user == null || user.Profile == null)
			{
				//Handle nulls safely
				return false;
			}
			return user.Profile.IsReportAdmin;
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
        /// Returns a special data validation exception that contains a list of fields and messages
        /// </summary>
        /// <param name="validationMessages">The dictionary of validation messages</param>
        /// <returns>The Fault Exception</returns>
        protected FaultException CreateValidationException(Dictionary<string, string> validationMessages)
        {
            ValidationFaultMessage validationFaultMessage = new ValidationFaultMessage();
			validationFaultMessage.Messages = new List<ValidationFaultMessageItem>();
            validationFaultMessage.Summary = "Validation Fault";
            foreach (KeyValuePair<string, string> validationMessage in validationMessages)
            {
                ValidationFaultMessageItem item = new ValidationFaultMessageItem();
                item.FieldName = validationMessage.Key;
                item.Message = validationMessage.Value;
                validationFaultMessage.Messages.Add(item);
            }

            //Build the Fault Reason
            FaultReason faultReason = new FaultReason(validationFaultMessage.Summary);

            return new FaultException<ValidationFaultMessage>(validationFaultMessage, faultReason);
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
		/// Checks if the passed in use credentials are valid and authorized for the projectId
		/// </summary>
		/// <param name="credentials">The API credentials</param>
		/// <param name="projectId">The ID of the project</param>
		/// <param name="CLASS_NAME">string of the class name</param>
		/// <param name="METHOD_NAME">string of the method name</param>
		/// <returns>Throws an exception if needed otherwise returns true</returns>
		protected bool IsAuthorizedForProject(RemoteCredentials credentials, int projectId, string CLASS_NAME, string METHOD_NAME)
		{
			//Make sure we have an authenticated user
			if (!IsAuthenticated(credentials))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("SessionNotAuthenticated", Resources.Messages.Services_SessionNotAuthenticated);
			}
			int userId = credentials.UserId;

			//Make sure we are connected to a project
			if (!IsAuthorized(credentials, projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw CreateFault("NotConnectedToProject", String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId));
			}
			return true;
		}
	}
}
