using System;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class Character_Select : MonoBehaviour
{
	[Serializable]
	public struct Character_Preview_Data
	{
		public string Name, Location_Name;
		public ushort Level, Character_ID;
	}
	
	public struct Retrieve_Characters_Response_Data
	{
		public List<Character_Preview_Data> Character_Previews;
	}

	public struct Character_Select_Data
	{
		public ushort Character_ID;
	}
	
	private struct Login_Response
	{
		public bool Success;
		public UInt32 User_ID;
	}
	
	[SerializeField] private VerticalLayoutGroup preview_container, button_container;
	[SerializeField] private Text character_preview_prefab;
	[SerializeField] private Button select_button_prefab;
	[SerializeField] private Network_Connection network;
	[SerializeField] private UI_Manager ui_manager;
	[SerializeField] private Page page;

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
		switch ((PACKET_TYPE) message.Packet_Type) {
			case PACKET_TYPE.GET_CHARACTER_PREVIEWS_RESPONSE:
				Retrieve_Characters_Response_Data data = (Retrieve_Characters_Response_Data) JsonUtility.FromJson(
					message.payload,
					typeof(Retrieve_Characters_Response_Data)
				);
				this.Populate_Character_Previews(data.Character_Previews);
				break;
			
			case PACKET_TYPE.LOGIN_RESPONSE:
				Login_Response login_response = (Login_Response) JsonUtility.FromJson(message.payload, typeof(Login_Response));
				if (login_response.Success) {
					this.Retrieve_Characters(login_response.User_ID);
					this.ui_manager.Switch_To(this.page);
				}
				break;
		}
	}

	private void Populate_Character_Previews(List<Character_Preview_Data> preview_data)
	{
		foreach (Character_Preview_Data character_data in preview_data) {
			Text txt = Instantiate(this.character_preview_prefab, this.preview_container.transform);
			txt.text = $"{character_data.Name}, Level {character_data.Level}, Location: {character_data.Location_Name}\n";
			
			Button btn = Instantiate(this.select_button_prefab, this.button_container.transform);
			btn.onClick.AddListener(delegate { this.Select_Character(character_data.Character_ID); });
		}
	}

	private void Select_Character(ushort character_id)
	{
		Character_Select_Data payload = new Character_Select_Data()
		{
			Character_ID = character_id
		};

		string payload_json = JsonUtility.ToJson(payload);

		Network_Connection.Network_Message msg = new Network_Connection.Network_Message
		{
			user_id = this.network.User_ID,
			Packet_Type = (UInt16) PACKET_TYPE.SELECT_CHARACTER,
			payload_size = (UInt16) payload_json.Length,
			payload = payload_json
		};
		this.network.Queue_Message(msg);
	}
	
	private void Retrieve_Characters(UInt32 user_id)
	{
		Network_Connection.Network_Message message = new Network_Connection.Network_Message
		{
			Packet_Type = (UInt16) PACKET_TYPE.GET_CHARACTER_PREVIEWS,
			payload = "",
			payload_size = 0,
			user_id = user_id
		};
		
		this.network.Queue_Message(message);
	}
}
