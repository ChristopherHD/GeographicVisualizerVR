// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/SphereTest3" {
     Properties {
         _MainTex ("Texture", 2D) = "white" {}
     }
     SubShader
     {
         Tags { "RenderType"="Opaque" }
         LOD 100
 
         Pass
         {
         
             CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
             //make fog work
             #pragma multi_compile_fog
             
             #include "UnityCG.cginc"
 
             struct appdata
             {
                 float4 vertex : POSITION;
                 float2 uv : TEXCOORD0;
                 float3 normal : NORMAL;
             };
 
             struct v2f
             {
                 float2 uv : TEXCOORD0;
                 UNITY_FOG_COORDS(1)
                 float4 vertex : SV_POSITION;
                  float3    normal : TEXCOORD1;
             };
 
             sampler2D _MainTex;
             float4 _MainTex_ST;
             
             v2f vert (appdata v)
             {
                 v2f o;
                 o.vertex = UnityObjectToClipPos(v.vertex);
                 o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                 o.normal = v.normal;
                 UNITY_TRANSFER_FOG(o,o.vertex);
             
                 return o;
             }
             
              #define PI 3.141592653589793
  
             inline float2 RadialCoords(float3 a_coords)
             {
                 float3 a_coords_n = normalize(a_coords);
                 float lon = atan2(a_coords_n.y, a_coords_n.x);
                 float lat = acos(a_coords_n.z);
                 float2 sphereCoords = float2(lon, lat) * (1.0 / PI);
                 return float2(sphereCoords.x * 0.5 + 0.5, 1 - sphereCoords.y);
                 
             }
 
             float4 frag(v2f IN, out float depth:DEPTH) : COLOR
             {
                 float2 equiUV = RadialCoords(IN.normal);
                  depth = 0;
                  equiUV.x = 1- equiUV.x;
                 return tex2Dlod (_MainTex, float4(equiUV.x,equiUV.y,0,0));
                
             }
 
             ENDCG
         }
     }
 }