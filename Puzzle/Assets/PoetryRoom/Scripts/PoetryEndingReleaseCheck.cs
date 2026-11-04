using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
public sealed class PoetryEndingReleaseCheck:MonoBehaviour {
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 static void Auto(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-endingReleaseCheck");if(i>=0&&i+1<args.Length)new GameObject("Ending release check").AddComponent<PoetryEndingReleaseCheck>().StartCoroutine(Run(args[i+1]));}
 static IEnumerator Run(string path){
  yield return null;while(!UnityEngine.Rendering.SplashScreen.isFinished)yield return null;yield return new WaitForSecondsRealtime(1);
  var g=FindFirstObjectByType<PoetryPuzzleGame>();var results=new List<string>();string snapshot=JsonUtility.ToJson(g.State);bool persistence=g.persist;g.persist=false;
  try{
   var s=new PoetryPuzzleState();s.solved[0]=s.solved[1]=true;s.installed[2]=true;s.compass=new[]{1,5,3};results.Add(!s.Check(2)?"PASS rejects old southwest answer":"FAIL old answer");s.compass=new[]{1,3,3};results.Add(s.Check(2)?"PASS northeast southeast southeast":"FAIL new compass");
   var old=new PoetryPuzzleState{version=4};old.solved[2]=true;old.compass=new[]{1,5,3};old.Give("bulb");old.Upgrade();results.Add(old.Valid()&&old.solved[2]&&old.Has("bulb")&&old.compass.SequenceEqual(new[]{1,3,3})?"PASS saved completed compass migrated":"FAIL migration");
   g.NewGame();g.State.solved[7]=true;g.State.Give("exit_key");g.Select("exit_key");
   int launches=g.Ending.LaunchCount;results.Add(g.Act(-1,"exit")?"PASS actual exit key action":"FAIL exit key");
   yield return new WaitForSecondsRealtime(1);results.Add(g.Ending.IsTransitioning?"PASS vortex delay active":"FAIL delay");
   yield return new WaitForSecondsRealtime(3);
   results.Add(g.Ending.LaunchCount==launches+1&&g.Ending.LastBrowserError==null?"PASS Windows shell accepted browser launch":"FAIL browser "+g.Ending.LastBrowserError);
   results.Add("OPENED_PATH "+g.Ending.LastOpenedPath);
   g.Refresh();yield return null;results.Add(g.Ending.LaunchCount==launches+1?"PASS refresh does not open another tab":"FAIL repeated launch");
  }finally{g.RestoreSnapshot(snapshot);g.persist=persistence;}
  File.WriteAllLines(path,results);Application.Quit(results.Any(x=>x.StartsWith("FAIL"))?1:0);
 }
}
