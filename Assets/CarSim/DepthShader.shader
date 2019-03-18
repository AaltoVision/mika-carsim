Shader "Unlit/DepthShader" {
    SubShader {
        Tags { "RenderType"="Opaque" }
        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            inline float Linear01FromEyeToLinear01FromNear(float depth01)
            {
                float near = _ProjectionParams.y;
                float far = _ProjectionParams.z;
                return (depth01 - near/far) * (1 + near/far);
            }


            struct v2f {
                float4 pos : SV_POSITION;
                float2 depth : TEXCOORD0;
            };

            v2f vert (appdata_base v) {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
//                UNITY_TRANSFER_DEPTH(o.depth);
                o.depth.x = COMPUTE_DEPTH_01;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target {
                float d01 = Linear01FromEyeToLinear01FromNear(i.depth.x);
                d01 = pow(d01, 0.25);
                return float4(d01, d01, d01, 1.0);
            }
            ENDCG
        }
    }
}
