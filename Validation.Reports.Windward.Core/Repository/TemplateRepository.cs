using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validation.Reports.Windward.Core.DbSet;
using Validation.Reports.Windward.Core.Model;

namespace Validation.Reports.Windward.Core.Repository
{
    public class TemplateRepository
    {
        private TemplateDbSet db = new TemplateDbSet();

        public IQueryable<Template> GetAllTemplates()
        {
            return db.Templates;
                //.Include("TemplateDataSource")
                //.Include("TemplateOutputType")
                //.Include("TemplateParameter");

        }

        public Template GetTemplateById(int Id)
        {
            return db.Templates.Where(x => x.TemplateId == Id).FirstOrDefault();
        }

        public void Create(Template newTemplate)
        {
            db.Templates.Add(newTemplate);
            db.SaveChanges();

            int templateId = newTemplate.TemplateId; 

        }

        public void Update(Template template)
        {
            db.Entry(template).State = EntityState.Modified;
            db.SaveChanges();
        }

        public void Delete(int templateId)
        {
            Template template = db.Templates.Find(templateId);

            if (template.DataSources.Count > 0)
            {
                foreach (TemplateDataSource datasource in template.DataSources.ToList())
                {
                    db.DataSources.Remove(datasource);
                    db.SaveChanges();
                }
            }

            if (template.Parameters.Count > 0)
            {
                foreach (TemplateParameter parameter in template.Parameters.ToList())
                {
                    db.Parameters.Remove(parameter);
                    db.SaveChanges();
                }
            }

            if (template.OutputTypes.Count > 0)
            {
                foreach (TemplateOutputType outputType in template.OutputTypes.ToList())
                {
                    db.OutputTypes.Remove(outputType);
                    db.SaveChanges();
                }
            }

            db.Templates.Remove(template);
            db.SaveChanges();
        }
    }

    public class ReportingRepository
    {
        private ReportingDbSet db = new ReportingDbSet();

        public IQueryable<Project> GetAllProjects()
        {
            return db.Projects.OrderBy(x => x.NAME);
        }

        public IQueryable<TestCase> GetAllTestCases()
        {
            return db.TestCases.OrderBy(x => x.NAME);
        }

        public IQueryable<TestRun> GetAllTestRuns()
        {
            return db.TestRuns.OrderBy(x => x.NAME);
        }

        public IQueryable<TestSet> GetAllTestSets()
        {
            return db.TestSets.OrderBy(x => x.NAME);
        }

        public IQueryable<TestCase> LikeTestCases(string partialText)
        {
            var testCases = db.TestCases.Where(x => x.NAME.Contains(partialText) && !x.IS_DELETED);

            if (int.TryParse(partialText, out var i))
            {
                var TestCasesById = db.TestCases.Where(x => x.TEST_CASE_ID == i).OrderBy(x => x.NAME);
                var unionResult = testCases.Union(TestCasesById);
                return unionResult;
            }
            return testCases;
        }

        public IQueryable<Incident> LikeIncidents(string partialText)
        {
            var testIncidents = db.Incidents.Where(x => x.NAME.Contains(partialText) && !x.IS_DELETED);
            if (int.TryParse(partialText, out var i))
            {
                var incidentById = db.Incidents.Where(x => x.INCIDENT_ID == i).OrderBy(x => x.NAME);
                var unionResult = testIncidents.Union(incidentById);
                return unionResult;
            }
            return testIncidents;
        }

        public IQueryable<TestRun> LikeTestRuns(string partialText)
        {
            var testRuns = db.TestRuns.Where(x => x.NAME.Contains(partialText));
            if (!int.TryParse(partialText, out var i))
                return testRuns;

            var testRunsById = db.TestRuns.Where(x => x.TEST_RUN_ID == i).OrderBy(x => x.NAME);
            var unionResult = testRuns.Union(testRunsById);
            return unionResult;
        }

        public IQueryable<TestSet> LikeTestSets(string partialText)
        {
            var testSets = db.TestSets.Where(x => x.NAME.Contains(partialText) && !x.IS_DELETED);

            if (int.TryParse(partialText, out var i))
            {
                var testSetsById = db.TestSets.Where(x => x.TEST_SET_ID == i).OrderBy(x => x.NAME);
                var unionResult = testSets.Union(testSetsById);
                return unionResult;
            }
            return testSets;
        }

        public IQueryable<Project> LikeProjects(string partialText)
        {
            var projects = db.Projects.Where(x => x.NAME.Contains(partialText) && x.IS_ACTIVE);
            if (int.TryParse(partialText, out var i))
            {
                var ProjectsById = db.Projects.Where(x => x.PROJECT_ID == i).OrderBy(x => x.NAME);
                var unionResult = projects.Union(ProjectsById);
                return unionResult;
            }
            return projects;
        }
    }
}
