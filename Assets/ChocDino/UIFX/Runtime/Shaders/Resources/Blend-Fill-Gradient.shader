Shader "Hidden/ChocDino/UIFX/Blend-Fill-Gradient"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _ResultTex ("Sprite Texture", 2D) = "white" {}

		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_GradientAxisParams				("Axis", Vector) = (1, 0, 0, 0)
		_GradientLinearStartLine		("Start Line", Vector) = (0, 0, 0, 0)
		_GradientLinearParams			("Linear Params", Vector) = (0, 0, 0, 0)

		_GradientTransform				("Transform", Vector) = (1, 0, 0, 0)
		_GradientRadial					("Transform", Vector) = (0, 0, 1, 0)
		_GradientDither					("Dither", Float) = 0.0

		_BlendMode						("Blend Mode", Int) = 0
				
		_Strength						("Strength", Float) = 1.0

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
		_ColorMask			("Color Mask", Float) = 15
	}

	CGINCLUDE

	#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
	#pragma multi_compile_local_fragment _ UNITY_UI_ALPHACLIP
	#pragma multi_compile_local_fragment _ DITHER
	#pragma multi_compile_local_fragment _ GRADIENT_COLORSPACE_PERCEPTUAL
	#pragma multi_compile_local_fragment _ BLEND_FORCEGAMMASPACE
	#pragma multi_compile_local_fragment GRADIENT_LERP_SMOOTH GRADIENT_LERP_LINEAR GRADIENT_LERP_STEP GRADIENT_LERP_STEPAA
	#pragma multi_compile_local_fragment GRADIENT_SHAPE_AXIS GRADIENT_SHAPE_LINEAR GRADIENT_SHAPE_RADIAL GRADIENT_SHAPE_CONIC

	#include "BlendUtils.cginc"
	#include "ColorUtils.cginc"
	#include "CompUtils.cginc"
	#include "Common/GradientUtils.cginc"
	#include "Common/BlendModes.cginc"

	uniform float _Strength;
	uniform float4 _GradientAxisParams;	// ScaleX, ScaleY, OffsetX, OffsetY
	uniform int _BlendMode;
	uniform half4 _ScreenRect;

	float4 frag(v2f i) : SV_Target
	{
		// Note: This is already pre-multiplied alpha
		float4 result = tex2D(_ResultTex, i.uv.xy);

		i.uv.x *= _ScreenRect.z;
		i.uv.y *= _ScreenRect.w;
		i.uv.x += _ScreenRect.x;
		i.uv.y += _ScreenRect.y;

		float gd = 0.0;
		{
			#if GRADIENT_SHAPE_AXIS
			gd = (i.uv.x * _GradientAxisParams.x + _GradientAxisParams.z) + (i.uv.y * _GradientAxisParams.y + _GradientAxisParams.w);
			#elif GRADIENT_SHAPE_LINEAR
			{
				// Scale UV by aspect ratio
				float2 uv = i.uv * float2(_GradientLinearParams.y, 1.0);

				float2 startPoint = _GradientLinearStartLine.xy;
				float2 startDir = _GradientLinearStartLine.zw;
				float dir = dot(uv - startPoint, startDir);
				float2 closestPoint = startPoint + startDir * dir;
				gd = distance(closestPoint, uv) * 1.0;
				gd /= _GradientLinearParams.x;
			}
			#elif GRADIENT_SHAPE_RADIAL
			{
				float2 ratio = float2(_ResultTex_TexelSize.y / _ResultTex_TexelSize.x, 1.0);
				float2 uv = (i.uv.xy - 0.5) * 2.0;
				gd = RadialGradientDistance(uv * ratio, _GradientRadial.xy * ratio, _GradientRadial.z);
			}
			#elif GRADIENT_SHAPE_CONIC
			{
				float HalfTurn = 3.141592654;
				float FullTurn = HalfTurn * 2.0;
				float2 uv = float2((1.0-i.uv.x)-0.5, (1.0-i.uv.y)-0.5); //TODO: make the rotation pivot a parameter
				uv += _GradientRadial.xy;
				gd = (atan2(uv.x, uv.y) + HalfTurn) / FullTurn;
			}
			#endif
		}

		float ditherOffset = 0.0;
		#if DITHER
		float dither = _GradientDither / _GradientTransform.x;	// higher scale == less dithering
		ditherOffset = dither * gradientNoise(i.uv*_ResultTex_TexelSize.zw) - (dither * 0.5);
		#endif

		float t = Wrap(gd + ditherOffset, _GradientTransform.x, _GradientTransform.y, _GradientTransform.z, _GradientTransform.w);
		
		#if GRADIENT_LERP_LINEAR
		float4 fill = Gradient_EvalLinear(_GradientColorCount, _GradientColors, _GradientAlphaCount, _GradientAlphas, t);
		#elif GRADIENT_LERP_STEP
		float4 fill = Gradient_EvalStep(_GradientColorCount, _GradientColors, _GradientAlphaCount, _GradientAlphas, t);
		#elif GRADIENT_LERP_STEPAA
		float4 fill = Gradient_EvalStepAA(_GradientColorCount, _GradientColors, _GradientAlphaCount, _GradientAlphas, t);
		#else
		float4 fill = Gradient_EvalSmooth(_GradientColorCount, _GradientColors, _GradientAlphaCount, _GradientAlphas, t);
		#endif

		#if GRADIENT_COLORSPACE_PERCEPTUAL
			#ifndef UNITY_COLORSPACE_GAMMA
			fill.rgb = OkLabToLinear(fill.rgb);
			#else
			fill.rgb = LinearToGammaSpace(OkLabToLinear(fill.rgb));
			#endif
		#endif

		fill = ToPremultiplied(fill);
		half4 blend = DoBlend(_BlendMode, result, fill);
		fixed4 color = lerp(result, blend, _Strength);
		//fixed4 color = result*fill;

		// 2D rect clipping
		#ifdef UNITY_UI_CLIP_RECT
		color = ApplyClipRect(color, i.mask);
		#endif

		// Alpha clipping
		#ifdef UNITY_UI_ALPHACLIP
		clip (color.a - 0.001);
		#endif

		color.rgb *= i.color.a;
		color *= i.color;

		return color;
	}

	ENDCG

	SubShader
	{
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
			"OutputsPremultipliedAlpha"="True"
		}

		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend One OneMinusSrcAlpha // Premultiplied transparency
		ColorMask [_ColorMask]

		Pass
		{
			Name "Blend-Fill-Gradient"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
}