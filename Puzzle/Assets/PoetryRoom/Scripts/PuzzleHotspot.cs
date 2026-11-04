using UnityEngine;
public sealed class PuzzleHotspot:MonoBehaviour
{
 public RoomFocusTarget owner;
 public int stage=-1,index;
 public string action,item;
 public RoomOpenable container;
 public bool Available=>!container||container.IsOpen;
}
