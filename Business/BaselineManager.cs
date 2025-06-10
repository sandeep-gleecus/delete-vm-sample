using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using static Inflectra.SpiraTest.DataModel.Artifact;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>This manager handles all Baseline functions.</summary>
	public class BaselineManager : ManagerBase
	{
		private const string CLASS_NAME = "Business.BaselineManager::";

		#region Retrieval Functions
		/// <summary>Returns the number of Baelines found in the Project and Release. If ReleaseId is null, ALL found in the project.</summary>
		/// <param name="productId">The product to pull for.</param>
		/// <param name="releaseId">The release to seach for.</param>
		/// <returns>A number of baselines found.</returns>
		public int Baseline_Count(int productId, int? releaseId = null)
		{
			const string METHOD_NAME = CLASS_NAME + "Baselines_Count()";
			Logger.LogEnteringEvent(METHOD_NAME);

			int retCount = 0;

			try
			{
				using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
				{
					var query = ct.ProjectBaselines.Where(bl => bl.ProjectId == productId);
					if (releaseId.HasValue)
						query = query.Where(g => g.ReleaseId == releaseId.Value);

					retCount = query.Count();
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(METHOD_NAME, ex);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retCount;
		}

		/// <summary>Retrieves a list of all the baselines in the product.</summary>
		/// <param name="productId">The id of the product the builds are in</param>
		/// <returns>A list of Baselines, or an empty list if exception or none exist.</returns>
		public List<ProjectBaseline> Baseline_RetrieveForProduct(int productId)
		{
			const string METHOD_NAME = CLASS_NAME + "RetrieveForProject()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//The returning list.
			List<ProjectBaseline> retList = new List<ProjectBaseline>();

			try
			{
				using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
				{
					retList = ct.ProjectBaselines
						.Include(b => b.Creator)
						.Include(b => b.Creator.Profile)
						.Include(b => b.Release)
						.Where(b => b.ProjectId == productId)
						.ToList();
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(METHOD_NAME, ex);
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		/// <summary>Retrieves a list of all the baselines in the product for the release.</summary>
		/// <param name="productId">The id of the product the builds are in</param>
		/// <param name="releaseId">The release ID to pull Baselines for.</param>
		/// <returns>A list of Baselines, or an empty list if exception or none exist.</returns>
		public List<ProjectBaseline> Baseline_RetrieveForProductRelease(int productId, long releaseId)
		{
			const string METHOD_NAME = CLASS_NAME + "Baselines_RetrieveForProductRelease()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//The returning list.
			List<ProjectBaseline> retList = new List<ProjectBaseline>();

			try
			{
				using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
				{
					retList = ct.ProjectBaselines
						.Include(b => b.Creator)
						.Where(b => b.ProjectId == productId && b.ReleaseId == releaseId)
						.ToList();
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(METHOD_NAME, ex);
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

		/// <summary>Retrieves baselines meeting the given criteria.</summary>
		/// <param name="productId">The ProductId to search in for Baselines. If null, any product.</param>
		/// <param name="releaseId">The ReleaseId to search in for Baselined. If null, any release.</param>
		/// <param name="filters"></param>
		/// <returns></returns>
		public List<ProjectBaseline> Baseline_RetrieveFilteredForProductRelease(int? productId, int? releaseId, SortFilter filters, double utcOffset, out int prePageCount)
		{
			const string METHOD_NAME = CLASS_NAME + "Baseline_RetrieveFilteredForProductRelease()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//The return list of data.
			List<ProjectBaseline> retList = new List<ProjectBaseline>();
			prePageCount = 0;

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from b in context.ProjectBaselines
									.Include(c => c.Creator)
									.Include(c => c.Creator.Profile)
									.Include(c => c.Release)
								select b;
					if (productId.HasValue)
						query = query.Where(b => b.ProjectId == productId.Value);
					if (releaseId.HasValue)
						query = query.Where(b => b.ReleaseId == releaseId.Value);

					//Add the dynamic filters
					//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
					Expression<Func<ProjectBaseline, bool>> filterClause =
						CreateFilterExpression<ProjectBaseline>(
							productId,
							null,
							Artifact.ArtifactTypeEnum.None,
							filters.FilterList,
                            utcOffset,
                            null,
							null);
					if (filterClause != null)
						query = (IOrderedQueryable<ProjectBaseline>)query.Where(filterClause);

					//Add the dynamic sort
					if (string.IsNullOrWhiteSpace(filters.SortProperty))
						query = query.OrderByDescending(b => b.CreationDate);
					else
						query = query.OrderUsingSortExpression(filters.SortExpression, "CreationDate");

					prePageCount = query.Count();

					//Execute the query
					retList = query
						.Skip(filters.StartingRowNumber - 1)
						.Take(filters.PageSize)
						.ToList();
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(METHOD_NAME, ex);
			}

			Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
			return retList;
		}

		/// <summary>Pulls a single baseline by it's ID.</summary>
		/// <param name="baselineId">The baseline requested, or NULL if not found.</param>
		public ProjectBaseline Baseline_RetrieveById(int baselineId)
		{
			const string METHOD_NAME = CLASS_NAME + "Baseline_RetrieveById()";
			Logger.LogEnteringEvent(METHOD_NAME);

			ProjectBaseline retValue = null;

			try
			{
				using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
				{
					retValue = ct.ProjectBaselines.
						Include(bb => bb.Creator).
						Include(bb => bb.Creator.Profile).
						Include(bb => bb.Release).
						SingleOrDefault(y => y.BaselineId == baselineId);
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(METHOD_NAME, ex);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retValue;
		}

		/// <summary>Returns a list of all the releases in the product that has one or more Baselines assigned to it.</summary>
		/// <param name="productId">The project/product ID.</param>
		/// <returns>A list of releases.</returns>
		public List<Release> Baselines_RetrieveReleasesForProject(int productId)
		{
			const string METHOD_NAME = CLASS_NAME + "Baselines_RetrieveReleasesForProject()";
			Logger.LogEnteringEvent(METHOD_NAME);

			List<Release> retList = new List<Release>();

			using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
			{
				retList = ct.Releases
					.Where(r => r.Baselines.Count > 0)
					.ToList();
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}

        /// <summary>This will return a list of all artifacts that were changed between the two Changesets.</summary>
        /// <param name="productId">The Product ID.</param>
        /// <param name="recentBaseline">The latest change to include.</param>
        /// <param name="previousBaseline">The lower limit of changes. If null, returns from the start of the Product.</param>
        /// <returns>The list of net changes</returns>
        /// <remarks>
        /// This version is private since we don't use this version and we don't currently unit test it
        /// </remarks>
        public List<HistoryChangeSetNetChangeSquashed> Artifacts_ChangedBetweenChangesets(
            int productId,
            int recentBaseline,
            SortFilter filters,
            double utcOffset,
            out int count)
        {
            return this.Artifacts_ChangedBetweenChangesets(productId, recentBaseline, null, filters, utcOffset, out count);
        }

        /// <summary>This will return a list of all artifacts that were changed between the two Changesets.</summary>
        /// <param name="productId">The Product ID.</param>
        /// <param name="recentBaseline">The latest change to include.</param>
        /// <param name="previousBaseline">The lower limit of changes. If null, returns from the start of the Product.</param>
        /// <param name="count">The count of changes before filtering</param>
        /// <returns>The list of net changes</returns>
        /// <remarks>
        /// This version is private since we don't use this version and we don't currently unit test it
        /// </remarks>
        private List<HistoryChangeSetNetChangeSquashed> Artifacts_ChangedBetweenChangesets(
			int productId, 
			int recentBaseline, 
			int? previousBaseline, 
			SortFilter filters,
            double utcOffset,
            out int count)
		{
			const string METHOD_NAME = CLASS_NAME + "Artifacts_ChangedBetweenChangesets()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//Our return list.
			List<HistoryChangeSetNetChangeSquashed> retList = new List<HistoryChangeSetNetChangeSquashed>();

			using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
			{
				//Pull from the view.
				var query = ct.HistoryChangeSetNetChangeSquasheds
					.Where(b => b.ProjectId == productId);

				//use an equal or between, depending on what we're looking for.
				if (previousBaseline.HasValue)
				{
					query = query
						.Where(b => b.BaselineId <= recentBaseline &&
							b.BaselineId > previousBaseline.Value
						);
				}
				else
				{
					query = query
						.Where(b => b.BaselineId == recentBaseline);
				}

                //Add the dynamic sort
                if (filters == null || string.IsNullOrWhiteSpace(filters.SortProperty))
				{
					//Default to sorting by creation date descending
					query = query
						.OrderByDescending(i => i.ChangeDate)
						.ThenByDescending(i => i.ArtifactTypeId)
						.ThenByDescending(i => i.ChangedArtifactId);
				}
				else
				{
					//We always sort by the physical ID to guarantee stable sorting
					query = query.OrderUsingSortExpression(filters.SortExpression, "ChangeDate");
				}

				//Add the dynamic filters
				if (filters != null)
				{
                    //If we are filterby by ArtifactType = (None), that does not work, so need to remove it
                    if (filters.FilterList != null && filters.FilterList.ContainsKey("ArtifactTypeId") && filters.FilterList["ArtifactTypeId"] is Int32 && (int)(filters.FilterList["ArtifactTypeId"]) == NoneFilterValue)
                    {
                        filters.FilterList.Remove("ArtifactTypeId");
                    }

					//Convert the remaining filters into the corresponding LINQ expressions using the generic conversion code
					var filterClause = CreateFilterExpression<HistoryChangeSetNetChangeSquashed>(
						productId,
						null,
						ArtifactTypeEnum.None,
						filters.FilterList,
                        utcOffset,
						null);

					if (filterClause != null)
						query = query.Where(filterClause);
				}

				//Get the total number of records in this query. This is so we can 'paginate our
				//  request from SQL Server.
				count = query.Count();

                //Check pagination values. (Make sure we're not asking for something outside
                //  of the rage we're looking for. 
                //Make pagination is in range
                if (filters == null)
                {
                    retList = query.ToList();
                }
                else
                {
                    if (filters.StartingRowNumber < 1 || filters.StartingRowNumber > count - 1)
                    {
                        filters.StartingPage = 1;
                    }

                    retList = query
                        .Skip(filters.StartingRowNumber - 1)
                        .Take(filters.PageSize)
                        .ToList();
                }
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retList;
		}
		#endregion Retrieval Functions

		#region Creation Functions
		/// <summary>Inserts the baseline into the database.</summary>
		/// <param name="newBase">The baseline to create.</param>
		/// <returns>The inserted baseline.</returns>
		/// <throws>ArgumentException</throws>
		public ProjectBaseline Baseline_Insert(ProjectBaseline newBase)
		{
			const string METHOD_NAME = CLASS_NAME + "Baseline_Insert()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
				{
					//Check that the given Project and Release are in the same project.
					bool isGood = ct.Releases
						.Any(r => r.ReleaseId == newBase.ReleaseId && r.ProjectId == newBase.ProjectId);

					if (isGood)
					{

						ct.ProjectBaselines.AddObject(newBase);
						ct.SaveChanges();
						//ct.SaveChanges(newBase.CreatorUserId, true, true, null);
					}
					else
						throw new ArgumentException("Release does not exist in project.", "ProjectBaseline.ReleaseId");
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(METHOD_NAME, ex);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return newBase;
		}

		/// <summary>Inserts the baseline into the database.</summary>
		/// <param name="changeSetId">The changeset's ID that this is linked to.</param>
		/// <param name="creatorId">The ID of the creator.</param>
		/// <param name="description">The description of the baseline.</param>
		/// <param name="name">The name of the baseline.</param>
		/// <param name="productId">The product ID of the baseline.</param>
		/// <param name="releaseId">The relase that the baseline is attached to.</param>
		/// <returns>The inserted baseline.</returns>
		/// <throws>ArgumentException</throws>
		public ProjectBaseline Baseline_Insert(long changeSetId, int creatorId, string name, string description, int productId, int releaseId)
		{
			const string METHOD_NAME = CLASS_NAME + "Baseline_Insert()";
			Logger.LogEnteringEvent(METHOD_NAME);

			//The date to use for the date fields.
			DateTime newDate = DateTime.UtcNow;

			ProjectBaseline newBase = new ProjectBaseline
			{
				ChangeSetId = changeSetId,
				ConcurrencyDate = newDate,
				CreationDate = newDate,
				CreatorUserId = creatorId,
				Description = description,
				IsActive = true,
				IsDeleted = false,
				IsApproved = true,
				LastUpdateDate = newDate,
				Name = name,
				ProjectId = productId,
				ReleaseId = releaseId
			};
			Baseline_Insert(newBase);

			Logger.LogExitingEvent(METHOD_NAME);
			return newBase;
		}

		/// <summary>
		/// Creates a new Baseline. Similar to <see cref="Baseline_Insert(long, int, string, string, int, int)"/> but 
		/// this function will pull the appropriate ChangeSet ID to link against.
		/// </summary>
		/// <param name="creatorId">The ID of the creator.</param>
		/// <param name="description">The description of the baseline.</param>
		/// <param name="name">The name of the baseline.</param>
		/// <param name="productId">The product ID of the baseline.</param>
		/// <param name="releaseId">The relase that the baseline is attached to.</param>
		/// <returns></returns>
		/// <throws>ArgumentException</throws>
		public ProjectBaseline Baseline_Create(int creatorId, string name, string description, int productId, int releaseId)
		{
			const string METHOD_NAME = CLASS_NAME + "Baseline_Create()";
			Logger.LogEnteringEvent(METHOD_NAME);

			ProjectBaseline retBase = null;

			//We need to get the highest changeset # from the database, first.,
			long changeSetId = 0;
			using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
			{
				changeSetId = ct.HistoryChangeSetsView
					.Where(h => h.ProjectId == productId)
					.Max(h => h.ChangeSetId);
			}
			//using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
			//{
			//	changeSetId = ct.HistoryChangeSets
			//		.Where(h => h.ProjectId == productId)
			//		.Select(h => h.ChangeSetId)
			//		.DefaultIfEmpty(0)  // Provide default value if no records are found
			//		.Max();
			//}
			if (changeSetId > 0)
			{
				retBase = Baseline_Insert(changeSetId, creatorId, name, description, productId, releaseId);
			}
			else
			{
				//No changesets. Throw an exception.
				throw new InvalidOperationException(GlobalResources.Messages.BaselineManager_CannotCreateWithoutChangeset);
			}
			new HistoryManager().LogCreation(productId, creatorId, DataModel.Artifact.ArtifactTypeEnum.ProjectBaseline, retBase.BaselineId, DateTime.UtcNow);
			Logger.LogEnteringEvent(METHOD_NAME);
			return retBase;
		}
		#endregion Creation Functions

		#region Update Functions
		/// <summary>Updates the specified Baseline.</summary>
		/// <param name="baselineId">The ID of the baseline to update.</param>
		/// <param name="newName">The baseline's new name.</param>
		/// <param name="newDesc">The baseline's new description.</param>
		/// <param name="newIsActive">The baseline's new IsActive flag.</param>
		/// <returns></returns>
		public ProjectBaseline Baseline_Update(int baselineId, string newName, string newDesc, bool newIsActive, int? userId = null)
		{
			const string METHOD_NAME = CLASS_NAME + "Baseline_Update()";
			Logger.LogEnteringEvent(METHOD_NAME);

			ProjectBaseline retVal = null;

			try
			{
				using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
				{
					//Get the existing one, first.
					retVal = ct.ProjectBaselines
						.SingleOrDefault(b => b.BaselineId == baselineId);

					if (retVal != null)
					{
						bool updReq = false;
						retVal.StartTracking();

						//Update the name, if necessary.
						if (!string.IsNullOrWhiteSpace(newName) && retVal.Name != newName)
						{
							retVal.Name = newName;
							updReq = true;
						}

						//Update the description, if necessary.
						if (!string.IsNullOrWhiteSpace(newDesc) && retVal.Description != newDesc)
						{
							retVal.Description = newDesc;
							updReq = true;
						}

						//Update the IsActive flag..
						if (retVal.IsActive != newIsActive)
						{
							retVal.IsActive = newIsActive;
							updReq = true;
						}

						//Save changes.
						if (updReq)
							ct.SaveChanges(userId, true, false, null);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(METHOD_NAME, ex);
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
			return retVal;
		}
		#endregion Update Functions

		#region Delete Functions
		/// <summary>Delets the given baseline from the system.</summary>
		/// <param name="programId">The program ID where this baseline exists in.</param>
		/// <param name="baselineId">The ID of the baseline to delete.</param>
		/// <returns>True if the baseline was deleted. False if no baseline existed</returns>
		public bool Baseline_Delete(int programId, int baselineId, int? userId = null)
		{
			const string METHOD_NAME = CLASS_NAME + "Baseline_Delete()";
			Logger.LogEnteringEvent(METHOD_NAME);

			bool retVal = false;

			try
			{
				//We need to get the highest changeset # from the database, first.,
				using (SpiraTestEntitiesEx ct = new SpiraTestEntitiesEx())
				{
					ProjectBaseline baseToDelete = ct.ProjectBaselines
						.SingleOrDefault(b => b.ProjectId == programId && b.BaselineId == baselineId);

					if (baseToDelete != null)
					{
						ct.ProjectBaselines.DeleteObject(baseToDelete);
						ct.SaveChanges();
						retVal = true;

						new HistoryManager().LogDeletion(programId, (int)userId, DataModel.Artifact.ArtifactTypeEnum.ProjectBaseline, baselineId, DateTime.UtcNow, null, null, baseToDelete.Name);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(METHOD_NAME, ex);
				throw;
			}

			Logger.LogEnteringEvent(METHOD_NAME);
			return retVal;
		}
		#endregion Delet Functions
	}
}
