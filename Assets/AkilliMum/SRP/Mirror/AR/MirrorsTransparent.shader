Shader "AkilliMum/URP/AR/MirrorsTransparent"
{
    Properties
    {
        //new values
        [HideInInspector]_ReflectionTex("Reflection", 2D) = "white" { } //left or all
        [HideInInspector]_ReflectionIntensity("Reflection Intensity", Float) = 0.5
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
            Name "ARMirrorsTransparent"
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

            TEXTURE2D(_ReflectionTex);       SAMPLER(sampler_ReflectionTex);
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

                float4 reflection = SAMPLE_TEXTURE2D (_ReflectionTex, sampler_ReflectionTex, screenUV);
                
                //if pixel is black (reflection is not there; full transparent)
                if(reflection.r == 0.0 && reflection.g == 0.0 && reflection.b == 0.0)
                    return half4(0, 0, 0, 0);
                else
                    return half4(reflection.rgb, _ReflectionIntensity);
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }

    FallBack "Hidden/InternalErrorShader"
}