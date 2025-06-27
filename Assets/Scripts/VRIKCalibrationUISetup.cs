using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

// VRIKCalibrationWithCountdown의 UI를 자동으로 설정해주는 헬퍼 스크립트
public class VRIKCalibrationUISetup : MonoBehaviour
{
    [Header("자동 생성된 UI 참조")]
    public Canvas calibrationCanvas;
    public Text countdownText;
    public Text instructionText;
    
    // UI를 자동으로 생성하고 설정
    public void SetupCalibrationUI()
    {
        // 1. Canvas 생성
        if (calibrationCanvas == null)
        {
            GameObject canvasGO = new GameObject("CalibrationCanvas");
            calibrationCanvas = canvasGO.AddComponent<Canvas>();
            calibrationCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        // 2. 카운트다운 텍스트 생성
        if (countdownText == null)
        {
            GameObject countdownGO = new GameObject("CountdownText");
            countdownGO.transform.SetParent(calibrationCanvas.transform, false);
            countdownText = countdownGO.AddComponent<Text>();
            
            // 카운트다운 텍스트 설정
            RectTransform countdownRect = countdownGO.GetComponent<RectTransform>();
            countdownRect.anchorMin = new Vector2(0.5f, 0.5f);
            countdownRect.anchorMax = new Vector2(0.5f, 0.5f);
            countdownRect.anchoredPosition = Vector2.zero;
            countdownRect.sizeDelta = new Vector2(200, 100);
            
            countdownText.text = "3";
            countdownText.fontSize = 72;
            countdownText.color = Color.white;
            countdownText.alignment = TextAnchor.MiddleCenter;
            countdownText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            
            // 외곽선 추가
            Outline outline = countdownGO.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, -2);
        }
        
        // 3. 안내 텍스트 생성
        if (instructionText == null)
        {
            GameObject instructionGO = new GameObject("InstructionText");
            instructionGO.transform.SetParent(calibrationCanvas.transform, false);
            instructionText = instructionGO.AddComponent<Text>();
            
            // 안내 텍스트 설정
            RectTransform instructionRect = instructionGO.GetComponent<RectTransform>();
            instructionRect.anchorMin = new Vector2(0.5f, 0.7f);
            instructionRect.anchorMax = new Vector2(0.5f, 0.7f);
            instructionRect.anchoredPosition = Vector2.zero;
            instructionRect.sizeDelta = new Vector2(600, 60);
            
            instructionText.text = "C키를 눌러 캘리브레이션 시작";
            instructionText.fontSize = 32;
            instructionText.color = Color.white;
            instructionText.alignment = TextAnchor.MiddleCenter;
            instructionText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            
            // 외곽선 추가
            Outline outline2 = instructionGO.AddComponent<Outline>();
            outline2.effectColor = Color.black;
            outline2.effectDistance = new Vector2(1, -1);
        }
        
        // 4. VRIKCalibrationWithCountdown 컴포넌트에 자동 연결
        VRIKCalibrationWithCountdown calibrationScript = GetComponent<VRIKCalibrationWithCountdown>();
        if (calibrationScript != null)
        {
            calibrationScript.countdownText = countdownText;
            calibrationScript.instructionText = instructionText;
            Debug.Log("✅ UI가 VRIKCalibrationWithCountdown에 자동 연결되었습니다!");
        }
        
        Debug.Log("✅ 캘리브레이션 UI 설정 완료!");
    }
}

#if UNITY_EDITOR
// 에디터에서 쉽게 설정할 수 있도록 커스텀 인스펙터 추가
[CustomEditor(typeof(VRIKCalibrationUISetup))]
public class VRIKCalibrationUISetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        VRIKCalibrationUISetup setup = (VRIKCalibrationUISetup)target;
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("UI 자동 생성 및 설정", GUILayout.Height(30)))
        {
            setup.SetupCalibrationUI();
            EditorUtility.SetDirty(setup);
        }
        
        EditorGUILayout.HelpBox(
            "이 버튼을 누르면:\n" +
            "1. Canvas가 자동 생성됩니다\n" +
            "2. 카운트다운 텍스트가 생성됩니다\n" +
            "3. 안내 메시지 텍스트가 생성됩니다\n" +
            "4. VRIKCalibrationWithCountdown에 자동 연결됩니다",
            MessageType.Info
        );
    }
}
#endif 