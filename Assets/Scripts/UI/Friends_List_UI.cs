using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UI;
using UnityEngine;

public class Friends_List_UI : MonoBehaviour
{
	[Serializable]
	private struct User_Info
	{
		public string Username;
		public int User_ID;
	}

	[Serializable]
	private struct Add_Friend_Data
	{
		public int Friend_ID;
	}

	[Serializable]
	private struct Add_Friend_Response
	{
		public bool Success;
	}
	
	[Serializable]
	private struct User_List
	{
		public List<User_Info> Users;
	}

	[SerializeField] private UI_Manager ui_manager;
	[SerializeField] private Page inspect_user_page, chat_page;
	[SerializeField] private Network_Connection network;
	[SerializeField] private TMP_InputField add_friend_input;
	[SerializeField] private TextMeshProUGUI status_text;
	[SerializeField] private User_List friends;
	[SerializeField] private string success_message, failed_message;
	[SerializeField] private Friends_List_Entry entry_prefab;
	[SerializeField] private GameObject entry_container;

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
		this.Get_Friends();
	}


	public void Get_Friends()
	{
		Network_Connection.Network_Message msg = new Network_Connection.Network_Message
		{
			Packet_Type = (UInt16) PACKET_TYPE.GET_FRIENDS,
			user_id = this.network.User_ID,
			payload_size = 0,
			payload = ""
		};
		
		this.network.Queue_Message(msg);
	}

	public void Add_Friend()
	{
		bool is_int = int.TryParse(this.add_friend_input.text, out int friend_id);
		if (!is_int) {
			this.Set_Status_Text($"User's ID must be a number.", Color.yellow);
			return;
		}
		
		Add_Friend_Data data = new Add_Friend_Data { Friend_ID = friend_id };
		string json_data = JsonUtility.ToJson(data);

		Network_Connection.Network_Message msg = new Network_Connection.Network_Message()
		{
			Packet_Type = (UInt16) PACKET_TYPE.ADD_FRIEND,
			user_id = this.network.User_ID,
			payload_size = (UInt16) json_data.Length,
			payload = json_data
		};
		this.network.Queue_Message(msg);
	}

	private void Populate_Friends_List()
	{
		foreach (Transform child in this.entry_container.transform) {
			Destroy(child.gameObject);
		}
		
		foreach (User_Info user in this.friends.Users) {
			Friends_List_Entry obj = Instantiate(this.entry_prefab, this.entry_container.transform);
			obj.Populate(user.Username, user.User_ID, this.inspect_user_page, this.chat_page, this.ui_manager);
		}
	}

	private void Set_Status_Text(string text, Color color)
	{
		this.status_text.text = text;
		this.status_text.color = color;
	}

	private void On_Network_Message_Received(Network_Connection.Network_Message message)
	{
		switch ((PACKET_TYPE) message.Packet_Type) {
			case PACKET_TYPE.GET_FRIENDS_RESPONSE:
				this.On_Get_Friends_Response(message);
				break;
			
			case PACKET_TYPE.ADD_FRIEND_RESPONSE:
				this.On_Add_Friend_Response(message);
				break;
			
			default:
				return;
		}
	}

	private void On_Get_Friends_Response(Network_Connection.Network_Message message)
	{
		this.friends = JsonUtility.FromJson<User_List>(message.payload);
		this.Populate_Friends_List();
	}

	private void On_Add_Friend_Response(Network_Connection.Network_Message message)
	{
		Add_Friend_Response data = JsonUtility.FromJson<Add_Friend_Response>(message.payload);
		if (data.Success) {
			this.Set_Status_Text(this.success_message, Color.green);
			this.Populate_Friends_List();
		}
		else {
			this.Set_Status_Text(this.failed_message, Color.red);
		}
	}
}
