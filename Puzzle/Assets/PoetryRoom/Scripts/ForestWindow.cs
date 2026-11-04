using UnityEngine;
using UnityEngine.Rendering.Universal;

[DefaultExecutionOrder(1000)]
public sealed class ForestWindow : MonoBehaviour
{
    public Camera mainCamera;
    public Camera forestCamera;
    public Renderer windowSurface;
    public Vector3 forestOffset;
    RenderTexture texture;
    Material material;
    void OnEnable() {
        material=new Material(windowSurface.sharedMaterial);
        windowSurface.material=material;
        forestCamera.cullingMask=1<<30;
        mainCamera.cullingMask&=~(1<<30);
        forestCamera.clearFlags=CameraClearFlags.SolidColor;
        forestCamera.backgroundColor=new Color(.34f,.40f,.34f);
        forestCamera.depth=-20;
        forestCamera.GetUniversalAdditionalCameraData().renderPostProcessing=false;
    }
    void LateUpdate() {
        int width=Mathf.Clamp(mainCamera.pixelWidth,1024,1920),height=Mathf.Clamp(mainCamera.pixelHeight,768,1080);
        if(texture==null || texture.width!=width || texture.height!=height) {
            if(texture){texture.Release();Destroy(texture);}
            texture=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32){name="Forest Window View",antiAliasing=1};texture.Create();
            forestCamera.targetTexture=texture;material.SetTexture("_ForestTex",texture);
        }
        forestCamera.transform.SetPositionAndRotation(mainCamera.transform.position+forestOffset,mainCamera.transform.rotation);
        forestCamera.nearClipPlane=.05f;forestCamera.farClipPlane=100;
        forestCamera.projectionMatrix=mainCamera.projectionMatrix;
        forestCamera.enabled=windowSurface.enabled;
    }
    void OnDisable() {
        if(forestCamera){forestCamera.enabled=false;forestCamera.targetTexture=null;}
        if(texture){texture.Release();Destroy(texture);}
        if(material)Destroy(material);
    }
}
