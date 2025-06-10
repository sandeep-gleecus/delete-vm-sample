using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Inflectra.SpiraTest.Business
{
	/// <summary>This class encapsulates all the data access functionality for sending Notifications.</summary>
	public class NotificationManager : ManagerBase
	{
		private const string CLASS = "Business.NotificationManager::";

		//Regex for template tokens..
		public static Regex regTempl = new Regex(@"\$\{(.*?)\}", RegexOptions.Compiled | RegexOptions.CultureInvariant);

		//Cached notification events and templates
		private static Dictionary<int, NotificationEvent> cachedEvents = new Dictionary<int, NotificationEvent>();
		private static Dictionary<string, NotificationArtifactTemplate> cachedTemplates = new Dictionary<string, NotificationArtifactTemplate>();

		#region Sub-Classes

		/// <summary>Class for message details sending..</summary>
		public class EmailMessageDetails
		{
			/// <summary>Creates an instance of the class.</summary>
			public EmailMessageDetails()
			{
				subjectList = new Dictionary<int, string>();
				toUserList = new List<EmailMessageDetailUser>();
				artifactTokens = new Dictionary<int, string>();
				fromUser = null;
			}

			/// <summary>The list of users the are recieving this message.</summary>
			public List<EmailMessageDetailUser> toUserList { get; set; }

			/// <summary>The list of subjects that are to be used.</summary>
			public Dictionary<int, string> subjectList { get; set; }

			/// <summary>List of artifact tokens being sent.</summary>
			public Dictionary<int, string> artifactTokens { get; set; }

			/// <summary>The project's token.</summary>
			public string projectToken { get; set; }

			/// <summary>The user sending the mail (or that made the change).</summary>
			public User fromUser { get; set; }

			public class EmailMessageDetailUser
			{
				/// <summary>The user's name.</summary>
				public string Name { get; set; }
				/// <summary>The user's email address.</summary>
				public string Address { get; set; }
				/// <summary>The user's ID #</summary>
				public int UserId { get; set; }
				/// <summary>The index # of the subject to use.</summary>
				public int SubjectId { get; set; }
				/// <summary>The artifact token that is to be used for this user.</summary>
				public int? ArtifactTokenId { get; set; }
				/// <summary>The source of this message.</summary>
				public string Source { get; set; }
			}
		}

		/// <summary>Class that holds user information for subscriptions and notifications.</summary>
		public class UserEmailDetails
		{
			public string Name { get; set; }
			public string Email { get; set; }
			public int ID { get; set; }
		}

		#endregion

		#region Notification Events
		/// <summary>Creates the default notification events for a new project template</summary>
		/// <param name="projectTemplateId"></param>
		public void NotificationEvent_CreateforNewProjectTemplate(int projectTemplateId)
		{
			const string METHOD_NAME = "NotificationEvent_CreateforNewProjectTemplate";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Create the events, first.
					List<NotificationEvent> newEvents = new List<NotificationEvent>();
					AddNotificationEventRow(context, projectTemplateId, 3, true, true, "Incident: Newly Created", "${Product} - A new incident  has been opened in ${ProjectName}", newEvents);
					AddNotificationEventRow(context, projectTemplateId, 3, false, true, "Incident: Owner Assigned", "${Product} - Incident ${ID#} has been assigned to you.", newEvents);
					AddNotificationEventRow(context, projectTemplateId, 1, true, false, "Requirement: Newly Created", "${Product} - A new requirement has been opened in ${ProjectName}", newEvents);
					AddNotificationEventRow(context, projectTemplateId, 1, false, true, "Requirement: Owner Assigned / Importance", "${Product} - Requirement ${ID#} has changed!", newEvents);
					AddNotificationEventRow(context, projectTemplateId, 2, false, true, "Test Case: Executed", "${Product} - A test case has been executed.", newEvents);
					AddNotificationEventRow(context, projectTemplateId, 6, true, false, "Task: Newly Created", "${Product} - A new task has been opened.", newEvents);
					AddNotificationEventRow(context, projectTemplateId, 6, false, true, "Task: Owner or Priority Changed", "${Product} - Task ${ID} has been updated.", newEvents);
					AddNotificationEventRow(context, projectTemplateId, 8, true, false, "Test Set: Newly Created", "${Product} - A new Test Set has been created in ${ProjectName}", newEvents);
					AddNotificationEventRow(context, projectTemplateId, 8, false, true, "Test Set: Execution Changed", "${Product} - A test set has been run in ${ProjectName}", newEvents);
					AddNotificationEventRow(context, projectTemplateId, 2, false, true, "Test Case: Approval", "${Product} - Test Case Approval Notification.", newEvents);

					//Added successfully, add our Project Roles for each one..
					ProjectRole projectRole = new ProjectRole();
					projectRole.ProjectRoleId = ProjectManager.ProjectRoleProjectOwner;
					context.ProjectRoles.Attach(projectRole);
					foreach (NotificationEvent notifyEvent in newEvents)
					{
						//Each one has the owner assigned..
						notifyEvent.ProjectRoles.Add(projectRole);
					}

					//Okay, now add our users..
					NotificationArtifactUserType notificationArtifactUserType1 = new NotificationArtifactUserType();
					notificationArtifactUserType1.ProjectArtifactNotifyTypeId = 1;
					context.NotificationArtifactUserTypes.Attach(notificationArtifactUserType1);

					NotificationArtifactUserType notificationArtifactUserType2 = new NotificationArtifactUserType();
					notificationArtifactUserType2.ProjectArtifactNotifyTypeId = 2;
					context.NotificationArtifactUserTypes.Attach(notificationArtifactUserType2);

					AddNotificationArtifactUserRow(context, notificationArtifactUserType1, newEvents[0]);
					AddNotificationArtifactUserRow(context, notificationArtifactUserType2, newEvents[1]);
					AddNotificationArtifactUserRow(context, notificationArtifactUserType1, newEvents[2]);
					AddNotificationArtifactUserRow(context, notificationArtifactUserType2, newEvents[3]);
					AddNotificationArtifactUserRow(context, notificationArtifactUserType2, newEvents[4]);
					AddNotificationArtifactUserRow(context, notificationArtifactUserType1, newEvents[5]);
					AddNotificationArtifactUserRow(context, notificationArtifactUserType2, newEvents[6]);
					AddNotificationArtifactUserRow(context, notificationArtifactUserType1, newEvents[7]);
					AddNotificationArtifactUserRow(context, notificationArtifactUserType2, newEvents[8]);
					AddNotificationArtifactUserRow(context, notificationArtifactUserType2, newEvents[9]);


					int artifactFieldId = (int)new ArtifactManager().RetrieveArtifactFieldIdByName(Artifact.ArtifactTypeEnum.TestCaseSignature, "TestCaseSignatureId");
					//Now add the fields..
					AddNotificationEventFieldRow(context, newEvents[1], 6);
					AddNotificationEventFieldRow(context, newEvents[3], 18);
					AddNotificationEventFieldRow(context, newEvents[3], 73);
					AddNotificationEventFieldRow(context, newEvents[4], 25);
					AddNotificationEventFieldRow(context, newEvents[4], 44);
					AddNotificationEventFieldRow(context, newEvents[6], 58);
					AddNotificationEventFieldRow(context, newEvents[6], 59);
					AddNotificationEventFieldRow(context, newEvents[8], 52);
					AddNotificationEventFieldRow(context, newEvents[8], 55);
					AddNotificationEventFieldRow(context, newEvents[9], artifactFieldId);

					//Now save it all
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, ex);
				Logger.Flush();
				throw ex;
			}
		}

		/// <summary>Copies all events and properties from one project template to another.</summary>
		/// <param name="fromProjectTemplateId">The project template ID to copy events from.</param>
		/// <param name="toProjectTemplateId">The project template ID to copy events to.</param>
		public void CopyProjectEvents(int fromProjectTemplateId, int toProjectTemplateId)
		{
			const string METHOD_NAME = CLASS + "CopyProjectEvents()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				//Get events for the specified project.
				List<NotificationEvent> notifyEvents = RetrieveEvents(fromProjectTemplateId);

				//For each event specified, pull all needed data.
				for (int i = 0; i < notifyEvents.Count; i++)
				{
					int notificationEventId = notifyEvents[i].NotificationEventId;
					using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
					{
						var query = from n in context.NotificationEvents
										.Include(n => n.ArtifactType)
										.Include(n => n.ArtifactFields)
										.Include(n => n.NotificationArtifactUserTypes)
										.Include(n => n.ProjectRoles)
									where n.NotificationEventId == notificationEventId
									select n;

						NotificationEvent sourceEvent = query.FirstOrDefault();

						if (sourceEvent != null)
						{
							//First, handle the main event.
							NotificationEvent newNotifyEvent = new NotificationEvent();
							newNotifyEvent.ProjectTemplateId = toProjectTemplateId;
							newNotifyEvent.Name = sourceEvent.Name;
							newNotifyEvent.IsActive = sourceEvent.IsActive;
							newNotifyEvent.IsArtifactCreation = sourceEvent.IsArtifactCreation;
							newNotifyEvent.ArtifactTypeId = sourceEvent.ArtifactTypeId;
							newNotifyEvent.EmailSubject = sourceEvent.EmailSubject;
							context.NotificationEvents.AddObject(newNotifyEvent);

							//Now loop through each of the other child collections, and update NotificationEventId.
							// - Notification Event Fields.
							for (int j = 0; j < sourceEvent.ArtifactFields.Count; j++)
							{
								newNotifyEvent.ArtifactFields.Add(sourceEvent.ArtifactFields[j]);
							}

							// - Notification Users
							for (int k = 0; k < sourceEvent.NotificationArtifactUserTypes.Count; k++)
							{
								newNotifyEvent.NotificationArtifactUserTypes.Add(sourceEvent.NotificationArtifactUserTypes[k]);
							}

							// - Notification Project Roles
							for (int l = 0; l < sourceEvent.ProjectRoles.Count; l++)
							{
								newNotifyEvent.ProjectRoles.Add(sourceEvent.ProjectRoles[l]);
							}

							//Do final save.
							context.SaveChanges();
						}
					}
				}
				Logger.LogExitingEvent(METHOD_NAME);
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(METHOD_NAME, ex);
				Logger.Flush();
				throw ex;
			}
		}

		///<summary>Retrieves a list of all Notification events for the selected Project Template</summary>
		///<param name="projectTemplateId">The id of the project template</param>
		///<returns>A notification list</returns>
		public List<NotificationEvent> RetrieveEvents(int projectTemplateId)
		{
			const string METHOD_NAME = CLASS + "RetrieveEvents()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				List<NotificationEvent> notificationEvents;

				//Actually execute the query and return the dataset
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from n in context.NotificationEvents.Include(n => n.ArtifactType)
								where n.ProjectTemplateId == projectTemplateId
								orderby n.Name, n.NotificationEventId
								select n;

					notificationEvents = query.ToList();
				}

				//Return the list
				Logger.LogExitingEvent(METHOD_NAME);
				return notificationEvents;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Retrieves a single NotificationEvent record and it's associated items (fields, template, users) by Event id.</summary>
		/// <param name="notificationEventId">The ID of the notification event to retrieve.</param>
		/// <returns>A notification enity hierarchy</returns>
		/// <remarks>This query takes a long time so we cache it</remarks>
		public NotificationEvent RetrieveEventById(int notificationEventId)
		{
			const string METHOD_NAME = "RetrieveEventById";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				NotificationEvent notificationEvent;

				//See if we have a cached instance available
				//if (cachedEvents.ContainsKey(notificationEventId))
				//{
				//	return cachedEvents[notificationEventId];
				//}

				//Actually execute the query and return the dataset
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from n in context.NotificationEvents
									.Include(n => n.ArtifactType)
									.Include(n => n.ArtifactFields)
									.Include(n => n.NotificationArtifactUserTypes)
									.Include(n => n.ProjectRoles)
								where n.NotificationEventId == notificationEventId
								select n;

					notificationEvent = query.FirstOrDefault();

					if (notificationEvent == null)
					{
						throw new ArtifactNotExistsException("Notify Event " + notificationEventId.ToString() + " doesn't exist in the system");
					}

					//Cache the event
					//cachedEvents.Add(notificationEventId, notificationEvent);
				}

				//Return the item
				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return notificationEvent;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>
		/// Helper method to add an artifact field to a notification event
		/// </summary>
		private void AddNotificationEventFieldRow(SpiraTestEntitiesEx context, NotificationEvent notificationEvent, int artifactFieldId)
		{
			ArtifactField artifactField = new ArtifactField();
			artifactField.ArtifactFieldId = artifactFieldId;
			context.ArtifactFields.Attach(artifactField);
			notificationEvent.ArtifactFields.Add(artifactField);
		}

		/// <summary>
		/// Helper method to add user (owner vs. creator) to a notifiction event
		/// </summary>
		private void AddNotificationArtifactUserRow(SpiraTestEntitiesEx context, NotificationArtifactUserType notificationArtifactUserType, NotificationEvent notificationEvent)
		{
			notificationEvent.NotificationArtifactUserTypes.Add(notificationArtifactUserType);
		}

		/// <summary>Helper method for quickly adding new events to the context</summary>
		/// <param name="context">The EF context</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <param name="artifactTypeId">The id of the artifact type</param>
		/// <param name="artifactCreation">Should the event fire on artifact creation</param>
		/// <param name="active">Is the event active</param>
		/// <param name="subjectLine">The subject line for the email</param>
		/// <param name="events">The collection of new events</param>
		/// <param name="name">The name of the event</param>
		private void AddNotificationEventRow(SpiraTestEntitiesEx context, int projectTemplateId, int artifactTypeId, bool artifactCreation, bool active, string name, string subjectLine, List<NotificationEvent> events)
		{
			NotificationEvent notificationEvent = new NotificationEvent();
			notificationEvent.Name = name;
			notificationEvent.IsActive = active;
			notificationEvent.IsArtifactCreation = artifactCreation;
			notificationEvent.ProjectTemplateId = projectTemplateId;
			notificationEvent.ArtifactTypeId = artifactTypeId;
			notificationEvent.EmailSubject = subjectLine;

			context.NotificationEvents.AddObject(notificationEvent);
			events.Add(notificationEvent);
		}

		/// <summary>Inserts a new Notification Event into the project template.</summary>
		/// <returns>The newly created NotificationEventId</returns>
		public int InsertEvent(string name, bool isActive, bool isOnCreation, int projectTemplateId, int artifactTypeId, string subjectLine)
		{
			const string METHOD_NAME = "InsertEvent";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				int notificationEventId;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Add the object and save
					NotificationEvent notificationEvent = new NotificationEvent();
					notificationEvent.Name = name;
					notificationEvent.IsActive = isActive;
					notificationEvent.IsArtifactCreation = isOnCreation;
					notificationEvent.ProjectTemplateId = projectTemplateId;
					notificationEvent.ArtifactTypeId = artifactTypeId;
					notificationEvent.EmailSubject = subjectLine;

					context.NotificationEvents.AddObject(notificationEvent);
					context.SaveChanges();
					notificationEventId = notificationEvent.NotificationEventId;
				}

				//Return the ID.
				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return notificationEventId;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Inserts or updates the provided notification event</summary>
		/// <param name="sourceEvent">The notification event object</param>
		/// <remarks>We don't use the EF change tracking to handle persistence because it doesn't correctly work with many-to-many joins</remarks>
		public void SaveEvent(NotificationEvent sourceEvent)
		{
			const string METHOD_NAME = "SaveEvent";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				//Clear all existing cached events
				cachedEvents.Clear();

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//See if we have a primary key defined, it not, treat as a simple add
					if (sourceEvent.ChangeTracker.State == ObjectState.Added && sourceEvent.NotificationEventId < 1)
					{
						//We need to create a new entity and add that, so that we can handle the joins properly in EF4 :-(
						NotificationEvent notificationEvent = new NotificationEvent();
						notificationEvent.Name = sourceEvent.Name;
						notificationEvent.IsActive = sourceEvent.IsActive;
						notificationEvent.IsArtifactCreation = sourceEvent.IsArtifactCreation;
						notificationEvent.ArtifactTypeId = sourceEvent.ArtifactTypeId;
						notificationEvent.EmailSubject = sourceEvent.EmailSubject;
						notificationEvent.ProjectTemplateId = sourceEvent.ProjectTemplateId;

						foreach (ProjectRole projectRole in sourceEvent.ProjectRoles)
						{
							ProjectRole projectRole2 = new ProjectRole();
							projectRole2.ProjectRoleId = projectRole.ProjectRoleId;
							context.ProjectRoles.Attach(projectRole2);
							notificationEvent.ProjectRoles.Add(projectRole2);
						}
						foreach (ArtifactField artifactField in sourceEvent.ArtifactFields)
						{
							ArtifactField artifactField2 = new ArtifactField();
							artifactField2.ArtifactFieldId = artifactField.ArtifactFieldId;
							context.ArtifactFields.Attach(artifactField2);
							notificationEvent.ArtifactFields.Add(artifactField2);
						}
						foreach (NotificationArtifactUserType userType in sourceEvent.NotificationArtifactUserTypes)
						{
							NotificationArtifactUserType notificationArtifactUserType = new NotificationArtifactUserType();
							notificationArtifactUserType.ProjectArtifactNotifyTypeId = userType.ProjectArtifactNotifyTypeId;
							context.NotificationArtifactUserTypes.Attach(notificationArtifactUserType);
							notificationEvent.NotificationArtifactUserTypes.Add(notificationArtifactUserType);
						}

						context.NotificationEvents.AddObject(notificationEvent);
						context.SaveChanges();
					}
					else
					{
						//Get a fresh copy from the database attached to the context
						int notificationEventId = sourceEvent.NotificationEventId;
						var query = from n in context.NotificationEvents
										.Include(n => n.ArtifactType)
										.Include(n => n.ArtifactFields)
										.Include(n => n.NotificationArtifactUserTypes)
										.Include(n => n.ProjectRoles)
									where n.NotificationEventId == notificationEventId
									select n;

						NotificationEvent notificationEvent = query.FirstOrDefault();
						if (notificationEvent != null)
						{
							//Now make the changes to the connected entity
							notificationEvent.StartTracking();
							notificationEvent.Name = sourceEvent.Name;
							notificationEvent.IsActive = sourceEvent.IsActive;
							notificationEvent.IsArtifactCreation = sourceEvent.IsArtifactCreation;
							notificationEvent.ArtifactTypeId = sourceEvent.ArtifactTypeId;
							notificationEvent.EmailSubject = sourceEvent.EmailSubject;

							//Now loop through each of the other child collections, and update NotificationEventId.
							// - Notification Event Fields.
							for (int j = 0; j < sourceEvent.ArtifactFields.Count; j++)
							{
								if (!notificationEvent.ArtifactFields.Any(n => n.ArtifactFieldId == sourceEvent.ArtifactFields[j].ArtifactFieldId))
								{
									ArtifactField artifactField = new ArtifactField();
									artifactField.ArtifactFieldId = sourceEvent.ArtifactFields[j].ArtifactFieldId;
									context.ArtifactFields.Attach(artifactField);
									notificationEvent.ArtifactFields.Add(artifactField);
								}
							}
							for (int j = 0; j < notificationEvent.ArtifactFields.Count; j++)
							{
								if (!sourceEvent.ArtifactFields.Any(n => n.ArtifactFieldId == notificationEvent.ArtifactFields[j].ArtifactFieldId))
								{
									notificationEvent.ArtifactFields.Remove(notificationEvent.ArtifactFields[j]);
								}
							}

							// - Notification Users
							for (int j = 0; j < sourceEvent.NotificationArtifactUserTypes.Count; j++)
							{
								if (!notificationEvent.NotificationArtifactUserTypes.Any(n => n.ProjectArtifactNotifyTypeId == sourceEvent.NotificationArtifactUserTypes[j].ProjectArtifactNotifyTypeId))
								{
									NotificationArtifactUserType notificationArtifactUserType = new NotificationArtifactUserType();
									notificationArtifactUserType.ProjectArtifactNotifyTypeId = sourceEvent.NotificationArtifactUserTypes[j].ProjectArtifactNotifyTypeId;
									context.NotificationArtifactUserTypes.Attach(notificationArtifactUserType);
									notificationEvent.NotificationArtifactUserTypes.Add(notificationArtifactUserType);
								}
							}
							for (int j = 0; j < notificationEvent.NotificationArtifactUserTypes.Count; j++)
							{
								if (!sourceEvent.NotificationArtifactUserTypes.Any(n => n.ProjectArtifactNotifyTypeId == notificationEvent.NotificationArtifactUserTypes[j].ProjectArtifactNotifyTypeId))
								{
									notificationEvent.NotificationArtifactUserTypes.Remove(notificationEvent.NotificationArtifactUserTypes[j]);
								}
							}

							// - Notification Project Roles
							for (int j = 0; j < sourceEvent.ProjectRoles.Count; j++)
							{
								if (!notificationEvent.ProjectRoles.Any(n => n.ProjectRoleId == sourceEvent.ProjectRoles[j].ProjectRoleId))
								{
									ProjectRole projectRole = new ProjectRole();
									projectRole.ProjectRoleId = sourceEvent.ProjectRoles[j].ProjectRoleId;
									context.ProjectRoles.Attach(projectRole);
									notificationEvent.ProjectRoles.Add(projectRole);
								}
							}
							for (int j = 0; j < notificationEvent.ProjectRoles.Count; j++)
							{
								if (!sourceEvent.ProjectRoles.Any(n => n.ProjectRoleId == notificationEvent.ProjectRoles[j].ProjectRoleId))
								{
									notificationEvent.ProjectRoles.Remove(notificationEvent.ProjectRoles[j]);
								}
							}
						}

						context.SaveChanges();
					}

				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, ex);
				Logger.Flush();
				throw ex;
			}

			Logger.LogExitingEvent(CLASS + METHOD_NAME);
		}

		/// <summary>Deletes all the notification events and templates for a specific project template</summary>
		/// <param name="projectTemplateId">The id of the project template</param>
		protected internal void DeleteAllForProjectTemplate(int projectTemplateId)
		{
			const string METHOD_NAME = CLASS + "DeleteAllForProjectTemplate()";

			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				//Clear the cached events
				cachedEvents.Clear();

				//Now delete the templates and events
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					context.Notification_DeleteAllForProjectTemplate(projectTemplateId);
				}
				Logger.LogExitingEvent(METHOD_NAME);
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(METHOD_NAME, ex);
				Logger.Flush();
				throw ex;
			}
		}

		/// <summary>Deletes a notification event from the system.</summary>
		/// <param name="notificationEventId">The notification ID to delete.</param>
		public void DeleteEvent(int notificationEventId)
		{
			const string METHOD_NAME = CLASS + "DeleteEvent()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				//Clear the cached events
				cachedEvents.Clear();

				// No need to delete records from TST_NOTIFICATION_EVENT_FIELD, TST_NOTIFICATION_ARTIFACT_USER,
				//   or TST_NOTIFICATION_PROJECT_ROLE first, as keys cascade deletes to those tables.
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from n in context.NotificationEvents
								where n.NotificationEventId == notificationEventId
								select n;

					NotificationEvent notificationEvent = query.FirstOrDefault();
					if (notificationEvent != null)
					{
						context.NotificationEvents.DeleteObject(notificationEvent);
						context.SaveChanges();
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(METHOD_NAME, ex);
				Logger.Flush();
				throw ex;
			}

			Logger.LogExitingEvent(METHOD_NAME);
		}
		#endregion

		#region Notification Artifact Templates
		/// <summary>Copies across the notification templates from an existing project template to a new project template</summary>
		/// <param name="existingProjectTemplateId">The id of the existing project template</param>
		/// <param name="newProjectTemplateId">The id of the new project template</param>
		/// <remarks>It assumes that the new project template does not already have any notification templates in place</remarks>
		public void NotificationTemplate_CopyForProjectTemplate(int existingProjectTemplateId, int newProjectTemplateId)
		{
			const string METHOD_NAME = CLASS + "NotificationTemplate_CopyForProjectTemplate()";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Get the existing notification templates
					var query = from nt in context.NotificationArtifactTemplates
								where nt.ProjectTemplateId == existingProjectTemplateId
								orderby nt.ArtifactTypeId
								select nt;

					List<NotificationArtifactTemplate> existingTemplates = query.ToList();

					//Add them to the new project template
					foreach (NotificationArtifactTemplate existingTemplate in existingTemplates)
					{
						NotificationArtifactTemplate newTemplate = new NotificationArtifactTemplate();
						newTemplate.ProjectTemplateId = newProjectTemplateId;
						newTemplate.ArtifactTypeId = existingTemplate.ArtifactTypeId;
						newTemplate.TemplateText = existingTemplate.TemplateText;
						context.NotificationArtifactTemplates.AddObject(newTemplate);
					}
					context.SaveChanges();
				}

				Logger.LogExitingEvent(METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Creates the default notification templates for a new project template</summary>
		/// <param name="projectTemplateId">The id of the project template</param>
		public void NotificationTemplate_CreateforNewProjectTemplate(int projectTemplateId)
		{
			const string METHOD_NAME = CLASS + "NotificationTemplate_CreateforNewProjectTemplate()";

			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				//Insert the various templates
				InsertNotificationTemplate(projectTemplateId, (int)Artifact.ArtifactTypeEnum.Document, "emailTemplate_documents_inline_default.htm");
				InsertNotificationTemplate(projectTemplateId, (int)Artifact.ArtifactTypeEnum.Incident, "emailTemplate_incidents_inline_default.htm");
				InsertNotificationTemplate(projectTemplateId, (int)Artifact.ArtifactTypeEnum.Requirement, "emailTemplate_requirements_inline_default.htm");
				InsertNotificationTemplate(projectTemplateId, (int)Artifact.ArtifactTypeEnum.Task, "emailTemplate_tasks_inline_default.htm");
				InsertNotificationTemplate(projectTemplateId, (int)Artifact.ArtifactTypeEnum.TestCase, "emailTemplate_testCase_inline_default.htm");
				InsertNotificationTemplate(projectTemplateId, (int)Artifact.ArtifactTypeEnum.TestSet, "emailTemplate_testSet_inline_default.htm");
				InsertNotificationTemplate(projectTemplateId, (int)Artifact.ArtifactTypeEnum.Risk, "emailTemplate_risks_inline_default.htm");
				InsertNotificationTemplate(projectTemplateId, (int)Artifact.ArtifactTypeEnum.Release, "emailTemplate_releases_inline_default.htm");

				Logger.LogExitingEvent(METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Inserts a single notification template into the project template</summary>
		/// <param name="projectTemplateId"></param>
		/// <param name="artifactTypeId"></param>
		/// <param name="filename"></param>
		/// <remarks>It does not check if it already exists, so check before calling</remarks>
		protected void InsertNotificationTemplate(int projectTemplateId, int artifactTypeId, string filename)
		{
			const string METHOD_NAME = "InsertNotificationTemplate()";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				//Get the text from the filename
				string templateText = null;
				Assembly assembly = Assembly.GetExecutingAssembly();
				//(e.g. Inflectra.SpiraTest.BuildCustomActions.StaticData.TST_ARTIFACT_FIELD.sql)
				using (Stream stream = assembly.GetManifestResourceStream("Inflectra.SpiraTest.Business.NotificationTemplates." + filename))
				{
					using (StreamReader reader = new StreamReader(stream))
					{
						templateText = reader.ReadToEnd();
					}
				}

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//We need to insert a template for each artifact type that supports them
					NotificationArtifactTemplate notificationTemplate = new NotificationArtifactTemplate();
					notificationTemplate.ArtifactTypeId = artifactTypeId;
					notificationTemplate.ProjectTemplateId = projectTemplateId;
					notificationTemplate.TemplateText = templateText;

					//Commit
					context.NotificationArtifactTemplates.AddObject(notificationTemplate);
					context.SaveChanges();
				}
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Get the template for the given project template andartifact type.</summary>
		/// <param name="artifactTypeId">The artifact type ID.</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <returns>The Template record</returns>
		public NotificationArtifactTemplate RetrieveTemplateById(int projectTemplateId, int artifactTypeId)
		{
			const string METHOD_NAME = "RetrieveTemplateById()";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				NotificationArtifactTemplate notificationArtifactTemplate;

				////See if we have a cached template
				//string cacheKey = artifactTypeId + "_" + projectTemplateId;
				//if (cachedTemplates.ContainsKey(cacheKey))
				//{
				//	return cachedTemplates[cacheKey];
				//}

				//Actually execute the query and return the dataset
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from n in context.NotificationArtifactTemplates
								where n.ArtifactTypeId == artifactTypeId && n.ProjectTemplateId == projectTemplateId
								select n;

					notificationArtifactTemplate = query.FirstOrDefault();

					if (notificationArtifactTemplate == null)
					{
						throw new ArtifactNotExistsException("ArtifactType " + artifactTypeId.ToString() + " does not have a notification template type in project template PT" + projectTemplateId);
					}
				}

				//Cache the item
				//cachedTemplates.Add(cacheKey, notificationArtifactTemplate);

				//Return the item
				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return notificationArtifactTemplate;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Get the template text for the given artifact type and project template.</summary>
		/// <param name="artifactTypeId">The artifact type ID.</param>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <returns>The string representing the template text.</returns>
		public string RetrieveTemplateTextById(int projectTemplateId, int artifactTypeId)
		{
			try
			{
				NotificationArtifactTemplate template = RetrieveTemplateById(projectTemplateId, artifactTypeId);
				if (template == null)
				{
					throw new ArtifactNotExistsException("ArtifactType " + artifactTypeId.ToString() + " does not have a notification template type in the system.");
				}
				return template.TemplateText;
			}
			catch (ArtifactNotExistsException)
			{
				throw;
			}
		}

		/// <summary>Retrieves all notification templates defined in the project template.</summary>
		/// <param name="projectTemplateId">The id of the project template</param>
		/// <returns>Template records</returns>
		public List<NotificationArtifactTemplate> RetrieveTemplates(int projectTemplateId)
		{
			const string METHOD_NAME = CLASS + "RetrieveTemplates()";

			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				List<NotificationArtifactTemplate> notificationArtifactTemplates;

				//Actually execute the query and return the dataset
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from n in context.NotificationArtifactTemplates
								where n.ProjectTemplateId == projectTemplateId
								orderby n.ArtifactTypeId
								select n;

					notificationArtifactTemplates = query.ToList();
				}

				//Return the item
				Logger.LogExitingEvent(METHOD_NAME);
				return notificationArtifactTemplates;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}

		/// <summary>Updates an existing template</summary>
		/// <param name="template">The template to update</param>
		public void UpdateTemplate(NotificationArtifactTemplate template)
		{
			const string METHOD_NAME = "UpdateTemplate()";

			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				//Clear the cached templates
				cachedTemplates.Clear();

				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					//Attach to the context and save changes
					context.NotificationArtifactTemplates.ApplyChanges(template);
					context.SaveChanges();
				}

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				Logger.Flush();
				throw;
			}
		}
		#endregion

		#region Template / Token Translation
		/// <summary>Checks wether the two items are the same or not. Both items must be the same object type.</summary>
		/// <param name="item1">First item to compare.</param>
		/// <param name="item2">Second item to compare.</param>
		/// <param name="artifactField">The ArtifactField entity for the item's data.</param>
		/// <returns>True if the items are equal, false if different.</returns>
		private bool CheckValuesEqual(object item1, object item2, ArtifactField artifactField)
		{
			//Set it to true so that other fields are ignore.
			bool retValue = true;

			//Record if our items are null..
			bool item1Null = (item1 == null || item1.GetType() == typeof(DBNull));
			bool item2Null = (item2 == null || item2.GetType() == typeof(DBNull));

			if (!item1Null || !item2Null)
			{
				if (item1Null != item2Null)
					retValue = false;
				else
				{
					if (item1.GetType() == item2.GetType())
					{
						try
						{
							switch (artifactField.ArtifactFieldTypeId)
							{
								/* Text Fields */
								case 1:     //Text
								case 6:     //Name&Description
								case 10:    //Flag
								case 12:    //HTML
									retValue = ((string)item1 == (string)item2);
									break;

								/* Integer Comparison */
								case 2:     //Lookup
								case 4:     //Identifier
								case 7:     //Custom Property Lookup
								case 8:     //Integer
								case 9:     //TimeInterval
								case 11:    //HierarchyLookup
									retValue = ((int)item1 == (int)item2);
									break;

								/* DateTime Comparison */
								case 3:     //DateTime
								case 15:    //Custom Property DateTime
									retValue = ((DateTime)item1 == (DateTime)item2);
									break;

								/* MultiList Comparison */
								case 14:    //Custom Property MultiList
											//TODO: Compare multiple lists.
									break;

								/* Decimal Comparison */
								case 13:    //Custom Property Decimal
									retValue = ((decimal)item1 == (decimal)item2);
									break;

								case 5: //Equalizer - Not Compared.
								default:
									break;
							}
						}
						catch { }
					}
					else
						retValue = false;
				}
			}

			return retValue;
		}

		/// <summary>Used from a regEx match to replace specified tokens.</summary>
		/// <param name="m">Match</param>
		/// <param name="artifact">The 'ArtifactView' to pull data from.</param>
		/// <returns>New string to use.</returns>
		/// <remarks>Certain tokens are not replaced, and changed for final sending. These are:
		/// ${1}	- The person this notification is to.
		/// ${2}	- The Event Name.
		/// ${3}	- Artifact Comments/Resolutions.
		/// </remarks>
		/// <param name="longTextFields">Should we translate the long-field tokens such as ${Comments} and ${Description}</param>
		private string ReplaceToken(Match m, Artifact artifact, List<ArtifactField> artFields, bool longTextFields, int? userId)
		{
			const string METHOD_NAME = CLASS + "ReplaceToken";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				//We need to get the Artifact's 'View' object, which means re-pulling the artifact from the database. D'oh.

				//Get token and any options..
				string tokenReal = m.Groups[1].Value;
				string token = tokenReal.ToLowerInvariant();
				string option = "";
				//If the token contains a ':', it means they wanted an option.
				//TODO: Options aren't implemented yet. But can be, easily.
				if (token.Contains(':'))
				{
					string[] tokens = token.Split(':');
					token = tokens[0];
					if (tokens.Count() > 1) option = tokens[1];
				}

				string retStr = "";
				//See if it's a static token first. (Prevents multiple exceptions for faster code.)
				switch (token)
				{
					#region Static Fields
					case "url":
						//Get the project ID first..
						int projId = (int)artifact["ProjectId"];
						int artId = -1;
						UrlRoots.NavigationLinkEnum artType = UrlRoots.NavigationLinkEnum.None;
						//Get the item-specific datas.
						switch (artifact.ArtifactType)
						{
							case Artifact.ArtifactTypeEnum.Incident:
								artId = ((dynamic)artifact).IncidentId;
								artType = UrlRoots.NavigationLinkEnum.Incidents;
								break;

							case Artifact.ArtifactTypeEnum.Requirement:
								artId = ((dynamic)artifact).RequirementId;
								artType = UrlRoots.NavigationLinkEnum.Requirements;
								break;

							case Artifact.ArtifactTypeEnum.TestCase:
								artId = ((dynamic)artifact).TestCaseId;
								artType = UrlRoots.NavigationLinkEnum.TestCases;
								break;

							case Artifact.ArtifactTypeEnum.Task:
								artId = ((dynamic)artifact).TaskId;
								artType = UrlRoots.NavigationLinkEnum.Tasks;
								break;

							case Artifact.ArtifactTypeEnum.TestSet:
								artId = ((dynamic)artifact).TestSetId;
								artType = UrlRoots.NavigationLinkEnum.TestSets;
								break;

							case Artifact.ArtifactTypeEnum.Document:
								artId = ((dynamic)artifact).AttachmentId;
								artType = UrlRoots.NavigationLinkEnum.Documents;
								break;

							case Artifact.ArtifactTypeEnum.Risk:
								artId = ((dynamic)artifact).RiskId;
								artType = UrlRoots.NavigationLinkEnum.Risks;
								break;

							case Artifact.ArtifactTypeEnum.Release:
								artId = ((dynamic)artifact).ReleaseId;
								artType = UrlRoots.NavigationLinkEnum.Releases;
								break;
						}
						//Get the artifact's URL here.
						string urlStr = UrlRoots.RetrieveURL(artType, projId, artId, null, true, userId);
						retStr = "<a href=\"" + urlStr + "\" target=\"_blank\">" + urlStr + "</a>";
						break;

					case "notifyto":
						//Return the notifyto, do not modify. To be added later during sending.
						retStr = "${1}";
						break;

					case "eventname":
						//Return the same tag, this will be added by the event handler.
						//Temporarily commented out.
						retStr = "${2}";
						break;

					case "product":
						//Return the product name.
						retStr = ConfigurationSettings.Default.License_ProductType;
						break;

					case "projectname":
						//Return the project name.
						int projectId = (int)artifact["ProjectId"];
						Project projData = new ProjectManager().RetrieveById(projectId);
						retStr = projData.Name;
						break;

					case "id":
					case "id#":
						//Return the item's ID number.
						int itemId = -1;
						switch (artifact.ArtifactType)
						{
							case Artifact.ArtifactTypeEnum.Incident:
								itemId = (int)artifact["IncidentId"];
								break;
							case Artifact.ArtifactTypeEnum.Requirement:
								itemId = (int)artifact["RequirementId"];
								break;
							case Artifact.ArtifactTypeEnum.TestCase:
								itemId = (int)artifact["TestCaseId"];
								break;
							case Artifact.ArtifactTypeEnum.Task:
								itemId = (int)artifact["TaskId"];
								break;
							case Artifact.ArtifactTypeEnum.TestSet:
								itemId = (int)artifact["TestSetId"];
								break;
							case Artifact.ArtifactTypeEnum.Document:
								itemId = (int)artifact["AttachmentId"];
								break;
							case Artifact.ArtifactTypeEnum.Risk:
								itemId = (int)artifact["RiskId"];
								break;
							case Artifact.ArtifactTypeEnum.Release:
								itemId = (int)artifact["ReleaseId"];
								break;
						}
						if (itemId > 0)
							if (token.EndsWith("#"))
								retStr = "#" + itemId.ToSafeString();
							else
								retStr = itemId.ToSafeString();
						break;
					#region Comments
					case "comment":
					case "resolution":
					case "comments":
					case "resolutions":
						{
							//Make sure the text supports long fields (e.g. Email Subjects do not)
							if (longTextFields)
							{
								//Static strings for the table bits. {0} User Name, {1} Date, {2} Comment Text.
								//-- Table opening.
								string tableOpening = "<table border=\"0\" cellpadding=\"4\" cellspacing=\"0\"><tbody>";
								//-- Header row.
								string tableHeaderRow = "<tr><td style=\"font-family: Helvetica,Arial,sans-serif;font-weight: normal;padding-top: 0px;padding-bottom: 0;padding-right: 0;margin-top: 0;margin-bottom: 0;margin-right: 0;margin-left: 0;Margin: 0;text-align: left;font-size: 11px;padding-left: 10px;color: #777;\">{0} ({1} UTC)</td><tr>" + Environment.NewLine;
								//-- Comment row.
								string tableDataRow = "<tr><td style=\"color: #0a0a0a;font-family: Helvetica,Arial,sans-serif;font-weight: normal;padding-top: 0;padding-bottom: 10px;padding-right: 0;margin-top: 0;margin-bottom: 0;margin-right: 0;margin-left: 0;Margin: 0;text-align: left;font-size: 13px;padding-left: 10px;line-height: 150%;vertical-align: middle;\">{0}</td></tr>" + Environment.NewLine;
								//-- Table closing.
								string tableClosing = "</tbody></table>";

								//Get the discussion for the artifact.
								//Get the item's id and type first:
								int artNum = -1;
								switch (artifact.ArtifactType)
								{
									case Artifact.ArtifactTypeEnum.Incident:
										artNum = ((dynamic)artifact).IncidentId;
										break;
									case Artifact.ArtifactTypeEnum.Requirement:
										artNum = ((dynamic)artifact).RequirementId;
										break;
									case Artifact.ArtifactTypeEnum.TestCase:
										artNum = ((dynamic)artifact).TestCaseId;
										break;
									case Artifact.ArtifactTypeEnum.Task:
										artNum = ((dynamic)artifact).TaskId;
										break;
									case Artifact.ArtifactTypeEnum.TestSet:
										artNum = ((dynamic)artifact).TestSetId;
										break;
									case Artifact.ArtifactTypeEnum.Document:
										artNum = ((dynamic)artifact).AttachmentId;
										break;
									case Artifact.ArtifactTypeEnum.Risk:
										artNum = ((dynamic)artifact).RiskId;
										break;
									case Artifact.ArtifactTypeEnum.Release:
										artNum = ((dynamic)artifact).ReleaseId;
										break;
								}

								IEnumerable<IDiscussion> discussThreads = null;
								Incident incident = null;

								//Now create the table and add the comments.
								if (artNum > 0)
								{
									//Get discussion threads. For everything except Incidents, we have a Manager Function for that.
									//  For Incidents, we need to re-pull the artifact. (To be sure we got new comments added.)
									if (artifact.ArtifactType != Artifact.ArtifactTypeEnum.Incident)
									{
										discussThreads = new DiscussionManager().Retrieve(artNum, artifact.ArtifactType, false);
									}
									else
									{
										incident = new IncidentManager().RetrieveById(artNum, true);
									}

									//Now we combine what we have into our return string. Open the table..
									retStr = tableOpening;

									//Now add our rows.
									if (option != "first" && option != "last")
									{
										//Add all comments from the incident.
										if (incident != null)
										{
											foreach (IncidentResolution resolution in incident.Resolutions)
											{
												//Header row.
												retStr += string.Format(tableHeaderRow, resolution.Creator.FullName, resolution.CreationDate.ToString("g"));
												//Comment row.
												retStr += string.Format(tableDataRow, resolution.Resolution);
											}
										}
										else if (discussThreads != null) //Add all comments from other artifact types.
										{
											foreach (IDiscussion commentRow in discussThreads)
											{
												//Header row.
												retStr += string.Format(tableHeaderRow, commentRow.CreatorName, commentRow.CreationDate.ToString("g"));
												//Comment row.
												retStr += string.Format(tableDataRow, commentRow.Text);
											}
										}
									}
									else
									{
										//We have an option. Either the oldest (first) or newest (last) comment.
										IncidentResolution incidentResolution = null;
										IDiscussion discussionThread = null;
										bool hasComments = false;
										if (incident != null && incident.Resolutions.Count > 0)
											hasComments = true;
										else if (discussThreads != null && discussThreads.Count() > 0)
											hasComments = true;

										if (option == "last")
										{
											if (incident != null)
											{
												//Pull from the new incident data.
												incidentResolution = incident.Resolutions.LastOrDefault();
											}
											else if (discussThreads != null)
											{
												//Pull from the discussion threads..
												discussionThread = discussThreads.LastOrDefault();
											}
										}

										if (option == "first")
										{
											if (incident != null)
											{
												//Pull from the new incident data.
												incidentResolution = incident.Resolutions.FirstOrDefault();
											}
											else if (discussThreads != null)
											{
												//Pull from the discussion threads..
												discussionThread = discussThreads.FirstOrDefault();
											}
										}

										//Now add that comment.
										if (incidentResolution != null && hasComments)
										{
											//Header row.
											retStr += string.Format(tableHeaderRow,
												incidentResolution.Creator.FullName,
												incidentResolution.CreationDate.ToString("g"));
											//Comment row.
											retStr += string.Format(tableDataRow,
												incidentResolution.Resolution);
										}
										else if (discussionThread != null && hasComments)
										{
											//Header row.
											retStr += string.Format(tableHeaderRow,
												discussionThread.CreatorName,
												discussionThread.CreationDate.ToString("g"));
											//Comment row.
											retStr += string.Format(tableDataRow,
												discussionThread.Text);
										}
									}

									//Now, terminate the table.
									retStr += tableClosing;
								}
							}
						}
						break;
					#endregion
					#endregion

					#region Dynamic Fields
					default:
						{
							//All other item tags here. We use the 'art' object if it's not null, otherwise, use the 'dataRow' object.
							try
							{
								//Okay, see if the field has a lookup fiels.
								string fieldProp = tokenReal; //The property we're going to use.

								//See if it's a real field, or if we need to add 'Id' to it.
								ArtifactField field = artFields.FirstOrDefault(af => af.Name.ToLowerInvariant() == fieldProp.ToLowerInvariant());
								if (field == null)
								{
									//The field don't exist. So, add 'Id' to the string, and see if THAT hits anything.
									field = artFields.FirstOrDefault(af => af.Name.ToLowerInvariant() == (fieldProp + "Id").ToLowerInvariant());
								}

								//Release has CompletionId that should really be PercentComplete
								if (artifact.ArtifactType == Artifact.ArtifactTypeEnum.Release && field != null && field.Name.ToLowerInvariant() == "completionid")
								{
									field.Name = "PercentComplete";
								}

								//Now we should have a field here. If now, skip it.
								if (field != null && artifact.ContainsProperty(field.Name))
								{
									fieldProp = field.Name;
									//Get the lookup property, if one.
									if (!string.IsNullOrWhiteSpace(field.LookupProperty))
									{
										fieldProp = field.LookupProperty;
									}

									//Get the value we're going to use, first.
									object tokenData = null;
									if (artifact.ArtifactType != Artifact.ArtifactTypeEnum.Release && (field.Name.StartsWith("Release") || field.Name.EndsWith("ReleaseId")))
									{
										//  If it's a release, then we want to stay on the ID field,
										// since we are going to have to query the release.
										tokenData = artifact[field.Name];
									}
									else if (artifact.ContainsProperty(fieldProp))
									{
										// Otherwise, get the field from the artifact.
										tokenData = artifact[fieldProp];
									}

									//If it's null, then we don't need to do anything
									if (tokenData != null)
									{
										int typeNum = field.ArtifactFieldTypeId;

										//If it's a lookup field, then we assume it's a string.
										//Except the case of releases which need to be retrieved separately
										if (!string.IsNullOrWhiteSpace(field.LookupProperty) &&
											!field.Name.StartsWith("Release") &&
											!field.Name.StartsWith("DetectedRelease") &&
											 !field.Name.StartsWith("ResolvedRelease") &&
											 !field.Name.StartsWith("VerifiedRelease") &&
											 artifact.ArtifactType != Artifact.ArtifactTypeEnum.Release)
										{
											typeNum = (int)(Artifact.ArtifactFieldTypeEnum.Text);
										}

										//It could be a multitude of things..
										switch ((Artifact.ArtifactFieldTypeEnum)typeNum)
										{
											/* Text Fields */
											case Artifact.ArtifactFieldTypeEnum.NameDescription:
											case Artifact.ArtifactFieldTypeEnum.Text:
											case Artifact.ArtifactFieldTypeEnum.Lookup:
												{
													if (tokenData is String)
													{
														retStr = (string)tokenData;
													}
													else
													{
														throw new ApplicationException(String.Format("Unable to convert '{0}' to string", field.Name));
													}
													break;
												}

											case Artifact.ArtifactFieldTypeEnum.Html:
												{
													//Make sure the text supports long fields (e.g. Email Subjects do not)
													if (longTextFields)
													{
														retStr = (string)tokenData;
													}
													break;
												}

											/* DateTime Fields */
											case Artifact.ArtifactFieldTypeEnum.DateTime:
											case Artifact.ArtifactFieldTypeEnum.CustomPropertyDate:
												{
													if (tokenData.GetType() == typeof(DateTime))
														retStr = ((DateTime)tokenData).ToShortDateString();
													else
														retStr = GlobalResources.General.Global_NoneFlag;
													break;
												}

											/* Integer Fields */
											case Artifact.ArtifactFieldTypeEnum.Identifier:
											case Artifact.ArtifactFieldTypeEnum.Integer:
												{
													retStr = ((int)tokenData).ToString();
													break;
												}

											/* Untranslated Fields */
											case Artifact.ArtifactFieldTypeEnum.CustomPropertyLookup:
												{
													//TODO: Custom Property Lookup field.
												}
												break;

											case Artifact.ArtifactFieldTypeEnum.Equalizer:
												{
													//See if we have a known field
													if (field.Name == "ExecutionStatusId" && tokenData is Int32)
													{
														int executionStatusId = (int)tokenData;
														ExecutionStatus executionStatus = new TestCaseManager().RetrieveExecutionStatusById(executionStatusId);
														if (executionStatus != null)
														{
															retStr = executionStatus.Name;
														}
													}
													if (field.Name == "PercentComplete" && tokenData is Int32)
													{
														//Display as a simple integer percentage
														int percentComplete = (int)tokenData;
														retStr = percentComplete.ToString() + "%";
													}
												}
												break;

											/* TimeInterval Field */
											case Artifact.ArtifactFieldTypeEnum.TimeInterval:
												{
													retStr = string.Format(
														GlobalResources.General.NotificationManager_HoursAndMinutes,
														(((int)tokenData) / 60).ToString(),
														(((int)tokenData) % 60).ToString());
													break;
												}

											/* Boolean / Flag Field */
											case Artifact.ArtifactFieldTypeEnum.Flag:
												{
													if (tokenData.GetType() == typeof(bool))
														retStr = (((Boolean)tokenData) ? GlobalResources.General.Global_Yes : GlobalResources.General.Global_No);
													else if (tokenData.GetType() == typeof(string))
														retStr = ((((string)tokenData).ToLowerInvariant() == "y") ? GlobalResources.General.Global_Yes : GlobalResources.General.Global_No);
													break;
												}

											/* HierarchyLookup Field */
											case Artifact.ArtifactFieldTypeEnum.HierarchyLookup:
												{
													try
													{
														//First see if the ReleaseName exists.
														int releaseId;
														if (tokenData is String && Int32.TryParse((string)tokenData, out releaseId))
														{
															ReleaseView release = new ReleaseManager().RetrieveById2(null, releaseId);
															if (release != null)
															{
																retStr = release.FullName;
															}
														}
														else if (tokenData is Int32)
														{
															releaseId = (int)tokenData;
															ReleaseView release = new ReleaseManager().RetrieveById2(null, releaseId);
															if (release != null)
															{
																retStr = release.FullName;
															}
														}
														else
														{
															//  There was an error looking up the release. Instead, get the
															// lookup field's data.
															retStr = (string)artifact[field.LookupProperty];
														}
													}
													catch
													{
														//  There was an error looking up the release. Instead, get the
														// lookup field's data.
														retStr = (string)artifact[field.LookupProperty];
													}
													break;
												}

											/* Decimal Fields */
											case Artifact.ArtifactFieldTypeEnum.Decimal:
												{
													retStr = ((decimal)tokenData).ToSafeString();
													break;
												}

											case Artifact.ArtifactFieldTypeEnum.CustomPropertyMultiList:
												//TODO: Custom Property MultiList
												break;

											case Artifact.ArtifactFieldTypeEnum.MultiList:
												{
													//May be joinable to CustomPropertyMultiList case above.

													//The TokenData is a list of IDs, seperated by commas.
													string values = "";
													foreach (string num in ((string)tokenData).Split(','))
													{
														//At this time, no way to handle the lookup fields with common code.
														// Check the name of the field.
														if (field.Name.ToLowerInvariant().StartsWith("component"))
														{
															//Convert the item we have to a number and get it's value.
															int comId = 0;
															if (int.TryParse(num, out comId))
															{
																//Get the ID..
																using (SpiraTestEntitiesEx db = new SpiraTestEntitiesEx())
																{
																	Component com = db.Components.FirstOrDefault(c => c.ComponentId == comId);

																	if (com != null)
																	{
																		//Add it's name to the string..
																		values += com.Name.Trim() + "; ";
																	}

																}
															}
														}
														else
														{
															//Just populate the field with the list of numbers..
															values += num + "; ";
														}
													}

													//Strip off the ending '; '.
													retStr = values.Trim().Trim(';');
													break;
												}

										}
									}
								}
								else
								{
									Logger.LogWarningEvent(METHOD_NAME, "There was no field definitions for token '" + m.Groups[1].Value.ToLowerInvariant() + "'");
								}
							}
							#endregion
							catch (Exception exception)
							{
								Logger.LogWarningEvent(METHOD_NAME, exception);
							}
							break;
						}
				}

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
				return retStr;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, exception);
				throw;
			}
		}

		/// <summary>Translated template text using the specified artifact.</summary>
		/// <param name="art">An artifact entity containing the artifact data to use.</param>
		/// <param name="templateText">The template text of the e-mail.</param>
		/// <param name="artFields">List of Artifact fields for the Artifact type.</param>
		/// <returns>A string containing the translated message.</returns>
		/// <param name="longTextFields">Should we translate the long-field tokens such as ${Comments} and ${Description}</param>
		public string TranslateTemplate(Artifact art, string templateText, List<ArtifactField> artFields, bool longTextFields = true, int? userId = null)
		{
			const string METHOD_NAME = CLASS + "TranslateTemplate";
			Logger.LogEnteringEvent(METHOD_NAME);

			try
			{
				//See if we need to get a new (complete, View) artifact..
				Artifact artView = null;
				if (!art.GetType().ToString().EndsWith("View")) //If we're already getting a view, don't need to re-get it.
				{
					try
					{
						switch (art.ArtifactType)
						{
							case Artifact.ArtifactTypeEnum.Requirement:
								artView = new RequirementManager().RetrieveById2(null, ((Requirement)art).RequirementId, true);
								break;

							case Artifact.ArtifactTypeEnum.Task:
								artView = new TaskManager().TaskView_RetrieveById(((Task)art).TaskId, true);
								break;

							case Artifact.ArtifactTypeEnum.Incident:
								artView = new IncidentManager().RetrieveById2(((Incident)art).IncidentId, true);
								break;

							case Artifact.ArtifactTypeEnum.TestCase:
								artView = new TestCaseManager().RetrieveById(((TestCase)art).ProjectId, ((TestCase)art).TestCaseId, true);
								break;

							case Artifact.ArtifactTypeEnum.TestSet:
								artView = new TestSetManager().RetrieveById(((TestSet)art).ProjectId, ((TestSet)art).TestSetId, true);
								break;

							case Artifact.ArtifactTypeEnum.Document:
								{
									if (art is ProjectAttachment)
									{
										ProjectAttachment projectAttachment = (ProjectAttachment)art;
										artView = new AttachmentManager().RetrieveForProjectById2(projectAttachment.ProjectId, projectAttachment.AttachmentId);
									}
									if (art is DataModel.Attachment)
									{
										DataModel.Attachment attachment = (DataModel.Attachment)art;
										artView = new AttachmentManager().RetrieveForProjectById2(attachment.ProjectId, attachment.AttachmentId);
									}
								}
								break;

							case Artifact.ArtifactTypeEnum.Risk:
								artView = new RiskManager().Risk_RetrieveById2(((Risk)art).RiskId, true);
								break;

							case Artifact.ArtifactTypeEnum.Release:
								artView = new ReleaseManager().RetrieveById2(null, ((Release)art).ReleaseId, true);
								break;
						}
					}
					catch (Exception ex)
					{
						Logger.LogWarningEvent(METHOD_NAME, ex, "Pulling ArtifactView from database.");
						artView = null;
					}
				}
				if (artView != null)
					art = artView;

				//The function to call when when the RegEx finds a match.
				MatchEvaluator matchRepl = new MatchEvaluator(delegate (Match m) { return ReplaceToken(m, art, artFields, longTextFields, userId); });

				//Get matches for tokens..
				string finishedTemplate = regTempl.Replace(templateText, matchRepl);

				Logger.LogExitingEvent(METHOD_NAME);
				return finishedTemplate;
			}
			catch (Exception exception)
			{
				Logger.LogErrorEvent(METHOD_NAME, exception);
				throw;
			}
		}
		#endregion

		#region Mail Sending
		/// <summary>Sends any emails for Events or Subscribed Artifacts.</summary>
		/// <param name="art">The artifact, marked as 'Changed' in StateChangeTracker.</param>
		/// <param name="custProp">The artifact's Custom Properties, marked as 'Changed' in StateChangeTracker.</param>
		/// <param name="newComment">Any new comment being added to the artifact. Requires the Artifact to be populated.</param>
		/// <param name="secondaryArtifact">The secondary artifact</param>
		/// <returns>The number of Notification Events that matched the changes.</returns>
		public int SendNotificationForArtifact(Artifact art = null, ArtifactCustomProperty custProp = null, string newComment = null, Artifact secondaryArtifact = null, int? workflowId = null, int? originalStatusId = null, int? loggedUserId = null)
		{
			const string METHOD = CLASS + "SendNotificationForArtifact()";
			Logger.LogEnteringEvent(METHOD);

			//Check that we have information.
			if (!string.IsNullOrWhiteSpace(newComment) && art == null)
				throw new ArgumentException("Artifact must be populated when adding a new comment.");

			//Now pull our data.
			int retEvtNum = 0;
			if (art != null || custProp != null || !string.IsNullOrWhiteSpace(newComment))
			{
				//Create the Email Message..
				EmailMessageDetails msgToSend = new EmailMessageDetails();
				msgToSend.projectToken = "PR-";

				//Get the artifact modification type.
				bool flagNewComment = (!string.IsNullOrWhiteSpace(newComment));
				ObjectState dataState = ObjectState.Unchanged;
				if (art != null)
				{
					dataState = art.EntityChangeTracker.State;

					//See if we need to override the state..
					if (OverrideNewtoNone((string)art["Name"]))
						dataState = ObjectState.Unchanged;
					else if (art.EntityChangeTracker.OriginalValues.ContainsKey("Name") &&
						OverrideUpdatedToNew((string)art.EntityChangeTracker.OriginalValues["Name"], (string)art["Name"]))
					{
						dataState = ObjectState.Added;
					}
				}

				//Get the artifact's token.
				msgToSend.artifactTokens.Add(1, art.ArtifactToken);

				//Get Project ID and events for this project.
				int projectId = (int)art["ProjectId"];
				msgToSend.projectToken = "PR-" + projectId.ToSafeString();

				//Get the template associated with the project. Only active ones!
				int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;
				List<NotificationEvent> notifyEvents = RetrieveEvents(projectTemplateId)
					.Where(t => t.IsActive)
					.ToList();

				//Get all avaialble fields..
				List<ArtifactField> artFields = new ArtifactManager().ArtifactField_RetrieveAll((int)art.ArtifactType, false, false);

				switch (dataState)
				{
					#region New Item
					case ObjectState.Added:
						{
							//The item was added. See if any event match that criteria.
							List<NotificationEvent> matchingEvts = notifyEvents.Where(evt => evt.IsArtifactCreation && evt.ArtifactTypeId == (int)art.ArtifactType).ToList();
							if (matchingEvts.Count > 0)
							{
								//Loop through each event we have..
								foreach (NotificationEvent notificationEvent in matchingEvts)
								{
									//Get the full Notification Event, with the child collections
									NotificationEvent evt = RetrieveEventById(notificationEvent.NotificationEventId);

									//Now, add the event data to our class. Get the number (index) and translated subject.
									int subjectNum = msgToSend.subjectList.Count + 1;
									msgToSend = new EmailMessageDetails();

									if (msgToSend.projectToken == null)
									{
										msgToSend.artifactTokens.Add(1, art.ArtifactToken);

										msgToSend.projectToken = "PR-" + projectId.ToSafeString();
									}
									string subj = TranslateTemplate(art, (string)evt.EmailSubject, artFields, false);
									msgToSend.subjectList.Add(subjectNum, subj);

									//Now, get the users that this event is going out to.
									List<UserEmailDetails> emailList = new List<UserEmailDetails>();
									GenerateEmailList(evt, art, ref emailList);
									if (workflowId != null && originalStatusId != null)
									{
										List<int> approvalUsers = new TestCaseWorkflowManager().WorkflowTransition_RetrieveApproversByInputStatus1((int)workflowId, (int)originalStatusId, projectId);
										if (approvalUsers != null && approvalUsers.Count != 0)
										{
											//if (msgToSend.su != "Test Case Approval Notification.")
											//{
											msgToSend = new EmailMessageDetails();

											subj = "Test Case Approval Notification.";

											if (msgToSend.projectToken == null)
											{
												msgToSend.artifactTokens.Add(1, art.ArtifactToken);

												msgToSend.projectToken = "PR-" + projectId.ToSafeString();
											}
											msgToSend.subjectList.Add(subjectNum, subj);
											//}

											foreach (var user in approvalUsers)
											{
												User toUser = new UserManager().GetUserById(user);
												UserEmailDetails approvalEmail = new UserEmailDetails();
												approvalEmail.ID = toUser.UserId;
												approvalEmail.Email = toUser.EmailAddress;
												approvalEmail.Name = toUser.UserName;

												emailList.Add(approvalEmail);
											}
										}
									}
									//Now we got our list of users, let's add them to the email class.
									foreach (UserEmailDetails email in emailList)
									{
										EmailMessageDetails.EmailMessageDetailUser user = new EmailMessageDetails.EmailMessageDetailUser();
										user.Address = email.Email;
										user.Name = email.Name;
										user.ArtifactTokenId = 1;
										user.UserId = email.ID;
										user.SubjectId = subjectNum;
										user.Source = "<NE-" + ((int)notificationEvent["NotificationEventId"]).ToSafeString() + ">";

										//Add it to the email details.
										msgToSend.toUserList.Add(user);
									}
								}

								//Get subscribed users for this artifact.
								List<UserEmailDetails> subList = new List<UserEmailDetails>();
								GenerateSubscriptionList(art, ref subList);
								foreach (UserEmailDetails user in subList)
								{
									EmailMessageDetails.EmailMessageDetailUser usr = new EmailMessageDetails.EmailMessageDetailUser();
									usr.Address = user.Email;
									usr.Name = user.Name;
									usr.ArtifactTokenId = 1;
									usr.UserId = user.ID;
									usr.SubjectId = -2;
									usr.Source = "Sub";

									if (msgToSend.toUserList.Count(u => u.UserId == usr.UserId) == 0)
										msgToSend.toUserList.Add(usr);
								}
								//Add subscription subject.
								msgToSend.subjectList.Add(-2, TranslateTemplate(art, GlobalResources.General.Notification_SubscriptionSubject, artFields, false, loggedUserId));

								//Now translate the template if we're actually sending out a notification.
								if (msgToSend.toUserList.Count > 0)
								{
									string templateText = RetrieveTemplateTextById(projectTemplateId, (int)art.ArtifactType);
									string strBody = TranslateTemplate(art, templateText, artFields, true, loggedUserId);
									SendEmail(msgToSend, strBody);
								}

							}
							if(workflowId != null)
							{
								//Now, add the event data to our class. Get the number (index) and translated subject.
								int subjectNum = msgToSend.subjectList.Count + 1;
								
								//Now, get the users that this event is going out to.
								List<UserEmailDetails> emailList = new List<UserEmailDetails>();
								//GenerateEmailList(evt, art, ref emailList);

								List<int> approvalUsers = new TestCaseWorkflowManager().WorkflowTransition_RetrieveApproversByInputStatus1((int)workflowId, (int)originalStatusId, projectId);
								if (approvalUsers != null && approvalUsers.Count != 0)
								{
									//if (msgToSend.su != "Test Case Approval Notification.")
									//{
									msgToSend = new EmailMessageDetails();

									string subj = "Test Case Approval Notification.";

									if (msgToSend.projectToken == null)
									{
										msgToSend.artifactTokens.Add(1, art.ArtifactToken);

										msgToSend.projectToken = "PR-" + projectId.ToSafeString();
									}
									msgToSend.subjectList.Add(subjectNum, subj);
									//}
									foreach (var user in approvalUsers)
									{
										User toUser = new UserManager().GetUserById(user);
										UserEmailDetails approvalEmail = new UserEmailDetails();
										approvalEmail.ID = toUser.UserId;
										approvalEmail.Email = toUser.EmailAddress;
										approvalEmail.Name = toUser.UserName;

										emailList.Add(approvalEmail);
									}
								}

								//Now we got our list of users, let's add them to the email class.
								foreach (UserEmailDetails email in emailList)
								{
									EmailMessageDetails.EmailMessageDetailUser user = new EmailMessageDetails.EmailMessageDetailUser();
									user.Address = email.Email;
									user.Name = email.Name;
									user.ArtifactTokenId = 1;
									user.UserId = email.ID;
									user.SubjectId = subjectNum;
									user.Source = "<NE->";

									//Add it to the email details.
									msgToSend.toUserList.Add(user);
								}

								//Get subscribed users for this artifact.
								List<UserEmailDetails> subList = new List<UserEmailDetails>();
								GenerateSubscriptionList(art, ref subList);
								foreach (UserEmailDetails user in subList)
								{
									EmailMessageDetails.EmailMessageDetailUser usr = new EmailMessageDetails.EmailMessageDetailUser();
									usr.Address = user.Email;
									usr.Name = user.Name;
									usr.ArtifactTokenId = 1;
									usr.UserId = user.ID;
									usr.SubjectId = -2;
									usr.Source = "Sub";

									if (msgToSend.toUserList.Count(u => u.UserId == usr.UserId) == 0)
										msgToSend.toUserList.Add(usr);
								}
								//Add subscription subject.
								msgToSend.subjectList.Add(-2, TranslateTemplate(art, GlobalResources.General.Notification_SubscriptionSubject, artFields, false, loggedUserId));

								//Now translate the template if we're actually sending out a notification.
								if (msgToSend.toUserList.Count > 0)
								{
									string templateText = RetrieveTemplateTextById(projectTemplateId, (int)art.ArtifactType);
									string strBody = TranslateTemplate(art, templateText, artFields, true, loggedUserId);
									SendEmail(msgToSend, strBody);
								}
							}
						}
						break;
					#endregion //New Item

					#region Modified Item
					case ObjectState.Modified:
						{
							//Get events related to this artifact type.
							List<NotificationEvent> matchingEvts = notifyEvents.Where(evt => evt.ArtifactTypeId == (int)art.ArtifactType).ToList();
														
							//Loop through each event and see if that event is looking for a changed field we have.
							foreach (NotificationEvent evtRow in matchingEvts)
							{
								NotificationEvent evt = RetrieveEventById(evtRow.NotificationEventId);

								//Flag to indicate if this event matches.
								bool evtMatch = false;

								//Loop through each field in the Event, and see if it's changed.
								for (int j = 0; j < evt.ArtifactFields.Count && evtMatch == false; j++)
								{
									//Now, add the event data to our class. Get the number (index) and translated subject.
									int subjectNum = msgToSend.subjectList.Count + 1;
									msgToSend = new EmailMessageDetails();

									if (msgToSend.projectToken == null)
									{
										msgToSend.artifactTokens.Add(1, art.ArtifactToken);

										msgToSend.projectToken = "PR-" + projectId.ToSafeString();
									}
									string subj = TranslateTemplate(art, (string)evt.EmailSubject, artFields, false);
									msgToSend.subjectList.Add(subjectNum, subj);

									//The current field.
									ArtifactField fieldRow = evt.ArtifactFields[j];

									try
									{
										//Find the definition of the field in our list.
										ArtifactField artField = artFields.SingleOrDefault(field => field.ArtifactFieldId == fieldRow.ArtifactFieldId);
										if (artField != null)
										{
											//Loop through each field, and see if it matches an event field.
											switch (artField.Name.Trim().ToLowerInvariant())
											{
												case "comments":
												case "resolution": //Special field. Called out to check if a new comment (not actual entity field) is added.
													{
														if (flagNewComment) //Flag indicating if we have a new comment.
															evtMatch = true;
													}
													break;
												case "issuspect":
													evtMatch = true;
													break;
												default: //All other fields should be an indexer (["fieldname"]) on the artifact.
													{
														//See if the field we're looking for is even contained in the artifact. (It may not be. Better be safe than sorry.)
														if (art.ContainsProperty(artField.Name))
														{
															//Check to see if the values changed.
															if (art.EntityChangeTracker.OriginalValues.ContainsKey(artField.Name) &&
																!CheckValuesEqual(art[artField.Name], art.EntityChangeTracker.OriginalValues[artField.Name], artField))
															{
																evtMatch = true;
															}
														}
														else if (secondaryArtifact != null && secondaryArtifact.ContainsProperty(artField.Name))
														{
															//Check to see if the values changed.
															if (secondaryArtifact.EntityChangeTracker.OriginalValues.ContainsKey(artField.Name) &&
																!CheckValuesEqual(secondaryArtifact[artField.Name], secondaryArtifact.EntityChangeTracker.OriginalValues[artField.Name], artField))
															{
																evtMatch = true;
															}
														}
														else
														{
															string msg = "Event #" +
																evt.NotificationEventId.ToSafeString() +
																" asked for field '" +
																artField.Name +
																"' which did not exist. Ignoring.";
															Logger.LogWarningEvent(METHOD, msg);
														}
													}
													break;
											}

											//If we have a matching event, get the users for it.
											if (evtMatch)
											{
												//Add one to the count.
												retEvtNum++;

												//Get users for this event.
												List<UserEmailDetails> emailList = new List<UserEmailDetails>();
												GenerateEmailList(evt, art, ref emailList);

												//Now loop through each user and add them to the message.
												foreach (UserEmailDetails user in emailList)
												{
													EmailMessageDetails.EmailMessageDetailUser usr = new EmailMessageDetails.EmailMessageDetailUser();
													usr.Address = user.Email;
													usr.Name = user.Name;
													usr.ArtifactTokenId = 1;
													usr.UserId = user.ID;
													usr.SubjectId = subjectNum;
													usr.Source = "<NE-" + evtRow.NotificationEventId.ToSafeString() + ">";

													if (msgToSend.toUserList.Count(u => u.UserId == usr.UserId) == 0)
														msgToSend.toUserList.Add(usr);
												}
												//Get subscribed users for this artifact.
												List<UserEmailDetails> subList = new List<UserEmailDetails>();
												GenerateSubscriptionList(art, ref subList);
												foreach (UserEmailDetails user in subList)
												{
													EmailMessageDetails.EmailMessageDetailUser usr = new EmailMessageDetails.EmailMessageDetailUser();
													usr.Address = user.Email;
													usr.Name = user.Name;
													usr.ArtifactTokenId = 1;
													usr.UserId = user.ID;
													usr.SubjectId = -2;
													usr.Source = "Sub";

													if (msgToSend.toUserList.Count(u => u.UserId == usr.UserId) == 0)
														msgToSend.toUserList.Add(usr);
												}
												//Add subscription subject.
												msgToSend.subjectList.Add(-2, TranslateTemplate(art, GlobalResources.General.Notification_SubscriptionSubject, artFields, false, loggedUserId));

												//Now translate the template if we're actually sending out a notification.
												if (msgToSend.toUserList.Count > 0)
												{
													string templateText = RetrieveTemplateTextById(projectTemplateId, (int)art.ArtifactType);
													string strBody = TranslateTemplate(art, templateText, artFields, true, loggedUserId);
													SendEmail(msgToSend, strBody);
												}
											}

											if (workflowId != null)
											{
												msgToSend = new EmailMessageDetails();

												subj = "Test Case Approval Notification.";

												if (msgToSend.projectToken == null)
												{
													msgToSend.artifactTokens.Add(1, art.ArtifactToken);

													msgToSend.projectToken = "PR-" + projectId.ToSafeString();
												}
												msgToSend.subjectList.Add(subjectNum, subj);

												List<int> approvalUsers = new TestCaseWorkflowManager().WorkflowTransition_RetrieveApproversByInputStatus1((int)workflowId, (int)originalStatusId, projectId);

												//Get users for this event.
												List<UserEmailDetails> emailList = new List<UserEmailDetails>();
												
												if (approvalUsers != null && approvalUsers.Count != 0)
												{
													foreach (var user in approvalUsers)
													{
														User toUser = new UserManager().GetUserById(user);
														UserEmailDetails approvalEmail = new UserEmailDetails();
														approvalEmail.ID = toUser.UserId;
														approvalEmail.Email = toUser.EmailAddress;
														approvalEmail.Name = toUser.UserName;

														emailList.Add(approvalEmail);
													}

													//Now loop through each user and add them to the message.
													foreach (UserEmailDetails user in emailList)
													{
														EmailMessageDetails.EmailMessageDetailUser usr = new EmailMessageDetails.EmailMessageDetailUser();
														usr.Address = user.Email;
														usr.Name = user.Name;
														usr.ArtifactTokenId = 1;
														usr.UserId = user.ID;
														usr.SubjectId = subjectNum;
														usr.Source = "<NE-" + evtRow.NotificationEventId.ToSafeString() + ">";

														if (msgToSend.toUserList.Count(u => u.UserId == usr.UserId) == 0)
															msgToSend.toUserList.Add(usr);
													}
												}

												//Get subscribed users for this artifact.
												List<UserEmailDetails> subList = new List<UserEmailDetails>();
												GenerateSubscriptionList(art, ref subList);
												foreach (UserEmailDetails user in subList)
												{
													EmailMessageDetails.EmailMessageDetailUser usr = new EmailMessageDetails.EmailMessageDetailUser();
													usr.Address = user.Email;
													usr.Name = user.Name;
													usr.ArtifactTokenId = 1;
													usr.UserId = user.ID;
													usr.SubjectId = -2;
													usr.Source = "Sub";

													if (msgToSend.toUserList.Count(u => u.UserId == usr.UserId) == 0)
														msgToSend.toUserList.Add(usr);
												}
												//Add subscription subject.
												msgToSend.subjectList.Add(-2, TranslateTemplate(art, GlobalResources.General.Notification_SubscriptionSubject, artFields, false, loggedUserId));

												//Now translate the template if we're actually sending out a notification.
												if (msgToSend.toUserList.Count > 0)
												{
													string templateText = RetrieveTemplateTextById(projectTemplateId, (int)art.ArtifactType);
													string strBody = TranslateTemplate(art, templateText, artFields, true, loggedUserId);
													SendEmail(msgToSend, strBody);
												}
											}

											if (loggedUserId != null)
											{
												var approvalUsers = new UserManager().RetrieveAssignedApproversToSendEmail(projectId, (int)loggedUserId); 
												subj = "Requirement Approve all Notification";
												msgToSend.subjectList.Add(subjectNum, subj);

												//Get users for this event.
												List<UserEmailDetails> emailList = new List<UserEmailDetails>();

												if (approvalUsers != null && approvalUsers.Count != 0)
												{
													foreach (var user in approvalUsers)
													{
														User toUser = new UserManager().GetUserById(user.UserId);
														UserEmailDetails approvalEmail = new UserEmailDetails();
														approvalEmail.ID = toUser.UserId;
														approvalEmail.Email = toUser.EmailAddress;
														approvalEmail.Name = toUser.UserName;

														emailList.Add(approvalEmail);
													}

													//Now loop through each user and add them to the message.
													foreach (UserEmailDetails user in emailList)
													{
														EmailMessageDetails.EmailMessageDetailUser usr = new EmailMessageDetails.EmailMessageDetailUser();
														usr.Address = user.Email;
														usr.Name = user.Name;
														usr.ArtifactTokenId = 1;
														usr.UserId = user.ID;
														usr.SubjectId = subjectNum;
														usr.Source = "<NE-" + evtRow.NotificationEventId.ToSafeString() + ">";

														if (msgToSend.toUserList.Count(u => u.UserId == usr.UserId) == 0)
															msgToSend.toUserList.Add(usr);
													}
												}

												//Get subscribed users for this artifact.
												List<UserEmailDetails> subList = new List<UserEmailDetails>();
												GenerateSubscriptionList(art, ref subList);
												foreach (UserEmailDetails user in subList)
												{
													EmailMessageDetails.EmailMessageDetailUser usr = new EmailMessageDetails.EmailMessageDetailUser();
													usr.Address = user.Email;
													usr.Name = user.Name;
													usr.ArtifactTokenId = 1;
													usr.UserId = user.ID;
													usr.SubjectId = -2;
													usr.Source = "Sub";

													if (msgToSend.toUserList.Count(u => u.UserId == usr.UserId) == 0)
														msgToSend.toUserList.Add(usr);
												}
												//Add subscription subject.
												msgToSend.subjectList.Add(-2, TranslateTemplate(art, GlobalResources.General.Notification_SubscriptionSubject, artFields, false, loggedUserId));

												//Now translate the template if we're actually sending out a notification.
												if (msgToSend.toUserList.Count > 0)
												{
													string templateText = RetrieveTemplateTextById(projectTemplateId, (int)art.ArtifactType);
													string strBody = TranslateTemplate(art, templateText, artFields, true, loggedUserId);
													SendEmail(msgToSend, strBody);
												}
											}
										}
									}
									catch (Exception ex)
									{
										Logger.LogErrorEvent(METHOD, ex, "Error processing Event #" + evt.NotificationEventId.ToSafeString() + ".");
									}
								}
							}
						}
						break;
					#endregion //Modified Item

					#region Comment Added without other changes

					case ObjectState.Unchanged:
						{
							//Unchanged, only send if a new comment was added
							if (flagNewComment)
							{
								//Get events related to this artifact type.
								List<NotificationEvent> matchingEvts = notifyEvents.Where(evt => evt.ArtifactTypeId == (int)art.ArtifactType).ToList();

								//Loop through each event and see if that event fires on a comment
								foreach (NotificationEvent evtRow in matchingEvts)
								{
									NotificationEvent evt = RetrieveEventById(evtRow.NotificationEventId);

									//See if this fires on comments being added
									foreach (ArtifactField fieldRow in evt.ArtifactFields)
									{
										//Now, add the event data to our class. Get the number (index) and translated subject.
										int subjectNum = msgToSend.subjectList.Count + 1;
										string subj = TranslateTemplate(art, evt.EmailSubject, artFields, false);
										msgToSend.subjectList.Add(subjectNum, subj);

										//Find the definition of the field in our list.
										ArtifactField artField = artFields.SingleOrDefault(field => field.ArtifactFieldId == fieldRow.ArtifactFieldId);
										if (artField != null)
										{
											if (artField.Name.Trim().ToLowerInvariant() == "comments" || artField.Name.Trim().ToLowerInvariant() == "resolution")
											{
												//Add one to the count.
												retEvtNum++;

												//Get users for this event.
												List<UserEmailDetails> emailList = new List<UserEmailDetails>();
												GenerateEmailList(evt, art, ref emailList);

												//Now loop through each user and add them to the message.
												foreach (UserEmailDetails user in emailList)
												{
													EmailMessageDetails.EmailMessageDetailUser usr = new EmailMessageDetails.EmailMessageDetailUser();
													usr.Address = user.Email;
													usr.Name = user.Name;
													usr.ArtifactTokenId = 1;
													usr.UserId = user.ID;
													usr.SubjectId = subjectNum;
													usr.Source = "<NE-" + evtRow.NotificationEventId.ToSafeString() + ">";

													if (msgToSend.toUserList.Count(u => u.UserId == usr.UserId) == 0)
														msgToSend.toUserList.Add(usr);
												}
												break;
											}
										}										
									}
								}

								//Get subscribed users for this artifact.
								List<UserEmailDetails> subList = new List<UserEmailDetails>();
								GenerateSubscriptionList(art, ref subList);
								foreach (UserEmailDetails user in subList)
								{
									EmailMessageDetails.EmailMessageDetailUser usr = new EmailMessageDetails.EmailMessageDetailUser();
									usr.Address = user.Email;
									usr.Name = user.Name;
									usr.ArtifactTokenId = 1;
									usr.UserId = user.ID;
									usr.SubjectId = -2;
									usr.Source = "Sub";

									if (msgToSend.toUserList.Count(u => u.UserId == usr.UserId) == 0)
										msgToSend.toUserList.Add(usr);
								}
								//Add subscription subject.
								msgToSend.subjectList.Add(-2, TranslateTemplate(art, GlobalResources.General.Notification_SubscriptionSubject, artFields, false, loggedUserId));

								//Now translate the template if we're actually sending out a notification.
								if (msgToSend.toUserList.Count > 0)
								{
									string templateText = RetrieveTemplateTextById(projectTemplateId, (int)art.ArtifactType);
									string strBody = TranslateTemplate(art, templateText, artFields, true, loggedUserId);
									SendEmail(msgToSend, strBody);
								}
							}
							if (loggedUserId != null)
							{
								//Now, add the event data to our class. Get the number (index) and translated subject.
								int subjectNum = msgToSend.subjectList.Count + 1;
								string subj = "Requirement Approve all Notification";
								msgToSend.subjectList.Add(subjectNum, subj);

								var approvalUsers = new UserManager().RetrieveAssignedApproversToSendEmail(projectId, (int)loggedUserId);

								//Get users for this event.
								List<UserEmailDetails> emailList = new List<UserEmailDetails>();

								if (approvalUsers != null && approvalUsers.Count != 0)
								{
									foreach (var user in approvalUsers)
									{
										User toUser = new UserManager().GetUserById(user.UserId);
										UserEmailDetails approvalEmail = new UserEmailDetails();
										approvalEmail.ID = toUser.UserId;
										approvalEmail.Email = toUser.EmailAddress;
										approvalEmail.Name = toUser.UserName;

										emailList.Add(approvalEmail);
									}

									//Now loop through each user and add them to the message.
									foreach (UserEmailDetails user in emailList)
									{
										EmailMessageDetails.EmailMessageDetailUser usr = new EmailMessageDetails.EmailMessageDetailUser();
										usr.Address = user.Email;
										usr.Name = user.Name;
										usr.ArtifactTokenId = 1;
										usr.UserId = user.ID;
										usr.SubjectId = subjectNum;
										usr.Source = "<NE->";

										if (msgToSend.toUserList.Count(u => u.UserId == usr.UserId) == 0)
											msgToSend.toUserList.Add(usr);
									}
								}

								//Get subscribed users for this artifact.
								List<UserEmailDetails> subList = new List<UserEmailDetails>();
								GenerateSubscriptionList(art, ref subList);
								foreach (UserEmailDetails user in subList)
								{
									EmailMessageDetails.EmailMessageDetailUser usr = new EmailMessageDetails.EmailMessageDetailUser();
									usr.Address = user.Email;
									usr.Name = user.Name;
									usr.ArtifactTokenId = 1;
									usr.UserId = user.ID;
									usr.SubjectId = -2;
									usr.Source = "Sub";

									if (msgToSend.toUserList.Count(u => u.UserId == usr.UserId) == 0)
										msgToSend.toUserList.Add(usr);
								}
								//Add subscription subject.
								msgToSend.subjectList.Add(-2, TranslateTemplate(art, GlobalResources.General.Notification_SubscriptionSubject, artFields, false, loggedUserId));

								//Now translate the template if we're actually sending out a notification.
								if (msgToSend.toUserList.Count > 0)
								{
									string templateText = RetrieveTemplateTextById(projectTemplateId, (int)art.ArtifactType);
									string strBody = TranslateTemplate(art, templateText, artFields, true, loggedUserId);
									SendEmail(msgToSend, strBody);
								}
							}
						}
						break;

					#endregion

					#region Unused States

					case ObjectState.Deleted:
						{
							//At this time, events are not configurable for deleting items.
						}
						break;
						#endregion //Unused States
				}

				////Get subscribed users for this artifact.
				//List<UserEmailDetails> subList = new List<UserEmailDetails>();
				//GenerateSubscriptionList(art, ref subList);
				//foreach (UserEmailDetails user in subList)
				//{
				//	EmailMessageDetails.EmailMessageDetailUser usr = new EmailMessageDetails.EmailMessageDetailUser();
				//	usr.Address = user.Email;
				//	usr.Name = user.Name;
				//	usr.ArtifactTokenId = 1;
				//	usr.UserId = user.ID;
				//	usr.SubjectId = -2;
				//	usr.Source = "Sub";

				//	if (msgToSend.toUserList.Count(u => u.UserId == usr.UserId) == 0)
				//		msgToSend.toUserList.Add(usr);
				//}
				////Add subscription subject.
				//msgToSend.subjectList.Add(-2, TranslateTemplate(art, GlobalResources.General.Notification_SubscriptionSubject, artFields, false, loggedUserId));

				////Now translate the template if we're actually sending out a notification.
				//if (msgToSend.toUserList.Count > 0)
				//{
				//	string templateText = RetrieveTemplateTextById(projectTemplateId, (int)art.ArtifactType);
				//	string strBody = TranslateTemplate(art, templateText, artFields, true, loggedUserId);
				//	SendEmail(msgToSend, strBody);
				//}
			}

			Logger.LogExitingEvent(METHOD);
			return retEvtNum;
		}

		/// <summary>Collects e-mails from the event specified to send a notification to.</summary>
		/// <param name="eventData">The event to pull e-mails for.</param>
		/// <param name="emailList">A list to save e-mail addresses into.</param>
		/// <param name="art">The artifact that we're sending the email for.</param>
		private void GenerateEmailList(NotificationEvent eventData, Artifact art, ref List<UserEmailDetails> emailList)
		{
			const string METHOD = CLASS + "GenerateEmailList()";
			Logger.LogEnteringEvent(METHOD);

			if (emailList == null) emailList = new List<UserEmailDetails>();
			UserManager usrMgr = new UserManager();

			//We only want to continue if we have an artifact.
			if (art != null)
			{
				#region Artifact Users
				//Set the parameter fields, first:
				string creatorField = "";
				switch (art.ArtifactType)
				{
					case Artifact.ArtifactTypeEnum.Requirement:
						creatorField = "AuthorId";
						break;
					case Artifact.ArtifactTypeEnum.TestCase:
						creatorField = "AuthorId";
						break;
					case Artifact.ArtifactTypeEnum.Incident:
						creatorField = "OpenerId";
						break;
					case Artifact.ArtifactTypeEnum.Task:
						creatorField = "CreatorId";
						break;
					case Artifact.ArtifactTypeEnum.TestSet:
						creatorField = "CreatorId";
						break;
					case Artifact.ArtifactTypeEnum.Document:
						creatorField = "AuthorId";
						break;
					case Artifact.ArtifactTypeEnum.Risk:
						creatorField = "CreatorId";
						break;
					case Artifact.ArtifactTypeEnum.Release:
						creatorField = "CreatorId";
						break;
					case Artifact.ArtifactTypeEnum.TestRun:
					case Artifact.ArtifactTypeEnum.TestStep:
					default:
						return;
				}

				//Specify the 'owner field'
				string ownerField = "OwnerId";
				if (art.ArtifactType == Artifact.ArtifactTypeEnum.Document)
				{
					ownerField = "EditorId";
				}

				//First get the owner and creator, if specified.
				if (eventData.NotificationArtifactUserTypes != null && eventData.NotificationArtifactUserTypes.Count > 0)
				{
					foreach (NotificationArtifactUserType userRow in eventData.NotificationArtifactUserTypes)
					{
						switch (userRow.ProjectArtifactNotifyTypeId)
						{
							case 1: //The Creator.
									//Get the creatorId from the artifact.
								if (art.ContainsProperty(creatorField) && art[creatorField] != DBNull.Value && art[creatorField] != null)
								{
									try
									{
										//Get the user's details.
										User creatorUser = usrMgr.GetUserById((int)art[creatorField]);

										//Check to see if the user turned off emails.
										if ((ConfigurationSettings.Default.EmailSettings_AllowUserControl && creatorUser.Profile.IsEmailEnabled)
											|| !ConfigurationSettings.Default.EmailSettings_AllowUserControl)
										{
											//See if the email address is already in there..
											if (emailList.Where(el => el.ID == creatorUser.UserId).Count() == 0)
											{
												UserEmailDetails usr = new UserEmailDetails();
												usr.Email = creatorUser.EmailAddress;
												usr.ID = creatorUser.UserId;
												usr.Name = creatorUser.FullName;

												emailList.Add(usr);
											}
										}
									}
									catch (ArtifactNotExistsException)
									{
										//user doesn't exist so just ignore it.
									}
									catch (Exception ex)
									{
										Logger.LogErrorEvent(METHOD, ex, "Trying to add creator of item into Email list.");
									}
								}
								break;

							case 2: //The Owner
									//Get the Owner ID from the artifact.
								if (art.ContainsProperty(ownerField) && art[ownerField] != DBNull.Value && art[ownerField] != null)
								{
									try
									{
										//Get the user's details.
										User ownerUser = usrMgr.GetUserById((int)art[ownerField]);

										//Check to see if the user turned off emails.
										if ((ConfigurationSettings.Default.EmailSettings_AllowUserControl && ownerUser.Profile.IsEmailEnabled)
											|| !ConfigurationSettings.Default.EmailSettings_AllowUserControl)
										{
											//See if the email address is already in there..
											if (emailList.Where(el => el.ID == ownerUser.UserId).Count() == 0)
											{
												UserEmailDetails usr = new UserEmailDetails();
												usr.Email = ownerUser.EmailAddress;
												usr.ID = ownerUser.UserId;
												usr.Name = ownerUser.FullName;

												emailList.Add(usr);
											}
										}
									}
									catch (ArtifactNotExistsException)
									{
										//user doesn't exist so just ignore
									}
									catch (Exception ex)
									{
										Logger.LogErrorEvent(METHOD, ex, "Trying to add owner of item into Email list.");
									}
								}
								break;
						}
					}
				}
				#endregion

				#region Project Roles
				//For each role specified in the event, pull users and add them if they're not added already.
				if (eventData.ProjectRoles != null && eventData.ProjectRoles.Count > 0)
				{
					foreach (ProjectRole roleRow in eventData.ProjectRoles)
					{
						//Get users in that role..
						List<User> roleUsers = usrMgr.RetrieveByProjectRoleId((int)art["ProjectId"], roleRow.ProjectRoleId);

						//For each user, add them to the list.
						foreach (User roleUser in roleUsers)
						{
							if (roleUser.Profile != null)
							{
								//Check to make sure they don't have emails disabled.
								if ((ConfigurationSettings.Default.EmailSettings_AllowUserControl && roleUser.Profile.IsEmailEnabled)
									|| !ConfigurationSettings.Default.EmailSettings_AllowUserControl)
								{
									//See if the email address is already in there. If not, add it.
									if (emailList.Where(el => el.ID == roleUser.UserId).Count() == 0)
									{
										UserEmailDetails usr = new UserEmailDetails();
										usr.Email = roleUser.EmailAddress;
										usr.ID = roleUser.UserId;
										usr.Name = roleUser.FullName;

										emailList.Add(usr);
									}
								}
							}
						}
					}
				}
				#endregion
			}
			else
			{
				string evtTxt = (eventData != null) ? eventData.NotificationEventId.ToSafeString() : "?";
				Logger.LogWarningEvent(METHOD, "Tried to send a notification event (#" + evtTxt + ") on a missing Artifact.");
			}
		}

		/// <summary>Hack to report whether the string given is a new item artifact.</summary>
		/// <param name="NameFieldValue">The current name fo the artifact.</param>
		/// <returns>True if the RowStatus should be overridden.</returns>
		private bool OverrideNewtoNone(string NameFieldValue)
		{
			if (NameFieldValue == "")
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>Hack to report whether the strings given should indicate that it is an Added artifact and not simply Modified.</summary>
		/// <param name="NewNameFieldValue">The new name for the artifact.</param>
		/// <param name="OldNameFieldValue">The original name of the artifact.</param>
		/// <returns>True if the RowStatus should be overridden.</returns>
		private bool OverrideUpdatedToNew(string OldNameFieldValue, string NewNameFieldValue)
		{
			if (OldNameFieldValue == "")
			{
				if (NewNameFieldValue != "")
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>Sends a notification to the specified email address.</summary>
		/// <param name="projId">The projectId of the artifact.</param>
		/// <param name="artifactId">The artifactId to send.</param>
		/// <param name="ArtifactType">The artifactType of the artifactId.</param>
		/// <param name="fromUserId">The userId sending the notification.</param>
		/// <param name="ToEmailList">A list of e-mail addresses, separated by ;'s</param>
		public void SendNotification(int projId, int artifactId, Artifact.ArtifactTypeEnum artifactType, int fromUserId, string toEmailList, string emailSubj = null)
		{
			//Generate list of email users, first..
			List<EmailMessageDetails.EmailMessageDetailUser> msgUsers = new List<EmailMessageDetails.EmailMessageDetailUser>();

			//Add the users to the list..
			string[] emailAddrs = toEmailList.Split(';');
			foreach (string emailAddr in emailAddrs)
			{
				//Make sure the user doesn't already exist.
				if (msgUsers.Count(u => u.Address.Trim() == emailAddr.Trim()) == 0)
					msgUsers.Add(new EmailMessageDetails.EmailMessageDetailUser()
					{
						Address = emailAddr,
						SubjectId = 1,
						UserId = -1,
						ArtifactTokenId = 1,
						Name = "",
						Source = "Direct"
					});
			}

			//Assuming we have at least one user, call the master function!
			if (msgUsers.Count > 0)
			{
				sendNotification(projId, artifactId, artifactType, fromUserId, msgUsers, emailSubj);
			}
		}

		/// <summary>Sends a notification to the specified user.</summary>
		/// <param name="artifactId">The artifactId to send.</param>
		/// <param name="ArtifactType">The artifactType of the artifactId.</param>
		/// <param name="toUserId">The userId to send the email to.</param>
		public void SendNotification(int projId, int artifactId, Artifact.ArtifactTypeEnum artifactType, int fromUserId, int toUserId, string emailSubj = null)
		{
			//Generate list of email users, first..
			List<EmailMessageDetails.EmailMessageDetailUser> msgUsers = new List<EmailMessageDetails.EmailMessageDetailUser>();

			//Get the user specified...
			User usr = new UserManager().GetUserById(toUserId, false);

			if (usr != null)
			{
				msgUsers.Add(new EmailMessageDetails.EmailMessageDetailUser()
				{
					Address = usr.EmailAddress,
					SubjectId = 1,
					UserId = toUserId,
					ArtifactTokenId = 1,
					Name = usr.FullName,
					Source = "Direct"
				});
			}

			//Assuming we have at least one user, call the master function!
			if (msgUsers.Count > 0)
			{
				sendNotification(projId, artifactId, artifactType, fromUserId, msgUsers, emailSubj);
			}
		}

		/// <summary>Sends a notification for the given artifact to the specified users.</summary>
		/// <param name="projectId">The project ID of the artifact.</param>
		/// <param name="artifactId">The artifact's ID.</param>
		/// <param name="artifactType">The artifact type.</param>
		/// <param name="fromUserId">The user ID sending the message.</param>
		/// <param name="userList">List of users we're sending TO.</param>
		/// <param name="emailSubj">The subject of the email to send.</param>
		private void sendNotification(int projectId, int artifactId, Artifact.ArtifactTypeEnum artifactType, int fromUserId, List<EmailMessageDetails.EmailMessageDetailUser> userList, string emailSubj = null)
		{
			const string METHOD = "SendNotification()";
			Logger.LogEnteringEvent(METHOD);

			//Generate the email class.
			EmailMessageDetails msgToSend = new EmailMessageDetails();

			//Set what we can now..
			msgToSend.toUserList = userList;
			msgToSend.projectToken = "PR-" + projectId.ToSafeString();

			//Get the From user..
			if (fromUserId > 0)
			{
				User fromUsr = new UserManager().GetUserById(fromUserId);
				if (fromUsr != null)
					msgToSend.fromUser = fromUsr;
			}

			//Now get the data for the template..
			try
			{
				//Sending a specific artifact, to a specified user(s).
				Artifact artToSend = null;
				try
				{

					switch (artifactType)
					{
						case Artifact.ArtifactTypeEnum.Requirement:
							RequirementView reqData = new RequirementManager().RetrieveById2(projectId, artifactId);
							if (reqData != null)
							{
								artToSend = reqData;

								//Add the token..
								msgToSend.artifactTokens.Add(1, reqData.ArtifactToken);
							}
							break;

						case Artifact.ArtifactTypeEnum.Document:
							ProjectAttachmentView attachmentView = new AttachmentManager().RetrieveForProjectById2(projectId, artifactId);
							if (attachmentView != null)
							{
								artToSend = attachmentView;

								//Add the token..
								msgToSend.artifactTokens.Add(1, attachmentView.ArtifactToken);
							}
							break;

						case Artifact.ArtifactTypeEnum.TestCase:
							TestCaseView testCaseView = new TestCaseManager().RetrieveById(projectId, artifactId);
							if (testCaseView != null)
							{
								artToSend = testCaseView;

								//Add the token..
								msgToSend.artifactTokens.Add(1, testCaseView.ArtifactToken);
							}

							break;

						case Artifact.ArtifactTypeEnum.Incident:
							Incident incData = new IncidentManager().RetrieveById(artifactId, true);
							if (incData != null)
							{
								artToSend = incData;

								//Add the token..
								msgToSend.artifactTokens.Add(1, incData.ArtifactToken);
							}
							break;

						case Artifact.ArtifactTypeEnum.Task:
							TaskView taskData = new TaskManager().TaskView_RetrieveById(artifactId);
							if (taskData != null)
							{
								artToSend = taskData;

								//Add the token..
								msgToSend.artifactTokens.Add(1, taskData.ArtifactToken);
							}
							break;

						case Artifact.ArtifactTypeEnum.TestSet:
							TestSetView testSetView = new TestSetManager().RetrieveById(projectId, artifactId);
							if (testSetView != null)
							{
								artToSend = testSetView;

								//Add the token..
								msgToSend.artifactTokens.Add(1, testSetView.ArtifactToken);
							}
							break;

						case Artifact.ArtifactTypeEnum.Risk:
							RiskView risk = new RiskManager().Risk_RetrieveById2(artifactId);
							if (risk != null)
							{
								artToSend = risk;

								//Add the token..
								msgToSend.artifactTokens.Add(1, risk.ArtifactToken);
							}
							break;

						case Artifact.ArtifactTypeEnum.Release:
							ReleaseView relData = new ReleaseManager().RetrieveById2(projectId, artifactId);
							if (relData != null)
							{
								artToSend = relData;

								//Add the token..
								msgToSend.artifactTokens.Add(1, relData.ArtifactToken);
							}
							break;

						case Artifact.ArtifactTypeEnum.TestRun:
						case Artifact.ArtifactTypeEnum.TestStep:
						default:
							return;
					}
				}
				catch (Exception exception)
				{
					//Catch the error and continue.
					Logger.LogErrorEvent(METHOD, exception);
				}

				if (artToSend != null)
				{
					//Add the subject..
					if (string.IsNullOrWhiteSpace(emailSubj))
					{
						emailSubj = string.Format(GlobalResources.General.NotificationManager_SendIndivNotification,
							ConfigurationSettings.Default.License_ProductType,
							(artifactType.ToString().Split('.')[artifactType.ToString().Split('.').Length - 1]));
					}
					msgToSend.subjectList.Add(1, emailSubj);

					//Get the template associated with the project
					int projectTemplateId = new TemplateManager().RetrieveForProject(projectId).ProjectTemplateId;

					//Get the template and translate it.
					string strBody = RetrieveTemplateTextById(projectTemplateId, (int)artifactType);
					List<ArtifactField> artFields = new ArtifactManager().ArtifactField_RetrieveAll((int)artToSend.ArtifactType, false, false);
					strBody = TranslateTemplate(artToSend, strBody, artFields, true);

					//Okay, now send it.
					SendEmail(msgToSend, strBody);
				}
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(METHOD, ex);
				throw;
			}
			Logger.LogExitingEvent(METHOD);

		}

		/// <summary>Sends an email message.</summary>
		public void SendEmail(EmailMessageDetails message, string emailBody, string eventName = "")
		{
			const string METHOD = CLASS + "SendEmail()";
			Logger.LogEnteringEvent(METHOD);

			//Get from address and body style.
			string emailFrom = ConfigurationSettings.Default.EmailSettings_EMailFrom;
			bool isHTML = ConfigurationSettings.Default.EmailSettings_SendInHTML;

			//Add the reply seperator line, if necessary.
			if (ConfigurationSettings.Default.EmailSettings_SendSeperator)
			{
				emailBody = GlobalResources.General.NotificationManager_ReplySeperationLine + "<br />" + Environment.NewLine + emailBody;
			}

			//Loop through each person that's getting an email.
			foreach (EmailMessageDetails.EmailMessageDetailUser user in message.toUserList)
			{
				try
				{
					//Translate tokens in the Body & Subject
					// - ${1} - User's Name
					// - ${2} - User's Email Address
					// - ${3} - Event Name
					string msgBody = emailBody.Replace("${1}", user.Name).Replace("${2}", user.Address).Replace("${3}", eventName);
					string msgSubj = message.subjectList[user.SubjectId].Replace("${1}", user.Name).Replace("${2}", user.Address).Replace("${3}", eventName);

					//Strip HTML if necessary.
					if (!isHTML) msgBody = Strings.StripHTML(msgBody);

					//Now make the message and add headers and set flags.
					MailMessage msg = new MailMessage(emailFrom, user.Address, msgSubj, msgBody);
					msg.IsBodyHtml = isHTML;
					msg.HeadersEncoding = System.Text.Encoding.UTF8;
					msg.SubjectEncoding = System.Text.Encoding.UTF8;
					msg.BodyEncoding = System.Text.Encoding.UTF8;
					msg.Sender = new MailAddress(emailFrom);
					msg.Headers.Add(MailClient.MESSAGEHEADER_SPIRA_DATE, MailClient.header_GenerateDate(DateTime.UtcNow));
					msg.Headers.Add(MailClient.MESSAGEHEADER_SPIRA_USERTO, "US-" + user.UserId.ToString());
					msg.Headers.Add(MailClient.MESSAGEHEADER_SPIRA_PROJECT, message.projectToken);
					if (user.ArtifactTokenId.HasValue && message.artifactTokens.ContainsKey(user.ArtifactTokenId.Value))
					{
						msg.Headers.Add(MailClient.MESSAGEHEADER_SPIRA_ARTIFACT, message.artifactTokens[user.ArtifactTokenId.Value]);
						msg.Headers.Add(MailClient.MESSAGEHEADER_SPIRA_SOURCE, user.Source);
						msg.Headers.Add(MailClient.MESSAGEHEADER_INREPLYTO, message.artifactTokens[user.ArtifactTokenId.Value]);
						msg.Headers.Add(MailClient.MESSAGEHEADER_REFERENCES, message.artifactTokens[user.ArtifactTokenId.Value]);
						msg.Subject = "[" + message.artifactTokens[user.ArtifactTokenId.Value] + "] " + msg.Subject;
					}
					if (ConfigurationSettings.Default.EmailSettings_SendFromUser && 
						Common.Properties.Settings.Default.LicenseEditable &&
						message.fromUser != null)
					{
						msg.From = new MailAddress(message.fromUser.EmailAddress, message.fromUser.FullName);
					}

					if (!string.IsNullOrEmpty(ConfigurationSettings.Default.EmailSettings_EMailReplyTo))
					{
						msg.ReplyToList.Add(new MailAddress(ConfigurationSettings.Default.EmailSettings_EMailReplyTo));
					}

					//Send message.
					MailClient.SendMessage(msg);
				}
				catch (Exception ex)
				{
					//Error sending email. Log and don't re-throw.
					Logger.LogErrorEvent(METHOD, ex, "Preparing email for sending.");
				}
			}
		}
		#endregion

		#region User Subscriptions
		/// <summary>Returns a list of all the subscriptions that a user is signed up for.</summary>
		/// <param name="UserId">The UserId to pull for.</param>
		/// <param name="projectId">Only retrieve for a specific project, leave null for all projects</param>
		/// <param name="includeLookups">Should we retrieve the lookups (name of artifact, date, etc.)</param>
		public List<NotificationUserSubscriptionView> RetrieveSubscriptionsForUser(int userId, int? projectId)
		{
			const string METHOD_NAME = "RetrieveSubscriptionsForUser";

			try
			{
				List<NotificationUserSubscriptionView> subscriptions;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from n in context.NotificationUserSubscriptionsView
								where n.UserId == userId
								select n;

					//Filter by project if appropriate
					if (projectId.HasValue)
					{
						query = query.Where(n => n.ProjectId == projectId.Value);
					}


					//Sort
					query = query.OrderByDescending(n => n.LastUpdateDate).ThenBy(n => n.ArtifactTypeId).ThenBy(n => n.ArtifactId);

					//Execute
					subscriptions = query.ToList();
				}

				//Return the data
				return subscriptions;
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, ex);
				Logger.Flush();
				throw ex;
			}
		}

		/// <summary>Returns a list of all users subscribed for the given artifact.</summary>
		/// <param name="artifactTypeId">The artifact type ID.</param>
		/// <param name="artifactId">The artifact ID.</param>
		/// <returns>List of objects containing all users subscribed to this artifact.</returns>
		public List<NotificationUserSubscription> RetrieveSubscriptionsForArtifact(int artifactTypeId, int artifactId)
		{
			const string METHOD_NAME = "RetrieveSubscriptionsForArtifact";

			try
			{
				List<NotificationUserSubscription> subscriptions;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from n in context.NotificationUserSubscriptions
									.Include(n => n.User)
									.Include(n => n.User.Profile)
								where n.ArtifactTypeId == artifactTypeId && n.ArtifactId == artifactId && n.User.IsActive
								orderby n.UserId
								select n;

					//Execute
					subscriptions = query.ToList();
				}

				//Return the data
				return subscriptions;
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, ex);
				Logger.Flush();
				throw ex;
			}
		}

		/// <summary>Returns whether or not a user is subscribed for a specified ArtifactId.</summary>
		/// <param name="userId">The UserId to check for.</param>
		/// <param name="artifactTypeId">The artifact type.</param>
		/// <param name="artifactId">The artifact ID to check for.</param>
		/// <returns>True if the user is subscibed. False if not.</returns>
		public bool IsUserSubscribed(int userId, int artifactTypeId, int artifactId)
		{
			const string METHOD_NAME = "IsUserSubscribed";

			try
			{
				bool isUserSubscribed;
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from n in context.NotificationUserSubscriptions
								where n.ArtifactTypeId == artifactTypeId && n.ArtifactId == artifactId && n.UserId == userId
								select n;

					//Execute
					isUserSubscribed = query.Any();
				}

				//Return the data
				return isUserSubscribed;
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, ex);
				Logger.Flush();
				throw ex;
			}
		}

		/// <summary>Subscribe a user to an artifact.</summary>
		/// <param name="userId">The UserId to subscribe.</param>
		/// <param name="artifactTypeId">The ArtifactId of the artifact.</param>
		/// <param name="artifactId">The artifactId of the item.</param>
		public void AddUserSubscription(int userId, int artifactTypeId, int artifactId)
		{
			const string METHOD_NAME = "AddUserSubscription";
			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from n in context.NotificationUserSubscriptions
								where n.ArtifactTypeId == artifactTypeId && n.ArtifactId == artifactId && n.UserId == userId
								select n;

					//See if it already exists
					NotificationUserSubscription subscription = query.FirstOrDefault();
					if (subscription == null)
					{
						//Add new subscription
						subscription = new NotificationUserSubscription();
						subscription.UserId = userId;
						subscription.ArtifactId = artifactId;
						subscription.ArtifactTypeId = artifactTypeId;
						context.NotificationUserSubscriptions.AddObject(subscription);
						context.SaveChanges();
					}
				}

				Logger.LogExitingEvent(CLASS + METHOD_NAME);
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, ex);
				Logger.Flush();
				throw ex;
			}
		}

		/// <summary>Un-subscribe a user from an artifact.</summary>
		/// <param name="userId">The UserId to unsubscribe.</param>
		/// <param name="artifactTypeId">The ArtifactId of the artifact.</param>
		/// <param name="artifactId">The artifactId of the item.</param>
		public void RemoveUserSubscription(int userId, int artifactTypeId, int artifactId)
		{
			const string METHOD_NAME = "RemoveUserSubscription";
			Logger.LogEnteringEvent(CLASS + METHOD_NAME);

			try
			{
				using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
				{
					var query = from n in context.NotificationUserSubscriptions
								where n.ArtifactTypeId == artifactTypeId && n.ArtifactId == artifactId && n.UserId == userId
								select n;

					//See if it already exists
					NotificationUserSubscription subscription = query.FirstOrDefault();
					if (subscription != null)
					{
						//Remove the subscription
						context.NotificationUserSubscriptions.DeleteObject(subscription);
						context.SaveChanges();
					}
				}
				Logger.LogExitingEvent(CLASS + METHOD_NAME);
			}
			catch (Exception ex)
			{
				Logger.LogErrorEvent(CLASS + METHOD_NAME, ex);
				Logger.Flush();
				throw ex;
			}
		}

		/// <summary>Checks the gives Artifact ID for any attached subscriptions.</summary>
		private void GenerateSubscriptionList(Artifact art, ref List<UserEmailDetails> emailList)
		{
			const string METHOD = CLASS + "GenerateSubscriptionList";
			Logger.LogEnteringEvent(METHOD);

			//Get the ID field name..
			string artifactField = "";
			switch (art.ArtifactType)
			{
				case Artifact.ArtifactTypeEnum.Requirement:
					artifactField = "RequirementId";
					break;
				case Artifact.ArtifactTypeEnum.Document:
					artifactField = "AttachmentId";
					break;
				case Artifact.ArtifactTypeEnum.TestCase:
					artifactField = "TestCaseId";
					break;
				case Artifact.ArtifactTypeEnum.Incident:
					artifactField = "IncidentId";
					break;
				case Artifact.ArtifactTypeEnum.Task:
					artifactField = "TaskId";
					break;
				case Artifact.ArtifactTypeEnum.TestSet:
					artifactField = "TestSetId";
					break;
				case Artifact.ArtifactTypeEnum.Risk:
					artifactField = "RiskId";
					break;
				case Artifact.ArtifactTypeEnum.Release:
					artifactField = "ReleaseId";
					break;
				case Artifact.ArtifactTypeEnum.TestStep:
				case Artifact.ArtifactTypeEnum.TestRun:
				default:
					return;
			}

			//Pull subscribed users.
			List<NotificationUserSubscription> userSubscriptions = RetrieveSubscriptionsForArtifact((int)art.ArtifactType, (int)art[artifactField]);
			UserManager usrMgr = new UserManager();
			foreach (NotificationUserSubscription subRow in userSubscriptions)
			{
				//Pull user info.
				try
				{
					User userRec = usrMgr.GetUserById(subRow.UserId);

					if ((ConfigurationSettings.Default.EmailSettings_AllowUserControl && userRec.Profile.IsEmailEnabled) || !ConfigurationSettings.Default.EmailSettings_AllowUserControl)
					{
						//Add them to the collection.
						emailList.Add(new UserEmailDetails() { Name = userRec.FullName, ID = userRec.UserId, Email = userRec.EmailAddress });
					}
				}
				catch (Exception ex)
				{
					Logger.LogWarningEvent(METHOD, ex, "Trying to pull a subscribed user for artifact type " + (int)art.ArtifactType + ", id #" + (int)art[artifactField]);
				}
			}
		}
		#endregion
	}
}
