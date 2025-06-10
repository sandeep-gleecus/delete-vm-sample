using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;


namespace Inflectra.SpiraTest.Business
{
    /// <summary>
    /// Responsible for managing Component entities in the system
    /// </summary>
    public class ComponentManager : ManagerBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Business.ComponentManager::";

        public int Component_Insert(int projectId, string name, bool isActive = true)
        {
            const string METHOD_NAME = "Component_Insert";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                Component component;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Populate the new entity
                    component = new Component();
                    component.ProjectId = projectId;
                    component.Name = name;
                    component.IsActive = isActive;
                    component.IsDeleted = false;

                    //Persist the new entity
					context.Components.AddObject(component);
					context.SaveChanges();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return component.ComponentId;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Returns a list of component names from their ids
        /// </summary>
        /// <param name="multipleLegend">The localized legend for multiple values</param>
        /// <param name="componentIds">The list of ids</param>
        /// <param name="components">The list of components</param>
        public static void GetComponentNamesFromIds(List<int> componentIds, List<Component> components, string multipleLegend, out string name, out string tooltip)
        {
            name = "";
            tooltip = "";
            if (componentIds != null && components != null)
            {
                foreach (int componentId in componentIds)
                {
                    Component component = components.FirstOrDefault(c => c.ComponentId == componentId);
                    if (component != null)
                    {
                        if (tooltip == "")
                        {
                            tooltip = component.Name;
                        }
                        else
                        {
                            tooltip += ", " + component.Name;
                        }
                        if (name != "")
                        {
                            //We already have one, so set to multiple
                            name = "(" + multipleLegend + ")";
                        }
                        if (name == "")
                        {
                            name = component.Name;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates a single component in the system
        /// </summary>
        /// <param name="component">The component being updated</param>
        public void Component_Update(Component component)
        {
            const string METHOD_NAME = "Component_Update";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Apply the changes to the context and persist
                    context.Components.ApplyChanges(component);
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

        /// <summary>
        /// Deletes a single component in the system
        /// </summary>
        /// <param name="componentId">The id of the component being deleted</param>
        /// <remarks>Soft-deletes the component only. Fails quietly if the component cannot be found</remarks>
        public void Component_Delete(int componentId)
        {
            const string METHOD_NAME = "Component_Delete";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the component and mark as deleted
                    var query = from c in context.Components
                                where c.ComponentId == componentId
                                select c;

                    Component component = query.FirstOrDefault();
                    if (component != null)
                    {
                        //Mark as deleted and persist
                        component.StartTracking();
                        component.IsDeleted = true;
                        context.SaveChanges();
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

        /// <summary>
        /// Undeletes a single component in the system
        /// </summary>
        /// <param name="componentId">The id of the component being deleted</param>
        /// <remarks>Throws an exception if the component cannot be found</remarks>
        public void Component_Undelete(int componentId)
        {
            const string METHOD_NAME = "Component_Undelete";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Get the component and mark as deleted
                    var query = from c in context.Components
                                where c.ComponentId == componentId
                                select c;

                    Component component = query.FirstOrDefault();
                    if (component == null)
                    {
                        throw new ArtifactNotExistsException(String.Format(GlobalResources.Messages.ComponentManager_ArtifactNotExists, componentId));
                    }
                    else
                    {
                        //Mark as deleted and persist
                        component.StartTracking();
                        component.IsDeleted = false;
                        context.SaveChanges();
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
        
        /// <summary>
        /// Retrieves a single component by its ID
        /// </summary>
        /// <param name="componentId">The id of the component</param>
        /// <param name="includeDeleted">Should we return deleted components</param>
        /// <returns>The component if found, otherwise NULL is returned</returns>
        public Component Component_RetrieveById(int componentId, bool includeDeleted = false)
        {
            const string METHOD_NAME = "Component_RetrieveById";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                Component component;

                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from c in context.Components
                                where c.ComponentId == componentId && (!c.IsDeleted || includeDeleted)
                                select c;

                    component = query.FirstOrDefault();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return component;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Retrieves a list of components in the project
        /// </summary>
        /// <param name="projectId">The id of the project</param>
        /// <param name="activeOnly">Should we only return active ones</param>
        /// <param name="includeDeleted">Should we include deleted</param>
        /// <returns>List of components</returns>
        public List<Component> Component_Retrieve(int projectId, bool activeOnly = true, bool includeDeleted = false)
        {
            const string METHOD_NAME = "Component_Retrieve";

			Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                List<Component> components;

                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    var query = from c in context.Components
                                where (c.IsActive || !activeOnly) && (!c.IsDeleted || includeDeleted) && c.ProjectId == projectId
                                orderby c.Name, c.ComponentId
                                select c;

                    components = query.ToList();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return components;
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
