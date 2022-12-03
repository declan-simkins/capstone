using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEditor;
using UnityEngine;

public class Tool_Item_Builder : EditorWindow
{
	private Vector2 scroll_pos;

	private DB_Info db_info;
	private SqliteConnection db;

	private string item_name, desc;
	private int value, equip_slot;
	private float bulk;
	private Dictionary<int, int> modifiers = new Dictionary<int, int>();
	
	[MenuItem("Tools/Item Builder")]
	public static void ShowWindow()
	{
		GetWindow<Tool_Item_Builder>();
	}

	private void OnValidate()
	{
	}

	private void OnGUI()
	{
		this.db_info = (DB_Info) EditorGUILayout.ObjectField("DB Info", this.db_info, typeof(DB_Info), false);
		if (this.db is null) {
			if (GUILayout.Button("Connect to DB")) {
				this.Connect_To_DB();
			}

			return;
		}
		
		this.scroll_pos = EditorGUILayout.BeginScrollView(this.scroll_pos);

		this.item_name = EditorGUILayout.TextField("Name", this.item_name);
		GUILayout.Label("Description");
		this.desc = EditorGUILayout.TextArea(this.desc, GUILayout.ExpandHeight(true));
		this.value = EditorGUILayout.IntField("Value", this.value);
		this.bulk = EditorGUILayout.FloatField("Bulk", this.bulk);

		var equip_slots = this.Get_Equipment_Slots();
		List<string> equip_slot_options = new List<string>();
		foreach (KeyValuePair<int,string> pair in equip_slots) {
			equip_slot_options.Add($"{pair.Key} - {pair.Value}");
		}
		this.equip_slot = EditorGUILayout.Popup("Equip Slot", this.equip_slot, equip_slot_options.ToArray());
		
		GUILayout.Label("Attribute Modifiers");
		this.Draw_Attribute_Fields();

		if (GUILayout.Button("Create Item")) {
			this.Create_Item();
		}
		EditorGUILayout.EndScrollView();
		
		if (GUI.changed) this.OnValidate();
	}

	private void Create_Item()
	{
		SqliteCommand item_query = this.db.CreateCommand();
		item_query.CommandText = $"INSERT INTO Item (name, description, equip_slot, bulk, value)\n" +
		                         $"VALUES (\"{this.item_name}\", \"{this.desc}\", {this.equip_slot}, {this.bulk}, {this.value});";
		int affected = item_query.ExecuteNonQuery();

		item_query.CommandText = "SELECT last_insert_rowid();";
		Int64 item_id = (Int64) item_query.ExecuteScalar();

		SqliteCommand mod_query = this.db.CreateCommand();
		mod_query.CommandText = $"INSERT INTO Item_Modifiers (item, attribute, amount) VALUES \n";
		List<KeyValuePair<int, int>> mods = new List<KeyValuePair<int, int>>();
		foreach (KeyValuePair<int,int> pair in this.modifiers) {
			mods.Add(pair);
		}

		for (int i = 0; i < mods.Count; i++) {
			var pair = mods[i];
			mod_query.CommandText += $"({item_id}, {pair.Key}, {pair.Value})";
			if (i + 1 < mods.Count) {
				mod_query.CommandText += ",";
			}
		}
		mod_query.CommandText += ";";
		affected = mod_query.ExecuteNonQuery();
		Debug.Log("Item Created.");
	}

	private void Draw_Attribute_Fields()
	{
		SqliteCommand query = this.db.CreateCommand();
		query.CommandText = "SELECT attribute_type_id, attribute_type_name FROM ATTRIBUTE_TYPE;";
		IDataReader result = query.ExecuteReader();
		while (result.Read()) {
			int type_id = result.GetInt32(0);
			string type_name = result.GetString(1);

			if (!this.modifiers.ContainsKey(type_id)) {
				this.modifiers[type_id] = 0;
			}
			this.modifiers[type_id] = EditorGUILayout.IntField(type_name, this.modifiers[type_id]);
		}
	}

	private Dictionary<int, string> Get_Equipment_Slots()
	{
		SqliteCommand query = this.db.CreateCommand();
		query.CommandText = "SELECT equipment_slot_id, equipment_slot_name FROM EQUIPMENT_SLOT;";
		IDataReader result = query.ExecuteReader();

		Dictionary<int, string> equipment_slots = new Dictionary<int, string>();
		while (result.Read()) {
			int slot_id = result.GetInt32(0);
			string slot_name = result.GetString(1);

			equipment_slots[slot_id] = slot_name;
		}

		return equipment_slots;
	}

	private void Connect_To_DB()
	{
		if (this.db_info is null) return;
		
		this.db = new SqliteConnection(this.db_info.connection_string);
		this.db.Open();
	}
}
