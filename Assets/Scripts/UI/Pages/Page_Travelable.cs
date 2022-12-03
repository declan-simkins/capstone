using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World;

namespace UI
{
	public abstract class Page_Travelable : Page
	{
		#region Serialized Fields
		[SerializeField] private TextMeshProUGUI title, body;
		#endregion Serialized Fields
		
		
		#region Public Fields
		public Button go_to_button;
		
		public Travelable travelable;
		#endregion Public Fields
		
		
		#region Protected Fields
		protected UI_Manager ui_manager;
		#endregion Protected Fields
		

		#region Protected Methods
		/// <summary>
		/// Adds on click methods to the close and travel buttons and
		/// adds the page to the `ui_manager`.
		/// </summary>
		protected virtual void Start()
		{
			this.ui_manager = this.travelable.ui_manager;
		
			this.close_button.onClick.AddListener(this.Close_Button_Click);
			this.go_to_button.onClick.AddListener(this.Go_To_Button_Click);

			this.page_name += this.travelable.title;
			this.page_name = Page.Generate_Page_Name(this.page_name);
		
			this.travelable.ui_manager.Add_Page(this);
			this.gameObject.SetActive(false);
		}

		/// <summary>
		/// Updates the title and description of the location's UI window.
		/// </summary>
		protected virtual void OnEnable()
		{
			if (this.travelable != null) {
				this.title.text = this.travelable.title;
				this.body.text = this.travelable.description;
			}
		}

		/// <summary>
		/// Method to be called when the close button is clicked. Toggles
		/// this page via the UI manager. 
		/// </summary>
		protected virtual void Close_Button_Click()
		{
			this.ui_manager.Toggle_Page(this.page_name);
		}
		#endregion Protected Methods

		
		#region Protected Abstract Methods
		/// <summary>
		/// Method to be called when the travel button is clicked. Currently just
		/// activates travel mode on the player and then closes the window.
		/// </summary>
		protected abstract void Go_To_Button_Click();
		#endregion Protected Abstract Methods
	}
}
