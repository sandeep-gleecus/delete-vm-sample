//using VMScheduler.Model;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;

//namespace VMScheduler.Process
//{
//    public class ECMProcessor
//    {
//        private Logger logger;
        
//        public ECMProcessor(Logger logger)
//        {
//            this.logger = logger;
//        }
//        public List<string> SendToECM(IList<Result> results)
//        {
//            List<string> nodes = new List<string>();
//            try
//            {
//                //int count = 0;
                
//                //we are using ECM APIs
//                #region Copy Files to ECM share
//                //if (results.Count > 0)
//                //{
//                //    foreach (var f in results)
//                //    {
//                //        //Copy file from current location
//                //        string fileName = f.Filename.Substring(f.Filename.LastIndexOf(@"\") + 1,
//                //                                               f.Filename.Length - (f.Filename.LastIndexOf(@"\") + 1));
//                //        string sourcePath = f.Filename;
//                //        string targetPath = f.DeliveryLocation;

//                //        // Use Path class to manipulate file and directory paths.



//                //        string destFile = System.IO.Path.Combine(targetPath, fileName);
//                //        logger.CreateUsageLogRecord("SendToECM", $"Coping to target - { destFile }", null, DateTime.Now, 0, Guid.Empty, Guid.Empty, "", "");

//                //        // To copy a file to another location and 
//                //        // overwrite the destination file if it already exists.
//                //        System.IO.File.Copy(f.Filename, destFile, true);

//                //        count++;
//                //    }
//                //}
//                #endregion

//                #region ECM API

//                if (results.Count > 0)
//                {
//                    foreach (var f in results)
//                    {
//                        if (!String.IsNullOrEmpty(f.Filename))
//                        {
//                            string filePath = String.Empty;
//                            filePath = f.Filename;

//                            string ecmUser = ConfigurationManager.AppSettings.Get("ECMUser");
//                            string ecmPassword = ConfigurationManager.AppSettings.Get("ECMPassword");
//                            string ecmUrl = ConfigurationManager.AppSettings.Get("ECMEndPoint");
//                            string type = ConfigurationManager.AppSettings.Get("ECMType");
//                            string uploadUrl = ecmUrl + "nodes";
//                            string fileName = String.Empty;


//                            //if (String.IsNullOrEmpty(destinationNode_Id)) destinationNode_Id = "295375";
//                            string destinationNode_Id = f.DeliveryLocation;
//                            if (!String.IsNullOrEmpty(filePath))
//                            {
//                                fileName = filePath.Substring(filePath.LastIndexOf(@"\"), filePath.Length - filePath.LastIndexOf(@"\"));
//                            }
//                            fileName = fileName.Substring(1, fileName.Length - 1);

//                            //Get OpenText ticket for admin user
//                            string ticketAdmin = GetECMTicket(ecmUrl, ecmUser, ecmPassword, "", "");

//                            //Get OpenText ticket for impersanation
//                            string ticket = GetECMTicket(ecmUrl, ecmUser, ecmPassword, ticketAdmin, results.FirstOrDefault().EmployeeNumber);

//                            if (String.IsNullOrEmpty(ticket))
//                            {
//                                nodes.Add("Unable to acquire security token");
//                                return nodes;
//                            }
//                            // Read file data
//                            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
//                            byte[] data = new byte[fs.Length];
//                            fs.Read(data, 0, data.Length);
//                            fs.Close();

//                            // Generate post objects
//                            Dictionary<string, object> postParameters = new Dictionary<string, object>();
//                            postParameters.Add("type", type);
//                            postParameters.Add("parent_Id", destinationNode_Id);
//                            postParameters.Add("filename", fileName);
//                            postParameters.Add("name", fileName);
//                            postParameters.Add("file", new FormUpload.FileParameter(data, fileName, "application/msword"));

//                            // Create request and receive response
//                            FormUpload.logger = logger;
//                            HttpWebResponse webResponse = FormUpload.MultipartFormDataPost(uploadUrl, ticket, postParameters);
//                            if (webResponse == null)
//                            {
//                                //we already logged error in the [ErrorLogs] table
//                                nodes.Add(FormUpload.otAPIError);
//                            }
//                            else
//                            {
//                                // Process response
//                                StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
//                                string fullResponse = responseReader.ReadToEnd();
//                                Node deserializedNode = JsonConvert.DeserializeObject<Node>(fullResponse);
//                                webResponse.Close();
//                                logger.CreateUsageLogRecord($"Document stored in ECM repository successfully. NodeId {fullResponse}", String.Empty, String.Empty, DateTime.MinValue, 0, Guid.Empty, Guid.Empty, String.Empty, String.Empty);
//                                Console.WriteLine(fullResponse);
//                                nodes.Add(deserializedNode.Id.ToString());
//                            }

