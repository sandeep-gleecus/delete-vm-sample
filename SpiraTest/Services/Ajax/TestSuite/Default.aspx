<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Services.Ajax.TestSuite.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>SpiraTeam JSUnit Test Suite Home Page</title>
</head>
<body>
    <script language="JavaScript" src="/jsunit/app/jsUnitCore.js" type="text/javascript"></script>
    <script language="javascript" type="text/javascript">
        //Contains a list of all the test pages to execute
        function suite()
        {
            var jsSuite = new top.jsUnitTestSuite();
            //RequirementsServiceTest.aspx
            jsSuite.addTestPage('<%= Request.Url.ToString().ToLower(System.Globalization.CultureInfo.InvariantCulture).Replace("default.aspx", "RequirementsServiceTest.aspx") %>');
            //ReleasesServiceTest.aspx
            jsSuite.addTestPage('<%= Request.Url.ToString().ToLower(System.Globalization.CultureInfo.InvariantCulture).Replace("default.aspx", "ReleasesServiceTest.aspx") %>');
            //TestCaseServiceTest.aspx
            jsSuite.addTestPage('<%= Request.Url.ToString().ToLower(System.Globalization.CultureInfo.InvariantCulture).Replace("default.aspx", "TestCaseServiceTest.aspx") %>');
            //TestSetServiceTest.aspx
            jsSuite.addTestPage('<%= Request.Url.ToString().ToLower(System.Globalization.CultureInfo.InvariantCulture).Replace("default.aspx", "TestSetServiceTest.aspx") %>');
            //IncidentsServiceTest.aspx
            jsSuite.addTestPage('<%= Request.Url.ToString().ToLower(System.Globalization.CultureInfo.InvariantCulture).Replace("default.aspx", "IncidentsServiceTest.aspx") %>');
            //TasksServiceTest.aspx
            jsSuite.addTestPage('<%= Request.Url.ToString().ToLower(System.Globalization.CultureInfo.InvariantCulture).Replace("default.aspx", "TasksServiceTest.aspx") %>');
            return jsSuite;
        }
    </script>
    <form id="form1" runat="server">
    </form>
</body>
</html>
