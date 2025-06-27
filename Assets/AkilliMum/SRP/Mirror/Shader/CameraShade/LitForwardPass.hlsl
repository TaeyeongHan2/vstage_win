//URP Reflective

#ifndef UNIVERSAL_FORWARD_LIT_PASS_INCLUDED
#define UNIVERSAL_FORWARD_LIT_PASS_INCLUDED
  
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#if UNITY_VERSION >= 202020
    // GLES2 has limited amount of interpolators
    #if defined(_PARALLAXMAP) && !defined(SHADER_API_GLES)
    #define REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR
    #endif

    #if (defined(_NORMALMAP) || (defined(_PARALLAXMAP) && !defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR))) || defined(_DETAIL)
    #define REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR
    #endif
#endif

// keep this file in sync with LitGBufferPass.hlsl

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    float2 lightmapUV   : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
#if UNITY_VERSION >= 202020

        float2 uv                       : TEXCOORD0;
        DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
        float3 positionWS               : TEXCOORD2;
    #endif

        float3 normalWS                 : TEXCOORD3;
    #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
        float4 tangentWS                : TEXCOORD4;    // xyz: tangent, w: sign
    #endif
        float3 viewDirWS                : TEXCOORD5;

        half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        float4 shadowCoord              : TEXCOORD7;
    #endif

    #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
        float3 viewDirTS                : TEXCOORD8;
    #endif

        //also must be defined on unityverison>202020!!!!!!!!!!
        float4 screenPos                : TEXCOORD9;
        float distance                  : TEXCOORD10;
        float4 worldPos                 : TEXCOORD11;
        float3 viewDir                  : TEXCOORD12;
        //float3 eyePos                 //: TEXCOORD12;
        float eyeIndex                  : TEXCOORD13;
        float3 objectPosWSRotated       : TEXCOORD14;

        float4 positionCS               : SV_POSITION;
        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO

#else

    float2 uv                       : TEXCOORD0;
    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

#if UNITY_VERSION >= 201936
    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
        float3 positionWS               : TEXCOORD2;
    #endif
#else
    #ifdef _ADDITIONAL_LIGHTS
        float3 positionWS               : TEXCOORD2;
    #endif
#endif

#ifdef _NORMALMAP
    float4 normalWS                 : TEXCOORD3;    // xyz: normal, w: viewDir.x
    float4 tangentWS                : TEXCOORD4;    // xyz: tangent, w: viewDir.y
    float4 bitangentWS              : TEXCOORD5;    // xyz: bitangent, w: viewDir.z
#else
    float3 normalWS                 : TEXCOORD3;
    float3 viewDirWS                : TEXCOORD4;
#endif

    half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light

#if UNITY_VERSION >= 201936
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        float4 shadowCoord          : TEXCOORD7;
    #endif
#else
    #ifdef _MAIN_LIGHT_SHADOWS
        float4 shadowCoord          : TEXCOORD7;
    #endif
#endif

    //also must be defined on unityverison>202020!!!!!!!!!!
    float4 screenPos                : TEXCOORD8;
    float distance                  : TEXCOORD9;
    float4 worldPos                 : TEXCOORD10;
    float3 viewDir                  : TEXCOORD11;
    //float3 eyePos                 //: TEXCOORD12;
    float eyeIndex                  : TEXCOORD12;
    float3 objectPosWSRotated       : TEXCOORD13;

    float4 positionCS               : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO

#endif

};

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;

#if UNITY_VERSION >= 201936
    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
        inputData.positionWS = input.positionWS;
    #endif
#else
    #ifdef _ADDITIONAL_LIGHTS
        inputData.positionWS = input.positionWS;
    #endif
#endif

#if UNITY_VERSION >= 202020
    half3 viewDirWS = SafeNormalize(input.viewDirWS);
    #if defined(_NORMALMAP) || defined(_DETAIL)
        float sgn = input.tangentWS.w;      // should be either +1 or -1
        float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
        inputData.normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz));
    #else
        inputData.normalWS = input.normalWS;
    #endif

        inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
        inputData.viewDirectionWS = viewDirWS;
#else
    #ifdef _NORMALMAP
        half3 viewDirWS = half3(input.normalWS.w, input.tangentWS.w, input.bitangentWS.w);
        inputData.normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz, input.bitangentWS.xyz, input.normalWS.xyz));
    #else
        half3 viewDirWS = input.viewDirWS;
        inputData.normalWS = input.normalWS;
    #endif

        inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
        viewDirWS = SafeNormalize(viewDirWS);

        inputData.viewDirectionWS = viewDirWS;
#endif



#if UNITY_VERSION >= 201936
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        inputData.shadowCoord = input.shadowCoord;
    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
        inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
    #else
        inputData.shadowCoord = float4(0, 0, 0, 0);
    #endif
#else
    #if defined(_MAIN_LIGHT_SHADOWS) && !defined(_RECEIVE_SHADOWS_OFF)
        inputData.shadowCoord = input.shadowCoord;
    #else
        inputData.shadowCoord = float4(0, 0, 0, 0);
    #endif
#endif

    inputData.fogCoord = input.fogFactorAndVertexLight.x;
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
    inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
#if UNITY_VERSION >= 202020
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
#endif
}

void Unity_RotateAboutAxis_Degrees_float(float3 In, float3 Axis, float Rotation, out float3 Out)
{
    Rotation = radians(Rotation);
    float s = sin(Rotation);
    float c = cos(Rotation);
    float one_minus_c = 1.0 - c;

    Axis = normalize(Axis);
    float3x3 rot_mat =
    { one_minus_c * Axis.x * Axis.x + c, one_minus_c * Axis.x * Axis.y - Axis.z * s, one_minus_c * Axis.z * Axis.x + Axis.y * s,
        one_minus_c * Axis.x * Axis.y + Axis.z * s, one_minus_c * Axis.y * Axis.y + c, one_minus_c * Axis.y * Axis.z - Axis.x * s,
        one_minus_c * Axis.z * Axis.x - Axis.y * s, one_minus_c * Axis.y * Axis.z + Axis.x * s, one_minus_c * Axis.z * Axis.z + c
    };
    Out = mul(rot_mat, In);
}

