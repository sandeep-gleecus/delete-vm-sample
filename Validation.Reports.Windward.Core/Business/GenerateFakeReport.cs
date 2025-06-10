//using System.Collections.Generic;
//using System.IO;

//using net.windward.api.csharp;

//using WindwardInterfaces.net.windward.api.csharp;

//namespace Validation.Reports.Windward.Core.Business
//{
//	public class GenerateFakeReport
//	{
//		//		public static void GenerateReport()
//		public void GetReport(Model.ReportRequest templateData, string appPath, string userName, string outputType = "")
//		{

//			// Initialize the engine.
//			Report.Init();

//			// Read the template file.
//			using (FileStream template = File.OpenRead(@"C:\Users\Administrator\Documents\TestCaseTest.docx"))
//			{
//				// Create the generated report file.
//				using (FileStream output = File.Create(@"C:\Users\Administrator\Documents\TestCaseTest.pdf"))
//				{
//					// Pass the 2 streams to the object that will create a PDF report.
//					// For other output types, you create a different object at this step.
//					using (Report myReport = new ReportPdf(template, output))
//					{

//						// Read in the template and prepare it to merge the data
//						myReport.ProcessSetup();

//						// Place all variables in this map. We assign this map to all datasources.
//						Dictionary<string, object> mapVariables = new Dictionary<string, object>();
//						// Replace the specified values with the values you want each time you run the report.
//						mapVariables.Add("TestCaseId", 18);
//						myReport.Parameters = mapVariables;

//						IDictionary<string, IReportDataSource> dataSources = new Dictionary<string, IReportDataSource>();
//						using (AdoDataSourceImpl VMASTER = new AdoDataSourceImpl("System.Data.SqlClient", @"Data Source=vmaster;Initial Catalog=ValidationMaster;Integrated Security=True"))
//						{
//							dataSources.Add("VMASTER", VMASTER);

//							// Insert all data into the report.
//							myReport.ProcessData(dataSources);
//						}

//						// Write the generated report to the PDF file.
//						myReport.ProcessComplete();
//					}
//				}
//			}
//		}

//	}
//}

