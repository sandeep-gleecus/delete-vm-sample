using System;
using System.IO;

namespace Validation.Reports.Windward.Core
{
	public static class Extensions
	{
		//convert string to stream.
		public static Stream ToStream(this string inputString)
		{
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(inputString);
			writer.Flush();
			stream.Position = 0;

			return stream;
		}

		//Get file from Local drive and convert it into string.
		public static string FileContentToString(this string filePah)
		{
			StreamReader reader = new StreamReader(filePah);
			string ret = reader.ReadToEnd();
			reader.Close();
			return ret;
		}

		//convert stream to byte[].
		public static byte[] ToByteArray(this Stream stream)
		{
			using (stream)
			{
				using (MemoryStream memStream = new MemoryStream())
				{
					stream.CopyTo(memStream);
					return memStream.ToArray();
				}
			}
		}

		//check if input is valid GUID
		public static bool IsValidGuid(this string str)
		{
			Guid guid;
			return Guid.TryParse(str, out guid);
		}
	}
}