//rotates the point according to our rotation input and returns result
float3 CreateRotation(float3 toRotate) {
    float3 output = toRotate;

    if (_EnviRotation.x != 0)
        Unity_RotateAboutAxis_Degrees_float(output, float3 (1, 0, 0), -_EnviRotation.x, output);
    if (_EnviRotation.y != 0)
        Unity_RotateAboutAxis_Degrees_float(output, float3 (0, 1, 0), -_EnviRotation.y, output);
    if (_EnviRotation.z != 0)
        Unity_RotateAboutAxis_Degrees_float(output, float3 (0, 0, 1), -_EnviRotation.z, output);

    return output;
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard (Physically Based) shader
Varyings LitPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

     //float4 posWorld = mul(unity_ObjectToWorld, input.positionOS.xyz);
    // #if UNITY_REQUIRE_FRAG_WORLDPOS
    //     #if UNITY_PACK_WORLDPOS_WITH_TANGENT
    //         o.tangentToWorldAndPackedData[0].w = posWorld.x;
    //         o.tangentToWorldAndPackedData[1].w = posWorld.y;
    //         o.tangentToWorldAndPackedData[2].w = posWorld.z;
    //     #else
    //         o.posWorld = posWorld.xyz;
    //     #endif
    // #endif
    // o.pos = UnityObjectToClipPos(v.vertex);
    // o.tex = TexCoords(v);
    //output.eyeVec = NormalizeNormalPerVertex(posWorld.xyz - _WorldSpaceCameraPos);
 
    ////rotate the item on objects space (we will use only for ref probe sampling) according to room rotation    
    //VertexPositionInputs vertexInputRotated = GetVertexPositionInputs(CreateRotation(input.positionOS.xyz));
    ////VertexNormalInputs normalInputRotated = GetVertexNormalInputs(CreateRotation(input.normalOS), input.tangentOS);
    ////use fake WS for ref probe sampling
    //output.objectPosWSRotated = vertexInputRotated.positionWS;
    /*half3 viewDirWSRotated = GetCameraPositionWS() - vertexInputRotated.positionWS;
#ifdef _NORMALMAP
    output.normalWSRotated = half4(normalInputRotated.normalWS, viewDirWSRotated.x);
#else
    output.normalWSRotated = NormalizeNormalPerVertex(normalInputRotated.normalWS);
#endif
    output.viewDirWSRotated = viewDirWSRotated;*/


    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    output.screenPos = ComputeScreenPos(vertexInput.positionCS);
    output.worldPos = mul(unity_ObjectToWorld, vertexInput.positionCS);

    output.distance = distance(_WorldSpaceCameraPos, mul(unity_ObjectToWorld, input.positionOS));

    // normalWS and tangentWS already normalize.
    // this is required to avoid skewing the direction during interpolation
    // also required for per-vertex lighting and SH evaluation
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    output.objectPosWSRotated = vertexInput.positionWS;

#ifdef _LCRS_PROBE_ROTATION
    float3 ObjectPosition = mul(unity_ObjectToWorld, float4(0.0, 0.0, 0.0, 1.0)).xyz;
    ObjectPosition += _EnviPosition;    
    output.objectPosWSRotated -= ObjectPosition;    
    output.objectPosWSRotated = CreateRotation(output.objectPosWSRotated);
    output.objectPosWSRotated += ObjectPosition;
#endif

#if UNITY_VERSION >= 202020
    half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
#else
    half3 viewDirWS = GetCameraPositionWS() - vertexInput.positionWS;
#endif
    //float3 viewDirWS = normalize(i.worldPosition - _WorldSpaceCameraPos); //water
    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
    
    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    
#if UNITY_VERSION >= 202020

    // already normalized from normal transform to WS.
    output.normalWS = normalInput.normalWS;
    output.viewDirWS = viewDirWS;
    #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR) || defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
        real sign = input.tangentOS.w * GetOddNegativeScale();
        half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
    #endif
    #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
        output.tangentWS = tangentWS;
    #endif

    #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
        half3 viewDirTS = GetViewDirectionTangentSpace(tangentWS, output.normalWS, viewDirWS);
        output.viewDirTS = viewDirTS;
    #endif

        OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
        OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

        output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
        output.positionWS = vertexInput.positionWS;
    #endif

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        output.shadowCoord = GetShadowCoord(vertexInput);
    #endif

        output.positionCS = vertexInput.positionCS;

#else

    #ifdef _NORMALMAP
        output.normalWS = half4(normalInput.normalWS, viewDirWS.x);
        output.tangentWS = half4(normalInput.tangentWS, viewDirWS.y);
        output.bitangentWS = half4(normalInput.bitangentWS, viewDirWS.z);
    #else
        output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS); 
        output.viewDirWS = viewDirWS;
    #endif
        
        OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
        OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

        output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

    #if UNITY_VERSION >= 201936
        #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
            output.positionWS = vertexInput.positionWS;
        #endif

        #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.shadowCoord = GetShadowCoord(vertexInput);
        #endif
    #else
        #ifdef _ADDITIONAL_LIGHTS
            output.positionWS = vertexInput.positionWS;
        #endif

        #if defined(_MAIN_LIGHT_SHADOWS) && !defined(_RECEIVE_SHADOWS_OFF)
            output.shadowCoord = GetShadowCoord(vertexInput);
        #endif
    #endif

        output.positionCS = vertexInput.positionCS;

