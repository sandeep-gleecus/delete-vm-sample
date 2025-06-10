using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.Configuration;

namespace Inflectra.SpiraTest.Web.Services.Wsdl.Documentation
{
    public enum XmlCommentFormat { Default, Portable };

    [AttributeUsage(AttributeTargets.Interface)]
    public class XmlCommentsAttribute : Attribute, IContractBehavior, IWsdlExportExtension
    {
        bool initialized;
        XmlCommentFormat format;
        string implementationClass = String.Empty;

        public XmlCommentsAttribute()
        {
        }

        public XmlCommentsAttribute(XmlCommentFormat format)
        {
            this.format = format;
        }

        /// <summary>
        /// Adds the normal C# comments to the generated WSDL to make documentation automatic
        /// </summary>
        /// <param name="format">The format to use</param>
        /// <param name="implementationClass">
        /// The name of the class implementing the interface whose comments should be used (leave blank to use the interface)
        /// </param>
        public XmlCommentsAttribute(XmlCommentFormat format, string implementationClass)
        {
            this.format = format;
            this.implementationClass = implementationClass;
        }

        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.DispatchRuntime dispatchRuntime)
        {
        }

        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
        }

        public void ExportContract(WsdlExporter exporter, WsdlContractConversionContext context)
        {
            XmlCommentsExporter.ExportContract(exporter, context, Format, ImplementationClass);
        }

        public void ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext context)
        {
            XmlCommentsExporter.ExportEndpoint(exporter, Format);
        }

        public string ImplementationClass
        {
            get
            {
                return this.implementationClass;
            }
        }

        public XmlCommentFormat Format
        {
            get
            {
                if (!initialized)
                {
                    initialized = true;
                    XmlCommentsConfig config =  XmlCommentsConfig.GetConfiguration();
                    if (config != null)
                        format = config.Format;
                }
                return format;
            }
            set
            {
                format = value;
            }
        }
    }

}
