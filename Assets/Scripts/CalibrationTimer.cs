using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CalibrationTimer : MonoBehaviour
{
    public MediaPipeToAvatarAdapter adapter;
    public OptimizedMediaPipeAdapter optimizedAdapter;
    public int timer = 5;
    public KeyCode calibrationKey = KeyCode.C;
    public TextMeshProUGUI text;

    private bool calibrated;

    private void Start()
    {
        Debug.Log("[CalibrationTimer] Starting...");
        
        if (adapter == null && optimizedAdapter == null)
        {
            adapter = FindObjectOfType<MediaPipeToAvatarAdapter>();
            optimizedAdapter = FindObjectOfType<OptimizedMediaPipeAdapter>();
            Debug.Log($"[CalibrationTimer] Auto-found adapters - Adapter: {adapter != null}, OptimizedAdapter: {optimizedAdapter != null}");
        }
        
        bool shouldEnable = false;
        Avatar[] a = FindObjectsByType<Avatar>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Debug.Log($"[CalibrationTimer] Found {a.Length} Avatar(s)");
        
        foreach (Avatar aa in a)
        {
            if (!aa.isActiveAndEnabled) continue;
            if (!aa.calibrationData)
            {
                shouldEnable = true;
                Debug.Log($"[CalibrationTimer] Avatar '{aa.gameObject.name}' needs calibration");
                break;
            }
        }
        
        // 텍스트 컴포넌트 확인
        if (text == null)
        {
            Debug.LogError("[CalibrationTimer] ❌ TextMeshProUGUI component is not assigned!");
            Debug.LogError("[CalibrationTimer] Please assign a TextMeshProUGUI in the Inspector!");
        }
        else
        {
            text.text = shouldEnable ? "Press " + calibrationKey + " to start calibration timer." : "";
            Debug.Log($"[CalibrationTimer] Text component assigned. Initial text: '{text.text}'");
        }

        gameObject.SetActive(shouldEnable);
        Debug.Log($"[CalibrationTimer] GameObject active: {shouldEnable}");
        
        if (!shouldEnable)
        {
            SetAdapterVisible(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(calibrationKey))
        {
            Debug.Log($"[CalibrationTimer] {calibrationKey} key pressed! Calibrated: {calibrated}");
            
            if(!calibrated)
            {
                calibrated = true;
                StartCoroutine(Timer());
            }
            else
            {
                StartCoroutine(Notify());
            }
        }
    }
    
    private IEnumerator Timer()
    {
        Debug.Log("[CalibrationTimer] Starting calibration timer...");
        
        int t = timer;
        while (t > 0)
        {
            string message = "Copy the avatars starting pose: " + t.ToString();
            
            if (text != null)
            {
                text.text = message;
            }
            else
            {
                Debug.LogError("[CalibrationTimer] ❌ Cannot display message - TextMeshProUGUI is null!");
            }
            
            Debug.Log($"[CalibrationTimer] Timer: {message}");
            yield return new WaitForSeconds(1f);
            --t;
        }
        
        Avatar[] a = FindObjectsByType<Avatar>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Debug.Log($"[CalibrationTimer] Calibrating {a.Length} avatars...");
        
        foreach(Avatar aa in a)
        {
            if (!aa.isActiveAndEnabled) continue;
            aa.Calibrate();
            Debug.Log($"[CalibrationTimer] Calibrated avatar: {aa.gameObject.name}");
        }
        
        if (a.Length > 0)
        {
            if (text != null)
            {
                text.text = "Calibration Completed";
            }
            Debug.Log("[CalibrationTimer] ✅ Calibration Completed");
            SetAdapterVisible(false);
        }
        else
        {
            if (text != null)
            {
                text.text = "Avatar in scene not found...";
            }
            Debug.LogError("[CalibrationTimer] ❌ No avatars found in scene!");
        }
        
        yield return new WaitForSeconds(1.5f);
        if (text != null)
        {
            text.text = "";
        }
    }
    
    private IEnumerator Notify()
    {
        string message = "Must restart instance to recalibrate.";
        
        if (text != null)
        {
            text.text = message;
        }
        
        Debug.LogWarning($"[CalibrationTimer] {message}");
        yield return new WaitForSeconds(3f);
        
        if (text != null)
        {
            text.text = "";
        }
    }

    private void SetAdapterVisible(bool visible)
    {
        if (adapter != null)
            adapter.SetVisible(visible);
        if (optimizedAdapter != null)
            optimizedAdapter.SetVisible(visible);
    }
}