#endif
   
        output.viewDir = viewDirWS;
        
        //output.eyePos = mul(UNITY_MATRIX_MV, vertexInput.positionCS);

    #if defined(UNITY_STEREO_INSTANCING_ENABLED) 

        output.eyeIndex = unity_StereoEyeIndex;

    #elif defined(UNITY_SINGLE_PASS_STEREO)

        output.eyeIndex = unity_StereoEyeIndex;

        // If Single-Pass Stereo mode is active, transform the
        // coordinates to get the correct output UV for the current eye.
        /*float4 scaleOffset = unity_StereoScaleOffset[output.eyeIndex];
        screenUV = (screenUV - scaleOffset.zw) / scaleOffset.xy;*/

    #else
        // When not using single pass stereo rendering, eye index must be determined by testing the
        // sign of the horizontal skew of the projection matrix.
        if (_DeviceType == 2.) //rift-s
        {
            if (unity_CameraProjection[0][2] > 0) {
                output.eyeIndex = 0.;
            }
            else {
                output.eyeIndex = 1.;
            }
        }
        else {
            if (unity_CameraProjection[0][2] > 0) {
                output.eyeIndex = 1.;
            }
            else {
                output.eyeIndex = 0.;
            }
        }
    #endif

    return output;
}

float mod(float a, float b)
{
    return a - floor(a / b) * b;
}
float2 mod(float2 a, float2 b)
{
    return a - floor(a / b) * b;
}
float3 mod(float3 a, float3 b)
{
    return a - floor(a / b) * b;
}
float4 mod(float4 a, float4 b)
{
    return a - floor(a / b) * b;
} 

// #define MAX_RADIUS 2
// #define HASHSCALE1 .1031
// #define HASHSCALE3 float3(.1031, .1030, .0973)

// float hash12(float2 p)
// {
//     float3 p3  = frac(float3(p.xyx) * HASHSCALE1);
//     p3 += dot(p3, p3.yzx + 19.19);
//     return frac((p3.x + p3.y) * p3.z);
// }

// float2 hash22(float2 p)
// {
//     float3 p3 = frac(float3(p.xyx) * HASHSCALE3);
//     p3 += dot(p3, p3.yzx+19.19);
//     return frac((p3.xx+p3.yz)*p3.zy);
// }

// float4 GetWithLOD(Texture2D tex, SamplerState sam, float2 uv){
//     if (_LODLevel>0) {
//         return SAMPLE_TEXTURE2D_LOD (tex, sam, uv, _LODLevel);
//     } else {
//         return SAMPLE_TEXTURE2D (tex, sam, uv);
//     }
// }

float4 GetReflectionTexture(float eyeIndex, float2 uv){
    if(eyeIndex == 0.){
        if(_LODLevel < 1.){
            return SAMPLE_TEXTURE2D (_ReflectionTex, sampler_ReflectionTex, uv);
        }
        else{
            return SAMPLE_TEXTURE2D_LOD (_ReflectionTex, sampler_ReflectionTex, uv, _LODLevel);
        }
    }
    else{
        if(_LODLevel < 1.){
            return SAMPLE_TEXTURE2D (_ReflectionTexOther, sampler_ReflectionTexOther, uv);
        }
        else{
            return SAMPLE_TEXTURE2D_LOD (_ReflectionTexOther, sampler_ReflectionTexOther, uv, _LODLevel);
        }
    }
    // return
    //     lerp
    //     (
    //         lerp 
    //         (
    //             SAMPLE_TEXTURE2D (_ReflectionTex, sampler_ReflectionTex, uv),
    //             SAMPLE_TEXTURE2D_LOD (_ReflectionTex, sampler_ReflectionTex, uv, _LODLevel),
    //             _LODLevel
    //         ),
    //         lerp
    //         (
    //             SAMPLE_TEXTURE2D (_ReflectionTexOther, sampler_ReflectionTexOther, uv),
    //             SAMPLE_TEXTURE2D_LOD (_ReflectionTexOther, sampler_ReflectionTexOther, uv, _LODLevel),
    //             _LODLevel
    //         ),
    //         eyeIndex
    //     );
}

float GetDepthTexture(float eyeIndex, float2 uv){
    if(eyeIndex == 0.){
        return SAMPLE_DEPTH_TEXTURE(_ReflectionTexDepth, sampler_ReflectionTexDepth, uv).r;
    }
    else{
        return SAMPLE_DEPTH_TEXTURE(_ReflectionTexOtherDepth, sampler_ReflectionTexOtherDepth, uv).r;
    }
    // return
    //     lerp
    //     (
    //         SAMPLE_DEPTH_TEXTURE(_ReflectionTexDepth, sampler_ReflectionTexDepth, uv).r,
    //         SAMPLE_DEPTH_TEXTURE(_ReflectionTexOtherDepth, sampler_ReflectionTexOtherDepth, uv).r,
    //         eyeIndex
    //     );
}

//#if defined(SHADER_API_OPENGL)  !defined(SHADER_TARGET_GLSL)
//#define UNITY_BUGGY_TEX2DPROJ4
#define UNITY_PROJ_COORD(a) a.xyw
//#else
//#define UNITY_PROJ_COORD(a) a
//#endif

half TestNeighboursForAR(float2 screenUV, float eyeIndex, half alpha) {
    float onePixelX = (1.0 / (_ScreenParams.xy / _ScreenParams.w + FLT_MIN).x);
    float onePixelY = (1.0 / (_ScreenParams.xy / _ScreenParams.w + FLT_MIN).y);
    
    float3 up, down, left, right = float3(0, 0, 0);

    for (int i = 1; i <= 3; i++)
    {
        float stepX = onePixelX * i;
        float stepY = onePixelY * i;

        up = GetReflectionTexture(eyeIndex, screenUV + float2(0, stepY)).rgb;
        down = GetReflectionTexture(eyeIndex, screenUV + float2(0, -stepY)).rgb;
        left = GetReflectionTexture(eyeIndex, screenUV + float2(-stepX, 0)).rgb;
        right = GetReflectionTexture(eyeIndex, screenUV + float2(stepX, 0)).rgb;
        
        half3 check = float3(0, 0, 0);

        if (all(check == up.rgb)
            ||
            all(check == right.rgb)
            ||
            all(check == down.rgb)
            ||
            all(check == left.rgb))
            return alpha * 0.25 * i; //so 3 pixels comming to the edges of the full transparency will fade
    }

    return alpha;
}

