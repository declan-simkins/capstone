using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum SKILLS
{
	POLEARMS,
	SWORDS,
	AXES,
	BLUNT,
	BOW,
	LIGHT_ARMOR,
	HEAVY_ARMOR,
	SHIELDS,
	ELEMENTAL,
	ARCANE,
	ESSENCE
}

namespace Scriptable_Objects.Scripts
{
	[CreateAssetMenu(fileName = "New Skills", menuName = "Scriptable Objects/Skills")]
	public class Skills : ScriptableObject, ISerializationCallbackReceiver
	{
		private const string key = "skills editor";
		private const int DEFAULT_SKILL_VALUE = 0;
		private const int DEFAULT_SKILL_XP = 0;

		
		#region Private Fields
		private Dictionary<SKILLS, Skill> base_skills = new Dictionary<SKILLS, Skill>();
		#endregion Private Fields
		
		
		#region Serialization Fields
		[HideInInspector, SerializeField] private List<SKILLS> _ser_base_skill_keys = new List<SKILLS>();
		[HideInInspector, SerializeField] private List<Skill> _ser_base_skill_values = new List<Skill>();
		#endregion Serialization Fields

		
		#region Properties
		public Dictionary<SKILLS, Skill> Base_Skills => new Dictionary<SKILLS, Skill>(this.base_skills);
		#endregion Properties

		
		private void OnEnable()
		{
			foreach (SKILLS skill_type in Enum.GetValues(typeof(SKILLS))) {
				Skill default_skill = CreateInstance<Skill>();
				default_skill.Base_Value = DEFAULT_SKILL_VALUE;
				default_skill.Current_Value = DEFAULT_SKILL_VALUE;
				default_skill.Skill_Type = skill_type;
				default_skill.XP = DEFAULT_SKILL_XP;

				if (this.base_skills.ContainsKey(skill_type)) {
					this.base_skills[skill_type] = default_skill;
				}
				else {
					this.base_skills.Add(skill_type, default_skill);
				}
			}
		}

		public static Skills Create(List<Skill> skills)
		{
			Skills new_skills = CreateInstance<Skills>();

			foreach (Skill skill in skills) {
				new_skills.base_skills.Add(skill.Skill_Type, skill);
			}

			return new_skills;
		}

		public void Reinit(List<Skill> new_skills)
		{
			foreach (Skill skill in new_skills) {
				this.base_skills.Add(skill.Skill_Type, skill);
			}
		}

		public Skill Get_Skill(SKILLS skill)
		{
			return this.base_skills[skill];
		}

		public int Get_Base_Skill(SKILLS skill)
		{
			return this.base_skills[skill].Base_Value;
		}
		
		public void Affect_Skill(SKILLS skill, int amount)
		{
			this.base_skills[skill].Current_Value += amount;
		}

		public void OnBeforeSerialize()
		{
			this._ser_base_skill_keys.Clear();
			this._ser_base_skill_values.Clear();

			foreach (KeyValuePair<SKILLS, Skill> kvp in this.base_skills) {
				this._ser_base_skill_keys.Add(kvp.Key);
				this._ser_base_skill_values.Add(kvp.Value);
			}
		}

		public void OnAfterDeserialize()
		{
			this.base_skills = new Dictionary<SKILLS, Skill>();

			for (int i = 0; i < this._ser_base_skill_keys.Count; i++) {
				this.base_skills.Add(this._ser_base_skill_keys[i], this._ser_base_skill_values[i]);
			}
		}

		/// <summary>
		/// Updates a given base skill if the appropriate key is provided.
		/// The key kinda limits access as ideally this would only be called via
		/// the editor.
		/// </summary>
		/// <param name="target_skill">Skill to be changed.</param>
		/// <param name="value">Value to change the skill to.</param>
		public void Update_Base_Skill(SKILLS target_skill, int value, string key)
		{
			if (key != Skills.key) {
				Debug.LogError("Attempt to update base skill without proper key.");
				return;
			}
			
			int modifier = this.base_skills[target_skill].Current_Value - this.base_skills[target_skill].Base_Value;
			this.base_skills[target_skill].Base_Value = value;
			this.base_skills[target_skill].Current_Value = this.base_skills[target_skill].Base_Value + modifier;
		}
	}
}
