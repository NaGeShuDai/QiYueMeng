using UnityEngine;

public sealed class RiverPadlock:MonoBehaviour {
 public PoetryPuzzleGame game;
 public Transform shackle;
 public Vector3 closedPosition;
 bool initialized;
 public bool IsOpen=>game&&game.State!=null&&game.State.installed[1];
 void LateUpdate(){
  var position=closedPosition+(IsOpen?new Vector3(.10f,0,0):Vector3.zero);
  var loop=IsOpen?new Vector3(.022f,0,.035f):Vector3.zero;
  float t=initialized?1-Mathf.Exp(-12*Time.unscaledDeltaTime):1;
  transform.localPosition=Vector3.Lerp(transform.localPosition,position,t);
  shackle.localPosition=Vector3.Lerp(shackle.localPosition,loop,t);
  initialized=true;
 }
}
