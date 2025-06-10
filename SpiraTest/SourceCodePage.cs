using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.ServerControls;

namespace Inflectra.SpiraTest.Web
{
    /// <summary>
    /// All the source code pages have this as their base class
    /// </summary>
    public class SourceCodePage : PageLayout
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.SourceCodePage::";

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

            //If the current branch is not in the list of branches, it may have been deleted, so return to default branch
            if (!branches.Any(b => b.BranchKey == currentBranchKey))
            {
                currentBranchKey = null;
            }

            if (String.IsNullOrEmpty(currentBranchKey))
            {
                //If no current branch, use the default
                SourceCodeBranch currentBranch = branches.FirstOrDefault(b => b.IsDefault);
                if (currentBranch != null)
                {
                    currentBranchKey = currentBranch.BranchKey;
                    SourceCodeManager.Set_UserSelectedBranch(UserId, ProjectId, currentBranchKey);
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
                            string indentCss = ((i > 0) ? " ml-c" + i : "");
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
                    string indentCss = ((indent > 0) ? " ml-c" + indent : "");
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
