Shader "Transparent/Navmesh/Transparent" {

	Properties {
			_MainTex ("Texture", 2D) = "white" {}
			_Color ("Main Color", Color) = (1,1,1,1)
			_Tint ("Tint", Color) = (1,1,1,1)
			_Emission ("Emission", Color) = (0,0,0,0)
			_Scale ("Scale", float) = 1
		}
		
            
	SubShader {
		
       	Pass {
       		ColorMask 0
       		
       	}
		
		Tags {"Queue"="Transparent+1" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 200
		
		ZWrite Off
		ZTest LEqual
		Offset 0, -20
		Cull Off
		Lighting On
		
		CGPROGRAM
		// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it does not contain a surface program or both vertex and fragment programs.
		#pragma exclude_renderers gles

		#pragma  surface surf Lambert alpha vertex:vert
		
		float4 _Color;
		float4 _Tint;
		float4 _Emission;
		float _Scale;
		sampler2D _MainTex;
		
		half4 LightingWrapLambert (SurfaceOutput s, half3 lightDir, half atten) {
			
			half4 c : COLOR;// = atten;
			c = half4 (0,0,0,1);
			return c;//_Amb/8;//half4 (0.1,0.1,0.1,1);
		}
		
		struct Input {
			
			half4 customColor;
			float3 worldPos;
		};
		
		void vert (inout appdata_full v, out Input o) {
			o.customColor = v.color;
		}
		      
		void surf (Input IN, inout SurfaceOutput o) {
			//clip (frac((IN.worldPos.x+IN.worldPos.z) * 0.5) - 0.5);
			half4 c = _Color * IN.customColor;
			c = IN.customColor * tex2D (_MainTex, float2(IN.worldPos.x*_Scale,IN.worldPos.z*_Scale));
			
			//o.Albedo = c.rgb* (1+_Tint.a) * ((frac((IN.worldPos.x+IN.worldPos.z) * 0.25)-0.5) > 0 ? 1 : _Low.a);
			//o.Albedo *= (frac(IN.worldPos.x*0.1)-0.5) > 0 ? 1.1 : 0.9;
			//o.Albedo *= (frac(IN.worldPos.z*0.1)-0.5) > 0 ? 1.1 : 0.9;
			o.Alpha = c.a;
			o.Emission = _Emission;
			o.Albedo = c;
			//o.Gloss = 1;
			
			//o.Emission = ;_Emission;
		}
		ENDCG
	}
	
	Fallback "Transparent/VertexLit"
}