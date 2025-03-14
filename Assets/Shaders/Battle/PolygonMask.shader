Shader "Custom/PolygonMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VerticesTex ("Vertices Texture", 2D) = "white" {}
        [KeywordEnum(None, Visible Inside Mask, Visible Outside Mask)] _Mode ("Mask Interaction", Float) = 0
        _Color ("Tint Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _VerticesTex;
            float _Mode;
            fixed4 _Color;


            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR; // 添加颜色通道
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float2 worldPos : TEXCOORD1;
                fixed4 color : COLOR; // 添加颜色传递
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xy;
                o.color = v.color; // 传递颜色数据
                return o;
            }


            bool PointInPolygon(float2 p)
            {
                float polygonCount = tex2D(_VerticesTex, float2(0, 0)).g;
                int count = 0;
                bool isInside = false;
                for (int i = 0; i < (int)polygonCount; i++)
                {
                    float vertexCount = tex2D(_VerticesTex, float2(0, i / polygonCount)).r;

                    for (int j = 0; j < (int)vertexCount; j++)
                    {
                        float2 v1 = tex2D(_VerticesTex, float2((j + 1) / (vertexCount + 1), i / polygonCount)).xy;
                        int nextIndex = (j + 1) % (int)vertexCount;
                        float2 v2 = tex2D(_VerticesTex,
                  float2((nextIndex + 1) / (vertexCount + 1), i / polygonCount)).xy;

                        if (v1.y > p.y != v2.y > p.y)
                        {
                            float intersectX = (v2.x - v1.x) * (p.y - v1.y) / (v2.y - v1.y) + v1.x;
                            if (p.x < intersectX)
                            {
                                count++;
                            }
                        }
                    }

                    if (count % 2 == 1)
                    {
                        isInside = true;
                    }
                }
                return isInside;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 p = i.worldPos;
                bool inside = PointInPolygon(p);

                if ((inside && _Mode == 2) || (!inside && _Mode == 1))
                    discard;

                fixed4 col = tex2D(_MainTex, i.uv);
                col *= i.color * _Color; // 添加颜色乘算
                return col;
            }
            ENDCG
        }
    }
}