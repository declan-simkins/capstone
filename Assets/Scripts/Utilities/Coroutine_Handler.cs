using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Coroutine_Handler : MonoBehaviour
{
	private static Coroutine_Handler instance;

	public static Coroutine_Handler Instance
	{
		get
		{
			if (instance != null) return instance;
			
			GameObject obj = new GameObject();
			obj.name = "Coroutine Handler";
			instance = obj.AddComponent<Coroutine_Handler>();
			return instance;
		}
	}

	private void Awake()
	{
		if (instance != null && instance != this) {
			Destroy(this.gameObject);
		}
		else {
			instance = this;
		}
	}
}
