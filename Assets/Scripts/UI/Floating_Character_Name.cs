using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Floating_Character_Name : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI text;
	[SerializeField] private Vector2 offset;

	private GameObject target;
	private Character_Info info;

	public void Initialize(GameObject init_target, Character_Info init_info)
	{
		this.target = init_target;
		this.info = init_info;
		this.transform.position = this.target.transform.position + (Vector3) this.offset;
	}

	private void Start()
	{
		if (this.target is null) return;
		this.Refresh_Text();
		this.transform.position = this.target.transform.position + (Vector3) this.offset;
	}

	private void Refresh_Text()
	{
		this.text.text = this.info.Character_Name;
	}

	private void LateUpdate()
	{
		this.transform.position = this.target.transform.position + (Vector3) this.offset;
	}
}
