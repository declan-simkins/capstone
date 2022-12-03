using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Chat Participants", menuName = "Scriptable Objects/Chat Participants")]
public class Chat_Participants : ScriptableObject
{
	public List<int> Participant_IDs;
}
