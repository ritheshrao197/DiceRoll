Shader "TRAIBAG/IndiDolls_Hair_urp_NoAlpha"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo", 2D) = "white" {}

        _Metallic ("Metallic", Range(0,1)) = 0
        _Glossiness ("Smoothness", Range(0,1)) = 0.5

        _EmissionMap ("Emission Map (Optional)", 2D) = "black" {}
        _EmissionColor ("Emission Color", Color) = (1,1,1,0)

        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimIntensity ("Rim Intensity", Range(0,4)) = 1.5
        _RimPower ("Rim Power", Range(0.5,8)) = 2
        _RimAmbientInfluence ("Rim Ambient Influence", Range(0,1)) = 1
        _RimUseViewSpace ("Rim UseViewSpace (0 or 1)", Range(0,1)) = 1

        _RingUseLocal ("Ring Use Local (0 UV, 1 Local)", Range(0,1)) = 0
        _RingColor ("Ring Color", Color) = (1,1,1,1)
        _RingIntensity ("Ring Intensity", Range(0,3)) = 1
        _RingCenter ("Ring Center", Range(0,1)) = 0.5
        _RingWidth ("Ring Width", Range(0.001,1)) = 0.15
        _RingSoftness ("Ring Softness", Range(0.001,1)) = 0.1

        _RingCenterViewInfluence ("Ring Center View Influence", Range(0,1)) = 0.7
        _RingCenterViewMaxShift ("Ring Center View Max Shift", Range(0,0.3)) = 0.12

        _LocalMinY ("Local Min Y", Float) = 0
        _LocalMaxY ("Local Max Y", Float) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 300
        Blend Off
        ZWrite On
        Cull Back

        ///////////////////////////////////////////////////
        // ShadowCaster Pass
        ///////////////////////////////////////////////////
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex vertShadow
            #pragma fragment fragShadow
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct AttributesShadow { float4 positionOS : POSITION; };
            struct VaryingsShadow { float4 positionCS : SV_POSITION; };

            VaryingsShadow vertShadow(AttributesShadow v)
            {
                VaryingsShadow o = (VaryingsShadow)0;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                return o;
            }

            float4 fragShadow(VaryingsShadow i) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        ///////////////////////////////////////////////////
        // ForwardLit Pass
        ///////////////////////////////////////////////////
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);
            TEXTURE2D(_EmissionMap);    SAMPLER(sampler_EmissionMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Metallic;
                float _Glossiness;
                float4 _EmissionColor;

                float4 _RimColor;
                float _RimIntensity;
                float _RimPower;
                float _RimAmbientInfluence;
                float _RimUseViewSpace;

                float4 _RingColor;
                float _RingIntensity;
                float _RingCenter;
                float _RingWidth;
                float _RingSoftness;
                float _RingUseLocal;
                float _RingCenterViewInfluence;
                float _RingCenterViewMaxShift;
                float _LocalMinY;
                float _LocalMaxY;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float2 uvMain     : TEXCOORD2;
                float fogFactor   : TEXCOORD3;
            };

            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.uvMain = v.uv;
                o.fogFactor = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            float4 frag(Varyings i, bool isFrontFace : SV_IsFrontFace) : SV_Target
            {
                float3 N = normalize(i.normalWS);
                N = isFrontFace ? N : -N;

                float3 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uvMain).rgb * _Color.rgb;

                float3 V = normalize(GetCameraPositionWS() - i.positionWS);

                Light mainLight = GetMainLight(float4(i.positionWS,1));
                float NdotL = saturate(dot(N, mainLight.direction));
                float3 direct = mainLight.color * NdotL;
                float3 ambient = SampleSH(N);

                float3 H = normalize(mainLight.direction + V);
                float nh = saturate(dot(N,H));

                float specPower = lerp(1.0, 128.0, _Glossiness);
                float specIntensity = pow(nh, specPower) * _Glossiness;
                float3 specCol = mainLight.color * specIntensity * NdotL;

                float edge = (_RimUseViewSpace > 0.5)
                    ? 1.0 - saturate(abs(mul((float3x3)UNITY_MATRIX_V, N).z))
                    : 1.0 - saturate(dot(N, V));
                float rim = pow(saturate(edge), _RimPower);
                float ambientLum = dot(ambient, float3(0.299,0.587,0.114));
                float rimAmbient = lerp(1.0, ambientLum, _RimAmbientInfluence);
                float3 rimCol = _RimColor.rgb * (_RimIntensity * rim * rimAmbient);

                float3 localPos = mul(unity_WorldToObject, float4(i.positionWS,1)).xyz;
                float y01 = saturate((localPos.y - _LocalMinY)/max(_LocalMaxY-_LocalMinY, 1e-5));
                float ringCoord = lerp(i.uvMain.y, y01, step(0.5,_RingUseLocal));

                float3 objUpWS = normalize(mul((float3x3)unity_ObjectToWorld, float3(0,1,0)));
                float3 objOriginWS = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
                float3 camToObj = normalize(objOriginWS - _WorldSpaceCameraPos);
                float centerOffset = -dot(objUpWS, camToObj) * _RingCenterViewMaxShift * _RingCenterViewInfluence;
                float ringCenterEff = saturate(_RingCenter + centerOffset);

                float a = smoothstep(ringCenterEff - _RingWidth, ringCenterEff - _RingSoftness, ringCoord);
                float b = 1.0 - smoothstep(ringCenterEff + _RingSoftness, ringCenterEff + _RingWidth, ringCoord);
                float ringMask = saturate(a*b);

                float3 emissionTex = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, i.uvMain).rgb;
                float3 emissionCol = (_RingColor.rgb*_RingIntensity + _EmissionColor.rgb + emissionTex) * ringMask;

                float3 diffuse = albedo * (direct + ambient);
                float3 finalColor = diffuse + specCol + rimCol + emissionCol;

                finalColor = MixFog(finalColor, i.fogFactor);

                return float4(finalColor, 1.0); // 알파 1.0으로 고정
            }
            ENDHLSL
        }
    }

    Fallback Off
    CustomEditor "TRAIBAG.HairShaderGUI"
}
