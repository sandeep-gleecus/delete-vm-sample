using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Inflectra.SpiraTest.Common;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Inflectra.SpiraTest.Web.Services.v4_0.DataObjects
{
    /// <summary>
    /// Represents a single filter that can be applied to a Retrieve query
    /// </summary>
    [DataContract]
    public class RemoteFilter
    {
        /// <summary>
        /// The name of the field being filtered on. For standard fields it would be something like 'RequirementStatusId',
        /// for custom properties, it would be of the format: 'Custom_01' not the display name.
        /// </summary>
        [DataMember]
        public string PropertyName;

        /// <summary>
        /// The filter value for fields that contain integers
        /// </summary>
        /// <remarks>
        /// Used when you only want to filter on a single value
        /// </remarks>
        [DataMember]
        public Nullable<int> IntValue;

        /// <summary>
        /// The filter value for fields that contain strings
        /// </summary>
        /// <remarks>
        /// This will perform a LIKE wildcard match on the string
        /// </remarks>
        [DataMember]
        public String StringValue;

        /// <summary>
        /// The filter value for fields that contain integers
        /// </summary>
        /// <remarks>
        /// Used when you only want to filter on a set of multiple values
        /// </remarks>
        [DataMember]
        public MultiValueFilter MultiValue;

        /// <summary>
        /// The filter value for date/time fields where you want to filter on a date range
        /// </summary>
        [DataMember]
        public DateRange DateRangeValue;
    }

    /// <summary>
    /// Represents a multi-values filter entry
    /// </summary>
    [DataContract]
    public class MultiValueFilter
    {
        //Internally the data is stored in a different class that is not exposed as part of the service
        private Common.MultiValueFilter internalMultiValueFilter = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public MultiValueFilter()
        {
            this.internalMultiValueFilter = new Common.MultiValueFilter();
        }

        /// <summary>
        /// Called by WCF instead of constructor when class created from deserialized WCF data
        /// </summary>
        /// <param name="sc"></param>
        [OnDeserializing]
        private void EnsureInternalObjectCreated(StreamingContext sc)
        {
            this.internalMultiValueFilter = new Common.MultiValueFilter();
        }

        /// <summary>
        /// Gets the internal representation of the multi-value filter
        /// </summary>
        /// <returns></returns>
        internal Common.MultiValueFilter Internal
        {
            get
            {
                return this.internalMultiValueFilter;
            }
        }

        /// <summary>
        /// Contains the list of specified values to filter on
        /// </summary>
        [DataMember]
        public int[] Values
        {
            get
            {
                return this.internalMultiValueFilter.Values.ToArray();
            }
            set
            {
                this.internalMultiValueFilter.Values.Clear();
                this.internalMultiValueFilter.Values.AddRange(value);
            }
        }

        /// <summary>
        /// Do we have the special case of a filter for (None) - i.e. all records that have no value set
        /// </summary>
        [DataMember]
        public bool IsNone
        {
            get
            {
                return this.internalMultiValueFilter.IsNone;
            }
            set
            {
                this.internalMultiValueFilter.IsNone = value;
            }
        }
    }

    /// <summary>
    /// Represents a date-range that is used in filters, etc.
    /// </summary>
    [DataContract]
    public class DateRange
    {
        //Internally the data is stored in a different class that is not exposed as part of the service
        private Common.DateRange internalDateRange = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public DateRange()
        {
            this.internalDateRange = new Common.DateRange();
        }

        /// <summary>
        /// Called by WCF instead of constructor when class created from deserialized WCF data
        /// </summary>
        /// <param name="sc"></param>
        [OnDeserializing]
        private void EnsureInternalObjectCreated(StreamingContext sc)
        {
            this.internalDateRange = new Common.DateRange();
        }

        /// <summary>
        /// Gets the internal representation of the date-range filter
        /// </summary>
        /// <returns></returns>
        internal Common.DateRange Internal
        {
            get
            {
                return this.internalDateRange;
            }
        }

        /// <summary>
        /// Do we want to consider times when filtering
        /// </summary>
        /// <remarks>This is not used interally by the class, just tracked for the client's benefit</remarks>
        [DataMember]
        public bool ConsiderTimes
        {
            get
            {
                return internalDateRange.ConsiderTimes;
            }
            set
            {
                internalDateRange.ConsiderTimes = value;
            }
        }

        /// <summary>
        /// The starting date of the date-range
        /// </summary>
        [DataMember]
        public Nullable<DateTime> StartDate
        {
            get
            {
                return internalDateRange.StartDate;
            }
            set
            {
                internalDateRange.StartDate = value;
            }
        }

        /// <summary>
        /// The ending date of the date-range
        /// </summary>
        [DataMember]
        public Nullable<DateTime> EndDate
        {
            get
            {
                return internalDateRange.EndDate;
            }
            set
            {
                internalDateRange.EndDate = value;
            }
        }
    }

}
