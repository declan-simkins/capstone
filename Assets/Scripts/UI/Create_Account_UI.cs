using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class Create_Account_UI : MonoBehaviour
{
	private struct Create_Account_Payload
	{
		public string Username, Password, Confirm;
	}

	private struct Create_Account_Response
	{
		public bool Success;
		public int User_ID;
	}
	
	[SerializeField] private InputField username, password, confirm;
	[SerializeField] private Text status_text;
	[SerializeField] private UI_Manager ui_manager;
	[SerializeField] private Page page;
	[SerializeField] private Network_Connection network;

	private void Start()
	{
		this.network.Message_Received += this.Network_Message_Received;
	}

	private void OnDestroy()
	{
		this.network.Message_Received -= this.Network_Message_Received;
	}

	public void Create_Account()
	{
		this.status_text.color = Color.yellow;
		this.status_text.text = "Creating Account...";

		Create_Account_Payload payload = new Create_Account_Payload
		{
			Username = this.username.text,
			Password = this.password.text,
			Confirm = this.confirm.text
		};
		this.network.Queue_Message_Type(PACKET_TYPE.CREATE_ACCOUNT, payload);
	}
	
	private void Network_Message_Received(Network_Connection.Network_Message message)
	{
		if ((PACKET_TYPE) message.Packet_Type != PACKET_TYPE.CREATE_ACCOUNT_RESPONSE) return;

		Create_Account_Response response = JsonUtility.FromJson<Create_Account_Response>(message.payload);

		if (response.Success) {
			this.status_text.color = Color.green;
			this.status_text.text = "Account created successfully";
		}
		else {
			this.status_text.color = Color.red;
			this.status_text.text = "Failed to create account";
		}
	}
}
