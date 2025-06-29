using UnityEngine;
using System.Collections.Generic;

namespace FinalIKDebug
{
    public class FindFinalIKComponents : MonoBehaviour
    {
        [System.Serializable]
        public class IKComponentInfo
        {
            public string componentType;
            public string gameObjectName;
            public string gameObjectPath;
            public bool isEnabled;
            public string solverType;
        }

        public List<IKComponentInfo> foundComponents = new List<IKComponentInfo>();

        [ContextMenu("Find All FinalIK Components")]
        public void FindAllFinalIKComponents()
        {
            foundComponents.Clear();
            
            Debug.Log("Starting FinalIK component search...");
            
            // Find all MonoBehaviour components in the scene
            MonoBehaviour[] allComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            
            foreach (var component in allComponents)
            {
                string typeName = component.GetType().FullName;
                if (typeName.Contains("RootMotion.FinalIK") || typeName.Contains("GrounderIK") || typeName.Contains("VRIK"))
                {
                    var info = new IKComponentInfo
                    {
                        componentType = component.GetType().Name,
                        gameObjectName = component.gameObject.name,
                        gameObjectPath = GetGameObjectPath(component.transform),
                        isEnabled = component.enabled,
                        solverType = "Unknown"
                    };
                    foundComponents.Add(info);
                    Debug.Log($"Found FinalIK component: {typeName} on GameObject: {component.gameObject.name} at path: {info.gameObjectPath}");
                    Debug.Log($"Component enabled: {component.enabled}, GameObject active: {component.gameObject.activeInHierarchy}");
                }
            }
            
            Debug.Log($"Total FinalIK components found: {foundComponents.Count}");
        }

        private string GetGameObjectPath(Transform transform)
        {
            if (transform.parent == null)
                return transform.name;
            return GetGameObjectPath(transform.parent) + "/" + transform.name;
        }

        [ContextMenu("Check For GrounderIK Errors")]
        public void CheckForGrounderIKErrors()
        {
            Debug.Log("Checking for GrounderIK-related errors...");
            
            MonoBehaviour[] allComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            
            foreach (var component in allComponents)
            {
                string typeName = component.GetType().FullName;
                if (typeName.Contains("GrounderIK"))
                {
                    Debug.Log($"Found GrounderIK: {typeName} on {component.gameObject.name}");
                    
                    // Use reflection to check the solver field
                    var solverField = component.GetType().GetField("solver");
                    if (solverField != null)
                    {
                        var solverValue = solverField.GetValue(component);
                        if (solverValue == null)
                        {
                            Debug.LogError($"GrounderIK solver is null on {component.gameObject.name}!");
                        }
                        else
                        {
                            Debug.Log($"GrounderIK solver type: {solverValue.GetType().FullName}");
                        }
                    }
                }
            }
        }
    }
}