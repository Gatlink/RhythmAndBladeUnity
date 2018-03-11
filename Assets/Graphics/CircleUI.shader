// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Custom/Circle UI"
{
    Properties
    {
        //[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _Thickness("Thickness", Range(0.0, 200)) = 0.1
        _Center("Center", Vector) = (0 ,0, 0, 0)
        _Radius("Radius", Range(0, 800)) = 0.4
        _Dropoff("Dropoff", Range(0.01, 2)) = 0.1

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                //float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                //float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            //fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            float _Thickness;
            float _Radius;
            float _Dropoff;
            float4 _Center;

            // must match reference canvas size
            static const float2 ScreenSize = float2(800.0, 600.0);
             
            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                //OUT.texcoord = v.texcoord;

                OUT.color = v.color * _Color;
                return OUT;
            }

            //sampler2D _MainTex;

            // r = radius
            // d = distance
            // t = thickness
            // p = % thickness used for dropoff
            float antialias(float r, float d, float t, float p) 
            {
                if( d < (r - 0.5*t))
                    return - pow( d - r + 0.5*t,2)/ pow(p*t, 2) + 1.0;
                else if ( d > (r + 0.5*t))
                    return - pow( d - r - 0.5*t,2)/ pow(p*t, 2) + 1.0; 
                else
                    return 1.0;
            }

            fixed4 circle(float2 position) 
            {
                float2 pos = position - _Center.xy;// / ScreenSize * _ScreenParams.xy;
                float distance = sqrt(pow(pos.x, 2) + pow(pos.y,2));
                float t = _Thickness / ScreenSize.y * _ScreenParams.y;
                float r = _Radius / ScreenSize.y * _ScreenParams.y;
                     
                return fixed4(1, 1, 1, antialias(r, distance, t, _Dropoff));
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                //half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                half4 color = circle(IN.worldPosition.xy) * IN.color;
                 
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
        ENDCG
        }
    }
}
