//#define DEBUG_LOG
// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.XR;

// ReSharper disable once CheckNamespace
namespace AkilliMum.SRP.Mirror
{
    [ExecuteAlways]
    public class MirrorManager : MonoBehaviour
    {
        [Tooltip("You can use this to show useful info about what is the script is doing on the console")]
        public bool IsDebug = false;

        [Tooltip("Please use this to enable/disable the script. DO NOT USE script's enable/disable check box!")]
        public bool IsEnabled = true;
        [Tooltip("You can name your script for to understand what is it for. Because if you use a lot of reflection scripts, you can forget which one is which :)")]
        public string Name;
        [Tooltip("Please use this to enable/disable the mirror in mirror effect. Use careful for performance reasons!")]
        public bool IsMirrorInMirror = false;
        [Tooltip("Please use this to give unique id's to mirrors which will be drawn together. So if you want to see a mirror inside another mirror, their id must be same!")]
        public string MirrorInMirrorId;
        [Tooltip("Check this if you suffer reflection glitches. Because camera may occlude some objects according to unity settings!")]
        public bool TurnOffOcclusion = false;
        //[Tooltip("Use object bounds for viewport? Use this if you have only one mirror object attached to the script.")]
        //public bool UseObjectBoundsForViewport = false;

        //[Header("Platform (Stand-Alone, VR, AR)")]
        [Tooltip("Please select the correct platform, Normal for stand alone like desktop or mobile etc., VR for virtual reality, AR for augmented reality!")]
        public Platform Platform = Platform.StandAlone;
        [Tooltip("It forces to render in MultiPass, please be sure that your device is in MultiPass mode!")]
        public bool ForceMultiPass = false;

        //[Header("Common")]
        [Tooltip("'Reflect' (mirror, reflective surface, transparent glass etc.) and 'Transparent' (transparent AR surface) is supported only right now.")]
        public WorkingType WorkingType = WorkingType.Reflect;
         [Tooltip("Mirror camera background clearing option. You can set it to color if you do not want the skybox to be reflected etc.!")]
        public ClearBackGround ClearBackGround = ClearBackGround.SkyBox;
        [Tooltip("Clear color for camera (you may change it for some effects like blur, so it blends with the background nicely)!")]
        public Color ClearColor = new Color(0, 0, 0, 0);
        [Tooltip("The mirror object's looking direction. Most of the time default 'GreenY' works perfectly. But try others if you get strange behavior.")]
        public UpVector UpVector = UpVector.GreenY;
        [Range(0, 10)]
        [Tooltip("Starts to drawing at this level of LOD. So if you do not creating perfect mirrors, you can use lower lod for surface and gain fps.")]
        public int CameraLODLevel = 0;
        //[Range(0, 10)]
        //[Tooltip("Creates the mip maps of the texture and uses the Xth value, you can use it specially for blur effects.")]
        //public int TextureLODLevel = 0;

        //[Header("Camera")]
        [Tooltip("Clips the rendering on mirror position by the percentage of the value. So if your distance from mirror is absolute (like car mirrors) you can gain very much performance!")]
        public bool UseClipping = false;
        [Tooltip("Percentage of the mirror according to screen size.")]
        [Range(1, 100)]
        public int ClippingPercentage = 20;
        [Tooltip("Which UV will be used to draw on Texture (it is generally UV0).")]
        public ClipUV ClipUV = ClipUV.None;
        [Tooltip("If you are using VR, this will select to which eye to render to texture. For example if you mimic the car mirrors, use Left for left and interior mirror, but use Right for the right mirror for consistency!")]
        public ClipEye ClipEye = ClipEye.None;
        //public Vector4 ClipMultiplier;
        [Tooltip("Enables the HDR, so post effects will be visible (like bloom) on the reflection.")]
        public bool HDR = false;
        [Tooltip("Enables the FOV on mirror camera (if applicable). '0' means, use main camera FOV :)")]
        [Range(0, 180)]
        public float FOV = 0;
        [Tooltip("Enables the anti aliasing (if only enabled in the project settings) for reflection. May decrease the performance, use with caution!")]
        public MSAALevel MSAALevel = MSAALevel.None;
        [Tooltip("Enables some pre-defined AA on mirror camera!")]
        public AntialiasingMode AntialiasingMode = AntialiasingMode.None;
        [Tooltip("Enables some pre-defined AA quality according to above selection on mirror camera!")]
        public AntialiasingQuality AntialiasingQuality = AntialiasingQuality.Low;
        [Tooltip("Should mirror camera render post effects (not recommended)")]
        public bool RenderPostProcessing = false;
        [Tooltip("Does mirror cam requires depth texture?")]
        public bool RequiresDepthTexture = false;
        [Tooltip("Does mirror cam requires opaque texture?")]
        public bool RequiresOpaqueTexture = false;
        [Tooltip("Filter mode to render texture (transparent render should render to Point!).")]
        public FilterMode FilterMode = FilterMode.Bilinear;
        [Tooltip("Enables mip mapping for the texture. So some effects like depth blur can work on shader. But it needs extra compute power!")]
        public bool MipMapping = false;
        [Tooltip("Disables the point and spot lights for the reflection. You may gain the fps, but you will lose the reality of reflection.")]
        public bool DisablePixelLights = false;
        //private IList<SceneLights> _additionalLights;
        [Tooltip("Enables the shadow on reflection. If you disable it; you may gain the fps, but you will lose the reality of reflection.")]
        public bool Shadow = false;
        //[Range(0, float.MaxValue)]
        //public float ShadowDistance = 0; //todo: shadow distance
        [Tooltip("Enables the culling distance calculations.")]
        public bool Cull = false;
        [Tooltip("Cull circle distance, so it just draws the objects in the distance. You may gain the fps, but you will lose the reality of reflection.")]
        public float CullDistance = 0;
        [Tooltip("Easy selection for reflection quality. Select 'Full' for perfect reflections! VR can render half; so select x2 etc. and try to find the best visual!")]
        public RenderTextureSize RenderTextureSize = RenderTextureSize.Manual;
        [Tooltip("The size (quality) of the reflection if manual is selected above! It should be set to width of the screen for perfect reflection! But try lower values to gain fps.")]
        public double ManualSize = 256;
        [Tooltip("Clipping distance to draw the reflection X units from the surface.")]
        public float ClipPlaneOffset = 0.02f;
        [Tooltip("Only these layers will be rendered by the reflection. So you can select what to be reflected with the reflection by putting them on certain layers.")]
        public LayerMask ReflectLayers = -1;

        //[Header("Affected Objects and Materials")]
        [Tooltip("Reflective surfaces (gameObjects) must be put here! Script calculates the reflection according to their position etc. and uses their material.")]
        public GameObject[] ReflectiveObjects;
        [Tooltip("Normally if camera does not see the objects(mirror meshes), for performance, we do not draw it. But there can be cases you always want them to be drawn. So you can check this!")]
        public bool DrawAlways;
        [Tooltip("If you want the camera to draw at a certain position (or according to another object like security cams), you can put the object here!")]
        public GameObject FakeCamera = null;

        //[Header("Simple Depth")]
        [Tooltip("Enables depth render (texture), so shader can use some depth based effects.")]
        public bool EnableDepth = false;

        //[Header("Depth Blur")]
        [Tooltip("Enables the advanced depth blur calculations.")]
        public bool EnableDepthBlur = false;
        [Tooltip("Depth blur shader to run on reflection texture.")]
        public Shader DepthBlurShader;
        public Material DepthBlurMaterial = null; //dynamic material to create and shade for above shader
        [Range(0, 30)]
        [Tooltip("Changes the depth distance, so the objects near to the reflective surface becomes more visible or not.")]
        public float DepthBlurCutoff = 0.8f;
        [Range(1, 20)]
        [Tooltip("Runs the depth blur shader on reflection texture X times. Larger iterations make more blurry images (but may decrease the fps)!")]
        public int DepthBlurIterations = 3;
        [Range(1, 20)]
        [Tooltip("This option makes the less blur on the pixels which are closer to surface. So you can make the far away pixels (on depth) more blurry!")]
        public float DepthBlurSurfacePower = 1;
        [Range(1, 50)]
        [Tooltip("Normally depth blur makes a circle blur. You can change this value if you want more horizontal blur!")]
        public int DepthBlurHorizontalMultiplier = 1;
        [Range(1, 50)]
        [Tooltip("Normally depth blur makes a circle blur. You can change this value if you want more vertical blur!")]
        public int DepthBlurVerticalMultiplier = 1;

