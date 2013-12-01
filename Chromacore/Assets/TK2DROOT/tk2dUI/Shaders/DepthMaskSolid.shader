Shader "tk2d/Depth Mask (Solid)" {
    SubShader {
        Tags { "Queue" = "Geometry-10" }
        Lighting Off
        ZTest LEqual
        ZWrite On
        ColorMask 0
        Pass {}
    }
}