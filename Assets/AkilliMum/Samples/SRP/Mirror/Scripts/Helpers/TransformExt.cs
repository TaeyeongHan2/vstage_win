using UnityEngine;

namespace AkilliMum.SRP.Mirror
{
    public class StoreTransform
    {
        public Vector3 position;
        public Vector3 eulerAngles;
        public Quaternion rotation;
        public Vector3 localScale;
    }

    /// <summary>
    /// Simple transform save, load extension
    /// </summary>
    public static class TransformExt
    {
        public static StoreTransform SaveLocal(this Transform aTransform)
        {
            return new StoreTransform()
            {
                position = aTransform.localPosition,
                eulerAngles = aTransform.localEulerAngles,
                rotation = aTransform.localRotation,
                localScale = aTransform.localScale
            };
        }
        public static StoreTransform SaveWorld(this Transform aTransform)
        {
            return new StoreTransform()
            {
                position = aTransform.position,
                eulerAngles = aTransform.eulerAngles,
                rotation = aTransform.rotation,
                localScale = aTransform.localScale
            };
        }

        public static void LoadLocal(this Transform aTransform, StoreTransform aData)
        {
            aTransform.localPosition = aData.position;
            aTransform.localEulerAngles = aData.eulerAngles;
            aTransform.localRotation = aData.rotation;
            aTransform.localScale = aData.localScale;
        }

        public static void LoadWorld(this Transform aTransform, StoreTransform aData)
        {
            aTransform.position = aData.position;
            aTransform.eulerAngles = aData.eulerAngles;
            aTransform.rotation = aData.rotation;
            aTransform.localScale = aData.localScale;
        }
    }
}