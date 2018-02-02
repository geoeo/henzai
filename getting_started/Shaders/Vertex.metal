#include <metal_stdlib>
using namespace metal;

struct VertexInput
{
    float2 Position[[attribute(0)]];
    float4 Color[[attribute(1)]];
    float xOff[[attribute(2)]];
};

struct PixelInput
{
    float4 Position[[position]];
    float4 Color;
};

struct projView {
    float4x4 View;
    float4x4 Proj;
};

vertex PixelInput VS(VertexInput input[[stage_in]],constant projView &pj [[ buffer(2) ]])
{
    PixelInput output;
    float2 position = input.Position;
    position.x += input.xOff;
    float4 positionCS = pj.Proj*pj.View*float4(position, 5, 1);
    output.Position = positionCS;
    //output.Position = float4(position, 1, 1);
    //float4 Color = float4(1,0,0,1);
    float4 Color = input.Color;
    //Color = pj.Proj;
    output.Color = Color;
    return output;
}