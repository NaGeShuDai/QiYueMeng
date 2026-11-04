using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class RoomFocusRepair {
 [MenuItem("Tools/Poetry Room/Repair Focus Anchors")]
 public static void Repair(){
  if(Application.isPlaying)throw new Exception("Stop Play before repairing scene anchors");
  var g=UnityEngine.Object.FindFirstObjectByType<PoetryPuzzleGame>();
  for(int i=0;i<g.stations.Length;i++){
   var s=g.stations[i];var t=s.focus;Undo.RecordObject(t,"Repair puzzle focus");Undo.RecordObject(t.transform,"Repair puzzle focus");
   Vector3 front=Vector3.ProjectOnPlane(s.model.TransformDirection(Vector3.down),Vector3.up).normalized;
   Vector3 inward=Vector3.ProjectOnPlane(g.view.roomCenter-s.model.position,Vector3.up);
   if(Vector3.Dot(front,inward)<0)front=-front;
   if(i!=0){
    float height=t.cameraPosition.y-t.lookAt.y;
    float distance=Vector3.ProjectOnPlane(t.cameraPosition-t.lookAt,Vector3.up).magnitude;
    t.lookAt=s.model.position+Vector3.up*(i==3?.45f:i==6?.25f:.20f);
    t.cameraPosition=t.lookAt+front*distance+Vector3.up*height;
   }
   float detailHeight=t.detailPosition.y-t.detailLookAt.y;
   float detailDistance=Vector3.ProjectOnPlane(t.detailPosition-t.detailLookAt,Vector3.up).magnitude;
   t.detailLookAt=s.model.position+Vector3.up*(i==0?.11f:i==3?.44f:i==5?.28f:i==6?.27f:i==4?.18f:i==7?.20f:.14f);
   t.detailPosition=t.detailLookAt+front*detailDistance+Vector3.up*detailHeight;
   t.BindViewPose(s.model);
   // The collider now lives in model space, so moving either the station or FBX keeps it aligned.
   var bounds=new Bounds();bool first=true;
   foreach(var filter in s.model.GetComponentsInChildren<MeshFilter>(true)){
    if(!filter.sharedMesh)continue;var b=filter.sharedMesh.bounds;
    for(int corner=0;corner<8;corner++){
     var p=b.center+Vector3.Scale(b.extents,new Vector3((corner&1)==0?-1:1,(corner&2)==0?-1:1,(corner&4)==0?-1:1));
     p=s.model.InverseTransformPoint(filter.transform.TransformPoint(p));if(first){bounds=new Bounds(p,Vector3.zero);first=false;}else bounds.Encapsulate(p);
    }
   }
   Undo.SetTransformParent(t.transform,s.model,"Attach focus to puzzle");t.transform.localPosition=bounds.center;t.transform.localRotation=Quaternion.identity;t.transform.localScale=Vector3.one;
   var box=t.GetComponent<BoxCollider>();Undo.RecordObject(box,"Repair focus collider");box.center=Vector3.zero;box.size=bounds.size+Vector3.one*.035f;box.enabled=t.Available;
   EditorUtility.SetDirty(t);
  }
  EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveScene(g.gameObject.scene);Inspect();Debug.Log("FOCUS_ANCHORS_REPAIRED");
 }
 [MenuItem("Tools/Poetry Room/Inspect Focus Anchors")]
 public static void Inspect(){
  var g=UnityEngine.Object.FindFirstObjectByType<PoetryPuzzleGame>();var report=new StringBuilder();
  report.AppendLine("playing="+Application.isPlaying+" room="+g.view.roomCenter);
  for(int i=0;i<g.stations.Length;i++){
   var s=g.stations[i];var t=s.focus;var c=t.GetComponent<BoxCollider>();
   report.AppendLine($"{i} {t.label} available={t.Available} focus={t.transform.position} collider={c.bounds} enabled={c.enabled} active={t.gameObject.activeInHierarchy} layer={t.gameObject.layer} camera={t.cameraPosition} look={t.lookAt} detail={t.detailPosition}");
   for(var p=s.model;p;p=p.parent)report.AppendLine($"  {p.name} pos={p.position} rot={p.eulerAngles} localRot={p.localEulerAngles} scale={p.lossyScale}");
   foreach(var r in s.model.GetComponentsInChildren<Renderer>())report.AppendLine($"  R {r.name} bounds={r.bounds} enabled={r.enabled}");
  }
  File.WriteAllText(Path.GetFullPath(Application.dataPath+"/../../UnityValidation/focus_anchors.txt"),report.ToString());
 }
}
