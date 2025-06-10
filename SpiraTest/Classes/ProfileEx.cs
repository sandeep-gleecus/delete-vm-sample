using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Profile;

namespace Inflectra.SpiraTest.Web.Classes
{
    /// <summary>
    /// Extends ProfileBase to include strongly typed attributes for commonly-access properties
    /// </summary>
    public class ProfileEx
    {
        protected ProfileBase profileBase;

        /// <summary>
        /// Constructor that creates it for the current user
        /// </summary>
        public ProfileEx()
        {
            this.profileBase = HttpContext.Current.Profile;

            //If we have no profile, create a dummy one
            if (this.profileBase != null && HttpContext.Current.User.Identity.IsAuthenticated && (String.IsNullOrEmpty(this.FirstName) || String.IsNullOrEmpty(this.LastName)))
            {
                this.FirstName = HttpContext.Current.User.Identity.Name;
                this.LastName = HttpContext.Current.User.Identity.Name;
                this.profileBase.Save();
            }
        }

        /// <summary>
        /// Constructor that creates a new profile for a specific user or retrieves it for an existing one
        /// </summary>
        /// <param name="username">The name of the user</param>
        public ProfileEx(string username)
        {
            this.profileBase = ProfileBase.Create(username);
        }

        /// <summary>
        /// Constructor that creates it for a specific user
        /// </summary>
        /// <param name="profileBase">Profile base class for the specific user</param>
        public ProfileEx(ProfileBase profileBase)
        {
            this.profileBase = profileBase;
        }

        /// <summary>
        /// Saves changes to the profile
        /// </summary>
        public void Save()
        {
            this.profileBase.Save();
        }

        /// <summary>
        /// Returns a handle to the base
        /// </summary>
        public ProfileBase Default
        {
            get
            {
                return this.profileBase;
            }
        }

        /// <summary>
        /// Returns the full name of the user
        /// </summary>
        public string FullName
        {
            get
            {
                if (String.IsNullOrWhiteSpace(MiddleInitial))
                {
                    return FirstName + " " + LastName;
                }
                else
                {
                    return FirstName + " " + MiddleInitial + " " + LastName;
                }
            }
        }

        /// <summary>
        /// The first name of the user
        /// </summary>
        public string FirstName
        {
            get
            {
                return (string)profileBase["FirstName"];
            }
            set
            {
                profileBase["FirstName"] = value;
            }
        }

        /// <summary>
        /// The last name of the user
        /// </summary>
        public string LastName
        {
            get
            {
                return (string)profileBase["LastName"];
            }
            set
            {
                profileBase["LastName"] = value;
            }
        }

        /// <summary>
        /// The middle initial of the user
        /// </summary>
        public string MiddleInitial
        {
            get
            {
                return (string)profileBase["MiddleInitial"];
            }
            set
            {
                profileBase["MiddleInitial"] = value;
            }
        }

        /// <summary>
        /// The Department for the user
        /// </summary>
        public string Department
        {
            get
            {
                return (string)profileBase["Department"];
            }
            set
            {
                profileBase["Department"] = value;
            }
        }

        /// <summary>
        /// The Organization of the user
        /// </summary>
        public string Organization
        {
            get
            {
                return (string)profileBase["Organization"];
            }
            set
            {
                profileBase["Organization"] = value;
            }
        }

        /// <summary>
        /// The Timezone for the user
        /// </summary>
        public string Timezone
        {
            get
            {
                return (string)profileBase["Timezone"];
            }
            set
            {
                profileBase["Timezone"] = value;
            }
        }

        /// <summary>
        /// The avatar image for the user
        /// </summary>
        public string AvatarImage
        {
            get
            {
                return (string)profileBase["AvatarImage"];
            }
            set
            {
                profileBase["AvatarImage"] = value;
            }
        }

        /// <summary>
        /// The avatar mime type for the user
        /// </summary>
        public string AvatarMimeType
        {
            get
            {
                return (string)profileBase["AvatarMimeType"];
            }
            set
            {
                profileBase["AvatarMimeType"] = value;
            }
        }

