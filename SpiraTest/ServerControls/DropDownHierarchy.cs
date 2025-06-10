using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using System.Drawing.Design;

using Inflectra.SpiraTest.Web.ServerControls.Authorization;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// This class extends the default drop-down list to display using a custom client
    /// component that better handles hierarchical data and the case of disabled values
    /// that you want displayed as read-only, but still displayed
    /// </summary>
    [ToolboxData("<{0}:DropDownHierarchy runat=server></{0}:DropDownHierarchy>")]
    public class DropDownHierarchy : DropDownList, IAuthorizedControl, IScriptControl
    {
        protected bool noValueItem = false;
        protected string noValueItemText = "";
        protected string metaData = "";
        protected AuthorizedControlBase authorizedControlBase;
        protected string indentLevelField;
        public const string AttributeKey_IndentLevel = "IndentLevel";
        public const string AttributeKey_Summary = "Summary";
        public const string AttributeKey_Alternate = "Alternate";
        public const string AttributeKey_Active = "Active";
        protected Dictionary<string, string> handlers;
        private string rawSelectedValue = "";


        //Viewstate keys
        protected const string ViewStateKey_MetaData_Base = "MetaData_";

        /// <summary>
        /// Constructor - delegates to base class
        /// </summary>
        public DropDownHierarchy()
            : base()
        {
            //Instantiate the authorized control default implementation
            authorizedControlBase = new AuthorizedControlBase(this.ViewState);
        }

        /// <summary>Gets the selected value.</summary>
        public string RawSelectedValue
        {
            get
            {
                return this.rawSelectedValue;
            }
        }


        /// <summary>
        /// Flag to determine if we want to display an initial entry for nullable fields
        /// </summary>
        [
        Bindable(true),
        Category("Data"),
        Description("Flag to determine if we want to display an initial entry for nullable fields"),
        DefaultValue(false),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public bool NoValueItem
        {
            get
            {
                return (this.noValueItem);
            }
            set
            {
                this.noValueItem = value;
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

        [
        Category("Behavior"),
        DefaultValue(""),
        Description("A parameter we want to pass to the client-side script (optional)")
        ]
        public string ClientScriptParameter
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["clientScriptParameter"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["clientScriptParameter"] = value;
            }
        }

        /// <summary>
        /// Contains the display name of the no-value item if displayed
        /// </summary>
        [
        Bindable(true),
        Category("Data"),
        Description("Contains the display name of the no-value item if displayed"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public string NoValueItemText
        {
            get
            {
                return (this.noValueItemText);
            }
            set
            {
                this.noValueItemText = value;
            }
        }

        [
        Category("Appearance"),
        DefaultValue(""),
        Description("The css class to use for a disabled drop-down list")
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

		[
	   Category("Appearance"),
	   DefaultValue(""),
	   Description("The tooltip text for the navigate button")
	   ]
		public string NavigateToText
		{
			[DebuggerStepThrough()]
			get
			{
				object obj = ViewState["NavigateToText"];

				return (obj == null) ? string.Empty : (string)obj;
			}
			[DebuggerStepThrough()]
			set
			{
				ViewState["NavigateToText"] = value;
			}
		}

		/// <summary>
		/// Resolves all datasources into an IEnumerable type
		/// </summary>
		/// <param name="source">The unresolved datasource</param>
		/// <param name="member">The data member (if any)</param>
		/// <returns>The resolved IEnumerable data source</returns>
		protected IEnumerable GetResolvedDataSource(object source, string member)
        {
            if (source != null && source is IListSource)
            {
                IListSource src = (IListSource)source;
                IList list = src.GetList();
                if (!src.ContainsListCollection)
                {
                    return list;
                }
                if (list != null && list is ITypedList)
                {

                    ITypedList tlist = (ITypedList)list;
                    PropertyDescriptorCollection pdc = tlist.GetItemProperties(new PropertyDescriptor[0]);
                    if (pdc != null && pdc.Count > 0)
                    {
                        PropertyDescriptor pd = null;
                        if (member != null && member.Length > 0)
                        {
                            pd = pdc.Find(member, true);
                        }
                        else
                        {
                            pd = pdc[0];
                        }
                        if (pd != null)
                        {
                            object rv = pd.GetValue(list[0]);
                            if (rv != null && rv is IEnumerable)
                            {
                                return (IEnumerable)rv;
                            }
                        }
                        throw new Exception("ListSource_Missing_DataMember");
                    }
                    throw new Exception("ListSource_Without_DataMembers");
                }
            }
            if (source is IEnumerable)
            {
                return (IEnumerable)source;
            }
            return null;
        }

        /// <summary>
        /// Overrides the rendering of the built-in select control to just display a div
        /// as the client component does the rest of the rendering for us
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            writer.WriteBeginTag("div");
            writer.WriteAttribute("id", this.ClientID);
            writer.WriteAttribute("class", this.CssClass);
            writer.Write(HtmlTextWriter.TagRightChar);
            writer.WriteEndTag("div");
            ScriptManager.GetCurrent(this.Page).RegisterScriptDescriptors(this);
        }

        /// <summary>
        /// Databinds the control to the datasource
        /// </summary>
        /// <remarks>Extends the base class to capture the indent level from the datasource</remarks>
        public override void DataBind()
        {
            //First call the base method
            base.DataBind();

            //Resolve the different types of datasource into an enumerable one
            IEnumerable enumerableDataSource = GetResolvedDataSource(this.DataSource, this.DataMember);

            //If we have a passed in indent-level field name then get the indent levels
            if (this.IndentLevelField != null && this.IndentLevelField != "" && enumerableDataSource != null)
            {
                //Iterate through the datasource and get the indent level and add to the items collection as an attribute
                IEnumerator e = enumerableDataSource.GetEnumerator();
                if (e != null)
                {
                    int i = 0;
                    //Account for the initial 'no-value' item
                    if (this.NoValueItem)
                    {
                       i++;
                    }
                    while (e.MoveNext())
                    {
                        //First handle the various extended attributes
                        if (e != null)
                        {
                            object dataItem = e.Current;
                            if (DataBinder.Eval(dataItem, this.IndentLevelField) != null)
                            {
                                string indentLevel = (string)DataBinder.Eval(dataItem, this.IndentLevelField);
                                this.Items[i].Attributes[AttributeKey_IndentLevel] = indentLevel;
                            }
                            if (!String.IsNullOrEmpty(this.SummaryItemField) && DataBinder.Eval(dataItem, this.SummaryItemField) != null)
                            {
                                object fieldValue = DataBinder.Eval(dataItem, this.SummaryItemField);
                                if (fieldValue is String)
                                {
                                    string summaryYn = (string)fieldValue;
                                    this.Items[i].Attributes[AttributeKey_Summary] = summaryYn;
                                }
                                if (fieldValue is Boolean)
                                {
                                    bool isSummary = (bool)fieldValue;
                                    this.Items[i].Attributes[AttributeKey_Summary] = (isSummary) ? "Y" : "N";
                                }
                            }
                            if (!String.IsNullOrEmpty(this.AlternateItemField) && DataBinder.Eval(dataItem, this.AlternateItemField) != null)
                            {
                                object fieldValue = DataBinder.Eval(dataItem, this.AlternateItemField);
                                if (fieldValue is String)
                                {
                                    string alternateYn = (string)fieldValue;
                                    this.Items[i].Attributes[AttributeKey_Alternate] = alternateYn;
                                }
                                if (fieldValue is Boolean)
                                {
                                    bool isAlternate = (bool)fieldValue;
                                    this.Items[i].Attributes[AttributeKey_Alternate] = (isAlternate) ? "Y" : "N";
                                }
                            }
                            if (!String.IsNullOrEmpty(this.ActiveItemField) && DataBinder.Eval(dataItem, this.ActiveItemField) != null)
                            {
                                object fieldValue = DataBinder.Eval(dataItem, this.ActiveItemField);
                                if (fieldValue is String)
                                {
                                    string activeYn = (string)fieldValue;
                                    this.Items[i].Attributes[AttributeKey_Active] = activeYn;
                                }
                                if (fieldValue is Boolean)
                                {
                                    bool isActive = (bool)fieldValue;
                                    this.Items[i].Attributes[AttributeKey_Active] = (isActive) ? "Y" : "N";
                                }
                            }
                            i++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the loading of viewstate when we're persisting the attributes of the DropDownList
        /// </summary>
        protected override void LoadViewState(object savedState)
        {
            if (savedState == null) return;

            // see if savedState is an object or object array
            if (savedState is object[])
            {
                // we have an array of items with attributes
                object[] state = (object[])savedState;
                base.LoadViewState(state[0]);   // load the base state

                for (int i = 1; i < state.Length; i++)
                {
                    if (state[i] != null)
                    {
                        // Load back in the attributes
                        object[] attribKV = (object[])state[i];
                        for (int k = 0; k < attribKV.Length; k += 2)
                        {
                            this.Items[i - 1].Attributes.Add(attribKV[k].ToString(),
                                attribKV[k + 1].ToString());
                        }
                    }
                }
            }
            else
            {
                // we have just the base state
                base.LoadViewState(savedState);
            }
        }

        /// <summary>
        /// Captures the raw POSTed values when you want to get the select value before databind occurs on postback
        /// </summary>
        /// <param name="postDataKey"></param>
        /// <param name="postCollection"></param>
        /// <returns></returns>
        protected override bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            this.rawSelectedValue = "";
            if (!base.IsEnabled)
            {
                return false;
            }

            string[] values = postCollection.GetValues(postDataKey);
            if (values.Length > 0)
            {
                this.rawSelectedValue = values[0];
            }

            return base.LoadPostData(postDataKey, postCollection);
        }

        /// <summary>
        /// Overrides the standard view-state save to also save attributes view state
        /// </summary>
        protected override object SaveViewState()
        {
            // Create an object array with one element for the DropDownListEx's
            // ViewState contents, and one element for each ListItem in DropDownListEx
            object[] state = new object[this.Items.Count + 1];

            object baseState = base.SaveViewState();
            state[0] = baseState;

            // Now, see if we even need to save the view state
            bool itemHasAttributes = false;
            for (int i = 0; i < this.Items.Count; i++)
            {
                if (this.Items[i].Attributes.Count > 0)
                {
                    itemHasAttributes = true;

                    // Create an array of the item's Attribute's keys and values
                    object[] attribKV = new object[this.Items[i].Attributes.Count * 2];
                    int k = 0;
                    foreach (string key in this.Items[i].Attributes.Keys)
                    {
                        attribKV[k++] = key;
                        attribKV[k++] = this.Items[i].Attributes[key];
                    }

                    state[i + 1] = attribKV;
                }
            }

            // return either baseState or state, depending on whether or not
            // any ListItems had attributes
            if (itemHasAttributes)
            {
                return state;
            }
            else
            {
                return baseState;
            }
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

        /// <summary>
        /// Event called before rendering control - extended to include a null initial item if requested
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDataBinding(EventArgs e)
        {
            //Execute the base class first
            try
            {
                base.OnDataBinding(e);
            }
            catch (ArgumentOutOfRangeException)
            {
                //There is a bug in .NET FW 2.0 which sometimes causes this error to be thrown
            }

            //Check to see if we want to add an initial item corresponding to no value
            if (this.noValueItem)
            {
                //Check that there's not already a 'no value' item there (in case of postbacks)
                if (this.Items.Count == 0 || this.Items[0].Text != this.NoValueItemText)
                {
                    ListItem item = new ListItem(this.NoValueItemText, "");
                    this.Items.Insert(0, item);
                }
            }
        }

        /// <summary>
        /// Make sure that we have the right authorization or configuration to display/enable the control
        /// </summary>
        /// <param name="e">The Event Arguments</param>
        protected override void OnPreRender(EventArgs e)
        {
            ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);
            if (scriptManager == null)
            {
                throw new InvalidOperationException("ScriptManager required on the page.");
            }

            //Register the client component
            scriptManager.RegisterScriptControl(this);
            ClientScriptManager clientScriptManager = Page.ClientScript;

            string script;
            //Hook into the selectedItemChanged event if necessary
            if (this.AutoPostBack)
            {
                //Add the special postback client code to force a postback
                script = "function " + this.ClientID + "_selectedItemChanged" + "(args) { " + Page.ClientScript.GetPostBackEventReference(this, "") + "; }\n";
                if (!clientScriptManager.IsClientScriptBlockRegistered(this.GetType(), this.UniqueID + "_selectedItemChanged"))
                {
                    clientScriptManager.RegisterClientScriptBlock(this.GetType(), this.UniqueID + "_selectedItemChanged", script, true);
                }
            }
            else if (!string.IsNullOrEmpty(this.ClientScriptMethod))
            {
                //If we don't have a server control then we can just execute the method
                //Otherwise we need to access the actual class
                string codeToExecute = "";
                if (string.IsNullOrEmpty(this.ClientScriptServerControlId))
                {
                    //See if we have a parameter to add
                    if (string.IsNullOrEmpty(this.ClientScriptParameter))
                    {
                        codeToExecute = this.ClientScriptMethod + "(args)";
                    }
                    else
                    {
                        codeToExecute = this.ClientScriptMethod + "('" + this.ClientScriptParameter + "',args)";
                    }
                }
                else
                {
                    //First we need to get the server control
                    Control clientScriptControl = this.Parent.FindControl(this.ClientScriptServerControlId);
                    if (clientScriptControl != null)
                    {
                        string clientId = clientScriptControl.ClientID;
                        //See if we have a parameter to add
                        if (string.IsNullOrEmpty(this.ClientScriptParameter))
                        {
                            codeToExecute = "$find('" + clientId + "')." + this.ClientScriptMethod + "(args)";
                        }
                        else
                        {
                            codeToExecute = "$find('" + clientId + "')." + this.ClientScriptMethod + "('" + this.ClientScriptParameter + "',args)";
                        }
                    }
                }
                script = "function " + this.ClientID + "_selectedItemChanged" + "(args) { " + codeToExecute + " }";
                if (!clientScriptManager.IsClientScriptBlockRegistered(this.GetType(), this.UniqueID + "_selectedItemChanged"))
                {
                    clientScriptManager.RegisterClientScriptBlock(this.GetType(), this.UniqueID + "_selectedItemChanged", script, true);
                }
            }

            //Finally, execute the base class
            base.OnPreRender(e);
        }

        /// <summary>
        /// The field in the datamember that contains the indent level for the text-field.
        /// The indent level should be in the format AAAAAB, etc.
        /// </summary>
        [
        Bindable(true),
        Category("Data"),
        Description("The field in the datamember that contains the indent level for the text-field"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public string IndentLevelField
        {
            get
            {
                String s = (String)ViewState["IndentLevelField"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["IndentLevelField"] = value;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(""),
        Description("The base URL that should be used when displaying links to the details page for the artifact in question")
        ]
        public string BaseUrl
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["baseUrl"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["baseUrl"] = value;
            }
        }

        /// <summary>
        /// The field in the datamember that contains the flag (Y/N) denoting a summary item
        /// </summary>
        [
        Bindable(true),
        Category("Data"),
        Description("The field in the datamember that contains the flag (Y/N) denoting a summary item"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public string SummaryItemField
        {
            get
            {
                String s = (String)ViewState["SummaryItemField"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["SummaryItemField"] = value;
            }
        }

        /// <summary>
        /// The field in the datamember that contains the flag (Y/N) denoting an alternate item
        /// </summary>
        [
        Bindable(true),
        Category("Data"),
        Description("The field in the datamember that contains the flag (Y/N) denoting an alternate item"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public string AlternateItemField
        {
            get
            {
                String s = (String)ViewState["AlternateItemField"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["AlternateItemField"] = value;
            }
        }

        /// <summary>
        /// The field in the datamember that contains the flag (Y/N) denoting an active item
        /// </summary>
        [
        Bindable(true),
        Category("Data"),
        Description("The field in the datamember that contains the flag (Y/N) denoting an active item"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public string ActiveItemField
        {
            get
            {
                String s = (String)ViewState["ActiveItemField"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["ActiveItemField"] = value;
            }
        }

        /// <summary>
        /// The width of the drop-down part of the list
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        Description("The width of the drop-down part of the list"),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public Unit ListWidth
        {
            get
            {
                if (ViewState["ListWidth"] == null)
                {
                    return Unit.Empty;
                }
                else
                {
                    Unit u = (Unit)ViewState["ListWidth"];
                    return u;
                }
            }

            set
            {
                ViewState["ListWidth"] = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        [Editor(typeof(ImageUrlEditor), typeof(UITypeEditor))]
        [Description("Used to display a non-summary item")]
        public string ItemImage
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["itemImage"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["itemImage"] = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        [Editor(typeof(ImageUrlEditor), typeof(UITypeEditor))]
        [Description("Used to display a summary item in its normal (non-expanded) state")]
        public string SummaryItemImage
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["summaryItemImage"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["summaryItemImage"] = value;
            }
        }

        [Category("Appearance")]
        [DefaultValue("")]
        [Editor(typeof(ImageUrlEditor), typeof(UITypeEditor))]
        [Description("Used to display an alterate item image (e.g. test case with steps or iteration vs. release)")]
        public string AlternateItemImage
        {
            [DebuggerStepThrough()]
            get
            {
                object obj = ViewState["alternateItemImage"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            [DebuggerStepThrough()]
            set
            {
                ViewState["alternateItemImage"] = value;
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

        /* IScriptControl Members */

        /// <summary>
        /// Return the various attributes to set on the client component
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            //Write the $create command that actually instantiates the control
            ScriptControlDescriptor desc = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.DropDownHierarchy", this.ClientID);

            //Populate the datasource that we set on the client
            JsonDictionaryOfStrings dataSource = new JsonDictionaryOfStrings();
            
            //Iterate through the list items and add to client datasource
            for (int i = 0; i < this.Items.Count; i++)
            {
                //Concatenate all the display info and the id
                //convert indent length string into a number
                int indentLevel = 0;
                string summary = "";
                string alternate = "";
                string active = "Y";
                if (!String.IsNullOrEmpty(this.Items[i].Attributes[AttributeKey_IndentLevel]))
                {
                    indentLevel = this.Items[i].Attributes[AttributeKey_IndentLevel].Length / 3;
                }
                if (!String.IsNullOrEmpty(this.Items[i].Attributes[AttributeKey_Summary]))
                {
                    summary = this.Items[i].Attributes[AttributeKey_Summary];
                }
                if (!String.IsNullOrEmpty(this.Items[i].Attributes[AttributeKey_Alternate]))
                {
                    alternate = this.Items[i].Attributes[AttributeKey_Alternate];
                }
                if (!String.IsNullOrEmpty(this.Items[i].Attributes[AttributeKey_Active]))
                {
                    active = this.Items[i].Attributes[AttributeKey_Active];
                }
                string itemValue = this.Items[i].Value + "_" + indentLevel.ToString() + "_" + summary + "_" + alternate + "_" + active;
                if (!dataSource.ContainsKey(itemValue))
                {
                    dataSource.Add(itemValue, this.Items[i].Text);
                }
            }

            //Set the various attributes
            desc.AddProperty("name", this.UniqueID);
            desc.AddProperty("enabled", this.Enabled);
            desc.AddProperty("enabledCssClass", this.CssClass);
            desc.AddProperty("disabledCssClass", this.DisabledCssClass);
			desc.AddProperty("navigateToText", this.NavigateToText);
			desc.AddScriptProperty("dataSource", JsonDictionaryConvertor.Serialize(dataSource));
            if (this.ListWidth != Unit.Empty)
            {
                desc.AddProperty("listWidth", this.ListWidth.ToString());
            }

            //If theming is enabled, need to pass the theme folder so that images resolve correctly
            if (Page.EnableTheming && Page.Theme != "")
            {
                if (HttpContext.Current.Request.ApplicationPath == "/")
                {
                    desc.AddProperty("themeFolder", "/App_Themes/" + Page.Theme + "/");
                }
                else
                {
                    desc.AddProperty("themeFolder", HttpContext.Current.Request.ApplicationPath + "/App_Themes/" + Page.Theme + "/");
                }
			}

            //The various icon images
            if (!string.IsNullOrEmpty(ItemImage))
            {
                desc.AddProperty("itemImage", ItemImage);
            }
            if (!string.IsNullOrEmpty(SummaryItemImage))
            {
                desc.AddProperty("summaryItemImage", SummaryItemImage);
            }
            if (!string.IsNullOrEmpty(AlternateItemImage))
            {
                desc.AddProperty("alternateItemImage", AlternateItemImage);
            }

			if (!string.IsNullOrEmpty(BaseUrl))
			{
				desc.AddProperty("baseUrl", this.ResolveUrl(BaseUrl));
			}

            //Set the selected value
            desc.AddProperty("selectedItem", this.SelectedValue);

            //Hook into the selectedItemChanged event if necessary
            if (this.AutoPostBack)
            {
                //We need to use the event to trigger a postback
                desc.AddEvent("selectedItemChanged", this.ClientID + "_selectedItemChanged");
            }
            else if (!string.IsNullOrEmpty(this.ClientScriptMethod))
            {
                //we need to use the event to fire the custom client method
                desc.AddEvent("selectedItemChanged", this.ClientID + "_selectedItemChanged");
            }

            //Add any custom client-side handlers
            if (this.handlers != null)
            {
                foreach (KeyValuePair<string, string> handler in this.handlers)
                {
                    desc.AddEvent(handler.Key, handler.Value);
                }
            }

            yield return desc;
        }

        /// <summary>
        /// Return the references to the client script resource files
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DropDownHierarchy.js"));
        }
    }

    public class UnityDropDownHierarchy : DropDownHierarchy
    {
    }
}
