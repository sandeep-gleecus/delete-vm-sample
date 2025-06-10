using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This page displays the error message if there is an invalid license
	/// or the number of concurrent users has been exceeded
	/// </summary>
	public partial class InvalidDbRevision : PageBase
	{
		#region Event Handlers

		/// <summary>This sets up the page upon loading</summary>
		/// <param name="sender">Page</param>
		/// <param name="e">EventArgs</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			if (BADDB_REV == 0)
			{
				Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Login), true);
				return;
			}
			else
			{
				MasterPages.Login masPage = (MasterPages.Login)Master;
				masPage.PageTitle = Resources.Main.InvalidDatabase_Title;

				//Get the revision information...
				ltrMessage.Text = string.Format(
					Resources.Messages.InvalidDatabase_InvalidRevisionInstalled,
					"v" + Common.Global.REQUIRED_DATABASE_REVISION.ToString(),
					"v" + BADDB_REV.ToString());
			}
		}

		#endregion
	}
}
