using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using System.ComponentModel;
using System.Collections;
using System.Text.RegularExpressions;

using Inflectra.SpiraTest.Web.ServerControls.Authorization;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.ServerControls
{
	/// <summary>
	/// This class extends the default list-box to allow double-clicking of items to raise custom
	/// events, and also to allow the display of indented hierarchical data
	/// </summary>
	[ToolboxData("<{0}:ListBoxEx runat=server></{0}:ListBoxEx>")]
	public class ListBoxEx : System.Web.UI.WebControls.ListBox, System.Web.UI.IPostBackEventHandler, IAuthorizedControl
	{
		protected AuthorizedControlBase authorizedControlBase;
		protected string dataIndentLevelField;
		protected string dataFolderField;
		protected string folderCssClass;
		protected string dataTooltipField;
		protected bool noValueItem = false;
		protected string noValueItemText = "";

		protected const string IndentLevelKey = "IndentLevel";
		protected const string FolderYnKey = "FolderYn";
		protected const string TooltipKey = "tooltip";

		static readonly object DoubleClickEvent = new object ();

		//Viewstate keys
		protected const string ViewStateKey_MetaData_Base = "MetaData_";

		/// <summary>
		/// Constructor - delegates to base class
		/// </summary>
		public ListBoxEx() : base()
		{
			//Instantiate the authorized control default implementation
			authorizedControlBase = new AuthorizedControlBase(this.ViewState);
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
		public string DataIndentLevelField
		{
			get
			{
				return (this.dataIndentLevelField);
			}
			set
			{
				this.dataIndentLevelField = value;
			}
		}

		/// <summary>
		/// The field in the datamember that contains the tooltip text
		/// </summary>
		[
		Bindable(true),
		Category("Data"),
		Description("The field in the datamember that contains the tooltip text"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public string DataTooltipField
		{
			get
			{
				return (this.dataTooltipField);
			}
			set
			{
				this.dataTooltipField = value;
			}
		}

		/// <summary>
		/// The field in the datamember that contains the folder flag ("Y" or "N")
		/// </summary>
		[
		Bindable(true),
		Category("Data"),
		Description("The field in the datamember that contains the folder flag ('Y' or 'N')"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public string DataFolderField
		{
			get
			{
				return (this.dataFolderField);
			}
			set
			{
				this.dataFolderField = value;
			}
		}

		/// <summary>
		/// The CSS class to apply to items marked as folder
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("The CSS class to apply to items marked as folder)"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public string FolderCssClass
		{
			get
			{
				return (this.folderCssClass);
			}
			set
			{
				this.folderCssClass = value;
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
		/// The event handler called when a double-click is performed on the list box
		/// </summary>
		[Category ("Action")]
		[Description ("Raised when a double-click event is raised on the list-box")]
		public event EventHandler DoubleClick 
		{
			add
			{
				Events.AddHandler (DoubleClickEvent, value);
			}
			remove
			{
				Events.RemoveHandler (DoubleClickEvent, value);
			}
		} 

		/// <summary>
		/// The protected delegate for handling double-click events
		/// </summary>
		/// <param name="e">The event arguments</param>
		protected virtual void OnDoubleClick (EventArgs e)
		{
			EventHandler eventHandler = (EventHandler) Events[DoubleClickEvent];
			if (eventHandler != null)
			{
				eventHandler (this, e);
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
					item.Selected = true;	//Select by default
					this.Items.Insert (0, item);
				}
			}
		}

		/// <summary>
		/// Overrides the standard view-state save to also save attributes view state
		/// </summary>
		protected override object SaveViewState()
		{
			// Create an object array with one element for the CheckBoxList's
			// ViewState contents, and one element for each ListItem in skmCheckBoxList
			object [] state = new object[this.Items.Count + 1];

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
					object [] attribKV = new object[this.Items[i].Attributes.Count * 2];
					int k = 0;
					foreach(string key in this.Items[i].Attributes.Keys)
					{
						attribKV[k++] = key;
						attribKV[k++] = this.Items[i].Attributes[key];
					}

					state[i+1] = attribKV;
				}
			}

			// return either baseState or state, depending on whether or not
			// any ListItems had attributes
			if (itemHasAttributes)
				return state;
			else
				return baseState;
		}

		/// <summary>
		/// Handles the loading of viewstate when we're persisting the attributes of the ListBox
		/// </summary>
		protected override void LoadViewState(object savedState)
		{
			if (savedState == null) return;

			// see if savedState is an object or object array
			if (savedState is object[])
			{
				// we have an array of items with attributes
				object [] state = (object[]) savedState;
				base.LoadViewState(state[0]);   // load the base state

				for (int i = 1; i < state.Length; i++)
				{
					if (state[i] != null)
					{
						// Load back in the attributes
						object [] attribKV = (object[]) state[i];
						for (int k = 0; k < attribKV.Length; k += 2)
							this.Items[i-1].Attributes.Add(attribKV[k].ToString(), 
								attribKV[k+1].ToString());
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
		/// Databinds the control to the datasource
		/// </summary>
		/// <remarks>Extends the base class to capture the indent level from the datasource</remarks>
		public override void DataBind()
		{
			//First call the base method
			base.DataBind ();

			//Resolve the different types of datasource into an enumerable one
			IEnumerable enumerableDataSource = GetResolvedDataSource (this.DataSource, this.DataMember);

			if (enumerableDataSource != null)
			{
				//If we have a passed in tooltip field name then get the tooltip text
				if (this.DataTooltipField != null && this.DataTooltipField != "")
				{
					//Iterate through the datasource and get the tooltip text and add to the items collection as an attribute
					IEnumerator e = enumerableDataSource.GetEnumerator();
					int i = 0;
					while (e.MoveNext())
					{
						//handle the tooltip text
						object dataItem = e.Current;
						string tooltipText = "";
						if (dataItem != null && DataBinder.Eval(dataItem, this.DataTooltipField) != null)
						{
							if (DataBinder.Eval(dataItem, this.DataTooltipField).GetType() == typeof(string))
							{
								tooltipText = (string) DataBinder.Eval(dataItem, this.DataTooltipField);
							}
						}
						this.Items[i].Attributes[TooltipKey] = tooltipText;
						i++;
					}
				}

				//If we have a passed in indent-level field name then get the indent levels
				if (this.DataIndentLevelField != null && this.DataIndentLevelField != "")
				{
					//Iterate through the datasource and get the indent level and add to the items collection as an attribute
					IEnumerator e = enumerableDataSource.GetEnumerator();
					int i = 0;
					while (e.MoveNext())
					{
						//First handle the indent level
						object dataItem = e.Current;
						string indentLevel = (string) DataBinder.Eval(dataItem, this.DataIndentLevelField);
						this.Items[i].Attributes[IndentLevelKey] = indentLevel;
						
						//Now handle the folder flag
						if (this.DataFolderField != null && this.DataFolderField != "")
						{
							string folderYn = (string) DataBinder.Eval(dataItem, this.DataFolderField);
							this.Items[i].Attributes[FolderYnKey] = folderYn;
						}
						i++;
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
		protected IEnumerable GetResolvedDataSource (object source, string member) 
		{
			if (source != null && source is IListSource) 
			{
				IListSource src = (IListSource) source;
				IList list = src.GetList ();
				if (!src.ContainsListCollection) 
				{
					return list;
				}
				if (list != null && list is ITypedList) 
				{

					ITypedList tlist = (ITypedList) list;
					PropertyDescriptorCollection pdc = tlist.GetItemProperties (new PropertyDescriptor[0]);
					if (pdc != null && pdc.Count > 0) 
					{
						PropertyDescriptor pd = null;
						if (member != null && member.Length > 0) 
						{
							pd = pdc.Find (member, true);
						} 
						else 
						{
							pd = pdc[0];
						}
						if (pd != null) 
						{
							object rv = pd.GetValue (list[0]);
							if (rv != null && rv is IEnumerable) 
							{
								return (IEnumerable)rv;
							}
						}
						throw new Exception ("ListSource_Missing_DataMember");
					}
					throw new Exception ("ListSource_Without_DataMembers");
				}
			}
			if (source is IEnumerable) 
			{
				return (IEnumerable)source;
			}
			return null;
		}

		/// <summary>
		/// Overrides the rendering of the SELECT control attributes to add the double-click event handler
		/// </summary>
		/// <param name="writer">The HTML output stream</param>
		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			//First add the standard attributes by calling base class
			base.AddAttributesToRender (writer);

			//Now add the double-click event handler
			writer.AddAttribute ("ondblclick", Page.ClientScript.GetPostBackEventReference(this, ""));

			//Handle height and width for non-ie browsers
			if (this.Context.Request.Browser.Browser.ToLower(System.Globalization.CultureInfo.InvariantCulture) != "ie")
			{
				//Can't use the AddStyleAttribute because the sub-control seems to
				//strip it out for non-IE browsers!
				writer.AddAttribute("style", "width:" + this.Width.ToString() + ";height:" + this.Height.ToString());
			}

			//Add the code to display a tooltip (used for items that get cut-off)
			//This is only used by Internet Explorer & Opera, since Firefox allows mouse-overs on option items
			if (this.DataTooltipField != null && this.DataTooltipField != "" && (this.Context.Request.Browser.Browser.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "ie" || this.Context.Request.Browser.Browser.ToLower(System.Globalization.CultureInfo.InvariantCulture) == "opera"))
			{
				writer.AddAttribute ("onChange", "ddrivetip(this.options[selectedIndex].getAttribute('" + TooltipKey + "').replace(/-+/,''));");
				writer.AddAttribute ("onMouseOut", "hideddrivetip();");
			}
		}

		/// <summary>
		/// Loads the script library used by the custom DHTML tooltips
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender (e);

            this.Page.ClientScript.RegisterClientScriptResource(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.Tooltip.js");

			//Check to see if we are authorized, if not then disable the button
            if (Context != null && this.Enabled)
            {
                int currentRoleId = authorizedControlBase.ProjectRoleId;
                bool isSystemAdmin = authorizedControlBase.UserIsAdmin;
                bool isGroupAdmin = authorizedControlBase.UserIsGroupAdmin;
                this.Enabled = (authorizedControlBase.IsAuthorized(currentRoleId, isSystemAdmin, isGroupAdmin) == Project.AuthorizationState.Authorized);
            }
		}

		/// <summary>
		/// Overrides the rendering of the individual items to include the indent level
		/// </summary>
		/// <param name="writer">The HTML output stream</param>
		protected override void RenderContents(HtmlTextWriter writer)
		{
			//Iterate through the list items and render
			for (int i = 0; i < this.Items.Count; i++)
			{
				//Open the tag
				writer.WriteBeginTag ("OPTION");

				//Write the attributes
				writer.WriteAttribute ("VALUE", this.Items[i].Value);
				//Handle the case if selected
				if (this.Items[i].Selected)
				{
					writer.WriteAttribute ("SELECTED", "1");
				}

				//Apply the appropriate style depending on if a folder or not
				if (this.DataFolderField != null && this.DataFolderField != "")
				{
					string folderYn = this.Items[i].Attributes[FolderYnKey];
					if (folderYn == "Y")
					{
						writer.WriteAttribute ("CLASS", this.FolderCssClass);
					}
				}

				//Add the code to display a tooltip (used for items that get cut-off)
				//This is only used by non-IE browsers and non-Opera
				if (this.DataTooltipField != null && this.DataTooltipField != "" && this.Context.Request.Browser.Browser.ToLower(System.Globalization.CultureInfo.InvariantCulture) != "ie" && this.Context.Request.Browser.Browser.ToLower(System.Globalization.CultureInfo.InvariantCulture) != "opera")
				{
					writer.WriteAttribute ("onMouseOver", "ddrivetip(this.getAttribute('" + TooltipKey + "').replace(/-+/,''));");
					writer.WriteAttribute ("onMouseOut", "hideddrivetip();");
				}

				//Write out any attributes stored on the items
				this.Items[i].Attributes.Render(writer);

				//Now render the caption as the inner text, handling the indents
				writer.Write (System.Web.UI.HtmlTextWriter.TagRightChar);
				if (this.DataIndentLevelField == null || this.DataIndentLevelField == "")
				{
					//Simply write out the text if we have no indent
					writer.Write(this.Items[i].Text);
				}
				else
				{
					//Get the indent level for this item, and use it to add an indentation to the text
					string indentLevel = this.Items[i].Attributes[IndentLevelKey];
					if (indentLevel == null)
					{
						indentLevel = "";
					}
					else
					{
						indentLevel = Regex.Replace (indentLevel, @"\w", "-");
					}
					string caption = indentLevel + " " + this.Items[i].Text;
					writer.Write(caption);
				}
				//Close the tag
				writer.WriteEndTag ("OPTION");
				writer.Write (writer.NewLine);
			}
		}

		/// <summary>
		/// Implements the postback event raising handler, for detecting and responding to the double-click of the
		/// list box
		/// </summary>
		/// <param name="eventArgument">The event argument (not used for double-click)</param>
		public void RaisePostBackEvent(string eventArgument)
		{
			//Call the delegate for the event handler
			OnDoubleClick (new EventArgs());
		}	
	}
}
