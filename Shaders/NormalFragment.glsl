#version 330 core

in vec3 fsin_Normal;
out vec4 fsout_Color;

void main()
{
    fsout_Color = vec4(fsin_Normal,1.0);
}
