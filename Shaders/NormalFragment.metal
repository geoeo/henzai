#include <metal_stdlib>
using namespace metal;

struct PixelInput
{
    float4 Position[[position]];
    float3 Normal;
};

fragment float4 FS(PixelInput input[[stage_in]])
{
    return float4(input.Normal,1.0);
}