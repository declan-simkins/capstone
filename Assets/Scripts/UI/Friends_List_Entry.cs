using System;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class Friends_List_Entry : MonoBehaviour
{
	[SerializeField] private Network_Connection network;
	[SerializeField] private UI_Manager ui_manager;
	[SerializeField] private Chat_Participants chat_participants;
	
	[SerializeField] private TextMeshProUGUI name_text;
	
	[SerializeField] private Page user_inspect_page;
	[SerializeField] private Page chat_page;

	private string username;
	private int user_id;

	public void Populate(string username, int user_id, Page inspect_page, Page chat_page, UI_Manager ui_manager)
	{
		this.username = username;
		this.user_id = user_id;
		this.name_text.text = this.username;
		this.user_inspect_page = inspect_page;
		this.chat_page = chat_page;
		this.ui_manager = ui_manager;
	}

	public void Inspect_User()
	{
		Network_Connection.Network_Message msg = new Network_Connection.Network_Message
		{
			Packet_Type = (UInt16) PACKET_TYPE.GET_CHARACTER_PREVIEWS,
			user_id = (UInt32) this.user_id,
			payload_size = 0,
			payload = ""
		};
		this.network.Queue_Message(msg);
		this.ui_manager.Switch_To(this.user_inspect_page);
	}

	public void Chat()
	{
		this.chat_participants.Participant_IDs.Clear();
		this.chat_participants.Participant_IDs.Add((int) this.network.User_ID);
		this.chat_participants.Participant_IDs.Add(this.user_id);
		if (this.chat_page.gameObject.activeSelf) {
			this.chat_page.GetComponent<Chat_UI>().Refresh();
		}
		else {
			this.ui_manager.Toggle_Page(this.chat_page);	
		}
	}
}
