using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
public sealed class PoetryPhysicalValidation:MonoBehaviour {
 PoetryPuzzleGame g;RoomViewController v;string snapshot,dir;bool persistence;List<string> results=new List<string>();
 public void Run(){StartCoroutine(RunAll());}
 void Check(bool ok,string label){results.Add((ok?"PASS ":"FAIL ")+label);File.WriteAllLines(dir+"/model_only_validation.txt",results);if(!ok)throw new Exception(label);}
 IEnumerator RunAll(){
  g=FindFirstObjectByType<PoetryPuzzleGame>();v=g.view;dir=Path.GetFullPath(Application.dataPath+"/../../UnityValidation");snapshot=JsonUtility.ToJson(g.State);persistence=g.persist;g.persist=false;if(g.Ending)g.Ending.suppressBrowserForValidation=true;g.NewGame();
  var stack=new Stack<IEnumerator>();stack.Push(Scenario());
  while(stack.Count>0){object value=null;bool next=false;try{next=stack.Peek().MoveNext();if(next)value=stack.Peek().Current;}catch(Exception e){results.Add("EXCEPTION "+e);break;}
   if(!next){stack.Pop();continue;}if(value is IEnumerator nested){stack.Push(nested);continue;}yield return value;
  }
  v.ReturnToOverview();g.RestoreSnapshot(snapshot);g.persist=persistence;if(g.Ending)g.Ending.suppressBrowserForValidation=false;v.pitch=34;v.SetOrbit(225);v.Snap();
  File.WriteAllLines(dir+"/model_only_validation.txt",results);Debug.Log("MODEL_ONLY_VALIDATION_DONE "+results.Count+" checks, failures="+results.Count(x=>x.StartsWith("FAIL")||x.StartsWith("EXCEPTION")));
 }
 IEnumerator Focus(RoomFocusTarget target){v.ReturnToOverview();for(int i=0;i<360;i+=45){v.SetOrbit(i);yield return null;if(target.Available)break;}Check(v.Focus(target),"focus "+target.label);v.Snap();yield return null;Physics.SyncTransforms();}
 IEnumerator Focus(int stage){yield return Focus(g.stations[stage].focus);yield return MouseClick(v.viewCamera.WorldToScreenPoint(g.stations[stage].focus.transform.position));yield return new WaitForSecondsRealtime(.85f);Check(v.IsDetail,"detail view "+stage);}
 IEnumerator MouseClick(Vector2 p){InputSystem.QueueStateEvent(Mouse.current,new MouseState{position=p});yield return null;yield return null;InputSystem.QueueStateEvent(Mouse.current,new MouseState{position=p}.WithButton(MouseButton.Left));yield return null;yield return null;InputSystem.QueueStateEvent(Mouse.current,new MouseState{position=p});yield return null;yield return null;}
 IEnumerator Select(string id){int index=g.State.inventory.IndexOf(id);Check(index>=0,"inventory contains "+id);Rect r=g.InventoryRect(index);var point=new Vector2(r.center.x,Screen.height-r.center.y);yield return MouseClick(point);Check(g.Selected==id,"icon selects "+id);if(id=="winding_key"){yield return MouseClick(point);Check(g.Selected=="","repeat click deselects inventory item");yield return MouseClick(point);Check(g.Selected==id,"inventory item can be selected again");yield return Capture("right_inventory_selected");}}
 PuzzleHotspot Hot(int stage,string action,int index=0)=>FindObjectsByType<PuzzleHotspot>(FindObjectsSortMode.None).First(h=>h.stage==stage&&h.action==action&&h.index==index);
 IEnumerator Click(PuzzleHotspot h){
  Physics.SyncTransforms();Vector3 chosen=Vector3.zero;bool found=false;
  foreach(var c in h.GetComponentsInChildren<Collider>().Where(c=>c.enabled)){
   Vector3[] points={c.bounds.center,c.bounds.center+Vector3.up*c.bounds.extents.y*.8f,c.bounds.center+v.viewCamera.transform.right*c.bounds.extents.magnitude*.3f};
   foreach(var p in points){var screen=v.viewCamera.WorldToScreenPoint(p);if(screen.z<=0||screen.x<0||screen.x>Screen.width||screen.y<0||screen.y>Screen.height||g.BlocksPointer(screen))continue;
    var ray=v.viewCamera.ScreenPointToRay(screen);
    var hits=Physics.RaycastAll(ray,100,1<<27,QueryTriggerInteraction.Collide).OrderBy(x=>x.distance);
    var actual=hits.Select(x=>x.collider.GetComponentInParent<PuzzleHotspot>()).FirstOrDefault(x=>x&&x.owner==v.Focused&&x.Available&&!(x.action=="install"&&x.stage>=0&&g.State.installed[x.stage]));
    if(actual==h){chosen=screen;found=true;break;}
   }if(found)break;
  }
  Check(found,"reachable model "+h.action+" "+h.stage+" "+h.index+" "+h.item);yield return MouseClick(chosen);
 }
 IEnumerator Click(int stage,string action,int index=0){yield return Click(Hot(stage,action,index));}
 IEnumerator Rewards(int stage){
  if(stage==0&&v.IsDetail){InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.Escape));yield return null;yield return null;InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());yield return new WaitForSecondsRealtime(.7f);}
  yield return new WaitForSecondsRealtime(.65f);Physics.SyncTransforms();
  yield return Capture("physical_rewards_"+stage);
  var rewards=FindObjectsByType<PuzzleHotspot>(FindObjectsSortMode.None).Where(h=>h.action=="reward"&&h.stage==stage).ToArray();
  foreach(var h in rewards){string id=h.item;yield return Click(h);Check(g.State.Has(id)&&!g.State.pendingRewards.Contains(id),"physical reward collected "+id);}
 }
 IEnumerator Capture(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(dir+"/"+name+".png");yield return null;}
 IEnumerator Install(int stage,string item){yield return Select(item);yield return Click(stage,"install");Check(g.State.installed[stage]&&!g.State.Has(item),"physical installation "+item);}
 IEnumerator Scenario(){
  Check(!g.BlocksPointer(new Vector2(Screen.width*.65f,Screen.height*.5f)),"room outside inventory remains interactive");
  var empty=g.InventoryRect(0);var emptyPoint=new Vector2(empty.center.x,Screen.height-empty.center.y);Check(g.BlocksPointer(emptyPoint),"empty inventory slot blocks scene clicks");yield return MouseClick(emptyPoint);Check(!v.Focused&&g.Selected=="","empty slot does not focus scene or select an item");
  Check(g.inventoryIcons.Length==14&&g.inventoryIcons.All(x=>x),"all inventory items have model portraits");
  yield return Capture("physical_overview");yield return Focus(0);
  g.firstDrawer.Toggle();Check(!g.firstDrawer.IsOpen,"first drawer locked before correct code");
  for(int i=0;i<3;i++)for(int k=0;k<new[]{5,6,8}[i];k++)yield return Click(0,"digit",i);
  Check(g.State.solved[0]&&g.firstDrawer.IsOpen,"568 automatically opens physical drawer");Check(!g.State.Has("winding_key"),"reward requires physical pickup");
  g.RestoreSnapshot(JsonUtility.ToJson(g.State));Check(g.State.pendingRewards.Contains("winding_key")&&g.firstDrawer.IsOpen,"uncollected rewards survive save restoration");yield return Rewards(0);
  int count=g.State.inventory.Count;Check(!g.Act(0,"reward",0,"winding_key")&&g.State.inventory.Count==count,"physical reward cannot be duplicated");
  yield return Focus(1);yield return Capture("river_padlock_closed");
  yield return Click(1,"install");Check(!g.State.installed[1],"empty hand cannot unlock river padlock");
  Check(g.State.Has("block2")&&!g.State.Has("block0"),"first wooden reward is ma instead of di");
  yield return Select("block2");yield return Click(1,"install");Check(!g.State.installed[1]&&g.State.Has("block2"),"wrong item cannot unlock river padlock");Check(g.Feedback.LastSound=="error","wrong item gives error sound");
  yield return Click(1,"cargo",1);Check(g.State.cargo==-1,"locked river cannot load cargo");
  yield return Install(1,"winding_key");yield return new WaitForSecondsRealtime(.65f);
  Check(g.Feedback.AudioReady,"music and three feedback audio clips loaded");Check(g.Feedback.LastSound=="success","correct key gives success sound");Check(g.Feedback.Thought.Contains("锁开了"),"unlock gives short thought");
  var padlock=FindFirstObjectByType<RiverPadlock>();Check(padlock&&padlock.IsOpen&&padlock.shackle.localPosition.z>.03f,"key opens physical shackle");
  yield return Capture("river_padlock_open");g.RestoreSnapshot(JsonUtility.ToJson(g.State));Check(padlock.IsOpen,"river lock remains open after state restoration");
  yield return Click(1,"cargo",0);yield return Click(1,"cross");Check(g.State.boatBank==0,"unsafe crossing auto resets");
  foreach(int cargo in new[]{1,-1,0,1,2,-1,1}){if(cargo>=0)yield return Click(1,"cargo",cargo);yield return Click(1,"cross");}
  Check(g.State.solved[1],"river solved exclusively on model");yield return Rewards(1);
  yield return Focus(2);yield return Install(2,"compass");for(int i=0;i<3;i++)for(int k=0;k<new[]{1,3,3}[i];k++)yield return Click(2,"compass",i);
  Check(g.State.solved[2],"compasses auto unlock");yield return Rewards(2);
  yield return Focus(3);yield return Install(3,"bulb");yield return Click(3,"reset");
  for(int i=0;i<9;i++){int n=(4-g.State.wires[i])%4;for(int j=0;j<n&&!g.State.solved[3];j++)yield return Click(3,"wire",i);}
  Check(g.State.solved[3]&&g.stations[4].model.gameObject.activeSelf,"circuit auto lights lamp and reveals box");yield return Capture("physical_wiring");
  yield return Focus(4);yield return Capture("love_clues_closed");for(int i=0;i<4;i++)for(int k=0;k<5;k++)yield return Click(4,"letter",i);
  Check(g.State.Word=="LOVE"&&g.State.solved[4]&&Quaternion.Angle(g.Node(4,"WordBox_Lid").localRotation,Quaternion.identity)>90,"LOVE opens lid without confirmation");yield return Capture("love_box_open");yield return Rewards(4);
  yield return Focus(5);yield return Install(5,"moon_disc");yield return Click(5,"moon",0);yield return Click(5,"reset");
  Check(g.State.moons.SequenceEqual(PoetryPuzzleState.InitialMoons()),"moon reset restores harder layout");yield return Capture("moon_harder_start");
  foreach(int i in new[]{1,2,3,4,5}){var expected=new PoetryPuzzleState{moons=(int[])g.State.moons.Clone()};expected.Flip(i);yield return Click(5,"moon",i);Check(g.State.moons.SequenceEqual(expected.moons),"moon click applies expected pattern "+i);if(i<5)Check(!g.State.solved[5],"moon not solved before fifth move "+i);}
  Check(g.State.solved[5],"moon model solved in five moves");yield return Rewards(5);
  foreach(var h in g.exploration){yield return Focus(h.owner);if(h.container){h.container.Toggle();yield return new WaitForSecondsRealtime(.7f);}yield return Click(h);Check(g.State.Has(h.item),"exploration pickup "+h.item);}
  yield return Focus(6);yield return Install(6,"tray_handle");
  yield return Select("pine");yield return Click(6,"slot",2);yield return Click(6,"slot",2);Check(g.State.Has("pine"),"occupied model slot returns item");
  string[] order={"stone","moon","boat","pine"};for(int i=0;i<4;i++){yield return Select(order[i]);yield return Click(6,"slot",i);}
  Check(g.State.solved[6],"object order auto opens drawer");yield return Rewards(6);
  yield return Focus(7);for(int i=0;i<4;i++){yield return Select("block"+i);yield return Click(7,"slot",i);}
  Check(g.State.solved[7],"four character blocks auto open final box");yield return Rewards(7);
  yield return Focus(g.exitFocus);yield return Select("exit_key");yield return Click(-1,"exit");Check(g.State.escaped&&Quaternion.Angle(g.exitDoor.localRotation,Quaternion.identity)>90,"key used on physical door lock");yield return Capture("physical_exit");
  g.NewGame();yield return Focus(g.stations[0].focus);InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.Escape));yield return null;yield return null;InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());yield return null;Check(v.Focused==null,"Escape returns without UI button");
 }
}
