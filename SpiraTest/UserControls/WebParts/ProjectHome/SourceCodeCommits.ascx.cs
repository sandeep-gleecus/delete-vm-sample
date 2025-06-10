using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.ServerControls;
using System.Globalization;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome
{
    public partial class SourceCodeCommits : WebPartBase, IWebPartReloadable
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.SourceCodeCommits::";

        /// <summary>
        /// The date format to use
        /// </summary>
        public bool DateFormatMonthFirst { get; set; }

        protected string CurrentBranchKey
        {
            get;
            set;
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
        /// Loads the control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            //Now load the content
            if (WebPartVisible)
            {
                LoadAndBindData();
            }
        }

        /// <summary>
        /// Loads the data in the control
        /// </summary>
        public void LoadAndBindData()
        {
            const string METHOD_NAME = "LoadAndBindData";

			try
			{
                //Finally we need to specify if the current date-format is M/D or D/M (no years displayed to save space)
                string dateFormat = CultureInfo.CurrentCulture.DateTimeFormat.MonthDayPattern.ToLowerInvariant();
                this.DateFormatMonthFirst = dateFormat.StartsWith("m");
                
                //Set the administration url
                this.lnkAdministration.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Administration, ProjectId);

                //Set the revision list URL
                this.lnkViewRevisions.NavigateUrl = UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.SourceCodeRevision, this.ProjectId);

                //Make sure we have source code enabled
                try
                {
                    //Get the list of branches for this project and add the event handler
                    SourceCodeManager sourceCodeManager = new SourceCodeManager(this.ProjectId);
                    CurrentBranchKey = LoadBranchList(sourceCodeManager, this.mnuBranches);
                }
                catch (SourceCodeProviderLoadingException)
                {
                    this.plcBranchInfo.Visible = false;
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
        /// Loads the list of branches
        /// </summary>
        /// <param name="mnuBranches"></param>
        /// <remarks>
        /// Splits  the branches into a hierarchy based on the 'slash' symbol like GitTower and GitKraken, etc.
        /// </remarks>
        protected string LoadBranchList(SourceCodeManager sourceCodeManager, DropMenu mnuBranches)
        {
            string currentBranchKey = SourceCodeManager.Get_UserSelectedBranch(UserId, ProjectId);
            List<SourceCodeBranch> branches = sourceCodeManager.RetrieveBranches();
            if (String.IsNullOrEmpty(currentBranchKey))
            {
                //If no current branch, use the default
                SourceCodeBranch currentBranch = branches.FirstOrDefault(b => b.IsDefault);
                if (currentBranch != null)
                {
                    currentBranchKey = currentBranch.BranchKey;
                }
            }
            Dictionary<string, string> folders = new Dictionary<string, string>();
            mnuBranches.DropMenuItems.Clear();
            foreach (SourceCodeBranch branch in branches)
            {
                //Get the branch name / path
                string branchPath = branch.BranchKey;
                string branchName = branchPath;

                //If it has slashes, it is a hierarchy
                int indent = 0;
                if (branchPath.Contains('/'))
                {
                    string[] branchSegments = branchPath.Split('/');
                    branchName = branchSegments.Last();
                    string folderPath = "";
                    indent = branchSegments.Length - 1;
                    for (int i = 0; i < indent; i++)
                    {
                        string folderName = branchSegments[i];
                        folderPath += folderName + "/";
                        if (!folders.ContainsKey(folderPath))
                        {
                            folders.Add(folderPath, folderName);

                            //Add to the menu
                            DropMenuItem dropMenuItem = new DropMenuItem();
                            string indentCss = ((indent > 1) ? " ml" + (indent - 1) * 3 : "");
                            dropMenuItem.GlyphIconCssClass = "far fa-folder-open mr3" + indentCss;
                            dropMenuItem.Name = folderPath;
                            dropMenuItem.Value = folderName;
                            dropMenuItem.ClientScriptMethod = "void(0)";
                            mnuBranches.DropMenuItems.Add(dropMenuItem);
                        }
                    }
                }

                {
                    //Add branch to the menu
                    DropMenuItem dropMenuItem = new DropMenuItem();
                    string indentCss = ((indent > 0) ? " ml" + (indent * 3) : "");
					dropMenuItem.GlyphIconCssClass = "fas fa-code-branch mr3" + indentCss;
					dropMenuItem.Name = branchPath;
                    dropMenuItem.Value = branchName;
                    dropMenuItem.ClientScriptMethod = "mnuBranches_click('" + branchPath + "')";
					mnuBranches.DropMenuItems.Add(dropMenuItem);
                }
            }
            mnuBranches.Text = currentBranchKey;

            return currentBranchKey;
        }
    }
}
