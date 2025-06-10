using System.Collections.Generic;
using static Inflectra.SpiraTest.Web.UserControls.GlobalNavigation;

namespace Inflectra.SpiraTest.Web.App_Models
{
    /// <summary>Used in the Admin pages for editing/viewing custom lists.</summary>
    public class JSON_AdminNavigation
    {
        #region Properties
        public JSON_AdminSection System { get; set; }
        public JSON_AdminSection Project { get; set; }
        public JSON_AdminSection Program { get; set; }
        public JSON_AdminSection Template { get; set; }
        #endregion
    }

    public class JSON_AdminSection
    {
        /// <summary>The ID suffix to use in the DOM.</summary>
        public string Id { get; set; }
        /// <summary>The int id of the specific workspace - eg projectId = 1</summary>
        public int WorkspaceId { get; set; }
        /// <summary>The title for the section of the admin menu (eg system, template).</summary>
        public string Name { get; set; }
        /// <summary>The url suffix to use with the section name/title.</summary>
        public string Url { get; set; }
        /// <summary>The list of links to display outside of any subsection.</summary>
        public List<JSON_AdminLink> Links { get; set; }
        /// <summary>The list of sub sections within the section.</summary>
        public List<JSON_AdminSubSection> SubSections { get; set; }
    }

    public class JSON_AdminSubSection
    {
        /// <summary>The ordering value - should be unique.</summary>
        public int Index { get; set; }
        /// <summary>The ID suffix to use in the DOM.</summary>
        public string Id { get; set; }
        /// <summary>The navigation link enum (relevant only really for templates) used for lookups.</summary>
        public int NavigationId { get; set; }
        /// <summary>The title for the sub section.</summary>
        public string Name { get; set; }
        /// <summary>The list of links to display in this subsection.</summary>
        public List<JSON_AdminLink> Links { get; set; }
		/// <summary>An int that represents the enum of the permission required to view this item</summary>
		public int[] AdminPermissions { get; set; }
	}

    public class JSON_AdminLink
    {
        /// <summary>The ordering value - should be unique.</summary>
        public int Index { get; set; }
        /// <summary>The ID suffix to use in the DOM.</summary>
        public string Id { get; set; }
        /// <summary>The name to describe the link for the sub section.</summary>
        public string Name { get; set; }
        /// <summary>Optional text to use for a tooltip.</summary>
        public string Title { get; set; }
        /// <summary>Normally this is a url suffix for within administration.</summary>
        public string Url { get; set; }
        /// <summary>True if the URL is on the root of the domain .</summary>
        public bool UrlIsFromRoot { get; set; }
        /// <summary>True if the URL needs no processing - ie is complete / external.</summary>
        public bool UrlIsComplete { get; set; }
	}
}
