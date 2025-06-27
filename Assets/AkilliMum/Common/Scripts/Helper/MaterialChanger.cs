using UnityEngine;
using UnityEngine.Rendering;

// ReSharper disable once CheckNamespace
namespace AkilliMum
{
    public class MaterialChanger : MonoBehaviour
    {

        public Material MaterialToChange;

        private MeshRenderer[] _renderers;

        private Material[] _materials;
        //private MirrorWithShader _mirror;

        // ReSharper disable once UnusedMember.Local
        void Start()
        {
            _renderers = GetComponentsInChildren<MeshRenderer>();

            _materials = new Material[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                _materials[i] = new Material(_renderers[i].material);
            }

            //_mirror = GetComponent<MirrorWithShader>();
        }

        public void EnableEffect()
        {
            //_mirror.enabled = true;
            foreach (var r in _renderers)
            {
                r.material = MaterialToChange;
                r.shadowCastingMode = ShadowCastingMode.Off;
            }

            var particles = GetComponentsInChildren<ParticleSystem>();
            foreach (var particle in particles)
            {
                var emis = particle.emission;
                emis.enabled = false;
            }
        }

        public void DisableEffect()
        {
            //_mirror.enabled = false;
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].material = new Material(_materials[i]);
                _renderers[i].shadowCastingMode = ShadowCastingMode.On;
            }

            var particles = GetComponentsInChildren<ParticleSystem>();
            foreach (var particle in particles)
            {
                var emis = particle.emission;
                emis.enabled = true;
            }
        }

    }
}