using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.Design;
 using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Inflectra.SpiraTest.Web.ServerControls.Authorization;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.ServerControls
{
	/// <summary>
	/// This class extends the default drop-down list to include an additional
	/// entry that can be used either for no-value or for telling the user to select a value
    /// and also uses a custom ajax control instead of the default HTML select control
	/// </summary>
	[ToolboxData("<{0}:DropDownListEx runat=server></{0}:DropDownListEx>")]
    public class DropDownListEx : DropDownList, IAuthorizedControl, IScriptControl
	{
		protected bool noValueItem = false;
		protected string noValueItemText = "";
		protected string metaData = "";
        private string rawSelectedValues = "";
        protected const string AttributeKey_Active = "Active";

		protected AuthorizedControlBase authorizedControlBase;
        protected Dictionary<string, string> handlers;

		/// <summary>
		/// Constructor - delegates to base class
		/// </summary>
		public DropDownListEx() : base()
		{
			//Instantiate the authorized control default implementation
			authorizedControlBase = new AuthorizedControlBase(this.ViewState);
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
        /// Flag to determine if we want to display the dropdown list entries the same on mobile as on desktop devices
        /// </summary>
        [
        Bindable(true),
        Category("Behavior"),
        Description("Flag to determine if we want to display the dropdown list entries the same on mobile as on desktop devices"),
        DefaultValue(false),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public bool DisplayMobileAsDesktop
        {
            get
            {
                return (this.displayMobileAsDesktop);
            }
            set
            {
                this.displayMobileAsDesktop = value;
            }
        }
        protected bool displayMobileAsDesktop = false;

        /// <summary>
        /// Flag to determine if we want to display the dropdown list entries as links
        /// </summary>
        [
        Bindable(true),
        Category("Behavior"),
        Description("Flag to determine if we want to display the dropdown list entries as links"),
        DefaultValue(false),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public bool DisplayAsHyperLinks
        {
            get
            {
                return (this.displayAsHyperLinks);
            }
            set
            {
                this.displayAsHyperLinks = value;
            }
        }
        protected bool displayAsHyperLinks = false;

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

		/// <summary>
		/// Event called before rendering control - extended to include a null initial item if requested
		/// </summary>
		/// <param name="e"></param>
		protected override void OnDataBinding(EventArgs e)
		{
			//Execute the base class first
			try
			{
				base.OnDataBinding (e);
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
					this.Items.Insert (0, item);
				}
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

        /// <summary>
        /// The base URL used when the dropdown items are displayed as hyperlinks
        /// </summary>
        [
        Bindable(true),
        Category("Misc"),
        Description("The base URL used when the dropdown items are displayed as hyperlinks"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public string HyperlinkBaseUrl
        {
            get
            {
                if (ViewState["HyperlinkBaseUrl"] == null)
                {
                    return "";
                }
                else
                {
                    return ((string)ViewState["HyperlinkBaseUrl"]);
                }
            }
            set
            {
                ViewState["HyperlinkBaseUrl"] = value;
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
                script = "function " + this.ClientID + "_selectedItemChanged" + "(args) { " + codeToExecute + " }\n";
                if (!clientScriptManager.IsClientScriptBlockRegistered(this.GetType(), this.UniqueID + "_selectedItemChanged"))
                {
                    clientScriptManager.RegisterClientScriptBlock(this.GetType(), this.UniqueID + "_selectedItemChanged", script, true);
                }
            }

            //Finally, execute the base class
            base.OnPreRender(e);
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
            
            //If not enabled, just render the text inside the div
            writer.WriteEndTag("div");

            ScriptManager.GetCurrent(this.Page).RegisterScriptDescriptors(this);
        }

        /// <summary>
        /// Databinds the control to the datasource
        /// </summary>
        /// <remarks>Extends the base class to capture the active flag from the datasource</remarks>
        public override void DataBind()
        {
            //First call the base method
            base.DataBind();

            //Resolve the different types of datasource into an enumerable one
            IEnumerable enumerableDataSource = GetResolvedDataSource(this.DataSource, this.DataMember);

            //If we have a passed in indent-level field name then get the indent levels
            if (enumerableDataSource != null)
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
                            if (!String.IsNullOrEmpty(this.ActiveItemField) && DataBinder.Eval(dataItem, this.ActiveItemField) != null)
                            {
                                object fieldValue = DataBinder.Eval(dataItem, this.ActiveItemField);
                                //Handle both Y/N legacy CHAR(1) fields and True/False BIT fields
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

        /// <summary>Updates the viewstate of the control with data returned from the client control</summary>
        protected override bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            this.rawSelectedValues = "";
            if (!base.IsEnabled)
            {
                return false;
            }

            string[] values = postCollection.GetValues(postDataKey);
            bool flag = false;
            this.EnsureDataBound();
            if (values == null)
            {
                if (this.SelectedIndex != -1)
                {
                    base.SetPostDataSelection(-1);
                    flag = true;
                }
                return flag;
            }

            //The custom client drop-down list returns a single value containing a comma-separated list
            if (values.Length < 1)
            {
                if (this.SelectedIndex != -1)
                {
                    base.SetPostDataSelection(-1);
                    flag = true;
                }
                return flag;
            }
            else
            {
                this.rawSelectedValues = values[0];
            }

            //Get the list of values
            string[] multiValues = values[0].Split(',');
            if (multiValues.Length < 1)
            {
                if (this.SelectedIndex != -1)
                {
                    base.SetPostDataSelection(-1);
                    flag = true;
                }
                return flag;
            }

            int selectedIndex = FindByValueInternal(multiValues[0], false);
            if (this.SelectedIndex != selectedIndex)
            {
                base.SetPostDataSelection(selectedIndex);
                flag = true;
            }
            return flag;
        }

        private int FindByValueInternal(string value, bool includeDisabled)
        {
            int num = 0;
            foreach (ListItem item in this.Items)
            {
                if (item.Value.Equals(value) && (includeDisabled || item.Enabled))
                {
                    return num;
                }
                num++;
            }
            return -1;
        }

        /// <summary>Gets the selected value.</summary>
        public string RawSelectedValue
        {
            get
            {
                return this.rawSelectedValues;
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
            ScriptControlDescriptor desc = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.DropDownList", this.ClientID);

            //Populate the datasource that we set on the client - need to explicitly serialize it
            JsonDictionaryOfStrings dataSource = new JsonDictionaryOfStrings();

            //Iterate through the list items and add to client datasource
            for (int i = 0; i < this.Items.Count; i++)
            {
                //Concatenate all the display info, active flag and the id
                //The client code knows to handle missing active flag safely
                string itemValue;
                if (String.IsNullOrEmpty(this.Items[i].Attributes[AttributeKey_Active]))
                {
                    itemValue = this.Items[i].Value;
                }
                else
                {
                    string activeYn = this.Items[i].Attributes[AttributeKey_Active];
                    itemValue = this.Items[i].Value + "_" + activeYn;
                }
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
            //Only single-selectable since server control based on standard DropDownList
            desc.AddProperty("multiSelectable", false);
            if (this.displayAsHyperLinks)
            {
                desc.AddProperty("displayAsHyperLinks", true);
                desc.AddProperty("hyperlinkBaseUrl", UrlRewriterModule.ResolveUrl(HyperlinkBaseUrl));
            }

            //set the bool value for whether to keep the ui the same across all devices - mobile and desktop
            if (DisplayMobileAsDesktop)
            {
                desc.AddProperty("displayMobileAsDesktop", true);
            }

            desc.AddScriptProperty("dataSource", JsonDictionaryConvertor.Serialize(dataSource));
            if (this.ListWidth == Unit.Empty)
            {
                if (this.Width != Unit.Empty)
                {
                    desc.AddProperty("listWidth", this.Width.ToString());
                }
            }
            else
            {
                desc.AddProperty("listWidth", this.ListWidth.ToString());
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
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DropDownList.js"));
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

    public class UnityDropDownListEx : DropDownListEx
    {
    }
}
