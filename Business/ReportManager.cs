using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.EntityClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Xsl;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System.Web.UI.WebControls;
using static Inflectra.SpiraTest.DataModel.Artifact;

using Fonet;
using Fonet.Render.Pdf;

using HtmlAgilityPack;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System.Web.UI.HtmlControls;
using System.Web.UI;
using System.Drawing;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>
	/// This class encapsulates all the data access functionality for
	/// creating, saving and retrieving reports in the system
	/// </summary>
	public class ReportManager : ManagerBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Business.ReportManager::";

		protected const int PAGINATION_SIZE = 50;
		protected const int MAX_CUSTOM_QUERY_ROWS = 10000;  //We only return the first 10,000 rows of a custom query

		public const string FORMAT_DATE_TIME_DISPLAY = "{0:G}"; //Use standard general form - so culture specific
		public const string FORMAT_DATE_TIME_XML = "{0:yyyy-MM-ddTHH:mm:ss}";
		public const string FORMAT_TIME_INTERVAL_HOURS = "{0:0.0}h"; //Time interval values (hours only)

		public const string PARSE_DATE_TIME_XML = "yyyy-MM-ddTHH:mm:ss";
		public const string PARSE_DATE_TIME_XML_WITH_TIMEZONE = "yyyy-MM-ddTHH:mm:sszzz";
		public const string REPORT_GENERATED_FOLDER = "SpiraReports";
		public const string REPORT_ENTITY_SQL_FORMAT = "select value R from {0} as R where R.PROJECT_ID = ${ProjectId}";
		public const string REPORT_CONTENT_DISPOSITION_XSL_FO = "xsl-fo";

		protected const int FOLDER_ID_ROOT = 0;    /* Test Cases and Test Sets */

		//Keeps track of API-based requests for generated reports
		private static ConcurrentDictionary<Guid, int> reportGenerationRequests = new ConcurrentDictionary<Guid, int>();
		protected const int GENERATION_ID_IN_PROGRESS = 0;

		//Regular expressions
		private static Regex absoluteAttachmentUrlRegex = new Regex(@"^(?:(?:https|http)://)?[a-zA-Z0-9\._\-]+(?::[0-9]+)?/[a-zA-Z0-9_\-/]*(?:[0-9]+?)/Attachment/([0-9]+?)\.aspx", RegexOptions.IgnoreCase);
		private static Regex relativeAttachmentUrlRegex = new Regex(@"^/[a-zA-Z0-9_\-/]*(?:[0-9]+?)/Attachment/([0-9]+?)\.aspx", RegexOptions.IgnoreCase);

		#region ESQL Custom Reporting

		public IQueryable<T> QueryReportableEntity<T>(string entitySet) where T : class
		{
			const string METHOD_NAME = "QueryReportableEntity";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				SpiraTestEntitiesEx context = new SpiraTestEntitiesEx();
				IQueryable<T> queryable = context.CreateObjectSet<T>(entitySet);
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return queryable;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Helper class that gets the list of reportable entities from the DataModel assembly.
		/// </summary>
		/// <returns></returns>
		public static Dictionary<string, string> GetReportableEntities()
		{
			const string METHOD_NAME = "GetReportableEntities";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				return DataModel.ReportableEntities.ReportableEntitiesManager.GetReportableEntities();
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Executes a Dynamic Entity SQL command, substituting the project id and project group id if necessary
		/// </summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="projectGroupId">The id of the current project group</param>
		/// <param name="sql">The Entity SQL to execute</param>
		/// <param name="numberOfRecords">The max number of records to return (optional)</param>
		/// <param name="releaseId">The id of the release (optional)</param>
		/// <returns>The XML representation of the data</returns>
		public XmlDocument ReportCustomSection_ExecuteSQL(int projectId, int projectGroupId, string sql, UpdateBackgroundProcessStatus updateBackgroundProcessStatus, int progress = 0, int numberOfRecords = Int32.MaxValue, int? releaseId = null)
		{
			const string METHOD_NAME = "ReportCustomSection_ExecuteSQL";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			string esqlQuery = sql;
			try
			{
				//Replace the project id and project group id
				if (projectId > 0)
				{
					esqlQuery = esqlQuery.Replace("${ProjectId}", projectId.ToString(), StringComparison.InvariantCultureIgnoreCase);
				}
				if (projectGroupId > 0)
				{
					esqlQuery = esqlQuery.Replace("${ProjectGroupId}", projectGroupId.ToString(), StringComparison.InvariantCultureIgnoreCase);
				}
				if (releaseId.HasValue && releaseId.Value > 0)
				{
					//We populate two tokens ${ReleaseId} for the release and ${ReleaseAndChildIds} to get a list of releases and sprints
					ReleaseManager releaseManager = new ReleaseManager();
					string releaseList = releaseManager.GetSelfAndIterationList(projectId, releaseId.Value);

					esqlQuery = esqlQuery.Replace("${ReleaseId}", releaseId.Value.ToString(), StringComparison.InvariantCultureIgnoreCase);
					if (!String.IsNullOrEmpty(releaseList))
					{
						esqlQuery = esqlQuery.Replace("${ReleaseAndChildIds}", releaseList, StringComparison.InvariantCultureIgnoreCase);
					}
				}
				else
				{
					//Populate as -1 to avoid a syntax error, that way it just doesn't match anything
					esqlQuery = esqlQuery.Replace("${ReleaseId}", "-1", StringComparison.InvariantCultureIgnoreCase);

					//For this one, we can return all the releases in the project
					ReleaseManager releaseManager = new ReleaseManager();
					string releaseList = String.Join(",", releaseManager.RetrieveByProjectId(projectId, true, true).Select(r => r.ReleaseId));
					esqlQuery = esqlQuery.Replace("${ReleaseAndChildIds}", releaseList, StringComparison.InvariantCultureIgnoreCase);
				}

				//Create the new XML document
				XmlDocument xmlDoc = new XmlDocument();

				//Create the new root-level element
				XmlElement xmlRootNode = xmlDoc.CreateElement("RESULTS");
				xmlDoc.AppendChild(xmlRootNode);

				using (EntityConnection conn = new EntityConnection("name=SpiraTestEntities"))
				{
					//Open the connection
					conn.Open();

					//Execute the command
					using (EntityCommand cmd = new EntityCommand(esqlQuery, conn))
					{
						//Set a larger timeout value (in seconds)
						cmd.CommandTimeout = Common.Properties.Settings.Default.ReportingESQLTimeout;

						using (EntityDataReader dataReader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
						{
							// Iterate through the collection of records (limited to the number specified, if appropriate)
							int count = 0;
							while (dataReader.Read() && count < numberOfRecords)
							{
								if (updateBackgroundProcessStatus != null)
								{
									updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
								}

								//Create the new row
								XmlElement xmlRowNode = xmlDoc.CreateElement("ROW");
								xmlRootNode.AppendChild(xmlRowNode);

								//Get all the columns
								for (int i = 0; i < dataReader.FieldCount; i++)
								{
									string fieldName = dataReader.GetName(i);
									object fieldValue = dataReader[i];

									//Create the new element
									XmlElement xmlFieldNode = xmlDoc.CreateElement(fieldName);
									xmlRowNode.AppendChild(xmlFieldNode);

									//Serialize the data, for dates we need to make sure it's in the correct format
									//that will be localized by the report writer
									if (fieldValue.GetType() == typeof(DateTime))
									{
										DateTime dateTime = (DateTime)fieldValue;
										xmlFieldNode.InnerText = String.Format(FORMAT_DATE_TIME_XML, dateTime);
									}
									else if (fieldValue.GetType() == typeof(DateTime?))
									{
										DateTime? dateTime = (DateTime?)fieldValue;
										if (dateTime.HasValue)
										{
											xmlFieldNode.InnerText = String.Format(FORMAT_DATE_TIME_XML, dateTime.Value);
										}
									}
									else
									{
										xmlFieldNode.InnerText = fieldValue.ToString();
									}
								}

								count++;
							}
						}
					}

					//Close the connection
					conn.Close();
				}

				return xmlDoc;
			}
			catch (EntitySqlException exception)
			{
				//Also log the SQL if we have this exception.
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception, esqlQuery);
				Logger.Flush();
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		/// <summary>
		/// Retrieves a list of active report categories in the system
		/// </summary>
		/// <returns>A list of active report categories, ordered by their display order</returns>
		public List<ReportCategory> RetrieveCategories()
		{
			const string METHOD_NAME = "RetrieveCategories";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ReportCategory> reportCategories;
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.ReportCategories
								where r.IsActive
								orderby r.Position, r.ReportCategoryId
								select r;

					reportCategories = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return reportCategories;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public List<ReportCategory> RetrieveCategoriesWithoutArtifacts(int categoryId)
		{
			const string METHOD_NAME = "RetrieveCategoriesWithoutArtifacts";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ReportCategory> reportCategories;
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.ReportCategories
								where r.IsActive && r.ArtifactTypeId == null && r.ReportCategoryId == categoryId
								orderby r.Position, r.ReportCategoryId
								select r;

					reportCategories = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return reportCategories;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public ReportCategory RetrieveCategoryByCategoryId(int categoryId)
		{
			const string METHOD_NAME = "RetrieveCategoriesWithoutArtifacts";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ReportCategory reportCategories;
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.ReportCategories
								where r.IsActive && r.ReportCategoryId == categoryId
								orderby r.Position, r.ReportCategoryId
								select r;

					reportCategories = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return reportCategories;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of active report categories in the system
		/// </summary>
		/// <returns>A list of active report categories, ordered by their display order</returns>
		public ReportCategory RetrieveCategoryByName(string categoryName)
		{
			const string METHOD_NAME = "RetrieveCategories";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ReportCategory reportCategories;
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.ReportCategories
								where r.Name == categoryName
								orderby r.Position, r.ReportCategoryId
								select r;

					reportCategories = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return reportCategories;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a dataset of reports in a specific category
		/// </summary>
		/// <param name="reportCategoryId">The id of the category we're interested in</param>
		/// <returns>A list of active reports in the specified category ordered by name then id</returns>
		public List<Report> RetrieveByCategoryId(int reportCategoryId)
		{
			const string METHOD_NAME = "RetrieveByCategoryId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<Report> reports;
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.Reports
								where r.ReportCategoryId == reportCategoryId
								orderby r.Name, r.ReportId
								select r;

					reports = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return (reports);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public List<Report> RetrieveActiveByCategoryId(int reportCategoryId)
		{
			const string METHOD_NAME = "RetrieveByCategoryId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<Report> reports;
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.Reports
								where r.ReportCategoryId == reportCategoryId && r.IsActive
								orderby r.Name, r.ReportId
								select r;

					reports = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return (reports);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public Report RetrieveReportByReport(int reportId)
		{
			const string METHOD_NAME = "RetrieveReportByReport";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				Report report;
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.Reports
								where r.ReportId == reportId
								select r;

					report = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return report;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Gets all the (active) standard report sections available in the system
		/// </summary>
		/// <param name="activeOnly">Do we just want active sections (default = true)</param>
		public List<ReportSection> ReportSection_Retrieve(bool activeOnly = true)
		{
			const string METHOD_NAME = "ReportSection_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ReportSection> reportSections;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.ReportSections
								where r.IsActive || !activeOnly
								orderby r.Name, r.ReportSectionId
								select r;

					reportSections = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return reportSections;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Gets a standard report section by its ID
		/// </summary>
		/// <param name="reportSectionId">The id of the report section</param>
		/// <returns>The report section entity, or null if it doesn't exist</returns>
		public ReportSection ReportSection_RetrieveById(int reportSectionId)
		{
			const string METHOD_NAME = "ReportSection_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ReportSection reportSection;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.ReportSections
								where r.ReportSectionId == reportSectionId
								select r;

					reportSection = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return reportSection;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public ReportCustomSection ReportCustomSection_RetrieveById(int reportCustomSectionId)
		{
			const string METHOD_NAME = "ReportCustomSection_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ReportCustomSection reportCustomSection;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.ReportCustomSections
								where r.ReportCustomSectionId == reportCustomSectionId
								select r;

					reportCustomSection = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return reportCustomSection;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Gets the file extension for a specific format
		/// </summary>
		/// <param name="configured_reportFormatId">The id of the format</param>
		/// <returns>The extension, defaults to '.doc' if we don't know</returns>
		public static string GetExtensionForFormat(int? configured_reportFormatId)
		{
			string ext = ".doc";
			if (configured_reportFormatId.HasValue)
			{
				switch ((Report.ReportFormatEnum)(configured_reportFormatId.Value))
				{
					case Report.ReportFormatEnum.Html:
						ext = ".htm";
						break;

					case Report.ReportFormatEnum.MsExcel2003:
					case Report.ReportFormatEnum.MsExcel2007:
						ext = ".xls";
						break;

					case Report.ReportFormatEnum.MsWord2003:
					case Report.ReportFormatEnum.MsWord2007:
						ext = ".doc";
						break;

					case Report.ReportFormatEnum.Pdf:
						ext = ".pdf";
						break;

					case Report.ReportFormatEnum.Xml:
						ext = ".xml";
						break;

					case Report.ReportFormatEnum.MsProj2003:
						ext = ".mpp";
						break;
				}
			}

			return ext;
		}

		/// <summary>
		/// Inserts the new report entity and optionally the formats it's linked to
		/// </summary>
		/// <param name="report">The passed in report entity</param>
		/// <param name="formats">The list of format ids</param>
		/// <returns>The updated Report entity</returns>
		public Report Report_Insert(Report report, List<int> formats = null, int? userId = null, int? adminSectionId = null, string action = null, bool logHistory = true)
		{
			const string METHOD_NAME = "Report_Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			string newValue = "";
			try
			{
				AdminAuditManager adminAuditManager = new AdminAuditManager();
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//See if we need to add any formats
					if (formats != null)
					{
						report.StartTracking();
						foreach (int reportFormatId in formats)
						{
							if (!report.Formats.Any(f => f.ReportFormatId == reportFormatId))
							{
								ReportFormat newReportFormat = new ReportFormat();
								newReportFormat.ReportFormatId = reportFormatId;
								context.ReportFormats.Attach(newReportFormat);
								report.Formats.Add(newReportFormat);
							}
						}
					}

					newValue = report.Name;

					//Add the object to the context
					context.Reports.AddObject(report);

					//Commit the changes
					context.SaveChanges();
				}

				TST_ADMIN_HISTORY_DETAILS_AUDIT details = new TST_ADMIN_HISTORY_DETAILS_AUDIT();
				details.NEW_VALUE = newValue;

				//Log history.
				if (logHistory)
					adminAuditManager.LogCreation1(Convert.ToInt32(userId), Convert.ToInt32(adminSectionId), report.ReportId, action, details, DateTime.UtcNow, ArtifactTypeEnum.Report, "ReportId");

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return report;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}


		public TST_REPORT_DOWNLOADABLE Downloadable_Report_Insert(TST_REPORT_DOWNLOADABLE report)
		{
			const string METHOD_NAME = "Downloadable_Report_Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			string newValue = "";
			try
			{
				AdminAuditManager adminAuditManager = new AdminAuditManager();
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{

					report.StartTracking();

					//Add the object to the context
					context.ReportDownloadable.AddObject(report);

					//Commit the changes
					context.SaveChanges();
				}


				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return report;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public void Downloadable_Report_Update(TST_REPORT_DOWNLOADABLE report)
		{
			const string METHOD_NAME = "Downloadable_Report_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			try
			{
				AdminAuditManager adminAuditManager = new AdminAuditManager();

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					try
					{
						context.ReportDownloadable.ApplyChanges(report);
						context.SaveChanges();
					}
					catch (Exception ex)
					{
						Logger.LogErrorEvent(METHOD_NAME, ex, "Saving Downloadable Report:");
						throw;
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}


		/// <summary>
		/// Updates the report and optionally the formats it's linked to
		/// </summary>
		/// <param name="report">The passed in report entity</param>
		/// <param name="formats">The list of format ids</param>
		public void Report_Update(Report report, List<int> formats = null, int? userId = null, int? adminSectionId = null, string action = null)
		{
			const string METHOD_NAME = "Report_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First apply the changes
					context.Reports.ApplyChanges(report);

					//See if the formats have changed
					if (formats != null)
					{
						report.StartTracking();
						//See if we need to add any formats
						foreach (int reportFormatId in formats)
						{
							if (!report.Formats.Any(f => f.ReportFormatId == reportFormatId))
							{
								ReportFormat newReportFormat = new ReportFormat();
								newReportFormat.ReportFormatId = reportFormatId;
								context.ReportFormats.Attach(newReportFormat);
								report.Formats.Add(newReportFormat);
							}
						}

						//See if we need to remove any formats
						for (int i = 0; i < report.Formats.Count; i++)
						{
							ReportFormat reportFormat = report.Formats[i];
							if (!formats.Contains(reportFormat.ReportFormatId))
							{
								context.ReportFormats.Attach(reportFormat);
								reportFormat.MarkAsDeleted();
							}
						}
					}

					//Apply the changes and save

					context.AdminSaveChanges(userId, report.ReportId, null, adminSectionId, action, true, true, null);
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Makes a copy of a report
		/// </summary>
		/// <param name="sourceReportId">The id of the report being copied</param>
		/// <returns>The id of the new copied report</returns>
		public int Report_Copy(int sourceReportId, int? userId = null)
		{
			const string METHOD_NAME = "Report_Copy";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Retrieve the specified report (including all its collections)
				Report sourceReport = RetrieveById(sourceReportId);

				Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
				string adminSectionName = "Edit Reports";
				var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

				int adminSectionId = adminSection.ADMIN_SECTION_ID;
				if (sourceReport == null)
				{
					throw new ArtifactNotExistsException(String.Format(GlobalResources.Messages.Report_ReportNotExist, sourceReportId));
				}

				int newReportId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the new report with a temporary name
					Report newReport = new Report();
					newReport.Name = (sourceReport.Name + CopiedArtifactNameSuffix).SafeSubstring(0, 50);
					newReport.Description = sourceReport.Description;
					newReport.IsActive = sourceReport.IsActive;
					newReport.ReportCategoryId = sourceReport.ReportCategoryId;
					newReport.Header = sourceReport.Header;
					newReport.Footer = sourceReport.Footer;
					newReport.Token = null; //Always null for user created sections

					//Copy across the formats
					foreach (ReportFormat sourceReportFormat in sourceReport.Formats)
					{
						ReportFormat newReportFormat = new ReportFormat();
						newReportFormat.ReportFormatId = sourceReportFormat.ReportFormatId;
						context.ReportFormats.Attach(newReportFormat);
						newReport.Formats.Add(newReportFormat);
					}

					//Copy across the standard sections
					foreach (ReportSectionInstance sourceSection in sourceReport.SectionInstances)
					{
						ReportSectionInstance newSection = new ReportSectionInstance();
						newSection.ReportSectionId = sourceSection.ReportSectionId;
						newSection.Header = sourceSection.Header;
						newSection.Footer = sourceSection.Footer;
						newSection.Template = sourceSection.Template;
						newReport.SectionInstances.Add(newSection);
					}

					//Copy across the custom sections
					foreach (ReportCustomSection sourceSection in sourceReport.CustomSections)
					{
						ReportCustomSection newSection = new ReportCustomSection();
						newSection.Name = sourceSection.Name;
						newSection.Description = sourceSection.Description;
						newSection.IsActive = sourceSection.IsActive;
						newSection.Query = sourceSection.Query;
						newSection.Template = sourceSection.Template;
						newSection.Header = sourceSection.Header;
						newSection.Footer = sourceSection.Footer;
						newReport.CustomSections.Add(newSection);
					}

					//Persist the new entity
					context.Reports.AddObject(newReport);
					context.SaveChanges();

					newReportId = newReport.ReportId;

					TST_ADMIN_HISTORY_DETAILS_AUDIT details = new TST_ADMIN_HISTORY_DETAILS_AUDIT();
					details.NEW_VALUE = newReport.Name;

					if (userId != null)
					{
						//Log history.
						adminAuditManager.LogCreation1(Convert.ToInt32(userId), Convert.ToInt32(adminSectionId), newReport.ReportId, "Cloned Report", details, DateTime.UtcNow, ArtifactTypeEnum.Report, "ReportId");
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return newReportId;
			}
			catch (ArtifactNotExistsException)
			{
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Deletes a report
		/// </summary>
		/// <param name="reportId">The id of the report being deleted</param>
		/// <remarks>It will throw an exception if you try and delete one of the default reports</remarks>
		public void Report_Delete(int reportId, int? userId = null)
		{
			const string METHOD_NAME = "Report_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First retrieve the report
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.Reports.Include("SavedReport").Include("GeneratedReports")
								where r.ReportId == reportId
								select r;

					Report report = query.FirstOrDefault();
					if (report != null)
					{
						if (String.IsNullOrEmpty(report.Token))
						{
							context.Reports.DeleteObject(report);
							context.SaveChanges();

							Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
							string adminSectionName = "Edit Reports";
							var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

							int adminSectionId = adminSection.ADMIN_SECTION_ID;

							//Add a changeset to mark it as deleted.
							new AdminAuditManager().LogDeletion1((int)userId, report.ReportId, report.Name, adminSectionId, "Report Deleted", DateTime.UtcNow, ArtifactTypeEnum.Report, "ReportId");
						}
						else
						{
							throw new InvalidOperationException(GlobalResources.Messages.Report_CannotDeleteDefaultReport);
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of all the reports in the system
		/// </summary>
		/// <param name="activeOnly">Do we only want active reportd</param>
		/// <returns>A list of reports in the specified category</returns>
		public List<Report> Report_Retrieve(bool activeOnly = true)
		{
			const string METHOD_NAME = "Report_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<Report> reports;
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.Reports.Include("Category")
								where r.IsActive || !activeOnly && r.Category.ArtifactTypeId != null
								orderby r.ReportCategoryId, r.Name, r.ReportId
								select r;

					reports = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return (reports);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves all the reports saved by a particular user
		/// </summary>
		/// <param name="userId">The id of the user we're interested in</param>
		/// <returns>The list of saved reports</returns>
		public List<SavedReportView> RetrieveSaved(int userId)
		{
			const string METHOD_NAME = "RetrieveSavedById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<SavedReportView> savedReports;
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.SavedReportsView
								where r.UserId == userId
								orderby r.ProjectName, r.Name, r.ReportSavedId
								select r;

					savedReports = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return savedReports;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves all the reports in a project that the user can see (shared and individual)
		/// </summary>
		/// <param name="userId">The current user</param>
		/// <param name="projectId">The current project</param>
		/// <param name="includeShared">Do I want to see my reports only or all ones marked as shareable</param>
		/// <returns>The list of saved reports</returns>
		public List<SavedReportView> RetrieveSaved(int userId, int? projectId, bool includeShared, string sorting, Hashtable filters)
		{
			const string METHOD_NAME = "RetrieveSavedById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<SavedReportView> savedReports;
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.SavedReportsView
									//where r.ProjectId == projectId
								select r;

					//Filter By
					if (filters != null && filters.Count > 0)
					{
						string filterBy = string.Empty;
						if (filters["lblProjectName"] != null)
						{
							filterBy = (string)filters["lblProjectName"];
							query = query.Where(p => p.ProjectName.Contains(filterBy));
						}
						if (filters["lblReportName"] != null) //Report Name
						{
							filterBy = (string)filters["lblReportName"];
							query = query.Where(p => p.Name.Contains(filterBy));
						}
						if (filters["lblCreationDate"] != null) //Report Name
						{
							DateTime filterBy1 = Convert.ToDateTime(filters["lblCreationDate"]);
							query = query.Where(p => p.CREATION_DATE.Value.Day == filterBy1.Day && p.CREATION_DATE.Value.Month == filterBy1.Month && p.CREATION_DATE.Value.Year == filterBy1.Year);
						}
						if (filters["ReportFormatId"] != null)
						{
							int filterById = Convert.ToInt32(filters["ReportFormatId"]);
							query = query.Where(p => p.ReportFormatId == filterById);
						}
					}

					//Filter our the project, if needed..
					if (projectId.HasValue)
					{
						query = query.Where(h => h.ProjectId == projectId.Value);
					}

					//Create the appropriate user clause
					if (includeShared)
					{
						//Add on an OR condition to include any saved report that has the shared flag set
						query = query.Where(r => r.UserId == userId || r.IsShared);
					}
					else
					{
						query = query.Where(r => r.UserId == userId);
					}

					//Add the sort
					if (!string.IsNullOrEmpty(sorting))
					{
						if (sorting == "ProjectName ASC")
							query = query.OrderBy(r => r.ProjectName).ThenBy(r => r.ReportSavedId);
						else if (sorting == "ProjectName DESC")
							query = query.OrderByDescending(r => r.ProjectName).ThenBy(r => r.ReportSavedId);
						else if (sorting == "ReportName ASC")
							query = query.OrderBy(r => r.Name).ThenBy(r => r.ReportSavedId);
						else if (sorting == "ReportName DESC")
							query = query.OrderByDescending(r => r.Name).ThenBy(r => r.ReportSavedId);
						else if (sorting == "CreationDate ASC")
							query = query.OrderBy(r => r.CREATION_DATE).ThenBy(r => r.ReportSavedId);
						else if (sorting == "CreationDate DESC")
							query = query.OrderByDescending(r => r.CREATION_DATE).ThenBy(r => r.ReportSavedId);
						else if (sorting == "FormatName ASC")
							query = query.OrderBy(r => r.FormatName).ThenBy(r => r.ReportSavedId);
						else if (sorting == "FormatName DESC")
							query = query.OrderByDescending(r => r.FormatName).ThenBy(r => r.ReportSavedId);
						else
							query = query.OrderBy(r => r.Name).ThenBy(r => r.ReportSavedId);
					}
					else
					{
						query = query.OrderBy(r => r.Name).ThenBy(r => r.ReportSavedId);
					}

					savedReports = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return savedReports;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public List<SavedReportDetails> RetrieveSavedReportDetails(int userId, int reportId, int? projectId, bool includeShared, string sorting, Hashtable filters)
		{
			const string METHOD_NAME = "RetrieveSavedReportDetails";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<SavedReportDetails> savedReports;
				using (SpiraTestEntities context = new SpiraTestEntities())
				{

					//Query for the report data
					var query = from r in context.SavedReports
								join p in context.Projects
								on r.ProjectId equals p.ProjectId
								join f in context.ReportFormats
								on r.ReportFormatId equals f.ReportFormatId
								where r.ReportId == reportId
								select new SavedReportDetails
								{
									ReportSavedId = r.ReportSavedId,
									ProjectId = r.ProjectId,
									ProjectName = p.Name,
									ReportId = r.ReportId,
									Name = r.Name,
									ReportFormatId = r.ReportFormatId,
									FormatName = f.Name,
									CREATION_DATE = r.CREATION_DATE,
									UserId = r.UserId,
								};

					//Filter By
					if (filters != null && filters.Count > 0)
					{
						string filterBy = string.Empty;
						//Project Name
						if (filters["lblProjectName"] != null)
						{
							filterBy = (string)filters["lblProjectName"];
							query = query.Where(p => p.ProjectName.Contains(filterBy));
						}
						if (filters["lblReportName"] != null) //Report Name
						{
							filterBy = (string)filters["lblReportName"];
							query = query.Where(p => p.Name.Contains(filterBy));
						}
						if (filters["lblCreationDate"] != null) //Report Name
						{
							DateTime filterBy1 = Convert.ToDateTime(filters["lblCreationDate"]);
							query = query.Where(p => p.CREATION_DATE.Value.Day == filterBy1.Day && p.CREATION_DATE.Value.Month == filterBy1.Month && p.CREATION_DATE.Value.Year == filterBy1.Year);
						}
						if (filters["ReportFormatId"] != null)
						{
							//Need to make sure that the project id is numeric
							int filterById = Convert.ToInt32(filters["ReportFormatId"]);
							query = query.Where(p => p.ReportFormatId == filterById);
						}
					}

					//Filter our the project, if needed..
					if (projectId.HasValue)
					{
						query = query.Where(h => h.ProjectId == projectId.Value);
					}

					//Create the appropriate user clause

					query = query.Where(r => r.UserId == userId);

					// Calculate the start date for the last 1 month
					DateTime startDate = DateTime.Now.AddMonths(-1);

					// LINQ query to filter data based on the create date
					query = query.Where(item => item.CREATION_DATE >= startDate);

					//Add the sort
					//Add the sort
					if (!string.IsNullOrEmpty(sorting))
					{
						if (sorting == "ProjectName ASC")
							query = query.OrderBy(r => r.ProjectName).ThenBy(r => r.ReportSavedId);
						else if (sorting == "ProjectName DESC")
							query = query.OrderByDescending(r => r.ProjectName).ThenBy(r => r.ReportSavedId);
						else if (sorting == "ReportName ASC")
							query = query.OrderBy(r => r.Name).ThenBy(r => r.ReportSavedId);
						else if (sorting == "ReportName DESC")
							query = query.OrderByDescending(r => r.Name).ThenBy(r => r.ReportSavedId);
						else if (sorting == "CreationDate ASC")
							query = query.OrderBy(r => r.CREATION_DATE).ThenBy(r => r.ReportSavedId);
						else if (sorting == "CreationDate DESC")
							query = query.OrderByDescending(r => r.CREATION_DATE).ThenBy(r => r.ReportSavedId);
						else if (sorting == "FormatName ASC")
							query = query.OrderBy(r => r.FormatName).ThenBy(r => r.ReportSavedId);
						else if (sorting == "FormatName DESC")
							query = query.OrderByDescending(r => r.FormatName).ThenBy(r => r.ReportSavedId);
						else
							query = query.OrderBy(r => r.Name).ThenBy(r => r.ReportSavedId);
					}
					else
					{
						query = query.OrderBy(r => r.Name).ThenBy(r => r.ReportSavedId);
					}

					savedReports = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return savedReports;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}


		/// <summary>
		/// Deletes an existing saved report in the system
		/// </summary>
		/// <param name="reportSavedId">The id of the saved report being deleted</param>
		/// <remarks>Doesn't complain if there is no item to be deleted</remarks>
		public void DeleteSaved(int reportSavedId)
		{
			const string METHOD_NAME = "DeleteSaved";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the saved report
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					var query = from r in context.SavedReports
								where r.ReportSavedId == reportSavedId
								select r;

					SavedReport savedReport = query.FirstOrDefault();
					if (savedReport != null)
					{
						//Delete the saved report
						context.SavedReports.DeleteObject(savedReport);
						context.SaveChanges();
					}
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Inserts a new saved report entry in the system
		/// </summary>
		/// <param name="name">The name of the project</param>
		/// <param name="parameters">The URL querystring encoded set of report parameters</param>
		/// <param name="projectId">The id of the project (optional)</param>
		/// <param name="reportId">The id of the base report being saved</param>
		/// <param name="share">Whether to share the report with the project team</param>
		/// <param name="userId">The user saving the report</param>
		/// <param name="reportFormatId">The id of the format being used</param>
		/// <returns>The newly created saved report</returns>
		public SavedReport InsertSaved(int reportId, int reportFormatId, int userId, int? projectId, string name, string parameters, bool share)
		{
			const string METHOD_NAME = "InsertSaved";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				SavedReport savedReport;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create a new saved report
					savedReport = new SavedReport();
					savedReport.ReportId = reportId;
					savedReport.ReportFormatId = reportFormatId;
					savedReport.UserId = userId;
					savedReport.ProjectId = projectId;
					savedReport.Name = name;
					savedReport.Parameters = parameters;
					savedReport.IsShared = share;
					savedReport.CREATION_DATE = DateTime.Now;

					//Persist the new saved report
					context.SavedReports.AddObject(savedReport);
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return savedReport;

			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a specific saved report
		/// </summary>
		/// <param name="reportSavedId">The id of the saved report</param>
		/// <returns>The saved report record</returns>
		public SavedReportView RetrieveSavedById(int reportSavedId)
		{
			const string METHOD_NAME = "RetrieveSavedById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				SavedReportView savedReport;
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.SavedReportsView
								where r.ReportSavedId == reportSavedId
								select r;

					savedReport = query.FirstOrDefault();
				}

				//If we don't have a record, throw a specific exception (since client will be expecting one record)
				if (savedReport == null)
				{
					throw new ArtifactNotExistsException("Saved Report " + reportSavedId + " doesn't exist in the system.");
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return savedReport;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of report formats
		/// </summary>
		/// <param name="activeOnly">Do we just want active formats</param>
		/// <returns>The list of formats</returns>
		public List<ReportFormat> ReportFormat_Retrieve(bool activeOnly = true)
		{
			const string METHOD_NAME = "ReportFormat_Retrieve";

			List<ReportFormat> reportFormats;

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Now get the format record
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.ReportFormats
								where r.IsActive || !activeOnly
								orderby r.Name, r.ReportFormatId
								select r;

					reportFormats = query.ToList();
				}

				return reportFormats;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public List<ReportFormat> ReportFormatRetrieve()
		{
			const string METHOD_NAME = "ReportFormatRetrieve";

			List<ReportFormat> reportFormats;

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Now get the format record
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.ReportFormats
								where r.IsActive == true
								orderby r.Name, r.ReportFormatId
								select r;

					reportFormats = query.ToList();
				}

				return reportFormats;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a report format by its id
		/// </summary>
		/// <param name="reportFormatId">The id of the format we're interested in</param>
		/// <returns>The Report Format record </returns>
		public ReportFormat RetrieveFormatById(int reportFormatId)
		{
			const string METHOD_NAME = "RetrieveFormatById";

			ReportFormat reportFormat;

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Now get the format record
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.ReportFormats
								where r.ReportFormatId == reportFormatId
								select r;

					reportFormat = query.FirstOrDefault();
				}

				//If we don't have a format, throw a specific exception (since client will be expecting one record)
				if (reportFormat == null)
				{
					throw new ArtifactNotExistsException("Report Format " + reportFormatId.ToString() + " doesn't exist in the system.");
				}

				return reportFormat;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a specific report by its id, including associated formats, sections and elements
		/// </summary>
		/// <param name="reportId">The id of the report we're interested in</param>
		/// <param name="includeInactiveSections">Should we include inactive sections</param>
		/// <returns>The matching report entity</returns>
		public Report RetrieveById(int reportId, bool includeInactiveSections = false)
		{
			const string METHOD_NAME = "RetrieveById";

			Report report;

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.Reports.Include("Category").Include("Formats").Include("SectionInstances").Include("SectionInstances.Section").Include("SectionInstances.Section.Elements").Include("CustomSections")
								where r.ReportId == reportId
								select r;

					report = query.FirstOrDefault();
				}

				//If we don't have a record, throw a specific exception (since client will be expecting one record)
				if (report == null)
				{
					throw new ArtifactNotExistsException("Report " + reportId.ToString() + " doesn't exist in the system.");
				}

				//We want to get a list of active formats, sections and active section elements
				//Remove any inactive elements and formats
				for (int i = 0; i < report.Formats.Count; i++)
				{
					if (!report.Formats[i].IsActive)
					{
						report.Formats.Remove(report.Formats[i]);
					}
				}

				if (!includeInactiveSections)
				{
					//Standard Sections
					for (int i = 0; i < report.SectionInstances.Count; i++)
					{
						if (!report.SectionInstances[i].Section.IsActive)
						{
							report.SectionInstances.Remove(report.SectionInstances[i]);
						}
						else
						{
							ReportSection reportSection = report.SectionInstances[i].Section;
							for (int j = 0; j < reportSection.Elements.Count; j++)
							{
								if (!reportSection.Elements[j].IsActive)
								{
									reportSection.Elements.Remove(reportSection.Elements[j]);
								}
							}
						}
					}

					//Custom Sections
					for (int i = 0; i < report.CustomSections.Count; i++)
					{
						if (!report.CustomSections[i].IsActive)
						{
							report.CustomSections.Remove(report.CustomSections[i]);
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return report;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a specific report by its token, including associated formats, sections and elements
		/// </summary>
		/// <param name="reportToken">The token (unique name) of the report we're interested in</param>
		/// <param name="includeInactiveSections">Should we include inactive sections</param>
		/// <returns>The matching report entity</returns>
		public Report RetrieveByToken(string reportToken, bool includeInactiveSections = false)
		{
			const string METHOD_NAME = "RetrieveByToken";

			Report report;

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.Reports.Include("Category").Include("Formats").Include("SectionInstances").Include("SectionInstances.Section").Include("SectionInstances.Section.Elements").Include("CustomSections")
								where r.Token == reportToken
								select r;

					report = query.FirstOrDefault();
				}

				//If we don't have a record, throw a specific exception (since client will be expecting one record)
				if (report == null)
				{
					throw new ArtifactNotExistsException("Report " + reportToken + " doesn't exist in the system.");
				}

				//We want to get a list of active formats, sections and active section elements
				//Remove any inactive elements and formats
				for (int i = 0; i < report.Formats.Count; i++)
				{
					if (!report.Formats[i].IsActive)
					{
						report.Formats.Remove(report.Formats[i]);
					}
				}

				if (!includeInactiveSections)
				{
					//Standard Sections
					for (int i = 0; i < report.SectionInstances.Count; i++)
					{
						if (!report.SectionInstances[i].Section.IsActive)
						{
							report.SectionInstances.Remove(report.SectionInstances[i]);
						}
						else
						{
							ReportSection reportSection = report.SectionInstances[i].Section;
							for (int j = 0; j < reportSection.Elements.Count; j++)
							{
								if (!reportSection.Elements[j].IsActive)
								{
									reportSection.Elements.Remove(reportSection.Elements[j]);
								}
							}
						}
					}

					//Custom Sections
					for (int i = 0; i < report.CustomSections.Count; i++)
					{
						if (!report.CustomSections[i].IsActive)
						{
							report.CustomSections.Remove(report.CustomSections[i]);
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return report;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates the progress bar and sets to zero once it hits 100%
		/// </summary>
		/// <param name="progress"></param>
		/// <returns></returns>
		protected int IncrementProgress(int progress)
		{
			progress++;
			if (progress > 100)
			{
				progress = 0;
			}

			return progress;
		}

		/// <summary>
		/// Generates a saved report on a background thread
		/// </summary>
		/// <param name="userId">The id of the user</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="savedReportId">The id of the saved report</param>
		/// <param name="appRootPath">The app root path</param>
		/// <param name="themeName">The name of the current theme</param>
		/// <param name="timezoneId">The current timezone</param>
		/// <returns>The GUID related to the generation request</returns>
		/// <remarks>
		/// Used by the API to generate the reports
		/// </remarks>
		public Guid GenerateSavedReport(int userId, int projectId, int savedReportId, string timezoneId, string themeName, string appRootPath)
		{
			const string METHOD_NAME = "GenerateSavedReport";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create the new Guid for the request
				Guid generationId = Guid.NewGuid();

				//Create the initial request
				if (reportGenerationRequests.TryAdd(generationId, GENERATION_ID_IN_PROGRESS))
				{
					//Generate the report as a background process
					if (!ThreadPool.QueueUserWorkItem(state => GenerateSavedReport_ASync(userId, projectId, savedReportId, generationId, timezoneId, themeName, appRootPath)))
					{
						//Remove the generation request
						int value;
						reportGenerationRequests.TryRemove(generationId, out value);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return generationId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Generates a saved report on a background thread
		/// </summary>
		/// <param name="userId">The id of the user</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="savedReportId">The id of the saved report</param>
		/// <param name="generationId">The id of the request to associate it with</param>
		/// <returns>The GUID related to the generation request</returns>
		/// <remarks>
		/// Used by the API to generate the reports
		/// </remarks>
		protected void GenerateSavedReport_ASync(int userId, int projectId, int savedReportId, Guid generationId, string timezoneId, string themeName, string appRootPath)
		{
			const string METHOD_NAME = "GenerateSavedReport_ASync";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Retrieve the saved report
				SavedReportView savedReport = RetrieveSavedById(savedReportId);
				if (savedReport != null)
				{
					//Get the template for this project
					int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

					int reportId = savedReport.ReportId;
					int reportFormatId = savedReport.ReportFormatId;
					string queryString = savedReport.Parameters;
					queryString += "&themeName=" + themeName;

					//Actually generate the report
					int generatedId = GenerateReport(userId, projectId, projectTemplateId, reportId, queryString, timezoneId, appRootPath);

					//Associate the ID with the generation request
					reportGenerationRequests.TryUpdate(generationId, generatedId, GENERATION_ID_IN_PROGRESS);
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				//Remove the report from the list of attempted generations
				int value;
				reportGenerationRequests.TryRemove(generationId, out value);

				//Don't rethrow as it causes an unhandled exception
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
			}
		}

		/// <summary>
		/// Checks to see if the reporting being generated is (a) done (b) in progress, or (c) abandoned
		/// </summary>
		/// <param name="reportGenerationId">The guid of the request</param>
		/// <returns>The ID of the generated report (when done) or null if still in progress</returns>
		/// <remarks>
		/// Throws an ArtifactNotExists exception if the generation request cannot be found
		/// </remarks>
		public int? CheckStatusOfGeneratingReport(Guid reportGenerationId)
		{
			const string METHOD_NAME = "CheckStatusOfGeneratingReport";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//See if the generation request exists
				if (reportGenerationRequests.ContainsKey(reportGenerationId))
				{
					int generatedReportId;
					if (reportGenerationRequests.TryGetValue(reportGenerationId, out generatedReportId))
					{
						if (generatedReportId == GENERATION_ID_IN_PROGRESS)
						{
							Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
							return null;
						}
						else
						{
							Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
							return generatedReportId;
						}
					}
					else
					{
						throw new ArtifactNotExistsException("There is no report generation request with ID=" + reportGenerationId.ToString());
					}
				}
				else
				{
					throw new ArtifactNotExistsException("There is no report generation request with ID=" + reportGenerationId.ToString());
				}
			}
			catch (ArtifactNotExistsException)
			{
				//Don't log
				throw;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Generates a new report as a temporary file X.dat
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="projectTemplateId">The id of the report template</param>
		/// <param name="queryString">The report querystring</param>
		/// <param name="reportId">The id of the report being generated</param>
		/// <param name="updateBackgroundProcessStatus">The background process monitor (optional)</param>
		/// <param name="userId">The id of the user running the report (to check permissions)</param>
		/// <param name="timezoneId">The id of the current user's timezone (for displaying date correctly)</param>
		/// <param name="appRootPath">The folder where the application can be found</param>
		/// <returns>The id of the temporary report file that was created</returns>
		/// <remarks>
		/// Since it's hard to determine how long the report will take we make the background process system
		/// display like an 'indeterminate' progress bar so that the user know that something is at least happening!!
		/// </remarks>
		public int GenerateReport(int userId, int projectId, int projectTemplateId, int reportId, string queryString, string timezoneId, string appRootPath, UpdateBackgroundProcessStatus updateBackgroundProcessStatus = null)
		{
			const string METHOD_NAME = "GenerateReport";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int progress = 0;
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(progress, GlobalResources.Messages.Report_StartGeneratingReport);
				}

				//Need to parse the 'querystring'
				Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Querystring = " + queryString);
				NameValueCollection query = HttpUtility.ParseQueryString(queryString);

				//Make sure we have the report format id
				if (String.IsNullOrEmpty(query["reportFormatId"]))
				{
					throw new InvalidOperationException("You need to supply a report format id.");
				}
				int reportFormatId = Int32.Parse(query["reportFormatId"]);

				//See if we are meant to save the generated report into the documents section
				int? savedDocumentFolderId = null;
				string savedDocumentFilename = null;
				if (!String.IsNullOrWhiteSpace(query["sg"]))
				{
					//Get the report filename and folder
					string[] parts = query["sg"].Split('|');
					int intValue;
					if (parts.Length >= 2 && Int32.TryParse(parts[0], out intValue))
					{
						if (Regex.IsMatch(parts[1], Global.VALIDATION_REGEX_FILENAME_XML))
						{
							savedDocumentFolderId = intValue;
							savedDocumentFilename = parts[1].Trim();
						}
					}
				}

				//Load the report entity that contains the available formats and elements
				Report report = RetrieveById(reportId);

				//Make sure the user is authorized
				ArtifactManager artifactManager = new ArtifactManager();
				ProjectManager projectManager = new ProjectManager();
				ProjectUserView projectMembership = projectManager.RetrieveUserMembershipById(projectId, userId);
				var project = projectManager.RetrieveById(projectId);
				int projectRoleId = 0;
				if (project.Name != "All Projects")
				{
					if (projectMembership == null)
					{
						throw new ArtifactAuthorizationException("You are not allowed to view this report due to your project permissions!");
					}
					projectRoleId = projectMembership.ProjectRoleId;
					if (report.Category.ArtifactTypeId.HasValue)
					{
						int artifactTypeId = report.Category.ArtifactTypeId.Value;
						if (projectManager.IsAuthorized(projectRoleId, (Artifact.ArtifactTypeEnum)artifactTypeId, Project.PermissionEnum.View) != Project.AuthorizationState.Authorized)
						{
							throw new ArtifactAuthorizationException("You are not allowed to view this report due to your project permissions!");
						}
					}
				}
				//Next get the report format information
				ReportFormat reportFormat = report.Formats.FirstOrDefault(f => f.ReportFormatId == reportFormatId);
				if (reportFormat == null)
				{
					//If we can't find the desired format, throw an exception
					throw new InvalidOperationException("The specified report format " + reportFormatId + " is not compatible with report " + reportId);
				}

				//If we have an XML format, record this because that format won't include any of the HTML static elements, just the data
				bool isXmlRawFormat = (reportFormat.ReportFormatId == (int)Report.ReportFormatEnum.Xml);

				string reportFormatToken = reportFormat.Token;
				string contentType = reportFormat.ContentType;
				string contentDisposition = reportFormat.ContentDisposition;

				//First we need to build the report body string
				StringBuilder reportText = new StringBuilder();

				//Next we need to add the report header to the string being built (not for XML format)
				if (!String.IsNullOrWhiteSpace(report.Header) && !isXmlRawFormat)
				{
					//Always enclose in a block element in case we have just literal text
					reportText.Append("<div>" + report.Header + "</div>");
				}

				//Now we need to iterate through all the sections in the report, adding the various elements
				//to the template xml
				int sectionCount = report.SectionInstances.Count + report.CustomSections.Count + 1;
				int i = 1;
				foreach (ReportSectionInstance sectionInstance in report.SectionInstances)
				{
					if (updateBackgroundProcessStatus != null)
					{
						updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
					}

					//Extract the template, header and footer
					string header = String.IsNullOrEmpty(sectionInstance.Header) ? "" : sectionInstance.Header;
					string footer = String.IsNullOrEmpty(sectionInstance.Footer) ? "" : sectionInstance.Footer;
					string template = sectionInstance.Template;

					//Get the actual section definition
					ReportSection reportSection = sectionInstance.Section;
					int reportSectionId = reportSection.ReportSectionId;

					//If there is no template, use the default one
					if (String.IsNullOrWhiteSpace(template))
					{
						template = reportSection.DefaultTemplate;
					}

					//See which secondary elements need to get added to the report section
					List<string> reportElements = new List<string>();
					foreach (ReportElement reportElement in reportSection.Elements)
					{
						//Now see if this element was specified in the querystring or not
						int reportElementId = reportElement.ReportElementId;
						string reportSectionAndElementId = "e_" + reportSectionId + "_" + reportElementId;
						if (!String.IsNullOrEmpty(query[reportSectionAndElementId]) && query[reportSectionAndElementId] == "1")
						{
							string reportElementToken = reportElement.Token;

							//See if the element requires permission-checking
							if (reportElement.ArtifactTypeId.HasValue)
							{
								//Make sure the user is authorized for this element
								if (projectManager.IsAuthorized(projectRoleId, (Artifact.ArtifactTypeEnum)reportElement.ArtifactTypeId.Value, Project.PermissionEnum.View) == Project.AuthorizationState.Authorized && !reportElements.Contains(reportElementToken))
								{
									reportElements.Add(reportElementToken);
								}
							}
							else
							{
								if (!reportElements.Contains(reportElementToken))
								{
									reportElements.Add(reportElementToken);
								}
							}
						}
					}

					//See if this section is connected to an artifact type
					DataModel.Artifact.ArtifactTypeEnum artifactType = DataModel.Artifact.ArtifactTypeEnum.None;
					Hashtable filters = null;
					Hashtable sorts = null;
					int? folderId = null;   //All folders
					if (reportSection.ArtifactTypeId.HasValue)
					{
						int artifactTypeId = reportSection.ArtifactTypeId.Value;
						artifactType = (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId;

						//See if we have any standard field filters/sorts to apply
						filters = new Hashtable();
						sorts = new Hashtable();
						List<ArtifactField> artifactFields = artifactManager.ArtifactField_RetrieveForReporting((DataModel.Artifact.ArtifactTypeEnum)artifactTypeId);
						foreach (ArtifactField artifactField in artifactFields)
						{
							int artifactFieldId = artifactField.ArtifactFieldId;

							//See if we have a filter value set in the querystring
							string sectionAndFieldId = "af_" + reportSectionId + "_" + artifactFieldId;
							if (!String.IsNullOrEmpty(query[sectionAndFieldId]))
							{
								//Get the field name, type and value
								string artifactFieldName = artifactField.Name;
								DataModel.Artifact.ArtifactFieldTypeEnum artifactFieldType = (DataModel.Artifact.ArtifactFieldTypeEnum)artifactField.ArtifactFieldTypeId;
								string filterTextValue = query[sectionAndFieldId];
								int filterIntValue;
								switch (artifactFieldType)
								{
									case DataModel.Artifact.ArtifactFieldTypeEnum.Lookup:
									case DataModel.Artifact.ArtifactFieldTypeEnum.MultiList:
										MultiValueFilter multiValueFilter;
										if (filterTextValue != null)
										{
											if (filterTextValue == "January")
											{
												filterTextValue = "1";
											}
											else if (filterTextValue == "February")
											{
												filterTextValue = "2";
											}
											else if (filterTextValue == "March")
											{
												filterTextValue = "3";
											}
											else if (filterTextValue == "April")
											{
												filterTextValue = "4";
											}
											else if (filterTextValue == "May")
											{
												filterTextValue = "5";
											}
											else if (filterTextValue == "July")
											{
												filterTextValue = "6";
											}
											else if (filterTextValue == "August")
											{
												filterTextValue = "7";
											}
											else if (filterTextValue == "September")
											{
												filterTextValue = "8";
											}
											else if (filterTextValue == "September")
											{
												filterTextValue = "9";
											}
											else if (filterTextValue == "October")
											{
												filterTextValue = "10";
											}
											else if (filterTextValue == "November")
											{
												filterTextValue = "11";
											}
											else if (filterTextValue == "December")
											{
												filterTextValue = "12";
											}
										}
										if (MultiValueFilter.TryParse(filterTextValue, out multiValueFilter))
										{
											filters.Add(artifactFieldName, multiValueFilter);
										}
										break;

									case DataModel.Artifact.ArtifactFieldTypeEnum.DateTime:
										DateRange dateRangeFilter;
										if (DateRange.TryParse(filterTextValue, out dateRangeFilter))
										{
											filters.Add(artifactFieldName, dateRangeFilter);
										}
										break;

									case DataModel.Artifact.ArtifactFieldTypeEnum.Flag:
										//Make sure no unusual values are detected
										if (filterTextValue == "Y" || filterTextValue == "N")
										{
											filters.Add(artifactFieldName, filterTextValue);
										}
										break;

									case DataModel.Artifact.ArtifactFieldTypeEnum.Equalizer:
									case DataModel.Artifact.ArtifactFieldTypeEnum.HierarchyLookup:
										if (Int32.TryParse(filterTextValue, out filterIntValue))
										{
											filters.Add(artifactFieldName, filterIntValue);
										}
										break;

									case DataModel.Artifact.ArtifactFieldTypeEnum.Integer:
										{
											IntRange intRange;
											if (IntRange.TryParse(filterTextValue, out intRange))
											{
												filters.Add(artifactFieldName, intRange);
											}
											break;
										}
									case DataModel.Artifact.ArtifactFieldTypeEnum.TimeInterval:
										{
											EffortRange effortRange;
											if (EffortRange.TryParse(filterTextValue, out effortRange))
											{
												filters.Add(artifactFieldName, effortRange);
											}
											break;
										}

									case DataModel.Artifact.ArtifactFieldTypeEnum.Decimal:
										{
											DecimalRange decimalRange;
											if (DecimalRange.TryParse(filterTextValue, out decimalRange))
											{
												filters.Add(artifactFieldName, decimalRange);
											}
											break;
										}

									case DataModel.Artifact.ArtifactFieldTypeEnum.Identifier:
										//We can accept either single IDs or a comma-separated list of IDs
										if (Int32.TryParse(filterTextValue, out filterIntValue))
										{
											filters.Add(artifactFieldName, filterIntValue);
										}
										else if (MultiValueFilter.TryParse(filterTextValue, out multiValueFilter))
										{
											filters.Add(artifactFieldName, multiValueFilter);
										}
										break;

									case DataModel.Artifact.ArtifactFieldTypeEnum.Text:
									case DataModel.Artifact.ArtifactFieldTypeEnum.NameDescription:
										filters.Add(artifactFieldName, filterTextValue);
										break;
								}
							}

							//See if we have a sort value set in the querystring
							sectionAndFieldId = "st_" + reportSectionId + "_" + artifactFieldId;
							if (!String.IsNullOrEmpty(query[sectionAndFieldId]))
							{
								//If this is a lookup field we need to get the lookup field to sort on
								DataModel.Artifact.ArtifactFieldTypeEnum artifactFieldType = (DataModel.Artifact.ArtifactFieldTypeEnum)artifactField.ArtifactFieldTypeId;
								if (artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.Lookup || artifactFieldType == DataModel.Artifact.ArtifactFieldTypeEnum.HierarchyLookup)
								{
									string artifactFieldName = artifactField.Name;
									string sortDirection = query[sectionAndFieldId];
									string lookupField = artifactField.LookupProperty;
									//Make sure lookup field is not NULL/empty
									if (String.IsNullOrWhiteSpace(lookupField))
									{
										sorts.Add(artifactFieldName, sortDirection);
									}
									else
									{
										sorts.Add(lookupField, sortDirection);
									}
								}
								else
								{
									string artifactFieldName = artifactField.Name;
									string sortDirection = query[sectionAndFieldId];
									sorts.Add(artifactFieldName, sortDirection);
								}
							}
						}

						//See if we have any custom property filters to apply
						CustomPropertyManager customPropertyManager = new CustomPropertyManager();
						List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, (DataModel.Artifact.ArtifactTypeEnum)artifactTypeId, false);
						foreach (CustomProperty customProperty in customProperties)
						{
							int customPropertyId = customProperty.CustomPropertyId;
							//See if we have a value set in the querystring
							string sectionAndPropertyId = "cp_" + reportSectionId + "_" + customPropertyId;
							if (!String.IsNullOrEmpty(query[sectionAndPropertyId]))
							{
								//Get the field name, type and value
								string customPropertyName = customProperty.CustomPropertyFieldName;
								string filterTextValue = query[sectionAndPropertyId];
								switch ((CustomProperty.CustomPropertyTypeEnum)customProperty.CustomPropertyTypeId)
								{
									case CustomProperty.CustomPropertyTypeEnum.Boolean:
										{
											//Make sure no unusual values are detected
											if (filterTextValue == "Y" || filterTextValue == "N")
											{
												filters.Add(customPropertyName, filterTextValue);
											}
										}
										break;
									case CustomProperty.CustomPropertyTypeEnum.Date:
										{
											DateRange dateRangeFilter;
											if (DateRange.TryParse(filterTextValue, out dateRangeFilter))
											{
												filters.Add(customPropertyName, dateRangeFilter);
											}
										}
										break;

									case CustomProperty.CustomPropertyTypeEnum.Decimal:
										{
											DecimalRange decimalRange;
											if (DecimalRange.TryParse(filterTextValue, out decimalRange))
											{
												filters.Add(customPropertyName, decimalRange);
											}
											break;
										}

									case CustomProperty.CustomPropertyTypeEnum.Integer:
										{
											IntRange intRange;
											if (IntRange.TryParse(filterTextValue, out intRange))
											{
												filters.Add(customPropertyName, intRange);
											}
											break;
										}

									case CustomProperty.CustomPropertyTypeEnum.Text:
										{
											filters.Add(customPropertyName, filterTextValue);
										}
										break;

									case CustomProperty.CustomPropertyTypeEnum.List:
									case CustomProperty.CustomPropertyTypeEnum.MultiList:
									case CustomProperty.CustomPropertyTypeEnum.User:
										{
											MultiValueFilter multiValueFilter;
											if (MultiValueFilter.TryParse(filterTextValue, out multiValueFilter))
											{
												filters.Add(customPropertyName, multiValueFilter);
											}

										}
										break;
								}
							}

							//See if we have a sort value set in the querystring
							sectionAndPropertyId = "st_" + reportSectionId + "_-" + customPropertyId;
							if (!String.IsNullOrEmpty(query[sectionAndPropertyId]))
							{
								string artifactFieldName = customProperty.CustomPropertyFieldName;
								string sortDirection = query[sectionAndPropertyId];
								sorts.Add(artifactFieldName, sortDirection);
							}
						}

						//See if we have any folder filters to apply
						string folderKey = "fl_" + reportSectionId;
						if (!String.IsNullOrWhiteSpace(query[folderKey]))
						{
							int intValue;
							if (Int32.TryParse(query[folderKey], out intValue))
							{
								folderId = intValue;
							}
						}
					}

					//Get the report section token and create the section
					string reportSectionToken = reportSection.Token;
					CreateReportSection(userId, projectId, projectTemplateId, ref reportText, reportSectionToken, header, footer, template, artifactType, isXmlRawFormat, reportFormatToken, reportElements, folderId, filters, sorts, timezoneId, updateBackgroundProcessStatus, progress);

					//Increment the count
					i++;
				}

				//See if we have a release filter for any of the custom sections
				const string customSectionReleaseId = "cs_rl";
				int? releaseId = null;
				if (!String.IsNullOrEmpty(query[customSectionReleaseId]))
				{
					int intValue;
					if (Int32.TryParse(query[customSectionReleaseId], out intValue))
					{
						releaseId = intValue;
					}
				}

				//Now we need to add the custom sections
				foreach (ReportCustomSection reportCustomSection in report.CustomSections)
				{
					if (updateBackgroundProcessStatus != null)
					{
						updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
					}

					//Create the custom section
					if (reportCustomSection.IsActive)
					{
						CreateCustomReportSection(userId, projectId, ref reportText, reportCustomSection.Query, reportCustomSection.Header, reportCustomSection.Footer, reportCustomSection.Template, isXmlRawFormat, reportFormatToken, timezoneId, updateBackgroundProcessStatus, progress, releaseId);
					}

					//Increment the count
					i++;
				}

				//Next we need to add the report footer to the string being built (not for XML format)
				if (!String.IsNullOrWhiteSpace(report.Footer) && !isXmlRawFormat)
				{
					//Always enclose in a block element in case we have just literal text
					reportText.Append("<div>" + report.Footer + "</div>");
				}

				//If we're rendering as a non-HTML/XML based format, need to convert to the appropriate format
				string reportString = reportText.ToString();
				if (reportFormatToken == "MsWord2003" || reportFormatToken == "MsExcel2003" || reportFormatToken == "Pdf")
				{
					reportString = HtmlConvertToTargetFormat(reportFormatToken, reportString, appRootPath);
				}

				//For newer versions of Word and Excel we use HTML and traverse it to hanlde images
				else if (reportFormatToken == "MsWord2007" || reportFormatToken == "MsExcel2007")
				{
					//Turn the string into an HTML doc so we can traverse and manipulate it
					HtmlDocument html = new HtmlDocument();
					html.LoadHtml(reportString);

					//Find all the images in the report - if there are any remove them
					HtmlNodeCollection images = html.DocumentNode.SelectNodes("//img");
					if (images != null && images.Count > 0)
					{
						foreach (HtmlNode image in images)
						{
							// for Word we embed the images so they can be rendered
							if (reportFormatToken == "MsWord2007")
							{
								string src = image.Attributes["src"].Value;
								string imageData = GetImageData(src);
								//Replace original src with the encoded version
								image.SetAttributeValue("src", imageData);

							}
							// for Excel (as of Excel 2019 the images never display so remove them
							else if (reportFormatToken == "MsExcel2007")
							{
								image.Remove();
							}
						}
					}

					//Convert the HTML object back to a string for use in rest of method
					HtmlNode docAsNode = html.DocumentNode;
					reportString = docAsNode.WriteContentTo();
				}

				//Next we need to put the report body into the appropriate format template
				string templateFilename = Path.Combine(appRootPath, "Reports/" + reportFormatToken + ".xml");
				if (!File.Exists(templateFilename))
				{
					throw new ApplicationException("Unable to load the report template file: '" + templateFilename + "' so aborting report generation!");
				}
				StreamReader streamReader = File.OpenText(templateFilename);
				string reportFormatTemplate = streamReader.ReadToEnd();
				streamReader.Close();

				//Replace the report title
				reportFormatTemplate = reportFormatTemplate.Replace("<!--REPORT-TITLE-->", report.Name + " " + GlobalResources.General.Global_Report);

				//Now replace the report body
				reportString = reportFormatTemplate.Replace("<!--REPORT-BODY-->", reportString);

				//Finally we need to save the report in a temporary location, and add an entry in the ReportGenerated table
				ReportGenerated reportGenerated = ReportGenerated_Create(userId, reportId, reportFormatId);
				int reportGeneratedId = reportGenerated.ReportGeneratedId;

				//Get the common data folder where we can store the temporary files
				string cacheFolder = ConfigurationSettings.Default.Cache_Folder;
				if (String.IsNullOrWhiteSpace(cacheFolder))
				{
					cacheFolder = Common.Global.CACHE_FOLDERPATH;
				}
				string folderPath = Path.Combine(cacheFolder, REPORT_GENERATED_FOLDER);
				if (!Directory.Exists(folderPath))
				{
					Directory.CreateDirectory(folderPath);
				}

				//Create the file. If We have a PDF format, we need to use the NFOP library (converts XSL-FO to its native format (e.g. PDF))
				string filePath;
				if (contentDisposition == REPORT_CONTENT_DISPOSITION_XSL_FO)
				{
					if (updateBackgroundProcessStatus != null)
					{
						updateBackgroundProcessStatus(95, GlobalResources.Messages.Report_ReportConvertingToPdf);
					}

					//Need to create a new XML doc from the text of the report
					XmlDocument reportDoc = new XmlDocument();
					reportDoc.LoadXml(reportString);

					//Create the PDF
					PdfRendererOptions options = new PdfRendererOptions();
					options.Author = ConfigurationSettings.Default.License_ProductType;
					options.Title = ConfigurationSettings.Default.License_ProductType + " Report";
					options.Subject = "Management Guide";
					options.EnableModify = false;
					options.EnableAdd = false;
					options.EnableCopy = true;
					options.EnablePrinting = true;
					options.FontType = FontType.Embed;


					//Need to use our custom image handler that handles embedded binary data
					//http://fonet.codeplex.com/discussions/231336
					FonetDriver fonetDriver = FonetDriver.Make();
					fonetDriver.ImageHandler = new FonetDriver.FonetImageHandler(ImageHandler);
					fonetDriver.Options = options;
					filePath = Path.Combine(folderPath, reportGeneratedId + ".dat");
					FileStream fileStream = File.Create(filePath);
					fonetDriver.Render(reportDoc, fileStream);
					fileStream.Close();
				}
				else
				{
					filePath = Path.Combine(folderPath, reportGeneratedId + ".dat");
					StreamWriter streamWriter = File.CreateText(filePath);
					streamWriter.Write(reportString);
					streamWriter.Close();
				}

				//Finally see if we need to put a copy of this report in the documents repository
				if (savedDocumentFolderId.HasValue && !String.IsNullOrEmpty(savedDocumentFilename))
				{
					try
					{
						byte[] binaryData = File.ReadAllBytes(filePath);
						string filenameWithExtension = savedDocumentFilename + GetExtensionForFormat(reportFormatId);
						AttachmentManager attachmentManager = new AttachmentManager();
						int attachmentId = attachmentManager.Insert(projectId, filenameWithExtension, report.Description, userId, binaryData, null, Artifact.ArtifactTypeEnum.None, "1.0", null, null, savedDocumentFolderId.Value, null);

						//Send a notification
						attachmentManager.SendCreationNotification(projectId, attachmentId, null, null);
					}
					catch (System.Exception exception)
					{
						Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, "Unable to save generated report into document repository - " + exception.Message);
						Logger.Flush();
						throw;
					}
				}

				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(100, GlobalResources.Messages.Report_ReportGenerationComplete);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return reportGeneratedId;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				//The report doesn't exist (in reality it would have been checked in the ASPX page before this was called)
				throw new InvalidOperationException("The passed in report ID or report format ID does not exist.");
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Handles the case of embedded images using the URL format data:
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		private static byte[] ImageHandler(string input)
		{
			if (string.IsNullOrEmpty(input) || !input.StartsWith("data:"))
			{
				return null; //fo performs normal processing
			}

			//String off the date:image/xxx;base64, section
			int index = input.IndexOf("base64,");
			if (index == -1)
			{
				return null; //fo performs normal processing
			}
			string base64data = input.Substring(index + "base64,".Length);

			//Convert into bytes
			byte[] imageData = Convert.FromBase64String(base64data);

			return imageData;
		}

		/// <summary>
		/// Retrieves the text of a previously generated report (and then removes the file and the entry)
		/// </summary>
		/// <param name="reportGeneratedId">The id of the generated report file</param>
		/// <returns>The contents of the file</returns>
		public string GetReportText(int reportGeneratedId)
		{
			const string METHOD_NAME = "GetReportText";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the common data folder where we can store the temporary files
				string cacheFolder = ConfigurationSettings.Default.Cache_Folder;
				if (String.IsNullOrWhiteSpace(cacheFolder))
				{
					cacheFolder = Common.Global.CACHE_FOLDERPATH;
				}
				string folderPath = Path.Combine(cacheFolder, REPORT_GENERATED_FOLDER);
				if (!Directory.Exists(folderPath))
				{
					return "";
				}

				//open the file
				string filePath = Path.Combine(folderPath, reportGeneratedId + ".dat");
				StreamReader streamReader = File.OpenText(filePath);
				string reportText = streamReader.ReadToEnd();
				streamReader.Close();

				//Delete the file and the entry in the DB
				File.Delete(filePath);
				ReportGenerated_Delete(reportGeneratedId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return reportText;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the binary data of a previously generated report (and then removes the file and the entry)
		/// </summary>
		/// <param name="reportGeneratedId">The id of the generated report file</param>
		/// <returns>The contents of the file</returns>
		/// <remarks>Used for PDFs since they are not saved as text files</remarks>
		public byte[] GetReportData(int reportGeneratedId)
		{
			const string METHOD_NAME = "GetReportData";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the common data folder where we can store the temporary files
				string cacheFolder = ConfigurationSettings.Default.Cache_Folder;
				if (String.IsNullOrWhiteSpace(cacheFolder))
				{
					cacheFolder = Common.Global.CACHE_FOLDERPATH;
				}
				string folderPath = Path.Combine(cacheFolder, REPORT_GENERATED_FOLDER);
				if (!Directory.Exists(folderPath))
				{
					return null;
				}

				//open the file
				string filePath = Path.Combine(folderPath, reportGeneratedId + ".dat");
				byte[] data = File.ReadAllBytes(filePath);

				//Delete the file and the entry in the DB
				File.Delete(filePath);
				ReportGenerated_Delete(reportGeneratedId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return data;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Deletes a generated report record
		/// </summary>
		/// <param name="reportGeneratedId">The id of the generated report</param>
		public void ReportGenerated_Delete(int reportGeneratedId)
		{
			const string METHOD_NAME = "ReportGenerated_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.ReportGenerateds
								where r.ReportGeneratedId == reportGeneratedId
								select r;

					ReportGenerated reportGenerated = query.FirstOrDefault();
					if (reportGenerated != null)
					{
						context.ReportGenerateds.DeleteObject(reportGenerated);
						context.SaveChanges();
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a generated report record
		/// </summary>
		/// <param name="reportGeneratedId">The id of the generated report</param>
		/// <returns>The generated report</returns>
		public ReportGenerated ReportGenerated_RetrieveById(int reportGeneratedId)
		{
			const string METHOD_NAME = "ReportGenerated_RetrieveById";

			ReportGenerated reportGenerated;

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.ReportGenerateds.Include("Format")
								where r.ReportGeneratedId == reportGeneratedId
								select r;

					reportGenerated = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return reportGenerated;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Creates a new generated report identifier
		/// </summary>
		/// <param name="reportId">The id of the report being generated</param>
		/// <param name="reportFormatId">The id of the format specified</param>
		/// <param name="userId">The id of the user creating the report</param>
		/// <returns>The newly created generator report</returns>
		public ReportGenerated ReportGenerated_Create(int userId, int reportId, int reportFormatId)
		{
			const string METHOD_NAME = "ReportGenerated_Create";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ReportGenerated reportGenerated = new ReportGenerated();
				reportGenerated.UserId = userId;
				reportGenerated.ReportId = reportId;
				reportGenerated.ReportFormatId = reportFormatId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Persist the new generated report entity
					context.ReportGenerateds.AddObject(reportGenerated);
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return reportGenerated;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#region Internal Functions

		/// <summary>
		/// Creates the custom report section XML, applies the appropriate XSLT transforms, adds header/footers and then adds to the report text
		/// </summary>
		/// <param name="userId">The id of the user creating the report</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="reportText">The master HTML/XML document containing the report</param>
		/// <param name="query">The custom report section Entity SQL query</param>
		/// <param name="footer">Any footer HTML to add</param>
		/// <param name="header">Any header HTML to add</param>
		/// <param name="template">The XSLT template to apply to the data</param>
		/// <param name="isXmlRawFormat">Are we returning just the data in raw XML format</param>
		/// <param name="reportFormatToken">The report format token</param>
		/// <param name="timezoneId">The id of the timezone</param>
		/// <param name="releaseId">Is there is a release ID specified</param>
		protected void CreateCustomReportSection(int userId, int projectId, ref StringBuilder reportText, string query, string header, string footer, string template, bool isXmlRawFormat, string reportFormatToken, string timezoneId, UpdateBackgroundProcessStatus updateBackgroundProcessStatus, int progress, int? releaseId = null)
		{
			const string METHOD_NAME = "CreateCustomReportSection";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Make sure we have a template specified
			if (String.IsNullOrWhiteSpace(template))
			{
				throw new ApplicationException(GlobalResources.Messages.Report_NeedToProvideAnXsltTemplate);
			}

			try
			{
				//First add the report section header
				if (!String.IsNullOrWhiteSpace(header) && !isXmlRawFormat)
				{
					//Always enclose in a block element in case we have just literal text
					reportText.Append("<div>" + header + "</div>");
				}

				//Get the project group id if we have a project selected
				int projectGroupId = -1;
				if (projectId > 0)
				{
					ProjectManager projectManager = new ProjectManager();
					Project project = projectManager.RetrieveById(projectId);
					projectGroupId = project.ProjectGroupId;

					//Make sure the current user is a member of the group, if not, switch to 0 so that no data is returned
					//The page has already made sure that we're a member of the project itself
					//and that we can view the appropriate category of report
					Business.ProjectGroupManager projectGroupManager = new Business.ProjectGroupManager();
					DataModel.ProjectGroup projectGroup = projectGroupManager.RetrieveById(projectGroupId, true);
					if (!projectGroup.Users.Any(p => p.ProjectGroupId == projectGroupId && p.UserId == userId))
					{
						projectGroupId = 0;
					}
				}

				//Generate the custom section XML from the custom query
				XmlDocument xmlDataDocument = ReportCustomSection_ExecuteSQL(projectId, projectGroupId, query, updateBackgroundProcessStatus, progress, MAX_CUSTOM_QUERY_ROWS, releaseId);

				//Make sure that some data was returned
				if (xmlDataDocument != null)
				{
					//Localize any dates
					LocalizeDates(xmlDataDocument, timezoneId);

					//Load in the appropriate transform (unless XML output, in which case not necessary)
					string outputText = "";
					if (reportFormatToken == "Xml")
					{
						outputText = xmlDataDocument.OuterXml;
					}
					else
					{
						//Load in the template as XML
						XmlDocument xmlTemplate = new XmlDocument();
						xmlTemplate.LoadXml(template);

						//Remove any unnecessary blank space since it will cause issues with the MS-Office reports
						TrimNodes(xmlTemplate.ChildNodes);

						XslCompiledTransform xslt = new XslCompiledTransform();
						XsltSettings xsltSettings = new XsltSettings();
						xsltSettings.EnableScript = true;
						xslt.Load(xmlTemplate, xsltSettings, null);

						//Transform the XML into a new XML document in string form
						//If we're using HTML format, need to use the XHTML writer to account for tag closing differences
						XmlTextWriter xmlWriter;
						MemoryStream memoryStream = new MemoryStream();
						if (reportFormatToken == "Html")
						{
							xmlWriter = new XHTMLWriter(memoryStream, Encoding.UTF8);
						}
						else
						{
							xmlWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
						}
						xslt.Transform(xmlDataDocument, null, xmlWriter);
						StreamReader streamReader = new StreamReader(memoryStream);
						memoryStream.Seek(0, SeekOrigin.Begin);
						outputText = streamReader.ReadToEnd();
					}

					//Remove the XML header
					outputText = outputText.Replace(@"<?xml version=""1.0"" encoding=""utf-8""?>", "");

					//Next add this content to the report
					reportText.Append(outputText);
				}

				//Finally add the report section footer
				if (!String.IsNullOrWhiteSpace(footer) && !isXmlRawFormat)
				{
					//Always enclose in a block element in case we have just literal text
					reportText.Append("<div>" + footer + "</div>");
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Applies the XML transform necessary to generate the section and add to the report XML
		/// </summary>
		/// <param name="reportText">The master HTML/XML document containing the report</param>
		/// <param name="reportSectionToken">The token for the report section</param>
		/// <param name="isXmlRawFormat">Are we returning just the data in raw XML format</param>
		/// <param name="artifactType">The artifact type (used when we don't have specific data needed for a section)</param>
		/// <param name="reportElements">The list of secondary elements to add to the report</param>
		/// <param name="filters">Any filters to apply to the section</param>
		/// <param name="context">The HTTP context</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="userId">The id of the user</param>
		/// <param name="sorts">Any sorts to apply to the section</param>
		/// <param name="footer">Any footer HTML to add</param>
		/// <param name="header">Any header HTML to add</param>
		/// <param name="reportFormatToken">The report format token</param>
		/// <param name="timezoneId">The id of the timezone</param>
		/// <param name="folderId">Any folder to specify (0 = root, null = all folders)</param>
		/// <param name="template">The XSLT template to apply to the data</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		protected void CreateReportSection(int userId, int projectId, int projectTemplateId, ref StringBuilder reportText, string reportSectionToken, string header, string footer, string template, DataModel.Artifact.ArtifactTypeEnum artifactType, bool isXmlRawFormat, string reportFormatToken, List<string> reportElements, int? folderId, Hashtable filters, Hashtable sorts, string timezoneId, UpdateBackgroundProcessStatus updateBackgroundProcessStatus, int progress)
		{
			//First add the report section header
			if (!String.IsNullOrWhiteSpace(header) && !isXmlRawFormat)
			{
				//Always enclose in a block element in case we have just literal text
				reportText.Append("<div>" + header + "</div>");
			}

			//Depending on the report section, we may need to load in specific data
			XmlDocument xmlDataDocument = null;
			switch (reportSectionToken)
			{
				case "ProjectOverview":
					xmlDataDocument = Get_ProjectOverview(projectId, reportFormatToken, timezoneId, updateBackgroundProcessStatus, progress);
					break;

				case "RequirementPlan":
					xmlDataDocument = Get_RequirementPlan(projectId, projectTemplateId, reportFormatToken, reportElements, filters, timezoneId, updateBackgroundProcessStatus, progress);
					break;

				case "ReleasePlan":
					xmlDataDocument = Get_ReleasePlan(projectId, projectTemplateId, reportFormatToken, reportElements, filters, timezoneId, updateBackgroundProcessStatus, progress);
					break;

				case "TestPrintable":
					xmlDataDocument = Get_TestScripts(projectId, projectTemplateId, reportFormatToken, reportElements, folderId, filters, sorts, timezoneId, updateBackgroundProcessStatus, progress);
					break;
			}
			var project = new ProjectManager().RetrieveProjectById(projectId);
			//Otherwise we do it by artifact type (that way report sections can share the code that builds the XML data stream)
			if (xmlDataDocument == null)
			{
				switch (artifactType)
				{
					case DataModel.Artifact.ArtifactTypeEnum.Requirement:
						xmlDataDocument = Get_RequirementDetails(userId, projectId, projectTemplateId, reportFormatToken, reportElements, filters, timezoneId, updateBackgroundProcessStatus, progress);
						break;

					case DataModel.Artifact.ArtifactTypeEnum.TestCase:
						xmlDataDocument = Get_TestCaseDetails(projectId, projectTemplateId, reportFormatToken, reportElements, folderId, filters, sorts, timezoneId, updateBackgroundProcessStatus, progress);
						break;

					case DataModel.Artifact.ArtifactTypeEnum.TestSet:
						xmlDataDocument = Get_TestSetDetails(projectId, projectTemplateId, reportFormatToken, reportElements, folderId, filters, sorts, timezoneId, updateBackgroundProcessStatus, progress);
						break;

					case DataModel.Artifact.ArtifactTypeEnum.Incident:
						xmlDataDocument = Get_IncidentDetails(userId, projectId, projectTemplateId, reportFormatToken, reportElements, filters, sorts, timezoneId, updateBackgroundProcessStatus, progress);
						break;

					case DataModel.Artifact.ArtifactTypeEnum.TestRun:
						xmlDataDocument = Get_TestRunDetails(projectId, projectTemplateId, reportFormatToken, reportElements, filters, sorts, timezoneId, updateBackgroundProcessStatus, progress);
						break;

					case DataModel.Artifact.ArtifactTypeEnum.Task:
						xmlDataDocument = Get_TaskDetails(userId, projectId, projectTemplateId, reportFormatToken, reportElements, folderId, filters, sorts, timezoneId, updateBackgroundProcessStatus, progress);
						break;

					case DataModel.Artifact.ArtifactTypeEnum.Release:
						xmlDataDocument = Get_ReleaseDetails(projectId, projectTemplateId, reportFormatToken, reportElements, filters, timezoneId, updateBackgroundProcessStatus, progress);
						break;

					case DataModel.Artifact.ArtifactTypeEnum.Risk:
						//Add summary risk table html
						var riskSummaryTable = GetRiskSummaryTable(projectTemplateId, projectId);
						reportText = reportText.Append("&nbsp" + "<div style=" + "color:#999;font-size:1.75rem;font-weight:bold>" + "Risk Summary" + "</div>" + riskSummaryTable + "&nbsp");
						xmlDataDocument = Get_RiskDetails(userId, projectId, projectTemplateId, reportFormatToken, reportElements, filters, sorts, timezoneId, updateBackgroundProcessStatus, progress);
						break;


					case DataModel.Artifact.ArtifactTypeEnum.AllProjectHistoryList:
						xmlDataDocument = Get_AllProjectHistoryDetails(userId, projectId, projectTemplateId, reportFormatToken, reportElements, filters, sorts, timezoneId, updateBackgroundProcessStatus, progress);
						break;
					case DataModel.Artifact.ArtifactTypeEnum.AllAuditTrail:
						xmlDataDocument = Get_AllAuditTrailDetails(userId, projectId, projectTemplateId, reportFormatToken, reportElements, filters, sorts, timezoneId, updateBackgroundProcessStatus, progress);
						break;
					case DataModel.Artifact.ArtifactTypeEnum.AllAdminAuditTrail:
						xmlDataDocument = Get_AllAdminAuditHistoryDetails(userId, projectId, projectTemplateId, reportFormatToken, reportElements, filters, sorts, timezoneId, updateBackgroundProcessStatus, progress);
						break;
					case DataModel.Artifact.ArtifactTypeEnum.AllUserAuditTrail:
						xmlDataDocument = Get_AllUserAuditHistoryDetails(userId, projectId, projectTemplateId, reportFormatToken, reportElements, filters, sorts, timezoneId, updateBackgroundProcessStatus, progress);
						break;

					case DataModel.Artifact.ArtifactTypeEnum.ProjectHistoryList:
						if (project.Name == "All Projects")
						{
							xmlDataDocument = Get_AllProjectHistoryDetails(userId, projectId, projectTemplateId, reportFormatToken, reportElements, filters, sorts, timezoneId, updateBackgroundProcessStatus, progress);
						}
						else
						{
							xmlDataDocument = Get_ProjectHistoryDetails(userId, projectId, projectTemplateId, reportFormatToken, reportElements, filters, sorts, timezoneId, updateBackgroundProcessStatus, progress);
						}
						break;
					case DataModel.Artifact.ArtifactTypeEnum.AuditTrail:
						if (project.Name == "All Projects")
						{
							xmlDataDocument = Get_AllAuditTrailDetails(userId, projectId, projectTemplateId, reportFormatToken, reportElements, filters, sorts, timezoneId, updateBackgroundProcessStatus, progress);
						}
						else
						{
							xmlDataDocument = Get_AuditTrailDetails(userId, projectId, projectTemplateId, reportFormatToken, reportElements, filters, sorts, timezoneId, updateBackgroundProcessStatus, progress);
						}
						break;
					case DataModel.Artifact.ArtifactTypeEnum.AdminAuditTrail:
						if (project.Name == "All Projects")
						{
							xmlDataDocument = Get_AllAdminAuditHistoryDetails(userId, projectId, projectTemplateId, reportFormatToken, reportElements, filters, sorts, timezoneId, updateBackgroundProcessStatus, progress);
						}
						else
						{
							xmlDataDocument = Get_AdminAuditHistoryDetails(userId, projectId, projectTemplateId, reportFormatToken, reportElements, filters, sorts, timezoneId, updateBackgroundProcessStatus, progress);
						}
						break;
					case DataModel.Artifact.ArtifactTypeEnum.UserAuditTrail:
						if (project.Name == "All Projects")
						{
							xmlDataDocument = Get_AllUserAuditHistoryDetails(userId, projectId, projectTemplateId, reportFormatToken, reportElements, filters, sorts, timezoneId, updateBackgroundProcessStatus, progress);
						}
						else
						{
							xmlDataDocument = Get_UserAuditHistoryDetails(userId, projectId, projectTemplateId, reportFormatToken, reportElements, filters, sorts, timezoneId, updateBackgroundProcessStatus, progress);
						}
						break;

					case DataModel.Artifact.ArtifactTypeEnum.SystemUsageReport:
						xmlDataDocument = Get_SystemUsageReport(userId, projectId, projectTemplateId, reportFormatToken, reportElements, filters, sorts, timezoneId, updateBackgroundProcessStatus, progress);
						break;
				}
			}

			//Make sure that some data was returned
			if (xmlDataDocument != null)
			{
				//Load in the appropriate transform (unless XML output, in which case not necessary)
				string outputText = "";
				if (reportFormatToken == "Xml")
				{
					outputText = xmlDataDocument.OuterXml;
				}
				else
				{
					//Load in the template as XML
					XmlDocument xmlTemplate = new XmlDocument();
					xmlTemplate.LoadXml(template);

					//Remove any unnecessary blank space since it will cause issues with the MS-Office reports
					TrimNodes(xmlTemplate.ChildNodes);

					XslCompiledTransform xslt = new XslCompiledTransform();
					XsltSettings xsltSettings = new XsltSettings();
					xsltSettings.EnableScript = true;
					xslt.Load(xmlTemplate, xsltSettings, null);

					//Transform the XML into a new XML document in string form
					//If we're using HTML format, need to use the XHTML writer to account for tag closing differences
					XmlTextWriter xmlWriter;
					MemoryStream memoryStream = new MemoryStream();
					if (reportFormatToken == "Html")
					{
						xmlWriter = new XHTMLWriter(memoryStream, Encoding.UTF8);
					}
					else
					{
						xmlWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
					}
					xslt.Transform(xmlDataDocument, null, xmlWriter);
					StreamReader streamReader = new StreamReader(memoryStream);
					memoryStream.Seek(0, SeekOrigin.Begin);
					outputText = streamReader.ReadToEnd();
				}

				//Remove the XML header
				outputText = outputText.Replace(@"<?xml version=""1.0"" encoding=""utf-8""?>", "");

				//Next add this content to the report
				reportText.Append(outputText);
			}

			//Finally add the report section footer
			if (!String.IsNullOrWhiteSpace(footer) && !isXmlRawFormat)
			{
				//Always enclose in a block element in case we have just literal text
				reportText.Append("<div>" + footer + "</div>");
			}
		}

		private string GetRiskSummaryTable(int ProjectTemplateId, int projectId)
		{
			string html = String.Empty;
			Table tableContent = new Table();


			TableRow row = new TableRow();
			TableCell cell = new TableCell();
			tableContent.ID = "Risk Summary";


			row.Height = 40;
			row.Width = 60;
			row.BorderColor = Color.White;
			row.BorderStyle = BorderStyle.Solid;
			row.Cells.Add(cell);


			List<RiskProbability> probabilities = new List<RiskProbability>();
			List<RiskImpact> impacts = new List<RiskImpact>();
			using (StringWriter sw = new StringWriter())
			{
				if (ProjectTemplateId > 0)
				{
					//We need to dynamically add the risk probabilities as columns to the Risk Summary table
					//We sort them in order of increasing score, not position, so need to resort first
					RiskManager riskManager = new RiskManager();
					probabilities = riskManager.RiskProbability_Retrieve(ProjectTemplateId).OrderBy(p => p.Score).ToList();
					for (int i = 0; i < probabilities.Count; i++)
					{
						cell = new TableCell();

						cell.Text = probabilities[i].Name;
						cell.BorderColor = Color.White;
						cell.BorderStyle = BorderStyle.Solid;
						row.Height = 40;
						row.Width = 60;
						row.BorderColor = Color.White;
						row.BorderStyle = BorderStyle.Solid;
						row.Cells.Add(cell);
					}

					tableContent.Rows.Add(row);

					int matrixSize = riskManager.Risk_MatrixSize(projectId);
					impacts = riskManager.RiskImpact_Retrieve(ProjectTemplateId).OrderByDescending(p => p.Score).ToList();
					for (int i = 0; i < impacts.Count; i++)
					{
						row = new TableRow();
						cell = new TableCell();

						cell.Text = impacts[i].Name;
						cell.BorderColor = Color.White;
						cell.BorderStyle = BorderStyle.Solid;
						row.Height = 40;
						row.Width = 60;
						row.BorderColor = Color.White;
						row.BorderStyle = BorderStyle.Solid;
						row.Cells.Add(cell);

						for (int j = 0; j < probabilities.Count; j++)
						{
							cell = new TableCell();
							var probabilityScore = Convert.ToInt32(probabilities.ElementAt(j).Score.ToString());
							var impactScore = Convert.ToInt32(impacts.ElementAt(i).Score.ToString());
							cell.ToolTip = (probabilityScore * impactScore).ToString();
							cell.BorderColor = Color.White;
							cell.BorderStyle = BorderStyle.Solid;
							//int? releaseId = WebPartBase.GetProjectSetting("ProjectHome.GeneralSettings", "SelectedReleaseId", (int?)null);

							//Convert old -1 saved releases to null
							//if (releaseId.HasValue && releaseId < 1)
							//{
							//releaseId = null;
							//}

							//Get the list of risks impacts, sorted by score descending then id
							//List<RiskImpact> impacts1 = riskManager.RiskImpact_Retrieve(ProjectTemplateId).OrderByDescending(p => p.Score).ThenBy(p => p.RiskImpactId).ToList();

							//Get the list of open risks in the project/release
							Hashtable filters = new Hashtable();
							filters.Add("RiskStatusId", RiskManager.RiskStatusId_AllOpen);
							//if (releaseId.HasValue)
							//{
							//	filters.Add("ReleaseId", releaseId.Value);
							//}
							TimeZoneInfo localTimeZone = TimeZoneInfo.Local;
							List<RiskView> openRisks = riskManager.Risk_Retrieve(projectId, "RiskId", true, 1, Int32.MaxValue, filters, localTimeZone.GetUtcOffset(DateTime.Now).TotalHours);

							//Set the number of risks
							if (openRisks != null)
							{
								int count = openRisks.Count(r => r.RiskProbabilityId == probabilities.ElementAt(j).RiskProbabilityId && r.RiskImpactId == impacts[i].RiskImpactId);
								if (count > 0)
								{
									//Find the button
									///LinkButton button = (LinkButton)cell.Controls[0];
									cell.Text = count.ToString();
									cell.Font.Bold = true;
									cell.HorizontalAlign = HorizontalAlign.Center;
									cell.BorderColor = Color.White;
									cell.BorderStyle = BorderStyle.Solid;
									//button.CommandArgument = probabilityId + "," + impact.RiskImpactId;
								}
							}
							//cell.Text = (probabilityScore * impactScore).ToString();
							row.Height = 40;
							row.Width = 60;
							row.BorderColor = Color.White;
							row.BorderStyle = BorderStyle.Solid;
							row.Cells.Add(cell);
						}

						tableContent.Rows.Add(row);
					}

					for (int i = 1; i < tableContent.Rows.Count; i++)
					{
						for (int j = 1; j < tableContent.Rows[i].Cells.Count; j++)
						{
							int score = Int32.Parse(tableContent.Rows[i].Cells[j].ToolTip);
							var colorHex = InterpolateColor3(score, matrixSize);
							//var color = Color.FromArgb(255, // hardcoded opaque
							//			int.Parse(colorHex.Substring(0, 2), NumberStyles.HexNumber),
							//			int.Parse(colorHex.Substring(2, 2), NumberStyles.HexNumber),
							//			int.Parse(colorHex.Substring(4, 2), NumberStyles.HexNumber));
							tableContent.Rows[i].Cells[j].BackColor = colorHex;
						}
					}
				}

				tableContent.RenderControl(new HtmlTextWriter(sw));

				html = sw.ToString();
			}

			return html;

		}

		public string GetReportRiskSummaryTable(int releaseId, int projectId, int matrix)
		{
			string html = String.Empty;
			Table tableContent = new Table();

			TableRow row = new TableRow();
			TableCell cell = new TableCell();
			tableContent.ID = "Risk Summary";

			row.Height = 60;
			row.Width = 60;
			row.BorderColor = Color.White;
			row.BorderStyle = BorderStyle.Solid;
			row.Cells.Add(cell);

			int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

			List<RiskProbability> probabilities = new List<RiskProbability>();
			List<RiskImpact> impacts = new List<RiskImpact>();
			using (StringWriter sw = new StringWriter())
			{
				if (projectTemplateId > 0)
				{
					//We need to dynamically add the risk probabilities as columns to the Risk Summary table
					//We sort them in order of increasing score, not position, so need to resort first
					RiskManager riskManager = new RiskManager();
					probabilities = riskManager.RiskProbability_RetrieveMatrix(projectTemplateId).OrderBy(p => p.Score).Take(matrix).ToList();
					for (int i = 0; i < probabilities.Count; i++)
					{
						cell = new TableCell();

						cell.Text = probabilities[i].Name;
						cell.BorderColor = Color.White;
						cell.BorderStyle = BorderStyle.Solid;
						row.Height = 560;
						row.Width = 560;
						row.BorderColor = Color.White;
						row.BorderStyle = BorderStyle.Solid;
						row.Cells.Add(cell);
					}

					tableContent.Rows.Add(row);

					//int matrixSize = riskManager.Risk_MatrixSize(projectId);
					impacts = riskManager.RiskImpact_RetrieveMatrix(projectTemplateId).OrderByDescending(p => p.Score).Take(matrix).ToList();
					for (int i = 0; i < impacts.Count; i++)
					{
						row = new TableRow();
						cell = new TableCell();

						cell.Text = impacts[i].Name;
						cell.BorderColor = Color.White;
						cell.BorderStyle = BorderStyle.Solid;
						row.Height = 560;
						row.Width = 560;
						row.BorderColor = Color.White;
						row.BorderStyle = BorderStyle.Solid;
						row.Cells.Add(cell);

						for (int j = 0; j < probabilities.Count; j++)
						{
							cell = new TableCell();
							cell.Width = 100;
							cell.Height = 100;
							var probabilityScore = Convert.ToInt32(probabilities.ElementAt(j).Score.ToString());
							var impactScore = Convert.ToInt32(impacts.ElementAt(i).Score.ToString());
							cell.ToolTip = (probabilityScore * impactScore).ToString();
							cell.BorderColor = Color.White;
							cell.BorderStyle = BorderStyle.Solid;

							//Get the list of open risks in the project/release
							Hashtable filters = new Hashtable();
							filters.Add("RiskStatusId", RiskManager.RiskStatusId_AllOpen);

							if (releaseId != -1)
							{
								if (releaseId > 0)
								{
									filters.Add("ReleaseId", releaseId);
								}
							}

							TimeZoneInfo localTimeZone = TimeZoneInfo.Local;
							List<RiskView> openRisks = riskManager.Risk_Retrieve(projectId, "RiskId", true, 1, Int32.MaxValue, filters, localTimeZone.GetUtcOffset(DateTime.Now).TotalHours);

							//Set the number of risks
							if (openRisks != null)
							{
								int count = openRisks.Count(r => r.RiskProbabilityId == probabilities.ElementAt(j).RiskProbabilityId && r.RiskImpactId == impacts[i].RiskImpactId);
								if (count > 0)
								{
									cell.Text = count.ToString();
									cell.Font.Bold = true;
									cell.HorizontalAlign = HorizontalAlign.Center;
									cell.BorderColor = Color.White;
									cell.BorderStyle = BorderStyle.Solid;
								}
							}

							row.Height = 560;
							row.Width = 560;
							row.BorderColor = Color.White;
							row.BorderStyle = BorderStyle.Solid;
							row.Cells.Add(cell);
						}

						tableContent.Rows.Add(row);
					}

					for (int i = 1; i < tableContent.Rows.Count; i++)
					{
						for (int j = 1; j < tableContent.Rows[i].Cells.Count; j++)
						{
							int score = Int32.Parse(tableContent.Rows[i].Cells[j].ToolTip);
							var colorHex = InterpolateColor4(score, matrix);
							tableContent.Rows[i].Cells[j].BackColor = colorHex;
						}
					}
				}

				tableContent.RenderControl(new HtmlTextWriter(sw));

				html = sw.ToString();
			}

			string RiskSummaryImagePath = "c:\\Program Files (x86)\\ValidationMaster\\Attachments\\RiskSummary\\";

			System.Drawing.Image image = TheArtOfDev.HtmlRenderer.WinForms.HtmlRender.RenderToImage(html);
			image.Save(RiskSummaryImagePath + "image.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

			return html;

		}

		public static Color InterpolateColor3(int weight, int matrixSize = 5)
		{
			try
			{
				switch (matrixSize)
				{
					case 3:
						{
							if (weight >= 5)
							{
								return ColorTranslator.FromHtml("#ff0000");
							}
							else if (weight >= 3)
							{
								return ColorTranslator.FromHtml("#ffff00");
							}
							else
							{
								return ColorTranslator.FromHtml("#32CD32");
							}
						}

					case 4:
						{
							if (weight >= 12)
							{
								return ColorTranslator.FromHtml("#ff0000");
							}
							else if (weight >= 7)
							{
								return ColorTranslator.FromHtml("#ff9900");
							}
							else if (weight >= 2)
							{
								return ColorTranslator.FromHtml("#ffff00");
							}
							else
							{
								return ColorTranslator.FromHtml("#32CD32");
							}
						}

					case 5:
						{
							if (weight >= 16)
							{
								return ColorTranslator.FromHtml("#ff0000");
							}
							else if (weight >= 10)
							{
								return ColorTranslator.FromHtml("#ff9900");
							}
							else if (weight >= 7)
							{
								return ColorTranslator.FromHtml("#ffff00");
							}
							else if (weight >= 3)
							{
								return ColorTranslator.FromHtml("#32CD32");
							}
							else
							{
								return ColorTranslator.FromHtml("#32CD32");
							}
						}

					default:
						{
							if (weight >= 15)
							{
								return ColorTranslator.FromHtml("#ff0000");
							}
							else if (weight <= 7)
							{
								return ColorTranslator.FromHtml("#32CD32");
							}
							else
							{
								return ColorTranslator.FromHtml("#99ff00");
							}
						}
				}

			}
			catch (ArgumentException)
			{
				//Happens if a bad hex code was input
				return new Color();
			}
		}

		public static Color InterpolateColor4(int weight, int matrixSize)
		{
			try
			{
				switch (matrixSize)
				{
					case 3:
						{
							if (weight >= 5)
							{//red
								return ColorTranslator.FromHtml("#bf0000");
							}
							else if (weight <= 2)
							{
								return ColorTranslator.FromHtml("#FFFF33");
							}
							else
							{
								return ColorTranslator.FromHtml("#32CD32");
							}
						}

					case 4:
						{
							if (weight >= 15)
							{
								//red
								return ColorTranslator.FromHtml("#bf0000");
							}
							else if (weight >= 7)
							{//yellow
								return ColorTranslator.FromHtml("#FFFF33");
							}
							else if (weight >= 2)
							{//green
								return ColorTranslator.FromHtml("#32CD32");
							}
							else
							{//blue
								return ColorTranslator.FromHtml("#1338BE");
							}
						}

					case 5:
						{
							if (weight >= 20)
							{
								return ColorTranslator.FromHtml("#bf0000");
							}
							else if (weight >= 10)
							{
								return ColorTranslator.FromHtml("#FFA500");
							}
							else if (weight >= 7)
							{
								return ColorTranslator.FromHtml("#FFFF33");
							}
							else if (weight >= 3)
							{
								return ColorTranslator.FromHtml("#32CD32");
							}
							else
							{
								return ColorTranslator.FromHtml("#1338BE");
							}
						}

					default:
						{
							if (weight >= 15)
							{
								return ColorTranslator.FromHtml("#bf0000");
							}
							else if (weight >= 10)
							{
								return ColorTranslator.FromHtml("#FFA500");
							}
							else if (weight >= 7)
							{
								return ColorTranslator.FromHtml("#FFFF33");
							}
							else if (weight >= 4)
							{
								return ColorTranslator.FromHtml("#32CD32");
							}
							else
							{
								return ColorTranslator.FromHtml("#1338BE");
							}
						}
				}

			}
			catch (ArgumentException)
			{
				//Happens if a bad hex code was input
				return new Color();
			}
		}


		/// <summary>
		/// Trims the whitespace in any XML nodelist
		/// </summary>
		/// <param name="xmlNodes">The xml node list</param>
		private void TrimNodes(XmlNodeList xmlNodes)
		{
			foreach (XmlNode xmlNode in xmlNodes)
			{
				if (xmlNode.HasChildNodes)
				{
					TrimNodes(xmlNode.ChildNodes);
				}
				else if (!String.IsNullOrEmpty(xmlNode.InnerText))
				{
					xmlNode.InnerText = xmlNode.InnerText.Trim();
				}

			}
		}

		/// <summary>
		/// Loads a dataset's XML as a child node of an existing XML node
		/// </summary>
		/// <param name="dataSet">The dataset whose data we want to insert</param>
		/// <param name="xmlParentDoc">The parent XML document</param>
		/// <param name="xmlParentNode">The parent XML node</param>
		/// <param name="childContainer">The name of the XML container to add the elements under</param>
		protected void InsertChildDataSet(DataSet dataSet, XmlDocument xmlParentDoc, XmlNode xmlParentNode, string childContainer)
		{
			XmlDocument xmlChildDoc = new XmlDocument();
			xmlChildDoc.LoadXml(dataSet.GetReportXml());
			XmlNode xmlChildNode = xmlParentDoc.CreateNode(XmlNodeType.Element, childContainer, "");
			xmlChildNode.InnerXml = xmlChildDoc.ChildNodes[0].InnerXml.Replace("xmlns=\"" + xmlChildDoc.ChildNodes[0].NamespaceURI + "\"", "");
			xmlParentNode.AppendChild(xmlChildNode);
		}

		/// <summary>
		/// Loads a single data-table's XML as a child node of an existing XML node
		/// </summary>
		/// <param name="dataSet">The dataset whose data we want to insert</param>
		/// <param name="xmlParentDoc">The parent XML document</param>
		/// <param name="xmlParentNode">The parent XML node</param>
		/// <param name="childContainer">The name of the XML container to add the elements under</param>
		/// <param name="tableName">The table we want to load</param>
		protected void InsertChildDataSet(DataSet dataSet, string tableName, XmlDocument xmlParentDoc, XmlNode xmlParentNode, string childContainer)
		{
			XmlDocument xmlChildDoc = new XmlDocument();
			xmlChildDoc.LoadXml(dataSet.GetReportXml());
			XmlNode xmlChildNode = xmlParentDoc.CreateNode(XmlNodeType.Element, childContainer, "");
			XmlNodeList xmlNodeList = xmlChildDoc.ChildNodes[0].SelectNodes(tableName);
			for (int i = 0; i < xmlNodeList.Count; i++)
			{
				xmlChildNode.InnerXml += "<" + tableName + ">" + xmlNodeList[i].InnerXml + "</" + tableName + ">";
			}
			xmlParentNode.AppendChild(xmlChildNode);
		}

		/// <summary>
		/// Loads a single entity'ss XML as a child node of an existing XML node
		/// </summary>
		/// <param name="entities">The entities whose data we want to insert</param>
		/// <param name="xmlParentDoc">The parent XML document</param>
		/// <param name="xmlParentNode">The parent XML node</param>
		/// <param name="childEntity">Override name for the child entity</param>
		/// <param name="childContainer">The name of the XML container to add the elements under</param>
		/// <param name="tableName">The table we want to load</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The container entity</returns>
		protected XmlNode InsertChildEntity<T>(int projectId, int projectTemplateId, List<T> entities, XmlDocument xmlParentDoc, XmlNode xmlParentNode, string childContainer, string timezoneId, List<DataSyncSystem> dataMappingSystems, string childEntity = null)
			where T : Entity
		{
			//Create the container first
			XmlNode xmlChildNode = xmlParentDoc.CreateNode(XmlNodeType.Element, childContainer, "");
			xmlParentNode.AppendChild(xmlChildNode);

			//Get the XML for each entity in the list
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			List<CustomProperty> customProperties = null;
			foreach (Entity entity in entities)
			{
				string nodeName = childEntity;
				if (String.IsNullOrEmpty(nodeName))
				{
					nodeName = entity.GetType().ToString();
				}
				XmlNode xmlEntityNode = xmlParentDoc.CreateElement(nodeName);
				xmlEntityNode.InnerXml = entity.InnerXml;
				xmlChildNode.AppendChild(xmlEntityNode);

				//If this is an Artifact then get its type and associated custom properties (if any exist)
				if (entity is Artifact)
				{
					Artifact artifact = (Artifact)entity;

					//Get the custom properties definition (if first time) and then artifact specific custom properties
					if (customProperties == null)
					{
						customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, artifact.ArtifactType, true, false, true);
					}
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, artifact.ArtifactId, artifact.ArtifactType);

					//Custom properties need to be specially parsed to add them to the XML in a way that can
					//be easily handled by XSLT.
					InsertCustomProperties(projectId, artifact.ArtifactId, artifact.ArtifactType, artifactCustomProperty, xmlParentDoc, xmlEntityNode, customProperties, timezoneId, dataMappingSystems);
				}
			}

			return xmlChildNode;
		}

		/// <summary>
		/// Adds the artifact history to an artifact node
		/// </summary>
		/// <param name="components">Components are only needed for test cases and incidents (where they are multi-selectable)</param>
		protected void InsertArtifactHistory(int projectId, int projectTemplateId, int artifactId, DataModel.Artifact.ArtifactTypeEnum artifactType, double utcOffset, XmlDocument xmlArtifactsDoc, XmlNode xmlArtifactNode, string timezoneId, List<DataSyncSystem> dataMappingSystems, List<CustomProperty> customProperties, List<Component> components = null)
		{
			HistoryManager historyManager = new HistoryManager();
			List<HistoryChangeSetResponse> historyChanges = historyManager.RetrieveByArtifactId(projectId, artifactId, artifactType, "ChangeDate", false, null, 1, Int32.MaxValue - 1, utcOffset, true);

			//Clean the history
			//CleanRichTextFields(historyChanges, new string[] { "OldValue", "NewValue" });

			//Loop through and lookup the display name of any list/multilist values
			if (customProperties != null)
			{
				//foreach (HistoryChangeSetResponse historyChange in historyChanges)
				//{

				//Get the change history as XML and add to the requirement XML
				InsertChildEntity<HistoryChangeSetResponse>(projectId, projectTemplateId, historyChanges, xmlArtifactsDoc, xmlArtifactNode, "History", timezoneId, dataMappingSystems, "ArtifactHistory");
				//}
			}
		}

		/// <summary>
		/// Loads a list of source code files into an existing childContainer
		/// </summary>
		/// <param name="artifactType">The type of artifact</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <param name="xmlParentDoc">The parent XML document</param>
		/// <param name="xmlParentNode">The parent XML node</param>
		/// <param name="childContainer">The name of the XML container to add the elements under (must already exist)</param>
		protected void InsertSourceCodeAttachments(int projectId, DataModel.Artifact.ArtifactTypeEnum artifactType, int artifactId, XmlDocument xmlParentDoc, XmlNode xmlParentNode, string childContainer)
		{
			const string METHOD_NAME = "InsertSourceCodeAttachments";

			//GER - 20220803 - Removed this code for ValidationMaster.  There are not limits to licensing of ValidationMaster.
			//if (Common.License.LicenseProductName == LicenseProductNameEnum.SpiraPlan || Common.License.LicenseProductName == LicenseProductNameEnum.SpiraTeam)
			{
				try
				{
					SourceCodeManager sourceCode = new SourceCodeManager(projectId);
					List<SourceCodeFile> sourceCodeFiles = sourceCode.RetrieveFilesForArtifact(artifactType, artifactId, null);

					XmlNode xmlChildNode = xmlParentNode.SelectSingleNode(childContainer);
					if (xmlChildNode != null)
					{
						//Add the source code files, converting them to the same XML schema as the attachments
						foreach (SourceCodeFile sourceCodeFile in sourceCodeFiles)
						{
							XmlNode xmlAttachment = xmlParentDoc.CreateElement("Attachment");
							XmlNode xmlField = xmlParentDoc.CreateElement("Filename");
							xmlField.InnerText = sourceCodeFile.Name;
							xmlAttachment.AppendChild(xmlField);
							xmlField = xmlParentDoc.CreateElement("Description");
							xmlField.InnerText = GlobalResources.General.Global_Revision + ": " + sourceCodeFile.RevisionName;
							xmlAttachment.AppendChild(xmlField);
							xmlField = xmlParentDoc.CreateElement("AuthorName");
							xmlField.InnerText = sourceCodeFile.AuthorName;
							xmlAttachment.AppendChild(xmlField);
							xmlField = xmlParentDoc.CreateElement("UploadDate");
							xmlField.InnerText = String.Format(FORMAT_DATE_TIME_XML, sourceCodeFile.LastUpdateDate);
							xmlAttachment.AppendChild(xmlField);
							//Add the new elements
							xmlChildNode.AppendChild(xmlAttachment);
						}
					}
				}
				catch (SourceCodeProviderGeneralException exception)
				{
					//Ignore - can happen if the source code provider is not correctly configured
					Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Unable to add source code attachments to report - " + exception.Message);
					Logger.Flush();
				}
			}
		}

		/// <summary>
		/// Loads a list of source code revisions into a childContainer
		/// </summary>
		/// <param name="artifactType">The type of artifact</param>
		/// <param name="artifactId">The id of the artifact</param>
		/// <param name="xmlParentDoc">The parent XML document</param>
		/// <param name="xmlParentNode">The parent XML node</param>
		/// <param name="childContainer">The name of the XML container to add the elements under, which it will create</param>
		protected void InsertSourceCodeRevisions(int userId, int projectId, DataModel.Artifact.ArtifactTypeEnum artifactType, int artifactId, XmlDocument xmlParentDoc, XmlNode xmlParentNode, string childContainer)
		{
			const string METHOD_NAME = "InsertSourceCodeRevisions";

			//GER - 20220803 - Removed this code for ValidationMaster.  There are not limits to licensing of ValidationMaster.
			//if (Common.License.LicenseProductName == LicenseProductNameEnum.SpiraPlan || Common.License.LicenseProductName == LicenseProductNameEnum.SpiraTeam)
			{
				try
				{
					//We get the revisions for the default branch, so pass null
					SourceCodeManager sourceCode = new SourceCodeManager(projectId);
					List<SourceCodeCommit> sourceCodeRevisions = sourceCode.RetrieveRevisionsForArtifact(artifactType, artifactId);

					XmlNode xmlChildNode = xmlParentNode.SelectSingleNode(childContainer);
					if (xmlChildNode == null)
					{
						//Create the node if it doesn't exist
						xmlChildNode = xmlParentDoc.CreateElement(childContainer);
						xmlParentNode.AppendChild(xmlChildNode);
					}
					//Add the source code revision as XML elements
					foreach (SourceCodeCommit sourceCodeRevision in sourceCodeRevisions)
					{
						XmlNode xmlAttachment = xmlParentDoc.CreateElement("SourceCodeRevision");
						XmlNode xmlField = xmlParentDoc.CreateElement("Name");
						xmlField.InnerText = sourceCodeRevision.Name;
						xmlAttachment.AppendChild(xmlField);
						xmlField = xmlParentDoc.CreateElement("Message");
						xmlField.InnerText = sourceCodeRevision.Message;
						xmlAttachment.AppendChild(xmlField);
						xmlField = xmlParentDoc.CreateElement("AuthorName");
						xmlField.InnerText = sourceCodeRevision.AuthorName;
						xmlAttachment.AppendChild(xmlField);
						xmlField = xmlParentDoc.CreateElement("UpdateDate");
						xmlField.InnerText = String.Format(FORMAT_DATE_TIME_XML, sourceCodeRevision.UpdateDate);
						xmlAttachment.AppendChild(xmlField);
						//Add the new elements
						xmlChildNode.AppendChild(xmlAttachment);
					}
				}
				catch (SourceCodeProviderGeneralException exception)
				{
					//Ignore - can happen if the source code provider is not correctly configured
					Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Unable to add source code revisions to report - " + exception.Message);
					Logger.Flush();
				}
			}
		}

		/// <summary>
		/// Inserts the list of comments into the artifact XML
		/// </summary>
		/// <param name="comments"></param>
		/// <param name="itemName">The name of the XML item to create for each item</param>
		/// <param name="xmlParentDoc">The parent XML document</param>
		/// <param name="xmlParentNode">The parent XML node</param>
		/// <param name="childContainer">The name of the XML container to add the elements under, which it will create</param>
		protected void InsertComments(IEnumerable<IDiscussion> comments, string itemName, XmlDocument xmlParentDoc, XmlNode xmlParentNode, string childContainer)
		{
			//If collection is non-nullable, loop through
			if (comments != null)
			{
				//Create the parent container if it does not exist
				XmlNode xmlChildNode = xmlParentNode.SelectSingleNode(childContainer);
				if (xmlChildNode == null)
				{
					//Create the node if it doesn't exist
					xmlChildNode = xmlParentDoc.CreateElement(childContainer);
					xmlParentNode.AppendChild(xmlChildNode);
				}

				foreach (IDiscussion comment in comments)
				{
					//Make sure the comment derives from entity
					if (comment is Entity)
					{
						Entity entity = (Entity)comment;
						XmlNode xmlItem = xmlParentDoc.CreateElement(itemName);
						xmlItem.InnerXml = entity.InnerXml;
						//Add the new elements
						xmlChildNode.AppendChild(xmlItem);
					}
				}
			}
		}

		/// <summary>
		/// Loads a list of builds into a childContainer
		/// </summary>
		/// <param name="releaseId">The id of the release that the builds belong to</param>
		/// <param name="xmlParentDoc">The parent XML document</param>
		/// <param name="utcOffset">The offset from UTC</param>
		/// <param name="xmlParentNode">The parent XML node</param>
		/// <param name="childContainer">The name of the XML container to add the elements under, which it will create</param>
		protected void InsertBuilds(int projectId, int releaseId, XmlDocument xmlParentDoc, XmlNode xmlParentNode, string childContainer, double utcOffset)
		{
			BuildManager buildManager = new BuildManager();
			List<BuildView> builds = buildManager.RetrieveForRelease(projectId, releaseId, utcOffset);

			XmlNode xmlChildNode = xmlParentNode.SelectSingleNode(childContainer);
			if (xmlChildNode == null)
			{
				//Create the node if it doesn't exist
				xmlChildNode = xmlParentDoc.CreateElement(childContainer);
				xmlParentNode.AppendChild(xmlChildNode);
			}

			//Add the builds as XML elements
			foreach (BuildView build in builds)
			{
				XmlNode xmlBuild = xmlParentDoc.CreateElement("Build");
				xmlBuild.InnerXml = build.InnerXml;
				//Add the new elements
				xmlChildNode.AppendChild(xmlBuild);
			}
		}


		/// <summary>
		/// Loads a artifact's XML custom properties as a child node of an existing XML node
		/// </summary>
		/// <param name="artifactId">The id of the artifact</param>
		/// <param name="artifactType">The type of the artifact</param>
		/// <param name="artifactCustomProperty">The custom property entity whose data we want to insert</param>
		/// <param name="xmlParentDoc">The parent XML document</param>
		/// <param name="xmlParentNode">The parent XML node</param>
		/// <param name="customProperties">The custom property definitions</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="timezoneId">The id of the timezone</param>
		/// <param name="dataSyncSystems">The list of any data sync systems for the project</param>
		protected void InsertCustomProperties(int projectId, int artifactId, Artifact.ArtifactTypeEnum artifactType, ArtifactCustomProperty artifactCustomProperty, XmlDocument xmlParentDoc, XmlNode xmlParentNode, List<CustomProperty> customProperties, string timezoneId, List<DataSyncSystem> dataSyncSystems)
		{
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();

			//First create the Custom Properties Node
			XmlNode xmlCustomPropertiesNode = xmlParentDoc.CreateNode(XmlNodeType.Element, "CustomProperties", "");

			//Now get the custom properties for this artifact and add to the custom property node
			foreach (CustomProperty customProperty in customProperties)
			{
				XmlNode xmlCustomPropertyNode = xmlParentDoc.CreateNode(XmlNodeType.Element, "CustomProperty", "");
				xmlCustomPropertiesNode.AppendChild(xmlCustomPropertyNode);

				//Add the alias name of the property
				XmlNode xmlCustomPropertyAlias = xmlParentDoc.CreateNode(XmlNodeType.Element, "Alias", "");
				xmlCustomPropertyAlias.InnerText = customProperty.Name;
				xmlCustomPropertyNode.AppendChild(xmlCustomPropertyAlias);

				//Add the name of the property
				XmlNode xmlCustomPropertyName = xmlParentDoc.CreateNode(XmlNodeType.Element, "Name", "");
				xmlCustomPropertyName.InnerText = customProperty.CustomPropertyFieldName;
				xmlCustomPropertyNode.AppendChild(xmlCustomPropertyName);

				//Add the type of the property
				XmlNode xmlCustomPropertyType = xmlParentDoc.CreateNode(XmlNodeType.Element, "Type", "");
				xmlCustomPropertyType.InnerText = customProperty.CustomPropertyTypeName;
				xmlCustomPropertyNode.AppendChild(xmlCustomPropertyType);

				//Now get the value of this property from the custom property dataset
				if (artifactCustomProperty != null)
				{
					string rawValue = (string)artifactCustomProperty[customProperty.CustomPropertyFieldName];

					//Handle each of the different types
					string displayValue = "";
					switch ((CustomProperty.CustomPropertyTypeEnum)customProperty.CustomPropertyTypeId)
					{
						case CustomProperty.CustomPropertyTypeEnum.User:
							{
								int intValue = (rawValue.FromDatabaseSerialization_Int32().HasValue) ? rawValue.FromDatabaseSerialization_Int32().Value : -1;
								//Get the user's name from the user manager (expensive operation but not many people
								//use User custom properties)
								if (intValue > 0)
								{
									try
									{
										User user = new UserManager().GetUserById(intValue);
										if (user != null)
										{
											displayValue = user.FullName;
										}
									}
									catch (ArtifactNotExistsException)
									{
										//Ignore
									}
								}
								break;
							}

						case CustomProperty.CustomPropertyTypeEnum.List:
							{
								int intValue = (rawValue.FromDatabaseSerialization_Int32().HasValue) ? rawValue.FromDatabaseSerialization_Int32().Value : -1;
								//Get the custom property value display text
								if (customProperty.List != null)
								{
									CustomPropertyValue cpv = customProperty.List.Values.FirstOrDefault(cv => cv.CustomPropertyValueId == intValue);
									if (cpv != null)
									{
										displayValue = cpv.Name;
									}
								}
								break;
							}

						case CustomProperty.CustomPropertyTypeEnum.MultiList:
							{
								List<int> values = rawValue.FromDatabaseSerialization_List_Int32();
								//For an editable results we just need comma-separated values
								//Get the custom property value display text
								if (customProperty.List != null && values != null && values.Count > 0)
								{
									//Display the value or the word '(Multiple)'
									if (values.Count > 1)
									{
										string tooltip = "";
										foreach (uint customPropertyValueId in values)
										{
											CustomPropertyValue cpv = customProperty.List.Values.FirstOrDefault(cv => cv.CustomPropertyValueId == customPropertyValueId);
											if (cpv != null)
											{
												if (tooltip == "")
												{
													tooltip = cpv.Name;
												}
												else
												{
													tooltip += ", " + cpv.Name;
												}
											}
										}
										displayValue = tooltip;
									}
									else
									{
										CustomPropertyValue cpv = customProperty.List.Values.FirstOrDefault(cv => cv.CustomPropertyValueId == values[0]);
										if (cpv != null)
										{
											displayValue = cpv.Name;
										}
									}
								}
							}
							break;


						case CustomProperty.CustomPropertyTypeEnum.Boolean:
							{
								bool? booleanValue = rawValue.FromDatabaseSerialization_Boolean();
								if (booleanValue.HasValue)
								{
									//The ajax controls expect Y/N in the text
									displayValue = (booleanValue.Value) ? "Y" : "N";
								}
							}
							break;

						case CustomProperty.CustomPropertyTypeEnum.Integer:
							{
								int? intValue = rawValue.FromDatabaseSerialization_Int32();
								if (intValue.HasValue)
								{
									intValue = intValue.Value;
									displayValue = intValue.Value.ToString();
								}
							}
							break;

						case CustomProperty.CustomPropertyTypeEnum.Decimal:
							{
								decimal? decValue = rawValue.FromDatabaseSerialization_Decimal();
								if (decValue.HasValue)
								{
									//Handle decimal precision
									CustomPropertyOptionValue optValue = customProperty.Options.FirstOrDefault(c => c.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.Precision);
									if (optValue != null)
									{
										int precision;
										if (Int32.TryParse(optValue.Value, out precision))
										{
											//If we round, need to display the unrounded version as a tooltip
											decimal roundedDec = Decimal.Round(decValue.Value, precision);
											displayValue = roundedDec.ToString();
										}
									}
									else
									{
										displayValue = decValue.Value.ToString();
									}
								}
							}
							break;

						case CustomProperty.CustomPropertyTypeEnum.Date:
							{
								DateTime? dateTimeValue = rawValue.FromDatabaseSerialization_DateTime();
								if (dateTimeValue.HasValue)
								{
									//Leave in UTC since we convert the whole document later, but put in a standardized format
									displayValue = String.Format(FORMAT_DATE_TIME_XML, dateTimeValue.Value);
								}
							}
							break;

						case CustomProperty.CustomPropertyTypeEnum.Text:
							{
								//See if we have rich-text enabled for this field or not
								bool isRichText = false;
								CustomPropertyOptionValue optValue = customProperty.Options.FirstOrDefault(c => c.CustomPropertyOptionId == (int)CustomProperty.CustomPropertyOptionEnum.RichText);
								if (optValue != null && optValue.Value.FromDatabaseSerialization_Boolean().HasValue && optValue.Value.FromDatabaseSerialization_Boolean().Value)
								{
									isRichText = true;
								}

								if (isRichText)
								{
									string dirtyText = rawValue.FromDatabaseSerialization_String();
									displayValue = CleanRichText(dirtyText);
								}
								else
								{
									displayValue = rawValue.FromDatabaseSerialization_String();
								}
							}
							break;
					}

					//Add the value if we have one
					if (!String.IsNullOrEmpty(displayValue))
					{
						XmlNode xmlCustomPropertyValue = xmlParentDoc.CreateNode(XmlNodeType.Element, "Value", "");
						xmlCustomPropertyValue.InnerText = displayValue;
						xmlCustomPropertyNode.AppendChild(xmlCustomPropertyValue);
					}
				}
			}

			//If the project has data-synchronization enabled, also add the sync ids if they exist
			if (dataSyncSystems != null && dataSyncSystems.Count > 0)
			{
				DataMappingManager dataMappingManager = new DataMappingManager();
				List<DataSyncArtifactMapping> artifactDataMappings = dataMappingManager.RetrieveDataSyncArtifactMappings(projectId, artifactType, artifactId);
				if (artifactDataMappings.Count > 0)
				{
					foreach (DataSyncArtifactMapping mappingRow in artifactDataMappings)
					{
						//Find the matching system name
						DataSyncSystem dataSyncSystem = dataSyncSystems.FirstOrDefault(d => d.DataSyncSystemId == mappingRow.DataSyncSystemId);
						if (dataSyncSystem != null)
						{
							XmlNode xmlCustomPropertyNode = xmlParentDoc.CreateNode(XmlNodeType.Element, "CustomProperty", "");
							xmlCustomPropertiesNode.AppendChild(xmlCustomPropertyNode);

							//Add the alias name of the property
							if (!String.IsNullOrEmpty(dataSyncSystem.Name))
							{
								XmlNode xmlCustomPropertyAlias = xmlParentDoc.CreateNode(XmlNodeType.Element, "Alias", "");
								xmlCustomPropertyAlias.InnerText = dataSyncSystem.Name;
								xmlCustomPropertyNode.AppendChild(xmlCustomPropertyAlias);

								//Add the name of the property
								XmlNode xmlCustomPropertyName = xmlParentDoc.CreateNode(XmlNodeType.Element, "Name", "");
								xmlCustomPropertyName.InnerText = dataSyncSystem.Name;
								xmlCustomPropertyNode.AppendChild(xmlCustomPropertyName);
							}

							//Add the type of the property
							XmlNode xmlCustomPropertyType = xmlParentDoc.CreateNode(XmlNodeType.Element, "Type", "");
							xmlCustomPropertyType.InnerText = "DataSync";
							xmlCustomPropertyNode.AppendChild(xmlCustomPropertyType);

							//Add the value of the property
							if (!String.IsNullOrEmpty(mappingRow.ExternalKey))
							{
								XmlNode xmlCustomPropertyValue = xmlParentDoc.CreateNode(XmlNodeType.Element, "Value", "");
								xmlCustomPropertyValue.InnerText = mappingRow.ExternalKey;
								xmlCustomPropertyNode.AppendChild(xmlCustomPropertyValue);
							}
						}
					}
				}
			}

			//Finally add the custom properties to the requirements package
			xmlParentNode.AppendChild(xmlCustomPropertiesNode);
		}

		/// <summary>
		/// Localizes a UTC date for the specific user's timezone
		/// </summary>
		/// <param name="timezoneId">The id/name of the desired timezone</param>
		/// <param name="utcDate">The universal date</param>
		/// <returns>The localized date</returns>
		private DateTime LocalizeDate(DateTime utcDate, string timezoneId)
		{
			//Force the date kind to be UTC
			utcDate = DateTime.SpecifyKind(utcDate, DateTimeKind.Utc);

			if (String.IsNullOrEmpty(timezoneId))
			{
				//Fallback to using the system local time
				return utcDate.ToLocalTime();
			}
			TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
			if (timeZone == null)
			{
				//Fallback to using the system local time
				return utcDate.ToLocalTime();
			}
			return TimeZoneInfo.ConvertTimeFromUtc(utcDate, timeZone);
		}


		/// <summary>
		/// Gets the project name and description for the report header
		/// </summary>
		/// <param name="reportFormatToken">The token for the report format</param>
		/// <param name="timezoneId">The </param>
		protected XmlDocument Get_ProjectOverview(int projectId, string reportFormatToken, string timezoneId, UpdateBackgroundProcessStatus updateBackgroundProcessStatus, int progress)
		{
			//Get the project information
			ProjectManager projectManager = new ProjectManager();
			Project project = projectManager.RetrieveById(projectId);
			int projectTemplateId = project.ProjectTemplateId;

			//Now get the project record XML node
			XmlDocument xmlProjectDoc = new XmlDocument();
			XmlNode xmlProjectData = xmlProjectDoc.CreateElement("ProjectData");
			xmlProjectDoc.AppendChild(xmlProjectData);

			XmlNode xmlProjectNode = xmlProjectDoc.CreateElement("Project");
			xmlProjectNode.InnerXml = project.InnerXml;
			xmlProjectData.AppendChild(xmlProjectNode);

			//Localize any dates in the document
			LocalizeDates(xmlProjectDoc, timezoneId);

			if (updateBackgroundProcessStatus != null)
			{
				updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
			}

			return xmlProjectDoc;
		}

		/// <summary>
		/// Localizes all the dates in a given xml document
		/// </summary>
		/// <param name="timezoneId">The id of the timezone</param>
		/// <param name="dataTable">The table of data that needs to have its dates localized</param>
		protected void LocalizeDates(XmlDocument xmlDoc, string timezoneId)
		{
			if (xmlDoc.HasChildNodes)
			{
				LocalizeDates(xmlDoc.ChildNodes, timezoneId);
			}
		}

		/// <summary>
		/// Localizes all the dates in a given xml node list
		/// </summary>
		/// <param name="timezoneId">The id of the timezone</param>
		/// <param name="dataTable">The table of data that needs to have its dates localized</param>
		protected void LocalizeDates(XmlNodeList xmlNodeList, string timezoneId)
		{
			//Loop through all the nodes
			foreach (XmlNode xmlNode in xmlNodeList)
			{
				if (xmlNode.NodeType == XmlNodeType.Element)
				{
					string innerText = xmlNode.InnerText;
					if (!String.IsNullOrEmpty(innerText))
					{
						//See if we have a date (use DateTimeStyles.None as DateTimeStyles.AssumeUniversal was causing it to be double-corrected!)
						DateTime utcDate;
						if (DateTime.TryParseExact(innerText, PARSE_DATE_TIME_XML, CultureInfo.InvariantCulture, DateTimeStyles.None, out utcDate))
						{
							//Standard dates
							DateTime localDate = LocalizeDate(utcDate, timezoneId);
							xmlNode.InnerText = String.Format(FORMAT_DATE_TIME_XML, localDate);
						}
						else if (DateTime.TryParseExact(innerText, DatabaseExtensions.FORMAT_DATE_TIME_INVARIANT, CultureInfo.InvariantCulture, DateTimeStyles.None, out utcDate))
						{
							//Custom property dates
							DateTime localDate = LocalizeDate(utcDate, timezoneId);
							xmlNode.InnerText = String.Format(FORMAT_DATE_TIME_XML, localDate);
						}
						else if (DateTime.TryParseExact(innerText, PARSE_DATE_TIME_XML_WITH_TIMEZONE, CultureInfo.InvariantCulture, DateTimeStyles.None, out utcDate))
						{
							//Dates with timezones
							DateTime localDate = LocalizeDate(utcDate, timezoneId);
							xmlNode.InnerText = String.Format(FORMAT_DATE_TIME_XML, localDate);
						}
					}

					//See if we have child nodes
					if (xmlNode.HasChildNodes)
					{
						LocalizeDates(xmlNode.ChildNodes, timezoneId);
					}
				}
			}
		}

		/// <summary>
		/// Makes a string safe for use in XML
		/// </summary>
		/// <param name="input">The input string (as object)</param>
		/// <returns>The output string</returns>
		protected string MakeXmlSafe(object input)
		{
			//Handle null reference case
			if (input == null)
			{
				return "";
			}

			//Handle empty string case
			string inputString = (string)input;
			if (inputString == "")
			{
				return inputString;
			}

			string output = inputString.Replace("\x00", "");
			output = output.Replace("\n", "");
			output = output.Replace("\r", "");
			output = output.Replace("\x01", "");
			output = output.Replace("\x02", "");
			output = output.Replace("\x03", "");
			output = output.Replace("\x04", "");
			output = output.Replace("\x05", "");
			output = output.Replace("\x06", "");
			output = output.Replace("\x07", "");
			output = output.Replace("\x08", "");
			output = output.Replace("\x0B", "");
			output = output.Replace("\x0C", "");
			output = output.Replace("\x0E", "");
			output = output.Replace("\x0F", "");
			output = output.Replace("\x10", "");
			output = output.Replace("\x11", "");
			output = output.Replace("\x12", "");
			output = output.Replace("\x13", "");
			output = output.Replace("\x14", "");
			output = output.Replace("\x15", "");
			output = output.Replace("\x16", "");
			output = output.Replace("\x17", "");
			output = output.Replace("\x18", "");
			output = output.Replace("\x19", "");
			output = output.Replace("\x1A", "");
			output = output.Replace("\x1B", "");
			output = output.Replace("\x1C", "");
			output = output.Replace("\x1D", "");
			output = output.Replace("\x1E", "");
			output = output.Replace("\x1F", "");
			return output;
		}

		/// <summary>
		/// Adds an Indent Level field to folder nodes
		/// </summary>
		/// <param name="xmlNode">The XML Node</param>
		/// <param name="indentLevel">The indent level string to include</param>
		protected void AddIndentLevel(XmlNode xmlNode, string indentLevel)
		{
			XmlNode xmlIndentLevel = xmlNode.OwnerDocument.CreateElement("IndentLevel");
			xmlNode.AppendChild(xmlIndentLevel);
			xmlIndentLevel.InnerText = indentLevel;
		}

		/// <summary>
		/// Converts named HTML entities (such as &uuml;) to their unicode hex equivalents that can be used in XML
		/// Fixes an issue where foreign characters were not being displayed correctly
		/// </summary>
		/// <param name="html">The HTML to convert</param>
		/// <returns></returns>
		public static string EntityToUnicode(string html)
		{
			Dictionary<string, string> replacements = new Dictionary<string, string>();
			Regex regex = new Regex("(&[a-z]{2,6}[0-9]{0,2};)", RegexOptions.IgnoreCase);
			foreach (Match match in regex.Matches(html))
			{
				if (!replacements.ContainsKey(match.Value))
				{
					string unicode = HttpUtility.HtmlDecode(match.Value);
					if (unicode.Length == 1)
					{
						replacements.Add(match.Value, string.Concat("&#", Convert.ToInt32(unicode[0]), ";"));
					}
				}
			}
			foreach (KeyValuePair<string, string> replacement in replacements)
			{
				html = html.Replace(replacement.Key, replacement.Value);
			}
			return html;
		}

		/// <summary>
		/// Converts the passed in HTML to the output expected by the native format
		/// </summary>
		/// <param name="token">The output format we're converting to</param>
		/// <param name="htmlMarkup">The HTML markup that needs converting</param>
		/// <returns>The converted output</returns>
		/// <param name="appRootPath">Path to the folder where the application can be found</param>
		/// <remarks>If we can't handle the format, we just use plain text</remarks>
		protected string HtmlConvertToTargetFormat(string token, string htmlMarkup, string appRootPath)
		{
			const string METHOD_NAME = "HtmlConvertToTargetFormat";

			try
			{
				//First make the markup XML safe (remove any control characters)
				htmlMarkup = MakeXmlSafe(htmlMarkup);

				//Read in the HTML as SGML and then use the appropriate XSLT to get it to the necessary output
				//format - need to convert HTML entities into something all XML specs can handle
				htmlMarkup = htmlMarkup.Replace("&nbsp;", "&#160;");
				htmlMarkup = htmlMarkup.Replace("&quot;", "&#34;");
				htmlMarkup = htmlMarkup.Replace("&lt;", "&#60;");
				htmlMarkup = htmlMarkup.Replace("&gt;", "&#62;");
				htmlMarkup = htmlMarkup.Replace("&amp;", "&#38;");
				htmlMarkup = htmlMarkup.Replace("&#x0", "");
				htmlMarkup = htmlMarkup.Replace("&#x1", "");
				htmlMarkup = htmlMarkup.Replace("&#x2", "");
				htmlMarkup = htmlMarkup.Replace("&#x3", "");
				htmlMarkup = htmlMarkup.Replace("&#x4", "");
				htmlMarkup = htmlMarkup.Replace("&#x5", "");
				htmlMarkup = htmlMarkup.Replace("&#x6", "");
				htmlMarkup = htmlMarkup.Replace("&#x7", "");
				htmlMarkup = htmlMarkup.Replace("&#x8", "");
				htmlMarkup = htmlMarkup.Replace("&#xB", "");
				htmlMarkup = htmlMarkup.Replace("&#xC", "");
				htmlMarkup = htmlMarkup.Replace("&#xE", "");
				htmlMarkup = htmlMarkup.Replace("&#xF", "");
				htmlMarkup = htmlMarkup.Replace("&#x10", "");
				htmlMarkup = htmlMarkup.Replace("&#x11", "");
				htmlMarkup = htmlMarkup.Replace("&#x12", "");
				htmlMarkup = htmlMarkup.Replace("&#x13", "");
				htmlMarkup = htmlMarkup.Replace("&#x14", "");
				htmlMarkup = htmlMarkup.Replace("&#x15", "");
				htmlMarkup = htmlMarkup.Replace("&#x16", "");
				htmlMarkup = htmlMarkup.Replace("&#x17", "");
				htmlMarkup = htmlMarkup.Replace("&#x18", "");
				htmlMarkup = htmlMarkup.Replace("&#x19", "");
				htmlMarkup = htmlMarkup.Replace("&#x1A", "");
				htmlMarkup = htmlMarkup.Replace("&#x1B", "");
				htmlMarkup = htmlMarkup.Replace("&#x1C", "");
				htmlMarkup = htmlMarkup.Replace("&#x1D", "");
				htmlMarkup = htmlMarkup.Replace("&#x1E", "");
				htmlMarkup = htmlMarkup.Replace("&#x1F", "");

				//Now any named entities (e.g. &uuml;)
				htmlMarkup = EntityToUnicode(htmlMarkup);

				//Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, htmlDoc);
				StringReader stringReader = new StringReader(htmlMarkup);
				SgmlReader sgmlReader = new SgmlReader();
				sgmlReader.DocType = "HTML";
				sgmlReader.WhitespaceHandling = WhitespaceHandling.All;
				sgmlReader.CaseFolding = CaseFolding.ToLower;
				sgmlReader.InputStream = stringReader;
				//Specify the local HTML4 DTD for performance reasons
				string dtdPath = Path.Combine(appRootPath, "Dtds\\html4-loose.dtd");
				//Need to convert C:\Program Files\SpiraTeam to file:///C:/Program%20Files/SpiraTeam
				sgmlReader.SystemLiteral = "file:///" + dtdPath.Replace(@"\", "/").Replace(" ", "%20");

				//Now create the new XML document from this (i.e. XHTML 1.0)
				XmlDocument xhtmlDocument = new XmlDocument();
				xhtmlDocument.PreserveWhitespace = true;
				xhtmlDocument.XmlResolver = null;
				xhtmlDocument.Load(sgmlReader);

				//For MS-Word 2003 XML format reports, need to do some pre-processing
				if (token == "MsWord2003")
				{
					Logger.LogTraceEvent("Before Processing: ", xhtmlDocument.OuterXml);
					//If this is an MS-Word format, need to do some cleanup before the main transformation
					XslCompiledTransform xslt2 = new XslCompiledTransform();
					XsltSettings xsltSettings2 = new XsltSettings();
					xsltSettings2.EnableScript = true;
					xslt2.Load(Path.Combine(appRootPath, "Transforms/XHtml_CleanUpForWord.xslt"), xsltSettings2, null);
					XmlTextWriter xmlWriter2;
					MemoryStream memoryStream2 = new MemoryStream();
					xmlWriter2 = new XmlTextWriter(memoryStream2, Encoding.UTF8);
					xslt2.Transform(new XmlNodeReader(xhtmlDocument), null, xmlWriter2);
					StreamReader streamReader2 = new StreamReader(memoryStream2);
					memoryStream2.Seek(0, SeekOrigin.Begin);
					string processedXHtml = streamReader2.ReadToEnd();
					xhtmlDocument.LoadXml(processedXHtml);
					Logger.LogTraceEvent("After Processing: ", xhtmlDocument.OuterXml);
				}

				//For MS-Excel 2003 XML format reports, need to do some pre-processing
				if (token == "MsExcel2003")
				{
					Logger.LogTraceEvent("Before Processing: ", xhtmlDocument.OuterXml);
					//If this is an MS-Word format, need to do some cleanup before the main transformation
					XslCompiledTransform xslt2 = new XslCompiledTransform();
					XsltSettings xsltSettings2 = new XsltSettings();
					xsltSettings2.EnableScript = true;
					xslt2.Load(Path.Combine(appRootPath, "Transforms/XHtml_CleanUpForExcel.xslt"), xsltSettings2, null);
					XmlTextWriter xmlWriter2;
					MemoryStream memoryStream2 = new MemoryStream();
					xmlWriter2 = new XmlTextWriter(memoryStream2, Encoding.UTF8);
					xslt2.Transform(new XmlNodeReader(xhtmlDocument), null, xmlWriter2);
					StreamReader streamReader2 = new StreamReader(memoryStream2);
					memoryStream2.Seek(0, SeekOrigin.Begin);
					string processedXHtml = streamReader2.ReadToEnd();
					xhtmlDocument.LoadXml(processedXHtml);
					Logger.LogTraceEvent("After Processing: ", xhtmlDocument.OuterXml);
				}

				//If we have any IMG tags we need to locate them and remove
				//Also add unique IDs since some formats rely on that
				XmlNodeList xmlImageNodes = xhtmlDocument.SelectNodes("//img");
				if (xmlImageNodes != null && xmlImageNodes.Count > 0)
				{
					int index = 1;
					List<XmlNode> imagesToRemove = new List<XmlNode>();
					foreach (XmlNode xmlImage in xmlImageNodes)
					{
						//By default we remove the images
						bool removeImage = true;
						//Get the SRC and see if we can embed the images - but not for Excel 2003 as this can never show images
						if (token != "MsExcel2003" && xmlImage.Attributes["src"] != null)
						{
							string href = xmlImage.Attributes["src"].Value;

							//If this is not an embedded datastream we need to actually try and get the image
							if (!String.IsNullOrWhiteSpace(href))
							{
								//See if we can actually resolve this url
								if (href.StartsWith("data:"))
								{
									removeImage = false;
								}
								else
								{
									string imageData = GetImageData(href);
									if (!String.IsNullOrEmpty(imageData))
									{
										xmlImage.Attributes["src"].Value = imageData;
										removeImage = false;
									}
								}
							}
						}

						if (removeImage)
						{
							imagesToRemove.Add(xmlImage);
						}
						else
						{
							if (xmlImage.Attributes["id"] == null)
							{
								XmlAttribute xmlAttribute = xhtmlDocument.CreateAttribute("id");
								xmlAttribute.Value = String.Format("img_{0:000000}", index);
								xmlImage.Attributes.Append(xmlAttribute);
							}
							else
							{
								xmlImage.Attributes["id"].Value = String.Format("img_{0:000000}", index);
							}
							index++;
						}
					}

					//Remove any images
					foreach (XmlNode xmlImage in imagesToRemove)
					{
						xmlImage.ParentNode.RemoveChild(xmlImage);
					}
				}

				//Now we need to transform this into the appropriate format
				string outputMarkup = "";
				XslCompiledTransform xslt = new XslCompiledTransform();
				XsltSettings xsltSettings = new XsltSettings();
				xsltSettings.EnableScript = true;
				xslt.Load(Path.Combine(appRootPath, "Transforms/XHtml_" + token + ".xslt"), xsltSettings, null);
				XmlTextWriter xmlWriter;
				MemoryStream memoryStream = new MemoryStream();
				xmlWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
				xslt.Transform(new XmlNodeReader(xhtmlDocument), null, xmlWriter);
				StreamReader streamReader = new StreamReader(memoryStream);
				memoryStream.Seek(0, SeekOrigin.Begin);
				outputMarkup = streamReader.ReadToEnd();

				//If we have just whitespace, try using the old RegEx expression as a fallback
				if (String.IsNullOrWhiteSpace(outputMarkup))
				{
					outputMarkup = Strings.StripHTML(htmlMarkup);
				}
				else
				{
					//For MS-Word format 2003 reports, need to do some post-processing
					if (token == "MsWord2003")
					{
						string dirtyWordML = "<wordml>" + outputMarkup + "</wordml>";
						Logger.LogTraceEvent("Before Processing2: ", dirtyWordML);
						//If this is an MS-Word format, need to do some cleanup after the main transformation
						XmlDocument wordMlDocument = new XmlDocument();
						wordMlDocument.PreserveWhitespace = true;
						wordMlDocument.XmlResolver = null;
						wordMlDocument.LoadXml(dirtyWordML);

						XslCompiledTransform xslt3 = new XslCompiledTransform();
						XsltSettings xsltSettings3 = new XsltSettings();
						xsltSettings3.EnableScript = true;
						xslt3.Load(Path.Combine(appRootPath, "Transforms/" + token + "_PostProcessing.xslt"), xsltSettings3, null);
						XmlTextWriter xmlWriter3;
						MemoryStream memoryStream3 = new MemoryStream();
						xmlWriter3 = new XmlTextWriter(memoryStream3, Encoding.UTF8);
						xslt3.Transform(new XmlNodeReader(wordMlDocument), null, xmlWriter3);
						StreamReader streamReader3 = new StreamReader(memoryStream3);
						memoryStream3.Seek(0, SeekOrigin.Begin);
						outputMarkup = streamReader3.ReadToEnd();
						Logger.LogTraceEvent("After Processing2: ", outputMarkup);
					}

					//For MS-Excel 2003 format reports, need to do some post-processing
					if (token == "MsExcel2003")
					{
						string dirtyWordML = "<Worksheet>" + outputMarkup + "</Worksheet>";
						Logger.LogTraceEvent("Before Processing2: ", dirtyWordML);
						//If this is an MS-Excel format, need to do some cleanup after the main transformation
						XmlDocument spreadsheetMlDocument = new XmlDocument();
						spreadsheetMlDocument.PreserveWhitespace = true;
						spreadsheetMlDocument.XmlResolver = null;
						spreadsheetMlDocument.LoadXml(dirtyWordML);

						XslCompiledTransform xslt3 = new XslCompiledTransform();
						XsltSettings xsltSettings3 = new XsltSettings();
						xsltSettings3.EnableScript = true;
						xslt3.Load(Path.Combine(appRootPath, "Transforms/" + token + "_PostProcessing.xslt"), xsltSettings3, null);
						XmlTextWriter xmlWriter3;
						MemoryStream memoryStream3 = new MemoryStream();
						xmlWriter3 = new XmlTextWriter(memoryStream3, Encoding.UTF8);
						xslt3.Transform(new XmlNodeReader(spreadsheetMlDocument), null, xmlWriter3);
						StreamReader streamReader3 = new StreamReader(memoryStream3);
						memoryStream3.Seek(0, SeekOrigin.Begin);
						outputMarkup = streamReader3.ReadToEnd();
						Logger.LogTraceEvent("After Processing2: ", outputMarkup);
					}

					//For Acrobat(PDF) format reports, need to do some post-processing
					if (token == "Pdf")
					{
						string dirtyXslFo = "<flow>" + outputMarkup + "</flow>";
						Logger.LogTraceEvent("Before Processing2: ", dirtyXslFo);
						//If this is an MS-Excel format, need to do some cleanup after the main transformation
						XmlDocument spreadsheetMlDocument = new XmlDocument();
						spreadsheetMlDocument.PreserveWhitespace = true;
						spreadsheetMlDocument.XmlResolver = null;
						spreadsheetMlDocument.LoadXml(dirtyXslFo);

						XslCompiledTransform xslt3 = new XslCompiledTransform();
						XsltSettings xsltSettings3 = new XsltSettings();
						xsltSettings3.EnableScript = true;
						xslt3.Load(Path.Combine(appRootPath, "Transforms/" + token + "_PostProcessing.xslt"), xsltSettings3, null);
						XmlTextWriter xmlWriter3;
						MemoryStream memoryStream3 = new MemoryStream();
						xmlWriter3 = new XmlTextWriter(memoryStream3, Encoding.UTF8);
						xslt3.Transform(new XmlNodeReader(spreadsheetMlDocument), null, xmlWriter3);
						StreamReader streamReader3 = new StreamReader(memoryStream3);
						memoryStream3.Seek(0, SeekOrigin.Begin);
						outputMarkup = streamReader3.ReadToEnd();
						Logger.LogTraceEvent("After Processing2: ", outputMarkup);
					}
				}
				return outputMarkup;
			}
			catch (XmlException exception)
			{
				//If we have problems parsing the XML, fallback to using the old RegEx expression to get the
				//plain-text version. We'll also log a warning
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to parse rich-text to generate report, converting to plain text instead. (" + exception.Message + ")");
				Logger.Flush();
				return Strings.StripHTML(htmlMarkup);
			}
			catch (ArgumentException exception)
			{
				//If we have problems parsing the SGML, fallback to using the old RegEx expression to get the
				//plain-text version. We'll also log a warning
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to parse rich-text to generate report, converting to plain text instead. (" + exception.Message + ")");
				Logger.Flush();
				return Strings.StripHTML(htmlMarkup);
			}
			catch (SgmlParseException exception)
			{
				//If we have problems parsing the SGML, fallback to using the old RegEx expression to get the
				//plain-text version. We'll also log a warning
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "Unable to parse rich-text to generate report, converting to plain text instead. (" + exception.Message + ")");
				Logger.Flush();
				return Strings.StripHTML(htmlMarkup);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Tries to get the image data accociated with a URL
		/// </summary>
		/// <param name="url">The URL</param>
		/// <returns>The image data</returns>
		protected string GetImageData(string url)
		{
			const string METHOD_NAME = "GetImageData";

			try
			{
				//See if we can open up the URL
				Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

				bool imageFound = false;
				byte[] imageBytes = null;
				string imageType = "";

				//See if we have a SpiraTeam internal URL to an attachment, in which case we just retrieve the item
				//directly from the Documents store
				//Try both  URLs
				Match match = absoluteAttachmentUrlRegex.Match(url);
				if (!match.Success)
				{
					match = relativeAttachmentUrlRegex.Match(url);
				}
				if (match.Success)
				{
					if (match.Groups.Count >= 2 && match.Groups[1].Success)
					{
						string matchValue = match.Groups[1].Value;
						int attachmentId;
						if (Int32.TryParse(matchValue, out attachmentId))
						{
							try
							{
								AttachmentManager attachmentManager = new AttachmentManager();
								Attachment attachment = attachmentManager.RetrieveById(attachmentId);
								string filename = attachment.Filename;
								FileStream fileStream = attachmentManager.OpenById(attachmentId);

								//have to sniff the file extension
								if (filename.ToLowerInvariant().EndsWith(".jpg") || filename.ToLowerInvariant().EndsWith(".jpeg"))
								{
									imageType = "image/jpeg";
								}
								else if (filename.ToLowerInvariant().EndsWith(".png"))
								{
									imageType = "image/png";
								}
								else if (filename.ToLowerInvariant().EndsWith(".gif"))
								{
									imageType = "image/gif";
								}
								else if (filename.ToLowerInvariant().EndsWith(".bmp"))
								{
									imageType = "image/bmp";
								}

								//Extract the data from the stream in byte form
								imageBytes = new byte[fileStream.Length];
								fileStream.Read(imageBytes, 0, (int)fileStream.Length);
								imageFound = true;
							}
							catch (Exception exception)
							{
								Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception, "Unable to open attachment with id = " + attachmentId);
								imageFound = false;
							}
						}
					}
				}

				if (!imageFound)
				{
					//Create the request
					WebRequest webRequest = WebRequest.Create(url);
					((HttpWebRequest)webRequest).UserAgent = ConfigurationSettings.Default.License_ProductType;

					//Get the response
					byte[] buffer = new byte[4096];
					using (WebResponse webResponse = webRequest.GetResponse())
					{
						//Determine the format
						imageType = webResponse.ContentType;

						using (Stream dataStream = webResponse.GetResponseStream())
						{
							//Read the content.
							using (MemoryStream tempMemStream = new MemoryStream())
							{
								int count = 0;
								do
								{
									count = dataStream.Read(buffer, 0, buffer.Length);
									tempMemStream.Write(buffer, 0, count);

								}
								while (count != 0);
								imageBytes = tempMemStream.ToArray();
							}
						}
					}
				}

				//See if the MIME type is for an image format
				if (!imageType.Contains("image"))
				{
					//have to sniff the file extension
					if (url.ToLowerInvariant().EndsWith(".jpg") || url.ToLowerInvariant().EndsWith(".jpeg"))
					{
						imageType = "image/jpeg";
					}
					else if (url.ToLowerInvariant().EndsWith(".png"))
					{
						imageType = "image/png";
					}
					else if (url.ToLowerInvariant().EndsWith(".gif"))
					{
						imageType = "image/gif";
					}
					else if (url.ToLowerInvariant().EndsWith(".bmp"))
					{
						imageType = "image/bmp";
					}
				}

				//Not able to resolve, so return an empty string
				if (!imageType.Contains("image") || imageBytes == null)
				{
					return "";
				}

				//Get as base64 string
				string base64Image = Convert.ToBase64String(imageBytes);
				string imageEmbeddedUrl = "data:" + imageType + ";base64," + base64Image;

				Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, url + " = " + imageEmbeddedUrl);
				return imageEmbeddedUrl;
			}
			catch (Exception exception)
			{
				//Fail quietly if we cannot get the image data
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
				return "";
			}
		}

		/// <summary>
		/// Gets the requirements detailed report section data XML
		/// </summary>
		/// <param name="reportElements">The list of seconary elements to render</param>
		/// <param name="reportFormatToken">The token for the report format</param>
		/// <param name="filters">Any filters to apply to the data</param>
		protected XmlDocument Get_RequirementDetails(int userId, int projectId, int projectTemplateId, string reportFormatToken, List<string> reportElements, Hashtable filters, string timezoneId, UpdateBackgroundProcessStatus updateBackgroundProcessStatus, int progress)
		{
			//Get the offset from the timezone
			double utcOffset = 0;
			if (!String.IsNullOrEmpty(timezoneId))
			{
				TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
				if (timeZone != null)
				{
					utcOffset = timeZone.GetUtcOffset(DateTime.Now).TotalHours;
				}
			}

			//Get the custom property definitions for this project
			Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, true, false, true);

			//Get the list of active data syncs for the project
			List<DataSyncSystem> dataMappingSystems = new DataMappingManager().RetrieveDataSyncSystemsForProject(projectId);

			//Get the list of requirements
			Business.RequirementManager requirementManager = new Business.RequirementManager();
			int count = requirementManager.Count(Business.UserManager.UserInternal, projectId, filters, utcOffset);
			List<RequirementView> requirements = new List<RequirementView>();
			for (int index = 1; index < count + 1; index += PAGINATION_SIZE)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

				requirements.AddRange(requirementManager.Retrieve(Business.UserManager.UserInternal, projectId, index, PAGINATION_SIZE, filters, utcOffset));
			}

			//Clean any rich-text fields in the dataset
			CleanRichTextFields(requirements, "Description");

			//Now get the requirements list XML node list
			XmlDocument xmlRequirementsDoc = new XmlDocument();
			XmlNode xmlReqData = xmlRequirementsDoc.CreateElement("RequirementData");
			xmlRequirementsDoc.AppendChild(xmlReqData);

			//Iterate through each entity in the collection and serialize
			foreach (RequirementView requirementView in requirements)
			{
				XmlNode xmlReqNode = xmlRequirementsDoc.CreateElement("Requirement");
				xmlReqNode.InnerXml = requirementView.InnerXml;
				xmlReqData.AppendChild(xmlReqNode);
			}

			//Loop through the requirements adding additional elements
			DiscussionManager discussionManager = new DiscussionManager();
			foreach (XmlNode xmlRequirementNode in xmlRequirementsDoc.ChildNodes[0].ChildNodes)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

				//Get the requirement id
				XmlNode xmlRequirementId = xmlRequirementNode.SelectSingleNode("RequirementId");
				if (xmlRequirementId != null)
				{
					int requirementId = Int32.Parse(xmlRequirementId.InnerText);

					//Now depending on the report elements the user has chosen
					//insert the appropriate secondary elements as child nodes

					//Custom Properties
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement);

					//Custom properties need to be specially parsed to add them to the XML in a way that can
					//be easily handled by XSLT.
					InsertCustomProperties(projectId, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, artifactCustomProperty, xmlRequirementsDoc, xmlRequirementNode, customProperties, timezoneId, dataMappingSystems);

					//Comments
					IEnumerable<IDiscussion> comments = discussionManager.Retrieve(requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, false);

					//Get the discussions as XML and add to the requirement XML
					InsertComments(comments, "Discussion", xmlRequirementsDoc, xmlRequirementNode, "Discussions");

					//Scenarios Steps
					List<RequirementStep> requirementSteps = requirementManager.RetrieveSteps(requirementId);

					//Get the steps as XML and add to the requirement XML
					InsertChildEntity<RequirementStep>(projectId, projectTemplateId, requirementSteps, xmlRequirementsDoc, xmlRequirementNode, "RequirementSteps", timezoneId, dataMappingSystems, "RequirementStep");

					//Test Coverage
					if (reportElements.Contains("TestCoverage"))
					{
						Business.TestCaseManager testCaseManager = new Business.TestCaseManager();
						List<TestCase> testCases = testCaseManager.RetrieveCoveredByRequirementId(projectId, requirementId);

						//Get the test cases as XML and add to the requirement XML
						XmlNode xmlTestCasesContainer = InsertChildEntity<TestCase>(projectId, projectTemplateId, testCases, xmlRequirementsDoc, xmlRequirementNode, "TestCases", timezoneId, dataMappingSystems, "TestCase");

						//Now add the test run to each of these test cases
						if (reportElements.Contains("TestRuns"))
						{
							//Loop through each test case
							foreach (XmlNode xmlTestCaseNode in xmlTestCasesContainer.ChildNodes)
							{
								XmlNode xmlTestCaseId = xmlTestCaseNode.SelectSingleNode("TestCaseId");
								if (xmlTestCaseId != null)
								{
									int testCaseId = Int32.Parse(xmlTestCaseId.InnerText);
									//Test Runs
									Business.TestRunManager testRunManager = new Business.TestRunManager();
									Hashtable filters2 = new Hashtable();
									filters2.Add("TestCaseId", testCaseId);
									//If we have a release filter set, then also apply this to the test run filters
									if (filters["ReleaseId"] != null)
									{
										filters2.Add("ReleaseId", (int)filters["ReleaseId"]);
									}
									int testRunCount = testRunManager.Count(projectId, filters2, utcOffset);
									for (int index = 1; index < testRunCount + 1; index += PAGINATION_SIZE)
									{
										List<TestRunView> testRuns = testRunManager.Retrieve(projectId, "EndDate", false, index, PAGINATION_SIZE, filters2, utcOffset);
										//Get the test runs as XML and add to the test case XML
										XmlNode xmlTestRunsContainer = InsertChildEntity<TestRunView>(projectId, projectTemplateId, testRuns, xmlRequirementsDoc, xmlTestCaseNode, "TestRuns", timezoneId, dataMappingSystems, "TestRun");

										//If we have incidents, also add these to each test run,
										//just the directly linked incidents
										if (reportElements.Contains("Incidents"))
										{
											//Loop through each test run
											foreach (XmlNode xmlTestRunNode in xmlTestRunsContainer.ChildNodes)
											{
												XmlNode xmlTestRunId = xmlTestRunNode.SelectSingleNode("TestRunId");
												if (xmlTestRunId != null)
												{
													int testRunId = Int32.Parse(xmlTestRunId.InnerText);

													//Incidents are related to the test run step, so use the special filter that aggregates to the Test Run as a whole
													IncidentManager incidentManager = new IncidentManager();
													Hashtable testRunFilters = new Hashtable();
													testRunFilters.Add("TestRunId", testRunId);
													List<IncidentView> incidents = incidentManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, testRunFilters, utcOffset);

													//Get the incidents as XML and add to the test run XML
													InsertChildEntity<IncidentView>(projectId, projectTemplateId, incidents, xmlRequirementsDoc, xmlTestRunNode, "Incidents", timezoneId, dataMappingSystems, "Incident");
												}
											}
										}
									}
								}
							}
						}
					}

					//Tasks
					if (reportElements.Contains("Tasks"))
					{
						Business.TaskManager taskManager = new Business.TaskManager();
						List<TaskView> tasks = taskManager.RetrieveByRequirementId(requirementId);

						//Get the tasks as XML and add to the requirement XML
						InsertChildEntity<TaskView>(projectId, projectTemplateId, tasks, xmlRequirementsDoc, xmlRequirementNode, "Tasks", timezoneId, dataMappingSystems, "Task");
					}

					//Attachments
					if (reportElements.Contains("Attachments"))
					{
						Business.AttachmentManager attachmentManager = new Business.AttachmentManager();
						List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, null, true, 1, Int32.MaxValue, null, 0);

						//Get the attachments as XML and add to the requirement XML
						InsertChildEntity<ProjectAttachmentView>(projectId, projectTemplateId, attachments, xmlRequirementsDoc, xmlRequirementNode, "Attachments", timezoneId, dataMappingSystems, "Attachment");

						//If the license supports source code we need to add these as well
						InsertSourceCodeAttachments(projectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, requirementId, xmlRequirementsDoc, xmlRequirementNode, "Attachments");
					}

					//History
					if (reportElements.Contains("History"))
					{
						InsertArtifactHistory(projectId, projectTemplateId, requirementId, Artifact.ArtifactTypeEnum.Requirement, utcOffset, xmlRequirementsDoc, xmlRequirementNode, timezoneId, dataMappingSystems, customProperties);
					}

					//Artifact Links (Requirements)
					if (reportElements.Contains("Requirements"))
					{
						Business.ArtifactLinkManager artifactLinkManager = new Business.ArtifactLinkManager();
						List<ArtifactLinkView> artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Requirement, requirementId);

						//Remove any non-requirement associations
						if (artifactLinks != null)
						{
							List<ArtifactLinkView> requirementArtifactLinks = artifactLinks.Where(a => a.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.Requirement).ToList();

							//Get the artifact links as XML and add to the requirement XML
							InsertChildEntity<ArtifactLinkView>(projectId, projectTemplateId, requirementArtifactLinks, xmlRequirementsDoc, xmlRequirementNode, "Requirements", timezoneId, dataMappingSystems, "ArtifactLink");
						}
					}

					//Artifact Links (Incidents)
					if (reportElements.Contains("Incidents"))
					{
						Business.ArtifactLinkManager artifactLinkManager = new Business.ArtifactLinkManager();
						List<ArtifactLinkView> artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Requirement, requirementId);

						//Remove any non-incident associations
						if (artifactLinks != null)
						{
							List<ArtifactLinkView> incidentArtifactLinks = artifactLinks.Where(a => a.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.Incident).ToList();

							//Get the artifact links as XML and add to the requirement XML
							InsertChildEntity<ArtifactLinkView>(projectId, projectTemplateId, incidentArtifactLinks, xmlRequirementsDoc, xmlRequirementNode, "Incidents", timezoneId, dataMappingSystems, "ArtifactLink");

						}
					}

					//Source Code
					if (reportElements.Contains("SourceCode"))
					{
						InsertSourceCodeRevisions(userId, projectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, requirementId, xmlRequirementsDoc, xmlRequirementNode, "SourceCodeRevisions");
					}
				}
			}

			//Localize any dates in the document
			LocalizeDates(xmlRequirementsDoc, timezoneId);

			//Return the data XML document
			return xmlRequirementsDoc;
		}

		/// <summary>
		/// Gets the requirements plan report section data XML
		/// </summary>
		/// <param name="reportElements">The list of seconary elements to render</param>
		/// <param name="reportFormatToken">The token for the report format</param>
		/// <param name="filters">Any filters to apply to the data</param>
		protected XmlDocument Get_RequirementPlan(int projectId, int projectTemplateId, string reportFormatToken, List<string> reportElements, Hashtable filters, string timezoneId, UpdateBackgroundProcessStatus updateBackgroundProcessStatus, int progress)
		{
			//Get the offset from the timezone
			double utcOffset = 0;
			if (!String.IsNullOrEmpty(timezoneId))
			{
				TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
				if (timeZone != null)
				{
					utcOffset = timeZone.GetUtcOffset(DateTime.Now).TotalHours;
				}
			}

			//Get the custom property definitions for this project
			Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, true, false, true);

			//Get the list of active data syncs for the project
			List<DataSyncSystem> dataMappingSystems = new DataMappingManager().RetrieveDataSyncSystemsForProject(projectId);

			//Get the list of requirements
			Business.RequirementManager requirementManager = new Business.RequirementManager();
			int count = requirementManager.Count(Business.UserManager.UserInternal, projectId, filters, utcOffset);
			List<RequirementView> requirements = new List<RequirementView>();
			for (int index = 1; index < count + 1; index += PAGINATION_SIZE)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

				requirements.AddRange(requirementManager.Retrieve(Business.UserManager.UserInternal, projectId, index, PAGINATION_SIZE, filters, utcOffset));
			}

			//Clean any rich-text fields in the dataset
			CleanRichTextFields(requirements, "Description");

			//Now get the requirements list XML node list
			XmlDocument xmlRequirementsDoc = new XmlDocument();
			XmlNode xmlReqData = xmlRequirementsDoc.CreateElement("RequirementData");
			xmlRequirementsDoc.AppendChild(xmlReqData);

			//Iterate through each entity in the collection and serialize
			foreach (RequirementView requirementView in requirements)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

				XmlNode xmlReqNode = xmlRequirementsDoc.CreateElement("Requirement");
				xmlReqNode.InnerXml = requirementView.InnerXml;
				xmlReqData.AppendChild(xmlReqNode);
			}

			//Loop through the requirements adding additional elements
			foreach (XmlNode xmlRequirementNode in xmlRequirementsDoc.ChildNodes[0].ChildNodes)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

				//Get the requirement id
				XmlNode xmlRequirementId = xmlRequirementNode.SelectSingleNode("RequirementId");
				if (xmlRequirementId != null)
				{
					int requirementId = Int32.Parse(xmlRequirementId.InnerText);

					//Now depending on the report elements the user has chosen
					//insert the appropriate secondary elements as child nodes

					//Custom Properties
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement);

					//Custom properties need to be specially parsed to add them to the XML in a way that can
					//be easily handled by XSLT.
					InsertCustomProperties(projectId, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, artifactCustomProperty, xmlRequirementsDoc, xmlRequirementNode, customProperties, timezoneId, dataMappingSystems);

					//Tasks
					if (reportElements.Contains("Tasks"))
					{
						Business.TaskManager taskManager = new Business.TaskManager();
						List<TaskView> tasks = taskManager.RetrieveByRequirementId(requirementId);

						//Get the tasks as XML and add to the requirement XML
						InsertChildEntity<TaskView>(projectId, projectTemplateId, tasks, xmlRequirementsDoc, xmlRequirementNode, "Tasks", timezoneId, dataMappingSystems, "Task");
					}
				}
			}

			//Finally we need to add the user list as that will also be needed in the report to show resources
			Business.UserManager userManager = new UserManager();
			List<User> users = userManager.RetrieveActiveByProjectId(projectId);
			//Need to strip out any sensitive fields from the report
			foreach (User user in users)
			{
				user.RssToken = null;
				user.Password = null;
				user.PasswordSalt = null;
				user.PasswordQuestion = null;
				user.PasswordAnswer = null;
			}
			InsertChildEntity(projectId, projectTemplateId, users, xmlRequirementsDoc, xmlRequirementsDoc.DocumentElement, "Users", timezoneId, dataMappingSystems, "User");

			//Localize any dates in the document
			LocalizeDates(xmlRequirementsDoc, timezoneId);

			//Return the data XML document
			return xmlRequirementsDoc;
		}

		/// <summary>
		/// Gets the release detailed report section data XML
		/// </summary>
		/// <param name="reportElements">The list of seconary elements to render</param>
		/// <param name="reportFormatToken">The token for the report format</param>
		/// <param name="filters">Any filters to apply to the data</param>
		protected XmlDocument Get_ReleaseDetails(int projectId, int projectTemplateId, string reportFormatToken, List<string> reportElements, Hashtable filters, string timezoneId, UpdateBackgroundProcessStatus updateBackgroundProcessStatus, int progress)
		{
			//Get the offset from the timezone
			double utcOffset = 0;
			if (!String.IsNullOrEmpty(timezoneId))
			{
				TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
				if (timeZone != null)
				{
					utcOffset = timeZone.GetUtcOffset(DateTime.Now).TotalHours;
				}
			}

			//Get the custom property definitions for this project
			Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Release, true, false, true);

			//Get the list of active data syncs for the project
			List<DataSyncSystem> dataMappingSystems = new DataMappingManager().RetrieveDataSyncSystemsForProject(projectId);

			//Get the list of releases/iterations for the project
			Business.ReleaseManager releaseManager = new Business.ReleaseManager();
			int count = releaseManager.Count(Business.UserManager.UserInternal, projectId, filters, utcOffset);
			List<ReleaseView> releases = new List<ReleaseView>();
			for (int index = 1; index < count + 1; index += PAGINATION_SIZE)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

				releases.AddRange(releaseManager.RetrieveByProjectId(Business.UserManager.UserInternal, projectId, index, PAGINATION_SIZE, filters, utcOffset));
			}

			//Clean any rich-text fields in the dataset
			CleanRichTextFields(releases, "Description");

			//Now get the releases list XML node list
			XmlDocument xmlReleasesDoc = new XmlDocument();
			XmlNode xmlReleaseData = xmlReleasesDoc.CreateElement("ReleaseData");
			xmlReleasesDoc.AppendChild(xmlReleaseData);

			//Iterate through each entity in the collection and serialize
			foreach (ReleaseView releaseView in releases)
			{
				XmlNode xmlReleaseNode = xmlReleasesDoc.CreateElement("Release");
				xmlReleaseNode.InnerXml = releaseView.InnerXml;
				xmlReleaseData.AppendChild(xmlReleaseNode);
			}

			//Loop through the releases adding additional elements
			DiscussionManager discussionManager = new DiscussionManager();
			foreach (XmlNode xmlReleaseNode in xmlReleasesDoc.ChildNodes[0].ChildNodes)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

				//Get the release id
				XmlNode xmlReleaseId = xmlReleaseNode.SelectSingleNode("ReleaseId");
				if (xmlReleaseId != null)
				{
					int releaseId = Int32.Parse(xmlReleaseId.InnerText);

					//Now depending on the report elements the user has chosen
					//insert the appropriate secondary elements as child nodes

					//Custom Properties
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, releaseId, DataModel.Artifact.ArtifactTypeEnum.Release);

					//Custom properties need to be specially parsed to add them to the XML in a way that can
					//be easily handled by XSLT.
					InsertCustomProperties(projectId, releaseId, DataModel.Artifact.ArtifactTypeEnum.Release, artifactCustomProperty, xmlReleasesDoc, xmlReleaseNode, customProperties, timezoneId, dataMappingSystems);

					//Comments
					IEnumerable<IDiscussion> comments = discussionManager.Retrieve(releaseId, DataModel.Artifact.ArtifactTypeEnum.Release, false);

					//Get the discussions as XML and add to the release XML
					InsertComments(comments, "Discussion", xmlReleasesDoc, xmlReleaseNode, "Discussions");

					//Test Coverage
					if (reportElements.Contains("TestCoverage"))
					{
						Business.TestCaseManager testCaseManager = new Business.TestCaseManager();
						List<TestCaseReleaseView> testCases = testCaseManager.RetrieveByReleaseId(projectId, releaseId, "Name", true, 1, Int32.MaxValue, null, utcOffset, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);

						//Get the test cases as XML and add to the release XML
						InsertChildEntity<TestCaseReleaseView>(projectId, projectTemplateId, testCases, xmlReleasesDoc, xmlReleaseNode, "TestCases", timezoneId, dataMappingSystems, "TestCase");
					}

					//Requirements
					if (reportElements.Contains("Requirements"))
					{
						Hashtable releaseFilter = new Hashtable();
						releaseFilter.Add("ReleaseId", releaseId);
						RequirementManager requirementManager = new RequirementManager();
						List<RequirementView> requirements = requirementManager.Retrieve(Business.UserManager.UserInternal, projectId, 1, Int32.MaxValue, releaseFilter, utcOffset);

						//Get the requirements as XML and add to the release XML
						InsertChildEntity(projectId, projectTemplateId, requirements, xmlReleasesDoc, xmlReleaseNode, "Requirements", timezoneId, dataMappingSystems, "Requirement");
					}

					//Tasks
					if (reportElements.Contains("Tasks"))
					{
						Business.TaskManager task = new Business.TaskManager();
						List<TaskView> tasks = task.RetrieveByReleaseId(projectId, releaseId);

						//Get the tasks as XML and add to the release XML
						InsertChildEntity<TaskView>(projectId, projectTemplateId, tasks, xmlReleasesDoc, xmlReleaseNode, "Tasks", timezoneId, dataMappingSystems, "Task");
					}

					//Test Runs
					if (reportElements.Contains("TestRuns"))
					{
						Business.TestRunManager testRunManager = new Business.TestRunManager();
						Hashtable filters2 = new Hashtable();
						filters2.Add("ReleaseId", releaseId);
						List<TestRunView> testRuns = testRunManager.Retrieve(projectId, "EndDate", false, 1, Int32.MaxValue, filters2, 0);

						//Get the test runs as XML and add to the release XML
						InsertChildEntity<TestRunView>(projectId, projectTemplateId, testRuns, xmlReleasesDoc, xmlReleaseNode, "TestRuns", timezoneId, dataMappingSystems, "TestRun");
					}

					//Incidents
					if (reportElements.Contains("Incidents"))
					{
						IncidentManager incidentManager = new IncidentManager();

						//Get the list of Detected Incidents and add to the Release XML
						Hashtable detectedReleaseFilter = new Hashtable();
						detectedReleaseFilter.Add("DetectedReleaseId", releaseId);
						List<IncidentView> detectedIncidents = incidentManager.Retrieve(projectId, "PriorityName", true, 1, Int32.MaxValue, detectedReleaseFilter, utcOffset);
						InsertChildEntity<IncidentView>(projectId, projectTemplateId, detectedIncidents, xmlReleasesDoc, xmlReleaseNode, "DetectedIncidents", timezoneId, dataMappingSystems, "Incident");

						//Get the list of Resolved Incidents and add to the Release XML
						Hashtable resolvedReleaseFilter = new Hashtable();
						resolvedReleaseFilter.Add("ResolvedReleaseId", releaseId);
						List<IncidentView> resolvedIncidents = incidentManager.Retrieve(projectId, "PriorityName", true, 1, Int32.MaxValue, resolvedReleaseFilter, utcOffset);
						InsertChildEntity<IncidentView>(projectId, projectTemplateId, resolvedIncidents, xmlReleasesDoc, xmlReleaseNode, "ResolvedIncidents", timezoneId, dataMappingSystems, "Incident");

						//Get the list of Verified Incidents and add to the Release XML
						Hashtable verifiedReleaseFilter = new Hashtable();
						verifiedReleaseFilter.Add("VerifiedReleaseId", releaseId);
						List<IncidentView> verifiedIncidents = incidentManager.Retrieve(projectId, "PriorityName", true, 1, Int32.MaxValue, verifiedReleaseFilter, utcOffset);
						InsertChildEntity<IncidentView>(projectId, projectTemplateId, verifiedIncidents, xmlReleasesDoc, xmlReleaseNode, "VerifiedIncidents", timezoneId, dataMappingSystems, "Incident");
					}

					//Builds
					if (reportElements.Contains("Builds"))
					{
						InsertBuilds(projectId, releaseId, xmlReleasesDoc, xmlReleaseNode, "Builds", utcOffset);
					}

					//Attachments
					if (reportElements.Contains("Attachments"))
					{
						Business.AttachmentManager attachmentManager = new Business.AttachmentManager();
						List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, releaseId, DataModel.Artifact.ArtifactTypeEnum.Release, null, true, 1, Int32.MaxValue, null, 0);

						//Get the attachments as XML and add to the release XML
						InsertChildEntity<ProjectAttachmentView>(projectId, projectTemplateId, attachments, xmlReleasesDoc, xmlReleaseNode, "Attachments", timezoneId, dataMappingSystems, "Attachment");

						//If the license supports source code we need to add these as well
						InsertSourceCodeAttachments(projectId, DataModel.Artifact.ArtifactTypeEnum.Release, releaseId, xmlReleasesDoc, xmlReleaseNode, "Attachments");
					}

					//History
					if (reportElements.Contains("History"))
					{
						InsertArtifactHistory(projectId, projectTemplateId, releaseId, Artifact.ArtifactTypeEnum.Release, utcOffset, xmlReleasesDoc, xmlReleaseNode, timezoneId, dataMappingSystems, customProperties);
					}
				}
			}

			//Localize any dates in the document
			LocalizeDates(xmlReleasesDoc, timezoneId);

			return xmlReleasesDoc;
		}

		/// <summary>
		/// Gets the release plan report section data XML
		/// </summary>
		/// <param name="reportElements">The list of seconary elements to render</param>
		/// <param name="reportFormatToken">The token for the report format</param>
		/// <param name="filters">Any filters to apply to the data</param>
		protected XmlDocument Get_ReleasePlan(int projectId, int projectTemplateId, string reportFormatToken, List<string> reportElements, Hashtable filters, string timezoneId, UpdateBackgroundProcessStatus updateBackgroundProcessStatus, int progress)
		{
			//Get the offset from the timezone
			double utcOffset = 0;
			if (!String.IsNullOrEmpty(timezoneId))
			{
				TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
				if (timeZone != null)
				{
					utcOffset = timeZone.GetUtcOffset(DateTime.Now).TotalHours;
				}
			}

			//Get the custom property definitions for this project
			Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Release, true, false, true);

			//Get the list of active data syncs for the project
			List<DataSyncSystem> dataMappingSystems = new DataMappingManager().RetrieveDataSyncSystemsForProject(projectId);

			//Get the list of releases/iterations for the project
			Business.ReleaseManager releaseManager = new Business.ReleaseManager();
			int count = releaseManager.Count(Business.UserManager.UserInternal, projectId, filters, utcOffset);
			List<ReleaseView> releases = new List<ReleaseView>();
			for (int index = 1; index < count + 1; index += PAGINATION_SIZE)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

				releases.AddRange(releaseManager.RetrieveByProjectId(Business.UserManager.UserInternal, projectId, index, PAGINATION_SIZE, filters, utcOffset));
			}

			//Clean any rich-text fields in the dataset
			CleanRichTextFields(releases, "Description");

			//Now get the releases list XML node list
			XmlDocument xmlReleasesDoc = new XmlDocument();
			XmlNode xmlReleaseData = xmlReleasesDoc.CreateElement("ReleaseData");
			xmlReleasesDoc.AppendChild(xmlReleaseData);

			//Iterate through each entity in the collection and serialize
			foreach (ReleaseView releaseView in releases)
			{
				XmlNode xmlReleaseNode = xmlReleasesDoc.CreateElement("Release");
				xmlReleaseNode.InnerXml = releaseView.InnerXml;
				xmlReleaseData.AppendChild(xmlReleaseNode);
			}

			//Loop through the releases adding additional elements
			foreach (XmlNode xmlReleaseNode in xmlReleasesDoc.ChildNodes[0].ChildNodes)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

				//Get the release id
				XmlNode xmlReleaseId = xmlReleaseNode.SelectSingleNode("ReleaseId");
				if (xmlReleaseId != null)
				{
					int releaseId = Int32.Parse(xmlReleaseId.InnerText);

					//Now depending on the report elements the user has chosen
					//insert the appropriate secondary elements as child nodes

					//Custom Properties
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, releaseId, DataModel.Artifact.ArtifactTypeEnum.Release);

					//Custom properties need to be specially parsed to add them to the XML in a way that can
					//be easily handled by XSLT.
					InsertCustomProperties(projectId, releaseId, DataModel.Artifact.ArtifactTypeEnum.Release, artifactCustomProperty, xmlReleasesDoc, xmlReleaseNode, customProperties, timezoneId, dataMappingSystems);

					//Test Coverage
					if (reportElements.Contains("TestCoverage"))
					{
						Business.TestCaseManager testCaseManager = new Business.TestCaseManager();
						List<TestCaseReleaseView> releaseTestCases = testCaseManager.RetrieveMappedByReleaseId2(projectId, releaseId);

						//Get the test cases as XML and add to the release XML
						InsertChildEntity<TestCaseReleaseView>(projectId, projectTemplateId, releaseTestCases, xmlReleasesDoc, xmlReleaseNode, "TestCases", timezoneId, dataMappingSystems, "TestCase");
					}

					//Requirements
					if (reportElements.Contains("Requirements"))
					{
						Hashtable releaseFilter = new Hashtable();
						releaseFilter.Add("ReleaseId", releaseId);
						RequirementManager requirementManager = new RequirementManager();
						List<RequirementView> requirements = requirementManager.Retrieve(Business.UserManager.UserInternal, projectId, 1, Int32.MaxValue, releaseFilter, utcOffset);

						//Need to remove any summary requirements for this report
						List<RequirementView> nonSummaryRequirements = requirements.Where(r => !r.IsSummary).ToList();

						//Get the requirements as XML and add to the release XML
						InsertChildEntity(projectId, projectTemplateId, nonSummaryRequirements, xmlReleasesDoc, xmlReleaseNode, "Requirements", timezoneId, dataMappingSystems, "Requirement");
					}

					//Tasks
					if (reportElements.Contains("Tasks"))
					{
						Business.TaskManager task = new Business.TaskManager();
						List<TaskView> tasks = task.RetrieveByReleaseId(projectId, releaseId);

						//Get the tasks as XML and add to the release XML
						InsertChildEntity<TaskView>(projectId, projectTemplateId, tasks, xmlReleasesDoc, xmlReleaseNode, "Tasks", timezoneId, dataMappingSystems, "Task");
					}

					//Resolved Incidents
					if (reportElements.Contains("Incidents"))
					{
						IncidentManager incidentManager = new IncidentManager();
						List<IncidentView> incidents = incidentManager.RetrieveByReleaseId(projectId, releaseId);

						//Get the incidents as XML and add to the release XML
						InsertChildEntity<IncidentView>(projectId, projectTemplateId, incidents, xmlReleasesDoc, xmlReleaseNode, "Incidents", timezoneId, dataMappingSystems, "Incident");
					}
				}
			}

			//Finally we need to add the user list as that will also be needed in the report to show resources
			Business.UserManager userManager = new UserManager();
			List<DataModel.User> users = userManager.RetrieveActiveByProjectId(projectId);
			//Need to strip out any sensitive fields from the report
			foreach (User user in users)
			{
				user.RssToken = null;
				user.Password = null;
				user.PasswordSalt = null;
				user.PasswordQuestion = null;
				user.PasswordAnswer = null;
			}
			InsertChildEntity(projectId, projectTemplateId, users, xmlReleasesDoc, xmlReleasesDoc.DocumentElement, "Users", timezoneId, dataMappingSystems, "User");

			//Localize any dates in the document
			LocalizeDates(xmlReleasesDoc, timezoneId);

			//Return the data XML document
			return xmlReleasesDoc;
		}

		/// <summary>
		/// Gets the test set detailed report section data XML
		/// </summary>
		/// <param name="reportElements">The list of seconary elements to render</param>
		/// <param name="reportFormatToken">The token for the report format</param>
		/// <param name="sorts">Any sorts to apply to the data</param>
		/// <param name="folderId">Any folder to filter on (null = no filter, 0 = root)</param>
		/// <param name="filters">Any filters to apply to the data</param>
		protected XmlDocument Get_TestSetDetails(int projectId, int projectTemplateId, string reportFormatToken, List<string> reportElements, int? folderId, Hashtable filters, Hashtable sorts, string timezoneId, UpdateBackgroundProcessStatus updateBackgroundProcessStatus, int progress)
		{
			//Get the offset from the timezone
			double utcOffset = 0;
			if (!String.IsNullOrEmpty(timezoneId))
			{
				TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
				if (timeZone != null)
				{
					utcOffset = timeZone.GetUtcOffset(DateTime.Now).TotalHours;
				}
			}

			//If we have TestCaseReleaseId as a report filter, need to also filter the execution data by that
			int? releaseId = null;
			if (filters.ContainsKey("TestCaseReleaseId"))
			{
				releaseId = (int)filters["TestCaseReleaseId"];
				//The field doesn't exist on the test set, so need to actually remove as well
				filters.Remove("TestCaseReleaseId");
			}

			//Extract the sort field and direction (we only support one sort at present)
			string sortField = "Name";
			bool sortAscending = true;
			if (sorts != null)
			{
				foreach (DictionaryEntry sortEntry in sorts)
				{
					sortField = (string)sortEntry.Key;
					sortAscending = ((string)sortEntry.Value == "asc");
					break;
				}
			}

			//Get the custom property definitions for this project
			Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestSet, true, false, true);

			//Get the list of active data syncs for the project
			List<DataSyncSystem> dataMappingSystems = new DataMappingManager().RetrieveDataSyncSystemsForProject(projectId);

			//Get the list of test sets for the project
			Business.TestSetManager testSetManager = new Business.TestSetManager();
			Business.TestCaseManager testCaseManager = new Business.TestCaseManager();

			//Now get the test set list XML node list
			XmlDocument xmlTestSetDoc = new XmlDocument();
			XmlNode xmlTestSetData = xmlTestSetDoc.CreateElement("TestSetData");
			xmlTestSetDoc.AppendChild(xmlTestSetData);

			//First we need to get the folder hierarchy for either the folder or all folders
			//root gets only root test cases, all other folders are recursive
			List<TestSetFolderHierarchyView> testSetFolders = testSetManager.TestSetFolder_GetList(projectId);
			if (folderId.HasValue)
			{
				if (folderId == FOLDER_ID_ROOT)
				{
					testSetFolders = testSetFolders.Where(t => !t.ParentTestSetFolderId.HasValue).ToList();
				}
				else
				{
					TestSetFolderHierarchyView matchedFolder = testSetFolders.FirstOrDefault(t => t.TestSetFolderId == folderId.Value);
					if (matchedFolder != null)
					{
						testSetFolders = testSetFolders.Where(t => HierarchicalList.IsSelfOrChildOf(t.IndentLevel, matchedFolder.IndentLevel)).ToList();
					}
				}
			}

			//First add the root folder, unless filtering by folder (other than root)
			if (!folderId.HasValue || folderId == FOLDER_ID_ROOT)
			{
				TestSetFolder testSetFolder = new TestSetFolder();
				testSetFolder.Name = GlobalResources.General.Global_Root;
				testSetFolder.TestSetFolderId = 0;

				XmlNode xmlTestSetFolderNode = xmlTestSetDoc.CreateElement("TestSetFolder");
				xmlTestSetFolderNode.InnerXml = testSetFolder.InnerXml;
				xmlTestSetData.AppendChild(xmlTestSetFolderNode);
				AddIndentLevel(xmlTestSetFolderNode, "");

				XmlNode xmlTestSetsCollectionNode = xmlTestSetDoc.CreateElement("TestSets");
				xmlTestSetFolderNode.AppendChild(xmlTestSetsCollectionNode);

				//See if we have a release specified
				if (releaseId.HasValue)
				{
					//Count the test sets in this folder
					int count = testSetManager.CountByRelease(projectId, releaseId.Value, filters, utcOffset, null);
					List<TestSetReleaseView> testSets = new List<TestSetReleaseView>();
					for (int index = 1; index < count + 1; index += PAGINATION_SIZE)
					{
						if (updateBackgroundProcessStatus != null)
						{
							updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
						}

						testSets.AddRange(testSetManager.RetrieveByReleaseId(projectId, releaseId.Value, sortField, sortAscending, index, PAGINATION_SIZE, filters, utcOffset, null, false, false));
					}

					//Clean any rich-text fields in the dataset
					CleanRichTextFields(testSets, "Description");

					//Iterate through each entity in the collection and serialize
					foreach (TestSetReleaseView testSetView in testSets)
					{
						XmlNode xmlTestSetNode = xmlTestSetDoc.CreateElement("TestSet");
						xmlTestSetNode.InnerXml = testSetView.InnerXml;
						xmlTestSetsCollectionNode.AppendChild(xmlTestSetNode);
					}
				}
				else
				{
					//Count the test sets in this folder
					int count = testSetManager.Count(projectId, filters, utcOffset, null);
					List<TestSetView> testSets = new List<TestSetView>();
					for (int index = 1; index < count + 1; index += PAGINATION_SIZE)
					{
						if (updateBackgroundProcessStatus != null)
						{
							updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
						}

						testSets.AddRange(testSetManager.Retrieve(projectId, sortField, sortAscending, index, PAGINATION_SIZE, filters, utcOffset, null, false, false));
					}

					//Clean any rich-text fields in the dataset
					CleanRichTextFields(testSets, "Description");

					//Iterate through each entity in the collection and serialize
					foreach (TestSetView testSetView in testSets)
					{
						XmlNode xmlTestSetNode = xmlTestSetDoc.CreateElement("TestSet");
						xmlTestSetNode.InnerXml = testSetView.InnerXml;
						xmlTestSetsCollectionNode.AppendChild(xmlTestSetNode);
					}
				}
			}

			//Now loop through each folder
			foreach (TestSetFolderHierarchyView testSetFolderHierarchyView in testSetFolders)
			{
				//Retrieve the actual folder again to get all its properties
				TestSetFolder testSetFolder = testSetManager.TestSetFolder_GetById(testSetFolderHierarchyView.TestSetFolderId);
				if (testSetFolder != null)
				{
					//See if we have a release specified
					if (releaseId.HasValue)
					{
						//Count the test sets in this folder, don't display the folder if empty
						int count = testSetManager.CountByRelease(projectId, releaseId.Value, filters, utcOffset, testSetFolder.TestSetFolderId);

						//Don't include the folder if empty, unless we have no filter or it has some descendents
						XmlNode xmlTestSetFolderNode = null;
						if (count > 0 || testSetFolder.TotalCount > 0 || filters == null || filters.Count == 0)
						{
							//Clean the rich text
							testSetFolder.Description = CleanRichText(testSetFolder.Description);

							xmlTestSetFolderNode = xmlTestSetDoc.CreateElement("TestSetFolder");
							xmlTestSetFolderNode.InnerXml = testSetFolder.InnerXml;
							xmlTestSetData.AppendChild(xmlTestSetFolderNode);
							AddIndentLevel(xmlTestSetFolderNode, testSetFolderHierarchyView.IndentLevel);
						}
						if (count > 0)
						{
							XmlNode xmlTestSetsCollectionNode = xmlTestSetDoc.CreateElement("TestSets");
							xmlTestSetFolderNode.AppendChild(xmlTestSetsCollectionNode);

							List<TestSetReleaseView> testSets = new List<TestSetReleaseView>();
							for (int index = 1; index < count + 1; index += PAGINATION_SIZE)
							{
								if (updateBackgroundProcessStatus != null)
								{
									updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
								}

								testSets.AddRange(testSetManager.RetrieveByReleaseId(projectId, releaseId.Value, sortField, sortAscending, index, PAGINATION_SIZE, filters, utcOffset, testSetFolder.TestSetFolderId));
							}

							//Clean any rich-text fields in the dataset
							CleanRichTextFields(testSets, "Description");

							//Iterate through each entity in the collection and serialize
							foreach (TestSetReleaseView testSetView in testSets)
							{
								XmlNode xmlTestSetNode = xmlTestSetDoc.CreateElement("TestSet");
								xmlTestSetNode.InnerXml = testSetView.InnerXml;
								xmlTestSetsCollectionNode.AppendChild(xmlTestSetNode);
							}
						}
					}
					else
					{
						//Count the test sets in this folder, don't display the folder if empty
						int count = testSetManager.Count(projectId, filters, utcOffset, testSetFolder.TestSetFolderId);

						//Don't include the folder if empty, unless we have no filter or it has some descendents
						XmlNode xmlTestSetFolderNode = null;
						if (count > 0 || testSetFolder.TotalCount > 0 || filters == null || filters.Count == 0)
						{
							//Clean the rich text
							testSetFolder.Description = CleanRichText(testSetFolder.Description);

							xmlTestSetFolderNode = xmlTestSetDoc.CreateElement("TestSetFolder");
							xmlTestSetFolderNode.InnerXml = testSetFolder.InnerXml;
							xmlTestSetData.AppendChild(xmlTestSetFolderNode);
							AddIndentLevel(xmlTestSetFolderNode, testSetFolderHierarchyView.IndentLevel);
						}
						if (count > 0)
						{
							XmlNode xmlTestSetsCollectionNode = xmlTestSetDoc.CreateElement("TestSets");
							xmlTestSetFolderNode.AppendChild(xmlTestSetsCollectionNode);

							List<TestSetView> testSets = new List<TestSetView>();
							for (int index = 1; index < count + 1; index += PAGINATION_SIZE)
							{
								if (updateBackgroundProcessStatus != null)
								{
									updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
								}

								testSets.AddRange(testSetManager.Retrieve(projectId, sortField, sortAscending, index, PAGINATION_SIZE, filters, utcOffset, testSetFolder.TestSetFolderId));
							}

							//Clean any rich-text fields in the dataset
							CleanRichTextFields(testSets, "Description");

							//Iterate through each entity in the collection and serialize
							foreach (TestSetView testSetView in testSets)
							{
								XmlNode xmlTestSetNode = xmlTestSetDoc.CreateElement("TestSet");
								xmlTestSetNode.InnerXml = testSetView.InnerXml;
								xmlTestSetsCollectionNode.AppendChild(xmlTestSetNode);
							}
						}
					}
				}
			}

			//Loop through the test sets adding additional elements
			DiscussionManager discussionManager = new DiscussionManager();
			foreach (XmlNode xmlTestSetNode in xmlTestSetDoc.SelectNodes("/TestSetData//TestSet[TestSetId]"))
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

				//Get the test set id
				XmlNode xmlTestSetId = xmlTestSetNode.SelectSingleNode("TestSetId");
				if (xmlTestSetId != null)
				{
					int testSetId = Int32.Parse(xmlTestSetId.InnerText);

					//Now depending on the report elements the user has chosen
					//insert the appropriate secondary elements as child nodes

					//Custom Properties
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet);

					//Custom properties need to be specially parsed to add them to the XML in a way that can
					//be easily handled by XSLT.
					InsertCustomProperties(projectId, testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet, artifactCustomProperty, xmlTestSetDoc, xmlTestSetNode, customProperties, timezoneId, dataMappingSystems);

					//Comments
					IEnumerable<IDiscussion> comments = discussionManager.Retrieve(testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet, false);

					//Get the discussions as XML and add to the test set XML
					InsertComments(comments, "Discussion", xmlTestSetDoc, xmlTestSetNode, "Discussions");

					//Test Set Parameter Values
					List<TestSetParameter> testSetParameters = testSetManager.RetrieveParameterValues(testSetId);
					if (testSetParameters.Count > 0)
					{
						//Get the parameters as XML and add to the test case XML
						InsertChildEntity<TestSetParameter>(projectId, projectTemplateId, testSetParameters, xmlTestSetDoc, xmlTestSetNode, "Parameters", timezoneId, dataMappingSystems, "TestSetParameter");
					}

					//Test Cases
					if (reportElements.Contains("TestCoverage"))
					{
						//Need to filter the test cases by release as well
						List<TestSetTestCaseView> testSetTestCases = testSetManager.RetrieveTestCases3(projectId, testSetId, releaseId);

						//Get the test cases as XML and add to the test set XML
						XmlNode xmlTestCasesContainer = InsertChildEntity<TestSetTestCaseView>(projectId, projectTemplateId, testSetTestCases, xmlTestSetDoc, xmlTestSetNode, "TestCases", timezoneId, dataMappingSystems, "TestCase");

						//Iterate through the test cases and see if we have any parameter values set
						//See if this test case has any parameter values set
						XmlNodeList xmlTestSetCaseNodes = xmlTestSetNode.SelectNodes("TestCases/TestCase");
						foreach (XmlNode xmlTestSetCaseNode in xmlTestSetCaseNodes)
						{
							int testSetTestCaseId;
							if (Int32.TryParse(xmlTestSetCaseNode.SelectSingleNode("TestSetTestCaseId").InnerText, out testSetTestCaseId))
							{
								List<TestSetTestCaseParameter> testSetCaseParameters = testSetManager.RetrieveTestCaseParameterValues(testSetTestCaseId);
								if (testSetCaseParameters.Count > 0)
								{
									//Get the parameters as XML and add to the test case XML
									InsertChildEntity<TestSetTestCaseParameter>(projectId, projectTemplateId, testSetCaseParameters, xmlTestSetDoc, xmlTestSetCaseNode, "Parameters", timezoneId, dataMappingSystems, "TestSetTestCaseParameter");
								}
							}
						}

						//Now add the test steps to each of these test cases
						if (reportElements.Contains("TestSteps"))
						{
							//Loop through each test case
							foreach (XmlNode xmlTestCaseNode in xmlTestCasesContainer.ChildNodes)
							{
								XmlNode xmlTestCaseId = xmlTestCaseNode.SelectSingleNode("TestCaseId");
								if (xmlTestCaseId != null)
								{
									int testCaseId = Int32.Parse(xmlTestCaseId.InnerText);
									//Test Steps
									List<TestStepView> testSteps = testCaseManager.RetrieveStepsForTestCase(testCaseId);

									//Get the test steps as XML and add to the test case XML
									InsertChildEntity<TestStepView>(projectId, projectTemplateId, testSteps, xmlTestSetDoc, xmlTestCaseNode, "TestSteps", timezoneId, dataMappingSystems, "TestStep");

									//Iterate through the test steps and see if we have any parameter values set
									//See if this test step has any parameter values set
									XmlNodeList xmlTestStepNodes = xmlTestCaseNode.SelectNodes("TestSteps/TestStep");
									foreach (XmlNode xmlTestStepNode in xmlTestStepNodes)
									{
										int testStepId;
										if (Int32.TryParse(xmlTestStepNode.SelectSingleNode("TestStepId").InnerText, out testStepId))
										{
											List<TestStepParameter> testStepParameters = testCaseManager.RetrieveParameterValues(testStepId);
											if (testStepParameters.Count > 0)
											{
												//Get the parameters as XML and add to the test case XML
												InsertChildEntity<TestStepParameter>(projectId, projectTemplateId, testStepParameters, xmlTestSetDoc, xmlTestStepNode, "Parameters", timezoneId, dataMappingSystems, "TestStepParameter");
											}
										}
									}
								}
							}
						}
					}

					//Test Runs
					if (reportElements.Contains("TestRuns"))
					{
						Business.TestRunManager testRunManager = new Business.TestRunManager();
						Hashtable filters2 = new Hashtable();
						filters2.Add("TestSetId", testSetId);
						List<TestRunView> testRuns = testRunManager.Retrieve(projectId, "EndDate", false, 1, Int32.MaxValue, filters2, 0);

						//Get the test runs as XML and add to the test set XML
						XmlNode xmlTestRunsContainer = InsertChildEntity<TestRunView>(projectId, projectTemplateId, testRuns, xmlTestSetDoc, xmlTestSetNode, "TestRuns", timezoneId, dataMappingSystems, "TestRun");

						//Now add the test run steps to each of these test runs
						if (reportElements.Contains("TestSteps"))
						{
							//Loop through each test run
							foreach (XmlNode xmlTestRunNode in xmlTestRunsContainer.ChildNodes)
							{
								XmlNode xmlTestRunId = xmlTestRunNode.SelectSingleNode("TestRunId");
								if (xmlTestRunId != null)
								{
									int testRunId = Int32.Parse(xmlTestRunId.InnerText);
									//Test Run Steps
									List<TestRunStepView> testRunSteps = testRunManager.TestRunStep_RetrieveForTestRun(testRunId);

									//Get the test run steps as XML and add to the test run XML
									InsertChildEntity<TestRunStepView>(projectId, projectTemplateId, testRunSteps, xmlTestSetDoc, xmlTestRunNode, "TestRunSteps", timezoneId, dataMappingSystems, "TestRunStep");
								}
							}
						}
					}

					//Attachments
					if (reportElements.Contains("Attachments"))
					{
						Business.AttachmentManager attachmentManager = new Business.AttachmentManager();
						List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, testSetId, DataModel.Artifact.ArtifactTypeEnum.TestSet, null, true, 1, Int32.MaxValue, null, 0);

						//Get the attachments as XML and add to the test set XML
						InsertChildEntity<ProjectAttachmentView>(projectId, projectTemplateId, attachments, xmlTestSetDoc, xmlTestSetNode, "Attachments", timezoneId, dataMappingSystems, "Attachment");

						//If the license supports source code we need to add these as well
						InsertSourceCodeAttachments(projectId, DataModel.Artifact.ArtifactTypeEnum.TestSet, testSetId, xmlTestSetDoc, xmlTestSetNode, "Attachments");
					}

					//History
					if (reportElements.Contains("History"))
					{
						InsertArtifactHistory(projectId, projectTemplateId, testSetId, Artifact.ArtifactTypeEnum.TestSet, utcOffset, xmlTestSetDoc, xmlTestSetNode, timezoneId, dataMappingSystems, customProperties);
					}
				}
			}

			//Localize any dates in the document
			LocalizeDates(xmlTestSetDoc, timezoneId);

			//Return the data XML document
			return xmlTestSetDoc;
		}

		/// <summary>
		/// Loads and displays the test script reports
		/// </summary>
		/// <param name="reportElements">The list of seconary elements to render</param>
		/// <param name="reportFormatToken">The token for the report format</param>
		/// <param name="filters">Any filters to apply to the data</param>
		/// <param name="sorts">Any sorts to apply to the data</param>
		/// <param name="folderId">Any folder to filter on (null = no filter, 0 = root)</param>
		/// <remarks>
		/// This is distinct from the test case reports because to be able to display a printable test script
		/// we actually need to create it on the fly from the selected test cases so that all the linked test
		/// steps get consolidated into the master list correctly
		/// </remarks>
		protected XmlDocument Get_TestScripts(int projectId, int projectTemplateId, string reportFormatToken, List<string> reportElements, int? folderId, Hashtable filters, Hashtable sorts, string timezoneId, UpdateBackgroundProcessStatus updateBackgroundProcessStatus, int progress)
		{
			//Get the offset from the timezone
			double utcOffset = 0;
			if (!String.IsNullOrEmpty(timezoneId))
			{
				TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
				if (timeZone != null)
				{
					utcOffset = timeZone.GetUtcOffset(DateTime.Now).TotalHours;
				}
			}

			//Extract the sort field and direction (we only support one sort at present)
			string sortField = "Name";
			bool sortAscending = true;
			if (sorts != null)
			{
				foreach (DictionaryEntry sortEntry in sorts)
				{
					sortField = (string)sortEntry.Key;
					sortAscending = ((string)sortEntry.Value == "asc");
					break;
				}
			}

			//Convert the folder id to the filter expected by the retrieve functions
			int? folderIdToFilterWith = TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES;
			if (folderId.HasValue)
			{
				//See if we have root
				if (folderId.Value > 0)
				{
					folderIdToFilterWith = folderId.Value;
				}
				else
				{
					folderIdToFilterWith = null;
				}
			}


			//Get the custom property definitions for this project
			Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
			List<CustomProperty> testCaseCustomProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, true, false, true);
			List<CustomProperty> testStepCustomProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestStep, true, false, true);

			//Get the list of active data syncs for the project
			List<DataSyncSystem> dataMappingSystems = new DataMappingManager().RetrieveDataSyncSystemsForProject(projectId);

			//Get the list of test cases. Have to handle release filter case separately
			//since this needs to use a separate function
			Business.TestCaseManager testCaseManager = new Business.TestCaseManager();
			List<int> testCaseExecutionList = new List<int>();
			if (filters["ReleaseId"] == null)
			{
				List<TestCaseView> testCases = new List<TestCaseView>();
				int count = testCaseManager.Count(projectId, filters, utcOffset, folderIdToFilterWith);
				for (int index = 1; index < count + 1; index += PAGINATION_SIZE)
				{
					if (updateBackgroundProcessStatus != null)
					{
						updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
					}

					testCases.AddRange(testCaseManager.Retrieve(projectId, sortField, sortAscending, index, PAGINATION_SIZE, filters, utcOffset, folderIdToFilterWith));
				}

				//Now we need to create the test run dataset from this test case dataset
				//First extract the list of test cases from this dataset
				foreach (TestCaseView testCaseRow in testCases)
				{
					testCaseExecutionList.Add(testCaseRow.TestCaseId);
				}
			}
			else
			{
				List<TestCaseReleaseView> testCases = new List<TestCaseReleaseView>();
				int releaseId = (int)filters["ReleaseId"];
				//Now we need to remove from the filters collection as the column doesn't exist in the TestCase table
				filters.Remove("ReleaseId");
				int count = testCaseManager.CountByRelease(projectId, releaseId, filters, utcOffset, folderIdToFilterWith);
				for (int index = 1; index < count + 1; index += PAGINATION_SIZE)
				{
					if (updateBackgroundProcessStatus != null)
					{
						updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
					}

					testCases.AddRange(testCaseManager.RetrieveByReleaseId(projectId, releaseId, sortField, sortAscending, index, PAGINATION_SIZE, filters, utcOffset, folderIdToFilterWith));
				}

				//Now we need to create the test run dataset from this test case dataset
				//First extract the list of test cases from this dataset
				foreach (TestCaseReleaseView testCaseRow in testCases)
				{
					testCaseExecutionList.Add(testCaseRow.TestCaseId);
				}
			}

			Business.TestRunManager testRunManager = new Business.TestRunManager();
			TestRunsPending testRunPendingSet;
			try
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

				testRunPendingSet = testRunManager.CreateFromTestCase(Business.UserManager.UserInternal, projectId, null, testCaseExecutionList, false);
			}
			catch (TestRunNoTestCasesException)
			{
				throw new InvalidOperationException("There were no test cases that match the current filters.");
			}

			//Now get the test run list XML node list
			XmlDocument xmlTestRunDoc = new XmlDocument();
			XmlNode xmlTestRunData = xmlTestRunDoc.CreateElement("TestRunData");
			xmlTestRunDoc.AppendChild(xmlTestRunData);

			//Serialize the pending record
			XmlNode xmlTestRunsPendingNode = xmlTestRunDoc.CreateElement("TestRunsPending");
			xmlTestRunsPendingNode.InnerXml = testRunPendingSet.InnerXml;
			xmlTestRunData.AppendChild(xmlTestRunsPendingNode);

			//Iterate through each test run in the collection and serialize
			foreach (TestRun testRun in testRunPendingSet.TestRuns)
			{
				XmlNode xmlTestRunNode = xmlTestRunDoc.CreateElement("TestRun");
				xmlTestRunNode.InnerXml = testRun.InnerXml;
				xmlTestRunData.AppendChild(xmlTestRunNode);

				//Now add the steps
				if (testRun.TestRunSteps.Count > 0)
				{
					XmlNode xmlTestRunSteps = xmlTestRunDoc.CreateElement("TestRunSteps");
					xmlTestRunNode.AppendChild(xmlTestRunSteps);

					foreach (TestRunStep testRunStep in testRun.TestRunSteps)
					{
						XmlNode xmlTestRunStepNode = xmlTestRunDoc.CreateElement("TestRunStep");
						xmlTestRunStepNode.InnerXml = testRunStep.InnerXml;
						xmlTestRunSteps.AppendChild(xmlTestRunStepNode);
					}
				}
			}

			//Loop through the test runs adding additional elements
			foreach (XmlNode xmlTestRunNode in xmlTestRunDoc.ChildNodes[0].ChildNodes)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

				//Make sure we have a TestRun node
				if (xmlTestRunNode.Name == "TestRun")
				{
					//Get the test case id
					XmlNode xmlTestCaseId = xmlTestRunNode.SelectSingleNode("TestCaseId");
					if (xmlTestCaseId != null)
					{
						int testCaseId = Int32.Parse(xmlTestCaseId.InnerText);

						//Now depending on the report elements the user has chosen
						//insert the appropriate secondary elements as child nodes
						//The test run steps are already present as a separate table

						//Custom Properties
						ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase);

						//Custom properties need to be specially parsed to add them to the XML in a way that can
						//be easily handled by XSLT.
						InsertCustomProperties(projectId, testCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase, artifactCustomProperty, xmlTestRunDoc, xmlTestRunNode, testCaseCustomProperties, timezoneId, dataMappingSystems);

						//Attachments
						if (reportElements.Contains("Attachments"))
						{
							Business.AttachmentManager attachmentManager = new Business.AttachmentManager();
							List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, testCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase, null, true, 1, Int32.MaxValue, null, 0);

							//Get the attachments as XML and add to the test case XML
							InsertChildEntity<ProjectAttachmentView>(projectId, projectTemplateId, attachments, xmlTestRunDoc, xmlTestRunNode, "Attachments", timezoneId, dataMappingSystems, "Attachment");

							//If the license supports source code we need to add these as well
							InsertSourceCodeAttachments(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, testCaseId, xmlTestRunDoc, xmlTestRunNode, "Attachments");
						}
					}

					//Now loop through the test run steps
					foreach (XmlNode xmlTestRunStepNode in xmlTestRunNode.SelectNodes("TestRunSteps/TestRunStep"))
					{
						//Get the test step id
						XmlNode xmlTestStepId = xmlTestRunStepNode.SelectSingleNode("TestStepId");
						if (xmlTestStepId != null)
						{
							int testStepId = Int32.Parse(xmlTestStepId.InnerText);

							//Now depending on the report elements the user has chosen
							//insert the appropriate secondary elements as child nodes

							//Custom Properties
							ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testStepId, DataModel.Artifact.ArtifactTypeEnum.TestStep);

							//Custom properties need to be specially parsed to add them to the XML in a way that can
							//be easily handled by XSLT.
							InsertCustomProperties(projectId, testStepId, DataModel.Artifact.ArtifactTypeEnum.TestStep, artifactCustomProperty, xmlTestRunDoc, xmlTestRunStepNode, testStepCustomProperties, timezoneId, dataMappingSystems);

							//Attachments
							if (reportElements.Contains("Attachments"))
							{
								Business.AttachmentManager attachmentManager = new Business.AttachmentManager();
								List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, testStepId, DataModel.Artifact.ArtifactTypeEnum.TestStep, null, true, 1, Int32.MaxValue, null, 0);

								//Get the attachments as XML and add to the test case XML
								InsertChildEntity<ProjectAttachmentView>(projectId, projectTemplateId, attachments, xmlTestRunDoc, xmlTestRunStepNode, "Attachments", timezoneId, dataMappingSystems, "Attachment");

								//If the license supports source code we need to add these as well
								InsertSourceCodeAttachments(projectId, DataModel.Artifact.ArtifactTypeEnum.TestStep, testStepId, xmlTestRunDoc, xmlTestRunStepNode, "Attachments");
							}
						}
					}
				}
			}

			//Localize any dates in the document
			LocalizeDates(xmlTestRunDoc, timezoneId);

			return xmlTestRunDoc;
		}

		/// <summary>
		/// Loads and displays the test run report
		/// </summary>
		/// <param name="reportElements">The list of seconary elements to render</param>
		/// <param name="reportFormatToken">The token for the report format</param>
		/// <param name="filters">Any filters to apply to the data</param>
		/// <param name="sorts">Any sorts to apply to the data</param>
		protected XmlDocument Get_TestRunDetails(int projectId, int projectTemplateId, string reportFormatToken, List<string> reportElements, Hashtable filters, Hashtable sorts, string timezoneId, UpdateBackgroundProcessStatus updateBackgroundProcessStatus, int progress)
		{
			//Get the offset from the timezone
			double utcOffset = 0;
			if (!String.IsNullOrEmpty(timezoneId))
			{
				TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
				if (timeZone != null)
				{
					utcOffset = timeZone.GetUtcOffset(DateTime.Now).TotalHours;
				}
			}

			//Get the custom property definitions for this project
			Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestRun, true, false, true);

			//Get the list of active data syncs for the project
			List<DataSyncSystem> dataMappingSystems = new DataMappingManager().RetrieveDataSyncSystemsForProject(projectId);

			//Extract the sort field and direction (we only support one sort at present)
			string sortField = "EndDate";
			bool sortAscending = false;
			if (sorts != null)
			{
				foreach (DictionaryEntry sortEntry in sorts)
				{
					sortField = (string)sortEntry.Key;
					sortAscending = ((string)sortEntry.Value == "asc");
					break;
				}
			}

			//Get the list of test runs
			TestRunManager testRunManager = new TestRunManager();
			int count = testRunManager.Count(projectId, filters, utcOffset);
			List<TestRunView> testRuns = new List<TestRunView>();
			for (int index = 1; index < count + 1; index += PAGINATION_SIZE)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

				testRuns.AddRange(testRunManager.Retrieve(projectId, sortField, sortAscending, index, PAGINATION_SIZE, filters, utcOffset));
			}

			//Clean any rich-text fields in the dataset
			CleanRichTextFields(testRuns, "Description");

			//Now get the test run list XML node list
			XmlDocument xmlTestRunDoc = new XmlDocument();
			XmlNode xmlTestRunData = xmlTestRunDoc.CreateElement("TestRunData");
			xmlTestRunDoc.AppendChild(xmlTestRunData);

			//Iterate through each entity in the collection and serialize
			foreach (TestRunView testRunView in testRuns)
			{
				XmlNode xmlTestRunNode = xmlTestRunDoc.CreateElement("TestRun");
				xmlTestRunNode.InnerXml = testRunView.InnerXml;
				xmlTestRunData.AppendChild(xmlTestRunNode);
			}

			//Loop through the test runs adding additional elements
			foreach (XmlNode xmlTestRunNode in xmlTestRunDoc.ChildNodes[0].ChildNodes)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

				//Get the test run id
				XmlNode xmlTestRunId = xmlTestRunNode.SelectSingleNode("TestRunId");
				if (xmlTestRunId != null)
				{
					int testRunId = Int32.Parse(xmlTestRunId.InnerText);

					//Now depending on the report elements the user has chosen
					//insert the appropriate secondary elements as child nodes

					if (reportElements.Contains("TestSteps"))
					{
						//Test Run Steps
						List<TestRunStepView> testRunSteps = testRunManager.TestRunStep_RetrieveForTestRun(testRunId);

						//Get the test run steps as XML and add to the test run XML
						InsertChildEntity<TestRunStepView>(projectId, projectTemplateId, testRunSteps, xmlTestRunDoc, xmlTestRunNode, "TestRunSteps", timezoneId, dataMappingSystems, "TestRunStep");
					}

					//Incidents
					if (reportElements.Contains("Incidents"))
					{
						//Incidents are related to the test run step, so use the special filter that aggregates to the Test Run as a whole
						IncidentManager incidentManager = new IncidentManager();
						Hashtable testRunFilters = new Hashtable();
						testRunFilters.Add("TestRunId", testRunId);
						List<IncidentView> incidents = incidentManager.Retrieve(projectId, "Name", true, 1, Int32.MaxValue, testRunFilters, utcOffset);

						//Get the incidents as XML and add to the test run XML
						InsertChildEntity<IncidentView>(projectId, projectTemplateId, incidents, xmlTestRunDoc, xmlTestRunNode, "Incidents", timezoneId, dataMappingSystems, "Incident");
					}

					//Attachments
					if (reportElements.Contains("Attachments"))
					{
						Business.AttachmentManager attachmentManager = new Business.AttachmentManager();
						List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, testRunId, DataModel.Artifact.ArtifactTypeEnum.TestRun, null, true, 1, Int32.MaxValue, null, 0);

						//Get the attachments as XML and add to the test case XML
						InsertChildEntity<ProjectAttachmentView>(projectId, projectTemplateId, attachments, xmlTestRunDoc, xmlTestRunNode, "Attachments", timezoneId, dataMappingSystems, "Attachment");

						//If the license supports source code we need to add these as well
						InsertSourceCodeAttachments(projectId, DataModel.Artifact.ArtifactTypeEnum.TestRun, testRunId, xmlTestRunDoc, xmlTestRunNode, "Attachments");
					}

					//Custom Properties
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testRunId, DataModel.Artifact.ArtifactTypeEnum.TestRun);

					//Custom properties need to be specially parsed to add them to the XML in a way that can
					//be easily handled by XSLT.
					InsertCustomProperties(projectId, testRunId, DataModel.Artifact.ArtifactTypeEnum.TestRun, artifactCustomProperty, xmlTestRunDoc, xmlTestRunNode, customProperties, timezoneId, dataMappingSystems);
				}
			}

			//Localize any dates in the document
			LocalizeDates(xmlTestRunDoc, timezoneId);

			return xmlTestRunDoc;
		}

		/// <summary>
		/// Gets the test case detailed report section data XML
		/// </summary>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="reportElements">The list of seconary elements to render</param>
		/// <param name="reportFormatToken">The token for the report format</param>
		/// <param name="filters">Any filters to apply to the data</param>
		/// <param name="sorts">Any sorts to apply to the data</param>
		/// <param name="folderId">Any folder to filter on (null = no filter, 0 = root (FOLDER_ID_ROOT))</param>
		/// <param name="projectId">The project</param>
		/// <param name="timezoneId">The timezone</param>
		protected XmlDocument Get_TestCaseDetails(int projectId, int projectTemplateId, string reportFormatToken, List<string> reportElements, int? folderId, Hashtable filters, Hashtable sorts, string timezoneId, UpdateBackgroundProcessStatus updateBackgroundProcessStatus, int progress)
		{
			//Get the offset from the timezone
			double utcOffset = 0;
			if (!String.IsNullOrEmpty(timezoneId))
			{
				TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
				if (timeZone != null)
				{
					utcOffset = timeZone.GetUtcOffset(DateTime.Now).TotalHours;
				}
			}

			//Extract the sort field and direction (we only support one sort at present)
			string sortField = "Name";
			bool sortAscending = true;
			if (sorts != null)
			{
				foreach (DictionaryEntry sortEntry in sorts)
				{
					sortField = (string)sortEntry.Key;
					sortAscending = ((string)sortEntry.Value == "asc");
					break;
				}
			}

			//Get the custom property and component definitions for this project
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			ComponentManager componentManager = new ComponentManager();
			List<CustomProperty> customProperties1 = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, true, false, true);
			List<CustomProperty> customProperties2 = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.TestStep, true, false, true);
			List<Component> components = componentManager.Component_Retrieve(projectId, false, false);

			//Get the list of active data syncs for the project
			List<DataSyncSystem> dataMappingSystems = new DataMappingManager().RetrieveDataSyncSystemsForProject(projectId);

			//Get the list of test cases. Have to handle release filter case separately
			//since this filter affects the execution status as well as the list of test cases
			TestCaseManager testCaseManager = new TestCaseManager();
			XmlDocument xmlTestCaseDoc = new XmlDocument();
			int releaseId = -1;
			if (filters["ReleaseId"] == null)
			{
				//First we need to get the folder hierarchy for either the folder or all folders
				//root gets only root test cases, all other folders are recursive
				List<TestCaseFolderHierarchyView> testCaseFolders = testCaseManager.TestCaseFolder_GetList(projectId);
				if (folderId.HasValue)
				{
					if (folderId == FOLDER_ID_ROOT)
					{
						testCaseFolders = testCaseFolders.Where(t => !t.ParentTestCaseFolderId.HasValue).ToList();
					}
					else
					{
						TestCaseFolderHierarchyView matchedFolder = testCaseFolders.FirstOrDefault(t => t.TestCaseFolderId == folderId.Value);
						if (matchedFolder != null)
						{
							//Get matched folder and children
							testCaseFolders = testCaseFolders.Where(t => HierarchicalList.IsSelfOrChildOf(t.IndentLevel, matchedFolder.IndentLevel)).ToList();
						}
					}
				}

				//Now get the test case list XML node list
				XmlNode xmlTestCaseData = xmlTestCaseDoc.CreateElement("TestCaseData");
				xmlTestCaseDoc.AppendChild(xmlTestCaseData);

				//First add the root folder, unless filtering by folder (other than root)
				if (!folderId.HasValue || folderId == FOLDER_ID_ROOT)
				{
					TestCaseFolder testCaseFolder = new TestCaseFolder();
					testCaseFolder.Name = GlobalResources.General.Global_Root;
					testCaseFolder.TestCaseFolderId = 0;

					XmlNode xmlTestCaseFolderNode = xmlTestCaseDoc.CreateElement("TestCaseFolder");
					xmlTestCaseFolderNode.InnerXml = testCaseFolder.InnerXml;
					xmlTestCaseData.AppendChild(xmlTestCaseFolderNode);
					AddIndentLevel(xmlTestCaseFolderNode, "");

					//Now get the test cases in this folder
					int count = testCaseManager.Count(projectId, filters, utcOffset, null);

					XmlNode xmlTestCasesCollectionNode = xmlTestCaseDoc.CreateElement("TestCases");
					xmlTestCaseFolderNode.AppendChild(xmlTestCasesCollectionNode);

					List<TestCaseView> testCases = new List<TestCaseView>();
					for (int index = 1; index < count + 1; index += PAGINATION_SIZE)
					{
						if (updateBackgroundProcessStatus != null)
						{
							updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
						}

						testCases.AddRange(testCaseManager.Retrieve(projectId, sortField, sortAscending, index, PAGINATION_SIZE, filters, utcOffset, null, false, false));

						//Clean any rich-text fields in the dataset
						CleanRichTextFields(testCases, "Description");
					}
					//Iterate through each entity in the collection and serialize
					foreach (TestCaseView testCaseView in testCases)
					{
						XmlNode xmlTestCaseNode = xmlTestCaseDoc.CreateElement("TestCase");
						xmlTestCaseNode.InnerXml = testCaseView.InnerXml;
						xmlTestCasesCollectionNode.AppendChild(xmlTestCaseNode);
					}
				}

				//Now loop through each folder
				foreach (TestCaseFolderHierarchyView testCaseFolderHierarchyView in testCaseFolders)
				{
					//Retrieve the actual folder again to get all its properties
					TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testCaseFolderHierarchyView.TestCaseFolderId);
					if (testCaseFolder != null)
					{
						//Clean the rich text
						testCaseFolder.Description = CleanRichText(testCaseFolder.Description);

						//Now get the test cases in this folder, don't include the folder if empty
						int count = testCaseManager.Count(projectId, filters, utcOffset, testCaseFolder.TestCaseFolderId);

						//Don't include the folder if empty, unless we have no filter or it has some descendents
						XmlNode xmlTestCaseFolderNode = null;
						if (count > 0 || testCaseFolder.TestCaseCount > 0 || filters == null || filters.Count == 0)
						{
							xmlTestCaseFolderNode = xmlTestCaseDoc.CreateElement("TestCaseFolder");
							xmlTestCaseFolderNode.InnerXml = testCaseFolder.InnerXml;
							xmlTestCaseData.AppendChild(xmlTestCaseFolderNode);
							AddIndentLevel(xmlTestCaseFolderNode, testCaseFolderHierarchyView.IndentLevel);
						}
						if (count > 0)
						{
							XmlNode xmlTestCasesCollectionNode = xmlTestCaseDoc.CreateElement("TestCases");
							xmlTestCaseFolderNode.AppendChild(xmlTestCasesCollectionNode);

							List<TestCaseView> testCases = new List<TestCaseView>();
							for (int index = 1; index < count + 1; index += PAGINATION_SIZE)
							{
								if (updateBackgroundProcessStatus != null)
								{
									updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
								}

								testCases.AddRange(testCaseManager.Retrieve(projectId, sortField, sortAscending, index, PAGINATION_SIZE, filters, utcOffset, testCaseFolder.TestCaseFolderId));

								//Clean any rich-text fields in the dataset
								CleanRichTextFields(testCases, "Description");
							}

							//Iterate through each entity in the collection and serialize
							foreach (TestCaseView testCaseView in testCases)
							{
								XmlNode xmlTestCaseNode = xmlTestCaseDoc.CreateElement("TestCase");
								xmlTestCaseNode.InnerXml = testCaseView.InnerXml;
								xmlTestCasesCollectionNode.AppendChild(xmlTestCaseNode);
							}
						}
					}
				}
			}
			else
			{
				releaseId = (int)filters["ReleaseId"];
				//Now we need to remove from the filters collection as the column doesn't exist in the TestCase table
				filters.Remove("ReleaseId");

				//First we need to get the folder hierarchy for either the folder or all folders
				//root gets only root test cases and folders, all other folders are recursive
				List<TestCaseFolderHierarchyView> testCaseFolders = testCaseManager.TestCaseFolder_GetList(projectId);
				if (folderId.HasValue)
				{
					if (folderId == FOLDER_ID_ROOT)
					{
						testCaseFolders = testCaseFolders.Where(t => !t.ParentTestCaseFolderId.HasValue).ToList();
					}
					else
					{
						TestCaseFolderHierarchyView matchedFolder = testCaseFolders.FirstOrDefault(t => t.TestCaseFolderId == folderId.Value);
						if (matchedFolder != null)
						{
							testCaseFolders = testCaseFolders.Where(t => HierarchicalList.IsSelfOrChildOf(t.IndentLevel, matchedFolder.IndentLevel)).ToList();
						}
					}
				}

				//Now get the test case list XML node list
				XmlNode xmlTestCaseData = xmlTestCaseDoc.CreateElement("TestCaseData");
				xmlTestCaseDoc.AppendChild(xmlTestCaseData);

				//First add the root folder, unless filtering by folder (other than root)
				if (!folderId.HasValue || folderId == FOLDER_ID_ROOT)
				{
					TestCaseFolder testCaseFolder = new TestCaseFolder();
					testCaseFolder.Name = GlobalResources.General.Global_Root;
					testCaseFolder.TestCaseFolderId = 0;

					XmlNode xmlTestCaseFolderNode = xmlTestCaseDoc.CreateElement("TestCaseFolder");
					xmlTestCaseFolderNode.InnerXml = testCaseFolder.InnerXml;
					xmlTestCaseData.AppendChild(xmlTestCaseFolderNode);
					AddIndentLevel(xmlTestCaseFolderNode, "");

					//Now get the test cases in this folder, don't include the folder if empty
					int count = testCaseManager.CountByRelease(projectId, releaseId, filters, utcOffset, null);

					XmlNode xmlTestCasesCollectionNode = xmlTestCaseDoc.CreateElement("TestCases");
					xmlTestCaseFolderNode.AppendChild(xmlTestCasesCollectionNode);

					List<TestCaseReleaseView> testCases = new List<TestCaseReleaseView>();
					for (int index = 1; index < count + 1; index += PAGINATION_SIZE)
					{
						if (updateBackgroundProcessStatus != null)
						{
							updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
						}

						testCases.AddRange(testCaseManager.RetrieveByReleaseId(projectId, releaseId, sortField, sortAscending, index, PAGINATION_SIZE, filters, utcOffset, null, false, false));

						//Clean any rich-text fields in the dataset
						CleanRichTextFields(testCases, "Description");
					}

					//Iterate through each entity in the collection and serialize
					foreach (TestCaseReleaseView testCaseView in testCases)
					{
						XmlNode xmlTestCaseNode = xmlTestCaseDoc.CreateElement("TestCase");
						xmlTestCaseNode.InnerXml = testCaseView.InnerXml;
						xmlTestCasesCollectionNode.AppendChild(xmlTestCaseNode);
					}
				}

				//Now loop through each folder
				foreach (TestCaseFolderHierarchyView testCaseFolderHierarchyView in testCaseFolders)
				{
					//Retrieve the actual folder again to get all its properties
					TestCaseFolder testCaseFolder = testCaseManager.TestCaseFolder_GetById(testCaseFolderHierarchyView.TestCaseFolderId);
					if (testCaseFolder != null)
					{
						//Clean the rich text
						testCaseFolder.Description = CleanRichText(testCaseFolder.Description);

						//Now get the test cases in this folder
						int count = testCaseManager.CountByRelease(projectId, releaseId, filters, utcOffset, testCaseFolder.TestCaseFolderId);

						//Don't include the folder if empty, unless we have no filter or it has some descendents
						XmlNode xmlTestCaseFolderNode = null;
						if (count > 0 || testCaseFolder.TestCaseCount > 0 || filters == null || filters.Count == 0)
						{
							xmlTestCaseFolderNode = xmlTestCaseDoc.CreateElement("TestCaseFolder");
							xmlTestCaseFolderNode.InnerXml = testCaseFolder.InnerXml;
							xmlTestCaseData.AppendChild(xmlTestCaseFolderNode);
							AddIndentLevel(xmlTestCaseFolderNode, testCaseFolderHierarchyView.IndentLevel);
						}
						if (count > 0)
						{
							XmlNode xmlTestCasesCollectionNode = xmlTestCaseDoc.CreateElement("TestCases");
							xmlTestCaseFolderNode.AppendChild(xmlTestCasesCollectionNode);

							List<TestCaseReleaseView> testCases = new List<TestCaseReleaseView>();
							for (int index = 1; index < count + 1; index += PAGINATION_SIZE)
							{
								if (updateBackgroundProcessStatus != null)
								{
									updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
								}

								testCases.AddRange(testCaseManager.RetrieveByReleaseId(projectId, releaseId, sortField, sortAscending, index, PAGINATION_SIZE, filters, utcOffset, testCaseFolder.TestCaseFolderId));

								//Clean any rich-text fields in the dataset
								CleanRichTextFields(testCases, "Description");
							}

							//Iterate through each entity in the collection and serialize
							foreach (TestCaseReleaseView testCaseView in testCases)
							{
								XmlNode xmlTestCaseNode = xmlTestCaseDoc.CreateElement("TestCase");
								xmlTestCaseNode.InnerXml = testCaseView.InnerXml;
								xmlTestCasesCollectionNode.AppendChild(xmlTestCaseNode);
							}
						}
					}
				}
			}

			//Loop through the test cases adding additional elements
			DiscussionManager discussionManager = new DiscussionManager();
			foreach (XmlNode xmlTestCaseNode in xmlTestCaseDoc.SelectNodes("/TestCaseData//TestCase[TestCaseId]"))
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

				//Get the test case id
				XmlNode xmlTestCaseId = xmlTestCaseNode.SelectSingleNode("TestCaseId");
				if (xmlTestCaseId != null)
				{
					int testCaseId = Int32.Parse(xmlTestCaseId.InnerText);

					//Now depending on the report elements the user has chosen
					//insert the appropriate secondary elements as child nodes

					//Components are simply a comma-separated list of names that we add a node
					//It's not part of the main view since it's a single-select

					//Get the componentIds
					XmlNode xmlComponentIds = xmlTestCaseNode.SelectSingleNode("ComponentIds");
					if (components != null && xmlComponentIds != null && !String.IsNullOrEmpty(xmlComponentIds.InnerText))
					{
						List<int> componentIds = xmlComponentIds.InnerText.FromDatabaseSerialization_List_Int32();
						string shortLegend;
						string componentNames;
						ComponentManager.GetComponentNamesFromIds(componentIds, components, GlobalResources.General.Global_Multiple, out shortLegend, out componentNames);
						if (!String.IsNullOrEmpty(componentNames))
						{
							XmlNode xmlComponentName = xmlTestCaseDoc.CreateElement("ComponentNames");
							xmlComponentName.InnerText = componentNames;
							xmlTestCaseNode.AppendChild(xmlComponentName);
						}
					}


					if (reportElements.Contains("TestSteps"))
					{
						//Test Steps
						List<TestStepView> testSteps = testCaseManager.RetrieveStepsForTestCase(testCaseId);

						//Get the test steps as XML and add to the test case XML
						InsertChildEntity<TestStepView>(projectId, projectTemplateId, testSteps, xmlTestCaseDoc, xmlTestCaseNode, "TestSteps", timezoneId, dataMappingSystems, "TestStep");

						//Iterate through the test steps and see if we have any parameter values set
						//See if this test step has any parameter values set
						XmlNodeList xmlTestStepNodes = xmlTestCaseNode.SelectNodes("TestSteps/TestStep");
						foreach (XmlNode xmlTestStepNode in xmlTestStepNodes)
						{
							int testStepId;
							if (Int32.TryParse(xmlTestStepNode.SelectSingleNode("TestStepId").InnerText, out testStepId))
							{
								List<TestStepParameter> testStepParameters = testCaseManager.RetrieveParameterValues(testStepId);
								if (testStepParameters.Count > 0)
								{
									//Get the parameters as XML and add to the test case XML
									InsertChildEntity<TestStepParameter>(projectId, projectTemplateId, testStepParameters, xmlTestCaseDoc, xmlTestStepNode, "Parameters", timezoneId, dataMappingSystems, "TestStepParameter");
								}
							}
						}

					}

					//Custom Properties
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, testCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase);

					//Custom properties need to be specially parsed to add them to the XML in a way that can
					//be easily handled by XSLT.
					InsertCustomProperties(projectId, testCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase, artifactCustomProperty, xmlTestCaseDoc, xmlTestCaseNode, customProperties1, timezoneId, dataMappingSystems);

					//Comments
					IEnumerable<IDiscussion> comments = discussionManager.Retrieve(testCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase, false);

					//Get the discussions as XML and add to the test case XML
					InsertComments(comments, "Discussion", xmlTestCaseDoc, xmlTestCaseNode, "Discussions");

					//Attachments
					if (reportElements.Contains("Attachments"))
					{
						Business.AttachmentManager attachmentManager = new Business.AttachmentManager();
						List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, testCaseId, DataModel.Artifact.ArtifactTypeEnum.TestCase, null, true, 1, Int32.MaxValue, null, 0);

						//Get the attachments as XML and add to the test case XML
						InsertChildEntity<ProjectAttachmentView>(projectId, projectTemplateId, attachments, xmlTestCaseDoc, xmlTestCaseNode, "Attachments", timezoneId, dataMappingSystems, "Attachment");

						//If the license supports source code we need to add these as well
						InsertSourceCodeAttachments(projectId, DataModel.Artifact.ArtifactTypeEnum.TestCase, testCaseId, xmlTestCaseDoc, xmlTestCaseNode, "Attachments");
					}

					//Requirements Coverage
					if (reportElements.Contains("Requirements"))
					{
						RequirementManager requirementManager = new RequirementManager();
						List<RequirementView> requirements = requirementManager.RetrieveCoveredByTestCaseId(Business.UserManager.UserInternal, projectId, testCaseId);

						//Get the requirements as XML and add to the test case XML
						InsertChildEntity(projectId, projectTemplateId, requirements, xmlTestCaseDoc, xmlTestCaseNode, "Requirements", timezoneId, dataMappingSystems, "Requirement");
					}

					//Test Runs
					if (reportElements.Contains("TestRuns"))
					{
						Business.TestRunManager testRunManager = new Business.TestRunManager();
						Hashtable filters2 = new Hashtable();
						filters2.Add("TestCaseId", testCaseId);
						//If we have a release filter set, then also apply this to the test run filters
						if (filters["ReleaseId"] != null)
						{
							filters2.Add("ReleaseId", (int)filters["ReleaseId"]);
						}
						int testRunCount = testRunManager.Count(projectId, filters2, utcOffset);
						List<TestRunView> testRuns = new List<TestRunView>();
						for (int index = 1; index < testRunCount + 1; index += PAGINATION_SIZE)
						{
							testRuns.AddRange(testRunManager.Retrieve(projectId, "EndDate", false, index, PAGINATION_SIZE, filters2, utcOffset));
						}

						//Get the test runs as XML and add to the test case XML
						XmlNode xmlTestRunsContainer = InsertChildEntity<TestRunView>(projectId, projectTemplateId, testRuns, xmlTestCaseDoc, xmlTestCaseNode, "TestRuns", timezoneId, dataMappingSystems, "TestRun");

						//Now add the test run steps to each of these test runs
						if (reportElements.Contains("TestSteps"))
						{
							//Loop through each test run
							foreach (XmlNode xmlTestRunNode in xmlTestRunsContainer.ChildNodes)
							{
								XmlNode xmlTestRunId = xmlTestRunNode.SelectSingleNode("TestRunId");
								if (xmlTestRunId != null)
								{
									int testRunId = Int32.Parse(xmlTestRunId.InnerText);
									//Test Run Steps
									List<TestRunStepView> testRunSteps = testRunManager.TestRunStep_RetrieveForTestRun(testRunId);

									//Get the test run steps as XML and add to the test run XML
									InsertChildEntity<TestRunStepView>(projectId, projectTemplateId, testRunSteps, xmlTestCaseDoc, xmlTestRunNode, "TestRunSteps", timezoneId, dataMappingSystems, "TestRunStep");
								}
							}
						}
					}

					//Test Sets
					if (reportElements.Contains("TestSets"))
					{
						//Get the list of test sets that this test case is in
						TestSetManager testSetManager = new TestSetManager();
						List<TestSetView> testSets = testSetManager.RetrieveByTestCaseId(projectId, testCaseId, "Name", true, 1, Int32.MaxValue, null, utcOffset);

						//Get the test sets as XML and add to the test case XML
						InsertChildEntity<TestSetView>(projectId, projectTemplateId, testSets, xmlTestCaseDoc, xmlTestCaseNode, "TestSets", timezoneId, dataMappingSystems, "TestSet");
					}

					//History
					if (reportElements.Contains("History"))
					{
						//Also need to merge in the test step custom properties
						List<CustomProperty> customProperties = new List<CustomProperty>();
						customProperties.AddRange(customProperties1);
						customProperties.AddRange(customProperties2);
						InsertArtifactHistory(projectId, projectTemplateId, testCaseId, Artifact.ArtifactTypeEnum.TestCase, utcOffset, xmlTestCaseDoc, xmlTestCaseNode, timezoneId, dataMappingSystems, customProperties, components);
					}

					//Incidents
					if (reportElements.Contains("Incidents"))
					{
						IncidentManager incidentManager = new IncidentManager();
						List<IncidentView> incidents = incidentManager.RetrieveByTestCaseId(testCaseId, false);

						//Get the incidents as XML and add to the test case XML
						InsertChildEntity<IncidentView>(projectId, projectTemplateId, incidents, xmlTestCaseDoc, xmlTestCaseNode, "Incidents", timezoneId, dataMappingSystems, "Incident");
					}

					//Releases
					if (reportElements.Contains("Releases"))
					{
						ReleaseManager releaseManager = new ReleaseManager();
						List<ReleaseView> releases = releaseManager.RetrieveMappedByTestCaseId(Business.UserManager.UserInternal, projectId, testCaseId);

						//If we have a release specified in the filter, only include that release and its children
						if (releaseId > 0)
						{
							List<ReleaseView> childReleases = releaseManager.RetrieveSelfAndChildren(projectId, releaseId, true, false);
							releases = releases.Where(r => childReleases.Any(c => c.ReleaseId == r.ReleaseId)).ToList();
						}

						//Get the releases as XML and add to the test case XML
						InsertChildEntity<ReleaseView>(projectId, projectTemplateId, releases, xmlTestCaseDoc, xmlTestCaseNode, "Releases", timezoneId, dataMappingSystems, "Release");
					}
				}
			}

			//Localize any dates in the document
			LocalizeDates(xmlTestCaseDoc, timezoneId);

			//Return the data XML document
			return xmlTestCaseDoc;
		}

		/// <summary>
		/// Cleans a rich text field in the datatable so that the report displays correctly
		/// </summary>
		/// <param name="dataTable">The datatable</param>
		/// <param name="fieldName">The field name</param>
		protected void CleanRichTextFields(DataTable dataTable, string fieldName)
		{
			CleanRichTextFields(dataTable, new string[] { fieldName });
		}

		/// <summary>
		/// Cleans rich text fields in the datatable so that the report displays correctly
		/// </summary>
		/// <param name="dataTable">The datatable</param>
		/// <param name="fieldNames">The field names</param>
		protected void CleanRichTextFields(DataTable dataTable, string[] fieldNames)
		{
			foreach (DataRow dataRow in dataTable.Rows)
			{
				foreach (string fieldName in fieldNames)
				{
					if (dataTable.Columns.Contains(fieldName) && dataTable.Columns[fieldName].DataType == typeof(string) && dataRow[fieldName] != DBNull.Value)
					{
						string cleanedRichText = CleanRichText((string)dataRow[fieldName]);
						dataRow[fieldName] = cleanedRichText;
					}
				}
			}
			dataTable.AcceptChanges();
		}

		/// <summary>
		/// Cleans a rich text field in the datatable so that the report displays correctly
		/// </summary>
		/// <param name="entities">The list of entities</param>
		/// <param name="fieldName">The field name</param>
		protected void CleanRichTextFields<T>(List<T> entities, string fieldName) where T : Entity
		{
			CleanRichTextFields<T>(entities, new string[] { fieldName });
		}

		/// <summary>
		/// Cleans rich text fields in the datatable so that the report displays correctly
		/// </summary>
		/// <param name="entities">The list of entities</param>
		/// <param name="fieldNames">The field names</param>
		protected void CleanRichTextFields<T>(List<T> entities, string[] fieldNames) where T : Entity
		{
			foreach (T typedEntity in entities)
			{
				Entity entity = (Entity)typedEntity;
				foreach (string fieldName in fieldNames)
				{
					if (entity.Properties.ContainsKey(fieldName) && entity.Properties[fieldName].PropertyType == typeof(string) && entity[fieldName] != null)
					{
						string cleanedRichText = CleanRichText((string)entity[fieldName]);
						entity[fieldName] = cleanedRichText;
					}
				}
			}
		}

		/// <summary>
		/// Cleans dirty HTML in the rich text description tags
		/// </summary>
		/// <param name="dirtyHtml">The 'dirty' HTML</param>
		/// <returns>The cleaned HTML</returns>
		protected string CleanRichText(string dirtyHtml)
		{
			if (String.IsNullOrEmpty(dirtyHtml))
			{
				return dirtyHtml;
			}

			// remove all scripts
			string cleanHtml = Regex.Replace(dirtyHtml, @"(<( )*script([^>])*>)(?s:.)*(<( )*(/)( )*script( )*>)", "", RegexOptions.IgnoreCase);

			// remove all style tags
			cleanHtml = Regex.Replace(cleanHtml, "(<( )*style([^>])*>)(?s:.)*(<( )*(/)( )*style( )*>)", "", RegexOptions.IgnoreCase);
			return cleanHtml;
		}

		/// <summary>
		/// Loads and displays the incident reports
		/// </summary>
		/// <param name="reportElements">The list of seconary elements to render</param>
		/// <param name="reportFormatToken">The token for the report format</param>
		/// <param name="filters">Any filters to apply to the data</param>
		/// <param name="sorts">Any sorts to apply to the data</param>
		protected XmlDocument Get_IncidentDetails(int userId, int projectId, int projectTemplateId, string reportFormatToken, List<string> reportElements, Hashtable filters, Hashtable sorts, string timezoneId, UpdateBackgroundProcessStatus updateBackgroundProcessStatus, int progress)
		{
			//Get the offset from the timezone
			double utcOffset = 0;
			if (!String.IsNullOrEmpty(timezoneId))
			{
				TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
				if (timeZone != null)
				{
					utcOffset = timeZone.GetUtcOffset(DateTime.Now).TotalHours;
				}
			}

			//Get the custom property and component definitions for this project
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			ComponentManager componentManager = new ComponentManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Incident, true, false, true);
			List<Component> components = componentManager.Component_Retrieve(projectId, false, false);

			//Get the list of active data syncs for the project
			List<DataSyncSystem> dataMappingSystems = new DataMappingManager().RetrieveDataSyncSystemsForProject(projectId);

			//Extract the sort field and direction (we only support one sort at present)
			string sortField = "IncidentId";
			bool sortAscending = true;
			if (sorts != null)
			{
				foreach (DictionaryEntry sortEntry in sorts)
				{
					sortField = (string)sortEntry.Key;
					sortAscending = ((string)sortEntry.Value == "asc");
					break;
				}
			}

			//Get the list of all incidents
			IncidentManager incidentManager = new IncidentManager();
			int count = incidentManager.Count(projectId, filters, utcOffset);
			List<IncidentView> incidents = new List<IncidentView>();
			for (int index = 0; index <= count; index += PAGINATION_SIZE)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

				incidents.AddRange(incidentManager.Retrieve(projectId, sortField, sortAscending, (index + 1), PAGINATION_SIZE, filters, utcOffset));
			}

			//Clean any rich-text fields in the dataset
			CleanRichTextFields(incidents, "Description");

			//Now get the incident list XML node list
			XmlDocument xmlIncidentsDoc = new XmlDocument();
			XmlNode xmlIncidentData = xmlIncidentsDoc.CreateElement("IncidentData");
			xmlIncidentsDoc.AppendChild(xmlIncidentData);

			//Iterate through each entity in the collection and serialize
			foreach (IncidentView incidentView in incidents)
			{
				XmlNode xmlIncidentNode = xmlIncidentsDoc.CreateElement("Incident");
				xmlIncidentNode.InnerXml = incidentView.InnerXml;
				xmlIncidentData.AppendChild(xmlIncidentNode);
			}

			//Loop through the incidents adding additional elements
			foreach (XmlNode xmlIncidentNode in xmlIncidentsDoc.ChildNodes[0].ChildNodes)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

				//Get the incident id
				XmlNode xmlIncidentId = xmlIncidentNode.SelectSingleNode("IncidentId");
				if (xmlIncidentId != null)
				{
					int incidentId = Int32.Parse(xmlIncidentId.InnerText);

					//Now depending on the report elements the user has chosen
					//insert the appropriate secondary elements as child nodes

					//Components are simply a comma-separated list of names that we add a node
					//It's not part of the main view since it's a single-select

					//Get the componentIds
					XmlNode xmlComponentIds = xmlIncidentNode.SelectSingleNode("ComponentIds");
					if (components != null && xmlComponentIds != null && !String.IsNullOrEmpty(xmlComponentIds.InnerText))
					{
						List<int> componentIds = xmlComponentIds.InnerText.FromDatabaseSerialization_List_Int32();
						string shortLegend;
						string componentNames;
						ComponentManager.GetComponentNamesFromIds(componentIds, components, GlobalResources.General.Global_Multiple, out shortLegend, out componentNames);
						if (!String.IsNullOrEmpty(componentNames))
						{
							XmlNode xmlComponentName = xmlIncidentsDoc.CreateElement("ComponentNames");
							xmlComponentName.InnerText = componentNames;
							xmlIncidentNode.AppendChild(xmlComponentName);
						}
					}

					//Custom Properties
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, incidentId, DataModel.Artifact.ArtifactTypeEnum.Incident);

					//Custom properties need to be specially parsed to add them to the XML in a way that can
					//be easily handled by XSLT.
					InsertCustomProperties(projectId, incidentId, DataModel.Artifact.ArtifactTypeEnum.Incident, artifactCustomProperty, xmlIncidentsDoc, xmlIncidentNode, customProperties, timezoneId, dataMappingSystems);

					//Incident Resolutions
					Incident incident = incidentManager.RetrieveById(incidentId, true);

					//Clean any rich-text fields in the dataset
					CleanRichTextFields(incident.Resolutions.ToList(), "Resolution");

					//Get the resolutions as XML and add to the incident XML
					InsertChildEntity<IncidentResolution>(projectId, projectTemplateId, incident.Resolutions.ToList(), xmlIncidentsDoc, xmlIncidentNode, "IncidentResolutions", timezoneId, dataMappingSystems, "IncidentResolution");

					//Artifact Links
					if (reportElements.Contains("Incidents") || reportElements.Contains("Requirements") || reportElements.Contains("Tasks"))
					{
						Business.ArtifactLinkManager artifactLinkManager = new Business.ArtifactLinkManager();
						List<ArtifactLinkView> artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId);

						//Remove any associations that were not wanted (requirements, incidents, tasks)
						if (artifactLinks != null)
						{
							List<ArtifactLinkView> wantedArtifactLinks = artifactLinks
								.Where(a => (a.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.Requirement && reportElements.Contains("Requirements")) || (a.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.Incident && reportElements.Contains("Incidents")) || (a.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.Task && reportElements.Contains("Tasks"))).ToList();

							//Get the artifact links as XML and add to the incident XML
							InsertChildEntity<ArtifactLinkView>(projectId, projectTemplateId, wantedArtifactLinks, xmlIncidentsDoc, xmlIncidentNode, "ArtifactLinks", timezoneId, dataMappingSystems, "ArtifactLink");
						}
					}

					//Source Code
					if (reportElements.Contains("SourceCode"))
					{
						InsertSourceCodeRevisions(userId, projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId, xmlIncidentsDoc, xmlIncidentNode, "SourceCodeRevisions");
					}

					//Attachments
					if (reportElements.Contains("Attachments"))
					{
						Business.AttachmentManager attachmentManager = new Business.AttachmentManager();
						List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, incidentId, DataModel.Artifact.ArtifactTypeEnum.Incident, null, true, 1, Int32.MaxValue, null, 0);

						//Get the attachments as XML and add to the incident XML
						InsertChildEntity<ProjectAttachmentView>(projectId, projectTemplateId, attachments, xmlIncidentsDoc, xmlIncidentNode, "Attachments", timezoneId, dataMappingSystems, "Attachment");

						//If the license supports source code we need to add these as well
						InsertSourceCodeAttachments(projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId, xmlIncidentsDoc, xmlIncidentNode, "Attachments");
					}

					//History
					if (reportElements.Contains("History"))
					{
						InsertArtifactHistory(projectId, projectTemplateId, incidentId, Artifact.ArtifactTypeEnum.Incident, utcOffset, xmlIncidentsDoc, xmlIncidentNode, timezoneId, dataMappingSystems, customProperties, components);
					}
				}
			}

			//Localize any dates in the document
			LocalizeDates(xmlIncidentsDoc, timezoneId);

			return xmlIncidentsDoc;
		}

		/// <summary>
		/// Loads and displays the risk reports
		/// </summary>
		/// <param name="reportElements">The list of seconary elements to render</param>
		/// <param name="reportFormatToken">The token for the report format</param>
		/// <param name="filters">Any filters to apply to the data</param>
		/// <param name="sorts">Any sorts to apply to the data</param>
		protected XmlDocument Get_RiskDetails(int userId, int projectId, int projectTemplateId, string reportFormatToken, List<string> reportElements, Hashtable filters, Hashtable sorts, string timezoneId, UpdateBackgroundProcessStatus updateBackgroundProcessStatus, int progress)
		{
			//Get the offset from the timezone
			double utcOffset = 0;
			if (!String.IsNullOrEmpty(timezoneId))
			{
				TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
				if (timeZone != null)
				{
					utcOffset = timeZone.GetUtcOffset(DateTime.Now).TotalHours;
				}
			}

			//Get the custom property and component definitions for this project
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Risk, true, false, true);

			//Extract the sort field and direction (we only support one sort at present)
			string sortField = "RiskId";
			bool sortAscending = true;
			if (sorts != null)
			{
				foreach (DictionaryEntry sortEntry in sorts)
				{
					sortField = (string)sortEntry.Key;
					sortAscending = ((string)sortEntry.Value == "asc");
					break;
				}
			}

			//Get the list of all risks
			RiskManager riskManager = new RiskManager();
			int count = riskManager.Risk_Count(projectId, filters, utcOffset);
			List<RiskView> risks = new List<RiskView>();
			for (int index = 0; index <= count; index += PAGINATION_SIZE)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

				risks.AddRange(riskManager.Risk_Retrieve(projectId, sortField, sortAscending, (index + 1), PAGINATION_SIZE, filters, utcOffset));
			}

			//Clean any rich-text fields in the dataset
			CleanRichTextFields(risks, "Description");

			//Now get the risk list XML node list
			XmlDocument xmlRisksDoc = new XmlDocument();



			XmlNode xmlRiskData = xmlRisksDoc.CreateElement("RiskData");
			xmlRisksDoc.AppendChild(xmlRiskData);

			//Iterate through each entity in the collection and serialize
			foreach (RiskView riskView in risks)
			{
				XmlNode xmlRiskNode = xmlRisksDoc.CreateElement("Risk");
				xmlRiskNode.InnerXml = riskView.InnerXml;
				xmlRiskData.AppendChild(xmlRiskNode);
			}

			//Loop through the risks adding additional elements
			foreach (XmlNode xmlRiskNode in xmlRisksDoc.ChildNodes[0].ChildNodes)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

				//Get the risk id
				XmlNode xmlRiskId = xmlRiskNode.SelectSingleNode("RiskId");
				if (xmlRiskId != null)
				{
					int riskId = Int32.Parse(xmlRiskId.InnerText);

					//Now depending on the report elements the user has chosen
					//insert the appropriate secondary elements as child nodes

					//Custom Properties
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, riskId, DataModel.Artifact.ArtifactTypeEnum.Risk);

					//Custom properties need to be specially parsed to add them to the XML in a way that can
					//be easily handled by XSLT.
					InsertCustomProperties(projectId, riskId, DataModel.Artifact.ArtifactTypeEnum.Risk, artifactCustomProperty, xmlRisksDoc, xmlRiskNode, customProperties, timezoneId, null);

					//Risk Comments and mitigations
					Risk risk = riskManager.Risk_RetrieveById(riskId, true, true);

					//Clean any rich-text fields in the dataset
					CleanRichTextFields(risk.Discussions.ToList(), "Text");

					//Get the discussions as XML and add to the risk XML
					InsertChildEntity<RiskDiscussion>(projectId, projectTemplateId, risk.Discussions.ToList(), xmlRisksDoc, xmlRiskNode, "RiskDiscussions", timezoneId, null, "RiskDiscussion");

					//Attachments
					if (reportElements.Contains("Attachments"))
					{
						Business.AttachmentManager attachmentManager = new Business.AttachmentManager();
						List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, riskId, DataModel.Artifact.ArtifactTypeEnum.Risk, null, true, 1, Int32.MaxValue, null, 0);

						//Get the attachments as XML and add to the risk XML
						InsertChildEntity<ProjectAttachmentView>(projectId, projectTemplateId, attachments, xmlRisksDoc, xmlRiskNode, "Attachments", timezoneId, null, "Attachment");

						//If the license supports source code we need to add these as well
						InsertSourceCodeAttachments(projectId, DataModel.Artifact.ArtifactTypeEnum.Risk, riskId, xmlRisksDoc, xmlRiskNode, "Attachments");
					}

					//History
					if (reportElements.Contains("History"))
					{
						InsertArtifactHistory(projectId, projectTemplateId, riskId, Artifact.ArtifactTypeEnum.Risk, utcOffset, xmlRisksDoc, xmlRiskNode, timezoneId, null, customProperties);
					}

					//Tasks
					if (reportElements.Contains("Tasks"))
					{
						Business.TaskManager taskManager = new Business.TaskManager();
						Hashtable taskFilters = new Hashtable();
						taskFilters.Add("RiskId", riskId);
						List<TaskView> tasks = taskManager.Retrieve(projectId, "StartDate", true, 1, Int32.MaxValue, taskFilters, utcOffset);

						//Get the tasks as XML and add to the risk XML
						InsertChildEntity<TaskView>(projectId, projectTemplateId, tasks, xmlRisksDoc, xmlRiskNode, "Tasks", timezoneId, null, "Task");
					}

					//Mitigations
					if (reportElements.Contains("Mitigations"))
					{
						List<RiskMitigation> mitigations = riskManager.RiskMitigation_Retrieve(riskId);

						//Get the tasks as XML and add to the risk XML
						InsertChildEntity<RiskMitigation>(projectId, projectTemplateId, mitigations, xmlRisksDoc, xmlRiskNode, "Mitigations", timezoneId, null, "Mitigation");
					}
				}
			}


			//Localize any dates in the document
			LocalizeDates(xmlRisksDoc, timezoneId);

			return xmlRisksDoc;
		}


		/// <summary>
		/// Loads and displays the risk reports
		/// </summary>
		/// <param name="reportElements">The list of seconary elements to render</param>
		/// <param name="reportFormatToken">The token for the report format</param>
		/// <param name="filters">Any filters to apply to the data</param>
		/// <param name="sorts">Any sorts to apply to the data</param>

		protected XmlDocument Get_AllProjectHistoryDetails(int userId, int projectId, int projectTemplateId, string reportFormatToken, List<string> reportElements, Hashtable filters, Hashtable sorts, string timezoneId, UpdateBackgroundProcessStatus updateBackgroundProcessStatus, int progress)
		{
			//Get the offset from the timezone
			double utcOffset = 0;
			if (!String.IsNullOrEmpty(timezoneId))
			{
				TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
				if (timeZone != null)
				{
					utcOffset = timeZone.GetUtcOffset(DateTime.Now).TotalHours;
				}
			}

			//Get the custom property and component definitions for this project
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Risk, true, false, true);

			//Extract the sort field and direction (we only support one sort at present)
			string sortField = "ChangeSetId";
			bool sortAscending = true;
			if (sorts != null)
			{
				foreach (DictionaryEntry sortEntry in sorts)
				{
					sortField = (string)sortEntry.Key;
					sortAscending = ((string)sortEntry.Value == "asc");
					break;
				}
			}

			List<long> changesetids = new List<long>();
			int? year = null;
			int? month = null;

			foreach (DictionaryEntry i in filters)
			{
				string key = i.Key.ToString();
				string data = i.Value.ToString();
				string[] list = data.Split(',');
				if (key == "ProjectHistoryChangeYear")
				{
					foreach (string l in list)
					{
						int id; int.TryParse(l, out id);
						year = id;
					}
				}
				if (key == "ProjectHistoryChangeMonth")
				{
					foreach (string l in list)
					{
						int id; int.TryParse(l, out id);
						month = id;
					}
				}
			}

			//Get the list of all risks
			HistoryManager historyManager = new HistoryManager();
			List<HistoryChangeSetResponse> historyChangeSets = new List<HistoryChangeSetResponse>();

			historyChangeSets = historyManager.RetrieveSetsByProjectId1(projectId, year, month, null, utcOffset, "", sortAscending, null, 1, -1).ToList();
			//.Where(t => changesetids.Contains(t.ChangeSetId)).ToList();


			for (int index = 0; index <= historyChangeSets.Count; index += PAGINATION_SIZE)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

			}


			//Now get the risk list XML node list
			XmlDocument xmlHistoryListDoc = new XmlDocument();
			XmlNode xmlHistoryData = xmlHistoryListDoc.CreateElement("ProductHistoryData");
			try
			{
				xmlHistoryListDoc.AppendChild(xmlHistoryData);
			}
			catch (Exception ex)
			{
				var msg = ex.Message;
			}

			//Iterate through each entity in the collection and serialize
			foreach (HistoryChangeSetResponse history in historyChangeSets)
			{
				XmlNode xmlHistoryNode = xmlHistoryListDoc.CreateElement("ProductHistory");
				xmlHistoryNode.InnerXml = history.InnerXml;
				xmlHistoryData.AppendChild(xmlHistoryNode);
			}

			//Loop through the risks adding additional elements
			foreach (XmlNode xmlRiskNode in xmlHistoryListDoc.ChildNodes[0].ChildNodes)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}


			}

			//Localize any dates in the document
			LocalizeDates(xmlHistoryListDoc, timezoneId);

			return xmlHistoryListDoc;
		}

		protected XmlDocument Get_ProjectHistoryDetails(int userId, int projectId, int projectTemplateId, string reportFormatToken, List<string> reportElements, Hashtable filters, Hashtable sorts, string timezoneId, UpdateBackgroundProcessStatus updateBackgroundProcessStatus, int progress)
		{
			//Get the offset from the timezone
			double utcOffset = 0;
			if (!String.IsNullOrEmpty(timezoneId))
			{
				TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
				if (timeZone != null)
				{
					utcOffset = timeZone.GetUtcOffset(DateTime.Now).TotalHours;
				}
			}

			//Get the custom property and component definitions for this project
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Risk, true, false, true);

			//Extract the sort field and direction (we only support one sort at present)
			string sortField = "ChangeSetId";
			bool sortAscending = true;
			if (sorts != null)
			{
				foreach (DictionaryEntry sortEntry in sorts)
				{
					sortField = (string)sortEntry.Key;
					sortAscending = ((string)sortEntry.Value == "asc");
					break;
				}
			}

			List<long> changesetids = new List<long>();

			foreach (DictionaryEntry i in filters)
			{
				string data = i.Value.ToString();
				string[] list = data.Split(',');
				foreach (string l in list)
				{
					long id; long.TryParse(l, out id);
					changesetids.Add(id);
				}
			}

			//Get the list of all risks
			HistoryManager historyManager = new HistoryManager();
			List<HistoryChangeSetResponse> historyChangeSets = new List<HistoryChangeSetResponse>();
			List<OldHistoryChangesetResponse> oldHistoryChangesetResponses = new List<OldHistoryChangesetResponse>();
			if (changesetids.Count > 0)
			{
				if (Inflectra.SpiraTest.Common.Global.IsOldToolExport)
				{
					oldHistoryChangesetResponses = historyManager.RetrieveOldHistorySetsByProjectIdExport(projectId, utcOffset, "", sortAscending, null, 1, -1).Where(t => changesetids.Contains(t.ChangeSetId)).ToList();

				}
				else
				  historyChangeSets = historyManager.RetrieveSetsByProjectId1(projectId, null, null, null, utcOffset, "", sortAscending, null, 1, -1).Where(t => changesetids.Contains(t.ChangeSetId)).ToList();
			}
			else
			{
				if (Inflectra.SpiraTest.Common.Global.IsOldToolExport)
				{
					oldHistoryChangesetResponses= historyManager.RetrieveOldHistorySetsByProjectIdExport(projectId,utcOffset,"",sortAscending, null, 1, -1).ToList();

				}
				else
				{
					historyChangeSets = historyManager.RetrieveSetsByProjectId1(projectId, null, null, null, utcOffset, "", sortAscending, null, 1, -1).ToList();
				}
			}

			for (int index = 0; index <= historyChangeSets.Count; index += PAGINATION_SIZE)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

			}
			if(oldHistoryChangesetResponses.Count>0)
			{
				for (int index = 0; index <= oldHistoryChangesetResponses.Count; index += PAGINATION_SIZE)
				{
					if (updateBackgroundProcessStatus != null)
					{
						updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
					}

				}
			}


			//Now get the risk list XML node list
			XmlDocument xmlHistoryListDoc = new XmlDocument();
			XmlNode xmlHistoryData = xmlHistoryListDoc.CreateElement("ProductHistoryData");
			try
			{
				xmlHistoryListDoc.AppendChild(xmlHistoryData);
			}
			catch (Exception ex)
			{
				var msg = ex.Message;
			}

			//Iterate through each entity in the collection and serialize
			foreach (HistoryChangeSetResponse history in historyChangeSets)
			{
				XmlNode xmlHistoryNode = xmlHistoryListDoc.CreateElement("ProductHistory");
				xmlHistoryNode.InnerXml = history.InnerXml;
				xmlHistoryData.AppendChild(xmlHistoryNode);
			}

			if (oldHistoryChangesetResponses.Count > 0)
			{
				foreach (OldHistoryChangesetResponse history in oldHistoryChangesetResponses)
				{
					XmlNode xmlHistoryNode = xmlHistoryListDoc.CreateElement("ProductHistory");
					xmlHistoryNode.InnerXml = history.InnerXml;
					xmlHistoryData.AppendChild(xmlHistoryNode);
				}
			}
			//Loop through the risks adding additional elements
			foreach (XmlNode xmlRiskNode in xmlHistoryListDoc.ChildNodes[0].ChildNodes)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}
			}

			//Localize any dates in the document
			LocalizeDates(xmlHistoryListDoc, timezoneId);

			return xmlHistoryListDoc;
		}

		protected XmlDocument Get_AllAuditTrailDetails(int userId, int projectId, int projectTemplateId, string reportFormatToken, List<string> reportElements, Hashtable filters, Hashtable sorts, string timezoneId, UpdateBackgroundProcessStatus updateBackgroundProcessStatus, int progress)
		{
			//Get the offset from the timezone
			double utcOffset = 0;
			if (!String.IsNullOrEmpty(timezoneId))
			{
				TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
				if (timeZone != null)
				{
					utcOffset = timeZone.GetUtcOffset(DateTime.Now).TotalHours;
				}
			}

			//Get the custom property and component definitions for this project
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Risk, true, false, true);

			//Extract the sort field and direction (we only support one sort at present)
			string sortField = "ChangeSetId";
			bool sortAscending = true;
			if (sorts != null)
			{
				foreach (DictionaryEntry sortEntry in sorts)
				{
					sortField = (string)sortEntry.Key;
					sortAscending = ((string)sortEntry.Value == "asc");
					break;
				}
			}

			List<long> changesetids = new List<long>();

			foreach (DictionaryEntry i in filters)
			{
				string data = i.Value.ToString();
				string[] list = data.Split(',');
				foreach (string l in list)
				{
					long id; long.TryParse(l, out id);
					changesetids.Add(id);
				}
			}

			//Get the list of all risks
			HistoryManager historyManager = new HistoryManager();
			List<HistoryChangeSetResponse> historyChangeSets = historyManager.RetrieveSets(utcOffset, "", sortAscending, null, 1, -1).ToList();
			//.Where(t => changesetids.Contains(t.ChangeSetId))


			for (int index = 0; index <= historyChangeSets.Count; index += PAGINATION_SIZE)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

			}


			//Now get the risk list XML node list
			XmlDocument xmlHistoryListDoc = new XmlDocument();
			XmlNode xmlHistoryData = xmlHistoryListDoc.CreateElement("ProductHistoryData");
			xmlHistoryListDoc.AppendChild(xmlHistoryData);

			//Iterate through each entity in the collection and serialize
			foreach (HistoryChangeSetResponse history in historyChangeSets)
			{
				XmlNode xmlHistoryNode = xmlHistoryListDoc.CreateElement("ProductHistory");
				xmlHistoryNode.InnerXml = history.InnerXml;
				xmlHistoryData.AppendChild(xmlHistoryNode);
			}

			//Loop through the risks adding additional elements
			foreach (XmlNode xmlRiskNode in xmlHistoryListDoc.ChildNodes[0].ChildNodes)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}


			}

			//Localize any dates in the document
			LocalizeDates(xmlHistoryListDoc, timezoneId);

			return xmlHistoryListDoc;
		}

		protected XmlDocument Get_AuditTrailDetails(int userId, int projectId, int projectTemplateId, string reportFormatToken, List<string> reportElements, Hashtable filters, Hashtable sorts, string timezoneId, UpdateBackgroundProcessStatus updateBackgroundProcessStatus, int progress)
		{
			//Get the offset from the timezone
			double utcOffset = 0;
			if (!String.IsNullOrEmpty(timezoneId))
			{
				TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
				if (timeZone != null)
				{
					utcOffset = timeZone.GetUtcOffset(DateTime.Now).TotalHours;
				}
			}

			//Get the custom property and component definitions for this project
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Risk, true, false, true);

			//Extract the sort field and direction (we only support one sort at present)
			string sortField = "ChangeSetId";
			bool sortAscending = true;
			if (sorts != null)
			{
				foreach (DictionaryEntry sortEntry in sorts)
				{
					sortField = (string)sortEntry.Key;
					sortAscending = ((string)sortEntry.Value == "asc");
					break;
				}
			}

			List<long> changesetids = new List<long>();

			foreach (DictionaryEntry i in filters)
			{
				string data = i.Value.ToString();
				string[] list = data.Split(',');
				foreach (string l in list)
				{
					long id; long.TryParse(l, out id);
					changesetids.Add(id);
				}
			}

			//Get the list of all risks
			HistoryManager historyManager = new HistoryManager();
			List<HistoryChangeSetResponse> historyChangeSets = new List<HistoryChangeSetResponse>();
			if (changesetids.Count > 0)
			{
				historyChangeSets = historyManager.RetrieveSets(utcOffset, "", sortAscending, null, 1, -1).Where(t => changesetids.Contains(t.ChangeSetId)).ToList();
			}
			else
			{
				historyChangeSets = historyManager.RetrieveSets(utcOffset, "", sortAscending, null, 1, -1).ToList();
			}

			for (int index = 0; index <= historyChangeSets.Count; index += PAGINATION_SIZE)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

			}


			//Now get the risk list XML node list
			XmlDocument xmlHistoryListDoc = new XmlDocument();
			XmlNode xmlHistoryData = xmlHistoryListDoc.CreateElement("ProductHistoryData");
			xmlHistoryListDoc.AppendChild(xmlHistoryData);

			//Iterate through each entity in the collection and serialize
			foreach (HistoryChangeSetResponse history in historyChangeSets)
			{
				XmlNode xmlHistoryNode = xmlHistoryListDoc.CreateElement("ProductHistory");
				xmlHistoryNode.InnerXml = history.InnerXml;
				xmlHistoryData.AppendChild(xmlHistoryNode);
			}

			//Loop through the risks adding additional elements
			foreach (XmlNode xmlRiskNode in xmlHistoryListDoc.ChildNodes[0].ChildNodes)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}


			}

			//Localize any dates in the document
			LocalizeDates(xmlHistoryListDoc, timezoneId);

			return xmlHistoryListDoc;
		}


		protected XmlDocument Get_AllAdminAuditHistoryDetails(int userId, int projectId, int projectTemplateId, string reportFormatToken, List<string> reportElements, Hashtable filters, Hashtable sorts, string timezoneId, UpdateBackgroundProcessStatus updateBackgroundProcessStatus, int progress)
		{
			//Get the offset from the timezone
			double utcOffset = 0;
			if (!String.IsNullOrEmpty(timezoneId))
			{
				TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
				if (timeZone != null)
				{
					utcOffset = timeZone.GetUtcOffset(DateTime.Now).TotalHours;
				}
			}

			//Get the custom property and component definitions for this project
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Risk, true, false, true);

			//Extract the sort field and direction (we only support one sort at present)
			string sortField = "ChangeSetId";
			bool sortAscending = true;
			if (sorts != null)
			{
				foreach (DictionaryEntry sortEntry in sorts)
				{
					sortField = (string)sortEntry.Key;
					sortAscending = ((string)sortEntry.Value == "asc");
					break;
				}
			}

			List<long> changesetids = new List<long>();

			foreach (DictionaryEntry i in filters)
			{
				string data = i.Value.ToString();
				string[] list = data.Split(',');
				foreach (string l in list)
				{
					long id; long.TryParse(l, out id);
					changesetids.Add(id);
				}
			}

			//Get the list of all risks
			AdminAuditManager historyManager = new AdminAuditManager();
			List<AdminHistoryChangeSetResponse> historyChangeSets = historyManager.RetrieveAdminHistoryChangeSets(utcOffset, "", sortAscending, null, 1, -1).ToList();
			//.Where(t => changesetids.Contains(t.ChangeSetId))


			for (int index = 0; index <= historyChangeSets.Count; index += PAGINATION_SIZE)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

			}


			//Now get the risk list XML node list
			XmlDocument xmlHistoryListDoc = new XmlDocument();
			XmlNode xmlHistoryData = xmlHistoryListDoc.CreateElement("ProductHistoryData");
			xmlHistoryListDoc.AppendChild(xmlHistoryData);

			//Iterate through each entity in the collection and serialize
			foreach (AdminHistoryChangeSetResponse history in historyChangeSets)
			{
				XmlNode xmlHistoryNode = xmlHistoryListDoc.CreateElement("ProductHistory");
				xmlHistoryNode.InnerXml = history.InnerXml;
				xmlHistoryData.AppendChild(xmlHistoryNode);
			}

			//Loop through the risks adding additional elements
			foreach (XmlNode xmlRiskNode in xmlHistoryListDoc.ChildNodes[0].ChildNodes)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}


			}

			//Localize any dates in the document
			LocalizeDates(xmlHistoryListDoc, timezoneId);

			return xmlHistoryListDoc;
		}

		protected XmlDocument Get_AdminAuditHistoryDetails(int userId, int projectId, int projectTemplateId, string reportFormatToken, List<string> reportElements, Hashtable filters, Hashtable sorts, string timezoneId, UpdateBackgroundProcessStatus updateBackgroundProcessStatus, int progress)
		{
			//Get the offset from the timezone
			double utcOffset = 0;
			if (!String.IsNullOrEmpty(timezoneId))
			{
				TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
				if (timeZone != null)
				{
					utcOffset = timeZone.GetUtcOffset(DateTime.Now).TotalHours;
				}
			}

			//Get the custom property and component definitions for this project
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Risk, true, false, true);

			//Extract the sort field and direction (we only support one sort at present)
			string sortField = "ChangeSetId";
			bool sortAscending = true;
			if (sorts != null)
			{
				foreach (DictionaryEntry sortEntry in sorts)
				{
					sortField = (string)sortEntry.Key;
					sortAscending = ((string)sortEntry.Value == "asc");
					break;
				}
			}

			List<long> changesetids = new List<long>();

			foreach (DictionaryEntry i in filters)
			{
				string data = i.Value.ToString();
				string[] list = data.Split(',');
				foreach (string l in list)
				{
					long id; long.TryParse(l, out id);
					changesetids.Add(id);
				}
			}

			//Get the list of all risks
			AdminAuditManager historyManager = new AdminAuditManager();
			List<AdminHistoryChangeSetResponse> historyChangeSets = new List<AdminHistoryChangeSetResponse>();
			if (changesetids.Count > 0)
			{
				historyChangeSets = historyManager.RetrieveAdminHistoryChangeSets(utcOffset, "", sortAscending, null, 1, -1).Where(t => changesetids.Contains(t.ChangeSetId)).ToList();
			}
			else
			{
				historyChangeSets = historyManager.RetrieveAdminHistoryChangeSets(utcOffset, "", sortAscending, null, 1, -1).ToList();
			}


			for (int index = 0; index <= historyChangeSets.Count; index += PAGINATION_SIZE)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

			}


			//Now get the risk list XML node list
			XmlDocument xmlHistoryListDoc = new XmlDocument();
			XmlNode xmlHistoryData = xmlHistoryListDoc.CreateElement("ProductHistoryData");
			xmlHistoryListDoc.AppendChild(xmlHistoryData);

			//Iterate through each entity in the collection and serialize
			foreach (AdminHistoryChangeSetResponse history in historyChangeSets)
			{
				XmlNode xmlHistoryNode = xmlHistoryListDoc.CreateElement("ProductHistory");
				xmlHistoryNode.InnerXml = history.InnerXml;
				xmlHistoryData.AppendChild(xmlHistoryNode);
			}

			//Loop through the risks adding additional elements
			foreach (XmlNode xmlRiskNode in xmlHistoryListDoc.ChildNodes[0].ChildNodes)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}


			}

			//Localize any dates in the document
			LocalizeDates(xmlHistoryListDoc, timezoneId);

			return xmlHistoryListDoc;
		}

		protected XmlDocument Get_AllUserAuditHistoryDetails(int userId, int projectId, int projectTemplateId, string reportFormatToken, List<string> reportElements, Hashtable filters, Hashtable sorts, string timezoneId, UpdateBackgroundProcessStatus updateBackgroundProcessStatus, int progress)
		{
			//Get the offset from the timezone
			double utcOffset = 0;
			if (!String.IsNullOrEmpty(timezoneId))
			{
				TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
				if (timeZone != null)
				{
					utcOffset = timeZone.GetUtcOffset(DateTime.Now).TotalHours;
				}
			}

			//Get the custom property and component definitions for this project
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Risk, true, false, true);

			//Extract the sort field and direction (we only support one sort at present)
			string sortField = "ChangeSetId";
			bool sortAscending = true;
			if (sorts != null)
			{
				foreach (DictionaryEntry sortEntry in sorts)
				{
					sortField = (string)sortEntry.Key;
					sortAscending = ((string)sortEntry.Value == "asc");
					break;
				}
			}

			List<long> changesetids = new List<long>();

			foreach (DictionaryEntry i in filters)
			{
				string data = i.Value.ToString();
				string[] list = data.Split(',');
				foreach (string l in list)
				{
					long id; long.TryParse(l, out id);
					changesetids.Add(id);
				}
			}

			//Get the list of all risks
			UserAuditManager userAuditManager = new UserAuditManager();
			List<UserHistoryChangeSetResponse> userHistoryChangeSets = userAuditManager.RetrieveUserHistoryChangeSets(utcOffset, "", sortAscending, null, 1, -1).ToList();


			for (int index = 0; index <= userHistoryChangeSets.Count; index += PAGINATION_SIZE)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

			}


			//Now get the risk list XML node list
			XmlDocument xmlHistoryListDoc = new XmlDocument();
			XmlNode xmlHistoryData = xmlHistoryListDoc.CreateElement("UserHistoryData");
			xmlHistoryListDoc.AppendChild(xmlHistoryData);

			//Iterate through each entity in the collection and serialize
			foreach (UserHistoryChangeSetResponse history in userHistoryChangeSets)
			{
				XmlNode xmlHistoryNode = xmlHistoryListDoc.CreateElement("UserHistory");
				xmlHistoryNode.InnerXml = history.InnerXml;
				xmlHistoryData.AppendChild(xmlHistoryNode);
			}

			//Loop through the risks adding additional elements
			foreach (XmlNode xmlRiskNode in xmlHistoryListDoc.ChildNodes[0].ChildNodes)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}


			}

			//Localize any dates in the document
			LocalizeDates(xmlHistoryListDoc, timezoneId);

			return xmlHistoryListDoc;
		}

		protected XmlDocument Get_UserAuditHistoryDetails(int userId, int projectId, int projectTemplateId, string reportFormatToken, List<string> reportElements, Hashtable filters, Hashtable sorts, string timezoneId, UpdateBackgroundProcessStatus updateBackgroundProcessStatus, int progress)
		{
			//Get the offset from the timezone
			double utcOffset = 0;
			if (!String.IsNullOrEmpty(timezoneId))
			{
				TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
				if (timeZone != null)
				{
					utcOffset = timeZone.GetUtcOffset(DateTime.Now).TotalHours;
				}
			}

			//Get the custom property and component definitions for this project
			CustomPropertyManager customPropertyManager = new CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Risk, true, false, true);

			//Extract the sort field and direction (we only support one sort at present)
			string sortField = "ChangeSetId";
			bool sortAscending = true;
			if (sorts != null)
			{
				foreach (DictionaryEntry sortEntry in sorts)
				{
					sortField = (string)sortEntry.Key;
					sortAscending = ((string)sortEntry.Value == "asc");
					break;
				}
			}

			List<long> changesetids = new List<long>();

			foreach (DictionaryEntry i in filters)
			{
				string data = i.Value.ToString();
				string[] list = data.Split(',');
				foreach (string l in list)
				{
					long id; long.TryParse(l, out id);
					changesetids.Add(id);
				}
			}

			//Get the list of all risks
			UserAuditManager userAuditManager = new UserAuditManager();

			List<UserHistoryChangeSetResponse> userHistoryChangeSets = new List<UserHistoryChangeSetResponse>();
			if (changesetids.Count > 0)
			{
				userHistoryChangeSets = userAuditManager.RetrieveUserHistoryChangeSets(utcOffset, "", sortAscending, null, 1, -1)
			.Where(t => changesetids.Contains(t.ChangeSetId)).ToList();
			}
			else
			{
				userHistoryChangeSets = userAuditManager.RetrieveUserHistoryChangeSets(utcOffset, "", sortAscending, null, 1, -1).ToList();
			}


			for (int index = 0; index <= userHistoryChangeSets.Count; index += PAGINATION_SIZE)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

			}


			//Now get the risk list XML node list
			XmlDocument xmlHistoryListDoc = new XmlDocument();
			XmlNode xmlHistoryData = xmlHistoryListDoc.CreateElement("UserHistoryData");
			xmlHistoryListDoc.AppendChild(xmlHistoryData);

			//Iterate through each entity in the collection and serialize
			foreach (UserHistoryChangeSetResponse history in userHistoryChangeSets)
			{
				XmlNode xmlHistoryNode = xmlHistoryListDoc.CreateElement("UserHistory");
				xmlHistoryNode.InnerXml = history.InnerXml;
				xmlHistoryData.AppendChild(xmlHistoryNode);
			}

			//Loop through the risks adding additional elements
			foreach (XmlNode xmlRiskNode in xmlHistoryListDoc.ChildNodes[0].ChildNodes)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}


			}

			//Localize any dates in the document
			LocalizeDates(xmlHistoryListDoc, timezoneId);

			return xmlHistoryListDoc;
		}


		protected XmlDocument Get_SystemUsageReport(int userId, int projectId, int projectTemplateId, string reportFormatToken, List<string> reportElements, Hashtable filters, Hashtable sorts, string timezoneId, UpdateBackgroundProcessStatus updateBackgroundProcessStatus, int progress)
		{
			List<long> years = new List<long>();

			foreach (DictionaryEntry i in filters)
			{
				string data = i.Value.ToString();
				string[] list = data.Split(',');
				foreach (string l in list)
				{
					long id; long.TryParse(l, out id);
					years.Add(id);
				}
			}

			//Get the list of all risks
			UserActivityLogManager userActivityLogManager = new UserActivityLogManager();
			List<SystemUsageReportResponse> SystemUsageReports = new List<SystemUsageReportResponse>();
			if (years.Count > 0)
			{
				SystemUsageReports = userActivityLogManager.RetrieveSystemUsageReport(years.FirstOrDefault().ToString()).ToList();
			}
			else
			{
				SystemUsageReports = userActivityLogManager.RetrieveSystemUsageReport(DateTime.Now.Year.ToString()).ToList();
			}

			for (int index = 0; index <= SystemUsageReports.Count; index += PAGINATION_SIZE)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

			}


			//Now get the risk list XML node list
			XmlDocument xmlHistoryListDoc = new XmlDocument();
			XmlNode xmlHistoryData = xmlHistoryListDoc.CreateElement("SystemUsageData");
			xmlHistoryListDoc.AppendChild(xmlHistoryData);

			//Iterate through each entity in the collection and serialize
			foreach (SystemUsageReportResponse data in SystemUsageReports)
			{
				XmlNode xmlHistoryNode = xmlHistoryListDoc.CreateElement("SystemUsage");
				xmlHistoryNode.InnerXml = data.InnerXml;
				xmlHistoryData.AppendChild(xmlHistoryNode);
			}

			//Loop through the risks adding additional elements
			foreach (XmlNode xmlRiskNode in xmlHistoryListDoc.ChildNodes[0].ChildNodes)
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}


			}

			//Localize any dates in the document
			LocalizeDates(xmlHistoryListDoc, timezoneId);

			return xmlHistoryListDoc;
		}


		/// <summary>
		/// Loads and displays the tasks reports
		/// </summary>
		/// <param name="reportElements">The list of seconary elements to render</param>
		/// <param name="reportFormatToken">The token for the report format</param>
		/// <param name="filters">Any filters to apply to the data</param>
		/// <param name="sorts">Any sorts to apply to the data</param>
		/// <param name="folderId">Any folder to filter on (null = no filter, 0 = root)</param>
		protected XmlDocument Get_TaskDetails(int userId, int projectId, int projectTemplateId, string reportFormatToken, List<string> reportElements, int? folderId, Hashtable filters, Hashtable sorts, string timezoneId, UpdateBackgroundProcessStatus updateBackgroundProcessStatus, int progress)
		{
			//Get the offset from the timezone
			double utcOffset = 0;
			if (!String.IsNullOrEmpty(timezoneId))
			{
				TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
				if (timeZone != null)
				{
					utcOffset = timeZone.GetUtcOffset(DateTime.Now).TotalHours;
				}
			}

			//Get the custom property definitions for this project
			Business.CustomPropertyManager customPropertyManager = new Business.CustomPropertyManager();
			List<CustomProperty> customProperties = customPropertyManager.CustomPropertyDefinition_RetrieveForArtifactType(projectTemplateId, Artifact.ArtifactTypeEnum.Task, true, false, true);

			//Get the list of active data syncs for the project
			List<DataSyncSystem> dataMappingSystems = new DataMappingManager().RetrieveDataSyncSystemsForProject(projectId);

			//Extract the sort field and direction (we only support one sort at present)
			string sortField = "Name";
			bool sortAscending = true;
			if (sorts != null)
			{
				foreach (DictionaryEntry sortEntry in sorts)
				{
					//We can't sort on progress as it's a calculated column
					if ((string)sortEntry.Key != "ProgressId")
					{
						sortField = (string)sortEntry.Key;
						sortAscending = ((string)sortEntry.Value == "asc");
						break;
					}
				}
			}

			//Get the list of all tasks
			Business.TaskManager taskManager = new Business.TaskManager();

			//First we need to get the folder hierarchy for either the folder or all folders
			//root gets only root test cases, all other folders are recursive
			List<TaskFolderHierarchyView> taskFolders = taskManager.TaskFolder_GetList(projectId);
			if (folderId.HasValue)
			{
				if (folderId == FOLDER_ID_ROOT)
				{
					taskFolders = taskFolders.Where(t => !t.ParentTaskFolderId.HasValue).ToList();
				}
				else
				{
					TaskFolderHierarchyView matchedFolder = taskFolders.FirstOrDefault(t => t.TaskFolderId == folderId.Value);
					if (matchedFolder != null)
					{
						taskFolders = taskFolders.Where(t => HierarchicalList.IsSelfOrChildOf(t.IndentLevel, matchedFolder.IndentLevel)).ToList();
					}
				}
			}

			//Now get the task list XML node list
			XmlDocument xmlTasksDoc = new XmlDocument();
			XmlNode xmlTaskData = xmlTasksDoc.CreateElement("TaskData");
			xmlTasksDoc.AppendChild(xmlTaskData);

			//First add the root folder, unless filtering by folder (other than root)
			if (!folderId.HasValue || folderId == FOLDER_ID_ROOT)
			{
				TaskFolder taskFolder = new TaskFolder();
				taskFolder.Name = GlobalResources.General.Global_Root;
				taskFolder.TaskFolderId = 0;

				XmlNode xmlTaskFolderNode = xmlTasksDoc.CreateElement("TaskFolder");
				xmlTaskFolderNode.InnerXml = taskFolder.InnerXml;
				xmlTaskData.AppendChild(xmlTaskFolderNode);
				AddIndentLevel(xmlTaskFolderNode, "");

				XmlNode xmlTasksCollectionNode = xmlTasksDoc.CreateElement("Tasks");
				xmlTaskFolderNode.AppendChild(xmlTasksCollectionNode);

				int count = taskManager.Count(projectId, filters, utcOffset, NoneFilterValue);
				List<TaskView> tasks = new List<TaskView>();
				for (int index = 1; index < count + 1; index += PAGINATION_SIZE)
				{
					if (updateBackgroundProcessStatus != null)
					{
						updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
					}

					tasks.AddRange(taskManager.Retrieve(projectId, sortField, sortAscending, index, PAGINATION_SIZE, filters, utcOffset, NoneFilterValue, false, false));
				}

				//Clean any rich-text fields in the list
				CleanRichTextFields(tasks, "Description");

				//Iterate through each entity in the collection and serialize
				foreach (TaskView taskView in tasks)
				{
					XmlNode xmlTaskNode = xmlTasksDoc.CreateElement("Task");
					xmlTaskNode.InnerXml = taskView.InnerXml;
					xmlTasksCollectionNode.AppendChild(xmlTaskNode);
				}
			}

			//Now loop through each folder
			foreach (TaskFolderHierarchyView taskFolderHierarchyView in taskFolders)
			{
				//Retrieve the actual folder again to get all its properties
				TaskFolder taskFolder = taskManager.TaskFolder_GetById(taskFolderHierarchyView.TaskFolderId);
				if (taskFolder != null)
				{
					XmlNode xmlTaskFolderNode = xmlTasksDoc.CreateElement("TaskFolder");
					xmlTaskFolderNode.InnerXml = taskFolder.InnerXml;
					xmlTaskData.AppendChild(xmlTaskFolderNode);
					AddIndentLevel(xmlTaskFolderNode, taskFolderHierarchyView.IndentLevel);

					XmlNode xmlTasksCollectionNode = xmlTasksDoc.CreateElement("Tasks");
					xmlTaskFolderNode.AppendChild(xmlTasksCollectionNode);

					int count = taskManager.Count(projectId, filters, utcOffset, taskFolder.TaskFolderId);
					List<TaskView> tasks = new List<TaskView>();
					for (int index = 1; index < count + 1; index += PAGINATION_SIZE)
					{
						if (updateBackgroundProcessStatus != null)
						{
							updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
						}

						tasks.AddRange(taskManager.Retrieve(projectId, sortField, sortAscending, index, PAGINATION_SIZE, filters, utcOffset, taskFolder.TaskFolderId));
					}

					//Clean any rich-text fields in the list
					CleanRichTextFields(tasks, "Description");

					//Iterate through each entity in the collection and serialize
					foreach (TaskView taskView in tasks)
					{
						XmlNode xmlTaskNode = xmlTasksDoc.CreateElement("Task");
						xmlTaskNode.InnerXml = taskView.InnerXml;
						xmlTasksCollectionNode.AppendChild(xmlTaskNode);
					}
				}
			}

			//Loop through the tasks adding additional elements
			DiscussionManager discussionManager = new DiscussionManager();
			foreach (XmlNode xmlTaskNode in xmlTasksDoc.SelectNodes("/TaskData//Task[TaskId]"))
			{
				if (updateBackgroundProcessStatus != null)
				{
					updateBackgroundProcessStatus(IncrementProgress(progress), GlobalResources.Messages.Report_StartGeneratingReport);
				}

				//Get the task id
				XmlNode xmlTaskId = xmlTaskNode.SelectSingleNode("TaskId");
				if (xmlTaskId != null)
				{
					int taskId = Int32.Parse(xmlTaskId.InnerText);

					//Now depending on the report elements the user has chosen
					//insert the appropriate secondary elements as child nodes

					//Custom Properties
					ArtifactCustomProperty artifactCustomProperty = customPropertyManager.ArtifactCustomProperty_RetrieveByArtifactId(projectId, projectTemplateId, taskId, DataModel.Artifact.ArtifactTypeEnum.Task);

					//Custom properties need to be specially parsed to add them to the XML in a way that can
					//be easily handled by XSLT.
					InsertCustomProperties(projectId, taskId, DataModel.Artifact.ArtifactTypeEnum.Task, artifactCustomProperty, xmlTasksDoc, xmlTaskNode, customProperties, timezoneId, dataMappingSystems);

					//Comments
					IEnumerable<IDiscussion> comments = discussionManager.Retrieve(taskId, DataModel.Artifact.ArtifactTypeEnum.Task, false);

					//Get the discussions as XML and add to the task XML
					InsertComments(comments, "Discussion", xmlTasksDoc, xmlTaskNode, "Discussions");


					//Artifact Links
					if (reportElements.Contains("Incidents") || reportElements.Contains("Tasks"))
					{
						Business.ArtifactLinkManager artifactLinkManager = new Business.ArtifactLinkManager();
						List<ArtifactLinkView> artifactLinks = artifactLinkManager.RetrieveByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Task, taskId);

						//Remove any associations that were not wanted (incidents, tasks)
						if (artifactLinks != null)
						{
							List<ArtifactLinkView> wantedArtifactLinks = artifactLinks
								.Where(a => (a.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.Incident && reportElements.Contains("Incidents")) || (a.ArtifactTypeId == (int)DataModel.Artifact.ArtifactTypeEnum.Task && reportElements.Contains("Tasks"))).ToList();

							//Get the artifact links as XML and add to the task XML
							InsertChildEntity<ArtifactLinkView>(projectId, projectTemplateId, wantedArtifactLinks, xmlTasksDoc, xmlTaskNode, "ArtifactLinks", timezoneId, dataMappingSystems, "ArtifactLink");
						}
					}

					//Attachments
					if (reportElements.Contains("Attachments"))
					{
						Business.AttachmentManager attachmentManager = new Business.AttachmentManager();
						List<ProjectAttachmentView> attachments = attachmentManager.RetrieveByArtifactId(projectId, taskId, DataModel.Artifact.ArtifactTypeEnum.Task, null, true, 1, Int32.MaxValue, null, 0);

						//Get the attachments as XML and add to the task XML
						InsertChildEntity<ProjectAttachmentView>(projectId, projectTemplateId, attachments, xmlTasksDoc, xmlTaskNode, "Attachments", timezoneId, dataMappingSystems, "Attachment");

						//If the license supports source code we need to add these as well
						InsertSourceCodeAttachments(projectId, DataModel.Artifact.ArtifactTypeEnum.Task, taskId, xmlTasksDoc, xmlTaskNode, "Attachments");
					}

					//Source Code
					if (reportElements.Contains("SourceCode"))
					{
						InsertSourceCodeRevisions(userId, projectId, DataModel.Artifact.ArtifactTypeEnum.Task, taskId, xmlTasksDoc, xmlTaskNode, "SourceCodeRevisions");
					}

					//History
					if (reportElements.Contains("History"))
					{
						InsertArtifactHistory(projectId, projectTemplateId, taskId, Artifact.ArtifactTypeEnum.Task, utcOffset, xmlTasksDoc, xmlTaskNode, timezoneId, dataMappingSystems, customProperties);
					}
				}
			}

			//Localize any dates in the document
			LocalizeDates(xmlTasksDoc, timezoneId);

			return xmlTasksDoc;
		}

		#endregion
	}

	/// <summary>
	/// Extension methods used by the Report Manager
	/// </summary>
	/// <remarks>
	/// Once we get rid of datasets, can delete this class
	/// </remarks>
	internal static class ReportManagerUtils
	{
		/// <summary>
		/// Gets the XML from a dataset in a format expected by the report
		/// </summary>
		/// <param name="dataSet"></param>
		/// <returns>The XML string</returns>
		internal static string GetReportXml(this DataSet dataSet)
		{
			XmlDocument xmlDoc = new XmlDocument();

			//Write out the dataset element
			XmlElement xmlDataSetNode = xmlDoc.CreateElement(dataSet.DataSetName);
			xmlDoc.AppendChild(xmlDataSetNode);

			//Loop through the data tables
			foreach (DataTable dataTable in dataSet.Tables)
			{
				//Loop through the rows
				foreach (DataRow dataRow in dataTable.Rows)
				{
					//Write out the datatable element for each row
					XmlElement xmlDataRowNode = xmlDoc.CreateElement(dataTable.TableName);
					xmlDataSetNode.AppendChild(xmlDataRowNode);

					//Loop through the columns/fields
					foreach (DataColumn dataColumn in dataTable.Columns)
					{
						string fieldName = dataColumn.ColumnName;
						XmlNode xmlFieldNode = xmlDoc.CreateElement(fieldName);
						xmlDataRowNode.AppendChild(xmlFieldNode);

						//Serialize the data, for dates we need to make sure it's in the correct format
						//that will be localized by the report writer
						object fieldValue = dataRow[fieldName];
						if (fieldValue != null && fieldValue != DBNull.Value)
						{
							if (fieldValue.GetType() == typeof(DateTime))
							{
								DateTime dateTime = (DateTime)fieldValue;
								xmlFieldNode.InnerText = String.Format(Entity.FORMAT_DATE_TIME_XML, dateTime);
							}
							else if (fieldName.EndsWith("Effort") && fieldValue.GetType() == typeof(int))
							{
								//Effort fields should be displayed in hours (decimal)
								int effort = (int)fieldValue;
								decimal hours = ((decimal)effort) / 60;
								xmlFieldNode.InnerText = hours.ToString("0.00");
							}
							else if (fieldName.EndsWith("Duration") && fieldValue.GetType() == typeof(int))
							{
								//Duration fields should be displayed in hours (decimal)
								int effort = (int)fieldValue;
								decimal hours = ((decimal)effort) / 60;
								xmlFieldNode.InnerText = hours.ToString("0.00");
							}
							else
							{
								xmlFieldNode.InnerText = fieldValue.ToString();
							}
						}
					}
				}
			}

			return xmlDataSetNode.OuterXml;
		}
	}
}
