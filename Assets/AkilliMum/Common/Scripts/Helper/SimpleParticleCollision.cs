using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace AkilliMum
{
    public class SimpleParticleCollision : MonoBehaviour
    {
        public GameObject Particle;
        private ParticleSystem _particleSystem; 

        public GameObject PrefabToCreate;

        private List<ParticleCollisionEvent> _collisionEvents;

        // ReSharper disable once UnusedMember.Local
        void Start()
        {
            _particleSystem = Particle.GetComponent<ParticleSystem>();
            //var pce = new ParticleCollisionEvent{};
            
            _collisionEvents = new List<ParticleCollisionEvent>();
        }

        //void OnParticleCollision(GameObject other)
        //{
        //    int numCollisionEvents = _particleSystem.GetCollisionEvents(other, collisionEvents);
        //    Debug.Log("numCollisionEvents: " +numCollisionEvents);
        //    int i = 0;

        //    while (i < numCollisionEvents)
        //    {
        //        
        //        i++;
        //    }
        //}

        // ReSharper disable once UnusedMember.Local
        void OnParticleCollision(GameObject other)
        {
            //int safeLength = _particleSystem.GetSafeCollisionEventSize();
            //if (collisionEvents.Length < safeLength)
            //    collisionEvents = new ParticleCollisionEvent[safeLength];
            int numCollisionEvents = _particleSystem.GetCollisionEvents(other, _collisionEvents);
            int i = 0;
            while (i < numCollisionEvents)
            {
                //var hit = collisionEvents[i].intersection
                var obj = Instantiate(PrefabToCreate, _collisionEvents[i].intersection,
                    Quaternion.AngleAxis(-90, Vector3.right));
                var destroy = obj.AddComponent<DestroyObject>();
                destroy.DestroyAfterNSecond = 5f;
                //Execute(() => { DestroyImmediate(obj); }, "", 5000);
                i++;
            }
        }
    }
}