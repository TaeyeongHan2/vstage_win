Shader "AkilliMum/URP/AR/MirrorsOnly"
{
    Properties
    {
        //new values
        [HideInInspector]_ReflectionTex("Reflection", 2D) = "white" { } //left or all
        [HideInInspector]_ReflectionTexOther("ReflectionOther", 2D) = "white" { } //right
        [HideInInspector]_ReflectionIntensity("Reflection Intensity", Float) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }
        LOD 300

        Pass
        {
            Name "ARMirrorsOnly"
            Tags
            {
            }
            
            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            
            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex HiddenVertex
            #pragma fragment HiddenFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_ReflectionTex);       SAMPLER(sampler_ReflectionTex);
            TEXTURE2D(_ReflectionTexOther);       SAMPLER(sampler_ReflectionTexOther);
            float _ReflectionIntensity;

            struct Attributes
            {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 positionOS   : POSITION;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 screenPos    : TEXCOORD0;
            };

            Varyings HiddenVertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

                output.positionCS = vertexInput.positionCS;
                output.screenPos = ComputeScreenPos(vertexInput.positionCS);

                return output;
            }

            half4 HiddenFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 screenUV = (input.screenPos.xy) / (input.screenPos.w);

                //float eyeIndex = 0;
                //#ifdef UNITY_SINGLE_PASS_STEREO
                  // eyeIndex = unity_StereoEyeIndex;
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
                    //lerp(SAMPLE_TEXTURE2D (_ReflectionTex, sampler_ReflectionTex, screenUV),
                      //   SAMPLE_TEXTURE2D (_ReflectionTexOther, sampler_ReflectionTexOther, screenUV),
                        // eyeIndex < 1 ? 0 : 1);
                //SAMPLE_TEXTURE2D (_ReflectionTex, sampler_ReflectionTex, screenUV);
                
                return reflection * _ReflectionIntensity;
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }

    FallBack "Hidden/InternalErrorShader"
}