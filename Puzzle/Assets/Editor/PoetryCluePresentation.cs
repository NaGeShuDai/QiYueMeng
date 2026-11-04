using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class PoetryCluePresentation {
 [MenuItem("Tools/Poetry Room/Revise Clue Presentation")]
 public static void Menu()=>Apply(true);
 public static void Apply(bool save){
  if(Application.isPlaying)throw new Exception("Stop Play first");
  var g=UnityEngine.Object.FindFirstObjectByType<PoetryPuzzleGame>();
  var poem=g.transform.Find("梦中梦I · 墙诗");var focus=poem.GetComponent<RoomFocusTarget>();
  var paper=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>r.name.StartsWith("Poetry print paper")).OrderBy(r=>Vector3.Distance(r.bounds.center,poem.position)).First();
  poem.localScale=Vector3.one;
  var texts=poem.GetComponentsInChildren<TextMesh>();var bounds=texts[0].GetComponent<Renderer>().bounds;foreach(var t in texts)bounds.Encapsulate(t.GetComponent<Renderer>().bounds);
  float paperWidth=Mathf.Max(paper.bounds.size.x,paper.bounds.size.z);float textWidth=Mathf.Max(bounds.size.x,bounds.size.z);
  float scale=Mathf.Min(paperWidth*.86f/textWidth,paper.bounds.size.y*.88f/bounds.size.y);
  var center=poem.InverseTransformPoint(bounds.center);poem.localScale=Vector3.one*scale;
  poem.position=paper.bounds.center+poem.forward*.012f-poem.TransformVector(center);
  var target=paper.bounds.center+poem.forward*.015f;focus.lookAt=target;focus.cameraPosition=target+poem.forward*Mathf.Max(paper.bounds.size.y*1.45f,paperWidth*1.5f);focus.fieldOfView=44;focus.BindViewPose(poem);
  var bc=poem.GetComponent<BoxCollider>();bc.center=center;bc.size=new Vector3(paperWidth/scale,paper.bounds.size.y/scale,.025f/scale);
  var second=g.transform.Find("梦中梦II · 方位诗");var lines=second.GetComponentsInChildren<TextMesh>(true).OrderBy(t=>int.Parse(t.name.Substring(5))).ToArray();
  string[] hint={"第二个梦里，","他们在梦和现实的哪个方位？"};
  for(int i=0;i<lines.Length;i++){lines[i].gameObject.SetActive(i<2);if(i<2){lines[i].text=hint[i];lines[i].characterSize=.012f;lines[i].transform.localPosition=new Vector3(.68f,.09f-i*.12f,0);}}
  foreach(var t in g.stations[2].model.GetComponentsInChildren<TextMesh>(true))if(t.name.StartsWith("Direction "))t.gameObject.SetActive(false);
  var love=g.stations[4].model.Find("Poetry LOVE clues");int[] shuffle={2,0,3,1};
  string[] clues={"我心\n叶·①","我心\n希望·②","Melt\n爱着·③","我心\n平静·②"};
  for(int i=0;i<4;i++)love.Find("Extraction "+i).GetComponent<TextMesh>().text=clues[shuffle[i]];
  love.Find("Reading rule").GetComponent<TextMesh>().text="回到英文原诗，取字母，再组成一个词";
  EditorUtility.SetDirty(g);EditorUtility.SetDirty(g.view);EditorSceneManager.MarkSceneDirty(g.gameObject.scene);
  if(save){EditorSceneManager.SaveScene(g.gameObject.scene);AssetDatabase.SaveAssets();}
  Debug.Log("CLUE_PRESENTATION_REVISED paper="+paper.bounds+" scale="+scale);
 }
}
