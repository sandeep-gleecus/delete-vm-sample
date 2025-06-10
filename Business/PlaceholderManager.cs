using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Business
{
    /// <summary>
    /// Manages placeholder artifacts that are used to allow attachments to be saved before the main artifact is saved
    /// </summary>
    public class PlaceholderManager : ManagerBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Business.PlaceholderManager::";

        /// <summary>Creates a new instance of the class.</summary>
        public PlaceholderManager()
        {
            const string METHOD_NAME = ".ctor()";
            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);
            Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
        }

        /// <summary>
        /// Retrieves a placeholder by its id
        /// </summary>
        /// <param name="placeholderId">The id of the placeholder</param>
        /// <returns>The placeholder object</returns>
        public Placeholder Placeholder_RetrieveById(int placeholderId)
        {
            const string METHOD_NAME = "Placeholder_RetrieveById";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                Placeholder placeholder;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Query for the placeholder
                    var query = from p in context.Placeholders
                                where p.PlaceholderId == placeholderId
                                select p;

                    placeholder = query.FirstOrDefault();
                    if (placeholder == null)
                    {
                        throw new ArtifactNotExistsException("Placeholder " + placeholderId + " does not exist in the system");
                    }
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return placeholder;
            }
            catch (ArtifactNotExistsException)
            {
                //Do not log
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
        /// Creates a new placeholder artifact in the system
        /// </summary>
        /// <param name="projectId">The id of the project that the placeholder is being created in</param>
        /// <returns>The newly created placeholder</returns>
        public Placeholder Placeholder_Create(int projectId)
        {
            const string METHOD_NAME = "Placeholder_Create";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                Placeholder placeholder = new Placeholder();
                placeholder.ProjectId = projectId;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Persist the new placeholder
                    context.Placeholders.AddObject(placeholder);
                    context.SaveChanges();
                }

                Logger.LogExitingEvent(CLASS_NAME + METHOD_NAME);
                return placeholder;
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
