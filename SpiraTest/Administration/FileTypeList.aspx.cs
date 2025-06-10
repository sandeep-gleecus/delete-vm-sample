using System;
using System.Web.UI.WebControls;
using System.Collections.Generic;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Administration
{
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "File Types List", "System/#file-type-icons", "File Types List")]
	[AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)]
	public partial class FileTypeList : AdministrationBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.FileTypeList::";

		private List<Filetype> fileTypes;

		protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//We need to get the list of FileTypes..
			this.fileTypes = new FileTypeManager().GetFileTypeValues();

			//Configure the datagrid.
			this.grdFileTypeList.RowCommand += new GridViewCommandEventHandler(grdFileTypeList_RowCommand);
			this.grdFileTypeList.RowDataBound += new GridViewRowEventHandler(grdFileTypeList_RowDataBound);
			this.grdFileTypeList.DataSource = this.fileTypes;
			this.grdFileTypeList.DataBind();

			//If a session var is set from the save page..
			if (Session["FILETYPE_ADDED"] != null)
			{
				this.lblMessage.Type = ServerControls.MessageBox.MessageType.Information;
				this.lblMessage.Text = (string)Session["FILETYPE_ADDED"];
				Session.Remove("FILETYPE_ADDED");
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		/// <summary>Hit when each row is databound. We need to take out the edit/delete commands for the first row.</summary>
		/// <param name="sender">DataGridEx</param>
		/// <param name="e"></param>
		private void grdFileTypeList_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				if ((int)((Filetype)e.Row.DataItem).FiletypeId == 0)
				{
					e.Row.Controls[4].Controls[1].Visible = false; // Edit Link
					e.Row.Controls[4].Controls[2].Visible = false; // Separator
					e.Row.Controls[4].Controls[3].Visible = false; // Delete Link
				}
			}
		}

		/// <summary>Hit when the user decides to try something.</summary>
		/// <param name="sender">GridViewEx</param>
		/// <param name="e">GridViewCommandEventArgs</param>
		private void grdFileTypeList_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			//First make sure the argument's an integer, and not '0'.
			if (GlobalFunctions.IsInteger((string)e.CommandArgument))
			{
				int recNum = int.Parse((string)e.CommandArgument);

				switch (e.CommandName.ToLowerInvariant())
				{
					case "editfile":
						Response.Redirect("FileTypeEdit.aspx?" + FileTypeEdit.PARAMETER_ID + "=" + recNum.ToString(), true);
						break;

					case "deletefile":
						//First make sure the argument's an integer, and not '0'.
						if (recNum > 0)
						{
							//Get theextension information.
                            FileTypeManager fileTypeManager = new FileTypeManager();
							Filetype filetype = fileTypeManager.GetFileTypeInfo(recNum);

							int rowAffected = fileTypeManager.DeleteFileType(recNum);

							//See if anything was deleted.
							if (rowAffected == 1)
							{
								this.lblMessage.Type = ServerControls.MessageBox.MessageType.Information;
                                this.lblMessage.Text = string.Format(Resources.Messages.Admin_FileTypes_SuccessDeleted, filetype.Description);
							}
							else
							{
								this.lblMessage.Type = ServerControls.MessageBox.MessageType.Error;
								this.lblMessage.Text = Resources.Messages.Admin_FileTypes_TypeDeletedError;
							}
							//Refresh the list
							this.fileTypes = fileTypeManager.GetFileTypeValues();
							this.grdFileTypeList.DataSource = this.fileTypes;
							this.grdFileTypeList.DataBind();

						}
						break;
				}
			}
		}
	}
}