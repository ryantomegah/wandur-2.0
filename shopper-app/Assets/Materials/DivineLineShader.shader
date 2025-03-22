Shader "Wandur/DivineLineShader"
{
    Properties
    {
        _Color ("Main Color", Color) = (0.2, 0.6, 1.0, 0.8)
        _SecondaryColor ("Secondary Color", Color) = (0.1, 0.3, 0.8, 0.5)
        _MainTex ("Texture", 2D) = "white" {}
        _FlowSpeed ("Flow Speed", Range(0, 5)) = 1.0
        _Offset ("Offset", Float) = 0
        _Width ("Width", Range(0, 2)) = 1.0
        _Brightness ("Brightness", Range(0, 3)) = 1.2
        _EdgeSoftness ("Edge Softness", Range(0, 1)) = 0.1
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
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
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _SecondaryColor;
            float _FlowSpeed;
            float _Offset;
            float _Width;
            float _Brightness;
            float _EdgeSoftness;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                // Calculate flow animation
                float flowOffset = fmod(_Time.y * _FlowSpeed + _Offset, 1.0);
                
                // Sample main texture with flow offset
                float2 flowUV = float2(i.uv.x - flowOffset, i.uv.y);
                float4 mainTex = tex2D(_MainTex, flowUV);
                
                // Create a pulse effect
                float pulse = 0.5 + 0.5 * sin(_Time.y * 3.0 + i.uv.x * 10.0);
                
                // Calculate distance from center line
                float distFromCenter = abs(i.uv.y - 0.5) * 2.0;
                float edgeFade = 1.0 - smoothstep(1.0 - _EdgeSoftness, 1.0, distFromCenter);
                
                // Blend between main and secondary color
                float4 blendedColor = lerp(_Color, _SecondaryColor, distFromCenter);
                
                // Apply pulse and width
                float widthFactor = smoothstep(1.0, 1.0 - _Width, distFromCenter);
                float alpha = blendedColor.a * widthFactor * edgeFade;
                
                // Add some brightness variation for shimmer effect
                float shimmer = 0.9 + 0.1 * sin(_Time.y * 5.0 + i.uv.x * 20.0);
                float3 color = blendedColor.rgb * _Brightness * shimmer;
                
                // Add pulse to alpha
                alpha *= (0.8 + 0.2 * pulse);
                
                // Final color
                float4 finalColor = float4(color, alpha);
                
                // Multiply with vertex color for LineRenderer control
                finalColor *= i.color;
                
                return finalColor;
            }
            ENDCG
        }
    }
    
    FallBack "Diffuse"
} 