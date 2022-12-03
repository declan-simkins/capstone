using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEditor;
using UnityEngine;

public class Tool_Location_To_DB : EditorWindow
{
	private Vector2 scroll_pos;
	
	private DB_Info db_info;
	private SqliteConnection db;
	private Location_Scene scene;
	private BoxCollider2D scene_boundary;
	private int location_id;

	[Serializable]
	public struct Server_Collider
	{
		public float X, Y;
		public float Width, Height;
		public string Tag;
	}

	[Serializable]
	private struct Item_Stack
	{
		public int Item, Amount;
	}
	
	[Serializable]
	private struct Server_Pickups
	{
		public Server_Collider Collider;
		public List<Item_Stack> Items;
	}

	[Serializable]
	private struct Encounter
	{
		public int Enemy_ID, Amount;
	}
	[Serializable]
	private struct Server_Encounter
	{
		public Server_Collider Collider;
		public List<Encounter> Enemies;
		public List<Vector2> Patrol_Points;
	}

	[Serializable]
	private struct Scene_Data
	{
		public Server_Collider Boundary;
		public List<Server_Collider> Colliders;
		public List<Server_Collider> Exits;
		public List<Server_Pickups> Pickups;
		public List<Server_Encounter> Encounters;
	}
	
	
	[MenuItem("Tools/Location to DB")]
	public static void ShowWindow()
	{
		GetWindow<Tool_Location_To_DB>();
	}
	
	private void OnValidate()
	{
		if (this.db_info != null) {
			this.db = new SqliteConnection(this.db_info.connection_string);
			this.db.Open();
		}
	}

	private void OnGUI()
	{
		EditorStyles.textField.wordWrap = true;
		this.db_info = (DB_Info) EditorGUILayout.ObjectField("DB INFO", this.db_info, typeof(DB_Info), false);
		this.scroll_pos = EditorGUILayout.BeginScrollView(this.scroll_pos);

		Dictionary<int, string> locations = this.Get_Locations();

		if (!(locations is null)) {
			List<string> location_options = new List<string>();
			foreach (KeyValuePair<int, string> pair in locations) {
				location_options.Add($"{pair.Key} - {pair.Value}");
			}

			this.location_id = EditorGUILayout.Popup("Location", this.location_id, location_options.ToArray());
		}

		this.scene = (Location_Scene) EditorGUILayout.ObjectField(
			"Location Scene",
			this.scene,
			typeof(Location_Scene),
			true
		);
		this.scene_boundary = (BoxCollider2D) EditorGUILayout.ObjectField(
			"Scene Boundary",
			this.scene_boundary,
			typeof(BoxCollider2D),
			true
		);

		EditorGUILayout.EndScrollView();
		if (GUILayout.Button("Update DB")) {
			this.Send_To_DB();
		}

		if (GUILayout.Button("Connect to DB")) {
			this.db = new SqliteConnection(this.db_info.connection_string);
			this.db.Open();
		}
	}

	private Dictionary<int, string> Get_Locations()
	{
		if (this.db is null) return null;
		
		SqliteCommand query = this.db.CreateCommand();
		query.CommandText = "SELECT location_id, name FROM Location";
		IDataReader result = query.ExecuteReader();

		Dictionary<int, string> locations = new Dictionary<int, string>();
		while (result.Read()) {
			int id = result.GetInt32(0);
			string loc_name = result.GetString(1);
			locations[id] = loc_name;
		}

		return locations;
	}

	private void Send_To_DB()
	{
		// Get collider data from location
		Server_Collider boundary = new Server_Collider
		{
			Width = this.scene_boundary.size.x * this.scene.transform.localScale.x,
			Height = this.scene_boundary.size.y * this.scene.transform.localScale.y,
			X = this.scene_boundary.bounds.min.x, // use min bounds because resolv considers position to be bottom left of the object
			Y = this.scene_boundary.bounds.min.y // use min bounds because resolv considers position to be bottom left of the object
		};
		Scene_Data server_colliders = new Scene_Data
		{
			Boundary = boundary,
			Colliders = new List<Server_Collider>(),
			Exits = new List<Server_Collider>(),
			Encounters = new List<Server_Encounter>(),
			Pickups = new List<Server_Pickups>()
		};
		foreach (Transform transform in this.scene.transform) {
			if (transform == this.scene.transform) continue;
			
			var obj = transform.gameObject;
			BoxCollider2D collider = obj.GetComponent<BoxCollider2D>();
			if (collider is null) continue;

			bool collider_prev = collider.enabled;
			collider.enabled = true;
			Server_Collider server_collider = new Server_Collider
			{
				Width = collider.size.x * this.scene.transform.localScale.x,
				Height = collider.size.y * this.scene.transform.localScale.y,
				X = collider.bounds.min.x,// use min bounds because resolv considers position to be bottom left of the object
				Y = collider.bounds.min.y, // use min bounds because resolv considers position to be bottom left of the object
				Tag = collider.tag
			};
			
			switch (collider.tag) {
				case "SOLID":
					server_colliders.Colliders.Add(server_collider);
					break;
				
				case "EXIT":
					server_colliders.Exits.Add(server_collider);
					break;
			}
			
			collider.enabled = collider_prev;
		}
		string json_colliders = JsonUtility.ToJson(server_colliders);

		// Run update query on db
		SqliteCommand query = this.db.CreateCommand();
		query.CommandText = "UPDATE Location_Scene\n"+
		                    $"SET scene_data = '{json_colliders}'\n"+
		                    $"WHERE location = {this.location_id + 1};";
		
		IDataReader result = query.ExecuteReader();
	}
}