//                            //count++;
//                            #endregion
//                        }
//                        else
//                        {
//                            nodes.Add("Document generation failed.");
//                        }
//                    }
//                }
//                return nodes;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.Message);
//                logger.CreateErrorLogRecord(ex.Message, "ECMProcessor - SendToECM");
//                return nodes;
//            }            

//        }

//        public void ProcessEcmMetadata(int nodeId)
//        {

//            string tenantName;
//            tenantName = ConfigurationManager.AppSettings.Get("TenantName");
//            string resourceUri = ConfigurationManager.AppSettings.Get("ResourceUri");
//            string clientId = ConfigurationManager.AppSettings.Get("ida:ClientId");
//            string clientSecret = ConfigurationManager.AppSettings.Get("ida:ClientSecret");
//            string authority = ConfigurationManager.AppSettings.Get("ida:AADInstance") +
//                ConfigurationManager.AppSettings.Get("ida:Domain");
//            string subscriptionKey = ConfigurationManager.AppSettings.Get("SubscriptionKey");
//            //1. Get a template category for a nodeID
//            //ULIODal dal = new ULIODal(tenantName, resourceUri, clientId, clientSecret, authority, subscriptionKey);

//            string query = ConfigurationManager.AppSettings["OrderList"];
//            //dal.GetOrderList(query);



//            //2. Update file metadata
//            //? Where are we going to get values for metadata 
//        }
//        public string GetECMTicket(string apiUrl, string user, string password, string otcsTicket, string impersonUser)
//        {
//            try
//            {
//                //1. Get security token
//                string queryString = "";
//                HttpWebRequest request = null;
//                if (String.IsNullOrEmpty(otcsTicket))
//                {
//                    queryString = apiUrl + $"auth?username={ user }&password={ password }";
//                    request = (HttpWebRequest)WebRequest.Create(@queryString);
//                }
//                else
//                {
//                    //impersonation
//                    queryString = apiUrl + $"auth?username={ user }&password={ password }&impersonate_username={ impersonUser }@global.ul.com";
//                    request = (HttpWebRequest)WebRequest.Create(@queryString);
//                    request.Headers.Add("OTCSTICKET", otcsTicket);
//                }                
               
//                request.ContentType = "application/json";
//                request.Method = "POST";
//                request.ContentLength = 0;
//                request.KeepAlive = false;

//                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

//                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
//                {
//                    string jsonResponse = reader.ReadToEnd();
//                    //ticket = jsonResponse.Substring(11, jsonResponse.Length - 11 - 2).Replace("\\", String.Empty);
//                    Ticket deserializedTicket = JsonConvert.DeserializeObject<Ticket>(jsonResponse);
//                    logger.CreateUsageLogRecord($"OpenText ticket generated successfully", String.Empty, String.Empty, DateTime.MinValue, 0, Guid.Empty, Guid.Empty, String.Empty, String.Empty);
//                    return deserializedTicket.ticket;
//                }
                
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.Message);
//                logger.CreateErrorLogRecord(ex.Message, "ECMProcessor - GetECMTicket");
//                return String.Empty;
//            }

//        }
        
//        //---------------------------------------Will be used to get documents from ECM
//        public void DownloadFile(string nodeId, string desitnationLocation)
//        {
//            #region Test Download from ECM
//            try
//            {
//                //string nodeDestination = "295489";

//                string ecmUser = ConfigurationManager.AppSettings.Get("ECMUser");
//                string ecmPassword = ConfigurationManager.AppSettings.Get("ECMPassword");
//                string ecmUrl = ConfigurationManager.AppSettings.Get("ECMEndPoint");
//                string queryString = ConfigurationManager.AppSettings.Get("ECMEndPoint") + $"nodes/{ nodeId }/Content";

//                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@queryString);
//                request.ContentType = "application/x-www-form-urlencoded";
//                request.ContentLength = 0;
//                request.Headers.Add("OTCSTICKET", GetECMTicket(ecmUrl, ecmUser, ecmPassword,"", ""));
//                request.Method = "GET";
//                request.KeepAlive = false;

