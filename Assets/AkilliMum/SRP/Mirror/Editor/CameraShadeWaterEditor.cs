//#if _AKILLIMUM_WATER

////#define DEBUG_RENDER

//using System.Collections.Generic;
//using UnityEditor;
//using UnityEngine;

//namespace AkilliMum.SRP.Mirror
//{
//    [CustomEditor(typeof(CameraShadeWater))]
//    [CanEditMultipleObjects]
//    public class CameraShadeWaterEditor : Editor
//    {
//        bool MenuCommon = true;
//        bool MenuCamera = true;
//        bool MenuWater = true;
//        //bool MenuRefraction = true;
//        //bool MenuDepthBlur = true;
//        //bool MenuSimpleDepthBlur = true;
//        //bool MenuFinalShader = true;
//        //bool MenuWaves = true;
//        //bool MenuRipples = true;
//        //bool MenuMasking = true;
//        //bool MenuWater = true;

//#if DEBUG_RENDER
//        SerializedProperty DebugImage;
//        SerializedProperty DebugImageDepth;
//#endif
//        SerializedProperty IsEnabled;
//        SerializedProperty Name;
//        SerializedProperty TurnOffOcclusion;
//        //SerializedProperty IsMirrorInMirror;
//        //SerializedProperty MirrorInMirrorId;

//        SerializedProperty DeviceType;

//        SerializedProperty WorkType;
//        SerializedProperty UpVector;
//        SerializedProperty Intensity;
//        //SerializedProperty DisableGBuffer;
//        SerializedProperty RunForEveryXthFrame;
//        SerializedProperty CameraLODLevel;

//        SerializedProperty HDR;
//        SerializedProperty MSAA;
//        SerializedProperty DisablePixelLights;
//        SerializedProperty Shadow;
//        SerializedProperty Cull;
//        SerializedProperty CullDistance;
//        SerializedProperty TextureSize;
//        SerializedProperty ManualSize;
//        SerializedProperty UseCameraClipPlane;
//        SerializedProperty ClipPlaneOffset;
//        SerializedProperty ReflectLayers;
//        SerializedProperty RefractLayers;
//        //SerializedProperty MixBlackColor;

//        //SerializedProperty EnableDepthBlur;
//        //SerializedProperty DepthBlurShader;
//        //SerializedProperty DepthBlurCutoff;
//        //SerializedProperty DepthBlurIterations;
//        //SerializedProperty DepthBlurSurfacePower;
//        //SerializedProperty DepthBlurHorizontalMultiplier;
//        //SerializedProperty DepthBlurVerticalMultiplier;

//        SerializedProperty Shades;

//        //SerializedProperty EnableSimpleDepth;
//        //SerializedProperty SimpleDepthCutoff;

//        //SerializedProperty EnableFinalShader;
//        //SerializedProperty FinalShader;
//        //SerializedProperty EnableARMode;
//        //SerializedProperty Iterations;
//        //SerializedProperty NeighbourPixels;
//        //SerializedProperty BlockSize;

//        //SerializedProperty EnableRefraction;
//        //SerializedProperty RefractionTexture;
//        //SerializedProperty Refraction;

//        //SerializedProperty EnableWaves;
//        //SerializedProperty WaveNoiseTexture;
//        //SerializedProperty WaveSize;
//        //SerializedProperty WaveDistortion;
//        //SerializedProperty WaveSpeed;

//        //SerializedProperty EnableRipples;
//        //SerializedProperty RippleTexture;
//        //SerializedProperty RippleSize;
//        //SerializedProperty RippleRefraction;
//        //SerializedProperty RippleDensity;
//        //SerializedProperty RippleSpeed;

//        //SerializedProperty EnableMask;
//        //SerializedProperty MaskTexture;
//        //SerializedProperty MaskTiling;
//        //SerializedProperty MaskCutoff;
//        //SerializedProperty MaskEdgeDarkness;

