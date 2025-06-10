using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using System.ComponentModel;

namespace Inflectra.SpiraTest.Web.ServerControls
{
	/// <summary>
	/// This class extends the default text-box to turn it into a color field with pop-up color picker
	/// </summary>
	[ToolboxData("<{0}:ColorPicker runat=server></{0}:ColorPicker>")]
	public class ColorPicker : System.Web.UI.WebControls.TextBox
	{
		protected string metaData;

		//Viewstate keys
		protected const string ViewStateKey_MetaData_Base = "MetaData_";

		/// <summary>
		/// Constructor - delegates to base class
		/// </summary>
		public ColorPicker() : base()
		{
			//Do Nothing
		}

		#region Properties

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

		#endregion

		#region Event Handlers

		/// <summary>
		/// Renders the control to the output stream
		/// </summary>
		/// <param name="writer">The text writer that receives the control output</param>
		protected override void Render(HtmlTextWriter writer)
		{
			//First render the enclosing table opening tags
			writer.WriteLine ("<table style=\"border-collapse: collapse; border: none;\"><tr>");

			//Render the color code symbol
			writer.WriteLine ("<td style=\"padding: 2px; border: none; vertical-align: middle;\">#</td>");

			//Now render out the text box inside a table cell
			writer.Write ("<td style=\"padding: 2px; border: none; vertical-align: middle;\">");
			base.Render (writer);
			writer.WriteLine ("</td>");

			//Now write out the box that displays the color selected
			writer.Write ("<td style=\"padding: 2px; border: none; vertical-align: middle;\">");
			writer.WriteLine ("<div id=\"" + this.ClientID + "_divColor\" onclick=\"window.open('" + ResolveClientUrl("~/Popups/ColorPicker.htm") + "?" +  this.ClientID + "', 'winColorPicker', 'resizable=no, help=no, status=no, toolbar=no, scroll=no, width=238, height=187, left=250, top=200');\" style=\"cursor: pointer; width: 30px; height: 15px; border: 1px solid black; background-color: #" + this.Text + "\"></div></td>");

			//Finally render the enclosing table closing tags
			writer.WriteLine ("</tr></table>");
		}

		#endregion
	}
}
