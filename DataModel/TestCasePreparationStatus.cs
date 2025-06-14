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
    [KnownType(typeof(TestCase))]
    public partial class TestCasePreparationStatus: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public int TestCasePreparationStatusId
        {
            get { return _testCasePreparationStatusId; }
            set
            {
                if (_testCasePreparationStatusId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'TestCasePreparationStatusId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    _testCasePreparationStatusId = value;
                    OnPropertyChanged("TestCasePreparationStatusId");
                }
            }
        }
        private int _testCasePreparationStatusId;
    
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

        #endregion

        #region Navigation Properties
    
        [DataMember]
        public TrackableCollection<TestCase> TST_TEST_CASE
        {
            get
            {
                if (_tST_TEST_CASE == null)
                {
                    _tST_TEST_CASE = new TrackableCollection<TestCase>();
                    _tST_TEST_CASE.CollectionChanged += FixupTST_TEST_CASE;
                }
                return _tST_TEST_CASE;
            }
            set
            {
                if (!ReferenceEquals(_tST_TEST_CASE, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        throw new InvalidOperationException("Cannot set the FixupChangeTrackingCollection when ChangeTracking is enabled");
                    }
                    if (_tST_TEST_CASE != null)
                    {
                        _tST_TEST_CASE.CollectionChanged -= FixupTST_TEST_CASE;
                    }
                    _tST_TEST_CASE = value;
                    if (_tST_TEST_CASE != null)
                    {
                        _tST_TEST_CASE.CollectionChanged += FixupTST_TEST_CASE;
                    }
                    OnNavigationPropertyChanged("TST_TEST_CASE");
                }
            }
        }
        private TrackableCollection<TestCase> _tST_TEST_CASE;

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
            TST_TEST_CASE.Clear();
        }

        #endregion

        #region Association Fixup
    
        private void FixupTST_TEST_CASE(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (e.NewItems != null)
            {
                foreach (TestCase item in e.NewItems)
                {
                    item.TestCasePreparationStatus = this;
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        if (!item.ChangeTracker.ChangeTrackingEnabled)
                        {
                            item.StartTracking();
                        }
                        ChangeTracker.RecordAdditionToCollectionProperties("TST_TEST_CASE", item);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (TestCase item in e.OldItems)
                {
                    if (ReferenceEquals(item.TestCasePreparationStatus, this))
                    {
                        item.TestCasePreparationStatus = null;
                    }
                    if (ChangeTracker.ChangeTrackingEnabled)
                    {
                        ChangeTracker.RecordRemovalFromCollectionProperties("TST_TEST_CASE", item);
                    }
                }
            }
        }

        #endregion

    }
}
