using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// This class encapsulates the Bootstrap DateTimePicker client component for use as a server-side control
    /// </summary>
    /// <remarks>
    /// https://eonasdan.github.io/bootstrap-datetimepicker
    /// </remarks>
    [
    ToolboxData("<{0}:DateTimePicker runat=server></{0}:DateTimePicker>"),
    DefaultProperty("Text"),
    ValidationProperty("Text")
    ]
    public class DateTimePicker : WebControl, IScriptControl
    {

        #region Overrides

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
        /// Add any attributes
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

        #endregion

        #region IScriptControl Members

        /// <summary>
        /// Return the various attributes to set on the client component
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            //Write the $create command that actually instantiates the control
            ScriptControlDescriptor desc = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.DateTimePicker", this.ClientID);

            //Need to get the current timezone and culture and send it through
            int utcOffsetMins = (int)Math.Truncate(GlobalFunctions.GetCurrentTimezoneUtcOffset() * 60);
            string currentCultue = CultureInfo.CurrentUICulture.Name;

            //The names used by MomentJS are slightly different than .NET
            string locale = currentCultue.ToLowerInvariant();
            if (locale == "en-us")
            {
                locale = "en";
            }
            if (locale.Contains('-'))
            {
                //Convert things like fr-fr to just "fr"
                string[] components = locale.Split('-');
                if (components.Length > 1 && components[0] == components[1])
                {
                    locale = components[0];
                }
            }

            //Add the properties
            desc.AddProperty("locale", locale);
            desc.AddProperty("utcOffset", utcOffsetMins);

            yield return desc;
        }

        /// <summary>
        /// Return the references to the client script resource files
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ClientScripts.moment-with-locales.min.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.bootstrap-datetimepicker.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DateTimePicker.js"));
        }

        #endregion
    }

    public class UnityDateTimePicker : DateTimePicker
    {
    }
}