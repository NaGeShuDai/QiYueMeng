using UnityEditor;
using UnityEngine;
public static class PoetryClueValidationMenu {
 [MenuItem("Tools/Poetry Room/Validate Poetry Clues")]
 public static void Run(){new GameObject("Poetry clue validation").AddComponent<PoetryClueValidation>().Run();}
}
