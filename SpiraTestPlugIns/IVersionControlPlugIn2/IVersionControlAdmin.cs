using System.Collections.Generic;

namespace Inflectra.SpiraTest.PlugIns.VersionControlPlugIn2
{
	/// <summary>Interface for hooking to ProjectLocker</summary>
	public interface IVersionControlAdmin
	{
		#region Account Calls
		/// <summary>Creates an account with the specified name.</summary>
		/// <param name="accountName">The name of the account.</param>
		/// <returns>A full populated Account.</returns>
		Account AccountCreate(string accountName);

		/// <summary>Retreves information for the given account.</summary>
		/// <param name="accountId">The account ID to pull.</param>
		/// <returns>A populated account.</returns>
		Account AccountRetrieve(long accountId);

		/// <summary>Deletes the specified account.</summary>
		/// <param name="accountId">The account ID to return.</param>
		/// <returns>A populated account, or null if the account was deleted completely.</returns>
		Account AccountDelete(long accountId);

		/// <summary>Marks the specified account inactive.</summary>
		/// <param name="accountId">The account ID to mark.</param>
		/// <returns>An updated account object.</returns>
		Account AccountMarkInactive(long accountId);

		/// <summary>Updates the specified account with the new name.</summary>
		/// <param name="accountId">The account ID to update.</param>
		/// <param name="newName">The new name of the account.</param>
		/// <returns>The populated account with the new information.</returns>
		Account AccountUpdate(long accountId, string newName);
		#endregion //Account Calls

		#region Project Calls
		/// <summary>Creates a project with the given name and repository type for the account ID.</summary>
		/// <param name="accountId">The ID to create the repository for.</param>
		/// <param name="projectName">The name of rhe repository/project.</param>
		/// <param name="repositoryType">The type (git, svn) of the repository.</param>
		/// <returns>The project that was created.</returns>
		Project ProjectCreate(long accountId, string projectName, Project.RepositoryTypeEnum repositoryType);

		/// <summary>Retrieves the project information for the given account ID and project ID.</summary>
		/// <param name="accountId">The account ID that the project should belong to.</param>
		/// <param name="projectId">The unique Project Id.</param>
		/// <returns>A populated Project object.</returns>
		Project ProjectRetrieve(long accountId, long projectId);

		/// <summary>Deletes a project.</summary>
		/// <param name="accountId">The accountId containing the project.</param>
		/// <param name="projectId">The projectId to delete.</param>
		/// <returns>A populated Project object.</returns>
		Project ProjectDelete(long accountId, long projectId);

		/// <summary>Marks a project inactive.</summary>
		/// <param name="accountId">The accountId containing the project.</param>
		/// <param name="projectId">The projectId to mark inactive.</param>
		/// <returns>A populated Project object.</returns>
		Project ProjectMarkInactive(long accountId, long projectId);

		/// <summary>Updates the project with a new name. Note that the type cannot be changed after being created.</summary>
		/// <param name="accountId">The accountId that contains the project.</param>
		/// <param name="projectId">The projectId of the project to update.</param>
		/// <param name="newName">The new name of the project.</param>
		/// <returns>An updated Project object.</returns>
		Project ProjectUpdate(long accountId, long projectId, string newName);
		#endregion //Project Calls

		#region User Calls
		/// <summary>Creates a new user with the information given.</summary>
		/// <param name="newuser">The new User to create.</param>
		/// <returns>A populated User object.</returns>
		User UserCreate(long accountId, User newUser, string password);

		/// <summary>tretrieves the specified user's information.</summary>
		/// <param name="accountId">The accountId that the user belongs to.</param>
		/// <param name="userId">The userId of the details to retrieve.</param>
		/// <returns>A populate user object.</returns>
		User UserRetrieve(long accountId, long userId);

		/// <summary>Marks a user inactive, or deletes a user (if already inactive).</summary>
		/// <param name="accountId">The accountId containing the user.</param>
		/// <param name="userId">The userId to delete.</param>
		/// <returns>A populated User  object.</returns>
		User UserDelete(long accountId, long userId);

		/// <summary>Marks a user inactive.</summary>
		/// <param name="accountId">The accountId containing the user.</param>
		/// <param name="userId">The userId to mark inactive.</param>
		/// <returns>Updated User object.</returns>
		User UserMarkInactive(long accountId, long userId);

