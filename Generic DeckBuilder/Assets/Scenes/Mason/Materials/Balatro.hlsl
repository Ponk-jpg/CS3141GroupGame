#ifndef BALATRO_INCLUDED
#define BALATRO_INCLUDED

void BalatroBackground_float(float2 UV, float Time, float SpinAmount, float Contrast, out float3 OutColor)
{
    // Center the UV coordinates (-0.5 to 0.5)
    float2 uv = UV - 0.5;

    // Rotate the UV grid over time to create a swirling axis
    float angle = Time * -2.0;
    float cosA = cos(angle);
    float sinA = sin(angle);
    uv = float2(uv.x * cosA - uv.y * sinA, uv.x * sinA + uv.y * cosA);

    // The Balatro procedural math loop
    for (int i = 1; i < 6; i++)
    {
        float float_i = float(i);
        uv.x += 0.7 / float_i * sin(float_i * uv.y + Time + float_i * 0.13) + 0.5;
        uv.y += 0.7 / float_i * cos(float_i * uv.x + Time + float_i * 0.22) + 0.5;
    }

    // Calculate the procedural lighting structure based on length
    float lengthUV = length(uv * SpinAmount);
    float strength = sin(lengthUV * Contrast) * 0.5 + 0.5;

    OutColor = float3(strength, strength, strength);
}

#endif