using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Business
{
    /// <summary>
    /// Performs system-level database maintenance
    /// </summary>
    public class SystemManager : ManagerBase
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Business.SystemManager::";

        protected const int SQL_COMMAND_TIMEOUT_REFRESH_INDEXES = 1200; //20 minutes

        /// <summary>
        /// Refreshes the databases indexes for the current system
        /// </summary>
        public void Database_RefreshIndexes()
        {
            const string METHOD_NAME = "Database_RefreshIndexes";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Called the stored proc to refresh the indexes
                    context.CommandTimeout = SQL_COMMAND_TIMEOUT_REFRESH_INDEXES;
                    context.System_RefreshIndexes();
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
        /// Checks to see if SQL Free Text Indexing is Installed
        /// </summary>
        public bool Database_CheckFreeTextIndexingInstalled()
        {
            const string METHOD_NAME = "Database_CheckFreeTextIndexingInstalled";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                bool isInstalled;
                using (SpiraTestEntitiesEx context = new SpiraTestEntitiesEx())
                {
                    //Called the stored proc to check the state of the feature
                    isInstalled = context.System_CheckFullTextIndexing().First().Value;
                }

                return isInstalled;
            }
            catch (System.Exception exception)
            {
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                Logger.Flush();
                throw;
            }
        }

        /// <summary>
        /// Deletes the sample data from the database
        /// </summary>
        /// <param name="userId">The user deleting the data</param>
        public void Database_DeleteSampleData(int userId)
        {
            const string METHOD_NAME = "Database_DeleteSampleData";

            Logger.LogEnteringEvent(CLASS_NAME + METHOD_NAME);

            try
            {
                bool success = true;    //Currently always true
                Logger.LogInformationalEvent(CLASS_NAME + METHOD_NAME, GlobalResources.Messages.System_StartingToClearSampleData);

                //First we need to delete the sample projects
                ProjectManager projectManager = new ProjectManager();
                for (int projectId = 1; projectId <= 7; projectId++)
                {
                    try
                    {
                        Project project = projectManager.RetrieveById(projectId);
                        //Make sure the project is a sample one
                        if (
                            (projectId == 1 && project.Name == "Library Information System (Sample)") ||
                            (projectId == 2 && project.Name == "Sample Empty Product 1") ||
                            (projectId == 3 && project.Name == "Sample Empty Product 2") ||
                            (projectId == 4 && project.Name == "ERP: Financials") ||
                            (projectId == 5 && project.Name == "ERP: Human Resources") ||
                            (projectId == 6 && project.Name == "Company Website") ||
                            (projectId == 7 && project.Name == "Customer Relationship Management (CRM)")
                            )
                        {
                            projectManager.Delete(userId, projectId);
                        }
                    }
                    catch (Exception)
                    {
                        Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, String.Format("Unable to delete sample project PR{0}", projectId));
                    }
                }

                //Next we delete the sample programs
                ProjectGroupManager projectGroupManager = new ProjectGroupManager();
                for (int projectGroupId = 2; projectGroupId <= 4; projectGroupId++)
                {
                    try
                    {
                        ProjectGroup projectGroup = projectGroupManager.RetrieveById(projectGroupId);
                        //Make sure the project is a sample one
                        if (
                            (projectGroupId == 2 && projectGroup.Name == "Sample Program") ||
                            (projectGroupId == 3 && projectGroup.Name == "Corporate Systems") ||
                            (projectGroupId == 4 && projectGroup.Name == "Sales and Marketing")
                            )
                        {
                            projectGroupManager.Delete(projectGroupId);
                        }
                    }
                    catch (Exception)
                    {
                        Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, String.Format("Unable to delete sample program PG{0}", projectGroupId));
                    }
                }

                //Next we delete the sample portfolios
                PortfolioManager portfolioManager = new PortfolioManager();
                for (int portfolioId = 1; portfolioId <= 1; portfolioId++)
                {
                    try
                    {
                        Portfolio portfolio = portfolioManager.Portfolio_RetrieveById(portfolioId);
                        if (portfolio != null)
                        {
                            //Make sure the project is a sample one
                            if (
                                (portfolioId == 1 && portfolio.Name == "Core Services")
                                )
                            {
                                portfolioManager.Portfolio_Delete(portfolioId);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, String.Format("Unable to delete sample project PR{0}", portfolioId));
                    }
                }

                //Finally we delete the sample users
                UserManager userManager = new UserManager();
                for(int userIdToDelete = 2; userIdToDelete <= 13; userIdToDelete++)
                {
                    try
                    {
                        User user = userManager.GetUserById(userIdToDelete);
                        //Make sure the user is a sample one
                        if (user.EmailAddress.Contains("@mycompany.com"))
                        {
                            userManager.DeleteUser(userIdToDelete, true);
                        }
                    }
                    catch (Exception)
                    {
                        Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, String.Format("Unable to delete sample user {0}", userIdToDelete));
                    }
                }

                //Finally update the status of the sample data
                if (success)
                {
                    ConfigurationSettings.Default.Database_SampleDataCanBeDeleted = false;
                    ConfigurationSettings.Default.Save();

                    //Cleared sample data
                    Logger.LogInformationalEvent(CLASS_NAME + METHOD_NAME, GlobalResources.Messages.System_FinishingToClearSampleData);
                }
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
