using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.Common
{
    /// <summary>
    /// Provides utility functions/extensions for dealing with booleans
    /// </summary>
    public static class Booleans
    {
        /// <summary>
        /// Returns the Y/N value used in dropdown lists
        /// </summary>
        /// <param name="flagValue">The boolean value</param>
        /// <returns>Y/N for true/false</returns>
        public static string ToFlagValue(this bool flagValue)
        {
            return (flagValue) ? "Y" : "N";
        }
    }
}
