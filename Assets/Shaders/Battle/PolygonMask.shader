Shader "Custom/PolygonMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VerticesTex ("Vertices Texture", 2D) = "white" {}
        _VertexCount ("Vertex Count", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _VerticesTex;  // 用于获取顶点纹理
            int _VertexCount;  // 顶点数量

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float2 worldPos : TEXCOORD1;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xy; // 计算世界坐标
                return o;
            }

            bool PointInPolygon(float2 p)
            {
                int count = 0;
                for (int i = 0; i < _VertexCount; i++)
                {
                    // 从纹理中读取顶点坐标
                    float2 v1 = tex2D(_VerticesTex, float2(i / float(_VertexCount), 0)).xy;
                    float2 v2 = tex2D(_VerticesTex, float2((i + 1) / float(_VertexCount), 0)).xy;

                    // 判断是否穿过射线
                    if ((v1.y > p.y) != (v2.y > p.y))
                    {
                        float intersectX = (v2.x - v1.x) * (p.y - v1.y) / (v2.y - v1.y) + v1.x;
                        if (p.x < intersectX)
                        {
                            count++;
                        }
                    }
                }
                return (count % 2) == 1;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 直接使用 worldPos 判断是否在多边形内
                float2 p = i.worldPos;
                if (!PointInPolygon(p)) discard;
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
