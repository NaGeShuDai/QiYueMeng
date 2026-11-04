Shader "PoetryRoom/ForestWindow"
{
 Properties { _ForestTex("Forest",2D)="black"{} }
 SubShader {
  Tags { "RenderType"="Opaque" "Queue"="Geometry+10" "RenderPipeline"="UniversalPipeline" }
  Pass {
   Name "Window" Tags {"LightMode"="UniversalForward"} Cull Off ZWrite On
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct Attributes {float4 positionOS:POSITION;};
   struct Varyings {float4 positionCS:SV_POSITION;float4 screen:TEXCOORD0;};
   TEXTURE2D(_ForestTex);SAMPLER(sampler_ForestTex);
   Varyings vert(Attributes a) {Varyings o;o.positionCS=TransformObjectToHClip(a.positionOS.xyz);o.screen=ComputeScreenPos(o.positionCS);return o;}
   half4 frag(Varyings i):SV_Target {return half4(SAMPLE_TEXTURE2D(_ForestTex,sampler_ForestTex,i.screen.xy/i.screen.w).rgb,1);}
   ENDHLSL
  }
 }
}
