Shader "AkilliMum/SRP/BlursLod"
{
    Properties
    {
        _Iteration("Iteration", float) = 1
        _NeighbourPixels("Neighbour Pixels", float) = 1
        _BaseMap("Base (RGB)", 2D) = "" {}
        _Lod("Lod",float) = 0
        _AR("AR Mode",float) = 0
    }

    SubShader
    {
        // With SRP we introduce a new "RenderPipeline" tag in Subshader. This allows to create shaders
        // that can match multiple render pipelines. If a RenderPipeline tag is not set it will match
        // any render pipeline. In case you want your subshader to only run in LWRP set the tag to
        // "UniversalRenderPipeline"
        Tags{"RenderType" = "Transparent" "RenderPipeline" = "UniversalRenderPipeline" "IgnoreProjector" = "True"}
        LOD 300

        // ------------------------------------------------------------------
        // Forward pass. Shades GI, emission, fog and all lights in a single pass.
        // Compared to Builtin pipeline forward renderer, LWRP forward renderer will
        // render a scene with multiple lights with less drawcalls and less overdraw.
        Pass
        {
            // "Lightmode" tag must be "UniversalForward" or not be defined in order for
            // to render objects.
            Name "StandardLit"
            //Tags{"LightMode" = "UniversalForward"}

            //Blend[_SrcBlend][_DstBlend]
            ZWrite Off ZTest Always
            //ZWrite[_ZWrite]
            //Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard SRP library
            // All shaders must be compiled with HLSLcc and currently only gles is not using HLSLcc by default
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            
            // Including the following two function is enought for shading with Universal Pipeline. Everything is included in them.
            // Core.hlsl will include SRP shader library, all constant buffers not related to materials (perobject, percamera, perframe).
            // It also includes matrix/space conversion functions and fog.
            // Lighting.hlsl will include the light functions/data to abstract light constants. You should use GetMainLight and GetLight functions
            // that initialize Light struct. Lighting.hlsl also include GI, Light BDRF functions. It also includes Shadows.

            // Required by all Universal Render Pipeline shaders.
            // It will include Unity built-in shader variables (except the lighting variables)
            // (https://docs.unity3d.com/Manual/SL-UnityShaderVariables.html
            // It will also include many utilitary functions. 
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

            // Include this if you are doing a lit shader. This includes lighting shader variables,
            // lighting and shadow functions
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Material shader variables are not defined in SRP or LWRP shader library.
            // This means _BaseColor, _BaseMap, _BaseMap_ST, and all variables in the Properties section of a shader
            // must be defined by the shader itself. If you define all those properties in CBUFFER named
            // UnityPerMaterial, SRP can cache the material properties between frames and reduce significantly the cost
            // of each drawcall.
            // In this case, for sinmplicity LitInput.hlsl is included. This contains the CBUFFER for the material
            // properties defined above. As one can see this is not part of the ShaderLibrary, it specific to the
            // LWRP Lit shader.
            //#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"

            float4 _BaseMap_ST;
            float _Lod;
            float _Iteration;
            float _NeighbourPixels;
            float _AR;
            
            TEXTURE2D(_BaseMap);       SAMPLER(sampler_BaseMap);

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv                       : TEXCOORD0;
                float4 positionCS               : SV_POSITION;
            };

            Varyings LitPassVertex(Attributes input)
            {
                Varyings output;

                // VertexPositionInputs contains position in multiple spaces (world, view, homogeneous clip space)
                // Our compiler will strip all unused references (say you don't use view space).
                // Therefore there is more flexibility at no additional cost with this struct.
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

                // TRANSFORM_TEX is the same as the old shader library.
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                // We just use the homogeneous clip position from the vertex input
                output.positionCS = vertexInput.positionCS;
                return output;
            }

            #define FLT_MAX 3.402823466e+38
            #define FLT_MIN 1.175494351e-38
            #define DBL_MAX 1.7976931348623158e+308
            #define DBL_MIN 2.2250738585072014e-308

            half4 LitPassFragment(Varyings input) : SV_Target
            {
                float2 uv = input.uv;

                float stepX = (1.0 / (_ScreenParams.xy / _ScreenParams.w + FLT_MIN).x) * _NeighbourPixels;
                //float stepX = (1.0 / (_ScreenParams.xy / _ScreenParams.w+FLT_MIN).x)*(_NeighbourPixels + (_Iteration % 2));
                float stepY = (1.0 / (_ScreenParams.xy / _ScreenParams.w + FLT_MIN).y) * _NeighbourPixels;
                //float stepY = (1.0 / (_ScreenParams.xy / _ScreenParams.w+FLT_MIN).y)*(_NeighbourPixels + (_Iteration % 2));

                if (_AR > 0)
                {
                    if (_Iteration == 1) //clear the texture
                    {
                        half3 check = float3(0, 0, 0);

                        float onePixelX = (1.0 / (_ScreenParams.xy / _ScreenParams.w + FLT_MIN).x);
                        float onePixelY = (1.0 / (_ScreenParams.xy / _ScreenParams.w + FLT_MIN).y);

                        half4 up = SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, uv + float2(0, onePixelY), _Lod);
                        half4 down = SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, uv + float2(0, -onePixelY), _Lod);
                        half4 left = SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, uv + float2(-onePixelX, 0), _Lod);
                        half4 right = SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, uv + float2(onePixelX, 0), _Lod);
                        
                        if (all(check == up.rgb)
                            &&
                            all(check == right.rgb))
                            return SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, uv + float2(-onePixelX, -onePixelY), _Lod);
                        if(all(check == right.rgb)
                            &&
                            all(check == down.rgb))
                            return SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, uv + float2(-onePixelX, onePixelY), _Lod);
                        if (all(check == down.rgb)
                            &&
                            all(check == left.rgb))
                            return SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, uv + float2(onePixelX, onePixelY), _Lod);
                        if (all(check == left.rgb)
                            &&
                            all(check == up.rgb))
                            return SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, uv + float2(onePixelX, -onePixelY), _Lod);

                        if (all(check == up.rgb))
                            return SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, uv + float2(0, -onePixelY), _Lod);
                        if (all(check == right.rgb))
                            return SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, uv + float2(-onePixelX, 0), _Lod);
                        if (all(check == down.rgb))
                            return SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, uv + float2(0, onePixelY), _Lod);
                        if (all(check == left.rgb))
                            return SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, uv + float2(onePixelX, 0), _Lod);

                        return SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, uv, _Lod);
                    }
                    else //jump black pixel blur
                    {
                        half3 check = float3(0, 0, 0);

                        half4 up = SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, uv + float2(0, stepY), _Lod);
                        half4 down = SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, uv + float2(0, -stepY), _Lod);
                        half4 left = SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, uv + float2(-stepX, 0), _Lod);
                        half4 right = SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, uv + float2(stepX, 0), _Lod);

                        if (all(check == up.rgb)
                            ||
                            all(check == right.rgb)
                            ||
                            all(check == down.rgb)
                            ||
                            all(check == left.rgb))
                            return SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, uv, _Lod);
                    }
                }

                half4 color = float4 (0,0,0,0);

                float2 copyUV;

                copyUV.x = uv.x - stepX;
                copyUV.y = uv.y - stepY;
                color += SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, copyUV,_Lod)*0.077847;
                copyUV.x = uv.x;
                copyUV.y = uv.y - stepY;
                color += SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, copyUV,_Lod)*0.123317;
                copyUV.x = uv.x + stepX;
                copyUV.y = uv.y - stepY;
                color += SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, copyUV,_Lod)*0.077847;

                copyUV.x = uv.x - stepX;
                copyUV.y = uv.y;
                color += SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, copyUV,_Lod)*0.123317;
                copyUV.x = uv.x;
                copyUV.y = uv.y;
                color += SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, copyUV,_Lod)*0.195346;
                copyUV.x = uv.x + stepX;
                copyUV.y = uv.y;
                color += SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, copyUV,_Lod)*0.123317;

                copyUV.x = uv.x - stepX;
                copyUV.y = uv.y + stepY;
                color += SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, copyUV,_Lod)*0.077847;
                copyUV.x = uv.x;
                copyUV.y = uv.y + stepY;
                color += SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, copyUV,_Lod)*0.123317;
                copyUV.x = uv.x + stepX;
                copyUV.y = uv.y + stepY;
                color += SAMPLE_TEXTURE2D_LOD (_BaseMap, sampler_BaseMap, copyUV,_Lod)*0.077847;

                return color;
            }
            ENDHLSL
        }
    }
}


