using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace Inflectra.SpiraTest.Web.JqPlot
{
    /// <summary>
    /// Echoes back the image data as a SVG download
    /// </summary>
    public class GraphImageSvg : IHttpHandler
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

                //Replace some known malformed SVG to make it open properly
                imageData = imageData.Replace("<svg style=\"overflow: hidden;\"", "<svg xmlns=\"http://www.w3.org/2000/svg\"");

                context.Response.ContentType = "image/svg-xml";
                context.Response.AddHeader("Content-Type", "image/svg-xml");
                context.Response.AddHeader("Content-Disposition", "attachment; filename=graph.svg");
                context.Response.Write(imageData);
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