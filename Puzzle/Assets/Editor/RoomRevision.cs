using System;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class RoomRevision
{
 [MenuItem("Tools/Poetry Room/Validate Revision")]
 public static void Validate(){new GameObject("Revision validation").AddComponent<RoomRevisionValidation>().Run();}
 [MenuItem("Tools/Poetry Room/Finish Revision")]
 public static void Finish(){
  var v=UnityEngine.Object.FindFirstObjectByType<RoomViewController>();
  var rs=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
  foreach(var t in v.targets)t.hideWhenFocused=rs.Where(r=>r.gameObject.layer!=30&&!t.surfaces.Contains(r)&&!r.name.StartsWith("Foundation")&&!r.name.StartsWith("Oak floorboard")).ToArray();
  var bedside=v.targets.First(t=>t.label=="床头柜转盘");bedside.cameraPosition=bedside.lookAt+Vector3.forward*2.15f+Vector3.up*1.10f;bedside.fieldOfView=46;
  EditorSceneManager.MarkSceneDirty(v.gameObject.scene);EditorSceneManager.SaveScene(v.gameObject.scene);
 }
 [MenuItem("Tools/Poetry Room/Inspect Revision")]
 public static void Inspect(){File.WriteAllLines("../UnityValidation/geometry.txt",UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r=>r.name.Contains("Wardrobe")||r.name.Contains("drawer")||r.name.Contains("Mirror")||r.name.Contains("Inset")||r.name.Contains("Overlay")||r.name.Contains("pull")).Select(r=>r.name+" | "+r.bounds.center+" | "+r.bounds.size));}
 [MenuItem("Tools/Poetry Room/Apply View Revision")]
 public static void Apply(){
  if(Application.isPlaying || UnityEngine.Object.FindFirstObjectByType<RoomOpenable>())throw new InvalidOperationException("Revision already applied or Play active; use Finish Revision for final framing.");
  var view=UnityEngine.Object.FindFirstObjectByType<RoomViewController>();
  var rs=GameObject.Find("Poetry Bedroom").GetComponentsInChildren<Renderer>();
  foreach(var target in view.targets){
   var b=target.surfaces[0].bounds;foreach(var r in target.surfaces)b.Encapsulate(r.bounds);
   Vector3 inward=Vector3.zero;
   switch(target.label){
    case "衣柜与镜子": inward=Vector3.left;target.lookAt=b.center;target.cameraPosition=b.center+inward*3.4f;break;
    case "书架排序机关": inward=Vector3.left;target.lookAt=b.center+Vector3.up*.12f;target.cameraPosition=target.lookAt+inward*2.6f+Vector3.up*.45f;break;
    case "床头柜转盘": inward=Vector3.forward;target.lookAt=b.center+Vector3.up*.23f;target.cameraPosition=target.lookAt+inward*1.7f+Vector3.up*1.05f;break;
    case "出口": inward=Vector3.back;target.lookAt=b.center;target.cameraPosition=b.center+inward*3.8f;break;
    case "墙上诗页": inward=Vector3.forward;target.lookAt=b.center;target.cameraPosition=b.center+inward*1.5f;break;
    case "书桌与信": inward=Vector3.right;target.cameraPosition=target.lookAt+inward*1.45f+Vector3.up*1.2f;break;
   }
   target.hideWhenFocused=rs.Where(r=>r.gameObject.layer!=30 && !target.surfaces.Contains(r) && !r.name.StartsWith("Foundation") && !r.name.StartsWith("Oak floorboard")).ToArray();
  }
  var wardrobe=view.targets.First(t=>t.label=="衣柜与镜子");
  // Author the formerly fixed open door as a closed, hinged assembly.
  var door=rs.First(r=>r.name.StartsWith("Wardrobe open door"));
  var parts=rs.Where(r=>r==door||r.name.StartsWith("Inset mirror")||r.name.StartsWith("Mirror brass")||r.name.StartsWith("Overlay insertion")).ToArray();
  var hinge=new GameObject("Interactive wardrobe mirror door");hinge.transform.position=new Vector3(door.bounds.max.x,door.bounds.center.y,door.bounds.center.z);
  foreach(var part in parts)part.transform.SetParent(hinge.transform,true);
  hinge.transform.Rotate(Vector3.up,-90,Space.World);
  var action=hinge.AddComponent<RoomOpenable>();action.owner=wardrobe;action.angle=110;
  foreach(var part in parts){part.gameObject.layer=28;if(!part.GetComponent<BoxCollider>())part.gameObject.AddComponent<BoxCollider>();}
  var right=rs.First(r=>r.name.StartsWith("Wardrobe closed panel"));
  var other=new GameObject("Interactive wardrobe right door");other.transform.position=new Vector3(right.bounds.center.x,right.bounds.center.y,right.bounds.min.z);
  right.transform.SetParent(other.transform,true);
  var handle=rs.First(r=>r.name.StartsWith("Wardrobe handle"));handle.transform.SetParent(other.transform,true);
  foreach(var part in new[]{right,handle}){part.gameObject.layer=28;part.gameObject.AddComponent<BoxCollider>();}
  var otherAction=other.AddComponent<RoomOpenable>();otherAction.owner=wardrobe;otherAction.angle=-110;
  foreach(var target in view.targets){
   string front=target.label=="书桌与信"?"Final drawer front":target.label=="床头柜转盘"?"Locked bedside drawer":target.label=="书架排序机关"?"Sequence reward drawer":null;
   if(front==null)continue;
   var face=rs.First(r=>r.name.StartsWith(front));
   Drawer(target,face,target.label=="书桌与信"?Vector3.right:target.label=="床头柜转盘"?Vector3.forward:Vector3.left,rs);
   if(target.label=="书桌与信")Drawer(target,rs.First(r=>r.name.StartsWith("Desk upper drawer")),Vector3.right,rs);
  }
  view.orbitSensitivity=.16f;view.Snap();EditorUtility.SetDirty(view);EditorSceneManager.MarkSceneDirty(view.gameObject.scene);EditorSceneManager.SaveScene(view.gameObject.scene);AssetDatabase.SaveAssets();
  Debug.Log("VIEW_REVISION_SAVED");
 }
 static void Drawer(RoomFocusTarget target,Renderer face,Vector3 direction,Renderer[] rs){
  var go=new GameObject("Interactive "+face.name);go.transform.position=face.bounds.center;
  face.transform.SetParent(go.transform,true);face.gameObject.layer=28;face.gameObject.AddComponent<BoxCollider>();
  foreach(var r in rs.Where(r=>(r.name.Contains("pull")||r.name.Contains("handle"))&&Vector3.Distance(r.bounds.center,face.bounds.center)<.36f&&Mathf.Abs(r.bounds.center.y-face.bounds.center.y)<.10f)){r.transform.SetParent(go.transform,true);r.gameObject.layer=28;r.gameObject.AddComponent<BoxCollider>();}
  var wood=new Material(Shader.Find("Universal Render Pipeline/Lit"));wood.color=new Color(.16f,.075f,.026f);wood.name="Drawer interior walnut";
  string path="Assets/PoetryRoom/Materials/DrawerInterior.mat";
  if(!AssetDatabase.LoadAssetAtPath<Material>(path))AssetDatabase.CreateAsset(wood,path);else {UnityEngine.Object.DestroyImmediate(wood);wood=AssetDatabase.LoadAssetAtPath<Material>(path);}
  bool alongX=Mathf.Abs(direction.x)>.5f;float width=alongX?face.bounds.size.z:face.bounds.size.x;
  Vector3 back=face.bounds.center-direction*.19f;float bottom=face.bounds.min.y+.015f;
  var added=new System.Collections.Generic.List<Renderer>();
  Action<string,Vector3,Vector3> panel=(name,pos,size)=>{var p=GameObject.CreatePrimitive(PrimitiveType.Cube);p.name=name;p.transform.position=pos;p.transform.localScale=size;p.transform.SetParent(go.transform,true);p.layer=28;p.GetComponent<Renderer>().sharedMaterial=wood;added.Add(p.GetComponent<Renderer>());};
  panel("Drawer tray bottom",new Vector3(back.x,bottom,back.z),alongX?new Vector3(.36f,.025f,width-.025f):new Vector3(width-.025f,.025f,.36f));
  for(int sign=-1;sign<=1;sign+=2){Vector3 side=alongX?Vector3.forward:Vector3.right;panel("Drawer tray side",back+side*(width*.5f-.02f),alongX?new Vector3(.36f,face.bounds.size.y*.75f,.025f):new Vector3(.025f,face.bounds.size.y*.75f,.36f));}
  target.surfaces=target.surfaces.Concat(added).ToArray();
  var a=go.AddComponent<RoomOpenable>();a.owner=target;a.travel=direction*.38f;
 }
}
