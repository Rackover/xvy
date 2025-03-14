Shader "Custom/SH_SimpleColored" {
Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
	_Color("Tint", Color) = (1,1,1,1)
}
SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 150

CGPROGRAM
#pragma surface surf Lambert noforwardadd

sampler2D _MainTex;
uniform half4 _Color;

struct Input {
    float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
    o.Albedo = c.rgb * _Color.rgb;
    o.Alpha = c.a;
}
ENDCG
}

Fallback "Mobile/Diffuse"
}
