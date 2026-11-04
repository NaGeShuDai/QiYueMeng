using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class RiverPadlockSetup {
 [MenuItem("Tools/Poetry Room/Add River Padlock")]
 public static void Apply(){
  if(Application.isPlaying)throw new Exception("Stop Play first");
  var g=UnityEngine.Object.FindFirstObjectByType<PoetryPuzzleGame>();var model=g.stations[1].model;
  if(model.Find("River Padlock"))throw new Exception("River padlock already exists");
  var brass=AssetDatabase.LoadAssetAtPath<Material>("Assets/PoetryRoom/PuzzleProps/Materials/PP_AgedBrass.mat");
  var dark=AssetDatabase.LoadAssetAtPath<Material>("Assets/PoetryRoom/PuzzleProps/Materials/PP_BlackenedMetal.mat");
  if(!dark){dark=new Material(brass);dark.color=new Color(.025f,.018f,.012f);AssetDatabase.CreateAsset(dark,"Assets/PoetryRoom/PuzzleProps/Materials/RiverLockKeyhole.mat");}
  var root=new GameObject("River Padlock").transform;root.SetParent(model,false);root.localPosition=new Vector3(.32f,-.345f,.12f);
  var component=root.gameObject.AddComponent<RiverPadlock>();component.game=g;component.closedPosition=root.localPosition;
  Action<string,PrimitiveType,Transform,Vector3,Vector3,Material> part=(name,type,parent,pos,size,material)=>{
   var o=GameObject.CreatePrimitive(type);o.name=name;o.transform.SetParent(parent,false);o.transform.localPosition=pos;o.transform.localScale=size;o.GetComponent<Renderer>().sharedMaterial=material;UnityEngine.Object.DestroyImmediate(o.GetComponent<Collider>());
  };
  part("Brass lock body",PrimitiveType.Cube,root,Vector3.zero,new Vector3(.105f,.038f,.085f),brass);
  part("Keyhole round",PrimitiveType.Cylinder,root,new Vector3(0,-.020f,.009f),new Vector3(.021f,.0015f,.021f),dark);
  part("Keyhole slot",PrimitiveType.Cube,root,new Vector3(0,-.021f,-.005f),new Vector3(.009f,.003f,.025f),dark);
  var loop=new GameObject("Opening shackle").transform;loop.SetParent(root,false);component.shackle=loop;
  Action<Vector3,Vector3> rod=(a,b)=>{
   var o=GameObject.CreatePrimitive(PrimitiveType.Cylinder);o.name="Shackle brass segment";o.transform.SetParent(loop,false);o.transform.localPosition=(a+b)*.5f;o.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);o.transform.localScale=new Vector3(.012f,(b-a).magnitude*.5f,.012f);o.GetComponent<Renderer>().sharedMaterial=brass;UnityEngine.Object.DestroyImmediate(o.GetComponent<Collider>());
  };
  rod(new Vector3(-.034f,0,.031f),new Vector3(-.034f,0,.071f));rod(new Vector3(.034f,0,.031f),new Vector3(.034f,0,.071f));
  for(int i=0;i<12;i++){float a=i*Mathf.PI/12,b=(i+1)*Mathf.PI/12;rod(new Vector3(Mathf.Cos(a)*.034f,0,.071f+Mathf.Sin(a)*.034f),new Vector3(Mathf.Cos(b)*.034f,0,.071f+Mathf.Sin(b)*.034f));}
  var socket=g.Node(1,"WindingKeySocket");var old=socket.GetComponent<PuzzleHotspot>();if(old)UnityEngine.Object.DestroyImmediate(old);foreach(var c in socket.GetComponents<Collider>())UnityEngine.Object.DestroyImmediate(c);
  root.gameObject.layer=27;var hit=root.gameObject.AddComponent<BoxCollider>();hit.center=new Vector3(0,0,.025f);hit.size=new Vector3(.12f,.065f,.15f);hit.isTrigger=true;
  var hotspot=root.gameObject.AddComponent<PuzzleHotspot>();hotspot.owner=g.stations[1].focus;hotspot.stage=1;hotspot.action="install";
  RoomFocusRepair.Repair();AssetDatabase.SaveAssets();Debug.Log("RIVER_PADLOCK_CREATED");
 }
}
