Shader "PostProcess/URPFogOfWar"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _FogOfWar ("fogOfWar", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };


            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            uniform sampler2D _MainTex;
            uniform sampler2D _FogOfWar;
            uniform float4 _TerrainSizePos; //xy = pos, zw = 1/size
            
            float GetFogOfWarFactor(float2 tc)
            {
                const float SUB_FOG_INTENSITY_COEF = 0.5;
                float2 rg = tex2D(_FogOfWar, tc).rg;
                return lerp(rg.r, rg.g, SUB_FOG_INTENSITY_COEF);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 UV = IN.positionHCS.xy / _ScaledScreenParams.xy;

                // Sample the depth from the Camera depth texture.
                #if UNITY_REVERSED_Z
                real depth = SampleSceneDepth(UV);
                #else
                    // Adjust Z to match NDC for OpenGL ([-1, 1])
                    real depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(UV));
                #endif

                // Reconstruct the world space positions.
                float3 worldPos = ComputeWorldSpacePosition(UV, depth, UNITY_MATRIX_I_VP);

                float2 fowTC = worldPos.xz * _TerrainSizePos.zw + _TerrainSizePos.xy;
                float fow = GetFogOfWarFactor(fowTC);
                half4 col = tex2D(_MainTex, UV) * fow;                
                return col;
            }
            ENDHLSL
        }
    }
}