
#if UNITY_2020_2_OR_NEWER

using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor.Rendering.Universal;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditor.Rendering.Universal.ShaderGUI;
//using UnityEditor.Rendering.Universal.ShaderGUI;

namespace AkilliMum.SRP.Mirror
{
    internal class ComplexEditor : BaseShaderGUI
    {
        private LitGUI.LitProperties litProperties;
        private LitDetailGUI.LitProperties litDetailProperties;
        private SavedBool m_DetailInputsFoldout;

        private bool MenuLocallyCorrection = true;
        private bool MenuReflection = true;

        public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties)
        {
            Material targetMat = materialEditorIn.target as Material;


            //MaterialProperty _EnviCubeMapBaked = ShaderGUI.FindProperty("_EnviCubeMapBaked", properties);
            //materialEditorIn.ShaderProperty(_EnviCubeMapBaked, "Custom Cube Map");

            #region Locally Correction

            {
                MaterialProperty _EnableLocallyCorrection =
                    ShaderGUI.FindProperty("_EnableLocallyCorrection", properties);

                MenuLocallyCorrection = EditorGUILayout.BeginFoldoutHeaderGroup(MenuLocallyCorrection,
                    new GUIContent { text = "Locally Correction" });

                bool enableLCRS = false;
                if (MenuLocallyCorrection)
                {
                    enableLCRS = _EnableLocallyCorrection.floatValue > 0.5f;
                    enableLCRS = EditorGUILayout.Toggle("Enable", enableLCRS);
                    _EnableLocallyCorrection.floatValue = enableLCRS ? 1.0f : -1.0f;

                    //if (realTimeRef)
                    //{
                    //    MaterialProperty _EnviCubeIntensity = ShaderGUI.FindProperty("_EnviCubeIntensity", properties);
                    //    materialEditorIn.ShaderProperty(_EnviCubeIntensity, "Intensity");

                    //    MaterialProperty _EnviCubeSmoothness = ShaderGUI.FindProperty("_EnviCubeSmoothness", properties);
                    //    materialEditorIn.ShaderProperty(_EnviCubeSmoothness, "Smoothness");
                    //}
                }

                EditorGUILayout.EndFoldoutHeaderGroup();

                if (enableLCRS)
                {
                    targetMat.EnableKeyword("_LOCALLYCORRECTION");
                    //Debug.Log("Enabled LCRS");
                }
                else
                {
                    targetMat.DisableKeyword("_LOCALLYCORRECTION");
                    //Debug.Log("Disabled LCRS");
                }
            }

            #endregion


            #region Reflection
            {
                MenuReflection = EditorGUILayout.BeginFoldoutHeaderGroup(MenuReflection,
                    new GUIContent { text = "Reflection" });

                MaterialProperty _DisableGI =
                    ShaderGUI.FindProperty("_DisableGI", properties);
                bool disableGI = _DisableGI.floatValue > 0.5f;

                MaterialProperty _UseFresnel =
                    ShaderGUI.FindProperty("_UseFresnel", properties);
                bool useFresnel = _UseFresnel.floatValue > 0.5f;

                MaterialProperty _EnableSimpleDepth =
                    ShaderGUI.FindProperty("_EnableSimpleDepth", properties);
                bool useDepth = _EnableSimpleDepth.floatValue > 0.5f;

                MaterialProperty _EnableDepthBlur =
                    ShaderGUI.FindProperty("_EnableDepthBlur", properties);
                MaterialProperty _MixBlackColor =
                    ShaderGUI.FindProperty("_MixBlackColor", properties);
                bool useDepthBlur = _EnableDepthBlur.floatValue > 0.5f;

                if (MenuReflection)
                {
                    //intensity
                    MaterialProperty _ReflectionIntensity = ShaderGUI.FindProperty("_ReflectionIntensity", properties);
                    materialEditorIn.ShaderProperty(_ReflectionIntensity, new GUIContent
                    {
                        text = "Intensity",
                        tooltip = "Intensity of reflection, like '0' = no reflection, '1' = full mirror"
                    });

                    //GI
                    disableGI = EditorGUILayout.Toggle(new GUIContent
                    {
                        text = "Disable GI",
                        tooltip =
                            "Disables GI to create full mirror, so it does not affected by GI and creates perfect reflection"
                    }, disableGI);
                    _DisableGI.floatValue = disableGI ? 1.0f : -1.0f;

                    //Fresnel Like Reflection
                    useFresnel = EditorGUILayout.Toggle(new GUIContent
                    {
                        text = "Fresnel Like Reflection",
                        tooltip =
                            "Uses fresnel like reflection, so far away objects (pixels) creates more reflectiveness"
                    }, useFresnel);
                    _UseFresnel.floatValue = useFresnel ? 1.0f : -1.0f;
                    if (useFresnel)
                    {
                        MaterialProperty _UseFresnelPower =
                            ShaderGUI.FindProperty("_UseFresnelPower", properties);
                        materialEditorIn.ShaderProperty(_UseFresnelPower, new GUIContent
                        {
                            text = "\tPower",
                            tooltip = "Power of the fresnel-ity"
                        });
                    }

                    //mip level
                    MaterialProperty _LODLevel = ShaderGUI.FindProperty("_LODLevel", properties);
                    materialEditorIn.ShaderProperty(_LODLevel, new GUIContent
                    {
                        text = "Mip Level",
                        tooltip = "Mip level of the texture to be used. Warning: Mip Mapping must be enabled on MirrorManager script!"
                    });

                    //refraction
                    MaterialProperty _RefractionTex = ShaderGUI.FindProperty("_RefractionTex", properties);
                    materialEditorIn.TexturePropertySingleLine(
                        new GUIContent
                        {
                            text = "Refraction Map",
                            tooltip = "Refraction normal map to mimic refraction on reflection"
                        }, _RefractionTex);
                    //MaterialProperty _EnableRefraction = ShaderGUI.FindProperty("_EnableRefraction", properties);
                    if (_RefractionTex.textureValue != null)
                    {
                        //_EnableRefraction.floatValue = 1;
                        CoreUtils.SetKeyword(targetMat, "_AKMU_MIRROR_REFRACTION", true);

                        MaterialProperty _ReflectionRefraction =
                            ShaderGUI.FindProperty("_ReflectionRefraction", properties);
                        materialEditorIn.ShaderProperty(_ReflectionRefraction, new GUIContent
                        {
                            text = "\tIntensity",
                            tooltip = "Refraction intensity to refract more or less"
                        });
                    }
                    else
                    {
                        //_EnableRefraction.floatValue = -1;
                        CoreUtils.SetKeyword(targetMat, "_AKMU_MIRROR_REFRACTION", false);
                    }

                    //wave
                    MaterialProperty _WaveNoiseTex = ShaderGUI.FindProperty("_WaveNoiseTex", properties);
                    materialEditorIn.TexturePropertySingleLine(
                        new GUIContent
                        {
                            text = "Wave Map",
                            tooltip = "Wave normal map to mimic waves on reflection"
                        }, _WaveNoiseTex);
                    //MaterialProperty _EnableWave = ShaderGUI.FindProperty("_EnableWave", properties);
                    if (_WaveNoiseTex.textureValue != null)
                    {
                        //_EnableWave.floatValue = 1;
                        CoreUtils.SetKeyword(targetMat, "_AKMU_MIRROR_WAVE", true);

                        MaterialProperty _WaveSize =
                            ShaderGUI.FindProperty("_WaveSize", properties);
                        materialEditorIn.ShaderProperty(_WaveSize, new GUIContent
                        {
                            text = "\tSize",
                            tooltip = "Size of the waves"
                        });

                        MaterialProperty _WaveDistortion =
                            ShaderGUI.FindProperty("_WaveDistortion", properties);
                        materialEditorIn.ShaderProperty(_WaveDistortion, new GUIContent
                        {
                            text = "\tDistortion",
                            tooltip = "Distortion amount of the waves"
                        });

                        MaterialProperty _WaveSpeed =
                            ShaderGUI.FindProperty("_WaveSpeed", properties);
                        materialEditorIn.ShaderProperty(_WaveSpeed, new GUIContent
                        {
                            text = "\tSpeed",
                            tooltip = "Speed of the waves according to the time"
                        });
                    }
                    else
                    {
                        //_EnableWave.floatValue = -1;
                        CoreUtils.SetKeyword(targetMat, "_AKMU_MIRROR_WAVE", false);
                    }

                    //[HideInInspector]_EnableRipple("Enable Ripples", Float) = -1.0
                    //    [HideInInspector]_RippleTex("Ripple", 2D) = "bump" { }
                    //[HideInInspector]_RippleSize("Ripple Size", Float) = 2.0
                    //    [HideInInspector]_RippleRefraction("Ripple Refraction", Float) = 0.02
                    //    [HideInInspector]_RippleDensity("Ripple Density", Float) = 1.0
                    //    [HideInInspector]_RippleSpeed("Ripple Speed", Float) = 0.3
                    //ripple
                    MaterialProperty _RippleTex = ShaderGUI.FindProperty("_RippleTex", properties);
                    materialEditorIn.TexturePropertySingleLine(
                        new GUIContent
                        {
                            text = "Ripple Map",
                            tooltip = "Ripple normal map to mimic ripples on reflection"
                        }, _RippleTex);
                    //MaterialProperty _EnableRipple = ShaderGUI.FindProperty("_EnableRipple", properties);
                    if (_RippleTex.textureValue != null)
                    {
                        //_EnableRipple.floatValue = 1;
                        CoreUtils.SetKeyword(targetMat, "_AKMU_MIRROR_RIPPLE", true);

                        MaterialProperty _RippleSize =
                            ShaderGUI.FindProperty("_RippleSize", properties);
                        materialEditorIn.ShaderProperty(_RippleSize, new GUIContent
                        {
                            text = "\tSize",
                            tooltip = "Size of the ripples"
                        });

                        MaterialProperty _RippleRefraction =
                            ShaderGUI.FindProperty("_RippleRefraction", properties);
                        materialEditorIn.ShaderProperty(_RippleRefraction, new GUIContent
                        {
                            text = "\tDistortion",
                            tooltip = "Distortion amount of the ripples"
                        });

                        MaterialProperty _RippleSpeed =
                            ShaderGUI.FindProperty("_RippleSpeed", properties);
                        materialEditorIn.ShaderProperty(_RippleSpeed, new GUIContent
                        {
                            text = "\tSpeed",
                            tooltip = "Speed of the ripples according to the time"
                        });

                        MaterialProperty _RippleDensity =
                            ShaderGUI.FindProperty("_RippleDensity", properties);
                        materialEditorIn.ShaderProperty(_RippleDensity, new GUIContent
                        {
                            text = "\tDensity",
                            tooltip = "Density of the ripples (hard-soft etc.)"
                        });
                    }
                    else
                    {
                        //_EnableRipple.floatValue = -1;
                        CoreUtils.SetKeyword(targetMat, "_AKMU_MIRROR_RIPPLE", false);
                    }

                    //depth
                    useDepth = EditorGUILayout.Toggle(new GUIContent
                    {
                        text = "Depth",
                        tooltip =
                            "Use reflection depth to mimic some fade-off on reflection. Warning: Depth must be enabled on MirrorManager script too!"
                    }, useDepth);
                    _EnableSimpleDepth.floatValue = useDepth ? 1.0f : -1.0f;
                    if (useDepth)
                    {
                        MaterialProperty _SimpleDepthCutoff =
                            ShaderGUI.FindProperty("_SimpleDepthCutoff", properties);
                        materialEditorIn.ShaderProperty(_SimpleDepthCutoff, new GUIContent
                        {
                            text = "\tCut-Off",
                            tooltip = "Depth cut-off value to set start-end reflection fade-off"
                        });
                    }

                    //depth blur
                    useDepthBlur = EditorGUILayout.Toggle(new GUIContent
                    {
                        text = "Depth Blur",
                        tooltip =
                            "Use advanced depth calculations to mimic some fade-off and blur on reflection. Warning: Depth Blur must be enabled on MirrorManager script too!"
                    }, useDepthBlur);
                    //_EnableDepthBlur.floatValue = _MixBlackColor.floatValue = useDepthBlur ? 1.0f : -1.0f;
                    _EnableDepthBlur.floatValue = useDepthBlur ? 1.0f : -1.0f;
                    
                    //mask
                    MaterialProperty _MaskTex = ShaderGUI.FindProperty("_MaskTex", properties);
                    materialEditorIn.TexturePropertySingleLine(
                        new GUIContent
                        {
                            text = "Alpha Mask Map",
                            tooltip = "Alpha mask texture to create some transparent areas on reflection (like puddles)"
                        }, _MaskTex);
                    //MaterialProperty _EnableMask = ShaderGUI.FindProperty("_EnableMask", properties);
                    if (_MaskTex.textureValue != null)
                    {
                        //_EnableMask.floatValue = 1;
                        CoreUtils.SetKeyword(targetMat, "_AKMU_MIRROR_MASK", true);

                        MaterialProperty _MaskCutoff =
                            ShaderGUI.FindProperty("_MaskCutoff", properties);
                        materialEditorIn.ShaderProperty(_MaskCutoff, new GUIContent
                        {
                            text = "\tCut-Off",
                            tooltip = "Cut-Off value to set start-end alpha fade-off"
                        });

                        MaterialProperty _MaskEdgeDarkness =
                            ShaderGUI.FindProperty("_MaskEdgeDarkness", properties);
                        materialEditorIn.ShaderProperty(_MaskEdgeDarkness, new GUIContent
                        {
                            text = "\tEdge Darkness",
                            tooltip = "To create darkness on edges of the mask to mimic intensity (like mud)"
                        });

                        MaterialProperty _MaskTiling =
                            ShaderGUI.FindProperty("_MaskTiling", properties);
                        materialEditorIn.ShaderProperty(_MaskTiling, new GUIContent
                        {
                            text = "\tTiling",
                            tooltip = "Tiling of the mask texture to fit on object as you want"
                        });
                    }
                    else
                    {
                        //_EnableMask.floatValue = -1;
                        CoreUtils.SetKeyword(targetMat, "_AKMU_MIRROR_MASK", false);
                    }
                }

                EditorGUILayout.EndFoldoutHeaderGroup();

                //enable disable GI with keyword
                if (disableGI)
                {
                    targetMat.EnableKeyword("_FULLMIRROR");
                }
                else
                {
                    targetMat.DisableKeyword("_FULLMIRROR");
                }
            }

            #endregion



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