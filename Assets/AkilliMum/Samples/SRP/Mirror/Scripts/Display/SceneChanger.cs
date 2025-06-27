using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AkilliMum.SRP.Mirror
{
    public class SceneChanger : MonoBehaviour
    {
        private void Update()
        {
            Application.targetFrameRate = 500; //todo: do not put this on publish
        }
        public void Back()
        {
            SceneManager.LoadScene("TestScenes");
        }
        public void Blur()
        {
            SceneManager.LoadScene("Blur");
        }
        public void Mirror()
        {
            SceneManager.LoadScene("Mirror");
        }
        public void Mirror_()
        {
            SceneManager.LoadScene("Mirror_");
        }
        public void MirrorBlur()
        {
            SceneManager.LoadScene("MirrorBlur");
        }
        public void MirrorBlur_()
        {
            SceneManager.LoadScene("MirrorBlur_");
        }
        public void MirrorBlurTex()
        {
            SceneManager.LoadScene("MirrorBlurTex");
        }
        public void MirrorBlurTex_()
        {
            SceneManager.LoadScene("MirrorBlurTex_");
        }
        public void MirrorRefracted()
        {
            SceneManager.LoadScene("MirrorRefracted");
        }
        public void MirrorRefracted_()
        {
            SceneManager.LoadScene("MirrorRefracted_");
        }
        public void MirrorTex()
        {
            SceneManager.LoadScene("MirrorTex");
        }
        public void MirrorTex_()
        {
            SceneManager.LoadScene("MirrorTex_");
        }
        public void MirrorTexRefracted()
        {
            SceneManager.LoadScene("MirrorTexRefracted");
        }
        public void Mosaic()
        {
            SceneManager.LoadScene("Mosaic");
        }
        public void RefractionFlat()
        {
            SceneManager.LoadScene("RefractionFlat");
        }
        public void MirrorMask()
        {
            SceneManager.LoadScene("MirrorMask");
        }
        public void MirrorMaskRefracted()
        {
            SceneManager.LoadScene("MirrorMaskRefracted");
        }
        public void MirrorMaskRipple()
        {
            SceneManager.LoadScene("MirrorMaskRipple");
        }
        public void MirrorMaskWave()
        {
            SceneManager.LoadScene("MirrorMaskWave");
        }
        public void MirrorRipple()
        {
            SceneManager.LoadScene("MirrorRipple");
        }
        public void MirrorWave()
        {
            SceneManager.LoadScene("MirrorWave");
        }
        public void MirrorAR()
        {
            SceneManager.LoadScene("MirrorAR");
        }
        public void MirrorDepth()
        {
            SceneManager.LoadScene("MirrorDepth");
        }
        public void MirrorGlass()
        {
            SceneManager.LoadScene("MirrorGlass");
        }
    }
}