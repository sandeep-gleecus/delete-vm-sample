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
    [KnownType(typeof(Task))]
    [KnownType(typeof(ProjectTemplate))]
    [KnownType(typeof(TaskWorkflow))]
    [KnownType(typeof(StandardTask))]
    public partial class TaskType: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public int TaskTypeId
        {
            get { return _taskTypeId; }
            set
            {
                if (_taskTypeId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'TaskTypeId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _taskTypeId = value;
                    OnPropertyChanged("TaskTypeId");
                }
            }
        }
        private int _taskTypeId;
    
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
        public int Position
        {
            get { return _position; }
            set
            {
                if (_position != value)
                {
                    ChangeTracker.RecordOriginalValue("Position", _position);
                    _position = value;
                    OnPropertyChanged("Position");
                }
            }
        }
        private int _position;
    
        [DataMember]
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value)
                {
                    ChangeTracker.RecordOriginalValue("IsActive", _isActive);
                    _isActive = value;
                    OnPropertyChanged("IsActive");
                }
            }
        }
        private bool _isActive;
    
        [DataMember]
        public int ProjectTemplateId
        {
            get { return _projectTemplateId; }
            set
            {
                if (_projectTemplateId != value)
                {
                    ChangeTracker.RecordOriginalValue("ProjectTemplateId", _projectTemplateId);
                    if (!IsDeserializing)
                    {
                        if (ProjectTemplate != null && ProjectTemplate.ProjectTemplateId != value)
                        {
                            ProjectTemplate = null;
                        }
                    }
                    _projectTemplateId = value;
                    OnPropertyChanged("ProjectTemplateId");
                }
            }
        }
        private int _projectTemplateId;
    
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
        public bool IsDefault
        {
            get { return _isDefault; }
            set
            {
                if (_isDefault != value)
                {
                    ChangeTracker.RecordOriginalValue("IsDefault", _isDefault);
                    _isDefault = value;
                    OnPropertyChanged("IsDefault");
                }
            }
        }
        private bool _isDefault;
    
        [DataMember]
        public bool IsCodeReview
        {
            get { return _isCodeReview; }
            set
            {
                if (_isCodeReview != value)
                {
                    ChangeTracker.RecordOriginalValue("IsCodeReview", _isCodeReview);
                    _isCodeReview = value;
                    OnPropertyChanged("IsCodeReview");
                }
            }
        }
        private bool _isCodeReview;
    
        [DataMember]
        public bool IsPullRequest
        {
            get { return _isPullRequest; }
            set
            {
                if (_isPullRequest != value)
                {
                    ChangeTracker.RecordOriginalValue("IsPullRequest", _isPullRequest);
                    _isPullRequest = value;
                    OnPropertyChanged("IsPullRequest");
                }
            }
        }
        private bool _isPullRequest;

        #endregion

        #region Navigation Properties
    
        [DataMember]
        public TrackableCollection<Task> Tasks
        {
            get
            {
                if (_tasks == null)
                {
                    _tasks = new TrackableCollection<Task>();
                    _tasks.CollectionChanged += FixupTasks;
                }
                return _tasks;
            }
            set
            {
                if (!ReferenceEquals(_tasks, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        throw new InvalidOperationException("Cannot set the FixupChangeTrackingCollection when ChangeTracking is enabled");
                    }
                    if (_tasks != null)
                    {
                        _tasks.CollectionChanged -= FixupTasks;
                    }
                    _tasks = value;
                    if (_tasks != null)
                    {
                        _tasks.CollectionChanged += FixupTasks;
                    }
                    OnNavigationPropertyChanged("Tasks");
                }
            }
        }
        private TrackableCollection<Task> _tasks;
    
        [DataMember]
        public ProjectTemplate ProjectTemplate
        {
            get { return _projectTemplate; }
            set
            {
                if (!ReferenceEquals(_projectTemplate, value))
                {
                    var previousValue = _projectTemplate;
                    _projectTemplate = value;
                    FixupProjectTemplate(previousValue);
                    OnNavigationPropertyChanged("ProjectTemplate");
                }
            }
        }
        private ProjectTemplate _projectTemplate;
    
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
        public TrackableCollection<StandardTask> TST_STANDARD_TASK
        {
            get
            {
                if (_tST_STANDARD_TASK == null)
                {
                    _tST_STANDARD_TASK = new TrackableCollection<StandardTask>();
                    _tST_STANDARD_TASK.CollectionChanged += FixupTST_STANDARD_TASK;
                }
                return _tST_STANDARD_TASK;
            }
            set
            {
                if (!ReferenceEquals(_tST_STANDARD_TASK, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        throw new InvalidOperationException("Cannot set the FixupChangeTrackingCollection when ChangeTracking is enabled");
                    }
                    if (_tST_STANDARD_TASK != null)
                    {
                        _tST_STANDARD_TASK.CollectionChanged -= FixupTST_STANDARD_TASK;
                    }
                    _tST_STANDARD_TASK = value;
                    if (_tST_STANDARD_TASK != null)
                    {
                        _tST_STANDARD_TASK.CollectionChanged += FixupTST_STANDARD_TASK;
                    }
                    OnNavigationPropertyChanged("TST_STANDARD_TASK");
                }
            }
        }
        private TrackableCollection<StandardTask> _tST_STANDARD_TASK;

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
            Tasks.Clear();
            ProjectTemplate = null;
            Workflow = null;
            TST_STANDARD_TASK.Clear();
        }

        #endregion

        #region Association Fixup
    
        private void FixupProjectTemplate(ProjectTemplate previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.TaskTypes.Contains(this))
            {
                previousValue.TaskTypes.Remove(this);
            }
    
            if (ProjectTemplate != null)
            {
                if (!ProjectTemplate.TaskTypes.Contains(this))
                {
                    ProjectTemplate.TaskTypes.Add(this);
                }
    
                ProjectTemplateId = ProjectTemplate.ProjectTemplateId;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("ProjectTemplate")
                    && (ChangeTracker.OriginalValues["ProjectTemplate"] == ProjectTemplate))
                {
                    ChangeTracker.OriginalValues.Remove("ProjectTemplate");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("ProjectTemplate", previousValue);
                }
                if (ProjectTemplate != null && !ProjectTemplate.ChangeTracker.ChangeTrackingEnabled)
                {
                    ProjectTemplate.StartTracking();
                }
            }
        }
    
        private void FixupWorkflow(TaskWorkflow previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.TaskTypes.Contains(this))
            {
                previousValue.TaskTypes.Remove(this);
            }
    
            if (Workflow != null)
            {
                if (!Workflow.TaskTypes.Contains(this))
                {
                    Workflow.TaskTypes.Add(this);
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
    
        private void FixupTasks(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (e.NewItems != null)
            {
                foreach (Task item in e.NewItems)
                {
                    item.Type = this;
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        if (!item.ChangeTracker.ChangeTrackingEnabled)
                        {
                            item.StartTracking();
                        }
                        ChangeTracker.RecordAdditionToCollectionProperties("Tasks", item);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (Task item in e.OldItems)
                {
                    if (ReferenceEquals(item.Type, this))
                    {
                        item.Type = null;
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        ChangeTracker.RecordRemovalFromCollectionProperties("Tasks", item);
                    }
                }
            }
        }
    
        private void FixupTST_STANDARD_TASK(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (e.NewItems != null)
            {
                foreach (StandardTask item in e.NewItems)
                {
                    item.Type = this;
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        if (!item.ChangeTracker.ChangeTrackingEnabled)
                        {
                            item.StartTracking();
                        }
                        ChangeTracker.RecordAdditionToCollectionProperties("TST_STANDARD_TASK", item);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (StandardTask item in e.OldItems)
                {
                    if (ReferenceEquals(item.Type, this))
                    {
                        item.Type = null;
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        ChangeTracker.RecordRemovalFromCollectionProperties("TST_STANDARD_TASK", item);
                    }
                }
            }
        }

        #endregion

    }
}
