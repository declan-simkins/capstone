using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using Unity.Mathematics;
using UnityEngine;

public class Character_Preview : MonoBehaviour
{
	[SerializeField] private Character inspection_target;
	[SerializeField] private Network_Connection network;
	[SerializeField] private TextMeshProUGUI text_preview;
	[SerializeField] private Page inspect_character_page;
	[SerializeField] private UI_Manager ui_manager;
	private int character_id;

	private void Start()
	{
		this.network.Message_Received += this.On_Network_Message_Received;
	}

	private void OnDestroy()
	{
		this.network.Message_Received -= this.On_Network_Message_Received;
	}

	public void Initialize(Page inspect_page, UI_Manager uim)
	{
		this.inspect_character_page = inspect_page;
		this.ui_manager = uim;
	}

	public void Populate(Character_Select.Character_Preview_Data data)
	{
		this.text_preview.text = $"{data.Name}, Level {data.Level}, {data.Location_Name}";
		this.character_id = data.Character_ID;
	}

	public void Inspect()
	{
		Character_Select.Character_Select_Data data = new Character_Select.Character_Select_Data
		{
			Character_ID = (UInt16) this.character_id
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
		this.inspection_target.attributes.Reinit(data.Attributes);
		this.inspection_target.resources.Reinit(data.Resources);
		this.inspection_target.skills.Reinit(data.Skills);
		this.inspection_target.info.Level = data.Level;
		this.inspection_target.info.Character_Name = data.Name;
		this.inspection_target.info.Location_ID = data.Location_ID;
		
		this.ui_manager.Switch_To(this.inspect_character_page);
	}
}
