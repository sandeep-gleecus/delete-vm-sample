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
using System.Data;
using System.Collections;
using static Inflectra.SpiraTest.Business.HistoryManager;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectAdmin
{
	/// <summary>
	/// Displays the most recent activity in the project
	/// </summary>
	public partial class HistoryChanges : WebPartBase, IWebPartReloadable
	{
		private const string CLASS_NAME = "Web.UserControls.WebParts.ProjectAdmin.HistoryChanges::";

		#region User Configurable Properties

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
				return rowsToDisplay;
			}
			set
			{
				rowsToDisplay = value;
				//Force the data to reload
				LoadAndBindData();
			}
		}
		protected int rowsToDisplay = 5;

		#endregion

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
			//Get the list of history items for the current project, user is always admin
			HistoryManager historyManager = new HistoryManager();
			List<HistoryChangeSetView> historyChangeSets = historyManager.RetrieveSetsByProjectId(ProjectId, GlobalFunctions.GetCurrentTimezoneUtcOffset(), "ChangeDate", false, null, 1, rowsToDisplay);
			rptActivity.DataSource = historyChangeSets;
			rptActivity.DataBind();
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
				//Add the event handlers
				rptActivity.ItemDataBound += new RepeaterItemEventHandler(rptActivity_ItemDataBound);

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
				if (Message != null)
				{
					Message.Text = Resources.Messages.Global_UnableToLoad + " '" + Title + "'";
					Message.Type = MessageBox.MessageType.Error;
				}
			}
		}

		/// <summary>
		/// Adds URLs and other dynamic data to the repeater items
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void rptActivity_ItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
			{
				HistoryChangeSetView historyChangeSet = (HistoryChangeSetView)e.Item.DataItem;

				HyperLinkEx lnkArtifact = (HyperLinkEx)e.Item.FindControl("lnkArtifact");
				if (lnkArtifact != null)
				{
					lnkArtifact.NavigateUrl = UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "HistoryDetails") + "?" + GlobalFunctions.PARAMETER_CHANGESET_ID + "=" + historyChangeSet.ChangeSetId;
				}

				//If we have SpiraTest, hide the user link since the resource page is not available
				HyperLinkEx lnkUser = (HyperLinkEx)e.Item.FindControl("lnkUser");
				if (lnkUser != null)
				{
					lnkUser.NavigateUrl = "~/Administration/UserDetailsEdit.aspx?" + GlobalFunctions.PARAMETER_USER_ID + "=" + historyChangeSet.UserId;
				}
			}
		}
	}
}
