using UnityEngine;
public sealed class RoomOpenable:MonoBehaviour
{
 public RoomFocusTarget owner;
 public Vector3 travel;
 public float angle;
 public int puzzleLockStage=-1;
 public bool IsOpen {get;private set;}
 Vector3 closedPosition;Quaternion closedRotation;
 void Awake(){closedPosition=transform.localPosition;closedRotation=transform.localRotation;}
 public void Toggle(){var game=FindFirstObjectByType<PoetryPuzzleGame>();if(puzzleLockStage>=0&&game&&!game.State.solved[puzzleLockStage]){if(game.Feedback){game.Feedback.Think("好像缺了什么");game.Feedback.Sound("error");}return;}IsOpen=!IsOpen;if(game&&game.Feedback)game.Feedback.Sound("interact");}
 public void SetOpen(bool open){IsOpen=open;}
 void Update(){
  float t=1-Mathf.Exp(-12*Time.deltaTime);
  transform.localPosition=Vector3.Lerp(transform.localPosition,closedPosition+(IsOpen?travel:Vector3.zero),t);
  transform.localRotation=Quaternion.Slerp(transform.localRotation,closedRotation*Quaternion.Euler(0,IsOpen?angle:0,0),t);
 }
}
