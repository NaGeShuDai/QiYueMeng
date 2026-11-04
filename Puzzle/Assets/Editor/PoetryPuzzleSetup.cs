using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class PoetryPuzzleSetup
{
 const string Base="Assets/PoetryRoom/PuzzleProps";
 static readonly string[] Groups={"P01_NumberLock","P02_RiverCrossing","P03_CompassBoard","P04_WiringLamp","P05_WordBox","P06_MoonMechanism","P07_ObjectSequence","P08_FinalWoodBox"};
 static Dictionary<string,Material> materials=new Dictionary<string,Material>();
 static Transform Find(Transform root,string n)=>root.GetComponentsInChildren<Transform>(true).First(t=>t.name==n||t.name.StartsWith(n+".",StringComparison.Ordinal));
 [MenuItem("Tools/Poetry Room/Install Puzzle Gameplay")]
 public static void Build(){
  if(EditorApplication.isPlaying)throw new Exception("Exit Play before installation");
  if(UnityEngine.Object.FindFirstObjectByType<PoetryPuzzleGame>())throw new Exception("Puzzle gameplay is already installed");
  Directory.CreateDirectory(Base+"/Prefabs");Directory.CreateDirectory(Base+"/Materials");
  var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
  EditorSceneManager.SaveScene(scene,"Assets/PoetryRoom/Scenes/PoetryBedroom_BeforePuzzles.unity",true);
  var view=UnityEngine.Object.FindFirstObjectByType<RoomViewController>();
  if(!view)throw new Exception("Open PoetryBedroom scene first");
  foreach(string f in Directory.GetFiles(Base+"/Models","*.fbx",SearchOption.AllDirectories)){
   var imp=(ModelImporter)AssetImporter.GetAtPath(f.Replace('\\','/'));imp.importCameras=false;imp.importLights=false;imp.isReadable=true;imp.SaveAndReimport();
  }
  var root=new GameObject("Poetry Puzzle Gameplay");var game=root.AddComponent<PoetryPuzzleGame>();game.view=view;
  game.exitFocus=view.targets.First(t=>t.label=="出口");
  string[] legacy={"State dial","Dial ","Word lock","START -","Movable poem","Token ","Desk lamp base","Lamp stem","Linen lamp","Old photograph","Photograph image","Letter ","Wax seal"};
  foreach(var r in GameObject.Find("Poetry Bedroom").GetComponentsInChildren<Renderer>())if(legacy.Any(p=>r.name.StartsWith(p)))r.gameObject.SetActive(false);
  var oldLight=GameObject.Find("Desk lamp glow");if(oldLight)oldLight.SetActive(false);
  var targetList=view.targets.ToList();
  Vector3[] positions={new Vector3(-1.55f,.855f,-1.84f),new Vector3(.05f,.67f,1.0f),new Vector3(2.18f,1.135f,1.2f),new Vector3(-2,.855f,.70f),new Vector3(-2,.855f,1.43f),new Vector3(1.12f,.72f,-1.3f),new Vector3(1.65f,.67f,.15f),new Vector3(-.23f,.43f,.34f)};
  float[] yaw={0,0,90,270,270,0,90,0};
  for(int i=0;i<8;i++){
   var holder=new GameObject(Groups[i]+" Station");holder.transform.SetParent(root.transform);holder.transform.position=positions[i];holder.transform.rotation=Quaternion.Euler(0,yaw[i],0);
   var model=Spawn(Base+"/Models/"+Groups[i]+".fbx",holder.transform);
   // Remove showcase rewards; actual rewards are inventory entries or installed instances.
   foreach(var t in model.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Pickup_")).ToArray())if(t)UnityEngine.Object.DestroyImmediate(t.gameObject);
   foreach(var t in model.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Plaque_")||t.name.StartsWith("Engraving_")||t.name.StartsWith("Word puzzle caption")).ToArray())if(t)UnityEngine.Object.DestroyImmediate(t.gameObject);
   if(i==2){var cp=Find(model.transform,"Compass_2");cp.localPosition=Find(model.transform,"CompassSocket_2").localPosition+new Vector3(0,0,.01f);}
   var focusObject=new GameObject("Focus - "+PoetryPuzzleGame.Titles[i]);focusObject.transform.SetParent(root.transform);focusObject.layer=29;
   var focus=focusObject.AddComponent<RoomFocusTarget>();focus.label=PoetryPuzzleGame.Titles[i];focus.priority=10;focus.surfaces=model.GetComponentsInChildren<Renderer>();
   Bounds b=focus.surfaces[0].bounds;foreach(var r in focus.surfaces)b.Encapsulate(r.bounds);
   focusObject.transform.position=b.center;var box=focusObject.AddComponent<BoxCollider>();box.size=b.size+Vector3.one*.035f;box.isTrigger=true;
   focus.lookAt=positions[i]+Vector3.up*(i==3?.30f:.12f);
   // Frontal overhead framing for tabletop mechanisms; the UI is on the right.
   Vector3 front=holder.transform.forward;
   focus.cameraPosition=focus.lookAt+front*(i==3?1.25f:.78f)+Vector3.up*(i==1?1.15f:1.03f);focus.fieldOfView=i==1?46:42;
   game.stations[i]=new PoetryPuzzleGame.Station{model=model.transform,focus=focus};targetList.Add(focus);
   Action<string,string,int> bind=(name,action,index)=>Hotspot(Find(model.transform,name),focus,i,action,index);
   switch(i){
    case 0:for(int k=0;k<3;k++)bind("DigitWheel_"+k,"digit",k);bind("Lock release catch","check",0);break;
    case 1:bind("WindingKeySocket","install",0);bind("River_Crank","cross",0);for(int k=0;k<3;k++)bind("Token_"+new[]{"Fox","Chicken","Grain"}[k],"cargo",k);break;
    case 2:for(int k=0;k<3;k++)bind("Compass_"+k,"compass",k);bind("CompassSocket_2","install",0);break;
    case 3:for(int k=0;k<9;k++)bind("WireTile_"+k,"wire",k);bind("LightBulbSocket","install",0);var l=new GameObject("Solved lamp light").AddComponent<Light>();l.transform.SetParent(Find(model.transform,"LightBulbSocket"),false);l.type=LightType.Point;l.color=new Color(1,.7f,.25f);l.intensity=1.1f;l.range=1.5f;l.enabled=false;break;
    case 4:for(int k=0;k<4;k++)bind("LetterWheel_"+k,"letter",k);bind("WordBox_Catch","check",0);break;
    case 5:for(int k=0;k<6;k++)bind("MoonTile_"+k,"moon",k);bind("MoonDiscSocket","install",0);bind("Moon_Reset","reset",0);break;
    case 6:for(int k=0;k<4;k++)bind("ObjectSocket_"+k,"slot",k);bind("TrayHandleSocket","install",0);break;
    case 7:for(int k=0;k<4;k++)bind("Install_WoodBlock_"+k,"slot",k);bind("FinalBox_Latch","check",0);break;
   }
   if(new[]{1,5,6,7}.Contains(i))Stand(holder.transform,positions[i].y,i==1?new Vector2(.88f,.68f):i==6?new Vector2(.77f,.38f):new Vector2(.59f,.55f));
  }
  string[] files={"Pickup_WindingKey","Compass_2","Pickup_LightBulb","Pickup_MoonDisc","Pickup_TrayHandle","Pickup_Pine","Pickup_Stone","Pickup_Boat","Pickup_Moon","Pickup_WoodBlock_第","Pickup_WoodBlock_七","Pickup_WoodBlock_码","Pickup_WoodBlock_头","Pickup_ExitKey"};
  game.itemPrefabs=new GameObject[files.Length];
  for(int i=0;i<files.Length;i++){
   var obj=Spawn(Base+"/Models/Items/"+files[i]+".fbx",null);obj.name=files[i];game.itemPrefabs[i]=PrefabUtility.SaveAsPrefabAsset(obj,Base+"/Prefabs/"+files[i]+".prefab");UnityEngine.Object.DestroyImmediate(obj);
  }
  var exploration=new List<PuzzleHotspot>();
  string[] names={"pine","stone","moon"};int[] itemIndex={5,6,8};string[] labels={"书架排序机关","床铺","书桌与信"};
  Vector3[] hiddenPos={new Vector3(2.02f,.455f,1.65f),new Vector3(.40f,.62f,-.35f),new Vector3(-1.72f,.49f,1.38f)};
  for(int i=0;i<3;i++){
   var obj=(GameObject)PrefabUtility.InstantiatePrefab(game.itemPrefabs[itemIndex[i]]);obj.name="Explore_"+names[i];obj.transform.position=hiddenPos[i];
   var owner=view.targets.First(t=>t.label==labels[i]);var h=Hotspot(obj.transform,owner,-1,"pickup",0);h.item=names[i];
   if(i==2){var drawer=UnityEngine.Object.FindObjectsByType<RoomOpenable>(FindObjectsSortMode.None).First(o=>o.name.Contains("Desk upper drawer"));obj.transform.SetParent(drawer.transform,true);h.container=drawer;}else obj.transform.SetParent(root.transform,true);
   owner.surfaces=owner.surfaces.Concat(obj.GetComponentsInChildren<Renderer>()).ToArray();exploration.Add(h);
  }
  game.exploration=exploration.ToArray();view.targets=targetList.ToArray();
  // Include imported objects in every focus isolation group, preserving floor and portal forest.
  var all=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None);
  foreach(var target in view.targets)target.hideWhenFocused=all.Where(r=>r.gameObject.layer!=30&&!target.surfaces.Contains(r)&&!r.name.StartsWith("Foundation")&&!r.name.StartsWith("Oak floorboard")).ToArray();
  game.stations[4].model.gameObject.SetActive(false);game.stations[7].model.gameObject.SetActive(false);
  Find(game.stations[2].model,"Compass_2").gameObject.SetActive(false);
  view.ReturnToOverview();view.Snap();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
  File.WriteAllText("../UnityValidation/puzzle_setup.txt","Installed 8 stations, 14 pickup prefabs, 3 exploration items, saved "+scene.path);
  Debug.Log("PUZZLE_INSTALL_COMPLETE");
 }
 static GameObject Spawn(string path,Transform parent){
  var obj=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));PrefabUtility.UnpackPrefabInstance(obj,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
  if(parent)obj.transform.SetParent(parent,false);
  foreach(var r in obj.GetComponentsInChildren<Renderer>(true)){
   var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++)if(mats[i])mats[i]=Material(mats[i]);r.sharedMaterials=mats;
  }return obj;
 }
 static Material Material(Material old){
  if(materials.TryGetValue(old.name,out var saved)&&saved)return saved;
  string name=old.name;string path=Base+"/Materials/"+name.Replace('/','_')+".mat";
  saved=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(!saved){saved=new Material(Shader.Find("Universal Render Pipeline/Lit"));saved.name=name;
   Color color=old.HasProperty("_Color")?old.GetColor("_Color"):Color.white;
   saved.SetColor("_BaseColor",name.Contains("AntiqueWalnut")?Color.white:color);
   if(name.Contains("AntiqueWalnut"))saved.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Base+"/Textures/AntiqueWalnut_BaseColor.png"));
   saved.SetFloat("_Metallic",name.Contains("Brass")?.65f:0);saved.SetFloat("_Smoothness",name.Contains("Brass")?.35f:.23f);saved.SetFloat("_Cull",0);
   AssetDatabase.CreateAsset(saved,path);
  }materials[name]=saved;return saved;
 }
 static PuzzleHotspot Hotspot(Transform t,RoomFocusTarget owner,int stage,string action,int index){
  var h=t.gameObject.AddComponent<PuzzleHotspot>();h.owner=owner;h.stage=stage;h.action=action;h.index=index;
  var colliders=t.GetComponentsInChildren<MeshFilter>(true);
  if(colliders.Length>0)foreach(var mf in colliders){mf.gameObject.layer=27;var c=mf.gameObject.AddComponent<MeshCollider>();c.sharedMesh=mf.sharedMesh;}
  else{t.gameObject.layer=27;var c=t.gameObject.AddComponent<BoxCollider>();c.size=Vector3.one*(action=="slot"?.10f:.075f);c.isTrigger=true;}
  return h;
 }
 static void Stand(Transform holder,float height,Vector2 size){
  var mat=AssetDatabase.LoadAssetAtPath<Material>(Base+"/Materials/PP_AntiqueWalnut.mat");
  Action<Vector3,Vector3> panel=(p,s)=>{var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name="Puzzle walnut stand";go.transform.SetParent(holder,false);go.transform.localPosition=p;go.transform.localScale=s;go.GetComponent<Renderer>().sharedMaterial=mat;UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());};
  panel(new Vector3(0,-.035f,0),new Vector3(size.x,.06f,size.y));
  foreach(int x in new[]{-1,1})foreach(int z in new[]{-1,1})panel(new Vector3(x*(size.x/2-.055f),-height/2-.03f,z*(size.y/2-.055f)),new Vector3(.055f,height-.06f,.055f));
 }
 [MenuItem("Tools/Poetry Room/Clear Puzzle Save")]
 public static void ClearSave(){PlayerPrefs.DeleteKey("PoetryRoom.Puzzle.v1");PlayerPrefs.Save();}
}
