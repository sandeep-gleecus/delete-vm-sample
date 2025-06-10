using System.Collections.Generic;
using System.Data;
using System.Linq;
using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Business
{
	public class ReportingTemplateManager : ManagerBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Business.ReportingTemplateManager::";

		/// <summary>
		///	Constructor method for class.
		/// </summary>
		public ReportingTemplateManager()
		{
		}

		public static IQueryable<Template> GetAllTemplates()
		{
			using (var context = new SpiraTestEntitiesEx())
			{
				return context.Templates.ToList().AsQueryable();
			}
		}

		public static Template GetTemplateById(int Id)
		{
			using (var context = new SpiraTestEntitiesEx())
			{
				return context.Templates.Include(x => x.TST_TEMPLATE_PARAMETER).Include(x => x.TST_TEMPLATE_OUTTYPE).Include(x => x.TST_TEMPLATE_DATASOURCE).Include(x => x.TST_TEMPLATE_DATASOURCE).Where(x => x.TemplateId == Id).FirstOrDefault();
			}
		}


		public static void Create(Template newTemplate)
		{
			using (var context = new SpiraTestEntitiesEx())
			{
				context.Templates.AddObject(newTemplate);
				context.SaveChanges();
			}
		}

		public static void Update(Template newTemplate)
		{
			using (var context = new SpiraTestEntitiesEx())
			{
				context.Templates.Attach(newTemplate);

				context.SaveChanges();
			}
		}

		public static void Delete(int templateId)
		{
			using (var context = new SpiraTestEntitiesEx())
			{
				Template template = context.Templates.Where(x => x.TemplateId == templateId).FirstOrDefault();

				if (template.TST_TEMPLATE_DATASOURCE.Count > 0)
				{
					foreach (TemplateDataSource datasource in template.TST_TEMPLATE_DATASOURCE.ToList())
					{
						context.TemplateDataSources.DeleteObject(datasource);
					}
				}

				if (template.TST_TEMPLATE_PARAMETER.Count > 0)
				{
					foreach (TemplateParameter parameter in template.TST_TEMPLATE_PARAMETER.ToList())
					{
						context.TemplateParameters.DeleteObject(parameter);
					}
				}

				if (template.TST_TEMPLATE_OUTTYPE.Count > 0)
				{
					foreach (TemplateOutputType outputType in template.TST_TEMPLATE_OUTTYPE.ToList())
					{
						context.TemplateOutputTypes.DeleteObject(outputType);
					}
				}

				context.Templates.DeleteObject(template);
				context.SaveChanges();
			}

		}

		public static IList<TestRun> GetTestRuns()
		{
			using (var context = new SpiraTestEntitiesEx())
			{
				return context.TestRuns.OrderBy(x => x.Name).ToList();
			}
		}

		public static IList<TestSet> GetTestSets()
		{
			using (var context = new SpiraTestEntitiesEx())
			{
				return context.TestSets.OrderBy(x => x.Name).ToList();
			}
		}

		public static IList<TestCase> GetTestCases()
		{
			using (var context = new SpiraTestEntitiesEx())
			{
				return context.TestCases.OrderBy(x => x.Name).ToList();
			}
		}

		public static IList<TestCase> LikeTestCases(string partialText)
		{
			using (var context = new SpiraTestEntitiesEx())
			{
				var testCases = context.TestCases.Where(x => x.Name.Contains(partialText) && !x.IsDeleted);
				if (int.TryParse(partialText, out var i))
				{
					var TestCasesById = context.TestCases.Where(x => x.TestCaseId == i).OrderBy(x => x.Name);
					var unionResult = testCases.Union(TestCasesById);
					return unionResult.ToList();
				}
				return testCases.ToList();
			}
		}

		public static IList<TestRun> LikeTestRuns(string partialText)
		{
			using (var context = new SpiraTestEntitiesEx())
			{
				var testRuns = context.TestRuns.Where(x => x.Name.Contains(partialText) && !x.IS_DELETED);
				if (int.TryParse(partialText, out var i))
				{
					var TestRunsById = context.TestRuns.Where(x => x.TestRunId == i).OrderBy(x => x.Name);
					var unionResult = testRuns.Union(TestRunsById);
					return unionResult.ToList();
				}
				return testRuns.ToList();
			}
		}

		public static IList<TestSet> LikeTestSets(string partialText)
		{
			using (var context = new SpiraTestEntitiesEx())
			{
				var testSets = context.TestSets.Where(x => x.Name.Contains(partialText) && !x.IsDeleted);
				if (int.TryParse(partialText, out var i))
				{
					var TestSetsById = context.TestSets.Where(x => x.TestSetId == i).OrderBy(x => x.Name);
					var unionResult = testSets.Union(TestSetsById);
					return unionResult.ToList();
				}
				return testSets.ToList();
			}
		}

		public static IList<Project> LikeProjects(string partialText)
		{
			using (var context = new SpiraTestEntitiesEx())
			{
				var projects = context.Projects.Where(x => x.Name.Contains(partialText) && x.IsActive);
				if (int.TryParse(partialText, out var i))
				{
					var ProjectsById = context.Projects.Where(x => x.ProjectId == i).OrderBy(x => x.Name);
					var unionResult = projects.Union(ProjectsById);
					return unionResult.ToList();
				}
				return projects.ToList();
			}
		}

		public static IList<Release> LikeReleases(string partialText)
		{
			using (var context = new SpiraTestEntitiesEx())
			{
				var releases = context.Releases.Where(x => x.Name.Contains(partialText));
				if (int.TryParse(partialText, out var i))
				{
					var ProjectsById = context.Releases.Where(x => x.ReleaseId == i).OrderBy(x => x.Name);
					var unionResult = releases.Union(ProjectsById);
					return unionResult.ToList();
				}
				return releases.ToList();
			}
		}

		public static IList<Incident> GetIncidents()
		{
			using (var context = new SpiraTestEntitiesEx())
			{
				return context.Incidents.OrderBy(x => x.Name).ToList();
			}
		}

		public static IList<Project> GetProjects()
		{
			using (var context = new SpiraTestEntitiesEx())
			{
				return context.Projects.OrderBy(x => x.Name).ToList();
			}
		}

		public static void AddSchedule(Schedule schedule)
		{
			using (var context = new SpiraTestEntitiesEx())
			{
				context.Schedules.AddObject(schedule);
				context.SaveChanges();
			}
		}

		public List<Template> RetrieveTemplates()
		{
			const string METHOD_NAME = "RetrieveTemplates";

			try
			{
				List<Template> reportTemplates;
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.Templates
								select r;

					reportTemplates = query.ToList();
				}

				return reportTemplates;
			}
			catch (System.Exception exception)
			{
				throw;
			}
		}

		public List<Template> RetrieveActiveTemplates()
		{
			const string METHOD_NAME = "RetrieveActiveTemplates";

			try
			{
				List<Template> reportTemplates;
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.Templates
								where r.Active == true
								select r;

					reportTemplates = query.ToList();
				}

				return reportTemplates;
			}
			catch (System.Exception exception)
			{
				throw;
			}
		}

		public List<Template> RetrieveTemplatesByCategories()
		{
			const string METHOD_NAME = "RetrieveTemplatesByCategories";

			try
			{
				List<Template> reportTemplates;
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.Templates
								where r.CategoryGroup > 0
								select r;

					reportTemplates = query.ToList();
				}

				return reportTemplates;
			}
			catch (System.Exception exception)
			{
				throw;
			}
		}

		public List<Template> RetrieveTemplatesByTemplateId(int templateId)
		{
			const string METHOD_NAME = "RetrieveTemplatesByTemplateId";

			try
			{
				List<Template> reportTemplates;
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.Templates
								where r.TemplateId == templateId
								select r;

					reportTemplates = query.ToList();
				}

				return reportTemplates;
			}
			catch (System.Exception exception)
			{
				throw;
			}
		}

		public List<Template> RetrieveTemplatesByCategoryId(int categoryId)
		{
			const string METHOD_NAME = "RetrieveTemplatesByCategories";

			try
			{
				List<Template> reportTemplates;
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.Templates
								where r.CategoryGroup == categoryId	
								select r;

					reportTemplates = query.ToList();
				}

				return reportTemplates;
			}
			catch (System.Exception exception)
			{
				throw;
			}
		}

		public List<ViewEnterpriseReportResponse> RetrieveReports(int? userId, int? projectId)
		{
			const string METHOD_NAME = "RetrieveReports";

			try
			{
				List<ViewEnterpriseReportResponse> reportTemplates;
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					var query1 = from t in context.ReportDownloadable
								 join r in context.ReportCategories
								on t.REPORT_CATEGORYID equals r.ReportCategoryId
								 join u in context.Users
								 on t.REPORT_OWNERID equals u.UserId
								 join u1 in context.Users
								 on t.CREATED_BY equals u1.UserId
								 select new ViewEnterpriseReportResponse
								 {
									 REPORT_NUMBER = t.REPORT_NUMBER,
									 REPORT_NAME = t.REPORT_NAME,
									 REPORT_CATEGORY = r.Name,
									 APPROVED = t.APPROVED,
									 REPORT_OWNER = u.UserName,
									 EFFECTIVE_DATE = t.EFFECTIVE_DATE,
									 APPROVAL_DATE = t.APPROVAL_DATE,
									 CREATED_BY = u1.UserName,
									 CREATED_DATE = t.CREATED_DATE
								 };

					reportTemplates = query1.ToList();

					//Query for the report data
					var query = from t in context.Templates
								where t.CategoryGroup > 0
								select new ViewEnterpriseReportResponse
								{
									REPORT_NUMBER = t.TemplateName,
									REPORT_NAME = t.TemplateLocation,
									REPORT_CATEGORY = null,
									APPROVED = (bool)t.Active,
									REPORT_OWNER = null,
									EFFECTIVE_DATE = null,
									APPROVAL_DATE = null,
									CREATED_BY = null,
									CREATED_DATE = t.CreationDate
								};

					List<ViewEnterpriseReportResponse> reportTemplates1 = query.ToList();

					foreach (var c in reportTemplates1)
					{
						c.REPORT_NAME = System.IO.Path.GetFileName(c.REPORT_NAME).ToString();
					}

					reportTemplates = reportTemplates.Concat(reportTemplates1).ToList();

					var query2 = from t in context.SavedReportsView
								 select new ViewEnterpriseReportResponse
								 {
									 REPORT_NUMBER = t.Name,
									 REPORT_NAME = t.Name,
									 REPORT_CATEGORY = null,
									 APPROVED = t.IsShared,
									 REPORT_OWNER = null,
									 EFFECTIVE_DATE = null,
									 APPROVAL_DATE = null,
									 CREATED_BY = null,
									 CREATED_DATE = System.DateTime.Now
								 };

					List<ViewEnterpriseReportResponse> reportTemplates2 = query2.ToList();

					reportTemplates = reportTemplates.Concat(reportTemplates2).ToList();
				}

				return reportTemplates;
			}
			catch (System.Exception exception)
			{
				throw;
			}
		}

		public List<TST_REPORT_DOWNLOADABLE> RetrieveDownloadableFiles(int categoryId)
		{
			const string METHOD_NAME = "RetrieveDownloadableFiles";

			try
			{
				List<TST_REPORT_DOWNLOADABLE> retrieveDownloadableFiles;
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.ReportDownloadable
								where r.REPORT_CATEGORYID == categoryId
								select r;

					retrieveDownloadableFiles = query.ToList();
				}

				return retrieveDownloadableFiles;
			}
			catch (System.Exception exception)
			{
				throw;
			}
		}

		public TST_REPORT_DOWNLOADABLE RetrieveDownloadableFileById(int id)
		{
			const string METHOD_NAME = "RetrieveDownloadableFileById";

			try
			{
				TST_REPORT_DOWNLOADABLE retrieveDownloadableFiles;
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.ReportDownloadable
								where r.DOWNLOADABLE_REPORT_ID == id
								select r;

					retrieveDownloadableFiles = query.FirstOrDefault();
				}

				return retrieveDownloadableFiles;
			}
			catch (System.Exception exception)
			{
				throw;
			}
		}

		public TST_REPORT_DOWNLOADABLE RetrieveDownloadableFileByName(string name)
		{
			const string METHOD_NAME = "RetrieveDownloadableFileByName";

			try
			{
				TST_REPORT_DOWNLOADABLE retrieveDownloadableFiles;
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					//Query for the report data
					var query = from r in context.ReportDownloadable
								where r.REPORT_NUMBER == name
								select r;

					retrieveDownloadableFiles = query.FirstOrDefault();
				}

				return retrieveDownloadableFiles;
			}
			catch (System.Exception exception)
			{
				throw;
			}
		}

		public List<ViewEnterpriseReportResponse> RetrieveDownloadableReports(int categoryId)
		{
			const string METHOD_NAME = "RetrieveReports";

			try
			{
				List<ViewEnterpriseReportResponse> reportTemplates;
				using (SpiraTestEntities context = new SpiraTestEntities())
				{
					var query1 = from t in context.ReportDownloadable
								 join r in context.ReportCategories
								on t.REPORT_CATEGORYID equals r.ReportCategoryId
								 join u in context.Users
								 on t.REPORT_OWNERID equals u.UserId
								 join u1 in context.Users
								 on t.CREATED_BY equals u1.UserId
								 where t.REPORT_CATEGORYID == categoryId
								 select new ViewEnterpriseReportResponse
								 {
									 DOWNLOADABLE_REPORT_ID = t.DOWNLOADABLE_REPORT_ID,
									 REPORT_NUMBER = t.REPORT_NUMBER,
									 REPORT_NAME = t.REPORT_NAME,
									 REPORT_CATEGORY = r.Name,
									 APPROVED = t.APPROVED,
									 REPORT_OWNER = u.UserName,
									 EFFECTIVE_DATE = t.EFFECTIVE_DATE,
									 APPROVAL_DATE = t.APPROVAL_DATE,
									 CREATED_BY = u1.UserName,
									 CREATED_DATE = t.CREATED_DATE
								 };

					reportTemplates = query1.ToList();

				}

				return reportTemplates;
			}
			catch (System.Exception exception)
			{
				throw;
			}
		}

		public void Delete_DownloadableReport(int downloadableReportId)
		{
			const string METHOD_NAME = "Delete_DownloadableReport";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Retrieve the downloadableReport
					var query = from w in context.ReportDownloadable
								where w.DOWNLOADABLE_REPORT_ID == downloadableReportId
								select w;

					TST_REPORT_DOWNLOADABLE downloadableReport = query.FirstOrDefault();

					//Make sure we have a downloadableReport
					if (downloadableReport != null)
					{
						//Delete the downloadableReport. The database cascades will handle the dependent entities
						context.ReportDownloadable.DeleteObject(downloadableReport);
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

	}
}
