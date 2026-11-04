using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class BedsideInteractionFix {
 [MenuItem("Tools/Poetry Room/Validate Bedside Fix")]
 public static void Validate(){if(!Application.isPlaying)throw new System.Exception("Enter Play first");new GameObject("Bedside fix validation").AddComponent<BedsideFixValidation>().Run();}
 [MenuItem("Tools/Poetry Room/Fix Bedside Interaction")]
 public static void Apply(){
  if(Application.isPlaying)throw new System.Exception("Stop Play first");
  var g=Object.FindFirstObjectByType<PoetryPuzzleGame>();var t=g.stations[0].focus;var old=g.firstDrawer.owner;
  old.focusProxy=t;
  t.surfaces=g.stations[0].model.GetComponentsInChildren<Renderer>(true).Concat(old.surfaces).Concat(g.firstDrawer.GetComponentsInChildren<Renderer>(true)).Distinct().ToArray();
  // Keep the top-facing wheels large enough to read while retaining the open drawer below.
  t.lookAt=new Vector3(-1.55f,.66f,-1.64f);t.cameraPosition=t.lookAt+new Vector3(0,1.40f,1.05f);t.fieldOfView=42;
  g.firstDrawer.SetOpen(false);
  g.RefreshIsolation();EditorUtility.SetDirty(g);EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveScene(g.gameObject.scene);
  File.WriteAllText("../UnityValidation/bedside_fix_setup.txt","proxy="+old.label+" -> "+t.label+"\ndrawer closed local="+g.firstDrawer.transform.localPosition+"\ntravel="+g.firstDrawer.travel+"\nlook="+t.lookAt+" camera="+t.cameraPosition+"\nsave="+PlayerPrefs.GetString("PoetryRoom.Puzzle.v1","none"));
 }
}
