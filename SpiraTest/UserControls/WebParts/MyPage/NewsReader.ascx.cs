using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Web.Attributes;

namespace Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage
{
    public partial class NewsReader : WebPartBase
    {
        #region User Configurable Properties

        /// <summary>
        /// Stores how many rows of data to display, default is unlimited
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("Global_NumberRowsToDisplay"),
        LocalizedWebDescription("Global_NumberRowsToDisplayTooltip"),
        DefaultValue(5)
        ]
        public int RowsToDisplay
        {
            get
            {
                return this.rowsToDisplay;
            }
            set
            {
                this.rowsToDisplay = value;
            }
        }
        protected int rowsToDisplay = 5;

        /// <summary>
        /// Stores the length of headline to display before truncating
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("NewsReader_HeadlineLength"),
        LocalizedWebDescription("NewsReader_HeadlineLengthTooltip"),
        DefaultValue(50)
        ]
        public int HeadlineLength
        {
            get
            {
                return this.headlineLength;
            }
            set
            {
                this.headlineLength = value;
            }
        }
        protected int headlineLength = 128;

        /// <summary>
        /// Stores the URL for the newsfeed
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("NewsReader_FeedUrl"),
        LocalizedWebDescription("NewsReader_FeedUrlTooltip")
        ]
        public string FeedUrl
        {
            get
            {
                //By default we use the feed url specified in the themed control. That way we can have different default feeds
                //for different branded versions of SpiraTest.
                if (String.IsNullOrEmpty(this.feedUrl))
                {
                    return this.lblDefaultUrl.Text;
                }
                else
                {
                    return this.feedUrl;
                }
            }
            set
            {
                //Make sure the url is valid
                Uri uri;
                if (Uri.TryCreate(value, UriKind.Absolute, out uri))
                {
                    this.feedUrl = value;
                }
            }
        }
        protected string feedUrl = "";

        /// <summary>
        /// Stores the Title for the newsfeed
        /// </summary>
        [
        WebBrowsable,
        Personalizable,
        LocalizedWebDisplayName("NewsReader_FeedTitle"),
        LocalizedWebDescription("NewsReader_FeedTitleTooltip")
        ]
        public string FeedTitle
        {
            get
            {
                return this.Title;
            }
            set
            {
                this.Title = value;
            }
        }

        #endregion

        /// <summary>
        /// Loads the control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
        }
    }
}