using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public sealed class PoetryReleaseSmoke:MonoBehaviour {
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 static void Auto(){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,"-poetrySmoke");if(i>=0&&i+1<a.Length)new GameObject("Release smoke check").AddComponent<PoetryReleaseSmoke>().Run(a[i+1],true);}
 public void Run(string path,bool quit){StartCoroutine(Check(path,quit));}
 IEnumerator Check(string path,bool quit){
  yield return null;yield return null;
  while(!UnityEngine.Rendering.SplashScreen.isFinished)yield return null;
  yield return new WaitForSecondsRealtime(1f);
  var results=new List<string>();var v=FindFirstObjectByType<RoomViewController>();
  float distance=v.distance,yaw=v.yaw,pitch=v.pitch;
  try{
   if(!v)throw new Exception("No room camera");v.ReturnToOverview();v.distance=10;v.ApplyScroll(120);results.Add((v.distance<9?"PASS":"FAIL")+" wheel notch changes overview scale");
   for(int i=0;i<30;i++)v.ApplyScroll(120);results.Add((Mathf.Approximately(v.distance,3)?"PASS":"FAIL")+" overview zoom in reaches 3");
   for(int i=0;i<40;i++)v.ApplyScroll(-120);results.Add((Mathf.Approximately(v.distance,18)?"PASS":"FAIL")+" overview zoom out reaches 18");
   var g=FindFirstObjectByType<PoetryPuzzleGame>();for(int a=0;a<360;a+=45){v.SetOrbit(a);if(g.stations[0].focus.Available)break;}v.Focus(g.stations[0].focus);v.Snap();float fov=v.viewCamera.fieldOfView;v.ApplyScroll(120);results.Add((v.viewCamera.fieldOfView<fov?"PASS":"FAIL")+" focused zoom works");
   v.ReturnToOverview();v.distance=distance;v.yaw=yaw;v.pitch=pitch;v.Snap();
   results.Add((File.Exists(Path.Combine(Application.streamingAssetsPath,"OurStory/index.html"))?"PASS":"FAIL")+" ending website packaged");results.Add((Resources.Load<Shader>("EndingVortex")?"PASS":"FAIL")+" ending shader available");results.Add((g.Feedback.AudioReady?"PASS":"FAIL")+" audio available");
  }catch(Exception e){results.Add("FAIL "+e);}
  Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllLines(path,results);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.ChangeExtension(path,"png"));yield return null;yield return null;if(quit)Application.Quit(results.Exists(x=>x.StartsWith("FAIL"))?1:0);
 }
}
