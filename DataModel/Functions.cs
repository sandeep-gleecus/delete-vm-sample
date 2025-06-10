using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Contains the custom EF4 functions used for making the conceptual model easier to use
    /// </summary>
    public static class Functions
    {
        [EdmFunction("Inflectra.SpiraTest.DataModel", "IsActive")]
        public static bool IsActive(this ProductType p)
        {
            throw new NotSupportedException("This function can only be used in a LINQ query");
        }

        /// <summary>
        /// Used to filter a custom property decimal text field
        /// </summary>
        /// <param name="operand">The field being filtered</param>
        /// <param name="filterValue">The filter value</param>
        /// <returns>True/False</returns>
        [EdmFunction("Inflectra.SpiraTest.DataModel.Store", "FN_CUSTOM_PROPERTY_GREATER_THAN_DECIMAL")]
        public static bool CustomPropertyGreaterThanOrEqualDecimal(string operand, decimal filterValue)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        /// <summary>
        /// Used to filter a custom property decimal text field
        /// </summary>
        /// <param name="operand">The field being filtered</param>
        /// <param name="filterValue">The filter value</param>
        /// <returns>True/False</returns>
        [EdmFunction("Inflectra.SpiraTest.DataModel.Store", "FN_CUSTOM_PROPERTY_LESS_THAN_DECIMAL")]
        public static bool CustomPropertyLessThanOrEqualDecimal(string operand, decimal filterValue)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        /// <summary>
        /// Used to filter a custom property decimal text field
        /// </summary>
        /// <param name="operand">The field being filtered</param>
        /// <param name="filterValue">The filter value</param>
        /// <returns>True/False</returns>
        [EdmFunction("Inflectra.SpiraTest.DataModel.Store", "FN_CUSTOM_PROPERTY_EQUALS_DECIMAL")]
        public static bool CustomPropertyEqualsDecimal(string operand, decimal filterValue)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        /// <summary>
        /// Used to filter a custom property int text field
        /// </summary>
        /// <param name="operand">The field being filtered</param>
        /// <param name="filterValue">The filter value</param>
        /// <returns>True/False</returns>
        [EdmFunction("Inflectra.SpiraTest.DataModel.Store", "FN_CUSTOM_PROPERTY_GREATER_THAN_INT")]
        public static bool CustomPropertyGreaterThanOrEqualInt(string operand, int filterValue)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        /// <summary>
        /// Used to filter a custom property int text field
        /// </summary>
        /// <param name="operand">The field being filtered</param>
        /// <param name="filterValue">The filter value</param>
        /// <returns>True/False</returns>
        [EdmFunction("Inflectra.SpiraTest.DataModel.Store", "FN_CUSTOM_PROPERTY_LESS_THAN_INT")]
        public static bool CustomPropertyLessThanOrEqualInt(string operand, int filterValue)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        /// <summary>
        /// Used to filter a custom property int text field
        /// </summary>
        /// <param name="operand">The field being filtered</param>
        /// <param name="filterValue">The filter value</param>
        /// <returns>True/False</returns>
        [EdmFunction("Inflectra.SpiraTest.DataModel.Store", "FN_CUSTOM_PROPERTY_EQUALS_INT")]
        public static bool CustomPropertyEqualsInt(string operand, int filterValue)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        /// <summary>
        /// Used to filter a custom property date/time text field
        /// </summary>
        /// <param name="operand">The field being filtered</param>
        /// <param name="filterValue">The filter value</param>
        /// <returns>True/False</returns>
        [EdmFunction("Inflectra.SpiraTest.DataModel.Store", "FN_CUSTOM_PROPERTY_GREATER_THAN_DATETIME")]
        public static bool CustomPropertyGreaterThanOrEqualDateTime(string operand, DateTime filterValue, int utcOffsetHours, int utcOffsetMinutes, bool considerTimes)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }

        /// <summary>
        /// Used to filter a custom property date/time text field
        /// </summary>
        /// <param name="operand">The field being filtered</param>
        /// <param name="filterValue">The filter value</param>
        /// <returns>True/False</returns>
        [EdmFunction("Inflectra.SpiraTest.DataModel.Store", "FN_CUSTOM_PROPERTY_LESS_THAN_DATETIME")]
        public static bool CustomPropertyLessThanOrEqualDateTime(string operand, DateTime filterValue, int utcOffsetHours, int utcOffsetMinutes, bool considerTimes)
        {
            throw new NotSupportedException("Direct calls are not supported.");
        }
    }
}
