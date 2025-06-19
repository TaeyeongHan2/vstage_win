using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Unity.Sample.PoseLandmarkDetection;
using Mediapipe.Tasks.Vision.PoseLandmarker;

public class MediaPipeToAvatarAdapter : MonoBehaviour
{
    [SerializeField] private PoseLandmarkerRunner poseLandmarkerRunner;
    [SerializeField] private Transform bodyParent;
    [SerializeField] private GameObject landmarkPrefab;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private GameObject headPrefab;
    [SerializeField] private bool enableHead = false;
    [SerializeField] private float multiplier = 10f;
    [SerializeField] private float landmarkScale = 1f;
    
    private Body body;
    private Transform virtualNeck;
    private Transform virtualHip;
    
    // MediaPipe index to Unity Landmark mapping
    private static readonly Dictionary<int, Landmark> indexToLandmark = new Dictionary<int, Landmark>()
    {
        {0, Landmark.NOSE},
        {1, Landmark.LEFT_EYE_INNER},
        {2, Landmark.LEFT_EYE},
        {3, Landmark.LEFT_EYE_OUTER},
        {4, Landmark.RIGHT_EYE_INNER},
        {5, Landmark.RIGHT_EYE},
        {6, Landmark.RIGHT_EYE_OUTER},
        {7, Landmark.LEFT_EAR},
        {8, Landmark.RIGHT_EAR},
        {9, Landmark.MOUTH_LEFT},
        {10, Landmark.MOUTH_RIGHT},
        {11, Landmark.LEFT_SHOULDER},
        {12, Landmark.RIGHT_SHOULDER},
        {13, Landmark.LEFT_ELBOW},
        {14, Landmark.RIGHT_ELBOW},
        {15, Landmark.LEFT_WRIST},
        {16, Landmark.RIGHT_WRIST},
        {17, Landmark.LEFT_PINKY},
        {18, Landmark.RIGHT_PINKY},
        {19, Landmark.LEFT_INDEX},
        {20, Landmark.RIGHT_INDEX},
        {21, Landmark.LEFT_THUMB},
        {22, Landmark.RIGHT_THUMB},
        {23, Landmark.LEFT_HIP},
        {24, Landmark.RIGHT_HIP},
        {25, Landmark.LEFT_KNEE},
        {26, Landmark.RIGHT_KNEE},
        {27, Landmark.LEFT_ANKLE},
        {28, Landmark.RIGHT_ANKLE},
        {29, Landmark.LEFT_HEEL},
        {30, Landmark.RIGHT_HEEL},
        {31, Landmark.LEFT_FOOT_INDEX},
        {32, Landmark.RIGHT_FOOT_INDEX}
    };
    
    public Transform GetLandmark(Landmark mark)
    {
        return body.instances[(int)mark].transform;
    }
    
    public Transform GetVirtualNeck() => virtualNeck;
    public Transform GetVirtualHip() => virtualHip;
    
    private void Start()
    {
        // Create body structure
        body = new Body(bodyParent, landmarkPrefab, linePrefab, landmarkScale, enableHead ? headPrefab : null);
        virtualNeck = new GameObject("VirtualNeck").transform;
        virtualHip = new GameObject("VirtualHip").transform;
        
        // Subscribe to pose results
        if (poseLandmarkerRunner != null)
        {
            poseLandmarkerRunner.OnPoseResult += OnPoseResult;
        }
    }
    
    private void OnDestroy()
    {
        if (poseLandmarkerRunner != null)
        {
            poseLandmarkerRunner.OnPoseResult -= OnPoseResult;
        }
    }
    
    private void OnPoseResult(PoseLandmarkerResult result)
    {
        if (result.poseLandmarks == null || result.poseLandmarks.Count == 0)
            return;
            
        // Get first pose (index 0)
        var landmarks = result.poseLandmarks[0];
        
        // Update landmark positions
        for (int i = 0; i < landmarks.landmarks.Count && i < 33; i++)
        {
            if (indexToLandmark.TryGetValue(i, out Landmark landmarkType))
            {
                var landmark = landmarks.landmarks[i];
                // Convert normalized coordinates to world space
                // MediaPipe uses normalized coordinates (0-1), we need to scale them
                Vector3 position = new Vector3(
                    -landmark.x * multiplier,  // Flip X for mirror
                    -landmark.y * multiplier,  // Flip Y since Unity Y is up
                    landmark.z * multiplier
                );
                
                body.instances[(int)landmarkType].transform.localPosition = position;
            }
        }
        
        // Update virtual joints
        UpdateVirtualJoints();
        
        // Update body lines
        body.UpdateLines();
    }
    
    private void UpdateVirtualJoints()
    {
        // Virtual neck = midpoint between shoulders
        virtualNeck.position = (
            body.instances[(int)Landmark.RIGHT_SHOULDER].transform.position + 
            body.instances[(int)Landmark.LEFT_SHOULDER].transform.position
        ) / 2f;
        
        // Virtual hip = midpoint between hips
        virtualHip.position = (
            body.instances[(int)Landmark.RIGHT_HIP].transform.position + 
            body.instances[(int)Landmark.LEFT_HIP].transform.position
        ) / 2f;
    }
    
