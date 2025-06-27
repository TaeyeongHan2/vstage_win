//#if _AKILLIMUM_WATER

////#define DEBUG_RENDER
////#define DEBUG_LOG

//using System;
//using UnityEngine;
//using UnityEngine.Rendering;
//using UnityEngine.Experimental.Rendering;
//using UnityEngine.Rendering.Universal;
//using Object = UnityEngine.Object;
//using Random = UnityEngine.Random;
//using System.Collections.Generic;
//using Unity.Mathematics;
//using System.Reflection;


//namespace AkilliMum.SRP.Mirror
//{
//        [ExecuteAlways]
//#if UNITY_2019_1_OR_NEWER
//        public class CameraShadeWater : CameraShade
//#else
//        public class CameraShadeWater : CameraShade, IBeforeCameraRender
//#endif
//    {

//        //// Singleton
//        //private static CameraShadeWaterTest _instance;
//        //public static CameraShadeWaterTest Instance
//        //{
//        //    get
//        //    {
//        //        if (_instance == null)
//        //            _instance = (CameraShadeWaterTest)FindObjectOfType(typeof(CameraShadeWaterTest));
//        //        return _instance;
//        //    }
//        //}
//        private GerstnerWavesJobsInstance _gerstnerWavesJobsInstance;

//        [Tooltip("Ramp Tex to define how the color changes with light and view angle.")]
//        public Texture2D FoamRampTex;
//        [Tooltip("Foam Tex for the water foams. It adds foams to the water according to wave refraction value.")]
//        public Texture2D FoamTex;
//        [Tooltip("Surface Tex to define surface normals.")]
//        public Texture2D SurfaceTex;
//        // Script references
//        //private PlanarReflections _planarReflections;

//        //private bool _useComputeBuffer;
//        //public bool computeOverride;

//        [Tooltip("Enables / disables water surface shadows.")]
//        public bool WaterShadow = false;

//        [Tooltip("Use custom depth for water. Because some devices can not use depth (even enabled on project settings like Oculus-Quest) it creates depth values via real camera.")]
//        public bool UseCustomDepth = false;
//        //[Tooltip("Manual distance to draw the water. Big values should mix the water color nicely with large distance. Most of the time you do not have to change this.")]
//        //public int ManualDistanceForSurface = 100000000;
//        //[Tooltip("Manual distance to mix the opaque texture with water. Just adjust it until you get the desired look.")]
//        //public int ManualDistanceForDepth = 500;

//        private RenderTexture _depthTex;
//        //public Texture bakedDepthTex;
//        private Camera _depthCam;
//        private Texture2D _rampTexture;

//        //[SerializeField]
//        private Wave[] _waves;

//        //[SerializeField]
//        //private ComputeBuffer waveBuffer;
//        private float _maxWaveHeight;
//        private float _waveHeight;

//        //[SerializeField]
//        //public WaterSettingsData settingsData;
//        //[SerializeField]
//        //public WaterSurfaceData surfaceData;
//        //[SerializeField]
//        //private WaterResources resources;

//        public float WaterDepthVisibility = 40.0f;
//        private float _waterDepthVisibility = 40.0f;

//        [Tooltip("Gradient to define absorption according to light and view angle.")]
//        public Gradient AbsorptionRamp;
//        private Gradient _absorptionRamp = new Gradient();

//        [Tooltip("Gradient to define scattering according to light and view angle.")]
//        public Gradient ScatterRamp;
//        private Gradient _scatterRamp = new Gradient();

//        [Tooltip("Changes the detail on waves, it adds more bump to the waves.")]
//        [Range(0f,2)]
//        public float WaveDetail = 0.2f;
//        [Tooltip("Changes refraction for reflection and under water items together.")]
//        [Range(0f, 10)]
//        public float WaveRefraction = 1.0f;

//        //public List<Wave> _waves = new List<Wave>();
//        private bool _customWaves = false;
//        private int randomSeed = 3234;

//        public BasicWaves BasicWaveSettings = new BasicWaves(1.5f, 45.0f, 5.0f);
//        private BasicWaves _basicWaveSettings = new BasicWaves(1.5f, 45.0f, 5.0f);

//        private FoamSettings _foamSettings = new FoamSettings();
//        //[SerializeField]
//        //private bool _init = false;

