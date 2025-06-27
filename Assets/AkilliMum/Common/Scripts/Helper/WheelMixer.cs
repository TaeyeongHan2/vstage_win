using System;
using UnityEngine;
using System.Collections;

namespace AkilliMum
{
    public class WheelMixer : MonoBehaviour
    {
        public bool activate = true;
        public GameObject[] wheels;
        public Vector3 direction = Vector3.right;
        public float speed = 100;
        public GameObject[] blurredWheels;
        public GameObject tireAndRotor;
        public Texture2D[] textures;

        void Update()
        {
            if (wheels != null &&
                wheels.Length > 0)
            {
                foreach (var t in wheels)
                {
                    t.transform.rotation = Quaternion.Lerp(t.transform.rotation,
                        t.transform.rotation * Quaternion.Euler(direction), speed * Time.deltaTime);
                }
            }
        }

        public const string BaseMap = "_Gloss";
        public const string BumpMap = "_Normal";
        public const string OcclusionMap = "_Occlusion";
        void FixedUpdate()
        {
            if (wheels != null &&
                wheels.Length > 0 &&
                blurredWheels != null &&
                blurredWheels.Length > 0 &&
                tireAndRotor != null &&
                textures != null &&
                textures.Length >= 15)
            {
                //foreach (var w in wheels)
                //{
                //    w.SetActive(false);
                //}
                foreach (var b in blurredWheels)
                {
                    b.SetActive(false);
                }

                if (!activate)
                {
                    blurredWheels[0].SetActive(true);
                    foreach (var mat in tireAndRotor.GetComponent<Renderer>().materials)
                    {
                        mat.SetTexture(BaseMap, textures[0]);
                        mat.SetTexture(BumpMap, textures[1]);
                        mat.SetTexture(OcclusionMap, textures[2]);
                    }
                    
                    return;
                }

                if (speed <= 200)
                {
                    blurredWheels[0].SetActive(true);
                    foreach (var mat in tireAndRotor.GetComponent<Renderer>().materials)
                    {
                        mat.SetTexture(BaseMap, textures[0]);
                        mat.SetTexture(BumpMap, textures[1]);
                        mat.SetTexture(OcclusionMap, textures[2]);
                    }
                }
                else if (speed > 200 && speed <= 500)
                {
                    blurredWheels[1].SetActive(true);
                    foreach (var mat in tireAndRotor.GetComponent<Renderer>().materials)
                    {
                        mat.SetTexture(BaseMap, textures[3]);
                        mat.SetTexture(BumpMap, textures[4]);
                        mat.SetTexture(OcclusionMap, textures[5]);
                    }
                }
                else if (speed > 500 && speed <= 800)
                {
                    //blurredWheels[0].SetActive(true);
                    blurredWheels[2].SetActive(true);
                    foreach (var mat in tireAndRotor.GetComponent<Renderer>().materials)
                    {
                        mat.SetTexture(BaseMap, textures[6]);
                        mat.SetTexture(BumpMap, textures[7]);
                        mat.SetTexture(OcclusionMap, textures[8]);
                    }
                }
                else if (speed > 800 && speed <= 1200)
                {
                    //blurredWheels[0].SetActive(true);
                    //blurredWheels[1].SetActive(true);
                    blurredWheels[3].SetActive(true);
                    foreach (var mat in tireAndRotor.GetComponent<Renderer>().materials)
                    {
                        mat.SetTexture(BaseMap, textures[9]);
                        mat.SetTexture(BumpMap, textures[10]);
                        mat.SetTexture(OcclusionMap, textures[11]);
                    }
                }
                else if (speed > 1200)
                {
                    //blurredWheels[0].SetActive(true);
                    //blurredWheels[1].SetActive(true);
                    //blurredWheels[2].SetActive(true);
                    blurredWheels[4].SetActive(true);
                    foreach (var mat in tireAndRotor.GetComponent<Renderer>().materials)
                    {
                        mat.SetTexture(BaseMap, textures[12]);
                        mat.SetTexture(BumpMap, textures[13]);
                        mat.SetTexture(OcclusionMap, textures[14]);
                    }
                }
            }
        }

    }
}
