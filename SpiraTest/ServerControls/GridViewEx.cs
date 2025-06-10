using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Inflectra.SpiraTest.Web.ServerControls
{
	[ToolboxData("<{0}:GridViewEx runat=server></{0}:GridViewEx>")]
	public class GridViewEx : GridView, IScriptControl
	{
		protected System.Web.UI.WebControls.TableItemStyle subHeaderStyle;
		protected bool showSubHeader;
		protected GridViewRow subHeaderRow;
		protected Hashtable filters;

		public const string SubHeaderRowId = "SubHeaderRow";

		/// <summary>
		/// Constructor - delegates to the base class
		/// </summary>
		public GridViewEx()
			: base()
		{
		}

		#region Properties

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

		/// <summary>
		/// Gets and sets the current sort command (columnname + Direction, e.g. IncidentId ASC)
		/// </summary>
		[
		Bindable(true),
		Category("Data"),
		Description("Gets and sets a hashtables of filters to be applied to the gridview (if appropriate)"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public new string SortExpression
		{
			get;
			set;
		}

		/// <summary>
		/// Gets and sets a hashtables of filters to be applied to the gridview (if appropriate)
		/// </summary>
		[
		Bindable(true),
		Category("Data"),
		Description("Gets and sets a hashtables of filters to be applied to the gridview (if appropriate)"),
		DefaultValue(null),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
		NotifyParentProperty(true),
		PersistenceMode(PersistenceMode.InnerProperty)
		]
		public Hashtable Filters
		{
			get
			{
				return this.filters;
			}
			set
			{
				this.filters = value;
			}
		}

		/// <summary>
		/// Defines the HTML template for the subheader of the gridviewex
		/// </summary>
		[
		Bindable(true),
		Category("Style"),
		Description("The style to be applied to sub-header items."),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
		NotifyParentProperty(true),
		PersistenceMode(PersistenceMode.InnerProperty)
		]
		public virtual System.Web.UI.WebControls.TableItemStyle SubHeaderStyle
		{
			get
			{
				if (this.subHeaderStyle == null)
				{
					this.subHeaderStyle = new TableItemStyle();
				}
				return this.subHeaderStyle;
			}
		}

		/// <summary>
		/// Gets or sets a value that indicates whether the subheader is displayed in the GridViewEx control
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("Gets or sets a value that indicates whether the subheader is displayed in the GridViewEx control"),
		DefaultValue(false),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public virtual bool ShowSubHeader
		{
			get
			{
				return this.showSubHeader;
			}
			set
			{
				this.showSubHeader = value;
			}
		}

		/// <summary>
		/// Returns true if a custom count is provided to support custom paging
		/// </summary>
		protected bool CustomPaging
		{
			get
			{
				return (VirtualItemCount != -1);
			}
		}

		/// <summary>
		/// Gets or sets a value that indicates the real item count when custom paging is used
		/// </summary>
		[
		Bindable(true),
		Category("Data"),
		Description("Gets or sets a value that indicates the real item count when custom paging is used"),
		DefaultValue(-1),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public virtual int VirtualItemCount
		{
			get
			{
				if (ViewState["virtualItemCount"] == null)
				{
					ViewState["virtualItemCount"] = -1;
				}
				return (int)ViewState["virtualItemCount"];
			}
			set
			{
				ViewState["virtualItemCount"] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value that indicates the current page index when used in custom paging
		/// </summary>
		[
		Bindable(true),
		Category("Data"),
		Description("Gets or sets a value that indicates the current page index when used in custom paging"),
		DefaultValue(0),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public int CurrentPageIndex
		{
			get
			{
				if (ViewState["currentPageIndex"] == null)
				{
					ViewState["currentPageIndex"] = 0;
				}
				return Convert.ToInt32(ViewState["currentPageIndex"]);
			}
			set
			{
				ViewState["currentPageIndex"] = value;
			}
		}

		/// <summary>
		/// Get or Set Text to display in empty data row
		/// </summary>
		[
		Description("Text to display in empty data row"),
		Category("Misc"),
		DefaultValue(""),
		]
		public string EmptyTableRowText
		{
			get
			{
				object o = ViewState["EmptyTableRowText"];
				return (o != null ? o.ToString() : "");
			}
			set
			{
				ViewState["EmptyTableRowText"] = value;
			}
		}

		#endregion

		#region IScriptControl Interface

		public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
		{
			ScriptControlDescriptor desc = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.GridView", ClientID);
			if (!string.IsNullOrEmpty(WebServiceClass))
			{
				desc.AddScriptProperty("webServiceClass", WebServiceClass);
			}
			yield return desc;
		}

		public IEnumerable<ScriptReference> GetScriptReferences()
		{
			yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.GridView.js"));
		}

		#endregion

		#region Methods and Event Handlers

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			//Always make the pager visible if custom paging enabled
			if (this.CustomPaging)
			{
				GridViewRow pagerRow = (GridViewRow)this.BottomPagerRow;
				if (pagerRow != null && pagerRow.Visible == false)
					pagerRow.Visible = true;
			}

			//--ARIA Attributes --
			//role=grid
			this.Attributes.Add("role", "grid");

			ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);

			if (scriptManager == null)
			{
				throw new InvalidOperationException("ScriptManager required on the page.");
			}

			scriptManager.RegisterScriptControl(this);

			//Display the code to handle persistent tooltips
			this.Page.ClientScript.RegisterClientScriptResource(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.Tooltip.js");
		}

		/// <summary>
		/// Renders out the script descriptors
		/// </summary>
		/// <param name="writer"></param>
		protected override void Render(HtmlTextWriter writer)
		{
			base.Render(writer);
			if (!DesignMode && this.Visible)
			{
				ScriptManager.GetCurrent(this.Page).RegisterScriptDescriptors(this);
			}
		}

		/// <summary>
		/// Override the CreateChildControls class to add the custom subheader
		/// </summary>
		/// <param name="dataSource">An System.Collections..::.IEnumerable that contains the data source for the GridView control</param>
		/// <param name="dataBinding">True to indicate that the child controls are bound to data; otherwise, false.</param>
		protected override int CreateChildControls(System.Collections.IEnumerable dataSource, bool dataBinding)
		{
			//First create the normal child controls for the gridview
			int numRows = 0;
			if (dataSource != null)
				numRows = base.CreateChildControls(dataSource, dataBinding);

			//no data rows created, create empty table if enabled
			if (numRows == 0 && !this.ShowHeaderWhenEmpty)
			{
				//create table with the right ID
				Table table = new Table();
				table.Attributes.Add("id", this.ClientID);

				//create a new header row
				GridViewRow row = base.CreateRow(-1, -1, DataControlRowType.Header, DataControlRowState.Normal);

				//convert the exisiting columns into an array and initialize
				DataControlField[] fields = new DataControlField[this.Columns.Count];
				this.Columns.CopyTo(fields, 0);
				this.InitializeRow(row, fields);
				table.Rows.Add(row);

				//create the empty row
				row = new GridViewRow(-1, -1, DataControlRowType.DataRow, DataControlRowState.Normal);
				TableCell cell = new TableCell();
				cell.ColumnSpan = this.Columns.Count;
				cell.Width = Unit.Percentage(100);
				cell.Controls.Add(new LiteralControl(EmptyTableRowText));
				row.Cells.Add(cell);
				table.Rows.Add(row);

				this.Controls.Add(table);
			}

			//Show the footer even if there are no rows
			if (numRows == 0 && ShowFooter)
			{
				if (Controls.Count > 0)
				{
					//Get all the grid's items(rows)
					Table table = (System.Web.UI.WebControls.Table)Controls[0];

					//create footer row 
					GridViewRow footerRow = base.CreateRow(-1, -1, DataControlRowType.Footer, DataControlRowState.Normal);

					//convert the exisiting columns into an array and initialize 
					DataControlField[] fields = new DataControlField[this.Columns.Count];
					this.Columns.CopyTo(fields, 0);

					this.InitializeRow(footerRow, fields);
					table.Rows.Add(footerRow);
				}
			}


			//Now add the additional subheading if the property set
			if (this.showSubHeader)
			{
				if (Controls.Count > 0)
				{
					//Get all the grid's items(rows)
					TableRowCollection rows = ((System.Web.UI.WebControls.Table)Controls[0]).Rows;

					//Now lets create the subheader row
					subHeaderRow = CreateRow(1, -1, DataControlRowType.Header, DataControlRowState.Normal);
					subHeaderRow.ID = SubHeaderRowId;
					//Override the styles with that of the specific sub-header (rather than header)
					subHeaderRow.ControlStyle.Reset();
					subHeaderRow.ControlStyle.CopyFrom(SubHeaderStyle);

					//Create an event attached to our new item
					GridViewRowEventArgs subHeaderRowArgs = new GridViewRowEventArgs(subHeaderRow);
					//Call RowCreated event
					OnRowCreated(subHeaderRowArgs);

					//Finally add the subheader to the rows collection
					rows.AddAt(1, subHeaderRow);

					//Increment the count of number rows created
					numRows++;
				}
			}

			//Return the number of rows created
			return numRows;
		}

		/// <summary>
		/// Updates the filters list from the values set by the user
		/// </summary>
		public void UpdateFilters()
		{
			//First we need to scan the list of columns
			string filterText;
			int filterId;
			string filterFlag;
			string filterProperty;

			//Clear the existing filters
			this.Filters.Clear();

			for (int i = 0; i < this.Columns.Count; i++)
			{
				//Reset filter variables
				filterText = "";    //For freetext
				filterId = -1;      //For drop-downs (other than true/false flags)
				filterFlag = "";    //For Y/N flags
				filterProperty = "";

				//First see if there is a dropdown value or textbox value in the filter in question
				//If so, get the value and property name
				TableCell cell = ((GridViewRow)this.Controls[0].Controls[1]).Cells[i];

				//Make sure this cell has a testbox, date-control or drop-down list
				foreach (Control control in cell.Controls)
				{
					if (control.GetType() == typeof(TextBoxEx) || control.GetType() == typeof(DateControl) || control.GetType() == typeof(DropDownListEx))
					{
						if (control.GetType() == typeof(Inflectra.SpiraTest.Web.ServerControls.TextBoxEx))
						{
							filterText = ((TextBoxEx)control).Text;
							filterProperty = ((TextBoxEx)control).MetaData;
						}
						if (control.GetType() == typeof(Inflectra.SpiraTest.Web.ServerControls.DateControl))
						{
							filterText = ((DateControl)control).Text;
							filterProperty = ((DateControl)control).MetaData;
						}
						if (control.GetType() == typeof(Inflectra.SpiraTest.Web.ServerControls.DropDownListEx))
						{
							string dropDownValue = ((DropDownListEx)control).SelectedItem.Value;
							//Convert into the ID of the row selected
							if (dropDownValue != "")
							{
								//See if we have the specific flag case
								if (dropDownValue.ToUpper() == "Y" || dropDownValue.ToUpper() == "N")
								{
									filterFlag = dropDownValue.ToUpper();
								}
								else
								{
									filterId = Int32.Parse(dropDownValue);
								}
								filterProperty = ((DropDownListEx)control).MetaData;
							}
						}
						if (filterText != "" || filterId != -1 || filterFlag != "")
						{
							//Now add the filters to the filters collection and then add to the viewstate
							if (filterText != "")
							{
								//Adding the freetext filter
								this.Filters.Add(filterProperty, filterText);
							}
							if (filterId != -1)
							{
								//Adding the lookup property filter
								this.Filters.Add(filterProperty, filterId);
							}
							if (filterFlag != "")
							{
								//Adding the flag property filter
								this.Filters.Add(filterProperty, filterFlag);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// When the datagrid loads/reloads need to make sure all column running counts reset
		/// </summary>
		/// <param name="e"></param>
		protected override void OnDataBinding(EventArgs e)
		{
			//First call the base method
			base.OnDataBinding(e);

			//Now we need to tell all the fields to restart counting
			foreach (DataControlField dcf in this.Columns)
			{
				if (dcf is IRunningTotalField)
				{
					IRunningTotalField runningTotalField = (IRunningTotalField)dcf;
					runningTotalField.RestartCount();
				}
			}
		}

		public override object DataSource
		{
			get
			{
				return base.DataSource;
			}
			set
			{
				base.DataSource = value;
				// we store the page index here so we dont lost it in databind
				CurrentPageIndex = PageIndex;
			}
		}

		/// <summary>
		/// Handles the setting of filters in the subheader if applicable
		/// </summary>
		/// <param name="e">The event arguments</param>
		/// 
		protected override void OnDataBound(EventArgs e)
		{
			base.OnDataBound(e);

			//Get the appropriate filter property names and controls from the datagrid
			for (int i = 0; i < this.Columns.Count; i++)
			{
				//See if we have any filters to deal with
				if (this.filters != null && this.filters.Count > 0)
				{
					//First we need to get the corresponding filter property name
					string filterProperty = "";
					TableCell cell = ((GridViewRow)this.Controls[0].Controls[1]).Cells[i];
					//Make sure this cell has a testbox, date-control or drop-down list
					foreach (Control control in cell.Controls)
					{
						if (control.GetType() == typeof(TextBoxEx) || control.GetType() == typeof(DateControl) || control.GetType() == typeof(DropDownListEx))
						{
							if (control.GetType() == typeof(Inflectra.SpiraTest.Web.ServerControls.TextBoxEx))
							{
								filterProperty = ((TextBoxEx)control).MetaData;
							}
							if (control.GetType() == typeof(Inflectra.SpiraTest.Web.ServerControls.DateControl))
							{
								filterProperty = ((DateControl)control).MetaData;
							}
							if (control.GetType() == typeof(Inflectra.SpiraTest.Web.ServerControls.DropDownListEx))
							{
								filterProperty = ((DropDownListEx)control).MetaData;
							}

							//Now see if we have a filter for that property in the hashtable
							object filterValue = this.filters[filterProperty];
							if (filterValue != null)
							{
								//Now we need to set the control value accordingly
								if (control.GetType() == typeof(Inflectra.SpiraTest.Web.ServerControls.TextBoxEx))
								{
									((TextBoxEx)control).Text = (string)filterValue;
								}
								if (control.GetType() == typeof(Inflectra.SpiraTest.Web.ServerControls.DateControl))
								{
									((DateControl)control).Text = (string)filterValue;
								}
								if (control.GetType() == typeof(Inflectra.SpiraTest.Web.ServerControls.DropDownListEx))
								{
									DropDownListEx dropDownList = ((DropDownListEx)control);
									//See if we have a flag or numeric lookup
									if (filterValue.GetType() == typeof(int))
									{
										try
										{
											dropDownList.SelectedValue = ((int)filterValue).ToString();
										}
										catch (Exception)
										{
											//Fail quietly - in case we have a stored filter that no longer exists
										}
									}
									if (filterValue.GetType() == typeof(string))
									{
										try
										{
											dropDownList.SelectedValue = ((string)filterValue);
										}
										catch (Exception)
										{
											//Fail quietly - in case we have a stored filter that no longer exists
										}
									}
								}
							}
						}
					}
				}

				//See if we have any filtersortfields that we need to set the sort for
				if (this.Columns[i].GetType() == typeof(FilterSortFieldEx))
				{
					FilterSortFieldEx filterSortField = (FilterSortFieldEx)this.Columns[i];
					//Now get a handle on the two imagebutton controls - header is row 0
					if (this.Controls.Count > 0 && this.Controls[0].Controls.Count > 0)
					{
						GridViewRow gridViewRow = (GridViewRow)this.Controls[0].Controls[0];
						if (i < gridViewRow.Cells.Count && gridViewRow.Cells[i] != null && filterSortField.Sortable && this.VirtualItemCount > 0)
						{
							ImageButtonEx sortAscending = (ImageButtonEx)(((GridViewRow)this.Controls[0].Controls[0]).Cells[i].Controls[1]);
							ImageButtonEx sortDescending = (ImageButtonEx)(((GridViewRow)this.Controls[0].Controls[0]).Cells[i].Controls[2]);

							//Now set the sorts appropriately
							if (this.SortExpression == filterSortField.DataField + " ASC")
							{
								sortAscending.Selected = true;
								sortDescending.Selected = false;
							}
							if (this.SortExpression == filterSortField.DataField + " DESC")
							{
								sortAscending.Selected = false;
								sortDescending.Selected = true;
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Displays the special pager that all gridviews in SpiraTest should use
		/// </summary>
		/// <param name="row">The gridview row used to display the pager</param>
		/// <param name="columnSpan">The number of columns that the pager needs to span</param>
		/// <param name="pagedDataSource">the data source providing the paged data</param>
		protected override void InitializePager(GridViewRow row, int columnSpan, PagedDataSource pagedDataSource)
		{
			//Handle the case of custom paging
			if (this.CustomPaging)
			{
				pagedDataSource.AllowCustomPaging = true;
				pagedDataSource.VirtualCount = VirtualItemCount;
				pagedDataSource.CurrentPageIndex = CurrentPageIndex;
				PageIndex = CurrentPageIndex;
			}
			int currentPageIndex = pagedDataSource.CurrentPageIndex;
			int pageCount = pagedDataSource.PageCount;
			int rowsPerPage = pagedDataSource.PageSize;

			//Create the new cell to hold the pager
			TableCell cell = new TableCell();
			cell.ColumnSpan = columnSpan;
			cell.HorizontalAlign = HorizontalAlign.Right;
			cell.CssClass = "priority1";
			row.Cells.Add(cell);

			//Now add the controls to the cell

			//The number of rows dropdown
			ManagerBase manager = new ManagerBase();
			SortedList<int, int> paginationOptions = manager.GetPaginationOptions();

			HtmlGenericControl div = new HtmlGenericControl();
			div.AddClass("pull-left hidden-xs");
			div.TagName = "div";
			cell.Controls.Add(div);
			div.Controls.Add(new LabelEx() { Text = Resources.ServerControls.GridViewEx_RowsPerPage, AssociatedControlID = "ddlNumberRows", AppendColon = true });

			DropDownListEx ddlNumberRows = new DropDownListEx();
			ddlNumberRows.ID = "ddlNumberRows";
			ddlNumberRows.SkinID = "NarrowPlusControl";
			ddlNumberRows.DataSource = paginationOptions;
			ddlNumberRows.DataTextField = "Value";
			ddlNumberRows.DataValueField = "Key";
			ddlNumberRows.AutoPostBack = true;
			div.Controls.Add(ddlNumberRows);
			if (rowsPerPage > 0)
			{
				ddlNumberRows.SelectedValue = rowsPerPage.ToString();
			}
			ddlNumberRows.SelectedIndexChanged += ddlNumberRows_SelectedIndexChanged;

			//First and previous buttons
			LinkButtonEx firstButton = new LinkButtonEx();
			firstButton.SkinID = "HyperLink";
			firstButton.ID = "btnFirst";
			HtmlGenericControl glyph = new HtmlGenericControl();
			glyph.TagName = "span";
			glyph.AddClass("fas fa-fast-backward");
			firstButton.Controls.Add(glyph);
			firstButton.CommandName = "Page";
			firstButton.CommandArgument = "First";
			cell.Controls.Add(firstButton);
			LinkButtonEx previousButton = new LinkButtonEx();
			previousButton.SkinID = "HyperLink";
			previousButton.ID = "btnPrevious";
			glyph = new HtmlGenericControl();
			glyph.TagName = "span";
			glyph.AddClass("fas fa-backward");
			previousButton.Controls.Add(glyph);
			previousButton.CommandName = "Page";
			previousButton.CommandArgument = "Prev";
			cell.Controls.Add(previousButton);

			//Disable the previous/first if on the first page
			if (currentPageIndex == 0)
			{
				firstButton.Enabled = false;
				previousButton.Enabled = false;
			}
			else
			{
				firstButton.Enabled = true;
				previousButton.Enabled = true;
			}

			//The page legends
			Literal literal1 = new Literal();
			literal1.Text = Resources.ServerControls.GridViewEx_DisplayingPage;
			cell.Controls.Add(literal1);

			TextBoxEx textBox = new TextBoxEx();
			textBox.CssClass = "text-box narrow";
			textBox.Attributes.Add("role", "narrow");
			textBox.ID = "txtCurrentPage";
			textBox.MaxLength = 10;
			textBox.Style.Add(HtmlTextWriterStyle.VerticalAlign, "middle");
			textBox.Text = (currentPageIndex + 1).ToString();
			cell.Controls.Add(textBox);

			LinkButtonEx refreshButton = new LinkButtonEx();
			refreshButton.SkinID = "HyperLink";
			refreshButton.ID = "btnRefresh";
			glyph = new HtmlGenericControl();
			glyph.TagName = "span";
			glyph.AddClass("fas fa-sync");
			refreshButton.Controls.Add(glyph);
			refreshButton.CommandName = "CustomPage";
			refreshButton.CommandArgument = "LoadPage";
			cell.Controls.Add(refreshButton);

			Literal literal2 = new Literal();
			literal2.Text = Resources.Main.Global_Of + " " + pageCount + " ";
			cell.Controls.Add(literal2);

			//The next and last buttons
			LinkButtonEx nextButton = new LinkButtonEx();
			nextButton.SkinID = "HyperLink";
			nextButton.ID = "btnNext";
			glyph = new HtmlGenericControl();
			glyph.TagName = "span";
			glyph.AddClass("fas fa-forward");
			nextButton.Controls.Add(glyph);
			nextButton.CommandName = "Page";
			nextButton.CommandArgument = "Next";
			cell.Controls.Add(nextButton);

			LinkButtonEx lastButton = new LinkButtonEx();
			lastButton.SkinID = "HyperLink";
			lastButton.ID = "btnLast";
			glyph = new HtmlGenericControl();
			glyph.TagName = "span";
			glyph.AddClass("fas fa-fast-forward");
			lastButton.Controls.Add(glyph);
			lastButton.CommandName = "Page";
			lastButton.CommandArgument = "Last";
			cell.Controls.Add(lastButton);

			//Disable the next/last if on the last page
			if (currentPageIndex >= pageCount - 1)
			{
				nextButton.Enabled = false;
				lastButton.Enabled = false;
			}
			else
			{
				nextButton.Enabled = true;
				lastButton.Enabled = true;
			}
		}

		void ddlNumberRows_SelectedIndexChanged(object sender, EventArgs e)
		{
			DropDownListEx ddlNumberRows = (DropDownListEx)sender;
			if (!String.IsNullOrEmpty(ddlNumberRows.SelectedValue))
			{
				this.PageSize = Int32.Parse(ddlNumberRows.SelectedValue);
				GridViewPageEventArgs args = new GridViewPageEventArgs(this.CurrentPageIndex);
				this.OnPageIndexChanging(args);
			}
		}

		protected override void OnRowCommand(GridViewCommandEventArgs e)
		{
			if (e.CommandName == "CustomPage" && e.CommandArgument != null && e.CommandArgument.ToString() == "LoadPage")
			{
				//If we have the case of the pager refresh button being clicked, need to convert the event into
				//one that sets the current page to the value entered in the textbox
				TextBoxEx textBox = (TextBoxEx)((Control)e.CommandSource).Parent.FindControl("txtCurrentPage");
				if (textBox != null)
				{
					int newPage = -1;
					if (Int32.TryParse(textBox.Text, out newPage))
					{
						if (newPage >= 1 && newPage <= this.PageCount)
						{
							GridViewPageEventArgs args = new GridViewPageEventArgs(newPage - 1);
							this.OnPageIndexChanging(args);
						}
					}
				}
			}
			else
			{
				//The default handler handles other cases
				base.OnRowCommand(e);
			}
		}

		/// <summary>
		/// Adds the primary key of the row as a client-side custom attribute 'tst:primaryKey'
		/// </summary>
		/// <param name="e"></param>
		protected override void OnRowDataBound(GridViewRowEventArgs e)
		{
			base.OnRowDataBound(e);

			if (e.Row.RowType == DataControlRowType.DataRow && this.DataKeyNames != null && this.DataKeyNames.Length > 0 && this.DataKeys[e.Row.RowIndex] != null)
			{
				DataKey dataKey = this.DataKeys[e.Row.RowIndex];
				if (dataKey.Value != null && dataKey.Value is Int32)
				{
					int primaryKey = (int)dataKey.Value;
					e.Row.Attributes["tst:primaryKey"] = primaryKey.ToString();
				}
			}
		}

		/// <summary>
		/// Handles the creation of our new custom subheader
		/// </summary>
		/// <param name="e">The arguments passed by the raising event</param>
		protected override void OnRowCreated(System.Web.UI.WebControls.GridViewRowEventArgs e)
		{
			//Delegate to inbuilt command
			base.OnRowCreated(e);

			//--ARIA Attributes --
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				//role=rowheader
				e.Row.Attributes.Add("role", "row");
			}
			if (e.Row.RowType == DataControlRowType.Footer)
			{
				//role=rowheader
				e.Row.Attributes.Add("role", "rowfooter");
			}

			if (e.Row.RowType == DataControlRowType.Header)
			{
				//--ARIA Attributes --
				//role=rowheader
				e.Row.Attributes.Add("role", "rowheader");

				if (e.Row.ID == GridViewEx.SubHeaderRowId)
				{
					//Loop through each column in the datagrid
					for (int i = 0; i < Columns.Count; i++)
					{
						//See if this column supports subheaders
						DataControlField field = Columns[i];
						if (field is ISubHeaderField)
						{
							//Create the new cell and render the appropriate column type
							DataControlFieldCell cell = new DataControlFieldCell(field);
							((ISubHeaderField)field).InitializeSubHeaderCell(this.ClientID, cell, DataControlRowState.Normal, e.Row.RowIndex);
							e.Row.Cells.Add(cell);
						}
					}
				}
			}
		}

		#endregion
	}

	/// <summary>
	/// This class extends the built-in ASP.NET TemplateField
	/// </summary>
	/// <remarks>It handles subheaders, header-spans and autosumming footer</remarks>
	[ToolboxData("<{0}:TemplateFieldEx runat=server></{0}:TemplateFieldEx>")]
	public class TemplateFieldEx : System.Web.UI.WebControls.TemplateField, ISubHeaderField, IRunningTotalField
	{
		protected System.Web.UI.ITemplate subHeaderTemplate;
		protected System.Web.UI.WebControls.TableItemStyle subHeaderStyle;
		protected int headerColumnSpan;
		protected int footerColumnSpan;
		protected int subHeaderColumnSpan;
		protected string footerField;
		protected string subHeaderText;

		protected int runningTotal = 0;

		/// <summary>
		/// Constructor - delegates to the base class
		/// </summary>
		public TemplateFieldEx()
			: base()
		{
		}

		/// <summary>
		/// Restarts the running total count
		/// </summary>
		public void RestartCount()
		{
			this.runningTotal = 0;
		}

		/// <summary>
		/// The number of item columns that the header should span. Passing a negative number hides the header cell.
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("The number of item columns that the header should span"),
		DefaultValue(0),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public int HeaderColumnSpan
		{
			get
			{
				return this.headerColumnSpan;
			}
			set
			{
				this.headerColumnSpan = value;
			}
		}

		/// <summary>
		/// The text that should be displayed in the sub-header - can used instead of a template
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("The text that should be displayed in the sub-header - can used instead of a template"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public string SubHeaderText
		{
			get
			{
				return this.subHeaderText;
			}
			set
			{
				this.subHeaderText = value;
			}
		}

		/// <summary>
		/// The number of item columns that the footer should span. Passing a negative number hides the footer cell.
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("The number of item columns that the footer should span"),
		DefaultValue(0),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public int FooterColumnSpan
		{
			get
			{
				return this.footerColumnSpan;
			}
			set
			{
				this.footerColumnSpan = value;
			}
		}

		/// <summary>
		/// The number of item columns that the sub-header should span. Passing a negative number hides the sub-header cell.
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("The number of item columns that the sub-header should span"),
		DefaultValue(0),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public int SubHeaderColumnSpan
		{
			get
			{
				return this.subHeaderColumnSpan;
			}
			set
			{
				this.subHeaderColumnSpan = value;
			}
		}

		/// <summary>
		/// The field in the data-source that should be used to generate the running total used in the footer
		/// </summary>
		[
		Bindable(true),
		Category("Data"),
		Description("The field in the data-source that should be used to generate the running total used in the footer"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public string FooterField
		{
			get
			{
				return this.footerField;
			}
			set
			{
				this.footerField = value;
			}
		}


		/// <summary>
		/// Defines the HTML template for the subheader column of the gridviewex
		/// </summary>
		[
		Bindable(true),
		Category("Style"),
		Description("The style to be applied to sub-header items."),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
		NotifyParentProperty(true),
		PersistenceMode(PersistenceMode.InnerProperty)
		]
		public virtual System.Web.UI.WebControls.TableItemStyle SubHeaderStyle
		{
			get
			{
				if (this.subHeaderStyle == null)
				{
					this.subHeaderStyle = new TableItemStyle();
				}
				return this.subHeaderStyle;
			}
		}

		/// <summary>
		/// Defines the HTML template for the subheader of the gridviewex
		/// </summary>
		[
		Browsable(false),
		TemplateContainer(typeof(DataGridItem)),
		PersistenceModeAttribute(PersistenceMode.InnerProperty)
		]
		public virtual System.Web.UI.ITemplate SubHeaderTemplate
		{
			get
			{
				return this.subHeaderTemplate;
			}
			set
			{
				this.subHeaderTemplate = value;
			}
		}

		/// <summary>
		/// Adds text or controls to a cell's controls collection.
		/// </summary>
		/// <param name="cell">A DataControlFieldCell that contains the text or controls of the DataControlField</param>
		/// <param name="cellType">One of the DataControlCellType values</param>
		/// <param name="rowState">One of the DataControlRowState values, specifying the state of the row that contains the DataControlFieldCell</param>
		/// <param name="rowIndex">The index of the row that the DataControlFieldCell is contained in</param>
		public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
		{
			//Delegate to the base implementation
			base.InitializeCell(cell, cellType, rowState, rowIndex);

			//Handle any header column spans
			if (cellType == DataControlCellType.Header && this.headerColumnSpan > 0)
			{
				cell.ColumnSpan = this.headerColumnSpan;
			}

			//Handle any footer column spans
			if (cellType == DataControlCellType.Footer && this.footerColumnSpan > 0)
			{
				cell.ColumnSpan = this.footerColumnSpan;
			}

			//If we have a negative header column span, then don't display the header
			if (cellType == DataControlCellType.Header && this.headerColumnSpan == -1)
			{
				cell.Visible = false;
			}

			//If we have a negative footer column span, then don't display the footer
			if (cellType == DataControlCellType.Footer && this.footerColumnSpan == -1)
			{
				cell.Visible = false;
			}

			//If we are displaying a footer, need to attach an event handler for calculating the sum
			if (this.FooterField != null && this.FooterField != "" && (cellType == DataControlCellType.DataCell || cellType == DataControlCellType.Footer))
			{
				cell.DataBinding += new EventHandler(cell_DataBinding);
			}
		}

		/// <summary>
		/// Adds text or controls to a subheader cell's controls collection.
		/// </summary>
		/// <param name="cell">A DataControlFieldCell that contains the text or controls of the DataControlField</param>
		/// <param name="rowState">One of the DataControlRowState values, specifying the state of the row that contains the DataControlFieldCell</param>
		/// <param name="rowIndex">The index of the row that the DataControlFieldCell is contained in</param>
		public void InitializeSubHeaderCell(string gridViewClientId, DataControlFieldCell cell, DataControlRowState rowState, int rowIndex)
		{
			//Since we have a subheader, handle it differently
			if (this.subHeaderTemplate == null)
			{
				//Simply use the subheader text if provided
				if (!String.IsNullOrWhiteSpace(this.subHeaderText))
				{
					cell.Text = this.subHeaderText;
				}
			}
			else
			{
				//Process the template to create the actual controls
				this.subHeaderTemplate.InstantiateIn(cell);
				cell.ControlStyle.CopyFrom(this.subHeaderStyle);
			}

			//Handle any sub-header column spans
			if (this.subHeaderColumnSpan > 0)
			{
				cell.ColumnSpan = this.subHeaderColumnSpan;
			}

			//If we have a negative sub-header column span, then don't display the sub-header
			if (this.subHeaderColumnSpan == -1)
			{
				cell.Visible = false;
			}
		}

		/// <summary>
		/// Called when the cell is databinding, used to calculate totals in the footer
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void cell_DataBinding(object sender, EventArgs e)
		{
			TableCell cell = (TableCell)sender;
			GridViewRow gridViewRow = (GridViewRow)cell.NamingContainer;

			//If we have an item, add to total
			if (gridViewRow.RowType == DataControlRowType.DataRow)
			{
				int dataValue = this.GetUnderlyingValue(gridViewRow.DataItem);
				runningTotal += dataValue;
			}

			//If we have a footer, bind total to column
			if (gridViewRow.RowType == DataControlRowType.Footer && runningTotal > 0)
			{
				cell.Text = runningTotal.ToString();
			}
		}

		/// <summary>
		/// Returns the underlying value for a specific bound data-item
		/// </summary>
		/// <param name="dataItem">The bound data-item</param>
		/// <returns>The integer representation of the value</returns>
		protected int GetUnderlyingValue(object dataItem)
		{
			//Handle NULLs correctly
			if (this.FooterField != null)
			{
				PropertyDescriptor boundFieldDesc = TypeDescriptor.GetProperties(dataItem).Find(this.FooterField, true);

				if (boundFieldDesc == null)
				{
					//Fail quietly so that VS.Net designer will work correctly
					return 0;
				}

				object dataValue = boundFieldDesc.GetValue(dataItem);

				if (dataValue != null && dataValue.GetType() == typeof(int))
				{
					return (int)dataValue;
				}
				else
				{
					return 0;
				}
			}
			else
			{
				return 0;
			}
		}
	}

	/// <summary>
	/// Implemented by all fields that support sub-headers
	/// </summary>
	public interface ISubHeaderField
	{
		/// <summary>
		/// Initializes the subheader field
		/// </summary>
		/// <param name="gridViewClientId">The ID of the gridview</param>
		/// <param name="cell">A DataControlFieldCell that contains the text or controls of the DataControlField</param>
		/// <param name="rowState">One of the DataControlRowState values, specifying the state of the row that contains the DataControlFieldCell</param>
		/// <param name="rowIndex">The index of the row that the DataControlFieldCell is contained in</param>
		void InitializeSubHeaderCell(string gridViewClientId, DataControlFieldCell cell, DataControlRowState rowState, int rowIndex);
	}

	/// <summary>
	/// This class extends the DataControlField to provide a CheckBoxField
	/// </summary>
	/// <remarks>Currently it simply implements the base class</remarks>
	[ToolboxData("<{0}:CheckBoxColumn runat=server></{0}:CheckBoxColumn>")]
	public class CheckBoxFieldEx : DataControlField, ISubHeaderField
	{
		protected string metaDataField;
		protected int footerColumnSpan;
		protected string dataKeyValue = "";

		/// <summary>
		/// Used to store the primary key identifier associated with this check-box column
		/// </summary>
		[
		Bindable(true),
		Category("Data"),
		Description("Used to store the primary key identifier associated with this check-box column"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public string DataKeyValue
		{
			get
			{
				return this.dataKeyValue;
			}
			set
			{
				this.dataKeyValue = value;
			}
		}

		/// <summary>
		/// The number of item columns that the footer should span. Passing a negative number hides the footer cell.
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("The number of item columns that the footer should span"),
		DefaultValue(0),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public int FooterColumnSpan
		{
			get
			{
				return this.footerColumnSpan;
			}
			set
			{
				this.footerColumnSpan = value;
			}
		}

		/// <summary>
		/// Defines the dataset field that stores the value to be filtered on
		/// </summary>
		[
		Bindable(true),
		Category("Data"),
		Description("Defines the dataset field that stores the value to be stored in the check-box meta-data"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public virtual string MetaDataField
		{
			get
			{
				return this.metaDataField;
			}
			set
			{
				this.metaDataField = value;
			}
		}

		/// <summary>
		/// Constructor - delegates to the base class
		/// </summary>
		public CheckBoxFieldEx()
			: base()
		{
		}

		/// <summary>
		/// Called when the cell is databinding, used to populate the check-box meta-data
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void cell_DataBinding(object sender, EventArgs e)
		{
			TableCell cell = (TableCell)sender;
			GridViewRow gridViewRow = (GridViewRow)cell.NamingContainer;
			GridViewEx gridView = (GridViewEx)gridViewRow.NamingContainer;

			if (cell.Controls[0].GetType() == typeof(CheckBoxEx))
			{
				//Get a handle to the checkbox if we have one
				CheckBoxEx checkBox = (CheckBoxEx)cell.Controls[0];

				//Get the underlying value being databound
				string dataValue = this.GetUnderlyingValue(gridViewRow.DataItem);
				checkBox.MetaData = dataValue;

				//Add the changed hyperlink
				string clientId = gridView.ClientID;
				checkBox.Attributes.Add("onclick", "$find('" + clientId + "').checkbox_changed()");
			}
		}

		/// <summary>
		/// This function returns a list of the selected checkboxes
		/// </summary>
		/// <returns>A collection of primary key IDs and index ids</returns>
		protected internal List<string> GetSelected()
		{
			//Get access to the gridview containing this field
			GridViewEx gridView = (GridViewEx)this.Control;

			//Find the index of this column
			int cellIndex = gridView.Columns.IndexOf(this);

			List<string> checkBoxList = new List<string>();
			//Loop through the list of checkboxes, and add those that are checked to
			//the collection of selected items
			foreach (GridViewRow row in gridView.Rows)
			{
				CheckBoxEx checkBox = (CheckBoxEx)row.Cells[cellIndex].Controls[0];
				string primaryKey = checkBox.MetaData;
				if (checkBox.Checked)
				{
					//The value stored in the list is the primary key id and the row index, separated by colons
					string listValue = primaryKey + ":" + row.RowIndex.ToString();
					checkBoxList.Add(listValue);
				}
			}
			return (checkBoxList);
		}

		/// <summary>
		/// Returns the underlying value for a specific bound data-item
		/// </summary>
		/// <param name="dataItem">The bound data-item</param>
		/// <returns>The string representation of the value</returns>
		protected string GetUnderlyingValue(object dataItem)
		{
			//Handle NULLs correctly
			if (this.MetaDataField != null)
			{
				PropertyDescriptor boundFieldDesc = TypeDescriptor.GetProperties(dataItem).Find(this.MetaDataField, true);

				if (boundFieldDesc == null)
				{
					//Fail quietly so that VS.Net designer will work correctly
					return "";
				}

				object dataValue = boundFieldDesc.GetValue(dataItem);

				if (dataValue.GetType() == typeof(string))
				{
					return (string)dataValue;
				}
				else if (dataValue.GetType() == typeof(int))
				{
					return ((int)dataValue).ToString();
				}
				else
				{
					return "";
				}
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Creates an empty CheckBoxFieldEx when called
		/// </summary>
		/// <returns>The empty data control field</returns>
		protected override DataControlField CreateField()
		{
			//Creates a new field of type CheckBoxFieldEx
			return new CheckBoxFieldEx();
		}

		/// <summary>
		/// Adds text or controls to a cell's controls collection.
		/// </summary>
		/// <param name="cell">A DataControlFieldCell that contains the text or controls of the DataControlField</param>
		/// <param name="cellType">One of the DataControlCellType values</param>
		/// <param name="rowState">One of the DataControlRowState values, specifying the state of the row that contains the DataControlFieldCell</param>
		/// <param name="rowIndex">The index of the row that the DataControlFieldCell is contained in</param>
		public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
		{
			//First call the base implementation
			base.InitializeCell(cell, cellType, rowState, rowIndex);

			//Handle any footer column spans
			if (cellType == DataControlCellType.Footer && this.footerColumnSpan > 0)
			{
				cell.ColumnSpan = this.footerColumnSpan;
			}

			//If we have a negative footer column span, then don't display the footer
			if (cellType == DataControlCellType.Footer && this.footerColumnSpan == -1)
			{
				cell.Visible = false;
			}

			//See if we have an item row type
			if (cellType == DataControlCellType.DataCell)
			{
				//Create a checkbox, the meta-data field is populated during databinding
				int colIndex = ((GridViewEx)this.Control).Columns.IndexOf(this);
				CheckBoxEx checkBox = new CheckBoxEx();
				checkBox.ID = "chkCol_" + rowIndex + "_" + colIndex;
				checkBox.Enabled = true;
				cell.Controls.Add(checkBox);
				//Add the event-handler to load in the meta-data values when data-binding
				cell.DataBinding += new EventHandler(cell_DataBinding);
			}
		}

		/// <summary>
		/// Adds the subheader cell if specified
		/// </summary>
		/// <param name="cell">A DataControlFieldCell that contains the text or controls of the DataControlField</param>
		/// <param name="rowState">One of the DataControlRowState values, specifying the state of the row that contains the DataControlFieldCell</param>
		/// <param name="rowIndex">The index of the row that the DataControlFieldCell is contained in</param>
		/// <param name="gridViewClientId">The id of tge gridview</param>
		public void InitializeSubHeaderCell(string gridViewClientId, DataControlFieldCell cell, DataControlRowState rowState, int rowIndex)
		{
			//First call the base implementation for type=header
			base.InitializeCell(cell, DataControlCellType.Header, rowState, rowIndex);

			//For a subheader we display the checkbox that will 'select all'
			CheckBoxEx checkBox = new CheckBoxEx();
			checkBox.ID = "chkHead" + this.MetaDataField + "SelectAll";
			checkBox.Enabled = true;
			cell.Controls.Add(checkBox);

			//Add the changed hyperlink
			checkBox.Attributes.Add("onclick", "$find('" + gridViewClientId + "').select_all_checkboxes()");
		}

		public static explicit operator CheckBoxFieldEx(Control v)
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// Any field that can have a running total needs this implement this interface
	/// </summary>
	public interface IRunningTotalField
	{
		void RestartCount();
	}

	/// <summary>
	/// This class extends the built-in ASP.NET Button Fiels
	/// </summary>
	/// <remarks>Adds support for auto-total footers</remarks>
	[ToolboxData("<{0}:ButtonFieldEx runat=server></{0}:ButtonFieldEx>")]
	public class ButtonFieldEx : System.Web.UI.WebControls.ButtonField, ISubHeaderField, IRunningTotalField
	{
		protected string subHeaderText;
		protected int headerColumnSpan;
		protected int footerColumnSpan;
		protected int subHeaderColumnSpan;
		protected string footerField;
		protected string commandArgumentField;
		protected System.Web.UI.WebControls.TableItemStyle subHeaderStyle;
		protected int runningTotal = 0;

		/// <summary>
		/// Constructor - delegates to the base class
		/// </summary>
		public ButtonFieldEx()
			: base()
		{
		}

		/// <summary>
		/// Restarts the running total count
		/// </summary>
		public void RestartCount()
		{
			this.runningTotal = 0;
		}

		/// <summary>
		/// Used to store any page-specific meta-data
		/// </summary>
		public string MetaData
		{
			get;
			set;
		}

		/// <summary>
		/// The number of item columns that the header should span. Passing a negative number hides the header cell.
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("The number of item columns that the header should span"),
		DefaultValue(0),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public int HeaderColumnSpan
		{
			get
			{
				return this.headerColumnSpan;
			}
			set
			{
				this.headerColumnSpan = value;
			}
		}

		/// <summary>
		/// The number of item columns that the footer should span. Passing a negative number hides the footer cell.
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("The number of item columns that the footer should span"),
		DefaultValue(0),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public int FooterColumnSpan
		{
			get
			{
				return this.footerColumnSpan;
			}
			set
			{
				this.footerColumnSpan = value;
			}
		}

		/// <summary>
		/// The number of item columns that the sub-header should span. Passing a negative number hides the sub-header cell.
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("The number of item columns that the sub-header should span"),
		DefaultValue(0),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public int SubHeaderColumnSpan
		{
			get
			{
				return this.subHeaderColumnSpan;
			}
			set
			{
				this.subHeaderColumnSpan = value;
			}
		}

		/// <summary>
		/// The field in the data-source that should be used to generate the running total used in the footer
		/// </summary>
		[
		Bindable(true),
		Category("Data"),
		Description("The field in the data-source that should be used to generate the running total used in the footer"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public string FooterField
		{
			get
			{
				return this.footerField;
			}
			set
			{
				this.footerField = value;
			}
		}

		/// <summary>
		/// Defines the style for the subheader column of the datagridex
		/// </summary>
		[
		Bindable(true),
		Category("Style"),
		Description("The style to be applied to sub-header items."),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
		NotifyParentProperty(true),
		PersistenceMode(PersistenceMode.InnerProperty)
		]
		public virtual System.Web.UI.WebControls.TableItemStyle SubHeaderStyle
		{
			get
			{
				if (this.subHeaderStyle == null)
				{
					this.subHeaderStyle = new TableItemStyle();
				}
				return this.subHeaderStyle;
			}
		}

		/// <summary>
		/// Specifies the field that should be used as the command argument
		/// </summary>
		[
		Bindable(true),
		Category("Data"),
		Description("Specifies the field that should be used as the command argument"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public virtual string CommandArgumentField
		{
			get
			{
				return this.commandArgumentField;
			}
			set
			{
				this.commandArgumentField = value;
			}
		}

		/// <summary>
		/// Defines the text for the subheader of the datagridex
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("Defines the text for the subheader of the datagridex"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public virtual string SubHeaderText
		{
			get
			{
				return this.subHeaderText;
			}
			set
			{
				this.subHeaderText = value;
			}
		}

		/// <summary>
		/// Adds the subheader cell if specified
		/// </summary>
		/// <param name="cell">A DataControlFieldCell that contains the text or controls of the DataControlField</param>
		/// <param name="rowState">One of the DataControlRowState values, specifying the state of the row that contains the DataControlFieldCell</param>
		/// <param name="rowIndex">The index of the row that the DataControlFieldCell is contained in</param>
		public virtual void InitializeSubHeaderCell(string gridViewClientId, DataControlFieldCell cell, DataControlRowState rowState, int rowIndex)
		{
			cell.Text = this.subHeaderText;

			//Handle any sub-header column spans
			if (this.subHeaderColumnSpan > 0)
			{
				cell.ColumnSpan = this.subHeaderColumnSpan;
			}

			//If we have a negative sub-header column span, then don't display the sub-header
			if (this.subHeaderColumnSpan == -1)
			{
				cell.Visible = false;
			}
		}

		/// <summary>
		/// Adds text or controls to a cell's controls collection.
		/// </summary>
		/// <param name="cell">A DataControlFieldCell that contains the text or controls of the DataControlField</param>
		/// <param name="cellType">One of the DataControlCellType values</param>
		/// <param name="rowState">One of the DataControlRowState values, specifying the state of the row that contains the DataControlFieldCell</param>
		/// <param name="rowIndex">The index of the row that the DataControlFieldCell is contained in</param>
		public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
		{
			//First call the base implementation
			base.InitializeCell(cell, cellType, rowState, rowIndex);

			//Handle any header column spans
			if (cellType == DataControlCellType.Header && this.headerColumnSpan > 0)
			{
				cell.ColumnSpan = this.headerColumnSpan;
			}

			//Handle any footer column spans
			if (cellType == DataControlCellType.Footer && this.footerColumnSpan > 0)
			{
				cell.ColumnSpan = this.footerColumnSpan;
			}

			//If we have a negative header column span, then don't display the header
			if (cellType == DataControlCellType.Header && this.headerColumnSpan == -1)
			{
				cell.Visible = false;
			}

			//If we have a negative footer column span, then don't display the footer
			if (cellType == DataControlCellType.Footer && this.footerColumnSpan == -1)
			{
				cell.Visible = false;
			}

			//If we are displaying a footer, need to attached an event handler for calculating the sum
			if (this.FooterField != null && this.FooterField != "" && (cellType == DataControlCellType.DataCell || cellType == DataControlCellType.Footer))
			{
				cell.DataBinding += new EventHandler(cell_DataBinding);
			}
		}

		/// <summary>
		/// Returns the underlying value for a specific bound data-item
		/// </summary>
		/// <param name="dataItem">The bound data-item</param>
		/// <param name="fieldName">The name of the field to lookup</param>
		/// <returns>The integer representation of the value</returns>
		protected int GetUnderlyingIntValue(object dataItem, string fieldName)
		{
			//Handle NULLs correctly
			if (fieldName != null)
			{
				PropertyDescriptor boundFieldDesc = TypeDescriptor.GetProperties(dataItem).Find(fieldName, true);

				if (boundFieldDesc == null)
				{
					//Fail quietly so that VS.Net designer will work correctly
					return 0;
				}

				object dataValue = boundFieldDesc.GetValue(dataItem);

				if (dataValue.GetType() == typeof(int))
				{
					return (int)dataValue;
				}
				else
				{
					return 0;
				}
			}
			else
			{
				return 0;
			}
		}

		/// <summary>
		/// Returns the underlying value for a specific bound data-item
		/// </summary>
		/// <param name="dataItem">The bound data-item</param>
		/// <param name="fieldName">The name of the field to lookup</param>
		/// <returns>The integer representation of the value</returns>
		protected string GetUnderlyingStringValue(object dataItem, string fieldName)
		{
			//Handle NULLs correctly
			if (fieldName != null)
			{
				PropertyDescriptor boundFieldDesc = TypeDescriptor.GetProperties(dataItem).Find(fieldName, true);

				if (boundFieldDesc == null)
				{
					//Fail quietly so that VS.Net designer will work correctly
					return "";
				}

				object dataValue = boundFieldDesc.GetValue(dataItem);

				if (dataValue.GetType() == typeof(string))
				{
					return (string)dataValue;
				}
				else
				{
					return dataValue.ToString();
				}
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Called when the cell is databinding, used to calculate totals in the footer
		/// and add the command argument to the button
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void cell_DataBinding(object sender, EventArgs e)
		{
			TableCell cell = (TableCell)sender;
			GridViewRow gridViewRow = (GridViewRow)cell.NamingContainer;

			//If we have an item, add to total and add the command argument (if one provided)
			if (gridViewRow.RowType == DataControlRowType.DataRow)
			{
				int dataValue = this.GetUnderlyingIntValue(gridViewRow.DataItem, this.FooterField);
				runningTotal += dataValue;

				//Handle the command argument
				if (this.CommandArgumentField != "")
				{
					string commandArgument = this.GetUnderlyingStringValue(gridViewRow.DataItem, this.CommandArgumentField);
					if (this.ButtonType == System.Web.UI.WebControls.ButtonType.Link && typeof(LinkButton).IsAssignableFrom(cell.Controls[0].GetType()))
					{
						LinkButton linkButton = (LinkButton)cell.Controls[0];
						linkButton.CommandArgument = commandArgument;
					}
					if (this.ButtonType == System.Web.UI.WebControls.ButtonType.Button && typeof(Button).IsAssignableFrom(cell.Controls[0].GetType()))
					{
						Button pushButton = (Button)cell.Controls[0];
						pushButton.CommandArgument = commandArgument;
					}
				}
			}

			//If we have a footer, bind total to column
			if (gridViewRow.RowType == DataControlRowType.Footer && runningTotal > 0)
			{
				cell.Text = runningTotal.ToString();
			}
		}
	}

	/// <summary>
	/// This class provides a special column for display artifact names and popup tooltips
	/// </summary>
	/// <remarks>
	/// The tooltip can be either loaded in at page-render time or asynchronously using an AJAX web service.
	/// </remarks>
	[ToolboxData("<{0}:BoundFieldEx runat=server></{0}:BoundFieldEx>")]
	public class NameDescriptionFieldEx : System.Web.UI.WebControls.BoundField, ISubHeaderField
	{
		protected string subHeaderText;
		protected int headerColumnSpan;
		protected int footerColumnSpan;
		protected int subHeaderColumnSpan;
		protected string footerField;
		protected System.Web.UI.WebControls.TableItemStyle subHeaderStyle;
		protected string descriptionField = "";
		protected string commandArgumentField = "";
		protected int nameMaxLength = 50;
		protected string commandName = "";
		protected string navigateUrlFormat = "";

		/// <summary>
		/// Constructor - delegates to the base class
		/// </summary>
		public NameDescriptionFieldEx()
			: base()
		{
		}

		/// <summary>
		/// The id of the project, needed for asynchronous AJAX tooltips, retrieved from settings
		/// </summary>
		public int ProjectId
		{
			get
			{
				if (SpiraContext.Current != null && SpiraContext.Current.ProjectId.HasValue)
				{
					return SpiraContext.Current.ProjectId.Value;
				}
				return -1;
			}
		}

		/// <summary>
		/// Defines the dataset field that stores the description to be displayed in the tooltip
		/// </summary>
		[
		Bindable(true),
		Category("Data"),
		Description("Defines the dataset field that stores the description to be displayed in the tooltip"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public virtual string DescriptionField
		{
			get
			{
				return this.descriptionField;
			}
			set
			{
				this.descriptionField = value;
			}
		}

		/// <summary>
		/// Defines the command name that the link button should use
		/// </summary>
		/// <remarks>
		/// If set, the hyperlink will be a link button
		/// </remarks>
		[
		Bindable(true),
		Category("Data"),
		Description("Defines the command name that the link button should use"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public virtual string CommandName
		{
			get
			{
				return this.commandName;
			}
			set
			{
				this.commandName = value;
			}
		}

		/// <summary>
		/// Defines the navigation Url format string that should be used
		/// </summary>
		/// <remarks>
		/// If set, the hyperlink will be a real permalink hyperlink
		/// e.g. http://myserver/spira/yada/{0}.aspx where the {0} will be replaced by the CommandArgumentField
		/// </remarks>
		[
		Bindable(true),
		Category("Data"),
		Description("Defines the navigation url format string that the hyperlink should use"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public virtual string NavigateUrlFormat
		{
			get
			{
				return this.navigateUrlFormat;
			}
			set
			{
				this.navigateUrlFormat = value;
			}
		}

		/// <summary>
		/// Defines the dataset field that stores the command argument for the link button or the parameter
		/// to be inserted into the hyperlink URL instead of parameter {0}
		/// </summary>
		[
		Bindable(true),
		Category("Data"),
		Description("Defines the dataset field that stores the command argument for the link button or hyperlink"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public virtual string CommandArgumentField
		{
			get
			{
				return this.commandArgumentField;
			}
			set
			{
				this.commandArgumentField = value;
			}
		}

		/// <summary>
		/// The number of item columns that the header should span. Passing a negative number hides the header cell.
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("The number of item columns that the header should span"),
		DefaultValue(0),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public int HeaderColumnSpan
		{
			get
			{
				return this.headerColumnSpan;
			}
			set
			{
				this.headerColumnSpan = value;
			}
		}

		/// <summary>
		/// The number of item columns that the footer should span. Passing a negative number hides the footer cell.
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("The number of item columns that the footer should span"),
		DefaultValue(0),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public int FooterColumnSpan
		{
			get
			{
				return this.footerColumnSpan;
			}
			set
			{
				this.footerColumnSpan = value;
			}
		}

		/// <summary>
		/// The number of item columns that the footer should span. Passing a negative number hides the footer cell.
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("The number of characters to display before being truncated"),
		DefaultValue(50),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public int NameMaxLength
		{
			get
			{
				return this.nameMaxLength;
			}
			set
			{
				this.nameMaxLength = value;
			}
		}

		/// <summary>
		/// The number of item columns that the sub-header should span. Passing a negative number hides the sub-header cell.
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("The number of item columns that the sub-header should span"),
		DefaultValue(0),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public int SubHeaderColumnSpan
		{
			get
			{
				return this.subHeaderColumnSpan;
			}
			set
			{
				this.subHeaderColumnSpan = value;
			}
		}

		/// <summary>
		/// The field in the data-source that should be used to generate the running total used in the footer
		/// </summary>
		[
		Bindable(true),
		Category("Data"),
		Description("The field in the data-source that should be used to generate the running total used in the footer"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public string FooterField
		{
			get
			{
				return this.footerField;
			}
			set
			{
				this.footerField = value;
			}
		}

		/// <summary>
		/// Defines the style for the subheader column of the gridviewex
		/// </summary>
		[
		Bindable(true),
		Category("Style"),
		Description("The style to be applied to sub-header items."),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
		NotifyParentProperty(true),
		PersistenceMode(PersistenceMode.InnerProperty)
		]
		public virtual System.Web.UI.WebControls.TableItemStyle SubHeaderStyle
		{
			get
			{
				if (this.subHeaderStyle == null)
				{
					this.subHeaderStyle = new TableItemStyle();
				}
				return this.subHeaderStyle;
			}
		}

		/// <summary>
		/// Defines the text for the subheader of the gridviewex
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("Defines the text for the subheader of the gridviewex"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public virtual string SubHeaderText
		{
			get
			{
				return this.subHeaderText;
			}
			set
			{
				this.subHeaderText = value;
			}
		}

		/// <summary>
		/// Adds the subheader cell if specified
		/// </summary>
		/// <param name="cell">A DataControlFieldCell that contains the text or controls of the DataControlField</param>
		/// <param name="rowState">One of the DataControlRowState values, specifying the state of the row that contains the DataControlFieldCell</param>
		/// <param name="rowIndex">The index of the row that the DataControlFieldCell is contained in</param>
		public virtual void InitializeSubHeaderCell(string gridViewClientId, DataControlFieldCell cell, DataControlRowState rowState, int rowIndex)
		{
			cell.Text = this.subHeaderText;

			//Handle any sub-header column spans
			if (this.subHeaderColumnSpan > 0)
			{
				cell.ColumnSpan = this.subHeaderColumnSpan;
			}

			//If we have a negative sub-header column span, then don't display the sub-header
			if (this.subHeaderColumnSpan == -1)
			{
				cell.Visible = false;
			}
		}

		/// <summary>
		/// Adds text or controls to a cell's controls collection.
		/// </summary>
		/// <param name="cell">A DataControlFieldCell that contains the text or controls of the DataControlField</param>
		/// <param name="cellType">One of the DataControlCellType values</param>
		/// <param name="rowState">One of the DataControlRowState values, specifying the state of the row that contains the DataControlFieldCell</param>
		/// <param name="rowIndex">The index of the row that the DataControlFieldCell is contained in</param>
		public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
		{
			//For data rows need to display a link button and tooltip
			if (cellType == DataControlCellType.DataCell)
			{
				//Create a link button control or hyperlink control
				if (String.IsNullOrEmpty(this.navigateUrlFormat))
				{
					LinkButtonEx linkButton = new LinkButtonEx();
					linkButton.Enabled = cell.Enabled;
					linkButton.CommandName = this.CommandName;
					cell.Controls.Add(linkButton);
					//Add the code to populate the link when the control is databound
					cell.DataBinding += new EventHandler(cell_DataBinding);
				}
				else
				{
					HyperLinkEx hyperLink = new HyperLinkEx();
					hyperLink.Enabled = cell.Enabled;
					cell.Controls.Add(hyperLink);
					//Add the code to populate the link when the control is databound
					cell.DataBinding += new EventHandler(cell_DataBinding);
				}
			}
			else
			{
				//Use the base implementation
				base.InitializeCell(cell, cellType, rowState, rowIndex);
			}

			//Handle any header column spans
			if (cellType == DataControlCellType.Header && this.headerColumnSpan > 0)
			{
				cell.ColumnSpan = this.headerColumnSpan;
			}

			//Handle any footer column spans
			if (cellType == DataControlCellType.Footer && this.footerColumnSpan > 0)
			{
				cell.ColumnSpan = this.footerColumnSpan;
			}

			//If we have a negative header column span, then don't display the header
			if (cellType == DataControlCellType.Header && this.headerColumnSpan == -1)
			{
				cell.Visible = false;
			}

			//If we have a negative footer column span, then don't display the footer
			if (cellType == DataControlCellType.Footer && this.footerColumnSpan == -1)
			{
				cell.Visible = false;
			}
		}

		/// <summary>
		/// Returns the underlying value for a specific bound data-item
		/// </summary>
		/// <param name="dataItem">The bound data-item</param>
		/// <param name="fieldName">The field name</param>
		/// <param name="format">The format string</param>
		/// <returns>The integer representation of the value</returns>
		protected string GetUnderlyingValue(object dataItem, string fieldName, string format)
		{
			//Handle null strings correctly
			if (fieldName != "")
			{
				PropertyDescriptor boundFieldDesc = TypeDescriptor.GetProperties(dataItem).Find(fieldName, true);

				if (boundFieldDesc == null)
				{
					//Fail quietly so that VS.Net designer will work correctly
					return "";
				}

				object dataValue = boundFieldDesc.GetValue(dataItem);

				if (dataValue == null)
				{
					return "";
				}

				if (dataValue.GetType() == typeof(int))
				{
					if (String.IsNullOrEmpty(format))
					{
						return ((int)dataValue).ToString();
					}
					else
					{
						return String.Format(format, dataValue);
					}
				}
				else if (dataValue.GetType() == typeof(string))
				{
					return (string)dataValue;
				}
				else if (dataValue.GetType() == typeof(DateTime))
				{
					return ((DateTime)dataValue).ToString("d");
				}
				else
				{
					return "";
				}
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Called when the cell is databinding, used to calculate totals in the footer
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void cell_DataBinding(object sender, EventArgs e)
		{
			TableCell cell = (TableCell)sender;
			GridViewRow gridViewRow = (GridViewRow)cell.NamingContainer;

			//See if we have a project Id in the data source
			int projectId = -1;
			string projectIdString = this.GetUnderlyingValue(gridViewRow.DataItem, "ProjectId", null);
			int intValue;
			if (!String.IsNullOrEmpty(projectIdString) && Int32.TryParse(projectIdString, out intValue))
			{
				projectId = intValue;
			}
			else
			{
				projectId = ProjectId;
			}

			if (cell.Controls[0].GetType() == typeof(LinkButtonEx))
			{
				LinkButtonEx linkButton = (LinkButtonEx)cell.Controls[0];
				//Get the underlying values being databound
				string nameValue = this.GetUnderlyingValue(gridViewRow.DataItem, this.DataField, this.DataFormatString);
				string descriptionValue = "";
				if (!String.IsNullOrEmpty(this.DescriptionField))
				{
					descriptionValue = this.GetUnderlyingValue(gridViewRow.DataItem, this.DescriptionField, "");
				}
				string commandArgument = "";
				if (!String.IsNullOrEmpty(this.CommandArgumentField))
				{
					commandArgument = this.GetUnderlyingValue(gridViewRow.DataItem, this.CommandArgumentField, "");
				}
				linkButton.Text = GlobalFunctions.TruncateName(nameValue, this.NameMaxLength);
				linkButton.CommandArgument = commandArgument;

				//--ARIA Attributes--
				//Set the label attribute of the row to be the name
				gridViewRow.Attributes.Add("aria-label", nameValue);

				//If we are using Ajax, then don't display the tooltip now, it gets loaded asynchronously
				GridViewEx gridView = (GridViewEx)gridViewRow.NamingContainer;
				if (String.IsNullOrEmpty(gridView.WebServiceClass))
				{
					if (String.IsNullOrEmpty(descriptionValue))
					{
						linkButton.ToolTip = nameValue;
					}
					else
					{
						linkButton.ToolTip = "<u>" + nameValue + "</u><br />" + descriptionValue;
					}
				}
				else
				{
					//Add the ajax handlers as long as we have a command argument set (the primary key)
					if (!String.IsNullOrEmpty(commandArgument))
					{
						string clientId = gridView.ClientID;
						linkButton.Attributes.Add("onmouseover", "$find('" + clientId + "').display_tooltip(" + commandArgument + "," + projectId + ")");
						linkButton.Attributes.Add("onmouseout", "$find('" + clientId + "').hide_tooltip()");

						//--ARIA Attributes--
						//Set the primary key of the row
						if (String.IsNullOrEmpty(gridViewRow.Attributes["tst:primarykey"]))
						{
							gridViewRow.Attributes.Add("tst:primarykey", commandArgument);
						}
					}
				}
			}
			if (cell.Controls[0].GetType() == typeof(HyperLinkEx))
			{
				HyperLinkEx hyperLink = (HyperLinkEx)cell.Controls[0];
				//Get the underlying values being databound
				string nameValue = this.GetUnderlyingValue(gridViewRow.DataItem, this.DataField, this.DataFormatString);
				string descriptionValue = "";
				if (!String.IsNullOrEmpty(this.DescriptionField))
				{
					descriptionValue = this.GetUnderlyingValue(gridViewRow.DataItem, this.DescriptionField, "");
				}
				string commandArgument = "";
				if (!String.IsNullOrEmpty(this.CommandArgumentField))
				{
					commandArgument = this.GetUnderlyingValue(gridViewRow.DataItem, this.CommandArgumentField, "");
				}
				hyperLink.Text = GlobalFunctions.TruncateName(nameValue, this.NameMaxLength);
				if (String.IsNullOrEmpty(commandArgument))
				{
					hyperLink.NavigateUrl = this.navigateUrlFormat;
				}
				else if (!String.IsNullOrEmpty(this.navigateUrlFormat) && navigateUrlFormat != "#")
				{
					//We use # when the navigate format is not specified but we still want a hyperlink not linkbutton
					hyperLink.NavigateUrl = String.Format(this.navigateUrlFormat, commandArgument);
				}

				//--ARIA Attributes--
				//Set the label attribute of the row to be the name
				gridViewRow.Attributes.Add("aria-label", nameValue);

				//If we are using Ajax, then don't display the tooltip now, it gets loaded asynchronously
				GridViewEx gridView = (GridViewEx)gridViewRow.NamingContainer;
				if (String.IsNullOrEmpty(gridView.WebServiceClass))
				{
					if (String.IsNullOrEmpty(descriptionValue))
					{
						hyperLink.ToolTip = nameValue;
					}
					else
					{
						hyperLink.ToolTip = "<u>" + nameValue + "</u><br />" + descriptionValue;
					}
				}
				else
				{
					//Add the ajax handlers as long as we have a command argument set (the primary key)
					if (!String.IsNullOrEmpty(commandArgument))
					{
						string clientId = gridView.ClientID;
						hyperLink.Attributes.Add("onmouseover", "$find('" + clientId + "').display_tooltip(" + commandArgument + "," + projectId + ")");
						hyperLink.Attributes.Add("onmouseout", "$find('" + clientId + "').hide_tooltip()");

						//--ARIA Attributes--
						//Set the primary key of the row
						if (String.IsNullOrEmpty(gridViewRow.Attributes["tst:primarykey"]))
						{
							gridViewRow.Attributes.Add("tst:primarykey", commandArgument);
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// This class extends the built-in ASP.NET BoundColumn
	/// </summary>
	/// <remarks>Currently it simply implements the base class</remarks>
	[ToolboxData("<{0}:BoundFieldEx runat=server></{0}:BoundFieldEx>")]
	public class BoundFieldEx : System.Web.UI.WebControls.BoundField, ISubHeaderField, IRunningTotalField
	{
		protected string subHeaderText;
		protected int headerColumnSpan;
		protected int footerColumnSpan;
		protected int subHeaderColumnSpan;
		protected string footerField;
		protected int maxLength = -1;
		protected System.Web.UI.WebControls.TableItemStyle subHeaderStyle;

		protected int runningTotal = 0;

		/// <summary>
		/// Constructor - delegates to the base class
		/// </summary>
		public BoundFieldEx()
			: base()
		{
		}

		/// <summary>
		/// Restarts the running total count
		/// </summary>
		public void RestartCount()
		{
			this.runningTotal = 0;
		}

		/// <summary>
		/// The number of characters to display before being truncated
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("The number of characters to display before being truncated"),
		DefaultValue(-1),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public int MaxLength
		{
			get
			{
				return this.maxLength;
			}
			set
			{
				this.maxLength = value;
			}
		}

		/// <summary>
		/// The number of item columns that the header should span. Passing a negative number hides the header cell.
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("The number of item columns that the header should span"),
		DefaultValue(0),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public int HeaderColumnSpan
		{
			get
			{
				return this.headerColumnSpan;
			}
			set
			{
				this.headerColumnSpan = value;
			}
		}

		/// <summary>
		/// The number of item columns that the footer should span. Passing a negative number hides the footer cell.
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("The number of item columns that the footer should span"),
		DefaultValue(0),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public int FooterColumnSpan
		{
			get
			{
				return this.footerColumnSpan;
			}
			set
			{
				this.footerColumnSpan = value;
			}
		}

		/// <summary>
		/// The number of item columns that the sub-header should span. Passing a negative number hides the sub-header cell.
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("The number of item columns that the sub-header should span"),
		DefaultValue(0),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public int SubHeaderColumnSpan
		{
			get
			{
				return this.subHeaderColumnSpan;
			}
			set
			{
				this.subHeaderColumnSpan = value;
			}
		}

		/// <summary>
		/// The field in the data-source that should be used to generate the running total used in the footer
		/// </summary>
		[
		Bindable(true),
		Category("Data"),
		Description("The field in the data-source that should be used to generate the running total used in the footer"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public string FooterField
		{
			get
			{
				return this.footerField;
			}
			set
			{
				this.footerField = value;
			}
		}

		/// <summary>
		/// The field in the data-source that should be used to generate the running total used in the footer
		/// </summary>
		[
		Bindable(true),
		Category("Data"),
		Description("The base name to use for the Fields.resx localized lookup string. The data-field value is appended"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public string LocalizedBase
		{
			get;
			set;
		}

		/// <summary>
		/// Defines the style for the subheader column of the gridviewex
		/// </summary>
		[
		Bindable(true),
		Category("Style"),
		Description("The style to be applied to sub-header items."),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
		NotifyParentProperty(true),
		PersistenceMode(PersistenceMode.InnerProperty)
		]
		public virtual System.Web.UI.WebControls.TableItemStyle SubHeaderStyle
		{
			get
			{
				if (this.subHeaderStyle == null)
				{
					this.subHeaderStyle = new TableItemStyle();
				}
				return this.subHeaderStyle;
			}
		}

		/// <summary>
		/// Defines the text for the subheader of the gridviewex
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("Defines the text for the subheader of the gridviewex"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public virtual string SubHeaderText
		{
			get
			{
				return this.subHeaderText;
			}
			set
			{
				this.subHeaderText = value;
			}
		}

		/// <summary>
		/// Adds the subheader cell if specified
		/// </summary>
		/// <param name="cell">A DataControlFieldCell that contains the text or controls of the DataControlField</param>
		/// <param name="rowState">One of the DataControlRowState values, specifying the state of the row that contains the DataControlFieldCell</param>
		/// <param name="rowIndex">The index of the row that the DataControlFieldCell is contained in</param>
		public virtual void InitializeSubHeaderCell(string gridViewClientId, DataControlFieldCell cell, DataControlRowState rowState, int rowIndex)
		{
			cell.Text = this.subHeaderText;

			//Handle any sub-header column spans
			if (this.subHeaderColumnSpan > 0)
			{
				cell.ColumnSpan = this.subHeaderColumnSpan;
			}

			//If we have a negative sub-header column span, then don't display the sub-header
			if (this.subHeaderColumnSpan == -1)
			{
				cell.Visible = false;
			}
		}

		/// <summary>
		/// Adds text or controls to a cell's controls collection.
		/// </summary>
		/// <param name="cell">A DataControlFieldCell that contains the text or controls of the DataControlField</param>
		/// <param name="cellType">One of the DataControlCellType values</param>
		/// <param name="rowState">One of the DataControlRowState values, specifying the state of the row that contains the DataControlFieldCell</param>
		/// <param name="rowIndex">The index of the row that the DataControlFieldCell is contained in</param>
		public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
		{
			//First we need to add the data to the various parts of the cell
			string headerText = null;
			bool flag = false;
			bool flag2 = false;
			if (((cellType == DataControlCellType.Header) && this.SupportsHtmlEncode) && this.HtmlEncode)
			{
				headerText = this.HeaderText;
				flag2 = true;
			}
			if (flag2 && !string.IsNullOrEmpty(headerText))
			{
				this.HeaderText = HttpUtility.HtmlEncode(headerText);
				flag = true;
			}
			base.InitializeCell(cell, cellType, rowState, rowIndex);
			if (flag)
			{
				this.HeaderText = headerText;
			}
			if (cellType == DataControlCellType.DataCell)
			{
				this.InitializeDataCell(cell, rowState);
			}

			//Handle any header column spans
			if (cellType == DataControlCellType.Header && this.headerColumnSpan > 0)
			{
				cell.ColumnSpan = this.headerColumnSpan;
			}

			//Handle any footer column spans
			if (cellType == DataControlCellType.Footer && this.footerColumnSpan > 0)
			{
				cell.ColumnSpan = this.footerColumnSpan;
			}

			//If we have a negative header column span, then don't display the header
			if (cellType == DataControlCellType.Header && this.headerColumnSpan == -1)
			{
				cell.Visible = false;
			}

			//If we have a negative footer column span, then don't display the footer
			if (cellType == DataControlCellType.Footer && this.footerColumnSpan == -1)
			{
				cell.Visible = false;
			}

			//If we are displaying a footer, need to attached an event handler for calculating the sum
			if (this.FooterField != null && this.FooterField != "" && (cellType == DataControlCellType.DataCell || cellType == DataControlCellType.Footer))
			{
				cell.DataBinding += new EventHandler(cell_DataBinding);
			}
		}

		/// <summary>
		/// Initializes the data cells
		/// </summary>
		/// <param name="cell"></param>
		/// <param name="rowState"></param>
		protected override void InitializeDataCell(DataControlFieldCell cell, DataControlRowState rowState)
		{
			Control child = null;
			Control control2 = null;
			if ((((rowState & DataControlRowState.Edit) != DataControlRowState.Normal) && !this.ReadOnly) || ((rowState & DataControlRowState.Insert) != DataControlRowState.Normal))
			{
				TextBox box = new TextBox();
				box.ToolTip = this.HeaderText;
				child = box;
				if ((this.DataField.Length != 0) && ((rowState & DataControlRowState.Edit) != DataControlRowState.Normal))
				{
					control2 = box;
				}
			}
			else if (this.DataField.Length != 0)
			{
				control2 = cell;
			}
			if (child != null)
			{
				cell.Controls.Add(child);
			}
			if ((control2 != null) && base.Visible)
			{
				control2.DataBinding += new EventHandler(this.OnDataBindField);
			}
		}

		/// <summary>
		/// Extends the handling of bound columns to include accessing the navigation properties of an entity
		/// </summary>
		/// <param name="controlContainer"></param>
		/// <returns></returns>
		protected override object GetValue(Control controlContainer)
		{
			//If we have a period in the DataField then we need to handle it differently since we have an entity
			if (String.IsNullOrEmpty(this.DataField) || !this.DataField.Contains("."))
			{
				return base.GetValue(controlContainer);
			}
			else
			{
				object component = null;
				string dataField = this.DataField;
				if (controlContainer == null)
				{
					throw new HttpException(SR.GetString("DataControlField_NoContainer"));
				}
				component = DataBinder.GetDataItem(controlContainer);
				if ((component == null) && !base.DesignMode)
				{
					throw new HttpException(SR.GetString("DataItem_Not_Found"));
				}
				if (!dataField.Equals(ThisExpression))
				{
					//Get the successive properties
					string[] fields = dataField.Split('.');
					object currentObject = component;
					foreach (string field in fields)
					{
						if (currentObject != null)
						{
                            object childobject = TypeDescriptor.GetProperties(currentObject)?.Find(field.Trim(), true)?.GetValue(currentObject);
							if (childobject == null)
							{
								//Just return null;
								return null;
								//throw new HttpException(String.Format("Field {0} was not found on the data item {1}", field, currentObject.GetType().ToString()));
							}
							currentObject = childobject;
						}
					}
					return currentObject;
				}
				return component;
			}
		}

		/// <summary>
		/// Databinds the cell
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected override void OnDataBindField(object sender, EventArgs e)
		{
			Control control = (Control)sender;
			Control namingContainer = control.NamingContainer;
			object dataValue = this.GetValue(namingContainer);
			bool encode = (this.SupportsHtmlEncode && this.HtmlEncode) && (control is TableCell);
			string str = this.FormatDataValue(dataValue, encode);
			if (control is TableCell)
			{
				if (str.Length == 0)
				{
					str = "&nbsp;";
				}
				//truncate if necessary
				if (this.MaxLength != -1)
				{
					((TableCell)control).ToolTip = str;
					str = GlobalFunctions.TruncateName(str, this.MaxLength);
				}

				//See if we have a localized value to lookup
				if (!String.IsNullOrEmpty(this.LocalizedBase))
				{
					string str1 = Resources.Fields.ResourceManager.GetString(this.LocalizedBase + str);
					if (!String.IsNullOrEmpty(str1))
					{
						str = str1;
					}
				}
				((TableCell)control).Text = str;
			}
			else
			{
				if (!(control is TextBox))
				{
					throw new HttpException("BoundField_WrongControlType");
				}
				if (this.ApplyFormatInEditMode)
				{
					((TextBox)control).Text = str;
				}
				else if (dataValue != null)
				{
					((TextBox)control).Text = dataValue.ToString();
				}
				if ((dataValue != null) && dataValue.GetType().IsPrimitive)
				{
					((TextBox)control).Columns = 5;
				}
			}
		}

		/// <summary>
		/// Returns the underlying value for a specific bound data-item
		/// </summary>
		/// <param name="dataItem">The bound data-item</param>
		/// <returns>The integer representation of the value</returns>
		protected int GetUnderlyingValue(object dataItem)
		{
			//Handle NULLs correctly
			if (this.FooterField != null)
			{
				PropertyDescriptor boundFieldDesc = TypeDescriptor.GetProperties(dataItem).Find(this.FooterField, true);

				if (boundFieldDesc == null)
				{
					//Fail quietly so that VS.Net designer will work correctly
					return 0;
				}

				object dataValue = boundFieldDesc.GetValue(dataItem);

				if (dataValue.GetType() == typeof(int))
				{
					return (int)dataValue;
				}
				else
				{
					return 0;
				}
			}
			else
			{
				return 0;
			}
		}

		/// <summary>
		/// Called when the cell is databinding, used to calculate totals in the footer
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void cell_DataBinding(object sender, EventArgs e)
		{
			TableCell cell = (TableCell)sender;
			GridViewRow gridViewRow = (GridViewRow)cell.NamingContainer;

			//If we have an item, add to total
			if (gridViewRow.RowType == DataControlRowType.DataRow)
			{
				int dataValue = this.GetUnderlyingValue(gridViewRow.DataItem);
				runningTotal += dataValue;
			}

			//If we have a footer, bind total to column
			if (gridViewRow.RowType == DataControlRowType.Footer && runningTotal > 0)
			{
				cell.Text = runningTotal.ToString();
			}
		}
	}

	/// <summary>
	/// This class extends the built-in ASP.NET HyperLinkField
	/// </summary>
	/// <remarks>Currently it simply implements the base class</remarks>
	[ToolboxData("<{0}:BoundFieldEx runat=server></{0}:BoundFieldEx>")]
	public class HyperLinkFieldEx : System.Web.UI.WebControls.HyperLinkField, ISubHeaderField, IRunningTotalField
	{
		protected string subHeaderText;
		protected int headerColumnSpan;
		protected int footerColumnSpan;
		protected int subHeaderColumnSpan;
		protected string footerField;
		protected System.Web.UI.WebControls.TableItemStyle subHeaderStyle;

		protected int runningTotal = 0;

		/// <summary>
		/// Constructor - delegates to the base class
		/// </summary>
		public HyperLinkFieldEx()
			: base()
		{
		}

		/// <summary>
		/// Restarts the running total count
		/// </summary>
		public void RestartCount()
		{
			this.runningTotal = 0;
		}

		/// <summary>
		/// The number of item columns that the header should span. Passing a negative number hides the header cell.
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("The number of item columns that the header should span"),
		DefaultValue(0),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public int HeaderColumnSpan
		{
			get
			{
				return this.headerColumnSpan;
			}
			set
			{
				this.headerColumnSpan = value;
			}
		}

		/// <summary>
		/// The number of item columns that the footer should span. Passing a negative number hides the footer cell.
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("The number of item columns that the footer should span"),
		DefaultValue(0),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public int FooterColumnSpan
		{
			get
			{
				return this.footerColumnSpan;
			}
			set
			{
				this.footerColumnSpan = value;
			}
		}

		/// <summary>
		/// The number of item columns that the sub-header should span. Passing a negative number hides the sub-header cell.
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("The number of item columns that the sub-header should span"),
		DefaultValue(0),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public int SubHeaderColumnSpan
		{
			get
			{
				return this.subHeaderColumnSpan;
			}
			set
			{
				this.subHeaderColumnSpan = value;
			}
		}

		/// <summary>
		/// The field in the data-source that should be used to generate the running total used in the footer
		/// </summary>
		[
		Bindable(true),
		Category("Data"),
		Description("The field in the data-source that should be used to generate the running total used in the footer"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public string FooterField
		{
			get
			{
				return this.footerField;
			}
			set
			{
				this.footerField = value;
			}
		}

		/// <summary>
		/// Defines the style for the subheader column of the gridviewex
		/// </summary>
		[
		Bindable(true),
		Category("Style"),
		Description("The style to be applied to sub-header items."),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
		NotifyParentProperty(true),
		PersistenceMode(PersistenceMode.InnerProperty)
		]
		public virtual System.Web.UI.WebControls.TableItemStyle SubHeaderStyle
		{
			get
			{
				if (this.subHeaderStyle == null)
				{
					this.subHeaderStyle = new TableItemStyle();
				}
				return this.subHeaderStyle;
			}
		}

		/// <summary>
		/// Defines the text for the subheader of the gridviewex
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("Defines the text for the subheader of the gridviewex"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public virtual string SubHeaderText
		{
			get
			{
				return this.subHeaderText;
			}
			set
			{
				this.subHeaderText = value;
			}
		}

		/// <summary>
		/// Adds the subheader cell if specified
		/// </summary>
		/// <param name="cell">A DataControlFieldCell that contains the text or controls of the DataControlField</param>
		/// <param name="rowState">One of the DataControlRowState values, specifying the state of the row that contains the DataControlFieldCell</param>
		/// <param name="rowIndex">The index of the row that the DataControlFieldCell is contained in</param>
		public virtual void InitializeSubHeaderCell(string gridViewClientId, DataControlFieldCell cell, DataControlRowState rowState, int rowIndex)
		{
			cell.Text = this.subHeaderText;

			//Handle any sub-header column spans
			if (this.subHeaderColumnSpan > 0)
			{
				cell.ColumnSpan = this.subHeaderColumnSpan;
			}

			//If we have a negative sub-header column span, then don't display the sub-header
			if (this.subHeaderColumnSpan == -1)
			{
				cell.Visible = false;
			}
		}

		/// <summary>
		/// Adds text or controls to a cell's controls collection.
		/// </summary>
		/// <param name="cell">A DataControlFieldCell that contains the text or controls of the DataControlField</param>
		/// <param name="cellType">One of the DataControlCellType values</param>
		/// <param name="rowState">One of the DataControlRowState values, specifying the state of the row that contains the DataControlFieldCell</param>
		/// <param name="rowIndex">The index of the row that the DataControlFieldCell is contained in</param>
		public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
		{
			//First call the base implementation
			base.InitializeCell(cell, cellType, rowState, rowIndex);

			//Handle any header column spans
			if (cellType == DataControlCellType.Header && this.headerColumnSpan > 0)
			{
				cell.ColumnSpan = this.headerColumnSpan;
			}

			//Handle any footer column spans
			if (cellType == DataControlCellType.Footer && this.footerColumnSpan > 0)
			{
				cell.ColumnSpan = this.footerColumnSpan;
			}

			//If we have a negative header column span, then don't display the header
			if (cellType == DataControlCellType.Header && this.headerColumnSpan == -1)
			{
				cell.Visible = false;
			}

			//If we have a negative footer column span, then don't display the footer
			if (cellType == DataControlCellType.Footer && this.footerColumnSpan == -1)
			{
				cell.Visible = false;
			}

			//If we are displaying a footer, need to attached an event handler for calculating the sum
			if (this.FooterField != null && this.FooterField != "" && (cellType == DataControlCellType.DataCell || cellType == DataControlCellType.Footer))
			{
				cell.DataBinding += new EventHandler(cell_DataBinding);
			}
		}

		/// <summary>
		/// Returns the underlying value for a specific bound data-item
		/// </summary>
		/// <param name="dataItem">The bound data-item</param>
		/// <returns>The integer representation of the value</returns>
		protected int GetUnderlyingValue(object dataItem)
		{
			//Handle NULLs correctly
			if (this.FooterField != null)
			{
				PropertyDescriptor hyperlinkFieldDesc = TypeDescriptor.GetProperties(dataItem).Find(this.FooterField, true);

				if (hyperlinkFieldDesc == null)
				{
					//Fail quietly so that VS.Net designer will work correctly
					return 0;
				}

				object dataValue = hyperlinkFieldDesc.GetValue(dataItem);

				if (dataValue.GetType() == typeof(int))
				{
					return (int)dataValue;
				}
				else
				{
					return 0;
				}
			}
			else
			{
				return 0;
			}
		}

		/// <summary>
		/// Called when the cell is databinding, used to calculate totals in the footer
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void cell_DataBinding(object sender, EventArgs e)
		{
			TableCell cell = (TableCell)sender;
			GridViewRow gridViewRow = (GridViewRow)cell.NamingContainer;

			//If we have an item, add to total
			if (gridViewRow.RowType == DataControlRowType.DataRow)
			{
				int dataValue = this.GetUnderlyingValue(gridViewRow.DataItem);
				runningTotal += dataValue;
			}

			//If we have a footer, bind total to column
			if (gridViewRow.RowType == DataControlRowType.Footer && runningTotal > 0)
			{
				cell.Text = runningTotal.ToString();
			}
		}
	}

	/// <summary>
	/// This class extends the BoundFieldEx to provide a FilterSortFieldex
	/// </summary>
	[ToolboxData("<{0}:FilterSortFieldEx runat=server></{0}:FilterSortFieldEx>")]
	public class FilterSortFieldEx : BoundFieldEx, ISubHeaderField
	{
		//Enumerations
		public enum FilterTypeEnum
		{
			DropDownList = 1,
			TextBox = 2,
			DateControl = 3,
			Flag = 4
		}

		/// <summary>
		/// Events
		/// </summary>
		public event FilterLookupDataBoundEventHandler LookupDataBound;

		/// <summary>
		/// Fields
		/// </summary>
		protected bool sortable = false;
		protected bool editable = false;
		protected bool nullable = true;
		protected string editableConditionField = "";
		protected string filterField = "";
		protected string filterLookupDataField = "";
		protected string filterLookupTextField = "";
		protected string filterLookupDataSourceID = "";
		protected FilterTypeEnum filterType = FilterTypeEnum.TextBox;
		protected Unit filterWidth = Unit.Empty;
		protected string navigateUrlFormat = "";
		protected string commandArgumentField = "";

		/// <summary>
		/// Constructor - delegates to the base class
		/// </summary>
		public FilterSortFieldEx()
			: base()
		{
		}

		/// <summary>
		/// Should the header row contain sorting arrows
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("Should the header row contain sorting arrows"),
		DefaultValue(false),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public virtual bool Sortable
		{
			get
			{
				return this.sortable;
			}
			set
			{
				this.sortable = value;
			}
		}

		/// <summary>
		/// Should the data rows be editable
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("Should the data rows be editable"),
		DefaultValue(false),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public virtual bool Editable
		{
			get
			{
				return this.editable;
			}
			set
			{
				this.editable = value;
			}
		}

		/// <summary>
		/// Should the data-rows be editable depending on whether another flag field is set
		/// </summary>
		/// <remarks>Used for fields that are not set at the folder level</remarks>
		[
		Bindable(true),
		Category("Appearance"),
		Description("Should the data rows be editable"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public virtual string EditableConditionField
		{
			get
			{
				return this.editableConditionField;
			}
			set
			{
				this.editableConditionField = value;
			}
		}

		/// <summary>
		///Sets the width of the appropriate filter control in the subheader
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("Sets the width of the appropriate filter control in the subheader"),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public virtual Unit FilterWidth
		{
			get
			{
				return this.filterWidth;
			}
			set
			{
				this.filterWidth = value;
			}
		}

		/// <summary>
		/// Should the data rows allow nullable data
		/// </summary>
		/// <remarks>
		/// This only affects the display of drop-down lists
		/// </remarks>
		[
		Bindable(true),
		Category("Appearance"),
		Description("Should the data rows allow nullable data"),
		DefaultValue(true),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public virtual bool Nullable
		{
			get
			{
				return this.nullable;
			}
			set
			{
				this.nullable = value;
			}
		}

		/// <summary>
		/// Defines the dataset field that stores the value to be filtered on
		/// </summary>
		[
		Bindable(true),
		Category("Data"),
		Description("Defines the dataset field that stores the value to be filtered on"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public virtual string FilterField
		{
			get
			{
				return this.filterField;
			}
			set
			{
				this.filterField = value;
			}
		}

		/// <summary>
		/// Defines the type of filter we should display
		/// </summary>
		[
		Bindable(true),
		Category("Appearance"),
		Description("Defines the type of filter we should display"),
		DefaultValue(FilterSortFieldEx.FilterTypeEnum.TextBox),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public virtual FilterTypeEnum FilterType
		{
			get
			{
				return this.filterType;
			}
			set
			{
				this.filterType = value;
			}
		}

		/// <summary>
		/// Gets and sets the data source of the sort lookup
		/// </summary>
		[
		Bindable(true),
		Category("Data"),
		Browsable(true),
		Description("Gets and sets the id of the filter lookup data source"),
		PersistenceMode(PersistenceMode.Attribute),
		RefreshProperties(RefreshProperties.Repaint),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
		]
		public virtual string FilterLookupDataSourceID
		{
			get
			{
				return this.filterLookupDataSourceID;
			}
			set
			{
				this.filterLookupDataSourceID = value;
			}
		}

		/// <summary>
		/// Defines the lookup dataset datamember to use in the drop-down list filter
		/// </summary>
		[
		Bindable(true),
		Category("Data"),
		Description("Defines the lookup dataset datamember to use in the drop-down list filter"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public virtual string FilterLookupDataField
		{
			get
			{
				return this.filterLookupDataField;
			}
			set
			{
				this.filterLookupDataField = value;
			}
		}

		/// <summary>
		/// Defines the lookup dataset datamember to use in the drop-down list filter
		/// </summary>
		[
		Bindable(true),
		Category("Data"),
		Description("Defines the lookup dataset datamember to use in the drop-down list filter"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public virtual string FilterLookupTextField
		{
			get
			{
				return this.filterLookupTextField;
			}
			set
			{
				this.filterLookupTextField = value;
			}
		}

		/// <summary>
		/// Defines the navigation Url format string that should be used if we want to display a hyperlink rather than just text for the data cells
		/// </summary>
		/// <remarks>
		/// If set, the hyperlink will be a real permalink hyperlink
		/// e.g. http://myserver/spira/yada/{0}.aspx where the {0} will be replaced by the CommandArgumentField
		/// </remarks>
		[
		Bindable(true),
		Category("Data"),
		Description("Defines the navigation url format string that the hyperlink should use"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public virtual string NavigateUrlFormat
		{
			get
			{
				return this.navigateUrlFormat;
			}
			set
			{
				this.navigateUrlFormat = value;
			}
		}

		/// <summary>
		/// Defines the dataset field that stores the command argument for the link button or the parameter
		/// to be inserted into the hyperlink URL instead of parameter {0}
		/// </summary>
		[
		Bindable(true),
		Category("Data"),
		Description("Defines the dataset field that stores the command argument for the link button or hyperlink"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public virtual string CommandArgumentField
		{
			get
			{
				return this.commandArgumentField;
			}
			set
			{
				this.commandArgumentField = value;
			}
		}

		/// <summary>
		/// Called when the cell is databinding, used to populate the edit values
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The event arguments</param>
		private void cell_DataBinding(object sender, EventArgs e)
		{
			TableCell cell = (TableCell)sender;
			GridViewRow gridViewRow = (GridViewRow)cell.NamingContainer;

			if (cell.Controls[0].GetType() == typeof(TextBoxEx))
			{
				//Get a handle to the text-box if we have one
				TextBoxEx textBox = (TextBoxEx)cell.Controls[0];

				//Get the underlying value being databound
				string dataValue = this.GetUnderlyingValue(gridViewRow.DataItem, this.FilterField);
				textBox.Text = dataValue;

				//Check to see if this field show be enabled based on an editable condition field
				if (this.EditableConditionField != null && this.EditableConditionField != "")
				{
					string flagValue = this.GetUnderlyingValue(gridViewRow.DataItem, this.EditableConditionField);
					//Folders sometimes don't allow certains values to be edited.
					if (flagValue == "Y")
					{
						textBox.Enabled = false;
						textBox.CssClass = "ReadOnlyTextBox";
					}
				}
			}
			if (cell.Controls[0].GetType() == typeof(DropDownListEx))
			{
				//Get a handle to the drop-down-list if we have one
				DropDownListEx dropDownList = (DropDownListEx)cell.Controls[0];

				//Get the underlying value being databound
				string dataValue = this.GetUnderlyingValue(gridViewRow.DataItem, this.FilterField);
				try
				{
					dropDownList.SelectedValue = dataValue;
				}
				catch (Exception)
				{
					//Fail Quietly
				}

				//Check to see if this field show be enabled based on an editable condition field
				if (this.EditableConditionField != null && this.EditableConditionField != "")
				{
					string flagValue = this.GetUnderlyingValue(gridViewRow.DataItem, this.EditableConditionField);
					//Folders sometimes don't allow certains values to be edited.
					if (flagValue == "Y")
					{
						dropDownList.Enabled = false;
					}
				}
			}
			if (cell.Controls[0].GetType() == typeof(DateControl))
			{
				//Get a handle to the date-control if we have one
				DateControl dateControl = (DateControl)cell.Controls[0];

				//Get the underlying value being databound
				string dataValue = this.GetUnderlyingValue(gridViewRow.DataItem, this.FilterField);
				dateControl.Text = dataValue;
			}
			if (cell.Controls[0].GetType() == typeof(HyperLinkEx) && !String.IsNullOrEmpty(this.navigateUrlFormat))
			{
				//Get a handle to the hyperlink if we have one
				HyperLinkEx hyperLink = (HyperLinkEx)cell.Controls[0];

				//Get the underlying value being databound
				string dataValue = this.GetUnderlyingValue(gridViewRow.DataItem, this.DataField);
				hyperLink.Text = Microsoft.Security.Application.Encoder.HtmlEncode(dataValue);

				//Set the URL
				string commandArgument = "";
				if (!String.IsNullOrEmpty(this.CommandArgumentField))
				{
					commandArgument = this.GetUnderlyingValue(gridViewRow.DataItem, this.CommandArgumentField);
				}

				//We use # when the navigate format is not specified but we still want a hyperlink not linkbutton
				hyperLink.NavigateUrl = String.Format(this.navigateUrlFormat, commandArgument);
			}
		}

		/// <summary>
		/// Returns the underlying value for a specific bound data-item
		/// </summary>
		/// <param name="dataItem">The bound data-item</param>
		/// <param name="fieldName">The field name</param>
		/// <returns>The integer representation of the value</returns>
		protected string GetUnderlyingValue(object dataItem, string fieldName)
		{
			//Handle null strings correctly
			if (fieldName != "")
			{
				if (fieldName.Contains("."))
				{
					//We have an entity navigation property
					string[] fields = fieldName.Split('.');
					object currentObject = dataItem;
					foreach (string field in fields)
					{
						if (currentObject != null)
						{
							object childobject = TypeDescriptor.GetProperties(currentObject).Find(field.Trim(), true).GetValue(currentObject);
							if (childobject == null)
							{
								//Just return null;
								return null;
								//throw new HttpException(String.Format("Field {0} was not found on the data item {1}", field, currentObject.GetType().ToString()));
							}
							currentObject = childobject;
						}
					}
					if (currentObject.GetType() == typeof(int))
					{
						return ((int)currentObject).ToString();
					}
					else if (currentObject.GetType() == typeof(string))
					{
						return (string)currentObject;
					}
					else if (currentObject.GetType() == typeof(bool))
					{
						return ((bool)currentObject).ToString();
					}
					else if (currentObject.GetType() == typeof(DateTime))
					{
						return ((DateTime)currentObject).ToString("d");
					}
					else
					{
						return "";
					}
				}
				else
				{
					PropertyDescriptor boundFieldDesc = TypeDescriptor.GetProperties(dataItem).Find(fieldName, true);

					if (boundFieldDesc == null)
					{
						//Fail quietly so that VS.Net designer will work correctly
						return "";
					}

					object dataValue = boundFieldDesc.GetValue(dataItem);

                    if (dataValue == null)
                    {
                        return "";
                    }
					if (dataValue.GetType() == typeof(int))
					{
						return ((int)dataValue).ToString();
					}
					else if (dataValue.GetType() == typeof(string))
					{
						return (string)dataValue;
					}
					else if (dataValue.GetType() == typeof(bool))
					{
						return ((bool)dataValue).ToString();
					}
					else if (dataValue.GetType() == typeof(DateTime))
					{
						return ((DateTime)dataValue).ToString("d");
					}
					else
					{
						return "";
					}
				}
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Adds text or controls to a subheader cell's controls collection.
		/// </summary>
		/// <param name="cell">A DataControlFieldCell that contains the text or controls of the DataControlField</param>
		/// <param name="rowState">One of the DataControlRowState values, specifying the state of the row that contains the DataControlFieldCell</param>
		/// <param name="rowIndex">The index of the row that the DataControlFieldCell is contained in</param>
		public override void InitializeSubHeaderCell(string gridViewClientId, DataControlFieldCell cell, DataControlRowState rowState, int rowIndex)
		{
			//First call the base method
			base.InitializeSubHeaderCell(gridViewClientId, cell, rowState, rowIndex);

			//First any provided sub-header literal text
			if (!String.IsNullOrEmpty(this.subHeaderText))
			{
				LiteralControl literalControl = new LiteralControl(this.subHeaderText + "&nbsp;");
				cell.Controls.Add(literalControl);
			}

			//Create the filter textbox, date-control or drop-down-list depending on the specified type
			if (this.FilterType == FilterTypeEnum.DropDownList || this.FilterType == FilterTypeEnum.Flag)
			{
				//Create a drop-down list
				DropDownListEx dropDownList = new DropDownListEx();
				dropDownList.CssClass = "DropDownList";
				dropDownList.ID = "ddl" + this.DataField + "Filter";
				if (this.FilterWidth == Unit.Empty)
				{
					dropDownList.Width = Unit.Pixel(100);
				}
				else
				{
					dropDownList.Width = this.FilterWidth;
				}
				dropDownList.MetaData = this.FilterField;
				dropDownList.DataSourceID = this.FilterLookupDataSourceID;
				dropDownList.DataValueField = this.FilterLookupDataField;
				dropDownList.DataTextField = this.FilterLookupTextField;
				dropDownList.NoValueItem = true;
				dropDownList.NoValueItemText = "-- Any --";
				cell.Controls.Add(dropDownList);

				//Handle the event when the dropdown is completely databound
				dropDownList.DataBound += new EventHandler(dropDownList_DataBound);
			}
			if (this.FilterType == FilterTypeEnum.TextBox)
			{
				//Create a textbox
				TextBoxEx textBox = new TextBoxEx();
				textBox.CssClass = "text-box";
				textBox.ID = "txt" + this.DataField + "Filter";
				if (this.FilterWidth == Unit.Empty)
				{
					textBox.Width = Unit.Pixel(80);
				}
				else
				{
					textBox.Width = this.FilterWidth;
				}
				textBox.MetaData = this.FilterField;
				cell.Controls.Add(textBox);
			}
			if (this.FilterType == FilterTypeEnum.DateControl)
			{
				//Create a date-control
				DateControl dateControl = new DateControl();
				dateControl.CssClass = "DatePicker";
				dateControl.ID = "dat" + this.DataField + "Filter";
				//The width of the filter for date control is fixed
				dateControl.Width = Unit.Pixel(90);
				dateControl.MetaData = this.FilterField;
				cell.Controls.Add(dateControl);
			}
		}

		/// <summary>
		/// Calls any registered event handlers
		/// </summary>
		/// <param name="sender">Sending object</param>
		/// <param name="e">Event arguments</param>
		void dropDownList_DataBound(object sender, EventArgs e)
		{
			//Now fire any events that are linked to the databound
			if (LookupDataBound != null)
			{
				DropDownListEx dropDownList = (DropDownListEx)sender;
				FilterLookupDataBoundEventArgs newEvent = new FilterLookupDataBoundEventArgs(dropDownList);
				LookupDataBound(this, newEvent);
			}
		}

		/// <summary>
		/// Databinds the specified field
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected override void OnDataBindField(object sender, EventArgs e)
		{
			TableCell cell = (TableCell)sender;
			GridViewRow gridViewRow = (GridViewRow)cell.NamingContainer;

			//If we have a flag field, need to display Y/N as Yes/No instead
			if (this.FilterType == FilterTypeEnum.Flag && gridViewRow.RowState != DataControlRowState.Edit)
			{
				string dataValue = this.GetUnderlyingValue(gridViewRow.DataItem, this.FilterField);
				cell.Text = (dataValue == "Y" || dataValue == "True") ? "Yes" : "No";
			}
			else
			{
				base.OnDataBindField(sender, e);
			}
		}

		/// <summary>
		/// Creates the individual tables cells from the gridview properties and items
		/// </summary>
		/// <param name="cell">A DataControlFieldCell that contains the text or controls of the DataControlField</param>
		/// <param name="cellType">One of the DataControlCellType values</param>
		/// <param name="rowState">One of the DataControlRowState values, specifying the state of the row that contains the DataControlFieldCell</param>
		/// <param name="rowIndex">The index of the row that the DataControlFieldCell is contained in</param>
		public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
		{
			//First call the base implementation - unless we have the edititem case
			if (cellType == DataControlCellType.DataCell)
			{
				if (rowState == DataControlRowState.Edit)
				{
					//Create the textbox, date-control or drop-down-list depending on the specified type
					//We can use the same enumeration as is used for filtering
					if (this.FilterType == FilterTypeEnum.DropDownList || this.FilterType == FilterTypeEnum.Flag)
					{
						//Create a drop-down list
						DropDownListEx dropDownList = new DropDownListEx();
						dropDownList.CssClass = "DropDownList";
						dropDownList.ID = "ddl" + this.DataField + "Edit";
						dropDownList.Style.Add("width", "100px");
						dropDownList.MetaData = this.FilterField;
						dropDownList.DataSourceID = this.FilterLookupDataSourceID;
						dropDownList.DataValueField = this.FilterLookupDataField;
						dropDownList.DataTextField = this.FilterLookupTextField;
						if (this.Nullable)
						{
							dropDownList.NoValueItem = true;
							dropDownList.NoValueItemText = Resources.Dialogs.Global_NoneDropDown;
						}
						else
						{
							dropDownList.NoValueItem = false;
						}
						cell.Controls.Add(dropDownList);
						//Display as disabled if field non-editable
						if (!this.Editable)
						{
							dropDownList.Enabled = false;
						}
					}
					if (this.FilterType == FilterTypeEnum.TextBox)
					{
						//Create a textbox
						TextBoxEx textBox = new TextBoxEx();
						textBox.CssClass = "text-box";
						textBox.ID = "txt" + this.DataField + "Edit";
						textBox.Style.Add("width", "80px");
						textBox.MetaData = this.FilterField;
						textBox.MaxLength = 255;    //Since they're mostly used for custom properties
						cell.Controls.Add(textBox);
						//Display as disabled if field non-editable
						if (!this.Editable)
						{
							textBox.Enabled = false;
						}
					}
					if (this.FilterType == FilterTypeEnum.DateControl)
					{
						//Create a date-control
						DateControl dateControl = new DateControl();
						dateControl.CssClass = "text-box";
						dateControl.ID = "dat" + this.DataField + "Edit";
						dateControl.Width = Unit.Pixel(70);
						dateControl.MetaData = this.FilterField;
						cell.Controls.Add(dateControl);
						//Display as disabled if field non-editable
						if (!this.Editable)
						{
							dateControl.Enabled = false;
						}
					}

					//Add the event-handler to load in the meta-data values when data-binding
					cell.DataBinding += new EventHandler(cell_DataBinding);
				}
				else
				{
					//Create a hyperlink control or just text
					if (String.IsNullOrEmpty(this.navigateUrlFormat))
					{
						base.InitializeCell(cell, cellType, rowState, rowIndex);
					}
					else
					{
						HyperLinkEx hyperLink = new HyperLinkEx();
						hyperLink.Enabled = cell.Enabled;
						cell.Controls.Add(hyperLink);
						//Add the code to populate the link when the control is databound
						cell.DataBinding += new EventHandler(cell_DataBinding);
					}
				}
			}
			else
			{
				base.InitializeCell(cell, cellType, rowState, rowIndex);
			}

			//See if we have a header row
			if (cellType == DataControlCellType.Header)
			{
				//First the header name
				LiteralControl literalControl = new LiteralControl(this.HeaderText + "&nbsp;");
				cell.Controls.Add(literalControl);

				//Now the two sort arrows if applicable
				if (this.Sortable)
				{
					ImageButtonEx sortAscending = new ImageButtonEx();
					sortAscending.ID = "btnSort" + this.DataField + "Ascending";
					sortAscending.CommandName = "SortColumns";
					sortAscending.CommandArgument = this.DataField + " ASC";
					sortAscending.AlternateText = String.Format(Resources.ServerControls.GridViewEx_SortByFieldAscending, this.HeaderText);
					sortAscending.ToolTip = String.Format(Resources.ServerControls.GridViewEx_SortByFieldAscending, this.HeaderText);
					sortAscending.ImageUrl = "Images/SortAscending.gif";
					sortAscending.HoverImageUrl = "Images/SortAscendingHover.gif";
					sortAscending.SelectedImageUrl = "Images/SortAscendingSelected.gif";
					cell.Controls.Add(sortAscending);

					ImageButtonEx sortDescending = new ImageButtonEx();
					sortAscending.ID = "btnSort" + this.DataField + "Descending";
					sortDescending.CommandName = "SortColumns";
					sortDescending.CommandArgument = this.DataField + " DESC";
					sortDescending.AlternateText = String.Format(Resources.ServerControls.GridViewEx_SortByFieldDescending, this.HeaderText);
					sortDescending.ToolTip = String.Format(Resources.ServerControls.GridViewEx_SortByFieldDescending, this.HeaderText);
					sortDescending.ImageUrl = "Images/SortDescending.gif";
					sortDescending.HoverImageUrl = "Images/SortDescendingHover.gif";
					sortDescending.SelectedImageUrl = "Images/SortDescendingSelected.gif";
					cell.Controls.Add(sortDescending);
				}
			}
		}
	}

	/// <summary>
	/// Delegate for the LookupDataBound event
	/// </summary>
	public delegate void FilterLookupDataBoundEventHandler(object sender, FilterLookupDataBoundEventArgs e);

	/// <summary>
	/// Event arguments for the LookupDataBound event
	/// </summary>
	public class FilterLookupDataBoundEventArgs : EventArgs
	{
		protected DropDownListEx filterDropDownList;

		public DropDownListEx FilterDropDownList
		{
			get
			{
				return this.filterDropDownList;
			}
			set
			{
				this.filterDropDownList = value;
			}
		}

		/// <summary>
		/// Constructor for event arguments
		/// </summary>
		public FilterLookupDataBoundEventArgs()
		{
		}

		/// <summary>
		/// Constructor for event arguments
		/// </summary>
		/// <param name="filterDropDownList">Reference to the dropdown list</param>
		public FilterLookupDataBoundEventArgs(DropDownListEx filterDropDownList)
		{
			this.filterDropDownList = filterDropDownList;
		}
	}
}
