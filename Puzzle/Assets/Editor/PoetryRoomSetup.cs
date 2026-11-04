using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class PoetryRoomSetup
{
 const string Root="Assets/PoetryRoom";
 static Renderer[] all;
 static GameObject model;
 static Camera camera;
 static RoomViewController controller;
 static Vector3 center,east,north;
 static Renderer First(string prefix)=>all.First(r=>r.name.StartsWith(prefix,StringComparison.Ordinal));
 static bool Starts(string name,params string[] prefixes)=>prefixes.Any(p=>name.StartsWith(p,StringComparison.Ordinal));
 static void Asset(UnityEngine.Object o,string path){AssetDatabase.CreateAsset(o,path);}
 [MenuItem("Tools/Poetry Room/Build Scene")]
 public static void Build()
 {
  if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play before building scene");
  Directory.CreateDirectory(Root+"/Materials");Directory.CreateDirectory(Root+"/Scenes");
  var importer=(ModelImporter)AssetImporter.GetAtPath(Root+"/Models/Poetry_Escape_Room_Complete.fbx");
  importer.isReadable=true;importer.importCameras=false;importer.importLights=false;importer.SaveAndReimport();
  var textureImporter=(TextureImporter)AssetImporter.GetAtPath(Root+"/Textures/Room_BaseColor.png");
  textureImporter.maxTextureSize=4096;textureImporter.textureCompression=TextureImporterCompression.Uncompressed;textureImporter.SaveAndReimport();
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models/Poetry_Escape_Room_Complete.fbx"));
  PrefabUtility.UnpackPrefabInstance(model,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
  model.name="Poetry Bedroom";
  all=model.GetComponentsInChildren<Renderer>();
  var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/Room_BaseColor.png");
  var materials=new Dictionary<string,Material>();
  foreach(var r in all) {
   bool forest=r.transform.parent && r.transform.parent.name.Contains("Forest_Remote");
   if(forest)r.gameObject.layer=30;
   var slots=r.sharedMaterials;
   for(int i=0;i<slots.Length;i++) {
    var old=slots[i];if(!old)continue;
    string key=old.name;
    if(!materials.TryGetValue(key,out var mat)) {
     mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));mat.name=key;
     Color color=old.HasProperty("_Color")?old.GetColor("_Color"):Color.white;
     mat.SetColor("_BaseColor",forest?color:Color.white);
     if(!forest)mat.SetTexture("_BaseMap",texture);
     mat.SetFloat("_Smoothness",key.Contains("brass")?.45f:.24f);
     mat.SetFloat("_Metallic",key.Contains("brass")?.65f:key.Contains("Mirror")?.9f:0);
     if(key.Contains("haze")){mat.SetFloat("_Cull",0);mat.SetColor("_BaseColor",new Color(.36f,.44f,.36f));}
     string safe=string.Concat(key.Select(c=>char.IsLetterOrDigit(c)||c=='_'?c:'_'));
     string path=Root+"/Materials/"+safe+".mat";
     if(AssetDatabase.LoadAssetAtPath<Material>(path))AssetDatabase.DeleteAsset(path);
     Asset(mat,path);materials.Add(key,mat);
    }
    slots[i]=mat;
   }
   r.sharedMaterials=slots;
  }
  center=First("Foundation").bounds.center;center.y=0;
  var portal=First("WINDOW_PORTAL");
  east=portal.bounds.center-center;east.y=0;east=Mathf.Abs(east.x)>Mathf.Abs(east.z)?Vector3.right*Mathf.Sign(east.x):Vector3.forward*Mathf.Sign(east.z);
  north=First("North wall").bounds.center-center;north.y=0;north.Normalize();
  camera=new GameObject("Main Camera").AddComponent<Camera>();camera.tag="MainCamera";camera.gameObject.AddComponent<AudioListener>();
  camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.095f,.105f,.10f);camera.nearClipPlane=.05f;camera.farClipPlane=150;
  camera.cullingMask=~((1<<30)|(1<<29));camera.allowHDR=true;
  camera.GetUniversalAdditionalCameraData().renderPostProcessing=false;
  controller=new GameObject("Room View and Focus").AddComponent<RoomViewController>();controller.viewCamera=camera;controller.roomCenter=center;
  var ceilings=all.Where(r=>Starts(r.name,"Ceiling","Ceiling oak beam")).ToArray();controller.ceilings=ceilings;foreach(var r in ceilings)r.enabled=false;
  var wallDefs=new List<RoomViewController.Wall>();
  AddWall(wallDefs,"North",north,r=>Starts(r.name,"North ","Poetry print","Print title","Print decorative"));
  AddWall(wallDefs,"East",east,r=>Starts(r.name,"East ","Window ","WINDOW_PORTAL","Shutter ","Plant stem","Plant leaf"));
  AddWall(wallDefs,"West",-east,r=>Starts(r.name,"West wall"));
  AddWall(wallDefs,"South",-north,r=>Starts(r.name,"South wall","Door ","EXIT -","Keyhole"));
  controller.walls=wallDefs.ToArray();
  // Match the initial overview to the window and bed wall, hiding their opposite walls.
  Vector3 eye=(-east-north).normalized;controller.yaw=Mathf.Atan2(-eye.x,-eye.z)*Mathf.Rad2Deg;controller.pitch=34;controller.distance=10.5f;
  var targets=new List<RoomFocusTarget>();
  AddTarget(targets,"书桌与信",r=>Starts(r.name,"Desk ","Final drawer","Word lock","START -","Letter ","Wax seal","Pen","Desk lamp","Lamp stem","Linen lamp"),true);
  AddTarget(targets,"书架排序机关",r=>Starts(r.name,"Bookcase ","Book spine","Book gilt","Sequence reward","Shelf drawer","Movable poem","Token "),false);
  AddTarget(targets,"床头柜转盘",r=>Starts(r.name,"Nightstand","Locked bedside","Bedside pull","State dial","Dial ","Old photograph","Photograph image"),true);
  AddTarget(targets,"衣柜与镜子",r=>Starts(r.name,"Wardrobe ","Inset mirror","Mirror brass","Overlay insertion","Hanging rail","Folded linen"),false);
  AddTarget(targets,"床铺",r=>Starts(r.name,"Bed ","Mattress","Duvet","Pillow","Headboard","Folded throw","Throw fringe"),true);
  AddTarget(targets,"窗外森林",r=>Starts(r.name,"Window jamb","Window crossbar","Window mullion","Window sill","WINDOW_PORTAL","Window wooden shutter"),false);
  AddTarget(targets,"出口",r=>Starts(r.name,"EXIT -","Door ","Keyhole"),false);
  AddTarget(targets,"墙上诗页",r=>Starts(r.name,"Poetry print","Print title","Print decorative"),false);
  controller.targets=targets.ToArray();
  var windowMat=new Material(Shader.Find("PoetryRoom/ForestWindow"));windowMat.name="Forest Window";
  if(AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/ForestWindow.mat"))AssetDatabase.DeleteAsset(Root+"/Materials/ForestWindow.mat");
  Asset(windowMat,Root+"/Materials/ForestWindow.mat");portal.sharedMaterial=windowMat;
  var forestCamera=new GameObject("Forest Camera - window only").AddComponent<Camera>();
  var window=new GameObject("Forest Window Projection").AddComponent<ForestWindow>();window.mainCamera=camera;window.forestCamera=forestCamera;window.windowSurface=portal;window.forestOffset=east*80;
  forestCamera.cullingMask=1<<30;forestCamera.enabled=false;
  var sun=new GameObject("Late afternoon sun").AddComponent<Light>();sun.type=LightType.Directional;sun.color=new Color(1,.85f,.67f);sun.intensity=1.7f;sun.transform.rotation=Quaternion.Euler(48,-32,0);sun.shadows=LightShadows.Soft;
  var fill=new GameObject("Room soft fill").AddComponent<Light>();fill.type=LightType.Point;fill.transform.position=center+Vector3.up*4;fill.range=12;fill.intensity=5;fill.color=new Color(1,.80f,.60f);fill.shadows=LightShadows.None;
  var lamp=new GameObject("Desk lamp glow").AddComponent<Light>();lamp.type=LightType.Point;lamp.transform.position=First("Lamp stem").bounds.center;lamp.range=2.2f;lamp.intensity=.65f;lamp.color=new Color(1,.61f,.25f);
  RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.42f,.43f,.40f);RenderSettings.ambientEquatorColor=new Color(.25f,.23f,.19f);RenderSettings.ambientGroundColor=new Color(.13f,.11f,.09f);
  controller.Snap();
  Selection.activeGameObject=controller.gameObject;
  if(SceneView.lastActiveSceneView)SceneView.lastActiveSceneView.LookAt(center+Vector3.up,Quaternion.LookRotation(-eye+Vector3.down*.6f),8);
  // Hide the remote forest in scene editing only, never disable it for its camera.
  foreach(var t in model.GetComponentsInChildren<Transform>())if(t.name=="Forest_Remote_80m")SceneVisibilityManager.instance.Hide(t.gameObject,true);
  EditorSceneManager.SaveScene(scene,Root+"/Scenes/PoetryBedroom.unity");
  EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Root+"/Scenes/PoetryBedroom.unity",true)};
  AssetDatabase.SaveAssets();
  Directory.CreateDirectory("../UnityValidation");
  File.WriteAllText("../UnityValidation/setup.txt","Room meshes="+all.Length+"\nEast="+east+" North="+north+"\nTargets="+targets.Count+"\n"+string.Join("\n",targets.Select(t=>t.label+" look="+t.lookAt+" camera="+t.cameraPosition)));
  Debug.Log("POETRY_SETUP_OK: scene saved, 8 focus targets, portal forest, dynamic cutaway.");
 }
 static void AddWall(List<RoomViewController.Wall> list,string name,Vector3 direction,Func<Renderer,bool> predicate) {
  list.Add(new RoomViewController.Wall{name=name,outward=direction,renderers=all.Where(predicate).ToArray(),colliders=new Collider[0]});
 }
 static void AddTarget(List<RoomFocusTarget> list,string label,Func<Renderer,bool> predicate,bool top) {
  var rs=all.Where(predicate).ToArray();if(rs.Length==0)throw new Exception("Missing focus group: "+label);
  Bounds b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);
  var o=new GameObject("Focus - "+label);o.layer=29;
  var t=o.AddComponent<RoomFocusTarget>();t.label=label;t.surfaces=rs;t.lookAt=b.center;
  Vector3 inward=center-b.center;inward.y=0;inward.Normalize();
  float span=Mathf.Max(b.size.x,b.size.z,b.size.y);
  t.cameraPosition=b.center+inward*Mathf.Max(1.35f,span*1.05f)+Vector3.up*(top?1.4f:.35f);
  if(label=="窗外森林") {t.cameraPosition=b.center-east*1.65f;t.lookAt=b.center;t.fieldOfView=54;}
  if(label=="衣柜与镜子")t.cameraPosition=b.center+inward*3.6f+Vector3.up*.4f;
  if(label=="书架排序机关"){t.lookAt=b.center+Vector3.up*.15f;t.cameraPosition=b.center+inward*2.4f+Vector3.up*1.05f;}
  if(label=="书桌与信") {t.lookAt=First("START -").bounds.center;t.cameraPosition=t.lookAt+inward*1.35f+Vector3.up*1.25f;}
  o.transform.position=b.center;var c=o.AddComponent<BoxCollider>();c.size=b.size+Vector3.one*.04f;c.isTrigger=true;list.Add(t);
 }
 [MenuItem("Tools/Poetry Room/Play")]
 public static void Play(){EditorApplication.isPlaying=true;}
 [MenuItem("Tools/Poetry Room/Stop")]
 public static void Stop(){EditorApplication.isPlaying=false;}
}
