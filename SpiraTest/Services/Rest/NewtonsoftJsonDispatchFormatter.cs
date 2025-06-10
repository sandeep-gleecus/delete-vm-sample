using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;
using System.IO;
using System.Net;
using Newtonsoft.Json.Converters;
using System.Globalization;
using Inflectra.SpiraTest.Common;
using System.Xml;

namespace Inflectra.SpiraTest.Web.Services.Rest
{
    /// <summary>
    /// Uses the Newtonsoft JSON library to handle JSON serialization/deserialization
    /// </summary>
    public class NewtonsoftJsonDispatchFormatter : IDispatchMessageFormatter
    {
        private readonly OperationDescription _operation;
        private readonly Dictionary<string, int> _parameterNames;
        private IDispatchMessageFormatter _fallbackFormatter;

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="operation">The operation</param>
        /// <param name="isRequest">Is it a request or a response being formatted/serialized</param>
        /// <param name="fallbackFormatter">The standard WCF formatter that we should use for XML data</param>
        public NewtonsoftJsonDispatchFormatter(OperationDescription operation, bool isRequest, IDispatchMessageFormatter fallbackFormatter)
        {
            _operation = operation;
            _fallbackFormatter = fallbackFormatter;
            if (isRequest)
            {
                var operationParameterCount = operation.Messages[0].Body.Parts.Count;
                if (operationParameterCount > 1)
                {
                    _parameterNames = new Dictionary<string, int>();
                    for (var i = 0; i < operationParameterCount; i++)
                    {
                        _parameterNames.Add(operation.Messages[0].Body.Parts[i].Name, i);
                    }
                }
            }
        }

        public void DeserializeRequest(Message message, object[] parameters)
        {
            try
            {
                if (message.IsEmpty)
                    return;

                //Make sure we're using JSON not XML
                if (WebOperationContext.Current.IncomingRequest.Accept != null &&
                    (WebOperationContext.Current.IncomingRequest.Accept.Contains("application/xml") || WebOperationContext.Current.IncomingRequest.Accept.Contains("text/xml") ||
                    WebOperationContext.Current.IncomingRequest.ContentType.Contains("application/xml") || WebOperationContext.Current.IncomingRequest.ContentType.Contains("text/xml")))
                {
                    //XML
                    _fallbackFormatter.DeserializeRequest(message, parameters);
                }
                else
                {
                    object bodyFormatProperty;
                    if (!message.Properties.TryGetValue(WebBodyFormatMessageProperty.Name, out bodyFormatProperty) ||
                         (bodyFormatProperty as WebBodyFormatMessageProperty).Format != WebContentFormat.Raw)
                    {
                        throw new InvalidOperationException("Incoming messages must have a body format of Raw. Is a ContentTypeMapper set on the WebHttpBinding?");
                    }

                    using (XmlDictionaryReader bodyReader = message.GetReaderAtBodyContents())
                    {
                        bodyReader.ReadStartElement("Binary");
                        byte[] rawBody = bodyReader.ReadContentAsBase64();
                        using (MemoryStream memoryStream = new MemoryStream(rawBody))
                        {
                            using (StreamReader streamReader = new StreamReader(memoryStream, Encoding.UTF8))
                            {
                                Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                                serializer.Converters.Add(new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ", DateTimeStyles = DateTimeStyles.AssumeUniversal });
                                if (parameters.Length == 1)
                                {
                                    // single parameter, assuming bare
                                    //The line below is only used for debugging purposes
                                    //string json1 = streamReader.ReadToEnd();
                                    int partCount = _operation.Messages[0].Body.Parts.Count;
                                    Type objType = _operation.Messages[0].Body.Parts[partCount - 1].Type;
                                    parameters[0] = serializer.Deserialize(streamReader, objType);
                                }
                                else
                                {
                                    // multiple parameter, needs to be wrapped
                                    Newtonsoft.Json.JsonReader reader = new Newtonsoft.Json.JsonTextReader(streamReader);
                                    reader.Read();
                                    if (reader.TokenType != Newtonsoft.Json.JsonToken.StartObject)
                                    {
                                        throw new InvalidOperationException("Input needs to be wrapped in an object");
                                    }

                                    reader.Read();
                                    while (reader.TokenType == Newtonsoft.Json.JsonToken.PropertyName)
                                    {
                                        string parameterName = reader.Value as string;
                                        reader.Read();
                                        if (_parameterNames.ContainsKey(parameterName))
                                        {
                                            int parameterIndex = _parameterNames[parameterName];
                                            parameters[parameterIndex] = serializer.Deserialize(reader, _operation.Messages[0].Body.Parts[parameterIndex].Type);
                                        }
                                        else
                                        {
                                            reader.Skip();
                                        }

                                        reader.Read();
                                    }

                                    reader.Close();
                                }

                                streamReader.Close();
                            }
                            memoryStream.Close();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent("NewtonsoftJsonDispatchFormatter.DeserializeRequest", exception);
                throw exception;
            }
        }

        public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        {
            //Make sure we're using JSON not XML
            if (WebOperationContext.Current.IncomingRequest.Accept != null &&
                (WebOperationContext.Current.IncomingRequest.Accept.Contains("application/xml") || WebOperationContext.Current.IncomingRequest.Accept.Contains("text/xml") ||
                WebOperationContext.Current.IncomingRequest.ContentType.Contains("application/xml") || WebOperationContext.Current.IncomingRequest.ContentType.Contains("text/xml")))
            {
                //XML
                return _fallbackFormatter.SerializeReply(messageVersion, parameters, result);
            }
            else
            {
                //JSON
                byte[] body;
                Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                serializer.Converters.Add(new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ", DateTimeStyles = DateTimeStyles.RoundtripKind });

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    //We need to specify that no BOM should be sent with the response.
                    UTF8Encoding utf8NoBom = new UTF8Encoding(false);
                    using (StreamWriter streamWriter = new StreamWriter(memoryStream, utf8NoBom))
                    {
                        using (Newtonsoft.Json.JsonWriter writer = new Newtonsoft.Json.JsonTextWriter(streamWriter))
                        {
                            //writer.Formatting = Newtonsoft.Json.Formatting.Indented;
                            serializer.Serialize(writer, result);
                            streamWriter.Flush();
                            body = memoryStream.ToArray();
                        }
                    }
                }

                var replyMessage = Message.CreateMessage(messageVersion, _operation.Messages[1].Action, new RawBodyWriter(body));
                replyMessage.Properties.Add(WebBodyFormatMessageProperty.Name, new WebBodyFormatMessageProperty(WebContentFormat.Raw));
                var respProp = new HttpResponseMessageProperty();
                respProp.Headers[HttpResponseHeader.ContentType] = "application/json";
                replyMessage.Properties.Add(HttpResponseMessageProperty.Name, respProp);
                return replyMessage;
            }
        }
    }
}