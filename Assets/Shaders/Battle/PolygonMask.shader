Shader "Custom/PolygonMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            float _Mode;
            fixed4 _Color;

            // 新的结构体用于存储多边形信息
            struct PolygonInfo
            {
                float vertexCount; // 多边形顶点数
                int startIndex;    // 对应顶点数组中的起始索引
            };

            // 结构化缓冲区传递多边形数据和顶点数据
            StructuredBuffer<PolygonInfo> _PolygonInfos;
            StructuredBuffer<float2> _PolygonVertices;
            int _PolygonCount;

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
                bool isInside = false;
                // 遍历所有多边形
                for (int i = 0; i < _PolygonCount; i++)
                {
                    PolygonInfo info = _PolygonInfos[i];
                    int vertexCount = (int)info.vertexCount;
                    int hitCount = 0;
                    // 逐边检测射线与多边形各边的交点
                    for (int j = 0; j < vertexCount; j++)
                    {
                        int nextIndex = (j + 1) % vertexCount;
                        float2 v1 = _PolygonVertices[info.startIndex + j];
                        float2 v2 = _PolygonVertices[info.startIndex + nextIndex];
                        if ((v1.y > p.y) != (v2.y > p.y))
                        {
                            float intersectX = (v2.x - v1.x) * (p.y - v1.y) / (v2.y - v1.y) + v1.x;
                            if (p.x < intersectX)
                            {
                                hitCount++;
                            }
                        }
                    }
                    // 累计判断是否在任一多边形内
                    isInside = isInside || ((hitCount & 1) == 1);
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
