using System;
using System.Reflection;
using UnityEngine;

// ReSharper disable CheckNamespace

namespace AkilliMum
{
    public static class Extensions
    {
        public static void CopyTo<T>(this object comp, T target) where T : new()
        {
            Type type = comp.GetType();
            //T toReturn = new T();
            if (type != typeof(T)) return; // type mis-match
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(target, pinfo.GetValue(comp, null), null);
                    }
                    catch
                    {
                        //ignore
                    } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }
            FieldInfo[] finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(target, finfo.GetValue(comp));

            }
            //return toReturn;
        }

        //public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
        //{
        //    return go.AddComponent<T>().GetCopyOf(toAdd) as T;
        //}

        public static bool IsVisibleFrom(this Renderer renderer, Camera camera)
        {
            if (renderer == null || camera == null)
                return false;

            UnityEngine.Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
        }
    }
}