using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Location DB", menuName = "Scriptable Objects/Location Database")]
public class Location_DB : ScriptableObject
{
	[SerializeField] private Network_Connection network;
	public List<Location_Data> locations;

	private void OnEnable()
	{
		// get locations from server
	}
}


public struct Location_Data
{
	public int Location_ID;
	public string name, description;
	public bool hidden;
}