        /// <summary>
        /// The number of unread instant messages
        /// </summary>
        public int UnreadMessages
        {
            get
            {
                return (int)profileBase["UnreadMessages"];
            }
            set
            {
                profileBase["UnreadMessages"] = value;
            }
        }

        /// <summary>
        /// Is the user a system administrator
        /// </summary>
        public bool IsAdmin
        {
            get
            {
                return (bool)profileBase["IsAdmin"];
            }
            set
            {
                profileBase["IsAdmin"] = value;
            }
        }

        /// <summary>
        /// Is the user marked as 'busy' to instant messenger users (not used currently)
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return (bool)profileBase["IsBusy"];
            }
            set
            {
                profileBase["IsBusy"] = value;
            }
        }

        /// <summary>
        /// Is the user marked as 'away' to instant messenger users (not used currently)
        /// </summary>
        public bool IsAway
        {
            get
            {
                return (bool)profileBase["IsAway"];
            }
            set
            {
                profileBase["IsAway"] = value;
            }
        }

        /// <summary>
        /// Does the user have email enabled
        /// </summary>
        public bool IsEmailEnabled
        {
            get
            {
                return (bool)profileBase["IsEmailEnabled"];
            }
            set
            {
                profileBase["IsEmailEnabled"] = value;
            }
        }

        /// <summary>
        /// Is the user a portfolio admin (viewer)
        /// </summary>
        public bool IsPortfolioAdmin
        {
            get
            {
                return (bool)profileBase["IsPortfolioAdmin"];
            }
            set
            {
                profileBase["IsPortfolioAdmin"] = value;
            }
        }

		/// <summary>
		/// Is the user a custom report admin
		/// </summary>
		public bool IsReportAdmin
		{
			get
			{
				return (bool)profileBase["IsReportAdmin"];
			}
			set
			{
				profileBase["IsReportAdmin"] = value;
			}
		}

		/// <summary>
		/// Is the user a resource administrator
		/// </summary>
		public bool IsResourceAdmin
        {
            get
            {
                return (bool)profileBase["IsResourceAdmin"];
            }
            set
            {
                profileBase["IsResourceAdmin"] = value;
            }
        }

        /// <summary>
        /// Is the user restricted
        /// </summary>
        public bool IsRestricted
        {
            get
            {
                return (bool)profileBase["IsRestricted"];
            }
            set
            {
                profileBase["IsRestricted"] = value;
            }
        }

        /// <summary>
        /// What was the id of the project that the user last opened
        /// </summary>
        public int? LastOpenedProjectId
        {
            get
            {
                return (int?)profileBase["LastOpenedProjectId"];
            }
            set
            {
                profileBase["LastOpenedProjectId"] = value;
            }
        }

        /// <summary>
        /// What was the id of the program that the user last opened
        /// </summary>
        public int? LastOpenedProjectGroupId
        {
            get
            {
                return (int?)profileBase["LastOpenedProjectGroupId"];
            }
            set
            {
                profileBase["LastOpenedProjectGroupId"] = value;
            }
        }

        /// <summary>
        /// What was the id of the project template that the user last opened
        /// </summary>
        public int? LastOpenedProjectTemplateId
        {
            get
            {
                return (int?)profileBase["LastOpenedProjectTemplateId"];
            }
            set
            {
                profileBase["LastOpenedProjectTemplateId"] = value;
            }
        }

        /// <summary>
        /// What was the id of the portfolio that the user last opened
        /// </summary>
        public int? LastOpenedPortfolioId
        {
            get
            {
                return (int?)profileBase["LastOpenedPortfolioId"];
            }
            set
            {
                profileBase["LastOpenedPortfolioId"] = value;
            }
        }

        /// <summary>
        /// Used for accessing the other, non-explicitly named profile properties
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public object this[string propertyName]
        {
            get
            {
                return profileBase[propertyName];
            }
            set
            {
                profileBase[propertyName] = value;
            }
        }
    }
}
