using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Resistances", menuName = "Scriptable Objects/Resistances")]
public class Character_Resistances : ScriptableObject, ISerializationCallbackReceiver
{
	#region Constants
	private const int DEFAULT_RESISTANCE_VALUE = 0;
	#endregion Constants
	
	#region Private Fields
	private Dictionary<DAMAGE_TYPES, Resistance> resistances = new Dictionary<DAMAGE_TYPES, Resistance>();
	#endregion Private Fields

	#region Serialization Fields
	[HideInInspector, SerializeField] private List<DAMAGE_TYPES> _ser_resistance_keys = new List<DAMAGE_TYPES>();
	[HideInInspector, SerializeField] private List<Resistance> _ser_resistance_values = new List<Resistance>();
	#endregion Serialization Fields
	
	private void OnEnable()
	{
		foreach (DAMAGE_TYPES resistance_type in Enum.GetValues(typeof(DAMAGE_TYPES))) {
			Resistance default_resistance = CreateInstance<Resistance>();
			default_resistance.Resistance_Type = resistance_type;
			default_resistance.Base_Value = DEFAULT_RESISTANCE_VALUE;
			default_resistance.Current_Value = DEFAULT_RESISTANCE_VALUE;

			if (this.resistances.ContainsKey(resistance_type)) {
				this.resistances[resistance_type] = default_resistance;
			}
			else {
				this.resistances.Add(resistance_type, default_resistance);
			}
		}
	}

	public static Character_Resistances Create(List<Resistance> resistances)
	{
		Character_Resistances new_resistances = CreateInstance<Character_Resistances>();

		foreach (Resistance resistance in resistances) {
			new_resistances.resistances.Add(resistance.Resistance_Type, resistance);
		}

		return new_resistances;
	}

	public Resistance Get_Resistance(DAMAGE_TYPES resistance_type)
	{
		return this.resistances[resistance_type];
	}
	
	public void OnBeforeSerialize()
	{
		this._ser_resistance_keys.Clear();
		this._ser_resistance_values.Clear();

		foreach (KeyValuePair<DAMAGE_TYPES, Resistance> kvp in this.resistances) {
			this._ser_resistance_keys.Add(kvp.Key);
			this._ser_resistance_values.Add(kvp.Value);
		}
	}

	public void OnAfterDeserialize()
	{
		this.resistances = new Dictionary<DAMAGE_TYPES, Resistance>();

		for (int i = 0; i < this._ser_resistance_keys.Count; i++) {
			this.resistances.Add(this._ser_resistance_keys[i], this._ser_resistance_values[i]);
		}
	}
}
