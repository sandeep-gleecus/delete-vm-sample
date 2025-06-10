using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

using Inflectra.SpiraTest.DataModel;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.TestSuite;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Inflectra.SpiraTest.ApiTestSuite.Rest_Code_Generation;

using NUnit.Framework;
using Inflectra.SpiraTest.AddOns.SpiraTestNUnitAddIn.SpiraTestFramework;


namespace Inflectra.SpiraTest.ApiTestSuite.API_Tests
{
	/// <summary>
	/// This fixture tests that the ODATA APIs let you retrieve data from all entities, it just checks the basic SELECT case
	/// </summary>
	[
	TestFixture,
	SpiraTestConfiguration(InternalRoutines.SPIRATEST_INTERNAL_URL, InternalRoutines.SPIRATEST_INTERNAL_LOGIN, InternalRoutines.SPIRATEST_INTERNAL_PASSWORD, InternalRoutines.SPIRATEST_INTERNAL_PROJECT_ID, InternalRoutines.SPIRATEST_INTERNAL_CURRENT_RELEASE_ID)
	]
	public class OData_ApiTest
	{
		/// <summary>
		/// Need to use a system admin username and API Key for the OData Service
		/// </summary>
		private const string API_KEY_SYSTEM_ADMIN = "{B9050F75-C5E6-4244-8712-FBF20061A976}";

		/// <summary>
		/// How many reportable entities we should have
		/// </summary>
		private const int NUMBER_REPORTABLE_ENTITIES = 62;

		/// <summary>
		/// Tests to make sure that we can enumerate all the reportable entities and query all of them
		/// </summary>
		[Test]
		[SpiraTestCase(2781)]
		public void _01_TestODataApi_SimpleSelectAll()
		{
			//First create the necessary URL for our instance
			string url = Properties.Settings.Default.ODataUrl;

			//Now retrieve the list of services using this endpoint
			RestRequest request = new RestRequest("ODATA");
			request.Credential = new System.Net.NetworkCredential();
			request.Credential.UserName = "administrator";
			request.Credential.Password = "Welcome123$";
			request.Method = "GET";
			request.Url = url;
			request.Headers.Add(new RestHeader() { Name = StandardHeaders.Accept, Value = "application/json" });

			//Authenticate against the web service by calling a simple method
			RestClient restClient = new RestClient(request);
			RestResponse response = restClient.SendRequest();
			if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK" && !String.IsNullOrEmpty(response.Body))
			{
				//Deserialize the data and verify
				JObject jObject = JObject.Parse(response.Body);
				Assert.AreEqual(Properties.Settings.Default.ODataUrl + "/$metadata", jObject["@odata.context"].Value<String>());
				JArray values = jObject["value"].Value<JArray>();
				Assert.AreEqual(NUMBER_REPORTABLE_ENTITIES, values.Count);

				//Loop through and query each of them
				foreach (JObject entitySet in values)
				{
					string entitySetUrl = entitySet["url"].Value<String>();
					QueryEntitySet(entitySetUrl);
				}
			}
			else
			{
				//Throw exception
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}

		/// <summary>
		/// Queries the specified entity set to see if the query can be executed without error
		/// </summary>
		/// <param name="entitySetUrl">The name of the entity set</param>
		private void QueryEntitySet(string entitySetUrl)
		{
			//First create the necessary URL for our instance
			string url = Properties.Settings.Default.ODataUrl + "/" + entitySetUrl;

			//Now retrieve the list of services using this endpoint
			RestRequest request = new RestRequest("ODATA");
			request.Credential = new System.Net.NetworkCredential();
			request.Credential.UserName = "administrator";
			request.Credential.Password = "Welcome123$";
			request.Method = "GET";
			request.Url = url;
			request.Headers.Add(new RestHeader() { Name = StandardHeaders.Accept, Value = "application/json" });

			//Authenticate against the web service by calling a simple method
			RestClient restClient = new RestClient(request);
			RestResponse response = restClient.SendRequest();
			if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK" && !String.IsNullOrEmpty(response.Body))
			{
				//Deserialize the data and verify
				JObject jObject = JObject.Parse(response.Body);
				Assert.AreEqual(Properties.Settings.Default.ODataUrl + "/$metadata#" + entitySetUrl, jObject["@odata.context"].Value<String>());
				JArray values = jObject["value"].Value<JArray>();

				//If we have data, see if it has columns
				if (values.Count > 0)
				{
					JObject entity = values.First().Value<JObject>();
					Assert.IsTrue(entity.Properties().Count() > 0, entitySetUrl);
				}
			}
			else
			{
				//Throw exception
				throw new Exception(response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value);
			}
		}
	}
}
