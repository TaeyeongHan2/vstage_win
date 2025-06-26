using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CalibrationUIPrefabSpawner : MonoBehaviour
{
    [Header("Prefab Reference")]
    public GameObject calibrationUIPrefab;
    
    [Header("Spawn Settings")]
    public bool spawnOnStart = true;
    public Vector2 spawnPosition = new Vector2(50, 0);
    
    private GameObject spawnedUI;
    
    void Start()
    {
        if (spawnOnStart)
            SpawnCalibrationUI();
    }
    
    [ContextMenu("Spawn Calibration UI")]
    public void SpawnCalibrationUI()
    {
        // Canvas 찾기 또는 생성
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        // 프리팹 인스턴스화
        if (calibrationUIPrefab != null)
        {
            spawnedUI = Instantiate(calibrationUIPrefab, canvas.transform);
            
            // 위치 조정
            RectTransform rect = spawnedUI.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = spawnPosition;
            }
            
            // 자동 연결 시도
            AutoConnectComponents();
        }
        else
        {
            Debug.LogError("Calibration UI Prefab not assigned!");
        }
    }
    
    void AutoConnectComponents()
    {
        if (spawnedUI == null) return;
        
        // CalibrationUISetup 컴포넌트 찾기
        var uiSetup = spawnedUI.GetComponentInChildren<CalibrationUISetup>();
        if (uiSetup != null)
        {
            // VRIKCalibrationController 찾아서 연결
            var calibController = FindObjectOfType<RootMotion.Demos.VRIKCalibrationController>();
            if (calibController != null)
            {
                uiSetup.calibrationController = calibController;
                Debug.Log("Auto-connected to VRIKCalibrationController");
            }
        }
    }
}