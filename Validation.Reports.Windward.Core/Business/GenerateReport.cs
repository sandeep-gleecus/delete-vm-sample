using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web.UI.WebControls;
using net.windward.api.csharp;

using Validation.Reports.Windward.Core.Model;

using WindwardInterfaces.net.windward.api.csharp;


namespace Validation.Reports.Windward.Core.Business
{
	public class GenerateReport
	{
		//        public async Task<Stream> GetReport(TemplateModel templateData, string appPath, string userName, string outputType = "")

		public Stream GetReport(ReportRequest templateData, string appPath, string userName, string outputType = "")
		{
			//string RiskSummaryReportName = "New_Risk_Summary_Report";
			using (System.IO.StreamWriter reportFile = new System.IO.StreamWriter(@"C:\Program Files (x86)\ValidationMaster\reporting\output\GenerateErrorLog.txt", true))
			{
				reportFile.WriteLine(" -- Get Report -- ");

				Stream template = null;

				//Initialize the engine
				Report.Init();
				Report report;
				//Stream reportOutput = null;

				Stream generatedReport = null;
				//var processed = true;
				string reportExtention = "";

				var dataSources = new Dictionary<string, IReportDataSource>();

				try
				{
					//template folder from web.config
					string folder = ConfigurationSettings.AppSettings["TemplateFolder"];
					reportFile.WriteLine(" Folder " + folder);

					if (string.IsNullOrEmpty(folder))
					{
						folder = appPath + "App_Data\\Documents\\Templates\\";
					}

					reportFile.WriteLine(" Folder1 " + folder);

					//Combine folder and file name
					String templateFilePath = templateData.TemplateDefinition.TemplateLocation;
					
					reportFile.WriteLine(" Template FilePath " + templateFilePath);
					//Create data sources dictionary

					//extract file extention. If SelectedOutputType is not null then use the SelectedOutputType else get the report type from file.
					reportExtention = !string.IsNullOrEmpty(outputType) ? outputType : Path.GetExtension(templateFilePath).ToLower();

					reportFile.WriteLine(" Report Extention " + reportExtention);

					//use .NET Engine to run it
					using (var templateFile = System.IO.File.OpenRead(templateFilePath))
					{
						reportFile.WriteLine(" Template File " + templateFile);
						//set output extension to be the same as input extension
						switch (reportExtention)
						{
							case ".pdf":
								report = new ReportPdf(templateFile);
								break;
							case ".eml":
								report = new ReportHtml(templateFile);
								((ReportHtml)report).SetImagePath(appPath + "images", "images", "wr_");
								break;
							case ".html":
								report = new ReportHtml(templateFile);
								((ReportHtml)report).SetImagePath(appPath + "images", "images", "wr_");
								break;
							case ".docx":
								report = new ReportDocx(templateFile);
								break;
							case ".xlsx":
								report = new ReportXlsx(templateFile);
								break;
							default:
								throw new Exception("Unsupported file format.");
						}
						//report.Timeout = 500;
						report.ProcessSetup();

						reportFile.WriteLine(" Template DataSource " + templateData.TemplateDefinition.TST_TEMPLATE_DATASOURCE);

						//Add Data Sources to the Mapping
						foreach (var data in templateData.TemplateDefinition.TST_TEMPLATE_DATASOURCE)
						{
							reportFile.WriteLine(" Data " + data);
							#region Ado Data Sources

							//IF SQL (provider System.Data.OleDb or System.Data.SqlClient or System.Data.Odbc ...)
							if (data.Type == "AdoDataSourceInfo" || data.ProviderClass == "System.Data.SqlClient" || data.ProviderClass == "System.Data.Odbc")
							{
								reportFile.WriteLine(" Data Type " + data.Type);
								reportFile.WriteLine(" Provider Class " + data.ProviderClass);
								// Place all variables in this map. We assign this map to all datasources.
								Dictionary<string, object> mapVariables = new Dictionary<string, object>();
								// Replace the specified values with the values you want each time you run the report.
								//These are report template Parameters/Variables, insert the values you want to use
								if (templateData.Variables != null)
								{
									reportFile.WriteLine(" Report Processing data");
									foreach (var item in templateData.Variables)
									{
										foreach (var variableNameThing in templateData.TemplateDefinition.TST_TEMPLATE_PARAMETER)
										{
											if (variableNameThing.ParameterLabel.ToLower() == item.VariableName.ToLower())
											{
												reportFile.WriteLine(" Variables");
												//string variableName = templateData.TemplateDefinition.TST_TEMPLATE_PARAMETER[0].ParameterLabel.ToString();
												string variableName = variableNameThing.ParameterLabel;
												string value = item.DefaultValue;
												if (item.VariableName == "WeekEndingDate")
													mapVariables.Add(variableName, DateTime.Parse(value));
												else
													if (!mapVariables.ContainsKey(variableName))
												{
													reportFile.WriteLine(" Variables--" + variableName);
													//if (templateData.TemplateDefinition.TemplateName.ToLower() == RiskSummaryReportName.ToLower())
													if(templateData.Variables.Count > 0)
													{
														//if (variableName.ToLower() == "FilePath".ToLower())
														//{
														//	string RiskSummaryImagePath = "c:\\Program Files (x86)\\ValidationMaster\\Attachments\\RiskSummary\\";
														//	mapVariables.Add(variableName, RiskSummaryImagePath + "image.jpg");
														//}
														if (variableName.ToLower() == "MatrixType".ToLower())
														{
															if (value.ToLower() == "3")
															{
																value = "3x3";
															}
															if (value.ToLower() == "4")
															{
																value = "4x4";
															}
															if (value.ToLower() == "5")
															{
																value = "5x5";
															}

															//string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(templateData.TemplateDefinition.TemplateLocation);

															//bool startsWithValidationMaster = fileNameWithoutExtension.StartsWith("ValidationMaster Risk Assessment Template");

															//if (startsWithValidationMaster)
															//{
															//	mapVariables.Add(variableName, value);
															//	string RiskSummaryImagePath = "c:\\Program Files (x86)\\ValidationMaster\\Attachments\\RiskSummary\\";
															//	mapVariables.Add("FilePath", RiskSummaryImagePath + "image.jpg");
															//}
															//else
															//{
																mapVariables.Add(variableName, value);
															//}
														}
														else
														{
															mapVariables.Add(variableName, value);
															reportFile.WriteLine(" mapVariables--" + mapVariables);
														}
													}
													else
													{
														mapVariables.Add(variableName, value);
													}

												}

											}
										}
									}
								}

								//mapVariables.Add("TestCaseId", 18);

								//mapVariables.Add("FilePath", @"D:\Source\theta\ValidationMaster\ckEditor\samples\img\logo.jpg");
								//mapVariables.Add("FilePath", @"D:\Source\theta\ValidationMaster\Attachments\risksummary\image.jpg");
								reportFile.WriteLine(" Provider Class" + data.ProviderClass);
								reportFile.WriteLine(" Connection String" + data.ConnectionString);
								using (AdoDataSourceImpl VMASTER = new AdoDataSourceImpl(data.ProviderClass, data.ConnectionString))
								{
									reportFile.WriteLine(" Map Variables" + mapVariables);
									VMASTER.Map = mapVariables;
									reportFile.WriteLine(" VMASTER Map" + VMASTER.Map);
									reportFile.WriteLine(" DataName-- " + data.Name);
									reportFile.WriteLine(" DataSource-- " + dataSources);
									dataSources.Add(data.Name, VMASTER);
									reportFile.WriteLine("--DataSource-- " + dataSources);
									// Insert all data into the report.
									report.ProcessData(dataSources);
									reportFile.WriteLine(" Report Processing data");
								}
							}

							#endregion

							//report.ProcessData(dataSources);
						}

						report.ProcessComplete();
						reportFile.WriteLine(" Report Processing data----");
						#region Process Report

						generatedReport = (MemoryStream)report.GetReport();
						reportFile.WriteLine(" Report Processing data Completed");
						#endregion

						return generatedReport;
					}
				}
				#region error handling
				catch (Exception ex)
				{
					using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Program Files (x86)\ValidationMaster\reporting\podfiles\log.txt", true))
					{
						file.WriteLine($"GenerateReport.exception: {ex.Message}");
					}
					throw new Exception(ex.InnerException.Message);
				}
				#endregion

				#region finnaly - close the connections
				// close everything
				finally
				{
					if (template != null)
						template.Close();

					//report.Close();

					#region Audit
					//Add audit Record
					#endregion
				}
			}
			#endregion
		}		

