#include <metal_stdlib>
using namespace metal;

struct PixelInput
{
    float4 Position[[position]];
    float3 CubeMapCoords;
};

fragment float4 FS(PixelInput input[[stage_in]],texturecube<float> tex [[texture(0)]],sampler sam [[sampler(0)]])
{
    return tex.sample(sam,input.CubeMapCoords);
}