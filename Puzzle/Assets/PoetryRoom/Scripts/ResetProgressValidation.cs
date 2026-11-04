using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public sealed class ResetProgressValidation:MonoBehaviour {
 readonly List<string> results=new List<string>();
 void Check(bool value,string label){results.Add((value?"PASS ":"FAIL ")+label);}
 public void Run(){StartCoroutine(Validate());}
 IEnumerator Validate(){
  var g=FindFirstObjectByType<PoetryPuzzleGame>();
  string dir=Path.GetFullPath(Application.dataPath+"/../../UnityValidation");
  bool persist=g.persist;g.persist=true;
  for(int run=0;run<2;run++){
   g.NewGame();
   for(int i=0;i<8;i++){g.State.solved[i]=true;g.State.installed[i]=true;}
   g.State.inventory.Add("block0");g.State.found.Add("pine");g.State.pendingRewards.Add("exit_key");g.State.digits[0]=7;g.State.escaped=run==1;g.Commit();g.Select("block0");
   g.view.Focus(g.stations[0].focus);g.view.Snap();
   yield return new WaitForSecondsRealtime(.3f);
   var rect=g.ResetProgressRect;var point=new Vector2(rect.center.x,Screen.height-rect.center.y);
   Check(g.BlocksPointer(point),"reset blocks scene clicks, escaped="+g.State.escaped);
   InputSystem.QueueStateEvent(Mouse.current,new MouseState{position=point}.WithButton(MouseButton.Left));yield return null;yield return null;
   InputSystem.QueueStateEvent(Mouse.current,new MouseState{position=point});yield return null;yield return null;
   yield return new WaitForSecondsRealtime(.7f);
   Check(JsonUtility.ToJson(g.State)==JsonUtility.ToJson(new PoetryPuzzleState()),"mouse click restores all puzzle state "+run);
   Check(!PlayerPrefs.HasKey("PoetryRoom.Puzzle.v1"),"save deleted "+run);
   Check(!g.view.Focused&&!g.view.IsDetail&&g.Selected=="","camera and selection reset "+run);
   Check(!g.firstDrawer.IsOpen,"bedside drawer closed "+run);
   Check(!g.stations[4].model.gameObject.activeSelf&&!g.stations[7].model.gameObject.activeSelf,"locked reward boxes hidden "+run);
   Check(!FindObjectsByType<PuzzleHotspot>(FindObjectsSortMode.None).Any(h=>h.action=="reward"),"spawned rewards removed "+run);
  }
  g.persist=persist;
  yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(dir+"/reset_progress.png");
  File.WriteAllLines(dir+"/reset_progress_validation.txt",results);Debug.Log("RESET_PROGRESS_VALIDATION_DONE "+results.Count+" checks");
 }
}
