using System;
using System.Collections.Generic;
using Camera_Behaviours;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Location_Scene : MonoBehaviour
{
	[SerializeField] private Network_Connection network;
	[SerializeField] private Character_Info player_info;
	
	[SerializeField] private Motor_Exploration occupant_prefab;
	[SerializeField] private Controller_Exploration player_prefab;
	[SerializeField] private Floating_Character_Name floating_name_prefab;
	
	[SerializeField] private Follow_Camera player_camera;
	[SerializeField] private Canvas canvas;

	private Dictionary<int, Motor_Exploration> occupant_motors = new Dictionary<int, Motor_Exploration>();

	[Serializable]
	private struct Location_Occupant
	{
		public int character_id;
		public float pos_x, pos_y;
	}
	
	[Serializable]
	private struct Update_Positions_Payload
	{
		public List<Location_Occupant> occupants;
	}

	private struct Exit_Location_Payload
	{
		public int Character_ID;
		public int Location_ID;
	}
	
	private void Start()
	{
		this.network.Message_Received += this.On_Network_Message_Received;
		this.Add_Player();
	}

	private void OnDestroy()
	{
		this.network.Message_Received -= this.On_Network_Message_Received;
	}

	private void Add_Occupant(Location_Occupant new_occupant)
	{
		var obj = Instantiate(this.occupant_prefab, this.transform);
		var floating_name = Instantiate(this.floating_name_prefab, this.canvas.transform);
		floating_name.transform.localScale.Scale(this.transform.localScale);
		floating_name.Initialize(obj.gameObject, this.player_info);
		obj.Place(new Vector2(new_occupant.pos_x, new_occupant.pos_y));
		this.occupant_motors[new_occupant.character_id] = obj;
	}

	private void Add_Player()
	{
		Location_Occupant player = new Location_Occupant
		{
			character_id = this.player_info.Character_ID,
			pos_x = 0,
			pos_y = 0
		};
		var obj = Instantiate(this.player_prefab, this.transform);
		var motor = obj.GetComponent<Motor_Exploration>();
		motor.Place(new Vector2(player.pos_x, player.pos_y));
		this.occupant_motors[player.character_id] = motor;
		this.player_camera.Set_Target(obj.gameObject);
	}

	// Should build a queue of occupants and deltas and then move them all in late update
	private void On_Network_Message_Received(Network_Connection.Network_Message message)
	{
		switch ((PACKET_TYPE) message.Packet_Type) {
			case PACKET_TYPE.UPDATE_PLAYER_POSITIONS:
				this.Update_Player_Positions(message);
				break;
			
			case PACKET_TYPE.EXIT_LOCATION:
				this.Exit_Location(message);
				break;
		}
	}

	private void Exit_Location(Network_Connection.Network_Message message)
	{
		var payload = JsonUtility.FromJson<Exit_Location_Payload>(message.payload);
		if (this.player_info.Character_ID == payload.Character_ID) {
			SceneManager.LoadScene("Adventure Layer");
		}
		else {
			Destroy(this.occupant_motors[payload.Character_ID].gameObject);
			this.occupant_motors.Remove(payload.Character_ID);
		}
	}

	private void Update_Player_Positions(Network_Connection.Network_Message message)
	{
		Update_Positions_Payload data = JsonUtility.FromJson<Update_Positions_Payload>(message.payload);
		foreach (Location_Occupant occupant in data.occupants) {
			if (!this.occupant_motors.ContainsKey(occupant.character_id)
			    && occupant.character_id != this.player_info.Character_ID) {
				this.Add_Occupant(occupant);
				continue;
			}
			
			this.occupant_motors[occupant.character_id].Place(
				new Vector2(
					occupant.pos_x + this.transform.localScale.x / 2,
					occupant.pos_y + this.transform.localScale.y / 2
				)
			);
		}

		foreach (KeyValuePair<int,Motor_Exploration> pair in this.occupant_motors) {
			
		}
	}
}
