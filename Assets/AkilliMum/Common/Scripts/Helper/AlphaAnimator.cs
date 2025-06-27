//using UnityEngine;
//using UnityEngine.UI;

//// ReSharper disable once CheckNamespace
//namespace AkilliMum
//{
//    public class AlphaAnimator : MonoBehaviour
//    {
//        //private const string AnimateGroupName = "iYJQb9OPT8ik6htF9UDGVu2DVy7yYC";

//        private Image _image;
//        private float _blinkMultiplier = -1;
//        private float _runMilliseconds = 20;

//        // ReSharper disable once UnusedMember.Local
//        void Start()
//        {
//            _image = GetComponent<Image>();

//            Execute(() => { Animate(_runMilliseconds); }, AnimateGroupName, _runMilliseconds);
//        }

//        void Animate(float runTime)
//        {
//            //Debug.Log("Animate danger will be called!");
//            float changeAlpha = 1f / runTime;
//            var value = _image.color.a + _blinkMultiplier * changeAlpha;

//            if (value <= 0.3) //blink between 0.3 and 1
//            {
//                _blinkMultiplier = 1;
//            }
//            else if (value >= 1)
//            {
//                _blinkMultiplier = -1;
//            }


//            value = Mathf.Clamp(value, 0.3f, 1);
//            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b,
//                value);

//            //call again
//            Execute(() => { Animate(_runMilliseconds); }, AnimateGroupName, _runMilliseconds);
//        }
//    }
//}