using System;
using System.Collections.Generic;
using UnityEngine;
// ReSharper disable FieldCanBeMadeReadOnly.Local
//  ^ Serialization fields cannot be made readonly; causes problems

public enum ATTRIBUTES
{
	VITALITY,
	AGILITY,
	AFFINITY,
}

[CreateAssetMenu(fileName = "New Character Attributes", menuName = "Scriptable Objects/Character Attributes")]
public class Character_Attributes : ScriptableObject, ISerializationCallbackReceiver
{
	[Serializable]
	public class Char_Attr : ScriptableObject
	{
		public ATTRIBUTES Attribute_Type;
		public int Base_Value, Current_Value;

		public static Char_Attr Create(Character_Attribute_Data data)
		{
			Char_Attr new_attr = CreateInstance<Char_Attr>();
			new_attr.Attribute_Type = (ATTRIBUTES) data.Attribute_Type;
			new_attr.Base_Value = data.Base_Value;
			new_attr.Current_Value = data.Current_Value;
			return new_attr;
		}
	}
	
	[SerializeField] private int DEFAULT_ATTRIBUTE_VALUE = 5;
	
	#region Private Fields
	private Dictionary<ATTRIBUTES, Char_Attr> attributes = new Dictionary<ATTRIBUTES, Char_Attr>();
	#endregion
	
	#region Serialization Fields
	[HideInInspector, SerializeField] private List<ATTRIBUTES> _ser_attributes_keys = new List<ATTRIBUTES>();
	[HideInInspector, SerializeField] private List<int> _ser_attributes_base_values = new List<int>();
	#endregion Serialization Fields
	
	#region Properties
	public Dictionary<ATTRIBUTES, Char_Attr> Attributes
		=> new Dictionary<ATTRIBUTES, Char_Attr>(this.attributes);
	#endregion Properties

	private void OnEnable()
	{
		// Cannot call CreateInstance in Deserialization callback, so complete here instead
		this.CompleteDeserialization();
	}

	private void CompleteDeserialization()
	{
		// Pull remaining values out of serialized lists
		for (int i = 0; i < this._ser_attributes_keys.Count; i++) {
			ATTRIBUTES attribute = this._ser_attributes_keys[i];
			if ((int) attribute >= this._ser_attributes_base_values.Count) break;
			int value = this._ser_attributes_base_values[i];
			
			Char_Attr deserialized_attribute = CreateInstance<Char_Attr>();
			deserialized_attribute.Attribute_Type = attribute;
			deserialized_attribute.Base_Value = value;
			deserialized_attribute.Current_Value = value;
			this.attributes.Add(attribute, deserialized_attribute);
		}
		
		// Fill in dictionary with default values for any ATTRIBUTES that weren't loaded
		foreach (ATTRIBUTES attribute in Enum.GetValues(typeof(ATTRIBUTES))) {
			if (this.attributes.ContainsKey(attribute)) continue;
			
			Char_Attr default_attribute = CreateInstance<Char_Attr>();
			default_attribute.Attribute_Type = attribute;
			default_attribute.Base_Value = this.DEFAULT_ATTRIBUTE_VALUE;
			default_attribute.Current_Value = this.DEFAULT_ATTRIBUTE_VALUE;
			this.attributes.Add(attribute, default_attribute);
		}
	}

	public Char_Attr Get_Attribute(ATTRIBUTES attribute_type)
	{
		return this.attributes[attribute_type];
	}

	public static Character_Attributes Create(List<Character_Attribute_Data> attributes)
	{
		Character_Attributes atts = CreateInstance<Character_Attributes>();
		
		foreach (Character_Attribute_Data attribute in attributes) {
			Char_Attr new_attr = CreateInstance<Char_Attr>();
			new_attr.Attribute_Type = (ATTRIBUTES) attribute.Attribute_Type;
			new_attr.Base_Value = attribute.Base_Value;
			new_attr.Current_Value = attribute.Current_Value;
			atts.attributes[attribute.Attribute_Enum] = new_attr;
		}

		return atts;
	}

	public void Reinit(List<Character_Attribute_Data> new_attributes)
	{
		foreach (Character_Attribute_Data attribute in new_attributes) {
			if (this.attributes.ContainsKey(attribute.Attribute_Enum)) {
				this.attributes[attribute.Attribute_Enum] = Char_Attr.Create(attribute);
			}
			else {
				this.attributes.Add(attribute.Attribute_Enum, Char_Attr.Create(attribute));
			}
		}
	}

	public void OnBeforeSerialize()
	{
		this._ser_attributes_keys.Clear();
		this._ser_attributes_base_values.Clear();

		foreach (KeyValuePair<ATTRIBUTES, Char_Attr> kvp in this.attributes) {
			this._ser_attributes_keys.Add(kvp.Key);
			this._ser_attributes_base_values.Add(kvp.Value.Base_Value);
		}
	}
	
	public void OnAfterDeserialize()
	{
		this.attributes = new Dictionary<ATTRIBUTES, Char_Attr>();
	}
}
