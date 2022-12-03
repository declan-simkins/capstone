using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character Info", menuName = "Scriptable Objects/Character Info")]
public class Character_Info : ScriptableObject
{
	public string Character_Name;
	public int Location_ID, Level, Character_ID;
}
