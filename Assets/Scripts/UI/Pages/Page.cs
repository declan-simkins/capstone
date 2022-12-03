using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class Page : MonoBehaviour
	{
		// TODO: Add tool for naming UI Pages so that uniqueness is enforced
		
		#region Public Fields
		[Tooltip("Name of this UI Page; must be unique to this scene. Will add" +
		         "tool for naming UI Pages so that uniqueness is enforced.")]
		public string page_name;

		public Button close_button;
		#endregion Public Fields

		
		#region Private Fields
		private static int next_id = 0;
		#endregion Private Fields

		
		#region Protected Methods
		protected static string Generate_Page_Name(string page_title)
		{
			string name = "PAGE_" + page_title + Page.next_id;
			Page.next_id++;
			return name;
		}
		#endregion Protected Methods
	}
}
