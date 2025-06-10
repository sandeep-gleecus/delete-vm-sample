









using System;
using System.Collections.Generic;
using System.Linq;

using Inflectra.SpiraTest.ApiTestSuite.Rest_Code_Generation;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Inflectra.SpiraTest.ApiTestSuite.Rest_Code_Generation.SpiraRestService60
{

    /// <summary>
    /// The auto-generated wrapper for calling Spira REST web services
    /// </summary>
	public partial class SpiraRestClient60
	{
		protected string baseUrl = "http://localhost/Spira/Services/v6_0/RestService.svc";


		public RemoteDocument[] Document_RetrieveForArtifact2(RemoteCredentials credentials, object project_id, object artifact_type_id, object artifact_id, object sort_by, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/artifact-types/{artifact_type_id}/artifacts/{artifact_id}/documents/search?sort_by={sort_by}";

            //Create the request
            RestRequest request = new RestRequest("Document_RetrieveForArtifact2");
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
			AddRequestParameter(request, "sort_by", sort_by);
			

			//Specify the body (if appropriate)

			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteFilters);
				Common.Logger.LogTraceEvent("Document_RetrieveForArtifact2", request.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_RetrieveForArtifact2", response.Body);
					RemoteDocument[] responseObj = JsonConvert.DeserializeObject<RemoteDocument[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteDocument[] Document_RetrieveForArtifact1(RemoteCredentials credentials, object project_id, object artifact_type_id, object artifact_id)
		{
			string path = "projects/{project_id}/artifact-types/{artifact_type_id}/artifacts/{artifact_id}/documents";

            //Create the request
            RestRequest request = new RestRequest("Document_RetrieveForArtifact1");
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_RetrieveForArtifact1", response.Body);
					RemoteDocument[] responseObj = JsonConvert.DeserializeObject<RemoteDocument[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteAssociation);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Association_Create", response.Body);
					RemoteAssociation responseObj = JsonConvert.DeserializeObject<RemoteAssociation>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteAssociation);
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


		public RemoteAssociation[] Association_RetrieveForArtifact1(RemoteCredentials credentials, object project_id, object artifact_type_id, object artifact_id)
		{
			string path = "projects/{project_id}/associations/{artifact_type_id}/{artifact_id}";

            //Create the request
            RestRequest request = new RestRequest("Association_RetrieveForArtifact1");
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Association_RetrieveForArtifact1", response.Body);
					RemoteAssociation[] responseObj = JsonConvert.DeserializeObject<RemoteAssociation[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteAssociation[] Association_RetrieveForArtifact2(RemoteCredentials credentials, object project_id, object artifact_type_id, object artifact_id, object sort_by, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/associations/{artifact_type_id}/{artifact_id}/search?sort_by={sort_by}";

            //Create the request
            RestRequest request = new RestRequest("Association_RetrieveForArtifact2");
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
			AddRequestParameter(request, "sort_by", sort_by);
			

			//Specify the body (if appropriate)

			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteFilters);
				Common.Logger.LogTraceEvent("Association_RetrieveForArtifact2", request.Body);

			}


			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {

				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteAssociation[] responseObj = JsonConvert.DeserializeObject<RemoteAssociation[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Association_RetrieveForArtifact2", response.Body);
					RemoteAssociation[] responseObj = JsonConvert.DeserializeObject<RemoteAssociation[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void Association_Delete(RemoteCredentials credentials, object project_id, object artifact_link_id)
		{
			string path = "projects/{project_id}/associations/{artifact_link_id}";

            //Create the request
            RestRequest request = new RestRequest("Association_Delete");
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
			AddRequestParameter(request, "artifact_link_id", artifact_link_id);
			

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
				request.Body = JsonConvert.SerializeObject(dateRange);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_CreateForAutomationHost", response.Body);
					RemoteAutomatedTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteAutomatedTestRun[]>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_CreateForAutomatedTestSet", response.Body);
					RemoteAutomatedTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteAutomatedTestRun[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteTestRun);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_RecordAutomated1", response.Body);
					RemoteAutomatedTestRun responseObj = JsonConvert.DeserializeObject<RemoteAutomatedTestRun>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteTestRuns);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_RecordAutomated2", response.Body);
					RemoteAutomatedTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteAutomatedTestRun[]>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_RetrieveAutomatedById", response.Body);
					RemoteAutomatedTestRun responseObj = JsonConvert.DeserializeObject<RemoteAutomatedTestRun>(response.Body);

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
			string path = "projects/{project_id}/test-runs/automated?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}";

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_RetrieveAutomated1", response.Body);
					RemoteAutomatedTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteAutomatedTestRun[]>(response.Body);

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
			string path = "projects/{project_id}/test-runs/search/automated?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}";

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
				request.Body = JsonConvert.SerializeObject(remoteFilters);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_RetrieveAutomated2", response.Body);
					RemoteAutomatedTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteAutomatedTestRun[]>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("AutomationEngine_RetrieveByToken", response.Body);
					RemoteAutomationEngine responseObj = JsonConvert.DeserializeObject<RemoteAutomationEngine>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("AutomationEngine_Retrieve", response.Body);
					RemoteAutomationEngine[] responseObj = JsonConvert.DeserializeObject<RemoteAutomationEngine[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteEngine);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("AutomationEngine_Create", response.Body);
					RemoteAutomationEngine responseObj = JsonConvert.DeserializeObject<RemoteAutomationEngine>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("AutomationEngine_RetrieveById", response.Body);
					RemoteAutomationEngine responseObj = JsonConvert.DeserializeObject<RemoteAutomationEngine>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void AutomationEngine_Update(RemoteCredentials credentials, RemoteAutomationEngine remoteEngine)
		{
			string path = "automation-engines";

            //Create the request
            RestRequest request = new RestRequest("AutomationEngine_Update");
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
			

			//Specify the body (if appropriate)

			if (remoteEngine == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteEngine);
				Common.Logger.LogTraceEvent("AutomationEngine_Update", request.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("AutomationHost_Retrieve1", response.Body);
					RemoteAutomationHost[] responseObj = JsonConvert.DeserializeObject<RemoteAutomationHost[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteFilters);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("AutomationHost_Retrieve2", response.Body);
					RemoteAutomationHost[] responseObj = JsonConvert.DeserializeObject<RemoteAutomationHost[]>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("AutomationHost_RetrieveById", response.Body);
					RemoteAutomationHost responseObj = JsonConvert.DeserializeObject<RemoteAutomationHost>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("AutomationHost_RetrieveByToken", response.Body);
					RemoteAutomationHost responseObj = JsonConvert.DeserializeObject<RemoteAutomationHost>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteAutomationHost);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("AutomationHost_Create", response.Body);
					RemoteAutomationHost responseObj = JsonConvert.DeserializeObject<RemoteAutomationHost>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteAutomationHost);
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


		public RemoteComponent[] Component_Retrieve(RemoteCredentials credentials, object project_id, object active_only, object include_deleted)
		{
			string path = "projects/{project_id}/components?active_only={active_only}&include_deleted={include_deleted}";

            //Create the request
            RestRequest request = new RestRequest("Component_Retrieve");
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
			AddRequestParameter(request, "include_deleted", include_deleted);
			

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
					RemoteComponent[] responseObj = JsonConvert.DeserializeObject<RemoteComponent[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Component_Retrieve", response.Body);
					RemoteComponent[] responseObj = JsonConvert.DeserializeObject<RemoteComponent[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteComponent Component_RetrieveById(RemoteCredentials credentials, object project_id, object component_id)
		{
			string path = "projects/{project_id}/components/{component_id}";

            //Create the request
            RestRequest request = new RestRequest("Component_RetrieveById");
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
			AddRequestParameter(request, "component_id", component_id);
			

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
					RemoteComponent responseObj = JsonConvert.DeserializeObject<RemoteComponent>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Component_RetrieveById", response.Body);
					RemoteComponent responseObj = JsonConvert.DeserializeObject<RemoteComponent>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteComponent Component_Create(RemoteCredentials credentials, object project_id, RemoteComponent remoteComponent)
		{
			string path = "projects/{project_id}/components";

            //Create the request
            RestRequest request = new RestRequest("Component_Create");
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

			if (remoteComponent == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteComponent);
				Common.Logger.LogTraceEvent("Component_Create", request.Body);

			}


			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {

				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteComponent responseObj = JsonConvert.DeserializeObject<RemoteComponent>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Component_Create", response.Body);
					RemoteComponent responseObj = JsonConvert.DeserializeObject<RemoteComponent>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void Component_Update(RemoteCredentials credentials, object project_id, object component_id, RemoteComponent remoteComponent)
		{
			string path = "projects/{project_id}/components/{component_id}";

            //Create the request
            RestRequest request = new RestRequest("Component_Update");
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
			AddRequestParameter(request, "component_id", component_id);
			

			//Specify the body (if appropriate)

			if (remoteComponent == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteComponent);
				Common.Logger.LogTraceEvent("Component_Update", request.Body);

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


		public void Component_Delete(RemoteCredentials credentials, object project_id, object component_id)
		{
			string path = "projects/{project_id}/components/{component_id}";

            //Create the request
            RestRequest request = new RestRequest("Component_Delete");
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
			AddRequestParameter(request, "component_id", component_id);
			

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


		public void Component_Undelete(RemoteCredentials credentials, object project_id, object component_id)
		{
			string path = "projects/{project_id}/components/{component_id}/undelete";

            //Create the request
            RestRequest request = new RestRequest("Component_Undelete");
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
			AddRequestParameter(request, "component_id", component_id);
			

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


		public RemoteCustomList CustomProperty_RetrieveCustomListById(RemoteCredentials credentials, object project_template_id, object custom_list_id)
		{
			string path = "project-templates/{project_template_id}/custom-lists/{custom_list_id}";

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
			AddRequestParameter(request, "project_template_id", project_template_id);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("CustomProperty_RetrieveCustomListById", response.Body);
					RemoteCustomList responseObj = JsonConvert.DeserializeObject<RemoteCustomList>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteCustomList CustomProperty_AddCustomList(RemoteCredentials credentials, object project_template_id, RemoteCustomList remoteCustomList)
		{
			string path = "project-templates/{project_template_id}/custom-lists";

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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

			//Specify the body (if appropriate)

			if (remoteCustomList == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteCustomList);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("CustomProperty_AddCustomList", response.Body);
					RemoteCustomList responseObj = JsonConvert.DeserializeObject<RemoteCustomList>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteCustomList[] CustomProperty_RetrieveCustomLists(RemoteCredentials credentials, object project_template_id)
		{
			string path = "project-templates/{project_template_id}/custom-lists";

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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("CustomProperty_RetrieveCustomLists", response.Body);
					RemoteCustomList[] responseObj = JsonConvert.DeserializeObject<RemoteCustomList[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void CustomProperty_UpdateCustomList(RemoteCredentials credentials, object project_template_id, object custom_list_id, RemoteCustomList remoteCustomList)
		{
			string path = "project-templates/{project_template_id}/custom-lists/{custom_list_id}";

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
			AddRequestParameter(request, "project_template_id", project_template_id);
			AddRequestParameter(request, "custom_list_id", custom_list_id);
			

			//Specify the body (if appropriate)

			if (remoteCustomList == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteCustomList);
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


		public RemoteCustomListValue CustomProperty_AddCustomListValue(RemoteCredentials credentials, object project_template_id, object custom_list_id, RemoteCustomListValue remoteCustomListValue)
		{
			string path = "project-templates/{project_template_id}/custom-lists/{custom_list_id}/values";

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
			AddRequestParameter(request, "project_template_id", project_template_id);
			AddRequestParameter(request, "custom_list_id", custom_list_id);
			

			//Specify the body (if appropriate)

			if (remoteCustomListValue == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteCustomListValue);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("CustomProperty_AddCustomListValue", response.Body);
					RemoteCustomListValue responseObj = JsonConvert.DeserializeObject<RemoteCustomListValue>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteCustomProperty[] CustomProperty_RetrieveForArtifactType(RemoteCredentials credentials, object project_template_id, object artifact_type_name)
		{
			string path = "project-templates/{project_template_id}/custom-properties/{artifact_type_name}";

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
			AddRequestParameter(request, "project_template_id", project_template_id);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("CustomProperty_RetrieveForArtifactType", response.Body);
					RemoteCustomProperty[] responseObj = JsonConvert.DeserializeObject<RemoteCustomProperty[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteCustomProperty CustomProperty_AddDefinition(RemoteCredentials credentials, object project_template_id, object custom_list_id, RemoteCustomProperty remoteCustomProperty)
		{
			string path = "project-templates/{project_template_id}/custom-properties?custom_list_id={custom_list_id}";

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
			AddRequestParameter(request, "project_template_id", project_template_id);
			AddRequestParameter(request, "custom_list_id", custom_list_id);
			

			//Specify the body (if appropriate)

			if (remoteCustomProperty == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteCustomProperty);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("CustomProperty_AddDefinition", response.Body);
					RemoteCustomProperty responseObj = JsonConvert.DeserializeObject<RemoteCustomProperty>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void CustomProperty_UpdateDefinition(RemoteCredentials credentials, object project_template_id, object custom_property_id, RemoteCustomProperty remoteCustomProperty)
		{
			string path = "project-templates/{project_template_id}/custom-properties/{custom_property_id}";

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
			AddRequestParameter(request, "project_template_id", project_template_id);
			AddRequestParameter(request, "custom_property_id", custom_property_id);
			

			//Specify the body (if appropriate)

			if (remoteCustomProperty == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteCustomProperty);
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


		public void CustomProperty_DeleteDefinition(RemoteCredentials credentials, object project_template_id, object custom_property_id)
		{
			string path = "project-templates/{project_template_id}/custom-properties/{custom_property_id}";

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
			AddRequestParameter(request, "project_template_id", project_template_id);
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


		public RemoteTableData Reports_RetrieveESQLQueryData(RemoteCredentials credentials, String esql_query)
		{
			string path = "query";

            //Create the request
            RestRequest request = new RestRequest("Reports_RetrieveESQLQueryData");
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

			if (esql_query == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(esql_query);
				Common.Logger.LogTraceEvent("Reports_RetrieveESQLQueryData", request.Body);

			}


			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {

				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTableData responseObj = JsonConvert.DeserializeObject<RemoteTableData>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Reports_RetrieveESQLQueryData", response.Body);
					RemoteTableData responseObj = JsonConvert.DeserializeObject<RemoteTableData>(response.Body);

					return responseObj;
				}

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("DataMapping_RetrieveArtifactMappings", response.Body);
					RemoteDataMapping[] responseObj = JsonConvert.DeserializeObject<RemoteDataMapping[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteProjectArtifact[] DataMapping_SearchArtifactMappings(RemoteCredentials credentials, object data_sync_system_id, object artifact_type_id, String externalKey)
		{
			string path = "data-mappings/{data_sync_system_id}/artifacts/{artifact_type_id}/search";

            //Create the request
            RestRequest request = new RestRequest("DataMapping_SearchArtifactMappings");
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
			AddRequestParameter(request, "artifact_type_id", artifact_type_id);
			

			//Specify the body (if appropriate)

			if (externalKey == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(externalKey);
				Common.Logger.LogTraceEvent("DataMapping_SearchArtifactMappings", request.Body);

			}


			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {

				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteProjectArtifact[] responseObj = JsonConvert.DeserializeObject<RemoteProjectArtifact[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("DataMapping_SearchArtifactMappings", response.Body);
					RemoteProjectArtifact[] responseObj = JsonConvert.DeserializeObject<RemoteProjectArtifact[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteDataMappings);
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
				request.Body = JsonConvert.SerializeObject(remoteDataMappings);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("DataMapping_RetrieveCustomPropertyMapping", response.Body);
					RemoteDataMapping responseObj = JsonConvert.DeserializeObject<RemoteDataMapping>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("DataMapping_RetrieveCustomPropertyValueMappings", response.Body);
					RemoteDataMapping[] responseObj = JsonConvert.DeserializeObject<RemoteDataMapping[]>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("DataMapping_RetrieveFieldValueMappings", response.Body);
					RemoteDataMapping[] responseObj = JsonConvert.DeserializeObject<RemoteDataMapping[]>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("DataMapping_RetrieveProjectMappings", response.Body);
					RemoteDataMapping[] responseObj = JsonConvert.DeserializeObject<RemoteDataMapping[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteDataMappings);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("DataMapping_RetrieveUserMappings", response.Body);
					RemoteDataMapping[] responseObj = JsonConvert.DeserializeObject<RemoteDataMapping[]>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("DataSyncSystem_Retrieve", response.Body);
					RemoteDataSyncSystem[] responseObj = JsonConvert.DeserializeObject<RemoteDataSyncSystem[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteDataSyncSystem DataSyncSystem_RetrieveById(RemoteCredentials credentials, object data_sync_system_id)
		{
			string path = "data-syncs/{data_sync_system_id}";

            //Create the request
            RestRequest request = new RestRequest("DataSyncSystem_RetrieveById");
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
					RemoteDataSyncSystem responseObj = JsonConvert.DeserializeObject<RemoteDataSyncSystem>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("DataSyncSystem_RetrieveById", response.Body);
					RemoteDataSyncSystem responseObj = JsonConvert.DeserializeObject<RemoteDataSyncSystem>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteDataSyncSystem DataSyncSystem_Create(RemoteCredentials credentials, RemoteDataSyncSystem remoteDataSyncSystem)
		{
			string path = "data-syncs";

            //Create the request
            RestRequest request = new RestRequest("DataSyncSystem_Create");
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

			if (remoteDataSyncSystem == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteDataSyncSystem);
				Common.Logger.LogTraceEvent("DataSyncSystem_Create", request.Body);

			}


			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {

				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDataSyncSystem responseObj = JsonConvert.DeserializeObject<RemoteDataSyncSystem>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("DataSyncSystem_Create", response.Body);
					RemoteDataSyncSystem responseObj = JsonConvert.DeserializeObject<RemoteDataSyncSystem>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void DataSyncSystem_Update(RemoteCredentials credentials, object data_sync_system_id, RemoteDataSyncSystem remoteDataSyncSystem)
		{
			string path = "data-syncs/{data_sync_system_id}";

            //Create the request
            RestRequest request = new RestRequest("DataSyncSystem_Update");
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
			AddRequestParameter(request, "data_sync_system_id", data_sync_system_id);
			

			//Specify the body (if appropriate)

			if (remoteDataSyncSystem == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteDataSyncSystem);
				Common.Logger.LogTraceEvent("DataSyncSystem_Update", request.Body);

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
				request.Body = JsonConvert.SerializeObject(lastRunDate);
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
				request.Body = JsonConvert.SerializeObject(lastRunDate);
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
				request.Body = JsonConvert.SerializeObject(remoteEvent);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_OpenFile", response.Body);
					Byte[] responseObj = JsonConvert.DeserializeObject<Byte[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteDocument Document_AddFile(RemoteCredentials credentials, object project_id, RemoteDocumentFile remoteDocument)
		{
			string path = "projects/{project_id}/documents/file";

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
			

			//Specify the body (if appropriate)

			if (remoteDocument == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteDocument);
				Common.Logger.LogTraceEvent("Document_AddFile", request.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_AddFile", response.Body);
					RemoteDocument responseObj = JsonConvert.DeserializeObject<RemoteDocument>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteDocument Document_AddUrl(RemoteCredentials credentials, object project_id, RemoteDocument remoteDocument)
		{
			string path = "projects/{project_id}/documents/url";

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
			

			//Specify the body (if appropriate)

			if (remoteDocument == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteDocument);
				Common.Logger.LogTraceEvent("Document_AddUrl", request.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_AddUrl", response.Body);
					RemoteDocument responseObj = JsonConvert.DeserializeObject<RemoteDocument>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_Retrieve1", response.Body);
					RemoteDocument[] responseObj = JsonConvert.DeserializeObject<RemoteDocument[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteFilters);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_Retrieve2", response.Body);
					RemoteDocument[] responseObj = JsonConvert.DeserializeObject<RemoteDocument[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteFilters);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_RetrieveForFolder", response.Body);
					RemoteDocument[] responseObj = JsonConvert.DeserializeObject<RemoteDocument[]>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_RetrieveById", response.Body);
					RemoteDocument responseObj = JsonConvert.DeserializeObject<RemoteDocument>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_RetrieveFolders", response.Body);
					RemoteDocumentFolder[] responseObj = JsonConvert.DeserializeObject<RemoteDocumentFolder[]>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_RetrieveFolderById", response.Body);
					RemoteDocumentFolder responseObj = JsonConvert.DeserializeObject<RemoteDocumentFolder>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteDocumentFolder[] Document_RetrieveFoldersByParentFolderId(RemoteCredentials credentials, object project_id, object parent_folder_id)
		{
			string path = "projects/{project_id}/document-folders/children?parent_folder_id={parent_folder_id}";

            //Create the request
            RestRequest request = new RestRequest("Document_RetrieveFoldersByParentFolderId");
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
			AddRequestParameter(request, "parent_folder_id", parent_folder_id);
			

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_RetrieveFoldersByParentFolderId", response.Body);
					RemoteDocumentFolder[] responseObj = JsonConvert.DeserializeObject<RemoteDocumentFolder[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteDocumentFolder);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_AddFolder", response.Body);
					RemoteDocumentFolder responseObj = JsonConvert.DeserializeObject<RemoteDocumentFolder>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteDocumentFolder);
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


		public RemoteDocumentType[] Document_RetrieveTypes(RemoteCredentials credentials, object project_template_id, object active_only)
		{
			string path = "project-templates/{project_template_id}/document-types?active_only={active_only}";

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
			AddRequestParameter(request, "project_template_id", project_template_id);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_RetrieveTypes", response.Body);
					RemoteDocumentType[] responseObj = JsonConvert.DeserializeObject<RemoteDocumentType[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteDocumentType Document_RetrieveDefaultType(RemoteCredentials credentials, object project_template_id)
		{
			string path = "project-templates/{project_template_id}/document-types/default";

            //Create the request
            RestRequest request = new RestRequest("Document_RetrieveDefaultType");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

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
					RemoteDocumentType responseObj = JsonConvert.DeserializeObject<RemoteDocumentType>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_RetrieveDefaultType", response.Body);
					RemoteDocumentType responseObj = JsonConvert.DeserializeObject<RemoteDocumentType>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteDocumentType Document_AddType(RemoteCredentials credentials, object project_template_id, RemoteDocumentType remoteDocumentType)
		{
			string path = "project-templates/{project_template_id}/document-types";

            //Create the request
            RestRequest request = new RestRequest("Document_AddType");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

			//Specify the body (if appropriate)

			if (remoteDocumentType == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteDocumentType);
				Common.Logger.LogTraceEvent("Document_AddType", request.Body);

			}


			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {

				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDocumentType responseObj = JsonConvert.DeserializeObject<RemoteDocumentType>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_AddType", response.Body);
					RemoteDocumentType responseObj = JsonConvert.DeserializeObject<RemoteDocumentType>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void Document_UpdateType(RemoteCredentials credentials, object project_template_id, RemoteDocumentType remoteDocumentType)
		{
			string path = "project-templates/{project_template_id}/document-types";

            //Create the request
            RestRequest request = new RestRequest("Document_UpdateType");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

			//Specify the body (if appropriate)

			if (remoteDocumentType == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteDocumentType);
				Common.Logger.LogTraceEvent("Document_UpdateType", request.Body);

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


		public RemoteDocumentVersion Document_AddFileVersion(RemoteCredentials credentials, object project_id, object document_id, object make_current, RemoteDocumentVersionFile remoteDocumentVersion)
		{
			string path = "projects/{project_id}/documents/{document_id}/versions/file?make_current={make_current}";

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
			AddRequestParameter(request, "make_current", make_current);
			

			//Specify the body (if appropriate)

			if (remoteDocumentVersion == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteDocumentVersion);
				Common.Logger.LogTraceEvent("Document_AddFileVersion", request.Body);

			}


			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {

				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDocumentVersion responseObj = JsonConvert.DeserializeObject<RemoteDocumentVersion>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_AddFileVersion", response.Body);
					RemoteDocumentVersion responseObj = JsonConvert.DeserializeObject<RemoteDocumentVersion>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteDocumentVersion Document_AddUrlVersion(RemoteCredentials credentials, object project_id, object document_id, object make_current, RemoteDocumentVersion remoteDocumentVersion)
		{
			string path = "projects/{project_id}/documents/{document_id}/versions/url?make_current={make_current}";

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
			AddRequestParameter(request, "make_current", make_current);
			

			//Specify the body (if appropriate)

			if (remoteDocumentVersion == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteDocumentVersion);
				Common.Logger.LogTraceEvent("Document_AddUrlVersion", request.Body);

			}


			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {

				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteDocumentVersion responseObj = JsonConvert.DeserializeObject<RemoteDocumentVersion>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_AddUrlVersion", response.Body);
					RemoteDocumentVersion responseObj = JsonConvert.DeserializeObject<RemoteDocumentVersion>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public Byte[] Document_OpenVersion(RemoteCredentials credentials, object project_id, object attachment_version_id)
		{
			string path = "projects/{project_id}/documents/versions/{attachment_version_id}/open";

            //Create the request
            RestRequest request = new RestRequest("Document_OpenVersion");
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
			AddRequestParameter(request, "attachment_version_id", attachment_version_id);
			

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Document_OpenVersion", response.Body);
					Byte[] responseObj = JsonConvert.DeserializeObject<Byte[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void Document_DeleteVersion(RemoteCredentials credentials, object project_id, object attachment_version_id)
		{
			string path = "projects/{project_id}/documents/versions/{attachment_version_id}";

            //Create the request
            RestRequest request = new RestRequest("Document_DeleteVersion");
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
			AddRequestParameter(request, "attachment_version_id", attachment_version_id);
			

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


		public RemoteTableData Reports_RetrieveCustomGraphData(RemoteCredentials credentials, object custom_graph_id, object project_id, object project_group_id)
		{
			string path = "graphs/{custom_graph_id}/data?project_id={project_id}&project_group_id={project_group_id}";

            //Create the request
            RestRequest request = new RestRequest("Reports_RetrieveCustomGraphData");
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
			AddRequestParameter(request, "custom_graph_id", custom_graph_id);
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "project_group_id", project_group_id);
			

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
					RemoteTableData responseObj = JsonConvert.DeserializeObject<RemoteTableData>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Reports_RetrieveCustomGraphData", response.Body);
					RemoteTableData responseObj = JsonConvert.DeserializeObject<RemoteTableData>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteHistoryChange[] History_RetrieveForArtifact1(RemoteCredentials credentials, object project_id, object artifact_type_id, object artifact_id, object start_row, object number_rows, object sort_property, object sort_direction)
		{
			string path = "projects/{project_id}/artifact-types/{artifact_type_id}/artifacts/{artifact_id}/history?start_row={start_row}&number_rows={number_rows}&sort_property={sort_property}&sort_direction={sort_direction}";

            //Create the request
            RestRequest request = new RestRequest("History_RetrieveForArtifact1");
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
			AddRequestParameter(request, "start_row", start_row);
			AddRequestParameter(request, "number_rows", number_rows);
			AddRequestParameter(request, "sort_property", sort_property);
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
					RemoteHistoryChange[] responseObj = JsonConvert.DeserializeObject<RemoteHistoryChange[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("History_RetrieveForArtifact1", response.Body);
					RemoteHistoryChange[] responseObj = JsonConvert.DeserializeObject<RemoteHistoryChange[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteHistoryChange[] History_RetrieveForArtifact2(RemoteCredentials credentials, object project_id, object artifact_type_id, object artifact_id, object start_row, object number_rows, object sort_property, object sort_direction, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/artifact-types/{artifact_type_id}/artifacts/{artifact_id}/history?start_row={start_row}&number_rows={number_rows}&sort_property={sort_property}&sort_direction={sort_direction}";

            //Create the request
            RestRequest request = new RestRequest("History_RetrieveForArtifact2");
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
			AddRequestParameter(request, "start_row", start_row);
			AddRequestParameter(request, "number_rows", number_rows);
			AddRequestParameter(request, "sort_property", sort_property);
			AddRequestParameter(request, "sort_direction", sort_direction);
			

			//Specify the body (if appropriate)

			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteFilters);
				Common.Logger.LogTraceEvent("History_RetrieveForArtifact2", request.Body);

			}


			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {

				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteHistoryChange[] responseObj = JsonConvert.DeserializeObject<RemoteHistoryChange[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("History_RetrieveForArtifact2", response.Body);
					RemoteHistoryChange[] responseObj = JsonConvert.DeserializeObject<RemoteHistoryChange[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteHistoryChangeSet History_RetrieveById(RemoteCredentials credentials, object project_id, object history_change_set_id)
		{
			string path = "projects/{project_id}/history/{history_change_set_id}";

            //Create the request
            RestRequest request = new RestRequest("History_RetrieveById");
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
			AddRequestParameter(request, "history_change_set_id", history_change_set_id);
			

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
					RemoteHistoryChangeSet responseObj = JsonConvert.DeserializeObject<RemoteHistoryChangeSet>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("History_RetrieveById", response.Body);
					RemoteHistoryChangeSet responseObj = JsonConvert.DeserializeObject<RemoteHistoryChangeSet>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveById", response.Body);
					RemoteIncident responseObj = JsonConvert.DeserializeObject<RemoteIncident>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveByIdList", response.Body);
					RemoteIncident[] responseObj = JsonConvert.DeserializeObject<RemoteIncident[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteIncident[] Incident_RetrieveByTestCase(RemoteCredentials credentials, object project_id, object test_case_id, object open_only)
		{
			string path = "projects/{project_id}/incidents/search-by-test-case/{test_case_id}?open_only={open_only}";

            //Create the request
            RestRequest request = new RestRequest("Incident_RetrieveByTestCase");
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
			AddRequestParameter(request, "open_only", open_only);
			

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveByTestCase", response.Body);
					RemoteIncident[] responseObj = JsonConvert.DeserializeObject<RemoteIncident[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteIncident[] Incident_RetrieveByTestRunStep(RemoteCredentials credentials, object project_id, object test_run_step_id)
		{
			string path = "projects/{project_id}/incidents/search-by-test-run-step/{test_run_step_id}";

            //Create the request
            RestRequest request = new RestRequest("Incident_RetrieveByTestRunStep");
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
			AddRequestParameter(request, "test_run_step_id", test_run_step_id);
			

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveByTestRunStep", response.Body);
					RemoteIncident[] responseObj = JsonConvert.DeserializeObject<RemoteIncident[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteIncident[] Incident_RetrieveByTestStep(RemoteCredentials credentials, object project_id, object test_step_id)
		{
			string path = "projects/{project_id}/incidents/search-by-test-step/{test_step_id}";

            //Create the request
            RestRequest request = new RestRequest("Incident_RetrieveByTestStep");
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
					RemoteIncident[] responseObj = JsonConvert.DeserializeObject<RemoteIncident[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveByTestStep", response.Body);
					RemoteIncident[] responseObj = JsonConvert.DeserializeObject<RemoteIncident[]>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_Count", response.Body);
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteIncident[] Incident_RetrieveForOwner(RemoteCredentials credentials)
		{
			string path = "incidents";

            //Create the request
            RestRequest request = new RestRequest("Incident_RetrieveForOwner");
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
					RemoteIncident[] responseObj = JsonConvert.DeserializeObject<RemoteIncident[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveForOwner", response.Body);
					RemoteIncident[] responseObj = JsonConvert.DeserializeObject<RemoteIncident[]>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_Retrieve1", response.Body);
					RemoteIncident[] responseObj = JsonConvert.DeserializeObject<RemoteIncident[]>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_Retrieve2", response.Body);
					RemoteIncident[] responseObj = JsonConvert.DeserializeObject<RemoteIncident[]>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveNew", response.Body);
					RemoteIncident[] responseObj = JsonConvert.DeserializeObject<RemoteIncident[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteFilters);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_Retrieve3", response.Body);
					RemoteIncident[] responseObj = JsonConvert.DeserializeObject<RemoteIncident[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteIncident);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_Create", response.Body);
					RemoteIncident responseObj = JsonConvert.DeserializeObject<RemoteIncident>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteIncident);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveComments", response.Body);
					RemoteComment[] responseObj = JsonConvert.DeserializeObject<RemoteComment[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteComments);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_AddComments", response.Body);
					RemoteComment[] responseObj = JsonConvert.DeserializeObject<RemoteComment[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteIncidentPriority[] Incident_RetrievePriorities(RemoteCredentials credentials, object project_template_id)
		{
			string path = "project-templates/{project_template_id}/incidents/priorities";

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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrievePriorities", response.Body);
					RemoteIncidentPriority[] responseObj = JsonConvert.DeserializeObject<RemoteIncidentPriority[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteIncidentPriority Incident_AddPriority(RemoteCredentials credentials, object project_template_id, RemoteIncidentPriority remoteIncidentPriority)
		{
			string path = "project-templates/{project_template_id}/incidents/priorities";

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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

			//Specify the body (if appropriate)

			if (remoteIncidentPriority == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteIncidentPriority);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_AddPriority", response.Body);
					RemoteIncidentPriority responseObj = JsonConvert.DeserializeObject<RemoteIncidentPriority>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteIncidentSeverity[] Incident_RetrieveSeverities(RemoteCredentials credentials, object project_template_id)
		{
			string path = "project-templates/{project_template_id}/incidents/severities";

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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveSeverities", response.Body);
					RemoteIncidentSeverity[] responseObj = JsonConvert.DeserializeObject<RemoteIncidentSeverity[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteIncidentSeverity Incident_AddSeverity(RemoteCredentials credentials, object project_template_id, RemoteIncidentSeverity remoteIncidentSeverity)
		{
			string path = "project-templates/{project_template_id}/incidents/severities";

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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

			//Specify the body (if appropriate)

			if (remoteIncidentSeverity == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteIncidentSeverity);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_AddSeverity", response.Body);
					RemoteIncidentSeverity responseObj = JsonConvert.DeserializeObject<RemoteIncidentSeverity>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteIncidentStatus[] Incident_RetrieveStatuses(RemoteCredentials credentials, object project_template_id)
		{
			string path = "project-templates/{project_template_id}/incidents/statuses";

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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveStatuses", response.Body);
					RemoteIncidentStatus[] responseObj = JsonConvert.DeserializeObject<RemoteIncidentStatus[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteIncidentStatus Incident_AddStatus(RemoteCredentials credentials, object project_template_id, RemoteIncidentStatus remoteIncidentStatus)
		{
			string path = "project-templates/{project_template_id}/incidents/statuses";

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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

			//Specify the body (if appropriate)

			if (remoteIncidentStatus == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteIncidentStatus);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_AddStatus", response.Body);
					RemoteIncidentStatus responseObj = JsonConvert.DeserializeObject<RemoteIncidentStatus>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteIncidentStatus Incident_RetrieveDefaultStatus(RemoteCredentials credentials, object project_template_id)
		{
			string path = "project-templates/{project_template_id}/incidents/statuses/default";

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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveDefaultStatus", response.Body);
					RemoteIncidentStatus responseObj = JsonConvert.DeserializeObject<RemoteIncidentStatus>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteIncidentType Incident_AddType(RemoteCredentials credentials, object project_template_id, RemoteIncidentType remoteIncidentType)
		{
			string path = "project-templates/{project_template_id}/incidents/types";

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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

			//Specify the body (if appropriate)

			if (remoteIncidentType == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteIncidentType);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_AddType", response.Body);
					RemoteIncidentType responseObj = JsonConvert.DeserializeObject<RemoteIncidentType>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteIncidentType[] Incident_RetrieveTypes(RemoteCredentials credentials, object project_template_id)
		{
			string path = "project-templates/{project_template_id}/incidents/types";

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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveTypes", response.Body);
					RemoteIncidentType[] responseObj = JsonConvert.DeserializeObject<RemoteIncidentType[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteIncidentType Incident_RetrieveDefaultType(RemoteCredentials credentials, object project_template_id)
		{
			string path = "project-templates/{project_template_id}/incidents/types/default";

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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveDefaultType", response.Body);
					RemoteIncidentType responseObj = JsonConvert.DeserializeObject<RemoteIncidentType>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteWorkflowTransition[] Incident_RetrieveWorkflowTransitions(RemoteCredentials credentials, object project_id, object incident_type_id, object incident_status_id, object is_detector, object is_owner)
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
					RemoteWorkflowTransition[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowTransition[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveWorkflowTransitions", response.Body);
					RemoteWorkflowTransition[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowTransition[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteWorkflowField[] Incident_RetrieveWorkflowFields(RemoteCredentials credentials, object project_template_id, object incident_type_id, object incident_status_id)
		{
			string path = "project-templates/{project_template_id}/incidents/types/{incident_type_id}/workflow/fields?status_id={incident_status_id}";

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
			AddRequestParameter(request, "project_template_id", project_template_id);
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
					RemoteWorkflowField[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowField[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveWorkflowFields", response.Body);
					RemoteWorkflowField[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowField[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteWorkflowCustomProperty[] Incident_RetrieveWorkflowCustomProperties(RemoteCredentials credentials, object project_template_id, object incident_type_id, object incident_status_id)
		{
			string path = "project-templates/{project_template_id}/incidents/types/{incident_type_id}/workflow/custom-properties?status_id={incident_status_id}";

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
			AddRequestParameter(request, "project_template_id", project_template_id);
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
					RemoteWorkflowCustomProperty[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowCustomProperty[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Incident_RetrieveWorkflowCustomProperties", response.Body);
					RemoteWorkflowCustomProperty[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowCustomProperty[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(testCaseIds);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_CreateFromTestCases", response.Body);
					RemoteManualTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteManualTestRun[]>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_CreateFromTestSet", response.Body);
					RemoteManualTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteManualTestRun[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteTestRuns);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_Save", response.Body);
					RemoteManualTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteManualTestRun[]>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_RetrieveManualById", response.Body);
					RemoteManualTestRun responseObj = JsonConvert.DeserializeObject<RemoteManualTestRun>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_RetrieveManual1", response.Body);
					RemoteManualTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteManualTestRun[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteFilters);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_RetrieveManual2", response.Body);
					RemoteManualTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteManualTestRun[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteMessageInfo Message_GetInfo(RemoteCredentials credentials)
		{
			string path = "messages";

            //Create the request
            RestRequest request = new RestRequest("Message_GetInfo");
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
					RemoteMessageInfo responseObj = JsonConvert.DeserializeObject<RemoteMessageInfo>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Message_GetInfo", response.Body);
					RemoteMessageInfo responseObj = JsonConvert.DeserializeObject<RemoteMessageInfo>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public Int64 Message_PostNew(RemoteCredentials credentials, RemoteMessageIndividual remoteMessage)
		{
			string path = "messages";

            //Create the request
            RestRequest request = new RestRequest("Message_PostNew");
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

			if (remoteMessage == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteMessage);
				Common.Logger.LogTraceEvent("Message_PostNew", request.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Message_PostNew", response.Body);
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void Message_MarkAllAsRead(RemoteCredentials credentials, object sender_user_id)
		{
			string path = "messages/senders/{sender_user_id}";

            //Create the request
            RestRequest request = new RestRequest("Message_MarkAllAsRead");
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
			AddRequestParameter(request, "sender_user_id", sender_user_id);
			

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


		public RemoteUserMessage[] Message_GetUnreadMessageSenders(RemoteCredentials credentials)
		{
			string path = "messages/senders";

            //Create the request
            RestRequest request = new RestRequest("Message_GetUnreadMessageSenders");
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
					RemoteUserMessage[] responseObj = JsonConvert.DeserializeObject<RemoteUserMessage[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Message_GetUnreadMessageSenders", response.Body);
					RemoteUserMessage[] responseObj = JsonConvert.DeserializeObject<RemoteUserMessage[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteMessage[] Message_RetrieveUnread(RemoteCredentials credentials)
		{
			string path = "messages/unread";

            //Create the request
            RestRequest request = new RestRequest("Message_RetrieveUnread");
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
					RemoteMessage[] responseObj = JsonConvert.DeserializeObject<RemoteMessage[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Message_RetrieveUnread", response.Body);
					RemoteMessage[] responseObj = JsonConvert.DeserializeObject<RemoteMessage[]>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Project_Retrieve", response.Body);
					RemoteProject[] responseObj = JsonConvert.DeserializeObject<RemoteProject[]>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Project_RetrieveById", response.Body);
					RemoteProject responseObj = JsonConvert.DeserializeObject<RemoteProject>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteProject);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Project_Create", response.Body);
					RemoteProject responseObj = JsonConvert.DeserializeObject<RemoteProject>(response.Body);

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


		public void Project_Update(RemoteCredentials credentials, object project_id, RemoteProject remoteProject)
		{
			string path = "projects/{project_id}";

            //Create the request
            RestRequest request = new RestRequest("Project_Update");
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

			if (remoteProject == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteProject);
				Common.Logger.LogTraceEvent("Project_Update", request.Body);

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


		public void Project_RefreshProgressExecutionStatusCaches1(RemoteCredentials credentials, object project_id, object run_async)
		{
			string path = "projects/{project_id}/refresh-caches?run_async={run_async}";

            //Create the request
            RestRequest request = new RestRequest("Project_RefreshProgressExecutionStatusCaches1");
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


		public void Project_RefreshProgressExecutionStatusCaches2(RemoteCredentials credentials, object project_id, object release_id, object run_async)
		{
			string path = "projects/{project_id}/refresh-caches/{release_id}?run_async={run_async}";

            //Create the request
            RestRequest request = new RestRequest("Project_RefreshProgressExecutionStatusCaches2");
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("ProjectRole_Retrieve", response.Body);
					RemoteProjectRole[] responseObj = JsonConvert.DeserializeObject<RemoteProjectRole[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteProjectTemplate[] ProjectTemplate_Retrieve(RemoteCredentials credentials)
		{
			string path = "project-templates";

            //Create the request
            RestRequest request = new RestRequest("ProjectTemplate_Retrieve");
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
					RemoteProjectTemplate[] responseObj = JsonConvert.DeserializeObject<RemoteProjectTemplate[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("ProjectTemplate_Retrieve", response.Body);
					RemoteProjectTemplate[] responseObj = JsonConvert.DeserializeObject<RemoteProjectTemplate[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteProjectTemplate ProjectTemplate_Create(RemoteCredentials credentials, object existing_project_template_id, RemoteProjectTemplate remoteProjectTemplate)
		{
			string path = "project-templates?existing_project_template_id={existing_project_template_id}";

            //Create the request
            RestRequest request = new RestRequest("ProjectTemplate_Create");
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
			AddRequestParameter(request, "existing_project_template_id", existing_project_template_id);
			

			//Specify the body (if appropriate)

			if (remoteProjectTemplate == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteProjectTemplate);
				Common.Logger.LogTraceEvent("ProjectTemplate_Create", request.Body);

			}


			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {

				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteProjectTemplate responseObj = JsonConvert.DeserializeObject<RemoteProjectTemplate>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("ProjectTemplate_Create", response.Body);
					RemoteProjectTemplate responseObj = JsonConvert.DeserializeObject<RemoteProjectTemplate>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void ProjectTemplate_Delete(RemoteCredentials credentials, object project_template_id)
		{
			string path = "project-templates/{project_template_id}";

            //Create the request
            RestRequest request = new RestRequest("ProjectTemplate_Delete");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

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


		public void ProjectTemplate_Update(RemoteCredentials credentials, object project_template_id, RemoteProjectTemplate remoteProjectTemplate)
		{
			string path = "project-templates/{project_template_id}";

            //Create the request
            RestRequest request = new RestRequest("ProjectTemplate_Update");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

			//Specify the body (if appropriate)

			if (remoteProjectTemplate == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteProjectTemplate);
				Common.Logger.LogTraceEvent("ProjectTemplate_Update", request.Body);

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


		public RemoteProjectTemplate ProjectTemplate_RetrieveById(RemoteCredentials credentials, object project_template_id)
		{
			string path = "project-templates/{project_template_id}";

            //Create the request
            RestRequest request = new RestRequest("ProjectTemplate_RetrieveById");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

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
					RemoteProjectTemplate responseObj = JsonConvert.DeserializeObject<RemoteProjectTemplate>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("ProjectTemplate_RetrieveById", response.Body);
					RemoteProjectTemplate responseObj = JsonConvert.DeserializeObject<RemoteProjectTemplate>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Project_RetrieveUserMembership", response.Body);
					RemoteProjectUser[] responseObj = JsonConvert.DeserializeObject<RemoteProjectUser[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void Project_AddUserMembership(RemoteCredentials credentials, object project_id, RemoteProjectUser remoteProjectUser)
		{
			string path = "projects/{project_id}/users";

            //Create the request
            RestRequest request = new RestRequest("Project_AddUserMembership");
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

			if (remoteProjectUser == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteProjectUser);
				Common.Logger.LogTraceEvent("Project_AddUserMembership", request.Body);

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


		public void Project_UpdateUserMembership(RemoteCredentials credentials, object project_id, RemoteProjectUser remoteProjectUser)
		{
			string path = "projects/{project_id}/users";

            //Create the request
            RestRequest request = new RestRequest("Project_UpdateUserMembership");
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

			if (remoteProjectUser == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteProjectUser);
				Common.Logger.LogTraceEvent("Project_UpdateUserMembership", request.Body);

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


		public void Project_RemoveUserMembership(RemoteCredentials credentials, object project_id, object user_id)
		{
			string path = "projects/{project_id}/users/{user_id}";

            //Create the request
            RestRequest request = new RestRequest("Project_RemoveUserMembership");
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Release_Retrieve1", response.Body);
					RemoteRelease[] responseObj = JsonConvert.DeserializeObject<RemoteRelease[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteFilters);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Release_Retrieve2", response.Body);
					RemoteRelease[] responseObj = JsonConvert.DeserializeObject<RemoteRelease[]>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Release_RetrieveById", response.Body);
					RemoteRelease responseObj = JsonConvert.DeserializeObject<RemoteRelease>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteFilters);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Release_Count", response.Body);
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteRelease);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Release_Create1", response.Body);
					RemoteRelease responseObj = JsonConvert.DeserializeObject<RemoteRelease>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteRelease);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Release_Create2", response.Body);
					RemoteRelease responseObj = JsonConvert.DeserializeObject<RemoteRelease>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteRelease);
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


		public void Release_Indent(RemoteCredentials credentials, object project_id, object release_id)
		{
			string path = "projects/{project_id}/releases/{release_id}/indent";

            //Create the request
            RestRequest request = new RestRequest("Release_Indent");
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


		public void Release_Outdent(RemoteCredentials credentials, object project_id, object release_id)
		{
			string path = "projects/{project_id}/releases/{release_id}/outdent";

            //Create the request
            RestRequest request = new RestRequest("Release_Outdent");
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Build_RetrieveById", response.Body);
					RemoteBuild responseObj = JsonConvert.DeserializeObject<RemoteBuild>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteBuild Build_RetrieveById_NoDescription(RemoteCredentials credentials, object project_id, object release_id, object build_id)
		{
			string path = "projects/{project_id}/releases/{release_id}/builds/{build_id}/no_description";

            //Create the request
            RestRequest request = new RestRequest("Build_RetrieveById_NoDescription");
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Build_RetrieveById_NoDescription", response.Body);
					RemoteBuild responseObj = JsonConvert.DeserializeObject<RemoteBuild>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteBuild[] Build_RetrieveByReleaseId1(RemoteCredentials credentials, object project_id, object release_id)
		{
			string path = "projects/{project_id}/releases/{release_id}/builds";

            //Create the request
            RestRequest request = new RestRequest("Build_RetrieveByReleaseId1");
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Build_RetrieveByReleaseId1", response.Body);
					RemoteBuild[] responseObj = JsonConvert.DeserializeObject<RemoteBuild[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteBuild[] Build_RetrieveByReleaseId2(RemoteCredentials credentials, object project_id, object release_id, object starting_row, object number_of_rows, object sort_by, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/releases/{release_id}/builds?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_by={sort_by}";

            //Create the request
            RestRequest request = new RestRequest("Build_RetrieveByReleaseId2");
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
			AddRequestParameter(request, "sort_by", sort_by);
			

			//Specify the body (if appropriate)

			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteFilters);
				Common.Logger.LogTraceEvent("Build_RetrieveByReleaseId2", request.Body);

			}


			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {

				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteBuild[] responseObj = JsonConvert.DeserializeObject<RemoteBuild[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Build_RetrieveByReleaseId2", response.Body);
					RemoteBuild[] responseObj = JsonConvert.DeserializeObject<RemoteBuild[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteBuild[] Build_RetrieveByReleaseId_NoDescription(RemoteCredentials credentials, object project_id, object release_id)
		{
			string path = "projects/{project_id}/releases/{release_id}/builds/no_description";

            //Create the request
            RestRequest request = new RestRequest("Build_RetrieveByReleaseId_NoDescription");
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Build_RetrieveByReleaseId_NoDescription", response.Body);
					RemoteBuild[] responseObj = JsonConvert.DeserializeObject<RemoteBuild[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteBuild);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Build_Create", response.Body);
					RemoteBuild responseObj = JsonConvert.DeserializeObject<RemoteBuild>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Release_RetrieveComments", response.Body);
					RemoteComment[] responseObj = JsonConvert.DeserializeObject<RemoteComment[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteComment);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Release_CreateComment", response.Body);
					RemoteComment responseObj = JsonConvert.DeserializeObject<RemoteComment>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteReleaseStatus[] Release_RetrieveStatuses(RemoteCredentials credentials, object project_template_id)
		{
			string path = "project-templates/{project_template_id}/releases/statuses";

            //Create the request
            RestRequest request = new RestRequest("Release_RetrieveStatuses");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

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
					RemoteReleaseStatus[] responseObj = JsonConvert.DeserializeObject<RemoteReleaseStatus[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Release_RetrieveStatuses", response.Body);
					RemoteReleaseStatus[] responseObj = JsonConvert.DeserializeObject<RemoteReleaseStatus[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(testCaseIds);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Release_RetrieveTestMapping", response.Body);
					RemoteReleaseTestCaseMapping[] responseObj = JsonConvert.DeserializeObject<RemoteReleaseTestCaseMapping[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteReleaseType[] Release_RetrieveTypes(RemoteCredentials credentials, object project_template_id)
		{
			string path = "project-templates/{project_template_id}/releases/types";

            //Create the request
            RestRequest request = new RestRequest("Release_RetrieveTypes");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

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
					RemoteReleaseType[] responseObj = JsonConvert.DeserializeObject<RemoteReleaseType[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Release_RetrieveTypes", response.Body);
					RemoteReleaseType[] responseObj = JsonConvert.DeserializeObject<RemoteReleaseType[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteWorkflowTransition[] Release_RetrieveWorkflowTransitions(RemoteCredentials credentials, object project_id, object release_type_id, object release_status_id, object is_creator, object is_owner)
		{
			string path = "projects/{project_id}/releases/types/{release_type_id}/workflow/transitions?status_id={release_status_id}&is_creator={is_creator}&isOwner={is_owner}";

            //Create the request
            RestRequest request = new RestRequest("Release_RetrieveWorkflowTransitions");
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
			AddRequestParameter(request, "release_type_id", release_type_id);
			AddRequestParameter(request, "release_status_id", release_status_id);
			AddRequestParameter(request, "is_creator", is_creator);
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
					RemoteWorkflowTransition[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowTransition[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Release_RetrieveWorkflowTransitions", response.Body);
					RemoteWorkflowTransition[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowTransition[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteWorkflowField[] Release_RetrieveWorkflowFields(RemoteCredentials credentials, object project_template_id, object release_type_id, object release_status_id)
		{
			string path = "project-templates/{project_template_id}/releases/types/{release_type_id}/workflow/fields?status_id={release_status_id}";

            //Create the request
            RestRequest request = new RestRequest("Release_RetrieveWorkflowFields");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			AddRequestParameter(request, "release_type_id", release_type_id);
			AddRequestParameter(request, "release_status_id", release_status_id);
			

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
					RemoteWorkflowField[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowField[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Release_RetrieveWorkflowFields", response.Body);
					RemoteWorkflowField[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowField[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteWorkflowCustomProperty[] Release_RetrieveWorkflowCustomProperties(RemoteCredentials credentials, object project_template_id, object release_type_id, object release_status_id)
		{
			string path = "project-templates/{project_template_id}/releases/types/{release_type_id}/workflow/custom-properties?status_id={release_status_id}";

            //Create the request
            RestRequest request = new RestRequest("Release_RetrieveWorkflowCustomProperties");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			AddRequestParameter(request, "release_type_id", release_type_id);
			AddRequestParameter(request, "release_status_id", release_status_id);
			

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
					RemoteWorkflowCustomProperty[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowCustomProperty[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Release_RetrieveWorkflowCustomProperties", response.Body);
					RemoteWorkflowCustomProperty[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowCustomProperty[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public Int32 Reports_CheckGeneratedReportStatus(RemoteCredentials credentials, object project_id, object report_generation_id)
		{
			string path = "projects/{project_id}/reports/generated?report_generation_id={report_generation_id}";

            //Create the request
            RestRequest request = new RestRequest("Reports_CheckGeneratedReportStatus");
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
			AddRequestParameter(request, "report_generation_id", report_generation_id);
			

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Reports_CheckGeneratedReportStatus", response.Body);
					Int32 responseObj = JsonConvert.DeserializeObject<Int32>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public Byte[] Reports_RetrieveGeneratedReport(RemoteCredentials credentials, object project_id, object generated_report_id)
		{
			string path = "projects/{project_id}/reports/generated/{generated_report_id}";

            //Create the request
            RestRequest request = new RestRequest("Reports_RetrieveGeneratedReport");
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
			AddRequestParameter(request, "generated_report_id", generated_report_id);
			

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Reports_RetrieveGeneratedReport", response.Body);
					Byte[] responseObj = JsonConvert.DeserializeObject<Byte[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteSavedReport[] Reports_RetrieveSaved(RemoteCredentials credentials, object project_id, object include_shared)
		{
			string path = "projects/{project_id}/reports/saved?include_shared={include_shared}";

            //Create the request
            RestRequest request = new RestRequest("Reports_RetrieveSaved");
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
			AddRequestParameter(request, "include_shared", include_shared);
			

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
					RemoteSavedReport[] responseObj = JsonConvert.DeserializeObject<RemoteSavedReport[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Reports_RetrieveSaved", response.Body);
					RemoteSavedReport[] responseObj = JsonConvert.DeserializeObject<RemoteSavedReport[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public String Reports_GenerateSavedReport(RemoteCredentials credentials, object project_id, object saved_report_id)
		{
			string path = "projects/{project_id}/reports/saved/{saved_report_id}/generate";

            //Create the request
            RestRequest request = new RestRequest("Reports_GenerateSavedReport");
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
			AddRequestParameter(request, "saved_report_id", saved_report_id);
			

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Reports_GenerateSavedReport", response.Body);
					String responseObj = JsonConvert.DeserializeObject<String>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_Count1", response.Body);
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteFilters);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_Count2", response.Body);
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteRequirement);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_Create1", response.Body);
					RemoteRequirement responseObj = JsonConvert.DeserializeObject<RemoteRequirement>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteRequirement);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_Create2", response.Body);
					RemoteRequirement responseObj = JsonConvert.DeserializeObject<RemoteRequirement>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteRequirement);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_Create3", response.Body);
					RemoteRequirement responseObj = JsonConvert.DeserializeObject<RemoteRequirement>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteFilters);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_Retrieve1", response.Body);
					RemoteRequirement[] responseObj = JsonConvert.DeserializeObject<RemoteRequirement[]>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_Retrieve2", response.Body);
					RemoteRequirement[] responseObj = JsonConvert.DeserializeObject<RemoteRequirement[]>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_RetrieveById", response.Body);
					RemoteRequirement responseObj = JsonConvert.DeserializeObject<RemoteRequirement>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteRequirement);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_RetrieveForOwner", response.Body);
					RemoteRequirement[] responseObj = JsonConvert.DeserializeObject<RemoteRequirement[]>(response.Body);

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
			string path = "projects/{project_id}/requirements/{requirement_id}/move?destination_requirement_id={destination_requirement_id}";

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
				request.Body = JsonConvert.SerializeObject(remoteComment);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_CreateComment", response.Body);
					RemoteComment responseObj = JsonConvert.DeserializeObject<RemoteComment>(response.Body);

					return responseObj;
				}

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_RetrieveComments", response.Body);
					RemoteComment[] responseObj = JsonConvert.DeserializeObject<RemoteComment[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteRequirementImportance[] Requirement_RetrieveImportances(RemoteCredentials credentials, object project_template_id)
		{
			string path = "project-templates/{project_template_id}/requirements/importances";

            //Create the request
            RestRequest request = new RestRequest("Requirement_RetrieveImportances");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

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
					RemoteRequirementImportance[] responseObj = JsonConvert.DeserializeObject<RemoteRequirementImportance[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_RetrieveImportances", response.Body);
					RemoteRequirementImportance[] responseObj = JsonConvert.DeserializeObject<RemoteRequirementImportance[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteRequirementStatus[] Requirement_RetrieveStatuses(RemoteCredentials credentials, object project_template_id)
		{
			string path = "project-templates/{project_template_id}/requirements/statuses";

            //Create the request
            RestRequest request = new RestRequest("Requirement_RetrieveStatuses");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

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
					RemoteRequirementStatus[] responseObj = JsonConvert.DeserializeObject<RemoteRequirementStatus[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_RetrieveStatuses", response.Body);
					RemoteRequirementStatus[] responseObj = JsonConvert.DeserializeObject<RemoteRequirementStatus[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteRequirementStep[] Requirement_RetrieveSteps(RemoteCredentials credentials, object project_id, object requirement_id)
		{
			string path = "projects/{project_id}/requirements/{requirement_id}/steps";

            //Create the request
            RestRequest request = new RestRequest("Requirement_RetrieveSteps");
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
					RemoteRequirementStep[] responseObj = JsonConvert.DeserializeObject<RemoteRequirementStep[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_RetrieveSteps", response.Body);
					RemoteRequirementStep[] responseObj = JsonConvert.DeserializeObject<RemoteRequirementStep[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteRequirementStep Requirement_RetrieveStepById(RemoteCredentials credentials, object project_id, object requirement_id, object requirement_step_id)
		{
			string path = "projects/{project_id}/requirements/{requirement_id}/steps/{requirement_step_id}";

            //Create the request
            RestRequest request = new RestRequest("Requirement_RetrieveStepById");
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
			AddRequestParameter(request, "requirement_step_id", requirement_step_id);
			

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
					RemoteRequirementStep responseObj = JsonConvert.DeserializeObject<RemoteRequirementStep>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_RetrieveStepById", response.Body);
					RemoteRequirementStep responseObj = JsonConvert.DeserializeObject<RemoteRequirementStep>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteRequirementStep Requirement_AddStep(RemoteCredentials credentials, object project_id, object requirement_id, object existing_requirement_step_id, object creator_id, RemoteRequirementStep remoteRequirementStep)
		{
			string path = "projects/{project_id}/requirements/{requirement_id}/steps?existing_requirement_step_id={existing_requirement_step_id}&creator_id={creator_id}";

            //Create the request
            RestRequest request = new RestRequest("Requirement_AddStep");
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
			AddRequestParameter(request, "existing_requirement_step_id", existing_requirement_step_id);
			AddRequestParameter(request, "creator_id", creator_id);
			

			//Specify the body (if appropriate)

			if (remoteRequirementStep == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteRequirementStep);
				Common.Logger.LogTraceEvent("Requirement_AddStep", request.Body);

			}


			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {

				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRequirementStep responseObj = JsonConvert.DeserializeObject<RemoteRequirementStep>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_AddStep", response.Body);
					RemoteRequirementStep responseObj = JsonConvert.DeserializeObject<RemoteRequirementStep>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void Requirement_UpdateStep(RemoteCredentials credentials, object project_id, object requirement_id, RemoteRequirementStep remoteRequirementStep)
		{
			string path = "projects/{project_id}/requirements/{requirement_id}/steps";

            //Create the request
            RestRequest request = new RestRequest("Requirement_UpdateStep");
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
			AddRequestParameter(request, "requirement_id", requirement_id);
			

			//Specify the body (if appropriate)

			if (remoteRequirementStep == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteRequirementStep);
				Common.Logger.LogTraceEvent("Requirement_UpdateStep", request.Body);

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


		public void Requirement_MoveStep(RemoteCredentials credentials, object project_id, object requirement_id, object source_requirement_step_id, object destination_requirement_step_id)
		{
			string path = "projects/{project_id}/requirements/{requirement_id}/steps/{source_requirement_step_id}/move?destination_requirement_step_id={destination_requirement_step_id}";

            //Create the request
            RestRequest request = new RestRequest("Requirement_MoveStep");
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
			AddRequestParameter(request, "source_requirement_step_id", source_requirement_step_id);
			AddRequestParameter(request, "destination_requirement_step_id", destination_requirement_step_id);
			

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


		public void Requirement_DeleteStep(RemoteCredentials credentials, object project_id, object requirement_id, object requirement_step_id)
		{
			string path = "projects/{project_id}/requirements/{requirement_id}/steps/{requirement_step_id}";

            //Create the request
            RestRequest request = new RestRequest("Requirement_DeleteStep");
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
			AddRequestParameter(request, "requirement_step_id", requirement_step_id);
			

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
				request.Body = JsonConvert.SerializeObject(remoteReqTestCaseMapping);
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
				request.Body = JsonConvert.SerializeObject(remoteReqTestCaseMapping);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_RetrieveTestCoverage", response.Body);
					RemoteRequirementTestCaseMapping[] responseObj = JsonConvert.DeserializeObject<RemoteRequirementTestCaseMapping[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void Requirement_AddTestStepCoverage(RemoteCredentials credentials, object project_id, RemoteRequirementTestStepMapping remoteReqTestStepMapping)
		{
			string path = "projects/{project_id}/requirements/test-steps";

            //Create the request
            RestRequest request = new RestRequest("Requirement_AddTestStepCoverage");
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

			if (remoteReqTestStepMapping == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteReqTestStepMapping);
				Common.Logger.LogTraceEvent("Requirement_AddTestStepCoverage", request.Body);

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


		public RemoteRequirementTestStepMapping[] Requirement_RetrieveTestStepCoverage(RemoteCredentials credentials, object project_id, object requirement_id)
		{
			string path = "projects/{project_id}/requirements/{requirement_id}/test-steps";

            //Create the request
            RestRequest request = new RestRequest("Requirement_RetrieveTestStepCoverage");
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
					RemoteRequirementTestStepMapping[] responseObj = JsonConvert.DeserializeObject<RemoteRequirementTestStepMapping[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_RetrieveTestStepCoverage", response.Body);
					RemoteRequirementTestStepMapping[] responseObj = JsonConvert.DeserializeObject<RemoteRequirementTestStepMapping[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void Requirement_RemoveTestStepCoverage(RemoteCredentials credentials, object project_id, RemoteRequirementTestStepMapping remoteReqTestStepMapping)
		{
			string path = "projects/{project_id}/requirements/test-steps";

            //Create the request
            RestRequest request = new RestRequest("Requirement_RemoveTestStepCoverage");
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

			if (remoteReqTestStepMapping == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteReqTestStepMapping);
				Common.Logger.LogTraceEvent("Requirement_RemoveTestStepCoverage", request.Body);

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


		public RemoteRequirementType[] Requirement_RetrieveTypes(RemoteCredentials credentials, object project_template_id)
		{
			string path = "project-templates/{project_template_id}/requirements/types";

            //Create the request
            RestRequest request = new RestRequest("Requirement_RetrieveTypes");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

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
					RemoteRequirementType[] responseObj = JsonConvert.DeserializeObject<RemoteRequirementType[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_RetrieveTypes", response.Body);
					RemoteRequirementType[] responseObj = JsonConvert.DeserializeObject<RemoteRequirementType[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteWorkflowTransition[] Requirement_RetrieveWorkflowTransitions(RemoteCredentials credentials, object project_id, object requirement_type_id, object requirement_status_id, object is_creator, object is_owner)
		{
			string path = "projects/{project_id}/requirements/types/{requirement_type_id}/workflow/transitions?status_id={requirement_status_id}&is_creator={is_creator}&isOwner={is_owner}";

            //Create the request
            RestRequest request = new RestRequest("Requirement_RetrieveWorkflowTransitions");
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
			AddRequestParameter(request, "requirement_type_id", requirement_type_id);
			AddRequestParameter(request, "requirement_status_id", requirement_status_id);
			AddRequestParameter(request, "is_creator", is_creator);
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
					RemoteWorkflowTransition[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowTransition[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_RetrieveWorkflowTransitions", response.Body);
					RemoteWorkflowTransition[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowTransition[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteWorkflowField[] Requirement_RetrieveWorkflowFields(RemoteCredentials credentials, object project_template_id, object requirement_type_id, object requirement_status_id)
		{
			string path = "project-templates/{project_template_id}/requirements/types/{requirement_type_id}/workflow/fields?status_id={requirement_status_id}";

            //Create the request
            RestRequest request = new RestRequest("Requirement_RetrieveWorkflowFields");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			AddRequestParameter(request, "requirement_type_id", requirement_type_id);
			AddRequestParameter(request, "requirement_status_id", requirement_status_id);
			

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
					RemoteWorkflowField[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowField[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_RetrieveWorkflowFields", response.Body);
					RemoteWorkflowField[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowField[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteWorkflowCustomProperty[] Requirement_RetrieveWorkflowCustomProperties(RemoteCredentials credentials, object project_template_id, object requirement_type_id, object requirement_status_id)
		{
			string path = "project-templates/{project_template_id}/requirements/types/{requirement_type_id}/workflow/custom-properties?status_id={requirement_status_id}";

            //Create the request
            RestRequest request = new RestRequest("Requirement_RetrieveWorkflowCustomProperties");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			AddRequestParameter(request, "requirement_type_id", requirement_type_id);
			AddRequestParameter(request, "requirement_status_id", requirement_status_id);
			

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
					RemoteWorkflowCustomProperty[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowCustomProperty[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Requirement_RetrieveWorkflowCustomProperties", response.Body);
					RemoteWorkflowCustomProperty[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowCustomProperty[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteRisk Risk_Create(RemoteCredentials credentials, object project_id, RemoteRisk remoteRisk)
		{
			string path = "projects/{project_id}/risks";

            //Create the request
            RestRequest request = new RestRequest("Risk_Create");
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

			if (remoteRisk == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteRisk);
				Common.Logger.LogTraceEvent("Risk_Create", request.Body);

			}


			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {

				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRisk responseObj = JsonConvert.DeserializeObject<RemoteRisk>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Risk_Create", response.Body);
					RemoteRisk responseObj = JsonConvert.DeserializeObject<RemoteRisk>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public Int64 Risk_Count1(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/risks/count";

            //Create the request
            RestRequest request = new RestRequest("Risk_Count1");
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Risk_Count1", response.Body);
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public Int64 Risk_Count2(RemoteCredentials credentials, object project_id, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/risks/count";

            //Create the request
            RestRequest request = new RestRequest("Risk_Count2");
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
				request.Body = JsonConvert.SerializeObject(remoteFilters);
				Common.Logger.LogTraceEvent("Risk_Count2", request.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Risk_Count2", response.Body);
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteRisk[] Risk_Retrieve(RemoteCredentials credentials, object project_id, object starting_row, object number_of_rows, object sort_field, object sort_direction, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/risks?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}";

            //Create the request
            RestRequest request = new RestRequest("Risk_Retrieve");
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
				request.Body = JsonConvert.SerializeObject(remoteFilters);
				Common.Logger.LogTraceEvent("Risk_Retrieve", request.Body);

			}


			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {

				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRisk[] responseObj = JsonConvert.DeserializeObject<RemoteRisk[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Risk_Retrieve", response.Body);
					RemoteRisk[] responseObj = JsonConvert.DeserializeObject<RemoteRisk[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteRisk Risk_RetrieveById(RemoteCredentials credentials, object project_id, object risk_id)
		{
			string path = "projects/{project_id}/risks/{risk_id}";

            //Create the request
            RestRequest request = new RestRequest("Risk_RetrieveById");
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
			AddRequestParameter(request, "risk_id", risk_id);
			

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
					RemoteRisk responseObj = JsonConvert.DeserializeObject<RemoteRisk>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Risk_RetrieveById", response.Body);
					RemoteRisk responseObj = JsonConvert.DeserializeObject<RemoteRisk>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteRisk[] Risk_RetrieveForOwner(RemoteCredentials credentials)
		{
			string path = "risks";

            //Create the request
            RestRequest request = new RestRequest("Risk_RetrieveForOwner");
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
					RemoteRisk[] responseObj = JsonConvert.DeserializeObject<RemoteRisk[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Risk_RetrieveForOwner", response.Body);
					RemoteRisk[] responseObj = JsonConvert.DeserializeObject<RemoteRisk[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void Risk_Update(RemoteCredentials credentials, object project_id, RemoteRisk remoteRisk)
		{
			string path = "projects/{project_id}/risks";

            //Create the request
            RestRequest request = new RestRequest("Risk_Update");
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

			if (remoteRisk == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteRisk);
				Common.Logger.LogTraceEvent("Risk_Update", request.Body);

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


		public void Risk_Delete(RemoteCredentials credentials, object project_id, object risk_id)
		{
			string path = "projects/{project_id}/risks/{risk_id}";

            //Create the request
            RestRequest request = new RestRequest("Risk_Delete");
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
			AddRequestParameter(request, "risk_id", risk_id);
			

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


		public RemoteComment[] Risk_RetrieveComments(RemoteCredentials credentials, object project_id, object risk_id)
		{
			string path = "projects/{project_id}/risks/{risk_id}/comments";

            //Create the request
            RestRequest request = new RestRequest("Risk_RetrieveComments");
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
			AddRequestParameter(request, "risk_id", risk_id);
			

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Risk_RetrieveComments", response.Body);
					RemoteComment[] responseObj = JsonConvert.DeserializeObject<RemoteComment[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteComment Risk_CreateComment(RemoteCredentials credentials, object project_id, object risk_id, RemoteComment remoteComment)
		{
			string path = "projects/{project_id}/risks/{risk_id}/comments";

            //Create the request
            RestRequest request = new RestRequest("Risk_CreateComment");
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
			AddRequestParameter(request, "risk_id", risk_id);
			

			//Specify the body (if appropriate)

			if (remoteComment == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteComment);
				Common.Logger.LogTraceEvent("Risk_CreateComment", request.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Risk_CreateComment", response.Body);
					RemoteComment responseObj = JsonConvert.DeserializeObject<RemoteComment>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteRiskImpact[] Risk_RetrieveImpacts(RemoteCredentials credentials, object project_template_id)
		{
			string path = "project-templates/{project_template_id}/risks/impacts";

            //Create the request
            RestRequest request = new RestRequest("Risk_RetrieveImpacts");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

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
					RemoteRiskImpact[] responseObj = JsonConvert.DeserializeObject<RemoteRiskImpact[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Risk_RetrieveImpacts", response.Body);
					RemoteRiskImpact[] responseObj = JsonConvert.DeserializeObject<RemoteRiskImpact[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteRiskMitigation[] Risk_RetrieveMitigations(RemoteCredentials credentials, object project_id, object risk_id)
		{
			string path = "projects/{project_id}/risks/{risk_id}/mitigations";

            //Create the request
            RestRequest request = new RestRequest("Risk_RetrieveMitigations");
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
			AddRequestParameter(request, "risk_id", risk_id);
			

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
					RemoteRiskMitigation[] responseObj = JsonConvert.DeserializeObject<RemoteRiskMitigation[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Risk_RetrieveMitigations", response.Body);
					RemoteRiskMitigation[] responseObj = JsonConvert.DeserializeObject<RemoteRiskMitigation[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteRiskMitigation Risk_RetrieveMitigationById(RemoteCredentials credentials, object project_id, object risk_id, object risk_mitigation_id)
		{
			string path = "projects/{project_id}/risks/{risk_id}/mitigations/{risk_mitigation_id}";

            //Create the request
            RestRequest request = new RestRequest("Risk_RetrieveMitigationById");
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
			AddRequestParameter(request, "risk_id", risk_id);
			AddRequestParameter(request, "risk_mitigation_id", risk_mitigation_id);
			

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
					RemoteRiskMitigation responseObj = JsonConvert.DeserializeObject<RemoteRiskMitigation>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Risk_RetrieveMitigationById", response.Body);
					RemoteRiskMitigation responseObj = JsonConvert.DeserializeObject<RemoteRiskMitigation>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteRiskMitigation Risk_AddMitigation(RemoteCredentials credentials, object project_id, object risk_id, object existing_risk_mitigation_id, object creator_id, RemoteRiskMitigation remoteRiskMitigation)
		{
			string path = "projects/{project_id}/risks/{risk_id}/mitigations?existing_risk_mitigation_id={existing_risk_mitigation_id}&creator_id={creator_id}";

            //Create the request
            RestRequest request = new RestRequest("Risk_AddMitigation");
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
			AddRequestParameter(request, "risk_id", risk_id);
			AddRequestParameter(request, "existing_risk_mitigation_id", existing_risk_mitigation_id);
			AddRequestParameter(request, "creator_id", creator_id);
			

			//Specify the body (if appropriate)

			if (remoteRiskMitigation == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteRiskMitigation);
				Common.Logger.LogTraceEvent("Risk_AddMitigation", request.Body);

			}


			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {

				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteRiskMitigation responseObj = JsonConvert.DeserializeObject<RemoteRiskMitigation>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Risk_AddMitigation", response.Body);
					RemoteRiskMitigation responseObj = JsonConvert.DeserializeObject<RemoteRiskMitigation>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void Risk_UpdateMitigation(RemoteCredentials credentials, object project_id, object risk_id, RemoteRiskMitigation remoteRiskMitigation)
		{
			string path = "projects/{project_id}/risks/{risk_id}/mitigations";

            //Create the request
            RestRequest request = new RestRequest("Risk_UpdateMitigation");
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
			AddRequestParameter(request, "risk_id", risk_id);
			

			//Specify the body (if appropriate)

			if (remoteRiskMitigation == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteRiskMitigation);
				Common.Logger.LogTraceEvent("Risk_UpdateMitigation", request.Body);

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


		public void Risk_DeleteMitigation(RemoteCredentials credentials, object project_id, object risk_id, object risk_mitigation_id)
		{
			string path = "projects/{project_id}/risks/{risk_id}/mitigations/{risk_mitigation_id}";

            //Create the request
            RestRequest request = new RestRequest("Risk_DeleteMitigation");
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
			AddRequestParameter(request, "risk_id", risk_id);
			AddRequestParameter(request, "risk_mitigation_id", risk_mitigation_id);
			

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


		public RemoteRiskProbability[] Risk_RetrieveProbabilities(RemoteCredentials credentials, object project_template_id)
		{
			string path = "project-templates/{project_template_id}/risks/probabilities";

            //Create the request
            RestRequest request = new RestRequest("Risk_RetrieveProbabilities");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

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
					RemoteRiskProbability[] responseObj = JsonConvert.DeserializeObject<RemoteRiskProbability[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Risk_RetrieveProbabilities", response.Body);
					RemoteRiskProbability[] responseObj = JsonConvert.DeserializeObject<RemoteRiskProbability[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteRiskStatus[] Risk_RetrieveStatuses(RemoteCredentials credentials, object project_template_id)
		{
			string path = "project-templates/{project_template_id}/risks/statuses";

            //Create the request
            RestRequest request = new RestRequest("Risk_RetrieveStatuses");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

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
					RemoteRiskStatus[] responseObj = JsonConvert.DeserializeObject<RemoteRiskStatus[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Risk_RetrieveStatuses", response.Body);
					RemoteRiskStatus[] responseObj = JsonConvert.DeserializeObject<RemoteRiskStatus[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteRiskType[] Risk_RetrieveTypes(RemoteCredentials credentials, object project_template_id)
		{
			string path = "project-templates/{project_template_id}/risks/types";

            //Create the request
            RestRequest request = new RestRequest("Risk_RetrieveTypes");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

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
					RemoteRiskType[] responseObj = JsonConvert.DeserializeObject<RemoteRiskType[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Risk_RetrieveTypes", response.Body);
					RemoteRiskType[] responseObj = JsonConvert.DeserializeObject<RemoteRiskType[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteWorkflowTransition[] Risk_RetrieveWorkflowTransitions(RemoteCredentials credentials, object project_id, object risk_type_id, object risk_status_id, object is_creator, object is_owner)
		{
			string path = "projects/{project_id}/risks/types/{risk_type_id}/workflow/transitions?status_id={risk_status_id}&is_creator={is_creator}&isOwner={is_owner}";

            //Create the request
            RestRequest request = new RestRequest("Risk_RetrieveWorkflowTransitions");
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
			AddRequestParameter(request, "risk_type_id", risk_type_id);
			AddRequestParameter(request, "risk_status_id", risk_status_id);
			AddRequestParameter(request, "is_creator", is_creator);
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
					RemoteWorkflowTransition[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowTransition[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Risk_RetrieveWorkflowTransitions", response.Body);
					RemoteWorkflowTransition[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowTransition[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteWorkflowField[] Risk_RetrieveWorkflowFields(RemoteCredentials credentials, object project_template_id, object risk_type_id, object risk_status_id)
		{
			string path = "project-templates/{project_template_id}/risks/types/{risk_type_id}/workflow/fields?status_id={risk_status_id}";

            //Create the request
            RestRequest request = new RestRequest("Risk_RetrieveWorkflowFields");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			AddRequestParameter(request, "risk_type_id", risk_type_id);
			AddRequestParameter(request, "risk_status_id", risk_status_id);
			

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
					RemoteWorkflowField[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowField[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Risk_RetrieveWorkflowFields", response.Body);
					RemoteWorkflowField[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowField[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteWorkflowCustomProperty[] Risk_RetrieveWorkflowCustomProperties(RemoteCredentials credentials, object project_template_id, object risk_type_id, object risk_status_id)
		{
			string path = "project-templates/{project_template_id}/risks/types/{risk_type_id}/workflow/custom-properties?status_id={risk_status_id}";

            //Create the request
            RestRequest request = new RestRequest("Risk_RetrieveWorkflowCustomProperties");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			AddRequestParameter(request, "risk_type_id", risk_type_id);
			AddRequestParameter(request, "risk_status_id", risk_status_id);
			

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
					RemoteWorkflowCustomProperty[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowCustomProperty[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Risk_RetrieveWorkflowCustomProperties", response.Body);
					RemoteWorkflowCustomProperty[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowCustomProperty[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteSavedFilter[] SavedFilter_RetrieveForUser(RemoteCredentials credentials)
		{
			string path = "saved-filters";

            //Create the request
            RestRequest request = new RestRequest("SavedFilter_RetrieveForUser");
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
					RemoteSavedFilter[] responseObj = JsonConvert.DeserializeObject<RemoteSavedFilter[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("SavedFilter_RetrieveForUser", response.Body);
					RemoteSavedFilter[] responseObj = JsonConvert.DeserializeObject<RemoteSavedFilter[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteSourceCodeBranch[] SourceCode_RetrieveBranches(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/source-code";

            //Create the request
            RestRequest request = new RestRequest("SourceCode_RetrieveBranches");
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
					RemoteSourceCodeBranch[] responseObj = JsonConvert.DeserializeObject<RemoteSourceCodeBranch[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("SourceCode_RetrieveBranches", response.Body);
					RemoteSourceCodeBranch[] responseObj = JsonConvert.DeserializeObject<RemoteSourceCodeBranch[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteSourceCodeConnection SourceCode_RetrieveConnectionInformation(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/source-code/connection";

            //Create the request
            RestRequest request = new RestRequest("SourceCode_RetrieveConnectionInformation");
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
					RemoteSourceCodeConnection responseObj = JsonConvert.DeserializeObject<RemoteSourceCodeConnection>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("SourceCode_RetrieveConnectionInformation", response.Body);
					RemoteSourceCodeConnection responseObj = JsonConvert.DeserializeObject<RemoteSourceCodeConnection>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteSourceCodeFile[] SourceCode_RetrieveFilesByFolder(RemoteCredentials credentials, object project_id, object branch_id, object folder_id)
		{
			string path = "projects/{project_id}/source-code/{branch_id}/folders/{folder_id}/files";

            //Create the request
            RestRequest request = new RestRequest("SourceCode_RetrieveFilesByFolder");
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
			AddRequestParameter(request, "branch_id", branch_id);
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
					RemoteSourceCodeFile[] responseObj = JsonConvert.DeserializeObject<RemoteSourceCodeFile[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("SourceCode_RetrieveFilesByFolder", response.Body);
					RemoteSourceCodeFile[] responseObj = JsonConvert.DeserializeObject<RemoteSourceCodeFile[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteSourceCodeFile[] SourceCode_RetrieveFilesByRevision(RemoteCredentials credentials, object project_id, object branch_id, object revision_id)
		{
			string path = "projects/{project_id}/source-code/{branch_id}/revisions/{revision_id}/files";

            //Create the request
            RestRequest request = new RestRequest("SourceCode_RetrieveFilesByRevision");
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
			AddRequestParameter(request, "branch_id", branch_id);
			AddRequestParameter(request, "revision_id", revision_id);
			

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
					RemoteSourceCodeFile[] responseObj = JsonConvert.DeserializeObject<RemoteSourceCodeFile[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("SourceCode_RetrieveFilesByRevision", response.Body);
					RemoteSourceCodeFile[] responseObj = JsonConvert.DeserializeObject<RemoteSourceCodeFile[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteSourceCodeFile SourceCode_RetrieveFileById(RemoteCredentials credentials, object project_id, object branch_id, object file_id)
		{
			string path = "projects/{project_id}/source-code/{branch_id}/files/{file_id}";

            //Create the request
            RestRequest request = new RestRequest("SourceCode_RetrieveFileById");
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
			AddRequestParameter(request, "branch_id", branch_id);
			AddRequestParameter(request, "file_id", file_id);
			

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
					RemoteSourceCodeFile responseObj = JsonConvert.DeserializeObject<RemoteSourceCodeFile>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("SourceCode_RetrieveFileById", response.Body);
					RemoteSourceCodeFile responseObj = JsonConvert.DeserializeObject<RemoteSourceCodeFile>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteSourceCodeFile[] SourceCode_RetrieveFilesForArtifact(RemoteCredentials credentials, object project_id, object branch_id, object artifact_type_id, object artifact_id)
		{
			string path = "projects/{project_id}/source-code/{branch_id}/files?artifact_type_id={artifact_type_id}&artifact_id={artifact_id}";

            //Create the request
            RestRequest request = new RestRequest("SourceCode_RetrieveFilesForArtifact");
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
			AddRequestParameter(request, "branch_id", branch_id);
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
					RemoteSourceCodeFile[] responseObj = JsonConvert.DeserializeObject<RemoteSourceCodeFile[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("SourceCode_RetrieveFilesForArtifact", response.Body);
					RemoteSourceCodeFile[] responseObj = JsonConvert.DeserializeObject<RemoteSourceCodeFile[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public Byte[] SourceCode_OpenFileById(RemoteCredentials credentials, object project_id, object branch_id, object file_id, object revision_id)
		{
			string path = "projects/{project_id}/source-code/{branch_id}/files/{file_id}/open?revision_id={revision_id}";

            //Create the request
            RestRequest request = new RestRequest("SourceCode_OpenFileById");
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
			AddRequestParameter(request, "branch_id", branch_id);
			AddRequestParameter(request, "file_id", file_id);
			AddRequestParameter(request, "revision_id", revision_id);
			

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("SourceCode_OpenFileById", response.Body);
					Byte[] responseObj = JsonConvert.DeserializeObject<Byte[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteSourceCodeFolder[] SourceCode_RetrieveFoldersByParent(RemoteCredentials credentials, object project_id, object branch_id, object parent_folder_id)
		{
			string path = "projects/{project_id}/source-code/{branch_id}/folders?parent_folder_id={parent_folder_id}";

            //Create the request
            RestRequest request = new RestRequest("SourceCode_RetrieveFoldersByParent");
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
			AddRequestParameter(request, "branch_id", branch_id);
			AddRequestParameter(request, "parent_folder_id", parent_folder_id);
			

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
					RemoteSourceCodeFolder[] responseObj = JsonConvert.DeserializeObject<RemoteSourceCodeFolder[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("SourceCode_RetrieveFoldersByParent", response.Body);
					RemoteSourceCodeFolder[] responseObj = JsonConvert.DeserializeObject<RemoteSourceCodeFolder[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteSourceCodeRevision[] SourceCode_RetrieveRevisions(RemoteCredentials credentials, object project_id, object branch_id, object start_Row, object number_rows, object sort_property, object sort_direction, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/source-code/{branch_id}/revisions?start_Row={start_Row}&number_rows={number_rows}&sort_property={sort_property}&sort_direction={sort_direction}";

            //Create the request
            RestRequest request = new RestRequest("SourceCode_RetrieveRevisions");
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
			AddRequestParameter(request, "branch_id", branch_id);
			AddRequestParameter(request, "start_Row", start_Row);
			AddRequestParameter(request, "number_rows", number_rows);
			AddRequestParameter(request, "sort_property", sort_property);
			AddRequestParameter(request, "sort_direction", sort_direction);
			

			//Specify the body (if appropriate)

			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteFilters);
				Common.Logger.LogTraceEvent("SourceCode_RetrieveRevisions", request.Body);

			}


			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {

				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteSourceCodeRevision[] responseObj = JsonConvert.DeserializeObject<RemoteSourceCodeRevision[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("SourceCode_RetrieveRevisions", response.Body);
					RemoteSourceCodeRevision[] responseObj = JsonConvert.DeserializeObject<RemoteSourceCodeRevision[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteSourceCodeRevision[] SourceCode_RetrieveRevisionsForFile(RemoteCredentials credentials, object project_id, object branch_id, object file_id)
		{
			string path = "projects/{project_id}/source-code/{branch_id}/files/{file_id}/revisions";

            //Create the request
            RestRequest request = new RestRequest("SourceCode_RetrieveRevisionsForFile");
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
			AddRequestParameter(request, "branch_id", branch_id);
			AddRequestParameter(request, "file_id", file_id);
			

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
					RemoteSourceCodeRevision[] responseObj = JsonConvert.DeserializeObject<RemoteSourceCodeRevision[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("SourceCode_RetrieveRevisionsForFile", response.Body);
					RemoteSourceCodeRevision[] responseObj = JsonConvert.DeserializeObject<RemoteSourceCodeRevision[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteSourceCodeRevision SourceCode_RetrieveRevisionById(RemoteCredentials credentials, object project_id, object branch_id, object revision_id)
		{
			string path = "projects/{project_id}/source-code/{branch_id}/revisions/{revision_id}";

            //Create the request
            RestRequest request = new RestRequest("SourceCode_RetrieveRevisionById");
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
			AddRequestParameter(request, "branch_id", branch_id);
			AddRequestParameter(request, "revision_id", revision_id);
			

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
					RemoteSourceCodeRevision responseObj = JsonConvert.DeserializeObject<RemoteSourceCodeRevision>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("SourceCode_RetrieveRevisionById", response.Body);
					RemoteSourceCodeRevision responseObj = JsonConvert.DeserializeObject<RemoteSourceCodeRevision>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteSourceCodeRevision[] SourceCode_RetrieveRevisionsForArtifact(RemoteCredentials credentials, object project_id, object branch_id, object artifact_type_id, object artifact_id)
		{
			string path = "projects/{project_id}/source-code/{branch_id}/revisions?artifact_type_id={artifact_type_id}&artifact_id={artifact_id}";

            //Create the request
            RestRequest request = new RestRequest("SourceCode_RetrieveRevisionsForArtifact");
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
			AddRequestParameter(request, "branch_id", branch_id);
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
					RemoteSourceCodeRevision[] responseObj = JsonConvert.DeserializeObject<RemoteSourceCodeRevision[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("SourceCode_RetrieveRevisionsForArtifact", response.Body);
					RemoteSourceCodeRevision[] responseObj = JsonConvert.DeserializeObject<RemoteSourceCodeRevision[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteLinkedArtifact[] SourceCode_RetrieveArtifactsForRevision(RemoteCredentials credentials, object project_id, object branch_id, object revision_id)
		{
			string path = "projects/{project_id}/source-code/{branch_id}/revisions/{revision_id}/associations";

            //Create the request
            RestRequest request = new RestRequest("SourceCode_RetrieveArtifactsForRevision");
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
			AddRequestParameter(request, "branch_id", branch_id);
			AddRequestParameter(request, "revision_id", revision_id);
			

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
					RemoteLinkedArtifact[] responseObj = JsonConvert.DeserializeObject<RemoteLinkedArtifact[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("SourceCode_RetrieveArtifactsForRevision", response.Body);
					RemoteLinkedArtifact[] responseObj = JsonConvert.DeserializeObject<RemoteLinkedArtifact[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void Subscription_SubscribeToArtifact(RemoteCredentials credentials, object project_id, object artifact_type_id, object artifact_id)
		{
			string path = "projects/{project_id}/artifact-types/{artifact_type_id}/artifacts/{artifact_id}/subscriptions";

            //Create the request
            RestRequest request = new RestRequest("Subscription_SubscribeToArtifact");
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


		public void Subscription_UnsubscribeFromArtifact(RemoteCredentials credentials, object project_id, object artifact_type_id, object artifact_id)
		{
			string path = "projects/{project_id}/artifact-types/{artifact_type_id}/artifacts/{artifact_id}/subscriptions";

            //Create the request
            RestRequest request = new RestRequest("Subscription_UnsubscribeFromArtifact");
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


		public RemoteArtifactSubscription[] Subscription_RetrieveForUser(RemoteCredentials credentials)
		{
			string path = "subscriptions";

            //Create the request
            RestRequest request = new RestRequest("Subscription_RetrieveForUser");
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
					RemoteArtifactSubscription[] responseObj = JsonConvert.DeserializeObject<RemoteArtifactSubscription[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Subscription_RetrieveForUser", response.Body);
					RemoteArtifactSubscription[] responseObj = JsonConvert.DeserializeObject<RemoteArtifactSubscription[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteArtifactSubscription[] Subscription_RetrieveForArtifact(RemoteCredentials credentials, object project_id, object artifact_type_id, object artifact_id)
		{
			string path = "projects/{project_id}/artifact-types/{artifact_type_id}/artifacts/{artifact_id}/subscriptions";

            //Create the request
            RestRequest request = new RestRequest("Subscription_RetrieveForArtifact");
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
					RemoteArtifactSubscription[] responseObj = JsonConvert.DeserializeObject<RemoteArtifactSubscription[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Subscription_RetrieveForArtifact", response.Body);
					RemoteArtifactSubscription[] responseObj = JsonConvert.DeserializeObject<RemoteArtifactSubscription[]>(response.Body);

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
			string path = "system/product-version";

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("System_GetProductVersion", response.Body);
					RemoteVersion responseObj = JsonConvert.DeserializeObject<RemoteVersion>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void System_ProcessNotifications(RemoteCredentials credentials)
		{
			string path = "system/notifications";

            //Create the request
            RestRequest request = new RestRequest("System_ProcessNotifications");
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


		public RemoteSetting[] System_GetSettings(RemoteCredentials credentials)
		{
			string path = "system/settings";

            //Create the request
            RestRequest request = new RestRequest("System_GetSettings");
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
					RemoteSetting[] responseObj = JsonConvert.DeserializeObject<RemoteSetting[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("System_GetSettings", response.Body);
					RemoteSetting[] responseObj = JsonConvert.DeserializeObject<RemoteSetting[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public String System_GetArtifactUrl(RemoteCredentials credentials, object navigation_link_id, object project_id, object artifact_id, object tab_name)
		{
			string path = "system/artifact-types/{navigation_link_id}/project/{project_id}/artifact/{artifact_id}?tab_name={tab_name}";

            //Create the request
            RestRequest request = new RestRequest("System_GetArtifactUrl");
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
			AddRequestParameter(request, "navigation_link_id", navigation_link_id);
			AddRequestParameter(request, "project_id", project_id);
			AddRequestParameter(request, "artifact_id", artifact_id);
			AddRequestParameter(request, "tab_name", tab_name);
			

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("System_GetArtifactUrl", response.Body);
					String responseObj = JsonConvert.DeserializeObject<String>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public Int32 System_GetProjectIdForArtifact(RemoteCredentials credentials, object artifact_type_id, object artifact_id)
		{
			string path = "system/artifact-types/{artifact_type_id}/{artifact_id}/project-id";

            //Create the request
            RestRequest request = new RestRequest("System_GetProjectIdForArtifact");
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
					Int32 responseObj = JsonConvert.DeserializeObject<Int32>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("System_GetProjectIdForArtifact", response.Body);
					Int32 responseObj = JsonConvert.DeserializeObject<Int32>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public String System_GetProductName(RemoteCredentials credentials)
		{
			string path = "system/settings/product-name";

            //Create the request
            RestRequest request = new RestRequest("System_GetProductName");
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
					String responseObj = JsonConvert.DeserializeObject<String>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("System_GetProductName", response.Body);
					String responseObj = JsonConvert.DeserializeObject<String>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public DateTime System_GetServerDateTime(RemoteCredentials credentials)
		{
			string path = "system/settings/server-date-time";

            //Create the request
            RestRequest request = new RestRequest("System_GetServerDateTime");
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
					DateTime responseObj = JsonConvert.DeserializeObject<DateTime>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("System_GetServerDateTime", response.Body);
					DateTime responseObj = JsonConvert.DeserializeObject<DateTime>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public String System_GetWebServerUrl(RemoteCredentials credentials)
		{
			string path = "system/settings/web-server-url";

            //Create the request
            RestRequest request = new RestRequest("System_GetWebServerUrl");
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
					String responseObj = JsonConvert.DeserializeObject<String>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("System_GetWebServerUrl", response.Body);
					String responseObj = JsonConvert.DeserializeObject<String>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteEvent2[] System_RetrieveEvents(RemoteCredentials credentials, object starting_row, object number_of_rows, object sort_field, object sort_direction, RemoteFilter[] remoteFilters)
		{
			string path = "system/events/search?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}";

            //Create the request
            RestRequest request = new RestRequest("System_RetrieveEvents");
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
				request.Body = JsonConvert.SerializeObject(remoteFilters);
				Common.Logger.LogTraceEvent("System_RetrieveEvents", request.Body);

			}


			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {

				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteEvent2[] responseObj = JsonConvert.DeserializeObject<RemoteEvent2[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("System_RetrieveEvents", response.Body);
					RemoteEvent2[] responseObj = JsonConvert.DeserializeObject<RemoteEvent2[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteTask);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_Create", response.Body);
					RemoteTask responseObj = JsonConvert.DeserializeObject<RemoteTask>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_Count1", response.Body);
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteFilters);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_Count2", response.Body);
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteFilters);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_Retrieve", response.Body);
					RemoteTask[] responseObj = JsonConvert.DeserializeObject<RemoteTask[]>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_RetrieveById", response.Body);
					RemoteTask responseObj = JsonConvert.DeserializeObject<RemoteTask>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_RetrieveForOwner", response.Body);
					RemoteTask[] responseObj = JsonConvert.DeserializeObject<RemoteTask[]>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_RetrieveNew", response.Body);
					RemoteTask[] responseObj = JsonConvert.DeserializeObject<RemoteTask[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteTask);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_RetrieveComments", response.Body);
					RemoteComment[] responseObj = JsonConvert.DeserializeObject<RemoteComment[]>(response.Body);

					return responseObj;
				}

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
				request.Body = JsonConvert.SerializeObject(remoteComment);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_CreateComment", response.Body);
					RemoteComment responseObj = JsonConvert.DeserializeObject<RemoteComment>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTaskFolder[] Task_RetrieveFolders(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/task-folders";

            //Create the request
            RestRequest request = new RestRequest("Task_RetrieveFolders");
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
					RemoteTaskFolder[] responseObj = JsonConvert.DeserializeObject<RemoteTaskFolder[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_RetrieveFolders", response.Body);
					RemoteTaskFolder[] responseObj = JsonConvert.DeserializeObject<RemoteTaskFolder[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTaskFolder[] Task_RetrieveFoldersByParent(RemoteCredentials credentials, object project_id, object parent_task_folder_id)
		{
			string path = "projects/{project_id}/task-folders/{parent_task_folder_id}/children";

            //Create the request
            RestRequest request = new RestRequest("Task_RetrieveFoldersByParent");
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
			AddRequestParameter(request, "parent_task_folder_id", parent_task_folder_id);
			

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
					RemoteTaskFolder[] responseObj = JsonConvert.DeserializeObject<RemoteTaskFolder[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_RetrieveFoldersByParent", response.Body);
					RemoteTaskFolder[] responseObj = JsonConvert.DeserializeObject<RemoteTaskFolder[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTaskFolder Task_CreateFolder(RemoteCredentials credentials, object project_id, RemoteTaskFolder remoteTaskFolder)
		{
			string path = "projects/{project_id}/task-folders";

            //Create the request
            RestRequest request = new RestRequest("Task_CreateFolder");
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

			if (remoteTaskFolder == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteTaskFolder);
				Common.Logger.LogTraceEvent("Task_CreateFolder", request.Body);

			}


			//Instantiate the rest client and call the method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK")
            {

				if (String.IsNullOrEmpty(response.Body))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings() {DateFormatHandling= DateFormatHandling.MicrosoftDateFormat};
					RemoteTaskFolder responseObj = JsonConvert.DeserializeObject<RemoteTaskFolder>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_CreateFolder", response.Body);
					RemoteTaskFolder responseObj = JsonConvert.DeserializeObject<RemoteTaskFolder>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTaskFolder Task_RetrieveFolderById(RemoteCredentials credentials, object project_id, object task_folder_id)
		{
			string path = "projects/{project_id}/task-folders/{task_folder_id}";

            //Create the request
            RestRequest request = new RestRequest("Task_RetrieveFolderById");
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
			AddRequestParameter(request, "task_folder_id", task_folder_id);
			

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
					RemoteTaskFolder responseObj = JsonConvert.DeserializeObject<RemoteTaskFolder>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_RetrieveFolderById", response.Body);
					RemoteTaskFolder responseObj = JsonConvert.DeserializeObject<RemoteTaskFolder>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void Task_DeleteFolder(RemoteCredentials credentials, object project_id, object task_folder_id)
		{
			string path = "projects/{project_id}/task-folders/{task_folder_id}";

            //Create the request
            RestRequest request = new RestRequest("Task_DeleteFolder");
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
			AddRequestParameter(request, "task_folder_id", task_folder_id);
			

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


		public void Task_UpdateFolder(RemoteCredentials credentials, object project_id, RemoteTaskFolder remoteTaskFolder)
		{
			string path = "projects/{project_id}/task-folders";

            //Create the request
            RestRequest request = new RestRequest("Task_UpdateFolder");
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

			if (remoteTaskFolder == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteTaskFolder);
				Common.Logger.LogTraceEvent("Task_UpdateFolder", request.Body);

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


		public RemoteTaskPriority[] Task_RetrievePriorities(RemoteCredentials credentials, object project_template_id)
		{
			string path = "project-templates/{project_template_id}/tasks/priorities";

            //Create the request
            RestRequest request = new RestRequest("Task_RetrievePriorities");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

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
					RemoteTaskPriority[] responseObj = JsonConvert.DeserializeObject<RemoteTaskPriority[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_RetrievePriorities", response.Body);
					RemoteTaskPriority[] responseObj = JsonConvert.DeserializeObject<RemoteTaskPriority[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTaskStatus[] Task_RetrieveStatuses(RemoteCredentials credentials, object project_template_id)
		{
			string path = "project-templates/{project_template_id}/tasks/statuses";

            //Create the request
            RestRequest request = new RestRequest("Task_RetrieveStatuses");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

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
					RemoteTaskStatus[] responseObj = JsonConvert.DeserializeObject<RemoteTaskStatus[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_RetrieveStatuses", response.Body);
					RemoteTaskStatus[] responseObj = JsonConvert.DeserializeObject<RemoteTaskStatus[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTaskType[] Task_RetrieveTypes(RemoteCredentials credentials, object project_template_id)
		{
			string path = "project-templates/{project_template_id}/tasks/types";

            //Create the request
            RestRequest request = new RestRequest("Task_RetrieveTypes");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

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
					RemoteTaskType[] responseObj = JsonConvert.DeserializeObject<RemoteTaskType[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_RetrieveTypes", response.Body);
					RemoteTaskType[] responseObj = JsonConvert.DeserializeObject<RemoteTaskType[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteWorkflowTransition[] Task_RetrieveWorkflowTransitions(RemoteCredentials credentials, object project_id, object task_type_id, object task_status_id, object is_creator, object is_owner)
		{
			string path = "projects/{project_id}/tasks/types/{task_type_id}/workflow/transitions?status_id={task_status_id}&is_creator={is_creator}&isOwner={is_owner}";

            //Create the request
            RestRequest request = new RestRequest("Task_RetrieveWorkflowTransitions");
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
			AddRequestParameter(request, "task_type_id", task_type_id);
			AddRequestParameter(request, "task_status_id", task_status_id);
			AddRequestParameter(request, "is_creator", is_creator);
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
					RemoteWorkflowTransition[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowTransition[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_RetrieveWorkflowTransitions", response.Body);
					RemoteWorkflowTransition[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowTransition[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteWorkflowField[] Task_RetrieveWorkflowFields(RemoteCredentials credentials, object project_template_id, object task_type_id, object task_status_id)
		{
			string path = "project-templates/{project_template_id}/tasks/types/{task_type_id}/workflow/fields?status_id={task_status_id}";

            //Create the request
            RestRequest request = new RestRequest("Task_RetrieveWorkflowFields");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			AddRequestParameter(request, "task_type_id", task_type_id);
			AddRequestParameter(request, "task_status_id", task_status_id);
			

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
					RemoteWorkflowField[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowField[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_RetrieveWorkflowFields", response.Body);
					RemoteWorkflowField[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowField[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteWorkflowCustomProperty[] Task_RetrieveWorkflowCustomProperties(RemoteCredentials credentials, object project_template_id, object task_type_id, object task_status_id)
		{
			string path = "project-templates/{project_template_id}/tasks/types/{task_type_id}/workflow/custom-properties?status_id={task_status_id}";

            //Create the request
            RestRequest request = new RestRequest("Task_RetrieveWorkflowCustomProperties");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			AddRequestParameter(request, "task_type_id", task_type_id);
			AddRequestParameter(request, "task_status_id", task_status_id);
			

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
					RemoteWorkflowCustomProperty[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowCustomProperty[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("Task_RetrieveWorkflowCustomProperties", response.Body);
					RemoteWorkflowCustomProperty[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowCustomProperty[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestCase TestCase_Create(RemoteCredentials credentials, object project_id, RemoteTestCase remoteTestCase)
		{
			string path = "projects/{project_id}/test-cases";

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
			

			//Specify the body (if appropriate)

			if (remoteTestCase == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteTestCase);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_Create", response.Body);
					RemoteTestCase responseObj = JsonConvert.DeserializeObject<RemoteTestCase>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveById", response.Body);
					RemoteTestCase responseObj = JsonConvert.DeserializeObject<RemoteTestCase>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteTestCase);
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
				request.Body = JsonConvert.SerializeObject(binaryData);
				Common.Logger.LogTraceEvent("TestCase_AddUpdateAutomationScript", request.Body);

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


		public Int64 TestCase_Count1(RemoteCredentials credentials, object project_id, object release_id)
		{
			string path = "projects/{project_id}/test-cases/count?release_id={release_id}";

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
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_Count1", response.Body);
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public Int64 TestCase_Count2(RemoteCredentials credentials, object project_id, object release_id, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/test-cases/count?release_id={release_id}";

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
			AddRequestParameter(request, "release_id", release_id);
			

			//Specify the body (if appropriate)

			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteFilters);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_Count2", response.Body);
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestCase[] TestCase_Retrieve1(RemoteCredentials credentials, object project_id, object starting_row, object number_of_rows, object sort_field, object sort_direction, object release_id)
		{
			string path = "projects/{project_id}/test-cases?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}&release_id={release_id}";

            //Create the request
            RestRequest request = new RestRequest("TestCase_Retrieve1");
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
					RemoteTestCase[] responseObj = JsonConvert.DeserializeObject<RemoteTestCase[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_Retrieve1", response.Body);
					RemoteTestCase[] responseObj = JsonConvert.DeserializeObject<RemoteTestCase[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestCase[] TestCase_Retrieve2(RemoteCredentials credentials, object project_id, object starting_row, object number_of_rows, object sort_field, object sort_direction, object release_id, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/test-cases/search?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}&release_id={release_id}";

            //Create the request
            RestRequest request = new RestRequest("TestCase_Retrieve2");
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
			AddRequestParameter(request, "release_id", release_id);
			

			//Specify the body (if appropriate)

			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteFilters);
				Common.Logger.LogTraceEvent("TestCase_Retrieve2", request.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_Retrieve2", response.Body);
					RemoteTestCase[] responseObj = JsonConvert.DeserializeObject<RemoteTestCase[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void TestCase_Move(RemoteCredentials credentials, object project_id, object test_case_id, object test_case_folder_id)
		{
			string path = "projects/{project_id}/test-cases/{test_case_id}/move?test_case_folder_id={test_case_folder_id}";

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
			AddRequestParameter(request, "test_case_folder_id", test_case_folder_id);
			

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveForOwner", response.Body);
					RemoteTestCase[] responseObj = JsonConvert.DeserializeObject<RemoteTestCase[]>(response.Body);

					return responseObj;
				}

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveComments", response.Body);
					RemoteComment[] responseObj = JsonConvert.DeserializeObject<RemoteComment[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteComment);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_CreateComment", response.Body);
					RemoteComment responseObj = JsonConvert.DeserializeObject<RemoteComment>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestCaseFolder TestCase_CreateFolder(RemoteCredentials credentials, object project_id, RemoteTestCaseFolder remoteTestCaseFolder)
		{
			string path = "projects/{project_id}/test-folders";

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
			

			//Specify the body (if appropriate)

			if (remoteTestCaseFolder == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteTestCaseFolder);
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
					RemoteTestCaseFolder responseObj = JsonConvert.DeserializeObject<RemoteTestCaseFolder>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_CreateFolder", response.Body);
					RemoteTestCaseFolder responseObj = JsonConvert.DeserializeObject<RemoteTestCaseFolder>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestCaseFolder TestCase_RetrieveFolderById(RemoteCredentials credentials, object project_id, object test_case_folder_id)
		{
			string path = "projects/{project_id}/test-folders/{test_case_folder_id}";

            //Create the request
            RestRequest request = new RestRequest("TestCase_RetrieveFolderById");
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
					RemoteTestCaseFolder responseObj = JsonConvert.DeserializeObject<RemoteTestCaseFolder>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveFolderById", response.Body);
					RemoteTestCaseFolder responseObj = JsonConvert.DeserializeObject<RemoteTestCaseFolder>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void TestCase_DeleteFolder(RemoteCredentials credentials, object project_id, object test_case_folder_id)
		{
			string path = "projects/{project_id}/test-folders/{test_case_folder_id}";

            //Create the request
            RestRequest request = new RestRequest("TestCase_DeleteFolder");
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
			AddRequestParameter(request, "test_case_folder_id", test_case_folder_id);
			

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


		public void TestCase_UpdateFolder(RemoteCredentials credentials, object project_id, RemoteTestCaseFolder remoteTestCaseFolder)
		{
			string path = "projects/{project_id}/test-folders";

            //Create the request
            RestRequest request = new RestRequest("TestCase_UpdateFolder");
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

			if (remoteTestCaseFolder == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteTestCaseFolder);
				Common.Logger.LogTraceEvent("TestCase_UpdateFolder", request.Body);

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


		public RemoteTestCaseFolder[] TestCase_RetrieveFolders(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/test-folders";

            //Create the request
            RestRequest request = new RestRequest("TestCase_RetrieveFolders");
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
					RemoteTestCaseFolder[] responseObj = JsonConvert.DeserializeObject<RemoteTestCaseFolder[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveFolders", response.Body);
					RemoteTestCaseFolder[] responseObj = JsonConvert.DeserializeObject<RemoteTestCaseFolder[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestCaseFolder[] TestCase_RetrieveFoldersByParent(RemoteCredentials credentials, object project_id, object parent_test_case_folder_id, object release_id)
		{
			string path = "projects/{project_id}/test-folders/{parent_test_case_folder_id}/children?release_id={release_id}";

            //Create the request
            RestRequest request = new RestRequest("TestCase_RetrieveFoldersByParent");
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
			AddRequestParameter(request, "parent_test_case_folder_id", parent_test_case_folder_id);
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
					RemoteTestCaseFolder[] responseObj = JsonConvert.DeserializeObject<RemoteTestCaseFolder[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveFoldersByParent", response.Body);
					RemoteTestCaseFolder[] responseObj = JsonConvert.DeserializeObject<RemoteTestCaseFolder[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public Int64 TestCase_CountForFolder1(RemoteCredentials credentials, object project_id, object test_folder_id, object release_id)
		{
			string path = "projects/{project_id}/test-folders/{test_folder_id}/count?release_id={release_id}";

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
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_CountForFolder1", response.Body);
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public Int64 TestCase_CountForFolder2(RemoteCredentials credentials, object project_id, object test_folder_id, object release_id, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/test-folders/{test_folder_id}/count?release_id={release_id}";

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
			AddRequestParameter(request, "release_id", release_id);
			

			//Specify the body (if appropriate)

			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteFilters);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_CountForFolder2", response.Body);
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestCase[] TestCase_RetrieveByFolder1(RemoteCredentials credentials, object project_id, object test_case_folder_id, object starting_row, object number_of_rows, object sort_field, object sort_direction, object release_id)
		{
			string path = "projects/{project_id}/test-folders/{test_case_folder_id}/test-cases?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}&release_id={release_id}";

            //Create the request
            RestRequest request = new RestRequest("TestCase_RetrieveByFolder1");
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
			AddRequestParameter(request, "starting_row", starting_row);
			AddRequestParameter(request, "number_of_rows", number_of_rows);
			AddRequestParameter(request, "sort_field", sort_field);
			AddRequestParameter(request, "sort_direction", sort_direction);
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
					RemoteTestCase[] responseObj = JsonConvert.DeserializeObject<RemoteTestCase[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveByFolder1", response.Body);
					RemoteTestCase[] responseObj = JsonConvert.DeserializeObject<RemoteTestCase[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestCase[] TestCase_RetrieveByFolder2(RemoteCredentials credentials, object project_id, object test_case_folder_id, object starting_row, object number_of_rows, object sort_field, object sort_direction, object release_id, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/test-folders/{test_case_folder_id}/test-cases/search?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}&release_id={release_id}";

            //Create the request
            RestRequest request = new RestRequest("TestCase_RetrieveByFolder2");
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
			AddRequestParameter(request, "test_case_folder_id", test_case_folder_id);
			AddRequestParameter(request, "starting_row", starting_row);
			AddRequestParameter(request, "number_of_rows", number_of_rows);
			AddRequestParameter(request, "sort_field", sort_field);
			AddRequestParameter(request, "sort_direction", sort_direction);
			AddRequestParameter(request, "release_id", release_id);
			

			//Specify the body (if appropriate)

			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteFilters);
				Common.Logger.LogTraceEvent("TestCase_RetrieveByFolder2", request.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveByFolder2", response.Body);
					RemoteTestCase[] responseObj = JsonConvert.DeserializeObject<RemoteTestCase[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(parameters);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_AddLink", response.Body);
					Int32 responseObj = JsonConvert.DeserializeObject<Int32>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteTestCaseParameter);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_AddParameter", response.Body);
					RemoteTestCaseParameter responseObj = JsonConvert.DeserializeObject<RemoteTestCaseParameter>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void TestCase_UpdateParameter(RemoteCredentials credentials, object project_id, RemoteTestCaseParameter remoteTestCaseParameter)
		{
			string path = "projects/{project_id}/test-cases/parameters";

            //Create the request
            RestRequest request = new RestRequest("TestCase_UpdateParameter");
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

			if (remoteTestCaseParameter == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteTestCaseParameter);
				Common.Logger.LogTraceEvent("TestCase_UpdateParameter", request.Body);

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


		public void TestCase_DeleteParameter(RemoteCredentials credentials, object project_id, RemoteTestCaseParameter remoteTestCaseParameter)
		{
			string path = "projects/{project_id}/test-cases/parameters";

            //Create the request
            RestRequest request = new RestRequest("TestCase_DeleteParameter");
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

			if (remoteTestCaseParameter == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteTestCaseParameter);
				Common.Logger.LogTraceEvent("TestCase_DeleteParameter", request.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_CreateParameterToken", response.Body);
					String responseObj = JsonConvert.DeserializeObject<String>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveParameters", response.Body);
					RemoteTestCaseParameter[] responseObj = JsonConvert.DeserializeObject<RemoteTestCaseParameter[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestCasePriority[] TestCase_RetrievePriorities(RemoteCredentials credentials, object project_template_id)
		{
			string path = "project-templates/{project_template_id}/test-cases/priorities";

            //Create the request
            RestRequest request = new RestRequest("TestCase_RetrievePriorities");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

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
					RemoteTestCasePriority[] responseObj = JsonConvert.DeserializeObject<RemoteTestCasePriority[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrievePriorities", response.Body);
					RemoteTestCasePriority[] responseObj = JsonConvert.DeserializeObject<RemoteTestCasePriority[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestCaseStatus[] TestCase_RetrieveStatuses(RemoteCredentials credentials, object project_template_id)
		{
			string path = "project-templates/{project_template_id}/test-cases/statuses";

            //Create the request
            RestRequest request = new RestRequest("TestCase_RetrieveStatuses");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

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
					RemoteTestCaseStatus[] responseObj = JsonConvert.DeserializeObject<RemoteTestCaseStatus[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveStatuses", response.Body);
					RemoteTestCaseStatus[] responseObj = JsonConvert.DeserializeObject<RemoteTestCaseStatus[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestCaseType[] TestCase_RetrieveTypes(RemoteCredentials credentials, object project_template_id)
		{
			string path = "project-templates/{project_template_id}/test-cases/types";

            //Create the request
            RestRequest request = new RestRequest("TestCase_RetrieveTypes");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			

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
					RemoteTestCaseType[] responseObj = JsonConvert.DeserializeObject<RemoteTestCaseType[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveTypes", response.Body);
					RemoteTestCaseType[] responseObj = JsonConvert.DeserializeObject<RemoteTestCaseType[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteWorkflowTransition[] TestCase_RetrieveWorkflowTransitions(RemoteCredentials credentials, object project_id, object test_case_type_id, object test_case_status_id, object is_creator, object is_owner)
		{
			string path = "projects/{project_id}/test-cases/types/{test_case_type_id}/workflow/transitions?status_id={test_case_status_id}&is_creator={is_creator}&isOwner={is_owner}";

            //Create the request
            RestRequest request = new RestRequest("TestCase_RetrieveWorkflowTransitions");
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
			AddRequestParameter(request, "test_case_type_id", test_case_type_id);
			AddRequestParameter(request, "test_case_status_id", test_case_status_id);
			AddRequestParameter(request, "is_creator", is_creator);
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
					RemoteWorkflowTransition[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowTransition[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveWorkflowTransitions", response.Body);
					RemoteWorkflowTransition[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowTransition[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteWorkflowField[] TestCase_RetrieveWorkflowFields(RemoteCredentials credentials, object project_template_id, object test_case_type_id, object test_case_status_id)
		{
			string path = "project-templates/{project_template_id}/test-cases/types/{test_case_type_id}/workflow/fields?status_id={test_case_status_id}";

            //Create the request
            RestRequest request = new RestRequest("TestCase_RetrieveWorkflowFields");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			AddRequestParameter(request, "test_case_type_id", test_case_type_id);
			AddRequestParameter(request, "test_case_status_id", test_case_status_id);
			

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
					RemoteWorkflowField[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowField[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveWorkflowFields", response.Body);
					RemoteWorkflowField[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowField[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteWorkflowCustomProperty[] TestCase_RetrieveWorkflowCustomProperties(RemoteCredentials credentials, object project_template_id, object test_case_type_id, object test_case_status_id)
		{
			string path = "project-templates/{project_template_id}/test-cases/types/{test_case_type_id}/workflow/custom-properties?status_id={test_case_status_id}";

            //Create the request
            RestRequest request = new RestRequest("TestCase_RetrieveWorkflowCustomProperties");
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
			AddRequestParameter(request, "project_template_id", project_template_id);
			AddRequestParameter(request, "test_case_type_id", test_case_type_id);
			AddRequestParameter(request, "test_case_status_id", test_case_status_id);
			

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
					RemoteWorkflowCustomProperty[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowCustomProperty[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveWorkflowCustomProperties", response.Body);
					RemoteWorkflowCustomProperty[] responseObj = JsonConvert.DeserializeObject<RemoteWorkflowCustomProperty[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestConfigurationSet TestConfiguration_RetrieveSetById(RemoteCredentials credentials, object project_id, object test_configuration_set_id)
		{
			string path = "projects/{project_id}/test-configuration-sets/{test_configuration_set_id}";

            //Create the request
            RestRequest request = new RestRequest("TestConfiguration_RetrieveSetById");
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
			AddRequestParameter(request, "test_configuration_set_id", test_configuration_set_id);
			

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
					RemoteTestConfigurationSet responseObj = JsonConvert.DeserializeObject<RemoteTestConfigurationSet>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestConfiguration_RetrieveSetById", response.Body);
					RemoteTestConfigurationSet responseObj = JsonConvert.DeserializeObject<RemoteTestConfigurationSet>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestConfigurationSet[] TestConfiguration_RetrieveSets(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/test-configuration-sets";

            //Create the request
            RestRequest request = new RestRequest("TestConfiguration_RetrieveSets");
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
					RemoteTestConfigurationSet[] responseObj = JsonConvert.DeserializeObject<RemoteTestConfigurationSet[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestConfiguration_RetrieveSets", response.Body);
					RemoteTestConfigurationSet[] responseObj = JsonConvert.DeserializeObject<RemoteTestConfigurationSet[]>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_Count1", response.Body);
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteFilters);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_Count2", response.Body);
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_RetrieveById", response.Body);
					RemoteTestRun responseObj = JsonConvert.DeserializeObject<RemoteTestRun>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_Retrieve1", response.Body);
					RemoteTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteTestRun[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteFilters);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_Retrieve2", response.Body);
					RemoteTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteTestRun[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void TestRun_Delete(RemoteCredentials credentials, object project_id, object test_run_id)
		{
			string path = "projects/{project_id}/test-runs/{test_run_id}";

            //Create the request
            RestRequest request = new RestRequest("TestRun_Delete");
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
			AddRequestParameter(request, "test_run_id", test_run_id);
			

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


		public RemoteTestRun[] TestRun_RetrieveByTestCaseId(RemoteCredentials credentials, object project_id, object test_case_id, object starting_row, object number_of_rows, object sort_field, object sort_direction, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/test-cases/{test_case_id}/test-runs/search?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}";

            //Create the request
            RestRequest request = new RestRequest("TestRun_RetrieveByTestCaseId");
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
				request.Body = JsonConvert.SerializeObject(remoteFilters);
				Common.Logger.LogTraceEvent("TestRun_RetrieveByTestCaseId", request.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestRun_RetrieveByTestCaseId", response.Body);
					RemoteTestRun[] responseObj = JsonConvert.DeserializeObject<RemoteTestRun[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestSet TestSet_Create(RemoteCredentials credentials, object project_id, RemoteTestSet remoteTestSet)
		{
			string path = "projects/{project_id}/test-sets";

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
			

			//Specify the body (if appropriate)

			if (remoteTestSet == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteTestSet);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_Create", response.Body);
					RemoteTestSet responseObj = JsonConvert.DeserializeObject<RemoteTestSet>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_RetrieveById", response.Body);
					RemoteTestSet responseObj = JsonConvert.DeserializeObject<RemoteTestSet>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteTestSet);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_RetrieveForOwner", response.Body);
					RemoteTestSet[] responseObj = JsonConvert.DeserializeObject<RemoteTestSet[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void TestSet_Move(RemoteCredentials credentials, object project_id, object test_set_id, object test_set_folder_id)
		{
			string path = "projects/{project_id}/test-sets/{test_set_id}/move?test_set_folder_id={test_set_folder_id}";

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
			AddRequestParameter(request, "test_set_folder_id", test_set_folder_id);
			

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


		public Int64 TestSet_Count1(RemoteCredentials credentials, object project_id, object release_id)
		{
			string path = "projects/{project_id}/test-sets/count?release_id={release_id}";

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
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_Count1", response.Body);
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public Int64 TestSet_Count2(RemoteCredentials credentials, object project_id, object release_id, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/test-sets/count?release_id={release_id}";

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
			AddRequestParameter(request, "release_id", release_id);
			

			//Specify the body (if appropriate)

			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteFilters);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_Count2", response.Body);
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestSet[] TestSet_Retrieve1(RemoteCredentials credentials, object project_id, object starting_row, object number_of_rows, object sort_field, object sort_direction, object release_id)
		{
			string path = "projects/{project_id}/test-sets?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}&release_id={release_id}";

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
			AddRequestParameter(request, "starting_row", starting_row);
			AddRequestParameter(request, "number_of_rows", number_of_rows);
			AddRequestParameter(request, "sort_field", sort_field);
			AddRequestParameter(request, "sort_direction", sort_direction);
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
					RemoteTestSet[] responseObj = JsonConvert.DeserializeObject<RemoteTestSet[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_Retrieve1", response.Body);
					RemoteTestSet[] responseObj = JsonConvert.DeserializeObject<RemoteTestSet[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestSet[] TestSet_Retrieve2(RemoteCredentials credentials, object project_id, object starting_row, object number_of_rows, object release_id, object sort_field, object sort_direction, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/test-sets/search?starting_row={starting_row}&number_of_rows={number_of_rows}&release_id={release_id}&sort_field={sort_field}&sort_direction={sort_direction}";

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
			AddRequestParameter(request, "release_id", release_id);
			AddRequestParameter(request, "sort_field", sort_field);
			AddRequestParameter(request, "sort_direction", sort_direction);
			

			//Specify the body (if appropriate)

			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteFilters);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_Retrieve2", response.Body);
					RemoteTestSet[] responseObj = JsonConvert.DeserializeObject<RemoteTestSet[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestConfigurationSet TestConfiguration_RetrieveForTestSet(RemoteCredentials credentials, object project_id, object test_set_id)
		{
			string path = "projects/{project_id}/test-sets/{test_set_id}/test-configuration-sets";

            //Create the request
            RestRequest request = new RestRequest("TestConfiguration_RetrieveForTestSet");
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
					RemoteTestConfigurationSet responseObj = JsonConvert.DeserializeObject<RemoteTestConfigurationSet>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestConfiguration_RetrieveForTestSet", response.Body);
					RemoteTestConfigurationSet responseObj = JsonConvert.DeserializeObject<RemoteTestConfigurationSet>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_RetrieveComments", response.Body);
					RemoteComment[] responseObj = JsonConvert.DeserializeObject<RemoteComment[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteComment);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_CreateComment", response.Body);
					RemoteComment responseObj = JsonConvert.DeserializeObject<RemoteComment>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestSetFolder TestSet_CreateFolder(RemoteCredentials credentials, object project_id, RemoteTestSetFolder remoteTestSetFolder)
		{
			string path = "projects/{project_id}/test-set-folders";

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
			

			//Specify the body (if appropriate)

			if (remoteTestSetFolder == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteTestSetFolder);
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
					RemoteTestSetFolder responseObj = JsonConvert.DeserializeObject<RemoteTestSetFolder>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_CreateFolder", response.Body);
					RemoteTestSetFolder responseObj = JsonConvert.DeserializeObject<RemoteTestSetFolder>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestSetFolder TestSet_RetrieveFolderById(RemoteCredentials credentials, object project_id, object test_set_folder_id)
		{
			string path = "projects/{project_id}/test-set-folders/{test_set_folder_id}";

            //Create the request
            RestRequest request = new RestRequest("TestSet_RetrieveFolderById");
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
			AddRequestParameter(request, "test_set_folder_id", test_set_folder_id);
			

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
					RemoteTestSetFolder responseObj = JsonConvert.DeserializeObject<RemoteTestSetFolder>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_RetrieveFolderById", response.Body);
					RemoteTestSetFolder responseObj = JsonConvert.DeserializeObject<RemoteTestSetFolder>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void TestSet_DeleteFolder(RemoteCredentials credentials, object project_id, object test_set_folder_id)
		{
			string path = "projects/{project_id}/test-set-folders/{test_set_folder_id}";

            //Create the request
            RestRequest request = new RestRequest("TestSet_DeleteFolder");
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
			AddRequestParameter(request, "test_set_folder_id", test_set_folder_id);
			

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


		public void TestSet_UpdateFolder(RemoteCredentials credentials, object project_id, RemoteTestSetFolder remoteTestSetFolder)
		{
			string path = "projects/{project_id}/test-set-folders";

            //Create the request
            RestRequest request = new RestRequest("TestSet_UpdateFolder");
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

			if (remoteTestSetFolder == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteTestSetFolder);
				Common.Logger.LogTraceEvent("TestSet_UpdateFolder", request.Body);

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


		public RemoteTestSetFolder[] TestSet_RetrieveFolders(RemoteCredentials credentials, object project_id)
		{
			string path = "projects/{project_id}/test-set-folders";

            //Create the request
            RestRequest request = new RestRequest("TestSet_RetrieveFolders");
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
					RemoteTestSetFolder[] responseObj = JsonConvert.DeserializeObject<RemoteTestSetFolder[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_RetrieveFolders", response.Body);
					RemoteTestSetFolder[] responseObj = JsonConvert.DeserializeObject<RemoteTestSetFolder[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestSetFolder[] TestSet_RetrieveFoldersByParent(RemoteCredentials credentials, object project_id, object parent_test_set_folder_id, object release_id)
		{
			string path = "projects/{project_id}/test-set-folders/{parent_test_set_folder_id}/children?release_id={release_id}";

            //Create the request
            RestRequest request = new RestRequest("TestSet_RetrieveFoldersByParent");
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
			AddRequestParameter(request, "parent_test_set_folder_id", parent_test_set_folder_id);
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
					RemoteTestSetFolder[] responseObj = JsonConvert.DeserializeObject<RemoteTestSetFolder[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_RetrieveFoldersByParent", response.Body);
					RemoteTestSetFolder[] responseObj = JsonConvert.DeserializeObject<RemoteTestSetFolder[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public Int64 TestSet_CountForFolder1(RemoteCredentials credentials, object project_id, object test_folder_id, object release_id)
		{
			string path = "projects/{project_id}/test-set-folders/{test_folder_id}/count?release_id={release_id}";

            //Create the request
            RestRequest request = new RestRequest("TestSet_CountForFolder1");
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
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_CountForFolder1", response.Body);
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public Int64 TestSet_CountForFolder2(RemoteCredentials credentials, object project_id, object test_folder_id, object release_id, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/test-set-folders/{test_folder_id}/count?release_id={release_id}";

            //Create the request
            RestRequest request = new RestRequest("TestSet_CountForFolder2");
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
			AddRequestParameter(request, "release_id", release_id);
			

			//Specify the body (if appropriate)

			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteFilters);
				Common.Logger.LogTraceEvent("TestSet_CountForFolder2", request.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_CountForFolder2", response.Body);
					Int64 responseObj = JsonConvert.DeserializeObject<Int64>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestSet[] TestSet_RetrieveByFolder1(RemoteCredentials credentials, object project_id, object test_set_folder_id, object starting_row, object number_of_rows, object sort_field, object sort_direction, object release_id)
		{
			string path = "projects/{project_id}/test-set-folders/{test_set_folder_id}/test-sets?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}&release_id={release_id}";

            //Create the request
            RestRequest request = new RestRequest("TestSet_RetrieveByFolder1");
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
			AddRequestParameter(request, "test_set_folder_id", test_set_folder_id);
			AddRequestParameter(request, "starting_row", starting_row);
			AddRequestParameter(request, "number_of_rows", number_of_rows);
			AddRequestParameter(request, "sort_field", sort_field);
			AddRequestParameter(request, "sort_direction", sort_direction);
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
					RemoteTestSet[] responseObj = JsonConvert.DeserializeObject<RemoteTestSet[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_RetrieveByFolder1", response.Body);
					RemoteTestSet[] responseObj = JsonConvert.DeserializeObject<RemoteTestSet[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestSet[] TestSet_RetrieveByFolder2(RemoteCredentials credentials, object project_id, object test_set_folder_id, object starting_row, object number_of_rows, object sort_field, object sort_direction, object release_id, RemoteFilter[] remoteFilters)
		{
			string path = "projects/{project_id}/test-set-folders/{test_set_folder_id}/test-sets/search?starting_row={starting_row}&number_of_rows={number_of_rows}&sort_field={sort_field}&sort_direction={sort_direction}&release_id={release_id}";

            //Create the request
            RestRequest request = new RestRequest("TestSet_RetrieveByFolder2");
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
			AddRequestParameter(request, "test_set_folder_id", test_set_folder_id);
			AddRequestParameter(request, "starting_row", starting_row);
			AddRequestParameter(request, "number_of_rows", number_of_rows);
			AddRequestParameter(request, "sort_field", sort_field);
			AddRequestParameter(request, "sort_direction", sort_direction);
			AddRequestParameter(request, "release_id", release_id);
			

			//Specify the body (if appropriate)

			if (remoteFilters == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteFilters);
				Common.Logger.LogTraceEvent("TestSet_RetrieveByFolder2", request.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_RetrieveByFolder2", response.Body);
					RemoteTestSet[] responseObj = JsonConvert.DeserializeObject<RemoteTestSet[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestSetParameter[] TestSet_RetrieveParameters(RemoteCredentials credentials, object project_id, object test_set_id)
		{
			string path = "projects/{project_id}/test-sets/{test_set_id}/parameters";

            //Create the request
            RestRequest request = new RestRequest("TestSet_RetrieveParameters");
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
					RemoteTestSetParameter[] responseObj = JsonConvert.DeserializeObject<RemoteTestSetParameter[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_RetrieveParameters", response.Body);
					RemoteTestSetParameter[] responseObj = JsonConvert.DeserializeObject<RemoteTestSetParameter[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void TestSet_AddParameter(RemoteCredentials credentials, object project_id, RemoteTestSetParameter remoteTestSetParameter)
		{
			string path = "projects/{project_id}/test-sets/parameters";

            //Create the request
            RestRequest request = new RestRequest("TestSet_AddParameter");
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

			if (remoteTestSetParameter == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteTestSetParameter);
				Common.Logger.LogTraceEvent("TestSet_AddParameter", request.Body);

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


		public void TestSet_UpdateParameter(RemoteCredentials credentials, object project_id, RemoteTestSetParameter remoteTestSetParameter)
		{
			string path = "projects/{project_id}/test-sets/parameters";

            //Create the request
            RestRequest request = new RestRequest("TestSet_UpdateParameter");
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

			if (remoteTestSetParameter == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteTestSetParameter);
				Common.Logger.LogTraceEvent("TestSet_UpdateParameter", request.Body);

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


		public void TestSet_DeleteParameter(RemoteCredentials credentials, object project_id, RemoteTestSetParameter remoteTestSetParameter)
		{
			string path = "projects/{project_id}/test-sets/parameters";

            //Create the request
            RestRequest request = new RestRequest("TestSet_DeleteParameter");
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

			if (remoteTestSetParameter == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteTestSetParameter);
				Common.Logger.LogTraceEvent("TestSet_DeleteParameter", request.Body);

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
				request.Body = JsonConvert.SerializeObject(parameters);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_AddTestMapping", response.Body);
					RemoteTestSetTestCaseMapping[] responseObj = JsonConvert.DeserializeObject<RemoteTestSetTestCaseMapping[]>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_RetrieveTestCaseMapping", response.Body);
					RemoteTestSetTestCaseMapping[] responseObj = JsonConvert.DeserializeObject<RemoteTestSetTestCaseMapping[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void TestSet_SetInUseStatus(RemoteCredentials credentials, object project_id, object test_set_id, object test_set_test_case_id, String is_in_use)
		{
			string path = "projects/{project_id}/test-sets/{test_set_id}/test-cases/{test_set_test_case_id}/in-use";

            //Create the request
            RestRequest request = new RestRequest("TestSet_SetInUseStatus");
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
			AddRequestParameter(request, "test_set_id", test_set_id);
			AddRequestParameter(request, "test_set_test_case_id", test_set_test_case_id);
			

			//Specify the body (if appropriate)

			if (is_in_use == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(is_in_use);
				Common.Logger.LogTraceEvent("TestSet_SetInUseStatus", request.Body);

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


		public Boolean TestSet_CheckInUseStatus(RemoteCredentials credentials, object project_id, object test_set_id, object test_set_test_case_id)
		{
			string path = "projects/{project_id}/test-sets/{test_set_id}/test-cases/{test_set_test_case_id}/in-use";

            //Create the request
            RestRequest request = new RestRequest("TestSet_CheckInUseStatus");
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
			AddRequestParameter(request, "test_set_test_case_id", test_set_test_case_id);
			

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
					Boolean responseObj = JsonConvert.DeserializeObject<Boolean>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_CheckInUseStatus", response.Body);
					Boolean responseObj = JsonConvert.DeserializeObject<Boolean>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveByTestSetId", response.Body);
					RemoteTestCase[] responseObj = JsonConvert.DeserializeObject<RemoteTestCase[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestSetTestCaseParameter[] TestSet_RetrieveTestCaseParameters(RemoteCredentials credentials, object project_id, object test_set_id, object test_set_test_case_id)
		{
			string path = "projects/{project_id}/test-sets/{test_set_id}/test-cases/{test_set_test_case_id}/parameters";

            //Create the request
            RestRequest request = new RestRequest("TestSet_RetrieveTestCaseParameters");
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
			AddRequestParameter(request, "test_set_test_case_id", test_set_test_case_id);
			

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
					RemoteTestSetTestCaseParameter[] responseObj = JsonConvert.DeserializeObject<RemoteTestSetTestCaseParameter[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestSet_RetrieveTestCaseParameters", response.Body);
					RemoteTestSetTestCaseParameter[] responseObj = JsonConvert.DeserializeObject<RemoteTestSetTestCaseParameter[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void TestSet_AddTestCaseParameter(RemoteCredentials credentials, object project_id, object test_set_id, RemoteTestSetTestCaseParameter remoteTestSetTestCaseParameter)
		{
			string path = "projects/{project_id}/test-sets/{test_set_id}/test-cases/parameters";

            //Create the request
            RestRequest request = new RestRequest("TestSet_AddTestCaseParameter");
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

			if (remoteTestSetTestCaseParameter == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteTestSetTestCaseParameter);
				Common.Logger.LogTraceEvent("TestSet_AddTestCaseParameter", request.Body);

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


		public void TestSet_UpdateTestCaseParameter(RemoteCredentials credentials, object project_id, object test_set_id, RemoteTestSetTestCaseParameter remoteTestSetTestCaseParameter)
		{
			string path = "projects/{project_id}/test-sets/{test_set_id}/test-cases/parameters";

            //Create the request
            RestRequest request = new RestRequest("TestSet_UpdateTestCaseParameter");
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
			AddRequestParameter(request, "test_set_id", test_set_id);
			

			//Specify the body (if appropriate)

			if (remoteTestSetTestCaseParameter == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteTestSetTestCaseParameter);
				Common.Logger.LogTraceEvent("TestSet_UpdateTestCaseParameter", request.Body);

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


		public void TestSet_DeleteTestCaseParameter(RemoteCredentials credentials, object project_id, object test_set_id, RemoteTestSetTestCaseParameter remoteTestSetTestCaseParameter)
		{
			string path = "projects/{project_id}/test-sets/{test_set_id}/test-cases/parameters";

            //Create the request
            RestRequest request = new RestRequest("TestSet_DeleteTestCaseParameter");
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

			if (remoteTestSetTestCaseParameter == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteTestSetTestCaseParameter);
				Common.Logger.LogTraceEvent("TestSet_DeleteTestCaseParameter", request.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteTestStep);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_AddStep", response.Body);
					RemoteTestStep responseObj = JsonConvert.DeserializeObject<RemoteTestStep>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestStep[] TestCase_RetrieveSteps(RemoteCredentials credentials, object project_id, object test_case_id)
		{
			string path = "projects/{project_id}/test-cases/{test_case_id}/test-steps";

            //Create the request
            RestRequest request = new RestRequest("TestCase_RetrieveSteps");
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
					RemoteTestStep[] responseObj = JsonConvert.DeserializeObject<RemoteTestStep[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveSteps", response.Body);
					RemoteTestStep[] responseObj = JsonConvert.DeserializeObject<RemoteTestStep[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteTestStep TestCase_RetrieveStepById(RemoteCredentials credentials, object project_id, object test_case_id, object test_step_id)
		{
			string path = "projects/{project_id}/test-cases/{test_case_id}/test-steps/{test_step_id}";

            //Create the request
            RestRequest request = new RestRequest("TestCase_RetrieveStepById");
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
					RemoteTestStep responseObj = JsonConvert.DeserializeObject<RemoteTestStep>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveStepById", response.Body);
					RemoteTestStep responseObj = JsonConvert.DeserializeObject<RemoteTestStep>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void TestCase_UpdateStep(RemoteCredentials credentials, object project_id, object test_case_id, RemoteTestStep remoteTestStep)
		{
			string path = "projects/{project_id}/test-cases/{test_case_id}/test-steps";

            //Create the request
            RestRequest request = new RestRequest("TestCase_UpdateStep");
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
			AddRequestParameter(request, "test_case_id", test_case_id);
			

			//Specify the body (if appropriate)

			if (remoteTestStep == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteTestStep);
				Common.Logger.LogTraceEvent("TestCase_UpdateStep", request.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestCase_RetrieveStepParameters", response.Body);
					RemoteTestStepParameter[] responseObj = JsonConvert.DeserializeObject<RemoteTestStepParameter[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void TestCase_AddStepParameters(RemoteCredentials credentials, object project_id, object test_case_id, object test_step_id, RemoteTestStepParameter[] testStepParameters)
		{
			string path = "projects/{project_id}/test-cases/{test_case_id}/test-steps/{test_step_id}/parameters";

            //Create the request
            RestRequest request = new RestRequest("TestCase_AddStepParameters");
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
			AddRequestParameter(request, "test_step_id", test_step_id);
			

			//Specify the body (if appropriate)

			if (testStepParameters == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(testStepParameters);
				Common.Logger.LogTraceEvent("TestCase_AddStepParameters", request.Body);

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


		public void TestCase_UpdateStepParameters(RemoteCredentials credentials, object project_id, object test_case_id, object test_step_id, RemoteTestStepParameter[] testStepParameters)
		{
			string path = "projects/{project_id}/test-cases/{test_case_id}/test-steps/{test_step_id}/parameters";

            //Create the request
            RestRequest request = new RestRequest("TestCase_UpdateStepParameters");
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
			AddRequestParameter(request, "test_case_id", test_case_id);
			AddRequestParameter(request, "test_step_id", test_step_id);
			

			//Specify the body (if appropriate)

			if (testStepParameters == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(testStepParameters);
				Common.Logger.LogTraceEvent("TestCase_UpdateStepParameters", request.Body);

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


		public void TestCase_DeleteStepParameters(RemoteCredentials credentials, object project_id, object test_case_id, object test_step_id, RemoteTestStepParameter[] testStepParameters)
		{
			string path = "projects/{project_id}/test-cases/{test_case_id}/test-steps/{test_step_id}/parameters";

            //Create the request
            RestRequest request = new RestRequest("TestCase_DeleteStepParameters");
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

			if (testStepParameters == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(testStepParameters);
				Common.Logger.LogTraceEvent("TestCase_DeleteStepParameters", request.Body);

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


		public RemoteRequirementTestStepMapping[] TestStep_RetrieveRequirementCoverage(RemoteCredentials credentials, object project_id, object test_step_id)
		{
			string path = "projects/{project_id}/test-steps/{test_step_id}/requirements";

            //Create the request
            RestRequest request = new RestRequest("TestStep_RetrieveRequirementCoverage");
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
					RemoteRequirementTestStepMapping[] responseObj = JsonConvert.DeserializeObject<RemoteRequirementTestStepMapping[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("TestStep_RetrieveRequirementCoverage", response.Body);
					RemoteRequirementTestStepMapping[] responseObj = JsonConvert.DeserializeObject<RemoteRequirementTestStepMapping[]>(response.Body);

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
				request.Body = JsonConvert.SerializeObject(remoteUser);
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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("User_Create", response.Body);
					RemoteUser responseObj = JsonConvert.DeserializeObject<RemoteUser>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("User_Retrieve", response.Body);
					RemoteUser responseObj = JsonConvert.DeserializeObject<RemoteUser>(response.Body);

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("User_RetrieveById", response.Body);
					RemoteUser responseObj = JsonConvert.DeserializeObject<RemoteUser>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteUser User_RetrieveByUserName(RemoteCredentials credentials, object user_name, object include_inactive)
		{
			string path = "users/usernames/{user_name}?include_inactive={include_inactive}";

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
			AddRequestParameter(request, "include_inactive", include_inactive);
			

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
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("User_RetrieveByUserName", response.Body);
					RemoteUser responseObj = JsonConvert.DeserializeObject<RemoteUser>(response.Body);

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


		public void User_Update(RemoteCredentials credentials, object user_id, RemoteUser remoteUser)
		{
			string path = "users/{user_id}";

            //Create the request
            RestRequest request = new RestRequest("User_Update");
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
			AddRequestParameter(request, "user_id", user_id);
			

			//Specify the body (if appropriate)

			if (remoteUser == null)
			{
				request.Body = "";
			}
			else
			{
				request.Body = JsonConvert.SerializeObject(remoteUser);
				Common.Logger.LogTraceEvent("User_Update", request.Body);

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


		public RemoteUser[] User_RetrieveAll(RemoteCredentials credentials)
		{
			string path = "users/all";

            //Create the request
            RestRequest request = new RestRequest("User_RetrieveAll");
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
					RemoteUser[] responseObj = JsonConvert.DeserializeObject<RemoteUser[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("User_RetrieveAll", response.Body);
					RemoteUser[] responseObj = JsonConvert.DeserializeObject<RemoteUser[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public RemoteUser[] User_RetrieveContacts(RemoteCredentials credentials)
		{
			string path = "users/contacts";

            //Create the request
            RestRequest request = new RestRequest("User_RetrieveContacts");
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
					RemoteUser[] responseObj = JsonConvert.DeserializeObject<RemoteUser[]>("", settings);
					return responseObj;
				}
				else
				{
					//Deserialize the response
					Common.Logger.LogTraceEvent("User_RetrieveContacts", response.Body);
					RemoteUser[] responseObj = JsonConvert.DeserializeObject<RemoteUser[]>(response.Body);

					return responseObj;
				}

            }
			else
			{
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}


		public void User_AddContact(RemoteCredentials credentials, object user_id)
		{
			string path = "users/contacts/{user_id}";

            //Create the request
            RestRequest request = new RestRequest("User_AddContact");
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


		public void User_RemoveContact(RemoteCredentials credentials, object user_id)
		{
			string path = "users/contacts/{user_id}";

            //Create the request
            RestRequest request = new RestRequest("User_RemoveContact");
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

			public System.Nullable<System.Int32> DocumentTypeId { get; set; }

			public System.Nullable<System.Int32> DocumentStatusId { get; set; }

			public System.Nullable<System.Int32> ProjectAttachmentFolderId { get; set; }

			public System.Collections.Generic.List<RemoteLinkedArtifact> AttachedArtifacts { get; set; }

			public System.Nullable<System.Int32> AuthorId { get; set; }

			public System.Nullable<System.Int32> EditorId { get; set; }

			public System.String FilenameOrUrl { get; set; }

			public System.String Description { get; set; }

			public System.DateTime UploadDate { get; set; }

			public System.DateTime EditedDate { get; set; }

			public System.Int32 Size { get; set; }

			public System.String CurrentVersion { get; set; }

			public System.Collections.Generic.List<RemoteDocumentVersion> Versions { get; set; }

			public System.String DocumentTypeName { get; set; }

			public System.String DocumentStatusName { get; set; }

			public System.String AttachmentTypeName { get; set; }

			public System.String AuthorName { get; set; }

			public System.String EditorName { get; set; }

			public System.Int32 ProjectId { get; set; }

			public System.Int32 ArtifactTypeId { get; set; }

			public System.DateTime ConcurrencyDate { get; set; }

			public System.Collections.Generic.List<RemoteArtifactCustomProperty> CustomProperties { get; set; }

			public System.Boolean IsAttachments { get; set; }

			public System.String Tags { get; set; }

	}


	public class RemoteFilter
	{

			public System.String PropertyName { get; set; }

			public System.Nullable<System.Int32> IntValue { get; set; }

			public System.String StringValue { get; set; }

			public MultiValueFilter MultiValue { get; set; }

			public DateRange DateRangeValue { get; set; }

	}


	public class RemoteAssociation
	{

			public System.Nullable<System.Int32> ArtifactLinkId { get; set; }

			public System.Int32 SourceArtifactId { get; set; }

			public System.Int32 SourceArtifactTypeId { get; set; }

			public System.Int32 DestArtifactId { get; set; }

			public System.Int32 DestArtifactTypeId { get; set; }

			public System.Int32 ArtifactLinkTypeId { get; set; }

			public System.Nullable<System.Int32> CreatorId { get; set; }

			public System.String Comment { get; set; }

			public System.Nullable<System.DateTime> CreationDate { get; set; }

			public System.String DestArtifactName { get; set; }

			public System.String DestArtifactTypeName { get; set; }

			public System.String CreatorName { get; set; }

			public System.String ArtifactLinkTypeName { get; set; }

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

			public System.Nullable<System.Int32> TestConfigurationId { get; set; }

			public System.Int32 ProjectId { get; set; }

			public System.Int32 ArtifactTypeId { get; set; }

			public System.DateTime ConcurrencyDate { get; set; }

			public System.Collections.Generic.List<RemoteArtifactCustomProperty> CustomProperties { get; set; }

			public System.Boolean IsAttachments { get; set; }

			public System.String Tags { get; set; }

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

			public System.Int32 ProjectId { get; set; }

			public System.Int32 ArtifactTypeId { get; set; }

			public System.DateTime ConcurrencyDate { get; set; }

			public System.Collections.Generic.List<RemoteArtifactCustomProperty> CustomProperties { get; set; }

			public System.Boolean IsAttachments { get; set; }

			public System.String Tags { get; set; }

	}


	public class RemoteComponent
	{

			public System.Nullable<System.Int32> ComponentId { get; set; }

			public System.Int32 ProjectId { get; set; }

			public System.String Name { get; set; }

			public System.Boolean IsActive { get; set; }

			public System.Boolean IsDeleted { get; set; }

	}


	public class RemoteCustomList
	{

			public System.Nullable<System.Int32> CustomPropertyListId { get; set; }

			public System.Int32 ProjectTemplateId { get; set; }

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

			public System.Boolean Active { get; set; }

			public System.Nullable<System.Int32> ParentCustomPropertyValueId { get; set; }

	}


	public class RemoteCustomProperty
	{

			public System.Nullable<System.Int32> CustomPropertyId { get; set; }

			public System.Int32 ProjectTemplateId { get; set; }

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


	public class RemoteTableData
	{

			public System.Collections.Generic.List<RemoteTableColumn> Columns { get; set; }

			public System.Collections.Generic.List<RemoteTableRow> Rows { get; set; }

	}


	public class RemoteDataMapping
	{

			public System.Nullable<System.Int32> ProjectId { get; set; }

			public System.Int32 InternalId { get; set; }

			public System.String ExternalKey { get; set; }

			public System.Boolean Primary { get; set; }

	}


	public class RemoteProjectArtifact
	{

			public System.Int32 ProjectId { get; set; }

			public System.Collections.Generic.List<System.Int32> ArtifactIds { get; set; }

	}


	public class RemoteDataSyncSystem
	{

			public System.Int32 DataSyncSystemId { get; set; }

			public System.Int32 DataSyncStatusId { get; set; }

			public System.String Name { get; set; }

			public System.String DisplayName { get; set; }

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

			public System.Boolean IsActive { get; set; }

	}


	public class RemoteEvent
	{

			public System.Int32 EventLogEntryType { get; set; }

			public System.String Message { get; set; }

			public System.String Details { get; set; }

	}


	public class RemoteDocumentFile
	{

			public System.Byte[] BinaryData { get; set; }

			public System.Nullable<System.Int32> AttachmentId { get; set; }

			public System.Int32 AttachmentTypeId { get; set; }

			public System.Nullable<System.Int32> DocumentTypeId { get; set; }

			public System.Nullable<System.Int32> DocumentStatusId { get; set; }

			public System.Nullable<System.Int32> ProjectAttachmentFolderId { get; set; }

			public System.Collections.Generic.List<RemoteLinkedArtifact> AttachedArtifacts { get; set; }

			public System.Nullable<System.Int32> AuthorId { get; set; }

			public System.Nullable<System.Int32> EditorId { get; set; }

			public System.String FilenameOrUrl { get; set; }

			public System.String Description { get; set; }

			public System.DateTime UploadDate { get; set; }

			public System.DateTime EditedDate { get; set; }

			public System.Int32 Size { get; set; }

			public System.String CurrentVersion { get; set; }

			public System.Collections.Generic.List<RemoteDocumentVersion> Versions { get; set; }

			public System.String DocumentTypeName { get; set; }

			public System.String DocumentStatusName { get; set; }

			public System.String AttachmentTypeName { get; set; }

			public System.String AuthorName { get; set; }

			public System.String EditorName { get; set; }

			public System.Int32 ProjectId { get; set; }

			public System.Int32 ArtifactTypeId { get; set; }

			public System.DateTime ConcurrencyDate { get; set; }

			public System.Collections.Generic.List<RemoteArtifactCustomProperty> CustomProperties { get; set; }

			public System.Boolean IsAttachments { get; set; }

			public System.String Tags { get; set; }

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

			public System.Nullable<System.Int32> DocumentTypeId { get; set; }

			public System.Int32 ProjectTemplateId { get; set; }

			public System.String Name { get; set; }

			public System.String Description { get; set; }

			public System.Boolean Active { get; set; }

			public System.Boolean Default { get; set; }

	}


	public class RemoteDocumentVersion
	{

			public System.Nullable<System.Int32> AttachmentVersionId { get; set; }

			public System.Int32 AttachmentId { get; set; }

			public System.Nullable<System.Int32> AuthorId { get; set; }

			public System.String FilenameOrUrl { get; set; }

			public System.String Description { get; set; }

			public System.DateTime UploadDate { get; set; }

			public System.Int32 Size { get; set; }

			public System.String VersionNumber { get; set; }

			public System.String AuthorName { get; set; }

	}


	public class RemoteDocumentVersionFile
	{

			public System.Byte[] BinaryData { get; set; }

			public System.Nullable<System.Int32> AttachmentVersionId { get; set; }

			public System.Int32 AttachmentId { get; set; }

			public System.Nullable<System.Int32> AuthorId { get; set; }

			public System.String FilenameOrUrl { get; set; }

			public System.String Description { get; set; }

			public System.DateTime UploadDate { get; set; }

			public System.Int32 Size { get; set; }

			public System.String VersionNumber { get; set; }

			public System.String AuthorName { get; set; }

	}


	public class RemoteHistoryChange
	{

			public System.Int64 HistoryChangeId { get; set; }

			public System.Int64 ChangeSetId { get; set; }

			public System.Nullable<System.Int32> CustomPropertyId { get; set; }

			public System.Nullable<System.Int32> ArtifactFieldId { get; set; }

			public System.String FieldName { get; set; }

			public System.String OldValue { get; set; }

			public System.String NewValue { get; set; }

			public System.String FieldCaption { get; set; }

			public RemoteHistoryChangeSet ChangeSet { get; set; }

	}


	public class RemoteHistoryChangeSet
	{

			public System.Int64 HistoryChangeSetId { get; set; }

			public System.Int32 ProjectId { get; set; }

			public System.Int32 UserId { get; set; }

			public System.String UserFullName { get; set; }

			public System.Int32 ArtifactId { get; set; }

			public System.String ArtifactName { get; set; }

			public System.Int32 ArtifactTypeId { get; set; }

			public System.String ArtifactTypeName { get; set; }

			public System.DateTime ChangeDate { get; set; }

			public System.Int32 ChangeTypeId { get; set; }

			public System.String ChangeTypeName { get; set; }

			public System.Int32 SignedId { get; set; }

			public System.Collections.Generic.List<RemoteHistoryChange> Changes { get; set; }

			public System.String Meaning { get; set; }

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

			public System.Collections.Generic.List<System.Int32> TestRunStepIds { get; set; }

			public System.Nullable<System.Int32> DetectedReleaseId { get; set; }

			public System.Nullable<System.Int32> ResolvedReleaseId { get; set; }

			public System.Nullable<System.Int32> VerifiedReleaseId { get; set; }

			public System.Collections.Generic.List<System.Int32> ComponentIds { get; set; }

			public System.String Name { get; set; }

			public System.String Description { get; set; }

			public System.Nullable<System.DateTime> CreationDate { get; set; }

			public System.Nullable<System.DateTime> StartDate { get; set; }

			public System.Nullable<System.DateTime> EndDate { get; set; }

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

			public System.Nullable<System.Int32> DetectedBuildId { get; set; }

			public System.String DetectedBuildName { get; set; }

			public System.Int32 ProjectId { get; set; }

			public System.Int32 ArtifactTypeId { get; set; }

			public System.DateTime ConcurrencyDate { get; set; }

			public System.Collections.Generic.List<RemoteArtifactCustomProperty> CustomProperties { get; set; }

			public System.Boolean IsAttachments { get; set; }

			public System.String Tags { get; set; }

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

			public System.Boolean IsPermanent { get; set; }

	}


	public class RemoteIncidentPriority
	{

			public System.Nullable<System.Int32> PriorityId { get; set; }

			public System.String Name { get; set; }

			public System.Boolean Active { get; set; }

			public System.String Color { get; set; }

			public System.Nullable<System.Int32> Score { get; set; }

	}


	public class RemoteIncidentSeverity
	{

			public System.Nullable<System.Int32> SeverityId { get; set; }

			public System.String Name { get; set; }

			public System.Boolean Active { get; set; }

			public System.String Color { get; set; }

			public System.Nullable<System.Int32> Score { get; set; }

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


	public class RemoteWorkflowTransition
	{

			public System.Boolean ExecuteByCreator { get; set; }

			public System.Boolean ExecuteByOwner { get; set; }

			public System.Int32 StatusId_Input { get; set; }

			public System.String StatusName_Input { get; set; }

			public System.Int32 StatusId_Output { get; set; }

			public System.String StatusName_Output { get; set; }

			public System.String Name { get; set; }

			public System.Int32 WorkflowId { get; set; }

			public System.Int32 TransitionId { get; set; }

			public System.Boolean RequireSignature { get; set; }

	}


	public class RemoteWorkflowField
	{

			public System.String FieldCaption { get; set; }

			public System.String FieldName { get; set; }

			public System.Int32 FieldId { get; set; }

			public System.Int32 FieldStateId { get; set; }

	}


	public class RemoteWorkflowCustomProperty
	{

			public System.String FieldCaption { get; set; }

			public System.String FieldName { get; set; }

			public System.Int32 CustomPropertyId { get; set; }

			public System.Int32 FieldStateId { get; set; }

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

			public System.Nullable<System.Int32> TestConfigurationId { get; set; }

			public System.Int32 ProjectId { get; set; }

			public System.Int32 ArtifactTypeId { get; set; }

			public System.DateTime ConcurrencyDate { get; set; }

			public System.Collections.Generic.List<RemoteArtifactCustomProperty> CustomProperties { get; set; }

			public System.Boolean IsAttachments { get; set; }

			public System.String Tags { get; set; }

	}


	public class RemoteMessageInfo
	{

			public System.Nullable<System.Int32> UnreadMessages { get; set; }

			public System.Collections.Generic.List<System.Int32> OnlineUsers { get; set; }

	}


	public class RemoteMessageIndividual
	{

			public System.Int32 RecipientUserId { get; set; }

			public System.String MessageText { get; set; }

	}


	public class RemoteUserMessage
	{

			public System.Int32 UserId { get; set; }

			public System.Int32 UnreadMessages { get; set; }

	}


	public class RemoteMessage
	{

			public System.Int64 MessageId { get; set; }

			public RemoteUser SenderUser { get; set; }

			public RemoteUser RecipientUser { get; set; }

			public System.DateTime CreationDate { get; set; }

			public System.DateTime LastUpdateDate { get; set; }

			public System.String Body { get; set; }

			public System.Boolean IsRead { get; set; }

			public RemoteLinkedArtifact RecipientArtifact { get; set; }

	}


	public class RemoteProject
	{

			public System.Nullable<System.Int32> ProjectId { get; set; }

			public System.Nullable<System.Int32> ProjectTemplateId { get; set; }

			public System.Nullable<System.Int32> ProjectGroupId { get; set; }

			public System.String Name { get; set; }

			public System.String Description { get; set; }

			public System.String Website { get; set; }

			public System.DateTime CreationDate { get; set; }

			public System.Boolean Active { get; set; }

			public System.Int32 WorkingHours { get; set; }

			public System.Int32 WorkingDays { get; set; }

			public System.Int32 NonWorkingHours { get; set; }

			public System.Nullable<System.DateTime> StartDate { get; set; }

			public System.Nullable<System.DateTime> EndDate { get; set; }

			public System.Int32 PercentComplete { get; set; }

	}


	public class RemoteProjectRole
	{

			public System.Nullable<System.Int32> ProjectRoleId { get; set; }

			public System.String Name { get; set; }

			public System.String Description { get; set; }

			public System.Boolean Active { get; set; }

			public System.Boolean Admin { get; set; }

			public System.Boolean DiscussionsAdd { get; set; }

			public System.Boolean SourceCodeView { get; set; }

			public System.Collections.Generic.List<RemoteRolePermission> Permissions { get; set; }

	}


	public class RemoteProjectTemplate
	{

			public System.Nullable<System.Int32> ProjectTemplateId { get; set; }

			public System.String Name { get; set; }

			public System.String Description { get; set; }

			public System.Boolean IsActive { get; set; }

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

			public System.Nullable<System.Int32> OwnerId { get; set; }

			public System.String OwnerName { get; set; }

			public System.String IndentLevel { get; set; }

			public System.String Name { get; set; }

			public System.String Description { get; set; }

			public System.String VersionNumber { get; set; }

			public System.DateTime CreationDate { get; set; }

			public System.DateTime LastUpdateDate { get; set; }

			public System.Boolean Summary { get; set; }

			public System.Boolean Active { get; set; }

			public System.Int32 ReleaseStatusId { get; set; }

			public System.Int32 ReleaseTypeId { get; set; }

			public System.DateTime StartDate { get; set; }

			public System.DateTime EndDate { get; set; }

			public System.Decimal ResourceCount { get; set; }

			public System.Decimal DaysNonWorking { get; set; }

			public System.Nullable<System.Int32> PlannedEffort { get; set; }

			public System.Nullable<System.Int32> AvailableEffort { get; set; }

			public System.Nullable<System.Int32> TaskEstimatedEffort { get; set; }

			public System.Nullable<System.Int32> TaskActualEffort { get; set; }

			public System.Nullable<System.Int32> TaskCount { get; set; }

			public System.String CreatorName { get; set; }

			public System.String FullName { get; set; }

			public System.String ReleaseStatusName { get; set; }

			public System.String ReleaseTypeName { get; set; }

			public System.Int32 CountBlocked { get; set; }

			public System.Int32 CountCaution { get; set; }

			public System.Int32 CountFailed { get; set; }

			public System.Int32 CountNotApplicable { get; set; }

			public System.Int32 CountNotRun { get; set; }

			public System.Int32 CountPassed { get; set; }

			public System.Int32 PercentComplete { get; set; }

			public System.Nullable<System.Int32> MilestoneId { get; set; }

			public System.Int32 ProjectId { get; set; }

			public System.Int32 ArtifactTypeId { get; set; }

			public System.DateTime ConcurrencyDate { get; set; }

			public System.Collections.Generic.List<RemoteArtifactCustomProperty> CustomProperties { get; set; }

			public System.Boolean IsAttachments { get; set; }

			public System.String Tags { get; set; }

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


	public class RemoteReleaseStatus
	{

			public System.Int32 ReleaseStatusId { get; set; }

			public System.String Name { get; set; }

			public System.Boolean Active { get; set; }

			public System.Int32 Position { get; set; }

	}


	public class RemoteReleaseTestCaseMapping
	{

			public System.Int32 ReleaseId { get; set; }

			public System.Int32 TestCaseId { get; set; }

	}


	public class RemoteReleaseType
	{

			public System.Int32 ReleaseTypeId { get; set; }

			public System.String Name { get; set; }

			public System.Nullable<System.Int32> WorkflowId { get; set; }

			public System.Boolean Active { get; set; }

			public System.Int32 Position { get; set; }

	}


	public class RemoteSavedReport
	{

			public System.Int32 SavedReportId { get; set; }

			public System.Nullable<System.Int32> ProjectId { get; set; }

			public System.String Name { get; set; }

			public System.Boolean IsShared { get; set; }

			public System.Int32 ReportFormatId { get; set; }

	}


	public class RemoteRequirement
	{

			public System.Nullable<System.Int32> RequirementId { get; set; }

			public System.String IndentLevel { get; set; }

			public System.Nullable<System.Int32> StatusId { get; set; }

			public System.Nullable<System.Int32> RequirementTypeId { get; set; }

			public System.Nullable<System.Int32> AuthorId { get; set; }

			public System.Nullable<System.Int32> OwnerId { get; set; }

			public System.Nullable<System.Int32> ImportanceId { get; set; }

			public System.Nullable<System.Int32> ReleaseId { get; set; }

			public System.Nullable<System.Int32> ComponentId { get; set; }

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

			public System.Nullable<System.Decimal> EstimatePoints { get; set; }

			public System.Nullable<System.Int32> EstimatedEffort { get; set; }

			public System.Nullable<System.Int32> TaskEstimatedEffort { get; set; }

			public System.Nullable<System.Int32> TaskActualEffort { get; set; }

			public System.Nullable<System.Int32> TaskCount { get; set; }

			public System.String ReleaseVersionNumber { get; set; }

			public System.String AuthorName { get; set; }

			public System.String OwnerName { get; set; }

			public System.String StatusName { get; set; }

			public System.String ImportanceName { get; set; }

			public System.String ProjectName { get; set; }

			public System.String RequirementTypeName { get; set; }

			public System.Collections.Generic.List<RemoteRequirementStep> Steps { get; set; }

			public System.Nullable<System.DateTime> StartDate { get; set; }

			public System.Nullable<System.DateTime> EndDate { get; set; }

			public System.Nullable<System.Int32> PercentComplete { get; set; }

			public System.Nullable<System.Int32> ThemeId { get; set; }

			public System.Nullable<System.Int32> GoalId { get; set; }

			public System.Int32 ProjectId { get; set; }

			public System.Int32 ArtifactTypeId { get; set; }

			public System.DateTime ConcurrencyDate { get; set; }

			public System.Collections.Generic.List<RemoteArtifactCustomProperty> CustomProperties { get; set; }

			public System.Boolean IsAttachments { get; set; }

			public System.String Tags { get; set; }

	}


	public class RemoteRequirementImportance
	{

			public System.Nullable<System.Int32> ImportanceId { get; set; }

			public System.String Name { get; set; }

			public System.Boolean Active { get; set; }

			public System.String Color { get; set; }

			public System.Nullable<System.Int32> Score { get; set; }

	}


	public class RemoteRequirementStatus
	{

			public System.Int32 RequirementStatusId { get; set; }

			public System.String Name { get; set; }

			public System.Boolean Active { get; set; }

			public System.Int32 Position { get; set; }

	}


	public class RemoteRequirementStep
	{

			public System.Nullable<System.Int32> RequirementStepId { get; set; }

			public System.Int32 RequirementId { get; set; }

			public System.Int32 Position { get; set; }

			public System.String Description { get; set; }

			public System.DateTime LastUpdateDate { get; set; }

			public System.DateTime ConcurrencyDate { get; set; }

			public System.DateTime CreationDate { get; set; }

	}


	public class RemoteRequirementTestCaseMapping
	{

			public System.Int32 RequirementId { get; set; }

			public System.Int32 TestCaseId { get; set; }

	}


	public class RemoteRequirementTestStepMapping
	{

			public System.Int32 RequirementId { get; set; }

			public System.Int32 TestStepId { get; set; }

	}


	public class RemoteRequirementType
	{

			public System.Int32 RequirementTypeId { get; set; }

			public System.String Name { get; set; }

			public System.Nullable<System.Int32> WorkflowId { get; set; }

			public System.Boolean IsActive { get; set; }

			public System.Boolean IsDefault { get; set; }

			public System.Boolean IsSteps { get; set; }

	}


	public class RemoteRisk
	{

			public System.Nullable<System.Int32> RiskId { get; set; }

			public System.Nullable<System.DateTime> ClosedDate { get; set; }

			public System.Nullable<System.Int32> ComponentId { get; set; }

			public System.String ComponentName { get; set; }

			public System.DateTime CreationDate { get; set; }

			public System.Int32 CreatorId { get; set; }

			public System.String CreatorName { get; set; }

			public System.String Description { get; set; }

			public System.Boolean IsDeleted { get; set; }

			public System.DateTime LastUpdateDate { get; set; }

			public System.String Name { get; set; }

			public System.Nullable<System.Int32> OwnerId { get; set; }

			public System.String OwnerName { get; set; }

			public System.Nullable<System.Int32> ReleaseId { get; set; }

			public System.String ReleaseName { get; set; }

			public System.String ReleaseVersionNumber { get; set; }

			public System.Nullable<System.DateTime> ReviewDate { get; set; }

			public System.Nullable<System.Int32> RiskImpactId { get; set; }

			public System.String RiskImpactName { get; set; }

			public System.Nullable<System.Int32> RiskProbabilityId { get; set; }

			public System.String RiskProbabilityName { get; set; }

			public System.Nullable<System.Int32> RiskStatusId { get; set; }

			public System.String RiskStatusName { get; set; }

			public System.Nullable<System.Int32> RiskTypeId { get; set; }

			public System.String RiskTypeName { get; set; }

			public System.Nullable<System.Int32> RiskExposure { get; set; }

			public System.Nullable<System.Int32> ProjectGroupId { get; set; }

			public System.Nullable<System.Int32> RiskDetectabilityId { get; set; }

			public System.String RiskDetectabilityName { get; set; }

			public System.Nullable<System.Int32> GoalId { get; set; }

			public System.Int32 ProjectId { get; set; }

			public System.Int32 ArtifactTypeId { get; set; }

			public System.DateTime ConcurrencyDate { get; set; }

			public System.Collections.Generic.List<RemoteArtifactCustomProperty> CustomProperties { get; set; }

			public System.Boolean IsAttachments { get; set; }

			public System.String Tags { get; set; }

	}


	public class RemoteRiskImpact
	{

			public System.Nullable<System.Int32> RiskImpactId { get; set; }

			public System.String Name { get; set; }

			public System.Boolean Active { get; set; }

			public System.Int32 Position { get; set; }

			public System.String Color { get; set; }

			public System.Int32 Score { get; set; }

	}


	public class RemoteRiskMitigation
	{

			public System.Nullable<System.Int32> RiskMitigationId { get; set; }

			public System.Int32 RiskId { get; set; }

			public System.Int32 Position { get; set; }

			public System.String Description { get; set; }

			public System.DateTime LastUpdateDate { get; set; }

			public System.DateTime ConcurrencyDate { get; set; }

			public System.DateTime CreationDate { get; set; }

			public System.Boolean IsDeleted { get; set; }

			public System.Boolean IsActive { get; set; }

			public System.Nullable<System.DateTime> ReviewDate { get; set; }

	}


	public class RemoteRiskProbability
	{

			public System.Nullable<System.Int32> RiskProbabilityId { get; set; }

			public System.String Name { get; set; }

			public System.Boolean Active { get; set; }

			public System.Int32 Position { get; set; }

			public System.String Color { get; set; }

			public System.Int32 Score { get; set; }

	}


	public class RemoteRiskStatus
	{

			public System.Int32 RiskStatusId { get; set; }

			public System.String Name { get; set; }

			public System.Boolean Active { get; set; }

			public System.Int32 Position { get; set; }

	}


	public class RemoteRiskType
	{

			public System.Int32 RiskTypeId { get; set; }

			public System.String Name { get; set; }

			public System.Nullable<System.Int32> WorkflowId { get; set; }

			public System.Boolean IsActive { get; set; }

			public System.Int32 Position { get; set; }

			public System.Boolean IsDefault { get; set; }

	}


	public class RemoteSavedFilter
	{

			public System.Int32 SavedFilterId { get; set; }

			public System.Int32 ProjectId { get; set; }

			public System.Int32 ArtifactTypeId { get; set; }

			public System.String Name { get; set; }

			public System.Boolean IsShared { get; set; }

			public RemoteSort Sort { get; set; }

			public System.Collections.Generic.List<RemoteFilter> Filters { get; set; }

			public System.String ArtifactTypeName { get; set; }

			public System.String ProjectName { get; set; }

	}


	public class RemoteSourceCodeBranch
	{

			public System.String Id { get; set; }

			public System.String Name { get; set; }

			public System.Boolean IsDefault { get; set; }

	}


	public class RemoteSourceCodeConnection
	{

			public System.String ProviderName { get; set; }

			public System.String Connection { get; set; }

			public System.String Login { get; set; }

			public System.String Password { get; set; }

	}


	public class RemoteSourceCodeFile
	{

			public System.String Id { get; set; }

			public System.String Name { get; set; }

			public RemoteSourceCodeFolder ParentFolder { get; set; }

			public System.Int32 Size { get; set; }

			public System.String AuthorName { get; set; }

			public RemoteSourceCodeRevision LastRevision { get; set; }

			public System.DateTime LastUpdateDate { get; set; }

			public System.String Action { get; set; }

			public System.String Path { get; set; }

			public System.Collections.Generic.List<RemoteLinkedArtifact> LinkedArtifacts { get; set; }

	}


	public class RemoteSourceCodeFolder
	{

			public System.String Id { get; set; }

			public System.String Name { get; set; }

			public RemoteSourceCodeFolder ParentFolder { get; set; }

			public System.Collections.Generic.List<RemoteSourceCodeFile> Files { get; set; }

			public System.Collections.Generic.List<RemoteSourceCodeFolder> Folders { get; set; }

			public System.Boolean IsRoot { get; set; }

	}


	public class RemoteSourceCodeRevision
	{

			public System.String Id { get; set; }

			public System.String Name { get; set; }

			public System.String AuthorName { get; set; }

			public System.String Message { get; set; }

			public System.DateTime UpdateDate { get; set; }

			public System.Boolean ContentChanged { get; set; }

			public System.Boolean PropertiesChanged { get; set; }

			public System.Collections.Generic.List<RemoteSourceCodeFile> Files { get; set; }

	}


	public class RemoteLinkedArtifact
	{

			public System.Int32 ArtifactId { get; set; }

			public System.Int32 ArtifactTypeId { get; set; }

			public System.String Name { get; set; }

			public System.String Status { get; set; }

	}


	public class RemoteArtifactSubscription
	{

			public System.Int32 ArtifactId { get; set; }

			public System.Int32 ArtifactTypeId { get; set; }

			public System.String ArtifactName { get; set; }

			public System.String ArtifactDescription { get; set; }

			public System.String ArtifactTypeName { get; set; }

			public System.Int32 ProjectId { get; set; }

			public System.String ProjectName { get; set; }

			public System.Int32 UserId { get; set; }

			public System.DateTime LastUpdateDate { get; set; }

			public System.String UserFullName { get; set; }

	}


	public class RemoteVersion
	{

			public System.String Version { get; set; }

			public System.Nullable<System.Int32> Patch { get; set; }

	}


	public class RemoteSetting
	{

			public System.String Name { get; set; }

			public System.String Value { get; set; }

	}


	public class RemoteEvent2
	{

			public System.Int32 EventTypeId { get; set; }

			public System.String EventTypeName { get; set; }

			public System.String Message { get; set; }

			public System.String Details { get; set; }

			public System.DateTime EventTimeUtc { get; set; }

			public System.String EventCategory { get; set; }

			public System.Int32 EventCode { get; set; }

			public System.String EventId { get; set; }

	}


	public class RemoteTask
	{

			public System.Nullable<System.Int32> TaskId { get; set; }

			public System.Int32 TaskStatusId { get; set; }

			public System.Nullable<System.Int32> TaskTypeId { get; set; }

			public System.Nullable<System.Int32> TaskFolderId { get; set; }

			public System.Nullable<System.Int32> RequirementId { get; set; }

			public System.Nullable<System.Int32> ReleaseId { get; set; }

			public System.Nullable<System.Int32> ComponentId { get; set; }

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

			public System.String TaskTypeName { get; set; }

			public System.String OwnerName { get; set; }

			public System.String TaskPriorityName { get; set; }

			public System.String ProjectName { get; set; }

			public System.String ReleaseVersionNumber { get; set; }

			public System.String RequirementName { get; set; }

			public System.Nullable<System.Int32> RiskId { get; set; }

			public System.Int32 ProjectId { get; set; }

			public System.Int32 ArtifactTypeId { get; set; }

			public System.DateTime ConcurrencyDate { get; set; }

			public System.Collections.Generic.List<RemoteArtifactCustomProperty> CustomProperties { get; set; }

			public System.Boolean IsAttachments { get; set; }

			public System.String Tags { get; set; }

	}


	public class RemoteTaskFolder
	{

			public System.Nullable<System.Int32> TaskFolderId { get; set; }

			public System.Int32 ProjectId { get; set; }

			public System.String IndentLevel { get; set; }

			public System.String Name { get; set; }

			public System.Nullable<System.Int32> ParentTaskFolderId { get; set; }

	}


	public class RemoteTaskPriority
	{

			public System.Nullable<System.Int32> PriorityId { get; set; }

			public System.String Name { get; set; }

			public System.Boolean Active { get; set; }

			public System.String Color { get; set; }

			public System.Nullable<System.Int32> Score { get; set; }

	}


	public class RemoteTaskStatus
	{

			public System.Int32 TaskStatusId { get; set; }

			public System.String Name { get; set; }

			public System.Boolean Active { get; set; }

			public System.Int32 Position { get; set; }

	}


	public class RemoteTaskType
	{

			public System.Int32 TaskTypeId { get; set; }

			public System.String Name { get; set; }

			public System.Nullable<System.Int32> WorkflowId { get; set; }

			public System.Boolean IsActive { get; set; }

			public System.Int32 Position { get; set; }

			public System.Boolean IsDefault { get; set; }

			public System.Boolean IsCodeReview { get; set; }

			public System.Boolean IsPullRequest { get; set; }

	}


	public class RemoteTestCase
	{

			public System.Nullable<System.Int32> TestCaseId { get; set; }

			public System.Nullable<System.Int32> ExecutionStatusId { get; set; }

			public System.Nullable<System.Int32> AuthorId { get; set; }

			public System.Nullable<System.Int32> OwnerId { get; set; }

			public System.Nullable<System.Int32> TestCasePriorityId { get; set; }

			public System.Nullable<System.Int32> TestCaseTypeId { get; set; }

			public System.Int32 TestCaseStatusId { get; set; }

			public System.Nullable<System.Int32> TestCaseFolderId { get; set; }

			public System.Collections.Generic.List<System.Int32> ComponentIds { get; set; }

			public System.Nullable<System.Int32> AutomationEngineId { get; set; }

			public System.Nullable<System.Int32> AutomationAttachmentId { get; set; }

			public System.String Name { get; set; }

			public System.String Description { get; set; }

			public System.DateTime CreationDate { get; set; }

			public System.DateTime LastUpdateDate { get; set; }

			public System.Nullable<System.DateTime> ExecutionDate { get; set; }

			public System.Nullable<System.Int32> EstimatedDuration { get; set; }

			public System.String AuthorName { get; set; }

			public System.String OwnerName { get; set; }

			public System.String ProjectName { get; set; }

			public System.String TestCasePriorityName { get; set; }

			public System.String TestCaseStatusName { get; set; }

			public System.String TestCaseTypeName { get; set; }

			public System.String ExecutionStatusName { get; set; }

			public System.Collections.Generic.List<RemoteTestStep> TestSteps { get; set; }

			public System.Nullable<System.Int32> ActualDuration { get; set; }

			public System.Boolean IsSuspect { get; set; }

			public System.Boolean IsTestSteps { get; set; }

			public System.Int32 ProjectId { get; set; }

			public System.Int32 ArtifactTypeId { get; set; }

			public System.DateTime ConcurrencyDate { get; set; }

			public System.Collections.Generic.List<RemoteArtifactCustomProperty> CustomProperties { get; set; }

			public System.Boolean IsAttachments { get; set; }

			public System.String Tags { get; set; }

	}


	public class RemoteTestCaseFolder
	{

			public System.Nullable<System.Int32> TestCaseFolderId { get; set; }

			public System.Int32 ProjectId { get; set; }

			public System.String IndentLevel { get; set; }

			public System.String Name { get; set; }

			public System.String Description { get; set; }

			public System.DateTime LastUpdateDate { get; set; }

			public System.Nullable<System.DateTime> ExecutionDate { get; set; }

			public System.Nullable<System.Int32> EstimatedDuration { get; set; }

			public System.String ProjectName { get; set; }

			public System.Nullable<System.Int32> ParentTestCaseFolderId { get; set; }

			public System.Nullable<System.Int32> ActualDuration { get; set; }

			public System.Int32 CountBlocked { get; set; }

			public System.Int32 CountCaution { get; set; }

			public System.Int32 CountFailed { get; set; }

			public System.Int32 CountNotApplicable { get; set; }

			public System.Int32 CountNotRun { get; set; }

			public System.Int32 CountPassed { get; set; }

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


	public class RemoteTestCasePriority
	{

			public System.Nullable<System.Int32> PriorityId { get; set; }

			public System.String Name { get; set; }

			public System.Boolean Active { get; set; }

			public System.String Color { get; set; }

			public System.Nullable<System.Int32> Score { get; set; }

	}


	public class RemoteTestCaseStatus
	{

			public System.Int32 TestCaseStatusId { get; set; }

			public System.String Name { get; set; }

			public System.Boolean Active { get; set; }

			public System.Int32 Position { get; set; }

	}


	public class RemoteTestCaseType
	{

			public System.Int32 TestCaseTypeId { get; set; }

			public System.String Name { get; set; }

			public System.Nullable<System.Int32> WorkflowId { get; set; }

			public System.Boolean IsActive { get; set; }

			public System.Int32 Position { get; set; }

			public System.Boolean IsDefault { get; set; }

			public System.Boolean IsExploratory { get; set; }

			public System.Boolean IsBdd { get; set; }

	}


	public class RemoteTestConfigurationSet
	{

			public System.Int32 TestConfigurationSetId { get; set; }

			public System.Boolean IsActive { get; set; }

			public System.DateTime CreationDate { get; set; }

			public System.DateTime LastUpdatedDate { get; set; }

			public System.DateTime ConcurrencyDate { get; set; }

			public System.Int32 ProjectId { get; set; }

			public System.String Name { get; set; }

			public System.String Description { get; set; }

			public System.Collections.Generic.List<RemoteTestConfigurationEntry> Entries { get; set; }

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

			public System.Nullable<System.Int32> TestConfigurationId { get; set; }

			public System.Int32 ProjectId { get; set; }

			public System.Int32 ArtifactTypeId { get; set; }

			public System.DateTime ConcurrencyDate { get; set; }

			public System.Collections.Generic.List<RemoteArtifactCustomProperty> CustomProperties { get; set; }

			public System.Boolean IsAttachments { get; set; }

			public System.String Tags { get; set; }

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

			public System.Nullable<System.Int32> TestSetFolderId { get; set; }

			public System.Nullable<System.Int32> EstimatedDuration { get; set; }

			public System.Nullable<System.Int32> ActualDuration { get; set; }

			public System.Boolean IsAutoScheduled { get; set; }

			public System.Boolean IsDynamic { get; set; }

			public System.String DynamicQuery { get; set; }

			public System.Nullable<System.Int32> TestConfigurationSetId { get; set; }

			public System.Nullable<System.Int32> BuildExecuteTimeInterval { get; set; }

			public System.Int32 ProjectId { get; set; }

			public System.Int32 ArtifactTypeId { get; set; }

			public System.DateTime ConcurrencyDate { get; set; }

			public System.Collections.Generic.List<RemoteArtifactCustomProperty> CustomProperties { get; set; }

			public System.Boolean IsAttachments { get; set; }

			public System.String Tags { get; set; }

	}


	public class RemoteTestSetFolder
	{

			public System.Nullable<System.Int32> TestSetFolderId { get; set; }

			public System.Int32 ProjectId { get; set; }

			public System.String IndentLevel { get; set; }

			public System.String Name { get; set; }

			public System.String Description { get; set; }

			public System.DateTime CreationDate { get; set; }

			public System.DateTime LastUpdateDate { get; set; }

			public System.Nullable<System.DateTime> ExecutionDate { get; set; }

			public System.Nullable<System.Int32> EstimatedDuration { get; set; }

			public System.String ProjectName { get; set; }

			public System.Nullable<System.Int32> ParentTestSetFolderId { get; set; }

			public System.Nullable<System.Int32> ActualDuration { get; set; }

			public System.Int32 CountBlocked { get; set; }

			public System.Int32 CountCaution { get; set; }

			public System.Int32 CountFailed { get; set; }

			public System.Int32 CountNotApplicable { get; set; }

			public System.Int32 CountNotRun { get; set; }

			public System.Int32 CountPassed { get; set; }

	}


	public class RemoteTestSetParameter
	{

			public System.Int32 TestSetId { get; set; }

			public System.Int32 TestCaseParameterId { get; set; }

			public System.String Name { get; set; }

			public System.String Value { get; set; }

	}


	public class RemoteTestSetTestCaseMapping
	{

			public System.Int32 TestSetTestCaseId { get; set; }

			public System.Int32 TestSetId { get; set; }

			public System.Int32 TestCaseId { get; set; }

			public System.Nullable<System.Int32> OwnerId { get; set; }

			public System.Nullable<System.DateTime> PlannedDate { get; set; }

			public System.Boolean IsSetupTeardown { get; set; }

	}


	public class RemoteTestSetTestCaseParameter
	{

			public System.Int32 TestSetTestCaseId { get; set; }

			public System.Int32 TestCaseParameterId { get; set; }

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

			public System.String Precondition { get; set; }

			public System.Int32 ProjectId { get; set; }

			public System.Int32 ArtifactTypeId { get; set; }

			public System.DateTime ConcurrencyDate { get; set; }

			public System.Collections.Generic.List<RemoteArtifactCustomProperty> CustomProperties { get; set; }

			public System.Boolean IsAttachments { get; set; }

			public System.String Tags { get; set; }

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