float3 LocalCorrect(float3 origVec, float3 bboxMin, float3 bboxMax, float3 objectPosWSRotated, float3 cubemapPos)
{
    // Find the ray intersection with box plane
    float3 invOrigVec = float3(1.0, 1.0, 1.0) / origVec;

    float3 intersecAtMaxPlane = (bboxMax - objectPosWSRotated) * invOrigVec;

    float3 intersecAtMinPlane = (bboxMin - objectPosWSRotated) * invOrigVec;

    // Get the largest intersection values (we are not intersted in negative values)
    float3 largestIntersec = max(intersecAtMaxPlane, intersecAtMinPlane);

    // Get the closest of all solutions
    float Distance = min(min(largestIntersec.x, largestIntersec.y), largestIntersec.z);

    // Get the intersection position
    float3 IntersectPositionWS = objectPosWSRotated + origVec * Distance;

    // Get corrected vector
    float3 localCorrectedVec = IntersectPositionWS - cubemapPos;
        
    return localCorrectedVec;
}

half3 GlossyEnvironmentReflectionEx(half3 reflectVector, half perceptualRoughness, half occlusion,
    float3 localCorrReflDirWS, half reflectionIntensity, float4 reflection)
    {
#if !defined(_ENVIRONMENTREFLECTIONS_OFF)

        half mip = PerceptualRoughnessToMipmapLevel(perceptualRoughness);
        //half4 encodedIrradiance = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectVector, mip);
        half4 encodedIrradiance = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, localCorrReflDirWS, mip);
        //half4 encodedIrradiance = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, rrr, mip);

//#if !defined(UNITY_USE_NATIVE_HDR)
//todo:! it was 
//#if defined(UNITY_USE_NATIVE_HDR) || defined(UNITY_DOTS_INSTANCING_ENABLED)
//on 202020 code! is that correct? There is no ! on HDR !!!!!

#if !defined(UNITY_USE_NATIVE_HDR) || defined(UNITY_DOTS_INSTANCING_ENABLED)
    half3 irradiance = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
    //half3 irradiance = DecodeHDREnvironment(encodedIrradiance, _EnviCubeMapBaked_HDR);
#else
    half3 irradiance = encodedIrradiance.rgb;
#endif

    //if(_MixBlackColor > 0 &&
    //    (reflection.a <= 0 || (reflection.r < 0.01 && reflection.g < 0.01 && reflection.b < 0.01))){
    //        return 0;
    //}

    if(_MixBlackColor > 0 && reflectionIntensity > 0)
    {
        return occlusion * reflection.rgb * reflectionIntensity;
    }
    else
    {
        return irradiance * occlusion;
    }
#endif // GLOSSY_REFLECTIONS

    return _GlossyEnvironmentColor.rgb * occlusion;
}

#if UNITY_VERSION >= 202020

    half3 GlobalIlluminationEx(BRDFData brdfData, BRDFData brdfDataClearCoat, float clearCoatMask,
        half3 bakedGI, half occlusion,
        half3 normalWS, half3 viewDirectionWS,
        float3 objectPosWSRotated, float3 bBoxMin, float3 bBoxMax, float3 enviCubeMapPos, half reflectionIntensity, float4 reflection)
    {
        half3 reflectVector = reflect(-viewDirectionWS, normalWS);

        #ifdef _LCRS_PROBE_ROTATION
            reflectVector = CreateRotation(reflectVector);
        #endif

        float3 localCorrReflDirWS = reflectVector;
        //find local correction if marked on shader!
        #ifdef _LOCALLYCORRECTION
            localCorrReflDirWS = LocalCorrect(reflectVector, bBoxMin, bBoxMax, objectPosWSRotated, enviCubeMapPos);
        #endif
        
        half NoV = saturate(dot(normalWS, viewDirectionWS));
        half fresnelTerm = Pow4(1.0 - NoV);

        half3 indirectDiffuse = bakedGI * occlusion;
        //half3 indirectSpecular = GlossyEnvironmentReflection(reflectVector, brdfData.perceptualRoughness, occlusion);

        half3 indirectSpecular = GlossyEnvironmentReflectionEx(reflectVector, brdfData.perceptualRoughness, occlusion,localCorrReflDirWS, reflectionIntensity, reflection);

        // half3 indirectSpecular;
        // if (reflectionIntensity > 0)
        // {
        //     indirectSpecular = half3(0,0,0);
        // }
        // else
        // {
        // }

        half3 color = EnvironmentBRDF(brdfData, indirectDiffuse, indirectSpecular, fresnelTerm);

        #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
            //half3 coatIndirectSpecular = GlossyEnvironmentReflection(reflectVector, brdfDataClearCoat.perceptualRoughness, occlusion);
            half3 coatIndirectSpecular = GlossyEnvironmentReflectionEx(localCorrReflDirWS, brdfDataClearCoat.perceptualRoughness, occlusion,localCorrReflDirWS);
            // TODO: "grazing term" causes problems on full roughness
            half3 coatColor = EnvironmentBRDFClearCoat(brdfDataClearCoat, clearCoatMask, coatIndirectSpecular, fresnelTerm);

            // Blend with base layer using khronos glTF recommended way using NoV
            // Smooth surface & "ambiguous" lighting
            // NOTE: fresnelTerm (above) is pow4 instead of pow5, but should be ok as blend weight.
            half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * fresnelTerm;
            return color * (1.0 - coatFresnel * clearCoatMask) + coatColor;
        #else
            return color;
        #endif
    }