// Shader "AkilliMum/SRP/BlursLod"
// {
//     Properties
//     {
//         _Iteration("Iteration", float) = 1
//         _NeighbourPixels("Neighbour Pixels", float) = 1
//         _MainTex("Base (RGB)", 2D) = "" {}
//         _Lod("Lod",float) = 0
//         _AR("AR Mode",float) = 0
//     }
//     SubShader
//     {
//         //Tags { "RenderType"="Transparent" "Queue"="Transparent"}
//         //LOD 100
//         //https://forum.unity.com/threads/graphics-blit-doesnt-work-on-ios-when-a-material-is-used.237000/
//         ZWrite Off ZTest Always

//         Pass
//         {
//             //HLSLPROGRAM
//             CGPROGRAM
//             #pragma prefer_hlslcc gles
//             #pragma exclude_renderers d3d11_9x
//             #pragma vertex vert
//             #pragma fragment frag
//             // make fog work
//             //#pragma multi_compile_fog
            
//             //#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
//             #include "UnityCG.cginc"

//             struct appdata
//             {
//                 float4 vertex : POSITION;
//                 float2 uv : TEXCOORD0;
//             };

//             struct v2f
//             {
//                 float2 uv : TEXCOORD0;
//                 float4 vertex : SV_POSITION;
//             };

