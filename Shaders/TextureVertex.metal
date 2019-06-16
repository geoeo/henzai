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
    float4 LightFrag;
    float2 UV;
};

struct transformPipeline {
    float4x4 View;
    float4x4 Proj;
    float4x4 World;
};

struct LightProjView
{
    float4x4 LightProjView;
};

vertex PixelInput VS(VertexInput input[[stage_in]],constant transformPipeline &pipeline [[ buffer(0) ]], constant LightProjView &lightpv [[buffer(1)]])
{
    PixelInput output;
    float3 position = input.Position;
    float4 positionWorld = pipeline.World*float4(position, 1);
    float4 positionCS = pipeline.Proj*pipeline.View*positionWorld;
    output.Position = positionCS;
    output.UV = input.UV;
    output.LightFrag = lightpv.LightProjView*positionWorld;
    return output;
}