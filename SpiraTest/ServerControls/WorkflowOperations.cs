using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// Displays an AJAX server control that displays a list of workflow operations and fires events on the linked AjaxFormManager
    /// when a workflow item is selected
    /// </summary>
    /// <see cref="AjaxFormManager"/>
    [ToolboxData("<{0}:WorkflowOperations runat=server></{0}:WorkflowOperations>")]
    public class WorkflowOperations : WebControl, IScriptControl
    {
        protected Dictionary<string, string> handlers;

        /// <summary>
        /// Constructor
        /// </summary>
        public WorkflowOperations()
        {
        }


        #region Properties

        /// <summary>
        /// Contains the project that the data is a part of
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
        /// Contains the primary key of the artifact being managed
        /// </summary>
        [
        Category("Context"),
        DefaultValue(-1)
        ]
        public int PrimaryKey
        {
            get
            {
                object obj = ViewState["PrimaryKey"];

                return (obj == null) ? -1 : (int)obj;
            }
            set
            {
                ViewState["PrimaryKey"] = value;
            }
        }

        [
        Category("Appearance"),
        DefaultValue(true),
        Description("Should the control automatically load when page first loaded")
        ]
        public bool AutoLoad
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["autoLoad"];

                return (obj == null) ? true : (bool)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["autoLoad"] = value;
            }
        }

        [
        Category("Appearance"),
        DefaultValue(true),
        Description("Should the control be loaded as a vertical button group (if not it is loaded as a dropdown")
        ]
        public bool VerticalGroup
        {
            get
            {
                object obj = ViewState["verticalGroup"];
                return (obj == null) ? true : (bool)obj;
            }
            set
            {
                ViewState["verticalGroup"] = value;
            }
        }

        [
        Category("Appearance"),
        DefaultValue(false),
        Description("Should the control be loaded as a horizontal button group")
        ]
        public bool HorizontalGroup
        {
            get
            {
                object obj = ViewState["horizontalGroup"];
                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["horizontalGroup"] = value;
            }
        }

        [
        Category("Data"),
        DefaultValue(""),
        Description("Contains the fully qualified namespace and class of the web service that will be providing the data to this Ajax server control")
        ]
        public string WebServiceClass
        {
            get
            {
                object obj = ViewState["webServiceClass"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["webServiceClass"] = value;
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
         Description("The ID of the related AjaxFormManager server control")
         ]
        public string FormManagerControlId
        {
            get
            {
                object obj = ViewState["FormManagerControlId"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["FormManagerControlId"] = value;
            }
        }

        #endregion

        #region IScriptControl Members

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            ScriptControlDescriptor descriptor = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.WorkflowOperations", this.ClientID);

            if (!string.IsNullOrEmpty(WebServiceClass))
            {
                descriptor.AddScriptProperty("webServiceClass", WebServiceClass);
            }
            if (ProjectId > -1)
            {
                descriptor.AddProperty("projectId", ProjectId);
            }
            if (PrimaryKey > -1)
            {
                descriptor.AddProperty("primaryKey", PrimaryKey);
            }
            descriptor.AddProperty("autoLoad", this.AutoLoad);
            descriptor.AddProperty("verticalGroup", this.VerticalGroup);
            descriptor.AddProperty("horizontalGroup", this.HorizontalGroup);

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

            if (!string.IsNullOrEmpty(FormManagerControlId))
            {
                //First we need to get the server control
                if (this.Parent.FindControl(this.FormManagerControlId) != null && this.Parent.FindControl(this.FormManagerControlId) is AjaxFormManager)
                {
                    AjaxFormManager formManager = (AjaxFormManager)this.Parent.FindControl(this.FormManagerControlId);
                    string clientId = formManager.ClientID;
                    descriptor.AddProperty("formManagerId", clientId);
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
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.WorkflowOperations.js"));
        }

        #endregion

        /// <summary>
        /// Allows the passing in of a collection of client-side event handlers
        /// </summary>
        /// <param name="handlers">The collection of handlers</param>
        public void SetClientEventHandlers(Dictionary<string, string> handlers)
        {
            this.handlers = handlers;
        }

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
        /// Render the various client component descriptors
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            //Ad the code to create a client component from this DOM element
            ScriptManager manager = ScriptManager.GetCurrent(this.Page);
            manager.RegisterScriptDescriptors(this);
        }

        /// <summary>
        /// Renders the control as a DIV
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        #endregion
    }
}
