using System;
using System.Collections;
using System.Collections.Generic;
using Scriptable_Objects.Scripts;
using TMPro;
using UnityEngine;

public class Character_Overview_UI : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI attribute_text
		, resistances_text
		, resources_text
		, skill_text
		, info_text;


	[SerializeField] private Character character;
	[SerializeField] private Network_Connection network;
	[SerializeField] private Character_Info info;

	private Character_Attributes Attributes => this.character.attributes;
	private Character_Resistances Resistances => this.character.resistances;
	private Character_Resources Resources => this.character.resources;
	private Character_Info Info => this.character.info;
	private Skills Skills => this.character.skills;

	private void Start()
	{
		this.network.Message_Received += this.On_Network_Message_Received;
	}

	private void OnDestroy()
	{
		this.network.Message_Received -= this.On_Network_Message_Received;
	}
	
	private void OnEnable()
	{
		this.Get_Character();
		this.StartCoroutine(this.Refresh_Tick());
	}

	private void OnDisable()
	{
		this.StopAllCoroutines();
	}

	private IEnumerator Refresh_Tick()
	{
		for (;;) {
			this.Refresh();
			yield return new WaitForSeconds(0.1f);
		}
	}


	private void Refresh()
	{
		this.Populate_Attributes();
		this.Populate_Skills();
		this.Populate_Resistances();
		this.Populate_Resources();
	}

	private void Populate_Attributes()
	{
		this.attribute_text.text = "";
		foreach (KeyValuePair<ATTRIBUTES, Character_Attributes.Char_Attr> kvp in this.Attributes.Attributes) {
			this.attribute_text.text += $"<b>{kvp.Key}</b>: {kvp.Value.Base_Value} -> {kvp.Value.Current_Value}\n";
		}
	}

	private void Populate_Skills()
	{
		this.skill_text.text = "";
		foreach (KeyValuePair<SKILLS, Skill> kvp in this.Skills.Base_Skills) {
			this.skill_text.text += $"<b>{kvp.Key}</b>: {kvp.Value.Base_Value} -> {kvp.Value.Current_Value} ";
			this.skill_text.text += $"({kvp.Value.XP}/100)\n";
		}
	}
	
	private void Populate_Resistances()
	{
		this.resistances_text.text = "";
		foreach (DAMAGE_TYPES resistance_type in Enum.GetValues(typeof(DAMAGE_TYPES))) {
			Resistance resistance = this.Resistances.Get_Resistance(resistance_type);
			this.resistances_text.text += $"<b>{resistance.Resistance_Type}</b>: ";
			this.resistances_text.text += $"{resistance.Base_Value} -> {resistance.Current_Value}\n";
		}
	}

	private void Populate_Resources()
	{
		this.resources_text.text = "";
		foreach (RESOURCES resource_type in Enum.GetValues(typeof(RESOURCES))) {
			Resource resource = this.Resources.Get_Resource(resource_type);
			this.resources_text.text += $"<b>{resource.Resource_Type}</b>: {resource.Current} / {resource.Max}\n";
		}
	}
	
	private void Get_Character()
	{
		Character_Select.Character_Select_Data data = new Character_Select.Character_Select_Data
		{
			Character_ID = (UInt16) this.info.Character_ID
		};
		string json_data = JsonUtility.ToJson(data);

		Network_Connection.Network_Message msg = new Network_Connection.Network_Message
		{
			Packet_Type = (UInt16) PACKET_TYPE.GET_CHARACTER,
			user_id = this.network.User_ID,
			payload_size = (UInt16) json_data.Length,
			payload = json_data
		};
		this.network.Queue_Message(msg);
	}

	private void On_Network_Message_Received(Network_Connection.Network_Message message)
	{
		if ((PACKET_TYPE) message.Packet_Type != PACKET_TYPE.GET_CHARACTER_RESPONSE) return;
		
		Character_Loader.Character_Data data = JsonUtility.FromJson<Character_Loader.Character_Data>(message.payload);
		this.character.attributes.Reinit(data.Attributes);
		this.character.resources.Reinit(data.Resources);
		this.character.skills.Reinit(data.Skills);
		this.character.info.Level = data.Level;
		this.character.info.Character_Name = data.Name;
		this.character.info.Location_ID = data.Location_ID;
	}
}
