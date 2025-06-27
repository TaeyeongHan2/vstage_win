Shader "AkilliMum/URP/AR/Mirrors"
{
    Properties
    {
        //new values
        [HideInInspector]_ReflectionTex("Reflection", 2D) = "white" { } //left or all
        [HideInInspector]_ReflectionTexOther("ReflectionOther", 2D) = "white" { } //right
        [HideInInspector]_ReflectionIntensity("Reflection Intensity", Float) = 0.5
        [HideInInspector]_LODLevel("LOD Level", Float) = 0.
        //[HideInInspector]_WetLevel("WetLevel", Float) = 0.
        [HideInInspector]_WorkType("Work Type", Float) = 1.
        //[HideInInspector]_MirrorCenter("Mirror Center", Vector) = (0,0,0,0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, None, 99)] _ClipUV("Clip UV", int) = 99

        [HideInInspector]_RefractionTex("Refraction", 2D) = "bump" {}
        [HideInInspector]_ReflectionRefraction("Reflection Refraction", Float) = 0.0
        
        [HideInInspector]_EnableDepthBlur("Enable Depth Blur", Float) = -1.0
        
        [HideInInspector]_EnableSimpleDepth("Enable Simple Depth", Float) = -1.0
        [HideInInspector]_SimpleDepthCutoff("Simple Depth Cutoff", Float) = 0.5
        //[HideInInspector]_DepthBlur("Depth Blur", Float) = 1
        [HideInInspector]_ReflectionTexDepth("ReflectionDepth", 2D) = "white" { } //left or all
        [HideInInspector]_ReflectionTexOtherDepth("ReflectionOtherDepth", 2D) = "white" { } //right
        [HideInInspector]_NearClip("Near Clip", Float) = 0.3
        [HideInInspector]_FarClip("Far Clip", Float) = 1000
        
        [HideInInspector]_EnableMask("Enable Mask", Float) = -1.0
        [HideInInspector]_MaskTex("Mask", 2D) = "white" {}
        [HideInInspector]_MaskCutoff("Mask Cutoff", Float) = 0.5
        [HideInInspector]_MaskEdgeDarkness("Mask Edge Darkness", Float) = 1.0
        [HideInInspector]_MaskTiling("Mask Tiling", Vector) = (1,1,1,1)
        
        [HideInInspector]_EnableWave("Enable Waves", Float) = -1.0
        [HideInInspector]_WaveNoiseTex("Wave Noise Tex", 2D) = "white" {}
        [HideInInspector]_WaveSize("Wave Size", float) = 12.0
        [HideInInspector]_WaveDistortion("Wave Distortion", Float) = 0.02
        [HideInInspector]_WaveSpeed("Wave Speed", Float) = 3.0
        
        [HideInInspector]_EnableRipple("Enable Ripples", Float) = -1.0
        [HideInInspector]_RippleTex("Ripple", 2D) = "bump" {}
        [HideInInspector]_RippleSize("Ripple Size", Float) = 2.0
        [HideInInspector]_RippleRefraction("Ripple Refraction", Float) = 0.02
        [HideInInspector]_RippleDensity("Ripple Density", Float) = 1.0
        [HideInInspector]_RippleSpeed("Ripple Speed", Float) = 0.3
        
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }
        LOD 300

        Pass
        {
            Name "ARMirrors"
            Tags
            {
            }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            
            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _ALPHAPREMULTIPLY_ON
            
            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex HiddenVertex
            #pragma fragment HiddenFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_RefractionTex);       SAMPLER(sampler_RefractionTex);
            TEXTURE2D(_ReflectionTex);       SAMPLER(sampler_ReflectionTex);
            TEXTURE2D(_ReflectionTexOther);       SAMPLER(sampler_ReflectionTexOther);
            TEXTURE2D(_ReflectionTexDepth);       SAMPLER(sampler_ReflectionTexDepth);
            TEXTURE2D(_ReflectionTexOtherDepth);       SAMPLER(sampler_ReflectionTexOtherDepth);
            TEXTURE2D(_MaskTex);       SAMPLER(sampler_MaskTex);
            TEXTURE2D(_RippleTex);       SAMPLER(sampler_RippleTex);
            TEXTURE2D(_WaveNoiseTex);       SAMPLER(sampler_WaveNoiseTex);
            
            half _EnableDepthBlur;

            half _EnableSimpleDepth;
            float _SimpleDepthCutoff;
            //float _DepthBlur;
            float _NearClip;
            float _FarClip;

            half _ReflectionIntensity;
            float _LODLevel;
            //float _WetLevel;

            half _ReflectionRefraction;

            half _EnableMask;
            half _MaskCutoff;
            half _MaskEdgeDarkness;
            half4 _MaskTiling;

            //half _UseOpaqueCamImage; //todo:

            half _EnableWave;
            half _WaveSize;
            half _WaveDistortion;
            half _WaveSpeed;

            half _EnableRipple;
            half _RippleSize;
            half _RippleRefraction;
            half _RippleDensity;
            half _RippleSpeed;

            half _Surface;
            float _WorkType;
            //half4 _MirrorCenter;
            int _ClipUV;

            struct Attributes
            {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 positionOS   : POSITION;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 screenPos    : TEXCOORD0;
                float pdistance     : TEXCOORD1;
            };

            Varyings HiddenVertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

                output.positionCS = vertexInput.positionCS;
                output.screenPos = ComputeScreenPos(vertexInput.positionCS);
                output.pdistance = distance(_WorldSpaceCameraPos, mul(unity_ObjectToWorld, input.positionOS));
                return output;
            }

            half4 HiddenFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            //return half4(0, 1, 0, 1);

                float2 screenUV = (input.screenPos.xy) / (input.screenPos.w);

                //float eyeIndex = 0;
                //#ifdef UNITY_SINGLE_PASS_STEREO
                //   eyeIndex = unity_StereoEyeIndex;
                //#else
                //When not using single pass stereo rendering, eye index must be determined by testing the
                //sign of the horizontal skew of the projection matrix.
                //if (unity_CameraProjection[0][2] > 0) {
                //   eyeIndex = 1.0;
                //} else {
                //   eyeIndex = 0.0;
                //}
                //#endif

                //#if UNITY_SINGLE_PASS_STEREO  //!!LWRP does not need that, i suppose it already corrects it with UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                    //If Single-Pass Stereo mode is active, transform the
                    //coordinates to get the correct output UV for the current eye.
                   //float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
                   //screenUV = (screenUV - scaleOffset.zw) / scaleOffset.xy;
                //#endif

                float4 reflection = SAMPLE_TEXTURE2D (_ReflectionTex, sampler_ReflectionTex, screenUV);
                //GetReflectionTexture(eyeIndex, screenUV);
                
                float alpha = reflection.a;
                
                if(_EnableDepthBlur>0)
                {
                    _ReflectionIntensity = reflection.a * _ReflectionIntensity; //alpha value will be set on depth blur shader

                    alpha = _ReflectionIntensity;
                }
                else if(_EnableSimpleDepth>0)
                {
                    float sceneDepthAtFrag = SAMPLE_DEPTH_TEXTURE(_ReflectionTexDepth, sampler_ReflectionTexDepth, screenUV).r;
                    //GetDepthTexture(eyeIndex, screenUV).r;
                          
                #if UNITY_REVERSED_Z
                    sceneDepthAtFrag = 1 - LinearEyeDepth(sceneDepthAtFrag, _ZBufferParams);
                #else
                    sceneDepthAtFrag = LinearEyeDepth(sceneDepthAtFrag, _ZBufferParams);
                #endif

                    float x, y, z, w; //pass camera clipping planes to shader
                #if UNITY_REVERSED_Z //SHADER_API_GLES3 // insted of UNITY_REVERSED_Z
                    x = -1.0 + _NearClip / _FarClip;
                    y = 1;
                    z = x / _NearClip;
                    w = 1 / _NearClip;
                #else
                    x = 1.0 - _NearClip / _FarClip;
                    y = _NearClip / _FarClip;
                    z = x / _NearClip;
                    w = y / _NearClip;
                #endif

                    //sceneDepthAtFrag = 1.0 / (z * sceneDepthAtFrag + w);
                    //float fragDepth = input.eyePos.z * -1;
                    //float depth = sceneDepthAtFrag;
                    //depth = pow(depth, _DepthCutoff * fragDepth);
                    //_ReflectionIntensity = depth; //change reflection intensity!!

                    sceneDepthAtFrag = 1.0 / (z * sceneDepthAtFrag + w);
                    
                    //float fragDepth = abs(eyePos.z);// * -1;

                    float depth = sceneDepthAtFrag;
                    
                    depth = clamp(pow(depth, _SimpleDepthCutoff * input.pdistance), 0., 1.);
                    
                    _ReflectionIntensity = depth; //change reflection intensity!!

                    alpha = depth; 
                }
                
                return float4(reflection.rgb, alpha);
                //return float4(reflection.rgb, 0.5);
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }

    FallBack "Hidden/InternalErrorShader"
}