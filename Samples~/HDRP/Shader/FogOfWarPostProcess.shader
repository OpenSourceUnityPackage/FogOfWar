Shader "Hidden/Shader/FogOfWarPostProcess"
{
    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"

    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        return output;
    }

    // List of properties to control your post process effect
    float _Intensity;
    TEXTURE2D_X(_InputTexture);

    uniform sampler2D _FogOfWar; // Global uniform shouldn't be exposed
    uniform float4 _TerrainSizePos; //xy = pos, zw = 1/size

    half GetFogOfWarFactor(float2 tc, float depth)
    {
        const float SUB_FOG_INTENSITY_COEF = 0.5;

        // Check with depth allow to avoid to render fow in unit square on the background
        const bool isOutside = tc.x < 0 || tc.x > 1 || tc.y < 0 || tc.y > 1 || depth == 0.0f;
        const half2 rg = lerp(tex2D(_FogOfWar, tc).rg, half2(1, 1), isOutside);
        return lerp(rg.r, rg.g, SUB_FOG_INTENSITY_COEF);
    }
    
    float4 FOWPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        uint2 positionSS = input.texcoord * _ScreenSize.xy;
        float3 outColor = LOAD_TEXTURE2D_X(_InputTexture, positionSS).xyz;

        float depth = LoadCameraDepth(input.positionCS.xy);

        // Reconstruct the world space positions.
        float3 worldPos = ComputeWorldSpacePosition(input.texcoord, depth, UNITY_MATRIX_I_VP);

        float2 fowTC = (worldPos.xz + _TerrainSizePos.xy) * _TerrainSizePos.zw;
        half fow = GetFogOfWarFactor(fowTC, depth);
        outColor = outColor * fow;

        return float4(outColor  * _Intensity,  1.0);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "FogOfWarPostProcess"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment FOWPostProcess
                #pragma vertex Vert
            ENDHLSL
        }
    }
    Fallback Off
}
