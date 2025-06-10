using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Transactions;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>
	/// This class encapsulates all the data access functionality for
	/// reading and writing Test Cases and Test Steps that are submitted and managed in the system
	/// </summary>
	public class TestCaseManager : ManagerBase
	{
		public enum TestCaseSignatureStatus
		{
			Draft = 1,
			Review = 2,
			Rejected = 3,
			Approved = 4,
			Requested = 9,
			Cancelled = 10
		}

		private const string CLASS_NAME = "Business.TestCaseManager::";

		//Cached lists
		public static List<TestCaseStatus> _staticTestCaseStatuses = null;

		public const int TEST_CASE_FOLDER_ID_ALL_TEST_CASES = -999;
		public const string REPOSITORY_TEST_EXTENSION = ".sstest";
		private int[] TEST_CASE_EXCLUDE_STATUS = new int[] { 3, 4, 5, 6 };
		//Constants
		public const int RELEASE_ID_ACTIVE_RELEASES_ONLY = -2;

		#region Internal Methods

		/// <summary>
		/// Copies across the testCase fields and workflows from one project template to another
		/// </summary>
		/// <param name="existingProjectTemplateId">The id of the existing project template</param>
		/// <param name="newProjectTemplateId">The id of the new project template</param>
		/// <param name="testCaseWorkflowMapping">The workflow mapping</param>
		/// <param name="testCaseTypeMapping">The type mapping</param>
		/// <param name="testCasePriorityMapping">The priority mapping</param>
		/// <param name="customPropertyIdMapping">The custom property mapping</param>
		protected internal void CopyToProjectTemplate(int existingProjectTemplateId, int newProjectTemplateId, Dictionary<int, int> testCaseWorkflowMapping, Dictionary<int, int> testCaseTypeMapping, Dictionary<int, int> testCasePriorityMapping, Dictionary<int, int> customPropertyIdMapping)
		{
			//***** Now we need to copy across the testCase workflows *****
			TestCaseWorkflowManager workflowManager = new TestCaseWorkflowManager();
			workflowManager.Workflow_Copy(existingProjectTemplateId, newProjectTemplateId, customPropertyIdMapping, testCaseWorkflowMapping);

			//***** Now we need to copy across the testCase types *****
			List<TestCaseType> testCaseTypes = TestCaseType_Retrieve(existingProjectTemplateId, false);
			for (int i = 0; i < testCaseTypes.Count; i++)
			{
				//Need to retrieve the mapped workflow for this type
				if (testCaseWorkflowMapping.ContainsKey(testCaseTypes[i].TestCaseWorkflowId))
				{
					int workflowId = testCaseWorkflowMapping[testCaseTypes[i].TestCaseWorkflowId];
					int newTestCaseTypeId = TestCaseType_Insert(
						newProjectTemplateId,
						testCaseTypes[i].Name,
						workflowId,
						testCaseTypes[i].IsDefault,
						testCaseTypes[i].IsActive,
						testCaseTypes[i].IsExploratory);
					testCaseTypeMapping.Add(testCaseTypes[i].TestCaseTypeId, newTestCaseTypeId);
				}
			}

			//***** Now we need to copy across the testCase priorities *****
			List<TestCasePriority> testCasePriorities = TestCasePriority_Retrieve(existingProjectTemplateId, false);
			for (int i = 0; i < testCasePriorities.Count; i++)
			{
				int newPriorityId = TestCasePriority_Insert(
					newProjectTemplateId,
					testCasePriorities[i].Name,
					testCasePriorities[i].Color,
					testCasePriorities[i].IsActive,
					testCasePriorities[i].Score);
				testCasePriorityMapping.Add(testCasePriorities[i].TestCasePriorityId, newPriorityId);
			}
		}

		/// <summary>
		/// Creates the testCase types, importances, default workflow, transitions and field states
		/// for a new project template using the default template
		/// </summary>
		/// <param name="projectTemplateId">The id of the project</param>
		internal void CreateDefaultEntriesForProjectTemplate(int projectTemplateId)
		{
			const string METHOD_NAME = "CreateDefaultEntriesForProjectTemplate";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to create the test case priorities
				TestCasePriority_Insert(projectTemplateId, "1 - Critical", "f47457", true, 1);
				TestCasePriority_Insert(projectTemplateId, "2 - High", "f29e56", true, 2);
				TestCasePriority_Insert(projectTemplateId, "3 - Medium", "f5d857", true, 3);
				TestCasePriority_Insert(projectTemplateId, "4 - Low", "f4f356", true, 4);

				//Next we need to create a default workflow for a project
				TestCaseWorkflowManager workflowManager = new TestCaseWorkflowManager();
				int workflowId = workflowManager.Workflow_InsertWithDefaultEntries(projectTemplateId, GlobalResources.General.Workflow_DefaultWorflow, true).TestCaseWorkflowId;

				//Next we need to create the test case types, associated with this workflow
				TestCaseType_Insert(projectTemplateId, "Acceptance", workflowId, false, true, false, 1);
				TestCaseType_Insert(projectTemplateId, "Compatibility", workflowId, false, true, false, 2);
				TestCaseType_Insert(projectTemplateId, "Functional", workflowId, true, true, false, 4);
				TestCaseType_Insert(projectTemplateId, "Integration", workflowId, false, true, false, 5);
				TestCaseType_Insert(projectTemplateId, "Load/Performance", workflowId, false, true, false, 6);
				TestCaseType_Insert(projectTemplateId, "Network", workflowId, false, true, false, 7);
				TestCaseType_Insert(projectTemplateId, "Regression", workflowId, false, true, false, 8);
				TestCaseType_Insert(projectTemplateId, "Scenario", workflowId, false, true, false, 9);
				TestCaseType_Insert(projectTemplateId, "Security", workflowId, false, true, false, 10);
				TestCaseType_Insert(projectTemplateId, "Unit", workflowId, false, true, false, 11);
				TestCaseType_Insert(projectTemplateId, "Usability", workflowId, false, true, false, 12);
				TestCaseType_Insert(projectTemplateId, "Exploratory", workflowId, false, true, true, 3);

				Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion


		#region Automation Script Handling

		/// <summary>Adds or updates the automation test script associated with a test case</summary>
		/// <param name="userId">The user making the change</param>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="testCaseId">The id of the test case</param>
		/// <param name="automationEngineId">The id of the automation engine</param>
		/// <param name="urlOrFilename">The url or filename for the test script</param>
		/// <param name="description">The description of the automation script</param>
		/// <param name="binaryData">The binary data that comprises the script (only for file attachments)</param>
		/// <param name="version">The version of the test script</param>
		/// <param name="documentTypeId">The attachment type to store the script under (optional)</param>
		/// <param name="projectAttachmentFolderId">The attachment folder to store the script under (optional)</param>
		public void AddUpdateAutomationScript(int userId, int projectId, int testCaseId, int? automationEngineId, string urlOrFilename, string description, byte[] binaryData, string version, int? documentTypeId, int? projectAttachmentFolderId)
		{
			const string METHOD_NAME = "AddUpdateAutomationScript";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Retrieve the test case entity and start tracking
				TestCase testCase = RetrieveById2(projectId, testCaseId);
				testCase.StartTracking();

				//First we need to update the automation engine
				if (automationEngineId.HasValue)
				{
					testCase.AutomationEngineId = automationEngineId.Value;

					//Next see if we have a script already attached
					AttachmentManager attachmentManager = new AttachmentManager();
					if (!testCase.AutomationAttachmentId.HasValue)
					{
						//We need to add a new attachment to the test case
						//See if we have a URL or file attachment
						int attachmentId;
						if (binaryData == null)
						{
							//Add a new URL attachment
							attachmentId = attachmentManager.Insert(
							   projectId,
							   urlOrFilename,
							   description,
							   userId,
							   testCaseId,
							   Artifact.ArtifactTypeEnum.TestCase,
							   version,
							   null,
							   documentTypeId,
							   projectAttachmentFolderId,
							   null,
							   userId
							   );
						}
						else
						{
							//Add a new file attachment
							attachmentId = attachmentManager.Insert(
							   projectId,
							   urlOrFilename,
							   description,
							   userId,
							   binaryData,
							   testCaseId,
							   Artifact.ArtifactTypeEnum.TestCase,
							   version,
							   null,
							   documentTypeId,
							   projectAttachmentFolderId,
							   null
							   );
						}

						testCase.AutomationAttachmentId = attachmentId;
					}
					else
					{
						//Retrieve the existing attachment and add a new version to it
						if (binaryData == null)
						{
							attachmentManager.InsertVersion(
							   projectId,
							   testCase.AutomationAttachmentId.Value,
							   urlOrFilename,
							   description,
							   userId,
							   version,
							   true
							   );
						}
						else
						{
							attachmentManager.InsertVersion(
							   projectId,
							   testCase.AutomationAttachmentId.Value,
							   urlOrFilename,
							   description,
							   userId,
							   binaryData,
							   version,
							   true
							   );
						}

						//If we have a document type or folder specified need to also update the main record
						if (documentTypeId.HasValue || projectAttachmentFolderId.HasValue)
						{
							ProjectAttachment existingAttachment = attachmentManager.RetrieveForProjectById(projectId, testCase.AutomationAttachmentId.Value);
							existingAttachment.StartTracking();
							if (projectAttachmentFolderId.HasValue)
							{
								existingAttachment.ProjectAttachmentFolderId = projectAttachmentFolderId.Value;
							}
							if (documentTypeId.HasValue)
							{
								existingAttachment.DocumentTypeId = documentTypeId.Value;
							}
							attachmentManager.Update(existingAttachment, userId);
						}
					}
				}
				else
				{
					//Remove the automation engine and associated script
					testCase.AutomationEngineId = null;

					if (testCase.AutomationAttachmentId.HasValue)
					{
						AttachmentManager attachmentManager = new AttachmentManager();
						attachmentManager.Delete(projectId, testCase.AutomationAttachmentId.Value, userId);
						testCase.AutomationAttachmentId = null;
					}
				}

				//Persist the changes
				Update(testCase, userId, null, true);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		#endregion

		#region Mapping functions

		/// <summary>Adds a list of test cases to a specific requirement</summary>
		/// <param name="projectId">The current project (the one requirement is in)</param>
		/// <param name="requirementId">The id of the requirement we're associating them with</param>
		/// <param name="testCaseIds">The list of tests being added</param>
		/// <remarks>
		/// 1) Duplicates are ignored
		/// 2) Positive testCaseIds are test cases, negative Ids are for test folders
		/// </remarks>
		/// <returns>The ids of the items that were actually mapped (e.g. the test cases in a folder)</returns>
		public List<int> AddToRequirement(int projectId, int requirementId, List<int> testCaseIds, int userId)
		{
			const string METHOD_NAME = "AddToRequirement";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the list of projects we are allowed to link to (not including the same project)
				List<ProjectArtifactSharing> sharingProjects = new ProjectManager().ProjectAssociation_RetrieveForDestProjectAndArtifact(projectId, Artifact.ArtifactTypeEnum.TestCase);

				//Now iterate through each of the test cases and see if it's a folder
				List<int> validatedTestCaseIds = new List<int>();
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Next get the requirement
					Requirement requirement = context.Requirements
						.SingleOrDefault(r =>
							!r.IsDeleted &&
							r.RequirementId == requirementId);

					if (requirement != null)
					{
						//First get the list of already mapped test cases (to avoid duplicates)
						List<TestCase> mappedTestCases = context.TestCases
									.Where(t =>
										!t.IsDeleted &&
										t.Requirements.Any(rt => rt.RequirementId == requirementId))
									.ToList();

						//Create our list of Test Cases to Associate.
						List<int> assocChanges = new List<int>();

						//The release ID assigned to the Reqirement for associating tests.
						int? relId = requirement.ReleaseId;

						foreach (int testCaseId in testCaseIds)
						{
							//Adding a specific test case.
							if (testCaseId > 0)
							{
								TestCase testCase = context.TestCases
									.SingleOrDefault(t =>
										!t.IsDeleted &&
										t.TestCaseId == testCaseId
									);

								if (testCase != null && !mappedTestCases.Any(t => t.TestCaseId == testCaseId) &&
									!validatedTestCaseIds.Contains(testCaseId) && (testCase.ProjectId == projectId || sharingProjects.Any(p => p.SourceProjectId == testCase.ProjectId)))
								{
									validatedTestCaseIds.Add(testCaseId);

									//Update last updated date of this test case
									testCase.StartTracking();
									testCase.LastUpdateDate = DateTime.UtcNow;

									//Add to the requirement
									testCase.Requirements.Add(requirement);

									//Add it to history report.
									if (!assocChanges.Any(i => i == testCase.ArtifactId))
										assocChanges.Add(testCase.ArtifactId);
								}

								new HistoryManager().LogCreation(projectId, userId, Artifact.ArtifactTypeEnum.Requirement, requirementId, DateTime.UtcNow, Artifact.ArtifactTypeEnum.TestCase, testCaseId);
							}

							//Adding a Test Case folder. (All contents.)
							if (testCaseId < 0)
							{
								int testCaseFolderId = -testCaseId;
								List<TestCase> testCasesInFolder = RetrieveAllInFolder(projectId, testCaseFolderId, false);
								foreach (TestCase testCaseInFolder in testCasesInFolder)
								{
									TestCase testCase = context.TestCases
										.SingleOrDefault(t =>
											t.TestCaseId == testCaseInFolder.TestCaseId &&
											!t.IsDeleted
									);

									if (testCase != null && !mappedTestCases.Any(t => t.TestCaseId == testCaseInFolder.TestCaseId) &&
										!validatedTestCaseIds.Contains(testCaseInFolder.TestCaseId) && (testCase.ProjectId == projectId || sharingProjects.Any(p => p.SourceProjectId == testCase.ProjectId)))
									{
										validatedTestCaseIds.Add(testCaseInFolder.TestCaseId);

										//Update last updated date of this test case 
										testCase.StartTracking();
										testCase.LastUpdateDate = DateTime.UtcNow;

										//Add to the requirement 
										testCase.Requirements.Add(requirement);

										//Add it to history report. 
										if (!assocChanges.Any(i => i == testCase.ArtifactId))
											assocChanges.Add(testCase.ArtifactId);
									}
								}
							}
						}
						//Save the changes (no history or noticiations)
						context.SaveChanges();

						//If we have a release ID, associate these test cases to the Release, too.
						if (relId.HasValue)
							AddToRelease(projectId, relId.Value, assocChanges, userId);

						//If we didn't throw an error, call the function to save the history entry for 
						new HistoryManager().AddRequirementTestCoverage(projectId, requirement.ArtifactId, requirement.Name, userId, assocChanges);
						
					}
				}

				if (validatedTestCaseIds.Count > 0)
				{
					//Finally perform a bulk refresh of the requirement list coverage summary data
					RequirementManager requirementManager = new RequirementManager();
					requirementManager.RefreshTaskProgressAndTestCoverage(projectId, requirementId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return validatedTestCaseIds;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Removes a list of test cases from a specific requirement</summary>
		/// <param name="projectId">The current project</param>
		/// <param name="requirementId">The id of the requirement we're associating them with</param>
		/// <param name="testCaseIds">The list of tests being added</param>
		/// <remarks>
		/// 1) Items that are not already mapped are ignored
		/// 2) Positive testCaseIds are test cases, negative Ids are for test folders
		/// </remarks>
		public void RemoveFromRequirement(int projectId, int requirementId, List<int> testCaseIds, int userId)
		{
			const string METHOD_NAME = "RemoveFromRequirement";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Now iterate through each of the test cases and make sure it's mapped already
				List<int> validatedTestCaseIds = new List<int>();
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First get the list of already mapped test cases (to avoid concurrency errors)
					var query = from r in context.Requirements.Include(r => r.TestCases)
								where r.ProjectId == projectId && r.RequirementId == requirementId
								select r;

					Requirement requirement = query.FirstOrDefault();
					if (requirement != null)
					{
						//For history recording!
						List<int> changeTests = new List<int>();

						requirement.StartTracking();
						requirement.LastUpdateDate = DateTime.UtcNow;
						foreach (int testCaseId in testCaseIds)
						{
							if (testCaseId > 0)
							{
								//Check to see if it's already mapped, if so, add to validated list
								TestCase mappedTestCase = requirement.TestCases.FirstOrDefault(t => t.TestCaseId == testCaseId);
								if (mappedTestCase != null)
								{
									validatedTestCaseIds.Add(testCaseId);

									//Update last updated date of this test case
									mappedTestCase.StartTracking();
									mappedTestCase.LastUpdateDate = DateTime.UtcNow;

									//Remove from the requirement
									requirement.TestCases.Remove(mappedTestCase);

									new HistoryManager().LogDeletion(projectId, userId, Artifact.ArtifactTypeEnum.Requirement, requirementId, DateTime.UtcNow, Artifact.ArtifactTypeEnum.TestCase, testCaseId);

									//Add it to our record, if it dosen't already exist.
									if (!changeTests.Any(g => g == mappedTestCase.TestCaseId))
										changeTests.Add(mappedTestCase.TestCaseId);
								}
							}
							if (testCaseId < 0)
							{
								int testCaseFolderId = -testCaseId;
								List<TestCase> testCasesInFolder = RetrieveAllInFolder(projectId, testCaseFolderId);
								foreach (TestCase testCase in testCasesInFolder)
								{
									TestCase mappedTestCase = requirement.TestCases.FirstOrDefault(t => t.TestCaseId == testCase.TestCaseId);
									if (mappedTestCase != null && !validatedTestCaseIds.Contains(testCase.TestCaseId))
									{
										//Don't update the last updated date of these ones for performance reasons
										validatedTestCaseIds.Add(testCase.TestCaseId);
										requirement.TestCases.Remove(mappedTestCase);

										//Add it to our record, if it dosen't already exist.
										if (!changeTests.Any(g => g == mappedTestCase.TestCaseId))
											changeTests.Add(mappedTestCase.TestCaseId);
									}
								}
							}
						}

						//Save changes
						context.SaveChanges();

						//If we didn't throw an error, call the function to save an association.
						new HistoryManager().RemoveRequirementTestCoverage(projectId, requirement.ArtifactId, requirement.Name, userId, changeTests);
					}
				}

				if (validatedTestCaseIds.Count > 0)
				{
					//Finally perform a bulk refresh of the requirement list coverage summary data
					RequirementManager requirement = new RequirementManager();
					requirement.RefreshTaskProgressAndTestCoverage(projectId, requirementId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Removes a list of test cases from a specific release</summary>
		/// <param name="projectId">The current project</param>
		/// <param name="releaseId">The id of the release we're associating them with</param>
		/// <param name="testCaseIds">The list of tests being removed</param>
		/// <remarks>
		/// 1) Items that are not already mapped are ignored.
		/// 2) Positive testCaseIds are test cases, negative Ids are for test folders
		/// </remarks>
		public void RemoveFromRelease(int projectId, int releaseId, List<int> testCaseIds, int userId)
		{
			const string METHOD_NAME = "RemoveFromRelease";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First get the list of already mapped test cases (to avoid concurrency errors)
				List<TestCase> mappedTestCases = RetrieveMappedByReleaseId(projectId, releaseId);

				//Now iterate through each of the test cases and make sure it's mapped already
				List<int> validatedTestCaseIds = new List<int>();
				List<int> testCaseFolderIsToRefresh = new List<int>();
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First get the list of already mapped test cases (to avoid concurrency errors)
					Release release = context.Releases
						.Where(r => r.ReleaseId == releaseId)
						.SingleOrDefault();

					if (release != null)
					{
						//For history recording!
						List<int> changeTests = new List<int>();

						foreach (int testCaseId in testCaseIds)
						{
							//See if we have a test case or folder id
							if (testCaseId > 0)
							{
								//Check to see if it's already mapped, if so, add to validated list
								TestCase mappedTestCase = mappedTestCases.FirstOrDefault(t => t.TestCaseId == testCaseId);
								if (mappedTestCase != null)
								{
									//Update last modified.
									context.TestCases.ApplyChanges(mappedTestCase);
									mappedTestCase.StartTracking();
									mappedTestCase.LastUpdateDate = DateTime.UtcNow;

									//Add to list
									validatedTestCaseIds.Add(testCaseId);

									//Also add the folder to be refreshed
									if (mappedTestCase.TestCaseFolderId.HasValue && !testCaseFolderIsToRefresh.Contains(mappedTestCase.TestCaseFolderId.Value))
									{
										testCaseFolderIsToRefresh.Add(mappedTestCase.TestCaseFolderId.Value);
									}

									//Add it to our record, if it dosen't already exist.
									if (!changeTests.Any(g => g == mappedTestCase.TestCaseId))
										changeTests.Add(mappedTestCase.TestCaseId);
								}
							}
							if (testCaseId < 0)
							{
								int testCaseFolderId = -testCaseId;
								List<TestCase> testCasesInFolder = RetrieveAllInFolder(projectId, testCaseFolderId);
								foreach (TestCase testCase in testCasesInFolder)
								{
									//Check to see if it's already mapped, if so, add to validated list
									if (mappedTestCases.Any(t => t.TestCaseId == testCase.TestCaseId) && !validatedTestCaseIds.Contains(testCase.TestCaseId))
									{
										//Don't update the last updated date of these ones for performance reasons
										validatedTestCaseIds.Add(testCase.TestCaseId);

										//Also add the folder to be refreshed
										if (testCase.TestCaseFolderId.HasValue && !testCaseFolderIsToRefresh.Contains(testCase.TestCaseFolderId.Value))
										{
											testCaseFolderIsToRefresh.Add(testCase.TestCaseFolderId.Value);
										}

										//Add it to our record, if it dosen't already exist.
										if (!changeTests.Any(g => g == testCase.TestCaseId))
											changeTests.Add(testCase.TestCaseId);
									}
								}

								//Also add the folder to be refreshed
								if (!testCaseFolderIsToRefresh.Contains(testCaseFolderId))
								{
									testCaseFolderIsToRefresh.Add(testCaseFolderId);
								}
							}
						}
						//Now remove the validated items from the mapping table
						foreach (int testCaseId in validatedTestCaseIds)
						{
							var query = from r in context.ReleaseTestCases
										where r.TestCaseId == testCaseId && r.ReleaseId == releaseId
										select r;

							ReleaseTestCase releaseTestCase = query.FirstOrDefault();
							if (releaseTestCase != null)
							{
								context.ReleaseTestCases.DeleteObject(releaseTestCase);
								new HistoryManager().LogDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.ReleaseTestCase, releaseId, DateTime.UtcNow, null, testCaseId);
							}
						}

						//Save changes
						context.SaveChanges();

						//If we didn't throw an error, call the function to save an association.
						new HistoryManager().RemoveReleaseTestCoverage(projectId, release.ArtifactId, release.Name, userId, changeTests);
						
					}
				}

				if (testCaseFolderIsToRefresh.Count > 0)
				{
					foreach (int testCaseFolderId in testCaseFolderIsToRefresh)
					{
						RefreshFolderExecutionStatus(projectId, testCaseFolderId);
					}
				}

				if (validatedTestCaseIds.Count > 0)
				{
					//Finally perform a bulk refresh of the release test status/effort/progress
					ReleaseManager releaseManager = new ReleaseManager();
					releaseManager.RefreshProgressEffortTestStatus(projectId, releaseId);
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Counts the number of test case covering a specific requirement
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="requirementId">The id of the release</param>
		/// <returns>The count</returns>
		public int CountCoveredByRequirementId(int projectId, int requirementId)
		{
			const string METHOD_NAME = "CountCoveredByRequirementId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int count;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Count how many test cases are connected to the requirement
					var query = from r in context.Requirements
								.Include(r => r.TestCases)
								where r.RequirementId == requirementId
								select r.TestCases.Count;

					count = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return count;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Adds a list of test cases to a specific release</summary>
		/// <param name="projectId">The current project</param>
		/// <param name="releaseId">The id of the release we're associating them with</param>
		/// <param name="testCaseIds">The list of tests being added</param>
		/// <remarks>
		/// 1) Duplicates are ignored
		/// 2) Positive testCaseIds are test cases, negative Ids are for test folders
		/// </remarks>
		/// <returns>The ids of the items that were actually mapped</returns>
		public List<int> AddToRelease(int projectId, int releaseId, List<int> testCaseIds, int userId)
		{
			const string METHOD_NAME = "AddToRelease";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First get the list of already mapped test cases (to avoid duplicates)
				List<TestCase> mappedTestCases = RetrieveMappedByReleaseId(projectId, releaseId);

				//Now iterate through each of the test cases and make sure not already mapped
				//and if so add
				List<int> validatedTestCaseIds = new List<int>();
				List<int> testCaseFolderIsToRefresh = new List<int>();
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Next get the requirement
					Release release = context.Releases
						.Where(r => r.ReleaseId == releaseId)
						.SingleOrDefault();

					if (release != null)
					{
						//Create our History list.
						List<int> assocChanges = new List<int>();


						foreach (int testCaseId in testCaseIds)
						{
							if (testCaseId > 0)
							{
								var query = from t in context.TestCases
											where t.TestCaseId == testCaseId && !t.IsDeleted && t.ProjectId == projectId
											select t;

								TestCase testCase = query.FirstOrDefault();
								if (testCase != null && !mappedTestCases.Any(t => t.TestCaseId == testCaseId) && !validatedTestCaseIds.Contains(testCaseId))
								{
									validatedTestCaseIds.Add(testCaseId);

									//Update last updated date of this test case
									testCase.StartTracking();
									testCase.LastUpdateDate = DateTime.UtcNow;

									//Also add the folder to be refreshed
									if (testCase.TestCaseFolderId.HasValue && !testCaseFolderIsToRefresh.Contains(testCase.TestCaseFolderId.Value))
										testCaseFolderIsToRefresh.Add(testCase.TestCaseFolderId.Value);

									//Add it to history report.
									if (!assocChanges.Any(i => i == testCase.ArtifactId))
										assocChanges.Add(testCase.ArtifactId);
								}
							}
							if (testCaseId < 0)
							{
								int testCaseFolderId = -testCaseId;
								List<TestCase> testCasesInFolder = RetrieveAllInFolder(projectId, testCaseFolderId, false);
								foreach (TestCase testCase in testCasesInFolder)
								{
									if (!mappedTestCases.Any(t => t.TestCaseId == testCase.TestCaseId) && !validatedTestCaseIds.Contains(testCase.TestCaseId))
									{
										//Don't update the last updated date of these ones for performance reasons
										validatedTestCaseIds.Add(testCase.TestCaseId);

										//Also add the folder to be refreshed
										if (testCase.TestCaseFolderId.HasValue && !testCaseFolderIsToRefresh.Contains(testCase.TestCaseFolderId.Value))
										{
											testCaseFolderIsToRefresh.Add(testCase.TestCaseFolderId.Value);
										}

										//Add it to history report.
										if (!assocChanges.Any(i => i == testCase.ArtifactId))
											assocChanges.Add(testCase.ArtifactId);
									}
								}

								//Also add the folder to be refreshed
								if (!testCaseFolderIsToRefresh.Contains(testCaseFolderId))
								{
									testCaseFolderIsToRefresh.Add(testCaseFolderId);
								}
							}
						}

						//Now add the validated items to the mapping table
						foreach (int testCaseId in validatedTestCaseIds)
						{
							//The execution data is initially left blank, it will be updated later
							ReleaseTestCase releaseTestCase = new ReleaseTestCase();
							releaseTestCase.ReleaseId = releaseId;
							releaseTestCase.TestCaseId = testCaseId;
							releaseTestCase.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotRun;
							context.ReleaseTestCases.AddObject(releaseTestCase);
						}

						//Save the changes (no history or noticiations)
						context.SaveChanges();
						//context.SaveChanges(userId, true, false, null);
						//If we didn't throw an error, call the function to save an association.
						new HistoryManager().AddReleaseTestCoverage(projectId, release.ArtifactId, release.Name, userId, assocChanges);

						new HistoryManager().LogCreation(projectId, userId, Artifact.ArtifactTypeEnum.TestCase, releaseId, DateTime.UtcNow);
					}
				}

				//Refresh the test cases' execution status
				if (validatedTestCaseIds.Count > 0)
				{
					TestRunManager testRunManager = new TestRunManager();
					foreach (int testCaseId in validatedTestCaseIds)
					{
						testRunManager.RefreshTestCaseExecutionStatus3(projectId, testCaseId, releaseId);
					}
				}

				//Refresh the folders
				if (testCaseFolderIsToRefresh.Count > 0)
				{
					foreach (int testCaseFolderId in testCaseFolderIsToRefresh)
					{
						RefreshFolderExecutionStatus(projectId, testCaseFolderId);
					}
				}

				if (validatedTestCaseIds.Count > 0)
				{
					//Finally perform a bulk refresh of the release test status
					ReleaseManager releaseManager = new ReleaseManager();
					releaseManager.RefreshProgressEffortTestStatus(projectId, releaseId);
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return validatedTestCaseIds;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the list of mapped test cases already mapped against a release</summary>
		/// <param name="releaseId">The ID of the release we want the list for</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <returns>Test Case list</returns>
		public List<TestCase> RetrieveMappedByReleaseId(int projectId, int releaseId)
		{
			const string METHOD_NAME = "RetrieveMappedByReleaseId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestCase> mappedTestCases;

				//Create custom query for retrieving the test cases and execute
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var hasTestCaseIds = from rt in context.ReleaseTestCases
										 where rt.ReleaseId == releaseId
										 select rt.TestCaseId;

					var query = from t in context.TestCases
								where !t.IsDeleted && hasTestCaseIds.Contains(t.TestCaseId)
								orderby t.Name, t.TestCaseId
								select t;

					mappedTestCases = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return mappedTestCases;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the list of mapped test cases already mapped against a release</summary>
		/// <param name="releaseId">The ID of the release we want the list for</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <returns>Test Case mapping records</returns>
		public List<TestCaseReleaseView> RetrieveMappedByReleaseId2(int projectId, int releaseId)
		{
			const string METHOD_NAME = "RetrieveMappedByReleaseId2";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestCaseReleaseView> mappedTestCases;

				//Create custom query for retrieving the test cases and execute
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from rt in context.TestCaseReleasesView
								where rt.ReleaseId == releaseId && !rt.IsDeleted
								orderby rt.Name, rt.TestCaseId
								select rt;

					mappedTestCases = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return mappedTestCases;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the list of covering test cases already mapped against a requirement</summary>
		/// <param name="requirementId">The ID of the requirement we want the list for</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <returns>Test Case list</returns>
		public List<TestCase> RetrieveCoveredByRequirementId(int projectId, int requirementId)
		{
			const string METHOD_NAME = "RetrieveCoveredByRequirementId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestCase> coveringTestCases;

				//Create custom query for retrieving the test cases and execute
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestCases
									.Include(t => t.ExecutionStatus)
									.Include(t => t.Priority)
									.Include(t => t.Status)
									.Include(t => t.Type)
								where !t.IsDeleted && t.Requirements.Any(rt => rt.RequirementId == requirementId)
								orderby t.Name, t.TestCaseId
								select t;

					coveringTestCases = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return coveringTestCases;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region TestCaseFolder functions

		/// <summary>Refreshes the complete test case folder count (and the count per status) for a given project</summary>
		/// <param name="projectId">The project we want to refresh</param>
		/// <remarks>Useful if a user has modified the live data in the database directly</remarks>
		public void RefreshFolderCounts(int projectId)
		{
			const string METHOD_NAME = "RefreshFolderCounts";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Make sure we have not disabled rollups, if so log.
				if (new ProjectSettings(projectId).RollupCalculationsDisabled)
				{
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, String.Format(GlobalResources.Messages.Global_RollupCalculationsDisabled, projectId));
					return;
				}

				//Get the list of test folders in the project
				List<TestCaseFolderHierarchyView> testFolderHierarchy = TestCaseFolder_GetList(projectId);

				//Refresh the test case folder count
				foreach (TestCaseFolderHierarchyView testFolder in testFolderHierarchy)
				{
					RefreshFolderExecutionStatus(projectId, testFolder.TestCaseFolderId);
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Updates the execution status of a test folder and all its successive parents
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="folderId">The id of the folder</param>
		/// <remarks>It also needs to update the same information for each release</remarks>
		protected internal void RefreshFolderExecutionStatus(int projectId, int folderId)
		{
			const string METHOD_NAME = "RefreshFolderExecutionStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Make sure we have not disabled rollups, if so log.
				if (new ProjectSettings(projectId).RollupCalculationsDisabled)
				{
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, String.Format(GlobalResources.Messages.Global_RollupCalculationsDisabled, projectId));
					return;
				}

				TestCaseFolder testCaseFolder;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the specified folder
					var query1 = from f in context.TestCaseFolders
								 where f.TestCaseFolderId == folderId
								 select f;

					testCaseFolder = query1.FirstOrDefault();
					if (testCaseFolder == null)
					{
						//Just exit
						return;
					}

					//Now do the update using the stored procedure
					context.CommandTimeout = SQL_COMMAND_TIMEOUT_CACHE_UPDATE;
					context.TestCase_RefreshFolderExecutionStatus(projectId, folderId);
				}

				//Finally recursively call this function to make sure that successive parents are rolled-up
				if (testCaseFolder.ParentTestCaseFolderId.HasValue)
				{
					RefreshFolderExecutionStatus(projectId, testCaseFolder.ParentTestCaseFolderId.Value);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Gets the list of child folders for a parent folder. If the folder id is null it gets the root folder
		/// </summary>
		/// <param name="releaseId">The id of the release</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="folderId">The parent folder id (or null for root folder)</param>
		/// <param name="utcOffset">The offset from UTC</param>
		/// <param name="sortProperty">The property name to be sorted on</param>
		/// <param name="sortAscending">Whether to sort the data ascending</param>
		/// <param name="filters">The collection of filters - pass null if none specified</param>

		/// <returns>List of folders</returns>
		public List<TestCaseFolderReleaseView> TestCaseFolder_GetByParentIdForRelease(int projectId, int? folderId, int releaseId, string sortProperty = null, bool sortAscending = true, Hashtable filters = null, double utcOffset = 0)
		{
			const string METHOD_NAME = "TestCaseFolder_GetByParentIdForRelease";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestCaseFolderReleaseView> folders;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//We have to use this syntax because using int? == int? comparison in EF4 LINQ will
					//result in invalid SQL (== NULL instead of IS NULL)
					IQueryable<TestCaseFolderReleaseView> query;
					if (folderId.HasValue)
					{
						query = from f in context.TestCaseFolderReleasesView
								where
									f.ParentTestCaseFolderId == folderId.Value &&
									f.ProjectId == projectId &&
									f.ReleaseId == releaseId
								select f;
					}
					else
					{
						query = from f in context.TestCaseFolderReleasesView
								where
									f.ParentTestCaseFolderId == null &&
									f.ProjectId == projectId &&
									f.ReleaseId == releaseId
								select f;
					}

					//Add the dynamic sort
					if (String.IsNullOrEmpty(sortProperty))
					{
						//Default to sorting by name ascending
						query = query.OrderBy(f => f.Name).ThenBy(f => f.TestCaseFolderId);
					}
					else
					{
						//We always sort by the physical ID to guarantee stable sorting
						string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
						query = query.OrderUsingSortExpression(sortExpression, "TestCaseFolderId");
					}

					//Add the dynamic filters
					if (filters != null)
					{
						//Get the template for this project
						int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<TestCaseFolderReleaseView, bool>> filterClause = CreateFilterExpression<TestCaseFolderReleaseView>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, filters, utcOffset, null, HandleTestFolderSpecificFilters);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<TestCaseFolderReleaseView>)query.Where(filterClause);
						}
					}

					folders = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return folders;
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Gets the list of child folders for a parent folder. If the folder id is null it gets the root folder
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="folderId">The parent folder id (or null for root folder)</param>
		/// <param name="utcOffset">The offset from UTC</param>
		/// <param name="sortProperty">The property name to be sorted on</param>
		/// <param name="sortAscending">Whether to sort the data ascending</param>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <returns>List of folders</returns>
		public List<TestCaseFolder> TestCaseFolder_GetByParentId(int projectId, int? folderId, string sortProperty = null, bool sortAscending = true, Hashtable filters = null, double utcOffset = 0)
		{
			const string METHOD_NAME = "TestCaseFolder_GetByParentId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestCaseFolder> folders;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//We have to use this syntax because using int? == int? comparison in EF4 LINQ will
					//result in invalid SQL (== NULL instead of IS NULL)
					IQueryable<TestCaseFolder> query;
					if (folderId.HasValue)
					{
						query = from f in context.TestCaseFolders
								where f.ParentTestCaseFolderId == folderId.Value && f.ProjectId == projectId
								select f;
					}
					else
					{
						query = from f in context.TestCaseFolders
								where f.ParentTestCaseFolderId == null && f.ProjectId == projectId
								select f;
					}

					//Add the dynamic sort
					if (String.IsNullOrEmpty(sortProperty))
					{
						//Default to sorting by name ascending
						query = query.OrderBy(f => f.Name).ThenBy(f => f.TestCaseFolderId);
					}
					else
					{
						//We always sort by the physical ID to guarantee stable sorting
						string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
						query = query.OrderUsingSortExpression(sortExpression, "TestCaseFolderId");
					}

					//Add the dynamic filters
					if (filters != null)
					{
						//Get the template for this project
						int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<TestCaseFolder, bool>> filterClause = CreateFilterExpression<TestCaseFolder>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, filters, utcOffset, null, HandleTestFolderSpecificFilters);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<TestCaseFolder>)query.Where(filterClause);
						}
					}

					folders = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return folders;
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Commits any changes made to a folder entity
		/// </summary>
		/// <param name="folder">The folder entity</param>
		/// <remarks>Can be used for both reordering the folder and making updates to the specific one</remarks>
		public void TestCaseFolder_Update(TestCaseFolder folder)
		{
			const string METHOD_NAME = CLASS_NAME + "TestCaseFolder_Update(TestCaseFolder)";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Check for null folder, do nothing
			if (folder == null)
			{
				return;
			}

			int? oldParentFolderId = null;
			bool updateFolder = false;
			int projectId = folder.ProjectId;
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					//If the folder has switched from a root one, make sure that there is at least one other folder in the project
					if (folder.ChangeTracker.OriginalValues.ContainsKey("ParentTestCaseFolderId"))
					{
						updateFolder = true;
						if (folder.ChangeTracker.OriginalValues["ParentTestCaseFolderId"] != null)
						{
							oldParentFolderId = (int?)folder.ChangeTracker.OriginalValues["ParentTestCaseFolderId"];
						}
						if (folder.ChangeTracker.OriginalValues["ParentTestCaseFolderId"] == null && folder.ParentTestCaseFolderId.HasValue)
						{
							//See how many root folders we have
							var query = from t in context.TestCaseFolders
										where t.ProjectId == folder.ProjectId && !t.ParentTestCaseFolderId.HasValue && t.TestCaseFolderId != folder.TestCaseFolderId
										select t;

							//Make sure we have one remaining
							if (query.Count() < 1)
							{
								throw new ProjectDefaultTestCaseFolderException("You need to have at least one top-level test case folder in the project");
							}
						}

						//Make sure the new parent folder is not a child of the current one (would create circular loop)
						if (folder.ParentTestCaseFolderId.HasValue)
						{
							List<TestCaseFolderHierarchyView> childFolders = TestCaseFolder_GetChildren(folder.ProjectId, folder.TestCaseFolderId, false);
							if (childFolders.Any(f => f.TestCaseFolderId == folder.ParentTestCaseFolderId.Value))
							{
								throw new FolderCircularReferenceException(GlobalResources.Messages.TestCase_CannotMakeParentChildOfCurrentTestCase);
							}
						}
					}

					context.TestCaseFolders.ApplyChanges(folder);
					context.SaveChanges();
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Saving folder:");
					throw;
				}
			}

			//Next refresh the folder hierarchy cache
			TestCaseFolder_RefreshHierarchy(projectId);

			//Need to also update the test case counts if the parent folder field changed
			//Rollup the changes to the folders
			if (updateFolder && folder.ParentTestCaseFolderId.HasValue)
			{
				RefreshFolderExecutionStatus(projectId, folder.ParentTestCaseFolderId.Value);
			}
			if (updateFolder && oldParentFolderId.HasValue)
			{
				RefreshFolderExecutionStatus(projectId, oldParentFolderId.Value);
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Exports a test folder (and their children) from one project to another</summary>
		/// <param name="userId">The user exporting the test folder</param>
		/// <param name="sourceProjectId">The project we're exporting from</param>
		/// <param name="sourceTestFolderId">The id of the test folder being exported</param>
		/// <param name="destProjectId">The project we're exporting to</param>
		/// <param name="testCaseMapping">A dictionary used to keep track of any test cases created needed when we have linked test cases</param>
		/// <param name="testFolderMapping">A dictionary used to keep track of any exported test folders</param>
		/// <returns>The id of the test folder in the new project</returns>
		public int TestCaseFolder_Export(int userId, int sourceProjectId, int sourceTestFolderId, int destProjectId, Dictionary<int, int> testCaseMapping, Dictionary<int, int> testFolderMapping)
		{
			const string METHOD_NAME = "TestCaseFolder_Export";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the test folder being copied
				TestCaseFolder sourceTestCaseFolder = TestCaseFolder_GetById(sourceTestFolderId);

				int? destParentFolderId = null;
				if (sourceTestCaseFolder.ParentTestCaseFolderId.HasValue)
				{
					if (testFolderMapping.ContainsKey(sourceTestCaseFolder.ParentTestCaseFolderId.Value))
					{
						destParentFolderId = testFolderMapping[sourceTestCaseFolder.ParentTestCaseFolderId.Value];
					}
				}

				//Firstly export the test folder itself
				int exportedTestFolderId = TestCaseFolder_Create(
					sourceTestCaseFolder.Name,
					destProjectId,
					sourceTestCaseFolder.Description,
					destParentFolderId).TestCaseFolderId;

				//Add to the mapping
				if (!testFolderMapping.ContainsKey(sourceTestFolderId))
				{
					testFolderMapping.Add(sourceTestFolderId, exportedTestFolderId);
				}

				//Next we need to export all the child test cases
				List<TestCaseView> childTestCases = Retrieve(sourceProjectId, "Name", true, 1, Int32.MaxValue, null, 0, sourceTestFolderId);
				foreach (TestCaseView childTestCase in childTestCases)
				{
					//Copy the test case, leaving its name unchanged
					TestCase_Export(userId, sourceProjectId, childTestCase.TestCaseId, destProjectId, testCaseMapping, exportedTestFolderId);
				}

				//Next we need to recursively call this function for any child test case folders
				List<TestCaseFolder> childTestFolders = TestCaseFolder_GetByParentId(sourceProjectId, sourceTestFolderId);
				if (childTestFolders != null && childTestFolders.Count > 0)
				{
					foreach (TestCaseFolder childTestFolder in childTestFolders)
					{
						//Copy the folder into the new folder, leaving the name unchanged
						TestCaseFolder_Export(userId, sourceProjectId, childTestFolder.TestCaseFolderId, destProjectId, testCaseMapping, testFolderMapping);
					}
				}

				//Finally refresh the folder hierarchy cache
				TestCaseFolder_RefreshHierarchy(destProjectId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the test folder id of the exported version
				return exportedTestFolderId;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Exports all the test folders (not test cases) from one project to another</summary>
		/// <param name="userId">The user exporting the test folder</param>
		/// <param name="sourceProjectId">The project we're exporting from</param>
		/// <param name="destProjectId">The project we're exporting to</param>
		/// <param name="testFolderMapping">A dictionary used to keep track of any exported test folders</param>
		/// <param name="parentFolder">The id of the parent folder (null = root)</param>
		/// <remarks>Used by the Project Copy function</remarks>
		protected internal void TestCaseFolder_ExportAll(int userId, int sourceProjectId, int destProjectId, Dictionary<int, int> testFolderMapping, int? parentFolder = null)
		{
			const string METHOD_NAME = "TestCaseFolder_ExportAll";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the immediate child folders
				List<TestCaseFolder> sourceTestCaseFolders = TestCaseFolder_GetByParentId(sourceProjectId, parentFolder);

				foreach (TestCaseFolder sourceTestCaseFolder in sourceTestCaseFolders)
				{
					int? destParentFolderId = null;
					if (sourceTestCaseFolder.ParentTestCaseFolderId.HasValue)
					{
						if (testFolderMapping.ContainsKey(sourceTestCaseFolder.ParentTestCaseFolderId.Value))
						{
							destParentFolderId = testFolderMapping[sourceTestCaseFolder.ParentTestCaseFolderId.Value];
						}
					}

					//Firstly export the test folder itself
					int exportedTestFolderId = TestCaseFolder_Create(
						sourceTestCaseFolder.Name,
						destProjectId,
						sourceTestCaseFolder.Description,
						destParentFolderId).TestCaseFolderId;

					//Add to the mapping
					if (!testFolderMapping.ContainsKey(sourceTestCaseFolder.TestCaseFolderId))
					{
						testFolderMapping.Add(sourceTestCaseFolder.TestCaseFolderId, exportedTestFolderId);
					}

					//Copy the folder into the new folder, leaving the name unchanged
					TestCaseFolder_ExportAll(userId, sourceProjectId, destProjectId, testFolderMapping, sourceTestCaseFolder.TestCaseFolderId);
				}

				//Finally refresh the folder hierarchy cache
				TestCaseFolder_RefreshHierarchy(destProjectId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Deletes a folder in the system, cascading any deletes to the child folders
		/// </summary>
		/// <param name="userId">The id of the user doing the delete</param>
		/// <param name="folderId">The id of the folder</param>
		/// <param name="projectId">The ID of the project</param>
		public void TestCaseFolder_Delete(int projectId, int folderId, int userId)
		{
			const string METHOD_NAME = "TestCaseFolder_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get this folder and any child ones
					List<TestCaseFolderHierarchyView> childFolders = context.TestCase_RetrieveChildFolders(projectId, folderId, true).OrderByDescending(t => t.IndentLevel).ToList();

					//If we have child folders, we need to delete them recursively
					for (int i = 0; i < childFolders.Count; i++)
					{
						//The database will unset the Folder Id of the testCases using the database cascade

						//Just do a 'detached' object delete by id
						TestCaseFolder folderToDelete = new TestCaseFolder();
						folderToDelete.TestCaseFolderId = childFolders[i].TestCaseFolderId;
						context.TestCaseFolders.Attach(folderToDelete);
						context.ObjectStateManager.ChangeObjectState(folderToDelete, EntityState.Deleted);
						context.SaveChanges();
					}
				}

				//Finally refresh the folder hierarchy cache
				TestCaseFolder_RefreshHierarchy(projectId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Exports a test case (and their steps) from one project to another</summary>
		/// <param name="userId">The user exporting the test case</param>
		/// <param name="sourceProjectId">The project we're exporting from</param>
		/// <param name="sourceTestCaseId">The id of the test case being exported</param>
		/// <param name="destProjectId">The project we're exporting to</param>
		/// <param name="destTestFolderId">The id of the destination folder or null for root</param>
		/// <param name="testCaseMapping">A dictionary used to keep track of any test cases created needed when we have linked test cases</param>
		/// <returns>The id of the test case in the new project</returns>
		public int TestCase_Export(int userId, int sourceProjectId, int sourceTestCaseId, int destProjectId, Dictionary<int, int> testCaseMapping, int? destTestFolderId = null)
		{
			const string METHOD_NAME = "TestCase_Export";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the test case being copied
				TestCase sourceTestCase = RetrieveById2(sourceProjectId, sourceTestCaseId);

				//We need to get the source and destination project templates
				//If they are the same, then certain additional values can get copied across
				int sourceProjectTemplateId = new TemplateManager().RetrieveForProject(sourceProjectId).ProjectTemplateId;
				int destProjectTemplateId = new TemplateManager().RetrieveForProject(destProjectId).ProjectTemplateId;
				bool templatesSame = (sourceProjectTemplateId == destProjectTemplateId);

				//If we have an automation attachment, we need to copy that to a new project
				//physically creating a new file if necessarys so that changes in one project
				//do not affect another (used to happen in Spira v3.x)
				int? newAutomationAttachmentId = null;
				Dictionary<int, int> attachmentsMapping = new Dictionary<int, int>();
				if (sourceTestCase.AutomationAttachmentId.HasValue)
				{
					newAutomationAttachmentId = ExportAutomationScript(sourceProjectId, sourceTestCase.AutomationAttachmentId.Value, destProjectId);

					//Map the new and old and new attachment ids together so that we can link to the right one when
					//the other attachments are exported.
					if (newAutomationAttachmentId.HasValue)
					{
						attachmentsMapping.Add(sourceTestCase.AutomationAttachmentId.Value, newAutomationAttachmentId.Value);
					}
				}

				//Insert a new test case with the data copied from the existing one
				//It goes into the root folder
				int exportedTestCaseId = Insert(
					userId,
					destProjectId,
					sourceTestCase.AuthorId,
					sourceTestCase.OwnerId,
					sourceTestCase.Name,
					sourceTestCase.Description,
					(templatesSame) ? (int?)sourceTestCase.TestCaseTypeId : null,
					(TestCase.TestCaseStatusEnum)sourceTestCase.TestCaseStatusId,
					(templatesSame) ? sourceTestCase.TestCasePriorityId : null,
					destTestFolderId,
					sourceTestCase.EstimatedDuration,
					sourceTestCase.AutomationEngineId,
					newAutomationAttachmentId
					);

				//Create history item..
				new HistoryManager().LogImport(destProjectId, sourceProjectId, sourceTestCaseId, userId, Artifact.ArtifactTypeEnum.TestCase, exportedTestCaseId, DateTime.UtcNow);

				//We copy custom properties if the templates are the same
				if (templatesSame)
				{
					//Now we need to copy across any custom properties
					new CustomPropertyManager().ArtifactCustomProperty_Export(sourceProjectTemplateId, sourceProjectId, sourceTestCaseId, destProjectId, exportedTestCaseId, Artifact.ArtifactTypeEnum.TestCase, userId);
				}

				//Add to the mapping collection (prevents duplicates)
				if (!testCaseMapping.ContainsKey(sourceTestCaseId))
				{
					testCaseMapping.Add(sourceTestCaseId, exportedTestCaseId);
				}

				//Now we need to copy across any test case parameters
				List<TestCaseParameter> testCaseParameters = RetrieveParameters(sourceTestCaseId);
				foreach (TestCaseParameter testCaseParameter in testCaseParameters)
				{
					InsertParameter(
						destProjectId,
						exportedTestCaseId,
						testCaseParameter.Name,
						testCaseParameter.DefaultValue, userId);
				}

				//Coverage and custom properties are not exported as they may not be the same in the two projects 

				//Now we need to export any linked  test cases
				ExportLinkedTestCases(userId, sourceProjectId, sourceTestCaseId, destProjectId, exportedTestCaseId, testCaseMapping);

				//Now we need to copy across the test steps
				ExportTestSteps(userId, sourceProjectId, sourceTestCaseId, destProjectId, exportedTestCaseId, testCaseMapping);

				//Now we need to copy across any linked attachments
				AttachmentManager attachmentManager = new AttachmentManager();
				attachmentManager.Export(sourceProjectId, Artifact.ArtifactTypeEnum.TestCase, sourceTestCaseId, destProjectId, exportedTestCaseId, attachmentsMapping);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the test case id of the copy
				return exportedTestCaseId;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Physically copies an automation script to the new project
		/// </summary>
		/// <param name="sourceProjectId">The id of the source project</param>
		/// <param name="automationAttachmentId">The id of the script</param>
		/// <returns>The id of the script in the new project</returns>
		/// <param name="destProjectId">The id of the destination project</param>
		protected int? ExportAutomationScript(int sourceProjectId, int automationAttachmentId, int destProjectId)
		{
			int? newAttachmentId = null;

			//We need to get the source and destination project templates
			//If they are the same, then certain additional values can get copied across
			int sourceProjectTemplateId = new TemplateManager().RetrieveForProject(sourceProjectId).ProjectTemplateId;
			int destProjectTemplateId = new TemplateManager().RetrieveForProject(destProjectId).ProjectTemplateId;
			bool templatesSame = (sourceProjectTemplateId == destProjectTemplateId);

			//First retrieve the current script
			AttachmentManager attachmentManager = new AttachmentManager();
			Attachment attachment = attachmentManager.RetrieveById(automationAttachmentId);
			if (attachment != null)
			{
				//See if we have URL or File (ignore source code)
				if (attachment.AttachmentTypeId == (int)Attachment.AttachmentTypeEnum.File)
				{
					//See if we have a Rapise style repository script
					//those require that we copy the entire folder
					//See if we have Rapise/SmarteStudio as they use 'repository' files
					if (!String.IsNullOrEmpty(attachment.Filename) && attachment.Filename.EndsWith(REPOSITORY_TEST_EXTENSION))
					{
						newAttachmentId = ExportRepositoryScript(sourceProjectId, attachment, destProjectId);
					}
					else
					{
						//Get the binary data
						using (FileStream fileStream = attachmentManager.OpenById(attachment.AttachmentId))
						{
							byte[] attachmentBytes = new byte[fileStream.Length];
							fileStream.Read(attachmentBytes, 0, (int)fileStream.Length);
							fileStream.Close();

							newAttachmentId = attachmentManager.Insert(
								destProjectId,
								attachment.Filename,
								attachment.Description,
								attachment.AuthorId,
								attachmentBytes,
								null,
								Artifact.ArtifactTypeEnum.None,
								null,
								attachment.Tags,
								(templatesSame) ? (int?)attachment.ProjectAttachments[0].DocumentTypeId : null,
								null,
								(templatesSame) ? (int?)attachment.DocumentStatusId : null
								);
						}
					}
				}
				if (attachment.AttachmentTypeId == (int)Attachment.AttachmentTypeEnum.URL)
				{
					newAttachmentId = attachmentManager.Insert(
						destProjectId,
						attachment.Filename,
						attachment.Description,
						attachment.AuthorId,
						null,
						Artifact.ArtifactTypeEnum.None,
						null,
						attachment.Tags,
						(templatesSame) ? (int?)attachment.ProjectAttachments[0].DocumentTypeId : null,
						null,
						(templatesSame) ? (int?)attachment.DocumentStatusId : null
						);
				}
			}

			return newAttachmentId;
		}

		/// <summary>
		/// Copies over a Rapise 'repository' style script to another project
		/// </summary>
		/// <param name="sourceProjectId">The id of the source project</param>
		/// <param name="sourceAttachment">The .SSTEST file</param>
		/// <param name="destProjectId">The id of the destination project</param>
		/// <returns>The id of the exported .SSTEST file</returns>
		public int? ExportRepositoryScript(int sourceProjectId, Attachment sourceAttachment, int destProjectId)
		{
			const string METHOD_NAME = "ExportRepositoryScript";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//We need to get the source and destination project templates
				//If they are the same, then certain additional values can get copied across
				int sourceProjectTemplateId = new TemplateManager().RetrieveForProject(sourceProjectId).ProjectTemplateId;
				int destProjectTemplateId = new TemplateManager().RetrieveForProject(destProjectId).ProjectTemplateId;
				bool templatesSame = (sourceProjectTemplateId == destProjectTemplateId);

				int? ssTestFileAttachmentId = null;
				//We need to get the folder containing this attachment
				AttachmentManager attachmentManager = new AttachmentManager();
				ProjectAttachment projectAttachment = attachmentManager.RetrieveForProjectById(sourceProjectId, sourceAttachment.AttachmentId);
				ProjectAttachmentFolder sourceAttachmentFolder = attachmentManager.RetrieveFolderById(projectAttachment.ProjectAttachmentFolderId);

				//Export the folder as a top-level folder under the root
				int rootFolder = attachmentManager.GetDefaultProjectFolder(destProjectId);
				int destAttachmentFolder = attachmentManager.InsertProjectAttachmentFolder(destProjectId, sourceAttachmentFolder.Name, rootFolder);

				//Now export all of the files in the folder
				List<ProjectAttachmentView> childAttachments = attachmentManager.RetrieveForProject(sourceProjectId, sourceAttachmentFolder.ProjectAttachmentFolderId, null, true, 1, Int32.MaxValue, null, 0);
				foreach (ProjectAttachmentView childAttachment in childAttachments)
				{
					//Get the binary data
					using (FileStream fileStream = attachmentManager.OpenById(childAttachment.AttachmentId))
					{
						byte[] attachmentBytes = new byte[fileStream.Length];
						fileStream.Read(attachmentBytes, 0, (int)fileStream.Length);
						fileStream.Close();

						int newAttachmentId = attachmentManager.Insert(
							destProjectId,
							childAttachment.Filename,
							childAttachment.Description,
							childAttachment.AuthorId,
							attachmentBytes,
							null,
							Artifact.ArtifactTypeEnum.None,
							null,
							childAttachment.Tags,
							(templatesSame) ? (int?)childAttachment.DocumentTypeId : null,
							destAttachmentFolder,
							(templatesSame) ? (int?)childAttachment.DocumentStatusId : null
							);

						//See if this is the .SSTEST file
						if (!String.IsNullOrEmpty(childAttachment.Filename) && childAttachment.Filename.EndsWith(REPOSITORY_TEST_EXTENSION))
						{
							ssTestFileAttachmentId = newAttachmentId;
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return ssTestFileAttachmentId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Creates a new test case and links it into the current test case
		/// </summary>
		/// <param name="userId">The id of the user making the change</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="testCaseId">The id of the current test case</param>
		/// <param name="testStepId">The id of the test step to insert before (null = at end)</param>
		/// <param name="testCaseFolderId">The id of the folder to put the new test case in (null = root)</param>
		/// <param name="testCaseName">The name of the new test case</param>
		/// <param name="newParameters">The parameters to use</param>
		/// <returns>The id of the new linked test case step</returns>
		public int TestCase_CreateNewLinkedTestCase(int userId, int projectId, int testCaseId, int? testStepId, int? testCaseFolderId, string testCaseName, Dictionary<string, string> newParameters)
		{
			const string METHOD_NAME = "TestCase_CreateNewLinkedTestCase";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First retrieve the current test case and locate the current step
				TestCase testCaseToImportInto = RetrieveByIdWithSteps(projectId, testCaseId);
				int? position = null;
				if (testStepId.HasValue)
				{
					TestStep testStep = testCaseToImportInto.TestSteps.FirstOrDefault(s => s.TestStepId == testStepId.Value);
					if (testStep != null)
					{
						position = testStep.Position;
					}
				}

				//Get the project settings collection
				ProjectSettings projectSettings = null;
				if (projectId > 0)
				{
					projectSettings = new ProjectSettings(projectId);
				}

				//First create the new test case in the specified folder (default type)
				int newLinkedTestCaseId = Insert(userId,
					projectId,
					userId,
					null,
					testCaseName,
					null,
					null,
					TestCase.TestCaseStatusEnum.Draft,
					null,
					testCaseFolderId,
					null,
					null,
					null,
					true,
					projectSettings != null ? projectSettings.Testing_CreateDefaultTestStep : false
					);

				//Next add the parameters and the values as the default value
				if (newParameters != null)
				{
					foreach (KeyValuePair<string, string> kvp in newParameters)
					{
						string parameterName = kvp.Key;
						string parameterValue = kvp.Value;
						InsertParameter(projectId, newLinkedTestCaseId, parameterName, parameterValue, userId);
					}
				}

				//Next create the linked step using this new test case and any parameters that have a value set
				Dictionary<string, string> parameterValues = new Dictionary<string, string>();
				if (newParameters != null)
				{
					foreach (KeyValuePair<string, string> kvp in newParameters)
					{
						string parameterName = kvp.Key;
						string parameterValue = kvp.Value;
						if (!String.IsNullOrWhiteSpace(parameterValue))
						{
							parameterValues.Add(parameterName, parameterValue);
						}
					}
				}
				int newLinkedTestStepId = InsertLink(userId, testCaseId, position, newLinkedTestCaseId, parameterValues, true);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return newLinkedTestStepId;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Imports all of the (non-linked) steps in a test case into the current test case
		/// </summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="testCaseId">the id of the current test case</param>
		/// <param name="testStepId">The id of the current test step to insert before (null = at end)</param>
		/// <param name="testCaseToImportId">The id of the test case to import the steps from</param>
		/// <param name="userId">The user making the change</param>
		public void TestCase_ImportSteps(int projectId, int testCaseId, int? testStepId, int testCaseToImportId, int userId)
		{
			const string METHOD_NAME = "TestCase_ImportSteps";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First retrieve the current test case and locate the current step
				TestCase testCaseToImportInto = RetrieveByIdWithSteps(projectId, testCaseId);
				int? position = null;
				if (testStepId.HasValue)
				{
					TestStep testStep = testCaseToImportInto.TestSteps.FirstOrDefault(s => s.TestStepId == testStepId.Value);
					if (testStep != null)
					{
						position = testStep.Position;
					}
				}

				//Retrieve the test case we're importing from
				TestCase testCaseToImportFrom = RetrieveByIdWithSteps(projectId, testCaseToImportId);

				//Loop through the steps and add one at a time (not linked steps)
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				foreach (TestStep testStep in testCaseToImportFrom.TestSteps)
				{
					if (!testStep.LinkedTestCaseId.HasValue)
					{
						//Insert the step
						int newTestStepId = InsertStep(userId, testCaseId, position, testStep.Description, testStep.ExpectedResult, testStep.SampleData);
						//Increment the position if provided
						if (position.HasValue)
						{
							position = position.Value + 1;
						}

						//Get the template for this project
						int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

						//Copy across any custom property values
						customPropertyManager.ArtifactCustomProperty_Copy(projectId, projectTemplateId, testStep.TestStepId, newTestStepId, Artifact.ArtifactTypeEnum.TestStep, userId);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Copies a test case (and its test steps) from one location to another. Will not copy deleted test cases.</summary>
		/// <param name="userId">The user that is performing the copy</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="sourceTestCaseId">The test case we want to copy</param>
		/// <param name="destTestCaseFolderId">The folder we want to copy it into (null = root)</param>
		/// <param name="prependName">Should we prepend 'Copy of ...' to name</param>
		/// <returns>The id of the copy of the test case</returns>
		public int TestCase_Copy(int userId, int projectId, int sourceTestCaseId, int? destTestCaseFolderId, bool prependName = true)
		{
			const string METHOD_NAME = "TestCase_Copy";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the test case being copied
				TestCase sourceTestCase = RetrieveById2(projectId, sourceTestCaseId);

				//Insert a new test case with the data copied from the existing one
				//** We do not copy across the test automation info - since that causes issues if the user
				//changes the test script in one test case that affects the other (change in v3.0 patch 11)
				string name = sourceTestCase.Name;
				if (prependName)
				{
					name = name + CopiedArtifactNameSuffix;
				}

				//Handle components
				List<int> componentIds = null;
				if (!String.IsNullOrEmpty(sourceTestCase.ComponentIds))
				{
					componentIds = sourceTestCase.ComponentIds.FromDatabaseSerialization_List_Int32();
				}

				int copiedTestCaseId = Insert(
					userId,
					projectId,
					sourceTestCase.AuthorId,
					sourceTestCase.OwnerId,
					name,
					sourceTestCase.Description,
					sourceTestCase.TestCaseTypeId,
					(TestCase.TestCaseStatusEnum)sourceTestCase.TestCaseStatusId,
					sourceTestCase.TestCasePriorityId,
					destTestCaseFolderId,
					sourceTestCase.EstimatedDuration,
					null,
					null,
					true,
					false,
					componentIds
					);

				//Now we need to copy across any test case parameters
				List<TestCaseParameter> testCaseParameters = RetrieveParameters(sourceTestCaseId);
				foreach (TestCaseParameter testCaseParameter in testCaseParameters)
				{
					InsertParameter(
						projectId,
						copiedTestCaseId,
						testCaseParameter.Name,
						testCaseParameter.DefaultValue, userId);
				}

				//Now we need to copy across any coverage information
				CopyCoverage(projectId, sourceTestCaseId, copiedTestCaseId, userId);

				//Now we need to copy across the test steps
				CopyTestSteps(userId, projectId, sourceTestCaseId, copiedTestCaseId);

				//Get the template for this project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Now we need to copy across any custom properties
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				customPropertyManager.ArtifactCustomProperty_Copy(projectId, projectTemplateId, sourceTestCaseId, copiedTestCaseId, Artifact.ArtifactTypeEnum.TestCase, userId);

				//Now we need to copy across any linked attachments
				AttachmentManager attachmentManager = new AttachmentManager();
				attachmentManager.Copy(projectId, Artifact.ArtifactTypeEnum.TestCase, sourceTestCaseId, copiedTestCaseId);

				//Send a notification
				SendCreationNotification(copiedTestCaseId, null, null);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the test case id of the copy
				return copiedTestCaseId;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Copies a test folder (and its child test cases) from one location to another. Will not copy deleted test cases.</summary>
		/// <param name="userId">The user that is making the copy</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="sourceTestFolderId">The folder we want to copy</param>
		/// <param name="destTestCaseFolderId">The folder we want to copy it into (null = root)</param>
		/// <param name="prependName">Should we prepend 'Copy of ...' to name</param>
		/// <returns>The id of the copy of the test folder</returns>
		public int TestCaseFolder_Copy(int userId, int projectId, int sourceTestFolderId, int? destTestCaseFolderId, bool prependName = true)
		{
			const string METHOD_NAME = "TestCaseFolder_Copy";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the test folder being copied
				TestCaseFolder sourceTestCaseFolder = TestCaseFolder_GetById(sourceTestFolderId);

				string name = sourceTestCaseFolder.Name;
				if (prependName)
				{
					name = name + CopiedArtifactNameSuffix;
				}

				//Firstly copy the test folder itself
				int copiedTestFolderId = TestCaseFolder_Create(
					name,
					projectId,
					sourceTestCaseFolder.Description,
					destTestCaseFolderId).TestCaseFolderId;

				//Next we need to copy all the child test cases
				List<TestCaseView> childTestCases = Retrieve(projectId, "Name", true, 1, Int32.MaxValue, null, 0, sourceTestFolderId);
				foreach (TestCaseView childTestCase in childTestCases)
				{
					//Copy the test case, leaving its name unchanged
					TestCase_Copy(userId, projectId, childTestCase.TestCaseId, copiedTestFolderId, false);
				}

				//Next we need to recursively call this function for any child test case folders
				List<TestCaseFolder> childTestFolders = TestCaseFolder_GetByParentId(projectId, sourceTestFolderId);
				if (childTestFolders != null && childTestFolders.Count > 0)
				{
					foreach (TestCaseFolder childTestFolder in childTestFolders)
					{
						//Copy the folder into the new folder, leaving the name unchanged
						TestCaseFolder_Copy(userId, projectId, childTestFolder.TestCaseFolderId, copiedTestFolderId, false);
					}
				}

				//Finally refresh the folder hierarchy cache
				TestCaseFolder_RefreshHierarchy(projectId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the test case id of the copy
				return copiedTestFolderId;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Creates a new testCase folder</summary>
		/// <param name="name">Name of the folder.</param>
		/// <param name="description">The description of the folder</param>
		/// <returns>The newly created testCase folder</returns>
		/// <param name="projectId">The id of the project</param>
		/// <param name="parentTestCaseFolderId">The id of a parent folder (null = top-level folder)</param>
		public TestCaseFolder TestCaseFolder_Create(string name, int projectId, string description, int? parentTestCaseFolderId = null)
		{
			const string METHOD_NAME = "TestCaseFolder_Create";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			TestCaseFolder testCaseFolder = new TestCaseFolder();

			try
			{

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create a new testCase folder entity
					Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Creating new TestCase Folder Entity");

					testCaseFolder.Name = name;
					testCaseFolder.Description = description;
					testCaseFolder.ProjectId = projectId;
					testCaseFolder.ParentTestCaseFolderId = parentTestCaseFolderId;
					testCaseFolder.CountBlocked = 0;
					testCaseFolder.CountCaution = 0;
					testCaseFolder.CountFailed = 0;
					testCaseFolder.CountNotApplicable = 0;
					testCaseFolder.CountNotRun = 0;
					testCaseFolder.CountPassed = 0;
					testCaseFolder.LastUpdateDate = DateTime.UtcNow;

					//Persist the new article folder
					context.TestCaseFolders.AddObject(testCaseFolder);
					context.SaveChanges();

					//Persist changes
					context.SaveChanges();
				}

				//Finally refresh the folder hierarchy cache
				TestCaseFolder_RefreshHierarchy(projectId);
			}
			catch (EntityForeignKeyException exception)
			{
				//This exception occurs if the parent has been deleted, throw a business exception
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw new ArtifactNotExistsException("The parent testCase folder specified no longer exists", exception);
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return testCaseFolder;
		}

		/// <summary>
		/// Gets a folder by its id
		/// </summary>
		/// <param name="folderId">The folder id</param>
		/// <returns>A folder or null if it doesn't exist</returns>
		public TestCaseFolder TestCaseFolder_GetById(int folderId)
		{
			const string METHOD_NAME = "TestCaseFolder_GetById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TestCaseFolder folder;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from f in context.TestCaseFolders
								where f.TestCaseFolderId == folderId
								select f;
					folder = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return folder;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary> 
		/// Checks if a folder exists by its id in the specified project 
		/// </summary> 
		/// <param name="projectId">The folder id</param> 
		/// <param name="folderId">The folder id</param> 
		/// <returns>bool of true if the folder exists in the project</returns> 
		public bool TestCaseFolder_Exists(int projectId, int folderId)
		{
			const string METHOD_NAME = "TestCaseFolder_Exists";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TestCaseFolder folder;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from f in context.TestCaseFolders
								where f.TestCaseFolderId == folderId && f.ProjectId == projectId
								select f;
					folder = query.FirstOrDefault();
				}

				//Make sure data was returned 
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return folder == null ? false : true;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception, "Looking for TestCaseFolder");
				throw;
			}
		}

		/// <summary>
		/// Refreshes the test folder hierachy in a project after folders are changed
		/// </summary>
		/// <param name="projectId">The ID of the current project</param>
		public void TestCaseFolder_RefreshHierarchy(int projectId)
		{
			const string METHOD_NAME = "TestCaseFolder_RefreshHierarchy";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Set a longer timeout for this as it's run infrequently to speed up retrieves
					context.CommandTimeout = SQL_COMMAND_TIMEOUT_CACHE_UPDATE;
					context.TestCase_RefreshFolderHierarchy(projectId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				//Don't throw, just log
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				//Don't throw, just log
			}
		}

		/// <summary>
		/// Gets the list of all folders in the project according to their hierarchical relationship, in alphabetical order (per-level)
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>List of folders</returns>
		public List<TestCaseFolderHierarchyView> TestCaseFolder_GetList(int projectId)
		{
			const string METHOD_NAME = "TestCaseFolder_GetList";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestCaseFolderHierarchyView> folders;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from f in context.TestCaseFoldersHierarchyView
								where f.ProjectId == projectId
								orderby f.IndentLevel, f.TestCaseFolderId
								select f;

					folders = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return folders;
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Gets the list of all parents of the specified folder in hierarchy order
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>List of folders</returns>
		public List<TestCaseFolderHierarchyView> TestCaseFolder_GetParents(int projectId, int testFolderId, bool includeSelf = false)
		{
			const string METHOD_NAME = "TestCaseFolder_GetParents";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestCaseFolderHierarchyView> folders;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					folders = context.TestCase_RetrieveParentFolders(projectId, testFolderId, includeSelf).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return folders;
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Gets the list of all children of the specified folder in hierarchy order
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>List of folders</returns>
		public List<TestCaseFolderHierarchyView> TestCaseFolder_GetChildren(int projectId, int testFolderId, bool includeSelf = false)
		{
			const string METHOD_NAME = "TestCaseFolder_GetChildren";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestCaseFolderHierarchyView> folders;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					folders = context.TestCase_RetrieveChildFolders(projectId, testFolderId, includeSelf).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return folders;
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		#region Test Case Priorities

		/// <summary>Retrieves a list of test case priorities</summary>
		/// <param name="activeOnly">Do we only want active ones</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <returns>List of test case priorities</returns>
		public List<TestCasePriority> TestCasePriority_Retrieve(int projectTemplateId, bool activeOnly = true)
		{
			const string METHOD_NAME = "TestCasePriority_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestCasePriority> priorities;

				//Create select command for retrieving the lookup data
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.TestCasePriorities
								where (p.IsActive || !activeOnly) && p.ProjectTemplateId == projectTemplateId
								orderby p.TestCasePriorityId, p.Score
								select p;

					priorities = query.OrderByDescending(i => i.TestCasePriorityId).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return priorities;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a test case priority by its id</summary>
		/// <param name="priorityId">The id of the priority</param>
		/// <returns>Untyped dataset of test case priority</returns>
		public TestCasePriority TestCasePriority_RetrieveById(int priorityId)
		{
			const string METHOD_NAME = "TestCasePriority_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TestCasePriority priority;

				//Create select command for retrieving the lookup data
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.TestCasePriorities
								where p.TestCasePriorityId == priorityId
								select p;

					priority = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return priority;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Updates the testCase priorities for a project</summary>
		/// <param name="testCasePriority">The testCase priority to be persisted</param>
		/// <remarks>This method performs the necessary updates</remarks>
		public void TestCasePriority_Update(TestCasePriority testCasePriority)
		{
			const string METHOD_NAME = "TestCasePriority_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Apply the changes
					context.TestCasePriorities.ApplyChanges(testCasePriority);
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Deletes a testCase priority</summary>
		/// <param name="priorityId">The priority to be deleted</param>
		/// <remarks>Only used by the unit tests for cleanup purposes</remarks>
		protected internal void TestCasePriority_Delete(int priorityId)
		{
			const string METHOD_NAME = "TestCasePriority_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the type
					var query = from t in context.TestCasePriorities
								where t.TestCasePriorityId == priorityId
								select t;

					TestCasePriority priority = query.FirstOrDefault();
					if (priority != null)
					{
						context.TestCasePriorities.DeleteObject(priority);
						context.SaveChanges();
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Inserts a new testCase priority for a specific project template</summary>
		/// <param name="projectTemplateId">The project template that the testCase priority belongs to</param>
		/// <param name="name">The display name of the testCase priority</param>
		/// <param name="active">Whether the testCase priority is active or not</param>
		/// <param name="color">The color code for the priority (in rrggbb hex format)</param>
		/// <param name="score">The numeric score value (weight) of the priority</param>
		/// <returns>The newly created testCase priority id</returns>
		public int TestCasePriority_Insert(int projectTemplateId, string name, string color, bool active, int score = 0)
		{
			const string METHOD_NAME = "TestCasePriority_Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int priorityId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Fill out entity with data for new testCase priority
					TestCasePriority testCasePriority = new TestCasePriority();
					testCasePriority.ProjectTemplateId = projectTemplateId;
					testCasePriority.Name = name.MaxLength(20);
					testCasePriority.Color = color.MaxLength(6);
					testCasePriority.IsActive = active;
					testCasePriority.Score = score;

					context.TestCasePriorities.AddObject(testCasePriority);
					context.SaveChanges();
					priorityId = testCasePriority.TestCasePriorityId;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return priorityId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}



		/// <summary>Retrieves a list of test case priorities</summary>
		/// <param name="activeOnly">Do we only want active ones</param>
		/// <returns>List of test case priorities</returns>
		public List<TestCasePreparationStatus> TestCasePreparation_Retrieve(bool activeOnly = true)
		{
			const string METHOD_NAME = "TestCasePreparation_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestCasePreparationStatus> preparations;

				//Create select command for retrieving the lookup data
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.TestCasePreparationStatus //.TestCasePriorities
								where (p.IsActive || !activeOnly) // && p.ProjectTemplateId == projectTemplateId
								orderby p.Position
								select p;

					preparations = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return preparations;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a test case priority by its id</summary>
		/// <param name="TestCasePreparationId">The id of the priority</param>
		/// <returns>Untyped dataset of test case priority</returns>
		public TestCasePreparationStatus TestCasePreparation_RetrieveById(int testCasePreparationId)
		{
			const string METHOD_NAME = "TestCasePreparation_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TestCasePreparationStatus testCasePreparation;

				//Create select command for retrieving the lookup data
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.TestCasePreparationStatus
								where p.TestCasePreparationStatusId == testCasePreparationId
								select p;

					testCasePreparation = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testCasePreparation;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Updates the testCase priorities for a project</summary>
		/// <param name="TestCasePreparation">The testCase testCasePreparation to be persisted</param>
		/// <remarks>This method performs the necessary updates</remarks>
		public void TestCasePreparation_Update(TestCasePreparationStatus testCasePreparation)
		{
			const string METHOD_NAME = "TestCasePreparation_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Apply the changes
					context.TestCasePreparationStatus.ApplyChanges(testCasePreparation);
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Deletes a testCase testCasePreparation</summary>
		/// <param name="TestCasePreparationId">The testCasePreparation to be deleted</param>
		/// <remarks>Only used by the unit tests for cleanup purposes</remarks>
		protected internal void TestCasePreparation_Delete(int testCasePreparationId)
		{
			const string METHOD_NAME = "TestCasePreparation_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the type
					var query = from t in context.TestCasePreparationStatus
								where t.TestCasePreparationStatusId == testCasePreparationId
								select t;

					TestCasePreparationStatus testCasePreparation = query.FirstOrDefault();
					if (testCasePreparation != null)
					{
						context.TestCasePreparationStatus.DeleteObject(testCasePreparation);
						context.SaveChanges();
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Inserts a new testCase testCasePreparation for a specific project template</summary>
		/// <param name="projectTemplateId">The project template that the testCase testCasePreparation belongs to</param>
		/// <param name="name">The display name of the testCase testCasePreparation</param>
		/// <param name="active">Whether the testCase testCasePreparation is active or not</param>
		/// <param name="color">The color code for the testCasePreparation (in rrggbb hex format)</param>
		/// <param name="score">The numeric score value (weight) of the testCasePreparation</param>
		/// <returns>The newly created testCase testCasePreparation id</returns>
		public int TestCasePreparation_Insert(string name, bool active, int position)
		{
			const string METHOD_NAME = "TestCasePreparation_Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int TestCasePreparationId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Fill out entity with data for new testCase testCasePreparation
					TestCasePreparationStatus testCasePreparation = new TestCasePreparationStatus();
					testCasePreparation.Name = name.MaxLength(50);
					testCasePreparation.IsActive = active;
					testCasePreparation.Position = position;

					context.TestCasePreparationStatus.AddObject(testCasePreparation);
					context.SaveChanges();
					TestCasePreparationId = testCasePreparation.TestCasePreparationStatusId;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return TestCasePreparationId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}



		#endregion

		#region Execution Status

		/// <summary>Retrieves a list of test-case execution status summary for a project / release</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="releaseId">The release we want to filter on (null for all, -2 = active releases only)</param>
		/// <returns>Untyped dataset of test case execution summary</returns>
		/// <remarks>Always returns all the execution status codes</remarks>
		public List<TestCase_ExecutionStatusSummary> RetrieveExecutionStatusSummary(int projectId, int? releaseId)
		{
			const string METHOD_NAME = "RetrieveExecutionStatusSummary";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Execute the stored procedure
				List<TestCase_ExecutionStatusSummary> executionStatiSummary;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//release Id = -2 ==> All Active Releases
					executionStatiSummary = context.TestCase_RetrieveExecutionStatusSummary_Project(projectId, releaseId).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return executionStatiSummary;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list of test-case execution status summary for a project / release</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <returns>A Dictionary</returns>
		/// <remarks>Always returns a Dictionary of Author and TestCase count</remarks>
		public System.Data.DataSet RetrieveTestPreparationSummary(int projectId)
		{
			const string METHOD_NAME = "RetrieveTestPreparationSummary";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Execute the stored procedure
				Dictionary<string, int> executionStatiSummary = new Dictionary<string, int>();
				DataSet executionSummary = null;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{

					//					string sql = @"SELECT CONCAT(u.First_Name, ' ', u.LAST_NAME) as UserName, 
					//		 SUM(CASE WHEN tc.TEST_CASE_STATUS_ID=1 THEN 1 ELSE 0 END) AS DRAFT

					//FROM TST_TEST_CASE tc
					//join TST_USER_PROFILE u on tc.AUTHOR_ID = u.USER_ID
					//GROUP BY CONCAT(u.First_Name,' ', u.LAST_NAME)";

					string sql = @"SELECT [Not Recorded],[Recorded - Failed],[Recorded - Issue],[Recorded - Passed] FROM 
								(
									SELECT tc.TEST_CASE_PREPARATION_STATUS_ID, tcps.[NAME]
									FROM  TST_TEST_CASE tc INNER JOIN TST_TEST_CASE_PREPARATION_STATUS tcps ON tc.TEST_CASE_PREPARATION_STATUS_ID = tcps.TEST_CASE_PREPARATION_STATUS_ID AND TC.Project_ID = 2
								) x
								pivot 
								(
									COUNT(TEST_CASE_PREPARATION_STATUS_ID)
									for [NAME] in ([Not Recorded],[Recorded - Failed],[Recorded - Issue],[Recorded - Passed])
								) p ";
					executionSummary = ExecuteSql(context, sql, "ExecutionSummary");
					//DataColumn[] primaryKeys = new DataColumn[1];
					//primaryKeys[0] = executionSummary.Tables[0].Columns[0];
					//executionSummary.Tables[0].PrimaryKey = primaryKeys;
					//executionSummary.Tables[0].Columns[0].Caption = "Test Prep Status";
					//executionSummary.Tables[0].Columns[0].ExtendedProperties.Add("Test Prep Status", "Test Prep Status");

					//executionSummary.Tables[0].Columns[1].Caption = "FUNCTIONAL";
					//executionSummary.Tables[0].Columns[1].ExtendedProperties.Add("Color", TestCaseManager.GetTestCaseStatusColor(4));

					//executionSummary.Tables[0].Columns[2].Caption = "SCENARIO";
					//executionSummary.Tables[0].Columns[2].ExtendedProperties.Add("Color", TestCaseManager.GetTestCaseStatusColor(3));

					//executionSummary.Tables[0].Columns[3].Caption = "PERFORMANCE";
					//executionSummary.Tables[0].Columns[3].ExtendedProperties.Add("Color", TestCaseManager.GetTestCaseStatusColor(1));

					////executionSummary.Tables[0].Columns[3].Caption = "Not Run";
					////executionSummary.Tables[0].Columns[3].ExtendedProperties.Add("Color", TestCaseManager.GetExecutionStatusColor(3));


					////executionSummary.Tables[0].Columns[5].Caption = "Caution";
					////executionSummary.Tables[0].Columns[5].ExtendedProperties.Add("Color", TestCaseManager.GetExecutionStatusColor(5));
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return executionSummary;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list of test-case execution status summary for an entire project group</summary>
		/// <param name="projectGroupId">The project group we're interested in</param>
		/// <param name="activeReleasesOnly">Do we only want active releases</param>
		/// <returns>Untyped dataset of test case execution summary</returns>
		/// <remarks>Always returns all the execution status codes</remarks>
		public List<TestCase_ExecutionStatusSummary> RetrieveExecutionStatusSummary(int projectGroupId, bool activeReleasesOnly)
		{
			const string METHOD_NAME = "RetrieveExecutionStatusSummary";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Execute the stored procedure
				List<TestCase_ExecutionStatusSummary> executionStatiSummary;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					executionStatiSummary = context.TestCase_RetrieveExecutionStatusSummary_Group(projectGroupId, activeReleasesOnly).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return executionStatiSummary;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Returns the name of a test case execution status by its id (non-localized)
		/// </summary>
		/// <param name="executionStatusId">The id of the execution status</param>
		/// <returns>The name of the status (non-localized)</returns>
		public string GetExecutionStatusName(int executionStatusId)
		{
			ExecutionStatus executionStatus = RetrieveExecutionStatusById(executionStatusId);
			if (executionStatus == null)
			{
				return "";
			}
			return executionStatus.Name;
		}

		/// <summary>
		/// Gets the HTML color code for a specific execution status (used in graphs)
		/// </summary>
		/// <param name="executionStatusId">The execution status</param>
		/// <returns>The html hex color code</returns>
		public static string GetExecutionStatusColor(int? executionStatusId)
		{
			if (executionStatusId.HasValue)
			{
				switch (executionStatusId.Value)
				{
					case (int)TestCase.ExecutionStatusEnum.Failed:
						return "f47457";

					case (int)TestCase.ExecutionStatusEnum.Passed:
						return "7eff7a";

					case (int)TestCase.ExecutionStatusEnum.Blocked:
						return "f4f356";

					case (int)TestCase.ExecutionStatusEnum.Caution:
						return "f29e56";

					case (int)TestCase.ExecutionStatusEnum.NotApplicable:
						return "d0d0d0";

					case (int)TestCase.ExecutionStatusEnum.NotRun:
						return "e0e0e0";

					default:
						return "";
				}
			}
			else
			{
				return "d0d0d0";
			}
		}

		public static string GetTestCaseStatusColor(int? coverageId)
		{
			if (coverageId.HasValue)
			{
				switch (coverageId.Value)
				{
					case /* Passed */ 1:
						return "7eff7a";

					case /* Failed */ 2:
						return "f47457";

					case /* Blocked */ 3:
						return "f4f356";

					case /*Caution*/ 4:
						return "f29e56";

					case /* Not Run */ 5:
						return "e0e0e0";

					case /* Not Covered */ 6:
						return "bbebfe";

					default:
						return "d0d0d0";
				}
			}
			else
			{
				return "d0d0d0";
			}
		}


		/// <summary>Retrieves a specific execution status by its id</summary>
		/// <param name="executionStatusId">The id of the execution status</param>
		/// <returns>Untyped dataset of execution statuses</returns>
		public ExecutionStatus RetrieveExecutionStatusById(int executionStatusId)
		{
			const string METHOD_NAME = "RetrieveExecutionStatusById";


			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ExecutionStatus executionStatus;

				//Create select command for retrieving the lookup data
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from e in context.ExecutionStati
								where e.ExecutionStatusId == executionStatusId
								select e;

					executionStatus = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return executionStatus;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list of execution statuses</summary>
		/// <returns>Untyped dataset of execution statuses</returns>
		public List<ExecutionStatus> RetrieveExecutionStatuses()
		{
			const string METHOD_NAME = "RetrieveExecutionStatuses";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ExecutionStatus> executionStati;

				//Create select command for retrieving the lookup data
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from e in context.ExecutionStati
								where e.IsActive
								orderby e.ExecutionStatusId
								select e;

					executionStati = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return executionStati;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Test Case Types

		/// <summary>Retrieves a list of test case types</summary>
		/// <returns>List of test case types</returns>
		/// <param name="activeOnly">Do we only want active ones</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		public List<TestCaseType> TestCaseType_Retrieve(int projectTemplateId, bool activeOnly = true)
		{
			const string METHOD_NAME = "TestCaseType_Retrieve()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				List<TestCaseType> testCaseTypes;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestCaseTypes
								where (t.IsActive || !activeOnly) && t.ProjectTemplateId == projectTemplateId
								orderby t.TestCaseTypeId, t.Position
								select t;

					testCaseTypes = query.OrderByDescending(i => i.TestCaseTypeId).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testCaseTypes;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a test case type by its id</summary>
		/// <returns>List of test case types</returns>
		public TestCaseType TestCaseType_RetrieveById(int typeId)
		{
			const string METHOD_NAME = "TestCaseType_RetrieveById()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TestCaseType type;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestCaseTypes
								where t.TestCaseTypeId == typeId
								select t;

					type = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return type;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Inserts a new testCase type for a specific project template</summary>
		/// <param name="projectTemplateId">The project template that the testCase type belongs to</param>
		/// <param name="name">The display name of the testCase type</param>
		/// <param name="active">Whether the testCase type is active or not</param>
		/// <param name="workflowId">The workflow id (pass null for project default)</param>
		/// <param name="defaultType">Is this the default (initial) type of newly created testCases</param>
		/// <param name="isExploratory">Is this an exploratory test case (executed differently)</param>
		/// <param name="position">The position (null = at end)</param>
		/// <returns>The newly created testCase type id</returns>
		public int TestCaseType_Insert(int projectTemplateId, string name, int? workflowId, bool defaultType, bool active, bool isExploratory = false, int? position = null)
		{
			const string METHOD_NAME = "TestCaseType_Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//If no workflow provided, simply use the project default workflow
				if (!workflowId.HasValue)
				{
					TestCaseWorkflowManager workflowManager = new TestCaseWorkflowManager();
					workflowId = workflowManager.Workflow_GetDefault(projectTemplateId).TestCaseWorkflowId;
				}

				int testCaseTypeId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the last position if none specified
					if (!position.HasValue)
					{
						var query = from t in context.TestCaseTypes
									where t.ProjectTemplateId == projectTemplateId && t.IsActive
									orderby t.Position descending
									select t;

						TestCaseType lastTestCaseType = query.FirstOrDefault();
						if (lastTestCaseType == null)
						{
							//position = 1;
							position = 0;
						}
						else
						{
							position = lastTestCaseType.Position + 1;
						}
					}

					TestCaseType testCaseType = new TestCaseType();
					testCaseType.ProjectTemplateId = projectTemplateId;
					testCaseType.Name = name.MaxLength(20);
					testCaseType.IsDefault = defaultType;
					testCaseType.IsActive = active;
					testCaseType.IsExploratory = isExploratory;
					testCaseType.TestCaseWorkflowId = workflowId.Value;
					testCaseType.Position = position.Value;

					context.TestCaseTypes.AddObject(testCaseType);
					context.SaveChanges();
					testCaseTypeId = testCaseType.TestCaseTypeId;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testCaseTypeId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Updates the testCase types for a project</summary>
		/// <param name="testCaseType">The testCase type to be persisted</param>
		/// <remarks>This method performs the necessary updates</remarks>
		public void TestCaseType_Update(TestCaseType testCaseType)
		{
			const string METHOD_NAME = "TestCaseType_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Apply the changes
					context.TestCaseTypes.ApplyChanges(testCaseType);
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Deletes the testCase types for a project template</summary>
		/// <param name="testCaseTypeId">The testCase type to be deleted</param>
		/// <remarks>Only used by the unit tests for cleanup purposes</remarks>
		protected internal void TestCaseType_Delete(int testCaseTypeId)
		{
			const string METHOD_NAME = "TestCaseType_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the type
					var query = from r in context.TestCaseTypes
								where r.TestCaseTypeId == testCaseTypeId
								select r;

					TestCaseType type = query.FirstOrDefault();
					if (type != null)
					{
						context.TestCaseTypes.DeleteObject(type);
						context.SaveChanges();
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the default test case type for the specified template
		/// </summary>
		/// <param name="projectTemplateId">The id of the template</param>
		/// <returns>The default test case type</returns>
		public TestCaseType TestCaseType_RetrieveDefault(int projectTemplateId)
		{
			const string METHOD_NAME = "TestCaseType_RetrieveDefault";


			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TestCaseType type;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from r in context.TestCaseTypes
								where r.ProjectTemplateId == projectTemplateId && r.IsDefault
								select r;

					type = query.FirstOrDefault();
					if (type == null)
					{
						throw new ApplicationException(String.Format(GlobalResources.Messages.TestCase_NoDefaultTypeForProjectTemplate, projectTemplateId));
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return type;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region TestCase Statuses

		/// <summary>Retrieves a list of test case statuses</summary>
		/// <returns>List of test case statuses</returns>
		/// <remarks>Cached since they don't change</remarks>
		public List<TestCaseStatus> RetrieveStatuses()
		{
			const string METHOD_NAME = "RetrieveStatuses()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				if (_staticTestCaseStatuses == null)
				{
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						var query = from t in context.TestCaseStati
									where t.IsActive
									orderby t.Position, t.TestCaseStatusId
									select t;

						_staticTestCaseStatuses = query.ToList();
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return _staticTestCaseStatuses;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a test case status by its id</summary>
		/// <returns>List of test case statuses</returns>
		/// <param name="statusId">The id of the test case status</param>
		public TestCaseStatus RetrieveStatusById(int statusId)
		{
			const string METHOD_NAME = "RetrieveStatusById()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				TestCaseStatus status;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestCaseStati
								where t.TestCaseStatusId == statusId
								select t;

					status = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return status;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Test Case functions

		/// <summary>Creates a new test case from the passed in requirement</summary>
		/// <param name="userId">The user performing the operation</param>
		/// <param name="projectId">The current project</param>
		/// <param name="requirementId">The requirement we're creating the test case from</param>
		/// <param name="testCaseFolderId">The test case folder it should be created in</param>
		/// <returns>The id of the newly created test case</returns>
		public int CreateFromRequirement(int userId, int projectId, int requirementId, int? testCaseFolderId)
		{
			const string METHOD_NAME = "CreateFromRequirement";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//First create a new test case with the requirement id embedded
				RequirementManager requirementManager = new RequirementManager();
				RequirementView requirement = requirementManager.RetrieveById2(projectId, requirementId);
				string testCaseName = GlobalResources.General.Global_Verify + ": " + requirement.Name;

				//Set the test case priority based on the requirement (score value has to match)
				int? testCasePriorityId = null;
				if (requirement.ImportanceId.HasValue)
				{
					Importance importance = requirementManager.RequirementImportance_RetrieveById(requirement.ImportanceId.Value);
					if (importance != null)
					{
						List<TestCasePriority> testCasePriorities = TestCasePriority_Retrieve(projectTemplateId);
						TestCasePriority testCasePriority = testCasePriorities.FirstOrDefault(p => p.Score == importance.Score);
						if (testCasePriority != null)
						{
							testCasePriorityId = testCasePriority.TestCasePriorityId;
						}
					}
				}

				//Handle components
				List<int> componentIds = null;
				if (requirement.ComponentId.HasValue)
				{
					componentIds = new List<int>() { requirement.ComponentId.Value };
				}

				//Get the project settings collection
				ProjectSettings projectSettings = null;
				if (projectId > 0)
				{
					projectSettings = new ProjectSettings(projectId);
				}

				//See if this requirement has steps, if so then don't create the default step regardless of settings
				bool createDefaultTestStep = projectSettings != null ? projectSettings.Testing_CreateDefaultTestStep : false;
				List<RequirementStep> requirementSteps = requirementManager.RetrieveSteps(requirementId);
				if (requirementSteps.Count > 0)
				{
					createDefaultTestStep = false;
				}

				//Create the test case
				int testCaseId = Insert(
					userId,
					projectId,
					userId,
					null,
					testCaseName,
					requirement.Description,
					null,
					TestCase.TestCaseStatusEnum.Draft,
					testCasePriorityId,
					testCaseFolderId,
					null,
					null,
					null,
					true,
					createDefaultTestStep,
					componentIds
					);

				//Add a comment describing the RQ it came from
				DiscussionManager discussionManager = new DiscussionManager();
				string comment = GlobalResources.General.TestCase_NewTestCaseComment + " " + requirement.ArtifactPrefix + requirement.RequirementId;
				discussionManager.Insert(userId, testCaseId, Artifact.ArtifactTypeEnum.TestCase, comment, projectId, true, false);

				//Now associate this new test case with the requirement
				List<int> testCases = new List<int>();
				testCases.Add(testCaseId);
				AddToRequirement(projectId, requirementId, testCases, userId);

				//If this requirement has use-case steps, add matching test steps to the test case
				if (requirementSteps != null && requirementSteps.Count > 0)
				{
					foreach (RequirementStep requirementStep in requirementSteps)
					{
						InsertStep(userId, testCaseId, null, requirementStep.Description, GlobalResources.General.TestStep_WorksAsExpected, null);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testCaseId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Sends a creation notification, typically only used for API creation calls where we need to retrieve and force it as 'added'
		/// </summary>
		/// <param name="testCaseId">The id of the test case</param>
		/// <param name="artifactCustomProperty">The custom property row</param>
		/// <param name="newComment">The new comment (if any)</param>
		/// <remarks>Fails quietly but logs errors</remarks>
		public void SendCreationNotification(int testCaseId, ArtifactCustomProperty artifactCustomProperty, string newComment)
		{
			const string METHOD_NAME = "SendCreationNotification";
			//Send a notification
			try
			{
				TestCaseView notificationArt = RetrieveById(null, testCaseId);
				notificationArt.MarkAsAdded();
				new NotificationManager().SendNotificationForArtifact(notificationArt, artifactCustomProperty, newComment);
			}
			catch (Exception exception)
			{
				//Log, but don't throw;
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
			}
		}

		/// <summary>Inserts a new test case into the system</summary>
		/// <param name="authorId">The user id of the person who created the test case</param>
		/// <param name="userId">The user that's actually creating the test case, for history.</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="ownerId">The owner of the test case</param>
		/// <param name="name">The short description of the test case</param>
		/// <param name="description">the long description of the test case (optional)</param>
		/// <param name="estimatedDuration">The estimated duration to execute - in minutes (optional)</param>
		/// <param name="automationEngineId">The id of the automation engine that the test script uses</param>
		/// <param name="automationAttachmentId">The id of the file/url attachment that stores the test script to be used</param>
		/// <param name="folderId">The folder the test case is being inserted in (null = root)</param>
		/// <param name="logHistory">Should we log history for this insert</param>
		/// <param name="priorityId">What is the priority of this test case (optional)</param>
		/// <param name="status">What is the status of this test case</param>
		/// <param name="typeId">What is the type of this test case (null = default)</param>
		/// <param name="createDefaultTestStep">Should we create a default test step</param>
		/// <param name="componentIds">Component ids</param>
		/// <returns>The ID of the newly created test case</returns>
		public int Insert(int userId, int projectId, int authorId, int? ownerId, string name, string description, int? typeId, TestCase.TestCaseStatusEnum status, int? priorityId, int? folderId, int? estimatedDuration, int? automationEngineId, int? automationAttachmentId, bool logHistory = true, bool createDefaultTestStep = false, List<int> componentIds = null)
		{
			const string METHOD_NAME = "Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Inititialization
			int testCaseId = -1;

			try
			{
				//Serialize the component IDs if provided
				string componentIdList = null;
				if (componentIds != null && componentIds.Count > 0)
				{
					componentIdList = componentIds.ToDatabaseSerialization();
				}

				//If no test case type specified, get the default one for the current project template
				if (!typeId.HasValue)
				{
					int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
					TestCaseType type = TestCaseType_RetrieveDefault(projectTemplateId);
					typeId = type.TestCaseTypeId;
				}

				//Fill out entity with data for new test case
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					TestCase testCase = new TestCase();
					testCase.TestCaseFolderId = folderId;
					testCase.ProjectId = projectId;
					testCase.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotRun;
					testCase.TestCaseStatusId = (int)status;
					testCase.TestCaseTypeId = typeId.Value;
					testCase.TestCasePriorityId = priorityId;
					testCase.AuthorId = authorId;
					testCase.OwnerId = ownerId;
					testCase.AutomationEngineId = automationEngineId;
					testCase.AutomationAttachmentId = automationAttachmentId;
					testCase.Name = name;
					testCase.Description = description;
					testCase.ConcurrencyDate = DateTime.UtcNow;
					testCase.CreationDate = DateTime.UtcNow;
					testCase.LastUpdateDate = DateTime.UtcNow;
					testCase.EstimatedDuration = estimatedDuration;
					testCase.ComponentIds = componentIdList;

					//Save test case and capture ID
					context.TestCases.AddObject(testCase);
					context.SaveChanges();
					testCaseId = testCase.TestCaseId;
				}

				//Add a history record for the inserted test case
				if (logHistory)
					new HistoryManager().LogCreation(projectId, userId, Artifact.ArtifactTypeEnum.TestCase, testCaseId, DateTime.UtcNow);

				//Next rollup any changes in execution status to the folder (if any)
				if (folderId.HasValue)
				{
					RefreshFolderExecutionStatus(projectId, folderId.Value);
				}

				//See if we need to create a default test step
				if (createDefaultTestStep)
				{
					// test case name can be null if creating from scratch from the test case pages
					string testStepDescription = String.IsNullOrWhiteSpace(name) ? GlobalResources.General.Description : name;
					//We use the name of the test case for the description of the first step
					InsertStep(userId, testCaseId, null, testStepDescription, GlobalResources.General.TestCase_DefaultTestStepExpectedResult, null, true);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testCaseId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Updates a test case that is passed-in</summary>
		/// <param name="task">The test case to be persisted</param>
		/// <param name="userId">The user making the change</param>
		/// <param name="rollbackId">Whether the update is a rollback or not. Pass the rollback changeset id if so</param>
		/// <param name="updHistory">Whether or not to update history. Default: TRUE</param>
		/// <remarks>Only use for updating test cases not test steps</remarks>
		public void Update(TestCase testCase, int userId, long? rollbackId = null, bool updHistory = true)
		{
			const string METHOD_NAME = "Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//If we have a null entity just return
			if (testCase == null)
			{
				return;
			}

			try
			{
				int projectId = testCase.ProjectId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Start tracking changes
					testCase.StartTracking();

					//If any of the test cases were recently made active/inactive need to change the exec status to Not-Run
					bool updateReleaseTestStatus = false;
					bool updateReleaseEffort = false;
					bool updateRequirement = false;
					bool updateFolder = false;
					bool updateTestSets = false;
					if (testCase.ChangeTracker.OriginalValues.ContainsKey("TestCaseStatusId"))
					{
						if (testCase.IsActive && testCase.ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.NotApplicable)
						{
							testCase.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotRun;
							updateReleaseTestStatus = true;
							updateRequirement = true;
							updateFolder = true;
						}
						if (!testCase.IsActive && testCase.ExecutionStatusId != (int)TestCase.ExecutionStatusEnum.NotApplicable)
						{
							testCase.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotApplicable;
							updateReleaseTestStatus = true;
							updateRequirement = true;
							updateFolder = true;
						}
					}

					//See if we need to refresh the requirement or release after updating
					if (testCase.ChangeTracker.OriginalValues.ContainsKey("EstimatedDuration"))
					{
						//See if we are including estimated duration in the planning options
						Project project = new ProjectManager().RetrieveById(projectId);
						if (project.IsEffortTestCases)
						{
							updateReleaseEffort = true;
						}
						updateFolder = true;
						updateTestSets = true;   //Also need to update any test sets that use this test case
					}

					//Get the list of mapped releases to update
					List<ReleaseView> mappedReleases = null;
					if (updateReleaseEffort || updateReleaseTestStatus)
					{
						ReleaseManager releaseManager = new ReleaseManager();
						//Need to refresh the linked releases with the new effort
						mappedReleases = releaseManager.RetrieveMappedByTestCaseId(UserManager.UserInternal, projectId, testCase.TestCaseId);
					}

					//Get the list of mapped requirements to update
					List<RequirementView> coveredRequirements = null;
					if (updateRequirement)
					{
						RequirementManager requirementManager = new RequirementManager();
						coveredRequirements = requirementManager.RetrieveCoveredByTestCaseId(UserManager.UserInternal, projectId, testCase.TestCaseId);
					}

					//Update the last-update and concurrency dates
					testCase.LastUpdateDate = DateTime.UtcNow;
					testCase.ConcurrencyDate = DateTime.UtcNow;
					var test = testCase.TestCaseStatusId;
					//Now apply the changes
					context.TestCases.ApplyChanges(testCase);

					//Save the changes, recording any history changes, and sending any notifications
					context.SaveChanges(userId, true, true, rollbackId);

					//Rollup the changes to the folders
					if (updateFolder && testCase.TestCaseFolderId.HasValue)
					{
						RefreshFolderExecutionStatus(projectId, testCase.TestCaseFolderId.Value);
					}
					if (updateFolder && testCase.ChangeTracker.OriginalValues.ContainsKey("TestCaseFolderId") && testCase.ChangeTracker.OriginalValues["TestCaseFolderId"] != null)
					{
						int oldTestCaseFolderId = (int)testCase.ChangeTracker.OriginalValues["TestCaseFolderId"];
						RefreshFolderExecutionStatus(projectId, oldTestCaseFolderId);
					}

					//Now look to see if we need to update the progress/effort info for any requirements
					if (updateRequirement && coveredRequirements != null && coveredRequirements.Count > 0)
					{
						RequirementManager requirementManager = new RequirementManager();
						foreach (RequirementView requirement in coveredRequirements)
						{
							requirementManager.RefreshTaskProgressAndTestCoverage(projectId, requirement.RequirementId);
						}
					}

					//Now look to see if we need to update the progress/effort info for any releases
					if (mappedReleases != null && mappedReleases.Count > 0)
					{
						ReleaseManager releaseManager = new ReleaseManager();
						List<int> releaseIds = mappedReleases.Select(r => r.ReleaseId).ToList();
						releaseManager.RefreshProgressEffortTestStatus(projectId, releaseIds);
					}

					//Update the test sets if necessary
					if (updateTestSets)
					{
						TestSetManager testSetManager = new TestSetManager();
						List<TestSetView> testSets = testSetManager.RetrieveByTestCaseId(projectId, testCase.TestCaseId, "TestSetId", true, 1, Int32.MaxValue, null, 0);
						foreach (TestSetView testSet in testSets)
						{
							testSetManager.TestSet_RefreshExecutionData(projectId, testSet.TestSetId);
						}
					}
				}
			}
			catch (OptimisticConcurrencyException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				//throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Marks a test case as blocked
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="projectId"></param>
		/// <param name="testCaseId"></param>
		/// <remarks>Does not create a test run, just updates the test case itself</remarks>
		public void Block(int userId, int projectId, int testCaseId)
		{
			const string METHOD_NAME = "Block";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Exclude already-blocked test cases
					var query = from t in context.TestCases
								where t.TestCaseId == testCaseId && !t.IsDeleted && t.ExecutionStatusId != (int)TestCase.ExecutionStatusEnum.Blocked
								select t;

					TestCase testCase = query.FirstOrDefault();
					if (testCase != null)
					{
						testCase.StartTracking();
						testCase.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.Blocked;
						testCase.LastUpdateDate = DateTime.UtcNow;
						context.SaveChanges(userId, false, true, null);

						//Rollup the changes to the folders
						if (testCase.TestCaseFolderId.HasValue)
						{
							RefreshFolderExecutionStatus(projectId, testCase.TestCaseFolderId.Value);
						}
					}
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Marks a blocked test case as not-run
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="projectId"></param>
		/// <param name="testCaseId"></param>
		/// <remarks>Does not create a test run, just updates the test case itself</remarks>
		public void UnBlock(int userId, int projectId, int testCaseId)
		{
			const string METHOD_NAME = "UnBlock";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Exclude already-blocked test cases
					var query = from t in context.TestCases
								where t.TestCaseId == testCaseId && !t.IsDeleted && t.ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.Blocked
								select t;

					TestCase testCase = query.FirstOrDefault();
					if (testCase != null)
					{
						testCase.StartTracking();
						//If the test case is in an active status we revert to Not Run, otherwise we mark as N/A
						if (testCase.IsActive)
						{
							testCase.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotRun;
						}
						else
						{
							testCase.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotApplicable;
						}
						testCase.LastUpdateDate = DateTime.UtcNow;
						context.SaveChanges(userId, false, true, null);

						//Rollup the changes to the folders
						if (testCase.TestCaseFolderId.HasValue)
						{
							RefreshFolderExecutionStatus(projectId, testCase.TestCaseFolderId.Value);
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a single test case in the system that has a certain ID together with all its associated test steps.</summary>
		/// <param name="testCaseId">The ID of the test case to be returned</param>
		/// <param name="includeDeletedItems">Should we include deleted items</param>
		/// <returns>Test Case entity, with test steps populated</returns>
		public TestCase RetrieveByIdWithSteps(int? projectId, int testCaseId, bool includeDeletedItems = false)
		{
			const string METHOD_NAME = "RetrieveByIdWithSteps";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TestCase testCase;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving test cases
					var query = from t in context.TestCases
								.Include(t => t.ExecutionStatus)
								.Include(t => t.Status)
								.Include(t => t.Type)
								where t.TestCaseId == testCaseId && (!t.IsDeleted || includeDeletedItems)
								select t;

					//Add project filter if supplied
					if (projectId.HasValue)
					{
						query = query.Where(t => t.ProjectId == projectId.Value);
					}

					testCase = query.FirstOrDefault();

					//If we don't have a record, throw a specific exception
					if (testCase == null)
					{
						throw new ArtifactNotExistsException("Test Case " + testCaseId + " doesn't exist in the system.");
					}

					//Now get the test steps, they are joined implicitly by 'fix-up'
					var query2 = from s in context.TestSteps
									.Include(s => s.ExecutionStatus)
									.Include(s => s.LinkedTestCase)
								 where
									(!s.IsDeleted || includeDeletedItems) &&
									s.TestCaseId == testCaseId &&
									(!s.LinkedTestCase.IsDeleted || includeDeletedItems || !s.LinkedTestCaseId.HasValue)
								 orderby s.Position, s.TestStepId
								 select s;

					query2.ToList();

				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testCase;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves all test cases in the specified project that ARE marked for deletion.</summary>
		/// <param name="projectId">The project ID to get items for.</param>
		/// <returns></returns>
		public List<TestCaseView> RetrieveDeleted(int projectId)
		{
			const string METHOD_NAME = "RetrieveDeleted()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestCaseView> deletedTestCases;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestCasesView
								where t.ProjectId == projectId && t.IsDeleted
								orderby t.TestCaseId
								select t;

					deletedTestCases = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return deletedTestCases;
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				//Do not rethrow.
				return new List<TestCaseView>();
			}
		}

		/// <summary>Retrieves a particular test case view entity by its ID</summary>
		/// <param name="testCaseId">The ID of the test case we want to retrieve</param>
		/// <param name="includeDeleted">Should we include deleted</param>
		/// <param name="projectId">The id of the project</param>
		/// <returns>A test case entity</returns>
		/// <seealso cref="RetrieveById2"/>
		public TestCaseView RetrieveById(int? projectId, int testCaseId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TestCaseView testCaseView;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the query for retrieving the test case entity
					var query = from t in context.TestCasesView
								where t.TestCaseId == testCaseId && (!t.IsDeleted || includeDeleted)
								select t;

					//Add the project filter if specified
					if (projectId.HasValue)
					{
						query = query.Where(t => t.ProjectId == projectId.Value);
					}

					testCaseView = query.FirstOrDefault();
				}
				//If we don't have a record, throw a specific exception (since client will be expecting one record)
				if (testCaseView == null)
				{
					throw new ArtifactNotExistsException("Test Case " + testCaseId.ToString() + " doesn't exist in the system.");
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the test case
				return testCaseView;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a particular test case entity by its ID</summary>
		/// <param name="testCaseId">The ID of the test case we want to retrieve</param>
		/// <returns>A test case entity</returns>
		/// <param name="includeDeleted">Should we include deleted</param>
		/// <param name="projectId">The current project</param>
		/// <seealso cref="RetrieveById"/>
		public TestCase RetrieveById2(int? projectId, int testCaseId, bool includeDeleted = false, bool includeType = false)
		{
			const string METHOD_NAME = "RetrieveById2";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TestCase testCase;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					ObjectQuery<TestCase> tcQuery = context.TestCases;

					if (includeType)
						tcQuery = tcQuery.Include(t => t.Type);

					//Create the query for retrieving the test case entity
					var query = from t in tcQuery
								where t.TestCaseId == testCaseId && (!t.IsDeleted || includeDeleted)
								select t;

					//Add the project filter if specified
					if (projectId.HasValue)
					{
						query = query.Where(t => t.ProjectId == projectId.Value);
					}

					testCase = query.FirstOrDefault();
				}
				//If we don't have a record, throw a specific exception (since client will be expecting one record)
				if (testCase == null)
				{
					throw new ArtifactNotExistsException("Test Case " + testCaseId.ToString() + " doesn't exist in the system.");
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the test case
				return testCase;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Counts all the test cases in the release</summary>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="folderId">
		/// The folder to filter by, null = root folder, TEST_CASE_FOLDER_ID_ALL_TEST_CASES = all test cases
		/// </param>
		/// <param name="countAllFolders">Should we count the test cases in all folders in the project</param>
		/// <returns>The total number of test cases</returns>
		/// <remarks>Used to help with pagination</remarks>
		public int CountByRelease(int projectId, int releaseId, Hashtable filters, double utcOffset, int? folderId = null, bool includeDeleted = false, bool countAllFolders = false)
		{
			const string METHOD_NAME = "CountByRelease";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int testCaseCount = 0;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from t in context.TestCaseReleasesView
								where
									(!t.IsDeleted || includeDeleted) &&
									t.ProjectId == projectId &&
									t.ReleaseId == releaseId
								select t;

					//Add the dynamic filters
					if (filters != null)
					{
						//Get the template for this project
						int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<TestCaseReleaseView, bool>> filterClause = CreateFilterExpression<TestCaseReleaseView>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, filters, utcOffset, null, HandleTestCaseSpecificFilters);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<TestCaseReleaseView>)query.Where(filterClause);
						}
					}

					//See if we need to filter by folder
					if (folderId.HasValue)
					{
						int folderIdValue = folderId.Value;
						if (folderIdValue != TEST_CASE_FOLDER_ID_ALL_TEST_CASES)
						{
							query = query.Where(t => t.TestCaseFolderId == folderIdValue);
						}
					}
					else if ((filters == null || filters.Count == 0) && !countAllFolders)
					{
						//test cases that have no folder (i.e. root), unless we have filters in which case show all
						query = query.Where(t => !t.TestCaseFolderId.HasValue);
					}

					//Get the count
					testCaseCount = query.Count();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testCaseCount;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Counts all the test cases in the project</summary>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="folderId">
		/// The folder to filter by, null = root folder, TEST_CASE_FOLDER_ID_ALL_TEST_CASES = all test cases
		/// </param>
		/// <param name="countAllFolders">Should we count the test cases in all folders in the project</param>
		/// <returns>The total number of test cases</returns>
		/// <remarks>Used to help with pagination</remarks>
		public int Count(int projectId, Hashtable filters, double utcOffset, int? folderId = null, bool includeDeleted = false, bool countAllFolders = false)
		{
			const string METHOD_NAME = "Count";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int testCaseCount = 0;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from t in context.TestCasesView
								where (!t.IsDeleted || includeDeleted) && t.ProjectId == projectId
								select t;

					//Add the dynamic filters
					if (filters != null)
					{
						//Get the template for this project
						int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<TestCaseView, bool>> filterClause = CreateFilterExpression<TestCaseView>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, filters, utcOffset, null, HandleTestCaseSpecificFilters);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<TestCaseView>)query.Where(filterClause);
						}
					}

					//See if we need to filter by folder
					if (folderId.HasValue)
					{
						int folderIdValue = folderId.Value;
						if (folderIdValue != TEST_CASE_FOLDER_ID_ALL_TEST_CASES)
						{
							query = query.Where(t => t.TestCaseFolderId == folderIdValue);
						}
					}
					else if ((filters == null || filters.Count == 0) && !countAllFolders)
					{
						//test cases that have no folder (i.e. root), unless we have filters in which case show all
						query = query.Where(t => !t.TestCaseFolderId.HasValue);
					}

					//Get the count
					testCaseCount = query.Count();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testCaseCount;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Returns a list of all of the test cases in the specified folder AND their subfolders
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="testCaseFolderId">The id of the test case folder</param>
		/// <returns>The list of test cases in order of folders</returns>
		public List<TestCase> RetrieveAllInFolder(int projectId, int testCaseFolderId, bool includeDeleted = true)
		{
			const string METHOD_NAME = "RetrieveAllInFolder";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestCase> testCases = new List<TestCase>();

				//First get a list of all the direct child folderds
				List<TestCaseFolder> childFolders = TestCaseFolder_GetByParentId(projectId, testCaseFolderId);

				//Recursively call this function if there are any
				if (childFolders != null && childFolders.Count > 0)
				{
					foreach (TestCaseFolder childFolder in childFolders)
					{
						List<TestCase> childTestCases = RetrieveAllInFolder(projectId, childFolder.TestCaseFolderId, includeDeleted);
						testCases.AddRange(childTestCases);
					}
				}

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First get all of the test cases in the specified folder
					var query = from t in context.TestCases
								where t.TestCaseFolderId == testCaseFolderId && t.ProjectId == projectId && (!t.IsDeleted || includeDeleted)
								orderby t.Name, t.TestCaseId
								select t;

					testCases.AddRange(query.ToList());
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testCases;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list of all test cases in a release</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="sortProperty">The property name to be sorted on</param>
		/// <param name="sortAscending">Whether to sort the data ascending</param>
		/// <param name="startRow">The first row to retrieve (starting at 1)</param>
		/// <param name="numberOfRows">The number of rows to retrieve</param>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <param name="ignoreRootFolderIfFilterSet">Should we ignore the root folder restriction if a filter is applied (default: TRUE)</param>
		/// <param name="folderId">
		/// The folder to filter by, null = root folder, TEST_CASE_FOLDER_ID_ALL_TEST_CASES = all test cases
		/// </param>
		/// <param name="includeDeleted">Should we include deleted testCases</param>
		/// <param name="utcOffset">The offset from UTC</param>
		/// <returns>TestCaseView list</returns>
		/// <remarks>Also brings across any associated custom properties</remarks>
		public List<TestCaseReleaseView> RetrieveByReleaseId(int projectId, int releaseId, string sortProperty, bool sortAscending, int startRow, int numberOfRows, Hashtable filters, double utcOffset, int? folderId = null, bool includeDeleted = false, bool ignoreRootFolderIfFilterSet = true)
		{
			const string METHOD_NAME = "RetrieveByReleaseId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestCaseReleaseView> testCases;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from t in context.TestCaseReleasesView
								where
									(!t.IsDeleted || includeDeleted) &&
									t.ProjectId == projectId &&
									t.ReleaseId == releaseId
								select t;

					//Add the dynamic sort
					if (String.IsNullOrEmpty(sortProperty))
					{
						//Default to sorting by last updated date descending
						query = query.OrderByDescending(t => t.LastUpdateDate).ThenBy(t => t.TestCaseId);
					}
					else
					{
						//We always sort by the physical ID to guarantee stable sorting
						string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
						query = query.OrderUsingSortExpression(sortExpression, "TestCaseId");
					}

					//Add the dynamic filters
					if (filters != null)
					{
						//Get the template for this project
						int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<TestCaseReleaseView, bool>> filterClause = CreateFilterExpression<TestCaseReleaseView>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, filters, utcOffset, null, HandleTestCaseSpecificFilters);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<TestCaseReleaseView>)query.Where(filterClause);
						}
					}

					//See if we need to filter by folder
					if (folderId.HasValue)
					{
						if (folderId != TEST_CASE_FOLDER_ID_ALL_TEST_CASES)
						{
							int folderIdValue = folderId.Value;
							query = query.Where(t => t.TestCaseFolderId == folderIdValue);
						}
					}
					else if (filters == null || filters.Count == 0 || ignoreRootFolderIfFilterSet == false)
					{
						//test cases that have no folder (i.e. root), unless we have filters in which case show all
						query = query.Where(t => !t.TestCaseFolderId.HasValue);
					}

					//Get the count
					int artifactCount = query.Count();

					//Make pagination is in range
					if (startRow < 1)
					{
						startRow = 1;
					}
					if (startRow > artifactCount)
					{
						//Return nothing
						return new List<TestCaseReleaseView>();
					}

					//Execute the query
					testCases = query
						.Skip(startRow - 1)
						.Take(numberOfRows)
						.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testCases;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the test cases by the owner regardless of project, sorted by status then date</summary>
		/// <param name="ownerId">The owner of the test cases we want returned</param>
		/// <param name="projectId">The project ID to filter by (null = all projects)</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <param name="numberRows">The number of rows to return</param>
		/// <returns>Test Case list</returns>
		/// <remarks>It also only returns test cases for active projects.</remarks>
		public List<TestCaseView> RetrieveByOwnerId(int ownerId, int? projectId, int numberRows = 500, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveByOwnerId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestCaseView> testCases;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestCasesView
								where t.OwnerId == ownerId && t.IsProjectActive
								select t;

					//Add on the project filter if appropriate
					if (projectId.HasValue)
					{
						query = query.Where(t => t.ProjectId == projectId.Value);
					}

					//Add on the deleted filter
					if (!includeDeleted)
					{
						query = query.Where(t => !t.IsDeleted);
					}

					//Sort by execution status, date
					query = query.OrderBy(t => t.ExecutionStatusId).ThenBy(t => t.ExecutionDate).ThenBy(t => t.TestCaseId);

					//Execute the query
					testCases = query.Take(numberRows).ToList();
				}


				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testCases;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list of all testCases in the system (for a project)</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="sortProperty">The property name to be sorted on</param>
		/// <param name="sortAscending">Whether to sort the data ascending</param>
		/// <param name="startRow">The first row to retrieve (starting at 1)</param>
		/// <param name="numberOfRows">The number of rows to retrieve</param>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <param name="folderId">
		/// The folder to filter by, null = root folder, TEST_CASE_FOLDER_ID_ALL_TEST_CASES = all test cases
		/// </param>
		/// <param name="includeDeleted">Should we include deleted testCases</param>
		/// <param name="ignoreRootFolderIfFilterSet">Should we ignore the root folder restriction if a filter is applied (default: TRUE)</param>
		/// <param name="utcOffset">The offset from UTC</param>
		/// <returns>TestCaseView list</returns>
		/// <remarks>Also brings across any associated custom properties</remarks>
		public List<TestCaseView> Retrieve(int projectId, string sortProperty, bool sortAscending, int startRow, int numberOfRows, Hashtable filters, double utcOffset, int? folderId = null, bool includeDeleted = false, bool ignoreRootFolderIfFilterSet = true)
		{
			const string METHOD_NAME = "Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestCaseView> testCases;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from t in context.TestCasesView
								where (!t.IsDeleted || includeDeleted) && t.ProjectId == projectId
								select t;

					//Add the dynamic sort
					if (String.IsNullOrEmpty(sortProperty))
					{
						//Default to sorting by last updated date descending
						query = query.OrderByDescending(t => t.LastUpdateDate).ThenBy(t => t.TestCaseId);
					}
					else
					{
						//We always sort by the physical ID to guarantee stable sorting
						string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
						query = query.OrderUsingSortExpression(sortExpression, "TestCaseId");
					}

					//Add the dynamic filters
					if (filters != null)
					{
						//Get the template for this project
						int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<TestCaseView, bool>> filterClause = CreateFilterExpression<TestCaseView>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.TestCase, filters, utcOffset, null, HandleTestCaseSpecificFilters);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<TestCaseView>)query.Where(filterClause);
						}
					}

					//See if we need to filter by folder
					if (folderId.HasValue)
					{
						if (folderId != TEST_CASE_FOLDER_ID_ALL_TEST_CASES)
						{
							int folderIdValue = folderId.Value;
							query = query.Where(t => t.TestCaseFolderId == folderIdValue);
						}
					}
					else if (filters == null || filters.Count == 0 || ignoreRootFolderIfFilterSet == false)
					{
						//test cases that have no folder (i.e. root)
						//if we have a filter active then show all folder matches
						query = query.Where(t => !t.TestCaseFolderId.HasValue);
					}

					//Get the count
					int artifactCount = query.Count();

					//Make pagination is in range
					if (startRow < 1)
					{
						startRow = 1;
					}
					if (startRow > artifactCount)
					{
						//Return nothing
						return new List<TestCaseView>();
					}

					//Execute the query
					testCases = query
						.Skip(startRow - 1)
						.Take(numberOfRows)
						.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testCases;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Handles any test case specific filters that are not generic</summary>
		/// <param name="expressionList">The existing list of expressions</param>
		/// <param name="filter">The current filter</param>
		/// <param name="projectId">The current project</param>
		/// <param name="p">The LINQ parameter</param>
		/// <param name="projectTemplateId">The current project template</param>
		/// <param name="utcOffset">The current offset from UTC</param>
		/// <returns>True if handled, return False for the standard filter handling</returns>
		protected internal bool HandleTestCaseSpecificFilters(int? projectId, int? projectTemplateId, ParameterExpression p, List<Expression> expressionList, KeyValuePair<string, object> filter, double utcOffset)
		{
			return false;
		}

		/// <summary>Handles any test case folder specific filters that are not generic</summary>
		/// <param name="expressionList">The existing list of expressions</param>
		/// <param name="filter">The current filter</param>
		/// <param name="projectId">The current project</param>
		/// <param name="projectTemplateId">the current project template</param>
		/// <param name="p">The LINQ parameter</param>
		/// <param name="utcOffset">The current offset from UTC</param>
		/// <returns>True if handled, return False for the standard filter handling</returns>
		protected internal bool HandleTestFolderSpecificFilters(int? projectId, int? projectTemplateId, ParameterExpression p, List<Expression> expressionList, KeyValuePair<string, object> filter, double utcOffset)
		{
			//By default, let the generic filter convertor handle the filter
			string filterProperty = filter.Key;
			object filterValue = filter.Value;

			//Handle the special case of execution status, since it doesn't map to a single column
			if (filterProperty == "ExecutionStatusId")
			{
				if (filterValue is Int32)
				{
					TestCase.ExecutionStatusEnum executionStatus = (TestCase.ExecutionStatusEnum)((int)filterValue);
					switch (executionStatus)
					{
						case TestCase.ExecutionStatusEnum.Blocked:
							{
								MemberExpression statusCount = Expression.PropertyOrField(p, "CountBlocked");
								Expression expression = Expression.GreaterThan(statusCount, Expression.Constant(0));
								expressionList.Add(expression);
							}
							break;

						case TestCase.ExecutionStatusEnum.Caution:
							{
								MemberExpression statusCount = Expression.PropertyOrField(p, "CountCaution");
								Expression expression = Expression.GreaterThan(statusCount, Expression.Constant(0));
								expressionList.Add(expression);
							}
							break;

						case TestCase.ExecutionStatusEnum.Failed:
							{
								MemberExpression statusCount = Expression.PropertyOrField(p, "CountFailed");
								Expression expression = Expression.GreaterThan(statusCount, Expression.Constant(0));
								expressionList.Add(expression);

							}
							break;

						case TestCase.ExecutionStatusEnum.NotApplicable:
							{
								MemberExpression statusCount = Expression.PropertyOrField(p, "CountNotApplicable");
								Expression expression = Expression.GreaterThan(statusCount, Expression.Constant(0));
								expressionList.Add(expression);
							}
							break;

						case TestCase.ExecutionStatusEnum.NotRun:
							{
								MemberExpression statusCount = Expression.PropertyOrField(p, "CountNotRun");
								Expression expression = Expression.GreaterThan(statusCount, Expression.Constant(0));
								expressionList.Add(expression);
							}
							break;

						case TestCase.ExecutionStatusEnum.Passed:
							{
								MemberExpression statusCount = Expression.PropertyOrField(p, "CountPassed");
								Expression expression = Expression.GreaterThan(statusCount, Expression.Constant(0));
								expressionList.Add(expression);
							}
							break;
					}
				}
				return true;
			}
			return false;
		}

		/// <summary>Marks the specified test case as deleted.</summary>
		/// <param name="userId">The userId making the deletion.</param>
		/// <param name="projectId">The projectId that the test case belongs to.</param>
		/// <param name="testCaseId">The test case to delete.</param>
		public void MarkAsDeleted(int userId, int projectId, int testCaseId)
		{
			const string METHOD_NAME = "MarkAsDeleted()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//We need to initially retrieve the test case (cannot be already deleted)
					var query = from t in context.TestCases
								where t.TestCaseId == testCaseId && !t.IsDeleted
								select t;

					//Get the test case
					TestCase testCase = query.FirstOrDefault();
					if (testCase != null)
					{
						//Next get a list of any releases and requirements mapped to this test case (used later)
						ReleaseManager releaseManager = new ReleaseManager();
						List<ReleaseView> mappedReleases = releaseManager.RetrieveMappedByTestCaseId(UserManager.UserInternal, null, testCaseId);
						RequirementManager requirementManager = new RequirementManager();
						List<RequirementView> requirements = requirementManager.RetrieveCoveredByTestCaseId(UserManager.UserInternal, null, testCaseId);

						//Mark as deleted
						testCase.StartTracking();
						testCase.LastUpdateDate = DateTime.UtcNow;
						testCase.IsDeleted = true;

						//Save changes, no history logged, that's done later for the delete
						context.SaveChanges();

						//Add a changeset to mark it as deleted.
						new HistoryManager().LogDeletion(projectId, userId, Artifact.ArtifactTypeEnum.TestCase, testCaseId, DateTime.UtcNow);

						//Now roll up the execution status to the folders
						if (testCase.TestCaseFolderId.HasValue)
						{
							RefreshFolderExecutionStatus(projectId, testCase.TestCaseFolderId.Value);
						}

						//Next perform a bulk refresh of the requirement list coverage summary data
						foreach (RequirementView requirement in requirements)
						{
							requirementManager.RefreshTaskProgressAndTestCoverage(requirement.ProjectId, requirement.RequirementId);
						}

						//Check to see if any test cases have links to it and need to have their steps flag changed
						context.TestCase_UpdateParentTestStepsFlag(projectId, testCaseId, true);

						//Finally perform a bulk refresh of the test status / progress / effort of the mapped releases
						Project project = new ProjectManager().RetrieveById(projectId);
						List<int> releaseIds = mappedReleases.Select(r => r.ReleaseId).ToList();
						releaseManager.RefreshProgressEffortTestStatus(projectId, releaseIds);
					}
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				throw ex;
			}
		}

		/// <summary>Undeletes a test case and all children test cases, making it available to users.</summary>
		/// <param name="testCaseId">The test case to undelete.</param>
		/// <param name="userId">The userId performing the undelete.</param>
		/// <param name="logHistory">Whether to log this to history or not. Default: TRUE</param>
		public void UnDelete(int testCaseId, int userId, long rollbackId, bool logHistory = true)
		{
			const string METHOD_NAME = "UnDelete()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			ReleaseManager releaseManager = new ReleaseManager();
			RequirementManager requirementManager = new RequirementManager();
			List<ReleaseView> mappedReleases = null;
			List<RequirementView> requirements = null;
			int projectId = -1;
			int? folderId = null;
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				//We need to initially retrieve the test case (needs to be marked as deleted)
				var query = from t in context.TestCases
							where t.TestCaseId == testCaseId && t.IsDeleted
							select t;

				//Get the test case
				TestCase testCase = query.FirstOrDefault();
				if (testCase != null)
				{
					//Next get a list of any releases and requirements mapped to this test case (used later)
					projectId = testCase.ProjectId;
					folderId = testCase.TestCaseFolderId;
					mappedReleases = releaseManager.RetrieveMappedByTestCaseId(UserManager.UserInternal, null, testCaseId);
					requirements = requirementManager.RetrieveCoveredByTestCaseId(UserManager.UserInternal, null, testCaseId);

					//Mark as undeleted
					testCase.StartTracking();
					testCase.LastUpdateDate = DateTime.UtcNow;
					testCase.IsDeleted = false;

					//Save changes, no history logged, that's done later
					context.SaveChanges();
				}
			}

			//Update the execution statuses.
			if (projectId > 0 && folderId.HasValue)
			{
				RefreshFolderExecutionStatus(projectId, folderId.Value);
			}

			//Next perform a bulk refresh of the requirement list coverage summary data
			if (requirements != null)
			{
				foreach (RequirementView requirement in requirements)
				{
					requirementManager.RefreshTaskProgressAndTestCoverage(projectId, requirement.RequirementId);
				}
			}

			//Finally perform a bulk refresh of the test status, progress, completeion of the mapped releases
			if (mappedReleases != null && mappedReleases.Count > 0)
			{
				List<int> releaseIds = mappedReleases.Select(r => r.ReleaseId).ToList();
				releaseManager.RefreshProgressEffortTestStatus(projectId, releaseIds);
			}

			//Log the undelete
			if (projectId > 0 && logHistory)
			{
				//Okay, mark it as being undeleted.
				new HistoryManager().LogUnDeletion(projectId, userId, Artifact.ArtifactTypeEnum.TestCase, testCaseId, rollbackId, DateTime.UtcNow);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Deletes a test-case in the system that has the specified ID</summary>
		/// <param name="userId">The user we're viewing the test cases as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="testCaseId">The ID of the test-case to be deleted</param>
		public void DeleteFromDatabase(int testCaseId, int userId)
		{
			const string METHOD_NAME = "DeleteFromDatabase()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//We need to initially retrieve the test case to make sure it exists
				TestCase testCase = null;
				try
				{
					testCase = RetrieveByIdWithSteps(null, testCaseId, true);
				}
				catch
				{
					return;
				}
				int projectId = testCase.ProjectId;
				int? folderId = testCase.TestCaseFolderId;

				//First remove any associated test runs
				new TestRunManager().DeleteByTestCaseId(testCaseId);

				//Next we need to delete any attachments associated with the test case
				AttachmentManager attachmentManager = new AttachmentManager();
				attachmentManager.DeleteByArtifactId(testCaseId, Artifact.ArtifactTypeEnum.TestCase);

				//Next we need to delete any custom properties associated with the test case		
				new CustomPropertyManager().ArtifactCustomProperty_DeleteByArtifactId(testCaseId, Artifact.ArtifactTypeEnum.TestCase);

				//Next we need to delete any parameters linked to this test case
				DeleteParametersByTestCaseId(projectId, testCaseId);

				//Next we need to get a list of any test steps that are linked to this test case
				//and remove them
				List<TestStep> linkingTestSteps = RetrieveLinkingTestSteps(testCaseId);
				for (int i = 0; i < linkingTestSteps.Count; i++)
				{
					int linkingTestCaseId = linkingTestSteps[i].TestCaseId;
					int linkingTestStepId = linkingTestSteps[i].TestStepId;
					DeleteStepFromDatabase(userId, linkingTestStepId, false);
				}

				//Now we need to delete any test steps associated with this test case
				//Need to use the method because it handles any linked test run steps
				//from any linked test cases
				for (int i = 0; i < testCase.TestSteps.Count; i++)
				{
					DeleteStepFromDatabase(userId, testCase.TestSteps[i].TestStepId, false);
				}

				//Finally call the stored procedure to delete the test case itself
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.TestCase_Delete(testCaseId);
				}

				//Update the execution statuses.
				if (projectId > 0 && folderId.HasValue)
				{
					RefreshFolderExecutionStatus(projectId, folderId.Value);
				}

				//Update the test case parameter hierarchy
				if (projectId > 0)
				{
					TestCase_RefreshParameterHierarchy(projectId, testCaseId);
				}

				//Log the purge.
				new HistoryManager().LogPurge(projectId, userId, Artifact.ArtifactTypeEnum.TestCase, testCaseId, DateTime.UtcNow, testCase.Name);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates the folder that a test case is linked to
		/// </summary>
		/// <param name="testCaseId">The id of the test case</param>
		/// <param name="folderId">The id of the folder</param>
		/// <remarks>Folder changes are not tracked in the artifact history for test cases</remarks>
		public void TestCase_UpdateFolder(int testCaseId, int? folderId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "TestCase_UpdateFolder";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int? oldFolderId = null;
				int? projectId = null;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Retrieve the testCase
					var query = from t in context.TestCases
								where t.TestCaseId == testCaseId && (!t.IsDeleted || includeDeleted)
								select t;

					TestCase testCase = query.FirstOrDefault();
					if (testCase != null)
					{
						projectId = testCase.ProjectId;
						oldFolderId = testCase.TestCaseFolderId;
						testCase.StartTracking();
						testCase.TestCaseFolderId = folderId;

						//We don't need to log history for this
						context.SaveChanges();
					}
				}

				if (projectId.HasValue)
				{
					//Rollup the changes to the folders
					if (oldFolderId.HasValue)
					{
						RefreshFolderExecutionStatus(projectId.Value, oldFolderId.Value);
					}
					if (folderId.HasValue && folderId != oldFolderId)
					{
						RefreshFolderExecutionStatus(projectId.Value, folderId.Value);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public void RequestApprovalForTestCase(int projectId, int currentStatusId, int testCaseId, List<int> userIds, int? loggedinUserId = null, string meaning = null)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				DateTime updatedDate = DateTime.Now;

				if (userIds.Count() > 0)
				{
					//cancel all current active workflows (if any)
					var existingWorkflows = context.TestCaseApprovalWorkflows.Where(x => x.TestCaseId == testCaseId);
					if (!existingWorkflows.Any()) 
					{
						foreach (var workflow in existingWorkflows)
						{
							workflow.IsActive = false;
							workflow.UpdateDate = DateTime.Now;
							foreach (var item in workflow.Signatures)
							{
								item.StatusId = (int)TestCaseSignatureStatus.Cancelled;
								item.UpdateDate = updatedDate;
								item.Meaning = "Auto Cancelled";
							}
						}

						var newWorkflow = new TestCaseApprovalWorkflow
						{
							IsActive = true,
							UpdateDate = updatedDate,
							TestCaseId = testCaseId
						};
						context.TestCaseApprovalWorkflows.AddObject(newWorkflow);


						foreach (var item in userIds)
						{
							newWorkflow.Signatures.Add(new TestCaseSignature
							{
								StatusId = (int)TestCaseManager.TestCaseSignatureStatus.Requested,
								//StatusId = currentStatusId,
								TestCaseId = testCaseId,
								RequestedDate = updatedDate,
								UserId = item,
								UpdateDate = updatedDate,
								Meaning = meaning
							});
						}
					}
					else
					{
						//foreach (var workflow in existingWorkflows)
						//{
						//	workflow.IsActive = true;
						//	workflow.UpdateDate = DateTime.Now;
						//}

						List<TestCaseSignature> changedEntities = new List<TestCaseSignature>();
						foreach (var workflow in existingWorkflows)
						{
							workflow.IsActive = true;
							workflow.UpdateDate = DateTime.Now;
							
							foreach (var item in userIds)
							{
								var workflowData = context.TestCaseApprovalWorkflows.Include(x => x.Signatures).Where(x => x.TestCaseId == testCaseId && x.IsActive).OrderByDescending(x => x.UpdateDate).FirstOrDefault();
								if (workflowData != null)
								{
									var allApprovalsRequired = workflowData.Signatures.Where(x => x.StatusId != (int)TestCaseManager.TestCaseSignatureStatus.Cancelled).ToList();
									var data = allApprovalsRequired.Where(x => x.UserId == item && x.StatusId == (int)TestCaseManager.TestCaseSignatureStatus.Requested).FirstOrDefault();
									if (data == null)
									{
										var testcaseData = RetrieveBySignature(testCaseId, item, currentStatusId);
										if (testcaseData == null)
										{
											testcaseData = new TestCaseSignature
											{
												StatusId = (int)TestCaseManager.TestCaseSignatureStatus.Requested,
												//StatusId = currentStatusId,
												TestCaseId = testCaseId,
												RequestedDate = updatedDate,
												ApprovalWorkflowId = workflow.TestCaseApprovalWorkflowId,
												UserId = item,
												UpdateDate = updatedDate,
												Meaning = meaning
											};
											changedEntities.Add(testcaseData.MarkAsAdded());
										}
									}
								}
							}
						}
						UpdateApproversForTestCaseSignature(changedEntities);
					}

					context.SaveChanges();
					//log history
					//new HistoryManager().LogCreation(projectId, (int)loggedinUserId, Artifact.ArtifactTypeEnum.TestCaseSignature, testCaseId, DateTime.UtcNow);
				}

			}
		}

		public void UpdateApproversForTestCaseSignature(List<TestCaseSignature> updatedEntities)
		{

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				foreach (var item in updatedEntities)
				{
					context.TestCaseSignatures.ApplyChanges(item);
				}

				context.SaveChanges();
			}
		}

		public TestCaseSignature RetrieveBySignature(int testCaseId, int userId, int statusId)
		{
			const string METHOD_NAME = "RetrieveBySignature()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TestCaseSignature testCaseSignature;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.TestCaseSignatures
								where r.TestCaseId == testCaseId && r.UserId == userId && r.StatusId == statusId
								select r;

					testCaseSignature = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testCaseSignature;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
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

		public List<TestCaseSignature> RetrieveExistingSignaturesForTestCase(int testCaseId)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var signatures = context.TestCaseSignatures.Include(x => x.User).Include(x => x.User.Profile).Where(x => x.TestCaseId == testCaseId && x.StatusId != (int)TestCaseManager.TestCaseSignatureStatus.Cancelled);
				return signatures.ToList();
			}
		}

		static bool ContainsApproveAfterTo(string input, string status)
		{
			// Find the position of the word "to"
			int indexOfTo = input.IndexOf("to");

			// If "to" is found, check if the word status comes after it
			if (indexOfTo != -1)
			{
				string substringAfterTo = input.Substring(indexOfTo);
				return substringAfterTo.Contains(status);
			}

			return false;
		}

		public List<TestCaseSignature> RetrieveExistingSignaturesForTestCaseByUserId(int testCaseId, int userId, string status)
		{
			List<TestCaseSignature> sign = new List<TestCaseSignature>();
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var signatures = context.TestCaseSignatures.Include(x => x.User).Include(x => x.User.Profile).Where(x => x.TestCaseId == testCaseId && x.UserId == userId);
				if (signatures != null)
				{
					foreach (var data in signatures)
					{
						string meaning = data.Meaning;
						bool result = ContainsApproveAfterTo(meaning,status);

						if (result)
						{
							sign.Add(data);
						}
					}
				}
				return sign;
			}
		}

		public TestCaseSignature RetrieveExistingSignaturesForTestCaseByUser(int testCaseId, int userId)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var signatures = context.TestCaseSignatures.Include(x => x.User).Include(x => x.User.Profile).Where(x => x.TestCaseId == testCaseId && x.UserId == userId);
				
				return signatures.FirstOrDefault();
			}
		}

		public TestCaseSignature RetrieveExistingSignaturesForLastTestCaseByUser(int testCaseId, int userId)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var signatures = context.TestCaseSignatures.Include(x => x.User).Include(x => x.User.Profile).Where(x => x.TestCaseId == testCaseId && x.UserId == userId).OrderByDescending(x => x.UpdateDate);
				return signatures.FirstOrDefault();
			}
		}

		public void UpdateTestCaseSignatureWorkflowState(int projectId, int testCaseId, int userId, TestCaseSignatureStatus status, int currentStatusId, string meaning)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var workflow = context.TestCaseApprovalWorkflows.Include(x => x.Signatures).Where(x => x.TestCaseId == testCaseId && x.IsActive).OrderByDescending(x => x.UpdateDate).FirstOrDefault();
				if (workflow != null)
				{
					workflow.StartTracking();
					var allApprovalsRequired = workflow.Signatures.Where(x => x.StatusId != (int)TestCaseManager.TestCaseSignatureStatus.Cancelled).ToList();

					//var found = allApprovalsRequired.Where(x => x.UserId == userId && x.StatusId == (int)TestCaseManager.TestCaseSignatureStatus.Requested).FirstOrDefault();
					var found = allApprovalsRequired.Where(x => x.UserId == userId && x.StatusId == (int)TestCaseManager.TestCaseSignatureStatus.Requested).FirstOrDefault();
					if (found != null)
					{
						found.StartTracking();
						found.StatusId = currentStatusId;
						found.UpdateDate = DateTime.Now;
						found.Meaning = meaning + " - Signed";
						if (allApprovalsRequired.All(x => x.StatusId == (int)TestCaseManager.TestCaseSignatureStatus.Approved))
						{
							workflow.IsActive = false;
							workflow.UpdateDate = DateTime.Now;
						}		

						//TestcaseSignature update
						context.SaveChanges(userId, true, false, null);
						//log history
						new HistoryManager().LogCreation(projectId, userId, Artifact.ArtifactTypeEnum.TestCaseSignature, testCaseId, DateTime.UtcNow);
					}

					//var otherUsers = allApprovalsRequired.Where(x => x.UserId != userId).ToList();

					//foreach(var user in otherUsers)
					//{
					//	if(user.StatusId != currentStatusId)
					//	{
					//		user.Meaning = meaning;
					//		found.UpdateDate = DateTime.Now;
					//		//TestcaseSignature update
					//		context.SaveChanges(userId, true, false, null);
					//	}
					//}
				}

			}
		}

		public List<TestCaseSignature> GetTestCaseSignaturesForCurrentWorkflow(int testCaseId)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var workflow = context.TestCaseApprovalWorkflows.Include(x => x.Signatures).Where(x => x.TestCaseId == testCaseId && x.IsActive).OrderByDescending(x => x.UpdateDate).FirstOrDefault();
				if (workflow != null)
				{
					return workflow.Signatures.ToList();
				}
			}

			return new List<TestCaseSignature>();
		}

		public List<TestCaseSignature> GetAllTestSignaturesForTestCase(int testCaseId)
		{
			List<TestCaseSignature> testCaseApprovals = new List<TestCaseSignature>();

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var approvals = context.TestCaseSignatures.Include(x => x.User).Include(x => x.User.Profile).Where(x => x.TestCaseId == testCaseId).OrderByDescending(x => x.ApprovalWorkflowId);
				foreach (var item in approvals)
				{
					testCaseApprovals.Add(item);
				}
			}

			return testCaseApprovals;
		}

		public bool GetApprovedTestSignaturesForTestCase(int testCaseId, int currentStatusId)
		{
			bool status = false;
			List<TestCaseSignature> testCaseApprovals = new List<TestCaseSignature>();

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var approvals = context.TestCaseSignatures.Include(x => x.User).Include(x => x.User.Profile).Where(x => x.TestCaseId == testCaseId).OrderByDescending(x => x.ApprovalWorkflowId);
				foreach (var item in approvals)
				{
					string meaning = item.Meaning;
					bool endsWithSigned = meaning.EndsWith("Signed", StringComparison.OrdinalIgnoreCase);

					if (endsWithSigned)
					{
						testCaseApprovals.Add(item);
					}
					//if (item.StatusId == currentStatusId)
					//{
					//	testCaseApprovals.Add(item);
					//}
				}
				int originalCount = approvals.Count();
				int signCount = testCaseApprovals.Count();
				if(originalCount == signCount)
				{
					status = true;
				}
			}

			return status;
		}

		public void CancelTestCaseApprovalWorkflow(int testCaseId)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var existingWorkflow = context.TestCaseApprovalWorkflows.Include(x => x.Signatures).Where(x => x.TestCaseId == testCaseId && x.IsActive).OrderByDescending(x => x.UpdateDate).FirstOrDefault();
				if (existingWorkflow != null)
				{
					existingWorkflow.IsActive = false;
					existingWorkflow.UpdateDate = DateTime.Now;
					foreach (var item in existingWorkflow.Signatures)
					{
						item.StatusId = (int)TestCaseSignatureStatus.Cancelled;
						item.UpdateDate = DateTime.Now;
					}
				}

				context.SaveChanges();

			}
		}


		#endregion

		#region Release Test Cases

		/// <summary>
		/// Gets the total estimated test case duration for a specific release
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="releaseId">The id of the release</param>
		/// <returns>The total release duration</returns>
		/// <remarks>
		/// Does NOT include child iterations of releases.
		/// </remarks>
		protected internal int? GetTotalReleaseDuration(int projectId, int releaseId)
		{
			const string METHOD_NAME = "GetTotalReleaseDuration";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int? totalDuration = null;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					totalDuration = context.TestCase_GetTotalReleaseDuration(projectId, releaseId).FirstOrDefault();
				}

				return totalDuration;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Adds the test cases associated with the specific requirement to the specified release
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="requirementId">The id of the requirement</param>
		/// <param name="releaseId">The id of the release</param>
		/// <remarks>
		/// 1) If the release is an iteration, also adds to parent release
		/// 2) Don't add the test case if not in the same project
		/// </remarks>
		protected internal void AddTestCasesToRequirementRelease(int projectId, int requirementId, int releaseId, int userId)
		{
			const string METHOD_NAME = "AddTestCasesToRequirementRelease";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//See if we are recording history.
				ProjectSettings prjSet = new ProjectSettings(projectId);
				bool recordHistory = prjSet.BaseliningEnabled && Global.Feature_Baselines;

				//Call stored procedure for performing the operation
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.TestCase_AddToRequirementRelease(projectId, requirementId, releaseId, recordHistory, userId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Test Case Parameters

		/// <summary>
		/// Refreshes the test case parameter hierarchy for the project
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		public void TestCase_RefreshParameterHierarchyForProject(int projectId)
		{
			const string METHOD_NAME = "TestCase_RefreshParameterHierarchyForProject";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Set a longer timeout for this as it's run infrequently to speed up retrieves
					context.CommandTimeout = SQL_COMMAND_TIMEOUT_CACHE_UPDATE;
					context.TestCase_RefreshParameterHierarchy(projectId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				//Don't throw, just log
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				//Don't throw, just log
			}
		}

		/// <summary>
		/// Refreshes the test case parameter hierarchy for the test case
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="testCaseId">The id of the test case</param>
		public void TestCase_RefreshParameterHierarchy(int projectId, int testCaseId)
		{
			const string METHOD_NAME = "TestCase_RefreshParameterHierarchy";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<int> parentTestCaseIds;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First refresh this test case
					context.TestCase_RefreshParameterHierarchyForTestCase(projectId, testCaseId);

					//Now see if there are any parents that need to be refreshed
					var query = from t in context.TestSteps
								where t.LinkedTestCaseId == testCaseId && t.TestCase.ProjectId == projectId && !t.IsDeleted && !t.TestCase.IsDeleted
								select t.TestCaseId;

					parentTestCaseIds = query.Distinct().ToList();
				}

				//Call this function recursively
				foreach (int parentTestCaseId in parentTestCaseIds)
				{
					this.TestCase_RefreshParameterHierarchy(projectId, parentTestCaseId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				//Don't throw, just log
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				//Don't throw, just log
			}
		}

		/// <summary>
		/// Refreshes the test case parameter hierarchy for a specified test step
		/// </summary>
		/// <param name="testStepId">The id of the test step</param>
		/// <param name="projectId">The id of the project</param>
		public void TestCase_RefreshParameterHierarchyForTestStep(int projectId, int testStepId)
		{
			const string METHOD_NAME = "TestCase_RefreshParameterHierarchyForTestStep";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int testCaseId = 0;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Find the test case of this test step
					var query = from t in context.TestSteps
								where t.TestCase.ProjectId == projectId && t.TestStepId == testStepId && !t.IsDeleted && !t.TestCase.IsDeleted
								select t.TestCaseId;

					testCaseId = query.FirstOrDefault();
				}
				if (testCaseId > 0)
				{
					TestCase_RefreshParameterHierarchy(projectId, testCaseId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				//Don't throw, just log
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				//Don't throw, just log
			}
		}

		/// <summary>
		/// Returns the full token of a test caseparameter from its name
		/// </summary>
		/// <param name="parameterName">The name of the parameter</param>
		/// <returns>The tokenized representation of the parameter used for search/replace</returns>
		/// <remarks>We use the same parameter format as Ant/NAnt</remarks>
		public static string CreateParameterToken(string parameterName)
		{
			return "${" + parameterName + "}";
		}

		/// <summary>
		/// Replaces all the parameters in a string
		/// </summary>
		/// <param name="input">The string containing the parameter tokens</param>
		/// <param name="parameters">The collection of paremeters (name & value)</param>
		/// <returns>The string with all the tokens replaced</returns>
		public static string ReplaceParameters(string input, Dictionary<string, TestRunParameter> parameters)
		{
			string output = input;
			if (parameters != null)
			{
				foreach (KeyValuePair<string, TestRunParameter> parameter in parameters)
				{
					if (parameter.Value != null)
					{
						string parameterName = parameter.Key;
						string parameterValue = parameter.Value.Value;
						output = output.Replace(CreateParameterToken(parameterName), parameterValue);
					}
				}
			}
			return output;
		}

		/// <summary>
		/// Inserts a new parameter for a test case
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="testCaseId">The test case in question</param>
		/// <param name="name">The name of the parameter</param>
		/// <param name="defaultValue">The default value of the parameter (optional)</param>
		/// <returns>The id of the new parameter</returns>
		public int InsertParameter(int projectId, int testCaseId, string name, string defaultValue, int? userId = null)
		{
			const string METHOD_NAME = "InsertParameter";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int testCaseParameterId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Fill out entity with data for new test case parameter
					TestCaseParameter testCaseParameter = new TestCaseParameter();
					testCaseParameter.TestCaseId = testCaseId;
					testCaseParameter.Name = name;
					testCaseParameter.DefaultValue = defaultValue;

					//Persist the entity
					context.TestCaseParameters.AddObject(testCaseParameter);
					context.SaveChanges();

					new HistoryManager().LogCreation(projectId, (int)userId, Artifact.ArtifactTypeEnum.TestCase, testCaseId, DateTime.UtcNow, Artifact.ArtifactTypeEnum.TestCaseParameter, testCaseParameter.TestCaseParameterId);

					testCaseParameterId = testCaseParameter.TestCaseParameterId;
				}

				//Refresh the hierarchy of parameters for the test case
				TestCase_RefreshParameterHierarchy(projectId, testCaseId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testCaseParameterId;
			}
			catch (EntityConstraintViolationException)
			{
				//If we have a unique constraint violation, throw a business exception
				throw new TestCaseDuplicateParameterNameException("That parameter name is already in use!");
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates an existing parameter for a test case
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="testCaseParameterId">The id of the parameter in question</param>
		/// <param name="name">The name of the parameter</param>
		/// <param name="defaultValue">The default value of the parameter (optional)</param>
		public void UpdateParameter(int projectId, int testCaseParameterId, string name, string defaultValue, int? userId = null)
		{
			const string METHOD_NAME = "UpdateParameter";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int? testCaseId = null;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Find the matching item and update
					var query = from p in context.TestCaseParameters
								where p.TestCaseParameterId == testCaseParameterId
								select p;

					TestCaseParameter testCaseParameter = query.FirstOrDefault();
					if (testCaseParameter != null)
					{
						testCaseParameter.StartTracking();
						testCaseParameter.Name = name;
						testCaseParameter.DefaultValue = defaultValue;
						testCaseId = testCaseParameter.TestCaseId;
					}

					//Persist the change
					context.SaveChanges(userId, true, false, null);
					//context.SaveChanges();
				}

				if (testCaseId.HasValue)
				{
					//Refresh the hierarchy of parameters for the test case
					TestCase_RefreshParameterHierarchy(projectId, testCaseId.Value);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (EntityConstraintViolationException)
			{
				//If we have a unique constraint violation, throw a business exception
				throw new TestCaseDuplicateParameterNameException("That parameter name is already in use!");
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Deletes a test case parameter that has the specified ID</summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="testCaseParameterId">The ID of the parameter to be deleted</param>
		public void DeleteParameter(int projectId, int testCaseParameterId, int? userId = null)
		{
			const string METHOD_NAME = "DeleteParameter";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int? testCaseId = null;
				string testcaseParameterName = string.Empty;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Find the matching to see if it exisst
					var query = from p in context.TestCaseParameters
								where p.TestCaseParameterId == testCaseParameterId
								select p;

					TestCaseParameter testCaseParameter = query.FirstOrDefault();
					if (testCaseParameter != null)
					{
						testcaseParameterName = testCaseParameter.Name;
						testCaseId = testCaseParameter.TestCaseId;
						//Called the stored proc to delete the parameter
						context.TestCase_DeleteParameter(testCaseParameterId);
					}
				}


				new HistoryManager().LogDeletion(projectId, (int)userId, Artifact.ArtifactTypeEnum.TestCase, (int)testCaseId, DateTime.UtcNow, Artifact.ArtifactTypeEnum.TestCaseParameter, testCaseParameterId, testcaseParameterName);


				if (testCaseId.HasValue)
				{
					//Refresh the hierarchy of parameters for the test case
					TestCase_RefreshParameterHierarchy(projectId, testCaseId.Value);
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Deletes all the test case parameters that belong to a test case
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="testCaseId">The ID of the test case</param>
		protected void DeleteParametersByTestCaseId(int projectId, int testCaseId)
		{
			const string METHOD_NAME = "DeleteParametersByTestCaseId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Setup the parameterized database command to map to the dataset columns
				//Need to delete from the parameter and parameter value tables
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.TestCase_DeleteParameterByTest(testCaseId);
				}

				//Refresh the hierarchy of parameters for the test case
				TestCase_RefreshParameterHierarchy(projectId, testCaseId);

			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Inserts a new parameter value for a linked test case test step
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="testStepId">The test step in question</param>
		/// <param name="testCaseParameterId">The id of the parameter</param>
		/// <param name="value">The value of the parameter</param>
		protected void InsertParameterValue(int projectId, int testStepId, int testCaseParameterId, string value)
		{
			const string METHOD_NAME = "InsertParameterValue";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Fill out entity with data for new test step parameter value
					TestStepParameter testStepParameter = new TestStepParameter();
					testStepParameter.TestStepId = testStepId;
					testStepParameter.TestCaseParameterId = testCaseParameterId;
					testStepParameter.Value = value;

					//Persist the entity
					context.TestStepParameters.AddObject(testStepParameter);
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list of parameters and values for a test step that is linked to a test case</summary>
		/// <param name="testStepId">The test step that is linking to a test case</param>
		/// <returns>List of test step parameters and values</returns>
		public List<TestStepParameter> RetrieveParameterValues(int testStepId)
		{
			const string METHOD_NAME = "RetrieveParameterValues";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestStepParameter> parameterValues;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.TestStepParameters.Include(p => p.Parameter)
								where p.TestStepId == testStepId
								select p;

					parameterValues = query.ToList();

					//Resort by name
					parameterValues = parameterValues.OrderBy(p => p.Name).ThenBy(p => p.TestCaseParameterId).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return parameterValues;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Insert/Update/Deletes the existing test step parameters for a single linked test step
		/// </summary>
		/// <param name="testStepId">The id of the test step</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="testStepParameters">The parameters</param>
		public void SaveParameterValues(int projectId, int testStepId, List<TestStepParameter> testStepParameters)
		{
			const string METHOD_NAME = "SaveParameterValues";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Now retrieve the parameters in the database
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.TestStepParameters
								where p.TestStepId == testStepId
								select p;

					List<TestStepParameter> parameterValuesInDb = query.ToList();

					//See if we need to add/update any existing parameters
					foreach (TestStepParameter parameter in testStepParameters)
					{
						TestStepParameter parameterValueInDb = parameterValuesInDb.FirstOrDefault(p => p.TestCaseParameterId == parameter.TestCaseParameterId);
						if (parameterValueInDb == null)
						{
							//Insert
							context.TestStepParameters.AddObject(parameter);
						}
						else
						{
							//Update
							parameterValueInDb.StartTracking();
							parameterValueInDb.Value = parameter.Value;
						}
					}

					//See if there are any that need deleting
					List<TestStepParameter> parametersToDelete = new List<TestStepParameter>();
					foreach (TestStepParameter parameterValueInDb in parameterValuesInDb)
					{
						if (!testStepParameters.Any(p => p.TestCaseParameterId == parameterValueInDb.TestCaseParameterId))
						{
							parametersToDelete.Add(parameterValueInDb);
						}
					}
					foreach (TestStepParameter parameterToDelete in parametersToDelete)
					{
						context.TestStepParameters.DeleteObject(parameterToDelete);
					}

					//Save all the changes
					context.SaveChanges();
				}

				//Refresh the hierarchy of parameters for the test step
				TestCase_RefreshParameterHierarchyForTestStep(projectId, testStepId);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list of test steps (with their test case id) that link to a test case</summary>
		/// <param name="linkedTestCaseId">The test case being linked to</param>
		/// <returns>list of test steps with test case ids</returns>
		protected List<TestStep> RetrieveLinkingTestSteps(int linkedTestCaseId)
		{
			const string METHOD_NAME = "RetrieveLinkingTestSteps";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				List<TestStep> linkingTestSteps;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestSteps
								where t.LinkedTestCaseId == linkedTestCaseId
								orderby t.TestCaseId, t.TestStepId
								select t;

					//Actually execute the query and return the dataset
					linkingTestSteps = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return linkingTestSteps;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Returns all of the test case parameters in the project
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>List of test case parameters</returns>
		public List<TestCaseParameter> RetrieveAllParameters(int projectId)
		{
			const string METHOD_NAME = "RetrieveAllParameters";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the list of test cases in the project
				List<TestCaseParameter> testCaseParameters;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestCaseParameters
								where t.TestCase.ProjectId == projectId
								orderby t.Name, t.TestCaseParameterId
								select t;

					testCaseParameters = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testCaseParameters;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public TestCaseParameter RetrieveParameterById(int parameterId)
		{
			const string METHOD_NAME = "RetrieveParameterById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the list of test cases in the project
				TestCaseParameter testCaseParameter;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestCaseParameters
								where t.TestCaseParameterId == parameterId
								orderby t.Name, t.TestCaseParameterId
								select t;

					testCaseParameter = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testCaseParameter;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public TestCaseSignature RetrieveTestCaseSignature(int testCaseId)
		{
			const string METHOD_NAME = "RetrieveTestCaseSignature";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the list of test cases in the project
				TestCaseSignature testCaseSignature;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestCaseSignatures
								where t.TestCaseId == testCaseId
								select t;

					testCaseSignature = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testCaseSignature;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public TestSetParameter RetrieveTestSetParameterById(int testSetId)
		{
			const string METHOD_NAME = "RetrieveTestSetParameterById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the list of test cases in the project
				TestSetParameter testSetParameter;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestSetParameters
								where t.TestSetId == testSetId
								orderby t.Value, t.TestCaseParameterId
								select t;

					testSetParameter = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testSetParameter;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list of parameters for a test case</summary>
		/// <param name="testCaseId">The test case in question</param>
		/// <returns>List of test case parameters</returns>
		/// <param name="includeAlreadySet">Do we want to include those from child test cases that already have a value set</param>
		/// <param name="includeInherited">Do we want to include those from child test cases</param>
		public List<TestCaseParameter> RetrieveParameters(int testCaseId, bool includeInherited = false, bool includeAlreadySet = false)
		{
			const string METHOD_NAME = "RetrieveParameters";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Call stored procedure for retrieving the data depending on whether we need to include inherited or not
				List<TestCaseParameter> testCaseParameters;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					testCaseParameters = context.TestCase_RetrieveParameters(testCaseId, includeInherited, includeAlreadySet).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testCaseParameters;
			}
			catch (SqlException exception)
			{
				//If we have a recursive query condition, throw the specific exception expected by the calling code
				if (exception.Number == 530)
				{
					throw new EntityInfiniteRecursionException("A recursive query has encountered an infinite loop condition");
				}
				else
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, "SqlException " + exception.Number + ": " + exception.Message);
					throw;
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public void SendUserTestCaseApproveNotification(string username, TestCase testCase)
		{
			const string METHOD_NAME = "SendUserTestCaseApproveNotification()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Generate the mail message.
			NotificationManager.EmailMessageDetails msgToSend = new NotificationManager.EmailMessageDetails();
			msgToSend.projectToken = "PR-xx";

			//Retrieve the user record
			User user = new UserManager().GetUserByLogin(username);
			msgToSend.toUserList.Add(new NotificationManager.EmailMessageDetails.EmailMessageDetailUser()
			{
				Address = user.EmailAddress,
				Name = user.FullName,
				SubjectId = 1,
				UserId = user.UserId,
				Source = "AcctApp"
			});

			//Get the URL and product name
			//string productName = Common.ConfigurationSettings.Default.License_ProductType;
			string webServerUrl = Common.ConfigurationSettings.Default.General_WebServerUrl;

			//Create the new mail message - eventually will need to template it
			msgToSend.subjectList.Add(1, "Test Case Approval Notification : " + testCase.Name);
			// Create email body content with dynamic data
		string body = $@"


Dear {user.FullName},<br><br>

We have a test case that requires your approval. Please review the following details and approve or reject the test case.<br><br>

- <strong>Test Case ID:</strong> {testCase.TestCaseId}<br>
- <strong>Test Case Title:</strong> {testCase.Name}<br>
- <strong>Test Case Description:</strong> {testCase.Description}<br>

If you have any questions or need additional information, please feel free to reach out.<br><br>

Thank you for your attention.<br><br>

Best regards, <br> 
validationmaster@mycompany.com";


			try
			{
				//SendCreationNotification(testCase.TestCaseId, null, null);
				new NotificationManager().SendEmail(msgToSend, body);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Test Steps

		/// <summary>
		/// Retrieves the list of test steps for a given test case
		/// </summary>
		/// <param name="testCaseId">The id of the test case</param>
		/// <param name="includeDeleted">Should we include deleted steps</param>
		/// <returns>The list of steps</returns>
		public List<TestStepView> RetrieveStepsForTestCase(int testCaseId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveStepsForTestCase";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestStepView> testSteps;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from s in context.TestStepsView
								where s.TestCaseId == testCaseId
								select s;

					//Add on the deleted check
					if (!includeDeleted)
					{
						query = query.Where(s => !s.IsDeleted && (!s.IsLinkedTestCaseDeleted.HasValue || !(s.IsLinkedTestCaseDeleted.Value)));
					}

					//Sort by position
					query = query.OrderBy(s => s.Position).ThenBy(s => s.TestStepId);

					testSteps = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testSteps;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of test steps where we only know the id of one of the steps
		/// </summary>
		/// <param name="testStepId">The id of the test step</param>
		/// <param name="includeDeleted">Should we include deleted steps</param>
		/// <returns>The list of steps</returns>
		public List<TestStepView> RetrieveStepsForTestCaseByStepId(int testStepId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveStepsForTestCaseByStepId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestStepView> testSteps;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First get the test case id from the passed in step
					var query1 = from s in context.TestSteps
								 where s.TestStepId == testStepId
								 select s.TestCaseId;

					var query2 = from s in context.TestStepsView
								 where query1.Contains(s.TestCaseId)
								 select s;

					//Add on the deleted check
					if (!includeDeleted)
					{
						query2 = query2.Where(s => !s.IsDeleted && (!s.IsLinkedTestCaseDeleted.HasValue || !(s.IsLinkedTestCaseDeleted.Value)));
					}

					//Sort by position
					query2 = query2.OrderBy(s => s.Position).ThenBy(s => s.TestStepId);

					testSteps = query2.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testSteps;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves all test steps in the specified project that ARE marked for deletion.</summary>
		/// <param name="projectId">The project ID to get items for.</param>
		/// <returns></returns>
		public List<TestStep> RetrieveAllDeletedSteps(int projectId)
		{
			const string METHOD_NAME = "RetrieveAllDeletedSteps()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TestStep> deletedTestSteps;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestSteps
								where t.TestCase.ProjectId == projectId && t.IsDeleted
								orderby t.TestStepId
								select t;

					deletedTestSteps = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return deletedTestSteps;
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				//Do not rethrow.
				return new List<TestStep>();
			}
		}

		/// <summary>
		/// Retrieves a single test step ID
		/// </summary>
		/// <param name="projectId">The id of the project (leave as null to not check)</param>
		/// <param name="testStepId">The id of the test step</param>
		/// <param name="includeDeleted">Should we return a deleted step</param>
		/// <returns>The test step or NULL if no record - no exception is thrown</returns>
		public TestStep RetrieveStepById(int? projectId, int testStepId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveStepById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TestStep testStep;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from s in context.TestSteps
									.Include(t => t.TestCase)
									.Include(t => t.LinkedTestCase)
								where s.TestStepId == testStepId
								select s;

					//Add on the project check
					if (projectId.HasValue)
					{
						query = query.Where(s => s.TestCase.ProjectId == projectId.Value);
					}

					//Add on the deleted check
					if (!includeDeleted)
					{
						query = query.Where(s => !s.IsDeleted && (s.LinkedTestCase == null || !s.LinkedTestCase.IsDeleted));
					}

					testStep = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testStep;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a single test step ID
		/// </summary>
		/// <param name="testCaseId">The id of the parent test case (leave as null to not check)</param>
		/// <param name="testStepId">The id of the test step</param>
		/// <param name="includeDeleted">Should we return a deleted step</param>
		/// <returns>The test step view or NULL if no record - no exception is thrown</returns>
		public TestStepView RetrieveStepById2(int? testCaseId, int testStepId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveStepById2";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TestStepView testStep;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from s in context.TestStepsView
								where s.TestStepId == testStepId
								select s;

					//Add on the test case check
					if (testCaseId.HasValue)
					{
						query = query.Where(s => s.TestCaseId == testCaseId.Value);
					}

					//Add on the deleted check
					if (!includeDeleted)
					{
						query = query.Where(s => !s.IsDeleted && (!s.IsLinkedTestCaseDeleted.HasValue || !(s.IsLinkedTestCaseDeleted.Value)));
					}

					testStep = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testStep;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Marks the specified step as being deleted.</summary>
		/// <param name="userId">The user performing the delete.</param>
		/// <param name="testCaseId">The test case ID.</param>
		/// <param name="testStepId">The test step ID.</param>
		public void MarkStepAsDeleted(int userId, int testCaseId, int testStepId)
		{
			const string METHOD_NAME = "MarkStepAsDeleted()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//We need to update the parameter hierarchy if a linked test case step is deleted
				int? linkedTestCaseId = null;
				int projectId = 0;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the test step we need to delete
					var query = from s in context.TestSteps.Include(s => s.TestCase)
								where s.TestStepId == testStepId && s.TestCaseId == testCaseId && !s.IsDeleted
								select s;

					//Get the test step
					TestStep testStep = query.FirstOrDefault();
					if (testStep != null)
					{
						//See if we have a linked step
						linkedTestCaseId = testStep.LinkedTestCaseId;
						projectId = testStep.TestCase.ProjectId;

						//Update the test step
						testStep.StartTracking();
						testStep.ConcurrencyDate = DateTime.UtcNow;
						testStep.LastUpdateDate = DateTime.UtcNow;
						testStep.IsDeleted = true;

						//Update the last update date of the test case itself
						testStep.TestCase.StartTracking();
						testStep.TestCase.LastUpdateDate = DateTime.UtcNow;

						//Commit the changes
						context.SaveChanges();

						//Now make a log history.
						new HistoryManager().LogDeletion(testStep.TestCase.ProjectId, userId, Artifact.ArtifactTypeEnum.TestStep, testStepId, DateTime.UtcNow);

						//Finally update the test case flag
						var query2 = from t in context.TestCases.Include(t => t.TestSteps)
									 where t.TestCaseId == testCaseId
									 select t;

						TestCase testCase = query2.FirstOrDefault();
						if (testCase != null && testCase.IsTestSteps && testCase.TestSteps.Count(s => !s.IsDeleted) < 1)
						{
							testCase.StartTracking();
							testCase.IsTestSteps = false;
						}
						context.SaveChanges();
					}
				}

				//If we have a linked step, refresh the hierarchy
				if (linkedTestCaseId.HasValue && projectId > 0)
				{
					this.TestCase_RefreshParameterHierarchy(projectId, linkedTestCaseId.Value);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates a test step into the database
		/// </summary>
		/// <param name="testStep">The test step to persist</param>
		/// <param name="userId">The id of the user making the change</param>
		/// <param name="rollbackId">Whether the update is a rollback or not. Pass the rollback changeset id if so</param>
		/// <param name="updHistory">Whether or not to update history. Default: TRUE</param>
		public void UpdateStep(TestStep testStep, int userId, long? rollbackId = null, bool updHistory = true)
		{
			const string METHOD_NAME = "Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//If we have a null entity just return
			if (testStep == null)
			{
				return;
			}

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Start tracking changes
					testStep.StartTracking();

					//Update the last-update and concurrency dates
					testStep.LastUpdateDate = DateTime.UtcNow;
					testStep.ConcurrencyDate = DateTime.UtcNow;

					//Update the last updated date of the test case that this step belongs to (if provided)
					if (testStep.TestCase != null)
					{
						testStep.TestCase.StartTracking();
						testStep.TestCase.LastUpdateDate = DateTime.UtcNow;
					}

					//Now apply the changes
					context.TestSteps.ApplyChanges(testStep);

					//Save the changes, recording any history changes, and sending any notifications
					context.SaveChanges(userId, true, true, rollbackId);
				}
			}
			catch (OptimisticConcurrencyException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Inserts a linked test case into the test step list at a point in front of the passed in position number
		/// </summary>
		/// <param name="userId">The user we're viewing the test cases as</param>
		/// <param name="testCaseId">The test-case-id of the test case that the link is to be added to</param>
		/// <param name="position">The position that we want to insert in front of - inserts at end if not provided</param>
		/// <param name="linkedTestCaseId">The test case id of the test case being linked to</param>
		/// <param name="parameters">Any parameter values to pass (name,value) or NULL</param>
		/// <param name="logHistory">Should we log history of the new link</param>
		/// <param name="refreshParameterHierarchy">Should we refresh the parameter hierarchy</param>
		/// <returns>The ID of the newly created test step</returns>
		/// <remarks>All linked tests are created with execution status of 'N/A'</remarks>
		public int InsertLink(int userId, int testCaseId, int? position, int linkedTestCaseId, Dictionary<string, string> parameters, bool logHistory = true, bool refreshParameterHierarchy = true)
		{
			const string METHOD_NAME = CLASS_NAME + "InsertLink()";

			Logger.LogEnteringEvent(METHOD_NAME);

			int testStepId = -1;

			try
			{
				//First lets retrieve the test-case and its associated test-steps
				TestCase testCase = RetrieveByIdWithSteps(null, testCaseId, true);

				//Make sure that we have a single row of populated test case data
				if (testCase == null)
				{
					throw new ArtifactNotExistsException("The passed in test case id doesn't correspond to a matching test case!");
				}

				//Get the project id of the test case
				int projectId = testCase.ProjectId;

				//Make sure we are not linking a test case to itself
				if (testCaseId == linkedTestCaseId)
				{
					throw new ArgumentException("You cannot link a test case to itself.");
				}

				//If we have no position number, simply default to the next available position
				if (!position.HasValue || position < 1)
				{
					if (testCase.TestSteps.Count == 0)
					{
						//position = 1;
						position = 0;
					}
					else
					{
						position = testCase.TestSteps.Max(g => g.Position) + 1;
					}
				}

				//Get the flag if we need to record position changes. (Insert changes are still logged.)
				bool logBaseline = new ProjectSettings(testCase.ProjectId).BaseliningEnabled && Global.Feature_Baselines && logHistory;
				HistoryManager hMgr = new HistoryManager();

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Begin transaction - needed to maintain integrity of position ordering
					using (TransactionScope transactionScope = new TransactionScope())
					{

						//Attach the test case and existing steps
						context.TestCases.ApplyChanges(testCase);
						testCase.StartTracking();

						//Now we need to move down any of the other test-steps that are below the passed-in position
						long? createSpaceChangeSetId = null; //Record for all the steps.
						foreach (var laterStep in testCase.TestSteps.Where(t => t.Position >= position.Value))
						{
							laterStep.StartTracking();
							int oldPos = laterStep.Position;
							laterStep.Position++;

							if (logBaseline)
								createSpaceChangeSetId = hMgr.RecordTestStepPosition(
									testCase.ProjectId,
									userId,
									testCase.TestCaseId,
									testCase.Name,
									laterStep.TestStepId,
									oldPos,
									laterStep.Position,
									createSpaceChangeSetId
									);
						}

						//Fill out the entity with data for new linked test case
						TestStep testStep = new TestStep();
						testCase.TestSteps.Add(testStep);
						testStep.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotApplicable;
						testStep.Description = GlobalResources.General.TestCase_Call;
						testStep.ExpectedResult = null;
						testStep.SampleData = null;
						testStep.Position = position.Value;
						testStep.LastUpdateDate = DateTime.UtcNow;
						testStep.IsAttachments = false;
						testStep.LinkedTestCaseId = linkedTestCaseId;
						testStep.ConcurrencyDate = DateTime.UtcNow;

						//Specify that the test case has steps
						testCase.IsTestSteps = true;

						//Finally we need to reset the test case execution status back to 'Not Run'
						//if it has the existing status of Passed
						if (testCase.ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.Passed)
						{
							testCase.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotRun;
						}

						//Save the changes
						context.SaveChanges(userId, false, true, null);

						//Commit transaction - needed to maintain integrity of position ordering
						transactionScope.Complete();

						//Get the ID.
						testStepId = testStep.TestStepId;
					}
				}

				//Log history.
				if (logHistory)
				{
					hMgr.LogCreation(testCase.ProjectId, userId, Artifact.ArtifactTypeEnum.TestStep, testStepId, DateTime.UtcNow);
				}

				//See if we need to log positional changes (only in baselining.
				if (logBaseline)
				{
					if (hMgr == null) hMgr = new HistoryManager();

					hMgr.RecordTestStepPosition(
						testCase.ProjectId,
						userId,
						testCase.TestCaseId,
						testCase.Name,
						testStepId,
						-1,
						position.Value);
				}

				//Now we need to add any of the parameter values
				if (parameters != null)
				{
					//First retrieve the list of parameters, both those directly defined
					//on the child test case and those inherited, but not already set
					List<TestCaseParameter> testCaseParameters = RetrieveParameters(linkedTestCaseId, true, false);
					foreach (TestCaseParameter testCaseParameter in testCaseParameters)
					{
						//See if we have a match for this parameter name in the dictionary
						if (parameters.ContainsKey(testCaseParameter.Name))
						{
							string parameterValue = parameters[testCaseParameter.Name];
							InsertParameterValue(projectId, testStepId, testCaseParameter.TestCaseParameterId, parameterValue);
						}
					}
				}

				//Refresh the hierarchy of parameters for the test cases
				if (refreshParameterHierarchy)
				{
					TestCase_RefreshParameterHierarchy(projectId, linkedTestCaseId);
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return testStepId;
			}
			catch (SqlException sqlException)
			{
				//Recursive issue
				if (sqlException.Number == 530)
				{
					//We need to delete the link but then rethrow the exception as a specific type
					if (testStepId != -1)
					{
						DeleteStepFromDatabase(userId, testStepId, false);
					}
					throw new EntityInfiniteRecursionException("A recursive query has encountered an infinite loop condition");
				}
				else
				{
					Logger.LogErrorEvent(METHOD_NAME, sqlException);
					throw;
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Marks a step as being undeleted.</summary>
		/// <param name="userId">The user doing the undelete.</param>
		/// <param name="testStepId">The test step ID.</param>
		/// <param name="rollbackId">The id of the changeset if part of a rollback</param>
		/// <param name="updHist">Whether to update history or not. Default:TRUE</param>
		public void UndeleteStep(int testStepId, int userId, long rollbackId, bool updHist = true)
		{
			const string METHOD_NAME = "UndeleteStep()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			int projectId = -1;
			int testCaseId = -1;
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var query = from s in context.TestSteps.Include(s => s.TestCase)
							where s.TestStepId == testStepId && s.IsDeleted
							select s;

				TestStep testStep = query.FirstOrDefault();
				if (testStep != null)
				{
					projectId = testStep.TestCase.ProjectId;
					testCaseId = testStep.TestCaseId;

					//Update the test step
					testStep.StartTracking();
					testStep.LastUpdateDate = DateTime.UtcNow;
					testStep.IsDeleted = false;

					//Update the test case
					testStep.TestCase.StartTracking();
					testStep.TestCase.LastUpdateDate = DateTime.UtcNow;

					context.SaveChanges();
				}
			}

			//Update the steps flag.
			if (testCaseId > 0)
			{
				TestCase testCase = RetrieveByIdWithSteps(null, testCaseId, false);
				UpdateTestStepFlag(testCaseId, (testCase.TestSteps.Count > 0));
			}

			//Now make a log history.
			if (updHist && projectId > 0)
			{
				new HistoryManager().LogUnDeletion(projectId, userId, Artifact.ArtifactTypeEnum.TestStep, testStepId, rollbackId, DateTime.UtcNow);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Updates the test step flag for a particular test case</summary>
		/// <param name="testCaseId">The ID of the test case</param>
		/// <param name="hasSteps">The true/false value of the flag</param>
		protected void UpdateTestStepFlag(int testCaseId, bool hasSteps)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				//Update the test case flag
				var query = from t in context.TestCases
							where t.TestCaseId == testCaseId
							select t;

				TestCase testCase = query.FirstOrDefault();
				if (testCase != null)
				{
					testCase.StartTracking();
					testCase.LastUpdateDate = DateTime.UtcNow;
					testCase.IsTestSteps = hasSteps;
					context.SaveChanges();
				}
			}
		}

		/// <summary>Deletes a test-step in the system that has the specified ID</summary>
		/// <param name="userId">The user that is doing the delete</param>
		/// <param name="testCaseId">The ID of the test-case containing the step</param>
		/// <param name="testStepId">The ID of the test-step to be deleted</param>
		public void DeleteStepFromDatabase(int userId, int testStepId, bool logHistory = true)
		{
			const string METHOD_NAME = CLASS_NAME + "DeleteStepFromDatabase()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				//First we need to retrieve the test step itself to get the test case id
				TestStep testStepToDelete = RetrieveStepById(null, testStepId, true);

				//Make sure it still exists
				if (testStepToDelete != null)
				{
					//Now retrieve the test case and all its steps
					int testCaseId = testStepToDelete.TestCaseId;
					TestCase testCase = RetrieveByIdWithSteps(null, testCaseId, true);
					int existingTestStepCount = testCase.TestSteps.Count;

					//Attach to the context and apply changes
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						context.TestCases.ApplyChanges(testCase);
						testCase.StartTracking();

						//Get the flag if we need to record baseline changes. (Insert changes are still logged.)
						bool logBaseline = new ProjectSettings(testCase.ProjectId).BaseliningEnabled && Global.Feature_Baselines;
						HistoryManager hMgr = new HistoryManager();

						//First we need to move up any of the other test-steps that are below the passed-in test step
						long? createSpaceChangeSetId = null; //Record for all the steps.
						foreach (TestStep laterStep in testCase.TestSteps.Where(g => g.Position > testStepToDelete.Position))
						{
							laterStep.StartTracking();
							int oldPos = laterStep.Position;
							laterStep.Position--;

							if (logBaseline)
								createSpaceChangeSetId = hMgr.RecordTestStepPosition(
									testCase.ProjectId,
									userId,
									testCase.TestCaseId,
									testCase.Name,
									laterStep.TestStepId,
									oldPos,
									laterStep.Position,
									createSpaceChangeSetId
									);
						}

						//Update the last updated date of the test case
						testCase.LastUpdateDate = DateTime.UtcNow;

						//Commit these changes
						context.SaveChanges(userId, true, false, null);

						//Next we need to delete any artifact links from this test step
						new ArtifactLinkManager().DeleteByArtifactId(Artifact.ArtifactTypeEnum.TestStep, testStepId);

						//Next we need to delete any attachments associated with the test step
						new AttachmentManager().DeleteByArtifactId(testStepId, Artifact.ArtifactTypeEnum.TestStep);

						//Next we need to delete any custom properties associated with the test step
						new CustomPropertyManager().ArtifactCustomProperty_DeleteByArtifactId(testStepId, Artifact.ArtifactTypeEnum.TestStep);

						//Log to history. Has to be BEFORE the actual delete, since we need infor on it to record history.
						if (logHistory)
							hMgr.LogPurge(testCase.ProjectId, userId, Artifact.ArtifactTypeEnum.TestStep, testStepId, DateTime.UtcNow);
						if (logBaseline)
							hMgr.RecordTestStepPosition(
								testCase.ProjectId,
								userId,
								testCase.TestCaseId,
								testCase.Name,
								testStepToDelete.TestStepId,
								testStepToDelete.Position,
								-1,
								createSpaceChangeSetId);

						//Finally call the stored prodedure to delete the test step
						context.TestCase_DeleteStep(testStepId);
					}
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Inserts a new test step into the system at a point in front of the passed in position number</summary>
		/// <param name="userId">The user we're viewing the test cases as</param>
		/// <param name="testCaseId">The test-case-id of the test case that the step is to be added to</param>
		/// <param name="position">The position that we want to insert in front of, NULL inserts at end</param>
		/// <param name="description">The description of the test step</param>
		/// <param name="expectedResult">The expected result</param>
		/// <param name="sampleData">Any sample data that the user needs to type in</param>
		/// <param name="logHistory">Whether to log history on creation or not. Default:TRUE</param>
		/// <param name="executionStatusId">Do we want to set an initial execution status</param>
		/// <returns>The ID of the newly created test step</returns>
		/// <remarks>1) All test steps are created with execution status of 'not run'.
		/// 2) Adding test steps to an existing test case changes the test case's status back to 'not run' as well</remarks>
		public int InsertStep(int userId, int testCaseId, int? position, string description, string expectedResult, string sampleData, bool logHistory = true, int? executionStatusId = null)
		{
			const string METHOD_NAME = CLASS_NAME + "InsertStep()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				int testStepId;

				//First lets retrieve the test-case and its associated test-steps
				TestCase testCase = RetrieveByIdWithSteps(null, testCaseId, true);

				//Make sure that we have a single row of populated test case data
				if (testCase == null)
				{
					throw new ArtifactNotExistsException("The passed in test case id doesn't correspond to a matching test case!");
				}

				int testCaseExecutionStatusId = testCase.ExecutionStatusId;

				//If we have no position number, simply default to the next available position
				if (!position.HasValue || position < 1)
				{
					if (testCase.TestSteps.Count == 0)
					{
						//position = 1;
						position = 0;
					}
					else
					{
						position = testCase.TestSteps.Max(g => g.Position) + 1;
					}
				}

				//Get the flag if we need to record position changes. (Insert changes are still logged.)
				bool logBaseline = new ProjectSettings(testCase.ProjectId).BaseliningEnabled && Global.Feature_Baselines && logHistory;
				HistoryManager hMgr = new HistoryManager();

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Begin transaction - needed to maintain integrity of position ordering
					using (TransactionScope transactionScope = new TransactionScope())
					{
						//Attach the test case and existing steps
						context.TestCases.ApplyChanges(testCase);
						testCase.StartTracking();

						//Now we need to move down any of the other test-steps that are below the passed-in position
						long? createSpaceChangeSetId = null; //Record for all the steps.
						foreach (var laterStep in testCase.TestSteps.Where(t => t.Position >= position.Value))
						{
							laterStep.StartTracking();
							int oldPos = laterStep.Position;
							laterStep.Position++;

							if (logBaseline)
								createSpaceChangeSetId = hMgr.RecordTestStepPosition(
									testCase.ProjectId,
									userId,
									testCase.TestCaseId,
									testCase.Name,
									laterStep.TestStepId,
									oldPos,
									laterStep.Position,
									createSpaceChangeSetId
									);
						}

						//Fill out the entity with data for new test step
						TestStep testStep = new TestStep();
						testCase.TestSteps.Add(testStep);
						testStep.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotRun;
						testStep.Description = description;
						testStep.ExpectedResult = expectedResult;
						testStep.SampleData = sampleData;
						testStep.Position = position.Value;
						testStep.LastUpdateDate = DateTime.UtcNow;
						testStep.IsAttachments = false;
						testStep.LinkedTestCaseId = null;   //Use InsertLink() instead for new linked test steps
						testStep.ConcurrencyDate = DateTime.UtcNow;
						if (executionStatusId.HasValue)
						{
							testStep.ExecutionStatusId = executionStatusId.Value;
						}

						//Specify that the test case has steps
						testCase.IsTestSteps = true;

						//Finally we need to reset the test case execution status back to 'Not Run'
						//if it has the existing status of Passed
						if (testCase.ExecutionStatusId == (int)TestCase.ExecutionStatusEnum.Passed)
						{
							testCase.ExecutionStatusId = (int)TestCase.ExecutionStatusEnum.NotRun;
						}

						//Save the changes
						context.SaveChanges(userId, logHistory, true, null);

						//Commit transaction - needed to maintain integrity of position ordering
						transactionScope.Complete();

						//Get the ID.
						testStepId = testStep.TestStepId;
					}
				}

				//Log history.
				if (logHistory)
				{
					hMgr.LogCreation(testCase.ProjectId, userId, Artifact.ArtifactTypeEnum.TestStep, testStepId, DateTime.UtcNow);
				}

				//See if we need to log positional changes (only in baselining.
				if (logBaseline)
				{
					if (hMgr == null) hMgr = new HistoryManager();

					hMgr.RecordTestStepPosition(
						testCase.ProjectId,
						userId,
						testCase.TestCaseId,
						testCase.Name,
						testStepId,
						-1,
						position.Value);
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return testStepId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Moves a test step in the test case list</summary>
		/// <param name="sourceTestStepId">The id of the test step we want to move</param>
		/// <param name="destTestStepId">The destination of where we want to move it to (Null means end of the list)</param>
		/// <param name="testCaseId">The id of the test case we're interested in</param>
		/// <remarks>Throws an exception if you pass-in a non-existant source test step</remarks>
		public void MoveStep(int testCaseId, int sourceTestStepId, int? destTestStepId, int userId)
		{
			const string METHOD_NAME = CLASS_NAME + "MoveStep";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				using (var ct = new SpiraTestEntitiesEx())
				{
					using (TransactionScope transactionScope = new TransactionScope())
					{
						// Pull the requirement with steps.
						TestCase baseTestCase = ct.TestCases
							.Include(g => g.TestSteps)
							.SingleOrDefault(r => r.TestCaseId == testCaseId);

						//If it don't exist, throw an error (handled by RetrieveByIdWithSteps() in TestCaseManager).
						if (baseTestCase == null)
							throw new ArtifactNotExistsException();

						//Make sure that we have the row to be moved in this dataset
						TestStep testStepToMove =
							baseTestCase.TestSteps.SingleOrDefault(s => s.TestStepId == sourceTestStepId);
						if (testStepToMove == null)
							throw new ArtifactNotExistsException("The passed in test case id and test step id doesn't correspond to a matching test step in the test case!");
						//Now grab the test step at the destination.
						TestStep testStepAtDest = null;
						if (destTestStepId.HasValue)
							testStepAtDest = baseTestCase.TestSteps.SingleOrDefault(s => s.TestStepId == destTestStepId.Value);
						// Record the initial Test Step position.
						int startingPosition = testStepToMove.Position;
						int endingPosition = (!destTestStepId.HasValue
							? baseTestCase.TestSteps.Max(f => f.Position) // If nothing given, we want the highest.
							: testStepAtDest.Position); // Otherwise, we want to put this at this position.
						if (startingPosition < endingPosition &&
							destTestStepId.HasValue)
							endingPosition--; //This is needed since the item goes in FRONT of the one selected, unless it's at the end.

						//This is needed for or WHERE clause.
						int lowPosition = Math.Min(startingPosition, endingPosition);
						int highPosition = Math.Max(startingPosition, endingPosition);

						//Check that the two items are not the same. If they are, nothing's changing.
						if (startingPosition == endingPosition) return;  //Nothing to do. Just return and say it's done.

						//Get the HistoryManager ifneeded.
						HistoryManager hMgr = null;
						bool recordHistory = new ProjectSettings(baseTestCase.ProjectId).BaseliningEnabled && Global.Feature_Baselines;
						if (recordHistory)
							hMgr = new HistoryManager();

						//The flag, in case we need to save.
						bool saveChanges = false;

						//Pull all the Test Steps that are between (inclusive) of our range.
						//  Steps outside of our ranger aren't moving, see.
						ct.ContextOptions.LazyLoadingEnabled = false; //Turn this off so the query does not change as WE make changes.
						var stepList = ct.TestSteps
							.Include(s => s.TestCase)
							.OrderBy(s => s.Position)
							.Where(s =>
								s.TestCaseId == testCaseId &&
								s.Position >= lowPosition &&
								s.Position <= highPosition)
							.ToList();
						ct.ContextOptions.LazyLoadingEnabled = true; //Re-enable, in case.

						//Loop through each one. 
						bool doIncrement = true; //Flag. Until we get to the new position, we INCREMENT the existing ones.
						long? recordHistoyChangeset = null; //In case we record history, we want to record this changeset.
						foreach (var step in stepList)
						{
							//Record the old position first.
							int oldPos = step.Position;

							//Start tracking.
							step.StartTracking();

							//Determine wheat we are doing to the position.
							if (step.Position == startingPosition) //Setting it to requesd value.
							{
								step.Position = endingPosition;
								doIncrement = !doIncrement;
								saveChanges = true;
							}
							else  //Moving it up or down.
							{
								step.Position = (doIncrement
									? ++step.Position
									: --step.Position);
								saveChanges = true;
							}

							//Record history.
							if (recordHistory && saveChanges)
								recordHistoyChangeset = hMgr.RecordTestStepPosition(
									baseTestCase.ProjectId,
									userId,
									baseTestCase.TestCaseId,
									baseTestCase.Name,
									step.TestStepId,
									oldPos,
									step.Position,
									recordHistoyChangeset);
						}
						//Update the TestCase date, and save all oour changes.
						if (saveChanges)
						{
							stepList[0].TestCase.StartTracking();
							stepList[0].TestCase.LastUpdateDate = DateTime.UtcNow;
							ct.SaveChanges();
						}

						//Commit the transaction
						transactionScope.Complete();
					}
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>Copies a single test step/link</summary>
		/// <param name="userId">The user we're viewing the test cases as</param>
		/// <param name="projectId">The current project</param>
		/// <param name="testCaseId">The test case that the steps belong to</param>
		/// <param name="testStepId">The test step being copied</param>
		/// <returns>The id of the newly created test step/link</returns>
		/// <remarks>This function currently is limited to adding the copied items just in front of the item being copied</remarks>
		public int CopyTestStep(int userId, int projectId, int testCaseId, int testStepId)
		{
			const string METHOD_NAME = "CopyTestStep";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First get the list of steps for the test case
				TestCase testCase = RetrieveByIdWithSteps(projectId, testCaseId);

				//Now we need to locate the test step
				TestStep testStep = testCase.TestSteps.FirstOrDefault(s => s.TestStepId == testStepId);
				if (testStep == null)
				{
					throw new ArtifactNotExistsException("Unable to locate test step " + testStepId + " in the project. It no longer exists!");
				}

				//See if we have the case of a linked test case (rather than a real step)
				int newTestStepId = -1;
				if (testStep.LinkedTestCaseId.HasValue)
				{
					//We have a linked test case
					//Lookup the linked test case
					int linkedTestCaseId = testStep.LinkedTestCaseId.Value;

					//Get the parameter values
					Dictionary<string, string> parameters = new Dictionary<string, string>();
					List<TestStepParameter> testStepParameters = RetrieveParameterValues(testStep.TestStepId);
					for (int j = 0; j < testStepParameters.Count; j++)
					{
						parameters.Add(testStepParameters[j].Name, testStepParameters[j].Value);
					}

					newTestStepId = InsertLink(
						userId,
						testCaseId,
						testStep.Position,
						linkedTestCaseId,
						parameters, true);
				}
				else
				{
					//We have a real test step

					//Insert the test step to the destination in the order they appear in the source
					newTestStepId = InsertStep(
						userId,
						testCaseId,
						testStep.Position,
						testStep.Description,
						testStep.ExpectedResult,
						testStep.SampleData,
						true
						);

					//Get the template for this project
					int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

					//Copy custom properties..
					new CustomPropertyManager().ArtifactCustomProperty_Copy(projectId, projectTemplateId, testStepId, newTestStepId, Artifact.ArtifactTypeEnum.TestStep, userId);

					//Finally copy over any file attachments
					AttachmentManager attachment = new AttachmentManager();
					attachment.Copy(projectId, Artifact.ArtifactTypeEnum.TestStep, testStep.TestStepId, newTestStepId);
				}

				//Update testcase's last updated date.
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.TestCases.ApplyChanges(testCase);
					testCase.StartTracking();
					testCase.LastUpdateDate = DateTime.UtcNow;
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return newTestStepId;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception.Message);
				Logger.Flush();
				throw;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// See if the test case is in a workflow status that allows the executing of test cases
		/// </summary>
		/// <returns>True if the status allows execution</returns>
		/// <remarks>The ExecutionStatusId field determines if we can execute or not</remarks>
		public bool IsTestCaseInExecutableStatus(TestCaseView testCase, List<TestCaseWorkflowField> fields)
		{
			return !fields.Any(f => f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive && f.ArtifactField.Name == "ExecutionStatusId");
		}

		/// <summary>
		/// See if the test case is in a workflow status that allows the executing of test cases
		/// </summary>
		/// <returns>True if the status allows test step editing</returns>
		/// <remarks>The IsTestSteps field determines if we can edit steps or not</remarks>
		public bool AreTestStepsEditableInStatus(TestCaseView testCase, List<TestCaseWorkflowField> fields)
		{
			return !fields.Any(f => f.WorkflowFieldStateId == (int)WorkflowFieldState.WorkflowFieldStateEnum.Inactive && f.ArtifactField.Name == "IsTestSteps");
		}

		/// <summary>
		/// See if the test case is in a workflow status that allows the executing of test cases
		/// </summary>
		/// <param name="testCaseId">The id of the test case</param>
		/// <returns>True if the status allows execution</returns>
		/// <remarks>The ExecutionStatusId field determines if we can execute or not</remarks>
		public bool IsTestCaseInExecutableStatus(int testCaseId)
		{
			const string METHOD_NAME = "IsTestCaseInExecutableStatus";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First get the test case itself
				TestCaseView testCase = RetrieveById(null, testCaseId);

				TestCaseWorkflowManager testCaseWorkflowManager = new TestCaseWorkflowManager();
				int workflowId = testCaseWorkflowManager.Workflow_GetForTestCaseType(testCase.TestCaseTypeId);
				List<TestCaseWorkflowField> fields = testCaseWorkflowManager.Workflow_RetrieveFieldStates(workflowId, testCase.TestCaseStatusId);
				bool isExecutable = IsTestCaseInExecutableStatus(testCase, fields);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return isExecutable;
			}
			catch (ArtifactNotExistsException)
			{
				//Test case does not exist
				return false;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// See if the test case is in a workflow status that allows the executing of test cases
		/// </summary>
		/// <param name="testCaseId">The id of the test case</param>
		/// <returns>True if the status allows test step editing</returns>
		/// <remarks>The IsTestSteps field determines if we can edit steps or not</remarks>
		public bool AreTestStepsEditableInStatus(int testCaseId)
		{
			const string METHOD_NAME = CLASS_NAME + "IsTestCaseInExecutableStatus()";

			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				//First get the test case itself
				TestCaseView testCase = RetrieveById(null, testCaseId);
				TestCaseWorkflowManager testCaseWorkflowManager = new TestCaseWorkflowManager();
				int workflowId = testCaseWorkflowManager.Workflow_GetForTestCaseType(testCase.TestCaseTypeId);
				List<TestCaseWorkflowField> fields = testCaseWorkflowManager.Workflow_RetrieveFieldStates(workflowId, testCase.TestCaseStatusId);
				bool areStepsEditable = AreTestStepsEditableInStatus(testCase, fields);

				Logger.LogExitingEvent(METHOD_NAME);
				return areStepsEditable;
			}
			catch (ArtifactNotExistsException)
			{
				//Test case does not exist
				return false;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Private and Protected Methods

		/// <summary>Copies the test coverage information for a specific test case</summary>
		/// <param name="projectId">The current project</param>
		/// <param name="sourceTestCaseId">The test case we're copying coverage FROM</param>
		/// <param name="destTestCaseId">The test case we're copying coverage TO</param>
		protected void CopyCoverage(int projectId, int sourceTestCaseId, int destTestCaseId, int userId)
		{
			//Instantiate a Requirement business object
			RequirementManager requirementManager = new RequirementManager();

			//Now we need to copy across any coverage information
			List<int> destRequirements = new List<int>();
			List<RequirementView> sourceRequirements = requirementManager.RetrieveCoveredByTestCaseId(User.UserInternal, projectId, sourceTestCaseId);
			foreach (RequirementView sourceRequirement in sourceRequirements)
			{
				destRequirements.Add(sourceRequirement.RequirementId);
			}

			requirementManager.AddToTestCase(projectId, destTestCaseId, destRequirements, userId);
		}

		/// <summary>Copies all the test steps for a specific test case</summary>
		/// <param name="userId">The user we're viewing the test cases as</param>
		/// <param name="projectId">The current project</param>
		/// <param name="sourceTestCaseId">The test case we're copying the steps FROM</param>
		/// <param name="destTestCaseId">The test case we're copying the steps TO</param>
		protected void CopyTestSteps(int userId, int projectId, int sourceTestCaseId, int destTestCaseId)
		{
			//First get the list of steps for the source test case
			TestCase sourceTestCase = RetrieveByIdWithSteps(null, sourceTestCaseId);

			//Now we need to copy across the steps to the destination
			bool refreshParameterHierarchy = false;
			foreach (TestStep testStep in sourceTestCase.TestSteps)
			{
				//See if we have the case of a linked test case (rather than a real step)
				if (testStep.LinkedTestCaseId.HasValue)
				{
					//We have a linked test case
					//Lookup the linked test case
					int linkedTestCaseId = testStep.LinkedTestCaseId.Value;

					//Get the parameter values
					Dictionary<string, string> parameters = new Dictionary<string, string>();
					List<TestStepParameter> testStepParameters = RetrieveParameterValues(testStep.TestStepId);
					foreach (TestStepParameter testStepParameter in testStepParameters)
					{
						if (!parameters.ContainsKey(testStepParameter.Name))
						{
							parameters.Add(testStepParameter.Name, testStepParameter.Value);
						}
					}

					//Insert a link, do not refresh parameter hierarchy, better to do it at the end for the entire case (better performance)
					InsertLink(
						userId,
						destTestCaseId,
						null,
						linkedTestCaseId,
						parameters,
						true,
						false);
					refreshParameterHierarchy = true;
				}
				else
				{
					//We have a real test step

					//Insert the test step to the destination in the order they appear in the source
					int copiedTestStepId = InsertStep(
						userId,
						destTestCaseId,
						null,
						testStep.Description,
						testStep.ExpectedResult,
						testStep.SampleData,
						false
						);

					//Get the template for this project
					int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

					//Copy custom properties..
					new CustomPropertyManager().ArtifactCustomProperty_Copy(projectId, projectTemplateId, testStep.TestStepId, copiedTestStepId, Artifact.ArtifactTypeEnum.TestStep, userId);

					//Finally copy over any file attachments
					AttachmentManager attachment = new AttachmentManager();
					attachment.Copy(projectId, Artifact.ArtifactTypeEnum.TestStep, testStep.TestStepId, copiedTestStepId);
				}
			}

			//Refresh the hierarchy of parameters for the test cases if we need to
			if (refreshParameterHierarchy)
			{
				TestCase_RefreshParameterHierarchy(projectId, destTestCaseId);
			}
		}

		/// <summary>Exports the linked test cases from a parent test case to a new project</summary>
		/// <param name="userId">The user we're viewing the test cases as</param>
		/// <param name="sourceTestCaseId">The test case we're copying the steps FROM</param>
		/// <param name="destTestCaseId">The test case we're copying the steps TO</param>
		/// <param name="destProjectId">The destination project</param>
		/// <param name="testCaseMapping">Mapping of exported test cases</param>
		/// <param name="sourceProjectId">The source project</param>
		/// <remarks>In the situation of linked test cases, we actually have to copy the linked test case</remarks>
		protected void ExportLinkedTestCases(int userId, int sourceProjectId, int sourceTestCaseId, int destProjectId, int destTestCaseId, Dictionary<int, int> testCaseMapping)
		{
			const string METHOD_NAME = "ExportLinkedTestCases()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//First get the list of steps for the source test case
			TestCase sourceTestCase = RetrieveByIdWithSteps(sourceProjectId, sourceTestCaseId);

			//Now we need to export any previously unseen linked test cases to the destination project
			for (int i = 0; i < sourceTestCase.TestSteps.Count; i++)
			{
				//See if we have the case of a linked test case
				if (sourceTestCase.TestSteps[i].LinkedTestCaseId.HasValue)
				{
					//We have a linked test case
					//Lookup the linked test case to see if it was already exported
					int linkedTestCaseId;
					if (!testCaseMapping.ContainsKey(sourceTestCase.TestSteps[i].LinkedTestCaseId.Value))
					{
						try
						{
							//We need to export this additional test case (calls export recursively)
							linkedTestCaseId = TestCase_Export(userId, sourceProjectId, sourceTestCase.TestSteps[i].LinkedTestCaseId.Value, destProjectId, testCaseMapping);
						}
						catch (ArtifactNotExistsException)
						{
							//It was a deleted item. Log a warning an  continue.
							Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "TestCase #" + sourceTestCase.TestSteps[i].LinkedTestCaseId.ToString() + " marked as deleted. Not exporting.");
						}
						catch (Exception ex2)
						{
							Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex2);
							throw ex2;
						}
					}
				}
			}
			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Exports the test steps for a specific test case to a new project</summary>
		/// <param name="userId">The user we're viewing the test cases as</param>
		/// <param name="sourceTestCaseId">The test case we're copying the steps FROM</param>
		/// <param name="destTestCaseId">The test case we're copying the steps TO</param>
		/// <param name="destProjectId">The destination project</param>
		/// <param name="testCaseMapping">Mapping of exported test cases</param>
		/// <param name="sourceProjectId">The source project</param>
		/// <remarks>In the situation of linked test cases, we actually have to copy the linked test case</remarks>
		protected void ExportTestSteps(int userId, int sourceProjectId, int sourceTestCaseId, int destProjectId, int destTestCaseId, Dictionary<int, int> testCaseMapping)
		{
			//First get the list of steps for the source test case
			TestCase sourceTestCase = RetrieveByIdWithSteps(sourceProjectId, sourceTestCaseId);

			//Now we need to copy across the steps to the destination
			for (int i = 0; i < sourceTestCase.TestSteps.Count; i++)
			{
				//See if we have the case of a linked test case (rather than a real step)
				if (!sourceTestCase.TestSteps[i].LinkedTestCaseId.HasValue)
				{
					//We have a real test step

					//Insert the test step to the destination in the order they appear in the source
					int copiedTestStepId = InsertStep(
						userId,
						destTestCaseId,
						sourceTestCase.TestSteps[i].Position,
						sourceTestCase.TestSteps[i].Description,
						sourceTestCase.TestSteps[i].ExpectedResult,
						sourceTestCase.TestSteps[i].SampleData,
						false
						);

					//Finally copy over any file attachments
					AttachmentManager attachmentManager = new AttachmentManager();
					attachmentManager.Export(sourceProjectId, Artifact.ArtifactTypeEnum.TestStep, sourceTestCase.TestSteps[i].TestStepId, destProjectId, copiedTestStepId);

				}
				else
				{
					//We have a linked test case
					//Lookup the linked test case (if we don't have one ignore at this point since we should have already exported it)
					int linkedTestCaseId;
					if (testCaseMapping.ContainsKey(sourceTestCase.TestSteps[i].LinkedTestCaseId.Value))
					{
						linkedTestCaseId = testCaseMapping[sourceTestCase.TestSteps[i].LinkedTestCaseId.Value];

						//Get the parameter values
						Dictionary<string, string> parameters = new Dictionary<string, string>();
						List<TestStepParameter> testStepParameters = RetrieveParameterValues(sourceTestCase.TestSteps[i].TestStepId);
						for (int j = 0; j < testStepParameters.Count; j++)
						{
							parameters.Add(testStepParameters[j].Name, testStepParameters[j].Value);
						}

						InsertLink(
							userId,
							destTestCaseId,
							null,
							linkedTestCaseId,
							parameters,
							true);
					}
				}
			}
		}

		/// <summary>
		/// Executes dynamic SQL against the underlying provider connection and uses
		/// an ADO.NET DataAdapter to return a datatable/dataset.
		/// </summary>
		/// <param name="context">The EF object context</param>
		/// <param name="sql">The dynamic SQL</param>
		/// <remarks>
		/// This is needed because Entities are not well suited to dynamic columns,
		/// so we use untyped datasets for this situation
		/// </remarks>
		protected DataSet ExecuteSql(ObjectContext context, string sql, string dataTable)
		{
			const string METHOD_NAME = "ExecuteSql";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//The return object
				DataSet dataSet = new DataSet();

				System.Data.EntityClient.EntityConnection entityConnection = (System.Data.EntityClient.EntityConnection)context.Connection;
				SqlConnection sqlConnection = null;
				if (entityConnection.StoreConnection is SqlConnection)
				{
					sqlConnection = (SqlConnection)entityConnection.StoreConnection;
				}
				else if (entityConnection.StoreConnection is EFTracingProvider.EFTracingConnection)
				{
					EFTracingProvider.EFTracingConnection tracingConnection = (EFTracingProvider.EFTracingConnection)entityConnection.StoreConnection;
					sqlConnection = (SqlConnection)tracingConnection.WrappedConnection;
				}
				if (sqlConnection == null)
				{
					throw new ApplicationException("Unable to cast StoreConnection: " + entityConnection.StoreConnection.GetType().Name);
				}

				ConnectionState initialState = sqlConnection.State;
				try
				{
					if (initialState != ConnectionState.Open)
					{
						sqlConnection.Open();  // open connection if not already open
					}
					SqlCommand sqlCommand = sqlConnection.CreateCommand();
					using (SqlDataAdapter daReport = new SqlDataAdapter(sqlCommand))
					{
						using (sqlCommand)
						{
							sqlCommand.CommandType = CommandType.Text;
							sqlCommand.CommandText = sql;
							sqlCommand.ExecuteNonQuery();
							daReport.Fill(dataSet, dataTable);
						}
					}
				}
				finally
				{
					if (initialState != ConnectionState.Open)
					{
						sqlConnection.Close(); // only close connection if not initially open
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return dataSet;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		/// <summary>
		/// This exception is thrown if you try and perform an operation that would result in there being no remaining top-level test case folders
		/// </summary>
		public class ProjectDefaultTestCaseFolderException : ApplicationException
		{
			public ProjectDefaultTestCaseFolderException()
			{
			}
			public ProjectDefaultTestCaseFolderException(string message)
				: base(message)
			{
			}
			public ProjectDefaultTestCaseFolderException(string message, Exception inner)
				: base(message, inner)
			{
			}
		}

		/// <summary>This exception is thrown when you try and insert a test case parameter with a name that is already in use for that test case.</summary>
		public class TestCaseDuplicateParameterNameException : ApplicationException
		{
			public TestCaseDuplicateParameterNameException()
			{
			}
			public TestCaseDuplicateParameterNameException(string message)
				: base(message)
			{
			}
			public TestCaseDuplicateParameterNameException(string message, Exception inner)
				: base(message, inner)
			{
			}
		}
	}

	/// <summary>This exception is thrown when you try and create a circular reference in the folder structures (test case, test set, task, documents)</summary>
	public class FolderCircularReferenceException : ApplicationException
	{
		public FolderCircularReferenceException()
		{
		}
		public FolderCircularReferenceException(string message)
			: base(message)
		{
		}
		public FolderCircularReferenceException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
