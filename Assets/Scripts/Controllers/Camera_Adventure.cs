using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Camera_Adventure : MonoBehaviour
{
	[SerializeField] private Camera main_camera;
	[SerializeField] private Controller_Adventure target;

	private Vector2 mouse_pos = new Vector2();

	private void Start()
	{
		Vector3 target_xy = new Vector3(
			this.target.transform.position.x,
			this.target.transform.position.y,
			this.transform.position.z
		);
		this.transform.position = target_xy;
	}

	public void Mouse_Moved(InputAction.CallbackContext context)
	{
		if (!context.performed) return;

		this.mouse_pos = context.ReadValue<Vector2>();
	}
	
	public void Mouse_Clicked(InputAction.CallbackContext context)
	{
		if (!context.performed) return;

		Vector2Int screen_center = new Vector2Int(Screen.width / 2, Screen.height / 2);
		Vector2 delta = this.mouse_pos - screen_center;
		this.main_camera.transform.Translate(delta);
	}
}
