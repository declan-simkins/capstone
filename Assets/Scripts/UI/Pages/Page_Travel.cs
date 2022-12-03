namespace UI
{
	public class Page_Travel : Page_Travelable
	{
		#region Protected Overrides
		/// <summary>
		/// Method to be called when the travel button is clicked. Currently just
		/// activates travel mode on the player and then closes the window.
		/// </summary>
		protected override void Go_To_Button_Click()
		{
			this.ui_manager.Toggle_Page(this.page_name);
		}
		#endregion Protected Overrides
	}
}
