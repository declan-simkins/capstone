using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using World;

[CustomEditor(typeof(Location))]
public class Editor_Location : Editor
{
	private SerializedProperty db_info_property, location_id_property;
	private SqliteConnection db;

	[SerializeField] private int location_id;
	private string location_name, location_description;
	private string modified_name, modified_description;

	private void OnEnable()
	{
		this.db_info_property = this.serializedObject.FindProperty("db_info");
		this.location_id_property = this.serializedObject.FindProperty("Location_ID");
		
		this.Connect_To_DB();

		this.location_id = this.location_id_property.intValue - 1;
		this.Get_Location_Details(this.location_id_property.intValue, out this.location_name, out this.location_description);
	}
	
	private void OnValidate()
	{
		this.location_id_property.intValue = this.location_id + 1;
		this.Get_Location_Details(this.location_id_property.intValue, out this.location_name, out this.location_description);
	}

	public override void OnInspectorGUI()
	{
		// Dropdown of all existing locations in DB
		EditorGUILayout.PropertyField(this.db_info_property, new GUIContent("DB Info"));
		if (!(this.db is null)) {
			Dictionary<int, string> locations = this.Get_Locations();
			List<string> location_options = new List<string>();
			foreach (KeyValuePair<int, string> pair in locations) {
				location_options.Add($"{pair.Key} - {pair.Value}");
			}

			this.location_id = EditorGUILayout.Popup("Location", this.location_id, location_options.ToArray());

			// Current name and description retrieved from DB
			GUILayout.Label("Current Name");
			GUILayout.TextArea(this.location_name, EditorStyles.textField, GUILayout.ExpandHeight(true));
			GUILayout.Label("Current Description");
			GUILayout.TextArea(this.location_description, EditorStyles.textField, GUILayout.ExpandHeight(true));

			// Text fields and button to update name and description
			this.modified_name = EditorGUILayout.TextField("Update Name", this.modified_name);
			GUILayout.Label("Update Description");
			this.modified_description = EditorGUILayout.TextArea(this.modified_description, GUILayout.Height(100));
			if (GUILayout.Button("Update Name and Description")) {
				this.Update_Location();
			}
		}

		if (GUILayout.Button("Connect to DB")) {
			this.Connect_To_DB();
		}

		if (GUI.changed) {
			this.OnValidate();
		}

		this.serializedObject.ApplyModifiedProperties();
	}

	private void Connect_To_DB()
	{
		DB_Info db_info = (DB_Info) this.db_info_property.objectReferenceValue;
		if (!(db_info is null)) {
			this.db = new SqliteConnection(db_info.connection_string);
			this.db.Open();
		}
	}
	
	private Dictionary<int, string> Get_Locations()
	{
		if (this.db is null) return null;
		
		SqliteCommand query = this.db.CreateCommand();
		query.CommandText = "SELECT location_id, name FROM Location;";
		IDataReader result = query.ExecuteReader();

		Dictionary<int, string> locations = new Dictionary<int, string>();
		while (result.Read()) {
			int id = result.GetInt32(0);
			string loc_name = result.GetString(1);
			locations[id] = loc_name;
		}

		return locations;
	}

	private void Get_Location_Details(int loc_id, out string loc_name, out string loc_desc)
	{
		if (this.db is null || loc_id < 1) {
			loc_name = "";
			loc_desc = "";
			return;
		}
		
		SqliteCommand query = this.db.CreateCommand();
		query.CommandText = $"SELECT name, description FROM Location WHERE location_id = {loc_id};";
		IDataReader result = query.ExecuteReader();
		result.Read();
		loc_name = result.GetString(0);
		loc_desc = result.GetString(1);
	}

	private void Update_Location()
	{
		SqliteCommand query = this.db.CreateCommand();
		query.CommandText = $"UPDATE Location\n" +
		                    $"SET name = \"{this.modified_name}\", description = \"{this.modified_description}\"\n" +
		                    $"WHERE location_id = {this.location_id_property.intValue};";
		IDataReader result = query.ExecuteReader();
	}
}