//        private static readonly int CameraRoll = Shader.PropertyToID("_CameraRoll");
//        private static readonly int InvViewProjection = Shader.PropertyToID("_InvViewProjection");
//        private static readonly int WaterDepthMap = Shader.PropertyToID("_WaterDepthMap");
//        private static readonly int FoamMap = Shader.PropertyToID("_FoamMap");
//        private static readonly int SurfaceMap = Shader.PropertyToID("_SurfaceMap");
//        private static readonly int WaveHeight = Shader.PropertyToID("_WaveHeight");
//        private static readonly int MaxWaveHeight = Shader.PropertyToID("_MaxWaveHeight");
//        private static readonly int MaxDepth = Shader.PropertyToID("_MaxDepth");
//        private static readonly int WaveCount = Shader.PropertyToID("_WaveCount");
//        private static readonly int CubemapTexture = Shader.PropertyToID("_CubemapTexture");
//        private static readonly int WaveDataBuffer = Shader.PropertyToID("_WaveDataBuffer");
//        private static readonly int WaveData = Shader.PropertyToID("waveData");
//        private static readonly int AbsorptionScatteringRamp = Shader.PropertyToID("_AbsorptionScatteringRamp");
//        private static readonly int DepthCamZParams = Shader.PropertyToID("_Water_DepthCamParams");

//        bool MapsAreAssigned()
//        {
//            if (!FoamRampTex || !FoamTex || !SurfaceTex)
//                return false;
//            return true;
//        }

//#if UNITY_2019_1_OR_NEWER
//        public override void CommonRender(ScriptableRenderContext context, Camera cameraSrp)
//        {
//#else
//        public override void CommonRender(LightweightRenderPipeline pipelineInstance, ScriptableRenderContext context, Camera cameraSrp)
//        {
//#endif
//            if (!MapsAreAssigned())
//                return;

//            var roll = cameraSrp.transform.localEulerAngles.z;
//            Shader.SetGlobalFloat(CameraRoll, roll);
//            Shader.SetGlobalMatrix(InvViewProjection,
//                (GL.GetGPUProjectionMatrix(cameraSrp.projectionMatrix, false) * cameraSrp.worldToCameraMatrix).inverse);

//            //// Water matrix
//            //const float quantizeValue = 6.25f;
//            //const float forwards = 10f;
//            //const float yOffset = -0.25f;

//            //var newPos = cameraSrp.transform.TransformPoint(Vector3.forward * forwards);
//            //newPos.y = yOffset;
//            //newPos.x = quantizeValue * (int)(newPos.x / quantizeValue);
//            //newPos.z = quantizeValue * (int)(newPos.z / quantizeValue);

//            //var matrix = Matrix4x4.TRS(newPos + transform.position, Quaternion.identity, Vector3.one); // transform.localToWorldMatrix;
//            //Debug.Log("draw mesh");
//            //foreach (var mesh in defaultWaterMeshes)
//            //{
//            //    Graphics.DrawMesh(mesh,
//            //        matrix,
//            //        defaultSeaMaterial,
//            //        gameObject.layer,
//            //        cameraSrp,
//            //        0,
//            //        null,
//            //        ShadowCastingMode.Off,
//            //        true,
//            //        null,
//            //        LightProbeUsage.Off,
//            //        null);
//            //}

//            //reinitalize the waves etc. if any value changes
//            if (_waterDepthVisibility != WaterDepthVisibility ||
//                !_absorptionRamp.Equals(AbsorptionRamp) ||
//                !_scatterRamp.Equals(ScatterRamp) ||
//                _basicWaveSettings.amplitude != BasicWaveSettings.amplitude ||
//                _basicWaveSettings.direction != BasicWaveSettings.direction ||
//                _basicWaveSettings.numWaves != BasicWaveSettings.numWaves ||
//                _basicWaveSettings.wavelength != BasicWaveSettings.wavelength)
//            {
//                Debug.Log("Water waves init called again...");

//                _waterDepthVisibility = WaterDepthVisibility;

//                _absorptionRamp = new Gradient
//                {
//                    alphaKeys = AbsorptionRamp.alphaKeys,
//                    colorKeys = AbsorptionRamp.colorKeys,
//                    mode = AbsorptionRamp.mode
//                };
//                _scatterRamp = new Gradient
//                {
//                    alphaKeys = ScatterRamp.alphaKeys,
//                    colorKeys = ScatterRamp.colorKeys,
//                    mode = ScatterRamp.mode
//                };

