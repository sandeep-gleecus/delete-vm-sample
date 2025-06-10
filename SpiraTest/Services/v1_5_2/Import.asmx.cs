using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using System.Xml;

using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Business;
using System.Web.Security;
using Inflectra.SpiraTest.Web.Classes;
using System.Web.Profile;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Services.Utils;

namespace Inflectra.SpiraTest.Web.Services.v1_5_2
{
    /// <summary>
    /// This class provides the functionality for importing data from an external application into SpiraTest
    /// and for exporting SpiraTest data for import into another application. It includes methods that were
    /// formerly in a separate Export web service.
    /// </summary>
    /// <remarks>
    /// The namespace includes a versioning compatibility parameter. This version number
    /// only changes when the API changes, so it may be lower that the current application version
    /// </remarks>
    [
    WebService
        (
        Namespace = "http://www.inflectra.com/SpiraTest/Services/v1.5.2/",
        Description = "This service provides the functionality for importing data from an external application into SpiraTest and for exporting SpiraTest data for import into another application."
        )
    ]
    public class Import : WebServiceBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Import::";

        /// <summary>
        /// Creates a new project in the system and makes the authenticated user owner of it
        /// </summary>
        /// <param name="name">The name of the project</param>
        /// <param name="description">The description of the project</param>
        /// <param name="url">A URL related to the project</param>
        /// <returns>The ID of the newly created project</returns>
        [
        WebMethod
            (
            Description = "Creates a new project in the system and makes the authenticated user owner of it",
            EnableSession = true
            )
        ]
        public int CreateProject(string name, string description, string url)
        {
            const string METHOD_NAME = "CreateProject";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            else
            {
                int userId = (int)Session[Session_Authenticated];

                //Make sure we have permissions to create projects (i.e. is a system admin)
                if (!IsSystemAdmin)
                {
                    //Throw back an exception
                    Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                    Logger.Flush();

                    throw new SoapException("Not Authorized to Create Projects - Need to be System Administrator",
                        SoapException.ClientFaultCode,
                        Context.Request.Url.AbsoluteUri);
                }

                //First create the project
                Business.ProjectManager projectManager = new Business.ProjectManager();
                int projectId = projectManager.Insert(name, null, description, url, true, null, userId);

                //Now we need to formally connect to this project so that the permissions get setup correctly
                //for subsequent API access
                this.ConnectToProject(projectId);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return projectId;
            }
        }

