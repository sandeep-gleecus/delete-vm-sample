using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Inflectra.SpiraTest.DataModel
{
	public static class DatabaseExtensions
	{
		public const string FORMAT_DATE_TIME_INVARIANT = "yyyy-MM-ddTHH:mm:ss.fff";
		public const string FORMAT_DATE_INVARIANT = "yyyy-MM-dd";
		public const char FORMAT_LIST_SEPARATOR = ',';
        public const string FORMAT_DATE_TIME_SECONDS_INVARIANT = "yyyy-MM-ddTHH:mm:ss";
        public const string FORMAT_INTEGER_SORTABLE = "0000000000";
        public const string FORMAT_DECIMAL_SORTABLE = "0000000000.##########";

        #region Custom Property Database Extensions
        #region String Conversion
        /// <summary>Converts a string to the database field format (string).</summary>
        /// <param name="parm1">The string to convert.</param>
        /// <returns>The converted string.</returns>
        public static string ToDatabaseSerialization(this string parm1)
		{
			if (string.IsNullOrWhiteSpace(parm1))
				return "";
			else
				return parm1;
		}

		/// <summary>Converts a string from the database into a string.</summary>
		/// <param name="parm1">The string to convert.</param>
		/// <returns>The converted string.</returns>
		public static string FromDatabaseSerialization_String(this string parm1)
		{
			//Check for nulls..
			if (string.IsNullOrWhiteSpace(parm1))
				return null;
			else
				return parm1;
		}
		#endregion

		#region DataTime Conversion
		/// <summary>Converts a DateTime to the database field format (string).</summary>
		/// <param name="parm1">The DateTime to convert.</param>
		/// <returns>The converted string.</returns>
		public static string ToDatabaseSerialization(this DateTime parm1, bool dateOnly=false)
		{
			string retString = "";
			if (dateOnly)
				retString = parm1.ToString(FORMAT_DATE_INVARIANT, System.Globalization.CultureInfo.InvariantCulture);
			else
				retString = parm1.ToString(FORMAT_DATE_TIME_INVARIANT, System.Globalization.CultureInfo.InvariantCulture);

			return retString;
		}

		/// <summary>Converts a string from a database field to a DateTime.</summary>
		/// <param name="parm1">The string from the database field.</param>
		/// <returns>A nullable DateTime.</returns>
		public static DateTime? FromDatabaseSerialization_DateTime(this string parm1)
		{
			DateTime? retDate = null;

			try
			{
				long numTicks;
				if (long.TryParse(parm1, out numTicks))
				{
					retDate = new DateTime(numTicks);
				}
				else
				{
                    DateTime tempDate;
                    if (DateTime.TryParseExact(parm1, FORMAT_DATE_TIME_INVARIANT, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out tempDate))
                    {
                        //Try parsing the full date-time
                        retDate = tempDate;
                    }
                    else if (DateTime.TryParseExact(parm1, FORMAT_DATE_INVARIANT, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out tempDate))
                    {
                        //Otherwise try using the date-only
                        retDate = tempDate;
                    }
				}
			}
			catch
			{
				retDate = null;
			}

			return retDate;
		}
		#endregion

		#region Int32 Conversion
		/// <summary>Converts an Int32 to the database field format (string).</summary>
		/// <param name="parm1">The Int32 to convert.</param>
		/// <returns>The converted string.</returns>
		public static string ToDatabaseSerialization(this Int32 parm1)
		{
			return parm1.ToString(FORMAT_INTEGER_SORTABLE);
		}

		/// <summary>Converts a string from a database field to an Int32.</summary>
		/// <param name="parm1">The string from the database field.</param>
		/// <returns>A nullable Int32.</returns>
		public static Int32? FromDatabaseSerialization_Int32(this string parm1)
		{
			Int32? retInt = null;

			try
			{
				retInt = Int32.Parse(parm1);
			}
			catch
			{
				retInt = null;
			}

			return retInt;
		}
		#endregion

		#region UInt32 Conversion
		/// <summary>Converts a UInt32 to the database field format (string).</summary>
		/// <param name="parm1">The UInt32 to convert.</param>
		/// <returns>The converted string.</returns>
		public static string ToDatabaseSerialization(this UInt32 parm1)
		{
			return parm1.ToString(FORMAT_INTEGER_SORTABLE);
		}

		/// <summary>Converts a string from a database field to a UInt32.</summary>
		/// <param name="parm1">The string from the database field.</param>
		/// <returns>A nullable UInt32.</returns>
		public static UInt32? FromDatabaseSerialization_UInt32(this string parm1)
		{
			UInt32? retInt = null;

			try
			{
				retInt = UInt32.Parse(parm1);
			}
			catch
			{
				retInt = null;
			}

			return retInt;
		}
		#endregion

		#region Decimal Conversion
		/// <summary>Converts a Decimal to the database field format (string).</summary>
		/// <param name="parm1">The Decimal to convert.</param>
		/// <returns>The converted string.</returns>
        /// <remarks>Uses the invariant culture to ensure a stable format is saved (periods not commas always)</remarks>
		public static string ToDatabaseSerialization(this Decimal parm1)
		{
            return parm1.ToString(FORMAT_DECIMAL_SORTABLE, System.Globalization.CultureInfo.InvariantCulture);
		}

		/// <summary>Converts a string from a database field to a Decimal.</summary>
		/// <param name="parm1">The string from the database field.</param>
		/// <returns>A nullable Decimal.</returns>
        /// <remarks>We want a stable format so we use the invariant culture (periods not commas always)</remarks>
		public static Decimal? FromDatabaseSerialization_Decimal(this string parm1)
		{
			Decimal? retVal = null;

			try
			{
                retVal = Decimal.Parse(parm1, System.Globalization.CultureInfo.InvariantCulture);
			}
			catch
			{
				retVal = null;
			}

			return retVal;
		}
		#endregion

		#region Boolean Conversion
		/// <summary>Converts a Decimal to the database field format (string).</summary>
		/// <param name="parm1">The Decimal to convert.</param>
		/// <returns>The converted string.</returns>
		public static string ToDatabaseSerialization(this Boolean parm1)
		{
			return ((parm1) ? "Y" : "N");
		}

		/// <summary>Converts a string from a database field to a Boolean.</summary>
		/// <param name="parm1">The string from the database field.</param>
		/// <returns>A nullable Boolean.</returns>
		public static Boolean? FromDatabaseSerialization_Boolean(this string parm1)
		{
			Boolean? retVal = null;
            if (parm1 == "Y")
            {
                retVal = true;
            }
            if (parm1 == "N")
            {
                retVal = false;
            }

			return retVal;
		}
		#endregion

		#region List<Int32> Conversion
		/// <summary>Converts an List&lt;Int32&gt; to the database field format (string).</summary>
		/// <param name="parm1">The List&lt;Int32&gt; to convert.</param>
		/// <returns>The converted string.</returns>
		public static string ToDatabaseSerialization(this List<Int32> parm1)
		{
			string retString = "";
            if (parm1 != null)
            {
                foreach (Int32 val in parm1)
                {
                    retString += val.ToDatabaseSerialization() + ",";
                }

                retString = retString.Trim(FORMAT_LIST_SEPARATOR);
            }

			return retString;
		}

		/// <summary>Converts a string from a database field to an Int32.</summary>
		/// <param name="parm1">The string from the database field.</param>
		/// <returns>A list of integers, empty if none were provided or an error occurs..</returns>
		public static List<Int32> FromDatabaseSerialization_List_Int32(this string parm1)
		{
			List<Int32> retList = new List<Int32>();

            //Check for null case
            if (String.IsNullOrEmpty(parm1))
            {
                return retList;
            }

			//Split the string up..
			string[] vals = parm1.Split(FORMAT_LIST_SEPARATOR);
			foreach (string val in vals)
			{
				Int32? num = val.FromDatabaseSerialization_Int32();
				if (num.HasValue)
					retList.Add(num.Value);
			}

			return retList;
		}
		#endregion

		#region List<UInt32> Conversion
		/// <summary>Converts an List&lt;UInt32&gt; to the database field format (string).</summary>
		/// <param name="parm1">The List&lt;Int32&gt; to convert.</param>
		/// <returns>The converted string.</returns>
		public static string ToDatabaseSerialization(this List<UInt32> parm1)
		{
			string retString = "";
			foreach (UInt32 val in parm1)
			{
				retString += val.ToDatabaseSerialization() + ",";
			}

			retString = retString.Trim(FORMAT_LIST_SEPARATOR);

			return retString;
		}

		/// <summary>Converts a string from a database field to an UInt32.</summary>
		/// <param name="parm1">The string from the database field.</param>
		/// <returns>A list of integers, empty if none were provided or an error occurs..</returns>
		public static List<UInt32> FromDatabaseSerialization_List_UInt32(this string parm1)
		{
			List<UInt32> retList = new List<UInt32>();

			//Check for null case
			if (String.IsNullOrEmpty(parm1))
			{
				return retList;
			}

			//Split the string up..
			string[] vals = parm1.Split(FORMAT_LIST_SEPARATOR);
			foreach (string val in vals)
			{
				UInt32? num = val.FromDatabaseSerialization_UInt32();
				if (num.HasValue)
					retList.Add(num.Value);
			}

			return retList;
		}
		#endregion

		#region Int64 Conversion
		/// <summary>Converts an Int64 to the database field format (string).</summary>
		/// <param name="parm1">The Int64 to convert.</param>
		/// <returns>The converted string.</returns>
		public static string ToDatabaseSerialization(this Int64 parm1)
		{
			return parm1.ToString(FORMAT_INTEGER_SORTABLE);
		}

		/// <summary>Converts a string from a database field to an Int64.</summary>
		/// <param name="parm1">The string from the database field.</param>
		/// <returns>A nullable Int64 (long)</returns>
		public static Int64? FromDatabaseSerialization_Int64(this string parm1)
		{
			Int64? retVal = null;

			try
			{
				retVal = Int64.Parse(parm1);
			}
			catch
			{
				retVal = null;
			}

			return retVal;

		}
		#endregion

		#region For Any Type
		/// <summary>Converts a string from a database field to a DateTime.</summary>
		/// <param name="parm1">The string from the database field.</param>
		/// <returns>A nullable DateTime.</returns>
		public static object FromDatabaseSerialization(this string parm1, string type)
		{
			object retObj = null;

			try
			{
				switch (type.Trim().ToLowerInvariant())
				{
					case "system.string":
						retObj = parm1.FromDatabaseSerialization_String();
						break;

					case "system.int32":
						retObj = parm1.FromDatabaseSerialization_Int32();
						break;

					case "system.uint32":
						retObj = parm1.FromDatabaseSerialization_UInt32();
						break;

					case "system.decimal":
						retObj = parm1.FromDatabaseSerialization_Decimal();
						break;

					case "system.boolean":
						retObj = parm1.FromDatabaseSerialization_Boolean();
						break;

					case "system.datetime":
						retObj = parm1.FromDatabaseSerialization_DateTime();
						break;

					case "system.collections.generic.list`1[system.int32]":
						retObj = parm1.FromDatabaseSerialization_List_Int32();
						break;

					case "system.collections.generic.list`1[system.uint32]":
						retObj = parm1.FromDatabaseSerialization_List_UInt32();
						break;

					default:
						break;
				}
			}
			catch { }

			return retObj;
		}

		/// <summary>Converts the object into a database string.</summary>
		/// <param name="parm1">The object to convert.</param>
		/// <returns>The string suitable for storing in a custom field.</returns>
		public static string ToDatabaseSerialization(this object parm1)
		{
			string retString = null;

			if (parm1 != null && parm1.GetType() != typeof(System.DBNull))
			{
				try
				{
					string strType = parm1.GetType().ToString().Trim().ToLowerInvariant();
					switch (strType)
					{
						case "system.string":
							retString = ((String)parm1).ToDatabaseSerialization();
							break;

						case "system.int32":
							retString = ((Int32)parm1).ToDatabaseSerialization();
							break;

						case "system.uint32":
							retString = ((UInt32)parm1).ToDatabaseSerialization();
							break;

						case "system.decimal":
							retString = ((Decimal)parm1).ToDatabaseSerialization();
							break;

						case "system.boolean":
							retString = ((Boolean)parm1).ToDatabaseSerialization();
							break;

						case "system.datetime":
							retString = ((DateTime)parm1).ToDatabaseSerialization();
							break;

						case "system.collections.generic.list`1[system.int32]":
							retString = ((List<Int32>)parm1).ToDatabaseSerialization();
							break;

						case "system.collections.generic.list`1[system.uint32]":
							retString = ((List<UInt32>)parm1).ToDatabaseSerialization();
							break;

						case "system.int64":
							retString = ((Int64)parm1).ToDatabaseSerialization();
							break;

						default:
							retString = "error";
							break;
					}

					//Handle empties ('nulls')
					if (string.IsNullOrWhiteSpace(retString))
						retString = null;
				}
				catch
				{ }
			}

			return retString;
		}
		#endregion
		#endregion
	}
}
