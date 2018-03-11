Shader "Custom/Circle" {
      Properties {
          _Color ("Color", Color) = (1,0,0,0)
          _Thickness("Thickness", Range(0.0, 200)) = 0.1
          _Center("Center", Vector) = (0 ,0, 0, 0)
          _Radius("Radius", Range(0, 800)) = 0.4
          _Dropoff("Dropoff", Range(0.01, 2)) = 0.1
      }
      SubShader {
          Pass {
              Blend SrcAlpha OneMinusSrcAlpha // Alpha blending
CGPROGRAM
             
              #pragma vertex vert
              #pragma fragment frag
              #include "UnityCG.cginc"
             
             
              fixed4 _Color; // low precision type is usually enough for colors
             float _Thickness;
             float _Radius;
             float _Dropoff;
             float4 _Center;
             
             // must match reference canvas size
             static const float2 ScreenSize = float2(800.0, 600.0);
             
              struct fragmentInput {
                  float4 pos : SV_POSITION;
                  float2 uv : TEXTCOORD0;
              };
  
              fragmentInput vert (appdata_base v)
              {
                  fragmentInput o;
  
                  o.pos = UnityObjectToClipPos (v.vertex);
                  o.uv = v.texcoord.xy - fixed2(0.5,0.5);
  
                  return o;
              }
  
              // r = radius
              // d = distance
              // t = thickness
              // p = % thickness used for dropoff
              float antialias(float r, float d, float t, float p) {
                  if( d < (r - 0.5*t))
                     return - pow( d - r + 0.5*t,2)/ pow(p*t, 2) + 1.0;
                 else if ( d > (r + 0.5*t))
                     return - pow( d - r - 0.5*t,2)/ pow(p*t, 2) + 1.0; 
                 else
                     return 1.0;
              }
              
              fixed4 frag(fragmentInput i) : SV_Target {
                    float2 pos = i.pos.xy - _Center.xy / ScreenSize * _ScreenParams.xy;
                 float distance = sqrt(pow(pos.x, 2) + pow(pos.y,2));
                 float t = _Thickness / ScreenSize.y * _ScreenParams.y;
                 float r = _Radius / ScreenSize.y * _ScreenParams.y;
                     
                 return fixed4(_Color.r, _Color.g, _Color.b, _Color.a*antialias(r, distance, t, _Dropoff));
              }
              
              
              ENDCG
          }
      }
  }