//                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

//                string filename = "";
//                string filePath = "";
//                //filePath = @"C:\Temp";
//                filePath = @desitnationLocation;
//                bool exists = System.IO.Directory.Exists(filePath);
//                // try another location
//                if (!exists) filePath = Environment.CurrentDirectory;


//                if (response.Headers.Count > 0)
//                {
//                    for (int i = 0; i < response.Headers.Count; ++i)
//                    {
//                        Console.WriteLine("\nHeader Name:{0}, Value :{1}", response.Headers.Keys[i], response.Headers[i]);
//                        // Get file name
//                        if (response.Headers.Keys[i] == "Content-Disposition")
//                        {
//                            filename = response.Headers[i].Substring(response.Headers[i].IndexOf("filename") + (int)Const.HELPER_PARSE_FILENAME,
//                                response.Headers[i].Length - (response.Headers[i].IndexOf("filename") + (int)Const.HELPER_PARSE_FILENAME));

//                        }
//                    }
//                }

//                Stream responseStream = response.GetResponseStream();
//                //Remove extra characters
//                if (!String.IsNullOrEmpty(filename))
//                {
//                    if (filename.IndexOf(" ") > 0)
//                    {
//                        //if file name has spaces we need to remove encoding characters
//                        filename = filename.Substring(1, filename.Length - 2);
//                    }
//                    else
//                        filename = filename.Substring(0, filename.Length);
//                }
//                logger.CreateUsageLogRecord("Download file", "File was successfully donloaded", filename, DateTime.MinValue, 0, Guid.Empty, Guid.Empty, String.Empty, String.Empty);


//                int fileExtPos = filename.LastIndexOf(".");
//                if (fileExtPos >= 0)
//                {
//                    string ext = filename.Substring(fileExtPos, filename.Length - fileExtPos);
//                    filename = filename.Substring(0, fileExtPos);

//                    filename = filePath + "\\" + filename + ext;
//                    using (Stream file = File.Create(filename))
//                    {
//                        CopyStream(responseStream, file);
//                    }
//                }

//                responseStream.Close();

//                // Releases the resources of the response.
//                response.Close();

//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.Message);

//            }

//            #endregion

//        }
//        public static void CopyStream(Stream input, Stream output)
//        {
//            byte[] buffer = new byte[8 * 1024];
//            int len;
//            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
//            {
//                output.Write(buffer, 0, len);
//            }
//        }
//        //--------------------------------------------------------------------------------
//    }


//    public static class FormUpload
//    {
//        public static string otAPIError { get; set; }
//        public static Logger logger { get; set; }
//        private static readonly Encoding encoding = Encoding.UTF8;
//        public static HttpWebResponse MultipartFormDataPost(string postUrl, string ticket, Dictionary<string, object> postParameters)
//        {
//            string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
//            string contentType = "multipart/form-data; boundary=" + formDataBoundary;

//            try
//            {
//                byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);

//                return PostForm(postUrl, contentType, ticket, formData);
//            }
//            catch (Exception ex)
//            {

//                logger.CreateErrorLogRecord(ex.Message, "ECMProcessor - MultipartFormDataPost");
//                return null;
//            }

//        }
//        private static HttpWebResponse PostForm(string postUrl, string contentType, string ticket, byte[] formData)
//        {
//            try
//            {
//                HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;

//                // Set up the request properties.
//                request.Method = "POST";
//                request.ContentType = contentType;
//                request.CookieContainer = new CookieContainer();
//                request.ContentLength = formData.Length;
//                request.Headers.Add("OTCSTICKET", ticket);
                
//                // Send the form data to the request.
//                using (Stream requestStream = request.GetRequestStream())
//                {
//                    requestStream.Write(formData, 0, formData.Length);
//                    requestStream.Close();
//                }

//                HttpWebResponse returnedResponse = (HttpWebResponse)request.GetResponse();
//                //returnedResponse.Close();
//                return returnedResponse;

//                //return request.GetResponse() as HttpWebResponse;
//            }
//            catch (WebException webExcp)
//            {
//                var resp = new StreamReader(webExcp.Response.GetResponseStream()).ReadToEnd();
//                Error deserializedError = JsonConvert.DeserializeObject<Error>(resp);
                
//                otAPIError = webExcp.Message + " : " + deserializedError.error;
//                logger.CreateErrorLogRecord(otAPIError, "ECMProcessor - PostForm");

