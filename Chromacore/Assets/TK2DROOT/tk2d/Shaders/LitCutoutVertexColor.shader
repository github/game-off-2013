Shader "tk2d/LitCutoutVertexColor" 
{
	Properties 
	{
	    _MainTex ("Base (RGB)", 2D) = "white" {}
	    _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
	}
	
	SubShader
	{
		Tags {"IgnoreProjector"="True" "RenderType"="TransparentCutout"}
		Cull Off Fog { Mode Off }
		LOD 110

		CGPROGRAM
		#pragma surface surf Lambert alphatest:_Cutoff
		struct Input {
			float2 uv_MainTex;
			fixed4 color : COLOR;
		};
		sampler2D _MainTex;
		void surf(Input IN, inout SurfaceOutput o)
		{
			fixed4 mainColor = tex2D(_MainTex, IN.uv_MainTex) * IN.color;
			o.Albedo = mainColor.rgb;
			o.Alpha = mainColor.a;
		}
		ENDCG		
	}
	
	SubShader 
	{
		Tags {"IgnoreProjector"="True" "RenderType"="TransparentCutout"}
		AlphaTest Greater 0.5 Blend Off	Cull Off
		LOD 100
	    Pass 
	    {
			Tags {"LightMode" = "Vertex" }
	    
			ColorMaterial AmbientAndDiffuse
	        Lighting On
	        
	        SetTexture [_MainTex] 
	        {
	            Combine texture * primary double, texture * primary
	        }
	    }
	}

	Fallback "tk2d/BlendVertexColor", 1
}
