using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.Common
{
    /// <summary>
    /// Represents an effort-range that is used in filters, etc.
    /// </summary>
    [Serializable]
    public class EffortRange
    {
        decimal? minValue;
        decimal? maxValue;

        /// <summary>
        /// The minimum value (or null if no min-value) in fractional hours
        /// </summary>
        public decimal? MinValue
        {
            get
            {
                return this.minValue;
            }
            set
            {
                AssertMaxValueLargerThanMinValue(value, this.maxValue);
                this.minValue = value;
            }
        }

        /// <summary>
        /// The maximum value (or null if no max-value) in fractional hours
        /// </summary>
        public decimal? MaxValue
        {
            get
            {
                return this.maxValue;
            }
            set
            {
                AssertMaxValueLargerThanMinValue(this.minValue, value);
                this.maxValue = value;
            }
        }

        /// <summary>
        /// The minimum value (or null if no max-value) in integer minutes
        /// </summary>
        public int? MinValueInMinutes
        {
            get
            {
                if (MinValue.HasValue)
                {
                    return (int)(MinValue.Value * 60M);
                }
                return null;
            }
        }

        /// <summary>
        /// The maximum value (or null if no max-value) in integer minutes
        /// </summary>
        public int? MaxValueInMinutes
        {
            get
            {
                if (MaxValue.HasValue)
                {
                    return (int)(MaxValue.Value * 60M);
                }
                return null;
            }
        }

        /// <summary>
        /// Clears the values stored in the class
        /// </summary>
        public void Clear()
        {
            MinValue = null;
            MaxValue = null;
        }

        /// <summary>
        /// Verifies that the min value is not larger than the max value
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        private void AssertMaxValueLargerThanMinValue(decimal? minValue, decimal? maxValue)
        {
            if ((minValue.HasValue && maxValue.HasValue) &&
                (minValue.Value > maxValue.Value))
                throw new InvalidOperationException("Min Value must be less than or equal to Max Value");
        }

        /// <summary>
        /// Returns the string representation of an effort-range in fractional hours
        /// </summary>
        /// <returns>The string representation</returns>
        public override string ToString()
        {
            string output = "";
            if (this.MinValue.HasValue)
            {
                //Output as a culture specific decimal
                output += this.MinValue.Value.ToString("0.0");
            }
            output += "|";  //Pipe separates the two values from each other
            if (this.MaxValue.HasValue)
            {
                //Output as a culture specific decimal
                output += this.MaxValue.Value.ToString("0.0");
            }
            return output;
        }

        /// <summary>
        /// Parses the input string to see if it's a valid decimal range
        /// </summary>
        /// <param name="s">The string being parsed</param>
        /// <param name="result">The populated decimal range object (if successful)</param>
        /// <returns>True if parsed</returns>
        /// <remarks>Any valid decimal or pair of decimals separated by a pipe (|) character will parse OK</remarks>
        public static bool TryParse(string s, out EffortRange result)
        {
            result = new EffortRange();
            if (String.IsNullOrEmpty(s))
            {
                //Empty range is valid
                return true;
            }
            else
            {
                if (s.Contains("|"))
                {
                    //We have two values
                    string[] values = s.Split('|');
                    if (values.Length < 2)
                    {
                        return false;
                    }
                    else
                    {
                        //Try parsing each
                        decimal minValue;
                        if (!String.IsNullOrEmpty(values[0]))
                        {
                            if (Decimal.TryParse(values[0], out minValue))
                            {
                                result.MinValue = minValue;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        decimal maxValue;
                        if (!String.IsNullOrEmpty(values[1]))
                        {
                            if (Decimal.TryParse(values[1], out maxValue))
                            {
                                result.MaxValue = maxValue;
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
                    //we just have just the min value
                    decimal minValue;
                    if (Decimal.TryParse(s, out minValue))
                    {
                        result.MinValue = minValue;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
