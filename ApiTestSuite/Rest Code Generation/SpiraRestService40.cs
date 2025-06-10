using System;
using System.Collections.Generic;
using System.Linq;

using Inflectra.SpiraTest.ApiTestSuite.Rest_Code_Generation;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Inflectra.SpiraTest.ApiTestSuite.Rest_Code_Generation.SpiraRestService40
{
    /// <summary>
    /// The auto-generated wrapper for calling Spira REST web services
    /// </summary>
	public partial class SpiraRestClient40
	{
		protected string baseUrl = "http://tardis/spira/services/v4_0/RestService.svc";

		public RemoteDocument[] Document_RetrieveForArtifact(RemoteCredentials credentials, object project_id, object artifact_type_id, object artifact_id)
		{
			string path = "projects/{project_id}/artifact-types/{artifact_type_id}/artifacts/{artifact_id}/documents";

            //Create the request
            RestRequest request = new RestRequest("Document_RetrieveForArtifact");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "artifact_type_id", artifact_type_id);
			AddRequestParameter(request, "artifact_id", artifact_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDocument[] responseObj = JsonConvert.DeserializeObject<RemoteDocument[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_RetrieveForArtifact", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDocument[] responseObj = JsonConvert.DeserializeObject<RemoteDocument[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void Document_AddToArtifactId(RemoteCredentials credentials, object project_id, object artifact_type_id, object artifact_id, object document_id)
		{
			string path = "projects/{project_id}/artifact-types/{artifact_type_id}/artifacts/{artifact_id}/documents/{document_id}";

            //Create the request
            RestRequest request = new RestRequest("Document_AddToArtifactId");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "artifact_type_id", artifact_type_id);
			AddRequestParameter(request, "artifact_id", artifact_id);
			AddRequestParameter(request, "document_id", document_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void Document_DeleteFromArtifact(RemoteCredentials credentials, object project_id, object artifact_type_id, object artifact_id, object document_id)
		{
			string path = "projects/{project_id}/artifact-types/{artifact_type_id}/artifacts/{artifact_id}/documents/{document_id}";

            //Create the request
            RestRequest request = new RestRequest("Document_DeleteFromArtifact");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "DELETE";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "artifact_type_id", artifact_type_id);
			AddRequestParameter(request, "artifact_id", artifact_id);
			AddRequestParameter(request, "document_id", document_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteAssociation Association_Create(RemoteCredentials credentials, object project_id, RemoteAssociation remoteAssociation)
		{
			string path = "projects/{project_id}/associations";

            //Create the request
            RestRequest request = new RestRequest("Association_Create");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteAssociation == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteAssociation, settings);
				Common.Logger.LogTraceEvent("Association_Create", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAssociation responseObj = JsonConvert.DeserializeObject<RemoteAssociation>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Association_Create", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAssociation responseObj = JsonConvert.DeserializeObject<RemoteAssociation>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void Association_Update(RemoteCredentials credentials, object project_id, RemoteAssociation remoteAssociation)
		{
			string path = "projects/{project_id}/associations";

            //Create the request
            RestRequest request = new RestRequest("Association_Update");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "PUT";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteAssociation == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteAssociation, settings);
				Common.Logger.LogTraceEvent("Association_Update", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteAssociation[] Association_RetrieveForArtifact(RemoteCredentials credentials, object project_id, object artifact_type_id, object artifact_id)
		{
			string path = "projects/{project_id}/associations/{artifact_type_id}/{artifact_id}";

            //Create the request
            RestRequest request = new RestRequest("Association_RetrieveForArtifact");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "artifact_type_id", artifact_type_id);
			AddRequestParameter(request, "artifact_id", artifact_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAssociation[] responseObj = JsonConvert.DeserializeObject<RemoteAssociation[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Association_RetrieveForArtifact", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAssociation[] responseObj = JsonConvert.DeserializeObject<RemoteAssociation[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteAutomationEngine AutomationEngine_RetrieveByToken(RemoteCredentials credentials, object token)
		{
			string path = "automation-engines/tokens{token}";

            //Create the request
            RestRequest request = new RestRequest("AutomationEngine_RetrieveByToken");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "token", token);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomationEngine responseObj = JsonConvert.DeserializeObject<RemoteAutomationEngine>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("AutomationEngine_RetrieveByToken", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomationEngine responseObj = JsonConvert.DeserializeObject<RemoteAutomationEngine>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteAutomationEngine[] AutomationEngine_Retrieve(RemoteCredentials credentials, object active_only)
		{
			string path = "automation-engines?active_only={active_only}";

            //Create the request
            RestRequest request = new RestRequest("AutomationEngine_Retrieve");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "active_only", active_only);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomationEngine[] responseObj = JsonConvert.DeserializeObject<RemoteAutomationEngine[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("AutomationEngine_Retrieve", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomationEngine[] responseObj = JsonConvert.DeserializeObject<RemoteAutomationEngine[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteAutomationEngine AutomationEngine_Create(RemoteCredentials credentials, RemoteAutomationEngine remoteEngine)
		{
			string path = "automation-engines";

            //Create the request
            RestRequest request = new RestRequest("AutomationEngine_Create");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			

			//Specify the body (if appropriate)
			if (remoteEngine == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteEngine, settings);
				Common.Logger.LogTraceEvent("AutomationEngine_Create", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomationEngine responseObj = JsonConvert.DeserializeObject<RemoteAutomationEngine>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("AutomationEngine_Create", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomationEngine responseObj = JsonConvert.DeserializeObject<RemoteAutomationEngine>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteAutomationEngine AutomationEngine_RetrieveById(RemoteCredentials credentials, object automation_engine_id)
		{
			string path = "automation-engines/{automation_engine_id}";

            //Create the request
            RestRequest request = new RestRequest("AutomationEngine_RetrieveById");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "automation_engine_id", automation_engine_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomationEngine responseObj = JsonConvert.DeserializeObject<RemoteAutomationEngine>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("AutomationEngine_RetrieveById", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomationEngine responseObj = JsonConvert.DeserializeObject<RemoteAutomationEngine>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteAutomationHost[] AutomationHost_Retrieve1(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/automation-hosts";

            //Create the request
            RestRequest request = new RestRequest("AutomationHost_Retrieve1");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomationHost[] responseObj = JsonConvert.DeserializeObject<RemoteAutomationHost[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("AutomationHost_Retrieve1", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomationHost[] responseObj = JsonConvert.DeserializeObject<RemoteAutomationHost[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteAutomationHost[] AutomationHost_Retrieve2(RemoteCredentials credentials, object project_id, object starting_row, object number_of_rows, object sort_field, object sort_direction, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/automation-hosts/search?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}";

            //Create the request
            RestRequest request = new RestRequest("AutomationHost_Retrieve2");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "starting_row", starting_row);
			AddRequestParameter(request, "number_of_rows", number_of_rows);
			AddRequestParameter(request, "sort_field", sort_field);
			AddRequestParameter(request, "sort_direction", sort_direction);
			

			//Specify the body (if appropriate)
			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteFilters, settings);
				Common.Logger.LogTraceEvent("AutomationHost_Retrieve2", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomationHost[] responseObj = JsonConvert.DeserializeObject<RemoteAutomationHost[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("AutomationHost_Retrieve2", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomationHost[] responseObj = JsonConvert.DeserializeObject<RemoteAutomationHost[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteAutomationHost AutomationHost_RetrieveById(RemoteCredentials credentials, object project_id, object automation_host_id)
		{
			string path = "projects/{project_id}/automation-hosts/{automation_host_id}";

            //Create the request
            RestRequest request = new RestRequest("AutomationHost_RetrieveById");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "automation_host_id", automation_host_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomationHost responseObj = JsonConvert.DeserializeObject<RemoteAutomationHost>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("AutomationHost_RetrieveById", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomationHost responseObj = JsonConvert.DeserializeObject<RemoteAutomationHost>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteAutomationHost AutomationHost_RetrieveByToken(RemoteCredentials credentials, object project_id, object token)
		{
			string path = "projects/{project_id}/automation-hosts/tokens/{token}";

            //Create the request
            RestRequest request = new RestRequest("AutomationHost_RetrieveByToken");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "token", token);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomationHost responseObj = JsonConvert.DeserializeObject<RemoteAutomationHost>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("AutomationHost_RetrieveByToken", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomationHost responseObj = JsonConvert.DeserializeObject<RemoteAutomationHost>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteAutomationHost AutomationHost_Create(RemoteCredentials credentials, object project_id, RemoteAutomationHost remoteAutomationHost)
		{
			string path = "projects/{project_id}/automation-hosts";

            //Create the request
            RestRequest request = new RestRequest("AutomationHost_Create");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteAutomationHost == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteAutomationHost, settings);
				Common.Logger.LogTraceEvent("AutomationHost_Create", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomationHost responseObj = JsonConvert.DeserializeObject<RemoteAutomationHost>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("AutomationHost_Create", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomationHost responseObj = JsonConvert.DeserializeObject<RemoteAutomationHost>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void AutomationHost_Update(RemoteCredentials credentials, object project_id, RemoteAutomationHost remoteAutomationHost)
		{
			string path = "projects/{project_id}/automation-hosts";

            //Create the request
            RestRequest request = new RestRequest("AutomationHost_Update");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "PUT";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteAutomationHost == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteAutomationHost, settings);
				Common.Logger.LogTraceEvent("AutomationHost_Update", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void AutomationHost_Delete(RemoteCredentials credentials, object project_id, object automation_host_id)
		{
			string path = "projects/{project_id}/automation-hosts/{automation_host_id}";

            //Create the request
            RestRequest request = new RestRequest("AutomationHost_Delete");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "DELETE";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "automation_host_id", automation_host_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteCustomList CustomProperty_RetrieveCustomListById(RemoteCredentials credentials, object project_id, object custom_list_id)
		{
			string path = "projects/{project_id}/custom-lists/{custom_list_id}";

            //Create the request
            RestRequest request = new RestRequest("CustomProperty_RetrieveCustomListById");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "custom_list_id", custom_list_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteCustomList responseObj = JsonConvert.DeserializeObject<RemoteCustomList>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("CustomProperty_RetrieveCustomListById", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteCustomList responseObj = JsonConvert.DeserializeObject<RemoteCustomList>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteCustomList CustomProperty_AddCustomList(RemoteCredentials credentials, object project_id, RemoteCustomList remoteCustomList)
		{
			string path = "projects/{project_id}/custom-lists";

            //Create the request
            RestRequest request = new RestRequest("CustomProperty_AddCustomList");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteCustomList == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteCustomList, settings);
				Common.Logger.LogTraceEvent("CustomProperty_AddCustomList", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteCustomList responseObj = JsonConvert.DeserializeObject<RemoteCustomList>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("CustomProperty_AddCustomList", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteCustomList responseObj = JsonConvert.DeserializeObject<RemoteCustomList>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteCustomList[] CustomProperty_RetrieveCustomLists(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/custom-lists";

            //Create the request
            RestRequest request = new RestRequest("CustomProperty_RetrieveCustomLists");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteCustomList[] responseObj = JsonConvert.DeserializeObject<RemoteCustomList[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("CustomProperty_RetrieveCustomLists", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteCustomList[] responseObj = JsonConvert.DeserializeObject<RemoteCustomList[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void CustomProperty_UpdateCustomList(RemoteCredentials credentials, object project_id, object custom_list_id, RemoteCustomList remoteCustomList)
		{
			string path = "projects/{project_id}/custom-lists/{custom_list_id}";

            //Create the request
            RestRequest request = new RestRequest("CustomProperty_UpdateCustomList");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "PUT";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "custom_list_id", custom_list_id);
			

			//Specify the body (if appropriate)
			if (remoteCustomList == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteCustomList, settings);
				Common.Logger.LogTraceEvent("CustomProperty_UpdateCustomList", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteCustomListValue CustomProperty_AddCustomListValue(RemoteCredentials credentials, object project_id, object custom_list_id, RemoteCustomListValue remoteCustomListValue)
		{
			string path = "projects/{project_id}/custom-lists/{custom_list_id}/values";

            //Create the request
            RestRequest request = new RestRequest("CustomProperty_AddCustomListValue");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "custom_list_id", custom_list_id);
			

			//Specify the body (if appropriate)
			if (remoteCustomListValue == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteCustomListValue, settings);
				Common.Logger.LogTraceEvent("CustomProperty_AddCustomListValue", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteCustomListValue responseObj = JsonConvert.DeserializeObject<RemoteCustomListValue>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("CustomProperty_AddCustomListValue", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteCustomListValue responseObj = JsonConvert.DeserializeObject<RemoteCustomListValue>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteCustomProperty[] CustomProperty_RetrieveForArtifactType(RemoteCredentials credentials, object project_id, object artifact_type_name)
		{
			string path = "projects/{project_id}/custom-properties/{artifact_type_name}";

            //Create the request
            RestRequest request = new RestRequest("CustomProperty_RetrieveForArtifactType");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "artifact_type_name", artifact_type_name);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteCustomProperty[] responseObj = JsonConvert.DeserializeObject<RemoteCustomProperty[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("CustomProperty_RetrieveForArtifactType", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteCustomProperty[] responseObj = JsonConvert.DeserializeObject<RemoteCustomProperty[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteCustomProperty CustomProperty_AddDefinition(RemoteCredentials credentials, object project_id, object custom_list_id, RemoteCustomProperty remoteCustomProperty)
		{
			string path = "projects/{project_id}/custom-properties?custom_list_id={custom_list_id}";

            //Create the request
            RestRequest request = new RestRequest("CustomProperty_AddDefinition");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "custom_list_id", custom_list_id);
			

			//Specify the body (if appropriate)
			if (remoteCustomProperty == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteCustomProperty, settings);
				Common.Logger.LogTraceEvent("CustomProperty_AddDefinition", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteCustomProperty responseObj = JsonConvert.DeserializeObject<RemoteCustomProperty>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("CustomProperty_AddDefinition", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteCustomProperty responseObj = JsonConvert.DeserializeObject<RemoteCustomProperty>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void CustomProperty_UpdateDefinition(RemoteCredentials credentials, object project_id, object custom_property_id, RemoteCustomProperty remoteCustomProperty)
		{
			string path = "projects/{project_id}/custom-properties/{custom_property_id}";

            //Create the request
            RestRequest request = new RestRequest("CustomProperty_UpdateDefinition");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "PUT";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "custom_property_id", custom_property_id);
			

			//Specify the body (if appropriate)
			if (remoteCustomProperty == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteCustomProperty, settings);
				Common.Logger.LogTraceEvent("CustomProperty_UpdateDefinition", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void CustomProperty_DeleteDefinition(RemoteCredentials credentials, object project_id, object custom_property_id)
		{
			string path = "projects/{project_id}/custom-properties/{custom_property_id}";

            //Create the request
            RestRequest request = new RestRequest("CustomProperty_DeleteDefinition");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "DELETE";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "custom_property_id", custom_property_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteDataMapping[] DataMapping_RetrieveArtifactMappings(RemoteCredentials credentials, object project_id, object data_sync_system_id, object artifact_type_id)
		{
			string path = "projects/{project_id}/data-mappings/{data_sync_system_id}/artifacts/{artifact_type_id}";

            //Create the request
            RestRequest request = new RestRequest("DataMapping_RetrieveArtifactMappings");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "data_sync_system_id", data_sync_system_id);
			AddRequestParameter(request, "artifact_type_id", artifact_type_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDataMapping[] responseObj = JsonConvert.DeserializeObject<RemoteDataMapping[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("DataMapping_RetrieveArtifactMappings", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDataMapping[] responseObj = JsonConvert.DeserializeObject<RemoteDataMapping[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void DataMapping_AddArtifactMappings(RemoteCredentials credentials, object project_id, object data_sync_system_id, object artifact_type_id, RemoteDataMapping[] remoteDataMappings)
		{
			string path = "projects/{project_id}/data-mappings/{data_sync_system_id}/artifacts/{artifact_type_id}";

            //Create the request
            RestRequest request = new RestRequest("DataMapping_AddArtifactMappings");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "data_sync_system_id", data_sync_system_id);
			AddRequestParameter(request, "artifact_type_id", artifact_type_id);
			

			//Specify the body (if appropriate)
			if (remoteDataMappings == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteDataMappings, settings);
				Common.Logger.LogTraceEvent("DataMapping_AddArtifactMappings", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void DataMapping_RemoveArtifactMappings(RemoteCredentials credentials, object project_id, object data_sync_system_id, object artifact_type_id, RemoteDataMapping[] remoteDataMappings)
		{
			string path = "projects/{project_id}/data-mappings/{data_sync_system_id}/artifacts/{artifact_type_id}";

            //Create the request
            RestRequest request = new RestRequest("DataMapping_RemoveArtifactMappings");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "DELETE";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "data_sync_system_id", data_sync_system_id);
			AddRequestParameter(request, "artifact_type_id", artifact_type_id);
			

			//Specify the body (if appropriate)
			if (remoteDataMappings == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteDataMappings, settings);
				Common.Logger.LogTraceEvent("DataMapping_RemoveArtifactMappings", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteDataMapping DataMapping_RetrieveCustomPropertyMapping(RemoteCredentials credentials, object project_id, object data_sync_system_id, object artifact_type_id, object custom_property_id)
		{
			string path = "projects/{project_id}/data-mappings/{data_sync_system_id}/artifacts/{artifact_type_id}/custom-properties/{custom_property_id}";

            //Create the request
            RestRequest request = new RestRequest("DataMapping_RetrieveCustomPropertyMapping");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "data_sync_system_id", data_sync_system_id);
			AddRequestParameter(request, "artifact_type_id", artifact_type_id);
			AddRequestParameter(request, "custom_property_id", custom_property_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDataMapping responseObj = JsonConvert.DeserializeObject<RemoteDataMapping>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("DataMapping_RetrieveCustomPropertyMapping", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDataMapping responseObj = JsonConvert.DeserializeObject<RemoteDataMapping>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteDataMapping[] DataMapping_RetrieveCustomPropertyValueMappings(RemoteCredentials credentials, object project_id, object data_sync_system_id, object artifact_type_id, object custom_property_id)
		{
			string path = "projects/{project_id}/data-mappings/{data_sync_system_id}/artifacts/{artifact_type_id}/custom-properties/{custom_property_id}/values";

            //Create the request
            RestRequest request = new RestRequest("DataMapping_RetrieveCustomPropertyValueMappings");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "data_sync_system_id", data_sync_system_id);
			AddRequestParameter(request, "artifact_type_id", artifact_type_id);
			AddRequestParameter(request, "custom_property_id", custom_property_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDataMapping[] responseObj = JsonConvert.DeserializeObject<RemoteDataMapping[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("DataMapping_RetrieveCustomPropertyValueMappings", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDataMapping[] responseObj = JsonConvert.DeserializeObject<RemoteDataMapping[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteDataMapping[] DataMapping_RetrieveFieldValueMappings(RemoteCredentials credentials, object project_id, object data_sync_system_id, object artifact_field_id)
		{
			string path = "projects/{project_id}/data-mappings/{data_sync_system_id}/field-values/{artifact_field_id}";

            //Create the request
            RestRequest request = new RestRequest("DataMapping_RetrieveFieldValueMappings");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "data_sync_system_id", data_sync_system_id);
			AddRequestParameter(request, "artifact_field_id", artifact_field_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDataMapping[] responseObj = JsonConvert.DeserializeObject<RemoteDataMapping[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("DataMapping_RetrieveFieldValueMappings", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDataMapping[] responseObj = JsonConvert.DeserializeObject<RemoteDataMapping[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteDataMapping[] DataMapping_RetrieveProjectMappings(RemoteCredentials credentials, object data_sync_system_id)
		{
			string path = "data-mappings/{data_sync_system_id}/projects";

            //Create the request
            RestRequest request = new RestRequest("DataMapping_RetrieveProjectMappings");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "data_sync_system_id", data_sync_system_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDataMapping[] responseObj = JsonConvert.DeserializeObject<RemoteDataMapping[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("DataMapping_RetrieveProjectMappings", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDataMapping[] responseObj = JsonConvert.DeserializeObject<RemoteDataMapping[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void DataMapping_AddUserMappings(RemoteCredentials credentials, object data_sync_system_id, RemoteDataMapping[] remoteDataMappings)
		{
			string path = "data-mappings/{data_sync_system_id}/users";

            //Create the request
            RestRequest request = new RestRequest("DataMapping_AddUserMappings");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "data_sync_system_id", data_sync_system_id);
			

			//Specify the body (if appropriate)
			if (remoteDataMappings == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteDataMappings, settings);
				Common.Logger.LogTraceEvent("DataMapping_AddUserMappings", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteDataMapping[] DataMapping_RetrieveUserMappings(RemoteCredentials credentials, object data_sync_system_id)
		{
			string path = "data-mappings/{data_sync_system_id}/users";

            //Create the request
            RestRequest request = new RestRequest("DataMapping_RetrieveUserMappings");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "data_sync_system_id", data_sync_system_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDataMapping[] responseObj = JsonConvert.DeserializeObject<RemoteDataMapping[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("DataMapping_RetrieveUserMappings", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDataMapping[] responseObj = JsonConvert.DeserializeObject<RemoteDataMapping[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteDataSyncSystem[] DataSyncSystem_Retrieve(RemoteCredentials credentials)
		{
			string path = "data-syncs";

            //Create the request
            RestRequest request = new RestRequest("DataSyncSystem_Retrieve");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDataSyncSystem[] responseObj = JsonConvert.DeserializeObject<RemoteDataSyncSystem[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("DataSyncSystem_Retrieve", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDataSyncSystem[] responseObj = JsonConvert.DeserializeObject<RemoteDataSyncSystem[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void DataSyncSystem_SaveRunFailure(RemoteCredentials credentials, object data_sync_system_id)
		{
			string path = "data-syncs/{data_sync_system_id}/failure";

            //Create the request
            RestRequest request = new RestRequest("DataSyncSystem_SaveRunFailure");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "data_sync_system_id", data_sync_system_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void DataSyncSystem_SaveRunSuccess(RemoteCredentials credentials, object data_sync_system_id, DateTime lastRunDate)
		{
			string path = "data-syncs/{data_sync_system_id}/success";

            //Create the request
            RestRequest request = new RestRequest("DataSyncSystem_SaveRunSuccess");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "data_sync_system_id", data_sync_system_id);
			

			//Specify the body (if appropriate)
			if (lastRunDate == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(lastRunDate, settings);
				Common.Logger.LogTraceEvent("DataSyncSystem_SaveRunSuccess", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void DataSyncSystem_SaveRunWarning(RemoteCredentials credentials, object data_sync_system_id, DateTime lastRunDate)
		{
			string path = "data-syncs/{data_sync_system_id}/warning";

            //Create the request
            RestRequest request = new RestRequest("DataSyncSystem_SaveRunWarning");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "data_sync_system_id", data_sync_system_id);
			

			//Specify the body (if appropriate)
			if (lastRunDate == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(lastRunDate, settings);
				Common.Logger.LogTraceEvent("DataSyncSystem_SaveRunWarning", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void DataSyncSystem_WriteEvent(RemoteCredentials credentials, RemoteEvent remoteEvent)
		{
			string path = "data-syncs/events";

            //Create the request
            RestRequest request = new RestRequest("DataSyncSystem_WriteEvent");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			

			//Specify the body (if appropriate)
			if (remoteEvent == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteEvent, settings);
				Common.Logger.LogTraceEvent("DataSyncSystem_WriteEvent", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public Byte[] Document_OpenFile(RemoteCredentials credentials, object project_id, object document_id)
		{
			string path = "projects/{project_id}/documents/{document_id}/open";

            //Create the request
            RestRequest request = new RestRequest("Document_OpenFile");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "document_id", document_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Byte[] responseObj = JsonConvert.DeserializeObject<Byte[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_OpenFile", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Byte[] responseObj = JsonConvert.DeserializeObject<Byte[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteDocument Document_AddFile(RemoteCredentials credentials, object project_id, object filename, object tags, object folder_id, object document_type_id, object artifact_type_id, object artifact_id, Byte[] binaryData)
		{
			string path = "projects/{project_id}/documents/file?filename={filename}&tags={tags}&folder_id={folder_id}&document_type_id={document_type_id}&artifact_type_id={artifact_type_id}&artifact_id={artifact_id}";

            //Create the request
            RestRequest request = new RestRequest("Document_AddFile");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "filename", filename);
			AddRequestParameter(request, "tags", tags);
			AddRequestParameter(request, "folder_id", folder_id);
			AddRequestParameter(request, "document_type_id", document_type_id);
			AddRequestParameter(request, "artifact_type_id", artifact_type_id);
			AddRequestParameter(request, "artifact_id", artifact_id);
			

			//Specify the body (if appropriate)
			if (binaryData == null)
			{
				request.Body = "";
			}
			else
			{
				string json = "[";
				bool first = true;
				foreach (byte item in (byte[])binaryData)
				{
					if (!first)
					{
						json += ",";
					}
					else
					{
						first = false;
					}
					json += item.ToString();
				}
				json += "]";
				request.Body = json;
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDocument responseObj = JsonConvert.DeserializeObject<RemoteDocument>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_AddFile", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDocument responseObj = JsonConvert.DeserializeObject<RemoteDocument>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteDocument Document_AddUrl(RemoteCredentials credentials, object project_id, object url, object tags, object folder_id, object document_type_id, object artifact_type_id, object artifact_id)
		{
			string path = "projects/{project_id}/documents/url?url={url}&tags={tags}&folder_id={folder_id}&document_type_id={document_type_id}&artifact_type_id={artifact_type_id}&artifact_id={artifact_id}";

            //Create the request
            RestRequest request = new RestRequest("Document_AddUrl");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "url", url);
			AddRequestParameter(request, "tags", tags);
			AddRequestParameter(request, "folder_id", folder_id);
			AddRequestParameter(request, "document_type_id", document_type_id);
			AddRequestParameter(request, "artifact_type_id", artifact_type_id);
			AddRequestParameter(request, "artifact_id", artifact_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDocument responseObj = JsonConvert.DeserializeObject<RemoteDocument>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_AddUrl", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDocument responseObj = JsonConvert.DeserializeObject<RemoteDocument>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void Document_Delete(RemoteCredentials credentials, object project_id, object document_id)
		{
			string path = "projects/{project_id}/documents/{document_id}";

            //Create the request
            RestRequest request = new RestRequest("Document_Delete");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "DELETE";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "document_id", document_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteDocument[] Document_Retrieve1(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/documents";

            //Create the request
            RestRequest request = new RestRequest("Document_Retrieve1");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDocument[] responseObj = JsonConvert.DeserializeObject<RemoteDocument[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_Retrieve1", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDocument[] responseObj = JsonConvert.DeserializeObject<RemoteDocument[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteDocument[] Document_Retrieve2(RemoteCredentials credentials, object project_id, object start_row, object number_rows, object sort_by, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/documents/search?start_row={start_row}&number_rows={number_rows}&sort_by={sort_by}";

            //Create the request
            RestRequest request = new RestRequest("Document_Retrieve2");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "start_row", start_row);
			AddRequestParameter(request, "number_rows", number_rows);
			AddRequestParameter(request, "sort_by", sort_by);
			

			//Specify the body (if appropriate)
			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteFilters, settings);
				Common.Logger.LogTraceEvent("Document_Retrieve2", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDocument[] responseObj = JsonConvert.DeserializeObject<RemoteDocument[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_Retrieve2", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDocument[] responseObj = JsonConvert.DeserializeObject<RemoteDocument[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteDocument[] Document_RetrieveForFolder(RemoteCredentials credentials, object project_id, object folder_id, object start_row, object number_rows, object sort_by, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/document-folders/{folder_id}/documents/search?start_row={start_row}&number_rows={number_rows}&sort_by={sort_by}";

            //Create the request
            RestRequest request = new RestRequest("Document_RetrieveForFolder");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "folder_id", folder_id);
			AddRequestParameter(request, "start_row", start_row);
			AddRequestParameter(request, "number_rows", number_rows);
			AddRequestParameter(request, "sort_by", sort_by);
			

			//Specify the body (if appropriate)
			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteFilters, settings);
				Common.Logger.LogTraceEvent("Document_RetrieveForFolder", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDocument[] responseObj = JsonConvert.DeserializeObject<RemoteDocument[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_RetrieveForFolder", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDocument[] responseObj = JsonConvert.DeserializeObject<RemoteDocument[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteDocument Document_RetrieveById(RemoteCredentials credentials, object project_id, object document_id)
		{
			string path = "projects/{project_id}/documents/{document_id}";

            //Create the request
            RestRequest request = new RestRequest("Document_RetrieveById");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "document_id", document_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDocument responseObj = JsonConvert.DeserializeObject<RemoteDocument>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_RetrieveById", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDocument responseObj = JsonConvert.DeserializeObject<RemoteDocument>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteDocumentFolder[] Document_RetrieveFolders(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/document-folders";

            //Create the request
            RestRequest request = new RestRequest("Document_RetrieveFolders");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDocumentFolder[] responseObj = JsonConvert.DeserializeObject<RemoteDocumentFolder[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_RetrieveFolders", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDocumentFolder[] responseObj = JsonConvert.DeserializeObject<RemoteDocumentFolder[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteDocumentFolder Document_RetrieveFolderById(RemoteCredentials credentials, object project_id, object folder_id)
		{
			string path = "projects/{project_id}/document-folders/{folder_id}";

            //Create the request
            RestRequest request = new RestRequest("Document_RetrieveFolderById");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "folder_id", folder_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDocumentFolder responseObj = JsonConvert.DeserializeObject<RemoteDocumentFolder>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_RetrieveFolderById", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDocumentFolder responseObj = JsonConvert.DeserializeObject<RemoteDocumentFolder>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteDocumentFolder Document_AddFolder(RemoteCredentials credentials, object project_id, RemoteDocumentFolder remoteDocumentFolder)
		{
			string path = "projects/{project_id}/document-folders";

            //Create the request
            RestRequest request = new RestRequest("Document_AddFolder");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteDocumentFolder == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteDocumentFolder, settings);
				Common.Logger.LogTraceEvent("Document_AddFolder", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDocumentFolder responseObj = JsonConvert.DeserializeObject<RemoteDocumentFolder>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_AddFolder", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDocumentFolder responseObj = JsonConvert.DeserializeObject<RemoteDocumentFolder>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void Document_DeleteFolder(RemoteCredentials credentials, object project_id, object folder_id)
		{
			string path = "projects/{project_id}/document-folders/{folder_id}";

            //Create the request
            RestRequest request = new RestRequest("Document_DeleteFolder");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "DELETE";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "folder_id", folder_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void Document_UpdateFolder(RemoteCredentials credentials, object project_id, object folder_id, RemoteDocumentFolder remoteDocumentFolder)
		{
			string path = "projects/{project_id}/document-folders/{folder_id}";

            //Create the request
            RestRequest request = new RestRequest("Document_UpdateFolder");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "PUT";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "folder_id", folder_id);
			

			//Specify the body (if appropriate)
			if (remoteDocumentFolder == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteDocumentFolder, settings);
				Common.Logger.LogTraceEvent("Document_UpdateFolder", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteDocumentType[] Document_RetrieveTypes(RemoteCredentials credentials, object project_id, object active_only)
		{
			string path = "projects/{project_id}/document-types?active_only={active_only}";

            //Create the request
            RestRequest request = new RestRequest("Document_RetrieveTypes");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "active_only", active_only);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDocumentType[] responseObj = JsonConvert.DeserializeObject<RemoteDocumentType[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_RetrieveTypes", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDocumentType[] responseObj = JsonConvert.DeserializeObject<RemoteDocumentType[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public Int32 Document_AddFileVersion(RemoteCredentials credentials, object project_id, object document_id, object filename, object version, object make_current, Byte[] binaryData)
		{
			string path = "projects/{project_id}/documents/{document_id}/versions/file?filename={filename}&version={version}&make_current={make_current}";

            //Create the request
            RestRequest request = new RestRequest("Document_AddFileVersion");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "document_id", document_id);
			AddRequestParameter(request, "filename", filename);
			AddRequestParameter(request, "version", version);
			AddRequestParameter(request, "make_current", make_current);
			

			//Specify the body (if appropriate)
			if (binaryData == null)
			{
				request.Body = "";
			}
			else
			{
				string json = "[";
				bool first = true;
				foreach (byte item in (byte[])binaryData)
				{
					if (!first)
					{
						json += ",";
					}
					else
					{
						first = false;
					}
					json += item.ToString();
				}
				json += "]";
				request.Body = json;
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int32 responseObj = JsonConvert.DeserializeObject<Int32>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_AddFileVersion", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int32 responseObj = JsonConvert.DeserializeObject<Int32>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public Int32 Document_AddUrlVersion(RemoteCredentials credentials, object project_id, object document_id, object url, object version, object make_current)
		{
			string path = "projects/{project_id}/documents/{document_id}/versions/url?url={url}&version={version}&make_current={make_current}";

            //Create the request
            RestRequest request = new RestRequest("Document_AddUrlVersion");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "document_id", document_id);
			AddRequestParameter(request, "url", url);
			AddRequestParameter(request, "version", version);
			AddRequestParameter(request, "make_current", make_current);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int32 responseObj = JsonConvert.DeserializeObject<Int32>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_AddUrlVersion", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int32 responseObj = JsonConvert.DeserializeObject<Int32>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteIncident Incident_RetrieveById(RemoteCredentials credentials, object project_id, object incident_id)
		{
			string path = "projects/{project_id}/incidents/{incident_id}";

            //Create the request
            RestRequest request = new RestRequest("Incident_RetrieveById");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "incident_id", incident_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncident responseObj = JsonConvert.DeserializeObject<RemoteIncident>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveById", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncident responseObj = JsonConvert.DeserializeObject<RemoteIncident>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteIncident[] Incident_RetrieveByIdList(RemoteCredentials credentials, object project_id, object incident_ids)
		{
			string path = "projects/{project_id}/incidents/search-by-ids?ids={incident_ids}";

            //Create the request
            RestRequest request = new RestRequest("Incident_RetrieveByIdList");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "incident_ids", incident_ids);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncident[] responseObj = JsonConvert.DeserializeObject<RemoteIncident[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveByIdList", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncident[] responseObj = JsonConvert.DeserializeObject<RemoteIncident[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public Int64 Incident_Count(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/incidents/count";

            //Create the request
            RestRequest request = new RestRequest("Incident_Count");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_Count", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteIncident[] Incident_Retrieve1(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/incidents";

            //Create the request
            RestRequest request = new RestRequest("Incident_Retrieve1");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncident[] responseObj = JsonConvert.DeserializeObject<RemoteIncident[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_Retrieve1", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncident[] responseObj = JsonConvert.DeserializeObject<RemoteIncident[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteIncident[] Incident_Retrieve2(RemoteCredentials credentials, object project_id, object start_row, object number_rows, object sort_by)
		{
			string path = "projects/{project_id}/incidents/search?start_row={start_row}&number_rows={number_rows}&sort_by={sort_by}";

            //Create the request
            RestRequest request = new RestRequest("Incident_Retrieve2");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "start_row", start_row);
			AddRequestParameter(request, "number_rows", number_rows);
			AddRequestParameter(request, "sort_by", sort_by);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncident[] responseObj = JsonConvert.DeserializeObject<RemoteIncident[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_Retrieve2", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncident[] responseObj = JsonConvert.DeserializeObject<RemoteIncident[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteIncident[] Incident_RetrieveNew(RemoteCredentials credentials, object project_id, object start_row, object number_rows, object creation_date)
		{
			string path = "projects/{project_id}/incidents/recent?start_row={start_row}&number_rows={number_rows}&creation_date={creation_date}";

            //Create the request
            RestRequest request = new RestRequest("Incident_RetrieveNew");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "start_row", start_row);
			AddRequestParameter(request, "number_rows", number_rows);
			AddRequestParameter(request, "creation_date", creation_date);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncident[] responseObj = JsonConvert.DeserializeObject<RemoteIncident[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveNew", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncident[] responseObj = JsonConvert.DeserializeObject<RemoteIncident[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteIncident[] Incident_Retrieve3(RemoteCredentials credentials, object project_id, object start_row, object number_rows, object sort_by, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/incidents/search?start_row={start_row}&number_rows={number_rows}&sort_by={sort_by}";

            //Create the request
            RestRequest request = new RestRequest("Incident_Retrieve3");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "start_row", start_row);
			AddRequestParameter(request, "number_rows", number_rows);
			AddRequestParameter(request, "sort_by", sort_by);
			

			//Specify the body (if appropriate)
			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteFilters, settings);
				Common.Logger.LogTraceEvent("Incident_Retrieve3", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncident[] responseObj = JsonConvert.DeserializeObject<RemoteIncident[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_Retrieve3", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncident[] responseObj = JsonConvert.DeserializeObject<RemoteIncident[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteIncident Incident_Create(RemoteCredentials credentials, object project_id, RemoteIncident remoteIncident)
		{
			string path = "projects/{project_id}/incidents";

            //Create the request
            RestRequest request = new RestRequest("Incident_Create");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteIncident == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteIncident, settings);
				Common.Logger.LogTraceEvent("Incident_Create", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncident responseObj = JsonConvert.DeserializeObject<RemoteIncident>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_Create", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncident responseObj = JsonConvert.DeserializeObject<RemoteIncident>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void Incident_Update(RemoteCredentials credentials, object project_id, object incident_id, RemoteIncident remoteIncident)
		{
			string path = "projects/{project_id}/incidents/{incident_id}";

            //Create the request
            RestRequest request = new RestRequest("Incident_Update");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "PUT";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "incident_id", incident_id);
			

			//Specify the body (if appropriate)
			if (remoteIncident == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteIncident, settings);
				Common.Logger.LogTraceEvent("Incident_Update", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void Incident_Delete(RemoteCredentials credentials, object project_id, object incident_id)
		{
			string path = "projects/{project_id}/incidents/{incident_id}";

            //Create the request
            RestRequest request = new RestRequest("Incident_Delete");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "DELETE";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "incident_id", incident_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteComment[] Incident_RetrieveComments(RemoteCredentials credentials, object project_id, object incident_id)
		{
			string path = "projects/{project_id}/incidents/{incident_id}/comments";

            //Create the request
            RestRequest request = new RestRequest("Incident_RetrieveComments");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "incident_id", incident_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteComment[] responseObj = JsonConvert.DeserializeObject<RemoteComment[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveComments", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteComment[] responseObj = JsonConvert.DeserializeObject<RemoteComment[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteComment[] Incident_AddComments(RemoteCredentials credentials, object project_id, object incident_id, RemoteComment[] remoteComments)
		{
			string path = "projects/{project_id}/incidents/{incident_id}/comments";

            //Create the request
            RestRequest request = new RestRequest("Incident_AddComments");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "incident_id", incident_id);
			

			//Specify the body (if appropriate)
			if (remoteComments == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteComments, settings);
				Common.Logger.LogTraceEvent("Incident_AddComments", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteComment[] responseObj = JsonConvert.DeserializeObject<RemoteComment[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_AddComments", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteComment[] responseObj = JsonConvert.DeserializeObject<RemoteComment[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteIncidentPriority[] Incident_RetrievePriorities(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/incidents/priorities";

            //Create the request
            RestRequest request = new RestRequest("Incident_RetrievePriorities");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncidentPriority[] responseObj = JsonConvert.DeserializeObject<RemoteIncidentPriority[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrievePriorities", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncidentPriority[] responseObj = JsonConvert.DeserializeObject<RemoteIncidentPriority[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteIncidentPriority Incident_AddPriority(RemoteCredentials credentials, object project_id, RemoteIncidentPriority remoteIncidentPriority)
		{
			string path = "projects/{project_id}/incidents/priorities";

            //Create the request
            RestRequest request = new RestRequest("Incident_AddPriority");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteIncidentPriority == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteIncidentPriority, settings);
				Common.Logger.LogTraceEvent("Incident_AddPriority", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncidentPriority responseObj = JsonConvert.DeserializeObject<RemoteIncidentPriority>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_AddPriority", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncidentPriority responseObj = JsonConvert.DeserializeObject<RemoteIncidentPriority>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteIncidentSeverity[] Incident_RetrieveSeverities(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/incidents/severities";

            //Create the request
            RestRequest request = new RestRequest("Incident_RetrieveSeverities");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncidentSeverity[] responseObj = JsonConvert.DeserializeObject<RemoteIncidentSeverity[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveSeverities", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncidentSeverity[] responseObj = JsonConvert.DeserializeObject<RemoteIncidentSeverity[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteIncidentSeverity Incident_AddSeverity(RemoteCredentials credentials, object project_id, RemoteIncidentSeverity remoteIncidentSeverity)
		{
			string path = "projects/{project_id}/incidents/severities";

            //Create the request
            RestRequest request = new RestRequest("Incident_AddSeverity");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteIncidentSeverity == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteIncidentSeverity, settings);
				Common.Logger.LogTraceEvent("Incident_AddSeverity", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncidentSeverity responseObj = JsonConvert.DeserializeObject<RemoteIncidentSeverity>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_AddSeverity", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncidentSeverity responseObj = JsonConvert.DeserializeObject<RemoteIncidentSeverity>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteIncidentStatus[] Incident_RetrieveStatuses(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/incidents/statuses";

            //Create the request
            RestRequest request = new RestRequest("Incident_RetrieveStatuses");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncidentStatus[] responseObj = JsonConvert.DeserializeObject<RemoteIncidentStatus[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveStatuses", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncidentStatus[] responseObj = JsonConvert.DeserializeObject<RemoteIncidentStatus[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteIncidentStatus Incident_AddStatus(RemoteCredentials credentials, object project_id, RemoteIncidentStatus remoteIncidentStatus)
		{
			string path = "projects/{project_id}/incidents/statuses";

            //Create the request
            RestRequest request = new RestRequest("Incident_AddStatus");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteIncidentStatus == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteIncidentStatus, settings);
				Common.Logger.LogTraceEvent("Incident_AddStatus", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncidentStatus responseObj = JsonConvert.DeserializeObject<RemoteIncidentStatus>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_AddStatus", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncidentStatus responseObj = JsonConvert.DeserializeObject<RemoteIncidentStatus>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteIncidentStatus Incident_RetrieveDefaultStatus(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/incidents/statuses/default";

            //Create the request
            RestRequest request = new RestRequest("Incident_RetrieveDefaultStatus");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncidentStatus responseObj = JsonConvert.DeserializeObject<RemoteIncidentStatus>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveDefaultStatus", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncidentStatus responseObj = JsonConvert.DeserializeObject<RemoteIncidentStatus>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteIncidentType Incident_AddType(RemoteCredentials credentials, object project_id, RemoteIncidentType remoteIncidentType)
		{
			string path = "projects/{project_id}/incidents/types";

            //Create the request
            RestRequest request = new RestRequest("Incident_AddType");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteIncidentType == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteIncidentType, settings);
				Common.Logger.LogTraceEvent("Incident_AddType", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncidentType responseObj = JsonConvert.DeserializeObject<RemoteIncidentType>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_AddType", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncidentType responseObj = JsonConvert.DeserializeObject<RemoteIncidentType>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteIncidentType[] Incident_RetrieveTypes(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/incidents/types";

            //Create the request
            RestRequest request = new RestRequest("Incident_RetrieveTypes");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncidentType[] responseObj = JsonConvert.DeserializeObject<RemoteIncidentType[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveTypes", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncidentType[] responseObj = JsonConvert.DeserializeObject<RemoteIncidentType[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteIncidentType Incident_RetrieveDefaultType(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/incidents/types/default";

            //Create the request
            RestRequest request = new RestRequest("Incident_RetrieveDefaultType");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncidentType responseObj = JsonConvert.DeserializeObject<RemoteIncidentType>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveDefaultType", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteIncidentType responseObj = JsonConvert.DeserializeObject<RemoteIncidentType>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteWorkflowIncidentTransition[] Incident_RetrieveWorkflowTransitions(RemoteCredentials credentials, object project_id, object incident_type_id, object incident_status_id, object is_detector, object is_owner)
		{
			string path = "projects/{project_id}/incidents/types/{incident_type_id}/workflow/transitions?status_id={incident_status_id}&is_detector={is_detector}&isOwner={is_owner}";

            //Create the request
            RestRequest request = new RestRequest("Incident_RetrieveWorkflowTransitions");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "incident_type_id", incident_type_id);
			AddRequestParameter(request, "incident_status_id", incident_status_id);
			AddRequestParameter(request, "is_detector", is_detector);
			AddRequestParameter(request, "is_owner", is_owner);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteWorkflowIncidentTransition[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowIncidentTransition[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveWorkflowTransitions", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteWorkflowIncidentTransition[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowIncidentTransition[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteWorkflowIncidentFields[] Incident_RetrieveWorkflowFields(RemoteCredentials credentials, object project_id, object incident_type_id, object incident_status_id)
		{
			string path = "projects/{project_id}/incidents/types/{incident_type_id}/workflow/fields?status_id={incident_status_id}";

            //Create the request
            RestRequest request = new RestRequest("Incident_RetrieveWorkflowFields");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "incident_type_id", incident_type_id);
			AddRequestParameter(request, "incident_status_id", incident_status_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteWorkflowIncidentFields[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowIncidentFields[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveWorkflowFields", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteWorkflowIncidentFields[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowIncidentFields[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteWorkflowIncidentCustomProperties[] Incident_RetrieveWorkflowCustomProperties(RemoteCredentials credentials, object project_id, object incident_type_id, object incident_status_id)
		{
			string path = "projects/{project_id}/incidents/types/{incident_type_id}/workflow/custom-properties?status_id={incident_status_id}";

            //Create the request
            RestRequest request = new RestRequest("Incident_RetrieveWorkflowCustomProperties");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "incident_type_id", incident_type_id);
			AddRequestParameter(request, "incident_status_id", incident_status_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteWorkflowIncidentCustomProperties[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowIncidentCustomProperties[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveWorkflowCustomProperties", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteWorkflowIncidentCustomProperties[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowIncidentCustomProperties[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteProject[] Project_Retrieve(RemoteCredentials credentials)
		{
			string path = "projects";

            //Create the request
            RestRequest request = new RestRequest("Project_Retrieve");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteProject[] responseObj = JsonConvert.DeserializeObject<RemoteProject[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Project_Retrieve", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteProject[] responseObj = JsonConvert.DeserializeObject<RemoteProject[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteProject Project_RetrieveById(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}";

            //Create the request
            RestRequest request = new RestRequest("Project_RetrieveById");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteProject responseObj = JsonConvert.DeserializeObject<RemoteProject>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Project_RetrieveById", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteProject responseObj = JsonConvert.DeserializeObject<RemoteProject>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteProject Project_Create(RemoteCredentials credentials, object existing_project_id, RemoteProject remoteProject)
		{
			string path = "projects?existing_project_id={existing_project_id}";

            //Create the request
            RestRequest request = new RestRequest("Project_Create");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "existing_project_id", existing_project_id);
			

			//Specify the body (if appropriate)
			if (remoteProject == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteProject, settings);
				Common.Logger.LogTraceEvent("Project_Create", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteProject responseObj = JsonConvert.DeserializeObject<RemoteProject>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Project_Create", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteProject responseObj = JsonConvert.DeserializeObject<RemoteProject>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void Project_Delete(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}";

            //Create the request
            RestRequest request = new RestRequest("Project_Delete");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "DELETE";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void Project_RefreshProgressExecutionStatusCaches(RemoteCredentials credentials, object project_id, object release_id, object run_async)
		{
			string path = "projects/{project_id}/refresh-caches/{release_id}?run_async={run_async}";

            //Create the request
            RestRequest request = new RestRequest("Project_RefreshProgressExecutionStatusCaches");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "release_id", release_id);
			AddRequestParameter(request, "run_async", run_async);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteProjectRole[] ProjectRole_Retrieve(RemoteCredentials credentials)
		{
			string path = "projects-roles";

            //Create the request
            RestRequest request = new RestRequest("ProjectRole_Retrieve");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteProjectRole[] responseObj = JsonConvert.DeserializeObject<RemoteProjectRole[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("ProjectRole_Retrieve", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteProjectRole[] responseObj = JsonConvert.DeserializeObject<RemoteProjectRole[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteProjectUser[] Project_RetrieveUserMembership(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/users";

            //Create the request
            RestRequest request = new RestRequest("Project_RetrieveUserMembership");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteProjectUser[] responseObj = JsonConvert.DeserializeObject<RemoteProjectUser[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Project_RetrieveUserMembership", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteProjectUser[] responseObj = JsonConvert.DeserializeObject<RemoteProjectUser[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteRelease[] Release_Retrieve1(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/releases";

            //Create the request
            RestRequest request = new RestRequest("Release_Retrieve1");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRelease[] responseObj = JsonConvert.DeserializeObject<RemoteRelease[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Release_Retrieve1", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRelease[] responseObj = JsonConvert.DeserializeObject<RemoteRelease[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteRelease[] Release_Retrieve2(RemoteCredentials credentials, object project_id, object start_row, object number_rows, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/releases/search?start_row={start_row}&number_rows={number_rows}";

            //Create the request
            RestRequest request = new RestRequest("Release_Retrieve2");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "start_row", start_row);
			AddRequestParameter(request, "number_rows", number_rows);
			

			//Specify the body (if appropriate)
			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteFilters, settings);
				Common.Logger.LogTraceEvent("Release_Retrieve2", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRelease[] responseObj = JsonConvert.DeserializeObject<RemoteRelease[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Release_Retrieve2", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRelease[] responseObj = JsonConvert.DeserializeObject<RemoteRelease[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteRelease Release_RetrieveById(RemoteCredentials credentials, object project_id, object release_id)
		{
			string path = "projects/{project_id}/releases/{release_id}";

            //Create the request
            RestRequest request = new RestRequest("Release_RetrieveById");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "release_id", release_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRelease responseObj = JsonConvert.DeserializeObject<RemoteRelease>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Release_RetrieveById", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRelease responseObj = JsonConvert.DeserializeObject<RemoteRelease>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public Int64 Release_Count(RemoteCredentials credentials, object project_id, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/releases/count";

            //Create the request
            RestRequest request = new RestRequest("Release_Count");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteFilters, settings);
				Common.Logger.LogTraceEvent("Release_Count", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Release_Count", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteRelease Release_Create1(RemoteCredentials credentials, object project_id, RemoteRelease remoteRelease)
		{
			string path = "projects/{project_id}/releases";

            //Create the request
            RestRequest request = new RestRequest("Release_Create1");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteRelease == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteRelease, settings);
				Common.Logger.LogTraceEvent("Release_Create1", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRelease responseObj = JsonConvert.DeserializeObject<RemoteRelease>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Release_Create1", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRelease responseObj = JsonConvert.DeserializeObject<RemoteRelease>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteRelease Release_Create2(RemoteCredentials credentials, object project_id, object parent_release_id, RemoteRelease remoteRelease)
		{
			string path = "projects/{project_id}/releases/{parent_release_id}";

            //Create the request
            RestRequest request = new RestRequest("Release_Create2");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "parent_release_id", parent_release_id);
			

			//Specify the body (if appropriate)
			if (remoteRelease == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteRelease, settings);
				Common.Logger.LogTraceEvent("Release_Create2", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRelease responseObj = JsonConvert.DeserializeObject<RemoteRelease>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Release_Create2", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRelease responseObj = JsonConvert.DeserializeObject<RemoteRelease>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void Release_Update(RemoteCredentials credentials, object project_id, RemoteRelease remoteRelease)
		{
			string path = "projects/{project_id}/releases";

            //Create the request
            RestRequest request = new RestRequest("Release_Update");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "PUT";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteRelease == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteRelease, settings);
				Common.Logger.LogTraceEvent("Release_Update", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void Release_Delete(RemoteCredentials credentials, object project_id, object release_id)
		{
			string path = "projects/{project_id}/releases/{release_id}";

            //Create the request
            RestRequest request = new RestRequest("Release_Delete");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "DELETE";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "release_id", release_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void Release_Move(RemoteCredentials credentials, object project_id, object release_id, object destination_release_id)
		{
			string path = "projects/{project_id}/releases/{release_id}/move?destination_release_id={destination_release_id}";

            //Create the request
            RestRequest request = new RestRequest("Release_Move");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "release_id", release_id);
			AddRequestParameter(request, "destination_release_id", destination_release_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteBuild Build_RetrieveById(RemoteCredentials credentials, object project_id, object release_id, object build_id)
		{
			string path = "projects/{project_id}/releases/{release_id}/builds/{build_id}";

            //Create the request
            RestRequest request = new RestRequest("Build_RetrieveById");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "release_id", release_id);
			AddRequestParameter(request, "build_id", build_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteBuild responseObj = JsonConvert.DeserializeObject<RemoteBuild>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Build_RetrieveById", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteBuild responseObj = JsonConvert.DeserializeObject<RemoteBuild>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteBuild[] Build_RetrieveByReleaseId(RemoteCredentials credentials, object project_id, object release_id)
		{
			string path = "projects/{project_id}/releases/{release_id}/builds";

            //Create the request
            RestRequest request = new RestRequest("Build_RetrieveByReleaseId");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "release_id", release_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteBuild[] responseObj = JsonConvert.DeserializeObject<RemoteBuild[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Build_RetrieveByReleaseId", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteBuild[] responseObj = JsonConvert.DeserializeObject<RemoteBuild[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteBuild Build_Create(RemoteCredentials credentials, object project_id, object release_id, RemoteBuild remoteBuild)
		{
			string path = "projects/{project_id}/releases/{release_id}/builds";

            //Create the request
            RestRequest request = new RestRequest("Build_Create");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "release_id", release_id);
			

			//Specify the body (if appropriate)
			if (remoteBuild == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteBuild, settings);
				Common.Logger.LogTraceEvent("Build_Create", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteBuild responseObj = JsonConvert.DeserializeObject<RemoteBuild>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Build_Create", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteBuild responseObj = JsonConvert.DeserializeObject<RemoteBuild>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteComment[] Release_RetrieveComments(RemoteCredentials credentials, object project_id, object release_id)
		{
			string path = "projects/{project_id}/releases/{release_id}/comments";

            //Create the request
            RestRequest request = new RestRequest("Release_RetrieveComments");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "release_id", release_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteComment[] responseObj = JsonConvert.DeserializeObject<RemoteComment[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Release_RetrieveComments", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteComment[] responseObj = JsonConvert.DeserializeObject<RemoteComment[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteComment Release_CreateComment(RemoteCredentials credentials, object project_id, object release_id, RemoteComment remoteComment)
		{
			string path = "projects/{project_id}/releases/{release_id}/comments";

            //Create the request
            RestRequest request = new RestRequest("Release_CreateComment");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "release_id", release_id);
			

			//Specify the body (if appropriate)
			if (remoteComment == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteComment, settings);
				Common.Logger.LogTraceEvent("Release_CreateComment", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteComment responseObj = JsonConvert.DeserializeObject<RemoteComment>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Release_CreateComment", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteComment responseObj = JsonConvert.DeserializeObject<RemoteComment>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void Release_AddTestMapping(RemoteCredentials credentials, object project_id, object release_id, Int32[] testCaseIds)
		{
			string path = "projects/{project_id}/releases/{release_id}/test-cases";

            //Create the request
            RestRequest request = new RestRequest("Release_AddTestMapping");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "release_id", release_id);
			

			//Specify the body (if appropriate)
			if (testCaseIds == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(testCaseIds, settings);
				Common.Logger.LogTraceEvent("Release_AddTestMapping", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void Release_RemoveTestMapping(RemoteCredentials credentials, object project_id, object release_id, object test_case_id)
		{
			string path = "projects/{project_id}/releases/{release_id}/test-cases/{test_case_id}";

            //Create the request
            RestRequest request = new RestRequest("Release_RemoveTestMapping");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "DELETE";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "release_id", release_id);
			AddRequestParameter(request, "test_case_id", test_case_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteReleaseTestCaseMapping[] Release_RetrieveTestMapping(RemoteCredentials credentials, object project_id, object release_id)
		{
			string path = "projects/{project_id}/releases/{release_id}/test-cases";

            //Create the request
            RestRequest request = new RestRequest("Release_RetrieveTestMapping");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "release_id", release_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteReleaseTestCaseMapping[] responseObj = JsonConvert.DeserializeObject<RemoteReleaseTestCaseMapping[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Release_RetrieveTestMapping", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteReleaseTestCaseMapping[] responseObj = JsonConvert.DeserializeObject<RemoteReleaseTestCaseMapping[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTestCase[] TestCase_RetrieveByReleaseId(RemoteCredentials credentials, object project_id, object release_id, object starting_row, object number_of_rows, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/releases/{release_id}/test-cases/search?starting_row={starting_row}&number_of_rows={number_of_rows}";

            //Create the request
            RestRequest request = new RestRequest("TestCase_RetrieveByReleaseId");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "release_id", release_id);
			AddRequestParameter(request, "starting_row", starting_row);
			AddRequestParameter(request, "number_of_rows", number_of_rows);
			

			//Specify the body (if appropriate)
			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteFilters, settings);
				Common.Logger.LogTraceEvent("TestCase_RetrieveByReleaseId", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestCase[] responseObj = JsonConvert.DeserializeObject<RemoteTestCase[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveByReleaseId", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestCase[] responseObj = JsonConvert.DeserializeObject<RemoteTestCase[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public Int64 Requirement_Count1(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/requirements/count";

            //Create the request
            RestRequest request = new RestRequest("Requirement_Count1");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_Count1", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public Int64 Requirement_Count2(RemoteCredentials credentials, object project_id, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/requirements/count";

            //Create the request
            RestRequest request = new RestRequest("Requirement_Count2");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteFilters, settings);
				Common.Logger.LogTraceEvent("Requirement_Count2", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_Count2", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteRequirement Requirement_Create1(RemoteCredentials credentials, object project_id, RemoteRequirement remoteRequirement)
		{
			string path = "projects/{project_id}/requirements";

            //Create the request
            RestRequest request = new RestRequest("Requirement_Create1");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteRequirement == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteRequirement, settings);
				Common.Logger.LogTraceEvent("Requirement_Create1", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRequirement responseObj = JsonConvert.DeserializeObject<RemoteRequirement>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_Create1", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRequirement responseObj = JsonConvert.DeserializeObject<RemoteRequirement>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteRequirement Requirement_Create2(RemoteCredentials credentials, object project_id, object indent_position, RemoteRequirement remoteRequirement)
		{
			string path = "projects/{project_id}/requirements/indent/{indent_position}";

            //Create the request
            RestRequest request = new RestRequest("Requirement_Create2");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "indent_position", indent_position);
			

			//Specify the body (if appropriate)
			if (remoteRequirement == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteRequirement, settings);
				Common.Logger.LogTraceEvent("Requirement_Create2", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRequirement responseObj = JsonConvert.DeserializeObject<RemoteRequirement>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_Create2", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRequirement responseObj = JsonConvert.DeserializeObject<RemoteRequirement>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteRequirement Requirement_Create3(RemoteCredentials credentials, object project_id, object parent_requirement_id, RemoteRequirement remoteRequirement)
		{
			string path = "projects/{project_id}/requirements/parent/{parent_requirement_id}";

            //Create the request
            RestRequest request = new RestRequest("Requirement_Create3");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "parent_requirement_id", parent_requirement_id);
			

			//Specify the body (if appropriate)
			if (remoteRequirement == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteRequirement, settings);
				Common.Logger.LogTraceEvent("Requirement_Create3", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRequirement responseObj = JsonConvert.DeserializeObject<RemoteRequirement>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_Create3", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRequirement responseObj = JsonConvert.DeserializeObject<RemoteRequirement>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteRequirement[] Requirement_Retrieve1(RemoteCredentials credentials, object project_id, object starting_row, object number_of_rows, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/requirements/search?starting_row={starting_row}&number_of_rows={number_of_rows}";

            //Create the request
            RestRequest request = new RestRequest("Requirement_Retrieve1");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "starting_row", starting_row);
			AddRequestParameter(request, "number_of_rows", number_of_rows);
			

			//Specify the body (if appropriate)
			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteFilters, settings);
				Common.Logger.LogTraceEvent("Requirement_Retrieve1", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRequirement[] responseObj = JsonConvert.DeserializeObject<RemoteRequirement[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_Retrieve1", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRequirement[] responseObj = JsonConvert.DeserializeObject<RemoteRequirement[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteRequirement[] Requirement_Retrieve2(RemoteCredentials credentials, object project_id, object starting_row, object number_of_rows)
		{
			string path = "projects/{project_id}/requirements?starting_row={starting_row}&number_of_rows={number_of_rows}";

            //Create the request
            RestRequest request = new RestRequest("Requirement_Retrieve2");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "starting_row", starting_row);
			AddRequestParameter(request, "number_of_rows", number_of_rows);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRequirement[] responseObj = JsonConvert.DeserializeObject<RemoteRequirement[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_Retrieve2", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRequirement[] responseObj = JsonConvert.DeserializeObject<RemoteRequirement[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteRequirement Requirement_RetrieveById(RemoteCredentials credentials, object project_id, object requirement_id)
		{
			string path = "projects/{project_id}/requirements/{requirement_id}";

            //Create the request
            RestRequest request = new RestRequest("Requirement_RetrieveById");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "requirement_id", requirement_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRequirement responseObj = JsonConvert.DeserializeObject<RemoteRequirement>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_RetrieveById", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRequirement responseObj = JsonConvert.DeserializeObject<RemoteRequirement>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void Requirement_Update(RemoteCredentials credentials, object project_id, RemoteRequirement remoteRequirement)
		{
			string path = "projects/{project_id}/requirements";

            //Create the request
            RestRequest request = new RestRequest("Requirement_Update");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "PUT";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteRequirement == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteRequirement, settings);
				Common.Logger.LogTraceEvent("Requirement_Update", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteRequirement[] Requirement_RetrieveForOwner(RemoteCredentials credentials)
		{
			string path = "requirements";

            //Create the request
            RestRequest request = new RestRequest("Requirement_RetrieveForOwner");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRequirement[] responseObj = JsonConvert.DeserializeObject<RemoteRequirement[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_RetrieveForOwner", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRequirement[] responseObj = JsonConvert.DeserializeObject<RemoteRequirement[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void Requirement_Delete(RemoteCredentials credentials, object project_id, object requirement_id)
		{
			string path = "projects/{project_id}/requirements/{requirement_id}";

            //Create the request
            RestRequest request = new RestRequest("Requirement_Delete");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "DELETE";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "requirement_id", requirement_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void Requirement_Move(RemoteCredentials credentials, object project_id, object requirement_id, object destination_requirement_id)
		{
			string path = "projects/{project_id}/requirements/{requirement_id}/move/{destination_requirement_id}";

            //Create the request
            RestRequest request = new RestRequest("Requirement_Move");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "requirement_id", requirement_id);
			AddRequestParameter(request, "destination_requirement_id", destination_requirement_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void Requirement_Indent(RemoteCredentials credentials, object project_id, object requirement_id)
		{
			string path = "projects/{project_id}/requirements/{requirement_id}/indent";

            //Create the request
            RestRequest request = new RestRequest("Requirement_Indent");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "requirement_id", requirement_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void Requirement_Outdent(RemoteCredentials credentials, object project_id, object requirement_id)
		{
			string path = "projects/{project_id}/requirements/{requirement_id}/outdent";

            //Create the request
            RestRequest request = new RestRequest("Requirement_Outdent");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "requirement_id", requirement_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteComment[] Requirement_RetrieveComments(RemoteCredentials credentials, object project_id, object requirement_id)
		{
			string path = "projects/{project_id}/requirements/{requirement_id}/comments";

            //Create the request
            RestRequest request = new RestRequest("Requirement_RetrieveComments");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "requirement_id", requirement_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteComment[] responseObj = JsonConvert.DeserializeObject<RemoteComment[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_RetrieveComments", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteComment[] responseObj = JsonConvert.DeserializeObject<RemoteComment[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteComment Requirement_CreateComment(RemoteCredentials credentials, object project_id, object requirement_id, RemoteComment remoteComment)
		{
			string path = "projects/{project_id}/requirements/{requirement_id}/comments";

            //Create the request
            RestRequest request = new RestRequest("Requirement_CreateComment");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "requirement_id", requirement_id);
			

			//Specify the body (if appropriate)
			if (remoteComment == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteComment, settings);
				Common.Logger.LogTraceEvent("Requirement_CreateComment", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteComment responseObj = JsonConvert.DeserializeObject<RemoteComment>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_CreateComment", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteComment responseObj = JsonConvert.DeserializeObject<RemoteComment>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void Requirement_AddTestCoverage(RemoteCredentials credentials, object project_id, RemoteRequirementTestCaseMapping remoteReqTestCaseMapping)
		{
			string path = "projects/{project_id}/requirements/test-cases";

            //Create the request
            RestRequest request = new RestRequest("Requirement_AddTestCoverage");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteReqTestCaseMapping == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteReqTestCaseMapping, settings);
				Common.Logger.LogTraceEvent("Requirement_AddTestCoverage", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void Requirement_RemoveTestCoverage(RemoteCredentials credentials, object project_id, RemoteRequirementTestCaseMapping remoteReqTestCaseMapping)
		{
			string path = "projects/{project_id}/requirements/test-cases";

            //Create the request
            RestRequest request = new RestRequest("Requirement_RemoveTestCoverage");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "DELETE";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteReqTestCaseMapping == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteReqTestCaseMapping, settings);
				Common.Logger.LogTraceEvent("Requirement_RemoveTestCoverage", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteRequirementTestCaseMapping[] Requirement_RetrieveTestCoverage(RemoteCredentials credentials, object project_id, object requirement_id)
		{
			string path = "projects/{project_id}/requirements/{requirement_id}/test-cases";

            //Create the request
            RestRequest request = new RestRequest("Requirement_RetrieveTestCoverage");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "requirement_id", requirement_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRequirementTestCaseMapping[] responseObj = JsonConvert.DeserializeObject<RemoteRequirementTestCaseMapping[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_RetrieveTestCoverage", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRequirementTestCaseMapping[] responseObj = JsonConvert.DeserializeObject<RemoteRequirementTestCaseMapping[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteVersion System_GetProductVersion(RemoteCredentials credentials)
		{
			string path = "system/productVersion";

            //Create the request
            RestRequest request = new RestRequest("System_GetProductVersion");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteVersion responseObj = JsonConvert.DeserializeObject<RemoteVersion>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("System_GetProductVersion", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteVersion responseObj = JsonConvert.DeserializeObject<RemoteVersion>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTask Task_Create(RemoteCredentials credentials, object project_id, RemoteTask remoteTask)
		{
			string path = "projects/{project_id}/tasks";

            //Create the request
            RestRequest request = new RestRequest("Task_Create");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteTask == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteTask, settings);
				Common.Logger.LogTraceEvent("Task_Create", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTask responseObj = JsonConvert.DeserializeObject<RemoteTask>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_Create", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTask responseObj = JsonConvert.DeserializeObject<RemoteTask>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public Int64 Task_Count1(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/tasks/count";

            //Create the request
            RestRequest request = new RestRequest("Task_Count1");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_Count1", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public Int64 Task_Count2(RemoteCredentials credentials, object project_id, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/tasks/count";

            //Create the request
            RestRequest request = new RestRequest("Task_Count2");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteFilters, settings);
				Common.Logger.LogTraceEvent("Task_Count2", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_Count2", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTask[] Task_Retrieve(RemoteCredentials credentials, object project_id, object starting_row, object number_of_rows, object sort_field, object sort_direction, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/tasks?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}";

            //Create the request
            RestRequest request = new RestRequest("Task_Retrieve");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "starting_row", starting_row);
			AddRequestParameter(request, "number_of_rows", number_of_rows);
			AddRequestParameter(request, "sort_field", sort_field);
			AddRequestParameter(request, "sort_direction", sort_direction);
			

			//Specify the body (if appropriate)
			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteFilters, settings);
				Common.Logger.LogTraceEvent("Task_Retrieve", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTask[] responseObj = JsonConvert.DeserializeObject<RemoteTask[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_Retrieve", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTask[] responseObj = JsonConvert.DeserializeObject<RemoteTask[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTask Task_RetrieveById(RemoteCredentials credentials, object project_id, object task_id)
		{
			string path = "projects/{project_id}/tasks/{task_id}";

            //Create the request
            RestRequest request = new RestRequest("Task_RetrieveById");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "task_id", task_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTask responseObj = JsonConvert.DeserializeObject<RemoteTask>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_RetrieveById", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTask responseObj = JsonConvert.DeserializeObject<RemoteTask>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTask[] Task_RetrieveForOwner(RemoteCredentials credentials)
		{
			string path = "tasks";

            //Create the request
            RestRequest request = new RestRequest("Task_RetrieveForOwner");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTask[] responseObj = JsonConvert.DeserializeObject<RemoteTask[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_RetrieveForOwner", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTask[] responseObj = JsonConvert.DeserializeObject<RemoteTask[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTask[] Task_RetrieveNew(RemoteCredentials credentials, object project_id, object creation_date, object start_row, object number_of_rows)
		{
			string path = "projects/{project_id}/tasks/new?creation_date={creation_date}&start_row={start_row}&number_of_rows={number_of_rows}";

            //Create the request
            RestRequest request = new RestRequest("Task_RetrieveNew");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "creation_date", creation_date);
			AddRequestParameter(request, "start_row", start_row);
			AddRequestParameter(request, "number_of_rows", number_of_rows);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTask[] responseObj = JsonConvert.DeserializeObject<RemoteTask[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_RetrieveNew", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTask[] responseObj = JsonConvert.DeserializeObject<RemoteTask[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void Task_Update(RemoteCredentials credentials, object project_id, RemoteTask remoteTask)
		{
			string path = "projects/{project_id}/tasks";

            //Create the request
            RestRequest request = new RestRequest("Task_Update");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "PUT";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteTask == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteTask, settings);
				Common.Logger.LogTraceEvent("Task_Update", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void Task_Delete(RemoteCredentials credentials, object project_id, object task_id)
		{
			string path = "projects/{project_id}/tasks/{task_id}";

            //Create the request
            RestRequest request = new RestRequest("Task_Delete");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "DELETE";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "task_id", task_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteComment Task_CreateComment(RemoteCredentials credentials, object project_id, object task_id, RemoteComment remoteComment)
		{
			string path = "projects/{project_id}/tasks/{task_id}/comments";

            //Create the request
            RestRequest request = new RestRequest("Task_CreateComment");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "task_id", task_id);
			

			//Specify the body (if appropriate)
			if (remoteComment == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteComment, settings);
				Common.Logger.LogTraceEvent("Task_CreateComment", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteComment responseObj = JsonConvert.DeserializeObject<RemoteComment>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_CreateComment", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteComment responseObj = JsonConvert.DeserializeObject<RemoteComment>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteComment[] Task_RetrieveComments(RemoteCredentials credentials, object project_id, object task_id)
		{
			string path = "projects/{project_id}/tasks/{task_id}/comments";

            //Create the request
            RestRequest request = new RestRequest("Task_RetrieveComments");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "task_id", task_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteComment[] responseObj = JsonConvert.DeserializeObject<RemoteComment[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_RetrieveComments", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteComment[] responseObj = JsonConvert.DeserializeObject<RemoteComment[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public Int64 TestCase_Count1(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/test-cases/count";

            //Create the request
            RestRequest request = new RestRequest("TestCase_Count1");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_Count1", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public Int64 TestCase_Count2(RemoteCredentials credentials, object project_id, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/test-cases/count";

            //Create the request
            RestRequest request = new RestRequest("TestCase_Count2");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteFilters, settings);
				Common.Logger.LogTraceEvent("TestCase_Count2", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_Count2", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTestCase TestCase_Create(RemoteCredentials credentials, object project_id, object parent_test_folder_id, RemoteTestCase remoteTestCase)
		{
			string path = "projects/{project_id}/test-cases?parent_test_folder_id={parent_test_folder_id}";

            //Create the request
            RestRequest request = new RestRequest("TestCase_Create");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "parent_test_folder_id", parent_test_folder_id);
			

			//Specify the body (if appropriate)
			if (remoteTestCase == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteTestCase, settings);
				Common.Logger.LogTraceEvent("TestCase_Create", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestCase responseObj = JsonConvert.DeserializeObject<RemoteTestCase>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_Create", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestCase responseObj = JsonConvert.DeserializeObject<RemoteTestCase>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTestCase[] TestCase_Retrieve1(RemoteCredentials credentials, object project_id, object starting_row, object number_of_rows, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/test-cases/search?starting_row={starting_row}&number_of_rows={number_of_rows}";

            //Create the request
            RestRequest request = new RestRequest("TestCase_Retrieve1");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "starting_row", starting_row);
			AddRequestParameter(request, "number_of_rows", number_of_rows);
			

			//Specify the body (if appropriate)
			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteFilters, settings);
				Common.Logger.LogTraceEvent("TestCase_Retrieve1", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestCase[] responseObj = JsonConvert.DeserializeObject<RemoteTestCase[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_Retrieve1", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestCase[] responseObj = JsonConvert.DeserializeObject<RemoteTestCase[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTestCase[] TestCase_Retrieve2(RemoteCredentials credentials, object project_id, object starting_row, object number_of_rows)
		{
			string path = "projects/{project_id}/test-cases/search?starting_row={starting_row}&number_of_rows={number_of_rows}";

            //Create the request
            RestRequest request = new RestRequest("TestCase_Retrieve2");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "starting_row", starting_row);
			AddRequestParameter(request, "number_of_rows", number_of_rows);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestCase[] responseObj = JsonConvert.DeserializeObject<RemoteTestCase[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_Retrieve2", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestCase[] responseObj = JsonConvert.DeserializeObject<RemoteTestCase[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTestCase TestCase_RetrieveById(RemoteCredentials credentials, object project_id, object test_case_id)
		{
			string path = "projects/{project_id}/test-cases/{test_case_id}";

            //Create the request
            RestRequest request = new RestRequest("TestCase_RetrieveById");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_case_id", test_case_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestCase responseObj = JsonConvert.DeserializeObject<RemoteTestCase>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveById", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestCase responseObj = JsonConvert.DeserializeObject<RemoteTestCase>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTestCase[] TestCase_RetrieveForOwner(RemoteCredentials credentials)
		{
			string path = "test-cases";

            //Create the request
            RestRequest request = new RestRequest("TestCase_RetrieveForOwner");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestCase[] responseObj = JsonConvert.DeserializeObject<RemoteTestCase[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveForOwner", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestCase[] responseObj = JsonConvert.DeserializeObject<RemoteTestCase[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTestCase[] TestCase_RetrieveByFolder(RemoteCredentials credentials, object project_id, object test_case_folder_id)
		{
			string path = "projects/{project_id}/test-folders/{test_case_folder_id}/test-cases";

            //Create the request
            RestRequest request = new RestRequest("TestCase_RetrieveByFolder");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_case_folder_id", test_case_folder_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestCase[] responseObj = JsonConvert.DeserializeObject<RemoteTestCase[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveByFolder", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestCase[] responseObj = JsonConvert.DeserializeObject<RemoteTestCase[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void TestCase_Update(RemoteCredentials credentials, object project_id, RemoteTestCase remoteTestCase)
		{
			string path = "projects/{project_id}/test-cases";

            //Create the request
            RestRequest request = new RestRequest("TestCase_Update");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "PUT";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteTestCase == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteTestCase, settings);
				Common.Logger.LogTraceEvent("TestCase_Update", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void TestCase_Delete(RemoteCredentials credentials, object project_id, object test_case_id)
		{
			string path = "projects/{project_id}/test-cases/{test_case_id}";

            //Create the request
            RestRequest request = new RestRequest("TestCase_Delete");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "DELETE";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_case_id", test_case_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void TestCase_Move(RemoteCredentials credentials, object project_id, object test_case_id, object destination_test_case_folder_id)
		{
			string path = "projects/{project_id}/test-cases/{test_case_id}/move?destination_test_case_folder_id={destination_test_case_folder_id}";

            //Create the request
            RestRequest request = new RestRequest("TestCase_Move");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_case_id", test_case_id);
			AddRequestParameter(request, "destination_test_case_folder_id", destination_test_case_folder_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void TestCase_AddUpdateAutomationScript(RemoteCredentials credentials, object project_id, object test_case_id, object automation_engine_id, object url_or_filename, object description, object version, object project_attachment_type_id, object project_attachment_folder_id, Byte[] binaryData)
		{
			string path = "projects/{project_id}/test-cases/{test_case_id}/add-update-automation-script?automation_engine_id={automation_engine_id}&url_or_filename={url_or_filename}&description={description}&version={version}&project_attachment_type_id={project_attachment_type_id}&project_attachment_folder_id={project_attachment_folder_id}";

            //Create the request
            RestRequest request = new RestRequest("TestCase_AddUpdateAutomationScript");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_case_id", test_case_id);
			AddRequestParameter(request, "automation_engine_id", automation_engine_id);
			AddRequestParameter(request, "url_or_filename", url_or_filename);
			AddRequestParameter(request, "description", description);
			AddRequestParameter(request, "version", version);
			AddRequestParameter(request, "project_attachment_type_id", project_attachment_type_id);
			AddRequestParameter(request, "project_attachment_folder_id", project_attachment_folder_id);
			

			//Specify the body (if appropriate)
			if (binaryData == null)
			{
				request.Body = "";
			}
			else
			{
				string json = "[";
				bool first = true;
				foreach (byte item in (byte[])binaryData)
				{
					if (!first)
					{
						json += ",";
					}
					else
					{
						first = false;
					}
					json += item.ToString();
				}
				json += "]";
				request.Body = json;
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteComment[] TestCase_RetrieveComments(RemoteCredentials credentials, object project_id, object test_case_id)
		{
			string path = "projects/{project_id}/test-cases/{test_case_id}/comments";

            //Create the request
            RestRequest request = new RestRequest("TestCase_RetrieveComments");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_case_id", test_case_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteComment[] responseObj = JsonConvert.DeserializeObject<RemoteComment[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveComments", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteComment[] responseObj = JsonConvert.DeserializeObject<RemoteComment[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteComment TestCase_CreateComment(RemoteCredentials credentials, object project_id, object test_case_id, RemoteComment remoteComment)
		{
			string path = "projects/{project_id}/test-cases/{test_case_id}/comments";

            //Create the request
            RestRequest request = new RestRequest("TestCase_CreateComment");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_case_id", test_case_id);
			

			//Specify the body (if appropriate)
			if (remoteComment == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteComment, settings);
				Common.Logger.LogTraceEvent("TestCase_CreateComment", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteComment responseObj = JsonConvert.DeserializeObject<RemoteComment>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_CreateComment", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteComment responseObj = JsonConvert.DeserializeObject<RemoteComment>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTestCase TestCase_CreateFolder(RemoteCredentials credentials, object project_id, object parent_test_folder_id, RemoteTestCase remoteTestCase)
		{
			string path = "projects/{project_id}/test-folders?parent_test_folder_id={parent_test_folder_id}";

            //Create the request
            RestRequest request = new RestRequest("TestCase_CreateFolder");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "parent_test_folder_id", parent_test_folder_id);
			

			//Specify the body (if appropriate)
			if (remoteTestCase == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteTestCase, settings);
				Common.Logger.LogTraceEvent("TestCase_CreateFolder", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestCase responseObj = JsonConvert.DeserializeObject<RemoteTestCase>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_CreateFolder", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestCase responseObj = JsonConvert.DeserializeObject<RemoteTestCase>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public Int64 TestCase_CountForFolder1(RemoteCredentials credentials, object project_id, object test_folder_id)
		{
			string path = "projects/{project_id}/test-folders/{test_folder_id}/count";

            //Create the request
            RestRequest request = new RestRequest("TestCase_CountForFolder1");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_folder_id", test_folder_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_CountForFolder1", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public Int64 TestCase_CountForFolder2(RemoteCredentials credentials, object project_id, object test_folder_id, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/test-folders/{test_folder_id}/count";

            //Create the request
            RestRequest request = new RestRequest("TestCase_CountForFolder2");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_folder_id", test_folder_id);
			

			//Specify the body (if appropriate)
			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteFilters, settings);
				Common.Logger.LogTraceEvent("TestCase_CountForFolder2", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_CountForFolder2", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public Int32 TestCase_AddLink(RemoteCredentials credentials, object project_id, object test_case_id, object linked_test_case_id, object position, RemoteTestStepParameter[] parameters)
		{
			string path = "projects/{project_id}/test-cases/{test_case_id}/test-links/{linked_test_case_id}?position={position}";

            //Create the request
            RestRequest request = new RestRequest("TestCase_AddLink");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_case_id", test_case_id);
			AddRequestParameter(request, "linked_test_case_id", linked_test_case_id);
			AddRequestParameter(request, "position", position);
			

			//Specify the body (if appropriate)
			if (parameters == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(parameters, settings);
				Common.Logger.LogTraceEvent("TestCase_AddLink", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int32 responseObj = JsonConvert.DeserializeObject<Int32>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_AddLink", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int32 responseObj = JsonConvert.DeserializeObject<Int32>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTestCaseParameter TestCase_AddParameter(RemoteCredentials credentials, object project_id, RemoteTestCaseParameter remoteTestCaseParameter)
		{
			string path = "projects/{project_id}/test-cases/parameters";

            //Create the request
            RestRequest request = new RestRequest("TestCase_AddParameter");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteTestCaseParameter == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteTestCaseParameter, settings);
				Common.Logger.LogTraceEvent("TestCase_AddParameter", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestCaseParameter responseObj = JsonConvert.DeserializeObject<RemoteTestCaseParameter>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_AddParameter", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestCaseParameter responseObj = JsonConvert.DeserializeObject<RemoteTestCaseParameter>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public String TestCase_CreateParameterToken(RemoteCredentials credentials, object project_id, object parameter_name)
		{
			string path = "projects/{project_id}/test-cases/parameters/create-token?parameter_name={parameter_name}";

            //Create the request
            RestRequest request = new RestRequest("TestCase_CreateParameterToken");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "parameter_name", parameter_name);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					String responseObj = JsonConvert.DeserializeObject<String>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_CreateParameterToken", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					String responseObj = JsonConvert.DeserializeObject<String>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTestCaseParameter[] TestCase_RetrieveParameters(RemoteCredentials credentials, object project_id, object test_case_id)
		{
			string path = "projects/{project_id}/test-cases/{test_case_id}/parameters";

            //Create the request
            RestRequest request = new RestRequest("TestCase_RetrieveParameters");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_case_id", test_case_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestCaseParameter[] responseObj = JsonConvert.DeserializeObject<RemoteTestCaseParameter[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveParameters", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestCaseParameter[] responseObj = JsonConvert.DeserializeObject<RemoteTestCaseParameter[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteManualTestRun[] TestRun_Save(RemoteCredentials credentials, object project_id, object end_date, RemoteManualTestRun[] remoteTestRuns)
		{
			string path = "projects/{project_id}/test-runs?end_date={end_date}";

            //Create the request
            RestRequest request = new RestRequest("TestRun_Save");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "PUT";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "end_date", end_date);
			

			//Specify the body (if appropriate)
			if (remoteTestRuns == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteTestRuns, settings);
				Common.Logger.LogTraceEvent("TestRun_Save", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteManualTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteManualTestRun[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_Save", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteManualTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteManualTestRun[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTestRun TestRun_RetrieveById(RemoteCredentials credentials, object project_id, object test_run_id)
		{
			string path = "projects/{project_id}/test-runs/{test_run_id}";

            //Create the request
            RestRequest request = new RestRequest("TestRun_RetrieveById");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_run_id", test_run_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestRun responseObj = JsonConvert.DeserializeObject<RemoteTestRun>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_RetrieveById", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestRun responseObj = JsonConvert.DeserializeObject<RemoteTestRun>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteAutomatedTestRun TestRun_RetrieveAutomatedById(RemoteCredentials credentials, object project_id, object test_run_id)
		{
			string path = "projects/{project_id}/test-runs/{test_run_id}/automated";

            //Create the request
            RestRequest request = new RestRequest("TestRun_RetrieveAutomatedById");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_run_id", test_run_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomatedTestRun responseObj = JsonConvert.DeserializeObject<RemoteAutomatedTestRun>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_RetrieveAutomatedById", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomatedTestRun responseObj = JsonConvert.DeserializeObject<RemoteAutomatedTestRun>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteManualTestRun TestRun_RetrieveManualById(RemoteCredentials credentials, object project_id, object test_run_id)
		{
			string path = "projects/{project_id}/test-runs/{test_run_id}/manual";

            //Create the request
            RestRequest request = new RestRequest("TestRun_RetrieveManualById");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_run_id", test_run_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteManualTestRun responseObj = JsonConvert.DeserializeObject<RemoteManualTestRun>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_RetrieveManualById", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteManualTestRun responseObj = JsonConvert.DeserializeObject<RemoteManualTestRun>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTestRun[] TestRun_Retrieve1(RemoteCredentials credentials, object project_id, object starting_row, object number_of_rows, object sort_field, object sort_direction)
		{
			string path = "projects/{project_id}/test-runs?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}";

            //Create the request
            RestRequest request = new RestRequest("TestRun_Retrieve1");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "starting_row", starting_row);
			AddRequestParameter(request, "number_of_rows", number_of_rows);
			AddRequestParameter(request, "sort_field", sort_field);
			AddRequestParameter(request, "sort_direction", sort_direction);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteTestRun[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_Retrieve1", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteTestRun[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTestRun[] TestRun_Retrieve2(RemoteCredentials credentials, object project_id, object starting_row, object number_of_rows, object sort_field, object sort_direction, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/test-runs/search?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}";

            //Create the request
            RestRequest request = new RestRequest("TestRun_Retrieve2");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "starting_row", starting_row);
			AddRequestParameter(request, "number_of_rows", number_of_rows);
			AddRequestParameter(request, "sort_field", sort_field);
			AddRequestParameter(request, "sort_direction", sort_direction);
			

			//Specify the body (if appropriate)
			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteFilters, settings);
				Common.Logger.LogTraceEvent("TestRun_Retrieve2", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteTestRun[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_Retrieve2", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteTestRun[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteManualTestRun[] TestRun_RetrieveManual1(RemoteCredentials credentials, object project_id, object starting_row, object number_of_rows, object sort_field, object sort_direction)
		{
			string path = "projects/{project_id}/test-runs/manual?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}";

            //Create the request
            RestRequest request = new RestRequest("TestRun_RetrieveManual1");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "starting_row", starting_row);
			AddRequestParameter(request, "number_of_rows", number_of_rows);
			AddRequestParameter(request, "sort_field", sort_field);
			AddRequestParameter(request, "sort_direction", sort_direction);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteManualTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteManualTestRun[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_RetrieveManual1", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteManualTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteManualTestRun[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteManualTestRun[] TestRun_RetrieveManual2(RemoteCredentials credentials, object project_id, object starting_row, object number_of_rows, object sort_field, object sort_direction, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/test-runs/search/manual?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}";

            //Create the request
            RestRequest request = new RestRequest("TestRun_RetrieveManual2");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "starting_row", starting_row);
			AddRequestParameter(request, "number_of_rows", number_of_rows);
			AddRequestParameter(request, "sort_field", sort_field);
			AddRequestParameter(request, "sort_direction", sort_direction);
			

			//Specify the body (if appropriate)
			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteFilters, settings);
				Common.Logger.LogTraceEvent("TestRun_RetrieveManual2", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteManualTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteManualTestRun[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_RetrieveManual2", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteManualTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteManualTestRun[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteAutomatedTestRun[] TestRun_RetrieveAutomated1(RemoteCredentials credentials, object project_id, object starting_row, object number_of_rows, object sort_field, object sort_direction)
		{
			string path = "projects/{project_id}/test-runs/automated??starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}";

            //Create the request
            RestRequest request = new RestRequest("TestRun_RetrieveAutomated1");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "starting_row", starting_row);
			AddRequestParameter(request, "number_of_rows", number_of_rows);
			AddRequestParameter(request, "sort_field", sort_field);
			AddRequestParameter(request, "sort_direction", sort_direction);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomatedTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteAutomatedTestRun[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_RetrieveAutomated1", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomatedTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteAutomatedTestRun[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteAutomatedTestRun[] TestRun_RetrieveAutomated2(RemoteCredentials credentials, object project_id, object starting_row, object number_of_rows, object sort_field, object sort_direction, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/test-runs/search/automated??starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}";

            //Create the request
            RestRequest request = new RestRequest("TestRun_RetrieveAutomated2");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "starting_row", starting_row);
			AddRequestParameter(request, "number_of_rows", number_of_rows);
			AddRequestParameter(request, "sort_field", sort_field);
			AddRequestParameter(request, "sort_direction", sort_direction);
			

			//Specify the body (if appropriate)
			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteFilters, settings);
				Common.Logger.LogTraceEvent("TestRun_RetrieveAutomated2", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomatedTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteAutomatedTestRun[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_RetrieveAutomated2", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomatedTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteAutomatedTestRun[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteManualTestRun[] TestRun_CreateFromTestCases(RemoteCredentials credentials, object project_id, object release_id, Int32[] testCaseIds)
		{
			string path = "projects/{project_id}/test-runs/create?release_id={release_id}";

            //Create the request
            RestRequest request = new RestRequest("TestRun_CreateFromTestCases");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "release_id", release_id);
			

			//Specify the body (if appropriate)
			if (testCaseIds == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(testCaseIds, settings);
				Common.Logger.LogTraceEvent("TestRun_CreateFromTestCases", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteManualTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteManualTestRun[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_CreateFromTestCases", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteManualTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteManualTestRun[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteManualTestRun[] TestRun_CreateFromTestSet(RemoteCredentials credentials, object project_id, object test_set_id)
		{
			string path = "projects/{project_id}/test-runs/create/test_set/{test_set_id}";

            //Create the request
            RestRequest request = new RestRequest("TestRun_CreateFromTestSet");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_set_id", test_set_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteManualTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteManualTestRun[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_CreateFromTestSet", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteManualTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteManualTestRun[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteAutomatedTestRun[] TestRun_CreateForAutomationHost(RemoteCredentials credentials, object project_id, object automation_host_token, DateRange dateRange)
		{
			string path = "projects/{project_id}/test-runs/create/automation_host/{automation_host_token}";

            //Create the request
            RestRequest request = new RestRequest("TestRun_CreateForAutomationHost");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "automation_host_token", automation_host_token);
			

			//Specify the body (if appropriate)
			if (dateRange == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(dateRange, settings);
				Common.Logger.LogTraceEvent("TestRun_CreateForAutomationHost", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomatedTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteAutomatedTestRun[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_CreateForAutomationHost", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomatedTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteAutomatedTestRun[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteAutomatedTestRun[] TestRun_CreateForAutomatedTestSet(RemoteCredentials credentials, object project_id, object test_set_id, object automation_host_token)
		{
			string path = "projects/{project_id}/test-runs/create/test_set/{test_set_id}/automation_host/{automation_host_token}";

            //Create the request
            RestRequest request = new RestRequest("TestRun_CreateForAutomatedTestSet");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_set_id", test_set_id);
			AddRequestParameter(request, "automation_host_token", automation_host_token);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomatedTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteAutomatedTestRun[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_CreateForAutomatedTestSet", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomatedTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteAutomatedTestRun[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public Int64 TestRun_Count1(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/test-runs/count";

            //Create the request
            RestRequest request = new RestRequest("TestRun_Count1");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_Count1", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public Int64 TestRun_Count2(RemoteCredentials credentials, object project_id, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/test-runs/count";

            //Create the request
            RestRequest request = new RestRequest("TestRun_Count2");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteFilters, settings);
				Common.Logger.LogTraceEvent("TestRun_Count2", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_Count2", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteAutomatedTestRun TestRun_RecordAutomated1(RemoteCredentials credentials, object project_id, RemoteAutomatedTestRun remoteTestRun)
		{
			string path = "projects/{project_id}/test-runs/record";

            //Create the request
            RestRequest request = new RestRequest("TestRun_RecordAutomated1");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteTestRun == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteTestRun, settings);
				Common.Logger.LogTraceEvent("TestRun_RecordAutomated1", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomatedTestRun responseObj = JsonConvert.DeserializeObject<RemoteAutomatedTestRun>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_RecordAutomated1", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomatedTestRun responseObj = JsonConvert.DeserializeObject<RemoteAutomatedTestRun>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteAutomatedTestRun[] TestRun_RecordAutomated2(RemoteCredentials credentials, object project_id, RemoteAutomatedTestRun[] remoteTestRuns)
		{
			string path = "projects/{project_id}/test-runs/record-multiple";

            //Create the request
            RestRequest request = new RestRequest("TestRun_RecordAutomated2");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteTestRuns == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteTestRuns, settings);
				Common.Logger.LogTraceEvent("TestRun_RecordAutomated2", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomatedTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteAutomatedTestRun[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_RecordAutomated2", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAutomatedTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteAutomatedTestRun[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTestSet TestSet_Create(RemoteCredentials credentials, object project_id, object parent_test_set_folder_id, RemoteTestSet remoteTestSet)
		{
			string path = "projects/{project_id}/test-sets?parent_test_set_folder_id={parent_test_set_folder_id}";

            //Create the request
            RestRequest request = new RestRequest("TestSet_Create");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "parent_test_set_folder_id", parent_test_set_folder_id);
			

			//Specify the body (if appropriate)
			if (remoteTestSet == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteTestSet, settings);
				Common.Logger.LogTraceEvent("TestSet_Create", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestSet responseObj = JsonConvert.DeserializeObject<RemoteTestSet>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_Create", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestSet responseObj = JsonConvert.DeserializeObject<RemoteTestSet>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTestSet[] TestSet_Retrieve1(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/test-sets";

            //Create the request
            RestRequest request = new RestRequest("TestSet_Retrieve1");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestSet[] responseObj = JsonConvert.DeserializeObject<RemoteTestSet[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_Retrieve1", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestSet[] responseObj = JsonConvert.DeserializeObject<RemoteTestSet[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTestSet[] TestSet_Retrieve2(RemoteCredentials credentials, object project_id, object starting_row, object number_of_rows, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/test-sets/search?starting_row={starting_row}&number_of_rows={number_of_rows}";

            //Create the request
            RestRequest request = new RestRequest("TestSet_Retrieve2");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "starting_row", starting_row);
			AddRequestParameter(request, "number_of_rows", number_of_rows);
			

			//Specify the body (if appropriate)
			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteFilters, settings);
				Common.Logger.LogTraceEvent("TestSet_Retrieve2", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestSet[] responseObj = JsonConvert.DeserializeObject<RemoteTestSet[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_Retrieve2", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestSet[] responseObj = JsonConvert.DeserializeObject<RemoteTestSet[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTestSet TestSet_RetrieveById(RemoteCredentials credentials, object project_id, object test_set_id)
		{
			string path = "projects/{project_id}/test-sets/{test_set_id}";

            //Create the request
            RestRequest request = new RestRequest("TestSet_RetrieveById");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_set_id", test_set_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestSet responseObj = JsonConvert.DeserializeObject<RemoteTestSet>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_RetrieveById", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestSet responseObj = JsonConvert.DeserializeObject<RemoteTestSet>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTestSet[] TestSet_RetrieveForOwner(RemoteCredentials credentials)
		{
			string path = "test-sets";

            //Create the request
            RestRequest request = new RestRequest("TestSet_RetrieveForOwner");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestSet[] responseObj = JsonConvert.DeserializeObject<RemoteTestSet[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_RetrieveForOwner", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestSet[] responseObj = JsonConvert.DeserializeObject<RemoteTestSet[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void TestSet_Update(RemoteCredentials credentials, object project_id, RemoteTestSet remoteTestSet)
		{
			string path = "projects/{project_id}/test-sets/";

            //Create the request
            RestRequest request = new RestRequest("TestSet_Update");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "PUT";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteTestSet == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteTestSet, settings);
				Common.Logger.LogTraceEvent("TestSet_Update", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void TestSet_Delete(RemoteCredentials credentials, object project_id, object test_set_id)
		{
			string path = "projects/{project_id}/test-sets/{test_set_id}";

            //Create the request
            RestRequest request = new RestRequest("TestSet_Delete");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "DELETE";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_set_id", test_set_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void TestSet_Move(RemoteCredentials credentials, object project_id, object test_set_id, object destination_test_set_folder_id)
		{
			string path = "projects/{project_id}/test-sets/{test_set_id}/move?destination_test_set_folder_id={destination_test_set_folder_id}";

            //Create the request
            RestRequest request = new RestRequest("TestSet_Move");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_set_id", test_set_id);
			AddRequestParameter(request, "destination_test_set_folder_id", destination_test_set_folder_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public Int64 TestSet_Count1(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/test-sets/count";

            //Create the request
            RestRequest request = new RestRequest("TestSet_Count1");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_Count1", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public Int64 TestSet_Count2(RemoteCredentials credentials, object project_id, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/test-sets/count";

            //Create the request
            RestRequest request = new RestRequest("TestSet_Count2");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			

			//Specify the body (if appropriate)
			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteFilters, settings);
				Common.Logger.LogTraceEvent("TestSet_Count2", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_Count2", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteComment[] TestSet_RetrieveComments(RemoteCredentials credentials, object project_id, object test_set_id)
		{
			string path = "projects/{project_id}/test-sets/{test_set_id}/comments";

            //Create the request
            RestRequest request = new RestRequest("TestSet_RetrieveComments");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_set_id", test_set_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteComment[] responseObj = JsonConvert.DeserializeObject<RemoteComment[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_RetrieveComments", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteComment[] responseObj = JsonConvert.DeserializeObject<RemoteComment[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteComment TestSet_CreateComment(RemoteCredentials credentials, object project_id, object test_set_id, RemoteComment remoteComment)
		{
			string path = "projects/{project_id}/test-sets/{test_set_id}/comments";

            //Create the request
            RestRequest request = new RestRequest("TestSet_CreateComment");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_set_id", test_set_id);
			

			//Specify the body (if appropriate)
			if (remoteComment == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteComment, settings);
				Common.Logger.LogTraceEvent("TestSet_CreateComment", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteComment responseObj = JsonConvert.DeserializeObject<RemoteComment>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_CreateComment", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteComment responseObj = JsonConvert.DeserializeObject<RemoteComment>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTestSet TestSet_CreateFolder(RemoteCredentials credentials, object project_id, object parent_test_set_folder_id, RemoteTestSet remoteTestSet)
		{
			string path = "projects/{project_id}/test-set-folders?parent_test_set_folder_id={parent_test_set_folder_id}";

            //Create the request
            RestRequest request = new RestRequest("TestSet_CreateFolder");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "parent_test_set_folder_id", parent_test_set_folder_id);
			

			//Specify the body (if appropriate)
			if (remoteTestSet == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteTestSet, settings);
				Common.Logger.LogTraceEvent("TestSet_CreateFolder", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestSet responseObj = JsonConvert.DeserializeObject<RemoteTestSet>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_CreateFolder", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestSet responseObj = JsonConvert.DeserializeObject<RemoteTestSet>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTestSetTestCaseMapping[] TestSet_AddTestMapping(RemoteCredentials credentials, object project_id, object test_set_id, object test_case_id, object owner_id, object existing_test_set_test_case_id, RemoteTestSetTestCaseParameter[] parameters)
		{
			string path = "projects/{project_id}/test-sets/{test_set_id}/test-case-mapping/{test_case_id}?owner_id={owner_id}&existing_test_set_test_case_id={existing_test_set_test_case_id}";

            //Create the request
            RestRequest request = new RestRequest("TestSet_AddTestMapping");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_set_id", test_set_id);
			AddRequestParameter(request, "test_case_id", test_case_id);
			AddRequestParameter(request, "owner_id", owner_id);
			AddRequestParameter(request, "existing_test_set_test_case_id", existing_test_set_test_case_id);
			

			//Specify the body (if appropriate)
			if (parameters == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(parameters, settings);
				Common.Logger.LogTraceEvent("TestSet_AddTestMapping", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestSetTestCaseMapping[] responseObj = JsonConvert.DeserializeObject<RemoteTestSetTestCaseMapping[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_AddTestMapping", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestSetTestCaseMapping[] responseObj = JsonConvert.DeserializeObject<RemoteTestSetTestCaseMapping[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void TestSet_RemoveTestMapping(RemoteCredentials credentials, object project_id, object test_set_id, object test_set_test_case_id)
		{
			string path = "projects/{project_id}/test-sets/{test_set_id}/test-cases/{test_set_test_case_id}";

            //Create the request
            RestRequest request = new RestRequest("TestSet_RemoveTestMapping");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "DELETE";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_set_id", test_set_id);
			AddRequestParameter(request, "test_set_test_case_id", test_set_test_case_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTestSetTestCaseMapping[] TestSet_RetrieveTestCaseMapping(RemoteCredentials credentials, object project_id, object test_set_id)
		{
			string path = "projects/{project_id}/test-sets/{test_set_id}/test-case-mapping";

            //Create the request
            RestRequest request = new RestRequest("TestSet_RetrieveTestCaseMapping");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_set_id", test_set_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestSetTestCaseMapping[] responseObj = JsonConvert.DeserializeObject<RemoteTestSetTestCaseMapping[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_RetrieveTestCaseMapping", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestSetTestCaseMapping[] responseObj = JsonConvert.DeserializeObject<RemoteTestSetTestCaseMapping[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTestCase[] TestCase_RetrieveByTestSetId(RemoteCredentials credentials, object project_id, object test_set_id)
		{
			string path = "projects/{project_id}/test-sets/{test_set_id}/test-cases";

            //Create the request
            RestRequest request = new RestRequest("TestCase_RetrieveByTestSetId");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_set_id", test_set_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestCase[] responseObj = JsonConvert.DeserializeObject<RemoteTestCase[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveByTestSetId", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestCase[] responseObj = JsonConvert.DeserializeObject<RemoteTestCase[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void TestCase_MoveStep(RemoteCredentials credentials, object project_id, object test_case_id, object source_test_step_id, object destination_test_step_id)
		{
			string path = "projects/{project_id}/test-cases/{test_case_id}/test-steps/{source_test_step_id}/move?destination_test_step_id={destination_test_step_id}";

            //Create the request
            RestRequest request = new RestRequest("TestCase_MoveStep");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_case_id", test_case_id);
			AddRequestParameter(request, "source_test_step_id", source_test_step_id);
			AddRequestParameter(request, "destination_test_step_id", destination_test_step_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void TestCase_DeleteStep(RemoteCredentials credentials, object project_id, object test_case_id, object test_step_id)
		{
			string path = "projects/{project_id}/test-cases/{test_case_id}/test-steps/{test_step_id}";

            //Create the request
            RestRequest request = new RestRequest("TestCase_DeleteStep");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "DELETE";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_case_id", test_case_id);
			AddRequestParameter(request, "test_step_id", test_step_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTestStep TestCase_AddStep(RemoteCredentials credentials, object project_id, object test_case_id, RemoteTestStep remoteTestStep)
		{
			string path = "projects/{project_id}/test-cases/{test_case_id}/test-steps";

            //Create the request
            RestRequest request = new RestRequest("TestCase_AddStep");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_case_id", test_case_id);
			

			//Specify the body (if appropriate)
			if (remoteTestStep == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteTestStep, settings);
				Common.Logger.LogTraceEvent("TestCase_AddStep", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestStep responseObj = JsonConvert.DeserializeObject<RemoteTestStep>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_AddStep", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestStep responseObj = JsonConvert.DeserializeObject<RemoteTestStep>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteTestStepParameter[] TestCase_RetrieveStepParameters(RemoteCredentials credentials, object project_id, object test_case_id, object test_step_id)
		{
			string path = "projects/{project_id}/test-cases/{test_case_id}/test-steps/{test_step_id}/parameters";

            //Create the request
            RestRequest request = new RestRequest("TestCase_RetrieveStepParameters");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "test_case_id", test_case_id);
			AddRequestParameter(request, "test_step_id", test_step_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestStepParameter[] responseObj = JsonConvert.DeserializeObject<RemoteTestStepParameter[]>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveStepParameters", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTestStepParameter[] responseObj = JsonConvert.DeserializeObject<RemoteTestStepParameter[]>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteUser User_Create(RemoteCredentials credentials, object password, object password_question, object password_answer, object project_id, object project_role_id, RemoteUser remoteUser)
		{
			string path = "users?password={password}&password_question={password_question}&password_answer={password_answer}&project_id={project_id}&project_role_id={project_role_id}";

            //Create the request
            RestRequest request = new RestRequest("User_Create");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "POST";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "password", password);
			AddRequestParameter(request, "password_question", password_question);
			AddRequestParameter(request, "password_answer", password_answer);
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "project_role_id", project_role_id);
			

			//Specify the body (if appropriate)
			if (remoteUser == null)
			{
				request.Body = "";
			}
			else
			{
				JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
				request.Body = JsonConvert.SerializeObject(remoteUser, settings);
				Common.Logger.LogTraceEvent("User_Create", request.Body);
			}

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteUser responseObj = JsonConvert.DeserializeObject<RemoteUser>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("User_Create", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteUser responseObj = JsonConvert.DeserializeObject<RemoteUser>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteUser User_Retrieve(RemoteCredentials credentials)
		{
			string path = "users";

            //Create the request
            RestRequest request = new RestRequest("User_Retrieve");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteUser responseObj = JsonConvert.DeserializeObject<RemoteUser>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("User_Retrieve", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteUser responseObj = JsonConvert.DeserializeObject<RemoteUser>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteUser User_RetrieveById(RemoteCredentials credentials, object user_id)
		{
			string path = "users/{user_id}";

            //Create the request
            RestRequest request = new RestRequest("User_RetrieveById");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "user_id", user_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteUser responseObj = JsonConvert.DeserializeObject<RemoteUser>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("User_RetrieveById", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteUser responseObj = JsonConvert.DeserializeObject<RemoteUser>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public RemoteUser User_RetrieveByUserName(RemoteCredentials credentials, object user_name)
		{
			string path = "users/usernames/{user_name}";

            //Create the request
            RestRequest request = new RestRequest("User_RetrieveByUserName");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "GET";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "user_name", user_name);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteUser responseObj = JsonConvert.DeserializeObject<RemoteUser>("", settings);
					return responseObj;				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("User_RetrieveByUserName", response.Body);
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteUser responseObj = JsonConvert.DeserializeObject<RemoteUser>(response.Body, settings);

					return responseObj;
				}
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		public void User_Delete(RemoteCredentials credentials, object user_id)
		{
			string path = "users/{user_id}";

            //Create the request
            RestRequest request = new RestRequest("User_Delete");
			if (credentials != null)
			{
				request.Credential = new System.Net.NetworkCredential();
				request.Credential.UserName = credentials.UserName;
				request.Credential.Password = credentials.ApiKey;
			}
            request.Method = "DELETE";
            request.Url = baseUrl + "/" + path;
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

			//Map the parameters
			AddRequestParameter(request, "user_id", user_id);
			

			//Specify the body (if appropriate)
			request.Body = "";

			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {
            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

	}

	#region Data Objects

	public class RemoteDocument
	{
			public System.Nullable<System.Int32> AttachmentId { get; set; }
			public System.Int32 AttachmentTypeId { get; set; }
			public System.Nullable<System.Int32> ProjectAttachmentTypeId { get; set; }
			public System.Nullable<System.Int32> ProjectAttachmentFolderId { get; set; }
			public System.Nullable<System.Int32> ArtifactTypeId { get; set; }
			public System.Nullable<System.Int32> ArtifactId { get; set; }
			public System.Nullable<System.Int32> AuthorId { get; set; }
			public System.Nullable<System.Int32> EditorId { get; set; }
			public System.String FilenameOrUrl { get; set; }
			public System.String Description { get; set; }
			public System.DateTime UploadDate { get; set; }
			public System.DateTime EditedDate { get; set; }
			public System.Int32 Size { get; set; }
			public System.String CurrentVersion { get; set; }
			public System.String Tags { get; set; }
			public System.Collections.Generic.List<RemoteDocumentVersion> Versions { get; set; }
			public System.String ProjectAttachmentTypeName { get; set; }
			public System.String AttachmentTypeName { get; set; }
			public System.String AuthorName { get; set; }
			public System.String EditorName { get; set; }
	}

	public class RemoteAssociation
	{
			public System.Nullable<System.Int32> ArtifactLinkId { get; set; }
			public System.Int32 SourceArtifactId { get; set; }
			public System.Int32 SourceArtifactTypeId { get; set; }
			public System.Int32 DestArtifactId { get; set; }
			public System.Int32 DestArtifactTypeId { get; set; }
			public System.Nullable<System.Int32> CreatorId { get; set; }
			public System.String Comment { get; set; }
			public System.Nullable<System.DateTime> CreationDate { get; set; }
			public System.String DestArtifactName { get; set; }
			public System.String DestArtifactTypeName { get; set; }
			public System.String CreatorName { get; set; }
	}

	public class RemoteAutomationEngine
	{
			public System.Nullable<System.Int32> AutomationEngineId { get; set; }
			public System.String Name { get; set; }
			public System.String Token { get; set; }
			public System.String Description { get; set; }
			public System.Boolean Active { get; set; }
	}

	public class RemoteAutomationHost
	{
			public System.Nullable<System.Int32> AutomationHostId { get; set; }
			public System.String Name { get; set; }
			public System.String Token { get; set; }
			public System.String Description { get; set; }
			public System.DateTime LastUpdateDate { get; set; }
			public System.Boolean Active { get; set; }
			public System.Nullable<System.DateTime> LastContactDate { get; set; }
			public System.Nullable<System.Int32> ProjectId { get; set; }
			public System.Int32 ArtifactTypeId { get; set; }
			public System.DateTime ConcurrencyDate { get; set; }
			public System.Collections.Generic.List<RemoteArtifactCustomProperty> CustomProperties { get; set; }
	}

	public class RemoteFilter
	{
			public System.String PropertyName { get; set; }
			public System.Nullable<System.Int32> IntValue { get; set; }
			public System.String StringValue { get; set; }
			public MultiValueFilter MultiValue { get; set; }
			public DateRange DateRangeValue { get; set; }
	}

	public class RemoteCustomList
	{
			public System.Nullable<System.Int32> CustomPropertyListId { get; set; }
			public System.Int32 ProjectId { get; set; }
			public System.String Name { get; set; }
			public System.Boolean Active { get; set; }
			public System.Boolean SortedOnValue { get; set; }
			public System.Collections.Generic.List<RemoteCustomListValue> Values { get; set; }
	}

	public class RemoteCustomListValue
	{
			public System.Nullable<System.Int32> CustomPropertyValueId { get; set; }
			public System.Int32 CustomPropertyListId { get; set; }
			public System.String Name { get; set; }
	}

	public class RemoteCustomProperty
	{
			public System.Nullable<System.Int32> CustomPropertyId { get; set; }
			public System.Int32 ProjectId { get; set; }
			public System.Int32 ArtifactTypeId { get; set; }
			public System.String Name { get; set; }
			public RemoteCustomList CustomList { get; set; }
			public System.String CustomPropertyFieldName { get; set; }
			public System.Int32 CustomPropertyTypeId { get; set; }
			public System.String CustomPropertyTypeName { get; set; }
			public System.Boolean IsDeleted { get; set; }
			public System.Int32 PropertyNumber { get; set; }
			public System.String SystemDataType { get; set; }
			public System.Collections.Generic.List<RemoteCustomPropertyOption> Options { get; set; }
	}

	public class RemoteDataMapping
	{
			public System.Nullable<System.Int32> ProjectId { get; set; }
			public System.Int32 InternalId { get; set; }
			public System.String ExternalKey { get; set; }
			public System.Boolean Primary { get; set; }
	}

	public class RemoteDataSyncSystem
	{
			public System.Int32 DataSyncSystemId { get; set; }
			public System.Int32 DataSyncStatusId { get; set; }
			public System.String Name { get; set; }
			public System.String Description { get; set; }
			public System.String ConnectionString { get; set; }
			public System.String Login { get; set; }
			public System.String Password { get; set; }
			public System.Int32 TimeOffsetHours { get; set; }
			public System.Nullable<System.DateTime> LastSyncDate { get; set; }
			public System.String Custom01 { get; set; }
			public System.String Custom02 { get; set; }
			public System.String Custom03 { get; set; }
			public System.String Custom04 { get; set; }
			public System.String Custom05 { get; set; }
			public System.Boolean AutoMapUsers { get; set; }
			public System.String DataSyncStatusName { get; set; }
	}

	public class RemoteEvent
	{
			public System.Int32 EventLogEntryType { get; set; }
			public System.String Message { get; set; }
			public System.String Details { get; set; }
	}

	public class RemoteDocumentFolder
	{
			public System.Nullable<System.Int32> ProjectAttachmentFolderId { get; set; }
			public System.Int32 ProjectId { get; set; }
			public System.Nullable<System.Int32> ParentProjectAttachmentFolderId { get; set; }
			public System.String Name { get; set; }
			public System.String IndentLevel { get; set; }
	}

	public class RemoteDocumentType
	{
			public System.Nullable<System.Int32> ProjectAttachmentTypeId { get; set; }
			public System.Int32 ProjectId { get; set; }
			public System.String Name { get; set; }
			public System.String Description { get; set; }
			public System.Boolean Active { get; set; }
			public System.Boolean Default { get; set; }
	}

	public class RemoteIncident
	{
			public System.Nullable<System.Int32> IncidentId { get; set; }
			public System.Nullable<System.Int32> PriorityId { get; set; }
			public System.Nullable<System.Int32> SeverityId { get; set; }
			public System.Nullable<System.Int32> IncidentStatusId { get; set; }
			public System.Nullable<System.Int32> IncidentTypeId { get; set; }
			public System.Nullable<System.Int32> OpenerId { get; set; }
			public System.Nullable<System.Int32> OwnerId { get; set; }
			public System.Nullable<System.Int32> TestRunStepId { get; set; }
			public System.Nullable<System.Int32> DetectedReleaseId { get; set; }
			public System.Nullable<System.Int32> ResolvedReleaseId { get; set; }
			public System.Nullable<System.Int32> VerifiedReleaseId { get; set; }
			public System.String Name { get; set; }
			public System.String Description { get; set; }
			public System.Nullable<System.DateTime> CreationDate { get; set; }
			public System.Nullable<System.DateTime> StartDate { get; set; }
			public System.Nullable<System.DateTime> ClosedDate { get; set; }
			public System.Int32 CompletionPercent { get; set; }
			public System.Nullable<System.Int32> EstimatedEffort { get; set; }
			public System.Nullable<System.Int32> ActualEffort { get; set; }
			public System.Nullable<System.Int32> RemainingEffort { get; set; }
			public System.Nullable<System.Int32> ProjectedEffort { get; set; }
			public System.DateTime LastUpdateDate { get; set; }
			public System.String PriorityName { get; set; }
			public System.String SeverityName { get; set; }
			public System.String IncidentStatusName { get; set; }
			public System.String IncidentTypeName { get; set; }
			public System.String OpenerName { get; set; }
			public System.String OwnerName { get; set; }
			public System.String ProjectName { get; set; }
			public System.String DetectedReleaseVersionNumber { get; set; }
			public System.String ResolvedReleaseVersionNumber { get; set; }
			public System.String VerifiedReleaseVersionNumber { get; set; }
			public System.Nullable<System.Boolean> IncidentStatusOpenStatus { get; set; }
			public System.Nullable<System.Int32> FixedBuildId { get; set; }
			public System.String FixedBuildName { get; set; }
			public System.Nullable<System.Int32> ProjectId { get; set; }
			public System.Int32 ArtifactTypeId { get; set; }
			public System.DateTime ConcurrencyDate { get; set; }
			public System.Collections.Generic.List<RemoteArtifactCustomProperty> CustomProperties { get; set; }
	}

	public class RemoteComment
	{
			public System.Nullable<System.Int32> CommentId { get; set; }
			public System.Int32 ArtifactId { get; set; }
			public System.Nullable<System.Int32> UserId { get; set; }
			public System.String UserName { get; set; }
			public System.String Text { get; set; }
			public System.Nullable<System.DateTime> CreationDate { get; set; }
			public System.Boolean IsDeleted { get; set; }
	}

	public class RemoteIncidentPriority
	{
			public System.Nullable<System.Int32> PriorityId { get; set; }
			public System.String Name { get; set; }
			public System.Boolean Active { get; set; }
			public System.String Color { get; set; }
	}

	public class RemoteIncidentSeverity
	{
			public System.Nullable<System.Int32> SeverityId { get; set; }
			public System.String Name { get; set; }
			public System.Boolean Active { get; set; }
			public System.String Color { get; set; }
	}

	public class RemoteIncidentStatus
	{
			public System.Nullable<System.Int32> IncidentStatusId { get; set; }
			public System.String Name { get; set; }
			public System.Boolean Active { get; set; }
			public System.Boolean Open { get; set; }
	}

	public class RemoteIncidentType
	{
			public System.Nullable<System.Int32> IncidentTypeId { get; set; }
			public System.String Name { get; set; }
			public System.Boolean Active { get; set; }
			public System.Boolean Issue { get; set; }
			public System.Boolean Risk { get; set; }
			public System.Int32 WorkflowId { get; set; }
	}

	public class RemoteWorkflowIncidentTransition
	{
			public System.Boolean ExecuteByDetector { get; set; }
			public System.Boolean ExecuteByOwner { get; set; }
			public System.Int32 IncidentStatusId_Input { get; set; }
			public System.String IncidentStatusName_Input { get; set; }
			public System.Int32 IncidentStatusId_Output { get; set; }
			public System.String IncidentStatusName_Output { get; set; }
			public System.String Name { get; set; }
			public System.Int32 WorkflowId { get; set; }
			public System.Int32 TransitionId { get; set; }
	}

	public class RemoteWorkflowIncidentFields
	{
			public System.String FieldCaption { get; set; }
			public System.String FieldName { get; set; }
			public System.Int32 FieldId { get; set; }
			public System.Int32 FieldStateId { get; set; }
	}

	public class RemoteWorkflowIncidentCustomProperties
	{
			public System.String FieldCaption { get; set; }
			public System.String FieldName { get; set; }
			public System.Int32 CustomPropertyId { get; set; }
			public System.Int32 FieldStateId { get; set; }
	}

	public class RemoteProject
	{
			public System.Nullable<System.Int32> ProjectId { get; set; }
			public System.String Name { get; set; }
			public System.String Description { get; set; }
			public System.String Website { get; set; }
			public System.DateTime CreationDate { get; set; }
			public System.Boolean Active { get; set; }
			public System.Int32 WorkingHours { get; set; }
			public System.Int32 WorkingDays { get; set; }
			public System.Int32 NonWorkingHours { get; set; }
	}

	public class RemoteProjectRole
	{
			public System.Nullable<System.Int32> ProjectRoleId { get; set; }
			public System.String Name { get; set; }
			public System.String Description { get; set; }
			public System.Boolean Active { get; set; }
			public System.Boolean Admin { get; set; }
			public System.Boolean DocumentsAdd { get; set; }
			public System.Boolean DocumentsEdit { get; set; }
			public System.Boolean DocumentsDelete { get; set; }
			public System.Boolean DiscussionsAdd { get; set; }
			public System.Boolean SourceCodeView { get; set; }
	}

	public class RemoteProjectUser
	{
			public System.Int32 ProjectId { get; set; }
			public System.Int32 ProjectRoleId { get; set; }
			public System.String ProjectRoleName { get; set; }
			public System.Nullable<System.Int32> UserId { get; set; }
			public System.String FirstName { get; set; }
			public System.String LastName { get; set; }
			public System.String MiddleInitial { get; set; }
			public System.String UserName { get; set; }
			public System.String LdapDn { get; set; }
			public System.String EmailAddress { get; set; }
			public System.Boolean Admin { get; set; }
			public System.Boolean Active { get; set; }
			public System.String Department { get; set; }
			public System.Boolean Approved { get; set; }
			public System.Boolean Locked { get; set; }
			public System.String RssToken { get; set; }
			public System.String FullName { get; set; }
	}

	public class RemoteRelease
	{
			public System.Nullable<System.Int32> ReleaseId { get; set; }
			public System.Nullable<System.Int32> CreatorId { get; set; }
			public System.String IndentLevel { get; set; }
			public System.String Name { get; set; }
			public System.String Description { get; set; }
			public System.String VersionNumber { get; set; }
			public System.DateTime CreationDate { get; set; }
			public System.DateTime LastUpdateDate { get; set; }
			public System.Boolean Summary { get; set; }
			public System.Boolean Active { get; set; }
			public System.Boolean Iteration { get; set; }
			public System.DateTime StartDate { get; set; }
			public System.DateTime EndDate { get; set; }
			public System.Int32 ResourceCount { get; set; }
			public System.Int32 DaysNonWorking { get; set; }
			public System.Nullable<System.Int32> PlannedEffort { get; set; }
			public System.Nullable<System.Int32> AvailableEffort { get; set; }
			public System.Nullable<System.Int32> TaskEstimatedEffort { get; set; }
			public System.Nullable<System.Int32> TaskActualEffort { get; set; }
			public System.Nullable<System.Int32> TaskCount { get; set; }
			public System.String CreatorName { get; set; }
			public System.String FullName { get; set; }
			public System.Nullable<System.Int32> ProjectId { get; set; }
			public System.Int32 ArtifactTypeId { get; set; }
			public System.DateTime ConcurrencyDate { get; set; }
			public System.Collections.Generic.List<RemoteArtifactCustomProperty> CustomProperties { get; set; }
	}

	public class RemoteBuild
	{
			public System.Nullable<System.Int32> BuildId { get; set; }
			public System.Int32 BuildStatusId { get; set; }
			public System.Int32 ProjectId { get; set; }
			public System.Int32 ReleaseId { get; set; }
			public System.String Name { get; set; }
			public System.String Description { get; set; }
			public System.DateTime LastUpdateDate { get; set; }
			public System.Nullable<System.DateTime> CreationDate { get; set; }
			public System.String BuildStatusName { get; set; }
			public System.Collections.Generic.List<RemoteBuildSourceCode> Revisions { get; set; }
	}

	public class RemoteReleaseTestCaseMapping
	{
			public System.Int32 ReleaseId { get; set; }
			public System.Int32 TestCaseId { get; set; }
	}

	public class RemoteTestCase
	{
			public System.Nullable<System.Int32> TestCaseId { get; set; }
			public System.String IndentLevel { get; set; }
			public System.Nullable<System.Int32> ExecutionStatusId { get; set; }
			public System.Nullable<System.Int32> AuthorId { get; set; }
			public System.Nullable<System.Int32> OwnerId { get; set; }
			public System.Nullable<System.Int32> TestCasePriorityId { get; set; }
			public System.Nullable<System.Int32> AutomationEngineId { get; set; }
			public System.Nullable<System.Int32> AutomationAttachmentId { get; set; }
			public System.String Name { get; set; }
			public System.String Description { get; set; }
			public System.DateTime CreationDate { get; set; }
			public System.DateTime LastUpdateDate { get; set; }
			public System.Nullable<System.DateTime> ExecutionDate { get; set; }
			public System.Nullable<System.Int32> EstimatedDuration { get; set; }
			public System.Boolean Folder { get; set; }
			public System.Boolean Active { get; set; }
			public System.String AuthorName { get; set; }
			public System.String OwnerName { get; set; }
			public System.String ProjectName { get; set; }
			public System.String TestCasePriorityName { get; set; }
			public System.Collections.Generic.List<RemoteTestStep> TestSteps { get; set; }
			public System.Nullable<System.Int32> ProjectId { get; set; }
			public System.Int32 ArtifactTypeId { get; set; }
			public System.DateTime ConcurrencyDate { get; set; }
			public System.Collections.Generic.List<RemoteArtifactCustomProperty> CustomProperties { get; set; }
	}

	public class RemoteRequirement
	{
			public System.Nullable<System.Int32> RequirementId { get; set; }
			public System.String IndentLevel { get; set; }
			public System.Nullable<System.Int32> StatusId { get; set; }
			public System.Nullable<System.Int32> AuthorId { get; set; }
			public System.Nullable<System.Int32> OwnerId { get; set; }
			public System.Nullable<System.Int32> ImportanceId { get; set; }
			public System.Nullable<System.Int32> ReleaseId { get; set; }
			public System.String Name { get; set; }
			public System.String Description { get; set; }
			public System.DateTime CreationDate { get; set; }
			public System.DateTime LastUpdateDate { get; set; }
			public System.Boolean Summary { get; set; }
			public System.Nullable<System.Int32> CoverageCountTotal { get; set; }
			public System.Nullable<System.Int32> CoverageCountPassed { get; set; }
			public System.Nullable<System.Int32> CoverageCountFailed { get; set; }
			public System.Nullable<System.Int32> CoverageCountCaution { get; set; }
			public System.Nullable<System.Int32> CoverageCountBlocked { get; set; }
			public System.Nullable<System.Int32> PlannedEffort { get; set; }
			public System.Nullable<System.Int32> TaskEstimatedEffort { get; set; }
			public System.Nullable<System.Int32> TaskActualEffort { get; set; }
			public System.Nullable<System.Int32> TaskCount { get; set; }
			public System.String ReleaseVersionNumber { get; set; }
			public System.String AuthorName { get; set; }
			public System.String OwnerName { get; set; }
			public System.String StatusName { get; set; }
			public System.String ImportanceName { get; set; }
			public System.String ProjectName { get; set; }
			public System.Nullable<System.Int32> ProjectId { get; set; }
			public System.Int32 ArtifactTypeId { get; set; }
			public System.DateTime ConcurrencyDate { get; set; }
			public System.Collections.Generic.List<RemoteArtifactCustomProperty> CustomProperties { get; set; }
	}

	public class RemoteRequirementTestCaseMapping
	{
			public System.Int32 RequirementId { get; set; }
			public System.Int32 TestCaseId { get; set; }
	}

	public class RemoteVersion
	{
			public System.String Version { get; set; }
			public System.Nullable<System.Int32> Patch { get; set; }
	}

	public class RemoteTask
	{
			public System.Nullable<System.Int32> TaskId { get; set; }
			public System.Int32 TaskStatusId { get; set; }
			public System.Nullable<System.Int32> RequirementId { get; set; }
			public System.Nullable<System.Int32> ReleaseId { get; set; }
			public System.Nullable<System.Int32> CreatorId { get; set; }
			public System.Nullable<System.Int32> OwnerId { get; set; }
			public System.Nullable<System.Int32> TaskPriorityId { get; set; }
			public System.String Name { get; set; }
			public System.String Description { get; set; }
			public System.DateTime CreationDate { get; set; }
			public System.DateTime LastUpdateDate { get; set; }
			public System.Nullable<System.DateTime> StartDate { get; set; }
			public System.Nullable<System.DateTime> EndDate { get; set; }
			public System.Int32 CompletionPercent { get; set; }
			public System.Nullable<System.Int32> EstimatedEffort { get; set; }
			public System.Nullable<System.Int32> ActualEffort { get; set; }
			public System.Nullable<System.Int32> RemainingEffort { get; set; }
			public System.Nullable<System.Int32> ProjectedEffort { get; set; }
			public System.String TaskStatusName { get; set; }
			public System.String OwnerName { get; set; }
			public System.String TaskPriorityName { get; set; }
			public System.String ProjectName { get; set; }
			public System.String ReleaseVersionNumber { get; set; }
			public System.String RequirementName { get; set; }
			public System.Nullable<System.Int32> ProjectId { get; set; }
			public System.Int32 ArtifactTypeId { get; set; }
			public System.DateTime ConcurrencyDate { get; set; }
			public System.Collections.Generic.List<RemoteArtifactCustomProperty> CustomProperties { get; set; }
	}

	public class RemoteTestStepParameter
	{
			public System.String Name { get; set; }
			public System.String Value { get; set; }
	}

	public class RemoteTestCaseParameter
	{
			public System.Nullable<System.Int32> TestCaseParameterId { get; set; }
			public System.Int32 TestCaseId { get; set; }
			public System.String Name { get; set; }
			public System.String DefaultValue { get; set; }
	}

	public class RemoteManualTestRun
	{
			public System.Collections.Generic.List<RemoteTestRunStep> TestRunSteps { get; set; }
			public System.Nullable<System.Int32> TestRunId { get; set; }
			public System.String Name { get; set; }
			public System.Int32 TestCaseId { get; set; }
			public System.Int32 TestRunTypeId { get; set; }
			public System.Nullable<System.Int32> TesterId { get; set; }
			public System.Int32 ExecutionStatusId { get; set; }
			public System.Nullable<System.Int32> ReleaseId { get; set; }
			public System.Nullable<System.Int32> TestSetId { get; set; }
			public System.Nullable<System.Int32> TestSetTestCaseId { get; set; }
			public System.DateTime StartDate { get; set; }
			public System.Nullable<System.DateTime> EndDate { get; set; }
			public System.Nullable<System.Int32> BuildId { get; set; }
			public System.Nullable<System.Int32> EstimatedDuration { get; set; }
			public System.Nullable<System.Int32> ActualDuration { get; set; }
			public System.Nullable<System.Int32> ProjectId { get; set; }
			public System.Int32 ArtifactTypeId { get; set; }
			public System.DateTime ConcurrencyDate { get; set; }
			public System.Collections.Generic.List<RemoteArtifactCustomProperty> CustomProperties { get; set; }
	}

	public class RemoteTestRun
	{
			public System.Nullable<System.Int32> TestRunId { get; set; }
			public System.String Name { get; set; }
			public System.Int32 TestCaseId { get; set; }
			public System.Int32 TestRunTypeId { get; set; }
			public System.Nullable<System.Int32> TesterId { get; set; }
			public System.Int32 ExecutionStatusId { get; set; }
			public System.Nullable<System.Int32> ReleaseId { get; set; }
			public System.Nullable<System.Int32> TestSetId { get; set; }
			public System.Nullable<System.Int32> TestSetTestCaseId { get; set; }
			public System.DateTime StartDate { get; set; }
			public System.Nullable<System.DateTime> EndDate { get; set; }
			public System.Nullable<System.Int32> BuildId { get; set; }
			public System.Nullable<System.Int32> EstimatedDuration { get; set; }
			public System.Nullable<System.Int32> ActualDuration { get; set; }
			public System.Nullable<System.Int32> ProjectId { get; set; }
			public System.Int32 ArtifactTypeId { get; set; }
			public System.DateTime ConcurrencyDate { get; set; }
			public System.Collections.Generic.List<RemoteArtifactCustomProperty> CustomProperties { get; set; }
	}

	public class RemoteAutomatedTestRun
	{
			public System.Int32 TestRunFormatId { get; set; }
			public System.String RunnerName { get; set; }
			public System.String RunnerTestName { get; set; }
			public System.Nullable<System.Int32> RunnerAssertCount { get; set; }
			public System.String RunnerMessage { get; set; }
			public System.String RunnerStackTrace { get; set; }
			public System.Nullable<System.Int32> AutomationHostId { get; set; }
			public System.Nullable<System.Int32> AutomationEngineId { get; set; }
			public System.String AutomationEngineToken { get; set; }
			public System.Nullable<System.Int32> AutomationAttachmentId { get; set; }
			public System.Collections.Generic.List<RemoteTestSetTestCaseParameter> Parameters { get; set; }
			public System.Nullable<System.DateTime> ScheduledDate { get; set; }
			public System.Collections.Generic.List<RemoteTestRunStep> TestRunSteps { get; set; }
			public System.Nullable<System.Int32> TestRunId { get; set; }
			public System.String Name { get; set; }
			public System.Int32 TestCaseId { get; set; }
			public System.Int32 TestRunTypeId { get; set; }
			public System.Nullable<System.Int32> TesterId { get; set; }
			public System.Int32 ExecutionStatusId { get; set; }
			public System.Nullable<System.Int32> ReleaseId { get; set; }
			public System.Nullable<System.Int32> TestSetId { get; set; }
			public System.Nullable<System.Int32> TestSetTestCaseId { get; set; }
			public System.DateTime StartDate { get; set; }
			public System.Nullable<System.DateTime> EndDate { get; set; }
			public System.Nullable<System.Int32> BuildId { get; set; }
			public System.Nullable<System.Int32> EstimatedDuration { get; set; }
			public System.Nullable<System.Int32> ActualDuration { get; set; }
			public System.Nullable<System.Int32> ProjectId { get; set; }
			public System.Int32 ArtifactTypeId { get; set; }
			public System.DateTime ConcurrencyDate { get; set; }
			public System.Collections.Generic.List<RemoteArtifactCustomProperty> CustomProperties { get; set; }
	}

	public class RemoteTestSet
	{
			public System.Nullable<System.Int32> TestSetId { get; set; }
			public System.String IndentLevel { get; set; }
			public System.Int32 TestSetStatusId { get; set; }
			public System.Nullable<System.Int32> CreatorId { get; set; }
			public System.Nullable<System.Int32> OwnerId { get; set; }
			public System.Nullable<System.Int32> ReleaseId { get; set; }
			public System.Nullable<System.Int32> AutomationHostId { get; set; }
			public System.Nullable<System.Int32> TestRunTypeId { get; set; }
			public System.Nullable<System.Int32> RecurrenceId { get; set; }
			public System.String Name { get; set; }
			public System.String Description { get; set; }
			public System.DateTime CreationDate { get; set; }
			public System.DateTime LastUpdateDate { get; set; }
			public System.Nullable<System.DateTime> PlannedDate { get; set; }
			public System.Nullable<System.DateTime> ExecutionDate { get; set; }
			public System.Boolean Folder { get; set; }
			public System.Nullable<System.Int32> CountPassed { get; set; }
			public System.Nullable<System.Int32> CountFailed { get; set; }
			public System.Nullable<System.Int32> CountCaution { get; set; }
			public System.Nullable<System.Int32> CountBlocked { get; set; }
			public System.Nullable<System.Int32> CountNotRun { get; set; }
			public System.Nullable<System.Int32> CountNotApplicable { get; set; }
			public System.String CreatorName { get; set; }
			public System.String OwnerName { get; set; }
			public System.String ProjectName { get; set; }
			public System.String TestSetStatusName { get; set; }
			public System.String ReleaseVersionNumber { get; set; }
			public System.String RecurrenceName { get; set; }
			public System.Nullable<System.Int32> ProjectId { get; set; }
			public System.Int32 ArtifactTypeId { get; set; }
			public System.DateTime ConcurrencyDate { get; set; }
			public System.Collections.Generic.List<RemoteArtifactCustomProperty> CustomProperties { get; set; }
	}

	public class RemoteTestSetTestCaseMapping
	{
			public System.Int32 TestSetTestCaseId { get; set; }
			public System.Int32 TestSetId { get; set; }
			public System.Int32 TestCaseId { get; set; }
			public System.Nullable<System.Int32> OwnerId { get; set; }
	}

	public class RemoteTestSetTestCaseParameter
	{
			public System.String Name { get; set; }
			public System.String Value { get; set; }
	}

	public class RemoteTestStep
	{
			public System.Nullable<System.Int32> TestStepId { get; set; }
			public System.Int32 TestCaseId { get; set; }
			public System.Nullable<System.Int32> ExecutionStatusId { get; set; }
			public System.Int32 Position { get; set; }
			public System.String Description { get; set; }
			public System.String ExpectedResult { get; set; }
			public System.String SampleData { get; set; }
			public System.Nullable<System.Int32> LinkedTestCaseId { get; set; }
			public System.DateTime LastUpdateDate { get; set; }
			public System.Nullable<System.Int32> ProjectId { get; set; }
			public System.Int32 ArtifactTypeId { get; set; }
			public System.DateTime ConcurrencyDate { get; set; }
			public System.Collections.Generic.List<RemoteArtifactCustomProperty> CustomProperties { get; set; }
	}

	public class RemoteUser
	{
			public System.Nullable<System.Int32> UserId { get; set; }
			public System.String FirstName { get; set; }
			public System.String LastName { get; set; }
			public System.String MiddleInitial { get; set; }
			public System.String UserName { get; set; }
			public System.String LdapDn { get; set; }
			public System.String EmailAddress { get; set; }
			public System.Boolean Admin { get; set; }
			public System.Boolean Active { get; set; }
			public System.String Department { get; set; }
			public System.Boolean Approved { get; set; }
			public System.Boolean Locked { get; set; }
			public System.String RssToken { get; set; }
			public System.String FullName { get; set; }
	}


	#endregion
}