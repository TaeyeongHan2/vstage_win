
//using System;
//using UnityEngine;
//using UnityEditor;
//using System.IO;
//using UnityEngine.Experimental.Rendering;

//namespace AkilliMum.SRP.LCRS
//{
//    public class LCRSBake : ScriptableWizard
//    {

//        public Transform renderPosition;

//        public Cubemap cubemap;

//        // Camera settings.
//        public int cameraDepth = 24;
//        public LayerMask cameraLayerMask = -1;
//        public Color cameraBackgroundColor;
//        public float cameraNearPlane = 0.1f;
//        public float cameraFarPlane = 2500.0f;

//        public bool cameraUseOcclusion = true;

//        // Cubemap settings.
//        public FilterMode cubemapFilterMode = FilterMode.Trilinear;

//        // Quality settings.
//        public int antiAliasing = 4;

//        public bool createIndividualImages = false;

//        // The folder where individual cubemap images will be saved
//        static string imageDirectory = "Assets/CubemapImages";

//        static string[] cubemapImage
//            = new string[] {"front+Z", "right+X", "back-Z", "left-X", "top+Y", "bottom-Y"};

//        static Vector3[] eulerAngles = new Vector3[]
//        {
//            new Vector3(0.0f, 0.0f, 0.0f),
//            new Vector3(0.0f, -90.0f, 0.0f), new Vector3(0.0f, 180.0f, 0.0f),
//            new Vector3(0.0f, 90.0f, 0.0f), new Vector3(-90.0f, 0.0f, 0.0f),
//            new Vector3(90.0f, 0.0f, 0.0f)
//        };

//        private Camera _camera;

//        void OnWizardUpdate()
//        {
//            helpString = "Set the position to render from and the cubemap to bake.";
//            if (renderPosition != null && cubemap != null)
//            {
//                isValid = true;
//            }
//            else
//            {
//                isValid = false;
//            }
//        }

//        void OnWizardCreate()
//        {

//            // Create temporary camera for rendering.
//            GameObject go = new GameObject("CubemapCam", typeof(Camera));
//            _camera = go.GetComponent<Camera>();
//            // Camera settings. 
//            _camera.depth = cameraDepth;
//            _camera.backgroundColor = cameraBackgroundColor;
//            _camera.cullingMask = cameraLayerMask;
//            _camera.nearClipPlane = cameraNearPlane;
//            _camera.farClipPlane = cameraFarPlane;
//            _camera.useOcclusionCulling = cameraUseOcclusion;
//            // Cubemap settings
//            cubemap.filterMode = cubemapFilterMode;
            
//            // Set antialiasing
//            QualitySettings.antiAliasing = antiAliasing;

//            // Place the camera on the render position.
//            go.transform.position = renderPosition.position;
//            go.transform.rotation = renderPosition.rotation; // Quaternion.identity;

//            // Bake the cubemap
//            var previousPos = Camera.main.transform.position;
//            var previousRot = Camera.main.transform.rotation;
//            Camera.main.transform.position = renderPosition.position;
//            Camera.main.transform.rotation = renderPosition.rotation;
//            try
//            {
//                Camera.main.RenderToCubemap(cubemap);
//            }
//            finally
//            {
//                Camera.main.transform.position = previousPos;
//                Camera.main.transform.rotation = previousRot;
//            }
            

//            // Rendering individual images
//            if (createIndividualImages)
//            {
//                if (!Directory.Exists(imageDirectory))
//                {
//                    Directory.CreateDirectory(imageDirectory);
//                }

//                RenderIndividualCubemapImages(go);
//            }


//            // Destroy the camera after rendering.
//            DestroyImmediate(go);
//        }

//        void RenderIndividualCubemapImages(GameObject go)
//        {
//            _camera.backgroundColor = Color.black;
//            _camera.clearFlags = CameraClearFlags.Skybox;
//            _camera.fieldOfView = 90;
//            //_camera.fieldOfView = Camera.main.fieldOfView; // 90;
//            _camera.aspect = 1.0f;
//            //_camera.aspect = Camera.main.aspect; //1.0f;

//            _camera.transform.position = renderPosition.position;
//            //go.transform.rotation = Quaternion.identity;

//            //Render individual images        
//            for (int camOrientation = 0; camOrientation < eulerAngles.Length; camOrientation++)
//            {
//                string imageName = Path.Combine(imageDirectory, cubemap.name + "_"
//                                                                             + cubemapImage[camOrientation] + ".png");
//                //_camera.transform.eulerAngles = eulerAngles[camOrientation] + go.transform.localRotation.eulerAngles;
//                if(camOrientation == 0)
//                    _camera.transform.SetPositionAndRotation(renderPosition.position, renderPosition.rotation);
//                else if (camOrientation == 1) //use the camera after 0 pass :) turn right 90 degree to the right :)
//                    _camera.transform.Rotate(_camera.transform.up, 90);
//                else if (camOrientation == 2)
//                    _camera.transform.Rotate(_camera.transform.up, 90);
//                else if (camOrientation == 3)
//                    _camera.transform.Rotate(_camera.transform.up, 90);
//                else if (camOrientation == 4)
//                {
//                    _camera.transform.Rotate(_camera.transform.up, 90); //will come to start position
//                    _camera.transform.Rotate(_camera.transform.right, 90); //will come to start position
//                }
//                else if (camOrientation == 5)
//                    _camera.transform.LookAt(-renderPosition.up);

//                RenderTexture renderTex = new RenderTexture(cubemap.height,
//                    cubemap.height, cameraDepth, DefaultFormat.HDR);
//                _camera.targetTexture = renderTex;
//                _camera.Render();
//                RenderTexture.active = renderTex;

//                Texture2D img = new Texture2D(cubemap.height, cubemap.height,
//                    TextureFormat.RGB24, false);
//                img.ReadPixels(new Rect(0, 0, cubemap.height, cubemap.height), 0, 0);

//                RenderTexture.active = null;
//                GameObject.DestroyImmediate(renderTex);

//                byte[] imgBytes = img.EncodeToPNG();
//                File.WriteAllBytes(imageName, imgBytes);

//                AssetDatabase.ImportAsset(imageName, ImportAssetOptions.ForceUpdate);
//            }

//            AssetDatabase.Refresh();
//        }

//        [MenuItem("GameObject/Bake Cubemap")]
//        static void RenderCubemap()
//        {
//            ScriptableWizard.DisplayWizard("Bake CubeMap", typeof(LCRSBake), "Bake!");
//        }
//    }
//}