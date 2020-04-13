#version 330 core
in vec4 FragPos;

// This will be light position
layout(std140) uniform cameraInfo
{
    vec4 cameraPos;
    float near_plane;
    float far_plane;
};

void main()
{
    // get distance between fragment and light source
    float lightDistance = length(FragPos - cameraPos);
    
    // map to [0;1] range by dividing by far_plane
    lightDistance = lightDistance / far_plane;
    
    // write this as modified depth
    gl_FragDepth = lightDistance;
}  