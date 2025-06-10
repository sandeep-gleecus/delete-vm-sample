using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.Common
{
    /// <summary>
    /// Represents an longeger-range that is used in filters, etc.
    /// </summary>
    [Serializable]
    public class LongRange
    {
        long? minValue;
        long? maxValue;

        /// <summary>
        /// The minimum value (or null if no min-value)
        /// </summary>
        public long? MinValue
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
        /// The maximum value (or null if no max-value)
        /// </summary>
        public long? MaxValue
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
        private void AssertMaxValueLargerThanMinValue(long? minValue, long? maxValue)
        {
            if ((minValue.HasValue && maxValue.HasValue) &&
                (minValue.Value > maxValue.Value))
                throw new InvalidOperationException("Min Value must be less than or equal to Max Value");
        }

        /// <summary>
        /// Returns the string representation of a long-range
        /// </summary>
        /// <returns>The string representation</returns>
        public override string ToString()
        {
            string output = "";
            if (this.MinValue.HasValue)
            {
                //Output as a culture specific long
                output += this.MinValue.Value.ToString();
            }
            output += "|";  //Pipe separates the two values from each other
            if (this.MaxValue.HasValue)
            {
                //Output as a culture specific long
                output += this.MaxValue.Value.ToString();
            }
            return output;
        }

        /// <summary>
        /// Parses the input string to see if it's a valid long range
        /// </summary>
        /// <param name="s">The string being parsed</param>
        /// <param name="result">The populated long range object (if successful)</param>
        /// <returns>True if parsed</returns>
        /// <remarks>Any valid long or pair of longs separated by a pipe (|) character will parse OK</remarks>
        public static bool TryParse(string s, out LongRange result)
        {
            result = new LongRange();
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
                        long minValue;
                        if (!String.IsNullOrEmpty(values[0]))
                        {
                            if (Int64.TryParse(values[0], out minValue))
                            {
                                result.MinValue = minValue;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        long maxValue;
                        if (!String.IsNullOrEmpty(values[1]))
                        {
                            if (Int64.TryParse(values[1], out maxValue))
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
                    long minValue;
                    if (Int64.TryParse(s, out minValue))
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
