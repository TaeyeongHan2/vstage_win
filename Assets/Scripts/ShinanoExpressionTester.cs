using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Shinanoの表情をキーボード入力でテストするためのスクリプト
/// VIVEフェイシャルトラッキングなしでもブレンドシェイプをテスト可能
/// </summary>
public class ShinanoExpressionTester : MonoBehaviour
{
    [System.Serializable]
    public class BlendshapeWeight
    {
        public string blendshapeName;
        [Range(0f, 1f)] public float weight;
    }
    
    [System.Serializable]
    public class ExpressionPreset
    {
        public string name;
        public KeyCode keyCode;
        public List<BlendshapeWeight> blendshapes = new List<BlendshapeWeight>();
        [TextArea(2, 4)]
        public string description;
    }
    
    [Header("Target Settings")]
    public SkinnedMeshRenderer shinanoBodyMesh;
    
    [Header("Expression Presets")]
    public List<ExpressionPreset> expressionPresets = new List<ExpressionPreset>();
    
    [Header("Manual Control")]
    public bool enableManualControl = true;
    public string targetBlendshape = "mouth_a1";
    [Range(0f, 1f)] public float manualValue = 0f;
    
    [Header("Animation Settings")]
    [Range(0f, 1f)] public float transitionSpeed = 0.15f;
    public bool smoothTransition = true;
    
    // Runtime
    private Dictionary<string, float> currentWeights = new Dictionary<string, float>();
    private Dictionary<string, float> targetWeights = new Dictionary<string, float>();
    private Dictionary<string, int> blendshapeIndices = new Dictionary<string, int>();
    
    void Start()
    {
        // Auto-find Shinano body mesh if not assigned
        if (shinanoBodyMesh == null)
        {
            shinanoBodyMesh = GetComponentInChildren<SkinnedMeshRenderer>();
            if (shinanoBodyMesh != null && shinanoBodyMesh.name == "Body")
            {
                Debug.Log("[ExpressionTester] Found Shinano Body mesh automatically");
            }
        }
        
        if (shinanoBodyMesh == null)
        {
            Debug.LogError("[ExpressionTester] No SkinnedMeshRenderer found!");
            enabled = false;
            return;
        }
        
        // Cache blendshape indices
        CacheBlendshapeIndices();
        
        // Initialize default presets if empty
        if (expressionPresets.Count == 0)
        {
            InitializeDefaultPresets();
        }
        
        Debug.Log($"[ExpressionTester] Initialized with {expressionPresets.Count} presets");
        ShowHelp();
    }
    
    void CacheBlendshapeIndices()
    {
        blendshapeIndices.Clear();
        for (int i = 0; i < shinanoBodyMesh.sharedMesh.blendShapeCount; i++)
        {
            string name = shinanoBodyMesh.sharedMesh.GetBlendShapeName(i);
            blendshapeIndices[name] = i;
        }
    }
    
