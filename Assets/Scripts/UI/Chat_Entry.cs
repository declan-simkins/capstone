using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Chat_Entry : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI content_text;
	[SerializeField] private TextMeshProUGUI sender_text;

	public void Populate(string content, string sender)
	{
		this.content_text.text = content;
		this.sender_text.text = sender;
	}
}
