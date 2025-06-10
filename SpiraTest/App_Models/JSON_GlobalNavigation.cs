using System.Collections.Generic;
using static Inflectra.SpiraTest.Web.UserControls.GlobalNavigation;

namespace Inflectra.SpiraTest.Web.App_Models
{
    /// <summary>Used in the Admin pages for editing/viewing custom lists.</summary>
    public class JSON_GlobalNavigation
    {
        #region Properties
        public List<JSON_Portfolio> Portfolios { get; set; }
        public List<JSON_Program> Programs { get; set; }
        public List<int> ProgramIdsByOwner { get; set; }
        public List<JSON_Project> Projects { get; set; }
        public List<int> ProjectIdsByOwner { get; set; }
        public List<JSON_Template> Templates { get; set; }
        public JSON_SiteMapNode NodeTree { get; set; }
        public NavigationHighlightedLink CurrentLocation { get; set; }
        public string HelpUrl { get; set; }
        public string HelpSection { get; set; }
        public string AdminUrl { get; set; }
        public string MyPageUrl { get; set; }
        public string MyProfileUrl { get; set; }
        public string MyTimecardUrl { get; set; }
        public JSON_UserNav User { get; set; }
        public JSON_AdminNavigation AdminNavigation { get; set; }
        #endregion
    }


    public class JSON_Portfolio
    {
        /// <summary>The portfolio's unique ID.</summary>
        public int Id { get; set; }

        /// <summary>The name of the portfolio.</summary>
        public string Name { get; set; }

        /// <summary>The description of the project.</summary>
        public string Description { get; set; }

        /// <summary>The url of the portfolio.</summary>
        public string WorkspaceUrl { get; set; }

        /// <summary>The url to the artifact accessed under the portfolio.</summary>
        public string ArtifactUrl { get; set; }

        /// <summary>whether the portfolio is enabled for the current user</summary>
        public bool IsEnabled { get; set; }

        /// <summary>the list of programs in the portfolio</summary>
        public int[] ProgramIds { get; set; }
    }


    public class JSON_Program
    {
        /// <summary>The program's unique ID.</summary>
        public int Id { get; set; }

        /// <summary>The name of the program.</summary>
        public string Name { get; set; }

        /// <summary>The description of the project.</summary>
        public string Description { get; set; }

        /// <summary>The url of the program.</summary>
        public string WorkspaceUrl { get; set;  }

        /// <summary>The url to the artifact accessed under the program.</summary>
        public string ArtifactUrl { get; set; }

        /// <summary>whether the program is enabled for the current user</summary>
        public bool IsEnabled { get; set; }

        /// <summary>the list of projects in the program</summary>
        public int?[] ProjectIds { get; set; }

        /// <summary>
        /// The id of the portfolio if there is one (null = unassigned)
        /// </summary>
        public int? PortfolioId { get; set; }
    }


    public class JSON_Project
    {
        /// <summary>The project's unique ID.</summary>
        public int Id { get; set; }

        /// <summary>The name of the project.</summary>
        public string Name { get; set; }

        /// <summary>The description of the project.</summary>
        public string Description { get; set; }

        /// <summary>The url of the program.</summary>
        public string WorkspaceUrl { get; set; }

        /// <summary>The url to the artifact accessed under the project</summary>
        public string ArtifactUrl { get; set; }

        /// <summary>whether the project is enabled for the current user</summary>
        public bool IsEnabled { get; set; }

        /// <summary>the program that the project is a part of</summary>
        public int? ProgramId { get; set; }
    }


    public class JSON_Template
    {
        /// <summary>The template's unique ID.</summary>
        public int Id { get; set; }

        /// <summary>The name of the template.</summary>
        public string Name { get; set; }
        /// <summary>The description of the template.</summary>
        public string Description { get; set; }
        /// <summary>The projects that use the template.</summary>
        public List<JSON_Project> Projects { get; set; }
    }

    public class JSON_SiteMapNode
    {
        /// <summary>The node's unique ID.</summary>
        public string Id { get; set; }

        /// <summary>The name of the node (localized).</summary>
        public string Name { get; set; }

        /// <summary>The description of the node (localized).</summary>
        public string Description { get; set; }

        /// <summary>The url to go to the node.</summary>
        public string Url { get; set; }
        /// <summary>Any node children.</summary>
        public List<JSON_SiteMapNode> Children { get; set; }
    }

    public class JSON_UserNav
    {
        /// <summary>URL of the profile page of the user</summary>
        public string ProfileUrl { get; set; }
        /// <summary>user's full name</summary>
        public string FullName { get; set; }
        /// <summary>user's first name</summary>
        public string FirstName { get; set; }
        /// <summary>user's last name</summary>
        public string LastName { get; set; }
        /// <summary>URL to the user's avatar </summary>
        public string AvatarUrl { get; set; }
        /// <summary>whether the user has an avatar icon actually set (ie not the default)</summary>
        public bool HasIcon { get; set; }
    }
}