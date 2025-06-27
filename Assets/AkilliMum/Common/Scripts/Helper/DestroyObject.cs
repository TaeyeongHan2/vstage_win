using System.Collections;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace AkilliMum
{
    public class DestroyObject : MonoBehaviour
    {
        public float DestroyAfterNSecond = 0;

        // ReSharper disable once UnusedMember.Local
        void Start()
        {
            if (DestroyAfterNSecond > 0)
            {
                StartCoroutine(DestroyMe(DestroyAfterNSecond));
                //Execute(() => { DestroyMe(); }, "", DestroyAfterNSecond * 1000);
            }
        }

        public void Destroy(float timeSeconds)
        {
            //Execute(() => { DestroyMe(); }, "", timeSeconds * 1000);
            StartCoroutine(DestroyMe(timeSeconds));
        }

        IEnumerator DestroyMe(float seconds)
        {
            if (seconds > 0)
                yield return new WaitForSeconds(seconds);

            if(Application.isPlaying)
                DestroyImmediate(gameObject);
            else
                Destroy(gameObject);
        }
    }
}