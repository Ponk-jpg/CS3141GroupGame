#ifndef MULTI_SHADOW_WARP_INCLUDED
#define MULTI_SHADOW_WARP_INCLUDED

uniform float4 _ShadowPositions[8];
uniform float _ShadowRadii[8];

void CalculateMultiShadows_float(
    float2 BaseUV,
    float2 ScreenUV,
    float Count,
    float GlobalSize, // NEW INPUT
    float MaxDarkness, // NEW INPUT
    float WarpStrength, // NEW INPUT
    out float2 WarpedUV,
    out float ShadowDarkness)
{
    WarpedUV = BaseUV;
    ShadowDarkness = 1.0;

    int totalEntities = clamp((int) Count, 0, 8);
    float2 totalDistortion = float2(0.0, 0.0);

    for (int e = 0; e < totalEntities; e++)
    {
        float2 toPixel = ScreenUV - _ShadowPositions[e].xy;
        toPixel.y *= 3.0; // Squash into floor ellipse

        float computedDistance = length(toPixel);
        float2 dir = (computedDistance > 0.0001) ? normalize(toPixel) : float2(0.0, 0.0);

        // 1. SIZE: Multiplied by GlobalSize slider
        float targetRadius = max(_ShadowRadii[e] * GlobalSize, 0.001);
        float force = saturate(1.0 - (computedDistance / targetRadius));
        force = smoothstep(0.0, 1.0, force);

        // 2. WARP STRENGTH: Multiplied by WarpStrength slider
        totalDistortion += dir * force * WarpStrength;

        // 3. DARKNESS: Blends down to your custom MaxDarkness slider value
        ShadowDarkness *= lerp(1.0, MaxDarkness, force);
    }

    WarpedUV = BaseUV + totalDistortion;
}

#endif