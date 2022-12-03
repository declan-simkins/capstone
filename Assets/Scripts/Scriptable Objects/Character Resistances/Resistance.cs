using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Resistance : ScriptableObject
{
	public DAMAGE_TYPES Resistance_Type;
	public int Base_Value, Current_Value;
}
