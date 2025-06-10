using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>
	/// Responsible to displaying a list of events from the System Event Log
	/// </summary>
	/// <see cref="Inflectra.SpiraTest.Common.Logger"/>
	/// <remarks>
	/// The actual logging of results is done through the Logger static class in the Common assembly
	/// </remarks>
	public class EventManager : ManagerBase
	{
		private const string CLASS_NAME = "Business.EventManager::";

		/// <summary>
		/// Retrieves a list of all the active Event Log entry types
		/// </summary>
		/// <returns></returns>
		public List<EventType> GetTypes()
		{
			const string METHOD_NAME = CLASS_NAME + "GetTypes()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				List<EventType> types;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from e in context.EventTypes
								where e.IsActive
								select e;
					types = query.ToList();
				}

				Logger.LogExitingEvent(METHOD_NAME);
				return types;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Clears all events in the system</summary>
		/// <param name="userName">The user the cleared the logs, if known.</param>
		/// <param name="logClear">If true, will add an entry record immediately after the log was cleared.</param>
		public void Clear(string userName = null, bool logClear = true)
		{
			const string METHOD_NAME = CLASS_NAME + "Clear()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Call the stored proc to delete events
					context.Logger_DeleteOld(DateTime.UtcNow.AddDays(1));

					//If the flag is true, let us record a message.
					if (logClear)
					{
						if (string.IsNullOrWhiteSpace(userName)) userName = GlobalResources.Messages.Admin_EventLog_ClearUserName;

						string msgFormat = string.Format(
							GlobalResources.Messages.Admin_EventLog_ClearUserNameMessage,
							userName);
						Logger.LogInformationalEvent(METHOD_NAME, msgFormat);
					}
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}

		/// <summary>
		/// Retrieves a sorted, filterable list of system events
		/// </summary>
		/// <param name="pageSize">How many events to return in a page</param>
		/// <param name="pageIndex">Which page index to return (zero-based)</param>
		/// <param name="totalRecords">How many total events we have</param>
		/// <param name="filters">The list of filters</param>
		/// <param name="utcOffset">The timezone offset from UTC</param>
		/// <param name="sortExpression">The sort expression (column [ASC|DESC])</param>
		/// <returns>The event collection</returns>
		public List<Event> GetEvents(Hashtable filters, string sortExpression, int pageIndex, int pageSize, double utcOffset, out int totalRecords)
		{
			const string METHOD_NAME = "GetEvents";

			try
			{
				List<Event> events;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Build the base query
					var query = from e in context.Events.Include("Type")
								select e;

					//Add the dynamic filters
					if (filters != null)
					{
						Expression<Func<Event, bool>> filterExpression = CreateFilterExpression<Event>(null, null, Artifact.ArtifactTypeEnum.None, filters, utcOffset);
						if (filterExpression != null)
						{
							query = (IOrderedQueryable<Event>)query.Where(filterExpression);
						}
					}

					//Add the dynamic sorts
					//Always sort by event id at the end to ensure stable sorting
					query = query.OrderUsingSortExpression(sortExpression, "EventId");

					//Make sure pagination is in range
					totalRecords = query.Count();
					if (pageIndex > totalRecords - 1)
					{
						pageIndex = (int)totalRecords - pageSize;
					}
					if (pageIndex < 0)
					{
						pageIndex = 0;
					}

					events = query
						.Skip(pageIndex)
						.Take(pageSize)
						.ToList();
				}

				return events;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}
	}
}
