using UnityEngine;
using System.Collections;
//using UnityStandardAssets.CinematicEffects;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

namespace AkilliMum.SRP.Mirror
{
    public class MainGui : MonoBehaviour
    {
        public Light Light;
        public Material Day;
        public Material Night;
        private bool _day = true;

        public Text ShaderName;
        public GameObject NormalFloor;
        public GameObject MirrorFloor;
        public GameObject MirrorBlurFloor;
        public GameObject MirrorBlurTexFloor;
        public GameObject MirrorRefFloor;
        public GameObject MirrorRefTexFloor;
        public GameObject MirrorTexFloorSurf;
        public GameObject MirrorBlurTexFloorSurf;
        public GameObject MirrorRefTexFloorSurf;
        public GameObject MirrorAlphaTexFloorSurf;
        public GameObject MirrorTransparent;
        public GameObject MirrorAlphaReflect;
        //public GameObject MirrorPostEffect;
        public GameObject Refraction;
        public GameObject Blur;
        public GameObject Mosaic;
        public GameObject PostEffect;
        //public GameObject LED;
        public GameObject Portal;
        //public GameObject Water;
        public GameObject Ghost;
        public GameObject Car;
        public GameObject Heli;

        public void Start()
        {
            Application.targetFrameRate = 300;
            ActivateDefault();
        }

        public void Off()
        {
            NormalFloor.SetActive(false);
            MirrorFloor.SetActive(false);
            MirrorBlurFloor.SetActive(false);
            MirrorBlurTexFloor.SetActive(false);
            MirrorRefFloor.SetActive(false);
            MirrorRefTexFloor.SetActive(false);
            MirrorTexFloorSurf.SetActive(false);
            MirrorBlurTexFloorSurf.SetActive(false);
            MirrorRefTexFloorSurf.SetActive(false);
            MirrorAlphaTexFloorSurf.SetActive(false);
            MirrorTransparent.SetActive(false);
            MirrorAlphaReflect.SetActive(false);
            //MirrorPostEffect.SetActive(false);
            Refraction.SetActive(false);
            Blur.SetActive(false);
            Mosaic.SetActive(false);
            PostEffect.SetActive(false);
            //LED.SetActive(false);
            Portal.SetActive(false);
            //Water.SetActive(false);
            Ghost.SetActive(false);
            Car.SetActive(true);
            Heli.SetActive(false);
        }

        public void ActivateShadows()
        {
            if (Light.shadows == LightShadows.None)
                Light.shadows = LightShadows.Hard;
            else
                Light.shadows = LightShadows.None;
        }

        public void ActivateDefault()
        {
            Off();
            NormalFloor.SetActive(true);
            ShaderName.text = "-";

        }

        public void SwitchSpinning()
        {
            var rotateList = FindObjectsOfType<SimpleRotaterAround>();
            foreach (var rotate in rotateList)
            {
                rotate.enabled = !rotate.enabled;
            }
        }

        public void SwitchDayNight()
        {
            if (_day)
            {
                RenderSettings.skybox = Night;
                Light.intensity = 0.1f;
            }
            else
            {
                RenderSettings.skybox = Day;
                Light.intensity = 1f;
            }
            DynamicGI.UpdateEnvironment();
            _day = !_day;
        }

        public void ActivateMirror()
        {
            NormalFloor.SetActive(false);

            if (MirrorFloor.activeSelf)
            {
                Off();
                MirrorBlurFloor.SetActive(true);
                ShaderName.text = "Mirror Blurred";
            }
            else if (MirrorBlurFloor.activeSelf)
            {
                Off();
                MirrorBlurTexFloor.SetActive(true);
                ShaderName.text = "Reflective Surface Blurred";
            }
            else if (MirrorBlurTexFloor.activeSelf)
            {
                Off();
                MirrorRefFloor.SetActive(true);
                ShaderName.text = "Refracted Mirror";
            }
            else if (MirrorRefFloor.activeSelf)
            {
                Off();
                MirrorRefTexFloor.SetActive(true);
                ShaderName.text = "Reflective Refracted Surface";
            }
            else if (MirrorRefTexFloor.activeSelf)
            {
                Off();
                MirrorTexFloorSurf.SetActive(true);
                ShaderName.text = "Reflective Surface + Lights + Shadows";
            }
            else if (MirrorTexFloorSurf.activeSelf)
            {
                Off();
                MirrorBlurTexFloorSurf.SetActive(true);
                ShaderName.text = "Reflective Blurred Surface + Lights + Shadows";
            }
            else if (MirrorBlurTexFloorSurf.activeSelf)
            {
                Off();
                MirrorRefTexFloorSurf.SetActive(true);
                ShaderName.text = "Reflective Refracted Surface + Lights + Shadows";
            }
            else if (MirrorRefTexFloorSurf.activeSelf)
            {
                Off();
                MirrorAlphaTexFloorSurf.SetActive(true);
                ShaderName.text = "(Beta) Wet Surface Simulation + Lights + Shadows";
            }
            else if (MirrorAlphaTexFloorSurf.activeSelf)
            {
                Off();
                MirrorTransparent.SetActive(true);
                ShaderName.text = "Transparent Reflection (For Augmented Reality)";
            }
            else if (MirrorTransparent.activeSelf)
            {
                Off();
                MirrorAlphaReflect.SetActive(true);
                NormalFloor.SetActive(true);
                ShaderName.text = "Transparent Reflective Glass";
            }
            //else if (MirrorTransparent.activeSelf)
            //{
            //    Off();
            //    MirrorPostEffect.SetActive(true);
            //    ShaderName.text = "Gameboy Post Effect Prefab Camera";
            //}
            else
            {
                Off();
                MirrorFloor.SetActive(true);
                ShaderName.text = "Mirror";
            }


        }

        public void ActivateRefraction()
        {
            ActivateDefault();
            Refraction.SetActive(true);
            ShaderName.text = "Refracted Glass-Ice";
        }

        public void ActivateBlur()
        {
            ActivateDefault();
            Blur.SetActive(true);
            ShaderName.text = "Blurred Glass";
        }

        public void ActivateDistortion()
        {
            ActivateDefault();
            Car.SetActive(false);
            Heli.SetActive(true);
            ShaderName.text = "Engine-Fire Heat Particles";
        }

        public void ActivateMosaic()
        {
            ActivateDefault();
            Mosaic.SetActive(true);
            ShaderName.text = "Mosaic-Censored Areas";
        }

        public void ActivatePostEffect()
        {
            ActivateDefault();
            PostEffect.SetActive(true);
            ShaderName.text = "Post Effect Camera";
        }

        //public void ActivateLED()
        //{
        //    ActivateDefault();
        //    LED.SetActive(true);
        //    ShaderName.text = "LED Tv Simulations";
        //}

        public void ActivatePortal()
        {
            ActivateDefault();
            Portal.SetActive(true);
        }

        //public void ActivateWater()
        //{
        //    Off();
        //    Water.SetActive(true);
        //}

        public void ActivateGhost()
        {
            Off();
            Ghost.SetActive(true);
            NormalFloor.SetActive(true);
            ShaderName.text = "Stealth-Ghost Simulations";
        }

        public void LoadShaderScene()
        {
            SceneManager.LoadScene("Shaders");
        }

        public void LoadEffectsScene()
        {
            SceneManager.LoadScene("PostEffects");
        }
    }
}