using UnityEngine;
using VIVE.OpenXR.Samples.FacialTracking;

/// <summary>
/// Mock 얼굴 트래킹 테스트 - 실제 하드웨어 없이 키보드로 시뮬레이션
/// </summary>
public class MockFacialTrackingTest : MonoBehaviour
{
    [Header("Simulated Values")]
    public float leftBlink = 0f;
    public float rightBlink = 0f;
    public float leftWide = 0f;
    public float rightWide = 0f;
    public float jawOpen = 0f;
    public float mouthPout = 0f;
    public float smileLeft = 0f;
    public float smileRight = 0f;
    
    [Header("Settings")]
    public float animationSpeed = 2f;
    
    void Start()
    {
        Debug.Log("🎮 Mock Facial Tracking Test Started!");
        Debug.Log("📌 Keyboard Controls:");
        Debug.Log("   Q/W - Left/Right Eye Blink");
        Debug.Log("   A/S - Left/Right Eye Wide");
        Debug.Log("   Z - Jaw Open");
        Debug.Log("   X - Mouth Pout");
        Debug.Log("   C/V - Smile Left/Right");
        Debug.Log("   Space - Reset all values");
    }
    
    void Update()
    {
        HandleKeyboardInput();
        SimulateAnimation();
        DisplayValues();
    }
    
    void HandleKeyboardInput()
    {
        // 눈 깜빡임
        if (Input.GetKey(KeyCode.Q))
            leftBlink = Mathf.MoveTowards(leftBlink, 1f, animationSpeed * Time.deltaTime);
        else
            leftBlink = Mathf.MoveTowards(leftBlink, 0f, animationSpeed * Time.deltaTime);
            
        if (Input.GetKey(KeyCode.W))
            rightBlink = Mathf.MoveTowards(rightBlink, 1f, animationSpeed * Time.deltaTime);
        else
            rightBlink = Mathf.MoveTowards(rightBlink, 0f, animationSpeed * Time.deltaTime);
        
        // 눈 크게 뜨기
        if (Input.GetKey(KeyCode.A))
            leftWide = Mathf.MoveTowards(leftWide, 1f, animationSpeed * Time.deltaTime);
        else
            leftWide = Mathf.MoveTowards(leftWide, 0f, animationSpeed * Time.deltaTime);
            
        if (Input.GetKey(KeyCode.S))
            rightWide = Mathf.MoveTowards(rightWide, 1f, animationSpeed * Time.deltaTime);
        else
            rightWide = Mathf.MoveTowards(rightWide, 0f, animationSpeed * Time.deltaTime);
        
        // 입 동작
        if (Input.GetKey(KeyCode.Z))
            jawOpen = Mathf.MoveTowards(jawOpen, 1f, animationSpeed * Time.deltaTime);
        else
            jawOpen = Mathf.MoveTowards(jawOpen, 0f, animationSpeed * Time.deltaTime);
            
        if (Input.GetKey(KeyCode.X))
            mouthPout = Mathf.MoveTowards(mouthPout, 1f, animationSpeed * Time.deltaTime);
        else
            mouthPout = Mathf.MoveTowards(mouthPout, 0f, animationSpeed * Time.deltaTime);
        
        // 웃기
        if (Input.GetKey(KeyCode.C))
            smileLeft = Mathf.MoveTowards(smileLeft, 1f, animationSpeed * Time.deltaTime);
        else
            smileLeft = Mathf.MoveTowards(smileLeft, 0f, animationSpeed * Time.deltaTime);
            
        if (Input.GetKey(KeyCode.V))
            smileRight = Mathf.MoveTowards(smileRight, 1f, animationSpeed * Time.deltaTime);
        else
            smileRight = Mathf.MoveTowards(smileRight, 0f, animationSpeed * Time.deltaTime);
        
        // 리셋
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetAllValues();
            Debug.Log("🔄 All values reset!");
        }
    }
    
    void SimulateAnimation()
    {
        // 자동 애니메이션 예제 (선택적)
        if (Input.GetKey(KeyCode.Return))
        {
            float time = Time.time;
            leftBlink = Mathf.Sin(time * 2f) > 0.8f ? 1f : 0f;
            rightBlink = Mathf.Sin(time * 2f + 0.1f) > 0.8f ? 1f : 0f;
            jawOpen = Mathf.PingPong(time * 0.5f, 0.3f);
            smileLeft = smileRight = Mathf.Max(0, Mathf.Sin(time * 1.5f)) * 0.5f;
        }
    }
    
    void DisplayValues()
    {
        // 값이 변경될 때만 로그 출력
        if (leftBlink > 0.1f || rightBlink > 0.1f)
            Debug.Log($"👁️ Blink - Left: {leftBlink:F2}, Right: {rightBlink:F2}");
            
        if (leftWide > 0.1f || rightWide > 0.1f)
            Debug.Log($"👁️ Wide - Left: {leftWide:F2}, Right: {rightWide:F2}");
            
        if (jawOpen > 0.1f)
            Debug.Log($"👄 Jaw Open: {jawOpen:F2}");
            
        if (mouthPout > 0.1f)
            Debug.Log($"👄 Mouth Pout: {mouthPout:F2}");
            
        if (smileLeft > 0.1f || smileRight > 0.1f)
            Debug.Log($"😊 Smile - Left: {smileLeft:F2}, Right: {smileRight:F2}");
    }
    
    void ResetAllValues()
    {
        leftBlink = rightBlink = 0f;
        leftWide = rightWide = 0f;
        jawOpen = mouthPout = 0f;
        smileLeft = smileRight = 0f;
    }
    
    // 외부에서 블렌드셰이프 배열로 접근할 수 있도록
    public float[] GetMockEyeBlendShapes()
    {
        var shapes = new float[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_MAX_ENUM_HTC];
        shapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_BLINK_HTC] = leftBlink;
        shapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_BLINK_HTC] = rightBlink;
        shapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_WIDE_HTC] = leftWide;
        shapes[(int)XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_WIDE_HTC] = rightWide;
        return shapes;
    }
    
    public float[] GetMockLipBlendShapes()
    {
        var shapes = new float[(int)XrLipShapeHTC.XR_LIP_SHAPE_MAX_ENUM_HTC];
        shapes[(int)XrLipShapeHTC.XR_LIP_SHAPE_JAW_OPEN_HTC] = jawOpen;
        shapes[(int)XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_POUT_HTC] = mouthPout;
        shapes[(int)XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_RAISER_LEFT_HTC] = smileLeft;
        shapes[(int)XrLipShapeHTC.XR_LIP_SHAPE_MOUTH_RAISER_RIGHT_HTC] = smileRight;
        return shapes;
    }
} 