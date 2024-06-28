Shader "Unlit/M_RawImageShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
	}
	SubShader
	{
		
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
		
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
         Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

		Pass
		{
            Name "Default"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma target 2.0
			// make fog work
			
			#include "UnityCG.cginc"
            #include "UnityUI.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
                float4 color    : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
                fixed4 color    : COLOR;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
			};
			 
			uniform sampler2D _MainTex;
            uniform fixed4 _Color;
            uniform fixed4 _TextureSampleAdd;
            uniform float4 _ClipRect;
			uniform float4 _MainTex_ST;

			v2f vert (appdata v)
			{
				v2f o;
				
				UNITY_INITIALIZE_OUTPUT(v2f, o);

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);

				
                col.a = UnityGet2DClipping(i.worldPosition.xy, _ClipRect);

				// apply fog
				return col;
			}
			ENDCG
		}
	}
}
