
//cause everythign has changed on 2020.2 i will not convert the water

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace AkilliMum.SRP.Mirror
{
    public static class Menu
    {
        //static AddRequest Request;
        //const string defineWater = "_AKILLIMUM_WATER";
        //const string defineSteamVR = "VIVE_STEREO_STEAMVR";
        //const string defineWaveVR = "VIVE_STEREO_WAVEVR";

        //[MenuItem("Akıllı Mum / Enable Water")]
        //static void LoadWaterPackagesAndEnableWater()
        //{
        //    // Add burst a package to the Project
        //    Request = Client.Add("com.unity.burst");
        //    EditorApplication.update += ProgressBurst;
        //}

        //[MenuItem("Akıllı Mum / Enable Steam VR")]
        //static void EnableSteamVR()
        //{
        //    Debug.Log("Enabling Steam VR...");

        //    // Get defines.
        //    BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        //    string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

        //    // Append only if not defined already.
        //    if (defines.Contains(defineSteamVR))
        //    {
        //        Debug.LogWarning("Selected build target (" + EditorUserBuildSettings.activeBuildTarget.ToString() + ") already contains <b>" + defineSteamVR + "</b> <i>Scripting Define Symbol</i>.");
        //        return;
        //    }

        //    // Append.
        //    PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, (defines + ";" + defineSteamVR));
        //    Debug.LogWarning("<b>" + defineSteamVR + "</b> added to <i>Scripting Define Symbols</i> for selected build target (" + EditorUserBuildSettings.activeBuildTarget.ToString() + ").");

        //    Debug.Log("Enabled Steam VR. Please select the correct device type from 'Camera Shade->Device Type' property!");

        //}

        //[MenuItem("Akıllı Mum / Enable Wave VR")]
        //static void EnableWaveVR()
        //{
        //    Debug.Log("Enabling Wave VR...");

        //    // Get defines.
        //    BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        //    string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

        //    // Append only if not defined already.
        //    if (defines.Contains(defineWaveVR))
        //    {
        //        Debug.LogWarning("Selected build target (" + EditorUserBuildSettings.activeBuildTarget.ToString() + ") already contains <b>" + defineWaveVR + "</b> <i>Scripting Define Symbol</i>.");
        //        return;
        //    }

        //    // Append.
        //    PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, (defines + ";" + defineWaveVR));
        //    Debug.LogWarning("<b>" + defineWaveVR + "</b> added to <i>Scripting Define Symbols</i> for selected build target (" + EditorUserBuildSettings.activeBuildTarget.ToString() + ").");

        //    Debug.Log("Enabled Wave VR. Please select the correct device type from 'Camera Shade->Device Type' property!");


        //}

        //static void ProgressBurst()
        //{
        //    if (Request.IsCompleted)
        //    {
        //        if (Request.Status == StatusCode.Success)
        //            Debug.Log("Installed: " + Request.Result.packageId);
        //        else if (Request.Status >= StatusCode.Failure)
        //            Debug.Log(Request.Error.message);

        //        EditorApplication.update -= ProgressBurst;

        //        // Add mathematics a package to the Project
        //        Request = Client.Add("com.unity.mathematics");
        //        EditorApplication.update += ProgressMathematics;
        //    }
        //    else
        //    {
        //        Debug.Log("Installing Burst...");
        //    }
        //}

        //static void ProgressMathematics()
        //{
        //    if (Request.IsCompleted)
        //    {
        //        if (Request.Status == StatusCode.Success)
        //            Debug.Log("Installed: " + Request.Result.packageId);
        //        else if (Request.Status >= StatusCode.Failure)
        //            Debug.Log(Request.Error.message);

        //        EditorApplication.update -= ProgressMathematics;

        //        EnableWater();
        //    }
        //    else
        //    {
        //        Debug.Log("Installing Mathematics...");
        //    }
        //}

        //static void EnableWater()
        //{
        //    Debug.Log("Enabling Water...");

        //    // Get defines.
        //    BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        //    string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

        //    // Append only if not defined already.
        //    if (defines.Contains(defineWater))
        //    {
        //        Debug.LogWarning("Selected build target (" + EditorUserBuildSettings.activeBuildTarget.ToString() + ") already contains <b>" + defineWater + "</b> <i>Scripting Define Symbol</i>.");
        //        return;
        //    }

        //    // Append.
        //    PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, (defines + ";" + defineWater));
        //    Debug.LogWarning("<b>" + defineWater + "</b> added to <i>Scripting Define Symbols</i> for selected build target (" + EditorUserBuildSettings.activeBuildTarget.ToString() + ").");

        //    Debug.Log("Enabled Water. Please open the water sample scene!");
        //}
    }
}
