using UnityEngine;

public class Renderer_Order_By_Position : MonoBehaviour
{
	[SerializeField] private Transform sorting_point;
	[SerializeField] private bool static_object = false;
	
	private Renderer _renderer;
	private float y_offset;

	private void Awake()
	{
		this._renderer = this.GetComponent<Renderer>();
		this.y_offset = (this.transform.position - this.sorting_point.position).y;
		this.Update_Sorting_Order();
		
		if (this.static_object) {
			Destroy(this.sorting_point.gameObject);
			Destroy(this);
		}
	}

	private void LateUpdate()
	{
		this.Update_Sorting_Order();
	}

	private void Update_Sorting_Order()
	{
		this._renderer.sortingOrder = (int) -(100 * (this.transform.position.y - this.y_offset));
	}
}
