Shader "Custom/S_MobileParticleAddNoFog" {
// Simplified Additive Particle shader. Differences from regular Additive Particle one:
// - no Tint color
// - no Smooth particle support
// - no AlphaTest
// - no ColorMask

    Properties {
        _MainTex ("Particle Texture", 2D) = "white" {}
    }

    Category {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha One
        Cull Off Lighting Off ZWrite Off  Fog { Mode Off }
    
        BindChannels {
            Bind "Color", color
            Bind "Vertex", vertex
            Bind "TexCoord", texcoord
        }
    
        SubShader {
            Pass {
                SetTexture [_MainTex] {
                    combine texture * primary
                }
            }
        }
    }
}