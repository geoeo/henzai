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
    float3 FragView;
    float3 NormalView;
    float3 LightView;
};

struct ProjViewWorld 
{
    float4x4 View;
    float4x4 Proj;
    float4x4 World;
};

struct Light {
    float3 Position;
};

struct NormalMatrix {
    float4x4 Matrix;
};

vertex PixelInput VS(VertexInput input[[stage_in]],constant ProjViewWorld &pjw [[ buffer(1) ]],constant Light &l [[ buffer(2) ]],constant NormalMatrix &m [[ buffer(3) ]])
{
    PixelInput output;
    float4 positionView = pjw.View*pjw.World*float4(input.Position, 1);
    float4 positionCS = pjw.Proj*positionView;
    
    float3x3 normalMatrix =  float3x3(m.Matrix[0].xyz,m.Matrix[1].xyz,m.Matrix[2].xyz);
    float4 lightView = pjw.View*float4(l.Position,1);

    output.Position = positionCS;
    output.FragView = positionView.xyz;
    output.NormalView = float3x3(pjw.View[0].xyz,pjw.View[1].xyz,pjw.View[2].xyz)*normalMatrix*input.Normal;
    output.LightView = lightView.xyz;
    return output;
}