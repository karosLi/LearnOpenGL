#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;

out vec3 Normal;
out vec3 Position;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    // 模型矩阵逆矩阵，在矩阵转置得到法向量矩阵，法向量矩阵与原始法向量相乘 得到变换法向量
    Normal = mat3(transpose(inverse(model))) * aNormal;
    
    // 顶点在世界空间中的位置
    Position = vec3(model * vec4(aPos, 1.0));
    
    // 裁剪空间的点
    gl_Position = projection * view * model * vec4(aPos, 1.0);
}
