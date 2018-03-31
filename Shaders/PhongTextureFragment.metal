#include <metal_stdlib>
using namespace metal;

struct PixelInput
{
    float4 Position[[position]];
    float3 FragWorld;
    float3 NormalWorld;
    float3 TangentWorld;
    float3 LightWorld;
    float2 UV;
    float3 CamPosWorld;
};

struct Material
{
    float4 Diffuse;
    float4 Specular;
    float4 Ambient;
    float4 Coefficients;
};

fragment float4 FS(PixelInput input[[stage_in]],
                   constant Material &material [[buffer(2)]],
                   texture2d<float> diffuseTexture [[texture(0)]],
                   texture2d<float> normalTexture [[texture(1)]],
                   sampler diffuseSampler [[sampler(0)]],
                   sampler normalSampler [[sampler(1)]])
{
    float4 diffuseTextureSample = diffuseTexture.sample(diffuseSampler,input.UV);
    float3 normalTextureSample = normalTexture.sample(normalSampler,input.UV).xyz;

    //float3 normal_sample = input.NormalWorld;
    float3 normal_sample = normalize(2.0*normalTextureSample-1.0);

    float3 Normal = normalize(input.NormalWorld);
    float3 Tangent = normalize(input.TangentWorld);
    float3 Bitangent = normalize(cross(Tangent, Normal));
    float3x3 TBN = float3x3(Tangent, Bitangent, Normal);

    normal_sample = normalize(TBN*normal_sample);


    float3 L = normalize(input.LightWorld-input.FragWorld);
    float l_dot_n = fmax(dot(L,normal_sample),0.0);
    float4 diffuse = l_dot_n*material.Diffuse*diffuseTextureSample;

    float3 R = reflect(-L,normal_sample);
    float3 V = normalize(input.CamPosWorld - input.FragWorld);
    float isDotFront = fmax(sign(dot(normal_sample,L)),0.0);
    float spec = fmax(powr(isDotFront*dot(V,R),material.Coefficients.x),0.0);
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