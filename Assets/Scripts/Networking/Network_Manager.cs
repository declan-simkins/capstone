using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;


public class Network_Manager : MonoBehaviour
{

	[SerializeField] private Network_Connection conn;

	private readonly Queue<Network_Connection.Network_Message> message_queue = new Queue<Network_Connection.Network_Message>();
	
	private const int HEADER_SIZE = 8; // TODO: Get this from server in a setup packet

	private UInt32 user_id;
	public UInt32 User_ID => this.user_id;

	private void Awake()
	{
		this.conn.Open_Connection();
	}

	private void Update()
	{
		if (!this.conn.Connected) return;
		
		if (this.conn.Available > 0) {
			this.conn.Receive_Messages();
		}
	}

	private void LateUpdate()
	{
		if (!this.conn.Connected) return;
		
		foreach (Network_Connection.Network_Message message in this.conn.Message_Queue) {
			this.conn.Send_Message(message);
		}
		this.conn.Message_Queue.Clear();
	}
}
