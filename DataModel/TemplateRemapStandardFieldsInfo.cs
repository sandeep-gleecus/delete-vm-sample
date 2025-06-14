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
    
    public partial class TemplateRemapStandardFieldsInfo : INotifyComplexPropertyChanging, INotifyPropertyChanged
    {
        #region Primitive Properties
    
        [DataMember]
        public string ArtifactType
        {
            get { return _artifactType; }
            set
            {
                if (_artifactType != value)
                {
                    OnComplexPropertyChanging();
                    _artifactType = value;
                    OnPropertyChanged("ArtifactType");
                }
            }
        }
        private string _artifactType;
    
        [DataMember]
        public string ArtifactField
        {
            get { return _artifactField; }
            set
            {
                if (_artifactField != value)
                {
                    OnComplexPropertyChanging();
                    _artifactField = value;
                    OnPropertyChanged("ArtifactField");
                }
            }
        }
        private string _artifactField;
    
        [DataMember]
        public Nullable<int> AffectedItemsCount
        {
            get { return _affectedItemsCount; }
            set
            {
                if (_affectedItemsCount != value)
                {
                    OnComplexPropertyChanging();
                    _affectedItemsCount = value;
                    OnPropertyChanged("AffectedItemsCount");
                }
            }
        }
        private Nullable<int> _affectedItemsCount;

        #endregion

        #region ChangeTracking
    
        private void OnComplexPropertyChanging()
        {
            if (_complexPropertyChanging != null)
            {
                _complexPropertyChanging(this, new EventArgs());
            }
        }
    
        event EventHandler INotifyComplexPropertyChanging.ComplexPropertyChanging { add { _complexPropertyChanging += value; } remove { _complexPropertyChanging -= value; } }
        private event EventHandler _complexPropertyChanging;
    
        private void OnPropertyChanged(String propertyName)
        {
            if (_propertyChanged != null)
            {
                _propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged { add { _propertyChanged += value; } remove { _propertyChanged -= value; } }
        private event PropertyChangedEventHandler _propertyChanged;
    
        public static void RecordComplexOriginalValues(String parentPropertyName, TemplateRemapStandardFieldsInfo complexObject, ObjectChangeTracker changeTracker)
        {
            if (String.IsNullOrEmpty(parentPropertyName))
            {
                throw new ArgumentException("String parameter cannot be null or empty.", "parentPropertyName");
            }
    
            if (changeTracker == null)
            {
                throw new ArgumentNullException("changeTracker");
            }
            changeTracker.RecordOriginalValue(String.Format(CultureInfo.InvariantCulture, "{0}.ArtifactType", parentPropertyName), complexObject == null ? null : (object)complexObject.ArtifactType);
            changeTracker.RecordOriginalValue(String.Format(CultureInfo.InvariantCulture, "{0}.ArtifactField", parentPropertyName), complexObject == null ? null : (object)complexObject.ArtifactField);
            changeTracker.RecordOriginalValue(String.Format(CultureInfo.InvariantCulture, "{0}.AffectedItemsCount", parentPropertyName), complexObject == null ? null : (object)complexObject.AffectedItemsCount);
        }

        #endregion

    }
}
