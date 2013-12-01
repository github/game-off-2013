Shader "tk2d/Depth Mask" {
    SubShader {
        Tags { "Queue" = "Transparent" }
        Lighting Off
        ZTest LEqual
        ZWrite On
        ColorMask 0
        Pass {}
    }
}