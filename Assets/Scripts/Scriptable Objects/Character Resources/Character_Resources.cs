using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu(fileName = "New Resources", menuName = "Scriptable Objects/Resources")]
public class Character_Resources : ScriptableObject, ISerializationCallbackReceiver
{
	#region Constants
	private const int DEFAULT_MAX = 100;
	private const int DEFAULT_MIN = 0;
	private const int DEFAULT_CURRENT = DEFAULT_MAX;
	private const int DEFAULT_REGEN_RATE = 1;
	private const bool DEFAULT_REGEN = false;
	#endregion Constants
	

	#region Private Fields
	private Dictionary<RESOURCES, Resource> resources = new Dictionary<RESOURCES, Resource>();
	#endregion Private Fields
	
	
	#region Serialization Fields
	[HideInInspector, SerializeField] private List<RESOURCES> _ser_resources_keys = new List<RESOURCES>();
	[HideInInspector, SerializeField] private List<Resource> _ser_resources_values = new List<Resource>();
	#endregion Serialization Fields

	
	private void OnEnable()
	{
		foreach (RESOURCES resource_type in Enum.GetValues(typeof(RESOURCES))) {
			Resource default_resource = CreateInstance<Resource>();
			default_resource.Resource_Type = resource_type;
			default_resource.Max = DEFAULT_MAX;
			default_resource.Min = DEFAULT_MIN;
			default_resource.Current = DEFAULT_CURRENT;
			default_resource.Regen_Rate = DEFAULT_REGEN_RATE;
			default_resource.Regen = DEFAULT_REGEN;
			
			if (this.resources.ContainsKey(resource_type)) {
				this.resources[resource_type] = default_resource;
			}
			else {
				this.resources.Add(resource_type, default_resource);
			}
		}
	}
	
	public Resource Get_Resource(RESOURCES resource_type)
	{
		return this.resources[resource_type];
	}

	public static Character_Resources Create(List<Resource> resources)
	{
		Character_Resources new_resources = CreateInstance<Character_Resources>();
		
		foreach (Resource resource in resources) {
			new_resources.resources[resource.Resource_Type] = resource;
		}

		return new_resources;
	}

	public void Reinit(List<Resource> new_resources)
	{
		foreach (Resource resource in new_resources) {
			this.resources[resource.Resource_Type] = resource;
		}
	}
	
	public void OnBeforeSerialize()
	{
		this._ser_resources_keys.Clear();
		this._ser_resources_values.Clear();

		foreach (KeyValuePair<RESOURCES, Resource> kvp in this.resources) {
			this._ser_resources_keys.Add(kvp.Key);
			this._ser_resources_values.Add(kvp.Value);
		}
	}

	public void OnAfterDeserialize()
	{
		this.resources = new Dictionary<RESOURCES, Resource>();

		for (int i = 0; i < this._ser_resources_keys.Count; i++) {
			this.resources.Add(this._ser_resources_keys[i], this._ser_resources_values[i]);
		}
	}
}
