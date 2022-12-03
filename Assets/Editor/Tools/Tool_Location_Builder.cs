using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEditor;
using UnityEngine;


public class Tool_Location_Builder : EditorWindow
{
	private SerializedObject sobj;
	private Vector2 loc_scroll_pos, route_scroll_pos, dest_scroll_pos;

	private DB_Info db_info;
	private SqliteConnection db;
	
	private string loc_name, loc_desc;
	private bool loc_hidden;

	private string route_name, route_desc;
	private bool route_hidden;
	private int length, complexity, danger;

	private int source_location, dest_location, dest_route;
	// List of route -> location (can populate a dropdown with all routes and locations in database)
	
	[MenuItem("Tools/Location Builder")]
	public static void ShowWindow()
	{
		GetWindow<Tool_Location_Builder>();
	}

	private void OnEnable()
	{
		this.sobj = new SerializedObject(this);
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
		this.sobj.Update();
		EditorStyles.textField.wordWrap = true;
		
		// https://forum.unity.com/threads/horizontal-line-in-editor-window.520812/
		GUIStyle horizontal_separator = new GUIStyle();
		horizontal_separator.normal.background = EditorGUIUtility.whiteTexture;
		horizontal_separator.margin = new RectOffset(0, 0, 4, 4);
		horizontal_separator.fixedHeight = 1;

		this.db_info = (DB_Info) EditorGUILayout.ObjectField("DB INFO", this.db_info, typeof(DB_Info), false);
		if (this.db is null) {
			if (GUILayout.Button("Connect to DB")) {
				if (this.db_info is null) return;
		
				this.db = new SqliteConnection(this.db_info.connection_string);
				this.db.Open();
			}

			return;
		}
		
		// Location Builder
		GUILayout.Box(GUIContent.none, horizontal_separator);
		this.Draw_Location_Builder();

		// Route Builder
		GUILayout.Box(GUIContent.none, horizontal_separator);
		this.Draw_Route_Builder();
		
		// Destination Builder
		GUILayout.Box(GUIContent.none, horizontal_separator);
		this.Draw_Destination_Builder();
		
		this.sobj.ApplyModifiedProperties();
	}

	private void Draw_Location_Builder()
	{
		this.loc_scroll_pos = EditorGUILayout.BeginScrollView(this.loc_scroll_pos);

		this.loc_name = EditorGUILayout.TextField("Location Name", this.loc_name);
		GUILayout.Label("Description");
		this.loc_desc = EditorGUILayout.TextArea(this.loc_desc, GUILayout.Height(100));
		this.loc_hidden = EditorGUILayout.Toggle("Hidden", this.loc_hidden);

		EditorGUILayout.EndScrollView();
		if (GUILayout.Button("Create Location")) {
			this.Create_Location();
		}
	}

	private void Draw_Route_Builder()
	{
		this.route_scroll_pos = EditorGUILayout.BeginScrollView(this.route_scroll_pos);

		this.route_name = EditorGUILayout.TextField("Route Name", this.route_name);
		GUILayout.Label("Route Description");
		this.route_desc = EditorGUILayout.TextArea(this.route_desc, GUILayout.Height(100));
		this.route_hidden = EditorGUILayout.Toggle("Hidden", this.route_hidden);
		this.complexity = EditorGUILayout.IntField("Complexity", this.complexity);
		this.danger = EditorGUILayout.IntField("Danger", this.danger);
		this.length = EditorGUILayout.IntField("Length", this.length);
		
		EditorGUILayout.EndScrollView();
		if (GUILayout.Button("Create Route")) {
			this.Create_Route();
		}
	}

