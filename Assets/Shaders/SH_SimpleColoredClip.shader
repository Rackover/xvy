Shader "Custom/SH_SimpleColoredClip" {
Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
	_Color("Tint", Color) = (1,1,1,1)
}
SubShader {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Overlay" }
	Fog { Mode Off }
	Cull Off
	ZWrite Off

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

	if (c.a < 0.5) {
		discard;
	}

    o.Alpha = c.a;
}
ENDCG
}

Fallback "Mobile/Diffuse"
}
