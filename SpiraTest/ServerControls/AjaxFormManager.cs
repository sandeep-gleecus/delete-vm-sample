using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.Design;
using System.Drawing.Design;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// Displays a special AJAX form manager that's used for asycnhronously populating form controls and also capturing the input,
    /// calling the ASP.NET validators and then sending the resuts to the web service. This is used instead of a traditional
    /// ASP.NET postback. It also includes functionality for tracking when form elements have changed and making sure the user
    /// is prompted to save when they try and navigate away from the page
    /// </summary>
    [
    ToolboxData("<{0}:AjaxFormManager runat=server></{0}:AjaxFormManager>"),
    ParseChildren(true),
    PersistChildren(false)
    ]
    public class AjaxFormManager : Control, IScriptControl
    {
        protected Dictionary<string, string> handlers;

        /// <summary>
        /// Constructor
        /// </summary>
        public AjaxFormManager()
        {
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
            //We just display a hidden SPAN tag to wrap the control functionality
            writer.AddAttribute("id", this.ClientID);
            writer.AddStyleAttribute("display", "none");
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.RenderEndTag();

            //Ad the code to create a client component from this DOM element
            ScriptManager.GetCurrent(this.Page).RegisterScriptDescriptors(this);
        }

        #endregion

        #region Properties

        [
         Category("Appearance"),
         DefaultValue(true),
         Description("Should the form automatically load when the page first loaded")
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
        DefaultValue(false),
        Description("Should we simply not load the data on an authorization failure, vs. redirecting the whole page")
        ]
        public bool HideOnAuthorizationFailure
        {
            get
            {
                object obj = ViewState["HideOnAuthorizationFailure"];

                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["HideOnAuthorizationFailure"] = value;
            }
        }

        [
         Category("Behavior"),
         DefaultValue(false),
         Description("Should the form automatically set the page name")
         ]
        public bool DisplayPageName
        {
            get
            {
                object obj = ViewState["displayPageName"];

                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["displayPageName"] = value;
            }
        }        

        [
         Category("Behavior"),
         DefaultValue(false),
         Description("Does the form use workflows")
         ]
        public bool WorkflowEnabled
        {
            get
            {
                object obj = ViewState["WorkflowEnabled"];

                return (obj == null) ? false : (bool)obj;
            }
            set
            {
                ViewState["WorkflowEnabled"] = value;
            }
        }

		[
		 Category("Behavior"),
		 DefaultValue(false),
		 Description("Makes the form full read only")
		 ]
		public bool ReadOnly
		{
			get
			{
				object obj = ViewState["ReadOnly"];

				return (obj == null) ? false : (bool)obj;
			}
			set
			{
				ViewState["ReadOnly"] = value;
			}
		}

		/// <summary>
		/// The field that contains the name of the artifact (used when setting the page title)
		/// </summary>
		public string NameField
        {
            get
            {
                object obj = ViewState["NameField"];

                return (obj == null) ? "Name" : (string)obj;
            }
            set
            {
                ViewState["NameField"] = value;
            }
        }

        /// <summary>
        /// The field that contains the description of the artifact (used when setting the page meta-description)
        /// </summary>
        public string DescriptionField
        {
            get
            {
                object obj = ViewState["DescriptionField"];

                return (obj == null) ? "Description" : (string)obj;
            }
            set
            {
                ViewState["DescriptionField"] = value;
            }
        }

        /// <summary>
        /// The name of the artifact to use when it's creating a new item (e.g. 'New Incident')
        /// </summary>
        public string NewItemName
        {
            get
            {
                object obj = ViewState["NewItemName"];

                return (obj == null) ? "" : (string)obj;
            }
            set
            {
                ViewState["NewItemName"] = value;
            }
        }

        [
         Category("Appearance"),
         DefaultValue(true),
         Description("Should the form automatically check to make sure all changes have been saved")
         ]
        public bool CheckUnsaved
        {
            get
            {
                object obj = ViewState["CheckUnsaved"];

                return (obj == null) ? true : (bool)obj;
            }
            set
            {
                ViewState["CheckUnsaved"] = value;
            }
        }

        /// <summary>
        /// Returns a list of controls that we want to populate with data
        /// </summary>
        [
        DesignerSerializationVisibility(DesignerSerializationVisibility.Visible),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public List<AjaxFormControl> ControlReferences
        {
            get
            {
                return this.controlReferences;
            }
        }
        protected List<AjaxFormControl> controlReferences = new List<AjaxFormControl>();

        /// <summary>
        /// Returns a list of save buttons that the manager interacts with
        /// </summary>
        [
        DesignerSerializationVisibility(DesignerSerializationVisibility.Visible),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public List<AjaxFormSaveButton> SaveButtons
        {
            get
            {
                return this.saveButtons;
            }
        }
        protected List<AjaxFormSaveButton> saveButtons = new List<AjaxFormSaveButton>();

        /// <summary>
        /// Returns a list of hyperlink controls that the manager interacts with
        /// </summary>
        [
        DesignerSerializationVisibility(DesignerSerializationVisibility.Visible),
        PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public List<AjaxHyperLinkControl> HyperLinkControls
        {
            get
            {
                return this.hyperLinkControls;
            }
        }
        protected List<AjaxHyperLinkControl> hyperLinkControls = new List<AjaxHyperLinkControl>();

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
        /// Contains the primary key of the data being looked-up
        /// </summary>
        [
        Category("Context"),
        DefaultValue(null)
        ]
        public int? PrimaryKey
        {
            get
            {
                object obj = ViewState["PrimaryKey"];

                return (int?)obj;
            }
            set
            {
                ViewState["PrimaryKey"] = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        [Editor(typeof(ImageUrlEditor), typeof(UITypeEditor))]
        [Description("Used to display a normal item")]
        public string ItemImage
        {
            get
            {
                object obj = ViewState["itemImage"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["itemImage"] = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        [Editor(typeof(ImageUrlEditor), typeof(UITypeEditor))]
        [Description("Used to display an alterate item image (e.g. test case with steps or iteration vs. release)")]
        public string AlternateItemImage
        {
            get
            {
                object obj = ViewState["alternateItemImage"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["alternateItemImage"] = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        [Editor(typeof(ImageUrlEditor), typeof(UITypeEditor))]
        [Description("Used to display a summary item in its normal (non-expanded) state")]
        public string SummaryItemImage
        {
            get
            {
                object obj = ViewState["summaryItemImage"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["summaryItemImage"] = value;
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
        Category("Context"),
        DefaultValue(""),
        Description("Contains the name of the artifact type being managed, user for display purposes.")
        ]
        public string ArtifactTypeName
        {
            get
            {
                object obj = ViewState["artifactTypeName"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["artifactTypeName"] = value;
            }
        }

        [
        Category("Context"),
        DefaultValue(""),
        Description("Contains the two-letter prefix of the artifact type being managed, user for display purposes.")
        ]
        public string ArtifactTypePrefix
        {
            get
            {
                object obj = ViewState["artifactTypePrefix"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["artifactTypePrefix"] = value;
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
        Description("The ID of the server control that we want to use to display the folder path")
        ]
        public string FolderPathControlId
        {
            get
            {
                object obj = ViewState["FolderPathControlId"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["FolderPathControlId"] = value;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(""),
        Description("The url template to use in the folder path")
        ]
        public string FolderPathUrlTemplate
        {
            get
            {
                object obj = ViewState["FolderPathUrlTemplate"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["FolderPathUrlTemplate"] = value;
            }
        }

        [
         Category("Behavior"),
         DefaultValue(""),
         Description("The ID of the server control that we want to use to revert a workflow operation")
         ]
        public string RevertButtonControlId
        {
            get
            {
                object obj = ViewState["RevertButtonControlId"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["RevertButtonControlId"] = value;
            }
        }

        [
        Category("Appearance"),
        DefaultValue(""),
        Description("The ID of the server control that we want to use to display the artifact icon")
        ]
        public string ArtifactImageControlId
        {
            get
            {
                object obj = ViewState["ArtifactImageControlId"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["ArtifactImageControlId"] = value;
            }
        }

        [
          Category("Behavior"),
          DefaultValue(""),
          Description("The ID of the WorkflowOperations server control that displays the list of workflow operations, use a comma to separate multiple controls")
          ]
        public string WorkflowOperationsControlId
        {
            get
            {
                object obj = ViewState["WorkflowOperationsControlId"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["WorkflowOperationsControlId"] = value;
            }
        }

        #endregion

        #region IScriptControl Members

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            ScriptControlDescriptor descriptor = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.AjaxFormManager", this.ClientID);

            if (!string.IsNullOrEmpty(WebServiceClass))
            {
                descriptor.AddScriptProperty("webServiceClass", WebServiceClass);
            }
            if (!string.IsNullOrEmpty(ArtifactTypeName))
            {
                descriptor.AddProperty("artifactTypeName", ArtifactTypeName);
            }
            if (!string.IsNullOrEmpty(ArtifactTypePrefix))
            {
                descriptor.AddProperty("artifactTypePrefix", ArtifactTypePrefix);
            }
            if (ProjectId > -1)
            {
                descriptor.AddProperty("projectId", ProjectId);
            }
            if (PrimaryKey.HasValue)
            {
                descriptor.AddProperty("primaryKey", PrimaryKey);
            }
            if (!string.IsNullOrEmpty(ItemImage))
            {
                descriptor.AddProperty("itemImage", ItemImage);
            }
            if (!string.IsNullOrEmpty(AlternateItemImage))
            {
                descriptor.AddProperty("alternateItemImage", AlternateItemImage);
            }
            if (!string.IsNullOrEmpty(SummaryItemImage))
            {
                descriptor.AddProperty("summaryItemImage", SummaryItemImage);
            }
            if (!string.IsNullOrEmpty(NewItemName))
            {
                descriptor.AddProperty("newItemName", NewItemName);
            }

            descriptor.AddProperty("autoLoad", this.AutoLoad);
            descriptor.AddProperty("checkUnsaved", this.CheckUnsaved);
			descriptor.AddProperty("workflowEnabled", this.WorkflowEnabled);
            descriptor.AddProperty("readOnly", this.ReadOnly);
			descriptor.AddProperty("displayPageName", this.DisplayPageName);
            descriptor.AddProperty("licensedProduct", Common.ConfigurationSettings.Default.License_ProductType);
            descriptor.AddProperty("hideOnAuthorizationFailure", this.HideOnAuthorizationFailure);
            if (!string.IsNullOrEmpty(NameField))
            {
                descriptor.AddProperty("nameField", NameField);
            }
            if (!string.IsNullOrEmpty(DescriptionField))
            {
                descriptor.AddProperty("descriptionField", NameField);
            }
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
            if (!string.IsNullOrEmpty(FolderPathControlId))
            {
                //First we need to get the server control
                Control folderPathControl = this.Parent.FindControl(this.FolderPathControlId);
                if (folderPathControl != null)
                {
                    string clientId = folderPathControl.ClientID;
                    descriptor.AddProperty("folderPathControlId", clientId);
                }
            }
            if (!string.IsNullOrEmpty(FolderPathUrlTemplate))
            {
                descriptor.AddProperty("folderPathUrlTemplate", FolderPathUrlTemplate);
            }
            //

            if (!string.IsNullOrEmpty(RevertButtonControlId))
            {
                //First we need to get the server control
                Control revertButtonControlId = this.Parent.FindControl(this.RevertButtonControlId);
                if (revertButtonControlId != null)
                {
                    string clientId = revertButtonControlId.ClientID;
                    descriptor.AddProperty("revertButtonControlId", clientId);
                }
            }

            if (!string.IsNullOrEmpty(ArtifactImageControlId))
            {
                //First we need to get the server control
                Control artifactImageControlId = this.Parent.FindControl(this.ArtifactImageControlId);
                if (artifactImageControlId != null)
                {
                    string clientId = artifactImageControlId.ClientID;
                    descriptor.AddProperty("artifactImageControlId", clientId);
                }
            }

            if (!string.IsNullOrEmpty(WorkflowOperationsControlId))
            {
                //See if we have more than one control (comma-separated)
                if (WorkflowOperationsControlId.Contains(","))
                {
                    string[] controlIds = WorkflowOperationsControlId.Split(',');
                    string clientIds = "";
                    foreach (string controlId in controlIds)
                    {
                        Control workflowOperationsControl = Page.FindControlRecursive(controlId);
                        if (workflowOperationsControl != null && workflowOperationsControl is WorkflowOperations)
                        {
                            string clientId = workflowOperationsControl.ClientID;
                            if (clientIds == "")
                            {
                                clientIds = clientId;
                            }
                            else
                            {
                                clientIds += "," + clientId;
                            }
                        }
                    }
                    descriptor.AddProperty("workflowOperationsControlId", clientIds);
                }
                else
                {
                    //First we need to get the server control
                    Control workflowOperationsControl = Page.FindControlRecursive(this.WorkflowOperationsControlId);
                    if (workflowOperationsControl != null && workflowOperationsControl is WorkflowOperations)
                    {
                        string clientId = workflowOperationsControl.ClientID;
                        descriptor.AddProperty("workflowOperationsControlId", clientId);
                    }
                }
            }

            //If theming is enabled, need to pass the theme folder so that images resolve correctly
            if (Page.EnableTheming && Page.Theme != "")
            {
                if (HttpContext.Current.Request.ApplicationPath == "/")
                {
                    descriptor.AddProperty("themeFolder", "/App_Themes/" + Page.Theme + "/");
                }
                else
                {
                    descriptor.AddProperty("themeFolder", HttpContext.Current.Request.ApplicationPath + "/App_Themes/" + Page.Theme + "/");
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

            //Need to convert the list of control references into a simple JSON dictionary
            //Also track the associated labels for each control
            string workflowStepLabelId = "";
            string workflowStepField = "";
            Dictionary<string, string> controlReferences = new Dictionary<string, string>();
            Dictionary<string, string> controlLabels = new Dictionary<string, string>();
            Dictionary<string,string> controlMapping = new Dictionary<string,string>(); //Temp, used to map server to client ids
            foreach (AjaxFormControl formControl in this.controlReferences)
            {
                //Need to locate the control and get its client id and type
                Control control = this.Page.FindControlRecursive(formControl.ControlId);
                if (control != null)
                {
                    if (!String.IsNullOrEmpty(control.ClientID) && !controlReferences.ContainsKey(control.ClientID))
                    {
                        string propertyName = "";
                        if (!String.IsNullOrEmpty(formControl.PropertyName))
                        {
                            propertyName = formControl.PropertyName;
                        }
                        string controlType = control.GetType().Name;
                        string controlClientID = control.ClientID;
                        controlReferences.Add(controlType + ":" + controlClientID, formControl.DataField + ":" + propertyName + ":" + (int)formControl.Direction + ":" + formControl.ChangesWorkflow.ToString().ToLowerInvariant() + ":" + formControl.IsWorkflowStep.ToString().ToLowerInvariant());
                        if (!controlMapping.ContainsKey(control.ID))
                        {
                            controlMapping.Add(control.ID, controlClientID);
                        }

                        if (String.IsNullOrEmpty(workflowStepLabelId) && formControl.IsWorkflowStep)
                        {
                            workflowStepLabelId = controlClientID;
                            workflowStepField = formControl.DataField;
                        }
                    }
                }
            }
            descriptor.AddProperty("controlReferences", controlReferences);

            //Now add the labels associated with the controls, ultimately we have a dictionary of
            //(key=label client id, value=control client id)
            List<LabelEx> labels = this.Page.GetAllControlsByType<LabelEx>();
            foreach (LabelEx label in labels)
            {
                if (!String.IsNullOrWhiteSpace(label.AssociatedControlID))
                {
                    if (controlMapping.ContainsKey(label.AssociatedControlID))
                    {
                        if (!controlLabels.ContainsKey(label.ClientID))
                        {
                            controlLabels.Add(label.ClientID, controlMapping[label.AssociatedControlID]);
                        }
                    }
                }
            }
            descriptor.AddProperty("controlLabels", controlLabels);

            //Need to convert the list of save buttons into a simple JSON dictionary
            Dictionary<string, string> saveButtons = new Dictionary<string, string>();
            foreach (AjaxFormSaveButton saveButton in this.saveButtons)
            {
                //See if we have a specified server control naming container
                if (String.IsNullOrEmpty(saveButton.ContainerId))
                {
                    //Need to locate the control and get its client id and type
                    Control control = this.Parent.FindControl(saveButton.ControlId);

                    //Try a recursive query from the page if not found
                    if (control == null)
                    {
                        control = this.Page.FindControlRecursive(saveButton.ControlId);
                    }
                    if (control != null)
                    {
                        if (!String.IsNullOrEmpty(control.ClientID) && !controlReferences.ContainsKey(control.ClientID))
                        {
                            saveButtons.Add(control.ClientID, control.GetType().Name);
                        }
                    }
                }
                else
                {
                    //Need to locate the container
                    Control container = this.Parent.FindControl(saveButton.ContainerId);
                    if (container != null)
                    {
                        //Need to locate the control and get its client id
                        Control control = container.FindControl(saveButton.ControlId);
                        if (control != null)
                        {
                            if (!String.IsNullOrEmpty(control.ClientID) && !controlReferences.ContainsKey(control.ClientID))
                            {
                                saveButtons.Add(control.ClientID, control.GetType().Name);
                            }
                        }
                    }
                }
            }
            descriptor.AddProperty("saveButtons", saveButtons);

            //Need to convert the list of hyperlink controls into a simple JSON dictionary
            Dictionary<string, string> hyperLinkControls = new Dictionary<string, string>();
            foreach (AjaxHyperLinkControl hyperLinkControl in this.hyperLinkControls)
            {
                //Need to locate the control and get its client id and type
                Control control = this.Parent.FindControl(hyperLinkControl.ControlId);

                //Try a recursive query from the page if not found
                if (control == null)
                {
                    control = this.Page.FindControlRecursive(hyperLinkControl.ControlId);
                }
                if (control != null)
                {
                    //Make sure it's a hyperlink server control
                    if (control is HyperLinkEx && !String.IsNullOrEmpty(control.ClientID) && !controlReferences.ContainsKey(control.ClientID))
                    {
                        hyperLinkControls.Add(control.ClientID, hyperLinkControl.DataField + ":" + hyperLinkControl.UrlFormatString);
                    }
                }
            }
            descriptor.AddProperty("hyperLinkControls", hyperLinkControls);

            //We return the control that displays the workflow step/status
            if (!String.IsNullOrEmpty(workflowStepLabelId))
            {
                descriptor.AddProperty("workflowStepLabel", workflowStepLabelId);
            }
            if (!String.IsNullOrEmpty(workflowStepField))
            {
                descriptor.AddProperty("workflowStepField", workflowStepField);
            }

            yield return descriptor;
        }

        // Generate the script reference
        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            //This Control
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.AjaxFormManager.js"));

            //Child Controls
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DialogBoxPanel.js"));
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
    }

    /// <summary>
    /// Stores references to the save buttons on the page that will be controlled by the manager
    /// </summary>
    [ToolboxData("<{0}:AjaxFormSaveButton runat=server></{0}:AjaxFormSaveButton>")]
    public class AjaxFormSaveButton
    {
        /// <summary>
        /// The ID of the server naming container control that the target control is in
        /// (not used if the target control is in the same container as this FormManager)
        /// </summary>
        public string ContainerId
        {
            get;
            set;
        }

        /// <summary>
        /// The ID of the server control that performs a save action
        /// </summary>
        public string ControlId
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Stores references to the controls that need to be data-bound and the data-field to use
    /// </summary>
    [ToolboxData("<{0}:AjaxFormControl runat=server></{0}:AjaxFormControl>")]
    public class AjaxFormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AjaxFormControl()
        {
            Direction = FormControlDirection.InOut;
            IsWorkflowStep = false;
            ChangesWorkflow = false;
        }

        public enum FormControlDirection
        {
            In = 1,
            Out = 2,
            InOut = 3
        }

        /// <summary>
        /// The ID of the server control that we want to populate with async data
        /// </summary>
        public string ControlId
        {
            get;
            set;
        }

        /// <summary>
        /// The name of the datafield we want to use to populate this control
        /// </summary>
        public string DataField
        {
            get;
            set;
        }

        /// <summary>
        /// The name of the datafield property we want to use to populate this control (defaults to the textValue)
        /// </summary>
        public string PropertyName
        {
            get;
            set;
        }

        /// <summary>
        /// The direction that data flows (defaults to InOut)
        /// </summary>
        public FormControlDirection Direction
        {
            get;
            set;
        }

        /// <summary>
        /// When set to true, changing this field changes the artifact's workflow (e.g. the product of a ticket or type of an incident)
        /// </summary>
        public bool ChangesWorkflow
        {
            get;
            set;
        }

        /// <summary>
        /// When set to true, this field corresponds to the step in the workflow (e.g. ticket status)
        /// </summary>
        public bool IsWorkflowStep
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Stores references to any hyperlinks that need to be data-bound and the data-field to use
    /// </summary>
    [ToolboxData("<{0}:AjaxHyperLinkControl runat=server></{0}:AjaxHyperLinkControl>")]
    public class AjaxHyperLinkControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AjaxHyperLinkControl()
        {
        }

        /// <summary>
        /// The ID of the hyperlink that we want to populate with async data
        /// </summary>
        public string ControlId
        {
            get;
            set;
        }

        /// <summary>
        /// The name of the datafield we want to use to populate this control
        /// </summary>
        public string DataField
        {
            get;
            set;
        }

        /// <summary>
        /// The format string that is applied to the intValue property when creating the hyperlink URL
        /// </summary>
        public string UrlFormatString
        {
            get;
            set;
        }
    }
}
