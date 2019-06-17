#include <metal_stdlib>
using namespace metal;

struct VertexInput
{
    float3 Position[[attribute(0)]];
    float3 Offset[[attribute(1)]];
};

struct PixelInput
{
    float4 Position[[position]];
};

struct ProjViewWorld 
{
    float4x4 View;
    float4x4 Proj;
    float4x4 World;
};



vertex PixelInput VS(VertexInput input[[stage_in]],constant ProjViewWorld &pjw [[ buffer(0) ]])
{
    PixelInput output;
    float3 position = input.Position;
    position.x += input.Offset.x;
    position.y += input.Offset.y;
    position.z += input.Offset.z;
    float4 positionWorld = pjw.World*float4(position, 1);
    float4 positionCS = pjw.Proj*pjw.View*positionWorld;


    output.Position = positionCS;
    return output;
}