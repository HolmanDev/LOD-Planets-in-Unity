Shader "Planet/PlanetFar"
{
    Properties
    {
        _ShrinkFactor("Shrink Factor", float) = 100000
    }
    SubShader
    {
        Tags {"LightMode"="ForwardBase" "Planet"="Unlit Terrain" } 
        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            // compile shader into multiple variants, with and without shadows
            // (we don't care about any lightmaps yet, so skip these variants)
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            // shadow helper functions and macros
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex   : POSITION;  // The vertex position in model space.
                float3 normal   : NORMAL;    // The vertex normal in model space.
                float4 texcoord : TEXCOORD0; // The first UV coordinate.
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                SHADOW_COORDS(1) // put shadows data into TEXCOORD1
                fixed3 diff : COLOR3;
                fixed3 ambient : COLOR1;
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
                float dist : FLOAT;
            };

            float _ShrinkFactor;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = mul(UNITY_MATRIX_VP, float4(mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xyz / _ShrinkFactor, 1.0f));
                o.uv = v.texcoord;
                o.dist = length(WorldSpaceViewDir(v.vertex));
                o.color = v.color;
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0.rgb;
                o.ambient = ShadeSH9(half4(worldNormal,1));
                // compute shadows data
                TRANSFER_SHADOW(o) 
                return o;
            }

            sampler2D _MainTex;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 textureColor = tex2D(_MainTex, i.uv.xy);
                fixed4 col = textureColor;
                
                // compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
                fixed shadow = SHADOW_ATTENUATION(i);
                // darken light's illumination with shadow, keep ambient intact
                fixed3 lighting = i.diff * shadow + i.ambient;
                col.rgb *= lighting;
                return col;
            }
            ENDCG
        }

        // shadow casting support
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }

    // Still a work in progress.
    SubShader {
        Tags { "RenderType"="Opaque" "Planet"="Ocean"}
        LOD 300
        
        CGPROGRAM
        #pragma surface surf StandardSpecular addshadow fullforwardshadows vertex:disp tessellate:tessDistance nolightmap
        #pragma target 4.6
        #include "Tessellation.cginc"

        struct appdata {
            float4 vertex : POSITION;
            float4 tangent : TANGENT;
            float3 normal : NORMAL;
            float2 texcoord : TEXCOORD0;
        };

        float _Tess;
        float _Size;

        float4 tessDistance (appdata v0, appdata v1, appdata v2) {
            float minDist = 10.0f;
            float maxDist = 25.0f;
            return 8 + UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, 0, _Size, _Tess);
        }

        sampler2D _DispTex;
        float _Displacement;

        void disp (inout appdata v)
        {
            float d = tex2Dlod(_DispTex, float4(v.texcoord.xy,0,0)).r * _Displacement;
            v.vertex.xyz = normalize(v.vertex.xyz);
        }

        struct Input {
            float2 uv_MainTex;
        };

        sampler2D _MainTex;
        sampler2D _NormalMap;
        fixed4 _Color;
        float _Smoothness;
        float _Specular;

        void surf (Input IN, inout SurfaceOutputStandardSpecular o) {
            half4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Specular = _SpecColor;
            o.Smoothness = _Smoothness;
            o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));
        }
        ENDCG
    }
}
