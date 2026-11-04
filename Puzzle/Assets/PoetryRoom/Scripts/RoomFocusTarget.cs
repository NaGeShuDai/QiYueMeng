using UnityEngine;

public sealed class RoomFocusTarget : MonoBehaviour
{
    public string label;
    public int priority;
    public RoomFocusTarget focusProxy;
    public RoomFocusTarget Resolved => focusProxy ? focusProxy : this;
    public Vector3 lookAt;
    public Vector3 cameraPosition;
    public float fieldOfView=42;
    public bool hasDetailView;
    public Vector3 detailPosition;
    public Vector3 detailLookAt;
    public float detailFieldOfView=42;
    public Transform viewAnchor;
    public Vector3 localCameraPosition,localLookAt,localDetailPosition,localDetailLookAt;
    public Vector3 CameraPosition=>viewAnchor?viewAnchor.TransformPoint(localCameraPosition):cameraPosition;
    public Vector3 LookAt=>viewAnchor?viewAnchor.TransformPoint(localLookAt):lookAt;
    public Vector3 DetailPosition=>viewAnchor?viewAnchor.TransformPoint(localDetailPosition):detailPosition;
    public Vector3 DetailLookAt=>viewAnchor?viewAnchor.TransformPoint(localDetailLookAt):detailLookAt;
    public void BindViewPose(Transform anchor){
        viewAnchor=anchor;localCameraPosition=anchor.InverseTransformPoint(cameraPosition);localLookAt=anchor.InverseTransformPoint(lookAt);
        localDetailPosition=anchor.InverseTransformPoint(detailPosition);localDetailLookAt=anchor.InverseTransformPoint(detailLookAt);
    }
    public Renderer[] surfaces;
    public Renderer[] hideWhenFocused=new Renderer[0];
    public bool Available
    {
        get { foreach(var r in surfaces) if(r && r.enabled && r.gameObject.activeInHierarchy) return true; return false; }
    }
}
