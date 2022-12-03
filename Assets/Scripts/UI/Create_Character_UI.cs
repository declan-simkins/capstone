using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Create_Character_UI : MonoBehaviour
{
	private struct Create_Character_Payload
	{
		public string Character_Name;
	}

	private struct Create_Character_Response
	{
		public bool Success;
		public int Character_ID;
	}
	
	[SerializeField] private InputField character_name;
	[SerializeField] private Network_Connection network;
	[SerializeField] private Text status_text;

	private void Start()
	{
		this.network.Message_Received += this.Network_Message_Received;
	}

	private void OnDestroy()
	{
		this.network.Message_Received -= this.Network_Message_Received;
	}

	public void Create_Character()
	{
		this.status_text.color = Color.yellow;
		this.status_text.text = "Creating Character...";

		Create_Character_Payload payload = new Create_Character_Payload
		{
			Character_Name = this.character_name.text
		};
		this.network.Queue_Message_Type(PACKET_TYPE.CREATE_CHARACTER, payload);
	}
	
	private void Network_Message_Received(Network_Connection.Network_Message message)
	{
		if ((PACKET_TYPE) message.Packet_Type != PACKET_TYPE.CREATE_CHARACTER_RESPONSE) return;

		Create_Character_Response response = JsonUtility.FromJson<Create_Character_Response>(message.payload);

		if (response.Success) {
			this.status_text.color = Color.green;
			this.status_text.text = "Character created successfully";
		}
		else {
			this.status_text.color = Color.red;
			this.status_text.text = "Failed to create character";
		}
	}
}
