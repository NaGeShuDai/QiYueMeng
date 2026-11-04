using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
public sealed class BedsideFixValidation:MonoBehaviour {
 PoetryPuzzleGame g;RoomViewController v;List<string> results=new List<string>();string dir,snapshot;bool persist;
 public void Run(){StartCoroutine(RunTest());}
 void Check(bool ok,string name){results.Add((ok?"PASS ":"FAIL ")+name);if(!ok)throw new Exception(name);}
 IEnumerator RunTest(){
  g=FindFirstObjectByType<PoetryPuzzleGame>();v=g.view;dir=Path.GetFullPath(Application.dataPath+"/../../UnityValidation");snapshot=JsonUtility.ToJson(g.State);persist=g.persist;g.persist=false;
  var stack=new Stack<IEnumerator>();stack.Push(Scenario());
  while(stack.Count>0){bool next;object current=null;try{next=stack.Peek().MoveNext();if(next)current=stack.Peek().Current;}catch(Exception e){results.Add("EXCEPTION "+e);break;}if(!next){stack.Pop();continue;}if(current is IEnumerator nested){stack.Push(nested);continue;}yield return current;}
  v.ReturnToOverview();g.RestoreSnapshot(snapshot);g.firstDrawer.SetOpen(false);g.persist=persist;v.pitch=34;v.SetOrbit(225);v.Snap();
  File.WriteAllLines(dir+"/bedside_fix_validation.txt",results);Debug.Log("BEDSIDE_VALIDATION_DONE "+string.Join(" | ",results));
 }
 IEnumerator Click(Vector3 world){var p=v.viewCamera.WorldToScreenPoint(world);Check(p.z>0&&p.x>0&&p.x<Screen.width&&p.y>0&&p.y<Screen.height,"click target inside viewport");
  InputSystem.QueueStateEvent(Mouse.current,new MouseState{position=p}.WithButton(MouseButton.Left));yield return null;yield return null;InputSystem.QueueStateEvent(Mouse.current,new MouseState{position=p});yield return null;yield return null;
 }
 IEnumerator Capture(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(dir+"/"+name+".png");yield return null;}
 IEnumerator Handle(){
  var face=g.firstDrawer.GetComponentsInChildren<Renderer>().First(r=>r.name.StartsWith("Locked bedside drawer"));
  yield return Click(face.bounds.center+Vector3.forward*face.bounds.extents.z);
  yield return new WaitForSecondsRealtime(.65f);
 }
 IEnumerator Scenario(){
  yield return null;Check(!g.firstDrawer.IsOpen,"loaded progress starts with drawer closed");
  if(g.State.solved[0]){
   v.SetOrbit(225);yield return null;Check(v.Focus(g.firstDrawer.owner),"loaded progress cabinet focus");v.Snap();yield return null;
   int before=g.State.digits[0];yield return Click(g.Node(0,"DigitWheel_0").TransformPoint(new Vector3(0,0,.027f)));
   Check(g.State.digits[0]==(before+1)%10&&!g.firstDrawer.IsOpen,"unlocked dial still rotates without reopening drawer");v.ReturnToOverview();
  }
  g.NewGame();yield return new WaitForSecondsRealtime(.65f);var closed=g.firstDrawer.transform.localPosition;
  Check(!g.firstDrawer.IsOpen,"fresh game drawer is closed");
  v.SetOrbit(225);yield return null;Physics.SyncTransforms();
  var old=g.firstDrawer.owner;var screen=v.viewCamera.WorldToScreenPoint(old.transform.position);
  Check(v.Pick(screen)==g.stations[0].focus,"clicking cabinet body selects lock focus");
  yield return Click(old.transform.position);yield return new WaitForSecondsRealtime(.9f);Check(v.Focused==g.stations[0].focus,"real overview click enters shared focus");
  yield return Handle();Check(!g.firstDrawer.IsOpen,"locked drawer cannot be pulled before code");
  yield return Capture("bedside_fixed_closed");
  for(int i=0;i<3;i++){
   var wheel=g.Node(0,"DigitWheel_"+i);var point=wheel.TransformPoint(new Vector3(0,0,.027f));
   for(int j=0;j<=i;j++)yield return Click(point);
   Check(g.State.digits[i]==i+1,"physical wheel "+i+" reaches correct digit");
  }
  yield return new WaitForSecondsRealtime(.65f);Check(g.State.solved[0]&&g.firstDrawer.IsOpen,"correct code automatically opens drawer once");yield return Capture("bedside_fixed_open");
  yield return Handle();Check(!g.firstDrawer.IsOpen&&Vector3.Distance(closed,g.firstDrawer.transform.localPosition)<.002f,"drawer front click closes it to original position");
  g.Refresh();yield return new WaitForSecondsRealtime(.3f);Check(!g.firstDrawer.IsOpen,"inventory refresh does not force drawer open");
  Check(FindObjectsByType<PuzzleHotspot>(FindObjectsSortMode.None).Where(h=>h.stage==0&&h.action=="reward").All(h=>!h.Available),"closed drawer blocks reward pickup");
  yield return Handle();Check(g.firstDrawer.IsOpen,"drawer front reopens unlocked drawer");
  yield return Handle();Check(!g.firstDrawer.IsOpen,"drawer supports repeated close");
  v.ReturnToOverview();yield return null;Check(v.Focus(old)&&v.Focused==g.stations[0].focus,"legacy focus redirected to same usable camera");v.Snap();yield return Capture("bedside_fixed_final");
 }
}
