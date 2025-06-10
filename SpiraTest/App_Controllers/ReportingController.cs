using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;
using Validation.Reports.Windward.Core.Model;

namespace Inflectra.SpiraTest.Web.App_Controllers
{
	[AllowAnonymous]
	[RoutePrefix("api/ReportApi")]
	public class ReportingController : ApiController
	{
		
		[HttpGet]
		[Route("all")]
		public IEnumerable<string> Get()
		{
			return new string[] { "value1", "value2" };
		}


		[Route("GetTemplateByID/{id}")]
		[HttpGet]
		public HttpResponseMessage GetAllTemplates(int id)
		{
			try
			{
				var template = ReportingTemplateManager.GetTemplateById(id);


				if (template == null)
				{
					throw new Exception("Requested template is not found.");
				}
				//remove connection string from cliend side calls
				template.TST_TEMPLATE_DATASOURCE.Clear();



				return Request.CreateResponse(HttpStatusCode.OK, template);
			}
			catch (Exception ex)
			{
				return Request.CreateResponse(HttpStatusCode.ExpectationFailed, ex.Message);
			}
		}

		[Route("GenerateReport/{templateId}/{variables}/{outputType}")]
		[HttpGet]
		public HttpResponseMessage GenerateReport(int? templateId, string variables, string outputType)
		{
			HttpResponseMessage result = null;
			Stream report = null;
			try
			{

				if (templateId == null)
				{
					throw new Exception("Template Id is not in correct format.");
				}
				Validation.Reports.Windward.Core.Business.GenerateReport reportModel = new Validation.Reports.Windward.Core.Business.GenerateReport();

				var selectedTemplate = ReportingTemplateManager.GetTemplateById(templateId.Value);


				List<Validation.Reports.Windward.Core.Model.RequestVariable> parameters = new List<Validation.Reports.Windward.Core.Model.RequestVariable>();

				NameValueCollection parseVariables = null;

				if (!string.IsNullOrEmpty(variables))
				{
					parseVariables = HttpUtility.ParseQueryString(variables);
				}

				foreach (var item in selectedTemplate.TST_TEMPLATE_PARAMETER)
				{
					//item.DefaultValue = parseVariables.Get(item.ParameterLabel);

					parameters.Add(new Validation.Reports.Windward.Core.Model.RequestVariable() { VariableName = item.ParameterLabel, DefaultValue = "DefaultValueTest" });
				}

				Validation.Reports.Windward.Core.Model.ReportRequest reportRequest = new Validation.Reports.Windward.Core.Model.ReportRequest()
				{
					TemplateDefinition = selectedTemplate,
					Variables = parameters
				};
				string templateFilePath = ConfigurationManager.AppSettings["templateFolder"].ToString();


				string fileExtension = "";
				string contentType = "";

				switch (outputType.ToLower())
				{
					case "pdf":
						fileExtension = ".pdf";
						contentType = "application/pdf";
						break;
					case "word":
						fileExtension = ".docx";
						contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
						break;
					case "html":
						fileExtension = ".html";
						contentType = "text/html";
						break;

				}
				//using (Stream report = (MemoryStream)reportModel.GetReport(reportRequest, HttpContext.Current.Request.PhysicalApplicationPath, "", fileExtension))
				//{

				// get the output and display it
				//Response.ContentType = contentType;
				//Response.BinaryWrite(((MemoryStream)report).ToArray());

				//string header = string.Format( "inline; filename={0}_{1}.doc", //select inline(open in browser) or attachment (download)
				//                               selectedTemplate.TemplateName,
				//                               DateTime.Now.ToString("yyyy-MM-dd"));
				//Response.Clear();
				//Response.AddHeader("content-disposition", header);
				//Response.ContentType = contentType;

				//report.CopyTo(Response.OutputStream);
				//Response.End();

				report = (MemoryStream)reportModel.GetReport(reportRequest, HttpContext.Current.Request.PhysicalApplicationPath, "", fileExtension);

				string outputName = selectedTemplate.TemplateName + "_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff") + fileExtension;

				result = Request.CreateResponse(HttpStatusCode.OK);
				result.Content = new StreamContent(report);
				result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
				result.Content.Headers.ContentDisposition.FileName = outputName;
				result.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

				return result;
				//}
			}
			catch (Exception ex)
			{
				//throw;
				//using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Program Files (x86)\ValidationMaster\reporting\podfiles\log.txt", true)) { file.WriteLine($"exception: {ex.Message}"); }

				result = Request.CreateResponse(ex);
				return result;
			}
			 
		}

