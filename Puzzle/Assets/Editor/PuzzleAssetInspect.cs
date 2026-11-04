using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
public static class PuzzleAssetInspect {
 [MenuItem("Tools/Poetry Room/Inspect Puzzle Assets")]
 public static void Run(){
  var lines=Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r=>r.name.Contains("Desk")||r.name.Contains("Nightstand")||r.name.Contains("Bookcase")||r.name.Contains("Wardrobe")||r.name.Contains("Bed ")).Select(r=>r.name+" center="+r.bounds.center+" size="+r.bounds.size).ToList();
  var asset=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PoetryRoom/PuzzleProps/Models/P01_NumberLock.fbx");
  var go=Object.Instantiate(asset);
  lines.AddRange(go.GetComponentsInChildren<Transform>().Take(18).Select(t=>t.name+" pos="+t.localPosition+" rot="+t.localEulerAngles+" scale="+t.localScale));
  foreach(var r in go.GetComponentsInChildren<Renderer>().Take(6))lines.Add("PROP "+r.name+" "+r.bounds.center+" "+r.bounds.size);
  Object.DestroyImmediate(go);File.WriteAllLines("../UnityValidation/puzzle_geometry.txt",lines);
 }
}
