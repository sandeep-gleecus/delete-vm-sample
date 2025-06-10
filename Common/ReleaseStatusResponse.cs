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
	[KnownType(typeof(ReleaseStatus))]
	[KnownType(typeof(ReleaseWorkflowTransitionRole))]
	[KnownType(typeof(ReleaseWorkflow))]
	public class ReleaseStatusResponse : IObjectWithChangeTracker, INotifyPropertyChanged
	{
		#region Primitive Properties

		[DataMember]
		public int WorkflowTransitionId
		{
			get { return _workflowTransitionId; }
			set
			{
				if (_workflowTransitionId != value)
				{
					if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
					{
						throw new InvalidOperationException("The property 'WorkflowTransitionId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
					}
					_workflowTransitionId = value;
					OnPropertyChanged("WorkflowTransitionId");
				}
			}
		}
		private int _workflowTransitionId;

		[DataMember]
		public string URL
		{
			get { return _url; }
			set
			{
				if (_url != value)
				{
					_url = value;
					OnPropertyChanged("URL");
				}
			}
		}
		private string _url;

		[DataMember]
		public string OutURL
		{
			get { return _outurl; }
			set
			{
				if (_outurl != value)
				{
					_outurl = value;
					OnPropertyChanged("OutURL");
				}
			}
		}
		private string _outurl;

		[DataMember]
		public string InURL
		{
			get { return _inurl; }
			set
			{
				if (_inurl != value)
				{
					_inurl = value;
					OnPropertyChanged("InURL");
				}
			}
		}
		private string _inurl;

		[DataMember]
		public Nullable<int> ReleaseWorkflowId
		{
			get { return _ReleaseWorkflowId; }
			set
			{
				if (_ReleaseWorkflowId != value)
				{

					if (!IsDeserializing)
					{
						if (Workflow != null && Workflow.ReleaseWorkflowId != value)
						{
							Workflow = null;
						}
					}
					_ReleaseWorkflowId = value;
					OnPropertyChanged("ReleaseWorkflowId");
				}
			}
		}
		private Nullable<int> _ReleaseWorkflowId;

		[DataMember]
		public int InputReleaseStatusId
		{
			get { return _inputReleaseStatusId; }
			set
			{
				if (_inputReleaseStatusId != value)
				{
					if (!IsDeserializing)
					{
						if (InputReleaseStatus != null && InputReleaseStatus.ReleaseStatusId != value)
						{
							InputReleaseStatus = null;
						}
					}
					_inputReleaseStatusId = value;
					OnPropertyChanged("InputReleaseStatusId");
				}
			}
		}
		private int _inputReleaseStatusId;

		[DataMember]
		public int OutputReleaseStatusId
		{
			get { return _outputReleaseStatusId; }
			set
			{
				if (_outputReleaseStatusId != value)
				{
					if (!IsDeserializing)
					{
						if (OutputReleaseStatus != null && OutputReleaseStatus.ReleaseStatusId != value)
						{
							OutputReleaseStatus = null;
						}
					}
					_outputReleaseStatusId = value;
					OnPropertyChanged("OutputReleaseStatusId");
				}
			}
		}
		private int _outputReleaseStatusId;

		[DataMember]
		public string TransitionName
		{
			get { return _name; }
			set
			{
				if (_name != value)
				{
					_name = value;
					OnPropertyChanged("TransitionName");
				}
			}
		}
		private string _name;

		[DataMember]
		public string InputStatusName
		{
			get { return _inputstatusname; }
			set
			{
				if (_inputstatusname != value)
				{
					_inputstatusname = value;
					OnPropertyChanged("InputStatusName");
				}
			}
		}
		private string _inputstatusname;

		[DataMember]
		public string OutputStatusName
		{
			get { return _outputstatusname; }
			set
			{
				if (_outputstatusname != value)
				{
					_outputstatusname = value;
					OnPropertyChanged("OutputStatusName");
				}
			}
		}
		private string _outputstatusname;

		[DataMember]
		public bool IsExecuteByCreator
		{
			get { return _isExecuteByCreator; }
			set
			{
				if (_isExecuteByCreator != value)
				{
					_isExecuteByCreator = value;
					OnPropertyChanged("IsExecuteByCreator");
				}
			}
		}
		private bool _isExecuteByCreator;

		[DataMember]
		public bool IsExecuteByOwner
		{
			get { return _isExecuteByOwner; }
			set
			{
				if (_isExecuteByOwner != value)
				{
					_isExecuteByOwner = value;
					OnPropertyChanged("IsExecuteByOwner");
				}
			}
		}
		private bool _isExecuteByOwner;

		[DataMember]
		public bool IsSignatureRequired
		{
			get { return _isSignatureRequired; }
			set
			{
				if (_isSignatureRequired != value)
				{
					_isSignatureRequired = value;
					OnPropertyChanged("IsSignatureRequired");
				}
			}
		}
		private bool _isSignatureRequired;

		[DataMember]
		public bool IsBlankOwner
		{
			get { return _isBlankOwner; }
			set
			{
				if (_isBlankOwner != value)
				{
					_isBlankOwner = value;
					OnPropertyChanged("IsBlankOwner");
				}
			}
		}
		private bool _isBlankOwner;

		[DataMember]
		public bool IsNotifyCreator
		{
			get { return _isNotifyCreator; }
			set
			{
				if (_isNotifyCreator != value)
				{
					_isNotifyCreator = value;
					OnPropertyChanged("IsNotifyCreator");
				}
			}
		}
		private bool _isNotifyCreator;

		[DataMember]
		public bool IsNotifyOwner
		{
			get { return _isNotifyOwner; }
			set
			{
				if (_isNotifyOwner != value)
				{
					_isNotifyOwner = value;
					OnPropertyChanged("IsNotifyOwner");
				}
			}
		}
		private bool _isNotifyOwner;

		[DataMember]
		public string NotifySubject
		{
			get { return _notifySubject; }
			set
			{
				if (_notifySubject != value)
				{
					_notifySubject = value;
					OnPropertyChanged("NotifySubject");
				}
			}
		}
		private string _notifySubject;

		#endregion

		#region Navigation Properties

		[DataMember]
		public ReleaseStatus InputReleaseStatus
		{
			get { return _inputReleaseStatus; }
			set
			{
				if (!ReferenceEquals(_inputReleaseStatus, value))
				{
					var previousValue = _inputReleaseStatus;
					_inputReleaseStatus = value;
					//FixupInputReleaseStatus(previousValue);
					OnNavigationPropertyChanged("InputReleaseStatus");
				}
			}
		}
		private ReleaseStatus _inputReleaseStatus;

		[DataMember]
		public ReleaseStatus OutputReleaseStatus
		{
			get { return _outputReleaseStatus; }
			set
			{
				if (!ReferenceEquals(_outputReleaseStatus, value))
				{
					var previousValue = _outputReleaseStatus;
					_outputReleaseStatus = value;
					//FixupOutputReleaseStatus(previousValue);
					OnNavigationPropertyChanged("OutputReleaseStatus");
				}
			}
		}
		private ReleaseStatus _outputReleaseStatus;

		[DataMember]
		public TrackableCollection<ReleaseWorkflowTransitionRole> TransitionRoles
		{
			get
			{
				if (_transitionRoles == null)
				{
					_transitionRoles = new TrackableCollection<ReleaseWorkflowTransitionRole>();
					//_transitionRoles.CollectionChanged += FixupTransitionRoles;
				}
				return _transitionRoles;
			}
			set
			{
				if (!ReferenceEquals(_transitionRoles, value))
				{
					if (ChangeTracker.ChangeTrackingEnabled)
					{
						throw new InvalidOperationException("Cannot set the FixupChangeTrackingCollection when ChangeTracking is enabled");
					}
					if (_transitionRoles != null)
					{
						//_transitionRoles.CollectionChanged -= FixupTransitionRoles;
						// This is the principal end in an association that performs cascade deletes.
						// Remove the cascade delete event handler for any entities in the current collection.

					}
					_transitionRoles = value;
					if (_transitionRoles != null)
					{
						//	_transitionRoles.CollectionChanged += FixupTransitionRoles;
						// This is the principal end in an association that performs cascade deletes.
						// Add the cascade delete event handler for any entities that are already in the new collection.

					}
					OnNavigationPropertyChanged("TransitionRoles");
				}
			}
		}
		private TrackableCollection<ReleaseWorkflowTransitionRole> _transitionRoles;

		[DataMember]
		public ReleaseWorkflow Workflow
		{
			get { return _workflow; }
			set
			{
				if (!ReferenceEquals(_workflow, value))
				{
					var previousValue = _workflow;
					_workflow = value;
					OnNavigationPropertyChanged("Workflow");
				}
			}
		}
		private ReleaseWorkflow _workflow;

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
			InputReleaseStatus = null;
			OutputReleaseStatus = null;
			TransitionRoles.Clear();
			Workflow = null;
		}

		#endregion


	}
}
