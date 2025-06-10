using System;
using System.Linq;
using System.Web.UI;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using System.Collections.Generic;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible to displaying the
	/// Project Source Code Revision List and handling all raised events
	/// </summary>
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.SourceCodeRevisions, "SourceCodeRevisions_Title", "Commits/#commit-list", "SourceCodeRevisions_Title")]
    public partial class SourceCodeRevisions : SourceCodePage
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Web.SourceCodeRevisions::";

		/// <summary>
		/// This sets up the page upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
				//Make sure we're active for source code
				bool isActiveForSourceCode = new SourceCodeManager().IsActiveProvidersForProject(ProjectId);
				if (isActiveForSourceCode)
				{
					LoadAndBindData();
				}
				else
				{
					this.lblMessage.Type = MessageBox.MessageType.Warning;
					this.lblMessage.Text = Resources.Messages.SourceCode_ProjectNotEnabled;
					this.grdSourceCodeRevisionList.Visible = false;
					this.plcToolbar.Visible = false;
					this.plcLegend.Visible = false;
					this.btnEnterCatch.Visible = false;
				}
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

        protected void LoadAndBindData()
        {
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

                //View SourceCode must be Yes
                if (!projectRole.IsSourceCodeView)
                {
                    Response.Redirect(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.MyPage, this.ProjectId, 0) + "?" + GlobalFunctions.PARAMETER_ERROR_MESSAGE + "=" + Resources.Messages.SourceCodeList_InvalidRole, true);
                }
            }

            try
            {
                //Make sure that we can actually load the version control provider and also get the branch list
                SourceCodeManager sourceCodeManager = new SourceCodeManager(this.ProjectId);

                //Populate the user and project id in the grid control
                this.grdSourceCodeRevisionList.ProjectId = this.ProjectId;

                //Register client events handlers on grid
                Dictionary<string, string> handlers = new Dictionary<string, string>();
                handlers.Add("loaded", "grdSourceCodeRevisionList_loaded");
                this.grdSourceCodeRevisionList.SetClientEventHandlers(handlers);

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

                if (!IsPostBack)
                {
                    //This is used to prevent ENTER firing the server events, since we want the grid to handle these client-side
                    this.Form.DefaultButton = this.btnEnterCatch.UniqueID;
                    this.btnEnterCatch.Attributes.Add("onclick", "return false;");
                    this.btnEnterCatch.Style.Add(HtmlTextWriterStyle.Display, "none");
                }

                //Reset the error message
                this.lblMessage.Text = "";

                string currentBranchKey = LoadBranchList(sourceCodeManager, this.mnuBranches);

                //Set the current branch
                Dictionary<string, object> standardFilters = new Dictionary<string, object>();
                standardFilters.Add("BranchKey", currentBranchKey);
                this.grdSourceCodeRevisionList.SetFilters(standardFilters);
            }
            catch (SourceCodeProviderAuthenticationException)
            {
                this.lblMessage.Text = Resources.Messages.SourceCodeList_UnableToAuthenticate;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                this.grdSourceCodeRevisionList.Visible = false;
            }
            catch (SourceCodeProviderLoadingException)
            {
                this.lblMessage.Text = Resources.Messages.SourceCodeList_UnableToLoadProvider;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                this.grdSourceCodeRevisionList.Visible = false;
            }
            catch (SourceCodeProviderGeneralException)
            {
                this.lblMessage.Text = Resources.Messages.SourceCodeList_GeneralError;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                this.grdSourceCodeRevisionList.Visible = false;
            }
            catch (SourceCodeCacheInvalidException)
            {
                this.lblMessage.Text = Resources.Messages.SourceCodeList_CacheNotReadyError;
                this.lblMessage.Type = MessageBox.MessageType.Error;
                this.grdSourceCodeRevisionList.Visible = false;
            }
        }
    }
}