		[Route("GetTestRun")]
		[HttpGet]
		public HttpResponseMessage GetTestRun()
		{
			try
			{
				var ddlList = new List<DropDownData>();
				var ddlData = new DropDownData();

				var testRuns = ReportingTemplateManager.GetTestRuns();

				foreach (var testRun in testRuns)
				{
					ddlData = new DropDownData
					{
						id = testRun.TestRunId.ToString(),
						name = $"{testRun.Name} (TR: {testRun.TestRunId.ToString()})"
					};
					ddlList.Add(ddlData);
				}
				return Request.CreateResponse(HttpStatusCode.OK, ddlList);
			}
			catch (Exception ex)
			{
				return Request.CreateResponse(HttpStatusCode.ExpectationFailed, ex.Message);
			}
		}

		[Route("GetTestSet")]
		[HttpGet]
		public HttpResponseMessage GetTestSet()
		{
			try
			{
				var ddlList = new List<DropDownData>();
				var ddlData = new DropDownData();


				var testSets = ReportingTemplateManager.GetTestRuns();

				foreach (var testSet in testSets)
				{
					ddlData = new DropDownData
					{
						id = testSet.TestSetId.ToString(),
						name = $"{testSet.Name} (TS: {testSet.TestSetId.ToString()})"
					};
					ddlList.Add(ddlData);
				}
				
				return Request.CreateResponse(HttpStatusCode.OK, ddlList);
			}
			catch (Exception ex)
			{
				return Request.CreateResponse(HttpStatusCode.ExpectationFailed, ex.Message);
			}
		}

		[Route("GetTestCase")]
		[HttpGet]
		public HttpResponseMessage GetTestCase()
		{
			try
			{
				var ddlList = new List<DropDownData>();
				var ddlData = new DropDownData();


				var data = ReportingTemplateManager.GetTestCases();

				foreach (var testSet in data)
				{
					ddlData = new DropDownData
					{
						id = testSet.TestCaseId.ToString(),
						name = $"{testSet.Name} (TS: {testSet.TestCaseId.ToString()})"
					};
					ddlList.Add(ddlData);
				}

				return Request.CreateResponse(HttpStatusCode.OK, ddlList);
			}
			catch (Exception ex)
			{
				return Request.CreateResponse(HttpStatusCode.ExpectationFailed, ex.Message);
			}
		}

		[Route("GenerateBatchReport/{templateId}/{variables}/{outputType}/{destination}/{deliveryLocation}/{userLocation}")]
		[HttpGet]
		public HttpResponseMessage GenerateBatchReport(int? templateId, string variables, string outputType, string destination, string deliveryLocation, string userLocation)
		{
			string outputFolder = ConfigurationManager.AppSettings["outputFolder"].ToString();

			try
			{

				using (System.IO.StreamWriter file = new System.IO.StreamWriter($"{outputFolder}InfoUsingLog.txt", true))
				{
					file.WriteLine($@"{Environment.NewLine}Starting Batch Process");
				}

				//if (userLocation.Trim().Length > 0)
				//{
				//    deliveryLocation = $"{deliveryLocation}/{userLocation.Trim()}";
				//}

				var scheduleD = DateTime.Now.ToShortDateString().Replace('/', '-');
				var scheduleT = DateTime.Now.ToShortTimeString();
#pragma warning disable 4014
				PopulateSchedule(templateId, variables, outputType, scheduleD, scheduleT, destination, deliveryLocation, userLocation, "BatchScheduler");
#pragma warning restore 4014

				using (System.IO.StreamWriter file = new System.IO.StreamWriter($"{outputFolder}InfoUsingLog.txt", true))
				{
					file.WriteLine($@"{Environment.NewLine}Completed Batch Process");
				}

				HttpResponseMessage result = new HttpResponseMessage();
				result = Request.CreateResponse(HttpStatusCode.OK);
				return result;
			}
			catch (Exception ex)
			{
				//throw;
				//using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Program Files (x86)\ValidationMaster\reporting\podfiles\log.txt", true)) { file.WriteLine($"exception: {ex.Message}"); }
				using (System.IO.StreamWriter file = new System.IO.StreamWriter($"{outputFolder}InfoUsingLog.txt", true))
				{
					file.WriteLine($@"ERROR: {ex.Message}");
				}

				HttpResponseMessage result = new HttpResponseMessage();
				result = Request.CreateResponse(ex);
				return result;
			}
		}

