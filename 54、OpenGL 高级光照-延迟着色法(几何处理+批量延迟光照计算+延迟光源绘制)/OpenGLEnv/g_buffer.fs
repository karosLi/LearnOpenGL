#version 330 core

/**
 延迟着色法-1、几何处理阶段
 
 G缓冲
 G缓冲(G-buffer)是对所有用来储存光照相关的数据，并在最后的光照处理阶段中使用的所有纹理的总称。趁此机会，让我们顺便复习一下在正向渲染中照亮一个片段所需要的所有数据：

 1、一个3D位置向量来计算(插值)片段位置变量供lightDir和viewDir使用
 2、一个RGB漫反射颜色向量，也就是反照率(Albedo)
 3、一个3D法向量来判断平面的斜率
 4、一个镜面强度(Specular Intensity)浮点值
 5、所有光源的位置和颜色向量
 6、玩家或者观察者的位置向量
 有了这些(逐片段)变量的处置权，我们就能够计算我们很熟悉的(布林-)冯氏光照(Blinn-Phong Lighting)了。光源的位置，颜色，和玩家的观察位置可以通过uniform变量来设置，但是其它变量对于每个对象的片段都是不同的。如果我们能以某种方式传输完全相同的数据到最终的延迟光照处理阶段中，我们就能计算与之前相同的光照效果了，尽管我们只是在渲染一个2D方形的片段。
 OpenGL并没有限制我们能在纹理中能存储的东西，所以现在你应该清楚在一个或多个屏幕大小的纹理中储存所有逐片段数据并在之后光照处理阶段中使用的可行性了。因为G缓冲纹理将会和光照处理阶段中的2D方形一样大，我们会获得和正向渲染设置完全一样的片段数据，但在光照处理阶段这里是一对一映射。
 
 对于每一个片段我们需要储存的数据有：一个位置向量、一个法向量，一个颜色向量，一个镜面强度值。
 
 https://learnopengl-cn.github.io/05%20Advanced%20Lighting/08%20Deferred%20Shading/
 */
layout (location = 0) out vec3 gPosition;// 一个位置向量，输出到颜色缓冲 0
layout (location = 1) out vec3 gNormal;// 一个法向量，输出到颜色缓冲 1
layout (location = 2) out vec4 gAlbedoSpec;// 一个颜色向量，一个镜面强度值，一起输出到颜色缓冲 2
//输出：https://learnopengl-cn.github.io/img/05/08/deferred_g_buffer.png

in vec2 TexCoords;// 纹理坐标
in vec3 FragPos;// 世界空间片段位置
in vec3 Normal;// 世界空间法向量

/// 由 mesh 内绘制方法设置的
uniform sampler2D texture_diffuse1;// 漫反射纹理
uniform sampler2D texture_specular1;// 镜面反射纹理

void main()
{
    // store the fragment position vector in the first gbuffer texture - 存储第一个G缓冲纹理中的片段位置向量
    gPosition = FragPos;
    // also store the per-fragment normals into the gbuffer - 同样存储对每个逐片段法线到G缓冲中
    gNormal = normalize(Normal);
    // and the diffuse per-fragment color - 和漫反射对每个逐片段颜色
    gAlbedoSpec.rgb = texture(texture_diffuse1, TexCoords).rgb;
    // store specular intensity in gAlbedoSpec's alpha component - 存储镜面强度到gAlbedoSpec的alpha分量
    gAlbedoSpec.a = texture(texture_specular1, TexCoords).r;
}
