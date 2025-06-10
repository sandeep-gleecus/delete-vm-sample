using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Attributes;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.UserControls;

namespace Inflectra.SpiraTest.Web.Administration
{
    [HeaderSettings(GlobalNavigation.NavigationHighlightedLink.Administration, "Admin_TaraVault_ProjectList", "System-Administration", "Admin_TaraVault_ProjectList")]
    [AdministrationLevelAttribute(AdministrationLevelAttribute.AdministrationLevels.SystemAdministrator)]
    public partial class TaraVault_ProjectList : AdministrationBase
	{
		private const string CLASS_NAME = "Web.Administration.Admin_TaraVault_ProjectList::";

        /// <summary>Hit when the page is first loaded.</summary>
        /// <param name="sender">Page</param>
        /// <param name="e">eventArgs</param>
        protected void Page_Load(object sender, EventArgs e)
		{
            if (!this.IsPostBack)
				this.LoadAndBindData();
		}

        /// <summary>Loads the table of users.</summary>
        private void LoadAndBindData()
		{
            //Load the TV users
            VaultManager vaultManager = new VaultManager();
            List<DataModel.Project> allProjects = vaultManager.Project_RetrieveTaraDefined();
            this.grdProjectList.DataSource = allProjects;
            this.grdProjectList.DataBind();
        }
    }
}