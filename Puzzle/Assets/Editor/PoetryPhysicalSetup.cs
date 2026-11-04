using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class PoetryPhysicalSetup {
 const string Folder="Assets/PoetryRoom/PuzzleProps/Icons";
 [MenuItem("Tools/Poetry Room/Apply Model Only Interaction")]
 public static void Apply(){
  if(Application.isPlaying)throw new Exception("Stop Play first");
  var game=UnityEngine.Object.FindFirstObjectByType<PoetryPuzzleGame>();
  if(!File.Exists("Assets/PoetryRoom/Scenes/PoetryBedroom_BeforeModelOnly.unity"))EditorSceneManager.SaveScene(game.gameObject.scene,"Assets/PoetryRoom/Scenes/PoetryBedroom_BeforeModelOnly.unity",true);
  foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))
   if(new[]{"River rule","Word puzzle caption","Destination caption","Card typography","Print title","Print decorative"}.Any(n=>r.name.StartsWith(n)))r.gameObject.SetActive(false);
  // The first lock actually releases the existing bedside drawer.
  game.firstDrawer=UnityEngine.Object.FindObjectsByType<RoomOpenable>(FindObjectsSortMode.None).First(p=>p.name.Contains("Locked bedside drawer"));game.firstDrawer.puzzleLockStage=0;
  var first=game.stations[0];first.focus.surfaces=first.model.GetComponentsInChildren<Renderer>(true).Concat(game.firstDrawer.owner.surfaces).Concat(game.firstDrawer.GetComponentsInChildren<Renderer>()).Distinct().ToArray();
  first.focus.lookAt=new Vector3(-1.55f,.55f,-1.55f);first.focus.cameraPosition=first.focus.lookAt+new Vector3(0,1.4f,1.5f);
  // Use the real keyhole, rather than a screen button, to unlock the exit.
  var keyhole=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(t=>t.name.StartsWith("Keyhole"));
  var exit=keyhole.GetComponent<PuzzleHotspot>();if(!exit)exit=keyhole.gameObject.AddComponent<PuzzleHotspot>();exit.owner=game.exitFocus;exit.action="exit";exit.stage=-1;keyhole.gameObject.layer=27;
  var kc=keyhole.GetComponent<BoxCollider>();if(!kc)kc=keyhole.gameObject.AddComponent<BoxCollider>();kc.size=new Vector3(.07f,.10f,.045f);
  // Expand tiny installation sockets without adding visible UI controls.
  foreach(var h in UnityEngine.Object.FindObjectsByType<PuzzleHotspot>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
   var c=h.GetComponent<BoxCollider>();if(c&&h.action=="install")c.size=Vector3.one*.11f;
  }
  // Physical reset pegs for the three minigames. The moon already has one.
  foreach(int i in new[]{1,3}){
   var model=game.stations[i].model;var existing=model.Find("Physical reset peg");if(existing){if(i==3)existing.localPosition=new Vector3(.25f,.20f,game.Node(3,"WireTile_0").localPosition.z+.025f);continue;}
   var button=GameObject.CreatePrimitive(PrimitiveType.Cylinder);button.name="Physical reset peg";button.transform.SetParent(model,false);
   button.transform.localPosition=i==1?new Vector3(.32f,-.25f,.16f):new Vector3(.25f,.20f,game.Node(3,"WireTile_0").localPosition.z+.025f);button.transform.localRotation=Quaternion.Euler(90,0,0);button.transform.localScale=new Vector3(.035f,.012f,.035f);button.layer=27;
   button.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/PoetryRoom/PuzzleProps/Materials/PP_AgedBrass.mat");
   var h=button.AddComponent<PuzzleHotspot>();h.owner=game.stations[i].focus;h.stage=i;h.action="reset";
  }
  game.rewardSockets=new Transform[14];
  Action<int,int,string,Vector3> socket=(item,stage,parentName,position)=>{
   Transform parent=parentName=="first"?game.firstDrawer.transform:parentName=="root"?game.stations[stage].model:game.Node(stage,parentName);
   string name="PhysicalReward_"+PoetryPuzzleGame.ItemIds[item];var anchor=parent.Find(name);if(!anchor){anchor=new GameObject(name).transform;anchor.SetParent(parent,false);}
   anchor.localPosition=position;anchor.localRotation=parentName=="first"?game.stations[0].model.rotation:Quaternion.identity;game.rewardSockets[item]=anchor;
  };
  socket(0,0,"first",new Vector3(-.11f,-.035f,-.14f));socket(9,0,"first",new Vector3(.11f,-.035f,-.14f));
  socket(1,1,"root",new Vector3(-.27f,-.12f,.16f));
  socket(2,2,"CompassRewardDrawer",new Vector3(-.10f,-.05f,.025f));socket(10,2,"CompassRewardDrawer",new Vector3(.10f,-.05f,.025f));
  socket(3,4,"root",new Vector3(-.10f,-.01f,.035f));socket(11,4,"root",new Vector3(.10f,-.01f,.025f));
  socket(7,5,"MoonRewardDrawer",new Vector3(-.065f,-.07f,.025f));socket(4,5,"MoonRewardDrawer",new Vector3(.065f,-.07f,.025f));
  socket(12,6,"SequenceRewardDrawer",new Vector3(0,-.04f,.025f));socket(13,7,"root",new Vector3(0,0,.035f));
  if(game.inventoryIcons==null||game.inventoryIcons.Length!=14||game.inventoryIcons.Any(x=>!x))GenerateIcons(game);
  game.RefreshIsolation();EditorUtility.SetDirty(game);EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);AssetDatabase.SaveAssets();Debug.Log("MODEL_ONLY_SETUP_COMPLETE");
 }
 static void GenerateIcons(PoetryPuzzleGame game){
  Directory.CreateDirectory(Folder);game.inventoryIcons=new Texture2D[game.itemPrefabs.Length];
  var camera=new GameObject("Temporary item portrait camera").AddComponent<Camera>();camera.enabled=false;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.065f,.055f,.035f,0);camera.cullingMask=1<<26;camera.orthographic=true;camera.nearClipPlane=.001f;camera.farClipPlane=20;camera.allowHDR=false;
  var light=new GameObject("Temporary icon light").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.7f;light.cullingMask=1<<26;light.transform.rotation=Quaternion.Euler(45,-30,0);
  var rt=new RenderTexture(256,256,24,RenderTextureFormat.ARGB32);rt.Create();camera.targetTexture=rt;
  try{for(int i=0;i<game.itemPrefabs.Length;i++){
   var obj=UnityEngine.Object.Instantiate(game.itemPrefabs[i]);obj.transform.position=new Vector3(1000,1000,1000);
   var renderers=obj.GetComponentsInChildren<Renderer>();Bounds bounds=renderers[0].bounds;foreach(var r in renderers){r.gameObject.layer=26;bounds.Encapsulate(r.bounds);}
   float size=Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z);camera.orthographicSize=size*.69f;
   camera.transform.position=bounds.center+new Vector3(.05f,.85f,.50f).normalized*2;camera.transform.LookAt(bounds.center);camera.Render();
   var previous=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(256,256,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,256,256),0,0);image.Apply();
   string path=Folder+"/"+PoetryPuzzleGame.ItemIds[i]+".png";File.WriteAllBytes(path,image.EncodeToPNG());RenderTexture.active=previous;
   UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(obj);AssetDatabase.ImportAsset(path);
   var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.SaveAndReimport();game.inventoryIcons[i]=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
  }}finally{camera.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(camera.gameObject);UnityEngine.Object.DestroyImmediate(light.gameObject);}
 }
}
