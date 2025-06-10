using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Text;
using System.Xml;

using Inflectra.SpiraTest.Business;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Web.ServerControls;
using Inflectra.SpiraTest.Web.Classes;
using System.Threading;

namespace Inflectra.SpiraTest.Web
{
	/// <summary>
	/// This webform code-behind class is responsible downloading the automated test launcher file
    /// that will be picked up by the client application so that it knows to launch an automated test set
	/// </summary>
    public partial class TestSetLaunch : PageLayout
	{
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.TestSetLaunch::";

		#region Event Handlers

		/// <summary>
		/// This sets up the page upon loading
		/// </summary>
		/// <param name="sender">The sending object</param>
		/// <param name="e">The page load event arguments</param>
		protected void Page_Load(object sender, System.EventArgs e)
		{
			const string METHOD_NAME = "Page_Load";

			Logger.LogEnteringEvent (CLASS_NAME + METHOD_NAME);

            try
            {
                //Get the id of the test set we're trying to launch
                int testSetId;
                if (String.IsNullOrEmpty(Request.QueryString[GlobalFunctions.PARAMETER_TEST_SET_ID]))
                {
                    Response.Write("You need to supply a test set id.");
                    return;
                }
                testSetId = Int32.Parse(Request.QueryString[GlobalFunctions.PARAMETER_TEST_SET_ID]);

                //Now we need to create the text of the file
                //we use a simple XML file format
                XmlDocument xmlDoc = new XmlDocument();
                XmlNode xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "", "");
                xmlDoc.AppendChild(xmlDeclaration);
                //<TestRun>
                XmlElement xmlRootElement = xmlDoc.CreateElement("TestRun");
                xmlDoc.AppendChild(xmlRootElement);
                //<ProjectId>
                XmlElement xmlElement = xmlDoc.CreateElement("ProjectId");
                xmlElement.InnerText = ProjectId.ToString();
                xmlRootElement.AppendChild(xmlElement);
                //<TestSetId>
                xmlElement = xmlDoc.CreateElement("TestSetId");
                xmlElement.InnerText = testSetId.ToString();
                xmlRootElement.AppendChild(xmlElement);
                //<ServerUrl>
                xmlElement = xmlDoc.CreateElement("ServerUrl");
                xmlElement.InnerText = ConfigurationSettings.Default.General_WebServerUrl;
                xmlRootElement.AppendChild(xmlElement);

                //Actually render out the report
                Response.AppendHeader("Content-disposition", "attachment; filename=TestSetLaunch.tst");
                Response.AppendHeader("Content-Type", "application/testmanagement");
                Response.Write(xmlDoc.OuterXml);
                Response.End();
            }
            catch (ThreadAbortException)
            {
                //Do Nothing
            }
            catch (Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Response.Write("Error: " + exception.Message);
            }

			Logger.LogExitingEvent (CLASS_NAME + METHOD_NAME);
			Logger.Flush();
		}

		#endregion
	}
}
