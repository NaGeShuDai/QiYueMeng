using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class RoomFeedback:MonoBehaviour {
 PoetryPuzzleGame game;AudioSource music,effects;AudioClip click,good,bad;GUIStyle text;Font font;
 string thought="";float until;RoomFocusTarget lastFocus;bool lastDetail;
 public string Thought=>thought;
 public string LastSound {get;private set;}
 public bool AudioReady=>music&&music.clip&&click&&good&&bad;
 public void Initialize(PoetryPuzzleGame owner){
  game=owner;music=gameObject.AddComponent<AudioSource>();effects=gameObject.AddComponent<AudioSource>();
  music.clip=Resources.Load<AudioClip>("RoomAudio/background");music.loop=true;music.volume=.22f;music.spatialBlend=0;music.Play();effects.volume=.45f;
  click=Resources.Load<AudioClip>("RoomAudio/interact");good=Resources.Load<AudioClip>("RoomAudio/success");bad=Resources.Load<AudioClip>("RoomAudio/error");
  Think("先四处看看。拖动能转身，点近些，再按 Esc 退回来。",7);
 }
 public void Sound(string kind){LastSound=kind;var clip=kind=="success"?good:kind=="error"?bad:click;if(clip)effects.PlayOneShot(clip);}
 public void Think(string value,float seconds=3.6f){thought=value;until=Time.unscaledTime+seconds;}
 bool MissingRequirement(int stage){
  if(stage<0||stage>=game.State.solved.Length||game.State.solved[stage])return false;
  return !game.State.Ready(stage)||((stage==1||stage==2||stage==3||stage==5||stage==6)&&!game.State.installed[stage]);
 }
 public void AfterAction(int stage,string action,bool accepted,bool solved,bool installed,bool escaped){
  if(escaped||solved){Sound("success");Think(escaped?"门开了。该往前走了。":"咔哒……好像有什么打开了。");return;}
  if(installed){Sound("success");Think(stage==1?"锁开了，试试转动曲柄。":"正好合适。现在应该能动了。");return;}
  if(!accepted&&(MissingRequirement(stage)||(action=="exit"&&!game.State.escaped))){Sound("error");Think("好像缺了什么");return;}
  if(!accepted){Sound("error");Think(action=="cross"?"不能把它们这样留在岸上……重来吧。":action=="install"?"这个位置，似乎还缺一件合适的东西。":action=="cargo"?"船还没到它那边。":"好像不对，再想想。");return;}
  if(action=="slot"&&stage>=0){var slots=stage==6?game.State.order:game.State.blocks;if(slots.All(x=>!string.IsNullOrEmpty(x))){Sound("error");Think("都放进去了，可顺序好像不对。");return;}}
  Sound("interact");
  if(action=="reward"||action=="pickup")Think("先收起来，或许用得上。");
  else if(action=="reset")Think("回到最初的样子，再试一次。");
 }
 void Update(){
  if(!game)return;var v=game.view;
  if(lastFocus!=v.Focused||lastDetail!=v.IsDetail){lastFocus=v.Focused;lastDetail=v.IsDetail;Sound("interact");
   if(v.Focused){string[] hints={"这首诗，我好像读过……有几个字不一样。","一次只能带一个，另一岸也得顾着。","人间、怀疑、答案……都围着梦和现实。","让线路连起来，灯也许会亮。","回到英文原诗，数一数字母。","一片翻动，会牵动另外两片。","这几样东西，好像都藏在同一段黄昏里。","这些字，应该能拼成一个地方。"};int stage=game.ActiveStage;Think(MissingRequirement(stage)?"好像缺了什么":stage>=0?hints[stage]:"靠近看看，有没有遗漏的东西。");}
  }
 }
 public string HoverName(Vector2 screen){
  if(!game.view.Focused||game.BlocksPointer(screen))return "";
  var hits=Physics.RaycastAll(game.view.viewCamera.ScreenPointToRay(screen),100,(1<<27)|(1<<28),QueryTriggerInteraction.Collide).OrderBy(h=>h.distance);
  foreach(var hit in hits){
   var h=hit.collider.GetComponentInParent<PuzzleHotspot>();if(h&&h.owner==game.view.Focused&&h.Available){
    if(h.action=="install"&&game.State.installed[h.stage])continue;
    switch(h.action){case "reset":return "机关复位";case "cross":return "转动曲柄";case "digit":return "数字转轮";case "letter":return "字母转轮";case "compass":return "转动指南针";case "wire":return "转动线路";case "moon":return "翻转月片";case "cargo":return new[]{"狐狸","鸡","麦子"}[h.index];case "slot":return "放置 / 取回";case "check":return "锁扣";case "exit":return "门锁";case "reward":case "pickup":return PoetryPuzzleGame.ItemName(h.item);case "install":return h.stage==1?"钥匙锁":h.stage==2?"指南针凹槽":h.stage==3?"灯泡插座":h.stage==5?"中心月盘":"托盘把手";}
   }
   var door=hit.collider.GetComponentInParent<RoomOpenable>();if(door&&door.owner.Resolved==game.view.Focused)return door.IsOpen?"关上":"打开";
  }return "";
 }
 void OnGUI(){
  if(!game)return;if(text==null){font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial Unicode MS"},18);text=new GUIStyle(GUI.skin.label){font=font,fontSize=18,alignment=TextAnchor.MiddleCenter,wordWrap=true};text.normal.textColor=new Color(1,.92f,.76f);}
  if(!game.State.escaped&&Time.unscaledTime<until){var color=GUI.color;GUI.color=new Color(color.r,color.g,color.b,Mathf.Clamp01((until-Time.unscaledTime)/.6f));Label(new Rect(30,Screen.height-65,Mathf.Max(160,Screen.width-200),50),thought);GUI.color=color;}
  if(!string.IsNullOrEmpty(game.Selected)){int index=game.State.inventory.IndexOf(game.Selected);if(index>=0){var r=game.InventoryRect(index);Label(new Rect(r.x-185,r.y+20,150,32),PoetryPuzzleGame.ItemName(game.Selected));}}
  if(Mouse.current!=null){var p=Mouse.current.position.ReadValue();string name=HoverName(p);if(name!="")Label(new Rect(Mathf.Clamp(p.x-90,8,Screen.width-190),Mathf.Clamp(Screen.height-p.y+22,70,Screen.height-105),180,30),name);}
 }
 void Label(Rect rect,string value){var old=text.normal.textColor;text.normal.textColor=new Color(0,0,0,.85f);GUI.Label(new Rect(rect.x+1,rect.y+1,rect.width,rect.height),value,text);text.normal.textColor=old;GUI.Label(rect,value,text);}
 void OnDestroy(){if(font)Destroy(font);}
}