//             sampler2D _MainTex;
//             float4 _MainTex_ST;
//             float _Lod;
//             float _Iteration;
//             float _NeighbourPixels;
//             float _AR;

//             v2f vert(appdata v)
//             {
//                 v2f o;
//                 o.vertex = UnityObjectToClipPos(v.vertex);
//                 //o.vertex = TransformObjectToHClip(v.vertex);
//                 o.uv = TRANSFORM_TEX(v.uv, _MainTex);
//                 //UNITY_TRANSFER_FOG(o,o.vertex);
//                 return o;
//             }

//             #define FLT_MAX 3.402823466e+38
//             #define FLT_MIN 1.175494351e-38
//             #define DBL_MAX 1.7976931348623158e+308
//             #define DBL_MIN 2.2250738585072014e-308

//             float4 frag(v2f i) : COLOR
//             {
//                 float stepX = (1.0 / (_ScreenParams.xy / _ScreenParams.w + FLT_MIN).x) * _NeighbourPixels;
//                 //float stepX = (1.0 / (_ScreenParams.xy / _ScreenParams.w+FLT_MIN).x)*(_NeighbourPixels + (_Iteration % 2));
//                 float stepY = (1.0 / (_ScreenParams.xy / _ScreenParams.w + FLT_MIN).y) * _NeighbourPixels;
//                 //float stepY = (1.0 / (_ScreenParams.xy / _ScreenParams.w+FLT_MIN).y)*(_NeighbourPixels + (_Iteration % 2));

//                 if (_AR > 0)
//                 {
//                     if (_Iteration == 1) //clear the texture
//                     {
//                         half3 check = float3(0, 0, 0);

//                         float onePixelX = (1.0 / (_ScreenParams.xy / _ScreenParams.w + FLT_MIN).x);
//                         float onePixelY = (1.0 / (_ScreenParams.xy / _ScreenParams.w + FLT_MIN).y);

//                         half4 up = tex2Dlod(_MainTex, float4(i.uv + float2(0, onePixelY), 0, _Lod));
//                         half4 down = tex2Dlod(_MainTex, float4(i.uv + float2(0, -onePixelY), 0, _Lod));
//                         half4 left = tex2Dlod(_MainTex, float4(i.uv + float2(-onePixelX, 0), 0, _Lod));
//                         half4 right = tex2Dlod(_MainTex, float4(i.uv + float2(onePixelX, 0), 0, _Lod));
                        
//                         if (all(check == up.rgb)
//                             &&
//                             all(check == right.rgb))
//                             return tex2Dlod(_MainTex, float4(i.uv + float2(-onePixelX, -onePixelY), 0, _Lod));
//                         if(all(check == right.rgb)
//                             &&
//                             all(check == down.rgb))
//                             return tex2Dlod(_MainTex, float4(i.uv + float2(-onePixelX, onePixelY), 0, _Lod));
//                         if (all(check == down.rgb)
//                             &&
//                             all(check == left.rgb))
//                             return tex2Dlod(_MainTex, float4(i.uv + float2(onePixelX, onePixelY), 0, _Lod));
//                         if (all(check == left.rgb)
//                             &&
//                             all(check == up.rgb))
//                             return tex2Dlod(_MainTex, float4(i.uv + float2(onePixelX, -onePixelY), 0, _Lod));

//                         if (all(check == up.rgb))
//                             return tex2Dlod(_MainTex, float4(i.uv + float2(0, -onePixelY), 0, _Lod));
//                         if (all(check == right.rgb))
//                             return tex2Dlod(_MainTex, float4(i.uv + float2(-onePixelX, 0), 0, _Lod));
//                         if (all(check == down.rgb))
//                             return tex2Dlod(_MainTex, float4(i.uv + float2(0, onePixelY), 0, _Lod));
//                         if (all(check == left.rgb))
//                             return tex2Dlod(_MainTex, float4(i.uv + float2(onePixelX, 0), 0, _Lod));

