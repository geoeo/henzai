#version 450
#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable

in vec4 fsin_Color;
out vec4 fsout_Color;

void main()
{
    fsout_Color = fsin_Color;
}
