Shader "Custom/SH_Water" {
Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
    _Cookie ("Cookie", 2D) = "white" {}
	_Movement ("Movement", Vector) = (1, 1, 0, 0)
	_CookieAmount ("CookieAmount", Float) = 0.3
	_Scale ("Scale", Vector) = (1, 1, 0, 0)
	_WaveScale ("WaveScale", Vector) = (1, 1, 1, 0)
	_Color("Tint", Color) = (1,1,1,1)
	_Specular("Specular color", Color) = (1,1,1,1)
}
SubShader {
    Tags { "RenderType"="Transparent" "Queue" = "Transparent"  }
	Fog { Mode Global } 
    LOD 150

CGPROGRAM
#pragma target 2.0
#pragma surface surf StandardSpecular alpha:blend vertex:vert noforwardadd 

sampler2D _MainTex;
sampler2D _Cookie;
uniform half4 _Color;
uniform half4 _Specular;
uniform half4 _Movement;
uniform half2 _Scale;
uniform half _CookieAmount;
uniform half4 _WaveScale;

struct Input {
	float3 worldPos;
    float2 uv_MainTex;      
};
void vert(inout appdata_full v){  
	  half3 position = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xyz;

	  v.vertex.y += sin(_Time.x * (position.x * _WaveScale.x + position.z * _WaveScale.z) * _WaveScale.w) * _WaveScale.z;
}

void surf (Input IN, inout SurfaceOutputStandardSpecular o) {

half2 uv =IN.uv_MainTex + _Movement.zw * _Time.x;
    fixed4 c = tex2D(_MainTex, uv);
	
     half cookie = tex2D(_Cookie, IN.worldPos.xz * _Scale.xy + _Movement.xy * _Time.x).r;

	 half cookieAmount = lerp(cookie, 1, 1 - _CookieAmount);

    o.Albedo = c.rrr * _Color.rgb * cookieAmount;
    o.Alpha = c.a * _Color.a;
	o.Specular = _Specular.rgb * cookieAmount;
}
ENDCG
}

Fallback "Mobile/Diffuse"
}
