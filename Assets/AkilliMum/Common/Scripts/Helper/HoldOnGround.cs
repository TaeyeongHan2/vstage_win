using UnityEngine;

// ReSharper disable once CheckNamespace
namespace AkilliMum
{
    /// <summary>
    /// To hold item on ground
    /// </summary>
    public class HoldOnGround : MonoBehaviour
    {
        [Tooltip("Hold the Y position always fixed on this value")]
        public float YPosition = 0;

        // ReSharper disable once UnusedMember.Local
        void FixedUpdate()
        {
            transform.position = new Vector3(transform.position.x, YPosition, transform.position.z);
        }
    }
}