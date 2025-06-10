using System;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Data;
using System.Linq;
using System.Collections.Generic;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;

using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.DataModel;
using System.Drawing;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage
{
    /// <summary>
    /// Displays the list of requirements that the user is the owner of
    /// </summary>
    public partial class RequirementsList : WebPartBase, IWebPartReloadable
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.RequirementsList::";

        #region User Configurable Properties

        /// <summary>
        /// Stores how many rows of data to display, default is 10
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("Global_NumberRowsToDisplay"),
        LocalizedWebDescription("Global_NumberRowsToDisplayTooltip"),
        DefaultValue(10)
        ]
        public int RowsToDisplay
        {
            get
            {
                return this.rowsToDisplay;
            }
            set
            {
				int rowsToDisplayMax = 50;
				this.rowsToDisplay = value < rowsToDisplayMax ? value : rowsToDisplayMax;
				//Force the data to reload
				LoadAndBindData();
            }
        }
        protected int rowsToDisplay = 10;

        /// <summary>
        /// Should we display requirements in 'completed' statuses (Default: FALSE)
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("RequirementsList_IncludeCompleted"),
        LocalizedWebDescription("RequirementsList_IncludeCompletedTooltip"),
        DefaultValue(false)
        ]
        public bool IncludeCompleted
        {
            get
            {
                return this.includeCompleted;
            }
            set
            {
                this.includeCompleted = value;
                //Force the data to reload
                LoadAndBindData();
            }
        }
        protected bool includeCompleted = false;

        /// <summary>
        /// Should we display requirements in the 'accepted' status (Default: TRUE)
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("RequirementsList_IncludeAccepted"),
        LocalizedWebDescription("RequirementsList_IncludeAcceptedTooltip"),
        DefaultValue(true)
        ]
        public bool IncludeAccepted
        {
            get
            {
                return this.includeAccepted;
            }
            set
            {
                this.includeAccepted = value;
                //Force the data to reload
                LoadAndBindData();
            }
        }
        protected bool includeAccepted = true;

        #endregion

        /// <summary>
        /// Loads the control data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";

            try
            {
                //Set the RSS Feed link if it's enabled
                if (!String.IsNullOrEmpty(UserRssToken))
                {
                    string rssUrl = this.ResolveUrl("~/Feeds/" + UserId + "/" + UserRssToken + "/AssignedRequirements.aspx");
                    this.Subtitle = GlobalFunctions.WEBPART_SUBTITLE_RSS + rssUrl;
                }

                //Register the event handlers
                this.grdRequirements.RowDataBound += new GridViewRowEventHandler(grdRequirements_RowDataBound);

                //Now load the content
                if (WebPartVisible)
                {
                    LoadAndBindData();
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                //Don't rethrow as this is loaded by an update panel and can't redirect to error page
                if (this.Message != null)
                {
                    this.Message.Text = Resources.Messages.Global_UnableToLoad + " '" + this.Title + "'";
                    this.Message.Type = MessageBox.MessageType.Error;
                }
            }
        }

        /// <summary>
        /// Returns a handle to the interface
        /// </summary>
        /// <returns>IWebPartReloadable</returns>
        [ConnectionProvider("ReloadableProvider", "ReloadableProvider")]
        public IWebPartReloadable GetReloadable()
        {
            return this;
        }

        /// <summary>
        /// Loads and binds the control data
        /// </summary>
        public void LoadAndBindData()
        {
            //Get the current project filter (if any)
            Nullable<int> filterProjectId = null;
            if (GetUserSetting(GlobalFunctions.USER_SETTINGS_MY_PAGE_SETTINGS, GlobalFunctions.USER_SETTINGS_KEY_FILTER_BY_PROJECT, false) && ProjectId > 0)
            {
                filterProjectId = ProjectId;
            }

            //See if we should display the project or release column
            if (!filterProjectId.HasValue)
            {
                //Display the project field
                this.grdRequirements.Columns[2].Visible = true;
                this.grdRequirements.Columns[3].Visible = false;
            }
            else
            {
                //Display the release field
                this.grdRequirements.Columns[2].Visible = false;
                this.grdRequirements.Columns[3].Visible = true;
            }

            //Set the base url for the Requirement URL field so that we get a HyperLink and not a LinkButton
            //The actual URL will be set during databinding
            ((NameDescriptionFieldEx)this.grdRequirements.Columns[1]).NavigateUrlFormat = "{0}";

            //Now get the list of requirements owned by the user
            Business.RequirementManager requirementManager = new Business.RequirementManager();
            List<RequirementView> requirements = requirementManager.RetrieveByOwnerId(UserId, filterProjectId, null, includeCompleted, this.rowsToDisplay, includeAccepted);

            this.grdRequirements.DataSource = requirements;
            this.grdRequirements.DataBind();
        }

        /// <summary>
        /// Applies selective formatting to the requirements grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void grdRequirements_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //Don't touch headers, footers or subheaders
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //Get the data item
                RequirementView requirement = (RequirementView)(e.Row.DataItem);
                //Lets handle the color of the importance column
                if (requirement.ImportanceId.HasValue)
                {
                    Color backColor = Color.FromName("#" + requirement.ImportanceColor);
                    e.Row.Cells[4].BackColor = backColor;
                }

                //Need to set the actual URL of the HyperLink
                HyperLinkEx hyperlink = (HyperLinkEx)e.Row.Cells[1].Controls[0];
                if (hyperlink != null)
                {
                    hyperlink.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, requirement.ProjectId, requirement.RequirementId) + "?" + GlobalFunctions.PARAMETER_REFERER_PROJECT_LIST + "=" + GlobalFunctions.PARAMETER_VALUE_TRUE;
                }
            }
        }
    }
}
