using UnityEngine;
using System.Collections;

namespace AkilliMum
{
    public class SimpleRotaterAround : MonoBehaviour
    {

        public Vector3 Around;
        public float Angle;
        public GameObject ToRotateAround;

        void FixedUpdate()
        {
            gameObject.transform.RotateAround(ToRotateAround.transform.position,
                                              Around, Angle);
        }
    }
}