        /// <summary>
        /// Adds a new user to the system and to the current project
        /// </summary>
        /// <param name="firstName">The first name of the user</param>
        /// <param name="middleInitial">The middle initial of the user</param>
        /// <param name="lastName">The last name of the user</param>
        /// <param name="userName">The username/login</param>
        /// <param name="emailAddress">The email address</param>
        /// <param name="password">The password</param>
        /// <param name="isAdmin">Whether the user is a system admin</param>
        /// <param name="isActive">Whether the user is active or not</param>
        /// <param name="projectRoleId">The project role for the user</param>
        /// <returns>The ID of the newly created user</returns>
        [
        WebMethod
            (
            Description = "Adds a new user to the system and to the current project",
            EnableSession = true
            )
        ]
        public int AddUser(string firstName, string middleInitial, string lastName, string userName, string emailAddress, string password, bool isAdmin, bool isActive, int projectRoleId)
        {
            const string METHOD_NAME = "AddUser";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int authenticatedUserId = (int)Session[Session_Authenticated];

            //Make sure we are connected to a project
            if (Session[Session_Authorized] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int projectId = (int)Session[Session_Authorized];

            //Make sure we have permissions to create users (i.e. is a system admin)
            if (!IsSystemAdmin)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException("Not Authorized to Create Users - Need to be System Administrator",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            //Make sure we have a populated first and last name so that we don't have the user created without a valid profile
            if (String.IsNullOrEmpty(firstName) || String.IsNullOrEmpty(lastName))
            {
                throw new SoapException("DataValidationError: " + Resources.Messages.Services_FirstOrLastNameNotProvided, SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            try
            {
                //First create the user, if it exists already ignore the exception
                string passwordQuestion = "What was the email address originally associated with the account?";
                string passwordAnswer = emailAddress;
                MembershipCreateStatus status;
                MembershipUser membershipUser = Membership.CreateUser(userName, password, emailAddress, passwordQuestion, passwordAnswer, isActive, out status);
                if (status == MembershipCreateStatus.Success)
                {
                    if (membershipUser == null)
                    {
                        string message = "Unable to create user - " + status.ToString();
                        Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, message);
                        Logger.Flush();
                        throw new SoapException(message,
                            SoapException.ClientFaultCode,
                            Context.Request.Url.AbsoluteUri);
                    }

                    //Now we need to add the profile information
                    ProfileEx profile = new ProfileEx(userName);
                    profile.FirstName = firstName;
                    profile.LastName = lastName;
                    profile.MiddleInitial = middleInitial;
                    profile.LastOpenedProjectId = projectId;
                    profile.IsEmailEnabled = true;
                    profile.IsAdmin = isAdmin;
                    profile.Save();
                }
                else if (status == MembershipCreateStatus.DuplicateUserName)
                {
                    //If we get this error we need to instead return the user record already in the system
                    membershipUser = Membership.GetUser(userName);
                }
                else
                {
                    string message = "Unable to create user - " + status.ToString();
                    Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, message);
                    Logger.Flush();
                    throw new SoapException(message,
                        SoapException.ClientFaultCode,
                        Context.Request.Url.AbsoluteUri);
                }

                //Now add the user to the current project as the specified role
                //Ignore any duplicate key errors
                int userId = (int)membershipUser.ProviderUserKey;
                Business.ProjectManager projectManager = new Business.ProjectManager();
                try
                {
                    projectManager.InsertUserMembership(userId, projectId, projectRoleId);
                }
                catch (ProjectDuplicateMembershipRecordException)
                {
                    //Ignore this error
                }
                catch (EntityConstraintViolationException)
                {
                    //Ignore error due to duplicate row
                }
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return userId;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Maps a requirement to a test case, so that the test case 'covers' the requirement
        /// </summary>
        /// <param name="requirementId">The requirement being covered</param>
        /// <param name="testCaseId">The test case performing the coverage</param>
        /// <remarks>If the coverage record already exists no error is raised</remarks>
        [
        WebMethod
            (
            Description = "Maps a requirement to a test case, so that the test case 'covers' the requirement",
            EnableSession = true
            )
        ]
        public void AddRequirementTestCoverage(int requirementId, int testCaseId)
        {
            const string METHOD_NAME = "AddRequirementTestCoverage";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int userId = (int)Session[Session_Authenticated];

            //Make sure we are connected to a project
            if (Session[Session_Authorized] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int projectId = (int)Session[Session_Authorized];

            //Make sure we have permissions to update test cases
            if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Modify) == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException("Not Authorized to Modify Test Cases",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            try
            {
                //Add the test case we want to use for coverage
                TestCaseManager testCaseManager = new TestCaseManager();
                List<int> testCaseIds = new List<int>();
                testCaseIds.Add(testCaseId);
                testCaseManager.AddToRequirement(projectId, requirementId, testCaseIds, userId);
            }
            catch (EntityConstraintViolationException)
            {
                //Ignore error due to duplicate row
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Removes a coverage mapping entry for a specific requirement and test case
        /// </summary>
        /// <param name="requirementId">The requirement being covered</param>
        /// <param name="testCaseId">The test case performing the coverage</param>
        [
        WebMethod
            (
            Description = "Removes a coverage mapping entry for a specific requirement and test case",
            EnableSession = true
            )
        ]
        public void RemoveRequirementTestCoverage(int requirementId, int testCaseId)
        {
            const string METHOD_NAME = "RemoveRequirementTestCoverage";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int userId = (int)Session[Session_Authenticated];

            //Make sure we are connected to a project
            if (Session[Session_Authorized] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int projectId = (int)Session[Session_Authorized];

            //Make sure we have permissions to update test cases
            if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Modify) == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException("Not Authorized to Modify Test Cases",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            try
            {
                //Remove the test case from the requirement
                TestCaseManager testCaseManager = new TestCaseManager();
                List<int> testCaseIds = new List<int>();
                testCaseIds.Add(testCaseId);
                testCaseManager.RemoveFromRequirement(projectId, requirementId, testCaseIds, userId);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Adds a new requirement record to the current project using the position offset method
        /// </summary>
        /// <param name="importanceId">The importance level of the requirement</param>
        /// <param name="requirementName">The requirement name</param>
        /// <param name="statusId">The status of the requirement</param>
        /// <param name="authorUserId">The author of the requirement (-1 for none)</param>
        /// <param name="indentPosition">The number of columns to indent the requirement by (positive for indent, negative for outdent)</param>
        /// <returns>The ID of the newly added requirement</returns>
        [
        WebMethod
            (
            Description = "Adds a new requirement record to the current project using the position offset method",
            EnableSession = true
            )
        ]
        public int AddRequirement(int releaseId, int statusId, int importanceId, string requirementName, string requirementDescription, int indentPosition, int authorUserId)
        {
            const string METHOD_NAME = "AddRequirement";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int userId = (int)Session[Session_Authenticated];

            //Make sure we are connected to a project
            if (Session[Session_Authorized] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int projectId = (int)Session[Session_Authorized];
            int requirementId = -1;

            //Make sure we have permissions to create requirements
            if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Requirement, (int)Project.PermissionEnum.Create) == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException("Not Authorized to Create Requirements",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            try
            {
                //Make sure the name isn't too long
                requirementName = Truncate(GlobalFunctions.HtmlScrubInput(requirementName), 255);

                //Default to the authenticated user if we have no author provided
                if (authorUserId == -1)
                {
                    authorUserId = userId;
                }

                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //See if we have any IDs that used to be static and that are now template/project based
                //If so, we need to convert them
                if (importanceId >= 1 && importanceId <= 4)
                {
                    ConvertRequirementLegacyStaticIds(projectTemplateId, ref importanceId);
                }

                //Now insert the requirement at the end
                Business.RequirementManager requirementManager = new Business.RequirementManager();
                requirementId = requirementManager.Insert(
                    userId,
                    projectId,
                    (releaseId < 1) ? null : (int?)releaseId,
                    null,
                    (int?)null,
                    (Requirement.RequirementStatusEnum)statusId,
                    null,
                    authorUserId,
                    null,
                    (importanceId < 1) ? null : (int?)importanceId,
                    requirementName,
                    requirementDescription,
                    null,
                    userId);

                //Finally we need to indent it or outdent it the correct number of times
                if (indentPosition > 0)
                {
                    for (int i = 0; i < indentPosition; i++)
                    {
                        requirementManager.Indent(userId, projectId, requirementId);
                    }
                }
                if (indentPosition < 0)
                {
                    for (int i = 0; i > indentPosition; i--)
                    {
                        requirementManager.Outdent(userId, projectId, requirementId);
                    }
                }

                //Send a notification
                requirementManager.SendCreationNotification(requirementId, null, null);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
            return requirementId;
        }

 
        /// <summary>
        /// Updates an existing requirement entry in the system (only its data, not its position)
        /// </summary>
        /// <param name="requirementId">The requirement to update</param>
        /// <param name="importanceId">The importance level of the requirement</param>
        /// <param name="requirementName">The requirement name</param>
        /// <param name="statusId">The status of the requirement</param>
        /// <param name="authorUserId">The author of the requirement (-1 for none)</param>
        [
        WebMethod
            (
            Description = "Updates an existing requirement entry in the system (only its data, not its position)",
            EnableSession = true
            )
        ]
        public void UpdateRequirement(int requirementId, int statusId, int importanceId, string requirementName, int authorUserId)
        {
            const string METHOD_NAME = "UpdateRequirement";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int userId = (int)Session[Session_Authenticated];

            //Make sure we are connected to a project
            if (Session[Session_Authorized] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int projectId = (int)Session[Session_Authorized];

            //Make sure we have permissions to update requirements
            if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Requirement, (int)Project.PermissionEnum.Modify) == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException("Not Authorized to Modify Requirements",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            try
            {
                //First retrieve the requirement dataset for the passed-in id
                Business.RequirementManager requirementManager = new Business.RequirementManager();
                Requirement requirement = requirementManager.RetrieveById3(projectId, requirementId);
                if (requirement == null)
                {
                    throw new SoapException("Cannot find the supplied requirement id in the system",
                        SoapException.ClientFaultCode,
                        Context.Request.Url.AbsoluteUri);
                }

                //Make sure that the project ids match
                if (requirement.ProjectId != projectId)
                {
                    throw new SoapException("The requirement does not belong to the authorized project",
                        SoapException.ClientFaultCode,
                        Context.Request.Url.AbsoluteUri);
                }

                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //See if we have any IDs that used to be static and that are now template/project based
                //If so, we need to convert them
                if (importanceId >= 1 && importanceId <= 4)
                {
                    ConvertRequirementLegacyStaticIds(projectTemplateId, ref importanceId);
                }

                //Make the data updates
                requirement.StartTracking();
                requirement.Name = requirementName;
                requirement.ImportanceId = (importanceId < 1) ? null : (int?)importanceId;
                requirement.RequirementStatusId = statusId;
                requirement.AuthorId = authorUserId;

                //Now commit the updates
                requirementManager.Update(userId, projectId, new List<Requirement>() { requirement });
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Updates an existing test case entry in the system (only its data, not its position)
        /// </summary>
        /// <param name="testCaseId">The test case to be updated</param>
        /// <param name="testCaseName">The test case name</param>
        /// <param name="authorUserId">The author of the test case (-1 if none)</param>
        /// <param name="ownerUserId">The owner of the test case (-1 if none)</param>
        /// <remarks>
        /// Only updates test cases not folders (because -1 could be ambigous between folders and root folder)
        /// </remarks>
        [
        WebMethod
            (
            Description = "Updates an existing test case entry in the system (only its data, not its position)",
            EnableSession = true
            )
        ]
        public void UpdateTestCase(int testCaseId, string testCaseName, int authorUserId, int ownerUserId)
        {
            const string METHOD_NAME = "UpdateTestCase";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int userId = (int)Session[Session_Authenticated];

            //Make sure we are connected to a project
            if (Session[Session_Authorized] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int projectId = (int)Session[Session_Authorized];

            //Make sure we have permissions to update test cases
            if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Modify) == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException("Not Authorized to Modify Test Cases",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            try
            {
                //First retrieve the test case dataset for the passed-in id
                TestCaseManager testCaseManager = new TestCaseManager();
                TestCase testCase = testCaseManager.RetrieveById2(projectId, testCaseId);
                if (testCase == null)
                {
                    throw new SoapException("Cannot find the supplied test case id in the system",
                        SoapException.ClientFaultCode,
                        Context.Request.Url.AbsoluteUri);
                }

                //Make sure that the project ids match
                if (testCase.ProjectId != projectId)
                {
                    throw new SoapException("The test case does not belong to the authorized project",
                        SoapException.ClientFaultCode,
                        Context.Request.Url.AbsoluteUri);
                }

                //Make the data updates
                testCase.StartTracking();
                testCase.Name = testCaseName;
                testCase.OwnerId = ownerUserId;
                testCase.AuthorId = authorUserId;

                //Handle any nullable values correctly
                if (ownerUserId == -1)
                {
                    testCase.OwnerId = null;
                }

                //Extract changes for use in notifications
                Dictionary<string, object> changes = testCase.ExtractChanges();

                //Now commit the updates
                testCaseManager.Update(testCase, userId);

                //Call notifications..
                try
                {
                    testCase.ApplyChanges(changes);
                    new NotificationManager().SendNotificationForArtifact(testCase, null, null);
                }
                catch (Exception ex)
                {
                    Logger.LogErrorEvent(METHOD_NAME, ex, "Sending message for " + testCase.ArtifactToken);
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Adds a new requirement record to the current project using the parent requirement method
        /// </summary>
        /// <param name="importanceId">The importance level of the requirement</param>
        /// <param name="requirementName">The requirement name</param>
        /// <param name="statusId">The status of the requirement</param>
        /// <param name="authorUserId">The author of the requirement (-1 for none)</param>
        /// <param name="parentRequirementId">The parent requirement (-1 if none)</param>
        /// <returns>The ID of the newly added requirement</returns>
        [
        WebMethod
            (
            Description = "Adds a new requirement record to the current project using the parent requirement method",
            EnableSession = true
            )
        ]
        public int AddRequirement2(int releaseId, int statusId, int importanceId, string requirementName, string requirementDescription, int parentRequirementId, int authorUserId)
        {
            const string METHOD_NAME = "AddRequirement2";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int userId = (int)Session[Session_Authenticated];

            //Make sure we are connected to a project
            if (Session[Session_Authorized] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int projectId = (int)Session[Session_Authorized];
            int requirementId = -1;

            //Make sure we have permissions to create requirements
            if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Requirement, (int)Project.PermissionEnum.Create) == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException("Not Authorized to Create Requirements",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            try
            {
                //Make sure the name isn't too long and HTML encode
                requirementName = Truncate(GlobalFunctions.HtmlScrubInput(requirementName), 255);

                //Default to the authenticated user if we have no author provided
                if (authorUserId == -1)
                {
                    authorUserId = userId;
                }

                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //See if we have any IDs that used to be static and that are now template/project based
                //If so, we need to convert them
                if (importanceId >= 1 && importanceId <= 4)
                {
                    ConvertRequirementLegacyStaticIds(projectTemplateId, ref importanceId);
                }

                //If we have a passed in parent requirement, then we need to insert the requirement as a child item
                Business.RequirementManager requirementManager = new Business.RequirementManager();
                if (parentRequirementId < 1)
                {
                    //Now insert the requirement at the end of the list
                    requirementId = requirementManager.Insert(userId,
                        projectId,
                        (releaseId < 1) ? null : (int?)releaseId,
                        null,
                        (int?)null,
                        (Requirement.RequirementStatusEnum)statusId,
                        null,
                        authorUserId,
                        null,
                        importanceId,
                        requirementName,
                        requirementDescription,
                        null,
                        userId);
                }
                else
                {
                    requirementId = requirementManager.InsertChild(userId,
                        projectId,
                        (releaseId < 1) ? null : (int?)releaseId,
                        null,
                        (parentRequirementId < 1) ? null : (int?)parentRequirementId,
                        (Requirement.RequirementStatusEnum)statusId,
                        null,
                        authorUserId,
                        null,
                        importanceId,
                        requirementName,
                        requirementDescription,
                        null,
                        userId);
                }

                //Send a notification
                requirementManager.SendCreationNotification(requirementId, null, null);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
            return requirementId;
        }

        /// <summary>
        /// Adds a new test folder to the current project
        /// </summary>
        /// <param name="folderName">The test folder name</param>
        /// <param name="folderDescription">The test folder detailed description ("" if none)</param>
        /// <param name="parentTestFolderId">The parent test folder (-1 if none)</param>
        /// <param name="authorUserId">The author of the test folder (-1 if none) [not used]</param>
        /// <param name="ownerUserId">The owner of the test folder (-1 if none) [not used]</param>
        /// <param name="priorityId">The priority of the test folder (-1 if none) [not used]</param>
        /// <returns>The ID of the folder created</returns>
        [
        WebMethod
            (
            Description = "Adds a new test folder to the current project",
            EnableSession = true
            )
        ]
        public int AddTestFolder(string folderName, string folderDescription, int parentTestFolderId, int authorUserId, int ownerUserId, int priorityId)
        {
            const string METHOD_NAME = "AddTestFolder";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int userId = (int)Session[Session_Authenticated];

            //Make sure we are connected to a project
            if (Session[Session_Authorized] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int projectId = (int)Session[Session_Authorized];

            //Make sure we have permissions to create test cases
            if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Create) == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException("Not Authorized to Create Test Folders",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            try
            {
                //Default to the authenticated user if we have no author provided
                if (authorUserId == -1)
                {
                    authorUserId = userId;
                }
                //Default to the authenticated user if we have no owner provided
                if (ownerUserId == -1)
                {
                    ownerUserId = userId;
                }

                //Make sure the name isn't too long
                folderName = Truncate(GlobalFunctions.HtmlScrubInput(folderName), 255);

                //Instantiate the test case business class
                TestCaseManager testCaseManager = new TestCaseManager();
                int testFolderId = 0;
                //See if we have a passed-in parent folder or not
                if (parentTestFolderId == -1)
                {
                    //Now insert the test folder at the end
                    testFolderId = testCaseManager.TestCaseFolder_Create(folderName, projectId, folderDescription).TestCaseFolderId;
                }
                else
                {
                    //Now insert the test folder under the parent
                    testFolderId = testCaseManager.TestCaseFolder_Create(folderName, projectId, folderDescription, parentTestFolderId).TestCaseFolderId;
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return testFolderId;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

 		/// <summary>
		/// Adds a new attachment into the system and associates it with the provided artifact
		/// </summary>
		/// <param name="filename">The filename of the attachment</param>
		/// <param name="description">An optional detailed description of the attachment</param>
		/// <param name="authorId">The uploader of the attachment</param>
		/// <param name="binaryData">A byte-array containing the attachment itself in binary form</param>
		/// <param name="artifactId">The id of the artifact to associate the attachment with</param>
		/// <param name="artifactType">The type of artifact to associate the attachment with</param>
		/// <returns>The id of the attachment</returns>
        [
        WebMethod
            (
            Description = "Adds a new attachment into the system and associates it with the provided artifact",
            EnableSession = true
            )
        ]
        public int AddAttachment(string filename, string description, int authorId, byte[] binaryData, int artifactId, int artifactTypeId)
        {
            const string METHOD_NAME = "AddAttachment";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int userId = (int)Session[Session_Authenticated];

            //Make sure we are connected to a project
            if (Session[Session_Authorized] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int projectId = (int)Session[Session_Authorized];
            int attachmentId = -1;

            //Make sure we have permissions to add documents
            if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Document, (int)Project.PermissionEnum.Create) == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotAuthorizedAddDocuments,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            try
            {
                //Default to the authenticated user if no author provided
                if (authorId == -1)
                {
                    authorId = userId;
                }

                //Now insert the attachment
                Business.AttachmentManager attachmentManager = new Business.AttachmentManager();
                attachmentId = attachmentManager.Insert(projectId, filename, description, authorId, binaryData, artifactId, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId, null, null, null, null, null);

                //Send a notification
                attachmentManager.SendCreationNotification(projectId, attachmentId, null, null);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
            return attachmentId;
        }

        /// <summary>
        /// Adds a new test set folder into the system
        /// </summary>
        /// <param name="parentTestSetFolderId">The id of the test set folder we want to insert within (or -1 for none)</param>
        /// <param name="creatorId">The creator's user id [not used]</param>
        /// <param name="description">The detailed description (optional)</param>
        /// <param name="name">The short name</param>
        /// <returns>The ID of the newly created test set folder</returns>
        [
        WebMethod
            (
            Description = "Adds a new test set folder into the system",
            EnableSession = true
            )
        ]
        public int AddTestSetFolder(int parentTestSetFolderId, int creatorId, string name, string description)
        {
            const string METHOD_NAME = "AddTestSetFolder";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int userId = (int)Session[Session_Authenticated];

            //Make sure we are connected to a project
            if (Session[Session_Authorized] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int projectId = (int)Session[Session_Authorized];
            int testSetFolderId = -1;

            //Make sure we have permissions to create test sets
            if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestSet, (int)Project.PermissionEnum.Create) == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException("Not Authorized to Create Test Folders",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            try
            {
                //Default to the authenticated user if no creator provided
                if (creatorId == -1)
                {
                    creatorId = userId;
                }

                //Now insert the test set folder
                TestSetManager testSetManager = new TestSetManager(); 
                //See if we have a passed-in parent folder or not
                if (parentTestSetFolderId == -1)
                {
                    //Now insert the test set folder at the root
                    testSetFolderId = testSetManager.TestSetFolder_Create(name, projectId, description).TestSetFolderId;
                }
                else
                {
                    //Now insert the test set folder under the specified parent folder
                    testSetFolderId = testSetManager.TestSetFolder_Create(name, projectId, description, parentTestSetFolderId).TestSetFolderId;
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
            return testSetFolderId;
        }


        /// <summary>
        /// Adds a new test set into the system
        /// </summary>
        /// <param name="parentTestSetFolderId">The id of the test set folder we want to insert within (or -1 for none)</param>
        /// <param name="creatorId">The creator's user id</param>
        /// <param name="description">The detailed description (optional)</param>
        /// <param name="name">The short name</param>
        /// <param name="ownerId">The owner's user id (optional)</param>
        /// <param name="plannedDate">The planned date to execute (optional)</param>
        /// <param name="isPlannedDateNull">Is the planned date null</param>
        /// <param name="releaseId">The release it should be targeted against (optional)</param>
        /// <param name="testSetStatusId">The test set's status</param>
        /// <returns>The ID of the newly created test set</returns>
        [
        WebMethod
            (
            Description = "Adds a new test set into the system at a point in front of the passed in test set",
            EnableSession = true
            )
        ]
        public int AddTestSet(int parentTestSetFolderId, int releaseId, int creatorId, int ownerId, int testSetStatusId, string name, string description, DateTime plannedDate, bool isPlannedDateNull)
        {
            const string METHOD_NAME = "AddTestSet";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int userId = (int)Session[Session_Authenticated];

            //Make sure we are connected to a project
            if (Session[Session_Authorized] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int projectId = (int)Session[Session_Authorized];
            int testSetId = -1;

            //Make sure we have permissions to create test sets
            if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestSet, (int)Project.PermissionEnum.Create) == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException("Not Authorized to Create Test Sets",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            try
            {
                //Default to the authenticated user if no creator provided
                if (creatorId == -1)
                {
                    creatorId = userId;
                }

                //Insert the test set
                TestSetManager testSetManager = new TestSetManager();
                testSetId = testSetManager.Insert(
                    userId,
                    projectId,
                    (parentTestSetFolderId > 0) ? (int?)parentTestSetFolderId : null,
                    (releaseId > 0) ? (int?)releaseId : null,
                    creatorId,
                    (ownerId > 0) ? (int?)ownerId : null,
                    (TestSet.TestSetStatusEnum)testSetStatusId,
                    name,
                    description,
                    (isPlannedDateNull) ? null : (DateTime?)plannedDate,
                    TestRun.TestRunTypeEnum.Manual,
                    null,
                    null);

                //Send a notification
                testSetManager.SendCreationNotification(testSetId, null, null);

            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
            return testSetId;
        }

        /// <summary>
        /// Adds a test case to a test set
        /// </summary>
        /// <param name="testCaseId">The test case we want to add</param>
        /// <param name="testSetId">The test set in question</param>
        /// <remarks>
        /// If the we are passed in the id of a test folder, we add all the child test cases,
        /// ignoring any duplicates.
        /// </remarks>
        [
        WebMethod
            (
            Description = "Adds a test case to a test set",
            EnableSession = true
            )
        ]
        public void AddTestSetTestCaseMapping(int testSetId, int testCaseId)
        {
            const string METHOD_NAME = "AddTestSetTestCaseMapping";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int userId = (int)Session[Session_Authenticated];

            //Make sure we are connected to a project
            if (Session[Session_Authorized] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int projectId = (int)Session[Session_Authorized];

            //Make sure we have permissions to update test cases
            if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Modify) == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException("Not Authorized to Modify Test Cases",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            try
            {
                //Now add the test case to the test set
                TestSetManager testSetManager = new TestSetManager();
                testSetManager.AddTestCases(projectId, testSetId, new List<int>() { testCaseId }, null, null);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Adds a new test case to the current project
        /// </summary>
        /// <param name="testCaseName">The test case name</param>
        /// <param name="parentTestFolderId">The parent test folder (-1 if none)</param>
        /// <param name="authorUserId">The author of the test case (-1 if none)</param>
        /// <param name="ownerUserId">The owner of the test case (-1 if none)</param>
        /// <param name="activeTestCase">Whether the test case is active or not</param>
        /// <param name="testCaseDescription">The description of the test case ("" if none)</param>
        /// <param name="estimatedDuration">The estimated duration (-1 if none)</param>
        /// <param name="priorityId">The priority of the test case (-1 if none)</param>
        /// <returns>the id of the test case created</returns>
        [
        WebMethod
            (
            Description = "Adds a new test case to the current project",
            EnableSession = true
            )
        ]
        public int AddTestCase(string testCaseName, string testCaseDescription, int parentTestFolderId, int authorUserId, int ownerUserId, bool activeTestCase, int priorityId, int estimatedDuration)
        {
            const string METHOD_NAME = "AddTestCase";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int userId = (int)Session[Session_Authenticated];

            //Make sure we are connected to a project
            if (Session[Session_Authorized] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int projectId = (int)Session[Session_Authorized];

            //Make sure we have permissions to create test cases
            if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Create) == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException("Not Authorized to Create Test Cases",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            try
            {
                //Default to the authenticated user if we have no author provided
                if (authorUserId == -1)
                {
                    authorUserId = userId;
                }

                //Make sure the name isn't too long
                testCaseName = Truncate(GlobalFunctions.HtmlScrubInput(testCaseName), 255);

                //Instantiate the test case business class
                TestCaseManager testCaseManager = new TestCaseManager();

                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //See if we have any IDs that used to be static and that are now template/project based
                //If so, we need to convert them
                if (priorityId >= 1 && priorityId <= 4)
                {
                    ConvertTestCaseLegacyStaticIds(projectTemplateId, ref priorityId);
                }

                //See if we have a passed in parent folder or not
                int testCaseId = testCaseManager.Insert(userId,
                    projectId,
                    authorUserId,
                    (ownerUserId > 0) ? (int?)ownerUserId : null,
                    testCaseName,
                    testCaseDescription,
                    null,
                    (activeTestCase) ? TestCase.TestCaseStatusEnum.ReadyForTest : TestCase.TestCaseStatusEnum.Obsolete,
                    (priorityId > 0) ? (int?)priorityId : null,
                    (parentTestFolderId > 0) ? (int?)parentTestFolderId : null,
                    (estimatedDuration == -1) ? null : (int?)estimatedDuration,
                    null,
                    null);

                //Send a notification
                testCaseManager.SendCreationNotification(testCaseId, null, null);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return testCaseId;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Adds a new test step to the test case specified
        /// </summary>
        /// <param name="description">The test step description</param>
        /// <param name="testCaseId">The test case to add it to</param>
        /// <param name="expectedResult">The expected result</param>
        /// <param name="position">The test step position number (i.e. the step ordering)</param>
        /// <param name="sampleData">The sample data</param>
        /// <returns>The ID of the test step created</returns>
        [
        WebMethod
            (
            Description = "Adds a new test step to the test case specified",
            EnableSession = true
            )
        ]
        public int AddTestStep(int testCaseId, int position, string description, string expectedResult, string sampleData)
        {
            const string METHOD_NAME = "AddTestStep";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int userId = (int)Session[Session_Authenticated];

            //Make sure we are connected to a project
            if (Session[Session_Authorized] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int projectId = (int)Session[Session_Authorized];

            //Make sure we have permissions to create test steps
            if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestStep, (int)Project.PermissionEnum.Create) == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException("Not Authorized to Create Test Steps",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            try
            {
                //Instantiate the test case business class
                TestCaseManager testCaseManager = new TestCaseManager();

                //Make sure the description, expected result and sample data aren't too long
                description = Truncate(description, 2000);
                expectedResult = Truncate(expectedResult, 2000);
                sampleData = Truncate(sampleData, 1000);

                //Now insert the test step
                int testStepId = testCaseManager.InsertStep(userId, testCaseId, position, description, expectedResult, sampleData);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return testStepId;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Gets the base URL of the website that SpiraTest is running under. Used for notifications and exported artifacts
        /// </summary>
        /// <returns>The URL</returns>
        [
        WebMethod
            (
            Description = "Gets the base URL of the website that SpiraTest is running under. Used for notifications and exported artifacts",
            EnableSession = true
            )
        ]
        public string GetWebServerUrl()
        {
            return ConfigurationSettings.Default.General_WebServerUrl;
        }

        /// <summary>
        /// Returns the full token of a test caseparameter from its name
        /// </summary>
        /// <param name="parameterName">The name of the parameter</param>
        /// <returns>The tokenized representation of the parameter used for search/replace</returns>
        /// <remarks>We use the same parameter format as Ant/NAnt</remarks>
        [
        WebMethod
            (
            Description = "Returns the full token of a test caseparameter from its name",
            EnableSession = true
            )
        ]
        public string CreateTestCaseParameterToken(string parameterName)
        {
            return TestCaseManager.CreateParameterToken(parameterName);
        }

        /// <summary>
        /// Adds a new parameter for a test case
        /// </summary>
        /// <param name="testCaseId">The test case in question</param>
        /// <param name="name">The name of the parameter</param>
        /// <param name="defaultValue">The default value of the parameter (optional)</param>
        /// <returns>The id of the new parameter</returns>
        /// <remarks>The parameter name is always made lower case</remarks>
        [
        WebMethod
            (
            Description = "Adds a new parameter for a test case",
            EnableSession = true
            )
        ]
        public int AddTestCaseParameter(int testCaseId, string name, string defaultValue)
        {
            const string METHOD_NAME = "AddTestStepLink";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int userId = (int)Session[Session_Authenticated];

            //Make sure we are connected to a project
            if (Session[Session_Authorized] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int projectId = (int)Session[Session_Authorized];

            //Make sure we have permissions to update test cases
            if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.TestCase, (int)Project.PermissionEnum.Modify) == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException("Not Authorized to Modify Test Cases",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            try
            {
                //Instantiate the test case business class
                TestCaseManager testCaseManager = new TestCaseManager();

                //Now insert the test case parameter
                int testCaseParameterId = testCaseManager.InsertParameter(projectId, testCaseId, name, defaultValue);

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return testCaseParameterId;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }
 
        /// <summary>
        /// Adds a new custom list into the system
        /// </summary>
        /// <param name="name">The display name of the custom list</param>
        /// <param name="active">Whether the custom list is active or not</param>
        /// <returns>The newly created custom property value id</returns>
        [
        WebMethod
            (
            Description = "Adds a new custom list into the system",
            EnableSession = true
            )
        ]
        public int AddCustomList(string name, bool active)
        {
            const string METHOD_NAME = "AddCustomPropertyValue";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int userId = (int)Session[Session_Authenticated];

            //Make sure we are connected to a project
            if (Session[Session_Authorized] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int projectId = (int)Session[Session_Authorized];

            //Make sure we have permissions to retrieve custom properties (project owner)
            if (!this.IsProjectOwner)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException("Not Authorized to View Project Custom Properties - need to be Project Owner",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            try
            {
                //Instantiate the custom property business class
                Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();

                //Now insert the new custom list
                int customListId = customPropertyManager.CustomPropertyList_AddValue(projectId, name, active).CustomPropertyListId;

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return customListId;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Adds a new custom list value into the system
        /// </summary>
        /// <param name="customPropertyListId">The id of the custom list the value is being added to</param>
        /// <param name="name">The display name of the property value</param>
        /// <param name="active">Whether the property value is active or not</param>
        /// <returns>The newly created custom list value id</returns>
        [
        WebMethod
            (
            Description = "Adds a new custom list value into the system",
            EnableSession = true
            )
        ]
        public int AddCustomListValue(int customPropertyListId, string name, bool active)
        {
            const string METHOD_NAME = "AddCustomListValue";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int userId = (int)Session[Session_Authenticated];

            //Make sure we are connected to a project
            if (Session[Session_Authorized] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            //Make sure we have permissions to retrieve custom properties (project owner)
            if (!this.IsProjectOwner)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException("Not Authorized to View Project Custom Properties - need to be Project Owner",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            try
            {
                //Instantiate the custom property business class
                Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();

                //Now insert the new custom property list value
                int customPropertyValueId = customPropertyManager.CustomPropertyList_AddValue(customPropertyListId, name, active).CustomPropertyValueId;

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();
                return customPropertyValueId;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Adds new custom properties to an artifact
        /// </summary>
        /// <param name="artifactId">The id of the artifact</param>
        /// <param name="artifactTypeId">The type of the artifact (1 = Requirement, 2 = Test Case, 3 = Incident, 4 = Release)</param>
        /// <param name="text01">User-defined freetext field</param>
        /// <param name="text01">User-defined freetext field</param>
        /// <param name="text02">User-defined freetext field</param>
        /// <param name="text03">User-defined freetext field</param>
        /// <param name="text04">User-defined freetext field</param>
        /// <param name="text05">User-defined freetext field</param>
        /// <param name="text06">User-defined freetext field</param>
        /// <param name="text07">User-defined freetext field</param>
        /// <param name="text08">User-defined freetext field</param>
        /// <param name="text09">User-defined freetext field</param>
        /// <param name="text10">User-defined freetext field</param>
        /// <param name="list01">User-defined list field</param>
        /// <param name="list02">User-defined list field</param>
        /// <param name="list03">User-defined list field</param>
        /// <param name="list04">User-defined list field</param>
        /// <param name="list05">User-defined list field</param>
        /// <param name="list06">User-defined list field</param>
        /// <param name="list07">User-defined list field</param>
        /// <param name="list08">User-defined list field</param>
        /// <param name="list09">User-defined list field</param>
        /// <param name="list10">User-defined list field</param>
        [
        WebMethod
            (
            Description = "Adds new custom properties to an artifact",
            EnableSession = true
            )
        ]
        public void AddCustomProperties(
            int artifactId,
            int artifactTypeId,
            string text01,
            string text02,
            string text03,
            string text04,
            string text05,
            string text06,
            string text07,
            string text08,
            string text09,
            string text10,
            int list01,
            int list02,
            int list03,
            int list04,
            int list05,
            int list06,
            int list07,
            int list08,
            int list09,
            int list10
            )
        {
            const string METHOD_NAME = "AddCustomProperties";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int userId = (int)Session[Session_Authenticated];

            //Make sure we are connected to a project
            if (Session[Session_Authorized] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int projectId = (int)Session[Session_Authorized];

            //Get the template associated with the project
            int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

            //Make sure we have permissions to modify the artifact type's custom properties
            if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, artifactTypeId, (int)Project.PermissionEnum.Modify) == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException("Not Authorized to Modify artifacts of type=" + artifactTypeId,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            try
            {
                //First we need to get the existing artifact custom property (if there is one)
                CustomPropertyManager customPropertyManager = new CustomPropertyManager();
                ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, artifactId, (Artifact.ArtifactTypeEnum)artifactTypeId, true);
                if (artifactCustomProperty == null)
                {
                    List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactTypeId(projectTemplateId, artifactTypeId, false, false);
                    artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_CreateNew(projectId, (Artifact.ArtifactTypeEnum)artifactTypeId, artifactId, customProperties);
                }
                else
                {
                    artifactCustomProperty.StartTracking();
                }

                //Set the freetext properties
                if (text01 != "")
                {
                    artifactCustomProperty.SetLegacyCustomProperty("TEXT_01", text01);
                }
                if (text02 != "")
                {
                    artifactCustomProperty.SetLegacyCustomProperty("TEXT_02", text02);
                }
                if (text03 != "")
                {
                    artifactCustomProperty.SetLegacyCustomProperty("TEXT_03", text03);
                }
                if (text04 != "")
                {
                    artifactCustomProperty.SetLegacyCustomProperty("TEXT_04", text04);
                }
                if (text05 != "")
                {
                    artifactCustomProperty.SetLegacyCustomProperty("TEXT_05", text05);
                }
                if (text06 != "")
                {
                    artifactCustomProperty.SetLegacyCustomProperty("TEXT_06", text06);
                }
                if (text07 != "")
                {
                    artifactCustomProperty.SetLegacyCustomProperty("TEXT_07", text07);
                }
                if (text08 != "")
                {
                    artifactCustomProperty.SetLegacyCustomProperty("TEXT_08", text08);
                }
                if (text09 != "")
                {
                    artifactCustomProperty.SetLegacyCustomProperty("TEXT_09", text09);
                }
                if (text10 != "")
                {
                    artifactCustomProperty.SetLegacyCustomProperty("TEXT_10", text10);
                }

                //Set the list properties
                if (list01 != -1)
                {
                    artifactCustomProperty.SetLegacyCustomProperty("LIST_01", list01);
                }
                if (list02 != -1)
                {
                    artifactCustomProperty.SetLegacyCustomProperty("LIST_02", list02);
                }
                if (list03 != -1)
                {
                    artifactCustomProperty.SetLegacyCustomProperty("LIST_03", list03);
                }
                if (list04 != -1)
                {
                    artifactCustomProperty.SetLegacyCustomProperty("LIST_04", list04);
                }
                if (list05 != -1)
                {
                    artifactCustomProperty.SetLegacyCustomProperty("LIST_05", list05);
                }
                if (list06 != -1)
                {
                    artifactCustomProperty.SetLegacyCustomProperty("LIST_06", list06);
                }
                if (list07 != -1)
                {
                    artifactCustomProperty.SetLegacyCustomProperty("LIST_07", list07);
                }
                if (list08 != -1)
                {
                    artifactCustomProperty.SetLegacyCustomProperty("LIST_08", list08);
                }
                if (list09 != -1)
                {
                    artifactCustomProperty.SetLegacyCustomProperty("LIST_09", list09);
                }
                if (list10 != -1)
                {
                    artifactCustomProperty.SetLegacyCustomProperty("LIST_10", list10);
                }

                //Now add the row and commit to the database
                customPropertyManager.ArtifactCustomProperty_Save(artifactCustomProperty, userId);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Adds a new incident severity to the current project
        /// </summary>
        /// <param name="name">The name of the severity</param>
        /// <param name="colorCode">A color code for it (RRGGBB hex format)</param>
        /// <returns>The id of the new incident severity</returns>
        [
        WebMethod
            (
            Description = "Adds a new incident severity to the current project",
            EnableSession = true
            )
        ]
        public int AddIncidentSeverity(string name, string colorCode)
        {
            const string METHOD_NAME = "AddIncidentSeverity";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int userId = (int)Session[Session_Authenticated];

            //Make sure we are connected to a project
            if (Session[Session_Authorized] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int projectId = (int)Session[Session_Authorized];
            int severityId = -1;

            //Make sure we have permissions to retrieve custom properties (project owner)
            if (!this.IsProjectOwner)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException("Not Authorized to View Project Custom Properties - need to be Project Owner",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Make sure the name isn't too long
                name = Truncate(GlobalFunctions.HtmlScrubInput(name), 20);
                colorCode = Truncate(GlobalFunctions.HtmlScrubInput(colorCode), 6);

                //Now insert the incident severity
                IncidentManager incidentManager = new IncidentManager();
                severityId = incidentManager.InsertIncidentSeverity(projectTemplateId, name, colorCode, true);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
            return severityId;
        }

        /// <summary>
        /// Adds a new incident type to the current project
        /// </summary>
        /// <param name="name">The name of the type</param>
        /// <returns>The id of the new incident type</returns>
        [
        WebMethod
            (
            Description = "Adds a new incident type to the current project",
            EnableSession = true
            )
        ]
        public int AddIncidentType(string name)
        {
            const string METHOD_NAME = "AddIncidentType";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int userId = (int)Session[Session_Authenticated];

            //Make sure we are connected to a project
            if (Session[Session_Authorized] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int projectId = (int)Session[Session_Authorized];
            int incidentTypeId = -1;

            //Make sure we have permissions to retrieve custom properties (project owner)
            if (!this.IsProjectOwner)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException("Not Authorized to View Project Custom Properties - need to be Project Owner",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Make sure the name isn't too long
                name = Truncate(GlobalFunctions.HtmlScrubInput(name), 20);

                //Now insert the incident type
                IncidentManager incidentManager = new IncidentManager();
                incidentTypeId = incidentManager.InsertIncidentType(projectTemplateId, name, null, false, false, false, true);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
            return incidentTypeId;
        }

        /// <summary>
        /// Adds a new incident status to the current project
        /// </summary>
        /// <param name="name">The name of the status</param>
        /// <returns>The id of the new incident status</returns>
        [
        WebMethod
            (
            Description = "Adds a new incident status to the current project",
            EnableSession = true
            )
        ]
        public int AddIncidentStatus(string name)
        {
            const string METHOD_NAME = "AddIncidentStatus";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int userId = (int)Session[Session_Authenticated];

            //Make sure we are connected to a project
            if (Session[Session_Authorized] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int projectId = (int)Session[Session_Authorized];
            int incidentStatusId = -1;

            //Make sure we have permissions to retrieve custom properties (project owner)
            if (!this.IsProjectOwner)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException("Not Authorized to View Project Custom Properties - need to be Project Owner",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Make sure the name isn't too long
                name = Truncate(GlobalFunctions.HtmlScrubInput(name), 20);

                //Now insert the incident status
                IncidentManager incidentManager = new IncidentManager();
                incidentStatusId = incidentManager.IncidentStatus_Insert(projectTemplateId, name, true, false, true);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
            return incidentStatusId;
        }

        /// <summary>
        /// Adds a new incident priority to the current project
        /// </summary>
        /// <param name="name">The name of the priority</param>
        /// <param name="colorCode">A color code for it (RRGGBB hex format)</param>
        /// <returns>The id of the new incident priority</returns>
        [
        WebMethod
            (
            Description = "Adds a new incident priority to the current project",
            EnableSession = true
            )
        ]
        public int AddIncidentPriority(string name, string colorCode)
        {
            const string METHOD_NAME = "AddIncidentPriority";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int userId = (int)Session[Session_Authenticated];

            //Make sure we are connected to a project
            if (Session[Session_Authorized] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int projectId = (int)Session[Session_Authorized];
            int priorityId = -1;

            //Make sure we have permissions to retrieve custom properties (project owner)
            if (!this.IsProjectOwner)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException("Not Authorized to View Project Custom Properties - need to be Project Owner",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            try
            {
                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //Make sure the name isn't too long
                name = Truncate(GlobalFunctions.HtmlScrubInput(name), 20);
                colorCode = Truncate(GlobalFunctions.HtmlScrubInput(colorCode), 6);

                //Now insert the incident priority
                IncidentManager incidentManager = new IncidentManager();
                priorityId = incidentManager.InsertIncidentPriority(projectTemplateId, name, colorCode, true);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
            return priorityId;
        }

        /// <summary>
        /// Inserts a new resolution into an existing incident
        /// </summary>
        /// <param name="incidentId">The incident to add the resolution to</param>
        /// <param name="resolution">The description of the resolution/comment</param>
        /// <param name="creationDate">The date that the resolution was added/created</param>
        /// <param name="creatorId">The user who's adding the resolution</param>
        /// <returns>The ID of the incident resolution</returns>
        [
        WebMethod
            (
            Description = "Adds a new resolution to an existing incident",
            EnableSession = true
            )
        ]
        public int AddIncidentResolution(int incidentId, string resolution, DateTime creationDate, int creatorId)
        {
            const string METHOD_NAME = "AddIncidentResolution";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int userId = (int)Session[Session_Authenticated];

            //Make sure we are connected to a project
            if (Session[Session_Authorized] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int projectId = (int)Session[Session_Authorized];
            int incidentResolutionId = -1;

            //Make sure we have permissions to retrieve custom properties (project owner)
            if (!this.IsProjectOwner)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException("Not Authorized to View Project Custom Properties - need to be Project Owner",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            try
            {
                //Default to the authenticated user if no creator provided
                if (creatorId == -1)
                {
                    creatorId = userId;
                }

                //Convert any dates from localtime to UTC
                creationDate = GlobalFunctions.UniversalizeDate(creationDate);

                //Now insert the incident resolution
                IncidentManager incidentManager = new IncidentManager();
                incidentResolutionId = incidentManager.InsertResolution(incidentId, resolution, creationDate, creatorId,true);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
            return incidentResolutionId;
        }

        /// <summary>
        /// Adds a new incident to the current project
        /// </summary>
        /// <param name="priorityId">The priority level of the incident</param>
        /// <param name="severityId">The severity level of the incident</param>
        /// <param name="name">The incident name</param>
        /// <param name="description">The detailed description of the incident</param>
        /// <param name="statusId">The incident status</param>
        /// <param name="typeId">The incident type</param>
        /// <param name="testRunStepId">The test run step to map to (-1 if none)</param>
        /// <param name="detectedByUserId">The user who detected the incident (-1 if none)</param>
        /// <param name="ownerUserId">The user who owns the incident (-1 if none)</param>
        /// <param name="closedDate">The date that the incident was closed (if in one of the closed status codes)</param>
        /// <returns>The ID of the incident created</returns>
        [
        WebMethod
            (
            Description = "Adds a new incident to the current project",
            EnableSession = true
            )
        ]
        public int AddIncident(int typeId, int priorityId, int severityId, int statusId, string name, string description, int testRunStepId, int detectedByUserId, int ownerUserId, DateTime closedDate, bool isClosed)
        {
            const string METHOD_NAME = "AddIncident";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int userId = (int)Session[Session_Authenticated];

            //Make sure we are connected to a project
            if (Session[Session_Authorized] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int projectId = (int)Session[Session_Authorized];
            int incidentId = -1;

            //Make sure we have permissions to create incidents
            if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Incident, (int)Project.PermissionEnum.Create) == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException("Not Authorized to Create Incidents",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            try
            {
                //Make sure the name isn't
                name = Truncate(GlobalFunctions.HtmlScrubInput(name), 255);

                //Default to the authenticated user if no detector provided
                if (detectedByUserId == -1)
                {
                    detectedByUserId = userId;
                }

                //Handle the nullables (this old API uses -1 instead of null since was based on .NET 1.1)
                Nullable<int> priorityIdNullable = (priorityId == -1) ? null : (Nullable<int>)priorityId;
                Nullable<int> severityIdNullable = (severityId == -1) ? null : (Nullable<int>)severityId;
                Nullable<int> ownerUserIdNullable = (ownerUserId == -1) ? null : (Nullable<int>)ownerUserId;
                Nullable<int> testRunStepIdNullable = (testRunStepId == -1) ? null : (Nullable<int>)testRunStepId;
                Nullable<int> typeIdNullable = (typeId == -1) ? null : (Nullable<int>)typeId;
                Nullable<int> statusIdNullable = (statusId == -1) ? null : (Nullable<int>)statusId;

                //If the closed flag is not set, make the closed-date null
                Nullable<DateTime> closedDateNullable;
                if (!isClosed)
                {
                    closedDateNullable = null;
                }
                else
                {
                    //Convert any dates from localtime to UTC
                    closedDateNullable = GlobalFunctions.UniversalizeDate(closedDate);
                }

                //Now insert the incident - the API doesn't support association with specific releases
                IncidentManager incidentManager = new IncidentManager();
				incidentId = incidentManager.Insert(
					projectId, 
					priorityIdNullable, 
					severityIdNullable, 
					detectedByUserId,
					ownerUserIdNullable,
					(testRunStepIdNullable.HasValue) ? new List<int>() { testRunStepIdNullable.Value } : null,
					name,
					description,
					null,
					null, 
					null, 
					typeIdNullable, 
					statusIdNullable, 
					DateTime.UtcNow,
					null, 
					closedDateNullable, 
					null, 
					null, 
					null,
                    null,
                    null,
					userId);

                //Send a notification
                incidentManager.SendCreationNotification(incidentId, null, null);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
            return incidentId;
        }


        /// <summary>
        /// Adds a new task to the current project
        /// </summary>
        /// <returns>The ID of the task created</returns>
        /// <param name="name">The name of the task</param>
        /// <param name="description">The detailed description of the task</param>
        /// <param name="taskStatusId">The status</param>
        /// <param name="taskPriorityId">The priority (optional)</param>
        /// <param name="requirementId">The requirement that the task is linked to (optional)</param>
        /// <param name="releaseId">The release that the task is scheduled for (optional)</param>
        /// <param name="ownerId">The owner of the task (optional)</param>
        /// <param name="startDate">The date the task starts (optional)</param>
        /// <param name="endDate">The date the task ends (optional)</param>
        /// <param name="completionPercentage">The completion percentage</param>
        /// <param name="estimatedEffort">The estimated effort (optional)</param>
        /// <param name="actualEffort">The actual effort (optional)</param>
        [
        WebMethod
            (
            Description = "Adds a new task to the current project",
            EnableSession = true
            )
        ]
        public int AddTask(string name, string description, int taskStatusId, int taskPriorityId, int requirementId, int releaseId, int ownerId, Nullable<DateTime> startDate,  Nullable<DateTime> endDate, int completionPercentage, int estimatedEffort, int actualEffort)
        {
            const string METHOD_NAME = "AddTask";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we have an authenticated user
            if (Session[Session_Authenticated] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_SessionNotAuthenticated,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int userId = (int)Session[Session_Authenticated];

            //Make sure we are connected to a project
            if (Session[Session_Authorized] == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException(Resources.Messages.Services_NotConnectedToProject,
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }
            int projectId = (int)Session[Session_Authorized];
            int taskId = -1;

            //Make sure we have permissions to create tasks
            if (this.ProjectRolePermissions == null || this.ProjectRolePermissions.FindByProjectRoleIdArtifactTypeIdPermissionId(this.ProjectRoleId, (int)DataModel.Artifact.ArtifactTypeEnum.Task, (int)Project.PermissionEnum.Create) == null)
            {
                //Throw back an exception
                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                Logger.Flush();

                throw new SoapException("Not Authorized to Create Tasks",
                    SoapException.ClientFaultCode,
                    Context.Request.Url.AbsoluteUri);
            }

            try
            {
                //Make sure the name isn't too long
                name = Truncate(GlobalFunctions.HtmlScrubInput(name), 255);

                //Make sure the description is safe
                description = GlobalFunctions.HtmlScrubInput(description);

                //Convert %complete into remaining effort values
                Nullable<int> remainingEffort = null;
                Nullable<int> estimatedEffortNullable = null;
                Nullable<int> actualEffortNullable = null;
                if (estimatedEffort != -1)
                {
                    estimatedEffortNullable = estimatedEffort;
                    remainingEffort = estimatedEffort * (100 - completionPercentage);
                    remainingEffort /= 100;
                }
                if (actualEffort != -1)
                {
                    actualEffortNullable = actualEffort;
                }

                //Convert any dates from localtime to UTC
                startDate = GlobalFunctions.UniversalizeDate(startDate);
                endDate = GlobalFunctions.UniversalizeDate(endDate);

                //Get the template associated with the project
                int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

                //See if we have any IDs that used to be static and that are now template/project based
                //If so, we need to convert them
                if (taskPriorityId >= 1 && taskPriorityId <= 4)
                {
                    ConvertTaskLegacyStaticIds(projectTemplateId, ref taskPriorityId);
                }

                //Now insert the task and capture the task id
                Business.TaskManager taskManager = new Business.TaskManager();
                taskId = taskManager.Insert(
                    projectId,
                    userId,
                    (Task.TaskStatusEnum)taskStatusId,
                    null,
                    null,
                    (requirementId == -1) ? null : (Nullable<int>)requirementId,
                    (releaseId == -1) ? null : (Nullable<int>)releaseId,
                    (ownerId == -1) ? null : (Nullable<int>)ownerId,
                    (taskPriorityId == -1) ? null : (int?)taskPriorityId,
                    name,
                    description,
                    startDate,
                    endDate,
                    estimatedEffortNullable,
                    actualEffortNullable,
                    remainingEffort,
					userId
                    );

                //Send a notification
                taskManager.SendCreationNotification(taskId, null, null);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
            return taskId;
        }

        #region Data Translation Methods

        /// <summary>
        /// Converts a requirement static ID to be template-based
        /// </summary>
        /// <param name="projectTemplateId">The id of the project template</param>
        /// <param name="importanceId">The requirement importance</param>
        public static void ConvertRequirementLegacyStaticIds(int projectTemplateId, ref int importanceId)
        {
            //First importance
            int importanceIdInput = importanceId;
            RequirementManager requirementManager = new RequirementManager();
            List<Importance> importances = requirementManager.RequirementImportance_Retrieve(projectTemplateId);
            Importance importance = importances.FirstOrDefault(p => p.Score == importanceIdInput || p.ImportanceId == importanceIdInput);
            if (importance == null)
            {
                //No match, so leave blank
                importanceId = -1;
            }
            else
            {
                importanceId = importance.ImportanceId;
            }
        }

        /// <summary>
        /// Converts a task static ID to be template-based
        /// </summary>
        /// <param name="projectTemplateId">The id of the project template</param>
        /// <param name="priorityId">The task priority</param>
        public static void ConvertTaskLegacyStaticIds(int projectTemplateId, ref int priorityId)
        {
            //First priority
            int priorityIdInput = priorityId;
            TaskManager taskManager = new TaskManager();
            List<TaskPriority> priorities = taskManager.TaskPriority_Retrieve(projectTemplateId);
            TaskPriority priority = priorities.FirstOrDefault(p => p.Score == priorityIdInput || p.TaskPriorityId == priorityIdInput);
            if (priority == null)
            {
                //No match, so leave blank
                priorityId = -1;
            }
            else
            {
                priorityId = priority.TaskPriorityId;
            }
        }

        /// <summary>
        /// Converts a task static ID to be template-based
        /// </summary>
        /// <param name="projectTemplateId">The id of the project template</param>
        /// <param name="priorityId">The task priority</param>
        public static void ConvertTestCaseLegacyStaticIds(int projectTemplateId, ref int priorityId)
        {
            //First priority
            int priorityIdInput = priorityId;
            TestCaseManager taskManager = new TestCaseManager();
            List<TestCasePriority> priorities = taskManager.TestCasePriority_Retrieve(projectTemplateId);
            TestCasePriority priority = priorities.FirstOrDefault(p => p.Score == priorityIdInput || p.TestCasePriorityId == priorityIdInput);
            if (priority == null)
            {
                //No match, so leave blank
                priorityId = -1;
            }
            else
            {
                priorityId = priority.TestCasePriorityId;
            }
        }

        #endregion
    }
}