        //[Header("Extend")]
        [Tooltip("You can add your custom shader paths here (like 'Shader Graphs/MyShader') and use mirror texture on your own shader!")]
        public string[] CustomShaders;

        [Tooltip("Texture to visualize output (reflection) on a UI-texture (RawImage) etc. You can use this (for example) to mimic car mirrors on UI!")]
        public UnityEngine.UI.RawImage UIImage;
        public UnityEngine.UI.RawImage UIImageDepth;

        //private float _deltaTime;

        [NonSerialized]
        public Camera _camera; //srp cam
        [NonSerialized]
        public Camera _cameraAttached; //if script attached to a camera
        [NonSerialized]
        public ScriptableRenderContext _context;

        public CameraManager _cameraManager;
        public RendererManager _rendererManager;
        public RenderTextureManager _renderTextureManager;
        public OptionManager _optionManager;


        // ReSharper disable once UnusedMember.Global
        protected virtual void OnEnable()
        {
            //Debug.Log("Base on enable");

            InitializeMirror();
        }

        public void InitializeMirror()
        {
            //destroy previous ones if any
            _cameraManager?.Destroy();
            _rendererManager?.Destroy();
            _renderTextureManager?.Destroy();
            _optionManager?.Destroy();

            _cameraManager = new CameraManager(GetCameraSettings());
            _rendererManager = new RendererManager(ReflectiveObjects, GetRendererSettings());
            _renderTextureManager = new RenderTextureManager(GetRenderTextureSettings());
            _optionManager = new OptionManager(GetOptionSettings());

            //ugly hack to not register 2 times :)
            RenderPipelineManager.beginCameraRendering -= ExecuteBeforeCameraRender;
            RenderPipelineManager.beginCameraRendering += ExecuteBeforeCameraRender;
        }

        RenderTextureManager.RenderTextureSettings GetRenderTextureSettings()
        {
            return new RenderTextureManager.RenderTextureSettings
            {
                IsDebug = IsDebug,
                WorkingType = WorkingType,
                Size = RenderTextureSize,
                //LODLevel = TextureLODLevel,
                ManualSize = ManualSize,
                HDR = HDR,
                MSAALevel = MSAALevel,
                FilterMode = FilterMode,
                MipMapping = MipMapping
            };
        }

        RendererManager.RendererSettings GetRendererSettings()
        {
            return new RendererManager.RendererSettings
            {
                IsDebug = IsDebug,
                UpVector = UpVector,
                CustomShaders = CustomShaders?.ToList(),
                DrawAlways = DrawAlways
            };
        }

        CameraManager.CameraSettings GetCameraSettings()
        {
            return new CameraManager.CameraSettings
            {
                IsDebug = IsDebug,
                WorkingType = WorkingType,
                UseClipping = UseClipping,
                ClipUV = (int)ClipUV,
                ClipEye = (int)ClipEye,
                ClippingPercentage = ClippingPercentage,
                //ClipMultiplier = ClipMultiplier,
                HDR = HDR,
                FOV = FOV,
                MSAALevel = MSAALevel,
                AntialiasingMode = AntialiasingMode,
                AntialiasingQuality = AntialiasingQuality,
                RenderPostProcessing = RenderPostProcessing,
                RequiresDepthTexture = RequiresDepthTexture,
                RequiresOpaqueTexture = RequiresOpaqueTexture,
                Platform = Platform,
                ForceMultiPass = ForceMultiPass,
                ClearBackGround = ClearBackGround,
                ClearColor = ClearColor,
                TurnOffOcclusion = TurnOffOcclusion,
                Cull = Cull,
                CullDistance = CullDistance,
                ReflectLayers = ReflectLayers,
                Shadow = Shadow,
                ClipPlaneOffset = ClipPlaneOffset,
                EnableDepth = EnableDepth,

                EnableDepthBlur = EnableDepthBlur,
                DepthBlurShader = DepthBlurShader,
                DepthBlurMaterial = DepthBlurMaterial,
                DepthBlurCutoff = DepthBlurCutoff,
                DepthBlurIterations = DepthBlurIterations,
                DepthBlurSurfacePower = DepthBlurSurfacePower,
                DepthBlurHorizontalMultiplier = DepthBlurHorizontalMultiplier,
                DepthBlurVerticalMultiplier = DepthBlurVerticalMultiplier,

                FakeCamera = FakeCamera
            };
        }

        OptionManager.OptionSettings GetOptionSettings()
        {
            return new OptionManager.OptionSettings
            {
                IsDebug = IsDebug,
                LODLevel = CameraLODLevel,
                DisablePixelLights = DisablePixelLights,
            };
        }


        // ReSharper disable once UnusedMember.Local
        // private void Update()
        // {
        //     _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
        // }

        private void ExecuteBeforeCameraRender(
           ScriptableRenderContext context,
           Camera cameraSrp)
        {
            if (_cameraAttached == null)
                _cameraAttached = GetComponent<Camera>();

            if (_camera == null)
                _camera = GetComponent<Camera>();
            if (_camera == null)
                _camera = Camera.main;
            if (_camera == null)
                _camera = cameraSrp;

            _context = context;

            //camera shade multi manager will handle the events!!
            if (IsMirrorInMirror)
                return;

            //do not draw if camera attach to camera item and this draw is about it
            if (_cameraAttached != null && cameraSrp != _cameraAttached)
                return;

            RenderReflective(null);
        }

        public virtual IList<Camera> RenderReflective(CameraShadeMultiManager manager, Camera sentCamera = null, bool invert = true, bool renderCam = true)
        {
            var cameras = new List<Camera>();

            //todo: check openxr, it may work different!
            //if (Platform == Platform.OpenXR && Application.isPlaying)
            //{
            //    //_stereoEye = 0;
            //    var camera0 = RenderMe(manager, StereoTargetEyeMask.Left, sentCamera, invert, renderCam);
            //    cameras.Add(camera0);

            //    //_stereoEye = 1;
            //    //todo: OPENXR single pass instanced, only one pass is enough???
            //    //var camera1 = RenderMe(manager, StereoTargetEyeMask.Right, sentCamera, invert, renderCam);
            //    //cameras.Add(camera1);
            //}
            if ((Platform == Platform.VR) &&
                Application.isPlaying &&
                ClipUV == ClipUV.None)
            {  //draw the scene twice for VR
                //Debug.Log("rendering VR...");
                //_stereoEye = 0;
                var camera0 = RenderMe(manager, StereoTargetEyeMask.Left, sentCamera, invert, renderCam);
                cameras.Add(camera0);

                //_stereoEye = 1;
                var camera1 = RenderMe(manager, StereoTargetEyeMask.Right, sentCamera, invert, renderCam);
                cameras.Add(camera1);
            }
            else
            {
                Camera camera0;
                //_stereoEye = -1;
                if (ClipEye == ClipEye.Left)
                    camera0 = RenderMe(manager, StereoTargetEyeMask.Left, sentCamera, invert, renderCam);
                else if (ClipEye == ClipEye.Right)
                    camera0 = RenderMe(manager, StereoTargetEyeMask.Right, sentCamera, invert, renderCam);
                else
                    camera0 = RenderMe(manager, StereoTargetEyeMask.None, sentCamera, invert, renderCam);
                cameras.Add(camera0);
            }

            return cameras;
        }

        private Camera GetCamera(Camera sentCamera)
        {
            var cameraToUse = sentCamera; // ?? _camera;

            if (!cameraToUse || cameraToUse == null)
                cameraToUse = _camera;

            return cameraToUse;
        }

        public virtual void CommonRender(
           ScriptableRenderContext context,
           Camera cameraSrp)
        {
            //common logic to call from inherited codes
        }

