using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Web.Attributes;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.PortfolioHome
{
    /// <summary>
    /// Displays the build list for the portfolio
    /// </summary>
    public partial class RecentBuilds : WebPartBase, IWebPartReloadable
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.UserControls.WebParts.PortfolioHome.RecentBuilds::";

        #region User Configurable Properties

        /// <summary>
        /// Stores how many rows of data to display, default is 15
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("Global_NumberRowsToDisplay"),
        LocalizedWebDescription("Global_NumberRowsToDisplayTooltip"),
        DefaultValue(15)
        ]
        public int RowsToDisplay
        {
            get
            {
                return this.rowsToDisplay;
            }
            set
            {
                int rowsToDisplayMax = 99;
                this.rowsToDisplay = value < rowsToDisplayMax ? value : rowsToDisplayMax;
                //Force the data to reload
                LoadAndBindData();
            }
        }
        protected int rowsToDisplay = 15;

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            //Now load the content
            if (WebPartVisible)
            {
                LoadAndBindData();
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
        /// Loads the control data
        /// </summary>
        public void LoadAndBindData()
        {
        }
        }
}