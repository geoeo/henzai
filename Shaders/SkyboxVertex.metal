#include <metal_stdlib>
using namespace metal;

struct VertexInput
{
    float3 Position[[attribute(0)]];
};

struct PixelInput
{
    float4 Position[[position]];
    float3 CubeMapCoords;
};

struct transformPipeline {
    float4x4 View;
    float4x4 Proj;
    float4x4 World;
};

vertex PixelInput VS(VertexInput input[[stage_in]],constant transformPipeline &pipeline [[ buffer(0) ]])
{
    PixelInput output;
    float4 position = float4(input.Position, 1);
    float4x4 View_No_Translation = float4x4(pipeline.View[0],pipeline.View[1],pipeline.View[2],float4(0,0,0,1));
    float4 positionCS = pipeline.Proj*View_No_Translation*pipeline.World*position;
    output.Position = positionCS.xyww;
    output.CubeMapCoords = position.xyz;
    return output;
}