// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

Shader "Hidden/Crest/Simulation/Update Shadow"
{
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/EditorShaderVariables.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/HDShadow.hlsl"

            #include "../OceanConstants.hlsl"
            #include "../OceanGlobals.hlsl"

            CBUFFER_START(CrestPerMaterial)
            // Settings._jitterDiameterSoft, Settings._jitterDiameterHard, Settings._currentFrameWeightSoft, Settings._currentFrameWeightHard
            float4 _JitterDiameters_CurrentFrameWeights;
            float _SimDeltaTime;

            float3 _CenterPos;
            float3 _Scale;
            float _LD_SliceIndex_Source;
            float4x4 _MainCameraProjectionMatrix;
            CBUFFER_END

            #include "../OceanInputsDriven.hlsl"
            #include "../OceanHelpersNew.hlsl"
            // noise functions used for jitter
            #include "../GPUNoise/GPUNoise.hlsl"

            struct Attributes
            {
                uint vertexID : SV_VertexID;
                real3 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                real4 _MainCameraCoords : TEXCOORD0;
                float3 _WorldPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                // unity_MatrixVP comes from EditorShaderVariables.hlsl. Not the same as UNITY_MATRIX_VP.
                output.positionCS = mul(unity_MatrixVP, float4(input.positionOS, 1.0));

                // World position from [0,1] quad
                output._WorldPos.xyz = float3(input.positionOS.x - 0.5, 0.0, input.positionOS.y - 0.5) * _Scale * 4.0 + _CenterPos;
                output._WorldPos.y = _OceanCenterPosWorld.y;
                output._MainCameraCoords = mul(_MainCameraProjectionMatrix, float4(output._WorldPos.xyz, 1.0));

                return output;
            }

            real2 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half2 shadow = 0.0;
                const half r_max = 0.5 - _LD_Params_Source[_LD_SliceIndex_Source].w;

                float3 positionWS = input._WorldPos.xyz;

                float depth;
                {
                    float width; float height;
                    _LD_TexArray_Shadow_Source.GetDimensions(width, height, depth);
                }

                // Shadow from last frame - manually implement black border.
                float3 uv_source = WorldToUV
                (
                    positionWS.xz,
                    _LD_Pos_Scale_Source[_LD_SliceIndex_Source],
                    _LD_Params_Source[_LD_SliceIndex_Source],
                    _LD_SliceIndex_Source
                );
                half2 r = abs(uv_source.xy - 0.5);
                if (max(r.x, r.y) <= r_max)
                {
                    SampleShadow(_LD_TexArray_Shadow_Source, uv_source, 1.0, shadow);
                }
                else if (_LD_SliceIndex_Source + 1.0 < depth)
                {
                    float3 uv_source_nextlod = WorldToUV
                    (
                        positionWS.xz,
                        _LD_Pos_Scale_Source[_LD_SliceIndex_Source + 1.0],
                        _LD_Params_Source[_LD_SliceIndex_Source + 1.0],
                        _LD_SliceIndex_Source + 1.0
                    );
                    half2 r2 = abs(uv_source_nextlod.xy - 0.5);
                    if (max(r2.x, r2.y) <= r_max)
                    {
                        SampleShadow(_LD_TexArray_Shadow_Source, uv_source_nextlod, 1.0, shadow);
                    }
                }

                #if (SHADEROPTIONS_CAMERA_RELATIVE_RENDERING != 0)
                    positionWS -= _WorldSpaceCameraPos.xyz;
                #endif

                // Check if the current sample is visible in the main camera (and therefore shadow map can be sampled). This
                // is required as the shadow buffer is world aligned and surrounds viewer.
                float3 projected = input._MainCameraCoords.xyz / input._MainCameraCoords.w;
                if (projected.z < 1.0 && projected.z > 0.0 && abs(projected.x) < 1.0 && abs(projected.y) < 1.0)
                {
                    float3 positionWS_0 = positionWS, positionWS_1 = positionWS;
                    if (_JitterDiameters_CurrentFrameWeights[0] > 0.0)
                    {
                        positionWS_0.xz += _JitterDiameters_CurrentFrameWeights[0] * (hash33(uint3(abs(positionWS.xz * 10.0), _Time.y * 120.0)) - 0.5).xy;
                        positionWS_1.xz += _JitterDiameters_CurrentFrameWeights[1] * (hash33(uint3(abs(positionWS.xz * 10.0), _Time.y * 120.0)) - 0.5).xy;
                    }

                    HDShadowContext shadowContext = InitShadowContext();

                    // Get directional light data. By definition we only have one directional light casting shadow.
                    DirectionalLightData light = _DirectionalLightDatas[_DirectionalShadowIndex];
                    float3 L = -light.forward;

                    half2 shadowThisFrame;

                    // Zeros are for screen space position and world space normal. Position is for filtering and normal
                    // is for normal bias. They did not appear to have an impact. But we might want to revisit.
                    shadowThisFrame[0] = GetDirectionalShadowAttenuation(shadowContext, 0, positionWS_0, 0, _DirectionalShadowIndex, L);
                    shadowThisFrame[1] = GetDirectionalShadowAttenuation(shadowContext, 0, positionWS_1, 0, _DirectionalShadowIndex, L);

                    half shadowStrength = light.shadowDimmer;
                    shadowThisFrame[0] = LerpWhiteTo(shadowThisFrame[0], shadowStrength);
                    shadowThisFrame[1] = LerpWhiteTo(shadowThisFrame[1], shadowStrength);

                    // TODO: I don't think this is necessary. Nor could I find an equivalent. If we need the shadow
                    // coordinates, EvalShadow_WorldToShadow from HDShadowAlgorithms.hlsl might work.
                    // shadowThisFrame[0] = BEYOND_SHADOW_FAR(coords_0) ? 1.0h : shadowThisFrame[0];
                    // shadowThisFrame[1] = BEYOND_SHADOW_FAR(coords_1) ? 1.0h : shadowThisFrame[1];
                    shadowThisFrame = (half2)1.0 - saturate(shadowThisFrame);

                    shadow = lerp(shadow, shadowThisFrame, _JitterDiameters_CurrentFrameWeights.zw * _SimDeltaTime * 60.0);
                }

                return shadow;
            }
            ENDHLSL
        }
    }
    Fallback Off
}
