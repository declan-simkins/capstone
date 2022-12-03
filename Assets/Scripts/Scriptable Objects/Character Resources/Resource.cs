using System;
using UnityEngine;


public enum RESOURCES
{
	HEALTH,
	MANA,
	STAMINA,
	ENERGY
}


[Serializable]
public class Resource : ScriptableObject
{
	public RESOURCES Resource_Type;
	public float Max, Min, Current, Regen_Rate;
	public bool Regen;

	public void Regen_Tick()
	{
		if (!this.Regen) return;
		
		this.Current += this.Regen_Rate;
	}
}
