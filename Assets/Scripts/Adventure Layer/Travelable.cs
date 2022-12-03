using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace World
{
	public abstract class Travelable : MonoBehaviour
	{
		#region Events and Delegates
		public delegate void Discovered_Handler(Travelable travelable);
		public static event Discovered_Handler Discovered;
		#endregion
	
		
		#region Serialized Fields
		[Header("LOCATION INFO")]
		[Tooltip("Whether or not this location will be hidden to the player")]
		[SerializeField] protected bool hidden = false;
		#endregion Serialized Fields
		
		
		#region Public Fields
		[Header("PLAYER PAWN")]
		[Tooltip("Where the player's pawn will be placed when at this location")]
		public Transform pawn_position;

		[Header("WORLD BUTTON")]
		public Button world_button;

		[Header("UI")]
		public GameObject scene_ui;
		public UI_Manager ui_manager;
		public GameObject ui_window_prefab;
		public UnityEvent on_world_button_click;
		
		[Tooltip("Title that will be shown in the UI Window")]
		public string title;
		
		[Tooltip("Description that will be shown in the UI Window")]
		[TextArea(8, 20)]
		public string description;
		#endregion Public Fields
		
		
		#region Protected Fields
		protected Page_Travelable _Page;
		#endregion Protected Fields
		
		
		#region Properties
		public bool Hidden => this.hidden;
		#endregion Properties
		

		#region Protected Methods
		protected virtual void World_Space_Button_Setup()
		{
			for (int i = 0; i < this.on_world_button_click.GetPersistentEventCount(); i++) {
				this.world_button.onClick.AddListener(this.on_world_button_click.Invoke);
			}
		}
		#endregion Protected Methods
		
		
		#region Private Methods
		// Event Handlers
		private void On_Player_Travelling(Location from, Location to)
		{
			if (to != this) {
				return;
			}
			
			this.gameObject.SetActive(true);
			this.hidden = false;
			Travelable.Discovered?.Invoke(this);
		}
		#endregion Private Method
	}
}
