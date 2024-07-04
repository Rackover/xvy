// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Skybox Gradient"
{
	Properties
	{
		_Top("Top", Color) = (1,0,0,0)
		_Middle("Middle", Color) = (0,0.0791831,1,0)
		_Bottom("Bottom", Color) = (0,1,0.03440881,0)
		_mult("mult", Float) = 1
		_Offset("Offset", Range( -1 , 1)) = 0
		_BottomMultiplier("BottomMultiplier", Range( 0 , 50)) = 1
		_TopMultiplier("TopMultiplier", Range( 0 , 50)) = 1

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend Off
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				
			};

			uniform float4 _Bottom;
			uniform float4 _Middle;
			uniform float _BottomMultiplier;
			uniform float _Offset;
			uniform float _mult;
			uniform float4 _Top;
			uniform float _TopMultiplier;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);

				float4 ase_clipPos = UnityObjectToClipPos(v.vertex);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord2 = screenPos;
				
				o.ase_texcoord1 = v.vertex;
				float3 vertexValue = float3(0, 0, 0);
				vertexValue = vertexValue;
				v.vertex.xyz += vertexValue;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				fixed4 finalColor;
				float4 screenPos = i.ase_texcoord2;
				float staticSwitch13 = i.ase_texcoord1.xyz.y;
				float temp_output_6_0 = ( ( staticSwitch13 + _Offset ) * _mult );
				float clampResult50 = clamp( ( _BottomMultiplier * temp_output_6_0 ) , -1.0 , 0.0 );
				float4 lerpResult26 = lerp( _Bottom , _Middle , ( clampResult50 + 1.0 ));
				float clampResult49 = clamp( ( temp_output_6_0 * _TopMultiplier ) , 0.0 , 1.0 );
				float4 lerpResult17 = lerp( _Middle , _Top , clampResult49);
				float4 lerpResult3 = lerp( lerpResult26 , lerpResult17 , temp_output_6_0);
				
				
				finalColor = lerpResult3;
				return finalColor;
			}
			ENDCG
		}
	}
	
	
}
