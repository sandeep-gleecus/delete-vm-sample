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

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.Administration
{
    public partial class DataSynchronization :  WebPartBase, IWebPartReloadable
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.Administration.DataSynchronization::";

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
                //Load the list of data-syncs and databind
                DataMappingManager dataMappingManager = new DataMappingManager();
                this.grdDataSynchronization.DataSource = dataMappingManager.RetrieveDataSyncSystems();
                this.grdDataSynchronization.DataBind();
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
            //Register any event handlers
            this.grdDataSynchronization.RowDataBound += grdDataSynchronization_RowDataBound;

            //Now load the content
            if (WebPartVisible)
            {
                LoadAndBindData();
            }
        }

        /// <summary>
        /// Adds selective formatting to the status field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdDataSynchronization_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataSyncSystem dataSyncSystem = (DataSyncSystem)e.Row.DataItem;

                //Change the color of the cell depending on the status
                switch ((DataSyncSystem.DataSyncStatusEnum)dataSyncSystem.DataSyncStatusId)
                {
                    case DataSyncSystem.DataSyncStatusEnum.Failure:
                        e.Row.Cells[2].CssClass = "bg-danger";
                        break;
                    case DataSyncSystem.DataSyncStatusEnum.NotRun:
                        e.Row.Cells[2].CssClass = "bg-info";
                        break;
                    case DataSyncSystem.DataSyncStatusEnum.Success:
                        e.Row.Cells[2].CssClass = "bg-success";
                        break;
                    case DataSyncSystem.DataSyncStatusEnum.Warning:
                        e.Row.Cells[2].CssClass = "bg-warning";
                        break;
                }
            }
        }
    }
}