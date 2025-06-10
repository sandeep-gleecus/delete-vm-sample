using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Web.ServerControls.Authorization;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Classes;
using System.Diagnostics;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// This class extends the default button control to automatically handle confirmation messages
    /// and also to be able to handle both client-side and server-side events
    /// </summary>
    public class ButtonEx : Button, IAuthorizedControl
    {
        protected string confirmationMessage;
        protected AuthorizedControlBase authorizedControlBase;

        		/// <summary>
		/// Constructor - delegates to base class
		/// </summary>
		public ButtonEx() : base()
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
		/// Whether we want to display a confirmation message or not before executing any server-side events
		/// </summary>
		[
		Bindable(true),
		Category("Behavior"),
		Description("Whether we want to display a confirmation message or not before executing any server-side events"),
		DefaultValue(false),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public bool Confirmation
		{
			get
			{
                if (ViewState["confirmation"] == null)
                {
                    return false;
                }
                else
                {
                    return (bool)ViewState["confirmation"];
                }
			}
			set
			{
				ViewState["confirmation"] = value;
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

		/// <summary>
		/// The confirmation message to use if the Confirm flag is set
		/// </summary>
		[
		Bindable(true),
		Category("Behavior"),
		Description("The confirmation message to use if the Confirm flag is set"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public string ConfirmationMessage
		{
			get
			{
				return this.confirmationMessage;
			}
			set
			{
				this.confirmationMessage = value;
			}
		}

		/// <summary>
		/// Make sure that we have the right authorization or configuration to display/enable the control
		/// </summary>
		/// <param name="e">The parameters passed to handler</param>
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender (e);

			//Check to see if we are authorized, if not then disable the button
			if (Context != null && this.Enabled)
			{
                int currentRoleId = authorizedControlBase.ProjectRoleId;
                bool isSystemAdmin = authorizedControlBase.UserIsAdmin;
                bool isGroupAdmin = authorizedControlBase.UserIsGroupAdmin;
                if (authorizedControlBase.UserIsArtifactCreatorOrOwner)
                {
                    //Limited OK
                    this.Enabled = (authorizedControlBase.IsAuthorized(currentRoleId, isSystemAdmin, isGroupAdmin) != Project.AuthorizationState.Prohibited);
                }
                else
                {
                    //Limited not Sufficient
                    this.Enabled = (authorizedControlBase.IsAuthorized(currentRoleId, isSystemAdmin, isGroupAdmin) == Project.AuthorizationState.Authorized);
                }
			}
		}

		/// <summary>
		/// Adds the confirmation client-side event to the existing control
		/// </summary>
		/// <param name="writer">The HTML writer output stream</param>
		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
            //First add the ID if specified
            if (!String.IsNullOrEmpty(this.ClientID))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            }

			//Next add the standard attributes to the control if we're using server-side events
            if (String.IsNullOrEmpty(this.ClientScriptMethod))
            {
                bool useSubmitBehavior = this.UseSubmitBehavior;
                if (this.Page != null)
                {
                    this.Page.VerifyRenderingInServerForm(this);
                }
                if (useSubmitBehavior)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Type, "submit");
                }
                else
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Type, "button");
                }
                PostBackOptions postBackOptions = this.GetPostBackOptions();
                string uniqueID = this.UniqueID;
                if ((uniqueID != null) && ((postBackOptions == null) || (postBackOptions.TargetControl == this)))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Name, uniqueID);
                }
                if (!String.IsNullOrEmpty(this.CssClass))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Value, this.Text);
                bool isEnabled = base.IsEnabled;
                string firstScript = string.Empty;
                if (isEnabled)
                {
                    firstScript = ServerControlCommon.EnsureEndWithSemiColon(this.OnClientClick);
                    if (base.HasAttributes)
                    {
                        string str3 = base.Attributes["onclick"];
                        if (str3 != null)
                        {
                            firstScript = firstScript + ServerControlCommon.EnsureEndWithSemiColon(str3);
                            base.Attributes.Remove("onclick");
                        }
                    }
                    if (this.Page != null)
                    {
                        string postBackEventReference = this.Page.ClientScript.GetPostBackEventReference(postBackOptions, false);
                        if (postBackEventReference != null)
                        {
                            firstScript = ServerControlCommon.MergeScript(firstScript, postBackEventReference, false);
                        }
                    }

                    //Now add the confirmation message box if the flag is set
                    if (this.Confirmation)
                    {
                        //Add the confirmation message, escaping any single or double quotes
                        string confirmationScript = this.confirmationMessage;
                        confirmationScript = confirmationScript.Replace("'", @"\'");
                        confirmationScript = confirmationScript.Replace("\"", @"\""");
                        if (String.IsNullOrEmpty(firstScript))
                        {
                            firstScript = "return confirm ('" + confirmationScript + "');";
                        }
                        else
                        {
                            firstScript = "if (confirm ('" + confirmationScript + "')) { " + firstScript + " } else { return false }";
                        }
                    }
                }
                if (this.Page != null)
                {
                    this.Page.ClientScript.RegisterForEventValidation(postBackOptions);
                }
                if (firstScript.Length > 0)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Onclick, firstScript);
                }
                if ((this.Enabled && !isEnabled) && this.SupportsDisabledAttribute)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
                }
            }
            else
            {
                //We never want it to use the Type=Submit version for client-side rendering because that will post the form
                writer.AddAttribute(HtmlTextWriterAttribute.Type, "button");

                string uniqueID = this.UniqueID;
                if (uniqueID != null)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Name, uniqueID);
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Value, this.Text);
                if (!String.IsNullOrEmpty(this.CssClass))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);
                }

                if (!this.Enabled && this.SupportsDisabledAttribute)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
                }

                //If we don't have a server control then we can just execute the method
                //Otherwise we need to access the actual class
                if (string.IsNullOrEmpty(this.ClientScriptServerControlId))
                {
                    //Now add the confirmation message box if the flag is set
                    if (this.Confirmation)
                    {
                        string script = this.confirmationMessage;
                        script = script.Replace("'", @"\'");
                        script = script.Replace("\"", @"\""");

                        writer.AddAttribute("onclick", "if (confirm ('" + script + "')) " + this.ClientScriptMethod + ";");
                    }
                    else
                    {
                        writer.AddAttribute("onclick", this.ClientScriptMethod);
                    }
                }
                else
                {
                    //First we need to get the server control
                    Control clientScriptControl = this.Parent.FindControl(this.ClientScriptServerControlId);
                    if (clientScriptControl != null)
                    {
                        string clientId = clientScriptControl.ClientID;
                        //Now add the confirmation message box if the flag is set
                        if (this.Confirmation)
                        {
                            string script = this.confirmationMessage;
                            script = script.Replace("'", @"\'");
                            script = script.Replace("\"", @"\""");

                            writer.AddAttribute("onclick", "if (confirm ('" + script + "')) $find('" + clientId + "')." + this.ClientScriptMethod + ";");
                        }
                        else
                        {
                            writer.AddAttribute("onclick", "$find('" + clientId + "')." + this.ClientScriptMethod);
                        }
                    }
                }
            }
		}
    }
}
