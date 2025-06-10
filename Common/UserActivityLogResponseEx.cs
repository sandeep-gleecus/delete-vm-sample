using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Common
{
	public partial class UserActivityLogResponse : Entity
	{

		/// <summary>
		/// Returns an SHA256 hash
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static string GetHashSha256(string text)
		{
			byte[] bytes = UTF8Encoding.Unicode.GetBytes(text);
			byte[] hash;
			using (SHA256 shaManaged = new SHA256Managed())
			{
				hash = shaManaged.ComputeHash(bytes);
			}
			string hashString = string.Empty;
			foreach (byte x in hash)
			{
				hashString += String.Format("{0:x2}", x);
			}
			return hashString;
		}
	}
}
