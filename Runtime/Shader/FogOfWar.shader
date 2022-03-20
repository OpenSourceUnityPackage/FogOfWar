Shader "Unlit/FogOfWar"
{
    Properties
    {
        _OpacFogOfWarRT ("Opac fog texture", 2D) = "white" {}
        _TransparentFogOfWarRT ("Transparent fog texture", 2D) = "white" {}
        _FogOfWarColor ("Fog of war color", COLOR) = (.25, .5, .5, 1)
        _FogOfWarTransparencyOpacity ("Fog of war transparency opacity", RANGE(0.0, 1.0)) = 0.5
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "FogOfWar.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return ProcessFogOfWar(i.uv);
            }
            ENDCG
        }
    }
}
