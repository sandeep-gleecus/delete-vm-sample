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
    [KnownType(typeof(Project))]
    public partial class TestCaseFolderHierarchy: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public int TestCaseFolderId
        {
            get { return _testCaseFolderId; }
            set
            {
                if (_testCaseFolderId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'TestCaseFolderId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _testCaseFolderId = value;
                    OnPropertyChanged("TestCaseFolderId");
                }
            }
        }
        private int _testCaseFolderId;
    
        [DataMember]
        public int ProjectId
        {
            get { return _projectId; }
            set
            {
                if (_projectId != value)
                {
                    ChangeTracker.RecordOriginalValue("ProjectId", _projectId);
                    if (!IsDeserializing)
                    {
                        if (Project != null && Project.ProjectId != value)
                        {
                            Project = null;
                        }
                    }
                    _projectId = value;
                    OnPropertyChanged("ProjectId");
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
        public Nullable<int> ParentTestCaseFolderId
        {
            get { return _parentTestCaseFolderId; }
            set
            {
                if (_parentTestCaseFolderId != value)
                {
                    ChangeTracker.RecordOriginalValue("ParentTestCaseFolderId", _parentTestCaseFolderId);
                    _parentTestCaseFolderId = value;
                    OnPropertyChanged("ParentTestCaseFolderId");
                }
            }
        }
        private Nullable<int> _parentTestCaseFolderId;
    
        [DataMember]
        public Nullable<int> HierarchyLevel
        {
            get { return _hierarchyLevel; }
            set
            {
                if (_hierarchyLevel != value)
                {
                    ChangeTracker.RecordOriginalValue("HierarchyLevel", _hierarchyLevel);
                    _hierarchyLevel = value;
                    OnPropertyChanged("HierarchyLevel");
                }
            }
        }
        private Nullable<int> _hierarchyLevel;
    
        [DataMember]
        public string IndentLevel
        {
            get { return _indentLevel; }
            set
            {
                if (_indentLevel != value)
                {
                    ChangeTracker.RecordOriginalValue("IndentLevel", _indentLevel);
                    _indentLevel = value;
                    OnPropertyChanged("IndentLevel");
                }
            }
        }
        private string _indentLevel;

        #endregion

        #region Navigation Properties
    
        [DataMember]
        public Project Project
        {
            get { return _project; }
            set
            {
                if (!ReferenceEquals(_project, value))
                {
                    var previousValue = _project;
                    _project = value;
                    FixupProject(previousValue);
                    OnNavigationPropertyChanged("Project");
                }
            }
        }
        private Project _project;

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
            Project = null;
        }

        #endregion

        #region Association Fixup
    
        private void FixupProject(Project previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.TestCaseFoldersHierarchy.Contains(this))
            {
                previousValue.TestCaseFoldersHierarchy.Remove(this);
            }
    
            if (Project != null)
            {
                if (!Project.TestCaseFoldersHierarchy.Contains(this))
                {
                    Project.TestCaseFoldersHierarchy.Add(this);
                }
    
                ProjectId = Project.ProjectId;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("Project")
                    && (ChangeTracker.OriginalValues["Project"] == Project))
                {
                    ChangeTracker.OriginalValues.Remove("Project");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("Project", previousValue);
                }
                if (Project != null && !Project.ChangeTracker.ChangeTrackingEnabled)
                {
                    Project.StartTracking();
                }
            }
        }

        #endregion

    }
}
