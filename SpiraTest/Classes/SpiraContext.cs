using Inflectra.SpiraTest.DataModel;
using System;
using System.Web;

namespace Inflectra.SpiraTest.Web.Classes
{
    /// <summary>
    /// Stores any application-wide temporary settings that are stored in the HTTP Context
    /// </summary>
    /// <remarks>The data only persists for the lifetime of an HTTP request (unlike Session)</remarks>
    internal class SpiraContext
    {
        #region Constructors

        public SpiraContext(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            //Default to not being an administrator
            this.IsProjectAdmin = false;
            this.IsGroupAdmin = false;
            this.IsGroupMember = false;
            this.IsArtifactCreatorOrOwner = false;
        }

        #endregion
        
        #region Singleton

        //this is a pseudo singleton, but since it's per request, there should never be a problem
        
        public static SpiraContext Current
        {
            get
            {
                HttpContext context = HttpContext.Current;
                if (context == null)
                {
                    throw new InvalidOperationException("SpiraContext can only be used from within a http context");
                }
                SpiraContext spiraContext = (SpiraContext)HttpContext.Current.Items["SpiraContext"];
                if (spiraContext == null)
                {
                    spiraContext = new SpiraContext(context);
                    HttpContext.Current.Items["SpiraContext"] = spiraContext;
                }
                return spiraContext;
            }    
        }
        
        #endregion
        
        /// <summary>
        /// Stores the current timezone id
        /// </summary>
        public string TimezoneId
        {
            get;
            set;
        }

        /// <summary>
        /// Used to store the culture name in case the current thread gets reset during the page/service lifecycle
        /// </summary>
        /// <remarks>
        /// Only set within the AJAX web services currently, so do not use outside of that context
        /// </remarks>
        public string CultureName
        {
            get;
            set;
        }

        /// <summary>
        /// The current project Id (null if not set)
        /// </summary>
        public int? ProjectId
        {
            get;
            set;
        }

        /// <summary>
        /// The current project templateId (null if not set)
        /// </summary>
        public int? ProjectTemplateId
        {
            get;
            set;
        }


        /// <summary>
        /// The current project template name
        /// </summary>
        public string ProjectTemplateName
        {
            get;
            set;
        }


        /// <summary>
        /// The current project name
        /// </summary>
        public string ProjectName
        {
            get;
            set;
        }

        /// <summary>
        /// The id of the current project's group
        /// </summary>
        public int? ProjectGroupId
        {
            get;
            set;
        }

        /// <summary>
        /// The id of the current portfolio
        /// </summary>
        public int? PortfolioId
        {
            get;
            set;
        }

        /// <summary>
        /// The name of the portfolio
        /// </summary>
        public string PortfolioName
        {
            get;
            set;
        }

        /// <summary>
        /// The name of the current project's group
        /// </summary>
        public string ProjectGroupName
        {
            get;
            set;
        }

        /// <summary>
        /// The id of the current project role
        /// </summary>
        public int? ProjectRoleId
        {
            get;
            set;
        }

        /// <summary>
        /// The name of the current project role
        /// </summary>
        public string ProjectRoleName
        {
            get;
            set;
        }

        /// <summary>
        /// Is the current user a project administrator
        /// </summary>
        public bool IsProjectAdmin
        {
            get;
            set;
        }

        /// <summary>
        /// Is the current user a template administrator of the current project
        /// </summary>
        public bool IsTemplateAdmin
        {
            get;
            set;
        }

        /// <summary>
        /// The current type of workspace
        /// </summary>
        public int? WorkspaceType
        {
            get;
            set;
        }

        /// <summary>
        /// Is the current user the owner of *ANY* project groups
        /// </summary>
        public bool IsGroupAdmin
        {
            get;
            set;
        }

        /// <summary>
        /// Is the current user a member of THE CURRENT project group
        /// </summary>
        public bool IsGroupMember
        {
            get;
            set;
        }

        /// <summary>
        /// Does the current user own the current artifact (or did the current user create it). Needed for permissions-management
        /// </summary>
        public bool IsArtifactCreatorOrOwner
        {
            get;
            set;
        }

		/// <summary>Does the current user have permission to add comments?</summary>
		public bool CanUserAddCommentToArtifacts
		{ get; set; }
    }
}