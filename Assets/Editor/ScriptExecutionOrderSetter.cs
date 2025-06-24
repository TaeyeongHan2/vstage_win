using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

public class ScriptExecutionOrderSetter : EditorWindow
{
    [MenuItem("Tools/Set Script Execution Order")]
    public static void ShowWindow()
    {
        GetWindow<ScriptExecutionOrderSetter>("Script Execution Order Setter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Script Execution Order Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("This will set the execution order for Dynamic Bone and Final IK scripts:");
        GUILayout.Label("• Final IK scripts: -200 to -150 (execute first)");
        GUILayout.Label("• Dynamic Bone scripts: 100 (execute later)");
        GUILayout.Space(10);

        if (GUILayout.Button("Apply Script Execution Order"))
        {
            ApplyScriptExecutionOrder();
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Reset All Execution Orders"))
        {
            ResetAllExecutionOrders();
        }
    }

    private static void ApplyScriptExecutionOrder()
    {
        // Final IK 스크립트들 (먼저 실행)
        var finalIKScripts = new Dictionary<string, int>
        {
            { "IK", -200 },
            { "FullBodyBipedIK", -150 },
            { "VRIK", -150 },
            { "AimIK", -150 },
            { "LookAtIK", -150 },
            { "CCDIK", -150 },
            { "FABRIK", -150 },
            { "FABRIKRoot", -150 },
            { "LimbIK", -150 },
            { "ArmIK", -150 },
            { "LegIK", -150 },
            { "TrigonometricIK", -150 }
        };

        // Dynamic Bone 스크립트들 (나중에 실행)
        var dynamicBoneScripts = new Dictionary<string, int>
        {
            { "DynamicBone", 100 },
            { "DynamicBoneCollider", 100 },
            { "DynamicBonePlaneCollider", 100 }
        };

        int appliedCount = 0;

        // Final IK 스크립트들 설정
        foreach (var script in finalIKScripts)
        {
            if (SetExecutionOrder(script.Key, script.Value))
            {
                appliedCount++;
            }
        }

        // Dynamic Bone 스크립트들 설정
        foreach (var script in dynamicBoneScripts)
        {
            if (SetExecutionOrder(script.Key, script.Value))
            {
                appliedCount++;
            }
        }

        Debug.Log($"Script Execution Order applied to {appliedCount} scripts.");
        EditorUtility.DisplayDialog("Complete", $"Script Execution Order applied to {appliedCount} scripts.", "OK");
    }

    private static bool SetExecutionOrder(string scriptName, int order)
    {
        var scripts = AssetDatabase.FindAssets($"t:MonoScript {scriptName}")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<MonoScript>)
            .Where(script => script != null && script.GetClass() != null && script.GetClass().Name == scriptName);

        bool found = false;
        foreach (var script in scripts)
        {
            if (MonoImporter.GetExecutionOrder(script) != order)
            {
                MonoImporter.SetExecutionOrder(script, order);
                Debug.Log($"Set execution order for {scriptName}: {order}");
                found = true;
            }
        }

        if (!found)
        {
            Debug.LogWarning($"Script not found: {scriptName}");
        }

        return found;
    }

    private static void ResetAllExecutionOrders()
    {
        var allScripts = AssetDatabase.FindAssets("t:MonoScript")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<MonoScript>)
            .Where(script => script != null);

        int resetCount = 0;
        foreach (var script in allScripts)
        {
            if (MonoImporter.GetExecutionOrder(script) != 0)
            {
                MonoImporter.SetExecutionOrder(script, 0);
                resetCount++;
            }
        }

        Debug.Log($"Reset execution order for {resetCount} scripts.");
        EditorUtility.DisplayDialog("Complete", $"Reset execution order for {resetCount} scripts.", "OK");
    }
} 