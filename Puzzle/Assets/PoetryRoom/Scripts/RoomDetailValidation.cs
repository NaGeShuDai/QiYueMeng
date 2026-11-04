using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
public sealed class RoomDetailValidation:MonoBehaviour {
 RoomViewController v;PoetryPuzzleGame g;string dir,snapshot;bool persist;List<string> log=new List<string>();
 public void Run(){StartCoroutine(CheckAll());}
 void Check(bool pass,string label){log.Add((pass?"PASS ":"FAIL ")+label);if(!pass)throw new Exception(label);}
 IEnumerator Click(Vector3 world){Vector3 p=v.viewCamera.WorldToScreenPoint(world);InputSystem.QueueStateEvent(Mouse.current,new MouseState{position=p});yield return null;yield return null;InputSystem.QueueStateEvent(Mouse.current,new MouseState{position=p}.WithButton(MouseButton.Left));yield return null;yield return null;InputSystem.QueueStateEvent(Mouse.current,new MouseState{position=p});yield return null;yield return null;}
 IEnumerator Escape(){InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.Escape));yield return null;yield return null;InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());yield return null;}
 IEnumerator Capture(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(dir+"/"+name+".png");yield return null;}
 IEnumerator CheckAll(){
  g=FindFirstObjectByType<PoetryPuzzleGame>();v=g.view;dir=Path.GetFullPath(Application.dataPath+"/../../UnityValidation");snapshot=JsonUtility.ToJson(g.State);persist=g.persist;g.persist=false;g.NewGame();
  var stack=new Stack<IEnumerator>();stack.Push(Scenario());while(stack.Count>0){bool next;object value=null;try{next=stack.Peek().MoveNext();if(next)value=stack.Peek().Current;}catch(Exception e){log.Add("EXCEPTION "+e);break;}if(!next){stack.Pop();continue;}if(value is IEnumerator nested)stack.Push(nested);else yield return value;}
  v.ReturnToOverview();g.RestoreSnapshot(snapshot);g.firstDrawer.SetOpen(false);g.persist=persist;v.pitch=34;v.SetOrbit(225);v.Snap();File.WriteAllLines(dir+"/detail_validation.txt",log);Debug.Log("DETAIL_VALIDATION_DONE "+log.Count+" checks");
 }
 IEnumerator Scenario(){
  g.State.solved[3]=true;g.State.solved[6]=true;g.Refresh();
  var visible=FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r=>r.enabled&&r.gameObject.layer!=30&&!v.ceilings.Contains(r)&&!v.walls.Any(w=>w.renderers.Contains(r))).ToArray();
  for(int i=0;i<8;i++){
   v.ReturnToOverview();var s=g.stations[i];bool reachable=false;
   for(int angle=0;angle<360;angle+=45){v.SetOrbit(angle);yield return null;Physics.SyncTransforms();var p=v.viewCamera.WorldToScreenPoint(s.focus.transform.position);if(p.z>0&&p.x>0&&p.x<Screen.width&&p.y>0&&p.y<Screen.height&&!g.BlocksPointer(p)&&v.Pick(p)==s.focus){reachable=true;break;}}
   Check(reachable,"overview ray reaches moved puzzle "+i);yield return Click(s.focus.transform.position);Check(v.Focused==s.focus,"mouse click focuses station "+i+" actual="+(v.Focused?v.Focused.label:"none"));v.Snap();yield return null;
   var inward=Vector3.ProjectOnPlane(v.roomCenter-s.model.position,Vector3.up);
   Check(Vector3.Dot(Vector3.ProjectOnPlane(s.focus.CameraPosition-s.focus.LookAt,Vector3.up),inward)>0,"context camera on room interior side "+i);
   Check(Vector3.Dot(Vector3.ProjectOnPlane(s.focus.DetailPosition-s.focus.DetailLookAt,Vector3.up),inward)>0,"detail camera on room interior side "+i);
   Vector3 initial=v.viewCamera.transform.position;int digit=g.State.digits[0];
   yield return Click(i==0?g.Node(0,"DigitWheel_0").TransformPoint(new Vector3(0,0,.027f)):s.focus.transform.position);
   yield return new WaitForSecondsRealtime(.85f);
   Check(v.IsDetail&&v.Focused==s.focus,"second click enters detail "+i);
   Check(Vector3.Distance(initial,v.viewCamera.transform.position)>.1f,"camera moves closer "+i);
   Check(visible.All(r=>!r||r.enabled),"other room objects remain visible "+i);
   Check(v.VisibleWalls<=2&&v.ceilings.All(r=>!r.enabled),"wall cutaway preserved "+i);
   yield return Capture("detail_station_"+i);
   if(i==0){Check(g.State.digits[0]==digit,"zoom click does not turn wheel");yield return Click(g.Node(0,"DigitWheel_0").TransformPoint(new Vector3(0,0,.027f)));Check(g.State.digits[0]==(digit+1)%10,"detail wheel click operates model");}
   yield return Escape();Check(!v.IsDetail&&v.Focused==s.focus,"first Escape returns to context "+i);
   yield return Escape();Check(!v.Focused,"second Escape returns to room "+i);
  }
  var river=g.stations[1];var originalPosition=river.model.position;var originalRotation=river.model.rotation;
  var localHit=river.model.InverseTransformPoint(river.focus.transform.position);var localCamera=river.model.InverseTransformPoint(river.focus.DetailPosition);
  try{
   river.model.position+=new Vector3(.3f,0,.2f);river.model.rotation=Quaternion.AngleAxis(180,Vector3.up)*originalRotation;Physics.SyncTransforms();
   Check(Vector3.Distance(river.focus.transform.position,river.model.TransformPoint(localHit))<.001f,"focus hitbox follows future model move and rotation");
   Check(Vector3.Distance(river.focus.DetailPosition,river.model.TransformPoint(localCamera))<.001f,"detail camera follows future model move and rotation");
  }finally{river.model.SetPositionAndRotation(originalPosition,originalRotation);Physics.SyncTransforms();}
 }
}
