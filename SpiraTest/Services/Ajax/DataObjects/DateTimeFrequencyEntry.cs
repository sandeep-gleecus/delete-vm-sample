using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    /// <summary>
    /// Represents a single datetime count entry in a data series
    /// </summary>
    [DataContract(Namespace = "tst.dataObjects")]
    public class DateTimeFrequencyEntry
    {
        /// <summary>
        /// The Date Time interval as a date
        /// </summary>
        [DataMember(Name="interval")]
        public DateTime Interval { get; set; }

        /// <summary>
        /// The Date Time interval as a localized caption
        /// </summary>
        [DataMember(Name = "caption")]
        public string Caption { get; set; }

        /// <summary>
        /// The count for the interval
        /// </summary>
        [DataMember(Name = "count")]
        public int Count { get; set; }
    }
}