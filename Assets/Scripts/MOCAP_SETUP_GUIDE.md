# Unity MediaPipe Motion Capture Setup Guide

## Overview
This is the simplest possible implementation for real-time motion capture using MediaPipe in Unity.

## System Architecture
```
Webcam → MediaPipe (PoseLandmarkerRunner) → DirectPoseController → wonjin_Shinano_kisekae
```

## Setup Instructions

1. **Open the Scene**
   - Open: `Assets/MediaPipeUnity/Samples/Scenes/Pose Landmark Detection/Pose Landmark Detection 1.unity`

2. **Add DirectPoseController**
   - Select the `wonjin_Shinano_kisekae` GameObject in the hierarchy
   - Add Component → DirectPoseController
   - The component will automatically find and connect to PoseLandmarkerRunner

3. **Run the Scene**
   - Press Play
   - Allow webcam access when prompted
   - Stand in T-pose for best initial calibration
   - The character will mirror your movements

## Configuration

### DirectPoseController Settings
- **Smoothing**: Adjust between 1-20 (default: 10)
  - Lower = More responsive but jittery
  - Higher = Smoother but less responsive

### MediaPipe Settings (in PoseLandmarkDetectionConfig)
- Model: Heavy (best accuracy)
- Delegate: CPU (for compatibility)
- Min Detection Confidence: 0.5
- Min Tracking Confidence: 0.5

## Troubleshooting

**Character not moving?**
- Check console for errors
- Ensure webcam is connected and permissions granted
- Verify DirectPoseController is added to wonjin_Shinano_kisekae

**Threading error?**
- DirectPoseController already handles thread safety with a queue system
- MediaPipe callbacks run on background threads, but pose application happens on main thread

**Performance issues?**
- Try adjusting smoothing parameter
- Consider using Lite model instead of Heavy in PoseLandmarkDetectionConfig

## Technical Details

The DirectPoseController:
- Subscribes to PoseLandmarkerRunner.OnPoseResult event
- Queues pose data from background thread
- Applies poses on main thread in Update()
- Uses simple 2-bone IK for arms and legs
- Converts MediaPipe coordinates to Unity coordinates
- Applies smoothing with Quaternion.Slerp

## File Structure
```
Assets/Scripts/
└── DirectPoseController.cs    # The only script needed
```

That's it! This is the simplest, most direct way to connect MediaPipe to your Unity character. 