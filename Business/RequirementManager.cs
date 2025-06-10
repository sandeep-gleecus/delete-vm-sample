using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Linq.Expressions;
using System.Transactions;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

using LinqToEdmx.Model.Conceptual;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>
	/// This class encapsulates all the data access functionality for
	/// reading and writing Requirements that are submitted and managed in the system
	/// </summary>
	public class RequirementManager : ManagerBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Business.RequirementManager::";

		//Cached lists/collections
		protected Dictionary<string, string> coverageFiltersList; /* Cannot be static since localized */
		public static List<RequirementStatus> _staticRequirementStatuses = null;    //Static, not customizable

		protected Dictionary<string, string> completionFiltersList;   /* Cannot be static since localized */

		//Constants
		public const int RELEASE_ID_ACTIVE_RELEASES_ONLY = -2;

		#region Sub Classes

		/// <summary>
		/// A single requirement entry for lookups/dropdowns
		/// </summary>
		public class RequirementLookupEntry
		{
			public int RequirementId { get; set; }
			public string Name { get; set; }
			public bool IsSummary { get; set; }
			public bool IsAlternate { get; set; }
			public string IndentLevel { get; set; }
		}

		#endregion

		#region Internal Methods

		/// <summary>
		/// Copies across the requirement fields and workflows from one project template to another
		/// </summary>
		/// <param name="existingProjectTemplateId">The id of the existing project template</param>
		/// <param name="newProjectTemplateId">The id of the new project template</param>
		/// <param name="requirementWorkflowMapping">The workflow mapping</param>
		/// <param name="requirementTypeMapping">The type mapping</param>
		/// <param name="requirementImportanceMapping">The importance mapping</param>
		/// <param name="customPropertyIdMapping">The custom property mapping</param>
		protected internal void CopyToProjectTemplate(int existingProjectTemplateId, int newProjectTemplateId, Dictionary<int, int> requirementWorkflowMapping, Dictionary<int, int> requirementTypeMapping, Dictionary<int, int> requirementImportanceMapping, Dictionary<int, int> customPropertyIdMapping)
		{
			//***** Now we need to copy across the requirement workflows *****
			RequirementWorkflowManager workflowManager = new RequirementWorkflowManager();
			workflowManager.Workflow_Copy(existingProjectTemplateId, newProjectTemplateId, customPropertyIdMapping, requirementWorkflowMapping);

			//***** Now we need to copy across the requirement types *****
			List<RequirementType> requirementTypes = RequirementType_Retrieve(existingProjectTemplateId, false, false);
			for (int i = 0; i < requirementTypes.Count; i++)
			{
				//Need to retrieve the mapped workflow for this type
				if (requirementTypes[i].RequirementWorkflowId.HasValue && requirementWorkflowMapping.ContainsKey(requirementTypes[i].RequirementWorkflowId.Value))
				{
					int workflowId = requirementWorkflowMapping[requirementTypes[i].RequirementWorkflowId.Value];
					int newRequirementTypeId = RequirementType_Insert(
						newProjectTemplateId,
						requirementTypes[i].Name,
						workflowId,
						requirementTypes[i].IsDefault,
						requirementTypes[i].IsActive,
						requirementTypes[i].IsSteps);
					requirementTypeMapping.Add(requirementTypes[i].RequirementTypeId, newRequirementTypeId);
				}
			}

			//***** Now we need to copy across the requirement importances *****
			List<Importance> requirementImportances = RequirementImportance_Retrieve(existingProjectTemplateId, false);
			for (int i = 0; i < requirementImportances.Count; i++)
			{
				int newImportanceId = RequirementImportance_Insert(
					newProjectTemplateId,
					requirementImportances[i].Name,
					requirementImportances[i].Color,
					requirementImportances[i].IsActive,
					requirementImportances[i].Score);
				requirementImportanceMapping.Add(requirementImportances[i].ImportanceId, newImportanceId);
			}
		}

		/// <summary>
		/// Creates the requirement types, importances, default workflow, transitions and field states
		/// for a new project template using the default template
		/// </summary>
		/// <param name="projectTemplateId">The id of the project</param>
		internal void CreateDefaultEntriesForProjectTemplate(int projectTemplateId)
		{
			const string METHOD_NAME = "CreateDefaultEntriesForProjectTemplate";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to create the requirement priorities
				RequirementImportance_Insert(projectTemplateId, "1 - Critical", "f47457", true, 1);
				RequirementImportance_Insert(projectTemplateId, "2 - High", "f29e56", true, 2);
				RequirementImportance_Insert(projectTemplateId, "3 - Medium", "f5d857", true, 3);
				RequirementImportance_Insert(projectTemplateId, "4 - Low", "f4f356", true, 4);

				//Next we need to create a default workflow for a project
				RequirementWorkflowManager workflowManager = new RequirementWorkflowManager();
				int workflowId = workflowManager.Workflow_InsertWithDefaultEntries(projectTemplateId, GlobalResources.General.Workflow_DefaultWorflow, true).RequirementWorkflowId;

				//Next we need to create the requirement types, associated with this workflow
				RequirementType_Insert(projectTemplateId, "Feature", workflowId, true, true, false);
				RequirementType_Insert(projectTemplateId, "Need", workflowId, false, true, false);
				RequirementType_Insert(projectTemplateId, "Quality", workflowId, false, true, false);
				RequirementType_Insert(projectTemplateId, "Use Case", workflowId, false, true, true);
				RequirementType_Insert(projectTemplateId, "User Story", workflowId, false, true, false);
				RequirementType_Insert(projectTemplateId, "Design Element", workflowId, false, true, false);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		#region Requirement Type Methods

		/// <summary>Retrieves a list of active 'use case' requirement types (i.e. they have steps)</summary>
		/// <returns>List of requirement types</returns>
		/// <param name="projectTemplateId">The id of the project template</param>
		public List<RequirementType> RequirementType_RetrieveUseCases(int projectTemplateId)
		{
			const string METHOD_NAME = "RequirementType_RetrieveUseCases()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				List<RequirementType> requirementTypes;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementTypes
								where r.IsActive && r.ProjectTemplateId == projectTemplateId && r.RequirementTypeId > 0 && r.IsSteps
								orderby r.Name, r.RequirementTypeId
								select r;

					requirementTypes = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirementTypes;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Inserts a new requirement type for a specific project template</summary>
		/// <param name="projectTemplateId">The project template that the requirement type belongs to</param>
		/// <param name="name">The display name of the requirement type</param>
		/// <param name="active">Whether the requirement type is active or not</param>
		/// <param name="workflowId">The workflow id (pass null for project default)</param>
		/// <param name="defaultType">Is this the default (initial) type of newly created requirements</param>
		/// <param name="hasSteps">Does this requirement type support steps (scenario)</param>
		/// <returns>The newly created requirement type id</returns>
		public int RequirementType_Insert(int projectTemplateId, string name, int? workflowId, bool defaultType, bool active, bool hasSteps = false)
		{
			const string METHOD_NAME = "RequirementType_Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//If no workflow provided, simply use the project default workflow
				if (!workflowId.HasValue)
				{
					RequirementWorkflowManager workflowManager = new RequirementWorkflowManager();
					workflowId = workflowManager.Workflow_GetDefault(projectTemplateId).RequirementWorkflowId;
				}

				//For now all have the same icon except use case types
				string icon = (hasSteps) ? "UseCase.gif" : "Requirement.gif";

				int requirementTypeId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					RequirementType requirementType = new RequirementType();
					requirementType.ProjectTemplateId = projectTemplateId;
					requirementType.Name = name.MaxLength(20);
					requirementType.IsDefault = defaultType;
					requirementType.IsActive = active;
					requirementType.IsSteps = hasSteps;
					requirementType.RequirementWorkflowId = workflowId.Value;
					requirementType.Icon = icon;

					context.RequirementTypes.AddObject(requirementType);
					context.SaveChanges();
					requirementTypeId = requirementType.RequirementTypeId;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirementTypeId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Deletes the requirement types for a project template</summary>
		/// <param name="requirementTypeId">The requirement type to be deleted</param>
		/// <remarks>Only used by the unit tests for cleanup purposes</remarks>
		protected internal void RequirementType_Delete(int requirementTypeId)
		{
			const string METHOD_NAME = "RequirementType_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the type
					var query = from r in context.RequirementTypes
								where r.RequirementTypeId == requirementTypeId
								select r;

					RequirementType type = query.FirstOrDefault();
					if (type != null)
					{
						context.RequirementTypes.DeleteObject(type);
						context.SaveChanges();
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Updates the requirement types for a project</summary>
		/// <param name="requirementType">The requirement type to be persisted</param>
		/// <remarks>This method performs the necessary updates</remarks>
		public void RequirementType_Update(RequirementType requirementType)
		{
			const string METHOD_NAME = "RequirementType_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Apply the changes
					context.RequirementTypes.ApplyChanges(requirementType);
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list of requirement types</summary>
		/// <returns>List of requirement statuses</returns>
		/// <param name="activeOnly">Do we want only active ones</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="includePackage">Should we include the 'package' type</param>
		public List<RequirementType> RequirementType_Retrieve(int projectTemplateId, bool includePackage, bool activeOnly = true)
		{
			const string METHOD_NAME = "RequirementType_Retrieve()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				List<RequirementType> requirementTypes;
				if (includePackage)
				{
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						var query = from r in context.RequirementTypes
									where (r.IsActive || !activeOnly) && (r.ProjectTemplateId == projectTemplateId || !r.ProjectTemplateId.HasValue)
									orderby r.RequirementTypeId, r.Name
									select r;

						requirementTypes = query.OrderByDescending(item => item.RequirementTypeId).ToList();
					}

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					return requirementTypes;
				}
				else
				{
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						var query = from r in context.RequirementTypes
									where (r.IsActive || !activeOnly) && r.ProjectTemplateId == projectTemplateId && r.RequirementTypeId > 0
									orderby r.RequirementTypeId, r.Name
									select r;

						requirementTypes = query.OrderByDescending(item => item.RequirementTypeId).ToList();

					}

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					return requirementTypes;
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the default requirement type for the specified template
		/// </summary>
		/// <param name="projectTemplateId">The id of the template</param>
		/// <returns>The default requirement type</returns>
		public RequirementType RequirementType_RetrieveDefault(int projectTemplateId)
		{
			const string METHOD_NAME = "RequirementType_RetrieveDefault";


			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				RequirementType type;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from r in context.RequirementTypes
								where r.ProjectTemplateId == projectTemplateId && r.IsDefault
								select r;

					type = query.FirstOrDefault();
					if (type == null)
					{
						throw new ApplicationException(String.Format(GlobalResources.Messages.Requirement_NoDefaultTypeForProjectTemplate, projectTemplateId));
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return type;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public Requirement Requirement_RetrieveById(int requirementId)
		{
			const string METHOD_NAME = "RequirementType_RetrieveById";


			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				Requirement requirement;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from r in context.Requirements
								where r.RequirementId == requirementId
								select r;

					requirement = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirement;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}


		/// <summary>
		/// Retrieves a requirement type by its ID
		/// </summary>
		/// <param name="requirementTypeId">The id of the type</param>
		/// <returns>The requirement type</returns>
		public RequirementType RequirementType_RetrieveById(int requirementTypeId)
		{
			const string METHOD_NAME = "RequirementType_RetrieveById";


			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				RequirementType type;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from r in context.RequirementTypes
								where r.RequirementTypeId == requirementTypeId
								select r;

					type = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return type;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Retrieves all of the artifact associations between all requirements in a project
		/// </summary>
		/// <param name="count">The total number of associations</param>
		/// <param name="numberOfRows">The number of rows to retrieve</param>
		/// <param name="startRow">The starting row (1-based)</param>
		/// <param name="projectId">The id of the project</param>
		/// <returns>All of the explicit associations between non-deleted requirements</returns>
		public List<ArtifactLink> ArtifactLink_RetrieveAllForRequirements(int projectId, out int count, int startRow = 1, int numberOfRows = 15)
		{
			const string METHOD_NAME = "ArtifactLink_RetrieveAllForRequirements";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<ArtifactLink> associations;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from a in context.ArtifactLinks
								join r1 in context.Requirements on a.SourceArtifactId equals r1.RequirementId
								join r2 in context.Requirements on a.DestArtifactId equals r2.RequirementId
								where
									a.SourceArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Requirement &&
									a.DestArtifactTypeId == (int)Artifact.ArtifactTypeEnum.Requirement &&
									r1.ProjectId == projectId && !r1.IsDeleted &&
									r2.ProjectId == projectId && !r2.IsDeleted
								orderby a.ArtifactLinkId
								select a;


					//Get the count
					count = query.Count();

					//Make pagination is in range
					if (startRow < 1 || startRow > count)
					{
						startRow = 1;
					}

					//Execute the query
					associations = query
						.Skip(startRow - 1)
						.Take(numberOfRows)
						.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return associations;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of all the requirements for a specific number of indent levels, optionally filtered by release
		/// </summary>
		/// <param name="numberOfRows">The number of rows to return</param>
		/// <param name="startRow">The starting row</param>
		/// <param name="projectId">The of the project</param>
		/// <param name="numberOfLevels">The number of levels (null = all)</param>
		/// <param name="parentRequirementId">The requirement to focus on along with all its children, as per pagination (null = no parent requirement filter)</param>
		/// <param name="releaseId">the release or null for all (not currently used)</param>
		/// <returns>The list of requirements</returns>
		/// <remarks>
		/// Right now this function does not support filtering by release. We'll add it if the customers ask for it.
		/// </remarks>
		public List<RequirementView> Requirement_RetrieveForMindMap(int projectId, int? numberOfLevels, out int count, int startRow = 1, int numberOfRows = 15, int? parentRequirementId = null, int? releaseId = null)
		{
			const string METHOD_NAME = "Requirement_RetrieveForMindMap";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from r in context.RequirementsView
								where !r.IsDeleted && r.ProjectId == projectId
								select r;

					//Add the parent requirement filter
					if (parentRequirementId.HasValue)
					{
						//Get the parent requirement
						Requirement parentRequirement = this.RetrieveById3(projectId, parentRequirementId.Value);
						//Check the parent exists and is a summary
						if (parentRequirement != null && parentRequirement.IsSummary)
						{
							string parentIndentLevel = parentRequirement.IndentLevel;
							query = query.Where(r =>
								r.RequirementId == parentRequirement.RequirementId ||
								(r.IndentLevel.Length >= (parentIndentLevel.Length + 3) && EntityFunctions.Left(r.IndentLevel, parentIndentLevel.Length) == parentIndentLevel)
							);
						}
					}

					//Handle showing for a certain number of levels only
					if (numberOfLevels.HasValue)
					{
						query = query.Where(r => r.IndentLevel.Length <= (numberOfLevels.Value * 3));
					}

					//Add the sort
					query = query.OrderBy(r => r.IndentLevel).ThenBy(r => r.RequirementId);

					//Get the count
					count = query.Count();

					//Make pagination is in range
					if (startRow < 1 || startRow > count)
					{
						startRow = 1;
					}

					//Execute the query
					requirements = query
						.Skip(startRow - 1)
						.Take(numberOfRows)
						.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Returns the color of the test coverage for requirements graphs
		/// </summary>
		/// <param name="coverageId"></param>
		/// <returns></returns>
		public static string GetCoverageStatusColor(int? coverageId)
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

		/// <summary>
		/// Returns a sorted list of values to populate the lookup for the
		/// requirements coverage filter
		/// </summary>
		/// <returns>Sorted List containing filter values</returns>
		public Dictionary<string, string> RetrieveCoverageFiltersLookup()
		{
			const string METHOD_NAME = "RetrieveCoverageFiltersLookup";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//If we don't have the filters list populated, then create, otherwise just return
				//We cannot use a static list because these are already localized
				if (coverageFiltersList == null)
				{
					coverageFiltersList = new Dictionary<string, string>();
					coverageFiltersList.Add("1", GlobalResources.General.Requirement_NotCovered);
					coverageFiltersList.Add("2", "=  0% " + GlobalResources.General.TestCase_Run);
					coverageFiltersList.Add("3", "<= 50% " + GlobalResources.General.TestCase_Run);
					coverageFiltersList.Add("4", "<  100% " + GlobalResources.General.TestCase_Run);
					coverageFiltersList.Add("5", ">  0% " + GlobalResources.General.TestCase_Failed);
					coverageFiltersList.Add("6", ">= 50% " + GlobalResources.General.TestCase_Failed);
					coverageFiltersList.Add("7", "=  100% " + GlobalResources.General.TestCase_Failed);
					coverageFiltersList.Add("8", ">  0% " + GlobalResources.General.TestCase_Caution);
					coverageFiltersList.Add("9", ">= 50% " + GlobalResources.General.TestCase_Caution);
					coverageFiltersList.Add("10", "=  100% " + GlobalResources.General.TestCase_Caution);
					coverageFiltersList.Add("11", ">  0% " + GlobalResources.General.TestCase_Blocked);
					coverageFiltersList.Add("12", ">= 50% " + GlobalResources.General.TestCase_Blocked);
					coverageFiltersList.Add("13", "=  100% " + GlobalResources.General.TestCase_Blocked);
					coverageFiltersList.Add("14", "=  0% " + GlobalResources.General.TestCase_Passed);
					coverageFiltersList.Add("15", ">  0% " + GlobalResources.General.TestCase_Passed);
					coverageFiltersList.Add("16", "<= 50% " + GlobalResources.General.TestCase_Passed);
					coverageFiltersList.Add("17", "<  100% " + GlobalResources.General.TestCase_Passed);
					coverageFiltersList.Add("18", "=  100% " + GlobalResources.General.TestCase_Passed);
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

			return coverageFiltersList;
		}

		/// <summary>
		/// Validates the estimate points to make sure it can be stored in the database field.
		/// </summary>
		/// <param name="estimatePoints">The estimate to validate</param>
		/// <returns>The validated data</returns>
		private decimal? ValidateEstimatePoints(decimal? estimatePoints)
		{
			//Make sure the data can fit into a DECIMAL(4,1) field (0.0 - 999.9)
			if (estimatePoints.HasValue)
			{
				if (estimatePoints.Value < 0M)
				{
					estimatePoints = 0M;
				}
				if (estimatePoints.Value >= 999.9M)
				{
					estimatePoints = 999.9M;
				}
			}

			return estimatePoints;
		}

		/// <summary>Adds a list of requirements to a specific test case</summary>
		/// <param name="projectId">The current project</param>
		/// <param name="testCaseId">The id of the test case we're associating them with</param>
		/// <param name="requirementIds">The list of requirements being added</param>
		/// <remarks>Duplicates are ignored and summary items have their child items added</remarks>
		/// <returns>The ids of the items that were actually mapped (e.g. the child items of a summary requirement)</returns>
		public List<int> AddToTestCase(int projectId, int testCaseId, List<int> requirementIds, int userId)
		{
			const string METHOD_NAME = "AddToTestCase";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the template for this project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Get the list of projects we are allowed to link to (not including the same project)
				List<ProjectArtifactSharing> sharingProjects = new ProjectManager().ProjectAssociation_RetrieveForDestProjectAndArtifact(projectId, Artifact.ArtifactTypeEnum.Requirement);

				//First get the list of already mapped requirements (to avoid duplicates)
				List<RequirementView> mappedRequirements = RetrieveCoveredByTestCaseId(UserManager.UserInternal, null, testCaseId);

				//Now iterate through each of the requirements and see if it's a summary
				Dictionary<int, int?> validatedRequirementIds = new Dictionary<int, int?>();
				Dictionary<int, int> requirementProjects = new Dictionary<int, int>();
				foreach (int requirementId in requirementIds)
				{
					try
					{
						RequirementView requirement = RetrieveById2(null, requirementId);
						if (requirement.IsSummary && (requirement.ProjectId == projectId || sharingProjects.Any(p => p.SourceProjectId == requirement.ProjectId)))
						{
							//Get the list of child items and add those as well
							List<RequirementView> childRequirements = RetrieveChildren(UserManager.UserInternal, projectId, requirement.IndentLevel, true);
							foreach (RequirementView childRequirement in childRequirements)
							{
								if (!childRequirement.IsSummary && !mappedRequirements.Any(r => r.RequirementId == childRequirement.RequirementId))
								{
									if (!validatedRequirementIds.ContainsKey(childRequirement.RequirementId))
									{
										validatedRequirementIds.Add(childRequirement.RequirementId, childRequirement.ReleaseId);
										requirementProjects.Add(childRequirement.RequirementId, childRequirement.ProjectId);
									}
								}
							}
						}

						//Check to see if it's already mapped, if not, add to validated list
						if (!mappedRequirements.Any(r => r.RequirementId == requirementId))
						{
							if (!validatedRequirementIds.ContainsKey(requirementId))
							{
								validatedRequirementIds.Add(requirement.RequirementId, requirement.ReleaseId);
								requirementProjects.Add(requirementId, requirement.ProjectId);
							}
						}
					}
					catch (ArtifactNotExistsException)
					{
						//We just ignore requirements that have been deleted
					}
				}

				//Now add the validated items to the mapping table
				TestCaseManager tMgr = new TestCaseManager();
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					foreach (var reqInfo in validatedRequirementIds)
					{
						//Do insert for each one
						context.Requirement_SaveCoverageInsert(reqInfo.Key, testCaseId);

						//Write Testcase -> Requirement linking history.
						new HistoryManager().AddRequirementTestCoverage(projectId, reqInfo.Key, null, userId, new List<int> { testCaseId });

						//Add the Test Case to the Release.
						if (reqInfo.Value.HasValue)
							tMgr.AddToRelease(projectId, reqInfo.Value.Value, new List<int> { testCaseId }, userId);

					}
				}

				//Finally perform a bulk refresh of the requirement list coverage summary data
				foreach (var reqInfo in validatedRequirementIds)
				{
					int requirementProjectId = requirementProjects[reqInfo.Key];
					RefreshTaskProgressAndTestCoverage(requirementProjectId, reqInfo.Key);
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return validatedRequirementIds.Select(kvp => kvp.Key).ToList();
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Removes a list of requirements from a specific test case</summary>
		/// <param name="projectId">The current project</param>
		/// <param name="testCaseId">The id of the test case we're de-associating them from</param>
		/// <param name="requirementIds">The list of requirements being removed</param>
		/// <remarks>Items that are not already mapped are ignored. Also this function does not expect to get summary items</remarks>
		public void RemoveFromTestCase(int projectId, int testCaseId, List<int> requirementIds, int userId)
		{
			const string METHOD_NAME = "RemoveFromTestCase";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First get the list of already mapped test cases (to avoid concurrency errors)
				List<RequirementView> mappedRequirements = RetrieveCoveredByTestCaseId(UserManager.UserInternal, null, testCaseId);

				//Now iterate through each of the requirements and make sure it's mapped already
				List<int> validatedRequirementIds = new List<int>();
				Dictionary<int, int> requirementProjects = new Dictionary<int, int>();
				foreach (int requirementId in requirementIds)
				{
					//Check to see if it's already mapped, if so, add to validated list
					RequirementView mappedRequirement = mappedRequirements.FirstOrDefault(r => r.RequirementId == requirementId);
					if (mappedRequirement != null && !validatedRequirementIds.Contains(requirementId))
					{
						validatedRequirementIds.Add(requirementId);
						requirementProjects.Add(requirementId, mappedRequirement.ProjectId);
					}
				}
				//Now remove the validated items from the mapping table
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					foreach (int requirementId in validatedRequirementIds)
					{
						//Do delete for each one
						context.Requirement_SaveCoverageDelete(requirementId, testCaseId);

						//Save history for each one.
						new HistoryManager().RemoveRequirementTestCoverage(projectId, requirementId, null, userId, new List<int> { testCaseId });
					}
				}

				//Finally perform a bulk refresh of the requirement list coverage summary data
				foreach (int requirementId in validatedRequirementIds)
				{
					int requirementProjectId = requirementProjects[requirementId];
					RefreshTaskProgressAndTestCoverage(requirementProjectId, requirementId);
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

		/// <summary>Generates the tooltip to display on requirement task progress bars</summary>
		public static string GenerateTaskProgressTooltip(RequirementView requirement)
		{
			int taskCount = requirement.TaskCount;

			//Handle case of no tasks
			if (taskCount == 0)
			{
				return GlobalResources.General.Global_NoTasks;
			}

			string tooltipText =
				"# " + GlobalResources.General.Global_Tasks + "=" + taskCount +
				", " + GlobalResources.General.Task_OnSchedule + "=" + requirement.TaskPercentOnTime +
				"%, " + GlobalResources.General.Task_RunningLate + "=" + requirement.TaskPercentLateFinish +
				"%, " + GlobalResources.General.Task_StartingLate + "=" + requirement.TaskPercentLateStart +
				"%, " + GlobalResources.General.Task_NotStarted + "=" + requirement.TaskPercentNotStart + "%";
			return tooltipText;
		}

		/// <summary>Creates a new requirement from the passed in test case</summary>
		/// <param name="userId">The user performing the operation</param>
		/// <param name="projectId">The current project</param>
		/// <param name="projectTemplateId">the id of thee project template</param>
		/// <param name="testCaseId">The test case we're creating the requirement from</param>
		/// <param name="existingRequirementId">The existing requirement it should be created in front of (optional)</param>
		/// <returns>The id of the newly created requirement</returns>
		public int CreateFromTestCase(int userId, int projectId, int testCaseId, int? existingRequirementId)
		{
			const string METHOD_NAME = "CreateFromTestCase";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the template for this project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//See if we have any use case requirement types defined, if not use the default
				int? requirementTypeId = null;
				List<RequirementType> useCaseTypes = RequirementType_RetrieveUseCases(projectTemplateId);
				if (useCaseTypes.Count > 0)
				{
					requirementTypeId = useCaseTypes.First().RequirementTypeId;
				}

				//First create a new requirement with the test case id embedded
				Business.TestCaseManager testCaseManager = new TestCaseManager();
				TestCaseView testCaseView = testCaseManager.RetrieveById(projectId, testCaseId);
				string requirementName = GlobalResources.General.Requirement_NewRequirement + " (" + testCaseView.Name + ")";
				int requirementId = Insert(
					userId,
					projectId,
					null,
					null,
					existingRequirementId,
					Requirement.RequirementStatusEnum.Requested,
					requirementTypeId,
					userId,
					null,
					null,
					requirementName,
					testCaseView.Description,
					null,
					userId
					);

				//Add a comment describing the TC it came from
				DiscussionManager discussion = new DiscussionManager();
				string comment = GlobalResources.General.Requirement_NewRequirementComment + " TC" + testCaseView.TestCaseId;
				discussion.Insert(userId, requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, comment, projectId, false, true);

				//Now associate this new requirement with the test case
				List<int> requirements = new List<int>();
				requirements.Add(requirementId);
				AddToTestCase(projectId, testCaseId, requirements, userId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirementId;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
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

		/// <summary>Counts all the requirements in the project for a sorted non-hierarchical grid</summary>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <returns>The total number of requirements</returns>
		/// <remarks>Used to help with pagination</remarks>
		public int Requirement_CountForSorted(int projectId, Hashtable filters = null, double utcOffset = 0, bool includeDeleted = false)
		{
			const string METHOD_NAME = "Requirement_CountForSorted";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int requirementCount = 0;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from t in context.RequirementsView
								where (!t.IsDeleted || includeDeleted) && t.ProjectId == projectId
								select t;

					//Add the dynamic filters
					if (filters != null)
					{
						//Get the template for this project
						int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<RequirementView, bool>> filterClause = CreateFilterExpression<RequirementView>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, filters, utcOffset, null, HandleRequirementSpecificFilters);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<RequirementView>)query.Where(filterClause);
						}
					}

					//Get the count
					requirementCount = query.Count();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirementCount;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Counts all the requirements in the project for the current user viewing</summary>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="includeDeleted">Whether or not to include deleted items or not. Default:FALSE</param>
		/// <returns>The total number of requirements</returns>
		/// <remarks>Used to help with pagination</remarks>
		public int Count(int userId, int projectId, Hashtable filters, double utcOffset, bool includeDeleted = false)
		{
			const string METHOD_NAME = "Count()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int artifactCount = 0;

				//Get the template for this project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Now we need to create the filters part of the WHERE clause
				string filtersClause = CreateFiltersClause(projectId, projectTemplateId, filters, utcOffset);

				//Call the stored procedure to get the count
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					artifactCount = context.Requirement_Count(userId, projectId, filtersClause, includeDeleted).FirstOrDefault().Value;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return artifactCount;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Returns a simple key/value dictionary that can be used in test set dropdown lists
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The key/value dictionary</returns>
		public Dictionary<string, string> RetrieveForLookups(int projectId)
		{
			const string METHOD_NAME = "RetrieveForLookups";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				Dictionary<string, string> requirementsLookup = new Dictionary<string, string>();

				//Next we need to get all the requirements with name, folder
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								where r.ProjectId == projectId && !r.IsDeleted
								orderby r.IndentLevel, r.RequirementId
								select new { r.RequirementId, r.IndentLevel, r.Name, r.IsSummary, r.RequirementTypeId, r.RequirementTypeIsSteps };

					//Get the list and group by folder
					var requirements = query.ToList();

					foreach (var requirement in requirements)
					{
						string alternateYn = (requirement.RequirementTypeIsSteps) ? "Y" : "N";
						string summaryYn = (requirement.IsSummary) ? "Y" : "N";
						requirementsLookup.Add(
							requirement.RequirementId + "_" + ((requirement.IndentLevel.Length / 3) + 1) + "_" + summaryYn + "_" + alternateYn + "_Y",
							requirement.Name);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirementsLookup;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Returns a list of test set/test set folder entries that can be used in test set dropdown lists
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The list of entries</returns>
		public List<RequirementLookupEntry> RetrieveForLookups2(int projectId)
		{
			const string METHOD_NAME = "RetrieveForLookups2";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementLookupEntry> requirementsLookup = new List<RequirementLookupEntry>();

				//Next we need to get all the test sets with name, folder
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								where r.ProjectId == projectId && !r.IsDeleted
								orderby r.IndentLevel, r.RequirementId
								select new { r.RequirementId, r.IndentLevel, r.Name, r.IsSummary, r.RequirementTypeId, r.RequirementTypeIsSteps };

					//Get the list and group by folder
					var requirements = query.ToList();

					foreach (var requirement in requirements)
					{
						requirementsLookup.Add(new RequirementLookupEntry()
						{
							RequirementId = requirement.RequirementId,
							Name = requirement.Name,
							IndentLevel = requirement.IndentLevel,
							IsSummary = requirement.IsSummary,
							IsAlternate = (requirement.RequirementTypeIsSteps)
						});
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirementsLookup;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list of all requirements in the project along with associated meta-data, sortable by user</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="filters">The list of filters to apply to the results (null if none)</param>
		/// <param name="numberOfRows">The number of rows to return</param>
		/// <param name="startRow">The starting row (1-based)</param>
		/// <param name="sortAscending">Should we sort ascending</param>
		/// <param name="sortProperty">What field should we sort on</param>
		/// <param name="utcOffset">What is our offset from UTC</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <returns>Requirement list</returns>
		/// <remarks>
		/// Does not use the user viewing meta-data, used for the sortable list view (not hierarchical views)
		/// </remarks>
		public List<RequirementView> Requirement_RetrieveSorted(int projectId, int startRow = 1, int numberOfRows = 15, Hashtable filters = null, double utcOffset = 0, string sortProperty = "Name", bool sortAscending = true, bool includeDeleted = false)
		{
			const string METHOD_NAME = "Requirement_RetrieveSorted";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from t in context.RequirementsView
								where (!t.IsDeleted || includeDeleted) && t.ProjectId == projectId
								select t;

					//Add the dynamic sort
					if (String.IsNullOrEmpty(sortProperty))
					{
						//Default to sorting by last updated date descending
						query = query.OrderByDescending(t => t.LastUpdateDate).ThenBy(t => t.RequirementId);
					}
					else
					{
						//We always sort by the physical ID to guarantee stable sorting
						string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
						query = query.OrderUsingSortExpression(sortExpression, "RequirementId");
					}

					//Add the dynamic filters
					if (filters != null)
					{
						//Get the template for this project
						int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						Expression<Func<RequirementView, bool>> filterClause = CreateFilterExpression<RequirementView>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.Requirement, filters, utcOffset, null, HandleRequirementSpecificFilters);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<RequirementView>)query.Where(filterClause);
						}
					}

					//Get the count
					int artifactCount = query.Count();

					//Make pagination is in range
					if (startRow < 1 || startRow > artifactCount)
					{
						startRow = 1;
					}

					//Execute the query
					requirements = query
						.Skip(startRow - 1)
						.Take(numberOfRows)
						.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list of all visible requirements in the system for a project along with associated meta-data, ordered by indentation level</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="filters">The list of filters to apply to the results (null if none)</param>
		/// <param name="numberOfRows">The number of rows to return</param>
		/// <param name="startRow">The starting row (1-based)</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <returns>Requirement list</returns>
		public List<RequirementView> Retrieve(int userId, int projectId, int startRow, int numberOfRows, Hashtable filters, double utcOffset, bool includeDeleted = false)
		{
			const string METHOD_NAME = "Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Set up the data set that is returned
			List<RequirementView> requirements;

			try
			{
				//Get the template for this project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Now we need to create the filters part of the WHERE clause
				string filtersClause = CreateFiltersClause(projectId, projectTemplateId, filters, utcOffset);

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					requirements = context.Requirement_Retrieve(userId, projectId, filtersClause, startRow, numberOfRows, false, includeDeleted).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return (requirements);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public RequirementView RetrieveRequirement(int artifactId, int projectId)
		{
			const string METHOD_NAME = "RetrieveRequirement";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Set up the data set that is returned
			RequirementView requirements;

			try
			{
				//Get the template for this project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								where
									r.ProjectId == projectId
									&& !r.IsDeleted && r.RequirementId == artifactId
								select r;

					requirements = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return (requirements);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Counts all the non-summary requirements in the project for the current user viewing</summary>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <returns>The total number of requirements</returns>
		/// <remarks>Used to help with pagination in combined req/task grids</remarks>
		public int CountNonSummary(int projectId, Hashtable filters, double utcOffset, bool includeDeleted = false)
		{
			const string METHOD_NAME = "CountNonSummary";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the template for this project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Now we need to create the filters part of the WHERE clause
				string filtersClause = CreateFiltersClause(projectId, projectTemplateId, filters, utcOffset);

				//Create select command for retrieving the lookup data
				int artifactCount;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					artifactCount = context.Requirement_CountNonSummary(projectId, filtersClause, includeDeleted).FirstOrDefault().Value;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return artifactCount;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Creates a new blank requirement record
		/// </summary>
		/// <param name="authorId">The id of the current user who will be initially set as the author</param>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The new blank incident entity with a single datarow</returns>
		/// <param name="requirementTypeId">The type of the new requirement, defaults to the default type if not specified</param>
		public RequirementView Requirement_New(int projectId, int authorId, int? requirementTypeId)
		{
			const string METHOD_NAME = "Requirement_New";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the project planning options
				ProjectManager projectManager = new ProjectManager();
				Project project = projectManager.RetrieveById(projectId);

				//Create the new entity
				RequirementView requirement = new RequirementView();
				RequirementStatus defaultStatus = RetrieveStatuses().Where(r => r.RequirementStatusId == (int)Requirement.RequirementStatusEnum.Requested).FirstOrDefault();

				//If no requirement type specified, get the default one for the current project template
				if (!requirementTypeId.HasValue)
				{
					int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
					RequirementType type = RequirementType_RetrieveDefault(projectTemplateId);
					requirementTypeId = type.RequirementTypeId;
				}

				//Populate the new requirement
				requirement.ProjectId = projectId;
				requirement.AuthorId = authorId;
				requirement.RequirementStatusId = defaultStatus.RequirementStatusId;
				requirement.RequirementStatusName = defaultStatus.Name;
				requirement.RequirementTypeId = requirementTypeId.Value;
				requirement.CreationDate = requirement.LastUpdateDate = requirement.ConcurrencyDate = DateTime.UtcNow;
				requirement.Name = "";
				requirement.Description = "";
				requirement.IsAttachments = false;
				requirement.EstimatePoints = project.ReqDefaultEstimate;

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirement;
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of summary requirements (backlogs) for the current project, used in the planning board product backlog
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>List of requirements</returns>
		/// <remarks>
		/// 1) Does not include summary items marked as rejected or obsolete, includes all others
		/// 2) Does not expand or collapse based on user navigation, always retrieves them all
		/// 3) Since it only gets summary ones, we don't currently need to paginate the data</remarks>
		public List<RequirementView> Requirement_RetrieveSummaryBacklog(int projectId)
		{
			const string METHOD_NAME = "Requirement_RetrieveSummaryBacklog(int)";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								where
									r.ProjectId == projectId
									&& !r.IsDeleted && r.IsSummary
									&& r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Rejected
									&& r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Obsolete
								orderby r.IndentLevel, r.RequirementId
								select r;

					requirements = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a single requirements - the first summary requirement in the hierarchical list for the current project
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>A requirement</returns>
		/// <remarks>
		/// 1) Does not include summary items marked as rejected or obsolete, includes all others
		/// 2) Does not expand or collapse based on user navigation</remarks>
		public Requirement Requirement_RetrieveFirstSummary(int projectId)
		{
			const string METHOD_NAME = "Requirement_RetrieveFirstSummary(int)";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				Requirement requirement;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.Requirements
								where
									r.ProjectId == projectId
									&& !r.IsDeleted && r.IsSummary
									&& r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Rejected
									&& r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Obsolete
								orderby r.IndentLevel, r.RequirementId
								select r;

					requirement = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirement;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates the rank(s) of the specific requirements (shuffling others as necessary)
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="requirementIds">The id of the requirements affected</param>
		/// <param name="existingRank">The rank we're inserting ahead of</param>
		public void Requirement_UpdateRanks(int projectId, List<int> requirementIds, int? existingRank)
		{
			const string METHOD_NAME = "Requirement_UpdateRanks";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				if (requirementIds != null && requirementIds.Count > 0)
				{
					//Convert the list into a CSV string
					string requirementIdList = requirementIds.ToDatabaseSerialization();

					//Call the stored procedure to update the ranks
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						context.Requirement_UpdateRanks(projectId, requirementIdList, existingRank);
					}

				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list of all non-summary requirements in the system for a project along with associated meta-data, ordered by indentation level </summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="filters">The list of filters to apply to the results (null if none)</param>
		/// <param name="numberOfRows">The number of rows to return</param>
		/// <param name="startRow">The starting row (1-based)</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <returns>Requirement dataset</returns>
		/// <remarks>Mainly used by the RequirementsTask service to </remarks>
		public List<RequirementView> RetrieveNonSummary(int userId, int projectId, int startRow, int numberOfRows, Hashtable filters, double utcOffset, bool includeDeleted = false)
		{
			const string METHOD_NAME = "Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the template for this project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Now we need to create the filters part of the WHERE clause
				string filtersClause = CreateFiltersClause(projectId, projectTemplateId, filters, utcOffset);

				//Call the self-normalizing, paginating stored procedure
				List<RequirementView> requirements;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					requirements = context.Requirement_RetrieveNonSummary(userId, projectId, filtersClause, startRow, numberOfRows, includeDeleted).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of non-summary requirements not assigned to a release/iteration for a specific component
		/// sorted by Rank then priority
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="componentId">The id of the component</param>
		/// <returns>The list of requirements</returns>
		/// <remarks>
		/// 1) Does not include requirements in the Rejected or Obsolete statuses
		/// </remarks>
		public List<RequirementView> Requirement_RetrieveBacklogByComponentId(int projectId, int? componentId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Requirement_RetrieveBacklogByComponentId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								where
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Rejected &&
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Obsolete &&
									r.ProjectId == projectId &&
									!r.ReleaseId.HasValue &&
									!r.IsSummary &&
									!r.IsDeleted
								select r;

					//Add the component filter
					if (componentId.HasValue)
					{
						query = query.Where(r => r.ComponentId.Value == componentId.Value);
					}
					else
					{
						query = query.Where(r => !r.ComponentId.HasValue);
					}

					//Order by rank then priority
					query = query.OrderByDescending(r => r.Rank).ThenBy(r => r.ImportanceName).ThenBy(r => r.RequirementId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					requirements = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of non-summary requirements assigned to a release/iteration for a specific component
		/// sorted by Rank then priority
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="componentId">The id of the component</param>
		/// <returns>The list of requirements</returns>
		/// <remarks>
		/// 1) Does not include requirements in the Rejected or Obsolete statuses
		/// </remarks>
		public List<RequirementView> Requirement_RetrieveAllReleasesByComponentId(int projectId, int? componentId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Requirement_RetrieveAllReleasesByComponentId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								where
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Rejected &&
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Obsolete &&
									r.ProjectId == projectId &&
									r.ReleaseId.HasValue &&
									!r.IsSummary &&
									!r.IsDeleted
								select r;

					//Add the component filter
					if (componentId.HasValue)
					{
						query = query.Where(r => r.ComponentId.Value == componentId.Value);
					}
					else
					{
						query = query.Where(r => !r.ComponentId.HasValue);
					}

					//Order by rank then priority
					query = query.OrderByDescending(r => r.Rank).ThenBy(r => r.ImportanceName).ThenBy(r => r.RequirementId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					requirements = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public List<RequirementView> Requirement_RetrieveAllBacklogByComponentId(int projectId, int? componentId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Requirement_RetrieveAllBacklogByComponentId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								where
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Rejected &&
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Obsolete &&
									r.ProjectId == projectId &&
									!r.ReleaseId.HasValue &&
									!r.IsSummary &&
									!r.IsDeleted
								select r;

					//Add the component filter
					if (componentId.HasValue)
					{
						query = query.Where(r => r.ComponentId.Value == componentId.Value);
					}
					else
					{
						query = query.Where(r => !r.ComponentId.HasValue);
					}

					//Order by rank then priority
					query = query.OrderByDescending(r => r.Rank).ThenBy(r => r.ImportanceName).ThenBy(r => r.RequirementId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					requirements = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}
		/// <summary>
		/// Retrieves the list of non-summary requirements assigned to a specific release/iteration for a specific component
		/// sorted by Rank then priority
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="componentId">The id of the component</param>
		/// <param name="releaseId">The id of the release/iteration</param>
		/// <returns>The list of requirements</returns>
		/// <remarks>
		/// 1) Does not include requirements in the Rejected or Obsolete statuses
		/// </remarks>
		public List<RequirementView> Requirement_RetrieveForReleaseByComponentId(int projectId, int? componentId, int releaseId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Requirement_RetrieveForReleaseByComponentId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								where
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Rejected &&
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Obsolete &&
									r.ProjectId == projectId &&
									!r.IsSummary &&
									!r.IsDeleted
								select r;

					//Add the component filter
					if (componentId.HasValue)
					{
						query = query.Where(r => r.ComponentId.Value == componentId.Value);
					}
					else
					{
						query = query.Where(r => !r.ComponentId.HasValue);
					}

					//Add the release/iteration filter (include child iterations of a release)
					if (releaseId != -2)
					{
						List<int> releaseAndIterations = new ReleaseManager().GetSelfAndIterations(projectId, releaseId);
						query = query.Where(r => releaseAndIterations.Contains(r.ReleaseId.Value));

					}
					else
					{
						query = query.Where(r => !r.ReleaseId.HasValue);
					}

					//Order by rank then priority
					query = query.OrderByDescending(r => r.Rank).ThenBy(r => r.ImportanceName).ThenBy(r => r.RequirementId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					requirements = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of non-summary requirements owned by a user and/or release/iteration
		/// sorted by Rank then priority
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="userId">The id of the user (null = unassigned)</param>
		/// <param name="releaseId">The id of the current release/iteration (null = no release/iteration)</param>
		/// <param name="considerChildIterations">Should we consider child iterations, only used when releaseId specified</param>
		/// <returns>The list of requirements</returns>
		/// <remarks>
		/// 1) Does not include requirements in the Rejected or Obsolete statuses
		/// </remarks>
		public List<RequirementView> Requirement_RetrieveBacklogByUserId(int projectId, int? releaseId, int? userId, bool considerChildIterations, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Requirement_RetrieveBacklogByUserId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								where
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Rejected &&
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Obsolete &&
									r.ProjectId == projectId &&
									r.ReleaseId.HasValue &&
									!r.IsSummary &&
									!r.IsDeleted
								select r;

					//Add the user filter
					if (userId.HasValue)
					{
						query = query.Where(r => r.OwnerId.Value == userId.Value);
					}
					else
					{
						query = query.Where(r => !r.OwnerId.HasValue);
					}

					//Add the release/iteration filter
					if (releaseId.HasValue && releaseId!=-2)
					{
						//Get the child iterations if required
						if (considerChildIterations)
						{
							List<int> releaseAndIterations = new ReleaseManager().GetSelfAndIterations(projectId, releaseId.Value);
							query = query.Where(r => releaseAndIterations.Contains(r.ReleaseId.Value));
						}
						else
						{
							query = query.Where(r => r.ReleaseId.Value == releaseId.Value);
						}
					}
					//else
					//{
					//	query = query.Where(r => !r.ReleaseId.HasValue);
					//}

					//Order by rank then priority
					query = query.OrderByDescending(r => r.Rank).ThenBy(r => r.ImportanceName).ThenBy(r => r.RequirementId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					requirements = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of non-summary requirements owned by a user for all releases in the project
		/// sorted by Rank then priority
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="userId">The id of the user (null = unassigned)</param>
		/// <returns>The list of requirements</returns>
		/// <remarks>
		/// 1) Does not include requirements in the Rejected or Obsolete statuses
		/// </remarks>
		public List<RequirementView> Requirement_RetrieveAllReleasesByUserId(int projectId, int? userId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Requirement_RetrieveAllReleasesByUserId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								where
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Rejected &&
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Obsolete &&
									r.ProjectId == projectId &&
									//!r.IsSummary &&
									//r.ReleaseId.HasValue &&
									!r.IsDeleted
								select r;

					//Add the user filter
					if (userId.HasValue)
					{
						query = query.Where(r => r.OwnerId.Value == userId.Value);
					}
					else
					{
						query = query.Where(r => !r.OwnerId.HasValue);
					}

					//Order by rank then priority
					query = query.OrderByDescending(r => r.Rank).ThenBy(r => r.ImportanceName).ThenBy(r => r.RequirementId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					requirements = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of non-summary requirements owned by a user and/or release/iteration
		/// sorted by Rank then priority
		/// </summary>
		/// <param name="projectId">The id of the project (null = all projects)</param>
		/// <param name="userId">The id of the user (null = unassigned)</param>
		/// <param name="projectGroupId">The id of the project group</param>
		/// <returns>The list of requirements</returns>
		/// <remarks>
		/// 1) Only includes requirements in the Planned, In-Progress, Developed, Testing statuses
		/// 2) Only includes active projects
		/// </remarks>
		public List<RequirementView> Requirement_RetrieveGroupBacklogByUserId(int projectGroupId, int? projectId, int? userId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Requirement_RetrieveGroupBacklogByUserId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								join p in context.Projects on r.ProjectId equals p.ProjectId
								where
									p.ProjectGroupId == projectGroupId &&
										(r.RequirementStatusId == (int)Requirement.RequirementStatusEnum.Planned ||
										r.RequirementStatusId == (int)Requirement.RequirementStatusEnum.InProgress ||
										r.RequirementStatusId == (int)Requirement.RequirementStatusEnum.Developed ||
										r.RequirementStatusId == (int)Requirement.RequirementStatusEnum.Tested) &&
									!r.IsSummary &&
									!r.IsDeleted &&
									p.IsActive
								select r;

					//Add the user filter
					if (userId.HasValue)
					{
						query = query.Where(r => r.OwnerId.Value == userId.Value);
					}
					else
					{
						query = query.Where(r => !r.OwnerId.HasValue);
					}

					//Add the project filter
					if (projectId.HasValue)
					{
						query = query.Where(r => r.ProjectId == projectId.Value);
					}

					//Order by rank then priority
					query = query.OrderByDescending(r => r.Rank).ThenBy(r => r.ImportanceName).ThenBy(r => r.RequirementId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					requirements = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of non-summary requirements associated with a specific status, for either a specific release/iteration or for the product backlog
		/// sorted by Rank then priority
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="statusId">The id of the requirement status (null = backlog)</param>
		/// <param name="releaseId">The id of the release (ignore for backlog)</param>
		/// <returns>The list of requirements</returns>
		/// <remarks>
		/// 1) Does include requirements in the Rejected status, but NOT in the Obsolete status
		/// 2) Includes the child iterations of the release
		/// </remarks>
		public List<RequirementView> Requirement_RetrieveBacklogByStatusId(int projectId, int? releaseId, int? statusId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Requirement_RetrieveBacklogByStatusId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								where
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Obsolete &&
									r.ProjectId == projectId &&
									r.ReleaseId.HasValue &&
									!r.IsSummary &&
									!r.IsDeleted
								select r;

					if (releaseId.HasValue)
					{
						//Add the status filter
						if (statusId.HasValue)
						{
							//Need to be in the status and part of the release/child iteration
							List<int> releaseAndIterations = new ReleaseManager().GetSelfAndIterations(projectId, releaseId.Value);
							query = query.Where(r => releaseAndIterations.Contains(r.ReleaseId.Value) && r.RequirementStatusId == statusId.Value);
						}
						else
						{
							//If no status, then only show items that are not part of the release
							//and are not-rejected (since it's the backlog)
							query = query.Where(r => !r.ReleaseId.HasValue && r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Rejected);
						}
					}
					else
					{
						//Only get items that have no release set (product backlog grouped by status)
						query = query.Where(r => !r.ReleaseId.HasValue);
						if (statusId.HasValue)
						{
							query = query.Where(r => r.RequirementStatusId == statusId.Value);
						}
						else
						{
							//return no items in this case
							return new List<RequirementView>();
						}
					}

					//Order by rank then priority
					query = query.OrderByDescending(r => r.Rank).ThenBy(r => r.ImportanceName).ThenBy(r => r.RequirementId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					requirements = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}
		
		public List<PlanningBoardReleaseResponse> Requirement_RetrieveRelease(int projectId, int startIndex = 0, int numRows = Int32.MaxValue,string filtertype = null)
		{
			const string METHOD_NAME = "Requirement_RetrieveBacklogByStatusId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<PlanningBoardReleaseResponse> requirements;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.Requirements
								join d in context.Projects
								on r.ProjectId equals d.ProjectId
								join u in context.Users
								on r.AuthorId equals u.UserId
								where
									r.ProjectId == projectId && r.ReleaseId > 0 &&
									!r.IsSummary &&
									!r.IsDeleted
								select new PlanningBoardReleaseResponse
								{
									ProjectId = d.ProjectId,
									ReleaseId = r.ReleaseId,
									RequirementId = r.RequirementId,
									AuthorId = r.AuthorId,
									OwnerId = 1,
									AuthorName = u.UserName,
									Name = r.Name,
									Description = r.Description,
									Rank = (int)r.EstimatePoints,
									Type = "Requirement",
								};


					//Order by rank then priority
					query = query.OrderByDescending(r => r.Rank).ThenBy(r => r.RequirementId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the item1
					requirements = query.ToList();

					var query1 = from r in context.Incidents
								join d in context.Projects
								on r.ProjectId equals d.ProjectId
								//join u in context.Users
								//on r.AuthorId equals u.UserId
								where
									r.ProjectId == projectId && r.DetectedReleaseId > 0 &&
									!r.IsDeleted
								select new PlanningBoardReleaseResponse
								{
									ProjectId = d.ProjectId,
									ReleaseId = r.DetectedReleaseId,
									RequirementId = r.IncidentId,
									AuthorId = 1,
									OwnerId = r.OwnerId,
									AuthorName =" ",
									Name = r.Name,
									Description = r.Description,
									Rank = r.EstimatedEffort/60,
									Type = "Incident",
								};

					List<PlanningBoardReleaseResponse> requirements1 = query1.ToList();

					requirements = requirements.Concat(requirements1).ToList();
					if (filtertype == "Requirements")
						requirements = requirements.Where(x => x.Type == "Requirement").ToList();
					else if (filtertype == "Incidents")
						requirements = requirements.Where(x => x.Type == "Incident").ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public List<PlanningBoardReleaseResponse> Requirement_RetrieveBacklogs(int projectId, int startIndex = 0, int numRows = Int32.MaxValue,string filtertype=null)
		{
			const string METHOD_NAME = "Requirement_RetrieveBacklogByStatusId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<PlanningBoardReleaseResponse> requirements;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.Requirements
								join d in context.Projects
								on r.ProjectId equals d.ProjectId
								join u in context.Users
								on r.AuthorId equals u.UserId
								where
									r.ProjectId == projectId && r.ReleaseId == null &&
									!r.IsSummary &&
									!r.IsDeleted
								select new PlanningBoardReleaseResponse
								{
									ProjectId = d.ProjectId,
									ReleaseId = r.ReleaseId,
									RequirementId = r.RequirementId,
									AuthorId = r.AuthorId,
									OwnerId = 1,
									AuthorName = u.UserName,
									Name = r.Name,
									Description = r.Description,
									Rank = (int)r.EstimatePoints,
									Type = "Requirement",
								};


					//Order by rank then priority
					query = query.OrderByDescending(r => r.Rank).ThenBy(r => r.RequirementId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the item1
					requirements = query.ToList();

					var query1 = from r in context.Incidents
								 join d in context.Projects
								 on r.ProjectId equals d.ProjectId
								 //join u in context.Users
								 //on r.AuthorId equals u.UserId
								 where
									 r.ProjectId == projectId && r.DetectedReleaseId == null &&
									 !r.IsDeleted
								 select new PlanningBoardReleaseResponse
								 {
									 ProjectId = d.ProjectId,
									 ReleaseId = r.DetectedReleaseId,
									 RequirementId = r.IncidentId,
									 AuthorId = 1,
									 OwnerId = 1,
									 AuthorName = "",
									 Name = r.Name,
									 Description = r.Description,
									 Rank = r.EstimatedEffort/60,
									 Type = "Incident",
								 };

					List<PlanningBoardReleaseResponse> requirements1 = query1.ToList();

					requirements = requirements.Concat(requirements1).ToList();
					if (filtertype == "Requirements")
						requirements = requirements.Where(x => x.Type == "Requirement").ToList();
					else if (filtertype == "Incidents")
						requirements = requirements.Where(x => x.Type == "Incident").ToList();


				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of non-summary requirements associated with a specific status, for all releases in the project
		/// sorted by Rank then priority
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="statusId">The id of the requirement status (null = backlog)</param>
		/// <returns>The list of requirements</returns>
		/// <remarks>
		/// 1) Does include requirements in the Rejected status, but NOT in the Obsolete status
		/// </remarks>
		public List<RequirementView> Requirement_RetrieveAllReleasesByStatusId(int projectId, int statusId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Requirement_RetrieveAllReleasesByStatusId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								where
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Obsolete &&
									r.ProjectId == projectId &&
									!r.IsSummary && r.ReleaseId.HasValue &&
									r.RequirementStatusId == statusId &&
									!r.IsDeleted
								select r;

					//Order by rank then priority
					query = query.OrderByDescending(r => r.Rank).ThenBy(r => r.ImportanceName).ThenBy(r => r.RequirementId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					requirements = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}
		public List<RequirementView> Requirement_RetrieveBacklogAllReleasesByStatusId(int projectId, int statusId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Requirement_RetrieveBacklogAllReleasesByStatusId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								where
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Obsolete &&
									r.ProjectId == projectId &&
									//r.ReleaseId.HasValue &&
									!r.IsSummary && !r.ReleaseId.HasValue &&
									r.RequirementStatusId == statusId &&
									!r.IsDeleted
								select r;

					//Order by rank then priority
					query = query.OrderByDescending(r => r.Rank).ThenBy(r => r.ImportanceName).ThenBy(r => r.RequirementId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					requirements = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}
		/// <summary>
		/// Retrieves the list of non-summary requirements associated with a specific status, for the product backlog
		/// sorted by Rank then priority
		/// </summary>
		/// <param name="projectGroupId">The id of the project group</param>
		/// <param name="projectId">The id of the project (null = all projects)</param>
		/// <param name="statusId">The id of the requirement status</param>
		/// <param name="unplannedItems">Do we want planned items or unplanned items</param>
		/// <returns>The list of requirements</returns>
		/// <remarks>
		/// 1) Does include requirements in the Rejected status, but NOT in the Obsolete status
		/// 2) Includes the child iterations of the release
		/// 3) Only includes active projects
		/// </remarks>
		public List<RequirementView> Requirement_RetrieveGroupBacklogByStatusId(int projectGroupId, int? statusId, int? projectId, bool unplannedItems, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Requirement_RetrieveGroupBacklogByStatusId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								join p in context.Projects on r.ProjectId equals p.ProjectId
								where
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Obsolete &&
									p.ProjectGroupId == projectGroupId &&
									!r.IsSummary &&
									!r.IsDeleted &&
									p.IsActive
								select r;

					//Only get items that have no release set (product backlog grouped by status) unless a project specified
					if (projectId.HasValue)
					{
						//Get the current items for the specified project in the group
						//We include developed and tested items, but not completed ones
						query = query.Where(r => r.ProjectId == projectId.Value &&
							(r.RequirementStatusId == (int)Requirement.RequirementStatusEnum.Planned ||
							r.RequirementStatusId == (int)Requirement.RequirementStatusEnum.InProgress ||
							r.RequirementStatusId == (int)Requirement.RequirementStatusEnum.Developed ||
							r.RequirementStatusId == (int)Requirement.RequirementStatusEnum.Tested));
						if (statusId.HasValue)
						{
							query = query.Where(r => r.RequirementStatusId == statusId.Value);
						}
						else
						{
							//return no items in this case
							return new List<RequirementView>();
						}
					}
					else
					{
						//Get the backlog for the entire group
						//See if we want planned or unplanned items
						if (unplannedItems)
						{
							query = query.Where(r => !r.ReleaseId.HasValue);
						}
						else
						{
							query = query.Where(r => r.ReleaseId.HasValue);
						}
						if (statusId.HasValue)
						{
							query = query.Where(r => r.RequirementStatusId == statusId.Value);
						}
						else
						{
							//return no items in this case
							return new List<RequirementView>();
						}
					}

					//Order by rank then priority
					query = query.OrderByDescending(r => r.Rank).ThenBy(r => r.ImportanceName).ThenBy(r => r.RequirementId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					requirements = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of non-summary requirements for a specific release
		/// sorted by Rank then priority
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="releaseId">The id of the release (or null for those in the product backlog)</param>
		/// <param name="considerChildIterations">Do we consider child iterations</param>
		/// <returns>The list of requirements</returns>
		/// <remarks>
		/// 1) Does not include requirements in the Rejected or Obsolete statuses
		/// </remarks>
		public List<RequirementView> Requirement_RetrieveBacklogByReleaseId(int projectId, int? releaseId, bool considerChildIterations, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Requirement_RetrieveBacklogByReleaseId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								where
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Rejected &&
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Obsolete &&
									r.ProjectId == projectId &&
									!r.IsSummary &&
									!r.IsDeleted
								select r;

					//Add the release filter
					if (releaseId.HasValue)
					{
						//Get the child iterations if required
						if (considerChildIterations)
						{
							List<int> releaseAndIterations = new ReleaseManager().GetSelfAndIterations(projectId, releaseId.Value);
							query = query.Where(r => releaseAndIterations.Contains(r.ReleaseId.Value));
						}
						else
						{
							query = query.Where(r => r.ReleaseId.Value == releaseId.Value);
						}
					}
					else
					{
						query = query.Where(r => !r.ReleaseId.HasValue);
					}

					//Order by rank then priority
					query = query.OrderByDescending(r => r.Rank).ThenBy(r => r.ImportanceName).ThenBy(r => r.RequirementId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					requirements = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of non-summary requirements for a specific project
		/// sorted by Rank then priority
		/// </summary>
		/// <param name="projectGroupId">The id of the project group</param>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The list of requirements</returns>
		/// <remarks>
		/// 1) Only includes requirements in active statuses for the project (Planned, In Progress, Developed, Tested)
		/// </remarks>
		public List<RequirementView> Requirement_RetrieveGroupBacklogByProjectId(int projectGroupId, int projectId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Requirement_RetrieveGroupBacklogByProjectId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								join p in context.Projects on r.ProjectId equals p.ProjectId
								where
									(r.RequirementStatusId == (int)Requirement.RequirementStatusEnum.Planned ||
									r.RequirementStatusId == (int)Requirement.RequirementStatusEnum.InProgress ||
									r.RequirementStatusId == (int)Requirement.RequirementStatusEnum.Developed ||
									r.RequirementStatusId == (int)Requirement.RequirementStatusEnum.Tested) &&
									p.ProjectGroupId == projectGroupId &&
									r.ProjectId == projectId &&
									!r.IsSummary &&
									!r.IsDeleted
								select r;

					//Order by rank then priority
					query = query.OrderByDescending(r => r.Rank).ThenBy(r => r.ImportanceName).ThenBy(r => r.RequirementId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					requirements = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Moves a set of non-summary requirements under a new parent
		/// </summary>
		/// <param name="requirementIds">The ids of the requirements being moved</param>
		/// <param name="packageRequirementId">The id of the new parent (or null if making root)</param>
		/// <param name="changerId">The id of the user making the change</param>
		/// <param name="projectId">The id of the project</param>
		public void Requirement_UpdateBacklogPackageRequirementId(int projectId, List<int> requirementIds, int? packageRequirementId, int changerId)
		{
			const string METHOD_NAME = "Requirement_UpdateBacklogPackageRequirementId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				try
				{
					//Begin transaction - needed to maintain integrity of hierarchy indent level
					using (TransactionScope transactionScope = new TransactionScope())
					{
						List<RequirementView> sourceRequirements;
						using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
						{
							//We need to initially retrieve the requirements, making sure not deleted and not a summary one
							var query = from r in context.RequirementsView
										where requirementIds.Contains(r.RequirementId) && !r.IsDeleted && !r.IsSummary
										orderby r.IndentLevel, r.RequirementId
										select r;

							sourceRequirements = query.ToList();
						}

						//Loop through the requirements
						foreach (RequirementView sourceRequirement in sourceRequirements)
						{
							string sourceIndentLevel = sourceRequirement.IndentLevel;
							string destIndentLevel = null;
							//Check to see if we are being passed an existing requirement to insert under
							if (packageRequirementId.HasValue)
							{
								RequirementView parentRequirement = RetrieveById2(projectId, packageRequirementId.Value, true);
								List<RequirementView> childRequirements = RetrieveChildren(Business.UserManager.UserInternal, projectId, parentRequirement.IndentLevel, false, true);

								//See if we have any existing child requirements
								if (childRequirements.Count > 0)
								{
									//Get the indent level of the last existing child
									string indentLevel = childRequirements[childRequirements.Count - 1].IndentLevel;

									//Now get the next indent level and use for that for the new item
									string newIndentLevel = HierarchicalList.IncrementIndentLevel(indentLevel);

									//Update the indent level of the requirement
									sourceRequirement.IndentLevel = newIndentLevel;
									UpdatePositionalData(changerId, new List<RequirementView>() { sourceRequirement });

									//Now close the gap in the hierarchy
									ReorderRequirementsAfterDelete(projectId, sourceIndentLevel);
								}
								else
								{
									//We have no children so get the indent level of the parent and increment that
									//i.e. insert after the parent, then we can do an indent
									string newIndentLevel = HierarchicalList.IncrementIndentLevel(parentRequirement.IndentLevel);

									//Now make room at this position
									ReorderRequirementsBeforeInsert(projectId, newIndentLevel);

									//Move the item here
									sourceRequirement.IndentLevel = newIndentLevel;
									UpdatePositionalData(changerId, new List<RequirementView>() { sourceRequirement });

									//Now close the gap in the hierarchy
									ReorderRequirementsAfterDelete(projectId, sourceIndentLevel);

									//Finally perform an indent of the new item to make a child
									Indent(changerId, projectId, sourceRequirement.RequirementId);
								}

								//Because the child list can contain deleted items, need to explicitly set the parent to a summary item
								//if not already
								if (!parentRequirement.IsSummary)
								{
									parentRequirement.IsSummary = true;
									UpdatePositionalData(changerId, new List<RequirementView>() { parentRequirement });
								}
							}
							else
							{
								//We simply move the requirement to the end of the list in this case as a root requirement
								//Get the next available indent level and parent id
								RequirementView destRequirement = Retrieve(User.UserInternal, projectId, "LEN(REQ.INDENT_LEVEL) = 3 ORDER BY REQ.INDENT_LEVEL DESC", 1, true).FirstOrDefault();
								string newIndentLevel;
								if (destRequirement == null)
								{
									newIndentLevel = "AAA";
								}
								else
								{
									//Now increment the indent level
									newIndentLevel = HierarchicalList.IncrementIndentLevel(destRequirement.IndentLevel);
								}

								//Update the indent level of the requirement
								sourceRequirement.IndentLevel = newIndentLevel;
								UpdatePositionalData(changerId, new List<RequirementView>() { sourceRequirement });

								//Now close the gap in the hierarchy
								ReorderRequirementsAfterDelete(projectId, sourceIndentLevel);
							}

							//Update the scope level, coverage and estimate/progress rollups for the source and destinations
							if (String.IsNullOrEmpty(destIndentLevel))
							{
								RefreshTaskProgressAndTestCoverage(projectId, destIndentLevel);
							}
							RefreshTaskProgressAndTestCoverage(projectId, sourceIndentLevel);
						}

						//Commit transaction - needed to maintain integrity of hierarchy indent level
						transactionScope.Complete();
					}
				}
				catch (System.Exception exception)
				{
					//Rollback transaction - needed to maintain integrity of hierarchy indent level
					//No need to call Rollback() explicitly with EF4, happens on TransactionScope.Dispose()

					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					Logger.Flush();
					throw;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of non-summary requirements not assigned to a release/iteration for a specific parent requirement (package)
		/// sorted by Rank then priority
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="packageRequirementId">The id of the parent package requirement</param>
		/// <returns>The list of requirements</returns>
		/// <remarks>
		/// 1) Does not include requirements in the Rejected or Obsolete statuses
		/// </remarks>
		public List<RequirementView> Requirement_RetrieveBacklogByPackageRequirementId(int projectId, int? packageRequirementId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Requirement_RetrieveBacklogByPackageRequirementId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								where
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Rejected &&
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Obsolete &&
									r.ProjectId == projectId &&
									!r.ReleaseId.HasValue &&
									!r.IsSummary &&
									!r.IsDeleted
								select r;

					//Add the parent requirement filter
					if (packageRequirementId.HasValue)
					{
						//First we need to get the parent requirement itself
						var query2 = from r in context.Requirements
									 where r.RequirementId == packageRequirementId.Value && !r.IsDeleted
									 select r;

						Requirement parentRequirement = query2.FirstOrDefault();
						if (parentRequirement == null)
						{
							return new List<RequirementView>();
						}
						string parentIndentLevel = parentRequirement.IndentLevel;
						query = query.Where(r => r.IndentLevel.Length == (parentIndentLevel.Length + 3) && EntityFunctions.Left(r.IndentLevel, parentIndentLevel.Length) == parentIndentLevel);
					}
					else
					{
						//Root requirements have no parent
						query = query.Where(r => r.IndentLevel.Length == 3);
					}

					//Order by rank then priority
					query = query.OrderByDescending(r => r.Rank).ThenBy(r => r.ImportanceName).ThenBy(r => r.RequirementId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					requirements = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of non-summary requirements assigned to any release/iteration for a specific parent requirement (package)
		/// sorted by Rank then priority
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="packageRequirementId">The id of the parent package requirement</param>
		/// <returns>The list of requirements</returns>
		/// <remarks>
		/// 1) Does not include requirements in the Rejected or Obsolete statuses
		/// </remarks>
		public List<RequirementView> Requirement_RetrieveAllReleasesByPackageRequirementId(int projectId, int? packageRequirementId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Requirement_RetrieveAllReleasesByPackageRequirementId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								where
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Rejected &&
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Obsolete &&
									r.ProjectId == projectId &&
									r.ReleaseId.HasValue &&
									!r.IsSummary &&
									!r.IsDeleted
								select r;

					//Add the parent requirement filter
					if (packageRequirementId.HasValue)
					{
						//First we need to get the parent requirement itself
						var query2 = from r in context.Requirements
									 where r.RequirementId == packageRequirementId.Value && !r.IsDeleted
									 select r;

						Requirement parentRequirement = query2.FirstOrDefault();
						if (parentRequirement == null)
						{
							return new List<RequirementView>();
						}
						string parentIndentLevel = parentRequirement.IndentLevel;
						query = query.Where(r => r.IndentLevel.Length == (parentIndentLevel.Length + 3) && EntityFunctions.Left(r.IndentLevel, parentIndentLevel.Length) == parentIndentLevel);
					}
					else
					{
						//Root requirements have no parent
						query = query.Where(r => r.IndentLevel.Length == 3);
					}

					//Order by rank then priority
					query = query.OrderByDescending(r => r.Rank).ThenBy(r => r.ImportanceName).ThenBy(r => r.RequirementId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					requirements = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of non-summary requirements assigned to a specific release/iteration for a specific parent requirement (package)
		/// sorted by Rank then priority
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="packageRequirementId">The id of the parent package requirement</param>
		/// <param name="releaseId">The id of the release/iteration</param>
		/// <returns>The list of requirements</returns>
		/// <remarks>
		/// 1) Does not include requirements in the Rejected or Obsolete statuses
		/// </remarks>
		public List<RequirementView> Requirement_RetrieveForReleaseByPackageRequirementId(int projectId, int? packageRequirementId, int releaseId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Requirement_RetrieveForReleaseByPackageRequirementId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								where
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Rejected &&
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Obsolete &&
									r.ProjectId == projectId &&
									!r.IsSummary &&
									!r.IsDeleted
								select r;

					//Add the parent requirement filter
					if (packageRequirementId.HasValue)
					{
						//First we need to get the parent requirement itself
						var query2 = from r in context.Requirements
									 where r.RequirementId == packageRequirementId.Value && !r.IsDeleted
									 select r;

						Requirement parentRequirement = query2.FirstOrDefault();
						if (parentRequirement == null)
						{
							return new List<RequirementView>();
						}
						string parentIndentLevel = parentRequirement.IndentLevel;
						query = query.Where(r => r.IndentLevel.Length == (parentIndentLevel.Length + 3) && EntityFunctions.Left(r.IndentLevel, parentIndentLevel.Length) == parentIndentLevel);
					}
					else
					{
						//Root requirements have no parent
						query = query.Where(r => r.IndentLevel.Length == 3);
					}

					//Add the release/iteration filter (include child iterations of a release)
					List<int> releaseAndIterations = new ReleaseManager().GetSelfAndIterations(projectId, releaseId);
					query = query.Where(r => releaseAndIterations.Contains(r.ReleaseId.Value));

					//Order by rank then priority
					query = query.OrderByDescending(r => r.Rank).ThenBy(r => r.ImportanceName).ThenBy(r => r.RequirementId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					requirements = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of non-summary requirements not assigned to a release/iteration for a specific importance
		/// sorted by Rank then id
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="importanceId">The id of the importance</param>
		/// <returns>The list of requirements</returns>
		/// <remarks>
		/// 1) Does not include requirements in the Rejected or Obsolete statuses
		/// </remarks>
		public List<RequirementView> Requirement_RetrieveBacklogByImportanceId(int projectId, int? importanceId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Requirement_RetrieveBacklogByImportanceId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								where
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Rejected &&
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Obsolete &&
									r.ProjectId == projectId &&
									!r.ReleaseId.HasValue &&
									!r.IsSummary &&
									!r.IsDeleted
								select r;

					//Add the importance filter
					if (importanceId.HasValue)
					{
						query = query.Where(r => r.ImportanceId.Value == importanceId.Value);
					}
					else
					{
						query = query.Where(r => !r.ImportanceId.HasValue);
					}

					//Order by rank then id
					query = query.OrderByDescending(r => r.Rank).ThenBy(r => r.RequirementId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					requirements = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of non-summary requirements assigned to a release/iteration for a specific importance
		/// sorted by Rank then id
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="importanceId">The id of the importance</param>
		/// <param name="releaseId">The id of the release/iteration</param>
		/// <returns>The list of requirements</returns>
		/// <remarks>
		/// 1) Does not include requirements in the Rejected or Obsolete statuses
		/// </remarks>
		public List<RequirementView> Requirement_RetrieveForReleaseByImportanceId(int projectId, int? importanceId, int releaseId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Requirement_RetrieveForReleaseByImportanceId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								where
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Rejected &&
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Obsolete &&
									r.ProjectId == projectId &&
									!r.IsSummary &&
									!r.IsDeleted
								select r;

					//Add the importance filter
					if (importanceId.HasValue)
					{
						query = query.Where(r => r.ImportanceId.Value == importanceId.Value);
					}
					else
					{
						query = query.Where(r => !r.ImportanceId.HasValue);
					}

					//Add the release/iteration filter (include child iterations of a release)
					List<int> releaseAndIterations = new ReleaseManager().GetSelfAndIterations(projectId, releaseId);
					query = query.Where(r => releaseAndIterations.Contains(r.ReleaseId.Value));

					//Order by rank then id
					query = query.OrderByDescending(r => r.Rank).ThenBy(r => r.RequirementId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					requirements = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of non-summary requirements assigned to a release/iteration for a specific importance
		/// sorted by Rank then id
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="importanceId">The id of the importance</param>
		/// <returns>The list of requirements</returns>
		/// <remarks>
		/// 1) Does not include requirements in the Rejected or Obsolete statuses
		/// </remarks>
		public List<RequirementView> Requirement_RetrieveAllReleasesByImportanceId(int projectId, int? importanceId, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Requirement_RetrieveAllReleasesByImportanceId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								where
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Rejected &&
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Obsolete &&
									r.ProjectId == projectId &&
									r.ReleaseId.HasValue &&
									!r.IsSummary &&
									!r.IsDeleted
								select r;

					//Add the importance filter
					if (importanceId.HasValue)
					{
						query = query.Where(r => r.ImportanceId.Value == importanceId.Value);
					}
					else
					{
						query = query.Where(r => !r.ImportanceId.HasValue);
					}

					//Order by rank then id
					query = query.OrderByDescending(r => r.Rank).ThenBy(r => r.RequirementId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					requirements = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the list of non-summary requirements not assigned to a release/iteration for a specific importance
		/// sorted by Rank then id
		/// </summary>
		/// <param name="unplannedItems">Do we want unplanned items or planned items</param>
		/// <param name="projectGroupId">The id of the project group</param>
		/// <param name="importanceId">The id of the importance</param>
		/// <returns>The list of requirements</returns>
		/// <remarks>
		/// 1) Does not include requirements in the Rejected or Obsolete statuses
		/// 2) Does not include inactive projects
		/// </remarks>
		public List<RequirementView> Requirement_RetrieveGroupBacklogByImportanceId(int projectGroupId, int? importanceId, bool unplannedItems, int startIndex = 0, int numRows = Int32.MaxValue)
		{
			const string METHOD_NAME = "Requirement_RetrieveGroupBacklogByImportanceId";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;

				//Create the LINQ query
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								join p in context.Projects on r.ProjectId equals p.ProjectId
								where
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Rejected &&
									r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Obsolete &&
									p.ProjectGroupId == projectGroupId &&
									!r.IsSummary &&
									!r.IsDeleted &&
									p.IsActive
								select r;

					//See if we want planned or unplanned items
					if (unplannedItems)
					{
						query = query.Where(r => !r.ReleaseId.HasValue);
					}
					else
					{
						query = query.Where(r => r.ReleaseId.HasValue);
					}

					//Add the importance filter
					if (importanceId.HasValue)
					{
						query = query.Where(r => r.ImportanceId.Value == importanceId.Value);
					}
					else
					{
						query = query.Where(r => !r.ImportanceId.HasValue);
					}

					//Order by rank then id
					query = query.OrderByDescending(r => r.Rank).ThenBy(r => r.RequirementId);

					//Handle pagination if specified
					if (startIndex > 0)
					{
						query = query.Skip(startIndex);
					}
					if (numRows < Int32.MaxValue)
					{
						query = query.Take(numRows);
					}

					//Get the items
					requirements = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves all the requirements for a particular release/iteration</summary>
		/// <param name="includeDeleted">Include deleted items</param>
		/// <param name="projectId">The current project</param>
		/// <param name="releaseId">The ID of the release/iteration we want to retrieve the requirements for</param>
		/// <returns>A requirement dataset</returns>
		/// <remarks>1) Passing null for release, returns all requirements that have no release set
		/// 2) Does not include child iterations or rejected requirements</remarks>
		public List<RequirementView> RetrieveByReleaseId(int projectId, int? releaseId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveByReleaseId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;

				//Create custom SQL WHERE clause for retrieving the requirements for the release/iteration
				if (releaseId.HasValue)
				{
					//Excludes rejected requirements
					requirements = Retrieve(UserManager.UserInternal, projectId, "REQ.RELEASE_ID = " + releaseId.Value +
						" AND REQ.REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Rejected +
						" ORDER BY IMPORTANCE_NAME, REQ.REQUIREMENT_ID", includeDeleted);
				}
				else
				{
					//We only want to include 'open' requirements
					requirements = Retrieve(UserManager.UserInternal, projectId, "REQ.RELEASE_ID IS NULL " +
						" AND REQ.IS_SUMMARY = 0 " +
						" AND REQ.REQUIREMENT_STATUS_ID IN (" + (int)Requirement.RequirementStatusEnum.Requested + "," + (int)Requirement.RequirementStatusEnum.UnderReview + "," + (int)Requirement.RequirementStatusEnum.Accepted + "," + (int)Requirement.RequirementStatusEnum.Planned + ") " +
						" ORDER BY IMPORTANCE_NAME, REQ.REQUIREMENT_ID", includeDeleted);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the list
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a single requirement in the system that has a certain ID</summary>
		/// <param name="requirementId">The ID of the requirement to be returned</param>
		/// <param name="projectId">The project we're interested in (optional)</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <returns>Requirement view</returns>
		public RequirementView RetrieveById(int userId, int? projectId, int requirementId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveById()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create custom SQL WHERE clause for retrieving the requirement item and execute
				List<RequirementView> requirements = Retrieve(userId, projectId, "REQ.REQUIREMENT_ID = " + requirementId.ToString(), includeDeleted);

				//If we don't have a record, throw a specific exception (since client will be expecting one record)
				if (requirements.Count == 0)
				{
					throw new ArtifactNotExistsException("Requirement " + requirementId + " doesn't exist in the system.");
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirements.FirstOrDefault();
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

		/// <summary>Retrieves a single requirement in the system that has a certain ID</summary>
		/// <param name="requirementId">The ID of the requirement to be returned</param>
		/// <param name="projectId">The project we're interested in (optional)</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <returns>Requirement view</returns>
		/// <remarks>
		/// Use this version when you don't need to worry about the IsVisible or IsExpanded properties,
		/// faster performance than the RetrieveById option
		/// </remarks>
		public RequirementView RetrieveById2(int? projectId, int requirementId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveById2()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				RequirementView requirement;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								where r.RequirementId == requirementId
								select r;

					if (!includeDeleted)
					{
						query = query.Where(r => !r.IsDeleted);
					}
					if (projectId.HasValue)
					{
						query = query.Where(r => r.ProjectId == projectId.Value);
					}

					requirement = query.FirstOrDefault();
					if (requirement == null)
					{
						throw new ArtifactNotExistsException("Requirement " + requirementId + " doesn't exist in the system.");
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirement;
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

		public RequirementView RetrieveById(int requirementId)
		{
			const string METHOD_NAME = "RetrieveById2()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				RequirementView requirement;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								where r.RequirementId == requirementId
								select r;

					requirement = query.FirstOrDefault();
					if (requirement == null)
					{
						throw new ArtifactNotExistsException("Requirement " + requirementId + " doesn't exist in the system.");
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirement;
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

		public TST_REQUIREMENT_SIGNATURE RetrieveBySignature(int requirementId, int userId)
		{
			const string METHOD_NAME = "RetrieveBySignature()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				TST_REQUIREMENT_SIGNATURE requirement;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementSignatures
								where r.REQUIREMENT_ID == requirementId && r.USER_ID == userId
								select r;

					requirement = query.FirstOrDefault();
					//if (requirement == null)
					//{
					//	throw new ArtifactNotExistsException("Requirement Signature" + requirementId + " doesn't exist in the system.");
					//}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirement;
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

		public List<TST_REQUIREMENT_SIGNATURE> RetrieveSignatures(int requirementId)
		{
			const string METHOD_NAME = "RetrieveSignatures()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<TST_REQUIREMENT_SIGNATURE> requirements;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementSignatures
								where r.REQUIREMENT_ID == requirementId
								select r;

					requirements = query.ToList();
					if (requirements == null)
					{
						throw new ArtifactNotExistsException("Requirement Signature" + requirementId + " doesn't exist in the system.");
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirements;
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

		public bool GetApprovedSignaturesForRequirement(int requirementId, int currentStatusId)
		{
			bool status = false;
			List<TST_REQUIREMENT_SIGNATURE> requirementApprovals = new List<TST_REQUIREMENT_SIGNATURE>();

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var approvals = context.RequirementSignatures.Include(x => x.TST_USER).Include(x => x.TST_USER.Profile).Where(x => x.REQUIREMENT_ID == requirementId).OrderByDescending(x => x.REQUIREMENT_APPROVAL_WORKFLOW_ID);
				foreach (var item in approvals)
				{
					if (item.STATUS_ID == currentStatusId)
					{
						requirementApprovals.Add(item);
					}
				}
				int originalCount = approvals.Count();
				int signCount = requirementApprovals.Count();
				if (originalCount == signCount)
				{
					status = true;
				}
			}

			return status;
		}

		public void RequestApprovalForRequirement(int projectId, int requirementId, List<int> userIds, int? loggedinUserId = null, string meaning = null)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				DateTime updatedDate = DateTime.Now;

				if (userIds.Count() > 0)
				{
					//cancel all current active workflows (if any)
					var req = context.RequirementsView.Where(x => x.RequirementId == requirementId && !x.IsDeleted);
					if (req != null)
					{
						var existingWorkflows = context.RequirementApprovalWorkflows.Where(x => x.REQUIREMENT_ID == requirementId);
						if (!existingWorkflows.Any())
						{
							var newWorkflow = new TST_REQUIREMENT_APPROVAL_WORKFLOW
							{
								IS_ACTIVE = true,
								UPDATE_DATE = updatedDate,
								REQUIREMENT_ID = requirementId
							};
							context.RequirementApprovalWorkflows.AddObject(newWorkflow);

							foreach (var item in userIds)
							{
								newWorkflow.TST_REQUIREMENT_SIGNATURE.Add(new TST_REQUIREMENT_SIGNATURE
								{
									STATUS_ID = (int)Requirement.RequirementStatusEnum.Requested,
									REQUIREMENT_ID = requirementId,
									REQUESTED_DATE = updatedDate,
									USER_ID = item,
									UPDATE_DATE = updatedDate,
									PROJECT_ID = projectId,
									MEANING = meaning
								});
							}
						}
						else
						{
							List<TST_REQUIREMENT_SIGNATURE> changedEntities = new List<TST_REQUIREMENT_SIGNATURE>();
							foreach (var workflow in existingWorkflows)
							{
								workflow.IS_ACTIVE = true;
								workflow.UPDATE_DATE = DateTime.Now;

								foreach (var item in userIds)
								{
									var data = RetrieveBySignature(requirementId, item);
									if (data == null)
									{
										data = new TST_REQUIREMENT_SIGNATURE
										{
											STATUS_ID = (int)Requirement.RequirementStatusEnum.Requested,
											REQUIREMENT_ID = requirementId,
											REQUESTED_DATE = updatedDate,
											USER_ID = item,
											UPDATE_DATE = updatedDate,
											REQUIREMENT_APPROVAL_WORKFLOW_ID = workflow.REQUIREMENT_APPROVAL_WORKFLOW_ID,
											PROJECT_ID = projectId,
											MEANING = meaning
										};
										changedEntities.Add(data.MarkAsAdded());
									}

								}
							}

							

							UpdateApproversForRequirementSignature(changedEntities);
						}
					}
				}

				context.SaveChanges();
				//log history
				//new HistoryManager().LogCreation(projectId, (int)loggedinUserId, Artifact.ArtifactTypeEnum.RequirementSignature, requirementId, DateTime.UtcNow);

			}
		}

		public void UpdateApproversForRequirementSignature(List<TST_REQUIREMENT_SIGNATURE> updatedEntities)
		{

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				foreach (var item in updatedEntities)
				{
					context.RequirementSignatures.ApplyChanges(item);
				}

				context.SaveChanges();
			}
		}

		public void UpdateRequirementSignatureWorkflowState(int projectId, int requirementId, string beforeStatus, int userId, Requirement.RequirementStatusEnum status, string meaning)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var workflow = context.RequirementApprovalWorkflows.Include(x => x.TST_REQUIREMENT_SIGNATURE).Where(x => x.REQUIREMENT_ID == requirementId && x.IS_ACTIVE).OrderByDescending(x => x.UPDATE_DATE).FirstOrDefault();
				if (workflow != null) 
				{
					var allApprovalsRequired = workflow.TST_REQUIREMENT_SIGNATURE.Where(x => x.STATUS_ID != (int)Requirement.RequirementStatusEnum.Rejected).ToList();

					var found = allApprovalsRequired.Where(x => x.USER_ID == userId).FirstOrDefault();
					if (found != null)
					{
						//foreach (var found in founds)
						//{
							//if (status == Requirement.RequirementStatusEnum.Approved)
							//{
								found.STATUS_ID = (int)Requirement.RequirementStatusEnum.Approved;
								found.UPDATE_DATE = DateTime.Now;
								found.MEANING = meaning + " - Signed";
								if (allApprovalsRequired.All(x => x.STATUS_ID == (int)Requirement.RequirementStatusEnum.Approved))
								{
									workflow.IS_ACTIVE = false;
									workflow.UPDATE_DATE = DateTime.Now;
								}
							//}
						//}

						//RequirementSignature update
						context.SaveChanges(userId, true, false, null);
						new HistoryManager().LogCreation(projectId, (int)userId, Artifact.ArtifactTypeEnum.RequirementSignature, requirementId, DateTime.UtcNow, null, null, beforeStatus);
					}
				}
			}
		}

		/// <summary>Retrieves a single requirement in the system that has a certain indent level</summary>
		/// <param name="indentLevel">The indent level of the requirement to be returned</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <returns>Requirement view or null</returns>
		/// <remarks>
		/// Use this version when you don't need to worry about the IsVisible or IsExpanded properties,
		/// faster performance than the RetrieveByIndentLevel() option
		/// </remarks>
		public RequirementView RetrieveByIndentLevel2(int projectId, string indentLevel, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveByIndentLevel2()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				RequirementView requirement;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementsView
								where r.IndentLevel == indentLevel && r.ProjectId == projectId
								select r;

					if (!includeDeleted)
					{
						query = query.Where(r => !r.IsDeleted);
					}

					requirement = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirement;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public List<TST_REQUIREMENT_SIGNATURE> GetAllSignaturesForRequirement(int requirementId)
		{
			List<TST_REQUIREMENT_SIGNATURE> requirementApprovals = new List<TST_REQUIREMENT_SIGNATURE>();

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				var approvals = context.RequirementSignatures.Include(x => x.TST_USER).Include(x => x.TST_USER.Profile).Where(x => x.REQUIREMENT_ID == requirementId).OrderByDescending(x => x.REQUIREMENT_APPROVAL_WORKFLOW_ID);
				foreach (var item in approvals)
				{
					requirementApprovals.Add(item);
				}
			}

			return requirementApprovals;
		}

		/// <summary>Retrieves a single requirement in the system that has a certain ID</summary>
		/// <param name="requirementId">The ID of the requirement to be returned</param>
		/// <param name="projectId">The project we're interested in (optional)</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <returns>Requirement</returns>
		/// <remarks>
		/// 1) Use this version when you don't need to worry about the IsVisible or IsExpanded properties,
		///    faster performance than the RetrieveById option.
		/// 2) Use this version when you need an editable Requirement not read-only RequirementView
		/// </remarks>
		public Requirement RetrieveById3(int? projectId, int requirementId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveById3()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{

				Requirement requirement;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.Requirements
								where r.RequirementId == requirementId
								select r;

					if (!includeDeleted)
					{
						query = query.Where(r => !r.IsDeleted);
					}
					if (projectId.HasValue)
					{
						query = query.Where(r => r.ProjectId == projectId.Value);
					}

					requirement = query.FirstOrDefault();
					if (requirement == null)
					{
						throw new ArtifactNotExistsException("Requirement " + requirementId + " doesn't exist in the system.");
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirement;
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

		public List<Requirement> RetrieveByProjectId(int? projectId)
		{
			const string METHOD_NAME = "RetrieveById3()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{

				List<Requirement> requirements;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.Requirements
								select r;

					if (projectId.HasValue)
					{
						query = query.Where(r => r.ProjectId == projectId.Value && !r.IsDeleted);
					}

					requirements = query.ToList();
					if (requirements == null)
					{
						throw new ArtifactNotExistsException("Requirement doesn't exist in the system.");
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirements;
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

		/// <summary>Returns a sorted list of values to populate the lookup for the requirements completion filter</summary>
		/// <returns>Dictionary containing filter values</returns>
		public Dictionary<string, string> RetrieveCompletionFiltersLookup()
		{
			const string METHOD_NAME = "RetrieveCompletionFiltersLookup";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//If we don't have the filters list populated, then create, otherwise just return
				if (this.completionFiltersList == null)
				{
					this.completionFiltersList = new Dictionary<string, string>();
					completionFiltersList.Add("1", GlobalResources.General.Global_NoRequirements);
					completionFiltersList.Add("2", "=  0% " + GlobalResources.General.Global_Complete);
					completionFiltersList.Add("3", "<= 25% " + GlobalResources.General.Global_Complete);
					completionFiltersList.Add("4", "<= 50% " + GlobalResources.General.Global_Complete);
					completionFiltersList.Add("5", "<= 75% " + GlobalResources.General.Global_Complete);
					completionFiltersList.Add("6", "<  100% " + GlobalResources.General.Global_Complete);
					completionFiltersList.Add("7", "=  100% " + GlobalResources.General.Global_Complete);
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

			return this.completionFiltersList;
		}

		/// <summary>Retrieves all items in the specified project that ARE marked for deletion.</summary>
		/// <param name="projectId">The project ID to get items for.</param>
		/// <returns>The list of requirements marked as deleted</returns>
		public List<RequirementView> RetrieveDeleted(int projectId)
		{
			const string METHOD_NAME = "RetrieveDeleted()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			List<RequirementView> requirements = new List<RequirementView>();
			try
			{
				requirements = Retrieve(-1, projectId, "REQ.IS_DELETED = 1", true); //Must include deleted items, to avoid collision.
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				//Do not rethrow.
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return requirements;
		}

		/// <summary>Retrieves the non-rejected and non-completed requirements by their owner regardless of project, sorted by priority then status</summary>
		/// <param name="ownerId">The owner of the requirements we want returned (pass null to retrieve all unassigned)</param>
		/// <param name="projectId">The id of the project (null for all)</param>
		/// <param name="releaseId">The id of the release (null for all)</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <param name="includeAccepted">Whether or not to include accepted items (not planned). Default: TRUE</param>
		/// <param name="includeCompleted">Include completed requirements</param>
		/// <param name="numberRows">The number of rows to return</param>
		/// <returns>Requirements dataset</returns>
		/// <remarks>1) This returns all requirements visible or not. It also only returns data for active projects
		/// 2) If you filter by release, it does NOT include items for the child iterations</remarks>
		public List<RequirementView> RetrieveByOwnerId(int? ownerId, int? projectId, int? releaseId, bool includeCompleted, int numberRows = 500, bool includeAccepted = true, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveByOwnerId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;

				string projectFilter = "";
				if (projectId.HasValue)
				{
					projectFilter = "AND REQ.PROJECT_ID = " + projectId.Value + " ";

					if (releaseId.HasValue)
					{
						projectFilter += " AND REQ.RELEASE_ID = " + releaseId.Value + " ";
					}
				}

				//Create custom SQL WHERE clause for retrieving the requirement item and execute
				if (ownerId.HasValue)
				{
					if (includeCompleted)
					{
						requirements = Retrieve(UserManager.UserInternal, null,
							"REQ.OWNER_ID = " + ownerId.Value +
							" AND REQ.PROJECT_IS_ACTIVE = 1 " +
							projectFilter +
							" ORDER BY REQ.IMPORTANCE_SCORE, REQ.REQUIREMENT_STATUS_ID, REQ.REQUIREMENT_ID", includeDeleted, false, numberRows);
					}
					else
					{
						if (includeAccepted)
						{
							requirements = Retrieve(UserManager.UserInternal, null,
								"REQ.OWNER_ID = " + ownerId.Value +
								" AND REQ.REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Rejected +
								" AND REQ.REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Completed +
								" AND REQ.REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Developed +
								" AND REQ.REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Tested +
								" AND REQ.REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Obsolete +
								" AND REQ.REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Released +
								" AND REQ.PROJECT_IS_ACTIVE = 1 " +
								projectFilter +
								" ORDER BY REQ.IMPORTANCE_SCORE, REQ.REQUIREMENT_STATUS_ID, REQ.REQUIREMENT_ID", includeDeleted, false, numberRows);
						}
						else
						{
							requirements = Retrieve(UserManager.UserInternal, null,
								"REQ.OWNER_ID = " + ownerId.Value +
								" AND REQ.REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Rejected +
								" AND REQ.REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Completed +
								" AND REQ.REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Developed +
								" AND REQ.REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Accepted +
								" AND REQ.REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Tested +
								" AND REQ.REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Obsolete +
								" AND REQ.REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Released +
								" AND REQ.PROJECT_IS_ACTIVE = 1 " +
								projectFilter +
								" ORDER BY REQ.IMPORTANCE_SCORE, REQ.REQUIREMENT_STATUS_ID, REQ.REQUIREMENT_ID", includeDeleted, false, numberRows);
						}
					}
				}
				else
				{
					requirements = Retrieve(UserManager.UserInternal, null,
						"REQ.OWNER_ID IS NULL " +
						" AND REQ.REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Rejected +
						" AND REQ.REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Completed +
						" AND REQ.REQUIREMENT_STATUS_ID <> " + (int)Requirement.RequirementStatusEnum.Obsolete +
						" AND REQ.PROJECT_IS_ACTIVE = 1 " +
						projectFilter +
						" ORDER BY REQ.IMPORTANCE_SCORE, REQ.REQUIREMENT_STATUS_ID, REQ.REQUIREMENT_ID", includeDeleted, false, numberRows);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the list of covered requirements already mapped against a test case</summary>
		/// <param name="testCaseId">The ID of the test case we want the list for</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The project we're interested in (optional)</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <returns>Requirement dataset</returns>
		public List<RequirementView> RetrieveCoveredByTestCaseId(int userId, int? projectId, int testCaseId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveCoveredByTestCaseId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create custom SQL WHERE clause for retrieving the requirements and execute
				List<RequirementView> requirements = Retrieve(userId, projectId, "REQ.REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM TST_REQUIREMENT_TEST_CASE WHERE TEST_CASE_ID = " + testCaseId.ToString() + ") ORDER BY REQ.INDENT_LEVEL ASC", includeDeleted);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return (requirements);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a single requirement in the system that has a certain indent level</summary>
		/// <param name="indentLevel">The indent-level of the requirement to be returned</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <returns>Requirement dataset</returns>
		public RequirementView RetrieveByIndentLevel(int userId, int projectId, string indentLevel, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveByIndentLevel()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create custom SQL WHERE clause for retrieving the requirement item and execute
				RequirementView requirement = Retrieve(userId, projectId, "REQ.INDENT_LEVEL = '" + indentLevel + "'", includeDeleted).FirstOrDefault();

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirement;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Returns the next available indent level (for new inserts)</summary>
		/// <param name="ignoreLastInserted">Do we want the next item at the root level, or the next one after the last one inserted</param>
		/// <param name="projectId">The id of the project</param>
		/// <param name="userId">The id of the user that is viewing the hierarchy</param>
		/// <returns>The indent level of the next available item Null if there is none.</returns>
		protected string GetNextAvailableIndentLevel(int userId, int projectId, bool ignoreLastInserted = false)
		{
			const string METHOD_NAME = "GetNextAvailableIndentLevel()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				string indentLevel = null;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					indentLevel = context.Requirement_GetNextAvailableIndentLevel(projectId, userId, ignoreLastInserted).FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return indentLevel;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a set of requirement from the system via a custom SQL filter and/or sort</summary>
		/// <param name="customFilterSort">A custom SQL WHERE and/or ORDER BY clause</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <param name="onlyShowVisible">Do we only want to include requirements that are visible for this user</param>
		/// <param name="numberRows">Should we limit the number of rows</param>
		/// <returns>Requirement list</returns>
		protected List<RequirementView> Retrieve(int userId, int? projectId, string customFilterSort, bool includeDeleted = false, bool onlyShowVisible = false, int? numberRows = null)
		{
			const string METHOD_NAME = "Retrieve()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Call the stored proc to get the list of requirements
					requirements = context.Requirement_RetrieveCustom(userId, projectId, customFilterSort, numberRows, includeDeleted, onlyShowVisible).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the peer requirement, immediate children and the parent summary requirement to the passed in requirement</summary>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="includeDeleted">Whether or not to include Deleted items. Default:FALSE</param>
		/// <param name="indentLevel">The indent-level of the requirement whose peers and parent we want to retrieve</param>
		/// <returns>Requirement list</returns>
		public List<RequirementView> RetrievePeersChildrenAndParent(int userId, int projectId, string indentLevel, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrievePeersChildrenAndParent()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Restore the parent release for this requirement and all peer requirements
				int length = indentLevel.Length;

				string parentIndentLevel = indentLevel.Substring(0, length - 3);
				List<RequirementView> requirements = Retrieve(userId, projectId, "SUBSTRING(REQ.INDENT_LEVEL, 1, " + (length - 3) + ") = '" + parentIndentLevel + "' AND ((LEN(REQ.INDENT_LEVEL) = " + (length - 3) + ") OR (LEN(REQ.INDENT_LEVEL) = " + length + ")) OR (LEN(REQ.INDENT_LEVEL) = " + (length + 3) + " AND SUBSTRING(REQ.INDENT_LEVEL, 1, " + length + ") = '" + indentLevel + "') AND REQ.PROJECT_ID = " + projectId.ToString() + " ORDER BY REQ.INDENT_LEVEL", includeDeleted);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the peer requirements and the parent summary requirement to the passed in requirement</summary>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="indentLevel">The indent-level of the requirement whose peers and parent we want to retrieve</param>
		/// <param name="includeSubFolders">Do we want to include subfolders or not</param>
		///<param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <returns>Requirement list</returns>
		public List<RequirementView> RetrievePeersAndParent(int userId, int projectId, string indentLevel, bool includeSubFolders, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrievePeersAndParent()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;

				//Restore the parent requirement for this requirement and all peer requirements
				int length = indentLevel.Length;

				string parentIndentLevel = indentLevel.Substring(0, length - 3);
				if (includeSubFolders)
				{
					requirements = Retrieve(userId, projectId, "SUBSTRING(REQ.INDENT_LEVEL, 1, " + (length - 3) + ") = '" + parentIndentLevel + "' AND ((LEN(REQ.INDENT_LEVEL) = " + (length - 3) + " AND REQ.IS_SUMMARY = 1) OR (LEN(REQ.INDENT_LEVEL) = " + length + ")) ORDER BY REQ.INDENT_LEVEL", includeDeleted);
				}
				else
				{
					requirements = Retrieve(userId, projectId, "SUBSTRING(REQ.INDENT_LEVEL, 1, " + (length - 3) + ") = '" + parentIndentLevel + "' AND ((LEN(REQ.INDENT_LEVEL) = " + (length - 3) + " AND REQ.IS_SUMMARY = 1) OR (LEN(REQ.INDENT_LEVEL) = " + length + " AND REQ.IS_SUMMARY = 0)) ORDER BY REQ.INDENT_LEVEL", includeDeleted);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a set of requirement from the system via a custom SQL filter and/or sort and where the number of rows is restricted</summary>
		/// <param name="customFilterSort">A custom SQL WHERE and/or ORDER BY clause</param>
		/// <param name="numRows">The number of rows to return</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <param name="onlyShowVisible">Do we only want to include requirements that are visible for this user</param>
		/// <returns>Requirement list</returns>
		protected List<RequirementView> Retrieve(int userId, int? projectId, string customFilterSort, int numRows, bool includeDeleted = false, bool onlyShowVisible = false)
		{
			const string METHOD_NAME = "Retrieve()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementView> requirements;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Call the stored proc to get the list of requirements
					requirements = context.Requirement_RetrieveCustom(userId, projectId, customFilterSort, numRows, includeDeleted, onlyShowVisible).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return (requirements);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the requirements summary (importance vs. status) data-table for a particular project</summary>
		/// <param name="projectId">The project we want the summary report for</param>
		/// <param name="releaseId">The release we want to filter on (null for all)</param>
		/// <param name="projectTemplateId">the id of the project template</param>
		/// <returns>An dataset of importance vs. status</returns>
		public DataSet RetrieveProjectSummary(int projectId, int projectTemplateId, int? releaseId)
		{
			const string METHOD_NAME = "RetrieveProjectSummary";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Create a new empty untyped dataset with a table to hold the summary data and columns
			//to hold the status codes. We use datasets because they are a more natural choice
			//than strongly typed classes when the columns are dynamic:
			//http://stackoverflow.com/questions/11383513/ef4-to-a-dataset-and-other-pivot-table-madness

			DataSet projectSummaryDataSet = new DataSet();
			projectSummaryDataSet.Tables.Add("RequirementSummary");
			projectSummaryDataSet.Tables["RequirementSummary"].Columns.Add("RequirementStatusId", typeof(int));
			projectSummaryDataSet.Tables["RequirementSummary"].Columns.Add("RequirementStatusName", typeof(string));
			//Make RequirementStatusId the primary key
			projectSummaryDataSet.Tables["RequirementSummary"].PrimaryKey = new DataColumn[] { projectSummaryDataSet.Tables["RequirementSummary"].Columns["RequirementStatusId"] };

			//We need to get a list of all the possible importance levels (for the column headings)
			List<Importance> importances = RequirementImportance_Retrieve(projectTemplateId);

			//Iterate through the summary dataset and add the columns
			for (int i = 0; i < importances.Count; i++)
			{
				//Add the importance ID as the column name
				projectSummaryDataSet.Tables["RequirementSummary"].Columns.Add(importances[i].ImportanceId.ToString(), typeof(int));
			}

			//Now add the (None) and TOTAL columns
			projectSummaryDataSet.Tables["RequirementSummary"].Columns.Add("None", typeof(int));
			projectSummaryDataSet.Tables["RequirementSummary"].Columns.Add("Total", typeof(int));

			//Get a list of the different statuses and add to summary
			List<RequirementStatus> statuses = RetrieveStatuses();
			for (int i = 0; i < statuses.Count; i++)
			{
				System.Data.DataRow dataRow = projectSummaryDataSet.Tables["RequirementSummary"].NewRow();
				dataRow["RequirementStatusId"] = statuses[i].RequirementStatusId;
				dataRow["RequirementStatusName"] = statuses[i].Name;
				projectSummaryDataSet.Tables["RequirementSummary"].Rows.Add(dataRow);
			}

			//Now we need to execute the query for each column to retrieve the count of requirements per scope level
			//Iterate through the summary dataset and add the columns
			List<RequirementStatusCountInfo> requirementStatusCountInfos;
			for (int i = 0; i < importances.Count; i++)
			{
				//Get the importance id
				int importanceId = importances[i].ImportanceId;

				//Get the dataset of requirement counts against scope level
				requirementStatusCountInfos = RetrieveCountByImportance(projectId, importanceId, releaseId);

				//Now iterate through this dataset of count against scope level and add to summary
				for (int j = 0; j < requirementStatusCountInfos.Count; j++)
				{
					//capture the scope-level id from the count dataset
					int statusId = requirementStatusCountInfos[j].RequirementStatusId;

					//Find the row with the matching ID in the summary table
					System.Data.DataRow dataRow = projectSummaryDataSet.Tables["RequirementSummary"].Rows.Find(statusId);

					//The importance ID is the column name
					if (dataRow != null && projectSummaryDataSet.Tables["RequirementSummary"].Columns.Contains(importanceId.ToString()))
					{
						dataRow[importanceId.ToString()] = requirementStatusCountInfos[j].RequirementCount;
					}
				}
			}

			//Now we need to add the requirements that don't have an importance level
			requirementStatusCountInfos = RetrieveCountByImportance(projectId, null, releaseId);

			//Now iterate through this dataset of count against scope level and add to summary
			for (int j = 0; j < requirementStatusCountInfos.Count; j++)
			{
				//capture the statusId from the count dataset
				int statusId = requirementStatusCountInfos[j].RequirementStatusId;

				//Find the row with the matching ID in the summary table
				System.Data.DataRow dataRow = projectSummaryDataSet.Tables["RequirementSummary"].Rows.Find(statusId);

				//The importance ID is the column name
				if (dataRow != null && projectSummaryDataSet.Tables["RequirementSummary"].Columns.Contains("None"))
				{
					dataRow["None"] = requirementStatusCountInfos[j].RequirementCount;
				}
			}

			//Finally we need to iterate through each row and calculate the total column
			for (int i = 0; i < projectSummaryDataSet.Tables["RequirementSummary"].Rows.Count; i++)
			{
				//Now iterate through the columns (except for the last one which is the total and first which is the ID)
				int total = 0;
				for (int j = 1; j < projectSummaryDataSet.Tables["RequirementSummary"].Columns.Count; j++)
				{
					if ((projectSummaryDataSet.Tables["RequirementSummary"].Rows[i][j]).GetType() == typeof(int))
					{
						int count = (int)projectSummaryDataSet.Tables["RequirementSummary"].Rows[i][j];
						total += count;
					}
				}

				//Finally set the row total field
				projectSummaryDataSet.Tables["RequirementSummary"].Rows[i]["Total"] = total;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

			return projectSummaryDataSet;
		}

		/// <summary>Retrieves a count of requirements by scope level for a particular importance level</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="importanceId">The importance ID (pass null) for those where it's NULL</param>
		/// <param name="releaseId">The release to filter on (pass null for all)</param>
		/// <returns>Untyped dataset of requirement count by scope level</returns>
		/// <remarks>Summary requirements are included in this count since they now have intrinsic attributes</remarks>
		protected List<RequirementStatusCountInfo> RetrieveCountByImportance(int projectId, int? importanceId, int? releaseId)
		{
			const string METHOD_NAME = "RetrieveCountByImportance()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementStatusCountInfo> requirementStatusCount;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Call the stored procedure to get the count by importance
					requirementStatusCount = context.Requirement_RetrieveCountByImportance(projectId, importanceId, releaseId).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirementStatusCount;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

		}

		/// <summary>Retrieves a list of incidents mapped against requirement</summary>
		/// <param name="projectId">The project ID we're interested in</param>
		/// <param name="releaseId">The release we want to filter on (null for all)</param>
		/// <param name="onlyIncludeWithOpenIncidents">Do we only display rows with open incidents</param>
		/// <param name="numberOfRows">The number of rows to return</param>
		/// <returns>Untyped dataset of requirements with a count of the number incidents (open and total)</returns>
		/// <remarks>Currently only returns Requirements marked as summary items or children with incidents. Note that we filter the incidents by detected release not the requirements themselves</remarks>
		public List<RequirementIncidentCount> RetrieveIncidentCount(int projectId, int? releaseId, int numberOfRows, bool onlyIncludeWithOpenIncidents)
		{
			const string METHOD_NAME = "RetrieveIncidentCount()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Call the stored procedure for retrieving the the list of open incidents and total incidents for requirements
				List<RequirementIncidentCount> requirementIncidentCounts;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					requirementIncidentCounts = context.Requirement_RetrieveIncidentCount(projectId, releaseId, numberOfRows, onlyIncludeWithOpenIncidents).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirementIncidentCounts;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a dataset of total requirement coverage for a project group</summary>
		/// <param name="projectGroupId">The project group we're interested in</param>
		/// <param name="activeReleasesOnly">Should we only show for active releases</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <returns>Untyped dataset of requirements coverage (i.e. passed, failed, not run, not covered</returns>
		public List<RequirementCoverageSummary> RetrieveCoverageSummary(int projectGroupId, bool activeReleasesOnly, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveCoverageSummary()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementCoverageSummary> requirementCoverageSummary;

				//Call the stored procedure for retrieving the total number of requirements per coverage status
				//(i.e. sum of coverage per requirement normalized by the count for that requirement
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//The following statuses should not be counted
					List<int> requirementStatusesToIgnore = new List<int>();
					requirementStatusesToIgnore.Add((int)Requirement.RequirementStatusEnum.Rejected);
					requirementStatusesToIgnore.Add((int)Requirement.RequirementStatusEnum.Obsolete);

					requirementCoverageSummary = context.Requirement_RetrieveGroupCoverage(projectGroupId, requirementStatusesToIgnore.ToDatabaseSerialization(), activeReleasesOnly, includeDeleted).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirementCoverageSummary;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a dataset of total requirement coverage for a project</summary>
		/// <param name="projectId">The project ID we're interested in</param>
		/// <param name="releaseId">The release we want to filter on (null for all, -2 = active releases only)</param>
		/// <param name="filterTestCases">Do we want to apply the release filter to the test case (vs. the requirements themselves)</param>
		/// <returns>Untyped dataset of requirements coverage (i.e. passed, failed, not run, not covered</returns>
		public List<RequirementCoverageSummary> RetrieveCoverageSummary(int projectId, int? releaseId, bool filterTestCases)
		{
			const string METHOD_NAME = "RetrieveCoverageSummary()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementCoverageSummary> coverageList;

				//If we need to filter the underlying test cases by the release, then we can't use the cached
				//coverage status stored in the requirements, but instead need to do directly to the test runs
				//and test cases
				if (filterTestCases && releaseId.HasValue && releaseId > 0)
				{
					//First we need to get a dataset of all the test cases in the project filtered by the release
					Business.TestCaseManager testCaseManager = new Business.TestCaseManager();
					List<TestCaseReleaseView> testCases = testCaseManager.RetrieveByReleaseId(projectId, releaseId.Value, "Name", true, 1, Int32.MaxValue, null, 0, TestCaseManager.TEST_CASE_FOLDER_ID_ALL_TEST_CASES);

					//Now we need to get a list of all the requirements in the project together with their mapped test cases
					List<RequirementTestCase> requirementsTestCases;
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						requirementsTestCases = context.Requirement_RetrieveTestCasesByRelease(projectId, releaseId.Value).ToList();
					}

					//Now lets iterate through this table, populating the coverage
					double notRunCount = 0;
					double passedCount = 0;
					double failedCount = 0;
					double cautionCount = 0;
					double blockedCount = 0;
					double notApplicableCount = 0;
					float runningNotRunCount = 0;
					float runningPassedCount = 0;
					float runningFailedCount = 0;
					float runningCautionCount = 0;
					float runningBlockedCount = 0;
					float runningTotalCount = 0;
					float runningNotApplicableCount = 0;
					for (int i = 0; i < requirementsTestCases.Count; i++)
					{
						RequirementTestCase requirementTestCase = requirementsTestCases[i];
						//Find the matching test case status
						int testCaseId = requirementTestCase.TestCaseId;
						TestCaseReleaseView testCaseRow = testCases.FirstOrDefault(t => t.TestCaseId == testCaseId);
						//Now add the current status to the running totals
						if (testCaseRow != null)
						{
							runningTotalCount++;
							switch (testCaseRow.ExecutionStatusId)
							{
								case (int)TestCase.ExecutionStatusEnum.Passed:
									runningPassedCount++;
									break;
								case (int)TestCase.ExecutionStatusEnum.Failed:
									runningFailedCount++;
									break;
								case (int)TestCase.ExecutionStatusEnum.Caution:
									runningCautionCount++;
									break;
								case (int)TestCase.ExecutionStatusEnum.Blocked:
									runningBlockedCount++;
									break;
								case (int)TestCase.ExecutionStatusEnum.NotRun:
									runningNotRunCount++;
									break;
								case (int)TestCase.ExecutionStatusEnum.NotApplicable:
									runningNotApplicableCount++;
									break;
							}
						}

						//See if this is either the last requirement or the next requirement is different
						int currentRequirementId = requirementTestCase.RequirementId;
						bool changeInRequirement = false;
						if (i >= requirementsTestCases.Count - 1)
						{
							changeInRequirement = true;
						}
						else
						{
							RequirementTestCase nextRequirementTestCase = requirementsTestCases[i + 1];
							int nextRequirementId = nextRequirementTestCase.RequirementId;
							if (nextRequirementId != currentRequirementId)
							{
								changeInRequirement = true;
							}
						}
						if (changeInRequirement)
						{
							//Populate the summary counts
							if (runningTotalCount > 0)
							{
								passedCount += runningPassedCount / runningTotalCount;
								failedCount += runningFailedCount / runningTotalCount;
								cautionCount += runningCautionCount / runningTotalCount;
								blockedCount += runningBlockedCount / runningTotalCount;
								notRunCount += runningNotRunCount / runningTotalCount;
								notApplicableCount += runningNotApplicableCount / runningTotalCount;
								Logger.LogTraceEvent("debug", currentRequirementId + "=" + runningFailedCount + ":" + runningBlockedCount + ":" + runningTotalCount);
							}

							//Reset the running totals
							runningNotRunCount = 0;
							runningPassedCount = 0;
							runningFailedCount = 0;
							runningCautionCount = 0;
							runningBlockedCount = 0;
							runningNotApplicableCount = 0;
							runningTotalCount = 0;
						}
					}

					//Now we need to create the new empty dataset to hold the requirements coverage summary
					coverageList = new List<RequirementCoverageSummary>();

					//Now we need to add each status one at a time using the appropriate query

					//Passed
					RequirementCoverageSummary coverageSummary = new RequirementCoverageSummary();
					coverageSummary.CoverageStatusOrder = 1;
					coverageSummary.CoverageStatus = "Passed";
					coverageSummary.CoverageCount = passedCount;
					coverageList.Add(coverageSummary);

					//Failed
					coverageSummary = new RequirementCoverageSummary();
					coverageSummary.CoverageStatusOrder = 2;
					coverageSummary.CoverageStatus = "Failed";
					coverageSummary.CoverageCount = failedCount;
					coverageList.Add(coverageSummary);

					//Blocked
					coverageSummary = new RequirementCoverageSummary();
					coverageSummary.CoverageStatusOrder = 3;
					coverageSummary.CoverageStatus = "Blocked";
					coverageSummary.CoverageCount = blockedCount;
					coverageList.Add(coverageSummary);

					//Caution
					coverageSummary = new RequirementCoverageSummary();
					coverageSummary.CoverageStatusOrder = 4;
					coverageSummary.CoverageStatus = "Caution";
					coverageSummary.CoverageCount = cautionCount;
					coverageList.Add(coverageSummary);

					//Not Run
					coverageSummary = new RequirementCoverageSummary();
					coverageSummary.CoverageStatusOrder = 5;
					coverageSummary.CoverageStatus = "Not Run";
					coverageSummary.CoverageCount = notRunCount;
					coverageList.Add(coverageSummary);

					//Not Applicable
					coverageSummary = new RequirementCoverageSummary();
					coverageSummary.CoverageStatusOrder = 7;
					coverageSummary.CoverageStatus = "Not Applicable";
					coverageSummary.CoverageCount = notRunCount;
					coverageList.Add(coverageSummary);

					//Not Covered
					//Get the list of requirements not mapped to test cases that are part of the release
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						List<int> requirementStatusesToIgnore = new List<int>();
						requirementStatusesToIgnore.Add((int)Requirement.RequirementStatusEnum.Rejected);
						requirementStatusesToIgnore.Add((int)Requirement.RequirementStatusEnum.Obsolete);
						double notCoveredCount = context.Requirement_RetrieveNotCoveredCount(projectId, releaseId, requirementStatusesToIgnore.ToDatabaseSerialization()).FirstOrDefault().Value;

						//Add the entry to the list
						coverageSummary = new RequirementCoverageSummary();
						coverageSummary.CoverageStatusOrder = 6;
						coverageSummary.CoverageStatus = "Not Covered";
						coverageSummary.CoverageCount = notCoveredCount;
						coverageList.Add(coverageSummary);
					}
				}
				else
				{
					//Create select command for retrieving the total number of requirements per coverage status
					//(i.e. sum of coverage per requirement normalized by the count for that requirement
					//The statuses to ignore are embedded within the stored procedure in this case.
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						//release Id = -2 ==> All Active Releases
						coverageList = context.Requirement_RetrieveCoverageSummary(projectId, releaseId).ToList();
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return coverageList;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Returns a list of requirement statuses that are in use by any workflows for the current template
		/// </summary>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <returns>The list of statuses</returns>
		/// <remarks>Used in the planning board to know which statuses to show</remarks>
		public List<RequirementStatus> RetrieveStatusesInUse(int projectTemplateId, bool includePrePlanned = true, bool includePlannedAndPost = true, bool includeObsolete = true)
		{
			const string METHOD_NAME = "RetrieveStatusesInUse()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			const int PLANNED_POSITION_ENUM = 5;

			try
			{
				List<RequirementStatus> statuses;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementStati
								where
									//get active statuses
									r.IsActive &&
									//of these get any that have a transition in AND out of them, but additionally include the default status - Requested
									//((r.WorkflowTransitionsInput.Any(w => w.Workflow.ProjectTemplateId == projectTemplateId) &&
									//r.WorkflowTransitionsOutput.Any(w => w.Workflow.ProjectTemplateId == projectTemplateId)) ||
									//r.RequirementStatusId == (int)Requirement.RequirementStatusEnum.Requested) &&
									//of these get statuses that fit in the criteria below
									((includePrePlanned || r.Position >= PLANNED_POSITION_ENUM) &&
									(includePlannedAndPost || r.Position < PLANNED_POSITION_ENUM)) &&
									(includeObsolete || r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Obsolete)
								orderby r.Position
								select r;

					statuses = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return statuses;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list of requirement statuses</summary>
		/// <returns>List of requirement statuses</returns>
		public List<RequirementStatus> RetrieveStatuses(bool includePrePlanned = true, bool includePlannedAndPost = true, bool includeObsolete = true)
		{
			const string METHOD_NAME = "RetrieveStatuses()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//_staticRequirementStatuses is a static list of ALL statuses. So if we want to retrieve all, and the statis list is already created return it
				if (_staticRequirementStatuses != null && includePrePlanned && includePlannedAndPost && includeObsolete)
				{
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					return _staticRequirementStatuses;
				}
				// otherwise run the query
				else
				{
					const int PLANNED_POSITION_ENUM = 5;
					List<RequirementStatus> statuses;

					//Create select command for retrieving the lookup data
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						var query = from r in context.RequirementStati
									where
										r.IsActive &&
										((includePrePlanned || r.Position >= PLANNED_POSITION_ENUM) &&
										(includePlannedAndPost || r.Position < PLANNED_POSITION_ENUM)) &&
										(includeObsolete || r.RequirementStatusId != (int)Requirement.RequirementStatusEnum.Obsolete)
									orderby r.Position
									select r;

						statuses = query.ToList();
					}
					//if we want to retrieve all, and have not yet set the static list, write out to it
					if (_staticRequirementStatuses == null && includePrePlanned && includePlannedAndPost && includeObsolete)
					{
						_staticRequirementStatuses = statuses;
					}
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					return statuses;
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public RequirementStatus RetrieveStatusById(int statusId)
		{
			const string METHOD_NAME = "RetrieveStatusById()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				RequirementStatus status;

				//Create select command for retrieving the lookup data
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementStati
								where r.RequirementStatusId == statusId
								select r;

				status = query.FirstOrDefault();
				}
				//if we want to retrieve all, and have not yet set the static list, write out to it
					
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return status;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public RequirementStatus RetrieveStatusByName(string statusName)
		{
			const string METHOD_NAME = "RetrieveStatusByName()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				RequirementStatus status;

				//Create select command for retrieving the lookup data
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementStati
								where r.Name == statusName
								select r;

					status = query.FirstOrDefault();
				}
				//if we want to retrieve all, and have not yet set the static list, write out to it

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return status;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Updates a list of requirements that are passed-in. This only updates the attributes not the positional data</summary>
		/// <param name="requirements">The requirements to be updated</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="rollbackId">Whether or not to update history.</param>
		/// <param name="projectId">The project we're interested in</param>
		public void Update(int userId, int projectId, List<Requirement> requirements, long? rollbackId = null, bool updHistory = true, string signature = null)
		{
			const string METHOD_NAME = "Update()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the template associated with the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//We need to keep track of any that have changed release/iteration
				Dictionary<int, int?> requirementsWithChangedReleaseIteration = new Dictionary<int, int?>();
				List<int> releasesNeedingRefreshingEffort = new List<int>();
				List<int> releasesNeedingRefreshingTestStatus = new List<int>();
				List<int> requirementsWithTestCasesToFlagAsSuspect = new List<int>();
				List<int> requirementsNeedingTaskTestRefreshing = new List<int>();
				//Need to retrieve the project planning settings to know how to handle certain changes
				ProjectManager projectManager = new ProjectManager();
				Project project = projectManager.RetrieveById(projectId);
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Loop through each requirement
					foreach (Requirement requirement in requirements)
					{
						//Make sure change tracking is enabled
						requirement.StartTracking();

						//If we have just set a release for the first time, switch to Planned if we're in a lower status
						if (project.IsReqStatusAutoPlanned && requirement.ReleaseId.HasValue && (!requirement.ChangeTracker.OriginalValues.ContainsKey("ReleaseId") || requirement.ChangeTracker.OriginalValues["ReleaseId"] == null))
						{
							if (requirement.RequirementStatusId == (int)Requirement.RequirementStatusEnum.Requested ||
									requirement.RequirementStatusId == (int)Requirement.RequirementStatusEnum.Accepted ||
									requirement.RequirementStatusId == (int)Requirement.RequirementStatusEnum.UnderReview)
							{
								//If the status actually changed FROM planned, then instead of switching TO planned,
								//just unset the release
								if (requirement.ChangeTracker.OriginalValues.ContainsKey("RequirementStatusId") && (int)requirement.ChangeTracker.OriginalValues["RequirementStatusId"] == (int)Requirement.RequirementStatusEnum.Planned)
								{
									requirement.ReleaseId = null;
								}
								else
								{
									requirement.RequirementStatusId = (int)Requirement.RequirementStatusEnum.Planned;
									//Add to the list to have task/test status updating
									if (!requirementsNeedingTaskTestRefreshing.Contains(requirement.RequirementId))
									{
										requirementsNeedingTaskTestRefreshing.Add(requirement.RequirementId);
									}
								}
							}
						}

						//If the estimate changed, need to update the corresponding effort value
						if ((requirement.EstimatePoints.HasValue && requirement.ChangeTracker.OriginalValues.ContainsKey("EstimatePoints") && requirement.ChangeTracker.OriginalValues["EstimatePoints"] == null) ||
							(!requirement.EstimatePoints.HasValue && requirement.ChangeTracker.OriginalValues.ContainsKey("EstimatePoints") && requirement.ChangeTracker.OriginalValues["EstimatePoints"] != null) ||
							(requirement.EstimatePoints.HasValue && requirement.ChangeTracker.OriginalValues.ContainsKey("EstimatePoints") && (decimal)requirement.ChangeTracker.OriginalValues["EstimatePoints"] != requirement.EstimatePoints.Value))
						{
							//Make sure that the estimated points is in the allowed range for the data-type
							requirement.EstimatePoints = ValidateEstimatePoints(requirement.EstimatePoints);

							//Get the estimated effort from the story point estimate
							requirement.EstimatedEffort = GetEstimatedEffortFromEstimatePoints(projectId, requirement.EstimatePoints);

							//Add to the list to have task/test status updating
							if (!requirementsNeedingTaskTestRefreshing.Contains(requirement.RequirementId))
							{
								requirementsNeedingTaskTestRefreshing.Add(requirement.RequirementId);
							}

							//Also need to update the release
							if (requirement.ReleaseId.HasValue && !releasesNeedingRefreshingEffort.Contains(requirement.ReleaseId.Value))
							{
								releasesNeedingRefreshingEffort.Add(requirement.ReleaseId.Value);
							}
						}

						//If the status changed then we need to update the releases (potentially)
						if (requirement.ChangeTracker.OriginalValues.ContainsKey("RequirementStatusId"))
						{
							//Also need to update the release
							if (requirement.ReleaseId.HasValue && !releasesNeedingRefreshingEffort.Contains(requirement.ReleaseId.Value))
							{
								releasesNeedingRefreshingEffort.Add(requirement.ReleaseId.Value);
							}
						}

						//If we have just switched status to In-Progress and we do not have any Tasks, automatically
						//create a single task associated with the requirement - not for SpiraTest
						//Also check that this setting is specified for this project

						//GER - 20220803 - Removed this code for ValidationMaster.  There are not limits to licensing of ValidationMaster.
						//if (License.LicenseProductName != LicenseProductNameEnum.SpiraTest)
						{
							//Need to also have a estimated effort set
							if (requirement.RequirementStatusId == (int)Requirement.RequirementStatusEnum.InProgress &&
								(requirement.ChangeTracker.OriginalValues.ContainsKey("RequirementStatusId") &&
								(int)requirement.ChangeTracker.OriginalValues["RequirementStatusId"] != (int)Requirement.RequirementStatusEnum.InProgress) &&
								requirement.TaskCount == 0 && requirement.EstimatedEffort.HasValue)
							{
								bool autoCreateTasks = project.IsTasksAutoCreate;
								if (autoCreateTasks)
								{
									//See if we have a release/iteration and default the start-date to the start of the iteration
									Nullable<DateTime> startDate = null;
									Nullable<DateTime> endDate = null;
									if (requirement.ReleaseId.HasValue)
									{
										ReleaseManager releaseManager = new ReleaseManager();
										ReleaseView release = releaseManager.RetrieveById2(projectId, requirement.ReleaseId.Value);
										startDate = release.StartDate;

										if (requirement.EstimatedEffort.HasValue)
										{
											//Now add on the effort to make the end date, assuming no other tasks
											//TODO: Need to make the system handle weekends and other non-working days
											decimal hrsPerDay = project.WorkingHours;
											int daysPerWeek = project.WorkingDays;
											decimal effortInHours = requirement.EstimatedEffort.Value / 60M;
											int numberDays = (int)Math.Ceiling(effortInHours / hrsPerDay);
											endDate = startDate.Value.AddDays(numberDays);
										}
										else
										{
											endDate = startDate;
										}
									}

									//Task and requirement priorities both have scores, see if we can find a matching one
									int? taskPriorityId = null;
									if (requirement.ImportanceId.HasValue)
									{
										Importance importance = RequirementImportance_RetrieveById(requirement.ImportanceId.Value);
										if (importance != null)
										{
											List<TaskPriority> taskPriorities = new TaskManager().TaskPriority_Retrieve(projectTemplateId);
											TaskPriority taskPriority = taskPriorities.FirstOrDefault(p => p.Score == importance.Score);
											if (taskPriority != null)
											{
												taskPriorityId = taskPriority.TaskPriorityId;
											}
										}
									}

									//Create a new task linked to this requirement
									//We use the default task type
									TaskManager taskManager = new TaskManager();
									taskManager.Insert(
										requirement.ProjectId,
										userId,
										Task.TaskStatusEnum.InProgress,
										null,
										null,
										requirement.RequirementId,
										requirement.ReleaseId,
										requirement.OwnerId,
										taskPriorityId,
										requirement.Name,
										requirement.Description,
										startDate,
										endDate,
										requirement.EstimatedEffort,
										null,
										requirement.EstimatedEffort,
										userId
										);

									//Add to the list to have task/test status updating
									if (!requirementsNeedingTaskTestRefreshing.Contains(requirement.RequirementId))
									{
										requirementsNeedingTaskTestRefreshing.Add(requirement.RequirementId);
									}
								}
							}
						}

						//We need to keep track of any that have changed release/iteration
						//Since we need to move tasks and add any linked test cases to the release
						int? moveTasksToReleaseId = null;
						bool makeChange = false;
						if (requirement.ReleaseId.HasValue)
						{
							if (requirement.ChangeTracker.OriginalValues.ContainsKey("ReleaseId"))
							{
								if (requirement.ChangeTracker.OriginalValues["ReleaseId"] != null)
								{
									//See if the releases have changed
									int oldReleaseId = (int)requirement.ChangeTracker.OriginalValues["ReleaseId"];
									if (oldReleaseId != requirement.ReleaseId.Value)
									{
										makeChange = true;
										moveTasksToReleaseId = requirement.ReleaseId.Value;
									}
								}
								else
								{
									//Previously no release was set, so move any tasks to the new release
									makeChange = true;
									moveTasksToReleaseId = requirement.ReleaseId.Value;
								}
							}
						}
						else if (requirement.ChangeTracker.OriginalValues.ContainsKey("ReleaseId") && requirement.ChangeTracker.OriginalValues["ReleaseId"] != null)
						{
							//Previously a release was set, so unlink the tasks from the release
							makeChange = true;
							moveTasksToReleaseId = null;
						}

						//See if we have a change to make
						if (makeChange)
						{
							requirementsWithChangedReleaseIteration.Add(requirement.RequirementId, moveTasksToReleaseId);
							//Add to the list to have task/test status updating
							if (!requirementsNeedingTaskTestRefreshing.Contains(requirement.RequirementId))
							{
								requirementsNeedingTaskTestRefreshing.Add(requirement.RequirementId);
							}
						}

						//If the requirement has an estimate and no tasks, but its release changed, need to refresh the Release effort values
						if (requirement.EstimatePoints.HasValue && requirement.TaskCount == 0 && requirement.ChangeTracker.OriginalValues.ContainsKey("ReleaseId"))
						{
							//Handle the case of a release being set, unset or changed
							if (requirement.ChangeTracker.OriginalValues["ReleaseId"] == null)
							{
								if (requirement.ReleaseId.HasValue && !releasesNeedingRefreshingEffort.Contains(requirement.ReleaseId.Value))
								{
									releasesNeedingRefreshingEffort.Add(requirement.ReleaseId.Value);
								}
							}
							else
							{
								if (!releasesNeedingRefreshingEffort.Contains((int)requirement.ChangeTracker.OriginalValues["ReleaseId"]))
								{
									releasesNeedingRefreshingEffort.Add((int)requirement.ChangeTracker.OriginalValues["ReleaseId"]);
								}
								if (requirement.ReleaseId.HasValue && !releasesNeedingRefreshingEffort.Contains(requirement.ReleaseId.Value))
								{
									releasesNeedingRefreshingEffort.Add(requirement.ReleaseId.Value);
								}
							}

							//Add to the list to have task/test status updating
							if (!requirementsNeedingTaskTestRefreshing.Contains(requirement.RequirementId))
							{
								requirementsNeedingTaskTestRefreshing.Add(requirement.RequirementId);
							}
						}

						//If the requirement estimate, status changed, the test coverage changed, or the task counts changed,
						//then we need to do a refresh of the task/test status
						//Add to the list to have task/test status updating
						if (!requirementsNeedingTaskTestRefreshing.Contains(requirement.RequirementId))
						{
							if (requirement.ChangeTracker.OriginalValues.ContainsKey("EstimatePoints") ||
								requirement.ChangeTracker.OriginalValues.ContainsKey("EstimatedEffort") ||
								requirement.ChangeTracker.OriginalValues.ContainsKey("TaskCount") ||
								requirement.ChangeTracker.OriginalValues.ContainsKey("RequirementStatusId") ||
								requirement.ChangeTracker.OriginalValues.ContainsKey("TaskEstimatedEffort") ||
								requirement.ChangeTracker.OriginalValues.ContainsKey("TaskActualEffort") ||
								requirement.ChangeTracker.OriginalValues.ContainsKey("TaskPercentLateFinish") ||
								requirement.ChangeTracker.OriginalValues.ContainsKey("TaskPercentLateStart") ||
								requirement.ChangeTracker.OriginalValues.ContainsKey("TaskPercentNotStart") ||
								requirement.ChangeTracker.OriginalValues.ContainsKey("TaskPercentOnTime") ||
								requirement.ChangeTracker.OriginalValues.ContainsKey("TaskProjectedEffort") ||
								requirement.ChangeTracker.OriginalValues.ContainsKey("TaskRemainingEffort") ||
								requirement.ChangeTracker.OriginalValues.ContainsKey("CoverageCountBlocked") ||
								requirement.ChangeTracker.OriginalValues.ContainsKey("CoverageCountCaution") ||
								requirement.ChangeTracker.OriginalValues.ContainsKey("CoverageCountFailed") ||
								requirement.ChangeTracker.OriginalValues.ContainsKey("CoverageCountPassed") ||
								requirement.ChangeTracker.OriginalValues.ContainsKey("CoverageCountTotal")
								)
							{
								requirementsNeedingTaskTestRefreshing.Add(requirement.RequirementId);
							}
						}


						//If we have any changes, we need to flag any test cases as suspect
						//The requirement needs to be accepted (or later) and the test case must not be draft, rejected or obsolete
						if ((requirement.RequirementStatusId == (int)Requirement.RequirementStatusEnum.Accepted ||
							requirement.RequirementStatusId == (int)Requirement.RequirementStatusEnum.Planned ||
							requirement.RequirementStatusId == (int)Requirement.RequirementStatusEnum.InProgress ||
							requirement.RequirementStatusId == (int)Requirement.RequirementStatusEnum.Developed ||
							requirement.RequirementStatusId == (int)Requirement.RequirementStatusEnum.Tested ||
							requirement.RequirementStatusId == (int)Requirement.RequirementStatusEnum.Completed)
							&& requirement.ChangeTracker.OriginalValues.Count > 0)
						{
							//See if any test cases need to be flagged
							if (!requirementsWithTestCasesToFlagAsSuspect.Contains(requirement.RequirementId))
							{
								requirementsWithTestCasesToFlagAsSuspect.Add(requirement.RequirementId);
							}
						}

						//Update the last-update and concurrency dates
						requirement.LastUpdateDate = DateTime.UtcNow;
						requirement.ConcurrencyDate = DateTime.UtcNow;

						//Now apply the changes
						context.Requirements.ApplyChanges(requirement);
						
						//Save the changes, recording any history changes, and sending any notifications
						context.SaveChanges(userId, true, true, rollbackId);

						//new HistoryManager().LogCreation(projectId, userId, Artifact.ArtifactTypeEnum.RequirementSignature, requirement.RequirementId, DateTime.UtcNow);
					}
				}

				//If any of the requirements had a change of release/iteration, then assign all the non-started tasks to the new release/iteration
				//and add any linked test cases to the release
				foreach (KeyValuePair<int, int?> kvp in requirementsWithChangedReleaseIteration)
				{
					int requirementId = kvp.Key;
					int? releaseId = kvp.Value;
					TaskManager taskManager = new TaskManager();
					List<TaskView> tasks = taskManager.RetrieveByRequirementId(requirementId);
					foreach (TaskView taskView in tasks)
					{
						Task task = taskView.ConvertTo<TaskView, Task>();
						task.StartTracking();
						//Only move not-started and deferred tasks
						if (task.TaskStatusId == (int)Task.TaskStatusEnum.NotStarted || task.TaskStatusId == (int)Task.TaskStatusEnum.Deferred)
						{
							task.ReleaseId = releaseId;
						}
						taskManager.Update(task, userId, rollbackId);
					}

					if (releaseId.HasValue)
					{
						TestCaseManager testCaseManager = new TestCaseManager();
						testCaseManager.AddTestCasesToRequirementRelease(projectId, requirementId, releaseId.Value, userId);
						if (!releasesNeedingRefreshingTestStatus.Contains(releaseId.Value))
						{
							releasesNeedingRefreshingTestStatus.Add(releaseId.Value);
						}
					}
				}

				//Rollup the status and progress for all affected requirements
				foreach (int requirementId in requirementsNeedingTaskTestRefreshing)
				{
					RefreshTaskProgressAndTestCoverage(projectId, requirementId);
				}

				//Refresh the release if we have any requirements that have an estimate and no tasks that changed release
				new ReleaseManager().RefreshProgressEffortTestStatus(projectId, releasesNeedingRefreshingEffort, false, true);

				//Mark any test cases as suspect
				foreach (int requirementId in requirementsWithTestCasesToFlagAsSuspect)
				{
					MarkLinkedTestCasesAsSuspect(projectId, requirementId, userId);
				}
			}
			catch (OptimisticConcurrencyException exception)
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

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		public TST_REQUIREMENT_SIGNATURE RetrieveReqSignature(int reqId, int userId)
		{
			const string METHOD_NAME = "RetrieveReqSignature";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the list of test cases in the project
				TST_REQUIREMENT_SIGNATURE reqSignature;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.RequirementSignatures
								where t.REQUIREMENT_ID == reqId && t.USER_ID == userId
								select t;

					reqSignature = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return reqSignature;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public TST_REQUIREMENT_SIGNATURE RetrieveRequiredApproveredSignatureById(int requirementId, int userId)
		{
			const string METHOD_NAME = "RetrieveRequiredApproveredSignatureById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			try
			{
				TST_REQUIREMENT_SIGNATURE approvalUsers = new TST_REQUIREMENT_SIGNATURE();

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.RequirementSignatures
								select t;

					approvalUsers = query.Where(x => x.REQUIREMENT_ID == requirementId && x.USER_ID == userId).FirstOrDefault();

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
				}

				return approvalUsers;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}

		}

		public List<TST_REQUIREMENT_SIGNATURE> RetrieveRequiredApproversById(int requirementId)
		{
			const string METHOD_NAME = "RetrieveRequiredApproversByProjectId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			try
			{
				List<TST_REQUIREMENT_SIGNATURE> approvalUsers = new List<TST_REQUIREMENT_SIGNATURE>();

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.RequirementSignatures
								select t;

					approvalUsers = query.Where(x => x.REQUIREMENT_ID == requirementId).ToList();
					
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
				}

				return approvalUsers;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}

		}

		public List<TST_REQUIREMENT_SIGNATURE> RetrieveRequiredApproversByUserId(int userId, int projectId)
		{
			const string METHOD_NAME = "RetrieveRequiredApproversById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			try
			{
				List<TST_REQUIREMENT_SIGNATURE> approvalUsers = new List<TST_REQUIREMENT_SIGNATURE>();

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the workflow transitions
					var query = from t in context.RequirementSignatures
								select t;

					approvalUsers = query.Where(x => x.USER_ID == userId && x.PROJECT_ID == projectId && !x.TST_REQUIREMENT.IsDeleted && x.STATUS_ID != (int)Requirement.RequirementStatusEnum.Approved).ToList();

					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					Logger.Flush();
				}

				return approvalUsers;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}

		}

		//public List<int> RetrieveRequiredApproverIds(int projectId)
		//{
		//	const string METHOD_NAME = "RetrieveRequiredApproversByProjectId";

		//	Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
		//	try
		//	{
		//		List<int> approvalUsers = new List<int>();

		//		using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
		//		{
		//			//Get the workflow transitions
		//			var query = from t in context.RequirementApprovalUsers
		//						select t;

		//			approvalUsers = query.Where(x => x.PROJECT_ID == projectId).ToList();

		//			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		//			Logger.Flush();
		//		}

		//		return approvalUsers;
		//	}
		//	catch (Exception exception)
		//	{
		//		Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
		//		throw;
		//	}

		//}

		/// <summary>
		/// Marks certain test cases linked to a requirement as suspect
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="requirementId">The id of the requirement</param>
		/// <param name="userId">The id of the user</param>
		protected void MarkLinkedTestCasesAsSuspect(int projectId, int requirementId, int userId)
		{
			const string METHOD_NAME = "MarkLinkedTestCasesAsSuspect";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestCases
								where
									t.Requirements.Any(r => r.RequirementId == requirementId) &&
									t.TestCaseStatusId != (int)TestCase.TestCaseStatusEnum.Obsolete &&
									t.TestCaseStatusId != (int)TestCase.TestCaseStatusEnum.Draft &&
									t.TestCaseStatusId != (int)TestCase.TestCaseStatusEnum.Rejected &&
									!t.IsSuspect
								orderby t.TestCaseId
								select t;

					List<TestCase> testCases = query.ToList();
					if (testCases.Count > 0)
					{
						foreach (TestCase testCase in testCases)
						{
							testCase.StartTracking();
							testCase.IsSuspect = true;
						}

						//Need to force a detection of changes (to ensure history recorded)
						context.DetectChanges();

						context.SaveChanges(userId, true, true, null);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Creates a new requirement from an existing incident</summary>
		/// <param name="incidentId">The id of the incident to use as its basis</param>
		/// <param name="userId">The user performing the action</param>
		/// <returns>The ID of the newly created requirement</returns>
		/// <remarks>
		/// 1) An association is also added between the requirement and incident
		/// 2) Any attachments linked to the incident are linked to the new requirement as well
		/// </remarks>
		public int CreateFromIncident(int incidentId, int userId)
		{
			const string METHOD_NAME = "CreateFromIncident()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the incident in question
				IncidentManager incidentManager = new IncidentManager();
				Incident incident = incidentManager.RetrieveById(incidentId, true);
				int projectId = incident.ProjectId;
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Convert the estimated effort back into story points
				decimal? estimatePoints = null;
				if (incident.EstimatedEffort.HasValue)
				{
					estimatePoints = GetEstimatePointsFromEffort(projectId, incident.EstimatedEffort.Value);
				}

				//See if we can match the priority to a requirement importance by Name value
				int? importanceId = null;
				if (incident.PriorityId.HasValue)
				{
					IncidentPriority incidentPriority = incidentManager.RetrieveIncidentPriorityById(incident.PriorityId.Value);
					if (incidentPriority != null)
					{
						//See if we have a match on name (cannot use score because it's not used for incidents)
						List<Importance> importances = RequirementImportance_Retrieve(projectTemplateId);
						Importance importance = importances.FirstOrDefault(i => i.Name == incidentPriority.Name);
						if (importance != null)
						{
							importanceId = importance.ImportanceId;
						}
					}
				}

				//See if we have a component specified (incidents allow multiple, if there is more than one we ignore since we cannot be sure which one is correct)
				int? componentId = null;
				if (!String.IsNullOrEmpty(incident.ComponentIds))
				{
					List<int> components = incident.ComponentIds.FromDatabaseSerialization_List_Int32();
					if (components.Count == 1)
					{
						componentId = components[0];
					}
				}

				//Now we need to create the new requirement (at the end of the requirements tree)
				int requirementId = Insert(
					userId,
					projectId,
					incident.ResolvedReleaseId,
					componentId,
					(int?)null,
					Requirement.RequirementStatusEnum.Requested,
					null,
					incident.OpenerId,
					incident.OwnerId,
					importanceId,
					incident.Name,
					incident.Description,
					estimatePoints,
					userId
					);

				//Next we need to create a link between the two
				ArtifactLinkManager artifactLink = new ArtifactLinkManager();
				artifactLink.Insert(projectId, DataModel.Artifact.ArtifactTypeEnum.Incident, incidentId, DataModel.Artifact.ArtifactTypeEnum.Requirement, requirementId, userId, GlobalResources.General.Requirement_ReqCreatedFromIncident, DateTime.UtcNow);

				//Next copy across the resolutions as comments
				if (incident.Resolutions != null)
				{
					DiscussionManager discussionManager = new DiscussionManager();
					foreach (IncidentResolution resolution in incident.Resolutions)
					{
						discussionManager.Insert(resolution.CreatorId, requirementId, Artifact.ArtifactTypeEnum.Requirement, resolution.Resolution, projectId, true, false);
					}
				}

				//Now associate any attachments
				AttachmentManager attachmentManager = new AttachmentManager();
				attachmentManager.Copy(projectId, Artifact.ArtifactTypeEnum.Incident, incidentId, Artifact.ArtifactTypeEnum.Requirement, requirementId);

				//Finally we need to copy over any List/Multilist custom properties that are the same between the two artifact types
				CustomPropertyManager customPropertyManager = new CustomPropertyManager();
				customPropertyManager.ArtifactCustomProperty_CopyListValues(projectId, projectTemplateId, userId, Artifact.ArtifactTypeEnum.Incident, incidentId, Artifact.ArtifactTypeEnum.Requirement, requirementId);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirementId;
			}
			catch (ArtifactNotExistsException exception)
			{
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, exception);
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

		/// <summary>
		/// Sends a creation notification, typically only used for API creation calls where we need to retrieve and force it as 'added'
		/// </summary>
		/// <param name="requirementId">The id of the requirement</param>
		/// <param name="artifactCustomProperty">The custom property row</param>
		/// <param name="newComment">The new comment (if any)</param>
		/// <remarks>Fails quietly but logs errors</remarks>
		public void SendCreationNotification(int requirementId, ArtifactCustomProperty artifactCustomProperty, string newComment)
		{
			const string METHOD_NAME = "SendCreationNotification";
			//Send a notification
			try
			{
				RequirementView notificationArt = RetrieveById2(null, requirementId);
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

		/// <summary>
		/// Suggests a new minutes/point metric based on the tasks in the current project
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>NULL if it cannot return a metric (e.g. no tasks), otherwise the new metric</returns>
		public int? SuggestNewPointEffortMetric(int projectId)
		{
			const string METHOD_NAME = "SuggestNewPointEffortMetric(int)";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int? newPointEffortMetric = null;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the list of requirement estimates vs. task effort, aggregated
					var query = from r in context.Requirements
								where r.ProjectId == projectId && !r.IsDeleted && r.EstimatePoints != null && r.TaskProjectedEffort != null
								group r by 1
									into g
								select new
								{
									EstimatePoints = g.Sum(x => x.EstimatePoints),
									TaskProjectedEffort = g.Sum(x => x.TaskProjectedEffort)
								};

					//See if we have a value and if so, get the new metric
					var result = query.FirstOrDefault();
					if (result != null)
					{
						if (result.EstimatePoints.HasValue && result.TaskProjectedEffort.HasValue && result.EstimatePoints.Value != 0)
						{
							decimal estimatePoints = result.EstimatePoints.Value;
							int taskProjectEffort = result.TaskProjectedEffort.Value;
							decimal metric = taskProjectEffort / estimatePoints;
							newPointEffortMetric = (int)metric;
						}
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return newPointEffortMetric;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Reverse engineers the requirement estimate (in story points) from a specific estimated effort
		/// </summary>
		/// <param name="estimatedEffort">The estimated effort (in minutes)</param>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The story point count</returns>
		public decimal? GetEstimatePointsFromEffort(int projectId, int? estimatedEffort)
		{
			if (estimatedEffort.HasValue)
			{
				//Get the project settings to know the conversion factor
				ProjectManager projectManager = new ProjectManager();
				Project project = projectManager.RetrieveById(projectId);
				decimal estimatePoints = ((decimal)(estimatedEffort.Value)) / project.ReqPointEffort;
				return estimatePoints;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Gets the total estimated effort for a specific release for requirements that have no tasks
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="releaseId">The id of the release</param>
		/// <returns>The total requirement estimated effort</returns>
		/// <remarks>
		/// Does NOT include child iterations of releases or requirements that have tasks
		/// </remarks>
		protected internal int? GetReleaseEstimatedEffort(int projectId, int releaseId)
		{
			const string METHOD_NAME = "GetReleaseEstimatedEffort";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int? estimatedEffort = null;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					estimatedEffort = context.Requirement_GetReleaseEstimate(projectId, releaseId).FirstOrDefault();
				}

				return estimatedEffort;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}
		/// <summary>
		/// Calculates the estimated effort (in minutes) from a specific number of estimated story points
		/// </summary>
		/// <param name="projectId">The current project</param>
		/// <param name="estimatePoints">The estimate (in points)</param>
		/// <returns>The estimated effort (in minutes)</returns>
		public int? GetEstimatedEffortFromEstimatePoints(int projectId, decimal? estimatePoints)
		{
			if (estimatePoints.HasValue)
			{
				//Get the project settings to know the conversion factor
				ProjectManager projectManager = new ProjectManager();
				Project project = projectManager.RetrieveById(projectId);
				int estimateInMinutes = (int)(estimatePoints.Value * project.ReqPointEffort);
				return estimateInMinutes;
			}
			else
			{
				return null;
			}
		}

		/// <summary>Inserts a new requirement into the system at a point in front of the passed in requirement</summary>
		/// <param name="existingRequirementId">The requirement id of the existing requirement that we are inserting in front of. If null, then insert requirement at end</param>
		/// <param name="status">The status of the requirement</param>
		/// <param name="typeId">The type of the requirement(null = default)</param>
		/// <param name="componentId">The is of the component its part of (optional)</param>
		/// <param name="authorId">The user id of the person who created the requirement</param>
		/// <param name="ownerId">The user id of the person who owns the requirement (optional)</param>
		/// <param name="importanceId">The business value of the requirement</param>
		/// <param name="name">The short description of the requirement</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="description">The detailed description of the requirement (optional)</param>
		/// <param name="releaseId">The release that the requirement is targeted for (optional)</param>
		/// <param name="estimatePoints">The estimated of the requirement (in points) (optional)</param>
		/// <param name="creatorLogId">The user that we should log the creation history event for</param>
		/// <param name="ignoreLastInserted">should we ignore the last requirement and insert at root level (if existingRequirement is null)</param>
		/// <returns>The ID of the newly created requirement</returns>
		public int Insert(int userId, int projectId, int? releaseId, int? componentId, int? existingRequirementId, Requirement.RequirementStatusEnum status, int? typeId, int authorId, int? ownerId, int? importanceId, string name, string description, decimal? estimatePoints, int creatorLogId, bool ignoreLastInserted = false, bool logHistory = true)
		{
			const string METHOD_NAME = "Insert()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				string indentLevel;

				//Check to see if we are being passed an existing requirement to insert before
				if (existingRequirementId.HasValue)
				{
					//First lets restore the existing requirement
					RequirementView existingRequirement = RetrieveById2(projectId, existingRequirementId.Value, true);
					if (existingRequirement == null)
					{
						throw (new System.Exception("Cannot retrieve existing requirement"));
					}
					indentLevel = existingRequirement.IndentLevel;
				}
				else
				{
					//Get the next available indent level and parent id
					indentLevel = this.GetNextAvailableIndentLevel(userId, projectId, ignoreLastInserted);

					//If we have no rows then default to indent-level AAA
					if (String.IsNullOrEmpty(indentLevel))
					{
						indentLevel = "AAA";
					}
					else
					{
						//Now increment the indent level
						indentLevel = HierarchicalList.IncrementIndentLevel(indentLevel);
					}
				}

				//Now we need to actually perform the insert
				int requirementId = Insert(userId, projectId, releaseId, componentId, indentLevel, status, typeId, authorId, ownerId, importanceId, name, description, estimatePoints, creatorLogId, logHistory);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirementId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Inserts a new requirement into the system under an existing parent requirement</summary>
		/// <param name="parentRequirementId">The requirement id of the existing requirement that we are inserting underneath</param>
		/// <param name="status">The scope level of the requirement</param>
		/// <param name="authorId">The user id of the person who created the requirement</param>
		/// <param name="ownerId">The user id of the person who owns the requirement (optional)</param>
		/// <param name="importanceId">The business value of the requirement</param>
		/// <param name="componentId">The id of the component the requirement is a part of (optional)</param>
		/// <param name="typeId">The type of requirement being added (null = default)</param>
		/// <param name="name">The short description of the requirement</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="description">The detailed description of the requirement (optional)</param>
		/// <param name="releaseId">The release that the requirement is targeted for (optional)</param>
		/// <param name="estimatePoints">The estimated of the requirement (in points) (optional)</param>
		/// <returns>The ID of the newly created requirement</returns>
		public int InsertChild(int userId, int projectId, int? releaseId, int? componentId, int? parentRequirementId, Requirement.RequirementStatusEnum status, int? typeId, int authorId, int? ownerId, int? importanceId, string name, string description, decimal? estimatePoints, int creatorLogId)
		{
			const string METHOD_NAME = "InsertChild()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//Inititialization
			int requirementId = -1;

			try
			{
				//Check to make sure we are being passed an existing requirement to insert underneath
				if (parentRequirementId.HasValue)
				{
					RequirementView parentRequirement = RetrieveById(userId, projectId, parentRequirementId.Value, true);
					List<RequirementView> childRequirements = RetrieveChildren(Business.UserManager.UserInternal, projectId, parentRequirement.IndentLevel, false, true);

					//See if we have any existing child requirements
					if (childRequirements.Count > 0)
					{
						//Get the indent level of the last existing child
						string indentLevel = childRequirements[childRequirements.Count - 1].IndentLevel;

						//Now get the next indent level and use for that for the new item
						indentLevel = HierarchicalList.IncrementIndentLevel(indentLevel);

						//Now insert the requirement at the specified position
						requirementId = Insert(userId, projectId, releaseId, componentId, indentLevel, status, typeId, authorId, ownerId, importanceId, name, description, estimatePoints, creatorLogId);

						//Because the child list can contain deleted items, need to explicitly set the parent to a summary item
						//if not already
						if (!parentRequirement.IsSummary)
						{
							parentRequirement.IsSummary = true;
							parentRequirement.IsExpanded = true;
							UpdatePositionalData(userId, new List<RequirementView>() { parentRequirement });
						}
					}
					else
					{
						//We have no children so get the indent level of the parent and increment that
						//i.e. insert after the parent, then we can do an indent
						string indentLevel = HierarchicalList.IncrementIndentLevel(parentRequirement.IndentLevel);

						//Now insert the requirement at the specified position
						requirementId = Insert(userId, projectId, releaseId, componentId, indentLevel, status, typeId, authorId, ownerId, importanceId, name, description, estimatePoints, creatorLogId);

						//Finally perform an indent
						Indent(userId, projectId, requirementId);
					}
				}
				else
				{
					//Now insert the requirement at the end of the list
					requirementId = Insert(userId, projectId, releaseId, componentId, (int?)null, status, typeId, authorId, ownerId, importanceId, name, description, estimatePoints, creatorLogId);
				}
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return requirementId;
		}

		/// <summary>Inserts a new requirement into the system at a specified indent level</summary>
		/// <param name="indentLevel">The indent level to use for the new requirement</param>
		/// <param name="status">The scope level of the requirement</param>
		/// <param name="logHistory">Should we log a history item</param>
		/// <param name="creatorLogId">The user to log history for</param>
		/// <param name="typeId">The type of the requirement (null = use default for project)</param>
		/// <param name="componentId">The component it belongs to (optional)</param>
		/// <param name="authorId">The user id of the person who created the requirement</param>
		/// <param name="ownerId">The user id of the person who owns the requirement (optional)</param>
		/// <param name="importanceId">The business value of the requirement (optional)</param>
		/// <param name="name">The short description of the requirement</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="description">The detailed description of the requirement (optional)</param>
		/// <param name="releaseId">The release that the requirement is targeted for (optional)</param>
		/// <param name="estimatePoints">The estimated of the requirement (in points) (optional)</param>
		/// <returns>The ID of the newly created requirement</returns>
		public int Insert(int userId, int projectId, int? releaseId, int? componentId, string indentLevel, Requirement.RequirementStatusEnum status, int? typeId, int authorId, int? ownerId, int? importanceId, string name, string description, decimal? estimatePoints, int creatorLogId, bool logHistory = true)
		{
			const string METHOD_NAME = "Insert()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				ProjectManager projectManager = new ProjectManager();
				Project project = projectManager.RetrieveById(projectId);

				//If we have a release set, switch to Planned if we're in a lower status
				if (project.IsReqStatusAutoPlanned && releaseId.HasValue)
				{
					if (status == Requirement.RequirementStatusEnum.Requested ||
							status == Requirement.RequirementStatusEnum.Accepted ||
							status == Requirement.RequirementStatusEnum.UnderReview)
					{
						status = Requirement.RequirementStatusEnum.Planned;
					}
				}

				//If no requirement type specified, get the default one for the current project template
				if (!typeId.HasValue)
				{

					int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
					RequirementType type = RequirementType_RetrieveDefault(projectTemplateId);
					typeId = type.RequirementTypeId;
				}

				int requirementId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Begin transaction - needed to maintain integrity of hierarchy indent level
					using (TransactionScope transactionScope = new TransactionScope())
					{
						try
						{
							//Before inserting, we need to first update the indent levels of the subsequent requirements
							//so that there is a gap in the indent level structure for our new requirement
							ReorderRequirementsBeforeInsert(projectId, indentLevel);

							if (!estimatePoints.HasValue)
							{
								//See if we have a project default requirements' estimate to use
								try
								{
									//Apply the project default requirements estimate
									if (project.ReqDefaultEstimate.HasValue)
									{
										estimatePoints = project.ReqDefaultEstimate;
									}
								}
								catch (ArtifactNotExistsException)
								{
									//Project no longer exists
									throw new EntityForeignKeyException(String.Format("The project PR{0} associated with this requirement has been deleted.", projectId));
								}
							}

							//Make sure that the estimated points is in the allowed range for the data-type
							estimatePoints = ValidateEstimatePoints(estimatePoints);

							//Get the estimated effort from the story point estimate
							int? estimatedEffort = GetEstimatedEffortFromEstimatePoints(projectId, estimatePoints);

							//Create the new requirement using the defined stored procedure, capturing the requirement id
							requirementId = context.Requirement_Insert(
								projectId,
								releaseId,
								(int)status,
								typeId.Value,
								authorId,
								ownerId,
								importanceId,
								componentId,
								name.MaxLength(255),
								description,
								System.DateTime.UtcNow,
								System.DateTime.UtcNow,
								System.DateTime.UtcNow,
								indentLevel,
								false,
								0,
								0,
								0,
								0,
								0,
								false,
								0,
								0,
								0,
								0,
								0,
								estimatePoints,
								estimatedEffort,
								false,
								true,
								userId
								).FirstOrDefault().Value;

							//Commit transaction - needed to maintain integrity of hierarchy indent level
							transactionScope.Complete();
						}
						catch (System.Exception exception)
						{
							//Rollback transaction - needed to maintain integrity of hierarchy indent level
							//No need to call Rollback() explicitly with EF4, happens on TransactionScope.Dispose()

							Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
							Logger.Flush();
							throw;
						}
					}
				}

				//Add a history record for the inserted incident.
				if (logHistory)
				{
					new HistoryManager().LogCreation(projectId, ((creatorLogId < 1) ? authorId : creatorLogId), DataModel.Artifact.ArtifactTypeEnum.Requirement, requirementId, DateTime.UtcNow);
				}

				//Update the scope level rollup and effort/progress
				RefreshTaskProgressAndTestCoverage(projectId, indentLevel);

				//If we have a release set, also update the release information
				if (releaseId.HasValue)
				{
					new ReleaseManager().RefreshProgressEffortTestStatus(projectId, releaseId.Value, false, true);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirementId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Indents a requirement in the system with the specified ID, including all subordinates</summary>
		/// <param name="requirementId">The ID of the requirement to be indented</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The project we're interested in</param>
		public void Indent(int userId, int projectId, int requirementId)
		{
			const string METHOD_NAME = "Indent()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the requirement to see if it has any children
				RequirementView markedRequirement = RetrieveById(userId, projectId, requirementId, true);
				string indentLevel = markedRequirement.IndentLevel;

				//First Need to check if we have a predecessor or not
				if (indentLevel.Substring(indentLevel.Length - 3, 3) == "AAA")
				{
					//We can't indent if we have no predecessor
					return;
				}

				//Get list of all child requirements for use later on. We don't rely on summary flag
				//because there may be deleted children [IN:002251]
				//Get all child requirements (not just immediate)
				List<RequirementView> childRequirements = RetrieveChildren(userId, projectId, indentLevel, true, true);

				//We need to modify the indent level of the selected item to be one level lower in the tree
				//We do this by getting the indent level of the last child of our predessor and incrementing that.
				//First we need to decrement our indent level to get our predecessors
				string predecessorIndentLevel = GetPreviousPeer(indentLevel, projectId, false);

				//Next we need to make the predecessor a summary item, no need to use StartTracking because we
				//are doing the update with a stored procedure
				bool summaryChange = false;
				RequirementView predecessorReq = RetrieveByIndentLevel(userId, projectId, predecessorIndentLevel, true);
				if (!predecessorReq.IsSummary)
				{
					summaryChange = true;
				}
				predecessorReq.IsSummary = true;
				predecessorReq.IsExpanded = true;
				string newIndentLevel = "";

				try
				{
					//Begin transaction - needed to maintain integrity of hierarchy indent level
					using (TransactionScope transactionScope = new TransactionScope())
					{
						//Make the initial position change update
						UpdatePositionalData(userId, new List<RequirementView>() { predecessorReq });

						//Next get his last immediate child and increment
						List<RequirementView> predecessorChildReqs = Retrieve(userId, projectId, "SUBSTRING(REQ.INDENT_LEVEL, 1, " + predecessorIndentLevel.Length.ToString() + ") = '" + predecessorIndentLevel + "' AND LEN(REQ.INDENT_LEVEL) = " + (predecessorIndentLevel.Length + 3) + " ORDER BY REQ.INDENT_LEVEL DESC", 1, true);
						if (predecessorChildReqs.Count == 0)
						{
							//Create a new child under the predecessor
							newIndentLevel = predecessorIndentLevel + "AAA";
						}
						else
						{
							string predecessorChildIndentLevel = predecessorChildReqs[0].IndentLevel;

							//Increment this to get our new level
							newIndentLevel = HierarchicalList.IncrementIndentLevel(predecessorChildIndentLevel);
						}
						markedRequirement.IndentLevel = newIndentLevel;

						//Now commit the change
						UpdatePositionalData(userId, new List<RequirementView>() { markedRequirement });

						//Now we need to update all the child elements
						if (childRequirements.Count > 0)
						{
							foreach (RequirementView childRequirement in childRequirements)
							{
								//Need to replace the base of the indent level of all its children
								string childIndentLevel = childRequirement.IndentLevel;
								string newChildIndentLevel = HierarchicalList.ReplaceIndentLevelBase(childIndentLevel, indentLevel, newIndentLevel);
								childRequirement.IndentLevel = newChildIndentLevel;
							}
							//Commit the changes
							UpdatePositionalData(userId, childRequirements);
						}

						//Now we need to reorder all subsequent items from where it used to be
						//We don't need to reorder after its new position as it's always inserted at the end
						ReorderRequirementsAfterDelete(projectId, indentLevel);

						//Commit transaction - needed to maintain integrity of hierarchy indent level
						transactionScope.Complete();
					}
				}
				catch (System.Exception exception)
				{
					//Rollback transaction - needed to maintain integrity of hierarchy indent level
					//No need to call Rollback() explicitly with EF4, happens on TransactionScope.Dispose()

					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					Logger.Flush();
					throw;
				}

				//Update the scope level rollup and effort/progress
				RefreshTaskProgressAndTestCoverage(projectId, newIndentLevel);

				//Update the effort of the predecessor release if made a summary
				if (predecessorReq != null && predecessorReq.ReleaseId.HasValue && summaryChange)
				{
					new ReleaseManager().RefreshProgressEffortTestStatus(projectId, predecessorReq.ReleaseId.Value);
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

		/// <summary>Returns the previous peer for the given indent level. If there is none, it will return an empty string. Used for indenting</summary>
		/// <param name="indentLevel">The indent level to find the peer for.</param>
		/// <param name="projectId">The project id to search on.</param>
		/// <param name="includeDeleted">Whether or not to include deleted items when finding the previous peer. Default:FALSE</param>
		/// <returns>The indent level ofthe previous peer. Null if there is none.</returns>
		protected string GetPreviousPeer(string indentLevel, int projectId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "GetPreviousPeer()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				string retString = null;

				//First make sure it HAS a peer.
				if (indentLevel.SafeSubstring(indentLevel.Length - 3) != "AAA")
				{
					//It's not an 'AAA', so it MAY have one before it that is not yet deleted. Get this one
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						retString = context.Requirement_GetPreviousPeer(projectId, indentLevel, includeDeleted).FirstOrDefault();
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return retString;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Returns a comma-separated list of ids for the passed in requirement and its child requirements</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="requirementId">The requirement whose child requirements we want to retrieve</param>
		/// <param name="includeDeleted">Whether or not to include Deleted items in the dataset. Default:FALSE</param>
		/// <returns>List of comma-separated requirement ids</returns>
		/// <remarks>Returns both summary and non-summary child requirements</remarks>
		public string GetSelfAndChildren(int projectId, int requirementId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "GetSelfAndChildren";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the list of the requirement and its children
				List<int> requirementIds;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					requirementIds = context.Requirement_GetSelfAndChildren(projectId, requirementId, includeDeleted).Where(r => r.HasValue).ToList().ConvertAll(r => r.Value);
				}

				//Create the comma-separated list - we default to -1 so that if the current requirement id doesn't exist
				//it will simply return no results as opposed to throwing a query syntax error in the query that uses this
				if (requirementIds.Count > 0)
				{
					return requirementIds.ToDatabaseSerialization();
				}
				else
				{
					return "-1";
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Outdents a requirement in the system with the specified ID, including all subordinates</summary>
		/// <param name="requirementId">The ID of the requirement to be outdented</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The project we're interested in</param>
		public void Outdent(int userId, int projectId, int requirementId)
		{
			const string METHOD_NAME = "Outdent()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the requirement to see if it has any children
				RequirementView markedRequirement = RetrieveById(userId, projectId, requirementId, true);
				string indentLevel = markedRequirement.IndentLevel;

				//First, need to make sure it is not top-level already
				if (indentLevel.Length <= 3)
				{
					//We can't outdent if it is top-level
					return;
				}

				//Get list of all child requirements for use later on. We don't rely on summary flag
				//because there may be deleted children [IN:002251]
				//Get all child requirements (not just immediate)
				List<RequirementView> childRequirements = RetrieveChildren(userId, projectId, indentLevel, true, true);

				//We need to modify the indent level of the selected item to be one level higher in the tree
				//We do this by getting the indent level of the parent and incrementing that
				RequirementView parentRequirement = RetrieveParent(userId, projectId, indentLevel, true);
				string parentIndentLevel = parentRequirement.IndentLevel;
				int parentRequirementId = parentRequirement.RequirementId;
				string incrementedParentIndentLevel = HierarchicalList.IncrementIndentLevel(parentIndentLevel);
				markedRequirement.IndentLevel = incrementedParentIndentLevel;

				try
				{
					//Begin transaction - needed to maintain integrity of hierarchy indent level
					using (TransactionScope transactionScope = new TransactionScope())
					{
						//Need to see how many children the parent has, and if it only has one
						//then outdenting this item will result in it no longer being a summary item
						if (RetrieveChildren(userId, projectId, parentIndentLevel, false).Count <= 1)
						{
							parentRequirement.IsSummary = false;
							parentRequirement.IsExpanded = false;
							UpdatePositionalData(userId, new List<RequirementView> { parentRequirement });
						}

						//Before making the change to the selected item, need to move subsequent items first
						ReorderRequirementsBeforeInsert(projectId, incrementedParentIndentLevel);

						//Now commit the change
						UpdatePositionalData(userId, new List<RequirementView> { markedRequirement });

						//Now we need to update all the child elements
						if (childRequirements.Count > 0)
						{
							foreach (RequirementView childRequirement in childRequirements)
							{
								//Need to replace the base of the indent level of all its children
								string childIndentLevel = childRequirement.IndentLevel;
								string newchildIndentLevel = HierarchicalList.ReplaceIndentLevelBase(childIndentLevel, indentLevel, incrementedParentIndentLevel);
								childRequirement.IndentLevel = newchildIndentLevel;
							}
							//Commit the changes
							UpdatePositionalData(userId, childRequirements);
						}

						//Now we need to reorder all subsequent items of the item itself
						ReorderRequirementsAfterDelete(projectId, indentLevel);

						//Commit transaction - needed to maintain integrity of hierarchy indent level
						transactionScope.Complete();
					}
				}
				catch (System.Exception exception)
				{
					//Rollback transaction - needed to maintain integrity of hierarchy indent level
					//No need to call Rollback() explicitly with EF4, happens on TransactionScope.Dispose()

					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					Logger.Flush();
					throw;
				}

				//Update the scope level, coverage and effort/progress rollups for the new position of this item
				RefreshTaskProgressAndTestCoverage(projectId, requirementId);

				//Also update the scope, coverage and effort/progress level rollups for the old position it was at
				//Unless there are no new children, in which case reset the status/progress of the parent directly
				RefreshTaskProgressAndTestCoverage(projectId, parentRequirementId);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>
		/// Expands/collapses the entire requirements tree for a project, so that it focuses in the selected requirement
		/// </summary>
		/// <param name="userId">The user we're viewing requirements as</param>
		/// <param name="requirementId">The requirement to focus on</param>
		/// <remarks>
		///     a) For a non-summary requirement. It should display its parents and peers only.
		///     b) For a summary requirement. It should display its parents and children only.
		/// </remarks>
		public void FocusOn(int userId, int requirementId)
		{
			const string METHOD_NAME = "FocusOn(int, int)";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Call the stored procedure that handles the expansion
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.Requirement_FocusOn(userId, requirementId);
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

		/// <summary>Expands the list of requirements to the specified number of places</summary>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="level">The number of places to indent to. Pass null to expand all levels</param>
		public void ExpandToLevel(int userId, int projectId, int? level)
		{
			const string METHOD_NAME = "ExpandToLevel()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//If we have been passed in null, change to 999 to represent all levels
			if (!level.HasValue)
			{
				level = 999;
			}

			try
			{
				//Call the stored procedure that handles the expansion
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.Requirement_ExpandToLevel(userId, projectId, level);
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

		/// <summary>Expands a summary requirement in the system with the specified ID. Note that expands do not cascade to subordinate summary items</summary>
		/// <param name="requirementId">The ID of the requirement to be expanded</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The project we're interested in</param>
		public void Expand(int userId, int projectId, int requirementId)
		{
			const string METHOD_NAME = "Expand()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the requirement to make sure it is a collapsed summary one
				RequirementView markedRequirement = RetrieveById(userId, null, requirementId, true);

				//Make sure the requirement is in our project or a shared one
				if (markedRequirement.ProjectId != projectId)
				{
					//See if the requirement is in a project that shares with this one
					ProjectManager projectManager = new ProjectManager();
					List<ProjectArtifactSharing> sharingProjects = projectManager.ProjectAssociation_RetrieveForDestProjectAndArtifact(projectId, Artifact.ArtifactTypeEnum.Requirement);
					if (sharingProjects.Any(p => p.SourceProjectId == markedRequirement.ProjectId))
					{
						projectId = markedRequirement.ProjectId;
					}
					else
					{
						//Do nothing if the project is not correct
						return;
					}
				}

				if (markedRequirement.IsSummary && !markedRequirement.IsExpanded)
				{
					//First we need to update its expanded flag
					markedRequirement.IsExpanded = true;
					UpdatePositionalData(userId, new List<RequirementView>() { markedRequirement });

					//Now get its immediate child items and make them visible
					//Call the stored procedure that makes the child items visible
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						context.Requirement_Expand(userId, projectId, markedRequirement.IndentLevel);
					}
				}
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

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Collapses a summary requirement in the system with the specified ID. Note that collapses cascade to affect all subordinate summary items</summary>
		/// <param name="requirementId">The ID of the requirement to be collapsed</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The project we're interested in</param>
		public void Collapse(int userId, int projectId, int requirementId)
		{
			const string METHOD_NAME = "Collapse()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the requirement to make sure it is an expanded summary one
				RequirementView markedRequirement = RetrieveById(userId, null, requirementId, true);

				//Make sure the requirement is in our project or a shared one
				if (markedRequirement.ProjectId != projectId)
				{
					//See if the requirement is in a project that shares with this one
					ProjectManager projectManager = new ProjectManager();
					List<ProjectArtifactSharing> sharingProjects = projectManager.ProjectAssociation_RetrieveForDestProjectAndArtifact(projectId, Artifact.ArtifactTypeEnum.Requirement);
					if (sharingProjects.Any(p => p.SourceProjectId == markedRequirement.ProjectId))
					{
						projectId = markedRequirement.ProjectId;
					}
					else
					{
						//Do nothing if the project is not correct
						return;
					}
				}

				if (markedRequirement.IsSummary && markedRequirement.IsExpanded)
				{
					//Call the stored procedure that collapses the summary requirement and its children
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						context.Requirement_Collapse(userId, projectId, requirementId);
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

		/// <summary>Moves a requirement (and its children) from one location to another</summary>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="sourceRequirementId">The requirement we want to move</param>
		/// <param name="destRequirementId">The requirement we want to move it in front of</param>
		/// <remarks>Pass null for the destination requirement ID to move the requirement to the end</remarks>
		public void Move(int userId, int projectId, int sourceRequirementId, int? destRequirementId)
		{
			const string METHOD_NAME = "Move()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				string destIndentLevel = "";
				string sourceIndentLevel = "";
				try
				{
					//Begin transaction - needed to maintain integrity of hierarchy indent level
					using (TransactionScope transactionScope = new TransactionScope())
					{
						//Check to see if we are being passed an existing requirement to insert before
						RequirementView destRequirement;
						RequirementView sourceRequirement;
						if (!destRequirementId.HasValue)
						{
							//We simply move the requirement to the end of the list in this case

							//Get the next available indent level and parent id
							destRequirement = Retrieve(userId, projectId, "IS_VISIBLE = 1 ORDER BY REQ.INDENT_LEVEL DESC", 1, false).FirstOrDefault();
							if (destRequirement == null)
							{
								//There are no undeleted items to add after, so make the new root
								destIndentLevel = "AAA";
							}
							else
							{
								destIndentLevel = destRequirement.IndentLevel;

								//Now increment the indent level
								destIndentLevel = HierarchicalList.IncrementIndentLevel(destIndentLevel);
							}
						}
						else
						{
							//Now we need to retrieve the destination requirement
							destRequirement = RetrieveById(userId, projectId, destRequirementId.Value);
							destIndentLevel = destRequirement.IndentLevel;

							//Make sure we're not trying to drag the requirement 'under itself' as this causes very strange issues
							sourceRequirement = RetrieveById(userId, projectId, sourceRequirementId, true);
							sourceIndentLevel = sourceRequirement.IndentLevel;
							if (sourceIndentLevel.Length < destIndentLevel.Length && destIndentLevel.SafeSubstring(0, sourceIndentLevel.Length) == sourceIndentLevel)
							{
								//Simply do nothing and log a warning message
								Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, GlobalResources.Messages.HierarchicalItem_CannotMoveUnderItself);
								return;
							}
						}
						//Next we need to create space in the hierarchy to prepare for the insert part of the move
						//Needed so that we don't violate the indent-level uniqueness constraint
						ReorderRequirementsBeforeInsert(projectId, destIndentLevel);

						//Next we need to retrieve the requirement to see if it has any children
						sourceRequirement = RetrieveById(userId, projectId, sourceRequirementId, true);
						sourceIndentLevel = sourceRequirement.IndentLevel;

						//Need to get the parent and peers to see if moving it will make the parent non-summary
						if (sourceIndentLevel.Length > 3)
						{
							RequirementView parentRequirement = RetrieveParent(UserManager.UserInternal, projectId, sourceIndentLevel, true);
							if (parentRequirement != null)
							{
								string parentIndentLevel = parentRequirement.IndentLevel;

								//Need to see how many children the parent has, and if it only has one
								//then outdenting this item will result in it no longer being a summary item
								if (RetrieveChildren(UserManager.UserInternal, projectId, parentIndentLevel, false).Count <= 1)
								{
									parentRequirement.IsSummary = false;
									parentRequirement.IsExpanded = false;
									UpdatePositionalData(userId, new List<RequirementView>() { parentRequirement });
								}
							}
						}

						//Get dataset of all child requirements for use later on
						List<RequirementView> childRequirements = null;
						if (sourceRequirement.IsSummary)
						{
							//Get all child requirements (not just immediate)
							childRequirements = RetrieveChildren(userId, projectId, sourceIndentLevel, true, true);
						}

						//Update the indent-level of the source requirement to the destination
						sourceRequirement.IndentLevel = destIndentLevel;

						//Now actually reposition the item being moved since we've made space for it in the tree
						UpdatePositionalData(userId, new List<RequirementView>() { sourceRequirement });

						//Now we need to update all the child elements
						if (sourceRequirement.IsSummary)
						{
							foreach (RequirementView childRequirement in childRequirements)
							{
								//Need to replace the base of the indent level of all its children
								string childIndentLevel = childRequirement.IndentLevel;
								string newchildIndentLevel = HierarchicalList.ReplaceIndentLevelBase(childIndentLevel, sourceIndentLevel, destIndentLevel);
								childRequirement.IndentLevel = newchildIndentLevel;
							}

							//Now commit the changes
							UpdatePositionalData(userId, childRequirements);
						}

						//Finally we need to update the indent levels of the subsequent requirements to account for the delete part of the move
						ReorderRequirementsAfterDelete(projectId, sourceIndentLevel);

						//Commit transaction - needed to maintain integrity of hierarchy indent level
						transactionScope.Complete();
					}
				}
				catch (System.Exception exception)
				{
					//Rollback transaction - needed to maintain integrity of hierarchy indent level
					//No need to call Rollback() explicitly with EF4, happens on TransactionScope.Dispose()

					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					Logger.Flush();
					throw;
				}

				//Update the scope level, coverage and estimate/progress rollups for the source and destinations
				RefreshTaskProgressAndTestCoverage(projectId, destIndentLevel);
				RefreshTaskProgressAndTestCoverage(projectId, sourceIndentLevel);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Exports a requirement (and its children) from one project to another</summary>
		/// <param name="userId">The user exporting the requirement</param>
		/// <param name="sourceProjectId">The project we're exporting from</param>
		/// <param name="sourceRequirementId">The id of the requirement being exported</param>
		/// <param name="destProjectId">The project we're exporting to</param>
		/// <returns>The id of the requirement in the new project</returns>
		public int Export(int userId, int sourceProjectId, int sourceRequirementId, int destProjectId)
		{
			const string METHOD_NAME = "Export()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//We need to get the source and destination project templates
				//If they are the same, then certain additional values can get copied across
				int sourceProjectTemplateId = new TemplateManager().RetrieveForProject(sourceProjectId).ProjectTemplateId;
				int destProjectTemplateId = new TemplateManager().RetrieveForProject(destProjectId).ProjectTemplateId;
				bool templatesSame = (sourceProjectTemplateId == destProjectTemplateId);

				//First we need to retrieve the requirement to see if it has any children
				RequirementView sourceRequirement = RetrieveById(userId, sourceProjectId, sourceRequirementId);
				string sourceIndentLevel = sourceRequirement.IndentLevel;

				//Get dataset of all child requirements for use later on
				List<RequirementView> childRequirements = null;
				if (sourceRequirement.IsSummary)
				{
					//Get all child requirements (not just immediate)
					childRequirements = RetrieveChildren(userId, sourceProjectId, sourceIndentLevel, true);
				}

				//Insert a new requirement with the data copied from the existing one
				//We don't associate with a release
				int exportedRequirementId = Insert(
					userId,
					destProjectId,
					null,
					null,
					null,
					(Requirement.RequirementStatusEnum)sourceRequirement.RequirementStatusId,
					(templatesSame) ? (int?)sourceRequirement.RequirementTypeId : null,
					sourceRequirement.AuthorId,
					sourceRequirement.OwnerId,
					(templatesSame) ? sourceRequirement.ImportanceId : null,
					sourceRequirement.Name,
					sourceRequirement.Description,
					sourceRequirement.EstimatePoints,
					userId,
					true, //True, since we don't care about the last inserted level when exporting.
					false); //False, since we don't want to log history and do it manually below.

				//Create a History record..
				new HistoryManager().LogImport(destProjectId, sourceProjectId, sourceRequirementId, userId, DataModel.Artifact.ArtifactTypeEnum.Requirement, exportedRequirementId, DateTime.UtcNow);

				//Now we need to copy across any scenario steps
				ExportScenario(userId, sourceProjectId, sourceRequirementId, destProjectId, exportedRequirementId);

				//We copy custom properties if the templates are the same
				if (templatesSame)
				{
					//Now we need to copy across any custom properties
					new CustomPropertyManager().ArtifactCustomProperty_Export(sourceProjectTemplateId, sourceProjectId, sourceRequirement.RequirementId, destProjectId, exportedRequirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, userId);
				}

				//Now we need to copy across any linked attachments
				AttachmentManager attachmentManager = new AttachmentManager();
				attachmentManager.Export(sourceProjectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, sourceRequirementId, destProjectId, exportedRequirementId);

				//Now retrieve the destination indent level to see if the reorder affected it
				RequirementView destRequirement = RetrieveById(userId, destProjectId, exportedRequirementId);
				string destIndentLevel = destRequirement.IndentLevel;

				//Now we need to insert all the child elements
				if (sourceRequirement.IsSummary)
				{
					//First make the copy an expanded summary item
					destRequirement.IsSummary = true;
					destRequirement.IsExpanded = true;
					UpdatePositionalData(userId, new List<RequirementView>() { destRequirement });

					foreach (RequirementView childRequirement in childRequirements)
					{
						//Need to replace the base of the indent level of all its children
						string childIndentLevel = childRequirement.IndentLevel;
						string newchildIndentLevel = HierarchicalList.ReplaceIndentLevelBase(childIndentLevel, sourceIndentLevel, destIndentLevel);

						//Insert a new requirement with the data copied from the existing one
						//We use the Insert overload that allows insertion at a specific indent level
						int newchildRequirementId = Insert(
							userId,
							destProjectId,
							null,
							null,
							newchildIndentLevel,
							(Requirement.RequirementStatusEnum)childRequirement.RequirementStatusId,
							(templatesSame) ? (int?)childRequirement.RequirementTypeId : null,
							childRequirement.AuthorId,
							childRequirement.OwnerId,
							(templatesSame) ? childRequirement.ImportanceId : null,
							childRequirement.Name,
							childRequirement.Description,
							childRequirement.EstimatePoints,
							userId,
							false
							);

						//Create a History record..
						new HistoryManager().LogImport(destProjectId, sourceProjectId, childRequirement.RequirementId, userId, DataModel.Artifact.ArtifactTypeEnum.Requirement, newchildRequirementId, DateTime.UtcNow);

						//Now we need to override the summary-status to match the child (and also handle the expanded flag)
						RequirementView newchildRequirement = RetrieveById(userId, destProjectId, newchildRequirementId);
						newchildRequirement.IsSummary = childRequirement.IsSummary;
						if (childRequirement.IsSummary)
						{
							//By default all new summary items are displayed as expanded
							newchildRequirement.IsExpanded = true;
						}
						UpdatePositionalData(userId, new List<RequirementView>() { newchildRequirement });

						//Now we need to copy across any scenario steps
						ExportScenario(userId, sourceProjectId, childRequirement.RequirementId, destProjectId, newchildRequirementId);

						//We copy custom properties if the templates are the same
						if (templatesSame)
						{
							//Now we need to copy across any custom properties
							new CustomPropertyManager().ArtifactCustomProperty_Export(sourceProjectTemplateId, sourceProjectId, childRequirement.RequirementId, destProjectId, newchildRequirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, userId);
						}

						//Now we need to copy across any linked attachments
						attachmentManager.Export(sourceProjectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, childRequirement.RequirementId, destProjectId, newchildRequirementId);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the requirement id of the exported item
				return exportedRequirementId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Copies the use case steps / scenario information for a specific requirement</summary>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The current project</param>
		/// <param name="sourceRequirementId">The requirement we're copying coverage FROM</param>
		/// <param name="destRequirementId">The requirement we're copying coverage TO</param>
		protected internal void CopyScenario(int userId, int projectId, int sourceRequirementId, int destRequirementId)
		{
			const string METHOD_NAME = "CopyScenario()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the list of steps that belong to the source requirement
				List<RequirementStep> sourceSteps = RetrieveSteps(sourceRequirementId);

				//Make sure we have some steps
				if (sourceSteps != null && sourceSteps.Count > 0)
				{
					//Add them to the destination
					foreach (RequirementStep sourceStep in sourceSteps)
					{
						InsertStep(projectId, destRequirementId, null, sourceStep.Description, userId);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Copies the use case steps / scenario information for a specific requirement to a requirement in a different project</summary>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="sourceRequirementId">The requirement we're copying coverage FROM</param>
		/// <param name="destRequirementId">The requirement we're copying coverage TO</param>
		/// <param name="sourceProjectId">The project we're copying coverage FROM</param>
		/// <param name="destProjectId">The project we're copying coverage TO</param>
		protected internal void ExportScenario(int userId, int sourceProjectId, int sourceRequirementId, int destProjectId, int destRequirementId)
		{
			const string METHOD_NAME = "ExportScenario()";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the list of steps that belong to the source requirement
				List<RequirementStep> sourceSteps = RetrieveSteps(sourceRequirementId);

				//Make sure we have some steps
				if (sourceSteps != null && sourceSteps.Count > 0)
				{
					//Add them to the destination, make sure they are actually in the source project
					foreach (RequirementStep sourceStep in sourceSteps)
					{
						InsertStep(destProjectId, destRequirementId, null, sourceStep.Description, userId, logHistory: false);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Splits up a requirement into two separate requirements
		/// </summary>
		/// <param name="estimatePoints">The number of estimation points to assign to the NEW requirement (null uses the auto calculation)</param>
		/// <param name="ownerId">The owner of the NEW requirement, leaving null uses the existing requirement's owner</param>
		/// <param name="requirementId">The id of the requirement to split</param>
		/// <param name="name">The name of the new requirement</param>
		/// <param name="userId">The id of the user performing the split</param>
		/// <param name="comment">The comment to add to the association between the two requirements (optional)</param>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The id of the newly created second requirement</returns>
		public int Split(int projectId, int requirementId, string name, int userId, decimal? estimatePoints = null, int? ownerId = null, string comment = null)
		{
			const string METHOD_NAME = "Split";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the template for this project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//First we need to retrieve the requirement we want to copy
				Requirement sourceRequirement = RetrieveById2(projectId, requirementId).ConvertTo<RequirementView, Requirement>();

				//Make sure we're not trying to split a summary requirement
				if (sourceRequirement.IsSummary)
				{
					throw new InvalidOperationException(GlobalResources.Messages.Requirement_CannotSplitSummaryRequirement);
				}


				//See if an estimate split was specified, only relevant if we have an existing estimate
				decimal? newEstimatePoints = null;
				decimal? existingEstimatePoints = null;
				Requirement.RequirementStatusEnum newRequirementStatus = Requirement.RequirementStatusEnum.Requested;
				if (sourceRequirement.EstimatePoints.HasValue)
				{
					//Track changes to the existing requirement being split from
					sourceRequirement.StartTracking();

					if (estimatePoints.HasValue)
					{
						//Subtract these points from the existing requirement
						//Handle the case where we try to assign more points than the original had
						if (estimatePoints.Value > sourceRequirement.EstimatePoints)
						{
							existingEstimatePoints = 0M;
							newEstimatePoints = sourceRequirement.EstimatePoints.Value;
						}
						else
						{
							existingEstimatePoints = sourceRequirement.EstimatePoints.Value - estimatePoints.Value;
							newEstimatePoints = estimatePoints.Value;
						}

						//We don't change the status for this case automatically, so use the same status for the new requirement
						newRequirementStatus = (Requirement.RequirementStatusEnum)sourceRequirement.RequirementStatusId;
					}
					else
					{
						//See what % of the requirement is done
						int progressPercent = sourceRequirement.TaskPercentOnTime;
						if (progressPercent == 100)
						{
							//Make the newly split requirement completed
							newRequirementStatus = Requirement.RequirementStatusEnum.Developed;
						}
						else
						{
							newRequirementStatus = (Requirement.RequirementStatusEnum)sourceRequirement.RequirementStatusId;
						}

						//Use this percentage to determine how much to move to the split requirement
						existingEstimatePoints = (sourceRequirement.EstimatePoints.Value * sourceRequirement.TaskPercentOnTime) / 100M;
						newEstimatePoints = (sourceRequirement.EstimatePoints.Value * (100M - sourceRequirement.TaskPercentOnTime)) / 100M;

						//Need to close existing requirement since we split all of the existing work from it
						if (sourceRequirement.RequirementStatusId == (int)Requirement.RequirementStatusEnum.InProgress || sourceRequirement.RequirementStatusId == (int)Requirement.RequirementStatusEnum.Planned)
						{
							sourceRequirement.RequirementStatusId = (int)Requirement.RequirementStatusEnum.Developed;
						}
					}

					//Update the existing estimate
					sourceRequirement.EstimatePoints = existingEstimatePoints;

					//Next update the estimate/status of the existing requirement
					Update(userId, projectId, new List<Requirement>() { sourceRequirement });
				}

				//If we don't have an override owner, use the source requirement's one
				if (!ownerId.HasValue)
				{
					ownerId = sourceRequirement.OwnerId;
				}

				//Actually perform the insert of the new requirement
				//We put it directly below the existing one
				string destIndentLevel = HierarchicalList.IncrementIndentLevel(sourceRequirement.IndentLevel);
				int newRequirementId = Insert(
					userId,
					projectId,
					sourceRequirement.ReleaseId,
					sourceRequirement.ComponentId,
					destIndentLevel,
					newRequirementStatus,
					sourceRequirement.RequirementTypeId,
					sourceRequirement.AuthorId,
					ownerId,
					sourceRequirement.ImportanceId,
					name,
					sourceRequirement.Description,
					newEstimatePoints,
					userId
					);

				//Now we need to copy across any custom properties
				new CustomPropertyManager().ArtifactCustomProperty_Copy(projectId, projectTemplateId, requirementId, newRequirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, userId);

				//Now we need to copy across any coverage information
				CopyCoverage(userId, projectId, requirementId, newRequirementId);

				//Now we need to copy across any scenario steps
				CopyScenario(userId, projectId, requirementId, newRequirementId);

				//Now we need to copy across any linked attachments
				new AttachmentManager().Copy(projectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, requirementId, newRequirementId);

				//Finally add an association between the tasks
				new ArtifactLinkManager().Insert(projectId, Artifact.ArtifactTypeEnum.Requirement, requirementId, Artifact.ArtifactTypeEnum.Requirement, newRequirementId, userId, comment, DateTime.UtcNow, ArtifactLink.ArtifactLinkTypeEnum.RelatedTo);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the requirement id of the newly created one
				return newRequirementId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Copies a requirement (and its children) from one location to another</summary>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="sourceRequirementId">The requirement we want to copy</param>
		/// <param name="destRequirementId">The requirement we want to copy it in front of</param>
		/// <remarks>
		/// Pass null for the destination requirement ID to copy the requirement to the end.
		/// This function also copies the test coverage information for the requirement(s) in question, as well
		/// as duplicating the various lasts linked to the requirement.
		/// </remarks>
		/// <returns>The id of the copy of the requirement</returns>
		public int Copy(int userId, int projectId, int sourceRequirementId, int? destRequirementId)
		{
			const string METHOD_NAME = "Copy()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the template for this project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//First we need to retrieve the requirement to see if it has any children
				RequirementView sourceRequirement = RetrieveById(userId, projectId, sourceRequirementId);
				string sourceIndentLevel = sourceRequirement.IndentLevel;

				//Get dataset of all child requirements for use later on
				List<RequirementView> childRequirements = null;
				if (sourceRequirement.IsSummary)
				{
					//Get all child requirements (not just immediate)
					childRequirements = RetrieveChildren(userId, projectId, sourceIndentLevel, true);
				}

				//Insert a new requirement with the data copied from the existing one
				int copiedRequirementId = Insert(
					userId,
					projectId,
					sourceRequirement.ReleaseId,
					sourceRequirement.ComponentId,
					destRequirementId,
					(Requirement.RequirementStatusEnum)sourceRequirement.RequirementStatusId,
					sourceRequirement.RequirementTypeId,
					sourceRequirement.AuthorId,
					sourceRequirement.OwnerId,
					sourceRequirement.ImportanceId,
					sourceRequirement.Name + CopiedArtifactNameSuffix,
					sourceRequirement.Description,
					sourceRequirement.EstimatePoints,
					userId
					);

				//Now we need to copy across any coverage information
				CopyCoverage(userId, projectId, sourceRequirementId, copiedRequirementId);

				//Now we need to copy across any scenario steps
				CopyScenario(userId, projectId, sourceRequirementId, copiedRequirementId);

				//Now we need to copy across any Tasks, making sure to reset them back to 'Not Started' with 0% progress
				TaskManager taskManager = new TaskManager();
				List<TaskView> tasks = taskManager.RetrieveByRequirementId(sourceRequirementId);
				foreach (TaskView task in tasks)
				{
					int newTaskId = taskManager.Insert(
						projectId,
						userId,
						Task.TaskStatusEnum.NotStarted,
						task.TaskTypeId,
						task.TaskFolderId,
						copiedRequirementId,
						task.ReleaseId,
						task.OwnerId,
						task.TaskPriorityId,
						task.Name,
						task.Description,
						task.StartDate,
						task.EndDate,
						task.EstimatedEffort,
						null,
						null,
						userId
						);

					//Now we need to copy across any custom properties
					new CustomPropertyManager().ArtifactCustomProperty_Copy(projectId, projectTemplateId, task.TaskId, newTaskId, DataModel.Artifact.ArtifactTypeEnum.Task, userId);
				}

				//Now we need to copy across any custom properties
				new CustomPropertyManager().ArtifactCustomProperty_Copy(projectId, projectTemplateId, sourceRequirementId, copiedRequirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, userId);

				//Now we need to copy across any linked attachments
				AttachmentManager attachment = new AttachmentManager();
				attachment.Copy(projectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, sourceRequirementId, copiedRequirementId);

				//We need to again re-retrieve the destination indent level to see if the reorder affected it
				RequirementView destRequirement = RetrieveById(userId, projectId, copiedRequirementId);
				string destIndentLevel = destRequirement.IndentLevel;

				//Now we need to insert all the child elements
				if (sourceRequirement.IsSummary)
				{
					//First make the copy an expanded summary item
					destRequirement.IsSummary = true;
					destRequirement.IsExpanded = true;
					UpdatePositionalData(userId, new List<RequirementView>() { destRequirement });

					foreach (RequirementView childRequirement in childRequirements)
					{
						//Need to replace the base of the indent level of all its children
						string childIndentLevel = childRequirement.IndentLevel;
						string newchildIndentLevel = HierarchicalList.ReplaceIndentLevelBase(childIndentLevel, sourceIndentLevel, destIndentLevel);

						//Insert a new requirement with the data copied from the existing one
						//We use the Insert overload that allows insertion at a specific indent level
						int newchildRequirementId = Insert(
							userId,
							projectId,
							childRequirement.ReleaseId,
							childRequirement.ComponentId,
							newchildIndentLevel,
							(Requirement.RequirementStatusEnum)childRequirement.RequirementStatusId,
							childRequirement.RequirementTypeId,
							childRequirement.AuthorId,
							childRequirement.OwnerId,
							childRequirement.ImportanceId,
							childRequirement.Name,
							childRequirement.Description,
							childRequirement.EstimatePoints,
							userId
							);

						//Now we need to override the summary-status to match the child (and also handle the expanded flag)
						RequirementView newchildRequirement = RetrieveById(userId, projectId, newchildRequirementId);
						newchildRequirement.IsSummary = childRequirement.IsSummary;
						if (childRequirement.IsSummary)
						{
							//By default all new summary items are displayed as expanded
							newchildRequirement.IsExpanded = true;
						}
						UpdatePositionalData(userId, new List<RequirementView>() { newchildRequirement });

						//Now we need to copy across any scenario steps
						CopyScenario(userId, projectId, childRequirement.RequirementId, newchildRequirementId);

						//Now we need to copy across any coverage information
						CopyCoverage(userId, projectId, childRequirement.RequirementId, newchildRequirementId);

						//Now we need to copy across any Tasks, making sure to reset them back to 'Not Started' with 0% progress
						tasks = taskManager.RetrieveByRequirementId(childRequirement.RequirementId);
						foreach (TaskView task in tasks)
						{
							int newTaskId = taskManager.Insert(
								projectId,
								userId,
								Task.TaskStatusEnum.NotStarted,
								task.TaskTypeId,
								task.TaskFolderId,
								newchildRequirementId,
								task.ReleaseId,
								task.OwnerId,
								task.TaskPriorityId,
								task.Name,
								task.Description,
								task.StartDate,
								task.EndDate,
								task.EstimatedEffort,
								null,
								null,
								userId
								);

							//Now we need to copy across any custom properties
							new CustomPropertyManager().ArtifactCustomProperty_Copy(projectId, projectTemplateId, task.TaskId, newTaskId, DataModel.Artifact.ArtifactTypeEnum.Task, userId);
						}

						//Now we need to copy across any custom properties
						new CustomPropertyManager().ArtifactCustomProperty_Copy(projectId, projectTemplateId, childRequirement.RequirementId, newchildRequirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement, userId);

						//Now we need to copy across any linked attachments
						attachment.Copy(projectId, DataModel.Artifact.ArtifactTypeEnum.Requirement, childRequirement.RequirementId, newchildRequirementId);
					}
				}

				//Send a notification
				SendCreationNotification(copiedRequirementId, null, null);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the requirement id of the copy
				return copiedRequirementId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Cascades a change of summary requirement deletion status to its child requirements.</summary>
		/// <param name="userId">The user making the change</param>
		/// <param name="projectId">The project the summary requirement belongs to</param>
		/// <param name="indentLevel">The indent level of the summary requirement</param>
		/// <param name="isDeleted">The new value of the deletion flag</param>
		protected void CascadeDeleteFlagChange(int userId, int projectId, string indentLevel, bool isDeleted)
		{
			const string METHOD_NAME = "CascadeDeleteFlagChange()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the list of child items
				List<RequirementView> childReqs = RetrieveChildren(userId, projectId, indentLevel, true, true);

				//Update the deleted flag for each of the child items
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					foreach (RequirementView childReq in childReqs)
					{
						//Attach to the context (converting from View to artifact)
						Requirement requirement = childReq.ConvertTo<RequirementView, Requirement>();
						context.Requirements.ApplyChanges(requirement);
						requirement.StartTracking();
						requirement.IsDeleted = isDeleted;
						requirement.LastUpdateDate = DateTime.UtcNow;
						requirement.ConcurrencyDate = DateTime.UtcNow;

						//We don't Log history for child cascaded deletes, only for the primary delete
					}

					//Commit the changes
					context.SaveChanges(userId, false, false, null);
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

		/// <summary>Marks the specified requirement as deleted.</summary>
		/// <param name="userId">The userId making the deletion.</param>
		/// <param name="projectId">The projectId that the requirement belongs to.</param>
		/// <param name="testCaseId">The requirement to delete.</param>
		public void MarkAsDeleted(int userId, int projectId, int requirementId)
		{
			const string METHOD_NAME = "MarkAsDeleted()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the release to get its indent-level
				RequirementView markedRequirement = RetrieveById(userId, projectId, requirementId, true);

				if (markedRequirement != null)
				{

					string indentLevel = markedRequirement.IndentLevel;
					bool summary = markedRequirement.IsSummary;

					//Now we need to retrieve its parent and see if the operation will make it non-summary
					int? parentRequirementId = null;
					if (indentLevel.Length > 3)
					{
						RequirementView parentRequirement = RetrieveParent(userId, projectId, indentLevel, true);
						if (parentRequirement != null)
						{
							string parentIndentLevel = parentRequirement.IndentLevel;
							parentRequirementId = parentRequirement.RequirementId;

							//Need to see how many children the parent has, and if it only has one
							//then outdenting this item will result in it no longer being a summary item
							if (RetrieveChildren(userId, projectId, parentIndentLevel, false).Count <= 1)
							{
								parentRequirement.IsSummary = false;
								parentRequirement.IsExpanded = false;
								UpdatePositionalData(userId, new List<RequirementView>() { parentRequirement });
							}
						}
					}

					//Mark children as deleted.
					CascadeDeleteFlagChange(userId, projectId, markedRequirement.IndentLevel, true);

					//If we have a release also will need to update the release effort information
					int? needToRefreshReleaseId = null;
					if (markedRequirement.ReleaseId.HasValue)
					{
						needToRefreshReleaseId = markedRequirement.ReleaseId.Value;
					}


					//Now mark this one as deleted.
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						//Attach to the context (converting from View to artifact)
						Requirement requirement = markedRequirement.ConvertTo<RequirementView, Requirement>();
						context.Requirements.ApplyChanges(requirement);
						requirement.StartTracking();
						requirement.IsDeleted = true;
						requirement.LastUpdateDate = DateTime.UtcNow;
						requirement.ConcurrencyDate = DateTime.UtcNow;

						//Commit the changes
						context.SaveChanges(userId, false, false, null);
					}

					//Update the coverage, scope level and progress/effort rollups
					RefreshTaskProgressAndTestCoverage(projectId, indentLevel);
					if (needToRefreshReleaseId.HasValue)
					{
						new ReleaseManager().RefreshProgressEffortTestStatus(projectId, needToRefreshReleaseId.Value, false, true);
					}

					//Add a changeset to mark it as deleted.
					new HistoryManager().LogDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.Requirement, requirementId, DateTime.UtcNow);
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				throw;
			}
			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Deletes a requirement in the system that has the specified ID</summary>
		/// <param name="requirementId">The ID of the requirement to be deleted</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The project we're interested in</param>
		public void DeleteFromDatabase(int requirementId, int userId)
		{
			const string METHOD_NAME = "DeleteFromDatabase()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the requirement to get its indent-level
				RequirementView markedRequirement = RetrieveById(userId, null, requirementId, true);
				string indentLevel = markedRequirement.IndentLevel;
				int projectId = markedRequirement.ProjectId;

				//Now we need to retrieve its parent and see if the operation will make it non-summary
				int? parentSummaryStatusChangedRequirementId = null;
				int? parentRequirementId = null;
				if (indentLevel.Length > 3)
				{
					RequirementView parentRequirement = RetrieveParent(userId, markedRequirement.ProjectId, indentLevel);
					if (parentRequirement != null)
					{
						parentRequirementId = parentRequirement.RequirementId;
						string parentIndentLevel = parentRequirement.IndentLevel;

						//Need to see how many children the parent has, and if it only has one
						//then outdenting this item will result in it no longer being a summary item
						if (RetrieveChildren(userId, markedRequirement.ProjectId, parentIndentLevel, false).Count <= 1)
						{
							parentRequirement.IsSummary = false;
							parentRequirement.IsExpanded = false;
							UpdatePositionalData(userId, new List<RequirementView>() { parentRequirement });
							parentSummaryStatusChangedRequirementId = parentRequirement.RequirementId;
						}
					}
				}

				//Now we need to retrieve and handle all its children
				if (markedRequirement.IsSummary)
				{
					List<RequirementView> childRequirements = RetrieveChildren(userId, markedRequirement.ProjectId, indentLevel, false, true);
					foreach (RequirementView childRequirement in childRequirements)
					{
						//Now delete 
						DeleteFromDatabase(childRequirement.RequirementId, userId);
					}
				}

				//Next we need to delete any artifact links to/from this requirement
				new Business.ArtifactLinkManager().DeleteByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Requirement, requirementId);

				//Next we need to delete any attachments associated with the requirement
				new Business.AttachmentManager().DeleteByArtifactId(requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement);

				//Next we need to delete any custom properties associated with the requirement
				new CustomPropertyManager().ArtifactCustomProperty_DeleteByArtifactId(requirementId, DataModel.Artifact.ArtifactTypeEnum.Requirement);

				try
				{
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						//Begin transaction - needed to maintain integrity of hierarchy indent level
						using (TransactionScope transactionScope = new TransactionScope())
						{
							//Actually perform the delete
							context.Requirement_Delete(requirementId);

							//Now we need to reorder all subsequent items
							ReorderRequirementsAfterDelete(projectId, indentLevel);

							//Commit transaction - needed to maintain integrity of hierarchy indent level
							transactionScope.Complete();
						}
					}
				}
				catch (System.Exception exception)
				{
					//Rollback transaction - needed to maintain integrity of hierarchy indent level
					//No need to call Rollback() explicitly with EF4, happens on TransactionScope.Dispose()

					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
					Logger.Flush();
					throw;
				}

				//If there is a parent requirement, recalculate the task and coverage status for that item
				if (parentRequirementId.HasValue)
				{
					RefreshTaskProgressAndTestCoverage(projectId, parentRequirementId.Value);
				}

				//Update the coverage, scope level and progress/effort rollups
				RefreshTaskProgressAndTestCoverage(projectId, indentLevel);

				//Log the purge.
				new HistoryManager().LogPurge(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.Requirement, requirementId, DateTime.UtcNow, markedRequirement.Name);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Deletes the user navigation data for all requirements for a given user</summary>
		/// <param name="userId">The user we want to delete the data for</param>
		/// <remarks>This is primarily used by the unit tests to clean up the system</remarks>
		public void DeleteUserNavigationData(int userId)
		{
			const string METHOD_NAME = "DeleteUserNavigationData";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Actually perform the delete
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.Requirement_DeleteNavigationData(userId);
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

		/// <summary>Checks for and optionally corrects indentation errors.</summary>
		/// <param name="projectID">Project ID of requirements to check</param>
		/// <param name="performFix">To actually perform the fix or not.</param>
		/// <param name="fillerName">Name of created test sets.</param>
		/// <returns>True if all indentations are correct. False if error.</returns>
		public bool CheckIndention(int projectId, bool performFix, string fillerName = "Filler Requirement", dynamic bkgProcess = null)
		{
			const string METHOD_NAME = "CheckIndention";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			bool retBool = true;

			//Get the info from our database..
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				List<DataTools_Requirement> reqs = context.DataTools_Requirements
					.Where(dt => dt.ProjectId == projectId)
					.OrderBy(dt => dt.IndentLevel)
					.ToList();

				//For each one we find, we need to:
				// - Look at its children. If it has children, and it's deleted, undelete it.
				// - Make sure it has a parent. If it has no parent, create one.
				for (int i = 0; i < reqs.Count; i++)
				{
					DataTools_Requirement req = reqs[i];

					//Update status if we have one..
					if (bkgProcess != null)
						bkgProcess.Progress = (int)(i / (float)(reqs.Count) * 100);

					// Check that its parents exist, regardless of Deleted Status.
					int indentIndex = 0;
					string parent = "";
					do
					{
						//Advance to the next level...
						indentIndex += 3;
						parent = req.IndentLevel.SafeSubstring(0, indentIndex);

						//See if the parent exists.
						if (reqs.Count(r => r.IndentLevel.Equals(parent)) < 1)
						{
							//Parent did not exist. Set flag and create it.
							retBool = false;

							if (performFix)
							{
								//We have to insert it manually into the database, as the functions to automagically insert them
								// will try to check and correct for parents.
								context.Requirement_InsertFiller(projectId, fillerName, parent);

								//Log it.
								string msg = "Added missing parent '" + parent + "' to satisfy row #" + i.ToString();
								Logger.LogInformationalEvent(CLASS_NAME + METHOD_NAME, msg);

								//Add it to our list..
								DataTools_Requirement newReq = new DataTools_Requirement();
								newReq.IndentLevel = parent;
								newReq.IsDeleted = false;
								newReq.IsSummary = true;
								newReq.ProjectId = projectId;
								newReq.RequirementId = -1;
								reqs.Add(newReq);
							}
						}
					} while (!parent.Equals(req.IndentLevel));

					// Now check that if it has children, it's marked as summary.
					if (reqs.Any(r =>
						!r.IsDeleted &&
						r.IndentLevel.StartsWith(req.IndentLevel) &&
						r.IndentLevel.Length > req.IndentLevel.Length))
					{
						if (!req.IsSummary)
						{
							//Flag as error.
							retBool = false;

							if (performFix)
							{
								//Pull the requirement..
								Requirement modReq = context.Requirements.SingleOrDefault(r => r.RequirementId == req.RequirementId);
								modReq.StartTracking();
								modReq.IsSummary = true;
								context.SaveChanges();

								//Update this record..
								req.IsSummary = true;
								try
								{
									context.Detach(req);
								}
								catch { }
							}
						}

						//If this one is deleted, see if any of its childred are NOT deleted.
						if (req.IsDeleted)
						{
							int numNoDeleted = reqs.Count(r => r.IndentLevel.StartsWith(req.IndentLevel) && !r.IsDeleted);

							if (numNoDeleted > 0)
							{
								retBool = false;

								if (performFix)
								{
									//Pull the requirement..
									Requirement modReq = context.Requirements.SingleOrDefault(r => r.RequirementId == req.RequirementId);
									modReq.StartTracking();
									modReq.IsDeleted = false;
									context.SaveChanges();

									//Update this record..
									req.IsDeleted = false;
									try
									{
										context.Detach(req);
									}
									catch { }
								}
							}
						}
					}
				}
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return retBool;
		}
		#endregion

		#region Requirement Step Methods

		/// <summary>
		/// Counts the number of steps belonging to a requirement
		/// </summary>
		/// <param name="requirementId">The requirement we're interested in</param>
		/// <param name="includeDeleted">Should we include deleted steps</param>
		/// <returns>The number of steps</returns>
		public int CountSteps(int requirementId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "CountSteps";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int count;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementSteps
								where r.RequirementId == requirementId && (!r.IsDeleted || includeDeleted)
								orderby r.Position, r.RequirementStepId
								select r;

					count = query.Count();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return count;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Copies a single requirement step</summary>
		/// <param name="userId">The user making the copy</param>
		/// <param name="projectId">The current project</param>
		/// <param name="destRequirementId">The requirement that the steps should be copied to (leave null to copy to same)</param>
		/// <param name="sourceRequirementStepId">The step being copied</param>
		/// <param name="destRequirementStepId">The step in the destination requirement that you want to insert it in front of (or null for last position)</param>
		/// <returns>The id of the newly created step</returns>
		public int CopyStep(int userId, int projectId, int sourceRequirementStepId, int? destRequirementId = null, int? destRequirementStepId = null)
		{
			const string METHOD_NAME = "CopyStep";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First get the source step
				RequirementStep requirementStep = RetrieveStepById(sourceRequirementStepId);
				if (requirementStep == null)
				{
					throw new ArtifactNotExistsException("Unable to locate requirement step " + sourceRequirementStepId + " in the project. It no longer exists!");
				}

				if (!destRequirementId.HasValue)
				{
					destRequirementId = requirementStep.RequirementId;
				}

				//Insert the step to the destination requirement
				int newRequirementStepId = InsertStep(
					projectId,
					destRequirementId.Value,
					destRequirementStepId,
					requirementStep.Description,
					userId
					);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return newRequirementStepId;
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

		/// <summary>
		/// Inserts a new requirement step
		/// </summary>
		/// <param name="requirementId">The id of the requirement we're adding the step to</param>
		/// <param name="description">The description of the step</param>
		/// <param name="creatorId">The id of the person adding the step (for history records)</param>
		/// <param name="creationDate">The date the step was added (defaults to the current date/time)</param>
		/// <param name="existingRequirementStepId">The ID of the step to insert in front of(passing null adds it to the end)</param>
		/// <param name="logHistory">Should we log a history entry</param>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The id of the new step</returns>
		/// <remarks>Also updates the last-updated-date of the parent requirement</remarks>
		public int InsertStep(int projectId, int requirementId, int? existingRequirementStepId, string description, int creatorId, DateTime? creationDate = null, bool logHistory = true)
		{
			const string METHOD_NAME = CLASS_NAME + "InsertStep";
			Logger.LogEnteringEvent(METHOD_NAME);

			int returnId = -1;
			try
			{
				//Rewrote to more closely match TestCase Manager's new InsertStep()
				using (var ct = new SpiraTestEntitiesEx())
				{
					//Get the flag if we need to record baseline changes. (Insert changes are still logged.)
					bool logBaseline = new ProjectSettings(projectId).BaseliningEnabled && Global.Feature_Baselines;
					HistoryManager hMgr = new HistoryManager();

					//Get the Requirement with steps. (This is the same as calling TestCaseManager.RetrieveByIdWithSteps()
					Requirement requirement = ct.Requirements
						.Include(r => r.Steps)
						.SingleOrDefault(r => r.RequirementId == requirementId);
					if (requirement == null)
						throw new ArtifactNotExistsException();

					//Get the position we want to insert this step into. (Unlike the TestCaseManager function, which accepts a 
					//  position as a parameter, THIS function is being sent the ID that contains the position. Why? We don't know!
					int? position = null;
					if (existingRequirementStepId.HasValue)
					{
						RequirementStep existingStep = ct.RequirementSteps
							.SingleOrDefault(s => s.RequirementId == requirementId &&
								s.RequirementStepId == existingRequirementStepId);
						if (existingStep != null)
							position = existingStep.Position;
					}

					//If we have no position number, simply default to the next available position
					if (!position.HasValue || position < 1)
					{
						if (requirement.Steps.Count == 0)
						{
							position = 1;
						}
						else
						{
							position = requirement.Steps.Max(g => g.Position) + 1;
						}
					}

					//Begin transaction - needed to maintain integrity of position ordering
					using (TransactionScope ts = new TransactionScope())
					{
						//Start the update.
						requirement.StartTracking();

						//Now we need to move down any of the other test-steps that are below the passed-in position
						long? createSpaceChangeSetId = null; //Record for all the steps.
						foreach (var laterStep in requirement.Steps.Where(t => t.Position >= position.Value))
						{
							laterStep.StartTracking();
							int oldPos = laterStep.Position;
							laterStep.Position++;

							if (logBaseline)
								createSpaceChangeSetId = hMgr.RecordReqStepPosition(
									requirement.ProjectId,
									creatorId,
									requirement.RequirementId,
									requirement.Name,
									laterStep.RequirementStepId,
									oldPos,
									laterStep.Position,
									createSpaceChangeSetId
									);
						}

						//Populate the new entity
						RequirementStep requirementStep = new RequirementStep();
						requirementStep.RequirementId = requirementId;
						requirementStep.Description = description;
						requirementStep.CreationDate = (creationDate.HasValue) ? creationDate.Value : DateTime.UtcNow;
						requirementStep.LastUpdateDate = DateTime.UtcNow;
						requirementStep.ConcurrencyDate = DateTime.UtcNow;
						requirementStep.Position = position.Value;
						requirementStep.IsDeleted = false;

						//Add the step to the table..
						ct.RequirementSteps.AddObject(requirementStep);

						//Update requirement's last updated date.
						requirement.LastUpdateDate = DateTime.UtcNow;

						//Save changes!
						ct.SaveChanges();

						//Commit transaction - needed to maintain integrity of position ordering
						ts.Complete();

						//Get the ID.
						returnId = requirementStep.RequirementStepId;
					}

					//Log history.
					if (logHistory)
						hMgr.LogCreation(projectId, creatorId, Artifact.ArtifactTypeEnum.RequirementStep, returnId, DateTime.UtcNow);
					//See if we need to log positional changes (only in baselining.
					if (logBaseline)
					{
						if (hMgr == null) hMgr = new HistoryManager();

						hMgr.RecordReqStepPosition(
							projectId,
							creatorId,
							requirement.RequirementId,
							requirement.Name,
							returnId,
							-1,
							position.Value);
					}
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return returnId;
		}

		/// <summary>
		/// Updates the last updated date of the requirement (usually called when a step is changed)
		/// </summary>
		/// <param name="context">The EF context</param>
		/// <param name="requirementId">The id of the requirement</param>
		protected void UpdateRequirementLastUpdateDate(SpiraTestEntitiesEx context, int requirementId)
		{
			//Finally we need to update the last updated date of the requirement itself
			var query3 = from r in context.Requirements
						 where r.RequirementId == requirementId
						 select r;

			Requirement requirement = query3.FirstOrDefault();
			if (requirement != null)
			{
				requirement.StartTracking();
				requirement.LastUpdateDate = DateTime.UtcNow;
			}
		}

		/// <summary>
		/// Retrieves all the steps associated with a requirement
		/// </summary>
		/// <param name="requirementId">The id of the requirement</param>
		/// <param name="includeDeleted">Should we include deleted steps</param>
		/// <returns>The list of steps</returns>
		public List<RequirementStep> RetrieveSteps(int requirementId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveSteps";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementStep> requirementSteps;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementSteps
								where r.RequirementId == requirementId && (!r.IsDeleted || includeDeleted)
										&& (!r.Requirement.IsDeleted || includeDeleted)
								orderby r.Position, r.RequirementStepId
								select r;

					requirementSteps = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirementSteps;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves all the deleted steps in a project
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The list of steps</returns>
		/// <remarks>Mainly used in the Undelete history functionality</remarks>
		public List<RequirementStep> RetrieveDeletedSteps(int projectId)
		{
			const string METHOD_NAME = "RetrieveDeletedSteps";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RequirementStep> requirementSteps;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementSteps
								where r.IsDeleted && r.Requirement.ProjectId == projectId
								orderby r.RequirementId, r.RequirementStepId
								select r;

					requirementSteps = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirementSteps;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Physically deletes/purges a deleted requirement step from the database
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="requirementStepId">The id of the step to be purged</param>
		/// <param name="userId">The id of the user making the change</param>
		public void PurgeStep(int projectId, int requirementStepId, int userId)
		{
			const string METHOD_NAME = CLASS_NAME + "PurgeStep()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
				{
					//Our history manager.
					HistoryManager hMgr = new HistoryManager();

					//Turn this off so the query does not change as WE make changes.
					ct.ContextOptions.LazyLoadingEnabled = false;

					//Get the flag if we need to record baseline changes. (Insert changes are still logged.)
					bool logBaseline = new ProjectSettings(projectId).BaseliningEnabled && Global.Feature_Baselines;

					//Begin transaction - needed to maintain integrity of position ordering
					using (TransactionScope transactionScope = new TransactionScope())
					{
						//Get the step..
						RequirementStep requirementStep = ct.RequirementSteps
							.SingleOrDefault(s => s.RequirementStepId == requirementStepId);

						if (requirementStep != null)
						{
							requirementStep.ProjectId = projectId; //Needed for history.

							//Retrtieve the Requirement with all steps.
							int requirementId = requirementStep.RequirementId;
							Requirement requirement = ct.Requirements
								.Include(r => r.Steps)
								.SingleOrDefault(r => r.ProjectId == projectId &&
									r.RequirementId == requirementId);
							//Start recording,
							requirement.StartTracking();

							//First we need to move up any of the other test-steps that are below the passed-in test step
							long? createSpaceChangeSetId = null; //Record for all the steps.
							foreach (RequirementStep laterStep in requirement.Steps.Where(g => g.Position > requirementStep.Position))
							{
								laterStep.StartTracking();
								int oldPos = laterStep.Position;
								laterStep.Position--;

								if (logBaseline)
									createSpaceChangeSetId = hMgr.RecordReqStepPosition(
										requirement.ProjectId,
										userId,
										requirement.RequirementId,
										requirement.Name,
										laterStep.RequirementStepId,
										oldPos,
										laterStep.Position,
										createSpaceChangeSetId
										);
							}

							//Update the last updated date of the test case
							requirement.LastUpdateDate = DateTime.UtcNow;

							//Now log the purge
							hMgr.LogPurge(projectId, userId, Artifact.ArtifactTypeEnum.RequirementStep, requirementStepId, DateTime.UtcNow);
							if (logBaseline)
								hMgr.RecordReqStepPosition(
									requirement.ProjectId,
									userId,
									requirement.RequirementId,
									requirement.Name,
									requirementStep.RequirementStepId,
									requirementStep.Position,
									-1,
									createSpaceChangeSetId);

							//Remove the step.
							ct.RequirementSteps.DeleteObject(requirementStep);

							//Commit these changes
							ct.SaveChanges(userId, true, false, null);

							//Commit transaction - needed to maintain integrity of position ordering
							transactionScope.Complete();
						}
					}

					ct.ContextOptions.LazyLoadingEnabled = true; //Shoul;d not be necessary, but goot cleanup.
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>
		/// Retrieves a single requirement step by its ID
		/// </summary>
		/// <param name="requirementStepId">The id of the step</param>
		/// <param name="populateProject">Should we populate the project id (used in rollbacks only)</param>
		/// <param name="includeDeleted">Should we include deleted steps</param>
		/// <returns>The step or NULL if not found</returns>
		public RequirementStep RetrieveStepById(int requirementStepId, bool includeDeleted = false, bool populateProject = false)
		{
			const string METHOD_NAME = "RetrieveStepById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				RequirementStep requirementStep;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RequirementSteps
								where r.RequirementStepId == requirementStepId && (!r.IsDeleted || includeDeleted)
										&& (!r.Requirement.IsDeleted || includeDeleted)
								select r;

					requirementStep = query.FirstOrDefault();

					if (populateProject && requirementStep != null)
					{
						var query2 = from r in context.Requirements
									 where r.RequirementId == requirementStep.RequirementId
									 select new { r.ProjectId };

						requirementStep.ProjectId = query2.FirstOrDefault().ProjectId;
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirementStep;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates a single requirement step in the system
		/// </summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="updHistory">Should we add a history entry</param>
		/// <param name="rollbackId">Is this part of a rollback?</param>
		/// <param name="userId">The id of the user making the change</param>
		/// <param name="requirementStep">The step being updated</param>
		/// <remarks>Should not be used for position moves</remarks>
		public void UpdateStep(int projectId, RequirementStep requirementStep, int userId, long? rollbackId = null, bool updHistory = true)
		{
			const string METHOD_NAME = "UpdateStep";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Apply the changes to the context
					context.RequirementSteps.ApplyChanges(requirementStep);

					//Finally we need to update the last updated date of the requirement itself
					UpdateRequirementLastUpdateDate(context, requirementStep.RequirementId);

					//If we're updating history, we need to also provide the project id to the artifact
					requirementStep.ProjectId = projectId;

					//Persist changes
					context.SaveChanges(userId, updHistory, true, rollbackId);
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
		/// Moves a step to a specific position in the list (or to the end)
		/// </summary>
		/// <param name="requirementId">The requirement that the steps belong to</param>
		/// <param name="sourceRequirementStepId">The ID of the step we want to move</param>
		/// <param name="destRequirementStepId">The ID of the step we want to move it in front of (or null to move to the end of the list)</param>
		public void MoveStep(int requirementId, int sourceRequirementStepId, int? destRequirementStepId, int userId)
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
						Requirement baseRequirement = ct.Requirements
							.Include(g => g.Steps)
							.SingleOrDefault(r => r.RequirementId == requirementId);

						//If it don't exist, throw an error (handled by RetrieveByIdWithSteps() in TestCaseManager).
						if (baseRequirement == null)
							throw new ArtifactNotExistsException();

						//Make sure that we have the row to be moved in this dataset
						RequirementStep reqStepToMove =
							baseRequirement.Steps.SingleOrDefault(s => s.RequirementStepId == sourceRequirementStepId);
						if (reqStepToMove == null)
							throw new ArtifactNotExistsException("The passed in Requirement id and step id doesn't correspond to a matching step in the requirement!");
						//Now grab the test step at the destination.
						RequirementStep reqStepAtDest = null;
						if (destRequirementStepId.HasValue)
							reqStepAtDest = baseRequirement.Steps.SingleOrDefault(s => s.RequirementStepId == destRequirementStepId.Value);
						// Record the initial Test Step position.
						int startingPosition = reqStepToMove.Position;
						int endingPosition = (!destRequirementStepId.HasValue
							? baseRequirement.Steps.Max(f => f.Position) //Unlike the Add, we do not add 1 here. D'oh.
							: reqStepAtDest.Position); //If null, select the next higest.
						if (startingPosition < endingPosition &&
							destRequirementStepId.HasValue)
							endingPosition--; //This is needed since the item goes in FRONT of the one selected, unless it's at the end.

						//This is needed for or WHERE clause.
						int lowPosition = Math.Min(startingPosition, endingPosition);
						int highPosition = Math.Max(startingPosition, endingPosition);

						//Check that the two items are not the same. If they are, nothing's changing.
						if (startingPosition == endingPosition || (reqStepAtDest != null && reqStepToMove.RequirementStepId == reqStepAtDest.RequirementStepId))
							return;

						//Get the HistoryManager ifneeded.
						HistoryManager hMgr = null;
						bool recordHistory = new ProjectSettings(baseRequirement.ProjectId).BaseliningEnabled && Global.Feature_Baselines;
						if (recordHistory)
							hMgr = new HistoryManager();

						//The flag, in case we need to save.
						bool saveChanges = false;

						//Pull all the Test Steps that are between (inclusive) of our range.
						//  Steps outside of our ranger aren't moving, see.
						ct.ContextOptions.LazyLoadingEnabled = false; //Turn this off so the query does not change as WE make changes.
						var reqStepList = ct.RequirementSteps
							.Include(s => s.Requirement)
							.OrderBy(s => s.Position)
							.Where(s =>
								s.RequirementId == requirementId &&
								s.Position >= lowPosition &&
								s.Position <= highPosition)
							.ToList();
						ct.ContextOptions.LazyLoadingEnabled = true; //Re-enable, in case.

						//Loop through each one. 
						bool doIncrement = true; //Flag. Until we get to the new position, we INCREMENT the existing ones.
						long? recordHistoyChangeset = null; //In case we record history, we want to record this changeset.
						foreach (var step in reqStepList)
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
								recordHistoyChangeset = hMgr.RecordReqStepPosition(
									baseRequirement.ProjectId,
									userId,
									step.RequirementId,
									step.Requirement.Name,
									step.RequirementStepId,
									oldPos,
									step.Position,
									recordHistoyChangeset);
						}

						//Update the Requirement date, and save all oour changes.
						if (saveChanges)
						{
							reqStepList[0].Requirement.StartTracking();
							reqStepList[0].Requirement.LastUpdateDate = DateTime.UtcNow;
							ct.SaveChanges();
						}

						//Commit the transaction
						transactionScope.Complete();
					}
				}

				Logger.LogExitingEvent(METHOD_NAME);
			}
			catch (EntityException exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>
		/// Deletes a single requirement step in the system
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="userId">The id of the user doing the delete</param>
		/// <param name="requirementStepId">The id of the step being deleted</param>
		/// <remarks>Soft-deletes the step only. Fails quietly if the step cannot be found</remarks>
		public void DeleteStep(int projectId, int requirementStepId, int userId)
		{
			const string METHOD_NAME = "DeleteStep";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the step and mark as deleted
					var query = from r in context.RequirementSteps
								where r.RequirementStepId == requirementStepId
								select r;

					RequirementStep requirementStep = query.FirstOrDefault();
					if (requirementStep != null)
					{
						//Mark as deleted and persist
						requirementStep.StartTracking();
						requirementStep.IsDeleted = true;

						//Finally we need to update the last updated date of the requirement itself
						UpdateRequirementLastUpdateDate(context, requirementStep.RequirementId);
						context.SaveChanges();
					}
				}

				//Now make a log history.
				new HistoryManager().LogDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.RequirementStep, requirementStepId, DateTime.UtcNow);

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
		/// Undeletes a single requirement step in the system
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="rollbackId">Pass a rollback id to update history during a rollback operation</param>
		/// <param name="userId">The id of the user making the change</param>
		/// <param name="requirementStepId">The id of the step being undeleted</param>
		/// <remarks>Throws an exception if the step cannot be found</remarks>
		public void UndeleteStep(int projectId, int requirementStepId, int userId, long? rollbackId)
		{
			const string METHOD_NAME = "UndeleteStep";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the step and mark as deleted
					var query = from r in context.RequirementSteps
								where r.RequirementStepId == requirementStepId
								select r;

					RequirementStep requirementStep = query.FirstOrDefault();
					if (requirementStep == null)
					{
						throw new ArtifactNotExistsException(String.Format(GlobalResources.Messages.Requirement_StepNotExists, requirementStepId));
					}
					else
					{
						//Mark as deleted and persist
						requirementStep.StartTracking();
						requirementStep.IsDeleted = false;

						//Finally we need to update the last updated date of the requirement itself
						UpdateRequirementLastUpdateDate(context, requirementStep.RequirementId);
						context.SaveChanges();
					}
				}

				//Now make a log history.
				if (rollbackId.HasValue)
					new HistoryManager().LogUnDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.RequirementStep, requirementStepId, rollbackId.Value, DateTime.UtcNow);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}
		public void UndeletRequirement(int projectId, int id)
		{
			const string METHOD_NAME = "UndeletRequirement";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the step and mark as deleted
					var query = from r in context.Requirements
								where r.RequirementId ==id 
								select r;

					Requirement requirement = query.FirstOrDefault();
					if (requirement == null)
					{
						var incidentq = from r in context.Incidents
									where r.IncidentId == id
									select r;
						//throw new ArtifactNotExistsException(String.Format("requirement Not exist.Please check", requirement));
						Incident incidentdata = incidentq.FirstOrDefault();
						if(incidentdata!= null)
						{
							incidentdata.StartTracking();
							incidentdata.IsDeleted = true
								;

							//Finally we need to update the last updated date of the requirement itself
							UpdateRequirementLastUpdateDate(context, incidentdata.IncidentId);
							context.SaveChanges();
						}
						else
							throw new ArtifactNotExistsException(String.Format("requirement or incident  Not exist.Please check"));
					}
					else
					{
						//Mark as deleted and persist
						requirement.StartTracking();
						requirement.IsDeleted = true
							;

						//Finally we need to update the last updated date of the requirement itself
						UpdateRequirementLastUpdateDate(context, requirement.RequirementId);
						context.SaveChanges();
					}
				}

				//Now make a log history.
				//if (rollbackId.HasValue)
				//	new HistoryManager().LogUnDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.RequirementStep, requirementid, rollbackId.Value, DateTime.UtcNow);

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

		#region Private and Protected Methods

		/// <summary>Copies the test coverage information for a specific requirement</summary>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The current project</param>
		/// <param name="sourceRequirementId">The requirement we're copying coverage FROM</param>
		/// <param name="destRequirementId">The requirement we're copying coverage TO</param>
		protected void CopyCoverage(int userId, int projectId, int sourceRequirementId, int destRequirementId)
		{
			//Instantiate a Test Case business object
			Business.TestCaseManager testCaseManager = new Business.TestCaseManager();

			//Now we need to copy across any coverage information
			List<TestCase> sourceTestCases = testCaseManager.RetrieveCoveredByRequirementId(projectId, sourceRequirementId);
			List<int> testCaseIds = new List<int>();
			foreach (TestCase testCaseRow in sourceTestCases)
			{
				//Add the row to the list
				testCaseIds.Add(testCaseRow.TestCaseId);
			}
			testCaseManager.AddToRequirement(projectId, destRequirementId, testCaseIds, userId);
		}

		/// <summary>Associates requirements with an iteration/release</summary>
		/// <param name="requirementIds">The requirements being associated</param>
		/// <param name="releaseId">The id of the iteration/release being associated with</param>
		/// <param name="userId">The ID of the user making the change</param>
		public void AssociateToIteration(List<int> requirementIds, int releaseId, int userId)
		{
			const string METHOD_NAME = "AssociateToIteration";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Make sure the release exists
				ReleaseManager releaseManager = new ReleaseManager();
				try
				{
					releaseManager.RetrieveById2(null, releaseId);
				}
				catch (ArtifactNotExistsException)
				{
					throw new ApplicationException(GlobalResources.Messages.ReleaseManager_ReleaseIterationNotExistsForAssociation);
				}

				//Retrieve the requirements
				int? projectId = null;
				List<Requirement> requirements;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//We need to initially retrieve the requirements, making sure not already associated
					var query = from r in context.Requirements
								where requirementIds.Contains(r.RequirementId) &&
										(r.ReleaseId != releaseId || !r.ReleaseId.HasValue) &&
										!r.IsDeleted
								select r;

					//Get the requirements
					requirements = query.ToList();
					foreach (Requirement requirement in requirements)
					{
						//Store the project for use later
						projectId = requirement.ProjectId;

						//Update the release id
						requirement.StartTracking();
						requirement.ReleaseId = releaseId;
						requirement.LastUpdateDate = DateTime.UtcNow;
						requirement.ConcurrencyDate = DateTime.UtcNow;
					}
				}
				//Update the batch
				if (projectId.HasValue)
				{
					Update(userId, projectId.Value, requirements);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

		}

		/// <summary>Assigns a requirement to a user</summary>
		/// <param name="requirementId">The requirement being associated</param>
		/// <param name="ownerId">The id of the user it's being assigned to (or null to deassign)</param>
		/// <param name="changerId">The ID of the user making the change</param>
		public void AssignToUser(int requirementId, int? ownerId, int changerId)
		{
			const string METHOD_NAME = "AssignToUser";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Retrieve the requirement - if we get an artifact not exists exception let the calling function handle
				RequirementView requirementView = RetrieveById2(null, requirementId);

				//Update the owner id
				Requirement requirement = requirementView.ConvertTo<RequirementView, Requirement>();
				requirement.StartTracking();
				requirement.OwnerId = ownerId;
				requirement.LastUpdateDate = DateTime.UtcNow;
				requirement.ConcurrencyDate = DateTime.UtcNow;
				Update(changerId, requirementView.ProjectId, new List<Requirement>() { requirement });
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>De-associates requirements from its currently assigned release</summary>
		/// <param name="requirementIds">The ID of the requirements to be de-associated</param>
		/// <param name="userId">The id of the user making the change</param>
		public void RemoveReleaseAssociation(int userId, List<int> requirementIds)
		{
			const string METHOD_NAME = "RemoveReleaseAssociation";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int? projectId = null;
				bool? isReqStatusAutoPlanned = null;
				List<Requirement> requirements;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//We need to initially retrieve the task to see if it's linked to a release
					var query = from r in context.Requirements
								where requirementIds.Contains(r.RequirementId) && r.ReleaseId.HasValue && !r.IsDeleted
								select r;

					//Get the requirements
					requirements = query.ToList();
					foreach (Requirement requirement in requirements)
					{
						//Store the project for use later
						projectId = requirement.ProjectId;

						//We need to retrieve the project planning settings
						if (!isReqStatusAutoPlanned.HasValue)
						{
							Project project = new ProjectManager().RetrieveById(projectId.Value);
							isReqStatusAutoPlanned = project.IsReqStatusAutoPlanned;
						}

						//Update the release id and status
						requirement.StartTracking();
						requirement.ReleaseId = null;
						requirement.LastUpdateDate = DateTime.UtcNow;
						requirement.ConcurrencyDate = DateTime.UtcNow;
						if (isReqStatusAutoPlanned.Value && requirement.RequirementStatusId == (int)Requirement.RequirementStatusEnum.Planned)
						{
							requirement.RequirementStatusId = (int)Requirement.RequirementStatusEnum.Accepted;
						}
					}
				}

				//Update the batch
				if (projectId.HasValue)
				{
					Update(userId, projectId.Value, requirements);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// This method takes the hashtable of filters and returns the SQL where clause that can be used by the retrieve stored procedures
		/// </summary>
		/// <param name="filters">The hashtable of filters (property,value)</param>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <returns>The SQL WHERE clause for the filters</returns>
		/// <remarks>Use an arraylist as the property value when you want a multiple set of values filtered against</remarks>
		protected string CreateFiltersClause(int projectId, int projectTemplateId, Hashtable filters, double utcOffset)
		{
			string filtersClause = "";
			if (filters != null)
			{
				IDictionaryEnumerator enumerator = filters.GetEnumerator();
				while (enumerator.MoveNext())
				{
					string filterProperty = enumerator.Key.ToString();
					object filterValue = enumerator.Value;
					string filterColumn = "";
					EntityProperty filterPropertyInfo = null;

					//Handle the special case of coverage, since it doesn't map to a single column
					if (filterProperty.ToLower() == "coverageid")
					{
						switch ((int)filterValue)
						{
							//Not Covered
							case 1:
								filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL = 0 ";
								break;

							//Run Filters
							case 2:
								filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND (REQ.COVERAGE_COUNT_PASSED + REQ.COVERAGE_COUNT_FAILED + REQ.COVERAGE_COUNT_CAUTION + REQ.COVERAGE_COUNT_BLOCKED) = 0 ";
								break;
							case 3:
								filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND ((REQ.COVERAGE_COUNT_PASSED + REQ.COVERAGE_COUNT_FAILED + REQ.COVERAGE_COUNT_CAUTION + REQ.COVERAGE_COUNT_BLOCKED) * 2) <= REQ.COVERAGE_COUNT_TOTAL ";
								break;
							case 4:
								filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND (REQ.COVERAGE_COUNT_PASSED + REQ.COVERAGE_COUNT_FAILED + REQ.COVERAGE_COUNT_CAUTION + REQ.COVERAGE_COUNT_BLOCKED) < REQ.COVERAGE_COUNT_TOTAL ";
								break;

							//Failed Filters
							case 5:
								filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND REQ.COVERAGE_COUNT_FAILED > 0 ";
								break;
							case 6:
								filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND (REQ.COVERAGE_COUNT_FAILED * 2) >= REQ.COVERAGE_COUNT_TOTAL ";
								break;
							case 7:
								filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND REQ.COVERAGE_COUNT_FAILED = REQ.COVERAGE_COUNT_TOTAL ";
								break;

							//Caution Filters
							case 8:
								filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND REQ.COVERAGE_COUNT_CAUTION > 0 ";
								break;
							case 9:
								filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND (REQ.COVERAGE_COUNT_CAUTION * 2) >= REQ.COVERAGE_COUNT_TOTAL ";
								break;
							case 10:
								filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND REQ.COVERAGE_COUNT_CAUTION = REQ.COVERAGE_COUNT_TOTAL ";
								break;

							//Blocked Filters
							case 11:
								filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND REQ.COVERAGE_COUNT_BLOCKED > 0 ";
								break;
							case 12:
								filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND (REQ.COVERAGE_COUNT_BLOCKED * 2) >= REQ.COVERAGE_COUNT_TOTAL ";
								break;
							case 13:
								filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND REQ.COVERAGE_COUNT_BLOCKED = REQ.COVERAGE_COUNT_TOTAL ";
								break;

							//Passed Filters
							case 14:
								// = 0% Passed
								filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND REQ.COVERAGE_COUNT_PASSED = 0 ";
								break;
							case 15:
								// > 0% Passed
								filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND REQ.COVERAGE_COUNT_PASSED > 0 ";
								break;
							case 16:
								// <= 50% Passed
								filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND (REQ.COVERAGE_COUNT_PASSED * 2) <= REQ.COVERAGE_COUNT_TOTAL ";
								break;
							case 17:
								// <  100% Passed
								filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND REQ.COVERAGE_COUNT_PASSED < REQ.COVERAGE_COUNT_TOTAL ";
								break;
							case 18:
								// =  100% Passed
								filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND REQ.COVERAGE_COUNT_PASSED = REQ.COVERAGE_COUNT_TOTAL ";
								break;
						}
					}
					else if (filterProperty.ToLower() == "progressid")
					{
						//Handle the special case of task progress, since it doesn't map to a single column
						switch ((int)filterValue)
						{
							//Not Started
							case 1:
								filtersClause += "AND REQ.TASK_PERCENT_NOT_START > 0 ";
								break;
							//Starting Late
							case 2:
								filtersClause += "AND REQ.TASK_PERCENT_LATE_START > 0 ";
								break;
							//On Schedule
							case 3:
								filtersClause += "AND REQ.TASK_PERCENT_LATE_START = 0 AND REQ.TASK_PERCENT_LATE_FINISH = 0 AND REQ.TASK_PERCENT_ON_TIME > 0 AND REQ.TASK_PERCENT_ON_TIME < 100 ";
								break;
							//Running Late
							case 4:
								filtersClause += "AND REQ.TASK_PERCENT_LATE_FINISH > 0 ";
								break;
							//Completed
							case 5:
								filtersClause += "AND REQ.TASK_PERCENT_ON_TIME = 100 ";
								break;
						}
					}
					//Handle the special case of release filters where we want to also retrieve child iterations/minor releases
					else if (filterProperty == "ReleaseId" && filterValue is Int32 && (int)filterValue != NoneFilterValue)
					{
						ReleaseManager releaseManager = new ReleaseManager();
						int releaseId = (int)filterValue;
						string releaseList = releaseManager.GetSelfAndChildRollupChildrenList(projectId, releaseId);
						filtersClause += "AND REQ.RELEASE_ID IN (" + releaseList + ") ";
					}
					else
					{
						//Now we need to convert the entity property name to the database column name and get the data-type
						try
						{
							string mappedColumn = GetPropertyMapping(typeof(RequirementView), filterProperty);
							if (!String.IsNullOrEmpty(mappedColumn))
							{
								filterColumn = mappedColumn;
							}
							EntityProperty entityPropertyInfo = GetPropertyInfo(typeof(RequirementView), filterProperty);
							if (entityPropertyInfo != null)
							{
								filterPropertyInfo = entityPropertyInfo;
							}
						}
						catch (IndexOutOfRangeException) { }

						//If we are filtering by RequirementId, check to see if we're actually passed a summary requirement
						//If so, we need to get the list of child requirements and add those as a list of requirement ids
						if (filterProperty == "RequirementId")
						{
							if (filterValue is Int32)
							{
								string requirementIdList = GetSelfAndChildren(projectId, (int)filterValue);
								MultiValueFilter mvf;
								if (String.IsNullOrEmpty(requirementIdList) || requirementIdList == "-1")
								{
									mvf = new MultiValueFilter();
									mvf.IsNone = true;
									filterValue = mvf;
								}
								else
								{
									if (MultiValueFilter.TryParse(requirementIdList, out mvf))
									{
										filterValue = mvf;
									}
								}
							}
						}

						//Add the generic filters
						filtersClause += CreateFilterClauseGeneric(projectId, projectTemplateId, filterProperty, filterValue, filterColumn, "REQ", filterPropertyInfo, Artifact.ArtifactTypeEnum.Requirement, utcOffset);
					}
				}
			}
			return filtersClause;
		}

		/// <summary>Handles any Requirement specific filters that are not generic</summary>
		/// <param name="expressionList">The existing list of expressions</param>
		/// <param name="filter">The current filter</param>
		/// <param name="projectId">The current project</param>
		/// <param name="projectTemplateId">The current project template</param>
		/// <param name="p">The LINQ parameter</param>
		/// <param name="utcOffset">The current offset from UTC</param>
		/// <returns>True if handled, return False for the standard filter handling</returns>
		protected internal bool HandleRequirementSpecificFilters(int? projectId, int? projectTemplateId, ParameterExpression p, List<Expression> expressionList, KeyValuePair<string, object> filter, double utcOffset)
		{
			//By default, let the generic filter convertor handle the filter
			string filterProperty = filter.Key;
			object filterValue = filter.Value;

			//Handle the special case of progress, since it doesn't map to a single column
			if (filterProperty == "ProgressId")
			{
				if (filterValue is Int32)
				{
					switch ((int)filterValue)
					{
						//Not Started
						case 1:
							{
								//AND REQ.TASK_PERCENT_NOT_START > 0
								MemberExpression taskPercentExp = LambdaExpression.PropertyOrField(p, "TaskPercentNotStart");
								Expression expression = Expression.GreaterThan(taskPercentExp, LambdaExpression.Constant(0));
								expressionList.Add(expression);
								break;
							}
						//Starting Late
						case 2:
							{
								//AND REQ.TASK_PERCENT_LATE_START > 0
								MemberExpression taskPercentExp = LambdaExpression.PropertyOrField(p, "TaskPercentLateStart");
								Expression expression = Expression.GreaterThan(taskPercentExp, LambdaExpression.Constant(0));
								expressionList.Add(expression);
								break;
							}

						//On Schedule
						case 3:
							{
								//AND REQ.TASK_PERCENT_LATE_START = 0 AND REQ.TASK_PERCENT_LATE_FINISH = 0 AND REQ.TASK_PERCENT_ON_TIME > 0 AND REQ.TASK_PERCENT_ON_TIME < 100 
								MemberExpression taskPercentLateStartExp = LambdaExpression.PropertyOrField(p, "TaskPercentLateStart");
								MemberExpression taskPercentLateFinishExp = LambdaExpression.PropertyOrField(p, "TaskPercentLateFinish");
								MemberExpression taskPercentOnTimeExp = LambdaExpression.PropertyOrField(p, "TaskPercentOnTime");
								Expression expression1 = Expression.Equal(taskPercentLateStartExp, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								Expression expression2 = Expression.Equal(taskPercentLateFinishExp, LambdaExpression.Constant(0));
								expressionList.Add(expression2);
								Expression expression3 = Expression.GreaterThan(taskPercentOnTimeExp, LambdaExpression.Constant(0));
								expressionList.Add(expression3);
								Expression expression4 = Expression.LessThan(taskPercentOnTimeExp, LambdaExpression.Constant(100));
								expressionList.Add(expression4);
								break;
							}

						//Late Finishing
						case 4:
							{
								//AND REQ.TASK_PERCENT_LATE_FINISH > 0
								MemberExpression taskPercentExp = LambdaExpression.PropertyOrField(p, "TaskPercentLateFinish");
								Expression expression = Expression.GreaterThan(taskPercentExp, LambdaExpression.Constant(0));
								expressionList.Add(expression);
								break;
							}

						//Completed
						case 5:
							{
								//AND REQ.TASK_PERCENT_ON_TIME = 100
								MemberExpression taskPercentExp = LambdaExpression.PropertyOrField(p, "TaskPercentOnTime");
								Expression expression = Expression.Equal(taskPercentExp, LambdaExpression.Constant(100));
								expressionList.Add(expression);
							}
							break;
					}
				}
				return true;
			}

			//Handle the special case of test coverage, since it doesn't map to a single column
			if (filterProperty == "CoverageId")
			{
				if (filterValue is Int32)
				{
					switch ((int)filterValue)
					{
						//Not Covered
						case 1:
							{
								//filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL = 0 ";
								MemberExpression coverageCountTotal = LambdaExpression.PropertyOrField(p, "CoverageCountTotal");
								Expression expression = Expression.Equal(coverageCountTotal, LambdaExpression.Constant(0));
								expressionList.Add(expression);
							}
							break;

						//Run Filters
						case 2:
							{
								//filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND (REQ.COVERAGE_COUNT_PASSED + REQ.COVERAGE_COUNT_FAILED + REQ.COVERAGE_COUNT_CAUTION + REQ.COVERAGE_COUNT_BLOCKED) = 0 ";
								MemberExpression coverageCountTotal = LambdaExpression.PropertyOrField(p, "CoverageCountTotal");
								Expression expression1 = Expression.GreaterThan(coverageCountTotal, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								MemberExpression coverageCountPassed = LambdaExpression.PropertyOrField(p, "CoverageCountPassed");
								MemberExpression coverageCountFailed = LambdaExpression.PropertyOrField(p, "CoverageCountFailed");
								MemberExpression coverageCountCaution = LambdaExpression.PropertyOrField(p, "CoverageCountCaution");
								MemberExpression coverageCountBlocked = LambdaExpression.PropertyOrField(p, "CoverageCountBlocked");
								Expression additionExp = Expression.Add(coverageCountPassed, coverageCountFailed);
								additionExp = Expression.Add(additionExp, coverageCountCaution);
								additionExp = Expression.Add(additionExp, coverageCountBlocked);
								Expression expression2 = Expression.Equal(additionExp, LambdaExpression.Constant(0));
								expressionList.Add(expression2);
							}
							break;
						case 3:
							{
								//filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND ((REQ.COVERAGE_COUNT_PASSED + REQ.COVERAGE_COUNT_FAILED + REQ.COVERAGE_COUNT_CAUTION + REQ.COVERAGE_COUNT_BLOCKED) * 2) <= REQ.COVERAGE_COUNT_TOTAL ";
								MemberExpression coverageCountTotal = LambdaExpression.PropertyOrField(p, "CoverageCountTotal");
								Expression expression1 = Expression.GreaterThan(coverageCountTotal, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								MemberExpression coverageCountPassed = LambdaExpression.PropertyOrField(p, "CoverageCountPassed");
								MemberExpression coverageCountFailed = LambdaExpression.PropertyOrField(p, "CoverageCountFailed");
								MemberExpression coverageCountCaution = LambdaExpression.PropertyOrField(p, "CoverageCountCaution");
								MemberExpression coverageCountBlocked = LambdaExpression.PropertyOrField(p, "CoverageCountBlocked");
								Expression additionExp = Expression.Add(coverageCountPassed, coverageCountFailed);
								additionExp = Expression.Add(additionExp, coverageCountCaution);
								additionExp = Expression.Add(additionExp, coverageCountBlocked);
								additionExp = Expression.Multiply(additionExp, LambdaExpression.Constant(2));
								Expression expression2 = Expression.LessThanOrEqual(additionExp, coverageCountTotal);
								expressionList.Add(expression2);
							}
							break;
						case 4:
							{
								//filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND (REQ.COVERAGE_COUNT_PASSED + REQ.COVERAGE_COUNT_FAILED + REQ.COVERAGE_COUNT_CAUTION + REQ.COVERAGE_COUNT_BLOCKED) < REQ.COVERAGE_COUNT_TOTAL ";
								MemberExpression coverageCountTotal = LambdaExpression.PropertyOrField(p, "CoverageCountTotal");
								Expression expression1 = Expression.GreaterThan(coverageCountTotal, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								MemberExpression coverageCountPassed = LambdaExpression.PropertyOrField(p, "CoverageCountPassed");
								MemberExpression coverageCountFailed = LambdaExpression.PropertyOrField(p, "CoverageCountFailed");
								MemberExpression coverageCountCaution = LambdaExpression.PropertyOrField(p, "CoverageCountCaution");
								MemberExpression coverageCountBlocked = LambdaExpression.PropertyOrField(p, "CoverageCountBlocked");
								Expression additionExp = Expression.Add(coverageCountPassed, coverageCountFailed);
								additionExp = Expression.Add(additionExp, coverageCountCaution);
								additionExp = Expression.Add(additionExp, coverageCountBlocked);
								Expression expression2 = Expression.LessThan(additionExp, coverageCountTotal);
								expressionList.Add(expression2);
							}
							break;

						//Failed Filters
						case 5:
							{
								//filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND REQ.COVERAGE_COUNT_FAILED > 0 ";
								MemberExpression coverageCountTotal = LambdaExpression.PropertyOrField(p, "CoverageCountTotal");
								Expression expression1 = Expression.GreaterThan(coverageCountTotal, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								MemberExpression coverageCountFailed = LambdaExpression.PropertyOrField(p, "CoverageCountFailed");
								Expression expression2 = Expression.GreaterThan(coverageCountFailed, LambdaExpression.Constant(0));
								expressionList.Add(expression2);
							}
							break;
						case 6:
							{
								//filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND (REQ.COVERAGE_COUNT_FAILED * 2) >= REQ.COVERAGE_COUNT_TOTAL ";
								MemberExpression coverageCountTotal = LambdaExpression.PropertyOrField(p, "CoverageCountTotal");
								Expression expression1 = Expression.GreaterThan(coverageCountTotal, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								MemberExpression coverageCountFailed = LambdaExpression.PropertyOrField(p, "CoverageCountFailed");
								Expression coverageCountFailedX2 = Expression.Multiply(coverageCountFailed, LambdaExpression.Constant(2));
								Expression expression2 = Expression.GreaterThanOrEqual(coverageCountFailedX2, coverageCountTotal);
								expressionList.Add(expression2);
							}
							break;
						case 7:
							{
								//filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND REQ.COVERAGE_COUNT_FAILED = REQ.COVERAGE_COUNT_TOTAL ";
								MemberExpression coverageCountTotal = LambdaExpression.PropertyOrField(p, "CoverageCountTotal");
								Expression expression1 = Expression.GreaterThan(coverageCountTotal, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								MemberExpression coverageCountFailed = LambdaExpression.PropertyOrField(p, "CoverageCountFailed");
								Expression expression2 = Expression.Equal(coverageCountFailed, coverageCountTotal);
								expressionList.Add(expression2);
							}
							break;

						//Caution Filters
						case 8:
							{
								//filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND REQ.COVERAGE_COUNT_CAUTION > 0 ";
								MemberExpression coverageCountTotal = LambdaExpression.PropertyOrField(p, "CoverageCountTotal");
								Expression expression1 = Expression.GreaterThan(coverageCountTotal, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								MemberExpression coverageCountCaution = LambdaExpression.PropertyOrField(p, "CoverageCountCaution");
								Expression expression2 = Expression.GreaterThan(coverageCountCaution, LambdaExpression.Constant(0));
								expressionList.Add(expression2);
							}
							break;
						case 9:
							{
								//filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND (REQ.COVERAGE_COUNT_CAUTION * 2) >= REQ.COVERAGE_COUNT_TOTAL ";
								MemberExpression coverageCountTotal = LambdaExpression.PropertyOrField(p, "CoverageCountTotal");
								Expression expression1 = Expression.GreaterThan(coverageCountTotal, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								MemberExpression coverageCountCaution = LambdaExpression.PropertyOrField(p, "CoverageCountCaution");
								Expression coverageCountCautionX2 = Expression.Multiply(coverageCountCaution, LambdaExpression.Constant(2));
								Expression expression2 = Expression.GreaterThanOrEqual(coverageCountCautionX2, coverageCountTotal);
								expressionList.Add(expression2);
							}
							break;
						case 10:
							{
								//filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND REQ.COVERAGE_COUNT_CAUTION = REQ.COVERAGE_COUNT_TOTAL ";
								MemberExpression coverageCountTotal = LambdaExpression.PropertyOrField(p, "CoverageCountTotal");
								Expression expression1 = Expression.GreaterThan(coverageCountTotal, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								MemberExpression coverageCountCaution = LambdaExpression.PropertyOrField(p, "CoverageCountCaution");
								Expression expression2 = Expression.Equal(coverageCountCaution, coverageCountTotal);
								expressionList.Add(expression2);
							}
							break;

						//Blocked Filters
						case 11:
							{
								//filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND REQ.COVERAGE_COUNT_BLOCKED > 0 ";
								MemberExpression coverageCountTotal = LambdaExpression.PropertyOrField(p, "CoverageCountTotal");
								Expression expression1 = Expression.GreaterThan(coverageCountTotal, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								MemberExpression coverageCountBlocked = LambdaExpression.PropertyOrField(p, "CoverageCountBlocked");
								Expression expression2 = Expression.GreaterThan(coverageCountBlocked, LambdaExpression.Constant(0));
								expressionList.Add(expression2);
							}
							break;
						case 12:
							{
								//filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND (REQ.COVERAGE_COUNT_BLOCKED * 2) >= REQ.COVERAGE_COUNT_TOTAL ";
								MemberExpression coverageCountTotal = LambdaExpression.PropertyOrField(p, "CoverageCountTotal");
								Expression expression1 = Expression.GreaterThan(coverageCountTotal, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								MemberExpression coverageCountBlocked = LambdaExpression.PropertyOrField(p, "CoverageCountBlocked");
								Expression coverageCountBlockedX2 = Expression.Multiply(coverageCountBlocked, LambdaExpression.Constant(2));
								Expression expression2 = Expression.GreaterThanOrEqual(coverageCountBlockedX2, coverageCountTotal);
								expressionList.Add(expression2);
							}
							break;
						case 13:
							{
								//filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND REQ.COVERAGE_COUNT_BLOCKED = REQ.COVERAGE_COUNT_TOTAL ";
								MemberExpression coverageCountTotal = LambdaExpression.PropertyOrField(p, "CoverageCountTotal");
								Expression expression1 = Expression.GreaterThan(coverageCountTotal, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								MemberExpression coverageCountBlocked = LambdaExpression.PropertyOrField(p, "CoverageCountBlocked");
								Expression expression2 = Expression.Equal(coverageCountBlocked, coverageCountTotal);
								expressionList.Add(expression2);
							}
							break;

						//Passed Filters
						case 14:
							{
								// = 0% Passed
								//filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND REQ.COVERAGE_COUNT_PASSED = 0 ";
								MemberExpression coverageCountTotal = LambdaExpression.PropertyOrField(p, "CoverageCountTotal");
								Expression expression1 = Expression.GreaterThan(coverageCountTotal, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								MemberExpression coverageCountPassed = LambdaExpression.PropertyOrField(p, "CoverageCountPassed");
								Expression expression2 = Expression.Equal(coverageCountPassed, LambdaExpression.Constant(0));
								expressionList.Add(expression2);
							}
							break;
						case 15:
							{
								// > 0% Passed
								//filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND REQ.COVERAGE_COUNT_PASSED > 0 ";
								MemberExpression coverageCountTotal = LambdaExpression.PropertyOrField(p, "CoverageCountTotal");
								Expression expression1 = Expression.GreaterThan(coverageCountTotal, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								MemberExpression coverageCountPassed = LambdaExpression.PropertyOrField(p, "CoverageCountPassed");
								Expression expression2 = Expression.GreaterThan(coverageCountPassed, LambdaExpression.Constant(0));
								expressionList.Add(expression2);
							}
							break;
						case 16:
							{
								// <= 50% Passed
								//filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND (REQ.COVERAGE_COUNT_PASSED * 2) <= REQ.COVERAGE_COUNT_TOTAL ";
								MemberExpression coverageCountTotal = LambdaExpression.PropertyOrField(p, "CoverageCountTotal");
								Expression expression1 = Expression.GreaterThan(coverageCountTotal, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								MemberExpression coverageCountPassed = LambdaExpression.PropertyOrField(p, "CoverageCountPassed");
								Expression coverageCountPassedX2 = Expression.Multiply(coverageCountPassed, LambdaExpression.Constant(2));
								Expression expression2 = Expression.LessThanOrEqual(coverageCountPassedX2, coverageCountTotal);
								expressionList.Add(expression2);
							}
							break;
						case 17:
							{
								// <  100% Passed
								//filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND REQ.COVERAGE_COUNT_PASSED < REQ.COVERAGE_COUNT_TOTAL ";
								MemberExpression coverageCountTotal = LambdaExpression.PropertyOrField(p, "CoverageCountTotal");
								Expression expression1 = Expression.GreaterThan(coverageCountTotal, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								MemberExpression coverageCountPassed = LambdaExpression.PropertyOrField(p, "CoverageCountPassed");
								Expression expression2 = Expression.LessThan(coverageCountPassed, coverageCountTotal);
								expressionList.Add(expression2);
							}
							break;
						case 18:
							{
								// =  100% Passed
								//filtersClause += "AND REQ.COVERAGE_COUNT_TOTAL > 0 AND REQ.COVERAGE_COUNT_PASSED = REQ.COVERAGE_COUNT_TOTAL ";
								MemberExpression coverageCountTotal = LambdaExpression.PropertyOrField(p, "CoverageCountTotal");
								Expression expression1 = Expression.GreaterThan(coverageCountTotal, LambdaExpression.Constant(0));
								expressionList.Add(expression1);
								MemberExpression coverageCountPassed = LambdaExpression.PropertyOrField(p, "CoverageCountPassed");
								Expression expression2 = Expression.Equal(coverageCountPassed, coverageCountTotal);
								expressionList.Add(expression2);
							}
							break;
					}
				}
			}

			//Handle the special case of release filters where we want to also retrieve child minor releases and iterations
			if (filterProperty == "ReleaseId" && (int)filterValue != NoneFilterValue && projectId.HasValue)
			{
				//Get the release and its child iterations
				int releaseId = (int)filterValue;
				List<int> releaseIds = new ReleaseManager().GetSelfAndChildRollupChildren(projectId.Value, releaseId);
				ConstantExpression releaseIdsExpression = LambdaExpression.Constant(releaseIds);
				MemberExpression memberExpression = LambdaExpression.PropertyOrField(p, "ReleaseId");
				//Equivalent to: p => releaseIds.Contains(p.ReleaseId) i.e. (RELEASE_ID IN (1,2,3))
				Expression releaseExpression = Expression.Call(releaseIdsExpression, "Contains", null, Expression.Convert(memberExpression, typeof(int)));
				expressionList.Add(releaseExpression);
				return true;
			}

			//By default, let the generic filter convertor handle the filter
			return false;
		}

		/// <summary>This function reorders a section of the requirement tree before an insert operation so that there is space in the requirements indent-level scheme for the new item</summary>
		/// <param name="indentLevel">The indent level that we want to reorder from</param>
		/// <param name="projectId">The project we're interested in</param>
		private void ReorderRequirementsBeforeInsert(int projectId, string indentLevel)
		{
			const string METHOD_NAME = "ReorderRequirementsBeforeInsert()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Call the stored procedure to do the reorder
					context.Requirement_ReorderRequirementsBeforeInsert(User.UserInternal, projectId, indentLevel);
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

		/// <summary>This function reorders a section of the requirement tree after a delete operation. It syncs up the 'indent' level string with the actual normalized data</summary>
		/// <param name="indentLevel">The indent level that we want to reorder from</param>
		/// <param name="projectId">The project we're interested in</param>
		private void ReorderRequirementsAfterDelete(int projectId, string indentLevel)
		{
			const string METHOD_NAME = "ReorderRequirementsAfterDelete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Call the stored procedure to do the reorder
					context.Requirement_ReorderRequirementsAfterDelete(User.UserInternal, projectId, indentLevel);
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

		/// <summary>Updates a list of requirements that are passed-in. This updates the positional data only</summary>
		/// <param name="userId">The user we're viewing the requirements as (if we pass the internal user then only changes to the Requirement table are performed)</param>
		/// <param name="requirementDataSet">The dataset to be persisted</param>
		protected internal void UpdatePositionalData(int userId, List<RequirementView> requirements)
		{
			const string METHOD_NAME = "UpdatePositionalData()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Iterate through the various requirements
					foreach (RequirementView requirement in requirements)
					{
						//Call the stored procedure that makes the updates
						context.Requirement_UpdatePositional(requirement.RequirementId, (userId == User.UserInternal) ? null : (int?)userId, requirement.IsExpanded, requirement.IsVisible, requirement.IsSummary, requirement.IndentLevel);
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

		/// <summary>Retrieves the parent requirement of the requirement who's indent level is passed in</summary>
		/// <param name="indentLevel">The indent-level of the requirement who's parent is to be returned</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="includeDeleted">Whether to include deleted items or not. Default:FALSE</param>
		/// <returns>Requirement entity</returns>
		private RequirementView RetrieveParent(int userId, int projectId, string indentLevel, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveParent()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the parent requirement by removing the last element of the indent level string
				string parentIndentLevel = indentLevel.Substring(0, indentLevel.Length - 3);

				//Create custom SQL WHERE clause for retrieving the content item and execute
				RequirementView requirement = Retrieve(userId, projectId, "REQ.INDENT_LEVEL = '" + parentIndentLevel + "'", includeDeleted).FirstOrDefault();

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirement;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the parent requirements of the requirement who's indent level is passed in</summary>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="includeDeleted">Whether or not to include deleted items or not. Default:FALSE</param>
		/// <param name="indentLevel">The indent-level of the requirement who's parents are to be returned</param>
		/// <returns>Requirement list</returns>
		public List<RequirementView> RetrieveParents(int userId, int projectId, string indentLevel, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveParents()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Store the length of the passed in indent level
				int length = indentLevel.Length;

				//Create custom SQL WHERE clause for retrieving the requirements and execute
				List<RequirementView> requirements = Retrieve(userId, projectId, "SUBSTRING('" + indentLevel + "', 1, LEN(REQ.INDENT_LEVEL)) = REQ.INDENT_LEVEL AND LEN(REQ.INDENT_LEVEL) < " + indentLevel.Length + " ORDER BY REQ.INDENT_LEVEL", includeDeleted);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the child requirements of the requirement who's indent level is passed in</summary>
		/// <param name="indentLevel">The indent-level of the requirement who's children are to be returned</param>
		/// <param name="recursive">Flag to determine whether the search includes sub-children requirements</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <returns>Requirement dataset</returns>
		public List<RequirementView> RetrieveChildren(int userId, int projectId, string indentLevel, bool recursive, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RetrieveChildren()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Store the length of the passed in indent level
				int length = indentLevel.Length;

				//Create custom SQL WHERE clause for retrieving the content item and execute
				List<RequirementView> requirements;
				if (recursive)
				{
					requirements = Retrieve(userId, projectId, "SUBSTRING(REQ.INDENT_LEVEL, 1, " + length + ") = '" + indentLevel + "' AND LEN(REQ.INDENT_LEVEL) >= " + (indentLevel.Length + 3) + " ORDER BY REQ.INDENT_LEVEL", includeDeleted);
				}
				else
				{
					requirements = Retrieve(userId, projectId, "SUBSTRING(REQ.INDENT_LEVEL, 1, " + length + ") = '" + indentLevel + "' AND LEN(REQ.INDENT_LEVEL) = " + (indentLevel.Length + 3) + " ORDER BY REQ.INDENT_LEVEL", includeDeleted);
				}
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Refreshes the summary task progress, task estimated/actual effort for a particular requirement in the requirements tree
		/// as well as its status/scope-level. It also updates the test coverage associated with the requirements in the hierarchy
		/// </summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="indentLevel">The indent level we want to refresh the task information for</param>
		/// <remarks>This will update a 'deleted' item, if a deleted item is passed as the requirementId.</remarks>
		protected internal void RefreshTaskProgressAndTestCoverage(int projectId, string indentLevel)
		{
			const string METHOD_NAME = "RefreshTaskProgressAndTestCoverage(int,string)";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Make sure we have not disabled rollups, if so log.
				if (new ProjectSettings(projectId).RollupCalculationsDisabled)
				{
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, String.Format(GlobalResources.Messages.Global_RollupCalculationsDisabled, projectId));
					return;
				}

				//First we need to retrieve the requirement record and make sure it exists
				RequirementView requirement = RetrieveByIndentLevel2(projectId, indentLevel, true);
				if (requirement != null)
				{
					//Now check the project's planning options
					Project project = new ProjectManager().RetrieveById(projectId);
					bool changeStatusFromTasks = project.IsReqStatusByTasks;
					bool changeStatusFromTestCases = project.IsReqStatusByTestCases;

					int requirementId = requirement.RequirementId;

					//Call the stored procedure to execute the refresh of the requirement test coverage and task progress
					//It also handles the rollup to the parent requirements at the same time
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						//Set a longer timeout to avoid it erroring out. IT can take a while this query
						context.CommandTimeout = SQL_COMMAND_TIMEOUT_CACHE_UPDATE;
						context.Requirement_RefreshTaskTestInfo(projectId, requirementId, indentLevel, changeStatusFromTasks, changeStatusFromTestCases);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Refreshes the summary task progress, task estimated/actual effort for a particular requirement in the requirements tree
		/// as well as its status/scope-level. It also updates the test coverage associated with the requirements in the hierarchy
		/// </summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="requirementId">The requirement we want to refresh the task information for</param>
		/// <remarks>This will update a 'deleted' item, if a deleted item is passed as the requirementId.</remarks>
		protected internal void RefreshTaskProgressAndTestCoverage(int projectId, int requirementId)
		{
			const string METHOD_NAME = "RefreshTaskProgressAndTestCoverage(int,int, bool, bool)";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Make sure we have not disabled rollups, if so log.
				if (new ProjectSettings(projectId).RollupCalculationsDisabled)
				{
					Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, String.Format(GlobalResources.Messages.Global_RollupCalculationsDisabled, projectId));
					return;
				}

				//First we need to retrieve the requirement record
				RequirementView requirement;
				try
				{
					requirement = RetrieveById2(projectId, requirementId, true);
				}
				catch (ArtifactNotExistsException)
				{
					//The record no longer exists (perhaps deleted by a user so just end quietly)
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					return;
				}
				string indentLevel = requirement.IndentLevel;

				//Now check the project's planning options
				Project project = new ProjectManager().RetrieveById(projectId);
				bool changeStatusFromTasks = project.IsReqStatusByTasks;
				bool changeStatusFromTestCases = project.IsReqStatusByTestCases;

				//Call the stored procedure to execute the refresh of the requirement test coverage and task progress
				//It also handles the rollup to the parent requirements at the same time
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Set a longer timeout to avoid it erroring out. IT can take a while this query
					context.CommandTimeout = SQL_COMMAND_TIMEOUT_CACHE_UPDATE;
					context.Requirement_RefreshTaskTestInfo(projectId, requirementId, indentLevel, changeStatusFromTasks, changeStatusFromTestCases);
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

		/// <summary>Undeletes a requirement and all children requirements, making it available to users.</summary>
		/// <param name="incidentID">The requirement to undelete.</param>
		/// <param name="userId">The userId performing the undelete.</param>
		/// <param name="logHistory">Whether to log this to history or not. Default:TRUE</param>
		public void UnDelete(int reqId, int userId, long rollbackId, bool logHistory = true)
		{
			const string METHOD_NAME = "UnDelete()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//First we need to update the deleted flag of the requirement
					var query = from r in context.Requirements
								where r.RequirementId == reqId && r.IsDeleted
								select r;

					Requirement requirement = query.FirstOrDefault();
					if (requirement != null)
					{
						requirement.StartTracking();
						requirement.IsDeleted = false;
						context.SaveChanges();

						//If successful, get the requirement and verify it's good.
						RequirementView requirementView = RetrieveById2(null, reqId, false);

						if (requirementView != null)
						{
							//Get the projectId and recursively undelete items.
							int projectId = requirementView.ProjectId;
							CascadeDeleteFlagChange(userId, projectId, requirementView.IndentLevel, false);

							//Update the coverage, scope level and progress/effort rollups
							RefreshTaskProgressAndTestCoverage(projectId, requirementView.RequirementId);

							//Now we need to retrieve its parent and see if the operation will make it summary
							int? parentRequirementId = null;
							if (requirementView.IndentLevel.Length > 3)
							{
								RequirementView parentRequirement = RetrieveParent(userId, projectId, requirementView.IndentLevel, true);
								if (parentRequirement != null)
								{
									string parentIndentLevel = parentRequirement.IndentLevel;
									parentRequirementId = parentRequirement.RequirementId;

									//Need to see how many children the parent has, and if it has one or more then 
									// undeleting will make it a summary.
									if (RetrieveChildren(userId, projectId, parentIndentLevel, false).Count >= 1)
									{
										parentRequirement.IsSummary = true;
										parentRequirement.IsExpanded = true;
										UpdatePositionalData(userId, new List<RequirementView>() { parentRequirement });
									}
								}
							}

							if (logHistory)
								new HistoryManager().LogUnDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.Requirement, reqId, rollbackId, DateTime.UtcNow);
						}
					}
				}
			}
			catch (ArtifactNotExistsException ex)
			{
				//Log a warning since it just means that the requirement no longer exists
				Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, ex);
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}
		#endregion

		#region Requirement Test Step functions

		/// <summary>Retrieves the list of covered requirements already mapped against a test step</summary>
		/// <param name="testStepId">The ID of the test step we want the list for</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The project we're interested in (optional)</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <returns>Requirement list</returns>
		public List<RequirementView> RequirementTestStep_RetrieveByTestStepId(int userId, int? projectId, int testStepId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RequirementTestStep_RetrieveByTestStepId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create custom SQL WHERE clause for retrieving the requirements and execute
				List<RequirementView> requirements = Retrieve(userId, projectId, "REQ.REQUIREMENT_ID IN (SELECT REQUIREMENT_ID FROM TST_REQUIREMENT_TEST_STEP WHERE TEST_STEP_ID = " + testStepId.ToString() + ") ORDER BY REQ.INDENT_LEVEL ASC", includeDeleted);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return requirements;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Adds a list of requirements to a specific test step</summary>
		/// <param name="projectId">The current project</param>
		/// <param name="testStepId">The id of the test step we're associating them with</param>
		/// <param name="requirementIds">The list of requirements being added</param>
		/// <remarks>Duplicates are ignored</remarks>
		/// <param name="includeChildren">Do we add the child items of summary requirements</param>
		/// <returns>The ids of the items that were actually mapped (e.g. the child items of a summary requirement)</returns>
		public List<int> RequirementTestStep_AddToTestStep(int projectId, int userId, int testStepId, List<int> requirementIds, bool includeChildren)
		{
			const string METHOD_NAME = "RequirementTestStep_AddToTestStep";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First get the list of already mapped requirements (to avoid duplicates)
				List<RequirementView> mappedRequirements = RequirementTestStep_RetrieveByTestStepId(UserManager.UserInternal, projectId, testStepId);

				//Now iterate through each of the requirements and see if it's a summary
				List<int> validatedRequirementIds = new List<int>();
				foreach (int requirementId in requirementIds)
				{
					try
					{
						RequirementView requirement = RetrieveById2(projectId, requirementId);
						if (requirement.IsSummary && includeChildren)
						{
							//Get the list of child items
							List<RequirementView> childRequirements = RetrieveChildren(UserManager.UserInternal, projectId, requirement.IndentLevel, true);
							foreach (RequirementView childRequirement in childRequirements)
							{
								if (!childRequirement.IsSummary && !mappedRequirements.Any(r => r.RequirementId == childRequirement.RequirementId))
								{
									if (!validatedRequirementIds.Contains(childRequirement.RequirementId))
									{
										validatedRequirementIds.Add(childRequirement.RequirementId);
									}
								}
							}
						}

						//Check to see if it's already mapped, if not, add to validated list
						if (!mappedRequirements.Any(r => r.RequirementId == requirementId))
						{
							if (!validatedRequirementIds.Contains(requirementId))
							{
								validatedRequirementIds.Add(requirementId);
							}
						}
					}
					catch (ArtifactNotExistsException)
					{
						//We just ignore requirements that have been deleted
					}
				}

				//Now add the validated items to the mapping table
				int? testCaseId = null;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestSteps.Include(t => t.Requirements)
								where t.TestStepId == testStepId
								select t;

					TestStep testStep = query.FirstOrDefault();
					if (testStep != null)
					{
						//Capture the test case id
						testCaseId = testStep.TestCaseId;

						//Update the date
						testStep.StartTracking();
						testStep.LastUpdateDate = DateTime.UtcNow;

						//Add the requirements
						foreach (int requirementId in validatedRequirementIds)
						{
							Requirement requirement = new Requirement();
							requirement.RequirementId = requirementId;
							context.Requirements.Attach(requirement);
							testStep.Requirements.Add(requirement);
							new HistoryManager().LogCreation(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.Requirement, requirementId, DateTime.UtcNow);
						}

						//Commit changes
						context.SaveChanges();
						
					}
				}

				//Now we need to also add the requirements to the matching test case as well
				if (validatedRequirementIds.Count > 0 && testCaseId.HasValue)
				{
					AddToTestCase(projectId, testCaseId.Value, validatedRequirementIds, userId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return validatedRequirementIds;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Removes a list of requirements from a specific test case</summary>
		/// <param name="projectId">The current project</param>
		/// <param name="testStepId">The id of the test step we're de-associating them from</param>
		/// <param name="requirementIds">The list of requirements being removed</param>
		/// <remarks>Items that are not already mapped are ignored. Also this function does not expect to get summary items</remarks>
		public void RequirementTestStep_RemoveFromTestStep(int projectId, int testStepId, List<int> requirementIds)
		{
			const string METHOD_NAME = "RequirementTestStep_RemoveFromTestStep";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Now remove the validated items from the mapping table
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestSteps.Include(t => t.Requirements)
								where t.TestStepId == testStepId
								select t;

					TestStep testStep = query.FirstOrDefault();
					if (testStep != null)
					{
						//Update the date
						testStep.StartTracking();
						testStep.LastUpdateDate = DateTime.UtcNow;

						//Remove the requirements
						foreach (int requirementId in requirementIds)
						{
							Requirement requirement = testStep.Requirements.FirstOrDefault(r => r.RequirementId == requirementId);
							if (requirement != null)
							{
								testStep.Requirements.Remove(requirement);
							}
						}

						//Commit changes
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

		/// <summary>Retrieves the list of covered test steps already mapped against a requirement</summary>
		/// <param name="testStepId">The ID of the test step we want the list for</param>
		/// <param name="userId">The user we're viewing the requirements as</param>
		/// <param name="projectId">The project we're interested in (optional)</param>
		/// <param name="includeDeleted">Whether or not to include deleted items. Default:FALSE</param>
		/// <returns>Requirement list</returns>
		public List<TestStep> RequirementTestStep_RetrieveByRequirementId(int projectId, int requirementId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RequirementTestStep_RetrieveByRequirementId()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Query to get the list of mapped test steps
				List<TestStep> testSteps;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.TestSteps
								where t.Requirements.Any(r => r.RequirementId == requirementId)
								select t;

					if (!includeDeleted)
					{
						query = query.Where(t => !t.IsDeleted);
					}
					query = query.OrderBy(t => t.TestStepId);

					testSteps = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return testSteps;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Importance Methods

		/// <summary>Inserts a new requirement importance for a specific project template</summary>
		/// <param name="projectTemplateId">The project template that the requirement importance belongs to</param>
		/// <param name="name">The display name of the requirement importance</param>
		/// <param name="active">Whether the requirement importance is active or not</param>
		/// <param name="color">The color code for the importance (in rrggbb hex format)</param>
		/// <param name="score">The numeric score value (weight) of the importance</param>
		/// <returns>The newly created requirement importance id</returns>
		public int RequirementImportance_Insert(int projectTemplateId, string name, string color, bool active, int score = 0)
		{
			const string METHOD_NAME = "RequirementImportance_Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int importanceId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Fill out entity with data for new requirement importance
					Importance requirementImportance = new Importance();
					requirementImportance.ProjectTemplateId = projectTemplateId;
					requirementImportance.Name = name.MaxLength(20);
					requirementImportance.Color = color.MaxLength(6);
					requirementImportance.IsActive = active;
					requirementImportance.Score = score;

					context.Importances.AddObject(requirementImportance);
					context.SaveChanges();
					importanceId = requirementImportance.ImportanceId;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return importanceId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Updates the requirement importances for a project</summary>
		/// <param name="requirementImportance">The requirement importance to be persisted</param>
		/// <remarks>This method performs the necessary updates</remarks>
		public void RequirementImportance_Update(Importance requirementImportance)
		{
			const string METHOD_NAME = "RequirementImportance_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Apply the changes
					context.Importances.ApplyChanges(requirementImportance);
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Deletes a requirement importance</summary>
		/// <param name="importanceId">The requirement importance to be deleted</param>
		/// <remarks>Only used by the unit tests for cleanup purposes</remarks>
		protected internal void RequirementImportance_Delete(int importanceId)
		{
			const string METHOD_NAME = "RequirementImportance_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the type
					var query = from r in context.Importances
								where r.ImportanceId == importanceId
								select r;

					Importance importance = query.FirstOrDefault();
					if (importance != null)
					{
						context.Importances.DeleteObject(importance);
						context.SaveChanges();
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				Logger.Flush();
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list of importance levels</summary>
		/// <returns>List of importance levels</returns>
		/// <param name="activeOnly">Do we only want active ones</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		public List<Importance> RequirementImportance_Retrieve(int projectTemplateId, bool activeOnly = true)
		{
			const string METHOD_NAME = "RequirementImportance_Retrieve()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				List<Importance> importances;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.Importances
								where (i.IsActive || !activeOnly) && i.ProjectTemplateId == projectTemplateId
								orderby i.ImportanceId, i.Score
								select i;
					
					importances = query.OrderByDescending(i => i.ImportanceId).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return importances;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a single requirement importance</summary>
		/// <returns>The importance entity</returns>
		/// <param name="importanceId">The id of the importance</param>
		public Importance RequirementImportance_RetrieveById(int importanceId)
		{
			const string METHOD_NAME = "RequirementImportance_RetrieveById()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				Importance importance;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.Importances
								where i.ImportanceId == importanceId
								select i;

					importance = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return importance;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion
	}
}
