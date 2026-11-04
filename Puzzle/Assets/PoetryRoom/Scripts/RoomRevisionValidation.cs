using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
public sealed class RoomRevisionValidation:MonoBehaviour
{
 public void Run(){StartCoroutine(Check());}
 IEnumerator Check(){
  var v=FindFirstObjectByType<RoomViewController>();var results=new List<string>();string dir=Path.GetFullPath(Application.dataPath+"/../../UnityValidation");
  float pitch=v.pitch,yaw=v.yaw;
  foreach(float p in new[]{-120f,-30f,0f,90f,140f,270f,390f}){v.pitch=p;v.SetOrbit(yaw);yield return null;results.Add("free pitch "+p+": "+(Mathf.Abs(v.pitch-p)<.001f));}
  v.pitch=pitch;v.SetOrbit(yaw);yield return null;
  InputSystem.QueueStateEvent(Mouse.current,new MouseState{position=new Vector2(500,350),delta=new Vector2(60,20)}.WithButton(MouseButton.Right));yield return null;yield return null;
  InputSystem.QueueStateEvent(Mouse.current,new MouseState{position=new Vector2(500,350)});yield return null;
  var pos=v.viewCamera.transform.position;var rot=v.viewCamera.transform.rotation;
  yield return new WaitForSecondsRealtime(.3f);results.Add("no rotation inertia: "+(Vector3.Distance(pos,v.viewCamera.transform.position)<.0001f&&Quaternion.Angle(rot,v.viewCamera.transform.rotation)<.001f));
  var parts=FindObjectsByType<RoomOpenable>(FindObjectsSortMode.None);
  foreach(var target in v.targets){
   v.ReturnToOverview();for(int angle=0;angle<360&&!target.Available;angle+=30){v.SetOrbit(angle);yield return null;}
   v.Focus(target);yield return new WaitForSecondsRealtime(.85f);
   RoomPlayValidation.Capture(v,"revised_focus_"+System.Array.IndexOf(v.targets,target),dir);
   foreach(var part in parts.Where(p=>p.owner==target)){
    var closed=part.transform.localPosition;var closedRot=part.transform.localRotation;
    for(int state=0;state<2;state++){
     var collider=part.GetComponentsInChildren<Collider>().First(c=>c.enabled);
     Vector3 screen=v.viewCamera.WorldToScreenPoint(collider.bounds.center);
     InputSystem.QueueStateEvent(Mouse.current,new MouseState{position=screen}.WithButton(MouseButton.Left));yield return null;yield return null;
     InputSystem.QueueStateEvent(Mouse.current,new MouseState{position=screen});yield return new WaitForSecondsRealtime(.7f);
     results.Add(part.name+(state==0?" mouse open: ":" mouse close: ")+(part.IsOpen==(state==0)));
     if(state==0)RoomPlayValidation.Capture(v,"open_"+part.name.Replace(' ','_'),dir);
    }
    results.Add(part.name+" restored pose: "+(Vector3.Distance(closed,part.transform.localPosition)<.002f&&Quaternion.Angle(closedRot,part.transform.localRotation)<.2f));
   }
   InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.Escape));yield return null;yield return null;InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());yield return null;
   bool restored=v.targets.Where(t=>t!=target).All(t=>t.surfaces.Where(r=>!v.walls.Any(w=>w.renderers.Contains(r))).All(r=>r.enabled));
   results.Add(target.label+" Esc restore: "+(v.Focused==null&&v.VisibleWalls<=2&&restored));
  }
  v.pitch=pitch;v.SetOrbit(yaw);File.WriteAllLines(dir+"/revision_validation.txt",results);Debug.Log("REVISION_VALIDATION_DONE");
 }
}
