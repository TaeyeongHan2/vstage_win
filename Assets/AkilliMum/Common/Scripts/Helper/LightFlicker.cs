using UnityEngine;
using System.Collections;

// ReSharper disable once CheckNamespace
namespace AkilliMum
{
    [RequireComponent(typeof(Light))]
    public class LightFlicker : MonoBehaviour
    {
        public float Time = 0.05f;

        private float _timer;
        private Light _light;

        // ReSharper disable once UnusedMember.Local
        void Start()
        {
            _light = GetComponent<Light>();
            _timer = Time;
            StartCoroutine("Flicker");
        }

        // ReSharper disable once UnusedMember.Local
        IEnumerator Flicker()
        {
            while (true)
            {
                _light.enabled = _light.enabled;

                do
                {
                    _timer -= UnityEngine.Time.deltaTime;
                    yield return null;
                } while (_timer > 0);

                _timer = Time;
            }
        }
    }
}