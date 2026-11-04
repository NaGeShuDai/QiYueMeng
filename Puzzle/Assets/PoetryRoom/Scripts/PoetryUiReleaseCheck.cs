using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
public sealed class PoetryUiReleaseCheck:MonoBehaviour {
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 static void Auto(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-uiReleaseCheck");if(i>=0&&i+1<a.Length)new GameObject("UI release check").AddComponent<PoetryUiReleaseCheck>().StartCoroutine(Run(a[i+1]));}
 static IEnumerator Run(string path){
  yield return null;while(!UnityEngine.Rendering.SplashScreen.isFinished)yield return null;yield return new WaitForSecondsRealtime(1);
  var g=FindFirstObjectByType<PoetryPuzzleGame>();string snapshot=JsonUtility.ToJson(g.State);bool persist=g.persist;g.persist=false;
  string result=(!g.QuitGameRect.Overlaps(g.ResetProgressRect)&&!g.QuitGameRect.Overlaps(g.InventoryPanelRect)?"PASS":"FAIL")+" exit button separated from reset and inventory\n";
  yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.ChangeExtension(path,"png"));yield return null;
  g.State.escaped=true;g.Refresh();yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.ChangeExtension(path,null)+"_ending.png");yield return null;
  g.RestoreSnapshot(snapshot);g.persist=persist;
  var rect=g.QuitGameRect;var point=new Vector2(rect.center.x,Screen.height-rect.center.y);result+=(g.BlocksPointer(point)?"PASS":"FAIL")+" quit button blocks world interaction\n";File.WriteAllText(path,result);
  bool clicking=false;Application.quitting+=()=>{if(clicking)File.AppendAllText(path,"PASS real exit button click quits player\n");};
  InputSystem.QueueStateEvent(Mouse.current,new MouseState{position=point});yield return null;yield return null;
  clicking=true;InputSystem.QueueStateEvent(Mouse.current,new MouseState{position=point}.WithButton(MouseButton.Left));yield return null;yield return null;
  InputSystem.QueueStateEvent(Mouse.current,new MouseState{position=point});yield return new WaitForSecondsRealtime(2);
  clicking=false;File.AppendAllText(path,"FAIL exit button did not quit\n");Application.Quit(1);
 }
}

