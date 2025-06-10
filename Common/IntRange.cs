using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.Common
{
    /// <summary>
    /// Represents an integer-range that is used in filters, etc.
    /// </summary>
    [Serializable]
    public class IntRange
    {
        int? minValue;
        int? maxValue;

        /// <summary>
        /// The minimum value (or null if no min-value)
        /// </summary>
        public int? MinValue
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
        public int? MaxValue
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
        private void AssertMaxValueLargerThanMinValue(int? minValue, int? maxValue)
        {
            if ((minValue.HasValue && maxValue.HasValue) &&
                (minValue.Value > maxValue.Value))
                throw new InvalidOperationException("Min Value must be less than or equal to Max Value");
        }

        /// <summary>
        /// Returns the string representation of a int-range
        /// </summary>
        /// <returns>The string representation</returns>
        public override string ToString()
        {
            string output = "";
            if (this.MinValue.HasValue)
            {
                //Output as a culture specific int
                output += this.MinValue.Value.ToString();
            }
            output += "|";  //Pipe separates the two values from each other
            if (this.MaxValue.HasValue)
            {
                //Output as a culture specific int
                output += this.MaxValue.Value.ToString();
            }
            return output;
        }

        /// <summary>
        /// Parses the input string to see if it's a valid int range
        /// </summary>
        /// <param name="s">The string being parsed</param>
        /// <param name="result">The populated int range object (if successful)</param>
        /// <returns>True if parsed</returns>
        /// <remarks>Any valid int or pair of ints separated by a pipe (|) character will parse OK</remarks>
        public static bool TryParse(string s, out IntRange result)
        {
            result = new IntRange();
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
                        int minValue;
                        if (!String.IsNullOrEmpty(values[0]))
                        {
                            if (Int32.TryParse(values[0], out minValue))
                            {
                                result.MinValue = minValue;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        int maxValue;
                        if (!String.IsNullOrEmpty(values[1]))
                        {
                            if (Int32.TryParse(values[1], out maxValue))
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
                    int minValue;
                    if (Int32.TryParse(s, out minValue))
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
