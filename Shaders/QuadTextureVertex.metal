#include <metal_stdlib>
using namespace metal;

struct VertexInput
{
    float3 Position[[attribute(0)]];
    float2 UV[[attribute(1)]];
};

struct PixelInput
{
    float4 Position[[position]];
    float2 UV;
};

struct transformPipeline {
    float4x4 View;
    float4x4 Proj;
    float4x4 World;
};

vertex PixelInput VS(VertexInput input[[stage_in]],constant transformPipeline &pipeline [[ buffer(0) ]])
{
    PixelInput output;
    float4 positionCS = pipeline.Proj*pipeline.View*pipeline.World*float4(input.Position, 1);
    output.Position = positionCS;
    output.UV = input.UV;
    return output;
}