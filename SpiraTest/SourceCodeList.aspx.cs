using System;
using System.Web.UI;
using System.Collections.Generic;
using System.Linq;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Microsoft.Security.Application;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// Project Source Code Repository List and handling all raised events
    /// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.SourceCode, "SourceCodeList_Title", "Source-Code/#source-code-file-list")]
    public partial class SourceCodeList : SourceCodePage
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.SourceCodeList::";

        protected string CurrentBranchKey
        {
            get;
            set;
        }

        /// <summary>
        /// This sets up the page upon loading
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The page load event arguments</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we're active for source code
			bool isActiveForSourceCode = new SourceCodeManager().IsActiveProvidersForProject(ProjectId);
			if (isActiveForSourceCode)
			{
				LoadAndBindData();
			}
			else
			{
				this.lblServerMessage.Type = MessageBox.MessageType.Warning;
				this.lblServerMessage.Text = Resources.Messages.SourceCode_ProjectNotEnabled;
				this.grdSourceCodeFileList.Visible = false;
				this.plcToolbar.Visible = false;
				this.btnEnterCatch.Visible = false;
				this.pnlFolders.Visible = false;
				this.trvFolders.Visible = false;
				this.lblMessage.Visible = false;
			}

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Loads the page data
        /// </summary>
        protected void LoadAndBindData()
        {
            //Populate the user and project id in the treeview
            this.trvFolders.ContainerId = this.ProjectId;

            //We need to make sure that the system is licensed for source code repository viewing
            if (License.LicenseProductName == LicenseProductNameEnum.SpiraTest)
            {
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.SourceCodeList_InvalidLicense, true);
            }

            //We need to determine what permissions the user has
            ProjectManager projectManager = new ProjectManager();
            ProjectRole projectRole = projectManager.RetrieveRolePermissions(ProjectRoleId);

            if (projectRole == null)
            {
                //No role found, so redirect
                Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.SourceCodeList_InvalidRole, true);
            }
            else
            {
                //See what role we have and display a message if necessary

                //View SourceCode must be Yes or the user must be a system admin
                if (!projectRole.IsSourceCodeView && !UserIsAdmin)
                {
                    Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.SourceCodeList_InvalidRole, true);
                }
            }

            //Specify the context for the folders sidebar
            this.pnlFolders.ProjectId = ProjectId;
            this.pnlFolders.Minimized = GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_MINIMIZED, false);
            this.pnlFolders.BodyWidth = Unit.Pixel(GetProjectSetting(GlobalFunctions.PROJECT_SETTINGS_GLOBAL_SIDEBAR_PANEL, GlobalFunctions.PROJECT_SETTINGS_KEY_WIDTH, 175));

            //Populate the user and project id in the grid control
            this.grdSourceCodeFileList.ProjectId = this.ProjectId;

            //We need to add some client-code that handles clicking on folders in the grid
            Dictionary<string, string> handlers = new Dictionary<string, string>();
            handlers.Add("focusOn", "grdSourceCodeFileList_focusOn");
            this.grdSourceCodeFileList.SetClientEventHandlers(handlers);

            //Custom CSS for the grid
            Dictionary<string, string> sourceCodeCssClasses = new Dictionary<string, string>();
            sourceCodeCssClasses.Add("Size", "priority3");
            this.grdSourceCodeFileList.SetCustomCssClasses(sourceCodeCssClasses);

            //Reset the error messages
            this.lblMessage.Text = "";
            this.lblServerMessage.Text = "";

            //See if we have a stored node that we need to populate
            //Also load the branch list
            try
            {
                //Get the list of branches for this project and add the event handler
                SourceCodeManager sourceCodeManager = new SourceCodeManager(this.ProjectId);
                CurrentBranchKey = LoadBranchList(sourceCodeManager, this.mnuBranches);

                //Set the current branch
                Dictionary<string, object> standardFilters = new Dictionary<string, object>();
                standardFilters.Add("BranchKey", CurrentBranchKey);
                this.grdSourceCodeFileList.SetFilters(standardFilters);

                //Populate the repository name, unless we have TaraVault, in which case show SVN/Git
                if (sourceCodeManager.RepositoryName == VaultManager.SOURCE_CODE_PROVIDER_TARA_VAULT)
                {
                    VaultManager vaultManager = new VaultManager();
                    Project tvProject = vaultManager.Project_RetrieveWithTaraVault(ProjectId);
                    User tvUser = vaultManager.User_RetrieveWithTaraVault(UserId);
                    this.lblRepositoryName.Text = Resources.Fields.Type + ": " + tvProject.TaraVault.VaultType.Name;

                    //Also populate the checkout/clone info, need to make sure user is a member of this TV project
                    if (tvUser != null && tvUser.TaraVault != null && tvUser.TaraVault.VaultProject.Any(p => p.ProjectId == ProjectId))
                    {
                        this.plcTaraVault.Visible = true;
                        if (tvProject.TaraVault.VaultTypeId == (int)VaultManager.VaultTypeEnum.Git)
                        {
                            this.ltrCloneCheckoutTitle.Text = Resources.Main.SourceCodeList_CloneTitle;
                            this.ltrCloneCheckoutIntro.Text = Resources.Main.SourceCodeList_CloneIntro;
                        }
                        if (tvProject.TaraVault.VaultTypeId == (int)VaultManager.VaultTypeEnum.Subversion)
                        {
                            this.ltrCloneCheckoutTitle.Text = Resources.Main.SourceCodeList_CheckoutTitle;
                            this.ltrCloneCheckoutIntro.Text = Resources.Main.SourceCodeList_CheckoutIntro;
                        }
                        this.txtCloneOrCheckoutUrl.Text = vaultManager.Project_GetConnectionString(this.ProjectId);
                        this.txtTaraVaultLogin.Text = tvUser.TaraVault.VaultUserLogin;
                        this.txtTaraVaultPassword.Text = new SimpleAES().DecryptString(tvUser.TaraVault.Password);
                    }
                }
                else
                {
                    this.lblRepositoryName.Text = sourceCodeManager.RepositoryName;

                    //Show the on-premise connection instead
                    this.plcOnPremise.Visible = true;
                    this.ltrOnPremiseConnectTitle.Text = Resources.Main.SourceCodeList_VersionControlConnectTitle;
                    this.ltrOnPremiseConnectIntro.Text = Resources.Main.SourceCodeList_VersionControlConnectIntro;
                    this.txtOnPremiseConnection.Text = sourceCodeManager.Connection;
                }

                //Get the current source code folder key
                string selectedFolderKey = SourceCodeManager.Get_UserSelectedSourceFolder(this.UserId, this.ProjectId);
                if (!String.IsNullOrEmpty(selectedFolderKey))
                {
                    try
                    {
                        SourceCodeFolder sourceCodeFolder = sourceCodeManager.RetrieveFolderByKey(selectedFolderKey, CurrentBranchKey);
                        if (sourceCodeFolder != null)
                        {
                            this.spanFolderName.InnerText = sourceCodeFolder.Name;
                            this.trvFolders.SelectedNodeId = sourceCodeFolder.FolderId.ToString();
                        }
                        else
                        {
                            //This folder does not exist in this branch, so unset the folder
                            SourceCodeManager.Set_UserSelectedSourceFolder(this.UserId, this.ProjectId, null);
                        }
                    }
                    catch (ArtifactNotExistsException)
                    {
                        //Do nothing as we will just not display a folder name,
                        //happens when page refreshes after the cache is initially invalidated
                    }
                }
            }
            catch (SourceCodeProviderAuthenticationException)
            {
                this.lblServerMessage.Text = Resources.Messages.SourceCodeList_UnableToAuthenticate;
                this.lblServerMessage.Type = MessageBox.MessageType.Error;
                this.grdSourceCodeFileList.Visible = false;
                this.trvFolders.Visible = false;
            }
            catch (SourceCodeProviderLoadingException)
            {
                this.lblServerMessage.Text = Resources.Messages.SourceCodeList_UnableToLoadProvider;
                this.lblServerMessage.Type = MessageBox.MessageType.Error;
                this.grdSourceCodeFileList.Visible = false;
                this.trvFolders.Visible = false;
            }
            catch (SourceCodeProviderGeneralException)
            {
                this.lblServerMessage.Text = Resources.Messages.SourceCodeList_GeneralError;
                this.lblServerMessage.Type = MessageBox.MessageType.Error;
                this.grdSourceCodeFileList.Visible = false;
                this.trvFolders.Visible = false;
            }
            catch (SourceCodeCacheInvalidException)
            {
                this.lblServerMessage.Text = Resources.Messages.SourceCodeList_CacheNotReadyError;
                this.lblServerMessage.Type = MessageBox.MessageType.Error;
                this.grdSourceCodeFileList.Visible = false;
                this.trvFolders.Visible = false;
            }

            if (!IsPostBack)
            {
                //This is used to prevent ENTER firing the server events, since we want the grid to handle these client-side
                this.Form.DefaultButton = this.btnEnterCatch.UniqueID;
                this.btnEnterCatch.Attributes.Add("onclick", "return false;");
                this.btnEnterCatch.Style.Add(HtmlTextWriterStyle.Display, "none");
            }

            //See if we have any error message passed from the calling page
            if (Request.QueryString[GlobalFunctions.PARAMETER_ERROR_MESSAGE] != null)
            {
                //Remove any script tags, etc. to prevent cross-site scripting attacks
                string safeMessage = Encoder.HtmlEncode(Request.QueryString[GlobalFunctions.PARAMETER_ERROR_MESSAGE]);
                this.lblServerMessage.Text = safeMessage;
                this.lblServerMessage.Type = MessageBox.MessageType.Error;
            }
        }
    }
}
