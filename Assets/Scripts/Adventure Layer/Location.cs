using System;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
	// TODO: Custom editor that loads locations off of databse and populates a dropdown with them
	//   the location game object can then choose which one it wants to "be"
	//   Routes are then determined by that selection
	public class Location : Travelable
	{
		#region Serialized Fields
		[SerializeField] private int Location_ID;
		[SerializeField] private Network_Connection network;
		[SerializeField] private Character_Info character;
		[SerializeField] private Location_DB locations;
		[SerializeField] private DB_Info db_info;
		#endregion Serialized Fields

		private struct Travel_Payload
		{
			public int Character_ID;
			public int Destination_ID;
		}

		private struct Enter_Location_Payload
		{
			public int Character_ID;
			public int Location_ID;
		}
		
		public void Travel()
		{
			if (this.character.Location_ID == this.Location_ID) {
				this.Enter_Location();
				return;
			}
			
			Travel_Payload payload = new Travel_Payload
			{
				Character_ID = this.character.Character_ID,
				Destination_ID = this.Location_ID
			};
			string json_payload = JsonUtility.ToJson(payload);
			Network_Connection.Network_Message msg = new Network_Connection.Network_Message
			{
				Packet_Type = (UInt16) PACKET_TYPE.TRAVEL,
				user_id = this.network.User_ID,
				payload_size = (UInt16) json_payload.Length,
				payload = json_payload
			};
			this.network.Queue_Message(msg);
		}

		private void Enter_Location()
		{
			Enter_Location_Payload payload = new Enter_Location_Payload
			{
				Character_ID = this.character.Character_ID,
				Location_ID = this.Location_ID
			};
			this.network.Queue_Message_Type(PACKET_TYPE.ENTER_LOCATION, payload);
		}
	}
}
