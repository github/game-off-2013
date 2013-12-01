Shader "Hidden/tk2d/EditorUtility" 
{
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZTest Always ZWrite Off Lighting Off Cull Off Fog { Mode Off } Blend SrcAlpha OneMinusSrcAlpha AlphaTest Greater 0
		LOD 110
		
		Pass 
		{
			CGPROGRAM
			#pragma vertex vert_vct
			#pragma fragment frag_mult 
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _Tint = float4(1,1,1,1);
			float4 _Clip;

			struct vin_vct 
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f_vct
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float2 w : TEXCOORD1;
			};

			v2f_vct vert_vct(vin_vct v)
			{
				v2f_vct o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = v.texcoord;
				o.w = mul(UNITY_MATRIX_MV, v.vertex).xy;
				return o;
			}

			fixed4 frag_mult(v2f_vct i) : COLOR
			{
				fixed4 col = tex2D(_MainTex, i.texcoord) * _Tint;
				if (i.w.x < _Clip.x || i.w.x > _Clip.z || i.w.y < _Clip.y || i.w.y > _Clip.w) col.a = 0;
				return col;
			}
		
			ENDCG
		} 
	}
}
