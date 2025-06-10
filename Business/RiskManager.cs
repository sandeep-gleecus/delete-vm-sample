using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Transactions;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using static Inflectra.SpiraTest.DataModel.Artifact;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>
	/// The code for handing the management of project risks in Spira
	/// </summary>
	public class RiskManager : ManagerBase
	{
		private const string CLASS_NAME = "Business.RiskManager::";

		#region List of the special risk statuses used in filters to pull back aggregate statuses

		//Risk Status
		public const int RiskStatusId_AllOpen = -2;
		public const int RiskStatusId_AllClosed = -3;

		#endregion

		#region Internal Methods

		public List<RiskStatus> RetrieveStatuses(int projectTemplateId)
		{
			const string METHOD_NAME = "RetrieveStatuses()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
			List <RiskStatus> riskStatuses = new List<RiskStatus>();	
			try
			{
				
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						var query = from r in context.RiskStati
									where r.IsActive && r.ProjectTemplateId == projectTemplateId
									orderby r.Position, r.RiskStatusId
									select r;

					riskStatuses = query.ToList();
					}
			
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return riskStatuses;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}


		/// <summary>
		/// Copies across the risk fields and workflows from one project template to another
		/// </summary>
		/// <param name="riskStatusMapping">The status mapping</param>
		/// <param name="existingProjectTemplateId">The id of the existing project template</param>
		/// <param name="newProjectTemplateId">The id of the new project template</param>
		/// <param name="riskWorkflowMapping">The workflow mapping</param>
		/// <param name="riskTypeMapping">The type mapping</param>
		/// <param name="riskProbabilityMapping">The probability mapping</param>
		/// <param name="riskImpactMapping">The impact mapping</param>
		/// <param name="customPropertyIdMapping">The custom property mapping</param>
		protected internal void CopyToProjectTemplate(int existingProjectTemplateId, int newProjectTemplateId, Dictionary<int, int> riskWorkflowMapping, Dictionary<int, int> riskStatusMapping, Dictionary<int, int> riskTypeMapping, Dictionary<int, int> riskImpactMapping, Dictionary<int, int> riskProbabilityMapping, Dictionary<int, int> customPropertyIdMapping)
		{
			//***** First we need to copy across the risk statuses *****
			List<RiskStatus> riskStati = this.RiskStatus_Retrieve(existingProjectTemplateId, false);
			for (int i = 0; i < riskStati.Count; i++)
			{
				int newRiskStatusId = this.RiskStatus_Insert(
					newProjectTemplateId,
					riskStati[i].Name,
					riskStati[i].IsOpen,
					riskStati[i].IsDefault,
					riskStati[i].IsActive,
					riskStati[i].Position
					);
				riskStatusMapping.Add(riskStati[i].RiskStatusId, newRiskStatusId);
			}

			//***** Now we need to copy across the risk workflows *****
			RiskWorkflowManager workflowManager = new RiskWorkflowManager();
			workflowManager.Workflow_Copy(existingProjectTemplateId, newProjectTemplateId, customPropertyIdMapping, riskWorkflowMapping, riskStatusMapping);

			//***** Now we need to copy across the risk types *****
			List<RiskType> riskTypes = this.RiskType_Retrieve(existingProjectTemplateId, false);
			for (int i = 0; i < riskTypes.Count; i++)
			{
				//Need to retrieve the mapped workflow for this type
				if (riskWorkflowMapping.ContainsKey(riskTypes[i].RiskWorkflowId))
				{
					int workflowId = (int)riskWorkflowMapping[riskTypes[i].RiskWorkflowId];
					int newRiskTypeId = this.RiskType_Insert(
						newProjectTemplateId,
						riskTypes[i].Name,
						workflowId,
						riskTypes[i].IsDefault,
						riskTypes[i].IsActive);
					riskTypeMapping.Add(riskTypes[i].RiskTypeId, newRiskTypeId);
				}
			}

			//***** Now we need to copy across the risk impacts *****
			List<RiskImpact> riskImpacts = this.RiskImpact_Retrieve(existingProjectTemplateId, false);
			for (int i = 0; i < riskImpacts.Count; i++)
			{
				int newImpactId = this.RiskImpact_Insert(
					newProjectTemplateId,
					riskImpacts[i].Name,
					riskImpacts[i].Color,
					riskImpacts[i].IsActive,
					riskImpacts[i].Score);
				riskImpactMapping.Add(riskImpacts[i].RiskImpactId, newImpactId);
			}

			//***** Now we need to copy across the risk probabilities *****
			List<RiskProbability> riskProbabilities = this.RiskProbability_Retrieve(existingProjectTemplateId, false);
			for (int i = 0; i < riskProbabilities.Count; i++)
			{
				int newProbabilityId = this.RiskProbability_Insert(
					newProjectTemplateId,
					riskProbabilities[i].Name,
					riskProbabilities[i].Color,
					riskProbabilities[i].IsActive,
					riskProbabilities[i].Score);
				riskProbabilityMapping.Add(riskProbabilities[i].RiskProbabilityId, newProbabilityId);
			}
		}

		/// <summary>
		/// Creates the risk types, priorities, default workflow, transitions and field states
		/// for a new project template using the default template
		/// </summary>
		/// <param name="projectTemplateId">The id of the project</param>
		internal void CreateDefaultEntriesForProjectTemplate(int projectTemplateId)
		{
			const string METHOD_NAME = "CreateDefaultEntriesForProjectTemplate";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//We need to store the mapping of risk statuses to position to create the workflow
				Dictionary<int, int> riskStatuses = new Dictionary<int, int>();

				//First we need to create the risk impacts
				//this.RiskImpact_Insert(projectTemplateId, "Catastrophic", "A23520", true, 4, 1);
				//this.RiskImpact_Insert(projectTemplateId, "Critical", "D8472B", true, 3, 2);
				//this.RiskImpact_Insert(projectTemplateId, "Marginal", "E27560", true, 2, 3);
				//this.RiskImpact_Insert(projectTemplateId, "Negligible", "ECA395", true, 1, 4);

				//Next we need to create the risk probabilities
				//this.RiskProbability_Insert(projectTemplateId, "Certain", "A23520", true, 5, 1);
				//this.RiskProbability_Insert(projectTemplateId, "Likely", "D8472B", true, 4, 2);
				//this.RiskProbability_Insert(projectTemplateId, "Possible", "E27560", true, 3, 3);
				//this.RiskProbability_Insert(projectTemplateId, "Unlikely", "ECA395", true, 2, 4);
				//this.RiskProbability_Insert(projectTemplateId, "Rare", "ECC3BB", true, 1, 5);

				//First we need to create the risk impacts
				this.RiskImpact_Insert(projectTemplateId, "Catastrophic", "32CD32", true, 5, 1);
				this.RiskImpact_Insert(projectTemplateId, "Major", "BF0000", true, 4, 2);
				this.RiskImpact_Insert(projectTemplateId, "Serious", "FFA500", true, 3, 3);
				this.RiskImpact_Insert(projectTemplateId, "Marginal", "1338BE", true, 2, 4);
				this.RiskImpact_Insert(projectTemplateId, "Negligible", "1338BE", true, 1, 5);

				//Next we need to create the risk probabilities
				this.RiskProbability_Insert(projectTemplateId, "Certain", "BF0000", true, 5, 1);
				this.RiskProbability_Insert(projectTemplateId, "Likely", "32CD32", true, 4, 2);
				this.RiskProbability_Insert(projectTemplateId, "Possible", "FFA500", true, 3, 3);
				this.RiskProbability_Insert(projectTemplateId, "Unlikely", "1338BE", true, 2, 4);
				this.RiskProbability_Insert(projectTemplateId, "Rare", "FFFF33", true, 1, 5);

				//Next we need to create the risk statuses
				int riskStatusId;
				riskStatusId = this.RiskStatus_Insert(projectTemplateId, "Identified", true, true, true, 1);
				riskStatuses.Add(1, riskStatusId);
				riskStatusId = this.RiskStatus_Insert(projectTemplateId, "Analyzed", true, false, true, 2);
				riskStatuses.Add(2, riskStatusId);
				riskStatusId = this.RiskStatus_Insert(projectTemplateId, "Evaluated", true, false, true, 3);
				riskStatuses.Add(3, riskStatusId);
				riskStatusId = this.RiskStatus_Insert(projectTemplateId, "Open", true, false, true, 4);
				riskStatuses.Add(4, riskStatusId);
				riskStatusId = this.RiskStatus_Insert(projectTemplateId, "Closed", false, false, true, 5);
				riskStatuses.Add(5, riskStatusId);
				riskStatusId = this.RiskStatus_Insert(projectTemplateId, "Rejected", false, false, true, 6);
				riskStatuses.Add(6, riskStatusId);

				//Next we need to create a default workflow for a project
				RiskWorkflowManager workflowManager = new RiskWorkflowManager();
				int workflowId = workflowManager.Workflow_InsertWithDefaultEntries(projectTemplateId, GlobalResources.General.Workflow_DefaultWorflow, true, riskStatuses).RiskWorkflowId;

				//Next we need to create the risk types, associated with this workflow
				RiskType_Insert(projectTemplateId, "Business", workflowId, true, true);
				RiskType_Insert(projectTemplateId, "Technical", workflowId, false, true);
				RiskType_Insert(projectTemplateId, "Financial", workflowId, false, true);
				RiskType_Insert(projectTemplateId, "Schedule", workflowId, false, true);
				RiskType_Insert(projectTemplateId, "Other", workflowId, false, true);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		#endregion

		#region Risk Probability Functions

		/// <summary>Retrieves a list of risk probabilities</summary>
		/// <returns>List of probabilities</returns>
		/// <param name="activeOnly">Do we only want active ones</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		public List<RiskProbability> RiskProbability_Retrieve(int projectTemplateId, bool activeOnly = true)
		{
			const string METHOD_NAME = "RiskProbability_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				List<RiskProbability> riskProbabilities;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.RiskProbabilities
								where (t.IsActive || !activeOnly) && t.ProjectTemplateId == projectTemplateId
								orderby t.Position, t.RiskProbabilityId
								select t;

					riskProbabilities = query.OrderByDescending(i => i.RiskProbabilityId).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return riskProbabilities;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public List<RiskProbability> RiskProbability_RetrieveMatrix(int projectTemplateId)
		{
			const string METHOD_NAME = "RiskProbability_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				List<RiskProbability> riskProbabilities;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.RiskProbabilities
								where t.ProjectTemplateId == projectTemplateId
								orderby t.Position, t.RiskProbabilityId
								select t;

					riskProbabilities = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return riskProbabilities;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}


		/// <summary>Retrieves an risk probability by its id</summary>
		/// <param name="probabilityId">The id of the probability</param>
		/// <returns>risk probability</returns>
		public RiskProbability RiskProbability_RetrieveById(int probabilityId)
		{
			const string METHOD_NAME = "RiskProbability_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				RiskProbability riskProbability;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.RiskProbabilities
								where i.RiskProbabilityId == probabilityId
								select i;

					riskProbability = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return riskProbability;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Updates the risk probabilities for a project</summary>
		/// <param name="riskProbability">The risk probability to be persisted</param>
		/// <remarks>This method performs the necessary updates</remarks>
		public void RiskProbability_Update(RiskProbability riskProbability)
		{
			const string METHOD_NAME = "RiskProbability_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Apply the changes
					context.RiskProbabilities.ApplyChanges(riskProbability);
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

		/// <summary>Deletes a risk probability</summary>
		/// <param name="probabilityId">The probability to be deleted</param>
		/// <remarks>Only used by the unit tests for cleanup purposes</remarks>
		protected internal void RiskProbability_Delete(int probabilityId)
		{
			const string METHOD_NAME = "RiskProbability_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the type
					var query = from t in context.RiskProbabilities
								where t.RiskProbabilityId == probabilityId
								select t;

					RiskProbability probability = query.FirstOrDefault();
					if (probability != null)
					{
						context.RiskProbabilities.DeleteObject(probability);
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

		/// <summary>Inserts a new risk probability for a specific project template</summary>
		/// <param name="projectTemplateId">The project template that the risk probability belongs to</param>
		/// <param name="name">The display name of the risk probability</param>
		/// <param name="active">Whether the risk probability is active or not</param>
		/// <param name="color">The color code for the probability (in rrggbb hex format)</param>
		/// <param name="score">The numeric score value (weight) of the probability</param>
		/// <param name="position">The position, or null for the end</param>
		/// <returns>The newly created risk probability id</returns>
		public int RiskProbability_Insert(int projectTemplateId, string name, string color, bool active, int score = 0, int? position = null)
		{
			const string METHOD_NAME = "RiskProbability_Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int probabilityId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the last position if none specified
					if (!position.HasValue)
					{
						var query = from t in context.RiskProbabilities
									where t.ProjectTemplateId == projectTemplateId && t.IsActive
									orderby t.Position descending
									select t;

						RiskProbability lastRiskProbability = query.FirstOrDefault();
						if (lastRiskProbability == null)
						{
							position = 1;
						}
						else
						{
							position = lastRiskProbability.Position + 1;
						}
					}

					//Fill out entity with data for new risk probability
					RiskProbability riskProbability = new RiskProbability();
					riskProbability.ProjectTemplateId = projectTemplateId;
					riskProbability.Name = name.MaxLength(20);
					riskProbability.Color = color.MaxLength(6);
					riskProbability.IsActive = active;
					riskProbability.Position = position.Value;
					riskProbability.Score = score;

					context.RiskProbabilities.AddObject(riskProbability);
					context.SaveChanges();
					probabilityId = riskProbability.RiskProbabilityId;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return probabilityId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Risk Impact Functions

		/// <summary>Retrieves a list of risk impacts</summary>
		/// <returns>List of impacts</returns>
		/// <param name="activeOnly">Do we only want active ones</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		public List<RiskImpact> RiskImpact_Retrieve(int projectTemplateId, bool activeOnly = true)
		{
			const string METHOD_NAME = "RiskImpact_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				List<RiskImpact> riskImpacts;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.RiskImpacts
								where (t.IsActive || !activeOnly) && t.ProjectTemplateId == projectTemplateId
								orderby t.Position, t.RiskImpactId
								select t;

					riskImpacts = query.OrderByDescending(i => i.RiskImpactId).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return riskImpacts;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public List<RiskImpact> RiskImpact_RetrieveMatrix(int projectTemplateId)
		{
			const string METHOD_NAME = "RiskImpact_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				List<RiskImpact> riskImpacts;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.RiskImpacts
								where t.ProjectTemplateId == projectTemplateId
								orderby t.Position, t.RiskImpactId
								select t;

					riskImpacts = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return riskImpacts;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves an risk impact by its id</summary>
		/// <param name="impactId">The id of the impact</param>
		/// <returns>risk impact</returns>
		public RiskImpact RiskImpact_RetrieveById(int impactId)
		{
			const string METHOD_NAME = "RiskImpact_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				RiskImpact riskImpact;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.RiskImpacts
								where i.RiskImpactId == impactId
								select i;

					riskImpact = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return riskImpact;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Updates the risk impacts for a project</summary>
		/// <param name="riskImpact">The risk impact to be persisted</param>
		/// <remarks>This method performs the necessary updates</remarks>
		public void RiskImpact_Update(RiskImpact riskImpact)
		{
			const string METHOD_NAME = "RiskImpact_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Apply the changes
					context.RiskImpacts.ApplyChanges(riskImpact);
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

		/// <summary>Deletes a risk impact</summary>
		/// <param name="impactId">The impact to be deleted</param>
		/// <remarks>Only used by the unit tests for cleanup purposes</remarks>
		protected internal void RiskImpact_Delete(int impactId)
		{
			const string METHOD_NAME = "RiskImpact_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the type
					var query = from t in context.RiskImpacts
								where t.RiskImpactId == impactId
								select t;

					RiskImpact impact = query.FirstOrDefault();
					if (impact != null)
					{
						context.RiskImpacts.DeleteObject(impact);
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

		/// <summary>Inserts a new risk impact for a specific project template</summary>
		/// <param name="projectTemplateId">The project template that the risk impact belongs to</param>
		/// <param name="name">The display name of the risk impact</param>
		/// <param name="active">Whether the risk impact is active or not</param>
		/// <param name="color">The color code for the impact (in rrggbb hex format)</param>
		/// <param name="score">The numeric score value (weight) of the impact</param>
		/// <param name="position">The position, or null for the end</param>
		/// <returns>The newly created risk impact id</returns>
		public int RiskImpact_Insert(int projectTemplateId, string name, string color, bool active, int score = 0, int? position = null)
		{
			const string METHOD_NAME = "RiskImpact_Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int impactId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the last position if none specified
					if (!position.HasValue)
					{
						var query = from t in context.RiskImpacts
									where t.ProjectTemplateId == projectTemplateId && t.IsActive
									orderby t.Position descending
									select t;

						RiskImpact lastRiskImpact = query.FirstOrDefault();
						if (lastRiskImpact == null)
						{
							position = 1;
						}
						else
						{
							position = lastRiskImpact.Position + 1;
						}
					}

					//Fill out entity with data for new risk impact
					RiskImpact riskImpact = new RiskImpact();
					riskImpact.ProjectTemplateId = projectTemplateId;
					riskImpact.Name = name.MaxLength(20);
					riskImpact.Color = color.MaxLength(6);
					riskImpact.IsActive = active;
					riskImpact.Position = position.Value;
					riskImpact.Score = score;

					context.RiskImpacts.AddObject(riskImpact);
					context.SaveChanges();
					impactId = riskImpact.RiskImpactId;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return impactId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Risk Type Methods

		/// <summary>Inserts a new risk type for a specific project template</summary>
		/// <param name="projectTemplateId">The project template that the risk type belongs to</param>
		/// <param name="name">The display name of the risk type</param>
		/// <param name="active">Whether the risk type is active or not</param>
		/// <param name="workflowId">The workflow id (pass null for project default)</param>
		/// <param name="defaultType">Is this the default (initial) type of newly created risks</param>
		/// <returns>The newly created risk type id</returns>
		public int RiskType_Insert(int projectTemplateId, string name, int? workflowId, bool defaultType, bool active)
		{
			const string METHOD_NAME = "RiskType_Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//If no workflow provided, simply use the project default workflow
				if (!workflowId.HasValue)
				{
					RiskWorkflowManager workflowManager = new RiskWorkflowManager();
					workflowId = workflowManager.Workflow_GetDefault(projectTemplateId).RiskWorkflowId;
				}

				int riskTypeId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					RiskType riskType = new RiskType();
					riskType.ProjectTemplateId = projectTemplateId;
					riskType.Name = name.MaxLength(20);
					riskType.IsDefault = defaultType;
					riskType.IsActive = active;
					riskType.RiskWorkflowId = workflowId.Value;

					context.RiskTypes.AddObject(riskType);
					context.SaveChanges();
					riskTypeId = riskType.RiskTypeId;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return riskTypeId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Updates the risk types for a project</summary>
		/// <param name="riskType">The risk type to be persisted</param>
		/// <remarks>This method performs the necessary updates</remarks>
		public void RiskType_Update(RiskType riskType)
		{
			const string METHOD_NAME = "RiskType_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Apply the changes
					context.RiskTypes.ApplyChanges(riskType);
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

		/// <summary>Retrieves a list of risk types</summary>
		/// <param name="activeOnly">Do we only want active ones</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <returns>List of types</returns>
		public List<RiskType> RiskType_Retrieve(int projectTemplateId, bool activeOnly = true)
		{
			const string METHOD_NAME = "RiskType_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				List<RiskType> riskTypes;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.RiskTypes
								where (t.IsActive || !activeOnly) && t.ProjectTemplateId == projectTemplateId
								orderby t.Name, t.RiskTypeId
								select t;

					riskTypes = query.OrderByDescending(i => i.RiskTypeId).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return riskTypes;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves the default risk type for the specified template
		/// </summary>
		/// <param name="projectTemplateId">The id of the template</param>
		/// <returns>The default risk type</returns>
		public RiskType RiskType_RetrieveDefault(int projectTemplateId)
		{
			const string METHOD_NAME = "RiskType_RetrieveDefault";


			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				RiskType type;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from r in context.RiskTypes
								where r.ProjectTemplateId == projectTemplateId && r.IsDefault
								select r;

					type = query.FirstOrDefault();
					if (type == null)
					{
						throw new ApplicationException(String.Format(GlobalResources.Messages.Risk_NoDefaultTypeForProjectTemplate, projectTemplateId));
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

		/// <summary>
		/// Retrieves a risk type by its ID
		/// </summary>
		/// <param name="riskTypeId">The id of the risk type</param>
		/// <returns>The risk type</returns>
		public RiskType RiskType_RetrieveById(int riskTypeId)
		{
			const string METHOD_NAME = "RiskType_RetrieveById";


			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				RiskType type;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create select command for retrieving the lookup data
					var query = from r in context.RiskTypes
								where r.RiskTypeId == riskTypeId
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

		/// <summary>Deletes the risk types for a project template</summary>
		/// <param name="riskTypeId">The risk type to be deleted</param>
		/// <remarks>Only used by the unit tests for cleanup purposes</remarks>
		protected internal void RiskType_Delete(int riskTypeId)
		{
			const string METHOD_NAME = "RiskType_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the type
					var query = from r in context.RiskTypes
								where r.RiskTypeId == riskTypeId
								select r;

					RiskType type = query.FirstOrDefault();
					if (type != null)
					{
						context.RiskTypes.DeleteObject(type);
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

		#endregion

		#region Risk Status Functions

		/// <summary>Deletes the risk statuses for a project template</summary>
		/// <param name="riskStatusId">The risk status to be deleted</param>
		/// <remarks>Only used by the unit tests for cleanup purposes</remarks>
		protected internal void RiskStatus_Delete(int riskStatusId)
		{
			const string METHOD_NAME = "RiskStatus_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the status
					var query = from r in context.RiskStati
								where r.RiskStatusId == riskStatusId
								select r;

					RiskStatus status = query.FirstOrDefault();
					if (status != null)
					{
						context.RiskStati.DeleteObject(status);
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

		/// <summary>Gets the default (i.e. initial) risk status for all newly created risks</summary>
		/// <param name="projectTemplateId">The current Project Template ID</param>
		/// <returns>The risk status</returns>
		/// <remarks>Returns null is there is no default risk status for the project template (shouldn't really happen)</remarks>
		public RiskStatus RiskStatus_RetrieveDefault(int projectTemplateId)
		{
			const string METHOD_NAME = "RiskStatus_RetrieveDefault";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				RiskStatus riskStatus;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.RiskStati
								where i.IsDefault && i.ProjectTemplateId == projectTemplateId
								select i;

					riskStatus = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return riskStatus;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the RiskStatus by the given ID.</summary>
		/// <param name="riskStatusId">The status ID to retrieve.</param>
		/// <param name="workflowId">Set the value if you only want workflow fields/custom properties for a specific workflow</param>
		/// <returns>The RiskStatus, or null if not found.</returns>
		/// <remarks>Will return deleted items.</remarks>
		/// <param name="includeWorkflowFields">Should we include the linked workflow fields (for all workflows)</param>
		public RiskStatus RiskStatus_RetrieveById(int riskStatusId, bool includeWorkflowFields = false)
		{
			const string METHOD_NAME = CLASS_NAME + "RiskStatus_RetrieveById(int,[bool])";

			Logger.LogEnteringEvent(METHOD_NAME);

			RiskStatus retStatus = null;
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				ObjectQuery<RiskStatus> riskStati = context.RiskStati;
				if (includeWorkflowFields)
				{
					riskStati = riskStati.Include("WorkflowFields").Include("WorkflowCustomProperties");
				}
				var query = from ts in riskStati
							where ts.RiskStatusId == riskStatusId
							select ts;

				try
				{
					retStatus = query.First();
				}
				catch (Exception ex)
				{
					Logger.LogWarningEvent(METHOD_NAME, ex, "Retrieving Risk Status ID #" + riskStatusId + ":");
					retStatus = null;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retStatus;
		}

		/// <summary>
		/// Returns a list of all the open status IDs in a project template (active only)
		/// </summary>
		/// <param name="projectTemplateId">The project template</param>
		/// <returns>List of ids</returns>
		public List<int> RiskStatus_RetrieveOpenIds(int projectTemplateId)
		{
			const string METHOD_NAME = "RiskStatus_RetrieveOpenIds";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<int> statusIds;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.RiskStati
								where i.ProjectTemplateId == projectTemplateId && i.IsOpen && i.IsActive
								orderby i.RiskStatusId
								select i.RiskStatusId;

					statusIds = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return statusIds;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Returns a list of all the closed status IDs in a project template (active only)
		/// </summary>
		/// <param name="projectTemplateId">The project template</param>
		/// <returns>List of ids</returns>
		public List<int> RiskStatus_RetrieveClosedIds(int projectTemplateId)
		{
			const string METHOD_NAME = "RiskStatus_RetrieveClosedIds";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<int> statusIds;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.RiskStati
								where i.ProjectTemplateId == projectTemplateId && !i.IsOpen && i.IsActive
								orderby i.RiskStatusId
								select i.RiskStatusId;

					statusIds = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return statusIds;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the all the risk statuses in the project template</summary>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="activeOnly">Do we only want active statuses</param>
		/// <param name="includeWorkflowFields">Should we include the linked workflow fields (for all workflows)</param>
		/// <returns>The RiskStatuses</returns>
		public List<RiskStatus> RiskStatus_Retrieve(int projectTemplateId, bool activeOnly = true, bool includeWorkflowFields = false)
		{
			const string METHOD_NAME = CLASS_NAME + "RiskStatus_Retrieve(int,[bool],[bool])";

			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				List<RiskStatus> retStatus;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					ObjectQuery<RiskStatus> riskStati = context.RiskStati;
					if (includeWorkflowFields)
					{
						riskStati = riskStati
							.Include(d => d.WorkflowFields)
							.Include(d => d.WorkflowCustomProperties);
					}
					var query = from i in riskStati
								where i.ProjectTemplateId == projectTemplateId && (i.IsActive || !activeOnly)
								orderby i.Position, i.RiskStatusId
								select i;

					retStatus = query.OrderByDescending(i => i.RiskStatusId).ToList();
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return retStatus;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Updates an risk status in the system.</summary>
		/// <param name="riskStatus">The status to update.</param>
		/// <returns>The updated status.</returns>
		public RiskStatus RiskStatus_Update(RiskStatus riskStatus)
		{
			const string METHOD_NAME = CLASS_NAME + "RiskStatus_Update(RiskStatus)";

			Logger.LogEnteringEvent(METHOD_NAME);

			RiskStatus retStatus = null;

			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				try
				{
					context.RiskStati.ApplyChanges(riskStatus);
					context.SaveChanges();

					retStatus = riskStatus;
				}
				catch (Exception ex)
				{
					Logger.LogErrorEvent(METHOD_NAME, ex, "Saving Status");
					throw;
				}
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retStatus;
		}

		/// <summary>Inserts a new risk status for a specific project template</summary>
		/// <param name="projectTemplateId">The project template that the risk status belongs to</param>
		/// <param name="name">The display name of the risk status</param>
		/// <param name="active">Whether the risk status is active or not</param>
		/// <param name="open">Is this risk status considered 'open'</param>
		/// <param name="defaultStatus">Is this the default (initial) status of newly created risks</param>
		/// <param name="position">The position, null = end of the list</param>
		/// <returns>The newly created risk status id</returns>
		public int RiskStatus_Insert(int projectTemplateId, string name, bool open, bool defaultStatus, bool active, int? position = null)
		{
			const string METHOD_NAME = "RiskStatus_Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int riskStatusId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					RiskStatus riskStatus = new RiskStatus();
					riskStatus.ProjectTemplateId = projectTemplateId;
					riskStatus.Name = name.MaxLength(20);
					riskStatus.IsDefault = defaultStatus;
					riskStatus.IsOpen = open;
					riskStatus.IsActive = active;

					if (position.HasValue)
					{
						riskStatus.Position = position.Value;
					}
					else
					{
						//Get the last position
						var query = from d in context.RiskStati
									where d.ProjectTemplateId == projectTemplateId
									orderby d.Position descending
									select d;

						RiskStatus lastStatus = query.FirstOrDefault();
						if (lastStatus == null)
						{
							riskStatus.Position = 1;
						}
						else
						{
							riskStatus.Position = lastStatus.Position + 1;
						}
					}

					context.RiskStati.AddObject(riskStatus);
					context.SaveChanges();
					riskStatusId = riskStatus.RiskStatusId;
				}

				//Now capture the newly created id and return
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return riskStatusId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}
		#endregion

		#region Risk Functions

		/// <summary>
		/// Creates a new blank risk record
		/// </summary>
		/// <param name="creatorId">The id of the current user who will be initially set as the creator</param>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The new blank risk entity with a single datarow</returns>
		public RiskView Risk_New(int projectId, int creatorId)
		{
			const string METHOD_NAME = "Risk_New";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create the new entity
				RiskView risk = new RiskView();

				//Find the default status for the project
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
				RiskStatus defaultStatus = RiskStatus_RetrieveDefault(projectTemplateId);
				RiskType defaultType = RiskType_RetrieveDefault(projectTemplateId);

				//Populate the new risk
				risk.ProjectId = projectId;
				risk.CreatorId = creatorId;
				risk.RiskStatusId = defaultStatus.RiskStatusId;
				risk.RiskStatusName = defaultStatus.Name;
				risk.RiskTypeId = defaultType.RiskTypeId;
				risk.RiskTypeName = defaultType.Name;
				risk.CreationDate = risk.LastUpdateDate = risk.ConcurrencyDate = DateTime.UtcNow;
				risk.Name = "";
				risk.Description = "";
				risk.IsAttachments = false;

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return risk;
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				throw;
			}
		}

		/// <summary>Inserts a new risk into the system</summary>
		/// <param name="description">The long description of the risk</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="name">The name of the risk</param>
		/// <param name="riskTypeId">The type of the risk (bug, issue, etc.)</param>
		/// <param name="riskStatusId">The initial status of the risk (new, open, etc.)</param>
		/// <param name="ownerId">Who has been assigned to own its resolution</param>
		/// <param name="closedDate">The date closed (Optional) - pass DateTimeNull if not set</param>
		/// <param name="creationDate">The creation date</param>
		/// <param name="creatorId">The user who created the risk</param>
		/// <param name="logHistory">Should we log a new risk history event</param>
		/// <param name="componentId">The id of the component (optional)</param>
		/// <param name="impactId">The impact of the risk</param>
		/// <param name="probabilityId">The probability of the risk</param>
		/// <param name="releaseId">The release</param>
		/// <param name="reviewDate">The review date</param>
		/// <returns>The ID of the newly created risk</returns>
		public int Risk_Insert(int projectId, int? riskStatusId, int? riskTypeId, int? probabilityId, int? impactId, int creatorId, int? ownerId, string name, string description, int? releaseId, int? componentId, DateTime creationDate, DateTime? reviewDate, DateTime? closedDate, bool logHistory = true)
		{
			const string METHOD_NAME = "Risk_Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int riskId;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//If we're passed null for risk status, we need to use the default one for this project template
					if (!riskStatusId.HasValue || !riskTypeId.HasValue)
					{
						int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

						if (!riskStatusId.HasValue)
							riskStatusId = RiskStatus_RetrieveDefault(projectTemplateId).RiskStatusId;

						//If we're passed null for risk type, we need to use the default one for this project
						if (!riskTypeId.HasValue)
							riskTypeId = RiskType_RetrieveDefault(projectTemplateId).RiskTypeId;
					}

					//Create the risk object and populate
					Risk risk = new Risk();
					risk.ProjectId = projectId;
					risk.RiskProbabilityId = probabilityId;
					risk.RiskImpactId = impactId;
					risk.RiskStatusId = riskStatusId.Value;
					risk.RiskTypeId = riskTypeId.Value;
					risk.ComponentId = componentId;
					risk.CreatorId = creatorId;
					risk.OwnerId = ownerId;
					risk.ReleaseId = releaseId;
					risk.Name = name.MaxLength(255);
					risk.Description = description;
					risk.CreationDate = creationDate;
					risk.ReviewDate = reviewDate;
					risk.ClosedDate = closedDate;
					risk.LastUpdateDate = System.DateTime.UtcNow;
					risk.ConcurrencyDate = DateTime.UtcNow;
					risk.IsAttachments = false;
					risk.IsDeleted = false;

					//Persist the risk and get the risk id
					context.Risks.AddObject(risk);
					context.SaveChanges();
					riskId = risk.RiskId;
				}

				//Add a history record for the inserted risk.
				if (logHistory)
				{
					new HistoryManager().LogCreation(projectId, creatorId, DataModel.Artifact.ArtifactTypeEnum.Risk, riskId, DateTime.UtcNow);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return riskId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates the last updated date of the risk (usually called when a step is changed)
		/// </summary>
		/// <param name="context">The EF context</param>
		/// <param name="riskId">The id of the risk</param>
		protected void UpdateRiskLastUpdateDate(SpiraTestEntitiesEx context, int riskId)
		{
			//Finally we need to update the last updated date of the risk itself
			var query3 = from r in context.Risks
						 where r.RiskId == riskId
						 select r;

			Risk risk = query3.FirstOrDefault();
			if (risk != null)
			{
				risk.StartTracking();
				risk.LastUpdateDate = DateTime.UtcNow;
			}
		}

		/// <summary>Undeletes an risk, making it available to users.</summary>
		/// <param name="riskId">The risk to undelete.</param>
		/// <param name="userId">The userId performing the undelete.</param>
		/// <param name="logHistory">Whether to log this to history or not. Default: TRUE</param>
		public void Risk_UnDelete(int riskId, int userId, long rollbackId, bool logHistory = true)
		{
			const string METHOD_NAME = "Risk_UnDelete()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//We need to initially retrieve the risk (needs to be marked as deleted)
					var query = from i in context.Risks
								where i.RiskId == riskId && i.IsDeleted
								select i;

					//Get the risk
					Risk risk = query.FirstOrDefault();
					if (risk != null)
					{
						int projectId = risk.ProjectId;

						//Mark as undeleted
						risk.StartTracking();
						risk.LastUpdateDate = DateTime.UtcNow;
						risk.IsDeleted = false;

						//Save changes, no history logged, that's done later
						context.SaveChanges();

						//Log the undelete
						if (logHistory)
						{
							//Okay, mark it as being undeleted.
							new HistoryManager().LogUnDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.Risk, riskId, rollbackId, DateTime.UtcNow);
						}
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

		/// <summary>Deletes an risk in the system that has the specified ID</summary>
		/// <param name="riskId">The ID of the risk to be deleted</param>
		public void Risk_DeleteFromDatabase(int riskId, int userId)
		{
			const string METHOD_NAME = "Risk_DeleteFromDatabase()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//We need to initially retrieve the risk to see if it's linked to a release
				Risk risk;
				try
				{
					risk = Risk_RetrieveById(riskId, false, false, true);
				}
				catch (ArtifactNotExistsException)
				{
					//If it's already deleted, just fail quietly
					Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
					return;
				}
				int projectId = risk.ProjectId;
				string riskName = risk.Name;

				//First we need to delete any attachments associated with the risk
				Business.AttachmentManager attachment = new Business.AttachmentManager();
				attachment.DeleteByArtifactId(riskId, DataModel.Artifact.ArtifactTypeEnum.Risk);

				//Next we need to delete any artifact links to/from this risk
				Business.ArtifactLinkManager artifactLink = new Business.ArtifactLinkManager();
				artifactLink.DeleteByArtifactId(DataModel.Artifact.ArtifactTypeEnum.Risk, riskId);

				//Next we need to delete any custom properties associated with the risk
				new CustomPropertyManager().ArtifactCustomProperty_DeleteByArtifactId(riskId, DataModel.Artifact.ArtifactTypeEnum.Risk);

				//Actually perform the delete on the risk and its resolutions
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.Risks.Attach(risk);
					context.Risks.DeleteObject(risk);
					context.SaveChanges();
				}

				//Log the purge.
				new HistoryManager().LogPurge(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.Risk, riskId, DateTime.UtcNow, riskName);
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Retrieves all items in the specified project that ARE marked for deletion.</summary>
		/// <param name="projectId">The project ID to get items for.</param>
		/// <returns>List of soft-deleted risks</returns>
		public List<RiskView> RetrieveDeleted(int projectId)
		{
			const string METHOD_NAME = "RetrieveDeleted()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RiskView> risks;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the query for retrieving the task list
					var query = from t in context.RisksView
								where t.ProjectId == projectId && t.IsDeleted
								orderby t.RiskId
								select t;

					risks = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return risks;
			}
			catch (Exception ex)
			{
				//Do not rethrow, just return an empty list
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				return new List<RiskView>();
			}
		}

		/// <summary>Marks an Risk as being deleted and no longer available in the system.</summary>
		/// <param name="riskId">The ID to mark as 'deleted'.</param>
		/// <param name="userId">The userId of the user performing the delete.</param>
		public void Risk_MarkAsDeleted(int projectId, int riskId, int userId)
		{
			const string METHOD_NAME = "Risk_MarkAsDeleted()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				bool deletePerformed = false;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Retrieve the risk
					var query = from i in context.Risks
								where i.RiskId == riskId && !i.IsDeleted
								select i;

					Risk risk = query.FirstOrDefault();
					if (risk != null)
					{
						//Mark as deleted
						risk.StartTracking();
						risk.LastUpdateDate = DateTime.UtcNow;
						risk.IsDeleted = true;
						context.SaveChanges();
						deletePerformed = true;
					}
				}

				if (deletePerformed)
				{
					//Add a changeset to mark it as deleted.
					new HistoryManager().LogDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.Risk, riskId, DateTime.UtcNow);
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				throw ex;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Makes a copy of an risk within the same project</summary>
		/// <param name="userId">The id of the user making the copy</param>
		/// <param name="riskId">The id of the risk we want to make a copy of</param>
		/// <returns>The id of the newly created copy</returns>
		public int Risk_Copy(int userId, int riskId)
		{
			const string METHOD_NAME = "Risk_Copy";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the risk we want to copy
				Risk risk = this.Risk_RetrieveById(riskId, true, true);

				//Get the project and project template id
				int projectId = risk.ProjectId;
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

				//Actually perform the insert of the copy
				int copiedRiskId = this.Risk_Insert(
					risk.ProjectId,
					risk.RiskStatusId,
					risk.RiskTypeId,
					risk.RiskProbabilityId,
					risk.RiskImpactId,
					userId,
					risk.OwnerId,
					risk.Name + CopiedArtifactNameSuffix,
					risk.Description,
					risk.ReleaseId,
					risk.ComponentId,
					DateTime.UtcNow,
					risk.ReviewDate,
					risk.ClosedDate
					);

				//Now we need to copy across any custom properties
				new CustomPropertyManager().ArtifactCustomProperty_Copy(projectId, projectTemplateId, riskId, copiedRiskId, DataModel.Artifact.ArtifactTypeEnum.Risk, userId);

				//Now we need to copy across any comments
				if (risk.Discussions.Count > 0)
				{
					DiscussionManager discussionManager = new DiscussionManager();
					foreach (RiskDiscussion discussion in risk.Discussions)
					{
						discussionManager.Insert(userId, copiedRiskId, Artifact.ArtifactTypeEnum.Risk, discussion.Text, projectId, discussion.IsPermanent, false);
					}
				}

				//Now we need to copy across any mitigations
				if (risk.Mitigations.Count > 0)
				{
					foreach (RiskMitigation mitigation in risk.Mitigations)
					{
						this.RiskMitigation_Insert(projectId, copiedRiskId, null, mitigation.Description, userId, DateTime.UtcNow, mitigation.ReviewDate, true);
					}
				}

				//Now we need to copy across any Tasks, making sure to reset them back to 'Not Started' with 0% progress
				TaskManager taskManager = new TaskManager();
				List<TaskView> tasks = taskManager.RetrieveByRiskId(riskId);
				foreach (TaskView task in tasks)
				{
					int newTaskId = taskManager.Insert(
						projectId,
						userId,
						DataModel.Task.TaskStatusEnum.NotStarted,
						task.TaskTypeId,
						task.TaskFolderId,
						null,
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
						userId,
						true,
						copiedRiskId
						);

					//Now we need to copy across any custom properties
					new CustomPropertyManager().ArtifactCustomProperty_Copy(projectId, projectTemplateId, task.TaskId, newTaskId, DataModel.Artifact.ArtifactTypeEnum.Task, userId);
				}

				//Now we need to copy across any linked attachments
				AttachmentManager attachment = new AttachmentManager();
				attachment.Copy(risk.ProjectId, DataModel.Artifact.ArtifactTypeEnum.Risk, riskId, copiedRiskId);

				//Finally we need to add an association from the old risk to the new one
				ArtifactLinkManager artifactLink = new ArtifactLinkManager();
				artifactLink.Insert(
					risk.ProjectId,
					DataModel.Artifact.ArtifactTypeEnum.Risk,
					riskId,
					DataModel.Artifact.ArtifactTypeEnum.Risk,
					copiedRiskId,
					userId,
					"Copied Risk",
					DateTime.UtcNow
					);

				//Send a notification
				this.Risk_SendCreationNotification(copiedRiskId, null, null);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the risk id of the copy
				return copiedRiskId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Exports an risk from one project to another</summary>
		/// <param name="riskId">The id of the risk we want to make a copy of</param>
		/// <param name="destProjectId">The project we want to export it to</param>
		/// <returns>The id of the newly created copy</returns>
		/// <remarks>Any project template configurable fields (priority, status, etc.) will be either unset or set to default values
		/// if the projects don't use the same template. Also the start and closed dates have to be left unset
		/// because we have a new detected on date.
		/// </remarks>
		public int Risk_Export(int riskId, int destProjectId, int userId)
		{
			const string METHOD_NAME = "Risk_Export";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First we need to retrieve the risk we want to copy
				Risk existingRisk = this.Risk_RetrieveById(riskId, true, true);

				//Get the IDs of the source and destination project templates
				TemplateManager templateManager = new TemplateManager();
				int sourceProjectTemplateId = templateManager.RetrieveForProject(existingRisk.ProjectId).ProjectTemplateId;
				int destProjectTemplateId = templateManager.RetrieveForProject(destProjectId).ProjectTemplateId;
				bool projectsUseSameTemplate = (sourceProjectTemplateId == destProjectTemplateId);

				int riskStatusId;
				int riskTypeId;
				int? riskProbabilityId = null;
				int? riskImpactId = null;
				if (projectsUseSameTemplate)
				{
					riskStatusId = existingRisk.RiskStatusId;
					riskTypeId = existingRisk.RiskTypeId;
					riskProbabilityId = existingRisk.RiskProbabilityId;
					riskImpactId = existingRisk.RiskImpactId;
				}
				else
				{
					//Need to get the default type and status for the project
					riskStatusId = this.RiskStatus_RetrieveDefault(destProjectTemplateId).RiskStatusId;
					riskTypeId = this.RiskType_RetrieveDefault(destProjectTemplateId).RiskTypeId;
				}

				//Actually perform the insert of the copy
				//Some will always be null as the projects don't have the same fields
				int exportedRiskId = this.Risk_Insert(
					destProjectId,
					riskStatusId,
					riskTypeId,
					riskProbabilityId,
					riskImpactId,
					userId,
					existingRisk.OwnerId,
					existingRisk.Name,
					existingRisk.Description,
					null,
					null,
					DateTime.UtcNow,
					(projectsUseSameTemplate) ? existingRisk.ReviewDate : null,
					(projectsUseSameTemplate) ? existingRisk.ClosedDate : null
					);

				//Create history item..
				new HistoryManager().LogImport(destProjectId, existingRisk.ProjectId, existingRisk.RiskId, userId, DataModel.Artifact.ArtifactTypeEnum.Risk, exportedRiskId, DateTime.UtcNow);

				//Now we need to copy across any comments
				if (existingRisk.Discussions.Count > 0)
				{
					DiscussionManager discussionManager = new DiscussionManager();
					foreach (RiskDiscussion discussion in existingRisk.Discussions)
					{
						discussionManager.Insert(userId, exportedRiskId, Artifact.ArtifactTypeEnum.Risk, discussion.Text, destProjectId, discussion.IsPermanent, false);
					}
				}

				//Now we need to copy across any mitigations
				if (existingRisk.Mitigations.Count > 0)
				{
					foreach (RiskMitigation mitigation in existingRisk.Mitigations)
					{
						this.RiskMitigation_Insert(destProjectId, exportedRiskId, null, mitigation.Description, userId, DateTime.UtcNow, mitigation.ReviewDate);
					}
				}

				//Now we need to copy across any linked attachments
				AttachmentManager attachmentManager = new AttachmentManager();
				attachmentManager.Export(existingRisk.ProjectId, DataModel.Artifact.ArtifactTypeEnum.Risk, riskId, destProjectId, exportedRiskId);

				//If they use the same template, also copy across any custom properties
				if (projectsUseSameTemplate)
				{
					//Now we need to copy across any custom properties
					new CustomPropertyManager().ArtifactCustomProperty_Export(sourceProjectTemplateId, existingRisk.ProjectId, riskId, destProjectId, exportedRiskId, DataModel.Artifact.ArtifactTypeEnum.Risk, userId);
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				//Return the risk id of the copy
				return exportedRiskId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public TST_ARTIFACT_SIGNATURE RetrieveRiskSignature(int riskId, int artifactTypeId)
		{
			const string METHOD_NAME = "RetrieveRiskSignature";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Get the list of risk in the project
				TST_ARTIFACT_SIGNATURE riskSignature;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.ArtifactSignatures
								where t.ARTIFACT_ID == riskId && t.ARTIFACT_TYPE_ID == artifactTypeId
								select t;

					query = query.OrderByDescending(r => r.UPDATE_DATE);

					riskSignature = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return riskSignature;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		public void RiskSignatureInsert(int projectId, int currentStatusId, Risk risk, string meaning, int? loggedinUserId = null)
		{
			using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
			{
				DateTime updatedDate = DateTime.Now;

				var newReqSignature = new TST_ARTIFACT_SIGNATURE
				{
					STATUS_ID = currentStatusId,
					ARTIFACT_ID = risk.RiskId,
					ARTIFACT_TYPE_ID = (int)ArtifactTypeEnum.Risk,
					USER_ID = (int)loggedinUserId,
					UPDATE_DATE = DateTime.Now,
					MEANING = meaning,
				};

				context.ArtifactSignatures.AddObject(newReqSignature);

				context.SaveChanges();
				//log history
				new HistoryManager().LogCreation(projectId, (int)loggedinUserId, Artifact.ArtifactTypeEnum.RiskSignature, risk.RiskId, DateTime.UtcNow);

			}
		}

		public RiskStatus RetrieveStatusById(int statusId)
		{
			const string METHOD_NAME = "RetrieveStatusById()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the lookup data
				RiskStatus status;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from t in context.RiskStati
								where t.RiskStatusId == statusId
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


		/// <summary>
		/// Sends a creation notification, typically only used for API creation calls where we need to retrieve and force it as 'added'
		/// </summary>
		/// <param name="riskId">The id of the risk</param>
		/// <param name="artifactCustomProperty">The custom property row</param>
		/// <param name="newComment">The new comment (if any)</param>
		/// <remarks>Fails quietly but logs errors</remarks>
		public void Risk_SendCreationNotification(int riskId, ArtifactCustomProperty artifactCustomProperty, string newComment)
		{
			const string METHOD_NAME = "Risk_SendCreationNotification";
			//Send a notification
			try
			{
				RiskView notificationArt = Risk_RetrieveById2(riskId);
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

		/// <summary>Retrieves a single risk in the system that has a certain ID</summary>
		/// <param name="riskId">The ID of the risk to be returned</param>
		/// <returns>Risk entity, or null if it does not exist</returns>
		/// <param name="includeDeleted">Should we include deleted risks</param>
		/// <param name="includeMitigations">Should we include the mitigations</param>
		/// <param name="includeComments">Should we include the comments</param>
		/// <remarks>Also retrieves associated mitigations, but they are NOT sorted</remarks>
		public Risk Risk_RetrieveById(int riskId, bool includeMitigations = false, bool includeComments = false, bool includeDeleted = false)
		{
			const string METHOD_NAME = "Risk_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				Risk risk;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//See if we want to include resolutions or comments
					ObjectQuery<Risk> riskSet = context.Risks;
					if (includeComments)
					{
						riskSet = (ObjectQuery<Risk>)riskSet
							.Include(r => r.Discussions)
							.Include("Discussions.Creator")
							.Include("Discussions.Creator.Profile");
					}

					//Create custom LINQ WHERE clause for retrieving the risk by id and execute
					var query = from i in riskSet
								where i.RiskId == riskId && (includeDeleted || !i.IsDeleted)
								select i;

					risk = query.FirstOrDefault();

					//Get undeleted mitigations, they get joined implicitly using EF4 'fix-up'
					if (includeMitigations)
					{
						var query2 = from m in context.RiskMitigations
									 where m.RiskId == riskId && !m.IsDeleted
									 select m;

						query2.ToList();
					}
				}

				//If we don't have a record, throw a specific exception (since client will not be expecting null)
				if (risk == null)
				{
					throw new ArtifactNotExistsException("Risk " + riskId + " doesn't exist in the system.");
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return risk;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a single risk view in the system that has a certain ID</summary>
		/// <param name="riskId">The ID of the risk to be returned</param>
		/// <returns>Risk view (or null if it doesn't exist)</returns>
		/// <param name="includeDeleted">Should we include deleted risks</param>
		public RiskView Risk_RetrieveById2(int riskId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "Risk_RetrieveById2";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				RiskView risk;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create custom LINQ WHERE clause for retrieving the risk by id and execute
					var query = from i in context.RisksView
								where i.RiskId == riskId && (includeDeleted || !i.IsDeleted)
								select i;

					risk = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return risk;
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

		/// <summary>Retrieves all items in the specified project that ARE marked for deletion.</summary>
		/// <param name="projectId">The project ID to get items for.</param>
		/// <returns></returns>
		public List<RiskView> Risk_RetrieveDeleted(int projectId)
		{
			const string METHOD_NAME = "Risk_RetrieveDeleted()";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RiskView> risks;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from i in context.RisksView
								where i.IsDeleted && i.ProjectId == projectId
								orderby i.RiskId
								select i;

					risks = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return risks;
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, ex);
				//Do not rethrow.
				return new List<RiskView>();
			}
		}

		/// <summary>Updates an Risk that is passed-in</summary>
		/// <param name="risk">The risk to be persisted</param>
		/// <param name="userId">The user making the change</param>
		/// <param name="rollbackId">Whether or not this save is a rollback. Default: NULL</param>
		/// <remarks>1) Sends notifications if risk status has changed
		/// 2) Inserts, updates or deletes any changes to the mitigations list</remarks>
		public void Risk_Update(Risk risk, int userId, long? rollbackId = null, bool sendNotification = false)
		{
			const string METHOD_NAME = "Risk_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			//If we have a null entity just return
			if (risk == null)
			{
				return;
			}

			//Next we need to formally validate the data
			Dictionary<string, string> validationMessages = Validate(risk);
			if (validationMessages.Count > 0)
			{
				//We need to return these messages back as special exceptions
				//We just sent back the first message
				string validationMessage = validationMessages.First().Value;
				throw new DataValidationException(validationMessage);
			}

			try
			{

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Start tracking changes
					risk.StartTracking();

					//Update the last-update and concurrency dates
					risk.LastUpdateDate = DateTime.UtcNow;
					risk.ConcurrencyDate = DateTime.UtcNow;

					//Now apply the changes (will auto-insert any provided resolutions)
					context.Risks.ApplyChanges(risk);

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
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
		}

		/// <summary>Handles any Risk specific filters that are not generic</summary>
		/// <param name="expressionList">The existing list of expressions</param>
		/// <param name="filter">The current filter</param>
		/// <param name="projectId">The current project</param>
		/// <param name="projectTemplateId">the current project template</param>
		/// <param name="p">The LINQ parameter</param>
		/// <param name="utcOffset">The current offset from UTC</param>
		/// <returns>True if handled, return False for the standard filter handling</returns>
		protected internal bool HandleRiskSpecificFilters(int? projectId, int? projectTemplateId, ParameterExpression p, List<Expression> expressionList, KeyValuePair<string, object> filter, double utcOffset)
		{
			//By default, let the generic filter convertor handle the filter
			string filterProperty = filter.Key;
			object filterValue = filter.Value;

			//Handle the special case of release filters where we want to also retrieve child iterations
			if (filterProperty == "ReleaseId" && (int)filterValue != NoneFilterValue && projectId.HasValue)
			{
				//Get the release and its child iterations
				int releaseId = (int)filterValue;
				List<int> releaseIds = new ReleaseManager().GetSelfAndIterations(projectId.Value, releaseId);
				ConstantExpression releaseIdsExpression = LambdaExpression.Constant(releaseIds);
				MemberExpression memberExpression = LambdaExpression.PropertyOrField(p, "ReleaseId");
				//Equivalent to: p => releaseIds.Contains(p.ReleaseId) i.e. (RELEASE_ID IN (1,2,3))
				Expression releaseExpression = Expression.Call(releaseIdsExpression, "Contains", null, Expression.Convert(memberExpression, typeof(int)));
				expressionList.Add(releaseExpression);
				return true;
			}

			//If we have the case of risk status then see if we have
			//one of the special aggregate values (denoted by negative ids)

			//Risk Status
			if (filterProperty == "RiskStatusId" && projectTemplateId.HasValue)
			{
				//See if it's using a simple int or a multi-value filter
				int? riskStatusId = null;
				if (filterValue is MultiValueFilter)
				{
					MultiValueFilter multiValueFilter = (MultiValueFilter)filterValue;
					if (multiValueFilter.Values.Count == 1)
					{
						riskStatusId = multiValueFilter.Values[0];
					}
				}
				if (filterValue is Int32)
				{
					riskStatusId = (int)filterValue;
				}

				if (riskStatusId.HasValue)
				{
					//All Open
					if (riskStatusId == RiskStatusId_AllOpen)
					{
						List<int> ids = RiskStatus_RetrieveOpenIds(projectTemplateId.Value);
						ConstantExpression idsExpression = LambdaExpression.Constant(ids);
						MemberExpression memberExpression = LambdaExpression.PropertyOrField(p, "RiskStatusId");
						//Equivalent to: p => ids.Contains(p.RiskStatusId) i.e. (RISK_STATUS_ID IN (1,2,3))
						Expression expression = Expression.Call(idsExpression, "Contains", null, Expression.Convert(memberExpression, typeof(int)));
						expressionList.Add(expression);
						return true;
					}
					//All Closed
					if (riskStatusId == RiskStatusId_AllClosed)
					{
						List<int> ids = RiskStatus_RetrieveClosedIds(projectTemplateId.Value);
						ConstantExpression idsExpression = LambdaExpression.Constant(ids);
						MemberExpression memberExpression = LambdaExpression.PropertyOrField(p, "RiskStatusId");
						//Equivalent to: p => ids.Contains(p.RiskStatusId) i.e. (RISK_STATUS_ID IN (1,2,3))
						Expression expression = Expression.Call(idsExpression, "Contains", null, Expression.Convert(memberExpression, typeof(int)));
						expressionList.Add(expression);
						return true;
					}
				}
			}

			//By default, let the generic filter convertor handle the filter
			return false;
		}

		/// <summary>
		/// Adds on any complex filters to the LINQ query that cannot be done easily using Expressions
		/// </summary>
		/// <param name="filters">The filters</param>
		/// <param name="query">The query</param>
		/// <see cref="HandleRiskSpecificFilters"/>
		protected void HandleRiskSpecificFiltersEx(ref IQueryable<RiskView> query, Hashtable filters, SpiraTestEntitiesEx context)
		{
			//None at present
		}

		/// <summary>Validates a risk row prior to it being sent for update</summary>
		/// <param name="risk">The risk row being validated</param>
		/// <returns>A dictionary of validation messages</returns>
		public Dictionary<string, string> Validate(Risk risk)
		{
			const string METHOD_NAME = "Validate";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			Dictionary<string, string> messages = new Dictionary<string, string>();

			try
			{
				//Make sure that the closed date is not before the creation date(!)
				if (risk.ClosedDate.HasValue)
				{
					if (risk.CreationDate.Date > risk.ClosedDate.Value.Date)
					{
						if (!messages.ContainsKey("ClosedDate"))
						{
							messages.Add("ClosedDate", GlobalResources.Messages.Risk_ClosedDateCannotBeBeforeCreated);
						}
					}
				}

				//Make sure that the start date is not before the created date(!)
				//Since review date has no time, we add 24 hours of buffer to avoid annoying messages
				//when you create and review an risk on the same day
				if (risk.ReviewDate.HasValue && risk.CreationDate.Date > risk.ReviewDate.Value.Date.AddHours(24))
				{
					if (!messages.ContainsKey("StartDate"))
					{
						messages.Add("StartDate", GlobalResources.Messages.Risk_ReviewDateCannotBeBeforeCreated);
					}
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return messages;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Counts all the risks in the system</summary>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="includeDeleted">True if you want the count to return deleted risks as well.</param>
		/// <returns>The total number of Risks</returns>
		/// <remarks>Used to help with pagination</remarks>
		public int Risk_Count(int projectId, Hashtable filters, double utcOffset, bool includeDeleted = false)
		{
			const string METHOD_NAME = "Risk_Count";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int riskCount = 0;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from i in context.RisksView
								where (!i.IsDeleted || includeDeleted) && i.ProjectId == projectId
								select i;

					//Add the dynamic filters
					if (filters != null)
					{
						//See if we have any filters that need special handling, that cannot be done through Expressions
						HandleRiskSpecificFiltersEx(ref query, filters, context);

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
						Expression<Func<RiskView, bool>> filterClause = CreateFilterExpression<RiskView>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.Risk, filters, utcOffset, null, HandleRiskSpecificFilters);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<RiskView>)query.Where(filterClause);
						}
					}

					//Get the count
					riskCount = query.Count();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return riskCount;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the list of open risks (new, assigned or open) for a given owner irrespective of the project.</summary>
		/// <param name="ownerId">The ID of the person owning the risks (pass null to retrieve all unassigned)</param>
		/// <param name="releaseId">The id of the release (null for all)</param>
		/// <param name="numberRows">The number of rows to return</param>
		/// <returns>Risk list</returns>
		/// <remarks>
		/// 1) The risks are sorted by priority then type. Only displays for active projects
		/// 2) If you filter by release, it does NOT include items for the child iterations
		/// </remarks>
		/// <param name="projectId">The id of the project, or pass null for all</param>
		public List<RiskView> Risk_RetrieveOpenByOwnerId(int? ownerId, int? projectId, int? releaseId, int numberRows = 500)
		{
			const string METHOD_NAME = "Risk_RetrieveOpenByOwnerId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RiskView> risks;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create base query for retrieving the risk records
					var query = from i in context.RisksView
								where !i.IsDeleted && i.ProjectIsActive && i.RiskStatusIsOpen
								select i;

					//Add the project filter if necessary
					if (projectId.HasValue)
					{
						query = query.Where(i => i.ProjectId == projectId.Value);

						//Add the release filter if necessary
						if (releaseId.HasValue)
						{
							query = query.Where(i => i.ReleaseId == releaseId.Value);
						}
					}

					//Add the Owner filter
					if (ownerId.HasValue)
					{
						query = query.Where(i => i.OwnerId == ownerId.Value);
					}
					else
					{
						query = query.Where(i => !i.OwnerId.HasValue);
					}

					//Order by exposure then type
					query = query.OrderByDescending(i => i.RiskExposure).ThenBy(i => i.RiskId);

					//Execute the query
					risks = query.Take(numberRows).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return risks;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves all of the project risks open for a program
		/// </summary>
		/// <param name="projectGroupId">The id of the program</param>
		/// <param name="numberRowsToDisplay">The number of rows to display</param>
		/// <returns>The list of risks</returns>
		public List<RiskView> Risk_RetrieveOpenForGroup(int projectGroupId, int numberRowsToDisplay)
		{
			const string METHOD_NAME = "Risk_RetrieveOpenForGroup";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RiskView> risks;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create base query for retrieving the risk records
					var query = from r in context.RisksView
								where !r.IsDeleted && r.ProjectIsActive && r.ProjectProjectGroupId == projectGroupId && r.RiskStatusIsOpen
								orderby r.RiskExposure descending, r.RiskId
								select r;

					//Execute the query
					risks = query.Take(numberRowsToDisplay).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return risks;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves all of the project risks open for a portfolio
		/// </summary>
		/// <param name="portfolioId">The id of the portfolio</param>
		/// <param name="numberRowsToDisplay">The number of rows to display</param>
		/// <returns>The list of risks</returns>
		public List<RiskView> Risk_RetrieveOpenForPortfolio(int portfolioId, int numberRowsToDisplay)
		{
			const string METHOD_NAME = "Risk_RetrieveOpenForPortfolio";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RiskView> risks;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create base query for retrieving the risk records
					var query = from r in context.RisksView
								join p in context.ProjectGroups on r.ProjectProjectGroupId equals p.ProjectGroupId
								where
									!r.IsDeleted &&
									r.ProjectIsActive &&
									p.IsActive &&
									p.PortfolioId == portfolioId &&
									r.RiskStatusIsOpen
								orderby r.RiskExposure descending, r.RiskId
								select r;

					//Execute the query
					risks = query.Take(numberRowsToDisplay).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return risks;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Calculates the size of the risk matrix for the Project ID supplied
		/// </summary>
		/// <param name="ProjectId">The project ID for the Risk</param>
		/// <returns>Integer that repersents the risk matrix size (3x3, 4x4, 5x5)</returns>
		/// <remarks>
		/// Only includes active projects, programs and porfolios
		/// </remarks>
		public int Risk_MatrixSize(int ProjectId)
		{
			const string METHOD_NAME = "Risk_MatrixSize";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				int matrixSize = 0;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create base query for retrieving the risk records
					var queryImpact = (from ri in context.RiskImpacts
									   join p in context.Projects on ri.ProjectTemplateId equals p.ProjectTemplateId
									   where
										   ri.IsActive &&
										   p.IsActive &&
										   p.ProjectId == ProjectId
									   select ri.Name).Count();

					//Execute the query
					var riskImpact = ((uint)queryImpact);

					//Create base query for retrieving the risk impact record count
					var queryProbability = (from rp in context.RiskProbabilities
											join p in context.Projects on rp.ProjectTemplateId equals p.ProjectTemplateId
											where
												rp.IsActive &&
												p.IsActive &&
												p.ProjectId == ProjectId
											select rp.Name).Count();

					//Execute the query
					var riskProbability = ((uint)queryProbability);

					//Calculate which value to return
					if (riskImpact == riskProbability)
					{
						//If Equal, return the value.
						matrixSize = (int)riskImpact;
					}
					else
					{
						//if not Equal, return the smaller value.
						matrixSize = (riskImpact > riskProbability) ? (int)riskProbability : matrixSize = (int)riskImpact;
					}

					//Make sure the returned value is an expected value;
					matrixSize = matrixSize > 5 ? 5 : matrixSize;
					matrixSize = matrixSize < 3 ? 3 : matrixSize;
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return matrixSize;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves all of the project risks open for the system
		/// </summary>
		/// <param name="numberRowsToDisplay">The number of rows to display</param>
		/// <returns>The list of risks</returns>
		/// <remarks>
		/// Only includes active projects, programs and porfolios
		/// </remarks>
		public List<RiskView> Risk_RetrieveAllOpen(int numberRowsToDisplay)
		{
			const string METHOD_NAME = "Risk_RetrieveAllOpen";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RiskView> risks;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create base query for retrieving the risk records
					var query = from r in context.RisksView
								join p in context.ProjectGroups on r.ProjectProjectGroupId equals p.ProjectGroupId
								where
									!r.IsDeleted &&
									r.ProjectIsActive &&
									p.IsActive &&
									r.RiskStatusIsOpen &&
									(!p.PortfolioId.HasValue || p.Portfolio.IsActive)
								orderby r.RiskExposure descending, r.RiskId
								select r;

					//Execute the query
					risks = query.Take(numberRowsToDisplay).ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return risks;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves the list of open risks (new, assigned or open) for a given opener irrespective of the project.</summary>
		/// <param name="openerId">The ID of the person who detected/opened the risks</param>
		/// <param name="projectId">The id of the project, or null for all</param>
		/// <returns>Risk dataset</returns>
		/// <remarks>The risks are sorted by last updated date descending. Only displays for active projects</remarks>
		public List<RiskView> Risk_RetrieveOpenByCreatorId(int openerId, int? projectId)
		{
			const string METHOD_NAME = "Risk_RetrieveOpenByCreatorId";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RiskView> risks;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create base query for retrieving the risk records
					var query = from i in context.RisksView
								where !i.IsDeleted && i.ProjectIsActive && i.CreatorId == openerId && i.RiskStatusIsOpen
								select i;

					//Add the project filter if necessary
					if (projectId.HasValue)
					{
						query = query.Where(i => i.ProjectId == projectId.Value);
					}

					//Order by last updated date then id
					query = query.OrderByDescending(i => i.LastUpdateDate).ThenBy(i => i.RiskId);

					//Execute the query
					risks = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return risks;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a list of all risks in the system along with associated meta-data</summary>
		/// <param name="projectId">The project we're interested in</param>
		/// <param name="sortProperty">The property name to be sorted on</param>
		/// <param name="sortAscending">Whether to sort the data ascending</param>
		/// <param name="startRow">The first row to retrieve (starting at 1)</param>
		/// <param name="numberOfRows">The number of rows to retrieve</param>
		/// <param name="filters">The collection of filters - pass null if none specified</param>
		/// <param name="includeDeleted">Whether to include deleted items in the list or not.</param>
		/// <returns>Risk List</returns>
		/// <remarks>Also brings across any associated custom properties</remarks>
		public List<RiskView> Risk_Retrieve(int projectId, string sortProperty, bool sortAscending, int startRow, int numberOfRows, Hashtable filters, double utcOffset, bool includeDeleted = false)
		{
			const string METHOD_NAME = "Risk_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RiskView> risks;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from i in context.RisksView
								where (!i.IsDeleted || includeDeleted) && i.ProjectId == projectId
								select i;

					//Add the dynamic sort
					if (String.IsNullOrEmpty(sortProperty))
					{
						//Default to sorting by creation date descending
						query = query.OrderByDescending(i => i.CreationDate).ThenBy(i => i.RiskId);
					}
					else
					{
						//We always sort by the physical ID to guarantee stable sorting
						string sortExpression = sortProperty + " " + ((sortAscending) ? "ASC" : "DESC");
						query = query.OrderUsingSortExpression(sortExpression, "RiskId");
					}

					//Add the dynamic filters
					if (filters != null)
					{
						//See if we have any filters that need special handling, that cannot be done through Expressions
						HandleRiskSpecificFiltersEx(ref query, filters, context);

						//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
						int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
						Expression<Func<RiskView, bool>> filterClause = CreateFilterExpression<RiskView>(projectId, projectTemplateId, Artifact.ArtifactTypeEnum.Risk, filters, utcOffset, null, HandleRiskSpecificFilters);
						if (filterClause != null)
						{
							query = (IOrderedQueryable<RiskView>)query.Where(filterClause);
						}
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
						return new List<RiskView>();
					}

					//Execute the query
					risks = query
						.Skip(startRow - 1)
						.Take(numberOfRows)
						.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return risks;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		#endregion

		#region Risk Mitigation Functions
		/// <summary>
		/// Retrieves all the mitigations associated with a risk
		/// </summary>
		/// <param name="riskId">The id of the risk</param>
		/// <param name="includeDeleted">Should we include deleted items</param>
		/// <returns>The list of mitigations</returns>
		public List<RiskMitigation> RiskMitigation_Retrieve(int riskId, bool includeDeleted = false)
		{
			const string METHOD_NAME = "RiskMitigation_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RiskMitigation> riskMitigations;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RiskMitigations
								where r.RiskId == riskId && (!r.IsDeleted || includeDeleted)
										&& (!r.Risk.IsDeleted || includeDeleted)
								orderby r.Position, r.RiskMitigationId
								select r;

					riskMitigations = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return riskMitigations;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves all the deleted mitigations in a project
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The list of mitigations</returns>
		/// <remarks>Mainly used in the Undelete history functionality</remarks>
		protected internal List<RiskMitigation> RiskMitigation_RetrieveDeleted(int projectId)
		{
			const string METHOD_NAME = "RiskMitigation_RetrieveDeleted";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<RiskMitigation> riskMitigations;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RiskMitigations
								where r.IsDeleted && r.Risk.ProjectId == projectId
								orderby r.RiskId, r.RiskMitigationId
								select r;

					riskMitigations = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return riskMitigations;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Copies a single risk mitigation</summary>
		/// <param name="userId">The user making the copy</param>
		/// <param name="projectId">The current project</param>
		/// <param name="destRiskId">The risk that the mitigations should be copied to (leave null to copy to same)</param>
		/// <param name="sourceRiskMitigationId">The mitigation being copied</param>
		/// <param name="destRiskMitigationId">The mitigation in the destination risk that you want to insert it in front of (or null for last position)</param>
		/// <returns>The id of the newly created mitigation</returns>
		public int RiskMitigation_Copy(int userId, int projectId, int sourceRiskMitigationId, int? destRiskId = null, int? destRiskMitigationId = null)
		{
			const string METHOD_NAME = "RiskMitigation_Copy";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//First get the source mitigation
				RiskMitigation riskMitigation = this.RiskMitigation_RetrieveById(sourceRiskMitigationId);
				if (riskMitigation == null)
				{
					throw new ArtifactNotExistsException("Unable to locate risk mitigation " + sourceRiskMitigationId + " in the project. It no longer exists!");
				}

				if (!destRiskId.HasValue)
				{
					destRiskId = riskMitigation.RiskId;
				}

				//Insert the mitigation to the destination risk
				int newRiskMitigationId = this.RiskMitigation_Insert(
					projectId,
					destRiskId.Value,
					destRiskMitigationId,
					riskMitigation.Description,
					userId, null, null, true
					);

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return newRiskMitigationId;
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
		/// Deletes a single risk mitigation in the system
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="userId">The id of the user doing the delete</param>
		/// <param name="riskMitigationId">The id of the mitigation being deleted</param>
		/// <remarks>Soft-deletes the step only. Fails quietly if the step cannot be found</remarks>
		public void RiskMitigation_Delete(int projectId, int riskMitigationId, int userId)
		{
			const string METHOD_NAME = "RiskMitigation_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the mitigation and mark as deleted
					var query = from r in context.RiskMitigations
								where r.RiskMitigationId == riskMitigationId
								select r;

					RiskMitigation riskMitigation = query.FirstOrDefault();
					if (riskMitigation != null)
					{
						//Mark as deleted and persist
						riskMitigation.StartTracking();
						riskMitigation.IsDeleted = true;

						//Finally we need to update the last updated date of the requirement itself
						UpdateRiskLastUpdateDate(context, riskMitigation.RiskMitigationId);
						context.SaveChanges();
					}
				}

				//Now make a log history.
				new HistoryManager().LogDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.RiskMitigation, riskMitigationId, DateTime.UtcNow);

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
		/// Undeletes a single risk mitigation in the system
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="rollbackId">Pass a rollback id to update history during a rollback operation</param>
		/// <param name="userId">The id of the user making the change</param>
		/// <param name="riskMitigationId">The id of the mitigation being undeleted</param>
		/// <remarks>Throws an exception if the mitigation cannot be found</remarks>
		public void RiskMitigation_Undelete(int projectId, int riskMitigationId, int userId, long? rollbackId)
		{
			const string METHOD_NAME = "RiskMitigation_Undelete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the mitigation and mark as deleted
					var query = from r in context.RiskMitigations
								where r.RiskMitigationId == riskMitigationId
								select r;

					RiskMitigation riskMitigation = query.FirstOrDefault();
					if (riskMitigation == null)
					{
						throw new ArtifactNotExistsException(String.Format(GlobalResources.Messages.Risk_MitigationNotExists, riskMitigationId));
					}
					else
					{
						//Mark as deleted and persist
						riskMitigation.StartTracking();
						riskMitigation.IsDeleted = false;

						//Finally we need to update the last updated date of the risk itself
						UpdateRiskLastUpdateDate(context, riskMitigation.RiskId);
						context.SaveChanges();
					}
				}

				//Now make a log history.
				if (rollbackId.HasValue)
					new HistoryManager().LogUnDeletion(projectId, userId, DataModel.Artifact.ArtifactTypeEnum.RiskMitigation, riskMitigationId, rollbackId.Value, DateTime.UtcNow);

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
		/// Physically deletes/purges a deleted risk mitigation from the database
		/// </summary>
		/// <param name="projectId">The id of the project</param>
		/// <param name="riskMitigationId">The id of the mitigation to be purged</param>
		/// <param name="userId">The id of the user making the change</param>
		public void RiskMitigation_Purge(int projectId, int riskMitigationId, int userId)
		{
			const string METHOD_NAME = CLASS_NAME + "RiskMitigation_Purge()";
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
						RiskMitigation mitigationStep = ct.RiskMitigations
							.SingleOrDefault(s => s.RiskMitigationId == riskMitigationId);

						if (mitigationStep != null)
						{
							mitigationStep.ProjectId = projectId; //Needed for history.

							//Retrtieve the Requirement with all steps.
							int riskId = mitigationStep.RiskId;
							Risk risk = ct.Risks
								.Include(r => r.Mitigations)
								.SingleOrDefault(r => r.ProjectId == projectId &&
									r.RiskId == riskId);
							//Start recording,
							risk.StartTracking();

							//First we need to move up any of the other test-steps that are below the passed-in test step
							long? createSpaceChangeSetId = null; //Record for all the steps.
							foreach (RiskMitigation laterStep in risk.Mitigations.Where(g => g.Position > mitigationStep.Position))
							{
								laterStep.ProjectId = risk.ProjectId; //Needed for history.
								laterStep.StartTracking();
								int oldPos = laterStep.Position;
								laterStep.Position--;

								if (logBaseline)
									createSpaceChangeSetId = hMgr.RecordRiskStepPosition(
										risk.ProjectId,
										userId,
										risk.RiskId,
										risk.Name,
										laterStep.RiskMitigationId,
										oldPos,
										laterStep.Position,
										createSpaceChangeSetId
										);
							}

							//Update the last updated date of the test case
							risk.LastUpdateDate = DateTime.UtcNow;

							//Now log the purge
							hMgr.LogPurge(projectId, userId, Artifact.ArtifactTypeEnum.RiskMitigation, riskMitigationId, DateTime.UtcNow);
							if (logBaseline)
								hMgr.RecordRiskStepPosition(
									risk.ProjectId,
									userId,
									risk.RiskId,
									risk.Name,
									mitigationStep.RiskMitigationId,
									mitigationStep.Position,
									-1,
									createSpaceChangeSetId);

							//Remove the step.
							ct.RiskMitigations.DeleteObject(mitigationStep);

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
		/// Inserts a new risk mitigation
		/// </summary>
		/// <param name="riskId">The id of the risk we're adding the mitigation to</param>
		/// <param name="description">The description of the mitigation</param>
		/// <param name="creatorId">The id of the person adding the mitigation (for history records)</param>
		/// <param name="creationDate">The date the mitigation was added (defaults to the current date/time)</param>
		/// <param name="existingRiskMitigationId">The ID of the mitigation to insert in front of(passing null adds it to the end)</param>
		/// <param name="logHistory">Should we log a history entry</param>
		/// <param name="projectId">The id of the project</param>
		/// <returns>The id of the new mitigation</returns>
		/// <remarks>Also updates the last-updated-date of the parent risk</remarks>
		public int RiskMitigation_Insert(int projectId, int riskId, int? existingRiskStepId, string description, int creatorId, DateTime? creationDate = null, DateTime? reviewDate = null, bool logHistory = true)
		{
			const string METHOD_NAME = CLASS_NAME + "RiskMitigation_Insert()";
			Logger.LogEnteringEvent(METHOD_NAME);

			int returnId = -1;
			try
			{
				//Rewrote to more closely match TestCase Manager's new InsertStep()
				using (var ct = new SpiraTestEntitiesEx())
				{
					//History manager, in case.
					HistoryManager hMgr = new HistoryManager();

					//Get the flag if we need to record baseline changes. (Insert changes are still logged.)
					bool logBaseline = new ProjectSettings(projectId).BaseliningEnabled && Global.Feature_Baselines;

					//Get the Requirement with steps. (This is the same as calling TestCaseManager.RetrieveByIdWithSteps()
					Risk risk = ct.Risks
						.Include(r => r.Mitigations)
						.SingleOrDefault(r => r.RiskId == riskId);
					if (risk == null)
						throw new ArtifactNotExistsException();

					//Get the position we want to insert this step into. (Unlike the TestCaseManager function, which accepts a 
					//  position as a parameter, THIS function is being sent the ID that contains the position. Why? We don't know!
					int? position = null;
					if (existingRiskStepId.HasValue)
					{
						RiskMitigation existingStep = ct.RiskMitigations
							.SingleOrDefault(s => s.RiskId == riskId &&
								s.RiskMitigationId == existingRiskStepId);
						if (existingStep != null)
							position = existingStep.Position;
					}

					//If we have no position number, simply default to the next available position
					if (!position.HasValue || position < 1)
					{
						if (risk.Mitigations.Count == 0)
							position = 1;
						else
							position = risk.Mitigations.Max(g => g.Position) + 1;
					}

					//Begin transaction - needed to maintain integrity of position ordering
					using (TransactionScope ts = new TransactionScope())
					{
						//Start the update.
						risk.StartTracking();

						//Now we need to move down any of the other test-steps that are below the passed-in position
						long? createSpaceChangeSetId = null; //Record for all the steps.
						foreach (var laterStep in risk.Mitigations.Where(t => t.Position >= position.Value))
						{
							laterStep.StartTracking();
							int oldPos = laterStep.Position;
							laterStep.Position++;

							if (logBaseline)
								createSpaceChangeSetId = hMgr.RecordRiskStepPosition(
									risk.ProjectId,
									creatorId,
									risk.RiskId,
									risk.Name,
									laterStep.RiskMitigationId,
									oldPos,
									laterStep.Position,
									createSpaceChangeSetId
									);
						}

						//Populate the new entity
						RiskMitigation newMitigation = new RiskMitigation
						{
							RiskId = riskId,
							Description = description,
							CreationDate = (creationDate.HasValue) ? creationDate.Value : DateTime.UtcNow,
							ReviewDate = reviewDate,
							LastUpdateDate = DateTime.UtcNow,
							ConcurrencyDate = DateTime.UtcNow,
							Position = position.Value,
							IsDeleted = false
						};

						//Add the step to the table..
						ct.RiskMitigations.AddObject(newMitigation);

						//Update requirement's last updated date.
						risk.LastUpdateDate = DateTime.UtcNow;

						//Save changes!
						ct.SaveChanges();

						//Commit transaction - needed to maintain integrity of position ordering
						ts.Complete();

						//Get the ID.
						returnId = newMitigation.RiskMitigationId;
					}

					//Log history.
					if (logHistory)
						hMgr.LogCreation(projectId, creatorId, Artifact.ArtifactTypeEnum.RiskMitigation, returnId, DateTime.UtcNow);
					//See if we need to log positional changes (only in baselining.
					if (logBaseline)
					{
						if (hMgr == null) hMgr = new HistoryManager();

						hMgr.RecordRiskStepPosition(
							risk.ProjectId,
							creatorId,
							risk.RiskId,
							risk.Name,
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
		/// Moves a mitigation to a specific position in the list (or to the end)
		/// </summary>
		/// <param name="riskId">The risk that the mitigations belong to</param>
		/// <param name="sourceRiskStepId">The ID of the mitigation we want to move</param>
		/// <param name="destRiskStepId">The ID of the mitigation we want to move it in front of (or null to move to the end of the list)</param>
		/// <param name="userId">The user performing the move.</param>
		public void RiskMitigation_Move(int riskId, int sourceRiskStepId, int? destRiskStepId, int userId)
		{
			const string METHOD_NAME = CLASS_NAME + "RiskMitigation_Move";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				using (var ct = new SpiraTestEntitiesEx())
				{
					using (TransactionScope transactionScope = new TransactionScope())
					{
						// Pull the requirement with steps.
						Risk baseRisk = ct.Risks
							.Include(g => g.Mitigations)
							.SingleOrDefault(r => r.RiskId == riskId);

						//If it don't exist, throw an error (handled by RetrieveByIdWithSteps() in TestCaseManager).
						if (baseRisk == null)
							throw new ArtifactNotExistsException();

						//Make sure that we have the row to be moved in this dataset
						RiskMitigation mitigationToMove =
							baseRisk.Mitigations.SingleOrDefault(s => s.RiskMitigationId == sourceRiskStepId);
						if (mitigationToMove == null)
							throw new ArtifactNotExistsException("The passed in Rick id and mitigation id doesn't correspond to a matching step in the requirement!");
						//Now grab the test step at the destination.
						RiskMitigation mitigationAtDest = null;
						if (destRiskStepId.HasValue)
							mitigationAtDest = baseRisk.Mitigations.SingleOrDefault(s => s.RiskMitigationId == destRiskStepId.Value);
						// Record the initial Test Step position.
						int startingPosition = mitigationToMove.Position;
						int endingPosition = (!destRiskStepId.HasValue
							? baseRisk.Mitigations.Max(f => f.Position) //Since we're not adding, only move.
							: mitigationAtDest.Position); //If null, select the next highest.
						if (startingPosition < endingPosition &&
							destRiskStepId.HasValue)
							endingPosition--; //This is needed since the item goes in FRONT of the one selected, unless it's at the end.

						//This is needed for or WHERE clause.
						int lowPosition = Math.Min(startingPosition, endingPosition);
						int highPosition = Math.Max(startingPosition, endingPosition);

						//Check that the two items are not the same. If they are, nothing's changing.
						if (startingPosition == endingPosition) return;

						//Get the HistoryManager ifneeded.
						HistoryManager hMgr = null;
						bool recordHistory = new ProjectSettings(baseRisk.ProjectId).BaseliningEnabled && Global.Feature_Baselines;
						if (recordHistory)
							hMgr = new HistoryManager();

						//The flag, in case we need to save.
						bool saveChanges = false;

						//Pull all the Mitigations that are between (inclusive) of our range.
						//  Steps outside of our ranger aren't moving, see.
						ct.ContextOptions.LazyLoadingEnabled = false; //Turn this off so the query does not change as WE make changes.
						var mitigationList = ct.RiskMitigations
							.Include(s => s.Risk)
							.OrderBy(s => s.Position)
							.Where(s =>
								s.RiskId == riskId &&
								s.Position >= lowPosition &&
								s.Position <= highPosition)
							.ToList();
						ct.ContextOptions.LazyLoadingEnabled = true; //Re-enable, in case.

						//Loop through each one. 
						bool doIncrement = true; //Flag. Until we get to the new position, we INCREMENT the existing ones.
						long? recordHistoyChangeset = null; //In case we record history, we want to record this changeset.
						foreach (var step in mitigationList)
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
								recordHistoyChangeset = hMgr.RecordRiskStepPosition(
									baseRisk.ProjectId,
									userId,
									step.RiskId,
									step.Risk.Name,
									step.RiskMitigationId,
									oldPos,
									step.Position,
									recordHistoyChangeset);
						}

						//Update the TestCase date, and save all oour changes.
						if (saveChanges)
						{
							mitigationList[0].Risk.StartTracking();
							mitigationList[0].Risk.LastUpdateDate = DateTime.UtcNow;
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
		/// Updates a single risk mitigation in the system
		/// </summary>
		/// <param name="projectId">The id of the current project</param>
		/// <param name="updHistory">Should we add a history entry</param>
		/// <param name="rollbackId">Is this part of a rollback?</param>
		/// <param name="userId">The id of the user making the change</param>
		/// <param name="riskMitigation">The mitigation being updated</param>
		/// <remarks>Should not be used for position moves</remarks>
		public void RiskMitigation_Update(int projectId, RiskMitigation riskMitigation, int userId, long? rollbackId = null, bool updHistory = true)
		{
			const string METHOD_NAME = "RiskMitigation_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Apply the changes to the context
					context.RiskMitigations.ApplyChanges(riskMitigation);

					//Finally we need to update the last updated date of the risk itself
					UpdateRiskLastUpdateDate(context, riskMitigation.RiskMitigationId);

					//If we're updating history, we need to also provide the project id to the artifact
					riskMitigation.ProjectId = projectId;

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
		/// Retrieves a single risk mitigation by its ID
		/// </summary>
		/// <param name="riskMitigationId">The id of the mitigation</param>
		/// <param name="populateProject">Should we populate the project id (used in rollbacks only)</param>
		/// <param name="includeDeleted">Should we include deleted mitigations</param>
		/// <returns>The mitigation or NULL if not found</returns>
		public RiskMitigation RiskMitigation_RetrieveById(int riskMitigationId, bool includeDeleted = false, bool populateProject = false, bool includeRisk = false)
		{
			const string METHOD_NAME = "RiskMitigation_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				RiskMitigation riskMitigation;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from r in context.RiskMitigations
								select r;
					if (includeRisk)
						query = from r in context.RiskMitigations.Include(m => m.Risk)
								select r;

					query = query.Where(r =>
						r.RiskMitigationId == riskMitigationId &&
							(!r.IsDeleted || includeDeleted) &&
							(!r.Risk.IsDeleted || includeDeleted));

					riskMitigation = query.FirstOrDefault();

					if (populateProject && riskMitigation != null)
					{
						var query2 = from r in context.Risks
									 where r.RiskId == riskMitigation.RiskId
									 select new { r.ProjectId };

						riskMitigation.ProjectId = query2.FirstOrDefault().ProjectId;
					}

				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return riskMitigation;
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
