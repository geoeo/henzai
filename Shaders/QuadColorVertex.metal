#include <metal_stdlib>
using namespace metal;

struct VertexInput
{
    float2 Position[[attribute(0)]];
    float4 Color[[attribute(1)]];
};

struct PixelInput
{
    float4 Position[[position]];
    float4 Color;
};

struct transformPipeline {
    float4x4 View;
    float4x4 Proj;
    float4x4 World;
};

vertex PixelInput VS(VertexInput input[[stage_in]],constant transformPipeline &pipeline [[ buffer(1) ]])
{
    PixelInput output;
    float4 positionCS = pipeline.Proj*pipeline.View*pipeline.World*float4(input.Position, 1, 1);
    output.Position = positionCS;
    float4 Color = input.Color;
    output.Color = Color;
    return output;
}