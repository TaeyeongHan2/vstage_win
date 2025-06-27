Shader "AkilliMum/SRP/SimplesLod"
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "" {}
        _Lod("Lod",float) = 0
    }
    SubShader
    {
        //Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        //LOD 100
        //https://forum.unity.com/threads/graphics-blit-doesnt-work-on-ios-when-a-material-is-used.237000/
        ZWrite Off ZTest Always

        Pass
        {
            //HLSLPROGRAM
            CGPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            //#pragma multi_compile_fog
            
            //#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Lod;
            float _Iteration;
            float _NeighbourPixels;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            //may become handy later
            //#define FLT_MAX 3.402823466e+38
            //#define FLT_MIN 1.175494351e-38
            //#define DBL_MAX 1.7976931348623158e+308
            //#define DBL_MIN 2.2250738585072014e-308

            float4 frag(v2f i) : COLOR
            {
                return tex2Dlod (_MainTex, float4(i.uv,0,_Lod)); //just return lod
            }
            //ENDHLSL
            ENDCG
        }
    }
}