//                         return tex2Dlod(_MainTex, float4(i.uv, 0, _Lod));
//                     }
//                     else //jump black pixel blur
//                     {
//                         half3 check = float3(0, 0, 0);

//                         half4 up = tex2Dlod(_MainTex, float4(i.uv + float2(0, stepY), 0, _Lod));
//                         half4 down = tex2Dlod(_MainTex, float4(i.uv + float2(0, -stepY), 0, _Lod));
//                         half4 left = tex2Dlod(_MainTex, float4(i.uv + float2(-stepX, 0), 0, _Lod));
//                         half4 right = tex2Dlod(_MainTex, float4(i.uv + float2(stepX, 0), 0, _Lod));

//                         if (all(check == up.rgb)
//                             ||
//                             all(check == right.rgb)
//                             ||
//                             all(check == down.rgb)
//                             ||
//                             all(check == left.rgb))
//                             return tex2Dlod(_MainTex, float4(i.uv, 0, _Lod));
//                     }
//                 }

//                 half4 color = float4 (0,0,0,0);

//                 float2 copyUV;

//                 copyUV.x = i.uv.x - stepX;
//                 copyUV.y = i.uv.y - stepY;
//                 color += tex2Dlod (_MainTex, float4(copyUV,0,_Lod))*0.077847;
//                 copyUV.x = i.uv.x;
//                 copyUV.y = i.uv.y - stepY;
//                 color += tex2Dlod (_MainTex, float4(copyUV,0,_Lod))*0.123317;
//                 copyUV.x = i.uv.x + stepX;
//                 copyUV.y = i.uv.y - stepY;
//                 color += tex2Dlod (_MainTex, float4(copyUV,0,_Lod))*0.077847;

//                 copyUV.x = i.uv.x - stepX;
//                 copyUV.y = i.uv.y;
//                 color += tex2Dlod (_MainTex, float4(copyUV,0,_Lod))*0.123317;
//                 copyUV.x = i.uv.x;
//                 copyUV.y = i.uv.y;
//                 color += tex2Dlod (_MainTex, float4(copyUV,0,_Lod))*0.195346;
//                 copyUV.x = i.uv.x + stepX;
//                 copyUV.y = i.uv.y;
//                 color += tex2Dlod (_MainTex, float4(copyUV,0,_Lod))*0.123317;

//                 copyUV.x = i.uv.x - stepX;
//                 copyUV.y = i.uv.y + stepY;
//                 color += tex2Dlod (_MainTex, float4(copyUV,0,_Lod))*0.077847;
//                 copyUV.x = i.uv.x;
//                 copyUV.y = i.uv.y + stepY;
//                 color += tex2Dlod (_MainTex, float4(copyUV,0,_Lod))*0.123317;
//                 copyUV.x = i.uv.x + stepX;
//                 copyUV.y = i.uv.y + stepY;
//                 color += tex2Dlod (_MainTex, float4(copyUV,0,_Lod))*0.077847;


