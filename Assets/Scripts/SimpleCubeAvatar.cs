using UnityEngine;
using UnityEngine.XR.OpenXR;
using VIVE.OpenXR.FacialTracking;

[RequireComponent(typeof(MeshRenderer))]
public class SimpleCubeAvatar : MonoBehaviour
{
    [Header("Visual Settings")]
    public Color neutralColor = Color.gray;
    public Color talkingColor = Color.green;
    public Color smilingColor = Color.yellow;
    
    [Header("Transform Settings")]
    public float maxScaleY = 2f;
    public float scaleSpeed = 5f;
    
    private ViveFacialTracking facialTrackingFeature;
    private float[] blendshapes;
    private MeshRenderer meshRenderer;
    private Vector3 originalScale;
    private Material material;
    
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        material = meshRenderer.material;
        originalScale = transform.localScale;
        
        facialTrackingFeature = OpenXRSettings.Instance?.GetFeature<ViveFacialTracking>();
        
        if (facialTrackingFeature == null || !facialTrackingFeature.enabled)
        {
            Debug.LogError("[SimpleCubeAvatar] ViveFacialTracking not available!");
            enabled = false;
        }
        
        blendshapes = new float[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MAX_ENUM_HTC];
    }
    
    void Update()
    {
        if (facialTrackingFeature == null) return;
        
        // Get facial expressions
        bool success = facialTrackingFeature.GetFacialExpressions(
            XrFacialTrackingTypeHTC.XR_FACIAL_TRACKING_TYPE_LIP_DEFAULT_HTC, 
            out blendshapes
        );
        
        if (success && blendshapes != null)
        {
            // Get key values
            float jawOpen = blendshapes[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_JAW_OPEN_HTC];
            float smileRight = blendshapes[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_RAISER_RIGHT_HTC];
            float smileLeft = blendshapes[(int)XrLipExpressionHTC.XR_LIP_EXPRESSION_MOUTH_RAISER_LEFT_HTC];
            float smileAvg = (smileRight + smileLeft) / 2f;
            
            // Scale Y based on jaw open
            float targetScaleY = originalScale.y + (jawOpen * (maxScaleY - originalScale.y));
            Vector3 targetScale = new Vector3(originalScale.x, targetScaleY, originalScale.z);
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
            
            // Change color based on expression
            Color targetColor = neutralColor;
            if (jawOpen > 0.3f)
            {
                targetColor = talkingColor;
            }
            else if (smileAvg > 0.2f)
            {
                targetColor = smilingColor;
            }
            
            if (material != null)
            {
                material.color = Color.Lerp(material.color, targetColor, Time.deltaTime * scaleSpeed);
            }
            
            // Rotate based on smile asymmetry
            float smileDiff = smileRight - smileLeft;
            transform.rotation = Quaternion.Euler(0, smileDiff * 30f, 0);
        }
    }
    
    void OnGUI()
    {
        if (blendshapes == null) return;
        
        GUI.color = Color.yellow;
        GUI.Label(new Rect(10, 200, 300, 20), "=== Cube Avatar Response ===");
        GUI.Label(new Rect(10, 220, 300, 20), "• Opens mouth = Cube stretches up");
        GUI.Label(new Rect(10, 240, 300, 20), "• Smile = Yellow color");
        GUI.Label(new Rect(10, 260, 300, 20), "• Talk = Green color");
    }
} 