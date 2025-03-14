Shader "Custom/SH_CookieCOlored" {
Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
    _Cookie ("Cookie", 2D) = "white" {}
	_Movement ("Movement", Vector) = (1, 1, 0, 0)
	_CookieAmount ("CookieAmount", Float) = 0.3
	_DesaturateAmount ("Desaturate", Float) = 0.0
	_Scale ("Scale", Vector) = (1, 1, 0, 0)
	_Color("Tint", Color) = (1,1,1,1)
}
SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 150

CGPROGRAM
#pragma target 2.0
#pragma surface surf Lambert noforwardadd

sampler2D _MainTex;
sampler2D _Cookie;
uniform half4 _Color;
uniform half2 _Movement;
uniform half2 _Scale;
uniform half _CookieAmount;
uniform half _DesaturateAmount;


struct Input {
	float3 worldPos;
    float2 uv_MainTex;      
};

void surf (Input IN, inout SurfaceOutput o) {
	const float3 LuminanceWeights = float3(0.299,0.587,0.114);
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
	
	float luminance = dot(c, LuminanceWeights);

     half cookie = tex2D(_Cookie, IN.worldPos.xz * _Scale.xy + _Movement.xy * _Time.x).r;


    o.Albedo = lerp(c.rgb, half3(luminance, luminance, luminance), _DesaturateAmount) * _Color.rgb * lerp(cookie, 1, 1 - _CookieAmount);
    o.Alpha = c.a;
}
ENDCG
}

Fallback "Mobile/Diffuse"
}
