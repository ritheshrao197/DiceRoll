Shader "TRAIBAG/IndiDolls_Sky"
{
    Properties
    {
        _SkyPreset ("Sky Preset", Float) = 0
        _SkyGradientTop ("Top Color", Color) = (0.15,0.56,0.72,1)
        _SkyGradientBottom ("Bottom Color", Color) = (0.78,0.84,0.88,1)
        _SkyGradientExponent ("Exponent", Float) = 2.5
        _HorizonLineColor ("Horizon Color", Color) = (0.92,0.88,0.78,1)
        _HorizonLineExponent ("Horizon Exponent", Float) = 4
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            fixed4 _SkyGradientTop;
            fixed4 _SkyGradientBottom;
            float _SkyGradientExponent;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float t = pow(i.pos.y / _ScreenParams.y, _SkyGradientExponent);
                fixed4 col = lerp(_SkyGradientBottom, _SkyGradientTop, t);
                return col;
            }
            ENDCG
        }
    }

    CustomEditor "TRAIBAG.IndiDolls_SkyGUI"
}
