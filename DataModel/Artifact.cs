using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Inflectra.SpiraTest.DataModel
{
	/// <summary>The generic base class for all artifact entities</summary>
	[DataContract(IsReference = true)]
	public abstract class Artifact : Entity
	{
		#region Enumerations
		/// <summary>Enumeration of Yes/No Flag</summary>
		public enum FlagValues
		{
			No = 0,
			Yes = 1
		};

		/// <summary>Enumeration of the various artifact field types</summary>
		public enum ArtifactFieldTypeEnum
		{
			Text = 1,
			Lookup = 2,
			DateTime = 3,
			Identifier = 4,
			Equalizer = 5,
			NameDescription = 6,
			CustomPropertyLookup = 7,
			Integer = 8,
			TimeInterval = 9,
			Flag = 10,
			HierarchyLookup = 11,
			Html = 12,
			Decimal = 13,
			CustomPropertyMultiList = 14,
			CustomPropertyDate = 15,
			MultiList = 16
		}

		/// <summary>Enumeration of the different Artifact Types.</summary>
		public enum ArtifactTypeEnum : int
		{
			//Should match entries in TST_ARTIFACT_TYPE, with exception of NONE and Timecard.
			//TODO: #'s -501 through -13 are not in DB table. Should they be?
			EnterpriseHome = -501, /* neg as not artifact proper, 5 for the enum of the workspace type, 01 as home is the first page we made for enterprise */
			PortfolioHome = -401, /* neg as not artifact proper, 4 for the enum of the workspace type, 01 as homepage is the first page we made for portfolios */
			GroupPlanningBoard = -36,
			GroupIncidents = -35,
			GroupReleases = -34,
			//TestConfigurationSet = -33,
			PullRequest = -38,
			SourceCodeRevision = -28,
			Build = -27,
			MyTimecard = -24,
			SourceCodeFile = -13,
			Program = -12,
			TestRunStep = -4,
			User = -3,
			Message = -2,
			Project = -1,
			None = 0,
			Requirement = 1,
			TestCase = 2,
			Incident = 3,
			Release = 4,
			TestRun = 5,
			Task = 6,
			TestStep = 7,
			TestSet = 8,
			AutomationHost = 9,
			AutomationEngine = 10,
			Placeholder = 11,
			RequirementStep = 12,
			Document = 13,
			Risk = 14,
			RiskMitigation = 15,
			AllProjectHistoryList = 16,
			AllAuditTrail = 19,
			AllAdminAuditTrail = 20,
			Portfolios = 21,
			AllUserAuditTrail = 22,
			ProjectTemplate = 23,
			ProjectRole = 24,
			ProjectRolePermission = 25,
			LoginProvider = 26,
			FileTypeIcon = 27,
			SourceCode = 28,
			DataSync = 29,
			Report = 30,
			Graph = 31,
			ReportSectionInstance = 32,
			ReportCustomSection = 33,
			SystemUsageReport = 34,
			ProjectGroupUser = 35,
			ProjectTagFrequency = 36,
			DocumentDiscussion = 37,
			ProjectBaseline = 38,
			ArtifactLink = 39,
			ReleaseDiscussion = 40,
			TestCaseDiscussion = 41,
			TaskDiscussion = 42,
			RiskDiscussion = 43,
			RequirementDiscussion = 44,
			TestSetDiscussion = 45,
			IncidentResolution = 48,
			ProjectHistoryList = 49,
			AuditTrail = 50,
			AdminAuditTrail = 51,
			UserAuditTrail = 52,
			TestCaseParameter = 53,
			TestSetParameter = 54,
			TestConfigurationSet = 55,
			ReleaseTestCase = 56,
			Event = 57,
			DocumentVersion = 58,
			TestCaseSignature = 59,
			RequirementSignature = 60,
			ReleaseSignature = 61,
			DocumentSignature = 62, 
			IncidentSignature = 63,
			RiskSignature = 64,
			TaskSignature = 65,
			Help =100
		}

		/// <summary>Enumeration of the different Types of Artifact association panels.</summary>
		public enum DisplayTypeEnum : int
		{
			None = 0,
			ArtifactLink = 1,
			Requirement_TestCases = 2,
			TestStep_Requirements = 3,
			TestCase_Releases = 4,
			TestCase_Requirements = 5,
			Release_TestCases = 6,
			Attachments = 10,
			Requirement_Tasks = 11,
			TestCase_Runs = 12,
			TestSet_Runs = 13,
			TestCase_Incidents = 14,
			TestSet_Incidents = 15,
			TestRun_Incidents = 16,
			TestCase_TestSets = 17,
			TestSet_TestCases = 18,
			TestStep_Incidents = 19,
			Build_Incidents = 20,
			Build_Associations = 21,
			Build_Revisions = 26,
			Risk_Tasks = 22,
			SourceCodeRevision_Associations = 23,
			SourceCodeFile_Associations = 24,
			SourceCodeFile_Revisions = 25,
			PullRequest_Revisions = 27,
			Baseline_ArtifactChanges = 28
		}
		#endregion

		#region Properties
		/// <summary>Returns the type of the current artifact</summary>
		public abstract ArtifactTypeEnum ArtifactType
		{
			get;
		}

		/// <summary>Returns the two-character prefix of the current artifact</summary>
		public abstract string ArtifactPrefix
		{
			get;
		}

		/// <summary>The string of the artifact, like "IN-xxx".</summary>
		public abstract string ArtifactToken
		{
			get;
		}

		/// <summary>The primary key, which may have different field names</summary>
		public abstract int ArtifactId
		{
			get;
		}

		/// <summary>
		/// Used to store the meaning of any digital signatures for the most recent change
		/// </summary>
		public string SignatureMeaning
		{
			get;
			set;
		}

		#endregion
	}

	/// <summary>Contains static extension properties and methods that can't be in the main class</summary>
	public static class ArtifactHelper
	{
		/// <summary>
		/// Applies any changed fields, needed for notifications
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="artifact"></param>
		/// <param name="changes"></param>
		public static void ApplyChanges<T>(this T artifact, Dictionary<string, object> changes)
			where T : Artifact, IObjectWithChangeTracker, new()
		{
			Dictionary<string, object> newValues = new Dictionary<string, object>();

			//Loop through are reapply all the changes again so that notifications can pick it up
			artifact.StopTracking();
			foreach (KeyValuePair<string, object> change in changes)
			{
				if (artifact.ContainsProperty(change.Key))
				{
					object oldValue = change.Value;
					object newValue = artifact[change.Key];
					artifact[change.Key] = oldValue;
					newValues.Add(change.Key, newValue);
				}
			}

			//Now remake the changes
			artifact.StartTracking();
			foreach (KeyValuePair<string, object> newValue in newValues)
			{
				if (artifact.ContainsProperty(newValue.Key))
				{
					artifact[newValue.Key] = newValue.Value;
				}
			}
		}

		/// <summary>
		/// Extracts any changed fields, needed for notifications
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="artifact"></param>
		/// <returns></returns>
		public static Dictionary<string, object> ExtractChanges<T>(this T changeTrackingObject) where T : IObjectWithChangeTracker, new()
		{
			Dictionary<string, object> changes = new Dictionary<string, object>();
			ObjectChangeTracker objectChangeTracker = changeTrackingObject.ChangeTracker;
			return objectChangeTracker.OriginalValues;
		}

		/// <summary>Extension method that makes it easier to see if a entity property accepts NULLs or not</summary>
		/// <param name="property">The input property being tested</param>
		/// <returns>Whether it accepts nulls or not</returns>
		public static bool IsNullable(this PropertyInfo property)
		{
			//Get the type of the property
			Type propertyType = property.PropertyType;

			//See if it will accept nulls (either a non-primitive or Nullable<T>)
			return (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(System.Nullable<>) || !propertyType.IsPrimitive);
		}

		/// <summary>Generic cloning method for artifacts</summary>
		/// <typeparam name="T">The artifact type</typeparam>
		/// <param name="artifact">The artifact being cloned</param>
		/// <returns>The cloned copy</returns>
		public static T Clone<T>(this T artifact) where T : Artifact, new()
		{
			//Create a new instance of the class
			T clonedArtifact = new T();

			//Since it's a subclass of artifact, can set all the values using the indexer
			//Make sure it can read/write values and also has a defined typecode (or nullable typecode)
			//That way we don't set any navigation properties
			foreach (KeyValuePair<string, PropertyInfo> kvp in artifact.Properties)
			{
				PropertyInfo propInfo = kvp.Value;

				//See if it's a trackable collection.
				bool isTrackable = (propInfo.PropertyType.IsGenericType && propInfo.PropertyType.GetGenericTypeDefinition() == typeof(TrackableCollection<>));

				if (propInfo.CanRead && propInfo.CanWrite && !isTrackable && (Type.GetTypeCode(propInfo.PropertyType) != TypeCode.Object || Type.GetTypeCode(Nullable.GetUnderlyingType(propInfo.PropertyType)) != TypeCode.Object))
				{
					clonedArtifact[kvp.Key] = artifact[kvp.Key];
				}
			}

			return clonedArtifact;
		}

		/// <summary>Converts one artifact type to another. Usually used to convert View entities to Table entities (and vice-versa)</summary>
		/// <typeparam name="T">The artifact type</typeparam>
		/// <param name="artifact">The artifact being cloned</param>
		/// <returns>The cloned copy</returns>
		public static T2 ConvertTo<T1, T2>(this T1 artifact)
			where T1 : Artifact
			where T2 : Artifact, new()
		{
			//Create a new instance of the class
			T2 clonedArtifact = new T2();

			//Since it's a subclass of artifact, can set all the values using the indexer
			//Make sure it can read/write values and also has a defined typecode (or nullable typecode)
			//That way we don't set any navigation properties
			foreach (KeyValuePair<string, PropertyInfo> kvp in artifact.Properties)
			{
				PropertyInfo propInfo = kvp.Value;
				if (propInfo.CanRead && propInfo.GetIndexParameters().Length == 0 && clonedArtifact.Properties.ContainsKey(kvp.Key) && clonedArtifact.Properties[kvp.Key].CanWrite &&
					(Type.GetTypeCode(kvp.Value.PropertyType) != TypeCode.Object || Type.GetTypeCode(Nullable.GetUnderlyingType(kvp.Value.PropertyType)) != TypeCode.Object))
				{
					clonedArtifact[kvp.Key] = artifact[kvp.Key];
				}
			}

			return clonedArtifact;
		}
	}
}
