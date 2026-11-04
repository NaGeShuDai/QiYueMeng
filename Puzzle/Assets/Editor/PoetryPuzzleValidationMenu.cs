using UnityEditor;
using UnityEngine;
public static class PoetryPuzzleValidationMenu {
 [MenuItem("Tools/Poetry Room/Validate Model Only")]
 public static void Physical(){if(!Application.isPlaying)throw new System.Exception("Enter Play first");new GameObject("Model only validation").AddComponent<PoetryPhysicalValidation>().Run();}
 [MenuItem("Tools/Poetry Room/Validate Puzzle Chain")]
 public static void Run(){if(!Application.isPlaying)throw new System.Exception("Enter Play first");new GameObject("Puzzle validation").AddComponent<PoetryPuzzleValidation>().Run();}
}
