using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class RoomDetailSetup {
 [MenuItem("Tools/Poetry Room/Validate Detail Views")]
 public static void Validate(){if(!Application.isPlaying)throw new Exception("Enter Play first");new GameObject("Detail validation").AddComponent<RoomDetailValidation>().Run();}
 [MenuItem("Tools/Poetry Room/Apply Visible Detail Views")]
 public static void Apply(){
  if(Application.isPlaying)throw new Exception("Stop Play first");
  var g=UnityEngine.Object.FindFirstObjectByType<PoetryPuzzleGame>();
  var river=g.stations[1].model;
  if(!river.Find("Boathouse clearance")){
   var house=new GameObject("Boathouse clearance").transform;house.SetParent(river,false);
   foreach(var node in river.Cast<Transform>().Where(t=>t!=house&&(t.name.StartsWith("Boathouse")||t.name.StartsWith("Reward boathouse")||t.name.StartsWith("Reward_Compass")||t==g.rewardSockets[1])).ToArray())node.SetParent(house,false);
   house.localPosition=new Vector3(0,-.20f,0);
  }
  if(!g.Node(1,"BoathouseRoofHinge")){
   var roof=g.Node(1,"Boathouse rebuilt roof");
   var hinge=new GameObject("BoathouseRoofHinge").transform;hinge.SetParent(roof.parent,false);hinge.localPosition=roof.localPosition+new Vector3(0,.125f,0);roof.SetParent(hinge,true);
  }
  foreach(var t in g.view.targets)t.hideWhenFocused=Array.Empty<Renderer>();
  for(int i=0;i<8;i++){
   var s=g.stations[i];var t=s.focus;var anchor=s.model.parent;
   t.hasDetailView=true;t.detailFieldOfView=42;
   // Mostly overhead, from the front: readable top plates without crossing adjacent furniture.
   Vector3 center=anchor.position+Vector3.up*(i==3?.32f:i==5?.28f:i==6?.27f:i==4?.18f:i==7?.20f:.14f);
   t.detailLookAt=center;
   float height=i==1?.95f:i==3?1.15f:i==6?.95f:.75f;
   float forward=i==1?.60f:i==3?.90f:i==6?.60f:.45f;
   t.detailPosition=center+Vector3.up*height+anchor.forward*forward;
   if(i==0){t.detailLookAt=anchor.position+Vector3.up*.11f;t.detailPosition=t.detailLookAt+Vector3.up*.70f+anchor.forward*.38f;t.detailFieldOfView=40;}
   if(i==4||i==7){t.detailPosition=center+Vector3.up*.85f+anchor.forward*.50f;}
   if(i==1)t.detailFieldOfView=48;
   if(i==3){t.detailLookAt=center+Vector3.up*.12f;t.detailPosition=t.detailLookAt+Vector3.up*1.35f+anchor.forward*1.05f;t.detailFieldOfView=48;}
  }
  // Focus no longer changes visibility. Only the established wall cutaway and ceiling rules remain.
  foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
   if(r.gameObject.layer!=30&&!g.view.ceilings.Contains(r)&&!g.view.walls.Any(w=>w.renderers.Contains(r)))r.enabled=true;
  }
  RoomFocusRepair.Repair();Debug.Log("VISIBLE_DETAIL_VIEWS_SAVED");
 }
}
