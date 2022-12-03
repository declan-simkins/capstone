using System;
using System.Collections.Generic;
using Scriptable_Objects.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;

// TODO: Load Resistances and equipment
[Serializable]
public class Character_Loader : MonoBehaviour
{
	[SerializeField] private Network_Connection network;
	[SerializeField] private Character_Attributes attributes;
	[SerializeField] private Character_Resources resources;
	[SerializeField] private Skills skills;
	[SerializeField] private Character_Info character;
	
	[Serializable]
	public class Character_Data
	{
		public string Name;
		public int Level;
		public int Location_ID;
		public int Character_ID;
		public List<Character_Attribute_Data> Attributes;
		public List<Skill> Skills;
		public List<Resource> Resources;
	}

	// Loads a character into the provided scriptable objects
	public void Load_Character(Character_Data data)
	{
		this.attributes.Reinit(data.Attributes);
		this.resources.Reinit(data.Resources);
		this.skills.Reinit(data.Skills);
		this.character.Location_ID = data.Location_ID;
		this.character.Character_Name = data.Name;
		this.character.Level = data.Level;
		this.character.Character_ID = data.Character_ID;
	}

	private void Start()
	{
		this.network.Message_Received += this.Network_Message_Received;
	}

	private void OnDestroy()
	{
		this.network.Message_Received -= this.Network_Message_Received;
	}

	private void Network_Message_Received(Network_Connection.Network_Message message)
	{
		if ((PACKET_TYPE) message.Packet_Type != PACKET_TYPE.SELECT_CHARACTER_RESPONSE) return;

		Character_Data data = JsonUtility.FromJson<Character_Data>(message.payload);

		this.Load_Character(data);
		SceneManager.LoadScene("Scenes/Adventure Layer");
	}
}
