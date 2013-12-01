Shader "tk2d/LitSolidVertexColor" 
{
	Properties 
	{
	    _MainTex ("Base (RGB)", 2D) = "white" {}
	}
	
	SubShader
	{
		Tags {"IgnoreProjector"="True" "RenderType"="Opaque"}
		Cull Off Fog { Mode Off }
		LOD 110

		CGPROGRAM
		#pragma surface surf Lambert
		struct Input {
			float2 uv_MainTex;
			fixed4 color : COLOR;
		};
		sampler2D _MainTex;
		void surf(Input IN, inout SurfaceOutput o)
		{
			fixed4 mainColor = tex2D(_MainTex, IN.uv_MainTex) * IN.color;
			o.Albedo = mainColor.rgb;
		}
		ENDCG		
	}
	
	SubShader 
	{
		Tags {"IgnoreProjector"="True" "RenderType"="Opaque"}
		Blend Off Cull Off
		LOD 100
	    Pass 
	    {
			Tags {"LightMode" = "Vertex"}
			
			ColorMaterial AmbientAndDiffuse
	        Lighting On
	        
	        SetTexture [_MainTex] 
	        {
	            Combine texture * primary double, texture * primary
	        }
	    }
	}

	Fallback "tk2d/SolidVertexColor", 1
}
