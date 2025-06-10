using System;
using System.Collections.Generic;
using System.Text;

namespace Inflectra.SpiraTest.Common
{
    /// <summary>
    /// Represents a multi-values filter entry
    /// </summary>
    /// <remarks>Has a special property to handle the case of filtering for (None)</remarks>
    [Serializable]
    public class MultiValueFilter
    {
        bool isNone = false;
        List<int> values = new List<int>();

        public const int NoneFilterValue = -999;

        #region Properties

        /// <summary>
        /// Returns the list of specified values to filter on
        /// </summary>
        public List<int> Values
        {
            get
            {
                return this.values;
            }
        }

        /// <summary>
        /// Do we have the special case of a filter for (None) - i.e. all records that have no value set
        /// </summary>
        public bool IsNone
        {
            get
            {
                return this.isNone;
            }
            set
            {
                this.isNone = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Resets the filter value back to nothing
        /// </summary>
        public void Clear()
        {
            this.isNone = false;
            this.values.Clear();
        }

        /// <summary>
        /// Parses the input string to see if it's a valid multi-value filter
        /// </summary>
        /// <param name="s">The string being parsed</param>
        /// <param name="result">The populated mutivalue object (if successful)</param>
        /// <returns>True if parsed</returns>
        /// <remarks>Any integer value or set of comma-separated integer values will parse ok</remarks>
        public static bool TryParse(string s, out MultiValueFilter result)
        {
            result = new MultiValueFilter();
            if (String.IsNullOrEmpty(s))
            {
                //Empty range is valid
                return true;
            }
            else
            {
                string[] values = s.Split(',');
                //Now parse each one into an integer
                for (int i = 0; i < values.Length; i++)
                {
                    int intValue = -1;
                    if (Int32.TryParse(values[i], out intValue))
                    {
                        //See if we have the special case of a no-value filter, which is represented by -999
                        //which overrides all other cases
                        if (intValue == NoneFilterValue)
                        {
                            result.Values.Clear();
                            result.IsNone = true;
                            return true;
                        }
                        else
                        {
                            result.Values.Add(intValue);
                        }
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
        /// Returns the string representation of the multi-valued filter
        /// </summary>
        /// <returns>The comma-separated list of values</returns>
        public override string ToString()
        {
            //First handle the special case of a (None) filter
            if (this.IsNone)
            {
                return NoneFilterValue.ToString();
            }
            else
            {
                //Now handle the case of no values
                if (Values.Count == 0)
                {
                    return "";
                }
                else
                {
                    string result = "";
                    foreach (int intValue in Values)
                    {
                        if (result == "")
                        {
                            result = intValue.ToString();
                        }
                        else
                        {
                            result += "," + intValue.ToString();
                        }
                    }
                    return result;
                }
            }
        }

        #endregion
    }
}
