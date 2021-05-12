#version 330 core
/**
 延迟着色法-2、光照处理阶段
  
 现在我们已经有了一大堆的片段数据储存在G缓冲中供我们处置，我们可以选择通过一个像素一个像素地遍历各个G缓冲纹理，并将储存在它们里面的内容作为光照算法的输入，来完全计算场景最终的光照颜色。由于所有的G缓冲纹理都代表的是最终变换的片段值，我们只需要对每一个像素执行一次昂贵的光照运算就行了。这使得延迟光照非常高效，特别是在需要调用大量重型片段着色器的复杂场景中。

 对于这个光照处理阶段，我们将会渲染一个2D全屏的方形(有一点像后期处理效果)并且在每个像素上运行一个昂贵的光照片段着色器。
 
 
 https://learnopengl-cn.github.io/05%20Advanced%20Lighting/08%20Deferred%20Shading/
 */
out vec4 FragColor;

in vec2 TexCoords;

// 几何阶段（g_buffer着色器）的输出，会输入到光照阶段着色器中来
uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D gAlbedoSpec;

struct Light {
    vec3 Position;
    vec3 Color;
    
    // 光照衰减因子
    float Linear;
    float Quadratic;
};
// 32 个点光源
const int NR_LIGHTS = 32;
uniform Light lights[NR_LIGHTS];
uniform vec3 viewPos;

/**
 根据 布林-冯氏光照(Blinn-Phong Lighting) 计算每一个片段的光照结果
 */
void main()
{
    // retrieve data from gbuffer - 从G缓冲中获取数据
    vec3 FragPos = texture(gPosition, TexCoords).rgb;
    vec3 Normal = texture(gNormal, TexCoords).rgb;
    vec3 Diffuse = texture(gAlbedoSpec, TexCoords).rgb;//漫反射光照反射颜色分量
    float Specular = texture(gAlbedoSpec, TexCoords).a;//镜面光照反射颜色分量
    
    // then calculate lighting as usual -  然后和往常一样地计算光照
    vec3 lighting  = Diffuse * 0.1; // hard-coded ambient component - 硬编码环境光照反射颜色分量
    vec3 viewDir  = normalize(viewPos - FragPos);
    for(int i = 0; i < NR_LIGHTS; ++i)
    {
        // diffuse
        vec3 lightDir = normalize(lights[i].Position - FragPos);
        vec3 diffuse = max(dot(Normal, lightDir), 0.0) * Diffuse * lights[i].Color;
        // specular
        vec3 halfwayDir = normalize(lightDir + viewDir);
        float spec = pow(max(dot(Normal, halfwayDir), 0.0), 16.0);
        vec3 specular = lights[i].Color * spec * Specular;
        // attenuation
        float distance = length(lights[i].Position - FragPos);
        float attenuation = 1.0 / (1.0 + lights[i].Linear * distance + lights[i].Quadratic * distance * distance);
        diffuse *= attenuation;
        specular *= attenuation;
        lighting += diffuse + specular;
    }
    FragColor = vec4(lighting, 1.0);
}
