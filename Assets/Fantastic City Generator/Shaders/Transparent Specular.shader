Shader "Custom/Trasparent Specular"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BumpMap("Normalmap", 2D) = "bump" {}
        _Metallic("Metallic", Range(0,1)) = 0.0
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _ReflectColor("Reflection Color", Color) = (1, 1, 1, 1)
        _Cube("Reflection Cubemap", CUBE) = "" {}
        //_MaskTex("Mask Texture", 2D) = "white" {}
        _Cutoff("Alpha cutoff", Range(0,1)) = 1
        
    }

    SubShader
    {
        Tags {"Queue" = "AlphaTest" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows keepalpha
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;
        samplerCUBE _Cube;
        
        struct Input
        {
            float2 uv_MainTex;
            float3 worldRefl;
            INTERNAL_DATA
        };

        half _Glossiness;
        half _Metallic;
        half _ValueAlpha;
        fixed4 _Color;
        fixed4 _ReflectColor;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {

            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);

            o.Albedo = (c.rgb * _Color) + (texCUBE(_Cube, WorldReflectionVector(IN, o.Normal)).rgb * _ReflectColor * (1 - c.a));
            o.Normal = UnpackNormal(tex2D(_BumpMap , IN.uv_MainTex) );
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            //o.Emission = texCUBE(_Cube, WorldReflectionVector(IN, o.Normal)).rgb * _ReflectColor *  (1 - c.a);
            
            o.Alpha = c.a;
            
        }
        ENDCG
    }
    Fallback "Legacy Shaders/Transparent/Cutout/VertexLit"
}