		public void Generate()
		{
			try
			{
				// Initialize the engine.
				Report.Init();

				// Read the template file.
				using (FileStream template = File.OpenRead(@"C:\Program Files (x86)\ValidationMaster\reporting\templatefiles\TestCaseTest.docx"))
				{
					var dtString = DateTime.Now.ToString("HHmmss");
					// Create the generated report file.
					using (FileStream output = File.Create($@"C:\Program Files (x86)\ValidationMaster\reporting\output\TestCaseTest.{dtString}.pdf"))
					{
						// Pass the 2 streams to the object that will create a PDF report.
						// For other output types, you create a different object at this step.
						using (Report report = new ReportPdf(template, output))
						{
							// Read in the template and prepare it to merge the data
							report.ProcessSetup();

							// Place all variables in this map. We assign this map to all datasources.
							Dictionary<string, object> mapVariables = new Dictionary<string, object>();
							// Replace the specified values with the values you want each time you run the report.
							mapVariables.Add("TestCaseId", 18);
							//report.Parameters = mapVariables;

							IDictionary<string, IReportDataSource> dataSources = new Dictionary<string, IReportDataSource>();
							using (AdoDataSourceImpl VMASTER = new AdoDataSourceImpl("System.Data.SqlClient", @"Data Source=vmaster;Initial Catalog=ValidationMaster;Integrated Security=True"))
							{

								VMASTER.Map = mapVariables;
								dataSources.Add("VMASTER", VMASTER);
								// Insert all data into the report.
								report.ProcessData(dataSources);
							}

							// Write the generated report to the PDF file.
							report.ProcessComplete();

							using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Program Files (x86)\ValidationMaster\reporting\output\ReportTesting_Log.txt", true))
							{
								file.WriteLine($"**************************");
								file.WriteLine($"{output.Name} Created at: {DateTime.Now}");
								file.WriteLine($"**************************");
								file.WriteLine($"{Environment.NewLine}");
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Program Files (x86)\ValidationMaster\reporting\output\ReportTesting_Log.txt", true))
				{
					file.WriteLine($"ReportGenerator.Generate()");
					file.WriteLine($"Occured at: {DateTime.Now}");
					file.WriteLine($"GenerateReport.exception: {ex.Message}");
					if (ex.InnerException != null)
						file.WriteLine($"GenerateReport.InnerException: {ex.InnerException.Message}");
					file.WriteLine($"{Environment.NewLine}");
				}

			}
		}


	}
}
