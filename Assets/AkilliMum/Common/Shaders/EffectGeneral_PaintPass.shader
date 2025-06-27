// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "AkilliMum/Particles/Effect/General_PaintPass" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}
	_ColorStrength ("Color strength", Float) = 1.0
	_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0

		//[Header(Blending)]
		  // https://docs.unity3d.com/ScriptReference/Rendering.BlendMode.html
	   [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlendEx("_SrcBlendEx (default = SrcAlpha)", Float) = 5 // 5 = SrcAlpha
	   [Enum(UnityEngine.Rendering.BlendMode)]_DstBlendEx("_DstBlendEx (default = OneMinusSrcAlpha)", Float) = 10 // 10 = OneMinusSrcAlpha


   //[Header(ZTest)]
   // https://docs.unity3d.com/ScriptReference/Rendering.CompareFunction.html
   // default need to be Disable, because we need to make sure decal render correctly even if camera goes into decal cube volume, although disable ZTest by default will prevent EarlyZ (bad for GPU performance)
	   [Enum(UnityEngine.Rendering.CompareFunction)]_ZTestEx("_ZTest (default = Disable or LessEqual", Float) = 0 //0 = disable

	   //[Header(Cull)]
	   // https://docs.unity3d.com/ScriptReference/Rendering.CullMode.html
	   // default need to be Front, because we need to make sure decal render correctly even if camera goes into decal cube
	   [Enum(UnityEngine.Rendering.CullMode)]_CullEx("_Cull (default = Back)", Float) = 2 //1 = Front, 2 Back

	   [Toggle(_ZWriteEx)] _ZWriteEx("_ZWriteEx (default = On)", Float) = 1
}

Category {
	Tags
	{
		//"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" 
		"Queue" = "Transparent"
		"IgnoreProjector" = "True"
		"RenderType" = "Transparent"
		"LightMode" = "CarPaint"
	}
	Blend[_SrcBlendEx][_DstBlendEx]
	ZWrite[_ZWriteEx]
	Cull[_CullEx]
	/*Blend SrcAlpha One
	Cull Off */
	Lighting Off 
	/*ZWrite Off */
	Fog { Mode Off}
	
	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_particles

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _TintColor;
			fixed _ColorStrength;
			
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				#ifdef SOFTPARTICLES_ON
				float4 projPos : TEXCOORD1;
				#endif
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				#ifdef SOFTPARTICLES_ON
				o.projPos = ComputeScreenPos (o.vertex);
				COMPUTE_EYEDEPTH(o.projPos.z);
				#endif
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}

			sampler2D _CameraDepthTexture;
			float _InvFade;
			
			fixed4 frag (v2f i) : COLOR
			{
				#ifdef SOFTPARTICLES_ON
				float sceneZ = LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
				float partZ = i.projPos.z;
				float fade = saturate (_InvFade * (sceneZ-partZ));
				i.color.a *= fade;
				#endif
				
				return 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord) * _ColorStrength;
			}
			ENDCG 
		}
	}	
}
}