		[Route("GetProject")]
		[HttpGet]
		public HttpResponseMessage GetProject(string q)
		{
			try
			{
				var ddlList = new List<DropDownData>();
				var ddlData = new DropDownData();
				var projects = ReportingTemplateManager.LikeProjects(q).OrderBy(x => x.Name);

				foreach (var projectDetail in projects)
				{
					ddlData = new DropDownData
					{
						id = projectDetail.ProjectId.ToString(),
						name = $"{projectDetail.Name} (PG: {projectDetail.ProjectId.ToString()})"
					};
					ddlList.Add(ddlData);
				}
				 
				return Request.CreateResponse(HttpStatusCode.OK, ddlList);
			}
			catch (Exception ex)
			{
				return Request.CreateResponse(HttpStatusCode.ExpectationFailed, ex.Message);
			}
		}

		[Route("CreateSchedule/{templateId}/{variables}/{outputType}/{scheduleD}/{scheduleT}/{destination}/{deliveryLocation}/{userFolder}")]
		[HttpGet]
		public HttpResponseMessage PopulateSchedule(int? templateId, string variables, string outputType, string scheduleD, string scheduleT, string destination, string deliveryLocation, string userFolder, string callingApp = "")
		{
			try
			{
				string outputFolder = ConfigurationManager.AppSettings["outputFolder"].ToString();

				#region Input Verification

				if (templateId == null)
				{
					using (System.IO.StreamWriter file = new System.IO.StreamWriter($"{outputFolder}InfoUsingLog.txt", true))
					{
						file.WriteLine($@"{Environment.NewLine}Template Id is not in correct format.");
					}
					throw new Exception("Template Id is not in correct format.");
				}

				if (destination == null)
				{
					using (System.IO.StreamWriter file = new System.IO.StreamWriter($"{outputFolder}InfoUsingLog.txt", true))
					{
						file.WriteLine($@"{Environment.NewLine}Destination is not in correct format.");
					}
					throw new Exception("Destination is not in correct format.");
				}

				if (outputType == null)
				{
					using (System.IO.StreamWriter file = new System.IO.StreamWriter($"{outputFolder}InfoUsingLog.txt", true))
					{
						file.WriteLine($@"{Environment.NewLine}Output Type is not in correct format.");
					}
					throw new Exception("Output Type is not in correct format.");
				}

				if (scheduleD == null)
				{
					using (System.IO.StreamWriter file = new System.IO.StreamWriter($"{outputFolder}InfoUsingLog.txt", true))
					{
						file.WriteLine($@"{Environment.NewLine}Schedule Date is not in correct format.");
					}
					throw new Exception("Schedule Date is not in correct format.");
				}

				if (scheduleT == null)
				{
					using (System.IO.StreamWriter file = new System.IO.StreamWriter($"{outputFolder}InfoUsingLog.txt", true))
					{
						file.WriteLine($@"{Environment.NewLine}Schedule Time is not in correct format.");
					}
					throw new Exception("Schedule Time is not in correct format.");
				}

				if (deliveryLocation == null)
				{
					using (System.IO.StreamWriter file = new System.IO.StreamWriter($"{outputFolder}InfoUsingLog.txt", true))
					{
						file.WriteLine($@"{Environment.NewLine}Delivery Location is not in correct format.");
					}
					throw new Exception("Delivery Location is not in correct format.");
				}

				if (!(destination.ToLower().Contains("sharepointonline") || destination.ToLower().Contains("sharepointonprem")))
				{
					using (System.IO.StreamWriter file = new System.IO.StreamWriter($"{outputFolder}InfoUsingLog.txt", true))
					{
						file.WriteLine($@"{Environment.NewLine}Destination of {destination} is not a valid selection.");
					}
					throw new Exception($"Destination of {destination} is not a valid selection.");
				}

				if (variables.Trim().Length == 0) return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Variables are not in valid format or don't exist.");

				#endregion

				var selectedTemplate = ReportingTemplateManager.GetTemplateById(templateId.Value);

				var scheduledDate = DateTime.Parse(scheduleD);
				var scheduleTime = Convert.ToDateTime(scheduleT);
				TimeSpan st = scheduleTime.TimeOfDay;
				DateTime scheduledDateTime = scheduledDate + st;

				if (variables.ToLower().Contains("testsetid"))
				{
					var parameterList = ProcessVariables(variables);
					var scheduleGroup = Guid.NewGuid();

					//Build Schedule for each parameter with the same ScheduleGroup.
					foreach (var parameter in parameterList)
					{
						var fn = GetFileName(parameter);

						var splitParameter = parameter.Split('=');

						if (splitParameter[0].ToLower().Contains("testsetid"))
						{
							var testSetId = Convert.ToInt32(splitParameter[1]);
							using (System.IO.StreamWriter file = new System.IO.StreamWriter($"{outputFolder}InfoUsingLog.txt", true))
							{
								file.WriteLine($@"Contains TestSetId: {testSetId}");
							}


							var testRuns = ReportingTemplateManager.GetTestRuns().Where(x => x.TestSetId == testSetId && x.ExecutionStatusId < 3);

							using (System.IO.StreamWriter file = new System.IO.StreamWriter($"{outputFolder}InfoUsingLog.txt", true))
							{
								file.WriteLine($@"TestRuns where EXECUTION_STATUS_ID < 3:  {testRuns.Count()}");

								int i = 0;

								foreach (var testRun in testRuns)
								{
									i++;
									file.WriteLine($@"{Environment.NewLine}Starting: {i}");
									var parameterJSON = "[{\"Name\":\"TemplateId\",\"Value\":\"" + templateId.Value + "\"},{\"Name\":\"TestRunID\",\"Value\":\"" + testRun.TestRunId + "\"}]";
									file.WriteLine($@"parameterJSON: {parameterJSON}");

									file.WriteLine($@"CallingApp: {callingApp ?? "Validation Master Scheduler"}");
									file.WriteLine($@"CreatedDate: {DateTime.Now}");
									file.WriteLine($@"DeliveryLocation: {deliveryLocation}");
									file.WriteLine($@"UserFolder: {userFolder}");
									file.WriteLine($@"DeliveryType: {destination}");
									file.WriteLine($@"OutputType: {outputType}");
									file.WriteLine($@"Parameters: {parameterJSON}");
									file.WriteLine($@"ScheduledTime: {scheduledDateTime}");
									file.WriteLine($@"Status: Ready");
									file.WriteLine($@"ScheduleGroupId: {scheduleGroup}");
									file.WriteLine($@"TemplateId: {templateId.Value}");
									file.WriteLine($@"TemplateName: {selectedTemplate.TemplateName}");
									//var outputFileName = string.IsNullOrEmpty(testRun.NAME) ? string.Empty : testRun.NAME;
									file.WriteLine($@"OutputFileName: {fn}");

									var schedule = new Schedule
									{
										//CallingApp = callingApp.Length == 0 ? "Validation Master Scheduler" : callingApp,
										CallingApp = callingApp ?? "Validation Master Scheduler",
										CreatedDate = DateTime.Now,
										DeliveryLocation = deliveryLocation,
										//UserFolder = userFolder,
										DeliveryType = destination,
										OutputType = outputType,
										Parameters = parameterJSON,
										ScheduledTime = scheduledDateTime,
										Status = "Ready",
										ScheduleGroupId = scheduleGroup,
										TST_TEMPLATE = selectedTemplate,
										TemplateName = selectedTemplate.TemplateName,
										//OutputFileName = fn PCS
									};

									file.WriteLine($@"schedule_{i}");
									ReportingTemplateManager.AddSchedule(schedule);
									file.WriteLine($@"Saved to DB: {schedule.TemplateName}");
								}
							}
						}
					}

				}
				else
				{
					var parameterList = ProcessVariables(variables);
					var scheduleGroup = Guid.NewGuid();


					using (System.IO.StreamWriter file = new System.IO.StreamWriter($"{outputFolder}InfoUsingLog.txt", true))
					{
						int i = 0;

						//Build Schedule for each parameter with the same ScheduleGroup.
						foreach (var parameter in parameterList)
						{
							var fn = GetFileName(parameter);
							i++;
							var splitParameter = parameter.Split('=');
							var parameterJSON = "[{\"Name\":\"TemplateId\",\"Value\":\"" + templateId.Value + "\"},{\"Name\":\"" + splitParameter[0] + "\",\"Value\":\"" + splitParameter[1] + "\"}]";

							file.WriteLine($@"{Environment.NewLine}Starting: {i}");
							file.WriteLine($@"parameterJSON: {parameterJSON}");

							var outputFileName = string.Empty; // string.IsNullOrEmpty(testRun.NAME) ? string.Empty : testRun.NAME;
							file.WriteLine($@"OutputFileName: {fn}");

							var schedule = new Schedule
							{
								CallingApp = callingApp.Length == 0 ? "Validation Master Scheduler" : callingApp,
								CreatedDate = DateTime.Now,
								DeliveryLocation = deliveryLocation,
								DeliveryType = destination,
								//UserFolder = userFolder,
								OutputType = outputType,
								Parameters = parameterJSON,
								ScheduledTime = scheduledDateTime,
								Status = "Ready",
								ScheduleGroupId = scheduleGroup,
								TemplateId = templateId.Value,
								TemplateName = selectedTemplate.TemplateName,
								//OutputFileName = fn
							};

							file.WriteLine($@"schedule_{i}");
							ReportingTemplateManager.AddSchedule(schedule);
							file.WriteLine($@"Saved to DB: {schedule.TemplateName}");
						}
					}
				}

				return Request.CreateResponse(HttpStatusCode.OK);
			}
			catch (Exception ex)
			{

				var messages = new List<string>();
				do
				{
					messages.Add(ex.Message);
					ex = ex.InnerException;
				}
				while (ex != null);
				var message = string.Join(" - ", messages);

				string outputFolder = ConfigurationManager.AppSettings["outputFolder"].ToString();
				using (System.IO.StreamWriter file = new System.IO.StreamWriter($"{outputFolder}InfoUsingLog.txt", true))
				{
					file.WriteLine($@"populateSchedule Error: {message}");
				}
				return Request.CreateResponse(HttpStatusCode.ExpectationFailed, message);
			}
		}

