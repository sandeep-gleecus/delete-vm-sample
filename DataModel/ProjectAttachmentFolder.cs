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
    [KnownType(typeof(ProjectAttachment))]
    [KnownType(typeof(ProjectAttachmentFolder))]
    public partial class ProjectAttachmentFolder: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public int ProjectAttachmentFolderId
        {
            get { return _projectAttachmentFolderId; }
            set
            {
                if (_projectAttachmentFolderId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'ProjectAttachmentFolderId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _projectAttachmentFolderId = value;
                    OnPropertyChanged("ProjectAttachmentFolderId");
                }
            }
        }
        private int _projectAttachmentFolderId;
    
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
                        if (Folder != null && Folder.ProjectId != value)
                        {
                            Folder = null;
                        }
                    }
                    _projectId = value;
                    OnPropertyChanged("ProjectId");
                }
            }
        }
        private int _projectId;
    
        [DataMember]
        public Nullable<int> ParentProjectAttachmentFolderId
        {
            get { return _parentProjectAttachmentFolderId; }
            set
            {
                if (_parentProjectAttachmentFolderId != value)
                {
                    ChangeTracker.RecordOriginalValue("ParentProjectAttachmentFolderId", _parentProjectAttachmentFolderId);
                    if (!IsDeserializing)
                    {
                        if (Parent != null && Parent.ProjectAttachmentFolderId != value)
                        {
                            Parent = null;
                        }
                    }
                    _parentProjectAttachmentFolderId = value;
                    OnPropertyChanged("ParentProjectAttachmentFolderId");
                }
            }
        }
        private Nullable<int> _parentProjectAttachmentFolderId;
    
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

        #endregion

        #region Navigation Properties
    
        [DataMember]
        public Project Folder
        {
            get { return _folder; }
            set
            {
                if (!ReferenceEquals(_folder, value))
                {
                    var previousValue = _folder;
                    _folder = value;
                    FixupFolder(previousValue);
                    OnNavigationPropertyChanged("Folder");
                }
            }
        }
        private Project _folder;
    
        [DataMember]
        public TrackableCollection<ProjectAttachment> Attachments
        {
            get
            {
                if (_attachments == null)
                {
                    _attachments = new TrackableCollection<ProjectAttachment>();
                    _attachments.CollectionChanged += FixupAttachments;
                }
                return _attachments;
            }
            set
            {
                if (!ReferenceEquals(_attachments, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        throw new InvalidOperationException("Cannot set the FixupChangeTrackingCollection when ChangeTracking is enabled");
                    }
                    if (_attachments != null)
                    {
                        _attachments.CollectionChanged -= FixupAttachments;
                    }
                    _attachments = value;
                    if (_attachments != null)
                    {
                        _attachments.CollectionChanged += FixupAttachments;
                    }
                    OnNavigationPropertyChanged("Attachments");
                }
            }
        }
        private TrackableCollection<ProjectAttachment> _attachments;
    
        [DataMember]
        public TrackableCollection<ProjectAttachmentFolder> Children
        {
            get
            {
                if (_children == null)
                {
                    _children = new TrackableCollection<ProjectAttachmentFolder>();
                    _children.CollectionChanged += FixupChildren;
                }
                return _children;
            }
            set
            {
                if (!ReferenceEquals(_children, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        throw new InvalidOperationException("Cannot set the FixupChangeTrackingCollection when ChangeTracking is enabled");
                    }
                    if (_children != null)
                    {
                        _children.CollectionChanged -= FixupChildren;
                    }
                    _children = value;
                    if (_children != null)
                    {
                        _children.CollectionChanged += FixupChildren;
                    }
                    OnNavigationPropertyChanged("Children");
                }
            }
        }
        private TrackableCollection<ProjectAttachmentFolder> _children;
    
        [DataMember]
        public ProjectAttachmentFolder Parent
        {
            get { return _parent; }
            set
            {
                if (!ReferenceEquals(_parent, value))
                {
                    var previousValue = _parent;
                    _parent = value;
                    FixupParent(previousValue);
                    OnNavigationPropertyChanged("Parent");
                }
            }
        }
        private ProjectAttachmentFolder _parent;

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
            Folder = null;
            Attachments.Clear();
            Children.Clear();
            Parent = null;
        }

        #endregion

        #region Association Fixup
    
        private void FixupFolder(Project previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.AttachmentFolders.Contains(this))
            {
                previousValue.AttachmentFolders.Remove(this);
            }
    
            if (Folder != null)
            {
                if (!Folder.AttachmentFolders.Contains(this))
                {
                    Folder.AttachmentFolders.Add(this);
                }
    
                ProjectId = Folder.ProjectId;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("Folder")
                    && (ChangeTracker.OriginalValues["Folder"] == Folder))
                {
                    ChangeTracker.OriginalValues.Remove("Folder");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("Folder", previousValue);
                }
                if (Folder != null && !Folder.ChangeTracker.ChangeTrackingEnabled)
                {
                    Folder.StartTracking();
                }
            }
        }
    
        private void FixupParent(ProjectAttachmentFolder previousValue, bool skipKeys = false)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.Children.Contains(this))
            {
                previousValue.Children.Remove(this);
            }
    
            if (Parent != null)
            {
                if (!Parent.Children.Contains(this))
                {
                    Parent.Children.Add(this);
                }
    
                ParentProjectAttachmentFolderId = Parent.ProjectAttachmentFolderId;
            }
            else if (!skipKeys)
            {
                ParentProjectAttachmentFolderId = null;
            }
    
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("Parent")
                    && (ChangeTracker.OriginalValues["Parent"] == Parent))
                {
                    ChangeTracker.OriginalValues.Remove("Parent");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("Parent", previousValue);
                }
                if (Parent != null && !Parent.ChangeTracker.ChangeTrackingEnabled)
                {
                    Parent.StartTracking();
                }
            }
        }
    
        private void FixupAttachments(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (e.NewItems != null)
            {
                foreach (ProjectAttachment item in e.NewItems)
                {
                    item.ProjectAttachmentFolder = this;
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        if (!item.ChangeTracker.ChangeTrackingEnabled)
                        {
                            item.StartTracking();
                        }
                        ChangeTracker.RecordAdditionToCollectionProperties("Attachments", item);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (ProjectAttachment item in e.OldItems)
                {
                    if (ReferenceEquals(item.ProjectAttachmentFolder, this))
                    {
                        item.ProjectAttachmentFolder = null;
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        ChangeTracker.RecordRemovalFromCollectionProperties("Attachments", item);
                    }
                }
            }
        }
    
        private void FixupChildren(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (e.NewItems != null)
            {
                foreach (ProjectAttachmentFolder item in e.NewItems)
                {
                    item.Parent = this;
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        if (!item.ChangeTracker.ChangeTrackingEnabled)
                        {
                            item.StartTracking();
                        }
                        ChangeTracker.RecordAdditionToCollectionProperties("Children", item);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (ProjectAttachmentFolder item in e.OldItems)
                {
                    if (ReferenceEquals(item.Parent, this))
                    {
                        item.Parent = null;
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        ChangeTracker.RecordRemovalFromCollectionProperties("Children", item);
                    }
                }
            }
        }

        #endregion

    }
}
