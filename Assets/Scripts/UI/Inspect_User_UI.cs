using System;
using UI;
using UnityEngine;

public class Inspect_User_UI : MonoBehaviour
{
	[SerializeField] private Network_Connection network;
	[SerializeField] private GameObject preview_container;
	[SerializeField] private Character_Preview character_preview_prefab;
	[SerializeField] private UI_Manager ui_manager;
	[SerializeField] private Page character_inspect_page; 

	private void Start()
	{
		this.network.Message_Received += this.On_Network_Message_Received;
	}

	private void OnDestroy()
	{
		this.network.Message_Received -= this.On_Network_Message_Received;
	}

	private void On_Network_Message_Received(Network_Connection.Network_Message message)
	{
		if ((PACKET_TYPE) message.Packet_Type != PACKET_TYPE.GET_CHARACTER_PREVIEWS_RESPONSE) return;

		var data = JsonUtility.FromJson<Character_Select.Retrieve_Characters_Response_Data>(message.payload);
		foreach (Transform child in this.preview_container.transform) {
			Destroy(child.gameObject);
		}
		
		foreach (Character_Select.Character_Preview_Data preview in data.Character_Previews) {
			Character_Preview obj = Instantiate(this.character_preview_prefab, this.preview_container.transform);
			obj.Populate(preview);
			obj.Initialize(this.character_inspect_page, this.ui_manager);
		}
	}
}
