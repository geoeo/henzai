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
    float3 FragWorld;
    float3 NormalWorld;
    float3 LightWorld;
    float3 CamPosWorld;
};

struct ProjViewWorld 
{
    float4x4 View;
    float4x4 Proj;
    float4x4 World;
};

struct Light {
    float4 Position;
    float4 Color;
    float4 Attenuation;
};

vertex PixelInput VS(VertexInput input[[stage_in]],constant ProjViewWorld &pjw [[ buffer(1) ]],constant Light &l [[ buffer(2) ]])
{
    PixelInput output;
    float4 positionWorld = pjw.World*float4(input.Position, 1);
    float4 positionCS = pjw.Proj*pjw.View*positionWorld;
    
    float3x3 normalMatrix =  float3x3(pjw.World[0].xyz,pjw.World[1].xyz,pjw.World[2].xyz);

    output.Position = positionCS;
    output.FragWorld = positionWorld.xyz;
    output.NormalWorld = float3x3(pjw.View[0].xyz,pjw.View[1].xyz,pjw.View[2].xyz)*normalMatrix*input.Normal;
    output.LightWorld = l.Position.xyz;
    output.CamPosWorld = pjw.View[3].xyz;
    return output;
}