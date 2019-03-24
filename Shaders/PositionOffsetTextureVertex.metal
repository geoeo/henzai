#include <metal_stdlib>
using namespace metal;

struct VertexInput
{
    float3 Position[[attribute(0)]];
    float2 UV[[attribute(1)]];
    float3 Offset[[attribute(2)]];
};

struct PixelInput
{
    float4 Position[[position]];
    float2 UV;
};

struct projViewWorld {
    float4x4 View;
    float4x4 Proj;
    float4x4 World;
};

vertex PixelInput VS(VertexInput input[[stage_in]],constant projViewWorld &pipeline [[ buffer(2) ]])
{
    PixelInput output;
    float3 position = input.Position;
    position.x += input.Offset.x;
    position.y += input.Offset.y;
    position.z += input.Offset.z;
    float4 positionCS = pipeline.Proj*pipeline.View*pipeline.World*float4(position, 1);
    output.Position = positionCS;
    output.UV = input.UV;
    return output;
}