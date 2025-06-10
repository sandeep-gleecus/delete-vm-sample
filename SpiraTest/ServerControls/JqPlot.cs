using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Classes;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.DataModel;
using System.Globalization;

namespace Inflectra.SpiraTest.Web.ServerControls
{
    /// <summary>
    /// Generates an ASP.NET AJAX wrapper control around the C3/J3 library.
    /// This makes it easier to use with an ASP.NET web form and also makes getting data
    /// from a WCF service easier.
    /// </summary>
    /// <remarks>
    /// It used to use the older jqPlot rendering library hence the name.
    /// </remarks>
    [
    ToolboxData("<{0}:JqPlot runat=server></{0}:JqPlot>")
    ]
    public class JqPlot : WebControl, IScriptControl
    {
        #region Enumerations

        /// <summary>
        /// The different custom graph types
        /// </summary>
        public enum CustomGraphTypeEnum
        {
            Bar = 0,
            Line = 1,
            Donut = 2
        }

        #endregion

        protected Dictionary<string, object> filters;

        #region Properties

        /// <summary>
        /// Contains the id of the selected graph we're generating
        /// </summary>
        [
        Category("Context"),
        DefaultValue(Graph.GraphEnum.None)
        ]
        public Graph.GraphEnum SelectedGraph
        {
            get
            {
                object obj = ViewState["SelectedGraph"];

                return (obj == null) ? Graph.GraphEnum.None : (Graph.GraphEnum)obj;
            }
            set
            {
                ViewState["SelectedGraph"] = value;
            }
        }

        /// <summary>
        /// Contains the date range for the graph (only used for date-range graphs)
        /// </summary>
        [
        Category("Context"),
        DefaultValue(null)
        ]
        public DateRange SelectedDateRange
        {
            get
            {
                object obj = ViewState["SelectedDateRange"];

                return (obj == null) ? null : (DateRange)obj;
            }
            set
            {
                ViewState["SelectedDateRange"] = value;
            }
        }

        /// <summary>
        /// Contains the x-axis field for the graph (only used for summary graphs)
        /// </summary>
        [
        Category("Context"),
        DefaultValue("")
        ]
        public string XAxisField
        {
            get
            {
                object obj = ViewState["XAxisField"];

                return (obj == null) ? "" : (string)obj;
            }
            set
            {
                ViewState["XAxisField"] = value;
            }
        }

