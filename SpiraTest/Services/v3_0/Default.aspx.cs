using System;

namespace Inflectra.SpiraTest.Web.Services.v3_0
{
	public partial class Default : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			//The default service is the Import/Export service
			Response.Redirect("ImportExport.aspx", true);
		}
	}
}