using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Common
{
	[KnownType(typeof(Project))]
	[KnownType(typeof(User))]
	[KnownType(typeof(TestCase))]
	[KnownType(typeof(RequirementUser))]
	[KnownType(typeof(RequirementStatus))]
	[KnownType(typeof(RequirementStep))]
	[KnownType(typeof(RequirementType))]
	[KnownType(typeof(Importance))]
	[KnownType(typeof(Task))]
	[KnownType(typeof(RequirementDiscussion))]
	[KnownType(typeof(Release))]
	[KnownType(typeof(TestStep))]
	[KnownType(typeof(ProjectGoal))]
	[KnownType(typeof(ProjectGroupTheme))]
	public class PlanningBoardReleaseResponse
	{
		#region Primitive Properties

		[DataMember]
		public int RequirementId
		{
			get { return _requirementId; }
			set
			{
				if (_requirementId != value)
				{
					_requirementId = value;
				}
			}
		}
		private int _requirementId;

		[DataMember]
		public int AuthorId
		{
			get { return _authorId; }
			set
			{
				if (_authorId != value)
				{
					_authorId = value;
				}
			}
		}
		private int _authorId;

		[DataMember]
		public int? OwnerId
		{
			get { return _ownerId; }
			set
			{
				if (_ownerId != value)
				{
					_ownerId = value;
				}
			}
		}
		private int? _ownerId;

		[DataMember]
		public string AuthorName
		{
			get { return _authorName; }
			set
			{
				if (_authorName != value)
				{
					_authorName = value;
				}
			}
		}
		private string _authorName;

		[DataMember]
		public string Type
		{
			get { return _type; }
			set
			{
				if (_type != value)
				{
					_type = value;
				}
			}
		}
		private string _type;

		[DataMember]
		public Nullable<int> ReleaseId
		{
			get { return _releaseId; }
			set
			{
				if (_releaseId != value)
				{
					_releaseId = value;
				}
			}
		}
		private Nullable<int> _releaseId;

		[DataMember]
		public int ProjectId
		{
			get { return _projectId; }
			set
			{
				if (_projectId != value)
				{
					_projectId = value;
				}
			}
		}
		private int _projectId;

		[DataMember]
		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
			}
		}
		private string _name;

		[DataMember]
		public string Description
		{
			get { return _description; }
			set
			{
					_description = value;
			}
		}
		private string _description;

		[DataMember]
		public Nullable<int> Rank
		{
			get { return _rank; }
			set
			{
				_rank = value;
			}
		}
		private Nullable<int> _rank;

		#endregion

	}
}
