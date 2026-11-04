using UnityEditor;
using UnityEngine;
public static class EndingPortalValidationMenu {
 [MenuItem("Tools/Poetry Room/Preview Memorial Website")]
 public static void Preview(){Application.OpenURL(new System.Uri(System.IO.Path.Combine(Application.streamingAssetsPath,"OurStory","index.html")).AbsoluteUri);}
 [MenuItem("Tools/Poetry Room/Validate Ending Portal")]
 public static void Run(){if(!Application.isPlaying)throw new System.Exception("Enter Play first");new GameObject("Ending validation").AddComponent<EndingPortalValidation>().Run();}
}
