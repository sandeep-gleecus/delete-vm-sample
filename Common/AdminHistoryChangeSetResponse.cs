using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Common
{
	public partial class AdminHistoryChangeSetResponse : IObjectWithChangeTracker, INotifyPropertyChanged
	{
		#region Primitive Properties

		[DataMember]
		public long ChangeSetId
		{
			get { return _changeSetId; }
			set
			{
				if (_changeSetId != value)
				{
					if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
					{
						throw new InvalidOperationException("The property 'ChangeSetId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
					}
					_changeSetId = value;
					OnPropertyChanged("ChangeSetId");
				}
			}
		}
		private long _changeSetId;

		[DataMember]
		public int UserId
		{
			get { return _userId; }
			set
			{
				if (_userId != value)
				{
					if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
					{
						throw new InvalidOperationException("The property 'UserId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
					}
					_userId = value;
					OnPropertyChanged("UserId");
				}
			}
		}
		private int _userId;

		[DataMember]
		public System.DateTime ChangeDate
		{
			get { return _changeDate; }
			set
			{
				if (_changeDate != value)
				{
					if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
					{
						throw new InvalidOperationException("The property 'ChangeDate' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
					}
					_changeDate = value;
					OnPropertyChanged("ChangeDate");
				}
			}
		}
		private System.DateTime _changeDate;

		[DataMember]
		public int ChangeTypeId
		{
			get { return _changeTypeId; }
			set
			{
				if (_changeTypeId != value)
				{
					if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
					{
						throw new InvalidOperationException("The property 'ChangeTypeId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
					}
					_changeTypeId = value;
					OnPropertyChanged("ChangeTypeId");
				}
			}
		}
		private int _changeTypeId;

		[DataMember]
		public string ChangeTypeName
		{
			get { return _changeTypeName; }
			set
			{
				if (_changeTypeName != value)
				{
					if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
					{
						throw new InvalidOperationException("The property 'ChangeTypeName' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
					}
					_changeTypeName = value;
					OnPropertyChanged("ChangeTypeName");
				}
			}
		}
		private string _changeTypeName;

		[DataMember]
		public string UserName
		{
			get { return _userName; }
			set
			{
				if (_userName != value)
				{
					//ChangeTracker.RecordOriginalValue("UserName", _userName);
					_userName = value;
					OnPropertyChanged("UserName");
				}
			}
		}
		private string _userName;

		[DataMember]
		public string Time
		{
			get { return _time; }
			set
			{
				if (_time != value)
				{
					//ChangeTracker.RecordOriginalValue("OldValue", _oldValue);
					_time = value;
					OnPropertyChanged("Time");
				}
			}
		}
		private string _time;

		[DataMember]
		public string TimeZone
		{
			get { return _timeZone; }
			set
			{
				if (_timeZone != value)
				{
					//ChangeTracker.RecordOriginalValue("OldValue", _oldValue);
					_timeZone = "UTC";
					OnPropertyChanged("TimeZone");
				}
			}
		}
		private string _timeZone;

		[DataMember]
		public string OldValue
		{
			get { return _oldValue; }
			set
			{
				if (_oldValue != value)
				{
					//ChangeTracker.RecordOriginalValue("OldValue", _oldValue);
					_oldValue = value;
					OnPropertyChanged("OldValue");
				}
			}
		}
		private string _oldValue;

		[DataMember]
		public string NewValue
		{
			get { return _newValue; }
			set
			{
				if (_newValue != value)
				{
					//ChangeTracker.RecordOriginalValue("NewValue", _newValue);
					_newValue = value;
					OnPropertyChanged("NewValue");
				}
			}
		}
		private string _newValue;

		[DataMember]
		public string FieldName
		{
			get { return _fieldName; }
			set
			{
				if (_fieldName != value)
				{
					//ChangeTracker.RecordOriginalValue("NewValue", _newValue);
					_fieldName = value;
					OnPropertyChanged("FieldName");
				}
			}
		}
		private string _fieldName;


		[DataMember]
		public Nullable<int> ARTIFACT_ID
		{
			get { return _aRTIFACT_ID; }
			set
			{
				if (_aRTIFACT_ID != value)
				{
					
					_aRTIFACT_ID = value;
					OnPropertyChanged("ARTIFACT_ID");
				}
			}
		}
		private Nullable<int> _aRTIFACT_ID;
		[DataMember]
		
		public Nullable<System.Guid> ARTIFACT_GUID_ID
		{
			get { return _aRTIFACT_GUID_ID; }
			set
			{
				if (_aRTIFACT_GUID_ID != value)
				{
					_aRTIFACT_GUID_ID = value;
					OnPropertyChanged("ARTIFACT_GUID_ID");
				}
			}
		}
		private Nullable<System.Guid> _aRTIFACT_GUID_ID;

		[DataMember]
		public string FieldCaption
		{
			get { return _fieldCaption; }
			set
			{
				if (_fieldCaption != value)
				{
					_fieldCaption = value;
					OnPropertyChanged("FieldCaption");
				}
			}
		}
		private string _fieldCaption;

		[DataMember]
		public int? FieldId
		{
			get { return _fieldId; }
			set
			{
				if (_fieldId != value)
				{
					_fieldId = value;
					OnPropertyChanged("FieldId");
				}
			}
		}
		private int? _fieldId;

		[DataMember]
		public string ActionDescription
		{
			get { return _actionDescription; }
			set
			{
				if (_actionDescription != value)
				{
					_actionDescription = value;
					OnPropertyChanged("ActionDescription");
				}
			}
		}
		private string _actionDescription;

		[DataMember]
		public string AdminSectionName
		{
			get { return _adminSectionName; }
			set
			{
				if (_adminSectionName != value)
				{
					if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
					{
						throw new InvalidOperationException("The property 'AdminSectionName' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
					}
					_adminSectionName = value;
					OnPropertyChanged("AdminSectionName");
				}
			}
		}
		private string _adminSectionName;

		#endregion

		#region ChangeTracking

		protected virtual void OnPropertyChanged(String propertyName)
		{
			if (ChangeTracker.State != ObjectState.Added && ChangeTracker.State != ObjectState.Deleted)
			{
				ChangeTracker.State = ObjectState.Modified;
			}
			if (_propertyChanged != null)
			{
				_propertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		protected virtual void OnNavigationPropertyChanged(String propertyName)
		{
			if (_propertyChanged != null)
			{
				_propertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged { add { _propertyChanged += value; } remove { _propertyChanged -= value; } }
		private event PropertyChangedEventHandler _propertyChanged;
		private ObjectChangeTracker _changeTracker;

		[DataMember]
		public ObjectChangeTracker ChangeTracker
		{
			get
			{
				if (_changeTracker == null)
				{
					_changeTracker = new ObjectChangeTracker();
					_changeTracker.ObjectStateChanging += HandleObjectStateChanging;
				}
				return _changeTracker;
			}
			set
			{
				if (_changeTracker != null)
				{
					_changeTracker.ObjectStateChanging -= HandleObjectStateChanging;
				}
				_changeTracker = value;
				if (_changeTracker != null)
				{
					_changeTracker.ObjectStateChanging += HandleObjectStateChanging;
				}
			}
		}

		private void HandleObjectStateChanging(object sender, ObjectStateChangingEventArgs e)
		{
			if (e.NewState == ObjectState.Deleted)
			{
				ClearNavigationProperties();
			}
		}

		protected bool IsDeserializing { get; private set; }

		[OnDeserializing]
		public void OnDeserializingMethod(StreamingContext context)
		{
			IsDeserializing = true;
		}

		[OnDeserialized]
		public void OnDeserializedMethod(StreamingContext context)
		{
			IsDeserializing = false;
			ChangeTracker.ChangeTrackingEnabled = true;
		}

		protected virtual void ClearNavigationProperties()
		{
		}


		#endregion
	}
}
