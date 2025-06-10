using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    /// <summary>
    /// The test runs pending
    /// </summary>
    [DataContract(Namespace = "tst.dataObjects", Name="TestRunsPending")]
    public class TestRunsPendingModel
    {
        [DataMember]
        public int TestRunsPendingId;

        [DataMember(EmitDefaultValue = false)]
        public int? ReleaseId;

        [DataMember(EmitDefaultValue = false)]
        public string ReleaseVersion;

        [DataMember]
        public int ProjectId;

        [DataMember(EmitDefaultValue = false)]
        public int? TestSetId;
        [DataMember]
        public string Name;

        [DataMember(EmitDefaultValue = false)]
        public int? CountPassed;

        [DataMember(EmitDefaultValue = false)]
        public int? CountFailed;

        [DataMember(EmitDefaultValue = false)]
        public int? CountBlocked;

        [DataMember(EmitDefaultValue = false)]
        public int? CountCaution;

        [DataMember(EmitDefaultValue = false)]
        public int? CountNotRun;

        [DataMember(EmitDefaultValue = false)]
        public int? CountNotApplicable;

        [DataMember(EmitDefaultValue = false)]
        public List<UserSettings> Settings;

        [DataMember(EmitDefaultValue = false)]
        public List<UserSettingsExploratory> SettingsExploratory;

        [DataMember(Name = "TestRun")]
        public List<TestRunModel> TestRuns;
    }

    [DataContract(Namespace = "tst.dataObjects")]
    public class UserSettings
    {
        [DataMember]
        public int? CurrentTestRunId;

        [DataMember]
        public int? CurrentTestRunStepId;

        [DataMember]
        public int? DisplayModeMain;

        [DataMember]
        public int? DisplayModeSub;

        [DataMember]
        public bool AlwaysShowTestRun;

        [DataMember]
        public bool ShowCustomProperties;

        [DataMember]
        public bool GuidedTourSeen;
    }

    [DataContract(Namespace = "tst.dataObjects")]
    public class UserSettingsExploratory
    {
        //set these to default true so that they are shown the first time a user goes to the page
        public UserSettingsExploratory()
        {
            ShowCaseDescription = true;
            ShowExpectedResult = true;
            ShowSampleData = true;

        }

        [DataMember]
        public int? CurrentTestRunId;

        [DataMember]
        public int? CurrentTestRunStepId;

        [DataMember]
        public bool ShowCaseDescription;

        [DataMember]
        public bool ShowExpectedResult;

        [DataMember]
        public bool ShowSampleData;

        [DataMember]
        public bool ShowCustomProperties;

        [DataMember]
        public bool ShowLastResult;

        [DataMember]
        public bool GuidedTourSeen;
    }

    [DataContract(Namespace = "tst.dataObjects")]
    public class TestRunModel
    {
        [DataMember]
        public int TestRunId;

        [DataMember]
        public int TestCaseId;

        [DataMember(EmitDefaultValue = false)]
        public int? ReleaseId;

        [DataMember]
        public int TesterId;

        [DataMember]
        public string Name;

        [DataMember]
        public string Description;

        [DataMember]
        public int? ExecutionStatusId;

        [DataMember(EmitDefaultValue = false)]
        public System.DateTime? StartDate;

        [DataMember(EmitDefaultValue = false)]
        public System.DateTime? EndDate;

        [DataMember(EmitDefaultValue = false)]
        public int? ActualDuration;

        [DataMember(Name = "TestRunStep")]
        public List<TestRunStepModel> TestRunSteps;

        //only for use in exploratory testing
        [DataMember(EmitDefaultValue = false)]
        public int? LastTestRunId;

        [DataMember(EmitDefaultValue = false)]
        public int? LastReleaseId;

        [DataMember(EmitDefaultValue = false)]
        public string LastReleaseVersion;
    }

    [DataContract(Namespace = "tst.dataObjects")]
    public class TestRunStepModel
    {
        [DataMember(EmitDefaultValue = false)]
        public int? TestRunStepId;

        [DataMember(EmitDefaultValue = false)]
        public int? TestStepId;

        [DataMember(EmitDefaultValue = false)]
        public int? TestCaseId;
        
        [DataMember(EmitDefaultValue = false)]
        public string Description;

        [DataMember]
        public int Position;

        [DataMember(EmitDefaultValue = false)]
        public string ExpectedResult;

        [DataMember(EmitDefaultValue = false)]
        public string SampleData;

        [DataMember(EmitDefaultValue = false)]
        public string ActualResult;

        [DataMember]
        public int? ExecutionStatusId;

        [DataMember(EmitDefaultValue = false)]
        public System.DateTime? StartDate;

        [DataMember(EmitDefaultValue = false)]
        public System.DateTime? EndDate;

        [DataMember(EmitDefaultValue = false)]
        public int? ActualDuration;

        //only for use in exploratory testing
        [DataMember(EmitDefaultValue = false)]
        public int? LastTestRunStepId;

        [DataMember(EmitDefaultValue = false)]
        public string LastActualResult;

        [DataMember(EmitDefaultValue = false)]
        public int? LastExecutionStatusId;

        [DataMember(EmitDefaultValue = false)]
        public System.DateTime? LastEndDate;
    }
}