using UnityEngine;
using UnityEngine.SceneManagement;
using World;

public class Controller_Adventure : MonoBehaviour
{
	[SerializeField] private Character_Info character_info;
	[SerializeField] private Location_Map location_map;
	[SerializeField] private Network_Connection network;

	private struct Travel_Response
	{
		public int Destination_ID;
		public bool Status;
	}
	
	private struct Enter_Location_Response
	{
		public bool Success;
		public string Scene_Name;
	}

	private void Awake()
	{
		Location start_location = this.location_map.Get_Location(this.character_info.Location_ID);
		if (start_location == null) return;

		Vector3 start_pos = start_location.pawn_position.position;
		this.gameObject.transform.position = start_pos;
	}

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
		switch ((PACKET_TYPE) message.Packet_Type) {
			case PACKET_TYPE.TRAVEL_RESPONSE:
				Travel_Response travel_response = JsonUtility.FromJson<Travel_Response>(message.payload);
				if (travel_response.Status == false) return;
				this.Travel_To(travel_response.Destination_ID);
				break;
			
			case PACKET_TYPE.ENTER_LOCATION_RESPONSE:
				Enter_Location_Response enter_response = JsonUtility.FromJson<Enter_Location_Response>(message.payload);
				if (enter_response.Success == false) return;
				this.Enter_Location(enter_response.Scene_Name);
				break;
			
			default:
				return;
		}
	}

	private void Travel_To(int location_id)
	{
		Location new_location = this.location_map.Get_Location(location_id);
		Vector3 new_pos = new_location.pawn_position.position;
		this.gameObject.transform.position = new_pos;
		this.character_info.Location_ID = location_id;
	}

	private void Enter_Location(string scene_name)
	{
		SceneManager.LoadScene(scene_name);
	}
}