//         //      //5*5 gaus
//         //      fixed2 copyUV;
//         //
//         //      copyUV.x = i.uv.x + stepX*-2;
//         //      copyUV.y = i.uv.y + stepY*-2;
//         //      color += tex2D (_MainTex, copyUV)*0.0036630036; // 1/273
//         //      copyUV.x = i.uv.x - stepX; //*-1
//         //      copyUV.y = i.uv.y + stepY*-2;
//         //      color += tex2D (_MainTex, copyUV)*0.0146520146; // 4/273
//         //      copyUV.x = i.uv.x; // + stepX*0;
//         //      copyUV.y = i.uv.y + stepY*-2;
//         //      color += tex2D (_MainTex, copyUV)*0.0256410256; // 7/273
//         //      copyUV.x = i.uv.x + stepX; //*1
//         //      copyUV.y = i.uv.y + stepY*-2;
//         //      color += tex2D (_MainTex, copyUV)*0.0146520146; // 4/273
//         //      copyUV.x = i.uv.x + stepX*2;
//         //      copyUV.y = i.uv.y + stepY*-2;
//         //      color += tex2D (_MainTex, copyUV)*0.0036630036; // 1/273
//         //
//         //      copyUV.x = i.uv.x + stepX*-2;
//         //      copyUV.y = i.uv.y - stepY; //*-1;
//         //      color += tex2D (_MainTex, copyUV)*0.0146520146; // 4/273
//         //      copyUV.x = i.uv.x - stepX; //*-1;
//         //      copyUV.y = i.uv.y - stepY; //*-1;
//         //      color += tex2D (_MainTex, copyUV)*0.0586080586; // 16/273
//         //      copyUV.x = i.uv.x; // + stepX*0;
//         //      copyUV.y = i.uv.y - stepY; //*-1;
//         //      color += tex2D (_MainTex, copyUV)*0.0952380952; // 26/273
//         //      copyUV.x = i.uv.x + stepX; //*1;
//         //      copyUV.y = i.uv.y - stepY; //*-1;
//         //      color += tex2D (_MainTex, copyUV)*0.0586080586; // 16/273
//         //      copyUV.x = i.uv.x + stepX*2;
//         //      copyUV.y = i.uv.y - stepY; //*-1;
//         //      color += tex2D (_MainTex, copyUV)*0.0146520146; // 4/273
//         //
//         //      copyUV.x = i.uv.x + stepX*-2;
//         //      copyUV.y = i.uv.y; // + stepY*0;
//         //      color += tex2D (_MainTex, copyUV)*0.0256410256; // 7/273
//         //      copyUV.x = i.uv.x - stepX; //*-1;
//         //      copyUV.y = i.uv.y; // + stepY*0;
//         //      color += tex2D (_MainTex, copyUV)*0.0952380952; // 26/273
//         //      copyUV.x = i.uv.x; // + stepX*0;
//         //      copyUV.y = i.uv.y; // + stepY*0;
//         //      color += tex2D (_MainTex, copyUV)*0.1501831501; // 41/273
//         //      copyUV.x = i.uv.x + stepX; //*1;
//         //      copyUV.y = i.uv.y; // + stepY*0;
//         //      color += tex2D (_MainTex, copyUV)*0.0952380952; // 26/273
//         //      copyUV.x = i.uv.x + stepX*2;
//         //      copyUV.y = i.uv.y; // + stepY*0;
//         //      color += tex2D (_MainTex, copyUV)*0.0256410256; // 7/273
//         //
//         //      copyUV.x = i.uv.x + stepX*-2;
//         //      copyUV.y = i.uv.y + stepY;
//         //      color += tex2D (_MainTex, copyUV)*0.0146520146; // 4/273
//         //      copyUV.x = i.uv.x - stepX; //*-1;
//         //      copyUV.y = i.uv.y + stepY;
//         //      color += tex2D (_MainTex, copyUV)*0.0586080586; // 16/273
//         //      copyUV.x = i.uv.x; // + stepX*0;
//         //      copyUV.y = i.uv.y + stepY;
//         //      color += tex2D (_MainTex, copyUV)*0.0952380952; // 26/273
//         //      copyUV.x = i.uv.x + stepX; //*1;
//         //      copyUV.y = i.uv.y + stepY;
//         //      color += tex2D (_MainTex, copyUV)*0.0586080586; // 16/273
//         //      copyUV.x = i.uv.x + stepX*2;
//         //      copyUV.y = i.uv.y + stepY;
//         //      color += tex2D (_MainTex, copyUV)*0.0146520146; // 4/273
//         //
//         //      copyUV.x = i.uv.x + stepX*-2;
//         //      copyUV.y = i.uv.y + stepY*2;
//         //      color += tex2D (_MainTex, copyUV)*0.0036630036; // 1/273
//         //      copyUV.x = i.uv.x - stepX; //*-1;
//         //      copyUV.y = i.uv.y + stepY*2;
//         //      color += tex2D (_MainTex, copyUV)*0.0146520146; // 4/273
//         //      copyUV.x = i.uv.x; // + stepX*0;
//         //      copyUV.y = i.uv.y + stepY*2;
//         //      color += tex2D (_MainTex, copyUV)*0.0256410256; // 7/273
//         //      copyUV.x = i.uv.x + stepX; //*1;
//         //      copyUV.y = i.uv.y + stepY*2;
//         //      color += tex2D (_MainTex, copyUV)*0.0146520146; // 4/273
//         //      copyUV.x = i.uv.x + stepX*2;
//         //      copyUV.y = i.uv.y + stepY*2;
//         //      color += tex2D (_MainTex, copyUV)*0.0036630036; // 1/273

//                 return color;
//             }
//             //ENDHLSL
//             ENDCG
//         }
//     }
// }