    void InitializeDefaultPresets()
    {
        // あ (A)
        expressionPresets.Add(new ExpressionPreset
        {
            name = "あ (A)",
            keyCode = KeyCode.Alpha1,
            description = "口を大きく開ける「あ」の音",
            blendshapes = new List<BlendshapeWeight>
            {
                new BlendshapeWeight { blendshapeName = "mouth_a1", weight = 0.8f },
                new BlendshapeWeight { blendshapeName = "mouthparts_open", weight = 0.3f }
            }
        });
        
        // い (I)
        expressionPresets.Add(new ExpressionPreset
        {
            name = "い (I)",
            keyCode = KeyCode.Alpha2,
            description = "口を横に広げる「い」の音",
            blendshapes = new List<BlendshapeWeight>
            {
                new BlendshapeWeight { blendshapeName = "mouth_i1", weight = 0.7f },
                new BlendshapeWeight { blendshapeName = "mouth_wide", weight = 0.4f }
            }
        });
        
        // う (U)
        expressionPresets.Add(new ExpressionPreset
        {
            name = "う (U)",
            keyCode = KeyCode.Alpha3,
            description = "口をすぼめる「う」の音",
            blendshapes = new List<BlendshapeWeight>
            {
                new BlendshapeWeight { blendshapeName = "mouth_u1", weight = 0.8f },
                new BlendshapeWeight { blendshapeName = "mouth_○_small", weight = 0.5f }
            }
        });
        
        // え (E)
        expressionPresets.Add(new ExpressionPreset
        {
            name = "え (E)",
            keyCode = KeyCode.Alpha4,
            description = "口を少し開けて横に広げる「え」の音",
            blendshapes = new List<BlendshapeWeight>
            {
                new BlendshapeWeight { blendshapeName = "mouth_e1", weight = 0.7f }
            }
        });
        
        // お (O)
        expressionPresets.Add(new ExpressionPreset
        {
            name = "お (O)",
            keyCode = KeyCode.Alpha5,
            description = "口を丸める「お」の音",
            blendshapes = new List<BlendshapeWeight>
            {
                new BlendshapeWeight { blendshapeName = "mouth_o1", weight = 0.8f },
                new BlendshapeWeight { blendshapeName = "mouth_○_big", weight = 0.4f }
            }
        });
        
        // 笑顔 (Smile)
        expressionPresets.Add(new ExpressionPreset
        {
            name = "笑顔 (Smile)",
            keyCode = KeyCode.S,
            description = "にっこり笑顔",
            blendshapes = new List<BlendshapeWeight>
            {
                new BlendshapeWeight { blendshapeName = "mouth_smile", weight = 0.8f },
                new BlendshapeWeight { blendshapeName = "eye_joy", weight = 0.5f },
                new BlendshapeWeight { blendshapeName = "other_cheek_1", weight = 0.3f }
            }
        });
        
        // 悲しい (Sad)
        expressionPresets.Add(new ExpressionPreset
        {
            name = "悲しい (Sad)",
            keyCode = KeyCode.D,
            description = "悲しい表情",
            blendshapes = new List<BlendshapeWeight>
            {
                new BlendshapeWeight { blendshapeName = "mouth_sad", weight = 0.7f },
                new BlendshapeWeight { blendshapeName = "eye_sad", weight = 0.8f },
                new BlendshapeWeight { blendshapeName = "eyebrow_sad1", weight = 0.9f }
            }
        });
        
        // 驚き (Surprise)
        expressionPresets.Add(new ExpressionPreset
        {
            name = "驚き (Surprise)",
            keyCode = KeyCode.W,
            description = "びっくりした表情",
            blendshapes = new List<BlendshapeWeight>
            {
                new BlendshapeWeight { blendshapeName = "mouth_o1", weight = 0.6f },
                new BlendshapeWeight { blendshapeName = "eye_surprise", weight = 1.0f },
                new BlendshapeWeight { blendshapeName = "eyebrow_surprised", weight = 0.9f }
            }
        });
        
        // 舌出し (Tongue Out)
        expressionPresets.Add(new ExpressionPreset
        {
            name = "舌出し (Tongue)",
            keyCode = KeyCode.T,
            description = "舌を出す",
            blendshapes = new List<BlendshapeWeight>
            {
                new BlendshapeWeight { blendshapeName = "tongue_pero", weight = 1.0f },
                new BlendshapeWeight { blendshapeName = "mouth_smile", weight = 0.3f }
            }
        });
        
        // ω
        expressionPresets.Add(new ExpressionPreset
        {
            name = "ω",
            keyCode = KeyCode.M,
            description = "ω の口",
            blendshapes = new List<BlendshapeWeight>
            {
                new BlendshapeWeight { blendshapeName = "mouth_ω", weight = 0.9f }
            }
        });
    }
    
