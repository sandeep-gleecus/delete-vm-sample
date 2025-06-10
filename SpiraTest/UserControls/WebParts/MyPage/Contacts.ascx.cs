using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage
{
    /// <summary>
    /// Displays a list of a user's contacts, from which he can send messages
    /// </summary>
    public partial class Contacts : WebPartBase, IWebPartReloadable
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.Contacts::";

        /// <summary>
        /// Loads the control data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

			try
			{
                //We have to set the message box programmatically for items that start out in the catalog
                this.MessageBoxId = "lblMessage";

                //Attach event handlers
                this.grdContacts.RowCommand += new GridViewCommandEventHandler(grdContacts_RowCommand);
                
                //Now load the content
                if (WebPartVisible)
                {
                    LoadAndBindData();
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                //Don't rethrow as this is loaded by an update panel and can't redirect to error page
                if (this.Message != null)
                {
                    this.Message.Text = Resources.Messages.Global_UnableToLoad + " '" + this.Title + "'";
                    this.Message.Type = MessageBox.MessageType.Error;
                }
            }
        }

        /// <summary>
        /// Called when a button in the contact list is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdContacts_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            //See what command was clicked
            if (e.CommandName.ToLowerInvariant() == "remove")
            {
                //Get the id of the user
                if (e.CommandArgument != null && e.CommandArgument is String)
                {
                    int contactUserId = Int32.Parse((string)e.CommandArgument);
                    new UserManager().UserContact_Remove(this.UserId, contactUserId);

                    //Reload the grid
                    LoadAndBindData();
                }
            }
        }

        /// <summary>
        /// Returns a handle to the interface
        /// </summary>
        /// <returns>IWebPartReloadable</returns>
        [ConnectionProvider("ReloadableProvider", "ReloadableProvider")]
        public IWebPartReloadable GetReloadable()
        {
            return this;
        }

        /// <summary>
		/// Loads and binds the data
		/// </summary>
        public void LoadAndBindData()
        {
            //Get the list of the current user's contacts
            UserManager userManager = new UserManager();
            List<User> contacts = userManager.UserContact_Retrieve(UserId);

            this.grdContacts.DataSource = contacts;
            this.grdContacts.DataBind();
        }
    }
}