        FrameDrawn _frameDrawn = new FrameDrawn();
        private Camera RenderMe(CameraShadeMultiManager manager, StereoTargetEyeMask stereoTargetEyeMask, Camera sentCamera, bool invert = true, bool renderCam = true)
        {
            var cameraToUse = GetCamera(sentCamera);

            //do not render if not enabled or not visible
            if (
                !IsEnabled ||
                //todo: reflection probe baking?
                //cameraToUse.cameraType == CameraType.Reflection ||
                //cameraToUse.cameraType == CameraType.Preview ||
                !_rendererManager.IsObjectVisible(cameraToUse)
            //todo: solve below problem
            //Time.frameCount < 3 //it may run before the screen initialized, render textures maybe created in wrong dimension!
            //                    //screen.height returns 30 etc.sometimes (related to camera initialization, this may run before main cam initalized
            //                    //and calculated the real dimensions of screen!
            )
                return null;

            //will go into the drawing, but in vr we have to check render count, it may draw the scene for each eye, for 2 times = 4 times!
            //so it may become very dramatic :)
            if (_frameDrawn.Frame != Time.frameCount)
            {
                //so we will create a new frame drawn for this frame!
                _frameDrawn = new FrameDrawn { Frame = Time.frameCount };
            }
            //if any frame is rendered? just return
            if ((_frameDrawn.IsLeftDrawn && stereoTargetEyeMask == StereoTargetEyeMask.Left)
                ||
                (_frameDrawn.IsRightDrawn && stereoTargetEyeMask == StereoTargetEyeMask.Right))
                return null;
            if (stereoTargetEyeMask == StereoTargetEyeMask.Left)
                _frameDrawn.IsLeftDrawn = true;
            if (stereoTargetEyeMask == StereoTargetEyeMask.Right)
                _frameDrawn.IsRightDrawn = true;


            CommonRender(_context, _camera);

            //hack to draw different FOV
            //var previousFieldOfView = cameraToUse.fieldOfView;
            //cameraToUse.fieldOfView = previousFieldOfView/4f;

            try
            {
                _optionManager.Start();

                //use this cam as ref afterwards
                _cameraManager.SetReferenceCamera(cameraToUse);

                //create mirror cam and set options etc.
                _cameraManager.CreateMirrorCamera();

                //create textures if necessary
                _renderTextureManager.CreateRenderTextures(_cameraManager);

                _cameraManager.SetFOV();

                //set fake camera if asked
                _cameraManager.SetFake();

                //set viewport if asked
                _cameraManager.SetViewport(_rendererManager, stereoTargetEyeMask);

                //draw scene
                _cameraManager.Draw(_rendererManager, _renderTextureManager,
                    _context, manager, 
                    stereoTargetEyeMask, invert, renderCam);

                //set materials if necessary
                _rendererManager.UpdateMaterials(_renderTextureManager, _cameraManager);
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
            finally
            {
                //revert view port
                _cameraManager.ResetViewport(stereoTargetEyeMask);

                //revert fake
                _cameraManager.ResetFake();

                //revert fov
                _cameraManager.ResetFOV();

                //revert options
                _optionManager.End();
            }

            if (UIImage)
                UIImage.texture = _renderTextureManager.LeftOrCenterRT1;
            if (UIImageDepth)
                UIImageDepth.texture = _renderTextureManager.LeftOrCenterDepthRT1;

            return _cameraManager.GetMirrorCamera();
        }

        // ReSharper disable once UnusedMember.Global
        protected virtual void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= ExecuteBeforeCameraRender;

            _renderTextureManager?.Destroy();
            _cameraManager?.Destroy();
        }
    }

    public class FrameDrawn
    {
        public int Frame { get; set; }
        public bool IsLeftDrawn { get; set; }
        public bool IsRightDrawn { get; set; }
    }

    public enum UpVector
    {
        None = 0,
        RedX = 1,
        RedX_Negative = 4,
        GreenY = 2,
        GreenY_Negative = 5,
        BlueZ = 3,
        BlueZ_Negative = 6
    }

    public enum ClipUV
    {
        UV0 = 0,
        UV1 = 1,
        UV2 = 2,
        UV3 = 3,
        None = 99
    }

    public enum ClipEye
    {
        None = 0,
        Left = 1,
        Right = 2
    }

    public enum MSAALevel
    {
        None = 0,
        X2 = 2,
        X4 = 4,
        X8 = 8
    }

    public enum WorkingType
    {
        Reflect = 1,
        //Direct = 20,
        Transparent = 30, //do not change 30, it is used in shader
        //Water = 40
    }

    public enum RenderTextureSize
    {
        Manual = 0,
        x4 = 6,
        x2 = 5,
        Full = 1,
        Half = 2,
        Quarter = 4,
        Eighth = 8,
    }

    public enum Platform
    {
        StandAlone = 0,
        VR = 10,
        //OpenXR = 20,
        //Oculus = 30,
        AR = 100, //do not change used in shader
    }

    public enum ClearBackGround
    {
        SkyBox = 1,
        Color = 2
    }

    public class RendererManager : ManagerBase
    {
        private GameObject _first;
        private List<Renderer> _renderers;
        private List<Material> _materials;

        private RendererSettings _settings;

        public struct RendererSettings
        {
            public bool IsDebug { get; set; }
            public UpVector UpVector { get; set; }
            public IList<string> CustomShaders { get; set; }
            public bool DrawAlways { get; set; }
        }

        public RendererManager(GameObject[] reflectiveObjects, RendererSettings renderSettings)
        {
            SetSettings(renderSettings); //call first!
            SetReflectiveObjects(reflectiveObjects);
        }

        public void SetSettings(RendererSettings renderSettings)
        {
            _settings = renderSettings;
        }

        /// <summary>
        /// Finds the materials to set reflection textures on them. So shader can draw them.
        /// </summary>
        /// <param name="reflectiveObjects"></param>
        public void SetReflectiveObjects(GameObject[] reflectiveObjects)
        {
            _renderers = new List<Renderer>();
            _materials = new List<Material>();

            if (reflectiveObjects != null && reflectiveObjects.Length > 0)
            {
                _first = reflectiveObjects[0]; //use as position etc.!
                foreach (var reflectiveObject in reflectiveObjects)
                {
                    if (reflectiveObject != null && reflectiveObject.GetComponent<Renderer>() != null)
                        _renderers.Add(reflectiveObject.GetComponent<Renderer>());
                }
            }

            if (_renderers != null)
            {
                foreach (var ren in _renderers)
                {
                    if (ren != null)
                    {
                        foreach (var mat in ren.sharedMaterials)
                        {
                            if (_settings.IsDebug) Debug.Log("mat.shader.namer: " + mat.shader.name);

                            if (mat != null &&
                                (mat.shader.name == "Shader Graphs/AMMirrorOneEye" ||
                                 mat.shader.name == "Shader Graphs/AMMirrorTwoEyes" ||
                                 //this our hand written old school way shader :)
                                 mat.shader.name == "AkilliMum/URP/Mirrors/Complex"))
                            {
                                _materials.Add(mat);
                            }
                            else if (mat != null &&
                                     _settings.CustomShaders != null &&
                                     _settings.CustomShaders.Contains(mat.shader.name))
                            {
                                if(_settings.IsDebug) Debug.Log("Found custom shader: " + mat.shader.name);
                                _materials.Add(mat);
                            }
                        }
                    }
                }
            }
        }

        public bool IsObjectVisible(Camera cam)
        {
            if (_settings.DrawAlways)
            {
                if(_settings.IsDebug) Debug.Log("Will always draw, frame ..." + Time.frameCount);
                return true;
            }

            var visible = false;
            foreach (var ren in _renderers)
            {
                if (ren.IsVisibleFrom(cam) && ren.gameObject.activeSelf) //if any of renderer is visible
                {
                    visible = true;
                    break;
                }
            }

            return visible;
        }

        public Vector3 GetPosition()
        {
            return _first.transform.position;
        }

        public GameObject GetFirst()
        {
            return _first;
        }

        public Vector3 GetNormal(bool invert)
        {
            Vector3 normal;
            if (_settings.UpVector == UpVector.GreenY)
            {
                normal = _first.transform.up; //all items must be on same vector direction :) so we can use first one
            }
            else if (_settings.UpVector == UpVector.GreenY_Negative)
            {
                normal = -_first.transform.up;
            }
            else if (_settings.UpVector == UpVector.BlueZ)
            {
                normal = _first.transform.forward;
            }
            else if (_settings.UpVector == UpVector.BlueZ_Negative)
            {
                normal = -_first.transform.forward;
            }
            else if (_settings.UpVector == UpVector.RedX)
            {
                normal = _first.transform.right;
            }
            else //if (UpVector == UpVector.RedX_Negative)
            {
                normal = -_first.transform.right;
            }
            normal *= (invert ? 1 : -1); //flip normals if drawing reflections reflection :)
            return normal;
        }

