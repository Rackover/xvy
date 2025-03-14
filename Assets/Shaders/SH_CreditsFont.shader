﻿Shader "Custom/Credits font" {
    Properties {
        _MainTex ("Font Texture", 2D) = "white" {}
        _Color("Text Color", Color) = (1, 1, 1, 1)
        _Deform("Deform", Vector) = (0, 0, 0, 0)
        _DeformGranularity("DeformGranularity", Vector) = (1, 1, 0, 0)
        _DeformSpeed("DeformSpeed", Vector) = (5, 5, 0, 0)
    }

    SubShader {

        Tags {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }
        Lighting Off Cull Off ZTest Always ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            CGPROGRAM
            #pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
			#include "UnityCG.cginc"

			struct appdata_t
			{
			    float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform fixed4 _Color;
			uniform float2 _Deform;
			uniform float2 _DeformSpeed;
			uniform float2 _DeformGranularity;

			v2f vert(appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color * _Color;
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.vertex.xy += _Deform * sin(_Time.x * _DeformSpeed + o.vertex.yx * _DeformGranularity);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = i.color;
                col.a *= tex2D(_MainTex, i.texcoord).a;
                return col;
            }
            ENDCG
        }
    }
}