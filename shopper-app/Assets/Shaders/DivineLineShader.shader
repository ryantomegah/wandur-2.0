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
                UNITY_FOG_COORDS(3)
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
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
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
                
                // Calculate pulse effect
                float pulse = sin(_Time.y * _PulseSpeed) * _PulseScale + 1;
                
                // Calculate edge glow
                float edgeGlow = 1 - saturate(abs(i.uv.x - 0.5) * 2);
                edgeGlow = pow(edgeGlow, 1 - _EdgeSoftness);
                
                // Calculate fresnel effect for view-dependent glow
                float fresnel = pow(1 - saturate(dot(i.normal, i.viewDir)), 2);
                
                // Combine effects
                float4 col = lerp(_SecondaryColor, _MainColor, flowPattern);
                col.rgb *= _GlowIntensity * pulse;
                col.rgb += fresnel * _MainColor.rgb * _GlowIntensity * 0.5;
                col.a *= edgeGlow;
                
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