using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Inflectra.SpiraTest.DataModel;

namespace Inflectra.SpiraTest.Common
{
	[DataContract(IsReference = true)]
	public partial class SavedReportDetails : IObjectWithChangeTracker, INotifyPropertyChanged
	{
		#region Primitive Properties

		[DataMember]
		public int ReportSavedId
		{
			get { return _reportSavedId; }
			set
			{
				if (_reportSavedId != value)
				{
					if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
					{
						throw new InvalidOperationException("The property 'ReportSavedId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
					}
					_reportSavedId = value;
					OnPropertyChanged("ReportSavedId");
				}
			}
		}
		private int _reportSavedId;

		[DataMember]
		public int ReportId
		{
			get { return _reportId; }
			set
			{
				if (_reportId != value)
				{
					if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
					{
						throw new InvalidOperationException("The property 'ReportId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
					}
					_reportId = value;
					OnPropertyChanged("ReportId");
				}
			}
		}
		private int _reportId;

		[DataMember]
		public int ReportFormatId
		{
			get { return _reportFormatId; }
			set
			{
				if (_reportFormatId != value)
				{
					if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
					{
						throw new InvalidOperationException("The property 'ReportFormatId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
					}
					_reportFormatId = value;
					OnPropertyChanged("ReportFormatId");
				}
			}
		}
		private int _reportFormatId;

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
		public Nullable<int> ProjectId
		{
			get { return _projectId; }
			set
			{
				if (_projectId != value)
				{
					_projectId = value;
					OnPropertyChanged("ProjectId");
				}
			}
		}
		private Nullable<int> _projectId;

		[DataMember]
		public string Name
		{
			get { return _name; }
			set
			{
				if (_name != value)
				{
					if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
					{
						throw new InvalidOperationException("The property 'Name' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
					}
					_name = value;
					OnPropertyChanged("Name");
				}
			}
		}
		private string _name;

		[DataMember]
		public Nullable<System.DateTime> CREATION_DATE
		{
			get { return _cREATION_DATE; }
			set
			{
				if (_cREATION_DATE != value)
				{
					//ChangeTracker.RecordOriginalValue("CREATION_DATE", _cREATION_DATE);
					_cREATION_DATE = value;
					OnPropertyChanged("CREATION_DATE");
				}
			}
		}
		private Nullable<System.DateTime> _cREATION_DATE;

		[DataMember]
		public string FormatName
		{
			get { return _formatName; }
			set
			{
				if (_formatName != value)
				{
					if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
					{
						throw new InvalidOperationException("The property 'Name' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
					}
					_formatName = value;
					OnPropertyChanged("FormatName");
				}
			}
		}
		private string _formatName;

		[DataMember]
		public string Parameters
		{
			get { return _parameters; }
			set
			{
				if (_parameters != value)
				{
					_parameters = value;
					OnPropertyChanged("Parameters");
				}
			}
		}
		private string _parameters;

		[DataMember]
		public bool IsShared
		{
			get { return _isShared; }
			set
			{
				if (_isShared != value)
				{
					if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
					{
						throw new InvalidOperationException("The property 'IsShared' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
					}
					_isShared = value;
					OnPropertyChanged("IsShared");
				}
			}
		}
		private bool _isShared;

		[DataMember]
		public string ProjectName
		{
			get { return _projectName; }
			set
			{
				if (_projectName != value)
				{
					_projectName = value;
					OnPropertyChanged("ProjectName");
				}
			}
		}
		private string _projectName;

		[DataMember]
		public string UserName
		{
			get { return _userName; }
			set
			{
				if (_userName != value)
				{
					_userName = value;
					OnPropertyChanged("UserName");
				}
			}
		}
		private string _userName;

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
					//_changeTracker.ObjectStateChanging += HandleObjectStateChanging;
				}
				return _changeTracker;
			}
			set
			{

				_changeTracker = value;
			}
		}

		#endregion

	}
	
}
