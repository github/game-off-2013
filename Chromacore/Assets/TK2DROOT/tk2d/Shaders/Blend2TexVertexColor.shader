// unlit, vertex color, 2 textures, alpha blended
// cull off

Shader "tk2d/Blend2TexVertexColor" 
{
	Properties 
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_GradientTex ("Gradient (RGBA)", 2D) = "white" {}
	}

	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite Off Lighting Off Cull Off Fog { Mode Off } Blend SrcAlpha OneMinusSrcAlpha
		LOD 110
		
		Pass 
		{
			CGPROGRAM
			#pragma vertex vert_vctt
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			sampler2D _GradientTex;

			struct vin_vctt
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};

			struct v2f_vctt
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float4 texcoord01 : TEXCOORD0;
			};

			v2f_vctt vert_vctt(vin_vctt v)
			{
				v2f_vctt o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;
				o.texcoord01.xy = v.texcoord;
				o.texcoord01.zw = v.texcoord1;
				return o;
			}

			fixed4 frag(v2f_vctt i) : COLOR
			{
				fixed4 col = tex2D(_MainTex, i.texcoord01.xy) * tex2D(_GradientTex, i.texcoord01.zw) * i.color;
				return col;
			}
			
			ENDCG
		} 
	}
 
	SubShader 
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite Off Blend SrcAlpha OneMinusSrcAlpha Cull Off Fog { Mode Off }
		LOD 100

		BindChannels 
		{
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord0
			Bind "TexCoord1", texcoord1
			Bind "Color", color
		}

		Pass 
		{
			Lighting Off
			SetTexture [_MainTex] { combine texture * primary } 
			SetTexture [_GradientTex] { combine texture * previous }	
		}
	}
}
