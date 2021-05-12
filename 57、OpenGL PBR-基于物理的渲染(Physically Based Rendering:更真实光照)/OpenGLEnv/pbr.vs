#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoords;
layout (location = 2) in vec3 aNormal;

out vec2 TexCoords;
out vec3 WorldPos;
out vec3 Normal;

uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;

void main()
{
    TexCoords = aTexCoords;
    WorldPos = vec3(model * vec4(aPos, 1.0));
    /**
     标准做法：
     mat4 NormalMatrix = transpose(inverse(model));
     但是main.cpp里并没有做旋转，所以法线在世界空间中和顶点还是垂直的
     */
    Normal = mat3(model) * aNormal;

    gl_Position =  projection * view * vec4(WorldPos, 1.0);
}
