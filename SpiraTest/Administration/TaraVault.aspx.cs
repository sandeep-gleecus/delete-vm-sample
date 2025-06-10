using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inflectra.SpiraTest.Web.Administration
{
	[HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_TaraVaultConfig", "System-Administration", "Admin_TaraVaultConfig")]
	[AdministrationLevelAttribute(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)]
	public partial class TaraVaultAdmin : AdministrationBase
	{
		private const string CLASS_NAME = "Web.Administration.TaraVault::";

		#region Events
		/// <summary>This sets up the page upon loading</summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = CLASS_NAME + "Page_Load";
			Logger.LogEnteringEvent(METHOD_NAME);

            //set the TaraVault logo url
            this.imgTaraVaultLogo.ImageUrl = "Images/product-TaraVault.svg";
            this.imgTaraVaultLogo.AlternateText = "TaraVault";

			if (!IsPostBack)
                LoadAndBindData();

            //Register event handlers
            this.btnProjActivate.Click += btnProjActivate_Click;
            this.btnProjDeactivate.Click += btnProjDeactivate_Click;
            this.btnActivate.Click += btnTaraActivate_Click;
            this.btnRefreshCache.Click += btnRefreshCache_Click;
            this.btnDeleteCache.Click += BtnDeleteCache_Click;

			Logger.LogExitingEvent(METHOD_NAME);
		}

        /// <summary>Clears the version control cache</summary>
        /// <param name="sender">btnClearCache</param>
        /// <param name="e">EventArgs</param>
        private void BtnDeleteCache_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnRefreshCache_Click";
            try
            {
                //Create instance..
                SourceCodeManager sourceCodeManager = new SourceCodeManager(this.ProjectId);

                //Delete cache
                sourceCodeManager.ClearCacheAndRefresh();

                //Reload page data
                LoadAndBindData();

                this.lblMessage.Text = Resources.Messages.Admin_VersionControl_ReloadingCache;
                this.lblMessage.Type = MessageBox.MessageType.Information;
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                lblMessage.Text = exception.Message;
                lblMessage.Type = MessageBox.MessageType.Error;
            }
        }

        /// <summary>Reloads (without clearing) the version control cache</summary>
        /// <param name="sender">btnClearCache</param>
        /// <param name="e">EventArgs</param>
        void btnRefreshCache_Click(object sender, EventArgs e)
		{
			const string METHOD_NAME = "btnRefreshCache_Click";
			try
			{
				//Create instance..
				SourceCodeManager sourceCodeManager = new SourceCodeManager(this.ProjectId);

                //Force reload..
                sourceCodeManager.LaunchCacheRefresh();

				//Reload page data
				LoadAndBindData();

				this.lblMessage.Text = Resources.Messages.Admin_VersionControl_ReloadingCache;
				this.lblMessage.Type = MessageBox.MessageType.Information;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                lblMessage.Text = exception.Message;
                lblMessage.Type = MessageBox.MessageType.Error;
			}
		}

		/// <summary>Hit when the user clicks the 'Activate TaraVault' button.</summary>
		/// <param name="sender">brnTaraActivate</param>
		/// <param name="e">EventArgs</param>
		private void btnTaraActivate_Click(object sender, EventArgs e)
		{
            try
            {
                //Activate the system.
                new VaultManager().Account_Activate();

                LoadAndBindData();
            }
            catch (Exception exception)
            {
                //Display the 'nice message'
                this.lblMessage.Text = Resources.Messages.Admin_TaraVault_UnableToActivate + " - " + exception.Message;
                this.lblMessage.Type = MessageBox.MessageType.Error;
            }
        }

		/// <summary>Hit when the user clicks the 'Deactivate Project' button.</summary>
		/// <param name="sender">btnProjDeactivate</param>
		/// <param name="e">EventArgs</param>
		private void btnProjDeactivate_Click(object sender, EventArgs e)
		{
			//The user wants to deactivate this project.
			//Remove all users from the project..
			new VaultManager().Project_DeleteTaraVault(ProjectId);

            //Refresh page data.
            LoadAndBindData();
		}

		/// <summary>Hit when the user clicks the 'Activate Project' button.</summary>
		/// <param name="sender">btnProjActivate</param>
		/// <param name="e">EventArgs</param>
		private void btnProjActivate_Click(object sender, EventArgs e)
		{
			//The user was sure, let's activate the project!

			//Filter out the project name for proper characters.
			string projname = "";
			foreach (char c in txtProjectName.Text.Trim())
				if (!((c < 48 || c > 57) && (c < 65 || c > 90) && (c < 97 || c > 122)))
					projname += c;

			try
			{
				// Create the project.
                int vaultTypeId = Int32.Parse(ddlProjectType.SelectedValue);
                VaultManager vaultManager = new VaultManager();
				vaultManager.Project_CreateUpdateTaraVault(ProjectId, (VaultManager.VaultTypeEnum)vaultTypeId, projname);

                lblMessage.Text = Resources.Messages.Admin_TaraVault_ProjCreated;
                lblMessage.Type = MessageBox.MessageType.Information;
			}
			catch (ConflictException)
			{
                //Name already exists. Tell user to create a new one.
                lblMessage.Text = Resources.Messages.Admin_TaraVault_ProjectNameExists;
                lblMessage.Type = MessageBox.MessageType.Error;
			}
			catch (Exception ex2)
			{
				//Log it, in case.
				Logger.LogErrorEvent(CLASS_NAME + "btnProjActivate_Click()", ex2);
                //Name already exists. Tell user to create a new one.
                lblMessage.Text = Resources.Messages.Admin_TaraVault_ErrorCreatingProject;
                lblMessage.Type = MessageBox.MessageType.Error;
			}

            //Refresh page data.
            LoadAndBindData();
		}
		#endregion Events

		/// <summary>Loads the page's data.</summary>
		protected void LoadAndBindData()
		{
			const string METHOD_NAME = CLASS_NAME + "LoadAndBindData()";
			Logger.LogEnteringEvent(METHOD_NAME);

            //Populate some of the labels
            locIntro2.Text = String.Format(Resources.Main.Admin_TaraVaultConfig_Intro2, ConfigurationSettings.Default.License_ProductType);
            locIntro3.Text = String.Format(Resources.Main.Admin_TaraVaultConfig_Intro3, "<a href=\"" + UrlRewriterModule.ResolveUrl("~/Administration/UserList.aspx") + "\">" + Resources.Main.UserList_Title + "</a>");

			//Set activate text first..
			if (!ConfigurationSettings.Default.TaraVault_HasAccount)
			{
                if (ConfigurationSettings.Default.TaraVault_UserLicense > 0)
                    lblActivate.Text = String.Format(
                        Resources.Messages.Admin_TaraVault_NotActive,
                        ConfigurationSettings.Default.License_ProductType);
                else
                {
                    lblActivate.Text = Resources.Messages.Admin_TaraVault_NotActiveNoService;
                    btnActivate.Visible = false;
                }
            }
            else
			{
                lblActivate.Text = String.Format(
					Resources.Messages.Admin_TaraVault_Active,
                    Resources.Main.Global_Unlimited,
					ConfigurationSettings.Default.License_ProductType);
			}
            //Set project name..
            ltrProjectName.Text = ProjectName;

            //Display/Hide the proper sections
            divNotActivated.Visible = (!ConfigurationSettings.Default.TaraVault_HasAccount);
            txtIsActive.Visible = (ConfigurationSettings.Default.TaraVault_HasAccount);
            divProjectSettings.Visible = (ConfigurationSettings.Default.TaraVault_HasAccount);

            //Load the dropdown..
            ddlProjectType.DataSource = new Dictionary<int, string>() { { 2, "GIT" }, { 1, "SVN" } };
            ddlProjectType.DataBind();

			//Set the main Account status..
			if (ConfigurationSettings.Default.TaraVault_HasAccount)
			{
                //It is active, so show our controls..
                rowTaraAcctUsers.Visible = true;
                rowTaraAcct.Visible = true;
                divProjectProperties.Visible = true;

                //Set our fields..
                lblAccountName.Text = ConfigurationSettings.Default.TaraVault_AccountName;
                lblAccountId.Text = ConfigurationSettings.Default.TaraVault_AccountId.ToString();
                lblAccountUsers.Text = Resources.Main.Global_Unlimited; //TaraVault is now free unlimited users
                lblActiveUsers.Text = new VaultManager().User_RetrieveAllTVActive().Count().ToString();
                lblActiveProjects.Text = new VaultManager().Project_RetrieveTaraDefined().Count.ToString();

                //Now load project information.
                if (ProjectId > 0)
				{
                    //Show data, hide message.
                    divProjectProperties.Visible = true;
                    divNoProjectSelected.Visible = false;

					//Load Project details..
					VaultManager vMgr = new VaultManager();
                    DataModel.Project proj = vMgr.Project_RetrieveWithTaraVault(ProjectId);
					if (proj.TaraVault != null)
					{
                        //We have data. Show the rows..
                        rowProjectName.Visible = true;
                        rowProjectType.Visible = true;
                        rowProjectUsers1.Visible = true;
                        rowProjectUsers2.Visible = true;
                        btnProjDeactivate.Visible = true;
                        btnProjActivate.Visible = false;
                        lblProjectType.Visible = true;

                        //Set labels..
                        lblProjectActive.Text = Resources.Main.Global_Yes;
                        txtProjectName.Text = proj.TaraVault.Name;
                        lblProjectType.Text = proj.TaraVault.VaultType.Name;
                        this.lblProjectId.Text = proj.TaraVault.VaultId.ToString();

                        //Disable the properties, so they can't be changed..
                        txtProjectName.Enabled = false;
                        ddlProjectType.Enabled = false;
                        ddlProjectType.Visible = false;

						//Load users assigned to this project..
						List<DataModel.User> users = new UserManager().GetUsersAssignedToTaraVaultProject(ProjectId);
                        grdUserList.DataSource = users;
                        grdUserList.VirtualItemCount = users.Count();
                        grdUserList.DataBind();

                        //Display the connect URL..
                        lblProjConnection.Text = vMgr.Project_GetConnectionString(proj.ProjectId);

                        //Display if the cache is running or not
                        if (SourceCodeManager.IsCacheUpdateRunning)
                        {
                            this.lblMessage.Text = Resources.Messages.VersionControl_CacheUpdateRunning;
                            this.lblMessage.Type = ServerControls.MessageBox.MessageType.Error;
                        }
                        else
                        {
                            this.lblMessage.Text = Resources.Messages.VersionControl_CacheUpToDate;
                            this.lblMessage.Type = ServerControls.MessageBox.MessageType.Information;
                        }
					}
					else
					{
                        //We have no data, so enable the editable fields..
                        ddlProjectType.Visible = true;
                        rowProjectUsers1.Visible = false;
                        rowProjectUsers2.Visible = false;
                        lblProjectType.Visible = false;

                        //Set our label..
                        lblProjectActive.Text = Resources.Main.Global_No;
                        txtProjectName.Enabled = true;
                        txtProjectName.Text = "";
                        ddlProjectType.Enabled = true;


                        //Enable the right button..
                        btnProjDeactivate.Visible = false;
                        btnRefreshCache.Visible = false;
                        btnDeleteCache.Visible = false;
                        btnProjActivate.Visible = true;

                        //Show the correct message that the project is not yet activated
                        lblMessage.Text = Resources.Messages.Admin_TaraVault_ProjectNotActivated;
                        lblMessage.Type = MessageBox.MessageType.Information;
					}

				}
				else
				{
                    //Hide data, show message.
                    divProjectProperties.Visible = false;
                    divNoProjectSelected.Visible = true;
				}
			}
			else
			{
                //No account is enabled. Hide everything, except the 'Activate' button.
                rowTaraAcct.Visible = false;
                rowTaraAcctUsers.Visible = false;
                divProjectProperties.Visible = false;
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}
	}
}
