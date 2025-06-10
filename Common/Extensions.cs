using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inflectra.SpiraTest.Common
{
	public static class Extensions
	{
		/// <summary>Returns a sub-array from the given array.</summary>
		/// <typeparam name="T">The type of data to expect.</typeparam>
		/// <param name="data">The original array.</param>
		/// <param name="index">The index to start copying from.</param>
		/// <param name="length">The number of objects in the new sub array.</param>
		/// <returns>An array containing the specified items.</returns>
		public static T[] SubArray<T>(this T[] data, int index, int length)
		{
			T[] result = new T[length];
			Array.Copy(data, index, result, 0, length);
			return result;
		}
	}
}
