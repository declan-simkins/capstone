using UnityEngine;

public class Motor_Exploration : MonoBehaviour
{
	[SerializeField] private Rigidbody2D rb;

	// TODO: Retrieve these from the server on first connect and store in a server info sobj
	[SerializeField] private int MOVE_SPEED;

	public void Move(Vector2 direction)
	{
		Vector2 velocity = direction * (this.MOVE_SPEED * Time.deltaTime);
		this.transform.Translate(velocity);
	}

	public void Place(Vector2 position)
	{
		this.transform.SetPositionAndRotation(position, Quaternion.identity);
	}
}
