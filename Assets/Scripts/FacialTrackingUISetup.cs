using UnityEngine;
using UnityEngine.UI;

public class FacialTrackingUISetup : MonoBehaviour
{
    [Header("Quick Setup")]
    public bool autoSetupOnStart = true;
    
    [Header("UI References")]
    public Canvas targetCanvas;
    public GameObject barPrefab;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupFacialTrackingUI();
        }
    }
    
    [ContextMenu("Setup Facial Tracking UI")]
    public void SetupFacialTrackingUI()
    {
        // Create canvas if needed
        if (targetCanvas == null)
        {
            GameObject canvasObj = new GameObject("Facial Tracking Canvas");
            targetCanvas = canvasObj.AddComponent<Canvas>();
            targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create main panel
        GameObject panel = new GameObject("Facial Tracking Panel");
        panel.transform.SetParent(targetCanvas.transform, false);
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0.5f);
        panelRect.anchorMax = new Vector2(0, 0.5f);
        panelRect.pivot = new Vector2(0, 0.5f);
        panelRect.anchoredPosition = new Vector2(20, 0);
        panelRect.sizeDelta = new Vector2(300, 400);
        
        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.8f);
        
        // Add layout
        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.spacing = 10;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        
        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        Text title = titleObj.AddComponent<Text>();
        title.text = "FACIAL TRACKING";
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title.fontSize = 20;
        title.alignment = TextAnchor.MiddleCenter;
        title.color = Color.white;
        
        LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = 30;
        
        // Lip section
        CreateSection(panel.transform, "LIP EXPRESSIONS");
        GameObject lipBars = CreateBarContainer(panel.transform, "Lip Bars");
        
        // Eye section  
        CreateSection(panel.transform, "EYE EXPRESSIONS");
        GameObject eyeBars = CreateBarContainer(panel.transform, "Eye Bars");
        
        // Create or update bar prefab
        if (barPrefab == null)
        {
            barPrefab = CreateBarPrefab();
        }
        
        // Add visualizer component
        FacialTrackingVisualizer visualizer = targetCanvas.gameObject.GetComponent<FacialTrackingVisualizer>();
        if (visualizer == null)
        {
            visualizer = targetCanvas.gameObject.AddComponent<FacialTrackingVisualizer>();
        }
        
        visualizer.barPrefab = barPrefab;
        visualizer.lipBarsContainer = lipBars.transform;
        visualizer.eyeBarsContainer = eyeBars.transform;
        visualizer.maxBarHeight = 100f;
        visualizer.lipBarColor = Color.green;
        visualizer.eyeBarColor = Color.cyan;
        
        Debug.Log("✅ Facial Tracking UI setup complete!");
    }
    
    void CreateSection(Transform parent, string sectionName)
    {
        GameObject section = new GameObject(sectionName);
        section.transform.SetParent(parent, false);
        
        Text text = section.AddComponent<Text>();
        text.text = sectionName;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 16;
        text.alignment = TextAnchor.MiddleLeft;
        text.color = Color.yellow;
        
        LayoutElement layout = section.AddComponent<LayoutElement>();
        layout.preferredHeight = 25;
    }
    
    GameObject CreateBarContainer(Transform parent, string name)
    {
        GameObject container = new GameObject(name);
        container.transform.SetParent(parent, false);
        
        RectTransform rect = container.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(280, 120);
        
        HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 5;
        layout.childAlignment = TextAnchor.LowerCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        
        LayoutElement containerLayout = container.AddComponent<LayoutElement>();
        containerLayout.preferredHeight = 120;
        
        return container;
    }
    
    GameObject CreateBarPrefab()
    {
        GameObject bar = new GameObject("Bar Prefab");
        
        RectTransform rect = bar.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0);
        rect.anchorMax = new Vector2(0.5f, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.sizeDelta = new Vector2(30, 0);
        
        Image img = bar.AddComponent<Image>();
        img.color = Color.white;
        
        bar.SetActive(false);
        return bar;
    }
} 