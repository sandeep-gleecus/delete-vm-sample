using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Inflectra.SpiraTest.Web.ServerControls
{
	/// <summary>
	/// This class encapsulates the DatePicker client component for use as a server-side control
	/// </summary>
	[
    ToolboxData("<{0}:DateControl runat=server></{0}:DateControl>"),
    DefaultProperty("Text"),
    ValidationProperty("Text")
    ]
    public class DateControl : WebControl, IScriptControl, IPostBackDataHandler
	{
		//Viewstate keys
		protected const string ViewStateKey_MetaData_Base = "MetaData_";

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
        Category("Appearance"),
        DefaultValue(""),
        Description("The css class to use for a disabled date control")
        ]
        public new string DisabledCssClass
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["DisabledCssClass"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["DisabledCssClass"] = value;
            }
        }

        /// <summary>
        /// The textual representation of the date
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        DefaultValue(""),
        Localizable(true)
        ]
        public string Text
        {
            get
            {
                String s = (String)ViewState["Text"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["Text"] = value;
            }
        }

        /// <summary>
        /// Changes the base tag to a DIV instead of a span
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        /// <summary>
        /// Registers the client component
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);
            if (scriptManager == null)
            {
                throw new InvalidOperationException("ScriptManager required on the page.");
            }

            //Register the client component
            scriptManager.RegisterScriptControl(this);

            base.OnPreRender(e);
        }

        /// <summary>
        /// Make sure that the outer div is display 'inline' by using a float style
        /// </summary>
        /// <param name="writer"></param>
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            if (this.ID != null)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            }
            if (this.ControlStyleCreated && !this.ControlStyle.IsEmpty)
            {
                this.ControlStyle.AddAttributesToRender(writer, this);
            }
            //writer.AddStyleAttribute("float", "left");
            if (this.Attributes != null)
            {
                System.Web.UI.AttributeCollection attributes = this.Attributes;
                IEnumerator enumerator = attributes.Keys.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string current = (string)enumerator.Current;
                    writer.AddAttribute(current, attributes[current]);
                }
            }
        }


        /// <summary>
        /// Render the various client component descriptors
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
            ScriptManager.GetCurrent(this.Page).RegisterScriptDescriptors(this);
        }

        /// <summary>
        /// Prevent the default rendering of the contents
        /// </summary>
        /// <param name="output">The output writer</param>
        /// <remarks>The output is handled by the client component</remarks>
        protected override void RenderContents(HtmlTextWriter output)
        {
            //Do Nothing
        }

        /// <summary>
        /// Defines the DateChanged event.
        /// </summary>
        public event System.EventHandler DateChanged;

        /// <summary>
        /// Invoke delegates registered with the Click event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDateChanged(System.EventArgs e)
        {
            if (DateChanged != null)
            {
                DateChanged(this, e);
            }
        }

        /* IScriptControl Members */

        /// <summary>
        /// Return the various attributes to set on the client component
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            //Write the $create command that actually instantiates the control
            ScriptControlDescriptor desc = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.DatePicker", this.ClientID);

            //Set the various attributes
            desc.AddProperty("value", this.Text);
            desc.AddProperty("name", this.UniqueID);
            desc.AddProperty("enabled", this.Enabled);
            desc.AddProperty("enabledCssClass", this.CssClass);
            desc.AddProperty("disabledCssClass", this.DisabledCssClass);

            //Finally we need to specify the server date format that we're using (for date controls)
            string dateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower(System.Globalization.CultureInfo.InvariantCulture);

            //Need to convert to the format expected by the control:
            //	m Month number (01 - January, etc.) 
            //	M = Month name 
            //	d = Date 
            //	y = Last two digits of the year 
            //	Y = All four digits of the year 
            //	W = Name of the day of the week (Monday, etc.)
            dateFormat = dateFormat.Replace("mm", "m");
            dateFormat = dateFormat.Replace("dd", "d");
            dateFormat = dateFormat.Replace("yyyy", "Y");
            dateFormat = dateFormat.Replace("yy", "y");
            desc.AddProperty("dateFormat", dateFormat);

            yield return desc;
        }

        /// <summary>
        /// Return the references to the client script resource files
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DatePicker.js"));
        }

        /* IPostBackDataHandler methods */

        /// <summary>
        /// Updates the text property when the page is posted
        /// </summary>
        /// <param name="postDataKey">The client id of the control</param>
        /// <param name="postColl">The collection of name/value pairs</param>
        /// <returns>Indicates whether the state has changed or not</returns>
        public bool LoadPostData(string postDataKey, NameValueCollection postColl)
        {
            string oldValue, postedValue;

            // Cache the current value of the property 
            oldValue = this.Text;

            // Get the posted value for the HTML element with the 
            // same ID as the control		
            postedValue = postColl[postDataKey];

            // Compare the posted value with Text and updates if needed
            if (oldValue != postedValue)
            {
                this.Text = postedValue;
                return true;
            }

            // Indicates whether the state has changed
            return false;
        }

        /// <summary>
        /// Raises the data changed event
        /// </summary>
        public void RaisePostDataChangedEvent()
        {
            this.OnDateChanged(EventArgs.Empty);
        }
	}

    public class UnityDateControl : DateControl
    {
    }
}