        /// <summary>
        /// Contains the group-by field for the graph (only used for summary graphs)
        /// </summary>
        [
        Category("Context"),
        DefaultValue("")
        ]
        public string GroupByField
        {
            get
            {
                object obj = ViewState["GroupByField"];

                return (obj == null) ? "" : (string)obj;
            }
            set
            {
                ViewState["GroupByField"] = value;
            }
        }

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
        /// The title of the graph
        /// </summary>
        [
        Category("Data"),
        DefaultValue(""),
        Description("The title of the graph")
        ]
        public string Title
        {
            get
            {
                object obj = ViewState["Title"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["Title"] = value;
            }
        }

        /// <summary>
        /// The Css class to use for the datagrid of graph values
        /// </summary>
        [
        Category("Appearance"),
        DefaultValue(""),
        Description("The Css class to use for the datagrid of graph values")
        ]
        public string DataGridCssClass
        {
            get
            {
                object obj = ViewState["DataGridCssClass"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["DataGridCssClass"] = value;
            }
        }

        /// <summary>
        /// The ID of the web part containing the graph (if applicable)
        /// </summary>
        [
        Category("Appearance"),
        DefaultValue(""),
        Description("The ID of the web part containing the graph (if applicable)")
        ]
        public string WebPartUniqueId
        {
            get
            {
                object obj = ViewState["WebPartUniqueId"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["WebPartUniqueId"] = value;
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

        [
        Category("Behavior"),
        DefaultValue(""),
        Description("The ClientID of the server control that we want to use to display error messages (div, span, etc.)")
        ]
        public string ErrorMessageClientID
        {
            get
            {
                object obj = ViewState["ErrorMessageClientID"];

                return (obj == null) ? string.Empty : (string)obj;
            }
            set
            {
                ViewState["ErrorMessageClientID"] = value;
            }
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
                return HtmlTextWriterTag.Div;
            }
        }

        #endregion

        #region IScriptControl Members

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            ScriptControlDescriptor descriptor = new ScriptControlDescriptor("Inflectra.SpiraTest.Web.ServerControls.JqPlot", this.ClientID);

            if (!string.IsNullOrEmpty(WebServiceClass))
            {
                //We pass it as a script property because this control expects it as a javascript object not a string
                //unlike some of the other (older) controls
                descriptor.AddScriptProperty("webServiceClass", WebServiceClass);
            }
            if (ProjectId > -1)
            {
                descriptor.AddProperty("projectId", ProjectId);
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

            if (!String.IsNullOrEmpty(ErrorMessageClientID))
            {
                descriptor.AddProperty("errorMessageControlId", ErrorMessageClientID);
            }
            else if (!string.IsNullOrEmpty(ErrorMessageControlId))
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
            if (!String.IsNullOrEmpty(this.Title))
            {
                descriptor.AddProperty("title", this.Title);
            }
            if (!String.IsNullOrEmpty(this.DataGridCssClass))
            {
                descriptor.AddProperty("dataGridCssClass", this.DataGridCssClass);
            }
            descriptor.AddProperty("graphId", (int)this.SelectedGraph);
            if (!String.IsNullOrEmpty(WebPartUniqueId))
            {
                descriptor.AddProperty("webPartUniqueId", WebPartUniqueId);
            }
            
            //See which type of graph we have
            descriptor.AddProperty("graphType", (int)this.GraphType);
            if (GraphType == Graph.GraphTypeEnum.DateRangeGraphs)
            {
                if (SelectedDateRange != null)
                {
                    descriptor.AddProperty("dateRange", this.SelectedDateRange.ToString());
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
                descriptor.AddProperty("dateFormat", dateFormat);
            }
            if (GraphType == Graph.GraphTypeEnum.SummaryGraphs)
            {
                if (ArtifactType != DataModel.Artifact.ArtifactTypeEnum.None)
                {
                    descriptor.AddProperty("artifactTypeId", (int)this.ArtifactType);
                }
                if (!String.IsNullOrEmpty(XAxisField))
                {
                    descriptor.AddProperty("xAxisField", this.XAxisField);
                }
                if (!String.IsNullOrEmpty(GroupByField))
                {
                    descriptor.AddProperty("groupByField", this.GroupByField);
                }
            }
            if (GraphType == Graph.GraphTypeEnum.CustomGraphs)
            {
                //Include the custom graph id and type
                descriptor.AddProperty("customGraphType", (int)this.CustomGraphType);
                if (this.CustomGraphId.HasValue)
                {
                    descriptor.AddProperty("customGraphId", this.CustomGraphId.Value);
                }
            }
            if (!GraphWidth.IsEmpty)
            {
                descriptor.AddProperty("graphWidth", this.GraphWidth.ToString());
            }
            if (!GraphHeight.IsEmpty)
            {
                descriptor.AddProperty("graphHeight", this.GraphHeight.ToString());
            }

            //Add any custom filters - need to serialize the values into strings
            if (this.filters != null)
            {
                descriptor.AddScriptProperty("filters", JsonDictionaryConvertor.Serialize(GlobalFunctions.SerializeCollection(this.filters)));
            }

            //Add the path to data download URL and base url
            descriptor.AddProperty("baseUrl", this.ResolveUrl("~"));
            descriptor.AddProperty("downloadUrl", UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Graphs, -2, -2, "Download")));

            yield return descriptor;
        }

        /// <summary>
        /// The type of artifact (used in summary charts)
        /// </summary>
        [
        Bindable(true),
        Category("Context"),
        Description("The type of artifact"),
        PersistenceMode(PersistenceMode.Attribute),
        DefaultValue(DataModel.Artifact.ArtifactTypeEnum.None)
        ]
        public DataModel.Artifact.ArtifactTypeEnum ArtifactType
        {
            get
            {
                if (ViewState["ArtifactType"] == null)
                {
                    return DataModel.Artifact.ArtifactTypeEnum.None;
                }
                else
                {
                    return (DataModel.Artifact.ArtifactTypeEnum)ViewState["ArtifactType"];
                }
            }
            set
            {
                ViewState["ArtifactType"] = value;
            }
        }

        /// <summary>
        /// The type of graph
        /// </summary>
        [
        Bindable(true),
        Category("Context"),
        Description("The type of graph"),
        PersistenceMode(PersistenceMode.Attribute),
        DefaultValue(Graph.GraphTypeEnum.SnapshotGraphs)
        ]
        public Graph.GraphTypeEnum GraphType
        {
            get
            {
                if (ViewState["GraphType"] == null)
                {
                    return Graph.GraphTypeEnum.SnapshotGraphs;
                }
                else
                {
                    return (Graph.GraphTypeEnum)ViewState["GraphType"];
                }
            }
            set
            {
                ViewState["GraphType"] = value;
            }
        }

        /// <summary>
        /// The id of the custom graph
        /// </summary>
        [
        Bindable(true),
        Category("Context"),
        Description("The type of graph"),
        PersistenceMode(PersistenceMode.Attribute),
        DefaultValue(null)
        ]
        public int? CustomGraphId
        {
            get
            {
                return (int?)ViewState["CustomGraphId"];
            }
            set
            {
                ViewState["CustomGraphId"] = value;
            }
        }

        /// <summary>
        /// The type of custom graph
        /// </summary>
        [
        Bindable(true),
        Category("Context"),
        Description("The type of graph"),
        PersistenceMode(PersistenceMode.Attribute),
        DefaultValue(CustomGraphTypeEnum.Bar)
        ]
        public CustomGraphTypeEnum CustomGraphType
        {
            get
            {
                if (ViewState["CustomGraphType"] == null)
                {
                    return CustomGraphTypeEnum.Bar;
                }
                else
                {
                    return (CustomGraphTypeEnum)ViewState["CustomGraphType"];
                }
            }
            set
            {
                ViewState["CustomGraphType"] = value;
            }
        }

        /// <summary>
        /// The height of the body of the graph
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        Description("The height of the graph"),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public Unit GraphHeight
        {
            get
            {
                if (ViewState["GraphHeight"] == null)
                {
                    return Unit.Empty;
                }
                else
                {
                    Unit u = (Unit)ViewState["GraphHeight"];
                    return u;
                }
            }

            set
            {
                ViewState["GraphHeight"] = value;
            }
        }

        /// <summary>
        /// The with of the body of the graph
        /// </summary>
        [
        Bindable(true),
        Category("Appearance"),
        Description("The width of the body of the graph"),
        PersistenceMode(PersistenceMode.Attribute)
        ]
        public Unit GraphWidth
        {
            get
            {
                if (ViewState["GraphWidth"] == null)
                {
                    return Unit.Empty;
                }
                else
                {
                    Unit u = (Unit)ViewState["GraphWidth"];
                    return u;
                }
            }

            set
            {
                ViewState["GraphWidth"] = value;
            }
        }

        // Generate the script reference
        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            //The Inflectra server control client libraries
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.JqPlot.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(typeof(ServerControlCommon), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.DialogBoxPanel.js"));

            //The main C3/D3 scripts
            //No need - on the Master Page already
            //yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ClientScripts.d3.min.js"));
            //yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ClientScripts.c3.min.js"));

            //The scripts used for exporting the graphs as images
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.rgbcolor.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.StackBlur.js"));
            yield return new ScriptReference(Page.ClientScript.GetWebResourceUrl(this.GetType(), "Inflectra.SpiraTest.Web.ServerControls.ClientScripts.canvg.js"));
            yield return new ScriptReference("~/JqPlot/canvas2image.js");
            yield return new ScriptReference("~/JqPlot/base64.js");
        }

        #endregion

        /// <summary>
        /// Allows the passing in of a collection of filters
        /// </summary>
        /// <param name="filters">The collection of filters</param>
        public void SetFilters(Dictionary<string, object> filters)
        {
            this.filters = filters;
        }
    }
}
