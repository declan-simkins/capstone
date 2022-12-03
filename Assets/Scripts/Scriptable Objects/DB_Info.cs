using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New DB Info", menuName = "Scriptable Objects/Database Info")]
[Serializable]
public class DB_Info : ScriptableObject
{
	[TextArea(4, 12)]
	public string connection_string;
}
