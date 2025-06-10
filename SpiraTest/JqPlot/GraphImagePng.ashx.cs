using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;

namespace Inflectra.SpiraTest.Web.JqPlot
{
    /// <summary>
    /// Echoes back the image data as a PNG download
    /// </summary>
    public class GraphImagePng : IHttpHandler
    {
        /// <summary>
        /// Render out the graph image
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            //We don't need to authorize the user since the full data packet is being sent in the POST anyway

            //If we're passed a path in the URL, then we need to send back the data,
            //otherwise store the data in a temporary location and then send back the path

            if (String.IsNullOrEmpty(context.Request.QueryString["guid"]))
            {
                //We are receiving the post data (first phase)

                //Get the raw data from the POST
                StreamReader streamReader = new StreamReader(context.Request.InputStream);
                string imageData = streamReader.ReadToEnd();
                
                //Save the file in a temporary location, using a GUID filename
                Guid guid = Guid.NewGuid();
                string folder = System.IO.Path.GetTempPath();
                string filename = guid.ToString() + ".dat";
                string path = Path.Combine(folder, filename);
                StreamWriter writer = File.CreateText(path);
                writer.Write(imageData);
                writer.Close();

                //Return the guid
                context.Response.ContentType = "text/plain";
                context.Response.Write(guid.ToString());
            }
            else
            {
                //We are sending back the post data (second phase)

                //Extract the guid from the querystring and use it to retrieve the file
                string guid = context.Request.QueryString["guid"].Trim();
                string folder = System.IO.Path.GetTempPath();
                string filename = guid.ToString() + ".dat";
                string path = Path.Combine(folder, filename);
                StreamReader reader = File.OpenText(path);
                string imageData = reader.ReadToEnd();

                // Remove the headers (data:,) part. 
                // A real application should use them according to needs such as to check image type
                string filteredData = imageData.Substring(imageData.IndexOf(',') + 1);

                // Need to decode before saving since the data we received is already base64 encoded
                byte[] unencodedData = Convert.FromBase64String(filteredData);

                context.Response.ContentType = "image/png";
                context.Response.AddHeader("Content-Type", "image/png");
                context.Response.AddHeader("Content-Disposition", "attachment; filename=graph.png");
                context.Response.BinaryWrite(unencodedData);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}