//        SerializedProperty FoamRampTex;
//        SerializedProperty FoamTex;
//        SerializedProperty SurfaceTex;
//        SerializedProperty WaterDepthVisibility;
//        SerializedProperty AbsorptionRamp;
//        SerializedProperty ScatterRamp;
//        SerializedProperty WaterShadow;
//        SerializedProperty UseCustomDepth;
//        //SerializedProperty ManualDistanceForSurface;
//        //SerializedProperty ManualDistanceForDepth;
//        SerializedProperty WaveDetail;
//        SerializedProperty WaveRefraction;
//        SerializedProperty WaveCount;
//        SerializedProperty WaveAmplitude;
//        SerializedProperty WaveDirection;
//        SerializedProperty WaveLength;
//        //SerializedProperty DepthDensity;
//        //SerializedProperty DistanceDensity;
//        //SerializedProperty WaveNormalMap;
//        //SerializedProperty WaveNormalScale;
//        //SerializedProperty WaveNormalSpeed;
//        //SerializedProperty ShallowColor;
//        //SerializedProperty DeepColor;
//        //SerializedProperty FarColor;
//        //SerializedProperty ReflectionContribution;
//        //SerializedProperty SSSColor;
//        //SerializedProperty FoamContribution;
//        //SerializedProperty FoamTexture;
//        //SerializedProperty FoamScale;
//        //SerializedProperty FoamSpeed;
//        //SerializedProperty FoamNoiseScale;
//        //SerializedProperty SunSpecularColor;
//        //SerializedProperty SunSpecularExponent;
//        //SerializedProperty SparklesNormalMap;
//        //SerializedProperty SparkleScale;
//        //SerializedProperty SparkleSpeed;
//        //SerializedProperty SparkleColor;
//        //SerializedProperty SparkleExponent;
//        //SerializedProperty EdgeFoamColor;
//        //SerializedProperty EdgeFoamTexture;
//        //SerializedProperty EdgeFoamDepth;
//        //SerializedProperty FancyShadows;
//        //SerializedProperty MaxShadowDistance; 
//        //SerializedProperty ShadowColor;
//        //SerializedProperty Wave1Direction;
//        //SerializedProperty Wave1Amplitude;
//        //SerializedProperty Wave1Wavelength;
//        //SerializedProperty Wave1Speed;
//        //SerializedProperty Wave2Direction;
//        //SerializedProperty Wave2Amplitude;
//        //SerializedProperty Wave2Wavelength;
//        //SerializedProperty Wave2Speed;


//        string space = "          ";
//        GUIStyle headerStyle;
//        //int headerSpace = 5;

//        void OnEnable()
//        {
//            headerStyle = new GUIStyle { fontStyle = FontStyle.Bold };

//#if DEBUG_RENDER
//            DebugImage = serializedObject.FindProperty("DebugImage");
//            DebugImageDepth = serializedObject.FindProperty("DebugImageDepth");
//#endif
//            IsEnabled = serializedObject.FindProperty("IsEnabled");
//            Name = serializedObject.FindProperty("Name");
//            TurnOffOcclusion = serializedObject.FindProperty("TurnOffOcclusion");
//            //IsMirrorInMirror = serializedObject.FindProperty("IsMirrorInMirror");
//            //MirrorInMirrorId = serializedObject.FindProperty("MirrorInMirrorId");

//            DeviceType = serializedObject.FindProperty("DeviceType");

//            WorkType = serializedObject.FindProperty("WorkType");
//            UpVector = serializedObject.FindProperty("UpVector");
//            Intensity = serializedObject.FindProperty("Intensity");
//            //DisableGBuffer = serializedObject.FindProperty("DisableGBuffer");
//            RunForEveryXthFrame = serializedObject.FindProperty("RunForEveryXthFrame");
//            CameraLODLevel = serializedObject.FindProperty("CameraLODLevel");

