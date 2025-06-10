using System;
using System.Collections.Generic;
using System.Text;

namespace Inflectra.SpiraTest.PlugIns
{
    /// <summary>
    /// Represents a date-range that is used in plug-in filters, etc.
    /// </summary>
    /// <remarks>We don't use the one in Common.dll to avoid a dependency on that assembly</remarks>
    [Serializable]
    public class DateRange : IEquatable<DateRange>
    {
        Nullable<DateTime> startDate, endDate;
        public DateRange() : this(new Nullable<DateTime>(), new Nullable<DateTime>()) { }
        public DateRange(Nullable<DateTime> startDate, Nullable<DateTime> endDate)
        {
            AssertStartDateFollowsEndDate(startDate, endDate);
            this.startDate = startDate;
            this.endDate = endDate;
        }
        public Nullable<TimeSpan> TimeSpan
        {
            get { return endDate - startDate; }
        }
        public Nullable<DateTime> StartDate
        {
            get { return startDate; }
            set
            {
                AssertStartDateFollowsEndDate(value, this.endDate);
                startDate = value;
            }
        }
        public Nullable<DateTime> EndDate
        {
            get { return endDate; }
            set
            {
                AssertStartDateFollowsEndDate(this.startDate, value);
                endDate = value;
            }
        }

        /// <summary>
        /// Do we want to consider times when filtering
        /// </summary>
        /// <remarks>This is not used interally by the class, just tracked for the client's benefit</remarks>
        public bool ConsiderTimes
        {
            get
            {
                return this.considerTimes;
            }
            set
            {
                this.considerTimes = value;
            }
        }
        protected bool considerTimes = false;

        /// <summary>
        /// Clears the values stored in the class
        /// </summary>
        public void Clear()
        {
            StartDate = null;
            EndDate = null;
            ConsiderTimes = false;
        }

        /// <summary>
        /// Parses the input string to see if it's a valid date range
        /// </summary>
        /// <param name="s">The string being parsed</param>
        /// <param name="result">The populated date range object (if successful)</param>
        /// <returns>True if parsed</returns>
        /// <remarks>Any valid date or pair of dates separated by a pipe (|) character will parse OK</remarks>
        public static bool TryParse(string s, out DateRange result)
        {
            result = new DateRange();
            if (String.IsNullOrEmpty(s))
            {
                //Empty range is valid
                return true;
            }
            else
            {
                if (s.Contains("|"))
                {
                    //We have two dates
                    string[] dates = s.Split('|');
                    if (dates.Length < 2)
                    {
                        return false;
                    }
                    else
                    {
                        //Try parsing each
                        DateTime startDate;
                        if (!String.IsNullOrEmpty(dates[0]))
                        {
                            if (DateTime.TryParse(dates[0], out startDate))
                            {
                                result.StartDate = startDate;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        DateTime endDate;
                        if (!String.IsNullOrEmpty(dates[1]))
                        {
                            if (DateTime.TryParse(dates[1], out endDate))
                            {
                                result.EndDate = endDate;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    //we just have just the start date
                    DateTime startDate;
                    if (DateTime.TryParse(s, out startDate))
                    {
                        result.StartDate = startDate;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Returns the string representation of a date-range
        /// </summary>
        /// <returns>The string representation</returns>
        public override string ToString()
        {
            string output = "";
            if (this.StartDate.HasValue)
            {
                //Output as a culture invariant date/time
                output += this.StartDate.Value.ToString("yyyy-MM-dd");
            }
            output += "|";  //Pipe separates the two dates from each other
            if (this.EndDate.HasValue)
            {
                //Output as a culture invariant date/time
                output += this.EndDate.Value.ToString("yyyy-MM-dd");
            }
            return output;
        }

        private void AssertStartDateFollowsEndDate(Nullable<DateTime> startDate,
            Nullable<DateTime> endDate)
        {
            if ((startDate.HasValue && endDate.HasValue) &&
                (endDate.Value < startDate.Value))
                throw new InvalidOperationException("Start Date must be less than or equal to End Date");
        }
        public DateRange GetIntersection(DateRange other)
        {
            if (!Intersects(other)) throw new InvalidOperationException("DateRanges do not intersect");
            return new DateRange(GetLaterStartDate(other.StartDate), GetEarlierEndDate(other.EndDate));
        }
        private Nullable<DateTime> GetLaterStartDate(Nullable<DateTime> other)
        {
            return Nullable.Compare<DateTime>(startDate, other) >= 0 ? startDate : other;
        }
        private Nullable<DateTime> GetEarlierEndDate(Nullable<DateTime> other)
        {
            //!endDate.HasValue == +infinity, not negative infinity
            //as is the case with !startDate.HasValue
            if (Nullable.Compare<DateTime>(endDate, other) == 0) return other;
            if (endDate.HasValue && !other.HasValue) return endDate;
            if (!endDate.HasValue && other.HasValue) return other;
            return (Nullable.Compare<DateTime>(endDate, other) >= 0) ? other : endDate;
        }
        public bool Intersects(DateRange other)
        {
            if ((this.startDate.HasValue && other.EndDate.HasValue &&
                other.EndDate.Value < this.startDate.Value) ||
                (this.endDate.HasValue && other.StartDate.HasValue &&
                other.StartDate.Value > this.endDate.Value) ||
                (other.StartDate.HasValue && this.endDate.HasValue &&
                this.endDate.Value < other.StartDate.Value) ||
                (other.EndDate.HasValue && this.startDate.HasValue &&
                this.startDate.Value > other.EndDate.Value))
            {
                return false;
            }
            return true;
        }

		/// <summary>Returns whether a specified date falls within this daterange.</summary>
		/// <param name="other">The date to check.</param>
		/// <returns>Boolean whether or not the other date is contained in this daterange.</returns>
		public bool Contains(DateTime other)
		{
			if (this.startDate.HasValue && this.endDate.HasValue)
			{
				if (other.Date >= this.startDate.Value.Date && other.Date <= this.endDate.Value.Date)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				if (this.startDate.HasValue && other.Date > this.startDate.Value.Date)
				{
					return true;
				}
				else if (this.endDate.HasValue && other.Date < this.endDate.Value.Date)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}
        public bool Equals(DateRange other)
        {
            if (object.ReferenceEquals(other, null)) return false;
            return ((startDate == other.StartDate) && (endDate == other.EndDate));
        }
    }

    /// <summary>
    /// Allows date range classes to be compared by their start date
    /// </summary>
    public class DateRangeComparerByStartDate : System.Collections.IComparer,
        System.Collections.Generic.IComparer<DateRange>
    {
        public int Compare(object x, object y)
        {
            if (!(x is DateRange) || !(y is DateRange))
                throw new System.ArgumentException("Value not a DateRange");
            return Compare((DateRange)x, (DateRange)y);
        }
        public int Compare(DateRange x, DateRange y)
        {
            if (x.StartDate < y.StartDate) { return -1; }
            if (x.StartDate > y.StartDate) { return 1; }
            return 0;
        }
    }
}
