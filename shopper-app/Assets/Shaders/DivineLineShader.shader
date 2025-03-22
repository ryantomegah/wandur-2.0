Shader "Wandur/DivineLineShader"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (0.5, 0.8, 1.0, 0.8)
        _SecondaryColor ("Secondary Color", Color) = (0.3, 0.6, 1.0, 0.6)
        _FlowSpeed ("Flow Speed", Float) = 1.0
        _FlowOffset ("Flow Offset", Float) = 0.0
        _LineWidth ("Line Width", Float) = 0.15
        _GlowIntensity ("Glow Intensity", Float) = 1.5
        _PulseSpeed ("Pulse Speed", Float) = 1.0
        _PulseScale ("Pulse Scale", Float) = 0.2
        _EdgeSoftness ("Edge Softness", Range(0, 1)) = 0.1
        
        // Enhanced visual properties
        _DetailNoiseTex ("Detail Noise", 2D) = "white" {}
        _NoiseScale ("Noise Scale", Float) = 2.0
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.2
        _ShimmerSpeed ("Shimmer Speed", Float) = 2.0
        _ShimmerStrength ("Shimmer Strength", Range(0, 1)) = 0.3
        _ProximityGlow ("Proximity Glow", Range(0, 5)) = 1.0
        _DistanceFromEnd ("Distance From End", Float) = 0.0
        [Toggle] _ResponsiveGlow ("Responsive Glow", Float) = 1.0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }
        
        LOD 100
        
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
                float3 normal : NORMAL;
                float3 objPos : TEXCOORD3;
                UNITY_FOG_COORDS(4)
            };
            
            float4 _MainColor;
            float4 _SecondaryColor;
            float _FlowSpeed;
            float _FlowOffset;
            float _LineWidth;
            float _GlowIntensity;
            float _PulseSpeed;
            float _PulseScale;
            float _EdgeSoftness;
            float _PathLength;
            sampler2D _DetailNoiseTex;
            float _NoiseScale;
            float _NoiseStrength;
            float _ShimmerSpeed;
            float _ShimmerStrength;
            float _ProximityGlow;
            float _DistanceFromEnd;
            float _ResponsiveGlow;
            
            v2f vert (appdata v)
            {
                v2f o;
                
                // Apply pulse effect
                float pulse = sin(_Time.y * _PulseSpeed) * _PulseScale + 1.0;
                float4 pulseVertex = v.vertex;
                
                // If responsive glow is enabled and we have distance info
                if (_ResponsiveGlow > 0.5 && _DistanceFromEnd > 0) {
                    // Calculate proximity scale
                    float proximityFactor = saturate(1.0 - (_DistanceFromEnd / 5.0)); // 5 meter range
                    float proximityPulse = 1.0 + (proximityFactor * _ProximityGlow * 0.3);
                    
                    // Apply extra pulse near the end
                    pulse *= lerp(1.0, proximityPulse, proximityFactor);
                    
                    // Add slight wiggle when close
                    if (proximityFactor > 0.7) {
                        float wiggle = sin(_Time.y * 15.0 + v.vertex.z * 10.0) * 0.02 * proximityFactor;
                        pulseVertex.x += wiggle * (1.0 - v.uv.y); // Only affect top of line
                    }
                }
                
                // Apply pulse to vertex
                pulseVertex.xy *= pulse;
                o.vertex = UnityObjectToClipPos(pulseVertex);
                
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.objPos = v.vertex.xyz;
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                o.normal = UnityObjectToWorldNormal(v.normal);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                // Calculate flow effect
                float flowOffset = _Time.y * _FlowSpeed + _FlowOffset;
                float2 flowUV = i.uv + float2(flowOffset, 0);
                
                // Create flowing pattern
                float flowPattern = sin(flowUV.x * 6.28318 + flowUV.y * _PathLength);
                flowPattern = (flowPattern + 1) * 0.5; // Normalize to 0-1
                
                // Add noise detail
                float2 noiseUV = float2(
                    i.worldPos.x * _NoiseScale + _Time.y * _ShimmerSpeed * 0.5,
                    i.worldPos.z * _NoiseScale - _Time.y * _ShimmerSpeed * 0.3
                );
                float noise = tex2D(_DetailNoiseTex, noiseUV).r;
                
                // Add second layer of noise with different scale
                float2 noise2UV = float2(
                    i.worldPos.z * _NoiseScale * 1.4 - _Time.y * _ShimmerSpeed * 0.6,
                    i.worldPos.x * _NoiseScale * 1.4 + _Time.y * _ShimmerSpeed * 0.4
                );
                float noise2 = tex2D(_DetailNoiseTex, noise2UV).r;
                
                // Combine noise layers
                float finalNoise = lerp(noise, noise2, 0.5) * 2.0 - 1.0; // -1 to 1 range
                
                // Add noise to flow pattern
                flowPattern = lerp(flowPattern, saturate(flowPattern + finalNoise * _NoiseStrength), _NoiseStrength);
                
                // Calculate pulse effect with time variation
                float pulse = sin(_Time.y * _PulseSpeed) * _PulseScale + 1;
                
                // Add shimmer effect that moves along the path
                float shimmerWave = sin(i.objPos.z * 5.0 + _Time.y * _ShimmerSpeed * 2.0);
                float shimmer = pow(saturate(shimmerWave * 0.5 + 0.5), 2.0) * _ShimmerStrength;
                
                // Calculate edge glow that's stronger at the sides
                float edgeGlow = 1 - saturate(abs(i.uv.x - 0.5) * 2);
                edgeGlow = pow(edgeGlow, 1 - _EdgeSoftness);
                
                // Calculate fresnel effect for view-dependent glow
                float fresnel = pow(1 - saturate(dot(i.normal, i.viewDir)), 2);
                
                // Add proximity glow if we have distance info and responsive glow is enabled
                float proximityFactor = 0;
                if (_ResponsiveGlow > 0.5 && _DistanceFromEnd > 0) {
                    proximityFactor = saturate(1.0 - (_DistanceFromEnd / 5.0)); // 5 meter range
                    // Enhance glow near destination
                    fresnel *= lerp(1.0, 2.0, proximityFactor);
                }
                
                // Combine effects
                float4 col = lerp(_SecondaryColor, _MainColor, flowPattern);
                
                // Add shimmer
                col.rgb += shimmer * _MainColor.rgb;
                
                // Apply glow
                col.rgb *= _GlowIntensity * pulse;
                
                // Add fresnel glow on edges
                col.rgb += fresnel * _MainColor.rgb * _GlowIntensity * 0.5;
                
                // Apply edge softness to alpha
                col.a *= edgeGlow;
                
                // Increase intensity near destination if we have distance info
                if (_ResponsiveGlow > 0.5 && proximityFactor > 0) {
                    // Add extra glow pulses near the destination
                    float proximityPulse = sin(_Time.y * (_PulseSpeed * 2.0 + proximityFactor * 5.0)) * 0.5 + 0.5;
                    col.rgb += proximityFactor * proximityPulse * _MainColor.rgb * _ProximityGlow;
                    
                    // Make it slightly more opaque near destination
                    col.a = lerp(col.a, min(col.a * 1.3, 1.0), proximityFactor);
                }
                
                // Apply distance fade
                float distanceFade = 1 - saturate(length(i.worldPos - _WorldSpaceCameraPos) * 0.1);
                col.a *= distanceFade;
                
                // Apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                
                return col;
            }
            ENDCG
        }
    }
    
    Fallback "Transparent/VertexLit"
} 