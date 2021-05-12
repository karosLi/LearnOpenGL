#version 330 core
/**
 点光源阴影
 
 上个教程我们学到了如何使用阴影映射技术创建动态阴影。效果不错，但它只适合定向光，因为阴影只是在单一定向光源下生成的。所以它也叫定向阴影映射，深度（阴影）贴图生成自定向光的视角。
 本节我们的焦点是在各种方向生成动态阴影。这个技术可以适用于点光源，生成所有方向上的阴影。
 
 点光源阴影，过去的名字是万向阴影贴图（omnidirectional shadow maps）技术。
 
 我们从光的透视图生成一个深度贴图，基于当前fragment位置来对深度贴图采样，然后用储存的深度值和每个fragment进行对比，看看它是否在阴影中。定向阴影映射和万向阴影映射的主要不同在于深度贴图的使用上。

 对于深度贴图，我们需要从一个点光源的所有渲染场景，普通2D深度贴图不能工作；如果我们使用立方体贴图会怎样？因为立方体贴图可以储存6个面的环境数据，它可以将整个场景渲染到立方体贴图的每个面上，把它们当作点光源四周的深度值来采样。
 https://learnopengl-cn.github.io/img/05/03/02/point_shadows_diagram.png
 生成后的深度立方体贴图被传递到光照像素着色器，它会用一个方向向量来采样立方体贴图，从而得到当前的fragment的深度（从光的透视图）
 
 
 阴影失真问题，使用阴影偏移/深度偏移解决
 阴影锯齿问题，使用 PCF 多次深度采样加权求出阴影值来解决
 
 https://learnopengl-cn.github.io/05%20Advanced%20Lighting/03%20Shadows/02%20Point%20Shadows/
 */

out vec4 FragColor;

in VS_OUT {
    vec3 FragPos;// 世界空间顶点位置
    vec3 Normal;// 世界空间法线向量
    vec2 TexCoords;//纹理坐标
} fs_in;

uniform sampler2D diffuseTexture;// 漫反射贴图
uniform samplerCube depthMap;// 深度立方体贴图

uniform vec3 lightPos;// 光源位置
uniform vec3 viewPos;// 摄像机方向

uniform float far_plane;// 视椎体元平面
uniform bool shadows;

float ShadowCalculation(vec3 fragPos)
{
    // get vector between fragment position and light position - 因为这个顶点着色器不再需要将他的位置向量变换到光空间，所以我们可以去掉FragPosLightSpace变量
    // 获取片段到光的方向向量
    // 我们得到了fragment的位置与光的位置之间的不同的向量，使用这个向量作为一个方向向量去对立方体贴图进行采样。
    // 方向向量不需要是单位向量，所以无需对它进行标准化。最后的closestDepth是光源和它最接近的可见fragment之间的标准化的深度值。
    vec3 lightToFrag = fragPos - lightPos;// 世界坐标系中片段向量减去点光源向量得到是点光源到片段的向量
    // ise the fragment to light vector to sample from the depth map - 获取深度立方体贴图中的深度值
    float closestDepth = texture(depthMap, lightToFrag).r;
    // it is currently in linear range between [0,1], let's re-transform it back to original depth value
    // closestDepth值现在在0到1的范围内了，所以我们先将其转换回0到far_plane的范围，这需要把他乘以far_plane：
    closestDepth *= far_plane;
    // now get current linear depth as the length between the fragment and light position
    // 获取当前fragment和光源之间的深度值，我们可以简单的使用fragToLight的长度来获取它，这取决于我们如何计算立方体贴图中的深度值：
    float currentDepth = length(lightToFrag);
    // test for shadows
    float bias = 0.05; // we use a much larger bias since depth is now in [near_plane, far_plane] range
    float shadow = currentDepth -  bias > closestDepth ? 1.0 : 0.0;
    
    // display closestDepth as debug (to visualize depth cubemap) - 一个简单的把深度缓冲显示出来的技巧是，在ShadowCalculation函数中计算标准化的closestDepth变量，把变量显示为：
    // 用于调试
    // FragColor = vec4(vec3(closestDepth / far_plane), 1.0);
    
    return shadow;
}

void main()
{
    // 片段着色器使用Blinn-Phong光照模型渲染场景
    vec3 color = texture(diffuseTexture, fs_in.TexCoords).rgb;
    vec3 normal = normalize(fs_in.Normal);
    vec3 lightColor = vec3(0.3);
    // ambient 环境光照分量 = 环境光照强度 * 材质环境光照反射分量颜色，为什么下面的计算又乘了一遍 材质环境光照反射分量颜色
//    vec3 ambient = 0.3 * color;
    // 所以这里我认为应该是如下：
    vec3 ambient = vec3(0.3);
    // diffuse
    vec3 lightDir = normalize(lightPos - fs_in.FragPos);
    float diff = max(dot(lightDir, normal), 0.0);
    vec3 diffuse = diff * lightColor;
    // specular
    vec3 viewDir = normalize(viewPos - fs_in.FragPos);
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = 0.0;
    vec3 halfwayDir = normalize(lightDir + viewDir);
    spec = pow(max(dot(normal, halfwayDir), 0.0), 64.0);
    vec3 specular = spec * lightColor;
    
    
    // calculate shadow - 计算阴影，计算出一个shadow值，当fragment在阴影中时是1.0，在阴影外是0.0
    float shadow = shadows ? ShadowCalculation(fs_in.FragPos) : 0.0;
    // 然后，diffuse和specular颜色会乘以这个阴影元素。由于阴影不会是全黑的（由于散射），我们把ambient分量从乘法中剔除。
    vec3 lighting = (ambient + (1.0 - shadow) * (diffuse + specular)) * color;
    
    FragColor = vec4(lighting, 1.0);
}