//                _basicWaveSettings = new BasicWaves(BasicWaveSettings.amplitude, BasicWaveSettings.direction,
//                    BasicWaveSettings.wavelength);
//                _basicWaveSettings.numWaves = BasicWaveSettings.numWaves;

//                Init();
//            }

//            foreach (var shade in Shades)
//            {
//                if (shade.MaterialToChange.HasProperty("_BumpScale"))
//                    shade.MaterialToChange.SetFloat("_BumpScale", WaveDetail);

//                if (shade.MaterialToChange.HasProperty("_WaveRefraction"))
//                    shade.MaterialToChange.SetFloat("_WaveRefraction", WaveRefraction);

//                if (WaterShadow)
//                {
//                    shade.MaterialToChange.DisableKeyword("_RECEIVE_SHADOWS_OFF");
//                }
//                else
//                {
//                    shade.MaterialToChange.EnableKeyword("_RECEIVE_SHADOWS_OFF");
//                }

//                if (UseCustomDepth)
//                {
//                    shade.MaterialToChange.EnableKeyword("DO_NOT_USE_DEPTH");
//                }
//                else
//                {
//                    shade.MaterialToChange.DisableKeyword("DO_NOT_USE_DEPTH");
//                }

//                //if (shade.MaterialToChange.HasProperty("_ManualDistanceSurface"))
//                //    shade.MaterialToChange.SetFloat("_ManualDistanceSurface", ManualDistanceForSurface);

//                //if (shade.MaterialToChange.HasProperty("_ManualDistanceDepth"))
//                //    shade.MaterialToChange.SetFloat("_ManualDistanceDepth", ManualDistanceForDepth);
//            }
//        }

//        public Wave[] GetWaves()
//        {
//            return _waves;
//        }

//        protected override void OnEnable()
//        {
//            base.OnEnable(); //!!!!!!!

//            if (!MapsAreAssigned())
//                return;

//            _gerstnerWavesJobsInstance = new GerstnerWavesJobsInstance();

//            //if (resources == null)
//            //{
//            //    Debug.Log("called Resources.Load(WaterResources) 1");
//            //    resources = Resources.Load("WaterResources") as WaterResources;
//            //}

//            //if (!computeOverride)
//            //    _useComputeBuffer = SystemInfo.supportsComputeShaders &&
//            //                       Application.platform != RuntimePlatform.WebGLPlayer &&
//            //                       Application.platform != RuntimePlatform.Android;
//            //else
//            //_useComputeBuffer = false;

//            Init();

//            //RenderPipelineManager.beginCameraRendering += BeginCameraRendering;


//            //add a water pass to camera for depth etc. for vr
//            if (Application.isPlaying &&
//                UseCustomDepth)
//            {
//                var waterPass = gameObject.AddComponent<CameraShade>();
                
//                Extensions.SimpleCopy(this, waterPass); //copy same properties

//                waterPass.WorkType = WorkType.Water; //change to water for depth etc.

//                waterPass.InitializeProperties(); //initialize for object and material

//                //move debug textures if any on current element (so waterPass will use them)
//#if DEBUG_RENDER
//                DebugImage = null;
//                DebugImageDepth = null;
//#endif
//            }
//        }

//        public GerstnerWavesJobsInstance GetGerstnerWavesJobsInstance()
//        {
//            return _gerstnerWavesJobsInstance;
//        }

//        protected override void OnDisable()
//        {
//            base.OnDisable();

//            Cleanup();
//        }

//        void Cleanup()
//        {
//            if(Application.isPlaying && _gerstnerWavesJobsInstance!=null)
//                _gerstnerWavesJobsInstance.Cleanup();

//            //RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;

//            if (_depthCam)
//            {
//                _depthCam.targetTexture = null;
//                SafeDestroy(_depthCam.gameObject);
//            }
//            if (_depthTex)
//            {
//                SafeDestroy(_depthTex);
//            }

//            //waveBuffer?.Dispose();
//        }

//        private static void SafeDestroy(Object o)
//        {
//            if (Application.isPlaying)
//                Destroy(o);
//            else
//                DestroyImmediate(o);
//        }

//        public void Init()
//        {
//            SetWaves();
//            GenerateColorRamp();

//            //if (bakedDepthTex)
//            //{
//            //    Shader.SetGlobalTexture(WaterDepthMap, bakedDepthTex);
//            //}

