using System;
using System.Linq;
using System.Data;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.DataModel;
using System.Collections.Generic;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.Administration
{
	/// <summary>
	/// Displays the admin page for managing portfolios in the system
	/// </summary>
	[
	HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_PortfolioList_Title", "System-Workspaces/#viewedit-portfolios", "Admin_PortfolioList_Title"),
	AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
	]
	public partial class PortfolioList : AdministrationBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.PortfolioList::";

		/// <summary>
		/// Handles initialization that needs to come before Page_Onload is fired
		/// </summary>
		/// <param name="e"></param>
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

            //Configure the gridview filter column properties
            ((FilterSortFieldEx)this.grdPortfolios.Columns[2]).FilterLookupDataSourceID = this.srcActiveFlag.UniqueID;
            ((FilterSortFieldEx)this.grdPortfolios.Columns[3]).SubHeaderText = GlobalFunctions.ARTIFACT_PREFIX_PORTFOLIO;
			((FilterSortFieldEx)this.grdPortfolios.Columns[3]).DataFormatString = GlobalFunctions.ARTIFACT_PREFIX_PORTFOLIO + GlobalFunctions.FORMAT_ID;
		}

		/// <summary>
		/// Called when the page is first loaded
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Make sure we're authorized to view portfolios
            if (!Common.Global.Feature_Portfolios)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Common.License.LicenseProductName + " does not support portfolios", true);
            }

            //Reset the error messages
            this.lblMessage.Text = "";

			//Add the event handlers
            this.btnApplyFilters.Click += new DropMenuEventHandler(btnApplyFilters_Click);
            this.btnClearFilters.Click += new DropMenuEventHandler(btnClearFilters_Click);
			this.btnAdd.Click += new DropMenuEventHandler(btnAdd_Click);
			this.grdPortfolios.PageIndexChanging += new GridViewPageEventHandler(grdPortfolios_PageIndexChanging);
			this.grdPortfolios.RowCommand += new GridViewCommandEventHandler(grdPortfolios_RowCommand);
            this.grdPortfolios.RowDataBound += grdPortfolios_RowDataBound;

			//Don't reload on Postback
			if (!IsPostBack)
			{
				//Load and bind data
				LoadAndBindData();
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

        /// <summary>
        /// Adds row-specific formatting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void grdPortfolios_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow && e.Row.DataItem != null)
            {
                DataModel.Portfolio portfolio = (DataModel.Portfolio)e.Row.DataItem;
                //Change the color of the cell depending on the status
                if (portfolio.IsActive)
                {
                    e.Row.Cells[2].CssClass = "bg-success priority2";
                }
                else
                {
                    e.Row.Cells[2].CssClass = "bg-warning priority2";
                }
            }
        }

        /// <summary>
        /// Loads the project group grid and populates the filter controls if necessary
        /// </summary>
        protected void LoadAndBindData()
		{
			const string METHOD_NAME = "LoadAndBindData";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Get the filter list as a local variable
			UserSettingsCollection filterList = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PORTFOLIO_LIST_FILTERS);

			//Now we need to update the dataset and re-databind the grid and its lookups
			PortfolioManager portfolioManager = new PortfolioManager();

            //Get the current sort
            string sortExpression = GetUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PORTFOLIO_LIST_GENERAL, GlobalFunctions.USER_SETTINGS_KEY_SORT_EXPRESSION, "Name ASC");

            //Get the list of portfolios
            List<DataModel.Portfolio> portfolios = portfolioManager.Portfolio_Retrieve(filterList, sortExpression);

            this.grdPortfolios.DataSource = portfolios;
			int startingIndex = GetUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PORTFOLIO_LIST_GENERAL, GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_START_ROW, 0);
            int pageSize = GetUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PORTFOLIO_LIST_GENERAL, GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_PAGE_SIZE, 15);
            this.grdPortfolios.PageSize = pageSize;
            this.grdPortfolios.SortExpression = sortExpression;

			//Finally we need to repopulate the filter controls
			this.grdPortfolios.Filters = filterList;

			try
			{
				this.grdPortfolios.PageIndex = startingIndex;
				this.grdPortfolios.DataBind();
			}
			catch (Exception)
			{
				startingIndex = 0;
				this.grdPortfolios.PageIndex = startingIndex;
				SaveUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PORTFOLIO_LIST_GENERAL, GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_START_ROW, startingIndex);
				this.grdPortfolios.DataBind();
			}

			//Set the default button to the filter (when enter pressed)
			this.Form.DefaultButton = this.btnApplyFilters.UniqueID;

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Returns the current sort expression
		/// </summary>
		/// <returns>The sort column and direction</returns>
		/// <remarks>Used from the actual ASPX page</remarks>
		protected string GetCurrentSortExpression()
		{
			return this.grdPortfolios.SortExpression;
		}

		/// <summary>
		/// Handles the event raised when the project group Add button is clicked
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnAdd_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnAdd_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Transfer control to the portfolio create page
			Response.Redirect("PortfolioCreate.aspx", true);

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Handles the click event on the item links in the portfolio grid
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void grdPortfolios_RowCommand(object source, GridViewCommandEventArgs e)
		{
			const string METHOD_NAME = "grdPortfolios_RowCommand";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Handle the appropriate event
			if (e.CommandName == "DeleteRow")
			{
				//Delete this portfolio
				int portfolioId = Int32.Parse((string)e.CommandArgument);
				PortfolioManager portfolioManager = new PortfolioManager();
                portfolioManager.Portfolio_Delete(portfolioId);

				//Now reload project group list and Databind
				LoadAndBindData();
			}
			if (e.CommandName == "SortColumns")
			{
				//Update the sort
				SaveUserSetting(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PORTFOLIO_LIST_GENERAL, GlobalFunctions.USER_SETTINGS_KEY_SORT_EXPRESSION, e.CommandArgument.ToString());
				LoadAndBindData();
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Handles the event raised when the project group list pagination page index is changed
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void grdPortfolios_PageIndexChanging(object source, GridViewPageEventArgs e)
		{
			const string METHOD_NAME = "grdPortfolios_PageIndexChanging";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Set the new page index
				this.grdPortfolios.PageIndex = e.NewPageIndex;
				UserSettingsCollection paginationSettings = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PORTFOLIO_LIST_GENERAL);
				paginationSettings[GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_START_ROW] = e.NewPageIndex;
                paginationSettings[GlobalFunctions.USER_SETTINGS_KEY_PAGINATION_PAGE_SIZE] = this.grdPortfolios.PageSize;
				paginationSettings.Save();

				//Reload project group list and Databind
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
		/// Handles the click event on the project group list filter button
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnApplyFilters_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnApplyFilters_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			UserSettingsCollection filterList = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PORTFOLIO_LIST_FILTERS);
			filterList.Clear();
			this.grdPortfolios.PageIndex = 0;

			//First we need to scan the list of columns and then update the saved filters
			this.grdPortfolios.Filters = filterList;
			this.grdPortfolios.UpdateFilters();
			filterList.Save();

			//Reload project group list and Databind
			LoadAndBindData();

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>
		/// Handles the click event on the project group list clear filters button
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void btnClearFilters_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnClearFilters_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Clear the filters in stored settings
			UserSettingsCollection filterList = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PORTFOLIO_LIST_FILTERS);
			filterList.Clear();
			filterList.Save();
			UserSettingsCollection paginationSettings = GetUserSettings(GlobalFunctions.USER_SETTINGS_ADMINISTRATION_PORTFOLIO_LIST_GENERAL);
			paginationSettings.Clear();
			paginationSettings.Save();

			//Reload user list and Databind
			LoadAndBindData();

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}
	}
}
