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
		[Toggle(_SCREENSPACE_ON)] _Screenspace("Screen space", Float) = 0
		_Offset("Offset", Range( -1 , 1)) = 0
		_BottomMultiplier("BottomMultiplier", Range( 0 , 10)) = 1
		_TopMultiplier("TopMultiplier", Range( 0 , 10)) = 1

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
			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#pragma shader_feature_local _SCREENSPACE_ON


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
				#endif
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
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
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float4 ase_clipPos = UnityObjectToClipPos(v.vertex);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord2 = screenPos;
				
				o.ase_texcoord1 = v.vertex;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = vertexValue;
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);

				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				#endif
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
				#endif
				float4 screenPos = i.ase_texcoord2;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				#ifdef _SCREENSPACE_ON
				float staticSwitch13 = ase_screenPosNorm.y;
				#else
				float staticSwitch13 = i.ase_texcoord1.xyz.y;
				#endif
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
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18900
775;518;1025;688;1112.227;430.2485;2.063085;True;False
Node;AmplifyShaderEditor.PosVertexDataNode;11;-1457.869,219.8092;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScreenPosInputsNode;2;-1465.688,43.00997;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;57;-1110.302,227.8625;Inherit;False;Property;_Offset;Offset;5;0;Create;True;0;0;0;False;0;False;0;0.219;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;13;-1185.987,85.10012;Inherit;False;Property;_Screenspace;Screen space;4;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-722.4412,410.8262;Inherit;False;Property;_mult;mult;3;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;56;-782.3694,115.025;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-548.1407,269.3258;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;60;-382.1476,162.6279;Inherit;False;Property;_BottomMultiplier;BottomMultiplier;6;0;Create;True;0;0;0;False;0;False;1;5.04;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;61;-71.84479,190.837;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;59;-387.4367,-78.91473;Inherit;False;Property;_TopMultiplier;TopMultiplier;7;0;Create;True;0;0;0;False;0;False;1;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;50;61.60902,84.96559;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;58;-68.31836,-311.6422;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;14;-341.0169,-290.662;Inherit;False;Property;_Middle;Middle;1;0;Create;True;0;0;0;False;0;False;0,0.0791831,1,0;1,0.8117526,0.4103774,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;4;-350.6467,-627.6375;Inherit;False;Property;_Top;Top;0;0;Create;True;0;0;0;False;0;False;1,0,0,0;0.9528302,0.2702727,0.2112406,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;51;194.4089,-17.43473;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;49;104.8707,-357.8381;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;5;-348.2748,-7.696128;Inherit;False;Property;_Bottom;Bottom;2;0;Create;True;0;0;0;False;0;False;0,1,0.03440881,0;0,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;17;325.0297,-531.9605;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;26;334.5087,-145.1971;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;48;-99.79164,422.5442;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;52;312.8094,291.3654;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;53;527.2095,236.9654;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;3;735.5062,-134.5598;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1;1070.532,-64.01619;Float;False;True;-1;2;ASEMaterialInspector;100;1;Skybox Gradient;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;False;True;0;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;RenderType=Opaque=RenderType;True;2;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;False;0
WireConnection;13;1;11;2
WireConnection;13;0;2;2
WireConnection;56;0;13;0
WireConnection;56;1;57;0
WireConnection;6;0;56;0
WireConnection;6;1;7;0
WireConnection;61;0;60;0
WireConnection;61;1;6;0
WireConnection;50;0;61;0
WireConnection;58;0;6;0
WireConnection;58;1;59;0
WireConnection;51;0;50;0
WireConnection;49;0;58;0
WireConnection;17;0;14;0
WireConnection;17;1;4;0
WireConnection;17;2;49;0
WireConnection;26;0;5;0
WireConnection;26;1;14;0
WireConnection;26;2;51;0
WireConnection;48;0;6;0
WireConnection;52;0;48;0
WireConnection;53;0;52;0
WireConnection;3;0;26;0
WireConnection;3;1;17;0
WireConnection;3;2;6;0
WireConnection;1;0;3;0
ASEEND*/
//CHKSM=8C594F3AC4CA512D0584B3155C97CD94B52E1D7A