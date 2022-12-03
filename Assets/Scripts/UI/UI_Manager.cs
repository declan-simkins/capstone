using System.Collections.Generic;
using UnityEngine;

namespace UI
{
	public class UI_Manager : MonoBehaviour
	{
		#region Serialized Fields
		[SerializeField] private Page default_page;
		[SerializeField] private List<Page> pages;
		#endregion Serialized Fields


		#region Public Methods
		/// <summary>
		/// Adds new UI_Page to the UI manager. 
		/// </summary>
		/// <param name="new_page">Page to be added to the UI manager.</param>
		public void Add_Page(Page new_page)
		{
			this.pages.Add(new_page);
		}

		/// <summary>
		/// Toggles hierarchy state of page #`page_num`.
		/// </summary>
		/// <param name="page_num">Index of page to be toggled.</param>
		public void Toggle_Page(int page_num)
		{
			GameObject page_game_obj = this.pages[page_num].gameObject;
			page_game_obj.SetActive(!page_game_obj.activeSelf);
		}

		/// <summary>
		/// Toggles hierarchy state of page `page_name`.
		/// </summary>
		/// <param name="page_name">Name of page to be toggled.</param>
		public void Toggle_Page(string page_name)
		{
			{ // for loop var scope
				Page page;
				int i;

				for (i = 0, page = this.pages[0]; i < this.pages.Count; i++, page = this.pages[i]) {
					if (page.page_name == page_name) {
						this.Toggle_Page(i);
						return;
					}
				}
			} // End for loop var scope
		}
		
		public void Toggle_Page(Page page)
		{
			page.gameObject.SetActive(!page.gameObject.activeSelf);
		}

		/// <summary>
		/// Sets hierarchy state of page `page_name` to true and sets
		/// hierarchy state of all other pages to false.
		/// </summary>
		/// <param name="page_name">Name of page to be switched to.</param>
		public void Switch_To(string page_name)
		{
			foreach (Page page in this.pages) {
				page.gameObject.SetActive(page.page_name == page_name);
			}
		}

		/// <summary>
		/// Sets hierarchy state of page #`page_num` to true and sets
		/// hierarchy state of all other pages to false.
		/// </summary>
		/// <param name="page_num">Index of page to be switched to.</param>
		public void Switch_To(int page_num)
		{
			this.Switch_To(this.pages[page_num].page_name);
		}

		/// <summary>
		/// Sets hierarchy state of page #`page_num` to true and sets
		/// hierarchy state of all other pages to false.
		/// </summary>
		/// <param name="_page">UI_Page object to be switched to.</param>
		public void Switch_To(Page _page)
		{
			this.Switch_To(_page.page_name);
		}
	
		/// <summary>
		/// Get index of page with name `page_name`.
		/// </summary>
		/// <param name="page_name">Name of page to get index of.</param>
		/// <returns>
		/// Index of the specified page or -1 if no such page
		/// exists within the UI manager
		/// </returns>
		public int Get_Page_Num(string page_name)
		{
			int page_i = -1;
			for (int i = 0; i < this.pages.Count; i++) {
				if (this.pages[i].page_name == page_name) {
					page_i = i;
				}
			}

			return page_i;
		}
		#endregion Public Methods

		
		#region Private Methods
		private void Start()
		{
			this.Wake_Up_Pages();
			if (this.default_page is null) return;
			this.default_page.gameObject.SetActive(true);
		}

		private void Wake_Up_Pages()
		{
			foreach (Page page in this.pages) {
				if (!(page.close_button is null)) {
					page.close_button.onClick.AddListener(delegate { this.Toggle_Page(page); });
				}

				page.gameObject.SetActive(true);
				page.gameObject.SetActive(false);
			}
		}
		#endregion Private Methods
	}
}