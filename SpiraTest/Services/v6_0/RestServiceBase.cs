using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Threading;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using System.ServiceModel.Web;
using Inflectra.SpiraTest.Web.Services.Utils;
using Inflectra.SpiraTest.Common;

namespace Inflectra.SpiraTest.Web.Services.v6_0
{
    /// <summary>
    /// The base class for all Rest services
    /// </summary>
    /// <remarks>
    /// Unlike SOAP services these are sessionless
    /// </remarks>
    [
    ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, Namespace = "http://www.inflectra.com/SpiraTest/Services/v6.0/"),
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)
    ]
    public class RestServiceBase
    {
        /// <summary>
        /// Is the current user authenticated
        /// </summary>
        protected bool IsAuthenticated
        {
            get
            {
                if (Thread.CurrentPrincipal == null)
                {
                    return false;
                }
                else if (Thread.CurrentPrincipal.Identity == null)
                {
                    return false;
                }
                else
                {
                    return Thread.CurrentPrincipal.Identity.IsAuthenticated;
                }
            }
        }

        /// <summary>
        /// The user id of the authenticated user (or null)
        /// </summary>
        protected int? AuthenticatedUserId
        {
            get
            {
                if (Thread.CurrentPrincipal == null)
                {
                    return null;
                }
                else if (Thread.CurrentPrincipal.Identity == null)
                {
                    return null;
                }
                else
                {
                    int userId;
                    if (Int32.TryParse(Thread.CurrentPrincipal.Identity.Name, out userId))
                    {
                        return userId;
                    }
                    return null;
                }
            }
        }

        /// <summary>
        /// Converts a normal exception into one that REST clients can handle
        /// </summary>
        /// <param name="exception">The normal exception</param>
        /// <returns>The Fault Exception</returns>
        protected WebFaultException<string> ConvertExceptions(Exception exception)
        {
            //If it's a WebFaultException already, then we have nothing to do
            if (exception is WebFaultException<string>)
            {
                return (WebFaultException<string>)exception;
            }

            //If an artifact not found, convert to a 404
            if (exception is ArtifactNotExistsException)
            {
                return new WebFaultException<string>(exception.Message, System.Net.HttpStatusCode.NotFound);
            }

            //Now return the WebFault Exception that contains the details of the exception
            //We always use BadRequest for these ones
            string detail = exception.GetType().Name + ": " + exception.Message;
            return new WebFaultException<string>(detail, System.Net.HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Creates a new REST web fault message based on the passed-in information
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="type">The error type</param>
        /// <returns>The Fault Exception</returns>
        protected WebFaultException<string> CreateFault(string type, string message)
        {
            //Now return the WebFault Exception that contains the details of the exception
            //We always use BadRequest for these ones
            string detail = type + ": " + message;
            return new WebFaultException<string>(message, System.Net.HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Returns a special data validation exception that contains a list of fields and messages
        /// </summary>
        /// <param name="validationMessages">The dictionary of validation messages</param>
        /// <returns>The Fault Exception</returns>
        protected WebFaultException<ValidationFaultMessage> CreateValidationException(Dictionary<string, string> validationMessages)
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

            return new WebFaultException<ValidationFaultMessage>(validationFaultMessage, System.Net.HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Checks to see if the current user is a system administrator
        /// </summary>
        /// <returns>True if a system administrator</returns>
        protected bool IsSystemAdmin()
        {
            User user = new UserManager().GetUserById(this.AuthenticatedUserId.Value);
            if (user == null || user.Profile == null)
            {
                //Handle nulls safely
                return false;
            }
            return user.Profile.IsAdmin;
        }

		/// <summary>
		/// Checks to see if the current user is a report administrator
		/// </summary>
		/// <returns>True if a report administrator</returns>
		protected bool IsReportAdmin()
		{
			User user = new UserManager().GetUserById(this.AuthenticatedUserId.Value);
			if (user == null || user.Profile == null)
			{
				//Handle nulls safely
				return false;
			}
			return user.Profile.IsReportAdmin;
		}

		/// <summary>
		/// Checks to see if the specified user is authorized to view a specific program
		/// </summary>
		/// <param name="credentials">The current user's credentials</param>
		/// <param name="projectGroupId">The id of the project group</param>
		/// <returns>True - if authorized</returns>
		protected bool IsAuthorizedForGroup(int userId, int projectGroupId)
        {
            ProjectGroupManager projectGroupManager = new ProjectGroupManager();
            bool isAuthorized = projectGroupManager.IsAuthorized(userId, projectGroupId);
            return isAuthorized;
        }

        /// <summary>
        /// Checks to see if the current user is authorized to perform a specific action on an artifact type
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="artifactType">The artifact type</param>
        /// <param name="permission">The operation being performed</param>
        /// <returns>True if authorized</returns>
        protected bool IsAuthorized(int projectId, DataModel.Artifact.ArtifactTypeEnum artifactType, Project.PermissionEnum permission)
        {
            bool isAuthorized = false;
            //See if we're authorized for this project
            //We only check to see that they have at least one role as the specific methods
            //need to check that the user can perform that specific function
            //we put the project role in session
            Business.ProjectManager projectManager = new Business.ProjectManager();
            ProjectUserView projectUser = projectManager.RetrieveUserMembershipById(projectId, this.AuthenticatedUserId.Value);
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
		/// Checks to see if the current user is a member (with any role) of the passed in project Id
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>True if a project member</returns>
		protected bool IsProjectMember(int projectId)
		{
			bool isAuthorized = false;
			//See if we're authorized for this project
			//We only check to see that they have at least one role as the specific methods
			Business.ProjectManager projectManager = new Business.ProjectManager();
			ProjectUserView projectUser = projectManager.RetrieveUserMembershipById(projectId, this.AuthenticatedUserId.Value);
			if (projectUser != null)
			{
				isAuthorized = true;
			}
			return isAuthorized;
		}

		protected int UserIdFullyAuthenticated(string CLASS_NAME, string METHOD_NAME)
		{
			if (!IsAuthenticated)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			int? userId = AuthenticatedUserId;
			if (!userId.HasValue)
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException(System.Net.HttpStatusCode.Unauthorized);
			}
			else
			{
				return userId.Value;
			}
		}

		/// <summary>
		/// Throws an exception if current user is not a member (with any role) of the passed in project Id
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="CLASS_NAME">The string of the class name</param>
		/// <param name="METHOD_NAME">The string of the method name</param>
		/// <returns>True if a project member</returns>
		protected void HandleProjectAuthorization(int projectId, string CLASS_NAME, string METHOD_NAME)
		{
			if (!IsProjectMember(projectId))
			{
				//Throw back an exception
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();

				throw new WebFaultException<string>(String.Format(Resources.Messages.WebPartBase_NotAuthorizedForProject, projectId), System.Net.HttpStatusCode.Unauthorized);
			}
		}
	}
}
