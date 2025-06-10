using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
	/// <summary>
	/// Helper class to perform deletes for relevant API call
	/// </summary>
	public class DeleteFunctions
	{
		/// <summary>
		/// Deletes existing test step parameters
		/// </summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="testStep">The test step object identified by id in the API request</param>
		/// <param name="testStepParameters">Array of parameter objects (names and values) passed in with the API call</param>
		public static void DeleteTestStepParameter(int projectId, TestStep testStep, List<RemoteTestStepParameter> remoteTestStepParameters)
		{
			//Call the business object to actually retrieve the test case parameters
			TestCaseManager testCaseManager = new TestCaseManager();

			//Next retrieve the parameters set on the linked test step for the specific test case
			List<TestStepParameter> testStepParameterValues = testCaseManager.RetrieveParameterValues(testStep.TestStepId);

			//Get the parameters of the test case itself that is a linked test step (so we can access the proper IDs)
			List<TestCaseParameter> testCaseParameters = new List<TestCaseParameter>();
			testCaseParameters = testCaseManager.RetrieveParameters(testStep.TestCaseId, true, true);

			//Loop through remote parameters
			List<TestStepParameter> testStepParametersToUpdate = new List<TestStepParameter>();
			//Delete existing parameters
			foreach (TestCaseParameter testCaseParameter in testCaseParameters)
			{
				TestStepParameter testStepParameter = testStepParameterValues.FirstOrDefault(p => p.TestCaseParameterId == testCaseParameter.TestCaseParameterId);
				// if the parameter is already set check if the remote parameters include it - if so delete it it
				if (testStepParameter != null)
				{
					//Check for a match in the remote parameters
					RemoteTestStepParameter remoteTestStepParameter = remoteTestStepParameters.FirstOrDefault(p => p.Name == testStepParameter.Name);
					//If so, add the object as originally set
					if (remoteTestStepParameter == null)
					{
						testStepParametersToUpdate.Add(testStepParameter);
					}
					//Otherwise do not add which will cause deletion
				}
			}

			//Update the parameters to the linked test step
			testCaseManager.SaveParameterValues(projectId, testStep.TestStepId, testStepParametersToUpdate);

			//Add the new parameters to the linked test step
			testCaseManager.SaveParameterValues(projectId, testStep.TestStepId, testStepParametersToUpdate);
		}
	}
}
