using System;
using System.Collections;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace AkilliMum
{
    public class SimpleCreator : MonoBehaviour
    {
        public Transform CreationPosition;
        public Vector3 CreationCorrection;
        public float TimeMs = 1000;
        public GameObject ToCreate;

        void Start()
        {
            StartMe();
        }

        private void StartMe()
        {
            //repeat forever
            StartCoroutine(CreateItem());
            //Execute(CreateItem, "", TimeMs, true, Int64.MaxValue);
        }

        IEnumerator CreateItem()
        {
            yield return new WaitForSeconds(TimeMs/1000f);

            var obj = Instantiate(ToCreate, Vector3.zero, Quaternion.identity, CreationPosition.parent);
            obj.transform.position = CreationCorrection;
            var destroy = obj.AddComponent<DestroyObject>();
            destroy.DestroyAfterNSecond = 5f;
            //Execute(()=>
            //{
            //    DestroyImmediate(obj);
            //}, "", 5000);
        }
    }
}