#version 330 core
out vec4 FragColor;

in vec3 Normal;// 法向量
in vec3 Position;// 片段，即顶点在世界空间中的位置，因为摄像机也在世界空间（往后z轴偏移了3，发生了模型变换，就表示摄像机处在了世界空间），只有在同一个空间才能做向量计算

uniform vec3 cameraPos;// 摄像机位置
uniform samplerCube skybox;// 天空盒子纹理

void main()
{
    
    /// 反射 - 想象3D模型可以反射周围环境
//    // 向量减法，求出摄像机反向向量，从摄像机到被观察到片段位置
//    vec3 I = normalize(Position - cameraPos);
//    // 求出反射向量，即天空盒子纹理的方向向量
//    vec3 R = reflect(I, normalize(Normal));
//    // 根据天空盒子纹理的方向向量进行纹理采样
//    FragColor = vec4(texture(skybox, R).rgb, 1.0);
    
    
    /// 折射 - 想象3D模型是玻璃制作的
    float ratio = 1.00 / 1.52;
    // 向量减法，求出摄像机反向向量，从摄像机到被观察到片段位置
    vec3 I = normalize(Position - cameraPos);
    // 求出折射向量，即天空盒子纹理的方向向量
    vec3 R = refract(I, normalize(Normal), ratio);
    // 根据天空盒子纹理的方向向量进行纹理采样
    FragColor = vec4(texture(skybox, R).rgb, 1.0);
}
