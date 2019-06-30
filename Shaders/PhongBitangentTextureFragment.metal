#include <metal_stdlib>
using namespace metal;

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

struct Light {
    float4 Position;
    float4 Color;
    float4 Attenuation;
};

struct SpotLight {
    float4 Position;
    float4 Color;
    float4 Direction;
    float4 Parameters;
};

struct Material
{
    float4 Diffuse;
    float4 Specular;
    float4 Ambient;
    float4 Coefficients;
};

float ShadowCalculation(float4 fragPosLightSpace, float l_dot_n, texture2d<float> shadowMapTexture, sampler shadowMapSampler)
{
    // perform perspective divide
    float3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = projCoords*0.5 +0.5;
    // we are sampling textures so we have to sample the center
    // get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)
    float closestDepth = shadowMapTexture.sample(shadowMapSampler, float2(projCoords.x, 1.0 - projCoords.y)).r; 
    // get depth of current fragment from light's perspective
    float currentDepth = projCoords.z;
    float bias = fmax(0.005 * (1.0 - l_dot_n), 0.005);  
    // check whether current frag pos is in shadow
    float shadow = currentDepth - bias > closestDepth  ? 1.0 : 0.0;  

    return shadow;
} 

fragment float4 FS(PixelInput input[[stage_in]],
                   constant Light &light [[buffer(1)]],
                   constant SpotLight &spotlight [[buffer(2)]],
                   constant Material &material [[buffer(3)]],
                   texture2d<float> diffuseTexture [[texture(0)]],
                   texture2d<float> normalTexture [[texture(1)]],
                   texture2d<float> shadowMapTexture [[texture(2)]],
                   sampler diffuseSampler [[sampler(0)]],
                   sampler normalSampler [[sampler(1)]],
                   sampler shadowMapSampler [[sampler(2)]])
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
    float attenuation;
    if(light.Position.w ==1.0){
        L = light.Position.xyz-input.FragWorld;
        float distance = length(L);
        attenuation = 1.0 / (light.Attenuation.x + distance*light.Attenuation.y + distance*distance*light.Attenuation.z);
    }
    else {
        L = -light.Position.xyz;
        attenuation = 1.0;
    }

    float4 pl_color = float4(0.0f);
    if(spotlight.Parameters.w == 1.0f){
        float3 lightDir = input.FragWorld-spotlight.Position.xyz;
        float distance = length(lightDir);
        float theta = dot(normalize(lightDir),normalize(spotlight.Direction.xyz));
        float epsilon = spotlight.Parameters.y - spotlight.Parameters.x;
        float intensity = clamp((theta - spotlight.Parameters.x) / epsilon, 0.0, 1.0);
        float pl_attenuation = 1.0 / (1.0 +spotlight.Direction.w *distance );
        pl_color = spotlight.Color*pl_attenuation*intensity;
    }

    L = normalize(L);
    float l_dot_n = fmax(dot(L,normal_sample),0.0);
    float4 diffuse = l_dot_n*material.Diffuse*diffuseTextureSample;

    float3 R = reflect(-L,normal_sample);
    float3 V = normalize(input.CamPosWorld - input.FragWorld);
    float isDotFront = fmax(sign(dot(normal_sample,L)),0.0);
    float spec = fmax(powr(isDotFront*dot(V,R),material.Coefficients.x),0.0);
    float4 specular = material.Specular*spec;

    float shadow = ShadowCalculation(input.LightFrag, l_dot_n, shadowMapTexture, shadowMapSampler);

    float4 color_out = float4(0.0,0.0,0.0,0.0);
    color_out += material.Ambient;
    color_out += attenuation*diffuse;
    color_out += attenuation*specular;
    color_out += attenuation*lightColor;
    color_out += pl_color;
    color_out *= (1.0 - shadow);

    //color_out = float4(input.NormalWorld,1.0);
    //color_out = float4(normal_sample,1.0);
    //color_out = float4(input.FragWorld,1.0);
    //color_out = float4(input.LightWorld,1.0);
    //color_out = diffuseTextureSample;
    //color_out = float4(normalTextureSample,1.0);
    //color_out = float4(l_dot_n,l_dot_n,l_dot_n,1.0);
    //color_out = float4(L.z,0.0,0.0,1.0);

    float gamma = 2.2;
    color_out.rgb = powr(color_out.rgb,float3(1.0/gamma));

    return color_out;
}