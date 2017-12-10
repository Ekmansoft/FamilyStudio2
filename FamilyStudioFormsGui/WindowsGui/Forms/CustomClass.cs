using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace FamilyStudioFormsGui.WindowsGui.Forms
{
	/// <summary>
	/// CustomClass (Which is binding to property grid)
	/// </summary>
	public class CustomClass: CollectionBase,ICustomTypeDescriptor
	{
    private bool debugFlag = false;
		/// <summary>
		/// Add CustomProperty to Collectionbase List
		/// </summary>
		/// <param name="Value"></param>
		public void Add(CustomProperty Value)
		{
			base.List.Add(Value);
		}

		/// <summary>
		/// Remove item from List
		/// </summary>
		/// <param name="Name"></param>
		public void Remove(string Name)
		{
			foreach(CustomProperty prop in base.List)
			{
				if(prop.Name == Name)
				{
					base.List.Remove(prop);
					return;
				}
			}
		}

		/// <summary>
		/// Indexer
		/// </summary>
		public CustomProperty this[int index] 
		{
			get 
			{
        if (debugFlag)
        {
          Console.WriteLine("CustomProperty.get(" + index + " of " + List.Count + ")");
        }
        return (CustomProperty)base.List[index];
			}
			set
			{
        Console.WriteLine("CustomProperty.set(" + index + " of " + List.Count + ")");
        base.List[index] = (CustomProperty)value;
			}
		}


		#region "TypeDescriptor Implementation"
		/// <summary>
		/// Get Class Name
		/// </summary>
		/// <returns>String</returns>
		public String GetClassName()
		{
			return TypeDescriptor.GetClassName(this,true);
		}

		/// <summary>
		/// GetAttributes
		/// </summary>
		/// <returns>AttributeCollection</returns>
		public AttributeCollection GetAttributes()
		{
			return TypeDescriptor.GetAttributes(this,true);
		}

		/// <summary>
		/// GetComponentName
		/// </summary>
		/// <returns>String</returns>
		public String GetComponentName()
		{
			return TypeDescriptor.GetComponentName(this, true);
		}

		/// <summary>
		/// GetConverter
		/// </summary>
		/// <returns>TypeConverter</returns>
		public TypeConverter GetConverter()
		{
			return TypeDescriptor.GetConverter(this, true);
		}

		/// <summary>
		/// GetDefaultEvent
		/// </summary>
		/// <returns>EventDescriptor</returns>
		public EventDescriptor GetDefaultEvent() 
		{
			return TypeDescriptor.GetDefaultEvent(this, true);
		}

		/// <summary>
		/// GetDefaultProperty
		/// </summary>
		/// <returns>PropertyDescriptor</returns>
		public PropertyDescriptor GetDefaultProperty() 
		{
			return TypeDescriptor.GetDefaultProperty(this, true);
		}

		/// <summary>
		/// GetEditor
		/// </summary>
		/// <param name="editorBaseType">editorBaseType</param>
		/// <returns>object</returns>
		public object GetEditor(Type editorBaseType) 
		{
			return TypeDescriptor.GetEditor(this, editorBaseType, true);
		}

		public EventDescriptorCollection GetEvents(Attribute[] attributes) 
		{
			return TypeDescriptor.GetEvents(this, attributes, true);
		}

		public EventDescriptorCollection GetEvents()
		{
			return TypeDescriptor.GetEvents(this, true);
		}

		public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			PropertyDescriptor[] newProps = new PropertyDescriptor[this.Count];
			for (int i = 0; i < this.Count; i++)
			{
				CustomProperty  prop = (CustomProperty) this[i];
				newProps[i] = new CustomPropertyDescriptor(ref prop, attributes);
			}

			return new PropertyDescriptorCollection(newProps);
		}

		public PropertyDescriptorCollection GetProperties()
		{
			return TypeDescriptor.GetProperties(this, true);
		}

		public object GetPropertyOwner(PropertyDescriptor pd) 
		{
			return this;
		}
		#endregion
	
	}

	/// <summary>
	/// Custom property class 
	/// </summary>
	public class CustomProperty
	{
    private bool debugFlag = false;
		private string sName = string.Empty;
		private bool bReadOnly = false;
		private bool bVisible = true;
		private object objValue = null;
    private CustomProperty parent;

    public CustomProperty(ref CustomProperty parent, string sName, object value, Type type, bool bReadOnly, bool bVisible)
    {
      this.parent = parent;
      this.sName = sName;
      this.objValue = value;
      this.type = type;
      this.bReadOnly = bReadOnly;
      this.bVisible = bVisible;
    }
    public CustomProperty(string sName)
    {
      this.parent = null;
      this.sName = sName;
      this.objValue = null;
      this.type = typeof(CustomProperty);
      this.bReadOnly = true;
      this.bVisible = false;
    }

		private Type type;
		public Type Type
		{
			get { return type; }
		}

		public bool ReadOnly
		{
			get
			{
				return bReadOnly;
			}
		}

		public string Name
		{
			get
			{
				return sName;
			}
		}

		public string Category
		{
			get
			{
        if(parent == null)
        {
				  return string.Empty;
        }
        else
        {
          return parent.Name;
        }
			}
		}

		public bool Visible
		{
			get
			{
				return bVisible;
			}
		}

		public object Value
		{
			get
			{
        if (debugFlag)
        {
          if (objValue != null)
          {
            Console.WriteLine("CustomProperty.Value.get " + Name + ":" + type + ":" + objValue.ToString() + ":" + objValue.GetType().ToString());
          }
          else
          {
            Console.WriteLine("CustomProperty.Value.get " + Name + ":" + type + ":");
          }
        }
        /*if (type == typeof(DateTime))
        {
          DateTime date;

          if (objValue != null)
          {
            if (DateTime.TryParse((string)objValue, out date))
            {
              return objValue;
            }
          }
          return DateTime.Now.Date.ToString();
        }*/
        return objValue;
			}
			set
			{
        if (debugFlag)
        {
          if (value != null)
          {
            Console.WriteLine("CustomProperty.Value.set " + Name + ":" + type + ":" + value.ToString() + ":" + value.GetType().ToString());
          }
          else
          {
            Console.WriteLine("CustomProperty.Value.set " + Name + ":" + type);
          }
        }
        objValue = value;
			}
		}

	}


	/// <summary>
	/// Custom PropertyDescriptor
	/// </summary>
	public class CustomPropertyDescriptor: PropertyDescriptor
	{
		CustomProperty m_Property;
		public CustomPropertyDescriptor(ref CustomProperty myProperty, Attribute [] attrs) :base(myProperty.Name, attrs)
		{
			m_Property = myProperty;
		}

		#region PropertyDescriptor specific
		
		public override bool CanResetValue(object component)
		{
			return false;
		}

		public override Type ComponentType
		{
			get { return null; }
		}

		public override object GetValue(object component)
		{
			return m_Property.Value;
		}

		public override string Description
		{
			get { return m_Property.Name; }
		}
		
		public override string Category
		{
			get	{ return m_Property.Category; }
		}

		public override string DisplayName
		{
			get { return m_Property.Name; }
		}

		public override bool IsReadOnly
		{
			get { return m_Property.ReadOnly; }
		}

		public override void ResetValue(object component)
		{
			//Have to implement
		}

		public override bool ShouldSerializeValue(object component)
		{
			return false;
		}

		public override void SetValue(object component, object value)
		{
			m_Property.Value = value;
		}

		public override Type PropertyType
		{
			get { return m_Property.Type; }
		}

		#endregion

			
	}
}
