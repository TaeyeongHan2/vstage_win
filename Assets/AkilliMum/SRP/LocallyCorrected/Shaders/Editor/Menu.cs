using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace AkilliMum.SRP.LCRS
{
    public static class Menu
    {
        [MenuItem("Akıllı Mum / LCRS / Reset Box Offset")]
        static void ResetBoxOffset()
        {
            var probe = (Selection.activeObject as GameObject)?.GetComponent<ReflectionProbe>();
            if (probe == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a reflection probe!", "OK");
                return;
            }

            //get probes box offset
            var center = probe.center;
            //reset offset
            probe.center = Vector3.zero;
            //change the position according to offset
            probe.transform.position += center;

        }
    }
}