//            HDR = serializedObject.FindProperty("HDR");
//            MSAA = serializedObject.FindProperty("MSAA");
//            DisablePixelLights = serializedObject.FindProperty("DisablePixelLights");
//            Shadow = serializedObject.FindProperty("Shadow");
//            Cull = serializedObject.FindProperty("Cull");
//            CullDistance = serializedObject.FindProperty("CullDistance");
//            TextureSize = serializedObject.FindProperty("TextureSize");
//            ManualSize = serializedObject.FindProperty("ManualSize");
//            UseCameraClipPlane = serializedObject.FindProperty("UseCameraClipPlane");
//            ClipPlaneOffset = serializedObject.FindProperty("ClipPlaneOffset");
//            ReflectLayers = serializedObject.FindProperty("ReflectLayers");
//            RefractLayers = serializedObject.FindProperty("DirectLayers");
//            //MixBlackColor = serializedObject.FindProperty("MixBlackColor");

//            //EnableDepthBlur = serializedObject.FindProperty("EnableDepthBlur");
//            //DepthBlurShader = serializedObject.FindProperty("DepthBlurShader");
//            //DepthBlurCutoff = serializedObject.FindProperty("DepthBlurCutoff");
//            //DepthBlurIterations = serializedObject.FindProperty("DepthBlurIterations");
//            //DepthBlurSurfacePower = serializedObject.FindProperty("DepthBlurSurfacePower");
//            //DepthBlurHorizontalMultiplier = serializedObject.FindProperty("DepthBlurHorizontalMultiplier");
//            //DepthBlurVerticalMultiplier = serializedObject.FindProperty("DepthBlurVerticalMultiplier");

//            Shades = serializedObject.FindProperty("Shades");

//            //EnableSimpleDepth = serializedObject.FindProperty("EnableSimpleDepth");
//            //SimpleDepthCutoff = serializedObject.FindProperty("SimpleDepthCutoff");

//            //EnableFinalShader = serializedObject.FindProperty("EnableFinalShader");
//            //FinalShader = serializedObject.FindProperty("FinalShader");
//            //EnableARMode = serializedObject.FindProperty("EnableARMode");
//            //Iterations = serializedObject.FindProperty("Iterations");
//            //NeighbourPixels = serializedObject.FindProperty("NeighbourPixels");
//            //BlockSize = serializedObject.FindProperty("BlockSize");

//            //EnableRefraction = serializedObject.FindProperty("EnableRefraction");
//            //RefractionTexture = serializedObject.FindProperty("RefractionTexture");
//            //Refraction = serializedObject.FindProperty("Refraction");

//            //EnableWaves = serializedObject.FindProperty("EnableWaves");
//            //WaveNoiseTexture = serializedObject.FindProperty("WaveNoiseTexture");
//            //WaveSize = serializedObject.FindProperty("WaveSize");
//            //WaveDistortion = serializedObject.FindProperty("WaveDistortion");
//            //WaveSpeed = serializedObject.FindProperty("WaveSpeed");

//            //EnableRipples = serializedObject.FindProperty("EnableRipples");
//            //RippleTexture = serializedObject.FindProperty("RippleTexture");
//            //RippleSize = serializedObject.FindProperty("RippleSize");
//            //RippleRefraction = serializedObject.FindProperty("RippleRefraction");
//            //RippleDensity = serializedObject.FindProperty("RippleDensity");
//            //RippleSpeed = serializedObject.FindProperty("RippleSpeed");

//            //EnableMask = serializedObject.FindProperty("EnableMask");
//            //MaskTexture = serializedObject.FindProperty("MaskTexture");
//            //MaskTiling = serializedObject.FindProperty("MaskTiling");
//            //MaskCutoff = serializedObject.FindProperty("MaskCutoff");
//            //MaskEdgeDarkness = serializedObject.FindProperty("MaskEdgeDarkness");

