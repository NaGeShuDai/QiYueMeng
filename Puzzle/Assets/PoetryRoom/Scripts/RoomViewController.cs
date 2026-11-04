using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class RoomViewController : MonoBehaviour
{
    [Serializable] public class Wall
    {
        public string name;
        public Vector3 outward;
        public Renderer[] renderers;
        public Collider[] colliders;
        public bool visible;
        public void Show(bool value) {
            visible=value;
            foreach(var r in renderers) if(r) r.enabled=value;
            foreach(var c in colliders) if(c) c.enabled=value;
        }
    }
    public Camera viewCamera;
    public Vector3 roomCenter;
    public Wall[] walls;
    public Renderer[] ceilings;
    public RoomFocusTarget[] targets;
    public float yaw=225, pitch=34, distance=10;
    public float orbitSensitivity=.22f;
    public RoomFocusTarget Focused {get;private set;}
    public RoomFocusTarget Hovered {get;private set;}
    public bool IsDetail {get;private set;}
    Vector2 down;
    bool dragging;
    bool uiGesture;
    PoetryPuzzleGame puzzles;
    float savedYaw,savedPitch,savedDistance;
    Vector3 desiredPosition,desiredLook;
    float desiredFov;
    float focusZoom=1;
    System.Collections.Generic.Dictionary<Renderer,bool> hidden=new System.Collections.Generic.Dictionary<Renderer,bool>();
    public int VisibleWalls {get {int n=0;foreach(var w in walls)if(w.visible)n++;return n;}}
    void Start() {
        Application.runInBackground=true;
        puzzles=FindFirstObjectByType<PoetryPuzzleGame>();
        foreach(var r in ceilings)if(r)r.enabled=false;
        UpdatePose(true);
    }
    void Update() {
        var mouse=Mouse.current;var keyboard=Keyboard.current;
        if(keyboard!=null && keyboard.escapeKey.wasPressedThisFrame) {
            if(IsDetail){IsDetail=false;focusZoom=1;}else ReturnToOverview();
        }
        if(mouse!=null) {
            var pos=mouse.position.ReadValue();
            bool overUI=puzzles&&puzzles.BlocksPointer(pos);
            if(mouse.leftButton.wasPressedThisFrame){down=pos;dragging=false;uiGesture=overUI;}
            if(mouse.leftButton.isPressed && Vector2.Distance(pos,down)>7) dragging=true;
            if(!Focused && !overUI && !uiGesture) {
                if(mouse.rightButton.isPressed || (mouse.leftButton.isPressed && dragging)) {
                    Vector2 d=mouse.delta.ReadValue();yaw+=d.x*orbitSensitivity;pitch-=d.y*orbitSensitivity;
                }

            }
            if(!overUI&&!uiGesture)ApplyScroll(mouse.scroll.ReadValue().y);
            Hovered=overUI?null:Pick(pos);
            if(mouse.leftButton.wasReleasedThisFrame && !overUI && !uiGesture && !dragging && Hovered && !Focused) Focus(Hovered);
            else if(mouse.leftButton.wasReleasedThisFrame && !overUI && !uiGesture && !dragging && Focused && !TryDetail(pos) && !(puzzles&&puzzles.WorldClick(pos))) {
                var hits=Physics.RaycastAll(viewCamera.ScreenPointToRay(pos),100,1<<28,QueryTriggerInteraction.Collide);
                Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
                foreach(var h in hits){var part=h.collider.GetComponentInParent<RoomOpenable>();if(part && part.owner && part.owner.Resolved==Focused){part.Toggle();break;}}
            }
            if(mouse.leftButton.wasReleasedThisFrame){if(puzzles&&uiGesture&&overUI&&!dragging)puzzles.ClickInventory(pos);uiGesture=false;}
        }
        if(!Focused && keyboard!=null) {
            float turn=(keyboard.dKey.isPressed||keyboard.rightArrowKey.isPressed?1:0)-(keyboard.aKey.isPressed||keyboard.leftArrowKey.isPressed?1:0);
            yaw+=turn*65*Time.unscaledDeltaTime;
        }
        UpdatePose(false);
    }
    public RoomFocusTarget Pick(Vector2 screen) {
        Ray ray=viewCamera.ScreenPointToRay(screen);
        RoomFocusTarget best=null;float distance=float.MaxValue;
        foreach(var h in Physics.RaycastAll(ray,100,1<<29,QueryTriggerInteraction.Collide)) {
            var t=h.collider.GetComponent<RoomFocusTarget>();if(t)t=t.Resolved;
            if(t&&t.Available&&(!best||t.priority>best.priority||(t.priority==best.priority&&h.distance<distance))){best=t;distance=h.distance;}
        }
        return best;
    }
    public bool Focus(RoomFocusTarget target) {
        if(target)target=target.Resolved;
        if(!target || !target.Available || Focused) return false;
        savedYaw=yaw;savedPitch=pitch;savedDistance=distance;Focused=target;IsDetail=false;focusZoom=1;
        return true;
    }
    bool TryDetail(Vector2 position){
        if(IsDetail)return false;
        var ray=viewCamera.ScreenPointToRay(position);
        var parts=Physics.RaycastAll(ray,100,(1<<27)|(1<<28),QueryTriggerInteraction.Collide);
        Array.Sort(parts,(a,b)=>a.distance.CompareTo(b.distance));
        foreach(var part in parts){
            var hotspot=part.collider.GetComponentInParent<PuzzleHotspot>();
            if(hotspot&&hotspot.owner==Focused&&hotspot.Available){if(hotspot.action=="reward"||hotspot.action=="pickup")return false;break;}
            var openable=part.collider.GetComponentInParent<RoomOpenable>();
            if(openable&&openable.owner&&openable.owner.Resolved==Focused)return false;
        }
        if(Focused&&Focused.hasDetailView){
            var bounds=Focused.GetComponent<Collider>();
            if(bounds&&bounds.Raycast(viewCamera.ScreenPointToRay(position),out var hit,100)){IsDetail=true;focusZoom=1;return true;}
        }
        var target=Pick(position);
        if(!target||!target.hasDetailView)return false;
        if(target==Focused&&IsDetail)return false;
        // The first click on a tabletop object only frames it; it must not also turn a dial.
        Focused=target;IsDetail=true;focusZoom=1;return true;
    }
    public void ReturnToOverview() {
        if(!Focused)return;
        RestoreHidden();Focused=null;IsDetail=false;yaw=savedYaw;pitch=savedPitch;distance=savedDistance;
    }
    void RestoreHidden(){foreach(var pair in hidden)if(pair.Key)pair.Key.enabled=pair.Value;hidden.Clear();}
    void OnDisable(){RestoreHidden();}
    public void SetOrbit(float angle,bool instant=true) {RestoreHidden();Focused=null;IsDetail=false;yaw=angle;UpdatePose(instant);}
    public void ApplyScroll(float delta) {
        if(Mathf.Approximately(delta,0))return;
        // Input System supplies wheel ticks on some platforms and 120-unit steps on others.
        float steps=Mathf.Clamp(Mathf.Abs(delta)>=120?delta/120f:delta,-8,8);
        float factor=Mathf.Exp(-steps*.12f);
        if(Focused)focusZoom=Mathf.Clamp(focusZoom*factor,.45f,1.5f);
        else distance=Mathf.Clamp(distance*factor,3f,18f);
        UpdatePose(true);
    }
    public void Snap() {UpdatePose(true);}
    void UpdatePose(bool instant) {
        if(Focused) {desiredPosition=IsDetail?Focused.DetailPosition:Focused.CameraPosition;desiredLook=IsDetail?Focused.DetailLookAt:Focused.LookAt;desiredFov=IsDetail?Focused.detailFieldOfView:Focused.fieldOfView;}
        else {
            desiredLook=roomCenter+Vector3.up*.85f;
            desiredPosition=desiredLook+Quaternion.Euler(pitch,yaw,0)*new Vector3(0,0,-distance);
            desiredFov=40;
        }
        if(Focused)desiredFov=Mathf.Clamp(desiredFov*focusZoom,18,70);
        float t=instant || !Focused?1:1-Mathf.Exp(-9*Time.unscaledDeltaTime);
        viewCamera.transform.position=Vector3.Lerp(viewCamera.transform.position,desiredPosition,t);
        var q=Focused?Quaternion.LookRotation(desiredLook-viewCamera.transform.position,Vector3.up):Quaternion.Euler(pitch,yaw,0);
        viewCamera.transform.rotation=Quaternion.Slerp(viewCamera.transform.rotation,q,t);
        viewCamera.fieldOfView=Mathf.Lerp(viewCamera.fieldOfView,desiredFov,t);
        UpdateWalls();
    }
    public void UpdateWalls() {
        // Looking toward a wall keeps it, including when a focus camera is inside the room.
        Vector3 direction=viewCamera.transform.forward;direction.y=0;direction.Normalize();
        foreach(var w in walls)w.Show(Vector3.Dot(direction,w.outward)>.03f);
        foreach(var r in ceilings)if(r)r.enabled=false;
        foreach(var t in targets)if(t) {
            var c=t.GetComponent<Collider>();if(c)c.enabled=t.Available;
        }
    }
}
