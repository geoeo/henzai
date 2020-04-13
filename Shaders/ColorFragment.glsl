#version 330 core

in vec4 fsin_Color;
out vec4 fsout_Color;

//TODO add shadow information i.e. lightPos etc.

void main()
{
    float gamma = 2.2;
    fsout_Color = vec4(pow(fsin_Color.rgb, vec3(1.0/gamma)),fsin_Color.a);
}