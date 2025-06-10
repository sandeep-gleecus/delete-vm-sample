using Inflectra.SpiraTest.Business.GlobalResources;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.PlugIns.VersionControlPlugIn2;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inflectra.SpiraTest.Business
{
	///This file contains API call code for TV admin functions.
	public partial class VaultManager : ManagerBase
	{
		//Certain constants used when displaying revisions in conjunction with real Spira artifacts
		public const string REVISION_ARTIFACT_TYPE_NAME = "Revision";
		public const int REVISION_ARTIFACT_TYPE_ID = -2;

        /// <summary>
        /// Sometimes ProjectLocker will already be using the same name, so suffix with _tv
        /// </summary>
        public const string OPTIONAL_ACCOUNT_NAME_SUFFIX = "_tv";

		#region API Variables
		/// <summary>The key used to 'log in' to the ProjectLocker API</summary>
		private const string API_KEY = "test.xml"; //TODO:CS
		/// <summary>The base URL for the API calls.</summary>
		private const string API_BASEURL = "https://portal.projectlocker.com/v1";
		/// <summary>The URL prefix for running Account operations.</summary>
		private const string API_ACCOUNT = "accounts/{0}";
		/// <summary>The URL for ProjectOperations.</summary>
		private const string API_PROJECT = API_ACCOUNT + "/projects/{1}";
		/// <summary>The URL for ProjectOperations.</summary>
		private const string API_USER = API_ACCOUNT + "/users/{1}";
		/// <summary>The URL for ProjectOperations.</summary>
		private const string API_PROJECTUSER = API_PROJECT + "/users/{2}";
		#endregion


		/// <summary>The name of the special test version control provider (it lives in the main plug-ins assembly)</summary>
		public const string TEST_VERSION_CONTROL_PROVIDER_NAME = "TestVersionControlProvider";

		/// <summary>The name of the TaraVault instance name.</summary>
		public const string TARABANK_VERSION_PROVIDER_NAME = "TaraVault";

		#region Admin REST Calls
		#region Account Calls
		/// <summary>Creates an account with the specified name.</summary>
		/// <param name="accountName">The name of the account.</param>
		/// <returns>A full populated Account.</returns>
		private Account Tara_Account_Create(string accountName)
		{
			const string FUNC = CLASS_NAME + "Tara_Account_Create()";
			Logger.LogEnteringEvent(FUNC);

			Account retValue = null;

			//Create the client.
			RestClient client = CreateRestClient();

			//Create our request..
			RestRequest req = GenerateRestRequest(Method.POST, string.Format(API_ACCOUNT, ""));
			req.AddJsonBody(new Dictionary<string, string> { { "name", accountName } });

			//Create the account.
			RestResponse<Account> reply = (RestResponse<Account>)client.Execute<Account>(req);

			//See if we have an error..
            if ((int)reply.StatusCode >= 300)
            {
                if (reply.StatusCode == System.Net.HttpStatusCode.Conflict || reply.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    //If the error was the account name already exists, try a second name with a suffix
                    accountName = accountName + OPTIONAL_ACCOUNT_NAME_SUFFIX;

                    RestRequest req2 = GenerateRestRequest(Method.POST, string.Format(API_ACCOUNT, ""));
                    req2.AddJsonBody(new Dictionary<string, string> { { "name", accountName } });

                    //Create the account.
                    RestResponse<Account> reply2 = (RestResponse<Account>)client.Execute<Account>(req2);

                    if ((int)reply2.StatusCode >= 300)
                    {
                        throw GenerateExFromResp((int)reply2.StatusCode, reply2.StatusDescription, reply2.Content, reply2.Request.Parameters);
                    }
                    else
                    {
                        retValue = reply2.Data;
                    }
                }
                else
                {
                    throw GenerateExFromResp((int)reply.StatusCode, reply.StatusDescription, reply.Content, reply.Request.Parameters);
                }
            }
            else
            {
                retValue = reply.Data;
            }

			Logger.LogExitingEvent(FUNC);
			return retValue;
		}

		/// <summary>Retreves information for the given account.</summary>
		/// <param name="accountId">The account ID to pull.</param>
		/// <returns>A populated account.</returns>
		private Account Tara_Account_Retrieve(long accountId)
		{
			const string FUNC = CLASS_NAME + "Account_Retrieve()";
			Logger.LogEnteringEvent(FUNC);

			Account retValue = null;

			//Our client & request.
			RestClient client = CreateRestClient();
			RestRequest req = GenerateRestRequest(Method.GET, string.Format(API_ACCOUNT, accountId));

			//Create the account.
			RestResponse<Account> reply = (RestResponse<Account>)client.Execute<Account>(req);

			//See if we have an error..
			if ((int)reply.StatusCode >= 400)
				throw GenerateExFromResp((int)reply.StatusCode, reply.StatusDescription, reply.Content, reply.Request.Parameters);
			else
				retValue = reply.Data;

			Logger.LogExitingEvent(FUNC);
			return retValue;
		}

		/// <summary>Deletes the specified account.</summary>
		/// <param name="accountId">The account ID to return.</param>
		/// <returns>A populated account, or null if the account was deleted completely.</returns>
		private Account Tara_Account_Delete(long accountId)
		{
			const string FUNC = CLASS_NAME + "Account_Delete()";
			Logger.LogEnteringEvent(FUNC);

			//Our return value..
			Account retValue = null;

			//Our client & request.
			RestClient client = CreateRestClient();
			RestRequest req = GenerateRestRequest(Method.DELETE, string.Format(API_ACCOUNT, accountId));

			//Create the account.
			RestResponse<Account> reply = (RestResponse<Account>)client.Execute<Account>(req);

			//See if we have an error..
			if ((int)reply.StatusCode >= 400)
				throw GenerateExFromResp((int)reply.StatusCode, reply.StatusDescription, reply.Content, reply.Request.Parameters);
			else
				retValue = reply.Data;

			Logger.LogExitingEvent(FUNC);
			return retValue;
		}

		/// <summary>Updates the specified account with the new name.</summary>
		/// <param name="accountId">The account ID to update.</param>
		/// <param name="newName">The new name of the account.</param>
		/// <returns>The populated account with the new information.</returns>
		private Account Tara_Account_Update(long accountId, string newName)
		{
			const string FUNC = CLASS_NAME + "Account_Update()";
			Logger.LogEnteringEvent(FUNC);

			//Return value..
			Account retValue = null;

			//Our client & request..
			RestClient client = CreateRestClient();
			RestRequest req = GenerateRestRequest(Method.PUT, string.Format(API_ACCOUNT, accountId));
			req.AddJsonBody(new Dictionary<string, string>() { { "name", newName } });

			//Create the account.
			RestResponse<Account> reply = (RestResponse<Account>)client.Execute<Account>(req);

			//See if we have an error..
			if ((int)reply.StatusCode >= 400)
				throw GenerateExFromResp((int)reply.StatusCode, reply.StatusDescription, reply.Content, reply.Request.Parameters);
			else
				retValue = reply.Data;

			Logger.LogExitingEvent(FUNC);
			return retValue;
		}
		#endregion Account Calls

		#region Project Calls
		/// <summary>Creates a project with the given name and repository type for the account ID.</summary>
		/// <param name="accountId">The ID to create the repository for.</param>
		/// <param name="projectName">The name of rhe repository/project.</param>
		/// <param name="repositoryType">The type (git, svn) of the repository.</param>
		/// <returns>The project that was created.</returns>
		private Project Tara_Project_Create(long accountId, string projectName, Project.RepositoryTypeEnum repositoryType)
		{
			const string FUNC = CLASS_NAME + "Project_Create()";
			Logger.LogEnteringEvent(FUNC);

			Project retValue = null;

			//Our return value & client..
			RestClient client = CreateRestClient();
			RestRequest req = GenerateRestRequest(Method.POST, string.Format(API_PROJECT, accountId, ""));
			req.AddJsonBody(new Project()
			{
				name = projectName,
				repository_type = ((repositoryType == Project.RepositoryTypeEnum.GIT) ? "git" : "svn")
			});

			//Create the account.
			RestResponse<Project> reply = (RestResponse<Project>)client.Execute<Project>(req);

			//See if we have an error..
			if ((int)reply.StatusCode >= 400)
				throw GenerateExFromResp((int)reply.StatusCode, reply.StatusDescription, reply.Content, reply.Request.Parameters);
			else
				retValue = reply.Data;

			Logger.LogExitingEvent(FUNC);
			return retValue;
		}

		/// <summary>Retrieves the project information for the given account ID and project ID.</summary>
		/// <param name="accountId">The account ID that the project should belong to.</param>
		/// <param name="projectId">The unique Project Id.</param>
		/// <returns>A populated Project object.</returns>
		private Project Tara_Project_Retrieve(long accountId, long projectId)
		{
			const string FUNC = CLASS_NAME + "Tara_Project_Retrieve()";
			Logger.LogEnteringEvent(FUNC);

			//Our return value & client..
			Project retValue = null;
			RestClient client = CreateRestClient();
			RestRequest req = GenerateRestRequest(Method.GET, string.Format(API_PROJECT, accountId, projectId));

			//Create the account.
			RestResponse<Project> reply = (RestResponse<Project>)client.Execute<Project>(req);

			//See if we have an error..
			if ((int)reply.StatusCode >= 400 && (int)reply.StatusCode != 404)
				throw GenerateExFromResp((int)reply.StatusCode, reply.StatusDescription, reply.Content, reply.Request.Parameters);
			else
			{
				retValue = reply.Data;
				if (reply.Data.id < 1)
					retValue = null;
			}

			Logger.LogExitingEvent(FUNC);
			return retValue;
		}

		/// <summary>Deletes a project.</summary>
		/// <param name="accountId">The accountId containing the project.</param>
		/// <param name="projectId">The projectId to delete.</param>
		/// <returns>A populated Project object.</returns>
		private Project Tara_Project_Delete(long accountId, long projectId)
		{
			const string FUNC = CLASS_NAME + "Project_Delete()";
			Logger.LogEnteringEvent(FUNC);

			//Our return value & client..
			Project retValue = null;
			RestClient client = CreateRestClient();
			RestRequest req = GenerateRestRequest(Method.DELETE, string.Format(API_PROJECT, accountId, projectId));

			//Create the account.
			RestResponse<Project> reply = (RestResponse<Project>)client.Execute<Project>(req);

			//See if we have an error..
			if ((int)reply.StatusCode >= 400)
				throw GenerateExFromResp((int)reply.StatusCode, reply.StatusDescription, reply.Content, reply.Request.Parameters);
			else
				retValue = reply.Data;

			Logger.LogExitingEvent(FUNC);
			return retValue;
		}

		/// <summary>Updates the project with a new name. Note that the type cannot be changed after being created.</summary>
		/// <param name="accountId">The accountId that contains the project.</param>
		/// <param name="projectId">The projectId of the project to update.</param>
		/// <param name="newName">The new name of the project.</param>
		/// <returns>An updated Project object.</returns>
		private Project Tara_Project_Update(long accountId, long projectId, string newName)
		{
			const string FUNC = CLASS_NAME + "Project_Update()";
			Logger.LogEnteringEvent(FUNC);

			throw new NotImplementedException("Projects cannot be updated once created.");

			//Our return value & client..
			Project retValue = null;
			RestClient client = CreateRestClient();

			//Create our request..
			RestRequest req = GenerateRestRequest(Method.PUT, string.Format(API_PROJECT, accountId, projectId));
			req.AddJsonBody(new Dictionary<string, string>() { { "name", newName } });

			//Create the account.
			RestResponse<Project> reply = (RestResponse<Project>)client.Execute<Project>(req);

			//See if we have an error..
			if ((int)reply.StatusCode >= 400)
				throw GenerateExFromResp((int)reply.StatusCode, reply.StatusDescription, reply.Content, reply.Request.Parameters);
			else
				retValue = reply.Data;

			Logger.LogExitingEvent(FUNC);
			return retValue;
		}
		#endregion Project Calls

		#region User Calls
		/// <summary>Creates a new user with the information given.</summary>
		/// <param name="newuser">The new User to create. PASSWORD field should be set.</param>
		/// <returns>A populated User object.</returns>
		private User Tara_User_Create(long accountId, User newUser)
		{
			const string FUNC = CLASS_NAME + "User_Create()";
			Logger.LogEnteringEvent(FUNC);

			User retValue = null;

			//Our return value & client..
			RestClient client = CreateRestClient();
			RestRequest req = GenerateRestRequest(Method.POST, string.Format(API_USER, accountId, ""));
			req.AddJsonBody(newUser);
			//req.AddJsonBody(new Dictionary<string, string>() {
			//	{"email", newUser.email},
			//	{"password", password},
			//	{"first_name", newUser.first_name},
			//	{"last_name", newUser.last_name},
			//	{"login", newUser.login},
			//	{"jabber_id", ""},
			//	{"twitter_id", ""}
			//});

			//Create the account.
			RestResponse<User> reply = (RestResponse<User>)client.Execute<User>(req);

			//See if we have an error..
			if ((int)reply.StatusCode >= 400)
				throw GenerateExFromResp((int)reply.StatusCode, reply.StatusDescription, reply.Content, reply.Request.Parameters);
			else
				retValue = reply.Data;

			Logger.LogExitingEvent(FUNC);
			return retValue;
		}

		/// <summary>tretrieves the specified user's information.</summary>
		/// <param name="accountId">The accountId that the user belongs to.</param>
		/// <param name="userId">The userId of the details to retrieve.</param>
		/// <returns>A populate user object.</returns>
		private User Tara_User_Retrieve(long accountId, long userId)
		{
			const string FUNC = CLASS_NAME + "User_Retrieve()";
			Logger.LogEnteringEvent(FUNC);

			User retValue = null;

			//Our return value & client..
			RestClient client = CreateRestClient();
			RestRequest req = GenerateRestRequest(Method.GET, string.Format(API_USER, accountId, userId));

			//Create the account.
			RestResponse<User> reply = (RestResponse<User>)client.Execute<User>(req);

			//See if we have an error..
			if ((int)reply.StatusCode >= 400)
				throw GenerateExFromResp((int)reply.StatusCode, reply.StatusDescription, reply.Content, reply.Request.Parameters);
			else
				retValue = reply.Data;

			Logger.LogExitingEvent(FUNC);
			return retValue;
		}

		/// <summary>Marks a user inactive, or deletes a user (if already inactive).</summary>
		/// <param name="accountId">The accountId containing the user.</param>
		/// <param name="userId">The userId to delete.</param>
		/// <returns>A populated User  object.</returns>
		private User Tara_User_Delete(long accountId, long userId)
		{
			const string FUNC = CLASS_NAME + "User_Delete()";
			Logger.LogEnteringEvent(FUNC);

			User retValue = null;

			//Our client..
			RestClient client = CreateRestClient();
			RestRequest req = GenerateRestRequest(Method.DELETE, string.Format(API_USER, accountId, userId));

			//Create the account.
			RestResponse<User> reply = (RestResponse<User>)client.Execute<User>(req);

			//See if we have an error..
			if ((int)reply.StatusCode == 404)
				retValue = null;
			else if ((int)reply.StatusCode >= 400)
				throw GenerateExFromResp((int)reply.StatusCode, reply.StatusDescription, reply.Content, reply.Request.Parameters);
			else
				retValue = reply.Data;

			Logger.LogExitingEvent(FUNC);
			return retValue;
		}

		/// <summary>Updates the user's speficied information.</summary>
		/// <param name="updateUser">The user with updated information.</param>
		/// <returns>A populated User object.</returns>
		/// <remarks>Leave fields blank if they are not getting changed.</remarks>
		private User Tara_User_Update(long accountId, User updateUser, string password)
		{
			const string FUNC = CLASS_NAME + "User_Update()";
			Logger.LogEnteringEvent(FUNC);

			User retValue = null;

			//Our client..
			RestClient client = CreateRestClient();
			RestRequest req = GenerateRestRequest(Method.PUT, string.Format(API_USER, accountId, updateUser.id));
			updateUser.password = password;
			req.AddJsonBody(updateUser);

			//Create the account.
			RestResponse<User> reply = (RestResponse<User>)client.Execute<User>(req);

			//See if we have an error..
			if ((int)reply.StatusCode >= 400)
				throw GenerateExFromResp((int)reply.StatusCode, reply.StatusDescription, reply.Content, reply.Request.Parameters);
			else
				retValue = reply.Data;

			Logger.LogExitingEvent(FUNC);
			return retValue;
		}
		#endregion User Calls

		#region Project User Calls
		/// <summary>Adds the specified user ID to the given project.</summary>
		/// <param name="accountId">The account that contains both the Project and User.</param>
		/// <param name="projectId">The projectId to attach the user to.</param>
		/// <param name="userId">The user to attach to the project.</param>
		/// <returns>A populated User object.</returns>
		private User Tara_Project_AddUser(long accountId, long projectId, long userId)
		{
			const string FUNC = CLASS_NAME + "Project_AddUser()";
			Logger.LogEnteringEvent(FUNC);

			User retValue = null;

			//Our return value & client..
			RestClient client = CreateRestClient();
			RestRequest req = GenerateRestRequest(Method.PUT, string.Format(API_PROJECTUSER, accountId, projectId, userId));

			//Create the account.
			RestResponse<User> reply = (RestResponse<User>)client.Execute<User>(req);

			//See if we have an error..
			if ((int)reply.StatusCode >= 400)
				throw GenerateExFromResp((int)reply.StatusCode, reply.StatusDescription, reply.Content, reply.Request.Parameters);
			else
				retValue = reply.Data;

			Logger.LogExitingEvent(FUNC);
			return retValue;
		}

		/// <summary>Removes a given user from the given project.</summary>
		/// <param name="accountId">The account that contains both the Project and User.</param>
		/// <param name="projectId">The projectId to delete the user from.</param>
		/// <param name="userId">The user to remove from the project.</param>
		/// <returns>A populatd User object.</returns>
		private User Tara_Project_RemoveUser(long accountId, long projectId, long userId)
		{
			const string FUNC = CLASS_NAME + "Project_RemoveUser()";
			Logger.LogEnteringEvent(FUNC);

			User retValue = null;

			//Our return value & client..
			RestClient client = CreateRestClient();
			RestRequest req = GenerateRestRequest(Method.DELETE, string.Format(API_PROJECTUSER, accountId, projectId, userId));

			//Create the account.
			RestResponse<User> reply = (RestResponse<User>)client.Execute<User>(req);

			//See if we have an error..
			if ((int)reply.StatusCode >= 400)
				throw GenerateExFromResp((int)reply.StatusCode, reply.StatusDescription, reply.Content, reply.Request.Parameters);
			else
				retValue = reply.Data;

			Logger.LogExitingEvent(FUNC);
			return retValue;
		}

		/// <summary>Retrieves a list of all uasers assigned to this project.</summary>
		/// <param name="accountId">The accountId that contains the project.</param>
		/// <param name="projectId">The project to get the list of users from.</param>
		/// <returns>A list of Users assigned to this project.</returns>
		private List<User> Tara_Project_RetrieveUsers(long accountId, long projectId)
		{
			const string FUNC = CLASS_NAME + "Project_RetrieveUsers()";
			Logger.LogEnteringEvent(FUNC);

			List<User> retValue = new List<User>();

			//Our return value & client..
			RestClient client = CreateRestClient();
			RestRequest req = GenerateRestRequest(Method.GET, string.Format(API_PROJECTUSER, accountId, projectId, ""));

			//Create the account.
			RestResponse<ListUsers> reply = (RestResponse<ListUsers>)client.Execute<ListUsers>(req);

			//See if we have an error..
			if ((int)reply.StatusCode >= 400)
				throw GenerateExFromResp((int)reply.StatusCode, reply.StatusDescription, reply.Content, reply.Request.Parameters);
			else
				if (reply.Data != null && reply.Data.users != null)
					retValue = reply.Data.users;

			Logger.LogExitingEvent(FUNC);
			return retValue;
		}
		#endregion Project User Calls
		#endregion Calls for Interface for Admin

		#region Static Function
		/// <summary>Creates a client with the specified info and security.</summary>
		/// <returns>A REST client.</returns>
		private static RestClient CreateRestClient()
		{
			RestClient client = new RestClient(API_BASEURL);
			client.Authenticator = new HttpBasicAuthenticator(API_KEY, "");
			client.FollowRedirects = true;
			client.MaxRedirects = 2;
			client.ReadWriteTimeout = 5000; // 5 secs.
			client.Timeout = 5000; // 5 secs.

			return client;
		}

		/// <summary>Given the error number and the message, return an exception for the code.</summary>
		/// <param name="num">The error code (409. 401, etc.)</param>
		/// <param name="msg">Any additional message.</param>
		/// <returns>An exception based on the error number, message.</returns>
		private static Exception GenerateExFromResp(int status, string msgSum, string msgDet, List<Parameter> sendParams)
		{
			if (status < 200)
				return null;
			else
			{
				Parameter bodyParm = sendParams.FirstOrDefault(p => p.Type == ParameterType.RequestBody);
				string reqBody = null;
				if (bodyParm != null)
					reqBody = bodyParm.Value.ToString();

				switch (status)
				{
					case 400:
						return new BadRequestException(msgSum) { ServerMsg = msgDet, RequestBody = reqBody };

					case 401:
						return new UnauthorizedException(msgSum) { ServerMsg = msgDet, RequestBody = reqBody };

					case 405:
						return new MethodNotAllowedException(msgSum) { ServerMsg = msgDet, RequestBody = reqBody };

					case 409:
						return new ConflictException(msgSum) { ServerMsg = msgDet, RequestBody = reqBody };

					default:
						return new RestException(msgSum) { ServerMsg = msgDet, RequestBody = reqBody, ErrCode = status };
				}
			}
		}
		#endregion

		#region API Classes
		/// <summary>Contains just a list of Users.</summary>
		private class ListUsers
		{
			public List<User> users { get; set; }
		}
		#endregion

		/// <summary>Generates a set REST request to send to PL.</summary>
		/// <param name="sendMethod">The method to send the request as.</param>
		/// <param name="Url">The URL location (not including schema and server).</param>
		/// <returns>A forumlated RestRequest</returns>
		private RestRequest GenerateRestRequest(Method sendMethod, string Url)
		{
			//Generate the request.
			RestRequest retValue = new RestRequest();

			//Now add the authentication and standard headers.
			retValue.AddHeader("Authorization", "Basic ZmYwYWJkNjA3YTU3MDEzMTViMjE1OGIwMzVmYzhmMGY6");
			retValue.RequestFormat = DataFormat.Json;
			retValue.ReadWriteTimeout = 2000;
			retValue.Resource = Url;
			retValue.Method = sendMethod;

			return retValue;
		}
	}

	#region Exception Definitions
	/// <summary>Base class for an Exception caused by a Rest Request.</summary>
	public class RestException : Exception
	{
		public RestException()
			: base()
		{ }

		public RestException(string msg)
			: base(msg)
		{ }

		public string ServerMsg { get; set; }
		public string RequestBody { get; set; }
		public int ErrCode { get; set; }
	}

	/// <summary>Represents when an item is made or updatred with a non-unique field. (Name, Email, etc.) [409 Error Code]</summary>
	public class ConflictException : RestException
	{
		public int ErrCode = 409;

		public ConflictException()
			: base(General.Exception_Conflict)
		{ }

		public ConflictException(string msg)
			: base(msg)
		{ }
	}

	/// <summary>Represents when an error in trhe request occurs. [400 Error Code]</summary>
	public class BadRequestException : RestException
	{
		public int ErrCode = 400;

		public BadRequestException()
			: base(General.Exception_BadRequest)
		{ }

		public BadRequestException(string msg)
			: base(msg)
		{ }
	}

	/// <summary>Represents when an error in trhe request occurs. [401 Error Code]</summary>
	public class UnauthorizedException : RestException
	{
		public int ErrCode = 401;

		public UnauthorizedException()
			: base(General.Exception_Unauthorized)
		{ }

		public UnauthorizedException(string msg)
			: base(msg)
		{ }
	}

	/// <summary>Represents when an error in the request cannot be executed. [405 Error Code]</summary>
	public class MethodNotAllowedException : RestException
	{
		public int ErrCode = 401;

		public MethodNotAllowedException()
			: base(General.Exceptioon_MethodNotAllowed)
		{ }

		public MethodNotAllowedException(string msg)
			: base(msg)
		{ }
	}
	#endregion
}
