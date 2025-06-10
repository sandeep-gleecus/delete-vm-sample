<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReleasesServiceTest.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.ReleasesServiceTest" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Releases Service JsUnit Test Suite</title>
</head>
<body>
    <form id="form1" runat="server">
    <div id="divComponent"></div>
	<asp:ScriptManager ID="ajxScriptManager" runat="server">
      <Services>  
        <asp:ServiceReference Path="~/Services/Ajax/ReleasesService.svc" />  
      </Services>  
    </asp:ScriptManager>  
    <script language="JavaScript" src="/jsunit/app/jsUnitCore.js" type="text/javascript"></script>
    <script language="javascript" type="text/javascript">
        //Test Methods
        var webServiceClass = 'Inflectra.SpiraTest.Web.Services.Ajax.ReleasesService';
        var userId = 2; //Fred Bloggs
        var projectId = 1;  //Library System
        var setUpPageStatus;
        var testCount = 5;  //Number of asynchronous tests to be run
        var testIndex;
        var retrieve_data;
        var retrieveNameDesc_data;
        var retrieveLookup_data;
        var refresh_data;
        var retrievePaginationOptions_data;
        
        //Called first when the page sets-up since we're testing asynchronous Ajax calls
        function setUpPage()
        {
            //Mark the page as running
            setUpPageStatus = 'running';
            testIndex = 0;
            
            //Call all of the test components
            testComponent.retrieve();
            testComponent.retrieveNameDesc();
            testComponent.retrieveLookup();
            testComponent.refresh();
            testComponent.retrievePaginationOptions();
        } 
        
        //Test that we can retrieve some data from the hierarchical list service
        function test_retrieve()
        {
            //Make sure that we have some data
            assertTrue ('No data returned', retrieve_data.length > 0);
            
            //Now make sure that we have the fields we expect in the first row (filters)
            assertEquals('currPage', 1, retrieve_data[0].currPage);
            assertEquals('pageCount', 1, retrieve_data[0].pageCount);
            
            //Now make sure that we have the number of data rows we expect
            assertEquals ('Number rows', 14, retrieve_data.length-1);
            
            //Finally verify some of the data returned
            //Release
            assertEquals('primaryKey', 1, retrieve_data[1].primaryKey);
            assertEquals('summary', true, retrieve_data[1].summary);
            assertEquals('expanded', true, retrieve_data[1].expanded);
            assertEquals('attachment', false, retrieve_data[1].attachment);
            assertEquals('alternate', false, retrieve_data[1].alternate);
            assertEquals('indent', 1, retrieve_data[1].indent);
            assertEquals('Name', 'Library System Release 1', retrieve_data[1]['Fields']['Name'].textValue);
            assertEquals('VersionNumber', '1.0.0.0', retrieve_data[1]['Fields']['VersionNumber'].textValue);
            assertEquals('StartDate', '1-Mar-2004', retrieve_data[1]['Fields']['StartDate'].textValue);
            assertEquals('EndDate', '12-Mar-2004', retrieve_data[1]['Fields']['EndDate'].textValue);
            assertEquals('PlannedEffort', '216.0h', retrieve_data[1]['Fields']['PlannedEffort'].textValue);
            assertEquals('PlannedEffort', 216*60, retrieve_data[1]['Fields']['PlannedEffort'].intValue);
            assertEquals('TaskEstimatedEffort', '94.0h', retrieve_data[1]['Fields']['TaskEstimatedEffort'].textValue);
            assertEquals('TaskEstimatedEffort', 94*60, retrieve_data[1]['Fields']['TaskEstimatedEffort'].intValue);
            assertEquals('IterationYn', 'N', retrieve_data[1]['Fields']['IterationYn'].textValue);
            assertEquals ('CountPassed:Value', 0, retrieve_data[1]['Fields']['CountPassed'].equalizerGreen);
            assertEquals ('TaskCount:Value', 50, retrieve_data[1]['Fields']['TaskCount'].equalizerGreen);
           
            //Iteration
            assertEquals('primaryKey', 11, retrieve_data[3].primaryKey);
            assertEquals('summary', false, retrieve_data[3].summary);
            assertEquals('expanded', false, retrieve_data[3].expanded);
            assertEquals('attachment', false, retrieve_data[3].attachment);
            assertEquals('alternate', true, retrieve_data[3].alternate);
            assertEquals('indent', 3, retrieve_data[3].indent);
            assertEquals('Name', 'Iteration 001', retrieve_data[3]['Fields']['Name'].textValue);
            assertEquals('VersionNumber', '1.0.1.0.0001', retrieve_data[3]['Fields']['VersionNumber'].textValue);
            assertEquals('StartDate', '13-Mar-2004', retrieve_data[3]['Fields']['StartDate'].textValue);
            assertEquals('EndDate', '20-Mar-2004', retrieve_data[3]['Fields']['EndDate'].textValue);
            assertEquals('PlannedEffort', '80.0h', retrieve_data[3]['Fields']['PlannedEffort'].textValue);
            assertEquals('PlannedEffort', 80*60, retrieve_data[3]['Fields']['PlannedEffort'].intValue);
            assertEquals('TaskEstimatedEffort', '', retrieve_data[3]['Fields']['TaskEstimatedEffort'].textValue);
            assertEquals('TaskEstimatedEffort', -1, retrieve_data[3]['Fields']['TaskEstimatedEffort'].intValue);
            assertEquals('IterationYn', 'Y', retrieve_data[3]['Fields']['IterationYn'].textValue);
            assertEquals('CountPassed:Value', 0, retrieve_data[3]['Fields']['CountPassed'].equalizerGreen);
            assertEquals('TaskCount:Value', 0, retrieve_data[3]['Fields']['TaskCount'].equalizerGreen);
        }
        
        //Test that we can retrieve the full name and description of an artifact
        function test_retrieveNameDesc()
        {
            //Make sure that the name and description matches
            assertEquals ('Name/Desc', '<u>Library System Release 1</u><br />\nThis is the initial release of the Library Management System', retrieveNameDesc_data);
        }
        
        function test_retrieveLookup()
        {
            //Make sure that we get the list of projects minus the current project
            assertEquals ('ProjectName', 'Sample Application One', retrieveLookup_data['2']);
            assertEquals ('ProjectName', 'Sample Application Two', retrieveLookup_data['3']);
        }
        
        function test_refresh()
        {
            //Verify the artifact's standard data
            assertEquals ('primaryKey', 1, refresh_data.primaryKey);
            assertEquals ('summary', true, refresh_data.summary);
            assertEquals ('expanded', true, refresh_data.expanded);
            assertEquals ('alternate', false, refresh_data.alternate);
            assertEquals ('indent', 1, refresh_data.indent);
            
            //Now verify the fields including any lookups
            assertEquals ('Name:Value', 'Library System Release 1', refresh_data.Fields['Name'].textValue);
            assertEquals ('VersionNumber:Value', '1.0.0.0', refresh_data.Fields['VersionNumber'].textValue);
            assertEquals ('VersionNumber:Required', true, refresh_data.Fields['VersionNumber'].required);
            assertEquals ('CountPassed:Value', 0, refresh_data.Fields['CountPassed'].equalizerGreen);
            assertEquals ('CountPassed:Required', false, refresh_data.Fields['CountPassed'].required);
            assertEquals ('TaskCount:Value', 50, refresh_data.Fields['TaskCount'].equalizerGreen);
            assertEquals ('TaskCount:Required', false, refresh_data.Fields['TaskCount'].required);
            assertEquals ('StartDate', '3/1/2004', refresh_data.Fields['StartDate'].textValue);
            assertEquals ('StartDate:Required', false, refresh_data.Fields['StartDate'].required);
            assertEquals ('EndDate', '3/12/2004', refresh_data.Fields['EndDate'].textValue);
            assertEquals ('EndDate:Required', false, refresh_data.Fields['EndDate'].required);
            assertEquals ('PlannedEffort', '216.0h', refresh_data.Fields['PlannedEffort'].textValue);
            assertEquals ('PlannedEffort', 216*60, refresh_data.Fields['PlannedEffort'].intValue);
            assertEquals ('PlannedEffort:Editable', false, refresh_data.Fields['PlannedEffort'].editable);
            assertEquals ('TaskEstimatedEffort', '94.0h', refresh_data.Fields['TaskEstimatedEffort'].textValue);
            assertEquals ('TaskEstimatedEffort', 94*60, refresh_data.Fields['TaskEstimatedEffort'].intValue);
            assertEquals ('TaskEstimatedEffort:Required', false, refresh_data.Fields['TaskEstimatedEffort'].required);
            assertEquals ('IterationYn', 'N', refresh_data.Fields['IterationYn'].textValue);
            assertEquals ('IterationYn:Lookups', 'Yes', refresh_data.Fields['IterationYn'].lookups['Y']);         
            assertEquals ('IterationYn:Required', true, refresh_data.Fields['IterationYn'].required);
        }
        
        function test_retrievePaginationOptions()
        {
            //Make sure that we get the list of pagination options, including which one is currently selected
            assertEquals ('Pagination-15', 'true', retrievePaginationOptions_data['15']);
            assertEquals ('Pagination-30', 'false', retrievePaginationOptions_data['30']);
        }
        
        // The following component is used to interact with the server ajax web service methods
        
        Type.registerNamespace('Inflectra.SpiraTest.Web.Services.Ajax.TestSuite');
        Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.ReleasesServiceTest = function(element)
        {
            this._element = element;
            Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.ReleasesServiceTest.initializeBase(this, [this._element]);
        }
        Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.ReleasesServiceTest.prototype =
        {
            initialize : function()
            {
                Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.ReleasesServiceTest.callBaseMethod(this, 'initialize');
            },
            dispose : function()
            {
                Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.ReleasesServiceTest.callBaseMethod(this, 'dispose');
            },
            
            retrieve : function()
            {
                var webServiceMethod = webServiceClass + '.Retrieve';
                var webServiceParams = '(userId, projectId, Function.createDelegate(this, this.retrieve_success), Function.createDelegate(this, this.failure))';
                eval (webServiceMethod + webServiceParams);
            },
            retrieve_success : function (dataSource)
            {
                //Set the data variable
                retrieve_data = dataSource;
                //Update the count
                this.updateStatus();
            },
            
            retrieveNameDesc : function()
            {
                var artifactId = 1; //Artifact being tested
                var webServiceMethod = webServiceClass + '.RetrieveNameDesc';
                var webServiceParams = '(artifactId, Function.createDelegate(this, this.retrieveNameDesc_success), Function.createDelegate(this, this.failure))';
                eval (webServiceMethod + webServiceParams);
            },
            retrieveNameDesc_success : function (dataSource)
            {
                //Set the data variable
                retrieveNameDesc_data = dataSource;
                //Update the count
                this.updateStatus();
            },
            
            retrieveLookup : function ()
            {
                var operation = 'Project'; //Lookup being tested
                var webServiceMethod = webServiceClass + '.RetrieveLookup';
                var webServiceParams = '(userId, projectId, operation, Function.createDelegate(this, this.retrieveLookup_success), Function.createDelegate(this, this.failure))';
                eval (webServiceMethod + webServiceParams);
            },
            retrieveLookup_success : function (dataSource)
            {
                //Set the data variable
                retrieveLookup_data = dataSource;
                //Update the count
                this.updateStatus();
            },
            
            refresh : function ()
            {
                var artifactId = 1; //Artifact being tested
                var webServiceMethod = webServiceClass + '.Refresh';
                var webServiceParams = '(userId, projectId, artifactId, Function.createDelegate(this, this.refresh_success), Function.createDelegate(this, this.failure))';
                eval (webServiceMethod + webServiceParams);
            },
            refresh_success : function (dataSource)
            {
                //Set the data variable
                refresh_data = dataSource;
                //Update the count
                this.updateStatus();
            },
            
            retrievePaginationOptions : function ()
            {
                var webServiceMethod = webServiceClass + '.RetrievePaginationOptions';
                var webServiceParams = '(userId, projectId, Function.createDelegate(this, this.retrievePaginationOptions_success), Function.createDelegate(this, this.failure))';
                eval (webServiceMethod + webServiceParams);
            },
            retrievePaginationOptions_success : function (dataSource)
            {
                //Set the data variable
                retrievePaginationOptions_data = dataSource;
                //Update the count
                this.updateStatus();
            },            
            
            //Generic functions used by all test methods
            failure : function (exception)
            {
                //Log the failure message
                retrieve_data = null;
                setUpPageStatus = 'complete';
            },
            updateStatus : function ()
            {
                testIndex++;
                if (testIndex >= testCount)
                {
                    setUpPageStatus = 'complete';
                }
            }
        }
        Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.ReleasesServiceTest.registerClass('Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.ReleasesServiceTest', Sys.UI.Control);
        var testComponent = $create (Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.ReleasesServiceTest, {}, {}, {}, document.getElementById('divComponent'));
        
    </script>
    </form>
</body>
</html>
