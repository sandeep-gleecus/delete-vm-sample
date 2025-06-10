using System;
using System.Data;
using System.Web.UI.WebControls.WebParts;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;
using System.Collections.Generic;


namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome
{
	public partial class TestPreparationStatus : WebPartBase, IWebPartReloadable
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.TestPreparationStatus::";

		protected string RedirectBaseUrl
		{
			get
			{
				return UrlRewriterModule.ResolveUrl("~/TestCaseList.aspx");
			}
		}

		[ConnectionProvider("ReloadableProvider", "ReloadableProvider")]
		public IWebPartReloadable GetReloadable()
		{
			return this;
		}

		public void LoadAndBindData()
		{
			this.jqSnapshotGraph.SelectedGraph = Graph.GraphEnum.TestPreparationSummary;
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			try
			{
				//Needs to set here because the value won't be set if the webpart is added from the catalog
				this.MessageBoxId = "lblMessage";

				//Pass the message box and other context items to the AJAX control
				this.jqSnapshotGraph.ErrorMessageClientID = this.MessageBoxClientID;
				this.jqSnapshotGraph.ProjectId = this.ProjectId;
				this.jqSnapshotGraph.WebPartUniqueId = this.WebPartUniqueId;

				//Now load the content
				if (!IsPostBack)
				{
					if (WebPartVisible)
					{
						LoadAndBindData();
					}
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				//Don't rethrow as this is loaded by an update panel and can't redirect to error page
				//if (this.Message != null)
				//{
				//	this.Message.Text = Resources.Messages.Global_UnableToLoad + " '" + this.Title + "'";
				//	this.Message.Type = MessageBox.MessageType.Error;
				//}
			}
		}
	}
}
