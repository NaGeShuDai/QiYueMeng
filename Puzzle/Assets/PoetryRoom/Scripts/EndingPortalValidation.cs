using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public sealed class EndingPortalValidation : MonoBehaviour {
 public void Run(){StartCoroutine(Check());}
 IEnumerator Check(){
  var g=FindFirstObjectByType<PoetryPuzzleGame>(); var results=new List<string>();
  string snapshot=JsonUtility.ToJson(g.State);bool persist=g.persist;g.persist=false;
  g.Ending.suppressBrowserForValidation=true;
  try {
   g.NewGame();g.State.solved[7]=true;g.State.Give("exit_key");g.Select("exit_key");
   g.view.Focus(g.exitFocus);g.view.Snap();
   results.Add(g.Act(-1,"exit")?"PASS final key opens door":"FAIL exit");
   yield return new WaitForSecondsRealtime(1);
   var portal=GameObject.Find("Ending - doorway vortex");
   results.Add(portal&&portal.activeInHierarchy?"PASS vortex visible":"FAIL vortex missing");
   results.Add(g.Ending.IsTransitioning?"PASS delay active":"FAIL delay");
   yield return new WaitForEndOfFrame();
   ScreenCapture.CaptureScreenshot(Path.GetFullPath(Application.dataPath+"/../../UnityValidation/ending_portal.png"));
   yield return new WaitForSecondsRealtime(2.7f);
   results.Add(g.Ending.LaunchCount==1?"PASS one browser launch requested":"FAIL launch count");
   g.Refresh();yield return new WaitForSecondsRealtime(.2f);
   results.Add(g.Ending.LaunchCount==1?"PASS refresh does not relaunch":"FAIL refresh relaunch");
   g.NewGame();g.State.solved[7]=true;g.State.Give("exit_key");g.Select("exit_key");g.Act(-1,"exit");g.NewGame();
   yield return new WaitForSecondsRealtime(3.5f);
   results.Add(g.Ending.LaunchCount==1&&!portal.activeSelf?"PASS reset cancels jump and hides vortex":"FAIL reset");
   results.Add(File.Exists(g.Ending.WebsitePath)?"PASS website exists":"FAIL website");
  } finally {g.RestoreSnapshot(snapshot);g.persist=persist;g.Ending.suppressBrowserForValidation=false;g.view.ReturnToOverview();}
  File.WriteAllLines(Path.GetFullPath(Application.dataPath+"/../../UnityValidation/ending_validation.txt"),results);
  Debug.Log("ENDING_VALIDATION_DONE "+string.Join("; ",results));Destroy(gameObject);
 }
}
