using System;
using System.Linq;
using System.Collections.Generic;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;
using System.ComponentModel;
using System.Globalization;

namespace Inflectra.SpiraTest.Web.UserControls
{
	public partial class SignaturePanel : UserControls.UserControlBase
	{
		
		public int ArtifactId
		{
			get
			{
				return ViewState["ArtifactId"] == null ? 0 : (int)ViewState["ArtifactId"];
			}
			set
			{ 
				ViewState["ArtifactId"] = value; 
			}
		}
	
		UserManager _userManager = new UserManager();
		TestCaseManager _testCaseManager = new TestCaseManager();
		
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!this.IsPostBack)
			{
				this.Initialize();
			} 
			
		}

		public bool Initialize()
		{
 
			var approvals = this._testCaseManager.GetAllTestSignaturesForTestCase(this.ArtifactId);

			if (approvals.Count != 0)
			{
				this.grdSignaturesList.DataSource = approvals;
				this.grdSignaturesList.DataBind();
				ShowDataPanel(true);
			}
			else
			{
				ShowDataPanel(false, "No Pending Approvals");
			}
		

			return true;
		}

		protected void grdSignaturesList_RowDataBound(object sender, System.Web.UI.WebControls.GridViewRowEventArgs e)
		{
			if (e.Row.RowType == System.Web.UI.WebControls.DataControlRowType.DataRow)
			{
				TestCaseSignature dataItem = e.Row.DataItem as TestCaseSignature;
				LabelEx lblStatus = e.Row.FindControl("lblStatus") as LabelEx;
				LabelEx lblfullName = e.Row.FindControl("lblFullName") as LabelEx;
				LabelEx lblRequestedDate = e.Row.FindControl("lblRequestedDate") as LabelEx;
				LabelEx lblUpdatedDate = e.Row.FindControl("lblUpdatedDate") as LabelEx;
				LabelEx lblMeaning = e.Row.FindControl("lblMeaning") as LabelEx;

				lblMeaning.Text = dataItem.Meaning;
				lblfullName.Text = dataItem.User?.FullName;
				lblStatus.Text = GetStatusString(dataItem.StatusId);
				lblRequestedDate.Text = dataItem.RequestedDate.HasValue ? dataItem.RequestedDate.Value.ToString("g", CultureInfo.CurrentUICulture) : String.Empty;
				lblUpdatedDate.Text = dataItem.UpdateDate.ToString("g", CultureInfo.CurrentUICulture);
			}

		}

		 
		private void ShowDataPanel(bool visible, string message = null)
		{
			this.dataPanel.Visible = visible;
			this.noDataPanel.Visible = !visible;
			this.lblMessage.Text = message ?? "No Signatures requested. Request Signatures.";
		}

		private string GetStatusString(int status)
		{
			switch (status)
			{
				case 4:
					return "Approved";
				case 3:
					return "Rejected";
				case 10:
					return "Cancelled";
				case 9:
					return "Waiting for Approval";
				default:
					return "";
			}
		}

		 
		 
		 
	}
}