        /// <summary>
        /// Updates material properties, it only runs if <see cref="RendererManager.IsDirty"/> is true
        /// </summary>
        /// <param name="renderTextureManager"></param>
        /// <param name="cameraManager"></param>
        public void UpdateMaterials(RenderTextureManager renderTextureManager, CameraManager cameraManager)
        {
            //re-run if only class is dirty :)
            if (!IsDirty)
                return;
            IsDirty = false;

            if (_settings.IsDebug) Debug.Log("renderer manager update materials called...");

            foreach (var mat in _materials)
            {
                if (_settings.IsDebug) Debug.Log("Application.platform: " + Application.platform);

                if (mat.HasProperty("_WorkType"))
                    mat.SetFloat("_WorkType", (int)cameraManager.GetSettings().WorkingType);

                //it is good on desktop, open it only for android (oculus quest etc.)
                if (XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.MultiPass ||
                    cameraManager.GetSettings().ForceMultiPass)
                {
                    if (mat.HasProperty("_IsMultiPass"))
                        mat.SetFloat("_IsMultiPass", 1f);
                }
                else
                {
                    if (mat.HasProperty("_IsMultiPass"))
                        mat.SetFloat("_IsMultiPass", 0f);
                }

                if (cameraManager.GetSettings().UseClipping)
                {
                    if (mat.HasProperty("_ClipUV"))
                        mat.SetInt("_ClipUV", cameraManager.GetSettings().ClipUV);
                    if (mat.HasProperty("_ClipPercentage"))
                        mat.SetInt("_ClipPercentage", cameraManager.GetSettings().ClippingPercentage);
                    if (mat.HasProperty("_ClipEye"))
                        mat.SetInt("_ClipEye", cameraManager.GetSettings().ClipEye);
                }
                else
                {
                    if (mat.HasProperty("_ClipUV"))
                        mat.SetInt("_ClipUV", (int)ClipUV.None);
                    if (mat.HasProperty("_ClipEye"))
                        mat.SetInt("_ClipEye", (int)ClipEye.None);
                }

                if (cameraManager.GetSettings().EnableDepth || cameraManager.GetSettings().EnableDepthBlur)
                {
                    if (mat.HasProperty("_NearClip"))
                        mat.SetFloat("_NearClip", cameraManager.GetMirrorCamera().nearClipPlane);
                    if (mat.HasProperty("_FarClip"))
                        mat.SetFloat("_FarClip", cameraManager.GetMirrorCamera().farClipPlane);
                }

                if (cameraManager.GetSettings().EnableDepthBlur)
                {
                    //Debug.Log("blitting depth to material texture..");
                    if (cameraManager.GetSettings().DepthBlurIterations % 2 == 1) //again a hack:)
                    {
                        if (mat.HasProperty("_LeftOrCenterTexture"))
                            mat.SetTexture("_LeftOrCenterTexture", renderTextureManager.LeftOrCenterRT3);

                        if (mat.HasProperty("_RightTexture"))
                            mat.SetTexture("_RightTexture", renderTextureManager.RightRT3);
                    }
                    else
                    {
                        if (mat.HasProperty("_LeftOrCenterTexture"))
                            mat.SetTexture("_LeftOrCenterTexture", renderTextureManager.LeftOrCenterRT2);

                        if (mat.HasProperty("_RightTexture"))
                            mat.SetTexture("_RightTexture", renderTextureManager.RightRT2);
                    }
                }
                else
                {
                    if (mat.HasProperty("_LeftOrCenterTexture"))
                        mat.SetTexture("_LeftOrCenterTexture", renderTextureManager.LeftOrCenterRT1);

                    if (mat.HasProperty("_RightTexture"))
                        mat.SetTexture("_RightTexture", renderTextureManager.RightRT1);
                }

                if (mat.HasProperty("_LeftOrCenterDepthTexture"))
                    mat.SetTexture("_LeftOrCenterDepthTexture", renderTextureManager.LeftOrCenterDepthRT1);

                if (mat.HasProperty("_RightDepthTexture"))
                    mat.SetTexture("_RightDepthTexture", renderTextureManager.RightDepthRT1);
            }
        }

        public override void Destroy()
        {

        }
    }

    public abstract class ManagerBase
    {
        private bool _isDirty = true;
        public bool IsDirty
        {
            get => _isDirty;
            set => _isDirty = value;
        }

        public abstract void Destroy();

        public void DestroyObject(UnityEngine.Object obj)
        {
            if (Application.isEditor)
                UnityEngine.Object.DestroyImmediate(obj);
            else
                UnityEngine.Object.Destroy(obj);
        }
    }

    /// <summary>
    /// Manages the render textures
    /// </summary>
    public class RenderTextureManager : ManagerBase
    {
        private RenderTexture _leftOrCenterRT1;
        public RenderTexture LeftOrCenterRT1
        {
            get { return _leftOrCenterRT1; }
            set { _leftOrCenterRT1 = value; }
        }

        private RenderTexture _leftOrCenterRT2;
        public RenderTexture LeftOrCenterRT2
        {
            get { return _leftOrCenterRT2; }
            set { _leftOrCenterRT2 = value; }
        }

        private RenderTexture _leftOrCenterRT3;
        public RenderTexture LeftOrCenterRT3
        {
            get { return _leftOrCenterRT3; }
            set { _leftOrCenterRT3 = value; }
        }

        private RenderTexture _rightRT1;
        public RenderTexture RightRT1
        {
            get { return _rightRT1; }
            set { _rightRT1 = value; }
        }

        private RenderTexture _rightRT2;
        public RenderTexture RightRT2
        {
            get { return _rightRT2; }
            set { _rightRT2 = value; }
        }

        private RenderTexture _rightRT3;
        public RenderTexture RightRT3
        {
            get { return _rightRT3; }
            set { _rightRT3 = value; }
        }

        private RenderTexture _leftOrCenterDepthRT1;
        public RenderTexture LeftOrCenterDepthRT1
        {
            get { return _leftOrCenterDepthRT1; }
            set { _leftOrCenterDepthRT1 = value; }
        }

        private RenderTexture _rightDepthRT1;
        public RenderTexture RightDepthRT1
        {
            get { return _rightDepthRT1; }
            set { _rightDepthRT1 = value; }
        }

        private RenderTextureSettings _settings;

        public struct RenderTextureSettings
        {
            public bool IsDebug { get; set; }
            public bool HDR { get; set; }
            public RenderTextureSize Size { get; set; }
            public double ManualSize { get; set; }
            public WorkingType WorkingType { get; set; }
            public MSAALevel MSAALevel { get; set; }
            public FilterMode FilterMode { get; set; }
            //public int LODLevel { get; set; }
            public bool MipMapping { get; set; }
        }

        public RenderTextureManager(RenderTextureSettings settings)
        {
            SetSettings(settings);
        }

        public void SetSettings(RenderTextureSettings settings)
        {
            _settings = settings;
        }

        public RenderTextureSettings GetSettings()
        {
            return _settings;
        }

        public void GenerateMipMaps(StereoTargetEyeMask stereoTargetEyeMask)
        {
            if (stereoTargetEyeMask != StereoTargetEyeMask.Right)
                _leftOrCenterRT1.GenerateMips();
            else
                _rightRT1.GenerateMips();
        }

        /// <summary>
        /// Creates the render textures, looks into the <see cref="RenderTextureManager.IsDirty"/> if recreation must be done
        /// </summary>
        public void CreateRenderTextures(CameraManager cameraManager)
        {
            //re-run if only class is dirty :)
            if (!IsDirty)
                return;
            IsDirty = false;

            if (_settings.IsDebug) Debug.Log("texture manager create render textures called...");

            //reflectionCamera = null;
            int depth = 24;

            var textureSizes = GetTextureSizes(cameraManager);

            //there must be something wrong, so create everything again
            if (textureSizes[0] < 50 || textureSizes[1] < 50)
                IsDirty = true;

            RenderTextureFormat textureFormatHDR;
            RenderTextureFormat textureFormat;

            if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat))
                textureFormatHDR = RenderTextureFormat.ARGBFloat;
            else
                textureFormatHDR = RenderTextureFormat.DefaultHDR;

