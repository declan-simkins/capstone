using System;
using TMPro;
using UnityEngine;

public class Attributes_UI : MonoBehaviour
{
	[SerializeField] private Character_Attributes attributes;
	[SerializeField] private TextMeshProUGUI attributes_text;

	private void OnEnable()
	{
		this.attributes_text.text = "";
		foreach (ATTRIBUTES attribute_type in Enum.GetValues(typeof(ATTRIBUTES))) {
			Character_Attributes.Char_Attr attributeData = this.attributes.Get_Attribute(attribute_type);
			this.attributes_text.text += $"{attribute_type}: {attributeData.Base_Value} -> {attributeData.Current_Value}\n";
		}
	}
}
