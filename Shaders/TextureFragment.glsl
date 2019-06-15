#version 330 core

uniform sampler2D Texture;
uniform sampler2D ShadowMap;

in vec2 fsin_UV;
out vec4 fsout_Color;

void main()
{
    vec4 texturecolor = texture(Texture, fsin_UV);
    fsout_Color = texturecolor;
}
