#version 330 core

uniform sampler2D CubeTexture;

in vec2 fsin_UV;
out vec4 fsout_Color;

void main()
{
    vec4 texturecolor = texture(CubeTexture, fsin_UV);
    fsout_Color = texturecolor;
}
