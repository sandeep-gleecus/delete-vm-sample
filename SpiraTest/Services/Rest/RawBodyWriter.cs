using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Web;
using System.Xml;

namespace Inflectra.SpiraTest.Web.Services.Rest
{
    public class RawBodyWriter : BodyWriter
    {
        private readonly byte[] _content;

        public RawBodyWriter(byte[] content)
            : base(true)
        {
            _content = content;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement("Binary");
            writer.WriteBase64(_content, 0, _content.Length);
            writer.WriteEndElement();
        }
    }
}