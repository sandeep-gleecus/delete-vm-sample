using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;

namespace Inflectra.SpiraTest.Web.ServerControls
{
	/// <summary>
	/// GroupRadioButton control is a standard radio-button with the extended 
	/// abilities to be used in groups.
	/// </summary>
	/// <remarks>
	/// Standard <see cref="System.Web.UI.WebControls.RadioButton"/> controls 
	/// cannot be grouped when are placed at the different rows of the DataGrid, 
	/// DataList, Repeater, etc. controls. 
	/// 
	/// The "name" attribute of the radio button HTML control that is rendered 
	/// at the web form after RadioButton control has been executed is depend 
	/// on the UniqueID of the RadioButton. So for the different rows of the 
	/// DataGrid/DataList/Repeater these attributes are different and radio 
	/// buttons do not belong to the same group.
	/// </remarks>	
	[ToolboxData("<{0}:RadioButtonEx runat=server></{0}:RadioButtonEx>")]
	public class RadioButtonEx : RadioButton, IPostBackDataHandler
	{
		public RadioButtonEx() : base()
		{
		}

		#region Properties

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

		private string Value
		{
			get
			{
				string val = Attributes["value"];
				if(val == null)
					val = UniqueID;
				else
					val = UniqueID + "_" + val;
				return val;
			}
		}

		#endregion
		
		#region Rendering

		protected override void Render(HtmlTextWriter output)
		{
            if (String.IsNullOrEmpty(Text))
            {
                this.RenderInputTag(output);
            }
            else
            {
                if (this.TextAlign == TextAlign.Left)
                {
                    this.RenderLabel(output, Text, ClientID);
                    this.RenderInputTag(output);
                }
                else
                {
                    this.RenderInputTag(output);
                    this.RenderLabel(output, Text, ClientID);
                }
            }
		}

        private void RenderLabel(HtmlTextWriter writer, string text, string clientID)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.For, clientID);
            writer.RenderBeginTag(HtmlTextWriterTag.Label);
            writer.Write(text);
            writer.RenderEndTag();
        }

		private void RenderInputTag(HtmlTextWriter htw)
		{
			htw.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
			htw.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
			htw.AddAttribute(HtmlTextWriterAttribute.Name, GroupName);
			htw.AddAttribute(HtmlTextWriterAttribute.Value, Value);
            if (Checked)
            {
                htw.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
            }
            if (!Enabled)
            {
                htw.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            }
            //We might be passed this through as an attribute, in which case render
            if (!String.IsNullOrEmpty(this.Attributes["disabled"]) && this.Attributes["disabled"].ToLowerInvariant() == "disabled")
            {
                htw.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            }

            //If we have client script associated, need to attach client handler
            if (!string.IsNullOrEmpty(this.ClientScriptMethod))
            {
                //If we don't have a server control then we can just execute the method
                //Otherwise we need to access the actual class
                if (string.IsNullOrEmpty(this.ClientScriptServerControlId))
                {
                    htw.AddAttribute("onclick", this.ClientScriptMethod);
                }
                else
                {
                    //First we need to get the server control
                    Control clientScriptControl = this.Parent.FindControl(this.ClientScriptServerControlId);
                    if (clientScriptControl != null)
                    {
                        string clientId = clientScriptControl.ClientID;
                        htw.AddAttribute("onclick", "$find('" + clientId + "')." + this.ClientScriptMethod);
                    }
                }
            }

			string onClick = Attributes["onclick"];
			if(AutoPostBack)
			{
				if(onClick != null)
					onClick = String.Empty;
                onClick += Page.ClientScript.GetPostBackEventReference(this, String.Empty);
				htw.AddAttribute(HtmlTextWriterAttribute.Onclick, onClick);
				htw.AddAttribute("language", "javascript");
			}
			else
			{
				if(onClick != null)
					htw.AddAttribute(HtmlTextWriterAttribute.Onclick, onClick);
			}

			if(AccessKey.Length > 0)
				htw.AddAttribute(HtmlTextWriterAttribute.Accesskey, AccessKey);
			if(TabIndex != 0)
				htw.AddAttribute(HtmlTextWriterAttribute.Tabindex, 
					TabIndex.ToString(NumberFormatInfo.InvariantInfo));
			htw.RenderBeginTag(HtmlTextWriterTag.Input);
			htw.RenderEndTag();


		}

		#endregion

		#region IPostBackDataHandler Members

		void IPostBackDataHandler.RaisePostDataChangedEvent()
		{
			OnCheckedChanged(EventArgs.Empty);
		}

		bool IPostBackDataHandler.LoadPostData(string postDataKey, 
			System.Collections.Specialized.NameValueCollection postCollection)
		{
			bool result = false;
			string value = postCollection[GroupName];
			if((value != null) && (value == Value))
			{
				if(!Checked)
				{
					Checked = true;
					result = true;
				}
			}
			else
			{
				if(Checked)
					Checked = false;
			}
			return result;
		}

		#endregion
	}
}
