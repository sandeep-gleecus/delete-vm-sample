using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;
using System;
using System.Collections.Generic;
using System.Web.Security;
using System.Web.UI;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This is the base class for all pages in the system
	/// </summary>
	public class PageBase : Page
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.PageBase::";

		/// <summary>
		/// Web Forms should not have any code inside their constructors
		/// since the OnInit event handler sets up the page
		/// </summary>
		public PageBase()
		{
			//Do Nothing
		}

		/// <summary>
		/// Overrides the default handling of cultures (which is based on the culture the windows server is set to)
		/// to take into account both what is set in Administration
		/// </summary>
		/// <remarks>The user's profile is taken into account in the overriding class in PageLayout</remarks>
		protected override void InitializeCulture()
		{
			base.InitializeCulture();

			//See if we have a system-wide culture set
			if (!String.IsNullOrEmpty(ConfigurationSettings.Default.Globalization_DefaultCulture))
			{
				this.UICulture = ConfigurationSettings.Default.Globalization_DefaultCulture;
				this.Culture = ConfigurationSettings.Default.Globalization_DefaultCulture;
			}

			//See if we have a system-wide timezone set
			if (String.IsNullOrEmpty(ConfigurationSettings.Default.Globalization_DefaultTimezone))
			{
				SpiraContext.Current.TimezoneId = TimeZoneInfo.Local.Id;
			}
			else
			{
				SpiraContext.Current.TimezoneId = ConfigurationSettings.Default.Globalization_DefaultTimezone;
			}
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			//Check if we have a bad DB Revsion.
			if (BADDB_REV > 0)
			{
				//Redirect them to the InvalidDatabase page, only if they're not ALREADY on the page.
				if (!GetType().Name.ToLowerInvariant().Equals("invaliddatabase_aspx"))
					Server.Transfer("~/InvalidDatabase.aspx");
			}
		}

		/// <summary>
		/// Provides easy access to the current ASP.NET user profile
		/// </summary>
		public ProfileEx Profile
		{
			get
			{
				if (this.profile == null)
				{
					this.profile = new ProfileEx();
				}
				if (this.profile.Default == null)
				{
					return null;
				}
				else
				{
					return this.profile;
				}
			}
		}
		private ProfileEx profile;

		/// <summary>
		/// Kills any user sessions other than the once currently logged in (not API sessions)
		/// </summary>
		protected void KillOtherUserSessions()
		{
			//Iterate  through all the users session mappings and get the list of keys to kill
			//We have to use two-passes to avoid locking issue on the hashtable
			List<string> removeList = new List<string>();
			MembershipUser membershipUser = Membership.GetUser();
			if (membershipUser != null && membershipUser.ProviderUserKey != null)
			{
				foreach (KeyValuePair<string, SessionDetails> item in Web.Global.UserSessionMapping)
				{
					//If we have the current userId mapped against a DIFFERENT session id
					if (item.Value.UserId == (int)membershipUser.ProviderUserKey && item.Key != Session.SessionID && String.IsNullOrEmpty(item.Value.PlugInName))
					{
						//Mark this item to be deleted
						removeList.Add(item.Key);
					}
				}
				//Now iterate through the arraylist to perform the actual deletes
				for (int i = 0; i < removeList.Count; i++)
				{
					SessionDetails removed;
					Global.UserSessionMapping.TryRemove(removeList[i], out removed);
				}
			}
		}

		/// <summary>
		/// Stores the version of the Database on the SQL Server. If 0, it is the same as
		/// 
		/// </summary>
		public static int BADDB_REV { get; set; }
	}
}
