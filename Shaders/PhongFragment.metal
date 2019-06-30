#include <metal_stdlib>
using namespace metal;

struct PixelInput
{
    float4 Position[[position]];
    float4 LightFrag;
    float3 FragWorld;
    float3 NormalWorld;
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

fragment float4 FS(PixelInput input[[stage_in]],constant Light &light [[buffer(1)]], constant Material &material [[buffer(2)]], texture2d<float> shadowMapTexture [[texture(0)]], sampler shadowMapSampler [[sampler(0)]])
{
    float4 lightColor = light.Color.w*float4(light.Color.x,light.Color.y,light.Color.z,1.0);
    
    float3 L = light.Position.xyz-input.FragWorld;
    float distance = length(L);
    float attenuation = 1.0 / (light.Attenuation.x + distance*light.Attenuation.y + distance*distance*light.Attenuation.z);

    L = normalize(L);
    float l_dot_n = fmax(dot(L,normalize(input.NormalWorld)),0.0);
    float4 diffuse = l_dot_n*material.Diffuse;

    float3 R = reflect(-L,input.NormalWorld);
    float3 V = normalize(input.CamPosWorld-input.FragWorld);
    float isDotFront = fmax(sign(dot(input.NormalWorld,L)),0.0);
    float spec = fmax(powr(isDotFront*dot(V,R),material.Coefficients.x),0.0);
    float4 specular = material.Specular*spec;

    float shadow = ShadowCalculation(input.LightFrag, l_dot_n, shadowMapTexture, shadowMapSampler);

    float4 color_out = material.Ambient;
    color_out += attenuation*diffuse;
    color_out += attenuation*specular;
    color_out += attenuation*lightColor;
    color_out *= (1.0 - shadow);
    //color_out += diffuse;
    //color_out += specular;
    //color_out += lightColor;
    //color_out = float4(input.NormalWorld,1.0);
    //color_out = float4(light.Position.xyz,1.0);
    //color_out = float4(1.0,0.0,0.0,1.0);
    //color_out = float4(shadow,shadow,shadow,1.0);

    float gamma = 2.2;
    color_out.rgb = powr(color_out.rgb,float3(1.0/gamma));

    return color_out;
}