//            FoamRampTex = serializedObject.FindProperty("FoamRampTex");
//            FoamTex = serializedObject.FindProperty("FoamTex");
//            SurfaceTex = serializedObject.FindProperty("SurfaceTex");
//            WaterDepthVisibility = serializedObject.FindProperty("WaterDepthVisibility");
//            AbsorptionRamp = serializedObject.FindProperty("AbsorptionRamp");
//            ScatterRamp = serializedObject.FindProperty("ScatterRamp");
//            WaterShadow = serializedObject.FindProperty("WaterShadow");
//            UseCustomDepth = serializedObject.FindProperty("UseCustomDepth");
//            //ManualDistanceForSurface = serializedObject.FindProperty("ManualDistanceForSurface");
//            //ManualDistanceForDepth = serializedObject.FindProperty("ManualDistanceForDepth");
//            WaveDetail = serializedObject.FindProperty("WaveDetail");
//            WaveRefraction = serializedObject.FindProperty("WaveRefraction");
//            WaveCount = serializedObject.FindProperty("BasicWaveSettings.numWaves");
//            WaveAmplitude = serializedObject.FindProperty("BasicWaveSettings.amplitude");
//            WaveDirection = serializedObject.FindProperty("BasicWaveSettings.direction");
//            WaveLength = serializedObject.FindProperty("BasicWaveSettings.wavelength");
//        }

//        public override void OnInspectorGUI()
//        {
//            serializedObject.Update();

//#if DEBUG_RENDER
//            EditorGUILayout.PropertyField(DebugImage);
//            EditorGUILayout.PropertyField(DebugImageDepth);
//#endif
//            EditorGUILayout.PropertyField(IsEnabled);
//            EditorGUILayout.PropertyField(Name);
//            EditorGUILayout.PropertyField(TurnOffOcclusion);
//            //EditorGUILayout.PropertyField(IsMirrorInMirror);
//            //if (IsMirrorInMirror.boolValue)
//            //{
//            //    EditorGUILayout.PropertyField(MirrorInMirrorId, new GUIContent { text = space + "Id" });
//            //}



//            EditorGUILayout.Space();
//            EditorGUILayout.LabelField(new GUIContent { text = "Device Type (AR, VR, XR)" }, headerStyle);
//            EditorGUILayout.PropertyField(DeviceType);



//            EditorGUILayout.Space();
//            EditorGUILayout.LabelField(new GUIContent { text = "Affected Objects and Materials" }, headerStyle);
//            EditorGUILayout.PropertyField(Shades);



//            EditorGUILayout.Space();
//            MenuCommon = EditorGUILayout.BeginFoldoutHeaderGroup(MenuCommon, new GUIContent { text = "Common" });
//            if (MenuCommon)
//            {
//                EditorGUILayout.PropertyField(WorkType);
//                EditorGUILayout.PropertyField(UpVector);
//                EditorGUILayout.PropertyField(Intensity);
//                //EditorGUILayout.PropertyField(DisableGBuffer);
//                EditorGUILayout.PropertyField(RunForEveryXthFrame);
//                EditorGUILayout.PropertyField(CameraLODLevel);
//            }
//            EditorGUILayout.EndFoldoutHeaderGroup();



//            EditorGUILayout.Space();
//            MenuCamera = EditorGUILayout.BeginFoldoutHeaderGroup(MenuCamera, new GUIContent { text = "Camera" });
//            if (MenuCamera)
//            {
//                EditorGUILayout.PropertyField(HDR);
//                EditorGUILayout.PropertyField(MSAA);
//                EditorGUILayout.PropertyField(DisablePixelLights);
//                EditorGUILayout.PropertyField(Shadow);
//                EditorGUILayout.PropertyField(Cull);
//                if (Cull.boolValue)
//                {
//                    EditorGUILayout.PropertyField(CullDistance, new GUIContent { text = space + "Distance" });
//                }
//                EditorGUILayout.PropertyField(TextureSize);
//                if (TextureSize.intValue == (int)TextureSizeType.Manual)
//                {
//                    EditorGUILayout.PropertyField(ManualSize, new GUIContent { text = space + "Size" });
//                }
//                EditorGUILayout.PropertyField(UseCameraClipPlane);
//                EditorGUILayout.PropertyField(ClipPlaneOffset);
//                EditorGUILayout.PropertyField(ReflectLayers);
//                EditorGUILayout.PropertyField(RefractLayers);
//                //EditorGUILayout.PropertyField(MixBlackColor);
//            }
//            EditorGUILayout.EndFoldoutHeaderGroup();



