using System;
using System.Data;
using System.Data.SqlClient;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Business;

namespace Inflectra.SpiraTest.Web.Administration
{
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Edit File Type", "System/#file-type-icons", "Edit File Type")]
	[AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)]
	public partial class FileTypeEdit : AdministrationBase
	{
		public const string PARAMETER_ID = "filetypeid";

		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.FileTypeList::";

		private int FileId = -1;
		private Filetype fileInfo;

		protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			if (string.IsNullOrWhiteSpace(Request.QueryString[FileTypeEdit.PARAMETER_ID]) || !GlobalFunctions.IsInteger(Request.QueryString[FileTypeEdit.PARAMETER_ID]))
			{
				Response.Redirect("FileTypeList.aspx", true);
				return;
			}
			else
				this.FileId = int.Parse(Request.QueryString[FileTypeEdit.PARAMETER_ID]);

			//Get the file type object
			this.fileInfo = new FileTypeManager().GetFileTypeInfo(this.FileId);

			if (this.fileInfo == null)
			{
				Response.Redirect("FileTypeList.aspx", true);
				return;
			}

			//Load up display items.
			if (!this.IsPostBack)
			{
				if (this.FileId > 0)
				{
					this.txtID.Text = this.FileId.ToString();
					this.txtExtension.Text = this.fileInfo.FileExtension;
					this.txtDescription.Text = this.fileInfo.Description;
					this.txtMimeType.Text = this.fileInfo.Mime;
					this.txtImage.Text = this.fileInfo.Icon;
					this.btnUpdate.Text = Resources.Buttons.Save;
				}
				else
				{
					this.txtID.Text = "--";
					this.txtExtension.Text = "";
					this.txtDescription.Text = "";
					this.txtMimeType.Text = this.fileInfo.Mime;
					this.txtImage.Text = this.fileInfo.Icon;
					this.btnUpdate.Text = Resources.Buttons.Add;
				}
			}
			//Set button events.
			this.btnUpdate.Click += new EventHandler(btnUpdate_Click);
		}

		/// <summary>They want to update or add a record.</summary>
		/// <param name="sender">ButtonEx</param>
		/// <param name="e">EventArgs</param>
		void btnUpdate_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnUpdate_Click";

			//Get values. (Sanity check.)
			string strMime = this.txtMimeType.Text.Trim();
			string strIcon = this.txtImage.Text.Trim();
			string strExt = this.txtExtension.Text.Trim().ToLowerInvariant();
			if (strExt.StartsWith(".")) strExt = strExt.Trim(new char[] { '.' });
			string strDesc = this.txtDescription.Text.Trim();
			int intID = 0;
			if (this.txtID.Text.Trim() != "--")
				intID = int.Parse(this.txtID.Text.Trim());

            FileTypeManager manager = new FileTypeManager();
			if (string.IsNullOrWhiteSpace(strMime) || string.IsNullOrWhiteSpace(strIcon))
			{
				//Need to get defaults.
				Filetype unknown = manager.GetFileTypeInfo(0);

				if (string.IsNullOrWhiteSpace(strMime)) strMime = unknown.Mime;
                if (string.IsNullOrWhiteSpace(strIcon)) strIcon = unknown.Icon;
			}

			//Now make sure the other required info isn't null.
			if (!string.IsNullOrWhiteSpace(strExt) && !string.IsNullOrWhiteSpace(strDesc))
			{
				try
				{
					manager.UpdateAddFileType(intID, strExt, strMime, strIcon, strDesc);
				}
                catch (EntityConstraintViolationException ex)
				{
                    //Extension already exists
                    Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
                    this.lblMessage.Type = ServerControls.MessageBox.MessageType.Error;
                    this.lblMessage.Text = string.Format(Resources.Messages.Admin_FileTypes_ExtensionExists, strExt);
                    return;
                }
				catch (Exception ex)
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
					this.lblMessage.Type = ServerControls.MessageBox.MessageType.Error;
					this.lblMessage.Text = string.Format(Resources.Main.Admin_FileTypes_ErrorAction, ((intID == 0) ? Resources.Main.Admin_FileTypes_Adding : Resources.Main.Admin_FileTypes_Updating));
					return;
				}

                //Refresh the cached values
                manager.RefreshFiletypes();

				Session["FILETYPE_ADDED"] = string.Format(((intID == 0) ? Resources.Messages.Admin_FileTypes_SuccessAdded : Resources.Messages.Admin_FileTypes_SuccessUpdated), strExt);
				Response.Redirect("FileTypeList.aspx", true);
				return;
			}
			else
			{
				//Show error message here.
				this.lblMessage.Type = ServerControls.MessageBox.MessageType.Error;
				this.lblMessage.Text = string.Format(Resources.Main.Admin_FileTypes_ErrorAction, ((intID == 0) ? Resources.Main.Admin_FileTypes_Adding : Resources.Main.Admin_FileTypes_Updating));
				return;
			}
		}
	}
}