using System;
using UnityEngine;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
	private struct Login_Data
	{
		public string Username, Password;
	}

	private struct Login_Response
	{
		public bool Success;
		public UInt32 User_ID;
	}
	
	[SerializeField] private Text username_text, password_text, status_text;
	[SerializeField] private GameObject login_screen;
	[SerializeField] private Network_Connection network;

	private void Start()
	{
		this.network.Message_Received += this.Network_Message_Received;
	}

	private void OnDestroy()
	{
		this.network.Message_Received -= this.Network_Message_Received;
	}

	public void Send_Login()
	{
		this.status_text.text = "Logging In...";
		
		Login_Data data = new Login_Data
		{
			Username = this.username_text.text,
			Password = this.password_text.text
		};
		string json_login_data = JsonUtility.ToJson(data);

		Network_Connection.Network_Message msg = new Network_Connection.Network_Message
		{
			user_id = 0,
			Packet_Type = (UInt16) PACKET_TYPE.LOGIN,
			payload_size = (UInt16) json_login_data.Length,
			payload = json_login_data
		};
		this.network.Queue_Message(msg);
	}

	private void Network_Message_Received(Network_Connection.Network_Message message)
	{
		if ((PACKET_TYPE) message.Packet_Type != PACKET_TYPE.LOGIN_RESPONSE) return;

		Login_Response login_response = (Login_Response) JsonUtility.FromJson(message.payload, typeof(Login_Response));

		if (login_response.Success) {
			this.status_text.color = Color.green;
			this.status_text.text = "Successfully logged in!";
		}
		else {
			this.status_text.color = Color.red;
			this.status_text.text = "Incorrect username or password!";
		}
	}
}
