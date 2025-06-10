using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using System.ComponentModel;

using Inflectra.SpiraTest.Web.ServerControls.Authorization;
using Inflectra.SpiraTest.Business;
using System.Collections.Generic;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.ServerControls
{
	/// <summary>
	/// This class extends the default label to allow authorization checking and handle required/disabled/hidden workflow states
	/// </summary>
	[ToolboxData("<{0}:LabelEx runat=server></{0}:LabelEx>")]
    public class LabelEx : System.Web.UI.WebControls.Label, IAuthorizedControl, IScriptControl
	{
		protected AuthorizedControlBase authorizedControlBase;

		/// <summary>
		/// Constructor - delegates to base class
		/// </summary>
		public LabelEx() : base()
		{
			//Instantiate the authorized control default implementation
			authorizedControlBase = new AuthorizedControlBase(this.ViewState);
		}

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

		/// <summary>
		/// Whether the label is for a required value, in which case it will append an asterisk and specify bold font
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description(" Whether the label is for a required value, in which case it will append an asterisk and specify bold font"),
		DefaultValue(false),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public bool Required
		{
			get
			{
				if (ViewState["Required"] == null)
				{
					return false;
				}
				else
				{
                    return ((bool)ViewState["Required"]);
				}
			}
			set
			{
                ViewState["Required"] = value;
			}
		}

        /// <summary>
        /// Should we append a colon to the label (used in forms)
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        Description("Should we append a colon to the label (used in forms)"),
        DefaultValue(false),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public bool AppendColon
        {
            get
            {
                if (ViewState["AppendColon"] == null)
                {
                    return false;
                }
                else
                {
                    return ((bool)ViewState["AppendColon"]);
                }
            }
            set
            {
                ViewState["AppendColon"] = value;
            }
        }

		/// <summary>
		/// Contains meta-data about the field that can be used in dynamic controls (e.g. datagrid)
		/// </summary>
		[
		Bindable(true),
		Category("Misc"),
		Description("Contains meta-data about the field that can be used in dynamic controls"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public string MetaData
		{
			get
			{
				if (ViewState["MetaData"] == null)
				{
					return "";
				}
				else
				{
                    return ((string)ViewState["MetaData"]);
				}
			}
			set
			{
                ViewState["MetaData"] = value;
			}
		}

        /// <summary>
        /// Whether the label can contain markup and we need to ensure that no embedded script is present
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        Description(" Whether the label can contain markup and we need to ensure that no embedded script is present"),
        DefaultValue(false),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public bool ContainsMarkup
        {
            get
            {
                if (ViewState["ContainsMarkup"] == null)
                {
                    return false;
                }
                else
                {
                    return ((bool)ViewState["ContainsMarkup"]);
                }
            }
            set
            {
                ViewState["ContainsMarkup"] = value;
            }
        }

        #region Overrides

        /// <summary>
        /// Registers the client component
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            //Check to see if we are authorized, if not then disable the button
            if (Context != null && this.Enabled)
            {
                int currentRoleId = authorizedControlBase.ProjectRoleId;
                bool isSystemAdmin = authorizedControlBase.UserIsAdmin;
                bool isGroupAdmin = authorizedControlBase.UserIsGroupAdmin;
                this.Enabled = (authorizedControlBase.IsAuthorized(currentRoleId, isSystemAdmin, isGroupAdmin) == Project.AuthorizationState.Authorized);
            }

            ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);
            if (scriptManager == null)
            {
                throw new InvalidOperationException("ScriptManager required on the page.");
            }

            //Register the client component
            if (this.Visible && !String.IsNullOrWhiteSpace(this.ClientID))
            {
                scriptManager.RegisterScriptControl(this);
            }

            //Add on a colon if necessary
            if (AppendColon)
            {
                if (!String.IsNullOrEmpty(Text) && Text.Substring(Text.Length - 1, 1) != ":")
                {
                    Text += ":";
                }
            }

            base.OnPreRender(e);
        }

        /// <summary>
        /// Adds the mouse-over tooltip events to the existing control
        /// </summary>
        /// <param name="writer">The HTML writer output stream</param>
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            //Get the tooltip then remove from control
            string tooltip = this.ToolTip;
            this.ToolTip = "";

            //First add the standard attributes to the control
            base.AddAttributesToRender(writer);

            //Put the tooltip back
            this.ToolTip = tooltip;
        }

        /// <summary>
        /// Render the various client component descriptors
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            //Ad the code to create a client component from this DOM element
            if (this.Visible && !String.IsNullOrWhiteSpace(this.ClientID))
            {
                ScriptManager manager = ScriptManager.GetCurrent(this.Page);
                manager.RegisterScriptDescriptors(this);
            }
        }

        #endregion

        #region IScriptControl Members

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            ScriptControlDescriptor descriptor = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.LabelEx", this.ClientID);

            descriptor.AddProperty("enabled", this.Enabled);
            descriptor.AddProperty("required", this.Required);
            descriptor.AddProperty("tooltip", this.ToolTip);
            descriptor.AddProperty("containsMarkup", this.ContainsMarkup);

            //If we have an associated control and it's one of the special AJAX controls, need to make clicking on the label activate
            //the control correctly. For standard HTML elements, the browser handles this automatically
            if (!String.IsNullOrEmpty(this.AssociatedControlID))
            {
                Control control = this.FindControl(this.AssociatedControlID);
                if (control != null)
                {
                    //See if we have one of the special AJAX types
                    if (control.GetType() == typeof(DropDownListEx) || control.GetType() == typeof(UnityDropDownListEx))
                    {
                        DropDownListEx ddl = (DropDownListEx)control;
                        string clientId = ddl.ClientID;
                        descriptor.AddProperty("associatedControlId", clientId);
                    }
                    if (control.GetType() == typeof(DropDownUserList) || control.GetType() == typeof(UnityDropDownUserList))
                    {
                        DropDownUserList ddl = (DropDownUserList)control;
                        string clientId = ddl.ClientID;
                        descriptor.AddProperty("associatedControlId", clientId);
                    }
                    if (control.GetType() == typeof(DropDownMultiList) || control.GetType() == typeof(UnityDropDownMultiList))
                    {
                        DropDownMultiList ddl = (DropDownMultiList)control;
                        string clientId = ddl.ClientID;
                        descriptor.AddProperty("associatedControlId", clientId);
                    }
                    if (control.GetType() == typeof(DropDownHierarchy) || control.GetType() == typeof(UnityDropDownHierarchy))
                    {
                        DropDownHierarchy ddl = (DropDownHierarchy)control;
                        string clientId = ddl.ClientID;
                        descriptor.AddProperty("associatedControlId", clientId);
                    }
                    if (control.GetType() == typeof(DateControl) || control.GetType() == typeof(UnityDateControl))
                    {
                        DateControl date = (DateControl)control;
                        string clientId = date.ClientID;
                        descriptor.AddProperty("associatedControlId", clientId);
                    }
                    if (control.GetType() == typeof(DateTimePicker) || control.GetType() == typeof(UnityDateTimePicker))
                    {
                        DateTimePicker dateTimePicker = (DateTimePicker)control;
                        string clientId = dateTimePicker.ClientID;
                        descriptor.AddProperty("associatedControlId", clientId);
                    }
                    if (control.GetType() == typeof(RichTextBoxJ))
                    {
                        RichTextBoxJ richTextBoxJ= (RichTextBoxJ)control;
                        string clientId = richTextBoxJ.ClientID;
                        descriptor.AddProperty("associatedControlId", clientId);
                    }
                    if (control.GetType() == typeof(DateRangeFilter))
                    {
                        DateRangeFilter filter = (DateRangeFilter)control;
                        string clientId = filter.ClientID;
                        descriptor.AddProperty("associatedControlId", clientId);
                    }
                    if (control.GetType() == typeof(DecimalRangeFilter))
                    {
                        DecimalRangeFilter filter = (DecimalRangeFilter)control;
                        string clientId = filter.ClientID;
                        descriptor.AddProperty("associatedControlId", clientId);
                    }
                    if (control.GetType() == typeof(IntRangeFilter))
                    {
                        IntRangeFilter filter = (IntRangeFilter)control;
                        string clientId = filter.ClientID;
                        descriptor.AddProperty("associatedControlId", clientId);
                    }
                    if (control.GetType() == typeof(EffortRangeFilter))
                    {
                        EffortRangeFilter filter = (EffortRangeFilter)control;
                        string clientId = filter.ClientID;
                        descriptor.AddProperty("associatedControlId", clientId);
                    }
                }
            }

            yield return descriptor;
        }

        // Generate the script reference
        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.LabelEx.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.Tooltip.js"));
        }

        #endregion
	}
}
