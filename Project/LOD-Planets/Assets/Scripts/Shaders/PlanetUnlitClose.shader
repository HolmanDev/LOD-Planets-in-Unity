Shader "Planet/Planet Unlit Close"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ClipDist("Far Clip Distance", Range(0,100000)) = 100000
    }
    SubShader
    {
        Tags {"LightMode"="ForwardBase" "Planet"="Unlit Terrain" "RenderType"="Opaque"}
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

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
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
            float _ClipDist;

            fixed4 frag (v2f i) : SV_Target
            {
                float mergeValue = clamp(i.dist / _ClipDist, 0, 1);
                fixed4 vertexColor = fixed4(GammaToLinearSpace(i.color.rgb),1);
                fixed4 textureColor = tex2D(_MainTex, i.uv.xy);
                fixed4 col = lerp(vertexColor, textureColor, mergeValue);
                
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
}