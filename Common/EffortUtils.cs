using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.Common
{
    /// <summary>
    /// Utilities for managing/handling effort values
    /// </summary>
    public static class EffortUtils
    {
        /// <summary>
        /// Converts an effort value from hours+minutes to just minutes
        /// </summary>
        /// <param name="hours">The hour component</param>
        /// <param name="minutes">The minute component</param>
        /// <returns>The total effort in minutes</returns>
        public static int GetEffortInMinutes(int hours, int minutes)
        {
            return hours * 60 + minutes;
        }

        /// <summary>
        /// Converts an effort value from minutes to hours+minutes
        /// </summary>
        /// <param name="minutes">The total effort in minutes</param>
        /// <returns>The effort hours component</returns>
        public static int GetEffortHoursComponent(int minutes)
        {
            return minutes / 60;
        }

        /// <summary>
        /// Converts an effort value from minutes to hours+minutes
        /// </summary>
        /// <param name="minutes">The total effort in minutes</param>
        /// <returns>The effort minutes component</returns>
        public static int GetEffortMinutesComponent(int minutes)
        {
            return minutes % 60;
        }
    }
}
