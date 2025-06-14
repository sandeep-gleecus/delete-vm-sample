//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;

namespace Inflectra.SpiraTest.DataModel
{
    [DataContract(IsReference = true)]
    [KnownType(typeof(TaskStatus))]
    [KnownType(typeof(TaskWorkflow))]
    [KnownType(typeof(TaskWorkflowTransitionRole))]
    public partial class TaskWorkflowTransition: IObjectWithChangeTracker, INotifyPropertyChanged
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
        public int TaskWorkflowId
        {
            get { return _taskWorkflowId; }
            set
            {
                if (_taskWorkflowId != value)
                {
                    ChangeTracker.RecordOriginalValue("TaskWorkflowId", _taskWorkflowId);
                    if (!IsDeserializing)
                    {
                        if (Workflow != null && Workflow.TaskWorkflowId != value)
                        {
                            Workflow = null;
                        }
                    }
                    _taskWorkflowId = value;
                    OnPropertyChanged("TaskWorkflowId");
                }
            }
        }
        private int _taskWorkflowId;
    
        [DataMember]
        public int InputTaskStatusId
        {
            get { return _inputTaskStatusId; }
            set
            {
                if (_inputTaskStatusId != value)
                {
                    ChangeTracker.RecordOriginalValue("InputTaskStatusId", _inputTaskStatusId);
                    if (!IsDeserializing)
                    {
                        if (InputTaskStatus != null && InputTaskStatus.TaskStatusId != value)
                        {
                            InputTaskStatus = null;
                        }
                    }
                    _inputTaskStatusId = value;
                    OnPropertyChanged("InputTaskStatusId");
                }
            }
        }
        private int _inputTaskStatusId;
    
        [DataMember]
        public int OutputTaskStatusId
        {
            get { return _outputTaskStatusId; }
            set
            {
                if (_outputTaskStatusId != value)
                {
                    ChangeTracker.RecordOriginalValue("OutputTaskStatusId", _outputTaskStatusId);
                    if (!IsDeserializing)
                    {
                        if (OutputTaskStatus != null && OutputTaskStatus.TaskStatusId != value)
                        {
                            OutputTaskStatus = null;
                        }
                    }
                    _outputTaskStatusId = value;
                    OnPropertyChanged("OutputTaskStatusId");
                }
            }
        }
        private int _outputTaskStatusId;
    
        [DataMember]
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    ChangeTracker.RecordOriginalValue("Name", _name);
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        private string _name;
    
        [DataMember]
        public bool IsExecuteByCreator
        {
            get { return _isExecuteByCreator; }
            set
            {
                if (_isExecuteByCreator != value)
                {
                    ChangeTracker.RecordOriginalValue("IsExecuteByCreator", _isExecuteByCreator);
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
                    ChangeTracker.RecordOriginalValue("IsExecuteByOwner", _isExecuteByOwner);
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
                    ChangeTracker.RecordOriginalValue("IsSignatureRequired", _isSignatureRequired);
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
                    ChangeTracker.RecordOriginalValue("IsBlankOwner", _isBlankOwner);
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
                    ChangeTracker.RecordOriginalValue("IsNotifyCreator", _isNotifyCreator);
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
                    ChangeTracker.RecordOriginalValue("IsNotifyOwner", _isNotifyOwner);
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
                    ChangeTracker.RecordOriginalValue("NotifySubject", _notifySubject);
                    _notifySubject = value;
                    OnPropertyChanged("NotifySubject");
                }
            }
        }
        private string _notifySubject;

        #endregion

        #region Navigation Properties
    
        [DataMember]
        public TaskStatus InputTaskStatus
        {
            get { return _inputTaskStatus; }
            set
            {
                if (!ReferenceEquals(_inputTaskStatus, value))
                {
                    var previousValue = _inputTaskStatus;
                    _inputTaskStatus = value;
                    FixupInputTaskStatus(previousValue);
                    OnNavigationPropertyChanged("InputTaskStatus");
                }
            }
        }
        private TaskStatus _inputTaskStatus;
    
        [DataMember]
        public TaskStatus OutputTaskStatus
        {
            get { return _outputTaskStatus; }
            set
            {
                if (!ReferenceEquals(_outputTaskStatus, value))
                {
                    var previousValue = _outputTaskStatus;
                    _outputTaskStatus = value;
                    FixupOutputTaskStatus(previousValue);
                    OnNavigationPropertyChanged("OutputTaskStatus");
                }
            }
        }
        private TaskStatus _outputTaskStatus;
    
        [DataMember]
        public TaskWorkflow Workflow
        {
            get { return _workflow; }
            set
            {
                if (!ReferenceEquals(_workflow, value))
                {
                    var previousValue = _workflow;
                    _workflow = value;
                    FixupWorkflow(previousValue);
                    OnNavigationPropertyChanged("Workflow");
                }
            }
        }
        private TaskWorkflow _workflow;
    
