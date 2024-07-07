// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

 Shader "Custom/isoshader" {
     Properties {
             decal ("Base (RGB)", 2D) = "black" {}
             decBump ("Bumpmap (RGB)", 2D) = "bump" {}
         }
         SubShader {
             Pass {
             Fog { Mode Off }
             Tags { "RenderType"="Opaque" }
             LOD 200
             
             CGPROGRAM
             
             #pragma vertex vert
             #pragma fragment frag
             #define PI 3.141592653589793238462643383279
             
             sampler2D decal;
             sampler2D decBump;
             
              struct appdata {
                 float4 vertex : POSITION;
                 float4 color : COLOR;
                 float4 texcoord : TEXCOORD0;
             };
      
             struct v2f {
                 float4 pos : SV_POSITION;
                 float4 tex : TEXCOORD0;
                 float4 col : COLOR0;
                 float3 pass_xy_position : TEXCOORD1;
             };
             
             v2f vert(appdata v){
                 v2f  o;
                 o.pos = UnityObjectToClipPos(v.vertex);
                 o.pass_xy_position = v.vertex.xyz;
                 o.tex = v.texcoord;
                 o.col = v.color;
                 return o;
             }
       
             float4 frag(v2f i) : COLOR {
                 float3 tc = i.tex;
                 tc.x = (PI + atan2(i.pass_xy_position.x, i.pass_xy_position.y)) / (2 * PI);
                 float4 color = tex2D(decal, tc);
                 return color;
             }
      
             ENDCG
         }
     }
 }