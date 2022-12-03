using UnityEngine;

namespace World
{
	public class Route : Travelable
	{
		#region Enums
		public enum Complexities : int
		{
			LOW = 0,
			MEDIUM = 1,
			HIGH = 2
		}
		#endregion Enums
		
		
		#region Static Fields
		private static string LOW_COMPLEXITY_STRING = "A simple paved road; I will likely make good time.";

		private static string MEDIUM_COMPLEXITY_STRING = "This road has seen better days; it will likely take" +
		                                                 "longer than expected to travel it.";

		private static string HIGH_COMPLEXITY_STRING = "Nothing more than a small wilderness trail... Better than" +
		                                               "bushwhacking, but it will take a while to traverse.";
		#endregion Static Fields
		
		
		#region Serialized Fields
		[Header("ROUTE")]
		[SerializeField] protected int length;
		[SerializeField] protected int danger;
		[SerializeField] protected Complexities complexity;
		#endregion Serialized Fields
		

		#region Private Fields
		private int food_cost;
		#endregion Private Fields
		
		
		#region Properties
		public int Danger => this.danger;
		public int Length => this.length;
		public Complexities Complexity => this.complexity;
		public int Food_Cost => this.food_cost;
		#endregion Properties

		
		#region Public Methods
		/// <summary>
		/// Toggles whether or not this route is visible in the game scene.
		/// </summary>
		public void Toggle_Visibility()
		{
			this.gameObject.SetActive(!this.gameObject.activeInHierarchy);
		}
		#endregion Public Methods


		#region Private Methods
		/// <summary>
		/// Gets a narrative style string that reflects the complexity of
		/// the route.
		/// </summary>
		/// <returns>
		/// Narrative style string reflective of the complexity
		/// of the route.
		/// </returns>
		private string Get_Complexity_Str()
		{
			switch ((int) this.complexity) {
				case (int) Route.Complexities.LOW: {
					return Route.LOW_COMPLEXITY_STRING;
				}

				case (int) Route.Complexities.MEDIUM: {
					return Route.MEDIUM_COMPLEXITY_STRING;
				}

				case (int) Route.Complexities.HIGH:
				default: {
					return Route.HIGH_COMPLEXITY_STRING;
				}
			}
		}
		#endregion Private Methods
	}
}