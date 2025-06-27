
#if UNITY_2020_2_OR_NEWER

using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor.Rendering.Universal;
using UnityEditor;
using UnityEditor.Rendering.Universal.ShaderGUI;
using UnityEditor.Rendering;
//using UnityEditor.Rendering.Universal.ShaderGUI;

namespace AkilliMum.SRP.LCRS
{
    internal class LCRS_2020_2_Editor : BaseShaderGUI
    {

        private LitGUI.LitProperties litProperties;
        private LitDetailGUI.LitProperties litDetailProperties;
        private SavedBool m_DetailInputsFoldout;

        private bool MenuRotation = true;

        public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties)
        {
            Material targetMat = materialEditorIn.target as Material;

            //MaterialProperty _EnviCubeMapBaked = ShaderGUI.FindProperty("_EnviCubeMapBaked", properties);
            //materialEditorIn.ShaderProperty(_EnviCubeMapBaked, "Custom Cube Map");

            MaterialProperty _EnableRotation = ShaderGUI.FindProperty("_EnableRotation", properties);

            MenuRotation = EditorGUILayout.BeginFoldoutHeaderGroup(MenuRotation, new GUIContent { text = "Rotation" });

            bool enableRotation = false;
            if (MenuRotation)
            {
                enableRotation = _EnableRotation.floatValue > 0.5f;
                enableRotation = EditorGUILayout.Toggle("Enable Rotation", enableRotation);
                _EnableRotation.floatValue = enableRotation ? 1.0f : 0.0f;

                if (enableRotation)
                {
                    MaterialProperty rotation = ShaderGUI.FindProperty("_EnviRotation", properties);
                    materialEditorIn.ShaderProperty(rotation, "Rotation");

                    MaterialProperty position = ShaderGUI.FindProperty("_EnviPosition", properties);
                    materialEditorIn.ShaderProperty(position, "Position Correction");

                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            if (enableRotation)
                targetMat.EnableKeyword("_LCRS_PROBE_ROTATION");
            else
                targetMat.DisableKeyword("_LCRS_PROBE_ROTATION");

            //call base!
            base.OnGUI(materialEditorIn, properties);

        }

#if UNITY_2021_2_OR_NEWER
        //?
#else
        public override void OnOpenGUI(Material material, MaterialEditor materialEditor)
        {
            base.OnOpenGUI(material, materialEditor);
            m_DetailInputsFoldout = new SavedBool($"{headerStateKey}.DetailInputsFoldout", true);
        }
#endif

#if UNITY_2021_2_OR_NEWER
        public override void FillAdditionalFoldouts(MaterialHeaderScopeList materialScopesList)
        {
            materialScopesList.RegisterHeaderScope(LitDetailGUI.Styles.detailInputs, Expandable.Details, _ => LitDetailGUI.DoDetailArea(litDetailProperties, materialEditor));
        }
#else
        public override void DrawAdditionalFoldouts(Material material)
        {
            m_DetailInputsFoldout.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_DetailInputsFoldout.value, LitDetailGUI.Styles.detailInputs);
            if (m_DetailInputsFoldout.value)
            {
                LitDetailGUI.DoDetailArea(litDetailProperties, materialEditor);
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
#endif
        // collect properties from the material properties
        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            litProperties = new LitGUI.LitProperties(properties);
            litDetailProperties = new LitDetailGUI.LitProperties(properties);
        }

        // material changed check
        [Obsolete]
        public override void MaterialChanged(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            SetMaterialKeywords(material, LitGUI.SetMaterialKeywords, LitDetailGUI.SetMaterialKeywords);
        }

        // material main surface options
        public override void DrawSurfaceOptions(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            // Detect any changes to the material
            EditorGUI.BeginChangeCheck();
            if (litProperties.workflowMode != null)
            {
                DoPopup(LitGUI.Styles.workflowModeText, litProperties.workflowMode, Enum.GetNames(typeof(LitGUI.WorkflowMode)));
            }
            //if (EditorGUI.EndChangeCheck())
            //{
            //    foreach (var obj in blendModeProp.targets)
            //        MaterialChanged((Material)obj);
            //}
            base.DrawSurfaceOptions(material);
        }

        // material main surface inputs
        public override void DrawSurfaceInputs(Material material)
        {
            base.DrawSurfaceInputs(material);
            LitGUI.Inputs(litProperties, materialEditor, material);
            DrawEmissionProperties(material, true);
            DrawTileOffset(materialEditor, baseMapProp);
        }

        // material main advanced options
        public override void DrawAdvancedOptions(Material material)
        {
            if (litProperties.reflections != null && litProperties.highlights != null)
            {
                EditorGUI.BeginChangeCheck();
                materialEditor.ShaderProperty(litProperties.highlights, LitGUI.Styles.highlightsText);
                materialEditor.ShaderProperty(litProperties.reflections, LitGUI.Styles.reflectionsText);
                //if (EditorGUI.EndChangeCheck())
                //{
                //    MaterialChanged(material);
                //}
            }

            base.DrawAdvancedOptions(material);
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (material.HasProperty("_Emission"))
            {
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));
            }

            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                SetupMaterialBlendMode(material);
                return;
            }