//                return null;
//            }
//            catch (Exception e)
//            {

//                otAPIError = e.Message;
//                logger.CreateErrorLogRecord(e.Message, "ECMProcessor - PostForm");
//                return null;
//            }
//            //catch (WebException ex)
//            //{
//            //    ex.Response
//            //    otAPIError = ex.Message;
//            //    logger.CreateErrorLogRecord(ex.Message, "ECMProcessor - PostForm");
//            //    return null;
//            //}

//        }
       
//        private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
//        {
//            Stream formDataStream = new System.IO.MemoryStream();
//            bool needsCLRF = false;

//            foreach (var param in postParameters)
//            {
//                // Add a CRLF to allow multiple parameters to be added.
//                // Skip it on the first parameter, add it to subsequent parameters.
//                if (needsCLRF)
//                    formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

//                needsCLRF = true;

//                if (param.Value is FileParameter)
//                {
//                    FileParameter fileToUpload = (FileParameter)param.Value;

//                    // Add just the first part of this param, since we will write the file data directly to the Stream
//                    string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
//                        boundary,
//                        param.Key,
//                        fileToUpload.FileName ?? param.Key,
//                        fileToUpload.ContentType ?? "application/octet-stream");

//                    formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

//                    // Write the file data directly to the Stream, rather than serializing it to a string.
//                    formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
//                }
//                else
//                {
//                    string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
//                        boundary,
//                        param.Key,
//                        param.Value);
//                    formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
//                }
//            }

//            // Add the end of the request.  Start with a newline
//            string footer = "\r\n--" + boundary + "--\r\n";
//            formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

//            // Dump the Stream into a byte[]
//            formDataStream.Position = 0;
//            byte[] formData = new byte[formDataStream.Length];
//            formDataStream.Read(formData, 0, formData.Length);
//            formDataStream.Close();

//            return formData;
//        }
//        public class FileParameter
//        {
//            public byte[] File { get; set; }
//            public string FileName { get; set; }
//            public string ContentType { get; set; }
//            public FileParameter(byte[] file) : this(file, null) { }
//            public FileParameter(byte[] file, string filename) : this(file, filename, null) { }
//            public FileParameter(byte[] file, string filename, string contenttype)
//            {
//                File = file;
//                FileName = filename;
//                ContentType = contenttype;
//            }
//        }
//    }



//    //public static string SendFile()
//    //{
//    //    WebResponse response = null;
//    //    try
//    //    {
//    //        string filePath = @"C:\_PSC\Clients\UL\VM Docs\EmptyDocW.docx";
//    //        string sWebAddress = ConfigurationManager.AppSettings.Get("ECMEndPoint") + "nodes";

//    //        string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
//    //        byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
//    //        HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(sWebAddress);
//    //        wr.ContentType = "multipart/form-data; boundary=" + boundary;
//    //        wr.Method = "POST";
//    //        wr.KeepAlive = true;
//    //        wr.Headers.Add("OTCSTICKET", GetECMTicket());
//    //        //wr.Credentials = System.Net.CredentialCache.DefaultCredentials;
//    //        Stream stream = wr.GetRequestStream();
//    //        string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";

//    //        stream.Write(boundarybytes, 0, boundarybytes.Length);
//    //        byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(filePath);
//    //        stream.Write(formitembytes, 0, formitembytes.Length);
//    //        stream.Write(boundarybytes, 0, boundarybytes.Length);
//    //        string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
//    //        string header = string.Format(headerTemplate, "file", Path.GetFileName(filePath), Path.GetExtension(filePath));
//    //        byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
//    //        stream.Write(headerbytes, 0, headerbytes.Length);

//    //        FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
//    //        byte[] buffer = new byte[4096];
//    //        int bytesRead = 0;
//    //        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
//    //            stream.Write(buffer, 0, bytesRead);
//    //        fileStream.Close();

//    //        byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
//    //        stream.Write(trailer, 0, trailer.Length);
//    //        stream.Close();

//    //        response = wr.GetResponse();
//    //        Stream responseStream = response.GetResponseStream();
//    //        StreamReader streamReader = new StreamReader(responseStream);
//    //        string responseData = streamReader.ReadToEnd();
//    //        return responseData;
//    //    }
//    //    catch (Exception ex)
//    //    {
//    //        return ex.Message;
//    //    }
//    //    finally
//    //    {
//    //        if (response != null)
//    //            response.Close();
//    //    }
//    //}


//}
