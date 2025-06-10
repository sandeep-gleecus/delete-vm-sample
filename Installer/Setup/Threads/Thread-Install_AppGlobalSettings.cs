using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

using Inflectra.SpiraTest.Installer.HelperClasses;
using Inflectra.SpiraTest.Installer.UI;

namespace Inflectra.SpiraTest.Installer.Threads
{
	internal partial class ProcessThread
	{
		/// <summary>Updates the location to store file attachments for the fresh install case</summary>
		private bool Install_ApplicationGlobalSettings(StreamWriter streamWriter, out FinalStatusEnum status, bool updateAttachment)
		{
			int TaskDisplayLine = 5;
			status = FinalStatusEnum.OK;
			ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Processing, TaskNum = TaskDisplayLine });
			streamWriter.WriteLine("Updating the location to store file attachments for the fresh install case");

			try
			{
				//Get the various properties
				string dbServer = App._installationOptions.SQLServerAndInstance;
				AuthenticationMode dbAuthType = App._installationOptions.DatabaseAuthentication;
				string dbName = App._installationOptions.DatabaseName;
				string dbUser = App._installationOptions.SQLInstallLogin;
				string dbPassword = App._installationOptions.SQLInstallPassword;
				string attachmentFolder = Path.Combine(App._installationOptions.InstallationFolder, "Attachments");
				string themeName = Themes.Inflectra.Resources.Global_ThemeName;
				string companyName = App._installationOptions.Organization;
				string licenseKey = App._installationOptions.LicenseKey;
				string productType = App._installationOptions.ProductName;

				//First we need to establish a connection with the database
				using (SqlConnection connection = new SqlConnection())
				{
					connection.ConnectionString = SQLUtilities.GenerateConnectionString(dbName, dbServer, dbAuthType, dbUser, dbPassword);
					connection.Open();

					//Now we need to update the attachments folder information
					using (SqlCommand command = new SqlCommand())
					{
						//Set connection string.
						command.Connection = connection;
						command.CommandType = CommandType.Text;

						//Update the Attachment Directory
						#region Attachment Record
						if (updateAttachment)
						{
							command.CommandText = "UPDATE TST_GLOBAL_SETTING SET VALUE = '" + SQLUtilities.SqlEncode(attachmentFolder) + "' WHERE NAME = 'General_AttachmentFolder'";
							command.ExecuteNonQuery();
						}
						#endregion Attachment Record

						//Update license details.
						#region License Details
						command.CommandText = "DELETE FROM TST_GLOBAL_SETTING WHERE NAME = 'License_Organization' OR NAME = 'License_LicenseKey' OR NAME = 'License_ProductType'";
						command.ExecuteNonQuery();

						//Now we need to insert the new license information
						command.CommandText = "INSERT INTO TST_GLOBAL_SETTING (NAME, VALUE) VALUES ('License_Organization', '" + companyName.Replace("'", "''") + "')";
						command.ExecuteNonQuery();
						command.CommandText = "INSERT INTO TST_GLOBAL_SETTING (NAME, VALUE) VALUES ('License_LicenseKey', '" + licenseKey.Replace("'", "''") + "')";
						command.ExecuteNonQuery();
						command.CommandText = "INSERT INTO TST_GLOBAL_SETTING (NAME, VALUE) VALUES ('License_ProductType', '" + productType.Replace("'", "''") + "')";
						command.ExecuteNonQuery();

						//Update the active flag for the various products that the user can license based on the theme
						#region Theme - SmarteSoft
						//if (themeName == "SmarteSoftTheme")
						//{
						//	//Make the SmarteXXX products active
						//	command.CommandText = "UPDATE TST_PRODUCT_TYPE SET ACTIVE_YN = 'Y' WHERE NAME = 'SmarteQM'";
						//	command.ExecuteNonQuery();
						//	//Make the other products inactive
						//	command.CommandText = "UPDATE TST_PRODUCT_TYPE SET ACTIVE_YN = 'N' WHERE NAME = 'SpiraTest'";
						//	command.ExecuteNonQuery();
						//	command.CommandText = "UPDATE TST_PRODUCT_TYPE SET ACTIVE_YN = 'N' WHERE NAME = 'SpiraPlan'";
						//	command.ExecuteNonQuery();
						//	command.CommandText = "UPDATE TST_PRODUCT_TYPE SET ACTIVE_YN = 'N' WHERE NAME = 'SpiraTeam'";
						//	command.ExecuteNonQuery();
						//	command.CommandText = "UPDATE TST_PRODUCT_TYPE SET ACTIVE_YN = 'N' WHERE NAME = 'DiagZ'";
						//	command.ExecuteNonQuery();
						//	command.CommandText = "UPDATE TST_PRODUCT_TYPE SET ACTIVE_YN = 'N' WHERE NAME = 'CodeWorksALM'";
						//	command.ExecuteNonQuery();
						//	command.CommandText = "UPDATE TST_PRODUCT_TYPE SET ACTIVE_YN = 'N' WHERE NAME = 'ValidationMaster'";
						//	command.ExecuteNonQuery();

						//	//Also need to make sure any references to the Rapise automation engine become SmarteStudio
						//	//And that any settings are branded correctly
						//	try
						//	{
						//		command.CommandText = "UPDATE TST_AUTOMATION_ENGINE SET NAME = 'SmarteStudio', TOKEN = 'SmarteStudio' WHERE TOKEN = 'Rapise'";
						//		command.ExecuteNonQuery();
						//		command.CommandText = "UPDATE TST_GLOBAL_SETTING SET VALUE = 'C:\\ProgramData\\SmarteSoft\\SmarteQM' WHERE NAME = 'Cache_Folder'";
						//		command.ExecuteNonQuery();
						//		command.CommandText = "INSERT INTO TST_GLOBAL_SETTING (NAME, VALUE) VALUES ('General_WebServerUrl', 'http://localhost/SmarteQM')";
						//		command.ExecuteNonQuery();
						//	}
						//	catch (Exception)
						//	{
						//		//Fail quietly
						//	}
						//}
						#endregion Theme - SmarteSoft
						#region Theme - ValidationMasterTheme
						//else if (themeName == "ValidationMasterTheme")
						{
							//Make the ValidationMaster products active
							command.CommandText = "UPDATE TST_PRODUCT_TYPE SET ACTIVE_YN = 'Y' WHERE NAME = 'ValidationMaster'";
							command.ExecuteNonQuery();
							//Make the other products inactive
							command.CommandText = "UPDATE TST_PRODUCT_TYPE SET ACTIVE_YN = 'N' WHERE NAME = 'SpiraTest'";
							command.ExecuteNonQuery();
							command.CommandText = "UPDATE TST_PRODUCT_TYPE SET ACTIVE_YN = 'N' WHERE NAME = 'SpiraPlan'";
							command.ExecuteNonQuery();
							command.CommandText = "UPDATE TST_PRODUCT_TYPE SET ACTIVE_YN = 'N' WHERE NAME = 'SpiraTeam'";
							command.ExecuteNonQuery();
							command.CommandText = "UPDATE TST_PRODUCT_TYPE SET ACTIVE_YN = 'N' WHERE NAME = 'DiagZ'";
							command.ExecuteNonQuery();
							command.CommandText = "UPDATE TST_PRODUCT_TYPE SET ACTIVE_YN = 'N' WHERE NAME = 'CodeWorksALM'";
							command.ExecuteNonQuery();
							command.CommandText = "UPDATE TST_PRODUCT_TYPE SET ACTIVE_YN = 'N' WHERE NAME = 'SmarteQM'";
							command.ExecuteNonQuery();

							//Also need to make sure any settings are branded correctly
							try
							{
								command.CommandText = "UPDATE TST_GLOBAL_SETTING SET VALUE = 'C:\\ProgramData\\ValidationMaster' WHERE NAME = 'Cache_Folder'";
								command.ExecuteNonQuery();
								command.CommandText = "INSERT INTO TST_GLOBAL_SETTING (NAME, VALUE) VALUES ('General_WebServerUrl', 'http://localhost/ValidationMaster')";
								command.ExecuteNonQuery();
							}
							catch (Exception)
							{
								//Fail quietly
							}
						}
						#endregion Theme - ValidationMasterTheme
						#region Theme - Inflectra
						//else
						//{
						//	//Make the SpiraXXX products active
						//	command.CommandText = "UPDATE TST_PRODUCT_TYPE SET ACTIVE_YN = 'Y' WHERE NAME = 'SpiraTest'";
						//	command.ExecuteNonQuery();
						//	command.CommandText = "UPDATE TST_PRODUCT_TYPE SET ACTIVE_YN = 'Y' WHERE NAME = 'SpiraPlan'";
						//	command.ExecuteNonQuery();
						//	command.CommandText = "UPDATE TST_PRODUCT_TYPE SET ACTIVE_YN = 'Y' WHERE NAME = 'SpiraTeam'";
						//	command.ExecuteNonQuery();
						//	//Make the other products inactive
						//	command.CommandText = "UPDATE TST_PRODUCT_TYPE SET ACTIVE_YN = 'N' WHERE NAME = 'SmarteQM'";
						//	command.ExecuteNonQuery();
						//	command.CommandText = "UPDATE TST_PRODUCT_TYPE SET ACTIVE_YN = 'N' WHERE NAME = 'DiagZ'";
						//	command.ExecuteNonQuery();
						//	command.CommandText = "UPDATE TST_PRODUCT_TYPE SET ACTIVE_YN = 'N' WHERE NAME = 'CodeWorksALM'";
						//	command.ExecuteNonQuery();
						//	command.CommandText = "UPDATE TST_PRODUCT_TYPE SET ACTIVE_YN = 'N' WHERE NAME = 'ValidationMaster'";
						//	command.ExecuteNonQuery();
						//}
						#endregion Theme - Inflectra

						#endregion
					}

					//Close the connection
					connection.Close();
				}

				//Finally we need to make the attachment folder read/write
				streamWriter.WriteLine("Setting the permissions of the attachments folder to be read/write");
				if (System.IO.Directory.Exists(attachmentFolder))
				{
					//Change file permissions to give NETWORK SERVICE Full Control
					DirectoryInfo dInfo = new DirectoryInfo(attachmentFolder);
					DirectorySecurity dSecurity = dInfo.GetAccessControl();
					//dSecurity.SetAccessRuleProtection(false, true);
					dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null), FileSystemRights.FullControl | FileSystemRights.CreateFiles | FileSystemRights.Modify | FileSystemRights.Read | FileSystemRights.ListDirectory | FileSystemRights.ReadAndExecute | FileSystemRights.Write, InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
					dInfo.SetAccessControl(dSecurity);
				}

				ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Success, TaskNum = TaskDisplayLine });
				return true;
			}
			catch (Exception ex)
			{
				//Log error.
				streamWriter.WriteLine("Error updating Application Golbal Settings: " + ex.Message);

				string ErrorMsg = "Error updating Application Golbal Settings:" + Environment.NewLine + ex.Message;
				ProgressUpdate(this, new ProgressArgs() { ErrorText = ErrorMsg, Progress = -1, Status = ItemProgress.ProcessStatusEnum.Error, TaskNum = TaskDisplayLine });
				return false;
			}
		}
	}
}
