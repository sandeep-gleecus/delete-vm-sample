using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Schema;
using LinqToEdmx.Model.Storage;
using Xml.Schema.Linq;

namespace LinqToEdmx.Model.Conceptual
{
  /// <summary>
  /// <para>
  /// Regular expression: ((Documentation?, any)?)
  /// </para>
  /// </summary>
  public class ReferenceType : XTypedElement, IXMetaData
  {
    private static readonly Dictionary<XName, Type> LocalElementDictionary = new Dictionary<XName, Type>();

    private static FSM _validationStates;

    static ReferenceType()
    {
      BuildElementDictionary();
      InitFsm();
    }

    /// <summary>
    /// <para>
    /// Occurrence: optional
    /// </para>
    /// <para>
    /// Setter: Appends
    /// </para>
    /// <para>
    /// Regular expression: ((Documentation?, any)?)
    /// </para>
    /// </summary>
    public Documentation Documentation
    {
      get
      {
        var x = GetElement(XName.Get("Documentation", "http://schemas.microsoft.com/ado/2009/11/edm"));
        return ((Documentation) (x));
      }
      set
      {
        SetElement(XName.Get("Documentation", "http://schemas.microsoft.com/ado/2009/11/edm"), value);
      }
    }

    /// <summary>
    /// <para>
    /// Regular expression: ((Documentation?, any)?)
    /// </para>
    /// </summary>
    public IEnumerable<XElement> Any
    {
      get
      {
        return GetWildCards(WildCard.DefaultWildCard);
      }
    }

    /// <summary>
    /// <para>
    /// Occurrence: required
    /// </para>
    /// </summary>
    public object Type
    {
      get
      {
        var x = Attribute(XName.Get("Type", ""));
        return XTypedServices.ParseValue<object>(x, XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.AnyAtomicType).Datatype);
      }
      set
      {
        SetAttribute(XName.Get("Type", ""), value, XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.AnyAtomicType).Datatype);
      }
    }

    #region IXMetaData Members

    Dictionary<XName, Type> IXMetaData.LocalElementsDictionary
    {
      get
      {
        return LocalElementDictionary;
      }
    }

    XName IXMetaData.SchemaName
    {
      get
      {
        return XName.Get("TReferenceType", "http://schemas.microsoft.com/ado/2009/11/edm");
      }
    }

    SchemaOrigin IXMetaData.TypeOrigin
    {
      get
      {
        return SchemaOrigin.Fragment;
      }
    }

    ILinqToXsdTypeManager IXMetaData.TypeManager
    {
      get
      {
        return LinqToXsdTypeManager.Instance;
      }
    }

    FSM IXMetaData.GetValidationStates()
    {
      return _validationStates;
    }

    #endregion

    public static explicit operator ReferenceType(XElement xe)
    {
      return XTypedServices.ToXTypedElement<ReferenceType>(xe, LinqToXsdTypeManager.Instance);
    }

    public override XTypedElement Clone()
    {
      return XTypedServices.CloneXTypedElement(this);
    }

    private static void BuildElementDictionary()
    {
      LocalElementDictionary.Add(XName.Get("Documentation", "http://schemas.microsoft.com/ado/2009/11/edm"), typeof (Documentation));
    }

    private static void InitFsm()
    {
      var transitions = new Dictionary<int, Transitions>();
      transitions.Add(1, new Transitions(new SingleTransition(XName.Get("Documentation", "http://schemas.microsoft.com/ado/2009/11/edm"), 2), new SingleTransition(new WildCard("##other", "http://schemas.microsoft.com/ado/2009/11/edm"), 3)));
      transitions.Add(2, new Transitions(new SingleTransition(new WildCard("##other", "http://schemas.microsoft.com/ado/2009/11/edm"), 2)));
      transitions.Add(3, new Transitions(new SingleTransition(new WildCard("##other", "http://schemas.microsoft.com/ado/2009/11/edm"), 3)));
      _validationStates = new FSM(1, new Set<int>(new[]
                                                   {
                                                     2, 1, 3
                                                   }), transitions);
    }
  }
}