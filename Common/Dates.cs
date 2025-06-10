using System;
using System.Globalization;

namespace Inflectra.SpiraTest.Common
{
	public static class Dates
	{
        //Display format strings
        public const string FORMAT_DATE = "{0:d-MMM-yyyy}";	//Use custom format that works in all cultures

        /// <summary>
        /// Converts a UTC date to a local date
        /// </summary>
        /// <param name="utcDate">The UTC Date</param>
        /// <param name="utcOffset">The UTC offset</param>
        /// <returns>The local date</returns>
        public static DateTime ToLocalDate(this DateTime utcDate, double utcOffset)
        {
            //If we have a local date already, do nothing
            if (utcDate.Kind == DateTimeKind.Local)
            {
                return utcDate;
            }

            return utcDate.AddHours(utcOffset);
        }

		/// <summary>Changes a given date to a nice string if under 24 hours ("1 hour 23 minutes ago") or the normal date string if greater than a day. Dates passed must be local NOT UTC!</summary>
		/// <param name="date1">The date to convert, in local time</param>
		/// <param name="compareDate">The date to compare against, in local time!</param>
		/// <param name="defaultFormat">The default date time format (i.e. "T" for LongTimePattern or a full specification like "M/d/y"). Default: "F" (FullDateTimePattern)</param>
		/// <returns>The date string.</returns>
		public static string ToNiceString(this DateTime date1, DateTime compareDate, string defaultFormat = "")
		{
			if (string.IsNullOrWhiteSpace(defaultFormat)) //Get the default format.
				defaultFormat = CultureInfo.CurrentCulture.DateTimeFormat.FullDateTimePattern;

			//Set the default return.
			string retString = date1.ToString(defaultFormat);

			//First get the time difference to see if we need to make it nice.
			TimeSpan timeDiff = compareDate - date1;
			if (timeDiff.TotalHours < 24)
			{
				string strHours = "";
				string strMinutes = "";

				if (timeDiff.TotalMinutes > 0)
				{
					//Get the hours string.
					strHours = string.Format(
						"{0} {1}",
						timeDiff.Hours.ToString(),
						((timeDiff.Hours > 1) ? Resources.Main.global_hours : Resources.Main.global_hour)
					);
					strMinutes = string.Format(
                        "{0} {1}",
						timeDiff.Minutes.ToString(),
						((timeDiff.Minutes > 1) ? Resources.Main.global_minutes : Resources.Main.global_minute)
					);
				}
				else
				{
					strMinutes = "< 1 " + Resources.Main.global_minute;
				}

				//Now generate the real string.
				string strNice = string.Format(
					"{0} {1} {2}",
					((timeDiff.Hours >= 1) ? strHours : ""),
					strMinutes,
					Resources.Main.global_ago
				);

				//Trim extra spaces.
				retString = strNice.Trim();
			}

			return retString;
		}
	}
}
