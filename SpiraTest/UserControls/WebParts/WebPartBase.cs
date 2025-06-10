using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Common;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts
{
	/// <summary>
	/// The base class for all user-control web parts
	/// </summary>
	public class WebPartBase : UserControlBase, IWebPart
	{
		#region IWebPart Interface

		private string title = "Untitled";
		public string Title
		{
			get { return this.title; }
			set { this.title = value; }
		}

		private string titleIconImageUrl = "";
		public string TitleIconImageUrl
		{
			get { return this.titleIconImageUrl; }
			set { this.titleIconImageUrl = value; }
		}

		private string catalogIconImageUrl = "";
		public string CatalogIconImageUrl
		{
			get { return this.catalogIconImageUrl; }
			set { this.catalogIconImageUrl = value; }
		}

		public string Description
		{
			get;
			set;
		}

		public string Subtitle
		{
			get;
			set;
		}

		public string TitleUrl
		{
			get;
			set;
		}

		#endregion

        /// <summary>
        /// The unique ID allocated to the web part when storing user personalization
        /// </summary>
        protected string WebPartUniqueId
        {
            get
            {
                GenericWebPart parent = (GenericWebPart)this.Parent;
                if (parent == null)
                {
                    return this.ID;
                }
                return (this.ID + "$" + parent.ID);
            }

        }

        /// <summary>
        /// Used to tell the web part if it's actually visible or not (so that it knows whether to bother loading the data)
        /// </summary>
        /// <remarks>
        /// You need to use this property instead of Control.Visible because that always returns TRUE regardless of
        /// whether the web part is actually visible or not
        /// </remarks>
        protected bool WebPartVisible
        {
            get
            {
                if (this.Visible)
                {
                    GenericWebPart parent = (GenericWebPart)this.Parent;
                    if (parent == null)
                    {
                        return false;
                    }
                    return (!parent.Hidden && !parent.IsClosed);
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// The chrome state of the web part
        /// </summary>
        public PartChromeState ChromeState
        {
            get
            {
                GenericWebPart parent = (GenericWebPart)this.Parent;
                if (parent == null)
                {
                    return PartChromeState.Normal;
                }
                else
                {
                    return parent.ChromeState;
                }
            }
            set
            {
                GenericWebPart parent = (GenericWebPart)this.Parent;
                if (parent != null)
                {
                    parent.ChromeState = value;
                }
            }
        }

 		/// <summary>
		/// Stores the ID of the message box control that web parts should use for displaying messages
		/// </summary>
		public string MessageBoxId
		{
			get
			{
				return this.messageBoxId;
			}
			set
			{
				this.messageBoxId = value;
			}
		}
		protected string messageBoxId;

		/// <summary>
		/// Contains the reference to the message box control that web parts should use for displaying messages
		/// </summary>
		public MessageBox Message
		{
			get
			{
				//Locate the message box and return
                Control control = this.Page.FindControlRecursive(messageBoxId);
                if (control == null)
                {
                    return null;
                }
                return (MessageBox)control;
			}
		}

        /// <summary>
        /// Contains the client DOM id to the message box control that web parts should use for displaying messages
        /// </summary>
        public string MessageBoxClientID
        {
            get
            {
                //Locate the message box and return
                Control control = this.Page.FindControlRecursive(messageBoxId);
                if (control == null)
                {
                    return "";
                }
                return ((MessageBox)control).ClientID;
            }
        }

		/// <summary>
		/// Gets a collection of user settings for the current user
		/// </summary>
		/// <param name="collectionName"></param>
		/// <returns></returns>
		/// <remarks>
		/// 1) The user id is obtained from session, will throw exception if not
		/// 2) The collection is returned from session (if available) otherwise restored from the database
		/// </remarks>
		protected UserSettingsCollection GetUserSettings(string collectionName)
		{
			//First get the user id from session
			if (UserId < 1)
			{
				throw new Exception("User Id Not Available");
			}
			UserSettingsCollection userSettingsCollection;

			//Now see if we have this setting already in session
            if (Context.Items[collectionName] == null)
			{
				//Restore settings from database and put in session
                userSettingsCollection = new UserSettingsCollection(UserId, collectionName);
				userSettingsCollection.Restore();
				Context.Items[collectionName] = userSettingsCollection;
				return userSettingsCollection;
			}
			//Restore from session
            userSettingsCollection = (UserSettingsCollection)Context.Items[collectionName];

			//return the copy from session
			return userSettingsCollection;
		}

		/// <summary>
		/// Gets a simple (i.e. single entry) boolean user setting
		/// </summary>
		/// <param name="collectionName">The name of the user setting</param>
		/// <param name="entryKey">The name of the setting key</param>
		/// <returns>The value of the key, or the defaultvalue if not set</returns>
		protected bool GetUserSetting(string collectionName, string entryKey, bool defaultValue)
		{
			UserSettingsCollection userSettingsCollection = GetUserSettings(collectionName);
			if (userSettingsCollection[entryKey] == null)
			{
				return defaultValue;
			}
			return ((bool)userSettingsCollection[entryKey]);
		}

		/// <summary>
		/// Gets a simple (i.e. single entry) integer project setting
		/// </summary>
		/// <param name="collectionName">The name of the project setting</param>
		/// <param name="entryKey">The name of the setting key</param>
		/// <returns>The value of the key, or the defaultvalue if not set</returns>
		protected int GetProjectSetting(string collectionName, string entryKey, int defaultValue)
		{
			ProjectSettingsCollection projectSettingsCollection = GetProjectSettings(collectionName);
			if (projectSettingsCollection[entryKey] == null)
			{
				return defaultValue;
			}
			return ((int)projectSettingsCollection[entryKey]);
		}

        /// <summary>
        /// Gets a simple (i.e. single entry) nullable integer project setting
        /// </summary>
        /// <param name="collectionName">The name of the project setting</param>
        /// <param name="entryKey">The name of the setting key</param>
        /// <returns>The value of the key, or the defaultvalue if not set</returns>
        protected int? GetProjectSetting(string collectionName, string entryKey, int? defaultValue)
        {
            ProjectSettingsCollection projectSettingsCollection = GetProjectSettings(collectionName);
            if (projectSettingsCollection[entryKey] == null)
            {
                return defaultValue;
            }
            return ((int)projectSettingsCollection[entryKey]);
        }

		/// <summary>
		/// Gets a collection of project settings for the current user/project
		/// </summary>
		/// <param name="collectionName"></param>
		/// <returns></returns>
		/// <remarks>
		/// 1) The user id and project id are obtained from session, will throw exception if not
		/// 2) The collection is returned from session (if available) otherwise restored from the database
		/// </remarks>
		protected ProjectSettingsCollection GetProjectSettings(string collectionName)
		{
			//First get the project id and user id from session
            if (UserId < 1)
			{
				throw new Exception("User Id Not Available");
			}
			if (ProjectId < 1)
			{
				throw new Exception("Current Project Id Not Stored in Context");
			}
			ProjectSettingsCollection projectSettingsCollection;

			//Now see if we have this setting already in session
			if (Context.Items[collectionName] == null)
			{
				//Restore settings from database and put in session
                projectSettingsCollection = new ProjectSettingsCollection(ProjectId, UserId, collectionName);
				projectSettingsCollection.Restore();
				return projectSettingsCollection;
			}
			//Restore from session
			projectSettingsCollection = (ProjectSettingsCollection)Context.Items[collectionName];

			//Make sure that the project id's match, if not, need to do a fresh restore from the database
            if (projectSettingsCollection.ProjectId == ProjectId)
			{
				//return the copy from session
				return projectSettingsCollection;
			}
			else
			{
				//Restore settings from database and put in session
                projectSettingsCollection = new ProjectSettingsCollection(ProjectId, UserId, collectionName);
				projectSettingsCollection.Restore();
				Context.Items[collectionName] = projectSettingsCollection;
				return projectSettingsCollection;
			}
		}

		/// <summary>
		/// Saves a simple (i.e. single entry) string project setting
		/// </summary>
		/// <param name="collectionName">The name of the project setting</param>
		/// <param name="entryKey">The name of the setting key</param>
		/// <param name="entryValue">The value to set it to</param>
		protected void SaveProjectSetting(string collectionName, string entryKey, string entryValue)
		{
			ProjectSettingsCollection projectSettingsCollection = GetProjectSettings(collectionName);
			projectSettingsCollection[entryKey] = entryValue;
			projectSettingsCollection.Save();
		}

		/// <summary>
		/// Saves a simple (i.e. single entry) integer project setting
		/// </summary>
		/// <param name="collectionName">The name of the project setting</param>
		/// <param name="entryKey">The name of the setting key</param>
		/// <param name="entryValue">The value to set it to</param>
		protected void SaveProjectSetting(string collectionName, string entryKey, int? entryValue)
		{
			ProjectSettingsCollection projectSettingsCollection = GetProjectSettings(collectionName);
            if (entryValue.HasValue)
            {
                projectSettingsCollection[entryKey] = entryValue;
            }
            else if (projectSettingsCollection.ContainsKey(entryKey))
            {
                projectSettingsCollection.Remove(entryKey);
            }
			projectSettingsCollection.Save();
		}
	}
}
