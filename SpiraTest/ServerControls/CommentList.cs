using System;
using System.Drawing.Design;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;

using Inflectra.SpiraTest.Web.ServerControls.Authorization;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// Displays an AJAX-enabled control that displays a list of comments for a particular artifact
    /// </summary>
    /// <remarks>It displays the user's avatar next to their comment</remarks>
    [
    ToolboxData("<{0}:CommentList runat=server></{0}:CommentList>"),
    ]
    public class CommentList : WebControl, IScriptControl
    {
        protected Dictionary<string, string> handlers;

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
        /// Contains the type of the artifact that the comments relate to
        /// </summary>
        [
        Bindable(true),
        Category("Context"),
        Description("Contains the type of the artifact that the comments relate to"),
        DefaultValue(DataModel.Artifact.ArtifactTypeEnum.None),
        PersistenceMode(PersistenceMode.Attribute),
        ]
        public DataModel.Artifact.ArtifactTypeEnum ArtifactType
        {
            get
            {
                object obj = ViewState["ArtifactType"];

                return (obj == null) ? DataModel.Artifact.ArtifactTypeEnum.None : (DataModel.Artifact.ArtifactTypeEnum)obj;
            }
            set
            {
                ViewState["ArtifactType"] = value;
            }
        }

        /// <summary>
        /// Contains the id of the artifact that the comments relate to
        /// </summary>
        [
        Category("Context"),
        DefaultValue(-1)
        ]
        public int ArtifactId
        {
            get
            {
                object obj = ViewState["ArtifactId"];

                return (obj == null) ? -1 : (int)obj;
            }
            set
            {
                ViewState["ArtifactId"] = value;
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
        Category("Appearance"),
        DefaultValue(true),
        Description("Should the control automatically load when page first loaded")
        ]
        public bool AutoLoad
        {
            get
            {
                object obj = ViewState["autoLoad"];

                return (obj == null) ? true : (bool)obj;
            }
            set
            {
                ViewState["autoLoad"] = value;
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

        /// <summary>
        /// Allows the passing in of a collection of client-side event handlers
        /// </summary>
        /// <param name="handlers">The collection of handlers</param>
        public void SetClientEventHandlers(Dictionary<string, string> handlers)
        {
            this.handlers = handlers;
        }

        #endregion

        #region IScriptControl Members

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            ScriptControlDescriptor descriptor = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.CommentList", this.ClientID);

            if (!string.IsNullOrEmpty(WebServiceClass))
            {
                descriptor.AddScriptProperty("webServiceClass", WebServiceClass);
            }
            if (!string.IsNullOrEmpty(CssClass))
            {
                descriptor.AddProperty("cssClass", CssClass);
            }
            if (ProjectId > -1)
            {
                descriptor.AddProperty("projectId", ProjectId);
            }
            if (ArtifactId > -1)
            {
                descriptor.AddProperty("artifactId", ArtifactId);
            }
            if (ArtifactType != DataModel.Artifact.ArtifactTypeEnum.None)
            {
                descriptor.AddProperty("artifactTypeId", (int)ArtifactType);
            }

            //The base URL of the avatar dynamic image URL
            descriptor.AddProperty("avatarBaseUrl", UrlRewriterModule.ResolveUrl("~/UserAvatar.ashx?" + GlobalFunctions.PARAMETER_USER_ID + "={0}&" + GlobalFunctions.PARAMETER_THEME_NAME + "=" + Page.Theme));

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
            descriptor.AddProperty("autoLoad", this.AutoLoad);

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
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.CommentList.js"));
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
    }
}
