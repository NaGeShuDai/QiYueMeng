Shader "PoetryRoom/EndingVortex"
{
 SubShader
 {
  Tags { "RenderType"="Opaque" "Queue"="Geometry+10" "RenderPipeline"="UniversalPipeline" }
  Pass
  {
   Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
   struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
   Varyings vert(Attributes v) { Varyings o; o.positionCS=TransformObjectToHClip(v.positionOS.xyz); o.uv=v.uv; return o; }
   half4 frag(Varyings i):SV_Target
   {
    float2 p=(i.uv-.5)*2;
    float r=length(p), a=atan2(p.y,p.x), t=_Time.y;
    float swirl=pow(.5+.5*sin(a*5+r*22-t*3),3);
    float ring=exp(-pow((r-.72)*7,2));
    float core=1-smoothstep(.05,.38,r);
    float edge=1-smoothstep(.82,1.15,r);
    float3 color=float3(.018,.025,.065)+edge*(float3(.16,.20,.56)*swirl+float3(.55,.30,.20)*ring*.8);
    color+=float3(.95,.79,.57)*core*.9;
    return half4(color,1);
   }
   ENDHLSL
  }
 }
}
