using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

using Inflectra.SpiraTest.Web.ServerControls.Authorization;
using Inflectra.SpiraTest.Business;
using System.Diagnostics;
using System.Web;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.ServerControls
{
	/// <summary>
	/// This class extends the default text-box to include an additional
	/// property that can be used to store a meta-data field that is used in dynamic filters
	/// </summary>
	[ToolboxData("<{0}:TextBoxEx runat=server></{0}:TextBoxEx>")]
	public class TextBoxEx : TextBox, IAuthorizedControl
	{
		protected bool dynamicHeight = false;
		protected AuthorizedControlBase authorizedControlBase;
		/// <summary>
		/// Constructor - delegates to base class
		/// </summary>
		public TextBoxEx()
			: base()
		{
			//Instantiate the authorized control default implementation
			authorizedControlBase = new AuthorizedControlBase(this.ViewState);
		}

		/// <summary>
		/// Determines whether the text box should auto-resize in height when text entered
		/// </summary>
        /// <remarks>Not used anymore</remarks>
		[
		Bindable(true),
		Category("Appearance"),
		Description("Determines whether the multi-line text box should auto-resize in height when text entered"),
		DefaultValue(false),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public bool DynamicHeight
		{
			get
			{
				return (this.dynamicHeight);
			}
			set
			{
				this.dynamicHeight = value;
			}
		}

        /// <summary>
        /// Determines whether the textbox should be read-only
        /// <remarks>
        /// Overrides the standard ReadOnly attribute because that turns off server-side postback of data
        /// </remarks>
        [
        Bindable(true),
        Category("Appearance"),
        Description("Determines whether the textbox should be a read-only or not"),
        DefaultValue(false),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public override bool ReadOnly
        {
            get
            {
                return (this.readOnly);
            }
            set
            {
                this.readOnly = value;
            }
        }
        protected bool readOnly = false;

        /// <summary>
        /// Determines whether the textbox should allow client-side script to be entered
        /// </summary>
        [
        Bindable(true),
        Category("Security"),
        Description("Determines whether the textbox should allow client-side script to be entered"),
        DefaultValue(false),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public bool AllowScripting
        {
            get
            {
                return (this.allowScripting);
            }
            set
            {
                this.allowScripting = value;
            }
        }
        protected bool allowScripting = false;

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
        /// The HTML placeholder attribute for text boxes
        /// </summary>
        [
        Bindable(true),
        Category("Display"),
        Description("The HTML placeholder attribute for text boxes"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public string PlaceHolder
        {
            get
            {
                if (ViewState["PlaceHolder"] == null)
                {
                    return "";
                }
                else
                {
                    return ((string)ViewState["PlaceHolder"]);
                }
            }
            set
            {
                ViewState["PlaceHolder"] = value;
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
		/// This overrides the default 'Text' property to strip out unwanted MS Word formatting.
		/// </summary>
		public override string Text
		{
			get
			{
				return this.StripWORD(base.Text);
			}
			set
			{
                //If the allow script property is not enabled, strip out dangerous XSS script
                if (this.AllowScripting)
                {
                    base.Text = value;
                }
                else
                {
                    base.Text = GlobalFunctions.HtmlScrubInput(value);
                }
			}
		}


		/// <summary>
		/// Adds the functionality to support client-size length checking of
		/// multiple-line text areas - specifically the reference to the JavaScript event handlers
		/// </summary>
		/// <param name="e">The Event Arguments</param>
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
			//Now execute the base class
			base.OnPreRender(e);
		}

		/// <summary>
		/// Adds the functionality to support client-size length checking of
		/// multiple-line text areas - specifically adds the client-size event handlers
		/// </summary>
		/// <param name="writer">The HTML write for displaying the markup</param>
		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
            //If read-only set the appropriate styling
            if (ReadOnly)
            {
                this.ControlStyle.CssClass = this.ControlStyle.CssClass + " " + DisabledCssClass;
            }
            //Next add the standard attributes to the control
            base.AddAttributesToRender(writer);

            if (!String.IsNullOrWhiteSpace(PlaceHolder))
            {
                writer.AddAttribute("placeholder", PlaceHolder, false);
            }

            //See if we have the multiple-line case (not rich-text)
            if (this.TextMode == TextBoxMode.MultiLine)
            {
                //Maximum length handling
                if (this.MaxLength > 0)
                {
                    //Now add the maximum length value (Used by all browsers)
                    writer.AddAttribute("maxlength", this.MaxLength.ToString(), false);
                }
                else
                {
                    //Otherwise default to unlimited length
                }

                //Add the flag and onload handler if the user has specified dynamic height
                if (DynamicHeight)
                {
                    writer.AddAttribute("dynamicHeight", "true", false);
                }

                //Add the read-only attribute
                if (ReadOnly)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.ReadOnly, "readonly", false);
                }
            }
		}

        /// <summary>
        /// Overrides the standard postback data handler so that read-only controls can still receive data.
        /// That way we can update the readonly flag on the client-side and it will still work
        /// </summary>
        /// <param name="postDataKey"></param>
        /// <param name="postCollection"></param>
        /// <returns></returns>
        protected override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            string text = this.Text;
            string str2 = postCollection[postDataKey];
            if (!text.Equals(str2, StringComparison.Ordinal))
            {
                this.Text = str2;
                return true;
            }
            return false;
        }

		/// <summary>
		/// Returns a string that has MS Word formatting and HTML comments stripped from it.
		/// </summary>
		/// <param name="inStr">String that has uncleaned text.</param>
		protected string StripWORD(string inStr)
		{
			return Common.Strings.StripWORD(inStr);
		}
	}

    public class UnityTextBoxEx : TextBoxEx
    {
    }
}