//            //if (!gameObject.TryGetComponent(out _planarReflections))
//            //{
//            //    _planarReflections = gameObject.AddComponent<PlanarReflections>();
//            //}
//            //_planarReflections.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
//            //_planarReflections.m_settings = settingsData.planarSettings;
//            //_planarReflections.enabled = settingsData.refType == ReflectionType.PlanarReflection;

//            //if (resources == null)
//            //{

//            //    Debug.Log("called Resources.Load(WaterResources) 2");
//            //    resources = Resources.Load("WaterResources") as WaterResources;
//            //}

//            Invoke(nameof(CaptureDepthMap), 1.0f);
//        }

//        private void LateUpdate()
//        {
//            if (!MapsAreAssigned())
//                return;

//            _gerstnerWavesJobsInstance.UpdateHeights();
//        }

//        public void FragWaveNormals(bool toggle)
//        {
//            var mat = GetComponent<Renderer>().sharedMaterial;
//            if (toggle)
//                mat.EnableKeyword("GERSTNER_WAVES");
//            else
//                mat.DisableKeyword("GERSTNER_WAVES");
//        }

//        private void SetWaves()
//        {
//            //SetupWaves(surfaceData._customWaves);
//            SetupWaves(_customWaves);

//            // set default resources
//            Shader.SetGlobalTexture(FoamMap, FoamTex);
//            Shader.SetGlobalTexture(SurfaceMap, SurfaceTex);

//            _maxWaveHeight = 0f;
//            foreach (var w in _waves)
//            {
//                _maxWaveHeight += w.amplitude;
//            }
//            _maxWaveHeight /= _waves.Length;

//            _waveHeight = transform.position.y;

//            Shader.SetGlobalFloat(WaveHeight, _waveHeight);
//            Shader.SetGlobalFloat(MaxWaveHeight, _maxWaveHeight);
//            //Shader.SetGlobalFloat(MaxDepth, surfaceData._waterMaxVisibility);
//            Shader.SetGlobalFloat(MaxDepth, WaterDepthVisibility);

//            //switch (settingsData.refType)
//            //{
//            //    case ReflectionType.Cubemap:
//            //        Shader.EnableKeyword("_REFLECTION_CUBEMAP");
//            //        Shader.DisableKeyword("_REFLECTION_PROBES");
//            //        Shader.DisableKeyword("_REFLECTION_PLANARREFLECTION");
//            //        Shader.SetGlobalTexture(CubemapTexture, settingsData.cubemapRefType);
//            //        break;
//            //    case ReflectionType.ReflectionProbe:
//            //        Shader.DisableKeyword("_REFLECTION_CUBEMAP");
//            //        Shader.EnableKeyword("_REFLECTION_PROBES");
//            //        Shader.DisableKeyword("_REFLECTION_PLANARREFLECTION");
//            //        break;
//            //    case ReflectionType.PlanarReflection:
//            //        Shader.DisableKeyword("_REFLECTION_CUBEMAP");
//            //        Shader.DisableKeyword("_REFLECTION_PROBES");
//            //        Shader.EnableKeyword("_REFLECTION_PLANARREFLECTION");
//            //        break;
//            //    default:
//            //        throw new ArgumentOutOfRangeException();
//            //}

//            Shader.SetGlobalInt(WaveCount, _waves.Length);

//            //GPU side
//            //if (_useComputeBuffer)
//            //{
//            //    Shader.EnableKeyword("USE_STRUCTURED_BUFFER");
//            //    if (waveBuffer == null)
//            //        waveBuffer = new ComputeBuffer(10, (sizeof(float) * 6));
//            //    waveBuffer.SetData(_waves);
//            //    Shader.SetGlobalBuffer(WaveDataBuffer, waveBuffer);
//            //}
//            //else
//            //{
//                Shader.DisableKeyword("USE_STRUCTURED_BUFFER");
//                Shader.SetGlobalVectorArray(WaveData, GetWaveData());
//            //}

//            //CPU side
//            if (_gerstnerWavesJobsInstance.Initialized == false && Application.isPlaying)
//                _gerstnerWavesJobsInstance.Init(this);
//        }

//        private Vector4[] GetWaveData()
//        {
//            var waveData = new Vector4[20];
//            for (var i = 0; i < _waves.Length; i++)
//            {
//                waveData[i] = new Vector4(_waves[i].amplitude, _waves[i].direction, _waves[i].wavelength, _waves[i].onmiDir);
//                waveData[i + 10] = new Vector4(_waves[i].origin.x, _waves[i].origin.y, 0, 0);
//            }
//            return waveData;
//        }

