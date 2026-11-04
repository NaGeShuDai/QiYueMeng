using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class LoveMoonSetup {
 [MenuItem("Tools/Poetry Room/Apply LOVE And Moon Revision")]
 public static void Apply(){
  if(Application.isPlaying)throw new Exception("Stop Play first");
  var g=UnityEngine.Object.FindFirstObjectByType<PoetryPuzzleGame>();
  ReplaceGlyph(g,"LetterWheel_0_H","LetterWheel_0_L",'L');ReplaceGlyph(g,"LetterWheel_2_M","LetterWheel_2_V",'V');
  RoomFocusRepair.Repair();
  var s=g.stations[4];var t=s.focus;Vector3 front=Vector3.ProjectOnPlane(s.model.TransformDirection(Vector3.down),Vector3.up).normalized;
  t.lookAt=s.model.position+Vector3.up*.17f;t.cameraPosition=t.lookAt+front*.95f+Vector3.up*.60f;t.fieldOfView=42;
  t.detailLookAt=t.lookAt;t.detailPosition=t.lookAt+front*.60f+Vector3.up*.55f;t.detailFieldOfView=42;t.BindViewPose(s.model);
  for(int i=0;i<6;i++)g.Node(5,"MoonTile_"+i).localRotation=Quaternion.Euler(PoetryPuzzleState.InitialMoons()[i]==1?0:180,0,0);
  EditorUtility.SetDirty(t);EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveScene(g.gameObject.scene);AssetDatabase.SaveAssets();ValidateRules();
 }
 static void ReplaceGlyph(PoetryPuzzleGame g,string oldName,string newName,char letter){
  var node=g.Node(4,oldName);if(!node)node=g.Node(4,newName);var filter=node.GetComponent<MeshFilter>();var b=filter.sharedMesh.bounds;
  var vertices=new List<Vector3>();var triangles=new List<int>();
  Action<Vector2,Vector2,float> stroke=(a,c,width)=>{
   var normal=new Vector2(-(c-a).y,(c-a).x).normalized*width*.5f;int n=vertices.Count;
   foreach(var p in new[]{a+normal,a-normal,c-normal,c+normal})vertices.Add(new Vector3(Mathf.LerpUnclamped(b.max.x,b.min.x,p.x),Mathf.LerpUnclamped(b.min.y,b.max.y,p.y),b.max.z));
   triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3,n+2,n+1,n,n+3,n+2,n});
  };
  if(letter=='L'){stroke(new Vector2(.09f,.08f),new Vector2(.09f,.96f),.18f);stroke(new Vector2(.09f,.08f),new Vector2(.92f,.08f),.16f);}
  else{stroke(new Vector2(.08f,.95f),new Vector2(.5f,.06f),.15f);stroke(new Vector2(.5f,.06f),new Vector2(.92f,.95f),.15f);}
  string path="Assets/PoetryRoom/PuzzleProps/Models/LoveGlyph_"+letter+".asset";
  var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(!mesh){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,path);}else mesh.Clear();
  mesh.name="Engraved "+letter;mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();filter.sharedMesh=mesh;node.name=newName;EditorUtility.SetDirty(mesh);EditorUtility.SetDirty(filter);
 }
 [MenuItem("Tools/Poetry Room/Validate LOVE And Moon Rules")]
 public static void ValidateRules(){
  var log=new List<string>();Action<bool,string> check=(pass,label)=>{log.Add((pass?"PASS ":"FAIL ")+label);if(!pass)throw new Exception(label);};
  int Encode(int[] a)=>a.Select((v,i)=>v<<i).Sum();
  var distances=Enumerable.Repeat(-1,64).ToArray();distances[63]=0;var queue=new Queue<int>();queue.Enqueue(63);
  while(queue.Count>0){int current=queue.Dequeue();for(int i=0;i<6;i++){var state=new PoetryPuzzleState();state.moons=Enumerable.Range(0,6).Select(j=>(current>>j)&1).ToArray();state.Flip(i);int next=Encode(state.moons);if(distances[next]>=0)continue;distances[next]=distances[current]+1;queue.Enqueue(next);}}
  check(distances.All(d=>d>=0),"all 64 moon states can reach all-lit");check(distances[Encode(PoetryPuzzleState.InitialMoons())]==5,"initial moon layout needs at least five clicks");
  var word=new PoetryPuzzleState();word.solved[3]=true;word.letters=new[]{0,0,0,1};check(!word.Check(4),"wrong word remains locked");word.letters[3]=0;check(word.Word=="LOVE"&&word.Check(4),"LOVE unlocks box");
  var legacy=new PoetryPuzzleState{version=1,moons=new[]{1,1,1,0,0,0}};legacy.solved[0]=true;legacy.inventory.Add("stone");check(legacy.Valid(),"legacy save accepted");legacy.Upgrade();check(legacy.version==2&&legacy.solved[0]&&legacy.Has("stone")&&legacy.moons.SequenceEqual(PoetryPuzzleState.InitialMoons()),"migration preserves progress and refreshes unsolved moon");
  var completed=new PoetryPuzzleState{version=1};completed.solved[5]=true;completed.moons=Enumerable.Repeat(1,6).ToArray();completed.Upgrade();check(completed.solved[5]&&completed.moons.All(v=>v==1),"completed moon stays completed");
  File.WriteAllLines(Path.GetFullPath(Application.dataPath+"/../../UnityValidation/love_moon_rules.txt"),log);Debug.Log("LOVE_MOON_RULES_PASSED");
 }
 [MenuItem("Tools/Poetry Room/Inspect Word Glyphs")]
 public static void Inspect(){
  var g=UnityEngine.Object.FindFirstObjectByType<PoetryPuzzleGame>();string report="";
  foreach(var name in new[]{"LetterWheel_0_H","LetterWheel_2_M"}){
   var t=g.Node(4,name);var mesh=t.GetComponent<MeshFilter>().sharedMesh;
   report+=name+" position="+t.localPosition+" rotation="+t.localEulerAngles+" scale="+t.localScale+" bounds="+mesh.bounds+"\n";
   foreach(var p in mesh.vertices)report+=p.ToString("F5")+" ";report+="\n";
  }
  File.WriteAllText(Path.GetFullPath(Application.dataPath+"/../../UnityValidation/word_glyphs.txt"),report);
 }
}
