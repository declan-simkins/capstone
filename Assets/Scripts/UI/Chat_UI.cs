using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

public class Chat_UI : MonoBehaviour
{
	[SerializeField] private Network_Connection network;
	[SerializeField] private Chat_Participants participants;
		
	[SerializeField] private GameObject chat_entry_container;
	[SerializeField] private Chat_Entry chat_entry_prefab;
	[SerializeField] private TextMeshProUGUI username_text;
	[SerializeField] private TMP_InputField send_message_input;


	private void Start()
	{
		this.network.Message_Received += this.On_Network_Message_Received;
	}

	private void OnEnable()
	{
		this.Refresh();
	}

	private void OnDestroy()
	{
		this.network.Message_Received -= this.On_Network_Message_Received;
	}

	private void On_Network_Message_Received(Network_Connection.Network_Message message)
	{
		if ((PACKET_TYPE) message.Packet_Type != PACKET_TYPE.GET_DMS_RESPONSE) return;
		
		foreach (Transform child in this.chat_entry_container.transform) {
			Destroy(child.gameObject);
		}
		Get_Chat_Response data = JsonUtility.FromJson<Get_Chat_Response>(message.payload);
		for (int i = 0; i < Mathf.Min(data.Messages.Count, data.Senders.Count); i++) {
			string msg = data.Messages[i];
			string sender = data.Senders[i];
			
			Chat_Entry obj = Instantiate(this.chat_entry_prefab, this.chat_entry_container.transform);
			obj.Populate(msg, sender);
		}

		if (data.Senders.Count < 2) return;
		this.username_text.text = $"Chat with {data.Senders[1]}";
	}

	public void Refresh()
	{
		Get_Chat_Payload data = new Get_Chat_Payload
		{
			Participant_IDs = new List<int>()
		};
		foreach (int id in this.participants.Participant_IDs) {
			data.Participant_IDs.Add(id);
		}
		string json_data = JsonUtility.ToJson(data);
		Network_Connection.Network_Message msg = new Network_Connection.Network_Message
		{
			Packet_Type = (UInt16) PACKET_TYPE.GET_DMS,
			user_id = this.network.User_ID,
			payload_size = (UInt16) json_data.Length,
			payload = json_data
		};
		this.network.Queue_Message(msg);
	}

	public void Send_Message()
	{
		int recipient;
		if (this.participants.Participant_IDs[0] == this.network.User_ID) {
			recipient = this.participants.Participant_IDs[1];
		}
		else {
			recipient = this.participants.Participant_IDs[0];
		}
		Send_DM_Payload data = new Send_DM_Payload
		{
			Sender = (int) this.network.User_ID,
			Recipient = recipient,
			Content = this.send_message_input.text
		};
		
		string json_data = JsonUtility.ToJson(data);
		Network_Connection.Network_Message msg = new Network_Connection.Network_Message
		{
			Packet_Type = (UInt16) PACKET_TYPE.SEND_DM,
			user_id = this.network.User_ID,
			payload_size = (UInt16) json_data.Length,
			payload = json_data
		};
		this.network.Queue_Message(msg);
		this.send_message_input.text = "";
	}
}