    void Update()
    {
        if (shinanoBodyMesh == null) return;
        
        // Check for preset keys
        foreach (var preset in expressionPresets)
        {
            if (Input.GetKeyDown(preset.keyCode))
            {
                ApplyPreset(preset);
            }
        }
        
        // Reset all expressions
        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Alpha0))
        {
            ResetAllExpressions();
        }
        
        // Manual control with arrow keys
        if (enableManualControl)
        {
            HandleManualControl();
        }
        
        // Help toggle
        if (Input.GetKeyDown(KeyCode.H))
        {
            ShowHelp();
        }
        
        // Smooth transition
        if (smoothTransition)
        {
            UpdateSmoothTransition();
        }
        else
        {
            ApplyTargetWeights();
        }
    }
    
    void HandleManualControl()
    {
        float delta = 0.02f;
        
        // Up/Down arrows to adjust value
        if (Input.GetKey(KeyCode.UpArrow))
        {
            manualValue = Mathf.Clamp01(manualValue + delta);
            SetBlendshapeWeight(targetBlendshape, manualValue);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            manualValue = Mathf.Clamp01(manualValue - delta);
            SetBlendshapeWeight(targetBlendshape, manualValue);
        }
        
        // Left/Right to cycle through blendshapes
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            CycleToNextBlendshape(1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            CycleToNextBlendshape(-1);
        }
    }
    
    void CycleToNextBlendshape(int direction)
    {
        var keys = new List<string>(blendshapeIndices.Keys);
        keys.Sort();
        
        int currentIndex = keys.IndexOf(targetBlendshape);
        if (currentIndex == -1) currentIndex = 0;
        
        currentIndex = (currentIndex + direction + keys.Count) % keys.Count;
        targetBlendshape = keys[currentIndex];
        manualValue = GetBlendshapeWeight(targetBlendshape) / 100f;
        
        Debug.Log($"[ExpressionTester] Selected blendshape: {targetBlendshape}");
    }
    
    void ApplyPreset(ExpressionPreset preset)
    {
        Debug.Log($"[ExpressionTester] Applying preset: {preset.name}");
        
        // Reset target weights
        targetWeights.Clear();
        
        // Set new target weights
        foreach (var blendshape in preset.blendshapes)
        {
            targetWeights[blendshape.blendshapeName] = blendshape.weight;
        }
    }
    
    void ResetAllExpressions()
    {
        Debug.Log("[ExpressionTester] Resetting all expressions");
        targetWeights.Clear();
        manualValue = 0f;
    }
    
    void UpdateSmoothTransition()
    {
        // Update current weights towards target weights
        var allKeys = new HashSet<string>(currentWeights.Keys);
        foreach (var key in targetWeights.Keys)
        {
            allKeys.Add(key);
        }
        
        foreach (var key in allKeys)
        {
            float targetWeight = targetWeights.ContainsKey(key) ? targetWeights[key] : 0f;
            float currentWeight = currentWeights.ContainsKey(key) ? currentWeights[key] : 0f;
            
            currentWeight = Mathf.Lerp(currentWeight, targetWeight, transitionSpeed);
            currentWeights[key] = currentWeight;
            
            if (blendshapeIndices.ContainsKey(key))
            {
                shinanoBodyMesh.SetBlendShapeWeight(blendshapeIndices[key], currentWeight * 100f);
            }
        }
    }
    
    void ApplyTargetWeights()
    {
        foreach (var kvp in targetWeights)
        {
            if (blendshapeIndices.ContainsKey(kvp.Key))
            {
                shinanoBodyMesh.SetBlendShapeWeight(blendshapeIndices[kvp.Key], kvp.Value * 100f);
            }
        }
    }
    
    void SetBlendshapeWeight(string name, float weight)
    {
        if (blendshapeIndices.ContainsKey(name))
        {
            targetWeights[name] = weight;
            if (!smoothTransition)
            {
                shinanoBodyMesh.SetBlendShapeWeight(blendshapeIndices[name], weight * 100f);
            }
        }
    }
    
    float GetBlendshapeWeight(string name)
    {
        if (blendshapeIndices.ContainsKey(name))
        {
            return shinanoBodyMesh.GetBlendShapeWeight(blendshapeIndices[name]);
        }
        return 0f;
    }
    
    void ShowHelp()
    {
        Debug.Log("=== Shinano Expression Tester ===");
        Debug.Log("Keyboard Controls:");
        foreach (var preset in expressionPresets)
        {
            Debug.Log($"  {preset.keyCode}: {preset.name} - {preset.description}");
        }
        Debug.Log("  R or 0: Reset all expressions");
        Debug.Log("  H: Show this help");
        Debug.Log("\nManual Control:");
        Debug.Log("  ↑/↓: Adjust current blendshape value");
        Debug.Log($"  ←/→: Cycle through blendshapes (Current: {targetBlendshape})");
        Debug.Log("================================");
    }
    
    void OnGUI()
    {
        // Display current expression info
        int y = 10;
        GUI.Label(new Rect(10, y, 400, 20), "=== Shinano Expression Tester ===");
        y += 25;
        
        // Show active expressions
        foreach (var kvp in currentWeights)
        {
            if (kvp.Value > 0.01f)
            {
                GUI.Label(new Rect(10, y, 300, 20), $"{kvp.Key}: {kvp.Value:F2}");
                
                // Draw bar
                GUI.DrawTexture(new Rect(200, y, kvp.Value * 100, 15), Texture2D.whiteTexture);
                y += 20;
            }
        }
        
        // Manual control info
        if (enableManualControl)
        {
            y += 10;
            GUI.Label(new Rect(10, y, 400, 20), $"Manual: {targetBlendshape} = {manualValue:F2}");
        }
    }
} 