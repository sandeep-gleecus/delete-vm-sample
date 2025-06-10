using System;

namespace Inflectra.SpiraTest.Web.Attributes
{
    /// <summary>
    /// This attribute stores the level of administrator needed to access a particular page
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class AdministrationLevelAttribute : Attribute
    {
        #region Enumerations

        /// <summary>
        /// Stores the list of different administration levels
        /// </summary>
        [Flags]
        public enum AdministrationLevels
        {
            SystemAdministrator = 0x01,
            ProjectOwner = 0x02,
            GroupOwner = 0x04,
            ProjectTemplateAdmin = 0x08,
			ReportAdmin = 0x10
        }

        #endregion

        #region Properties

        /// <summary>
        /// Contains the current list of ORed administration levels that can access the page
        /// </summary>
        public AdministrationLevels AdministrationLevel
        {
            get
            {
                return this.administrationLevel;
            }
            set
            {
                this.administrationLevel = value;
            }
        }
        private AdministrationLevels administrationLevel;

        #endregion

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="administrationLevels">The ORed list of administration levels that can access this page</param>
        public AdministrationLevelAttribute(AdministrationLevels administrationLevel)
        {
            this.administrationLevel = administrationLevel;
        }
    }
}