	private void Draw_Destination_Builder()
	{
		this.dest_scroll_pos = EditorGUILayout.BeginScrollView(this.dest_scroll_pos);

		// Source location dropdown
		Dictionary<int, string> locations = this.Get_Locations();
		List<string> location_options = new List<string>();
		if (!(locations is null)) {
			foreach (KeyValuePair<int, string> pair in locations) {
				location_options.Add($"{pair.Key} - {pair.Value}");
			}

			this.source_location = EditorGUILayout.Popup("Location", this.source_location, location_options.ToArray());
		}
		
		// Existing destinations and routes
		GUILayout.Label("Existing Destinations:");
		var destinations = this.Get_Destinations(this.source_location + 1, out var routes);

		int[] d_keys = new int[destinations.Count];
		destinations.Keys.CopyTo(d_keys, 0);
		int[] r_keys = new int[routes.Count];
		routes.Keys.CopyTo(r_keys, 0);
		for (int i = 0; i < destinations.Count; i++) {
			int d_id = d_keys[i];
			string d_name = destinations[d_id];
			int r_id = r_keys[i];
			string r_name = routes[r_id];

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label($"{d_name} via {r_name}");
			if (GUILayout.Button("Delete")) {
				this.Delete_Destination(this.source_location + 1, d_id, r_id);
			}
			EditorGUILayout.EndHorizontal();
		}
		
		// Route dropdown
		List<string> all_routes = this.Get_Routes();
		this.dest_route = EditorGUILayout.Popup("Route", this.dest_route, all_routes.ToArray());
		
		// Destination location dropdown TODO: Exclude source location
		this.dest_location = EditorGUILayout.Popup("Destination", this.dest_location, location_options.ToArray());
		
		EditorGUILayout.EndScrollView();

		if (GUILayout.Button("Add Destination")) {
			this.Add_Destination();
		}
	}

	private void Create_Location()
	{
		SqliteCommand query = this.db.CreateCommand();
		query.CommandText = $"INSERT INTO Location (name, description, hidden)\n" +
		                    $"VALUES (\"{this.loc_name}\", \"{this.loc_desc}\", {this.loc_hidden});";
		IDataReader result = query.ExecuteReader();
	}

	private void Create_Route()
	{
		SqliteCommand query = this.db.CreateCommand();
		query.CommandText = $"INSERT INTO Route (name, description, hidden, length, complexity, danger)\n" +
		                    $"VALUES (\"{this.route_name}\", \"{this.route_desc}\", {this.route_hidden}, "+
		                    $"{this.length}, {this.complexity}, {this.danger});";
		IDataReader result = query.ExecuteReader();
	}

	private void Add_Destination()
	{
		SqliteCommand query = this.db.CreateCommand();
		query.CommandText = $"INSERT INTO Location_Destination (location, route, destination)\n" +
		                    $"VALUES ({this.source_location + 1}, {this.dest_route + 1}, {this.dest_location + 1});";
		IDataReader result = query.ExecuteReader();
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

	private List<string> Get_Routes()
	{
		if (this.db is null) return null;
		
		SqliteCommand query = this.db.CreateCommand();
		query.CommandText = "SELECT route_id, name FROM Route;";
		IDataReader result = query.ExecuteReader();

		List<string> routes = new List<string>();
		while (result.Read()) {
			int id = result.GetInt32(0);
			string r_name = result.GetString(1);
			routes.Add($"{id} - {r_name}");
		}

		return routes;
	}

	private Dictionary<int, string> Get_Destinations(int source_location, out Dictionary<int, string> routes)
	{
		routes = new Dictionary<int, string>();
		if (this.db is null) return null;

		SqliteCommand query = this.db.CreateCommand();

		query.CommandText = "SELECT Route.name, Route.route_id, d.name, d.dest\n" +
		                    "FROM Location_Destination JOIN Route JOIN (\n" +
								"SELECT Location.name AS name, Location.location_id AS dest\n" +
								"FROM Location_Destination JOIN Location\n" +
								"WHERE Location_Destination.destination = Location.location_id\n" +
								$"AND Location_Destination.location = {source_location}) AS d\n" +
		                    "WHERE Location_Destination.route = Route.route_id\n" +
		                    "AND Location_Destination.destination = d.dest\n" +
		                    $"AND Location_Destination.location = {source_location}";
		IDataReader result = query.ExecuteReader();

		Dictionary<int, string> destinations = new Dictionary<int, string>();
		while (result.Read()) {
			string r_name = result.GetString(0);
			int r_id = result.GetInt32(1);
			string d_name = result.GetString(2);
			int d_id = result.GetInt32(3);

			routes[r_id] = r_name;
			destinations[d_id] = d_name;
		}

		return destinations;
	}

	private void Delete_Destination(int source, int dest, int route)
	{
		SqliteCommand query = this.db.CreateCommand();
		query.CommandText = $"DELETE FROM Location_Destination\n" +
		                    $"WHERE location = {source} AND destination = {dest} AND route = {route};";
		IDataReader result = query.ExecuteReader();
	}
}
