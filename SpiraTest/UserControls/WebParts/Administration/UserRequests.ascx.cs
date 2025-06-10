using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.ComponentModel;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Attributes;
using System.Collections;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.Administration
{
    public partial class UserRequests : WebPartBase, IWebPartReloadable
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.Administration.UserRequests::";

        /// <summary>
        /// Stores how many rows of data to display, default is 5
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("Global_NumberRowsToDisplay"),
        LocalizedWebDescription("Global_NumberRowsToDisplayTooltip"),
        DefaultValue(5)
        ]
        public int RowsToDisplay
        {
            get
            {
                return this.rowsToDisplay;
            }
            set
            {
                this.rowsToDisplay = value;
                //Force the data to reload
                LoadAndBindData();
            }
        }
        protected int rowsToDisplay = 5;

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
        /// Loads the data in the control
        /// </summary>
        public void LoadAndBindData()
        {
            const string METHOD_NAME = "LoadAndBindData";

            try
            {
                //We only want unapproved users
                Hashtable filterList = new Hashtable();
                filterList["IsApproved"] = false;

                //Get the list of pending user requests
                Business.UserManager userManager = new Business.UserManager();
                int totalCount = 0;
                List<DataModel.User> users = userManager.GetUsers(filterList, "CreationDate DESC", 0, RowsToDisplay, GlobalFunctions.GetCurrentTimezoneUtcOffset(), out totalCount, true, true);

                //Databind the grid
                this.grdUserManagement.DataSource = users;
                this.grdUserManagement.DataBind();
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
        /// Loads the page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            //Now load the content
            if (WebPartVisible)
            {
                LoadAndBindData();
            }
        }
    }
}