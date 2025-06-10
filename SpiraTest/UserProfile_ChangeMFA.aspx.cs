using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.MasterPages;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;

using Base32;
using OtpSharp;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// User Profile Edit Page and handling all raised events
	/// </summary>
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.MyProfile, "ChangeMfa_Title", "User-Product-Management/#mfa-settings")]
	public partial class UserProfile_ChangeMFA : PageLayout
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserProfile_ChangeMFA::";

		#region Google Authenticator Properties

		/// <summary>
		/// The secret key
		/// </summary>
		public string SecretKey
		{
			get
			{
				if (ViewState["SecretKey"] == null)
				{
					return "";
				}
				else
				{
					return (string)ViewState["SecretKey"];
				}
			}
			set
			{
				ViewState["SecretKey"] = value;
			}
		}

		/// <summary>
		/// The barcode url
		/// </summary>
		public string BarcodeUrl
		{
			get
			{
				if (ViewState["BarcodeUrl"] == null)
				{
					return "";
				}
				else
				{
					return (string)ViewState["BarcodeUrl"];
				}
			}
			set
			{
				ViewState["BarcodeUrl"] = value;
			}
		}

		/// <summary>
		/// The generated code
		/// </summary>
		public string Code
		{
			get
			{
				if (ViewState["Code"] == null)
				{
					return "";
				}
				else
				{
					return (string)ViewState["Code"];
				}
			}
			set
			{
				ViewState["Code"] = value;
			}
		}

		#endregion

		/// <summary>
		/// This generates the necessary data to display the QR Code
		/// </summary>
		protected void GenerateAuthenticatorData()
		{
			byte[] secretKey = KeyGeneration.GenerateRandomKey(20);
			string userName = User.Identity.Name;
			string issuer = GlobalFunctions.MFA_ISSUER_NAME;
			string issuerEncoded = HttpUtility.UrlEncode(issuer);
			string barcodeUrl = KeyUrl.GetTotpUrl(secretKey, userName) + "&issuer=" + issuerEncoded;

			SecretKey = Base32Encoder.Encode(secretKey);
			BarcodeUrl = barcodeUrl;
		}

		/// <summary>
		/// This sets up the page upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			//Register event handlers
			this.btnSubmit.Click += BtnSubmit_Click;
			this.btnRemove.Click += BtnRemove_Click;

			//Load the page content
			if (!IsPostBack)
			{
				if (Page.User.Identity.IsAuthenticated)
				{
					try
					{
						//Display the appropriate MFA link (add or change)
						User user = new UserManager().GetUserByLogin(Page.User.Identity.Name);
						if (String.IsNullOrEmpty(user.MfaToken))
						{
							//There is no MFA, so display add link
							this.plcAddMFA.Visible = true;
							this.plcAddMFAText.Visible = true;
						}
						else
						{
							//There is an existing MFA, so display change link
							this.plcChangeMFA.Visible = true;
							this.plcChangeMFAText.Visible = true;
							this.btnRemove.Visible = true;
						}

						//Generate the authenticator QR Code
						GenerateAuthenticatorData();
					}
					catch (ArtifactNotExistsException)
					{
						//User does not exist, redirect back to user profile page
						Response.Redirect("~/UserProfile.aspx");
					}
				}
			}
		}

		/// <summary>
		/// Removes MFA from the user's profile
		/// </summary>
		private void BtnRemove_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "BtnSubmit_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Simply set the token to NULL
				UserManager userManager = new UserManager();
				User user = userManager.GetUserByLogin(Page.User.Identity.Name);
				user.StartTracking();
				user.MfaToken = null;
				user.LastActivityDate = DateTime.UtcNow;
				userManager.Update(user);

				//Success, so just redirect back to the user profile page
				Response.Redirect("~/UserProfile.aspx");

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (ArtifactNotExistsException)
			{
				this.lblMessage.Text = Resources.Messages.ChangeMfa_UserNotExists1;
				this.lblMessage.Type = ServerControls.MessageBox.MessageType.Error;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				this.lblMessage.Text = exception.Message;
				this.lblMessage.Type = ServerControls.MessageBox.MessageType.Error;
			}
		}

		/// <summary>
		/// Adds/updates the MFA from a user's profile after making sure the code is valid
		/// </summary>
		private void BtnSubmit_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "BtnSubmit_Click";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Make sure all validators have fired
				if (!this.IsValid)
				{
					return;
				}

				//Get the user's entered OTP
				string oneTimePassword = this.txtOTP.Text.Trim();

				//Now we need to compare that with the OTP we'd expect from the system
				byte[] secretKey = Base32Encoder.Decode(SecretKey);

				long timeStepMatched = 0;
				var otp = new Totp(secretKey);
				if (otp.VerifyTotp(oneTimePassword, out timeStepMatched, new VerificationWindow(2, 2)))
				{
					UserManager userManager = new UserManager();
					User user = userManager.GetUserByLogin(Page.User.Identity.Name);
					user.StartTracking();
					user.MfaToken = SecretKey;
					user.LastActivityDate = DateTime.UtcNow;
					userManager.Update(user);

					//Success, so just redirect back to the user profile page
					Response.Redirect("~/UserProfile.aspx");
				}
				else
				{
					this.lblMessage.Text = Resources.Messages.ChangeMfa_Otp_Invalid2;
					this.lblMessage.Type = ServerControls.MessageBox.MessageType.Error;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (ArtifactNotExistsException)
			{
				this.lblMessage.Text = Resources.Messages.ChangeMfa_UserNotExists2;
				this.lblMessage.Type = ServerControls.MessageBox.MessageType.Error;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				this.lblMessage.Text = exception.Message;
				this.lblMessage.Type = ServerControls.MessageBox.MessageType.Error;
			}
		}
	}
}