//        private void SetupWaves(bool custom)
//        {
//            //if (!custom)
//            //{
//                //create basic waves based off basic wave settings
//                var backupSeed = Random.state;
//                //Random.InitState(surfaceData.randomSeed);
//                Random.InitState(randomSeed);
//                //var basicWaves = surfaceData._basicWaveSettings;
//                var basicWaves = BasicWaveSettings;
//                var a = basicWaves.amplitude;
//                var d = basicWaves.direction;
//                var l = basicWaves.wavelength;
//                var numWave = basicWaves.numWaves;
//                _waves = new Wave[numWave];

//                var r = 1f / numWave;

//                for (var i = 0; i < numWave; i++)
//                {
//                    var p = Mathf.Lerp(0.5f, 1.5f, i * r);
//                    var amp = a * p * Random.Range(0.8f, 1.2f);
//                    var dir = d + Random.Range(-90f, 90f);
//                    var len = l * p * Random.Range(0.6f, 1.4f);
//                    _waves[i] = new Wave(amp, dir, len, Vector2.zero, false);
//                    //Random.InitState(surfaceData.randomSeed + i + 1);
//                    Random.InitState(randomSeed + i + 1);
//                }
//                Random.state = backupSeed;
//            //}
//            //else
//            //{
//            //    _waves = surfaceData._waves.ToArray();
//            //}
//        }

//        private void GenerateColorRamp()
//        {
//            if (_rampTexture == null)
//                _rampTexture = new Texture2D(128, 4, GraphicsFormat.R8G8B8A8_SRGB, TextureCreationFlags.None);
//            _rampTexture.wrapMode = TextureWrapMode.Clamp;

//            //var defaultFoamRamp = defaultFoamRamp;

//            var cols = new Color[512];
//            for (var i = 0; i < 128; i++)
//            {
//                //cols[i] = surfaceData._absorptionRamp.Evaluate(i / 128f);
//                cols[i] = AbsorptionRamp.Evaluate(i / 128f);
//            }
//            for (var i = 0; i < 128; i++)
//            {
//                //cols[i + 128] = surfaceData._scatterRamp.Evaluate(i / 128f);
//                cols[i + 128] = ScatterRamp.Evaluate(i / 128f);
//            }
//            for (var i = 0; i < 128; i++)
//            {
//                //switch (surfaceData._foamSettings.foamType)
//                switch (_foamSettings.foamType)
//                {
//                    case 0: // default
//                        cols[i + 256] = FoamRampTex.GetPixelBilinear(i / 128f, 0.5f);
//                        break;
//                    case 1: // simple
//                        //cols[i + 256] = defaultFoamRamp.GetPixelBilinear(surfaceData._foamSettings.basicFoam.Evaluate(i / 128f), 0.5f);
//                        cols[i + 256] = FoamRampTex.GetPixelBilinear(_foamSettings.basicFoam.Evaluate(i / 128f), 0.5f);
//                        break;
//                    case 2: // custom
//                        cols[i + 256] = Color.black;
//                        break;
//                }
//            }
//            _rampTexture.SetPixels(cols);
//            _rampTexture.Apply();
//            Shader.SetGlobalTexture(AbsorptionScatteringRamp, _rampTexture);
//        }

//        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//        ////////////////////////////////////////Shoreline Depth Texture/////////////////////////////////////////////////
//        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//        [ContextMenu("Capture Depth")]
//        public void CaptureDepthMap()
//        {
//            //Generate the camera
//            if (_depthCam == null)
//            {
//                var go =
//                    new GameObject("depthCamera") { hideFlags = HideFlags.HideAndDontSave }; //create the cameraObject
//                _depthCam = go.AddComponent<Camera>();
//            }

//            var additionalCamData = _depthCam.GetUniversalAdditionalCameraData();
//            additionalCamData.renderShadows = false;
//            additionalCamData.requiresColorOption = CameraOverrideOption.Off;
//            additionalCamData.requiresDepthOption = CameraOverrideOption.Off;

//            var t = _depthCam.transform;
//            var depthExtra = 4.0f;
//            t.position = Vector3.up * (transform.position.y + depthExtra);//center the camera on this water plane height
//            t.up = Vector3.forward;//face the camera down

