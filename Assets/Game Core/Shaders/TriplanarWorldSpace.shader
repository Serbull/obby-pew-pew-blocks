
Shader "Custom/TriplanarWorldSpace"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Scale ("Texture Scale", Float) = 1.0
        _BlendSharpness ("Blend Sharpness", Range(1, 10)) = 4
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            fixed4 _Color;
            float _Scale;
            float _BlendSharpness;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };

            float3 WorldNormalReconstruct(float3 normal) {
                return normalize(normal);
            }

            v2f vert (appdata v)
            {
                v2f o;
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = worldPos.xyz;
                o.worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float3 blendWeights(float3 normal)
            {
                float3 absN = abs(normal);
                float power = _BlendSharpness;
                float3 weights = pow(absN, power);
                return weights / (weights.x + weights.y + weights.z);
            }

            float4 SampleTriplanar(sampler2D tex, float3 worldPos, float3 blend, float scale)
            {
                float2 xz = worldPos.zy * scale;
                float2 yz = worldPos.xz * scale;
                float2 xy = worldPos.xy * scale;

                float4 x = tex2D(tex, xz);
                float4 y = tex2D(tex, yz);
                float4 z = tex2D(tex, xy);

                return x * blend.x + y * blend.y + z * blend.z;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal = WorldNormalReconstruct(i.worldNormal);
                float3 blend = blendWeights(normal);
                float4 col = SampleTriplanar(_MainTex, i.worldPos, blend, _Scale);
                return col * _Color;
            }
            ENDCG
        }
    }
}
