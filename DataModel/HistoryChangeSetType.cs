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
    [KnownType(typeof(HistoryChangeSet))]
    [KnownType(typeof(GlobalHistoryChangeset))]
    [KnownType(typeof(TST_ADMIN_HISTORY_CHANGESET))]
    [KnownType(typeof(TST_USER_HISTORY_CHANGESET))]
    public partial class HistoryChangeSetType: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public int ChangeTypeId
        {
            get { return _changeTypeId; }
            set
            {
                if (_changeTypeId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'ChangeTypeId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _changeTypeId = value;
                    OnPropertyChanged("ChangeTypeId");
                }
            }
        }
        private int _changeTypeId;
    
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
        public TrackableCollection<HistoryChangeSet> ChangeSets
        {
            get
            {
                if (_changeSets == null)
                {
                    _changeSets = new TrackableCollection<HistoryChangeSet>();
                    _changeSets.CollectionChanged += FixupChangeSets;
                }
                return _changeSets;
            }
            set
            {
                if (!ReferenceEquals(_changeSets, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        throw new InvalidOperationException("Cannot set the FixupChangeTrackingCollection when ChangeTracking is enabled");
                    }
                    if (_changeSets != null)
                    {
                        _changeSets.CollectionChanged -= FixupChangeSets;
                    }
                    _changeSets = value;
                    if (_changeSets != null)
                    {
                        _changeSets.CollectionChanged += FixupChangeSets;
                    }
                    OnNavigationPropertyChanged("ChangeSets");
                }
            }
        }
        private TrackableCollection<HistoryChangeSet> _changeSets;
    
        [DataMember]
        public TrackableCollection<GlobalHistoryChangeset> TST_GLOBAL_HISTORY_CHANGESET
        {
            get
            {
                if (_tST_GLOBAL_HISTORY_CHANGESET == null)
                {
                    _tST_GLOBAL_HISTORY_CHANGESET = new TrackableCollection<GlobalHistoryChangeset>();
                    _tST_GLOBAL_HISTORY_CHANGESET.CollectionChanged += FixupTST_GLOBAL_HISTORY_CHANGESET;
                }
                return _tST_GLOBAL_HISTORY_CHANGESET;
            }
            set
            {
                if (!ReferenceEquals(_tST_GLOBAL_HISTORY_CHANGESET, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        throw new InvalidOperationException("Cannot set the FixupChangeTrackingCollection when ChangeTracking is enabled");
                    }
                    if (_tST_GLOBAL_HISTORY_CHANGESET != null)
                    {
                        _tST_GLOBAL_HISTORY_CHANGESET.CollectionChanged -= FixupTST_GLOBAL_HISTORY_CHANGESET;
                    }
                    _tST_GLOBAL_HISTORY_CHANGESET = value;
                    if (_tST_GLOBAL_HISTORY_CHANGESET != null)
                    {
                        _tST_GLOBAL_HISTORY_CHANGESET.CollectionChanged += FixupTST_GLOBAL_HISTORY_CHANGESET;
                    }
                    OnNavigationPropertyChanged("TST_GLOBAL_HISTORY_CHANGESET");
                }
            }
        }
        private TrackableCollection<GlobalHistoryChangeset> _tST_GLOBAL_HISTORY_CHANGESET;
    
        [DataMember]
        public TrackableCollection<TST_ADMIN_HISTORY_CHANGESET> TST_ADMIN_HISTORY_CHANGESET
        {
            get
            {
                if (_tST_ADMIN_HISTORY_CHANGESET == null)
                {
                    _tST_ADMIN_HISTORY_CHANGESET = new TrackableCollection<TST_ADMIN_HISTORY_CHANGESET>();
                    _tST_ADMIN_HISTORY_CHANGESET.CollectionChanged += FixupTST_ADMIN_HISTORY_CHANGESET;
                }
                return _tST_ADMIN_HISTORY_CHANGESET;
            }
            set
            {
                if (!ReferenceEquals(_tST_ADMIN_HISTORY_CHANGESET, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        throw new InvalidOperationException("Cannot set the FixupChangeTrackingCollection when ChangeTracking is enabled");
                    }
                    if (_tST_ADMIN_HISTORY_CHANGESET != null)
                    {
                        _tST_ADMIN_HISTORY_CHANGESET.CollectionChanged -= FixupTST_ADMIN_HISTORY_CHANGESET;
                    }
                    _tST_ADMIN_HISTORY_CHANGESET = value;
                    if (_tST_ADMIN_HISTORY_CHANGESET != null)
                    {
                        _tST_ADMIN_HISTORY_CHANGESET.CollectionChanged += FixupTST_ADMIN_HISTORY_CHANGESET;
                    }
                    OnNavigationPropertyChanged("TST_ADMIN_HISTORY_CHANGESET");
                }
            }
        }
        private TrackableCollection<TST_ADMIN_HISTORY_CHANGESET> _tST_ADMIN_HISTORY_CHANGESET;
    
        [DataMember]
        public TrackableCollection<TST_USER_HISTORY_CHANGESET> TST_USER_HISTORY_CHANGESET
        {
            get
            {
                if (_tST_USER_HISTORY_CHANGESET == null)
                {
                    _tST_USER_HISTORY_CHANGESET = new TrackableCollection<TST_USER_HISTORY_CHANGESET>();
                    _tST_USER_HISTORY_CHANGESET.CollectionChanged += FixupTST_USER_HISTORY_CHANGESET;
                }
                return _tST_USER_HISTORY_CHANGESET;
            }
            set
            {
                if (!ReferenceEquals(_tST_USER_HISTORY_CHANGESET, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        throw new InvalidOperationException("Cannot set the FixupChangeTrackingCollection when ChangeTracking is enabled");
                    }
                    if (_tST_USER_HISTORY_CHANGESET != null)
                    {
                        _tST_USER_HISTORY_CHANGESET.CollectionChanged -= FixupTST_USER_HISTORY_CHANGESET;
                    }
                    _tST_USER_HISTORY_CHANGESET = value;
                    if (_tST_USER_HISTORY_CHANGESET != null)
                    {
                        _tST_USER_HISTORY_CHANGESET.CollectionChanged += FixupTST_USER_HISTORY_CHANGESET;
                    }
                    OnNavigationPropertyChanged("TST_USER_HISTORY_CHANGESET");
                }
            }
        }
        private TrackableCollection<TST_USER_HISTORY_CHANGESET> _tST_USER_HISTORY_CHANGESET;

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
            ChangeSets.Clear();
            TST_GLOBAL_HISTORY_CHANGESET.Clear();
            TST_ADMIN_HISTORY_CHANGESET.Clear();
            TST_USER_HISTORY_CHANGESET.Clear();
        }

        #endregion

        #region Association Fixup
    
        private void FixupChangeSets(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (e.NewItems != null)
            {
                foreach (HistoryChangeSet item in e.NewItems)
                {
                    item.Type = this;
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        if (!item.ChangeTracker.ChangeTrackingEnabled)
                        {
                            item.StartTracking();
                        }
                        ChangeTracker.RecordAdditionToCollectionProperties("ChangeSets", item);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (HistoryChangeSet item in e.OldItems)
                {
                    if (ReferenceEquals(item.Type, this))
                    {
                        item.Type = null;
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        ChangeTracker.RecordRemovalFromCollectionProperties("ChangeSets", item);
                    }
                }
            }
        }
    
        private void FixupTST_GLOBAL_HISTORY_CHANGESET(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (e.NewItems != null)
            {
                foreach (GlobalHistoryChangeset item in e.NewItems)
                {
                    item.ChangeType = this;
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        if (!item.ChangeTracker.ChangeTrackingEnabled)
                        {
                            item.StartTracking();
                        }
                        ChangeTracker.RecordAdditionToCollectionProperties("TST_GLOBAL_HISTORY_CHANGESET", item);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (GlobalHistoryChangeset item in e.OldItems)
                {
                    if (ReferenceEquals(item.ChangeType, this))
                    {
                        item.ChangeType = null;
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        ChangeTracker.RecordRemovalFromCollectionProperties("TST_GLOBAL_HISTORY_CHANGESET", item);
                    }
                }
            }
        }
    
        private void FixupTST_ADMIN_HISTORY_CHANGESET(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (e.NewItems != null)
            {
                foreach (TST_ADMIN_HISTORY_CHANGESET item in e.NewItems)
                {
                    item.TST_HISTORY_CHANGESET_TYPE = this;
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        if (!item.ChangeTracker.ChangeTrackingEnabled)
                        {
                            item.StartTracking();
                        }
                        ChangeTracker.RecordAdditionToCollectionProperties("TST_ADMIN_HISTORY_CHANGESET", item);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (TST_ADMIN_HISTORY_CHANGESET item in e.OldItems)
                {
                    if (ReferenceEquals(item.TST_HISTORY_CHANGESET_TYPE, this))
                    {
                        item.TST_HISTORY_CHANGESET_TYPE = null;
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        ChangeTracker.RecordRemovalFromCollectionProperties("TST_ADMIN_HISTORY_CHANGESET", item);
                    }
                }
            }
        }
    
        private void FixupTST_USER_HISTORY_CHANGESET(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (e.NewItems != null)
            {
                foreach (TST_USER_HISTORY_CHANGESET item in e.NewItems)
                {
                    item.TST_HISTORY_CHANGESET_TYPE = this;
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        if (!item.ChangeTracker.ChangeTrackingEnabled)
                        {
                            item.StartTracking();
                        }
                        ChangeTracker.RecordAdditionToCollectionProperties("TST_USER_HISTORY_CHANGESET", item);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (TST_USER_HISTORY_CHANGESET item in e.OldItems)
                {
                    if (ReferenceEquals(item.TST_HISTORY_CHANGESET_TYPE, this))
                    {
                        item.TST_HISTORY_CHANGESET_TYPE = null;
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        ChangeTracker.RecordRemovalFromCollectionProperties("TST_USER_HISTORY_CHANGESET", item);
                    }
                }
            }
        }

        #endregion

    }
}