		private List<string> ProcessVariables(string variables)
		{
			var finalParameterList = new List<string>();
			var hasEqual = variables.IndexOf("=") > 0;
			//var hasComma = variables.IndexOf(",") > 0;
			var hasDash = variables.IndexOf("-") > 0;
			if (hasEqual)
			{
				//get right of =
				var equalIndex = variables.IndexOf("=");
				var parameterName = variables.Substring(0, equalIndex);
				var values = variables.Replace('&', ' ').Substring(equalIndex + 1);
				var arrayVales = values.Split(',');
				finalParameterList.Clear();
				foreach (var val in arrayVales)
				{
					if (!string.IsNullOrEmpty(val))
					{
						hasDash = val.IndexOf("-") > 0;
						if (hasDash)
						{
							var fullRange = val.Trim().Split('-');
							var low = Convert.ToInt32(fullRange[0]);
							var high = Convert.ToInt32(fullRange[1]);
							if (high > low)
							{
								for (int i = low; i <= high; i++)
								{
									finalParameterList.Add($"{parameterName}={i}");
								}
							}
							else
							{
								finalParameterList.Add($"{parameterName}={low}");
								finalParameterList.Add($"{parameterName}={high}");
							}
						}
						else
						{
							finalParameterList.Add($"{parameterName}={val.Trim()}");
						}
					}
				}

				return finalParameterList;
			}
			return new List<string>() { "none" };
		}