        [DataMember]
        public TrackableCollection<TaskWorkflowTransitionRole> TransitionRoles
        {
            get
            {
                if (_transitionRoles == null)
                {
                    _transitionRoles = new TrackableCollection<TaskWorkflowTransitionRole>();
                    _transitionRoles.CollectionChanged += FixupTransitionRoles;
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
                        _transitionRoles.CollectionChanged -= FixupTransitionRoles;
                        // This is the principal end in an association that performs cascade deletes.
                        // Remove the cascade delete event handler for any entities in the current collection.
                        foreach (TaskWorkflowTransitionRole item in _transitionRoles)
                        {
                            ChangeTracker.ObjectStateChanging -= item.HandleCascadeDelete;
                        }
                    }
                    _transitionRoles = value;
                    if (_transitionRoles != null)
                    {
                        _transitionRoles.CollectionChanged += FixupTransitionRoles;
                        // This is the principal end in an association that performs cascade deletes.
                        // Add the cascade delete event handler for any entities that are already in the new collection.
                        foreach (TaskWorkflowTransitionRole item in _transitionRoles)
                        {
                            ChangeTracker.ObjectStateChanging += item.HandleCascadeDelete;
                        }
                    }
                    OnNavigationPropertyChanged("TransitionRoles");
                }
            }
        }
        private TrackableCollection<TaskWorkflowTransitionRole> _transitionRoles;

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
    
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged{ add { _propertyChanged += value; } remove { _propertyChanged -= value; } }
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
                if(_changeTracker != null)
                {
                    _changeTracker.ObjectStateChanging -= HandleObjectStateChanging;
                }
                _changeTracker = value;
                if(_changeTracker != null)
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
            InputTaskStatus = null;
            OutputTaskStatus = null;
            Workflow = null;
            TransitionRoles.Clear();
        }

        #endregion

        #region Association Fixup
    
        private void FixupInputTaskStatus(TaskStatus previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.WorkflowTransitionsInput.Contains(this))
            {
                previousValue.WorkflowTransitionsInput.Remove(this);
            }
    
            if (InputTaskStatus != null)
            {
                if (!InputTaskStatus.WorkflowTransitionsInput.Contains(this))
                {
                    InputTaskStatus.WorkflowTransitionsInput.Add(this);
                }
    
                InputTaskStatusId = InputTaskStatus.TaskStatusId;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("InputTaskStatus")
                    && (ChangeTracker.OriginalValues["InputTaskStatus"] == InputTaskStatus))
                {
                    ChangeTracker.OriginalValues.Remove("InputTaskStatus");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("InputTaskStatus", previousValue);
                }
                if (InputTaskStatus != null && !InputTaskStatus.ChangeTracker.ChangeTrackingEnabled)
                {
                    InputTaskStatus.StartTracking();
                }
            }
        }
    
        private void FixupOutputTaskStatus(TaskStatus previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.WorkflowTransitionsOutput.Contains(this))
            {
                previousValue.WorkflowTransitionsOutput.Remove(this);
            }
    
            if (OutputTaskStatus != null)
            {
                if (!OutputTaskStatus.WorkflowTransitionsOutput.Contains(this))
                {
                    OutputTaskStatus.WorkflowTransitionsOutput.Add(this);
                }
    
                OutputTaskStatusId = OutputTaskStatus.TaskStatusId;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("OutputTaskStatus")
                    && (ChangeTracker.OriginalValues["OutputTaskStatus"] == OutputTaskStatus))
                {
                    ChangeTracker.OriginalValues.Remove("OutputTaskStatus");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("OutputTaskStatus", previousValue);
                }
                if (OutputTaskStatus != null && !OutputTaskStatus.ChangeTracker.ChangeTrackingEnabled)
                {
                    OutputTaskStatus.StartTracking();
                }
            }
        }
    
        private void FixupWorkflow(TaskWorkflow previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.Transitions.Contains(this))
            {
                previousValue.Transitions.Remove(this);
            }
    
            if (Workflow != null)
            {
                if (!Workflow.Transitions.Contains(this))
                {
                    Workflow.Transitions.Add(this);
                }
    
                TaskWorkflowId = Workflow.TaskWorkflowId;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("Workflow")
                    && (ChangeTracker.OriginalValues["Workflow"] == Workflow))
                {
                    ChangeTracker.OriginalValues.Remove("Workflow");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("Workflow", previousValue);
                }
                if (Workflow != null && !Workflow.ChangeTracker.ChangeTrackingEnabled)
                {
                    Workflow.StartTracking();
                }
            }
        }
    
        private void FixupTransitionRoles(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (e.NewItems != null)
            {
                foreach (TaskWorkflowTransitionRole item in e.NewItems)
                {
                    item.Transition = this;
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        if (!item.ChangeTracker.ChangeTrackingEnabled)
                        {
                            item.StartTracking();
                        }
                        ChangeTracker.RecordAdditionToCollectionProperties("TransitionRoles", item);
                    }
                    // This is the principal end in an association that performs cascade deletes.
                    // Update the event listener to refer to the new dependent.
                    ChangeTracker.ObjectStateChanging += item.HandleCascadeDelete;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (TaskWorkflowTransitionRole item in e.OldItems)
                {
                    if (ReferenceEquals(item.Transition, this))
                    {
                        item.Transition = null;
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        ChangeTracker.RecordRemovalFromCollectionProperties("TransitionRoles", item);
                        // Delete the dependent end of this identifying association. If the current state is Added,
                        // allow the relationship to be changed without causing the dependent to be deleted.
                        if (item.ChangeTracker.State != ObjectState.Added)
                        {
                            item.MarkAsDeleted();
                        }
                    }
                    // This is the principal end in an association that performs cascade deletes.
                    // Remove the previous dependent from the event listener.
                    ChangeTracker.ObjectStateChanging -= item.HandleCascadeDelete;
                }
            }
        }

        #endregion

    }
}
