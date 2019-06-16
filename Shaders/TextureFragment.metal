#include <metal_stdlib>
using namespace metal;

struct PixelInput
{
    float4 Position[[position]];
    float4 LightFrag;
    float2 UV;
};

fragment float4 FS(PixelInput input[[stage_in]],texture2d<float> tex [[texture(0)]],texture2d<float> shadowMapTexture [[texture(1)]], sampler sam [[sampler(0)]], sampler shadowMapSampler [[sampler(1)]])
{
    return tex.sample(sam,input.UV);
}