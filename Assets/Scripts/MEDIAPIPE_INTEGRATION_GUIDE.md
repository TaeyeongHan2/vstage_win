# MediaPipe Unity Plugin Integration Guide

## Overview
This guide explains how to integrate the MediaPipeUnityPlugin directly with the Avatar motion capture system, eliminating the need for Python scripts and UDP communication.

## Architecture Comparison

### Old Architecture
```
Python Script (mocap_sender.py) → UDP/Named Pipes → Unity (PipeServer) → Avatar
```

### New Architecture
```
MediaPipeUnityPlugin → MediaPipeToAvatarAdapter → Avatar
```

## Setup Instructions

### 1. Prerequisites
- MediaPipeUnityPlugin installed in your project
- Humanoid character with Avatar.cs script
- Unity 2021.3 or later

### 2. Scene Setup

1. **Open or Create a Pose Detection Scene**
   - Copy `Assets/MediaPipeUnity/Samples/Scenes/Pose Landmark Detection/Pose Landmark Detection.unity`
   - Or add PoseLandmarkerRunner to your existing scene

2. **Add MediaPipeToAvatarAdapter**
   ```
   GameObject → Create Empty → Rename to "MediaPipeAdapter"
   Add Component → MediaPipeToAvatarAdapter
   ```

3. **Configure the Adapter**
   - Assign PoseLandmarkerRunner reference
   - Create and assign landmark prefabs (or use invisible transforms)
   - Set multiplier value (default: 10)

4. **Update Avatar References**
   - Replace all PipeServer references with MediaPipeToAvatarAdapter
   - Update CalibrationTimer references

### 3. Component Configuration

#### PoseLandmarkerRunner Settings
- Model: BlazePose Heavy (recommended for best accuracy)
- Running Mode: LIVE_STREAM
- Min Detection Confidence: 0.5
- Min Tracking Confidence: 0.5
- Num Poses: 1

#### MediaPipeToAvatarAdapter Settings
- Multiplier: 10 (adjust based on your scene scale)
- Enable Head: true/false (based on your needs)
- Landmark Scale: 1.0

### 4. Calibration Process

The calibration process remains the same:
1. Press 'C' key to start calibration
2. Match the T-pose during countdown
3. System will automatically calibrate avatar bones to landmarks

## Performance Optimization

### Option 1: Visual Debugging (MediaPipeToAvatarAdapter)
- Shows landmark positions and connections
- Useful for debugging and calibration
- Higher overhead due to visual elements

### Option 2: Optimized Performance (OptimizedMediaPipeAdapter)
- No visual elements
- Built-in position smoothing
- Lower memory footprint
- Recommended for production

## Troubleshooting

### Common Issues

1. **"MediaPipeToAvatarAdapter not found" error**
   - Ensure the adapter GameObject is in the scene
   - Check that the script is properly attached

2. **Avatar not moving**
   - Verify PoseLandmarkerRunner is detecting poses
   - Check if OnPoseResult event is connected
   - Ensure calibration was successful

3. **Jittery movement**
   - Increase smoothing factor in OptimizedMediaPipeAdapter
   - Check camera/lighting conditions
   - Consider using heavier pose model

### Performance Tips

1. **GPU Acceleration**
   - Enable GPU delegate in PoseLandmarkerRunner config
   - Requires compatible GPU

2. **Frame Rate**
   - Limit detection to 30 FPS for stable performance
   - Use async image reading mode

3. **Multiple Avatars**
   - Single PoseLandmarkerRunner can drive multiple avatars
   - Add avatars to the adapter's avatar list

## Migration from PipeServer

1. **Remove old components**
   - Delete PipeServer GameObject
   - Remove ServerUDP.cs references
   - Stop running mocap_sender.py

2. **Update scripts**
   - Replace PipeServer with MediaPipeToAvatarAdapter in all scripts
   - Update method calls (same interface maintained)

3. **Test calibration**
   - Calibration data format remains compatible
   - Existing calibration files will work

## Advanced Features

### Custom Landmark Mapping
Modify the `indexToLandmark` dictionary in MediaPipeToAvatarAdapter to change landmark assignments.

### Multi-Person Tracking
Set `NumPoses > 1` in PoseLandmarkerRunner config and modify adapter to handle multiple poses.

### Recording and Playback
Implement recording by saving PoseLandmarkerResult data with timestamps for later playback.

## Benefits

- **Lower Latency**: Direct data flow without network communication
- **Simplified Setup**: No Python environment required
- **Cross-Platform**: Works on all Unity-supported platforms
- **Better Integration**: Access to all MediaPipe features
- **Improved Reliability**: No network connection issues 