//            //EditorGUILayout.Space();
//            //EditorGUILayout.LabelField(new GUIContent { text = "Refraction" }, headerStyle);

//            //EditorGUILayout.PropertyField(EnableRefraction, new GUIContent { text = "Enable" });
//            //if (EnableRefraction.boolValue)
//            //{
//            //    EditorGUILayout.PropertyField(RefractionTexture, new GUIContent { text = space + "Texture" });
//            //    EditorGUILayout.PropertyField(Refraction, new GUIContent { text = space + "Density" });
//            //}



//            //EditorGUILayout.Space();
//            //EditorGUILayout.LabelField(new GUIContent { text = "Depth Blur" }, headerStyle);

//            //EditorGUILayout.PropertyField(EnableDepthBlur, new GUIContent { text = "Enable" });
//            //if (EnableDepthBlur.boolValue)
//            //{
//            //    EditorGUILayout.PropertyField(DepthBlurShader, new GUIContent { text = space + "Shader" });
//            //    EditorGUILayout.PropertyField(DepthBlurCutoff, new GUIContent { text = space + "Cutoff" });
//            //    EditorGUILayout.PropertyField(DepthBlurIterations, new GUIContent { text = space + "Iterations" });
//            //    EditorGUILayout.PropertyField(DepthBlurSurfacePower, new GUIContent { text = space + "Surface Power" });
//            //    EditorGUILayout.PropertyField(DepthBlurHorizontalMultiplier, new GUIContent { text = space + "Horizontal Multiplier" });
//            //    EditorGUILayout.PropertyField(DepthBlurVerticalMultiplier, new GUIContent { text = space + "Vertical Multiplier" });
//            //}



//            //EditorGUILayout.Space();
//            //EditorGUILayout.LabelField(new GUIContent { text = "Simple Depth" }, headerStyle);

//            //EditorGUILayout.PropertyField(EnableSimpleDepth, new GUIContent { text = "Enable" });
//            //if (EnableSimpleDepth.boolValue)
//            //{
//            //    EditorGUILayout.PropertyField(SimpleDepthCutoff, new GUIContent { text = space + "Cutoff" });
//            //}



//            //EditorGUILayout.Space();
//            //EditorGUILayout.LabelField(new GUIContent { text = "Shader to Run on Final Texture" }, headerStyle);

//            //EditorGUILayout.PropertyField(EnableFinalShader, new GUIContent { text = "Enable" });
//            //if (EnableFinalShader.boolValue)
//            //{
//            //    EditorGUILayout.PropertyField(FinalShader, new GUIContent { text = space + "Shader" });
//            //    EditorGUILayout.PropertyField(EnableARMode, new GUIContent { text = space + "Enable AR Mode" });
//            //    EditorGUILayout.PropertyField(Iterations, new GUIContent { text = space + "Iterations" });
//            //    EditorGUILayout.PropertyField(NeighbourPixels, new GUIContent { text = space + "Pixel Count" });
//            //    EditorGUILayout.PropertyField(BlockSize, new GUIContent { text = space + "Block Size" });
//            //}



//            //EditorGUILayout.Space();
//            //EditorGUILayout.LabelField(new GUIContent { text = "Waves" }, headerStyle);

//            //EditorGUILayout.PropertyField(EnableWaves, new GUIContent { text = "Enable" });
//            //if (EnableWaves.boolValue)
//            //{
//            //    EditorGUILayout.PropertyField(WaveNoiseTexture, new GUIContent { text = space + "Noise Texture" });
//            //    EditorGUILayout.PropertyField(WaveSize, new GUIContent { text = space + "Size" });
//            //    EditorGUILayout.PropertyField(WaveDistortion, new GUIContent { text = space + "Distortion" });
//            //    EditorGUILayout.PropertyField(WaveSpeed, new GUIContent { text = space + "Speed" });
//            //}



