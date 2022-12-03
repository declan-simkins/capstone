using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class Controller_Exploration : MonoBehaviour
{
	[SerializeField] private Character_Info character;
	[SerializeField] private Motor_Exploration motor;
	[SerializeField] private Network_Connection network;

	private Vector2 move_direction;

	private struct Move_Payload
	{
		public int Character_ID;
		public Vector2 Direction;
	}

	private void Update()
	{
		this.Move(this.move_direction);
	}
	
	public void Move(Vector2 direction)
	{
		if (direction.x == 0 && direction.y == 0) return;
		
		Move_Payload payload = new Move_Payload
		{
			Character_ID = this.character.Character_ID,
			Direction = direction
		};
		this.network.Queue_Message_Type(PACKET_TYPE.MOVE, payload);
		this.motor.Move(direction);
	}

	public void Move_Up(InputAction.CallbackContext context)
	{
		if (context.started) {
			this.move_direction += new Vector2(0, 1);
		}
		else if (context.canceled) {
			this.move_direction -= new Vector2(0, 1);
		}
	}
	
	public void Move_Down(InputAction.CallbackContext context)
	{
		if (context.started) {
			this.move_direction += new Vector2(0, -1);
		}
		else if (context.canceled) {
			this.move_direction -= new Vector2(0, -1);
		}
	}
	
	public void Move_Left(InputAction.CallbackContext context)
	{
		if (context.started) {
			this.move_direction += new Vector2(-1, 0);
		}
		else if (context.canceled) {
			this.move_direction -= new Vector2(-1, 0);
		}
	}
	
	public void Move_Right(InputAction.CallbackContext context)
	{
		if (context.started) {
			this.move_direction += new Vector2(1, 0);
		}
		else if (context.canceled) {
			this.move_direction -= new Vector2(1, 0);
		}
	}
}
