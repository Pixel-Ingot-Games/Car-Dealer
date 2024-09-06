// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ITS/Vertex Colors"
{
	Properties
	{
		_Float0("Float 0", Range( 0 , 1)) = 0
		_Smoth("Smoth", Range( 0 , 1)) = 0.8769649
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float4 vertexColor : COLOR;
		};

		uniform float _Float0;
		uniform float _Smoth;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 lerpResult13 = lerp( float4( 0,0,0,0 ) , float4(1,1,1,1) , i.vertexColor);
			o.Albedo = ( ( lerpResult13 * i.vertexColor ) * i.vertexColor ).xyz;
			float temp_output_20_0 = ( 1.0 - i.vertexColor.a );
			float lerpResult23 = lerp( 0.0 , _Float0 , temp_output_20_0);
			o.Metallic = lerpResult23;
			float lerpResult25 = lerp( 0.1 , _Smoth , temp_output_20_0);
			o.Smoothness = lerpResult25;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18900
1920;277;1684;752;4004.896;1284.681;2.55164;True;False
Node;AmplifyShaderEditor.VertexColorNode;1;-1400.14,69.77386;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;17;-1429.567,-190.0543;Inherit;False;Constant;_Vector1;Vector 1;1;0;Create;True;0;0;0;False;0;False;1,1,1,1;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;13;-1163.524,-153.9858;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-716.4924,164.8476;Inherit;False;Property;_Float0;Float 0;0;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-507.5081,547.8282;Inherit;False;Constant;_Float1;Float 1;2;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-859.9617,691.6375;Inherit;False;Property;_Smoth;Smoth;1;0;Create;True;0;0;0;False;0;False;0.8769649;0.3;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-909.8891,-127.5393;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.OneMinusNode;20;-1008.542,281.1474;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;23;-374.5921,169.1576;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;25;-334.9021,705.7961;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-653.2143,-124.4872;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;5;2.863695,-2.863693;Float;False;True;-1;2;;0;0;Standard;ITS/Vertex Colors;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;13;1;17;0
WireConnection;13;2;1;0
WireConnection;14;0;13;0
WireConnection;14;1;1;0
WireConnection;20;0;1;4
WireConnection;23;1;22;0
WireConnection;23;2;20;0
WireConnection;25;0;29;0
WireConnection;25;1;24;0
WireConnection;25;2;20;0
WireConnection;15;0;14;0
WireConnection;15;1;1;0
WireConnection;5;0;15;0
WireConnection;5;3;23;0
WireConnection;5;4;25;0
ASEEND*/
//CHKSM=F76242AF49875DDEDB0B051580ABB1A139EDF214