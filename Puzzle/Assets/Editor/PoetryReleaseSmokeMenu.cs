using UnityEngine;
using UnityEditor;
using System.IO;
public static class PoetryReleaseSmokeMenu {
 [MenuItem("Tools/Poetry Room/Validate Release Zoom")]
 public static void Check(){new GameObject("Zoom validation").AddComponent<PoetryReleaseSmoke>().Run(Path.GetFullPath(Application.dataPath+"/../../UnityValidation/zoom_validation.txt"),false);}
}
