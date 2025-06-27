Shader "AkilliMum/Standard/Invisible/Skyboxes" {
Properties{
    _Tint("Tint Color", Color) = (.5, .5, .5, .5)
    [Gamma] _Exposure("Exposure", Range(0, 8)) = 1.0
    _Rotation("Rotation", Range(0, 360)) = 0
    [NoScaleOffset] _Tex("Cubemap   (HDR)", Cube) = "grey" {}
}

SubShader{
    Tags{ "Queue" = "Background" }
    
    Pass{
        //ZWrite Off
        Cull Front

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0

        #include "UnityCG.cginc"

        samplerCUBE _Tex;
        half4 _Tex_HDR;
        half4 _Tint;
        half _Exposure;
        float _Rotation;

        float3 RotateAroundYInDegrees(float3 vertex, float degrees)
        {
            float alpha = degrees * UNITY_PI / 180.0;
            float sina, cosa;
            sincos(alpha, sina, cosa);
            float2x2 m = float2x2(cosa, -sina, sina, cosa);
            return float3(mul(m, vertex.xz), vertex.y).xzy;
        }

        struct appdata_t {
            float4 vertex : POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f {
            float4 vertex : SV_POSITION;
            float3 texcoord : TEXCOORD0;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        v2f vert(appdata_t v)
        {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            float3 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);
            o.vertex = UnityObjectToClipPos(rotated);
            o.texcoord = v.vertex.xyz;
            return o;
        }

        fixed4 frag(v2f i) : SV_Target
        {
            half4 tex = texCUBE(_Tex, i.texcoord);
            half3 c = DecodeHDR(tex, _Tex_HDR);
            c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
            c *= _Exposure;
            return half4(c, 1);
        }
        ENDCG
    }
}


Fallback Off

}

//Shader "AkilliMum/Standard/Invisible/Skyboxes" {
//    Properties{
//        _Tint("Tint Color", Color) = (.5, .5, .5, .5)
//        [Gamma] _Exposure("Exposure", Range(0, 8)) = 1.0
//        _Rotation("Rotation", Range(0, 360)) = 0
//        _Cube("Environment Map   (HDR)", Cube) = "grey" {}
//    }
//
//    SubShader{
//        Tags{ "Queue" = "Background" }
//
//        Pass{
//            //ZWrite Off
//            Cull Front
//
//            CGPROGRAM
//
//            #pragma vertex vert  
//            #pragma fragment frag 
//
//            #include "UnityCG.cginc"
//
//            uniform samplerCUBE _Cube;
//            uniform half4 _Cube_HDR;
//            uniform half4 _Tint;
//            uniform half _Exposure;
//            uniform float _Rotation;
//
//            struct vertexInput {
//                float4 vertex : POSITION;
//            };
//
//            struct vertexOutput {
//                float4 pos : SV_POSITION;
//                float3 viewDir : TEXCOORD1;
//            };
//
//            float4 RotateAroundYInDegrees(float4 vertex, float degrees)
//            {
//                float alpha = degrees * UNITY_PI / 180.0;
//                float sina, cosa;
//                sincos(alpha, sina, cosa);
//                float2x2 m = float2x2(cosa, -sina, sina, cosa);
//                return float4(mul(m, vertex.xz), vertex.yw).xzyw;
//            }
//
//            vertexOutput vert(vertexInput input)
//            {
//                vertexOutput output;
//
//                UNITY_SETUP_INSTANCE_ID(input);
//                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
//
//                float4x4 modelMatrix = unity_ObjectToWorld;
//                output.viewDir = mul(modelMatrix, input.vertex).xyz
//                    - _WorldSpaceCameraPos;
//                output.pos = UnityObjectToClipPos(RotateAroundYInDegrees(input.vertex, _Rotation));
//                return output;
//            }
//
//            float4 frag(vertexOutput input) : COLOR
//            {
//                half4 tex = texCUBE(_Cube, input.viewDir);
//                half3 c = DecodeHDR(tex, _Cube_HDR);
//                c = c * _Tint.rgb * unity_ColorSpaceDouble;
//                c *= _Exposure;
//                return half4(c, 1);
//            }
//
//            ENDCG
//        }
//    }
//}