using System;
using Scriptable_Objects.Scripts;
using UnityEngine;

public enum DAMAGE_TYPES
{
	BLUDGEONING,
	PIERCING,
	SLASHING,
	ARCANE,
	ICHOR,
	FIRE,
	LIGHT,
	LIGHTNING,
	MENTAL,
	PRIMAL,
	SPIRIT,
	VOID
}

[Serializable]
public class Damage_Source
{
	[SerializeField] private DAMAGE_TYPES damage_type;
	[SerializeField] private int amount;

	public Damage_Source(DAMAGE_TYPES damage_type, int amount)
	{
		this.damage_type = damage_type;
		this.amount = amount;
	}

	public DAMAGE_TYPES Damage_Type => this.damage_type;
	public int Amount => this.amount;
}
