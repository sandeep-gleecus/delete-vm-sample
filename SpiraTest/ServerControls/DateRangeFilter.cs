using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using Inflectra.SpiraTest.Common;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// This class encapsulates the DateRangeFilter client component for use as a server-side control
    /// </summary>
    [
    ToolboxData("<{0}:DateRangeFilter runat=server></{0}:DateRangeFilter>"),
    DefaultProperty("Value"),
    ValidationProperty("Value")
    ]
    public class DateRangeFilter : WebControl, IScriptControl, IPostBackDataHandler
    {
        //Viewstate keys
        protected const string ViewStateKey_MetaData_Base = "MetaData_";

        protected Dictionary<string, string> handlers;

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

        /// <summary>
        /// The date-range value
        /// </summary>
        [
        Bindable(true),
        Category("Data"),
        DefaultValue(null),
        Localizable(true)
        ]
        public DateRange Value
        {
            get
            {
                return (DateRange)ViewState["Value"];
            }

            set
            {
                ViewState["Value"] = value;
            }
        }

        /// <summary>
        /// Returns whether the user entered a valid date-range or not
        /// </summary>
        [
        Category("Data"),
        DefaultValue(true)
        ]
        public bool IsValid
        {
            get
            {
                return this.isValid;
            }
        }
        protected bool isValid = true;

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
            if (this.Enabled)
            {
                scriptManager.RegisterScriptControl(this);
            }

            base.OnPreRender(e);
        }

        /// <summary>
        /// Make sure that the outer div is display 'inline' by using a float style
        /// </summary>
        /// <param name="writer"></param>
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            base.AddAttributesToRender(writer);
            writer.AddStyleAttribute("float", "left");
        }

        /// <summary>
        /// Render the various client component descriptors
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            if (this.Enabled)
            {
                ScriptManager.GetCurrent(this.Page).RegisterScriptDescriptors(this);
            }
        }

        /// <summary>
        /// Renders out the contents if the control is not enabled
        /// </summary>
        /// <param name="output"></param>
        /// <remarks>If enabled, the output is handled by the client component</remarks>
        protected override void RenderContents(HtmlTextWriter output)
        {
            if (!this.Enabled)
            {
                output.Write(this.Value.ToString());
            }
        }

        // Defines the DateChanged event.
        public event System.EventHandler DateChanged;

        //Invoke delegates registered with the Click event.
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
            ScriptControlDescriptor desc = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.DateRangeFilter", this.ClientID);

            //Set the various attributes
            if (this.Value != null)
            {
                string textValue = "";
                if (this.Value.StartDate.HasValue)
                {
                    textValue += String.Format(GlobalFunctions.FORMAT_DATE_EDITABLE, this.Value.StartDate.Value);
                }
                textValue += "|";
                if (this.Value.EndDate.HasValue)
                {
                    textValue += String.Format(GlobalFunctions.FORMAT_DATE_EDITABLE, this.Value.EndDate.Value);
                }

                desc.AddProperty("value", textValue);
            }
            desc.AddProperty("name", this.UniqueID);

            //Add any custom client-side handlers
            if (this.handlers != null)
            {
                foreach (KeyValuePair<string, string> handler in this.handlers)
                {
                    desc.AddEvent(handler.Key, handler.Value);
                }
            }

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
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DateRangeFilter.js"));
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
            string oldValue = "";
            string postedValue;

            // Cache the current value of the property in string representation
            if (this.Value != null)
            {
                oldValue = this.Value.ToString();
            }

            // Get the posted value for the HTML element with the 
            // same ID as the control
            if (postColl != null && !String.IsNullOrEmpty(postDataKey) && postColl[postDataKey] != null)
            {
                postedValue = postColl[postDataKey];

                // Compare the posted value with Text and updates if needed
                if (oldValue != postedValue)
                {
                    if (postedValue == "")
                    {
                        this.Value = null;
                        return true;
                    }
                    try
                    {
                        DateRange dateRange;
                        if (DateRange.TryParse(postedValue, out dateRange))
                        {
                            this.Value = dateRange;
                            return true;
                        }
                        else
                        {
                            //Didn't validate correctly
                            this.isValid = false;
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        //Don't rethrow, just set the validation flag
                        this.isValid = false;
                    }
                }
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

        #region Public Methods

        /// <summary>
        /// Allows the passing in of a collection of client-side event handlers
        /// </summary>
        /// <param name="handlers">The collection of handlers</param>
        public void SetClientEventHandlers(Dictionary<string, string> handlers)
        {
            this.handlers = handlers;
        }

        #endregion
    }
}
