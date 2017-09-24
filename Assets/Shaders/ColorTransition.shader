Shader "Custom/ColorTransition"
{
	Properties
	{
		_ColorA ("Color A", Color) = (1,1,1,1)
		_ColorB ("Color B", Color) = (1,1,1,1)
		_Cutoff ("Cutoff", Range(0,1)) = 0.0
		_CutoffRamp ("Cutoff Ramp", Range(0,1)) = 0.0
		_MainTex ("Cutoff Texture (RGB)", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input
		{
			float2 uv_MainTex;
		};

		fixed4 _ColorA;
		fixed4 _ColorB;
		fixed _Cutoff;
		fixed _CutoffRamp;

		void surf (Input IN, inout SurfaceOutput o)
		{
			fixed4 lookup = tex2D(_MainTex, IN.uv_MainTex);
			fixed delta = clamp((lookup.r - (_Cutoff - _CutoffRamp)) / (_CutoffRamp * 2.0), 0.0, 1.0);
			fixed4 albedo = lerp(_ColorB, _ColorA, delta);

			o.Albedo = albedo.rgb;
			o.Alpha = albedo.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
