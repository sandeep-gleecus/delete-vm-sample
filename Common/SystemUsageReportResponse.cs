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
	public partial class SystemUsageReportResponse : IObjectWithChangeTracker, INotifyPropertyChanged
	{
		#region Primitive Properties

		[DataMember]
		public string MonthName
		{
			get { return _monthName; }
			set
			{
				if (_monthName != value)
				{
					_monthName = value;
					OnPropertyChanged("MonthName");
				}
			}
		}
		private string _monthName;

		[DataMember]
		public string ActiveAccount
		{
			get { return _activeAccount; }
			set
			{
				if (_activeAccount != value)
				{
					if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
					{
						throw new InvalidOperationException("The property 'ActiveAccount' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
					}
					_activeAccount = value;
					OnPropertyChanged("ActiveAccount");
				}
			}
		}
		private string _activeAccount;
		

		[DataMember]
		public string ActiveUserPercentage
		{
			get { return _activeUserPercentage; }
			set
			{
				if (_activeUserPercentage != value)
				{
					_activeUserPercentage = value;
					OnPropertyChanged("ActiveUserPercentage");
				}
			}
		}
		private string _activeUserPercentage;

		[DataMember]
		public string AvgNoOfConnPerDay
		{
			get { return _avgNoOfConnPerDay; }
			set
			{
				if (_avgNoOfConnPerDay != value)
				{
					_avgNoOfConnPerDay = value;
					OnPropertyChanged("AvgNoOfConnPerDay");
				}
			}
		}
		private string _avgNoOfConnPerDay;

		[DataMember]
		public string AvgNoOfConnPerWeek
		{
			get { return _avgNoOfConnPerWeek; }
			set
			{
				if (_avgNoOfConnPerWeek != value)
				{
					_avgNoOfConnPerWeek = value;
					OnPropertyChanged("AvgNoOfConnPerWeek");
				}
			}
		}
		private string _avgNoOfConnPerWeek;

		[DataMember]
		public string AvgNoOfConnPerMonth
		{
			get { return _avgNoOfConnPerMonth; }
			set
			{
				if (_avgNoOfConnPerMonth != value)
				{
					_avgNoOfConnPerMonth = value;
					OnPropertyChanged("AvgNoOfConnPerMonth");
				}
			}
		}
		private string _avgNoOfConnPerMonth;

		[DataMember]
		public string AvgConnTimePerDay
		{
			get { return _avgConnTimePerDay; }
			set
			{
				if (_avgConnTimePerDay != value)
				{
					_avgConnTimePerDay = value;
					OnPropertyChanged("AvgConnTimePerDay");
				}
			}
		}
		private string _avgConnTimePerDay;

		[DataMember]
		public string AvgConnTimePerWeek
		{
			get { return _avgConnTimePerWeek; }
			set
			{
				if (_avgConnTimePerWeek != value)
				{
					_avgConnTimePerWeek = value;
					OnPropertyChanged("AvgConnTimePerWeek");
				}
			}
		}
		private string _avgConnTimePerWeek;

		[DataMember]
		public string AvgConnTimePerMonth
		{
			get { return _avgConnTimePerMonth; }
			set
			{
				if (_avgConnTimePerMonth != value)
				{
					_avgConnTimePerMonth = value;
					OnPropertyChanged("AvgConnTimePerMonth");
				}
			}
		}
		private string _avgConnTimePerMonth;

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
