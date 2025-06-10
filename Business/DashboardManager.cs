using System;
using System.Data;
using System.Linq;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>
	/// Handles the data access to the custom dashboard personalization data (implemented as ASP.NET web parts) 
	/// </summary>
	public class DashboardManager : ManagerBase
	{
        private const string CLASS_NAME = "Inflectra.SpiraTest.Business.DashboardManager::";

		/// <summary>
		///	Constructor method for class.
		/// </summary>
		public DashboardManager()
		{
		}

		/// <summary>
		/// Retrieves a list of personalization settings that are not specific to the user
		/// </summary>
		/// <param name="dashboardPath">The URL of the dashboard we want the settings for</param>
		/// <returns>The binary page setting data</returns>
		public byte[] RetrieveGlobalSettings(string dashboardPath)
		{
			const string METHOD_NAME = "RetrieveGlobalSettings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				byte[] buffer = null;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the dashboard id
					var query = from d in context.Dashboards
								where d.LoweredPath == dashboardPath.ToLower()
								select d;

					Dashboard dashboard = query.FirstOrDefault();

					if (dashboard != null)
					{
						//Now get the global settings
						var query2 = from dg in context.DashboardGlobalPersonalizations
									 where dg.DashboardId == dashboard.DashboardId
									 select dg;

						DashboardGlobalPersonalization dashboardGlobalPersonalization = query2.FirstOrDefault();
						if (dashboardGlobalPersonalization != null)
						{
							buffer = dashboardGlobalPersonalization.PageSettings;
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return buffer;
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);

				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of personalization settings that are specific to the user
		/// </summary>
		/// <param name="dashboardPath">The URL of the dashboard we want the settings for</param>
		/// <returns>The binary page setting data</returns>
		/// <param name="userName">The user name we're storing the dashboard settings for</param>
		public byte[] RetrieveUserSettings(string dashboardPath, string userName)
		{
			const string METHOD_NAME = "RetrieveUserSettings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				byte[] buffer = null;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the dashboard id
					var query = from d in context.Dashboards
								where d.LoweredPath == dashboardPath.ToLower()
								select d;

					Dashboard dashboard = query.FirstOrDefault();

					if (dashboard != null)
					{
						//Get the user id
						var query2 = from u in context.Users
									 where u.UserName.ToLower() == userName.ToLower()
									 select u;

						User user = query2.FirstOrDefault();

						if (user != null)
						{
							//Now get the user settings
							var query3 = from du in context.DashboardUserPersonalizations
										 where du.DashboardId == dashboard.DashboardId && du.UserId == user.UserId
										 select du;

							DashboardUserPersonalization dashboardUserPersonalization = query3.FirstOrDefault();
							if (dashboardUserPersonalization != null)
							{
								buffer = dashboardUserPersonalization.PageSettings;
							}
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return buffer;
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);

				throw;
			}
		}

		/// <summary>
		/// Saves the list of personalization settings that are specific to a user
		/// </summary>
		/// <param name="dashboardPath">The URL of the dashboard we want the settings for</param>
		/// <param name="userName">The user name we're storing the dashboard settings for</param>
		/// <param name="pageSettings">The binary page setting data</param>
		public void SaveUserSettings(string dashboardPath, string userName, byte[] pageSettings)
		{
			const string METHOD_NAME = "SaveUserSettings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					//Get the dashboard id
					var query = from d in context.Dashboards
								where d.LoweredPath == dashboardPath.ToLower()
								select d;

					Dashboard dashboard = query.FirstOrDefault();

					int dashboardId;
					if (dashboard == null)
					{
						dashboardId = CreateNewDashboard(dashboardPath);
					}
					else
					{
						dashboardId = dashboard.DashboardId;
					}

					//Get the user id
					var query2 = from u in context.Users
								 where u.UserName.ToLower() == userName.ToLower()
								 select u;

					User user = query2.FirstOrDefault();

					if (user != null)
					{
						//See if we have an existing settings record
						var query3 = from du in context.DashboardUserPersonalizations
									 where du.DashboardId == dashboardId && du.UserId == user.UserId
									 select du;

						DashboardUserPersonalization dashboardUserPersonalization = query3.FirstOrDefault();
						if (dashboardUserPersonalization == null)
						{
							dashboardUserPersonalization = new DashboardUserPersonalization();
							dashboardUserPersonalization.DashboardId = dashboardId;
							dashboardUserPersonalization.UserId = user.UserId;
							dashboardUserPersonalization.PageSettings = pageSettings;
							dashboardUserPersonalization.LastUpdateDate = DateTime.UtcNow;
							context.AddObject("DashboardUserPersonalizations", dashboardUserPersonalization);
						}
						else
						{
							dashboardUserPersonalization.PageSettings = pageSettings;
							dashboardUserPersonalization.LastUpdateDate = DateTime.UtcNow;
						}

						//Persist the changes
						context.SaveChanges();
					}
				}
				catch (Exception exception)
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					throw;
				}
			}
			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Saves the list of personalization settings that are NOT specific to a user
		/// </summary>
		/// <param name="dashboardPath">The URL of the dashboard we want the settings for</param>
		/// <param name="pageSettings">The binary page setting data</param>
		public void SaveGlobalSettings(string dashboardPath, byte[] pageSettings)
		{
			const string METHOD_NAME = "SaveGlobalSettings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the dashboard id
					var query = from d in context.Dashboards
								where d.LoweredPath == dashboardPath.ToLower()
								select d;

					Dashboard dashboard = query.FirstOrDefault();

					int dashboardId;
					if (dashboard == null)
					{
						dashboardId = CreateNewDashboard(dashboardPath);
					}
					else
					{
						dashboardId = dashboard.DashboardId;
					}

					//See if we have an existing settings record
					var query2 = from dg in context.DashboardGlobalPersonalizations
								 where dg.DashboardId == dashboardId
								 select dg;

					DashboardGlobalPersonalization dashboardGlobalPersonalization = query2.FirstOrDefault();
					if (dashboardGlobalPersonalization == null)
					{
						dashboardGlobalPersonalization = new DashboardGlobalPersonalization();
						dashboardGlobalPersonalization.DashboardId = dashboardId;
						dashboardGlobalPersonalization.PageSettings = pageSettings;
						dashboardGlobalPersonalization.LastUpdateDate = DateTime.UtcNow;
						context.AddObject("DashboardGlobalPersonalizations", dashboardGlobalPersonalization);
					}
					else
					{
						dashboardGlobalPersonalization.PageSettings = pageSettings;
						dashboardGlobalPersonalization.LastUpdateDate = DateTime.UtcNow;
					}

					//Persist the changes
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);

				throw;
			}
		}

		/// <summary>
		/// Creates a new dashboard entry
		/// </summary>
		/// <param name="dashboardPath">The path to the dashboard</param>
		/// <returns>The id of the newly created dashboard</returns>
		protected int CreateNewDashboard(string dashboardPath)
		{
			const string METHOD_NAME = "CreateNewDashboard";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the dashboard id
					var query = from d in context.Dashboards
								where d.LoweredPath == dashboardPath.ToLower()
								select d;

					Dashboard dashboard = query.FirstOrDefault();
					if (dashboard == null)
					{
						dashboard = new Dashboard();
						dashboard.Path = dashboardPath;
						dashboard.LoweredPath = dashboardPath.ToLower();
						context.AddObject("Dashboards", dashboard);
						context.SaveChanges();
					}

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					return dashboard.DashboardId;
				}
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);

				throw;
			}
		}

		/// <summary>
		/// Deletes the list of personalization settings that are NOT specific to a user
		/// </summary>
		/// <param name="dashboardPath">The URL of the dashboard we want the settings for</param>
		public void DeleteGlobalSettings(string dashboardPath)
		{
			const string METHOD_NAME = "DeleteGlobalSettings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the dashboard id
					var query = from d in context.Dashboards
								where d.LoweredPath == dashboardPath.ToLower()
								select d;

					Dashboard dashboard = query.FirstOrDefault();

					if (dashboard != null)
					{
						//See if we have an existing settings record
						var query2 = from dg in context.DashboardGlobalPersonalizations
									 where dg.DashboardId == dashboard.DashboardId
									 select dg;

						DashboardGlobalPersonalization dashboardGlobalPersonalization = query2.FirstOrDefault();
						if (dashboardGlobalPersonalization != null)
						{
							//Delete the record
							context.DeleteObject(dashboardGlobalPersonalization);

							//Persist the changes
							context.SaveChanges();
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);

				throw;
			}
		}

		/// <summary>
		/// Deletes the list of personalization settings that are specific to a user
		/// </summary>
		/// <param name="dashboardPath">The URL of the dashboard we want the settings for</param>
		/// <param name="userName">The user name we're storing the dashboard settings for</param>
		public void DeleteUserSettings(string dashboardPath, string userName)
		{
			const string METHOD_NAME = "SaveUserSettings";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the dashboard id
					var query = from d in context.Dashboards
								where d.LoweredPath == dashboardPath.ToLower()
								select d;

					Dashboard dashboard = query.FirstOrDefault();

					if (dashboard != null)
					{
						//Get the user id
						var query2 = from u in context.Users
									 where u.UserName.ToLower() == userName.ToLower()
									 select u;

						User user = query2.FirstOrDefault();

						if (user != null)
						{
							//See if we have an existing settings record
							var query3 = from du in context.DashboardUserPersonalizations
										 where du.DashboardId == dashboard.DashboardId && du.UserId == user.UserId
										 select du;

							DashboardUserPersonalization dashboardUserPersonalization = query3.FirstOrDefault();
							if (dashboardUserPersonalization != null)
							{
								//Delete the record
								context.DeleteObject(dashboardUserPersonalization);

								//Persist the changes
								context.SaveChanges();
							}
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);

				throw;
			}
		}
	}
}
