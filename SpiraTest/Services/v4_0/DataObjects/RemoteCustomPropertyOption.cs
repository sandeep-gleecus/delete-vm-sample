using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Services.v4_0.DataObjects
{
    /// <summary>
    /// Represents a single project custom property option entry.
    /// Examples of options include: maximum length, minimum value, etc.
    /// </summary>
    public class RemoteCustomPropertyOption
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public RemoteCustomPropertyOption()
        {
        }

        /// <summary>
        /// Creates a new remote custom property option object from the corresponding entity
        /// </summary>
        /// <param name="customPropertyOptionValue">The custom property option value entity</param>
        internal RemoteCustomPropertyOption(CustomPropertyOptionValue customPropertyOptionValue)
        {
            //Populate the fields
            this.CustomPropertyOptionId = customPropertyOptionValue.CustomPropertyOptionId;
            this.Value = customPropertyOptionValue.Value;
        }

        /// <summary>
        /// The id of the custom property option. Allowed values are:
        ///     AllowEmpty = 1,
        ///     MaxLength = 2,
        ///     MinLength = 3,
        ///     RichText = 4,
        ///     Default = 5,
        ///     MaxValue = 6,
        ///     MinValue = 7,
        ///     Precision = 8
        /// </summary>
        public int CustomPropertyOptionId;

        /// <summary>
        /// The value of the custom property option
        /// </summary>
        public string Value;
    }
}