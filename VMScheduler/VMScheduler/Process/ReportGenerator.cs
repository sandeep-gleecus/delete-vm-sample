using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace VMScheduler
{
	using VMScheduler.Process;
	public enum Const
	{
		HELPER_PARSE_NAME = 8,
		HELPER_PARSE_VALUE = 9,
		HELPER_PARSE_FILENAME = 9
	}
	public class ReportGenerator
	{
		Logger logger;
		public ReportGenerator(Logger logger)
		{
			this.logger = logger;
		}

		/// <summary>
		/// Creates new report by calling VM API
		/// </summary>
		public string GenerateReport(string lookupKeys, string outputType, string scheduleId, string templateId, string callingApplication, string outputFileName)
		{
			try
			{
				lookupKeys = lookupKeys.Replace("[{", "").Replace("}]", "").Replace("{", "").Replace("}", "");

				var formVars = new Dictionary<string, string>();
				string[] parts = lookupKeys.Split(',');
				string name = "", value = "", postData = "";
				bool valueRead = false;
				foreach (string part in parts)
				{
					Console.WriteLine("{0}", part);
					if (part.Contains("Name"))
						name = part.Substring(8, part.Length - (int)Const.HELPER_PARSE_NAME - 1);

					if (part.Contains("Value"))
					{
						value = part.Substring(9, part.Length - (int)Const.HELPER_PARSE_VALUE - 1);
						valueRead = true;
					}


					if ((!String.IsNullOrWhiteSpace(name) && !String.IsNullOrWhiteSpace(value)) || valueRead)
					{
						if (name.ToLower() != "templateid")
						{
							formVars.Add(name, value);
							postData += HttpUtility.UrlEncode(name) + "=" + HttpUtility.UrlEncode(value) + "&";
							Console.WriteLine(name + "," + value);
						}
						name = ""; value = ""; valueRead = false;
					}
				}

				if (postData.Substring(postData.Length - 1, 1) == "&")
					postData = postData.Substring(0, postData.Length - 1);
				byte[] data = Encoding.ASCII.GetBytes(postData);

				string queryString = ConfigurationManager.AppSettings.Get("VMAPIEndPoint");
				string workingFolder = ConfigurationManager.AppSettings.Get("OutputFolder");
				var filename = APICall(workingFolder, queryString, postData, outputType, scheduleId, templateId, outputFileName);

				return filename;


				#region HTTP POST - Not Used

				////var templateId = 2074;

				//var url = $"{queryString}{templateId}/{postData}/{outputType}";

				//HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@queryString);
				//request.ContentType = "application/x-www-form-urlencoded";
				//request.ContentLength = data.Length;
				//request.Headers.Add("CallingApp", callingApp);
				//request.Headers.Add("parameters", postData);
				//request.Headers.Add("outputType", outputType);

				//request.Headers.Add("OutputFileType", outputType);

				//request.Method = "POST";
				////request.Method = "GET";
				//request.KeepAlive = false;

				//Stream requestStream = request.GetRequestStream();
				//requestStream.Write(data, 0, data.Length);
				//requestStream.Close();

				//HttpWebResponse response = (HttpWebResponse)request.GetResponse();

				//string filename = "";
				//string filePath = "";
				//filePath = appLocation;
				//bool exists = System.IO.Directory.Exists(filePath);
				//// try another location
				//if (!exists) filePath = Environment.CurrentDirectory;


				//if (response.Headers.Count > 0)
				//{
				//    for (int i = 0; i < response.Headers.Count; ++i)
				//    {
				//        Console.WriteLine("\nHeader Name:{0}, Value :{1}", response.Headers.Keys[i], response.Headers[i]);
				//        // Get file name
				//        if (response.Headers.Keys[i] == "Content-Disposition")
				//        {
				//            filename = response.Headers[i].Substring(response.Headers[i].IndexOf("filename") + (int)Const.HELPER_PARSE_FILENAME,
				//                response.Headers[i].Length - (response.Headers[i].IndexOf("filename") + (int)Const.HELPER_PARSE_FILENAME));

				//        }
				//    }
				//}

				//Stream responseStream = response.GetResponseStream();
				////Remove extra characters
				//if (!String.IsNullOrEmpty(filename))
				//{
				//    if (filename.IndexOf(" ") > 0)
				//    {
				//        //if file name has spaces we need to remove encoding characters
				//        filename = filename.Substring(1, filename.Length - 2);
				//    }
				//    else
				//        filename = filename.Substring(0, filename.Length);
				//}
				//else
				//{
				//    filename = $"moo{DateTime.Now.Ticks}.txt";
				//}

				//int fileExtPos = filename.LastIndexOf(".");
				//if (fileExtPos >= 0)
				//{
				//    string ext = filename.Substring(fileExtPos, filename.Length - fileExtPos);
				//    filename = filename.Substring(0, fileExtPos);

				//    filename = filePath + "\\" + filename + "_" + scheduleId + ext;
				//    using (Stream file = File.Create(filename))
				//    {
				//        CopyStream(responseStream, file);
				//    }
				//}

				//responseStream.Close();

				//// Releases the resources of the response.
				//response.Close();
				//return filename;


				#endregion

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				logger.CreateErrorLogRecord(ex.Message, "ReportGenerator ");
				return String.Empty;
			}
		}

		public string APICall(string appLocation, string queryString, string postData, string outputType, string scheduleId, string templateId, string outputFileName)
		{
			string filename = "";
			string filePath = "";
			filePath = appLocation;
			bool exists = System.IO.Directory.Exists(filePath);
			// try another location
			if (!exists) filePath = Environment.CurrentDirectory;

			var encodedPostData = WebUtility.HtmlEncode(postData);
			string ext;
			//GET
			var url = $"{queryString}{templateId}/{postData}/{outputType}";
			WebRequest request = WebRequest.Create(url);
			request.Method = "GET";

			if (!string.IsNullOrEmpty(outputFileName))
			{
				switch (outputType.ToLower())
				{
					case "word":
						ext = "docx";
						break;
					case "excel":
						ext = "xlsx";
						break;
					case "html":
						ext = "htm";
						break;
					case "pdf":
						ext = "pdf";
						break;
					default:
						ext = "txt";
						break;
				}
				filename = $"{outputFileName}.{ext}";
			}

			using (WebResponse response = request.GetResponse())
			{
				if (response.Headers.Count > 0)
				{
					if (string.IsNullOrEmpty(filename))
					{
						for (int i = 0; i < response.Headers.Count; ++i)
						{
							Console.WriteLine("\nHeader Name:{0}, Value :{1}", response.Headers.Keys[i], response.Headers[i]);
							// Get file name
							if (response.Headers.Keys[i] == "Content-Disposition")
							{
								filename = response.Headers[i].Substring(response.Headers[i].IndexOf("filename") + (int)Const.HELPER_PARSE_FILENAME,
									response.Headers[i].Length - (response.Headers[i].IndexOf("filename") + (int)Const.HELPER_PARSE_FILENAME));
								Console.WriteLine($"FileName: {filename}");
							}
						}
					}

					Stream responseStream = response.GetResponseStream();
					//Remove extra characters
					if (string.IsNullOrEmpty(filename))
					{
						filename = $"default{DateTime.Now.Ticks}.txt";
					}

					int fileExtPos = filename.LastIndexOf(".");
					if (fileExtPos >= 0)
					{
						ext = filename.Substring(fileExtPos, filename.Length - fileExtPos);
						filename = filename.Substring(0, fileExtPos);

						filename = filePath + "\\" + filename + "_" + scheduleId + ext;
						Console.WriteLine($"File Name and Path: {filename}");
						using (Stream file = File.Create(filename))
						{
							CopyStream(responseStream, file);
						}
					}
				}
			}
			return filename;
		}

		/// <summary>
		/// Copies the contents of input to output. Doesn't close either stream.
		/// </summary>
		public static void CopyStream(Stream input, Stream output)
		{
			byte[] buffer = new byte[8 * 1024];
			int len;
			while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
			{
				output.Write(buffer, 0, len);
			}
		}
	}
}
