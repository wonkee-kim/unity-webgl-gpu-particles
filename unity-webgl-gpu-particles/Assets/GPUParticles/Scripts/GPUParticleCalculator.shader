Shader "GPUParticle/GPUParticleCalculator"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            float _DeltaTime;
            float3 _TargetPosition;

            float _RandomValues[256];

            float4 _ParticleSpeedArgs; // xy: range, z: limit, w: bounceness

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 position = tex2D(_MainTex, float2(i.uv.x, 0.25));
                float4 velocity = tex2D(_MainTex, float2(i.uv.x, 0.75));

                float4 texelSize = _MainTex_TexelSize; // (1/width, 1/height, width, height)
                int index = (int)(i.uv.x * texelSize.z);

                float speed = _RandomValues[index] * (_ParticleSpeedArgs.y - _ParticleSpeedArgs.x) + _ParticleSpeedArgs.x;
                float maxSpeed = _ParticleSpeedArgs.z;

                // float3 vel = velocity.xyz * 0.99;
                float3 vel = velocity.xyz;
                float3 direction = (_TargetPosition - position.xyz);
                float distance = length(direction);
                vel = vel + direction * speed * _DeltaTime;
                // vel.y -= 9.8 * distance * 0.1 * _DeltaTime;
                if(length(vel) > maxSpeed)
                {
                    vel = normalize(vel) * maxSpeed;
                }

                float brightness = velocity.w;
                brightness -= 5 * _DeltaTime; // decrease brightness
                brightness = max(brightness, 0.2);

                float3 pos = position.xyz + velocity.xyz * _DeltaTime;
                if(pos.y - position.w * 0.5 < 0)
                {
                    pos.y = position.w * 0.5;
                    vel.y = _ParticleSpeedArgs.w * speed * saturate(1.0 - distance / 30) * 2;
                    brightness = 2;
                }

                // position
                if(i.uv.y < 0.5)
                {
                    return float4(pos, position.w); // position.w: size
                }
                // velocity
                else
                {
                    return float4(vel, brightness); // veclotiy.w: brightness
                }
            }
            ENDCG
        }
    }
}
