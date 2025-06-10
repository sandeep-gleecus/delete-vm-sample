using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;
using Inflectra.SpiraTest.Web.Attributes;

namespace Inflectra.SpiraTest.Web.Administration
{
    /// <summary>
    /// This webform code-behind class is responsible to displaying the
    /// Administration Automation Engine Details Page and handling all raised events
    /// </summary>
    [
    HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_AutomationEngineDetails_Title", "System-Integration/#test-automation", "Admin_AutomationEngineDetails_Title"),
    AdministrationLevel(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)
    ]
    public partial class AutomationEngineDetails : AdministrationBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Administration.AutomationEngineDetails::";

        protected int userId;
        protected int automationEngineId;

        /// <summary>
        /// Sets up the page when first loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //Reset the error message
            this.lblMessage.Text = "";

            //Get the automation engine id from the querystring (if we have one)
            if (string.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_AUTOMATION_ENGINE_ID]))
            {
                //Denotes insert mode
                this.automationEngineId = -1;
                this.btnUpdateAndClose.Visible = false;
                this.btnInsertAndClose.Visible = true;
            }
            else
            {
                this.automationEngineId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_AUTOMATION_ENGINE_ID]);
                this.btnUpdateAndClose.Visible = true;
                this.btnInsertAndClose.Visible = false;
            }

            //Register the button event handlers
            this.btnInsertAndClose.Click += new EventHandler(btnInsertAndClose_Click);
            this.btnUpdateAndClose.Click += new EventHandler(btnUpdateAndClose_Click);
            this.btnCancel.Click += new EventHandler(btnCancel_Click);

            //Only load the data once
            if (!IsPostBack)
            {
                LoadAndBindData();
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Updates the automation engine provider
        /// </summary>
        /// <remarks>Returns TRUE if update successful</remarks>
        protected bool UpdateDetails()
        {
            const string METHOD_NAME = "UpdateDetails"; 
            
            try
            {
                //Retrieve the existing record and make the updates
                AutomationManager automationManager = new AutomationManager();
                try
                {
                    AutomationEngine automationEngine = automationManager.RetrieveEngineById(this.automationEngineId);
                    automationEngine.StartTracking();
                    automationEngine.Name = this.txtName.Text.Trim();
                    if (String.IsNullOrEmpty(this.txtDescription.Text.Trim()))
                    {
                        automationEngine.Description = "";
                    }
                    else
                    {
                        automationEngine.Description = GlobalFunctions.HtmlScrubInput(this.txtDescription.Text.Trim());
                    }
                    automationEngine.Token = this.txtToken.Text.Trim();
                    automationEngine.IsActive = this.chkActive.Checked;

                    //Custom configuration fields - not currently supported
                    /*
                    if (this.txtCustom01.Text.Trim() == "")
                    {
                        automationEngine.SetCustom01Null();
                    }
                    else
                    {
                        automationEngine.Custom01 = this.txtCustom01.Text.Trim();
                    }
                    if (this.txtCustom02.Text.Trim() == "")
                    {
                        automationEngine.SetCustom02Null();
                    }
                    else
                    {
                        automationEngine.Custom02 = this.txtCustom02.Text.Trim();
                    }
                    if (this.txtCustom03.Text.Trim() == "")
                    {
                        automationEngine.SetCustom03Null();
                    }
                    else
                    {
                        automationEngine.Custom03 = this.txtCustom03.Text.Trim();
                    }
                    if (this.txtCustom04.Text.Trim() == "")
                    {
                        automationEngine.SetCustom04Null();
                    }
                    else
                    {
                        automationEngine.Custom04 = this.txtCustom04.Text.Trim();
                    }
                    if (this.txtCustom05.Text.Trim() == "")
                    {
                        automationEngine.SetCustom05Null();
                    }
                    else
                    {
                        automationEngine.Custom05 = this.txtCustom05.Text.Trim();
                    }*/
                    automationManager.UpdateEngine(automationEngine, this.userId);
                }
                catch (EntityConstraintViolationException)
                {
                    this.lblMessage.Text = Resources.Messages.Admin_AutomationEngineDetails_TokenNotUnique;
                    this.lblMessage.Type = MessageBox.MessageType.Error;
                    return false;
                }
                catch (ArtifactNotExistsException)
                {
                    //Just redirect back to the automation engine home
                    Response.Redirect("AutomationEngines.aspx");
                    return false;
                }
                return true;
            }
            catch (System.Threading.ThreadAbortException)
            {
                //Ignore since it's caused by the redirect
                return true;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Updates the changes to the automation engine and goes back to the automation engine list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnUpdateAndClose_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnUpdateAndClose_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //First make sure the page validated OK
            if (!Page.IsValid)
            {
                return;
            }

            //Actually perform the update
            bool success = UpdateDetails();

            //Just redirect back to the automation engine home
            if (success)
            {
                Response.Redirect("AutomationEngines.aspx");
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Redirects the user back to the parent page when cancel clicked
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnCancel_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            Response.Redirect("AutomationEngines.aspx", true);

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }


        /// <summary>
        /// Inserts a new automation engine
        /// </summary>
        /// <returns>The id of the new engine</returns>
        protected int? InsertEngine()
        {
            const string METHOD_NAME = "InsertEngine";

            try
            {
                //Now add the new automation engine handling NULLs correctly
                AutomationManager automationManager = new AutomationManager();
                string description = null;
                if (this.txtDescription.Text.Trim() != "")
                {
                    description = GlobalFunctions.HtmlScrubInput(this.txtDescription.Text.Trim());
                }

                //Custom configuration fields - notcurrently supported
                /*
                string custom01 = SourceCode.NullParameterString;
                string custom02 = SourceCode.NullParameterString;
                string custom03 = SourceCode.NullParameterString;
                string custom04 = SourceCode.NullParameterString;
                string custom05 = SourceCode.NullParameterString;
                if (this.txtCustom01.Text.Trim() != "")
                {
                    custom01 = this.txtCustom01.Text.Trim();
                }
                if (this.txtCustom02.Text.Trim() != "")
                {
                    custom02 = this.txtCustom02.Text.Trim();
                }
                if (this.txtCustom03.Text.Trim() != "")
                {
                    custom03 = this.txtCustom03.Text.Trim();
                }
                if (this.txtCustom04.Text.Trim() != "")
                {
                    custom04 = this.txtCustom04.Text.Trim();
                }
                if (this.txtCustom05.Text.Trim() != "")
                {
                    custom05 = this.txtCustom05.Text.Trim();
                }*/

                return automationManager.InsertEngine(
                    this.txtName.Text.Trim(),
                    this.txtToken.Text.Trim(),
                    description,
                    this.chkActive.Checked,
                    this.userId
                    );
            }
            catch (EntityConstraintViolationException)
            {
                this.lblMessage.Text = "You need to enter a unique Token for this automation engine. The one specified is already in use.";
                return null;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Inserts the new version automation engine into the system and returns to the automation engine list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnInsertAndClose_Click(object sender, EventArgs e)
        {
            const string METHOD_NAME = "btnInsertAndClose_Click";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //First make sure the page validated OK
            if (!Page.IsValid)
            {
                return;
            }

            //Actually do the insert
            int? engineId = InsertEngine();

            //Just redirect back to the automation engine list
            if (engineId.HasValue)
            {
                Response.Redirect("AutomationEngines.aspx");
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
            Logger.Flush();
        }

        /// <summary>
        /// Loads the automation engine information
        /// </summary>
        protected void LoadAndBindData()
        {
            const string METHOD_NAME = "LoadAndBindData";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                // Databind the form so that the validators work
                this.DataBind();

                //See if we're inserting or updating
                if (this.automationEngineId == -1)
                {
                    this.lblEngineName.Text = Resources.Main.Admin_AutomationEngineDetails_NewProvider;
                    this.chkActive.Checked = true;
                }
                else
                {
                    //First we need to retrieve the automation engine record
                    AutomationManager automationManager = new AutomationManager();
                    AutomationEngine automationEngine = null;
                    try
                    {
                        automationEngine = automationManager.RetrieveEngineById(this.automationEngineId);
                    }
                    catch (ArtifactNotExistsException)
                    {
                        this.lblMessage.Text = Resources.Messages.Admin_AutomationEngineDetails_UpdatedAdmin_AutomationEngineDetails_EngineNotExists;
                        this.lblMessage.Type = MessageBox.MessageType.Error;
                        return;
                    }

                    //Populate the form, handling NULLs correctly
                    this.lblEngineName.Text = automationEngine.Name;
                    this.txtName.Text = automationEngine.Name;
                    if (String.IsNullOrEmpty(automationEngine.Description))
                    {
                        this.txtDescription.Text = "";
                    }
                    else
                    {
                        this.txtDescription.Text = automationEngine.Description;
                    }
                    this.txtToken.Text = automationEngine.Token;
                    this.chkActive.Checked = automationEngine.IsActive;

                    //Custom Configuration Parameters - not currently supported
                    /*
                    if (versionControlRow.IsCustom01Null())
                    {
                        this.txtCustom01.Text = "";
                    }
                    else
                    {
                        this.txtCustom01.Text = versionControlRow.Custom01;
                    }
                    if (versionControlRow.IsCustom02Null())
                    {
                        this.txtCustom02.Text = "";
                    }
                    else
                    {
                        this.txtCustom02.Text = versionControlRow.Custom02;
                    }
                    if (versionControlRow.IsCustom03Null())
                    {
                        this.txtCustom03.Text = "";
                    }
                    else
                    {
                        this.txtCustom03.Text = versionControlRow.Custom03;
                    }
                    if (versionControlRow.IsCustom04Null())
                    {
                        this.txtCustom04.Text = "";
                    }
                    else
                    {
                        this.txtCustom04.Text = versionControlRow.Custom04;
                    }
                    if (versionControlRow.IsCustom05Null())
                    {
                        this.txtCustom05.Text = "";
                    }
                    else
                    {
                        this.txtCustom05.Text = versionControlRow.Custom05;
                    }*/
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
    }
}