		private static string GetFileName(string parameter)
		{
			var fileName = string.Empty;


			var splitParameter = parameter.Split('=');
			var id = Convert.ToInt32(splitParameter[1]);
			var parmName = splitParameter[0];

			//file.WriteLine($@"Contains {parmName}: {id}");


			switch (parmName.ToLower())
			{
				case "testcaseid":
					var testCases = ReportingTemplateManager.GetTestCases().Where(x => x.TestCaseId == id && x.ExecutionStatusId < 3).OrderBy(x => x.Name);
					foreach (var testCase in testCases)
					{
						fileName = string.IsNullOrEmpty(testCase.Name) ? string.Empty : testCase.Name;
					}
					break;
				case "testrunid":
					var testRuns = ReportingTemplateManager.GetTestRuns().Where(x => x.TestRunId == id && x.ExecutionStatusId < 3).OrderBy(x => x.Name);
					foreach (var testRun in testRuns)
					{
						fileName = string.IsNullOrEmpty(testRun.Name) ? string.Empty : testRun.Name;
					}
					break;
				case "testsetid":
					var testSetRuns = ReportingTemplateManager.GetTestRuns().Where(x => x.TestRunId == id && x.ExecutionStatusId < 3).OrderBy(x => x.Name);
					foreach (var testRun in testSetRuns)
					{
						fileName = string.IsNullOrEmpty(testRun.Name) ? string.Empty : testRun.Name;
					}
					break;
				case "incidentid":
					var incidents = ReportingTemplateManager.GetIncidents().Where(x => x.IncidentId == id && !x.IsDeleted).OrderBy(x => x.Name);
					foreach (var incident in incidents)
					{
						fileName = string.IsNullOrEmpty(incident.Name) ? string.Empty : incident.Name;
					}
					break;
				case "projectid":
					var projects = ReportingTemplateManager.GetProjects().Where(x => x.ProjectId == id && x.IsActive).OrderBy(x => x.Name);
					foreach (var project in projects)
					{
						fileName = string.IsNullOrEmpty(project.Name) ? string.Empty : project.Name;
					}
					break;
				default:
					fileName = string.Empty;
					break;
			}
			return fileName;
		}
	}
}