		/// <summary>Updates the user's speficied information.</summary>
		/// <param name="updateUser">The user with updated information.</param>
		/// <returns>A populated User object.</returns>
		/// <remarks>Leave fields blank if they are not getting changed.</remarks>
		User UserUpdate(long accountId, User updateUser, string password);
		#endregion //User Calls

		#region Project User Calls
		/// <summary>Adds the specified user ID to the given project.</summary>
		/// <param name="accountId">The account that contains both the Project and User.</param>
		/// <param name="projectId">The projectId to attach the user to.</param>
		/// <param name="userId">The user to attach to the project.</param>
		/// <returns>A populated User object.</returns>
		User ProjectAddUser(long accountId, long projectId, long userId);

		/// <summary>Removes a given user from the given project.</summary>
		/// <param name="accountId">The account that contains both the Project and User.</param>
		/// <param name="projectId">The projectId to delete the user from.</param>
		/// <param name="userId">The user to remove from the project.</param>
		/// <returns>A populatd User object.</returns>
		User ProjectRemoveUser(long accountId, long projectId, long userId);

		/// <summary>Retrieves a list of all uasers assigned to this project.</summary>
		/// <param name="accountId">The accountId that contains the project.</param>
		/// <param name="projectId">The project to get the list of users from.</param>
		/// <returns>A list of Users assigned to this project.</returns>
		List<User> ProjectRetrieveUsers(long accountId, long projectId);
		#endregion //Project User Calls

	}

	/// <summary>A root account for a Spira install.</summary>
	public class Account
	{
		/// <summary>The newly created account's ID number.</summary>
		public long id { get; set; }
		/// <summary>The account's active status.</summary>
		public bool active { get; set; }
		/// <summary>The server the account is created on?</summary>
		public string server { get; set; }
		/// <summary>The name of the account.</summary>
		public string name { get; set; }
	}

	/// <summary>A project that is contained within an account.</summary>
	public class Project
	{
		/// <summary>The project unique ID.</summary>
		public long id { get; set; }

		/// <summary>The name of the account.</summary>
		public string name { get; set; }

		/// <summary>Sets the repository type.</summary>
		public string repository_type
		{
			set
			{
				if (value.ToLowerInvariant().Trim() == "git")
					this.repositoryType = RepositoryTypeEnum.GIT;
				else
					this.repositoryType = RepositoryTypeEnum.SVN;
			}
			get
			{
				return ((repositoryType == Project.RepositoryTypeEnum.GIT) ? "GIT" : "SVN");
			}
		}

		/// <summary>The repository type of the project.</summary>
		private RepositoryTypeEnum repositoryType { get; set; }

		/// <summary>The account ID that this project is linked to.</summary>
		public long account_id { get; set; }

		/// <summary>The account name that this project is linked to.</summary>
		public string account_name { get; set; }

		/// <summary>Whether the project is active or not.</summary>
		public bool active { get; set; }

		/// <summary>The types of repositories supported.</summary>
		public enum RepositoryTypeEnum
		{
			GIT = 2,
			SVN = 1
		}
	}

	/// <summary>A user that is contained within an account, and can belong to a project.</summary>
	public class User
	{
		/// <summary>The project unique ID.</summary>
		public long? id { get; set; }
		/// <summary>The name of the account.</summary>
		public string email { get; set; }
		/// <summary>User's first name.</summary>
		public string firstname { get; set; }
		/// <summary>User's first name.</summary>
		public string lastname { get; set; }
		/// <summary>User's Jabber (IM) id. NOT USED.</summary>
		public string jabber_id { get; set; }
		/// <summary>User's twitter id. NOT USED.</summary>
		public string twitter_id { get; set; }
		/// <summary>The user's login string.</summary>
		public string login { get; set; }
		/// <summary>The user's password. Only set on create.</summary>
		public string password { get; set; }
		/// <summary>Whether the user is an Admin or not. NOT USED.</summary>
		public bool? is_admin { get; set; }
		/// <summary>Whether the user's account is active or not.</summary>
		public bool? active { get; set; }
	}
}
