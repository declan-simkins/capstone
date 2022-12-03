using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Equip_UI : MonoBehaviour
{
	[SerializeField] private TMP_InputField item_id_input;
	[SerializeField] private Network_Connection network;
	[SerializeField] private Character_Info char_info;

	[Serializable]
	public struct Equip_Payload
	{
		public int Character_ID;
		public int Equipment_ID;
	}
	
	public void Send_Equip_Request()
	{
		int.TryParse(this.item_id_input.text, out int id);
		var payload = new Equip_Payload
		{
			Character_ID = this.char_info.Character_ID,
			Equipment_ID = id
		};
		this.network.Queue_Message_Type(PACKET_TYPE.EQUIP, payload);
	}
}
