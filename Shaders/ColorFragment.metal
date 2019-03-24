#include <metal_stdlib>
using namespace metal;

struct PixelInput
{
    float4 Position[[position]];
    float4 Color;
};

fragment float4 FS(PixelInput input[[stage_in]])
{
    float4 color_out = input.Color;
    float gamma = 2.2;
    color_out.rgb = powr(color_out.rgb,float3(1.0/gamma));
    return color_out;
}