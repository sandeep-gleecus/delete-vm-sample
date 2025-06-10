using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Dispatcher;
using System.ServiceModel;
using System.Net;
using System.ServiceModel.Channels;
using System.Runtime.Serialization.Json;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.Services.Ajax.DataObjects;
using Inflectra.SpiraTest.Web.Classes;

namespace Inflectra.SpiraTest.Web.Services.Ajax.Json
{
    internal class SpiraErrorHandler : IErrorHandler
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Ajax.Json.SpiraErrorHandler::";

        public bool HandleError(Exception error)
        {
            //Tell the system that we handle all errors here. 
            return true;
        }

        public void ProvideFault(Exception error, System.ServiceModel.Channels.MessageVersion version, ref System.ServiceModel.Channels.Message fault)
        {
            const string METHOD_NAME = "ProvideFault";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            //See if we have one of the special case exceptions that we need to handle separately
            if (typeof(SourceCodeProviderGeneralException).IsAssignableFrom(error.GetType()))
            {
                //User displayable error
                JsonError msErrObject = new JsonError { ExceptionType = Resources.Messages.SourceCode_ProviderError, Message = error.Message, FaultCode = -1, StackTrace = "" };

                // create a fault message containing our FaultContract object 
                fault = Message.CreateMessage(version, "", msErrObject, new DataContractJsonSerializer(msErrObject.GetType()));

                // tell WCF to use JSON encoding rather than default XML 
                var wbf = new WebBodyFormatMessageProperty(WebContentFormat.Json);
                fault.Properties.Add(WebBodyFormatMessageProperty.Name, wbf);

                //Modify response 
                var rmp = new HttpResponseMessageProperty();

                rmp.Headers[HttpResponseHeader.ContentType] = "application/json";
                rmp.Headers["jsonerror"] = "true";

                //Internal server error with exception mesasage as status. 
                rmp.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                rmp.StatusDescription = error.Message;

                fault.Properties.Add(HttpResponseMessageProperty.Name, rmp);
            }
            else if (error.GetType() == typeof(System.Data.SqlClient.SqlException) && ((System.Data.SqlClient.SqlException)error).Number == -2)
            {
                //We have a timeout, give a more meaningful error message
                //User displayable error
                JsonError msErrObject = new JsonError {
                    ExceptionType = "DatabaseException",
                    Message = Resources.Messages.Global_DatabaseTimeout, 
                    FaultCode = -1,
                    StackTrace = "" };

                // create a fault message containing our FaultContract object 
                fault = Message.CreateMessage(version, "", msErrObject, new DataContractJsonSerializer(msErrObject.GetType()));

                // tell WCF to use JSON encoding rather than default XML 
                var wbf = new WebBodyFormatMessageProperty(WebContentFormat.Json);
                fault.Properties.Add(WebBodyFormatMessageProperty.Name, wbf);

                //Modify response 
                var rmp = new HttpResponseMessageProperty();

                rmp.Headers[HttpResponseHeader.ContentType] = "application/json";
                rmp.Headers["jsonerror"] = "true";

                //Internal server error with exception mesasage as status. 
                rmp.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                rmp.StatusDescription = error.Message;

                fault.Properties.Add(HttpResponseMessageProperty.Name, rmp);
            }
            else if (error.GetType() == typeof(SourceCodeCacheInvalidException))
            {
                //The source code cache is not ready, display a 'friendly' error
                //User displayable error
                JsonError msErrObject = new JsonError
                {
                    ExceptionType = Resources.Messages.SourceCode_ProviderInitializing,
                    Message = Resources.Messages.SourceCodeList_CacheNotReadyError,
                    FaultCode = -1,
                    StackTrace = ""
                };

                // create a fault message containing our FaultContract object 
                fault = Message.CreateMessage(version, "", msErrObject, new DataContractJsonSerializer(msErrObject.GetType()));

                // tell WCF to use JSON encoding rather than default XML 
                var wbf = new WebBodyFormatMessageProperty(WebContentFormat.Json);
                fault.Properties.Add(WebBodyFormatMessageProperty.Name, wbf);

                //Modify response 
                var rmp = new HttpResponseMessageProperty();

                rmp.Headers[HttpResponseHeader.ContentType] = "application/json";
                rmp.Headers["jsonerror"] = "true";

                //Internal server error with exception mesasage as status. 
                rmp.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                rmp.StatusDescription = error.Message;

                fault.Properties.Add(HttpResponseMessageProperty.Name, rmp);
            }
            else if (error.GetType() == typeof(System.Data.OptimisticConcurrencyException))
            {
                //We have a concurrency issue, give a more meaningful error message
                //User displayable error
                JsonError msErrObject = new JsonError
                {
                    ExceptionType = "ConcurrencyException",
                    Message = Resources.Messages.Global_DataChangedBySomeoneElse,
                    FaultCode = -1,
                    StackTrace = ""
                };

                // create a fault message containing our FaultContract object 
                fault = Message.CreateMessage(version, "", msErrObject, new DataContractJsonSerializer(msErrObject.GetType()));

                // tell WCF to use JSON encoding rather than default XML 
                var wbf = new WebBodyFormatMessageProperty(WebContentFormat.Json);
                fault.Properties.Add(WebBodyFormatMessageProperty.Name, wbf);

                //Modify response 
                var rmp = new HttpResponseMessageProperty();

                rmp.Headers[HttpResponseHeader.ContentType] = "application/json";
                rmp.Headers["jsonerror"] = "true";

                //Internal server error with exception mesasage as status. 
                rmp.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                rmp.StatusDescription = error.Message;

                fault.Properties.Add(HttpResponseMessageProperty.Name, rmp);
            }
            else if (error.GetType() == typeof(DataValidationException))
            {
                //Data validation errors
                JsonError msErrObject = new JsonError { ExceptionType = /*Do Not Localize*/"DataValidationException", Message = error.Message, FaultCode = -1, StackTrace = "" };

                // create a fault message containing our FaultContract object 
                fault = Message.CreateMessage(version, "", msErrObject, new DataContractJsonSerializer(msErrObject.GetType()));

                // tell WCF to use JSON encoding rather than default XML 
                var wbf = new WebBodyFormatMessageProperty(WebContentFormat.Json);
                fault.Properties.Add(WebBodyFormatMessageProperty.Name, wbf);

                //Modify response 
                var rmp = new HttpResponseMessageProperty();

                rmp.Headers[HttpResponseHeader.ContentType] = "application/json";
                rmp.Headers["jsonerror"] = "true";

                //Internal server error with exception mesasage as status. 
                rmp.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                rmp.StatusDescription = error.Message;

                fault.Properties.Add(HttpResponseMessageProperty.Name, rmp);
            }
            else if (error.GetType() == typeof(DataValidationExceptionEx))
            {
                //Data validation errors where we have a list of messages
                DataValidationExceptionEx dataValidationExceptionEx = (DataValidationExceptionEx)error;

                //Data validation errors
                JsonError msErrObject = new JsonError();
                msErrObject.FaultCode = -1;
                msErrObject.ExceptionType = /*Do Not Localize*/"DataValidationExceptionEx";

                //We serialize the messages JSON string
                //[{'FieldName':'xxx','Message':'xxx'},...]
                string messageSerialized = "[";
                bool first = true;
                foreach (ValidationMessage message in dataValidationExceptionEx.Messages)
                {
                    if (!first)
                    {
                        messageSerialized += ",";
                    }
                    first = false;
                    messageSerialized += "{FieldName:'" + GlobalFunctions.JSEncode(message.FieldName) + "',Message:'" + GlobalFunctions.JSEncode(message.Message) + "'}";
                }
                messageSerialized += "]";
                msErrObject.Message = messageSerialized;

                // create a fault message containing our FaultContract object 
                fault = Message.CreateMessage(version, "", msErrObject, new DataContractJsonSerializer(msErrObject.GetType()));

                // tell WCF to use JSON encoding rather than default XML 
                var wbf = new WebBodyFormatMessageProperty(WebContentFormat.Json);
                fault.Properties.Add(WebBodyFormatMessageProperty.Name, wbf);

                //Modify response 
                var rmp = new HttpResponseMessageProperty();

                rmp.Headers[HttpResponseHeader.ContentType] = "application/json";
                rmp.Headers["jsonerror"] = "true";

                //Internal server error with exception mesasage as status. 
                rmp.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                rmp.StatusDescription = "DataValidationExceptionEx";

                fault.Properties.Add(HttpResponseMessageProperty.Name, rmp);
            }
            else
            {
                if (error is FaultException<int>)
                {
                    FaultException<int> fe = (FaultException<int>)error;

                    //Detail for the returned value 
                    int faultCode = fe.Detail;
                    string cause = fe.Message;

                    //The json serializable object 
                    //Clean the product name from the stack trace
                    string exceptionType = error.GetType().FullName;
                    exceptionType = exceptionType.Replace("Inflectra.SpiraTest", "APPLICATION");
                    JsonError msErrObject = new JsonError { ExceptionType = exceptionType, Message = cause, FaultCode = faultCode, StackTrace = error.StackTrace };

                    //The fault to be returned 
                    fault = Message.CreateMessage(version, "", msErrObject, new DataContractJsonSerializer(msErrObject.GetType()));

                    // tell WCF to use JSON encoding rather than default XML 
                    WebBodyFormatMessageProperty wbf = new WebBodyFormatMessageProperty(WebContentFormat.Json);

                    // Add the formatter to the fault 
                    fault.Properties.Add(WebBodyFormatMessageProperty.Name, wbf);

                    //Modify response 
                    HttpResponseMessageProperty rmp = new HttpResponseMessageProperty();

                    // return custom error code, 400. 
                    rmp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    rmp.StatusDescription = "Bad request";

                    //Mark the jsonerror and json content 
                    rmp.Headers[HttpResponseHeader.ContentType] = "application/json";
                    rmp.Headers["jsonerror"] = "true";

                    //Add to fault 
                    fault.Properties.Add(HttpResponseMessageProperty.Name, rmp);
                }
                else
                {
                    //Arbitrary error 
                    //Clean the product name from the stack trace
                    string exceptionType = error.GetType().FullName;
                    exceptionType = exceptionType.Replace("Inflectra.SpiraTest", "APPLICATION");
                    JsonError msErrObject = new JsonError { ExceptionType = exceptionType, Message = error.Message, FaultCode = -1, StackTrace = error.StackTrace };

                    // create a fault message containing our FaultContract object 
                    fault = Message.CreateMessage(version, "", msErrObject, new DataContractJsonSerializer(msErrObject.GetType()));

                    // tell WCF to use JSON encoding rather than default XML 
                    var wbf = new WebBodyFormatMessageProperty(WebContentFormat.Json);
                    fault.Properties.Add(WebBodyFormatMessageProperty.Name, wbf);

                    //Modify response 
                    var rmp = new HttpResponseMessageProperty();

                    rmp.Headers[HttpResponseHeader.ContentType] = "application/json";
                    rmp.Headers["jsonerror"] = "true";

                    //Internal server error with exception mesasage as status. 
                    rmp.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                    rmp.StatusDescription = error.Message;

                    fault.Properties.Add(HttpResponseMessageProperty.Name, rmp);
                }

                //Don't log certain 'known exceptions' that are already logged elsewhere
                if (!(error is XsrfViolationException))
                {
                    Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, error);
                }
            }

            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
        }
    }

    /// <summary>
    /// Special exception used to pass data validation messages
    /// </summary>
    public class DataValidationExceptionEx : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messages">The list of data validation messages</param>
        public DataValidationExceptionEx(List<ValidationMessage> messages)
            : base("DataValidationExceptionEx")
        {
            this.messages = messages;
        }

        /// <summary>
        /// 
        /// </summary>
        public List<ValidationMessage> Messages
        {
            get
            {
                return this.messages;
            }
        }
        protected List<ValidationMessage> messages;
    }
}