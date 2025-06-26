using UnityEngine;
using RootMotion.FinalIK;
using RootMotion.Demos;

[RequireComponent(typeof(VRIKCalibrationController))]
public class QuickCalibrationFix : MonoBehaviour
{
    private VRIKCalibrationController controller;
    
    void Start()
    {
        controller = GetComponent<VRIKCalibrationController>();
    }
    
    // UI 버튼에서 직접 호출 가능
    public void DoCalibration()
    {
        if (controller != null)
        {
            controller.data = VRIKCalibrator.Calibrate(
                controller.ik,
                controller.settings,
                controller.headTracker,
                controller.bodyTracker,
                controller.leftHandTracker,
                controller.rightHandTracker,
                controller.leftFootTracker,
                controller.rightFootTracker
            );
            Debug.Log("Calibration Complete!");
        }
    }
}