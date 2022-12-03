using System;
using UnityEngine;

[Serializable]
public class Character_Attribute_Data
{
	public int Attribute_Type;
	public int Base_Value;
	public int Current_Value;

	public ATTRIBUTES Attribute_Enum => (ATTRIBUTES) this.Attribute_Type;
}
