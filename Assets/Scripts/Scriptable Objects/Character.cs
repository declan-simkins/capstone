using System.Collections;
using System.Collections.Generic;
using Scriptable_Objects.Scripts;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Scriptable Objects/Character")]
public class Character : ScriptableObject
{
	public Character_Attributes attributes;
	public Character_Resistances resistances;
	public Character_Resources resources;
	public Character_Info info;
	public Skills skills;
}