#else

    half3 GlobalIlluminationEx(BRDFData brdfData, half3 bakedGI, half occlusion, half3 normalWS, half3 viewDirectionWS,
        float3 objectPosWSRotated, float3 bBoxMin, float3 bBoxMax, float3 enviCubeMapPos, half reflectionIntensity, float4 reflection)
    {    
        half3 reflectVector = reflect(-viewDirectionWS, normalWS);

    #ifdef _LCRS_PROBE_ROTATION
        reflectVector = CreateRotation(reflectVector);
    #endif

        float3 localCorrReflDirWS = reflectVector;
        //find local correction if marked on shader!
        #ifdef _LOCALLYCORRECTION
            localCorrReflDirWS = LocalCorrect(reflectVector, bBoxMin, bBoxMax, objectPosWSRotated, enviCubeMapPos);
        #endif

        //float3 localCorrReflDirWS = LocalCorrect(reflectVector, bBoxMin, bBoxMax, objectPosWSRotated, enviCubeMapPos);

        half fresnelTerm = Pow4(1.0 - saturate(dot(normalWS, viewDirectionWS)));

        half3 indirectDiffuse = bakedGI * occlusion;
        half3 indirectSpecular = GlossyEnvironmentReflectionEx(reflectVector, brdfData.perceptualRoughness, occlusion, localCorrReflDirWS, reflectionIntensity, reflection);

        // half3 indirectSpecular;
        // if (reflectionIntensity > 0)
        // {
        //     //indirectSpecular = half3(0,0,0);
        //     indirectSpecular = GlossyEnvironmentReflectionEx(reflectVector, 0, 0, localCorrReflDirWS);
        // }
        // else
        // {
        //     indirectSpecular = GlossyEnvironmentReflectionEx(reflectVector, brdfData.perceptualRoughness, occlusion, localCorrReflDirWS);
        // }

        return EnvironmentBRDF(brdfData, indirectDiffuse, indirectSpecular, fresnelTerm);
    }

#endif



#if UNITY_VERSION >= 202020

    ///////////////////////////////////////////////////////////////////////////////
    //                      Fragment Functions                                   //
    //       Used by ShaderGraph and others builtin renderers                    //
    ///////////////////////////////////////////////////////////////////////////////
    half4 UniversalFragmentPBREx(InputData inputData, SurfaceData surfaceData,
        float3 objectPosWSRotated, float3 bBoxMin, float3 bBoxMax, float3 enviCubeMapPos, half reflectionIntensity, float4 reflection)
    {
    #ifdef _SPECULARHIGHLIGHTS_OFF
        bool specularHighlightsOff = true;
    #else
        bool specularHighlightsOff = false;
    #endif

        BRDFData brdfData;

        // NOTE: can modify alpha
        InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);

        BRDFData brdfDataClearCoat = (BRDFData)0;
    #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
        // base brdfData is modified here, rely on the compiler to eliminate dead computation by InitializeBRDFData()
        InitializeBRDFDataClearCoat(surfaceData.clearCoatMask, surfaceData.clearCoatSmoothness, brdfData, brdfDataClearCoat);
    #endif

        // To ensure backward compatibility we have to avoid using shadowMask input, as it is not present in older shaders
    #if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
        half4 shadowMask = inputData.shadowMask;
    #elif !defined (LIGHTMAP_ON)
        half4 shadowMask = unity_ProbesOcclusion;
    #else
        half4 shadowMask = half4(1, 1, 1, 1);
    #endif

        Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask);

        #if defined(_SCREEN_SPACE_OCCLUSION)
            AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(inputData.normalizedScreenSpaceUV);
            mainLight.color *= aoFactor.directAmbientOcclusion;
            surfaceData.occlusion = min(surfaceData.occlusion, aoFactor.indirectAmbientOcclusion);
        #endif

        MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);
        half3 color = GlobalIlluminationEx(brdfData, brdfDataClearCoat, surfaceData.clearCoatMask,
                                        inputData.bakedGI, surfaceData.occlusion,
                                        inputData.normalWS, inputData.viewDirectionWS,
                                        objectPosWSRotated, bBoxMin, bBoxMax, enviCubeMapPos, reflectionIntensity, reflection);
        color += LightingPhysicallyBased(brdfData, brdfDataClearCoat,
                                        mainLight,
                                        inputData.normalWS, inputData.viewDirectionWS,
                                        surfaceData.clearCoatMask, specularHighlightsOff);

    #ifdef _ADDITIONAL_LIGHTS
        uint pixelLightCount = GetAdditionalLightsCount();
        for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
        {
            Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);
            #if defined(_SCREEN_SPACE_OCCLUSION)
                light.color *= aoFactor.directAmbientOcclusion;
            #endif
            color += LightingPhysicallyBased(brdfData, brdfDataClearCoat,
                                            light,
                                            inputData.normalWS, inputData.viewDirectionWS,
                                            surfaceData.clearCoatMask, specularHighlightsOff);
        }
    #endif

    #ifdef _ADDITIONAL_LIGHTS_VERTEX
        color += inputData.vertexLighting * brdfData.diffuse;
    #endif

        color += surfaceData.emission;

        return half4(color, surfaceData.alpha);
    }

#else

    ///////////////////////////////////////////////////////////////////////////////
    //                      Fragment Functions                                   //
    //       Used by ShaderGraph and others builtin renderers                    //
    ///////////////////////////////////////////////////////////////////////////////
    half4 UniversalFragmentPBREx(InputData inputData, half3 albedo, half metallic, half3 specular,
        half smoothness, half occlusion, half3 emission, half alpha,
        float3 objectPosWSRotated, float3 bBoxMin, float3 bBoxMax, float3 enviCubeMapPos, half reflectionIntensity, float4 reflection)
    {
        BRDFData brdfData;
        InitializeBRDFData(albedo, metallic, specular, smoothness, alpha, brdfData);

        Light mainLight = GetMainLight(inputData.shadowCoord);
        MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, half4(0, 0, 0, 0));

        half3 color = GlobalIlluminationEx(brdfData, inputData.bakedGI, occlusion, inputData.normalWS, inputData.viewDirectionWS,
            objectPosWSRotated, bBoxMin, bBoxMax, enviCubeMapPos, reflectionIntensity, reflection);
        
        color += LightingPhysicallyBased(brdfData, mainLight, inputData.normalWS, inputData.viewDirectionWS);

    #ifdef _ADDITIONAL_LIGHTS
        uint pixelLightCount = GetAdditionalLightsCount();
        for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
        {
            Light light = GetAdditionalLight(lightIndex, inputData.positionWS);
            color += LightingPhysicallyBased(brdfData, light, inputData.normalWS, inputData.viewDirectionWS);
        }
    #endif

    #ifdef _ADDITIONAL_LIGHTS_VERTEX
        color += inputData.vertexLighting * brdfData.diffuse;
    #endif

        color += emission;
        return half4(color, alpha);
    }
