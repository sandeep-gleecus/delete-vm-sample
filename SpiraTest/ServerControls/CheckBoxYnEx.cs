using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using System.ComponentModel;

using Inflectra.SpiraTest.Web.ServerControls.Authorization;
using Inflectra.SpiraTest.Business;
using System.Diagnostics;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.ServerControls
{
	/// <summary>
	/// This class extends the default check-box to allow authorization checking
	/// </summary>
	[ToolboxData("<{0}:CheckBoxEx runat=server></{0}:CheckBoxEx>")]
	public class CheckBoxYnEx : System.Web.UI.WebControls.CheckBox, IAuthorizedControl
	{
		protected AuthorizedControlBase authorizedControlBase;

		//Viewstate keys
		protected const string ViewStateKey_MetaData_Base = "MetaData_";

		/// <summary>
		/// Constructor - delegates to base class
		/// </summary>
		public CheckBoxYnEx() : base()
		{
			//Instantiate the authorized control default implementation
			authorizedControlBase = new AuthorizedControlBase(this.ViewState);
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
				if (ViewState[ViewStateKey_MetaData_Base + this.ID] == null)
				{
					return "";
				}
				else
				{
					return ((string)ViewState[ViewStateKey_MetaData_Base + this.ID]);
				}
			}
			set
			{
				ViewState[ViewStateKey_MetaData_Base + this.ID] = value;
			}
		}

        [
 Category("Behavior"),
 DefaultValue(""),
 Description("The client side script method that we want to execute")
 ]
        public string ClientScriptMethod
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["clientScriptMethod"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["clientScriptMethod"] = value;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(""),
        Description("The ID of the server control that we want to execute the client script method of (leave blank for a global function)")
        ]
        public string ClientScriptServerControlId
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["clientScriptServerControlId"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["clientScriptServerControlId"] = value;
            }
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
        /// Adds any client-side ajax handlers
        /// </summary>
        /// <param name="writer"></param>
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            base.AddAttributesToRender(writer);

            //If we have client script associated, need to attach client handler
            if (!string.IsNullOrEmpty(this.ClientScriptMethod))
            {
                //If we don't have a server control then we can just execute the method
                //Otherwise we need to access the actual class
                if (string.IsNullOrEmpty(this.ClientScriptServerControlId))
                {
                    writer.AddAttribute("onclick", this.ClientScriptMethod);
                }
                else
                {
                    //First we need to get the server control
                    Control clientScriptControl = this.Parent.FindControl(this.ClientScriptServerControlId);
                    if (clientScriptControl != null)
                    {
                        string clientId = clientScriptControl.ClientID;
                        writer.AddAttribute("onclick", "$find('" + clientId + "')." + this.ClientScriptMethod);
                    }
                }
            }
        }

		/// <summary>
		/// Make sure that we have the right authorization or configuration to display/enable the control
		/// </summary>
		/// <param name="e">The Event Arguments</param>
		protected override void OnPreRender(EventArgs e)
		{
			//First execute the base class
			base.OnPreRender (e);

            //Check to see if we are authorized, if not then disable the checkbox
            if (Context != null && this.Enabled)
            {
                int currentRoleId = authorizedControlBase.ProjectRoleId;
                bool isSystemAdmin = authorizedControlBase.UserIsAdmin;
                bool isGroupAdmin = authorizedControlBase.UserIsGroupAdmin;
                this.Enabled = (authorizedControlBase.IsAuthorized(currentRoleId, isSystemAdmin, isGroupAdmin) == Project.AuthorizationState.Authorized);
            }

            //Add the CSS Class name to the inner INPUT tag
            this.InputAttributes.Add("class", "CheckBoxYn");

			//Output the jQuery code
			string clientscriptRegister = String.Empty;
			if (!String.IsNullOrWhiteSpace(this.ClientScriptMethod))
			{
				clientscriptRegister = $"$('#{this.ClientID}').on('switchChange.bootstrapSwitch', function(e) {{{this.ClientScriptMethod}}} );";
			}

			string script = "$(function() { $('.CheckBoxYn').bootstrapSwitch({onText:'" + Resources.Main.Global_Yes + "', offText:'" + Resources.Main.Global_No + "'});" + clientscriptRegister + "  });";
			
			this.Page.ClientScript.RegisterStartupScript(typeof(CheckBoxYnEx), this.Page.GetType().FullName, script, true);
		}
	}
}
