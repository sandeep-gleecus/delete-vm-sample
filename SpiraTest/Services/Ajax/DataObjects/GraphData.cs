using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    /// <summary>
    /// Represents the graphing data returned by the GraphingService AJAX web service
    /// </summary>
    [DataContract(Namespace = "tst.dataObjects")]
    public class GraphData
    {
        /// <summary>
        /// Any graph-specific options
        /// </summary>
        [DataMember(Name="options", EmitDefaultValue=false)]
        public string Options
        {
            get;
            set;
        }

        /// <summary>
        /// The caption for the x-axis (if any)
        /// </summary>
        [DataMember]
        public string XAxisCaption
        {
            get;
            set;
        }

        /// <summary>
        /// The jqplot interval name for the x-axis (date-range graphs only)
        /// </summary>
        [DataMember]
        public string Interval
        {
            get;
            set;
        }

        /// <summary>
        /// The list of all the x-axis points returned in the data
        /// </summary>
        [DataMember]
        public List<GraphAxisPosition> XAxis
        {
            get
            {
                return this.xaxis;
            }
            set
            {
                this.xaxis = value;
            }
        }
        protected List<GraphAxisPosition> xaxis = new List<GraphAxisPosition>();

        /// <summary>
        /// List of dataseries being returned
        /// </summary>
        [DataMember]
        public List<DataSeries> Series
        {
            get
            {
                return this.series;
            }
            set
            {
                this.series = value;
            }
        }
        protected List<DataSeries> series = new List<DataSeries>();

		/// <summary>
		/// List of categories being returned
		/// </summary>
		[DataMember]
		public List<string> Categories
		{
			get
			{
				return this.categories;
			}
			set
			{
				this.categories = value;
			}
		}
		protected List<string> categories = new List<string>();
	}

    /// <summary>
    /// Represents a single data-series in the system
    /// </summary>
    [DataContract(Namespace = "tst.dataObjects")]
    public class DataSeries
    {
        /// <summary>
        /// The internal name of the series
        /// </summary>
        [DataMember]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// The display caption of the series
        /// </summary>
        [DataMember]
        public string Caption
        {
            get;
            set;
        }

        /// <summary>
        /// The HTML color code of the series - leave blank if using the graph's defaults
        /// </summary>
        [DataMember]
        public string Color
        {
            get;
            set;
        }

        /// <summary>
        /// The id of the type of graph series (only used for snapshot graphs currently)
        /// </summary>
        [DataMember]
        public Nullable<int> Type
        {
            get;
            set;
        }

        /// <summary>
        /// Used when the series only has a single value;
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int? Value
        {
            get;
            set;
        }

        /// <summary>
        /// The list of values, correlated against the axis datapoints
        /// </summary>
        [DataMember]
        public JsonDictionaryOfDecimals Values
        {
            get
            {
                return this.values;
            }
            set
            {
                this.values = value;
            }

        }
        protected JsonDictionaryOfDecimals values = new JsonDictionaryOfDecimals();

		[DataMember]
		public List<int> IntegerValues
		{
			get
			{
				return this.integerValues;
			}
			set
			{
				this.integerValues = value;
			}
		}

		protected List<int> integerValues = new List<int>();


		[DataMember]
		public List<ColumnData> Columns
		{
			get
			{
				return this.columns;
			}
			set
			{
				this.columns = value;
			}
		}
		protected List<ColumnData> columns = new List<ColumnData>();
	}

    /// <summary>
    /// A single graph axis
    /// </summary>
    [DataContract(Namespace = "tst.dataObjects")]
    public class GraphAxisPosition
    {
        /// <summary>
        /// The id for the axis point
        /// (used to correlate the various results)
        /// </summary>
        [DataMember]
        public int Id
        {
            get;
            set;
        }

        /// <summary>
        /// Used when the axis consists of named positions
        /// </summary>
        [DataMember]
        public string StringValue
        {
            get;
            set;
        }

        /// <summary>
        /// Used when the axis consists of date-positions
        /// </summary>
        [DataMember]
        public Nullable<DateTime> DateValue
        {
            get;
            set;
        }
    }

	[DataContract(Namespace = "tst.dataObjects")]
	public class ColumnData
	{
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public List<int> Values
		{
			get
			{
				return this.values;
			}
			set
			{
				this.values = value;
			}
		}

		protected List<int> values = new List<int>();
	}
}
