using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Web.ServerControls.Authorization;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    [
    ToolboxData("<{0}:PlanningBoard runat=server></{0}:PlanningBoard>")
    ]
    public class PlanningBoard : WebControl, IScriptControl, IAuthorizedControl
    {
        protected AuthorizedControlBase authorizedControlBase;
        protected Dictionary<string, string> handlers;

        #region Enumerations

        /// <summary>
        /// The various group by options for the planning board
        /// </summary>
        public enum PlanningGroupByOptions
        {
            ByComponent = 1,
            ByPackage = 2,
            ByPriority = 3,
            ByRelease = 4,
            ByIteration = 5,
            ByStatus = 6,
            ByPerson = 7,
            ByRequirement = 8
        }

        #endregion

        #region Child Classes

        /// <summary>
        /// A single caption displayed in the group-by dropdown list
        /// </summary>
        public class GroupByOption
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public GroupByOption(int id, string caption, bool isActive = true)
            {
                this.Id = id;
                this.Caption = caption;
                this.IsActive = isActive;
            }

            public int Id { get; set; }
            public string Caption { get; set; }
            public bool IsActive { get; set; }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public PlanningBoard()
        {
            //Instantiate the authorized control default implementation
            authorizedControlBase = new AuthorizedControlBase(this.ViewState);
        }

        /// <summary>
        /// Returns a list of group by options for the main Planning Board and Requirements Board
        /// </summary>
        /// <param name="allReleasesSelected">Do we have 'all releases' selected</param>
        /// <param name="noReleaseSelected">Do we have 'no release' selected</param>
        /// <param name="iterationSelected">Do we have an iteration selected</param>
        /// <returns>The list of ids and display names</returns>
        public List<GroupByOption> GetGroupByOptions(bool noReleaseSelected, bool allReleasesSelected, bool iterationSelected)
        {
            //Create the master list
            List<GroupByOption> groupByOptions = new List<GroupByOption>();

            //Populate
            groupByOptions.Add(new GroupByOption((int)PlanningGroupByOptions.ByComponent, Resources.ServerControls.PlanningBoard_ByComponent, true));
            groupByOptions.Add(new GroupByOption((int)PlanningGroupByOptions.ByPackage, Resources.ServerControls.PlanningBoard_ByPackage, true));
            groupByOptions.Add(new GroupByOption((int)PlanningGroupByOptions.ByPriority, Resources.ServerControls.PlanningBoard_ByPriority, true));
            groupByOptions.Add(new GroupByOption((int)PlanningGroupByOptions.ByRelease, Resources.ServerControls.PlanningBoard_ByRelease, allReleasesSelected));
            groupByOptions.Add(new GroupByOption((int)PlanningGroupByOptions.ByIteration, Resources.ServerControls.PlanningBoard_ByIteration, (!noReleaseSelected && !allReleasesSelected && !iterationSelected)));
            groupByOptions.Add(new GroupByOption((int)PlanningGroupByOptions.ByStatus, Resources.ServerControls.PlanningBoard_ByStatus, true));
            groupByOptions.Add(new GroupByOption((int)PlanningGroupByOptions.ByPerson, Resources.ServerControls.PlanningBoard_ByPerson, (!noReleaseSelected)));
            return groupByOptions;
        }

        /// <summary>
        /// Returns a list of group by options for the Task Board
        /// </summary>
        /// <param name="allReleasesSelected">Do we have 'all releases' selected</param>
        /// <param name="noReleaseSelected">Do we have 'no release' selected</param>
        /// <param name="iterationSelected">Do we have an iteration selected</param>
        /// <param name="releaseOrIterationSelected">Do we have a release or iteration selected</param>
        /// <returns>The list of ids and display names</returns>
        public List<GroupByOption> GetTaskBoardGroupByOptions(bool noReleaseSelected, bool allReleasesSelected, bool releaseOrIterationSelected, bool iterationSelected)
        {
            //Create the master list
            List<GroupByOption> groupByOptions = new List<GroupByOption>();

            //Populate
            groupByOptions.Add(new GroupByOption((int)PlanningGroupByOptions.ByPriority, Resources.ServerControls.PlanningBoard_ByPriority, true));
            groupByOptions.Add(new GroupByOption((int)PlanningGroupByOptions.ByRelease, Resources.ServerControls.PlanningBoard_ByRelease, allReleasesSelected));
            groupByOptions.Add(new GroupByOption((int)PlanningGroupByOptions.ByIteration, Resources.ServerControls.PlanningBoard_ByIteration, (!noReleaseSelected && !allReleasesSelected && !iterationSelected)));
            groupByOptions.Add(new GroupByOption((int)PlanningGroupByOptions.ByStatus, Resources.ServerControls.PlanningBoard_ByStatus, true));
            groupByOptions.Add(new GroupByOption((int)PlanningGroupByOptions.ByPerson, Resources.ServerControls.PlanningBoard_ByPerson, (!noReleaseSelected)));
            groupByOptions.Add(new GroupByOption((int)PlanningGroupByOptions.ByRequirement, Resources.ServerControls.PlanningBoard_ByRequirement, releaseOrIterationSelected));
            return groupByOptions;
        }

        /// <summary>
        /// Returns a list of group by options for the Incident Board
        /// </summary>
        /// <param name="allReleasesSelected">Do we have 'all releases' selected</param>
        /// <param name="noReleaseSelected">Do we have 'no release' selected</param>
        /// <param name="iterationSelected">Do we have an iteration selected</param>
        /// <returns>The list of ids and display names</returns>
        public List<GroupByOption> GetIncidentBoardGroupByOptions(bool noReleaseSelected, bool allReleasesSelected, bool iterationSelected)
        {
            //Create the master list
            List<GroupByOption> groupByOptions = new List<GroupByOption>();

            //Populate
            groupByOptions.Add(new GroupByOption((int)PlanningGroupByOptions.ByPriority, Resources.ServerControls.PlanningBoard_ByPriority, true));
            groupByOptions.Add(new GroupByOption((int)PlanningGroupByOptions.ByRelease, Resources.ServerControls.PlanningBoard_ByRelease, allReleasesSelected));
            groupByOptions.Add(new GroupByOption((int)PlanningGroupByOptions.ByIteration, Resources.ServerControls.PlanningBoard_ByIteration, (!noReleaseSelected && !allReleasesSelected && !iterationSelected)));
            groupByOptions.Add(new GroupByOption((int)PlanningGroupByOptions.ByStatus, Resources.ServerControls.PlanningBoard_ByStatus, true));
            groupByOptions.Add(new GroupByOption((int)PlanningGroupByOptions.ByPerson, Resources.ServerControls.PlanningBoard_ByPerson, (!noReleaseSelected)));
            return groupByOptions;
        }

        /// <summary>
        /// Returns a list of group by options for the project group planning board
        /// </summary>
        /// <param name="allProjectsSelected">Do we have 'all projects' selected</param>
        /// <param name="noProjectSelected">Do we have 'no project' selected</param>
        /// <returns>The list of ids and display names</returns>
        public List<GroupByOption> GetProjectGroupGroupByOptions(bool noProjectSelected, bool allProjectsSelected)
        {
            //Create the master list
            List<GroupByOption> groupByOptions = new List<GroupByOption>();

            //Populate
            groupByOptions.Add(new GroupByOption((int)PlanningGroupByOptions.ByPriority, Resources.ServerControls.PlanningBoard_ByPriority, (true)));
            groupByOptions.Add(new GroupByOption((int)PlanningGroupByOptions.ByRelease, Resources.ServerControls.PlanningBoard_ByProject, allProjectsSelected));
            groupByOptions.Add(new GroupByOption((int)PlanningGroupByOptions.ByStatus, Resources.ServerControls.PlanningBoard_ByStatus, (true)));
            groupByOptions.Add(new GroupByOption((int)PlanningGroupByOptions.ByPerson, Resources.ServerControls.PlanningBoard_ByPerson, (!noProjectSelected)));
            return groupByOptions;
        }

        /// <summary>
        /// Allows the passing in of a collection of client-side event handlers
        /// </summary>
        /// <param name="handlers">The collection of handlers</param>
        public void SetClientEventHandlers(Dictionary<string, string> handlers)
        {
            this.handlers = handlers;
        }

        #region Properties

        /// <summary>
        /// Contains the id of the current project
        /// </summary>
        [
        Category("Context"),
        DefaultValue(-1)
        ]
        public int ProjectId
        {
            get
            {
                object obj = ViewState["projectId"];

                return (obj == null) ? -1 : (int)obj;
            }
            set
            {
                ViewState["projectId"] = value;
            }
        }

        /// <summary>
        /// Contains the id of the current release (-1 for no release, -2 for all releases)
        /// </summary>
        [
        Category("Context"),
        DefaultValue(-1)
        ]
        public int ReleaseId
        {
            get
            {
                object obj = ViewState["ReleaseId"];

                return (obj == null) ? -1 : (int)obj;
            }
            set
            {
                ViewState["ReleaseId"] = value;
            }
        }

        [
        Category("Data"),
        DefaultValue(""),
        Description("Contains the fully qualified namespace and class of the web service that will be providing the data to this Ajax server control")
        ]
        public string WebServiceClass
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["webServiceClass"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["webServiceClass"] = value;
            }
        }

        /// <summary>
        /// Should we include Tasks in the planning board story cards
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(false)
        ]
        public bool IncludeTasks
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["IncludeTasks"];

                return (obj == null) ? false : (bool)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["IncludeTasks"] = value;
            }
        }

        /// <summary>
        /// Should we include Incidents in the planning board story cards
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(true)
        ]
        public bool IncludeIncidents
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["IncludeIncidents"];

                return (obj == null) ? true : (bool)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["IncludeIncidents"] = value;
            }
        }

        /// <summary>
        /// Does this planning board instance support being ranked
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(false)
        ]
        public bool SupportsRanking
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["SupportsRanking"];

                return (obj == null) ? false : (bool)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["SupportsRanking"] = value;
            }

        }

        /// <summary>
        /// Is the current release id actually an iteration
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(false)
        ]
        public bool IsIteration
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["IsIteration"];

                return (obj == null) ? false : (bool)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["IsIteration"] = value;
            }
        }

        /// <summary>
        /// Should we include Test Cases in the planning board story cards
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(false)
        ]
        public bool IncludeTestCases
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["IncludeTestCases"];

                return (obj == null) ? false : (bool)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["IncludeTestCases"] = value;
            }
        }

        /// <summary>
        /// Should we include the Requirement Details in the planning board story cards
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(true)
        ]
        public bool IncludeDetails
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["IncludeDetails"];

                return (obj == null) ? true : (bool)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["IncludeDetails"] = value;
            }
        }

        /// <summary>
        /// Should we allow the user to create new story card items (assuming they have permissions)
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(true)
        ]
        public bool AllowCreate
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["AllowCreate"];

                return (obj == null) ? true : (bool)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["AllowCreate"] = value;
            }
        }

        /// <summary>
        /// This is the grouping option for the planning board
        /// </summary>
        [
        Bindable(true),
        Category("Behavior"),
        Description("This is the grouping option for the planning board"),
        DefaultValue(PlanningGroupByOptions.ByPackage),
        PersistenceMode(PersistenceMode.Attribute),
        ]
        public PlanningGroupByOptions GroupBy
        {
            get
            {
                object obj = ViewState["GroupBy"];

                return (obj == null) ? PlanningGroupByOptions.ByComponent : (PlanningGroupByOptions)obj;
            }
            set
            {
                ViewState["GroupBy"] = value;
            }
        }

        [
         Category("Behavior"),
         DefaultValue(""),
         Description("The ID of the server control that we want to use to display error messages (div, span, etc.)")
         ]
        public string ErrorMessageControlId
        {
            get
            {
                object obj = ViewState["errorMessageControlId"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["errorMessageControlId"] = value;
            }
        }

        [
         Category("Behavior"),
         DefaultValue(""),
         Description("The ID of the dropdown server control that contains the list of group by options")
         ]
        public string GroupByControlId
        {
            get
            {
                object obj = ViewState["GroupByControlId"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["GroupByControlId"] = value;
            }
        }

        [
         Category("Behavior"),
         DefaultValue(""),
         Description("The ID of the dropdown server control that contains the list of releases")
         ]
        public string ReleaseControlId
        {
            get
            {
                object obj = ViewState["ReleaseControlId"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["ReleaseControlId"] = value;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(""),
        Description("The CSS class of the datagrids that dragging onto is considered a deassociation")
        ]
        public string DestinationElementCssClass
        {
            get
            {
                object obj = ViewState["destinationElementCssClass"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["destinationElementCssClass"] = value;
            }
        }

		[
		 Category("Behavior"),
		 DefaultValue(false),
		 Description("Sets the whole planning board to allow editing / viewing artifacts in dialog - defaults to true")
		 ]
		public bool BoardSupportsEditing
		{
			get
			{
				object obj = ViewState["BoardSupportsEditing"];

				return (obj == null) ? true : (bool)obj;
			}
			set
			{
				ViewState["BoardSupportsEditing"] = value;
			}
		}

		#endregion

		#region IScriptControl Members

		public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            ScriptControlDescriptor descriptor = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.PlanningBoard", this.ClientID);

            if (!string.IsNullOrEmpty(CssClass))
            {
                descriptor.AddProperty("cssClass", CssClass);
            }
            if (!string.IsNullOrEmpty(DestinationElementCssClass))
            {
                descriptor.AddProperty("destinationElementCssClass", DestinationElementCssClass);
            }            
            if (ProjectId > -1)
            {
                descriptor.AddProperty("projectId", ProjectId);
            }
            descriptor.AddProperty("groupBy", (int)GroupBy);
            descriptor.AddProperty("releaseId", ReleaseId);
            descriptor.AddProperty("includeDetails", IncludeDetails);
            descriptor.AddProperty("includeIncidents", IncludeIncidents);
            descriptor.AddProperty("includeTasks", IncludeTasks);
            descriptor.AddProperty("includeTestCases", IncludeTestCases);
            descriptor.AddProperty("isIteration", IsIteration);
            descriptor.AddProperty("supportsRanking", SupportsRanking);

            if (!string.IsNullOrEmpty(WebServiceClass))
            {
                descriptor.AddScriptProperty("webServiceClass", WebServiceClass);
            }

            //The base URL of the avatar dynamic image URL
            descriptor.AddProperty("avatarBaseUrl", UrlRewriterModule.ResolveUrl("~/UserAvatar.ashx?" + GlobalFunctions.PARAMETER_USER_ID + "={0}&" + GlobalFunctions.PARAMETER_THEME_NAME + "=" + Page.Theme));

            //If theming is enabled, need to pass the theme folder so that images resolve correctly
            if (Page.EnableTheming && Page.Theme != "")
            {
                if (HttpContext.Current.Request.ApplicationPath == "/")
                {
                    descriptor.AddProperty("themeFolder", "/App_Themes/" + Page.Theme + "/");
                }
                else
                {
                    descriptor.AddProperty("themeFolder", HttpContext.Current.Request.ApplicationPath + "/App_Themes/" + Page.Theme + "/");
                }
            }

			//Check to see if we are authorized to create and/or edit requirements (the main items)
			bool allowEdit = false;
			bool allowCreate = false;
            if (Context != null)
            {
                int currentRoleId = authorizedControlBase.ProjectRoleId;
                bool isSystemAdmin = authorizedControlBase.UserIsAdmin;
                bool isGroupAdmin = authorizedControlBase.UserIsGroupAdmin;
                authorizedControlBase.Authorized_Permission = Project.PermissionEnum.BulkEdit;
                allowEdit = (authorizedControlBase.IsAuthorized(currentRoleId, isSystemAdmin, isGroupAdmin) == Project.AuthorizationState.Authorized);
                if (AllowCreate)
                {
                    //Check permissions
                    authorizedControlBase.Authorized_Permission = Project.PermissionEnum.Create;
                    allowCreate = (authorizedControlBase.IsAuthorized(currentRoleId, isSystemAdmin, isGroupAdmin) == Project.AuthorizationState.Authorized);
                }
            }
			descriptor.AddProperty("allowEdit", allowEdit);
            descriptor.AddProperty("boardSupportsEditing", this.BoardSupportsEditing);
			descriptor.AddProperty("allowCreate", allowCreate);

            if (!string.IsNullOrEmpty(ErrorMessageControlId))
            {
                //First we need to get the server control
                Control errorMessageControl = this.Parent.FindControl(this.ErrorMessageControlId);
                if (errorMessageControl != null)
                {
                    string clientId = errorMessageControl.ClientID;
                    descriptor.AddProperty("errorMessageControlId", clientId);
                }
            }
            if (!String.IsNullOrEmpty(GroupByControlId))
            {
                //First we need to get the server control
                Control groupByControl = this.Page.FindControlRecursive(this.GroupByControlId);
                if (groupByControl != null)
                {
                    string clientId = groupByControl.ClientID;
                    descriptor.AddProperty("groupByControlId", clientId);
                }
            }
            if (!String.IsNullOrEmpty(ReleaseControlId))
            {
                //First we need to get the server control
                Control releaseControl = this.Page.FindControlRecursive(this.ReleaseControlId);
                if (releaseControl != null)
                {
                    string clientId = releaseControl.ClientID;
                    descriptor.AddProperty("releaseControlId", clientId);
                }
            }
            
            //Add any custom client-side handlers
            if (this.handlers != null)
            {
                foreach (KeyValuePair<string, string> handler in this.handlers)
                {
                    descriptor.AddEvent(handler.Key, handler.Value);
                }
            }

            yield return descriptor;
        }

        // Generate the script reference
        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DragDrop.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.Tooltip.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.Equalizer.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.PlanningBoard.js"));
        }

        #endregion

        #region Overrides

        protected override void OnPreRender(System.EventArgs e)
        {
            base.OnPreRender(e);

            ScriptManager manager = ScriptManager.GetCurrent(this.Page);
            manager.RegisterScriptControl(this);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            ScriptManager manager = ScriptManager.GetCurrent(this.Page);
            manager.RegisterScriptDescriptors(this);
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Table;
            }
        }

        #endregion

        #region IAuthorizedControl Members

        /// <summary>
        /// This is the type of artifact that the user's role needs to have permissions for
        /// </summary>
        [
        Bindable(true),
        Category("Security"),
        Description("This is the type of artifact that the user's role needs to have permissions for"),
        DefaultValue(DataModel.Artifact.ArtifactTypeEnum.None),
        PersistenceMode(PersistenceMode.Attribute),
        ]
        public DataModel.Artifact.ArtifactTypeEnum Authorized_ArtifactType
        {
            get
            {
                return authorizedControlBase.Authorized_ArtifactType;
            }
            set
            {
                authorizedControlBase.Authorized_ArtifactType = value;
            }
        }

        /// <summary>
        /// This is the type of action that the user's role needs to have permissions for
        /// </summary>
        [
        Bindable(true),
        Category("Security"),
        Description("This is the type of action that the user's role needs to have permissions for"),
        DefaultValue(Project.PermissionEnum.None),
        PersistenceMode(PersistenceMode.Attribute),
        ]
        public Project.PermissionEnum Authorized_Permission
        {
            get
            {
                return authorizedControlBase.Authorized_Permission;
            }
            set
            {
                authorizedControlBase.Authorized_Permission = value;
            }
        }

        #endregion
    }
}
