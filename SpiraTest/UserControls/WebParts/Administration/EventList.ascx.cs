using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Attributes;
using System.ComponentModel;
using System.Diagnostics;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.Administration
{
    public partial class EventList : WebPartBase, IWebPartReloadable
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.Administration.EventList::";

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
                //Get the list of recent events
                EventManager eventManager = new EventManager();
                int totalCount = 0;
                List<Event> events = eventManager.GetEvents(null, "EventTimeUtc DESC", 0, RowsToDisplay, GlobalFunctions.GetCurrentTimezoneUtcOffset(), out totalCount);

                //Databind the grid
                this.grdEventLog.DataSource = events;
                this.grdEventLog.DataBind();
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
            //Register event handlers
            this.grdEventLog.RowDataBound += new GridViewRowEventHandler(grdEventLog_RowDataBound);

            //Now load the content
            if (WebPartVisible)
            {
                LoadAndBindData();
            }
        }

        /// <summary>
        /// Adds coloration to the events in the event log grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdEventLog_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //Make sure we have a data row
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                Event evt = (Event)e.Row.DataItem;
                string cssClass = "";
                switch ((EventLogEntryType)evt.EventTypeId)
                {
                    case EventLogEntryType.Error:
                        cssClass = "EventError priority1";
                        break;
                    case EventLogEntryType.Warning:
                        cssClass = "EventWarning priority1";
                        break;
                    case EventLogEntryType.Information:
                        cssClass = "EventInformational priority1";
                        break;
                    case EventLogEntryType.SuccessAudit:
                        cssClass = "EventSuccessAudit priority1";
                        break;
                    case EventLogEntryType.FailureAudit:
                        cssClass = "EventFailureAudit priority1";
                        break;
                }
                e.Row.Cells[1].CssClass = cssClass;

                //We need to convert the datetime in the EventTimeUtc field to the local time for the current user
                DateTime localDate = GlobalFunctions.LocalizeDate(evt.EventTimeUtc);
                e.Row.Cells[0].Text = localDate.ToString("g");
            }
        }
    }
}