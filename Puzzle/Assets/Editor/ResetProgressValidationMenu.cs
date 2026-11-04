using UnityEditor;
using UnityEngine;
public static class ResetProgressValidationMenu {
 [MenuItem("Tools/Poetry Room/Validate Reset Button (Clears Save)")]
 public static void Run(){if(!Application.isPlaying)throw new System.Exception("Enter Play first");new GameObject("Reset progress validation").AddComponent<ResetProgressValidation>().Run();}
}
