using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v5_0.DataObjects
{
    /// <summary>
    /// Represents a single custom property instance associated with an artifact
    /// </summary>
    public class RemoteArtifactCustomProperty
    {
        /// <summary>
        /// The number of the custom property field
        /// </summary>
        public int PropertyNumber;

        /// <summary>
        /// The value of a string custom property
        /// </summary>
        public string StringValue;

        /// <summary>
        /// The value of an integer custom property
        /// </summary>
        public int? IntegerValue;

        /// <summary>
        /// The value of a boolean custom property
        /// </summary>
        public bool? BooleanValue;

        /// <summary>
        /// The value of a date-time custom property
        /// </summary>
        public DateTime? DateTimeValue;

        /// <summary>
        /// The value of a decimal custom property
        /// </summary>
        public Decimal? DecimalValue;

        /// <summary>
        /// The value of a multi-list custom property
        /// </summary>
        public List<int> IntegerListValue;

        /// <summary>
        /// The associated custom property definition (read-only)
        /// </summary>
        public RemoteCustomProperty Definition;
    }
}