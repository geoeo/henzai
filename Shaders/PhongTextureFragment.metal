#include <metal_stdlib>
using namespace metal;

struct PixelInput
{
    float4 Position[[position]];
    float3 FragWorld;
    float3 NormalWorld;
    float3 LightWorld;
    float2 UV;
    float3 CamPosWorld;
};

struct Material
{
    float4 Diffuse;
    float4 Specular;
    float4 Ambient;
};

fragment float4 FS(PixelInput input[[stage_in]],
                   constant Material &material [[buffer(3)]],
                   texture2d<float> diffuseTexture [[texture(0)]],
                   texture2d<float> normalTexture [[texture(1)]],
                   sampler diffuseSampler [[sampler(0)]],
                   sampler normalSampler [[sampler(1)]])
{
    float4 diffuseTextureSample = diffuseTexture.sample(diffuseSampler,input.UV);
    float4 normalTextureSample = normalTexture.sample(normalSampler,input.UV);

    float3 L = normalize(input.LightWorld-input.FragWorld);
    float l_dot_n = dot(L,input.NormalWorld);
    float4 diffuse = l_dot_n*material.Diffuse;

    float3 R = reflect(-L,input.NormalWorld);
    float3 V = normalize(input.CamPosWorld - input.FragWorld);
    float isDotFront = fmax(sign(dot(input.NormalWorld,L)),0.0);
    float spec = powr(isDotFront*dot(V,R),32.0);
    float4 specular = material.Specular*spec;

    float4 color_out = material.Ambient;
    color_out += diffuse;
    color_out += specular;
    //color_out = float4(input.NormalWorld,1.0);
    //color_out = float4(input.LightWorld,1.0);
    //color_out = diffuseTextureSample;
    //color_out = normalTextureSample;

    return color_out;
}