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
    [KnownType(typeof(User))]
    [KnownType(typeof(TestSet))]
    public partial class TestSetDiscussion: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public int DiscussionId
        {
            get { return _discussionId; }
            set
            {
                if (_discussionId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'DiscussionId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _discussionId = value;
                    OnPropertyChanged("DiscussionId");
                }
            }
        }
        private int _discussionId;
    
        [DataMember]
        public int ArtifactId
        {
            get { return _artifactId; }
            set
            {
                if (_artifactId != value)
                {
                    ChangeTracker.RecordOriginalValue("ArtifactId", _artifactId);
                    if (!IsDeserializing)
                    {
                        if (TestSet != null && TestSet.TestSetId != value)
                        {
                            TestSet = null;
                        }
                    }
                    _artifactId = value;
                    OnPropertyChanged("ArtifactId");
                }
            }
        }
        private int _artifactId;
    
        [DataMember]
        public int CreatorId
        {
            get { return _creatorId; }
            set
            {
                if (_creatorId != value)
                {
                    ChangeTracker.RecordOriginalValue("CreatorId", _creatorId);
                    if (!IsDeserializing)
                    {
                        if (Creator != null && Creator.UserId != value)
                        {
                            Creator = null;
                        }
                    }
                    _creatorId = value;
                    OnPropertyChanged("CreatorId");
                }
            }
        }
        private int _creatorId;
    
        [DataMember]
        public string Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    ChangeTracker.RecordOriginalValue("Text", _text);
                    _text = value;
                    OnPropertyChanged("Text");
                }
            }
        }
        private string _text;
    
        [DataMember]
        public System.DateTime CreationDate
        {
            get { return _creationDate; }
            set
            {
                if (_creationDate != value)
                {
                    ChangeTracker.RecordOriginalValue("CreationDate", _creationDate);
                    _creationDate = value;
                    OnPropertyChanged("CreationDate");
                }
            }
        }
        private System.DateTime _creationDate;
    
        [DataMember]
        public bool IsDeleted
        {
            get { return _isDeleted; }
            set
            {
                if (_isDeleted != value)
                {
                    ChangeTracker.RecordOriginalValue("IsDeleted", _isDeleted);
                    _isDeleted = value;
                    OnPropertyChanged("IsDeleted");
                }
            }
        }
        private bool _isDeleted;
    
        [DataMember]
        public bool IsPermanent
        {
            get { return _isPermanent; }
            set
            {
                if (_isPermanent != value)
                {
                    ChangeTracker.RecordOriginalValue("IsPermanent", _isPermanent);
                    _isPermanent = value;
                    OnPropertyChanged("IsPermanent");
                }
            }
        }
        private bool _isPermanent;

        #endregion

        #region Navigation Properties
    
        [DataMember]
        public User Creator
        {
            get { return _creator; }
            set
            {
                if (!ReferenceEquals(_creator, value))
                {
                    var previousValue = _creator;
                    _creator = value;
                    FixupCreator(previousValue);
                    OnNavigationPropertyChanged("Creator");
                }
            }
        }
        private User _creator;
    
        [DataMember]
        public TestSet TestSet
        {
            get { return _testSet; }
            set
            {
                if (!ReferenceEquals(_testSet, value))
                {
                    var previousValue = _testSet;
                    _testSet = value;
                    FixupTestSet(previousValue);
                    OnNavigationPropertyChanged("TestSet");
                }
            }
        }
        private TestSet _testSet;

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
            Creator = null;
            TestSet = null;
        }

        #endregion

        #region Association Fixup
    
        private void FixupCreator(User previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.TestSetDiscussions.Contains(this))
            {
                previousValue.TestSetDiscussions.Remove(this);
            }
    
            if (Creator != null)
            {
                if (!Creator.TestSetDiscussions.Contains(this))
                {
                    Creator.TestSetDiscussions.Add(this);
                }
    
                CreatorId = Creator.UserId;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("Creator")
                    && (ChangeTracker.OriginalValues["Creator"] == Creator))
                {
                    ChangeTracker.OriginalValues.Remove("Creator");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("Creator", previousValue);
                }
                if (Creator != null && !Creator.ChangeTracker.ChangeTrackingEnabled)
                {
                    Creator.StartTracking();
                }
            }
        }
    
        private void FixupTestSet(TestSet previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.Discussions.Contains(this))
            {
                previousValue.Discussions.Remove(this);
            }
    
            if (TestSet != null)
            {
                if (!TestSet.Discussions.Contains(this))
                {
                    TestSet.Discussions.Add(this);
                }
    
                ArtifactId = TestSet.TestSetId;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("TestSet")
                    && (ChangeTracker.OriginalValues["TestSet"] == TestSet))
                {
                    ChangeTracker.OriginalValues.Remove("TestSet");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("TestSet", previousValue);
                }
                if (TestSet != null && !TestSet.ChangeTracker.ChangeTrackingEnabled)
                {
                    TestSet.StartTracking();
                }
            }
        }

        #endregion

    }
}
