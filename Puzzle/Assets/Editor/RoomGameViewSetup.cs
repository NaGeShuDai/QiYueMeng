using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class RoomGameViewSetup {
 const BindingFlags Flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 [MenuItem("Tools/Poetry Room/Fit Game View")]
 public static void Fit(){
  var type=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
  var window=EditorWindow.GetWindow(type);
  // Free Aspect matches the render surface to the available editor pane.
  type.GetProperty("selectedSizeIndex",Flags).SetValue(window,0);
  type.GetMethod("SnapZoom",Flags).Invoke(window,new object[]{1f});
  window.Focus();window.Repaint();
  EditorApplication.delayCall+=Inspect;
  Debug.Log("Game view fitted: Free Aspect, 1x, centered.");
 }
 [MenuItem("Tools/Poetry Room/Inspect Game View")]
 public static void Inspect(){
  var type=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
  var window=EditorWindow.GetWindow(type);
  string report="Screen="+Screen.width+"x"+Screen.height+" pixelsPerPoint="+EditorGUIUtility.pixelsPerPoint+" window="+window.position+"\n"+EditorJsonUtility.ToJson(window,true);
  var zoom=type.GetField("m_ZoomArea",Flags).GetValue(window);
  foreach(var m in zoom.GetType().GetMethods(Flags))if(m.Name.Contains("Scale")||m.Name.Contains("scale")||m.Name.Contains("Transform")||m.Name.Contains("shownArea"))report+="\nZOOM "+m;
  foreach(var m in type.GetMethods(Flags))if(m.Name.Contains("Zoom")||m.Name.Contains("Scale"))report+="\nGAME "+m;
  foreach(var p in type.GetProperties(Flags))if(p.Name.IndexOf("size",StringComparison.OrdinalIgnoreCase)>=0||p.Name.IndexOf("rect",StringComparison.OrdinalIgnoreCase)>=0){try{report+="\n"+p.Name+"="+p.GetValue(window);}catch{}}
  File.WriteAllText(Path.GetFullPath(Application.dataPath+"/../../UnityValidation/game_view.txt"),report);
 }
}
