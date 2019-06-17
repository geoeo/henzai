#include <metal_stdlib>
using namespace metal;

struct VertexInput
{
    float3 Position[[attribute(0)]];
    float3 Normal[[attribute(1)]];
    float2 UV[[attribute(2)]];
    float3 Tangent[[attribute(3)]];
    float3 Bitangent[[attribute(4)]];
    float3 Offset[[attribute(5)]];
};

struct PixelInput
{
    float4 Position[[position]];
    float3 FragWorld;
    float3 NormalWorld;
    float3 TangentWorld;
    float3 BitangentWorld;
    float4 LightFrag;
    float2 UV;
    float3 CamPosWorld;
};

struct ProjViewWorld 
{
    float4x4 View;
    float4x4 Proj;
    float4x4 World;
};

struct LightProjView
{
    float4x4 LightProjView;
};


vertex PixelInput VS(VertexInput input[[stage_in]],constant ProjViewWorld &pjw [[buffer(0)]], constant LightProjView &lightpv [[buffer(4)]])
{
    PixelInput output;
    float3 position = input.Position;
    position.x += input.Offset.x;
    position.y += input.Offset.y;
    position.z += input.Offset.z;
    float4 positionWorld = pjw.World*float4(position, 1);
    float4 positionCS = pjw.Proj*pjw.View*positionWorld;

    float3x3 normalMatrix =  float3x3(pjw.World[0].xyz,pjw.World[1].xyz,pjw.World[2].xyz);

    output.Position = positionCS;
    output.FragWorld = positionWorld.xyz;
    output.NormalWorld = normalMatrix*input.Normal;
    output.TangentWorld = normalMatrix*input.Tangent;
    output.BitangentWorld = normalMatrix*input.Bitangent;
    output.UV = input.UV;
    output.CamPosWorld = pjw.View[3].xyz;
    output.LightFrag = lightpv.LightProjView*positionWorld;
    return output;
}