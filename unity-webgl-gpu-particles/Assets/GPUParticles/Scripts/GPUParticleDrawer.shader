Shader "GPUParticle/GPUParticleDrawer"
{
    Properties
    {
        _BaseColor ("Base color", Color) = (1,1,1,1)
        _BaseMap ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varying
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float fogCoord : TEXCOORD1;
                float3 ambient : TEXCOORD2;
                float3 diffuse : TEXCOORD3;
                float3 emission : TEXCOORD4;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            TEXTURE2D(_RTData);
            SAMPLER(sampler_RTData);

            CBUFFER_START(UnityPerMaterial)
                float4 _RTData_ST;
                float4 _RTData_TexelSize;
                float4 _BaseMap_ST;
                half4 _BaseColor;
                float _RandomValues[256];
                half2 _ParticleSize;
                half _ParticleBrightness;
            CBUFFER_END

            Varying vert (Attributes IN, uint instanceID : SV_InstanceID)
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                Varying OUT = (Varying)0;
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float halfPixelSize = 0.5f * _RTData_TexelSize.x;
                float2 dataUVx = float2(instanceID * _RTData_TexelSize.x + halfPixelSize, 0.25);
                float2 dataUVy = float2(instanceID * _RTData_TexelSize.x + halfPixelSize, 0.75);

                float4 data1 = SAMPLE_TEXTURE2D_LOD(_RTData, sampler_RTData, dataUVx, 0);
                float4 data2 = SAMPLE_TEXTURE2D_LOD(_RTData, sampler_RTData, dataUVy, 0);

                float particleSize = data1.w * (_ParticleSize.y - _ParticleSize.x) + _ParticleSize.x;

                float3 positionOS = IN.positionOS.xyz * particleSize;
                float3 positionWS = data1.xyz + positionOS;
                float3 normalWS = IN.normalOS;

                float NoL = saturate(dot(normalWS, _MainLightPosition.xyz));
                float3 ambient = SampleSH(normalWS);
                float3 diffuse = NoL * _MainLightColor.rgb;

                float3 particleColor = abs(data2.xyz); // Use velocity as color

                // OUT.positionCS = TransformObjectToHClip(position);
                OUT.positionCS = mul(UNITY_MATRIX_VP, float4(positionWS, 1.0));
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.ambient = ambient;
                OUT.diffuse = diffuse;
                OUT.emission = particleColor * data2.w * _ParticleBrightness;
                OUT.fogCoord = ComputeFogFactor(OUT.positionCS.z);
                return OUT;
            }

            half4 frag (Varying IN) : SV_Target
            {
                half shadow = MainLightRealtimeShadow(IN.positionCS);
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                float3 lighting = IN.diffuse * shadow + IN.ambient;
                half4 color = half4(albedo.rgb * lighting + IN.emission, albedo.a);
                color.rgb = MixFog(color.rgb, IN.fogCoord);
                return color;
            }
            ENDHLSL
        }
    }
}
