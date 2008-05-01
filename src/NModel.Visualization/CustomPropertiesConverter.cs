//-------------------------------------
// Copyright (c) Microsoft Corporation
//-------------------------------------
using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.Collections;

namespace NModel.Visualization
{
    /// <summary>
    /// Interface that is used by the property grid for customized viewing of values
    /// </summary>
  [TypeConverter(typeof(CustomPropertiesConverter))]
  public interface ICustomPropertyConverter
  {
      /// <summary>
      /// Returns the keys of elements to be viewed.
      /// </summary>
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
      ICollection GetKeys();
      /// <summary>
      /// The value corresponding to the key
      /// </summary>
    object ValueOf(object key);
      /// <summary>
      /// The display name of the key
      /// </summary>
    string DisplayNameOf(object key);
      /// <summary>
      /// Help text associated with the key
      /// </summary>
    string DescriptionOf(object key);
      /// <summary>
      /// Whether the value can be changed or not.
      /// </summary>
    bool IsReadOnly(object key);
      /// <summary>
      /// The category of the key 
      /// </summary>
    string CategoryOf(object key);
      /// <summary>
      /// Change the value of the key
      /// </summary>
    void SetValue(object key, object value);
      /// <summary>
      /// Name of a custom editor for the given key
      /// </summary>
    string CustomEditorOf(object key);
      /// <summary>
      /// Image associated with the key
      /// </summary>
    object ImageOf(object key);
      /// <summary>
      /// Event handler associated with the key
      /// </summary>
    EventHandler HandlerOf(object key);
      /// <summary>
      /// Tooltip for the key
      /// </summary>
    string TooltipOf(object key);
      /// <summary>
      /// Context menu for the key
      /// </summary>
    object ContextMenuOf(object key);
      /// <summary>
      /// Whether the value of the key should be expanded by default
      /// </summary>
    bool IsDefaultExpanded(object key);
  }

  /// <summary>
  /// A default implementation of ICustomPropertyConverter. It requires the user only to
  /// implement GetKeys and ValueOf.
  /// </summary>
  internal abstract class DefaultCustomPropertyConverter : ICustomPropertyConverter
  {
    #region ICustomPropertyConverter Members

      public abstract ICollection GetKeys();
    public abstract object ValueOf(object key);

      public virtual string DisplayNameOf(object key)
      {
          if (key == null)
              return "";
          else
              return key.ToString();
      }

      public virtual string DescriptionOf(object key)
      {
          return DisplayNameOf(key);
      }

    public virtual bool IsReadOnly(object key)
    {
      return false;
    }

    public virtual string CategoryOf(object key)
    {
      return "";
    }

    public virtual void SetValue(object key, object value)
    {
    }

    public virtual string CustomEditorOf(object key)
    {
      return null;
    }

    public virtual object ImageOf(object key)
    {
      return null;
    }

    public virtual System.EventHandler HandlerOf(object key)
    {
      return null;
    }

    public virtual string TooltipOf(object key)
    {
      return null;
    }

    public virtual object ContextMenuOf(object key)
    {
      return null;
    }

    public virtual bool IsDefaultExpanded(object key)
    {
      return false;
    }

    #endregion

  }


  internal class CustomPropertiesConverter : ExpandableObjectConverter
  {
    public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
    {
      PropertyDescriptorCollection ps = base.GetProperties(context, value, attributes);
      ICustomPropertyConverter s = value as ICustomPropertyConverter;
      if (s == null) throw new InvalidOperationException();
      //keep given properties and add one for each key
      PropertyDescriptor[] props = new PropertyDescriptor[ps.Count + s.GetKeys().Count];
      int i = 0;
      foreach (PropertyDescriptor p in ps) props[i++] = p;
      foreach (object key in s.GetKeys())
      { 
        //create attributes for s
        ArrayList attrs = new ArrayList();
        attrs.Add(new DescriptionAttribute(s.DescriptionOf(key)));
        attrs.Add(new ReadOnlyAttribute(s.IsReadOnly(key)));
        attrs.Add(new CategoryAttribute(s.CategoryOf(key)));
        attrs.Add(RefreshPropertiesAttribute.All);
        string customEditor = s.CustomEditorOf(key);
        if (customEditor != null)
          attrs.Add(new EditorAttribute(customEditor,typeof(System.Drawing.Design.UITypeEditor)));
        object v = s.ValueOf(key);
        props[i++] = new KeyDescriptor(s.GetType(), key, s.DisplayNameOf(key), (v==null ? "":v).GetType(), 
          (Attribute[])attrs.ToArray(typeof(Attribute)),
          s.IsDefaultExpanded(key));
      }
      PropertyDescriptorCollection res = new PropertyDescriptorCollection(props);
      return res;
    }
  
    private class KeyDescriptor : 
      System.ComponentModel.TypeConverter.SimplePropertyDescriptor,
      IKeyDescriptor
    {
      private object key;
      private bool defaultExpanded;

      //private AttributeCollection attrs;
      public override void SetValue(object x, object value)
      {
        ICustomPropertyConverter s = x as ICustomPropertyConverter;
        s.SetValue(key, value);
      }
      public override object GetValue(object x)
      {
        ICustomPropertyConverter s = x as ICustomPropertyConverter;
        return s.ValueOf(key);
      }

      public KeyDescriptor(Type compType, object key, string keyName, Type propType, Attribute[] atts,
                           bool defaultExpanded) : base(compType, keyName, propType,atts)
      {
        this.key = key;
        this.defaultExpanded = defaultExpanded;
      }

      public object Key 
      {
        get
        {
          return key;
        }
      }

      public bool DefaultExpanded 
      { 
        get 
        {
          return defaultExpanded;
        }
      }
    }
  }

  internal interface IKeyDescriptor
  {
    object Key { get; }
    bool DefaultExpanded { get; }
  }

}
