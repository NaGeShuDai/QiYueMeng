using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class PoetryClueSetup {
 const string Folder="Assets/PoetryRoom/PoetryClues";
 static Font font;static PoetryPuzzleGame game;
 static Transform Root(Transform parent,string name){var old=parent.Find(name);if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);var t=new GameObject(name).transform;t.SetParent(parent,false);return t;}
 static TextMesh Text(Transform parent,string name,string text,Vector3 pos,float size,TextAnchor anchor=TextAnchor.MiddleCenter){var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=pos;t.localRotation=Quaternion.Euler(0,180,0);var m=t.gameObject.AddComponent<TextMesh>();m.font=font;m.fontSize=64;m.characterSize=size;m.anchor=anchor;m.text=text;m.color=new Color(.92f,.78f,.52f);m.GetComponent<Renderer>().sharedMaterial=font.material;return m;}
 static void Include(RoomFocusTarget f,Transform root){f.surfaces=f.surfaces.Where(r=>r).Concat(root.GetComponentsInChildren<Renderer>(true)).Distinct().ToArray();}
 static Transform Reading(string name,string[] lines,Vector3 position,Quaternion rotation,float width,float height,RoomViewController.Wall wall){
  var root=Root(game.transform,name);root.position=position;root.rotation=rotation;
  for(int i=0;i<lines.Length;i++)Text(root,"Line "+i,lines[i],new Vector3(width/2,height/2-i*height/(lines.Length-1),0),width/24* .16f,TextAnchor.MiddleLeft);
  var f=root.gameObject.AddComponent<RoomFocusTarget>();f.label=name;f.priority=15;f.surfaces=root.GetComponentsInChildren<Renderer>();f.lookAt=position;f.cameraPosition=position+root.forward*(width*1.20f);f.fieldOfView=45;f.BindViewPose(root);
  root.gameObject.layer=29;var c=root.gameObject.AddComponent<BoxCollider>();c.size=new Vector3(width,height+.1f,.015f);
  game.view.targets=game.view.targets.Where(t=>t).Append(f).ToArray();
  if(wall!=null){wall.renderers=wall.renderers.Where(r=>r).Concat(f.surfaces).ToArray();wall.colliders=wall.colliders.Where(x=>x).Append(c).ToArray();}return root;
 }
 [MenuItem("Tools/Poetry Room/Apply Poetry Clues")]
 public static void Apply(){
  if(Application.isPlaying)throw new Exception("Stop Play first");game=UnityEngine.Object.FindFirstObjectByType<PoetryPuzzleGame>();font=AssetDatabase.LoadAssetAtPath<Font>(Folder+"/PoetryChinese.ttf");
  if(!font)throw new Exception("Missing Chinese font");
  var south=game.view.walls.OrderBy(w=>Vector3.Distance(w.outward,Vector3.back)).First();
  Reading("梦中梦I · 墙诗",new[]{"梦中梦 I","我总在同一个梦里停靠，","一个从没去过的海岸","第七码头的石牌在雾里轻摇。","我把梦里的灯塔画成记号，","地图上没有它，只在我心里反复描。","别人说那只是梦，我不相信——","因为那一日梦回，我触及晚潮。","于是收拾行李，决定去找。","第七码头，从没到过的地方","我向往的岛。","天明前出发，","沿着梦里的方向走一遭。","异于书中，数墙上之笔。"},new Vector3(-1.35f,2.02f,-2.43f),Quaternion.identity,1.75f,1.1f,south);
  var east=game.view.walls.OrderBy(w=>Vector3.Distance(w.outward,Vector3.right)).First();
  Reading("梦中梦II · 方位诗",new[]{"梦中梦 II                    北 ↑","第七码头在哪？","地图摊开，只有空白回答我的盘算。","如果从没去过，我凭什么认定它在人间？","我反复把梦和现实放在同一张纸上看，","哪一边先褪色，哪一边先变暗。","可那浪潮声清晰得像一句判断，","逼我把怀疑变成答案。","第七码头也许不是岸，","是我非去不可的那道关。","以梦和现实为中"},new Vector3(2.43f,2.05f,1.16f),Quaternion.Euler(0,-90,0),1.72f,.9f,east);
  var cp=Root(game.stations[2].model,"Poetry compass labels");
  foreach(var r in game.stations[2].model.GetComponentsInChildren<Renderer>(true))if(r.name.StartsWith("Compass association")||r.name.StartsWith("Compass cardinal"))r.gameObject.SetActive(false);
  string[] labels={"人间","怀疑","答案"};string[] dirs={"北","东北","东","东南","南","西南","西","西北"};
  for(int i=0;i<3;i++){
   var needle=game.Node(2,"CompassNeedle_"+i);Vector3 p=game.stations[2].model.InverseTransformPoint(needle.position);
   Text(cp,"Meaning "+i,labels[i],p+new Vector3(0,-.105f,.012f),.0027f);
   for(int d=0;d<8;d++){float a=d*Mathf.PI/4;var mark=Text(cp,"Direction "+i+" "+d,dirs[d],p+new Vector3(-Mathf.Sin(a)*.052f,Mathf.Cos(a)*.052f,.012f),.0016f);mark.color=new Color(.12f,.07f,.035f);}
  }Include(game.stations[2].focus,cp);
  var oldLove=game.Node(4,"WordBox_Lid").Find("Poetry LOVE clues");if(oldLove)UnityEngine.Object.DestroyImmediate(oldLove.gameObject);
  var love=Root(game.stations[4].model,"Poetry LOVE clues");
  for(int i=0;i<4;i++){var label=Text(love,"Extraction "+i,new[]{"我心\n叶·①","我心\n希望·②","Melt\n爱着·③","我心\n平静·②"}[i],new Vector3(-.18f+i*.12f,-.098f,.153f),.003f);label.transform.localRotation=Quaternion.Euler(0,180,0);}
  var rule=Text(love,"Reading rule","回到英文原诗，取第几字母",new Vector3(0,.105f,.153f),.0025f);rule.transform.localRotation=Quaternion.Euler(0,180,0);Include(game.stations[4].focus,love);
  var seq=Root(game.stations[6].model,"Poetry sequence clue");Text(seq,"Stanza cue","漫步 · 黄昏酿成了酒 →",new Vector3(0,.105f,.442f),.0035f);Include(game.stations[6].focus,seq);
  PoetryCluePresentation.Apply(false);
  BuildItems();
  typeof(PoetryPhysicalSetup).GetMethod("GenerateIcons",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{game});
  EditorUtility.SetDirty(game);EditorUtility.SetDirty(game.view);EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);AssetDatabase.SaveAssets();Debug.Log("POETRY_CLUES_APPLIED");
 }
 static void Part(Transform root,PrimitiveType type,Vector3 pos,Vector3 scale,Material mat){var o=GameObject.CreatePrimitive(type);o.transform.SetParent(root,false);o.transform.localPosition=pos;o.transform.localScale=scale;UnityEngine.Object.DestroyImmediate(o.GetComponent<Collider>());o.GetComponent<Renderer>().sharedMaterial=mat;}
 static Mesh Extrude(Vector2[] outline,string name){int n=outline.Length;var vs=new Vector3[n*2+2];for(int i=0;i<n;i++){vs[i]=new Vector3(outline[i].x,-.012f,outline[i].y);vs[i+n]=new Vector3(outline[i].x,.012f,outline[i].y);}vs[n*2]=new Vector3(0,-.012f,.05f);vs[n*2+1]=new Vector3(0,.012f,.05f);var ts=new System.Collections.Generic.List<int>();for(int i=0;i<n;i++){int j=(i+1)%n;ts.AddRange(new[]{2*n,j,i,2*n+1,i+n,j+n,i,j,j+n,i,j+n,i+n});}var mesh=new Mesh{name=name};mesh.vertices=vs;mesh.triangles=ts.ToArray();mesh.RecalculateNormals();string path=Folder+"/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(mesh,old);UnityEngine.Object.DestroyImmediate(mesh);return old;}AssetDatabase.CreateAsset(mesh,path);return mesh;}
 static void BuildItems(){
  var wood=AssetDatabase.LoadAssetAtPath<Material>("Assets/PoetryRoom/PuzzleProps/Materials/PP_AgedBrass.mat");
  var woodPath=AssetDatabase.FindAssets("t:Material",new[]{"Assets/PoetryRoom/PuzzleProps/Materials"}).Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault(x=>x.ToLower().Contains("wood"));if(woodPath!=null)wood=AssetDatabase.LoadAssetAtPath<Material>(woodPath);
  foreach(int index in new[]{5,6,7,8}){
   var root=new GameObject("Poetry_"+PoetryPuzzleGame.ItemIds[index]);var t=root.transform;
   Part(t,PrimitiveType.Cube,new Vector3(0,0,.007f),new Vector3(.09f,.045f,.014f),wood);
   if(index==6){Part(t,PrimitiveType.Cylinder,new Vector3(0,0,.025f),new Vector3(.014f,.023f,.014f),wood);t.GetChild(1).localRotation=Quaternion.Euler(90,0,0);Part(t,PrimitiveType.Sphere,new Vector3(0,0,.065f),new Vector3(.065f,.065f,.055f),wood);}
   else {Vector2[] poly;if(index==5)poly=Enumerable.Range(0,10).Select(i=>{float a=i*Mathf.PI/5;float r=i%2==0?.05f:.023f;return new Vector2(Mathf.Sin(a)*r,.06f+Mathf.Cos(a)*r);}).ToArray();else if(index==7)poly=new[]{new Vector2(-.05f,.015f),new Vector2(-.018f,.10f),new Vector2(.012f,.05f),new Vector2(.03f,.078f),new Vector2(.05f,.015f)};else poly=Enumerable.Range(0,21).Select(i=>{float a=i*Mathf.PI/20;return new Vector2(Mathf.Cos(a)*.048f,.018f+Mathf.Sin(a)*.048f);}).ToArray();var child=new GameObject("Carved silhouette");child.transform.SetParent(t,false);child.AddComponent<MeshFilter>().sharedMesh=Extrude(poly,"PoetryShape"+index);child.AddComponent<MeshRenderer>().sharedMaterial=wood;}
   var prefab=PrefabUtility.SaveAsPrefabAsset(root,Folder+"/Item"+index+".prefab");game.itemPrefabs[index]=prefab;UnityEngine.Object.DestroyImmediate(root);
   foreach(var h in game.exploration.Where(h=>h.item==PoetryPuzzleGame.ItemIds[index])){
    if(PrefabUtility.IsPartOfPrefabInstance(h.gameObject))PrefabUtility.UnpackPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(h.gameObject),PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
    h.gameObject.SetActive(true);foreach(var c in h.GetComponentsInChildren<Collider>(true)){c.gameObject.SetActive(true);c.enabled=true;}
    foreach(var r in h.GetComponentsInChildren<Renderer>(true))UnityEngine.Object.DestroyImmediate(r);
    var old=h.transform.Find("Poetry replacement");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
    var visual=(GameObject)PrefabUtility.InstantiatePrefab(prefab,h.transform);visual.name="Poetry replacement";visual.transform.localPosition=Vector3.zero;visual.transform.localRotation=Quaternion.identity;Include(h.owner,visual.transform);
    var collider=h.GetComponent<BoxCollider>();if(!collider)collider=h.gameObject.AddComponent<BoxCollider>();collider.center=new Vector3(0,0,.055f);collider.size=new Vector3(.12f,.08f,.13f);collider.enabled=true;h.gameObject.layer=27;
   }
  }
 }
}

