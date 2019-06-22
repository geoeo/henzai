#include <metal_stdlib>
using namespace metal;

struct PixelInput
{
    float4 Position[[position]];
};

// Passthrough for depthss
fragment float4 FS(PixelInput input[[stage_in]])
{

    return float4(0,0,0,0);
}