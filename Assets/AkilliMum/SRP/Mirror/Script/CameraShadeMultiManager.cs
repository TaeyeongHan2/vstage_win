//#define DEBUG_RENDER
//#define DEBUG_LOG

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.Rendering;

// ReSharper disable UnusedMember.Local
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace AkilliMum.SRP.Mirror
{
    //[ImageEffectAllowedInSceneView]
    //[ExecuteInEditMode]
    public class CameraShadeMultiManager : MonoBehaviour
    {
        IList<CameraShade> _originalCameraShades = new List<CameraShade>();

        CameraShade[,] _copyCameraShades;
        IList<RenderTexture>[,] _copyTextures;

        [Tooltip("Mirror in mirror recursive count")]
        [Range(1, 10)]
        public int Depth = 2;
        [Tooltip("Please use this to give unique id's to mirrors which will be drawn together. So if you want to see a mirror inside another mirror, their id must be same!")]
        public string MirrorInMirrorId;
        [Tooltip("Decreases the size of the reflection 2 over X times for each depth, so you may gain performance but may lose reality!")]
        [Range(0, 10)]
        public float DecreaseSize2OverXTimes = 1;
        [Tooltip("Draws shadows only for first X depth (if applicable on real mirror, if it is not; if will disable it anyway)!")]
        [Range(1, 10)]
        public int ShadowDepth = 1;
        [Tooltip("Disables the MSAA after Xth depth (if applicable on real mirror, if it is not; if will disable it anyway)!")]
        [Range(1, 10)]
        public int DisableMSAAAfterXthDepth = 1;
        [Tooltip("Disables the pixel lights after Xth depth (if applicable on real mirror, if it is not; if will disable it anyway)!")]
        [Range(1, 10)]
        public int DisablePixelLightsAfterXthDepth = 1;

        [NonSerialized]
        public Camera _camera;
        [NonSerialized]
        public ScriptableRenderContext _context;

        private void OnEnable()
        {
            InitializeProperties();

            RenderPipelineManager.beginCameraRendering += ExecuteBeforeCameraRender;
        }

        // Cleanup all the objects we possibly have created
        void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= ExecuteBeforeCameraRender;
        }

        public void InitializeProperties()
        {
            //get originals
            _originalCameraShades = GetComponents<CameraShade>();
            _originalCameraShades = _originalCameraShades
             .Where(a => a.IsMirrorInMirror && a.MirrorInMirrorId == MirrorInMirrorId).ToList();
            //create N * Depth script copy
            _copyCameraShades = new CameraShade[_originalCameraShades.Count, Depth];
            _copyTextures = new IList<RenderTexture>[_originalCameraShades.Count, Depth];

            for (int camIndex = 0; camIndex < _originalCameraShades.Count; camIndex++)
            {
                for (int depth = 0; depth < Depth; depth++)
                {
                    var copy = gameObject.AddComponent<CameraShade>();
#if DEBUG_LOG
                    //Debug.Log("found a mirror in mirror camera shade. adding to list...");
#endif
                    _originalCameraShades[camIndex].CopyTo(copy);
                    copy.InitializeProperties();

                    //get the real size of original
                    var size = _originalCameraShades[camIndex].GetTextureSizes()[0];
                    //set the copy to manual
                    copy.TextureSize = TextureSizeType.Manual;
                    //decrease the size on each depth; depth/1, depth/2, depth/3 etc
                    copy.ManualSize = size / (int)Mathf.Pow(2, DecreaseSize2OverXTimes * depth);
                    copy.ManualSize = copy.ManualSize + copy.ManualSize % 2;
                    if (copy.ManualSize <= 128)
                        copy.ManualSize = 128;

                    //shadows?
                    if (_originalCameraShades[camIndex].Shadow == true && ShadowDepth > depth)
                    {
                        copy.Shadow = true;
                    }
                    else //do not draw shadows for depth > 1
                    {
                        copy.Shadow = false;
                        //copy.ShadowDistance = 0;
                    }

                    //MSAA
                    if (_originalCameraShades[camIndex].MSAA && DisableMSAAAfterXthDepth > depth)
                    {
                        copy.MSAA = true;
                    }
                    else
                    {
                        copy.MSAA = false;
                    }

                    //Lights
                    if (_originalCameraShades[camIndex].DisablePixelLights == false && DisablePixelLightsAfterXthDepth > depth)
                    {
                        copy.DisablePixelLights = false;
                    }
                    else
                    {
                        copy.DisablePixelLights = true;
                    }

                    _copyCameraShades[camIndex, depth] = copy;
                }
            }

        }

        public void ExecuteBeforeCameraRender(
           ScriptableRenderContext context,
           Camera cameraSrp)
        {
            _camera = cameraSrp;

            _context = context;

            RenderReflective();
        }

        int GetNextCamIndex(int camIndex)
        {
            camIndex += 1;
            if (camIndex >= _originalCameraShades.Count)
                return 0;
            return camIndex;
        }

        Camera[] cameraList;
        public void RenderReflective()
        {
            if (_originalCameraShades.Count < 3)
            {
                //for each script in scene (which are mirror in mirror)
                for (int eachCam = 0; eachCam < _originalCameraShades.Count; eachCam++)
                {
                    //continue if mirror is not visible! (other mirrors will draw this one even if it is not visible anyway :) )
                    if (!_originalCameraShades[eachCam].IsObjectVisible(_camera))
                    {
#if DEBUG_LOG
                    Debug.Log(eachCam + " is not visible, skipping it!");
#endif
                        continue;
                    }

                    cameraList = new Camera[Depth];

                    var nextCamIndex = eachCam;

                    cameraList[0] = null; //the main cam

                    for (int eachDepth = 1; eachDepth < Depth; eachDepth++)
                    {
                        //get camera according to last cam (reflection of it)
                        cameraList[eachDepth] = _originalCameraShades[nextCamIndex]
                            .RenderReflective(this, cameraList[eachDepth - 1], true, false)[0];
#if DEBUG_LOG
                                            Debug.Log("added cam=" + cameraList[eachDepth] + " for eachCam=" + eachCam + " eachDepth=" + eachDepth);
#endif
                        //get next cam
                        nextCamIndex = GetNextCamIndex(nextCamIndex);
                    }

                    nextCamIndex = eachCam;
                    if (Depth % 2 == 0) //if depth is odd we have to start from other cam
                        nextCamIndex = GetNextCamIndex(nextCamIndex);

                    //draw
                    for (int eachDepth = Depth - 1; eachDepth >= 0; eachDepth--)
                    {
#if DEBUG_LOG
                                            Debug.Log("draw next camIndex: " + nextCamIndex + " depth:" + eachDepth);
#endif
                        if (eachDepth == Depth - 1)
                        {
                            //do not draw anything for last depth
                            _copyCameraShades[nextCamIndex, eachDepth].ReflectLayers = -1;
                            //set intensity 0, so we draw only material texture (or material color) for nice display
                            _copyCameraShades[nextCamIndex, eachDepth].Intensity = 0;
                            //transparency may seem bad on last depth, so draw it as reflected always
                            _copyCameraShades[nextCamIndex, eachDepth].WorkType = WorkType.Reflect;
                            //enable g buffer, because real mirror (intensity 1) may bad results for last depth
                            _copyCameraShades[nextCamIndex, eachDepth].DisableGBuffer = false;
                            //disable probes anyway :)
                            //_copyCameraShades[nextCamIndex, eachDepth].DisableRProbes = true;
                        }
                        _copyCameraShades[nextCamIndex, eachDepth]
                          .RenderReflective(this, cameraList[eachDepth], nextCamIndex == eachCam);

                        //get next cam
                        nextCamIndex = GetNextCamIndex(nextCamIndex);
                    }

                    //break; //todo: break on first loop, remove later

                    //get the latest (true) draw of the cam
                    _copyTextures[eachCam, 0] = _copyCameraShades[eachCam, 0].CopyTextures();
                }

                //set latest (true) draws
                for (int eachCam = 0; eachCam < _originalCameraShades.Count; eachCam++)
                {
                    _copyCameraShades[eachCam, 0].PasteTextures(_copyTextures[eachCam, 0]);
                    _copyCameraShades[eachCam, 0].UpdateMaterialProperties(_camera);
                }
            }
            else
            {
                //for each script in scene (which are mirror in mirror)
                for (int eachCam = 0; eachCam < _originalCameraShades.Count; eachCam++)
                {
                    //continue if mirror is not visible! (other mirrors will draw this one even if it is not visible anyway :) )
                    if (!_originalCameraShades[eachCam].IsObjectVisible(_camera))
                    {
#if DEBUG_LOG
                        Debug.Log(eachCam + " is not visible, skipping it!");
#endif
                        continue;
                    }

                    cameraList = new Camera[_originalCameraShades.Count + 1];
                    var nextCam = eachCam;
                    int order = 0;
                    cameraList[order] = null;
                    order++;
                    //get the reflected cam for first mirror //DO NOT RENDER!
                    cameraList[order] = _originalCameraShades[eachCam].RenderReflective(this, null, true, false)[0];


                    //draw fakes recursion
                    for (int disableCam = 0; disableCam < _originalCameraShades.Count; disableCam++)
                    {
                        var camera = _originalCameraShades[disableCam].RenderReflective(this, cameraList[1], true, false)[0];

                        //do not draw anything for last depth
                        _copyCameraShades[disableCam, 2].ReflectLayers = -1;
                        //set intensity 0, so we draw only material texture (or material color) for nice display
                        _copyCameraShades[disableCam, 2].Intensity = 0;
                        //transparency may seem bad on last depth, so draw it as reflected always
                        _copyCameraShades[disableCam, 2].WorkType = WorkType.Reflect;
                        //enable g buffer, because real mirror (intensity 1) may bad results for last depth
                        _copyCameraShades[disableCam, 2].DisableGBuffer = false;
                        //disable probes anyway :)
                        //_copyCameraShades[disableCam, 2].DisableRProbes = true;

                        _copyCameraShades[disableCam, 2].RenderReflective(this, camera, true, true);
                        _copyTextures[disableCam, 2] = _copyCameraShades[disableCam, 2].CopyTextures();

                    }


                    //draw first recursion
                    nextCam = eachCam;
                    for (int toDrawCam = 0; toDrawCam < _originalCameraShades.Count; toDrawCam++)
                    {
                        if (eachCam == toDrawCam)
                        {
                            continue;
                        }

                        nextCam = GetNextCamIndex(nextCam);

                        //set fake textures
                        for (int disableCam = 0; disableCam < _originalCameraShades.Count; disableCam++)
                        {
                            _copyCameraShades[disableCam, 2].PasteTextures(_copyTextures[disableCam, 2]);
                            _copyCameraShades[disableCam, 2].UpdateMaterialProperties(_camera);
                        }

                        //draw first recursion
                        _copyCameraShades[nextCam, 1].RenderReflective(this, cameraList[1], false, true);
                        _copyTextures[nextCam, 1] = _copyCameraShades[nextCam, 1].CopyTextures();
                    }

                    //paste first recursion real textures
                    for (int toDrawCam = 0; toDrawCam < _originalCameraShades.Count; toDrawCam++)
                    {
                        _copyCameraShades[toDrawCam, 1].PasteTextures(_copyTextures[toDrawCam, 1]);
                        _copyCameraShades[toDrawCam, 1].UpdateMaterialProperties(_camera);
                    }

                    //real one
                    _copyCameraShades[eachCam, 0].RenderReflective(this, null, true, true);
                    //get the latest (true) draw of the cam and copy the textures
                    _copyTextures[eachCam, 0] = _copyCameraShades[eachCam, 0].CopyTextures();
                }

                //set latest (true) draws from textures
                for (int eachCam = 0; eachCam < _originalCameraShades.Count; eachCam++)
                {
                    _copyCameraShades[eachCam, 0].PasteTextures(_copyTextures[eachCam, 0]);
                    _copyCameraShades[eachCam, 0].UpdateMaterialProperties(_camera);
                }
            }

        }
    }

    public class Container
    {
        public Camera Camera { get; set; }
        public CameraShade CameraShade { get; set; }
    }
}