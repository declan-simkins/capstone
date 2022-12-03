using System.Collections.Generic;
using UnityEngine;
using World;

public class Location_Map : MonoBehaviour
{
	[SerializeField] private List<Location> locations;

	public Location Get_Location(int id)
	{
		if (id > this.locations.Count || id == 0) return null;
		
		return this.locations[id - 1];
	}
}
