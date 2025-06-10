using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.ComponentModel;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectAdmin
{
    /// <summary>
    /// Displays the data mapping information for the project
    /// </summary>
    public partial class DataMapping : WebPartBase, IWebPartReloadable
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectAdmin.DataMapping::";

        DataMappingManager dataMappingManager = new DataMappingManager();

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
        /// Loads the control data
        /// </summary>
        public void LoadAndBindData()
        {
            //Load the list of active data-syncs and databind
            this.grdDataSynchronization.DataSource = dataMappingManager.RetrieveDataSyncSystems(true);
            this.grdDataSynchronization.DataBind();
        }

        /// <summary>
        /// Loads the control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            try
            {
                //Register event handlers
                this.grdDataSynchronization.RowDataBound += grdDataSynchronization_RowDataBound;

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
        /// Adds selective formatting to the status field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdDataSynchronization_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataSyncSystem dataSyncSystem = (DataSyncSystem)e.Row.DataItem;

                //We need to see if this project is active for this system
                try
                {
                    DataSyncProject dataSyncProject = dataMappingManager.RetrieveDataSyncProject(dataSyncSystem.DataSyncSystemId, ProjectId);
                    if (dataSyncProject != null)
                    {
                        e.Row.Cells[1].Text = GlobalFunctions.DisplayYnFlag(dataSyncProject.ActiveYn);
                        //Change the color of the cell depending on the status
                        if (dataSyncProject.ActiveYn == "Y")
                        {
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
                        else
                        {
                            //Inactive for this project
                            e.Row.Cells[2].CssClass = "bg-light-gray";
                            e.Row.Cells[2].Text = Resources.Fields.NA;
                        }
                    }
                }
                catch (DataSyncNotConfiguredException)
                {
                    //Inactive for this project
                    e.Row.Cells[1].Text = GlobalFunctions.DisplayYnFlag(false);
                    e.Row.Cells[2].CssClass = "bg-light-gray";
                    e.Row.Cells[2].Text = Resources.Fields.NA;
                }
            }
        }
    }
}