
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ReSharper disable CheckNamespace

namespace AkilliMum.SRP.LCRS
{
    public class LCRSManager : MonoBehaviour
    {
        //[Tooltip("Debug the bounding boxes to see if script creates the right items!")]
        //public bool IsDebug = false;
        //[Tooltip("Debug the bounding boxes to see if script creates the right items!")]
        //public bool IsDebug = false;
        [Tooltip("If you are using non-static rotated environments, use this option to pass the rotation automatically to the shader.")]
        public bool UpdateRotationOnRuntime = false;

        private List<LCRSItem> _items = new List<LCRSItem>();
        private ReflectionProbe[] _refProbes;

        void OnEnable()
        {
            InitializeProperties();
        }

        public void InitializeProperties()
        {
            _refProbes = (ReflectionProbe[])FindObjectsOfType(typeof(ReflectionProbe));

            Renderer[] renderers = (Renderer[])FindObjectsOfType(typeof(Renderer));
            if (renderers != null && renderers.Length > 0)
            {
                foreach (var ren in renderers)
                {
                    if (ren?.sharedMaterials != null && ren.sharedMaterials.Length > 0)
                    {
                        foreach (var mat in ren.sharedMaterials)
                        {
                            if (mat?.shader?.name == "AkilliMum/URP/LCRS/2019" ||
                                mat?.shader?.name == "AkilliMum/URP/LCRS/2020_2" ||
                                mat?.shader?.name == "AkilliMum/URP/Mirrors/2019" ||
                                mat?.shader?.name == "AkilliMum/URP/Mirrors/2020_2" ||
                                mat?.shader?.name == "AkilliMum/URP/SSPR/2019" ||
                                mat?.shader?.name == "AkilliMum/URP/SSPR/2020_2" ||
                                mat?.shader?.name == "AkilliMum/URP/CarPaint/PBR" ||
                                mat?.shader?.name == "AkilliMum/URP/CarPaint/PBR_PaintPass")
                            {
                                Debug.Log("found mat. setting values...");
                                CreateBoundsAndSetPositions(ren, mat);
                            }
                        }
                    }
                }
            }
        }

        //void OnDrawGizmos()
        //{
        //    //if(IsDebug)
        //    //{
        //    //    Gizmos.color = Color.yellow;
        //    //    //Gizmos.DrawWireCube(_probe.gameObject.transform.position, _probe.size);
        //    //}
        //}

        void Update()
        {
            if (UpdateRotationOnRuntime)
            {
                foreach (var item in _items)
                {
                    //root is important here, cause ref probe renders that actually!
                    //Debug.Log("rotation: " + item.Renderer.transform.root.transform.rotation.eulerAngles);
                    item.Material.SetVector("_EnviRotation", item.Renderer.transform.root.transform.rotation.eulerAngles);
                }
            }
        }

        public void CreateBoundsAndSetPositions(Renderer ren, Material mat)
        {
            if (ren == null || mat == null)
            {
                Debug.Log("renderer or material is null. please check the item...");
                return;
            }

            //have to check both renderer and material, because ren may have multiple materials (bug resolved in 1.01)
            var item = _items.FirstOrDefault(a =>
                a.Renderer.GetInstanceID() == ren.GetInstanceID() &&
                a.Material.GetInstanceID() == mat.GetInstanceID());
            if (item == null)
            {
                item = new LCRSItem
                {
                    Renderer = ren,
                    Material = mat
                };
                _items.Add(item);
            }

            //get nearest ref probe (use if there is a marked one)
            var mark = ren.gameObject?.GetComponent<MarkProbes>()?.MarkProbe;
            var anchorOverride = ren.gameObject?.GetComponent<MeshRenderer>()?.probeAnchor?
                .gameObject?.GetComponent<ReflectionProbe>();
            var refProbe = (mark ?? anchorOverride) ?? GetNearest(ren.transform);
            if (refProbe == null)
                return;

            //zero the bounds
            //item.Bounds = new Bounds(ren.transform.position, Vector3.zero);
            item.Bounds = new Bounds(refProbe.transform.position, Vector3.zero);

            //calculate the encapsulating bound
            //item.Bounds.Encapsulate(ren.bounds);
            item.Bounds.Encapsulate(refProbe.bounds);

            //do not change for default :)
            //if (BoundingBox == BoundingBox.Average)
            //{
            ////get average for x,z space
            //var average = (item.Bounds.size.x + item.Bounds.size.z) / 2;
            //item.Bounds.size = new Vector3(average, item.Bounds.size.y, average);
            //}
            //else if(BoundingBox == BoundingBox.Max)
            //{
            //    //get max for x,z space
            //    var max = _bounds.size.x > _bounds.size.z ? _bounds.size.x : _bounds.size.z;
            //    _bounds.size = new Vector3(max, _bounds.size.y, max);
            //}
            //else if (BoundingBox == BoundingBox.Min)
            //{
            //    //get min for x,z space
            //    var min = _bounds.size.x > _bounds.size.z ? _bounds.size.z : _bounds.size.x;
            //    _bounds.size = new Vector3(min, _bounds.size.y, min);
            //}

            //set positions
            Vector3 bboxLenght = item.Bounds.size; //calculated here and does not change
            //Vector3 centerBBox = _probe.gameObject.transform.position; //if object is moving?
            //Vector3 centerBBox = item.Renderer.transform.position;
            Vector3 centerBBox = refProbe.transform.position;

            // Min and max BBox points in world coordinates.
            Vector3 BMin = centerBBox - bboxLenght / 2;
            Vector3 BMax = centerBBox + bboxLenght / 2;
            // Pass the values to the material.

            item.Material.SetVector("_BBoxMin", BMin);
            item.Material.SetVector("_BBoxMax", BMax);
            item.Material.SetVector("_EnviCubeMapPos", centerBBox);
            ////ren.sharedMaterial.SetTexture("_EnviCubeMapMain", _probe.texture);
            //ren.sharedMaterial.SetTexture("_EnviCubeMapMain", _cube);
            //ren.sharedMaterial.SetTexture("_EnviCubeMapToMix1", found == null ? null : found.texture);
            //ren.sharedMaterial.SetFloat("_MixMultiplier", _MixMultiplier);
            ////ren.sharedMaterial.SetFloat("_EnviCubeSmoothness", Smoothness); //moved to main shader
            ////ren.sharedMaterial.SetFloat("_EnviCubeIntensity", Intensity); //moved to main shader
            //if (MixOtherProbes)
            //    ren.sharedMaterial.EnableKeyword("_REALTIMEREFLECTION_MIX");
            //else
            //    ren.sharedMaterial.DisableKeyword("_REALTIMEREFLECTION_MIX");

        }

        ReflectionProbe GetNearest(Transform source)
        {
            if (_refProbes == null || _refProbes.Length <= 0)
                return null;

            ReflectionProbe bestTarget = null;
            float closestDistanceSqr = Mathf.Infinity;
            Vector3 currentPosition = source.position;
            foreach (var potentialTarget in _refProbes)
            {
                Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    bestTarget = potentialTarget;
                }
            }

            Debug.Log("best target name: " + bestTarget?.gameObject?.name);
            return bestTarget;
        }
    }


    public class LCRSItem
    {
        public Renderer Renderer;
        public Material Material;
        public Bounds Bounds;
        public ReflectionProbe RefProbe;
    }
    //public enum BoundingBox
    //{
    //    Default = 1,
    //    Average = 10,
    //    Min = 20,
    //    Max = 30
    //}

}