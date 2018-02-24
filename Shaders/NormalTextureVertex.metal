#include <metal_stdlib>
using namespace metal;

struct VertexInput
{
    float3 Position[[attribute(0)]];
    float3 Normal[[attribute(1)]];
    float2 UV[[attribute(2)]];
};

struct PixelInput
{
    float4 Position[[position]];
    float3 Normal;
};

struct projView {
    float4x4 View;
    float4x4 Proj;
};

vertex PixelInput VS(VertexInput input[[stage_in]],constant projView &pj [[ buffer(1) ]])
{
    PixelInput output;
    float3 position = input.Position;
    float4 positionCS = pj.Proj*pj.View*float4(position, 1);
    output.Position = positionCS;
    output.Normal = input.Normal;
    return output;
}