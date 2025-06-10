using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System.Text.RegularExpressions;
using static Inflectra.SpiraTest.DataModel.Artifact;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>
	/// Responsible for managing portfolios of programs in the system
	/// </summary>
	public class PortfolioManager : ManagerBase
	{
		private const string CLASS_NAME = "Inflectra.SpiraTest.Business.PortfolioManager::";

		/// <summary>
		/// Retrieves a list of portfolios in the system
		/// </summary>
		/// <param name="activeOnly">Should we only return active ones</param>
		/// <returns>List of portfolios</returns>
		public List<Portfolio> Portfolio_Retrieve(bool activeOnly = true)
		{
			const string METHOD_NAME = "Portfolio_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<Portfolio> portfolios;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.Portfolios
								where (p.IsActive || !activeOnly)
								orderby p.Name, p.PortfolioId
								select p;

					portfolios = query.ToList();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return portfolios;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Updates the completion metrics of the specified portfolio
		/// </summary>
		/// <param name="portfolioId">The id of the portfolio</param>
		protected internal void RefreshRequirementCompletion(int portfolioId)
		{
			const string METHOD_NAME = "RefreshRequirementCompletion";
			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.Portfolio_RefreshRequirementCompletion(portfolioId);
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
		/// Updates an updated portfolio object that is passed-in
		/// </summary>
		/// <param name="portfolio">The entity to be persisted</param>
		public void Portfolio_Update(Portfolio portfolio, int? userId = null, int? adminSectionId = null, string action = null)
		{
			const string METHOD_NAME = "Portfolio_Update";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Attach the entity to the context and save changes
					context.Portfolios.ApplyChanges(portfolio);
					context.AdminSaveChanges(userId, portfolio.PortfolioId, null, adminSectionId, action, true, true, null);
					context.SaveChanges();
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
		/// Retrieves a single portfolio by its ID
		/// </summary>
		/// <param name="portfolioId">The ID of the portfolio</param>
		/// <returns>The portfolio or NULL if not found</returns>
		public Portfolio Portfolio_RetrieveById(int portfolioId)
		{
			const string METHOD_NAME = "Portfolio_RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				Portfolio portfolio;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.Portfolios
								where p.PortfolioId == portfolioId
								select p;

					portfolio = query.FirstOrDefault();
				}

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return portfolio;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Retrieves a list of all portfolios that match the passed-in filter
		/// </summary>
		/// <param name="filters">The hashtable of filters to apply to the portfolio list</param>
		/// <param name="sortExpression">The sort expression to use, pass null for default</param>
		/// <returns>List of portfolios</returns>
		/// <remarks>
		/// Pass filters = null for all projects.
		/// The filters supported are for name, description, and active flag only
		/// </remarks>
		public List<Portfolio> Portfolio_Retrieve(Hashtable filters, string sortExpression)
		{
			const string METHOD_NAME = "Portfolio_Retrieve"; 

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				List<Portfolio> portfolios;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the base query

					//Create select command for retrieving all the portfolios
					var query = from p in context.Portfolios
								select p;

					//Now add any specified filters
					if (filters != null)
					{
						//Project Name
						if (filters["Name"] != null)
						{
							//Break up the name into keywords
							MatchCollection keywordMatches = Regex.Matches((string)filters["Name"], Common.Global.REGEX_KEYWORD_MATCHER);
							foreach (Match keywordMatch in keywordMatches)
							{
								string keyword = keywordMatch.Value.Replace("\"", "");
								query = query.Where(p => p.Name.Contains(keyword));
							}
						}

						//Description
						if (filters["Description"] != null)
						{
							//Break up the website into keywords
							MatchCollection keywordMatches = Regex.Matches((string)filters["Description"], Common.Global.REGEX_KEYWORD_MATCHER);
							foreach (Match keywordMatch in keywordMatches)
							{
								string keyword = keywordMatch.Value.Replace("\"", "");
								query = query.Where(p => p.Description.Contains(keyword));
							}
						}

						//Active Flag (check for both Y/N form and the newer boolean form)
						if (filters["ActiveYn"] != null && filters["ActiveYn"] is String)
						{
							bool isActive = ((string)filters["ActiveYn"] == "Y");
							query = query.Where(p => p.IsActive == isActive);
						}
						if (filters["IsActive"] != null)
						{
							bool isActive;
							if (filters["IsActive"] is Boolean)
							{
								isActive = (bool)filters["IsActive"];
							}
							else
							{
								isActive = ((string)filters["IsActive"] == "Y");
							}
							query = query.Where(p => p.IsActive == isActive);
						}

						//Portfolio ID
						if (filters["PortfolioId"] != null)
						{
							//The value might be an Int32 or String, so use ToString() to be on the safe side
							//Need to make sure that the project group id is numeric
							int portfolioId;
							if (Int32.TryParse(filters["PortfolioId"].ToString(), out portfolioId))
							{
								query = query.Where(p => p.PortfolioId == portfolioId);
							}
						}
					}

					//Add the sorts and execute
					if (String.IsNullOrEmpty(sortExpression))
					{
						query = query.OrderBy(p => p.Name).ThenBy(p => p.PortfolioId);
					}
					else
					{
						query = query.OrderUsingSortExpression(sortExpression, "PortfolioId");
					}
					portfolios = query.ToList();
				}

				//Return the dataset
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return portfolios;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Inserts a new portfolio in the system
		/// </summary>
		/// <param name="name">The name of the portfolio</param>
		/// <param name="description">The description of the portfolio</param>
		/// <param name="isActive">Is the portfolio active</param>
		/// <returns>The id of the new portfolio</returns>
		public int Portfolio_Insert(string name, string description, bool isActive, int? userId = null, int? adminSectionId = null, string action = null, bool logHistory = true)
		{
			const string METHOD_NAME = "Portfolio_Insert";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//AdminAudit manager, in case.
				AdminAuditManager adminAuditManager = new AdminAuditManager();
				TST_ADMIN_HISTORY_DETAILS_AUDIT details = new TST_ADMIN_HISTORY_DETAILS_AUDIT();
				int portfolioId;
				string newValue = String.Empty;

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the new object
					Portfolio portfolio = new Portfolio();
					portfolio.Name = name;
					portfolio.Description = description;
					portfolio.IsActive = isActive;
					portfolio.RequirementCount = 0;

					//Now we need to save the portfolio
					context.Portfolios.AddObject(portfolio);
					context.SaveChanges();
					portfolioId = portfolio.PortfolioId;

					newValue = portfolio.Name;


					details.NEW_VALUE = newValue;

				}

				//Log history.
				if (logHistory)
					adminAuditManager.LogCreation1(Convert.ToInt32(userId), Convert.ToInt32(adminSectionId), portfolioId, action, details, DateTime.UtcNow, ArtifactTypeEnum.Portfolios, "PortfolioId");

				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);

				return portfolioId;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Deletes a portfolio from the system
		/// </summary>
		/// <param name="portfolioId">The portfolio to be deleted</param>
		public void Portfolio_Delete(int portfolioId, int? userId = null)
		{
			const string METHOD_NAME = "Portfolio_Delete";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Make sure the portfolio and exists
					var query = from p in context.Portfolios.Include(p => p.ProjectGroups)
								where p.PortfolioId == portfolioId
								select p;

					Portfolio portfolio = query.FirstOrDefault();
					if (portfolio != null)
					{
						//Now we need to delete the portfolio
						context.Portfolios.DeleteObject(portfolio);
						Business.AdminAuditManager adminAuditManager = new Business.AdminAuditManager();
						string adminSectionName = "View / Edit Portfolios";
						var adminSection = adminAuditManager.AdminSection_RetrieveByName(adminSectionName);

						int adminSectionId = adminSection.ADMIN_SECTION_ID;

						//Add a changeset to mark it as deleted.
						new AdminAuditManager().LogDeletion1((int)userId, portfolio.PortfolioId, portfolio.Name, adminSectionId, "Portfolio Deleted", DateTime.UtcNow, ArtifactTypeEnum.Portfolios, "PortfolioId");
						context.SaveChanges();
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

		/// <summary>Retrieves a single portfolio record by portfolio id</summary>
		/// <param name="portfolioId">The ID of the portfolio to retrieve</param>
		/// <returns>A portfolio entity</returns>
		public Portfolio RetrieveById(int portfolioId)
		{
			const string METHOD_NAME = "RetrieveById";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

			try
			{
				//Create select command for retrieving the project record
				Portfolio portfolio;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from p in context.Portfolios
								where p.PortfolioId == portfolioId
								select p;

					portfolio = query.FirstOrDefault();
				}

				//Throw an exception if the project record is not found
				if (portfolio == null)
				{
					throw new ArtifactNotExistsException("Portfolio " + portfolioId + " doesn't exist in the system");
				}

				//Return the portfolio
				Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
				return portfolio;
			}
			catch (System.Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}
	}
}