//            //EditorGUILayout.Space();
//            //EditorGUILayout.LabelField(new GUIContent { text = "Ripples" }, headerStyle);

//            //EditorGUILayout.PropertyField(EnableRipples, new GUIContent { text = "Enable" });
//            //if (EnableRipples.boolValue)
//            //{
//            //    EditorGUILayout.PropertyField(RippleTexture, new GUIContent { text = space + "Texture" });
//            //    EditorGUILayout.PropertyField(RippleSize, new GUIContent { text = space + "Size" });
//            //    EditorGUILayout.PropertyField(RippleRefraction, new GUIContent { text = space + "Refraction" });
//            //    EditorGUILayout.PropertyField(RippleDensity, new GUIContent { text = space + "Density" });
//            //    EditorGUILayout.PropertyField(RippleSpeed, new GUIContent { text = space + "Speed" });
//            //}



//            //EditorGUILayout.Space();
//            //EditorGUILayout.LabelField(new GUIContent { text = "Mask" }, headerStyle);

//            //EditorGUILayout.PropertyField(EnableMask, new GUIContent { text = "Enable" });
//            //if (EnableMask.boolValue)
//            //{
//            //    EditorGUILayout.PropertyField(MaskTexture, new GUIContent { text = space + "Texture" });
//            //    EditorGUILayout.PropertyField(MaskTiling, new GUIContent { text = space + "Tiling" });
//            //    EditorGUILayout.PropertyField(MaskCutoff, new GUIContent { text = space + "Cutoff" });
//            //    EditorGUILayout.PropertyField(MaskEdgeDarkness, new GUIContent { text = space + "Edge Darkness" });
//            //}



//            EditorGUILayout.Space();
//            MenuWater = EditorGUILayout.BeginFoldoutHeaderGroup(MenuWater, new GUIContent { text = "Water" });
//            if (MenuWater)
//            {
//                EditorGUILayout.PropertyField(FoamRampTex, new GUIContent { text = "Ramp Tex" });
//                EditorGUILayout.PropertyField(SurfaceTex, new GUIContent { text = "Surface Tex" });
//                EditorGUILayout.PropertyField(FoamTex, new GUIContent { text = "Foam Tex" });
//                EditorGUILayout.PropertyField(WaterDepthVisibility, new GUIContent { text = "Depth Visibility" });
//                EditorGUILayout.PropertyField(AbsorptionRamp, new GUIContent { text = "Absorption" });
//                EditorGUILayout.PropertyField(ScatterRamp, new GUIContent { text = "Scatter" });
//                EditorGUILayout.PropertyField(WaterShadow, new GUIContent { text = "Surface Shadows" });
//                EditorGUILayout.PropertyField(UseCustomDepth, new GUIContent { text = "Use Custom Depth" });
//                //if (DoNotUseDepth.boolValue)
//                //{
//                //    EditorGUILayout.PropertyField(ManualDistanceForSurface, new GUIContent { text = space + "Distance for Surface" });
//                //    EditorGUILayout.PropertyField(ManualDistanceForDepth, new GUIContent { text = space + "Distance for Depth" });
//                //}
//                EditorGUILayout.PropertyField(WaveDetail, new GUIContent { text = "Wave Detail" });
//                EditorGUILayout.PropertyField(WaveRefraction, new GUIContent { text = "Refraction" });
//                EditorGUILayout.PropertyField(WaveCount, new GUIContent { text = "Wave Count" });
//                EditorGUILayout.PropertyField(WaveAmplitude, new GUIContent { text = "Wave Amplitude" });
//                EditorGUILayout.PropertyField(WaveDirection, new GUIContent { text = "Wave Direction" });
//                EditorGUILayout.PropertyField(WaveLength, new GUIContent { text = "Wave Length" });
//            }
//            EditorGUILayout.EndFoldoutHeaderGroup();


//            serializedObject.ApplyModifiedProperties();
//        }

//    }

//}

//#endif