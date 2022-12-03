using System;
using System.Collections;
using System.Collections.Generic;
using Scriptable_Objects.Scripts;
using TMPro;
using UnityEngine;

public class Skills_UI : MonoBehaviour
{
	[SerializeField] private Skills skills;
	[SerializeField] private TextMeshProUGUI skills_text;

	private void OnEnable()
	{
		this.skills_text.text = "";
		foreach (SKILLS skill_type in Enum.GetValues(typeof(SKILLS))) {
			Skill skill = this.skills.Get_Skill(skill_type);
			this.skills_text.text += $"{skill_type}: {skill.Base_Value} -> {skill.Current_Value}\n";
		}
	}
}
