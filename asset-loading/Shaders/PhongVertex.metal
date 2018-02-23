#include <metal_stdlib>
using namespace metal;

struct VertexInput
{
    float3 Position[[attribute(0)]];
    float3 Normal[[attribute(1)]];
};

struct PixelInput
{
    float4 Position[[position]];
    float3 PositionView;
    float3 NormalView;
    float3 LightView;
};

struct ProjView 
{
    float4x4 View;
    float4x4 Proj;
};

struct Light {
    float3 position;
};

vertex PixelInput VS(VertexInput input[[stage_in]],constant ProjView &pj [[ buffer(1) ]],constant Light &l [[ buffer(2) ]])
{
    PixelInput output;
    float4x4 View = pj.View;
    float4 positionView = View*float4(input.Position, 1);
    float4 positionCS = pj.Proj*positionView;

    float4 lightView = View*float4(l.position,1);

    output.Position = positionCS;
    output.PositionView = positionView.xyz;
    output.NormalView = float3x3(View[0].xyz,View[1].xyz,View[2].xyz)*input.Normal;
    output.LightView = lightView.xyz;
    return output;
}