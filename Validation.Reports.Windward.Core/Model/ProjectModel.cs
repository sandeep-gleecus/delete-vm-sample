using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Validation.Reports.Windward.Core.DbSet;
using Validation.Reports.Windward.Core.Model;

namespace Validation.Reports.Windward.Models
{
    public class ProjectModel
    {
        private ReportingDbSet db = new ReportingDbSet();
        private List<Project> _projects;

        public ProjectModel()
        {
            LoadProjects();
        }

        public void LoadProjects()
        {
            _projects = db.Projects.ToList();
        }

        public List<Project> GetAllProjects
        {
            get { return _projects.OrderBy(x => x.NAME).ToList<Project>(); }
        }


        //public List<Project> GetAllProjects()
        //{
        //    var projList = db.Projects.OrderBy(x => x.NAME).ToList();
        //    return projList;
        //}

    }
    public class TestCaseModel
    {
        private ReportingDbSet db = new ReportingDbSet();

        private List<TestCase> _testCases;

        public TestCaseModel()
        {
            LoadTestCases();
        }

        public void LoadTestCases()
        {
            _testCases = db.TestCases.ToList();
        }

        public List<TestCase> GetAllTestCases
        {
            get { return _testCases.OrderBy(x => x.NAME).ToList<TestCase>(); }
        }


        //public List<TestCase> GetAllTestCases()
        //{
        //    var testCaseList = db.TestCases.OrderBy(x => x.NAME).ToList();
        //    return testCaseList;
        //}
    }
}