#include <metal_stdlib>
using namespace metal;

struct PixelInput
{
    float4 Position[[position]];
    float3 PositionView;
    float3 NormalView;
    float3 LightView;
};

struct Material
{
    float4 diffuse;
    float4 specular;
    float4 ambient;
};

fragment float4 FS(PixelInput input[[stage_in]],constant Material &material [[buffer(2)]])
{
    return float4(input.NormalView,1.0);
}