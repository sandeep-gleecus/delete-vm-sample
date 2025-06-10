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
	public partial class UserActivityLogResponse : IObjectWithChangeTracker, INotifyPropertyChanged
	{
		#region Primitive Properties

		[DataMember]
		public long UserActivityLogId
		{
			get { return _userActivityLogId; }
			set
			{
				if (_userActivityLogId != value)
				{
					if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
					{
						throw new InvalidOperationException("The property 'ChaUserActivityLogIdngeSetId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
					}
					_userActivityLogId = value;
					OnPropertyChanged("UserActivityLogId");
				}
			}
		}
		private long _userActivityLogId;

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
		public Nullable<System.DateTime> LoginDate
		{
			get { return _loginDate; }
			set
			{
				if (_loginDate != value)
				{
					//ChangeTracker.RecordOriginalValue("LastLoginDate", _lastLoginDate);
					_loginDate = value;
					OnPropertyChanged("LoginDate");
				}
			}
		}
		private Nullable<System.DateTime> _loginDate;

		[DataMember]
		public Nullable<System.DateTime> LogoutDate
		{
			get { return _logoutDate; }
			set
			{
				if (_logoutDate != value)
				{
					//ChangeTracker.RecordOriginalValue("LastLoginDate", _lastLogoutDate);
					_logoutDate = value;
					OnPropertyChanged("LogoutDate");
				}
			}
		}
		private Nullable<System.DateTime> _logoutDate;

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
		public string ActivityDescription
		{
			get { return _activityDescription; }
			set
			{
				if (_activityDescription != value)
				{
					//ChangeTracker.RecordOriginalValue("NewValue", _newValue);
					_activityDescription = value;
					OnPropertyChanged("ActivityDescription");
				}
			}
		}
		private string _activityDescription;

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
