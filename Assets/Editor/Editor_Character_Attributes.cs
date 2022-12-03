using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Character_Attributes))]
public class Editor_Character_Attributes : UnityEditor.Editor
{
	public override void OnInspectorGUI()
	{
		Character_Attributes attributes = (Character_Attributes) this.target;
		
		GUI.enabled = false;
		foreach (KeyValuePair<ATTRIBUTES, Character_Attributes.Char_Attr> kvp in attributes.Attributes) {
			EditorGUILayout.ObjectField($"{kvp.Key}"
				, kvp.Value
				, typeof(Character_Attributes.Char_Attr)
				, false
			);
			//GUILayout.Label($"{kvp.Key} | {kvp.Value.Base_Value}");
		}
		GUI.enabled = true;
	}
}
