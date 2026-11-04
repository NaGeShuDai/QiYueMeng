using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class PoetryPuzzlePolish {
 [MenuItem("Tools/Poetry Room/Polish Puzzle Framing")]
 public static void Run(){
  if(Application.isPlaying)throw new Exception("Stop Play first");
  var game=UnityEngine.Object.FindFirstObjectByType<PoetryPuzzleGame>();
  var stone=game.exploration.First(h=>h.item=="stone");stone.transform.position=new Vector3(.72f,.05f,-.25f);
  var bed=stone.owner;bed.lookAt=new Vector3(-.23f,.55f,-1.05f);bed.cameraPosition=new Vector3(.15f,2.8f,2.2f);bed.fieldOfView=46;
  var moon=game.exploration.First(h=>h.item=="moon");moon.transform.position=new Vector3(-1.86f,.50f,1.38f);
  moon.transform.rotation=Quaternion.Euler(0,90,0)*Quaternion.Euler(-90,0,0);
  var desk=moon.owner;desk.lookAt=new Vector3(-1.8f,.55f,.95f);desk.cameraPosition=new Vector3(.40f,1.85f,.95f);desk.fieldOfView=46;
  for(int i=0;i<8;i++){
   var s=game.stations[i];var anchor=s.model.parent;var front=anchor.forward;
   s.focus.lookAt=anchor.position+Vector3.up*(i==3?.44f:i==6?.25f:.20f);
   float distance=i==3?1.65f:i==1?1.10f:1.05f;
   s.focus.cameraPosition=s.focus.lookAt+front*distance+Vector3.up*(i==3?1.30f:1.25f);
   s.focus.fieldOfView=i==3?48:i==1?46:42;
  }
  if(!game.exitDoor){
   var all=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None);
   var door=all.First(r=>r.name.StartsWith("EXIT - hinged door"));var hinge=new GameObject("Final exit door hinge").transform;hinge.position=new Vector3(door.bounds.max.x,door.bounds.min.y,door.bounds.center.z);
   foreach(var r in all.Where(r=>r==door||r.name.StartsWith("Door recessed panel")||r.name.StartsWith("Door panel inset")||r.name.StartsWith("Door escutcheon")||r.name.StartsWith("Door knob")||r.name.StartsWith("Keyhole")))r.transform.SetParent(hinge,true);
   game.exitDoor=hinge;
  }
  game.RefreshIsolation();EditorUtility.SetDirty(game);EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);AssetDatabase.SaveAssets();
  Debug.Log("PUZZLE_FRAMING_SAVED");
 }
}
