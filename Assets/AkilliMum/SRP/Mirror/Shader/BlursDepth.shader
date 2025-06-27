Shader "AkilliMum/SRP/BlursDepth"
{
    Properties
    {
        _Iteration("Iteration", float) = 1
        _SurfacePower("Surface Power", float) = 1
        _MainTex("Base (RGBA)", 2D) = "" {}
        _DepthTex("Depth", 2D) = "white" { }
        _Lod("Lod",float) = 0
        _DepthCutoff("Depth Cutoff",float) = 0
        _HorizontalBlurMultiplier("HorizontalBlurMultiplier",float) = 1
        _VerticalBlurMultiplier("VerticalBlurMultiplier",float) = 1
        _NearClip("Near Clip", Float) = 0.3
        _FarClip("Far Clip", Float) = 1000
    }

    SubShader
    {
        Pass
        {
            Tags{
                "RenderType" = "Transparent"
                "IgnoreProjector" = "True"
                "Queue" = "Transparent"
            }
            //Tags{
            //   "RenderType" = "Opaque"
            //   "IgnoreProjector" = "True"
            //   "Queue" = "Opaque"
            //}
            //LOD 100
            ZWrite Off
            /*ZTest Always ZWrite Off
            Fog { Mode off }*/
            //AlphaToMask On
            Blend One Zero //OneMinusSrcAlpha//SrcAlpha Zero //OneMinusSrcAlpha, One One //SrcColor One//One OneMinusSrcAlpha
            
            //ZWrite Off
            //ZTest Always
            //Cull Off
            //Fog{ Mode off }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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
                float4 screenPos : TEXCOORD1;
                float4 eyePos : TEXCOORD2;
                float texdistance : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _DepthTex;
            float4 _DepthTex_ST;
            float _Lod;
            float _SurfacePower;
            float _Iteration;
            float _DepthCutoff;
            float _HorizontalBlurMultiplier;
            float _VerticalBlurMultiplier;
            float _NearClip;
            float _FarClip;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.texdistance = distance(_WorldSpaceCameraPos, mul(unity_ObjectToWorld, o.uv));
                o.eyePos = mul(UNITY_MATRIX_MV, v.vertex);
                return o;
            }

            #define FLT_MAX 3.402823466e+38
            #define FLT_MIN 1.175494351e-38
            #define DBL_MAX 1.7976931348623158e+308
            #define DBL_MIN 2.2250738585072014e-308

            half GetDepth(float4 screenPos, float texdistance, float4 eyePos)
            {
                //float sceneDepthAtFrag = tex2Dproj(_DepthTex, UNITY_PROJ_COORD(i.screenPos)).r;
                float sceneDepthAtFrag = tex2Dproj(_DepthTex, UNITY_PROJ_COORD(screenPos)).r;

#if UNITY_REVERSED_Z
                sceneDepthAtFrag = 1 - LinearEyeDepth(sceneDepthAtFrag);
#else
                sceneDepthAtFrag = LinearEyeDepth(sceneDepthAtFrag);
#endif

                float x, y, z, w;
                //float _NearClip = 0.3; //pass camera clipping planes to shader
                //float FarClip = 1000;
#if UNITY_REVERSED_Z //SHADER_API_GLES3 // insted of UNITY_REVERSED_Z
                x = -1.0 + _NearClip / _FarClip;
                y = 1;
                z = x / _NearClip;
                w = 1 / _NearClip;
                float fragDepth = eyePos.z;// *-1;
#else
                x = 1.0 - _NearClip / _FarClip;
                y = _NearClip / _FarClip;
                z = x / _NearClip;
                w = y / _NearClip;
                float fragDepth = eyePos.z * -1;
#endif
                sceneDepthAtFrag = 1.0 / (z * sceneDepthAtFrag + w);

                //float fragDepth = abs(eyePos.z);// * 1;
                
                //return clamp(pow(sceneDepthAtFrag, (_DepthCutoff * texdistance) / 100), 0., 1.);
                //return clamp(pow(sceneDepthAtFrag, _DepthCutoff * fragDepth * texdistance), 0., 1.);
                //return clamp(pow(sceneDepthAtFrag, _DepthCutoff * fragDepth * texdistance), 0., 1.);
                //return clamp(pow(sceneDepthAtFrag, _DepthCutoff * fragDepth), 0., 1.);
                //return clamp(pow(sceneDepthAtFrag, _DepthCutoff), 0., 1.);
                return clamp(pow(sceneDepthAtFrag, _DepthCutoff * fragDepth * texdistance), 0., 1.);
            }

            half GetAlphaFromDepth(float4 screenPos, float texdistance, fixed2 uv, float4 eyePos)
            {
                return
                    lerp
                        (
                            GetDepth(screenPos, texdistance, eyePos),
                            tex2Dlod(_MainTex, float4(uv, 0, _Lod)).a,
                            clamp(_Iteration - 1,0,1) //GetDepth will return (calculated) only for first iteration 
                        );
                //if (_Iteration == 1) //only calculate the depth for first step
                //{
                //    return GetDepth(screenPos, texdistance, eyePos);
                //}
                //else
                //{
                //    return tex2Dlod(_MainTex, float4(uv, 0, _Lod)).a;
                //}
            }

            fixed4 frag(v2f i) : COLOR
            {
                //return (1,1,0,1); 
                half depth = GetDepth(i.screenPos, i.texdistance, i.eyePos);
                //return depth; //test
                //half depth = GetAlphaFromDepth(i.screenPos, i.texdistance, i.uv, i.eyePos);

                float stepX = (1.0 / (_ScreenParams.xy / _ScreenParams.w + FLT_MIN).x);
                float stepY = (1.0 / (_ScreenParams.xy / _ScreenParams.w + FLT_MIN).y);
                //blur
                stepX = stepX * _HorizontalBlurMultiplier * pow((1 - depth), _SurfacePower); //so pixels near to surface will blur less
                stepY = stepY * _VerticalBlurMultiplier   * pow((1 - depth), _SurfacePower);
                 
                half4 color = float4 (0,0,0,0);
                half alpha = 0; //tex2Dlod(_MainTex, float4(i.uv, 0, _Lod)).a*(1-depth); //0

                fixed2 copyUV;  

                copyUV.x = i.uv.x - stepX;
                copyUV.y = i.uv.y - stepY / 5; //mix top less on Y
                color += tex2Dlod(_MainTex, float4(copyUV, 0, _Lod)) * 0.077847;
                alpha += GetAlphaFromDepth(i.screenPos, i.texdistance, copyUV, i.eyePos) * 0.077847;
                copyUV.x = i.uv.x;
                copyUV.y = i.uv.y - stepY / 5; //mix top less on Y
                color += tex2Dlod(_MainTex, float4(copyUV, 0, _Lod)) * 0.123317;
                alpha += GetAlphaFromDepth(i.screenPos, i.texdistance, copyUV, i.eyePos) * 0.123317;
                copyUV.x = i.uv.x + stepX;
                copyUV.y = i.uv.y - stepY;
                color += tex2Dlod(_MainTex, float4(copyUV, 0, _Lod)) * 0.077847;
                alpha += GetAlphaFromDepth(i.screenPos, i.texdistance, copyUV, i.eyePos) * 0.077847;

                copyUV.x = i.uv.x - stepX;
                copyUV.y = i.uv.y;
                color += tex2Dlod(_MainTex, float4(copyUV, 0, _Lod)) * 0.123317;
                alpha += GetAlphaFromDepth(i.screenPos, i.texdistance, copyUV, i.eyePos) * 0.123317;
                copyUV.x = i.uv.x;
                copyUV.y = i.uv.y;
                color += tex2Dlod(_MainTex, float4(copyUV, 0, _Lod)) * 0.195346;
                alpha += GetAlphaFromDepth(i.screenPos, i.texdistance, copyUV, i.eyePos) * 0.195346;
                copyUV.x = i.uv.x + stepX;
                copyUV.y = i.uv.y;
                color += tex2Dlod(_MainTex, float4(copyUV, 0, _Lod)) * 0.123317;
                alpha += GetAlphaFromDepth(i.screenPos, i.texdistance, copyUV, i.eyePos) * 0.123317;

                copyUV.x = i.uv.x - stepX;
                copyUV.y = i.uv.y + stepY;
                color += tex2Dlod(_MainTex, float4(copyUV, 0, _Lod)) * 0.077847;
                alpha += GetAlphaFromDepth(i.screenPos, i.texdistance, copyUV, i.eyePos) * 0.077847;
                copyUV.x = i.uv.x;
                copyUV.y = i.uv.y + stepY;
                color += tex2Dlod(_MainTex, float4(copyUV, 0, _Lod)) * 0.123317;
                alpha += GetAlphaFromDepth(i.screenPos, i.texdistance, copyUV, i.eyePos) * 0.123317;
                copyUV.x = i.uv.x + stepX;
                copyUV.y = i.uv.y + stepY;
                color += tex2Dlod(_MainTex, float4(copyUV, 0, _Lod)) * 0.077847;
                alpha += GetAlphaFromDepth(i.screenPos, i.texdistance, copyUV, i.eyePos) * 0.077847;
              
                //color.a carries the previous depth value, we have to blend it too! we blend it above like color!
                
                color.a = clamp(alpha,0,1);

                return color;
            }

            ENDCG
        }
    }
}
