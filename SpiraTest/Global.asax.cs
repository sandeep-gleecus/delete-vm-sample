using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.App_Start;
using Inflectra.SpiraTest.Web.Classes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.EntityClient;
using System.Web.Http;
using System.Web.SessionState;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This class contains all the application-wide event handlers.
	/// </summary>
	public class Global : System.Web.HttpApplication
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Global::";

		/// <summary>Stores the list of active user sessions</summary>
		public static ConcurrentDictionary<string, SessionDetails> UserSessionMapping
		{
			get
			{
				return userSessionMapping;
			}
		}
		private static ConcurrentDictionary<string, SessionDetails> userSessionMapping = new ConcurrentDictionary<string, SessionDetails>();

		/// <summary>Constructor Method</summary>
		public Global()
		{ }

		/// <summary>Kills all the sessions for a specified user</summary>
		/// <param name="includeApiSessions">Should we also kill API sessions</param>
		/// <param name="userId">The id of the user</param>
		public static void KillUserSessions(int userId, bool includeApiSessions)
		{
			//Iterate  through all the users session mappings and get the list of keys to kill
			//We have to use two-passes to avoid locking issue on the dictionary
			//though now that we're using a thread-safe dictionary, no lock needed
			List<string> removeList = new List<string>();
			foreach (KeyValuePair<string, SessionDetails> item in userSessionMapping)
			{
				//If we have the current userId mapped against a DIFFERENT session id
				if (item.Value.UserId == userId && (String.IsNullOrEmpty(item.Value.PlugInName) || includeApiSessions))
				{
					//Mark this item to be deleted
					removeList.Add(item.Key);
				}
			}
			//Now iterate through the arraylist to perform the actual deletes
			for (int i = 0; i < removeList.Count; i++)
			{
				SessionDetails removed;
				userSessionMapping.TryRemove(removeList[i], out removed);
			}
		}

		/// <summary>Registers a new session of the main web application</summary>
		/// <param name="sessionId">The id of the session</param>
		/// <param name="userId">The id of the user</param>
		public static void RegisterSession(string sessionId, int userId)
		{
			RegisterSession(sessionId, userId, "");
		}

		/// <summary>Registers a new session</summary>
		/// <param name="sessionId">The id of the session</param>
		/// <param name="userId">The id of the user</param>
		/// <param name="plugInName">The name of the application calling the API, leave as null-string for the web application</param>
		public static void RegisterSession(string sessionId, int userId, string plugInName)
		{
			//We use the new concurrent dictionary GetOrAdd method
			SessionDetails sessionDetails = new SessionDetails();
			sessionDetails = userSessionMapping.GetOrAdd(sessionId, sessionDetails);
			sessionDetails.UserId = userId;
			sessionDetails.LicenseUsed = true;
			sessionDetails.PlugInName = plugInName;
		}

		/// <summary>This method counts to see the number of concurrently logged-in users</summary>
		/// <returns>The count of concurrently logged-in users</returns>
		public static int ConcurrentUsersCount()
		{
			//Iterate through the user session mapping table to count the number of distinct users
			List<int> foundUsers = new List<int>();
			foreach (KeyValuePair<string, SessionDetails> item in userSessionMapping)
			{
				//See if we have this value in our list of users
				int userId = item.Value.UserId;
				if (!foundUsers.Contains(userId) && item.Value.LicenseUsed)
				{
					foundUsers.Add(userId);
				}
			}

			return foundUsers.Count;
		}

		/// <summary>Returns a list of the user ids currently active in the system</summary>
		public static List<int> GetActiveUserIds()
		{
			//Iterate through the user session mapping table to generate the list of distinct users
			//Have to lock the dictionary to avoid threading issues
			List<int> foundUsers = new List<int>();
			foreach (KeyValuePair<string, SessionDetails> item in userSessionMapping)
			{
				//See if we have this value in our list of users
				int userId = item.Value.UserId;
				if (!foundUsers.Contains(userId))
				{
					foundUsers.Add(userId);
				}
			}
			return foundUsers;
		}

		/// <summary>Returns a list of the users currently active in the system</summary>
		/// <returns>A dataset of the user session data</returns>
		public static List<DataModel.User> GetActiveUserSessions()
		{
			//Iterate through the user session mapping table to generate the list of distinct users
			List<int> foundUsers = GetActiveUserIds();

			//Now retrieve the user details for these users
			Business.UserManager userManager = new Business.UserManager();
			List<DataModel.User> users = userManager.GetUsersByIds(foundUsers);

			//Now add the session information to the data returned
			foreach (KeyValuePair<string, SessionDetails> item in userSessionMapping)
			{
				DataModel.User user = users.Find(u => u.UserId == item.Value.UserId);
				if (user != null)
				{
					user.SessionId = item.Key;
					if (String.IsNullOrEmpty(item.Value.PlugInName))
					{
						user.PlugInName = "Web Application";
					}
					else
					{
						user.PlugInName = item.Value.PlugInName;
					}
				}
			}
			return users;
		}

		/// <summary>This method is called when the application is first started</summary>
		/// <param name="sender">The object raising the event</param>
		/// <param name="e">The event arguments</param>
		protected void Application_Start(Object sender, EventArgs e)
		{
			//Enable Web API
			GlobalConfiguration.Configure(WebApiConfig.Register);

			try
			{
				using (EntityConnection conn = new EntityConnection("name=SpiraTestEntities"))
				{
					try
					{
						//Open the connection
						conn.Open();

						//Execute the command
						string cmdTxt = "select T.Value as VERSION_NO from SpiraTestEntities.GlobalSettings as T where T.Name = @ExRev;";
						using (EntityCommand cmd = new EntityCommand(cmdTxt, conn))
						{
							var prm = cmd.CreateParameter();
							prm.Direction = ParameterDirection.Input;
							prm.ParameterName = "ExRev";
							prm.DbType = DbType.String;
							prm.Value = "Database_Revision";
							cmd.Parameters.Add(prm);
							using (EntityDataReader dataReader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
							{
								if (dataReader.Read())
								{
									string dbVersion = dataReader.GetString(0);
									int dbVerInt = 0;
									if (int.TryParse(dbVersion, out dbVerInt))
									{
										if (dbVerInt != Common.Global.REQUIRED_DATABASE_REVISION)
											PageBase.BADDB_REV = dbVerInt;
									}
								}
							}
						}
					}
					finally
					{
						conn.Close();
					}
				}
			}
			catch (Exception)
			{ }
		}

		/// <summary>This method is called when a session is started</summary>
		/// <param name="sender">The object raising the event</param>
		/// <param name="e">The event arguments</param>
		protected void Session_Start(Object sender, EventArgs e)
		{ }


		/// <summary>
		/// Removes the Server header from all requests, makes it harder for hacker to identify server type
		/// </summary>
		protected void Application_PreSendRequestHeaders()
		{
			Logger.LogEnteringEvent(CLASS_NAME + "Application_PreSendRequestHeaders");
			Response.Headers.Remove("Server");
		}

		/// <summary>This method is called when the application serves a request</summary>
		/// <param name="sender">The object raising the event</param>
		/// <param name="e">The event arguments</param>
		protected void Application_BeginRequest(Object sender, EventArgs e)
		{
			//Ajax services don't need session access, which will speed up service access
			if (Context.Request.Path.Contains("/Services/Ajax"))
			{
				Logger.LogTraceEvent("Application_BeginRequest:", "Disabling Session State for: " + Context.Request.Path);
				Context.SetSessionStateBehavior(SessionStateBehavior.Disabled);
			}
		}

		/// <summary>This method is called when the application is first started</summary>
		/// <param name="sender">The object raising the event</param>
		/// <param name="e">The event arguments</param>
		protected void Application_EndRequest(Object sender, EventArgs e)
		{
			Logger.LogEnteringEvent(CLASS_NAME + "Application_EndRequest");
		}

		/// <summary>This method is called when the application is about to send the content</summary>
		/// <param name="sender">The object raising the event</param>
		/// <param name="e">The event arguments</param>
		protected void Application_PreSendContent(Object sender, EventArgs e)
		{
			Logger.LogEnteringEvent(CLASS_NAME + "Application_PreSendContent");
		}

		/// <summary>This method is called when an authentication request is received</summary>
		/// <param name="sender">The object raising the event</param>
		/// <param name="e">The event arguments</param>
		protected void Application_AuthenticateRequest(Object sender, EventArgs e)
		{
			Logger.LogEnteringEvent(CLASS_NAME + "Application_AuthenticateRequest");
		}

		/// <summary>This method is called when the application raises an error</summary>
		/// <param name="sender">The object raising the event</param>
		/// <param name="e">The event arguments</param>
		protected void Application_Error(Object sender, EventArgs e)
		{ }

		/// <summary>This method is called when the session ends. Currently used to remove the user from the active user pool</summary>
		/// <param name="sender">The object raising the event</param>
		/// <param name="e">The event arguments</param>
		protected void Session_End(Object sender, EventArgs e)
		{
			const string METHOD_NAME = "Session_End";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure that the session is removed from the mapping of sessions to user
			//This prevents users who never log-out from tying up licenses
			Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Checking that session: " + Session.SessionID + " was unlicensed");

			//Iterate  through all the users session mappings and get the list of keys to kill
			//We have to use two-passes to avoid locking issue on the dictionary
			List<string> removeList = new List<string>();
			foreach (KeyValuePair<string, SessionDetails> item in userSessionMapping)
			{
				//If we have the current userId then mark for deletion
				if (item.Key == Session.SessionID)
				{
					//Mark this item to be deleted
					removeList.Add(item.Key);
					Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Unlicensed session: " + Session.SessionID);
				}
			}
			//Now iterate through the arraylist to perform the actual deletes
			for (int i = 0; i < removeList.Count; i++)
			{
				SessionDetails removed;
				userSessionMapping.TryRemove(removeList[i], out removed);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>This method is called when the application is stopped</summary>
		/// <param name="sender">The object raising the event</param>
		/// <param name="e">The event arguments</param>
		protected void Application_End(Object sender, EventArgs e)
		{ }
	}
}