#endif

// Used in Standard (Physically Based) shader
half4 LitPassFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

#if UNITY_VERSION >= 202020
    #if defined(_PARALLAXMAP)
        #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
            half3 viewDirTS = input.viewDirTS;
        #else
            half3 viewDirTS = GetViewDirectionTangentSpace(input.tangentWS, input.normalWS, input.viewDirWS);
        #endif
        ApplyPerPixelDisplacement(viewDirTS, input.uv);
    #endif
#endif

    SurfaceData surfaceData;
    InitializeStandardLitSurfaceData(input.uv, surfaceData);

    if (_DeviceType == 30.) //AR mode, eyeIndex comes wrong on certain angles on AR!!!! so set it to zero always
    {
        input.eyeIndex = 0;
    }

    half4 maskAlpha = half4(1,1,1,1);
    half4 mask = half4(1,1,1,1);
    if(_EnableMask > 0){
    
        #if UNITY_VERSION >= 201900
        maskAlpha = SampleAlbedoAlpha(input.uv/half2(_MaskTiling.r,_MaskTiling.g), TEXTURE2D_ARGS(_MaskTex, sampler_MaskTex));
        #else
        maskAlpha = SampleAlbedoAlpha(input.uv/half2(_MaskTiling.r,_MaskTiling.g), TEXTURE2D_PARAM(_MaskTex, sampler_MaskTex));
        #endif
        
        mask = smoothstep(maskAlpha.a, 0, _MaskCutoff); 
    }
    
    //todo: moved
    ////recalculate according to mask
    //surfaceData.smoothness = lerp(surfaceData.smoothness, 1.0, mask.a);
    //// Water F0 specular is 0.02 (based on IOR of 1.33)
    //surfaceData.specular = lerp(surfaceData.specular, 0.02, mask.a);

    float2 screenUV = (input.screenPos.xy) / (input.screenPos.w+FLT_MIN);

    //#if UNITY_SINGLE_PASS_STEREO  //!!LWRP does not need that, i suppose it already corrects it with UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        //If Single-Pass Stereo mode is active, transform the
        //coordinates to get the correct output UV for the current eye.
       //float4 scaleOffset = unity_StereoScaleOffset[input.eyeIndex];
       //screenUV = (screenUV - scaleOffset.zw) / scaleOffset.xy;
    //#endif

    half3 nor = float3(1,1,1);
    if(_EnableRefraction > 0){
    
        #if UNITY_VERSION >= 201900
        nor = SampleNormal(input.uv, TEXTURE2D_ARGS(_RefractionTex, sampler_RefractionTex), _BumpScale);
        #else
        nor = SampleNormal(input.uv, TEXTURE2D_PARAM(_RefractionTex, sampler_RefractionTex), _BumpScale);
        #endif
        
        screenUV.xy += (nor * (_ReflectionRefraction / input.distance));
    }
    
    float4 reflection = float4(1,1,1,0);
    
    half3 col_orig1 = half3(1,1,1);
    half3 col_orig2 = half3(1,1,1);
    half3 wave1 = half3(1,1,1);
    half3 wave2 = half3(1,1,1);
    half2 input1 = half2(1,1);
    half2 input2 = half2(1,1);
    //half3 rippleUV = half3(0,0,0);
    if(_EnableWave > 0)
    {
        #if UNITY_VERSION >= 201900
        col_orig1 = SampleNormal(input.uv/_WaveSize + _WaveSpeed*_Time.y, TEXTURE2D_ARGS(_WaveNoiseTex, sampler_WaveNoiseTex), _BumpScale);
        col_orig2 = SampleNormal(input.uv/_WaveSize - _WaveSpeed*_Time.y, TEXTURE2D_ARGS(_WaveNoiseTex, sampler_WaveNoiseTex), _BumpScale);
        #else
        col_orig1 = SampleNormal(input.uv/_WaveSize + _WaveSpeed*_Time.y, TEXTURE2D_PARAM(_WaveNoiseTex, sampler_WaveNoiseTex), _BumpScale);
        col_orig2 = SampleNormal(input.uv/_WaveSize - _WaveSpeed*_Time.y, TEXTURE2D_PARAM(_WaveNoiseTex, sampler_WaveNoiseTex), _BumpScale);
        #endif

        _WaveDistortion /= input.distance;

        // float wave1 = (col_orig1.r * _WaveDistortion - _WaveDistortion / 2) * mask.a;
        // float wave2 = (col_orig2.g * _WaveDistortion - _WaveDistortion / 2) * mask.a;
        wave1 = (col_orig1 * _WaveDistortion) * mask.a;
        wave2 = (col_orig2 * _WaveDistortion) * mask.a;
        
        half2 screenUV1 = screenUV + wave1;
        half2 screenUV2 = screenUV - wave2;
        
        //input1 = input.uv + wave1;
        //input2 = input.uv - wave2;
        
        reflection = GetReflectionTexture(input.eyeIndex, screenUV1) / 2 +
                     GetReflectionTexture(input.eyeIndex, screenUV2) / 2;
    }
    else if (_EnableRipple > 0){
        
        //float2 temp_cast_0 = (_RainDrops_Tile).xx;                                                //RAIN
        float2 temp_cast_0 = (_RippleSize).xx;                                              //RAIN
        float2 uv_TexCoord53 = input.uv * temp_cast_0;                                      //RAIN
        float2 appendResult57 = (float2(frac(uv_TexCoord53.x), frac(uv_TexCoord53.y))); //RAIN
        // *** BEGIN Flipbook UV Animation vars ***
        // Total tiles of Flipbook Texture
        float fbtotaltiles58 = 8.0 * 8.0;                                                       //RAIN
        // Offsets for cols and rows of Flipbook Texture
        float fbcolsoffset58 = 1.0f / 8.0;                                                      //RAIN
        float fbrowsoffset58 = 1.0f / 8.0;                                                      //RAIN
        // Speed of animation
        //float fbspeed58 = _Time[1] * _RainSpeed;                                              //RAIN
        float fbspeed58 = _Time[1] * _RippleSpeed;                                              //RAIN
        // UV Tiling (col and row offset)
        float2 fbtiling58 = float2(fbcolsoffset58, fbrowsoffset58);                             //RAIN
        // UV Offset - calculate current tile linear index, and convert it to (X * coloffset, Y * rowoffset)
        // Calculate current tile linear index
        float fbcurrenttileindex58 = round(fmod(fbspeed58 + 0.0, fbtotaltiles58));          //RAIN
        fbcurrenttileindex58 += (fbcurrenttileindex58 < 0) ? fbtotaltiles58 : 0;                //RAIN
        // Obtain Offset X coordinate from current tile linear index
        float fblinearindextox58 = round(fmod(fbcurrenttileindex58, 8.0));              //RAIN
        // Multiply Offset X by coloffset
        float fboffsetx58 = fblinearindextox58 * fbcolsoffset58;                                //RAIN
        // Obtain Offset Y coordinate from current tile linear index
        float fblinearindextoy58 = round(fmod((fbcurrenttileindex58 - fblinearindextox58) / 8.0, 8.0));//RAIN
        // Reverse Y to get tiles from Top to Bottom
        fblinearindextoy58 = (int)(8.0 - 1) - fblinearindextoy58;                                   //RAIN
        // Multiply Offset Y by rowoffset
        float fboffsety58 = fblinearindextoy58 * fbrowsoffset58;                                //RAIN
        // UV Offset
        float2 fboffset58 = float2(fboffsetx58, fboffsety58);                                   //RAIN
        // Flipbook UV
        half2 fbuv58 = appendResult57 * fbtiling58 + fboffset58;                                //RAIN
        // *** END Flipbook UV Animation vars ***
        //float4 temp_output_63_0 = (tex2D(_Mask, customUVs39, float2(0, 0), float2(0, 0)) * i.vertexColor);
        if(_EnableMask > 0)
        {
            #if UNITY_VERSION >= 201900
            half3 ripNor = SampleNormal(fbuv58, TEXTURE2D_ARGS(_RippleTex, sampler_RippleTex), _RippleDensity);
            #else
            half3 ripNor = SampleNormal(fbuv58, TEXTURE2D_PARAM(_RippleTex, sampler_RippleTex), _RippleDensity);
            #endif

            float3 lerpResult61 = lerp(                                                             //RAIN
            //UnpackScaleNormal(tex2D(_Normal, customUVs39, temp_output_40_0, temp_output_41_0), _NormalScale),
            //o.Normal,
            surfaceData.normalTS,
            //UnpackScaleNormal(tex2D(_RippleTex, fbuv58), _RainDrops_Power),
            ripNor ,
            //temp_output_63_0.r);
            //wetAlpha);
            mask.a);
            //o.Normal = lerpResult61;                                                              //RAIN
            surfaceData.normalTS = lerpResult61;                                                              //RAIN
            //o.Normal = lerp(o.Normal, float3(0, 0, 1), wetAlpha);

            //normal = UnpackScaleNormal(tex2D(_RippleTex, fbuv58), _RippleDensity).mask;
        }
        else
        {
            //normal = UnpackScaleNormal(tex2D(_RippleTex, fbuv58), _RippleDensity);
            #if UNITY_VERSION >= 201900
            surfaceData.normalTS = SampleNormal(fbuv58, TEXTURE2D_ARGS(_RippleTex, sampler_RippleTex), _RippleDensity);
            #else
            surfaceData.normalTS = SampleNormal(fbuv58, TEXTURE2D_PARAM(_RippleTex, sampler_RippleTex), _RippleDensity);
            #endif
            //surfaceData.normalTS = SampleNormal(fbuv58, TEXTURE2D_ARGS(_RippleTex, sampler_RippleTex), _RippleDensity);
        }

        input2 = (surfaceData.normalTS * (_RippleRefraction / input.distance)); //use as static //so far away pixels will not be refracted very much
        input1 = input.uv - input2;//2_MainTex_ST; //will be used to animate real texture
        screenUV.xy -= input2; 
        
        reflection = GetReflectionTexture(input.eyeIndex, screenUV);
    }
    else{
        reflection = GetReflectionTexture(input.eyeIndex, screenUV);
    }
    
    //update normals
    if(_EnableRefraction > 0){
    
        #if UNITY_VERSION >= 201900
        half3 bump = SampleNormal(input.uv/_ReflectionRefraction, TEXTURE2D_ARGS(_RefractionTex, sampler_RefractionTex), _BumpScale);
        #else
        half3 bump = SampleNormal(input.uv/_ReflectionRefraction, TEXTURE2D_PARAM(_RefractionTex, sampler_RefractionTex), _BumpScale);
        #endif
    
        surfaceData.normalTS = 
            _ReflectionIntensity > 0 ?
                (
                    mask.a > 0 ?
                        bump * float3(0,0,mask.a) //nor *_ReflectionRefraction 
                        //surfaceData.normalTS + bump * mask.a //nor *_ReflectionRefraction 
                        :
                        surfaceData.normalTS
                )
                :
                surfaceData.normalTS;
    }
    
    if(_EnableWave > 0)
    {
        surfaceData.normalTS= 
            _ReflectionIntensity > 0 ? 
                (
                    //surfaceData.normalTS+col_orig1.rgg*mask.a
                    
                    mask.a > 0 ?
                        col_orig1.rgb * col_orig2.rgb * float3(0,0,mask.a)
                        :
                        surfaceData.normalTS
                )
                :
                surfaceData.normalTS;
                                       
        //recalculate albedo with similar waves:)
        if(_ReflectionIntensity > 0){
            //(col_orig1 * _WaveDistortion) * mask.a;
            //float3 a1 = (surfaceData.normalTS * (_WaveDistortion / input.distance)); //use as static //so far away pixels will not be refracted very much
            //float3 a2 = (surfaceData.normalTS * (_WaveDistortion / input.distance)); //use as static //so far away pixels will not be refracted very much
            input1 = input.uv + wave1;//2_MainTex_ST; //will be used to animate real texture
            input2 = input.uv - wave2;//2_MainTex_ST; //will be used to animate real texture
            //screenUV.xy -= input2; 
        
            #if UNITY_VERSION >= 201900
            surfaceData.albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input1);
            #else
            surfaceData.albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input1);
            #endif
            
        }
    }
    else if (_EnableRipple > 0)
    {
        //recalculate albedo with similar ripples:)
        if(_ReflectionIntensity > 0){
        
            #if UNITY_VERSION >= 201900
            surfaceData.albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input1);
            #else
            surfaceData.albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input1);
            #endif
            
        }
    }
    
    //lerp reflection  
    //reflection = lerp(reflection1, reflection2, input.eyeIndex);
    
    if(_EnableDepthBlur>0)
    {
        _ReflectionIntensity = reflection.a * _ReflectionIntensity; //alpha value will be set on depth blur shader

        surfaceData.alpha = _ReflectionIntensity;
    }
    else if(_EnableSimpleDepth>0)
    {
        float sceneDepthAtFrag = GetDepthTexture(input.eyeIndex, screenUV).r;
              
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
        
        depth = clamp(pow(depth, _SimpleDepthCutoff * input.distance), 0., 1.);
        
        _ReflectionIntensity = depth * _ReflectionIntensity; //change reflection intensity!!  //!!!!!!!!!!!!!!!!! multiply wiht intensity to give the ability to user the reflection amount!

        surfaceData.alpha = depth;
    }
    
    //if(_MixBlackColor > 0){
    //    float3 check = float3(0.005, 0.005, 0.005); //todo: why 0 does not work???
    //    //if (all(check.rgb == reflection.rgb)){
    //    if (check.r > reflection.r && check.g > reflection.g && check.b > reflection.b){
    //        //reflection.rgb = diffColor.rgb;
    //        _ReflectionIntensity = 0;
    //    } 
    //}

    //in black color mixing (probe mixing) clear color alpha is zero! But 

    if(_MixBlackColor > 0){
        if (reflection.a <= 0){
            _ReflectionIntensity = 0;
        }
    }

    //calculate output 
    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);
    
    if(_Surface != 1){ //manipulate if surface is not transparent
        if(_EnableMask>0){
            surfaceData.albedo = surfaceData.albedo * (1-mask.a) +
                surfaceData.albedo * mask.a * (1-_ReflectionIntensity);
                
            surfaceData.emission = reflection * _ReflectionIntensity * pow(mask.a,_MaskEdgeDarkness);
        }
        else{
            surfaceData.albedo   = (1-_ReflectionIntensity) * surfaceData.albedo;
            surfaceData.emission = _ReflectionIntensity     * reflection;
        }
    }

    if(_Surface == 1){ //transparent mix of reflection for glass like objects
        //no need masking because it already alpha enabled :)
        if(_WorkType == 3.) //My transparency
        {
            _ReflectionIntensity *= reflection.a;
            
            surfaceData.alpha = _ReflectionIntensity * mask;

            //if (all (reflection.rgb == float3(0,0,0))) {
            //    surfaceData.alpha = 0.;
            //}
            //else
            //{
            //    //test the neighbours
            //    surfaceData.alpha = TestNeighboursForAR(screenUV, input.eyeIndex, surfaceData.alpha);
            //}

            surfaceData.albedo = 0; // reflection* mask; 
            surfaceData.emission = reflection * mask;  //0;
        }
        else{
            surfaceData.albedo = surfaceData.albedo * surfaceData.alpha;
            surfaceData.emission = half4(reflection.rgb,_ReflectionIntensity) * (1 - surfaceData.alpha)* mask;

            //color = color * surfaceData.alpha +
                //half4(reflection.rgb,_ReflectionIntensity) * (1 - surfaceData.alpha);
        
            surfaceData.alpha = max(_ReflectionIntensity * mask, surfaceData.alpha);
        }
    }

    #if UNITY_VERSION >= 202020
        half4 color = UniversalFragmentPBREx(inputData, surfaceData,
            input.objectPosWSRotated, _BBoxMin.xyz, _BBoxMax.xyz, _EnviCubeMapPos.xyz, _ReflectionIntensity, reflection);
    #else
        half4 color = UniversalFragmentPBREx(inputData, surfaceData.albedo, surfaceData.metallic,
            surfaceData.specular, surfaceData.smoothness, surfaceData.occlusion, surfaceData.emission, surfaceData.alpha,
            input.objectPosWSRotated, _BBoxMin.xyz, _BBoxMax.xyz, _EnviCubeMapPos.xyz, _ReflectionIntensity, reflection);
    #endif
    
    // #ifdef _LOCALLYCORRECTION
    //     half4 color = UniversalFragmentPBREx(inputData, surfaceData.albedo, surfaceData.metallic, 
    //         surfaceData.specular, surfaceData.smoothness, surfaceData.occlusion, surfaceData.emission, surfaceData.alpha,
    //         input.objectPosWS, _BBoxMin.xyz, _BBoxMax.xyz, _EnviCubeMapPos.xyz);
    // #else
    //     half4 color = UniversalFragmentPBR(inputData, surfaceData.albedo, surfaceData.metallic,
    //         surfaceData.specular, surfaceData.smoothness, surfaceData.occlusion, surfaceData.emission, surfaceData.alpha);
    // #endif

    #ifdef _FULLMIRROR
        color = color * (1 - _ReflectionIntensity) + _ReflectionIntensity * half4(reflection.rgb, surfaceData.alpha);
    #endif

    if(_MixBlackColor > 0){
        if (reflection.a <= 0){
            color = color * (1 - _ReflectionIntensity) + _ReflectionIntensity * half4(reflection.rgb, surfaceData.alpha);
        }
    }
    
    color.rgb = MixFog(color.rgb, inputData.fogCoord);

#if UNITY_VERSION >= 202020
    color.a = OutputAlpha(color.a, _Surface);
#endif

    return color;
}

#endif
