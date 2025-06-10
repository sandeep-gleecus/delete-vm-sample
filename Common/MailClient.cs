using System;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;

namespace Inflectra.SpiraTest.Common
{
	/// <summary>This contains a generic facade for sending email messages from within SpiraTest</summary>
	/// <remarks>Current uses the Microsoft.NET SmtpClient to send mail messages</remarks>
	public class MailClient
	{
		private const string CLASS = "Inflectra.SpiraTest.Common.MailClient::";

		/// <summary>Header for emails with an artifact.</summary>
		public const string MESSAGEHEADER_SPIRA_ARTIFACT = "X-Inflectra-Spira-Art";
		public const string MESSAGEHEADER_SPIRA_DATE = "X-Inflectra-Spira-Dte";
		public const string MESSAGEHEADER_SPIRA_USERTO = "X-Inflectra-Spira-UsrTo";
		public const string MESSAGEHEADER_SPIRA_PROJECT = "X-Inflectra-Spira-Prj";
		public const string MESSAGEHEADER_SPIRA_SOURCE = "X-Inflectra-Spira-Src";
		public const string MESSAGEHEADER_INREPLYTO = "In-Reply-To";
		public const string MESSAGEHEADER_REFERENCES = "References";

		/// <summary>Sends an email message using the application's email setup.</summary>
		/// <param name="message">The message to send.</param>
		public static void SendMessage(MailMessage message)
		{
			const string METHOD = CLASS + "SendMessage()";
			Logger.LogEnteringEvent(METHOD);

			if (ConfigurationSettings.Default.EmailSettings_Enabled)
			{
				if (message.To != null && message.To.Count > 0 && message.From != null && !string.IsNullOrWhiteSpace(message.From.Address))
				{
					//Get the mail settings from the global configuration settings
					string mailServer = ConfigurationSettings.Default.EmailSettings_MailServer;
					int mailServerPort = ConfigurationSettings.Default.EmailSettings_MailServerPort;
					string smtpUser = ConfigurationSettings.Default.EmailSettings_SmtpUser;
					string smtpPassword = ConfigurationSettings.Default.EmailSettings_SmtpPassword;
					bool useSSL = ConfigurationSettings.Default.EmailSettings_UseSSL;

					//So that we don't impact the overall application performance we're going to run the email
					//action outside of the primary ASP.NET worker threads - improves performance
					//See http://jdconley.com/blog/archive/2009/01/14/fire-and-forget-email-webservices-and-more-in-asp.net.aspx
					using (new SynchronizationContextSwitcher())
					{
						try
						{
							//Send the message using the SMTP method specified (if provided)
							//Default to localhost and port 25 (if nothing else specified)
							SmtpClient smtpClient;
							if (String.IsNullOrEmpty(mailServer))
							{
								smtpClient = new SmtpClient("localhost");
								smtpClient.DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis;
							}
							else
							{
								//Specify the mail server and port
								smtpClient = new SmtpClient(mailServer, mailServerPort);

								//Specify that we're using an existing SMTP server
								smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

								//Specify whether we using SSL or not
								smtpClient.EnableSsl = useSSL;
							}

							//Use SMTP authentication if provided
							if (smtpUser != null && smtpUser != "" && smtpPassword != null && smtpPassword != "")
								smtpClient.Credentials = new NetworkCredential(smtpUser, smtpPassword);

							//Trace logging
							Logger.LogTraceEvent(METHOD, "Sending email to '" + smtpClient.Host + ":" + smtpClient.Port.ToString() + "'");

							//Send the message asynchronously
							object userState = message;
							smtpClient.SendCompleted += new SendCompletedEventHandler(smtpClient_SendCompleted);
							smtpClient.SendAsync(message, userState);
						}
						catch (Exception ex)
						{
							Logger.LogErrorEvent(METHOD, ex, "Sending email message.");
						}
					}
				}
				else
				{
					Logger.LogWarningEvent(METHOD, "Unable to send message - From: or To: address empty.");
				}
			}
			else
			{
				Logger.LogTraceEvent(METHOD, "Email sending disabled in administration. Not sending message.");
			}

			Logger.LogExitingEvent(METHOD);
		}

        /// <summary>Sends a mail message to the specified person.</summary>
        /// <param name="fromEmailAddress">The e-mail address the mail is sent from.</param>
        /// <param name="toEmailAddress">The e-mail address the mail is being sent to.</param>
        /// <param name="subject">The subject line of the message.</param>
        /// <param name="body">The body of the message.</param>
        /// <param name="isHtml">Whether the message is HTML encoded or not.</param>
        /// <remarks>No translations are done on the fields - the email is sent out exactly as the fields say.</remarks>
        /// <remarks>Same function as used in KronoDesk but this currently not being used as of 2017-12</remarks>
        //public static void SendMessage(string fromEmailAddress, string toEmailAddress, string subject, string body, bool isHtml, string replyToAddress = null)
        //{
        //	const string METHOD = "SendMessage()";
        //	Logger.LogEnteringEvent(METHOD);

        //	if (!string.IsNullOrWhiteSpace(fromEmailAddress))
        //	{
        //		if (!string.IsNullOrWhiteSpace(toEmailAddress))
        //		{
        //			//Create the Mail Message..
        //			MailMessage msg = new MailMessage(fromEmailAddress, toEmailAddress, subject, body);
        //			if (!string.IsNullOrWhiteSpace(replyToAddress))
        //				msg.ReplyToList.Add(new MailAddress(replyToAddress));

        //			MailClient.SendMessage(msg);
        //		}
        //		else
        //		{
        //			throw new ArgumentNullException("toEmailAddress", "You must provide an address to send the email to.");
        //		}
        //	}
        //	else
        //	{
        //		throw new ArgumentNullException("fromEmailAddress", "You must provide an address to send emails from.");
        //	}

        //	Logger.LogExitingEvent(METHOD);
        //}

        /// <summary>Called when the SMTP server returns.</summary>
        /// <param name="sender">The SmtpClient</param>
        /// <param name="e">AsyncCompletedEventArgs</param>
        static void smtpClient_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            const string METHOD = CLASS + "smtpClient_SendCompleted()";
            Logger.LogEnteringEvent(METHOD);

            //Log if the email was sent or not
            if (e.Cancelled)
                Logger.LogErrorEvent(METHOD, "SMTP asynchronous send was cancelled.");
            else if (e.Error != null)
                Logger.LogErrorEvent(METHOD, e.Error, "Callback from SMTP Send:");
            else
                Logger.LogTraceEvent(METHOD, "SMTP Send successful.");

            Logger.LogExitingEvent(METHOD);
        }

        /// <summary>Converts the given date/time (or 'Now') into the string used in the message headers.</summary>
        /// <param name="date">The date to convery, if null uses current.</param>
        /// <returns>String used for the email header.</returns>
        public static string header_GenerateDate(DateTime? date = null)
		{
			//Essentially, the format is:
			// - YYYYMMDD_HHMMSS_Z
			if (!date.HasValue)
				date = DateTime.Now;

			return date.Value.ToString(@"yyyyMMdd\_HHmmss\_zzz");
		}
	}
}
