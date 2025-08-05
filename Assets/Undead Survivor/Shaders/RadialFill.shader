Shader "Custom/RadialFill"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _FillAmount ("Fill Amount", Range(0, 1)) = 0
        _StartAngle ("Start Angle", Float) = 0
        _Clockwise ("Clockwise", Float) = 1
        _CenterPoint ("Center Point", Vector) = (0.5, 0, 0, 0)
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _FillAmount;
            float _StartAngle;
            float _Clockwise;
            float4 _CenterPoint;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 설정 가능한 중심점 사용
                float2 centerPoint = _CenterPoint.xy;
                float2 centeredUV = i.uv - centerPoint;
                
                // 각도 계산 (atan2 사용) - 반원에 맞게 조정
                float angle = atan2(centeredUV.y, centeredUV.x);
                angle = degrees(angle);
                
                // 반원 범위로 각도 정규화 (0도 = 오른쪽, 90도 = 위, 180도 = 왼쪽)
                if (angle < 0) angle += 360;
                if (angle > 180) angle = 180; // 반원 범위 제한
                
                // 시계방향/반시계방향 처리
                float fillAngle;
                if (_Clockwise > 0.5)
                {
                    // 시계방향: 0도에서 시작해서 180도로
                    fillAngle = angle - _StartAngle;
                    if (fillAngle < 0) fillAngle += 180;
                }
                else
                {
                    // 반시계방향: 180도에서 시작해서 0도로
                    fillAngle = _StartAngle - angle;
                    if (fillAngle < 0) fillAngle += 180;
                }
                
                // 채우기 진행도 계산 (반원이므로 180도 기준)
                float maxFillAngle = 180 * _FillAmount;
                float alpha = step(fillAngle, maxFillAngle);
                
                // 부드러운 경계를 위한 smoothstep (선택사항)
                // alpha = smoothstep(maxFillAngle + 2, maxFillAngle - 2, fillAngle);
                
                // 텍스처 샘플링
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                col.a *= alpha;
                
                return col;
            }
            ENDCG
        }
    }
}