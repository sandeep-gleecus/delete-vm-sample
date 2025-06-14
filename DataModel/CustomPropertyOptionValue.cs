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
    [KnownType(typeof(CustomProperty))]
    [KnownType(typeof(CustomPropertyOption))]
    public partial class CustomPropertyOptionValue: IObjectWithChangeTracker, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public int CustomPropertyOptionId
        {
            get { return _customPropertyOptionId; }
            set
            {
                if (_customPropertyOptionId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'CustomPropertyOptionId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    if (!IsDeserializing)
                    {
                        if (CustomPropertyOption != null && CustomPropertyOption.CustomPropertyOptionId != value)
                        {
                            CustomPropertyOption = null;
                        }
                    }
                    _customPropertyOptionId = value;
                    OnPropertyChanged("CustomPropertyOptionId");
                }
            }
        }
        private int _customPropertyOptionId;
    
        [DataMember]
        public int CustomPropertyId
        {
            get { return _customPropertyId; }
            set
            {
                if (_customPropertyId != value)
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added)
                    {
                        throw new InvalidOperationException("The property 'CustomPropertyId' is part of the object's key and cannot be changed. Changes to key properties can only be made when the object is not being tracked or is in the Added state.");
                    }
                    if (!IsDeserializing)
                    {
                        if (CustomProperty != null && CustomProperty.CustomPropertyId != value)
                        {
                            CustomProperty = null;
                        }
                    }
                    _customPropertyId = value;
                    OnPropertyChanged("CustomPropertyId");
                }
            }
        }
        private int _customPropertyId;
    
        [DataMember]
        public string Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    ChangeTracker.RecordOriginalValue("Value", _value);
                    _value = value;
                    OnPropertyChanged("Value");
                }
            }
        }
        private string _value;

        #endregion

        #region Navigation Properties
    
        [DataMember]
        public CustomProperty CustomProperty
        {
            get { return _customProperty; }
            set
            {
                if (!ReferenceEquals(_customProperty, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added && value != null)
                    {
                        // This the dependent end of an identifying relationship, so the principal end cannot be changed if it is already set,
                        // otherwise it can only be set to an entity with a primary key that is the same value as the dependent's foreign key.
                        if (CustomPropertyId != value.CustomPropertyId)
                        {
                            throw new InvalidOperationException("The principal end of an identifying relationship can only be changed when the dependent end is in the Added state.");
                        }
                    }
                    var previousValue = _customProperty;
                    _customProperty = value;
                    FixupCustomProperty(previousValue);
                    OnNavigationPropertyChanged("CustomProperty");
                }
            }
        }
        private CustomProperty _customProperty;
    
        [DataMember]
        public CustomPropertyOption CustomPropertyOption
        {
            get { return _customPropertyOption; }
            set
            {
                if (!ReferenceEquals(_customPropertyOption, value))
                {
                    if (ChangeTracker.ChangeTrackingEnabled && ChangeTracker.State != ObjectState.Added && value != null)
                    {
                        // This the dependent end of an identifying relationship, so the principal end cannot be changed if it is already set,
                        // otherwise it can only be set to an entity with a primary key that is the same value as the dependent's foreign key.
                        if (CustomPropertyOptionId != value.CustomPropertyOptionId)
                        {
                            throw new InvalidOperationException("The principal end of an identifying relationship can only be changed when the dependent end is in the Added state.");
                        }
                    }
                    var previousValue = _customPropertyOption;
                    _customPropertyOption = value;
                    FixupCustomPropertyOption(previousValue);
                    OnNavigationPropertyChanged("CustomPropertyOption");
                }
            }
        }
        private CustomPropertyOption _customPropertyOption;

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
            CustomProperty = null;
            CustomPropertyOption = null;
        }

        #endregion

        #region Association Fixup
    
        private void FixupCustomProperty(CustomProperty previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.Options.Contains(this))
            {
                previousValue.Options.Remove(this);
            }
    
            if (CustomProperty != null)
            {
                if (!CustomProperty.Options.Contains(this))
                {
                    CustomProperty.Options.Add(this);
                }
    
                CustomPropertyId = CustomProperty.CustomPropertyId;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("CustomProperty")
                    && (ChangeTracker.OriginalValues["CustomProperty"] == CustomProperty))
                {
                    ChangeTracker.OriginalValues.Remove("CustomProperty");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("CustomProperty", previousValue);
                }
                if (CustomProperty != null && !CustomProperty.ChangeTracker.ChangeTrackingEnabled)
                {
                    CustomProperty.StartTracking();
                }
            }
        }
    
        private void FixupCustomPropertyOption(CustomPropertyOption previousValue)
        {
            if (IsDeserializing)
            {
                return;
            }
    
            if (previousValue != null && previousValue.Values.Contains(this))
            {
                previousValue.Values.Remove(this);
            }
    
            if (CustomPropertyOption != null)
            {
                if (!CustomPropertyOption.Values.Contains(this))
                {
                    CustomPropertyOption.Values.Add(this);
                }
    
                CustomPropertyOptionId = CustomPropertyOption.CustomPropertyOptionId;
            }
            if (ChangeTracker.ChangeTrackingEnabled)
            {
                if (ChangeTracker.OriginalValues.ContainsKey("CustomPropertyOption")
                    && (ChangeTracker.OriginalValues["CustomPropertyOption"] == CustomPropertyOption))
                {
                    ChangeTracker.OriginalValues.Remove("CustomPropertyOption");
                }
                else
                {
                    ChangeTracker.RecordOriginalValue("CustomPropertyOption", previousValue);
                }
                if (CustomPropertyOption != null && !CustomPropertyOption.ChangeTracker.ChangeTrackingEnabled)
                {
                    CustomPropertyOption.StartTracking();
                }
            }
        }

        #endregion

    }
}
