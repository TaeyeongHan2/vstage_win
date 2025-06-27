using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using TMPro;

namespace AkilliMum
{
    public class FpsGui : MonoBehaviour
    {
        //public Text Text;  //to change ui
        public TextMeshProUGUI Text;

        //int frameCount = 0;
        //float dt = 0.0f;
        //float fps = 0.0f;
        //float updateRate = 2.0f;  // 4 updates per sec.

        public float updateInterval = 0.5F;

        private float accum = 0; // FPS accumulated over the interval
        private int frames = 0; // Frames drawn over the interval
        private float timeleft; // Left time for current interval
		
        private void Start()
		{
            Application.targetFrameRate = 500; //do not do that :)

            timeleft = updateInterval;  
		}

		void Update()
        {
            timeleft -= Time.deltaTime;
            accum += Time.timeScale / Time.deltaTime;
            ++frames;

            // Interval ended - update GUI text and start new interval
            if (timeleft <= 0.0)
            {

                // display two fractional digits (f2 format)
                float fps = accum / frames;
                //string format = System.String.Format("{0:F2} FPS", fps);

                //  DebugConsole.Log(format,level);
                timeleft = updateInterval;
                accum = 0.0F;
                frames = 0;
                Text.text = string.Format("{0:N1} fps", fps);
            }
        }

        //string groupName = Guid.NewGuid().ToString(); //just to group thread actions

        //void Start()
        //{
        //    Execute(() =>
        //    {
        //        CallGui();
        //    }, groupName, 500, false);
        //}

        //void CallGui()
        //{
        //    Text.text = string.Format("{0:N1} fps", fps);

        //    Execute(() =>
        //    {
        //        CallGui();
        //    }, groupName, 500, false);
        //}


        //protected override void Update()
        //{
        //    base.Update();

        //    frameCount++;
        //    dt += Time.deltaTime;
        //    if (dt > 1.0f / updateRate)
        //    {
        //        fps = frameCount / dt;
        //        //Debug.Log("fps:" + fps);
        //        frameCount = 0;
        //        dt -= 1.0f / updateRate;
        //    }
        //}
    }
}