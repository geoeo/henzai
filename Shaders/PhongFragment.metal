#include <metal_stdlib>
using namespace metal;

struct PixelInput
{
    float4 Position[[position]];
    float3 FragWorld;
    float3 NormalWorld;
    float3 LightWorld;
    float3 CamPosWorld;
};

struct Material
{
    float4 Diffuse;
    float4 Specular;
    float4 Ambient;
    float4 Coefficients;
};

fragment float4 FS(PixelInput input[[stage_in]],constant Material &material [[buffer(2)]])
{
    float3 L = normalize(input.LightWorld-input.FragWorld);
    float l_dot_n = fmax(dot(L,input.NormalWorld),0.0);
    float4 diffuse = l_dot_n*material.Diffuse;

    float3 R = reflect(-L,input.NormalWorld);
    float3 V = normalize(input.CamPosWorld-input.FragWorld);
    float isDotFront = fmax(sign(dot(input.NormalWorld,L)),0.0);
    float spec = fmax(powr(isDotFront*dot(V,R),material.Coefficients.x),0.0);
    float4 specular = material.Specular*spec;

    float4 color_out = material.Ambient;
    color_out += diffuse;
    color_out += specular;
    //color_out = float4(input.NormalWorld,1.0);
    //color_out = float4(input.LightWorld,1.0);

    return color_out;
}