    public void SetVisible(bool visible)
    {
        bodyParent.gameObject.SetActive(visible);
    }
    
    // Body class moved from PipeServer
    public class Body
    {
        private const int LANDMARK_COUNT = 33;
        private const int LINES_COUNT = 11;
        
        public Transform parent;
        public GameObject[] instances = new GameObject[LANDMARK_COUNT];
        public LineRenderer[] lines = new LineRenderer[LINES_COUNT];
        
        public Body(Transform parent, GameObject landmarkPrefab, GameObject linePrefab, float s, GameObject headPrefab)
        {
            this.parent = parent;
            for (int i = 0; i < instances.Length; ++i)
            {
                instances[i] = Object.Instantiate(landmarkPrefab);
                instances[i].transform.localScale = Vector3.one * s;
                instances[i].transform.parent = parent;
                instances[i].name = ((Landmark)i).ToString();
            }
            for (int i = 0; i < lines.Length; ++i)
            {
                lines[i] = Object.Instantiate(linePrefab).GetComponent<LineRenderer>();
                lines[i].transform.parent = parent;
            }

            if (headPrefab)
            {
                GameObject head = Object.Instantiate(headPrefab);
                head.transform.parent = instances[(int)Landmark.NOSE].transform;
                head.transform.localPosition = headPrefab.transform.position;
                head.transform.localRotation = headPrefab.transform.localRotation;
                head.transform.localScale = headPrefab.transform.localScale;
            }
        }
        
        public void UpdateLines()
        {
            lines[0].positionCount = 4;
            lines[0].SetPosition(0, Position((Landmark)32));
            lines[0].SetPosition(1, Position((Landmark)30));
            lines[0].SetPosition(2, Position((Landmark)28));
            lines[0].SetPosition(3, Position((Landmark)32));
            lines[1].positionCount = 4;
            lines[1].SetPosition(0, Position((Landmark)31));
            lines[1].SetPosition(1, Position((Landmark)29));
            lines[1].SetPosition(2, Position((Landmark)27));
            lines[1].SetPosition(3, Position((Landmark)31));

            lines[2].positionCount = 3;
            lines[2].SetPosition(0, Position((Landmark)28));
            lines[2].SetPosition(1, Position((Landmark)26));
            lines[2].SetPosition(2, Position((Landmark)24));
            lines[3].positionCount = 3;
            lines[3].SetPosition(0, Position((Landmark)27));
            lines[3].SetPosition(1, Position((Landmark)25));
            lines[3].SetPosition(2, Position((Landmark)23));

            lines[4].positionCount = 5;
            lines[4].SetPosition(0, Position((Landmark)24));
            lines[4].SetPosition(1, Position((Landmark)23));
            lines[4].SetPosition(2, Position((Landmark)11));
            lines[4].SetPosition(3, Position((Landmark)12));
            lines[4].SetPosition(4, Position((Landmark)24));

            lines[5].positionCount = 4;
            lines[5].SetPosition(0, Position((Landmark)12));
            lines[5].SetPosition(1, Position((Landmark)14));
            lines[5].SetPosition(2, Position((Landmark)16));
            lines[5].SetPosition(3, Position((Landmark)22));
            lines[6].positionCount = 4;
            lines[6].SetPosition(0, Position((Landmark)11));
            lines[6].SetPosition(1, Position((Landmark)13));
            lines[6].SetPosition(2, Position((Landmark)15));
            lines[6].SetPosition(3, Position((Landmark)21));

            lines[7].positionCount = 4;
            lines[7].SetPosition(0, Position((Landmark)16));
            lines[7].SetPosition(1, Position((Landmark)18));
            lines[7].SetPosition(2, Position((Landmark)20));
            lines[7].SetPosition(3, Position((Landmark)16));
            lines[8].positionCount = 4;
            lines[8].SetPosition(0, Position((Landmark)15));
            lines[8].SetPosition(1, Position((Landmark)17));
            lines[8].SetPosition(2, Position((Landmark)19));
            lines[8].SetPosition(3, Position((Landmark)15));

            lines[9].positionCount = 2;
            lines[9].SetPosition(0, Position((Landmark)10));
            lines[9].SetPosition(1, Position((Landmark)9));

            lines[10].positionCount = 5;
            lines[10].SetPosition(0, Position((Landmark)8));
            lines[10].SetPosition(1, Position((Landmark)5));
            lines[10].SetPosition(2, Position((Landmark)0));
            lines[10].SetPosition(3, Position((Landmark)2));
            lines[10].SetPosition(4, Position((Landmark)7));
        }
        
        public Vector3 Position(Landmark Mark)
        {
            return instances[(int)Mark].transform.position;
        }
    }
} 