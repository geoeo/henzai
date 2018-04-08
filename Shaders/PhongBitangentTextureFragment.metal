#include <metal_stdlib>
using namespace metal;

struct PixelInput
{
    float4 Position[[position]];
    float3 FragWorld;
    float3 NormalWorld;
    float3 TangentWorld;
    float3 BitangentWorld;
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

struct Light {
    float4 Position;
    float4 Color;
    float4 Attenuation;
};

fragment float4 FS(PixelInput input[[stage_in]],
                   constant Light &light [[buffer(1)]],
                   constant Material &material [[buffer(2)]],
                   texture2d<float> diffuseTexture [[texture(0)]],
                   texture2d<float> normalTexture [[texture(1)]],
                   sampler diffuseSampler [[sampler(0)]],
                   sampler normalSampler [[sampler(1)]])
{
    float4 lightColor = light.Color.w*float4(light.Color.x,light.Color.y,light.Color.z,1.0);

    float4 diffuseTextureSample = diffuseTexture.sample(diffuseSampler,input.UV);
    float3 normalTextureSample = normalTexture.sample(normalSampler,input.UV).xyz;

    //float3 normal_sample = input.NormalWorld;
    float3 normal_sample = normalize(2.0*normalTextureSample-1.0);

    float3 Normal = normalize(input.NormalWorld);
    float3 Tangent = normalize(input.TangentWorld);
    float3 Bitangent = normalize(input.BitangentWorld);
    float3x3 TBN = float3x3(Tangent, Bitangent, Normal);

    //normal_sample = Normal;
    normal_sample = normalize(TBN*normal_sample);


    float3 L; 
    if(light.Position.z ==1.0){
        L = normalize(light.Position.xyz-input.FragWorld);
    }
    else {
        L = -light.Position.xyz
    }
    float distance = length(L);
    float attenuation = 1.0 / (light.Attenuation.x + distance*light.Attenuation.y + distance*distance*light.Attenuation.z);

    float l_dot_n = fmax(dot(L,normal_sample),0.0);
    float4 diffuse = l_dot_n*material.Diffuse*diffuseTextureSample;

    float3 R = reflect(-L,normal_sample);
    float3 V = normalize(input.CamPosWorld - input.FragWorld);
    float isDotFront = fmax(sign(dot(normal_sample,L)),0.0);
    float spec = fmax(powr(isDotFront*dot(V,R),material.Coefficients.x),0.0);
    float4 specular = material.Specular*spec;

    float4 color_out = float4(0.0,0.0,0.0,0.0);
    color_out += material.Ambient;
    color_out += diffuse;
    color_out += specular;
    color_out *= (attenuation*lightColor);
    //color_out = float4(input.NormalWorld,1.0);
    //color_out = float4(normal_sample,1.0);
    //color_out = float4(input.FragWorld,1.0);
    //color_out = float4(input.LightWorld,1.0);
    //color_out = diffuseTextureSample;
    //color_out = float4(normalTextureSample,1.0);
    //color_out = float4(l_dot_n,l_dot_n,l_dot_n,1.0);
    //color_out = float4(L.z,0.0,0.0,1.0);

    return color_out;
}