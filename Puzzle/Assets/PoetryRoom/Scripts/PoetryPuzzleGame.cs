using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PoetryPuzzleGame:MonoBehaviour
{
 [Serializable] public class Station { public Transform model; public RoomFocusTarget focus; }
 public Station[] stations=new Station[8];
 public RoomViewController view;
 public GameObject[] itemPrefabs;
 public PuzzleHotspot[] exploration;
 public RoomFocusTarget exitFocus;
 public Transform exitDoor;
 public bool persist=true;
 public PoetryPuzzleState State {get;private set;}
 public string Selected {get;private set;}="";
 public string Message {get;private set;}="先查看床头柜上的三位锁。";
 const string SaveKey="PoetryRoom.Puzzle.v1";
 readonly Dictionary<Transform,Vector3> origins=new Dictionary<Transform,Vector3>();
 readonly Dictionary<string,GameObject> placed=new Dictionary<string,GameObject>();
 bool firstUnlocked;
 public RoomFeedback Feedback {get;private set;}

 public static readonly string[] Titles={"三位密码锁","摆渡","三个指南针","接通航灯","归处密码盒","六面月相","四物排序","第七码头"};
 static readonly string[] Required={"","winding_key","compass","bulb","","moon_disc","tray_handle",""};
 static readonly string[] Reward={"上弦钥匙、‘第’木块","第三枚指南针","灯泡、‘七’木块","四字母密码盒（已放在书桌旁）","中心月盘、‘码’木块","小船、托盘把手","‘头’木块和最终木盒（已放在床尾）","出口钥匙"};
 public static string ItemName(string id){switch(id){case "winding_key":return "上弦钥匙";case "compass":return "指南针";case "bulb":return "灯泡";case "moon_disc":return "中心月盘";case "tray_handle":return "托盘把手";case "pine":return "星星";case "stone":return "酒杯";case "boat":return "山峰";case "moon":return "落日";case "exit_key":return "出口钥匙";case "block0":return "第";case "block1":return "七";case "block2":return "码";case "block3":return "头";default:return id;}}
 void Awake(){
  State=new PoetryPuzzleState();
  if(persist&&PlayerPrefs.HasKey(SaveKey)){try{var saved=JsonUtility.FromJson<PoetryPuzzleState>(PlayerPrefs.GetString(SaveKey));if(saved!=null&&saved.Valid()){saved.Upgrade();State=saved;}}catch(Exception){Message="存档无法读取，已开始新游戏。";}}
  foreach(var s in stations)foreach(var t in s.model.GetComponentsInChildren<Transform>(true))origins[t]=t.localPosition;
  if(State.pendingRewards==null)State.pendingRewards=new List<string>();
  firstUnlocked=State.solved[0];
  if(firstDrawer)firstDrawer.SetOpen(false);
 }
 public EndingPortal Ending {get;private set;}
 void Start(){Feedback=gameObject.AddComponent<RoomFeedback>();Feedback.Initialize(this);Ending=gameObject.AddComponent<EndingPortal>();Ending.Initialize(this);Refresh();}
 public int ActiveStage=>Array.FindIndex(stations,s=>s.focus==view.Focused);
 public Transform Node(int stage,string name)=>stations[stage].model.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name==name||t.name.StartsWith(name+".",StringComparison.Ordinal));
 public bool BlocksPointer(Vector2 screen){
  var point=new Vector2(screen.x,Screen.height-screen.y);
  if(ResetProgressRect.Contains(point))return true;
  if(State.escaped&&new Rect(Screen.width/2f-240,Screen.height-112,480,96).Contains(point))return true;
  if(State.escaped)return false;
  if(InventoryPanelRect.Contains(point))return true;
  return false;
 }
 public bool WorldClick(Vector2 pos){
  if(BlocksPointer(pos))return true;
  var ray=view.viewCamera.ScreenPointToRay(pos);
  var front=Physics.RaycastAll(ray,100,1<<28,QueryTriggerInteraction.Collide)
   .Where(hit=>{var p=hit.collider.GetComponentInParent<RoomOpenable>();return p&&p.owner&&p.owner.Resolved==view.Focused;})
   .Select(hit=>hit.distance).DefaultIfEmpty(float.MaxValue).Min();
  var hits=Physics.RaycastAll(ray,100,1<<27,QueryTriggerInteraction.Collide).OrderBy(h=>h.distance);
  foreach(var hit in hits){var h=hit.collider.GetComponentInParent<PuzzleHotspot>();if(h&&h.owner==view.Focused&&h.Available){
   if(h.action=="install"&&h.stage>=0&&State.installed[h.stage])continue;
   if(front<hit.distance-.002f)return false;
   Act(h.stage,h.action,h.index,h.item);return true;
  }}
  return false;
 }
 public void Select(string item){Selected=State.Has(item)?item:"";if(Feedback)Feedback.Sound("interact");}
 public void Tell(string message){Message=message;}
 public bool Act(int stage,string action,int index=0,string item=""){
  int solved=State.solved.Count(x=>x);bool installed=stage>=0&&State.installed[stage];bool escaped=State.escaped;
  bool result=ActCore(stage,action,index,item);
  if(Feedback)Feedback.AfterAction(stage,action,result,State.solved.Count(x=>x)>solved,stage>=0&&!installed&&State.installed[stage],!escaped&&State.escaped);
  return result;
 }
 bool ActCore(int stage,string action,int index,string item){
  if(State.escaped)return false;
  if(action=="reward"){
   if(!State.pendingRewards.Remove(item))return false;State.Give(item);Commit();return true;
  }
  if(action=="pickup"){
   var source=exploration.FirstOrDefault(h=>h&&h.item==item);
   if(!source||!source.Available){Tell("先打开存放它的抽屉。");return false;}
   if(!State.Collect(item))return false;Tell("找到："+ItemName(item));Commit();return true;
  }
  if(action=="exit"){
   if(!State.solved[7]||!State.Has("exit_key")||Selected!="exit_key"){Tell("门仍锁着，需要最后木盒里的钥匙。");return false;}
   State.inventory.Remove("exit_key");State.escaped=true;Tell("门开了。下一站，我们的故事。");Commit();if(Ending)Ending.Begin();return true;
  }
  if(stage<0||stage>7)return false;
  if(stage==0&&State.solved[0]&&action=="digit"){
   State.digits[index]=(State.digits[index]+1)%10;Commit();return true;
  }
  if(!State.Ready(stage)){Tell("机关缺少前一处奖励中的必要组件。");return false;}
  if(State.solved[stage]){Tell("这处机关已经完成。奖励已领取。");return false;}
  if(action=="install"){
   if(Required[stage]==""||State.installed[stage])return false;
   if(Selected!=Required[stage]){Tell("先在背包选择“"+ItemName(Required[stage])+"”，再安装。");return false;}
   if(!State.Install(stage,Selected))return false;Selected="";Tell("组件已安装，机关可以操作了。");Commit();return true;
  }
  if(Required[stage]!=""&&!State.installed[stage]){Tell("先安装："+ItemName(Required[stage]));return false;}
  switch(action){
   case "digit":State.digits[index]=(State.digits[index]+1)%10;break;
   case "compass":State.compass[index]=(State.compass[index]+1)%8;break;
   case "wire":State.wires[index]=(State.wires[index]+1)%4;break;
   case "letter":State.letters[index]=(State.letters[index]+1)%6;break;
   case "moon":State.Flip(index);break;
   case "cargo":
    if(State.riverBanks[index]!=State.boatBank){Tell("它在对岸，船够不到。");return false;}
    State.cargo=State.cargo==index?-1:index;break;
   case "cross":if(!State.Cross()){Tell("无人看守的一岸发生了争食。棋子已回到起点。每船只能带一件。");Commit();return false;}break;
   case "slot":
    var slots=stage==6?State.order:State.blocks;
    string[] allowed=stage==6?new[]{"pine","stone","boat","moon"}:new[]{"block0","block1","block2","block3"};
    if(string.IsNullOrEmpty(Selected)){
     if(!string.IsNullOrEmpty(slots[index])){State.Give(slots[index]);slots[index]=null;}else{Tell("先选择背包物品，再点击空槽。");return false;}
    }else{
     if(!allowed.Contains(Selected)||!State.Has(Selected)){Tell("这个物品不适合放进这里。");return false;}
     if(!string.IsNullOrEmpty(slots[index]))State.Give(slots[index]);
     slots[index]=Selected;State.inventory.Remove(Selected);Selected="";
    }break;
   case "reset":ResetPuzzle(stage);Tell("本处机关已复位，已安装组件会保留。");break;
   case "check":break;
   default:return false;
  }
  if(State.Check(stage))Tell("机关打开！获得："+Reward[stage]);
  else if(action=="check")Tell("还没有解开。请核对诗页中的线索。");
  Commit();return true;
 }
 void ResetPuzzle(int s){switch(s){
  case 0:State.digits=new int[3];break;case 1:State.ResetRiver();break;case 2:State.compass=new int[3];break;
  case 3:State.wires=new[]{1,2,1,3,2,0,2,1,3};break;case 4:State.letters=new[]{1,1,1,1};break;case 5:State.moons=PoetryPuzzleState.InitialMoons();break;
  case 6:case 7:var a=s==6?State.order:State.blocks;foreach(var id in a)if(!string.IsNullOrEmpty(id))State.Give(id);Array.Clear(a,0,a.Length);break;
 }}
 public void NewGame(){view.ReturnToOverview();foreach(var part in FindObjectsByType<RoomOpenable>(FindObjectsSortMode.None))if(part.IsOpen)part.SetOpen(false);State=new PoetryPuzzleState();Selected="";Commit();}
 public void ResetProgress(){NewGame();if(persist){PlayerPrefs.DeleteKey(SaveKey);PlayerPrefs.Save();}if(Feedback){Feedback.Think("重新来过，再仔细看看。");Feedback.Sound("interact");}}
 public void Commit(){if(persist){PlayerPrefs.SetString(SaveKey,JsonUtility.ToJson(State));PlayerPrefs.Save();}Refresh();}
 public void RestoreSnapshot(string json){var restored=JsonUtility.FromJson<PoetryPuzzleState>(json);if(restored==null||!restored.Valid())throw new ArgumentException("Invalid puzzle snapshot");if(restored.pendingRewards==null)restored.pendingRewards=new List<string>();restored.Upgrade();State=restored;Selected="";Refresh();}
 void Rotation(int stage,string name,Vector3 euler){var t=Node(stage,name);if(t)t.localRotation=Quaternion.Euler(euler);}
 void Slide(int stage,string name,bool open,float amount=.18f){var t=Node(stage,name);if(t&&origins.ContainsKey(t))t.localPosition=origins[t]+new Vector3(0,open?-amount:0,0);}
 void SetActive(int stage,string name,bool on){var t=Node(stage,name);if(t)t.gameObject.SetActive(on);}
 GameObject Item(string id){string[] ids={"winding_key","compass","bulb","moon_disc","tray_handle","pine","stone","boat","moon","block0","block1","block2","block3","exit_key"};int i=Array.IndexOf(ids,id);return i>=0&&i<itemPrefabs.Length?itemPrefabs[i]:null;}
 void Place(string key,string item,Transform socket){
  if(placed.TryGetValue(key,out var old)){
   if(old&&old.name==item&&old.transform.parent==socket){old.SetActive(true);return;}if(old){old.SetActive(false);Destroy(old);}placed.Remove(key);
  }
  if(string.IsNullOrEmpty(item)||!socket)return;
  var prefab=Item(item);if(!prefab)return;
  var go=Instantiate(prefab,socket);go.name=item;
  go.transform.localPosition=Vector3.zero;go.transform.localRotation=Quaternion.identity;go.transform.localScale=Vector3.one;
  // Item FBX root contains the axis conversion; sockets already live inside that conversion.
  foreach(var c in go.GetComponentsInChildren<Collider>()){c.enabled=false;Destroy(c);}
  foreach(var r in go.GetComponentsInChildren<Renderer>())r.gameObject.layer=0;
  placed[key]=go;
 }
 public void Refresh(){
  if(State==null)return;
  SetActive(4,"Poetry LOVE clues",!State.solved[4]);
  if(exitDoor)exitDoor.localRotation=Quaternion.Euler(0,State.escaped?-100:0,0);
  if(Ending)Ending.Sync(State.escaped);
  stations[4].model.gameObject.SetActive(State.solved[3]);stations[7].model.gameObject.SetActive(State.solved[6]);
  for(int i=0;i<3;i++)Rotation(0,"DigitWheel_"+i,new Vector3(0,0,State.digits[i]*36));
  for(int i=0;i<3;i++)Rotation(2,"CompassNeedle_"+i,new Vector3(0,0,State.compass[i]*45));
  SetActive(2,"Compass_2",State.installed[2]);
  for(int i=0;i<9;i++)Rotation(3,"WireTile_"+i,new Vector3(0,0,State.wires[i]*90));
  for(int i=0;i<4;i++)Rotation(4,"LetterWheel_"+i,new Vector3(0,0,State.letters[i]*60));
  for(int i=0;i<6;i++)Rotation(5,"MoonTile_"+i,new Vector3(State.moons[i]==1?0:180,0,0));
  // The river key is consumed by the padlock; it no longer remains on the crank.
  Place("winding",null,Node(1,"WindingKeySocket"));
  Place("bulb",State.installed[3]?"bulb":null,Node(3,"LightBulbSocket"));
  Place("center",State.installed[5]?"moon_disc":null,Node(5,"MoonDiscSocket"));
  Place("handle",State.installed[6]?"tray_handle":null,Node(6,"TrayHandleSocket"));
  for(int i=0;i<4;i++){Place("order"+i,State.order[i],Node(6,"ObjectSocket_"+i));Place("block"+i,State.blocks[i],Node(7,"Install_WoodBlock_"+i));}
  var boat=Node(1,"Boat_Carriage");boat.localPosition=origins[boat]+new Vector3(State.boatBank==1?-.42f:0,0,0);
  string[] animals={"Fox","Chicken","Grain"};for(int i=0;i<3;i++){
   var t=Node(1,"Token_"+animals[i]);t.localPosition=origins[t]+new Vector3(State.riverBanks[i]==1?-.56f:0,0,0);
   if(State.cargo==i)t.position=Node(1,"CargoSocket").position;
  }
  Rotation(1,"Boathouse_Door",new Vector3(0,0,State.solved[1]?-100:0));
  Rotation(1,"BoathouseRoofHinge",new Vector3(State.solved[1]?-110:0,0,0));
  Slide(2,"CompassRewardDrawer",State.solved[2]);Slide(3,"LampRewardDrawer",State.solved[3]);
  Slide(5,"MoonRewardDrawer",State.solved[5]);Slide(6,"SequenceRewardDrawer",State.solved[6]);
  Rotation(4,"WordBox_Lid",new Vector3(State.solved[4]?-105:0,0,0));
  Rotation(7,"FinalBox_Lid",new Vector3(State.solved[7]?-105:0,0,0));
  var light=stations[3].model.GetComponentInChildren<Light>(true);if(light)light.enabled=State.solved[3];
  foreach(var h in exploration)if(h)h.gameObject.SetActive(!State.found.Contains(h.item));
  RefreshRewards();
  // Refresh focus surfaces after dynamically installing pickups, including hidden/solved boxes.
  foreach(var s in stations)s.focus.surfaces=s.model.GetComponentsInChildren<Renderer>(true);
  if(firstDrawer)stations[0].focus.surfaces=stations[0].focus.surfaces.Concat(firstDrawer.owner.surfaces).Concat(firstDrawer.GetComponentsInChildren<Renderer>(true)).Distinct().ToArray();
  RefreshIsolation();
 }
 public void RefreshIsolation(){
  foreach(var focus in view.targets)focus.hideWhenFocused=Array.Empty<Renderer>();
 }
 public static readonly string[] ItemIds={"winding_key","compass","bulb","moon_disc","tray_handle","pine","stone","boat","moon","block0","block1","block2","block3","exit_key"};
 public Texture2D[] inventoryIcons;
 public Transform[] rewardSockets;
 public RoomOpenable firstDrawer;
 GUIStyle resetStyle;
 Font resetFont;
 Texture2D selectionArrow;
 public Rect ResetProgressRect=>new Rect(Screen.width-148,16,132,42);
 int InventorySlots=>Mathf.Max(6,State.inventory.Count);
 float InventorySize=>Mathf.Min(72,(Screen.height-104f)/InventorySlots-7);
 public Rect InventoryPanelRect=>new Rect(Screen.width-InventorySize-52,74,InventorySize+40,InventorySlots*(InventorySize+7)+12);
 public Rect InventoryRect(int index){
  float size=InventorySize;
  return new Rect(Screen.width-size-20,80+index*(size+7),size,size);
 }
 public void ClickInventory(Vector2 screen){
  var point=new Vector2(screen.x,Screen.height-screen.y);
  if(ResetProgressRect.Contains(point)){ResetProgress();return;}
  if(State.escaped)return;
  for(int i=0;i<State.inventory.Count;i++)if(InventoryRect(i).Contains(point)){string id=State.inventory[i];Select(Selected==id?"":id);return;}
 }
 void OnGUI(){
  if(State==null)return;
  SetActive(4,"Poetry LOVE clues",!State.solved[4]);
  if(resetStyle==null){
   resetFont=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Noto Sans CJK SC","Arial Unicode MS"},18);
   resetStyle=new GUIStyle(GUI.skin.label){font=resetFont,fontSize=18,alignment=TextAnchor.MiddleCenter};
   resetStyle.normal.textColor=new Color(1,.91f,.73f);
  }
  var buttonColor=GUI.backgroundColor;
  GUI.backgroundColor=new Color(.48f,.32f,.19f,1);
  GUI.Label(ResetProgressRect,"重置进度",resetStyle);
  GUI.backgroundColor=buttonColor;
  if(State.escaped)return;
  var originalColor=GUI.color;
  GUI.color=new Color(.10f,.065f,.04f,.86f);GUI.DrawTexture(InventoryPanelRect,Texture2D.whiteTexture);
  GUI.color=originalColor;
  for(int i=0;i<InventorySlots;i++){
   string id=i<State.inventory.Count?State.inventory[i]:null;int icon=Array.IndexOf(ItemIds,id);Rect rect=InventoryRect(i);
   bool selected=id!=null&&Selected==id;
   GUI.color=selected?new Color(1,.76f,.35f):new Color(.43f,.31f,.19f);
   GUI.DrawTexture(rect,Texture2D.whiteTexture);
   GUI.color=new Color(.16f,.11f,.075f);GUI.DrawTexture(new Rect(rect.x+2,rect.y+2,rect.width-4,rect.height-4),Texture2D.whiteTexture);
   GUI.color=originalColor;
   if(icon>=0&&inventoryIcons!=null&&icon<inventoryIcons.Length&&inventoryIcons[icon])GUI.DrawTexture(new Rect(rect.x+5,rect.y+5,rect.width-10,rect.height-10),inventoryIcons[icon],ScaleMode.ScaleToFit,true);
   if(selected){
    if(!selectionArrow){
     selectionArrow=new Texture2D(24,20,TextureFormat.RGBA32,false);selectionArrow.filterMode=FilterMode.Point;
     for(int y=0;y<20;y++)for(int x=0;x<24;x++)selectionArrow.SetPixel(x,y,(x<13&&Mathf.Abs(y-9.5f)<3)||(x>=10&&x<=23-Mathf.Abs(y-9.5f)*1.3f)?new Color(1,.76f,.35f):Color.clear);
     selectionArrow.Apply();
    }
    GUI.DrawTexture(new Rect(rect.x-28,rect.center.y-10,24,20),selectionArrow);
   }
  }
 }
 void OnDestroy(){if(selectionArrow)Destroy(selectionArrow);if(resetFont)Destroy(resetFont);}
 void RefreshRewards(){
  // Opening is a one-time unlock event, not a pose enforced by every inventory refresh.
  if(firstDrawer){
   if(!State.solved[0])firstDrawer.SetOpen(false);
   else if(!firstUnlocked)firstDrawer.SetOpen(true);
  }
  firstUnlocked=State.solved[0];
  if(rewardSockets==null)return;
  for(int i=0;i<ItemIds.Length;i++){
   if(i>=rewardSockets.Length||!rewardSockets[i])continue;
   string id=ItemIds[i],key="reward_"+id;
   int socketIndex=i;
   if(State.ShuffledBlocks&&i>=9&&i<=12)socketIndex=new[]{10,12,9,11}[i-9];
   Place(key,State.pendingRewards.Contains(id)?id:null,rewardSockets[socketIndex]);
   if(!placed.TryGetValue(key,out var obj)||!obj)continue;
   var hotspot=obj.GetComponent<PuzzleHotspot>();
   if(!hotspot){
    hotspot=obj.AddComponent<PuzzleHotspot>();hotspot.action="reward";hotspot.item=id;
    int[] stages={0,1,2,4,5,-1,-1,5,-1,0,2,4,6,7};hotspot.stage=stages[socketIndex];hotspot.owner=stations[hotspot.stage].focus;if(hotspot.stage==0)hotspot.container=firstDrawer;
    var c=obj.AddComponent<BoxCollider>();var meshes=obj.GetComponentsInChildren<MeshFilter>();
    Bounds b=new Bounds(Vector3.zero,Vector3.zero);bool initial=true;
    foreach(var mesh in meshes){var mb=mesh.sharedMesh.bounds;for(int corner=0;corner<8;corner++){
     var v=mb.center+Vector3.Scale(mb.extents,new Vector3((corner&1)==0?-1:1,(corner&2)==0?-1:1,(corner&4)==0?-1:1));
     v=obj.transform.InverseTransformPoint(mesh.transform.TransformPoint(v));if(initial){b=new Bounds(v,Vector3.zero);initial=false;}else b.Encapsulate(v);
    }}
    c.center=b.center;c.size=b.size+Vector3.one*.012f;c.isTrigger=true;obj.layer=27;
   }
  }
 }
}
