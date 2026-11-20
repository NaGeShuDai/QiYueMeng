using System;
using System.Collections;
using System.IO;
using UnityEngine;
public sealed class ReleaseWebsiteCheck : MonoBehaviour {
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 static void Init(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-poetryWebSmoke");if(i>=0&&i+1<args.Length)new GameObject("Release website check").AddComponent<ReleaseWebsiteCheck>().StartCoroutine(Check(args[i+1]));}
 static IEnumerator Check(string report){
  yield return null;yield return new WaitForSecondsRealtime(2);
  string result;
  try {
   var game=FindFirstObjectByType<PoetryPuzzleGame>();
   if(!game||!game.Ending)throw new Exception("Ending not initialized");
   var content=File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"OurStory/content.js"));
   if(!content.Contains("卿卿")||!content.Contains("瀚")||content.Contains("小月"))throw new Exception("Old website names");
   game.Ending.OpenWebsite();
   if(game.Ending.LastBrowserError!=null)throw new Exception(game.Ending.LastBrowserError);
   result="PASS packaged names and real ending browser launch\n"+game.Ending.LastOpenedPath;
  }catch(Exception e){result="FAIL "+e;}
  File.WriteAllText(report,result);
  yield return new WaitForSecondsRealtime(4);
  Application.Quit(result.StartsWith("PASS")?0:1);
 }
}
