using Inflectra.OAuth2;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;
using System;
using System.Linq;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.ServerControls
{
	/// <summary>Changes the CreateUserWizard control to use the SpiraTest notification system instead of the built-in one</summary>
	public class CreateUserWizardEx : CreateUserWizard
	{
		private const string CLASS_NAME = "Web.ServerControls.CreateUserWizardEx::";

		/// <summary>Constructor</summary>
		public CreateUserWizardEx()
			: base()
		{
			const string METHOD_NAME = CLASS_NAME + ".ctor()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				//We don't use their mail system, so leave MailDefinition unset

				//Register the event handlers
				SendMailError += CreateUserWizardEx_SendMailError;
				SendingMail += CreateUserWizardEx_SendingMail;
				CreatedUser += CreateUserWizardEx_CreatedUser;

				Logger.LogExitingEvent(METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Used to log errors in sending emails that the new account has been created</summary>
		void CreateUserWizardEx_SendMailError(object sender, SendMailErrorEventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "CreateUserWizardEx_SendMailError()";
			Logger.LogErrorEvent(METHOD_NAME, e.Exception);
		}

		/// <summary>Called when mail is sent, used to customize the message if needed.</summary>
		void CreateUserWizardEx_SendingMail(object sender, MailMessageEventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "CreateUserWizardEx_SendingMail()";

			//Do nothing
			Logger.LogTraceEvent(METHOD_NAME, "Sending Mail to '" + e.Message.To + "' from '" + e.Message.From + "' with subject '" + e.Message.Subject + "'.");

			//Finally, set the cancelled flag, we send it manually in the CreatedUser event instead
			e.Cancel = true;
		}

		/// <summary>Registers the user</summary>
		protected void CreateUserWizardEx_CreatedUser(object sender, EventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "CreateUserWizardEx_CreatedUser()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				if (Page.Session[OAuthManager.AUTHTOCTRL_COOKNAME] != null)
				{
					//In case we are OAuth, we need to copy the email address to the UserName property.
					var oAuthTok = OAuthManager.UserLoginInfo.Parse((string)Page.Session[OAuthManager.AUTHTOCTRL_COOKNAME]);
					if (oAuthTok != null && string.IsNullOrWhiteSpace(UserName))
					{
						//See if we got an email from the OAuth provider.
						if (oAuthTok.ProviderData.ContainsKey((int)Provider.ClaimTypeEnum.Email) &&
							!string.IsNullOrWhiteSpace((string)oAuthTok.ProviderData[(int)Provider.ClaimTypeEnum.Email]))
						{
							UserName = oAuthTok.ProviderData[(int)Provider.ClaimTypeEnum.Email].ToSafeString();
						}
						else
						{
							CreateUserWizardStep step = WizardSteps.OfType<CreateUserWizardStep>().ToList()[0];
							TextBoxEx email = (TextBoxEx)step.ContentTemplateContainer.FindControl("Email");
							if (email != null && !string.IsNullOrWhiteSpace(GlobalFunctions.HtmlScrubInput(email.Text.Trim())))
							{
								UserName = GlobalFunctions.HtmlScrubInput(email.Text.Trim());
							}
							else
							{
								UserName = Guid.NewGuid().ToString("N");
							}
						}
						//Otherwise, just make their userID a random GUID.
					}
					//No OAuth info? Should nver get here, but give 'em a random GUID.
					else if (string.IsNullOrWhiteSpace(UserName))
						UserName = Guid.NewGuid().ToString("N");

					//Remove the session variable.
					Page.Session.Remove(OAuthManager.AUTHTOCTRL_COOKNAME);
				}

				//We need to now set the profile properties that are not part of the membership provider
				ProfileEx profile = new ProfileEx(UserName);
				CreateUserWizardStep registerUserWizardStep = WizardSteps.OfType<CreateUserWizardStep>().ToList()[0];
				if (profile != null)
				{
					//First Name
					TextBoxEx firstName = (TextBoxEx)registerUserWizardStep.ContentTemplateContainer.FindControl("FirstName");
					if (firstName != null)
					{
						profile.FirstName = GlobalFunctions.HtmlScrubInput(firstName.Text.Trim());
					}

					//Last Name
					TextBoxEx lastName = (TextBoxEx)registerUserWizardStep.ContentTemplateContainer.FindControl("LastName");
					if (lastName != null)
					{
						profile.LastName = GlobalFunctions.HtmlScrubInput(lastName.Text.Trim());
					}

					//Middle Initial
					TextBoxEx middleInitial = (TextBoxEx)registerUserWizardStep.ContentTemplateContainer.FindControl("MiddleInitial");
					if (middleInitial != null)
					{
						profile.MiddleInitial = GlobalFunctions.HtmlScrubInput(middleInitial.Text.Trim());
					}

					//Default values for other fields
					profile.IsAdmin = false;
					profile.IsEmailEnabled = true;
					profile.Save();
				}

				//Send notification to user letting them know their account was created.
				new UserManager().SendNewAccountRequestEmail(UserName);
				Logger.LogExitingEvent(METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Overrides the given string if we're in OAuth handling.</summary>
		public override string DuplicateUserNameErrorMessage
		{
			get
			{
				if (Page.Session[OAuthManager.AUTHTOCTRL_COOKNAME] != null)
					return Resources.Messages.NeedAccount_OAuthLoginInUse;
				else
					return base.DuplicateUserNameErrorMessage;
			}
			set { }
		}
	}
}