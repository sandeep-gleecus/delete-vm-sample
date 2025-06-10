using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Web.Security;
using System.Collections;

using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using System.IO;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// Changes the PasswordRecovery control to use the SpiraTest notification system instead of the built-in one
    /// </summary>
    [ToolboxData("<{0}:PasswordRecoveryEx runat=server></{0}:PasswordRecoveryEx>")]
    public class PasswordRecoveryEx : PasswordRecovery
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.ServerControls.PasswordRecoveryEx::";

        /// <summary>
        /// Constructor
        /// </summary>
        public PasswordRecoveryEx() : base()
        {
            const string METHOD_NAME = CLASS_NAME + "PasswordRecoveryEx";

            Logger.LogEnteringEvent(METHOD_NAME);

            try
            {
                //Specify the mail definition programatically

                //The only field we need from the built-in mail sending system is the unecrypted password
                //so we set the message to be just the password token
                const string messageTemplate = "<%Password%>";

                //Save it as a specific filename since that is expected by the mail sending system
                string messageTemplateFolder = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Inflectra");
                string messageTemplatePath = Path.Combine(messageTemplateFolder, @"SpiraTest_PasswordRecovery.txt");
                if (!File.Exists(messageTemplatePath))
                {
                    if (!Directory.Exists(messageTemplateFolder))
                    {
                        Directory.CreateDirectory(messageTemplateFolder);
                    }
                    StreamWriter streamWriter = File.CreateText(messageTemplatePath);
                    streamWriter.Write(messageTemplate);
                    streamWriter.Close();
                }
                this.MailDefinition.BodyFileName = messageTemplatePath;
                this.MailDefinition.Subject = "N/A";   //Not actually used
                this.MailDefinition.From = Common.ConfigurationSettings.Default.EmailSettings_EMailFrom;

                //We need to also handle the special cases of email being disabled and the user being an LDAP user
                this.VerifyingUser +=new LoginCancelEventHandler(PasswordRecoveryEx_VerifyingUser);

                //Register the event handler that intercepts the mail and sends it through SpiraTest instead
                this.SendingMail += new MailMessageEventHandler(PasswordRecoveryEx_SendingMail);
                this.SendMailError += new SendMailErrorEventHandler(PasswordRecoveryEx_SendMailError);

                Logger.LogExitingEvent(METHOD_NAME);
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Makes sure that email is enabled and that the user is not an LDAP managed user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PasswordRecoveryEx_VerifyingUser(object sender, LoginCancelEventArgs e)
        {
 	        //Make sure that the email system is enabled
            if (!ConfigurationSettings.Default.EmailSettings_Enabled)
            {
                e.Cancel = true;
                this.SetFailureTextLabel(this.UserNameTemplateContainer, Resources.Messages.EmailPassword_EmailNotEnabled);
            }
            else
            {
                //Make sure that this user is not an LDAP managed user
                SpiraMembershipProvider provider = (SpiraMembershipProvider)Membership.Provider;
                bool isLdap = provider.IsLdapUser(this.UserName);
                if (isLdap)
                {
                    e.Cancel = true;
                    this.SetFailureTextLabel(this.UserNameTemplateContainer, String.Format(Resources.Messages.EmailPassword_NotResetLDAP, ConfigurationSettings.Default.License_ProductType));
                }
            }
        }

        /// <summary>
        /// Sets the failure text
        /// </summary>
        /// <param name="container"></param>
        /// <param name="failureText"></param>
        private void SetFailureTextLabel(Control container, string failureText)
        {
            ITextControl failureTextLabel = (ITextControl)container.FindControl("FailureText");
            if (failureTextLabel != null)
            {
                failureTextLabel.Text = failureText;
            }
        }

        /// <summary>
        /// Logs any email errors
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PasswordRecoveryEx_SendMailError(object sender, SendMailErrorEventArgs e)
        {
            const string METHOD_NAME = "PasswordRecoveryEx_SendMailError";

            Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, e.Exception);
        }

        /// <summary>
        /// Called when mail is sent, used to customize the message if needed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PasswordRecoveryEx_SendingMail(object sender, MailMessageEventArgs e)
        {
            const string METHOD_NAME = CLASS_NAME + "PasswordRecoveryEx_SendingMail";
            Logger.LogEnteringEvent(METHOD_NAME);

            //Send the email to reset the user's password
            new UserManager().SendPasswordResetNotification(this.UserName, e.Message.Body);

            //Finally, set the cancelled flag, we sent it manually.
            e.Cancel = true;

            Logger.LogExitingEvent(METHOD_NAME);
        }
    }
}