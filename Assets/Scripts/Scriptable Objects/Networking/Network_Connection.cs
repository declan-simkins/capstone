using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;


public enum PACKET_TYPE
{
	LOGIN,
	LOGIN_RESPONSE,
	GET_CHARACTER_PREVIEWS,
	GET_CHARACTER_PREVIEWS_RESPONSE,
	SELECT_CHARACTER,
	SELECT_CHARACTER_RESPONSE,
	GET_FRIENDS,
	GET_FRIENDS_RESPONSE,
	ADD_FRIEND,
	ADD_FRIEND_RESPONSE,
	GET_DMS,
	GET_DMS_RESPONSE,
	SEND_DM,
	TRAVEL,
	TRAVEL_RESPONSE,
	ENTER_LOCATION,
	ENTER_LOCATION_RESPONSE,
	MOVE,
	UPDATE_PLAYER_POSITIONS,
	CREATE_ACCOUNT,
	CREATE_ACCOUNT_RESPONSE,
	CREATE_CHARACTER,
	CREATE_CHARACTER_RESPONSE,
	EXIT_LOCATION,
	EQUIP,
	EQUIP_RESPONSE,
	GET_CHARACTER,
	GET_CHARACTER_RESPONSE
}


[CreateAssetMenu(fileName = "New Network Connection", menuName = "Scriptable Objects/Network Connection")]
public class Network_Connection : ScriptableObject
{
	public struct Network_Message
	{
		public UInt32 user_id;
		public UInt16 Packet_Type, payload_size;
		public string payload;
	}
	
	public delegate void On_Message_Received(Network_Message message);
	public event On_Message_Received Message_Received;

	private readonly IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
	[SerializeField] private int port = 8000;
	[SerializeField] private bool use_local_ip = true;
	[SerializeField] private byte[] remote_ip;

	private Socket socket;
	private IPEndPoint end_point;

	private readonly Queue<Network_Message> message_queue = new Queue<Network_Message>();
	
	private const int HEADER_SIZE = 8; // TODO: Get this from server in a setup packet
	
	private UInt32 user_id;
	public UInt32 User_ID => this.user_id;

	public bool Connected => this.socket.Connected;
	public int Available => this.socket.Available;
	public Queue<Network_Message> Message_Queue => this.message_queue;

	private void OnEnable()
	{
		this.socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
	}

	[ContextMenu("Open Connection")]
	public void Open_Connection()
	{
		if (this.socket.Connected) return;

		if (this.use_local_ip) {
			this.end_point = new IPEndPoint(this.Get_Local_IP(), port);
		}
		else {
			var addr = new IPAddress(this.remote_ip);
			this.end_point = new IPEndPoint(addr, port);
		}

		this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		this.socket.Connect(this.end_point);

		if (this.socket.Connected) {
			Debug.Log("Connection Successful.");
		}
		else {
			Debug.Log("Connection Failed.");
		}
	}
	
	public void Queue_Message(Network_Message message)
	{
		this.message_queue.Enqueue(message);
	}

	public void Queue_Message_Type<T>(PACKET_TYPE packet_type, T payload_data)
	{
		string json_payload = JsonUtility.ToJson(payload_data);
		Network_Message msg = new Network_Message
		{
			Packet_Type = (UInt16) packet_type,
			user_id = this.user_id,
			payload_size = (UInt16) json_payload.Length,
			payload = json_payload
		};
		this.Queue_Message(msg);
	}

	public async void Send_Message(Network_Message message)
	{
		if (this.message_queue.Count < 0) return;
		
		byte[] byte_message = Message_Bytes(message);
		ArraySegment<byte> bytes = new ArraySegment<byte>(byte_message);
		await this.socket.SendAsync(bytes, SocketFlags.None);
	}

	public void Receive_Messages()
	{
		int byte_count = 0;
		do {
			if (this.socket.Available == 0) {
				return;
			}
			
			Network_Message message = new Network_Message();

			// Get header
			byte[] header_bytes = new byte[HEADER_SIZE];
			byte_count = this.socket.Receive(header_bytes, header_bytes.Length, SocketFlags.None);
			if (byte_count == 0) {
				return;
			}

			message.user_id = Read_Bytes32(header_bytes, 0);
			message.Packet_Type = Read_Bytes16(header_bytes, 4);
			message.payload_size = Read_Bytes16(header_bytes, 6);

			// Get Payload
			byte[] payload_bytes = new byte[message.payload_size];
			this.socket.Receive(payload_bytes, payload_bytes.Length, SocketFlags.None);
			message.payload = Encoding.UTF8.GetString(payload_bytes);

			if ((PACKET_TYPE) message.Packet_Type == PACKET_TYPE.LOGIN_RESPONSE) {
				this.user_id = message.user_id;
			}
			this.Message_Received?.Invoke(message);
		} while (byte_count > 0);
	}
	
	private static byte[] Message_Bytes(Network_Message message)
	{
		byte[] msg_bytes = new byte[HEADER_SIZE + message.payload_size + 1];

		ArraySegment<byte> id_bytes = new ArraySegment<byte>(msg_bytes, 0, 4);
		To_Bytes(message.user_id, id_bytes);
		
		ArraySegment<byte> action_bytes = new ArraySegment<byte>(msg_bytes, 4, 2);
		To_Bytes(message.Packet_Type, action_bytes);

		ArraySegment<byte> size_bytes = new ArraySegment<byte>(msg_bytes, 6, 2);
		To_Bytes(message.payload_size, size_bytes);

		// Append payload
		Encoding.UTF8.GetBytes(message.payload).CopyTo(msg_bytes, HEADER_SIZE);
		
		return msg_bytes;
	}

	private static void To_Bytes(UInt32 value, ArraySegment<byte> dest)
	{
		if (dest.Array == null) return;

		UInt32 mask = 0x000000FF;
		for (int i = 0; i < dest.Count; i++) {
			byte temp = Convert.ToByte((value >> (i * 8)) & mask);
			// Debug.Log($"Byte {dest.Offset + i}: {temp}");
			dest.Array[dest.Offset + i] = temp;
		}
	}
	
	private static UInt32 Read_Bytes32(byte[] bytes, int start)
	{
		return BitConverter.ToUInt32(bytes, start);
	}
	
	private static UInt16 Read_Bytes16(byte[] bytes, int start)
	{
		return BitConverter.ToUInt16(bytes, start);
	}
	
	private IPAddress Get_Local_IP()
	{
		foreach (IPAddress ip_address in this.host.AddressList) {
			if (ip_address.AddressFamily == AddressFamily.InterNetwork) {
				return ip_address;
			}
		}

		return null;
	}
	
	[ContextMenu("Close Connection")]
	public void Close_Connection()
	{
		this.socket.Close();
		Debug.Log("Connection Closed.");
	}

	private void OnDestroy()
	{
		this.Close_Connection();
	}
}


[Serializable]
public struct Get_Chat_Payload
{
	public List<int> Participant_IDs;
}

[Serializable]
public struct Get_Chat_Response
{
	public List<string> Messages;
	public List<string> Senders;
}

[Serializable]
public struct Send_DM_Payload
{
	public int Sender;
	public int Recipient;
	public string Content;
}