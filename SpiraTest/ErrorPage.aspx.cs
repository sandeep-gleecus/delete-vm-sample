using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.MasterPages;
using Inflectra.SpiraTest.Web.UserControls;
using System;
using System.Web;
using System.Web.UI;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// Custom SpiraTest Error Page and handling all raised events
	/// </summary>
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.ErrorPage, null)]
	public partial class ErrorPage : PageLayout
	{

		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.ErrorPage::";
		protected string adminEmail = "";

		#region Event Handlers

		/// <summary>
		/// This sets up the page upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			//Send the page title
			((MasterPageBase)(this.Master)).PageTitle = Resources.Main.ErrorPage_Title;

			//Since all errors redirect here, we want to call as few methods
			//as possible to avoid endless looping. So we don't log this method

			//Try and get the error information from the calling page
			Exception lastException = null;
			string lastPageUrl = "";

			try
			{
				Inflectra.SpiraTest.Web.PageLayout lastPage = (Inflectra.SpiraTest.Web.PageLayout)Context.Handler;
				lastException = lastPage.LastPageError;
				lastPageUrl = lastPage.ID;

				//Get and set the email address of the admin account (user ID of 1)
				DataModel.User user = new UserManager().GetUserById(1, false);
				adminEmail = "mailto:" + user.EmailAddress;
			}
			catch (InvalidCastException)
			{
				//Fail quietly
			}

			//Display the error details on the page if we have them
			if (lastException == null)
			{
				this.divErrorDetails.InnerText = Resources.Messages.ErrorPage_DetailsNotAvailable;
			}
			else
			{
                //Handle XSRF/CSRF violations 'nicely'
                if (lastException is XsrfViolationException || lastException is ViewStateException || (lastException is HttpException && lastException.Message.Contains("viewstate")))
                {
                    this.divErrorDetails.InnerHtml = Resources.Messages.ErrorPage_ValidationOfXsrfTokenFailed;
                }
                else
                {
                    //Clean the product name from the stack trace
                    string cleanStackTrace = "";
                    if (!String.IsNullOrEmpty(lastException.StackTrace))
                    {
                        cleanStackTrace = lastException.StackTrace.Replace("Inflectra.SpiraTest", "APPLICATION");
                    }
                    //Don't localize these as it will make Inflectra's job harder to support
                    this.divErrorDetails.InnerHtml =
                        "<br /><b>Offending URL:</b> " + lastPageUrl +
                        "<br /><b>Source:</b> " + lastException.Source +
                        "<br /><b>Message:</b> " + lastException.Message +
                        ((lastException.InnerException != null) ? "; " + lastException.InnerException.Message : "") +
                        "<br /><b>Stack trace:</b> " + cleanStackTrace;
                }
			}
		}

		#endregion
	}
}
