Shader "Unlit/ParticleRender"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "particledata.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                // float4 vertex : SV_POSITION;
            };

            static const float3 g_positions[4] =
            {
                float3(-1, 1, 0),
                float3( 1, 1, 0),
                float3(-1,-1, 0),
                float3( 1,-1, 0),
            };

            static const float2 g_texcoords[4] =
            {
                float2(0, 0),
                float2(1, 0),
                float2(0, 1),
                float2(1, 1),
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float     _ParticleSize;
            float4x4  _InvViewMatrix;

            StructuredBuffer<ParticleData> _ParticleBuffer;

            v2f vert (appdata v, uint id : SV_VertexID)
            {
                v2f o = (v2f)0;
                float3 position = g_positions[0] * 1.0; //* _particle size 
                o.position = UnityObjectToClipPos(float4(position, 1.0));
                o.position = float4(_ParticleBuffer[id].position, 1);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
