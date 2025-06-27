using UnityEngine;

namespace AkilliMum
{
    public class SimpleMove : MonoBehaviour
    {
        public Vector3 Direction;
        public float Speed;
        public bool Dynamic;

        void Update()
        {
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position,
                gameObject.transform.position+Direction * Speed, Time.deltaTime);
        }

        void FixedUpdate()
        {
            if (Dynamic)
            {
                Speed += new System.Random().Next(1, 5) / 10f;
            }
        }
    }
}