using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public sealed class PoetryPuzzleState
{
 public int version=4;
 public bool legacyBlockOrder;
 public bool ShuffledBlocks=>version>=3&&!legacyBlockOrder;
 public string RewardBlock(int stage){int index=stage==0?0:stage==2?1:stage==4?2:3;return "block"+(ShuffledBlocks?new[]{2,0,3,1}[index]:index);}
 public bool[] solved=new bool[8];
 public bool[] installed=new bool[8];
 public List<string> inventory=new List<string>();
 public List<string> found=new List<string>();
 public List<string> pendingRewards=new List<string>();
 public int[] digits=new int[3], compass=new int[3], letters={1,1,1,1};
 public static readonly string[] LetterValues={"LABCDE","OAEIUR","VNOPQR","EAIOTY"};
 public string Word=>string.Concat(letters.Select((value,index)=>LetterValues[index][value]));
 public int[] wires={1,2,1,3,2,0,2,1,3};
 public static int[] InitialMoons()=>new[]{1,1,0,1,0,0};
 public int[] moons=InitialMoons();
 public void Upgrade(){if(version>=4)return;legacyBlockOrder=version<3;if(version<2){if(!solved[4])letters=new[]{1,1,1,1};if(!solved[5])moons=InitialMoons();}digits=solved[0]?new[]{5,6,8}:new int[3];compass=solved[2]?new[]{1,5,3}:new int[3];if(solved[6])order=new[]{"stone","moon","boat","pine"};version=4;}
 public string[] order=new string[4], blocks=new string[4];
 public int[] riverBanks=new int[3];
 public int boatBank, cargo=-1;
 public bool escaped;
 public bool Ready(int stage)=>stage==0||solved[stage-1];
 public bool Has(string item)=>inventory.Contains(item);
 public void Give(string item){if(!inventory.Contains(item))inventory.Add(item);}
 public bool Install(int stage,string item){if(!Ready(stage)||!inventory.Remove(item))return false;installed[stage]=true;return true;}
 public bool Collect(string item){if(found.Contains(item))return false;found.Add(item);Give(item);return true;}
 public bool Complete(int stage){
  if(solved[stage]||!Ready(stage))return false;
  solved[stage]=true;
  switch(stage){
   case 0:pendingRewards.AddRange(new[]{"winding_key",RewardBlock(0)});break;
   case 1:pendingRewards.Add("compass");break;
   case 2:pendingRewards.AddRange(new[]{"bulb",RewardBlock(2)});break;
   case 4:pendingRewards.AddRange(new[]{"moon_disc",RewardBlock(4)});break;
   case 5:pendingRewards.AddRange(new[]{"boat","tray_handle"});break;
   case 6:pendingRewards.Add(RewardBlock(6));break;
   case 7:pendingRewards.Add("exit_key");break;
  }
  return true;
 }
 public bool Check(int stage){
  if(!Ready(stage)||solved[stage])return false;
  bool pass=false;
  switch(stage){
   case 0:pass=digits.SequenceEqual(new[]{5,6,8});break;
   case 1:pass=installed[1]&&riverBanks.All(b=>b==1);break;
   case 2:pass=installed[2]&&compass.SequenceEqual(new[]{1,5,3});break;
   case 3:pass=installed[3]&&WireConnected();break;
   case 4:pass=Word=="LOVE";break;
   case 5:pass=installed[5]&&moons.All(v=>v==1);break;
   case 6:pass=installed[6]&&order.SequenceEqual(new[]{"stone","moon","boat","pine"});break;
   case 7:pass=blocks.SequenceEqual(new[]{"block0","block1","block2","block3"});break;
  }
  return pass&&Complete(stage);
 }
 public void ResetRiver(){riverBanks=new int[3];boatBank=0;cargo=-1;}
 public bool Cross(){
  if(!installed[1]||solved[1])return false;
  if(cargo>=0&&riverBanks[cargo]!=boatBank){cargo=-1;return false;}
  boatBank=1-boatBank;if(cargo>=0)riverBanks[cargo]=boatBank;cargo=-1;
  bool unsafeBank=(riverBanks[0]==riverBanks[1]&&riverBanks[1]!=boatBank)||(riverBanks[1]==riverBanks[2]&&riverBanks[1]!=boatBank);
  if(unsafeBank){ResetRiver();return false;}return true;
 }
 // Each press flips itself, the next clockwise plate and the opposite plate.
 public void Flip(int i){foreach(int offset in new[]{0,1,3})moons[(i+offset)%6]^=1;}
 // Bits are N,E,S,W. Row zero is north, clockwise rotation is positive.
 public int Ports(int index){
  int[] source={6,12,5,9,3,12,5,5,3};
  int p=source[index];for(int i=0;i<wires[index];i++)p=((p<<1)&15)|(p>>3);return p;
 }
 public bool WireConnected(){
  if((Ports(3)&8)==0||(Ports(8)&2)==0)return false;
  var seen=new HashSet<int>{3};var queue=new Queue<int>();queue.Enqueue(3);
  int[] dx={0,1,0,-1},dy={-1,0,1,0};
  while(queue.Count>0){int n=queue.Dequeue();for(int d=0;d<4;d++){
   int x=n%3+dx[d],y=n/3+dy[d];if(x<0||x>2||y<0||y>2)continue;
   int next=y*3+x;if((Ports(n)&(1<<d))!=0&&(Ports(next)&(1<<((d+2)%4)))!=0&&seen.Add(next))queue.Enqueue(next);
  }}
  return seen.Contains(0)&&seen.Contains(4)&&seen.Contains(8);
 }
 public bool Valid()=>(version>=1&&version<=4)&&solved?.Length==8&&installed?.Length==8&&digits?.Length==3&&compass?.Length==3&&letters?.Length==4&&wires?.Length==9&&moons?.Length==6&&order?.Length==4&&blocks?.Length==4&&riverBanks?.Length==3&&inventory!=null&&found!=null&&cargo>=-1&&cargo<3&&boatBank>=0&&boatBank<=1&&digits.All(x=>x>=0&&x<10)&&compass.All(x=>x>=0&&x<(version>=4?8:4))&&letters.All(x=>x>=0&&x<6)&&wires.All(x=>x>=0&&x<4)&&moons.All(x=>x==0||x==1)&&riverBanks.All(x=>x==0||x==1);
}
