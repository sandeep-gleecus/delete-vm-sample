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
    [KnownType(typeof(ArtifactField))]
    [KnownType(typeof(Workflow))]
    [KnownType(typeof(WorkflowFieldState))]
    [KnownType(typeof(IncidentStatus))]
    public partial class WorkflowField: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public int WorkflowId
        {
            get { return _workflowId; }
            set
            {
                if (_workflowId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'WorkflowId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    if (!IsDeserializing)
                    {
                        if (Workflow != null && Workflow.WorkflowId != value)
                        {
                            Workflow = null;
                        }
                    }
                    _workflowId = value;
                    OnPropertyChanged("WorkflowId");
                }
            }
        }
        private int _workflowId;
    
        [DataMember]
        public int ArtifactFieldId
        {
            get { return _artifactFieldId; }
            set
            {
                if (_artifactFieldId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'ArtifactFieldId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    if (!IsDeserializing)
                    {
                        if (Field != null && Field.ArtifactFieldId != value)
                        {
                            Field = null;
                        }
                    }
                    _artifactFieldId = value;
                    OnPropertyChanged("ArtifactFieldId");
                }
            }
        }
        private int _artifactFieldId;
    
        [DataMember]
        public int IncidentStatusId
        {
            get { return _incidentStatusId; }
            set
            {
                if (_incidentStatusId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'IncidentStatusId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    if (!IsDeserializing)
                    {
                        if (Status != null && Status.IncidentStatusId != value)
                        {
                            Status = null;
                        }
                    }
                    _incidentStatusId = value;
                    OnPropertyChanged("IncidentStatusId");
                }
            }
        }
        private int _incidentStatusId;
    
        [DataMember]
        public int WorkflowFieldStateId
        {
            get { return _workflowFieldStateId; }
            set
            {
                if (_workflowFieldStateId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'WorkflowFieldStateId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    if (!IsDeserializing)
                    {
                        if (State != null && State.WorkflowFieldStateId != value)
                        {
                            State = null;
                        }
                    }
                    _workflowFieldStateId = value;
                    OnPropertyChanged("WorkflowFieldStateId");
                }
            }
        }
        private int _workflowFieldStateId;

        #endregion

        #region Navigation Properties
    
        [DataMember]
        public ArtifactField Field
        {
            get { return _field; }
            set
            {
                if (!ReferenceEquals(_field, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added && value != null)
                    {
                        // This the dependent end of an identifying relationship, so the principal end cannot be changed if it is already set,
                        // otherwise it can only be set to an entity with a primary key that is the same value as the dependent's foreign key.
                        if (ArtifactFieldId != value.ArtifactFieldId)
                        {
                            throw new InvalidOperationException("The principal end of an identifying relationship can only be changed when the dependent end is in the Added state.");
                        }
                    }
                    var previousValue = _field;
                    _field = value;
                    FixupField(previousValue);
                    OnNavigationPropertyChanged("Field");
                }
            }
        }
        private ArtifactField _field;
    
        [DataMember]
        public Workflow Workflow
        {
            get { return _workflow; }
            set
            {
                if (!ReferenceEquals(_workflow, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added && value != null)
                    {
                        // This the dependent end of an identifying relationship, so the principal end cannot be changed if it is already set,
                        // otherwise it can only be set to an entity with a primary key that is the same value as the dependent's foreign key.
                        if (WorkflowId != value.WorkflowId)
                        {
                            throw new InvalidOperationException("The principal end of an identifying relationship can only be changed when the dependent end is in the Added state.");
                        }
                    }
                    var previousValue = _workflow;
                    _workflow = value;
                    FixupWorkflow(previousValue);
                    OnNavigationPropertyChanged("Workflow");
                }
            }
        }
        private Workflow _workflow;
    
        [DataMember]
        public WorkflowFieldState State
        {
            get { return _state; }
            set
            {
                if (!ReferenceEquals(_state, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added && value != null)
                    {
                        // This the dependent end of an identifying relationship, so the principal end cannot be changed if it is already set,
                        // otherwise it can only be set to an entity with a primary key that is the same value as the dependent's foreign key.
                        if (WorkflowFieldStateId != value.WorkflowFieldStateId)
                        {
                            throw new InvalidOperationException("The principal end of an identifying relationship can only be changed when the dependent end is in the Added state.");
                        }
                    }
                    var previousValue = _state;
                    _state = value;
                    FixupState(previousValue);
                    OnNavigationPropertyChanged("State");
                }
            }
        }
        private WorkflowFieldState _state;
    
        [DataMember]
        public IncidentStatus Status
        {
            get { return _status; }
            set
            {
                if (!ReferenceEquals(_status, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added && value != null)
                    {
                        // This the dependent end of an identifying relationship, so the principal end cannot be changed if it is already set,
                        // otherwise it can only be set to an entity with a primary key that is the same value as the dependent's foreign key.
                        if (IncidentStatusId != value.IncidentStatusId)
                        {
                            throw new InvalidOperationException("The principal end of an identifying relationship can only be changed when the dependent end is in the Added state.");
                        }
                    }
                    var previousValue = _status;
                    _status = value;
                    FixupStatus(previousValue);
                    OnNavigationPropertyChanged("Status");
                }
            }
        }
        private IncidentStatus _status;

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
    
        // This entity type is the dependent end in at least one association that performs cascade deletes.
        // This event handler will process notifications that occur when the principal end is deleted.
        internal void HandleCascadeDelete(object sender, ObjectStateChangingEventArgs e)
        {
            if (e.NewState == ObjectState.Deleted)
            {
                this.MarkAsDeleted();
            }
        }
    
        protected virtual void ClearNavigationProperties()
        {
            Field = null;
            Workflow = null;
            State = null;
            Status = null;
        }

        #endregion

        #region Association Fixup
    
        private void FixupField(ArtifactField previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.IncidentWorkflowFields.Contains(this))
            {
                previousValue.IncidentWorkflowFields.Remove(this);
            }
    
            if (Field != null)
            {
                if (!Field.IncidentWorkflowFields.Contains(this))
                {
                    Field.IncidentWorkflowFields.Add(this);
                }
    
                ArtifactFieldId = Field.ArtifactFieldId;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("Field")
                    && (ChangeTracker.OriginalValues["Field"] == Field))
                {
                    ChangeTracker.OriginalValues.Remove("Field");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("Field", previousValue);
                }
                if (Field != null && !Field.ChangeTracker.ChangeTrackingEnabled)
                {
                    Field.StartTracking();
                }
            }
        }
    
        private void FixupWorkflow(Workflow previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.Fields.Contains(this))
            {
                previousValue.Fields.Remove(this);
            }
    
            if (Workflow != null)
            {
                if (!Workflow.Fields.Contains(this))
                {
                    Workflow.Fields.Add(this);
                }
    
                WorkflowId = Workflow.WorkflowId;
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
    
        private void FixupState(WorkflowFieldState previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.IncidentFields.Contains(this))
            {
                previousValue.IncidentFields.Remove(this);
            }
    
            if (State != null)
            {
                if (!State.IncidentFields.Contains(this))
                {
                    State.IncidentFields.Add(this);
                }
    
                WorkflowFieldStateId = State.WorkflowFieldStateId;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("State")
                    && (ChangeTracker.OriginalValues["State"] == State))
                {
                    ChangeTracker.OriginalValues.Remove("State");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("State", previousValue);
                }
                if (State != null && !State.ChangeTracker.ChangeTrackingEnabled)
                {
                    State.StartTracking();
                }
            }
        }
    
        private void FixupStatus(IncidentStatus previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.WorkflowFields.Contains(this))
            {
                previousValue.WorkflowFields.Remove(this);
            }
    
            if (Status != null)
            {
                if (!Status.WorkflowFields.Contains(this))
                {
                    Status.WorkflowFields.Add(this);
                }
    
                IncidentStatusId = Status.IncidentStatusId;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("Status")
                    && (ChangeTracker.OriginalValues["Status"] == Status))
                {
                    ChangeTracker.OriginalValues.Remove("Status");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("Status", previousValue);
                }
                if (Status != null && !Status.ChangeTracker.ChangeTrackingEnabled)
                {
                    Status.StartTracking();
                }
            }
        }

        #endregion

    }
}
