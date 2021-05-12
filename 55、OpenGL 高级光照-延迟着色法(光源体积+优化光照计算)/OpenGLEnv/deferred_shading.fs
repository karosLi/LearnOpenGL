#version 330 core
/**
 延迟着色法-2、光照处理阶段
  
 现在我们已经有了一大堆的片段数据储存在G缓冲中供我们处置，我们可以选择通过一个像素一个像素地遍历各个G缓冲纹理，并将储存在它们里面的内容作为光照算法的输入，来完全计算场景最终的光照颜色。由于所有的G缓冲纹理都代表的是最终变换的片段值，我们只需要对每一个像素执行一次昂贵的光照运算就行了。这使得延迟光照非常高效，特别是在需要调用大量重型片段着色器的复杂场景中。

 对于这个光照处理阶段，我们将会渲染一个2D全屏的方形(有一点像后期处理效果)并且在每个像素上运行一个昂贵的光照片段着色器。
 
 
 光源体积/半径
 限制每一个光源的光照范围，减少计算量，优化光照性能
 
 延迟渲染一直被称赞的原因就是它能够渲染大量的光源而不消耗大量的性能。然而，延迟渲染它本身并不能支持非常大量的光源，因为我们仍然必须要对场景中每一个光源计算每一个片段的光照分量。真正让大量光源成为可能的是我们能够对延迟渲染管线引用的一个非常棒的优化：光体积(Light Volumes)

 通常情况下，当我们渲染一个复杂光照场景下的片段着色器时，我们会计算场景中每一个光源的贡献，不管它们离这个片段有多远。很大一部分的光源根本就不会到达这个片段，所以为什么我们还要浪费这么多光照运算呢？

 隐藏在光体积背后的想法就是计算光源的半径，或是体积，也就是光能够到达片段的范围。由于大部分光源都使用了某种形式的衰减(Attenuation)，我们可以用它来计算光源能够到达的最大路程，或者说是半径。我们接下来只需要对那些在一个或多个光体积内的片段进行繁重的光照运算就行了。这可以给我们省下来很可观的计算量，因为我们现在只在需要的情况下计算光照。

 这个方法的难点基本就是找出一个光源光体积的大小，或者是半径。
 
 计算一个光源的体积或半径
 为了获取一个光源的体积半径，我们需要解一个对于一个我们认为是黑暗(Dark)的亮度(Brightness)的衰减方程，它可以是0.0，或者是更亮一点的但仍被认为黑暗的值，像是0.03。为了展示我们如何计算光源的体积半径，我们将会使用一个在投光物这节中引入的一个更加复杂，但非常灵活的衰减方程。
 我们现在想要在Flight等于0的前提下解这个方程，也就是说光在该距离完全是黑暗的。然而这个方程永远不会真正等于0.0，所以它没有解。所以，我们不会求表达式等于0.0时候的解，相反我们会求当亮度值靠近于0.0的解，这时候它还是能被看做是黑暗的。在这个教程的演示场景中，我们选择5/256作为一个合适的光照值；除以256是因为默认的8-bit帧缓冲可以每个分量显示这么多强度值(Intensity)。
 
 
 求一个接近 0.0 的解，选择5/256作为一个合适的光照值；除以256是因为默认的8-bit帧缓冲可以每个分量显示这么多强度值(Intensity)。
 
 真正使用光体积
 下面那个片段着色器在实际情况下不能真正地工作，并且它只演示了我们可以不知怎样能使用光体积减少光照运算。然而事实上，你的GPU和GLSL并不擅长优化循环和分支。这一缺陷的原因是GPU中着色器的运行是高度并行的，大部分的架构要求对于一个大的线程集合，GPU需要对它运行完全一样的着色器代码从而获得高效率。这通常意味着一个着色器运行时总是执行一个if语句所有的分支从而保证着色器运行都是一样的，这使得我们之前的半径检测优化完全变得无用，我们仍然在对所有光源计算光照！

 使用光体积更好的方法是渲染一个实际的球体，并根据光体积的半径缩放。这些球的中心放置在光源的位置，由于它是根据光体积半径缩放的，这个球体正好覆盖了光的可视体积。这就是我们的技巧：我们使用大体相同的延迟片段着色器来渲染球体。因为球体产生了完全匹配于受影响像素的着色器调用，我们只渲染了受影响的像素而跳过其它的像素。下面这幅图展示了这一技巧：
 https://learnopengl-cn.github.io/img/05/08/deferred_light_volume_rendered.png
 
 
 延迟渲染 vs 正向渲染
 仅仅是延迟着色法它本身(没有光体积)已经是一个很大的优化了，每个像素仅仅运行一个单独的片段着色器，然而对于正向渲染，我们通常会对一个像素运行多次片段着色器。当然，
 延迟渲染确实带来一些缺点：大内存开销，没有MSAA和混合(仍需要正向渲染的配合)。

 当你有一个很小的场景并且没有很多的光源时候，延迟渲染并不一定会更快一点，甚至有些时候由于开销超过了它的优点还会更慢。然而在一个更复杂的场景中，延迟渲染会快速变成一个重要的优化，特别是有了更先进的优化拓展的时候。

 最后我仍然想指出，基本上所有能通过正向渲染完成的效果能够同样在延迟渲染场景中实现，这通常需要一些小的翻译步骤。举个例子，如果我们想要在延迟渲染器中使用法线贴图(Normal Mapping)，我们需要改变几何渲染阶段着色器来输出一个世界空间法线(World-space Normal)，它从法线贴图中提取出来(使用一个TBN矩阵)而不是表面法线，光照渲染阶段中的光照运算一点都不需要变。如果你想要让视差贴图工作，首先你需要在采样一个物体的漫反射，镜面，和法线纹理之前首先置换几何渲染阶段中的纹理坐标。一旦你了解了延迟渲染背后的理念，变得有创造力并不是什么难事。


 
 
 
 
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
    // 光源光照半径
    float Radius;
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
        // calculate distance between light source and current fragment - 如果片段到光源距离超出光源光照半径，就不进行光照计算
        float distance = length(lights[i].Position - FragPos);
        if(distance < lights[i].Radius)
        {
            // diffuse
            vec3 lightDir = normalize(lights[i].Position - FragPos);
            vec3 diffuse = max(dot(Normal, lightDir), 0.0) * Diffuse * lights[i].Color;
            // specular
            vec3 halfwayDir = normalize(lightDir + viewDir);
            float spec = pow(max(dot(Normal, halfwayDir), 0.0), 16.0);
            vec3 specular = lights[i].Color * spec * Specular;
            // attenuation
            float attenuation = 1.0 / (1.0 + lights[i].Linear * distance + lights[i].Quadratic * distance * distance);
            diffuse *= attenuation;
            specular *= attenuation;
            lighting += diffuse + specular;
        }
    }
    FragColor = vec4(lighting, 1.0);
}
