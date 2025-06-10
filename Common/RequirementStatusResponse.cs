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
	[KnownType(typeof(RequirementStatus))]
	[KnownType(typeof(RequirementWorkflowTransitionRole))]
	[KnownType(typeof(RequirementWorkflow))]
	public partial class RequirementStatusResponse : IObjectWithChangeTracker, INotifyPropertyChanged
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
		public Nullable<int> RequirementWorkflowId
		{
			get { return _requirementWorkflowId; }
			set
			{
				if (_requirementWorkflowId != value)
				{
					
					if (!IsDeserializing)
					{
						if (Workflow != null && Workflow.RequirementWorkflowId != value)
						{
							Workflow = null;
						}
					}
					_requirementWorkflowId = value;
					OnPropertyChanged("RequirementWorkflowId");
				}
			}
		}
		private Nullable<int> _requirementWorkflowId;

		[DataMember]
		public int InputRequirementStatusId
		{
			get { return _inputRequirementStatusId; }
			set
			{
				if (_inputRequirementStatusId != value)
				{
					if (!IsDeserializing)
					{
						if (InputRequirementStatus != null && InputRequirementStatus.RequirementStatusId != value)
						{
							InputRequirementStatus = null;
						}
					}
					_inputRequirementStatusId = value;
					OnPropertyChanged("InputRequirementStatusId");
				}
			}
		}
		private int _inputRequirementStatusId;

		[DataMember]
		public int OutputRequirementStatusId
		{
			get { return _outputRequirementStatusId; }
			set
			{
				if (_outputRequirementStatusId != value)
				{
					if (!IsDeserializing)
					{
						if (OutputRequirementStatus != null && OutputRequirementStatus.RequirementStatusId != value)
						{
							OutputRequirementStatus = null;
						}
					}
					_outputRequirementStatusId = value;
					OnPropertyChanged("OutputRequirementStatusId");
				}
			}
		}
		private int _outputRequirementStatusId;

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
					OnPropertyChanged("InRLU");
				}
			}
		}
		private string _inurl;


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
		public RequirementStatus InputRequirementStatus
		{
			get { return _inputRequirementStatus; }
			set
			{
				if (!ReferenceEquals(_inputRequirementStatus, value))
				{
					var previousValue = _inputRequirementStatus;
					_inputRequirementStatus = value;
					//FixupInputRequirementStatus(previousValue);
					OnNavigationPropertyChanged("InputRequirementStatus");
				}
			}
		}
		private RequirementStatus _inputRequirementStatus;

		[DataMember]
		public RequirementStatus OutputRequirementStatus
		{
			get { return _outputRequirementStatus; }
			set
			{
				if (!ReferenceEquals(_outputRequirementStatus, value))
				{
					var previousValue = _outputRequirementStatus;
					_outputRequirementStatus = value;
					//FixupOutputRequirementStatus(previousValue);
					OnNavigationPropertyChanged("OutputRequirementStatus");
				}
			}
		}
		private RequirementStatus _outputRequirementStatus;

		[DataMember]
		public TrackableCollection<RequirementWorkflowTransitionRole> TransitionRoles
		{
			get
			{
				if (_transitionRoles == null)
				{
					_transitionRoles = new TrackableCollection<RequirementWorkflowTransitionRole>();
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
		private TrackableCollection<RequirementWorkflowTransitionRole> _transitionRoles;

		[DataMember]
		public RequirementWorkflow Workflow
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
		private RequirementWorkflow _workflow;

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
			InputRequirementStatus = null;
			OutputRequirementStatus = null;
			TransitionRoles.Clear();
			Workflow = null;
		}

		#endregion


	}
}