//            _depthCam.enabled = true;
//            _depthCam.orthographic = true;
//            _depthCam.orthographicSize = 250;//hardcoded = 1k area - TODO
//            _depthCam.nearClipPlane = 0.01f;
//            _depthCam.farClipPlane = WaterDepthVisibility + depthExtra;
//            //_depthCam.farClipPlane = surfaceData._waterMaxVisibility + depthExtra;
//            _depthCam.allowHDR = false;
//            _depthCam.allowMSAA = false;
//            _depthCam.cullingMask = (1 << 10);
//            //Generate RT
//            if (!_depthTex)
//                _depthTex = new RenderTexture(1024, 1024, 24, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
//            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
//            {
//                _depthTex.filterMode = FilterMode.Point;
//            }
//            _depthTex.wrapMode = TextureWrapMode.Clamp;
//            _depthTex.name = "WaterDepthMap";
//            //do depth capture
//            _depthCam.targetTexture = _depthTex;
//            _depthCam.Render();
//            Shader.SetGlobalTexture(WaterDepthMap, _depthTex);
//            // set depth bufferParams for depth cam(since it doesnt exist and only temporary)
//            var _params = new Vector4(t.position.y, 250, 0, 0);
//            //Vector4 zParams = new Vector4(1-f/n, f/n, (1-f/n)/f, (f/n)/f);//2015
//            Shader.SetGlobalVector(DepthCamZParams, _params);

//            //           #if UNITY_EDITOR
//            //            Texture2D tex2D = new Texture2D(1024, 1024, TextureFormat.Alpha8, false);
//            //            Graphics.CopyTexture(_depthTex, tex2D);
//            //            byte[] image = tex2D.EncodeToPNG();
//            //            System.IO.File.WriteAllBytes(Application.dataPath + "/WaterDepth.png", image);
//            //            #endif

//            _depthCam.enabled = false;
//            _depthCam.targetTexture = null;
//        }

//        [Serializable]
//        public enum DebugMode { none, stationary, screen };
//    }
//    [System.Serializable]
//    public struct Wave
//    {
//        public float amplitude; // height of the wave in units(m)
//        public float direction; // direction the wave travels in degrees from Z+
//        public float wavelength; // distance between crest>crest
//        public float2 origin; // Omi directional point of origin
//        public float onmiDir; // Is omni?

//        public Wave(float amp, float dir, float length, float2 org, bool omni)
//        {
//            amplitude = amp;
//            direction = dir;
//            wavelength = length;
//            origin = org;
//            onmiDir = omni ? 1 : 0;
//        }
//    }

//    [System.Serializable]
//    public class BasicWaves
//    {
//        [Tooltip("Changes the number of waves.")]
//        [Range(1,10)]
//        public int numWaves = 6;
//        [Tooltip("Changes the amplitude for the waves.")]
//        [Range(0.1f,30)]
//        public float amplitude;
//        [Tooltip("Changes the direction of the waves (through an axis).")]
//        public float direction;
//        [Tooltip("Changes the wave length for smoother or harder waves.")]
//        [Range(1, 200)]
//        public float wavelength;

//        public BasicWaves(float amp, float dir, float len)
//        {
//            numWaves = 6;
//            amplitude = amp;
//            direction = dir;
//            wavelength = len;
//        }
//    }

//    [System.Serializable]
//    public class FoamSettings
//    {
//        public int foamType; // 0=default, 1=simple, 3=custom
//        public AnimationCurve basicFoam;
//        public AnimationCurve liteFoam;
//        public AnimationCurve mediumFoam;
//        public AnimationCurve denseFoam;

//        // Foam curves
//        public FoamSettings()
//        {
//            foamType = 0;
//            basicFoam = new AnimationCurve(new Keyframe[2]{new Keyframe(0.25f, 0f),
//                                                                    new Keyframe(1f, 1f)});
//            liteFoam = new AnimationCurve(new Keyframe[3]{new Keyframe(0.2f, 0f),
//                                                                    new Keyframe(0.4f, 1f),
//                                                                    new Keyframe(0.7f, 0f)});
//            mediumFoam = new AnimationCurve(new Keyframe[3]{new Keyframe(0.4f, 0f),
//                                                                    new Keyframe(0.7f, 1f),
//                                                                    new Keyframe(1f, 0f)});
//            denseFoam = new AnimationCurve(new Keyframe[2]{new Keyframe(0.7f, 0f),
//                                                                    new Keyframe(1f, 1f)});
//        }
//    }
//}

//#endif