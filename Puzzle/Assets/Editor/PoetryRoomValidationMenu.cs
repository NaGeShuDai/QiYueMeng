using UnityEditor;
using UnityEngine;
public static class PoetryRoomValidationMenu
{
 [MenuItem("Tools/Poetry Room/Validate Play")]
 public static void Run(){if(!Application.isPlaying)throw new System.Exception("Enter Play first");new GameObject("Play Validation").AddComponent<RoomPlayValidation>().Run();}
}
