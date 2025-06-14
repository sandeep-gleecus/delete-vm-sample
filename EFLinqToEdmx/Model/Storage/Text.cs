using System.Collections.Generic;
using System.Xml.Linq;
using Xml.Schema.Linq;

namespace LinqToEdmx.Model.Storage
{
  /// <summary>
  /// <para>
  /// Regular expression: (any)
  /// </para>
  /// </summary>
  public class Text : XTypedElement, IXMetaData
  {
    private static FSM _validationStates;

    static Text()
    {
      InitFsm();
    }

    /// <summary>
    /// <para>
    /// Regular expression: (any)
    /// </para>
    /// </summary>
    public IEnumerable<XElement> Any
    {
      get
      {
        return GetWildCards(WildCard.DefaultWildCard);
      }
    }

    #region IXMetaData Members

    XName IXMetaData.SchemaName
    {
      get
      {
        return XName.Get("TText", "http://schemas.microsoft.com/ado/2009/11/edm/ssdl");
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

    public static explicit operator Text(XElement xe)
    {
      return XTypedServices.ToXTypedElement<Text>(xe, LinqToXsdTypeManager.Instance);
    }

    public override XTypedElement Clone()
    {
      return XTypedServices.CloneXTypedElement(this);
    }

    private static void InitFsm()
    {
      var transitions = new Dictionary<int, Transitions>();
      transitions.Add(1, new Transitions(new SingleTransition(new WildCard("##other", "http://schemas.microsoft.com/ado/2009/11/edm/ssdl"), 1)));
      _validationStates = new FSM(1, new Set<int>(1), transitions);
    }
  }
}