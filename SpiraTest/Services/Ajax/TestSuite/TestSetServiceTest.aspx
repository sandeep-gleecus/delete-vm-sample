<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestSetServiceTest.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TestSetServiceTest" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Test Set Service JsUnit Test Suite</title>
</head>
<body>
    <form id="form1" runat="server">
    <div id="divComponent"></div>
	<asp:ScriptManager ID="ajxScriptManager" runat="server">
      <Services>  
        <asp:ServiceReference Path="~/Services/Ajax/TestSetService.svc" />  
      </Services>  
    </asp:ScriptManager>  
    <script language="JavaScript" src="/jsunit/app/jsUnitCore.js" type="text/javascript"></script>
    <script language="javascript" type="text/javascript">
        //Test Methods
        var webServiceClass = 'Inflectra.SpiraTest.Web.Services.Ajax.TestSetService';
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
            assertEquals ('Number rows', 8, retrieve_data.length-1);
            
            //Finally verify some of the data returned
            //Test Set Folder
            assertEquals('primaryKey', 8, retrieve_data[1].primaryKey);
            assertEquals('summary', true, retrieve_data[1].summary);
            assertEquals('expanded', true, retrieve_data[1].expanded);
            assertEquals('attachment', false, retrieve_data[1].attachment);
            assertEquals('alternate', false, retrieve_data[1].alternate);
            assertEquals('indent', 1, retrieve_data[1].indent);
            assertEquals('Name', 'Functional Test Sets', retrieve_data[1]['Fields']['Name'].textValue);
            assertEquals('TestSetStatusId', 2, retrieve_data[1]['Fields']['TestSetStatusId'].intValue);
            assertEquals('PlannedDate', '5-Feb-2007', retrieve_data[1]['Fields']['PlannedDate'].textValue);
            assertEquals('CountNotRun', '', retrieve_data[1]['Fields']['CountNotRun'].textValue);
           
            //Test Set
            assertEquals('primaryKey', 1, retrieve_data[2].primaryKey);
            assertEquals('summary', false, retrieve_data[2].summary);
            assertEquals('expanded', false, retrieve_data[2].expanded);
            assertEquals('attachment', false, retrieve_data[2].attachment);
            assertEquals('alternate', false, retrieve_data[2].alternate);
            assertEquals('indent', 2, retrieve_data[2].indent);
            assertEquals('Name', 'Testing Cycle for Release 1.0', retrieve_data[2]['Fields']['Name'].textValue);
            assertEquals('TestSetStatusId', 2, retrieve_data[2]['Fields']['TestSetStatusId'].intValue);
            assertEquals('OwnerId', 3, retrieve_data[2]['Fields']['OwnerId'].intValue);
            assertEquals('ExecutionDate', '1-Dec-2003', retrieve_data[2]['Fields']['ExecutionDate'].textValue);
            assertEquals('PlannedDate', '5-Feb-2007', retrieve_data[2]['Fields']['PlannedDate'].textValue);
            assertEquals('CountNotRun', 0, retrieve_data[1]['Fields']['CountNotRun'].equalizerGreen);
            assertEquals('CountNotRun', 0, retrieve_data[1]['Fields']['CountNotRun'].equalizerRed);
            assertEquals('CountNotRun', 0, retrieve_data[1]['Fields']['CountNotRun'].equalizerOrange);
            assertEquals('CountNotRun', 0, retrieve_data[1]['Fields']['CountNotRun'].equalizerYellow);
            assertEquals('CountNotRun', 0, retrieve_data[1]['Fields']['CountNotRun'].equalizerGray);
        }
        
        //Test that we can retrieve the full name and description of an artifact
        function test_retrieveNameDesc()
        {
            //Make sure that the name and description matches
            assertEquals ('Name/Desc', '<u>Testing Cycle for Release 1.0</u><br />\nThis tests the functionality introduced in release 1.0 of the library system', retrieveNameDesc_data);
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
            assertEquals ('summary', false, refresh_data.summary);
            assertEquals ('expanded', false, refresh_data.expanded);
            assertEquals ('alternate', false, refresh_data.alternate);
            assertEquals ('indent', 2, refresh_data.indent);
            
            //Now verify the fields including any lookups
            assertEquals('Name:Value', 'Testing Cycle for Release 1.0', refresh_data.Fields['Name'].textValue);
            assertEquals('PlannedDate:Value', '2/5/2007', refresh_data.Fields['PlannedDate'].textValue);
            assertEquals('PlannedDate:Required', false, refresh_data.Fields['PlannedDate'].required);
            assertEquals('ExecutionDate:Editable', false, refresh_data.Fields['ExecutionDate'].editable);
            assertEquals('OwnerId:Value', 3, refresh_data.Fields['OwnerId'].intValue);
            assertEquals('OwnerId:Required', false, refresh_data.Fields['OwnerId'].required);
            assertEquals('OwnerId:Lookups', 'Fred Bloggs', refresh_data.Fields['OwnerId'].lookups['2']);
            assertEquals('TestSetStatusId:Value', 2, refresh_data.Fields['TestSetStatusId'].intValue);
            assertEquals('TestSetStatusId:Required', true, refresh_data.Fields['TestSetStatusId'].required);
            assertEquals('TestSetStatusId:Lookups1', 'Not Started', refresh_data.Fields['TestSetStatusId'].lookups['1']);
            assertEquals('TestSetStatusId:Lookups2', 'In Progress', refresh_data.Fields['TestSetStatusId'].lookups['2']);
        }
        
        function test_retrievePaginationOptions()
        {
            //Make sure that we get the list of pagination options, including which one is currently selected
            assertEquals ('Pagination-15', 'true', retrievePaginationOptions_data['15']);
            assertEquals ('Pagination-30', 'false', retrievePaginationOptions_data['30']);
        }
        
        // The following component is used to interact with the server ajax web service methods
        
        Type.registerNamespace('Inflectra.SpiraTest.Web.Services.Ajax.TestSuite');
        Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TestCaseServiceTest = function(element)
        {
            this._element = element;
            Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TestCaseServiceTest.initializeBase(this, [this._element]);
        }
        Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TestCaseServiceTest.prototype =
        {
            initialize : function()
            {
                Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TestCaseServiceTest.callBaseMethod(this, 'initialize');
            },
            dispose : function()
            {
                Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TestCaseServiceTest.callBaseMethod(this, 'dispose');
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
        Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TestCaseServiceTest.registerClass('Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TestCaseServiceTest', Sys.UI.Control);
        var testComponent = $create (Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.TestCaseServiceTest, {}, {}, {}, document.getElementById('divComponent'));
        
    </script>
    </form>
</body>
</html>
