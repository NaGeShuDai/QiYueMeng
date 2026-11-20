from pathlib import Path
p=Path('Puzzle/Assets/PoetryRoom/Scripts/PoetryPuzzleGame.cs')
s=p.read_text(encoding='utf8')
s=s[:s.index(' void Styles()')]+''' public static readonly string[] ItemIds={"winding_key","compass","bulb","moon_disc","tray_handle","pine","stone","boat","moon","block0","block1","block2","block3","exit_key"};
 public Texture2D[] inventoryIcons;
 public Transform[] rewardSockets;
 public RoomOpenable firstDrawer;
 public Rect InventoryRect(int index){
  float size=Mathf.Min(76,Screen.width/15f),width=State.inventory.Count*(size+7);
  return new Rect((Screen.width-width)/2+index*(size+7),Screen.height-size-18,size,size);
 }
 void OnGUI(){
  if(State==null||State.escaped)return;
  for(int i=0;i<State.inventory.Count;i++){
   string id=State.inventory[i];int icon=Array.IndexOf(ItemIds,id);Rect rect=InventoryRect(i);
   var old=GUI.backgroundColor;GUI.backgroundColor=Selected==id?new Color(1,.72f,.25f):new Color(.35f,.29f,.20f,.85f);
   if(GUI.Button(rect,GUIContent.none))Select(Selected==id?"":id);
   GUI.backgroundColor=old;
   if(icon>=0&&inventoryIcons!=null&&icon<inventoryIcons.Length&&inventoryIcons[icon])GUI.DrawTexture(new Rect(rect.x+5,rect.y+5,rect.width-10,rect.height-10),inventoryIcons[icon],ScaleMode.ScaleToFit,true);
  }
 }
 void RefreshRewards(){
  if(firstDrawer)firstDrawer.SetOpen(State.solved[0]);
  if(rewardSockets==null)return;
  for(int i=0;i<ItemIds.Length;i++){
   if(i>=rewardSockets.Length||!rewardSockets[i])continue;
   string id=ItemIds[i],key="reward_"+id;
   Place(key,State.pendingRewards.Contains(id)?id:null,rewardSockets[i]);
   if(!placed.TryGetValue(key,out var obj)||!obj)continue;
   var hotspot=obj.GetComponent<PuzzleHotspot>();
   if(!hotspot){
    hotspot=obj.AddComponent<PuzzleHotspot>();hotspot.action="reward";hotspot.item=id;
    int[] stages={0,1,2,4,5,-1,-1,5,-1,0,2,4,6,7};hotspot.stage=stages[i];hotspot.owner=stations[stages[i]].focus;
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
'''
s=s.replace(' Font font; GUIStyle heading,body,button; float noticeUntil;','')
s=s.replace('  font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","Arial"},18);','  if(State.pendingRewards==null)State.pendingRewards=new List<string>();')
a=s.index('  if(Journal||ResetPrompt||State.escaped)return true;');b=s.index('\n }',a)
s=s[:a]+'''  if(State.escaped)return false;
  var point=new Vector2(screen.x,Screen.height-screen.y);
  for(int i=0;i<State.inventory.Count;i++)if(InventoryRect(i).Contains(point))return true;
  return false;'''+s[b:]
s=s.replace('public void Tell(string message){Message=message;noticeUntil=Time.unscaledTime+7;}','public void Tell(string message){Message=message;}')
s=s.replace('  if(action=="pickup"){','''  if(action=="reward"){
   if(!State.pendingRewards.Remove(item))return false;State.Give(item);Commit();return true;
  }
  if(action=="pickup"){''')
s=s.replace('if(!State.solved[7]||!State.Has("exit_key"))','if(!State.solved[7]||!State.Has("exit_key")||Selected!="exit_key")')
s=s.replace('  foreach(var h in exploration)if(h)h.gameObject.SetActive(!State.found.Contains(h.item));','  foreach(var h in exploration)if(h)h.gameObject.SetActive(!State.found.Contains(h.item));\n  RefreshRewards();')
s=s.replace('   if(old&&old.name==item){old.SetActive(true);return;}if(old)Destroy(old);placed.Remove(key);','   if(old&&old.name==item){old.SetActive(true);return;}if(old){old.SetActive(false);Destroy(old);}placed.Remove(key);')
s=s.replace('foreach(var c in go.GetComponentsInChildren<Collider>())Destroy(c);','foreach(var c in go.GetComponentsInChildren<Collider>()){c.enabled=false;Destroy(c);}')
s=s.replace('foreach(var hit in hits){var h=hit.collider.GetComponentInParent<PuzzleHotspot>();if(h&&h.owner==view.Focused&&h.Available){Act(h.stage,h.action,h.index,h.item);return true;}}','''foreach(var hit in hits){var h=hit.collider.GetComponentInParent<PuzzleHotspot>();if(h&&h.owner==view.Focused&&h.Available){
   if(h.action=="install"&&h.stage>=0&&State.installed[h.stage])continue;
   Act(h.stage,h.action,h.index,h.item);return true;
  }}''')
p.write_text(s,encoding='utf8')
