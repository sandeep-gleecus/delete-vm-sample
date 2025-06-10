using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.Rest
{
    /// <summary>
    /// Overrides the standard content type mapper when using the Newtonsoft JSON serializer
    /// </summary>
    public class NewtonsoftJsonContentTypeMapper : WebContentTypeMapper
    {
        public override WebContentFormat GetMessageFormatForContentType(string contentType)
        {
            //If XML use the standard formatters, otherwise use the RAW type since we'll be using Newtonsoft JSON
            if (contentType.Contains("text/xml") || contentType.Contains("application/xml"))
            {
                return WebContentFormat.Xml;
            }
            else
            {
                return WebContentFormat.Raw;
            }
        }
    }
}