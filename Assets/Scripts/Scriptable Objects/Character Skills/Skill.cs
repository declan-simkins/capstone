using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Skill : ScriptableObject
{
	[HideInInspector] public SKILLS Skill_Type;
	public int Base_Value, Current_Value, XP;
}
