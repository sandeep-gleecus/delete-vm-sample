using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;
using System.Web.Security;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.Administration
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// Import Users from LDAP Server Page and handling all raised events
	/// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Import LDAP Users", "System-Users/#importing-ldap-users", "User Import"),
    AdministrationLevelAttribute (AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
	public partial class UserLdapImport : AdministrationBase
	{
		protected LdapUserCollection ldapUserCollection;

		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.AdministrationUserLdapImport::";

        #region Properties

        /// <summary>
        /// Stores the current filters. We don't use the user collection setting as we don't need these filters
        /// persisted to the database since it's used for importing which will be unique each time.
        /// </summary>
        protected Hashtable Filters
        {
            get
            {
                if (ViewState["filters"] == null)
                {
                    ViewState["filters"] = new Hashtable();
                }
                return (Hashtable)ViewState["filters"];
            }
        }

        /// <summary>
        /// Stores the current sort expression. We don't use the user collection setting as we don't need the sort
        /// persisted to the database since it's used for importing which will be unique each time.
        /// </summary>
        protected string SortExpression
        {
            get
            {
                if (ViewState["sortExpression"] == null)
                {
                    return "CommonName ASC";
                }
                else
                {
                    return (string)ViewState["sortExpression"];
                }
            }
            set
            {
                ViewState["sortExpression"] = value;
            }
        }

        #endregion

        /// <summary>
		/// This sets up the page upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent (CLASS_NAME + METHOD_NAME);

			//Reset the error message
			this.lblMessage.Text = "";

			//Register the event handlers
			this.btnImport.Click += new DropMenuEventHandler(btnImport_Click);
            this.btnCancel.Click += new DropMenuEventHandler(btnCancel_Click);
            this.btnFilter.Click += new DropMenuEventHandler(btnFilter_Click);
            this.btnClearFilters.Click += new DropMenuEventHandler(btnClearFilters_Click);
            this.grdLdapUsers.PageIndexChanging += new GridViewPageEventHandler(grdLdapUsers_PageIndexChanging);
            this.grdLdapUsers.RowCommand += new GridViewCommandEventHandler(grdLdapUsers_RowCommand);

			//Only load the data once
			if (!IsPostBack) 
			{
                LoadAndBindData();
			}

			Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

        /// <summary>
        /// Handles clicks on the gridview, including the sort arrows
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdLdapUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            const string METHOD_NAME = "grdLdapUsers_RowCommand";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            if (e.CommandName == "SortColumns")
            {
                //Update the sort
                this.SortExpression = e.CommandArgument.ToString();
                LoadAndBindData();
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Clears the filters used for the import list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnClearFilters_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnClearFilters_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Reset the page index back to 0 and clear saved filters
            this.grdLdapUsers.PageIndex = 0;
            this.Filters.Clear();

            //Reload user list and Databind
            LoadAndBindData();

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Applies the specified filters to the import list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnFilter_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnFilter_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Reset the page index back to 0 and clear saved filters
            this.grdLdapUsers.PageIndex = 0;
            this.Filters.Clear();

            //First we need to scan the list of columns and then update the saved filters
            this.grdLdapUsers.Filters = this.Filters;
            this.grdLdapUsers.UpdateFilters();

            //Reload user list and Databind
            LoadAndBindData();

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Loads in the list of users to be imported
        /// </summary>
        protected void LoadAndBindData()
        {
            //Get the list of LDAP users that we can import
            try
            {
                LdapClient ldapClient = new LdapClient();
                ldapClient.LoadSettings();
                this.ldapUserCollection = ldapClient.GetUsers(this.Filters, this.SortExpression);
            }
            catch (LdapClientUnableToAccessServerException exception)
            {
                this.lblMessage.Text = String.Format(Resources.Messages.UserLdapImport_UnableToAccessLdapServer, exception.Message);
                this.DataBind();
                return;
            }
            catch (Exception exception)
            {
                this.lblMessage.Text = Resources.Messages.UserLdapImport_LdapGeneralError + " " + exception.Message;
                this.DataBind();
                return;
            }

            //Make sure that we have a users collection object
            if (this.ldapUserCollection == null)
            {
                this.lblMessage.Text = Resources.Messages.UserLdapImport_UnableToGetUserList;
                this.DataBind();
                return;
            }

            //Before displaying the users, we need to remove any from the list that already exist in SpiraTest
            //We match them by login name (we get all users including inactive or unapproved)
            MembershipUserCollection users = Membership.GetAllUsers();
            foreach (MembershipUser user in users)
            {
                //See if the user's login is in the import list, and if so, remove
                string login = user.UserName;
                foreach (LdapUser ldapUser in this.ldapUserCollection)
                {
                    if (ldapUser.Login.ToLowerInvariant() == login.ToLowerInvariant())
                    {
                        this.ldapUserCollection.Remove(ldapUser);
                        break;
                    }
                }
            }

            //Finally we need to repopulate the filter and sort controls on the grid
            this.grdLdapUsers.SortExpression = this.SortExpression;
            this.grdLdapUsers.Filters = this.Filters;

            //Databind the form
            this.DataBind();

            //Set the default button to the filter (when enter pressed)
            this.Form.DefaultButton = this.btnFilter.UniqueID;
        }

        /// <summary>
        /// Called when a pagination link was clicked
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        void grdLdapUsers_PageIndexChanging(object source, GridViewPageEventArgs e)
        {
            const string METHOD_NAME = "grdLdapUsers_PageIndexChanging";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                //Set the new page index
                this.grdLdapUsers.PageIndex = e.NewPageIndex;

                //Load and databind again
                LoadAndBindData();
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

		/// <summary>
		/// Imports the selected users into SpiraTest from the LDAP server, then redirects back
		/// to the administration home page.
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnImport_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnImport_Click";

			Logger.LogEnteringEvent (CLASS_NAME + METHOD_NAME);

			//First make sure we have no server-side validation errors
			if (!this.IsValid)
			{
				return;
			}

			//First we need to get the list of users that need to be imported from the datagrid
            List<string> userList = GlobalFunctions.DecodeCheckBoxes(this.grdLdapUsers, 0, 0, false);

			//Make sure at one user is marked for import
			if (userList.Count < 1)
			{
                this.lblMessage.Text = Resources.Messages.UserLdapImport_NeedToSelectOneUser;
                this.lblMessage.Type = MessageBox.MessageType.Error;
				return;
			}

			//Now we need to get the list of users from the LDAP Server again
            SpiraMembershipProvider provider = (SpiraMembershipProvider)Membership.Provider;
			LdapClient ldapClient = new LdapClient();
			try
			{
				ldapClient.LoadSettings();
				this.ldapUserCollection = ldapClient.GetUsers(this.Filters, this.SortExpression);
			}
			catch (LdapClientUnableToAccessServerException exception)
			{
                this.lblMessage.Text = String.Format(Resources.Messages.UserLdapImport_UnableToAccessLdapServer, exception);
                this.lblMessage.Type = MessageBox.MessageType.Error;
                return;
			}

			//Iterate through the list of users marked for import to get their details
            int failureCount = 0;
			foreach (string itemValue in userList)
			{
				//Get the user's login
				string [] itemElements = itemValue.Split(':');
				string login = itemElements[0];
				foreach (LdapUser ldapUser in this.ldapUserCollection)
				{
					//Find the user to import
					if (login == ldapUser.Login)
					{
						//Get the information from the LDAP user handling null values correctly
						string firstName;
						if (ldapUser.FirstName == null || ldapUser.FirstName == "")
						{
							firstName = login;
						}
						else
						{
							if (ldapUser.FirstName.Trim() == "")
							{
								firstName = login;
							}
							else
							{
								firstName = ldapUser.FirstName.Trim();
							}
						}
                        string middleInitial;
						if (ldapUser.MiddleInitial.Trim().Length < 1)
						{
							middleInitial = null;
						}
						else
						{
							middleInitial = ldapUser.MiddleInitial.Trim().Substring(0, 1);
						}
						string lastName;
						if (ldapUser.LastName == null || ldapUser.LastName == "")
						{
							lastName = login;
						}
						else
						{
							if (ldapUser.LastName.Trim() == "")
							{
								lastName = login;
							}
							else
							{
								lastName = ldapUser.LastName.Trim();
							}
						}
						string emailAddress;
						if (ldapUser.EmailAddress == null || ldapUser.EmailAddress == "")
						{
							//Create a fake email address that will at least validate
							emailAddress = login + "@mycompany.com";
						}
						else
						{
							if (ldapUser.EmailAddress.Trim() == "")
							{
                                emailAddress = login + "@mycompany.com";
							}
							else
							{
								emailAddress = ldapUser.EmailAddress.Trim();
							}
						}

                        //Create a new RSS GUID Token for the user
                        string rssToken = GlobalFunctions.GenerateGuid();

						//Now we need to add this user with the appropriate information
                        
                        //First create the user
                        MembershipCreateStatus status;
                        MembershipUser newUser = provider.CreateUser(
                            login,
                            null,
                            emailAddress,
                            null,
                            null,
                            true,
                            ldapUser.DistinguishedName,
                            rssToken,
                            out status
                            );

                        //Check the status
                        if (status == MembershipCreateStatus.Success)
                        {
                            //Now update the profile
                            ProfileEx profile = new ProfileEx(newUser.UserName);
                            profile.FirstName = firstName;
                            profile.MiddleInitial = middleInitial;
                            profile.LastName = lastName;
                            profile.Department = null;
                            profile.IsAdmin = false;
                            profile.IsEmailEnabled = true;
                            profile.Save();
                        }
                        else
                        {
                            failureCount++;
                        }
					}
				}
			}

            //Denote Success and reload data
            LoadAndBindData();

            if (failureCount == 0)
            {
                this.lblMessage.Text = Resources.Messages.UserLdapImport_ImportSuccess;
                this.lblMessage.Type = MessageBox.MessageType.Information;
            }
            else
            {
                this.lblMessage.Text = String.Format(Resources.Messages.UserLdapImport_ImportWithErrors, failureCount);
                this.lblMessage.Type = MessageBox.MessageType.Error;
            }

			Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Redirects the user back to the administration home page when cancel clicked
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnCancel_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnCancel_Click";

			Logger.LogEnteringEvent (CLASS_NAME + METHOD_NAME);

            Response.Redirect("UserList.aspx");

			Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}
	}
}
