using System;

using Inflectra.SpiraTest.Web.UserControls;

namespace Inflectra.SpiraTest.Web.Attributes
{
    /// <summary>
    /// This stores the navigation link that should be applied to a page
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class HeaderSettingsAttribute : Attribute
    {
        private GlobalNavigation.NavigationHighlightedLink highlightedLink;
        private string pageTitle;
        private string helpJumpTag;
        private string breadcrumbText;

        #region Properties

        /// <summary>
        /// The navigation link
        /// </summary>
        public GlobalNavigation.NavigationHighlightedLink HighlightedLink
        {
            get
            {
                return this.highlightedLink;
            }
            set
            {
                this.highlightedLink = value;
            }
        }

        /// <summary>
        /// The help jump tag for the page
        /// </summary>
        public string HelpJumpTag
        {
            get
            {
                return this.helpJumpTag;
            }
            set
            {
                this.helpJumpTag = value;
            }
        }

        /// <summary>
        /// The page title
        /// </summary>
        public string PageTitle
        {
            get
            {
                return this.pageTitle;
            }
            set
            {
                this.pageTitle = value;
            }
        }

        /// <summary>
        /// Any additional text that we want to display in the breadcrumbing
        /// </summary>
        public string BreadcrumbText
        {
            get
            {
                return this.breadcrumbText;
            }
            set
            {
                this.breadcrumbText = value;
            }
        }

        #endregion

        /// <summary>
        /// The constructor is called when the attribute is set.
        /// </summary>
        /// <param name="pageTitle">The title of the page</param>
        /// <param name="highlightedLink">The link we should display for this class</param>
        public HeaderSettingsAttribute(GlobalNavigation.NavigationHighlightedLink highlightedLink, string pageTitle)
        {
            //Update the local member variables
            this.highlightedLink = highlightedLink;
            this.pageTitle = pageTitle;
            this.helpJumpTag = "";
            this.breadcrumbText = "";
        }

        /// <summary>
        /// The constructor is called when the attribute is set.
        /// </summary>
        /// <param name="pageTitle">The title of the page</param>
        /// <param name="highlightedLink">The link we should display for this class</param>
        /// <param name="helpJumpTag">The jump tag within the help navigation system</param>
        public HeaderSettingsAttribute(GlobalNavigation.NavigationHighlightedLink highlightedLink, string pageTitle, string helpJumpTag)
        {
            //Update the local member variables
            this.highlightedLink = highlightedLink;
            this.pageTitle = pageTitle;
            this.helpJumpTag = helpJumpTag;
            this.breadcrumbText = "";
        }

        /// <summary>
        /// The constructor is called when the attribute is set.
        /// </summary>
        /// <param name="pageTitle">The title of the page</param>
        /// <param name="highlightedLink">The link we should display for this class</param>
        /// <param name="helpJumpTag">The jump tag within the help navigation system</param>
        public HeaderSettingsAttribute(GlobalNavigation.NavigationHighlightedLink highlightedLink, string pageTitle, string helpJumpTag, string breadcrumbText)
        {
            //Update the local member variables
            this.highlightedLink = highlightedLink;
            this.pageTitle = pageTitle;
            this.helpJumpTag = helpJumpTag;
            this.breadcrumbText = breadcrumbText;
        }
    }
}