            SurfaceType surfaceType = SurfaceType.Opaque;
            BlendMode blendMode = BlendMode.Alpha;
            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                surfaceType = SurfaceType.Opaque;
                material.SetFloat("_AlphaClip", 1);
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                // NOTE: legacy shaders did not provide physically based transparency
                // therefore Fade mode
                surfaceType = SurfaceType.Transparent;
                blendMode = BlendMode.Alpha;
            }
            material.SetFloat("_Surface", (float)surfaceType);
            material.SetFloat("_Blend", (float)blendMode);

            if (oldShader.name.Equals("Standard (Specular setup)"))
            {
                material.SetFloat("_WorkflowMode", (float)LitGUI.WorkflowMode.Specular);
                Texture texture = material.GetTexture("_SpecGlossMap");
                if (texture != null)
                    material.SetTexture("_MetallicSpecGlossMap", texture);
            }
            else
            {
                material.SetFloat("_WorkflowMode", (float)LitGUI.WorkflowMode.Metallic);
                Texture texture = material.GetTexture("_MetallicGlossMap");
                if (texture != null)
                    material.SetTexture("_MetallicSpecGlossMap", texture);
            }

            //MaterialChanged(material);
        }
    }

    internal class LitDetailGUI
    {
        public static class Styles
        {
            public static readonly GUIContent detailInputs = new GUIContent("Detail Inputs",
                "These settings let you add details to the surface.");

            public static readonly GUIContent detailMaskText = new GUIContent("Mask",
                "Select a mask for the Detail maps. The mask uses the alpha channel of the selected texture. The __Tiling__ and __Offset__ settings have no effect on the mask.");

            public static readonly GUIContent detailAlbedoMapText = new GUIContent("Base Map",
                "Select the texture containing the surface details.");

            public static readonly GUIContent detailNormalMapText = new GUIContent("Normal Map",
                "Select the texture containing the normal vector data.");

            public static readonly GUIContent detailAlbedoMapScaleInfo = new GUIContent("Setting the scaling factor to a value other than 1 results in a less performant shader variant.");
        }

        public struct LitProperties
        {
            public MaterialProperty detailMask;
            public MaterialProperty detailAlbedoMapScale;
            public MaterialProperty detailAlbedoMap;
            public MaterialProperty detailNormalMapScale;
            public MaterialProperty detailNormalMap;

            public LitProperties(MaterialProperty[] properties)
            {
                detailMask = BaseShaderGUI.FindProperty("_DetailMask", properties, false);
                detailAlbedoMapScale = BaseShaderGUI.FindProperty("_DetailAlbedoMapScale", properties, false);
                detailAlbedoMap = BaseShaderGUI.FindProperty("_DetailAlbedoMap", properties, false);
                detailNormalMapScale = BaseShaderGUI.FindProperty("_DetailNormalMapScale", properties, false);
                detailNormalMap = BaseShaderGUI.FindProperty("_DetailNormalMap", properties, false);
            }
        }

        public static void DoDetailArea(LitProperties properties, MaterialEditor materialEditor)
        {
            materialEditor.TexturePropertySingleLine(Styles.detailMaskText, properties.detailMask);
            materialEditor.TexturePropertySingleLine(Styles.detailAlbedoMapText, properties.detailAlbedoMap,
                properties.detailAlbedoMap.textureValue != null ? properties.detailAlbedoMapScale : null);
            if (properties.detailAlbedoMapScale.floatValue != 1.0f)
            {
                EditorGUILayout.HelpBox(Styles.detailAlbedoMapScaleInfo.text, MessageType.Info, true);
            }
            materialEditor.TexturePropertySingleLine(Styles.detailNormalMapText, properties.detailNormalMap,
                properties.detailNormalMap.textureValue != null ? properties.detailNormalMapScale : null);
            materialEditor.TextureScaleOffsetProperty(properties.detailAlbedoMap);
        }

        public static void SetMaterialKeywords(Material material)
        {
            if (material.HasProperty("_DetailAlbedoMap") && material.HasProperty("_DetailNormalMap") && material.HasProperty("_DetailAlbedoMapScale"))
            {
                bool isScaled = material.GetFloat("_DetailAlbedoMapScale") != 1.0f;
                bool hasDetailMap = material.GetTexture("_DetailAlbedoMap") || material.GetTexture("_DetailNormalMap");
                CoreUtils.SetKeyword(material, "_DETAIL_MULX2", !isScaled && hasDetailMap);
                CoreUtils.SetKeyword(material, "_DETAIL_SCALED", isScaled && hasDetailMap);
            }
        }
    }

    internal class SavedBool
    {
        private bool m_Value;
        private string m_Name;

        public bool value
        {
            get
            {
                return this.m_Value;
            }
            set
            {
                if (this.m_Value == value)
                    return;
                this.m_Value = value;
                EditorPrefs.SetBool(this.m_Name, value);
            }
        }

        public SavedBool(string name, bool value)
        {
            this.m_Name = name;
            this.m_Value = EditorPrefs.GetBool(name, value);
        }

        public static implicit operator bool(SavedBool s)
        {
            return s.value;
        }
    }
}

#endif