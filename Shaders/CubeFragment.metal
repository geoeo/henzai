#include <metal_stdlib>
using namespace metal;

struct PixelInput
{
    float4 Position[[position]];
    float2 UV;
};

fragment float4 FS(PixelInput input[[stage_in]],texture2d<float> tex [[texture(0)]],sampler sam [[sampler(0)]])
{
    return tex.sample(sam,input.UV);
}