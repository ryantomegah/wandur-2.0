Shader "Wandur/NavigationMarker"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (1,1,1,1)
        _RimColor ("Rim Color", Color) = (0,1,1,1)
        _RimPower ("Rim Power", Range(0.5,8.0)) = 3.0
        _GlowIntensity ("Glow Intensity", Range(0,2)) = 1.0
        _PulseSpeed ("Pulse Speed", Range(0,5)) = 1.0
        _AlphaClip ("Alpha Clip", Range(0,1)) = 0.3
        _MainTex ("Main Texture", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 viewDir : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainColor;
            float4 _RimColor;
            float _RimPower;
            float _GlowIntensity;
            float _PulseSpeed;
            float _AlphaClip;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                o.normalDir = worldNormal;
                o.viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPos);
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Sample main texture
                fixed4 col = tex2D(_MainTex, i.uv) * _MainColor;
                
                // Calculate rim effect
                float rim = 1.0 - saturate(dot(i.viewDir, i.normalDir));
                rim = pow(rim, _RimPower);
                
                // Pulse effect
                float pulse = (sin(_Time.y * _PulseSpeed) + 1.0) * 0.5;
                
                // Combine effects
                float3 finalColor = col.rgb;
                finalColor += _RimColor.rgb * rim * _GlowIntensity;
                finalColor += _MainColor.rgb * pulse * _GlowIntensity * 0.3;
                
                // Calculate alpha
                float alpha = col.a;
                alpha = max(alpha, rim * _RimColor.a);
                alpha = max(alpha, pulse * 0.3);
                alpha *= _MainColor.a;
                
                // Apply alpha clip
                clip(alpha - _AlphaClip);
                
                return float4(finalColor, alpha);
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit"
} 