            if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32))
                textureFormat = RenderTextureFormat.ARGB32;
            else
                textureFormat = RenderTextureFormat.Default;

            if (_settings.IsDebug) Debug.Log("will create render texture sized: width:" + textureSizes[0] + " height: " + textureSizes[1]);

            _leftOrCenterRT1 = Create("1VdA4XHv0DNgALiPBjErmUAdTpKHmwy", textureSizes[0], textureSizes[1], depth, textureFormat, textureFormatHDR);
            _leftOrCenterRT2 = Create("kh1H4uh0fjT4w1meIuCO2DR5AvwCb2", textureSizes[0], textureSizes[1], depth, textureFormat, textureFormatHDR);
            _leftOrCenterRT3 = Create("MHdugjyxTKL20GKQwGT1jUZS2vIduy", textureSizes[0], textureSizes[1], depth, textureFormat, textureFormatHDR);
            _rightRT1 = Create("WMXrGP1UKcsbl7kUidgZvClTek6syr", textureSizes[0], textureSizes[1], depth, textureFormat, textureFormatHDR);
            _rightRT2 = Create("b8deekK4hyDbZ5NqvdAxkcd41cXn5z", textureSizes[0], textureSizes[1], depth, textureFormat, textureFormatHDR);
            _rightRT3 = Create("i9cOpxL9h16kQ9GWC70yJOTgJc4wJu", textureSizes[0], textureSizes[1], depth, textureFormat, textureFormatHDR);

            _leftOrCenterDepthRT1 = CreateDepth("dQqWNXTxXpyTxIRhrmvs73ebKsaaKh", textureSizes[0], textureSizes[1], depth);
            _rightDepthRT1 = CreateDepth("PsiIcbyqRIwTcUclRSfiwb44lrqmyj", textureSizes[0], textureSizes[1], depth);
        }

        private RenderTexture Create(string name, int width, int height, int depth, RenderTextureFormat textureFormat, RenderTextureFormat textureFormatHDR)
        {
            RenderTexture renderTexture;
            if (_settings.HDR)
                renderTexture = new RenderTexture(width, height, depth, textureFormatHDR);
            else
                renderTexture = new RenderTexture(width, height, depth, textureFormat);

            renderTexture.name = name;

            if ((int)_settings.MSAALevel > 0)
                renderTexture.antiAliasing = (int)_settings.MSAALevel;
            renderTexture.filterMode = _settings.FilterMode;

            renderTexture.isPowerOfTwo = true;
            renderTexture.hideFlags = HideFlags.HideAndDontSave;
            if (_settings.MipMapping)
            {
                renderTexture.useMipMap = true;
                renderTexture.autoGenerateMips = false;
            }
            else
            {
                renderTexture.useMipMap = false;
            }

            return renderTexture;
        }

        private RenderTexture CreateDepth(string name, int width, int height, int depth)
        {
            RenderTexture renderTexture;
            renderTexture = new RenderTexture(width, height, depth, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
            renderTexture.wrapMode = TextureWrapMode.Clamp;
            renderTexture.name = name;
            renderTexture.isPowerOfTwo = true;
            renderTexture.hideFlags = HideFlags.DontSave;
            return renderTexture;
        }

        private int[] GetTextureSizes(CameraManager cameraManager)
        {
            //Calculate the render size
            double width = Screen.width;
            double height = Screen.height;

            /*if VR */
            if (cameraManager.GetSettings().Platform == Platform.VR &&
                Application.IsPlaying(cameraManager.GetReferenceCamera()))
            {
                width = XRSettings.eyeTextureWidth;
                height = XRSettings.eyeTextureHeight;
            }

            if (width <= 0 || height <= 0)
            {
                width = Screen.width;
                height = Screen.height;
            }

            if (_settings.IsDebug) Debug.Log("screen width-height: " + width + " " + height);

            switch (_settings.Size)
            {
                case RenderTextureSize.Eighth:
                    _settings.ManualSize = width / 8;
                    break;
                case RenderTextureSize.Quarter:
                    _settings.ManualSize = width / 4;
                    break;
                case RenderTextureSize.Half:
                    _settings.ManualSize = width / 2;
                    break;
                case RenderTextureSize.Full:
                    _settings.ManualSize = width;
                    break;
                case RenderTextureSize.x2:
                    _settings.ManualSize = width * 2;
                    break;
                case RenderTextureSize.x4:
                    _settings.ManualSize = width * 4;
                    break;
                    //default:
                    //    break; //do not change Manual size
            }

            double textureSize = _settings.ManualSize + _settings.ManualSize % 2; //calculate the width and height according to aspect
          
            //int ops creates 0 (zero)!!!
            double textureSizeHeight = textureSize * (height / width);
            textureSizeHeight = textureSizeHeight + textureSizeHeight % 2;

            return new[] { (int)textureSize, (int)textureSizeHeight };
        }

        public override void Destroy()
        {
            DestroyObject(_leftOrCenterRT1);
            _leftOrCenterRT1 = null;
            DestroyObject(_leftOrCenterRT2);
            _leftOrCenterRT2 = null;
            DestroyObject(_leftOrCenterRT3);
            _leftOrCenterRT3 = null;

            DestroyObject(_rightRT1);
            _rightRT1 = null;
            DestroyObject(_rightRT2);
            _rightRT2 = null;
            DestroyObject(_rightRT3);
            _rightRT3 = null;

            DestroyObject(_leftOrCenterDepthRT1);
            _leftOrCenterDepthRT1 = null;
            DestroyObject(_rightDepthRT1);
            _rightDepthRT1 = null;
        }
    }

    /// <summary>
    /// Manages the reflection camera
    /// </summary>
    public class CameraManager : ManagerBase
    {
        public const string CameraNameStart = "Mirror camera for ";

        private Camera _mainCamera;

        private GameObject _mirrorCameraContainer;
        private Camera _mirrorCamera;

        //private List<XRNodeState> nodeStates = new List<XRNodeState>();

        private CameraSettings _settings;

        public struct CameraSettings
        {
            public bool IsDebug { get; set; }
            public WorkingType WorkingType { get; set; }
             public ClearBackGround ClearBackGround { get; set; }
            public Color ClearColor { get; set; }
            public Platform Platform { get; set; }
            public bool ForceMultiPass { get; set; }
            public bool HDR { get; set; }
            public float FOV { get; set; }
            public bool UseClipping { get; set; }
            public int ClipUV { get; set; }
            public int ClipEye { get; set; }
            public int ClippingPercentage { get; set; }
            //public Vector4 ClipMultiplier { get; set; }
            public bool TurnOffOcclusion { get; set; }
            public MSAALevel MSAALevel { get; set; }
            public AntialiasingMode AntialiasingMode { get; set; }
            public AntialiasingQuality AntialiasingQuality { get; set; }
            public bool RenderPostProcessing { get; set; }
            public bool RequiresDepthTexture { get; set; }
            public bool RequiresOpaqueTexture { get; set; }
            public bool Cull { get; set; }
            public float CullDistance { get; set; }
            public LayerMask ReflectLayers { get; set; }
            public bool Shadow { get; set; }
            public float ClipPlaneOffset { get; set; }
            public bool EnableDepth { get; set; }
            public bool EnableDepthBlur { get; set; }
            public Shader DepthBlurShader { get; set; }
            public Material DepthBlurMaterial { get; set; }
            public float DepthBlurCutoff { get; set; }
            public int DepthBlurIterations { get; set; }
            public float DepthBlurSurfacePower { get; set; }
            public int DepthBlurHorizontalMultiplier { get; set; }
            public int DepthBlurVerticalMultiplier { get; set; }
            public GameObject FakeCamera { get; set; }
        }

        public CameraManager(CameraSettings settings)
        {
            _settings = settings;
        }

        //public void SetSettings(CameraSettings settings)
        //{
        //    _settings = settings;
        //}

        public CameraSettings GetSettings()
        {
            return _settings;
        }

        public void SetReferenceCamera(Camera camera)
        {
            //so we will create mirror cam
            if (_mainCamera is null ||
                _mirrorCamera is null ||
                _mirrorCamera.name != CameraNameStart + camera.GetInstanceID())
            {
                _mainCamera = camera;
                IsDirty = true;
            }
        }

        public Camera GetMirrorCamera()
        {
            return _mirrorCamera;
        }

        public Camera GetReferenceCamera()
        {
            return _mainCamera;
        }

        /// <summary>
        /// Creates the reflection camera and returns it, recreation is done only if <see cref="CameraManager.IsDirty"/> is true
        /// </summary>
        /// <returns></returns>
        public void CreateMirrorCamera()
        {
            //re-run if only class is dirty :)
            if (!IsDirty)
                return;
            IsDirty = false;

            if (_settings.IsDebug) Debug.Log("camera manager create mirror camera called...");

            _mirrorCameraContainer = new GameObject("Mirror object for " + _mainCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));
            _mirrorCamera = _mirrorCameraContainer.GetComponent<Camera>();

            _mirrorCamera.CopyFrom(_mainCamera);

            _mirrorCamera.name = CameraNameStart + _mainCamera.GetInstanceID();
            _mirrorCamera.enabled = false;
            _mirrorCamera.gameObject.AddComponent<FlareLayer>();
            _mirrorCameraContainer.hideFlags = HideFlags.HideAndDontSave;

            //_mirrorCamera.gameObject.AddComponent<FlareLayer>();
             
            //try to get camData
            var urpCamData = _mirrorCamera.gameObject.GetComponent<UniversalAdditionalCameraData>();
            //if it is not added before, add
            if (urpCamData == null)
            {
                urpCamData = _mirrorCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
                if (_settings.IsDebug) Debug.Log("adding uacd for mirror cam (it was null)");
            }
            if (urpCamData != null)
            {
                var realCamData = _mainCamera.GetComponent<UniversalAdditionalCameraData>();
                if (realCamData != null)
                    realCamData.CopyTo(urpCamData);
                //urpCamData.stopNaN = _mainCamera;
                //urpCamData.dithering = true;
                urpCamData.requiresColorOption = _settings.RequiresOpaqueTexture ? CameraOverrideOption.On : CameraOverrideOption.Off;
                urpCamData.requiresDepthOption = _settings.RequiresDepthTexture ? CameraOverrideOption.On : CameraOverrideOption.Off;
                urpCamData.renderShadows = _settings.Shadow; // turn on-off shadows for the reflection camera
                urpCamData.antialiasing = _settings.AntialiasingMode;
                urpCamData.antialiasingQuality = _settings.AntialiasingQuality;
                urpCamData.renderPostProcessing = _settings.RenderPostProcessing;
                if(_settings.Platform == Platform.VR)
                    urpCamData.allowXRRendering = true;
                else
                    urpCamData.allowXRRendering = false;
                //if (_settings.Platform == Platform.VR) 
                //    urpCamData.allowXRRendering = true;
            }

            UpdateCameraModes();
        }

        private void UpdateCameraModes()
        {           
            if (_settings.ClearBackGround == ClearBackGround.Color)
            {
                //transparency need to have clear color (to clear on shader) and no MSAA
                _mirrorCamera.clearFlags = CameraClearFlags.Color;
                _mirrorCamera.backgroundColor = _settings.ClearColor;
            }

            // update other values to match current camera.
            // even if we are supplying custom camera&projection matrices,
            // some of values are used elsewhere (e.g. skybox uses far plane)
            //todo: manuel clip planes? it does not work actually, cause view matrix etc. changes them

            //_mirrorCamera.nearClipPlane = _mainCamera.nearClipPlane;
            //_mirrorCamera.farClipPlane = _mainCamera.farClipPlane;

            //_mirrorCamera.orthographic = _mainCamera.orthographic;
            //_mirrorCamera.orthographicSize = _mainCamera.orthographicSize;

            if (_settings.EnableDepth || _settings.EnableDepthBlur)
            {
                _mirrorCamera.depthTextureMode = DepthTextureMode.Depth;
            }
            else
            {
                _mirrorCamera.depthTextureMode = DepthTextureMode.None;
            }

            //_mirrorCamera.aspect = _mainCamera.aspect;
            //_mirrorCamera.renderingPath = _mainCamera.renderingPath;
            _mirrorCamera.allowHDR = _settings.HDR;

            _mirrorCamera.allowMSAA = (int)_settings.MSAALevel > 0 || _settings.AntialiasingMode != AntialiasingMode.None;

            _mirrorCamera.useOcclusionCulling = !_settings.TurnOffOcclusion;
            //_mirrorCamera.cameraType = _mainCamera.cameraType;

            //set cull distance if selected
            if (_settings.Cull)
            {
                float[] distances = new float[32]; //for all layers :)
                for (int i = 0; i < distances.Length; i++)
                {
                    distances[i] = _settings.Cull ? _settings.CullDistance : _mirrorCamera.farClipPlane; //the culling distance
                }
                _mirrorCamera.layerCullDistances = distances;
                _mirrorCamera.layerCullSpherical = _settings.Cull;
            }

            //_mirrorCamera.cullingMask = ~(1 << 4) & _settings.ReflectLayers.value; // never render water layer
            _mirrorCamera.cullingMask = _settings.ReflectLayers.value;
        }

        private Rect _viewPort;
        private Quaternion _viewRotation;
        private StereoTargetEyeMask _viewEyeMask;
        public void SetViewport(RendererManager rendererManager, StereoTargetEyeMask stereoTargetEyeMask)
        {
            if (_settings.UseClipping)
            {
                ////find the screenpos of the mirror 
                //Vector3 screenPos = _mainCamera.WorldToScreenPoint(rendererManager.GetPosition());
                //Debug.Log("screen pos renderer: " + screenPos);

                //RESET MAIN CAM!!!!!!!!!!!
                //save previous values
                _viewPort = _mainCamera.rect;
                _viewRotation = _mainCamera.transform.rotation;
                _viewEyeMask = _mainCamera.stereoTargetEye;

                //change cam
                //todo: is it necessary?
                //_mainCamera.stereoTargetEye = stereoTargetEyeMask;
                if (_settings.IsDebug) Debug.Log("First item pos: " + rendererManager.GetFirst().transform.position);
                _mainCamera.transform.LookAt(rendererManager.GetFirst().transform.position,
                    rendererManager.GetFirst().transform.forward);

                var multi = 100f / _settings.ClippingPercentage;
                var ratio = _settings.ClippingPercentage / 100f;

                Rect r = new Rect(new Vector2(0.5f - 1f / (multi * 2), 0.5f - 1f / (multi * 2)),
                new Vector2(ratio, ratio));

                SetScissorRect(r, stereoTargetEyeMask);
            }
        }

        public void ResetViewport(StereoTargetEyeMask stereoTargetEyeMask)
        {
            if (_settings.UseClipping)
            {
                if (stereoTargetEyeMask != StereoTargetEyeMask.None)
                {
                    _mainCamera.ResetStereoProjectionMatrices();
                }
                else
                {
                    _mainCamera.ResetProjectionMatrix();
                }
                _mainCamera.rect = _viewPort;
                _mainCamera.transform.rotation = _viewRotation;
                _mainCamera.stereoTargetEye = _viewEyeMask;
            }
        }

        public void SetScissorRect(Rect r, StereoTargetEyeMask stereoTargetEyeMask)
        {
            if (r.x < 0)
            {
                r.width += r.x;
                r.x = 0;
            }

            if (r.y < 0)
            {
                r.height += r.y;
                r.y = 0;
            }

            r.width = Mathf.Min(1 - r.x, r.width);
            r.height = Mathf.Min(1 - r.y, r.height);

            //???do need to change it here (or reset)?
            //cam.rect = new Rect(0, 0, 1, 1);

            //do need to reset them???
            if (stereoTargetEyeMask != StereoTargetEyeMask.None)
            {
                _mainCamera.ResetStereoProjectionMatrices();
            }
            else
            {
                _mainCamera.ResetProjectionMatrix();
            }

            Matrix4x4 m;
            //if (_settings.Platform == Platform.VR)
            //{
            //    //todo: it seems to draw correct texture for both eyes on clipping enabled! search for more!!
            //    m = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            //}
            //else
            if (stereoTargetEyeMask != StereoTargetEyeMask.None)
            {
                m = _mainCamera.GetStereoProjectionMatrix(
                    stereoTargetEyeMask == StereoTargetEyeMask.Left
                        ? Camera.StereoscopicEye.Left
                        : Camera.StereoscopicEye.Right);
            }
            else
                m = _mainCamera.projectionMatrix;
            _mainCamera.rect = r;
            //Matrix4x4 m1 = Matrix4x4.TRS(new Vector3(r.x, r.y, 0), Quaternion.identity, new Vector3(r.width, r.height, 1));
            Matrix4x4 m2 = Matrix4x4.TRS(new Vector3((1 / r.width - 1), (1 / r.height - 1), 0), Quaternion.identity, new Vector3(1 / r.width, 1 / r.height, 1));
            Matrix4x4 m3 = Matrix4x4.TRS(new Vector3(-r.x * 2 / r.width, -r.y * 2 / r.height, 0), Quaternion.identity, Vector3.one);
            var result = m3 * m2 * m;

            //cam.projectionMatrix = result;
            if (stereoTargetEyeMask != StereoTargetEyeMask.None)
            {
                //_mainCamera.SetStereoProjectionMatrices(result,re);
                _mainCamera.SetStereoProjectionMatrix(stereoTargetEyeMask == StereoTargetEyeMask.Left
                    ? Camera.StereoscopicEye.Left
                    : Camera.StereoscopicEye.Right, result);
            }
            else
                _mainCamera.projectionMatrix = result;
        }

        private Vector3 _viewTransformPosition;
        private Quaternion _viewTransformRotation;
        private Vector3 _viewTransformForward;

        public void SetFake()
        {
            if (_settings.FakeCamera != null && _settings.FakeCamera)
            {
                //save previous values
                _viewTransformPosition = _mainCamera.transform.position;
                _viewTransformRotation = _mainCamera.transform.rotation;
                _viewTransformForward = _mainCamera.transform.forward;

                //change cam
                _mainCamera.transform.position = _settings.FakeCamera.transform.position;
                _mainCamera.transform.rotation = _settings.FakeCamera.transform.rotation;
                _mainCamera.transform.forward = _settings.FakeCamera.transform.forward;
            }
        }
        public void ResetFake()
        {
            if (_settings.FakeCamera != null && _settings.FakeCamera)
            {
                _mainCamera.transform.position = _viewTransformPosition;
                _mainCamera.transform.rotation = _viewTransformRotation;
                _mainCamera.transform.forward = _viewTransformForward;
            }
        }

        private float _viewFOV;
        public void SetFOV()
        {
            if (_settings.FOV > 0)
            {
                //save previous values
                _viewFOV = _mainCamera.fieldOfView;

                //change cam
                _mainCamera.fieldOfView = _settings.FOV;
            }
        }
        public void ResetFOV()
        {
            if (_settings.FOV > 0)
            {
                //change cam
                _mainCamera.fieldOfView = _viewFOV;
            }
        }

        // Calculates reflection matrix around the given plane
        private void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
        {
            reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
            reflectionMat.m01 = (-2F * plane[0] * plane[1]);
            reflectionMat.m02 = (-2F * plane[0] * plane[2]);
            reflectionMat.m03 = (-2F * plane[3] * plane[0]);

            reflectionMat.m10 = (-2F * plane[1] * plane[0]);
            reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
            reflectionMat.m12 = (-2F * plane[1] * plane[2]);
            reflectionMat.m13 = (-2F * plane[3] * plane[1]);

            reflectionMat.m20 = (-2F * plane[2] * plane[0]);
            reflectionMat.m21 = (-2F * plane[2] * plane[1]);
            reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
            reflectionMat.m23 = (-2F * plane[3] * plane[2]);

            reflectionMat.m30 = 0F;
            reflectionMat.m31 = 0F;
            reflectionMat.m32 = 0F;
            reflectionMat.m33 = 1F;
        }

        // Given position/normal of the plane, calculates plane in camera space.
        private Vector4 CameraSpacePlane(Matrix4x4 worldToCameraMatrix, Vector3 pos, Vector3 normal, float sideSign)
        {
            Vector3 offsetPos = pos + normal * _settings.ClipPlaneOffset;
            Vector3 cpos = worldToCameraMatrix.MultiplyPoint(offsetPos);
            Vector3 cnormal = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }

        private void MakeProjectionMatrixOblique(ref Matrix4x4 matrix, Vector4 clipPlane)
        {
            Vector4 q;

            // Calculate the clip-space corner point opposite the clipping plane
            // as (sgn(clipPlane.x), sgn(clipPlane.y), 1, 1) and
            // transform it into camera space by multiplying it
            // by the inverse of the projection matrix

            q.x = (sgn(clipPlane.x) + matrix[8]) / matrix[0];
            q.y = (sgn(clipPlane.y) + matrix[9]) / matrix[5];
            q.z = -1.0F;
            q.w = (1.0F + matrix[10]) / matrix[14];

            // Calculate the scaled plane vector
            Vector4 c = clipPlane * (2.0F / Vector3.Dot(clipPlane, q));

            // Replace the third row of the projection matrix
            matrix[2] = c.x;
            matrix[6] = c.y;
            matrix[10] = c.z + 1.0F;
            matrix[14] = c.w;
        }

        // Extended sign: returns -1, 0 or 1 based on sign of a
        private float sgn(float a)
        {
            if (a > 0.0f) return 1.0f;
            if (a < 0.0f) return -1.0f;
            return 0.0f;
        }

        public void Draw(RendererManager rendererManager,
            RenderTextureManager renderTextureManager,
            ScriptableRenderContext context,
            CameraShadeMultiManager manager, 
            StereoTargetEyeMask stereoTargetEyeMask, bool invert, bool renderCam)
        {
            if (_settings.IsDebug) Debug.Log("Drawing frame: " + Time.frameCount + " for " + rendererManager.GetFirst().name +
                                             " for " + stereoTargetEyeMask);

            //todo: is it necessary for none-VR?
            //_mirrorCamera.stereoTargetEye = stereoTargetEyeMask;
            //_mainCamera.stereoTargetEye = stereoTargetEyeMask; //todo: needed?

            // find out the reflection plane: position and normal in world space
            Vector3 pos = rendererManager.GetPosition();
            Vector3 normal = rendererManager.GetNormal(invert);

            // Reflect camera around reflection plane
            float d = -Vector3.Dot(normal, pos) - _settings.ClipPlaneOffset;
            Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

            Matrix4x4 reflection = Matrix4x4.zero;
            CalculateReflectionMatrix(ref reflection, reflectionPlane);

            Matrix4x4 worldToCameraMatrix = _mainCamera.worldToCameraMatrix * reflection;
            if (stereoTargetEyeMask != StereoTargetEyeMask.None)
            {
                //Thanks to "SJAM" :)
                //Debug.Log("_mainCamera.stereoSeparation: "+ _mainCamera.stereoSeparation);
                //float fix = _mainCamera.stereoSeparation != 0 ? _mainCamera.stereoSeparation + 0.01F : .032F; //for normal 64 mm IPD
                //float fix = _mainCamera.stereoSeparation != 0 ? _mainCamera.stereoSeparation + 0.012F : .034F; //for wide 68 mm IPD
                //float fix = _mainCamera.stereoSeparation != 0 ? _mainCamera.stereoSeparation + 0.007F : .029F; //for narrow 64 mm IPD
                //default IPD is 0.064 for the Unity, so we add half of it to fix camera matrix, but is that true on all cases?
                //stereoseperation is coming 0.022 for unity cam! what the :)
                //for IPD 64 is fairly right for middle seperation, but for narrow or wide seperation fix value should be set slightly
                //different, but unity is not giving me that!
                //IPD Range   Lens Spacing Setting
                //61 mm or smaller    1(narrowest, 58 mm)
                //61 mm to 66 mm  2(middle, 63mm)
                //66mm or larger  3(widest, 68mm)

                //todo: move to new nodelist
#pragma warning disable 618
                var distance = Vector3.Distance(InputTracking.GetLocalPosition(XRNode.LeftEye),
                    InputTracking.GetLocalPosition(XRNode.RightEye));
#pragma warning restore 618
                //so our fix should be half of this separation! //todo? is that breaks performance?
                var fix = distance / 2;

                worldToCameraMatrix[12] += stereoTargetEyeMask == StereoTargetEyeMask.Left ? fix : -fix;
            }

            _mirrorCamera.transform.SetPositionAndRotation(_mainCamera.transform.position, _mainCamera.transform.rotation);

            _mirrorCamera.worldToCameraMatrix = worldToCameraMatrix;

            // Setup oblique projection matrix so that near plane is our reflection
            // plane. This way we clip everything below/above it for free.
            Vector4 clipPlane = CameraSpacePlane(worldToCameraMatrix, pos, normal, invert ? 1.0f : -1.0f);

            Matrix4x4 projectionMatrix = _mainCamera.projectionMatrix;
            if (stereoTargetEyeMask != StereoTargetEyeMask.None)
            {
                projectionMatrix = _mainCamera.GetStereoProjectionMatrix(
                    stereoTargetEyeMask == StereoTargetEyeMask.Left
                        ? Camera.StereoscopicEye.Left
                        : Camera.StereoscopicEye.Right);
            }

            if (_mirrorCamera.orthographic)
            {
                projectionMatrix = _mainCamera.CalculateObliqueMatrix(clipPlane);
            }
            else
            {
                MakeProjectionMatrixOblique(ref projectionMatrix, clipPlane);
            }
            _mirrorCamera.projectionMatrix = projectionMatrix;

            var oldInvertCulling = GL.invertCulling;
            GL.invertCulling = invert;

            if (renderCam)
            {
                //set targets
                if (stereoTargetEyeMask != StereoTargetEyeMask.Right || _settings.ClipEye > 0)
                    _mirrorCamera.targetTexture = renderTextureManager.LeftOrCenterRT1;
                else
                    _mirrorCamera.targetTexture = renderTextureManager.RightRT1;
                //_mirrorCamera.stereoSeparation = 0.064F;
                UniversalRenderPipeline.RenderSingleCamera(manager == null ? context : manager._context, _mirrorCamera);

                if (_settings.EnableDepth || _settings.EnableDepthBlur)
                {
                    //set targets
                    if (stereoTargetEyeMask != StereoTargetEyeMask.Right)
                        _mirrorCamera.targetTexture = renderTextureManager.LeftOrCenterDepthRT1;
                    else
                        _mirrorCamera.targetTexture = renderTextureManager.RightDepthRT1;
                    //_mirrorCamera.stereoSeparation = 0.064F;
                    UniversalRenderPipeline.RenderSingleCamera(manager == null ? context : manager._context, _mirrorCamera);
                }
            }

            GL.invertCulling = oldInvertCulling;

            if (_settings.EnableDepthBlur)
            {
                if (_settings.DepthBlurMaterial == null)
                {
                    _settings.DepthBlurMaterial = new Material(_settings.DepthBlurShader);
                    _settings.DepthBlurMaterial.hideFlags = HideFlags.HideAndDontSave;
                }
            }

            if (_settings.EnableDepthBlur)
            {
                //Debug.Log("blitting depth..");
                //hold texture1 unchanged!!
                if (stereoTargetEyeMask != StereoTargetEyeMask.Right)
                    Graphics.Blit(renderTextureManager.LeftOrCenterRT1,
                        renderTextureManager.LeftOrCenterRT2);
                else
                    Graphics.Blit(renderTextureManager.RightRT1,
                        renderTextureManager.RightRT2);

                //if (TextureLODLevel > 0)
                //{
                //    if (_stereoEye <= 0)
                //        _reflectionTexture2.GenerateMips();
                //    else
                //        _reflectionTexture2Other.GenerateMips();
                //}

                for (int i = 1; i <= _settings.DepthBlurIterations; i++)
                {
                    //if (stereoTargetEyeMask != StereoTargetEyeMask.Right)
                    //{
                    //    if(i % 2 == 1)
                    //        _settings.DepthBlurMaterial.SetTexture("_MainTex", renderTextureManager.GetLeftOrCenterRenderTexture(2));
                    //    else
                    //        _settings.DepthBlurMaterial.SetTexture("_MainTex", renderTextureManager.GetLeftOrCenterRenderTexture(3));
                    //}
                    //else
                    //{
                    //    if (i % 2 == 1)
                    //        _settings.DepthBlurMaterial.SetTexture("_MainTex", renderTextureManager.GetRightRenderTexture(2));
                    //    else
                    //        _settings.DepthBlurMaterial.SetTexture("_MainTex", renderTextureManager.GetRightRenderTexture(3));
                    //}

                    if (_settings.DepthBlurMaterial.HasProperty("_Iteration"))
                        _settings.DepthBlurMaterial.SetFloat("_Iteration", i);

                    if (_settings.DepthBlurMaterial.HasProperty("_DepthTex"))
                        _settings.DepthBlurMaterial.SetTexture("_DepthTex", renderTextureManager.LeftOrCenterDepthRT1);

                    if (_settings.DepthBlurMaterial.HasProperty("_DepthTex") && stereoTargetEyeMask == StereoTargetEyeMask.Right)
                        _settings.DepthBlurMaterial.SetTexture("_DepthTex", renderTextureManager.RightDepthRT1);

                    //if (_depthBlurMaterial.HasProperty("_Lod"))
                    //    _depthBlurMaterial.SetFloat("_Lod", TextureLODLevel);
                    if (_settings.DepthBlurMaterial.HasProperty("_DepthCutoff"))
                        _settings.DepthBlurMaterial.SetFloat("_DepthCutoff", _settings.DepthBlurCutoff);
                    if (_settings.DepthBlurMaterial.HasProperty("_SurfacePower"))
                        _settings.DepthBlurMaterial.SetFloat("_SurfacePower", _settings.DepthBlurSurfacePower);
                    if (_settings.DepthBlurMaterial.HasProperty("_VerticalBlurMultiplier"))
                        _settings.DepthBlurMaterial.SetFloat("_VerticalBlurMultiplier", _settings.DepthBlurVerticalMultiplier);
                    if (_settings.DepthBlurMaterial.HasProperty("_HorizontalBlurMultiplier"))
                        _settings.DepthBlurMaterial.SetFloat("_HorizontalBlurMultiplier", _settings.DepthBlurHorizontalMultiplier);
                    if (_settings.DepthBlurMaterial.HasProperty("_NearClip"))
                        _settings.DepthBlurMaterial.SetFloat("_NearClip", _mirrorCamera.nearClipPlane);
                    if (_settings.DepthBlurMaterial.HasProperty("_FarClip"))
                        _settings.DepthBlurMaterial.SetFloat("_FarClip", _mirrorCamera.farClipPlane);

                    if (i % 2 == 1) //a little hack to copy textures in order from 1 to 2 than 2 to 1 and so :)
                    {
                        if (stereoTargetEyeMask != StereoTargetEyeMask.Right)
                            Graphics.Blit(renderTextureManager.LeftOrCenterRT2,
                                renderTextureManager.LeftOrCenterRT3, _settings.DepthBlurMaterial);
                        else
                            Graphics.Blit(renderTextureManager.RightRT2,
                                renderTextureManager.RightRT3, _settings.DepthBlurMaterial);
                        //if (TextureLODLevel > 0)
                        //{
                        //    if (_stereoEye <= 0)
                        //        _reflectionTexture3.GenerateMips();
                        //    else
                        //        _reflectionTexture3Other.GenerateMips();
                        //}
                    }
                    else
                    {
                        if (stereoTargetEyeMask != StereoTargetEyeMask.Right)
                            Graphics.Blit(renderTextureManager.LeftOrCenterRT3,
                                renderTextureManager.LeftOrCenterRT2, _settings.DepthBlurMaterial);
                        else
                            Graphics.Blit(renderTextureManager.RightRT3,
                                renderTextureManager.RightRT2, _settings.DepthBlurMaterial);
                        //if (TextureLODLevel > 0)
                        //{
                        //    if (_stereoEye <= 0)
                        //        _reflectionTexture2.GenerateMips();
                        //    else
                        //        _reflectionTexture2Other.GenerateMips();
                        //}
                    }
                }
            }

            if (renderTextureManager.GetSettings().MipMapping)
                renderTextureManager.GenerateMipMaps(stereoTargetEyeMask);

            if (_settings.IsDebug) Debug.Log("Finished frame: " + Time.frameCount + " for " + rendererManager.GetFirst().name +
                                             " for " + stereoTargetEyeMask);
        }

        public override void Destroy()
        {
            _mirrorCamera = null;
            _mirrorCameraContainer = null;
        }
    }

    public class OptionManager : ManagerBase
    {
        private bool _previousFog;
        private int _previousLODLevel;
        private int _previousMaxAdditionalLightsCount;
        private OptionSettings _settings;

        public struct OptionSettings
        {
            public bool IsDebug { get; set; }
            public int LODLevel { get; set; }
            public bool DisablePixelLights { get; set; }
        }

        public OptionManager(OptionSettings settings)
        {
            SetSettings(settings);
        }

        private void SetSettings(OptionSettings settings)
        {
            _settings = settings;
        }

        public void Start()
        {
            //Debug.Log("Option manager start called...");

            //do not draw fog
            _previousFog = RenderSettings.fog;
            RenderSettings.fog = false;

            // Optionally disable pixel lights for reflection/refraction
            if (_settings.DisablePixelLights)
            {
                //save previous
                _previousMaxAdditionalLightsCount =
                    ((UniversalRenderPipelineAsset)GraphicsSettings.defaultRenderPipeline).maxAdditionalLightsCount;
                //disable
                ((UniversalRenderPipelineAsset)GraphicsSettings.defaultRenderPipeline).maxAdditionalLightsCount = 0;
            }

            //set lod level
            _previousLODLevel = QualitySettings.maximumLODLevel;
            QualitySettings.maximumLODLevel = _settings.LODLevel;
        }

        public void End()
        {
            //Debug.Log("Option manager end called...");

            RenderSettings.fog = _previousFog;

            if (_settings.DisablePixelLights)
            {
                //revert
                ((UniversalRenderPipelineAsset)GraphicsSettings.defaultRenderPipeline).maxAdditionalLightsCount =
                    _previousMaxAdditionalLightsCount;
            }

            QualitySettings.maximumLODLevel = _previousLODLevel;
        }

        public override void Destroy()
        {

        }
    }
}