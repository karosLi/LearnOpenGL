#version 330 core
/**
 SSAO + 简化版的延迟着色法（几何处理阶段）
 
 延迟着色法-1、几何处理阶段
 
 生成 G-Buffer
 */
layout (location = 0) out vec3 gPosition;// 一个位置向量，输出到颜色缓冲 0
layout (location = 1) out vec3 gNormal;// 一个法向量，输出到颜色缓冲 1
layout (location = 2) out vec4 gAlbedo;// 一个颜色向量，一个镜面强度值，一起输出到颜色缓冲 2

in vec2 TexCoords;
in vec3 FragPos;
in vec3 Normal;

/**
 线性深度，目前没有用到
 */
float LinearizeDepth(float depth)
{
    float z = depth * 2.0 - 1.0; // 回到NDC
    return (2.0 * NEAR * FAR) / (FAR + NEAR - z * (FAR - NEAR));
}

void main()
{
    // store the fragment position vector in the first gbuffer texture - 存储第一个G缓冲纹理中的片段位置向量
    gPosition = FragPos;
    // also store the per-fragment normals into the gbuffer - 存储对每个逐片段法线到G缓冲中
    gNormal = normalize(Normal);
    // and the diffuse per-fragment color - 存储漫反射对每个逐片段颜色到G缓冲中（没有存储镜面强度到gAlbedoSpec的alpha分量)
    gAlbedo.rgb = vec3(0.95);
}
