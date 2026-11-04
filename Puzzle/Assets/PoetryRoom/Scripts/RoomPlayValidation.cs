using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public sealed class RoomPlayValidation : MonoBehaviour
{
 public void Run(){StartCoroutine(Validate());}
 IEnumerator Validate() {
  var view=FindFirstObjectByType<RoomViewController>();
  var results=new List<string>();string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../../UnityValidation"));Directory.CreateDirectory(dir);
  yield return new WaitForSecondsRealtime(.5f);
  float startYaw=view.yaw;
  for(int a=0;a<360;a+=15) {
   view.SetOrbit(a);yield return null;
   bool ceilingOff=Array.TrueForAll(view.ceilings,r=>!r.enabled);
   results.Add($"orbit {a}: walls={view.VisibleWalls}, ceilingHidden={ceilingOff}, pass={view.VisibleWalls<=2&&ceilingOff}");
  }
  view.SetOrbit(startYaw);yield return new WaitForSecondsRealtime(.3f);Capture(view,"overview",dir);
  foreach(var target in view.targets) {
   bool found=false;
   // Find an overview angle with an unobstructed clickable target.
   for(int a=0;a<360 && !found;a+=30) {
    view.SetOrbit(a);yield return null;Physics.SyncTransforms();
    var p=view.viewCamera.WorldToScreenPoint(target.transform.position);
    found=p.z>0 && p.x>0 && p.x<Screen.width && p.y>0 && p.y<Screen.height && view.Pick(p)==target;
   }
   results.Add($"clickable {target.label}: {found}");
   if(!found)continue;
   float previousYaw=view.yaw;var originalPosition=view.viewCamera.transform.position;
   var screen=view.viewCamera.WorldToScreenPoint(target.transform.position);
   InputSystem.QueueStateEvent(Mouse.current,new MouseState{position=screen}.WithButton(MouseButton.Left));
   yield return null;yield return null;
   InputSystem.QueueStateEvent(Mouse.current,new MouseState{position=screen});
   yield return null;yield return new WaitForSecondsRealtime(.7f);
   results.Add($"mouse focus {target.label}: {view.Focused==target}");
   results.Add($"focused surface visible {target.label}: {target.Available}, wallLimit={view.VisibleWalls<=2}");
   Capture(view,"focus_"+Array.IndexOf(view.targets,target),dir);
   if(target.label=="窗外森林") {
    var win=FindFirstObjectByType<ForestWindow>();
    results.Add($"forest isolated: {(view.viewCamera.cullingMask & (1<<30))==0 && win.forestCamera.cullingMask==(1<<30)}");
    view.viewCamera.transform.position+=view.viewCamera.transform.right*.3f;
    Capture(view,"forest_parallax",dir);
   }
   InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.Escape));
   yield return null;yield return null;
   InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());
   yield return new WaitForSecondsRealtime(.8f);
   results.Add($"escape restore {target.label}: {view.Focused==null && Mathf.Abs(view.yaw-previousYaw)<.01f && Vector3.Distance(originalPosition,view.viewCamera.transform.position)<.04f}");
  }
  view.SetOrbit(startYaw);view.Snap();
  File.WriteAllLines(Path.Combine(dir,"play_validation.txt"),results);
  Debug.Log("POETRY_PLAY_VALIDATION_DONE "+results.Count+" checks");
 }
 public static void Capture(RoomViewController view,string name,string dir) {
  var cam=view.viewCamera;var window=FindFirstObjectByType<ForestWindow>();
  float oldAspect=cam.aspect;var oldTarget=cam.targetTexture;
  var rt=RenderTexture.GetTemporary(1200,900,24,RenderTextureFormat.ARGB32);cam.targetTexture=rt;cam.aspect=4f/3;
  window.SendMessage("LateUpdate");if(window.forestCamera.enabled)window.forestCamera.Render();cam.Render();
  var previous=RenderTexture.active;RenderTexture.active=rt;
  var image=new Texture2D(1200,900,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1200,900),0,0);image.Apply();File.WriteAllBytes(Path.Combine(dir,name+".png"),image.EncodeToPNG());
  Destroy(image);RenderTexture.active=previous;cam.targetTexture=oldTarget;cam.aspect=oldAspect;RenderTexture.ReleaseTemporary(rt);
 }
}
