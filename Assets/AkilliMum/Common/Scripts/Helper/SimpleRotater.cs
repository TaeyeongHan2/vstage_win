using UnityEngine;
using System.Collections;

namespace AkilliMum
{
    public class SimpleRotater : MonoBehaviour
    {
        public Vector3 direction = Vector3.right;
        public float Speed = 5;

        void Update()
        {
            gameObject.transform.RotateAround(gameObject.transform.position,
                direction,
                Speed * Time.deltaTime);
        }
    }
}
