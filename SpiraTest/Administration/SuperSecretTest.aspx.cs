using System;
using System.Collections.Generic;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Microsoft.Security.Application;

namespace Inflectra.SpiraTest.Web.Administration
{
	/// <summary>This webform code-behind class is responsible to displaying the Administration Page and handling all raised events</summary>
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_DataTools_Title", "Product-General-Settings/#product-data-tools", "Admin_DataTools_Title")]
	public partial class SuperSecretTest : AdministrationBase
	{
		private const string CLASS_NAME = "Web.Administration.SuperSecretTest::";

		/// <summary>This sets up the page upon loading</summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "Page_Load()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Add the client event handler to the background task process
			Dictionary<string, string> handlers = new Dictionary<string, string>();
			handlers.Add("succeeded", "ajxBackgroundProcessManager_success");
			ajxBackgroundProcessManager.SetClientEventHandlers(handlers);

			//Finally, see if we have any error message passed from a calling page
			if (!string.IsNullOrWhiteSpace(Request.QueryString[GlobalFunctions.PARAMETER_ERROR_MESSAGE]))
			{
				lblMessage.Text = Encoder.HtmlEncode(Request.QueryString[GlobalFunctions.PARAMETER_ERROR_MESSAGE]);
				lblMessage.Type = MessageBox.MessageType.Error;
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}
	}
}
